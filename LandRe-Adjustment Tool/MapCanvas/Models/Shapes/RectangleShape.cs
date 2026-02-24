using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.DrawingCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.DrawingCanvas.Models.Shapes
{
    /// <summary>
    /// Represents a rectangle/parcel in world coordinates.
    /// CRITICAL for land replotting - most parcels are rectangular
    /// </summary>
    public class RectangleShape : Shape, ISnapProvider
    {
        public PointD Start { get; set; }
        public PointD End { get; set; }

        public RectangleShape(PointD start, PointD end) : base()
        {
            Start = start;
            End = end;
        }

        private RectangleShape(RectangleShape source) : base(source)
        {
            Start = source.Start;
            End = source.End;
        }

        public RectangleD GetBoundingBoxD()
        {
            double minX = Math.Min(Start.X, End.X);
            double minY = Math.Min(Start.Y, End.Y);
            double maxX = Math.Max(Start.X, End.X);
            double maxY = Math.Max(Start.Y, End.Y);
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public override RectangleD GetBoundingBox()
        {
            return GetBoundingBoxD();
        }

        public void Draw(Graphics g, Func<PointD, PointF> worldToScreen, bool isPreview = false)
        {
            if (!IsVisible) return;

            PointF screenStart = worldToScreen(Start);
            PointF screenEnd = worldToScreen(End);

            float x = Math.Min(screenStart.X, screenEnd.X);
            float y = Math.Min(screenStart.Y, screenEnd.Y);
            float width = Math.Abs(screenStart.X - screenEnd.X);
            float height = Math.Abs(screenStart.Y - screenEnd.Y);

            Color drawColor = isPreview ? Color.LightGray : IsSelected ? Color.Yellow : BorderColor;
            float penWidth = IsSelected ? 2f : 0.25f; // AutoCAD default lineweight

            using (Pen pen = new Pen(drawColor, penWidth))
            {
                pen.LineJoin = LineJoin.Round;
                if (isPreview)
                {
                    pen.DashStyle = DashStyle.Dash;
                }
                g.DrawRectangle(pen, x, y, width, height);
            }
        }

        public override void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false)
        {
            if (!IsVisible) return;
            try
            {
                // CRITICAL FIX: Keep in double precision until final cast
                PointD screenStartD = worldToScreen(Start);
                PointD screenEndD = worldToScreen(End);

                // PIXEL-PERFECT FIX: Round to nearest pixel for consistent rendering
                float x1 = (float)Math.Round(screenStartD.X);
                float y1 = (float)Math.Round(screenStartD.Y);
                float x2 = (float)Math.Round(screenEndD.X);
                float y2 = (float)Math.Round(screenEndD.Y);

                // Calculate bounds with rounded coordinates
                double x = Math.Min(x1, x2);
                double y = Math.Min(y1, y2);
                double width = Math.Abs(x2 - x1);
                double height = Math.Abs(y2 - y1);

                // Validate with double precision
                if (width <= 0 || height <= 0)
                    return;

                Color drawColor = isPreview ? Color.LightGray : IsSelected ? Color.Yellow : BorderColor;
                float penWidth = IsSelected ? 2f : 0.25f; // AutoCAD default lineweight

                using (Pen pen = new Pen(drawColor, penWidth))
                {
                    pen.LineJoin = LineJoin.Round;
                    if (isPreview)
                    {
                        pen.DashStyle = DashStyle.Dash;
                    }

                    // Cast to float when calling DrawRectangle
                    g.DrawRectangle(pen, (float)x, (float)y, (float)width, (float)height);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Graphics error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool ContainsPoint(PointD worldPoint, double tolerance)
        {
            var box = GetBoundingBoxD();
            return worldPoint.X >= box.X - tolerance && worldPoint.X <= box.X + box.Width + tolerance &&
                   worldPoint.Y >= box.Y - tolerance && worldPoint.Y <= box.Y + box.Height + tolerance;
        }

        public override bool ContainsPoint(PointD worldPoint, float tolerance)
        {
            return ContainsPoint(new PointD(worldPoint.X, worldPoint.Y), tolerance);
        }

        public override IShape Clone()
        {
            return new RectangleShape(this);
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            // Four corners
            yield return new SnapPoint(SnapType.Endpoint, Start, this);
            yield return new SnapPoint(SnapType.Endpoint, new PointD(Start.X, End.Y), this);
            yield return new SnapPoint(SnapType.Endpoint, new PointD(End.X, Start.Y), this);
            yield return new SnapPoint(SnapType.Endpoint, End, this);
            // Midpoints of edges
            yield return new SnapPoint(SnapType.Midpoint, new PointD((Start.X + End.X) / 2, Start.Y), this);
            yield return new SnapPoint(SnapType.Midpoint, new PointD((Start.X + End.X) / 2, End.Y), this);
            yield return new SnapPoint(SnapType.Midpoint, new PointD(Start.X, (Start.Y + End.Y) / 2), this);
            yield return new SnapPoint(SnapType.Midpoint, new PointD(End.X, (Start.Y + End.Y) / 2), this);
            // Center
            yield return new SnapPoint(SnapType.Center, new PointD((Start.X + End.X) / 2, (Start.Y + End.Y) / 2), this);
        }
    }
}