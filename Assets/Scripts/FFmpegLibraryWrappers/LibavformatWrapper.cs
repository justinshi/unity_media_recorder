using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace UnityMediaRecorder.FFmpegLibraryWrappers {
  public static class LibavformatWrapper {
    private const string LIBFORMAT_PLUGIN_NAME_ =
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
      "avformat-58.dll";
#else
      "PLATFORM_NOT_SUPPORTED";
#endif
    public const int AVFMT_GLOBALHEADER = 0x0040;
    public const int AVFMT_NOFILE = 0x0001;
    public const int AVIO_FLAG_WRITE = 2;

#pragma warning disable IDE1006
    [DllImport(LIBFORMAT_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_interleaved_write_frame(AVFormatContext* s, AVPacket* pkt);

    [DllImport(LIBFORMAT_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_write_trailer(AVFormatContext* s);

    [DllImport(LIBFORMAT_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern AVStream* avformat_new_stream(AVFormatContext* s, AVCodec* c);

    [DllImport(LIBFORMAT_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avformat_alloc_output_context2(AVFormatContext** ctx, AVOutputFormat* oformat, [MarshalAs(UnmanagedType.LPUTF8Str)] string format_name, [MarshalAs(UnmanagedType.LPUTF8Str)] string filename);

    [DllImport(LIBFORMAT_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avio_open(AVIOContext** s, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int flags);

    [DllImport(LIBFORMAT_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avformat_write_header(AVFormatContext* s, AVDictionary** options);

    [DllImport(LIBFORMAT_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avio_closep(AVIOContext** s);

    [DllImport(LIBFORMAT_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void avformat_free_context(AVFormatContext* s);
#pragma warning restore IDE1006
  }
}
