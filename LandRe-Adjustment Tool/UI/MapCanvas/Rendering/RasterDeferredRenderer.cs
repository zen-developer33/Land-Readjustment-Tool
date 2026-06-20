using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;

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
        private Dictionary<int, Bitmap> _layerCaches = new Dictionary<int, Bitmap>();
        private bool _cacheValid;
        private bool _panBufferValid;
        private readonly object _sync = new();
        private readonly List<Bitmap> _retiredBitmaps = new List<Bitmap>();
        private int _activeFrameLeases;
        private bool _disposed;
        private double _lastRefreshElapsedMs;

        public void Invalidate()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                _cacheValid = false;
                _panBufferValid = false;
                _zoomStartView = null;
                RetireLayerCachesCore();
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

        public bool RenderNow(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine,
            MapRenderBackend renderBackend = MapRenderBackend.GdiPlus,
            CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            RasterRenderResult? renderResult = RenderBitmap(
                canvasSize,
                rasterLayers,
                engine,
                renderBackend,
                cancellationToken);

            if (renderResult == null)
                return false;

            bool swapped = false;
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                lock (_sync)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    if (_disposed)
                    {
                        return false;
                    }

                    ResizeCore(ClampSize(canvasSize));
                    Bitmap? previousCache = _rasterCache;
                    Dictionary<int, Bitmap> previousLayerCaches = _layerCaches;
                    _rasterCache = renderResult.CompositeBitmap;
                    _layerCaches = renderResult.LayerCaches;
                    swapped = true;
                    _cacheValid = true;
                    _panBufferValid = false;
                    _zoomStartView = null;
                    _lastRefreshElapsedMs = stopwatch.Elapsed.TotalMilliseconds;
                    RetireBitmap(previousCache);
                    RetireLayerCaches(previousLayerCaches);
                    return true;
                }
            }
            finally
            {
                if (!swapped)
                {
                    renderResult.Dispose();
                }
            }
        }

        public DeferredRendererDebugState GetDebugState()
        {
            lock (_sync)
            {
                return new DeferredRendererDebugState(
                    _cacheValid,
                    _panBufferValid,
                    _zoomStartView.HasValue,
                    _canvasSize,
                    _layerCaches.Count,
                    _activeFrameLeases,
                    _retiredBitmaps.Count,
                    _lastRefreshElapsedMs);
            }
        }

        public bool TryRecomposeFromLayerCaches(
            IReadOnlyList<IRasterRenderLayer> rasterLayers)
        {
            ArgumentNullException.ThrowIfNull(rasterLayers);

            lock (_sync)
            {
                if (_disposed || _canvasSize.Width <= 0 || _canvasSize.Height <= 0)
                {
                    return false;
                }

                bool hasVisibleLayer = rasterLayers.Any(layer => layer.IsVisible);
                if (hasVisibleLayer &&
                    rasterLayers.Any(layer =>
                        layer.IsVisible && !_layerCaches.ContainsKey(layer.LayerId)))
                {
                    return false;
                }

                Bitmap recomposed = new(
                    _canvasSize.Width,
                    _canvasSize.Height,
                    PixelFormat.Format32bppPArgb);

                try
                {
                    using Graphics graphics = Graphics.FromImage(recomposed);
                    ClearSurface(graphics, Color.Transparent);
                    ConfigureRasterQuality(graphics);

                    foreach (IRasterRenderLayer rasterLayer in rasterLayers)
                    {
                        if (!rasterLayer.IsVisible ||
                            !_layerCaches.TryGetValue(
                                rasterLayer.LayerId,
                                out Bitmap? layerBitmap))
                        {
                            continue;
                        }

                        DrawBitmapUnscaled(graphics, layerBitmap);
                    }

                    Bitmap? previousCache = _rasterCache;
                    _rasterCache = recomposed;
                    _cacheValid = true;
                    _panBufferValid = false;
                    _zoomStartView = null;
                    RetireBitmap(previousCache);
                    return true;
                }
                catch
                {
                    recomposed.Dispose();
                    throw;
                }
            }
        }

        public Task<bool> RenderAsync(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine,
            MapRenderBackend renderBackend = MapRenderBackend.GdiPlus,
            CancellationToken cancellationToken = default)
        {
            MapCanvasEngine engineSnapshot = engine.CreateSnapshot();
            IRasterRenderLayer[] layerSnapshot = rasterLayers.ToArray();
            Size sizeSnapshot = ClampSize(canvasSize);

            return Task.Run(
                () => RenderNow(
                    sizeSnapshot,
                    layerSnapshot,
                    engineSnapshot,
                    renderBackend,
                    cancellationToken));
        }

        public bool BeginPan(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine)
        {
            Resize(canvasSize);

            if (_panBuffer == null || _rasterCache == null)
                return false;

            lock (_sync)
            {
                if (_disposed)
                    return false;

                _panBufferValid = false;

                if (!_cacheValid || _panBuffer == null || _rasterCache == null)
                    return false;

                using Graphics graphics = Graphics.FromImage(_panBuffer);
                ClearSurface(graphics, Color.Transparent);
                graphics.CompositingMode = CompositingMode.SourceOver;
                ConfigureRasterQuality(graphics);
                if (TryCalculateZoomDestination(engine, out RectangleF zoomDestination))
                {
                    DrawBitmap(graphics, _rasterCache, zoomDestination);
                }
                else
                {
                    DrawBitmapUnscaled(graphics, _rasterCache);
                }

                _panBufferValid = true;
                return true;
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

                if (!TryCalculateZoomDestination(engine, out RectangleF destination))
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

        private bool TryCalculateZoomDestination(
            MapCanvasEngine engine,
            out RectangleF destination)
        {
            destination = default;

            if (_rasterCache == null ||
                !_zoomStartView.HasValue)
            {
                return false;
            }

            RasterViewSnapshot start = _zoomStartView.Value;
            if (start.ZoomScale <= 0)
            {
                return false;
            }

            double scale = engine.ZoomScale / start.ZoomScale;
            if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
            {
                return false;
            }

            float left = (float)((start.ViewOriginWorld.X - engine.ViewOriginWorld.X) * engine.ZoomScale);
            float top = (float)(
                start.CanvasSize.Height * (1.0 - scale) -
                ((start.ViewOriginWorld.Y - engine.ViewOriginWorld.Y) * engine.ZoomScale));
            float scaledWidth = (float)(_rasterCache.Width * scale);
            float scaledHeight = (float)(_rasterCache.Height * scale);

            destination = new RectangleF(left, top, scaledWidth, scaledHeight);
            return IsValidDestination(destination);
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
                RetireLayerCachesCore();
                DisposeRetiredBitmapsIfSafe();
            }
        }

        private RasterRenderResult? RenderBitmap(
            Size canvasSize,
            IReadOnlyList<IRasterRenderLayer> rasterLayers,
            MapCanvasEngine engine,
            MapRenderBackend renderBackend,
            CancellationToken cancellationToken)
        {
            Size clampedSize = ClampSize(canvasSize);
            Bitmap compositeBitmap = new(
                clampedSize.Width,
                clampedSize.Height,
                PixelFormat.Format32bppPArgb);
            Dictionary<int, Bitmap> layerCaches = [];

            try
            {
                using Graphics graphics = Graphics.FromImage(compositeBitmap);
                ClearSurface(graphics, Color.Transparent);
                ConfigureRasterQuality(graphics);

                if (rasterLayers.Count > 0)
                {
                    RectangleD visibleBounds = engine.GetVisibleWorldBounds();

                    foreach (IRasterRenderLayer rasterLayer in rasterLayers)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            compositeBitmap.Dispose();
                            DisposeLayerCaches(layerCaches);
                            return null;
                        }

                        if (!rasterLayer.IsVisible)
                        {
                            continue;
                        }

                        Bitmap layerBitmap = new(
                            clampedSize.Width,
                            clampedSize.Height,
                            PixelFormat.Format32bppPArgb);

                        bool layerRendered = false;
                        try
                        {
                            using Graphics layerGraphics =
                                Graphics.FromImage(layerBitmap);
                            ClearSurface(layerGraphics, Color.Transparent);
                            ConfigureRasterQuality(layerGraphics);

                            layerRendered = rasterLayer.RenderVisible(
                                layerGraphics,
                                engine,
                                visibleBounds,
                                interactive: false,
                                renderBackend,
                                cancellationToken);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                compositeBitmap.Dispose();
                                layerBitmap.Dispose();
                                DisposeLayerCaches(layerCaches);
                                return null;
                            }

                            if (layerRendered)
                            {
                                DrawBitmapUnscaled(graphics, layerBitmap);
                                layerCaches[rasterLayer.LayerId] = layerBitmap;
                            }
                            else if (rasterLayer is XyzLiveTileRenderLayer)
                            {
                                compositeBitmap.Dispose();
                                DisposeLayerCaches(layerCaches);
                                return null;
                            }
                        }
                        finally
                        {
                            if (!layerRendered)
                            {
                                layerBitmap.Dispose();
                            }
                        }
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    compositeBitmap.Dispose();
                    DisposeLayerCaches(layerCaches);
                    return null;
                }

                return new RasterRenderResult(compositeBitmap, layerCaches);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                compositeBitmap.Dispose();
                DisposeLayerCaches(layerCaches);
                return null;
            }
            catch
            {
                compositeBitmap.Dispose();
                DisposeLayerCaches(layerCaches);
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
            RetireLayerCachesCore();
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

        private void RetireLayerCachesCore()
        {
            Dictionary<int, Bitmap> previousLayerCaches = _layerCaches;
            _layerCaches = [];
            RetireLayerCaches(previousLayerCaches);
        }

        private void RetireLayerCaches(Dictionary<int, Bitmap> layerCaches)
        {
            foreach (Bitmap bitmap in layerCaches.Values)
            {
                RetireBitmap(bitmap);
            }

            layerCaches.Clear();
        }

        private static void DisposeLayerCaches(Dictionary<int, Bitmap> layerCaches)
        {
            foreach (Bitmap bitmap in layerCaches.Values)
            {
                bitmap.Dispose();
            }

            layerCaches.Clear();
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
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.CompositingMode = CompositingMode.SourceOver;
        }

        private static Size ClampSize(Size size)
        {
            return new Size(
                Math.Max(1, size.Width),
                Math.Max(1, size.Height));
        }

        /// <summary>
        /// Clears an internal raster cache target through the backend surface
        /// abstraction.
        /// </summary>
        private static void ClearSurface(Graphics graphics, Color color)
        {
            using GdiMapRenderSurface surface = CreateSurface(graphics);
            surface.Clear(color);
        }

        /// <summary>
        /// Draws a cached bitmap at native size through the backend image path.
        /// </summary>
        private static void DrawBitmapUnscaled(Graphics graphics, Bitmap bitmap)
        {
            DrawBitmap(
                graphics,
                bitmap,
                new RectangleF(0, 0, bitmap.Width, bitmap.Height));
        }

        /// <summary>
        /// Draws a cached bitmap into the requested destination rectangle
        /// through the backend image path.
        /// </summary>
        private static void DrawBitmap(
            Graphics graphics,
            Bitmap bitmap,
            RectangleF destination)
        {
            using GdiMapRenderSurface surface = CreateSurface(graphics);
            using GdiMapImage image = new(bitmap);
            surface.DrawImage(
                image,
                destination,
                new RectangleF(0, 0, bitmap.Width, bitmap.Height),
                new ImageStyle(1.0f, ImageInterpolation.NearestNeighbor));
        }

        /// <summary>
        /// Creates the current GDI-backed render surface for an internal raster
        /// cache bitmap target.
        /// </summary>
        private static GdiMapRenderSurface CreateSurface(Graphics graphics) =>
            new(graphics, Size.Round(graphics.VisibleClipBounds.Size));

        private readonly record struct RasterViewSnapshot(
            PointD ViewOriginWorld,
            double ZoomScale,
            Size CanvasSize);

        private sealed class RasterRenderResult(
            Bitmap compositeBitmap,
            Dictionary<int, Bitmap> layerCaches) : IDisposable
        {
            public Bitmap CompositeBitmap { get; } = compositeBitmap;

            public Dictionary<int, Bitmap> LayerCaches { get; } = layerCaches;

            public void Dispose()
            {
                CompositeBitmap.Dispose();
                DisposeLayerCaches(LayerCaches);
            }
        }
    }
}
