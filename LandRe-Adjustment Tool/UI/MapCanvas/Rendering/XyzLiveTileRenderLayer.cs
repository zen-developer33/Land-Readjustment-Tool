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


        private readonly RectangleD _validProjectExtent;
        // ── Constants ──────────────────────────────────────────────────────────
        private const int TilePixelSize = 256;
        private const int MaxCachedTiles = 512;
        private const int MaxTilesPerFrame = 256;
        private const int MaxBootstrapTilesPerFrame = 16;
        private const int MaxConcurrentFetches = 6;
        private const int DebounceMilliseconds = 120;
        private const int MaxSupportedZoom = 22;
        private const int ProjectedTileMeshSubdivisions = 4;
        private const int MinFreshTileZoom = 6;
        private const int MaxMosaicMeshSubdivisions = 32;
        private const double WebMercatorExtent = 20037508.342789244;
        private const double WebMercatorWorldSize = WebMercatorExtent * 2.0;
        private const double InitialResolution = WebMercatorWorldSize / TilePixelSize;
        private const string WebMercatorSrsDefinition = "EPSG:3857";
        private static readonly RectangleD AsiaWebMercatorBounds =
            CreateWebMercatorBoundsFromLonLat(24.0, -12.0, 150.0, 82.0);

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
        private readonly HashSet<TileKey> _noDataTiles = [];
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

        // ── Composite bitmap cache ──────────────────────────────────────────────
        // Pre-rendered offscreen bitmap of all cached tiles at the last settled viewport.
        // Interactive pan/zoom frames blit-and-stretch this instead of re-drawing every tile.
        private Bitmap? _compositeBitmap;
        private RectangleD _compositeWorldBounds;
        private Size _compositeCanvasSize;
        private bool _compositeValid;
        private double _compositeZoomScale;   // engine zoom when composite was built

        private bool _lastRenderInteractive;
        private bool _lastViewportAllowed;

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
            _validProjectExtent = ExpandExtent(worldBounds, 0.5);
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
        public bool CanRenderFromMemoryCacheDuringInteraction => true;

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
            RectangleD fetchWebMercatorBounds = default;
            int fetchZoom = 0;
            bool lastInteractive = interactive;
            bool lastViewportAllowed = false;

            lock (_renderSync)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsVisible)
                {
                    _lastRenderInteractive = interactive;
                    _lastViewportAllowed = false;
                    return false;
                }

                _lastRenderInteractive = interactive;
                _lastViewportAllowed = false;

                if (interactive && !_viewportCts.IsCancellationRequested)
                {
                    _viewportCts.Cancel();
                }

                // Do NOT clip against WorldBounds/_validProjectExtent here.
                // For live XYZ tiles, those projected bounds can be inaccurate in local/project CRS.
                // Instead, transform the current canvas viewport to WebMercator,
                // then clip only in WebMercator using AsiaWebMercatorBounds.
                if (!TryTransformBounds(
                        _projectToWebMercator,
                        visibleWorldBounds,
                        out RectangleD visibleWebMercatorBounds) ||
                    !TryClipWebMercatorBounds(
                        visibleWebMercatorBounds,
                        out RectangleD clippedWebMercatorBounds))
                {
                    _lastViewportAllowed = false;
                    return false;
                }

                _lastViewportAllowed = true;
                lastViewportAllowed = true;

                int zoom = SelectZoom(engine, clippedWebMercatorBounds, interactive);
                if (zoom < MinFreshTileZoom)
                {
                    _lastViewportAllowed = false;
                    drawnAny = _compositeBitmap != null && DrawCompositeCache(graphics, engine);
                    return drawnAny;
                }

                if (!TryCreateTileRange(zoom, clippedWebMercatorBounds, out TileRange tileRange))
                {
                    return false;
                }

                // Persist viewport for the debounced fetch that fires after this returns.
                _lastWebMercatorBounds = clippedWebMercatorBounds;
                _lastZoom = zoom;
                fetchWebMercatorBounds = clippedWebMercatorBounds;
                fetchZoom = zoom;

                Size canvasSize = engine.CanvasSize;

                if (interactive)
                {
                    // ── Interactive: NEVER draw tiles. Just stretch the cached composite. ──
                    // Pan/zoom = one DrawImage call, zero tile iteration, zero reprojection.
                    drawnAny = _compositeBitmap != null &&
                               _compositeCanvasSize == canvasSize &&
                               DrawCompositeCache(graphics, engine);
                }
                else
                {
                    // ── Settled: rebuild composite from cached tiles, then blit 1:1. ────
                    drawnAny = RebuildCompositeAndDraw(
                        graphics,
                        engine,
                        visibleWorldBounds,
                        tileRange,
                        canvasSize,
                        cancellationToken);
                }
            }

            // Arm the debounce timer outside the lock so the render thread is not
            // penalised for the timer-system call.
            if (!_disposed)
            {
                if (!lastInteractive && lastViewportAllowed && !drawnAny)
                {
                    QueueBootstrapFetch(fetchWebMercatorBounds, fetchZoom);
                }

                if (!lastInteractive && lastViewportAllowed)
                {
                    _debounceTimer.Change(DebounceMilliseconds, Timeout.Infinite);
                }
            }

            return drawnAny;
        }

        /// <summary>
        /// Paints the pre-built composite bitmap translated/scaled to the current viewport.
        /// One DrawImage call per frame — O(1) regardless of tile count.
        /// </summary>
        private bool DrawCompositeCache(Graphics graphics, MapCanvasEngine engine)
        {
            if (_compositeBitmap == null)
            {
                return false;
            }

            // Map the stored world bounds corners to screen space in the current viewport.
            PointD tl = engine.WorldToScreen(
                new PointD(MinX(_compositeWorldBounds), MaxY(_compositeWorldBounds)));
            PointD br = engine.WorldToScreen(
                new PointD(MaxX(_compositeWorldBounds), MinY(_compositeWorldBounds)));

            if (!IsFiniteD(tl.X) || !IsFiniteD(tl.Y) ||
                !IsFiniteD(br.X) || !IsFiniteD(br.Y))
            {
                return false;
            }

            float dstW = (float)(br.X - tl.X);
            float dstH = (float)(br.Y - tl.Y);
            if (dstW < 0.5f || dstH < 0.5f)
            {
                return false;
            }

            GraphicsState gs = graphics.Save();
            try
            {
                // SourceOver so transparent composite margins don't erase layers below.
                graphics.CompositingMode = CompositingMode.SourceOver;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                // HighQualityBicubic gives smooth appearance when zooming — no blocky pixels.
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.None;

                graphics.DrawImage(
                    _compositeBitmap,
                    new RectangleF((float)tl.X, (float)tl.Y, dstW, dstH),
                    new RectangleF(0, 0, _compositeBitmap.Width, _compositeBitmap.Height),
                    GraphicsUnit.Pixel);
            }
            finally
            {
                graphics.Restore(gs);
            }

            return true;
        }

        /// <summary>
        /// Draws all cached tiles into an offscreen composite bitmap (applying the Asia clip
        /// and pixel-perfect alignment), then blits it 1:1 to the canvas.
        /// Sets <see cref="_compositeValid"/> so subsequent interactive frames can fast-path.
        /// </summary>
        private bool RebuildCompositeAndDraw(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            TileRange tileRange,
            Size canvasSize,
            CancellationToken cancellationToken)
        {
            // Allocate or resize the offscreen bitmap to match the canvas.
            if (_compositeBitmap == null ||
                _compositeBitmap.Width != canvasSize.Width ||
                _compositeBitmap.Height != canvasSize.Height)
            {
                _compositeBitmap?.Dispose();
                _compositeValid = false;
                _compositeBitmap = canvasSize.Width > 0 && canvasSize.Height > 0
                    ? new Bitmap(canvasSize.Width, canvasSize.Height, PixelFormat.Format32bppPArgb)
                    : null;
            }

            bool drawnAny = false;

            if (_compositeBitmap != null)
            {
                using (Graphics cg = Graphics.FromImage(_compositeBitmap))
                {
                    cg.Clear(Color.Transparent);

                    // PixelOffsetMode.None = pixels land exactly on integer coordinates.
                    // HighQuality offsets by 0.5 px which creates 1-pixel seams between tiles.
                    cg.SmoothingMode = SmoothingMode.None;
                    cg.InterpolationMode = InterpolationMode.NearestNeighbor;
                    cg.PixelOffsetMode = PixelOffsetMode.None;
                    cg.CompositingQuality = CompositingQuality.HighSpeed;
                    // SourceCopy prevents alpha-fringe bleed at tile seams.
                    cg.CompositingMode = CompositingMode.SourceCopy;

                    // Clip to the Asia continent boundary so tiles that partially
                    // overlap the edge are not drawn beyond the Asian extent.
                    ApplyAsiaScreenClip(cg, engine);

                    drawnAny = DrawVisibleTileMosaic(
                        cg, engine, visibleWorldBounds, tileRange, cancellationToken);
                }

                // Store the viewport reference so interactive frames can remap it.
                if (drawnAny)
                {
                    _compositeWorldBounds = visibleWorldBounds;
                    _compositeCanvasSize = canvasSize;
                    _compositeZoomScale = engine.ZoomScale;
                    _compositeValid = true;
                }

                // Blit composite to the canvas using SourceOver so transparent margins
                // (Asia clip edges) do not erase underlying layers.
                GraphicsState gs = graphics.Save();
                try
                {
                    graphics.CompositingMode = CompositingMode.SourceOver;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImageUnscaled(_compositeBitmap, 0, 0);
                }
                finally
                {
                    graphics.Restore(gs);
                }
            }
            else
            {
                // Fallback: draw directly when bitmap allocation fails (e.g. zero-size canvas).
                GraphicsState gs = graphics.Save();
                try
                {
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.None;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.CompositingMode = CompositingMode.SourceCopy;

                    drawnAny = DrawVisibleTileMosaic(
                        graphics, engine, visibleWorldBounds, tileRange, cancellationToken);
                }
                finally
                {
                    graphics.Restore(gs);
                }
            }

            return drawnAny;
        }

        /// <summary>
        /// Clips <paramref name="g"/> to the Asia extent projected into current screen space.
        /// Tiles that spill beyond the Asian boundary are masked out.
        /// </summary>
        private void ApplyAsiaScreenClip(Graphics g, MapCanvasEngine engine)
        {
            RectangleD asiaBoundsProject;
            if (_projectIsWebMercator)
            {
                asiaBoundsProject = AsiaWebMercatorBounds;
            }
            else if (!TryTransformBounds(
                         _webMercatorToProject,
                         AsiaWebMercatorBounds,
                         out asiaBoundsProject))
            {
                return;
            }

            PointD tl = engine.WorldToScreen(
                new PointD(MinX(asiaBoundsProject), MaxY(asiaBoundsProject)));
            PointD br = engine.WorldToScreen(
                new PointD(MaxX(asiaBoundsProject), MinY(asiaBoundsProject)));

            if (!IsFiniteD(tl.X) || !IsFiniteD(tl.Y) ||
                !IsFiniteD(br.X) || !IsFiniteD(br.Y))
            {
                return;
            }

            float left   = (float)Math.Min(tl.X, br.X);
            float top    = (float)Math.Min(tl.Y, br.Y);
            float right  = (float)Math.Max(tl.X, br.X);
            float bottom = (float)Math.Max(tl.Y, br.Y);

            if (right <= left || bottom <= top)
            {
                return;
            }

            g.SetClip(
                new RectangleF(left, top, right - left, bottom - top),
                CombineMode.Intersect);
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
                ClearTileCache();          // also disposes _compositeBitmap
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

        private bool DrawVisibleTileMosaic(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            TileRange tileRange,
            CancellationToken cancellationToken)
        {
            int columns = tileRange.MaxX - tileRange.MinX + 1;
            int rows = tileRange.MaxY - tileRange.MinY + 1;
            if (columns <= 0 || rows <= 0)
            {
                return false;
            }

            int mosaicWidth = columns * TilePixelSize;
            int mosaicHeight = rows * TilePixelSize;
            using Bitmap mosaic = new(
                mosaicWidth,
                mosaicHeight,
                PixelFormat.Format32bppPArgb);

            bool hasPixels = false;
            using (Graphics mosaicGraphics = Graphics.FromImage(mosaic))
            {
                mosaicGraphics.Clear(Color.Transparent);
                mosaicGraphics.SmoothingMode = SmoothingMode.None;
                mosaicGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                mosaicGraphics.PixelOffsetMode = PixelOffsetMode.None;
                mosaicGraphics.CompositingQuality = CompositingQuality.HighSpeed;
                mosaicGraphics.CompositingMode = CompositingMode.SourceCopy;

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

                        Rectangle destination = new(
                            (x - tileRange.MinX) * TilePixelSize,
                            (y - tileRange.MinY) * TilePixelSize,
                            TilePixelSize,
                            TilePixelSize);

                        if (_tileCache.TryGetValue(key, out Bitmap? bitmap))
                        {
                            TouchTile(key);
                            DrawMosaicBitmapRegion(
                                mosaicGraphics,
                                bitmap,
                                destination,
                                new RectangleF(0, 0, bitmap.Width, bitmap.Height),
                                smooth: false);
                            hasPixels = true;
                            continue;
                        }

                        if (TryGetParentPlaceholder(
                                key,
                                maxLevelsUp: 4,
                                out Bitmap? parentBitmap,
                                out RectangleF parentSource))
                        {
                            DrawMosaicBitmapRegion(
                                mosaicGraphics,
                                parentBitmap,
                                destination,
                                parentSource,
                                smooth: true);
                            hasPixels = true;
                        }
                    }
                }
            }

            if (!hasPixels)
            {
                return false;
            }

            return DrawMosaicSurface(
                graphics,
                engine,
                mosaic,
                GetWebMercatorTileRangeBounds(tileRange));
        }

        private bool DrawMosaicSurface(
            Graphics graphics,
            MapCanvasEngine engine,
            Bitmap mosaic,
            RectangleD webMercatorBounds)
        {
            RectangleF source = new(0, 0, mosaic.Width, mosaic.Height);
            if (_projectIsWebMercator)
            {
                RectangleF destination =
                    WorldBoundsToScreenRectangle(engine, webMercatorBounds);
                if (!IsValidDestination(destination))
                {
                    return false;
                }

                DrawBitmapRegion(
                    graphics,
                    mosaic,
                    AlignDestinationToPixelGrid(destination),
                    source);
                return true;
            }

            int subdivisionsX = Math.Clamp(
                (mosaic.Width / TilePixelSize) * 2,
                8,
                MaxMosaicMeshSubdivisions);
            int subdivisionsY = Math.Clamp(
                (mosaic.Height / TilePixelSize) * 2,
                8,
                MaxMosaicMeshSubdivisions);
            bool drawnAny = false;

            for (int row = 0; row < subdivisionsY; row++)
            {
                for (int column = 0; column < subdivisionsX; column++)
                {
                    RectangleD cellBounds = GetSubBounds(
                        webMercatorBounds,
                        column,
                        row,
                        subdivisionsX,
                        subdivisionsY);
                    if (!TryCreateProjectedScreenQuad(
                            engine,
                            cellBounds,
                            out PointF[] destination))
                    {
                        continue;
                    }

                    RectangleF cellSource = GetSourceSubRectangle(
                        source,
                        column,
                        row,
                        subdivisionsX,
                        subdivisionsY);
                    if (!IsValidSource(cellSource))
                    {
                        continue;
                    }

                    DrawBitmapQuad(
                        graphics,
                        mosaic,
                        ExpandDestinationQuadForSeams(destination),
                        cellSource);
                    drawnAny = true;
                }
            }

            return drawnAny;
        }

        private void DrawMosaicBitmapRegion(
            Graphics graphics,
            Bitmap bitmap,
            Rectangle destination,
            RectangleF source,
            bool smooth)
        {
            InterpolationMode savedInterpolation = graphics.InterpolationMode;
            graphics.InterpolationMode = smooth
                ? InterpolationMode.HighQualityBicubic
                : InterpolationMode.NearestNeighbor;

            try
            {
                using ImageAttributes attributes = CreateDefaultImageAttributes();
                graphics.DrawImage(
                    bitmap,
                    destination,
                    source.X,
                    source.Y,
                    source.Width,
                    source.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }
            finally
            {
                graphics.InterpolationMode = savedInterpolation;
            }
        }

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
                        if (TryDrawParentTileFallback(
                                graphics,
                                engine,
                                key,
                                projectBounds))
                        {
                            drawnAny = true;
                        }

                        continue;
                    }

                    TouchTile(key);

                    drawnAny |= DrawTileBitmap(
                        graphics,
                        engine,
                        bitmap,
                        key,
                        projectBounds);
                }
            }

            return drawnAny;
        }

        private bool DrawTileBitmap(
            Graphics graphics,
            MapCanvasEngine engine,
            Bitmap bitmap,
            TileKey key,
            RectangleD projectBounds)
        {
            if (_projectIsWebMercator)
            {
                RectangleF destination =
                    WorldBoundsToScreenRectangle(engine, projectBounds);
                if (!IsValidDestination(destination))
                {
                    return false;
                }

                DrawBitmapRegion(
                    graphics,
                    bitmap,
                    AlignDestinationToPixelGrid(destination),
                    new RectangleF(0, 0, bitmap.Width, bitmap.Height));
                return true;
            }

            return DrawTileBitmapMesh(
                graphics,
                engine,
                bitmap,
                key,
                new RectangleF(0, 0, bitmap.Width, bitmap.Height));
        }

        private bool DrawTileBitmapMesh(
            Graphics graphics,
            MapCanvasEngine engine,
            Bitmap bitmap,
            TileKey key,
            RectangleF sourceBounds)
        {
            RectangleD webMercatorBounds = GetWebMercatorTileBounds(key);
            bool drawnAny = false;

            for (int row = 0; row < ProjectedTileMeshSubdivisions; row++)
            {
                for (int column = 0; column < ProjectedTileMeshSubdivisions; column++)
                {
                    RectangleD sourceWebMercatorBounds =
                        GetSubTileWebMercatorBounds(
                            webMercatorBounds,
                            column,
                            row,
                            ProjectedTileMeshSubdivisions);

                    if (!TryCreateProjectedScreenQuad(
                            engine,
                            sourceWebMercatorBounds,
                            out PointF[] destination))
                    {
                        continue;
                    }

                    RectangleF source = GetSourceSubRectangle(
                        sourceBounds,
                        column,
                        row,
                        ProjectedTileMeshSubdivisions);
                    if (!IsValidSource(source))
                    {
                        continue;
                    }

                    DrawBitmapQuad(
                        graphics,
                        bitmap,
                        destination,
                        source);
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

                return DrawParentBitmapRegion(
                    graphics,
                    engine,
                    parentBitmap,
                    missingKey,
                    missingProjectBounds,
                    srcRect);

            }

            return false;
        }

        // ── Debounce & background fetch ────────────────────────────────────────

        private bool TryGetParentPlaceholder(
            TileKey missingKey,
            int maxLevelsUp,
            out Bitmap? bitmap,
            out RectangleF sourceRect)
        {
            bitmap = null;
            sourceRect = default;

            for (int dz = 1; dz <= maxLevelsUp; dz++)
            {
                int parentZoom = missingKey.Z - dz;
                if (parentZoom < 0)
                {
                    return false;
                }

                int factor = 1 << dz;
                TileKey parentKey = new(
                    parentZoom,
                    missingKey.X / factor,
                    missingKey.Y / factor);
                if (!_tileCache.TryGetValue(parentKey, out Bitmap? parentBitmap))
                {
                    continue;
                }

                TouchTile(parentKey);
                float sourceWidth = parentBitmap.Width / (float)factor;
                float sourceHeight = parentBitmap.Height / (float)factor;
                int offsetX = missingKey.X % factor;
                int offsetY = missingKey.Y % factor;

                bitmap = parentBitmap;
                sourceRect = new RectangleF(
                    offsetX * sourceWidth,
                    offsetY * sourceHeight,
                    sourceWidth,
                    sourceHeight);
                return true;
            }

            return false;
        }

        private bool DrawParentBitmapRegion(
            Graphics graphics,
            MapCanvasEngine engine,
            Bitmap parentBitmap,
            TileKey missingKey,
            RectangleD missingProjectBounds,
            RectangleF source)
        {
            InterpolationMode savedMode = graphics.InterpolationMode;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            try
            {
                if (_projectIsWebMercator)
                {
                    RectangleF destination = WorldBoundsToScreenRectangle(
                        engine,
                        missingProjectBounds);
                    if (!IsValidDestination(destination))
                    {
                        return false;
                    }

                    DrawBitmapRegion(
                        graphics,
                        parentBitmap,
                        AlignDestinationToPixelGrid(destination),
                        source);
                    return true;
                }

                return DrawTileBitmapMesh(
                    graphics,
                    engine,
                    parentBitmap,
                    missingKey,
                    source);
            }
            finally
            {
                graphics.InterpolationMode = savedMode;
            }
        }

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
            bool lastInteractive;
            bool lastViewportAllowed;

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
                lastInteractive = _lastRenderInteractive;
                lastViewportAllowed = _lastViewportAllowed;
            }

            oldCts.Cancel();
            oldCts.Dispose();

            if (newCts.IsCancellationRequested)
            {
                return;
            }

            if (lastInteractive || !lastViewportAllowed)
            {
                return;
            }

            _ = FetchMissingTilesAsync(webMercatorBounds, zoom, newCts.Token);
        }

        private static RectangleD ExpandExtent(RectangleD bounds, double paddingFactor)
        {
            double padX = bounds.Width * paddingFactor;
            double padY = bounds.Height * paddingFactor;

            return new RectangleD(
                bounds.X - padX,
                bounds.Y - padY,
                bounds.Width + padX * 2.0,
                bounds.Height + padY * 2.0);
        }

        private static bool TryGetIntersection(
            RectangleD a,
            RectangleD b,
            out RectangleD intersection)
        {
            double left = Math.Max(a.Left, b.Left);
            double right = Math.Min(a.Right, b.Right);
            double bottom = Math.Max(a.Bottom, b.Bottom);
            double top = Math.Min(a.Top, b.Top);

            if (right <= left || top <= bottom)
            {
                intersection = default;
                return false;
            }

            intersection = new RectangleD(left, bottom, right - left, top - bottom);
            return true;
        }

        private void QueueBootstrapFetch(
            RectangleD webMercatorBounds,
            int desiredZoom)
        {
            int bootstrapZoom = SelectBootstrapZoom(
                webMercatorBounds,
                desiredZoom);
            if (bootstrapZoom < 0)
            {
                return;
            }

            _ = FetchMissingTilesAsync(
                webMercatorBounds,
                bootstrapZoom,
                CancellationToken.None,
                MaxBootstrapTilesPerFrame);
        }

        private static int SelectBootstrapZoom(
            RectangleD webMercatorBounds,
            int desiredZoom)
        {
            int bootstrapZoom = Math.Clamp(
                desiredZoom,
                0,
                MaxSupportedZoom);

            while (bootstrapZoom > 0 &&
                   TryCreateTileRange(
                       bootstrapZoom,
                       webMercatorBounds,
                       out TileRange tileRange) &&
                   tileRange.Count > MaxBootstrapTilesPerFrame)
            {
                bootstrapZoom--;
            }

            return TryCreateTileRange(
                bootstrapZoom,
                webMercatorBounds,
                out _)
                ? bootstrapZoom
                : -1;
        }

        private async Task FetchMissingTilesAsync(
            RectangleD webMercatorBounds,
            int zoom,
            CancellationToken cancellationToken,
            int maxTilesToFetch = int.MaxValue)
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
                        if (missing.Count >= maxTilesToFetch)
                        {
                            break;
                        }

                        TileKey key = new TileKey(zoom, x, y);
                        if (!_tileCache.ContainsKey(key) &&
                            !_noDataTiles.Contains(key) &&
                            _pendingFetches.Add(key))
                        {
                            missing.Add(key);
                        }
                    }

                    if (missing.Count >= maxTilesToFetch)
                    {
                        break;
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

                    // Disk cache avoids re-downloading valid imagery the user has already seen.
                    byte[]? bytes = TryReadDiskCache(key);
                    bool bytesFromDiskCache = bytes != null && bytes.Length > 0;
                    bool bytesDownloaded = false;

                    if (bytes == null || bytes.Length == 0)
                    {
                        string url = BuildTileUrl(key);
                        bytes = await SharedHttpClient
                            .GetByteArrayAsync(url, cancellationToken)
                            .ConfigureAwait(false);
                        bytesDownloaded = bytes != null && bytes.Length > 0;
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

                    if (bitmap != null && IsLikelyNoDataTile(bitmap))
                    {
                        bitmap.Dispose();
                        bitmap = null;

                        if (bytesFromDiskCache)
                        {
                            TryDeleteDiskCache(key);
                        }

                        lock (_renderSync)
                        {
                            _noDataTiles.Add(key);
                            if (_noDataTiles.Count > MaxCachedTiles * 8)
                            {
                                _noDataTiles.Clear();
                                _noDataTiles.Add(key);
                            }
                        }

                        return;
                    }

                    if (bytesDownloaded && bytes.Length > 0)
                    {
                        TryWriteDiskCache(key, bytes);
                    }
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
                    _noDataTiles.Remove(key);
                    bitmap = null; // ownership transferred to cache
                    // A new tile has arrived — the composite is stale and must be rebuilt.
                    _compositeValid = false;
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

            double rawZoom = Math.Log(
                InitialResolution / metersPerPixel,
                2.0);

            int desiredZoom = interactive
                ? (int)Math.Floor(rawZoom + 0.3)
                : (int)Math.Round(
                    rawZoom,
                    MidpointRounding.AwayFromZero);

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

        private static RectangleD GetWebMercatorTileRangeBounds(TileRange range)
        {
            RectangleD topLeft = GetWebMercatorTileBounds(
                new TileKey(range.Zoom, range.MinX, range.MinY));
            RectangleD bottomRight = GetWebMercatorTileBounds(
                new TileKey(range.Zoom, range.MaxX, range.MaxY));

            double left = MinX(topLeft);
            double right = MaxX(bottomRight);
            double top = MaxY(topLeft);
            double bottom = MinY(bottomRight);

            return new RectangleD(left, bottom, right - left, top - bottom);
        }

        private static bool TryTransformBounds(
            CoordinateTransformation transformation,
            RectangleD sourceBounds,
            out RectangleD targetBounds)
        {
            targetBounds = default;

            const int gridSize = 4;
            double sourceMinX = MinX(sourceBounds);
            double sourceMaxX = MaxX(sourceBounds);
            double sourceMinY = MinY(sourceBounds);
            double sourceMaxY = MaxY(sourceBounds);

            double targetMinX = double.MaxValue;
            double targetMaxX = double.MinValue;
            double targetMinY = double.MaxValue;
            double targetMaxY = double.MinValue;
            int transformedCount = 0;

            for (int row = 0; row < gridSize; row++)
            {
                double y = sourceMinY +
                    (sourceMaxY - sourceMinY) * row / (gridSize - 1.0);

                for (int column = 0; column < gridSize; column++)
                {
                    double x = sourceMinX +
                        (sourceMaxX - sourceMinX) * column / (gridSize - 1.0);

                    if (!TryTransformPoint(
                            transformation,
                            new PointD(x, y),
                            out PointD transformed))
                    {
                        continue;
                    }

                    targetMinX = Math.Min(targetMinX, transformed.X);
                    targetMaxX = Math.Max(targetMaxX, transformed.X);
                    targetMinY = Math.Min(targetMinY, transformed.Y);
                    targetMaxY = Math.Max(targetMaxY, transformed.Y);
                    transformedCount++;
                }
            }

            if (transformedCount == 0)
            {
                return false;
            }

            if (targetMaxX <= targetMinX || targetMaxY <= targetMinY)
            {
                return false;
            }

            targetBounds = new RectangleD(
                targetMinX,
                targetMinY,
                targetMaxX - targetMinX,
                targetMaxY - targetMinY);
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
            double left = Math.Max(MinX(bounds), MinX(AsiaWebMercatorBounds));
            double right = Math.Min(MaxX(bounds), MaxX(AsiaWebMercatorBounds));
            double bottom = Math.Max(MinY(bounds), MinY(AsiaWebMercatorBounds));
            double top = Math.Min(MaxY(bounds), MaxY(AsiaWebMercatorBounds));

            if (right <= left || top <= bottom)
            {
                return false;
            }

            clipped = new RectangleD(left, bottom, right - left, top - bottom);
            return true;
        }

        private static bool IsViewportWithinAllowedBounds(
            RectangleD visibleWebMercatorBounds)
        {
            return ContainsBounds(AsiaWebMercatorBounds, visibleWebMercatorBounds);
        }

        private static bool ContainsBounds(RectangleD outer, RectangleD inner)
        {
            return MinX(inner) >= MinX(outer) &&
                   MaxX(inner) <= MaxX(outer) &&
                   MinY(inner) >= MinY(outer) &&
                   MaxY(inner) <= MaxY(outer);
        }

        // ── Drawing helpers ────────────────────────────────────────────────────

        private bool TryCreateProjectedScreenQuad(
            MapCanvasEngine engine,
            RectangleD webMercatorBounds,
            out PointF[] destination)
        {
            destination = [];

            PointD topLeftWebMercator = new(
                MinX(webMercatorBounds),
                MaxY(webMercatorBounds));
            PointD topRightWebMercator = new(
                MaxX(webMercatorBounds),
                MaxY(webMercatorBounds));
            PointD bottomLeftWebMercator = new(
                MinX(webMercatorBounds),
                MinY(webMercatorBounds));

            if (!TryTransformPoint(
                    _webMercatorToProject,
                    topLeftWebMercator,
                    out PointD topLeftProject) ||
                !TryTransformPoint(
                    _webMercatorToProject,
                    topRightWebMercator,
                    out PointD topRightProject) ||
                !TryTransformPoint(
                    _webMercatorToProject,
                    bottomLeftWebMercator,
                    out PointD bottomLeftProject))
            {
                return false;
            }

            PointD topLeft = engine.WorldToScreen(topLeftProject);
            PointD topRight = engine.WorldToScreen(topRightProject);
            PointD bottomLeft = engine.WorldToScreen(bottomLeftProject);

            destination =
            [
                new PointF((float)topLeft.X, (float)topLeft.Y),
                new PointF((float)topRight.X, (float)topRight.Y),
                new PointF((float)bottomLeft.X, (float)bottomLeft.Y)
            ];

            return destination.All(point =>
                IsFiniteF(point.X) &&
                IsFiniteF(point.Y));
        }

        private void DrawBitmapQuad(
            Graphics graphics,
            Bitmap bitmap,
            PointF[] destination,
            RectangleF source)
        {
            RectangleF expandedSource = ExpandSourceForSeams(
                source,
                bitmap.Width,
                bitmap.Height);

            PixelOffsetMode oldPixelOffset = graphics.PixelOffsetMode;
            InterpolationMode oldInterpolation = graphics.InterpolationMode;
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            try
            {
                if (_opacityImageAttributes == null)
                {
                    using ImageAttributes attributes = new();
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(
                        bitmap,
                        destination,
                        expandedSource,
                        GraphicsUnit.Pixel,
                        attributes);
                    return;
                }

                graphics.DrawImage(
                    bitmap,
                    destination,
                    expandedSource,
                    GraphicsUnit.Pixel,
                    _opacityImageAttributes);
            }
            finally
            {
                graphics.InterpolationMode = oldInterpolation;
                graphics.PixelOffsetMode = oldPixelOffset;
            }
        }

        private void DrawBitmapRegion(
            Graphics graphics,
            Bitmap bitmap,
            RectangleF destination,
            RectangleF source,
            bool isPlaceholder = false)
        {
            // Always use integer Rectangle destination — the 3-point parallelogram overload
            // causes 1-pixel seam bleed between adjacent tiles in GDI+.
            Rectangle dest = CreateIntegerDestinationRectangle(destination);

            InterpolationMode oldInterpolation = graphics.InterpolationMode;
            graphics.InterpolationMode = isPlaceholder
                ? InterpolationMode.HighQualityBicubic
                : InterpolationMode.NearestNeighbor;

            try
            {
                ImageAttributes ia = _opacityImageAttributes ?? CreateDefaultImageAttributes();
                bool ownedIa = _opacityImageAttributes == null;
                try
                {
                    graphics.DrawImage(
                        bitmap,
                        dest,
                        source.X,
                        source.Y,
                        source.Width,
                        source.Height,
                        GraphicsUnit.Pixel,
                        ia);
                }
                finally
                {
                    if (ownedIa) ia.Dispose();
                }
            }
            finally
            {
                graphics.InterpolationMode = oldInterpolation;
            }

            
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

        private static ImageAttributes CreateDefaultImageAttributes()
        {
            ImageAttributes ia = new ImageAttributes();
            ia.SetWrapMode(WrapMode.TileFlipXY);
            return ia;
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

        private static bool IsLikelyNoDataTile(Bitmap bitmap)
        {
            int step = Math.Max(1, Math.Min(bitmap.Width, bitmap.Height) / 32);
            int total = 0;
            int neutral = 0;
            int brightNeutral = 0;
            double sum = 0.0;
            double sumSq = 0.0;

            for (int y = 0; y < bitmap.Height; y += step)
            {
                for (int x = 0; x < bitmap.Width; x += step)
                {
                    Color color = bitmap.GetPixel(x, y);
                    int max = Math.Max(color.R, Math.Max(color.G, color.B));
                    int min = Math.Min(color.R, Math.Min(color.G, color.B));
                    int brightness = (color.R + color.G + color.B) / 3;

                    if (max - min <= 14 &&
                        brightness >= 120 &&
                        brightness <= 245)
                    {
                        neutral++;
                    }

                    if (max - min <= 18 && brightness >= 220)
                    {
                        brightNeutral++;
                    }

                    sum += brightness;
                    sumSq += brightness * brightness;
                    total++;
                }
            }

            if (total == 0)
            {
                return false;
            }

            double neutralRatio = neutral / (double)total;
            double brightRatio = brightNeutral / (double)total;
            double mean = sum / total;
            double variance = Math.Max(0.0, (sumSq / total) - (mean * mean));

            // Esri "Map data not yet available" tiles are mostly flat gray with
            // small bright text. Real imagery has much more chroma and texture.
            return neutralRatio > 0.78 &&
                   brightRatio > 0.015 &&
                   variance < 1800.0;
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
            _noDataTiles.Clear();
            _projectTileBoundsCache.Clear();

            _compositeValid = false;
            _compositeBitmap?.Dispose();
            _compositeBitmap = null;
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

        private void TryDeleteDiskCache(TileKey key)
        {
            try
            {
                string path = GetDiskCachePath(key);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Disk cache cleanup is best-effort.
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
            // Only Asia extent, not full world.
            // Full WebMercator -> UTM transform is unreliable and can make the layer disappear.
            return TryTransformBounds(
                    webMercatorToProject,
                    AsiaWebMercatorBounds,
                    out RectangleD projectedAsia)
                ? projectedAsia
                : AsiaWebMercatorBounds;
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

        private static RectangleF GetSourceSubRectangle(
            RectangleF source,
            int column,
            int row,
            int subdivisions)
        {
            float width = source.Width / subdivisions;
            float height = source.Height / subdivisions;
            return new RectangleF(
                source.Left + column * width,
                source.Top + row * height,
                width,
                height);
        }

        private static RectangleF GetSourceSubRectangle(
            RectangleF source,
            int column,
            int row,
            int columns,
            int rows)
        {
            float width = source.Width / columns;
            float height = source.Height / rows;
            return new RectangleF(
                source.Left + column * width,
                source.Top + row * height,
                width,
                height);
        }

        private static RectangleF ExpandSourceForSeams(
            RectangleF source,
            int bitmapWidth,
            int bitmapHeight)
        {
            const float inflate = 0.5f;

            float left = Math.Max(0.0f, source.Left - inflate);
            float top = Math.Max(0.0f, source.Top - inflate);
            float right = Math.Min(bitmapWidth, source.Right + inflate);
            float bottom = Math.Min(bitmapHeight, source.Bottom + inflate);

            if (right <= left)
            {
                right = Math.Min(bitmapWidth, left + 1.0f);
            }

            if (bottom <= top)
            {
                bottom = Math.Min(bitmapHeight, top + 1.0f);
            }

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        private static PointF[] ExpandDestinationQuadForSeams(PointF[] destination)
        {
            if (destination.Length < 3)
            {
                return destination;
            }

            PointF topLeft = destination[0];
            PointF topRight = destination[1];
            PointF bottomLeft = destination[2];
            PointF bottomRight = new(
                topRight.X + bottomLeft.X - topLeft.X,
                topRight.Y + bottomLeft.Y - topLeft.Y);

            float centerX =
                (topLeft.X + topRight.X + bottomLeft.X + bottomRight.X) / 4.0f;
            float centerY =
                (topLeft.Y + topRight.Y + bottomLeft.Y + bottomRight.Y) / 4.0f;

            return
            [
                ExpandPointFromCenter(topLeft, centerX, centerY),
                ExpandPointFromCenter(topRight, centerX, centerY),
                ExpandPointFromCenter(bottomLeft, centerX, centerY)
            ];
        }

        private static PointF ExpandPointFromCenter(
            PointF point,
            float centerX,
            float centerY)
        {
            const float overlapPixels = 1.25f;
            float dx = point.X - centerX;
            float dy = point.Y - centerY;
            float length = MathF.Sqrt(dx * dx + dy * dy);
            if (length <= 0.001f)
            {
                return point;
            }

            float scale = (length + overlapPixels) / length;
            return new PointF(
                centerX + dx * scale,
                centerY + dy * scale);
        }

        private static RectangleD GetSubTileWebMercatorBounds(
            RectangleD tileBounds,
            int column,
            int row,
            int subdivisions)
        {
            double minX = MinX(tileBounds);
            double maxX = MaxX(tileBounds);
            double minY = MinY(tileBounds);
            double maxY = MaxY(tileBounds);
            double width = (maxX - minX) / subdivisions;
            double height = (maxY - minY) / subdivisions;

            double left = minX + column * width;
            double top = maxY - row * height;
            double bottom = top - height;
            return new RectangleD(left, bottom, width, height);
        }

        private static RectangleD GetSubBounds(
            RectangleD bounds,
            int column,
            int row,
            int columns,
            int rows)
        {
            double minX = MinX(bounds);
            double maxX = MaxX(bounds);
            double minY = MinY(bounds);
            double maxY = MaxY(bounds);
            double width = (maxX - minX) / columns;
            double height = (maxY - minY) / rows;

            double left = minX + column * width;
            double top = maxY - row * height;
            double bottom = top - height;
            return new RectangleD(left, bottom, width, height);
        }

        private static bool IsValidSource(RectangleF source) =>
            IsFiniteF(source.Left) &&
            IsFiniteF(source.Top) &&
            IsFiniteF(source.Width) &&
            IsFiniteF(source.Height) &&
            source.Width > 0.0f &&
            source.Height > 0.0f;

        private static RectangleD CreateWebMercatorBoundsFromLonLat(
            double west,
            double south,
            double east,
            double north)
        {
            PointD southWest = ProjectLonLatToWebMercator(west, south);
            PointD northEast = ProjectLonLatToWebMercator(east, north);
            double left = Math.Min(southWest.X, northEast.X);
            double right = Math.Max(southWest.X, northEast.X);
            double bottom = Math.Min(southWest.Y, northEast.Y);
            double top = Math.Max(southWest.Y, northEast.Y);
            return new RectangleD(left, bottom, right - left, top - bottom);
        }

        private static PointD ProjectLonLatToWebMercator(double longitude, double latitude)
        {
            double clampedLatitude = Math.Clamp(latitude, -85.05112878, 85.05112878);
            double x = longitude / 180.0 * WebMercatorExtent;
            double radians = clampedLatitude * Math.PI / 180.0;
            double y = Math.Log(Math.Tan((Math.PI / 4.0) + (radians / 2.0))) *
                WebMercatorExtent / Math.PI;
            return new PointD(
                Math.Clamp(x, -WebMercatorExtent, WebMercatorExtent),
                Math.Clamp(y, -WebMercatorExtent, WebMercatorExtent));
        }

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

        private static RectangleF AlignDestinationToPixelGrid(RectangleF destination)
        {
            // Floor left/top and Ceiling right/bottom so adjacent tiles always share an
            // integer pixel edge — no sub-pixel gap can appear between neighbours.
            float left   = (float)Math.Floor(destination.Left);
            float top    = (float)Math.Floor(destination.Top);
            float right  = (float)Math.Ceiling(destination.Right);
            float bottom = (float)Math.Ceiling(destination.Bottom);

            if (right <= left)
            {
                right = left + 1.0f;
            }

            if (bottom <= top)
            {
                bottom = top + 1.0f;
            }

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

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
