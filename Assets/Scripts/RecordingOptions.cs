using System.Collections.Generic;
using UnityEngine;
using UnityMediaRecorder.FFmpegLibraryWrappers;

namespace UnityMediaRecorder {
  public class RecordingOptions {
    public readonly string name;
    public readonly VideoParams vParams;
    public readonly AudioParams aParams;

    public RecordingOptions(string name, VideoParams vParams = null, AudioParams aParams = null) {
      this.name = name;
      this.vParams = vParams;
      this.aParams = aParams;
    }
  }

  public class VideoParams {
    public readonly int frameRate;
    public readonly long bitRate;
    public readonly int width;
    public readonly int height;
    public readonly string filterGraphDesc;

    public VideoParams(int frameRate, long bitRate, int width, int height, string filterGraphDesc) {
      this.frameRate = frameRate;
      this.bitRate = bitRate;
      this.width = width;
      this.height = height;
      this.filterGraphDesc = filterGraphDesc;
    }
  }

  public class AudioParams {
    private static readonly Dictionary<AudioSpeakerMode, int> UNITY_SPEAKER_MODE_TO_FFMPEG_CHANNEL_LAYOUT_ =
      new Dictionary<AudioSpeakerMode, int> {
        { AudioSpeakerMode.Mono, LibavutilWrapper.AV_CH_LAYOUT_MONO },
        { AudioSpeakerMode.Stereo, LibavutilWrapper.AV_CH_LAYOUT_STEREO },
        { AudioSpeakerMode.Quad, LibavutilWrapper.AV_CH_LAYOUT_QUAD },
        { AudioSpeakerMode.Surround, LibavutilWrapper.AV_CH_LAYOUT_SURROUND },
        { AudioSpeakerMode.Mode5point1, LibavutilWrapper.AV_CH_LAYOUT_5POINT1 },
        { AudioSpeakerMode.Mode7point1, LibavutilWrapper.AV_CH_LAYOUT_7POINT1 },
        { AudioSpeakerMode.Prologic, LibavutilWrapper.AV_CH_LAYOUT_STEREO } // TODO: check if this is okay
      };

    public readonly int sampleRate;
    public readonly long bitRate;
    public readonly string filterGraphDesc;
    public readonly int channelLayout;

    public AudioParams(int sampleRate, long bitRate, string filterGraphDesc, AudioSpeakerMode speakerMode) {
      this.sampleRate = sampleRate;
      this.bitRate = bitRate;
      this.filterGraphDesc = filterGraphDesc;
      channelLayout = UNITY_SPEAKER_MODE_TO_FFMPEG_CHANNEL_LAYOUT_[speakerMode];
    }
  }
}
