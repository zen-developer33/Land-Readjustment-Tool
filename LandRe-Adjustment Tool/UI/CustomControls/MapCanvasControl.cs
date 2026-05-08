using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using NtsPolygon = NetTopologySuite.Geometries.Polygon;
using NtsPrecisionModel = NetTopologySuite.Geometries.PrecisionModel;

namespace Land_Readjustment_Tool.UI.CustomControls
{
    public partial class MapCanvasControl : UserControl
    {
        private const int ZoomSettleIntervalMs = 50;
        private const int ObjectSelectionTolerancePixels = 8;
        private const double ScreenPixelsPerMetre = 96.0 / 0.0254;
        private const bool DefaultShowDebugOverlay = false;
        private static readonly double[] StandardScaleDenominators = BuildStandardScaleDenominators();
        private static readonly NtsGeometryFactory SelectionGeometryFactory = new(new NtsPrecisionModel(), 0);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);

        private readonly MapCanvasEngine _engine;
        private readonly MapCanvasRenderer _renderer;
        private readonly RasterDeferredRenderer _rasterDeferredRenderer = new();
        private readonly List<IRasterRenderLayer> _rasterRenderLayers = [];
        private readonly Font _debugOverlayFont = new("Consolas", 8.25f, FontStyle.Regular);
        private MapCanvasRenderSettings _renderSettings;

        public event Action<string, string, double>? StatusChanged;
        public event Action<IShape>? ShapeCompleted;
        public event Action? SelectToolRequested;
        public event Action<IReadOnlyList<Guid>>? SelectedObjectsDeleteRequested;

        private bool _panToolActive;
        private bool _isPanning;
        private bool _isZooming;
        private string? _zoomDirection;
        private bool _zoomWindowActive;
        private bool _isSelectingZoomWindow;
        private Point _lastPanPoint;
        private PointF _totalPanDelta;
        private PointD? _panStartWorld;
        private Point _zoomWindowStart;
        private Point _zoomWindowCurrent;
        private PointD? _currentMouseWorld;
        private bool _zoomingStatusTimerDisposed;
        private CancellationTokenSource? _rasterRenderCancellation;
        private int _rasterRenderGeneration;
        private bool _rasterCacheRefreshPending;
        private volatile bool _liveTileRefreshPending;
        private bool _showDebugOverlay = DefaultShowDebugOverlay;
        private long _debugFrameNumber;
        private double _lastDebugFrameElapsedMs;
        private double _averageDebugFrameElapsedMs;
        private bool _blockPanUntilZoomSettle;
        private bool _holdZoomStartFrameUntilRasterRefresh;
        private bool _suppressStaleRasterFrameUntilFreshRender;
        private MapCanvasTool _activeTool = MapCanvasTool.Select;
        private readonly List<PointD> _drawingVertices = [];
        private List<CanvasFeature> _vectorFeatures = [];
        private List<CanvasLayer> _vectorLayers = [];
        private readonly HashSet<Guid> _selectedShapeIds = [];
        private bool _isSelectingObjects;
        private Point _objectSelectionStart;
        private Point _objectSelectionCurrent;
        private IShape? _previewShape;
        private string _activeDrawingLayerName = "Features";
        private CanvasLayer? _activeDrawingLayer;
        private Cursor? _panCursor;
        private readonly ContextMenuStrip _objectSelectionContextMenu = new();
        private readonly ToolStripMenuItem _mnuDeleteSelectedObjects = new("Delete Selected Object(s)");
        private readonly System.Windows.Forms.Timer _zoomingStatusTimer = new()
        {
            Interval = ZoomSettleIntervalMs
        };

        public MapCanvasControl()
        {
            InitializeComponent();
            ConfigureGraphicsPipeline();
            _engine = new MapCanvasEngine(canvasSurface.Size);
            _renderSettings = MapCanvasRenderSettings.CreateLightDefaults();
            _renderer = new MapCanvasRenderer(_engine, _renderSettings);
            _zoomingStatusTimer.Tick += ZoomingStatusTimer_Tick;
            _mnuDeleteSelectedObjects.Click += (_, _) => RequestDeleteSelectedObjects();
            _objectSelectionContextMenu.Items.Add(_mnuDeleteSelectedObjects);
            WireInteractionEvents();
            UpdateStatusBar();
        }

        /// <summary>
        /// Initializes the canvas with project settings.
        /// Call this after the control is created to apply project-specific canvas colors and grid settings.
        /// </summary>
        public void InitializeWithProjectSettings(Land_Readjustment_Tool.Core.Entities.Project.ProjectSettings projectSettings)
        {
            if (projectSettings != null)
            {
                _renderSettings = MapCanvasRenderSettings.CreateFromProjectSettings(projectSettings);
                ApplyRenderSettings(_renderSettings);
            }
        }

        /// <summary>
        /// Sets anti-flicker and redraw behavior for smooth graphics rendering.
        /// </summary>
        private void ConfigureGraphicsPipeline()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            UpdateStyles();
            DoubleBuffered = true;
            ResizeRedraw = true;
            canvasSurface.TabStop = true;
        }

        private void WireInteractionEvents()
        {
            canvasSurface.MouseEnter += (_, _) => canvasSurface.Focus();
            canvasSurface.MouseWheel += canvasSurface_MouseWheel;
            canvasSurface.MouseDown += canvasSurface_MouseDown;
            canvasSurface.MouseMove += canvasSurface_MouseMove;
            canvasSurface.MouseUp += canvasSurface_MouseUp;
            canvasSurface.MouseLeave += canvasSurface_MouseLeave;
            canvasSurface.KeyDown += canvasSurface_KeyDown;
        }

        /// <summary>
        /// Forces a redraw of the canvas surface.
        /// </summary>
        public void RequestRender()
        {
            if (_engine != null)
            {
                UpdateStatusBar();
            }

            canvasSurface.Invalidate();
        }

        public void ApplyRenderSettings(MapCanvasRenderSettings settings)
        {
            _renderSettings = settings?.Clone() ?? MapCanvasRenderSettings.CreateLightDefaults();
            _renderer.UpdateSettings(_renderSettings);
            BackColor = _renderSettings.BackgroundColor;
            canvasSurface.BackColor = _renderSettings.BackgroundColor;
            RequestRender();
        }

        public void ApplyBackgroundColor(Color color)
        {
            _renderSettings.BackgroundColor = color;
            _renderer.UpdateSettings(_renderSettings);
            BackColor = color;
            canvasSurface.BackColor = color;
            RequestRender();
        }

        public void ApplyGridColor(Color color)
        {
            _renderSettings.MajorGridColor = Color.FromArgb(150, color.R, color.G, color.B);
            _renderSettings.MinorGridColor = Color.FromArgb(70, color.R, color.G, color.B);
            _renderer.UpdateSettings(_renderSettings);
            RequestRender();
        }

        public void ApplyGridVisible(bool visible)
        {
            _renderSettings.ShowGrid = visible;
            _renderer.UpdateSettings(_renderSettings);
            RequestRender();
        }

        public void ApplySnapEnabled(bool enabled)
        {
            // Snapping will be added when geometry editing returns to the new canvas.
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowDebugOverlay
        {
            get => _showDebugOverlay;
            set
            {
                if (_showDebugOverlay == value)
                {
                    return;
                }

                _showDebugOverlay = value;
                RequestRender();
            }
        }

        public void SetVectorLayers(IEnumerable<CanvasLayer>? layers)
        {
            CanvasLayer[] vectorLayers = layers?.ToArray() ?? [];
            _vectorLayers = vectorLayers.ToList();
            _renderer.UpdateVectorLayers(vectorLayers);
            RefreshActiveDrawingLayer(vectorLayers);
            UpdateWorldBounds();
            RefreshVectorCacheForCurrentViewImmediately();
            RequestRender();
        }

        public void UpdateVectorLayer(CanvasLayer layer)
        {
            _renderer.UpdateVectorLayer(layer);
            if (_activeDrawingLayer?.Id == layer.Id ||
                string.Equals(_activeDrawingLayerName, layer.Name, StringComparison.OrdinalIgnoreCase))
            {
                _activeDrawingLayer = layer;
                _activeDrawingLayerName = layer.Name;
            }

            RefreshVectorCacheForCurrentViewImmediately();
            RequestRender();
        }

        public void SetVectorFeatures(IEnumerable<CanvasFeature>? features)
        {
            _vectorFeatures = features?.ToList() ?? [];
            foreach (CanvasFeature feature in _vectorFeatures)
            {
                feature.Shape.IsSelected = _selectedShapeIds.Contains(feature.Shape.Id);
            }

            _renderer.UpdateVectorFeatures(_vectorFeatures);
            UpdateWorldBounds();
            RefreshVectorCacheForCurrentViewImmediately();
            RequestRender();
        }

        public void SetActiveTool(MapCanvasTool tool, string? layerName = null)
        {
            SetActiveTool(tool, null, layerName);
        }

        public void SetActiveTool(MapCanvasTool tool, CanvasLayer? layer)
        {
            SetActiveTool(tool, layer, layer?.Name);
        }

        private void SetActiveTool(
            MapCanvasTool tool,
            CanvasLayer? layer,
            string? layerName)
        {
            _activeTool = tool;
            _panToolActive = false;
            _zoomWindowActive = false;
            _isSelectingZoomWindow = false;
            _isSelectingObjects = false;
            _drawingVertices.Clear();
            _previewShape = null;

            if (layer != null)
            {
                _activeDrawingLayer = layer;
                _activeDrawingLayerName = layer.Name;
            }

            if (!string.IsNullOrWhiteSpace(layerName))
            {
                _activeDrawingLayerName = layerName.Trim();
                if (_activeDrawingLayer != null &&
                    !string.Equals(_activeDrawingLayer.Name, _activeDrawingLayerName, StringComparison.OrdinalIgnoreCase))
                {
                    _activeDrawingLayer = null;
                }
            }

            UpdateCanvasCursor();
            UpdateStatusBar();
            RequestRender();
        }

        private void RefreshActiveDrawingLayer(IReadOnlyList<CanvasLayer> vectorLayers)
        {
            CanvasLayer? activeLayer = _activeDrawingLayer?.Id > 0
                ? vectorLayers.FirstOrDefault(layer => layer.Id == _activeDrawingLayer.Id)
                : null;

            activeLayer ??= vectorLayers.FirstOrDefault(layer =>
                string.Equals(layer.Name, _activeDrawingLayerName, StringComparison.OrdinalIgnoreCase));

            if (activeLayer == null)
            {
                return;
            }

            _activeDrawingLayer = activeLayer;
            _activeDrawingLayerName = activeLayer.Name;
        }

        public void ZoomIn()
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            BeginZoomNavigation("In");
            ZoomAtCanvasCenter(zoomIn: true);
            RequestRender();
            ArmZoomSettleTimer();
        }

        public void ZoomOut()
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            BeginZoomNavigation("Out");
            ZoomAtCanvasCenter(zoomIn: false);
            RequestRender();
            ArmZoomSettleTimer();
        }

        public void ZoomExtents()
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            _engine.ZoomToExtents();
            RefreshRasterCacheForCurrentViewAsync();
            RefreshVectorCacheForCurrentViewImmediately();
            RequestRender();
        }

        public void ZoomToWorldBounds(RectangleD worldBounds)
        {
            if (IsCanvasInteractionLocked ||
                worldBounds.Width <= 0 ||
                worldBounds.Height <= 0)
            {
                return;
            }

            _engine.ZoomToExtents(worldBounds);
            RefreshRasterCacheForCurrentViewAsync();
            RefreshVectorCacheForCurrentViewImmediately();
            RequestRender();
        }

        public MapCanvasViewportState GetViewportState()
        {
            RectangleD visibleBounds = _engine.GetVisibleWorldBounds();
            return new MapCanvasViewportState(
                visibleBounds.X + visibleBounds.Width / 2.0,
                visibleBounds.Y + visibleBounds.Height / 2.0,
                _engine.ZoomScale,
                visibleBounds.Width,
                visibleBounds.Height);
        }

        public RectangleD GetVisibleWorldBounds() =>
            _engine.GetVisibleWorldBounds();

        public bool TryApplyViewportState(MapCanvasViewportState? viewportState)
        {
            if (viewportState == null ||
                !double.IsFinite(viewportState.CenterX) ||
                !double.IsFinite(viewportState.CenterY) ||
                !double.IsFinite(viewportState.ZoomScale) ||
                viewportState.ZoomScale <= 0)
            {
                return false;
            }

            _engine.SetViewport(
                new PointD(viewportState.CenterX, viewportState.CenterY),
                viewportState.ZoomScale);
            RefreshRasterCacheForCurrentViewAsync();
            RefreshVectorCacheForCurrentViewImmediately();
            RequestRender();
            return true;
        }

        public bool ZoomToRasterLayer(int layerId)
        {
            if (IsCanvasInteractionLocked)
            {
                return false;
            }

            IRasterRenderLayer? rasterLayer = _rasterRenderLayers
                .FirstOrDefault(layer => layer.LayerId == layerId);
            if (rasterLayer == null ||
                rasterLayer is XyzLiveTileRenderLayer ||
                !TryNormalizeWorldBounds(rasterLayer.WorldBounds, out RectangleD bounds))
            {
                return false;
            }

            _engine.ZoomToExtents(bounds);
            RefreshRasterCacheForCurrentViewAsync();
            RefreshVectorCacheForCurrentViewImmediately();
            RequestRender();
            return true;
        }

        public void SetPanToolActive(bool active)
        {
            if (active && IsPanBlockedByZoomDebounce)
            {
                return;
            }

            if (IsCanvasInteractionLocked)
            {
                return;
            }

            _panToolActive = active;
            if (active)
            {
                _activeTool = MapCanvasTool.Select;
                _drawingVertices.Clear();
                _previewShape = null;
            }
            _zoomWindowActive = false;
            _isSelectingZoomWindow = false;
            UpdateCanvasCursor();
            UpdateStatusBar();
        }

        public void BeginZoomWindow()
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            _zoomWindowActive = true;
            _panToolActive = false;
            _activeTool = MapCanvasTool.Select;
            _drawingVertices.Clear();
            _previewShape = null;
            _isPanning = false;
            UpdateCanvasCursor();
            UpdateStatusBar();
        }

        public bool IsPanToolActive => _panToolActive;

        public void SetRasterLayers(
            IEnumerable<CanvasLayer>? rasterLayers,
            string? projectFolderPath,
            string? projectSrsDefinition = null)
        {
            DisposeRasterRenderLayers();

            if (rasterLayers != null)
            {
                // Shared callback used by live tile layers to trigger a repaint when a
                // tile finishes loading on a background thread.
                Action liveTileCallback = () =>
                {
                    if (!IsDisposed && IsHandleCreated && !_liveTileRefreshPending)
                    {
                        _liveTileRefreshPending = true;
                        BeginInvoke((MethodInvoker)(() =>
                        {
                            _liveTileRefreshPending = false;
                            if (!IsInteractiveNavigation)
                            {
                                RefreshRasterCacheForCurrentViewAsync();
                            }
                            else if (_isZooming)
                            {
                                ArmZoomSettleTimer();
                            }

                            RequestRender();
                        }));
                    }
                };

                foreach (CanvasLayer rasterLayer in rasterLayers
                    .OrderBy(layer => IsLiveOnlineBasemap(layer, projectFolderPath) ? 0 : 1)
                    .ThenBy(layer => layer.DisplayOrder)
                    .ThenBy(layer => layer.Name))
                {
                    try
                    {
                        _rasterRenderLayers.Add(
                            RasterRenderLayerFactory.FromCanvasLayer(
                                rasterLayer,
                                projectFolderPath,
                                liveTileCallback,
                                projectSrsDefinition));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Raster layer skipped: {rasterLayer.Name}. {ex.Message}");
                    }
                }
            }

            UpdateRasterWorldBounds();
            _renderer.UpdateRasterLayers(_rasterRenderLayers);
            RefreshRasterCacheForCurrentViewAsync();
            RequestRender();
        }

        /// <summary>
        /// Recreates every raster render layer after project CRS/datum changes.
        /// Live XYZ and MBTiles renderers cache coordinate transformations at
        /// construction time, so CRS changes must rebuild the renderer objects
        /// from the refreshed project layer records.
        /// </summary>
        public void RebuildRasterLayersAfterCrsChange(
            IEnumerable<CanvasLayer>? rasterLayers,
            string? projectFolderPath,
            string? projectSrsDefinition = null)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            StopZoomInteraction();
            CancelActiveCanvasGesture();
            CancelPendingRasterRender();
            _rasterCacheRefreshPending = false;
            _liveTileRefreshPending = false;
            _suppressStaleRasterFrameUntilFreshRender = false;
            _rasterDeferredRenderer.Invalidate();

            SetRasterLayers(
                rasterLayers,
                projectFolderPath,
                projectSrsDefinition);
        }

        private static bool IsLiveOnlineBasemap(
            CanvasLayer layer,
            string? projectFolderPath)
        {
            if (string.IsNullOrWhiteSpace(layer.SourceFile))
            {
                return false;
            }

            string sourcePath = Path.IsPathRooted(layer.SourceFile)
                ? Path.GetFullPath(layer.SourceFile)
                : Path.GetFullPath(Path.Combine(projectFolderPath ?? string.Empty, layer.SourceFile));

            if (File.Exists(sourcePath) &&
                XyzLiveTileRenderLayer.IsLiveTileVrtPath(sourcePath))
            {
                return true;
            }

            return layer.SourceFile.EndsWith(
                       ".vrt",
                       StringComparison.OrdinalIgnoreCase) &&
                   layer.Description != null &&
                   (layer.Description.Contains(
                        "internet",
                        StringComparison.OrdinalIgnoreCase) ||
                    layer.Description.Contains(
                        "lazy VRT",
                        StringComparison.OrdinalIgnoreCase) ||
                    layer.Description.Contains(
                        "GDAL_WMS",
                        StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Updates one raster layer visibility without rebuilding the full canvas layer stack.
        /// </summary>
        public bool SetRasterLayerVisibility(int layerId, bool isVisible)
        {
            IRasterRenderLayer? rasterLayer = _rasterRenderLayers
                .FirstOrDefault(layer => layer.LayerId == layerId);

            if (rasterLayer == null)
                return false;

            CancelPendingRasterRender();
            _rasterRenderGeneration++;
            rasterLayer.UpdateRenderState(isVisible, rasterLayer.Transparency);
            if (!_rasterDeferredRenderer.TryRecomposeFromLayerCaches(
                    _rasterRenderLayers))
            {
                _rasterDeferredRenderer.Invalidate();
                RefreshRasterCacheForCurrentViewAsync();
            }

            RequestRender();
            return true;
        }

        /// <summary>
        /// Updates one raster layer's lightweight render state without rebuilding raster datasets.
        /// </summary>
        public bool SetRasterLayerRenderState(
            int layerId,
            bool isVisible,
            int transparency)
        {
            IRasterRenderLayer? rasterLayer = _rasterRenderLayers
                .FirstOrDefault(layer => layer.LayerId == layerId);

            if (rasterLayer == null)
                return false;

            bool needsLayerRerender =
                isVisible && rasterLayer.Transparency != transparency;

            CancelPendingRasterRender();
            _rasterRenderGeneration++;
            rasterLayer.UpdateRenderState(isVisible, transparency);
            if (!needsLayerRerender &&
                _rasterDeferredRenderer.TryRecomposeFromLayerCaches(
                    _rasterRenderLayers))
            {
                RequestRender();
                return true;
            }

            _rasterDeferredRenderer.Invalidate();
            RefreshRasterCacheForCurrentViewAsync();
            RequestRender();
            return true;
        }

        /// <summary>
        /// Releases currently opened raster datasets before project raster files are rewritten.
        /// </summary>
        public void ClearRasterLayers()
        {
            DisposeRasterRenderLayers();
            UpdateRasterWorldBounds();
            RequestRender();
        }

        private void canvasSurface_Resize(object? sender, EventArgs e)
        {
            if (_engine == null)
            {
                return;
            }

            _engine.UpdateCanvasSize(canvasSurface.Size);
            _rasterDeferredRenderer.Resize(canvasSurface.Size);
            _renderer.ResizeVectorCache(canvasSurface.Size);
            RefreshRasterCacheForCurrentViewAsync();
            RefreshVectorCacheForCurrentViewImmediately();
            RequestRender();
        }

        private void canvasSurface_Paint(object? sender, PaintEventArgs e)
        {
            Stopwatch frameStopwatch = Stopwatch.StartNew();
            RasterRenderFrame? rasterFrame = GetRasterRenderFrame(out CanvasFrameSource rasterFrameSource);
            RasterRenderFrame? vectorFrame = GetVectorRenderFrame(out CanvasFrameSource vectorFrameSource);

            if (rasterFrameSource == CanvasFrameSource.None &&
                !ShouldDeferDirectRasterRendering)
            {
                rasterFrameSource = CanvasFrameSource.Direct;
            }

            if (vectorFrameSource == CanvasFrameSource.None &&
                !ShouldDeferDirectVectorRendering)
            {
                vectorFrameSource = CanvasFrameSource.Direct;
            }

            _renderer.Render(
                e.Graphics,
                rasterFrame,
                ShouldDeferDirectRasterRendering,
                vectorFrame,
                ShouldDeferDirectVectorRendering,
                GetZoomWindowRectangle(),
                _previewShape,
                _activeDrawingLayer,
                _showDebugOverlay);

            DrawObjectSelectionRectangle(e.Graphics);
            frameStopwatch.Stop();
            UpdateDebugFrameTiming(frameStopwatch.Elapsed.TotalMilliseconds);
            DrawDebugOverlayIfNeeded(e.Graphics, rasterFrameSource, vectorFrameSource);
        }

        private void canvasSurface_MouseWheel(object? sender, MouseEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? MapCanvasEngine.ZoomStep : 1.0 / MapCanvasEngine.ZoomStep;
            BeginZoomNavigation(e.Delta > 0 ? "In" : "Out");

            ZoomAtPoint(e.Location, e.Delta > 0, zoomFactor);
            _currentMouseWorld = _engine.ScreenToWorld(e.Location);
            RequestRender();
            
            // Redraw the precise raster shortly after the last wheel event.
            ArmZoomSettleTimer();
        }

        private void ZoomAtCanvasCenter(bool zoomIn)
        {
            Point center = new(canvasSurface.Width / 2, canvasSurface.Height / 2);
            double zoomFactor = zoomIn ? MapCanvasEngine.ZoomStep : 1.0 / MapCanvasEngine.ZoomStep;
            ZoomAtPoint(center, zoomIn, zoomFactor);
        }

        private void ZoomAtPoint(Point screenPoint, bool zoomIn, double normalZoomFactor)
        {
            if (_renderSettings.ZoomBehavior != MapCanvasZoomBehavior.StandardScaleSteps)
            {
                _engine.ZoomAtPoint(screenPoint, normalZoomFactor);
                _renderer.InvalidateVectorCache();
                return;
            }

            double targetZoomScale = GetNextStandardZoomScale(_engine.ZoomScale, zoomIn);
            _engine.ZoomAtPointToScale(screenPoint, targetZoomScale);
            _renderer.InvalidateVectorCache();
        }

        private static double GetNextStandardZoomScale(double currentZoomScale, bool zoomIn)
        {
            if (currentZoomScale <= 0 || !double.IsFinite(currentZoomScale))
            {
                return currentZoomScale;
            }

            double currentDenominator = ScreenPixelsPerMetre / currentZoomScale;
            const double tolerance = 0.0000001;

            double? targetDenominator = null;

            if (zoomIn)
            {
                for (int i = StandardScaleDenominators.Length - 1; i >= 0; i--)
                {
                    if (StandardScaleDenominators[i] < currentDenominator * (1.0 - tolerance))
                    {
                        targetDenominator = StandardScaleDenominators[i];
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < StandardScaleDenominators.Length; i++)
                {
                    if (StandardScaleDenominators[i] > currentDenominator * (1.0 + tolerance))
                    {
                        targetDenominator = StandardScaleDenominators[i];
                        break;
                    }
                }
            }

            if (!targetDenominator.HasValue)
            {
                return currentZoomScale * (zoomIn ? MapCanvasEngine.ZoomStep : 1.0 / MapCanvasEngine.ZoomStep);
            }

            return ScreenPixelsPerMetre / targetDenominator.Value;
        }

        private static double[] BuildStandardScaleDenominators()
        {
            double[] baseSteps = [1.0, 1.25, 2.0, 2.5, 4.0, 5.0, 7.5];
            List<double> values = [];

            for (int exponent = -6; exponent <= 12; exponent++)
            {
                double magnitude = Math.Pow(10.0, exponent);

                foreach (double step in baseSteps)
                {
                    values.Add(step * magnitude);
                }
            }

            values.Sort();
            return values.ToArray();
        }

        private void canvasSurface_MouseDown(object? sender, MouseEventArgs e)
        {
            canvasSurface.Focus();

            bool isPanGesture =
                e.Button == MouseButtons.Middle ||
                (_panToolActive && e.Button == MouseButtons.Left);

            if (isPanGesture && IsPanBlockedByZoomDebounce)
            {
                return;
            }

            if (IsCanvasInteractionLocked)
            {
                return;
            }

            if (_activeTool != MapCanvasTool.Select &&
                e.Button is MouseButtons.Left or MouseButtons.Right)
            {
                HandleDrawingMouseDown(e);
                return;
            }

            if (_zoomWindowActive && e.Button == MouseButtons.Left)
            {
                _isSelectingZoomWindow = true;
                _zoomWindowStart = e.Location;
                _zoomWindowCurrent = e.Location;
                RequestRender();
                return;
            }

            if (_activeTool == MapCanvasTool.Select &&
                e.Button == MouseButtons.Right &&
                _selectedShapeIds.Count > 0)
            {
                _objectSelectionContextMenu.Show(canvasSurface, e.Location);
                return;
            }

            if (isPanGesture)
            {
                CancelPendingRasterRender();
                CancelLiveTileLoading();
                SetLiveTileInternetFetchingSuspended(true);
                _isPanning = true;
                _holdZoomStartFrameUntilRasterRefresh = false;
                _lastPanPoint = e.Location;
                _totalPanDelta = PointF.Empty;
                _panStartWorld = _engine.ScreenToWorld(e.Location);
                _currentMouseWorld = _panStartWorld;
                _rasterDeferredRenderer.BeginPan(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine);
                RefreshVectorCacheForCurrentViewImmediately();
                _renderer.BeginVectorPan(canvasSurface.Size);
                canvasSurface.Capture = true;
                UpdateCanvasCursor();
                UpdateStatusBar();
                return;
            }

            if (_activeTool == MapCanvasTool.Select && e.Button == MouseButtons.Left)
            {
                _isSelectingObjects = true;
                _objectSelectionStart = e.Location;
                _objectSelectionCurrent = e.Location;
                canvasSurface.Capture = true;
                RequestRender();
            }
        }

        private void canvasSurface_MouseMove(object? sender, MouseEventArgs e)
        {
            if (IsCanvasInteractionLocked)
            {
                _currentMouseWorld = _engine.ScreenToWorld(e.Location);
                return;
            }

            if (_isPanning && IsPanBlockedByZoomDebounce)
            {
                _isPanning = false;
                canvasSurface.Capture = false;
                _panStartWorld = null;
                _totalPanDelta = PointF.Empty;
                UpdateCanvasCursor();
                RequestRender();
                return;
            }
            
            if (_isSelectingZoomWindow)
            {
                _currentMouseWorld = _engine.ScreenToWorld(e.Location);
                _zoomWindowCurrent = e.Location;
                RequestRender();
                return;
            }

            if (_isPanning)
            {
                // Freeze cursor world coordinates while panning to avoid jitter.
                // During drag the cursor shows the world coordinate captured at pan start.
                _currentMouseWorld = _panStartWorld;
                int dx = e.X - _lastPanPoint.X;
                int dy = e.Y - _lastPanPoint.Y;
                _totalPanDelta = new PointF(
                    _totalPanDelta.X + dx,
                    _totalPanDelta.Y + dy);
                _engine.PanByScreenDelta(dx, dy);
                _lastPanPoint = e.Location;
                RequestRender();
                return;
            }

            if (_isSelectingObjects)
            {
                _currentMouseWorld = _engine.ScreenToWorld(e.Location);
                _objectSelectionCurrent = e.Location;
                RequestRender();
                return;
            }

            _currentMouseWorld = _engine.ScreenToWorld(e.Location);
            UpdateDrawingPreview(e.Location);
            UpdateStatusBar();
        }

        private void canvasSurface_MouseUp(object? sender, MouseEventArgs e)
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            if (_isSelectingZoomWindow)
            {
                Rectangle? rectangle = GetZoomWindowRectangle();
                _isSelectingZoomWindow = false;
                _zoomWindowActive = false;

                if (rectangle.HasValue && rectangle.Value.Width > 8 && rectangle.Value.Height > 8)
                {
                    ZoomToScreenRectangle(rectangle.Value);
                    RefreshRasterCacheForCurrentViewAsync();
                    RefreshVectorCacheForCurrentViewImmediately();
                }

                UpdateCanvasCursor();
                RequestRender();
                return;
            }

            if (_isPanning)
            {
                _isPanning = false;
                canvasSurface.Capture = false;
                _currentMouseWorld = _engine.ScreenToWorld(e.Location);
                _panStartWorld = null;
                _totalPanDelta = PointF.Empty;
                SetLiveTileInternetFetchingSuspended(false);
                _suppressStaleRasterFrameUntilFreshRender = true;
                RefreshRasterCacheForCurrentViewAsync();
                RefreshVectorCacheForCurrentViewImmediately();
                UpdateCanvasCursor();
                RequestRender();
                return;
            }

            if (_isSelectingObjects && e.Button == MouseButtons.Left)
            {
                _isSelectingObjects = false;
                canvasSurface.Capture = false;
                _objectSelectionCurrent = e.Location;
                bool additiveSelection = (ModifierKeys & Keys.Control) == Keys.Control;
                ApplyObjectSelectionFromMouseUp(e.Location, additiveSelection);
                RequestRender();
            }
        }

        private void canvasSurface_MouseLeave(object? sender, EventArgs e)
        {
            if (!_isPanning && !_isSelectingZoomWindow)
            {
                UpdateStatusBar();
            }
        }

        private void canvasSurface_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _selectedShapeIds.Count > 0)
            {
                RequestDeleteSelectedObjects();
                e.Handled = true;
                return;
            }

            if (e.KeyCode != Keys.Escape)
            {
                return;
            }

            CancelDrawing();
            _isSelectingObjects = false;
            canvasSurface.Capture = false;
            ClearSelectedObjects();
            if (_activeTool != MapCanvasTool.Select)
            {
                SelectToolRequested?.Invoke();
            }

            e.Handled = true;
        }

        private void HandleDrawingMouseDown(MouseEventArgs e)
        {
            PointD worldPoint = _engine.ScreenToWorld(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                CompleteMultiPointDrawing();
                return;
            }

            switch (_activeTool)
            {
                case MapCanvasTool.Point:
                    PolylineShape pointShape = new([worldPoint], isClosed: false);
                    pointShape.Properties["ObjectType"] = "Point";
                    CompleteShape(pointShape);
                    break;
                case MapCanvasTool.Line:
                case MapCanvasTool.Rectangle:
                case MapCanvasTool.Circle:
                    HandleTwoPointDrawing(worldPoint);
                    break;
                case MapCanvasTool.Polyline:
                case MapCanvasTool.Polygon:
                    _drawingVertices.Add(worldPoint);
                    UpdateDrawingPreview(e.Location);
                    break;
            }
        }

        private void HandleTwoPointDrawing(PointD worldPoint)
        {
            if (_drawingVertices.Count == 0)
            {
                _drawingVertices.Add(worldPoint);
                _previewShape = null;
                RequestRender();
                return;
            }

            IShape? completedShape = _activeTool switch
            {
                MapCanvasTool.Line => new LineShape(_drawingVertices[0], worldPoint),
                MapCanvasTool.Rectangle => new RectangleShape(_drawingVertices[0], worldPoint),
                MapCanvasTool.Circle => new CircleShape(_drawingVertices[0], worldPoint),
                _ => null
            };

            CompleteShape(completedShape);
        }

        private void CompleteMultiPointDrawing()
        {
            int minimumVertices = _activeTool == MapCanvasTool.Polygon ? 3 : 2;
            if (_drawingVertices.Count < minimumVertices)
            {
                CancelDrawing();
                return;
            }

            CompleteShape(new PolylineShape(
                _drawingVertices.ToArray(),
                _activeTool == MapCanvasTool.Polygon));
        }

        private void UpdateDrawingPreview(Point screenPoint)
        {
            if (_activeTool == MapCanvasTool.Select ||
                _drawingVertices.Count == 0)
            {
                return;
            }

            PointD worldPoint = _engine.ScreenToWorld(screenPoint);
            _previewShape = _activeTool switch
            {
                MapCanvasTool.Line => new LineShape(_drawingVertices[0], worldPoint),
                MapCanvasTool.Rectangle => new RectangleShape(_drawingVertices[0], worldPoint),
                MapCanvasTool.Circle => new CircleShape(_drawingVertices[0], worldPoint),
                MapCanvasTool.Polyline => new PolylineShape(
                    _drawingVertices.Concat([worldPoint]),
                    isClosed: false),
                MapCanvasTool.Polygon => new PolylineShape(
                    _drawingVertices.Concat([worldPoint]),
                    isClosed: true),
                _ => null
            };

            if (_previewShape != null)
            {
                _previewShape.LayerName = _activeDrawingLayerName;
            }

            RequestRender();
        }

        private void CompleteShape(IShape? shape)
        {
            if (shape == null)
            {
                CancelDrawing();
                return;
            }

            shape.LayerName = _activeDrawingLayerName;
            _drawingVertices.Clear();
            _previewShape = null;
            ShapeCompleted?.Invoke(shape);
            UpdateStatusBar();
            RequestRender();
        }

        private void CancelDrawing()
        {
            _drawingVertices.Clear();
            _previewShape = null;
            UpdateStatusBar();
            RequestRender();
        }

        private void ApplyObjectSelectionFromMouseUp(Point mouseUpLocation, bool additiveSelection)
        {
            Rectangle selectionRectangle = CreateScreenRectangle(_objectSelectionStart, mouseUpLocation);
            if (selectionRectangle.Width <= 4 && selectionRectangle.Height <= 4)
            {
                SelectObjectByClick(mouseUpLocation, additiveSelection);
                return;
            }

            bool isWindowSelection = mouseUpLocation.X >= _objectSelectionStart.X;
            RectangleD worldRectangle = CreateWorldRectangle(selectionRectangle);
            List<CanvasFeature> selectedFeatures = _vectorFeatures
                .Where(IsSelectableDrawingFeature)
                .Where(feature => isWindowSelection
                    ? ContainsSelectionGeometry(worldRectangle, feature.Shape)
                    : IntersectsSelectionRectangle(worldRectangle, feature.Shape))
                .ToList();

            if (additiveSelection)
            {
                AddSelectedObjects(selectedFeatures);
            }
            else
            {
                ReplaceSelectedObjects(selectedFeatures);
            }
        }

        private void SelectObjectByClick(Point screenPoint, bool additiveSelection)
        {
            PointD worldPoint = _engine.ScreenToWorld(new PointD(screenPoint.X, screenPoint.Y));
            double worldTolerance = _engine.ScreenToWorldDistance(ObjectSelectionTolerancePixels);

            CanvasFeature? hitFeature = _vectorFeatures
                .Where(IsSelectableDrawingFeature)
                .Reverse()
                .FirstOrDefault(feature => IsScreenPointNearShape(
                    feature.Shape,
                    worldPoint,
                    worldTolerance));

            if (!additiveSelection)
            {
                ReplaceSelectedObjects(hitFeature == null ? [] : [hitFeature]);
                return;
            }

            if (hitFeature == null)
            {
                return;
            }

            if (!_selectedShapeIds.Add(hitFeature.Shape.Id))
            {
                _selectedShapeIds.Remove(hitFeature.Shape.Id);
            }

            ApplySelectedShapeFlags();
            RefreshVectorCacheForCurrentViewImmediately();
            UpdateStatusBar();
        }

        private void ReplaceSelectedObjects(IEnumerable<CanvasFeature> selectedFeatures)
        {
            _selectedShapeIds.Clear();
            foreach (CanvasFeature feature in selectedFeatures)
            {
                _selectedShapeIds.Add(feature.Shape.Id);
            }

            ApplySelectedShapeFlags();
            RefreshVectorCacheForCurrentViewImmediately();
            UpdateStatusBar();
        }

        private void AddSelectedObjects(IEnumerable<CanvasFeature> selectedFeatures)
        {
            bool changed = false;
            foreach (CanvasFeature feature in selectedFeatures)
            {
                changed |= _selectedShapeIds.Add(feature.Shape.Id);
            }

            if (!changed)
            {
                return;
            }

            ApplySelectedShapeFlags();
            RefreshVectorCacheForCurrentViewImmediately();
            UpdateStatusBar();
        }

        private void ClearSelectedObjects()
        {
            if (_selectedShapeIds.Count == 0)
                return;

            _selectedShapeIds.Clear();
            ApplySelectedShapeFlags();
            RefreshVectorCacheForCurrentViewImmediately();
        }

        private void ApplySelectedShapeFlags()
        {
            foreach (CanvasFeature feature in _vectorFeatures)
            {
                feature.Shape.IsSelected = _selectedShapeIds.Contains(feature.Shape.Id);
            }
        }

        private void RequestDeleteSelectedObjects()
        {
            if (_selectedShapeIds.Count == 0)
                return;

            SelectedObjectsDeleteRequested?.Invoke(_selectedShapeIds.ToArray());
        }

        public void ClearSelectionAfterDelete()
        {
            _selectedShapeIds.Clear();
            ApplySelectedShapeFlags();
            RequestRender();
        }

        private bool IsSelectableDrawingFeature(CanvasFeature feature)
        {
            return feature.Shape.IsVisible &&
                   feature.Layer?.IsVisible != false &&
                   feature.Layer?.IsLocked != true &&
                   feature.Layer != null &&
                   CanvasLayerTreeService.IsDrawingMarkupLayer(feature.Layer);
        }

        private RectangleD CreateWorldRectangle(Rectangle screenRectangle)
        {
            PointD first = _engine.ScreenToWorld(new Point(screenRectangle.Left, screenRectangle.Top));
            PointD second = _engine.ScreenToWorld(new Point(screenRectangle.Right, screenRectangle.Bottom));
            double left = Math.Min(first.X, second.X);
            double right = Math.Max(first.X, second.X);
            double bottom = Math.Min(first.Y, second.Y);
            double top = Math.Max(first.Y, second.Y);
            return new RectangleD(left, bottom, right - left, top - bottom);
        }

        private static bool IntersectsSelectionRectangle(RectangleD selectionBounds, IShape shape)
        {
            NtsGeometry shapeGeometry = CreateSelectionGeometry(shape);
            if (shapeGeometry.IsEmpty)
            {
                return false;
            }

            NtsPolygon selectionPolygon = SelectionGeometryFactory.CreatePolygon(
                [
                    new NtsCoordinate(selectionBounds.Left, selectionBounds.Top),
                    new NtsCoordinate(selectionBounds.Right, selectionBounds.Top),
                    new NtsCoordinate(selectionBounds.Right, selectionBounds.Bottom),
                    new NtsCoordinate(selectionBounds.Left, selectionBounds.Bottom),
                    new NtsCoordinate(selectionBounds.Left, selectionBounds.Top)
                ]);

            return shapeGeometry.Intersects(selectionPolygon);
        }

        private static bool ContainsSelectionGeometry(RectangleD selectionBounds, IShape shape)
        {
            NtsGeometry shapeGeometry = CreateSelectionGeometry(shape);
            if (shapeGeometry.IsEmpty)
            {
                return false;
            }

            NtsPolygon selectionPolygon = SelectionGeometryFactory.CreatePolygon(
                [
                    new NtsCoordinate(selectionBounds.Left, selectionBounds.Top),
                    new NtsCoordinate(selectionBounds.Right, selectionBounds.Top),
                    new NtsCoordinate(selectionBounds.Right, selectionBounds.Bottom),
                    new NtsCoordinate(selectionBounds.Left, selectionBounds.Bottom),
                    new NtsCoordinate(selectionBounds.Left, selectionBounds.Top)
                ]);

            return selectionPolygon.Covers(shapeGeometry);
        }

        private static NtsGeometry CreateSelectionGeometry(IShape shape)
        {
            return shape switch
            {
                LineShape line => SelectionGeometryFactory.CreateLineString(
                    [
                        new NtsCoordinate(line.Start.X, line.Start.Y),
                        new NtsCoordinate(line.End.X, line.End.Y)
                    ]),
                PolylineShape polyline => CreateSelectionGeometryFromPolyline(polyline),
                RectangleShape rectangle => CreateSelectionPolygonFromRectangle(rectangle.Start, rectangle.End),
                CircleShape circle => SelectionGeometryFactory.CreatePoint(
                    new NtsCoordinate(circle.Center.X, circle.Center.Y)).Buffer(circle.GetRadius(), quadrantSegments: 24),
                EllipseShape ellipse => CreateSelectionPolygonFromEllipse(ellipse.Start, ellipse.End),
                TextShape text => SelectionGeometryFactory.CreatePoint(
                    new NtsCoordinate(text.Position.X, text.Position.Y)),
                _ => CreateSelectionPolygonFromBounds(shape.GetBoundingBox())
            };
        }

        private static NtsGeometry CreateSelectionGeometryFromPolyline(PolylineShape polyline)
        {
            if (polyline.Vertices.Count == 0)
            {
                return SelectionGeometryFactory.CreateGeometryCollection();
            }

            if (polyline.Vertices.Count == 1)
            {
                PointD vertex = polyline.Vertices[0];
                return SelectionGeometryFactory.CreatePoint(new NtsCoordinate(vertex.X, vertex.Y));
            }

            NtsCoordinate[] coordinates = polyline.Vertices
                .Select(vertex => new NtsCoordinate(vertex.X, vertex.Y))
                .ToArray();

            if (polyline.IsClosed && coordinates.Length >= 3)
            {
                return SelectionGeometryFactory.CreatePolygon(CloseRing(coordinates));
            }

            return SelectionGeometryFactory.CreateLineString(coordinates);
        }

        private static NtsPolygon CreateSelectionPolygonFromRectangle(PointD start, PointD end)
        {
            return SelectionGeometryFactory.CreatePolygon([
                new NtsCoordinate(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y)),
                new NtsCoordinate(Math.Max(start.X, end.X), Math.Min(start.Y, end.Y)),
                new NtsCoordinate(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y)),
                new NtsCoordinate(Math.Min(start.X, end.X), Math.Max(start.Y, end.Y)),
                new NtsCoordinate(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y))
            ]);
        }

        private static NtsPolygon CreateSelectionPolygonFromEllipse(PointD start, PointD end)
        {
            double minX = Math.Min(start.X, end.X);
            double minY = Math.Min(start.Y, end.Y);
            double maxX = Math.Max(start.X, end.X);
            double maxY = Math.Max(start.Y, end.Y);
            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;
            double radiusX = (maxX - minX) / 2.0;
            double radiusY = (maxY - minY) / 2.0;

            const int segments = 72;
            NtsCoordinate[] ring = new NtsCoordinate[segments + 1];

            for (int i = 0; i < segments; i++)
            {
                double theta = 2.0 * Math.PI * i / segments;
                ring[i] = new NtsCoordinate(
                    centerX + radiusX * Math.Cos(theta),
                    centerY + radiusY * Math.Sin(theta));
            }

            ring[segments] = new NtsCoordinate(ring[0].X, ring[0].Y);
            return SelectionGeometryFactory.CreatePolygon(ring);
        }

        private static NtsPolygon CreateSelectionPolygonFromBounds(RectangleD bounds)
        {
            return SelectionGeometryFactory.CreatePolygon([
                new NtsCoordinate(bounds.Left, bounds.Top),
                new NtsCoordinate(bounds.Right, bounds.Top),
                new NtsCoordinate(bounds.Right, bounds.Bottom),
                new NtsCoordinate(bounds.Left, bounds.Bottom),
                new NtsCoordinate(bounds.Left, bounds.Top)
            ]);
        }

        private static NtsCoordinate[] CloseRing(NtsCoordinate[] coordinates)
        {
            if (coordinates.Length == 0)
            {
                return coordinates;
            }

            if (coordinates[0].Equals2D(coordinates[^1]))
            {
                return coordinates;
            }

            NtsCoordinate[] closed = new NtsCoordinate[coordinates.Length + 1];
            Array.Copy(coordinates, closed, coordinates.Length);
            closed[^1] = new NtsCoordinate(coordinates[0].X, coordinates[0].Y);
            return closed;
        }

        private bool IsScreenPointNearShape(
            IShape shape,
            PointD worldPoint,
            double toleranceWorld)
        {
            NtsGeometry shapeGeometry = CreateSelectionGeometry(shape);
            if (shapeGeometry.IsEmpty)
            {
                return false;
            }

            NtsGeometry hitArea = SelectionGeometryFactory.CreatePoint(
                new NtsCoordinate(worldPoint.X, worldPoint.Y)).Buffer(toleranceWorld, quadrantSegments: 16);
            return shapeGeometry.Intersects(hitArea);
        }

        private GraphicsPath? CreateScreenOutlinePath(IShape shape)
        {
            GraphicsPath path = new();
            try
            {
                switch (shape)
                {
                    case LineShape line:
                        path.AddLine(ToScreenPointF(line.Start), ToScreenPointF(line.End));
                        break;

                    case PolylineShape polyline when polyline.Vertices.Count >= 2:
                        PointF[] points = polyline.Vertices
                            .Select(ToScreenPointF)
                            .ToArray();
                        path.AddLines(points);
                        if (polyline.IsClosed && points.Length > 2)
                        {
                            path.CloseFigure();
                        }
                        break;

                    case RectangleShape rectangle:
                        path.AddRectangle(CreateScreenRectangle(
                            ToScreenPointF(rectangle.Start),
                            ToScreenPointF(rectangle.End)));
                        break;

                    case CircleShape circle:
                        PointF center = ToScreenPointF(circle.Center);
                        PointF edge = ToScreenPointF(circle.RadiusPoint);
                        float radius = Distance(center, edge);
                        if (radius <= 0)
                        {
                            path.Dispose();
                            return null;
                        }

                        path.AddEllipse(
                            center.X - radius,
                            center.Y - radius,
                            radius * 2.0f,
                            radius * 2.0f);
                        break;

                    case EllipseShape ellipse:
                        path.AddEllipse(CreateScreenRectangle(
                            ToScreenPointF(ellipse.Start),
                            ToScreenPointF(ellipse.End)));
                        break;

                    default:
                        path.Dispose();
                        return null;
                }

                if (path.PointCount == 0)
                {
                    path.Dispose();
                    return null;
                }

                return path;
            }
            catch
            {
                path.Dispose();
                return null;
            }
        }

        private Point ToScreenPoint(PointD worldPoint)
        {
            PointD screenPoint = _engine.WorldToScreen(worldPoint);
            return new Point(
                (int)Math.Round(screenPoint.X),
                (int)Math.Round(screenPoint.Y));
        }

        private PointF ToScreenPointF(PointD worldPoint)
        {
            PointD screenPoint = _engine.WorldToScreen(worldPoint);
            return new PointF(
                (float)screenPoint.X,
                (float)screenPoint.Y);
        }

        private static RectangleF CreateScreenRectangle(PointF first, PointF second)
        {
            float left = Math.Min(first.X, second.X);
            float top = Math.Min(first.Y, second.Y);
            float right = Math.Max(first.X, second.X);
            float bottom = Math.Max(first.Y, second.Y);
            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        private static double Distance(Point first, Point second)
        {
            double dx = first.X - second.X;
            double dy = first.Y - second.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static float Distance(PointF first, PointF second)
        {
            float dx = first.X - second.X;
            float dy = first.Y - second.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        private static Rectangle CreateScreenRectangle(Point first, Point second)
        {
            int left = Math.Min(first.X, second.X);
            int top = Math.Min(first.Y, second.Y);
            int right = Math.Max(first.X, second.X);
            int bottom = Math.Max(first.Y, second.Y);
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private void DrawObjectSelectionRectangle(Graphics graphics)
        {
            if (!_isSelectingObjects)
                return;

            Rectangle rect = CreateScreenRectangle(_objectSelectionStart, _objectSelectionCurrent);
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            bool isWindowSelection = _objectSelectionCurrent.X >= _objectSelectionStart.X;
            Color borderColor = Color.FromArgb(0, 122, 204);
            Color fillColor = isWindowSelection
                ? Color.FromArgb(36, 0, 122, 204)
                : Color.FromArgb(32, 0, 122, 204);

            using SolidBrush fillBrush = new(fillColor);
            using Pen borderPen = new(borderColor, 1.4f);
            if (!isWindowSelection)
            {
                borderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            }

            graphics.FillRectangle(fillBrush, rect);
            graphics.DrawRectangle(borderPen, rect);
        }

        private Rectangle? GetZoomWindowRectangle()
        {
            if (!_isSelectingZoomWindow)
            {
                return null;
            }

            int left = Math.Min(_zoomWindowStart.X, _zoomWindowCurrent.X);
            int top = Math.Min(_zoomWindowStart.Y, _zoomWindowCurrent.Y);
            int right = Math.Max(_zoomWindowStart.X, _zoomWindowCurrent.X);
            int bottom = Math.Max(_zoomWindowStart.Y, _zoomWindowCurrent.Y);
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private void ZoomToScreenRectangle(Rectangle screenRectangle)
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            PointD topLeft = _engine.ScreenToWorld(new Point(screenRectangle.Left, screenRectangle.Top));
            PointD bottomRight = _engine.ScreenToWorld(new Point(screenRectangle.Right, screenRectangle.Bottom));

            double left = Math.Min(topLeft.X, bottomRight.X);
            double right = Math.Max(topLeft.X, bottomRight.X);
            double bottom = Math.Min(topLeft.Y, bottomRight.Y);
            double top = Math.Max(topLeft.Y, bottomRight.Y);

            _engine.ZoomToExtents(new RectangleD(left, bottom, right - left, top - bottom), padding: 1.0);
        }

        private void UpdateCanvasCursor()
        {
            if (_isPanning || _panToolActive)
            {
                canvasSurface.Cursor = GetPanCursor();
            }
            else if (_zoomWindowActive)
            {
                canvasSurface.Cursor = Cursors.Cross;
            }
            else if (_activeTool != MapCanvasTool.Select)
            {
                canvasSurface.Cursor = Cursors.Cross;
            }
            else
            {
                canvasSurface.Cursor = Cursors.Default;
            }
        }

        private Cursor GetPanCursor()
        {
            if (_panCursor != null)
                return _panCursor;

            string[] candidatePaths =
            [
                Path.Combine(AppContext.BaseDirectory, "Resources", "Cursors", "pan.cur"),
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "Resources",
                    "Cursors",
                    "pan.cur"))
            ];

            foreach (string path in candidatePaths)
            {
                if (!File.Exists(path))
                    continue;

                try
                {
                    IntPtr cursorHandle = LoadCursorFromFile(path);
                    if (cursorHandle != IntPtr.Zero)
                    {
                        _panCursor = new Cursor(cursorHandle);
                        return _panCursor;
                    }
                }
                catch
                {
                    break;
                }
            }

            return Cursors.Hand;
        }

        private void UpdateStatusBar()
        {
            string coordinatesText = _currentMouseWorld.HasValue? $"E: {_currentMouseWorld.Value.X:F4}    N: {_currentMouseWorld.Value.Y:F4}" : "E: --    N: --"; 

            string modeText = GetModeText();

            StatusChanged?.Invoke(coordinatesText, modeText, _engine.ZoomScale);
        }

        private string GetModeText()
        {
            if (_isZooming && _zoomDirection != null)
            {
                return $"Mode: Zooming {_zoomDirection}";
            }

            if (_activeTool != MapCanvasTool.Select)
            {
                return _drawingVertices.Count == 0
                    ? $"Mode: Draw {_activeTool}"
                    : $"Mode: Draw {_activeTool} ({_drawingVertices.Count})";
            }

            if (_isSelectingZoomWindow || _zoomWindowActive)
            {
                return "Mode: Zoom Window";
            }

            if (_isPanning || _panToolActive)
            {
                return "Mode: Pan";
            }

            return "Mode: Ready";
        }

        private bool IsCanvasInteractionLocked => false;
        private bool IsPanBlockedByZoomDebounce =>
            _blockPanUntilZoomSettle ||
            _isZooming ||
            (!_zoomingStatusTimerDisposed && _zoomingStatusTimer.Enabled);

        private bool IsInteractiveNavigation =>
            _isPanning || _isZooming || _isSelectingZoomWindow;

        private bool ShouldDeferDirectRasterRendering =>
            IsInteractiveNavigation ||
            (_rasterCacheRefreshPending &&
             !_suppressStaleRasterFrameUntilFreshRender);

        private bool ShouldDeferDirectVectorRendering =>
            _isPanning;

        private RasterRenderFrame? GetRasterRenderFrame(out CanvasFrameSource source)
        {
            source = CanvasFrameSource.None;

            if (_isPanning &&
                _rasterDeferredRenderer.TryGetPanFrame(
                    _totalPanDelta,
                    out RasterRenderFrame panFrame))
            {
                source = CanvasFrameSource.PanCache;
                return panFrame;
            }

            if (_isZooming &&
                _rasterDeferredRenderer.TryGetZoomFrame(
                    _engine,
                    out RasterRenderFrame zoomFrame))
            {
                source = CanvasFrameSource.ZoomCache;
                return zoomFrame;
            }

            if (_holdZoomStartFrameUntilRasterRefresh &&
                _rasterDeferredRenderer.TryGetZoomFrame(
                    _engine,
                    out RasterRenderFrame heldZoomFrame))
            {
                source = CanvasFrameSource.HeldZoomCache;
                return heldZoomFrame;
            }

            if (!_suppressStaleRasterFrameUntilFreshRender &&
                _rasterDeferredRenderer.TryGetCacheFrame(
                    out RasterRenderFrame cacheFrame))
            {
                source = CanvasFrameSource.Cache;
                return cacheFrame;
            }

            return null;
        }

        private RasterRenderFrame? GetVectorRenderFrame(out CanvasFrameSource source)
        {
            source = CanvasFrameSource.None;

            if (_isPanning &&
                _renderer.TryGetVectorPanFrame(
                    _totalPanDelta,
                    out RasterRenderFrame panFrame))
            {
                source = CanvasFrameSource.PanCache;
                return panFrame;
            }

            if (!_isZooming &&
                !_isSelectingZoomWindow &&
                _renderer.TryGetVectorCacheFrame(out RasterRenderFrame cacheFrame))
            {
                source = CanvasFrameSource.Cache;
                return cacheFrame;
            }

            return null;
        }

        private void UpdateDebugFrameTiming(double elapsedMs)
        {
            _debugFrameNumber++;
            _lastDebugFrameElapsedMs = elapsedMs;
            _averageDebugFrameElapsedMs = _averageDebugFrameElapsedMs <= 0
                ? elapsedMs
                : (_averageDebugFrameElapsedMs * 0.90) + (elapsedMs * 0.10);
        }

        private void DrawDebugOverlayIfNeeded(
            Graphics graphics,
            CanvasFrameSource rasterFrameSource,
            CanvasFrameSource vectorFrameSource)
        {
            if (!_renderer.IsDebugOverlayRequested)
            {
                return;
            }

            MapCanvasRendererDebugState rendererState = _renderer.GetDebugState();
            DeferredRendererDebugState rasterState = _rasterDeferredRenderer.GetDebugState();
            VectorRenderStats vectorStats = rendererState.VectorStats;
            RectangleD viewport = _engine.GetVisibleWorldBounds();
            double scaleDenominator = _engine.ZoomScale > 0
                ? ScreenPixelsPerMetre / _engine.ZoomScale
                : 0.0;

            List<string> lines =
            [
                $"MAP DEBUG  frame #{_debugFrameNumber}  {_lastDebugFrameElapsedMs:0.0} ms  avg {_averageDebugFrameElapsedMs:0.0} ms  fps {GetFramesPerSecond():0}",
                $"View  zoom {_engine.ZoomScale:0.###}  scale 1:{scaleDenominator:0}  size {canvasSurface.Width}x{canvasSurface.Height}",
                $"World X {viewport.Left:0.###}..{viewport.Right:0.###}  Y {viewport.Top:0.###}..{viewport.Bottom:0.###}  W/H {viewport.Width:0.###}/{viewport.Height:0.###}",
                $"Raster layers {rendererState.VisibleRasterLayerCount}/{rendererState.RasterLayerCount}  draw {rasterFrameSource}  cache {(rasterState.CacheValid ? "valid" : "cold")}  pan {(rasterState.PanBufferValid ? "ready" : "no")}  zoom {(rasterState.ZoomFrameAvailable ? "held" : "no")}  refresh {rasterState.LastRefreshElapsedMs:0.0} ms  pending {_rasterCacheRefreshPending}",
                $"Vector layers {rendererState.VectorLayerCount}  features {vectorStats.TotalFeatureCount}  STRtree {vectorStats.SpatialIndexEntryCount}  draw {vectorFrameSource}  cache {(rendererState.VectorCache.CacheValid ? "valid" : "cold")}  pan {(rendererState.VectorCache.PanBufferValid ? "ready" : "no")}  refresh {rendererState.VectorCache.LastRefreshElapsedMs:0.0} ms",
                $"Vector query {vectorStats.QueryCandidateCount} in {vectorStats.QueryElapsedMs:0.00} ms  rendered {vectorStats.RenderedFeatureCount}  hidden {vectorStats.HiddenSkippedCount}  LOD {(vectorStats.LevelOfDetailEnabled ? "on" : "off")} skipped {vectorStats.LodSkippedCount} min {vectorStats.MinimumVisibleWorldSize:0.###}",
                $"Interaction tool {_activeTool}  pan {_isPanning}  zoom {_isZooming} {_zoomDirection ?? ""}  raster deferred {ShouldDeferDirectRasterRendering}  vector deferred {ShouldDeferDirectVectorRendering}  live pending {_liveTileRefreshPending}"
            ];

            string watch = BuildDebugWatchLine(
                rasterFrameSource,
                vectorFrameSource,
                rendererState,
                rasterState);
            if (!string.IsNullOrWhiteSpace(watch))
            {
                lines.Add(watch);
            }

            DrawDebugOverlayPanel(graphics, lines);
        }

        private double GetFramesPerSecond()
        {
            return _averageDebugFrameElapsedMs > 0
                ? 1000.0 / _averageDebugFrameElapsedMs
                : 0.0;
        }

        private string BuildDebugWatchLine(
            CanvasFrameSource rasterFrameSource,
            CanvasFrameSource vectorFrameSource,
            MapCanvasRendererDebugState rendererState,
            DeferredRendererDebugState rasterState)
        {
            List<string> items = [];
            VectorRenderStats vectorStats = rendererState.VectorStats;

            if (_lastDebugFrameElapsedMs > 33.0)
            {
                items.Add("slow frame");
            }

            if (_isPanning &&
                (rasterFrameSource != CanvasFrameSource.PanCache ||
                 vectorFrameSource != CanvasFrameSource.PanCache))
            {
                items.Add("pan cache miss");
            }

            if (rendererState.VisibleRasterLayerCount > 0 &&
                !rasterState.CacheValid &&
                !_rasterCacheRefreshPending)
            {
                items.Add("raster cache cold");
            }

            if (vectorStats.TotalFeatureCount > 0 &&
                vectorStats.QueryCandidateCount == vectorStats.TotalFeatureCount)
            {
                items.Add("viewport query includes all vectors");
            }

            if (vectorStats.QueryElapsedMs > 8.0)
            {
                items.Add("slow vector query");
            }

            return items.Count == 0
                ? "Watch: OK"
                : $"Watch: {string.Join(", ", items)}";
        }

        private void DrawDebugOverlayPanel(Graphics graphics, IReadOnlyList<string> lines)
        {
            if (lines.Count == 0)
            {
                return;
            }

            const int padding = 8;
            const int margin = 8;
            int lineHeight = Math.Max(14, TextRenderer.MeasureText("0", _debugOverlayFont).Height);
            int panelWidth = Math.Min(
                Math.Max(420, canvasSurface.ClientSize.Width - margin * 2),
                980);
            int panelHeight = padding * 2 + lineHeight * lines.Count;
            int panelX = margin;
            int panelY = Math.Max(margin, canvasSurface.ClientSize.Height - panelHeight - margin);
            Rectangle panelBounds = new(panelX, panelY, panelWidth, panelHeight);

            using SolidBrush backgroundBrush = new(Color.FromArgb(218, 24, 28, 34));
            using Pen borderPen = new(Color.FromArgb(170, 0, 170, 255), 1.0f);
            graphics.FillRectangle(backgroundBrush, panelBounds);
            graphics.DrawRectangle(borderPen, panelBounds);

            Color textColor = Color.FromArgb(236, 244, 248);
            for (int i = 0; i < lines.Count; i++)
            {
                Rectangle textBounds = new(
                    panelX + padding,
                    panelY + padding + i * lineHeight,
                    panelWidth - padding * 2,
                    lineHeight);
                TextRenderer.DrawText(
                    graphics,
                    lines[i],
                    _debugOverlayFont,
                    textBounds,
                    textColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);
            }
        }

        private void ZoomingStatusTimer_Tick(object? sender, EventArgs e)
        {
            _zoomingStatusTimer.Stop();
            SetLiveTileInternetFetchingSuspended(false);
            RefreshRasterCacheForCurrentViewAsync(endZoomWhenComplete: true);
            RefreshVectorCacheForCurrentViewImmediately();
            UpdateStatusBar();
            RequestRender();
        }

        private void BeginZoomNavigation(string direction)
        {
            CancelActiveCanvasGesture();
            CancelLiveTileLoading();
            SetLiveTileInternetFetchingSuspended(true);
            _suppressStaleRasterFrameUntilFreshRender = false;

            if (!_isZooming)
            {
                CancelPendingRasterRender();
                _holdZoomStartFrameUntilRasterRefresh = false;
                _rasterDeferredRenderer.BeginZoom(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine);
                _renderer.InvalidateVectorCache();
            }

            _isZooming = true;
            _zoomDirection = direction;
            UpdateStatusBar();
        }

        private void ArmZoomSettleTimer()
        {
            _blockPanUntilZoomSettle = true;
            _zoomingStatusTimer.Stop();
            _zoomingStatusTimer.Start();
        }

        private void StopAndDisposeZoomingStatusTimer()
        {
            if (_zoomingStatusTimerDisposed)
            {
                return;
            }

            _zoomingStatusTimer.Stop();
            _blockPanUntilZoomSettle = false;
            _zoomingStatusTimer.Tick -= ZoomingStatusTimer_Tick;
            _zoomingStatusTimer.Dispose();
            _zoomingStatusTimerDisposed = true;
        }

        private void CancelPendingRasterRender()
        {
            CancellationTokenSource? previousCancellation = _rasterRenderCancellation;
            _rasterRenderCancellation = null;
            previousCancellation?.Cancel();
            previousCancellation?.Dispose();
            _rasterCacheRefreshPending = false;
        }

        private void CancelLiveTileLoading()
        {
            foreach (IRasterRenderLayer layer in _rasterRenderLayers)
            {
                if (layer is XyzLiveTileRenderLayer liveTileLayer)
                {
                    liveTileLayer.CancelPendingTileFetches();
                }
            }
        }

        private void SetLiveTileInternetFetchingSuspended(bool suspended)
        {
            foreach (IRasterRenderLayer layer in _rasterRenderLayers)
            {
                if (layer is XyzLiveTileRenderLayer liveTileLayer)
                {
                    liveTileLayer.SetInternetFetchingSuspended(suspended);
                }
            }
        }

        private void CancelActiveCanvasGesture()
        {
            if (_isPanning)
            {
                _isPanning = false;
                _panStartWorld = null;
                _totalPanDelta = PointF.Empty;
                _holdZoomStartFrameUntilRasterRefresh = false;
                _suppressStaleRasterFrameUntilFreshRender = false;
                canvasSurface.Capture = false;
                _rasterDeferredRenderer.Invalidate();
                _renderer.InvalidateVectorCache();
            }

            if (_isSelectingZoomWindow)
            {
                _isSelectingZoomWindow = false;
                _zoomWindowActive = false;
            }
        }

        private void StopZoomInteraction()
        {
            if (!_isZooming)
            {
                return;
            }

            _zoomingStatusTimer.Stop();
            _blockPanUntilZoomSettle = false;
            SetLiveTileInternetFetchingSuspended(false);
            CancelPendingRasterRender();
            _rasterDeferredRenderer.EndZoom();
            _isZooming = false;
            _zoomDirection = null;
            _rasterCacheRefreshPending = false;
            _suppressStaleRasterFrameUntilFreshRender = false;
            UpdateCanvasCursor();
            UpdateStatusBar();
        }

        private void RefreshRasterCacheForCurrentViewImmediately()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            CancelPendingRasterRender();
            _rasterRenderGeneration++;

            try
            {
                _engine.SnapViewOriginToPixelGrid();
                _rasterDeferredRenderer.RenderNow(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine);
            }
            catch (OperationCanceledException)
            {
                // A stale background raster render was canceled before the immediate pan refresh.
            }
            catch (ObjectDisposedException)
            {
                // The canvas or a raster dataset was disposed while a stale render was ending.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Immediate raster cache refresh failed: {ex.Message}");
            }
            finally
            {
                _rasterCacheRefreshPending = false;
            }
        }

        private void RefreshVectorCacheForCurrentViewImmediately()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            try
            {
                _renderer.RefreshVectorCache(canvasSurface.Size);
            }
            catch (ObjectDisposedException)
            {
                // The canvas was disposed while a vector cache frame was being refreshed.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Vector cache refresh failed: {ex.Message}");
            }
        }

        private async void RefreshRasterCacheForCurrentViewAsync(bool endZoomWhenComplete = false)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (IsInteractiveNavigation && !endZoomWhenComplete)
            {
                CancelPendingRasterRender();
                return;
            }

            CancelPendingRasterRender();

            CancellationTokenSource cancellation = new();
            CancellationToken token = cancellation.Token;
            _rasterRenderCancellation = cancellation;
            int generation = ++_rasterRenderGeneration;
            _rasterCacheRefreshPending = true;

            try
            {
                _engine.SnapViewOriginToPixelGrid();
                bool refreshed = await _rasterDeferredRenderer.RenderAsync(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine,
                    token);

                if (token.IsCancellationRequested ||
                    generation != _rasterRenderGeneration ||
                    IsDisposed ||
                    Disposing)
                {
                    return;
                }

                if (!refreshed)
                {
                    if (endZoomWhenComplete)
                    {
                        _holdZoomStartFrameUntilRasterRefresh = true;
                    }

                    return;
                }

                if (endZoomWhenComplete ||
                    _holdZoomStartFrameUntilRasterRefresh)
                {
                    _rasterDeferredRenderer.EndZoom();
                    _holdZoomStartFrameUntilRasterRefresh = false;
                }

                _suppressStaleRasterFrameUntilFreshRender = false;

                if (IsHandleCreated)
                {
                    BeginInvoke((MethodInvoker)RequestRender);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during fast pan/zoom sequences.
            }
            catch (ObjectDisposedException)
            {
                // The canvas or a raster dataset was disposed while a stale render was ending.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Raster cache refresh failed: {ex.Message}");
            }
            finally
            {
                if (ReferenceEquals(_rasterRenderCancellation, cancellation))
                {
                    if (endZoomWhenComplete)
                    {
                        if (!_holdZoomStartFrameUntilRasterRefresh)
                        {
                            _rasterDeferredRenderer.EndZoom();
                        }

                        SetLiveTileInternetFetchingSuspended(false);
                        _blockPanUntilZoomSettle = false;
                        _isZooming = false;
                        _zoomDirection = null;
                    }

                    _rasterRenderCancellation = null;
                    _rasterCacheRefreshPending = false;

                    if (endZoomWhenComplete && !IsDisposed && !Disposing)
                    {
                        UpdateCanvasCursor();
                        UpdateStatusBar();
                        RequestRender();
                    }
                }

                cancellation.Dispose();
            }
        }

        private void UpdateRasterWorldBounds()
        {
            UpdateWorldBounds();
        }

        private void UpdateWorldBounds()
        {
            List<IRasterRenderLayer> extentLayers = _rasterRenderLayers
                .Where(layer => layer is not XyzLiveTileRenderLayer)
                .ToList();

            RectangleD? vectorBounds = _renderer.GetVectorFeatureBounds();

            if (extentLayers.Count == 0 && !vectorBounds.HasValue)
            {
                _engine.SetWorldBounds(MapCanvasEngine.DefaultWorldBounds);
                return;
            }

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (IRasterRenderLayer layer in extentLayers)
            {
                minX = Math.Min(minX, Math.Min(layer.WorldBounds.Left, layer.WorldBounds.Right));
                maxX = Math.Max(maxX, Math.Max(layer.WorldBounds.Left, layer.WorldBounds.Right));
                minY = Math.Min(minY, Math.Min(layer.WorldBounds.Top, layer.WorldBounds.Bottom));
                maxY = Math.Max(maxY, Math.Max(layer.WorldBounds.Top, layer.WorldBounds.Bottom));
            }

            if (vectorBounds.HasValue)
            {
                RectangleD bounds = vectorBounds.Value;
                minX = Math.Min(minX, bounds.Left);
                maxX = Math.Max(maxX, bounds.Right);
                minY = Math.Min(minY, bounds.Top);
                maxY = Math.Max(maxY, bounds.Bottom);
            }

            _engine.SetWorldBounds(
                new RectangleD(minX, minY, maxX - minX, maxY - minY));
        }

        private static bool TryNormalizeWorldBounds(
            RectangleD source,
            out RectangleD normalized)
        {
            normalized = default;

            double left = Math.Min(source.Left, source.Right);
            double right = Math.Max(source.Left, source.Right);
            double bottom = Math.Min(source.Top, source.Bottom);
            double top = Math.Max(source.Top, source.Bottom);

            if (!IsFinite(left) ||
                !IsFinite(right) ||
                !IsFinite(bottom) ||
                !IsFinite(top) ||
                right <= left ||
                top <= bottom)
            {
                return false;
            }

            normalized = new RectangleD(left, bottom, right - left, top - bottom);
            return true;
        }

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);

        private void DisposeRasterRenderLayers()
        {
            CancelPendingRasterRender();

            foreach (IRasterRenderLayer rasterLayer in _rasterRenderLayers)
                rasterLayer.Dispose();

            _rasterRenderLayers.Clear();
            _renderer.UpdateRasterLayers(Array.Empty<IRasterRenderLayer>());
            _rasterDeferredRenderer.Invalidate();
        }
    }
}
