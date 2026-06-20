using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia
{
    /// <summary>
    /// Batches multiple Skia GPU render-surface leases into one frame surface.
    /// The frame is read back to the WinForms target only when the scope ends.
    /// </summary>
    internal sealed class SkiaGpuFrameRenderScope : IDisposable
    {
        [ThreadStatic]
        private static SkiaGpuFrameRenderScope? current;

        private readonly SkiaGpuMapRenderSurface _surface;
        private readonly SkiaGpuFrameRenderScope? _previous;
        private bool _disposed;

        public SkiaGpuFrameRenderScope(Graphics graphics, Size pixelSize)
        {
            _previous = current;
            _surface = new SkiaGpuMapRenderSurface(graphics, pixelSize);
            current = this;
        }

        public static bool TryCreateLease(
            Graphics graphics,
            Size pixelSize,
            MapRenderSurfaceOptions options,
            out IMapRenderSurface surface)
        {
            SkiaGpuFrameRenderScope? scope = current;
            if (scope != null &&
                !scope._disposed &&
                scope._surface.CanLease(graphics, pixelSize))
            {
                surface = scope._surface.CreateFrameLease(options);
                return true;
            }

            surface = null!;
            return false;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            current = _previous;
            _surface.Dispose();
        }
    }
}
