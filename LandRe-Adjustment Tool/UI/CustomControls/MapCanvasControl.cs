using System.Drawing;
using System.Windows.Forms;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;

namespace Land_Readjustment_Tool.UI.CustomControls
{
    public partial class MapCanvasControl : UserControl
    {
        private const int ZoomSettleIntervalMs = 40;

        private readonly MapCanvasEngine _engine;
        private readonly MapCanvasRenderer _renderer;
        private readonly RasterDeferredRenderer _rasterDeferredRenderer = new();
        private readonly List<RasterRenderLayer> _rasterRenderLayers = [];
        private MapCanvasRenderSettings _renderSettings;

        public event Action<string, string>? StatusChanged;

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

        public void ZoomIn()
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            _engine.ZoomIn();
            RefreshRasterCacheForCurrentView();
            RequestRender();
        }

        public void ZoomOut()
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            _engine.ZoomOut();
            RefreshRasterCacheForCurrentView();
            RequestRender();
        }

        public void ZoomExtents()
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            _engine.ZoomToExtents();
            RefreshRasterCacheForCurrentView();
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
            RefreshRasterCacheForCurrentView();
            RequestRender();
        }

        public void SetPanToolActive(bool active)
        {
            if (IsCanvasInteractionLocked)
            {
                return;
            }

            _panToolActive = active;
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
            _isPanning = false;
            UpdateCanvasCursor();
            UpdateStatusBar();
        }

        public bool IsPanToolActive => _panToolActive;

        public void SetRasterLayers(
            IEnumerable<CanvasLayer>? rasterLayers,
            string? projectFolderPath)
        {
            DisposeRasterRenderLayers();

            if (rasterLayers != null)
            {
                foreach (CanvasLayer rasterLayer in rasterLayers
                    .Where(layer => layer.IsVisible)
                    .OrderBy(layer => layer.DisplayOrder)
                    .ThenBy(layer => layer.Name))
                {
                    try
                    {
                        _rasterRenderLayers.Add(
                            RasterRenderLayer.FromCanvasLayer(
                                rasterLayer,
                                projectFolderPath));
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
            RefreshRasterCacheForCurrentView();
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
            RefreshRasterCacheForCurrentView();
            RequestRender();
        }

        private void canvasSurface_Paint(object? sender, PaintEventArgs e)
        {
            _renderer.Render(
                e.Graphics,
                GetRasterRenderFrame(),
                IsInteractiveNavigation,
                GetZoomWindowRectangle());
        }

        private void canvasSurface_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (!_isZooming)
            {
                CancelActiveCanvasGesture();
                _rasterDeferredRenderer.BeginZoom(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine);
            }

            _isZooming = true;
            double zoomFactor = e.Delta > 0 ? MapCanvasEngine.ZoomStep : 1.0 / MapCanvasEngine.ZoomStep;
            _zoomDirection = e.Delta > 0 ? "In" : "Out";
            UpdateStatusBar();
            
            _engine.ZoomAtPoint(e.Location, zoomFactor);
            _currentMouseWorld = _engine.ScreenToWorld(e.Location);
            RequestRender();
            
            // Redraw the precise raster shortly after the last wheel event.
            _zoomingStatusTimer.Stop();
            _zoomingStatusTimer.Start();
        }

        private void canvasSurface_MouseDown(object? sender, MouseEventArgs e)
        {
            canvasSurface.Focus();

            if (IsCanvasInteractionLocked)
            {
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

            if (e.Button == MouseButtons.Middle || (_panToolActive && e.Button == MouseButtons.Left))
            {
                _isPanning = true;
                _lastPanPoint = e.Location;
                _totalPanDelta = PointF.Empty;
                _panStartWorld = _engine.ScreenToWorld(e.Location);
                _currentMouseWorld = _panStartWorld;
                _rasterDeferredRenderer.BeginPan(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine);
                canvasSurface.Capture = true;
                UpdateCanvasCursor();
                UpdateStatusBar();
            }
        }

        private void canvasSurface_MouseMove(object? sender, MouseEventArgs e)
        {
            if (IsCanvasInteractionLocked)
            {
                _currentMouseWorld = _engine.ScreenToWorld(e.Location);
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

            _currentMouseWorld = _engine.ScreenToWorld(e.Location);
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
                    RefreshRasterCacheForCurrentView();
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
                RefreshRasterCacheForCurrentView();
                UpdateCanvasCursor();
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
            if (_zoomWindowActive)
            {
                canvasSurface.Cursor = Cursors.Cross;
            }
            else if (_panToolActive || _isPanning)
            {
                canvasSurface.Cursor = Cursors.Hand;
            }
            else
            {
                canvasSurface.Cursor = Cursors.Default;
            }
        }

        private void UpdateStatusBar()
        {
            string coordinatesText = _currentMouseWorld.HasValue? $"E: {_currentMouseWorld.Value.X:F4}    N: {_currentMouseWorld.Value.Y:F4}" : "E: --    N: --"; 

            string modeText = GetModeText();

            StatusChanged?.Invoke(coordinatesText, modeText);
        }

        private string GetModeText()
        {
            if (_isZooming && _zoomDirection != null)
            {
                return $"Mode: Zooming {_zoomDirection}";
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

        private bool IsCanvasInteractionLocked => _isZooming;

        private bool IsInteractiveNavigation =>
            _isPanning || _isZooming || _isSelectingZoomWindow;

        private RasterRenderFrame? GetRasterRenderFrame()
        {
            if (_isPanning &&
                _rasterDeferredRenderer.TryGetPanFrame(
                    _totalPanDelta,
                    out RasterRenderFrame panFrame))
            {
                return panFrame;
            }

            if (_isZooming &&
                _rasterDeferredRenderer.TryGetZoomFrame(
                    _engine,
                    out RasterRenderFrame zoomFrame))
            {
                return zoomFrame;
            }

            if (!IsInteractiveNavigation &&
                _rasterDeferredRenderer.TryGetCacheFrame(
                    out RasterRenderFrame cacheFrame))
            {
                return cacheFrame;
            }

            return null;
        }

        private void ZoomingStatusTimer_Tick(object? sender, EventArgs e)
        {
            _zoomingStatusTimer.Stop();
            _isZooming = false;
            _zoomDirection = null;
            _rasterDeferredRenderer.EndZoom();
            RefreshRasterCacheForCurrentView();
            UpdateCanvasCursor();
            UpdateStatusBar();
            RequestRender();
        }

        private void StopAndDisposeZoomingStatusTimer()
        {
            if (_zoomingStatusTimerDisposed)
            {
                return;
            }

            _zoomingStatusTimer.Stop();
            _zoomingStatusTimer.Tick -= ZoomingStatusTimer_Tick;
            _zoomingStatusTimer.Dispose();
            _zoomingStatusTimerDisposed = true;
        }

        private void CancelActiveCanvasGesture()
        {
            if (_isPanning)
            {
                _isPanning = false;
                _panStartWorld = null;
                _totalPanDelta = PointF.Empty;
                canvasSurface.Capture = false;
                _rasterDeferredRenderer.Invalidate();
            }

            if (_isSelectingZoomWindow)
            {
                _isSelectingZoomWindow = false;
                _zoomWindowActive = false;
            }
        }

        private void RefreshRasterCacheForCurrentView()
        {
            _rasterDeferredRenderer.RenderNow(
                canvasSurface.Size,
                _rasterRenderLayers,
                _engine);
        }

        private void UpdateRasterWorldBounds()
        {
            if (_rasterRenderLayers.Count == 0)
            {
                _engine.SetWorldBounds(MapCanvasEngine.DefaultWorldBounds);
                return;
            }

            double minX = _rasterRenderLayers.Min(layer =>
                Math.Min(layer.WorldBounds.Left, layer.WorldBounds.Right));
            double maxX = _rasterRenderLayers.Max(layer =>
                Math.Max(layer.WorldBounds.Left, layer.WorldBounds.Right));
            double minY = _rasterRenderLayers.Min(layer =>
                Math.Min(layer.WorldBounds.Top, layer.WorldBounds.Bottom));
            double maxY = _rasterRenderLayers.Max(layer =>
                Math.Max(layer.WorldBounds.Top, layer.WorldBounds.Bottom));

            _engine.SetWorldBounds(
                new RectangleD(minX, minY, maxX - minX, maxY - minY));
        }

        private void DisposeRasterRenderLayers()
        {
            foreach (RasterRenderLayer rasterLayer in _rasterRenderLayers)
                rasterLayer.Dispose();

            _rasterRenderLayers.Clear();
            _renderer.UpdateRasterLayers(Array.Empty<RasterRenderLayer>());
            _rasterDeferredRenderer.Invalidate();
        }
    }
}
