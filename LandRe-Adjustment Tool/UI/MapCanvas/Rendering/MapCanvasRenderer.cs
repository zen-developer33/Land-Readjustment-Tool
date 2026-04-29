using System.Drawing;
using System.Drawing.Drawing2D;
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
        private readonly Font _gridFont = new("Arial", 8.0f, FontStyle.Regular);
        private readonly Font _axisFont = new("Arial", 9.0f, FontStyle.Regular);
        private double _lastAdaptiveMinorSize;

        /// <summary>
        /// Renders the full canvas frame for the current viewport state.
        /// </summary>
        /// <param name="graphics">Target graphics surface for drawing.</param>
        /// <param name="engine">Viewport engine providing world/screen transforms.</param>
        /// <param name="settings">Rendering options and theme colors.</param>
        /// <param name="zoomWindowRectangle">
        /// Optional selection rectangle used by zoom-window interaction.
        /// </param>
        public void Render(
            Graphics graphics,
            MapCanvasEngine engine,
            MapCanvasRenderSettings settings,
            IReadOnlyList<RasterRenderLayer> rasterLayers,
            RasterRenderFrame? rasterFrame,
            bool interactiveRaster,
            Rectangle? zoomWindowRectangle)
        {
            ConfigureGraphics(graphics);
            graphics.Clear(settings.BackgroundColor);

            // Basemap/raster content must be drawn before map overlays.
            // Grid, axis markers, and interaction UI are intentionally drawn last.
            if (rasterFrame.HasValue)
            {
                DrawRasterFrame(graphics, rasterFrame.Value);
            }
            else
            {
                RenderRasterLayers(
                    graphics,
                    engine,
                    rasterLayers,
                    interactiveRaster);
            }

            ConfigureGraphics(graphics);

            if (settings.ShowGrid)
            {
                RenderGrid(graphics, engine, settings);
            }

            if (settings.ShowAxisLines || settings.ShowOriginMarker)
            {
                RenderAxisAndOriginMarker(graphics, engine, settings);
            }

            if (zoomWindowRectangle.HasValue)
            {
                RenderZoomWindow(graphics, settings, zoomWindowRectangle.Value);
            }
        }

        private static void RenderRasterLayers(
            Graphics graphics,
            MapCanvasEngine engine,
            IReadOnlyList<RasterRenderLayer> rasterLayers,
            bool interactive)
        {
            if (rasterLayers.Count == 0)
                return;

            RectangleD visibleBounds = engine.GetVisibleWorldBounds();

            foreach (RasterRenderLayer layer in rasterLayers)
            {
                layer.RenderVisible(
                    graphics,
                    engine,
                    visibleBounds,
                    interactive);
            }
        }

        private static void DrawRasterFrame(
            Graphics graphics,
            RasterRenderFrame rasterFrame)
        {
            GraphicsState state = graphics.Save();
            try
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.DrawImage(
                    rasterFrame.Bitmap,
                    rasterFrame.Destination);
            }
            finally
            {
                graphics.Restore(state);
            }
        }

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
        /// <param name="engine">Canvas engine for coordinate transforms.</param>
        /// <param name="settings">Grid color and visibility settings.</param>
        private void RenderGrid(Graphics graphics, MapCanvasEngine engine, MapCanvasRenderSettings settings)
        {
            double zoomScale = engine.ZoomScale;
            if (zoomScale < 0.001 || zoomScale > 100000)
            {
                return;
            }

            RectangleD viewport = engine.GetVisibleWorldBounds();
            double worldLeft = viewport.Left;
            double worldRight = viewport.Right;
            double worldBottom = Math.Min(viewport.Top, viewport.Bottom);
            double worldTop = Math.Max(viewport.Top, viewport.Bottom);

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
            bool showLabels = settings.ShowGridLabels && zoomScale > 0.00001 && zoomScale < 10000 && estimatedVerticalLines < 500;

            using Pen minorPen = new(settings.MinorGridColor, 0.25f);
            using Pen majorPen = new(settings.MajorGridColor, 0.25f);
            using Brush gridTextBrush = new SolidBrush(settings.GridLabelColor);

            for (double x = startX; x <= endX; x += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(x, adaptiveMajorSize);
                Pen pen = isMajor ? majorPen : minorPen;
                PointD screenStart = engine.WorldToScreen(new PointD(x, worldBottom));
                PointD screenEnd = engine.WorldToScreen(new PointD(x, worldTop));

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
                PointD screenStart = engine.WorldToScreen(new PointD(worldLeft, y));
                PointD screenEnd = engine.WorldToScreen(new PointD(worldRight, y));

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
        /// <param name="settings">Style settings used for fill/border colors.</param>
        /// <param name="rectangle">Screen-space zoom-window rectangle.</param>
        private static void RenderZoomWindow(Graphics graphics, MapCanvasRenderSettings settings, Rectangle rectangle)
        {
            if (rectangle.Width < 2 || rectangle.Height < 2)
            {
                return;
            }

            using Brush fill = new SolidBrush(Color.FromArgb(42, settings.AccentColor));
            using Pen border = new(settings.AccentColor, 1.4f) { DashStyle = DashStyle.Dash };
            graphics.FillRectangle(fill, rectangle);
            graphics.DrawRectangle(border, rectangle);
        }

        /// <summary>
        /// Renders world-origin axis lines and the origin marker/labels according to visibility settings.
        /// </summary>
        /// <param name="graphics">Target graphics surface.</param>
        /// <param name="engine">Canvas engine for coordinate transforms.</param>
        /// <param name="settings">Axis and marker style/visibility settings.</param>
        private void RenderAxisAndOriginMarker(
            Graphics graphics,
            MapCanvasEngine engine,
            MapCanvasRenderSettings settings)
        {
            RectangleD viewport = engine.GetVisibleWorldBounds();
            RectangleF clientRect = graphics.VisibleClipBounds;
            PointD originScreen = engine.WorldToScreen(new PointD(0, 0));
            PointD topScreen = engine.WorldToScreen(new PointD(0, viewport.Bottom));
            PointD rightScreen = engine.WorldToScreen(new PointD(viewport.Right, 0));

            if (settings.ShowAxisLines)
            {
                using Pen xAxisPen = new(settings.AxisXColor, settings.AxisLineWidth);
                using Pen yAxisPen = new(settings.AxisYColor, settings.AxisLineWidth);

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

            if (!settings.ShowOriginMarker)
            {
                return;
            }

            bool originInViewport = originScreen.X >= clientRect.Left &&
                                    originScreen.X <= clientRect.Right &&
                                    originScreen.Y >= clientRect.Top &&
                                    originScreen.Y <= clientRect.Bottom;

            float markerLength = settings.AxisMarkerLengthPx;
            float markerSquareSize = settings.AxisMarkerSquareSizePx;

            using Pen markerPen = new(settings.AxisMarkerColor, settings.AxisMarkerLineWidth);
            using Brush markerBrush = new SolidBrush(settings.AxisMarkerColor);
            using Brush markerTextBrush = new SolidBrush(settings.AxisLabelColor);

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

                if (settings.ShowAxisLabels)
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

            if (settings.ShowAxisLabels)
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
        RectangleF Destination);
}
