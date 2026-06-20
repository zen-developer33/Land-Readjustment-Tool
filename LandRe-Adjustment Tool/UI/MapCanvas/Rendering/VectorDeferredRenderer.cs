using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;

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
        private readonly List<Bitmap> _retiredBitmaps = new List<Bitmap>();
        private Bitmap? _vectorCache;
        private Bitmap? _panBuffer;
        private Bitmap? _zoomBuffer;
        private Size _canvasSize;
        private ViewSnapshot? _zoomStartView;
        private bool _cacheValid;
        private bool _panBufferValid;
        private bool _zoomBufferValid;
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
                _zoomBufferValid = false;
                _zoomStartView = null;
            }
        }

        public bool HasValidCache
        {
            get
            {
                lock (_sync)
                {
                    return !_disposed && _cacheValid && _vectorCache != null;
                }
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
            bool antiAliasingEnabled,
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
                ClearSurface(graphics, Color.Transparent);
                ConfigureVectorCacheQuality(
                    graphics,
                    vectorRenderer.FeatureCount,
                    antiAliasingEnabled);

                if (!cancellationToken.IsCancellationRequested)
                {
                    RectangleD visibleBounds = engine.GetVisibleWorldBounds();
                    vectorRenderer.Render(
                        graphics,
                        engine,
                        visibleBounds,
                        useLevelOfDetail: vectorRenderer.FeatureCount > LevelOfDetailThreshold,
                        canvasSize: clampedSize,
                        antiAliasingEnabled: antiAliasingEnabled);
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
                    _zoomBufferValid && _zoomStartView.HasValue,
                    _canvasSize,
                    0,
                    _activeFrameLeases,
                    _retiredBitmaps.Count,
                    _lastRefreshElapsedMs);
            }
        }

        public bool BeginPan(Size canvasSize, Action<Graphics>? renderOverlay = null)
        {
            Resize(canvasSize);

            lock (_sync)
            {
                if (_disposed)
                {
                    return false;
                }

                _panBufferValid = false;
                if (!_cacheValid || _vectorCache == null || _panBuffer == null)
                {
                    return false;
                }

                using Graphics graphics = Graphics.FromImage(_panBuffer);
                ClearSurface(graphics, Color.Transparent);
                graphics.CompositingMode = CompositingMode.SourceOver;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                DrawBitmapUnscaled(graphics, _vectorCache);
                renderOverlay?.Invoke(graphics);
                _panBufferValid = true;
                return true;
            }
        }

        public bool BeginZoom(
            Size canvasSize,
            MapCanvasEngine engine,
            Action<Graphics>? renderOverlay = null)
        {
            Resize(canvasSize);

            lock (_sync)
            {
                if (_disposed)
                    return false;

                _zoomBufferValid = false;
                if (!_cacheValid || _vectorCache == null || _zoomBuffer == null)
                {
                    _zoomStartView = null;
                    return false;
                }

                using Graphics graphics = Graphics.FromImage(_zoomBuffer);
                ClearSurface(graphics, Color.Transparent);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                DrawBitmapUnscaled(graphics, _vectorCache);
                graphics.CompositingMode = CompositingMode.SourceOver;
                renderOverlay?.Invoke(graphics);

                _zoomStartView = new ViewSnapshot(
                    engine.ViewOriginWorld,
                    engine.ZoomScale,
                    _canvasSize);
                _zoomBufferValid = true;
                return true;
            }
        }

        public void EndZoom()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                _zoomStartView = null;
                _zoomBufferValid = false;
            }
        }

        public bool TryGetZoomFrame(MapCanvasEngine engine, out RasterRenderFrame frame)
        {
            lock (_sync)
            {
                if (_disposed ||
                    !_cacheValid ||
                    !_zoomBufferValid ||
                    _zoomBuffer == null ||
                    !_zoomStartView.HasValue)
                {
                    frame = default;
                    return false;
                }

                if (!TryCalculateZoomDestination(engine, out RectangleF destination))
                {
                    frame = default;
                    return false;
                }

                frame = CreateFrameLease(_zoomBuffer, destination);
                return true;
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
                RetireBitmap(_zoomBuffer);
                _vectorCache = null;
                _panBuffer = null;
                _zoomBuffer = null;
                _zoomStartView = null;
                _cacheValid = false;
                _panBufferValid = false;
                _zoomBufferValid = false;
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
            RetireBitmap(_zoomBuffer);
            _vectorCache = new Bitmap(
                _canvasSize.Width,
                _canvasSize.Height,
                PixelFormat.Format32bppPArgb);
            _panBuffer = new Bitmap(
                _canvasSize.Width,
                _canvasSize.Height,
                PixelFormat.Format32bppPArgb);
            _zoomBuffer = new Bitmap(
                _canvasSize.Width,
                _canvasSize.Height,
                PixelFormat.Format32bppPArgb);
            _cacheValid = false;
            _panBufferValid = false;
            _zoomBufferValid = false;
            _zoomStartView = null;
        }

        private bool TryCalculateZoomDestination(
            MapCanvasEngine engine,
            out RectangleF destination)
        {
            destination = default;

            if (_zoomBuffer == null || !_zoomStartView.HasValue)
                return false;

            ViewSnapshot start = _zoomStartView.Value;
            if (start.ZoomScale <= 0)
                return false;

            double scale = engine.ZoomScale / start.ZoomScale;
            if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
                return false;

            float left = (float)((start.ViewOriginWorld.X - engine.ViewOriginWorld.X) * engine.ZoomScale);
            float top = (float)(
                start.CanvasSize.Height * (1.0 - scale) -
                ((start.ViewOriginWorld.Y - engine.ViewOriginWorld.Y) * engine.ZoomScale));
            float scaledWidth = (float)(_zoomBuffer.Width * scale);
            float scaledHeight = (float)(_zoomBuffer.Height * scale);

            destination = new RectangleF(left, top, scaledWidth, scaledHeight);
            return float.IsFinite(destination.X) &&
                   float.IsFinite(destination.Y) &&
                   float.IsFinite(destination.Width) &&
                   float.IsFinite(destination.Height) &&
                   destination.Width > 0 &&
                   destination.Height > 0;
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

        private readonly record struct ViewSnapshot(
            PointD ViewOriginWorld,
            double ZoomScale,
            Size CanvasSize);

        private static void ConfigureVectorCacheQuality(
            Graphics graphics,
            int featureCount,
            bool antiAliasingEnabled)
        {
            if (!antiAliasingEnabled)
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.CompositingMode = CompositingMode.SourceOver;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                return;
            }

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Match the aliased branch (no pixel offset). Vector coordinates are
            // integer-snapped in ToScreenPointF, so Half/HighQuality would shift
            // baked shapes half a pixel north-west versus the AA-off rendering and
            // make them jump when AA is toggled. None keeps the cache aligned.
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            // Use the same compositing quality the live/direct draw uses so a
            // line baked into the cache has the SAME apparent weight as the live
            // preview. AssumeLinear blends antialiased edges differently (bolder),
            // which made the live preview look thinner than the cached shapes.
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        }

        private static Size ClampSize(Size size)
        {
            return new Size(
                Math.Max(1, size.Width),
                Math.Max(1, size.Height));
        }

        /// <summary>
        /// Clears an internally owned cache bitmap through the backend surface
        /// abstraction.
        /// </summary>
        private static void ClearSurface(Graphics graphics, Color color)
        {
            using GdiMapRenderSurface surface = CreateSurface(graphics);
            surface.Clear(color);
        }

        /// <summary>
        /// Draws a cached bitmap at its native size through the backend image
        /// abstraction.
        /// </summary>
        private static void DrawBitmapUnscaled(Graphics graphics, Bitmap bitmap)
        {
            using GdiMapRenderSurface surface = CreateSurface(graphics);
            using GdiMapImage image = new(bitmap);
            surface.DrawImage(
                image,
                new RectangleF(0, 0, bitmap.Width, bitmap.Height),
                new RectangleF(0, 0, bitmap.Width, bitmap.Height),
                new ImageStyle(1.0f, ImageInterpolation.NearestNeighbor));
        }

        /// <summary>
        /// Creates the current GDI-backed render surface for an internal cache
        /// bitmap target.
        /// </summary>
        private static GdiMapRenderSurface CreateSurface(Graphics graphics) =>
            new(graphics, Size.Round(graphics.VisibleClipBounds.Size));
    }
}
