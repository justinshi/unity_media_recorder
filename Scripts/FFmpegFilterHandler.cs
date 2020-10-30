using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using UnityMediaRecorder.Utils;

namespace UnityMediaRecorder {

  public unsafe class FFmpegAudioFilterHandler : FFmpegFilterHandler {

    public static FFmpegAudioFilterHandler Factory(int sampleRate, AVSampleFormat format, ulong channelLayout, string desc, AVFrame* fr) {
      if (desc == String.Empty) {
        return null;
      }

      string sampleFmtName = ffmpeg.av_get_sample_fmt_name(format);
      if (sampleFmtName == null) {
        throw new FFmpegException("Sample format not recognized");
      }

      string args = String.Join(
        ":",
        $"time_base=1/{sampleRate}",
        $"sample_rate={sampleRate}",
        $"sample_fmt={sampleFmtName}",
        $"channel_layout={channelLayout}");

      return new FFmpegAudioFilterHandler(args, desc, fr);
    }

    private FFmpegAudioFilterHandler(string args, string desc, AVFrame* fr) : base(args, desc, fr) {}

  }

  public unsafe class FFmpegVideoFilterHandler : FFmpegFilterHandler {

    public static FFmpegVideoFilterHandler Factory(int width, int height, string desc, AVFrame* fr) {
      if (desc == String.Empty) {
        return null;
      }

      int pixelFmt = fr->format;

      string args = String.Join(
        ":",
        $"video_size={width}x{height}",
        $"pix_fmt={pixelFmt}",
        "time_base=1/1000",
        "pixel_aspect=1/1");

      FFmpegVideoFilterHandler videoFilterHandler = new FFmpegVideoFilterHandler(args, desc, fr);

      int ret = ffmpeg.av_opt_set_bin(
        videoFilterHandler.bufSinkCtx_,
        "pix_fmts",
        (byte*) &pixelFmt,
        Marshal.SizeOf(pixelFmt),
        ffmpeg.AV_OPT_SEARCH_CHILDREN);
      FFmpegUtils.CheckRet(ret, "Failed to set sink filter output format");

      return videoFilterHandler;
    }

    private FFmpegVideoFilterHandler(string args, string desc, AVFrame* fr) : base(args, desc, fr) {}

  }

  public abstract unsafe class FFmpegFilterHandler {

    protected readonly AVFilterContext* bufSinkCtx_;
    private readonly AVFilterContext* bufSrcCtx_;
    private readonly AVFilterGraph* graph_;
    private readonly AVFrame* fr_;

    protected FFmpegFilterHandler(string args, string desc, AVFrame* fr) {
      fr_ = fr;

      AVFilter* bufSrc = ffmpeg.avfilter_get_by_name("buffer");
      FFmpegUtils.CheckRet(bufSrc, "Failed to find source filter");

      AVFilter* bufSink = ffmpeg.avfilter_get_by_name("buffersink");
      FFmpegUtils.CheckRet(bufSink, "Failed to find sink filter");

      graph_ = ffmpeg.avfilter_graph_alloc();
      FFmpegUtils.CheckRet(graph_, "Failed to allocate filter graph");

      int ret;
      fixed (AVFilterContext** ptr = &bufSrcCtx_) {
        ret = ffmpeg.avfilter_graph_create_filter(ptr, bufSrc, "in", args, null, graph_);
      }

      FFmpegUtils.CheckRet(ret, "Failed to create source filter and add to graph");

      fixed (AVFilterContext** ptr = &bufSinkCtx_) {
        ret = ffmpeg.avfilter_graph_create_filter(ptr, bufSink, "out", null, null, graph_);
      }

      FFmpegUtils.CheckRet(ret, "Failed to create sink filter and add to graph");

      AVFilterInOut* outputs = ffmpeg.avfilter_inout_alloc();
      FFmpegUtils.CheckRet(outputs, "Failed to allocate graph outputs");

      outputs->filter_ctx = bufSrcCtx_;
      outputs->pad_idx = 0;
      outputs->next = null;
      outputs->name = ffmpeg.av_strdup("in");
      FFmpegUtils.CheckRet(outputs->name, "Failed to allocate output name string");

      AVFilterInOut* inputs = ffmpeg.avfilter_inout_alloc();
      FFmpegUtils.CheckRet(inputs, "Failed to allocate graph inputs");

      inputs->filter_ctx = bufSinkCtx_;
      inputs->pad_idx = 0;
      inputs->next = null;
      inputs->name = ffmpeg.av_strdup("out");
      FFmpegUtils.CheckRet(inputs->name, "Failed to allocate input name string");

      ret = ffmpeg.avfilter_graph_parse_ptr(graph_, desc, &inputs, &outputs, null);
      FFmpegUtils.CheckRet(ret, "Failed to add to filter graph based on description string");

      ret = ffmpeg.avfilter_graph_config(graph_, null);
      FFmpegUtils.CheckRet(ret, "Failed to verify and configure graph");

      ffmpeg.avfilter_inout_free(&outputs);
      ffmpeg.avfilter_inout_free(&inputs);
    }

    public void ApplyFilter() {
      int ret = ffmpeg.av_buffersrc_write_frame(bufSrcCtx_, fr_);
      FFmpegUtils.CheckRet(ret, "Failed to write frame to filter");

      ffmpeg.av_frame_unref(fr_);

      ret = ffmpeg.av_buffersink_get_frame(bufSinkCtx_, fr_);
      FFmpegUtils.CheckRet(ret, "Failed to write filter output data to frame");
    }

    public void CleanUp() {
      fixed (AVFilterGraph** ptr = &graph_) {
        ffmpeg.avfilter_graph_free(ptr);
      }
    }

  }

}
