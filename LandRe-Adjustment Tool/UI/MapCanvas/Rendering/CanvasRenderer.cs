using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Handles all rendering operations for the drawing canvas.
    /// 
    /// FIXES APPLIED:
    /// 1. Fixed horizontal grid lines rendering issue (Y-coordinate min/max swap)
    /// 2. Removed all snapping behavior for smooth drawing
    /// 
    /// KEY OPTIMIZATION: Object Pooling
    /// - Reuses Pen, Brush, Font objects across frames
    /// - Avoids garbage collection pressure
    /// - Industry standard for high-performance graphics
    /// </summary>
    public class CanvasRenderer : IDisposable
    {
        #region Pooled Resources (CRITICAL FOR PERFORMANCE)

        // Grid rendering resources
        private Pen _majorGridPen;
        private Pen _minorGridPen;
        private Brush _gridTextBrush;
        private Font _gridFont;

        // UI overlay resources
        private Font _overlayLabelFont;
        private Font _overlayValueFont;
        private Brush _overlayBgBrush;
        private Brush _overlayLabelBrush;
        private Brush _overlayValueBrush;
        private Brush _overlayInfoBrush;
        private Pen _overlayBorderPen;

        // Current theme colors
        private Color _majorGridColor;
        private Color _minorGridColor;
        private Color _backgroundColor;

        #endregion

        #region Grid Settings

        private int _majorGridSize = 500;  // 1 kilometer
        private int _minorGridSize = 100;   // 200 meters

        public int MajorGridSize
        {
            get => _majorGridSize;
            set
            {
                _majorGridSize = value;
                UpdateGridPens();
            }
        }

        public int MinorGridSize
        {
            get => _minorGridSize;
            set
            {
                _minorGridSize = value;
                UpdateGridPens();
            }
        }

        #endregion

        public static double SnapGlyphSize = 8.0;

        public CanvasRenderer()
        {
            InitializeResources();
            SetDarkTheme(); // Default theme
        }

        private void InitializeResources()
        {
            // Grid resources
            _gridFont = new Font("Arial", 8);

            // UI overlay resources
            _overlayLabelFont = new Font("Arial", 9, FontStyle.Bold);
            _overlayValueFont = new Font("Consolas", 10, FontStyle.Regular);
            _overlayBgBrush = new SolidBrush(Color.FromArgb(220, 30, 30, 30));
            _overlayLabelBrush = new SolidBrush(Color.FromArgb(200, 200, 200));
            _overlayValueBrush = new SolidBrush(Color.FromArgb(100, 255, 100));
            _overlayInfoBrush = new SolidBrush(Color.White);
            _overlayBorderPen = new Pen(Color.FromArgb(100, 150, 150, 150), 1);
        }

        private void UpdateGridPens()
        {
            // Dispose old pens
            _majorGridPen?.Dispose();
            _minorGridPen?.Dispose();
            _gridTextBrush?.Dispose();

            // Create new pens with current colors
            _majorGridPen = new Pen(_majorGridColor, 0.25f);
            _minorGridPen = new Pen(_minorGridColor, 0.25f);
            _gridTextBrush = new SolidBrush(Color.Gray); //use gray color for text to ensure visibility on both themes
        }

        #region Theme Management

        public void SetDarkTheme()
        {
            _backgroundColor = Color.FromArgb(34, 41, 51);
            _majorGridColor = Color.FromArgb(20, 255, 255, 255); // was 5
            _minorGridColor = Color.FromArgb(5, 255, 255, 255); // was 2
            UpdateGridPens();
        }

        public void SetLightTheme()
        {
            _backgroundColor = Color.White;
            _majorGridColor = Color.FromArgb(236, 236, 236);
            _minorGridColor = Color.FromArgb(245, 245, 245);
            UpdateGridPens();
        }

        public void SetBackgroundColor(Color color)
        {
            _backgroundColor = color;
        }

        public void SetGridColor(Color color)
        {
            _majorGridColor = Color.FromArgb(110, color.R, color.G, color.B);
            _minorGridColor = Color.FromArgb(45, color.R, color.G, color.B);
            UpdateGridPens();
        }

        public Color BackgroundColor => _backgroundColor;

        #endregion

        #region Main Render Method

        // Ensures the viewport used for shape queries is never less than 1x1 units
        private static RectangleD ClampViewport(RectangleD viewport)
        {
            double minWidth = 1.0;
            double minHeight = 1.0;
            double width = Math.Max(viewport.Width, minWidth);
            double height = Math.Max(viewport.Height, minHeight);
            return new RectangleD(viewport.X, viewport.Y, width, height);
        }

        public void Render(
            Graphics g,
            DrawingEngine engine,
            List<IShape> shapes,
            IShape previewShape,
            PointD? currentMouseWorldPos)
        {
            // Set high-quality rendering modes
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Render grid
            RenderGrid(g, engine);

            // Draw X and Y axis markers (AutoCAD style)
            DrawAxisMarkers(g, engine);

            // Clamp viewport for shape queries to avoid disappearing objects at extreme zoom
            // (This is where you would use ClampViewport if shape queries are done here)
            // Example usage if needed:
            // var clampedViewport = ClampViewport(engine.GetViewportBounds());
            // var visibleShapes = shapeManager.QueryShapesInBound(clampedViewport);

            // Render all shapes (no culling here, culling is done in ShapeManager)
            foreach (var shape in shapes)
            {
                if (shape.IsVisible)
                    shape.Draw(g, engine.WorldToScreen);
            }

            // Render preview (dashed outline while drawing)
            if (previewShape != null)
            {
                previewShape.Draw(g, engine.WorldToScreen, isPreview: true);
            }

            // Render UI overlay
            RenderUIOverlay(g, engine, currentMouseWorldPos);
        }

        // Draw X (red) and Y (green) axis markers at world origin
        private PointD? _lastValidOriginScreen = null;

        // Helper to check if a float is valid for drawing
        private static bool IsValid(float v) => !float.IsNaN(v) && !float.IsInfinity(v) && v > -1e6f && v < 1e6f;
        private static bool IsValidPoint(PointD pt) => IsValid((float)pt.X) && IsValid((float)pt.Y);
        private static bool IsFarEnough(PointD a, PointD b) =>
            Math.Abs(a.X - b.X) > 1.0 || Math.Abs(a.Y - b.Y) > 1.0;

        public void DrawAxisMarkers(Graphics g, DrawingEngine engine)
        {
            // Get world origin in screen coordinates
            PointD originScreen = engine.WorldToScreen(new PointD(0, 0));
            RectangleD viewport = engine.GetViewportBounds();

            // Get viewport pixel bounds
            RectangleF clientRect = g.VisibleClipBounds;

            // World coordinates for axis lines (full length)
            PointD worldTop = new PointD(0, viewport.Bottom); // Y+ is up (screen)
            PointD worldRight = new PointD(viewport.Right, 0);

            // Convert to screen coordinates
            PointD topScreen = engine.WorldToScreen(worldTop);
            PointD rightScreen = engine.WorldToScreen(worldRight);

            // Use grid lineweight for axis lines (very thin)
            float axisLineWeight = 0.3f;
            float markerLineWeight = 0.7f;
            float markerLength = 32f;
            float markerBox = 7f;

            // Colors
            Color xAxisColor = Color.Red;
            Color yAxisColor = Color.LimeGreen;
            Color markerColor = Color.White;

            // Check if origin is inside the viewport (screen)
            bool originInViewport =
                originScreen.X >= clientRect.Left && originScreen.X <= clientRect.Right &&
                originScreen.Y >= clientRect.Top && originScreen.Y <= clientRect.Bottom;

            // If origin is out of view, clamp marker to bottom-left as overlay
            if (!originInViewport)
            {
                // Padding from edge
                float pad = 18f;
                float x = clientRect.Left + pad;
                float y = clientRect.Bottom - pad;

                // Draw L marker (white)
                using (var markerPen = new Pen(markerColor, markerLineWeight))
                {
                    g.DrawLine(markerPen, x, y, x + markerLength, y);
                    g.DrawLine(markerPen, x, y, x, y - markerLength);
                }
                // Draw small white square at marker
                using (var markerBrush = new SolidBrush(markerColor))
                {
                    g.FillRectangle(markerBrush, x - markerBox / 2, y - markerBox / 2, markerBox, markerBox);
                }
                // Draw X and Y labels
                using (var font = new Font("Arial", 9, FontStyle.Regular))
                using (var labelBrush = new SolidBrush(markerColor))
                {
                    g.DrawString("X", font, labelBrush, x + markerLength + 4, y - 10);
                    g.DrawString("Y", font, labelBrush, x - 14, y - markerLength - 12);
                }

                // Draw X axis (origin to right edge)
                if (IsValidPoint(originScreen) && IsValidPoint(rightScreen) && IsFarEnough(originScreen, rightScreen))
                {
                    using (var xPen = new Pen(xAxisColor, axisLineWeight))
                    {
                        g.DrawLine(xPen, (float)originScreen.X, (float)originScreen.Y, (float)rightScreen.X, (float)rightScreen.Y);
                    }
                }
                // Draw Y axis (origin to top edge, which is screen up)
                if (IsValidPoint(originScreen) && IsValidPoint(topScreen) && IsFarEnough(originScreen, topScreen))
                {
                    using (var yPen = new Pen(yAxisColor, axisLineWeight))
                    {
                        g.DrawLine(yPen, (float)originScreen.X, (float)originScreen.Y, (float)topScreen.X, (float)topScreen.Y);
                    }
                }
                return;
            }

            // Draw X axis (origin to right edge)
            if (IsValidPoint(originScreen) && IsValidPoint(rightScreen) && IsFarEnough(originScreen, rightScreen))
            {
                using (var xPen = new Pen(xAxisColor, axisLineWeight))
                {
                    g.DrawLine(xPen, (float)originScreen.X, (float)originScreen.Y, (float)rightScreen.X, (float)rightScreen.Y);
                }
            }
            // Draw Y axis (origin to top edge, which is screen up)
            if (IsValidPoint(originScreen) && IsValidPoint(topScreen) && IsFarEnough(originScreen, topScreen))
            {
                using (var yPen = new Pen(yAxisColor, axisLineWeight))
                {
                    g.DrawLine(yPen, (float)originScreen.X, (float)originScreen.Y, (float)topScreen.X, (float)topScreen.Y);
                }
            }

            // Draw L-shaped marker (white)
            using (var markerPen = new Pen(markerColor, markerLineWeight))
            {
                g.DrawLine(markerPen,
                    (float)originScreen.X,
                    (float)originScreen.Y,
                    (float)(originScreen.X + markerLength),
                    (float)originScreen.Y);

                g.DrawLine(markerPen,
                    (float)originScreen.X,
                    (float)originScreen.Y,
                    (float)originScreen.X,
                    (float)(originScreen.Y - markerLength));
            }

            // Draw small white square at origin
            using (var markerBrush = new SolidBrush(markerColor))
            {
                g.FillRectangle(markerBrush, (float)originScreen.X - markerBox / 2, (float)originScreen.Y - markerBox / 2, markerBox, markerBox);
            }

            // Draw X and Y labels
            using (var font = new Font("Arial", 9, FontStyle.Regular))
            using (var labelBrush = new SolidBrush(markerColor))
            {
                g.DrawString("X", font, labelBrush, (float)(originScreen.X + markerLength + 4), (float)originScreen.Y - 10);
                g.DrawString("Y", font, labelBrush, (float)originScreen.X - 14, (float)(originScreen.Y - markerLength - 12));
            }
        }

        #endregion

        #region Grid Rendering

        /// <summary>
        /// Render adaptive grid with performance optimization.
        /// 
        /// FIX APPLIED: Corrected Y-coordinate handling for horizontal lines
        /// The viewport.Bottom and viewport.Top were being used incorrectly
        /// due to coordinate system transformation (Y-axis flip)
        /// 
        /// ADAPTIVE GRID (AutoCAD-style):
        /// - Grid spacing changes based on zoom level
        /// - Prevents grid from becoming too dense or sparse
        /// - Maintains visual consistency across zoom levels
        /// </summary>
        #region Grid Rendering

        public void RenderGrid(Graphics g, DrawingEngine engine)
        {
            double zoomScale = engine.ZoomScale;
            if (zoomScale < 0.001 || zoomScale > 100000)
                return;

            RectangleD viewport = engine.GetViewportBounds();
            double worldLeft = viewport.Left;
            double worldRight = viewport.Right;
            double worldBottom = Math.Min(viewport.Top, viewport.Bottom);
            double worldTop = Math.Max(viewport.Top, viewport.Bottom);

            // ---------------------- AutoCAD-like adaptive grid spacing (with hysteresis) ----------------------
            const int MAJOR_DIVISIONS = 5;

            // Pixel band — allows zooming more before spacing changes
            const double MIN_MINOR_PIXELS = 10.0;
            const double MAX_MINOR_PIXELS = 32.0;   // Increase this if you want even more zoom before change

            const double MIN_MINOR_WORLD = 1e-9;
            const double MAX_MINOR_WORLD = 1e12;

            double adaptiveMinorSize =
                (_lastAdaptiveMinorSize > 0)
                ? _lastAdaptiveMinorSize
                : SnapToNiceStep(16.0 / zoomScale);

            adaptiveMinorSize = SnapToNiceStep(adaptiveMinorSize);

            double minorGridPixels = adaptiveMinorSize * zoomScale;

            // Zoomed out too much → increase spacing
            while (minorGridPixels < MIN_MINOR_PIXELS && adaptiveMinorSize < MAX_MINOR_WORLD)
            {
                adaptiveMinorSize = NextNiceStep(adaptiveMinorSize);
                minorGridPixels = adaptiveMinorSize * zoomScale;
            }

            // Zoomed in too much → decrease spacing
            while (minorGridPixels > MAX_MINOR_PIXELS && adaptiveMinorSize > MIN_MINOR_WORLD)
            {
                adaptiveMinorSize = PrevNiceStep(adaptiveMinorSize);
                minorGridPixels = adaptiveMinorSize * zoomScale;
            }

            adaptiveMinorSize = Math.Max(MIN_MINOR_WORLD, Math.Min(MAX_MINOR_WORLD, adaptiveMinorSize));

            double adaptiveMajorSize = adaptiveMinorSize * MAJOR_DIVISIONS;

            _lastAdaptiveMinorSize = adaptiveMinorSize;
            // --------------------------------------------------------------------------------------------------

            double worldWidth = worldRight - worldLeft;
            double worldHeight = worldTop - worldBottom;

            int estimatedVerticalLines = Math.Max(1, (int)(worldWidth / adaptiveMinorSize));
            int estimatedHorizontalLines = Math.Max(1, (int)(worldHeight / adaptiveMinorSize));

            const int MAX_LINES_PER_AXIS = 500;
            if (estimatedVerticalLines > MAX_LINES_PER_AXIS ||
                estimatedHorizontalLines > MAX_LINES_PER_AXIS)
            {
                return;
            }

            double startX = Math.Floor(worldLeft / adaptiveMinorSize) * adaptiveMinorSize;
            double endX = Math.Ceiling(worldRight / adaptiveMinorSize) * adaptiveMinorSize;
            double startY = Math.Floor(worldBottom / adaptiveMinorSize) * adaptiveMinorSize;
            double endY = Math.Ceiling(worldTop / adaptiveMajorSize) * adaptiveMajorSize;

            // Extend grid bounds to nearest major grid line outside viewport
            startX = Math.Floor(worldLeft / adaptiveMajorSize) * adaptiveMajorSize;
            endX = Math.Ceiling(worldRight / adaptiveMajorSize) * adaptiveMajorSize;
            startY = Math.Floor(worldBottom / adaptiveMajorSize) * adaptiveMajorSize;
            endY = Math.Ceiling(worldTop / adaptiveMajorSize) * adaptiveMajorSize;

            bool showLabels = zoomScale > 0.00001 && zoomScale < 10000 && estimatedVerticalLines < 500;

            // Vertical lines
            for (double x = startX; x <= endX; x += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(x, adaptiveMajorSize);
                Pen pen = isMajor ? _majorGridPen : _minorGridPen;

                PointD screenStart = engine.WorldToScreen(new PointD(x, worldBottom));
                PointD screenEnd = engine.WorldToScreen(new PointD(x, worldTop));

                if (IsValidPoint(screenStart) && IsValidPoint(screenEnd))
                {
                    g.DrawLine(pen,
                        (float)screenStart.X, (float)screenStart.Y,
                        (float)screenEnd.X, (float)screenEnd.Y);
                }

                if (isMajor && showLabels)
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Far;

                        string label = FormatGridLabel(x, adaptiveMajorSize);
                        g.DrawString(label, _gridFont, _gridTextBrush,
                            (float)screenStart.X, (float)screenStart.Y, sf);
                    }
                }
            }

            // Horizontal lines
            for (double y = startY; y <= endY; y += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(y, adaptiveMajorSize);
                Pen pen = isMajor ? _majorGridPen : _minorGridPen;

                PointD screenStart = engine.WorldToScreen(new PointD(worldLeft, y));
                PointD screenEnd = engine.WorldToScreen(new PointD(worldRight, y));

                if (IsValidPoint(screenStart) && IsValidPoint(screenEnd))
                {
                    g.DrawLine(pen,
                        (float)screenStart.X, (float)screenStart.Y,
                        (float)screenEnd.X, (float)screenEnd.Y);
                }

                if (isMajor && showLabels)
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Near;
                        sf.LineAlignment = StringAlignment.Center;

                        string label = FormatGridLabel(y, adaptiveMajorSize);
                        g.DrawString(label, _gridFont, _gridTextBrush,
                            10, (float)screenStart.Y, sf);
                    }
                }
            }
        }

        // Overload for custom viewport bounds
        public void RenderGrid(Graphics g, DrawingEngine engine, RectangleD bounds)
        {
            double zoomScale = engine.ZoomScale;
            if (zoomScale < 0.002 || zoomScale > 100000)
                return;

            RectangleD viewport = bounds;
            double worldLeft = viewport.Left;
            double worldRight = viewport.Right;
            double worldBottom = Math.Min(viewport.Top, viewport.Bottom);
            double worldTop = Math.Max(viewport.Top, viewport.Bottom);

            // ---------------------- AutoCAD-like adaptive grid spacing (with hysteresis) ----------------------
            const int MAJOR_DIVISIONS = 5;
            const double MIN_MINOR_PIXELS = 10.0;
            const double MAX_MINOR_PIXELS = 32.0;
            const double MIN_MINOR_WORLD = 1e-9;
            const double MAX_MINOR_WORLD = 1e12;

            double adaptiveMinorSize = (_lastAdaptiveMinorSize > 0) ? _lastAdaptiveMinorSize : SnapToNiceStep(16.0 / zoomScale);
            adaptiveMinorSize = SnapToNiceStep(adaptiveMinorSize);
            double minorGridPixels = adaptiveMinorSize * zoomScale;

            while (minorGridPixels < MIN_MINOR_PIXELS && adaptiveMinorSize < MAX_MINOR_WORLD)
            {
                adaptiveMinorSize = NextNiceStep(adaptiveMinorSize);
                minorGridPixels = adaptiveMinorSize * zoomScale;
            }
            while (minorGridPixels > MAX_MINOR_PIXELS && adaptiveMinorSize > MIN_MINOR_WORLD)
            {
                adaptiveMinorSize = PrevNiceStep(adaptiveMinorSize);
                minorGridPixels = adaptiveMinorSize * zoomScale;
            }
            adaptiveMinorSize = Math.Max(MIN_MINOR_WORLD, Math.Min(MAX_MINOR_WORLD, adaptiveMinorSize));
            double adaptiveMajorSize = adaptiveMinorSize * MAJOR_DIVISIONS;
            _lastAdaptiveMinorSize = adaptiveMinorSize;

            double worldWidth = worldRight - worldLeft;
            double worldHeight = worldTop - worldBottom;
            int estimatedVerticalLines = Math.Max(1, (int)(worldWidth / adaptiveMinorSize));
            int estimatedHorizontalLines = Math.Max(1, (int)(worldHeight / adaptiveMinorSize));
            const int MAX_LINES_PER_AXIS = 500;
            if (estimatedVerticalLines > MAX_LINES_PER_AXIS || estimatedHorizontalLines > MAX_LINES_PER_AXIS)
                return;

            double startX = Math.Floor(worldLeft / adaptiveMinorSize) * adaptiveMinorSize;
            double endX = Math.Ceiling(worldRight / adaptiveMinorSize) * adaptiveMinorSize;
            double startY = Math.Floor(worldBottom / adaptiveMinorSize) * adaptiveMinorSize;
            double endY = Math.Ceiling(worldTop / adaptiveMinorSize) * adaptiveMinorSize;

            // Extend grid bounds to nearest major grid line outside viewport
            startX = Math.Floor(worldLeft / adaptiveMajorSize) * adaptiveMajorSize;
            endX = Math.Ceiling(worldRight / adaptiveMajorSize) * adaptiveMajorSize;
            startY = Math.Floor(worldBottom / adaptiveMajorSize) * adaptiveMajorSize;
            endY = Math.Ceiling(worldTop / adaptiveMajorSize) * adaptiveMajorSize;

            bool showLabels = zoomScale > 0.00001 && zoomScale < 10000 && estimatedVerticalLines < 500;

            // Vertical lines
            for (double x = startX; x <= endX; x += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(x, adaptiveMajorSize);
                Pen pen = isMajor ? _majorGridPen : _minorGridPen;
                PointD screenStart = engine.WorldToScreen(new PointD(x, worldBottom));
                PointD screenEnd = engine.WorldToScreen(new PointD(x, worldTop));
                if (IsValidPoint(screenStart) && IsValidPoint(screenEnd))
                {
                    g.DrawLine(pen, (float)screenStart.X, (float)screenStart.Y, (float)screenEnd.X, (float)screenEnd.Y);
                }
                if (isMajor && showLabels)
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Far;
                        string label = FormatGridLabel(x, adaptiveMajorSize);
                        g.DrawString(label, _gridFont, _gridTextBrush, (float)screenStart.X, (float)screenStart.Y, sf);
                    }
                }
            }
            // Horizontal lines
            for (double y = startY; y <= endY; y += adaptiveMinorSize)
            {
                bool isMajor = IsMajorLine(y, adaptiveMajorSize);
                Pen pen = isMajor ? _majorGridPen : _minorGridPen;
                PointD screenStart = engine.WorldToScreen(new PointD(worldLeft, y));
                PointD screenEnd = engine.WorldToScreen(new PointD(worldRight, y));
                if (IsValidPoint(screenStart) && IsValidPoint(screenEnd))
                {
                    g.DrawLine(pen, (float)screenStart.X, (float)screenStart.Y, (float)screenEnd.X, (float)screenEnd.Y);
                }
                if (isMajor && showLabels)
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Near;
                        sf.LineAlignment = StringAlignment.Center;
                        string label = FormatGridLabel(y, adaptiveMajorSize);
                        g.DrawString(label, _gridFont, _gridTextBrush, 10, (float)screenStart.Y, sf);
                    }
                }
            }
        }

        // =================== HELPERS ===================

        private double _lastAdaptiveMinorSize = 0;

        private static double SnapToNiceStep(double value)
        {
            if (value <= 0) return 1.0;

            double exp = Math.Floor(Math.Log10(value));
            double pow10 = Math.Pow(10, exp);
            double f = value / pow10;

            double niceF;
            if (f <= 1) niceF = 1;
            else if (f <= 2) niceF = 2;
            else if (f <= 5) niceF = 5;
            else niceF = 10;

            return niceF * pow10;
        }

        private static double NextNiceStep(double current)
        {
            double exp = Math.Floor(Math.Log10(current));
            double pow10 = Math.Pow(10, exp);
            double f = current / pow10;

            if (f < 1) return 1 * pow10;
            if (f < 2) return 2 * pow10;
            if (f < 5) return 5 * pow10;
            return 1 * Math.Pow(10, exp + 1);
        }

        private static double PrevNiceStep(double current)
        {
            double exp = Math.Floor(Math.Log10(current));
            double pow10 = Math.Pow(10, exp);
            double f = current / pow10;

            if (f > 5) return 5 * pow10;
            if (f > 2) return 2 * pow10;
            if (f > 1) return 1 * pow10;

            return 5 * Math.Pow(10, exp - 1);
        }

        private static bool IsMajorLine(double value, double majorStep)
        {
            double k = value / majorStep;
            return Math.Abs(k - Math.Round(k)) < 1e-6;
        }

        private string FormatGridLabel(double value, double majorStep)
        {
            double step = Math.Abs(majorStep);

            if (step >= 1000) return $"{value:F0}";
            if (step >= 1) return $"{value:F1}";
            if (step >= 0.01) return $"{value:F2}";
            return $"{value:F3}";
        }

        #endregion

        #region UI Overlay Rendering

        public void RenderUIOverlay(Graphics g, DrawingEngine engine, PointD? mousePos)
        {
            RenderUIOverlay(g, engine, mousePos, false, 0, 0);
        }

        /// <summary>
        /// Render UI overlay with optional snap status information.
        /// </summary>
        public void RenderUIOverlay(Graphics g, DrawingEngine engine, PointD? mousePos,
            bool snapEnabled, int visibleShapes, int snapCandidates)
        {
            if (!mousePos.HasValue) return;

            string coordsLabel = "Coordinates:";
            string xCoord = $"X: {mousePos.Value.X:F4}";
            string yCoord = $"Y: {mousePos.Value.Y:F4}";
            string zoomText = engine.GetZoomPercentage();
            string scaleText = engine.GetScaleString();

            float x = 10;
            float y = 10;
            float lineHeight = 20;
            float padding = 10;

            SizeF xSize = g.MeasureString(xCoord, _overlayValueFont);
            SizeF ySize = g.MeasureString(yCoord, _overlayValueFont);
            SizeF zoomSize = g.MeasureString(zoomText, _overlayValueFont);
            SizeF scaleSize = g.MeasureString(scaleText, _overlayValueFont);

            float maxWidth = Math.Max(Math.Max(Math.Max(xSize.Width, ySize.Width), zoomSize.Width), scaleSize.Width);

            // Add snap status if enabled
            float snapLineHeight = 0;
            SizeF snapStatusSize = SizeF.Empty;
            if (snapEnabled)
            {
                string snapStatus = $"Snap: {visibleShapes} shapes, {snapCandidates} pts";
                snapStatusSize = g.MeasureString(snapStatus, _overlayValueFont);
                maxWidth = Math.Max(maxWidth, snapStatusSize.Width);
                snapLineHeight = lineHeight;
            }

            float boxWidth = maxWidth + (padding * 2);
            float boxHeight = lineHeight * 5.5f + snapLineHeight;

            // Draw background box
            g.FillRectangle(_overlayBgBrush, x, y, boxWidth, boxHeight);
            g.DrawRectangle(_overlayBorderPen, x, y, boxWidth, boxHeight);

            float textX = x + padding;
            float textY = y + padding;

            g.DrawString(coordsLabel, _overlayLabelFont, _overlayLabelBrush, textX, textY);
            textY += lineHeight;
            g.DrawString(xCoord, _overlayValueFont, _overlayValueBrush, textX, textY);
            textY += lineHeight;
            g.DrawString(yCoord, _overlayValueFont, _overlayValueBrush, textX, textY);
            textY += lineHeight + 5;
            g.DrawString(zoomText, _overlayValueFont, _overlayInfoBrush, textX, textY);
            textY += lineHeight;
            g.DrawString(scaleText, _overlayValueFont, _overlayInfoBrush, textX, textY);

            // Draw snap status if enabled
            if (snapEnabled)
            {
                textY += lineHeight;
                string snapStatus = $"Snap: {visibleShapes} shapes, {snapCandidates} pts";
                g.DrawString(snapStatus, _overlayValueFont, new SolidBrush(Color.LightGreen), textX, textY);
            }
        }

        #endregion

        #region Snap Status Rendering

        /// <summary>
        /// Render warning when snap is temporarily disabled due to complexity.
        /// Shows in bottom-right corner with yellow warning color.
        /// </summary>
        public void RenderSnapDisabledWarning(Graphics g, Size canvasSize, int visibleShapes, int maxShapes, int snapCandidates, int maxCandidates)
        {
            string warningText;
            if (visibleShapes > maxShapes)
            {
                warningText = $"⚠ Snap disabled: {visibleShapes} shapes (max: {maxShapes})";
            }
            else if (snapCandidates > maxCandidates)
            {
                warningText = $"⚠ Snap disabled: {snapCandidates} candidates (max: {maxCandidates})";
            }
            else
            {
                return; // No warning needed
            }

            using (var font = new Font("Arial", 10, FontStyle.Bold))
            using (var warningBrush = new SolidBrush(Color.FromArgb(255, 255, 200)))
            using (var bgBrush = new SolidBrush(Color.FromArgb(200, 80, 60, 0)))
            using (var borderPen = new Pen(Color.FromArgb(255, 200, 150, 0), 2f))
            {
                SizeF textSize = g.MeasureString(warningText, font);
                float padding = 8;
                float boxWidth = textSize.Width + padding * 2;
                float boxHeight = textSize.Height + padding * 2;

                float x = canvasSize.Width - boxWidth - 10;
                float y = canvasSize.Height - boxHeight - 10;

                // Draw warning box
                g.FillRectangle(bgBrush, x, y, boxWidth, boxHeight);
                g.DrawRectangle(borderPen, x, y, boxWidth, boxHeight);
                g.DrawString(warningText, font, warningBrush, x + padding, y + padding);
            }
        }

        #endregion

        #region Snap Glyph Rendering

        /// <summary>
        /// Render snap glyphs with PIXEL-PERFECT alignment.
        /// 
        /// KEY FIX: Round coordinates to nearest pixel BEFORE drawing
        /// to ensure snap glyph aligns exactly with rendered geometry.
        /// This prevents the 1-2 pixel offset visible at high zoom levels.
        /// </summary>
        public void RenderSnapGlyph(Graphics g, DrawingEngine engine, SnapPoint snapPoint)
        {
            if (snapPoint == null) return;

            var screen = engine.WorldToScreen(snapPoint.Position);

            // CRITICAL FIX: Round to nearest pixel for pixel-perfect alignment
            // This ensures snap glyph renders at same pixel as the actual geometry
            float x = (float)Math.Round(screen.X);
            float y = (float)Math.Round(screen.Y);
            float r = (float)CanvasRenderer.SnapGlyphSize;

            switch (snapPoint.Type)
            {
                case SnapType.Endpoint:
                    using (var pen = new Pen(Color.Lime, 2f))
                    {
                        g.DrawRectangle(pen, x - r, y - r, r * 2, r * 2);
                    }
                    break;
                case SnapType.Midpoint:
                    using (var pen = new Pen(Color.Lime, 2f))
                    {
                        var p1 = new System.Drawing.PointF(x, y - r);
                        var p2 = new System.Drawing.PointF(x - r, y + r);
                        var p3 = new System.Drawing.PointF(x + r, y + r);
                        g.DrawPolygon(pen, new[] { p1, p2, p3 });
                    }
                    break;
                case SnapType.Center:
                    using (var pen = new Pen(Color.Lime, 2f))
                    {
                        g.DrawLine(pen, x - r, y, x + r, y);
                        g.DrawLine(pen, x, y - r, x, y + r);
                    }
                    break;
                case SnapType.Quadrant:
                    using (var pen = new Pen(Color.Lime, 2f))
                    {
                        g.DrawEllipse(pen, x - r, y - r, r * 2, r * 2);
                    }
                    break;
                case SnapType.Intersection:
                    using (var pen = new Pen(Color.Lime, 2f))
                    {
                        g.DrawLine(pen, x - r, y - r, x + r, y + r);
                        g.DrawLine(pen, x - r, y + r, x + r, y - r);
                    }
                    break;
                case SnapType.Perpendicular:
                    // AutoCAD-style perpendicular glyph:
                    // A small right-angle symbol (corner square) at the foot point.
                    // The horizontal bar sits on the snap point, the vertical bar
                    // rises from it — universally recognised as the ⊥ symbol.
                    using (var pen = new Pen(Color.Cyan, 2f))
                    {
                        float half = r * 0.75f;
                        // Horizontal base of the right-angle symbol
                        g.DrawLine(pen, x - half, y + half, x + half, y + half);
                        // Vertical stem rising from centre of base
                        g.DrawLine(pen, x, y + half, x, y - half);
                        // Small horizontal tick at the foot to mark the 90° corner
                        g.DrawLine(pen, x, y + half * 0.5f, x + half * 0.5f, y + half * 0.5f);
                    }
                    break;
                default:
                    using (var pen = new Pen(Color.Lime, 2f))
                    {
                        g.DrawEllipse(pen, x - r / 2, y - r / 2, r, r);
                    }
                    break;
            }
        }

        #endregion

        #region Dispose Pattern

        public void Dispose()
        {
            // Dispose all pooled resources
            _majorGridPen?.Dispose();
            _minorGridPen?.Dispose();
            _gridTextBrush?.Dispose();
            _gridFont?.Dispose();

            _overlayLabelFont?.Dispose();
            _overlayValueFont?.Dispose();
            _overlayBgBrush?.Dispose();
            _overlayLabelBrush?.Dispose();
            _overlayValueBrush?.Dispose();
            _overlayInfoBrush?.Dispose();
            _overlayBorderPen?.Dispose();
        }
        #endregion
    }
}

public static class PointDExtensions
{
    public static PointF ToPointF(this PointD pt)
    {
        return new PointF((float)pt.X, (float)pt.Y);
    }
}
#endregion
