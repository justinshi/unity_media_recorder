using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace UnityMediaRecorder.FFmpegLibraryWrappers {
  public static class LibavutilWrapper {
    private const string LIBAVUTIL_PLUGIN_NAME_ =
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
      "avutil-56.dll";
#else
      "PLATFORM_NOT_SUPPORTED";
#endif
    public const int AV_OPT_SEARCH_CHILDREN = 1 << 0;
    public const int EAGAIN = 11;
    public const int AV_CH_FRONT_CENTER = 0x00000004;
    public const int AV_CH_FRONT_LEFT = 0x00000001;
    public const int AV_CH_FRONT_RIGHT = 0x00000002;
    public const int AV_CH_BACK_LEFT = 0x00000010;
    public const int AV_CH_BACK_RIGHT = 0x00000020;
    public const int AV_CH_SIDE_LEFT = 0x00000200;
    public const int AV_CH_SIDE_RIGHT = 0x00000400;
    public const int AV_CH_LOW_FREQUENCY = 0x00000008;
    public const int AV_CH_LAYOUT_MONO = AV_CH_FRONT_CENTER;
    public const int AV_CH_LAYOUT_STEREO = AV_CH_FRONT_LEFT | AV_CH_FRONT_RIGHT;
    public const int AV_CH_LAYOUT_QUAD = AV_CH_LAYOUT_STEREO | AV_CH_BACK_LEFT | AV_CH_BACK_RIGHT;
    public const int AV_CH_LAYOUT_SURROUND = AV_CH_LAYOUT_STEREO | AV_CH_FRONT_CENTER;
    public const int AV_CH_LAYOUT_5POINT0 = AV_CH_LAYOUT_SURROUND | AV_CH_SIDE_LEFT | AV_CH_SIDE_RIGHT;
    public const int AV_CH_LAYOUT_5POINT1 = AV_CH_LAYOUT_5POINT0 | AV_CH_LOW_FREQUENCY;
    public const int AV_CH_LAYOUT_7POINT1 = AV_CH_LAYOUT_5POINT1 | AV_CH_BACK_LEFT | AV_CH_BACK_RIGHT;

    public static readonly int AVERROR_EOF = FFERRTAG('E', 'O', 'F', ' ');

    public static int MKTAG<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d) {
      return (int) (Convert.ToUInt32(a) | (Convert.ToUInt32(b) << 8) | (Convert.ToUInt32(c) << 16) | (Convert.ToUInt32(d) << 24));
    }

    public static int AVERROR<T>(T e) {
      return -Convert.ToInt32(e);
    }

    public static int FFERRTAG<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d) {
      return -MKTAG(a, b, c, d);
    }

#pragma warning disable IDE1006
    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
    public static extern string av_get_sample_fmt_name(AVSampleFormat sample_fmt);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long av_gettime();

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_opt_set_bin(void* obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, byte* val, int size, int search_flags);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern byte* av_strdup([MarshalAs(UnmanagedType.LPUTF8Str)] string s);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_opt_set_sample_fmt(void* obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, AVSampleFormat fmt, int search_flags);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_opt_set_int(void* obj, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, long val, int search_flags);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void av_frame_unref(AVFrame* frame);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern AVFrame* av_frame_alloc();

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_frame_get_buffer(AVFrame* frame, int align);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int av_get_channel_layout_nb_channels(ulong channel_layout);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void av_frame_free(AVFrame** frame);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_frame_make_writable(AVFrame* frame);

    [DllImport(LIBAVUTIL_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_compare_ts(long ts_a, AVRational tb_a, long ts_b, AVRational tb_b);
#pragma warning restore IDE1006
  }
}
