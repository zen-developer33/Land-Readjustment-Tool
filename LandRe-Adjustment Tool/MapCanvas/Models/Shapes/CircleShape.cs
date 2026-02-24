using Land_Readjustment_Tool.DrawingCanvas.Models.Snapping;
using NetTopologySuite.Utilities;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.DrawingCanvas.Models.Shapes
{
    /// <summary>
    /// Represents a circle defined by center point and a point on the circumference.
    /// Used for radial measurements, roundabouts, etc.
    /// </summary>
    public class CircleShape : Shape, ISnapProvider
    {
        public PointD Center { get; set; }
        public PointD RadiusPoint { get; set; }

        public CircleShape(PointD center, PointD radiusPoint) 
        {
            Center = center;
            RadiusPoint = radiusPoint;
        }

        private CircleShape(CircleShape source) : base(source)
        {
            Center = source.Center;
            RadiusPoint = source.RadiusPoint;
        }

        public CircleShape(Shape source) : base(source)
        {
            if (source is CircleShape circle)
            {
                Center = circle.Center;
                RadiusPoint = circle.RadiusPoint;
            }
            else
            {
                Center = new PointD(0, 0);
                RadiusPoint = new PointD(1, 0);
            }
        }

        /// <summary>
        /// Calculate radius in world units
        /// </summary>
        public double GetRadius()
        {
            return Distance(Center, RadiusPoint);
        }

        public override RectangleD GetBoundingBox()
        {
            double radius = GetRadius();
            return new RectangleD(Center.X - radius, Center.Y - radius, radius * 2, radius * 2);
        }
        public override void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false)
        {
            if (!IsVisible) return; 

            try
            {
                PointD centerScreenD = worldToScreen(Center);
                PointD edgeScreenD = worldToScreen(RadiusPoint);

                // PIXEL-PERFECT FIX: Round center to pixel for alignment
                float centerX = (float)Math.Round(centerScreenD.X);
                float centerY = (float)Math.Round(centerScreenD.Y);
                float edgeX = (float)Math.Round(edgeScreenD.X);
                float edgeY = (float)Math.Round(edgeScreenD.Y);

                // Calculate screen radius with double precision
                double dx = edgeX - centerX;
                double dy = edgeY - centerY;
                float screenRadius = (float)Math.Sqrt(dx * dx + dy * dy);

                // Validate radius
                if (float.IsNaN(screenRadius) || float.IsInfinity(screenRadius) || screenRadius <= 0)
                    return;

                RectangleF rect = new RectangleF(centerX - screenRadius, centerY - screenRadius, screenRadius * 2, screenRadius * 2);

                // Validate rectangle
                if (rect.Width <= 0 || rect.Height <= 0 || float.IsNaN(rect.Width) || float.IsNaN(rect.Height) || float.IsInfinity(rect.Width) || float.IsInfinity(rect.Height))
                    return;

                using (var pen = new Pen(BorderColor, isPreview ? 1.5f : 0.25f)) // AutoCAD default lineweight
                {
                    if (isPreview)
                    {
                        pen.DashStyle = DashStyle.Dash;
                    }
                    g.DrawEllipse(pen, rect);
                }

                // Draw radius line and value in preview mode
                if (isPreview)
                {
                    using (var radiusPen = new Pen(Color.LightGray, 1.2f) { DashStyle = DashStyle.Dot })
                    {
                        g.DrawLine(radiusPen, centerX, centerY, edgeX, edgeY);
                    }
                    // Draw radius value at midpoint
                    var mid = new PointF((centerX + edgeX) / 2, (centerY + edgeY) / 2);
                    string radiusText = $"{GetRadius():F3}";
                    using (var font = new Font("Arial", 9, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.LightGray))
                    {
                        var textSize = g.MeasureString(radiusText, font);
                        g.DrawString(radiusText, font, brush, mid.X - textSize.Width / 2, mid.Y - textSize.Height / 2);
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log or display error
                // System.Diagnostics.Debug.WriteLine($"CircleShape.Draw error: {ex.Message}");
            }
        }

        public override bool ContainsPoint(PointD worldPoint, float tolerance)
        {
            double dx = worldPoint.X - Center.X;
            double dy = worldPoint.Y - Center.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            return Math.Abs(dist - GetRadius()) <= tolerance;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            // Center
            yield return new SnapPoint(SnapType.Center, Center, this);

            // Quadrant points (0°, 90°, 180°, 270°)
            double radius = GetRadius();
            double[] angles = { 0, Math.PI / 2, Math.PI, 3 * Math.PI / 2 };
            foreach (var angle in angles)
            {
                double x = Center.X + radius * Math.Cos(angle);
                double y = Center.Y + radius * Math.Sin(angle);
                yield return new SnapPoint(SnapType.Quadrant, new PointD(x, y), this);
            }
        }

        public override IShape Clone()
        {
            return new CircleShape(this);
        }

        private static double Distance(PointD a, PointD b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}