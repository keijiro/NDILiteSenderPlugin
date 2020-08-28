// NDILiteSenderPlugin - NDI send-only plugin for Unity
// https://github.com/keijiro/NDILiteSenderPlugin

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Klak.NdiLite
{
    public class NdiLiteSender : MonoBehaviour
    {
        #region Source texture

        [SerializeField] RenderTexture _sourceTexture = null;

        public RenderTexture sourceTexture {
            get { return _sourceTexture; }
            set { _sourceTexture = value; }
        }

        #endregion

        #region Format option

        [SerializeField] bool _alphaSupport = false;

        public bool alphaSupport {
            get { return _alphaSupport; }
            set { _alphaSupport = value; }
        }

        #endregion

        #region Private members

        [SerializeField, HideInInspector] ComputeShader _compute = null;

        Queue<ReadbackBuffer> _readbackQueue = new Queue<ReadbackBuffer>();
        ReadbackBuffer _sending;
        IntPtr _plugin;

        void DisposeBuffer(ReadbackBuffer buffer)
        {
            buffer.Source.Dispose();
            buffer.Dispose();
        }

        #endregion

        #region Plugin entry points

        public enum FourCC : uint
        {
            UYVY = 0x59565955,
            UYVA = 0x41565955
        }

        #if !UNITY_EDITOR && UNITY_IOS
        const string _dllName = "__Internal";
        #else
        const string _dllName = "NDILiteSender";
        #endif

        [DllImport(_dllName)]
        public static extern IntPtr NDI_CreateSender(string name);

        [DllImport(_dllName)]
        public static extern void NDI_DestroySender(IntPtr sender);

        [DllImport(_dllName)]
        public static extern void NDI_SendFrame(IntPtr sender, IntPtr data, int width, int height, FourCC fourCC);

        [DllImport(_dllName)]
        public static extern void NDI_SyncSender(IntPtr sender);

        #endregion

        #region MonoBehaviour implementation

        void OnDisable()
        {
            // Sync with the sender and dispose the frame.
            if (_sending != null)
            {
                NDI_SyncSender(_plugin);
                DisposeBuffer(_sending);
            }

            // Dispose all the readback requests in the queue.
            while (_readbackQueue.Count > 0)
                DisposeBuffer(_readbackQueue.Dequeue());
        }

        void Start()
        {
            _plugin = NDI_CreateSender(gameObject.name);
        }

        void OnDestroy()
        {
            NDI_DestroySender(_plugin);
        }

        unsafe void Update()
        {
            if (_sourceTexture == null || !_sourceTexture.IsCreated()) return;

            var w = _sourceTexture.width / 2;
            var h = _sourceTexture.height * (_alphaSupport ? 3 : 2) / 2;

            ReadbackBuffer buffer = null;

            if (_readbackQueue.Count > 0 && _readbackQueue.Peek().IsCompleted)
            {
                buffer = _sending;
                _sending = _readbackQueue.Dequeue();

                // Wait for the plugin to complete the previous frame.
                NDI_SyncSender(_plugin);

                // Get the pointer to the data buffer, and give it to the plugin.
                NDI_SendFrame(
                    _plugin, (IntPtr)_sending.Data.GetUnsafePtr(),
                    _sourceTexture.width, _sourceTexture.height,
                    _alphaSupport ? FourCC.UYVA : FourCC.UYVY
                );

                NDI_SyncSender(_plugin);

                // Dispose the buffer if it can't be reused.
                if (_sending.Source.count != w * h)
                {
                    NDI_SyncSender(_plugin); // Wait before disposing.
                    DisposeBuffer(_sending);
                    _sending = null;
                }
            }

            // Readback buffer allocation
            if (buffer == null)
                buffer = new ReadbackBuffer(new ComputeBuffer(w * h, 4));

            // Invoke the compute.
            _compute.SetTexture(0, "Source", _sourceTexture);
            _compute.SetBuffer(0, "Destination", buffer.Source);
            _compute.Dispatch(0, w / 8, _sourceTexture.height / 8, 1);

            if (_alphaSupport)
            {
                _compute.SetTexture(1, "Source", _sourceTexture);
                _compute.SetBuffer(1, "Destination", buffer.Source);
                _compute.Dispatch(1, w / 16, _sourceTexture.height / 8, 1);
            }

            // Push to the readback queue.
            buffer.RequestReadback();
            _readbackQueue.Enqueue(buffer);
        }

        #endregion
    }
}
