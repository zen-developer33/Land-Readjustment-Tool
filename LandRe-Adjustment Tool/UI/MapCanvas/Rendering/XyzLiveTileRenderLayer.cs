using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using OSGeo.GDAL;
using OSGeo.OSR;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    internal enum LiveTileFetchStatusKind
    {
        Idle,
        Fetching,
        Disconnected
    }

    internal sealed class LiveTileFetchStatusChangedEventArgs : EventArgs
    {
        public LiveTileFetchStatusChangedEventArgs(
            bool isFetching,
            int pendingTileCount)
            : this(
                isFetching
                    ? LiveTileFetchStatusKind.Fetching
                    : LiveTileFetchStatusKind.Idle,
                pendingTileCount)
        {
        }

        public LiveTileFetchStatusChangedEventArgs(
            LiveTileFetchStatusKind statusKind,
            int pendingTileCount)
        {
            StatusKind = statusKind;
            PendingTileCount = Math.Max(0, pendingTileCount);
        }

        public LiveTileFetchStatusKind StatusKind { get; }
        public bool IsFetching => StatusKind == LiveTileFetchStatusKind.Fetching;
        public bool IsDisconnected => StatusKind == LiveTileFetchStatusKind.Disconnected;
        public int PendingTileCount { get; }
    }

    /// <summary>
    /// Renders a live XYZ/TMS tile layer by fetching tiles on demand from the internet.
    /// Uses an LRU memory cache (512 tiles), a persistent disk cache, a bounded
    /// semaphore (6 concurrent HTTP requests), per-viewport cancellation, and an
    /// 50 ms debounce so fetches only start once the viewport settles.
    /// </summary>
    internal sealed class XyzLiveTileRenderLayer : IRasterRenderLayer
    {


        private readonly RectangleD _validProjectExtent;
        // ── Constants ──────────────────────────────────────────────────────────
        private const int TilePixelSize = 256;
        private const int MaxCachedTiles = 512;
        private const int MaxTilesPerFrame = 256;
        private const int MaxBootstrapTilesPerFrame = 16;
        private const int MaxFallbackTilesPerFrame = 64;
        private const int MaxConcurrentFetches = 10;
        // Prefetch budgets keep look-ahead fetching from starving the visible
        // viewport: only a bounded ring/next-zoom slice is pulled per settle.
        private const int MaxPrefetchTilesPerFrame = 48;
        private const double PrefetchRingFactor = 0.5;
        private const int DebounceMilliseconds = 50;
        private const int FetchCompleteQuietMilliseconds = 500;
        private const int InternetProbeTimeoutMilliseconds = 2500;
        private const int InternetConnectivityCacheMilliseconds = 6000;
        private const int MaxSupportedZoom = 22;
        private const int BingLiveMaxFetchZoom = 14;
        private const int ProjectedTileMeshSubdivisions = 4;
        private const int MinFreshTileZoom = 0;
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
        private static readonly object FetchStatusSync = new();
        private static readonly object InternetConnectivitySync = new();
        private static readonly System.Threading.Timer FetchCompleteQuietTimer =
            new(OnFetchCompleteQuietElapsed, state: null, Timeout.Infinite, Timeout.Infinite);
        private static readonly Uri[] InternetProbeUris =
        [
            new("https://www.msftconnecttest.com/connecttest.txt"),
            new("https://www.google.com/generate_204")
        ];
        private static int _activeFetchTileCount;
        private static bool _fetchStatusReportedFetching;
        private static DateTime _lastInternetConnectivityCheckUtc = DateTime.MinValue;
        private static bool _lastInternetConnectivityAvailable = true;

        internal static event EventHandler<LiveTileFetchStatusChangedEventArgs>? FetchStatusChanged;

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
        private readonly Dictionary<TileKey, Bitmap> _tileCache = new Dictionary<TileKey, Bitmap>();
        private readonly LinkedList<TileKey> _tileLru = new LinkedList<TileKey>();
        private readonly Dictionary<TileKey, LinkedListNode<TileKey>> _tileLruNodes = new Dictionary<TileKey, LinkedListNode<TileKey>>();
        private readonly Dictionary<TileKey, int> _pendingFetches = new Dictionary<TileKey, int>();
        private readonly HashSet<TileKey> _noDataTiles = new HashSet<TileKey>();
        private readonly Dictionary<TileKey, RectangleD> _projectTileBoundsCache = new Dictionary<TileKey, RectangleD>();
        private int _pendingFetchGeneration;

        // ── Tile source ────────────────────────────────────────────────────────
        private readonly string _urlTemplate;
        private readonly string _diskCacheRoot;
        private readonly int _maxSourceZoom;
        private readonly bool _allowParentPlaceholders;

        // ── Tile-ready callback ────────────────────────────────────────────────
        private readonly Action? _invalidateCallback;

        // ── Debounce and viewport tracking ────────────────────────────────────
        private CancellationTokenSource _viewportCts = new CancellationTokenSource();
        private readonly System.Threading.Timer _debounceTimer;
        private RectangleD _lastWebMercatorBounds;
        private int _lastZoom;

        // ── Opacity ────────────────────────────────────────────────────────────

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
        private bool _internetFetchSuspended;

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
            int maxSourceZoom,
            bool allowParentPlaceholders,
            Action? invalidateCallback)
        {
            LayerId = layer.Id;
            Name = layer.Name;
            FilePath = filePath;
            Transparency = 0;
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
            _maxSourceZoom = GetEffectiveMaxSourceZoom(
                urlTemplate,
                Math.Clamp(maxSourceZoom, 0, MaxSupportedZoom));
            _allowParentPlaceholders = allowParentPlaceholders;
            _invalidateCallback = invalidateCallback;
            _debounceTimer = new System.Threading.Timer(
                OnDebounceElapsed,
                state: null,
                dueTime: Timeout.Infinite,
                period: Timeout.Infinite);
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
            Action? invalidateCallback = null,
            string? projectSrsDefinition = null)
        {
            GdalBootstrapper.ConfigureAll();
            if (!GdalConfiguration.Usable)
            {
                throw new InvalidOperationException(
                    "GDAL/PROJ is not configured correctly. Live XYZ tile reprojection cannot continue.");
            }

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
            int maxSourceZoom = ResolveMaxSourceZoom(wmsXmlPath, urlTemplate);

            string diskCacheRoot = BuildDiskCacheRoot(urlTemplate);

            SpatialReference webMercatorSrs =
                CreateSpatialReference(WebMercatorSrsDefinition);
            SpatialReference projectSrs =
                TryCreateSpatialReference(projectSrsDefinition) ??
                ExtractProjectSrs(filePath) ??
                ExtractProjectSrsFromGdal(filePath) ??
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
                maxSourceZoom,
                ShouldAllowParentPlaceholders(urlTemplate),
                invalidateCallback);
        }

        // ── IRasterRenderLayer implementation ──────────────────────────────────

        public bool RenderVisible(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool interactive,
            MapRenderBackend renderBackend = MapRenderBackend.GdiPlus,
            CancellationToken cancellationToken = default)
        {
            bool drawnAny;
            RectangleD fetchWebMercatorBounds = default;
            int fetchZoom = 0;
            bool lastInteractive = interactive;
            bool lastViewportAllowed = false;
            bool fetchSuspended = false;
            HashSet<TileKey>? fallbackFetchCandidates = null;

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
                fetchSuspended = _internetFetchSuspended;

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
                    System.Diagnostics.Debug.WriteLine(
                        $"[XyzLiveTileRenderLayer] Viewport transform/clip failed for '{Name}'. " +
                        $"Project bounds: X={visibleWorldBounds.X}, Y={visibleWorldBounds.Y}, " +
                        $"W={visibleWorldBounds.Width}, H={visibleWorldBounds.Height}.");
                    _lastViewportAllowed = false;
                    return false;
                }

                _lastViewportAllowed = true;
                lastViewportAllowed = true;

                int zoom = Math.Max(
                    MinFreshTileZoom,
                    SelectZoom(engine, clippedWebMercatorBounds, interactive));

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
                using RasterImageRenderContext imageContext = new(
                    graphics,
                    renderBackend,
                    canvasSize);

                if (interactive)
                {
                    // ── Interactive: NEVER draw tiles. Just stretch the cached composite. ──
                    // Pan/zoom = one DrawImage call, zero tile iteration, zero reprojection.
                    drawnAny = _compositeBitmap != null &&
                               _compositeCanvasSize == canvasSize &&
                               DrawCompositeCache(imageContext, engine);
                }
                else
                {
                    // ── Settled: rebuild composite from cached tiles, then blit 1:1. ────
                    fallbackFetchCandidates = new HashSet<TileKey>();
                    drawnAny = RebuildCompositeAndDraw(
                        imageContext,
                        renderBackend,
                        graphics,
                        engine,
                        visibleWorldBounds,
                        tileRange,
                        canvasSize,
                        cancellationToken,
                        fallbackFetchCandidates);
                }
            }

            // Arm the debounce timer outside the lock so the render thread is not
            // penalised for the timer-system call.
            if (!_disposed && !fetchSuspended)
            {
                if (_allowParentPlaceholders &&
                    !lastInteractive &&
                    lastViewportAllowed &&
                    !drawnAny)
                {
                    QueueBootstrapFetch(fetchWebMercatorBounds, fetchZoom);
                }

                if (!lastInteractive && lastViewportAllowed)
                {
                    QueueFallbackFetches(fallbackFetchCandidates);
                    _debounceTimer.Change(DebounceMilliseconds, Timeout.Infinite);
                }
            }

            return drawnAny;
        }

        public bool RenderVisible(
            IMapRenderSurface surface,
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
            bool fetchSuspended = false;
            HashSet<TileKey>? fallbackFetchCandidates = null;

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
                fetchSuspended = _internetFetchSuspended;

                if (interactive && !_viewportCts.IsCancellationRequested)
                {
                    _viewportCts.Cancel();
                }

                if (!TryTransformBounds(
                        _projectToWebMercator,
                        visibleWorldBounds,
                        out RectangleD visibleWebMercatorBounds) ||
                    !TryClipWebMercatorBounds(
                        visibleWebMercatorBounds,
                        out RectangleD clippedWebMercatorBounds))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[XyzLiveTileRenderLayer] Viewport transform/clip failed for '{Name}'. " +
                        $"Project bounds: X={visibleWorldBounds.X}, Y={visibleWorldBounds.Y}, " +
                        $"W={visibleWorldBounds.Width}, H={visibleWorldBounds.Height}.");
                    _lastViewportAllowed = false;
                    return false;
                }

                _lastViewportAllowed = true;
                lastViewportAllowed = true;

                int zoom = Math.Max(
                    MinFreshTileZoom,
                    SelectZoom(engine, clippedWebMercatorBounds, interactive));

                if (!TryCreateTileRange(zoom, clippedWebMercatorBounds, out TileRange tileRange))
                {
                    return false;
                }

                _lastWebMercatorBounds = clippedWebMercatorBounds;
                _lastZoom = zoom;
                fetchWebMercatorBounds = clippedWebMercatorBounds;
                fetchZoom = zoom;

                using IDisposable asiaClip = ApplyAsiaSurfaceClip(surface, engine);
                using RasterImageRenderContext imageContext = new(surface);
                if (!interactive)
                {
                    fallbackFetchCandidates = new HashSet<TileKey>();
                }

                drawnAny = DrawVisibleTiles(
                    imageContext,
                    engine,
                    visibleWorldBounds,
                    tileRange,
                    cancellationToken,
                    interactive ? null : fallbackFetchCandidates);
            }

            if (!_disposed && !fetchSuspended)
            {
                if (_allowParentPlaceholders &&
                    !lastInteractive &&
                    lastViewportAllowed &&
                    !drawnAny)
                {
                    QueueBootstrapFetch(fetchWebMercatorBounds, fetchZoom);
                }

                if (!lastInteractive && lastViewportAllowed)
                {
                    QueueFallbackFetches(fallbackFetchCandidates);
                    _debounceTimer.Change(DebounceMilliseconds, Timeout.Infinite);
                }
            }

            return drawnAny;
        }

        /// <summary>
        /// Paints the pre-built composite bitmap translated/scaled to the current viewport.
        /// One DrawImage call per frame — O(1) regardless of tile count.
        /// </summary>
        private bool DrawCompositeCache(RasterImageRenderContext imageContext, MapCanvasEngine engine)
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

            imageContext.DrawBitmap(
                _compositeBitmap,
                new RectangleF((float)tl.X, (float)tl.Y, dstW, dstH),
                new RectangleF(0, 0, _compositeBitmap.Width, _compositeBitmap.Height),
                GetOpacityFactor(),
                ImageInterpolation.HighQuality,
                tileFlipXY: false);
            return true;
        }

        /// <summary>
        /// Draws all cached tiles into an offscreen composite bitmap (applying the Asia clip
        /// and pixel-perfect alignment), then blits it 1:1 to the canvas.
        /// Sets <see cref="_compositeValid"/> so subsequent interactive frames can fast-path.
        /// </summary>
        private bool RebuildCompositeAndDraw(
            RasterImageRenderContext imageContext,
            MapRenderBackend renderBackend,
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            TileRange tileRange,
            Size canvasSize,
            CancellationToken cancellationToken,
            HashSet<TileKey>? fallbackFetchCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

                    using RasterImageRenderContext compositeContext = new(
                        cg,
                        renderBackend,
                        canvasSize);
                    drawnAny = DrawVisibleTileMosaic(
                        compositeContext,
                        renderBackend,
                        engine,
                        visibleWorldBounds,
                        tileRange,
                        cancellationToken,
                        fallbackFetchCandidates);
                }

                cancellationToken.ThrowIfCancellationRequested();

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
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.None;
                    cancellationToken.ThrowIfCancellationRequested();
                    imageContext.DrawBitmap(
                        _compositeBitmap,
                        new RectangleF(0, 0, _compositeBitmap.Width, _compositeBitmap.Height),
                        new RectangleF(0, 0, _compositeBitmap.Width, _compositeBitmap.Height),
                        GetOpacityFactor(),
                        ImageInterpolation.NearestNeighbor,
                        tileFlipXY: false);
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
                        imageContext,
                        renderBackend,
                        engine,
                        visibleWorldBounds,
                        tileRange,
                        cancellationToken,
                        fallbackFetchCandidates);
                }
                finally
                {
                    graphics.Restore(gs);
                }
            }

            return drawnAny;
        }

        private bool RebuildCompositeAndDraw(
            RasterImageRenderContext imageContext,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            TileRange tileRange,
            Size canvasSize,
            CancellationToken cancellationToken,
            HashSet<TileKey>? fallbackFetchCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
                    cg.SmoothingMode = SmoothingMode.None;
                    cg.InterpolationMode = InterpolationMode.NearestNeighbor;
                    cg.PixelOffsetMode = PixelOffsetMode.None;
                    cg.CompositingQuality = CompositingQuality.HighSpeed;
                    cg.CompositingMode = CompositingMode.SourceCopy;
                    ApplyAsiaScreenClip(cg, engine);

                    using RasterImageRenderContext compositeContext = new(
                        cg,
                        MapRenderBackend.GdiPlus,
                        canvasSize);
                    drawnAny = DrawVisibleTileMosaic(
                        compositeContext,
                        MapRenderBackend.GdiPlus,
                        engine,
                        visibleWorldBounds,
                        tileRange,
                        cancellationToken,
                        fallbackFetchCandidates);
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (drawnAny)
                {
                    _compositeWorldBounds = visibleWorldBounds;
                    _compositeCanvasSize = canvasSize;
                    _compositeZoomScale = engine.ZoomScale;
                    _compositeValid = true;

                    imageContext.DrawBitmap(
                        _compositeBitmap,
                        new RectangleF(0, 0, _compositeBitmap.Width, _compositeBitmap.Height),
                        new RectangleF(0, 0, _compositeBitmap.Width, _compositeBitmap.Height),
                        GetOpacityFactor(),
                        ImageInterpolation.NearestNeighbor,
                        tileFlipXY: false);
                }
            }
            else
            {
                drawnAny = DrawVisibleTileMosaic(
                    imageContext,
                    MapRenderBackend.GdiPlus,
                    engine,
                    visibleWorldBounds,
                    tileRange,
                    cancellationToken,
                    fallbackFetchCandidates);
            }

            return drawnAny;
        }

        /// <summary>
        /// Clips <paramref name="g"/> to the Asia extent projected into current screen space.
        /// Tiles that spill beyond the Asian boundary are masked out.
        /// </summary>
        private void ApplyAsiaScreenClip(Graphics g, MapCanvasEngine engine)
        {
            if (!TryGetAsiaScreenClipRectangle(engine, out RectangleF clipRectangle))
            {
                return;
            }

            g.SetClip(clipRectangle, CombineMode.Intersect);
        }

        private IDisposable ApplyAsiaSurfaceClip(IMapRenderSurface surface, MapCanvasEngine engine)
        {
            IDisposable state = surface.SaveState();
            if (!TryGetAsiaScreenClipRectangle(engine, out RectangleF clipRectangle))
            {
                return state;
            }

            IMapPathBuilder pathBuilder = surface.CreatePath();
            pathBuilder.AddRectangle(clipRectangle);
            using IMapPath clipPath = pathBuilder.Build();
            surface.ClipPath(clipPath);
            return state;
        }

        private bool TryGetAsiaScreenClipRectangle(MapCanvasEngine engine, out RectangleF clipRectangle)
        {
            clipRectangle = RectangleF.Empty;
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
                return false;
            }

            PointD tl = engine.WorldToScreen(
                new PointD(MinX(asiaBoundsProject), MaxY(asiaBoundsProject)));
            PointD br = engine.WorldToScreen(
                new PointD(MaxX(asiaBoundsProject), MinY(asiaBoundsProject)));

            if (!IsFiniteD(tl.X) || !IsFiniteD(tl.Y) ||
                !IsFiniteD(br.X) || !IsFiniteD(br.Y))
            {
                return false;
            }

            float left = (float)Math.Min(tl.X, br.X);
            float top = (float)Math.Min(tl.Y, br.Y);
            float right = (float)Math.Max(tl.X, br.X);
            float bottom = (float)Math.Max(tl.Y, br.Y);

            if (right <= left || bottom <= top)
            {
                return false;
            }

            clipRectangle = new RectangleF(left, top, right - left, bottom - top);
            return true;
        }

        public void UpdateRenderState(bool isVisible, int transparency)
        {
            lock (_renderSync)
            {
                IsVisible = isVisible;
                Transparency = 0;
            }
        }

        public void InvalidateCache()
        {
            lock (_renderSync)
            {
                ClearTileCache();
            }
        }

        public void CancelPendingTileFetches()
        {
            if (_disposed)
            {
                return;
            }

            CancellationTokenSource oldCts;
            lock (_renderSync)
            {
                _debounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
                oldCts = _viewportCts;
                _viewportCts = new CancellationTokenSource();
                _lastRenderInteractive = true;
                _lastViewportAllowed = false;
                _pendingFetches.Clear();
                _pendingFetchGeneration++;
            }

            oldCts.Cancel();
            oldCts.Dispose();
        }

        public void SetInternetFetchingSuspended(bool suspended)
        {
            if (_disposed)
            {
                return;
            }

            CancellationTokenSource? oldCts = null;
            lock (_renderSync)
            {
                if (_internetFetchSuspended == suspended)
                {
                    return;
                }

                _internetFetchSuspended = suspended;
                if (suspended)
                {
                    _debounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    oldCts = _viewportCts;
                    _viewportCts = new CancellationTokenSource();
                    _lastRenderInteractive = true;
                    _lastViewportAllowed = false;
                    _pendingFetches.Clear();
                    _pendingFetchGeneration++;
                }
            }

            if (oldCts != null)
            {
                oldCts.Cancel();
                oldCts.Dispose();
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
            RasterImageRenderContext imageContext,
            MapRenderBackend renderBackend,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            TileRange tileRange,
            CancellationToken cancellationToken,
            HashSet<TileKey>? fallbackFetchCandidates)
        {
            int columns = tileRange.MaxX - tileRange.MinX + 1;
            int rows = tileRange.MaxY - tileRange.MinY + 1;
            if (columns <= 0 || rows <= 0)
            {
                return false;
            }

            int mosaicWidth = columns * TilePixelSize;
            int mosaicHeight = rows * TilePixelSize;
            bool allCellsCovered = true;
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
                    using RasterImageRenderContext mosaicContext = new(
                        mosaicGraphics,
                        renderBackend,
                        new Size(mosaicWidth, mosaicHeight));

                    for (int y = tileRange.MinY; y <= tileRange.MaxY; y++)
                {
                    for (int x = tileRange.MinX; x <= tileRange.MaxX; x++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        TileKey key = new TileKey(tileRange.Zoom, x, y);
                        // The range was already computed from the current viewport
                        // transformed into WebMercator. A second per-tile MUTM
                        // footprint test is fragile for custom projected CRSs and
                        // can incorrectly skip every visible tile.

                        Rectangle destination = new(
                            (x - tileRange.MinX) * TilePixelSize,
                            (y - tileRange.MinY) * TilePixelSize,
                            TilePixelSize,
                            TilePixelSize);

                        if (TryGetCachedTileBitmap(key, out Bitmap? bitmap) &&
                            bitmap != null)
                        {
                            DrawMosaicBitmapRegion(
                                mosaicContext,
                                bitmap,
                                destination,
                                new RectangleF(0, 0, bitmap.Width, bitmap.Height),
                                smooth: false);
                            hasPixels = true;
                            continue;
                        }

                        if (_allowParentPlaceholders &&
                            TryGetParentPlaceholder(
                                key,
                                out Bitmap? parentBitmap,
                                out RectangleF parentSource) &&
                            parentBitmap != null)
                        {
                            DrawMosaicBitmapRegion(
                                mosaicContext,
                                parentBitmap,
                                destination,
                                parentSource,
                                smooth: true);
                            hasPixels = true;
                            continue;
                        }

                        CollectParentFallbackFetchCandidates(
                            key,
                            fallbackFetchCandidates);
                        allCellsCovered = false;
                    }
                }
            }

            if (!hasPixels || !allCellsCovered)
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            return DrawMosaicSurface(
                imageContext,
                engine,
                mosaic,
                GetWebMercatorTileRangeBounds(tileRange),
                cancellationToken);
        }

        private bool DrawMosaicSurface(
            RasterImageRenderContext imageContext,
            MapCanvasEngine engine,
            Bitmap mosaic,
            RectangleD webMercatorBounds,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
                    imageContext,
                    mosaic,
                    AlignDestinationToPixelGrid(destination),
                    source);
                return true;
            }

            return DrawWarpedMosaicSurface(
                imageContext,
                engine,
                mosaic,
                webMercatorBounds,
                cancellationToken);
        }

        private bool DrawWarpedMosaicSurface(
            RasterImageRenderContext imageContext,
            MapCanvasEngine engine,
            Bitmap mosaic,
            RectangleD webMercatorBounds,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Size canvasSize = engine.CanvasSize;
            if (canvasSize.Width <= 0 || canvasSize.Height <= 0)
            {
                return false;
            }

            using Bitmap warped = new(
                canvasSize.Width,
                canvasSize.Height,
                PixelFormat.Format32bppPArgb);

            BitmapData sourceData = mosaic.LockBits(
                new Rectangle(0, 0, mosaic.Width, mosaic.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppPArgb);
            BitmapData targetData = warped.LockBits(
                new Rectangle(0, 0, warped.Width, warped.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);

            // 32bpp surfaces have a 4-byte aligned stride, so treat each pixel as a
            // single int (BGRA) instead of four bytes — one copy per pixel instead
            // of four, with no change to the sampled value.
            int sourceStride = Math.Abs(sourceData.Stride) / 4;
            int targetStride = Math.Abs(targetData.Stride) / 4;
            int[] sourcePixels = new int[sourceStride * mosaic.Height];
            int[] targetPixels = new int[targetStride * warped.Height];

            try
            {
                Marshal.Copy(sourceData.Scan0, sourcePixels, 0, sourcePixels.Length);
                cancellationToken.ThrowIfCancellationRequested();
                FillWarpedMosaicPixels(
                    sourcePixels,
                    sourceStride,
                    mosaic.Width,
                    mosaic.Height,
                    targetPixels,
                    targetStride,
                    warped.Width,
                    warped.Height,
                    engine,
                    webMercatorBounds,
                    cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                Marshal.Copy(targetPixels, 0, targetData.Scan0, targetPixels.Length);
            }
            finally
            {
                mosaic.UnlockBits(sourceData);
                warped.UnlockBits(targetData);
            }

            cancellationToken.ThrowIfCancellationRequested();
            imageContext.DrawBitmap(
                warped,
                new RectangleF(0, 0, warped.Width, warped.Height),
                new RectangleF(0, 0, warped.Width, warped.Height),
                GetOpacityFactor(),
                ImageInterpolation.NearestNeighbor,
                tileFlipXY: false);

            return true;
        }

        private void FillWarpedMosaicPixels(
            int[] sourcePixels,
            int sourceStride,
            int sourceWidth,
            int sourceHeight,
            int[] targetPixels,
            int targetStride,
            int targetWidth,
            int targetHeight,
            MapCanvasEngine engine,
            RectangleD webMercatorBounds,
            CancellationToken cancellationToken)
        {
            const int cellSize = 24;
            double sourceMinX = MinX(webMercatorBounds);
            double sourceMaxX = MaxX(webMercatorBounds);
            double sourceMinY = MinY(webMercatorBounds);
            double sourceMaxY = MaxY(webMercatorBounds);
            double sourceScaleX = (sourceWidth - 1.0) / (sourceMaxX - sourceMinX);
            double sourceScaleY = (sourceHeight - 1.0) / (sourceMaxY - sourceMinY);

            int cellCols = (targetWidth + cellSize - 1) / cellSize;
            int cellRows = (targetHeight + cellSize - 1) / cellSize;
            if (cellCols <= 0 || cellRows <= 0)
            {
                return;
            }

            int cornerCols = cellCols + 1;
            int cornerRows = cellRows + 1;
            int[] colBoundaries = new int[cornerCols];
            for (int c = 0; c < cornerCols; c++)
            {
                colBoundaries[c] = Math.Min(targetWidth, c * cellSize);
            }

            int[] rowBoundaries = new int[cornerRows];
            for (int r = 0; r < cornerRows; r++)
            {
                rowBoundaries[r] = Math.Min(targetHeight, r * cellSize);
            }

            // ── Phase 1: coarse reprojection mesh ──────────────────────────────
            // GDAL's CoordinateTransformation is NOT thread-safe, so every
            // screen->WebMercator transform happens here on a single thread. Each
            // shared cell corner is transformed exactly once instead of up to four
            // times (the old per-cell code re-transformed neighbouring corners),
            // which alone cuts GDAL transform calls roughly 4x.
            double[] cornerX = new double[cornerRows * cornerCols];
            double[] cornerY = new double[cornerRows * cornerCols];
            bool[] cornerValid = new bool[cornerRows * cornerCols];
            for (int r = 0; r < cornerRows; r++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int rowBase = r * cornerCols;
                int screenY = rowBoundaries[r];
                for (int c = 0; c < cornerCols; c++)
                {
                    if (TryScreenToWebMercator(engine, colBoundaries[c], screenY, out PointD wm))
                    {
                        cornerX[rowBase + c] = wm.X;
                        cornerY[rowBase + c] = wm.Y;
                        cornerValid[rowBase + c] = true;
                    }
                }
            }

            // ── Phase 2: per-pixel warp, parallel across cell rows ─────────────
            // Pure CPU math + array sampling: no GDAL, no shared mutable state, and
            // each cell row writes a disjoint band of target rows. The sampled
            // result is identical to the single-threaded path (same mesh corners,
            // same bilinear interpolation, same nearest-neighbour sampling), so the
            // distortion is preserved exactly — only the throughput changes.
            ParallelOptions options = new()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount)
            };

            Parallel.For(0, cellRows, options, cellRowIndex =>
            {
                int cellTop = rowBoundaries[cellRowIndex];
                int cellBottom = rowBoundaries[cellRowIndex + 1];
                int cornerRowBase = cellRowIndex * cornerCols;
                int cornerRowBaseNext = (cellRowIndex + 1) * cornerCols;

                for (int cellColIndex = 0; cellColIndex < cellCols; cellColIndex++)
                {
                    options.CancellationToken.ThrowIfCancellationRequested();

                    int cellLeft = colBoundaries[cellColIndex];
                    int cellRight = colBoundaries[cellColIndex + 1];

                    int i00 = cornerRowBase + cellColIndex;
                    int i10 = i00 + 1;
                    int i01 = cornerRowBaseNext + cellColIndex;
                    int i11 = i01 + 1;
                    if (!cornerValid[i00] || !cornerValid[i10] ||
                        !cornerValid[i01] || !cornerValid[i11])
                    {
                        continue;
                    }

                    double wm00X = cornerX[i00], wm00Y = cornerY[i00];
                    double wm10X = cornerX[i10], wm10Y = cornerY[i10];
                    double wm01X = cornerX[i01], wm01Y = cornerY[i01];
                    double wm11X = cornerX[i11], wm11Y = cornerY[i11];

                    double invWidth = 1.0 / Math.Max(1, cellRight - cellLeft);
                    double invHeight = 1.0 / Math.Max(1, cellBottom - cellTop);

                    for (int y = cellTop; y < cellBottom; y++)
                    {
                        double v = (y - cellTop + 0.5) * invHeight;
                        double leftX = Lerp(wm00X, wm01X, v);
                        double leftY = Lerp(wm00Y, wm01Y, v);
                        double rightX = Lerp(wm10X, wm11X, v);
                        double rightY = Lerp(wm10Y, wm11Y, v);
                        int targetRow = y * targetStride;

                        for (int x = cellLeft; x < cellRight; x++)
                        {
                            double u = (x - cellLeft + 0.5) * invWidth;
                            double wmX = Lerp(leftX, rightX, u);
                            double wmY = Lerp(leftY, rightY, u);
                            if (wmX < sourceMinX || wmX > sourceMaxX ||
                                wmY < sourceMinY || wmY > sourceMaxY)
                            {
                                continue;
                            }

                            int sourceX = (int)Math.Round((wmX - sourceMinX) * sourceScaleX);
                            int sourceY = (int)Math.Round((sourceMaxY - wmY) * sourceScaleY);
                            if ((uint)sourceX >= (uint)sourceWidth ||
                                (uint)sourceY >= (uint)sourceHeight)
                            {
                                continue;
                            }

                            targetPixels[targetRow + x] =
                                sourcePixels[sourceY * sourceStride + sourceX];
                        }
                    }
                }
            });
        }

        private bool TryScreenToWebMercator(
            MapCanvasEngine engine,
            int screenX,
            int screenY,
            out PointD webMercator)
        {
            PointD projectPoint = engine.ScreenToWorld(
                new PointD(screenX, screenY));
            return TryTransformPoint(
                _projectToWebMercator,
                projectPoint,
                out webMercator);
        }

        private static double Lerp(double a, double b, double t) =>
            a + (b - a) * t;

        private void DrawMosaicBitmapRegion(
            RasterImageRenderContext imageContext,
            Bitmap bitmap,
            Rectangle destination,
            RectangleF source,
            bool smooth)
        {
            imageContext.DrawBitmap(
                bitmap,
                destination,
                source,
                GetOpacityFactor(),
                smooth ? ImageInterpolation.HighQuality : ImageInterpolation.NearestNeighbor,
                tileFlipXY: true);
        }

        private bool DrawVisibleTiles(
            RasterImageRenderContext imageContext,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            TileRange tileRange,
            CancellationToken cancellationToken,
            HashSet<TileKey>? fallbackFetchCandidates = null)
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
                        if (_allowParentPlaceholders &&
                            TryDrawParentTileFallback(
                                imageContext,
                                engine,
                                key,
                                projectBounds))
                        {
                            drawnAny = true;
                        }

                        CollectParentFallbackFetchCandidates(
                            key,
                            fallbackFetchCandidates);
                        continue;
                    }

                    TouchTile(key);

                    drawnAny |= DrawTileBitmap(
                        imageContext,
                        engine,
                        bitmap,
                        key,
                        projectBounds);
                }
            }

            return drawnAny;
        }

        private bool DrawTileBitmap(
            RasterImageRenderContext imageContext,
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
                    imageContext,
                    bitmap,
                    AlignDestinationToPixelGrid(destination),
                    new RectangleF(0, 0, bitmap.Width, bitmap.Height));
                return true;
            }

            return DrawTileBitmapMesh(
                imageContext,
                engine,
                bitmap,
                key,
                new RectangleF(0, 0, bitmap.Width, bitmap.Height));
        }

        private bool DrawTileBitmapMesh(
            RasterImageRenderContext imageContext,
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
                        imageContext,
                        bitmap,
                        destination,
                        source);
                    drawnAny = true;
                }
            }

            return drawnAny;
        }

        /// <summary>
        /// Walks down the zoom pyramid looking for a cached ancestor tile and draws its
        /// sub-region scaled to fill <paramref name="missingProjectBounds"/>.
        /// This gives the user a blurry-but-visible placeholder while the correct tile
        /// loads, exactly as Google Maps and QGIS do.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if a fallback tile was drawn; otherwise
        /// <see langword="false"/>.
        /// </returns>
        private bool TryDrawParentTileFallback(
            RasterImageRenderContext imageContext,
            MapCanvasEngine engine,
            TileKey missingKey,
            RectangleD missingProjectBounds)
        {
            for (int dz = 1; dz <= missingKey.Z; dz++)
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

                if (!TryGetCachedTileBitmap(parentKey, out Bitmap? parentBitmap) ||
                    parentBitmap == null)
                {
                    continue;
                }

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
                    imageContext,
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
            out Bitmap? bitmap,
            out RectangleF sourceRect)
        {
            bitmap = null;
            sourceRect = default;

            for (int dz = 1; dz <= missingKey.Z; dz++)
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
                if (!TryGetCachedTileBitmap(parentKey, out Bitmap? parentBitmap) ||
                    parentBitmap == null)
                {
                    continue;
                }

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

        private void CollectParentFallbackFetchCandidates(
            TileKey missingKey,
            HashSet<TileKey>? fallbackFetchCandidates)
        {
            if (fallbackFetchCandidates == null)
            {
                return;
            }

            for (int dz = 1; dz <= missingKey.Z; dz++)
            {
                int factor = 1 << dz;
                TileKey parentKey = new(
                    missingKey.Z - dz,
                    missingKey.X / factor,
                    missingKey.Y / factor);

                if (TryGetCachedTileBitmap(parentKey, out Bitmap? parentBitmap) &&
                    parentBitmap != null)
                {
                    return;
                }

                if (_noDataTiles.Contains(parentKey))
                {
                    continue;
                }

                if (_pendingFetches.ContainsKey(parentKey))
                {
                    continue;
                }

                fallbackFetchCandidates.Add(parentKey);
            }
        }

        private bool DrawParentBitmapRegion(
            RasterImageRenderContext imageContext,
            MapCanvasEngine engine,
            Bitmap parentBitmap,
            TileKey missingKey,
            RectangleD missingProjectBounds,
            RectangleF source)
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
                    imageContext,
                    parentBitmap,
                    AlignDestinationToPixelGrid(destination),
                    source);
                return true;
            }

            return DrawTileBitmapMesh(
                imageContext,
                engine,
                parentBitmap,
                missingKey,
                source);
        }

        /// <summary>Called on a thread-pool thread 50 ms after the last render pass.</summary>
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

                if (_internetFetchSuspended)
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

            _ = FetchVisibleThenPrefetchAsync(webMercatorBounds, zoom, newCts.Token);
        }

        /// <summary>
        /// Fetches the visible tiles first, then proactively warms the cache with a
        /// one-ring border at the current zoom and a slice of the next zoom level so
        /// panning and zooming into adjacent areas shows fresh tiles immediately.
        /// Prefetch work is bounded and shares the viewport cancellation token, so a
        /// new pan/zoom cancels it before it can compete with the next visible fetch.
        /// </summary>
        private async Task FetchVisibleThenPrefetchAsync(
            RectangleD webMercatorBounds,
            int zoom,
            CancellationToken cancellationToken)
        {
            // Visible tiles always come first and uncapped.
            await FetchMissingTilesAsync(webMercatorBounds, zoom, cancellationToken)
                .ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Ring prefetch at the current zoom. Center tiles are already cached and
            // skipped by the missing-tile check, so only the surrounding border is
            // pulled.
            RectangleD ringBounds = ExpandExtent(webMercatorBounds, PrefetchRingFactor);
            await FetchMissingTilesAsync(
                    ringBounds,
                    zoom,
                    cancellationToken,
                    MaxPrefetchTilesPerFrame)
                .ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Next zoom level prefetch so zooming in is instant.
            if (zoom < _maxSourceZoom)
            {
                await FetchMissingTilesAsync(
                        webMercatorBounds,
                        zoom + 1,
                        cancellationToken,
                        MaxPrefetchTilesPerFrame)
                    .ConfigureAwait(false);
            }
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
            CancellationToken cancellationToken;
            lock (_renderSync)
            {
                if (_disposed || _internetFetchSuspended)
                {
                    return;
                }

                cancellationToken = _viewportCts.Token;
            }

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
                cancellationToken,
                MaxBootstrapTilesPerFrame);
        }

        private void QueueFallbackFetches(HashSet<TileKey>? fallbackFetchCandidates)
        {
            if (fallbackFetchCandidates == null || fallbackFetchCandidates.Count == 0)
            {
                return;
            }

            List<TileKey> missing = new();
            int pendingGeneration;

            lock (_renderSync)
            {
                if (_disposed || _internetFetchSuspended)
                {
                    return;
                }

                pendingGeneration = _pendingFetchGeneration;

                foreach (TileKey key in fallbackFetchCandidates
                             .OrderBy(candidate => candidate.Z)
                             .ThenBy(candidate => candidate.Y)
                             .ThenBy(candidate => candidate.X))
                {
                    if (missing.Count >= MaxFallbackTilesPerFrame)
                    {
                        break;
                    }

                    if (!TryGetCachedTileBitmap(key, out _) &&
                        !_noDataTiles.Contains(key) &&
                        !_pendingFetches.ContainsKey(key))
                    {
                        _pendingFetches[key] = pendingGeneration;
                        missing.Add(key);
                    }
                }
            }

            if (missing.Count == 0)
            {
                return;
            }

            _ = FetchRegisteredTilesAsync(
                missing,
                pendingGeneration,
                CancellationToken.None);
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

            List<TileKey> missing = new List<TileKey>();
            int pendingGeneration;

            lock (_renderSync)
            {
                pendingGeneration = _pendingFetchGeneration;
                for (int y = tileRange.MinY; y <= tileRange.MaxY; y++)
                {
                    for (int x = tileRange.MinX; x <= tileRange.MaxX; x++)
                    {
                        if (missing.Count >= maxTilesToFetch)
                        {
                            break;
                        }

                        TileKey key = new TileKey(zoom, x, y);
                        if (!TryGetCachedTileBitmap(key, out _) &&
                            !_noDataTiles.Contains(key) &&
                            !_pendingFetches.ContainsKey(key))
                        {
                            _pendingFetches[key] = pendingGeneration;
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
                if (cancellationToken.IsCancellationRequested)
                {
                    RemovePendingFetches(missing, pendingGeneration);
                }

                return;
            }

            bool internetAvailable;
            try
            {
                internetAvailable = await HasInternetConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                RemovePendingFetches(missing, pendingGeneration);
                return;
            }

            if (!internetAvailable)
            {
                RemovePendingFetches(missing, pendingGeneration);
                ReportFetchDisconnected();
                return;
            }

            IEnumerable<Task> tasks =
                missing.Select(key =>
                    FetchAndCacheTileAsync(
                        key,
                        pendingGeneration,
                        cancellationToken));

            ReportFetchStatus(tileCountDelta: missing.Count);
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
            finally
            {
                ReportFetchStatus(tileCountDelta: -missing.Count);
            }
        }

        private async Task FetchRegisteredTilesAsync(
            IReadOnlyCollection<TileKey> missing,
            int pendingGeneration,
            CancellationToken cancellationToken)
        {
            bool internetAvailable;
            try
            {
                internetAvailable = await HasInternetConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                RemovePendingFetches(missing, pendingGeneration);
                return;
            }

            if (!internetAvailable)
            {
                RemovePendingFetches(missing, pendingGeneration);
                ReportFetchDisconnected();
                return;
            }

            IEnumerable<Task> tasks =
                missing.Select(key =>
                    FetchAndCacheTileAsync(
                        key,
                        pendingGeneration,
                        cancellationToken));

            ReportFetchStatus(tileCountDelta: missing.Count);
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected on pan/zoom - a new viewport CTS will be issued.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[XyzLiveTileRenderLayer] Tile fallback batch error: {ex.Message}");
            }
            finally
            {
                ReportFetchStatus(tileCountDelta: -missing.Count);
            }
        }

        private void RemovePendingFetches(
            IEnumerable<TileKey> keys,
            int pendingGeneration)
        {
            lock (_renderSync)
            {
                foreach (TileKey key in keys)
                {
                    if (_pendingFetches.TryGetValue(
                            key,
                            out int registeredGeneration) &&
                        registeredGeneration == pendingGeneration)
                    {
                        _pendingFetches.Remove(key);
                    }
                }
            }
        }

        private static async Task<bool> HasInternetConnectionAsync(CancellationToken cancellationToken)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                UpdateInternetConnectivityCache(false);
                return false;
            }

            DateTime nowUtc = DateTime.UtcNow;
            lock (InternetConnectivitySync)
            {
                if ((nowUtc - _lastInternetConnectivityCheckUtc).TotalMilliseconds <
                    InternetConnectivityCacheMilliseconds)
                {
                    return _lastInternetConnectivityAvailable;
                }
            }

            foreach (Uri probeUri in InternetProbeUris)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using CancellationTokenSource timeoutCts =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(InternetProbeTimeoutMilliseconds);

                try
                {
                    using HttpRequestMessage request = new(HttpMethod.Get, probeUri);
                    request.Headers.CacheControl = new()
                    {
                        NoCache = true
                    };

                    using HttpResponseMessage response =
                        await SharedHttpClient
                            .SendAsync(
                                request,
                                HttpCompletionOption.ResponseHeadersRead,
                                timeoutCts.Token)
                            .ConfigureAwait(false);

                    bool available = (int)response.StatusCode < 500;
                    if (available)
                    {
                        UpdateInternetConnectivityCache(true);
                        return true;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch
                {
                    // Try the next connectivity endpoint before deciding the machine is offline.
                }
            }

            UpdateInternetConnectivityCache(false);
            return false;
        }

        private static void UpdateInternetConnectivityCache(bool available)
        {
            lock (InternetConnectivitySync)
            {
                _lastInternetConnectivityAvailable = available;
                _lastInternetConnectivityCheckUtc = DateTime.UtcNow;
            }
        }

        private async Task FetchAndCacheTileAsync(
            TileKey key,
            int pendingGeneration,
            CancellationToken cancellationToken)
        {
            Bitmap? bitmap = null;
            try
            {
                // Disk-cached tiles must not consume a network concurrency slot:
                // read the disk cache first (async, off the semaphore) and only take
                // the fetch semaphore for an actual network download.
                byte[]? bytes = await TryReadDiskCacheAsync(key, cancellationToken)
                    .ConfigureAwait(false);
                bool bytesFromDiskCache = bytes != null && bytes.Length > 0;
                bool bytesDownloaded = false;

                if (!bytesFromDiskCache)
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

                        string url = BuildTileUrl(key);
                        bytes = await SharedHttpClient
                            .GetByteArrayAsync(url, cancellationToken)
                            .ConfigureAwait(false);
                        bytesDownloaded = bytes != null && bytes.Length > 0;
                    }
                    finally
                    {
                        FetchSemaphore.Release();
                    }
                }

                if (bytes == null || bytes.Length == 0)
                {
                    return;
                }

                // Decode on thread pool — keeps byte[] alive for only the decode
                // duration. Decoding now happens off the network semaphore so a slot
                // is freed for the next download as soon as bytes arrive.
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
                    if (_pendingFetches.TryGetValue(
                            key,
                            out int registeredGeneration) &&
                        registeredGeneration == pendingGeneration)
                    {
                        _pendingFetches.Remove(key);
                    }
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

            desiredZoom = Math.Clamp(desiredZoom, 0, _maxSourceZoom);

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
            ;

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
            RasterImageRenderContext imageContext,
            Bitmap bitmap,
            PointF[] destination,
            RectangleF source)
        {
            RectangleF expandedSource = ExpandSourceForSeams(
                source,
                bitmap.Width,
                bitmap.Height);

            imageContext.DrawBitmap(
                bitmap,
                destination,
                expandedSource,
                GetOpacityFactor(),
                ImageInterpolation.NearestNeighbor,
                tileFlipXY: true);
        }

        private void DrawBitmapRegion(
            RasterImageRenderContext imageContext,
            Bitmap bitmap,
            RectangleF destination,
            RectangleF source,
            bool isPlaceholder = false)
        {
            // Always use integer Rectangle destination — the 3-point parallelogram overload
            // causes 1-pixel seam bleed between adjacent tiles in GDI+.
            Rectangle dest = CreateIntegerDestinationRectangle(destination);

            imageContext.DrawBitmap(
                bitmap,
                dest,
                source,
                GetOpacityFactor(),
                ImageInterpolation.NearestNeighbor,
                tileFlipXY: true);
        }

        private float GetOpacityFactor() =>
            (float)Math.Clamp((100 - Transparency) / 100d, 0d, 1d);

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

                // Render once straight into a 32bpp surface. The old path built an
                // intermediate Bitmap(image) and then Clone()- d it, decoding the
                // pixels into two extra full-tile bitmaps on every tile.
                Bitmap result = new(
                    image.Width,
                    image.Height,
                    PixelFormat.Format32bppPArgb);
                try
                {
                    using Graphics graphics = Graphics.FromImage(result);
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.None;
                    graphics.DrawImage(
                        image,
                        new Rectangle(0, 0, image.Width, image.Height));
                }
                catch
                {
                    result.Dispose();
                    throw;
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsLikelyNoDataTile(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int step = Math.Max(1, Math.Min(width, height) / 32);
            int total = 0;
            int neutral = 0;
            int brightNeutral = 0;
            double sum = 0.0;
            double sumSq = 0.0;

            // Sample over a single LockBits snapshot instead of calling GetPixel
            // per sample. GetPixel locks/unlocks the bitmap on every call, which
            // ran on every decoded tile; one Marshal.Copy is 20-50x faster here.
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            int stride = Math.Abs(data.Stride) / 4;
            int[] pixels = new int[stride * height];
            try
            {
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            for (int y = 0; y < height; y += step)
            {
                int rowBase = y * stride;
                for (int x = 0; x < width; x += step)
                {
                    int px = pixels[rowBase + x];
                    int r = (px >> 16) & 0xFF;
                    int g = (px >> 8) & 0xFF;
                    int b = px & 0xFF;
                    int max = Math.Max(r, Math.Max(g, b));
                    int min = Math.Min(r, Math.Min(g, b));
                    int brightness = (r + g + b) / 3;

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

        private bool TryGetCachedTileBitmap(TileKey key, out Bitmap? bitmap)
        {
            if (_tileCache.TryGetValue(key, out bitmap))
            {
                TouchTile(key);
                return true;
            }

            if (_noDataTiles.Contains(key))
            {
                bitmap = null;
                return false;
            }

            byte[]? bytes = TryReadDiskCache(key);
            if (bytes == null || bytes.Length == 0)
            {
                bitmap = null;
                return false;
            }

            Bitmap? decoded = DecodeTileBitmap(bytes);
            if (decoded == null)
            {
                bitmap = null;
                return false;
            }

            if (IsLikelyNoDataTile(decoded))
            {
                decoded.Dispose();
                TryDeleteDiskCache(key);
                _noDataTiles.Add(key);
                bitmap = null;
                return false;
            }

            _tileCache[key] = decoded;
            _noDataTiles.Remove(key);
            LinkedListNode<TileKey> node = _tileLru.AddLast(key);
            _tileLruNodes[key] = node;
            TrimTileCache();
            bitmap = decoded;
            return true;
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
            _pendingFetchGeneration++;
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

        private async Task<byte[]?> TryReadDiskCacheAsync(
            TileKey key,
            CancellationToken cancellationToken)
        {
            try
            {
                string path = GetDiskCachePath(key);
                if (!File.Exists(path))
                {
                    return null;
                }

                return await File
                    .ReadAllBytesAsync(path, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
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
            BuildTileUrl(_urlTemplate, key.Z, key.X, key.Y);

        private static string BuildTileUrl(
            string urlTemplate,
            int zoom,
            int tileX,
            int tileY)
        {
            string url = urlTemplate;
            if (ContainsTileToken(url, "quadkey"))
            {
                string quadkey = QuadkeyConverter.TileXYToQuadkey(
                    tileX,
                    tileY,
                    zoom);
                url = ReplaceTileToken(url, "quadkey", quadkey);
            }

            return ReplaceTileToken(
                ReplaceTileToken(
                    ReplaceTileToken(
                        url,
                        "z",
                        zoom.ToString(CultureInfo.InvariantCulture)),
                    "x",
                    tileX.ToString(CultureInfo.InvariantCulture)),
                "y",
                tileY.ToString(CultureInfo.InvariantCulture));
        }

        private static bool ContainsTileToken(string urlTemplate, string token)
        {
            return urlTemplate.Contains($"${{{token}}}", StringComparison.OrdinalIgnoreCase) ||
                   urlTemplate.Contains($"{{{token}}}", StringComparison.OrdinalIgnoreCase);
        }

        private static string ReplaceTileToken(
            string value,
            string token,
            string replacement)
        {
            return value
                .Replace($"${{{token}}}", replacement, StringComparison.OrdinalIgnoreCase)
                .Replace($"{{{token}}}", replacement, StringComparison.OrdinalIgnoreCase);
        }

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
            return string.IsNullOrWhiteSpace(text) ? null : WebUtility.HtmlDecode(text);
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

        private static int ResolveMaxSourceZoom(
            string wmsXmlPath,
            string urlTemplate)
        {
            int maxZoom = ExtractTileLevel(wmsXmlPath) ?? MaxSupportedZoom;
            string url = urlTemplate.ToLowerInvariant();

            // ArcGIS imagery services often return placeholder/partial tiles above
            // their real cache coverage. Capping prevents mixed high-zoom misses and
            // parent placeholders from appearing as warped/distorted blocks.
            if (url.Contains("services.arcgisonline.com/arcgis/rest/services/world_imagery/"))
            {
                maxZoom = Math.Min(maxZoom, 19);
            }
            else if (url.Contains("services.arcgisonline.com/arcgis/rest/services/world_physical_map/"))
            {
                maxZoom = Math.Min(maxZoom, 16);
            }
            else if (url.Contains("services.arcgisonline.com/arcgis/rest/services/") ||
                     url.Contains("basemap.nationalmap.gov/arcgis/rest/services/"))
            {
                maxZoom = Math.Min(maxZoom, 19);
            }

            return Math.Clamp(maxZoom, 0, MaxSupportedZoom);
        }

        private static int? ExtractTileLevel(string wmsXmlPath)
        {
            try
            {
                XmlDocument doc = new()
                {
                    XmlResolver = null
                };
                doc.Load(wmsXmlPath);
                XmlNode? node = doc.SelectSingleNode("//*[local-name()='TileLevel']");
                return int.TryParse(
                    node?.InnerText?.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int tileLevel)
                    ? tileLevel
                    : null;
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
                XmlDocument document = new()
                {
                    XmlResolver = null
                };
                document.Load(vrtPath);

                XmlNode? rootSrsNode = document.DocumentElement?
                    .ChildNodes
                    .Cast<XmlNode>()
                    .FirstOrDefault(node =>
                        node.NodeType == XmlNodeType.Element &&
                        string.Equals(
                            node.Name,
                            "SRS",
                            StringComparison.OrdinalIgnoreCase));

                string definition = rootSrsNode?.InnerText.Trim() ?? string.Empty;
                return string.IsNullOrWhiteSpace(definition)
                    ? null
                    : CreateSpatialReference(definition);
            }
            catch
            {
                return null;
            }
        }

        private static SpatialReference? ExtractProjectSrsFromGdal(string vrtPath)
        {
            try
            {
                GdalConfiguration.ConfigureGdal();
                if (!GdalConfiguration.Usable)
                {
                    return null;
                }

                using Dataset dataset = Gdal.Open(vrtPath, Access.GA_ReadOnly);
                string definition = dataset?.GetProjectionRef() ?? string.Empty;
                return string.IsNullOrWhiteSpace(definition)
                    ? null
                    : CreateSpatialReference(definition);
            }
            catch
            {
                return null;
            }
        }

        private static string BuildDiskCacheRoot(string urlTemplate)
        {
            string cacheIdentity = BuildDiskCacheIdentity(urlTemplate);
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(cacheIdentity));
            string prefix = Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
            return Path.Combine(Path.GetTempPath(), "replot-live-tiles", prefix);
        }

        private static string BuildDiskCacheIdentity(string urlTemplate)
        {
            string normalized = urlTemplate.Trim();
            string zoomPolicy = IsBingTileSource(normalized)
                ? $"bing-overzoom-max-z{BingLiveMaxFetchZoom}"
                : "exact-web-mercator-live-cache-v1";

            return $"{normalized}|{zoomPolicy}";
        }

        private static bool ShouldAllowParentPlaceholders(string urlTemplate)
        {
            // Parent tiles are only accepted when they cover the whole missing
            // visible mosaic; partial parent/exact mixes are rejected by the draw path.
            return true;
        }

        private static int GetEffectiveMaxSourceZoom(
            string urlTemplate,
            int requestedMaxZoom)
        {
            int clampedMaxZoom = Math.Clamp(requestedMaxZoom, 0, MaxSupportedZoom);
            return IsBingTileSource(urlTemplate)
                ? Math.Min(clampedMaxZoom, BingLiveMaxFetchZoom)
                : clampedMaxZoom;
        }

        private static bool IsBingTileSource(string urlTemplate)
        {
            return urlTemplate.Contains(
                       "virtualearth.net",
                       StringComparison.OrdinalIgnoreCase) ||
                   ContainsTileToken(urlTemplate, "quadkey");
        }

        private static void ReportFetchStatus(int tileCountDelta)
        {
            LiveTileFetchStatusChangedEventArgs? status = null;
            lock (FetchStatusSync)
            {
                int previousActiveFetchTileCount = _activeFetchTileCount;
                _activeFetchTileCount = Math.Max(
                    0,
                    _activeFetchTileCount + tileCountDelta);

                if (previousActiveFetchTileCount == 0 &&
                    _activeFetchTileCount > 0)
                {
                    FetchCompleteQuietTimer.Change(
                        Timeout.Infinite,
                        Timeout.Infinite);
                    if (!_fetchStatusReportedFetching)
                    {
                        _fetchStatusReportedFetching = true;
                        status = new LiveTileFetchStatusChangedEventArgs(
                            isFetching: true,
                            _activeFetchTileCount);
                    }
                }
                else if (_activeFetchTileCount == 0 &&
                         _fetchStatusReportedFetching)
                {
                    FetchCompleteQuietTimer.Change(
                        FetchCompleteQuietMilliseconds,
                        Timeout.Infinite);
                }
            }

            if (status != null)
            {
                FetchStatusChanged?.Invoke(null, status);
            }
        }

        private static void ReportFetchDisconnected()
        {
            LiveTileFetchStatusChangedEventArgs status;
            lock (FetchStatusSync)
            {
                FetchCompleteQuietTimer.Change(
                    Timeout.Infinite,
                    Timeout.Infinite);

                if (_activeFetchTileCount == 0)
                {
                    _fetchStatusReportedFetching = false;
                }

                status = new LiveTileFetchStatusChangedEventArgs(
                    LiveTileFetchStatusKind.Disconnected,
                    pendingTileCount: 0);
            }

            FetchStatusChanged?.Invoke(null, status);
        }

        private static void OnFetchCompleteQuietElapsed(object? state)
        {
            LiveTileFetchStatusChangedEventArgs? status = null;
            lock (FetchStatusSync)
            {
                if (_activeFetchTileCount == 0 &&
                    _fetchStatusReportedFetching)
                {
                    _fetchStatusReportedFetching = false;
                    status = new LiveTileFetchStatusChangedEventArgs(
                        isFetching: false,
                        pendingTileCount: 0);
                }
            }

            if (status != null)
            {
                FetchStatusChanged?.Invoke(null, status);
            }
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
            ApplyBallparkDatumTransformIfNeeded(srs);
            return srs;
        }

        private static void ApplyBallparkDatumTransformIfNeeded(
            SpatialReference srs)
        {
            try
            {
                srs.AutoIdentifyEPSG();

                string? authorityCode =
                    srs.GetAuthorityCode(null) ??
                    srs.GetAuthorityCode("PROJCS") ??
                    srs.GetAuthorityCode("GEOGCS");
                if (!string.IsNullOrWhiteSpace(authorityCode))
                {
                    return;
                }

                string? geographicName = srs.GetAttrValue("GEOGCS", 0);
                string? datumName = srs.GetAttrValue("DATUM", 0);
                if (ContainsIgnoreCase(geographicName, "WGS") ||
                    ContainsIgnoreCase(datumName, "WGS"))
                {
                    return;
                }

                srs.ExportToWkt(out string wkt, []);
                if (wkt.Contains("TOWGS84", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Custom MUTM/Everest-style CRSs without a datum transform may
                // parse correctly but still fail when OSR transforms them to
                // EPSG:3857. Attach a zero Bursa-Wolf fallback so live imagery
                // remains renderable; a configured datum transform still wins.
                srs.SetTOWGS84(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            }
            catch
            {
                // Optional rendering fallback only.
            }
        }

        private static bool ContainsIgnoreCase(string? value, string text) =>
            value?.Contains(text, StringComparison.OrdinalIgnoreCase) == true;

        private static SpatialReference? TryCreateSpatialReference(
            string? definition)
        {
            if (string.IsNullOrWhiteSpace(definition))
            {
                return null;
            }

            try
            {
                return CreateSpatialReference(definition);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[XyzLiveTileRenderLayer] Project CRS override could not be parsed. {ex.Message}");
                return null;
            }
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
                Timeout = TimeSpan.FromSeconds(20),
                // Prefer HTTP/2 so many tile requests multiplex over a single
                // connection; fall back to HTTP/1.1 for servers that lack it.
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = System.Net.Http.HttpVersionPolicy.RequestVersionOrLower
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
