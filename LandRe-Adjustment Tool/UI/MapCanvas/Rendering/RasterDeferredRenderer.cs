using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
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
        private bool _panBufferValid;
        private readonly object _sync = new();
        private readonly List<Bitmap> _retiredBitmaps = [];
        private int _activeFrameLeases;
        private bool _disposed;

        public void Invalidate()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                _cacheValid = false;
                _panBufferValid = false;
                _zoomStartView = null;
            }
        }

        public void Resize(Size canvasSize)
        {
            Size clampedSize = ClampSize(canvasSize);
            lock (_sync)
            {
                ResizeCore(clampedSize);
            }
        }

        public void RenderNow(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine,
            CancellationToken cancellationToken = default)
        {
            Bitmap? renderedBitmap = RenderBitmap(
                canvasSize,
                rasterLayers,
                engine,
                cancellationToken);

            if (renderedBitmap == null)
                return;

            bool swapped = false;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                lock (_sync)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_disposed)
                    {
                        return;
                    }

                    ResizeCore(ClampSize(canvasSize));
                    Bitmap? previousCache = _rasterCache;
                    _rasterCache = renderedBitmap;
                    swapped = true;
                    _cacheValid = true;
                    _panBufferValid = false;
                    _zoomStartView = null;
                    RetireBitmap(previousCache);
                }
            }
            finally
            {
                if (!swapped)
                {
                    renderedBitmap.Dispose();
                }
            }
        }

        public Task RenderAsync(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine,
            CancellationToken cancellationToken = default)
        {
            MapCanvasEngine engineSnapshot = engine.CreateSnapshot();
            IRasterRenderLayer[] layerSnapshot = rasterLayers.ToArray();
            Size sizeSnapshot = ClampSize(canvasSize);

            return Task.Run(
                () =>
                {
                    try
                    {
                        RenderNow(
                            sizeSnapshot,
                            layerSnapshot,
                            engineSnapshot,
                            cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                    }
                },
                cancellationToken);
        }

        public void BeginPan(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine)
        {
            Resize(canvasSize);

            if (_panBuffer == null || _rasterCache == null)
                return;

            lock (_sync)
            {
                if (_disposed)
                    return;

                _panBufferValid = false;

                if (!_cacheValid || _panBuffer == null || _rasterCache == null)
                    return;

                using Graphics graphics = Graphics.FromImage(_panBuffer);
                graphics.Clear(Color.Transparent);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImageUnscaled(_rasterCache, 0, 0);
                _panBufferValid = true;
            }
        }

        public void BeginZoom(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine)
        {
            Resize(canvasSize);

            lock (_sync)
            {
                if (_disposed)
                    return;

                _zoomStartView = _cacheValid && _rasterCache != null
                    ? new RasterViewSnapshot(
                        engine.ViewOriginWorld,
                        engine.ZoomScale,
                        _canvasSize)
                    : null;
            }
        }

        public void EndZoom()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                _zoomStartView = null;
            }
        }

        public bool TryGetCacheFrame(out RasterRenderFrame frame)
        {
            lock (_sync)
            {
                if (_disposed || !_cacheValid || _rasterCache == null)
                {
                    frame = default;
                    return false;
                }

                frame = CreateFrameLease(
                    _rasterCache,
                    new RectangleF(
                        0,
                        0,
                        _rasterCache.Width,
                        _rasterCache.Height));
                return true;
            }
        }

        public bool TryGetPanFrame(PointF totalPanDelta, out RasterRenderFrame frame)
        {
            lock (_sync)
            {
                if (_disposed || !_panBufferValid || _panBuffer == null)
                {
                    frame = default;
                    return false;
                }

                frame = CreateFrameLease(
                    _panBuffer,
                    new RectangleF(
                        totalPanDelta.X,
                        totalPanDelta.Y,
                        _panBuffer.Width,
                        _panBuffer.Height));
                return true;
            }
        }

        public bool TryGetZoomFrame(
            MapCanvasEngine engine,
            out RasterRenderFrame frame)
        {
            lock (_sync)
            {
                if (_disposed ||
                    !_cacheValid ||
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

                RectangleF destination = new(left, top, scaledWidth, scaledHeight);
                if (!IsValidDestination(destination))
                {
                    frame = default;
                    return false;
                }

                frame = CreateFrameLease(
                    _rasterCache,
                    destination);
                return true;
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _disposed = true;
                RetireBitmap(_rasterCache);
                RetireBitmap(_panBuffer);
                _rasterCache = null;
                _panBuffer = null;
                _cacheValid = false;
                _panBufferValid = false;
                _zoomStartView = null;
                DisposeRetiredBitmapsIfSafe();
            }
        }

        private Bitmap? RenderBitmap(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine,
            CancellationToken cancellationToken)
        {
            Size clampedSize = ClampSize(canvasSize);
            Bitmap bitmap = new(
                clampedSize.Width,
                clampedSize.Height,
                PixelFormat.Format32bppPArgb);

            try
            {
                using Graphics graphics = Graphics.FromImage(bitmap);
                graphics.Clear(Color.Transparent);

                if (rasterLayers.Count > 0)
                {
                    ConfigureRasterQuality(graphics);
                    RectangleD visibleBounds = engine.GetVisibleWorldBounds();

                    foreach (IRasterRenderLayer rasterLayer in rasterLayers)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        rasterLayer.RenderVisible(
                            graphics,
                            engine,
                            visibleBounds,
                            interactive: false,
                            cancellationToken);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        private void ResizeCore(Size clampedSize)
        {
            if (_disposed)
                return;

            if (clampedSize == _canvasSize)
                return;

            _canvasSize = clampedSize;
            RetireBitmap(_rasterCache);
            RetireBitmap(_panBuffer);
            _rasterCache = new Bitmap(_canvasSize.Width, _canvasSize.Height, PixelFormat.Format32bppPArgb);
            _panBuffer = new Bitmap(_canvasSize.Width, _canvasSize.Height, PixelFormat.Format32bppPArgb);
            _cacheValid = false;
            _panBufferValid = false;
            _zoomStartView = null;
        }

        private RasterRenderFrame CreateFrameLease(
            Bitmap bitmap,
            RectangleF destination)
        {
            _activeFrameLeases++;
            return new RasterRenderFrame(
                bitmap,
                destination,
                _sync,
                ReleaseFrameLease);
        }

        private void ReleaseFrameLease()
        {
            lock (_sync)
            {
                if (_activeFrameLeases > 0)
                {
                    _activeFrameLeases--;
                }

                DisposeRetiredBitmapsIfSafe();
            }
        }

        private void RetireBitmap(Bitmap? bitmap)
        {
            if (bitmap == null)
            {
                return;
            }

            if (_activeFrameLeases > 0)
            {
                _retiredBitmaps.Add(bitmap);
                return;
            }

            bitmap.Dispose();
        }

        private void DisposeRetiredBitmapsIfSafe()
        {
            if (_activeFrameLeases > 0)
            {
                return;
            }

            foreach (Bitmap bitmap in _retiredBitmaps)
            {
                bitmap.Dispose();
            }

            _retiredBitmaps.Clear();
        }

        private static bool IsValidDestination(RectangleF destination)
        {
            return IsFinite(destination.Left) &&
                   IsFinite(destination.Top) &&
                   IsFinite(destination.Width) &&
                   IsFinite(destination.Height) &&
                   destination.Width > 0 &&
                   destination.Height > 0;
        }

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        private static void ConfigureRasterQuality(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
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
