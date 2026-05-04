using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;


namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Renders map-canvas visual layers such as background, adaptive grid,
    /// axis/origin marker, and interactive zoom window overlays.
    /// </summary>
    public sealed class MapCanvasRenderer : IDisposable
    {
        private readonly record struct MapCanvasRenderViewport(
            RectangleD VisibleWorldBounds,
            RectangleF VisibleScreenBounds);

        private readonly Font _gridFont = new("Arial", 8.0f, FontStyle.Regular);
        private readonly Font _axisFont = new("Arial", 9.0f, FontStyle.Regular);
        private readonly MapCanvasEngine _engine;
        private readonly MapCanvasRenderOrderService _renderOrderService;
        private double _lastAdaptiveMinorSize;
        private MapCanvasRenderSettings _settings;
        private IReadOnlyList<IRasterRenderLayer> _rasterLayers = [];

        /// <summary>
        /// Creates a renderer for drawing the map canvas using the supplied engine and settings.
        /// </summary>
        /// <param name="engine">Viewport engine providing world/screen transforms.</param>
        /// <param name="settings">Rendering options and theme colors.</param>
        /// <param name="renderOrderService">Optional service that defines canvas render-pass order.</param>
        public MapCanvasRenderer(
            MapCanvasEngine engine,
            MapCanvasRenderSettings settings,
            MapCanvasRenderOrderService? renderOrderService = null)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _settings = settings?.Clone()
                ?? throw new ArgumentNullException(nameof(settings));
            _renderOrderService = renderOrderService
                ?? new MapCanvasRenderOrderService();
        }

        /// <summary>
        /// Replaces the renderer settings while keeping the renderer-owned copy isolated.
        /// </summary>
        /// <param name="settings">Updated rendering options and theme colors.</param>
        public void UpdateSettings(MapCanvasRenderSettings settings)
        {
            _settings = settings?.Clone()
                ?? throw new ArgumentNullException(nameof(settings));
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
            Rectangle? zoomWindowRectangle)
        {
            ArgumentNullException.ThrowIfNull(graphics);

            ConfigureGraphics(graphics);
            graphics.Clear(_settings.BackgroundColor);
            MapCanvasRenderViewport viewport = CreateViewport(graphics);

            foreach (MapCanvasRenderStage stage in _renderOrderService.GetFrameStages())
            {
                ConfigureGraphics(graphics);

                switch (stage)
                {
                    case MapCanvasRenderStage.FixedReference:
                        RenderFixedReferenceLayers(graphics, viewport);
                        break;

                    case MapCanvasRenderStage.RasterContent:
                        RenderRasterContent(
                            graphics,
                            viewport,
                            rasterFrame,
                            interactiveRaster);
                        break;

                    case MapCanvasRenderStage.VectorContent:


                    case MapCanvasRenderStage.InteractionOverlay:
                        RenderInteractionOverlay(graphics, zoomWindowRectangle);
                        break;
                }
            }
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

        /// <summary>
        /// Draws fixed reference visuals that should be placed before map content.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="viewport">Current world and screen viewport.</param>
        private void RenderFixedReferenceLayers(
            Graphics graphics,
            MapCanvasRenderViewport viewport)
        {
            if (_settings.ShowGrid)
            {
                RenderGrid(graphics, viewport);
            }

            if (_settings.ShowAxisLines || _settings.ShowOriginMarker)
            {
                RenderAxisAndOriginMarker(graphics, viewport);
            }
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
                RasterRenderFrame frame = rasterFrame.Value;
                try
                {
                    DrawRasterFrame(graphics, frame);
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
                RenderRasterLayers(graphics, viewport, interactiveRaster);
            }
        }

        /// <summary>
        /// Draws temporary interaction feedback that must stay visible above map content.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="zoomWindowRectangle">Optional screen-space zoom-window rectangle.</param>
        private void RenderInteractionOverlay(
            Graphics graphics,
            Rectangle? zoomWindowRectangle)
        {
            if (zoomWindowRectangle.HasValue)
            {
                RenderZoomWindow(graphics, zoomWindowRectangle.Value);
            }
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
            bool interactive)
        {
            if (_rasterLayers.Count == 0)
                return;

            foreach (IRasterRenderLayer layer in _rasterLayers)
            {
                layer.RenderVisible(
                    graphics,
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
        private static void DrawRasterFrame(
            Graphics graphics,
            RasterRenderFrame rasterFrame)
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

                GraphicsState state = graphics.Save();
                try
                {
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.DrawImage(
                        rasterFrame.Bitmap,
                        [
                            new PointF(destination.Left, destination.Top),
                            new PointF(destination.Right, destination.Top),
                            new PointF(destination.Left, destination.Bottom)
                        ],
                        source,
                        GraphicsUnit.Pixel);
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
                finally
                {
                    graphics.Restore(state);
                }
            }
        }

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
            _gridFont.Dispose();
            _axisFont.Dispose();
        }

        /// <summary>
        /// Applies high-quality graphics options for anti-aliased rendering.
        /// </summary>
        /// <param name="graphics">Graphics context to configure.</param>
        private static void ConfigureGraphics(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        /// <summary>
        /// Draws adaptive major/minor grid lines and optional coordinate labels
        /// for the current visible world extent.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="viewport">Current world and screen viewport.</param>
        private void RenderGrid(
            Graphics graphics,
            MapCanvasRenderViewport viewport)
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
            double adaptiveMajorSize = adaptiveMinorSize * majorDivisions;
            _lastAdaptiveMinorSize = adaptiveMinorSize;

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
                _settings.ShowGridLabels &&
                zoomScale > 0.00001 &&
                zoomScale < 10000 &&
                estimatedVerticalLines < 500;

            using Pen minorPen = new(_settings.MinorGridColor, 0.25f);
            using Pen majorPen = new(_settings.MajorGridColor, 0.25f);
            using Brush gridTextBrush = new SolidBrush(_settings.GridLabelColor);

            for (double x = startX; x <= endX; x += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(x, adaptiveMajorSize);
                Pen pen = isMajor ? majorPen : minorPen;
                PointD screenStart = _engine.WorldToScreen(new PointD(x, worldBottom));
                PointD screenEnd = _engine.WorldToScreen(new PointD(x, worldTop));

                if (IsValidPoint(screenStart) && IsValidPoint(screenEnd))
                {
                    graphics.DrawLine(pen, (float)screenStart.X, (float)screenStart.Y, (float)screenEnd.X, (float)screenEnd.Y);
                }

                if (isMajor && showLabels)
                {
                    using StringFormat sf = new()
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Far
                    };

                    graphics.DrawString(
                        FormatGridLabel(x, adaptiveMajorSize),
                        _gridFont,
                        gridTextBrush,
                        (float)screenStart.X,
                        (float)screenStart.Y,
                        sf);
                }
            }

            for (double y = startY; y <= endY; y += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(y, adaptiveMajorSize);
                Pen pen = isMajor ? majorPen : minorPen;
                PointD screenStart = _engine.WorldToScreen(new PointD(worldLeft, y));
                PointD screenEnd = _engine.WorldToScreen(new PointD(worldRight, y));

                if (IsValidPoint(screenStart) && IsValidPoint(screenEnd))
                {
                    graphics.DrawLine(pen, (float)screenStart.X, (float)screenStart.Y, (float)screenEnd.X, (float)screenEnd.Y);
                }

                if (isMajor && showLabels)
                {
                    using StringFormat sf = new()
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };

                    graphics.DrawString(
                        FormatGridLabel(y, adaptiveMajorSize),
                        _gridFont,
                        gridTextBrush,
                        10,
                        (float)screenStart.Y,
                        sf);
                }
            }
        }

        /// <summary>
        /// Draws the temporary zoom-window overlay rectangle.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="rectangle">Screen-space zoom-window rectangle.</param>
        private void RenderZoomWindow(Graphics graphics, Rectangle rectangle)
        {
            if (rectangle.Width < 2 || rectangle.Height < 2)
            {
                return;
            }

            using Brush fill = new SolidBrush(_settings.ZoomWindowFillColor);
            using Pen border = new(_settings.ZoomWindowBorderColor, _settings.ZoomWindowLineWidth)
            {
                DashStyle = _settings.ZoomWindowLineType
            };
            graphics.FillRectangle(fill, rectangle);
            graphics.DrawRectangle(border, rectangle);
        }

        /// <summary>
        /// Renders world-origin axis lines and the origin marker/labels according to visibility settings.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        private void RenderAxisAndOriginMarker(
            Graphics graphics,
            MapCanvasRenderViewport viewport)
        {
            RectangleF clientRect = viewport.VisibleScreenBounds;
            PointD originScreen = _engine.WorldToScreen(new PointD(0, 0));
            PointD topScreen = _engine.WorldToScreen(
                new PointD(0, viewport.VisibleWorldBounds.Bottom));
            PointD rightScreen = _engine.WorldToScreen(
                new PointD(viewport.VisibleWorldBounds.Right, 0));

            if (_settings.ShowAxisLines)
            {
                using Pen xAxisPen = new(_settings.AxisXColor, _settings.AxisLineWidth);
                using Pen yAxisPen = new(_settings.AxisYColor, _settings.AxisLineWidth);

                if (IsValidPoint(originScreen) && IsValidPoint(rightScreen) && IsFarEnough(originScreen, rightScreen))
                {
                    graphics.DrawLine(
                        xAxisPen,
                        (float)originScreen.X,
                        (float)originScreen.Y,
                        (float)rightScreen.X,
                        (float)rightScreen.Y);
                }

                if (IsValidPoint(originScreen) && IsValidPoint(topScreen) && IsFarEnough(originScreen, topScreen))
                {
                    graphics.DrawLine(
                        yAxisPen,
                        (float)originScreen.X,
                        (float)originScreen.Y,
                        (float)topScreen.X,
                        (float)topScreen.Y);
                }
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

            using Pen markerPen = new(_settings.AxisMarkerColor, _settings.AxisMarkerLineWidth);
            using Brush markerBrush = new SolidBrush(_settings.AxisMarkerColor);
            using Brush markerTextBrush = new SolidBrush(_settings.AxisLabelColor);

            if (!originInViewport)
            {
                const float edgePadding = 18.0f;
                float x = clientRect.Left + edgePadding;
                float y = clientRect.Bottom - edgePadding;

                graphics.DrawLine(markerPen, x, y, x + markerLength, y);
                graphics.DrawLine(markerPen, x, y, x, y - markerLength);
                graphics.FillRectangle(
                    markerBrush,
                    x - markerSquareSize / 2.0f,
                    y - markerSquareSize / 2.0f,
                    markerSquareSize,
                    markerSquareSize);

                if (_settings.ShowAxisLabels)
                {
                    graphics.DrawString("X", _axisFont, markerTextBrush, x + markerLength + 4.0f, y - 10.0f);
                    graphics.DrawString("Y", _axisFont, markerTextBrush, x - 14.0f, y - markerLength - 12.0f);
                }

                return;
            }

            float ox = (float)originScreen.X;
            float oy = (float)originScreen.Y;

            graphics.DrawLine(markerPen, ox, oy, ox + markerLength, oy);
            graphics.DrawLine(markerPen, ox, oy, ox, oy - markerLength);
            graphics.FillRectangle(
                markerBrush,
                ox - markerSquareSize / 2.0f,
                oy - markerSquareSize / 2.0f,
                markerSquareSize,
                markerSquareSize);

            if (_settings.ShowAxisLabels)
            {
                graphics.DrawString("X", _axisFont, markerTextBrush, ox + markerLength + 4.0f, oy - 10.0f);
                graphics.DrawString("Y", _axisFont, markerTextBrush, ox - 14.0f, oy - markerLength - 12.0f);
            }
        }

        /// <summary>
        /// Checks whether a numeric value is finite and within drawable coordinate limits.
        /// </summary>
        /// <param name="value">Coordinate component to validate.</param>
        /// <returns>
        /// <see langword="true"/> when the value is finite and inside renderer limits; otherwise <see langword="false"/>.
        /// </returns>
        private static bool IsValid(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > -1e6 && value < 1e6;
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
        Action? Release) : IDisposable
    {
        public void Dispose()
        {
            Release?.Invoke();
        }
    }
}
