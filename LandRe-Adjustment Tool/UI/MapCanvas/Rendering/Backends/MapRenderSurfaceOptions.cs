using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends
{
    /// <summary>
    /// Describes how a map render surface should be selected and initialized.
    /// </summary>
    /// <remarks>
    /// Keep these options small and host-oriented. Detailed layer styling belongs
    /// in render settings and style records, while this type answers the boundary
    /// question: which backend should own the frame surface?
    /// </remarks>
    public sealed class MapRenderSurfaceOptions
    {
        /// <summary>
        /// Gets default options for the current production renderer.
        /// </summary>
        public static MapRenderSurfaceOptions Default { get; } = new();

        /// <summary>
        /// Gets or initializes the backend requested by the caller.
        /// </summary>
        public MapRenderBackend RequestedBackend { get; init; } = MapRenderBackend.GdiPlus;

        /// <summary>
        /// Gets or initializes the quality preset applied immediately after surface creation.
        /// </summary>
        public RenderQuality InitialQuality { get; init; } = RenderQuality.VectorHighQuality;

        /// <summary>
        /// Gets or initializes whether the factory should apply <see cref="InitialQuality"/>.
        /// </summary>
        public bool ApplyInitialQuality { get; init; } = true;

        /// <summary>
        /// Gets or initializes whether unavailable backends should fall back to GDI+.
        /// </summary>
        /// <remarks>
        /// This keeps the application usable while SkiaSharp is introduced in
        /// stages. Tests can disable fallback to verify strict failure behavior.
        /// </remarks>
        public bool FallbackToGdiPlusWhenUnavailable { get; init; } = true;

        /// <summary>
        /// Creates options for a specific requested backend.
        /// </summary>
        /// <param name="backend">Backend requested by the caller.</param>
        public static MapRenderSurfaceOptions ForBackend(MapRenderBackend backend) =>
            new() { RequestedBackend = backend };
    }
}
