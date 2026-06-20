using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Services.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;


namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Renders map-canvas visual layers such as background, adaptive grid,
    /// axis/origin marker, and interactive zoom window overlays.
    /// </summary>
    public sealed class MapCanvasRenderer : IDisposable
    {
        private const int LevelOfDetailDirectRenderThreshold = 20_000;
        private const float GridFontSizePx = 10.67f;
        private const float AxisFontSizePx = 12.0f;
        private static readonly MapRenderSurfaceOptions OverlaySurfaceOptions = new()
        {
            ApplyInitialQuality = false
        };
        private static readonly MapRenderSurfaceOptions RasterFrameSurfaceOptions = new()
        {
            InitialQuality = RenderQuality.RasterHighSpeed
        };

        private readonly record struct MapCanvasRenderViewport(
            RectangleD VisibleWorldBounds,
            RectangleF VisibleScreenBounds);

        private readonly MapCanvasEngine _engine;
        private readonly MapCanvasRenderOrderService _renderOrderService;
        private readonly IMapRenderSurfaceFactory _renderSurfaceFactory;
        private readonly CanvasVectorRenderer _vectorRenderer;
        private readonly VectorDeferredRenderer _vectorDeferredRenderer = new();
        private double _lastAdaptiveMinorSize;
        private MapCanvasRenderSettings _settings;
        private IReadOnlyList<IRasterRenderLayer> _rasterLayers = [];
        private bool _debugOverlayRequested;
        private Bitmap? _skiaBackingBitmap;

        /// <summary>
        /// Creates a renderer for drawing the map canvas using the supplied engine and settings.
        /// </summary>
        /// <param name="engine">Viewport engine providing world/screen transforms.</param>
        /// <param name="settings">Rendering options and theme colors.</param>
        /// <param name="renderOrderService">Optional service that defines canvas render-pass order.</param>
        /// <param name="renderSurfaceFactory">
        /// Optional backend-neutral surface factory used by migrated render paths.
        /// </param>
        public MapCanvasRenderer(
            MapCanvasEngine engine,
            MapCanvasRenderSettings settings,
            MapCanvasRenderOrderService? renderOrderService = null,
            IMapRenderSurfaceFactory? renderSurfaceFactory = null)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _settings = settings?.Clone()
                ?? throw new ArgumentNullException(nameof(settings));
            _renderOrderService = renderOrderService
                ?? new MapCanvasRenderOrderService();
            _renderSurfaceFactory = renderSurfaceFactory
                ?? MapRenderSurfaceFactory.Default;
            _vectorRenderer = new CanvasVectorRenderer(_renderSurfaceFactory);
            _vectorRenderer.UpdateRenderBackend(_settings.RenderBackend);
        }

        /// <summary>
        /// Replaces the renderer settings while keeping the renderer-owned copy isolated.
        /// </summary>
        /// <param name="settings">Updated rendering options and theme colors.</param>
        public void UpdateSettings(MapCanvasRenderSettings settings)
        {
            _settings = settings?.Clone()
                ?? throw new ArgumentNullException(nameof(settings));
            _vectorRenderer.UpdateRenderBackend(_settings.RenderBackend);
        }

        /// <summary>
        /// Replaces the raster render layers used by future render passes.
        /// </summary>
        /// <param name="rasterLayers">Raster layers in drawing order.</param>
        public void UpdateRasterLayers(
            IReadOnlyList<IRasterRenderLayer> rasterLayers)
        {
            _rasterLayers = rasterLayers?.ToArray()
                ?? throw new ArgumentNullException(nameof(rasterLayers));
        }

        public void UpdateVectorLayers(IEnumerable<CanvasLayer>? layers)
        {
            _vectorRenderer.UpdateLayers(layers);
            _vectorDeferredRenderer.Invalidate();
        }

        public void UpdateVectorLayer(CanvasLayer layer)
        {
            _vectorRenderer.UpdateLayer(layer);
            _vectorDeferredRenderer.Invalidate();
        }

        public void UpdateVectorFeatures(IEnumerable<CanvasFeature>? features, bool invalidateCache = true)
        {
            _vectorRenderer.UpdateFeatures(features);
            if (invalidateCache)
            {
                _vectorDeferredRenderer.Invalidate();
            }
        }

        public void SetVectorRenderExclusions(IEnumerable<Guid>? shapeIds, bool invalidateCache = true)
        {
            _vectorRenderer.SetExcludedShapeIds(shapeIds);
            if (invalidateCache)
            {
                _vectorDeferredRenderer.Invalidate();
            }
        }

        public void UpdateAreaPrecisionSettings(int sqmPrecision, int traditionalPrecision)
        {
            _vectorRenderer.UpdateAreaPrecisionSettings(sqmPrecision, traditionalPrecision);
            _vectorDeferredRenderer.Invalidate();
        }

        public RectangleD? GetVectorFeatureBounds() =>
            _vectorRenderer.GetFeatureBounds();

        /// <summary>
        /// Spatially queries vector features intersecting the given world bounds
        /// (fast index lookup) for per-mouse-move work like snapping.
        /// </summary>
        public IReadOnlyList<CanvasFeature> QueryVectorFeatures(RectangleD worldBounds) =>
            _vectorRenderer.QueryFeatures(worldBounds);

        /// <summary>
        /// Renders the full canvas frame for the current viewport state.
        /// </summary>
        /// <param name="graphics">Target graphics surface for drawing.</param>
        /// <param name="rasterFrame">
        /// Optional cached raster frame used during interactive navigation.
        /// </param>
        /// <param name="interactiveRaster">
        /// Whether raster rendering is happening during pan/zoom interaction.
        /// </param>
        /// <param name="zoomWindowRectangle">
        /// Optional selection rectangle used by zoom-window interaction.
        /// </param>
        public void Render(
            Graphics graphics,
            RasterRenderFrame? rasterFrame,
            bool interactiveRaster,
            RasterRenderFrame? vectorFrame,
            bool interactiveVector,
            Rectangle? zoomWindowRectangle,
            IShape? previewShape = null,
            CanvasLayer? previewLayer = null,
            bool showDebugOverlay = false,
            bool suppressDecorations = false,
            bool suppressGridLabels = false,
            bool suppressFixedReferenceLayers = false,
            RasterRenderFrame? fixedReferenceFrame = null,
            bool suppressBackgroundClear = false)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            _debugOverlayRequested = showDebugOverlay;
            using IDisposable? gpuFrameScope = BeginGpuFrameScopeIfNeeded(graphics);

            if (!suppressBackgroundClear)
            {
                using IMapRenderSurface surface = CreateFrameSurface(graphics);
                surface.Clear(_settings.BackgroundColor);
            }

            MapCanvasRenderViewport viewport = CreateViewport(graphics);
            foreach (MapCanvasRenderStage stage in _renderOrderService.GetFrameStages())
            {
                switch (stage)
                {
                    case MapCanvasRenderStage.FixedReference:
                        RenderFixedReferenceContent(
                            graphics,
                            viewport,
                            fixedReferenceFrame,
                            suppressGridLabels,
                            suppressFixedReferenceLayers);
                        break;

                    case MapCanvasRenderStage.RasterContent:
                        ConfigureRasterGraphics(graphics);
                        RenderRasterContent(
                            graphics,
                            viewport,
                            rasterFrame,
                            interactiveRaster);
                        break;

                    case MapCanvasRenderStage.VectorContent:
                        ConfigureVectorContentGraphics(graphics);
                        RenderVectorContent(
                            graphics,
                            viewport,
                            vectorFrame,
                            interactiveVector);
                        break;

                    case MapCanvasRenderStage.InteractionOverlay:
                        ConfigureVectorGraphics(graphics);
                        _vectorRenderer.RenderPreview(
                            graphics,
                            _engine,
                            previewShape,
                            previewLayer);
                        RenderInteractionOverlay(
                            graphics,
                            viewport,
                            zoomWindowRectangle,
                            suppressDecorations);
                        break;
                }
            }
        }

        public bool RenderCachedDirect(
            IMapRenderSurface surface,
            RasterRenderFrame? rasterFrame,
            bool interactiveRaster,
            RasterRenderFrame? vectorFrame,
            bool interactiveVector,
            bool suppressGridLabels,
            bool suppressFixedReferenceLayers,
            RasterRenderFrame? fixedReferenceFrame = null,
            bool suppressBackgroundClear = false,
            Rectangle? zoomWindowRectangle = null)
        {
            ArgumentNullException.ThrowIfNull(surface);

            MapCanvasRenderViewport viewport = CreateViewport(surface.PixelSize);
            if (!CanRenderCachedDirect(
                rasterFrame,
                interactiveRaster,
                vectorFrame,
                interactiveVector,
                fixedReferenceFrame,
                suppressFixedReferenceLayers))
            {
                return false;
            }

            if (!suppressBackgroundClear)
            {
                surface.Clear(_settings.BackgroundColor);
            }

            foreach (MapCanvasRenderStage stage in _renderOrderService.GetFrameStages())
            {
                switch (stage)
                {
                    case MapCanvasRenderStage.FixedReference:
                        surface.SetQuality(RenderQuality.VectorHighQuality);
                        RenderFixedReferenceContent(
                            surface,
                            viewport,
                            fixedReferenceFrame,
                            suppressGridLabels,
                            suppressFixedReferenceLayers);
                        fixedReferenceFrame = null;
                        break;

                    case MapCanvasRenderStage.RasterContent:
                        surface.SetQuality(RenderQuality.RasterHighSpeed);
                        RenderRasterContent(
                            surface,
                            viewport,
                            rasterFrame,
                            interactiveRaster);
                        rasterFrame = null;
                        break;

                    case MapCanvasRenderStage.VectorContent:
                        surface.SetQuality(
                            _settings.AntiAliasingEnabled
                                ? RenderQuality.VectorHighQuality
                                : RenderQuality.VectorHighSpeed);
                        RenderVectorContent(
                            surface,
                            viewport,
                            vectorFrame,
                            interactiveVector);
                        vectorFrame = null;
                        break;

                    case MapCanvasRenderStage.InteractionOverlay:
                        // Screen decorations (axis lines, origin marker, north
                        // marker) are intentionally NOT drawn here so they are
                        // excluded from the captured GPU viewport snapshot. The
                        // caller redraws them fresh each frame via
                        // RenderScreenDecorations, matching GDI behavior where
                        // these overlays are never baked into a frame cache.
                        surface.SetQuality(RenderQuality.VectorHighQuality);
                        RenderInteractionOverlay(
                            surface,
                            viewport,
                            zoomWindowRectangle,
                            suppressDecorations: true);
                        break;
                }
            }

            return true;
        }

        private bool CanRenderCachedDirect(
            RasterRenderFrame? rasterFrame,
            bool interactiveRaster,
            RasterRenderFrame? vectorFrame,
            bool interactiveVector,
            RasterRenderFrame? fixedReferenceFrame,
            bool suppressFixedReferenceLayers)
        {
            return true;
        }

        private IDisposable? BeginGpuFrameScopeIfNeeded(Graphics graphics)
        {
            if (_settings.RenderBackend != MapRenderBackend.SkiaGpu ||
                !_renderSurfaceFactory.IsBackendAvailable(MapRenderBackend.SkiaGpu))
            {
                return null;
            }

            Size pixelSize = Size.Round(graphics.VisibleClipBounds.Size);
            pixelSize = new Size(Math.Max(1, pixelSize.Width), Math.Max(1, pixelSize.Height));
            return new SkiaGpuFrameRenderScope(graphics, pixelSize);
        }

        public void ResizeVectorCache(Size canvasSize)
        {
            _vectorDeferredRenderer.Resize(canvasSize);
        }

        public void RenderTransientShape(
            Graphics graphics,
            IShape? shape,
            CanvasLayer? layer = null,
            CanvasObject? canvasObject = null,
            bool forceUnselected = false)
        {
            if (shape == null)
            {
                return;
            }

            ConfigureVectorGraphics(graphics);
            _vectorRenderer.RenderPreview(
                graphics,
                _engine,
                shape,
                layer,
                canvasObject,
                drawAsPreview: false,
                forceUnselected: forceUnselected);
        }

        public void RenderTransientShape(
            IMapRenderSurface surface,
            IShape? shape,
            CanvasLayer? layer = null,
            CanvasObject? canvasObject = null,
            bool forceUnselected = false)
        {
            if (shape == null)
            {
                return;
            }

            using Bitmap metricsBitmap = new(1, 1, PixelFormat.Format32bppPArgb);
            using Graphics fallbackGraphics = Graphics.FromImage(metricsBitmap);
            ConfigureVectorGraphics(fallbackGraphics);
            _vectorRenderer.RenderPreview(
                surface,
                fallbackGraphics,
                _engine,
                shape,
                layer,
                canvasObject,
                drawAsPreview: false,
                forceUnselected: forceUnselected);
        }

        public void RenderPreviewShape(
            IMapRenderSurface surface,
            IShape? shape,
            CanvasLayer? layer = null,
            CanvasObject? canvasObject = null,
            bool forceUnselected = false)
        {
            if (shape == null)
            {
                return;
            }

            using Bitmap metricsBitmap = new(1, 1, PixelFormat.Format32bppPArgb);
            using Graphics fallbackGraphics = Graphics.FromImage(metricsBitmap);
            ConfigureVectorGraphics(fallbackGraphics);
            _vectorRenderer.RenderPreview(
                surface,
                fallbackGraphics,
                _engine,
                shape,
                layer,
                canvasObject,
                drawAsPreview: true,
                forceUnselected: forceUnselected);
        }

        /// <summary>
        /// Live-renders a batch of transient shapes (e.g. all shapes being moved)
        /// with one shared render context.
        /// </summary>
        public void RenderTransientShapes(
            Graphics graphics,
            IReadOnlyList<(IShape Shape, CanvasLayer? Layer, CanvasObject? CanvasObject)> shapes,
            bool forceUnselected = false)
        {
            if (shapes == null || shapes.Count == 0)
            {
                return;
            }

            ConfigureVectorGraphics(graphics);
            _vectorRenderer.RenderTransientShapes(graphics, _engine, shapes, forceUnselected);
        }

        public void RenderTransientShapes(
            IMapRenderSurface surface,
            IReadOnlyList<(IShape Shape, CanvasLayer? Layer, CanvasObject? CanvasObject)> shapes,
            bool forceUnselected = false)
        {
            if (shapes == null || shapes.Count == 0)
            {
                return;
            }

            using Bitmap metricsBitmap = new(1, 1, PixelFormat.Format32bppPArgb);
            using Graphics fallbackGraphics = Graphics.FromImage(metricsBitmap);
            ConfigureVectorGraphics(fallbackGraphics);
            _vectorRenderer.RenderTransientShapes(surface, fallbackGraphics, _engine, shapes, forceUnselected);
        }

        public void RenderTransientShapes(
            Graphics graphics,
            MapCanvasEngine engine,
            IReadOnlyList<(IShape Shape, CanvasLayer? Layer, CanvasObject? CanvasObject)> shapes,
            bool forceUnselected = false)
        {
            if (shapes == null || shapes.Count == 0)
            {
                return;
            }

            ConfigureVectorGraphics(graphics);
            _vectorRenderer.RenderTransientShapes(graphics, engine, shapes, forceUnselected);
        }

        public void InvalidateVectorCache()
        {
            _vectorDeferredRenderer.Invalidate();
        }

        public bool HasValidVectorCache => _vectorDeferredRenderer.HasValidCache;

        /// <summary>
        /// Draws selection decoration for one selected feature on top of the
        /// current frame without rebuilding the deferred vector cache.
        /// </summary>
        public void RenderSelectionDecoration(
            Graphics graphics,
            IShape? shape,
            CanvasLayer? layer,
            CanvasFeature? feature)
        {
            if (shape == null)
            {
                return;
            }

            ConfigureVectorGraphics(graphics);
            _vectorRenderer.RenderSelectionDecoration(
                graphics,
                _engine,
                shape,
                layer,
                feature,
                _settings.AntiAliasingEnabled);
        }

        public void RenderSelectionDecoration(
            IMapRenderSurface surface,
            IShape? shape,
            CanvasLayer? layer,
            CanvasFeature? feature)
        {
            if (shape == null)
            {
                return;
            }

            using Bitmap metricsBitmap = new(1, 1, PixelFormat.Format32bppPArgb);
            using Graphics fallbackGraphics = Graphics.FromImage(metricsBitmap);
            ConfigureVectorGraphics(fallbackGraphics);
            _vectorRenderer.RenderSelectionDecoration(
                surface,
                fallbackGraphics,
                _engine,
                shape,
                layer,
                feature,
                _settings.AntiAliasingEnabled);
        }

        public bool RefreshVectorCache(Size canvasSize)
        {
            return _vectorDeferredRenderer.RenderNow(
                canvasSize,
                _vectorRenderer,
                _engine,
                _settings.AntiAliasingEnabled);
        }

        public async Task<bool> RefreshVectorCacheAsync(
            Size canvasSize,
            CancellationToken cancellationToken = default)
        {
            MapCanvasEngine engineSnapshot = _engine.CreateSnapshot();
            Size sizeSnapshot = canvasSize;

            return await Task.Run(
                () => _vectorDeferredRenderer.RenderNow(
                    sizeSnapshot,
                    _vectorRenderer,
                    engineSnapshot,
                    _settings.AntiAliasingEnabled,
                    cancellationToken),
                cancellationToken);
        }

        public bool BeginVectorPan(Size canvasSize, Action<Graphics>? renderOverlay = null)
        {
            return _vectorDeferredRenderer.BeginPan(canvasSize, renderOverlay);
        }

        public bool BeginVectorZoom(Size canvasSize, Action<Graphics>? renderOverlay = null)
        {
            return _vectorDeferredRenderer.BeginZoom(canvasSize, _engine, renderOverlay);
        }

        public void EndVectorZoom()
        {
            _vectorDeferredRenderer.EndZoom();
        }

        public bool TryGetVectorCacheFrame(out RasterRenderFrame frame)
        {
            return _vectorDeferredRenderer.TryGetCacheFrame(out frame);
        }

        public bool TryGetVectorZoomFrame(out RasterRenderFrame frame)
        {
            return _vectorDeferredRenderer.TryGetZoomFrame(_engine, out frame);
        }

        public bool TryGetVectorPanFrame(PointF totalPanDelta, out RasterRenderFrame frame)
        {
            return _vectorDeferredRenderer.TryGetPanFrame(totalPanDelta, out frame);
        }

        public bool IsDebugOverlayRequested => _debugOverlayRequested;

        public MapCanvasRendererDebugState GetDebugState()
        {
            return new MapCanvasRendererDebugState
            {
                RasterLayerCount = _rasterLayers.Count,
                VisibleRasterLayerCount = _rasterLayers.Count(layer => layer.IsVisible),
                VectorLayerCount = _vectorRenderer.LayerCount,
                VectorStats = _vectorRenderer.LastRenderStats,
                VectorCache = _vectorDeferredRenderer.GetDebugState()
            };
        }

        /// <summary>
        /// Captures the current world and screen bounds used by frame render passes.
        /// </summary>
        /// <param name="graphics">Graphics surface whose visible bounds are being rendered.</param>
        /// <returns>The viewport values for the current frame.</returns>
        private MapCanvasRenderViewport CreateViewport(Graphics graphics)
        {
            return new MapCanvasRenderViewport(
                _engine.GetVisibleWorldBounds(),
                graphics.VisibleClipBounds);
        }

        private MapCanvasRenderViewport CreateViewport(Size pixelSize)
        {
            return new MapCanvasRenderViewport(
                _engine.GetVisibleWorldBounds(),
                new RectangleF(0, 0, Math.Max(1, pixelSize.Width), Math.Max(1, pixelSize.Height)));
        }

        /// <summary>
        /// Creates a backend-neutral surface for the current frame graphics target.
        /// For the Skia CPU backend a shared backing bitmap is reused across passes
        /// within a frame to avoid the per-call unmanaged allocation cost.
        /// </summary>
        /// <param name="graphics">Native graphics target supplied by WinForms.</param>
        /// <param name="options">Surface creation and quality options.</param>
        private IMapRenderSurface CreateFrameSurface(
            Graphics graphics,
            MapRenderSurfaceOptions? options = null)
        {
            MapRenderSurfaceOptions resolvedOptions = CreateSurfaceOptions(options);
            if (resolvedOptions.RequestedBackend == MapRenderBackend.SkiaCpu)
            {
                Size size = Size.Round(graphics.VisibleClipBounds.Size);
                size = new Size(Math.Max(1, size.Width), Math.Max(1, size.Height));
                if (_skiaBackingBitmap == null ||
                    _skiaBackingBitmap.Width != size.Width ||
                    _skiaBackingBitmap.Height != size.Height)
                {
                    _skiaBackingBitmap?.Dispose();
                    _skiaBackingBitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
                }

                SkiaCpuMapRenderSurface surface = new(_skiaBackingBitmap, graphics);
                if (resolvedOptions.ApplyInitialQuality)
                {
                    surface.SetQuality(resolvedOptions.InitialQuality);
                }

                return surface;
            }

            return _renderSurfaceFactory.CreateForGraphics(
                graphics,
                Size.Round(graphics.VisibleClipBounds.Size),
                resolvedOptions);
        }

        /// <summary>
        /// Copies a surface option template while injecting the backend selected
        /// by the current render settings.
        /// </summary>
        private MapRenderSurfaceOptions CreateSurfaceOptions(MapRenderSurfaceOptions? options)
        {
            MapRenderSurfaceOptions template = options ?? OverlaySurfaceOptions;
            return new MapRenderSurfaceOptions
            {
                RequestedBackend = _settings.RenderBackend,
                InitialQuality = template.InitialQuality,
                ApplyInitialQuality = template.ApplyInitialQuality,
                FallbackToGdiPlusWhenUnavailable = template.FallbackToGdiPlusWhenUnavailable
            };
        }

        /// <summary>
        /// Creates a backend-neutral image wrapper for a cached bitmap frame.
        /// </summary>
        private static IMapImage CreateFrameImage(RasterRenderFrame frame) =>
            new GdiMapImage(
                frame.Bitmap,
                ownsImage: false,
                allowSkiaImageCache: frame.CacheableOnGpu);

        /// <summary>
        /// Draws fixed reference visuals that should be placed before map content.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="viewport">Current world and screen viewport.</param>
        private void RenderFixedReferenceLayers(
            Graphics graphics,
            MapCanvasRenderViewport viewport,
            bool suppressGridLabels = false,
            double? gridMinorWorldSize = null)
        {
            // The only fixed-reference content is the grid, so skip allocating a
            // surface entirely when the grid is hidden (avoids a wasted GPU/CPU
            // surface per frame).
            if (!_settings.ShowGrid)
            {
                return;
            }

            using IMapRenderSurface surface = CreateFrameSurface(graphics);
            RenderFixedReferenceLayers(
                surface,
                viewport,
                suppressGridLabels,
                gridMinorWorldSize);
        }

        private void RenderFixedReferenceLayers(
            IMapRenderSurface surface,
            MapCanvasRenderViewport viewport,
            bool suppressGridLabels = false,
            double? gridMinorWorldSize = null)
        {
            if (_settings.ShowGrid)
            {
                RenderGrid(surface, viewport, suppressGridLabels, gridMinorWorldSize);
            }
        }

        private void RenderFixedReferenceContent(
            Graphics graphics,
            MapCanvasRenderViewport viewport,
            RasterRenderFrame? fixedReferenceFrame,
            bool suppressGridLabels,
            bool suppressFixedReferenceLayers)
        {
            if (fixedReferenceFrame.HasValue)
            {
                RasterRenderFrame frame = fixedReferenceFrame.Value;
                try
                {
                    DrawCachedFrame(
                        graphics,
                        frame,
                        InterpolationMode.NearestNeighbor);
                }
                finally
                {
                    frame.Dispose();
                }

                return;
            }

            if (suppressFixedReferenceLayers)
            {
                return;
            }

            ConfigureVectorGraphics(graphics);
            RenderFixedReferenceLayers(
                graphics,
                viewport,
                suppressGridLabels);
        }

        private void RenderFixedReferenceContent(
            IMapRenderSurface surface,
            MapCanvasRenderViewport viewport,
            RasterRenderFrame? fixedReferenceFrame,
            bool suppressGridLabels,
            bool suppressFixedReferenceLayers)
        {
            if (fixedReferenceFrame.HasValue)
            {
                RasterRenderFrame frame = fixedReferenceFrame.Value;
                try
                {
                    DrawCachedFrame(
                        surface,
                        frame,
                        InterpolationMode.NearestNeighbor);
                }
                finally
                {
                    frame.Dispose();
                }

                return;
            }

            if (suppressFixedReferenceLayers)
            {
                return;
            }

            RenderFixedReferenceLayers(
                surface,
                viewport,
                suppressGridLabels);
        }

        public void RenderFixedReferences(Graphics graphics, bool suppressGridLabels = false)
        {
            RenderFixedReferences(graphics, suppressGridLabels, gridMinorWorldSize: null);
        }

        public void RenderFixedReferences(
            Graphics graphics,
            bool suppressGridLabels,
            double? gridMinorWorldSize)
        {
            ArgumentNullException.ThrowIfNull(graphics);

            ConfigureVectorGraphics(graphics);
            RenderFixedReferenceLayers(
                graphics,
                CreateViewport(graphics),
                suppressGridLabels,
                gridMinorWorldSize);
        }

        public void RenderFixedReferences(
            IMapRenderSurface surface,
            bool suppressGridLabels = false,
            double? gridMinorWorldSize = null)
        {
            ArgumentNullException.ThrowIfNull(surface);

            surface.SetQuality(RenderQuality.VectorHighQuality);
            RenderFixedReferenceLayers(
                surface,
                CreateViewport(surface.PixelSize),
                suppressGridLabels,
                gridMinorWorldSize);
        }

        public double GetCurrentGridMinorWorldSize()
        {
            if (!_settings.ShowGrid ||
                _engine.ZoomScale < 0.001 ||
                _engine.ZoomScale > 100000)
            {
                return 0;
            }

            return ResolveAdaptiveMinorSize(_engine.ZoomScale);
        }

        public GridPanPadding GetGridPanPadding(Size canvasSize)
        {
            if (!_settings.ShowGrid ||
                canvasSize.Width <= 0 ||
                canvasSize.Height <= 0 ||
                _engine.ZoomScale < 0.001 ||
                _engine.ZoomScale > 100000)
            {
                return GridPanPadding.Empty;
            }

            const int majorDivisions = 5;

            double minorWorldSize = ResolveAdaptiveMinorSize(_engine.ZoomScale);
            double majorWorldSize = minorWorldSize * majorDivisions;
            if (!double.IsFinite(majorWorldSize) || majorWorldSize <= 0)
            {
                return GridPanPadding.Empty;
            }

            RectangleD visibleWorld = _engine.GetVisibleWorldBounds();
            double worldLeft = visibleWorld.Left;
            double worldRight = visibleWorld.Right;
            double worldBottom = Math.Min(visibleWorld.Top, visibleWorld.Bottom);
            double worldTop = Math.Max(visibleWorld.Top, visibleWorld.Bottom);

            double leftMajor = Math.Floor(worldLeft / majorWorldSize) * majorWorldSize;
            if (leftMajor >= worldLeft - 1e-9)
            {
                leftMajor -= majorWorldSize;
            }

            double rightMajor = Math.Ceiling(worldRight / majorWorldSize) * majorWorldSize;
            if (rightMajor <= worldRight + 1e-9)
            {
                rightMajor += majorWorldSize;
            }

            double topMajor = Math.Ceiling(worldTop / majorWorldSize) * majorWorldSize;
            if (topMajor <= worldTop + 1e-9)
            {
                topMajor += majorWorldSize;
            }

            double bottomMajor = Math.Floor(worldBottom / majorWorldSize) * majorWorldSize;
            if (bottomMajor >= worldBottom - 1e-9)
            {
                bottomMajor -= majorWorldSize;
            }

            PointD leftScreen = _engine.WorldToScreen(new PointD(leftMajor, worldBottom));
            PointD rightScreen = _engine.WorldToScreen(new PointD(rightMajor, worldBottom));
            PointD topScreen = _engine.WorldToScreen(new PointD(worldLeft, topMajor));
            PointD bottomScreen = _engine.WorldToScreen(new PointD(worldLeft, bottomMajor));

            return new GridPanPadding(
                GetBoundedGridPadding(-leftScreen.X),
                GetBoundedGridPadding(-topScreen.Y),
                GetBoundedGridPadding(rightScreen.X - canvasSize.Width),
                GetBoundedGridPadding(bottomScreen.Y - canvasSize.Height));
        }

        private static int GetBoundedGridPadding(double value)
        {
            if (!double.IsFinite(value) || value <= 0)
            {
                return 0;
            }

            const int maximumPadding = 4096;
            return Math.Clamp((int)Math.Ceiling(value) + 1, 0, maximumPadding);
        }

        /// <summary>
        /// Draws the raster frame cache or visible raster layers for the current viewport.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="viewport">Current world and screen viewport.</param>
        /// <param name="rasterFrame">Optional cached raster frame used during interaction.</param>
        /// <param name="interactiveRaster">Whether raster rendering is happening during navigation.</param>
        private void RenderRasterContent(
            Graphics graphics,
            MapCanvasRenderViewport viewport,
            RasterRenderFrame? rasterFrame,
            bool interactiveRaster)
        {
            if (rasterFrame.HasValue)
            {
                // Draw only the shifted/scaled cached raster frame during interaction.
                // Do not ask live XYZ layers to draw again here; doing both leaves the
                // previous cache visible behind a second shifted cache.
                RasterRenderFrame frame = rasterFrame.Value;
                try
                {
                    DrawCachedFrame(
                        graphics,
                        frame,
                            InterpolationMode.NearestNeighbor);
                }
                finally
                {
                    frame.Dispose();
                }
            }
            else if (interactiveRaster)
            {
                // Interaction frames must not invoke layer rendering or network-backed tiles.
                return;
            }
            else
            {
                RenderRasterLayers(
                    graphics,
                    viewport,
                    interactiveRaster,
                cachedOnly: false);
            }
        }

        private void RenderRasterContent(
            IMapRenderSurface surface,
            MapCanvasRenderViewport viewport,
            RasterRenderFrame? rasterFrame,
            bool interactiveRaster)
        {
            if (rasterFrame.HasValue)
            {
                RasterRenderFrame frame = rasterFrame.Value;
                try
                {
                    DrawCachedFrame(
                        surface,
                        frame,
                        InterpolationMode.NearestNeighbor);
                }
                finally
                {
                    frame.Dispose();
                }
            }
            else if (interactiveRaster)
            {
                return;
            }
            else
            {
                RenderRasterLayers(surface, viewport, interactiveRaster);
            }
        }

        private void RenderVectorContent(
            Graphics graphics,
            MapCanvasRenderViewport viewport,
            RasterRenderFrame? vectorFrame,
            bool interactiveVector)
        {
            if (vectorFrame.HasValue)
            {
                RasterRenderFrame frame = vectorFrame.Value;
                try
                {
                    DrawCachedFrame(
                        graphics,
                        frame,
                        InterpolationMode.NearestNeighbor);
                }
                finally
                {
                    frame.Dispose();
                }

                return;
            }

            if (interactiveVector)
            {
                return;
            }

            _vectorRenderer.Render(
                graphics,
                _engine,
                viewport.VisibleWorldBounds,
                useLevelOfDetail: _vectorRenderer.FeatureCount > LevelOfDetailDirectRenderThreshold,
                canvasSize: Size.Round(viewport.VisibleScreenBounds.Size),
                antiAliasingEnabled: _settings.AntiAliasingEnabled);
        }

        private void RenderVectorContent(
            IMapRenderSurface surface,
            MapCanvasRenderViewport viewport,
            RasterRenderFrame? vectorFrame,
            bool interactiveVector)
        {
            if (vectorFrame.HasValue)
            {
                RasterRenderFrame frame = vectorFrame.Value;
                try
                {
                    DrawCachedFrame(
                        surface,
                        frame,
                        InterpolationMode.NearestNeighbor);
                }
                finally
                {
                    frame.Dispose();
                }

                return;
            }

            if (interactiveVector)
            {
                return;
            }

            using Bitmap metricsBitmap = new(1, 1, PixelFormat.Format32bppPArgb);
            using Graphics fallbackGraphics = Graphics.FromImage(metricsBitmap);
            ConfigureVectorGraphics(fallbackGraphics);
            _vectorRenderer.Render(
                surface,
                fallbackGraphics,
                _engine,
                viewport.VisibleWorldBounds,
                useLevelOfDetail: _vectorRenderer.FeatureCount > LevelOfDetailDirectRenderThreshold,
                canvasSize: Size.Round(viewport.VisibleScreenBounds.Size),
                antiAliasingEnabled: _settings.AntiAliasingEnabled);
        }

        /// <summary>
        /// Draws temporary interaction feedback that must stay visible above map content.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="viewport">Current world and screen viewport.</param>
        /// <param name="zoomWindowRectangle">Optional screen-space zoom-window rectangle.</param>
        private void RenderInteractionOverlay(
            Graphics graphics,
            MapCanvasRenderViewport viewport,
            Rectangle? zoomWindowRectangle,
            bool suppressDecorations = false)
        {
            if (zoomWindowRectangle.HasValue)
            {
                RenderZoomWindow(graphics, zoomWindowRectangle.Value);
            }

            if (!suppressDecorations && (_settings.ShowAxisLines || _settings.ShowOriginMarker))
            {
                RenderAxisAndOriginMarker(graphics, viewport);
            }

            if (!suppressDecorations && _settings.ShowNorthMarker)
            {
                RenderNorthMarker(graphics, viewport.VisibleScreenBounds);
            }
        }

        private void RenderInteractionOverlay(
            IMapRenderSurface surface,
            MapCanvasRenderViewport viewport,
            Rectangle? zoomWindowRectangle,
            bool suppressDecorations = false)
        {
            if (zoomWindowRectangle.HasValue)
            {
                RenderZoomWindow(surface, zoomWindowRectangle.Value);
            }

            if (!suppressDecorations && (_settings.ShowAxisLines || _settings.ShowOriginMarker))
            {
                RenderAxisAndOriginMarker(surface, viewport);
            }

            if (!suppressDecorations && _settings.ShowNorthMarker)
            {
                RenderNorthMarker(surface, viewport.VisibleScreenBounds);
            }
        }

        /// <summary>
        /// Draws screen-anchored decorations (world axis lines, origin marker and
        /// north marker) fresh for the current viewport, without the zoom-window
        /// rectangle. The GPU path calls this every frame on top of the cached
        /// map snapshot so these overlays are never baked into the snapshot and
        /// stay crisp and correctly positioned during pan/zoom, exactly as the
        /// GDI path redraws them each frame.
        /// </summary>
        /// <param name="surface">Target render surface.</param>
        public void RenderScreenDecorations(IMapRenderSurface surface)
        {
            ArgumentNullException.ThrowIfNull(surface);

            MapCanvasRenderViewport viewport = CreateViewport(surface.PixelSize);
            surface.SetQuality(
                _settings.AntiAliasingEnabled
                    ? RenderQuality.VectorHighQuality
                    : RenderQuality.VectorHighSpeed);

            if (_settings.ShowAxisLines || _settings.ShowOriginMarker)
            {
                RenderAxisAndOriginMarker(surface, viewport);
            }

            if (_settings.ShowNorthMarker)
            {
                RenderNorthMarker(surface, viewport.VisibleScreenBounds);
            }
        }

        /// <summary>
        /// Draws the north marker overlay through the backend-neutral render surface.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="clientRect">Visible canvas bounds in screen pixels.</param>
        private void RenderNorthMarker(Graphics graphics, RectangleF clientRect)
        {
            using IMapRenderSurface surface = CreateFrameSurface(graphics);
            RenderNorthMarker(surface, clientRect);
        }

        private void RenderNorthMarker(IMapRenderSurface surface, RectangleF clientRect)
        {
            float minSide = Math.Min(clientRect.Width, clientRect.Height);
            float size = Math.Max(36.0f, Math.Min(60.0f, minSide * 0.10f));
            float margin = Math.Max(10.0f, size * 0.24f);

            float centerX = clientRect.Right - margin - size * 0.5f;
            float top = clientRect.Top + margin;

            Color canvasBg = _settings.BackgroundColor;
            bool isDarkCanvas = CanvasThemeColorService.IsDarkCanvas(canvasBg);
            Color lineColor = isDarkCanvas
                ? Color.FromArgb(242, 248, 250)
                : Color.FromArgb(18, 23, 28);
            Color rightFillColor = isDarkCanvas
                ? Color.FromArgb(78, 201, 176)
                : Color.FromArgb(20, 24, 29);
            Color leftFillColor = isDarkCanvas
                ? Color.FromArgb(48, 60, 66)
                : Color.FromArgb(245, 248, 250);

            float arrowTop = top;
            float arrowBottom = arrowTop + size * 0.78f;
            float arrowHalf = size * 0.22f;
            float spineBottom = arrowBottom - size * 0.16f;

            PointF tip = new(centerX, arrowTop);
            PointF spine = new(centerX, spineBottom);
            PointF leftBase = new(centerX - arrowHalf, arrowBottom);
            PointF rightBase = new(centerX + arrowHalf, arrowBottom);

            using IMapPath leftTriangle = CreatePolygonPath(surface, [tip, spine, leftBase]);
            using IMapPath rightTriangle = CreatePolygonPath(surface, [tip, rightBase, spine]);
            using IMapPath outline = CreatePolygonPath(surface, [tip, rightBase, spine, leftBase]);

            FillStyle leftFill = new(leftFillColor);
            FillStyle rightFill = new(rightFillColor);
            StrokeStyle outlineStroke = new(
                lineColor,
                Math.Max(1.4f, size * 0.035f),
                Cap: LineCapKind.Flat,
                Join: LineJoinKind.Miter);

            surface.FillPath(leftTriangle, leftFill);
            surface.FillPath(rightTriangle, rightFill);
            surface.DrawPath(outline, outlineStroke);
            surface.DrawLine(tip, spine, outlineStroke);

            float labelTop = arrowBottom + Math.Max(2.0f, size * 0.04f);
            TextStyle labelStyle = new(
                "Segoe UI",
                Math.Max(10f, size * 0.26f),
                lineColor,
                Bold: true,
                HorizontalAlign: TextAlign.Center,
                VerticalAlign: TextAlign.Near);
            surface.DrawText(
                "N",
                new RectangleF(centerX - size * 0.5f, labelTop, size, Math.Max(14.0f, size * 0.35f)),
                labelStyle);
        }

        /// <summary>
        /// Creates a backend-owned closed polygon path for screen-space overlay geometry.
        /// </summary>
        private static IMapPath CreatePolygonPath(IMapRenderSurface surface, ReadOnlySpan<PointF> points)
        {
            IMapPathBuilder builder = surface.CreatePath();
            builder.AddPolygon(points);
            return builder.Build();
        }
        /// <summary>
        /// Draws all visible raster layer tiles that intersect the current viewport.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="viewport">Current world and screen viewport.</param>
        /// <param name="interactive">Whether raster rendering is happening during navigation.</param>
        private void RenderRasterLayers(
            Graphics graphics,
            MapCanvasRenderViewport viewport,
            bool interactive,
            bool cachedOnly)
        {
            if (_rasterLayers.Count == 0)
                return;

            foreach (IRasterRenderLayer layer in _rasterLayers)
            {
                if (cachedOnly &&
                    !layer.CanRenderFromMemoryCacheDuringInteraction)
                {
                    continue;
                }

                layer.RenderVisible(
                    graphics,
                    _engine,
                    viewport.VisibleWorldBounds,
                    interactive,
                    _settings.RenderBackend);
            }
        }

        private void RenderRasterLayers(
            IMapRenderSurface surface,
            MapCanvasRenderViewport viewport,
            bool interactive)
        {
            if (_rasterLayers.Count == 0)
            {
                return;
            }

            foreach (IRasterRenderLayer layer in _rasterLayers)
            {
                layer.RenderVisible(
                    surface,
                    _engine,
                    viewport.VisibleWorldBounds,
                    interactive);
            }
        }

        /// <summary>
        /// Draws a cached raster frame using fast image settings during interactive navigation.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="rasterFrame">Cached raster bitmap and its screen destination.</param>
        private void DrawCachedFrame(
            Graphics graphics,
            RasterRenderFrame rasterFrame,
            InterpolationMode interpolationMode)
        {
            lock (rasterFrame.SyncRoot)
            {
                if (!TryCreateClippedRasterDraw(
                    rasterFrame.Bitmap,
                    rasterFrame.Destination,
                    graphics.VisibleClipBounds,
                    out RectangleF destination,
                    out RectangleF source))
                {
                    return;
                }

                try
                {
                    using IMapRenderSurface surface = CreateFrameSurface(
                        graphics,
                        RasterFrameSurfaceOptions);
                    using IMapImage image = CreateFrameImage(rasterFrame);
                    surface.DrawImage(
                        image,
                        destination,
                        source,
                        new ImageStyle(1.0f, ToImageInterpolation(interpolationMode)));
                }
                catch (ArgumentException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Skipped invalid raster cache frame: {ex.Message}");
                }
                catch (ExternalException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Skipped GDI raster cache frame draw: {ex.Message}");
                }
            }
        }

        private static void DrawCachedFrame(
            IMapRenderSurface surface,
            RasterRenderFrame rasterFrame,
            InterpolationMode interpolationMode)
        {
            lock (rasterFrame.SyncRoot)
            {
                RectangleF clipBounds = new(0, 0, surface.PixelSize.Width, surface.PixelSize.Height);
                if (!TryCreateClippedRasterDraw(
                    rasterFrame.Bitmap,
                    rasterFrame.Destination,
                    clipBounds,
                    out RectangleF destination,
                    out RectangleF source))
                {
                    return;
                }

                using IMapImage image = CreateFrameImage(rasterFrame);
                surface.SetQuality(RenderQuality.RasterHighSpeed);
                surface.DrawImage(
                    image,
                    destination,
                    source,
                    new ImageStyle(1.0f, ToImageInterpolation(interpolationMode)));
            }
        }

        /// <summary>
        /// Converts a GDI+ interpolation mode into the backend-neutral image mode.
        /// </summary>
        private static ImageInterpolation ToImageInterpolation(InterpolationMode interpolationMode) =>
            interpolationMode == InterpolationMode.HighQualityBicubic
                ? ImageInterpolation.HighQuality
                : ImageInterpolation.NearestNeighbor;

        private static bool TryCreateClippedRasterDraw(
            Bitmap bitmap,
            RectangleF destination,
            RectangleF clipBounds,
            out RectangleF clippedDestination,
            out RectangleF source)
        {
            clippedDestination = default;
            source = default;

            if (!IsDrawableBitmap(bitmap) ||
                !IsValidRectangle(destination))
            {
                return false;
            }

            destination = NormalizeRectangle(destination);
            if (destination.Width <= 0 || destination.Height <= 0)
            {
                return false;
            }

            RectangleF clip = IsValidRectangle(clipBounds)
                ? NormalizeRectangle(clipBounds)
                : destination;

            float left = Math.Max(destination.Left, clip.Left);
            float top = Math.Max(destination.Top, clip.Top);
            float right = Math.Min(destination.Right, clip.Right);
            float bottom = Math.Min(destination.Bottom, clip.Bottom);

            if (right <= left || bottom <= top)
            {
                return false;
            }

            float sourceScaleX = bitmap.Width / destination.Width;
            float sourceScaleY = bitmap.Height / destination.Height;
            float sourceLeft = Math.Clamp((left - destination.Left) * sourceScaleX, 0, bitmap.Width);
            float sourceTop = Math.Clamp((top - destination.Top) * sourceScaleY, 0, bitmap.Height);
            float sourceRight = Math.Clamp((right - destination.Left) * sourceScaleX, 0, bitmap.Width);
            float sourceBottom = Math.Clamp((bottom - destination.Top) * sourceScaleY, 0, bitmap.Height);

            if (sourceRight <= sourceLeft || sourceBottom <= sourceTop)
            {
                return false;
            }

            clippedDestination = RectangleF.FromLTRB(
                destination.Left + sourceLeft / sourceScaleX,
                destination.Top + sourceTop / sourceScaleY,
                destination.Left + sourceRight / sourceScaleX,
                destination.Top + sourceBottom / sourceScaleY);
            source = RectangleF.FromLTRB(
                sourceLeft,
                sourceTop,
                sourceRight,
                sourceBottom);
            return IsValidRectangle(clippedDestination) &&
                   IsValidRectangle(source);
        }

        private static bool IsDrawableBitmap(Bitmap bitmap)
        {
            try
            {
                return bitmap.Width > 0 && bitmap.Height > 0;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static RectangleF NormalizeRectangle(RectangleF rectangle)
        {
            return RectangleF.FromLTRB(
                Math.Min(rectangle.Left, rectangle.Right),
                Math.Min(rectangle.Top, rectangle.Bottom),
                Math.Max(rectangle.Left, rectangle.Right),
                Math.Max(rectangle.Top, rectangle.Bottom));
        }

        private static bool IsValidRectangle(RectangleF rectangle)
        {
            return IsFinite(rectangle.Left) &&
                   IsFinite(rectangle.Top) &&
                   IsFinite(rectangle.Right) &&
                   IsFinite(rectangle.Bottom) &&
                   rectangle.Width > 0 &&
                   rectangle.Height > 0;
        }

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        /// <summary>
        /// Releases GDI resources used by renderer-owned fonts.
        /// </summary>
        public void Dispose()
        {
            _vectorRenderer.Dispose();
            _vectorDeferredRenderer.Dispose();
            _skiaBackingBitmap?.Dispose();
            _skiaBackingBitmap = null;
        }

        /// <summary>
        /// Applies high-quality graphics options for anti-aliased rendering.
        /// </summary>
        /// <param name="graphics">Graphics context to configure.</param>
        // For vector content — grid, axis, overlays
        private void ConfigureVectorGraphics(Graphics graphics)
        {
            if (_settings.AntiAliasingEnabled)
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                // Vector coordinates are integer-snapped in ToScreenPointF, so the
                // aliased path (PixelOffsetMode.None) sits exactly on pixel
                // boundaries. Using Half/HighQuality here would offset sampling by
                // half a pixel and make shapes jump north-west when AA is toggled.
                // Keep None so geometry stays put regardless of AA state.
                graphics.PixelOffsetMode = PixelOffsetMode.None;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                return;
            }

            graphics.SmoothingMode = SmoothingMode.None;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        }

        // For raster content — tiles, bitmaps
        private static void ConfigureRasterGraphics(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
        }

        private void ConfigureVectorContentGraphics(Graphics graphics)
        {
            ConfigureVectorGraphics(graphics);
        }

        /// <summary>
        /// Draws adaptive major/minor grid lines and optional coordinate labels
        /// for the current visible world extent.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="viewport">Current world and screen viewport.</param>
        private void RenderGrid(
            Graphics graphics,
            MapCanvasRenderViewport viewport,
            bool suppressLabels = false,
            double? gridMinorWorldSize = null)
        {
            using IMapRenderSurface surface = CreateFrameSurface(graphics);
            RenderGrid(
                surface,
                viewport,
                suppressLabels,
                gridMinorWorldSize);
        }

        private void RenderGrid(
            IMapRenderSurface surface,
            MapCanvasRenderViewport viewport,
            bool suppressLabels = false,
            double? gridMinorWorldSize = null)
        {
            double zoomScale = _engine.ZoomScale;
            if (zoomScale < 0.001 || zoomScale > 100000)
            {
                return;
            }

            double worldLeft = viewport.VisibleWorldBounds.Left;
            double worldRight = viewport.VisibleWorldBounds.Right;
            double worldBottom = Math.Min(
                viewport.VisibleWorldBounds.Top,
                viewport.VisibleWorldBounds.Bottom);
            double worldTop = Math.Max(
                viewport.VisibleWorldBounds.Top,
                viewport.VisibleWorldBounds.Bottom);

            const int majorDivisions = 5;
            const double minMinorPixels = 10.0;
            const double maxMinorPixels = 32.0;
            double adaptiveMinorSize = gridMinorWorldSize.GetValueOrDefault();
            if (!gridMinorWorldSize.HasValue ||
                !double.IsFinite(adaptiveMinorSize) ||
                adaptiveMinorSize <= 0)
            {
                adaptiveMinorSize = ResolveAdaptiveMinorSize(
                    zoomScale,
                    minMinorPixels,
                    maxMinorPixels);
            }

            double adaptiveMajorSize = adaptiveMinorSize * majorDivisions;

            double worldWidth = worldRight - worldLeft;
            double worldHeight = worldTop - worldBottom;
            int estimatedVerticalLines = Math.Max(1, (int)(worldWidth / adaptiveMinorSize));
            int estimatedHorizontalLines = Math.Max(1, (int)(worldHeight / adaptiveMinorSize));

            const int maxLinesPerAxis = 500;
            if (estimatedVerticalLines > maxLinesPerAxis || estimatedHorizontalLines > maxLinesPerAxis)
            {
                return;
            }

            double startX = Math.Floor(worldLeft / adaptiveMajorSize) * adaptiveMajorSize;
            double endX = Math.Ceiling(worldRight / adaptiveMajorSize) * adaptiveMajorSize;
            double startY = Math.Floor(worldBottom / adaptiveMajorSize) * adaptiveMajorSize;
            double endY = Math.Ceiling(worldTop / adaptiveMajorSize) * adaptiveMajorSize;
            bool showLabels =
                !suppressLabels &&
                _settings.ShowGridLabels &&
                zoomScale > 0.00001 &&
                zoomScale < 10000 &&
                estimatedVerticalLines < 500;

            StrokeStyle minorStroke = new(_settings.MinorGridColor, 0.25f, Cap: LineCapKind.Flat);
            StrokeStyle majorStroke = new(_settings.MajorGridColor, 0.25f, Cap: LineCapKind.Flat);
            TextStyle xLabelStyle = new(
                "Arial",
                GridFontSizePx,
                _settings.GridLabelColor,
                HorizontalAlign: TextAlign.Center,
                VerticalAlign: TextAlign.Far);
            TextStyle yLabelStyle = new(
                "Arial",
                GridFontSizePx,
                _settings.GridLabelColor,
                HorizontalAlign: TextAlign.Near,
                VerticalAlign: TextAlign.Center);

            for (double x = startX; x <= endX; x += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(x, adaptiveMajorSize);
                if (!isMajor && !_settings.ShowMinorGridLines)
                {
                    continue;
                }

                StrokeStyle stroke = isMajor ? majorStroke : minorStroke;
                PointD screenStart = _engine.WorldToScreen(new PointD(x, worldBottom));
                PointD screenEnd = _engine.WorldToScreen(new PointD(x, worldTop));

                DrawClippedScreenLine(
                    surface,
                    stroke,
                    screenStart,
                    screenEnd,
                    viewport.VisibleScreenBounds);

                if (isMajor && showLabels)
                {
                    DrawPointLabel(
                        surface,
                        FormatGridLabel(x, adaptiveMajorSize),
                        new PointF((float)screenStart.X, (float)screenStart.Y),
                        xLabelStyle);
                }
            }

            for (double y = startY; y <= endY; y += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(y, adaptiveMajorSize);
                if (!isMajor && !_settings.ShowMinorGridLines)
                {
                    continue;
                }

                StrokeStyle stroke = isMajor ? majorStroke : minorStroke;
                PointD screenStart = _engine.WorldToScreen(new PointD(worldLeft, y));
                PointD screenEnd = _engine.WorldToScreen(new PointD(worldRight, y));

                DrawClippedScreenLine(
                    surface,
                    stroke,
                    screenStart,
                    screenEnd,
                    viewport.VisibleScreenBounds);

                if (isMajor && showLabels)
                {
                    DrawPointLabel(
                        surface,
                        FormatGridLabel(y, adaptiveMajorSize),
                        new PointF(10.0f, (float)screenStart.Y),
                        yLabelStyle);
                }
            }
        }

        /// <summary>
        /// Draws a text label anchored at one screen point using backend text metrics.
        /// </summary>
        private static void DrawPointLabel(
            IMapRenderSurface surface,
            string text,
            PointF anchor,
            in TextStyle style)
        {
            SizeF size = surface.MeasureText(text, style);
            RectangleF layout = new(
                anchor.X - ResolveHorizontalAnchorOffset(size.Width, style.HorizontalAlign),
                anchor.Y - ResolveVerticalAnchorOffset(size.Height, style.VerticalAlign),
                Math.Max(1.0f, size.Width),
                Math.Max(1.0f, size.Height));
            surface.DrawText(text, layout, style);
        }

        /// <summary>
        /// Resolves how much a point-anchored label should move left for alignment.
        /// </summary>
        private static float ResolveHorizontalAnchorOffset(float width, TextAlign align) =>
            align switch
            {
                TextAlign.Center => width / 2.0f,
                TextAlign.Far => width,
                _ => 0.0f
            };

        /// <summary>
        /// Resolves how much a point-anchored label should move up for alignment.
        /// </summary>
        private static float ResolveVerticalAnchorOffset(float height, TextAlign align) =>
            align switch
            {
                TextAlign.Center => height / 2.0f,
                TextAlign.Far => height,
                _ => 0.0f
            };

        private double ResolveAdaptiveMinorSize(
            double zoomScale,
            double minMinorPixels = 10.0,
            double maxMinorPixels = 32.0)
        {
            const double minMinorWorld = 1e-9;
            const double maxMinorWorld = 1e12;

            double adaptiveMinorSize = _lastAdaptiveMinorSize > 0
                ? _lastAdaptiveMinorSize
                : SnapToNiceStep(16.0 / zoomScale);

            adaptiveMinorSize = SnapToNiceStep(adaptiveMinorSize);
            double minorGridPixels = adaptiveMinorSize * zoomScale;

            while (minorGridPixels < minMinorPixels && adaptiveMinorSize < maxMinorWorld)
            {
                adaptiveMinorSize = NextNiceStep(adaptiveMinorSize);
                minorGridPixels = adaptiveMinorSize * zoomScale;
            }

            while (minorGridPixels > maxMinorPixels && adaptiveMinorSize > minMinorWorld)
            {
                adaptiveMinorSize = PrevNiceStep(adaptiveMinorSize);
                minorGridPixels = adaptiveMinorSize * zoomScale;
            }

            adaptiveMinorSize = Math.Max(minMinorWorld, Math.Min(maxMinorWorld, adaptiveMinorSize));
            _lastAdaptiveMinorSize = adaptiveMinorSize;
            return adaptiveMinorSize;
        }

        /// <summary>
        /// Draws the temporary zoom-window overlay rectangle.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="rectangle">Screen-space zoom-window rectangle.</param>
        private void RenderZoomWindow(Graphics graphics, Rectangle rectangle)
        {
            using IMapRenderSurface surface = CreateFrameSurface(graphics);
            RenderZoomWindow(surface, rectangle);
        }

        private void RenderZoomWindow(IMapRenderSurface surface, Rectangle rectangle)
        {
            if (rectangle.Width < 2 || rectangle.Height < 2)
            {
                return;
            }

            RectangleF screenRectangle = new(
                rectangle.X,
                rectangle.Y,
                rectangle.Width,
                rectangle.Height);
            FillStyle fill = new(_settings.ZoomWindowFillColor);
            StrokeStyle border = new(
                _settings.ZoomWindowBorderColor,
                _settings.ZoomWindowLineWidth,
                ToDashPatternKind(_settings.ZoomWindowLineType),
                Cap: LineCapKind.Flat,
                Join: LineJoinKind.Miter);

            surface.FillRectangle(screenRectangle, fill);
            surface.DrawRectangle(screenRectangle, border);
        }

        /// <summary>
        /// Converts existing GDI+ dash settings into the backend-neutral dash model.
        /// </summary>
        private static DashPatternKind ToDashPatternKind(DashStyle dashStyle) =>
            dashStyle switch
            {
                DashStyle.Dash => DashPatternKind.Dashed,
                DashStyle.Dot => DashPatternKind.Dotted,
                DashStyle.DashDot => DashPatternKind.DashDot,
                DashStyle.DashDotDot => DashPatternKind.DashDoubleDot,
                _ => DashPatternKind.Solid
            };

        /// <summary>
        /// Renders world-origin axis lines and the origin marker/labels according to visibility settings.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        private void RenderAxisAndOriginMarker(
            Graphics graphics,
            MapCanvasRenderViewport viewport)
        {
            using IMapRenderSurface surface = CreateFrameSurface(graphics);
            RenderAxisAndOriginMarker(surface, viewport);
        }

        private void RenderAxisAndOriginMarker(
            IMapRenderSurface surface,
            MapCanvasRenderViewport viewport)
        {
            RectangleF clientRect = viewport.VisibleScreenBounds;
            PointD originScreen = _engine.WorldToScreen(new PointD(0, 0));
            PointD xAxisStart = _engine.WorldToScreen(
                new PointD(viewport.VisibleWorldBounds.Left, 0));
            PointD xAxisEnd = _engine.WorldToScreen(
                new PointD(viewport.VisibleWorldBounds.Right, 0));
            PointD yAxisStart = _engine.WorldToScreen(
                new PointD(0, viewport.VisibleWorldBounds.Bottom));
            PointD yAxisEnd = _engine.WorldToScreen(
                new PointD(0, viewport.VisibleWorldBounds.Top));

            if (_settings.ShowAxisLines)
            {
                StrokeStyle xAxisStroke = new(_settings.AxisXColor, _settings.AxisLineWidth, Cap: LineCapKind.Flat);
                StrokeStyle yAxisStroke = new(_settings.AxisYColor, _settings.AxisLineWidth, Cap: LineCapKind.Flat);

                DrawClippedScreenLine(
                    surface,
                    xAxisStroke,
                    xAxisStart,
                    xAxisEnd,
                    viewport.VisibleScreenBounds);

                DrawClippedScreenLine(
                    surface,
                    yAxisStroke,
                    yAxisStart,
                    yAxisEnd,
                    viewport.VisibleScreenBounds);
            }

            if (!_settings.ShowOriginMarker)
            {
                return;
            }

            bool originInViewport = originScreen.X >= clientRect.Left &&
                                    originScreen.X <= clientRect.Right &&
                                    originScreen.Y >= clientRect.Top &&
                                    originScreen.Y <= clientRect.Bottom;

            float markerLength = _settings.AxisMarkerLengthPx;
            float markerSquareSize = _settings.AxisMarkerSquareSizePx;
            StrokeStyle markerStroke = new(
                _settings.AxisMarkerColor,
                _settings.AxisMarkerLineWidth,
                Cap: LineCapKind.Flat);
            FillStyle markerFill = new(_settings.AxisMarkerColor);
            TextStyle markerText = new(
                "Arial",
                AxisFontSizePx,
                _settings.AxisLabelColor);

            if (!originInViewport)
            {
                const float edgePadding = 18.0f;
                float x = clientRect.Left + edgePadding;
                float y = clientRect.Bottom - edgePadding;

                DrawOriginMarker(
                    surface,
                    new PointF(x, y),
                    markerLength,
                    markerSquareSize,
                    markerStroke,
                    markerFill,
                    markerText,
                    _settings.ShowAxisLabels);

                return;
            }

            float ox = (float)originScreen.X;
            float oy = (float)originScreen.Y;

            DrawOriginMarker(
                surface,
                new PointF(ox, oy),
                markerLength,
                markerSquareSize,
                markerStroke,
                markerFill,
                markerText,
                _settings.ShowAxisLabels);
        }

        /// <summary>
        /// Draws the small X/Y origin marker through the backend surface.
        /// </summary>
        private static void DrawOriginMarker(
            IMapRenderSurface surface,
            PointF origin,
            float markerLength,
            float markerSquareSize,
            in StrokeStyle markerStroke,
            in FillStyle markerFill,
            in TextStyle markerText,
            bool showLabels)
        {
            surface.DrawLine(origin, new PointF(origin.X + markerLength, origin.Y), markerStroke);
            surface.DrawLine(origin, new PointF(origin.X, origin.Y - markerLength), markerStroke);
            surface.FillRectangle(
                new RectangleF(
                    origin.X - markerSquareSize / 2.0f,
                    origin.Y - markerSquareSize / 2.0f,
                    markerSquareSize,
                    markerSquareSize),
                markerFill);

            if (!showLabels)
            {
                return;
            }

            DrawPointLabel(
                surface,
                "X",
                new PointF(origin.X + markerLength + 4.0f, origin.Y - 10.0f),
                markerText);
            DrawPointLabel(
                surface,
                "Y",
                new PointF(origin.X - 14.0f, origin.Y - markerLength - 12.0f),
                markerText);
        }

        /// <summary>
        /// Checks whether a numeric value is finite before drawing.
        /// </summary>
        private static bool IsValid(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        /// <summary>
        /// Checks whether a point is safe for drawing on the graphics surface.
        /// </summary>
        /// <param name="point">Point to validate.</param>
        /// <returns>
        /// <see langword="true"/> when both X and Y are valid drawable values; otherwise <see langword="false"/>.
        /// </returns>
        private static bool IsValidPoint(PointD point)
        {
            return IsValid(point.X) && IsValid(point.Y);
        }

        /// <summary>
        /// Draws a screen-space segment after clipping it to the visible canvas.
        /// </summary>
        private static void DrawClippedScreenLine(
            IMapRenderSurface surface,
            in StrokeStyle stroke,
            PointD start,
            PointD end,
            RectangleF visibleScreenBounds)
        {
            if (!IsValidPoint(start) || !IsValidPoint(end))
            {
                return;
            }

            RectangleF clipBounds = RectangleF.Inflate(visibleScreenBounds, 2.0f, 2.0f);
            if (!TryClipScreenSegment(start, end, clipBounds, out PointF clippedStart, out PointF clippedEnd) ||
                !IsFarEnough(clippedStart, clippedEnd))
            {
                return;
            }

            surface.DrawLine(clippedStart, clippedEnd, stroke);
        }

        /// <summary>
        /// Clips a finite screen segment to a rectangle using the Liang-Barsky algorithm.
        /// </summary>
        private static bool TryClipScreenSegment(
            PointD start,
            PointD end,
            RectangleF clipBounds,
            out PointF clippedStart,
            out PointF clippedEnd)
        {
            clippedStart = default;
            clippedEnd = default;

            if (clipBounds.Width <= 0.0f || clipBounds.Height <= 0.0f)
            {
                return false;
            }

            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double t0 = 0.0;
            double t1 = 1.0;

            if (!ClipScreenEdge(-dx, start.X - clipBounds.Left, ref t0, ref t1) ||
                !ClipScreenEdge(dx, clipBounds.Right - start.X, ref t0, ref t1) ||
                !ClipScreenEdge(-dy, start.Y - clipBounds.Top, ref t0, ref t1) ||
                !ClipScreenEdge(dy, clipBounds.Bottom - start.Y, ref t0, ref t1))
            {
                return false;
            }

            double clippedStartX = start.X + t0 * dx;
            double clippedStartY = start.Y + t0 * dy;
            double clippedEndX = start.X + t1 * dx;
            double clippedEndY = start.Y + t1 * dy;

            if (!IsValid(clippedStartX) ||
                !IsValid(clippedStartY) ||
                !IsValid(clippedEndX) ||
                !IsValid(clippedEndY))
            {
                return false;
            }

            clippedStart = new PointF((float)clippedStartX, (float)clippedStartY);
            clippedEnd = new PointF((float)clippedEndX, (float)clippedEndY);
            return true;
        }

        private static bool ClipScreenEdge(
            double p,
            double q,
            ref double t0,
            ref double t1)
        {
            if (Math.Abs(p) < double.Epsilon)
            {
                return q >= 0.0;
            }

            double r = q / p;
            if (p < 0.0)
            {
                if (r > t1)
                {
                    return false;
                }

                if (r > t0)
                {
                    t0 = r;
                }
            }
            else
            {
                if (r < t0)
                {
                    return false;
                }

                if (r < t1)
                {
                    t1 = r;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns whether two points are separated by at least one pixel.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>
        /// <see langword="true"/> when points are visually distinct for line drawing; otherwise <see langword="false"/>.
        /// </returns>
        private static bool IsFarEnough(PointD a, PointD b)
        {
            return Math.Abs(a.X - b.X) > 1.0 || Math.Abs(a.Y - b.Y) > 1.0;
        }

        private static bool IsFarEnough(PointF a, PointF b)
        {
            return Math.Abs(a.X - b.X) > 1.0f || Math.Abs(a.Y - b.Y) > 1.0f;
        }

        /// <summary>
        /// Rounds an arbitrary step size to a "nice" grid step using 1-2-5 scaling.
        /// </summary>
        /// <param name="value">Raw step value.</param>
        /// <returns>Nearest normalized step in 1-2-5 sequence.</returns>
        private static double SnapToNiceStep(double value)
        {
            if (value <= 0)
            {
                return 1.0;
            }

            double exp = Math.Floor(Math.Log10(value));
            double pow10 = Math.Pow(10, exp);
            double f = value / pow10;

            double niceF;
            if (f <= 1)
            {
                niceF = 1;
            }
            else if (f <= 2)
            {
                niceF = 2;
            }
            else if (f <= 5)
            {
                niceF = 5;
            }
            else
            {
                niceF = 10;
            }

            return niceF * pow10;
        }

        /// <summary>
        /// Gets the next larger "nice" step from a 1-2-5 progression.
        /// </summary>
        /// <param name="current">Current normalized step.</param>
        /// <returns>Next larger normalized step.</returns>
        private static double NextNiceStep(double current)
        {
            double exp = Math.Floor(Math.Log10(current));
            double pow10 = Math.Pow(10, exp);
            double f = current / pow10;

            if (f < 1)
            {
                return 1 * pow10;
            }

            if (f < 2)
            {
                return 2 * pow10;
            }

            if (f < 5)
            {
                return 5 * pow10;
            }

            return Math.Pow(10, exp + 1);
        }

        /// <summary>
        /// Gets the next smaller "nice" step from a 1-2-5 progression.
        /// </summary>
        /// <param name="current">Current normalized step.</param>
        /// <returns>Next smaller normalized step.</returns>
        private static double PrevNiceStep(double current)
        {
            double exp = Math.Floor(Math.Log10(current));
            double pow10 = Math.Pow(10, exp);
            double f = current / pow10;

            if (f > 5)
            {
                return 5 * pow10;
            }

            if (f > 2)
            {
                return 2 * pow10;
            }

            if (f > 1)
            {
                return pow10;
            }

            return 5 * Math.Pow(10, exp - 1);
        }

        /// <summary>
        /// Determines whether a grid coordinate belongs to a major grid division.
        /// </summary>
        /// <param name="value">Grid coordinate value.</param>
        /// <param name="majorStep">Major-step interval.</param>
        /// <returns><see langword="true"/> when the value lies on a major step; otherwise <see langword="false"/>.</returns>
        private static bool IsMajorLine(double value, double majorStep)
        {
            double k = value / majorStep;
            return Math.Abs(k - Math.Round(k)) < 1e-6;
        }

        /// <summary>
        /// Formats grid labels based on current major-step precision.
        /// </summary>
        /// <param name="value">World coordinate value to display.</param>
        /// <param name="majorStep">Major-step interval used to infer precision.</param>
        /// <returns>Formatted coordinate string for grid labels.</returns>
        private static string FormatGridLabel(double value, double majorStep)
        {
            double step = Math.Abs(majorStep);

            if (step >= 1000)
            {
                return $"{value:F0}";
            }

            if (step >= 1)
            {
                return $"{value:F1}";
            }

            if (step >= 0.01)
            {
                return $"{value:F2}";
            }

            return $"{value:F3}";
        }
    }

    public readonly record struct RasterRenderFrame(
        Bitmap Bitmap,
        RectangleF Destination,
        object SyncRoot,
        Action? Release,
        bool CacheableOnGpu = false) : IDisposable
    {
        public void Dispose()
        {
            Release?.Invoke();
        }
    }

    public readonly record struct GridPanPadding(int Left, int Top, int Right, int Bottom)
    {
        public static GridPanPadding Empty { get; } = new(0, 0, 0, 0);

        public int Horizontal => Left + Right;

        public int Vertical => Top + Bottom;

        public bool IsEmpty => Left <= 0 && Top <= 0 && Right <= 0 && Bottom <= 0;
    }
}
