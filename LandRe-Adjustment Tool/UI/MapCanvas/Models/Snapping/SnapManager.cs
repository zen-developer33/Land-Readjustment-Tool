using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping
{
    public sealed class MapCanvasSnapManager
    {
        public IEnumerable<SnapPoint> GetSnapCandidates(
            IReadOnlyList<IShape> nearbyShapes,
            IEnumerable<SnapPoint> extraSnapPoints,
            PointD? fromPoint)
        {
            List<SnapPoint> candidates = [];

            foreach (IShape shape in nearbyShapes)
            {
                candidates.AddRange(shape.GetSnapPoints());
            }

            candidates.AddRange(GetIntersectionSnapPoints(nearbyShapes));
            candidates.AddRange(extraSnapPoints);

            if (fromPoint.HasValue)
            {
                candidates.AddRange(GetPerpendicularSnapPoints(fromPoint.Value, nearbyShapes));
            }

            return candidates;
        }

        public SnapPoint? FindNearestSnapPointFromList(
            IEnumerable<SnapPoint> snapPoints,
            Point screenPoint,
            MapCanvasEngine engine,
            double snapPixelTolerance)
        {
            SnapPoint? best = null;
            double bestDistance = snapPixelTolerance;
            int bestPriority = int.MaxValue;

            foreach (SnapPoint snapPoint in snapPoints)
            {
                PointD snapScreen = engine.WorldToScreen(snapPoint.Position);
                double dx = snapScreen.X - screenPoint.X;
                double dy = snapScreen.Y - screenPoint.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                int priority = GetSnapPriority(snapPoint.Type);
                if (distance < bestDistance - 0.25 ||
                    (Math.Abs(distance - bestDistance) <= 0.25 && priority < bestPriority))
                {
                    bestDistance = distance;
                    bestPriority = priority;
                    best = snapPoint;
                }
            }

            return best;
        }

        public IEnumerable<SnapPoint> GetIntersectionSnapPoints(IReadOnlyList<IShape> shapes)
        {
            List<SnapPoint> result = [];

            for (int i = 0; i < shapes.Count; i++)
            {
                for (int j = i + 1; j < shapes.Count; j++)
                {
                    result.AddRange(GetShapePairIntersections(shapes[i], shapes[j]));
                }
            }

            return result;
        }

        public IEnumerable<SnapPoint> GetPerpendicularSnapPoints(
            PointD fromPoint,
            IEnumerable<IShape> shapes)
        {
            List<SnapPoint> result = [];

            foreach (IShape shape in shapes)
            {
                foreach ((PointD a, PointD b) in GetSegments(shape))
                {
                    TryAddPerpendicularFoot(fromPoint, a, b, result);
                }

                if (shape is CircleShape circle)
                {
                    TryAddCirclePerpendicularFoot(fromPoint, circle, result);
                }

                if (shape is ArcShape arc)
                {
                    TryAddCirclePerpendicularFoot(
                        fromPoint,
                        arc.Center,
                        arc.Radius,
                        result);
                }
            }

            return result;
        }

        public IEnumerable<PointD> GetPolylineSelfIntersections(IReadOnlyList<PointD> vertices)
        {
            List<PointD> result = [];

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                for (int j = 0; j < i - 1; j++)
                {
                    if (TryGetSegmentsIntersection(
                            vertices[i],
                            vertices[i + 1],
                            vertices[j],
                            vertices[j + 1],
                            out PointD intersection))
                    {
                        result.Add(intersection);
                    }
                }
            }

            return result;
        }

        private IEnumerable<SnapPoint> GetShapePairIntersections(IShape shapeA, IShape shapeB)
        {
            List<SnapPoint> result = [];
            List<(PointD a, PointD b)> segmentsA = GetSegments(shapeA);
            List<(PointD a, PointD b)> segmentsB = GetSegments(shapeB);

            foreach ((PointD a, PointD b) segmentA in segmentsA)
            {
                foreach ((PointD a, PointD b) segmentB in segmentsB)
                {
                    if (TryGetSegmentsIntersection(
                            segmentA.a,
                            segmentA.b,
                            segmentB.a,
                            segmentB.b,
                            out PointD intersection))
                    {
                        result.Add(new SnapPoint(SnapType.Intersection, intersection, null));
                    }
                }
            }

            if (shapeA is CircleShape circleA)
            {
                foreach ((PointD a, PointD b) segment in segmentsB)
                {
                    result.AddRange(LineCircleIntersections(segment.a, segment.b, circleA));
                }
            }

            if (shapeB is CircleShape circleB)
            {
                foreach ((PointD a, PointD b) segment in segmentsA)
                {
                    result.AddRange(LineCircleIntersections(segment.a, segment.b, circleB));
                }
            }

            if (shapeA is CircleShape firstCircle &&
                shapeB is CircleShape secondCircle)
            {
                result.AddRange(CircleCircleIntersections(firstCircle, secondCircle));
            }

            return result;
        }

        private static List<(PointD a, PointD b)> GetSegments(IShape shape)
        {
            List<(PointD a, PointD b)> segments = [];

            switch (shape)
            {
                case LineShape line:
                    segments.Add((line.Start, line.End));
                    break;

                case PolylineShape { Vertices.Count: > 1 } polyline:
                    for (int i = 0; i < polyline.Vertices.Count - 1; i++)
                    {
                        segments.Add((polyline.Vertices[i], polyline.Vertices[i + 1]));
                    }

                    if (polyline.IsClosed && polyline.Vertices.Count > 2)
                    {
                        segments.Add((polyline.Vertices[^1], polyline.Vertices[0]));
                    }

                    break;

                case RectangleShape rectangle:
                    PointD leftBottom = new(
                        Math.Min(rectangle.Start.X, rectangle.End.X),
                        Math.Min(rectangle.Start.Y, rectangle.End.Y));
                    PointD rightBottom = new(
                        Math.Max(rectangle.Start.X, rectangle.End.X),
                        Math.Min(rectangle.Start.Y, rectangle.End.Y));
                    PointD rightTop = new(
                        Math.Max(rectangle.Start.X, rectangle.End.X),
                        Math.Max(rectangle.Start.Y, rectangle.End.Y));
                    PointD leftTop = new(
                        Math.Min(rectangle.Start.X, rectangle.End.X),
                        Math.Max(rectangle.Start.Y, rectangle.End.Y));

                    segments.Add((leftBottom, rightBottom));
                    segments.Add((rightBottom, rightTop));
                    segments.Add((rightTop, leftTop));
                    segments.Add((leftTop, leftBottom));
                    break;

                case ArcShape arc:
                    PointD[] arcPoints = arc.SamplePoints(96).ToArray();
                    for (int i = 0; i < arcPoints.Length - 1; i++)
                    {
                        segments.Add((arcPoints[i], arcPoints[i + 1]));
                    }

                    break;
            }

            return segments;
        }

        private static void TryAddPerpendicularFoot(
            PointD fromPoint,
            PointD segmentStart,
            PointD segmentEnd,
            ICollection<SnapPoint> result)
        {
            double dx = segmentEnd.X - segmentStart.X;
            double dy = segmentEnd.Y - segmentStart.Y;
            double lengthSquared = dx * dx + dy * dy;
            if (lengthSquared < 1e-12)
            {
                return;
            }

            double t = ((fromPoint.X - segmentStart.X) * dx +
                        (fromPoint.Y - segmentStart.Y) * dy) / lengthSquared;
            if (t < 0.0 || t > 1.0)
            {
                return;
            }

            PointD foot = new(
                segmentStart.X + t * dx,
                segmentStart.Y + t * dy);
            if (Distance(fromPoint, foot) < 1e-6)
            {
                return;
            }

            result.Add(new SnapPoint(SnapType.Perpendicular, foot, null));
        }

        private static void TryAddCirclePerpendicularFoot(
            PointD fromPoint,
            CircleShape circle,
            ICollection<SnapPoint> result)
        {
            TryAddCirclePerpendicularFoot(fromPoint, circle.Center, circle.GetRadius(), result);
        }

        private static void TryAddCirclePerpendicularFoot(
            PointD fromPoint,
            PointD center,
            double radius,
            ICollection<SnapPoint> result)
        {
            double dx = fromPoint.X - center.X;
            double dy = fromPoint.Y - center.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < 1e-9)
            {
                return;
            }

            double ux = dx / distance;
            double uy = dy / distance;
            result.Add(new SnapPoint(
                SnapType.Perpendicular,
                new PointD(center.X + ux * radius, center.Y + uy * radius),
                null));
            result.Add(new SnapPoint(
                SnapType.Perpendicular,
                new PointD(center.X - ux * radius, center.Y - uy * radius),
                null));
        }

        public static bool TryGetSegmentsIntersection(
            PointD p1,
            PointD p2,
            PointD q1,
            PointD q2,
            out PointD intersection)
        {
            intersection = default;
            double s1x = p2.X - p1.X;
            double s1y = p2.Y - p1.Y;
            double s2x = q2.X - q1.X;
            double s2y = q2.Y - q1.Y;
            double denominator = -s2x * s1y + s1x * s2y;
            if (Math.Abs(denominator) < 1e-12)
            {
                return false;
            }

            double s = (-s1y * (p1.X - q1.X) + s1x * (p1.Y - q1.Y)) / denominator;
            double t = (s2x * (p1.Y - q1.Y) - s2y * (p1.X - q1.X)) / denominator;
            if (s < 0.0 || s > 1.0 || t < 0.0 || t > 1.0)
            {
                return false;
            }

            intersection = new PointD(p1.X + t * s1x, p1.Y + t * s1y);
            return true;
        }

        private static IEnumerable<SnapPoint> LineCircleIntersections(
            PointD lineStart,
            PointD lineEnd,
            CircleShape circle)
        {
            List<SnapPoint> result = [];
            double radius = circle.GetRadius();
            double x1 = lineStart.X - circle.Center.X;
            double y1 = lineStart.Y - circle.Center.Y;
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            double a = dx * dx + dy * dy;
            if (a < 1e-12)
            {
                return result;
            }

            double b = 2.0 * (x1 * dx + y1 * dy);
            double c = x1 * x1 + y1 * y1 - radius * radius;
            double discriminant = b * b - 4.0 * a * c;
            if (discriminant < 0.0)
            {
                return result;
            }

            double root = Math.Sqrt(discriminant);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                double t = (-b + sign * root) / (2.0 * a);
                if (t >= 0.0 && t <= 1.0)
                {
                    result.Add(new SnapPoint(
                        SnapType.Intersection,
                        new PointD(lineStart.X + t * dx, lineStart.Y + t * dy),
                        null));
                }
            }

            return result;
        }

        private static IEnumerable<SnapPoint> CircleCircleIntersections(
            CircleShape first,
            CircleShape second)
        {
            List<SnapPoint> result = [];
            double dx = second.Center.X - first.Center.X;
            double dy = second.Center.Y - first.Center.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double firstRadius = first.GetRadius();
            double secondRadius = second.GetRadius();

            if (distance <= 1e-12 ||
                distance > firstRadius + secondRadius ||
                distance < Math.Abs(firstRadius - secondRadius))
            {
                return result;
            }

            double a = (firstRadius * firstRadius -
                        secondRadius * secondRadius +
                        distance * distance) / (2.0 * distance);
            double hSquared = firstRadius * firstRadius - a * a;
            if (hSquared < 0.0)
            {
                return result;
            }

            double h = Math.Sqrt(hSquared);
            double baseX = first.Center.X + a * dx / distance;
            double baseY = first.Center.Y + a * dy / distance;
            double offsetX = -dy * h / distance;
            double offsetY = dx * h / distance;

            result.Add(new SnapPoint(
                SnapType.Intersection,
                new PointD(baseX + offsetX, baseY + offsetY),
                null));
            result.Add(new SnapPoint(
                SnapType.Intersection,
                new PointD(baseX - offsetX, baseY - offsetY),
                null));
            return result;
        }

        private static double Distance(PointD first, PointD second)
        {
            double dx = second.X - first.X;
            double dy = second.Y - first.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static int GetSnapPriority(SnapType snapType)
        {
            return snapType switch
            {
                SnapType.Endpoint => 0,
                SnapType.Intersection => 1,
                SnapType.Midpoint => 2,
                SnapType.Center => 3,
                SnapType.Quadrant => 4,
                SnapType.Perpendicular => 5,
                _ => 10
            };
        }
    }
}
