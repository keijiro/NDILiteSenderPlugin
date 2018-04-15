NDI Lite Sender Plugin
======================

![gif](https://i.imgur.com/nEYNkrV.gif)

This is an lightweight implementation of NDI™ plugin for Unity iOS.

This plugin has some limitation compared to the [full implementation] of the
plugin.

- It only supports sender functionalities. It can't be a receiver.
- The implementation is not optimal because of lack of the GPU readback
  feature.
- The camera capture mode is dropped. It has to be used with a render texture.

[NewTek NDI]: http://NDI.NewTek.com/
[full implementation]: https://github.com/keijiro/KlakNDI

System requirements
-------------------

- Unity 2018.1 or later
- NewTek NDI SDK
- iOS devices supported by NDI SDK

The NDI SDK should be installed in `/NewTek NDI SDK`. You have to modify the 
search paths in [PbxModifier.cs] when the installation directory is changed.

[PbxModifier.cs]: https://github.com/keijiro/NDILiteSenderPlugin/blob/master/Assets/Klak/NDILite/Editor/PbxModifier.cs
