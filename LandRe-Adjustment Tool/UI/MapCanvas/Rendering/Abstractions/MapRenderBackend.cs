namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Identifies the concrete rendering engine used by the map canvas.
    /// </summary>
    /// <remarks>
    /// Keep backend selection centralized. Renderer logic should branch on this
    /// value only at factory/host boundaries, not inside every draw method.
    /// </remarks>
    public enum MapRenderBackend
    {
        /// <summary>
        /// Use the current Windows Forms GDI+ renderer backed by <see cref="Graphics"/>.
        /// </summary>
        GdiPlus = 0,

        /// <summary>
        /// Use SkiaSharp CPU raster rendering.
        /// </summary>
        SkiaCpu = 1,

        /// <summary>
        /// Reserved for future SkiaSharp GPU rendering. This option is exposed
        /// now so settings can remain stable, but no GPU adapter is implemented
        /// in the current build.
        /// </summary>
        SkiaGpu = 2,

        /// <summary>
        /// Compatibility alias for earlier planning/tests that used the generic
        /// SkiaSharp backend name. It currently resolves to the CPU adapter.
        /// </summary>
        SkiaSharp = SkiaCpu
    }
}
