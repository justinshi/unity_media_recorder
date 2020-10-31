# UnityMediaRecorder
Provides functionality for recording audio and video in-game in Unity.

## Usage
- Attach a `RecorderBehaviour` to a `GameObject` in your Unity scene.
- Optionally, attach a `Camera` and/or an `AudioListener` (`RecorderBehaviour` will automatically attach these if not found)
- In your own script, get a reference to the `RecorderBehaviour`
- To start a recording, call `StartRecording`, using an instance of `RecordingOptions` to specify parameters (video frame rate, audio bit rate, etc.)
- Call `StopRecording` to complete the recording and finish saving it to a file in your "My Videos" directory

See `RecorderExampleBehaviour.cs` and `RecorderExampleScene.unity` for an example.

## Dependencies
- Uses FFmpeg for audio/video processing and encoding (http://ffmpeg.org/)
- Uses FFmpeg.AutoGen for C#/.NET FFmpeg bindings (https://github.com/Ruslan-B/FFmpeg.AutoGen)
