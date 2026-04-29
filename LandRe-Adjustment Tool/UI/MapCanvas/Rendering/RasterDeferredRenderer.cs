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
        private RasterViewSnapshot? _zoomStartView;
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

        public void BeginZoom(
            Size canvasSize,
            IReadOnlyList<RasterRenderLayer> rasterLayers,
            MapCanvasEngine engine)
        {
            Resize(canvasSize);

            if (!_cacheValid)
            {
                RenderNow(canvasSize, rasterLayers, engine);
            }

            _zoomStartView = new RasterViewSnapshot(
                engine.ViewOriginWorld,
                engine.ZoomScale,
                _canvasSize);
        }

        public void EndZoom()
        {
            _zoomStartView = null;
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
            MapCanvasEngine engine,
            out RasterRenderFrame frame)
        {
            if (!_cacheValid ||
                _rasterCache == null ||
                !_zoomStartView.HasValue)
            {
                frame = default;
                return false;
            }

            RasterViewSnapshot start = _zoomStartView.Value;
            if (start.ZoomScale <= 0)
            {
                frame = default;
                return false;
            }

            double scale = engine.ZoomScale / start.ZoomScale;
            if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
            {
                frame = default;
                return false;
            }

            float left = (float)((start.ViewOriginWorld.X - engine.ViewOriginWorld.X) * engine.ZoomScale);
            float top = (float)(
                start.CanvasSize.Height * (1.0 - scale) -
                ((start.ViewOriginWorld.Y - engine.ViewOriginWorld.Y) * engine.ZoomScale));
            float scaledWidth = (float)(_rasterCache.Width * scale);
            float scaledHeight = (float)(_rasterCache.Height * scale);

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

        private readonly record struct RasterViewSnapshot(
            PointD ViewOriginWorld,
            double ZoomScale,
            Size CanvasSize);
    }
}
