using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using OSGeo.OSR;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Renders a live XYZ/TMS tile layer by fetching tiles on demand from the internet.
    /// Uses an LRU memory cache (512 tiles), a persistent disk cache, a bounded
    /// semaphore (6 concurrent HTTP requests), per-viewport cancellation, and an
    /// 80 ms debounce so fetches only start once the viewport settles.
    /// </summary>
    internal sealed class XyzLiveTileRenderLayer : IRasterRenderLayer
    {
        // ── Constants ──────────────────────────────────────────────────────────
        private const int TilePixelSize = 256;
        private const int MaxCachedTiles = 512;
        private const int MaxTilesPerFrame = 256;
        private const int MaxConcurrentFetches = 6;
        private const int DebounceMilliseconds = 80;
        private const int MaxSupportedZoom = 22;
        private const double WebMercatorExtent = 20037508.342789244;
        private const double WebMercatorWorldSize = WebMercatorExtent * 2.0;
        private const double InitialResolution = WebMercatorWorldSize / TilePixelSize;
        private const string WebMercatorSrsDefinition = "EPSG:3857";

        // ── Process-wide shared resources ──────────────────────────────────────
        private static readonly HttpClient SharedHttpClient = CreateSharedHttpClient();
        private static readonly SemaphoreSlim FetchSemaphore =
            new SemaphoreSlim(MaxConcurrentFetches, MaxConcurrentFetches);

        // ── Per-instance synchronization ───────────────────────────────────────
        /// <summary>
        /// Guards all mutable state: render state, tile cache, debounce fields.
        /// Background threads lock this only briefly (cache insert, pending-set update)
        /// so the render thread is never blocked by an in-flight HTTP request.
        /// </summary>
        private readonly object _renderSync = new();

        // ── Coordinate transforms ──────────────────────────────────────────────
        private readonly SpatialReference _webMercatorSrs;
        private readonly SpatialReference _projectSrs;
        private readonly CoordinateTransformation _webMercatorToProject;
        private readonly CoordinateTransformation _projectToWebMercator;
        private readonly bool _projectIsWebMercator;

        // ── LRU tile cache ─────────────────────────────────────────────────────
        private readonly Dictionary<TileKey, Bitmap> _tileCache = [];
        private readonly LinkedList<TileKey> _tileLru = [];
        private readonly Dictionary<TileKey, LinkedListNode<TileKey>> _tileLruNodes = [];
        private readonly HashSet<TileKey> _pendingFetches = [];
        private readonly Dictionary<TileKey, RectangleD> _projectTileBoundsCache = [];

        // ── Tile source ────────────────────────────────────────────────────────
        private readonly string _urlTemplate;
        private readonly string _diskCacheRoot;

        // ── Tile-ready callback ────────────────────────────────────────────────
        private readonly Action? _invalidateCallback;

        // ── Debounce and viewport tracking ────────────────────────────────────
        private CancellationTokenSource _viewportCts = new CancellationTokenSource();
        private readonly System.Threading.Timer _debounceTimer;
        private RectangleD _lastWebMercatorBounds;
        private int _lastZoom;

        // ── Opacity ────────────────────────────────────────────────────────────
        private ImageAttributes? _opacityImageAttributes;

        // ── Dispose guard ──────────────────────────────────────────────────────
        private volatile bool _disposed;

        private XyzLiveTileRenderLayer(
            CanvasLayer layer,
            string filePath,
            string urlTemplate,
            string diskCacheRoot,
            RectangleD worldBounds,
            SpatialReference webMercatorSrs,
            SpatialReference projectSrs,
            CoordinateTransformation webMercatorToProject,
            CoordinateTransformation projectToWebMercator,
            bool projectIsWebMercator,
            Action? invalidateCallback)
        {
            LayerId = layer.Id;
            Name = layer.Name;
            FilePath = filePath;
            Transparency = Math.Clamp(layer.FillTransparency, 0, 100);
            IsVisible = layer.IsVisible;
            WorldBounds = worldBounds;
            _urlTemplate = urlTemplate;
            _diskCacheRoot = diskCacheRoot;
            _webMercatorSrs = webMercatorSrs;
            _projectSrs = projectSrs;
            _webMercatorToProject = webMercatorToProject;
            _projectToWebMercator = projectToWebMercator;
            _projectIsWebMercator = projectIsWebMercator;
            _invalidateCallback = invalidateCallback;
            _debounceTimer = new System.Threading.Timer(
                OnDebounceElapsed,
                state: null,
                dueTime: Timeout.Infinite,
                period: Timeout.Infinite);
            UpdateOpacityAttributes();
        }

        // ── IRasterRenderLayer properties ──────────────────────────────────────

        public int LayerId { get; }
        public string Name { get; }
        public string FilePath { get; }
        public RectangleD WorldBounds { get; }
        public int Transparency { get; private set; }
        public bool IsVisible { get; private set; }

        // ── Factory helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="filePath"/> is a VRT file
        /// that wraps a GDAL WMS/TMS network-service descriptor, as produced by the live
        /// tile import path.
        /// </summary>
        public static bool IsLiveTileVrtPath(string filePath)
        {
            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".vrt",
                    StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                string content = File.ReadAllText(filePath);
                return content.Contains(".gdal-wms", StringComparison.OrdinalIgnoreCase) ||
                       content.Contains("GDAL_WMS", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a live tile render layer from a canvas layer whose source file is a
        /// GDAL VRT that wraps a WMS/TMS network service descriptor.
        /// </summary>
        /// <param name="layer">Canvas layer metadata (name, id, transparency, …).</param>
        /// <param name="filePath">Absolute path to the .vrt file.</param>
        /// <param name="invalidateCallback">
        /// Optional action invoked from a background thread when a tile finishes loading
        /// so the hosting control can schedule a repaint (e.g. <c>BeginInvoke(RequestRender)</c>).
        /// </param>
        public static XyzLiveTileRenderLayer FromCanvasLayer(
            CanvasLayer layer,
            string filePath,
            Action? invalidateCallback = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Live tile VRT for layer '{layer.Name}' was not found.",
                    filePath);
            }

            string? wmsXmlPath = FindVrtSourcePath(filePath);
            if (wmsXmlPath == null || !File.Exists(wmsXmlPath))
            {
                throw new InvalidOperationException(
                    $"Live tile VRT for layer '{layer.Name}' does not reference a valid WMS XML file.");
            }

            string urlTemplate = ExtractUrlTemplate(wmsXmlPath)
                ?? throw new InvalidOperationException(
                    $"Could not extract the tile URL template from '{Path.GetFileName(wmsXmlPath)}'.");

            string diskCacheRoot = BuildDiskCacheRoot(urlTemplate);

            SpatialReference webMercatorSrs =
                CreateSpatialReference(WebMercatorSrsDefinition);
            SpatialReference projectSrs =
                ExtractProjectSrs(filePath) ??
                CreateSpatialReference(WebMercatorSrsDefinition);

            CoordinateTransformation webMercatorToProject =
                new CoordinateTransformation(webMercatorSrs, projectSrs);
            CoordinateTransformation projectToWebMercator =
                new CoordinateTransformation(projectSrs, webMercatorSrs);

            bool projectIsWebMercator = IsWebMercatorSpatialReference(projectSrs);

            RectangleD worldBounds = BuildWorldBounds(webMercatorToProject);

            return new XyzLiveTileRenderLayer(
                layer,
                filePath,
                urlTemplate,
                diskCacheRoot,
                worldBounds,
                webMercatorSrs,
                projectSrs,
                webMercatorToProject,
                projectToWebMercator,
                projectIsWebMercator,
                invalidateCallback);
        }

        // ── IRasterRenderLayer implementation ──────────────────────────────────

        public bool RenderVisible(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool interactive,
            CancellationToken cancellationToken = default)
        {
            bool drawnAny;

            lock (_renderSync)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsVisible || !TryIntersects(WorldBounds, visibleWorldBounds))
                {
                    return false;
                }

                if (!TryTransformBounds(
                        _projectToWebMercator,
                        visibleWorldBounds,
                        out RectangleD visibleWebMercatorBounds) ||
                    !TryClipWebMercatorBounds(
                        visibleWebMercatorBounds,
                        out RectangleD clippedWebMercatorBounds))
                {
                    return false;
                }

                int zoom = SelectZoom(engine, clippedWebMercatorBounds, interactive);

                if (!TryCreateTileRange(zoom, clippedWebMercatorBounds, out TileRange tileRange))
                {
                    return false;
                }

                // Persist viewport for the debounced fetch that fires after this returns.
                _lastWebMercatorBounds = clippedWebMercatorBounds;
                _lastZoom = zoom;

                GraphicsState graphicsState = graphics.Save();
                try
                {
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    // FIX: SourceCopy prevents alpha-fringe bleed at tile edges.
                    // SourceOver blends pre-multiplied alpha edge pixels against the
                    // transparent off-screen buffer, producing dark seams.
                    graphics.CompositingMode = CompositingMode.SourceCopy;

                    drawnAny = DrawVisibleTiles(
                        graphics,
                        engine,
                        visibleWorldBounds,
                        tileRange,
                        cancellationToken);
                }
                finally
                {
                    graphics.Restore(graphicsState);
                }
            }

            // Arm the debounce timer outside the lock so the render thread is not
            // penalised for the timer-system call.
            if (!_disposed)
            {
                _debounceTimer.Change(DebounceMilliseconds, Timeout.Infinite);
            }

            return drawnAny;
        }

        public void UpdateRenderState(bool isVisible, int transparency)
        {
            lock (_renderSync)
            {
                IsVisible = isVisible;
                Transparency = Math.Clamp(transparency, 0, 100);
                UpdateOpacityAttributes();
            }
        }

        public void InvalidateCache()
        {
            lock (_renderSync)
            {
                ClearTileCache();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _debounceTimer.Dispose();

            CancellationTokenSource oldCts;
            lock (_renderSync)
            {
                oldCts = _viewportCts;
                ClearTileCache();
                _projectTileBoundsCache.Clear();
                _opacityImageAttributes?.Dispose();
                _opacityImageAttributes = null;
                _webMercatorToProject.Dispose();
                _projectToWebMercator.Dispose();
                _webMercatorSrs.Dispose();
                _projectSrs.Dispose();
            }

            oldCts.Cancel();
            oldCts.Dispose();
        }

        // ── Synchronous tile draw ──────────────────────────────────────────────

        private bool DrawVisibleTiles(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            TileRange tileRange,
            CancellationToken cancellationToken)
        {
            bool drawnAny = false;

            for (int y = tileRange.MinY; y <= tileRange.MaxY; y++)
            {
                for (int x = tileRange.MinX; x <= tileRange.MaxX; x++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    TileKey key = new TileKey(tileRange.Zoom, x, y);
                    RectangleD projectBounds = GetProjectTileBounds(key);

                    if (!TryIntersects(projectBounds, visibleWorldBounds))
                    {
                        continue;
                    }

                    if (!_tileCache.TryGetValue(key, out Bitmap? bitmap))
                    {
                        // FIX: Draw a scaled parent tile as a placeholder while the real
                        // tile is being fetched from the internet. This prevents the map
                        // going blank or showing gaps during pan/zoom. The real tile
                        // replaces the placeholder as soon as _invalidateCallback fires.
                        if (TryDrawParentTileFallback(graphics, engine, key, projectBounds))
                        {
                            drawnAny = true;
                        }
                        continue;
                    }

                    TouchTile(key);

                    RectangleF destination =
                        WorldBoundsToScreenRectangle(engine, projectBounds);
                    if (!IsValidDestination(destination))
                    {
                        continue;
                    }

                    DrawBitmapRegion(
                        graphics,
                        bitmap,
                        AlignDestinationToPixelGrid(destination),
                        new RectangleF(0, 0, bitmap.Width, bitmap.Height));

                    drawnAny = true;
                }
            }

            return drawnAny;
        }

        /// <summary>
        /// Walks up to 3 zoom levels looking for a cached ancestor tile and draws its
        /// sub-region scaled to fill <paramref name="missingProjectBounds"/>.
        /// This gives the user a blurry-but-visible placeholder while the correct tile
        /// loads, exactly as Google Maps and QGIS do.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if a fallback tile was drawn; otherwise
        /// <see langword="false"/>.
        /// </returns>
        private bool TryDrawParentTileFallback(
            Graphics graphics,
            MapCanvasEngine engine,
            TileKey missingKey,
            RectangleD missingProjectBounds)
        {
            for (int dz = 1; dz <= 3; dz++)
            {
                int parentZoom = missingKey.Z - dz;
                if (parentZoom < 0)
                    break;

                // Each zoom step up doubles the tile coverage area.
                // scale = 2 for dz=1, 4 for dz=2, 8 for dz=3.
                int scale = 1 << dz;
                int parentX = missingKey.X / scale;
                int parentY = missingKey.Y / scale;
                TileKey parentKey = new TileKey(parentZoom, parentX, parentY);

                if (!_tileCache.TryGetValue(parentKey, out Bitmap? parentBitmap))
                    continue;

                // Compute which sub-region of the 256×256 parent bitmap covers
                // the missing child tile.
                int subTileSize = TilePixelSize / scale;
                int subX = missingKey.X % scale;
                int subY = missingKey.Y % scale;
                RectangleF srcRect = new RectangleF(
                    subX * subTileSize,
                    subY * subTileSize,
                    subTileSize,
                    subTileSize);

                RectangleF dest = WorldBoundsToScreenRectangle(engine, missingProjectBounds);
                if (!IsValidDestination(dest))
                    return false;

                Rectangle intDest = CreateIntegerDestinationRectangle(
                    AlignDestinationToPixelGrid(dest));

                // Use NearestNeighbor — fast pixel doubling, no bicubic blur.
                InterpolationMode savedMode = graphics.InterpolationMode;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                if (_opacityImageAttributes != null)
                {
                    graphics.DrawImage(
                        parentBitmap,
                        intDest,
                        srcRect.X,
                        srcRect.Y,
                        srcRect.Width,
                        srcRect.Height,
                        GraphicsUnit.Pixel,
                        _opacityImageAttributes);
                }
                else
                {
                    using ImageAttributes ia = new ImageAttributes();
                    ia.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(
                        parentBitmap,
                        intDest,
                        srcRect.X,
                        srcRect.Y,
                        srcRect.Width,
                        srcRect.Height,
                        GraphicsUnit.Pixel,
                        ia);
                }

                graphics.InterpolationMode = savedMode;
                return true;
            }

            return false;
        }

        // ── Debounce & background fetch ────────────────────────────────────────

        /// <summary>Called on a thread-pool thread 80 ms after the last render pass.</summary>
        private void OnDebounceElapsed(object? state)
        {
            if (_disposed)
            {
                return;
            }

            CancellationTokenSource newCts = new CancellationTokenSource();
            CancellationTokenSource oldCts;
            RectangleD webMercatorBounds;
            int zoom;

            lock (_renderSync)
            {
                if (_disposed)
                {
                    newCts.Dispose();
                    return;
                }

                oldCts = _viewportCts;
                _viewportCts = newCts;
                webMercatorBounds = _lastWebMercatorBounds;
                zoom = _lastZoom;
            }

            oldCts.Cancel();
            oldCts.Dispose();

            _ = FetchMissingTilesAsync(webMercatorBounds, zoom, newCts.Token);
        }

        private async Task FetchMissingTilesAsync(
            RectangleD webMercatorBounds,
            int zoom,
            CancellationToken cancellationToken)
        {
            if (!TryCreateTileRange(zoom, webMercatorBounds, out TileRange tileRange))
            {
                return;
            }

            List<TileKey> missing = [];

            lock (_renderSync)
            {
                for (int y = tileRange.MinY; y <= tileRange.MaxY; y++)
                {
                    for (int x = tileRange.MinX; x <= tileRange.MaxX; x++)
                    {
                        TileKey key = new TileKey(zoom, x, y);
                        if (!_tileCache.ContainsKey(key) && _pendingFetches.Add(key))
                        {
                            missing.Add(key);
                        }
                    }
                }
            }

            if (missing.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            IEnumerable<Task> tasks =
                missing.Select(key => FetchAndCacheTileAsync(key, cancellationToken));

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected on pan/zoom — a new viewport CTS will be issued.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[XyzLiveTileRenderLayer] Tile batch error: {ex.Message}");
            }
        }

        private async Task FetchAndCacheTileAsync(
            TileKey key,
            CancellationToken cancellationToken)
        {
            Bitmap? bitmap = null;
            try
            {
                await FetchSemaphore
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // Disk cache — avoids re-downloading tiles the user has already seen.
                    byte[]? bytes = TryReadDiskCache(key);

                    if (bytes == null || bytes.Length == 0)
                    {
                        string url = BuildTileUrl(key);
                        bytes = await SharedHttpClient
                            .GetByteArrayAsync(url, cancellationToken)
                            .ConfigureAwait(false);

                        if (bytes != null && bytes.Length > 0)
                        {
                            TryWriteDiskCache(key, bytes);
                        }
                    }

                    if (bytes == null || bytes.Length == 0)
                    {
                        return;
                    }

                    // Decode on thread pool — keeps byte[] alive for only the decode duration.
                    byte[] capturedBytes = bytes;
                    bitmap = await Task
                        .Run(() => DecodeTileBitmap(capturedBytes), cancellationToken)
                        .ConfigureAwait(false);
                }
                finally
                {
                    FetchSemaphore.Release();
                }

                if (bitmap == null || cancellationToken.IsCancellationRequested)
                {
                    bitmap?.Dispose();
                    bitmap = null;
                    return;
                }

                lock (_renderSync)
                {
                    if (_disposed || _tileCache.ContainsKey(key))
                    {
                        // Disposed or another task beat us here.
                        bitmap.Dispose();
                        bitmap = null;
                        return;
                    }

                    _tileCache[key] = bitmap;
                    bitmap = null; // ownership transferred to cache
                    LinkedListNode<TileKey> node = _tileLru.AddLast(key);
                    _tileLruNodes[key] = node;
                    TrimTileCache();
                }

                // Signal the canvas to repaint on the UI thread.
                _invalidateCallback?.Invoke();
            }
            catch (OperationCanceledException)
            {
                bitmap?.Dispose();
            }
            catch (Exception ex)
            {
                bitmap?.Dispose();
                System.Diagnostics.Debug.WriteLine(
                    $"[XyzLiveTileRenderLayer] Z{key.Z}/X{key.X}/Y{key.Y}: {ex.Message}");
            }
            finally
            {
                lock (_renderSync)
                {
                    _pendingFetches.Remove(key);
                }
            }
        }

        // ── Zoom selection ─────────────────────────────────────────────────────

        private int SelectZoom(
            MapCanvasEngine engine,
            RectangleD visibleWebMercatorBounds,
            bool interactive)
        {
            double metersPerPixel = Math.Max(
                Math.Abs(visibleWebMercatorBounds.Width) /
                Math.Max(1.0, engine.CanvasSize.Width),
                Math.Abs(visibleWebMercatorBounds.Height) /
                Math.Max(1.0, engine.CanvasSize.Height));

            if (metersPerPixel <= 0.0 ||
                double.IsNaN(metersPerPixel) ||
                double.IsInfinity(metersPerPixel))
            {
                return 0;
            }

            int desiredZoom = (int)Math.Round(
                Math.Log(InitialResolution / metersPerPixel, 2.0),
                MidpointRounding.AwayFromZero);

            if (interactive)
            {
                desiredZoom--;
            }

            desiredZoom = Math.Clamp(desiredZoom, 0, MaxSupportedZoom);

            // Guard against runaway tile counts at very deep zoom or large viewports.
            while (desiredZoom > 0 &&
                   TryCreateTileRange(
                       desiredZoom,
                       visibleWebMercatorBounds,
                       out TileRange guard) &&
                   guard.Count > MaxTilesPerFrame)
            {
                desiredZoom--;
            }

            return desiredZoom;
        }

        // ── Tile range ─────────────────────────────────────────────────────────

        private static bool TryCreateTileRange(
            int zoom,
            RectangleD webMercatorBounds,
            out TileRange tileRange)
        {
            tileRange = default;

            if (zoom < 0 || zoom > MaxSupportedZoom)
            {
                return false;
            }

            long matrixSize = 1L << zoom;
            if (matrixSize <= 0)
            {
                return false;
            }

            double tileWorldSize = WebMercatorWorldSize / matrixSize;
            double left = Math.Clamp(MinX(webMercatorBounds), -WebMercatorExtent, WebMercatorExtent);
            double right = Math.Clamp(MaxX(webMercatorBounds), -WebMercatorExtent, WebMercatorExtent);
            double bottom = Math.Clamp(MinY(webMercatorBounds), -WebMercatorExtent, WebMercatorExtent);
            double top = Math.Clamp(MaxY(webMercatorBounds), -WebMercatorExtent, WebMercatorExtent);

            int minX = ClampTileIndex(
                (int)Math.Floor((left + WebMercatorExtent) / tileWorldSize),
                matrixSize);
            int maxX = ClampTileIndex(
                (int)Math.Floor((right + WebMercatorExtent) / tileWorldSize),
                matrixSize);
            int minY = ClampTileIndex(
                (int)Math.Floor((WebMercatorExtent - top) / tileWorldSize),
                matrixSize);
            int maxY = ClampTileIndex(
                (int)Math.Floor((WebMercatorExtent - bottom) / tileWorldSize),
                matrixSize);

            if (maxX < minX || maxY < minY)
            {
                return false;
            }

            tileRange = new TileRange(zoom, minX, maxX, minY, maxY);
            return true;
        }

        // ── Coordinate helpers ─────────────────────────────────────────────────

        private RectangleD GetProjectTileBounds(TileKey key)
        {
            if (_projectTileBoundsCache.TryGetValue(key, out RectangleD cached))
            {
                return cached;
            }

            RectangleD webMercatorBounds = GetWebMercatorTileBounds(key);
            RectangleD projectBounds;

            if (_projectIsWebMercator)
            {
                projectBounds = webMercatorBounds;
            }
            else if (!TryTransformBounds(
                         _webMercatorToProject,
                         webMercatorBounds,
                         out projectBounds))
            {
                projectBounds = webMercatorBounds;
            }

            _projectTileBoundsCache[key] = projectBounds;
            TrimProjectBoundsCache();
            return projectBounds;
        }

        private static RectangleD GetWebMercatorTileBounds(TileKey key)
        {
            long matrixSize = 1L << key.Z;
            double tileWorldSize = WebMercatorWorldSize / matrixSize;
            double left = -WebMercatorExtent + key.X * tileWorldSize;
            double right = left + tileWorldSize;
            double top = WebMercatorExtent - key.Y * tileWorldSize;
            double bottom = top - tileWorldSize;

            return new RectangleD(left, bottom, right - left, top - bottom);
        }

        private static bool TryTransformBounds(
            CoordinateTransformation transformation,
            RectangleD sourceBounds,
            out RectangleD targetBounds)
        {
            targetBounds = default;

            PointD[] sourcePoints =
            [
                new PointD(MinX(sourceBounds), MinY(sourceBounds)),
                new PointD(MinX(sourceBounds), MaxY(sourceBounds)),
                new PointD(MaxX(sourceBounds), MinY(sourceBounds)),
                new PointD(MaxX(sourceBounds), MaxY(sourceBounds)),
                new PointD((MinX(sourceBounds) + MaxX(sourceBounds)) / 2.0, MinY(sourceBounds)),
                new PointD((MinX(sourceBounds) + MaxX(sourceBounds)) / 2.0, MaxY(sourceBounds)),
                new PointD(MinX(sourceBounds), (MinY(sourceBounds) + MaxY(sourceBounds)) / 2.0),
                new PointD(MaxX(sourceBounds), (MinY(sourceBounds) + MaxY(sourceBounds)) / 2.0),
                new PointD(
                    (MinX(sourceBounds) + MaxX(sourceBounds)) / 2.0,
                    (MinY(sourceBounds) + MaxY(sourceBounds)) / 2.0)
            ];

            List<PointD> transformed = [];
            foreach (PointD src in sourcePoints)
            {
                if (TryTransformPoint(transformation, src, out PointD dst))
                {
                    transformed.Add(dst);
                }
            }

            if (transformed.Count == 0)
            {
                return false;
            }

            double minX = transformed.Min(p => p.X);
            double maxX = transformed.Max(p => p.X);
            double minY = transformed.Min(p => p.Y);
            double maxY = transformed.Max(p => p.Y);

            if (maxX <= minX || maxY <= minY)
            {
                return false;
            }

            targetBounds = new RectangleD(minX, minY, maxX - minX, maxY - minY);
            return true;
        }

        private static bool TryTransformPoint(
            CoordinateTransformation transformation,
            PointD source,
            out PointD result)
        {
            result = default;
            try
            {
                double[] pt = [source.X, source.Y, 0.0];
                transformation.TransformPoint(pt);
                if (!IsFiniteD(pt[0]) || !IsFiniteD(pt[1]))
                {
                    return false;
                }

                result = new PointD(pt[0], pt[1]);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryClipWebMercatorBounds(
            RectangleD bounds,
            out RectangleD clipped)
        {
            clipped = default;
            double left = Math.Max(MinX(bounds), -WebMercatorExtent);
            double right = Math.Min(MaxX(bounds), WebMercatorExtent);
            double bottom = Math.Max(MinY(bounds), -WebMercatorExtent);
            double top = Math.Min(MaxY(bounds), WebMercatorExtent);

            if (right <= left || top <= bottom)
            {
                return false;
            }

            clipped = new RectangleD(left, bottom, right - left, top - bottom);
            return true;
        }

        // ── Drawing helpers ────────────────────────────────────────────────────

        private void DrawBitmapRegion(
            Graphics graphics,
            Bitmap bitmap,
            RectangleF destination,
            RectangleF source)
        {
            if (_opacityImageAttributes == null)
            {
                using ImageAttributes ia = new ImageAttributes();
                ia.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(
                    bitmap,
                    [
                        new PointF(destination.Left, destination.Top),
                        new PointF(destination.Right, destination.Top),
                        new PointF(destination.Left, destination.Bottom)
                    ],
                    source,
                    GraphicsUnit.Pixel,
                    ia);
                return;
            }

            Rectangle dest = CreateIntegerDestinationRectangle(destination);
            graphics.DrawImage(
                bitmap,
                dest,
                source.X,
                source.Y,
                source.Width,
                source.Height,
                GraphicsUnit.Pixel,
                _opacityImageAttributes);
        }

        private void UpdateOpacityAttributes()
        {
            _opacityImageAttributes?.Dispose();
            _opacityImageAttributes = null;

            double opacityFactor = (100 - Transparency) / 100.0;
            if (opacityFactor >= 1.0)
            {
                return;
            }

            ImageAttributes ia = new ImageAttributes();
            ColorMatrix matrix = new ColorMatrix(
            [
                [1f, 0f, 0f, 0f, 0f],
                [0f, 1f, 0f, 0f, 0f],
                [0f, 0f, 1f, 0f, 0f],
                [0f, 0f, 0f, (float)opacityFactor, 0f],
                [0f, 0f, 0f, 0f, 1f]
            ]);
            ia.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetWrapMode(WrapMode.TileFlipXY);
            _opacityImageAttributes = ia;
        }

        // ── Bitmap decoding ────────────────────────────────────────────────────

        private static Bitmap? DecodeTileBitmap(byte[] tileData)
        {
            try
            {
                using MemoryStream stream = new MemoryStream(tileData, writable: false);
                using Image image = Image.FromStream(
                    stream,
                    useEmbeddedColorManagement: false,
                    validateImageData: false);

                Bitmap bitmap = new Bitmap(
                    image.Width,
                    image.Height,
                    PixelFormat.Format32bppPArgb);

                using Graphics g = Graphics.FromImage(bitmap);
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                g.DrawImageUnscaled(image, 0, 0);
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        // ── LRU cache management ───────────────────────────────────────────────

        private void TouchTile(TileKey key)
        {
            if (!_tileLruNodes.TryGetValue(key, out LinkedListNode<TileKey>? node))
            {
                return;
            }

            _tileLru.Remove(node);
            _tileLru.AddLast(node);
        }

        private void TrimTileCache()
        {
            while (_tileCache.Count > MaxCachedTiles && _tileLru.First != null)
            {
                TileKey evicted = _tileLru.First.Value;
                _tileLru.RemoveFirst();
                _tileLruNodes.Remove(evicted);
                _projectTileBoundsCache.Remove(evicted);

                if (_tileCache.Remove(evicted, out Bitmap? bm))
                {
                    bm.Dispose();
                }
            }
        }

        private void TrimProjectBoundsCache()
        {
            if (_projectTileBoundsCache.Count <= MaxCachedTiles * 2)
            {
                return;
            }

            HashSet<TileKey> live = [.. _tileCache.Keys];
            foreach (TileKey key in _projectTileBoundsCache.Keys
                .Where(k => !live.Contains(k))
                .Take(_projectTileBoundsCache.Count - MaxCachedTiles)
                .ToArray())
            {
                _projectTileBoundsCache.Remove(key);
            }
        }

        private void ClearTileCache()
        {
            foreach (Bitmap bm in _tileCache.Values)
            {
                bm.Dispose();
            }

            _tileCache.Clear();
            _tileLru.Clear();
            _tileLruNodes.Clear();
            _pendingFetches.Clear();
            _projectTileBoundsCache.Clear();
        }

        // ── Disk cache ─────────────────────────────────────────────────────────

        private byte[]? TryReadDiskCache(TileKey key)
        {
            try
            {
                string path = GetDiskCachePath(key);
                return File.Exists(path) ? File.ReadAllBytes(path) : null;
            }
            catch
            {
                return null;
            }
        }

        private void TryWriteDiskCache(TileKey key, byte[] bytes)
        {
            try
            {
                string path = GetDiskCachePath(key);
                string? dir = Path.GetDirectoryName(path);
                if (dir != null)
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllBytes(path, bytes);
            }
            catch
            {
                // Disk cache writes are best-effort; never block rendering.
            }
        }

        private string GetDiskCachePath(TileKey key) =>
            Path.Combine(
                _diskCacheRoot,
                key.Z.ToString(CultureInfo.InvariantCulture),
                $"{key.X.ToString(CultureInfo.InvariantCulture)}_{key.Y.ToString(CultureInfo.InvariantCulture)}");

        // ── URL construction ───────────────────────────────────────────────────

        private string BuildTileUrl(TileKey key) =>
            _urlTemplate
                .Replace("${z}", key.Z.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
                .Replace("${x}", key.X.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
                .Replace("${y}", key.Y.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);

        // ── Static initialization helpers ──────────────────────────────────────

        /// <summary>
        /// Reads the VRT file and returns the absolute path of the WMS/TMS XML source
        /// referenced by its first &lt;SourceFilename&gt; element.
        /// </summary>
        private static string? FindVrtSourcePath(string vrtPath)
        {
            try
            {
                string content = File.ReadAllText(vrtPath);
                string? dir = Path.GetDirectoryName(vrtPath);

                // GDAL Warp VRTs reference the source as <SourceDataset>; simple VRTs use <SourceFilename>.
                string rawPath =
                    ExtractVrtElementText(content, "SourceDataset") ??
                    ExtractVrtElementText(content, "SourceFilename");

                if (string.IsNullOrWhiteSpace(rawPath))
                {
                    return null;
                }

                if (Path.IsPathRooted(rawPath))
                {
                    return rawPath;
                }

                return string.IsNullOrWhiteSpace(dir)
                    ? rawPath
                    : Path.GetFullPath(Path.Combine(dir, rawPath));
            }
            catch
            {
                return null;
            }
        }

        private static string? ExtractVrtElementText(string content, string elementName)
        {
            int start = content.IndexOf($"<{elementName}", StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return null;

            int contentStart = content.IndexOf('>', start) + 1;
            int end = content.IndexOf($"</{elementName}>", contentStart, StringComparison.OrdinalIgnoreCase);
            if (contentStart <= 0 || end < 0)
                return null;

            string text = content[contentStart..end].Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        /// <summary>
        /// Parses the GDAL WMS XML file and returns the raw tile URL template
        /// (with <c>${z}</c>, <c>${x}</c>, <c>${y}</c> placeholders intact).
        /// </summary>
        private static string? ExtractUrlTemplate(string wmsXmlPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(wmsXmlPath);

                // Accept <ServerUrl> at any depth inside the GDAL_WMS document.
                XmlNode? node = doc.SelectSingleNode("//*[local-name()='ServerUrl']");
                string? url = node?.InnerText?.Trim();
                return string.IsNullOrWhiteSpace(url) ? null : url;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts the project CRS WKT from the &lt;SRS&gt; element of the VRT file.
        /// Returns <see langword="null"/> when the element is absent or unparseable.
        /// </summary>
        private static SpatialReference? ExtractProjectSrs(string vrtPath)
        {
            try
            {
                string content = File.ReadAllText(vrtPath);

                int startTag = content.IndexOf("<SRS", StringComparison.OrdinalIgnoreCase);
                if (startTag < 0)
                {
                    return null;
                }

                int contentStart = content.IndexOf('>', startTag) + 1;
                int endTag = content.IndexOf(
                    "</SRS>",
                    contentStart,
                    StringComparison.OrdinalIgnoreCase);
                if (contentStart <= 0 || endTag < 0)
                {
                    return null;
                }

                string wkt = content[contentStart..endTag].Trim();
                return string.IsNullOrWhiteSpace(wkt)
                    ? null
                    : CreateSpatialReference(wkt);
            }
            catch
            {
                return null;
            }
        }

        private static string BuildDiskCacheRoot(string urlTemplate)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(urlTemplate));
            string prefix = Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
            return Path.Combine(Path.GetTempPath(), "replot-live-tiles", prefix);
        }

        private static RectangleD BuildWorldBounds(
            CoordinateTransformation webMercatorToProject)
        {
            // Full WebMercator extent (±20 037 508 m in both axes).
            RectangleD full = new RectangleD(
                -WebMercatorExtent,
                -WebMercatorExtent,
                WebMercatorWorldSize,
                WebMercatorWorldSize);

            return TryTransformBounds(webMercatorToProject, full, out RectangleD projected)
                ? projected
                : full;
        }

        private static SpatialReference CreateSpatialReference(string definition)
        {
            SpatialReference srs = new SpatialReference(string.Empty);
            srs.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);

            if (srs.SetFromUserInput(definition) != 0)
            {
                string wkt = definition;
                if (srs.ImportFromWkt(ref wkt) != 0)
                {
                    srs.Dispose();
                    throw new InvalidOperationException(
                        $"Could not parse CRS definition '{definition}'.");
                }
            }

            srs.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            return srs;
        }

        private static bool IsWebMercatorSpatialReference(SpatialReference srs)
        {
            try
            {
                srs.AutoIdentifyEPSG();
                string? code =
                    srs.GetAuthorityCode(null) ??
                    srs.GetAuthorityCode("PROJCS");
                if (code is "3857" or "900913" or "3785")
                {
                    return true;
                }

                string? name = srs.GetAttrValue("PROJCS", 0);
                return name != null &&
                       (name.Contains("Pseudo-Mercator", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Web Mercator", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Popular Visualisation", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private static HttpClient CreateSharedHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = MaxConcurrentFetches,
                AutomaticDecompression =
                    DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                UseCookies = false
            };

            HttpClient client = new HttpClient(handler, disposeHandler: true)
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (compatible; RePlot/1.0)");
            return client;
        }

        // ── Screen geometry helpers ────────────────────────────────────────────

        private static RectangleF WorldBoundsToScreenRectangle(
            MapCanvasEngine engine,
            RectangleD worldBounds)
        {
            PointD topLeft = engine.WorldToScreen(
                new PointD(MinX(worldBounds), MaxY(worldBounds)));
            PointD bottomRight = engine.WorldToScreen(
                new PointD(MaxX(worldBounds), MinY(worldBounds)));

            return RectangleF.FromLTRB(
                (float)Math.Min(topLeft.X, bottomRight.X),
                (float)Math.Min(topLeft.Y, bottomRight.Y),
                (float)Math.Max(topLeft.X, bottomRight.X),
                (float)Math.Max(topLeft.Y, bottomRight.Y));
        }

        private static RectangleF AlignDestinationToPixelGrid(RectangleF destination) =>
            RectangleF.FromLTRB(
                (float)Math.Floor(destination.Left),
                (float)Math.Floor(destination.Top),
                (float)Math.Ceiling(destination.Right),
                (float)Math.Ceiling(destination.Bottom));

        private static Rectangle CreateIntegerDestinationRectangle(RectangleF destination) =>
            Rectangle.FromLTRB(
                (int)Math.Floor(destination.Left),
                (int)Math.Floor(destination.Top),
                (int)Math.Ceiling(destination.Right),
                (int)Math.Ceiling(destination.Bottom));

        private static bool IsValidDestination(RectangleF r) =>
            IsFiniteF(r.Left) && IsFiniteF(r.Top) &&
            IsFiniteF(r.Width) && IsFiniteF(r.Height) &&
            r.Width >= 0.5f && r.Height >= 0.5f;

        private static bool TryIntersects(RectangleD a, RectangleD b) =>
            MinX(a) < MaxX(b) &&
            MaxX(a) > MinX(b) &&
            MinY(a) < MaxY(b) &&
            MaxY(a) > MinY(b);

        private static int ClampTileIndex(int value, long matrixSize)
        {
            if (value < 0) return 0;
            if (value >= matrixSize) return (int)matrixSize - 1;
            return value;
        }

        private static double MinX(RectangleD r) => Math.Min(r.Left, r.Right);
        private static double MaxX(RectangleD r) => Math.Max(r.Left, r.Right);
        private static double MinY(RectangleD r) => Math.Min(r.Top, r.Bottom);
        private static double MaxY(RectangleD r) => Math.Max(r.Top, r.Bottom);

        private static bool IsFiniteD(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        private static bool IsFiniteF(float v) => !float.IsNaN(v) && !float.IsInfinity(v);

        // ── Inner types ────────────────────────────────────────────────────────

        private readonly record struct TileKey(int Z, int X, int Y);

        private readonly record struct TileRange(
            int Zoom,
            int MinX,
            int MaxX,
            int MinY,
            int MaxY)
        {
            public long Count =>
                (long)Math.Max(0, MaxX - MinX + 1) *
                Math.Max(0, MaxY - MinY + 1);
        }
    }
}