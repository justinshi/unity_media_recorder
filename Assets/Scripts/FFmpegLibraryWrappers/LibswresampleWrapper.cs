using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace UnityMediaRecorder.FFmpegLibraryWrappers {
  public static class LibswresampleWrapper {
    private const string LIBSWRESAMPLE_PLUGIN_NAME_ =
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
      "swresample-3.dll";
#else
      "PLATFORM_NOT_SUPPORTED";
#endif

#pragma warning disable IDE1006
    [DllImport(LIBSWRESAMPLE_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern SwrContext* swr_alloc();

    [DllImport(LIBSWRESAMPLE_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int swr_init(SwrContext* s);

    [DllImport(LIBSWRESAMPLE_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int swr_convert(SwrContext* s, byte** out_arg, int out_count, byte** in_arg, int in_count);

    [DllImport(LIBSWRESAMPLE_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void swr_free(SwrContext** s);
#pragma warning restore IDE1006
  }
}
