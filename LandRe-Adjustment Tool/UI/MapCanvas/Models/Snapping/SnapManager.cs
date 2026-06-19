using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping
{
    public sealed class MapCanvasSnapManager
    {
        private const double GeometryTolerance = 1e-9;
        private const double SnapPointDuplicateTolerance = 1e-6;

        public IEnumerable<SnapPoint> GetSnapCandidates(
            IReadOnlyList<IShape> nearbyShapes,
            IEnumerable<SnapPoint> extraSnapPoints,
            PointD? fromPoint)
        {
            List<SnapPoint> candidates = new List<SnapPoint>();

            foreach (IShape shape in nearbyShapes)
            {
                // Snap candidates come from logical geometry only. Layer line styles such as
                // dashed, dotted, or centerline are paint instructions and must not create
                // separate snap targets for the rendered dash pieces.
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
            List<SnapPoint> result = new List<SnapPoint>();

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
            List<SnapPoint> result = new List<SnapPoint>();

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

            }

            return result;
        }

        public IEnumerable<PointD> GetPolylineSelfIntersections(IReadOnlyList<PointD> vertices)
        {
            List<PointD> result = new List<PointD>();

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
            List<SnapPoint> result = new List<SnapPoint>();
            List<IntersectionPrimitive> primitivesA = GetIntersectionPrimitives(shapeA);
            List<IntersectionPrimitive> primitivesB = GetIntersectionPrimitives(shapeB);

            foreach (IntersectionPrimitive primitiveA in primitivesA)
            {
                foreach (IntersectionPrimitive primitiveB in primitivesB)
                {
                    AddPrimitiveIntersections(primitiveA, primitiveB, result);
                }
            }

            return result;
        }

        private static List<IntersectionPrimitive> GetIntersectionPrimitives(IShape shape)
        {
            List<IntersectionPrimitive> primitives = new List<IntersectionPrimitive>();

            switch (shape)
            {
                case LineShape line:
                    AddLinePrimitive(primitives, line.Start, line.End);
                    break;

                case PolylineShape { Vertices.Count: > 1 } polyline:
                    AddPolylinePrimitives(primitives, polyline);
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

                    AddLinePrimitive(primitives, leftBottom, rightBottom);
                    AddLinePrimitive(primitives, rightBottom, rightTop);
                    AddLinePrimitive(primitives, rightTop, leftTop);
                    AddLinePrimitive(primitives, leftTop, leftBottom);
                    break;

                case CircleShape circle:
                    AddCirclePrimitive(primitives, circle.Center, circle.GetRadius());
                    break;

                case ArcShape arc:
                    AddArcPrimitive(primitives, arc);
                    break;
            }

            return primitives;
        }

        private static void AddPolylinePrimitives(
            ICollection<IntersectionPrimitive> primitives,
            PolylineShape polyline)
        {
            if (polyline.Segments.Count > 0)
            {
                foreach (PolylineShape.PolylineSegment segment in polyline.Segments)
                {
                    if (segment.Kind == PolylineShape.PolylineSegmentKind.Arc && segment.Arc != null)
                    {
                        AddArcPrimitive(primitives, segment.Arc);
                    }
                    else
                    {
                        AddLinePrimitive(primitives, segment.Start, segment.End);
                    }
                }

                if (polyline.IsClosed && polyline.Vertices.Count > 2)
                {
                    PointD lastEnd = polyline.Segments[^1].End;
                    PointD first = polyline.Vertices[0];
                    if (Distance(lastEnd, first) > GeometryTolerance)
                    {
                        AddLinePrimitive(primitives, lastEnd, first);
                    }
                }

                return;
            }

            for (int i = 0; i < polyline.Vertices.Count - 1; i++)
            {
                AddLinePrimitive(primitives, polyline.Vertices[i], polyline.Vertices[i + 1]);
            }

            if (polyline.IsClosed && polyline.Vertices.Count > 2)
            {
                AddLinePrimitive(primitives, polyline.Vertices[^1], polyline.Vertices[0]);
            }
        }

        private static void AddLinePrimitive(
            ICollection<IntersectionPrimitive> primitives,
            PointD start,
            PointD end)
        {
            if (Distance(start, end) <= GeometryTolerance)
            {
                return;
            }

            primitives.Add(IntersectionPrimitive.Line(start, end));
        }

        private static void AddCirclePrimitive(
            ICollection<IntersectionPrimitive> primitives,
            PointD center,
            double radius)
        {
            if (!double.IsFinite(radius) || radius <= GeometryTolerance)
            {
                return;
            }

            primitives.Add(IntersectionPrimitive.Circle(center, radius));
        }

        private static void AddArcPrimitive(
            ICollection<IntersectionPrimitive> primitives,
            ArcShape arc)
        {
            if (!double.IsFinite(arc.Radius) ||
                arc.Radius <= GeometryTolerance ||
                !double.IsFinite(arc.StartAngleRadians) ||
                !double.IsFinite(arc.SweepAngleRadians) ||
                Math.Abs(arc.SweepAngleRadians) <= GeometryTolerance)
            {
                return;
            }

            primitives.Add(IntersectionPrimitive.Arc(
                arc.Center,
                arc.Radius,
                arc.StartAngleRadians,
                arc.SweepAngleRadians));
        }

        private static void AddPrimitiveIntersections(
            IntersectionPrimitive first,
            IntersectionPrimitive second,
            ICollection<SnapPoint> result)
        {
            if (first.Kind == IntersectionPrimitiveKind.Line &&
                second.Kind == IntersectionPrimitiveKind.Line)
            {
                if (TryGetSegmentsIntersection(first.Start, first.End, second.Start, second.End, out PointD intersection))
                {
                    AddIntersection(result, intersection);
                }

                return;
            }

            if (first.Kind == IntersectionPrimitiveKind.Line &&
                second.Kind is IntersectionPrimitiveKind.Circle or IntersectionPrimitiveKind.Arc)
            {
                AddLineCircularIntersections(first, second, result);
                return;
            }

            if (second.Kind == IntersectionPrimitiveKind.Line &&
                first.Kind is IntersectionPrimitiveKind.Circle or IntersectionPrimitiveKind.Arc)
            {
                AddLineCircularIntersections(second, first, result);
                return;
            }

            if (first.Kind is IntersectionPrimitiveKind.Circle or IntersectionPrimitiveKind.Arc &&
                second.Kind is IntersectionPrimitiveKind.Circle or IntersectionPrimitiveKind.Arc)
            {
                AddCircularCircularIntersections(first, second, result);
            }
        }

        private static void AddLineCircularIntersections(
            IntersectionPrimitive line,
            IntersectionPrimitive circular,
            ICollection<SnapPoint> result)
        {
            foreach (PointD point in GetLineCircleIntersectionPoints(
                         line.Start,
                         line.End,
                         circular.Center,
                         circular.Radius))
            {
                if (PrimitiveContainsPoint(circular, point))
                {
                    AddIntersection(result, point);
                }
            }
        }

        private static void AddCircularCircularIntersections(
            IntersectionPrimitive first,
            IntersectionPrimitive second,
            ICollection<SnapPoint> result)
        {
            foreach (PointD point in GetCircleCircleIntersectionPoints(
                         first.Center,
                         first.Radius,
                         second.Center,
                         second.Radius))
            {
                if (PrimitiveContainsPoint(first, point) &&
                    PrimitiveContainsPoint(second, point))
                {
                    AddIntersection(result, point);
                }
            }
        }

        private static bool PrimitiveContainsPoint(
            IntersectionPrimitive primitive,
            PointD point)
        {
            if (primitive.Kind != IntersectionPrimitiveKind.Arc)
            {
                return true;
            }

            double angle = Math.Atan2(point.Y - primitive.Center.Y, point.X - primitive.Center.X);
            return ArcShape.AngleLiesOnSweepPublic(
                angle,
                primitive.StartAngleRadians,
                primitive.SweepAngleRadians);
        }

        private static void AddIntersection(
            ICollection<SnapPoint> result,
            PointD point)
        {
            foreach (SnapPoint existing in result)
            {
                if (Distance(existing.Position, point) <= SnapPointDuplicateTolerance)
                {
                    return;
                }
            }

            result.Add(new SnapPoint(SnapType.Intersection, point, null));
        }

        private static List<(PointD a, PointD b)> GetSegments(IShape shape)
        {
            List<(PointD a, PointD b)> segments = new List<(PointD a, PointD b)>();

            switch (shape)
            {
                case LineShape line:
                    segments.Add((line.Start, line.End));
                    break;

                case PolylineShape { Vertices.Count: > 1 } polyline:
                    if (polyline.Segments.Count > 0)
                    {
                        foreach (PolylineShape.PolylineSegment segment in polyline.Segments)
                        {
                            if (segment.Kind == PolylineShape.PolylineSegmentKind.Arc && segment.Arc != null)
                            {
                                continue;
                            }
                            else
                            {
                                segments.Add((segment.Start, segment.End));
                            }
                        }

                        if (polyline.IsClosed && polyline.Vertices.Count > 2)
                        {
                            PointD lastEnd = polyline.Segments[^1].End;
                            PointD first = polyline.Vertices[0];
                            if (Distance(lastEnd, first) > 1e-9)
                            {
                                segments.Add((lastEnd, first));
                            }
                        }

                        break;
                    }

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

                case ArcShape:
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

        private static IEnumerable<PointD> GetLineCircleIntersectionPoints(
            PointD lineStart,
            PointD lineEnd,
            PointD center,
            double radius)
        {
            List<PointD> result = new List<PointD>();
            double x1 = lineStart.X - center.X;
            double y1 = lineStart.Y - center.Y;
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            double a = dx * dx + dy * dy;
            if (a < GeometryTolerance)
            {
                return result;
            }

            double b = 2.0 * (x1 * dx + y1 * dy);
            double c = x1 * x1 + y1 * y1 - radius * radius;
            double discriminant = b * b - 4.0 * a * c;
            if (discriminant < -GeometryTolerance)
            {
                return result;
            }

            if (Math.Abs(discriminant) <= GeometryTolerance)
            {
                discriminant = 0.0;
            }

            double root = Math.Sqrt(discriminant);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                double t = (-b + sign * root) / (2.0 * a);
                if (t >= -GeometryTolerance && t <= 1.0 + GeometryTolerance)
                {
                    t = Math.Clamp(t, 0.0, 1.0);
                    AddUniquePoint(
                        result,
                        new PointD(lineStart.X + t * dx, lineStart.Y + t * dy));
                }
            }

            return result;
        }

        private static IEnumerable<PointD> GetCircleCircleIntersectionPoints(
            PointD firstCenter,
            double firstRadius,
            PointD secondCenter,
            double secondRadius)
        {
            List<PointD> result = [];
            double dx = secondCenter.X - firstCenter.X;
            double dy = secondCenter.Y - firstCenter.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= GeometryTolerance ||
                distance > firstRadius + secondRadius + GeometryTolerance ||
                distance < Math.Abs(firstRadius - secondRadius) - GeometryTolerance)
            {
                return result;
            }

            double a = (firstRadius * firstRadius -
                        secondRadius * secondRadius +
                        distance * distance) / (2.0 * distance);
            double hSquared = firstRadius * firstRadius - a * a;
            if (hSquared < -GeometryTolerance)
            {
                return result;
            }

            if (Math.Abs(hSquared) <= GeometryTolerance)
            {
                hSquared = 0.0;
            }

            double h = Math.Sqrt(hSquared);
            double baseX = firstCenter.X + a * dx / distance;
            double baseY = firstCenter.Y + a * dy / distance;
            double offsetX = -dy * h / distance;
            double offsetY = dx * h / distance;

            AddUniquePoint(result, new PointD(baseX + offsetX, baseY + offsetY));
            AddUniquePoint(result, new PointD(baseX - offsetX, baseY - offsetY));
            return result;
        }

        private static void AddUniquePoint(
            ICollection<PointD> points,
            PointD point)
        {
            foreach (PointD existing in points)
            {
                if (Distance(existing, point) <= SnapPointDuplicateTolerance)
                {
                    return;
                }
            }

            points.Add(point);
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

        private enum IntersectionPrimitiveKind
        {
            Line,
            Circle,
            Arc
        }

        private readonly record struct IntersectionPrimitive(
            IntersectionPrimitiveKind Kind,
            PointD Start,
            PointD End,
            PointD Center,
            double Radius,
            double StartAngleRadians,
            double SweepAngleRadians)
        {
            public static IntersectionPrimitive Line(PointD start, PointD end) =>
                new(
                    IntersectionPrimitiveKind.Line,
                    start,
                    end,
                    default,
                    0.0,
                    0.0,
                    0.0);

            public static IntersectionPrimitive Circle(PointD center, double radius) =>
                new(
                    IntersectionPrimitiveKind.Circle,
                    default,
                    default,
                    center,
                    radius,
                    0.0,
                    0.0);

            public static IntersectionPrimitive Arc(
                PointD center,
                double radius,
                double startAngleRadians,
                double sweepAngleRadians) =>
                new(
                    IntersectionPrimitiveKind.Arc,
                    default,
                    default,
                    center,
                    radius,
                    startAngleRadians,
                    sweepAngleRadians);
        }
    }
}
