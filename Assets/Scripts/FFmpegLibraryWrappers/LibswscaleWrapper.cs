using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace UnityMediaRecorder.FFmpegLibraryWrappers {
  public static class LibswscaleWrapper {
    private const string LIBSWSCALE_PLUGIN_NAME_ =
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
      "swscale-5.dll";
#else
      "PLATFORM_NOT_SUPPORTED";
#endif

#pragma warning disable IDE1006
    [DllImport(LIBSWSCALE_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern SwsContext* sws_getContext(int srcW, int srcH, AVPixelFormat srcFormat, int dstW, int dstH, AVPixelFormat dstFormat, int flags, SwsFilter* srcFilter, SwsFilter* dstFilter, double* param);

    [DllImport(LIBSWSCALE_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int sws_scale(SwsContext* c, byte*[] srcSlice, int[] srcStride, int srcSliceY, int srcSliceH, byte*[] dst, int[] dstStride);

    [DllImport(LIBSWSCALE_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void sws_freeContext(SwsContext* swsContext);
#pragma warning restore IDE1006
  }
}
