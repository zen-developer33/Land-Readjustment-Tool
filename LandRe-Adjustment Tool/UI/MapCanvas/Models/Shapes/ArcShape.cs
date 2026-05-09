using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    public sealed class ArcShape : Shape, ISnapProvider
    {
        private const int BoundsSampleCount = 96;

        public PointD Center { get; set; }
        public double Radius { get; set; }
        public double StartAngleRadians { get; set; }
        public double SweepAngleRadians { get; set; }

        public ArcShape(
            PointD center,
            double radius,
            double startAngleRadians,
            double sweepAngleRadians)
        {
            Center = center;
            Radius = radius;
            StartAngleRadians = startAngleRadians;
            SweepAngleRadians = sweepAngleRadians;
        }

        private ArcShape(ArcShape source) : base(source)
        {
            Center = source.Center;
            Radius = source.Radius;
            StartAngleRadians = source.StartAngleRadians;
            SweepAngleRadians = source.SweepAngleRadians;
        }

        public static ArcShape? FromThreePoints(PointD start, PointD through, PointD end)
        {
            double ax = start.X;
            double ay = start.Y;
            double bx = through.X;
            double by = through.Y;
            double cx = end.X;
            double cy = end.Y;

            double denominator = 2.0 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            if (Math.Abs(denominator) < 1e-9)
            {
                return null;
            }

            double aSquared = ax * ax + ay * ay;
            double bSquared = bx * bx + by * by;
            double cSquared = cx * cx + cy * cy;
            double centerX = (aSquared * (by - cy) + bSquared * (cy - ay) + cSquared * (ay - by)) / denominator;
            double centerY = (aSquared * (cx - bx) + bSquared * (ax - cx) + cSquared * (bx - ax)) / denominator;
            PointD center = new(centerX, centerY);
            double radius = Distance(center, start);
            if (radius <= 0.0 || !double.IsFinite(radius))
            {
                return null;
            }

            double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            double throughAngle = Math.Atan2(through.Y - center.Y, through.X - center.X);
            double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);
            double orientation = Cross(start, through, end);
            double sweep = orientation >= 0.0
                ? NormalizePositive(endAngle - startAngle)
                : -NormalizePositive(startAngle - endAngle);

            if (!AngleLiesOnSweep(throughAngle, startAngle, sweep))
            {
                sweep = sweep >= 0.0
                    ? sweep - Math.PI * 2.0
                    : sweep + Math.PI * 2.0;
            }

            return new ArcShape(center, radius, startAngle, sweep);
        }

        public static ArcShape? FromCenterStartEnd(PointD center, PointD start, PointD end)
        {
            double radius = Distance(center, start);
            if (radius <= 0.0 || !double.IsFinite(radius))
            {
                return null;
            }

            double endRadius = Distance(center, end);
            if (endRadius <= 0.0 || !double.IsFinite(endRadius))
            {
                return null;
            }

            double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);
            double sweep = NormalizePositive(endAngle - startAngle);
            if (sweep <= 1e-9)
            {
                return null;
            }

            return new ArcShape(center, radius, startAngle, sweep);
        }

        public PointD StartPoint => PointAt(0.0);
        public PointD EndPoint => PointAt(1.0);
        public PointD MidPoint => PointAt(0.5);

        public PointD PointAt(double fraction)
        {
            double angle = StartAngleRadians + SweepAngleRadians * fraction;
            return new PointD(
                Center.X + Radius * Math.Cos(angle),
                Center.Y + Radius * Math.Sin(angle));
        }

        public IEnumerable<PointD> SamplePoints(int sampleCount)
        {
            int count = Math.Max(2, sampleCount);
            for (int i = 0; i < count; i++)
            {
                yield return PointAt((double)i / (count - 1));
            }
        }

        public override RectangleD GetBoundingBox()
        {
            List<PointD> points = SamplePoints(BoundsSampleCount).ToList();
            double minX = points.Min(point => point.X);
            double maxX = points.Max(point => point.X);
            double minY = points.Min(point => point.Y);
            double maxY = points.Max(point => point.Y);
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public override void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false)
        {
            PointF[] points = SamplePoints(64)
                .Select(worldToScreen)
                .Select(point => new PointF((float)point.X, (float)point.Y))
                .ToArray();
            if (points.Length < 2)
            {
                return;
            }

            using Pen pen = new(isPreview ? Color.LightGray : BorderColor, isPreview ? 1.5f : 0.25f);
            g.DrawLines(pen, points);
        }

        public override IShape Clone() => new ArcShape(this);

        public override bool ContainsPoint(PointD worldPoint, float tolerance)
        {
            double distance = Distance(Center, worldPoint);
            if (Math.Abs(distance - Radius) > tolerance)
            {
                return false;
            }

            double angle = Math.Atan2(worldPoint.Y - Center.Y, worldPoint.X - Center.X);
            return AngleLiesOnSweep(angle, StartAngleRadians, SweepAngleRadians);
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            yield return new SnapPoint(SnapType.Endpoint, StartPoint, this);
            yield return new SnapPoint(SnapType.Midpoint, MidPoint, this);
            yield return new SnapPoint(SnapType.Endpoint, EndPoint, this);
            yield return new SnapPoint(SnapType.Center, Center, this);

            double[] quadrantAngles =
            [
                0.0,
                Math.PI / 2.0,
                Math.PI,
                Math.PI * 1.5
            ];

            foreach (double angle in quadrantAngles)
            {
                if (AngleLiesOnSweep(angle, StartAngleRadians, SweepAngleRadians))
                {
                    yield return new SnapPoint(
                        SnapType.Quadrant,
                        new PointD(
                            Center.X + Radius * Math.Cos(angle),
                            Center.Y + Radius * Math.Sin(angle)),
                        this);
                }
            }
        }

        private static bool AngleLiesOnSweep(double angle, double startAngle, double sweepAngle)
        {
            if (sweepAngle >= 0.0)
            {
                double positiveDelta = NormalizePositive(angle - startAngle);
                return positiveDelta <= sweepAngle + 1e-9;
            }

            double negativeDelta = NormalizePositive(startAngle - angle);
            return negativeDelta <= -sweepAngle + 1e-9;
        }

        private static double NormalizePositive(double angle)
        {
            double full = Math.PI * 2.0;
            angle %= full;
            return angle < 0.0 ? angle + full : angle;
        }

        private static double Cross(PointD first, PointD second, PointD third)
        {
            return (second.X - first.X) * (third.Y - first.Y) -
                   (second.Y - first.Y) * (third.X - first.X);
        }

        private static double Distance(PointD first, PointD second)
        {
            double dx = second.X - first.X;
            double dy = second.Y - first.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
