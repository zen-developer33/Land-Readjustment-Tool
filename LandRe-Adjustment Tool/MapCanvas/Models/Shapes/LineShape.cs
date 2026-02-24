using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.DrawingCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.DrawingCanvas.Models.Shapes
{
    /// <summary>
    /// Represents a line segment in world coordinates.
    /// Used for boundaries, measurement lines, etc.
    /// </summary>
    public class LineShape : Shape, ISnapProvider
    {
        public PointD Start { get; set; }
        public PointD End { get; set; }

        public LineShape(PointD start, PointD end) : base()
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Copy constructor for cloning
        /// </summary>
        private LineShape(LineShape source) : base(source)
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

                // Validate with double precision
                if (double.IsNaN(screenStart.X) || double.IsNaN(screenStart.Y) ||
                    double.IsNaN(screenEnd.X) || double.IsNaN(screenEnd.Y) ||
                    double.IsInfinity(screenStart.X) || double.IsInfinity(screenStart.Y) ||
                    double.IsInfinity(screenEnd.X) || double.IsInfinity(screenEnd.Y))
                    return;

                // PIXEL-PERFECT FIX: Round to nearest pixel for consistent rendering with snap glyphs
                float x1 = (float)Math.Round(screenStart.X);
                float y1 = (float)Math.Round(screenStart.Y);
                float x2 = (float)Math.Round(screenEnd.X);
                float y2 = (float)Math.Round(screenEnd.Y);

                // Select color based on state
                Color drawColor = isPreview ? Color.LightGray :
                                 IsSelected ? Color.Yellow : BorderColor;

                float penWidth = IsSelected ? 2f : 0.25f; // AutoCAD default lineweight

                using (Pen pen = new Pen(drawColor, penWidth))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    if (isPreview)
                    {
                        pen.DashStyle = DashStyle.Dash;
                    }

                    // Draw with pixel-rounded coordinates
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
            catch (Exception ex)
            {
                // Optionally log or display error
                // System.Diagnostics.Debug.WriteLine($"LineShape.Draw error: {ex.Message}");
            }
        }

        public override bool ContainsPoint(PointD worldPoint, float tolerance)
        {
            // Use double-precision for hit test
            double dist = DistanceToSegment(new PointD(worldPoint.X, worldPoint.Y), Start, End);
            return dist <= tolerance;
        }

        public override IShape Clone()
        {
            return new LineShape(this);
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            yield return new SnapPoint(SnapType.Endpoint, Start, this);
            yield return new SnapPoint(SnapType.Endpoint, End, this);
            yield return new SnapPoint(SnapType.Midpoint, new PointD((Start.X + End.X) / 2, (Start.Y + End.Y) / 2), this);
        }

        private static double DistanceToSegment(PointD p, PointD a, PointD b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            if (dx == 0 && dy == 0) return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));
            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;
            return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
        }
    }
}