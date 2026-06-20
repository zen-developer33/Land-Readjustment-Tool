using System.Diagnostics;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Diagnostics
{
    /// <summary>
    /// Per-window snapshot of render-surface activity. A "window" is the span
    /// between two debug-overlay reads (roughly one painted frame).
    /// </summary>
    public readonly record struct RenderSurfaceWindow(
        MapRenderBackend Backend,
        int SurfaceCount,
        double CreateMs,
        double ReadbackMs,
        double BlitMs,
        double MaxReadbackMs,
        int GdiPathFallbackCount,
        long LifetimeSurfaces,
        double LifetimeReadbackMs);

    /// <summary>
    /// Lightweight, thread-safe collector that records how each render surface
    /// behaves per backend. Used to diagnose why a backend is fast or slow:
    /// how many surfaces are created per frame, and how much time goes into
    /// surface creation, GPU read-back, and the GDI composite blit.
    /// </summary>
    /// <remarks>
    /// Recording is a no-op unless <see cref="Enabled"/> is true (driven by the
    /// debug overlay), so this has zero cost in normal operation. Surfaces are
    /// created on both the UI thread (decorations) and background threads (vector
    /// cache rebuild), so all access is guarded by a lock.
    /// </remarks>
    public static class RenderBackendTelemetry
    {
        private const double SlowReadbackLogThresholdMs = 3.0;

        private static readonly object Sync = new();

        /// <summary>Turns recording and logging on. Set by the debug overlay toggle.</summary>
        public static volatile bool Enabled;

        // Current window accumulators (reset on each overlay read).
        private static MapRenderBackend _backend;
        private static int _count;
        private static double _createMs;
        private static double _readbackMs;
        private static double _blitMs;
        private static double _maxReadbackMs;
        private static int _gdiPathFallbackCount;

        // Lifetime totals (never reset) for log summaries.
        private static long _lifetimeSurfaces;
        private static double _lifetimeReadbackMs;

        /// <summary>
        /// Records that a Skia backend had to convert a legacy GDI path.
        /// </summary>
        public static void RecordGdiPathFallback()
        {
            if (!Enabled)
            {
                return;
            }

            lock (Sync)
            {
                _gdiPathFallbackCount++;
            }
        }

        /// <summary>
        /// Records one render surface's lifecycle timings.
        /// </summary>
        /// <param name="backend">Backend that produced the surface.</param>
        /// <param name="createMs">Time spent constructing the surface (GPU/bitmap allocation).</param>
        /// <param name="readbackMs">
        /// Time spent flushing and copying pixels back to the CPU. Zero for GDI+
        /// and CPU surfaces that draw straight into the target bitmap.
        /// </param>
        /// <param name="blitMs">Time spent compositing the surface to the GDI target.</param>
        public static void Record(
            MapRenderBackend backend,
            double createMs,
            double readbackMs,
            double blitMs)
        {
            if (!Enabled)
            {
                return;
            }

            lock (Sync)
            {
                _backend = backend;
                _count++;
                _createMs += createMs;
                _readbackMs += readbackMs;
                _blitMs += blitMs;
                if (readbackMs > _maxReadbackMs)
                {
                    _maxReadbackMs = readbackMs;
                }

                _lifetimeSurfaces++;
                _lifetimeReadbackMs += readbackMs;
            }

            if (readbackMs >= SlowReadbackLogThresholdMs)
            {
                Debug.WriteLine(
                    $"[RenderBackend] {backend} surface: create {createMs:0.0} ms, " +
                    $"readback {readbackMs:0.0} ms, blit {blitMs:0.0} ms");
            }
        }

        /// <summary>
        /// Returns the current window's totals and resets the window accumulators.
        /// </summary>
        public static RenderSurfaceWindow SnapshotAndReset()
        {
            lock (Sync)
            {
                RenderSurfaceWindow snapshot = new(
                    _backend,
                    _count,
                    _createMs,
                    _readbackMs,
                    _blitMs,
                    _maxReadbackMs,
                    _gdiPathFallbackCount,
                    _lifetimeSurfaces,
                    _lifetimeReadbackMs);

                _count = 0;
                _createMs = 0;
                _readbackMs = 0;
                _blitMs = 0;
                _maxReadbackMs = 0;
                _gdiPathFallbackCount = 0;

                return snapshot;
            }
        }
    }
}
