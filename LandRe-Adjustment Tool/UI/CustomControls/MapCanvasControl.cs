using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
    public enum CanvasObjectAssignmentKind
    {
        Parcel,
        Road,
        Block
    }

    public sealed class CanvasCreateFeaturesMenuOpeningEventArgs : EventArgs
    {
        public CanvasCreateFeaturesMenuOpeningEventArgs(
            IReadOnlyList<Guid> selectedObjectIds,
            ToolStripMenuItem menuItem)
        {
            SelectedObjectIds = selectedObjectIds;
            MenuItem = menuItem;
        }

        public IReadOnlyList<Guid> SelectedObjectIds { get; }

        public ToolStripMenuItem MenuItem { get; }
    }

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

        private enum SelectionGripKind
        {
            Vertex,
            SegmentMidpoint,
            ArcMidpoint,
            CircleCenter,
            CircleQuadrant,
            EllipseCenter,
            EllipseQuadrant,
            TextPosition,
            GeometricCenter
        }

        private enum SelectionGripGlyph
        {
            Square,
            SegmentRectangle,
            Diamond
        }

        private const int ZoomSettleIntervalMs = 150;
        private const int ObjectSelectionTolerancePixels = 3;
        private const int SnapQueryBoxPixels = 10;
        private const double GripHitTolerancePixels = 8.0;
        private const float GripSquareSizePixels = 8.0f;
        private const float GripSegmentLengthPixels = 16.0f;
        private const float GripSegmentThicknessPixels = 4.0f;
        private const double DefaultSnapPickTolerancePixels = 8.0;
        private const float DefaultSnapGlyphSizePixels = 14.0f;
        private const double CommonGripToleranceWorld = 0.005;
        private const double ActiveGripSnapSuppressionPixels = 4.0;
        private const double ScreenPixelsPerMetre = 96.0 / 0.0254;
        private const double MaxSnapScaleDenominator = 50000.0;
        private const double SelectionZoomPadding = 0.72;
        private const double ObjectExtentZoomPadding = 0.86;
        private const double SelectionZoomBoundsMarginFactor = 0.18;
        private const double MinimumSelectionZoomWorldSpan = 100.0;
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
        // Raised once for a whole batch of edited shapes (move/grip/text) so the
        // host persists and reloads ONCE instead of per shape.
        public event Action<IReadOnlyList<IShape>>? ShapesEdited;
        public event Action? SelectToolRequested;
        public event Action<IReadOnlyList<Guid>>? SelectedObjectsDeleteRequested;
        public event Action<CanvasObjectAssignmentKind>? SelectedObjectsAssignDataRequested;
        public event Action<IReadOnlyList<Guid>>? SelectedCanvasObjectsChanged;
        public event EventHandler<CanvasCreateFeaturesMenuOpeningEventArgs>? SelectedObjectsCreateFeaturesMenuOpening;

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
        private int _renderUpdateBatchDepth;
        private bool _renderRequestPending;
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
        private Bitmap? _compositePanBitmap;
        private Bitmap? _gridPanBitmap;
        private Bitmap? _refreshHoldFrame;
        private GridPanPadding _gridPanPadding;
        private bool _snapEnabled = true;
        private bool _orthoModeEnabled;
        private bool _applicationEditLocked;
        private double _snapPickTolerancePixels = DefaultSnapPickTolerancePixels;
        private float _snapGlyphSizePixels = DefaultSnapGlyphSizePixels;
        private SnapPoint? _currentSnapPoint;
        private int _lastSnapQueryFeatureCount;
        private int _lastSnapCandidateCount;
        private double _lastSnapQueryElapsedMs;
        private MapCanvasTool _activeTool = MapCanvasTool.Select;
        private ArcDrawingMode _arcDrawingMode = ArcDrawingMode.ThreePoint;
        private bool _centerStartEndArcClockwise;
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
        private Dictionary<int, CanvasLayer> _vectorLayersById = new();
        private readonly HashSet<Guid> _selectedShapeIds = new HashSet<Guid>();
        private bool _isSelectingObjects;
        private Point _objectSelectionStart;
        private Point _objectSelectionCurrent;
        private IReadOnlyList<CanvasFeature> _immediateEditedOverlayFeatures = Array.Empty<CanvasFeature>();
        private Guid _pendingDrawnShapeId = Guid.Empty;
        private readonly HashSet<Guid> _pendingEditedShapeIds = new();
        private IShape? _justCompletedShape;
        private CanvasLayer? _justCompletedShapeLayer;
        private SelectionGrip? _hoveredSelectionGrip;
        private ActiveGripEdit? _activeGripEdit;
        private IShape? _previewShape;
        private string _activeDrawingLayerName = "Features";
        private CanvasLayer? _activeDrawingLayer;
        private Cursor? _panCursor;
        private Cursor? _selectionCursor;
        private Cursor? _zoomInCursor;
        private Cursor? _zoomOutCursor;
        private Cursor? _zoomWindowCursor;
        private readonly ContextMenuStrip _objectSelectionContextMenu = new();
        private readonly ContextMenuStrip _drawingOptionsContextMenu = new();
        private readonly ToolStripMenuItem _mnuAssignData = new("Assign Data");
        private readonly ToolStripMenuItem _mnuDeleteSelectedObjects = new("Delete Selected Object(s)");
        private readonly ToolStripMenuItem _mnuEditText = new("Edit Text");
        private readonly ToolStripMenuItem _mnuCreateFeaturesFromSelection = new("Create Features...");
        private readonly ToolStripMenuItem _mnuMoveSelectedObjects = new("Move object(s)");
        private CanvasObjectAssignmentKind? _currentSelectionAssignmentKind;
        private MoveOperation? _activeMoveOperation;
        private Bitmap? _movePreviewBitmap;
        private PointF _movePreviewBitmapReferenceScreen;
        private double _movePreviewBitmapScale;
        // Frozen snapshot of the whole canvas captured when a geometric-center
        // (whole-shape) grip move starts. While that move is active only this
        // bitmap and the moving preview are painted — no other rendering.
        private Bitmap? _gripMoveFreezeBitmap;
        private TextBox? _activeTextEditor;
        private PointD? _activeTextAnchorWorld;
        private string _activeTextEditorFontName = "Nirmala UI";
        private float _activeTextEditorBaseFontSize = 6.0f;
        private bool _activeTextEditorScalesWithZoom = true;
        private string _activeTextEditorAlignment = "Left Top";
        private bool _textEditorCompleting;
        private bool _textEditorJustCancelled;
        private bool _suppressNextCanvasMouseDownAfterTextCancel;
        private CanvasFeature? _editingTextFeature;
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
            _mnuAssignData.Click += (_, _) => RequestAssignDataForSelectedObjects();
            _mnuDeleteSelectedObjects.Click += (_, _) => RequestDeleteSelectedObjects();
            _mnuEditText.Click += (_, _) => BeginTextEditFromContextMenu();
            _mnuMoveSelectedObjects.Click += (_, _) => BeginMoveSelectedObjectsFromContextMenu();
            _objectSelectionContextMenu.Items.Add(_mnuEditText);
            _objectSelectionContextMenu.Items.Add(new ToolStripSeparator());
            _objectSelectionContextMenu.Items.Add(_mnuMoveSelectedObjects);
            _objectSelectionContextMenu.Items.Add(_mnuAssignData);
            _objectSelectionContextMenu.Items.Add(_mnuCreateFeaturesFromSelection);
            _objectSelectionContextMenu.Items.Add(new ToolStripSeparator());
            _objectSelectionContextMenu.Items.Add(_mnuDeleteSelectedObjects);
            _drawingOptionsContextMenu.Closed += (_, _) => canvasSurface.Focus();
            WireInteractionEvents();
            UpdateStatusBar();
        }

        private sealed class SelectionGrip
        {
            public required CanvasFeature Feature { get; init; }
            public required IShape Shape { get; init; }
            public required SelectionGripKind Kind { get; init; }
            public required SelectionGripGlyph Glyph { get; init; }
            public required PointD Position { get; init; }
            public PointD SegmentStart { get; init; }
            public PointD SegmentEnd { get; init; }
            public int VertexIndex { get; init; } = -1;
            public int SegmentIndex { get; init; } = -1;
            public int AuxiliaryIndex { get; init; } = -1;
        }

        private sealed class ActiveGripEdit
        {
            public ActiveGripEdit(
                SelectionGrip grip,
                IShape originalShape,
                IShape previewShape,
                SelectionGrip previewGrip,
                Point startScreenPoint,
                IReadOnlyList<LinkedGripEdit> linkedEdits)
            {
                Grip = grip;
                OriginalShape = originalShape;
                PreviewShape = previewShape;
                PreviewGrip = previewGrip;
                StartScreenPoint = startScreenPoint;
                CurrentWorldPoint = grip.Position;
                LinkedEdits = linkedEdits;
            }

            public SelectionGrip Grip { get; }
            public IShape OriginalShape { get; }
            public IShape PreviewShape { get; }
            public SelectionGrip PreviewGrip { get; }
            public Point StartScreenPoint { get; }
            public PointD CurrentWorldPoint { get; set; }
            public bool HasPointerMoved { get; set; }
            public bool AwaitingClickCommit { get; set; }
            public IReadOnlyList<LinkedGripEdit> LinkedEdits { get; }
        }

        private sealed class LinkedGripEdit
        {
            public LinkedGripEdit(SelectionGrip grip)
            {
                Grip = grip;
                OriginalShape = grip.Shape.Clone();
                PreviewShape = CreateGripEditPreviewShape(grip.Shape);
                PreviewGrip = CloneGripForShape(grip, PreviewShape);
            }

            public SelectionGrip Grip { get; }
            public IShape OriginalShape { get; }
            public IShape PreviewShape { get; }
            public SelectionGrip PreviewGrip { get; }
        }

        private enum MoveOperationPhase
        {
            AwaitingReference,
            AwaitingDestination
        }

        private sealed class MoveOperation
        {
            public required IReadOnlyList<MoveItem> Items { get; init; }
            public MoveOperationPhase Phase { get; set; } = MoveOperationPhase.AwaitingReference;
            public PointD ReferenceWorld { get; set; }
        }

        private sealed class MoveItem
        {
            public required CanvasFeature Feature { get; init; }
            public required IShape OriginalShape { get; init; }
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
            canvasSurface.MouseEnter += (_, _) => { if (_activeTextEditor == null) canvasSurface.Focus(); };
            canvasSurface.MouseWheel += canvasSurface_MouseWheel;
            canvasSurface.MouseDown += canvasSurface_MouseDown;
            canvasSurface.MouseMove += canvasSurface_MouseMove;
            canvasSurface.MouseUp += canvasSurface_MouseUp;
            canvasSurface.MouseLeave += canvasSurface_MouseLeave;
            canvasSurface.KeyDown += canvasSurface_KeyDown;
            canvasSurface.MouseDoubleClick += canvasSurface_MouseDoubleClick;
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

            if (_renderUpdateBatchDepth > 0)
            {
                _renderRequestPending = true;
                return;
            }

            canvasSurface.Invalidate();
        }

        public IDisposable DeferRenderUpdates()
        {
            return new RenderUpdateBatchScope(this);
        }

        private void BeginRenderUpdateBatch()
        {
            if (_renderUpdateBatchDepth == 0)
                CaptureRefreshHoldFrame();

            _renderUpdateBatchDepth++;
        }

        private void EndRenderUpdateBatch()
        {
            if (_renderUpdateBatchDepth <= 0)
            {
                _renderUpdateBatchDepth = 0;
                return;
            }

            _renderUpdateBatchDepth--;
            if (_renderUpdateBatchDepth == 0 && _renderRequestPending)
            {
                _renderRequestPending = false;
                RequestRender();
            }
        }

        private sealed class RenderUpdateBatchScope : IDisposable
        {
            private readonly MapCanvasControl _owner;
            private bool _disposed;

            public RenderUpdateBatchScope(MapCanvasControl owner)
            {
                _owner = owner;
                _owner.BeginRenderUpdateBatch();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _owner.EndRenderUpdateBatch();
            }
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

        public void UpdateAreaPrecisionSettings(int sqmPrecision, int traditionalPrecision)
        {
            _renderer.UpdateAreaPrecisionSettings(sqmPrecision, traditionalPrecision);
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
        public bool IsTextInputActive => _activeTextEditor != null;

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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ApplicationEditLocked
        {
            get => _applicationEditLocked;
            set
            {
                if (_applicationEditLocked == value)
                {
                    return;
                }

                _applicationEditLocked = value;
                if (value)
                {
                    CancelActiveTextEditor();
                    CancelActiveGripEdit(restoreOriginal: true);
                    CancelMoveOperation();
                    CancelDrawing();
                    _hoveredSelectionGrip = null;
                    SetActiveTool(MapCanvasTool.Select, null, null);
                }

                UpdateCanvasCursor();
                UpdateStatusBar();
                RequestRender();
            }
        }

        public void SetVectorLayers(IEnumerable<CanvasLayer>? layers)
        {
            CaptureRefreshHoldFrame();
            CanvasLayer[] vectorLayers = layers?.ToArray() ?? [];
            _vectorLayers = vectorLayers.ToList();
            RebuildVectorLayerIndex();
            _renderer.UpdateVectorLayers(vectorLayers);
            RefreshActiveDrawingLayer(vectorLayers);
            PruneSelectionToSelectableFeatures();
            UpdateWorldBounds();
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
        }

        public void UpdateVectorLayer(CanvasLayer layer)
        {
            CaptureRefreshHoldFrame();
            ReplaceVectorLayerSnapshot(layer);
            _renderer.UpdateVectorLayer(layer);
            if (_activeDrawingLayer?.Id == layer.Id ||
                string.Equals(_activeDrawingLayerName, layer.Name, StringComparison.OrdinalIgnoreCase))
            {
                _activeDrawingLayer = layer;
                _activeDrawingLayerName = layer.Name;
            }

            PruneSelectionToSelectableFeatures();
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
        }

        private void ReplaceVectorLayerSnapshot(CanvasLayer layer)
        {
            int existingIndex = _vectorLayers.FindIndex(item => item.Id == layer.Id);
            if (existingIndex >= 0)
            {
                _vectorLayers[existingIndex] = layer;
            }
            else
            {
                _vectorLayers.Add(layer);
            }

            RebuildVectorLayerIndex();
        }

        private void RebuildVectorLayerIndex()
        {
            _vectorLayersById = _vectorLayers
                .GroupBy(layer => layer.Id)
                .ToDictionary(group => group.Key, group => group.First());
        }

        private CanvasLayer? ResolveFeatureLayer(CanvasFeature feature)
        {
            // O(1) lookup (rebuilt whenever _vectorLayers changes) — this is
            // called per-feature in hot paths (snapping, grips, render overlays).
            return _vectorLayersById.TryGetValue(feature.CanvasObject.CanvasLayerId, out CanvasLayer? layer)
                ? layer
                : feature.Layer;
        }

        private CanvasLayer? ResolveShapeLayer(IShape? shape)
        {
            if (shape == null)
            {
                return null;
            }

            return _vectorLayers.FirstOrDefault(layer =>
                string.Equals(layer.Name, shape.LayerName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Resolves the layer whose style the in-progress preview and the
        /// just-completed shape should use so they exactly match the final
        /// cached render (same line weight, color, dash, fill). Falls back to
        /// the named layer snapshot when only the active drawing layer name is
        /// known, preventing the white/1px default-style preview mismatch.
        /// </summary>
        private CanvasLayer? GetEffectiveDrawingLayer()
        {
            // Always prefer the live layer snapshot from _vectorLayers so the
            // preview uses the exact same line weight / color / style as the
            // shapes already rendered on that layer. The cached
            // _activeDrawingLayer object can be a stale copy with an outdated
            // line weight, which makes the preview look thinner than the final.
            if (_activeDrawingLayer != null)
            {
                CanvasLayer? current =
                    _vectorLayers.FirstOrDefault(layer => layer.Id == _activeDrawingLayer.Id) ??
                    _vectorLayers.FirstOrDefault(layer =>
                        string.Equals(layer.Name, _activeDrawingLayer.Name, StringComparison.OrdinalIgnoreCase));
                return current ?? _activeDrawingLayer;
            }

            if (!string.IsNullOrWhiteSpace(_activeDrawingLayerName))
            {
                return _vectorLayers.FirstOrDefault(layer =>
                    string.Equals(layer.Name, _activeDrawingLayerName, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        public void SetVectorFeatures(IEnumerable<CanvasFeature>? features)
        {
            CaptureRefreshHoldFrame();
            // Snapshot the previous overlay so we can carry over shapes whose
            // background cache refresh hasn't landed yet when the user draws or
            // edits shapes quickly.
            IReadOnlyList<CanvasFeature> previousOverlay = _immediateEditedOverlayFeatures;
            Guid pendingDrawnShapeId = _pendingDrawnShapeId;
            _pendingDrawnShapeId = Guid.Empty;
            HashSet<Guid> pendingEditedShapeIds = _pendingEditedShapeIds.Count > 0
                ? new HashSet<Guid>(_pendingEditedShapeIds)
                : new HashSet<Guid>();
            _pendingEditedShapeIds.Clear();

            // Capture cache validity BEFORE any cache mutation so the
            // incremental fast-path can decide whether the existing bitmap is
            // still trustworthy.
            bool cacheValidBefore = _renderer.HasValidVectorCache;

            _vectorFeatures = features?.ToList() ?? [];
            _hoveredSelectionGrip = null;
            _activeGripEdit = null;
            _immediateEditedOverlayFeatures = Array.Empty<CanvasFeature>();
            _activeMoveOperation = null;
            DisposeMovePreviewBitmap();
            DisposeGripMoveFreezeBitmap();
            PruneSelectionToSelectableFeatures();
            foreach (CanvasFeature feature in _vectorFeatures)
            {
                feature.Shape.IsSelected =
                    _selectedShapeIds.Contains(feature.Shape.Id) &&
                    IsSelectableDrawingFeature(feature);
            }

            // Build the transient overlay = the just-drawn/edited shapes (plus
            // any still-pending overlay shapes from a refresh that hasn't landed
            // yet). These are painted on top of the kept cache so the change is
            // visible instantly without a full synchronous re-render.
            Dictionary<Guid, CanvasFeature> currentById = _vectorFeatures
                .GroupBy(f => f.Shape.Id)
                .ToDictionary(g => g.Key, g => g.First());
            HashSet<Guid> overlayIds = new();
            List<CanvasFeature> overlay = new();

            void AddOverlay(Guid id)
            {
                if (currentById.TryGetValue(id, out CanvasFeature? current) &&
                    overlayIds.Add(current.Shape.Id))
                {
                    overlay.Add(current);
                }
            }

            foreach (CanvasFeature prior in previousOverlay)
            {
                AddOverlay(prior.Shape.Id);
            }

            if (pendingDrawnShapeId != Guid.Empty)
            {
                AddOverlay(pendingDrawnShapeId);
            }

            foreach (Guid editedId in pendingEditedShapeIds)
            {
                AddOverlay(editedId);
            }

            // Only take the incremental path for a genuine add/edit, where the
            // changed shapes are absent from the cached bitmap (newly drawn) or
            // were excluded from it during the grip edit. Other reloads
            // (deletions, bulk refreshes) must do a real rebuild so removed or
            // changed-elsewhere geometry is not left baked in the cache.
            bool hasPendingAddOrEdit =
                pendingDrawnShapeId != Guid.Empty || pendingEditedShapeIds.Count > 0;
            bool useIncrementalFastPath =
                hasPendingAddOrEdit && overlay.Count > 0 && cacheValidBefore;

            if (useIncrementalFastPath)
            {
                // Keep the existing cache valid (it shows everything from before
                // this add/edit) and paint the new shape as a TRANSIENT overlay
                // for instant, flicker-free feedback. The async rebuild below
                // bakes the shape into the cache and the overlay is cleared when
                // it lands — so the overlay never persists (no per-frame cost).
                _renderer.SetVectorRenderExclusions(null, invalidateCache: false);
                _renderer.UpdateVectorFeatures(_vectorFeatures, invalidateCache: false);
                UpdateWorldBounds();

                _immediateEditedOverlayFeatures = overlay;
                _justCompletedShape = null;
                _justCompletedShapeLayer = null;

                RefreshVectorCacheForCurrentViewAsync();
                RequestRender();
                return;
            }

            _justCompletedShape = null;
            _justCompletedShapeLayer = null;
            _renderer.SetVectorRenderExclusions(null);
            _renderer.UpdateVectorFeatures(_vectorFeatures);
            UpdateWorldBounds();
            EnsureVectorZoomSnapshot();
            _holdVectorZoomFrameUntilRefresh = true;
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
        }

        public void PreviewSelectCanvasObject(Guid canvasObjectId, bool zoomToObject)
        {
            CanvasFeature? feature = _vectorFeatures
                .FirstOrDefault(item =>
                    item.CanvasObject.Id == canvasObjectId ||
                    item.Shape.Id == canvasObjectId);

            ReplaceSelectedObjects(
                feature != null && IsSelectableDrawingFeature(feature)
                    ? new[] { feature }
                    : Array.Empty<CanvasFeature>());

            if (feature != null &&
                zoomToObject &&
                TryGetCombinedFeatureBounds([feature], out RectangleD bounds))
            {
                ZoomToSelectionBounds(bounds);
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
                    .Where(IsSelectableDrawingFeature)
                    .ToList();

            ReplaceSelectedObjects(features);

            if (zoomToSelection &&
                TryGetCombinedFeatureBounds(features, out RectangleD bounds))
            {
                ZoomToSelectionBounds(bounds);
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
                ZoomToObjectExtentBounds(bounds);
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
            if (_applicationEditLocked && tool != MapCanvasTool.Select)
            {
                tool = MapCanvasTool.Select;
                layer = null;
                layerName = null;
                NotifyEditLocked();
            }

            if (tool != MapCanvasTool.Select)
            {
                CanvasLayer? targetLayer = layer
                    ?? (string.IsNullOrWhiteSpace(layerName)
                        ? null
                        : _vectorLayers.FirstOrDefault(item =>
                            string.Equals(item.Name, layerName, StringComparison.OrdinalIgnoreCase)));

                if (IsDrawingLayerUnavailable(targetLayer))
                {
                    NotifyDrawingLayerUnavailable(targetLayer!);
                    tool = MapCanvasTool.Select;
                    layer = null;
                    layerName = null;
                    SelectToolRequested?.Invoke();
                }
            }

            bool toolChanged = _activeTool != tool;
            _activeTool = tool;
            _panToolActive = false;
            _zoomWindowActive = false;
            _isSelectingZoomWindow = false;
            _isSelectingObjects = false;
            _hoveredSelectionGrip = null;
            CancelActiveGripEdit(restoreOriginal: true);
            CancelActiveTextEditor();
            _drawingVertices.Clear();
            _previewShape = null;

            if (toolChanged && tool != MapCanvasTool.Select)
                ClearSelectedObjects();

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
            BeginZoomNavigation("In");
            ZoomAtCanvasCenter(zoomIn: true);
            RefreshActiveTextEditorMetrics();
            RequestRender();
            ArmZoomSettleTimer();
        }

        public void ZoomOut()
        {
            BeginZoomNavigation("Out");
            ZoomAtCanvasCenter(zoomIn: false);
            RefreshActiveTextEditorMetrics();
            RequestRender();
            ArmZoomSettleTimer();
        }

        public void ZoomExtents()
        {
            _engine.ZoomToExtents();
            RefreshRasterCacheForCurrentViewAsync();
            RefreshVectorCacheForCurrentViewAsync();
            RefreshActiveTextEditorMetrics();
            RequestRender();
        }

        /// <summary>
        /// Zooms to a specific scale factor (e.g., 0.5 = 50%, 1.0 = 100%, 2.0 = 200%)
        /// </summary>
        public void ZoomToScale(double scaleFactor)
        {
            if (scaleFactor <= 0)
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
            RefreshActiveTextEditorMetrics();
            RequestRender();
        }

        public void ZoomToWorldBounds(RectangleD worldBounds)
        {
            ZoomToWorldBounds(worldBounds, padding: 0.9);
        }

        private void ZoomToSelectionBounds(RectangleD worldBounds)
        {
            if (!TryNormalizeZoomWorldBounds(worldBounds, out RectangleD normalizedBounds))
            {
                return;
            }

            ZoomToWorldBounds(ExpandSelectionZoomBounds(normalizedBounds), SelectionZoomPadding);
        }

        private void ZoomToObjectExtentBounds(RectangleD worldBounds)
        {
            if (!TryNormalizeZoomWorldBounds(worldBounds, out RectangleD normalizedBounds))
            {
                return;
            }

            ZoomToWorldBounds(ExpandObjectExtentZoomBounds(normalizedBounds), ObjectExtentZoomPadding);
        }

        private void ZoomToWorldBounds(RectangleD worldBounds, double padding)
        {
            if (!TryNormalizeZoomWorldBounds(worldBounds, out RectangleD zoomBounds))
            {
                return;
            }

            PrepareProgrammaticZoom();
            _engine.ZoomToExtents(zoomBounds, padding);
            RefreshRasterCacheForCurrentViewAsync();
            RefreshVectorCacheForCurrentViewAsync();
            RefreshActiveTextEditorMetrics();
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
            RefreshActiveTextEditorMetrics();
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
            CaptureRefreshHoldFrame();
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

            CaptureRefreshHoldFrame();
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

            CaptureRefreshHoldFrame();
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
            CaptureRefreshHoldFrame();
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
            if (ShouldDrawRefreshHoldFrame())
            {
                DrawRefreshHoldFrame(e.Graphics);
                return;
            }

            RenderCanvasFrame(e.Graphics, updateDebugTiming: true);

            if (_refreshHoldFrame != null &&
                _renderUpdateBatchDepth == 0 &&
                !_rasterCacheRefreshPending &&
                !_vectorCacheRefreshPending)
            {
                ClearRefreshHoldFrame();
            }
        }

        private void RenderCanvasFrame(Graphics graphics, bool updateDebugTiming)
        {
            // Geometric-center (whole-shape) grip move: paint ONLY the frozen
            // canvas snapshot captured at move start plus the moving preview.
            // No scene re-rendering happens until the move commits or cancels.
            if (_gripMoveFreezeBitmap != null &&
                _activeGripEdit != null &&
                _activeGripEdit.Grip.Kind == SelectionGripKind.GeometricCenter)
            {
                graphics.DrawImageUnscaled(_gripMoveFreezeBitmap, 0, 0);
                DrawActiveGripEditOverlay(graphics);
                DrawSelectionGrips(graphics);
                DrawSnapGlyph(graphics);
                return;
            }

            Stopwatch frameStopwatch = Stopwatch.StartNew();
            RasterRenderFrame? rasterFrame = GetRasterRenderFrame(out CanvasFrameSource rasterFrameSource);
            RasterRenderFrame? vectorFrame = GetVectorRenderFrame(out CanvasFrameSource vectorFrameSource);
            RasterRenderFrame? fixedReferenceFrame = GetFixedReferenceRenderFrame();
            bool deferDirectRasterRendering = ShouldDeferDirectRasterRendering;
            bool deferDirectVectorRendering = ShouldDeferDirectVectorRendering;

            if (vectorFrameSource == CanvasFrameSource.None &&
                _vectorCacheRefreshPending &&
                !IsInteractiveNavigation &&
                _activeGripEdit == null &&
                !_holdVectorPanFrameUntilRefresh &&
                !_holdVectorZoomFrameUntilRefresh)
            {
                deferDirectVectorRendering = false;
            }

            if (rasterFrameSource == CanvasFrameSource.None &&
                !deferDirectRasterRendering)
            {
                rasterFrameSource = CanvasFrameSource.Direct;
            }

            if (vectorFrameSource == CanvasFrameSource.None &&
                !deferDirectVectorRendering)
            {
                vectorFrameSource = CanvasFrameSource.Direct;
            }

            _renderer.Render(
                graphics,
                rasterFrame,
                deferDirectRasterRendering,
                vectorFrame,
                deferDirectVectorRendering,
                GetZoomWindowRectangle(),
                _previewShape,
                GetEffectiveDrawingLayer(),
                _showDebugOverlay,
                suppressDecorations: _activeTextEditor != null,
                suppressGridLabels: IsInteractiveNavigation,
                suppressFixedReferenceLayers: IsInteractiveNavigation && fixedReferenceFrame == null,
                fixedReferenceFrame: fixedReferenceFrame);

            DrawImmediateEditedFeatureOverlay(graphics);
            DrawJustCompletedShapeOverlay(graphics);
            DrawActiveGripOriginalOverlay(graphics);
            DrawObjectSelectionRectangle(graphics);
            DrawActiveGripEditOverlay(graphics);
            DrawMoveOperationOverlay(graphics);
            DrawSelectionGrips(graphics);
            DrawSnapGlyph(graphics);
            // Diameter preview drawing is handled by the vector renderer when the
            // preview shape contains the CenterDiameterEndpoint property.
            if (updateDebugTiming)
            {
                frameStopwatch.Stop();
                UpdateDebugFrameTiming(frameStopwatch.Elapsed.TotalMilliseconds);
                DrawDebugOverlayIfNeeded(graphics, rasterFrameSource, vectorFrameSource);
            }
        }

        public Bitmap CaptureCurrentImage(float outputScale = 1.0f)
        {
            if (IsDisposed ||
                Disposing ||
                canvasSurface.IsDisposed ||
                canvasSurface.ClientSize.Width <= 0 ||
                canvasSurface.ClientSize.Height <= 0)
            {
                throw new InvalidOperationException("The map canvas is not ready for screenshot capture.");
            }

            if (float.IsNaN(outputScale) || float.IsInfinity(outputScale) || outputScale < 1.0f)
            {
                outputScale = 1.0f;
            }

            outputScale = Math.Min(outputScale, 4.0f);
            int snapshotWidth = Math.Max(1, (int)Math.Ceiling(canvasSurface.ClientSize.Width * outputScale));
            int snapshotHeight = Math.Max(1, (int)Math.Ceiling(canvasSurface.ClientSize.Height * outputScale));

            Bitmap snapshot = new(
                snapshotWidth,
                snapshotHeight,
                PixelFormat.Format32bppPArgb);

            try
            {
                snapshot.SetResolution(96.0f * outputScale, 96.0f * outputScale);

                using Graphics graphics = Graphics.FromImage(snapshot);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                graphics.ScaleTransform(outputScale, outputScale);
                graphics.Clear(canvasSurface.BackColor);

                if (ShouldDrawRefreshHoldFrame())
                {
                    DrawRefreshHoldFrame(graphics);
                }
                else
                {
                    RenderCanvasFrame(graphics, updateDebugTiming: false);
                }

                return snapshot;
            }
            catch
            {
                snapshot.Dispose();
                throw;
            }
        }

        private bool ShouldDrawRefreshHoldFrame()
        {
            return _refreshHoldFrame != null &&
                   !IsInteractiveNavigation &&
                   (_renderUpdateBatchDepth > 0 ||
                    _rasterCacheRefreshPending ||
                    _vectorCacheRefreshPending);
        }

        private void DrawRefreshHoldFrame(Graphics graphics)
        {
            if (_refreshHoldFrame == null)
                return;

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImageUnscaled(_refreshHoldFrame, Point.Empty);
            graphics.CompositingMode = CompositingMode.SourceOver;
        }

        private void CaptureRefreshHoldFrame()
        {
            if (_refreshHoldFrame != null ||
                IsDisposed ||
                Disposing ||
                canvasSurface.IsDisposed ||
                canvasSurface.ClientSize.Width <= 0 ||
                canvasSurface.ClientSize.Height <= 0 ||
                IsInteractiveNavigation)
            {
                return;
            }

            Bitmap frame = new(
                canvasSurface.ClientSize.Width,
                canvasSurface.ClientSize.Height,
                PixelFormat.Format32bppPArgb);

            try
            {
                using Graphics graphics = Graphics.FromImage(frame);
                graphics.Clear(canvasSurface.BackColor);
                RenderCanvasFrame(graphics, updateDebugTiming: false);
                _refreshHoldFrame = frame;
            }
            catch (Exception ex) when (ex is ObjectDisposedException ||
                                       ex is InvalidOperationException ||
                                       ex is ExternalException)
            {
                frame.Dispose();
            }
            catch
            {
                frame.Dispose();
            }
        }

        private void ClearRefreshHoldFrame()
        {
            _refreshHoldFrame?.Dispose();
            _refreshHoldFrame = null;
        }

        private void RequestRefreshHoldReleaseRenderIfReady()
        {
            if (_refreshHoldFrame == null ||
                _renderUpdateBatchDepth > 0 ||
                _rasterCacheRefreshPending ||
                _vectorCacheRefreshPending ||
                IsDisposed ||
                Disposing ||
                !IsHandleCreated)
            {
                return;
            }

            BeginInvoke((MethodInvoker)RequestRender);
        }

        private void canvasSurface_MouseWheel(object? sender, MouseEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? MapCanvasEngine.ZoomStep : 1.0 / MapCanvasEngine.ZoomStep;
            BeginZoomNavigation(e.Delta > 0 ? "In" : "Out");

            ZoomAtPoint(e.Location, e.Delta > 0, zoomFactor);
            _currentMouseWorld = _engine.ScreenToWorld(e.Location);
            RefreshActiveTextEditorMetrics();
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
            if (_activeTextEditor != null)
            {
                // Allow middle-mouse pan while the text editor is open; block everything else.
                if (e.Button == MouseButtons.Middle)
                {
                    HandlePanStart(e.Location);
                    return;
                }

                FinishActiveTextEditorFromOutsideClick();
                return;
            }

            canvasSurface.Focus();

            if (_suppressNextCanvasMouseDownAfterTextCancel)
            {
                _suppressNextCanvasMouseDownAfterTextCancel = false;
                return;
            }

            if (_applicationEditLocked &&
                _activeTool != MapCanvasTool.Select &&
                e.Button is MouseButtons.Left or MouseButtons.Right)
            {
                NotifyEditLocked();
                SelectToolRequested?.Invoke();
                return;
            }

            // Move-operation click handling takes precedence over everything else — the user has
            // explicitly entered a two-click placement mode and other selection/grip gestures must
            // be ignored until they pick the destination or cancel via Escape / right-click.
            if (_activeMoveOperation != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    CancelMoveOperation();
                    return;
                }
                if (e.Button == MouseButtons.Left && HandleMoveOperationClick(e.Location))
                {
                    return;
                }
                if (e.Button != MouseButtons.Middle)
                {
                    return;
                }
            }

            bool isPanGesture =
                e.Button == MouseButtons.Middle ||
                (_panToolActive && e.Button == MouseButtons.Left);

            if (isPanGesture && IsPanBlockedByZoomDebounce && _activeGripEdit == null)
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

            if (_activeTool == MapCanvasTool.Select &&
                _activeGripEdit != null &&
                _activeGripEdit.AwaitingClickCommit)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ApplyActiveGripEdit(e.Location);
                    CommitActiveGripEdit();
                    return;
                }

                if (e.Button == MouseButtons.Right)
                {
                    CancelActiveGripEdit(restoreOriginal: true);
                    return;
                }
            }

            if (_zoomWindowActive && e.Button == MouseButtons.Left)
            {
                _isSelectingZoomWindow = true;
                _zoomWindowStart = e.Location;
                _zoomWindowCurrent = e.Location;
                RequestRender();
                return;
            }

            if (_activeTool == MapCanvasTool.Select && e.Button == MouseButtons.Right)
            {
                CanvasFeature? hitFeature = FindSelectableFeatureAtScreenPoint(e.Location);
                if (hitFeature != null)
                {
                    ReplaceSelectedObjects([hitFeature]);
                    _objectSelectionContextMenu.Show(canvasSurface, e.Location);
                    return;
                }

                if (_selectedShapeIds.Count > 0)
                {
                    _objectSelectionContextMenu.Show(canvasSurface, e.Location);
                    return;
                }
            }

            if (isPanGesture)
            {
                HandlePanStart(e.Location);
                return;
            }

            if (_activeTool == MapCanvasTool.Select && e.Button == MouseButtons.Left)
            {
                if (!_applicationEditLocked &&
                    _hoveredSelectionGrip != null &&
                    BeginGripEdit(_hoveredSelectionGrip, e.Location))
                {
                    return;
                }

                _isSelectingObjects = true;
                _objectSelectionStart = e.Location;
                _objectSelectionCurrent = e.Location;
                canvasSurface.Capture = true;
                RequestRender();
            }
        }

        private void HandlePanStart(Point location)
        {
            CancelPendingRasterRender();
            CancelPendingVectorRender();
            CancelLiveTileLoading();
            SetLiveTileInternetFetchingSuspended(true);

            _lastPanPoint = location;
            _totalPanDelta = PointF.Empty;
            _panStartWorld = _engine.ScreenToWorld(location);
            _panStartPoint = location;
            _panStartWorldOrigin = _engine.ViewOriginWorld;
            _currentMouseWorld = _panStartWorld;

            // Snapshot exactly what is on screen right now into one composite bitmap.
            // _isPanning is still false here so GetRasterRenderFrame / GetVectorRenderFrame
            // return the same frames that were used in the last paint call.
            BuildCompositePanBuffer(canvasSurface.Size);
            BuildGridPanBuffer(canvasSurface.Size);

            // Clear prior hold flags AFTER building the composite so the composite
            // correctly captures the held pan/zoom frames that were visible on screen.
            _holdVectorPanFrameUntilRefresh = false;
            _holdZoomStartFrameUntilRasterRefresh = false;
            _isPanning = true;

            // Keep the separate deferred buffers as fallback for when the composite
            // build failed (e.g. insufficient memory).
            _rasterDeferredRenderer.BeginPan(canvasSurface.Size, _rasterRenderLayers, _engine);
            EnsureVectorPanSnapshot();

            canvasSurface.Capture = true;
            UpdateCanvasCursor();
            UpdateStatusBar();
        }

        private void canvasSurface_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _activeTool != MapCanvasTool.Select)
                return;

            if (_applicationEditLocked)
            {
                NotifyEditLocked();
                return;
            }

            PointD worldPoint = _engine.ScreenToWorld(new PointD(e.X, e.Y));
            double worldTolerance = _engine.ScreenToWorldDistance(ObjectSelectionTolerancePixels);

            CanvasFeature? hit = FindTextShapeHitAtScreenPoint(e.Location)
                ?? FindClickHitFeature(worldPoint, worldTolerance);

            if (hit?.Shape is TextShape textShape)
                BeginTextEditExisting(hit, textShape, e.Location);
        }

        private void canvasSurface_MouseMove(object? sender, MouseEventArgs e)
        {
            if (IsCanvasInteractionLocked && !_isPanning)
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
                RefreshActiveTextEditorMetrics();
                RequestRender();
                return;
            }

            if (_activeGripEdit != null)
            {
                ApplyActiveGripEdit(e.Location);
                if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                {
                    _activeGripEdit.HasPointerMoved = true;
                }

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

            if (_activeMoveOperation != null)
            {
                RequestRender();
                UpdateStatusBar();
                return;
            }

            if (_applicationEditLocked)
            {
                _hoveredSelectionGrip = null;
            }
            else
            {
                UpdateHoveredSelectionGrip(e.Location);
            }
            UpdateDrawingPreview(e.Location);
            UpdateStatusBar();
        }

        private void canvasSurface_MouseUp(object? sender, MouseEventArgs e)
        {
            // Always release an in-progress pan, even when the text editor is active.
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
                RefreshActiveTextEditorMetrics();
                UpdateCanvasCursor();
                RequestRender();
                return;
            }

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

            if (_activeGripEdit != null && e.Button == MouseButtons.Left)
            {
                ApplyActiveGripEdit(e.Location);
                if (_activeGripEdit.HasPointerMoved ||
                    Distance(_activeGripEdit.StartScreenPoint, e.Location) > 2.0)
                {
                    CommitActiveGripEdit();
                }
                else
                {
                    _activeGripEdit.AwaitingClickCommit = true;
                    canvasSurface.Capture = false;
                    UpdateCanvasCursor();
                    RequestRender();
                }

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
                if (_activeGripEdit == null)
                {
                    _hoveredSelectionGrip = null;
                    UpdateCanvasCursor();
                }
                UpdateStatusBar();
            }
        }

        private void canvasSurface_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_textEditorJustCancelled)
            {
                _textEditorJustCancelled = false;
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Escape && _activeMoveOperation != null)
            {
                CancelMoveOperation();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Escape && _activeGripEdit != null)
            {
                CancelActiveGripEdit(restoreOriginal: true);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Delete && _selectedShapeIds.Count > 0)
            {
                if (_applicationEditLocked)
                {
                    NotifyEditLocked();
                    e.Handled = true;
                    return;
                }

                RequestDeleteSelectedObjects();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (_applicationEditLocked)
                {
                    NotifyEditLocked();
                    e.Handled = true;
                    return;
                }

                if (CanCompleteMultiPointDrawing())
                {
                    CompleteMultiPointDrawing();
                    e.Handled = true;
                }
                return;
            }

            if (e.KeyCode == Keys.Back)
            {
                if (_applicationEditLocked)
                {
                    NotifyEditLocked();
                    e.Handled = true;
                    return;
                }

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
            if (_applicationEditLocked)
            {
                NotifyEditLocked();
                SelectToolRequested?.Invoke();
                return;
            }

            CanvasLayer? drawingLayer = GetEffectiveDrawingLayer();
            if (IsDrawingLayerUnavailable(drawingLayer))
            {
                NotifyDrawingLayerUnavailable(drawingLayer!);
                SelectToolRequested?.Invoke();
                return;
            }

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
                case MapCanvasTool.Text:
                    BeginSingleLineTextInput(worldPoint, e.Location);
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
                ? ArcShape.FromCenterStartEnd(
                    _drawingVertices[0],
                    _drawingVertices[1],
                    _drawingVertices[2],
                    _centerStartEndArcClockwise)
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

        private void BeginSingleLineTextInput(PointD worldPoint, Point screenPoint)
        {
            CancelActiveTextEditor();
            _isSelectingObjects = false;
            canvasSurface.Capture = false;

            string alignment = GetActiveTextAlignment();
            (string fontName, float baseFontSize, bool scaleWithZoom) = GetTextEditorFontSpec(_activeDrawingLayer);
            _editingTextFeature = null;
            SpawnTextEditor(worldPoint, screenPoint, alignment, fontName, baseFontSize, scaleWithZoom, string.Empty, ResolveActiveTextColor());
        }

        private void BeginTextEditExisting(CanvasFeature feature, TextShape textShape, Point screenPoint)
        {
            CancelActiveTextEditor();
            _isSelectingObjects = false;
            canvasSurface.Capture = false;

            // Prefer the layer's full combined alignment ("Center Middle") so the textbox
            // is anchored exactly where the rendered text sits.
            string alignment = feature.Layer?.TextAlignment ?? textShape.HorizontalAlignment;
            (string fontName, float baseFontSize, bool scaleWithZoom) = GetTextEditorFontSpec(feature.Layer, textShape);
            Color textColor = textShape.FillColor == Color.Transparent ? Color.Black : textShape.FillColor;
            _editingTextFeature = feature;
            textShape.IsBeingEdited = true;
            RequestRender(); // erase background text immediately before editor appears
            SpawnTextEditor(textShape.Position, screenPoint, alignment, fontName, baseFontSize, scaleWithZoom, textShape.Text, textColor);
        }

        private void SpawnTextEditor(
            PointD worldPoint,
            Point screenPoint,
            string alignment,
            string fontName,
            float baseFontSize,
            bool scaleWithZoom,
            string initialText,
            Color textColor)
        {
            _activeTextEditorFontName = fontName;
            _activeTextEditorBaseFontSize = Math.Clamp(baseFontSize, 1.0f, 120.0f);
            _activeTextEditorScalesWithZoom = scaleWithZoom;
            _activeTextEditorAlignment = NormalizeFullAlignment(alignment);
            Font font = CreateScaledTextEditorFont();
            int initialWidth = Math.Max(120,
                TextRenderer.MeasureText(string.IsNullOrEmpty(initialText) ? "W" : initialText, font).Width + 16);
            int initialHeight = font.Height + 8;

            TextBox editor = new()
            {
                BorderStyle = BorderStyle.FixedSingle,
                Font = font,
                TextAlign = ToHorizontalAlignment(_activeTextEditorAlignment),
                BackColor = canvasSurface.BackColor,
                ForeColor = textColor,
                Text = initialText,
                Location = GetTextEditorLocation(screenPoint, _activeTextEditorAlignment, initialWidth, initialHeight),
                Size = new Size(initialWidth, initialHeight),
                TabStop = false
            };

            editor.KeyDown += ActiveTextEditor_KeyDown;
            editor.TextChanged += ActiveTextEditor_TextChanged;
            editor.LostFocus += ActiveTextEditor_LostFocus;

            _activeTextEditor = editor;
            _activeTextAnchorWorld = worldPoint;
            _textEditorCompleting = false;
            canvasSurface.Controls.Add(editor);
            editor.BringToFront();
            editor.SelectAll();
            editor.Focus();
            RefreshActiveTextEditorMetrics();
            UpdateStatusBar();
        }

        private (string FontName, float BaseFontSize, bool ScaleWithZoom) GetTextEditorFontSpec(
            CanvasLayer? layer,
            TextShape? textShape = null)
        {
            string fontName = string.IsNullOrWhiteSpace(layer?.LabelFontName)
                ? textShape?.Font.FontFamily.Name ?? "Nirmala UI"
                : layer.LabelFontName.Trim();
            float fontSize = (float)Math.Clamp(
                layer?.LabelFontSize > 0 ? layer.LabelFontSize : textShape?.Font.Size ?? 10.0,
                1.0,
                120.0);
            bool scaleWithZoom = layer?.LabelScaleWithZoom ?? true;

            return (fontName, fontSize, scaleWithZoom);
        }

        private Font CreateScaledTextEditorFont()
        {
            float fontSize = ResolveScaledTextEditorFontSize();
            try
            {
                return new Font(_activeTextEditorFontName, fontSize);
            }
            catch
            {
                return new Font("Nirmala UI", fontSize);
            }
        }

        private float ResolveScaledTextEditorFontSize()
        {
            if (!_activeTextEditorScalesWithZoom)
                return Math.Clamp(_activeTextEditorBaseFontSize, 1.0f, 120.0f);

            double zoomFactor = double.IsFinite(_engine.ZoomScale)
                ? Math.Clamp(_engine.ZoomScale, 0.25, 12.0)
                : 1.0;
            return (float)Math.Clamp(_activeTextEditorBaseFontSize * zoomFactor, 4.0, 180.0);
        }

        private Color ResolveActiveTextColor()
        {
            string? htmlColor = _activeDrawingLayer?.LabelColor;
            if (string.IsNullOrWhiteSpace(htmlColor))
                htmlColor = _activeDrawingLayer?.BorderColor;

            if (!string.IsNullOrWhiteSpace(htmlColor))
            {
                try { return ColorTranslator.FromHtml(htmlColor); }
                catch { }
            }

            return Color.Black;
        }

        private string GetActiveTextAlignment()
        {
            return _activeDrawingLayer?.TextAlignment ?? "Left";
        }

        private static string NormalizeFullAlignment(string? alignment)
        {
            if (string.IsNullOrWhiteSpace(alignment)) return "Left Top";
            string[] parts = alignment.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string h = (parts.Length > 0 ? parts[0] : "").ToLowerInvariant() switch
            {
                "center" or "centre" or "middle" => "Center",
                "right" => "Right",
                _ => "Left"
            };
            string v = (parts.Length > 1 ? parts[1] : "").ToLowerInvariant() switch
            {
                "middle" or "center" or "centre" => "Middle",
                "bottom" => "Bottom",
                _ => "Top"
            };
            return $"{h} {v}";
        }

        private static HorizontalAlignment ToHorizontalAlignment(string alignment)
        {
            return TextShape.NormalizeHorizontalAlignment(alignment) switch
            {
                "Center" => HorizontalAlignment.Center,
                "Right"  => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Left
            };
        }

        private static Point GetTextEditorLocation(Point anchorScreenPoint, string alignment, int width, int height = 0)
        {
            string[] parts = alignment.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string h = (parts.Length > 0 ? parts[0] : "").ToLowerInvariant();
            string v = (parts.Length > 1 ? parts[1] : "").ToLowerInvariant();

            int x = h switch
            {
                "center" or "centre" => anchorScreenPoint.X - width / 2,
                "right"              => anchorScreenPoint.X - width,
                _                   => anchorScreenPoint.X
            };
            int y = v switch
            {
                "middle" or "center" => anchorScreenPoint.Y - height / 2,
                "bottom"             => anchorScreenPoint.Y - height,
                _                   => anchorScreenPoint.Y - 2
            };

            return new Point(x, y);
        }

        private void ActiveTextEditor_TextChanged(object? sender, EventArgs e)
        {
            RefreshActiveTextEditorMetrics();
        }

        private void RefreshActiveTextEditorMetrics()
        {
            if (_activeTextEditor == null)
                return;

            Font replacementFont = CreateScaledTextEditorFont();
            Font oldFont = _activeTextEditor.Font;
            bool fontChanged =
                !string.Equals(oldFont.FontFamily.Name, replacementFont.FontFamily.Name, StringComparison.OrdinalIgnoreCase) ||
                Math.Abs(oldFont.Size - replacementFont.Size) > 0.1f;

            if (fontChanged)
            {
                _activeTextEditor.Font = replacementFont;
                oldFont.Dispose();
            }
            else
            {
                replacementFont.Dispose();
            }

            int scaledMinWidth = Math.Max(120, _activeTextEditor.Font.Height * 6);
            int measuredWidth = TextRenderer.MeasureText(
                string.IsNullOrEmpty(_activeTextEditor.Text) ? "WWWWWWW" : _activeTextEditor.Text,
                _activeTextEditor.Font).Width + 20;
            int maxEditorWidth = Math.Max(scaledMinWidth, 1600);
            int width = Math.Clamp(measuredWidth, scaledMinWidth, maxEditorWidth);
            int height = Math.Max(24, _activeTextEditor.Font.Height + 10);
            Point anchorScreen = _activeTextAnchorWorld.HasValue
                ? ToScreenPoint(_activeTextAnchorWorld.Value)
                : _activeTextEditor.Location;

            _activeTextEditor.Size = new Size(width, height);
            _activeTextEditor.Location = GetTextEditorLocation(anchorScreen, _activeTextEditorAlignment, width, height);
        }

        private void ActiveTextEditor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                _suppressNextCanvasMouseDownAfterTextCancel = true;
                CommitActiveTextEditor();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                // _editingTextFeature is cleared inside CancelActiveTextEditor so the shape's
                // IsBeingEdited is reset (restoring the old text) before the reference is lost.
                _suppressNextCanvasMouseDownAfterTextCancel = true;
                CancelActiveTextEditor();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void ActiveTextEditor_LostFocus(object? sender, EventArgs e)
        {
            if (_textEditorCompleting || _activeTextEditor == null)
                return;

            // If the canvas surface just stole focus (scroll/pan), restore the editor.
            BeginInvoke(ActiveTextEditor_LostFocusDeferred);
        }

        private void ActiveTextEditor_LostFocusDeferred()
        {
            if (_textEditorCompleting || _activeTextEditor == null)
                return;

            FinishActiveTextEditorFromOutsideClick();
        }

        private void FinishActiveTextEditorFromOutsideClick()
        {
            if (_textEditorCompleting || _activeTextEditor == null)
                return;

            if (string.IsNullOrWhiteSpace(_activeTextEditor.Text))
            {
                // CancelActiveTextEditor resets IsBeingEdited, restoring old text for existing shapes.
                CancelActiveTextEditor();
                return;
            }

            CommitActiveTextEditor();
        }

        private void CommitActiveTextEditor()
        {
            if (_activeTextEditor == null || !_activeTextAnchorWorld.HasValue)
                return;

            string text = _activeTextEditor.Text.Trim();
            PointD anchor = _activeTextAnchorWorld.Value;
            string alignment = _editingTextFeature?.Shape is TextShape existing
                ? existing.HorizontalAlignment
                : GetActiveTextAlignment();
            CanvasFeature? editTarget = _editingTextFeature;

            // Reject whitespace-only input.
            // CancelActiveTextEditor resets IsBeingEdited, restoring old text for existing shapes.
            if (!text.Any(c => !char.IsWhiteSpace(c)))
            {
                CancelActiveTextEditor();
                return;
            }

            if (editTarget?.Shape is TextShape editingTs)
                editingTs.IsBeingEdited = false;

            _editingTextFeature = null;
            CancelActiveTextEditor();

            if (editTarget?.Shape is TextShape targetTextShape)
            {
                // Update existing text shape and persist it (single-item batch).
                targetTextShape.Text = text;
                ShapesEdited?.Invoke(new IShape[] { targetTextShape });
                EnsureVectorZoomSnapshot();
                _holdVectorZoomFrameUntilRefresh = true;
                RefreshVectorCacheForCurrentViewAsync();
                RequestRender();
                return;
            }

            TextShape textShape = new(anchor, text, horizontalAlignment: alignment);
            textShape.Properties["ObjectType"] = "Text";
            textShape.Properties["TextAlignment"] = alignment;
            CompleteShape(textShape);
        }

        private void CancelActiveTextEditor()
        {
            TextBox? editor = _activeTextEditor;
            if (editor == null)
                return;

            // Restore IsBeingEdited on the shape being edited so it renders again with its old text.
            if (_editingTextFeature?.Shape is TextShape ts)
                ts.IsBeingEdited = false;
            _editingTextFeature = null;

            _textEditorCompleting = true;
            _textEditorJustCancelled = true;
            editor.KeyDown -= ActiveTextEditor_KeyDown;
            editor.TextChanged -= ActiveTextEditor_TextChanged;
            editor.LostFocus -= ActiveTextEditor_LostFocus;
            canvasSurface.Controls.Remove(editor);
            editor.Dispose();
            _activeTextEditor = null;
            _activeTextAnchorWorld = null;
            _textEditorCompleting = false;
            // _textEditorJustCancelled is cleared by the next canvas KeyDown event, not via BeginInvoke,
            // to avoid a timing race where the canvas Escape reaches KeyDown after the flag clears.
            UpdateStatusBar();
            RequestRender();
        }

        // Returns the topmost TextShape feature whose last-rendered screen bounds contain the point.
        private CanvasFeature? FindTextShapeHitAtScreenPoint(Point screenPoint)
        {
            return _vectorFeatures
                .Where(IsSelectableDrawingFeature)
                .Where(f => f.Shape is TextShape ts &&
                            ts.LastRenderedBounds.HasValue &&
                            ts.LastRenderedBounds.Value.Contains(screenPoint.X, screenPoint.Y))
                .OrderByDescending(GetSelectionDrawingMarkupRenderPass)
                .ThenByDescending(GetSelectionRePlotGroupRenderPass)
                .ThenByDescending(GetSelectionCadastralParcelRenderPass)
                .ThenByDescending(GetSelectionProjectBoundaryRenderPass)
                .ThenByDescending(GetSelectionDisplayOrder)
                .ThenByDescending(f => f.CanvasObject.Id)
                .FirstOrDefault();
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
                    if (_arcDrawingMode == ArcDrawingMode.CenterStartEnd)
                    {
                        _drawingOptionsContextMenu.Items.Add(new ToolStripSeparator());
                        AddSectionHeader("Arc Direction");
                        AddDrawingOption("CCW", !_centerStartEndArcClockwise, () => SetCenterStartEndArcDirection(clockwise: false));
                        AddDrawingOption("CW", _centerStartEndArcClockwise, () => SetCenterStartEndArcDirection(clockwise: true));
                    }
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

        private void SetCenterStartEndArcDirection(bool clockwise)
        {
            _centerStartEndArcClockwise = clockwise;
            RefreshDrawingPreviewFromCursor();
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
                    worldPoint,
                    _centerStartEndArcClockwise),
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
            // Stash the just-drawn shape so the paint pipeline can keep it
            // visible until SetVectorFeatures (called after the async save)
            // hands us a CanvasFeature for the same shape and parks it in the
            // _immediateEditedOverlayFeatures slot.
            _justCompletedShape = shape;
            _justCompletedShapeLayer = GetEffectiveDrawingLayer();
            _previewShape = null;
            _currentSnapPoint = null;
            // Mark this shape so SetVectorFeatures (called after the async save)
            // can keep the current vector cache valid, paint the new shape as an
            // immediate overlay, and let the full re-render happen asynchronously
            // — avoiding the freeze caused by a sync cache rebuild.
            _pendingDrawnShapeId = shape.Id;
            ShapeCompleted?.Invoke(shape);
            UpdateStatusBar();
            RequestRender();
        }

        private void CancelDrawing()
        {
            CancelActiveTextEditor();
            _drawingVertices.Clear();
            _drawingSegments.Clear();
            _polylineArcAwaitingCenter = false;
            _polylineArcAwaitingEnd = false;
            _polylineArcCenterPoint = null;
            _pendingPolylineArcThroughPoint = null;
            _previewShape = null;
            _currentSnapPoint = null;
            _justCompletedShape = null;
            _justCompletedShapeLayer = null;
            _pendingDrawnShapeId = Guid.Empty;
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
            bool isGripEditing = _activeGripEdit != null;
            bool isMoving = _activeMoveOperation != null;
            if (!_snapEnabled ||
                IsSnapTemporarilySuspendedForZoom() ||
                (_activeTool == MapCanvasTool.Select && !isGripEditing && !isMoving) ||
                (IsInteractiveNavigation && !isGripEditing && !isMoving) ||
                (IsPanBlockedByZoomDebounce && !isGripEditing && !isMoving))
            {
                ClearCurrentSnapPoint();
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            int queryRadius = SnapQueryBoxPixels;
            Rectangle screenQuery = new(
                screenPoint.X - queryRadius,
                screenPoint.Y - queryRadius,
                queryRadius * 2,
                queryRadius * 2);
            RectangleD worldQuery = CreateWorldRectangle(screenQuery);
            PointD mouseWorld = _engine.ScreenToWorld(screenPoint);
            SnapPoint? previousSnapPoint = _currentSnapPoint;

            // Use the spatial index to fetch only the handful of features near
            // the cursor instead of scanning every feature on each mouse move
            // (O(log n + k) vs O(n)). This keeps snapping — and therefore
            // drawing/editing/moving — smooth on large cadastral scenes.
            List<IShape> nearbyShapes = _renderer.QueryVectorFeatures(worldQuery)
                .Where(IsSnapCandidateFeature)
                .Select(feature => feature.Shape)
                .Where(shape => ShapeIntersectsWorldQuery(shape, worldQuery))
                .Take(250)
                .ToList();

            // When grip editing, exclude original edited shapes and use only preview shapes
            // to avoid duplicate/conflicting snap candidates from both versions. The preview
            // keeps every non-active grip on the edited object available for snapping.
            if (_activeGripEdit != null)
            {
                // Remove the original edited shape from candidates (we'll add the preview).
                nearbyShapes.RemoveAll(s => ReferenceEquals(s, _activeGripEdit.Grip.Shape));

                // Remove any linked edited shapes.
                foreach (var linkedEdit in _activeGripEdit.LinkedEdits)
                {
                    nearbyShapes.RemoveAll(s => ReferenceEquals(s, linkedEdit.Grip.Shape));
                }

                // Add the preview shape if it intersects the query area.
                if (ShapeIntersectsWorldQuery(_activeGripEdit.PreviewShape, worldQuery) &&
                    !nearbyShapes.Any(s => ReferenceEquals(s, _activeGripEdit.PreviewShape)))
                {
                    nearbyShapes.Add(_activeGripEdit.PreviewShape);
                }

                // Add linked edited preview shapes if they intersect the query area.
                foreach (var linkedEdit in _activeGripEdit.LinkedEdits)
                {
                    if (ShapeIntersectsWorldQuery(linkedEdit.PreviewShape, worldQuery) &&
                        !nearbyShapes.Any(s => ReferenceEquals(s, linkedEdit.PreviewShape)))
                    {
                        nearbyShapes.Add(linkedEdit.PreviewShape);
                    }
                }
            }

            PointD? fromPoint = _drawingVertices.Count > 0 ? _drawingVertices[^1] : null;
            List<SnapPoint> candidates = _snapManager
                .GetSnapCandidates(
                    nearbyShapes,
                    BuildInProgressSnapPoints(mouseWorld),
                    fromPoint)
                .Where(snapPoint => ScreenQueryContainsSnapPoint(screenQuery, snapPoint))
                .Where(snapPoint => !IsSuppressedPreviewSnapPoint(snapPoint))
                .Where(snapPoint => !IsActiveGripSnapPoint(snapPoint))
                .ToList();

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

        private bool IsSnapTemporarilySuspendedForZoom()
        {
            double scaleDenominator = GetCurrentScaleDenominator();
            return scaleDenominator > MaxSnapScaleDenominator;
        }

        private double GetCurrentScaleDenominator()
        {
            return _engine.ZoomScale > 0 && double.IsFinite(_engine.ZoomScale)
                ? ScreenPixelsPerMetre / _engine.ZoomScale
                : double.PositiveInfinity;
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

            if (_activeTool == MapCanvasTool.Polygon && _drawingVertices.Count > 2)
            {
                PointD first = _drawingVertices[0];
                PointD last = _drawingVertices[^1];
                yield return new SnapPoint(
                    SnapType.Midpoint,
                    new PointD((first.X + last.X) / 2.0, (first.Y + last.Y) / 2.0),
                    null);
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
            // Grip-edit previews contribute snap points so the dragged grip can snap onto
            // other endpoints/midpoints/centers/quadrants of the same edited object. The
            // single active handle is filtered separately by IsActiveGripSnapPoint.
            if (IsGripEditPreviewShape(snapPoint.ParentShape))
            {
                return false;
            }

            return _activeTool == MapCanvasTool.Circle &&
                   _drawingVertices.Count > 0 &&
                   snapPoint.Type == SnapType.Quadrant &&
                   ReferenceEquals(snapPoint.ParentShape, _previewShape);
        }

        private bool IsGripEditPreviewShape(IShape? shape)
        {
            if (_activeGripEdit == null || shape == null)
            {
                return false;
            }

            if (ReferenceEquals(shape, _activeGripEdit.PreviewShape))
            {
                return true;
            }

            return _activeGripEdit.LinkedEdits.Any(linkedEdit =>
                ReferenceEquals(shape, linkedEdit.PreviewShape));
        }

        private bool IsActiveGripEditedShape(IShape? shape)
        {
            if (_activeGripEdit == null || shape == null)
            {
                return false;
            }

            if (ReferenceEquals(shape, _activeGripEdit.Grip.Shape) ||
                ReferenceEquals(shape, _activeGripEdit.PreviewShape) ||
                shape.Id == _activeGripEdit.Grip.Shape.Id)
            {
                return true;
            }

            return _activeGripEdit.LinkedEdits.Any(linkedEdit =>
                ReferenceEquals(shape, linkedEdit.Grip.Shape) ||
                ReferenceEquals(shape, linkedEdit.PreviewShape) ||
                shape.Id == linkedEdit.Grip.Shape.Id);
        }

        private bool IsActiveGripSnapPoint(SnapPoint snapPoint)
        {
            if (_activeGripEdit == null)
            {
                return false;
            }

            // Only suppress the SPECIFIC handle being dragged (identified by index in its preview
            // shape, not by world position). All other vertices, midpoints, arc centers, etc. of
            // the edited shape remain valid snap targets. This is the only correct way to keep
            // the snap engaged when the dragged vertex sits on top of another vertex of the same
            // shape (the typical "snap onto neighbor" workflow).
            if (ReferenceEquals(snapPoint.ParentShape, _activeGripEdit.PreviewShape) &&
                IsDraggedHandleSnapPoint(snapPoint, _activeGripEdit.PreviewShape, _activeGripEdit.Grip))
            {
                return true;
            }

            foreach (LinkedGripEdit linkedEdit in _activeGripEdit.LinkedEdits)
            {
                if (ReferenceEquals(snapPoint.ParentShape, linkedEdit.PreviewShape) &&
                    IsDraggedHandleSnapPoint(snapPoint, linkedEdit.PreviewShape, linkedEdit.Grip))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDraggedHandleSnapPoint(
            SnapPoint snapPoint,
            IShape previewShape,
            SelectionGrip grip)
        {
            // Polyline / polygon vertex grip: identify the dragged vertex by INDEX so that a
            // coincident neighbour (e.g. after snapping the dragged vertex onto another vertex)
            // still contributes a valid Endpoint snap candidate.
            if (grip.Kind == SelectionGripKind.Vertex && previewShape is PolylineShape pl &&
                grip.VertexIndex >= 0 && grip.VertexIndex < pl.Vertices.Count)
            {
                if (snapPoint.Type != SnapType.Endpoint)
                    return false;

                PointD draggedPos = pl.Vertices[grip.VertexIndex];
                if (!IsSameWorldPoint(snapPoint.Position, draggedPos))
                    return false;

                // If another vertex of the same shape sits at the same world position, we cannot
                // tell which of the identical Endpoint snap points was generated from the dragged
                // index — keep both so the snap glyph stays anchored on the coincident neighbour.
                for (int i = 0; i < pl.Vertices.Count; i++)
                {
                    if (i == grip.VertexIndex) continue;
                    if (IsSameWorldPoint(pl.Vertices[i], draggedPos))
                        return false;
                }
                return true;
            }

            // Line endpoint grip: index 0 = Start, index 1 = End.
            if (grip.Kind == SelectionGripKind.Vertex && previewShape is LineShape ln)
            {
                if (snapPoint.Type != SnapType.Endpoint)
                    return false;

                PointD draggedPos = grip.VertexIndex == 0 ? ln.Start : ln.End;
                return IsSameWorldPoint(snapPoint.Position, draggedPos) &&
                       !HasCoincidentSnapPointAtHandle(previewShape, draggedPos);
            }

            // For other grip kinds (segment midpoint, arc midpoint, circle/ellipse handles, etc.),
            // suppress a snap point only when it sits exactly on the live grip position.
            PointD? handlePos = TryGetGripWorldPoint(new SelectionGrip
            {
                Feature = grip.Feature,
                Shape = previewShape,
                Kind = grip.Kind,
                Glyph = grip.Glyph,
                Position = grip.Position,
                SegmentStart = grip.SegmentStart,
                SegmentEnd = grip.SegmentEnd,
                VertexIndex = grip.VertexIndex,
                SegmentIndex = grip.SegmentIndex,
                AuxiliaryIndex = grip.AuxiliaryIndex
            });
            return handlePos.HasValue &&
                   IsSameWorldPoint(snapPoint.Position, handlePos.Value) &&
                   !HasCoincidentSnapPointAtHandle(previewShape, handlePos.Value);
        }

        private static bool HasCoincidentSnapPointAtHandle(IShape previewShape, PointD handlePosition)
        {
            int coincidentCount = 0;
            foreach (SnapPoint candidate in previewShape.GetSnapPoints())
            {
                if (!IsSameWorldPoint(candidate.Position, handlePosition))
                {
                    continue;
                }

                coincidentCount++;
                if (coincidentCount > 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSameWorldPoint(PointD a, PointD b)
        {
            return Math.Abs(a.X - b.X) < 1e-9 && Math.Abs(a.Y - b.Y) < 1e-9;
        }

        private bool IsSnapNearScreenPoint(PointD snapWorld, PointD referenceWorld)
        {
            PointD snapScreen = _engine.WorldToScreen(snapWorld);
            PointD gripScreen = _engine.WorldToScreen(referenceWorld);
            if (!double.IsFinite(snapScreen.X) || !double.IsFinite(snapScreen.Y) ||
                !double.IsFinite(gripScreen.X) || !double.IsFinite(gripScreen.Y))
            {
                return false;
            }

            double dx = snapScreen.X - gripScreen.X;
            double dy = snapScreen.Y - gripScreen.Y;
            return Math.Sqrt(dx * dx + dy * dy) <= ActiveGripSnapSuppressionPixels;
        }

        private static PointD? TryGetGripWorldPoint(SelectionGrip grip)
        {
            switch (grip.Shape)
            {
                case LineShape line:
                    return grip.Kind switch
                    {
                        SelectionGripKind.Vertex when grip.VertexIndex == 0 => line.Start,
                        SelectionGripKind.Vertex when grip.VertexIndex == 1 => line.End,
                        SelectionGripKind.SegmentMidpoint => Midpoint(line.Start, line.End),
                        _ => null
                    };

                case PolylineShape polyline:
                    if (grip.Kind == SelectionGripKind.GeometricCenter)
                    {
                        return polyline.Vertices.Count >= 2
                            ? ComputePolylineGeometricCenter(polyline)
                            : (PointD?)null;
                    }

                    if (grip.Kind == SelectionGripKind.Vertex)
                    {
                        return grip.VertexIndex >= 0 && grip.VertexIndex < polyline.Vertices.Count
                            ? polyline.Vertices[grip.VertexIndex]
                            : null;
                    }

                    if (grip.Kind == SelectionGripKind.SegmentMidpoint)
                    {
                        if (polyline.Segments.Count > 0)
                        {
                            if (grip.SegmentIndex == -1)
                            {
                                if (polyline.Vertices.Count < 2)
                                {
                                    return null;
                                }

                                PointD lastEnd = polyline.Segments[^1].End;
                                PointD first = polyline.Vertices[0];
                                return Midpoint(lastEnd, first);
                            }

                            if (grip.SegmentIndex < 0 || grip.SegmentIndex >= polyline.Segments.Count)
                            {
                                return null;
                            }

                            PolylineShape.PolylineSegment segment = polyline.Segments[grip.SegmentIndex];
                            return Midpoint(segment.Start, segment.End);
                        }

                        if (polyline.Vertices.Count < 2)
                        {
                            return null;
                        }

                        int segmentCount = polyline.IsClosed && polyline.Vertices.Count > 2
                            ? polyline.Vertices.Count
                            : polyline.Vertices.Count - 1;
                        if (grip.SegmentIndex < 0 || grip.SegmentIndex >= segmentCount)
                        {
                            return null;
                        }

                        PointD start = polyline.Vertices[grip.SegmentIndex];
                        PointD end = polyline.Vertices[(grip.SegmentIndex + 1) % polyline.Vertices.Count];
                        return Midpoint(start, end);
                    }

                    if (grip.Kind == SelectionGripKind.ArcMidpoint &&
                        grip.SegmentIndex >= 0 &&
                        grip.SegmentIndex < polyline.Segments.Count)
                    {
                        PolylineShape.PolylineSegment segment = polyline.Segments[grip.SegmentIndex];
                        return segment.Arc?.MidPoint;
                    }

                    return null;

                case RectangleShape rectangle:
                    if (grip.Kind == SelectionGripKind.GeometricCenter)
                    {
                        return ComputeRectangleCenter(rectangle);
                    }

                    PointD[] corners = GetRectangleCorners(rectangle);
                    if (grip.Kind == SelectionGripKind.Vertex &&
                        grip.VertexIndex >= 0 && grip.VertexIndex < corners.Length)
                    {
                        return corners[grip.VertexIndex];
                    }

                    if (grip.Kind == SelectionGripKind.SegmentMidpoint &&
                        grip.SegmentIndex >= 0 && grip.SegmentIndex < corners.Length)
                    {
                        PointD start = corners[grip.SegmentIndex];
                        PointD end = corners[(grip.SegmentIndex + 1) % corners.Length];
                        return Midpoint(start, end);
                    }

                    return null;

                case CircleShape circle:
                    if (grip.Kind == SelectionGripKind.CircleCenter)
                    {
                        return circle.Center;
                    }

                    if (grip.Kind == SelectionGripKind.CircleQuadrant)
                    {
                        double radius = circle.GetRadius();
                        double[] circleAngles = [0.0, Math.PI / 2.0, Math.PI, Math.PI * 1.5];
                        int index = grip.AuxiliaryIndex;
                        if (index < 0 || index >= circleAngles.Length)
                        {
                            return null;
                        }

                        double angle = circleAngles[index];
                        return new PointD(
                            circle.Center.X + radius * Math.Cos(angle),
                            circle.Center.Y + radius * Math.Sin(angle));
                    }

                    return null;

                case ArcShape arc:
                    return grip.Kind switch
                    {
                        SelectionGripKind.Vertex when grip.VertexIndex == 0 => arc.StartPoint,
                        SelectionGripKind.Vertex when grip.VertexIndex == 1 => arc.EndPoint,
                        SelectionGripKind.ArcMidpoint => arc.MidPoint,
                        _ => null
                    };

                case EllipseShape ellipse:
                    RectangleD bounds = ellipse.GetBoundingBox();
                    PointD center = new(bounds.Left + bounds.Width / 2.0, bounds.Bottom + bounds.Height / 2.0);
                    if (grip.Kind == SelectionGripKind.EllipseCenter)
                    {
                        return center;
                    }

                    if (grip.Kind == SelectionGripKind.EllipseQuadrant)
                    {
                        PointD[] quadrants =
                        [
                            new PointD(bounds.Right, center.Y),
                            new PointD(center.X, bounds.Top),
                            new PointD(bounds.Left, center.Y),
                            new PointD(center.X, bounds.Bottom)
                        ];

                        int index = grip.AuxiliaryIndex;
                        return index >= 0 && index < quadrants.Length
                            ? quadrants[index]
                            : null;
                    }

                    return null;

                case TextShape text:
                    return grip.Kind == SelectionGripKind.TextPosition ? text.Position : null;
            }

            return null;
        }

        private bool ScreenQueryContainsSnapPoint(Rectangle screenQuery, SnapPoint snapPoint)
        {
            PointD screen = _engine.WorldToScreen(snapPoint.Position);
            return screenQuery.Contains(
                new Point(
                    (int)Math.Round(screen.X),
                    (int)Math.Round(screen.Y)));
        }

        private bool IsSnapCandidateFeature(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return feature.Shape.IsVisible &&
                   feature.CanvasObject.IsVisible &&
                   layer?.IsVisible == true &&
                   !IsActiveGripEditedShape(feature.Shape);
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

        private void UpdateHoveredSelectionGrip(Point screenPoint)
        {
            SelectionGrip? previousGrip = _hoveredSelectionGrip;
            _hoveredSelectionGrip = _activeTool == MapCanvasTool.Select && _activeGripEdit == null
                ? FindSelectionGripAtScreenPoint(screenPoint)
                : null;

            if (!SameSelectionGrip(previousGrip, _hoveredSelectionGrip))
            {
                UpdateCanvasCursor();
                RequestRender();
            }
        }

        private SelectionGrip? FindSelectionGripAtScreenPoint(Point screenPoint)
        {
            double nearestDistance = GripHitTolerancePixels;
            SelectionGrip? nearest = null;

            foreach (SelectionGrip grip in EnumerateSelectionGrips())
            {
                PointD screen = _engine.WorldToScreen(grip.Position);
                if (!double.IsFinite(screen.X) || !double.IsFinite(screen.Y))
                    continue;

                double distance = Math.Sqrt(
                    Math.Pow(screen.X - screenPoint.X, 2.0) +
                    Math.Pow(screen.Y - screenPoint.Y, 2.0));
                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = grip;
                }
            }

            return nearest;
        }

        private IEnumerable<SelectionGrip> EnumerateSelectionGrips()
        {
            if (_selectedShapeIds.Count == 0 || _activeTool != MapCanvasTool.Select)
            {
                yield break;
            }

            foreach (CanvasFeature feature in _vectorFeatures)
            {
                // Cheap O(1) selected-set check first so the per-feature layer
                // resolution in IsGripEditableFeature only runs for the (few)
                // selected features — grips exist only for selected shapes.
                // Keeps Select-mode hover fast on large scenes.
                if (!_selectedShapeIds.Contains(feature.Shape.Id))
                    continue;

                if (!IsGripEditableFeature(feature))
                    continue;

                foreach (SelectionGrip grip in CreateSelectionGrips(feature))
                {
                    yield return grip;
                }
            }
        }

        private bool IsGripEditableFeature(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return _activeTool == MapCanvasTool.Select &&
                   _selectedShapeIds.Contains(feature.Shape.Id) &&
                   feature.Shape.IsVisible &&
                   feature.CanvasObject.IsVisible &&
                   layer?.IsVisible == true &&
                   layer.IsSelectable &&
                   layer.IsLocked != true &&
                   CanvasLayerTreeService.IsDrawingMarkupLayer(layer);
        }

        private IEnumerable<SelectionGrip> CreateSelectionGrips(CanvasFeature feature)
        {
            IShape shape = feature.Shape;
            switch (shape)
            {
                case LineShape line:
                    yield return CreateGrip(feature, SelectionGripKind.Vertex, SelectionGripGlyph.Square, line.Start, vertexIndex: 0);
                    yield return CreateGrip(feature, SelectionGripKind.Vertex, SelectionGripGlyph.Square, line.End, vertexIndex: 1);
                    yield return CreateGrip(
                        feature,
                        SelectionGripKind.SegmentMidpoint,
                        SelectionGripGlyph.SegmentRectangle,
                        Midpoint(line.Start, line.End),
                        segmentStart: line.Start,
                        segmentEnd: line.End,
                        segmentIndex: 0);
                    break;

                case PolylineShape polyline:
                    foreach (SelectionGrip grip in CreatePolylineSelectionGrips(feature, polyline))
                        yield return grip;
                    break;

                case RectangleShape rectangle:
                    foreach (SelectionGrip grip in CreateRectangleSelectionGrips(feature, rectangle))
                        yield return grip;
                    break;

                case CircleShape circle:
                    yield return CreateGrip(feature, SelectionGripKind.CircleCenter, SelectionGripGlyph.Square, circle.Center);
                    double radius = circle.GetRadius();
                    double[] circleAngles = [0.0, Math.PI / 2.0, Math.PI, Math.PI * 1.5];
                    for (int i = 0; i < circleAngles.Length; i++)
                    {
                        double angle = circleAngles[i];
                        yield return CreateGrip(
                            feature,
                            SelectionGripKind.CircleQuadrant,
                            SelectionGripGlyph.Diamond,
                            new PointD(
                                circle.Center.X + radius * Math.Cos(angle),
                                circle.Center.Y + radius * Math.Sin(angle)),
                            auxiliaryIndex: i);
                    }
                    break;

                case ArcShape arc:
                    yield return CreateGrip(feature, SelectionGripKind.Vertex, SelectionGripGlyph.Square, arc.StartPoint, vertexIndex: 0);
                    yield return CreateGrip(feature, SelectionGripKind.ArcMidpoint, SelectionGripGlyph.SegmentRectangle, arc.MidPoint,
                        segmentStart: arc.StartPoint, segmentEnd: arc.EndPoint, segmentIndex: 0);
                    yield return CreateGrip(feature, SelectionGripKind.Vertex, SelectionGripGlyph.Square, arc.EndPoint, vertexIndex: 1);
                    break;

                case EllipseShape ellipse:
                    foreach (SelectionGrip grip in CreateEllipseSelectionGrips(feature, ellipse))
                        yield return grip;
                    break;

                case TextShape text:
                    yield return CreateGrip(feature, SelectionGripKind.TextPosition, SelectionGripGlyph.Square, text.Position);
                    break;
            }
        }

        private IEnumerable<SelectionGrip> CreatePolylineSelectionGrips(CanvasFeature feature, PolylineShape polyline)
        {
            if (polyline.Vertices.Count >= 2)
            {
                yield return CreateGrip(
                    feature,
                    SelectionGripKind.GeometricCenter,
                    SelectionGripGlyph.Diamond,
                    ComputePolylineGeometricCenter(polyline));
            }

            for (int i = 0; i < polyline.Vertices.Count; i++)
            {
                yield return CreateGrip(
                    feature,
                    SelectionGripKind.Vertex,
                    SelectionGripGlyph.Square,
                    polyline.Vertices[i],
                    vertexIndex: i);
            }

            if (polyline.Segments.Count > 0)
            {
                for (int i = 0; i < polyline.Segments.Count; i++)
                {
                    PolylineShape.PolylineSegment segment = polyline.Segments[i];
                    if (segment.Kind == PolylineShape.PolylineSegmentKind.Arc && segment.Arc != null)
                    {
                        yield return CreateGrip(
                            feature,
                            SelectionGripKind.ArcMidpoint,
                            SelectionGripGlyph.SegmentRectangle,
                            segment.Arc.MidPoint,
                            segmentStart: segment.Start,
                            segmentEnd: segment.End,
                            segmentIndex: i);
                        continue;
                    }

                    yield return CreateGrip(
                        feature,
                        SelectionGripKind.SegmentMidpoint,
                        SelectionGripGlyph.SegmentRectangle,
                        Midpoint(segment.Start, segment.End),
                        segmentStart: segment.Start,
                        segmentEnd: segment.End,
                        segmentIndex: i);
                }

                if (polyline.IsClosed && polyline.Vertices.Count > 2)
                {
                    PointD lastEnd = polyline.Segments[^1].End;
                    PointD first = polyline.Vertices[0];
                    if (DistanceWorld(lastEnd, first) > 1e-9)
                    {
                        yield return CreateGrip(
                            feature,
                            SelectionGripKind.SegmentMidpoint,
                            SelectionGripGlyph.SegmentRectangle,
                            Midpoint(lastEnd, first),
                            segmentStart: lastEnd,
                            segmentEnd: first,
                            segmentIndex: -1);
                    }
                }

                yield break;
            }

            int segmentCount = polyline.IsClosed && polyline.Vertices.Count > 2
                ? polyline.Vertices.Count
                : Math.Max(0, polyline.Vertices.Count - 1);
            for (int i = 0; i < segmentCount; i++)
            {
                PointD start = polyline.Vertices[i];
                PointD end = polyline.Vertices[(i + 1) % polyline.Vertices.Count];
                yield return CreateGrip(
                    feature,
                    SelectionGripKind.SegmentMidpoint,
                    SelectionGripGlyph.SegmentRectangle,
                    Midpoint(start, end),
                    segmentStart: start,
                    segmentEnd: end,
                    segmentIndex: i);
            }
        }

        private IEnumerable<SelectionGrip> CreateRectangleSelectionGrips(CanvasFeature feature, RectangleShape rectangle)
        {
            PointD[] corners = GetRectangleCorners(rectangle);

            yield return CreateGrip(
                feature,
                SelectionGripKind.GeometricCenter,
                SelectionGripGlyph.Diamond,
                ComputeRectangleCenter(rectangle));

            for (int i = 0; i < corners.Length; i++)
            {
                yield return CreateGrip(feature, SelectionGripKind.Vertex, SelectionGripGlyph.Square, corners[i], vertexIndex: i);
            }

            for (int i = 0; i < corners.Length; i++)
            {
                PointD start = corners[i];
                PointD end = corners[(i + 1) % corners.Length];
                yield return CreateGrip(
                    feature,
                    SelectionGripKind.SegmentMidpoint,
                    SelectionGripGlyph.SegmentRectangle,
                    Midpoint(start, end),
                    segmentStart: start,
                    segmentEnd: end,
                    segmentIndex: i);
            }
        }

        private IEnumerable<SelectionGrip> CreateEllipseSelectionGrips(CanvasFeature feature, EllipseShape ellipse)
        {
            RectangleD bounds = ellipse.GetBoundingBox();
            PointD center = new(bounds.Left + bounds.Width / 2.0, bounds.Bottom + bounds.Height / 2.0);
            PointD[] quadrants =
            [
                new PointD(bounds.Right, center.Y),
                new PointD(center.X, bounds.Top),
                new PointD(bounds.Left, center.Y),
                new PointD(center.X, bounds.Bottom)
            ];

            yield return CreateGrip(feature, SelectionGripKind.EllipseCenter, SelectionGripGlyph.Square, center);
            for (int i = 0; i < quadrants.Length; i++)
            {
                yield return CreateGrip(
                    feature,
                    SelectionGripKind.EllipseQuadrant,
                    SelectionGripGlyph.Diamond,
                    quadrants[i],
                    auxiliaryIndex: i);
            }
        }

        private static SelectionGrip CreateGrip(
            CanvasFeature feature,
            SelectionGripKind kind,
            SelectionGripGlyph glyph,
            PointD position,
            PointD? segmentStart = null,
            PointD? segmentEnd = null,
            int vertexIndex = -1,
            int segmentIndex = -1,
            int auxiliaryIndex = -1)
        {
            return new SelectionGrip
            {
                Feature = feature,
                Shape = feature.Shape,
                Kind = kind,
                Glyph = glyph,
                Position = position,
                SegmentStart = segmentStart ?? position,
                SegmentEnd = segmentEnd ?? position,
                VertexIndex = vertexIndex,
                SegmentIndex = segmentIndex,
                AuxiliaryIndex = auxiliaryIndex
            };
        }

        private static IShape CreateGripEditPreviewShape(IShape source)
        {
            IShape preview = source.Clone();
            // Preview renders with the shape's normal color/line weight (no
            // selection glow) so the user sees the actual geometry being edited.
            preview.IsSelected = false;
            return preview;
        }

        private static SelectionGrip CloneGripForShape(SelectionGrip grip, IShape shape)
        {
            return new SelectionGrip
            {
                Feature = grip.Feature,
                Shape = shape,
                Kind = grip.Kind,
                Glyph = grip.Glyph,
                Position = grip.Position,
                SegmentStart = grip.SegmentStart,
                SegmentEnd = grip.SegmentEnd,
                VertexIndex = grip.VertexIndex,
                SegmentIndex = grip.SegmentIndex,
                AuxiliaryIndex = grip.AuxiliaryIndex
            };
        }

        private bool BeginGripEdit(SelectionGrip grip, Point screenPoint)
        {
            if (!IsGripEditableFeature(grip.Feature))
                return false;

            // A geometric-center grip is a whole-shape MOVE. Snapshot the whole
            // canvas exactly as shown right now (cache + selection glow at the
            // original location) BEFORE entering the edit. While the move is
            // active only that frozen bitmap and the moving preview are painted
            // — no other rendering work per frame.
            if (grip.Kind == SelectionGripKind.GeometricCenter)
            {
                CaptureGripMoveFreezeBitmap();
            }

            IReadOnlyList<LinkedGripEdit> linkedEdits = FindLinkedGripEdits(grip);
            IShape originalShape = grip.Shape.Clone();
            IShape previewShape = CreateGripEditPreviewShape(grip.Shape);
            SelectionGrip previewGrip = CloneGripForShape(grip, previewShape);
            _activeGripEdit = new ActiveGripEdit(
                grip,
                originalShape,
                previewShape,
                previewGrip,
                screenPoint,
                linkedEdits);
            _hoveredSelectionGrip = grip;
            _currentMouseWorld = grip.Position;
            _isSelectingObjects = false;
            canvasSurface.Capture = true;
            _pendingEditedShapeIds.Clear();

            // A geometric-center grip is a whole-shape MOVE. Behave like the
            // "Move object(s)" menu: keep the shape in the cached background
            // (no exclusion, no synchronous base rebuild → no start hitch) and
            // just paint the moving preview on top. Vertex/handle edits still
            // exclude + rebuild so the cache shows the live deformation.
            if (grip.Kind != SelectionGripKind.GeometricCenter)
            {
                ApplyGripEditVectorRenderExclusions();
                RefreshVectorCacheForGripEditBase();
            }
            // Ensure we don't show glyph on the held grip
            ClearCurrentSnapPoint();
            UpdateCanvasCursor();
            UpdateStatusBar();
            RequestRender();
            return true;
        }

        private void ApplyActiveGripEdit(Point screenPoint)
        {
            if (_activeGripEdit == null)
                return;

            UpdateCurrentSnapPoint(screenPoint);
            PointD target;
            bool snappedToCandidate = false;
            if (_currentSnapPoint != null && !IsActiveGripSnapPoint(_currentSnapPoint))
            {
                snappedToCandidate = true;
                target = _currentSnapPoint.Position;
            }
            else
            {
                target = _engine.ScreenToWorld(screenPoint);
            }

            if (!snappedToCandidate)
            {
                target = ApplyGripEditOrthoConstraint(_activeGripEdit.Grip, _activeGripEdit.OriginalShape, target);
            }

            _activeGripEdit.CurrentWorldPoint = target;
            _currentMouseWorld = target;
            RestoreShapeGeometry(_activeGripEdit.PreviewShape, _activeGripEdit.OriginalShape);
            ApplyGripGeometry(_activeGripEdit.PreviewGrip, _activeGripEdit.OriginalShape, target);

            if (_activeGripEdit.LinkedEdits.Count > 0)
            {
                foreach (LinkedGripEdit linkedEdit in _activeGripEdit.LinkedEdits)
                {
                    RestoreShapeGeometry(linkedEdit.PreviewShape, linkedEdit.OriginalShape);
                    ApplyGripGeometry(linkedEdit.PreviewGrip, linkedEdit.OriginalShape, target);
                }
            }

            UpdateStatusBar();
        }

        private PointD ApplyGripEditOrthoConstraint(
            SelectionGrip grip,
            IShape originalShape,
            PointD target)
        {
            if (!_orthoModeEnabled)
                return target;

            if (TryResolveGripEditFixedAxis(grip, out OrthoConstraintAxis fixedAxis))
            {
                return OrthoConstraintService.Constrain(grip.Position, target, fixedAxis);
            }

            PointD anchor = ResolveGripEditOrthoAnchor(grip, originalShape);
            return OrthoConstraintService.ConstrainToDominantAxis(anchor, target);
        }

        private static bool TryResolveGripEditFixedAxis(
            SelectionGrip grip,
            out OrthoConstraintAxis axis)
        {
            axis = OrthoConstraintAxis.Horizontal;

            if (grip.Shape is not RectangleShape ||
                grip.Kind != SelectionGripKind.SegmentMidpoint)
            {
                return false;
            }

            axis = grip.SegmentIndex is 1 or 3
                ? OrthoConstraintAxis.Horizontal
                : OrthoConstraintAxis.Vertical;
            return true;
        }

        private static PointD ResolveGripEditOrthoAnchor(
            SelectionGrip grip,
            IShape originalShape)
        {
            if (grip.Kind == SelectionGripKind.CircleQuadrant &&
                originalShape is CircleShape circle)
            {
                return circle.Center;
            }

            if (grip.Kind == SelectionGripKind.EllipseQuadrant &&
                originalShape is EllipseShape ellipse)
            {
                RectangleD bounds = ellipse.GetBoundingBox();
                return new PointD(
                    bounds.Left + bounds.Width / 2.0,
                    bounds.Bottom + bounds.Height / 2.0);
            }

            if (grip.Kind == SelectionGripKind.CircleCenter &&
                originalShape is CircleShape centerCircle)
            {
                return centerCircle.Center;
            }

            if (grip.Kind == SelectionGripKind.EllipseCenter &&
                originalShape is EllipseShape centerEllipse)
            {
                RectangleD bounds = centerEllipse.GetBoundingBox();
                return new PointD(
                    bounds.Left + bounds.Width / 2.0,
                    bounds.Bottom + bounds.Height / 2.0);
            }

            return grip.Position;
        }

        private void CommitActiveGripEdit()
        {
            if (_activeGripEdit == null)
                return;

            if (_applicationEditLocked)
            {
                CancelActiveGripEdit(restoreOriginal: true);
                NotifyEditLocked();
                return;
            }

            SelectionGrip grip = _activeGripEdit.Grip;
            IReadOnlyList<LinkedGripEdit> linkedEdits = _activeGripEdit.LinkedEdits;
            List<CanvasFeature> editedFeatures = GetActiveGripEditedFeatures(grip, linkedEdits);
            RestoreShapeGeometry(grip.Shape, _activeGripEdit.PreviewShape);
            foreach (LinkedGripEdit linkedEdit in linkedEdits)
            {
                RestoreShapeGeometry(linkedEdit.Grip.Shape, linkedEdit.PreviewShape);
            }

            _activeGripEdit = null;
            _hoveredSelectionGrip = null;
            canvasSurface.Capture = false;
            DisposeGripMoveFreezeBitmap();

            CanvasLayer? gripLayer = ResolveFeatureLayer(grip.Feature);
            if (gripLayer != null)
            {
                grip.Shape.LayerName = gripLayer.Name;
                grip.Shape.Properties[CanvasFeatureService.CanvasLayerIdPropertyKey] = gripLayer.Id;
            }

            if (linkedEdits.Count > 0)
            {
                foreach (LinkedGripEdit linkedEdit in linkedEdits)
                {
                    CanvasLayer? linkedLayer = ResolveFeatureLayer(linkedEdit.Grip.Feature);
                    if (linkedLayer != null)
                    {
                        linkedEdit.Grip.Shape.LayerName = linkedLayer.Name;
                        linkedEdit.Grip.Shape.Properties[CanvasFeatureService.CanvasLayerIdPropertyKey] = linkedLayer.Id;
                    }
                }
            }

            bool wholeShapeMove = grip.Kind == SelectionGripKind.GeometricCenter;
            _pendingEditedShapeIds.Clear();

            if (wholeShapeMove)
            {
                // Geometric-center move kept the shape in the cache at its old
                // position, so rebuild now (like the "Move object(s)" commit) to
                // bake the new position and avoid a ghost at the old spot.
                _immediateEditedOverlayFeatures = Array.Empty<CanvasFeature>();
                _renderer.SetVectorRenderExclusions(null, invalidateCache: false);
                _renderer.UpdateVectorFeatures(_vectorFeatures);
                UpdateWorldBounds();
                EnsureVectorZoomSnapshot();
                _holdVectorZoomFrameUntilRefresh = true;
                RefreshVectorCacheForCurrentViewAsync();
            }
            else
            {
                _immediateEditedOverlayFeatures = editedFeatures;
                // Keep the bitmap cache (which already excludes the edited shapes)
                // and paint the edited shapes as an overlay on top — do NOT trigger
                // a full re-render here. The cache + edited-shape overlay continues
                // to render until another canvas operation bakes it in (the DB
                // reload below takes the incremental fast-path in SetVectorFeatures,
                // and pan/zoom/etc. rebuild lazily). This keeps editing smooth.
                foreach (CanvasFeature editedFeature in editedFeatures)
                {
                    _pendingEditedShapeIds.Add(editedFeature.Shape.Id);
                }

                _renderer.SetVectorRenderExclusions(null, invalidateCache: false);
                _renderer.UpdateVectorFeatures(_vectorFeatures, invalidateCache: false);
                UpdateWorldBounds();
            }

            UpdateCanvasCursor();
            UpdateStatusBar();
            RequestRender();

            HashSet<Guid> editedShapeIds = new();
            List<IShape> editedShapes = new();
            if (editedShapeIds.Add(grip.Shape.Id))
            {
                editedShapes.Add(grip.Shape);
            }

            foreach (LinkedGripEdit linkedEdit in linkedEdits)
            {
                if (editedShapeIds.Add(linkedEdit.Grip.Shape.Id))
                {
                    editedShapes.Add(linkedEdit.Grip.Shape);
                }
            }

            // Persist the grip edit (plus any linked edits) as ONE batch.
            if (editedShapes.Count > 0)
            {
                ShapesEdited?.Invoke(editedShapes);
            }

            if (editedShapes.Count > 1)
            {
                _commandService.LogCommand($"Edited {editedShapes.Count} objects");
            }
            else
            {
                _commandService.LogCommand($"Edited {grip.Feature.CanvasObject.ObjectType}");
            }
        }

        private void CancelActiveGripEdit(bool restoreOriginal)
        {
            if (_activeGripEdit == null)
                return;

            _activeGripEdit = null;
            _hoveredSelectionGrip = null;
            canvasSurface.Capture = false;
            DisposeGripMoveFreezeBitmap();
            _renderer.SetVectorRenderExclusions(null);
            EnsureVectorZoomSnapshot();
            _holdVectorZoomFrameUntilRefresh = true;
            RefreshVectorCacheForCurrentViewAsync();
            UpdateCanvasCursor();
            UpdateStatusBar();
            RequestRender();
        }

        private static List<CanvasFeature> GetActiveGripEditedFeatures(
            SelectionGrip grip,
            IReadOnlyList<LinkedGripEdit> linkedEdits)
        {
            HashSet<Guid> editedShapeIds = new();
            List<CanvasFeature> editedFeatures = new();
            if (editedShapeIds.Add(grip.Shape.Id))
            {
                editedFeatures.Add(grip.Feature);
            }

            foreach (LinkedGripEdit linkedEdit in linkedEdits)
            {
                if (editedShapeIds.Add(linkedEdit.Grip.Shape.Id))
                {
                    editedFeatures.Add(linkedEdit.Grip.Feature);
                }
            }

            return editedFeatures;
        }

        private void ApplyGripEditVectorRenderExclusions()
        {
            _renderer.SetVectorRenderExclusions(GetActiveGripEditedShapeIds());
        }

        private IReadOnlyList<Guid> GetActiveGripEditedShapeIds()
        {
            if (_activeGripEdit == null)
            {
                return [];
            }

            HashSet<Guid> ids = new();
            ids.Add(_activeGripEdit.Grip.Shape.Id);
            foreach (LinkedGripEdit linkedEdit in _activeGripEdit.LinkedEdits)
            {
                ids.Add(linkedEdit.Grip.Shape.Id);
            }

            return ids.ToArray();
        }

        private void RefreshVectorCacheForGripEditBase()
        {
            if (IsDisposed || Disposing || canvasSurface.IsDisposed)
            {
                return;
            }

            CancelPendingVectorRender();
            _vectorRenderGeneration++;

            try
            {
                _renderer.RefreshVectorCache(canvasSurface.Size);
                _renderer.EndVectorZoom();
                _holdVectorPanFrameUntilRefresh = false;
                _holdVectorZoomFrameUntilRefresh = false;
                _heldVectorPanDelta = PointF.Empty;
                DisposeCompositePanBitmap();
            }
            catch (OperationCanceledException)
            {
                // A stale vector refresh was canceled before the edit-base cache was created.
            }
            catch (ObjectDisposedException)
            {
                // The control or renderer was disposed while preparing the edit-base cache.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Grip edit base vector cache refresh failed: {ex.Message}");
            }
            finally
            {
                _vectorCacheRefreshPending = false;
            }
        }

        private IReadOnlyList<LinkedGripEdit> FindLinkedGripEdits(SelectionGrip grip)
        {
            if (_selectedShapeIds.Count <= 1)
            {
                return Array.Empty<LinkedGripEdit>();
            }

            Dictionary<Guid, SelectionGrip> linked = new();
            PointD anchor = grip.Position;

            foreach (SelectionGrip candidate in EnumerateSelectionGrips())
            {
                if (candidate.Shape.Id == grip.Shape.Id)
                {
                    continue;
                }

                if (DistanceWorld(candidate.Position, anchor) > CommonGripToleranceWorld)
                {
                    continue;
                }

                if (!linked.TryGetValue(candidate.Shape.Id, out SelectionGrip? existing) ||
                    GetGripLinkPriority(candidate.Kind) < GetGripLinkPriority(existing.Kind))
                {
                    linked[candidate.Shape.Id] = candidate;
                }
            }

            if (linked.Count == 0)
            {
                return Array.Empty<LinkedGripEdit>();
            }

            List<LinkedGripEdit> edits = new(linked.Count);
            foreach (SelectionGrip candidate in linked.Values)
            {
                edits.Add(new LinkedGripEdit(candidate));
            }

            return edits;
        }

        private static int GetGripLinkPriority(SelectionGripKind kind)
        {
            return kind switch
            {
                SelectionGripKind.Vertex => 0,
                SelectionGripKind.ArcMidpoint => 1,
                SelectionGripKind.SegmentMidpoint => 2,
                SelectionGripKind.TextPosition => 3,
                SelectionGripKind.CircleCenter => 4,
                SelectionGripKind.CircleQuadrant => 5,
                SelectionGripKind.EllipseCenter => 6,
                SelectionGripKind.EllipseQuadrant => 7,
                _ => 8
            };
        }

        private void ApplyGripGeometry(SelectionGrip grip, IShape originalShape, PointD target)
        {
            switch (grip.Shape)
            {
                case LineShape line when originalShape is LineShape originalLine:
                    ApplyLineGrip(line, originalLine, grip, target);
                    break;

                case PolylineShape polyline when originalShape is PolylineShape originalPolyline:
                    ApplyPolylineGrip(polyline, originalPolyline, grip, target);
                    break;

                case RectangleShape rectangle when originalShape is RectangleShape originalRectangle:
                    ApplyRectangleGrip(rectangle, originalRectangle, grip, target);
                    break;

                case CircleShape circle when originalShape is CircleShape originalCircle:
                    ApplyCircleGrip(circle, originalCircle, grip, target);
                    break;

                case ArcShape arc when originalShape is ArcShape originalArc:
                    ApplyArcGrip(arc, originalArc, grip, target);
                    break;

                case EllipseShape ellipse when originalShape is EllipseShape originalEllipse:
                    ApplyEllipseGrip(ellipse, originalEllipse, grip, target);
                    break;

                case TextShape text when originalShape is TextShape originalText:
                    text.Translate(target - originalText.Position);
                    break;
            }
        }

        private static void ApplyLineGrip(LineShape line, LineShape originalLine, SelectionGrip grip, PointD target)
        {
            if (grip.Kind == SelectionGripKind.SegmentMidpoint)
            {
                PointD delta = target - grip.Position;
                line.Start = originalLine.Start + delta;
                line.End = originalLine.End + delta;
                return;
            }

            if (grip.VertexIndex == 0)
            {
                line.Start = target;
            }
            else if (grip.VertexIndex == 1)
            {
                line.End = target;
            }
        }

        private static void ApplyPolylineGrip(
            PolylineShape polyline,
            PolylineShape originalPolyline,
            SelectionGrip grip,
            PointD target)
        {
            if (grip.Kind == SelectionGripKind.GeometricCenter)
            {
                PointD originalCenter = ComputePolylineGeometricCenter(originalPolyline);
                PointD delta = target - originalCenter;
                for (int i = 0; i < polyline.Vertices.Count && i < originalPolyline.Vertices.Count; i++)
                {
                    polyline.Vertices[i] = originalPolyline.Vertices[i] + delta;
                }
                for (int i = 0; i < polyline.Segments.Count && i < originalPolyline.Segments.Count; i++)
                {
                    PolylineShape.PolylineSegment src = originalPolyline.Segments[i];
                    PolylineShape.PolylineSegment dst = polyline.Segments[i];
                    dst.Kind = src.Kind;
                    dst.Start = src.Start + delta;
                    dst.End = src.End + delta;
                    if (src.Arc != null)
                    {
                        dst.Arc = new ArcShape(
                            src.Arc.Center + delta,
                            src.Arc.Radius,
                            src.Arc.StartAngleRadians,
                            src.Arc.SweepAngleRadians);
                    }
                    else
                    {
                        dst.Arc = null;
                    }
                }
                return;
            }

            if (grip.Kind == SelectionGripKind.Vertex)
            {
                if (grip.VertexIndex < 0 || grip.VertexIndex >= polyline.Vertices.Count)
                    return;

                polyline.Vertices[grip.VertexIndex] = target;
                SynchronizePolylineSegmentsFromVertices(polyline, originalPolyline);
                return;
            }

            if (grip.Kind == SelectionGripKind.SegmentMidpoint)
            {
                PointD delta = target - grip.Position;
                if (polyline.Segments.Count > 0)
                {
                    MovePolylineSegmentVertices(polyline, grip.SegmentIndex, delta);
                    SynchronizePolylineSegmentsFromVertices(polyline, originalPolyline);
                    return;
                }

                int startIndex = grip.SegmentIndex;
                int endIndex = (grip.SegmentIndex + 1) % Math.Max(1, polyline.Vertices.Count);
                if (startIndex >= 0 && startIndex < polyline.Vertices.Count)
                {
                    polyline.Vertices[startIndex] = originalPolyline.Vertices[startIndex] + delta;
                }

                if (endIndex >= 0 && endIndex < polyline.Vertices.Count && endIndex != startIndex)
                {
                    polyline.Vertices[endIndex] = originalPolyline.Vertices[endIndex] + delta;
                }

                return;
            }

            if (grip.Kind == SelectionGripKind.ArcMidpoint &&
                grip.SegmentIndex >= 0 &&
                grip.SegmentIndex < polyline.Segments.Count)
            {
                PolylineShape.PolylineSegment segment = polyline.Segments[grip.SegmentIndex];
                PolylineShape.PolylineSegment originalSegment = originalPolyline.Segments[grip.SegmentIndex];
                if (originalSegment.Arc == null)
                    return;

                ArcShape? newArc = ArcShape.FromThreePoints(
                    originalSegment.Start,
                    target,
                    originalSegment.End);
                if (newArc == null)
                    return;

                segment.Kind = PolylineShape.PolylineSegmentKind.Arc;
                segment.Start = originalSegment.Start;
                segment.End = originalSegment.End;
                segment.Arc = newArc;
            }
        }

        private static void MovePolylineSegmentVertices(PolylineShape polyline, int segmentIndex, PointD delta)
        {
            if (segmentIndex == -1)
            {
                if (polyline.Vertices.Count >= 2)
                {
                    polyline.Vertices[^1] += delta;
                    polyline.Vertices[0] += delta;
                }

                return;
            }

            if (segmentIndex < 0 || segmentIndex >= polyline.Segments.Count)
                return;

            if (segmentIndex < polyline.Vertices.Count)
            {
                polyline.Vertices[segmentIndex] += delta;
            }

            int endIndex = segmentIndex + 1;
            if (endIndex < polyline.Vertices.Count)
            {
                polyline.Vertices[endIndex] += delta;
            }
        }

        private static void SynchronizePolylineSegmentsFromVertices(
            PolylineShape polyline,
            PolylineShape originalPolyline)
        {
            if (polyline.Segments.Count == 0)
                return;

            int segmentCount = Math.Min(polyline.Segments.Count, originalPolyline.Segments.Count);
            for (int i = 0; i < segmentCount; i++)
            {
                if (i >= polyline.Vertices.Count)
                    break;

                PolylineShape.PolylineSegment segment = polyline.Segments[i];
                PolylineShape.PolylineSegment originalSegment = originalPolyline.Segments[i];
                PointD start = polyline.Vertices[i];
                PointD end;
                if (i + 1 < polyline.Vertices.Count)
                    end = polyline.Vertices[i + 1];
                else if (polyline.IsClosed && polyline.Vertices.Count > 0)
                    end = polyline.Vertices[0];
                else
                    end = segment.End;

                segment.Start = start;
                segment.End = end;

                if (segment.Kind == PolylineShape.PolylineSegmentKind.Arc && originalSegment.Arc != null)
                {
                    PointD adaptedMid = ComputeAdaptedArcMidPoint(
                        originalSegment.Arc.MidPoint,
                        originalSegment.Start, originalSegment.End,
                        start, end);
                    segment.Arc = ArcShape.FromThreePoints(start, adaptedMid, end);
                    if (segment.Arc == null)
                    {
                        segment.Kind = PolylineShape.PolylineSegmentKind.Line;
                    }
                }
            }
        }

        private static PointD ComputeAdaptedArcMidPoint(
            PointD originalArcMidPoint,
            PointD originalStart, PointD originalEnd,
            PointD newStart, PointD newEnd)
        {
            // Preserve the sagitta-to-chord ratio so the arc curvature character survives
            // as the vertex moves, even when the original midpoint would be collinear with
            // the new endpoints.
            double origDx = originalEnd.X - originalStart.X;
            double origDy = originalEnd.Y - originalStart.Y;
            double origChordLen = Math.Sqrt(origDx * origDx + origDy * origDy);
            if (origChordLen < 1e-9)
                return originalArcMidPoint;

            double origPerpX = -origDy / origChordLen;
            double origPerpY = origDx / origChordLen;

            double origMidX = (originalStart.X + originalEnd.X) * 0.5;
            double origMidY = (originalStart.Y + originalEnd.Y) * 0.5;
            double sagX = originalArcMidPoint.X - origMidX;
            double sagY = originalArcMidPoint.Y - origMidY;
            double ratio = (sagX * origPerpX + sagY * origPerpY) / origChordLen;

            double newDx = newEnd.X - newStart.X;
            double newDy = newEnd.Y - newStart.Y;
            double newChordLen = Math.Sqrt(newDx * newDx + newDy * newDy);
            if (newChordLen < 1e-9)
                return originalArcMidPoint;

            double newPerpX = -newDy / newChordLen;
            double newPerpY = newDx / newChordLen;

            double newSagitta = ratio * newChordLen;
            return new PointD(
                (newStart.X + newEnd.X) * 0.5 + newPerpX * newSagitta,
                (newStart.Y + newEnd.Y) * 0.5 + newPerpY * newSagitta);
        }

        private static void ApplyRectangleGrip(
            RectangleShape rectangle,
            RectangleShape originalRectangle,
            SelectionGrip grip,
            PointD target)
        {
            RectangleD bounds = originalRectangle.GetBoundingBox();
            double left = bounds.Left;
            double right = bounds.Right;
            double bottom = bounds.Bottom;
            double top = bounds.Top;

            if (grip.Kind == SelectionGripKind.GeometricCenter)
            {
                PointD originalCenter = ComputeRectangleCenter(originalRectangle);
                PointD delta = target - originalCenter;
                rectangle.Start = new PointD(left + delta.X, bottom + delta.Y);
                rectangle.End = new PointD(right + delta.X, top + delta.Y);
                return;
            }

            if (grip.Kind == SelectionGripKind.Vertex)
            {
                switch (grip.VertexIndex)
                {
                    case 0:
                        left = target.X;
                        bottom = target.Y;
                        break;
                    case 1:
                        right = target.X;
                        bottom = target.Y;
                        break;
                    case 2:
                        right = target.X;
                        top = target.Y;
                        break;
                    case 3:
                        left = target.X;
                        top = target.Y;
                        break;
                }
            }
            else if (grip.Kind == SelectionGripKind.SegmentMidpoint)
            {
                switch (grip.SegmentIndex)
                {
                    case 0:
                        bottom = target.Y;
                        break;
                    case 1:
                        right = target.X;
                        break;
                    case 2:
                        top = target.Y;
                        break;
                    case 3:
                        left = target.X;
                        break;
                }
            }

            rectangle.Start = new PointD(left, bottom);
            rectangle.End = new PointD(right, top);
        }

        private static void ApplyCircleGrip(CircleShape circle, CircleShape originalCircle, SelectionGrip grip, PointD target)
        {
            if (grip.Kind == SelectionGripKind.CircleCenter)
            {
                PointD delta = target - originalCircle.Center;
                circle.Center = originalCircle.Center + delta;
                circle.RadiusPoint = originalCircle.RadiusPoint + delta;
                return;
            }

            if (grip.Kind == SelectionGripKind.CircleQuadrant &&
                DistanceWorld(originalCircle.Center, target) > 1e-9)
            {
                circle.Center = originalCircle.Center;
                circle.RadiusPoint = target;
            }
        }

        private static void ApplyArcGrip(ArcShape arc, ArcShape originalArc, SelectionGrip grip, PointD target)
        {
            ArcShape? newArc = grip.Kind switch
            {
                SelectionGripKind.Vertex when grip.VertexIndex == 0 => ArcShape.FromThreePoints(
                    target,
                    originalArc.MidPoint,
                    originalArc.EndPoint),
                SelectionGripKind.Vertex when grip.VertexIndex == 1 => ArcShape.FromThreePoints(
                    originalArc.StartPoint,
                    originalArc.MidPoint,
                    target),
                SelectionGripKind.ArcMidpoint => ArcShape.FromThreePoints(
                    originalArc.StartPoint,
                    target,
                    originalArc.EndPoint),
                _ => null
            };

            if (newArc == null)
                return;

            arc.Center = newArc.Center;
            arc.Radius = newArc.Radius;
            arc.StartAngleRadians = newArc.StartAngleRadians;
            arc.SweepAngleRadians = newArc.SweepAngleRadians;
        }

        private static void ApplyEllipseGrip(
            EllipseShape ellipse,
            EllipseShape originalEllipse,
            SelectionGrip grip,
            PointD target)
        {
            RectangleD bounds = originalEllipse.GetBoundingBox();
            double left = bounds.Left;
            double right = bounds.Right;
            double bottom = bounds.Bottom;
            double top = bounds.Top;
            PointD center = new(left + bounds.Width / 2.0, bottom + bounds.Height / 2.0);

            if (grip.Kind == SelectionGripKind.EllipseCenter)
            {
                PointD delta = target - center;
                ellipse.Start = originalEllipse.Start + delta;
                ellipse.End = originalEllipse.End + delta;
                return;
            }

            if (grip.Kind != SelectionGripKind.EllipseQuadrant)
                return;

            switch (grip.AuxiliaryIndex)
            {
                case 0:
                    right = target.X;
                    left = center.X - (right - center.X);
                    break;
                case 1:
                    top = target.Y;
                    bottom = center.Y - (top - center.Y);
                    break;
                case 2:
                    left = target.X;
                    right = center.X + (center.X - left);
                    break;
                case 3:
                    bottom = target.Y;
                    top = center.Y + (center.Y - bottom);
                    break;
            }

            ellipse.Start = new PointD(left, bottom);
            ellipse.End = new PointD(right, top);
        }

        private static void RestoreShapeGeometry(IShape target, IShape source)
        {
            switch (target, source)
            {
                case (LineShape targetLine, LineShape sourceLine):
                    targetLine.Start = sourceLine.Start;
                    targetLine.End = sourceLine.End;
                    break;

                case (PolylineShape targetPolyline, PolylineShape sourcePolyline):
                    targetPolyline.Vertices = sourcePolyline.Vertices.ToList();
                    targetPolyline.Segments = sourcePolyline.Segments
                        .Select(ClonePolylineSegment)
                        .ToList();
                    targetPolyline.IsClosed = sourcePolyline.IsClosed;
                    break;

                case (RectangleShape targetRectangle, RectangleShape sourceRectangle):
                    targetRectangle.Start = sourceRectangle.Start;
                    targetRectangle.End = sourceRectangle.End;
                    break;

                case (CircleShape targetCircle, CircleShape sourceCircle):
                    targetCircle.Center = sourceCircle.Center;
                    targetCircle.RadiusPoint = sourceCircle.RadiusPoint;
                    break;

                case (ArcShape targetArc, ArcShape sourceArc):
                    targetArc.Center = sourceArc.Center;
                    targetArc.Radius = sourceArc.Radius;
                    targetArc.StartAngleRadians = sourceArc.StartAngleRadians;
                    targetArc.SweepAngleRadians = sourceArc.SweepAngleRadians;
                    break;

                case (EllipseShape targetEllipse, EllipseShape sourceEllipse):
                    targetEllipse.Start = sourceEllipse.Start;
                    targetEllipse.End = sourceEllipse.End;
                    break;

                case (TextShape targetText, TextShape sourceText):
                    targetText.Translate(sourceText.Position - targetText.Position);
                    break;
            }
        }

        private static PolylineShape.PolylineSegment ClonePolylineSegment(PolylineShape.PolylineSegment segment)
        {
            return new PolylineShape.PolylineSegment(
                segment.Kind,
                segment.Start,
                segment.End,
                segment.Arc == null
                    ? null
                    : new ArcShape(
                        segment.Arc.Center,
                        segment.Arc.Radius,
                        segment.Arc.StartAngleRadians,
                        segment.Arc.SweepAngleRadians));
        }

        private void DrawSelectionGrips(Graphics graphics)
        {
            if (_activeTool != MapCanvasTool.Select || _selectedShapeIds.Count == 0)
                return;

            SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.None;
            try
            {
                foreach (SelectionGrip grip in EnumerateSelectionGrips())
                {
                    bool isHot = SameSelectionGrip(grip, _hoveredSelectionGrip) ||
                                 (_activeGripEdit != null && SameSelectionGrip(grip, _activeGripEdit.Grip));
                    DrawSelectionGrip(graphics, grip, isHot);
                }
            }
            finally
            {
                graphics.SmoothingMode = previousSmoothingMode;
            }
        }

        private void DrawActiveGripEditOverlay(Graphics graphics)
        {
            if (_activeGripEdit == null)
                return;

            _renderer.RenderTransientShape(
                graphics,
                _activeGripEdit.PreviewShape,
                ResolveFeatureLayer(_activeGripEdit.Grip.Feature),
                _activeGripEdit.Grip.Feature.CanvasObject,
                forceUnselected: true);

            foreach (LinkedGripEdit linkedEdit in _activeGripEdit.LinkedEdits)
            {
                _renderer.RenderTransientShape(
                    graphics,
                    linkedEdit.PreviewShape,
                    ResolveFeatureLayer(linkedEdit.Grip.Feature),
                    linkedEdit.Grip.Feature.CanvasObject,
                    forceUnselected: true);
            }
        }

        private void DrawImmediateEditedFeatureOverlay(Graphics graphics)
        {
            if (_immediateEditedOverlayFeatures.Count == 0)
            {
                return;
            }

            foreach (CanvasFeature feature in _immediateEditedOverlayFeatures)
            {
                _renderer.RenderTransientShape(
                    graphics,
                    feature.Shape,
                    ResolveFeatureLayer(feature),
                    feature.CanvasObject);
            }
        }

        private void DrawJustCompletedShapeOverlay(Graphics graphics)
        {
            IShape? shape = _justCompletedShape;
            if (shape == null)
            {
                return;
            }

            // Render with the active drawing layer's full style so the bridge
            // frame between mouse-up and the SetVectorFeatures overlay matches
            // exactly what the final cached render will look like.
            _renderer.RenderTransientShape(
                graphics,
                shape,
                _justCompletedShapeLayer);
        }

        private void DrawActiveGripOriginalOverlay(Graphics graphics)
        {
            if (_activeGripEdit == null)
                return;

            // For a geometric-center (whole-shape) move the original is left in
            // the cached background (move-menu style), so don't redraw it here —
            // that avoids a doubled static copy and extra per-frame rendering.
            if (_activeGripEdit.Grip.Kind == SelectionGripKind.GeometricCenter)
                return;

            _renderer.RenderTransientShape(
                graphics,
                _activeGripEdit.Grip.Shape,
                ResolveFeatureLayer(_activeGripEdit.Grip.Feature),
                _activeGripEdit.Grip.Feature.CanvasObject,
                forceUnselected: true);

            foreach (LinkedGripEdit linkedEdit in _activeGripEdit.LinkedEdits)
            {
                _renderer.RenderTransientShape(
                    graphics,
                    linkedEdit.Grip.Shape,
                    ResolveFeatureLayer(linkedEdit.Grip.Feature),
                    linkedEdit.Grip.Feature.CanvasObject,
                    forceUnselected: true);
            }
        }

        private void DrawSelectionGrip(Graphics graphics, SelectionGrip grip, bool isHot)
        {
            PointD screen = _engine.WorldToScreen(grip.Position);
            if (!double.IsFinite(screen.X) || !double.IsFinite(screen.Y))
                return;

            Color fillColor = isHot
                ? Color.FromArgb(255, 255, 96, 32)
                : Color.FromArgb(255, 0, 122, 204);
            Color outlineColor = isHot
                ? Color.FromArgb(255, 120, 36, 0)
                : Color.FromArgb(255, 0, 60, 115);

            using SolidBrush brush = new(fillColor);
            using Pen pen = new(outlineColor, 1.0f);
            PointF center = new((float)screen.X, (float)screen.Y);

            switch (grip.Glyph)
            {
                case SelectionGripGlyph.Diamond:
                    DrawDiamondGrip(graphics, center, brush, pen);
                    break;
                case SelectionGripGlyph.SegmentRectangle:
                    DrawSegmentRectangleGrip(graphics, grip, center, brush, pen);
                    break;
                default:
                    DrawSquareGrip(graphics, center, brush, pen);
                    break;
            }
        }

        private static void DrawSquareGrip(Graphics graphics, PointF center, Brush brush, Pen pen)
        {
            float half = GripSquareSizePixels / 2.0f;
            RectangleF rect = new(center.X - half, center.Y - half, GripSquareSizePixels, GripSquareSizePixels);
            graphics.FillRectangle(brush, rect);
            graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        private static void DrawDiamondGrip(Graphics graphics, PointF center, Brush brush, Pen pen)
        {
            float half = GripSquareSizePixels / 2.0f;
            PointF[] points =
            [
                new(center.X, center.Y - half),
                new(center.X + half, center.Y),
                new(center.X, center.Y + half),
                new(center.X - half, center.Y)
            ];
            graphics.FillPolygon(brush, points);
            graphics.DrawPolygon(pen, points);
        }

        private void DrawSegmentRectangleGrip(Graphics graphics, SelectionGrip grip, PointF center, Brush brush, Pen pen)
        {
            PointF start = ToScreenPointF(grip.SegmentStart);
            PointF end = ToScreenPointF(grip.SegmentEnd);
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length <= 0.0001)
            {
                DrawSquareGrip(graphics, center, brush, pen);
                return;
            }

            float ux = (float)(dx / length);
            float uy = (float)(dy / length);
            float nx = -uy;
            float ny = ux;
            float halfLength = GripSegmentLengthPixels / 2.0f;
            float halfThickness = GripSegmentThicknessPixels / 2.0f;
            PointF[] points =
            [
                new(center.X - ux * halfLength - nx * halfThickness, center.Y - uy * halfLength - ny * halfThickness),
                new(center.X + ux * halfLength - nx * halfThickness, center.Y + uy * halfLength - ny * halfThickness),
                new(center.X + ux * halfLength + nx * halfThickness, center.Y + uy * halfLength + ny * halfThickness),
                new(center.X - ux * halfLength + nx * halfThickness, center.Y - uy * halfLength + ny * halfThickness)
            ];
            graphics.FillPolygon(brush, points);
            graphics.DrawPolygon(pen, points);
        }

        private static bool SameSelectionGrip(SelectionGrip? first, SelectionGrip? second)
        {
            if (first == null || second == null)
                return first == null && second == null;

            return first.Shape.Id == second.Shape.Id &&
                   first.Kind == second.Kind &&
                   first.VertexIndex == second.VertexIndex &&
                   first.SegmentIndex == second.SegmentIndex &&
                   first.AuxiliaryIndex == second.AuxiliaryIndex;
        }

        private static PointD Midpoint(PointD first, PointD second)
        {
            return new PointD((first.X + second.X) / 2.0, (first.Y + second.Y) / 2.0);
        }

        private static PointD[] GetRectangleCorners(RectangleShape rectangle)
        {
            RectangleD bounds = rectangle.GetBoundingBox();
            return
            [
                new PointD(bounds.Left, bounds.Bottom),
                new PointD(bounds.Right, bounds.Bottom),
                new PointD(bounds.Right, bounds.Top),
                new PointD(bounds.Left, bounds.Top)
            ];
        }

        private static PointD ComputeRectangleCenter(RectangleShape rectangle)
        {
            RectangleD bounds = rectangle.GetBoundingBox();
            return new PointD(
                bounds.Left + bounds.Width / 2.0,
                bounds.Bottom + bounds.Height / 2.0);
        }

        private static PointD ComputePolylineGeometricCenter(PolylineShape polyline)
        {
            // Closed polygons (>= 3 vertices) use the area-weighted (shoelace) centroid so the
            // grip lands inside the shape's actual interior. Open polylines and degenerate cases
            // fall back to the vertex average.
            var vertices = polyline.Vertices;
            if (vertices.Count == 0)
                return new PointD(0, 0);

            if (polyline.IsClosed && vertices.Count >= 3)
            {
                double area = 0.0;
                double cx = 0.0;
                double cy = 0.0;
                for (int i = 0; i < vertices.Count; i++)
                {
                    PointD a = vertices[i];
                    PointD b = vertices[(i + 1) % vertices.Count];
                    double cross = a.X * b.Y - b.X * a.Y;
                    area += cross;
                    cx += (a.X + b.X) * cross;
                    cy += (a.Y + b.Y) * cross;
                }
                area *= 0.5;
                if (Math.Abs(area) > 1e-12)
                {
                    cx /= 6.0 * area;
                    cy /= 6.0 * area;
                    return new PointD(cx, cy);
                }
            }

            double sx = 0, sy = 0;
            foreach (PointD v in vertices)
            {
                sx += v.X;
                sy += v.Y;
            }
            return new PointD(sx / vertices.Count, sy / vertices.Count);
        }

        private void DrawSnapGlyph(Graphics graphics)
        {
            if (_currentSnapPoint == null || IsActiveGripSnapPoint(_currentSnapPoint))
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
                .Where(feature => feature.Shape is TextShape textShape
                    ? IsTextShapeSelectedByScreenRectangle(
                        textShape,
                        selectionRectangle,
                        requireContained: isWindowSelection)
                    : isWindowSelection
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
            CanvasFeature? hitFeature = FindSelectableFeatureAtScreenPoint(screenPoint);

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
            // The selection glow is baked into the cached bitmap, so rebuild the
            // cache off the UI thread; every frame stays a fast cache blit.
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
            UpdateStatusBar();
            NotifySelectedCanvasObjectsChanged();
        }

        private CanvasFeature? FindSelectableFeatureAtScreenPoint(Point screenPoint)
        {
            PointD worldPoint = _engine.ScreenToWorld(new PointD(screenPoint.X, screenPoint.Y));
            double worldTolerance = _engine.ScreenToWorldDistance(ObjectSelectionTolerancePixels);

            // Check text shapes first using their accurate screen-space bounds.
            return FindTextShapeHitAtScreenPoint(screenPoint)
                ?? FindClickHitFeature(worldPoint, worldTolerance);
        }

        private static bool IsTextShapeSelectedByScreenRectangle(
            TextShape textShape,
            Rectangle selectionRectangle,
            bool requireContained)
        {
            if (!textShape.LastRenderedBounds.HasValue)
            {
                return false;
            }

            RectangleF textBounds = textShape.LastRenderedBounds.Value;
            if (textBounds.Width <= 0f || textBounds.Height <= 0f)
            {
                return false;
            }

            textBounds.Inflate(2f, 2f);
            RectangleF selectionBounds = new(
                selectionRectangle.Left,
                selectionRectangle.Top,
                selectionRectangle.Width,
                selectionRectangle.Height);

            return requireContained
                ? selectionBounds.Contains(textBounds)
                : selectionBounds.IntersectsWith(textBounds);
        }

        private CanvasFeature? FindClickHitFeature(PointD worldPoint, double toleranceWorld)
        {
            NtsGeometry pickPoint = SelectionGeometryFactory.CreatePoint(
                new NtsCoordinate(worldPoint.X, worldPoint.Y));

            // Build candidates in the inverse of vector render order so the visually
            // topmost selectable layer receives the first hit opportunity.
            List<(CanvasFeature Feature, NtsGeometry Geometry)> candidates = _vectorFeatures
                .Where(IsSelectableDrawingFeature)
                .Where(feature => feature.Shape is not TextShape)
                .OrderByDescending(GetSelectionDrawingMarkupRenderPass)
                .ThenByDescending(GetSelectionBuildingFootprintRenderPass)
                .ThenByDescending(GetSelectionRePlotGroupRenderPass)
                .ThenByDescending(GetSelectionCadastralParcelRenderPass)
                .ThenByDescending(GetSelectionProjectBoundaryRenderPass)
                .ThenByDescending(GetSelectionDisplayOrder)
                .ThenByDescending(f => f.CanvasObject.Id)
                .Select(feature => (Feature: feature, Geometry: CreateSelectionGeometry(feature.Shape)))
                .Where(candidate => !candidate.Geometry.IsEmpty)
                .ToList();

            // Hit test: the topmost rendered feature within pick tolerance wins.
            // Area-selectable data polygons report zero distance when the point is
            // inside them, while editable drawing polygons use their boundary.
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
                .OrderByDescending(candidate => GetSelectionDrawingMarkupRenderPass(candidate.Feature))
                .ThenByDescending(candidate => GetSelectionBuildingFootprintRenderPass(candidate.Feature))
                .ThenByDescending(candidate => GetSelectionRePlotGroupRenderPass(candidate.Feature))
                .ThenByDescending(candidate => GetSelectionCadastralParcelRenderPass(candidate.Feature))
                .ThenByDescending(candidate => GetSelectionProjectBoundaryRenderPass(candidate.Feature))
                .ThenByDescending(candidate => GetSelectionDisplayOrder(candidate.Feature))
                .ThenBy(candidate => candidate.Distance)
                .ThenBy(candidate => GetSelectionGeometryPickArea(candidate.Geometry))
                .ThenByDescending(candidate => candidate.Feature.CanvasObject.Id)
                .Select(candidate => candidate.Feature)
                .FirstOrDefault();
        }

        private int GetSelectionDrawingMarkupRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return layer != null && CanvasLayerTreeService.IsDrawingMarkupLayer(layer) ? 1 : 0;
        }

        private int GetSelectionBuildingFootprintRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return layer != null &&
                   string.Equals(layer.LayerType, "BuildingFootprint", StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;
        }

        private int GetSelectionRePlotGroupRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            if (layer == null)
                return 0;

            if (IsReplottedParcelSelectionLayer(layer))
                return 3;

            if (CanvasLayerTreeService.IsBlockLayoutLayer(layer))
                return 2;

            if (CanvasLayerTreeService.IsRoadsGroupKey(GetSelectionLayerGroupKey(layer)))
                return 1;

            return 0;
        }

        private int GetSelectionCadastralParcelRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return IsImportedCadastralParcelFeature(feature, layer) ? 0 : 1;
        }

        private static bool IsReplottedParcelSelectionLayer(CanvasLayer layer)
        {
            return string.Equals(layer.LayerType, "ReplottedParcel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "PrivateReplotParcel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "PublicFacility", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "OpenSpace", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "ServiceSalesPlot", StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetSelectionLayerGroupKey(CanvasLayer layer)
        {
            if (CanvasLayerTreeService.IsBlockLayoutLayer(layer))
                return CanvasLayerTreeService.BlockLayoutGroupKey;

            if (string.Equals(layer.LayerType, "RoadParcel", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(layer.LayerType, "ProposedRoad", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(layer.LayerType, CanvasLayerTreeService.RoadCenterlineLayerType, StringComparison.OrdinalIgnoreCase))
            {
                return CanvasLayerTreeService.RoadsGroupKey;
            }

            if (IsReplottedParcelSelectionLayer(layer))
                return CanvasLayerTreeService.ReplottedParcelsGroupKey;

            return null;
        }

        private int GetSelectionProjectBoundaryRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return layer != null && CanvasLayerTreeService.IsProjectBoundaryLayer(layer) ? 1 : 0;
        }

        private int GetSelectionDisplayOrder(CanvasFeature feature) =>
            ResolveFeatureLayer(feature)?.DisplayOrder ?? int.MaxValue;

        private NtsGeometry CreateClickSelectionGeometry(CanvasFeature feature, NtsGeometry selectionGeometry)
        {
            if (IsProjectBoundaryFeature(feature) && selectionGeometry.Area > 0)
            {
                return selectionGeometry.Boundary;
            }

            if (UsesAreaClickSelection(feature) || selectionGeometry.Area <= 0)
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
            CancelActiveGripEdit(restoreOriginal: true);
            _hoveredSelectionGrip = null;
            _selectedShapeIds.Clear();
            foreach (CanvasFeature feature in selectedFeatures.Where(IsSelectableDrawingFeature))
            {
                _selectedShapeIds.Add(feature.Shape.Id);
            }

            ApplySelectedShapeFlags();
            // The selection glow is baked into the cached bitmap, so rebuild the
            // cache off the UI thread; every frame stays a fast cache blit.
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
            UpdateStatusBar();
            NotifySelectedCanvasObjectsChanged();
        }

        private bool TryGetCombinedFeatureBounds(
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
                RectangleD featureBounds;
                if (feature.Shape is TextShape textShape &&
                    textShape.LastRenderedBounds.HasValue)
                {
                    featureBounds = ConvertScreenBoundsToWorldBounds(textShape.LastRenderedBounds.Value);
                }
                else if (!TryNormalizeZoomWorldBounds(feature.Shape.GetBoundingBox(), out featureBounds))
                {
                    continue;
                }

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

            if (!hasBounds)
                return false;

            if (right <= left)
            {
                left -= 5.0;
                right += 5.0;
            }

            if (top <= bottom)
            {
                bottom -= 5.0;
                top += 5.0;
            }

            bounds = new RectangleD(left, bottom, right - left, top - bottom);
            return true;
        }

        private static RectangleD ExpandSelectionZoomBounds(RectangleD bounds)
        {
            double width = Math.Max(bounds.Width, MinimumSelectionZoomWorldSpan);
            double height = Math.Max(bounds.Height, MinimumSelectionZoomWorldSpan);
            double marginX = Math.Max(width * SelectionZoomBoundsMarginFactor, 1.0);
            double marginY = Math.Max(height * SelectionZoomBoundsMarginFactor, 1.0);
            double centerX = bounds.Left + bounds.Width / 2.0;
            double centerY = bounds.Top + bounds.Height / 2.0;
            double expandedWidth = width + marginX * 2.0;
            double expandedHeight = height + marginY * 2.0;

            return new RectangleD(
                centerX - expandedWidth / 2.0,
                centerY - expandedHeight / 2.0,
                expandedWidth,
                expandedHeight);
        }

        private static RectangleD ExpandObjectExtentZoomBounds(RectangleD bounds)
        {
            double marginX = Math.Max(bounds.Width * 0.08, 0.5);
            double marginY = Math.Max(bounds.Height * 0.08, 0.5);

            return new RectangleD(
                bounds.Left - marginX,
                bounds.Top - marginY,
                bounds.Width + marginX * 2.0,
                bounds.Height + marginY * 2.0);
        }

        private RectangleD ConvertScreenBoundsToWorldBounds(RectangleF screenBounds)
        {
            PointD worldTopLeft = _engine.ScreenToWorld(new PointD(screenBounds.Left, screenBounds.Top));
            PointD worldBottomRight = _engine.ScreenToWorld(new PointD(screenBounds.Right, screenBounds.Bottom));
            double left = Math.Min(worldTopLeft.X, worldBottomRight.X);
            double right = Math.Max(worldTopLeft.X, worldBottomRight.X);
            double bottom = Math.Min(worldTopLeft.Y, worldBottomRight.Y);
            double top = Math.Max(worldTopLeft.Y, worldBottomRight.Y);
            return new RectangleD(left, bottom, right - left, top - bottom);
        }

        private void AddSelectedObjects(IEnumerable<CanvasFeature> selectedFeatures)
        {
            _hoveredSelectionGrip = null;
            bool changed = false;
            foreach (CanvasFeature feature in selectedFeatures.Where(IsSelectableDrawingFeature))
            {
                changed |= _selectedShapeIds.Add(feature.Shape.Id);
            }

            if (!changed)
            {
                return;
            }

            ApplySelectedShapeFlags();
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
            UpdateStatusBar();
            NotifySelectedCanvasObjectsChanged();
        }

        private void ClearSelectedObjects()
        {
            if (_selectedShapeIds.Count == 0)
                return;

            CancelActiveGripEdit(restoreOriginal: true);
            _hoveredSelectionGrip = null;
            _selectedShapeIds.Clear();
            ApplySelectedShapeFlags();
            RefreshVectorCacheForCurrentViewAsync();
            RequestRender();
            NotifySelectedCanvasObjectsChanged();
        }

        private void ApplySelectedShapeFlags()
        {
            foreach (CanvasFeature feature in _vectorFeatures)
            {
                feature.Shape.IsSelected =
                    _selectedShapeIds.Contains(feature.Shape.Id) &&
                    IsSelectableDrawingFeature(feature);
            }
        }

        private void RequestDeleteSelectedObjects()
        {
            if (_applicationEditLocked)
            {
                NotifyEditLocked();
                return;
            }

            if (_selectedShapeIds.Count == 0)
                return;

            Guid[] editableShapeIds = _vectorFeatures
                .Where(feature => _selectedShapeIds.Contains(feature.Shape.Id) &&
                                  IsDeletableSelectedFeature(feature))
                .Select(feature => feature.Shape.Id)
                .ToArray();
            if (editableShapeIds.Length == 0)
                return;

            SelectedObjectsDeleteRequested?.Invoke(editableShapeIds);
        }

        private void ObjectSelectionContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            bool hasSingleText = SelectionContainsSingleTextShape(out _);
            _mnuEditText.Visible = hasSingleText && !_applicationEditLocked;
            ConfigureAssignDataMenuForSelection();
            _mnuDeleteSelectedObjects.Enabled = !_applicationEditLocked &&
                                                SelectionContainsDeletableObject();
            _mnuMoveSelectedObjects.Enabled = !_applicationEditLocked &&
                                              _activeMoveOperation == null &&
                                              _activeGripEdit == null &&
                                              SelectionContainsMovableObject();
            ConfigureCreateFeaturesFromSelectionMenu();
        }

        private void ConfigureAssignDataMenuForSelection()
        {
            _currentSelectionAssignmentKind = ResolveSelectionAssignmentKind();
            _mnuAssignData.Text = GetAssignDataMenuText(_currentSelectionAssignmentKind);
            _mnuAssignData.Enabled = !_applicationEditLocked &&
                                     _currentSelectionAssignmentKind.HasValue;
        }

        private static string GetAssignDataMenuText(CanvasObjectAssignmentKind? assignmentKind)
        {
            return assignmentKind switch
            {
                CanvasObjectAssignmentKind.Parcel => "Assign Parcel Data",
                CanvasObjectAssignmentKind.Road => "Assign Road Data",
                CanvasObjectAssignmentKind.Block => "Assign Block Data",
                _ => "Assign Data"
            };
        }

        private void ConfigureCreateFeaturesFromSelectionMenu()
        {
            _mnuCreateFeaturesFromSelection.DropDownItems.Clear();

            IReadOnlyList<CanvasFeature> selectedSourceFeatures = GetSelectedFeatureCreationSourceFeatures();
            IReadOnlyList<Guid> selectedSourceObjectIds = selectedSourceFeatures
                .Select(feature => feature.Shape.Id)
                .Distinct()
                .ToArray();
            _mnuCreateFeaturesFromSelection.Visible = selectedSourceObjectIds.Count > 0;
            if (selectedSourceObjectIds.Count == 0)
            {
                _mnuCreateFeaturesFromSelection.Enabled = false;
                return;
            }

            if (_applicationEditLocked)
            {
                _mnuCreateFeaturesFromSelection.DropDownItems.Add(new ToolStripMenuItem("Edit Lock is active")
                {
                    Enabled = false
                });
                _mnuCreateFeaturesFromSelection.Enabled = false;
                return;
            }

            if (SelectionContainsMixedCreateFeatureKinds(selectedSourceFeatures))
            {
                _mnuCreateFeaturesFromSelection.DropDownItems.Add(new ToolStripMenuItem(
                    "Select one drawing object type at a time")
                {
                    Enabled = false
                });
                _mnuCreateFeaturesFromSelection.Enabled = false;
                return;
            }

            SelectedObjectsCreateFeaturesMenuOpening?.Invoke(
                this,
                new CanvasCreateFeaturesMenuOpeningEventArgs(
                    selectedSourceObjectIds,
                    _mnuCreateFeaturesFromSelection));

            _mnuCreateFeaturesFromSelection.Enabled = _mnuCreateFeaturesFromSelection.DropDownItems
                .Cast<ToolStripItem>()
                .Any(item => item.Enabled);
        }

        private IReadOnlyList<CanvasFeature> GetSelectedFeatureCreationSourceFeatures()
        {
            return _vectorFeatures
                .Where(feature =>
                    _selectedShapeIds.Contains(feature.Shape.Id) &&
                    IsFeatureCreationSourceFeature(feature))
                .ToArray();
        }

        private static bool SelectionContainsMixedCreateFeatureKinds(
            IReadOnlyList<CanvasFeature> selectedDrawingFeatures)
        {
            return selectedDrawingFeatures
                .Select(GetCreateFeatureSelectionKind)
                .Where(kind => !string.IsNullOrWhiteSpace(kind))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(2)
                .Count() > 1;
        }

        private static string GetCreateFeatureSelectionKind(CanvasFeature feature)
        {
            if (feature.Shape is TextShape)
                return "Annotation";

            return feature.CanvasObject.Shape.OgcGeometryType switch
            {
                NetTopologySuite.Geometries.OgcGeometryType.Point or
                NetTopologySuite.Geometries.OgcGeometryType.MultiPoint => "Point",
                NetTopologySuite.Geometries.OgcGeometryType.LineString or
                NetTopologySuite.Geometries.OgcGeometryType.MultiLineString or
                NetTopologySuite.Geometries.OgcGeometryType.Polygon or
                NetTopologySuite.Geometries.OgcGeometryType.MultiPolygon => "LinearOrPolygon",
                _ => string.Empty
            };
        }

        private bool SelectionContainsSingleTextShape(out CanvasFeature? textFeature)
        {
            var selected = _vectorFeatures
                .Where(f => _selectedShapeIds.Contains(f.Shape.Id) &&
                            IsSelectableDrawingFeature(f) &&
                            IsEditableDrawingFeature(f))
                .ToList();
            if (selected.Count == 1 && selected[0].Shape is TextShape)
            {
                textFeature = selected[0];
                return true;
            }
            textFeature = null;
            return false;
        }

        private void BeginTextEditFromContextMenu()
        {
            if (_applicationEditLocked)
            {
                NotifyEditLocked();
                return;
            }

            if (!SelectionContainsSingleTextShape(out CanvasFeature? feature) ||
                feature?.Shape is not TextShape textShape)
                return;

            Point screenCenter = canvasSurface.PointToClient(Cursor.Position);
            if (textShape.LastRenderedBounds.HasValue)
            {
                var b = textShape.LastRenderedBounds.Value;
                screenCenter = new Point((int)b.Left, (int)b.Top);
            }
            BeginTextEditExisting(feature, textShape, screenCenter);
        }

        private void RequestAssignDataForSelectedObjects()
        {
            if (_applicationEditLocked)
            {
                NotifyEditLocked();
                return;
            }

            CanvasObjectAssignmentKind? assignmentKind =
                _currentSelectionAssignmentKind ?? ResolveSelectionAssignmentKind();
            if (!assignmentKind.HasValue)
                return;

            SelectedObjectsAssignDataRequested?.Invoke(assignmentKind.Value);
        }

        private CanvasObjectAssignmentKind? ResolveSelectionAssignmentKind()
        {
            CanvasObjectAssignmentKind[] assignmentKinds = _vectorFeatures
                .Where(feature =>
                    _selectedShapeIds.Contains(feature.Shape.Id) &&
                    IsSelectableDrawingFeature(feature))
                .Select(ResolveAssignmentKind)
                .Where(kind => kind.HasValue)
                .Select(kind => kind!.Value)
                .Distinct()
                .Take(2)
                .ToArray();

            return assignmentKinds.Length == 1
                ? assignmentKinds[0]
                : null;
        }

        private CanvasObjectAssignmentKind? ResolveAssignmentKind(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            if (layer == null)
                return null;

            if (IsRoadAssignmentLayer(layer))
                return CanvasObjectAssignmentKind.Road;

            if (IsBlockAssignmentLayer(layer))
                return CanvasObjectAssignmentKind.Block;

            if (IsSelectableImportedCadastralParcel(feature) ||
                IsOriginalParcelAssignmentLayer(layer))
            {
                return CanvasObjectAssignmentKind.Parcel;
            }

            return null;
        }

        private static bool IsRoadAssignmentLayer(CanvasLayer layer)
        {
            return string.Equals(layer.LayerType, CanvasLayerTreeService.RoadCenterlineLayerType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "RoadParcel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "ProposedRoad", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "ExistingRoad", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBlockAssignmentLayer(CanvasLayer layer)
        {
            return CanvasLayerTreeService.IsBlockLayoutLayer(layer);
        }

        private static bool IsOriginalParcelAssignmentLayer(CanvasLayer layer)
        {
            return string.Equals(layer.LayerType, "BaselineParcel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "ProjectBoundary", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "BuildingFootprint", StringComparison.OrdinalIgnoreCase);
        }

        private void BeginMoveSelectedObjectsFromContextMenu()
        {
            if (_applicationEditLocked)
            {
                NotifyEditLocked();
                return;
            }

            if (_activeMoveOperation != null || _activeGripEdit != null)
                return;

            List<MoveItem> items = new();
            foreach (CanvasFeature feature in _vectorFeatures)
            {
                if (!_selectedShapeIds.Contains(feature.Shape.Id))
                    continue;
                if (!IsSelectableDrawingFeature(feature))
                    continue;
                if (IsSelectableImportedCadastralParcel(feature))
                    continue;
                CanvasLayer? layer = ResolveFeatureLayer(feature);
                if (layer == null || layer.IsLocked == true)
                    continue;
                if (!CanvasLayerTreeService.IsDrawingMarkupLayer(layer))
                    continue;

                items.Add(new MoveItem
                {
                    Feature = feature,
                    OriginalShape = feature.Shape.Clone()
                });
            }

            if (items.Count == 0)
                return;

            _activeMoveOperation = new MoveOperation { Items = items };
            canvasSurface.Focus();
            UpdateStatusBar();
            RequestRender();
        }

        private void CaptureMovePreviewBitmap()
        {
            DisposeMovePreviewBitmap();

            if (_activeMoveOperation == null)
                return;

            Size canvasSize = canvasSurface.Size;
            if (canvasSize.Width <= 0 || canvasSize.Height <= 0)
                return;

            Bitmap bmp = new(canvasSize.Width, canvasSize.Height, PixelFormat.Format32bppPArgb);
            try
            {
                using Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.Transparent);
                foreach (MoveItem item in _activeMoveOperation.Items)
                {
                    // Render the unselected original-position clone so the move
                    // preview shows the shape's normal color/line weight (no
                    // selection glow) while it is being dragged.
                    _renderer.RenderTransientShape(
                        g,
                        item.OriginalShape,
                        ResolveFeatureLayer(item.Feature),
                        item.Feature.CanvasObject);
                }
            }
            catch
            {
                bmp.Dispose();
                return;
            }

            _movePreviewBitmap = bmp;
            _movePreviewBitmapScale = _engine.ZoomScale;
            PointD refScreen = _engine.WorldToScreen(_activeMoveOperation.ReferenceWorld);
            _movePreviewBitmapReferenceScreen = new PointF((float)refScreen.X, (float)refScreen.Y);
        }

        private void DisposeMovePreviewBitmap()
        {
            _movePreviewBitmap?.Dispose();
            _movePreviewBitmap = null;
        }

        /// <summary>
        /// Captures the whole canvas exactly as it is currently shown (cached
        /// background plus the selected shape's glow at its original location)
        /// into a frozen bitmap. Used as the static background for a
        /// geometric-center grip move so only this bitmap and the moving
        /// preview are painted while the move is active.
        /// </summary>
        private void CaptureGripMoveFreezeBitmap()
        {
            DisposeGripMoveFreezeBitmap();

            Size canvasSize = canvasSurface.Size;
            if (canvasSize.Width <= 0 || canvasSize.Height <= 0)
                return;

            Bitmap bmp = new(canvasSize.Width, canvasSize.Height, PixelFormat.Format32bppPArgb);
            try
            {
                using Graphics g = Graphics.FromImage(bmp);
                g.Clear(canvasSurface.BackColor);
                // _gripMoveFreezeBitmap is still null and _activeGripEdit is not
                // set yet, so RenderCanvasFrame renders the full normal frame
                // (background + cache + selection glow) instead of recursing
                // into the freeze path.
                RenderCanvasFrame(g, updateDebugTiming: false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Grip move freeze capture failed: {ex.Message}");
                bmp.Dispose();
                return;
            }

            _gripMoveFreezeBitmap = bmp;
        }

        private void DisposeGripMoveFreezeBitmap()
        {
            _gripMoveFreezeBitmap?.Dispose();
            _gripMoveFreezeBitmap = null;
        }

        private void CommitMoveOperation()
        {
            if (_activeMoveOperation == null ||
                _activeMoveOperation.Phase != MoveOperationPhase.AwaitingDestination ||
                !_currentMouseWorld.HasValue)
                return;

            PointD cursor = _currentMouseWorld.Value;
            PointD delta = new(
                cursor.X - _activeMoveOperation.ReferenceWorld.X,
                cursor.Y - _activeMoveOperation.ReferenceWorld.Y);

            List<IShape> editedShapes = new();
            foreach (MoveItem item in _activeMoveOperation.Items)
            {
                // Reset live shape to its pre-move state, then translate by the final delta.
                RestoreShapeGeometry(item.Feature.Shape, item.OriginalShape);
                item.Feature.Shape.Translate(delta);
                editedShapes.Add(item.Feature.Shape);
            }

            _activeMoveOperation = null;
            DisposeMovePreviewBitmap();
            _renderer.UpdateVectorFeatures(_vectorFeatures);
            UpdateWorldBounds();
            // Snapshot the just-rendered committed state into the zoom buffer so paint has
            // something to show while the async cache rebuild runs (no blank-frame flicker).
            EnsureVectorZoomSnapshot();
            _holdVectorZoomFrameUntilRefresh = true;
            RefreshVectorCacheForCurrentViewAsync();
            UpdateStatusBar();
            RequestRender();

            // Persist the whole move as ONE batch (single DB reload + single
            // cache rebuild) instead of one reload per moved shape.
            if (editedShapes.Count > 0)
            {
                ShapesEdited?.Invoke(editedShapes);
                _commandService.LogCommand(editedShapes.Count > 1
                    ? $"Moved {editedShapes.Count} objects"
                    : "Moved object");
            }
        }

        private void CancelMoveOperation()
        {
            if (_activeMoveOperation == null)
                return;

            // Restore live shapes from the pre-move snapshot (no-op if we never entered
            // AwaitingDestination, but safe to run unconditionally).
            foreach (MoveItem item in _activeMoveOperation.Items)
            {
                RestoreShapeGeometry(item.Feature.Shape, item.OriginalShape);
            }

            _activeMoveOperation = null;
            DisposeMovePreviewBitmap();
            _renderer.UpdateVectorFeatures(_vectorFeatures);
            EnsureVectorZoomSnapshot();
            _holdVectorZoomFrameUntilRefresh = true;
            RefreshVectorCacheForCurrentViewAsync();
            UpdateStatusBar();
            RequestRender();
        }

        private void DrawMoveOperationOverlay(Graphics graphics)
        {
            // Keep rendering the moved shapes during pan too: the engine updates
            // live while panning and the cached background shifts by the same
            // delta, so the offset formula below stays correct (same approach as
            // the grip/geometric-center move). Previously this returned early on
            // pan, which made the moved shapes vanish while panning.
            if (_activeMoveOperation == null ||
                _activeMoveOperation.Phase != MoveOperationPhase.AwaitingDestination ||
                !_currentMouseWorld.HasValue)
                return;

            // Re-capture bitmap when zoom scale changes (pan-only changes are handled by offset math).
            if (_movePreviewBitmap == null ||
                Math.Abs(_engine.ZoomScale - _movePreviewBitmapScale) > _movePreviewBitmapScale * 1e-9)
            {
                CaptureMovePreviewBitmap();
            }

            if (_movePreviewBitmap == null)
                return;

            // offset = screen(cursor) − screen(reference at capture)
            // This formula is correct for any pan offset; only breaks for zoom (handled above).
            PointD cursorScreen = _engine.WorldToScreen(_currentMouseWorld.Value);
            float dx = (float)cursorScreen.X - _movePreviewBitmapReferenceScreen.X;
            float dy = (float)cursorScreen.Y - _movePreviewBitmapReferenceScreen.Y;

            // DrawImageUnscaled is a fast 1:1 pixel blit. The graphics context is
            // configured for high-quality bicubic interpolation here, which would
            // otherwise resample the full-screen bitmap on every mouse move.
            graphics.DrawImageUnscaled(
                _movePreviewBitmap,
                (int)Math.Round(dx),
                (int)Math.Round(dy));
        }

        private bool HandleMoveOperationClick(Point screenPoint)
        {
            if (_activeMoveOperation == null)
                return false;

            if (_applicationEditLocked)
            {
                CancelMoveOperation();
                NotifyEditLocked();
                return true;
            }

            // Use the snapped cursor world point when available; otherwise the raw cursor.
            PointD picked = _currentSnapPoint?.Position ?? _engine.ScreenToWorld(screenPoint);

            if (_activeMoveOperation.Phase == MoveOperationPhase.AwaitingReference)
            {
                _activeMoveOperation.ReferenceWorld = picked;
                _activeMoveOperation.Phase = MoveOperationPhase.AwaitingDestination;
                _currentMouseWorld = picked;

                // Capture shapes at their original positions into a bitmap.
                // The originals remain visible in the vector cache (selection highlight + grips).
                CaptureMovePreviewBitmap();
                UpdateStatusBar();
                RequestRender();
                return true;
            }

            // Destination click — commit the translation.
            _currentMouseWorld = picked;
            CommitMoveOperation();
            return true;
        }

        private bool SelectionContainsEditableObject()
        {
            return _vectorFeatures.Any(feature =>
                _selectedShapeIds.Contains(feature.Shape.Id) &&
                IsEditableDrawingFeature(feature));
        }

        private bool SelectionContainsDeletableObject()
        {
            return _vectorFeatures.Any(feature =>
                _selectedShapeIds.Contains(feature.Shape.Id) &&
                IsDeletableSelectedFeature(feature));
        }

        private bool SelectionContainsMovableObject()
        {
            return _vectorFeatures.Any(feature =>
                _selectedShapeIds.Contains(feature.Shape.Id) &&
                IsSelectableDrawingFeature(feature) &&
                !IsSelectableImportedCadastralParcel(feature) &&
                ResolveFeatureLayer(feature) is CanvasLayer ml &&
                ml.IsLocked != true &&
                CanvasLayerTreeService.IsDrawingMarkupLayer(ml));
        }

        public void ClearSelectionAfterDelete()
        {
            CancelActiveGripEdit(restoreOriginal: false);
            _hoveredSelectionGrip = null;
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
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return feature.Shape.IsVisible &&
                   feature.CanvasObject.IsVisible &&
                   layer?.IsVisible == true &&
                   layer.IsSelectable &&
                   ((layer.IsLocked != true &&
                     CanvasLayerTreeService.IsDrawingMarkupLayer(layer)) ||
                    IsSelectableImportedCadastralParcel(feature) ||
                    IsSelectableRePlotDataFeature(feature) ||
                    IsSelectableExternalReferenceFeature(feature));
        }

        private bool IsEditableDrawingFeature(CanvasFeature feature)
        {
            if (_applicationEditLocked)
                return false;

            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return feature.Shape.IsVisible &&
                   feature.CanvasObject.IsVisible &&
                   layer?.IsVisible == true &&
                   layer.IsSelectable &&
                   layer.IsLocked != true &&
                   (CanvasLayerTreeService.IsDrawingMarkupLayer(layer) ||
                    IsGeneratedRoadParcelDependencyLayer(layer));
        }

        private static bool IsGeneratedRoadParcelDependencyLayer(CanvasLayer layer)
        {
            return CanvasLayerTreeService.IsProjectBoundaryLayer(layer) ||
                   string.Equals(layer.LayerType, "Block", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.Name, "Blocks", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsDeletableSelectedFeature(CanvasFeature feature)
        {
            if (_applicationEditLocked)
                return false;

            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return feature.Shape.IsVisible &&
                   feature.CanvasObject.IsVisible &&
                   layer?.IsVisible == true &&
                   layer.IsSelectable &&
                   layer.IsLocked != true &&
                   (CanvasLayerTreeService.IsDrawingMarkupLayer(layer) ||
                    IsSelectableImportedCadastralParcel(feature) ||
                    IsSelectableRePlotDataFeature(feature));
        }

        private bool UsesAreaClickSelection(CanvasFeature feature)
        {
            return IsSelectableImportedCadastralParcel(feature) ||
                   IsSelectableRePlotDataFeature(feature);
        }

        private bool IsProjectBoundaryFeature(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return layer != null && CanvasLayerTreeService.IsProjectBoundaryLayer(layer);
        }

        private bool IsSelectableRePlotDataFeature(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return layer != null &&
                   CanvasLayerTreeService.IsRePlotDataLayer(layer) &&
                   !CanvasLayerTreeService.IsDrawingMarkupLayer(layer) &&
                   !string.Equals(layer.LayerType, CanvasLayerTreeService.RasterLayerType, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSelectableExternalReferenceFeature(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveFeatureLayer(feature);
            return layer != null &&
                   CanvasLayerTreeService.IsExternalImportedLayer(layer) &&
                   !string.Equals(layer.LayerType, CanvasLayerTreeService.RasterLayerType, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsFeatureCreationSourceFeature(CanvasFeature feature)
        {
            if (_applicationEditLocked)
                return false;

            CanvasLayer? layer = ResolveFeatureLayer(feature);
            if (layer == null || layer.IsVisible != true || !layer.IsSelectable)
                return false;

            return (layer.IsLocked != true && CanvasLayerTreeService.IsDrawingMarkupLayer(layer)) ||
                   IsSelectableExternalReferenceFeature(feature);
        }

        private void PruneSelectionToSelectableFeatures()
        {
            if (_selectedShapeIds.Count == 0)
            {
                ApplySelectedShapeFlags();
                return;
            }

            HashSet<Guid> selectableShapeIds = _vectorFeatures
                .Where(IsSelectableDrawingFeature)
                .Select(feature => feature.Shape.Id)
                .ToHashSet();

            int removedCount = _selectedShapeIds.RemoveWhere(id => !selectableShapeIds.Contains(id));
            if (removedCount > 0)
            {
                _hoveredSelectionGrip = null;
                if (_activeGripEdit != null &&
                    !_selectedShapeIds.Contains(_activeGripEdit.Grip.Shape.Id))
                {
                    CancelActiveGripEdit(restoreOriginal: true);
                }
            }

            ApplySelectedShapeFlags();

            if (removedCount > 0)
            {
                UpdateStatusBar();
                NotifySelectedCanvasObjectsChanged();
            }
        }

        private static bool IsSelectableImportedCadastralParcel(CanvasFeature feature)
        {
            return string.Equals(feature.CanvasObject.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(feature.CanvasObject.GeometryMetadataJson) &&
                   feature.CanvasObject.GeometryMetadataJson.Contains("CadastralParcel", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsImportedCadastralParcelFeature(CanvasFeature feature, CanvasLayer? layer)
        {
            return string.Equals(feature.CanvasObject.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase) &&
                   ((layer?.Description?.StartsWith(
                         "Imported cadastral map layer",
                         StringComparison.OrdinalIgnoreCase) == true) ||
                    (!string.IsNullOrWhiteSpace(feature.CanvasObject.GeometryMetadataJson) &&
                     feature.CanvasObject.GeometryMetadataJson.Contains("CadastralParcel", StringComparison.OrdinalIgnoreCase)));
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
            if (!_isSelectingObjects || _activeTextEditor != null)
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
            using Pen borderPen = new(borderColor, 1.0f);
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
            RefreshActiveTextEditorMetrics();
        }

        private void UpdateCanvasCursor()
        {
            // Active panning always shows the pan cursor, even mid grip-edit or
            // mid move operation (so panning during a move shows the hand).
            if (_isPanning || _panToolActive)
            {
                canvasSurface.Cursor = GetPanCursor();
            }
            else if (_activeGripEdit != null || _hoveredSelectionGrip != null)
            {
                canvasSurface.Cursor = Cursors.SizeAll;
            }
            else if (_isZooming)
            {
                canvasSurface.Cursor = string.Equals(_zoomDirection, "Out", StringComparison.OrdinalIgnoreCase)
                    ? GetZoomOutCursor()
                    : GetZoomInCursor();
            }
            else if (_zoomWindowActive)
            {
                canvasSurface.Cursor = GetZoomWindowCursor();
            }
            else if (_activeTool != MapCanvasTool.Select)
            {
                canvasSurface.Cursor = Cursors.Cross;
            }
            else
            {
                canvasSurface.Cursor = GetSelectionCursor();
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

        private Cursor GetSelectionCursor()
        {
            if (_selectionCursor != null)
                return _selectionCursor;

            string[] candidatePaths =
            [
                Path.Combine(AppContext.BaseDirectory, "Resources", "Cursors", "selection.cur"),
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "Resources",
                    "Cursors",
                    "selection.cur"))
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
                        _selectionCursor = new Cursor(cursorHandle);
                        return _selectionCursor;
                    }
                }
                catch
                {
                    break;
                }
            }

            return Cursors.Default;
        }

        private Cursor GetZoomInCursor()
        {
            return GetCustomCursor(ref _zoomInCursor, "zoomin.cur", Cursors.Cross);
        }

        private Cursor GetZoomOutCursor()
        {
            return GetCustomCursor(ref _zoomOutCursor, "zoomout.cur", Cursors.Cross);
        }

        private Cursor GetZoomWindowCursor()
        {
            return GetCustomCursor(ref _zoomWindowCursor, "zoomwindow.cur", Cursors.Cross);
        }

        private Cursor GetCustomCursor(
            ref Cursor? cachedCursor,
            string fileName,
            Cursor fallbackCursor)
        {
            if (cachedCursor != null)
                return cachedCursor;

            string[] candidatePaths =
            [
                Path.Combine(AppContext.BaseDirectory, "Resources", "Cursors", fileName),
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "Resources",
                    "Cursors",
                    fileName))
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
                        cachedCursor = new Cursor(cursorHandle);
                        return cachedCursor;
                    }
                }
                catch
                {
                    break;
                }
            }

            return fallbackCursor;
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
            if (_applicationEditLocked &&
                _activeTool == MapCanvasTool.Select &&
                !_isPanning &&
                !_isSelectingZoomWindow &&
                !_zoomWindowActive)
            {
                return "Mode: Edit Locked";
            }

            if (_isZooming && _zoomDirection != null)
                return $"Mode: Zooming {_zoomDirection}";

            if (_activeTool != MapCanvasTool.Select)
            {
                if (_activeTool == MapCanvasTool.Text && _activeTextEditor != null)
                    return "Mode: Draw Text";

                return _drawingVertices.Count == 0
                    ? $"Mode: Draw {_activeTool}"
                    : $"Mode: Draw {_activeTool} ({_drawingVertices.Count})";
            }

            if (_isSelectingZoomWindow || _zoomWindowActive)
                return "Mode: Zoom Window";

            if (_isPanning || _panToolActive)
                return "Mode: Pan";

            if (_activeGripEdit != null)
                return "Mode: Grip Edit";

            if (_activeMoveOperation != null)
                return "Mode: Move";

            return "Mode: Ready";
        }

        private string GetCommandPromptText()
        {
            if (_applicationEditLocked)
                return "Edit Lock active: selection and navigation only";

            if (_activeMoveOperation != null)
            {
                return _activeMoveOperation.Phase == MoveOperationPhase.AwaitingReference
                    ? "Move object(s): click the reference (base) point  [Esc/right-click to cancel]"
                    : "Move object(s): click the destination point  [Esc/right-click to cancel]";
            }

            if (_activeGripEdit != null)
            {
                return _activeGripEdit.AwaitingClickCommit
                    ? "Grip edit: click final position  [Esc/right-click to cancel]"
                    : "Grip edit: drag to modify  [Esc to cancel]";
            }

            if (_hoveredSelectionGrip != null)
            {
                return "Grip: click to edit selected geometry";
            }

            if (_activeTool == MapCanvasTool.Text)
            {
                return _activeTextEditor != null
                    ? "Text: type single-line text  [Enter to place, Esc to cancel]"
                    : "Text: Click insertion point";
            }

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

        private void NotifyEditLocked()
        {
            _commandService.SetPrompt("Edit Lock active: unlock the project to modify records, layers, or canvas objects");
            StatusChanged?.Invoke(
                _currentMouseWorld.HasValue
                    ? $"E: {_currentMouseWorld.Value.X:F4}    N: {_currentMouseWorld.Value.Y:F4}"
                    : "E: --    N: --",
                "Mode: Edit Locked",
                _engine.ZoomScale);
        }

        /// <summary>
        /// Returns true when the supplied drawing layer cannot be drawn on
        /// because it is locked and/or hidden.
        /// </summary>
        private static bool IsDrawingLayerUnavailable(CanvasLayer? layer) =>
            layer != null && (layer.IsLocked || !layer.IsVisible);

        /// <summary>
        /// Tells the user that the target drawing layer is locked and/or hidden
        /// so drawing is not allowed, and updates the canvas command prompt.
        /// </summary>
        private void NotifyDrawingLayerUnavailable(CanvasLayer layer)
        {
            string reason =
                layer.IsLocked && !layer.IsVisible ? "locked and hidden" :
                layer.IsLocked ? "locked" :
                "hidden";
            string message =
                $"The layer “{layer.Name}” is {reason}. " +
                "Unlock and show the layer before drawing on it.";

            _commandService.SetPrompt(message);
            MessageBox.Show(
                message,
                "Drawing Not Allowed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private bool IsCanvasInteractionLocked => _activeTextEditor != null;
        private bool IsPanBlockedByZoomDebounce =>
            _blockPanUntilZoomSettle ||
            _isZooming ||
            (!_zoomingStatusTimerDisposed && _zoomingStatusTimer.Enabled);

        private bool IsInteractiveNavigation =>
            _isPanning || _isZooming || _isSelectingZoomWindow;

        private bool ShouldDeferDirectRasterRendering =>
            IsInteractiveNavigation ||
            (_compositePanBitmap != null && _holdVectorPanFrameUntilRefresh) ||
            (_rasterCacheRefreshPending &&
             !_suppressStaleRasterFrameUntilFreshRender);

        private bool ShouldDeferDirectVectorRendering =>
            _isPanning ||
            _isZooming ||
            _activeGripEdit != null ||
            _vectorCacheRefreshPending ||
            _holdVectorPanFrameUntilRefresh ||
            _holdVectorZoomFrameUntilRefresh;

        private RasterRenderFrame? GetRasterRenderFrame(out CanvasFrameSource source)
        {
            source = CanvasFrameSource.None;

            // Composite bitmap contains both raster and vector baked together at pan start.
            // Use it during the active pan and in the post-pan hold period so both layers
            // always shift by exactly the same pixel delta (fixes Bug 1 and Bug 2).
            if (_compositePanBitmap != null &&
                (_isPanning || _holdVectorPanFrameUntilRefresh))
            {
                PointF delta = _isPanning ? _totalPanDelta : _heldVectorPanDelta;
                source = CanvasFrameSource.PanCache;
                return new RasterRenderFrame(
                    _compositePanBitmap,
                    new RectangleF(delta.X, delta.Y, _compositePanBitmap.Width, _compositePanBitmap.Height),
                    _compositePanBitmap,
                    () => { });
            }

            // Fallback: separate raster pan frame when the composite was not available.
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

        private RasterRenderFrame? GetFixedReferenceRenderFrame()
        {
            if (_gridPanBitmap == null ||
                (!_isPanning && !_holdVectorPanFrameUntilRefresh))
            {
                return null;
            }

            PointF delta = _isPanning ? _totalPanDelta : _heldVectorPanDelta;
            return new RasterRenderFrame(
                _gridPanBitmap,
                new RectangleF(
                    delta.X - _gridPanPadding.Left,
                    delta.Y - _gridPanPadding.Top,
                    _gridPanBitmap.Width,
                    _gridPanBitmap.Height),
                _gridPanBitmap,
                () => { });
        }

        private RasterRenderFrame? GetVectorRenderFrame(out CanvasFrameSource source)
        {
            source = CanvasFrameSource.None;

            // Composite bitmap already has both raster and vector merged.
            // Signal PanCache so the paint loop skips the direct-render path,
            // but return null so RenderVectorContent draws nothing separately.
            if (_compositePanBitmap != null &&
                (_isPanning || _holdVectorPanFrameUntilRefresh))
            {
                source = CanvasFrameSource.PanCache;
                return null;
            }

            // Fallback: separate vector pan frame when composite is not available.
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
            double scaleDenominator = GetCurrentScaleDenominator();
            bool snapSuspendedForZoom = IsSnapTemporarilySuspendedForZoom();

            List<string> lines =
            [
                $"MAP DEBUG  frame #{_debugFrameNumber}  {_lastDebugFrameElapsedMs:0.0} ms  avg {_averageDebugFrameElapsedMs:0.0} ms  fps {GetFramesPerSecond():0}",
                $"View  zoom {_engine.ZoomScale:0.###}  scale 1:{scaleDenominator:0}  size {canvasSurface.Width}x{canvasSurface.Height}",
                $"World X {viewport.Left:0.###}..{viewport.Right:0.###}  Y {viewport.Top:0.###}..{viewport.Bottom:0.###}  W/H {viewport.Width:0.###}/{viewport.Height:0.###}",
                $"Raster layers {rendererState.VisibleRasterLayerCount}/{rendererState.RasterLayerCount}  draw {rasterFrameSource}  cache {(rasterState.CacheValid ? "valid" : "cold")}  pan {(rasterState.PanBufferValid ? "ready" : "no")}  zoom {(rasterState.ZoomFrameAvailable ? "held" : "no")}  refresh {rasterState.LastRefreshElapsedMs:0.0} ms  pending {_rasterCacheRefreshPending}",
                $"Vector layers {rendererState.VectorLayerCount}  features {vectorStats.TotalFeatureCount}  STRtree {vectorStats.SpatialIndexEntryCount}  draw {vectorFrameSource}  cache {(rendererState.VectorCache.CacheValid ? "valid" : "cold")}  pan {(rendererState.VectorCache.PanBufferValid ? "ready" : "no")}  refresh {rendererState.VectorCache.LastRefreshElapsedMs:0.0} ms",
                $"Vector query {vectorStats.QueryCandidateCount} in {vectorStats.QueryElapsedMs:0.00} ms  rendered {vectorStats.RenderedFeatureCount}  hidden {vectorStats.HiddenSkippedCount}  LOD {(vectorStats.LevelOfDetailEnabled ? "on" : "off")} skipped {vectorStats.LodSkippedCount} min {vectorStats.MinimumVisibleWorldSize:0.###}",
                $"Snap {(_snapEnabled ? (snapSuspendedForZoom ? "suspended" : "on") : "off")}  max scale 1:{MaxSnapScaleDenominator:0}  Ortho {(_orthoModeEnabled ? "on" : "off")}  glyph {_snapGlyphSizePixels:0.#} px  tolerance {_snapPickTolerancePixels:0.#} px  query features {_lastSnapQueryFeatureCount}  candidates {_lastSnapCandidateCount}  {_lastSnapQueryElapsedMs:0.00} ms  current {_currentSnapPoint?.Type.ToString() ?? "none"}",
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
            CancelActiveCanvasGesture(preserveActiveEdit: true);
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
            UpdateCanvasCursor();
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

        /// <summary>
        /// Renders the current visible map state (raster + vector) into a single
        /// composite bitmap that is shifted as one unit during panning. This
        /// eliminates the independent-shift artefact that arises when the two
        /// deferred caches were captured from different viewport positions.
        /// Must be called while _isPanning is still false.
        /// </summary>
        private void BuildCompositePanBuffer(Size canvasSize)
        {
            DisposeCompositePanBitmap();

            if (canvasSize.Width <= 0 || canvasSize.Height <= 0)
                return;

            Bitmap composite = new(canvasSize.Width, canvasSize.Height, PixelFormat.Format32bppPArgb);
            try
            {
                using Graphics g = Graphics.FromImage(composite);

                // GetRasterRenderFrame / GetVectorRenderFrame behave as they would
                // in the last paint call because _isPanning is false here.
                RasterRenderFrame? rasterFrame = GetRasterRenderFrame(out _);
                RasterRenderFrame? vectorFrame = GetVectorRenderFrame(out _);

                bool deferRaster = rasterFrame == null && ShouldDeferDirectRasterRendering;
                bool deferVector = vectorFrame == null && ShouldDeferDirectVectorRendering;

                _renderer.Render(
                    g,
                    rasterFrame,
                    deferRaster,
                    vectorFrame,
                    deferVector,
                    zoomWindowRectangle: null,
                    previewShape: null,
                    previewLayer: null,
                    showDebugOverlay: false,
                    suppressDecorations: true,
                    suppressGridLabels: true,
                    suppressFixedReferenceLayers: true,
                    suppressBackgroundClear: true);

                rasterFrame?.Dispose();
                vectorFrame?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MapCanvas] Composite pan buffer build failed: {ex.Message}");
                composite.Dispose();
                return;
            }

            _compositePanBitmap = composite;
        }

        private void BuildGridPanBuffer(Size canvasSize)
        {
            DisposeGridPanBitmap();

            GridPanPadding padding = _renderer.GetGridPanPadding(canvasSize);
            if (padding.IsEmpty || canvasSize.Width <= 0 || canvasSize.Height <= 0)
            {
                return;
            }

            Size paddedSize = new(
                checked(canvasSize.Width + padding.Horizontal),
                checked(canvasSize.Height + padding.Vertical));
            Bitmap gridBitmap = new(paddedSize.Width, paddedSize.Height, PixelFormat.Format32bppPArgb);

            try
            {
                MapCanvasEngine gridEngine = _engine.CreateSnapshot();
                gridEngine.UpdateCanvasSize(paddedSize);
                gridEngine.SetViewOriginFromPanStart(
                    _engine.ViewOriginWorld,
                    padding.Left,
                    -padding.Bottom);

                using MapCanvasRenderer gridRenderer = new(gridEngine, _renderSettings);
                using Graphics g = Graphics.FromImage(gridBitmap);
                g.Clear(Color.Transparent);
                gridRenderer.RenderFixedReferences(g, suppressGridLabels: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MapCanvas] Grid pan buffer build failed: {ex.Message}");
                gridBitmap.Dispose();
                return;
            }

            _gridPanBitmap = gridBitmap;
            _gridPanPadding = padding;
        }

        private void DisposeCompositePanBitmap()
        {
            _compositePanBitmap?.Dispose();
            _compositePanBitmap = null;
            DisposeGridPanBitmap();
        }

        private void DisposeGridPanBitmap()
        {
            _gridPanBitmap?.Dispose();
            _gridPanBitmap = null;
            _gridPanPadding = GridPanPadding.Empty;
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

        private void CancelActiveCanvasGesture(bool preserveActiveEdit = false)
        {
            // Grip edit and the two-click move are both in-progress editing
            // gestures that must survive pan/zoom navigation (the navigation
            // pipelines pass preserveActiveEdit: true). Only a hard reset
            // (e.g. CRS rebuild, edit lock) cancels them.
            if (!preserveActiveEdit)
            {
                CancelActiveGripEdit(restoreOriginal: true);
                CancelMoveOperation();
            }

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
                DisposeCompositePanBitmap();
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

        private void PrepareProgrammaticZoom()
        {
            if (_isPanning)
            {
                _isPanning = false;
                _panStartWorld = null;
                _totalPanDelta = PointF.Empty;
                _heldVectorPanDelta = PointF.Empty;
                _holdVectorPanFrameUntilRefresh = false;
                DisposeCompositePanBitmap();
                canvasSurface.Capture = false;
                _rasterDeferredRenderer.Invalidate();
                _renderer.InvalidateVectorCache();
            }

            if (_isSelectingZoomWindow)
            {
                _isSelectingZoomWindow = false;
                _zoomWindowActive = false;
            }

            if (_isZooming)
            {
                StopZoomInteraction();
            }
            else
            {
                _zoomingStatusTimer.Stop();
                _blockPanUntilZoomSettle = false;
                _zoomDirection = null;
                _rasterDeferredRenderer.EndZoom();
                _renderer.EndVectorZoom();
                _holdZoomStartFrameUntilRasterRefresh = false;
                _holdVectorZoomFrameUntilRefresh = false;
            }

            _suppressStaleRasterFrameUntilFreshRender = true;
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
                _immediateEditedOverlayFeatures = Array.Empty<CanvasFeature>();
                // Note: the selection decoration overlay is intentionally NOT
                // cleared here. The scene cache is rendered selection-free, so
                // the overlay remains the single live source of selection.
                DisposeCompositePanBitmap();

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
                    RequestRefreshHoldReleaseRenderIfReady();
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
                    RequestRefreshHoldReleaseRenderIfReady();

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

        private static bool TryNormalizeZoomWorldBounds(
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
                !IsFinite(top))
            {
                return false;
            }

            if (right <= left)
            {
                left -= 5.0;
                right += 5.0;
            }

            if (top <= bottom)
            {
                bottom -= 5.0;
                top += 5.0;
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
