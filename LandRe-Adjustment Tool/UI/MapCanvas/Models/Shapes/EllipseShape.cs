using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    /// <summary>
    /// Represents an ellipse/oval shape.
    /// Less common in parcels but useful for curved features
    /// </summary>
    public class EllipseShape : Shape
    {
        public PointD Start { get; set; }
        public PointD End { get; set; }

        public EllipseShape(PointD start, PointD end) : base()
        {
            Start = start;
            End = end;
        }

        private EllipseShape(EllipseShape source) : base(source)
        {
            Start = source.Start;
            End = source.End;
        }

        public override RectangleD GetBoundingBox()
        {
            double minX = Math.Min(Start.X, End.X);
            double minY = Math.Min(Start.Y, End.Y);
            double maxX = Math.Max(Start.X, End.X);
            double maxY = Math.Max(Start.Y, End.Y);
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public override void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false)
        {
            if (!IsVisible) return;
            try
            {
                // CRITICAL FIX: Keep in double precision until final cast
                PointD screenStart = worldToScreen(Start);
                PointD screenEnd = worldToScreen(End);

                // PIXEL-PERFECT FIX: Round to nearest pixel
                float x1 = (float)Math.Round(screenStart.X);
                float y1 = (float)Math.Round(screenStart.Y);
                float x2 = (float)Math.Round(screenEnd.X);
                float y2 = (float)Math.Round(screenEnd.Y);

                // Calculate dimensions with rounded coordinates
                double x = Math.Min(x1, x2);
                double y = Math.Min(y1, y2);
                double width = Math.Abs(x2 - x1);
                double height = Math.Abs(y2 - y1);

                // Validate with double precision
                if (width <= 0 || height <= 0)
                    return;

                Color drawColor = isPreview ? Color.LightGray :
                                 IsSelected ? Color.Yellow : BorderColor;

                float penWidth = IsSelected ? 2f : 0.25f; // AutoCAD default lineweight

                using (Pen pen = new Pen(drawColor, penWidth))
                {
                    if (isPreview)
                    {
                        pen.DashStyle = DashStyle.Dash;
                    }

                    // Cast to float ONLY when calling Draw methods
                    g.DrawEllipse(pen, (float)x, (float)y, (float)width, (float)height);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error drawing ellipse: {ex.Message}", "Drawing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally log or display error
                // System.Diagnostics.Debug.WriteLine($"EllipseShape.Draw error: {ex.Message}");
            }
        }

        public override bool ContainsPoint(PointD worldPoint, float tolerance)
        {
            double minX = Math.Min(Start.X, End.X);
            double minY = Math.Min(Start.Y, End.Y);
            double maxX = Math.Max(Start.X, End.X);
            double maxY = Math.Max(Start.Y, End.Y);
            double cx = (minX + maxX) / 2.0;
            double cy = (minY + maxY) / 2.0;
            double rx = (maxX - minX) / 2.0;
            double ry = (maxY - minY) / 2.0;
            if (rx == 0 || ry == 0) return false;
            double dx = worldPoint.X - cx;
            double dy = worldPoint.Y - cy;
            return ((dx * dx) / (rx * rx) + (dy * dy) / (ry * ry)) <= 1.0 + tolerance;
        }

        public override IShape Clone()
        {
            return new EllipseShape(this);
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            // Center
            double cx = (Start.X + End.X) / 2;
            double cy = (Start.Y + End.Y) / 2;
            double rx = Math.Abs(End.X - Start.X) / 2;
            double ry = Math.Abs(End.Y - Start.Y) / 2;
            var center = new PointD(cx, cy);
            yield return new SnapPoint(SnapType.Center, center, this);
            // Quadrants (right, top, left, bottom)
            yield return new SnapPoint(SnapType.Quadrant, new PointD(cx + rx, cy), this); // right
            yield return new SnapPoint(SnapType.Quadrant, new PointD(cx, cy + ry), this); // top
            yield return new SnapPoint(SnapType.Quadrant, new PointD(cx - rx, cy), this); // left
            yield return new SnapPoint(SnapType.Quadrant, new PointD(cx, cy - ry), this); // bottom
        }
    }
}