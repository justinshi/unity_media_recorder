# UnityMediaRecorder
Provides functionality for recording audio and video in-game in Unity.

## Usage
- Attach a `RecorderBehaviour` to a `GameObject` in your Unity scene.
- Optionally, attach a `Camera` and/or an `AudioListener` (`RecorderBehaviour` will automatically attach these if not found)
- In your own script, get a reference to the `RecorderBehaviour`
- To start a recording, call `StartRecording`, using an instance of `RecordingOptions` to specify parameters (video frame rate, audio bit rate, etc.)
- Call `StopRecording` to complete the recording and finish saving it to a file in your "My Videos" directory

See `Example.cs` and `Example.unity` for an example.

## Prerequisites/Dependencies
- Currently for Windows only, tested with Unity 2019.4.13f1 with API Compatibility Level .NET 4.x. You could try compiling FFmpeg for other platforms, with different configuration settings, and/or with external libraries. YMMV
- Uses FFmpeg (4.3.1, http://ffmpeg.org/) for audio/video processing and encoding.
- Uses FFmpeg.AutoGen (4.3.0.3, https://github.com/Ruslan-B/FFmpeg.AutoGen) for C#/.NET bindings of FFmpeg types.

## License
UnityMediaRecorder is available under the MIT license - see LICENSE for more details.

The bundled FFmpeg libraries are licensed under LGPL 2.1 or later, and have been cross-compiled for Windows with MinGW-w64 on Ubuntu, configured with the command `./configure --arch=x86_64 --target-os=mingw32 --cross-prefix=x86_64-w64-mingw32- --enable-shared`.

The bundled code from FFmpeg.AutoGen is licensed under LGPL 3.
