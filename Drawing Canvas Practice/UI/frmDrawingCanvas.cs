using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.DrawingCanvas
{
    public partial class frmDrawingCanvas : Form
    {
        private PointF? startPoint = null;
        private PointF? currentPoint = null;
        private PointF? currentWorldMousePosition = new PointF(0, 0);

        private List<Shape> shapes = new List<Shape>();
        private Stack<List<Shape>> undoStack = new Stack<List<Shape>>();
        private Stack<List<Shape>> redoStack = new Stack<List<Shape>>();

        private Color majorGridColor = Color.FromArgb(10, 255, 255, 255);
        private Color minorGridColor = Color.FromArgb(20, 255, 255, 255);
        private int majorGridsize = 1000;  // 1 kilometer
        private int minorGridsize = 200;   // 200 meters

        private float defaultSnapSize = 0.001f;  // 1mm default
        private float zoomScale = 1.0f;

        private const float MIN_ZOOM = 0.001f;  // Very zoomed out
        private const float MAX_ZOOM = 100000.0f; // 1mm precision
        private const float ZOOM_STEP = 1.4f;

        private RectangleF worldBounds = new RectangleF(2900000, 200000, 400000, 700000);

        private PointF viewOffset = new PointF(0f, 0f);
        private bool isPanning = false;
        private PointF lastMousePosition = new PointF(0, 0);
        private bool panToolActive = false;

        private enum DrawingTool
        {
            Line,
            Rectangle,
            Ellipse,
            Circle
        }
        private DrawingTool currentMode = DrawingTool.Line;

        public frmDrawingCanvas()
        {
            InitializeComponent();

            // CRITICAL: Set these BEFORE any other initialization
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            cbTheme.Items.Clear();
            cbTheme.Items.AddRange(new string[] { "Dark", "Light" });
            cbTheme.SelectedIndex = 0;
            cbDrawingTool.Items.Clear();
            cbDrawingTool.Items.AddRange(new string[] { "Line", "Rectangle", "Ellipse", "Circle" });
            cbDrawingTool.SelectedItem = DrawingTool.Line.ToString();
            panelCanvas.BackColor = Color.FromArgb(34, 41, 51);

            panelCanvas.MouseWheel += panelCanvas_MouseWheel!;
            SetInitialView();
        }

        private void SetInitialView()
        {
            float centerX = 335000f;
            float centerY = 3065000f;
            float viewWidth = 5000f;
            float viewHeight = 5000f;

            float zoomX = panelCanvas.Width / viewWidth;
            float zoomY = panelCanvas.Height / viewHeight;
            zoomScale = Math.Min(zoomX, zoomY) * 0.9f;
            zoomScale = Math.Max(MIN_ZOOM, Math.Min(MAX_ZOOM, zoomScale));

            viewOffset.X = (panelCanvas.Width / 2.0f) - (centerX * zoomScale);
            viewOffset.Y = (panelCanvas.Height / 2.0f) - panelCanvas.Height + (centerY * zoomScale);

            panelCanvas.Invalidate();
        }



        private void panelCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomFactor = e.Delta > 0 ? ZOOM_STEP : (1.0f / ZOOM_STEP);
            ZoomAtPoint(e.Location, zoomFactor);
        }

        private PointF ScreenToWorld(Point screenPoint)
        {
            return ScreenToWorld(new PointF(screenPoint.X, screenPoint.Y));
        }

        private PointF ScreenToWorld(PointF screenPoint)
        {
            float x = screenPoint.X;
            float y = screenPoint.Y;

            x = x - viewOffset.X;
            y = y - viewOffset.Y;
            y = y - panelCanvas.Height;
            y = -y;
            x = x / zoomScale;
            y = y / zoomScale;

            return new PointF(x, y);
        }

        private PointF WorldToScreen(PointF worldPoint)
        {
            float x = worldPoint.X;
            float y = worldPoint.Y;

            x = x * zoomScale;
            y = y * zoomScale;
            y = -y;
            y = y + panelCanvas.Height;
            x = x + viewOffset.X;
            y = y + viewOffset.Y;

            return new PointF(x, y);
        }

        private float ScreenToWorldDistance(float screenDistance)
        {
            return screenDistance / zoomScale;
        }

        private float WorldToScreenDistance(float worldDistance)
        {
            return worldDistance * zoomScale;
        }

        private void DrawGrid(Graphics g, int majorGridsize, int minorGridsize, Color majorColor, Color minorColor)
        {
            // AutoCAD-style adaptive grid with performance optimization

            // Early exit conditions for performance
            if (zoomScale < 0.00001f || zoomScale > 500.0f)
            {
                return; // Grid not useful at extreme zoom levels
            }

            PointF topLeftWorld = ScreenToWorld(new Point(0, 0));
            PointF bottomRightWorld = ScreenToWorld(new Point(panelCanvas.Width, panelCanvas.Height));

            float worldLeft = Math.Min(topLeftWorld.X, bottomRightWorld.X);
            float worldRight = Math.Max(topLeftWorld.X, bottomRightWorld.X);
            float worldBottom = Math.Min(topLeftWorld.Y, bottomRightWorld.Y);
            float worldTop = Math.Max(topLeftWorld.Y, bottomRightWorld.Y);

            // Calculate current grid size in pixels
            float minorGridPixels = minorGridsize * zoomScale;

            // Adaptive grid scaling - AutoCAD style
            int adaptiveMinorSize = minorGridsize;
            int adaptiveMajorSize = majorGridsize;

            // Scale DOWN when zoomed OUT (grid too dense)
            while (minorGridPixels < 20 && adaptiveMinorSize < 1000000)
            {
                adaptiveMinorSize *= 5;
                adaptiveMajorSize *= 5;
                minorGridPixels = adaptiveMinorSize * zoomScale;
            }

            // Scale UP when zoomed IN (grid too sparse)
            while (minorGridPixels > 150 && adaptiveMinorSize > 0.001f)
            {
                adaptiveMinorSize /= 5;
                adaptiveMajorSize /= 5;
                minorGridPixels = adaptiveMinorSize * zoomScale;
            }

            // Performance check: estimate line count
            float worldWidth = worldRight - worldLeft;
            float worldHeight = worldTop - worldBottom;

            int estimatedVerticalLines = Math.Max(1, (int)(worldWidth / adaptiveMinorSize));
            int estimatedHorizontalLines = Math.Max(1, (int)(worldHeight / adaptiveMinorSize));

            // Limit to prevent performance issues
            const int MAX_LINES_PER_AXIS = 500;
            if (estimatedVerticalLines > MAX_LINES_PER_AXIS || estimatedHorizontalLines > MAX_LINES_PER_AXIS)
            {
                return; // Too many lines would cause lag
            }

            // Draw the grid
            using (Pen majorPen = new Pen(majorColor, 1.5f))
            using (Pen minorPen = new Pen(minorColor, 0.8f))
            using (Brush textBrush = new SolidBrush(majorColor))
            {
                // Calculate grid bounds
                float startX = (float)(Math.Floor(worldLeft / adaptiveMinorSize) * adaptiveMinorSize);
                float endX = (float)(Math.Ceiling(worldRight / adaptiveMinorSize) * adaptiveMinorSize);
                float startY = (float)(Math.Floor(worldBottom / adaptiveMinorSize) * adaptiveMinorSize);
                float endY = (float)(Math.Ceiling(worldTop / adaptiveMinorSize) * adaptiveMinorSize);

                // Draw vertical lines
                for (float x = startX; x <= endX; x += adaptiveMinorSize)
                {
                    bool isMajor = (Math.Abs(x % adaptiveMajorSize) < 0.01f);
                    Pen pen = isMajor ? majorPen : minorPen;

                    PointF screenStart = WorldToScreen(new PointF(x, worldBottom));
                    PointF screenEnd = WorldToScreen(new PointF(x, worldTop));
                    g.DrawLine(pen, screenStart, screenEnd);

                    // Draw labels on major grid lines only at appropriate zoom
                    if (isMajor && zoomScale > 0.005f && zoomScale < 50.0f && estimatedVerticalLines < 100)
                    {
                        PointF textPos = WorldToScreen(new PointF(x, worldBottom));
                        textPos.Y = panelCanvas.Height - 20; // Bottom of screen

                        using (StringFormat sf = new StringFormat())
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Near;

                            // Format based on size
                            string label = FormatGridLabel(x, adaptiveMajorSize);
                            g.DrawString(label, SystemFonts.DefaultFont, textBrush, textPos, sf);
                        }
                    }
                }

                // Draw horizontal lines
                for (float y = startY; y <= endY; y += adaptiveMinorSize)
                {
                    bool isMajor = (Math.Abs(y % adaptiveMajorSize) < 0.01f);
                    Pen pen = isMajor ? majorPen : minorPen;

                    PointF screenStart = WorldToScreen(new PointF(worldLeft, y));
                    PointF screenEnd = WorldToScreen(new PointF(worldRight, y));
                    g.DrawLine(pen, screenStart, screenEnd);

                    // Draw labels on major grid lines only at appropriate zoom
                    if (isMajor && zoomScale > 0.005f && zoomScale < 50.0f && estimatedHorizontalLines < 100)
                    {
                        PointF textPos = WorldToScreen(new PointF(worldLeft, y));
                        textPos.X = 10; // Left edge of screen

                        using (StringFormat sf = new StringFormat())
                        {
                            sf.Alignment = StringAlignment.Near;
                            sf.LineAlignment = StringAlignment.Center;

                            // Format based on size
                            string label = FormatGridLabel(y, adaptiveMajorSize);
                            g.DrawString(label, SystemFonts.DefaultFont, textBrush, textPos, sf);
                        }
                    }
                }
            }
        }


        // ========================================
        // 4. ADD THIS NEW HELPER METHOD
        // ========================================
        private string FormatGridLabel(float value, int gridSize)
        {
            // Smart formatting based on grid size
            if (gridSize >= 1000)
            {
                return $"{value:F0}"; // No decimals for large grids (e.g., "550000")
            }
            else if (gridSize >= 1)
            {
                return $"{value:F1}"; // 1 decimal for medium grids (e.g., "123.5")
            }
            else if (gridSize >= 0.01)
            {
                return $"{value:F2}"; // 2 decimals for small grids (e.g., "12.34")
            }
            else
            {
                return $"{value:F3}"; // 3 decimals for mm-level grids (e.g., "1.234")
            }
        }


        // ========================================
        // 5. GRID SIZE PROGRESSION REFERENCE
        // ========================================
        /*
        AutoCAD-style adaptive grid sizes at different zoom levels:

        Zoom Out → Zoom In:
        1,000,000m / 200,000m (country level)
        200,000m  / 40,000m   (region level)
        40,000m   / 8,000m    (large area)
        8,000m    / 1,600m    (district)
        1,600m    / 320m      (neighborhood)
        320m      / 64m       (block)
        64m       / 12.8m     (building cluster)
        12.8m     / 2.56m     (large building)
        2.56m     / 0.512m    (room)
        0.512m    / 0.1024m   (furniture)
        0.1024m   / 0.02048m  (detail - ~20mm)
        0.02048m  / 0.004096m (fine detail - ~4mm)
        0.004096m / 0.0008192m (precision - ~0.8mm)

        Each level divides by 5, maintaining the 5:1 major/minor ratio.
        */






        public abstract class Shape
        {
            private Color color = Color.White;
            private Color FillColor = Color.FromArgb(28, 255, 165, 0);
            private Color PreviewColor = Color.LightGray;

            // Draw in screen space - convert world coords to screen coords
            public abstract void Draw(Graphics g, Func<PointF, PointF> worldToScreen, bool isPreview = false);
            public abstract RectangleF GetBoundingBox();
            public abstract Shape Clone();

            public void SetBorderColor(Color color) => this.color = color;
            public Color GetBorderColor() => color;
            public void SetFillColor(Color color) => FillColor = color;
            public Color GetFillColor() => FillColor;
            public void SetPreviewColor(Color color) => PreviewColor = color;
            public Color GetPreviewColor() => PreviewColor;
        }

        public class LineShape : Shape
        {
            public PointF Start { get; set; }
            public PointF End { get; set; }

            public LineShape(PointF start, PointF end)
            {
                Start = start;
                End = end;
            }

            public override RectangleF GetBoundingBox()
            {
                float minX = Math.Min(Start.X, End.X);
                float minY = Math.Min(Start.Y, End.Y);
                float maxX = Math.Max(Start.X, End.X);
                float maxY = Math.Max(Start.Y, End.Y);
                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }

            public override void Draw(Graphics g, Func<PointF, PointF> worldToScreen, bool isPreview = false)
            {
                PointF screenStart = worldToScreen(Start);
                PointF screenEnd = worldToScreen(End);

                float penWidth = isPreview ? 1f : 1f;
                using (Pen pen = new Pen(isPreview ? GetPreviewColor() : GetBorderColor(), penWidth))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    if (isPreview) pen.DashStyle = DashStyle.Dash;
                    g.DrawLine(pen, screenStart, screenEnd);
                }
            }

            public override Shape Clone() => new LineShape(Start, End);
        }

        public class RectangleShape : Shape
        {
            public PointF Start { get; set; }
            public PointF End { get; set; }

            public RectangleShape(PointF start, PointF end)
            {
                Start = start;
                End = end;
            }

            public override RectangleF GetBoundingBox()
            {
                float minX = Math.Min(Start.X, End.X);
                float minY = Math.Min(Start.Y, End.Y);
                float maxX = Math.Max(Start.X, End.X);
                float maxY = Math.Max(Start.Y, End.Y);
                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }

            public override void Draw(Graphics g, Func<PointF, PointF> worldToScreen, bool isPreview = false)
            {
                PointF screenStart = worldToScreen(Start);
                PointF screenEnd = worldToScreen(End);

                float x = Math.Min(screenStart.X, screenEnd.X);
                float y = Math.Min(screenStart.Y, screenEnd.Y);
                float width = Math.Abs(screenStart.X - screenEnd.X);
                float height = Math.Abs(screenStart.Y - screenEnd.Y);

                float penWidth = isPreview ? 1f : 1f;

                using (Brush brush = new SolidBrush(GetFillColor()))
                using (Pen pen = new Pen(isPreview ? GetPreviewColor() : GetBorderColor(), penWidth))
                {
                    pen.LineJoin = LineJoin.Round;
                    if (isPreview) pen.DashStyle = DashStyle.Dash;

                    g.FillRectangle(brush, x, y, width, height);
                    g.DrawRectangle(pen, x, y, width, height);
                }
            }

            public override Shape Clone() => new RectangleShape(Start, End);
        }

        public class CircleShape : Shape
        {
            public PointF start { get; set; }
            public PointF end { get; set; }

            public CircleShape(PointF start, PointF end)
            {
                this.start = start;
                this.end = end;
            }

            public override RectangleF GetBoundingBox()
            {
                float radius = (float)Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
                return new RectangleF(start.X - radius, start.Y - radius, radius * 2, radius * 2);
            }

            public override void Draw(Graphics g, Func<PointF, PointF> worldToScreen, bool isPreview = false)
            {
                // Calculate radius in world space
                float worldRadius = (float)Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));

                // Convert center to screen
                PointF screenCenter = worldToScreen(start);

                // Convert a point at radius distance to screen to get screen radius
                PointF worldRadiusPoint = new PointF(start.X + worldRadius, start.Y);
                PointF screenRadiusPoint = worldToScreen(worldRadiusPoint);
                float screenRadius = Math.Abs(screenRadiusPoint.X - screenCenter.X);

                float x = screenCenter.X - screenRadius;
                float y = screenCenter.Y - screenRadius;
                float diameter = screenRadius * 2;

                float penWidth = isPreview ? 1f : 1f;

                using (Pen pen = new Pen(isPreview ? GetPreviewColor() : GetBorderColor(), penWidth))
                {
                    if (isPreview)
                    {
                        pen.DashStyle = DashStyle.Dash;

                        // Draw radius line
                        PointF screenEnd = worldToScreen(end);
                        g.DrawLine(pen, screenCenter, screenEnd);

                        // Draw radius text
                        PointF midPoint = new PointF(
                            (screenCenter.X + screenEnd.X) / 2,
                            (screenCenter.Y + screenEnd.Y) / 2
                        );

                        using (StringFormat sf = new StringFormat())
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                            using (Brush textBrush = new SolidBrush(GetPreviewColor()))
                            {
                                g.DrawString($"{worldRadius:F3}", new Font("Arial", 10), textBrush, midPoint, sf);
                            }
                        }
                    }

                    // Draw the circle using high-quality arc
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddEllipse(x, y, diameter, diameter);
                        g.DrawPath(pen, path);
                    }
                }
            }

            public override Shape Clone() => new CircleShape(start, end);
        }

        public class EllipseShape : Shape
        {
            public PointF Start { get; set; }
            public PointF End { get; set; }

            public EllipseShape(PointF start, PointF end)
            {
                Start = start;
                End = end;
            }

            public override RectangleF GetBoundingBox()
            {
                float x = Math.Min(Start.X, End.X);
                float y = Math.Min(Start.Y, End.Y);
                float width = Math.Abs(Start.X - End.X);
                float height = Math.Abs(Start.Y - End.Y);
                return new RectangleF(x, y, width, height);
            }

            public override void Draw(Graphics g, Func<PointF, PointF> worldToScreen, bool isPreview = false)
            {
                PointF screenStart = worldToScreen(Start);
                PointF screenEnd = worldToScreen(End);

                float x = Math.Min(screenStart.X, screenEnd.X);
                float y = Math.Min(screenStart.Y, screenEnd.Y);
                float width = Math.Abs(screenStart.X - screenEnd.X);
                float height = Math.Abs(screenStart.Y - screenEnd.Y);

                float penWidth = isPreview ? 1f : 1f;

                using (Brush brush = new SolidBrush(GetFillColor()))
                using (Pen pen = new Pen(isPreview ? GetPreviewColor() : GetBorderColor(), penWidth))
                {
                    if (isPreview) pen.DashStyle = DashStyle.Dash;
                    g.FillEllipse(brush, x, y, width, height);
                    g.DrawEllipse(pen, x, y, width, height);
                }
            }

            public override Shape Clone() => new EllipseShape(Start, End);
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
        }

        private void panelCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                panToolActive = true;
                isPanning = true;
                panelCanvas.Cursor = Cursors.Hand;
            }

            if (panToolActive && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle))
            {
                isPanning = true;
                lastMousePosition = e.Location;
            }
        }

        private void panelCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (panToolActive || e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right) return;

            if (!startPoint.HasValue)
            {
                startPoint = currentWorldMousePosition;  // Use snapped position
                currentPoint = startPoint;
                panelCanvas.Invalidate();
                return;
            }

            currentPoint = currentWorldMousePosition;  // Use snapped position
            SaveStateForUndo();

            Shape newShape = currentMode switch
            {
                DrawingTool.Line => new LineShape(startPoint.Value, currentPoint.Value),
                DrawingTool.Rectangle => new RectangleShape(startPoint.Value, currentPoint.Value),
                DrawingTool.Ellipse => new EllipseShape(startPoint.Value, currentPoint.Value),
                DrawingTool.Circle => new CircleShape(startPoint.Value, currentPoint.Value),
                _ => throw new InvalidOperationException("Unknown drawing tool")
            };

            shapes.Add(newShape);
            startPoint = null;
            currentPoint = null;
            panelCanvas.Invalidate();
        }

        private void panelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            PointF rawWorldPos = ScreenToWorld(e.Location);
            currentWorldMousePosition = rawWorldPos;

            if (isPanning)
            {

                float dx = e.X - lastMousePosition.X;
                float dy = e.Y - lastMousePosition.Y;
                viewOffset.X += dx;
                viewOffset.Y += dy;
                lastMousePosition = e.Location;
                panelCanvas.Invalidate();
                return;
            }

            if (startPoint.HasValue)
            {
                currentPoint = currentWorldMousePosition;
            }

            panelCanvas.Invalidate();
        }

        private void DrawUIOverlay(Graphics g)
        {
            string coordsLabel = "Coordinates:";
            string xCoord = $"X: {currentWorldMousePosition?.X:F3}";
            string yCoord = $"Y: {currentWorldMousePosition?.Y:F3}";
            string zoomText = $"Zoom: {(zoomScale * 100):F1}%";
            string scaleText = $"Scale: 1:{(1 / zoomScale):F2}";

            using (Font labelFont = new Font("Arial", 9, FontStyle.Bold))
            using (Font valueFont = new Font("Consolas", 10, FontStyle.Regular))
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(220, 30, 30, 30)))
            using (Brush labelBrush = new SolidBrush(Color.FromArgb(200, 200, 200)))
            using (Brush valueBrush = new SolidBrush(Color.FromArgb(100, 255, 100)))
            using (Brush infoBrush = new SolidBrush(Color.White))
            {
                float x = 10;
                float y = 10;
                float lineHeight = 20;
                float padding = 10;

                SizeF labelSize = g.MeasureString(coordsLabel, labelFont);
                SizeF xSize = g.MeasureString(xCoord, valueFont);
                SizeF ySize = g.MeasureString(yCoord, valueFont);
                SizeF zoomSize = g.MeasureString(zoomText, valueFont);
                SizeF scaleSize = g.MeasureString(scaleText, valueFont);

                float maxWidth = Math.Max(Math.Max(Math.Max(xSize.Width, ySize.Width), zoomSize.Width), scaleSize.Width);
                float boxWidth = maxWidth + (padding * 2);
                float boxHeight = lineHeight * 5.5f;

                using (Pen borderPen = new Pen(Color.FromArgb(100, 150, 150, 150), 1))
                {
                    g.FillRectangle(bgBrush, x, y, boxWidth, boxHeight);
                    g.DrawRectangle(borderPen, x, y, boxWidth, boxHeight);
                }

                float textX = x + padding;
                float textY = y + padding;

                g.DrawString(coordsLabel, labelFont, labelBrush, textX, textY);
                textY += lineHeight;
                g.DrawString(xCoord, valueFont, valueBrush, textX, textY);
                textY += lineHeight;
                g.DrawString(yCoord, valueFont, valueBrush, textX, textY);
                textY += lineHeight + 5;
                g.DrawString(zoomText, valueFont, infoBrush, textX, textY);
                textY += lineHeight;
                g.DrawString(scaleText, valueFont, infoBrush, textX, textY);
            }
        }

        private void panelCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            //if (isPanning)
            //{
            //    float dx = e.X - lastMousePosition.X;
            //    float dy = e.Y - lastMousePosition.Y;
            //    viewOffset.X += dx;
            //    viewOffset.Y += dy;
            //    panelCanvas.Invalidate();
            //}

            if (e.Button == MouseButtons.Middle)
            {
                panToolActive = false;
                panelCanvas.Cursor = Cursors.Cross;
            }

            isPanning = false;
        }

        private void panelCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // CRITICAL: Use optimal settings for screen-space rendering
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // NO TRANSFORMATION - Draw everything in screen space!
            // Grid needs manual transformation
            DrawGrid(g, majorGridsize, minorGridsize, majorGridColor, minorGridColor);

            // Draw all shapes in screen space with manual coordinate conversion
            foreach (var shape in shapes)
            {
                shape.Draw(g, WorldToScreen);
            }

            // Draw preview
            if (startPoint.HasValue && currentPoint.HasValue)
            {
                Shape previewShape = currentMode switch
                {
                    DrawingTool.Line => new LineShape(startPoint.Value, currentPoint.Value),
                    DrawingTool.Rectangle => new RectangleShape(startPoint.Value, currentPoint.Value),
                    DrawingTool.Ellipse => new EllipseShape(startPoint.Value, currentPoint.Value),
                    DrawingTool.Circle => new CircleShape(startPoint.Value, currentPoint.Value),
                    _ => throw new InvalidOperationException("Unknown drawing tool")
                };
                previewShape.Draw(g, WorldToScreen, isPreview: true);
            }

            DrawUIOverlay(g);
        }

        private void SaveStateForUndo()
        {
            List<Shape> snapshot = new List<Shape>();
            foreach (Shape s in shapes)
            {
                snapshot.Add(s.Clone());
            }
            undoStack.Push(snapshot);
            redoStack.Clear();
        }

        private void Undo()
        {
            if (undoStack.Count > 0)
            {
                List<Shape> currentSnapshot = shapes.Select(s => s.Clone()).ToList();
                redoStack.Push(currentSnapshot);
                shapes = undoStack.Pop();
                panelCanvas.Invalidate();
            }
        }

        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                List<Shape> currentSnapshot = shapes.Select(s => s.Clone()).ToList();
                undoStack.Push(currentSnapshot);
                shapes = redoStack.Pop();
                panelCanvas.Invalidate();
            }
        }

        private void zoomToExtent()
        {
            if (shapes.Count == 0) return;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var shape in shapes)
            {
                var bounds = shape.GetBoundingBox();
                minX = Math.Min(minX, bounds.Left);
                minY = Math.Min(minY, bounds.Top);
                maxX = Math.Max(maxX, bounds.Right);
                maxY = Math.Max(maxY, bounds.Bottom);
            }

            float centerX = (minX + maxX) / 2;
            float centerY = (minY + maxY) / 2;
            float width = maxX - minX;
            float height = maxY - minY;

            if (width < 1) width = 100;
            if (height < 1) height = 100;

            float zoomX = panelCanvas.Width / width;
            float zoomY = panelCanvas.Height / height;
            zoomScale = Math.Min(zoomX, zoomY) * 0.9f;
            zoomScale = Math.Max(MIN_ZOOM, Math.Min(MAX_ZOOM, zoomScale));

            viewOffset.X = (panelCanvas.Width / 2.0f) - (centerX * zoomScale);
            viewOffset.Y = (panelCanvas.Height / 2.0f) - panelCanvas.Height + (centerY * zoomScale);

            panelCanvas.Invalidate();
        }

        private void ZoomAtCenter(float zoomFactor)
        {
            Point centerScreen = new Point(panelCanvas.Width / 2, panelCanvas.Height / 2);
            ZoomAtPoint(centerScreen, zoomFactor);
        }

        private void ZoomAtPoint(Point screenPoint, float zoomFactor)
        {
            float newZoomScale = zoomScale * zoomFactor;
            newZoomScale = Math.Max(MIN_ZOOM, Math.Min(MAX_ZOOM, newZoomScale));
            if (newZoomScale == zoomScale) return;

            PointF worldBeforeZoom = ScreenToWorld(screenPoint);
            zoomScale = newZoomScale;
            PointF newScreenPoint = WorldToScreen(worldBeforeZoom);

            viewOffset.X += (screenPoint.X - newScreenPoint.X);
            viewOffset.Y += (screenPoint.Y - newScreenPoint.Y);

            panelCanvas.Invalidate();
        }

        private void cbDrawingTool_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentMode = cbDrawingTool.SelectedIndex switch
            {
                0 => DrawingTool.Line,
                1 => DrawingTool.Rectangle,
                2 => DrawingTool.Ellipse,
                3 => DrawingTool.Circle,
                _ => DrawingTool.Line
            };
        }

        private void cbTheme_Click(object sender, EventArgs e) { }

        private void cbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTheme.SelectedIndex == 0)
            {
                panelCanvas.BackColor = Color.FromArgb(34, 41, 51);
                majorGridColor = Color.FromArgb(25, 255, 255, 255);
                minorGridColor = Color.FromArgb(10, 255, 255, 255);
            }
            else
            {
                panelCanvas.BackColor = Color.White;
                majorGridColor = Color.FromArgb(236, 236, 236);
                minorGridColor = Color.FromArgb(245, 245, 245);
            }
            panelCanvas.Invalidate();
        }

        private void frmDrawingCanvas_Load(object sender, EventArgs e) { }
        private void buttonRefresh_Click(object sender, EventArgs e) => ClearDrawingState();

        private void ClearDrawingState()
        {
            shapes.Clear();
            startPoint = null;
            currentPoint = null;
            panelCanvas.Invalidate();
        }

        private void btnUndo_Click(object sender, EventArgs e) => Undo();
        private void btnRedo(object sender, EventArgs e) => Redo();

        private void toolLine_Click(object sender, EventArgs e)
        {
            panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
            currentMode = DrawingTool.Line;
            cbDrawingTool.SelectedItem = DrawingTool.Line.ToString();
        }

        private void toolRectangle_Click(object sender, EventArgs e)
        {
            panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
            currentMode = DrawingTool.Rectangle;
            cbDrawingTool.SelectedItem = DrawingTool.Rectangle.ToString();
        }

        private void toolCircle_Click(object sender, EventArgs e)
        {
            panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
            currentMode = DrawingTool.Circle;
            cbDrawingTool.SelectedItem = DrawingTool.Circle.ToString();
        }

        private void toolEllipse_Click(object sender, EventArgs e)
        {
            panToolActive = false;
            panelCanvas.Cursor = Cursors.Cross;
            currentMode = DrawingTool.Ellipse;
            cbDrawingTool.SelectedItem = DrawingTool.Ellipse.ToString();
        }

        private void btnPan_Click(object sender, EventArgs e)
        {
            panToolActive = !panToolActive;
            panelCanvas.Cursor = panToolActive ? Cursors.Hand : Cursors.Cross;
        }

        private void btnZoomIn(object sender, EventArgs e) => ZoomAtCenter(ZOOM_STEP);
        private void btnZoomOut_Click(object sender, EventArgs e) => ZoomAtCenter(1.0f / ZOOM_STEP);
        private void toolStripButton5_Click(object sender, EventArgs e) => zoomToExtent();
        private void panelCanvas_MouseEnter(object sender, EventArgs e) => panelCanvas.Focus();
        private void panelCanvas_MouseLeave(object sender, EventArgs e) => panelCanvas.Invalidate();

        private void TestPerformance()
        {
            var random = new Random();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Draw 1000 random rectangles
            for (int i = 0; i < 1000; i++)
            {
                float x1 = random.Next(330000, 340000);
                float y1 = random.Next(3060000, 3070000);
                float x2 = x1 + random.Next(100, 500);
                float y2 = y1 + random.Next(100, 500);

                var shape = new RectangleShape(new PointF(x1, y1), new PointF(x2, y2));

                // OLD VERSION
                shapes.Add(shape);

            }

            stopwatch.Stop();
            _ = MessageBox.Show($"Added 1000 shapes in {stopwatch.ElapsedMilliseconds}ms");

            panelCanvas.Invalidate();
        }

        private void btnLoadShapes_Click(object sender, EventArgs e)
        {
            TestPerformance();
        }
    }
}