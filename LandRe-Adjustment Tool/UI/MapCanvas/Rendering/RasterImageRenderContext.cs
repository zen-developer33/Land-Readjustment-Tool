using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Owns one backend-neutral image drawing surface for a raster layer render
    /// pass.
    /// </summary>
    /// <remarks>
    /// Raster layers may draw many tiles in one frame. This context keeps the
    /// expensive backend surface creation outside the per-tile loop while still
    /// allowing the layer code to stay independent from GDI+ or SkiaSharp.
    /// </remarks>
    internal sealed class RasterImageRenderContext : IDisposable
    {
        private readonly IMapRenderSurfaceFactory _surfaceFactory;
        private readonly IMapRenderSurface _surface;
        private readonly bool _ownsSurface;
        private bool _disposed;

        /// <summary>
        /// Creates a raster image context for an existing WinForms paint target.
        /// </summary>
        public RasterImageRenderContext(
            Graphics graphics,
            MapRenderBackend backend,
            Size? pixelSize = null,
            IMapRenderSurfaceFactory? surfaceFactory = null)
        {
            _surfaceFactory = surfaceFactory ?? MapRenderSurfaceFactory.Default;
            _surface = _surfaceFactory.CreateForGraphics(
                graphics,
                pixelSize ?? Size.Round(graphics.VisibleClipBounds.Size),
                new MapRenderSurfaceOptions
                {
                    RequestedBackend = backend,
                    InitialQuality = RenderQuality.RasterHighSpeed
                });
            _ownsSurface = true;
        }

        /// <summary>
        /// Creates a raster image context for an already-active render surface.
        /// </summary>
        public RasterImageRenderContext(
            IMapRenderSurface surface,
            IMapRenderSurfaceFactory? surfaceFactory = null)
        {
            _surfaceFactory = surfaceFactory ?? MapRenderSurfaceFactory.Default;
            _surface = surface ?? throw new ArgumentNullException(nameof(surface));
            _surface.SetQuality(RenderQuality.RasterHighSpeed);
            _ownsSurface = false;
        }

        /// <summary>
        /// Draws a bitmap into a destination rectangle.
        /// </summary>
        public void DrawBitmap(
            Bitmap bitmap,
            RectangleF destination,
            RectangleF source,
            float opacity = 1.0f,
            ImageInterpolation interpolation = ImageInterpolation.NearestNeighbor,
            bool tileFlipXY = true)
        {
            if (!IsValidRectangle(destination) || !IsValidRectangle(source))
            {
                return;
            }

            using IMapImage image = _surfaceFactory.CreateImage(bitmap, ownsImage: false);
            _surface.DrawImage(
                image,
                destination,
                source,
                new ImageStyle(opacity, interpolation, tileFlipXY));
        }

        /// <summary>
        /// Draws a bitmap into a destination parallelogram.
        /// </summary>
        public void DrawBitmap(
            Bitmap bitmap,
            ReadOnlySpan<PointF> destination,
            RectangleF source,
            float opacity = 1.0f,
            ImageInterpolation interpolation = ImageInterpolation.NearestNeighbor,
            bool tileFlipXY = true)
        {
            if (destination.Length < 3 || !IsValidRectangle(source))
            {
                return;
            }

            using IMapImage image = _surfaceFactory.CreateImage(bitmap, ownsImage: false);
            _surface.DrawImage(
                image,
                destination,
                source,
                new ImageStyle(opacity, interpolation, tileFlipXY));
        }

        /// <summary>
        /// Flushes and releases the backend surface.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_ownsSurface)
            {
                _surface.Dispose();
            }
        }

        /// <summary>
        /// Checks whether a rectangle can be used as an image source or target.
        /// </summary>
        private static bool IsValidRectangle(RectangleF rectangle) =>
            float.IsFinite(rectangle.Left) &&
            float.IsFinite(rectangle.Top) &&
            float.IsFinite(rectangle.Right) &&
            float.IsFinite(rectangle.Bottom) &&
            rectangle.Width > 0.0f &&
            rectangle.Height > 0.0f;
    }
}
