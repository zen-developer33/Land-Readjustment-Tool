using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public enum CanvasFrameSource
    {
        None,
        Direct,
        Cache,
        PanCache,
        ZoomCache,
        HeldZoomCache
    }

    public sealed class VectorRenderStats
    {
        public static VectorRenderStats Empty { get; } = new();

        public int TotalFeatureCount { get; init; }
        public int SpatialIndexEntryCount { get; init; }
        public int QueryCandidateCount { get; init; }
        public int RenderedFeatureCount { get; init; }
        public int HiddenSkippedCount { get; init; }
        public int LodSkippedCount { get; init; }
        public bool LevelOfDetailEnabled { get; init; }
        public double MinimumVisibleWorldSize { get; init; }
        public double QueryElapsedMs { get; init; }
        public double RenderElapsedMs { get; init; }
        public RectangleD VisibleWorldBounds { get; init; }
    }

    public readonly record struct DeferredRendererDebugState(
        bool CacheValid,
        bool PanBufferValid,
        bool ZoomFrameAvailable,
        Size CanvasSize,
        int LayerCacheCount,
        int ActiveFrameLeases,
        int RetiredBitmapCount,
        double LastRefreshElapsedMs);

    public sealed class MapCanvasRendererDebugState
    {
        public int RasterLayerCount { get; init; }
        public int VisibleRasterLayerCount { get; init; }
        public int VectorLayerCount { get; init; }
        public VectorRenderStats VectorStats { get; init; } = VectorRenderStats.Empty;
        public DeferredRendererDebugState VectorCache { get; init; }
    }
}
