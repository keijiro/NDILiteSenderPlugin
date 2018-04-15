using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Klak.NdiLite
{
    public class NdiSenderLite : MonoBehaviour
    {
        #region Source texture

        [SerializeField] RenderTexture _sourceTexture;

        public RenderTexture sourceTexture {
            get { return _sourceTexture; }
            set { _sourceTexture = value; }
        }

        #endregion

        #region Format option

        [SerializeField] bool _alphaSupport;

        public bool alphaSupport {
            get { return _alphaSupport; }
            set { _alphaSupport = value; }
        }

        #endregion

        #region Private members

        [SerializeField, HideInInspector] ComputeShader _compute;

        ComputeBuffer _conversionBuffer;
        int _sourceWidth;
        IntPtr _plugin;

        #endregion

        #region Plugin entry points

        public enum FourCC : uint
        {
            UYVY = 0x59565955,
            UYVA = 0x41565955
        }

        [DllImport("__Internal")]
        public static extern IntPtr NDI_CreateSender(string name);

        [DllImport("__Internal")]
        public static extern void NDI_DestroySender(IntPtr sender);

        [DllImport("__Internal")]
        public static extern void NDI_SendFrame(IntPtr sender, IntPtr data, int width, int height, FourCC fourCC);

        [DllImport("__Internal")]
        public static extern void NDI_SyncSender(IntPtr sender);

        #endregion

        #region MonoBehaviour implementation

        IEnumerator Start()
        {
            // Plugin initialization
            _plugin = NDI_CreateSender(gameObject.name);

            // Local variables only used in this coroutine.
            var wait = new WaitForEndOfFrame();
            var data = (Color32[])null;

            while (true)
            {
                // Wait for the end of the frame.
                yield return wait;

                // Do nothing if there is no conversion yet.
                if (_conversionBuffer == null) continue;

                // Wait for the plugin to complete the previous frame.
                NDI_SyncSender(_plugin);

                // (Re)allocate the data buffer if it can't be reused.
                if (data == null || data.Length != _conversionBuffer.count)
                    data = new Color32[_conversionBuffer.count];

                // Request compute buffer readback.
                _conversionBuffer.GetData(data);

                // Get the pointer to the data buffer, and give it to the plugin.
                var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
                NDI_SendFrame(
                    _plugin, gch.AddrOfPinnedObject(),
                    _sourceWidth, _conversionBuffer.count * 2 / _sourceWidth,
                    FourCC.UYVY
                );
                gch.Free();
            }
        }

        void OnDisable()
        {
            // Compute buffers should be disposed in OnDisable, not OnDestroy.
            if (_conversionBuffer != null)
            {
                _conversionBuffer.Dispose();
                _conversionBuffer = null;
            }
        }

        void OnDestroy()
        {
            NDI_DestroySender(_plugin);
        }

        void Update()
        {
            if (_sourceTexture == null) return;

            _sourceWidth = _sourceTexture.width;
            var size = _sourceWidth / 2 * _sourceTexture.height;

            // Dispose the conversion buffer if it can't be reused.
            if (_conversionBuffer != null && _conversionBuffer.count != size)
            {
                _conversionBuffer.Dispose();
                _conversionBuffer = null;
            }

            // Conversion buffer allocation
            if (_conversionBuffer == null)
                _conversionBuffer = new ComputeBuffer(size, 4);

            // Invoke the compute.
            _compute.SetTexture(0, "Source", _sourceTexture);
            _compute.SetBuffer(0, "Destination", _conversionBuffer);
            _compute.Dispatch(0, _sourceWidth / 16, _sourceTexture.height / 8, 1);
        }

        #endregion
    }
}
