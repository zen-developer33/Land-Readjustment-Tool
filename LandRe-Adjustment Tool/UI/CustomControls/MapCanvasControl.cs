using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;

namespace Land_Readjustment_Tool.UI.CustomControls
{
    public partial class MapCanvasControl : UserControl
    {
        private const int ZoomSettleIntervalMs = 50;
        private const double ScreenPixelsPerMetre = 96.0 / 0.0254;
        private static readonly double[] StandardScaleDenominators = BuildStandardScaleDenominators();

        private readonly MapCanvasEngine _engine;
        private readonly MapCanvasRenderer _renderer;
        private readonly RasterDeferredRenderer _rasterDeferredRenderer = new();
        private readonly List<IRasterRenderLayer> _rasterRenderLayers = [];
        private MapCanvasRenderSettings _renderSettings;

        public event Action<string, string, double>? StatusChanged;

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
        private bool _blockPanUntilZoomSettle;
        private bool _holdZoomStartFrameUntilRasterRefresh;
        private bool _suppressStaleRasterFrameUntilFreshRender;
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
            RefreshRasterCacheForCurrentViewAsync();
            RequestRender();
        }

        private void canvasSurface_Paint(object? sender, PaintEventArgs e)
        {
            _renderer.Render(
                e.Graphics,
                GetRasterRenderFrame(),
                ShouldDeferDirectRasterRendering,
                GetZoomWindowRectangle());
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

            if (_zoomWindowActive && e.Button == MouseButtons.Left)
            {
                _isSelectingZoomWindow = true;
                _zoomWindowStart = e.Location;
                _zoomWindowCurrent = e.Location;
                RequestRender();
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
                    RefreshRasterCacheForCurrentViewAsync();
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

            StatusChanged?.Invoke(coordinatesText, modeText, _engine.ZoomScale);
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

            if (_holdZoomStartFrameUntilRasterRefresh &&
                _rasterDeferredRenderer.TryGetZoomFrame(
                    _engine,
                    out RasterRenderFrame heldZoomFrame))
            {
                return heldZoomFrame;
            }

            if (!_suppressStaleRasterFrameUntilFreshRender &&
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
            SetLiveTileInternetFetchingSuspended(false);
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
                _holdZoomStartFrameUntilRasterRefresh = false;
                _rasterDeferredRenderer.BeginZoom(
                    canvasSurface.Size,
                    _rasterRenderLayers,
                    _engine);
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

        private async void RefreshRasterCacheForCurrentViewAsync(bool endZoomWhenComplete = false)
        {
            if (IsDisposed || Disposing)
            {
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
            List<IRasterRenderLayer> extentLayers = _rasterRenderLayers
                .Where(layer => layer is not XyzLiveTileRenderLayer)
                .ToList();

            if (extentLayers.Count == 0)
            {
                _engine.SetWorldBounds(MapCanvasEngine.DefaultWorldBounds);
                return;
            }

            double minX = extentLayers.Min(layer =>
                Math.Min(layer.WorldBounds.Left, layer.WorldBounds.Right));
            double maxX = extentLayers.Max(layer =>
                Math.Max(layer.WorldBounds.Left, layer.WorldBounds.Right));
            double minY = extentLayers.Min(layer =>
                Math.Min(layer.WorldBounds.Top, layer.WorldBounds.Bottom));
            double maxY = extentLayers.Max(layer =>
                Math.Max(layer.WorldBounds.Top, layer.WorldBounds.Bottom));

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
