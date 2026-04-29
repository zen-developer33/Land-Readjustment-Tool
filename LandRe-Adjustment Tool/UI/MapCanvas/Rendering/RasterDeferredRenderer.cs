using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Caches the raster layer stack as a screen bitmap, then blits that
    /// bitmap during pan. This mirrors the legacy deferred renderer pattern.
    /// </summary>
    public sealed class RasterDeferredRenderer : IDisposable
    {
        private Bitmap? _rasterCache;
        private Bitmap? _panBuffer;
        private Size _canvasSize;
        private bool _cacheValid;

        public void Invalidate()
        {
            _cacheValid = false;
        }

        public void Resize(Size canvasSize)
        {
            Size clampedSize = ClampSize(canvasSize);
            if (clampedSize == _canvasSize)
                return;

            _canvasSize = clampedSize;
            _rasterCache?.Dispose();
            _panBuffer?.Dispose();
            _rasterCache = new Bitmap(_canvasSize.Width, _canvasSize.Height, PixelFormat.Format32bppArgb);
            _panBuffer = new Bitmap(_canvasSize.Width, _canvasSize.Height, PixelFormat.Format32bppArgb);
            _cacheValid = false;
        }

        public void RenderNow(
            Size canvasSize,
            IReadOnlyList<RasterRenderLayer> rasterLayers,
            MapCanvasEngine engine)
        {
            Resize(canvasSize);

            if (_rasterCache == null)
                return;

            using Graphics graphics = Graphics.FromImage(_rasterCache);
            graphics.Clear(Color.Transparent);

            if (rasterLayers.Count > 0)
            {
                ConfigureHighQuality(graphics);
                RectangleD visibleBounds = engine.GetVisibleWorldBounds();

                foreach (RasterRenderLayer rasterLayer in rasterLayers)
                {
                    rasterLayer.RenderVisible(
                        graphics,
                        engine,
                        visibleBounds,
                        interactive: false);
                }
            }

            _cacheValid = true;
        }

        public void BeginPan(
            Size canvasSize,
            IReadOnlyList<RasterRenderLayer> rasterLayers,
            MapCanvasEngine engine)
        {
            Resize(canvasSize);

            if (!_cacheValid)
            {
                RenderNow(canvasSize, rasterLayers, engine);
            }

            if (_panBuffer == null || _rasterCache == null)
                return;

            using Graphics graphics = Graphics.FromImage(_panBuffer);
            graphics.Clear(Color.Transparent);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImageUnscaled(_rasterCache, 0, 0);
        }

        public bool TryGetCacheFrame(out RasterRenderFrame frame)
        {
            if (!_cacheValid || _rasterCache == null)
            {
                frame = default;
                return false;
            }

            frame = new RasterRenderFrame(
                _rasterCache,
                new RectangleF(
                    0,
                    0,
                    _rasterCache.Width,
                    _rasterCache.Height));
            return true;
        }

        public bool TryGetPanFrame(PointF totalPanDelta, out RasterRenderFrame frame)
        {
            if (_panBuffer == null)
            {
                frame = default;
                return false;
            }

            frame = new RasterRenderFrame(
                _panBuffer,
                new RectangleF(
                    totalPanDelta.X,
                    totalPanDelta.Y,
                    _panBuffer.Width,
                    _panBuffer.Height));
            return true;
        }

        public bool TryGetZoomFrame(
            PointF anchor,
            double scale,
            out RasterRenderFrame frame)
        {
            if (!_cacheValid || _rasterCache == null)
            {
                frame = default;
                return false;
            }

            float scaledWidth = (float)(_rasterCache.Width * scale);
            float scaledHeight = (float)(_rasterCache.Height * scale);
            float left = anchor.X - (float)((anchor.X) * scale);
            float top = anchor.Y - (float)((anchor.Y) * scale);

            frame = new RasterRenderFrame(
                _rasterCache,
                new RectangleF(left, top, scaledWidth, scaledHeight));
            return true;
        }

        public void Dispose()
        {
            _rasterCache?.Dispose();
            _panBuffer?.Dispose();
        }

        private static void ConfigureHighQuality(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
        }

        private static Size ClampSize(Size size)
        {
            return new Size(
                Math.Max(1, size.Width),
                Math.Max(1, size.Height));
        }
    }
}
