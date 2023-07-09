using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityMediaRecorder.FFmpegLibraryWrappers;

namespace UnityMediaRecorder.Utils {
  public class FFmpegException : Exception {
    public FFmpegException(string message) : base(message) { }
  }

  public static class FFmpegUtils {
    public static void CheckRet(int ret, string errMsg) {
      if (ret < 0) {
        throw new FFmpegException($"Error code: ({ret}) - {errMsg}");
      }
    }

    public static unsafe void CheckRet<T>(T* ptr, string errMsg) where T : unmanaged {
      if (ptr == null) {
        throw new FFmpegException($"Null pointer to variable of type: {typeof(T)} - {errMsg}");
      }
    }

    public static string GetFilenameExtension(bool video, bool audio) {
      if (video) {
        return ".mp4";
      }

      if (audio) {
        return ".aac";
      }

      throw new ArgumentException("Recording cannot have both video and audio disabled");
    }

    public static bool CheckFlag(long val, long flags) {
      return Convert.ToBoolean(val & flags);
    }

    public static unsafe int GetSampleRate(AVCodec* encoder, int targetSampleRate) {
      if (encoder->type != AVMediaType.AVMEDIA_TYPE_AUDIO) {
        throw new ArgumentException("Not an audio encoder");
      }

      if (encoder->supported_samplerates == null) {
        return targetSampleRate;
      }

      int diff = int.MaxValue;
      int ret = -1;
      for (int* rate = encoder->supported_samplerates; *rate != 0; rate++) {
        int newDiff = Mathf.Abs(*rate - targetSampleRate);
        if (newDiff < diff) {
          diff = newDiff;
          ret = *rate;
        }
      }

      return ret;
    }

    public static unsafe AVPixelFormat GetPixelFormat(AVCodec* encoder) {
      if (encoder->type != AVMediaType.AVMEDIA_TYPE_VIDEO) {
        throw new ArgumentException("Not an video encoder");
      }

      return encoder->pix_fmts == null ? AVPixelFormat.AV_PIX_FMT_YUV420P : encoder->pix_fmts[0];
    }

    public static unsafe AVRational GetFrameRate(AVCodec* encoder, int targetFrameRate) {
      if (encoder->type != AVMediaType.AVMEDIA_TYPE_VIDEO) {
        throw new ArgumentException("Not an video encoder");
      }

      if (encoder->supported_framerates == null) {
        return new AVRational { num = targetFrameRate, den = 1 };
      }

      float diff = float.PositiveInfinity;
      AVRational ret = new AVRational();
      for (AVRational* rate = encoder->supported_framerates; rate->num != 0 || rate->den != 0; rate++) {
        float newDiff = Mathf.Abs((float) rate->num / rate->den - targetFrameRate);
        if (newDiff < diff) {
          diff = newDiff;
          ret = *rate;
        }
      }

      return ret;
    }

    public static unsafe AVSampleFormat GetSampleFormat(AVCodec* encoder) {
      if (encoder->type != AVMediaType.AVMEDIA_TYPE_AUDIO) {
        throw new ArgumentException("Not an audio encoder");
      }

      return encoder->sample_fmts == null ? AVSampleFormat.AV_SAMPLE_FMT_FLTP : encoder->sample_fmts[0];
    }

    public static unsafe ulong GetChannelLayout(AVCodec* encoder, int channelLayout) {
      if (encoder->channel_layouts == null) {
        return LibavutilWrapper.AV_CH_LAYOUT_STEREO;
      }

      ulong targetChannelLayout = (ulong) channelLayout;
      for (ulong* layout = encoder->channel_layouts; *layout != 0; layout++) {
        if (*layout == targetChannelLayout) {
          return targetChannelLayout;
        }
      }

      return LibavutilWrapper.AV_CH_LAYOUT_STEREO;
    }

    public static unsafe void DebugLogVideoEncoderInfo(AVCodec* encoder) {
      if (encoder->type != AVMediaType.AVMEDIA_TYPE_VIDEO) {
        throw new ArgumentException("Not a video encoder");
      }

      List<string> formats = new List<string>();
      if (encoder->pix_fmts == null) {
        formats.Add("unknown");
      } else {
        for (AVPixelFormat* format = encoder->pix_fmts; *format != AVPixelFormat.AV_PIX_FMT_NONE; format++) {
          formats.Add((*format).ToString());
        }
      }

      Debug.Log($"Supported pixel formats for {encoder->id}: {String.Join(", ", formats)}");

      List<string> rates = new List<string>();
      if (encoder->supported_framerates == null) {
        rates.Add("any");
      } else {
        for (AVRational* rate = encoder->supported_framerates; rate->num != 0 || rate->den != 0; rate++) {
          rates.Add(((float) rate->num / rate->den).ToString(CultureInfo.InvariantCulture));
        }
      }

      Debug.Log($"Supported frame rates for {encoder->id}: {String.Join(", ", rates)}");
    }

    public static unsafe void DebugLogAudioEncoderInfo(AVCodec* encoder) {
      if (encoder->type != AVMediaType.AVMEDIA_TYPE_AUDIO) {
        throw new ArgumentException("Not an audio encoder");
      }

      List<string> formats = new List<string>();
      if (encoder->sample_fmts == null) {
        formats.Add("unknown");
      } else {
        for (AVSampleFormat* format = encoder->sample_fmts; *format != AVSampleFormat.AV_SAMPLE_FMT_NONE; format++) {
          formats.Add((*format).ToString());
        }
      }

      Debug.Log($"Supported sample formats for {encoder->id}: {String.Join(", ", formats)}");

      List<string> rates = new List<string>();
      if (encoder->supported_samplerates == null) {
        rates.Add("unknown");
      } else {
        for (int* rate = encoder->supported_samplerates; *rate != 0; rate++) {
          rates.Add((*rate).ToString());
        }
      }

      Debug.Log($"Supported sample rates for {encoder->id}: {String.Join(", ", rates)}");

      List<string> layouts = new List<string>();
      if (encoder->channel_layouts == null) {
        layouts.Add("unknown");
      } else {
        for (ulong* layout = encoder->channel_layouts; *layout != 0; layout++) {
          layouts.Add((*layout).ToString());
        }
      }

      Debug.Log($"Supported channel layouts for {encoder->id}: {String.Join(", ", layouts)}");
    }

    public static void DebugLogRecordingOptions(RecordingOptions options) {
      Debug.Log($"Options for recording \"{options.name}\":");
      Debug.Log($"\tVideo encoding enabled: {options.vParams != null}");
      Debug.Log($"\tAudio encoding enabled: {options.aParams != null}");
      Debug.Log($"\tVideo frame rate: {options.vParams.frameRate}");
      Debug.Log($"\tAudio sample rate: {options.aParams.sampleRate}");
      Debug.Log($"\tVideo bit rate: {options.vParams.bitRate}");
      Debug.Log($"\tAudio bit rate: {options.aParams.bitRate}");
      Debug.Log($"\tVideo width: {options.vParams.width}");
      Debug.Log($"\tVideo height: {options.vParams.height}");
      Debug.Log($"\tVideo FFmpeg filter description: {options.vParams.filterGraphDesc}");
      Debug.Log($"\tAudio FFmpeg filter description: {options.aParams.filterGraphDesc}");
      Debug.Log($"\tAudio FFmpeg channel alyout: {options.aParams.channelLayout}");
    }
  }
}
