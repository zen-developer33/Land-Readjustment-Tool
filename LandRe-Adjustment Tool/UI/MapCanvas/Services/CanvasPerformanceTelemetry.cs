namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    public sealed record CanvasOperationPerformanceSnapshot(
        string OperationName,
        string Detail,
        double DatabaseElapsedMs,
        double CanvasElapsedMs,
        double TotalElapsedMs,
        DateTime CapturedAt)
    {
        public static CanvasOperationPerformanceSnapshot Empty { get; } = new(
            "No operation measured yet",
            "Use the canvas normally. Draw, copy, move, delete, or refresh to see timings here.",
            0,
            0,
            0,
            DateTime.MinValue);
    }

    public sealed record CanvasLivePerformanceSnapshot(
        int LoadedObjectCount,
        int VisibleObjectCount,
        int SelectedObjectCount,
        int SpatialCandidateCount,
        int RenderedObjectCount,
        int HiddenSkippedCount,
        int LodSkippedCount,
        bool LevelOfDetailEnabled,
        double LastFrameElapsedMs,
        double AverageFrameElapsedMs,
        double FramesPerSecond,
        double VectorQueryElapsedMs,
        double VectorRenderElapsedMs,
        bool VectorCacheValid,
        double VectorCacheRefreshElapsedMs,
        int VectorLayerCount,
        int RasterLayerCount,
        int VisibleRasterLayerCount);

    public static class CanvasPerformanceTelemetry
    {
        private static readonly object SyncRoot = new();
        private static CanvasOperationPerformanceSnapshot _lastOperation =
            CanvasOperationPerformanceSnapshot.Empty;

        public static void RecordOperation(
            string operationName,
            string detail,
            double databaseElapsedMs,
            double canvasElapsedMs,
            double totalElapsedMs)
        {
            lock (SyncRoot)
            {
                _lastOperation = new CanvasOperationPerformanceSnapshot(
                    operationName,
                    detail,
                    databaseElapsedMs,
                    canvasElapsedMs,
                    totalElapsedMs,
                    DateTime.Now);
            }
        }

        public static CanvasOperationPerformanceSnapshot GetLastOperation()
        {
            lock (SyncRoot)
            {
                return _lastOperation;
            }
        }
    }
}
