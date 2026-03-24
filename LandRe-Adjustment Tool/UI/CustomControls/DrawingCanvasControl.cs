
using netDxf.Entities;
using netDxf;
using System.ComponentModel;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Core.Commands;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.UI.CustomControls
{
    /// <summary>
    /// Drawing Canvas UserControl
    ///
    /// Converted from frmDrawingCanvasRefactored (Form) → DrawingCanvasControl (UserControl).
    /// Only two things changed from the original:
    ///   1. Class declaration: Form → UserControl
    ///   2. Namespaces updated to match Land_Readjustment_Tool project
    ///
    /// Everything else — all logic, regions, handlers — is identical.
    ///
    /// RESPONSIBILITIES:
    /// - Handle UI events (mouse, keyboard, toolbar)
    /// - Coordinate between engine, renderer, and shape manager
    /// - Update UI state (cursor, toolbar selections)
    ///
    /// NOT RESPONSIBLE FOR (delegated to other classes):
    /// - Coordinate transformations → DrawingEngine
    /// - Shape storage/queries → ShapeManager
    /// - Rendering logic → CanvasRenderer
    /// - Undo/redo logic → UndoRedoManager
    /// </summary>
    public partial class DrawingCanvasControl : UserControl
    {
        #region Core Components (Dependency Injection Pattern)

        private DrawingEngine _engine;
        private ShapeManager _shapeManager;
        private CanvasRenderer _renderer;
        private UndoRedoManager _undoManager;
        private OptimizedDeferredRenderer _deferredRenderer;

        #endregion

        #region Drawing State

        private enum DrawingTool
        {
            Line,
            Rectangle,
            Ellipse,
            Circle,
            Polyline
        }

        private DrawingTool _currentTool = DrawingTool.Line;
        private PointD? _drawStartPoint = null;
        private PointD? _drawCurrentPoint = null;
        private PointD? _currentMouseWorldPos = null;
        private PointD? _lastInterpolatedMouseWorldPos = null;
        private PointD? _lastInterpolatedDrawCurrentPoint = null;

        #endregion

        #region Pan State

        private bool _isPanning = false;
        private bool _panToolActive = false;
        private PointD? _panStart = null;
        private PointD _totalPanDelta = new PointD(0, 0);

        #endregion

        #region Snap Configuration

        private int _maxShapesForSnapping = 200;
        private int _maxSnapCandidates = 1000;
        private bool _snapTemporarilyDisabled = false;

        private int _lastVisibleShapesCount = 0;
        private int _lastSnapCandidatesCount = 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(200)]
        public int MaxShapesForSnapping
        {
            get => _maxShapesForSnapping;
            set => _maxShapesForSnapping = Math.Max(10, Math.Min(value, 10000));
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(1000)]
        public int MaxSnapCandidates
        {
            get => _maxSnapCandidates;
            set => _maxSnapCandidates = Math.Max(50, Math.Min(value, 50000));
        }

        #endregion

        private bool _showGrid = true;

        private List<PointD> _polylineVertices = new List<PointD>();
        private List<SnapPoint?> _polylineSnapPoints = new List<SnapPoint?>();
        private List<SnapPoint> _polylineConfirmedSnapPoints = new List<SnapPoint>();
        private bool _polylineDrawing = false;
        private bool _polylineClosed = false;
        private bool _isDebugMode = false;
        private SnapManager _snapManager = new SnapManager();
        private SnapPoint? _currentSnapPoint = null;

        public event EventHandler CollapseLeftPanelClicked;

        public DrawingCanvasControl()
        {
            InitializeComponent();
            // Ensure double buffering for flicker-free drawing
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            InitializeDrawingSystem();
            SetupUIControls();
            this.Load += DrawingCanvasControl_Load;
        }

        private void InitializeDrawingSystem()
        {
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            this.UpdateStyles();
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            _engine = new DrawingEngine(panelCanvas.Size);

            RectangleD worldBounds = new RectangleD(0, 0, 10000000, 10000000);
            _shapeManager = new ShapeManager(worldBounds);

            _renderer = new CanvasRenderer();
            _undoManager = new UndoRedoManager(maxUndoLevels: 100);

            _engine.SetView(500f, 500f, 1000f, 1000f);

            panelCanvas.MouseWheel += PanelCanvas_MouseWheel!;
            panelCanvas.Resize += PanelCanvas_Resize!;

            _deferredRenderer = new OptimizedDeferredRenderer(panelCanvas.Size, _engine, _shapeManager);
        }

        private void PanelCanvas_Resize(object sender, EventArgs e)
        {
            if (_engine != null)
                _engine.UpdateCanvasSize(panelCanvas.Size);

            if (_deferredRenderer != null)
                _deferredRenderer.Resize(panelCanvas.Size);

            panelCanvas.Invalidate();
        }

        private void SetupUIControls()
        {
            cbTheme.Items.Clear();
            cbTheme.Items.AddRange(new string[] { "Dark", "Light" });
            cbTheme.SelectedIndex = 0;

            panelCanvas.BackColor = _renderer.BackgroundColor;

            btnShowHideGrid.Checked = true;
            btnShowHideGrid.Text = "Hide Grid";
            panelCanvas.Invalidate();
            btnShowHideGrid.Click += btnShowHideGrid_Click!;
        }

        private void DrawingCanvasControl_Load(object sender, EventArgs e)
        {
            btnCollapseLeftPanel.Click += BtnCollapseLeftPanel_Click;
        }

        private void BtnCollapseLeftPanel_Click(object sender, EventArgs e)
        {
            CollapseLeftPanelClicked?.Invoke(this, EventArgs.Empty);
        }

        #region Paint Event

        private void panelCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Layer 1: Background
            g.Clear(_renderer.BackgroundColor);

            // Layer 2: Grid + axis markers
            if (_showGrid)
                _renderer.RenderGrid(g, _engine);

            _renderer.DrawAxisMarkers(g, _engine);

            // Layer 3: Shapes
            if (_isPanning)
                _deferredRenderer.DrawShapesDuringPan(g, _totalPanDelta.X, _totalPanDelta.Y);
            else
                _deferredRenderer.DrawShapesCache(g);

            // Layer 4: Preview shape (while drawing)
            IShape previewShape = CreatePreviewShape();
            if (previewShape != null)
                previewShape.Draw(g, _engine.WorldToScreen, isPreview: true);

            // Layer 5: Polyline in-progress preview
            if (_currentTool == DrawingTool.Polyline && _polylineDrawing && _polylineVertices.Count > 0)
                DrawPolylinePreview(g);

            // Layer 6: Snap glyph
            if (_currentSnapPoint != null)
                _renderer.RenderSnapGlyph(g, _engine, _currentSnapPoint);

            // Layer 7: UI overlay
            if (_currentMouseWorldPos.HasValue)
            {
                bool showSnapInfo = toolSnap.Checked && !_snapTemporarilyDisabled;
                int displayShapes = showSnapInfo ? _lastVisibleShapesCount : 0;
                int displayCandidates = showSnapInfo ? _lastSnapCandidatesCount : 0;
                _renderer.RenderUIOverlay(g, _engine, _currentMouseWorldPos,
                    showSnapInfo, displayShapes, displayCandidates);
            }

            // Layer 8: Snap disabled warning
            if (toolSnap.Checked && _snapTemporarilyDisabled)
            {
                _renderer.RenderSnapDisabledWarning(g, panelCanvas.Size,
                    _lastVisibleShapesCount, _maxShapesForSnapping,
                    _lastSnapCandidatesCount, _maxSnapCandidates);
            }

            // Debug overlay
            if (_isDebugMode)
                DrawDebugOverlay(g);
        }

        private void DrawPolylinePreview(Graphics g)
        {
            if (_polylineVertices.Count > 1)
            {
                var poly = new PolylineShape(_polylineVertices, _polylineClosed);
                poly.Draw(g, _engine.WorldToScreen, isPreview: false);
            }

            if (!_polylineClosed && _currentMouseWorldPos.HasValue && _polylineVertices.Count > 0)
            {
                var last = _engine.WorldToScreen(_polylineVertices[_polylineVertices.Count - 1]);
                var current = _engine.WorldToScreen(_currentMouseWorldPos.Value);
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.LightGray, 1f))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLine(pen, (float)last.X, (float)last.Y, (float)current.X, (float)current.Y);
                }
            }
        }

        #endregion

        #region DXF Import

        private void btnImportDxf_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DXF Files (*.dxf)|*.dxf";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var dxf = DxfDocument.Load(openFileDialog.FileName);
                    var shapes = new List<IShape>();
                    RectangleD? importBounds = null;

                    foreach (var entity in dxf.Entities.All)
                    {
                        if (entity.Type == EntityType.Line)
                        {
                            var line = (Line)entity;
                            var s = new LineShape(
                                new PointD(line.StartPoint.X, line.StartPoint.Y),
                                new PointD(line.EndPoint.X, line.EndPoint.Y));
                            shapes.Add(s);
                            importBounds = ExpandBounds(importBounds, s.GetBoundingBox());
                        }
                        if (entity.Type == EntityType.Polyline2D)
                        {
                            var polyline = (Polyline2D)entity;
                            for (int i = 0; i < polyline.Vertexes.Count - 1; i++)
                            {
                                var start = polyline.Vertexes[i].Position;
                                var end = polyline.Vertexes[i + 1].Position;
                                var s = new LineShape(
                                    new PointD(start.X, start.Y),
                                    new PointD(end.X, end.Y));
                                shapes.Add(s);
                                importBounds = ExpandBounds(importBounds, s.GetBoundingBox());
                            }
                        }
                        if (entity.Type == EntityType.Circle)
                        {
                            var circle = (Circle)entity;
                            var center = circle.Center;
                            var radius = circle.Radius;
                            var s = new CircleShape(new PointD(center.X, center.Y), new PointD(center.X + radius, center.Y));
                            shapes.Add(s);
                            importBounds = ExpandBounds(importBounds, s.GetBoundingBox());
                        }
                        if (entity.Type == EntityType.Text)
                        {
                            var text = (Text)entity;
                            var position = text.Position;
                            var s = new TextShape(new PointD(position.X, position.Y), text.Value);
                            shapes.Add(s);
                            importBounds = ExpandBounds(importBounds, s.GetBoundingBox());
                        }
                    }

                    if (importBounds.HasValue)
                        _shapeManager.EnsureWorldBoundsCovers(importBounds.Value);

                    var command = new BulkAddShapesCommand(_shapeManager, shapes);
                    _undoManager.ExecuteCommand(command);
                    _deferredRenderer.RenderNow();
                    panelCanvas.Invalidate();
                    btnZoomExtents_Click(sender, e);
                    _deferredRenderer.RenderNow();
                    panelCanvas.Invalidate();
                }
            }
        }

        private RectangleD ExpandBounds(RectangleD? current, RectangleD add)
        {
            if (!current.HasValue) return add;
            double minX = System.Math.Min(current.Value.Left, add.Left);
            double minY = System.Math.Min(current.Value.Top, add.Top);
            double maxX = System.Math.Max(current.Value.Right, add.Right);
            double maxY = System.Math.Max(current.Value.Bottom, add.Bottom);
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        #endregion

        #region Mouse Events

        private void PanelCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (_currentTool == DrawingTool.Polyline)
            {
                if (e.Button == MouseButtons.Left)
                {
                    PointD pt = (_currentSnapPoint != null) ? _currentSnapPoint.Position : _engine.ScreenToWorld(new PointD(e.Location.X, e.Location.Y));
                    SnapPoint? snap = _currentSnapPoint;
                    if (!_polylineDrawing)
                    {
                        _polylineVertices.Clear();
                        _polylineSnapPoints.Clear();
                        _polylineConfirmedSnapPoints.Clear();
                        _polylineDrawing = true;
                        _polylineClosed = false;
                    }
                    _polylineVertices.Add(pt);
                    _polylineSnapPoints.Add(snap);

                    if (snap != null)
                        _polylineConfirmedSnapPoints.Add(new SnapPoint(snap.Type, pt, null));
                    else
                        _polylineConfirmedSnapPoints.Add(new SnapPoint(SnapType.Endpoint, pt, null));

                    if (_polylineVertices.Count > 1)
                    {
                        var prev = _polylineVertices[_polylineVertices.Count - 2];
                        var mid = new PointD((prev.X + pt.X) / 2, (prev.Y + pt.Y) / 2);
                        _polylineConfirmedSnapPoints.Add(new SnapPoint(SnapType.Midpoint, mid, null));
                    }

                    _polylineConfirmedSnapPoints.RemoveAll(s => s.Type == SnapType.Intersection);

                    foreach (var ipt in _snapManager.GetPolylineSelfIntersections(_polylineVertices))
                        _polylineConfirmedSnapPoints.Add(new SnapPoint(SnapType.Intersection, ipt, null));

                    double minX = _polylineVertices.Min(v => v.X), maxX = _polylineVertices.Max(v => v.X);
                    double minY = _polylineVertices.Min(v => v.Y), maxY = _polylineVertices.Max(v => v.Y);
                    double buf = _engine.ScreenToWorldDistance(50);
                    var polyBounds = new RectangleD(minX - buf, minY - buf,
                                                     (maxX - minX) + buf * 2,
                                                     (maxY - minY) + buf * 2);
                    var nearbyShapes = _shapeManager.QueryShapesInBound(polyBounds)
                                                    .OfType<ISnapProvider>().ToList();
                    _polylineConfirmedSnapPoints.AddRange(
                        _snapManager.GetPolylineShapeIntersections(_polylineVertices, nearbyShapes));

                    panelCanvas.Invalidate();
                    return;
                }
                if (e.Button == MouseButtons.Right && _polylineDrawing)
                {
                    if (_polylineVertices.Count > 1)
                    {
                        var shape = new PolylineShape(_polylineVertices, false);
                        var command = new AddShapeCommand(_shapeManager, shape);
                        _undoManager.ExecuteCommand(command);
                        _deferredRenderer.RenderNow();
                    }
                    _polylineVertices.Clear();
                    _polylineSnapPoints.Clear();
                    _polylineConfirmedSnapPoints.Clear();
                    _polylineDrawing = false;
                    _polylineClosed = false;
                    panelCanvas.Invalidate();
                    return;
                }
            }

            if (e.Button == MouseButtons.Middle ||
                (_panToolActive && e.Button == MouseButtons.Left))
            {
                _isPanning = true;
                _panStart = new PointD(e.Location.X, e.Location.Y);
                _totalPanDelta = new PointD(0, 0);
                _deferredRenderer.BeginPan();
                panelCanvas.Cursor = Cursors.Hand;
                return;
            }

            if (e.Button == MouseButtons.Left && !_panToolActive)
            {
                PointD pt = (_currentSnapPoint != null) ? _currentSnapPoint.Position : _currentMouseWorldPos ?? _engine.ScreenToWorld(new PointD(e.Location.X, e.Location.Y));
                if (!_drawStartPoint.HasValue)
                {
                    _drawStartPoint = pt;
                    _drawCurrentPoint = _drawStartPoint;
                }
                else
                {
                    _drawCurrentPoint = pt;
                    FinishDrawing();
                }
            }
        }

        private void PanelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var newWorldPos = _engine.ScreenToWorld(new PointD(e.Location.X, e.Location.Y));
            float alpha = 1f;
            if (_lastInterpolatedMouseWorldPos.HasValue)
            {
                _currentMouseWorldPos = new PointD(
                    _lastInterpolatedMouseWorldPos.Value.X * (1 - alpha) + newWorldPos.X * alpha,
                    _lastInterpolatedMouseWorldPos.Value.Y * (1 - alpha) + newWorldPos.Y * alpha
                );
            }
            else
            {
                _currentMouseWorldPos = newWorldPos;
            }
            _lastInterpolatedMouseWorldPos = _currentMouseWorldPos;

            if (toolSnap.Checked)
            {
                RectangleD visibleWorldRect = _engine.GetVisibleWorldRectangle();
                var visibleShapes = _shapeManager.QueryShapesInBound(visibleWorldRect).OfType<ISnapProvider>().ToList();
                _lastVisibleShapesCount = visibleShapes.Count;

                if (visibleShapes.Count <= _maxShapesForSnapping)
                {
                    var mouseScreen = new PointD(e.Location.X, e.Location.Y);
                    var extraSnapPoints = new List<SnapPoint>();

                    if (_currentTool == DrawingTool.Polyline && _polylineDrawing && _polylineVertices.Count > 0)
                    {
                        foreach (var v in _polylineVertices)
                            extraSnapPoints.Add(new SnapPoint(SnapType.Endpoint, v, null));
                        for (int i = 0; i < _polylineVertices.Count - 1; i++)
                        {
                            var a = _polylineVertices[i];
                            var b = _polylineVertices[i + 1];
                            var mid = new PointD((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                            extraSnapPoints.Add(new SnapPoint(SnapType.Midpoint, mid, null));
                        }
                        foreach (var snap in _polylineConfirmedSnapPoints)
                        {
                            if (snap.Type == SnapType.Intersection)
                                extraSnapPoints.Add(snap);
                        }
                    }

                    PointD? fromPoint = _drawStartPoint;

                    var snapCandidates = _snapManager.GetSnapCandidates(
                        visibleShapes,
                        extraSnapPoints,
                        mouseScreen,
                        _engine,
                        _polylineVertices,
                        _currentMouseWorldPos,
                        _currentTool == DrawingTool.Polyline && _polylineDrawing,
                        fromPoint
                    ).ToList();

                    _lastSnapCandidatesCount = snapCandidates.Count;

                    if (snapCandidates.Count <= _maxSnapCandidates)
                    {
                        _currentSnapPoint = _snapManager.FindNearestSnapPointFromList(snapCandidates, mouseScreen, _engine);
                        if (_currentSnapPoint != null)
                            _currentMouseWorldPos = _currentSnapPoint.Position;
                        _snapTemporarilyDisabled = false;
                    }
                    else
                    {
                        _currentSnapPoint = null;
                        _snapTemporarilyDisabled = true;
                    }
                }
                else
                {
                    _currentSnapPoint = null;
                    _lastSnapCandidatesCount = 0;
                    _snapTemporarilyDisabled = true;
                }
            }
            else
            {
                _currentSnapPoint = null;
            }

            if (_currentTool == DrawingTool.Polyline && _polylineDrawing)
            {
                panelCanvas.Invalidate();
                return;
            }

            if (_isPanning)
            {
                double frameDeltaX = e.X - (_panStart.Value.X + _totalPanDelta.X);
                double frameDeltaY = e.Y - (_panStart.Value.Y + _totalPanDelta.Y);
                _totalPanDelta.X += frameDeltaX;
                _totalPanDelta.Y += frameDeltaY;
                _engine.Pan(frameDeltaX, frameDeltaY);
                panelCanvas.Invalidate();
                return;
            }

            if (_drawStartPoint.HasValue)
            {
                if (_lastInterpolatedDrawCurrentPoint.HasValue)
                {
                    _drawCurrentPoint = new PointD(
                        _lastInterpolatedDrawCurrentPoint.Value.X * (1 - alpha) + _currentMouseWorldPos.Value.X * alpha,
                        _lastInterpolatedDrawCurrentPoint.Value.Y * (1 - alpha) + _currentMouseWorldPos.Value.Y * alpha
                    );
                }
                else
                {
                    _drawCurrentPoint = _currentMouseWorldPos;
                }
                _lastInterpolatedDrawCurrentPoint = _drawCurrentPoint;
            }

            panelCanvas.Invalidate();
        }

        private void PanelCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle ||
                (e.Button == MouseButtons.Left && _panToolActive))
            {
                if (_isPanning)
                {
                    _isPanning = false;
                    _totalPanDelta = new PointD(0, 0);
                    _deferredRenderer.EndPan();
                    panelCanvas.Cursor = _panToolActive ? Cursors.Hand : Cursors.Cross;
                    panelCanvas.Invalidate();
                }
            }
        }

        private void PanelCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomFactor = e.Delta > 0 ? (float)DrawingEngine.ZOOM_STEP : (float)(1.0 / DrawingEngine.ZOOM_STEP);
            _engine.ZoomAtPoint(e.Location, zoomFactor);
            _deferredRenderer.RenderNow();
            panelCanvas.Invalidate();
        }

        private void PanelCanvas_MouseEnter(object sender, EventArgs e)
        {
            _ = panelCanvas.Focus();
        }

        #endregion

        #region Drawing Operations

        private IShape CreatePreviewShape()
        {
            if (!_drawStartPoint.HasValue || !_drawCurrentPoint.HasValue
                || (_currentTool == DrawingTool.Polyline && _polylineVertices.Count == 0))
                return null;
            return _currentTool switch
            {
                DrawingTool.Line => new LineShape(_drawStartPoint.Value, _drawCurrentPoint.Value),
                DrawingTool.Rectangle => new RectangleShape(_drawStartPoint.Value, _drawCurrentPoint.Value),
                DrawingTool.Circle => new CircleShape(_drawStartPoint.Value, _drawCurrentPoint.Value),
                DrawingTool.Ellipse => new EllipseShape(_drawStartPoint.Value, _drawCurrentPoint.Value),
                DrawingTool.Polyline => new PolylineShape(new List<PointD> { _drawStartPoint.Value, _drawCurrentPoint.Value }),
                _ => null
            };
        }

        private void FinishDrawing()
        {
            if (!_drawStartPoint.HasValue || !_drawCurrentPoint.HasValue)
                return;

            IShape newShape = _currentTool switch
            {
                DrawingTool.Line => new LineShape(_drawStartPoint.Value, _drawCurrentPoint.Value),
                DrawingTool.Rectangle => new RectangleShape(_drawStartPoint.Value, _drawCurrentPoint.Value),
                DrawingTool.Circle => new CircleShape(_drawStartPoint.Value, _drawCurrentPoint.Value),
                DrawingTool.Ellipse => new EllipseShape(_drawStartPoint.Value, _drawCurrentPoint.Value),
                DrawingTool.Polyline => new PolylineShape(new List<PointD> { _drawStartPoint.Value, _drawCurrentPoint.Value }),
                _ => null
            };

            if (newShape != null)
            {
                var command = new AddShapeCommand(_shapeManager, newShape);
                _undoManager.ExecuteCommand(command);
                _deferredRenderer.RenderNow();
            }

            _drawStartPoint = null;
            _drawCurrentPoint = null;
            panelCanvas.Invalidate();
        }

        #endregion

        #region Toolbar Events

        private void btnPan_Click(object sender, EventArgs e)
        {
            _panToolActive = !_panToolActive;
            panelCanvas.Cursor = _panToolActive ? Cursors.Hand : Cursors.Cross;
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            _engine.ZoomAtCenter(DrawingEngine.ZOOM_STEP);
            _deferredRenderer.RenderNow();
            panelCanvas.Invalidate();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            _engine.ZoomAtCenter(1.0f / DrawingEngine.ZOOM_STEP);
            _deferredRenderer.RenderNow();
            panelCanvas.Invalidate();
        }

        private void btnZoomExtents_Click(object sender, EventArgs e)
        {
            RectangleD extents = _shapeManager.CalculateExtents();
            _engine.ZoomToExtents(extents);
            _deferredRenderer.RenderNow();
            panelCanvas.Invalidate();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            _undoManager.Undo();
            _deferredRenderer.RenderNow();
            panelCanvas.Invalidate();
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            _undoManager.Redo();
            _deferredRenderer.RenderNow();
            panelCanvas.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            var command = new ClearAllCommand(_shapeManager);
            _undoManager.ExecuteCommand(command);
            _deferredRenderer.RenderNow();
            panelCanvas.Invalidate();
        }

        private void CancelActiveDrawing()
        {
            _drawStartPoint = null;
            _drawCurrentPoint = null;
            _lastInterpolatedDrawCurrentPoint = null;
            _polylineVertices.Clear();
            _polylineSnapPoints.Clear();
            _polylineConfirmedSnapPoints.Clear();
            _polylineDrawing = false;
            _polylineClosed = false;
            panelCanvas.Invalidate();
        }

        private void cbDrawingTool_SelectedIndexChanged(object sender, EventArgs e)
        {
            CancelActiveDrawing();
            _panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
        }

        private void toolLine_Click(object sender, EventArgs e)
        {
            CancelActiveDrawing();
            _currentTool = DrawingTool.Line;
            _panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
        }

        private void toolRectangle_Click(object sender, EventArgs e)
        {
            CancelActiveDrawing();
            _currentTool = DrawingTool.Rectangle;
            _panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
        }

        private void toolCircle_Click(object sender, EventArgs e)
        {
            CancelActiveDrawing();
            _currentTool = DrawingTool.Circle;
            _panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
        }

        private void toolEllipse_Click(object sender, EventArgs e)
        {
            CancelActiveDrawing();
            _currentTool = DrawingTool.Ellipse;
            _panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
        }

        private void toolPolyline_Click(object sender, EventArgs e)
        {
            _currentTool = DrawingTool.Polyline;
            _panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
            _polylineVertices.Clear();
            _polylineSnapPoints.Clear();
            _polylineDrawing = false;
            _polylineClosed = false;
        }

        private void btnShowHideGrid_Click(object sender, EventArgs e)
        {
            _showGrid = btnShowHideGrid.Checked;
            btnShowHideGrid.Text = _showGrid ? "Hide Grid" : "Show Grid";
            panelCanvas.Invalidate();
        }

        private void cbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTheme.SelectedIndex == 0)
                _renderer.SetDarkTheme();
            else
                _renderer.SetLightTheme();

            panelCanvas.BackColor = _renderer.BackgroundColor;
            panelCanvas.Invalidate();
        }

        private void toolPolygon_Click(object sender, EventArgs e)
        {
            // Reserved for polygon tool implementation
        }

        private void toolSnap_Click(object sender, EventArgs e)
        {
            // toolSnap.Checked is toggled automatically via CheckOnClick = true
        }

        private void btnLoadShapes_Click(object sender, EventArgs e)
        {
            TestPerformance();
            ZoomExtents();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            btnImportDxf_Click(sender, e);
            ZoomExtents();
        }

        #endregion

        #region Keyboard Shortcuts

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.R)
            {
                CancelActiveDrawing();
                _currentTool = DrawingTool.Rectangle;
                _panToolActive = false;
                panelCanvas.Cursor = Cursors.Cross;
            }
            else if (keyData == Keys.P)
            {
                CancelActiveDrawing();
                _currentTool = DrawingTool.Polyline;
                _panToolActive = false;
                panelCanvas.Cursor = Cursors.Cross;
            }
            else if (keyData == Keys.L)
            {
                CancelActiveDrawing();
                _currentTool = DrawingTool.Line;
                _panToolActive = false;
                panelCanvas.Cursor = Cursors.Cross;
            }

            if (_currentTool == DrawingTool.Polyline && _polylineDrawing)
            {
                if (keyData == Keys.Escape)
                {
                    _polylineVertices.Clear();
                    _polylineDrawing = false;
                    _polylineClosed = false;
                    panelCanvas.Invalidate();
                    return true;
                }
                if (keyData == Keys.C)
                {
                    if (_polylineVertices.Count > 2)
                    {
                        _polylineClosed = true;
                        var shape = new PolylineShape(_polylineVertices, true);
                        var command = new AddShapeCommand(_shapeManager, shape);
                        _undoManager.ExecuteCommand(command);
                    }
                    _polylineVertices.Clear();
                    _polylineDrawing = false;
                    panelCanvas.Invalidate();
                    _deferredRenderer.RenderNow();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Performance Test

        private void TestPerformance()
        {
            var random = new Random();
            var shapes = new List<IShape>();

            for (int i = 0; i < 1000; i++)
            {
                float x1 = random.Next(330000, 340000);
                float y1 = random.Next(3060000, 3070000);
                float x2 = x1 + random.Next(100, 500);
                float y2 = y1 + random.Next(100, 500);
                shapes.Add(new RectangleShape(new PointD(x1, y1), new PointD(x2, y2)));
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var command = new BulkAddShapesCommand(_shapeManager, shapes);
            _undoManager.ExecuteCommand(command);
            stopwatch.Stop();

            MessageBox.Show(
                $"Added {shapes.Count} shapes in {stopwatch.ElapsedMilliseconds}ms\n\n" +
                $"Performance: {shapes.Count / (stopwatch.ElapsedMilliseconds / 1000.0):F0} shapes/sec\n" +
                $"Undo/Redo: Supported (1 command for all {shapes.Count} shapes)",
                "Performance Test",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            panelCanvas.Invalidate();
        }

        #endregion

        #region Debug Overlay

        private void DrawDebugOverlay(Graphics g)
        {
            var visibleWorldRect = _engine.GetVisibleWorldRectangle();
            var visibleShapes = _shapeManager.QueryShapesInBound(visibleWorldRect).OfType<ISnapProvider>().ToList();
            var allShapes = _shapeManager.GetAllShapes();

            using (var font = new Font("Consolas", 10, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Red))
            using (var bgBrush = new SolidBrush(Color.FromArgb(180, 30, 30, 30)))
            {
                string debugText = $"Shapes: {visibleShapes.Count} / Total: {allShapes.Count}\n" +
                    $"Viewport: X={visibleWorldRect.X:F0}, Y={visibleWorldRect.Y:F0}, W={visibleWorldRect.Width:F0}, H={visibleWorldRect.Height:F0}";

                if (allShapes.Count > 0)
                {
                    var box = allShapes.Last().GetBoundingBox();
                    debugText += $"\nLastShapeBox: X={box.X:F0}, Y={box.Y:F0}, W={box.Width:F0}, H={box.Height:F0}";
                }

                if (toolSnap.Checked)
                {
                    RectangleD visibleWorldRectDbg = _engine.GetVisibleWorldRectangle();
                    var visibleShapesDbg = _shapeManager.QueryShapesInBound(visibleWorldRectDbg).OfType<ISnapProvider>().ToList();
                    var mouseScreenDbg = panelCanvas.PointToClient(Cursor.Position);
                    var mouseScreenPt = new PointD(mouseScreenDbg.X, mouseScreenDbg.Y);
                    var extraSnapPointsDbg = new List<SnapPoint>();

                    if (_currentTool == DrawingTool.Polyline && _polylineDrawing && _polylineVertices.Count > 0)
                    {
                        foreach (var v in _polylineVertices)
                            extraSnapPointsDbg.Add(new SnapPoint(SnapType.Endpoint, v, null));
                        for (int i = 0; i < _polylineVertices.Count - 1; i++)
                        {
                            var a = _polylineVertices[i];
                            var b = _polylineVertices[i + 1];
                            var mid = new PointD((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                            extraSnapPointsDbg.Add(new SnapPoint(SnapType.Midpoint, mid, null));
                        }
                    }

                    var allSnapCandidates = _snapManager.GetSnapCandidates(
                        visibleShapesDbg,
                        extraSnapPointsDbg,
                        mouseScreenPt,
                        _engine,
                        _polylineVertices,
                        _currentMouseWorldPos,
                        _currentTool == DrawingTool.Polyline && _polylineDrawing
                    ).ToList();

                    debugText += $"\nSnap Candidates: {allSnapCandidates.Count}";
                    foreach (var snap in allSnapCandidates)
                        debugText += $"\n  [{snap.Type}] X={snap.Position.X:F4}, Y={snap.Position.Y:F4}";
                }

                var size = g.MeasureString(debugText, font);
                float x = 10;
                float y = panelCanvas.Height - size.Height - 10;
                g.FillRectangle(bgBrush, x - 5, y - 5, size.Width + 10, size.Height + 10);
                g.DrawString(debugText, font, brush, x, y);
            }
        }

        private void btnShowDebugLog_Click(object sender, EventArgs e)
        {
            _isDebugMode = !_isDebugMode;
            btnShowDebugLog.Text = _isDebugMode ? "Hide Debug Log" : "Show Debug Log";
            panelCanvas.Invalidate();
        }

        #endregion

        #region Public API (called from MainForm)

        /// <summary>
        /// Called from MainForm to zoom to all loaded shapes.
        /// </summary>
        public void ZoomExtents() => btnZoomExtents_Click(this, EventArgs.Empty);

        /// <summary>
        /// Called from MainForm to undo last action.
        /// </summary>
        public void Undo() => btnUndo_Click(this, EventArgs.Empty);

        /// <summary>
        /// Called from MainForm to redo last undone action.
        /// </summary>
        public void Redo() => btnRedo_Click(this, EventArgs.Empty);

        /// <summary>
        /// Called if undo is needed externally (e.g. renderer error recovery).
        /// </summary>
        public void UndoLastCommandIfPossible()
        {
            _undoManager?.Undo();
            panelCanvas.Invalidate();
        }

        /// <summary>
        /// Applies background color from project settings.
        /// Called when project opens or settings change.
        /// </summary>
        public void ApplyBackgroundColor(Color color)
        {
            // Update renderer directly
            if (color.GetBrightness() < 0.5f)
                _renderer.SetDarkTheme();
            else
                _renderer.SetLightTheme();

            // Override with exact color from settings
            panelCanvas.BackColor = color;
            panelCanvas.Invalidate();
        }

        /// <summary>
        /// Sets grid visibility from project settings.
        /// </summary>
        public void ApplyGridVisible(bool visible)
        {
            _showGrid = visible;
            btnShowHideGrid.Checked = visible;
            btnShowHideGrid.Text = visible
                ? "Hide Grid" : "Show Grid";
            panelCanvas.Invalidate();
        }

        /// <summary>
        /// Sets snap enabled from project settings.
        /// </summary>
        public void ApplySnapEnabled(bool enabled)
        {
            toolSnap.Checked = enabled;
            panelCanvas.Invalidate();
        }
        private void UpdateWorldBoundsToCurrentView()
        {
            RectangleD viewport = _engine.GetViewportBounds();
            _shapeManager.SetWorldBounds(viewport);
        }

        #endregion

        #region Cleanup

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderer?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
