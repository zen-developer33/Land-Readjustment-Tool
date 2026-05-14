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
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;
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
        private enum ArcDrawingMode
        {
            ThreePoint,
            CenterStartEnd
        }

        private enum CircleDrawingMode
        {
            CenterRadius,
            CenterDiameter,
            TwoPointDiameter,
            ThreePoint
        }

        private enum PolylineSegmentDrawingMode
        {
            Line,
            TangentArc,     // Arc auto-tangent to the previous segment direction
            ThreePointArc   // Arc through an explicit through-point picked by the user
        }

        private const int ZoomSettleIntervalMs = 150;
        private const int ObjectSelectionTolerancePixels = 3;
        private const int SnapQueryBoxPixels = 20;
        private const double DefaultSnapPickTolerancePixels = 8.0;
        private const float DefaultSnapGlyphSizePixels = 14.0f;
        private const double ScreenPixelsPerMetre = 96.0 / 0.0254;
        private const bool DefaultShowDebugOverlay = false;
        private static readonly double[] StandardScaleDenominators = BuildStandardScaleDenominators();
        private static readonly NtsGeometryFactory SelectionGeometryFactory = new(new NtsPrecisionModel(), 0);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);

        private readonly MapCanvasEngine _engine;
        private readonly MapCanvasRenderer _renderer;
        private readonly RasterDeferredRenderer _rasterDeferredRenderer = new();
        private readonly MapCanvasSnapManager _snapManager = new();
        private readonly List<IRasterRenderLayer> _rasterRenderLayers = new List<IRasterRenderLayer>();
        private readonly Font _debugOverlayFont = new("Consolas", 8.25f, FontStyle.Regular);
        private MapCanvasRenderSettings _renderSettings;
        private readonly CanvasCommandService _commandService = new();

        public CanvasCommandService CommandService => _commandService;

        public event Action<string, string, double>? StatusChanged;
        public event Action<IShape>? ShapeCompleted;
        public event Action? SelectToolRequested;
        public event Action<IReadOnlyList<Guid>>? SelectedObjectsDeleteRequested;
        public event Action? SelectedObjectsAssignParcelDataRequested;
        public event Action<IReadOnlyList<Guid>>? SelectedCanvasObjectsChanged;

        private bool _panToolActive;
        private bool _isPanning;
        private bool _isZooming;
        private string? _zoomDirection;
        private bool _zoomWindowActive;
        private bool _isSelectingZoomWindow;
        private Point _lastPanPoint;
        private Point _panStartPoint;
        private PointF _totalPanDelta;
        private PointD? _panStartWorld;
        private PointD _panStartWorldOrigin;
        private Point _zoomWindowStart;
        private Point _zoomWindowCurrent;
        private PointD? _currentMouseWorld;
        private bool _zoomingStatusTimerDisposed;
        private CancellationTokenSource? _rasterRenderCancellation;
        private int _rasterRenderGeneration;
        private bool _rasterCacheRefreshPending;
        private CancellationTokenSource? _vectorRenderCancellation;
        private int _vectorRenderGeneration;
        private bool _vectorCacheRefreshPending;
        private volatile bool _liveTileRefreshPending;
        private bool _showDebugOverlay = DefaultShowDebugOverlay;
        private long _debugFrameNumber;
        private double _lastDebugFrameElapsedMs;
        private double _averageDebugFrameElapsedMs;
        private bool _blockPanUntilZoomSettle;
        private bool _holdZoomStartFrameUntilRasterRefresh;
        private bool _suppressStaleRasterFrameUntilFreshRender;
        private bool _holdVectorPanFrameUntilRefresh;
        private bool _holdVectorZoomFrameUntilRefresh;
        private PointF _heldVectorPanDelta;
        private bool _snapEnabled = true;
        private bool _orthoModeEnabled;
        private double _snapPickTolerancePixels = DefaultSnapPickTolerancePixels;
        private float _snapGlyphSizePixels = DefaultSnapGlyphSizePixels;
        private SnapPoint? _currentSnapPoint;
        private int _lastSnapQueryFeatureCount;
        private int _lastSnapCandidateCount;
        private double _lastSnapQueryElapsedMs;
        private MapCanvasTool _activeTool = MapCanvasTool.Select;
        private ArcDrawingMode _arcDrawingMode = ArcDrawingMode.ThreePoint;
        private CircleDrawingMode _circleDrawingMode = CircleDrawingMode.CenterRadius;
        private PolylineSegmentDrawingMode _polylineSegmentMode = PolylineSegmentDrawingMode.Line;
        private bool _polylineArcAwaitingCenter;
        private bool _polylineArcAwaitingEnd;
        private PointD? _polylineArcCenterPoint;
        private PointD? _pendingPolylineArcThroughPoint;
        private readonly List<PointD> _drawingVertices = new List<PointD>();
        private readonly List<PolylineShape.PolylineSegment> _drawingSegments = new List<PolylineShape.PolylineSegment>();
        private List<CanvasFeature> _vectorFeatures = new List<CanvasFeature>();
        private List<CanvasLayer> _vectorLayers = new List<CanvasLayer>();
        private readonly HashSet<Guid> _selectedShapeIds = new HashSet<Guid>();
        private bool _isSelectingObjects;
        private Point _objectSelectionStart;
        private Point _objectSelectionCurrent;
        private IShape? _previewShape;
        private string _activeDrawingLayerName = "Features";
        private CanvasLayer? _activeDrawingLayer;
        private Cursor? _panCursor;
        private readonly ContextMenuStrip _objectSelectionContextMenu = new();
        private readonly ContextMenuStrip _drawingOptionsContextMenu = new();
        private readonly ToolStripMenuItem _mnuAssignParcelData = new("Assign Parcel Data...");
        private readonly ToolStripMenuItem _mnuDeleteSelectedObjects = new("Delete Selected Object(s)");
        private readonly System.Windows.Forms.Timer _zoomingStatusTimer = new()
        {
            Interval = Math.Max(1, ZoomSettleIntervalMs)
        };

        public MapCanvasControl()
        {
            InitializeComponent();
            ConfigureGraphicsPipeline();
            _engine = new MapCanvasEngine(canvasSurface.Size);
            _renderSettings = MapCanvasRenderSettings.CreateLightDefaults();
            _renderer = new MapCanvasRenderer(_engine, _renderSettings);
            _zoomingStatusTimer.Tick += ZoomingStatusTimer_Tick;
            _objectSelectionContextMenu.Opening += ObjectSelectionContextMenu_Opening;
            _mnuAssignParcelData.Click += (_, _) => SelectedObjectsAssignParcelDataRequested?.Invoke();
            _mnuDeleteSelectedObjects.Click += (_, _) => RequestDeleteSelectedObjects();
            _objectSelectionContextMenu.Items.Add(_mnuAssignParcelData);
            _objectSelectionContextMenu.Items.Add(new ToolStripSeparator());
            _objectSelectionContextMenu.Items.Add(_mnuDeleteSelectedObjects);
            _drawingOptionsContextMenu.Closed += (_, _) => canvasSurface.Focus();
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
                ApplySnapSettings(
                    projectSettings.SnapEnabled,
                    projectSettings.SnapTolerancePx,
                    projectSettings.SnapGlyphSizePx);
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
            RefreshVectorCacheForCurrentViewAsync();
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

        public MapCanvasRenderSettings GetRenderSettings()
        {
            return _renderSettings;
        }

        public void ApplyNorthMarkerVisible(bool visible)
        {
            _renderSettings.ShowNorthMarker = visible;
            _renderer.UpdateSettings(_renderSettings);
            RequestRender();
        }

        public void ApplySnapEnabled(bool enabled)
        {
            ApplySnapSettings(enabled, _snapPickTolerancePixels, _snapGlyphSizePixels);
        }

        public void ApplySnapSettings(bool enabled, double tolerancePixels, double glyphSizePixels)
        {
            _snapEnabled = enabled;
            _snapPickTolerancePixels = Math.Clamp(tolerancePixels, 1.0, 50.0);
            _snapGlyphSizePixels = (float)Math.Clamp(glyphSizePixels, 6.0, 32.0);
            if (!enabled)
            {
                ClearCurrentSnapPoint();
            }

            RequestRender();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SnapEnabled => _snapEnabled;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool OrthoModeEnabled
        {
            get => _orthoModeEnabled;
            set
            {
                if (_orthoModeEnabled == value)
                {
                    return;
                }

                _orthoModeEnabled = value;
                RefreshDrawingPreviewFromCursor();
                UpdateStatusBar();
            }
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
            RefreshVectorCacheForCurrentViewAsync();
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

            RefreshVectorCacheForCurrentViewAsync();
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
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
        }

        public void PreviewSelectCanvasObject(Guid canvasObjectId, bool zoomToObject)
        {
            CanvasFeature? feature = _vectorFeatures
                .FirstOrDefault(item =>
                    item.CanvasObject.Id == canvasObjectId ||
                    item.Shape.Id == canvasObjectId);

            ReplaceSelectedObjects(feature == null ? [] : [feature]);

            if (feature != null &&
                zoomToObject &&
                TryNormalizeWorldBounds(feature.Shape.GetBoundingBox(), out RectangleD bounds))
            {
                ZoomToWorldBounds(bounds);
            }

            RequestRender();
        }

        public void SelectCanvasObjects(IEnumerable<Guid> canvasObjectIds, bool zoomToSelection)
        {
            HashSet<Guid> ids = canvasObjectIds
                .Where(id => id != Guid.Empty)
                .ToHashSet();
            List<CanvasFeature> features = ids.Count == 0
                ? []
                : _vectorFeatures
                    .Where(item => ids.Contains(item.CanvasObject.Id) || ids.Contains(item.Shape.Id))
                    .ToList();

            ReplaceSelectedObjects(features);

            if (zoomToSelection &&
                TryGetCombinedFeatureBounds(features, out RectangleD bounds))
            {
                ZoomToWorldBounds(bounds);
                return;
            }

            RequestRender();
        }

        public void ZoomToCanvasObjects(IEnumerable<Guid> canvasObjectIds)
        {
            HashSet<Guid> ids = canvasObjectIds
                .Where(id => id != Guid.Empty)
                .ToHashSet();
            if (ids.Count == 0)
                return;

            List<CanvasFeature> features = _vectorFeatures
                .Where(item => ids.Contains(item.CanvasObject.Id) || ids.Contains(item.Shape.Id))
                .ToList();

            if (TryGetCombinedFeatureBounds(features, out RectangleD bounds))
            {
                ZoomToWorldBounds(bounds);
                return;
            }

            RequestRender();
        }

        public void ClearPreviewSelection()
        {
            ClearSelectedObjects();
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
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
        }

        /// <summary>
        /// Zooms to a specific scale factor (e.g., 0.5 = 50%, 1.0 = 100%, 2.0 = 200%)
        /// </summary>
        public void ZoomToScale(double scaleFactor)
        {
            if (IsCanvasInteractionLocked || scaleFactor <= 0)
            {
                return;
            }

            // Get current view center
            double centerX = _engine.ViewOriginWorld.X + (Width / 2.0) / _engine.ZoomScale;
            double centerY = _engine.ViewOriginWorld.Y + (Height / 2.0) / _engine.ZoomScale;
            PointD centerWorld = new(centerX, centerY);

            // Set viewport with new zoom scale while maintaining center
            _engine.SetViewport(centerWorld, scaleFactor);
            RefreshRasterCacheForCurrentViewAsync();
            RefreshVectorCacheForCurrentViewAsync();
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
            RefreshVectorCacheForCurrentViewAsync();
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
            RefreshVectorCacheForCurrentViewAsync();
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
            RefreshVectorCacheForCurrentViewAsync();
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
            RefreshVectorCacheForCurrentViewAsync();
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
            DrawSnapGlyph(e.Graphics);
            // Diameter preview drawing is handled by the vector renderer when the
            // preview shape contains the CenterDiameterEndpoint property.
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
                return;
            }

            double targetZoomScale = GetNextStandardZoomScale(_engine.ZoomScale, zoomIn);
            _engine.ZoomAtPointToScale(screenPoint, targetZoomScale);
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
            List<double> values = new List<double>();

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
                CancelPendingVectorRender();
                CancelLiveTileLoading();
                SetLiveTileInternetFetchingSuspended(true);
                _isPanning = true;
                _holdVectorPanFrameUntilRefresh = false;
                _holdZoomStartFrameUntilRasterRefresh = false;
                _lastPanPoint = e.Location;
                _totalPanDelta = PointF.Empty;
                _panStartWorld = _engine.ScreenToWorld(e.Location);
                _panStartPoint = e.Location;
                _panStartWorldOrigin = _engine.ViewOriginWorld;
                _currentMouseWorld = _panStartWorld;
                _rasterDeferredRenderer.BeginPan(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine);
                EnsureVectorPanSnapshot();
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
                int totalDx = e.X - _panStartPoint.X;
                int totalDy = e.Y - _panStartPoint.Y;
                _totalPanDelta = new PointF(totalDx, totalDy);
                _engine.SetViewOriginFromPanStart(
                    _panStartWorldOrigin,
                    totalDx,
                    totalDy);
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

            UpdateCurrentSnapPoint(e.Location);
            if (_activeTool != MapCanvasTool.Select && _drawingVertices.Count > 0)
            {
                _currentMouseWorld = GetCurrentDrawingWorldPoint(e.Location);
            }
            else
            {
                _currentMouseWorld = _currentSnapPoint?.Position ?? _engine.ScreenToWorld(e.Location);
            }
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
                    RefreshVectorCacheForCurrentViewAsync();
                }

                UpdateCanvasCursor();
                RequestRender();
                return;
            }

            if (_isPanning)
            {
                PointF finalPanDelta = _totalPanDelta;
                _isPanning = false;
                canvasSurface.Capture = false;
                _currentMouseWorld = _engine.ScreenToWorld(e.Location);
                _panStartWorld = null;
                _heldVectorPanDelta = finalPanDelta;
                _holdVectorPanFrameUntilRefresh = true;
                _totalPanDelta = PointF.Empty;
                SetLiveTileInternetFetchingSuspended(false);
                _suppressStaleRasterFrameUntilFreshRender = true;
                RefreshRasterCacheForCurrentViewAsync();
                RefreshVectorCacheForCurrentViewAsync();
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
                ClearCurrentSnapPoint();
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

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (CanCompleteMultiPointDrawing())
                {
                    CompleteMultiPointDrawing();
                    e.Handled = true;
                }
                return;
            }

            if (e.KeyCode == Keys.Back)
            {
                UndoLastDrawingVertex();
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
            UpdateCurrentSnapPoint(e.Location);
            PointD worldPoint = GetCurrentDrawingWorldPoint(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                ShowDrawingOptionsMenu(e.Location);
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
                    HandleTwoPointDrawing(worldPoint);
                    break;
                case MapCanvasTool.Circle:
                    if (_circleDrawingMode == CircleDrawingMode.ThreePoint)
                    {
                        HandleThreePointCircleDrawing(worldPoint);
                    }
                    else
                    {
                        HandleTwoPointDrawing(worldPoint);
                    }

                    break;
                case MapCanvasTool.Arc:
                    HandleArcDrawing(worldPoint);
                    break;
                case MapCanvasTool.Polyline:
                case MapCanvasTool.Polygon:
                    HandlePolylineOrPolygonDrawing(worldPoint, e.Location);
                    break;
            }
        }

        private PointD GetCurrentDrawingWorldPoint(Point screenPoint)
        {
            PointD mouseWorld = _engine.ScreenToWorld(screenPoint);

            // Snap always takes priority — even when ortho is active.
            if (_snapEnabled && _currentSnapPoint != null)
                return _currentSnapPoint.Position;

            if (!IsActiveLineSegmentOrthoConstrained())
                return mouseWorld;

            PointD anchor = GetCurrentOrthoAnchor();
            return OrthoConstraintService.ConstrainToDominantAxis(anchor, mouseWorld);
        }

        private bool IsActiveLineSegmentOrthoConstrained()
        {
            if (!_orthoModeEnabled || _drawingVertices.Count == 0)
            {
                return false;
            }

            return _activeTool == MapCanvasTool.Line ||
                   ((_activeTool == MapCanvasTool.Polyline || _activeTool == MapCanvasTool.Polygon) &&
                    _polylineSegmentMode == PolylineSegmentDrawingMode.Line &&
                    !_polylineArcAwaitingCenter &&
                    !_polylineArcAwaitingEnd &&
                    !_pendingPolylineArcThroughPoint.HasValue);
        }

        private PointD GetCurrentOrthoAnchor()
        {
            return _activeTool == MapCanvasTool.Line
                ? _drawingVertices[0]
                : _drawingVertices[^1];
        }

        private void RefreshDrawingPreviewFromCursor()
        {
            if (_activeTool != MapCanvasTool.Select && _drawingVertices.Count > 0)
            {
                UpdateDrawingPreview(canvasSurface.PointToClient(Cursor.Position));
                return;
            }

            RequestRender();
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
                MapCanvasTool.Circle when _circleDrawingMode != CircleDrawingMode.ThreePoint => CreateCircleShape(_drawingVertices[0], worldPoint),
                _ => null
            };

            CompleteShape(completedShape);
        }

        private void HandleArcDrawing(PointD worldPoint)
        {
            _drawingVertices.Add(worldPoint);
            if (_drawingVertices.Count < 3)
            {
                RequestRender();
                return;
            }

            ArcShape? arc = _arcDrawingMode == ArcDrawingMode.CenterStartEnd
                ? ArcShape.FromCenterStartEnd(_drawingVertices[0], _drawingVertices[1], _drawingVertices[2])
                : ArcShape.FromThreePoints(_drawingVertices[0], _drawingVertices[1], _drawingVertices[2]);
            CompleteShape(arc);
        }

        private void HandleThreePointCircleDrawing(PointD worldPoint)
        {
            _drawingVertices.Add(worldPoint);
            if (_drawingVertices.Count < 3)
            {
                RequestRender();
                return;
            }

            CompleteShape(CreateThreePointCircle(
                _drawingVertices[0],
                _drawingVertices[1],
                _drawingVertices[2]));
        }

        private void HandlePolylineOrPolygonDrawing(PointD worldPoint, Point screenPoint)
        {
            if (_polylineArcAwaitingCenter)
            {
                _polylineArcCenterPoint = worldPoint;
                _polylineArcAwaitingCenter = false;
                _polylineArcAwaitingEnd = true;
                UpdateDrawingPreview(screenPoint);
                return;
            }

            if (_polylineArcAwaitingEnd && _polylineArcCenterPoint.HasValue)
            {
                AddPolylineCenterArcSegment(_polylineArcCenterPoint.Value, _drawingVertices[^1], worldPoint);
                _polylineArcAwaitingEnd = false;
                _polylineArcCenterPoint = null;
                UpdateDrawingPreview(screenPoint);
                return;
            }

            if (_polylineSegmentMode == PolylineSegmentDrawingMode.Line ||
                _drawingVertices.Count == 0)
            {
                AddPolylineLineSegment(worldPoint);
                UpdateDrawingPreview(screenPoint);
                return;
            }

            if (_polylineSegmentMode == PolylineSegmentDrawingMode.ThreePointArc)
            {
                if (!_pendingPolylineArcThroughPoint.HasValue)
                {
                    // First click: store the through-point, wait for end-point
                    _pendingPolylineArcThroughPoint = worldPoint;
                    UpdateDrawingPreview(screenPoint);
                    UpdateStatusBar();
                    return;
                }

                // Second click: complete the arc through the stored through-point
                AddPolylineThreePointArcSegment(
                    _drawingVertices[^1],
                    _pendingPolylineArcThroughPoint.Value,
                    worldPoint);
                _pendingPolylineArcThroughPoint = null;
                UpdateDrawingPreview(screenPoint);
                return;
            }

            // TangentArc mode: auto-tangent from the previous segment direction
            if (_drawingVertices.Count >= 2)
            {
                AddPolylineArcSegment(_drawingVertices[^1], GetTangentReferencePoint(), worldPoint);
            }
            else
            {
                AddPolylineLineSegment(worldPoint);
            }

            UpdateDrawingPreview(screenPoint);
        }

        private IShape? CreateCircleShape(PointD firstPoint, PointD secondPoint)
        {
            return _circleDrawingMode switch
            {
                CircleDrawingMode.CenterRadius => new CircleShape(firstPoint, secondPoint),
                CircleDrawingMode.CenterDiameter => CreateCenterDiameterCircle(firstPoint, secondPoint),
                CircleDrawingMode.TwoPointDiameter => CreateTwoPointDiameterCircle(firstPoint, secondPoint),
                _ => null
            };
        }

        private static CircleShape? CreateCenterDiameterCircle(PointD center, PointD diameterPoint)
        {
            PointD radiusPoint = new(
                center.X + (diameterPoint.X - center.X) / 2.0,
                center.Y + (diameterPoint.Y - center.Y) / 2.0);
            return SameWorldPoint(center, radiusPoint)
                ? null
                : new CircleShape(center, radiusPoint);
        }

        private static CircleShape? CreateTwoPointDiameterCircle(PointD firstPoint, PointD secondPoint)
        {
            if (SameWorldPoint(firstPoint, secondPoint))
            {
                return null;
            }

            PointD center = new(
                (firstPoint.X + secondPoint.X) / 2.0,
                (firstPoint.Y + secondPoint.Y) / 2.0);
            return new CircleShape(center, secondPoint);
        }

        private static CircleShape? CreateThreePointCircle(PointD first, PointD second, PointD third)
        {
            ArcShape? arc = ArcShape.FromThreePoints(first, second, third);
            return arc == null
                ? null
                : new CircleShape(arc.Center, new PointD(arc.Center.X + arc.Radius, arc.Center.Y));
        }

        private void ShowDrawingOptionsMenu(Point location)
        {
            _drawingOptionsContextMenu.Items.Clear();

            bool drawingStarted = _drawingVertices.Count > 0 ||
                                  _polylineArcAwaitingCenter ||
                                  _polylineArcAwaitingEnd ||
                                  _pendingPolylineArcThroughPoint.HasValue;

            // --- Section 1: Creation Methods ---
            switch (_activeTool)
            {
                case MapCanvasTool.Arc:
                    AddSectionHeader("Creation Method");
                    AddDrawingOption("3 Point Arc", _arcDrawingMode == ArcDrawingMode.ThreePoint, () => SetArcDrawingMode(ArcDrawingMode.ThreePoint));
                    AddDrawingOption("Center, Start, End", _arcDrawingMode == ArcDrawingMode.CenterStartEnd, () => SetArcDrawingMode(ArcDrawingMode.CenterStartEnd));
                    break;

                case MapCanvasTool.Circle:
                    AddSectionHeader("Creation Method");
                    AddDrawingOption("Center + Radius", _circleDrawingMode == CircleDrawingMode.CenterRadius, () => SetCircleDrawingMode(CircleDrawingMode.CenterRadius));
                    AddDrawingOption("Center + Diameter", _circleDrawingMode == CircleDrawingMode.CenterDiameter, () => SetCircleDrawingMode(CircleDrawingMode.CenterDiameter));
                    AddDrawingOption("2 Point Diameter", _circleDrawingMode == CircleDrawingMode.TwoPointDiameter, () => SetCircleDrawingMode(CircleDrawingMode.TwoPointDiameter));
                    AddDrawingOption("3 Point Circle", _circleDrawingMode == CircleDrawingMode.ThreePoint, () => SetCircleDrawingMode(CircleDrawingMode.ThreePoint));
                    break;

                case MapCanvasTool.Polyline:
                case MapCanvasTool.Polygon:
                    AddSectionHeader("Segment Type");
                    AddDrawingOption("Line Segment", _polylineSegmentMode == PolylineSegmentDrawingMode.Line, () => SetPolylineSegmentMode(PolylineSegmentDrawingMode.Line));
                    AddDrawingOption("Tangent Arc", _polylineSegmentMode == PolylineSegmentDrawingMode.TangentArc, () => SetPolylineSegmentMode(PolylineSegmentDrawingMode.TangentArc));
                    AddDrawingOption("3-Point Arc", _polylineSegmentMode == PolylineSegmentDrawingMode.ThreePointArc, () => SetPolylineSegmentMode(PolylineSegmentDrawingMode.ThreePointArc));
                    break;
            }

            // --- Section 2: Enter Input (only when drawing has started) ---
            switch (_activeTool)
            {
                case MapCanvasTool.Line when drawingStarted:
                    _drawingOptionsContextMenu.Items.Add(new ToolStripSeparator());
                    AddSectionHeader("Enter Input");
                    AddDrawingCommand("Length and Angle...", PromptStandaloneLineLengthAndAngle);
                    break;

                case MapCanvasTool.Rectangle when drawingStarted:
                    _drawingOptionsContextMenu.Items.Add(new ToolStripSeparator());
                    AddSectionHeader("Enter Input");
                    AddDrawingCommand("Width and Height...", PromptRectangleSize);
                    break;

                case MapCanvasTool.Circle when drawingStarted &&
                    _circleDrawingMode is CircleDrawingMode.CenterRadius or CircleDrawingMode.CenterDiameter:
                    _drawingOptionsContextMenu.Items.Add(new ToolStripSeparator());
                    AddSectionHeader("Enter Input");
                    AddDrawingCommand(_circleDrawingMode == CircleDrawingMode.CenterRadius
                        ? "Radius..."
                        : "Diameter...", PromptCircleValue);
                    break;

                case MapCanvasTool.Polyline when drawingStarted:
                case MapCanvasTool.Polygon when drawingStarted:
                    if (_polylineSegmentMode == PolylineSegmentDrawingMode.Line ||
                        _polylineSegmentMode == PolylineSegmentDrawingMode.TangentArc ||
                        _polylineArcAwaitingCenter || _polylineArcAwaitingEnd)
                    {
                        _drawingOptionsContextMenu.Items.Add(new ToolStripSeparator());
                        AddSectionHeader("Enter Input");
                        if (_polylineSegmentMode == PolylineSegmentDrawingMode.Line)
                            AddDrawingCommand("Length and Angle...", PromptPolylineLineLengthAndAngle);
                        else
                            AddDrawingCommand("Arc Center and End...", BeginPolylineArcCenterEndInput);
                    }
                    break;
            }

            // --- Section 3: Finish / Undo / Cancel (only when drawing has started) ---
            if (drawingStarted)
            {
                _drawingOptionsContextMenu.Items.Add(new ToolStripSeparator());

                if (CanCompleteMultiPointDrawing())
                    AddDrawingCommand("Finish  [Enter]", CompleteMultiPointDrawing);

                if (_drawingVertices.Count > 1 ||
                    _drawingSegments.Count > 0 ||
                    _polylineArcAwaitingCenter ||
                    _polylineArcAwaitingEnd)
                {
                    AddDrawingCommand("Undo Last Point  [Backspace]", UndoLastDrawingVertex);
                }

                AddDrawingCommand("Cancel  [Esc]", CancelDrawing);
            }

            if (_drawingOptionsContextMenu.Items.Count > 0)
            {
                _drawingOptionsContextMenu.Show(canvasSurface, location);
            }
        }

        private void AddSectionHeader(string text)
        {
            ToolStripMenuItem header = new(text)
            {
                Enabled = false,
                Font = new Font(SystemFonts.MenuFont ?? SystemFonts.DefaultFont, FontStyle.Bold)
            };
            _drawingOptionsContextMenu.Items.Add(header);
        }

        private void AddDrawingOption(string text, bool isChecked, Action action)
        {
            ToolStripMenuItem item = new(text)
            {
                Checked = isChecked
            };
            item.Click += (_, _) => action();
            _drawingOptionsContextMenu.Items.Add(item);
        }

        private void AddDrawingCommand(string text, Action action)
        {
            ToolStripMenuItem item = new(text);
            item.Click += (_, _) => action();
            _drawingOptionsContextMenu.Items.Add(item);
        }

        private void SetArcDrawingMode(ArcDrawingMode mode)
        {
            _arcDrawingMode = mode;
            if (_drawingVertices.Count > 1)
            {
                CancelDrawing();
            }
            else
            {
                RequestRender();
            }
        }

        private void SetCircleDrawingMode(CircleDrawingMode mode)
        {
            _circleDrawingMode = mode;
            if (_drawingVertices.Count > 0)
            {
                CancelDrawing();
            }
            else
            {
                RequestRender();
            }
        }

        private void SetPolylineSegmentMode(PolylineSegmentDrawingMode mode)
        {
            _polylineSegmentMode = mode;
            _polylineArcAwaitingCenter = false;
            _polylineArcAwaitingEnd = false;
            _polylineArcCenterPoint = null;
            _pendingPolylineArcThroughPoint = null;
            UpdateStatusBar();
            RequestRender();
        }

        private void BeginPolylineArcCenterEndInput()
        {
            if (_drawingVertices.Count == 0)
            {
                return;
            }

            _polylineArcAwaitingCenter = true;
            _polylineArcAwaitingEnd = false;
            _polylineArcCenterPoint = null;
            RequestRender();
        }

        private void AddPolylineLineSegment(PointD endPoint)
        {
            if (_drawingVertices.Count == 0)
            {
                _drawingVertices.Add(endPoint);
                return;
            }

            PointD startPoint = _drawingVertices[^1];
            _drawingSegments.Add(new PolylineShape.PolylineSegment(
                PolylineShape.PolylineSegmentKind.Line,
                startPoint,
                endPoint));
            _drawingVertices.Add(endPoint);
        }

        // Returns the tangent-reference point for FromTangentStartEnd.
        // When the last drawn segment is an arc, the reference is derived from the
        // arc's endpoint tangent so the new arc continues smoothly. Otherwise falls
        // back to the second-to-last vertex (line predecessor).
        private PointD GetTangentReferencePoint()
        {
            if (_drawingSegments.Count > 0 &&
                _drawingSegments[^1].Kind == PolylineShape.PolylineSegmentKind.Arc &&
                _drawingSegments[^1].Arc != null)
            {
                ArcShape lastArc = _drawingSegments[^1].Arc!;
                double endAngle = lastArc.StartAngleRadians + lastArc.SweepAngleRadians;
                double sign = lastArc.SweepAngleRadians >= 0.0 ? 1.0 : -1.0;
                PointD arcEnd = _drawingVertices[^1];
                double tx = -sign * Math.Sin(endAngle);
                double ty =  sign * Math.Cos(endAngle);
                return new PointD(arcEnd.X - tx, arcEnd.Y - ty);
            }

            return _drawingVertices[^2];
        }

        private void AddPolylineArcSegment(PointD startPoint, PointD tangentReferencePoint, PointD endPoint)
        {
            ArcShape? arc = ArcShape.FromTangentStartEnd(startPoint, tangentReferencePoint, endPoint);
            if (arc == null)
            {
                AddPolylineLineSegment(endPoint);
                return;
            }

            _drawingSegments.Add(new PolylineShape.PolylineSegment(
                PolylineShape.PolylineSegmentKind.Arc,
                arc.StartPoint,
                arc.EndPoint,
                arc));
            _drawingVertices.Add(endPoint);
        }

        private void AddPolylineCenterArcSegment(PointD centerPoint, PointD startPoint, PointD endPoint)
        {
            ArcShape? arc = ArcShape.FromCenterStartEnd(centerPoint, startPoint, endPoint);
            if (arc == null)
            {
                AddPolylineLineSegment(endPoint);
                return;
            }

            _drawingSegments.Add(new PolylineShape.PolylineSegment(
                PolylineShape.PolylineSegmentKind.Arc,
                arc.StartPoint,
                arc.EndPoint,
                arc));
            _drawingVertices.Add(endPoint);
        }

        private void AddPolylineThreePointArcSegment(PointD startPoint, PointD throughPoint, PointD endPoint)
        {
            ArcShape? arc = ArcShape.FromThreePoints(startPoint, throughPoint, endPoint);
            if (arc == null)
            {
                AddPolylineLineSegment(endPoint);
                return;
            }

            _drawingSegments.Add(new PolylineShape.PolylineSegment(
                PolylineShape.PolylineSegmentKind.Arc,
                arc.StartPoint,
                arc.EndPoint,
                arc));
            _drawingVertices.Add(endPoint);
        }

        private void PromptPolylineLineLengthAndAngle()
        {
            if (_drawingVertices.Count == 0 ||
                !TryPromptTwoValues("Line Segment", "Length", "Angle (deg)", out double length, out double angleDegrees))
            {
                return;
            }

            PointD start = _drawingVertices[^1];
            double radians = angleDegrees * Math.PI / 180.0;
            PointD end = new(
                start.X + length * Math.Cos(radians),
                start.Y + length * Math.Sin(radians));

            _polylineArcAwaitingCenter = false;
            _polylineArcAwaitingEnd = false;
            _polylineArcCenterPoint = null;
            AddPolylineLineSegment(end);
            UpdateDrawingPreview(PointToClient(Cursor.Position));
        }

        private void PromptStandaloneLineLengthAndAngle()
        {
            if (_drawingVertices.Count == 0 ||
                !TryPromptTwoValues("Line", "Length", "Angle (deg)", out double length, out double angleDegrees))
            {
                return;
            }

            PointD start = _drawingVertices[0];
            double radians = angleDegrees * Math.PI / 180.0;
            PointD end = new(
                start.X + length * Math.Cos(radians),
                start.Y + length * Math.Sin(radians));

            CompleteShape(new LineShape(start, end));
        }

        private void PromptRectangleSize()
        {
            if (_drawingVertices.Count == 0 ||
                !TryPromptTwoValues("Rectangle Size", "Length", "Breadth", out double length, out double breadth))
            {
                return;
            }

            PointD start = _drawingVertices[0];
            PointD reference = _currentMouseWorld ?? start;

            double dx = reference.X - start.X;
            double dy = reference.Y - start.Y;

            double xSign = dx < 0.0 ? -1.0 : 1.0;
            double ySign = dy < 0.0 ? -1.0 : 1.0;

            PointD end = new(
                start.X + Math.Abs(length) * xSign,
                start.Y + Math.Abs(breadth) * ySign);

            CompleteShape(new RectangleShape(start, end));
        }

        private void PromptCircleValue()
        {
            if (_drawingVertices.Count == 0)
            {
                return;
            }

            string label = _circleDrawingMode == CircleDrawingMode.CenterDiameter
                ? "Diameter"
                : "Radius";
            if (!TryPromptOneValue("Circle Value", label, out double value) || value <= 0.0)
            {
                return;
            }

            PointD center = _drawingVertices[0];
            double radius = _circleDrawingMode == CircleDrawingMode.CenterDiameter
                ? value / 2.0
                : value;
            CompleteShape(new CircleShape(center, new PointD(center.X + radius, center.Y)));
        }

        private bool TryPromptOneValue(
            string title,
            string label,
            out double value)
        {
            value = 0.0;
            using Form form = CreateValuePromptForm(title, 240, 126);
            Label inputLabel = new()
            {
                Text = label,
                AutoSize = true,
                Location = new Point(12, 16)
            };
            TextBox inputBox = new()
            {
                Location = new Point(86, 12),
                Width = 126
            };
            form.Controls.Add(inputLabel);
            form.Controls.Add(inputBox);
            AddPromptButtons(form);
            form.Shown += (_, _) => inputBox.Focus();

            if (form.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return false;
            }

            return TryParsePromptDouble(inputBox.Text, out value);
        }

        private bool TryPromptTwoValues(
            string title,
            string firstLabel,
            string secondLabel,
            out double firstValue,
            out double secondValue)
        {
            firstValue = 0.0;
            secondValue = 0.0;
            using Form form = CreateValuePromptForm(title, 278, 158);
            Label firstInputLabel = new()
            {
                Text = firstLabel,
                AutoSize = true,
                Location = new Point(12, 16)
            };
            TextBox firstInputBox = new()
            {
                Location = new Point(104, 12),
                Width = 136
            };
            Label secondInputLabel = new()
            {
                Text = secondLabel,
                AutoSize = true,
                Location = new Point(12, 48)
            };
            TextBox secondInputBox = new()
            {
                Location = new Point(104, 44),
                Width = 136
            };
            form.Controls.AddRange(new Control[] { firstInputLabel, firstInputBox, secondInputLabel, secondInputBox });
            AddPromptButtons(form);
            form.Shown += (_, _) => firstInputBox.Focus();

            if (form.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return false;
            }

            return TryParsePromptDouble(firstInputBox.Text, out firstValue) &&
                   TryParsePromptDouble(secondInputBox.Text, out secondValue);
        }

        private static Form CreateValuePromptForm(string title, int width, int height)
        {
            return new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(width, height)
            };
        }

        private static void AddPromptButtons(Form form)
        {
            Button okButton = new()
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(76, 28),
                Location = new Point(form.ClientSize.Width - 168, form.ClientSize.Height - 42)
            };
            Button cancelButton = new()
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(76, 28),
                Location = new Point(form.ClientSize.Width - 86, form.ClientSize.Height - 42)
            };
            form.AcceptButton = okButton;
            form.CancelButton = cancelButton;
            form.Controls.Add(okButton);
            form.Controls.Add(cancelButton);
        }

        private static bool TryParsePromptDouble(string text, out double value)
        {
            return double.TryParse(
                       text,
                       NumberStyles.Float,
                       CultureInfo.CurrentCulture,
                       out value) ||
                   double.TryParse(
                       text,
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out value);
        }

        private void CompleteMultiPointDrawing()
        {
            if (!CanCompleteMultiPointDrawing())
            {
                CancelDrawing();
                return;
            }

            CompleteShape(new PolylineShape(
                _drawingVertices.ToArray(),
                _drawingSegments.ToArray(),
                _activeTool == MapCanvasTool.Polygon));
        }

        private bool CanCompleteMultiPointDrawing()
        {
            int minimumVertices = _activeTool == MapCanvasTool.Polygon ? 3 : 2;
            return (_activeTool == MapCanvasTool.Polyline ||
                    _activeTool == MapCanvasTool.Polygon) &&
                   _drawingVertices.Count >= minimumVertices &&
                   !_polylineArcAwaitingCenter &&
                   !_polylineArcAwaitingEnd &&
                   !_pendingPolylineArcThroughPoint.HasValue;
        }

        private void UpdateDrawingPreview(Point screenPoint)
        {
            if (_activeTool == MapCanvasTool.Select ||
                _drawingVertices.Count == 0)
            {
                return;
            }

            PointD worldPoint = GetCurrentDrawingWorldPoint(screenPoint);
            _previewShape = _activeTool switch
            {
                MapCanvasTool.Line => new LineShape(_drawingVertices[0], worldPoint),
                MapCanvasTool.Rectangle => new RectangleShape(_drawingVertices[0], worldPoint),
                MapCanvasTool.Circle => CreateCirclePreview(worldPoint),
                MapCanvasTool.Arc => CreateArcPreview(worldPoint),
                MapCanvasTool.Polyline => CreateMultiPointPreview(worldPoint, isClosed: false),
                MapCanvasTool.Polygon => CreateMultiPointPreview(worldPoint, isClosed: true),
                _ => null
            };

            // If we're in Center+Diameter mode, attach the raw diameter endpoint
            // to the preview shape so the renderer can draw a diameter preview
            // (and display the diameter value) instead of a radius value.
            if (_previewShape is CircleShape previewCircle &&
                _circleDrawingMode == CircleDrawingMode.CenterDiameter)
            {
                previewCircle.Properties["CenterDiameterEndpoint"] = worldPoint;
            }
            else if (_previewShape is CircleShape diameterCircle &&
                     _circleDrawingMode == CircleDrawingMode.TwoPointDiameter)
            {
                diameterCircle.Properties["DiameterEndpoints"] = new[] { _drawingVertices[0], worldPoint };
            }
            else if (_previewShape is CircleShape threePointCircle &&
                     _circleDrawingMode == CircleDrawingMode.ThreePoint)
            {
                threePointCircle.Properties["SuppressPreviewHelpers"] = true;
            }
            if (_previewShape != null)
            {
                _previewShape.LayerName = _activeDrawingLayerName;
            }

            RequestRender();
        }

        private IShape? CreateCirclePreview(PointD worldPoint)
        {
            if (_circleDrawingMode == CircleDrawingMode.ThreePoint)
            {
                return _drawingVertices.Count switch
                {
                    1 => CreateTwoPointDiameterCircle(_drawingVertices[0], worldPoint),
                    >= 2 => CreateThreePointCircle(_drawingVertices[0], _drawingVertices[1], worldPoint),
                    _ => null
                };
            }

            return CreateCircleShape(_drawingVertices[0], worldPoint);
        }

        private IShape? CreateArcPreview(PointD worldPoint)
        {
            return _drawingVertices.Count switch
            {
                1 => new LineShape(_drawingVertices[0], worldPoint),
                >= 2 when _arcDrawingMode == ArcDrawingMode.CenterStartEnd => ArcShape.FromCenterStartEnd(
                    _drawingVertices[0],
                    _drawingVertices[1],
                    worldPoint),
                >= 2 => ArcShape.FromThreePoints(
                    _drawingVertices[0],
                    _drawingVertices[1],
                    worldPoint),
                _ => null
            };
        }

        private IShape CreateMultiPointPreview(PointD worldPoint, bool isClosed)
        {
            List<PointD> points = new List<PointD>(_drawingVertices);
            List<PolylineShape.PolylineSegment> segments = new List<PolylineShape.PolylineSegment>(_drawingSegments);

            if (_polylineArcAwaitingCenter && points.Count > 0)
            {
                points.Add(worldPoint);
                return new PolylineShape(points, segments, isClosed);
            }

            if (_polylineArcAwaitingEnd &&
                points.Count > 0 &&
                _polylineArcCenterPoint.HasValue)
            {
                ArcShape? arc = ArcShape.FromCenterStartEnd(
                    _polylineArcCenterPoint.Value,
                    points[^1],
                    worldPoint);
                if (arc != null)
                {
                    segments.Add(new PolylineShape.PolylineSegment(
                        PolylineShape.PolylineSegmentKind.Arc,
                        arc.StartPoint,
                        arc.EndPoint,
                        arc));
                    points.Add(worldPoint);
                    return new PolylineShape(points, segments, isClosed);
                }
                // Arc is degenerate; show only what's been drawn so far.
                return new PolylineShape(points, segments, isClosed);
            }

            if (_polylineSegmentMode == PolylineSegmentDrawingMode.Line && points.Count > 0)
            {
                segments.Add(new PolylineShape.PolylineSegment(
                    PolylineShape.PolylineSegmentKind.Line,
                    points[^1],
                    worldPoint));
                points.Add(worldPoint);
                return new PolylineShape(points, segments, isClosed);
            }

            if (_polylineSegmentMode == PolylineSegmentDrawingMode.ThreePointArc && points.Count > 0)
            {
                if (_pendingPolylineArcThroughPoint.HasValue)
                {
                    // Through-point is locked; preview the live arc through the mouse.
                    ArcShape? arc = ArcShape.FromThreePoints(
                        points[^1],
                        _pendingPolylineArcThroughPoint.Value,
                        worldPoint);
                    if (arc != null)
                    {
                        segments.Add(new PolylineShape.PolylineSegment(
                            PolylineShape.PolylineSegmentKind.Arc,
                            arc.StartPoint,
                            arc.EndPoint,
                            arc));
                    }
                    else
                    {
                        segments.Add(new PolylineShape.PolylineSegment(
                            PolylineShape.PolylineSegmentKind.Line,
                            points[^1],
                            worldPoint));
                    }
                }
                else
                {
                    // Through-point not yet picked; show rubber-band line to mouse.
                    segments.Add(new PolylineShape.PolylineSegment(
                        PolylineShape.PolylineSegmentKind.Line,
                        points[^1],
                        worldPoint));
                }

                points.Add(worldPoint);
                return new PolylineShape(points, segments, isClosed);
            }

            // TangentArc mode
            if (points.Count >= 2)
            {
                ArcShape? arc = ArcShape.FromTangentStartEnd(points[^1], GetTangentReferencePoint(), worldPoint);
                if (arc != null)
                {
                    segments.Add(new PolylineShape.PolylineSegment(
                        PolylineShape.PolylineSegmentKind.Arc,
                        arc.StartPoint,
                        arc.EndPoint,
                        arc));
                }
                else
                {
                    segments.Add(new PolylineShape.PolylineSegment(
                        PolylineShape.PolylineSegmentKind.Line,
                        points[^1],
                        worldPoint));
                }

                points.Add(worldPoint);
                return new PolylineShape(points, segments, isClosed);
            }

            points.Add(worldPoint);
            return new PolylineShape(points, segments, isClosed);
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
            _drawingSegments.Clear();
            _polylineSegmentMode = PolylineSegmentDrawingMode.Line;
            _polylineArcAwaitingCenter = false;
            _polylineArcAwaitingEnd = false;
            _polylineArcCenterPoint = null;
            _pendingPolylineArcThroughPoint = null;
            _previewShape = null;
            _currentSnapPoint = null;
            ShapeCompleted?.Invoke(shape);
            UpdateStatusBar();
            RequestRender();
        }

        private void CancelDrawing()
        {
            _drawingVertices.Clear();
            _drawingSegments.Clear();
            _polylineArcAwaitingCenter = false;
            _polylineArcAwaitingEnd = false;
            _polylineArcCenterPoint = null;
            _pendingPolylineArcThroughPoint = null;
            _previewShape = null;
            _currentSnapPoint = null;
            UpdateStatusBar();
            RequestRender();
        }

        private void UndoLastDrawingVertex()
        {
            if (_polylineArcAwaitingEnd)
            {
                _polylineArcAwaitingEnd = false;
                _polylineArcCenterPoint = null;
                UpdateStatusBar();
                RequestRender();
                return;
            }

            if (_polylineArcAwaitingCenter)
            {
                _polylineArcAwaitingCenter = false;
                UpdateStatusBar();
                RequestRender();
                return;
            }

            if (_pendingPolylineArcThroughPoint.HasValue)
            {
                _pendingPolylineArcThroughPoint = null;
                UpdateStatusBar();
                RequestRender();
                return;
            }

            if (_drawingSegments.Count > 0)
            {
                _drawingSegments.RemoveAt(_drawingSegments.Count - 1);
                if (_drawingVertices.Count > 1)
                    _drawingVertices.RemoveAt(_drawingVertices.Count - 1);
                UpdateStatusBar();
                RequestRender();
                return;
            }

            if (_drawingVertices.Count > 0)
            {
                _drawingVertices.RemoveAt(_drawingVertices.Count - 1);
                UpdateStatusBar();
                RequestRender();
            }
        }

        private void ClearCurrentSnapPoint()
        {
            if (_currentSnapPoint == null &&
                _lastSnapCandidateCount == 0 &&
                _lastSnapQueryFeatureCount == 0)
            {
                return;
            }

            _currentSnapPoint = null;
            _lastSnapCandidateCount = 0;
            _lastSnapQueryFeatureCount = 0;
            _lastSnapQueryElapsedMs = 0.0;
            RequestRender();
        }

        private void UpdateCurrentSnapPoint(Point screenPoint)
        {
            if (!_snapEnabled ||
                _activeTool == MapCanvasTool.Select ||
                IsInteractiveNavigation ||
                IsPanBlockedByZoomDebounce)
            {
                ClearCurrentSnapPoint();
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            int queryRadius = Math.Max(
                SnapQueryBoxPixels,
                (int)Math.Ceiling(_snapPickTolerancePixels));
            Rectangle screenQuery = new(
                screenPoint.X - queryRadius,
                screenPoint.Y - queryRadius,
                queryRadius * 2,
                queryRadius * 2);
            RectangleD worldQuery = CreateWorldRectangle(screenQuery);
            PointD mouseWorld = _engine.ScreenToWorld(screenPoint);

            List<IShape> nearbyShapes = _vectorFeatures
                .Where(IsSnapCandidateFeature)
                .Select(feature => feature.Shape)
                .Where(shape => ShapeIntersectsWorldQuery(shape, worldQuery))
                .Take(250)
                .ToList();

            PointD? fromPoint = _drawingVertices.Count > 0 ? _drawingVertices[^1] : null;
            List<SnapPoint> candidates = _snapManager
                .GetSnapCandidates(
                    nearbyShapes,
                    BuildInProgressSnapPoints(mouseWorld),
                    fromPoint)
                .Where(snapPoint => ScreenQueryContainsSnapPoint(screenQuery, snapPoint))
                .Where(snapPoint => !IsSuppressedPreviewSnapPoint(snapPoint))
                .ToList();

            SnapPoint? previousSnapPoint = _currentSnapPoint;
            _currentSnapPoint = _snapManager.FindNearestSnapPointFromList(
                candidates,
                screenPoint,
                _engine,
                _snapPickTolerancePixels);

            stopwatch.Stop();
            _lastSnapQueryFeatureCount = nearbyShapes.Count;
            _lastSnapCandidateCount = candidates.Count;
            _lastSnapQueryElapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            if (!SameSnapPoint(previousSnapPoint, _currentSnapPoint))
            {
                RequestRender();
            }
        }

        private IEnumerable<SnapPoint> BuildInProgressSnapPoints(PointD mouseWorld)
        {
            // --- Committed vertex endpoint / midpoint snaps ---
            for (int index = 0; index < _drawingVertices.Count; index++)
            {
                yield return new SnapPoint(SnapType.Endpoint, _drawingVertices[index], null);
                if (index > 0)
                {
                    PointD previous = _drawingVertices[index - 1];
                    PointD current = _drawingVertices[index];
                    yield return new SnapPoint(
                        SnapType.Midpoint,
                        new PointD((previous.X + current.X) / 2.0, (previous.Y + current.Y) / 2.0),
                        null);
                }
            }

            // --- Self-intersection snaps on committed vertices ---
            foreach (PointD intersection in _snapManager.GetPolylineSelfIntersections(_drawingVertices))
            {
                yield return new SnapPoint(SnapType.Intersection, intersection, null);
            }

            // --- Snap points from committed arc segments (center, midpoint, quadrants) ---
            foreach (PolylineShape.PolylineSegment segment in _drawingSegments)
            {
                if (segment.Kind != PolylineShape.PolylineSegmentKind.Arc || segment.Arc == null)
                    continue;

                ArcShape arc = segment.Arc;
                yield return new SnapPoint(SnapType.Center, arc.Center, null);
                yield return new SnapPoint(SnapType.Midpoint, arc.MidPoint, null);

                double[] quadrantAngles = [0.0, Math.PI / 2.0, Math.PI, Math.PI * 1.5];
                foreach (double angle in quadrantAngles)
                {
                    if (ArcShape.AngleLiesOnSweepPublic(angle, arc.StartAngleRadians, arc.SweepAngleRadians))
                    {
                        yield return new SnapPoint(
                            SnapType.Quadrant,
                            new PointD(
                                arc.Center.X + arc.Radius * Math.Cos(angle),
                                arc.Center.Y + arc.Radius * Math.Sin(angle)),
                            null);
                    }
                }
            }

            // --- Pending 3-point arc through-point (locked in, awaiting end-point click) ---
            if (_pendingPolylineArcThroughPoint.HasValue)
            {
                yield return new SnapPoint(SnapType.Endpoint, _pendingPolylineArcThroughPoint.Value, null);
            }

            // NOTE: Preview (rubber-band) shapes do NOT contribute snap points.
            // Only the committed/finished portions of the in-progress shape are snappable,
            // so the live cursor doesn't snap to its own moving geometry.
        }

        private bool IsSuppressedPreviewSnapPoint(SnapPoint snapPoint)
        {
            return _activeTool == MapCanvasTool.Circle &&
                   _drawingVertices.Count > 0 &&
                   snapPoint.Type == SnapType.Quadrant &&
                   ReferenceEquals(snapPoint.ParentShape, _previewShape);
        }

        private bool ScreenQueryContainsSnapPoint(Rectangle screenQuery, SnapPoint snapPoint)
        {
            PointD screen = _engine.WorldToScreen(snapPoint.Position);
            return screenQuery.Contains(
                new Point(
                    (int)Math.Round(screen.X),
                    (int)Math.Round(screen.Y)));
        }

        private static bool IsSnapCandidateFeature(CanvasFeature feature)
        {
            return feature.Shape.IsVisible &&
                   feature.Layer?.IsVisible != false &&
                   feature.Layer?.IsLocked != true;
        }

        private static bool ShapeIntersectsWorldQuery(IShape shape, RectangleD worldQuery)
        {
            RectangleD bounds = shape.GetBoundingBox();
            if (bounds.Width == 0.0 && bounds.Height == 0.0)
            {
                return worldQuery.Contains(new PointD(bounds.X, bounds.Y));
            }

            return bounds.IntersectsWith(worldQuery);
        }

        private static bool SameSnapPoint(SnapPoint? first, SnapPoint? second)
        {
            if (first == null || second == null)
            {
                return first == null && second == null;
            }

            return first.Type == second.Type &&
                   SameWorldPoint(first.Position, second.Position);
        }

        private static bool SameWorldPoint(PointD first, PointD second)
        {
            return Math.Abs(first.X - second.X) <= 1e-9 &&
                   Math.Abs(first.Y - second.Y) <= 1e-9;
        }

        private static double DistanceWorld(PointD first, PointD second)
        {
            double dx = second.X - first.X;
            double dy = second.Y - first.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void DrawSnapGlyph(Graphics graphics)
        {
            if (_currentSnapPoint == null)
            {
                return;
            }

            PointD screen = _engine.WorldToScreen(_currentSnapPoint.Position);
            if (!double.IsFinite(screen.X) || !double.IsFinite(screen.Y))
            {
                return;
            }

            float size = _snapGlyphSizePixels;
            float half = size / 2f;
            PointF center = new((float)screen.X, (float)screen.Y);
            // Use one contrasting goldish-green outline color with no fill.
            Color strokeColor = Color.FromArgb(255, 120, 185, 20);
            using Pen pen = new(strokeColor, 2.35f)
            {
                LineJoin = LineJoin.Miter
            };

            switch (_currentSnapPoint.Type)
            {
                case SnapType.Endpoint:
                    RectangleF endpointRect = new(center.X - half, center.Y - half, size, size);
                    graphics.DrawRectangle(
                        pen,
                        endpointRect.X,
                        endpointRect.Y,
                        endpointRect.Width,
                        endpointRect.Height);
                    break;

                case SnapType.Midpoint:
                    PointF[] triangle =
                    [
                        new(center.X, center.Y - half),
                        new(center.X + half, center.Y + half),
                        new(center.X - half, center.Y + half)
                    ];
                    graphics.DrawPolygon(pen, triangle);
                    break;

                case SnapType.Center:
                    RectangleF circleRect = new(center.X - half, center.Y - half, size, size);
                    graphics.DrawEllipse(pen, circleRect);
                    break;

                case SnapType.Quadrant:
                    PointF[] diamond =
                    [
                        new(center.X, center.Y - half),
                        new(center.X + half, center.Y),
                        new(center.X, center.Y + half),
                        new(center.X - half, center.Y)
                    ];
                    graphics.DrawPolygon(pen, diamond);
                    break;

                case SnapType.Intersection:
                    // Draw a diagonal cross for intersection snaps.
                    graphics.DrawLine(pen, center.X - half, center.Y - half, center.X + half, center.Y + half);
                    graphics.DrawLine(pen, center.X - half, center.Y + half, center.X + half, center.Y - half);
                    break;

                case SnapType.Perpendicular:
                    // Simple L: vertical bar on the left, horizontal bar at the bottom.
                    graphics.DrawLine(pen, center.X - half, center.Y - half, center.X - half, center.Y + half);
                    graphics.DrawLine(pen, center.X - half, center.Y + half, center.X + half, center.Y + half);
                    break;
            }
        }

        private void DrawCircleDiameterPreview(Graphics graphics)
        {
            if (_activeTool != MapCanvasTool.Circle ||
                _circleDrawingMode != CircleDrawingMode.CenterDiameter ||
                _drawingVertices.Count == 0 ||
                _previewShape == null)
            {
                return;
            }

            if (_previewShape is not CircleShape circle)
            {
                return;
            }

            // Draw a preview line from center to the current preview point and show diameter value at midpoint
            if (_currentMouseWorld == null)
            {
                return;
            }

            PointF screenCenter = ToScreenPointF(circle.Center);
            PointF screenEdge = ToScreenPointF(_currentMouseWorld.Value);

            if (!double.IsFinite(screenCenter.X) || !double.IsFinite(screenCenter.Y) ||
                !double.IsFinite(screenEdge.X) || !double.IsFinite(screenEdge.Y))
            {
                return;
            }

            using Pen previewPen = new(Color.FromArgb(200, 0, 120, 215), 1.5f);
            previewPen.DashStyle = DashStyle.Dash;
            graphics.DrawLine(previewPen, screenCenter, screenEdge);

            // Diameter value (world units) = 2 * radius
            double worldRadius = Math.Sqrt(
                (circle.Center.X - _currentMouseWorld.Value.X) * (circle.Center.X - _currentMouseWorld.Value.X) +
                (circle.Center.Y - _currentMouseWorld.Value.Y) * (circle.Center.Y - _currentMouseWorld.Value.Y));
            double diameter = worldRadius * 2.0;

            string text = diameter.ToString("0.##", CultureInfo.InvariantCulture);
            Font font = _debugOverlayFont;
            Size textSize = TextRenderer.MeasureText(text, font);

            // Midpoint of the preview line (center -> edge)
            PointF mid = new PointF((screenCenter.X + screenEdge.X) / 2f, (screenCenter.Y + screenEdge.Y) / 2f);

            RectangleF textBg = new RectangleF(mid.X - textSize.Width / 2f - 4f, mid.Y - textSize.Height / 2f - 2f, textSize.Width + 8f, textSize.Height + 4f);
            using Brush bg = new SolidBrush(Color.FromArgb(200, Color.White));
            using Pen border = new(Color.FromArgb(160, 0, 0, 0));
            graphics.FillRectangle(bg, textBg);
            graphics.DrawRectangle(border, Rectangle.Round(textBg));
            using Brush textBrush = new SolidBrush(Color.FromArgb(220, 0, 0, 0));
            TextRenderer.DrawText(
                graphics,
                text,
                font,
                Rectangle.Round(textBg),
                Color.FromArgb(220, 0, 0, 0),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
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

            CanvasFeature? hitFeature = FindClickHitFeature(worldPoint, worldTolerance);

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
            RefreshVectorCacheForCurrentViewAsync();
            UpdateStatusBar();
            NotifySelectedCanvasObjectsChanged();
        }

        private CanvasFeature? FindClickHitFeature(PointD worldPoint, double toleranceWorld)
        {
            NtsGeometry pickPoint = SelectionGeometryFactory.CreatePoint(
                new NtsCoordinate(worldPoint.X, worldPoint.Y));
            List<(CanvasFeature Feature, NtsGeometry Geometry)> candidates = _vectorFeatures
                .Where(IsSelectableDrawingFeature)
                .Reverse()
                .Select(feature => (Feature: feature, Geometry: CreateSelectionGeometry(feature.Shape)))
                .Where(candidate => !candidate.Geometry.IsEmpty)
                .ToList();

            CanvasFeature? exactHit = candidates
                .Where(candidate => IsSelectableImportedCadastralParcel(candidate.Feature) &&
                                    candidate.Geometry.Covers(pickPoint))
                .OrderBy(candidate => GetSelectionGeometryPickArea(candidate.Geometry))
                .Select(candidate => candidate.Feature)
                .FirstOrDefault();
            if (exactHit != null)
                return exactHit;

            return candidates
                .Select(candidate => new
                {
                    candidate.Feature,
                    candidate.Geometry,
                    ClickGeometry = CreateClickSelectionGeometry(candidate.Feature, candidate.Geometry)
                })
                .Where(candidate => !candidate.ClickGeometry.IsEmpty)
                .Select(candidate => new
                {
                    candidate.Feature,
                    candidate.Geometry,
                    Distance = candidate.ClickGeometry.Distance(pickPoint)
                })
                .Where(candidate => candidate.Distance <= toleranceWorld)
                .OrderBy(candidate => candidate.Distance)
                .ThenBy(candidate => GetSelectionGeometryPickArea(candidate.Geometry))
                .Select(candidate => candidate.Feature)
                .FirstOrDefault();
        }

        private static NtsGeometry CreateClickSelectionGeometry(CanvasFeature feature, NtsGeometry selectionGeometry)
        {
            if (IsSelectableImportedCadastralParcel(feature) || selectionGeometry.Area <= 0)
            {
                return selectionGeometry;
            }

            return selectionGeometry.Boundary;
        }

        private static double GetSelectionGeometryPickArea(NtsGeometry geometry)
        {
            if (geometry.Area > 0)
                return geometry.Area;

            NetTopologySuite.Geometries.Envelope envelope = geometry.EnvelopeInternal;
            return envelope.IsNull
                ? double.MaxValue
                : Math.Max(0.0, envelope.Width * envelope.Height);
        }

        private void ReplaceSelectedObjects(IEnumerable<CanvasFeature> selectedFeatures)
        {
            _selectedShapeIds.Clear();
            foreach (CanvasFeature feature in selectedFeatures)
            {
                _selectedShapeIds.Add(feature.Shape.Id);
            }

            ApplySelectedShapeFlags();
            RefreshVectorCacheForCurrentViewAsync();
            UpdateStatusBar();
            NotifySelectedCanvasObjectsChanged();
        }

        private static bool TryGetCombinedFeatureBounds(
            IReadOnlyList<CanvasFeature> features,
            out RectangleD bounds)
        {
            bounds = default;
            bool hasBounds = false;
            double left = 0;
            double right = 0;
            double bottom = 0;
            double top = 0;

            foreach (CanvasFeature feature in features)
            {
                if (!TryNormalizeWorldBounds(feature.Shape.GetBoundingBox(), out RectangleD featureBounds))
                    continue;

                if (!hasBounds)
                {
                    left = featureBounds.Left;
                    right = featureBounds.Right;
                    bottom = featureBounds.Bottom;
                    top = featureBounds.Top;
                    hasBounds = true;
                    continue;
                }

                left = Math.Min(left, featureBounds.Left);
                right = Math.Max(right, featureBounds.Right);
                bottom = Math.Min(bottom, featureBounds.Bottom);
                top = Math.Max(top, featureBounds.Top);
            }

            if (!hasBounds || right <= left || top <= bottom)
                return false;

            bounds = new RectangleD(left, bottom, right - left, top - bottom);
            return true;
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
            RefreshVectorCacheForCurrentViewAsync();
            UpdateStatusBar();
            NotifySelectedCanvasObjectsChanged();
        }

        private void ClearSelectedObjects()
        {
            if (_selectedShapeIds.Count == 0)
                return;

            _selectedShapeIds.Clear();
            ApplySelectedShapeFlags();
            RefreshVectorCacheForCurrentViewAsync();
            NotifySelectedCanvasObjectsChanged();
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

        private void ObjectSelectionContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            _mnuAssignParcelData.Enabled = SelectionContainsImportedCadastralParcel();
            _mnuDeleteSelectedObjects.Enabled = SelectionContainsEditableObject();
        }

        private bool SelectionContainsImportedCadastralParcel()
        {
            return _vectorFeatures.Any(feature =>
                _selectedShapeIds.Contains(feature.Shape.Id) &&
                IsSelectableImportedCadastralParcel(feature));
        }

        private bool SelectionContainsEditableObject()
        {
            return _vectorFeatures.Any(feature =>
                _selectedShapeIds.Contains(feature.Shape.Id) &&
                feature.Layer?.IsLocked != true);
        }

        public void ClearSelectionAfterDelete()
        {
            _selectedShapeIds.Clear();
            ApplySelectedShapeFlags();
            NotifySelectedCanvasObjectsChanged();
            RequestRender();
        }

        private void NotifySelectedCanvasObjectsChanged()
        {
            SelectedCanvasObjectsChanged?.Invoke(_selectedShapeIds.ToArray());
        }

        private bool IsSelectableDrawingFeature(CanvasFeature feature)
        {
            return feature.Shape.IsVisible &&
                   feature.Layer?.IsVisible != false &&
                   feature.Layer != null &&
                   ((feature.Layer.IsLocked != true &&
                     CanvasLayerTreeService.IsDrawingMarkupLayer(feature.Layer)) ||
                    IsSelectableImportedCadastralParcel(feature));
        }

        private static bool IsSelectableImportedCadastralParcel(CanvasFeature feature)
        {
            return string.Equals(feature.CanvasObject.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(feature.CanvasObject.GeometryMetadataJson) &&
                   feature.CanvasObject.GeometryMetadataJson.Contains("CadastralParcel", StringComparison.OrdinalIgnoreCase);
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
            NtsGeometry selectionGeometry = CreateSelectionTestGeometry(shape);
            if (selectionGeometry.IsEmpty)
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

            return selectionGeometry.Intersects(selectionPolygon);
        }

        private static bool ContainsSelectionGeometry(RectangleD selectionBounds, IShape shape)
        {
            NtsGeometry selectionGeometry = CreateSelectionTestGeometry(shape);
            if (selectionGeometry.IsEmpty)
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

            return selectionPolygon.Covers(selectionGeometry);
        }

        private static NtsGeometry CreateSelectionTestGeometry(IShape shape)
        {
            NtsGeometry shapeGeometry = CreateSelectionGeometry(shape);
            return shapeGeometry is NtsPolygon or NetTopologySuite.Geometries.MultiPolygon
                ? shapeGeometry.Boundary
                : shapeGeometry;
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
                DonutPolygonShape donut => donut.ToGeometry(),
                ArcShape arc => SelectionGeometryFactory.CreateLineString(
                    arc.SamplePoints(96)
                        .Select(point => new NtsCoordinate(point.X, point.Y))
                        .ToArray()),
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

            if (polyline.Segments.Count > 0)
            {
                NtsCoordinate[] sampledCoordinates = polyline.GetGeometryPoints(24)
                    .Select(vertex => new NtsCoordinate(vertex.X, vertex.Y))
                    .ToArray();

                if (polyline.IsClosed && sampledCoordinates.Length >= 3)
                {
                    return SelectionGeometryFactory.CreatePolygon(CloseRing(sampledCoordinates));
                }

                return SelectionGeometryFactory.CreateLineString(sampledCoordinates);
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
            Color borderColor = isWindowSelection
                ? Color.FromArgb(33, 148, 204)   // window: AutoCAD blue
                : Color.FromArgb(30, 168, 50);   // crossing: AutoCAD green
            Color fillColor = isWindowSelection
                ? Color.FromArgb(40, 33, 148, 204)
                : Color.FromArgb(40, 30, 168, 50);

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
            string coordinatesText = _currentMouseWorld.HasValue
                ? $"E: {_currentMouseWorld.Value.X:F4}    N: {_currentMouseWorld.Value.Y:F4}"
                : "E: --    N: --";

            string modeText = GetModeText();
            _commandService.SetPrompt(GetCommandPromptText());
            StatusChanged?.Invoke(coordinatesText, modeText, _engine.ZoomScale);
        }

        private string GetModeText()
        {
            if (_isZooming && _zoomDirection != null)
                return $"Mode: Zooming {_zoomDirection}";

            if (_activeTool != MapCanvasTool.Select)
            {
                return _drawingVertices.Count == 0
                    ? $"Mode: Draw {_activeTool}"
                    : $"Mode: Draw {_activeTool} ({_drawingVertices.Count})";
            }

            if (_isSelectingZoomWindow || _zoomWindowActive)
                return "Mode: Zoom Window";

            if (_isPanning || _panToolActive)
                return "Mode: Pan";

            return "Mode: Ready";
        }

        private string GetCommandPromptText()
        {
            if (_activeTool == MapCanvasTool.Select || _activeTool == MapCanvasTool.Point)
                return "Ready";

            switch (_activeTool)
            {
                case MapCanvasTool.Line:
                    return _drawingVertices.Count == 0
                        ? "Line: Click first point"
                        : "Line: Click end point  [Right-click for options]";

                case MapCanvasTool.Rectangle:
                    return _drawingVertices.Count == 0
                        ? "Rectangle: Click first corner"
                        : "Rectangle: Click opposite corner  [Right-click for options]";

                case MapCanvasTool.Circle:
                    return _circleDrawingMode switch
                    {
                        CircleDrawingMode.CenterRadius => _drawingVertices.Count == 0
                            ? "Circle: Click center point"
                            : "Circle: Click or enter radius point  [Right-click for options]",
                        CircleDrawingMode.CenterDiameter => _drawingVertices.Count == 0
                            ? "Circle: Click center point"
                            : "Circle: Click diameter end point  [Right-click for options]",
                        CircleDrawingMode.TwoPointDiameter => _drawingVertices.Count == 0
                            ? "Circle (2-pt): Click first diameter end"
                            : "Circle (2-pt): Click second diameter end",
                        CircleDrawingMode.ThreePoint => _drawingVertices.Count == 0
                            ? "Circle (3-pt): Click first point"
                            : _drawingVertices.Count == 1
                                ? "Circle (3-pt): Click second point"
                                : "Circle (3-pt): Click third point",
                        _ => "Circle: Click to draw"
                    };

                case MapCanvasTool.Arc:
                    return _arcDrawingMode switch
                    {
                        ArcDrawingMode.ThreePoint => _drawingVertices.Count == 0
                            ? "Arc (3-pt): Click start point"
                            : _drawingVertices.Count == 1
                                ? "Arc (3-pt): Click through point"
                                : "Arc (3-pt): Click end point",
                        ArcDrawingMode.CenterStartEnd => _drawingVertices.Count == 0
                            ? "Arc: Click center point"
                            : _drawingVertices.Count == 1
                                ? "Arc: Click start point"
                                : "Arc: Click end point",
                        _ => "Arc: Click to draw"
                    };

                case MapCanvasTool.Polyline:
                case MapCanvasTool.Polygon:
                    if (_polylineArcAwaitingCenter)
                        return $"{_activeTool}: Click arc center point  [Backspace to cancel]";
                    if (_polylineArcAwaitingEnd)
                        return $"{_activeTool}: Click arc end point  [Backspace to cancel]";
                    if (_drawingVertices.Count == 0)
                        return $"{_activeTool}: Click first point";
                    if (_polylineSegmentMode == PolylineSegmentDrawingMode.ThreePointArc)
                        return _pendingPolylineArcThroughPoint.HasValue
                            ? $"{_activeTool} (3-pt Arc): Click end point  [Backspace to undo]"
                            : $"{_activeTool} (3-pt Arc): Click through point  [Right-click: options / Finish / Cancel]";
                    if (_polylineSegmentMode == PolylineSegmentDrawingMode.TangentArc)
                        return $"{_activeTool} (Tangent Arc): Click end point  [Right-click: options / Finish / Cancel]";
                    return $"{_activeTool}: Click next point  [Right-click: options / Finish / Cancel]";
            }

            return "Ready";
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
            _isPanning ||
            _isZooming ||
            _vectorCacheRefreshPending ||
            _holdVectorPanFrameUntilRefresh ||
            _holdVectorZoomFrameUntilRefresh;

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

            if (_isPanning)
            {
                return null;
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

            if (_isPanning)
            {
                return null;
            }

            if (_holdVectorPanFrameUntilRefresh &&
                _renderer.TryGetVectorPanFrame(
                    _heldVectorPanDelta,
                    out RasterRenderFrame heldPanFrame))
            {
                source = CanvasFrameSource.PanCache;
                return heldPanFrame;
            }

            if (_holdVectorPanFrameUntilRefresh)
            {
                return null;
            }

            if (_isZooming &&
                _renderer.TryGetVectorZoomFrame(out RasterRenderFrame zoomFrame))
            {
                source = CanvasFrameSource.ZoomCache;
                return zoomFrame;
            }

            if (_isZooming)
            {
                return null;
            }

            if (_holdVectorZoomFrameUntilRefresh &&
                _renderer.TryGetVectorZoomFrame(out RasterRenderFrame heldZoomFrame))
            {
                source = CanvasFrameSource.HeldZoomCache;
                return heldZoomFrame;
            }

            if (_holdVectorZoomFrameUntilRefresh)
            {
                return null;
            }

            if (!_isSelectingZoomWindow &&
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
                $"Snap {(_snapEnabled ? "on" : "off")}  Ortho {(_orthoModeEnabled ? "on" : "off")}  glyph {_snapGlyphSizePixels:0.#} px  tolerance {_snapPickTolerancePixels:0.#} px  query features {_lastSnapQueryFeatureCount}  candidates {_lastSnapCandidateCount}  {_lastSnapQueryElapsedMs:0.00} ms  current {_currentSnapPoint?.Type.ToString() ?? "none"}",
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
            List<string> items = new List<string>();
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
            _holdVectorZoomFrameUntilRefresh = true;
            RefreshRasterCacheForCurrentViewAsync(endZoomWhenComplete: true);
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
                CancelPendingVectorRender();
                bool reuseHeldVectorZoomFrame = false;
                if (_holdVectorZoomFrameUntilRefresh &&
                    _renderer.TryGetVectorZoomFrame(out RasterRenderFrame heldVectorZoomFrame))
                {
                    heldVectorZoomFrame.Dispose();
                    reuseHeldVectorZoomFrame = true;
                }

                _holdZoomStartFrameUntilRasterRefresh = false;
                _rasterDeferredRenderer.BeginZoom(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine);
                if (!reuseHeldVectorZoomFrame)
                {
                    _holdVectorZoomFrameUntilRefresh = false;
                    EnsureVectorZoomSnapshot();
                }
            }
            else if (_rasterCacheRefreshPending || _vectorCacheRefreshPending)
            {
                CancelPendingRasterRender();
                CancelPendingVectorRender();
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

        private void CancelPendingVectorRender()
        {
            CancellationTokenSource? previousCancellation = _vectorRenderCancellation;
            _vectorRenderCancellation = null;
            previousCancellation?.Cancel();
            previousCancellation?.Dispose();
            _vectorCacheRefreshPending = false;
        }

        private void EnsureVectorPanSnapshot()
        {
            if (_renderer.BeginVectorPan(canvasSurface.Size))
            {
                return;
            }

            try
            {
                _renderer.RefreshVectorCache(canvasSurface.Size);
                _renderer.BeginVectorPan(canvasSurface.Size);
            }
            catch (ObjectDisposedException)
            {
                // The control or renderer was disposed while preparing the pan snapshot.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Vector pan snapshot preparation failed: {ex.Message}");
            }
        }

        private void EnsureVectorZoomSnapshot()
        {
            if (_renderer.BeginVectorZoom(canvasSurface.Size))
            {
                return;
            }

            try
            {
                _renderer.RefreshVectorCache(canvasSurface.Size);
                _renderer.BeginVectorZoom(canvasSurface.Size);
            }
            catch (ObjectDisposedException)
            {
                // The control or renderer was disposed while preparing the zoom snapshot.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Vector zoom snapshot preparation failed: {ex.Message}");
            }
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
                _holdVectorPanFrameUntilRefresh = false;
                _holdVectorZoomFrameUntilRefresh = false;
                _heldVectorPanDelta = PointF.Empty;
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
            CancelPendingVectorRender();
            _rasterDeferredRenderer.EndZoom();
            _renderer.EndVectorZoom();
            _isZooming = false;
            _zoomDirection = null;
            _rasterCacheRefreshPending = false;
            _suppressStaleRasterFrameUntilFreshRender = false;
            _holdVectorZoomFrameUntilRefresh = false;
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

        private async void RefreshVectorCacheForCurrentViewAsync()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (_isPanning)
            {
                return;
            }

            CancelPendingVectorRender();

            CancellationTokenSource cancellation = new();
            _vectorRenderCancellation = cancellation;
            int generation = ++_vectorRenderGeneration;
            _vectorCacheRefreshPending = true;

            try
            {
                bool refreshed = await _renderer.RefreshVectorCacheAsync(
                    canvasSurface.Size,
                    cancellation.Token);

                if (!refreshed ||
                    cancellation.IsCancellationRequested ||
                    generation != _vectorRenderGeneration ||
                    IsDisposed ||
                    Disposing)
                {
                    return;
                }

                _renderer.EndVectorZoom();
                _holdVectorPanFrameUntilRefresh = false;
                _holdVectorZoomFrameUntilRefresh = false;
                _heldVectorPanDelta = PointF.Empty;

                if (IsHandleCreated)
                {
                    BeginInvoke((MethodInvoker)RequestRender);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during fast interaction sequences.
            }
            catch (ObjectDisposedException)
            {
                // Canvas disposed while vector render was in flight.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Vector cache refresh failed: {ex.Message}");
            }
            finally
            {
                if (ReferenceEquals(_vectorRenderCancellation, cancellation))
                {
                    _vectorRenderCancellation = null;
                    _vectorCacheRefreshPending = false;
                }

                cancellation.Dispose();
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
                        RefreshVectorCacheForCurrentViewAsync();
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
