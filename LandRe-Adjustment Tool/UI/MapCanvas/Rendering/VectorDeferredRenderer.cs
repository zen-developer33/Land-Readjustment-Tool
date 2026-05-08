using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Caches the vector feature stack as a transparent bitmap and shifts that
    /// bitmap during pan. This keeps interaction frames cheap while preserving
    /// precise vector rendering after the viewport settles.
    /// </summary>
    public sealed class VectorDeferredRenderer : IDisposable
    {
        private const int LevelOfDetailThreshold = 20_000;
        private readonly object _sync = new();
        private readonly List<Bitmap> _retiredBitmaps = [];
        private Bitmap? _vectorCache;
        private Bitmap? _panBuffer;
        private Size _canvasSize;
        private bool _cacheValid;
        private bool _panBufferValid;
        private bool _disposed;
        private int _activeFrameLeases;
        private double _lastRefreshElapsedMs;

        public void Invalidate()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _cacheValid = false;
                _panBufferValid = false;
            }
        }

        public void Resize(Size canvasSize)
        {
            lock (_sync)
            {
                ResizeCore(ClampSize(canvasSize));
            }
        }

        public bool RenderNow(
            Size canvasSize,
            CanvasVectorRenderer vectorRenderer,
            MapCanvasEngine engine,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(vectorRenderer);
            ArgumentNullException.ThrowIfNull(engine);

            Size clampedSize = ClampSize(canvasSize);
            Bitmap bitmap = new(
                clampedSize.Width,
                clampedSize.Height,
                PixelFormat.Format32bppPArgb);
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                using Graphics graphics = Graphics.FromImage(bitmap);
                graphics.Clear(Color.Transparent);
                ConfigureVectorCacheQuality(graphics, vectorRenderer.FeatureCount);

                if (!cancellationToken.IsCancellationRequested)
                {
                    RectangleD visibleBounds = engine.GetVisibleWorldBounds();
                    vectorRenderer.Render(
                        graphics,
                        engine,
                        visibleBounds,
                        useLevelOfDetail: vectorRenderer.FeatureCount > LevelOfDetailThreshold,
                        canvasSize: clampedSize);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    bitmap.Dispose();
                    return false;
                }

                lock (_sync)
                {
                    if (_disposed)
                    {
                        bitmap.Dispose();
                        return false;
                    }

                    ResizeCore(clampedSize);
                    Bitmap? previousCache = _vectorCache;
                    _vectorCache = bitmap;
                    _cacheValid = true;
                    _panBufferValid = false;
                    _lastRefreshElapsedMs = stopwatch.Elapsed.TotalMilliseconds;
                    RetireBitmap(previousCache);
                    return true;
                }
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        public DeferredRendererDebugState GetDebugState()
        {
            lock (_sync)
            {
                return new DeferredRendererDebugState(
                    _cacheValid,
                    _panBufferValid,
                    false,
                    _canvasSize,
                    0,
                    _activeFrameLeases,
                    _retiredBitmaps.Count,
                    _lastRefreshElapsedMs);
            }
        }

        public void BeginPan(Size canvasSize)
        {
            Resize(canvasSize);

            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _panBufferValid = false;
                if (!_cacheValid || _vectorCache == null || _panBuffer == null)
                {
                    return;
                }

                using Graphics graphics = Graphics.FromImage(_panBuffer);
                graphics.Clear(Color.Transparent);
                graphics.CompositingMode = CompositingMode.SourceOver;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.DrawImageUnscaled(_vectorCache, 0, 0);
                _panBufferValid = true;
            }
        }

        public bool TryGetCacheFrame(out RasterRenderFrame frame)
        {
            lock (_sync)
            {
                if (_disposed || !_cacheValid || _vectorCache == null)
                {
                    frame = default;
                    return false;
                }

                frame = CreateFrameLease(
                    _vectorCache,
                    new RectangleF(0, 0, _vectorCache.Width, _vectorCache.Height));
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

        public void Dispose()
        {
            lock (_sync)
            {
                _disposed = true;
                RetireBitmap(_vectorCache);
                RetireBitmap(_panBuffer);
                _vectorCache = null;
                _panBuffer = null;
                _cacheValid = false;
                _panBufferValid = false;
                DisposeRetiredBitmapsIfSafe();
            }
        }

        private void ResizeCore(Size clampedSize)
        {
            if (_disposed || clampedSize == _canvasSize)
            {
                return;
            }

            _canvasSize = clampedSize;
            RetireBitmap(_vectorCache);
            RetireBitmap(_panBuffer);
            _vectorCache = new Bitmap(
                _canvasSize.Width,
                _canvasSize.Height,
                PixelFormat.Format32bppPArgb);
            _panBuffer = new Bitmap(
                _canvasSize.Width,
                _canvasSize.Height,
                PixelFormat.Format32bppPArgb);
            _cacheValid = false;
            _panBufferValid = false;
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

        private static void ConfigureVectorCacheQuality(Graphics graphics, int featureCount)
        {
            if (featureCount <= 1_000)
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
            }
            else
            {
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
            }

            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.CompositingMode = CompositingMode.SourceOver;
        }

        private static Size ClampSize(Size size)
        {
            return new Size(
                Math.Max(1, size.Width),
                Math.Max(1, size.Height));
        }
    }
}
