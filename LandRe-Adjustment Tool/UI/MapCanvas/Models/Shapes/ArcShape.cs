using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    public sealed class ArcShape : Shape, ISnapProvider
    {
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

        public static ArcShape? FromCenterStartEnd(PointD center, PointD start, PointD end, bool? clockwise = null)
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

            double ccwSweep = NormalizePositive(endAngle - startAngle);
            double cwSweep = ccwSweep - Math.PI * 2.0;   // always negative

            double sweep;
            if (clockwise == true)
            {
                sweep = cwSweep;
            }
            else if (clockwise == false)
            {
                sweep = ccwSweep;
            }
            else
            {
                // Choose the shorter arc (prefer CCW when equal)
                sweep = Math.Abs(cwSweep) < ccwSweep - 1e-9 ? cwSweep : ccwSweep;
            }

            if (Math.Abs(sweep) <= 1e-9)
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
            // Seed with the two endpoints, then add any axis-extremum that lies on the arc.
            double minX = Math.Min(StartPoint.X, EndPoint.X);
            double maxX = Math.Max(StartPoint.X, EndPoint.X);
            double minY = Math.Min(StartPoint.Y, EndPoint.Y);
            double maxY = Math.Max(StartPoint.Y, EndPoint.Y);

            double[] extremaAngles = [0.0, Math.PI / 2.0, Math.PI, Math.PI * 1.5];
            foreach (double angle in extremaAngles)
            {
                if (!AngleLiesOnSweep(angle, StartAngleRadians, SweepAngleRadians))
                {
                    continue;
                }

                double x = Center.X + Radius * Math.Cos(angle);
                double y = Center.Y + Radius * Math.Sin(angle);
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }

            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public override void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false)
        {
            PointD centerScreenD = worldToScreen(Center);
            PointD radiusScreenD = worldToScreen(new PointD(Center.X + Radius, Center.Y));
            float centerX = (float)centerScreenD.X;
            float centerY = (float)centerScreenD.Y;
            float radius = (float)Distance(
                new PointD(centerScreenD.X, centerScreenD.Y),
                new PointD(radiusScreenD.X, radiusScreenD.Y));
            if (!float.IsFinite(centerX) ||
                !float.IsFinite(centerY) ||
                !float.IsFinite(radius) ||
                radius <= 0.5f)
            {
                return;
            }

            RectangleF bounds = new(
                centerX - radius,
                centerY - radius,
                radius * 2.0f,
                radius * 2.0f);
            float startAngleDegrees = (float)(-StartAngleRadians * 180.0 / Math.PI);
            float sweepAngleDegrees = (float)(-SweepAngleRadians * 180.0 / Math.PI);
            if (!float.IsFinite(startAngleDegrees) ||
                !float.IsFinite(sweepAngleDegrees) ||
                Math.Abs(sweepAngleDegrees) < 0.001f)
            {
                return;
            }

            using Pen pen = new(isPreview ? Color.LightGray : BorderColor, isPreview ? 1.5f : 0.25f);
            g.DrawArc(pen, bounds, startAngleDegrees, sweepAngleDegrees);
        }

        public override IShape Clone() => new ArcShape(this);

        /// <summary>
        /// Moves the arc center by the supplied world-coordinate delta.
        /// </summary>
        /// <param name="delta">The distance to add to <see cref="Center"/>.</param>
        public override void Translate(PointD delta)
        {
            Center = new PointD(Center.X + delta.X, Center.Y + delta.Y);
        }

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

            double[] quadrantAngles = new[]
            {
                0.0,
                Math.PI / 2.0,
                Math.PI,
                Math.PI * 1.5
            };

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

        public static ArcShape? FromTangentStartEnd(PointD start, PointD prev, PointD end)
        {
            double vx = start.X - prev.X;
            double vy = start.Y - prev.Y;
            double vlen = Math.Sqrt(vx * vx + vy * vy);
            if (vlen < 1e-9)
            {
                return null;
            }

            // Normal to the tangent at start (points toward circle center offset)
            double nx = -vy;
            double ny = vx;

            // Midpoint of chord from start to end
            double mx = (start.X + end.X) / 2.0;
            double my = (start.Y + end.Y) / 2.0;

            // Perpendicular to chord
            double bx = -(end.Y - start.Y);
            double by = (end.X - start.X);

            // Solve for t and s in: start + t * n = mid + s * b
            double a11 = nx;
            double a12 = -bx;
            double a21 = ny;
            double a22 = -by;
            double rhsx = mx - start.X;
            double rhsy = my - start.Y;
            double det = a11 * a22 - a12 * a21;
            if (Math.Abs(det) < 1e-12)
            {
                return null;
            }

            double t = (rhsx * a22 - a12 * rhsy) / det;
            double centerX = start.X + t * nx;
            double centerY = start.Y + t * ny;
            PointD center = new(centerX, centerY);
            double radius = Distance(center, start);
            if (radius <= 0.0 || !double.IsFinite(radius))
            {
                return null;
            }

            double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);

            double tIncX = -Math.Sin(startAngle);
            double tIncY = Math.Cos(startAngle);
            double dot = tIncX * vx + tIncY * vy;
            double sweep = dot >= 0
                ? NormalizePositive(endAngle - startAngle)
                : -NormalizePositive(startAngle - endAngle);

            return new ArcShape(center, radius, startAngle, sweep);
        }

        public static bool AngleLiesOnSweepPublic(double angle, double startAngle, double sweepAngle)
            => AngleLiesOnSweep(angle, startAngle, sweepAngle);

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
