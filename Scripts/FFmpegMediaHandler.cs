using FFmpeg.AutoGen;
using UnityMediaRecorder.Utils;

namespace UnityMediaRecorder {

  public unsafe class FFmpegMediaHandler {

    public long nextPts;

    public readonly AVFrame* fr;
    public readonly AVCodecContext* encCtx;
    private readonly AVStream* st_;
    private readonly AVFormatContext* outCtx_;
    private readonly FFmpegFilterHandler filterHandler_;

    public FFmpegMediaHandler(AVCodecID codecId, AVFormatContext* outCtx, VideoParams vParams) {
      AVCodec* enc = ffmpeg.avcodec_find_encoder(codecId);
      FFmpegUtils.CheckRet(enc, "Failed to find encoder");

      if (enc->type != AVMediaType.AVMEDIA_TYPE_VIDEO) {
        throw new FFmpegException("Encoder media type is not video");
      }

      outCtx_ = outCtx;

      st_ = ffmpeg.avformat_new_stream(outCtx_, null);
      FFmpegUtils.CheckRet(st_, "Failed to allocate stream");

      st_->id = (int) outCtx_->nb_streams - 1;

      encCtx = ffmpeg.avcodec_alloc_context3(enc);
      FFmpegUtils.CheckRet(encCtx, "Failed to allocate encoder context");

      fr = ffmpeg.av_frame_alloc();
      FFmpegUtils.CheckRet(fr, "Failed to allocate frame");

      // assign timestamps to frames based on time we requested texture for precision
      AVRational timeBase = new AVRational { num = 1, den = 1000 };
      encCtx->bit_rate = vParams.bitRate;
      encCtx->width = vParams.width;
      encCtx->height = vParams.height;
      encCtx->time_base = timeBase;
      encCtx->pix_fmt = FFmpegUtils.GetPixelFormat(enc);
      st_->time_base = timeBase;

      if (FFmpegUtils.CheckFlag(outCtx_->oformat->flags, ffmpeg.AVFMT_GLOBALHEADER)) {
        encCtx->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;
      }

      int ret = ffmpeg.avcodec_open2(encCtx, enc, null);
      FFmpegUtils.CheckRet(ret, "Failed to open encoder");

      fr->format = (int) encCtx->pix_fmt;
      fr->width = encCtx->width;
      fr->height = encCtx->height;
      filterHandler_ = FFmpegVideoFilterHandler.Factory(vParams.width, vParams.height, vParams.filterGraphDesc, fr);

      ret = ffmpeg.av_frame_get_buffer(fr, 0);
      FFmpegUtils.CheckRet(ret, "Failed to allocate frame buffer");

      ret = ffmpeg.avcodec_parameters_from_context(st_->codecpar, encCtx);
      FFmpegUtils.CheckRet(ret, "Failed to copy stream parameters");
    }

    public FFmpegMediaHandler(AVCodecID codecId, AVFormatContext* outCtx, AudioParams aParams) {
      AVCodec* enc = ffmpeg.avcodec_find_encoder(codecId);
      FFmpegUtils.CheckRet(enc, "Failed to find encoder");

      if (enc->type != AVMediaType.AVMEDIA_TYPE_AUDIO) {
        throw new FFmpegException("Encoder media type is not audio");
      }

      outCtx_ = outCtx;

      st_ = ffmpeg.avformat_new_stream(outCtx_, null);
      FFmpegUtils.CheckRet(st_, "Failed to allocate stream");

      st_->id = (int) outCtx_->nb_streams - 1;

      encCtx = ffmpeg.avcodec_alloc_context3(enc);
      FFmpegUtils.CheckRet(encCtx, "Failed to allocate encoder context");

      fr = ffmpeg.av_frame_alloc();
      FFmpegUtils.CheckRet(fr, "Failed to allocate frame");

      int sampleRate = FFmpegUtils.GetSampleRate(enc, aParams.sampleRate);
      AVRational timeBase = new AVRational { num = 1, den = sampleRate };
      encCtx->sample_fmt = FFmpegUtils.GetSampleFormat(enc);
      encCtx->bit_rate = aParams.bitRate;
      encCtx->sample_rate = sampleRate;
      encCtx->channel_layout = FFmpegUtils.GetChannelLayout(enc, aParams.channelLayout);
      encCtx->channels = ffmpeg.av_get_channel_layout_nb_channels(encCtx->channel_layout);
      encCtx->time_base = timeBase;
      st_->time_base = timeBase;


      if (FFmpegUtils.CheckFlag(outCtx_->oformat->flags, ffmpeg.AVFMT_GLOBALHEADER)) {
        encCtx->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;
      }

      int ret = ffmpeg.avcodec_open2(encCtx, enc, null);
      FFmpegUtils.CheckRet(ret, "Failed to open encoder");

      fr->format = (int) encCtx->sample_fmt;
      fr->channel_layout = encCtx->channel_layout;
      fr->sample_rate = encCtx->sample_rate;
      fr->nb_samples = encCtx->frame_size;
      filterHandler_ = FFmpegAudioFilterHandler.Factory(encCtx->sample_rate, encCtx->sample_fmt, encCtx->channel_layout, aParams.filterGraphDesc, fr);

      ret = ffmpeg.av_frame_get_buffer(fr, 0);
      FFmpegUtils.CheckRet(ret, "Failed to allocate frame buffer");

      ret = ffmpeg.avcodec_parameters_from_context(st_->codecpar, encCtx);
      FFmpegUtils.CheckRet(ret, "Failed to copy stream parameters");
    }

    public void WriteFrame() {
      filterHandler_?.ApplyFilter();

      int ret = ffmpeg.avcodec_send_frame(encCtx, fr);
      FFmpegUtils.CheckRet(ret, "Failed to send frame to encoder");

      AVPacket pkt = new AVPacket();

      while (ret >= 0) {
        ret = ffmpeg.avcodec_receive_packet(encCtx, &pkt);
        if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF) {
          break;
        }

        FFmpegUtils.CheckRet(ret, "Failed to encode frame");

        ffmpeg.av_packet_rescale_ts(&pkt, encCtx->time_base, st_->time_base);
        pkt.stream_index = st_->index;

        ret = ffmpeg.av_interleaved_write_frame(outCtx_, &pkt);
        FFmpegUtils.CheckRet(ret, "Failed to write packet to file");

        ffmpeg.av_packet_unref(&pkt);
      }
    }

    public void CleanUp() {
      fixed (AVCodecContext** ptr = &encCtx) {
        ffmpeg.avcodec_free_context(ptr);
      }

      fixed (AVFrame** ptr = &fr) {
        ffmpeg.av_frame_free(ptr);
      }

      filterHandler_?.CleanUp();
    }

  }

}
