using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace UnityMediaRecorder.FFmpegLibraryWrappers {
  public static class LibavcodecWrapper {
    private const string LIBAVCODEC_PLUGIN_NAME_ =
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
      "avcodec-58.dll";
#else
      "PLATFORM_NOT_SUPPORTED";
#endif
    public const int AV_CODEC_FLAG_GLOBAL_HEADER = 1 << 22;

#pragma warning disable IDE1006
    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void av_packet_rescale_ts(AVPacket* pkt, AVRational tb_src, AVRational tb_dst);

    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void av_packet_unref(AVPacket* pkt);

    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern AVCodec* avcodec_find_encoder(AVCodecID id);

    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern AVCodecContext* avcodec_alloc_context3(AVCodec* codec);

    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avcodec_open2(AVCodecContext* avctx, AVCodec* codec, AVDictionary** options);

    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avcodec_parameters_from_context(AVCodecParameters* par, AVCodecContext* codec);

    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avcodec_send_frame(AVCodecContext* avctx, AVFrame* frame);

    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avcodec_receive_packet(AVCodecContext* avctx, AVPacket* avpkt);

    [DllImport(LIBAVCODEC_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void avcodec_free_context(AVCodecContext** avctx);
#pragma warning restore IDE1006
  }
}
