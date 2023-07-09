using System;
using FFmpeg.AutoGen;
using UnityMediaRecorder.Utils;
using UnityMediaRecorder.FFmpegLibraryWrappers;

namespace UnityMediaRecorder {
  public unsafe class Recording {
    public readonly RecordingOptions options;

    private readonly FFmpegMediaHandler aHandler_;
    private readonly FFmpegMediaHandler vHandler_;
    private readonly AVFormatContext* outCtx_;

    private long? videoStartTime_;

    public bool IsVideoEnabled => options.vParams != null;
    public bool IsAudioEnabled => options.aParams != null;

    public Recording(RecordingOptions options) {
      this.options = options;

      string filename = options.name + FFmpegUtils.GetFilenameExtension(IsVideoEnabled, IsAudioEnabled);

      int ret;

      fixed (AVFormatContext** ptr = &outCtx_) {
        ret = LibavformatWrapper.avformat_alloc_output_context2(ptr, null, null, filename);
      }
      FFmpegUtils.CheckRet(ret, "Failed to allocate output context");

      AVOutputFormat* outFmt = outCtx_->oformat;

      if (!FFmpegUtils.CheckFlag(outFmt->flags, LibavformatWrapper.AVFMT_NOFILE)) {
        ret = LibavformatWrapper.avio_open(&outCtx_->pb, filename, LibavformatWrapper.AVIO_FLAG_WRITE);
        FFmpegUtils.CheckRet(ret, "Failed to open file");
      }

      if (IsVideoEnabled && outFmt->video_codec != AVCodecID.AV_CODEC_ID_NONE) {
        vHandler_ = new FFmpegMediaHandler(outFmt->video_codec, outCtx_, options.vParams);
      }

      if (IsAudioEnabled && outFmt->audio_codec != AVCodecID.AV_CODEC_ID_NONE) {
        aHandler_ = new FFmpegMediaHandler(outFmt->audio_codec, outCtx_, options.aParams);
      }

      ret = LibavformatWrapper.avformat_write_header(outCtx_, null);
      FFmpegUtils.CheckRet(ret, "Failed to write stream header");
    }

    public void EncodeVideoFrame(byte[] data, int width, int height, long timestamp) {
      if (!IsVideoEnabled) {
        throw new InvalidOperationException("Video recording disabled");
      }

      // Assuming that texture data from Unity will be RGBA - 1 byte per channel
      SwsContext* swsCtx = LibswscaleWrapper.sws_getContext(
        width,
        height,
        AVPixelFormat.AV_PIX_FMT_RGBA,
        options.vParams.width,
        options.vParams.height,
        (AVPixelFormat) vHandler_.fr->format,
        0,
        null,
        null,
        null);
      FFmpegUtils.CheckRet(swsCtx, "Failed to allocate scaling/conversion context");

      byte*[] dataPlanes;
      fixed (byte* dataPtr = data) {
        dataPlanes = new[] { dataPtr };
      }

      int[] linesize = { 4 * width };

      int ret = LibavutilWrapper.av_frame_make_writable(vHandler_.fr);
      FFmpegUtils.CheckRet(ret, "Failed to make video frame writable");

      LibswscaleWrapper.sws_scale(
        swsCtx,
        dataPlanes,
        linesize,
        0,
        height,
        vHandler_.fr->data,
        vHandler_.fr->linesize);

      LibswscaleWrapper.sws_freeContext(swsCtx);

      vHandler_.fr->pts = timestamp;
      vHandler_.WriteFrame();
    }

    public void EncodeAudioFrame(float[] data, int channels, int sampleRate) {
      if (!IsAudioEnabled) {
        throw new InvalidOperationException("Audio recording disabled");
      }

      SwrContext* swrCtx = LibswresampleWrapper.swr_alloc();
      FFmpegUtils.CheckRet(swrCtx, "Failed to allocate resampling context");

      // Assume that audio data from Unity will be interleaved floats
      LibavutilWrapper.av_opt_set_sample_fmt(swrCtx, "in_sample_fmt", AVSampleFormat.AV_SAMPLE_FMT_FLT, 0);
      LibavutilWrapper.av_opt_set_int(swrCtx, "in_channel_count", channels, 0);
      LibavutilWrapper.av_opt_set_int(swrCtx, "in_sample_rate", sampleRate, 0);
      LibavutilWrapper.av_opt_set_sample_fmt(swrCtx, "out_sample_fmt", aHandler_.encCtx->sample_fmt, 0);
      LibavutilWrapper.av_opt_set_int(swrCtx, "out_channel_count", aHandler_.encCtx->channels, 0);
      LibavutilWrapper.av_opt_set_int(swrCtx, "out_sample_rate", aHandler_.encCtx->sample_rate, 0);

      int ret = LibswresampleWrapper.swr_init(swrCtx);
      FFmpegUtils.CheckRet(ret, "Failed to initialize resampling context");

      ret = LibavutilWrapper.av_frame_make_writable(aHandler_.fr);
      FFmpegUtils.CheckRet(ret, "Failed to make audio frame writable");

      int samplesPerChannel = data.Length / channels;
      byte*[] frameDataPtrArray = aHandler_.fr->data;
      fixed (byte** frameDataDblPtr = frameDataPtrArray) {
        fixed (float* dataPtr = data) {
          byte** dataDblPtr = (byte**) &dataPtr;
          ret = LibswresampleWrapper.swr_convert(
            swrCtx,
            frameDataDblPtr,
            samplesPerChannel,
            dataDblPtr,
            samplesPerChannel);
        }
      }
      FFmpegUtils.CheckRet(ret, "Failed to resample audio data");

      LibswresampleWrapper.swr_free(&swrCtx);

      aHandler_.fr->pts = aHandler_.nextPts;
      aHandler_.nextPts += samplesPerChannel;
      aHandler_.WriteFrame();
    }

    public void CleanUp() {
      int ret = LibavformatWrapper.av_write_trailer(outCtx_);
      FFmpegUtils.CheckRet(ret, "Failed to write stream trailer");

      if (!FFmpegUtils.CheckFlag(outCtx_->oformat->flags, LibavformatWrapper.AVFMT_NOFILE)) {
        ret = LibavformatWrapper.avio_closep(&outCtx_->pb);
        FFmpegUtils.CheckRet(ret, "Failed to close file");
      }

      vHandler_?.CleanUp();
      aHandler_?.CleanUp();
      LibavformatWrapper.avformat_free_context(outCtx_);
    }

    public bool CanEncodeAudioFrame() {
      if (!IsAudioEnabled) {
        return false;
      }

      if (!IsVideoEnabled) {
        return true;
      }

      return LibavutilWrapper.av_compare_ts(
        aHandler_.nextPts,
        aHandler_.encCtx->time_base,
        vHandler_.nextPts,
        vHandler_.encCtx->time_base) < 0;
    }

    public bool CanEncodeVideoFrame(out long timestamp) {
      if (!IsVideoEnabled) {
        timestamp = -1;
        return false;
      }

      long currentTime = LibavutilWrapper.av_gettime();
      if (!videoStartTime_.HasValue) {
        videoStartTime_ = currentTime;
      }

      timestamp = (currentTime - videoStartTime_.Value) / 1000;
      if (timestamp >= vHandler_.nextPts) {
        // nextPts is not actually used for timestamps for video, but is used to provide an estimate of the next frame's timestamp
        vHandler_.nextPts = timestamp + 1000 / options.vParams.frameRate;
        return true;
      }

      return false;
    }
  }
}
