using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace UnityMediaRecorder.FFmpegLibraryWrappers {
  public static class LibavfilterWrapper {
    private const string LIBAVFILTER_PLUGIN_NAME_ =
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
      "avfilter-7.dll";
#else
      "PLATFORM_NOT_SUPPORTED";
#endif

#pragma warning disable IDE1006
    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern AVFilter* avfilter_get_by_name([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern AVFilterGraph* avfilter_graph_alloc();

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avfilter_graph_create_filter(AVFilterContext** filt_ctx, AVFilter* filt, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string args, void* opaque, AVFilterGraph* graph_ctx);

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern AVFilterInOut* avfilter_inout_alloc();

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avfilter_graph_parse_ptr(AVFilterGraph* graph, [MarshalAs(UnmanagedType.LPUTF8Str)] string filters, AVFilterInOut** inputs, AVFilterInOut** outputs, void* log_ctx);

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int avfilter_graph_config(AVFilterGraph* graphctx, void* log_ctx);

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void avfilter_inout_free(AVFilterInOut** inout);

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_buffersrc_write_frame(AVFilterContext* ctx, AVFrame* frame);

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern int av_buffersink_get_frame(AVFilterContext* ctx, AVFrame* frame);

    [DllImport(LIBAVFILTER_PLUGIN_NAME_, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public unsafe static extern void avfilter_graph_free(AVFilterGraph** graph);
#pragma warning restore IDE1006
  }
}
