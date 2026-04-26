using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;

namespace Land_Readjustment_Tool.UI.CustomControls
{
    public partial class MapCanvasControl : UserControl
    {
        private readonly MapCanvasEngine _engine;
        private readonly MapCanvasRenderer _renderer;
        private MapCanvasRenderSettings _renderSettings;

        public event Action<string, string, string, string>? StatusChanged;

        private bool _panToolActive;
        private bool _isPanning;
        private bool _zoomWindowActive;
        private bool _isSelectingZoomWindow;
        private Point _lastPanPoint;
        private Point _zoomWindowStart;
        private Point _zoomWindowCurrent;
        private PointD? _currentMouseWorld;

        public MapCanvasControl()
        {
            InitializeComponent();
            ConfigureGraphicsPipeline();
            _engine = new MapCanvasEngine(canvasSurface.Size);
            _renderer = new MapCanvasRenderer();
            _renderSettings = MapCanvasRenderSettings.CreateLightDefaults();
            WireInteractionEvents();
            UpdateStatusBar();
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
            BackColor = _renderSettings.BackgroundColor;
            canvasSurface.BackColor = _renderSettings.BackgroundColor;
            RequestRender();
        }

        public void ApplyBackgroundColor(Color color)
        {
            _renderSettings.BackgroundColor = color;
            BackColor = color;
            canvasSurface.BackColor = color;
            RequestRender();
        }

        public void ApplyGridColor(Color color)
        {
            _renderSettings.MajorGridColor = Color.FromArgb(150, color.R, color.G, color.B);
            _renderSettings.MinorGridColor = Color.FromArgb(70, color.R, color.G, color.B);
            RequestRender();
        }

        public void ApplyGridVisible(bool visible)
        {
            _renderSettings.ShowGrid = visible;
            RequestRender();
        }

        public void ApplySnapEnabled(bool enabled)
        {
            // Snapping will be added when geometry editing returns to the new canvas.
        }

        public void ZoomIn()
        {
            _engine.ZoomIn();
            RequestRender();
        }

        public void ZoomOut()
        {
            _engine.ZoomOut();
            RequestRender();
        }

        public void ZoomExtents()
        {
            _engine.ZoomToExtents();
            RequestRender();
        }

        public void SetPanToolActive(bool active)
        {
            _panToolActive = active;
            _zoomWindowActive = false;
            _isSelectingZoomWindow = false;
            UpdateCanvasCursor();
            UpdateStatusBar();
        }

        public void BeginZoomWindow()
        {
            _zoomWindowActive = true;
            _panToolActive = false;
            _isPanning = false;
            UpdateCanvasCursor();
            UpdateStatusBar();
        }

        public bool IsPanToolActive => _panToolActive;

        private void canvasSurface_Resize(object? sender, EventArgs e)
        {
            if (_engine == null)
            {
                return;
            }

            _engine.UpdateCanvasSize(canvasSurface.Size);
            RequestRender();
        }

        private void canvasSurface_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            _renderer.Render(
                g,
                _engine,
                _renderSettings,
                GetZoomWindowRectangle());
        }

        private void canvasSurface_MouseWheel(object? sender, MouseEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? MapCanvasEngine.ZoomStep : 1.0 / MapCanvasEngine.ZoomStep;
            _engine.ZoomAtPoint(e.Location, zoomFactor);
            _currentMouseWorld = _engine.ScreenToWorld(e.Location);
            RequestRender();
        }

        private void canvasSurface_MouseDown(object? sender, MouseEventArgs e)
        {
            canvasSurface.Focus();

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
                canvasSurface.Capture = true;
                UpdateCanvasCursor();
                UpdateStatusBar();
            }
        }

        private void canvasSurface_MouseMove(object? sender, MouseEventArgs e)
        {
            _currentMouseWorld = _engine.ScreenToWorld(e.Location);

            if (_isSelectingZoomWindow)
            {
                _zoomWindowCurrent = e.Location;
                RequestRender();
                return;
            }

            if (_isPanning)
            {
                int dx = e.X - _lastPanPoint.X;
                int dy = e.Y - _lastPanPoint.Y;
                _engine.PanByScreenDelta(dx, dy);
                _lastPanPoint = e.Location;
                RequestRender();
                return;
            }

            RequestRender();
        }

        private void canvasSurface_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_isSelectingZoomWindow)
            {
                Rectangle? rectangle = GetZoomWindowRectangle();
                _isSelectingZoomWindow = false;
                _zoomWindowActive = false;

                if (rectangle.HasValue && rectangle.Value.Width > 8 && rectangle.Value.Height > 8)
                {
                    ZoomToScreenRectangle(rectangle.Value);
                }

                UpdateCanvasCursor();
                RequestRender();
                return;
            }

            if (_isPanning)
            {
                _isPanning = false;
                canvasSurface.Capture = false;
                UpdateCanvasCursor();
                RequestRender();
            }
        }

        private void canvasSurface_MouseLeave(object? sender, EventArgs e)
        {
            if (!_isPanning && !_isSelectingZoomWindow)
            {
                _currentMouseWorld = null;
                RequestRender();
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
            string coordinatesText = _currentMouseWorld.HasValue
                ? $"E: {_currentMouseWorld.Value.X:F4}    N: {_currentMouseWorld.Value.Y:F4}"
                : "E: --    N: --";

            string zoomText = _engine.GetZoomLabel();
            string scaleText = _engine.GetScaleLabel();
            string modeText = GetModeText();

            StatusChanged?.Invoke(coordinatesText, zoomText, scaleText, modeText);
        }

        private string GetModeText()
        {
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
    }
}
