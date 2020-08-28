**NOTE**: This project is obsolete. The same sender functionality was added
to the KlakNDI plugin. See the project page for details.

https://github.com/keijiro/KlakNDI


NDI Lite Sender Plugin
======================

![gif](https://i.imgur.com/nEYNkrV.gif)

This is an lightweight implementation of NDI™ plugin for Unity iOS and macOS.

This plugin has some limitation compared to the [full implementation] of the
plugin.

- It only supports sender functionalities.
- The camera capture mode is omitted. It has to be used with a render texture.

[NewTek NDI]: http://NDI.NewTek.com/
[full implementation]: https://github.com/keijiro/KlakNDI

System requirements
-------------------

- Unity 2018.1 or later
- NDI compatible iOS or Mac device

Also it needs the [NDI SDK] when building to iOS. The SDK should be installed
to `/NewTek NDI SDK`. You have to modify the search paths in [PbxModifier.cs]
when installed to a different location.

[NDI SDK]: https://www.newtek.com/ndi/sdk/
[PbxModifier.cs]: https://github.com/keijiro/NDILiteSenderPlugin/blob/master/Assets/Klak/NDILite/Editor/PbxModifier.cs
