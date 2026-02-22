using System.Collections.Generic;
using System.Linq;
using Drawing_Canvas_Practice.Models.Shapes;
using Drawing_Canvas_Practice.Core;

namespace Drawing_Canvas_Practice.Models.Snapping
{
    /// <summary>
    /// Owns ALL snap-related logic. The form knows nothing about how
    /// snap candidates are found or how intersections are computed.
    ///
    /// RESPONSIBILITIES:
    ///   - Find nearest snap point to the mouse cursor
    ///   - Build the full candidate list (shape endpoints, midpoints, centers,
    ///     quadrants, shape-shape intersections, polyline self-intersections,
    ///     polyline-shape intersections)
    ///   - All geometric intersection math (segment-segment, line-circle,
    ///     circle-circle)
    ///
    /// WHAT THE FORM DOES:
    ///   - Calls GetSnapCandidates() and FindNearestSnapPointFromList()
    ///   - Passes raw data (visible shapes, polyline vertices, mouse pos)
    ///   - Uses the returned SnapPoint for cursor snapping and UI display
    ///
    /// WHAT THE FORM DOES NOT DO:
    ///   - Any intersection math
    ///   - Any spatial queries for snapping
    ///   - Any distance-to-shape calculations
    /// </summary>
    public class SnapManager
    {
        public double SnapPixelTolerance { get; set; } = 15.0; // pixels

        // ─────────────────────────────────────────────────────────────────────
        // PUBLIC SNAP API  –  the only methods the form calls
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Master entry point called on every MouseMove.
        ///
        /// Builds ALL snap candidates:
        ///   1. Endpoint / midpoint / center / quadrant from every visible shape
        ///   2. Shape-shape intersections
        ///   3. In-progress polyline extras (vertices + segment midpoints)
        ///   4. Polyline self-intersections
        ///   5. Polyline-shape intersections
        ///   6. Perpendicular snap points from drawing start point → existing segments
        ///      (only when a start point exists, i.e. drawing is in progress)
        /// </summary>
        public IEnumerable<SnapPoint> GetSnapCandidates(
            IEnumerable<ISnapProvider> visibleShapes,
            List<SnapPoint>? extraSnapPoints,
            PointD mouseScreen,
            DrawingEngine engine,
            List<PointD>? polylineVertices = null,
            PointD? mouseWorld = null,
            bool polylineDrawing = false,
            PointD? fromPoint = null)
        {
            var shapeList = visibleShapes.ToList();
            var candidates = new List<SnapPoint>();

            // 1. Basic snap points from all visible shapes
            candidates.AddRange(shapeList.SelectMany(s => s.GetSnapPoints()));

            // 2. Shape-shape intersections
            candidates.AddRange(GetIntersectionSnapPoints(shapeList));

            // 3, 4, 5 — in-progress polyline contributions
            if (polylineDrawing && polylineVertices != null && polylineVertices.Count > 0)
            {
                // 3. Extra snap points passed in by the form
                //    (vertices and segment midpoints of the in-progress polyline)
                if (extraSnapPoints != null)
                    candidates.AddRange(extraSnapPoints);

                // 4. Polyline self-intersections
                foreach (var pt in GetPolylineSelfIntersections(polylineVertices))
                    candidates.Add(new SnapPoint(SnapType.Intersection, pt, null));

                // 5. Polyline crossing existing shapes
                candidates.AddRange(GetPolylineShapeIntersections(polylineVertices, shapeList));
            }

            // 6. Perpendicular snap — only when a drawing start point exists.
            //    For polyline: the active "from" point is the last placed vertex.
            PointD? activeFromPoint = fromPoint;
            if (polylineDrawing && polylineVertices != null && polylineVertices.Count > 0)
                activeFromPoint = polylineVertices[polylineVertices.Count - 1];

            if (activeFromPoint.HasValue)
                candidates.AddRange(GetPerpendicularSnapPoints(activeFromPoint.Value, shapeList));

            return candidates;
        }

        /// <summary>
        /// Returns the snap point from the candidate list that is closest to
        /// the mouse cursor within SnapPixelTolerance.
        /// Returns null when no snap point is close enough.
        /// </summary>
        public SnapPoint? FindNearestSnapPointFromList(
            IEnumerable<SnapPoint> snapPoints,
            PointD mouseScreen,
            DrawingEngine engine)
        {
            SnapPoint? best = null;
            double bestDistWorld = engine.ScreenToWorldDistance(SnapPixelTolerance);
            PointD mouseWorld = engine.ScreenToWorld(mouseScreen);

            foreach (var snap in snapPoints)
            {
                double dx = snap.Position.X - mouseWorld.X;
                double dy = snap.Position.Y - mouseWorld.Y;
                double dist = System.Math.Sqrt(dx * dx + dy * dy);
                if (dist < bestDistWorld)
                {
                    bestDistWorld = dist;
                    best = snap;
                }
            }

            return best;
        }

        /// <summary>
        /// Convenience overload for simple snap queries without a pre-built list.
        /// </summary>
        public SnapPoint? FindNearestSnapPoint(
            IEnumerable<ISnapProvider> shapes,
            PointD mouseScreen,
            DrawingEngine engine)
            => FindNearestSnapPointFromList(
                shapes.SelectMany(s => s.GetSnapPoints()),
                mouseScreen, engine);

        // ─────────────────────────────────────────────────────────────────────
        // SHAPE-SHAPE INTERSECTIONS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all intersection snap points between every unique pair
        /// of visible shapes. O(n²) — fine for up to ~200 shapes in viewport.
        /// </summary>
        public IEnumerable<SnapPoint> GetIntersectionSnapPoints(
            IEnumerable<ISnapProvider> shapes)
        {
            var result = new List<SnapPoint>();
            var shapeList = shapes.ToList();

            for (int i = 0; i < shapeList.Count; i++)
                for (int j = i + 1; j < shapeList.Count; j++)
                    result.AddRange(GetShapePairIntersections(shapeList[i], shapeList[j]));

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PERPENDICULAR SNAP  –  temporary, only while drawing is in progress
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all perpendicular snap points from a fixed "from" point
        /// to every line segment in the visible shapes.
        ///
        /// WHAT THIS IS:
        ///   The "foot of the perpendicular" — the unique point on a segment
        ///   such that a line from fromPoint to that point is exactly 90° to
        ///   the segment. This is what AutoCAD shows with the small square glyph.
        ///
        /// WHEN IT IS ACTIVE:
        ///   - Line tool:     after clicking the start point
        ///   - Polyline tool: after placing each vertex (from = last vertex)
        ///   - Circle tool:   after clicking the centre point
        ///   - NOT active before the first click (fromPoint is null)
        ///   - NOT active for circle-to-circle perpendicular (not meaningful)
        ///
        /// SUPPORTED SHAPES:
        ///   LineShape, PolylineShape (each segment), RectangleShape (4 edges)
        ///   CircleShape: gives the point on the circumference along the
        ///   radial line from center through fromPoint (tangent perpendicular)
        /// </summary>
        public List<SnapPoint> GetPerpendicularSnapPoints(
            PointD fromPoint,
            IEnumerable<ISnapProvider> shapes)
        {
            var result = new List<SnapPoint>();

            foreach (var shape in shapes)
            {
                switch (shape)
                {
                    case LineShape line:
                        TryAddPerpendicularFoot(fromPoint, line.Start, line.End, result);
                        break;

                    case PolylineShape poly:
                        // Check every segment of the polyline
                        for (int i = 0; i < poly.Vertices.Count - 1; i++)
                            TryAddPerpendicularFoot(
                                fromPoint, poly.Vertices[i], poly.Vertices[i + 1], result);
                        if (poly.IsClosed && poly.Vertices.Count > 2)
                            TryAddPerpendicularFoot(
                                fromPoint, poly.Vertices.Last(), poly.Vertices.First(), result);
                        break;

                    case RectangleShape rect:
                        // Decompose rectangle into 4 edges
                        var c0 = rect.Start;
                        var c1 = new PointD(rect.End.X, rect.Start.Y);
                        var c2 = rect.End;
                        var c3 = new PointD(rect.Start.X, rect.End.Y);
                        TryAddPerpendicularFoot(fromPoint, c0, c1, result);
                        TryAddPerpendicularFoot(fromPoint, c1, c2, result);
                        TryAddPerpendicularFoot(fromPoint, c2, c3, result);
                        TryAddPerpendicularFoot(fromPoint, c3, c0, result);
                        break;

                    case CircleShape circle:
                        // The perpendicular from fromPoint to a circle is the
                        // point on the circumference along the line from centre
                        // through fromPoint.  This is the point where a chord
                        // from fromPoint would be 90° to the tangent.
                        TryAddCirclePerpendicularFoot(fromPoint, circle, result);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the foot of the perpendicular from point P to segment A→B.
        /// Adds a Perpendicular snap point if the foot lies ON the segment (t ∈ [0,1])
        /// and fromPoint is not already on the line (degenerate case).
        ///
        /// MATH:
        ///   Project P onto the infinite line through A and B:
        ///     t = dot(P - A, B - A) / dot(B - A, B - A)
        ///   The foot is: F = A + t * (B - A)
        ///   Valid when t ∈ [0, 1]  (foot is on the segment, not outside it)
        /// </summary>
        private static void TryAddPerpendicularFoot(
            PointD fromPoint,
            PointD segA,
            PointD segB,
            List<SnapPoint> result)
        {
            double dx = segB.X - segA.X;
            double dy = segB.Y - segA.Y;
            double lenSq = dx * dx + dy * dy;

            // Degenerate segment (zero length) — skip
            if (lenSq < 1e-12) return;

            // Project fromPoint onto the infinite line
            double t = ((fromPoint.X - segA.X) * dx +
                        (fromPoint.Y - segA.Y) * dy) / lenSq;

            // Only valid if foot is on the segment
            if (t < 0.0 || t > 1.0) return;

            double footX = segA.X + t * dx;
            double footY = segA.Y + t * dy;

            // Skip if fromPoint is already on the segment (perpendicular is the point itself)
            double distToFoot = System.Math.Sqrt(
                (fromPoint.X - footX) * (fromPoint.X - footX) +
                (fromPoint.Y - footY) * (fromPoint.Y - footY));
            if (distToFoot < 1e-6) return;

            result.Add(new SnapPoint(SnapType.Perpendicular, new PointD(footX, footY), null));
        }

        /// <summary>
        /// Computes the perpendicular snap point on a circle from an external point.
        ///
        /// WHAT THIS MEANS FOR A CIRCLE:
        ///   Draw a line from the circle's center through fromPoint.
        ///   That line hits the circle circumference at two points.
        ///   The one on the same side as fromPoint is the perpendicular snap
        ///   (a line from fromPoint to that point is perpendicular to the tangent).
        ///   AutoCAD gives both points but the near one is the useful one.
        ///
        /// SKIPPED when fromPoint is exactly at the circle center (degenerate).
        /// </summary>
        private static void TryAddCirclePerpendicularFoot(
            PointD fromPoint,
            CircleShape circle,
            List<SnapPoint> result)
        {
            double dx = fromPoint.X - circle.Center.X;
            double dy = fromPoint.Y - circle.Center.Y;
            double dist = System.Math.Sqrt(dx * dx + dy * dy);

            // fromPoint is at the centre — no well-defined perpendicular direction
            if (dist < 1e-9) return;

            double r = circle.GetRadius();
            double ux = dx / dist; // unit vector from centre toward fromPoint
            double uy = dy / dist;

            // Near foot: on the same side as fromPoint
            result.Add(new SnapPoint(
                SnapType.Perpendicular,
                new PointD(circle.Center.X + ux * r, circle.Center.Y + uy * r),
                null));

            // Far foot: on the opposite side (AutoCAD also shows this)
            result.Add(new SnapPoint(
                SnapType.Perpendicular,
                new PointD(circle.Center.X - ux * r, circle.Center.Y - uy * r),
                null));
        }

        // ─────────────────────────────────────────────────────────────────────
        // IN-PROGRESS POLYLINE INTERSECTIONS
        // ─────────────────────────────────────────────────────────────────────
        /// Called when a new vertex is placed so snap candidates stay current.
        /// </summary>
        public List<PointD> GetPolylineSelfIntersections(List<PointD> vertices)
        {
            var result = new List<PointD>();
            int n = vertices.Count;

            for (int i = 0; i < n - 1; i++)
                for (int j = 0; j < i - 1; j++) // skip adjacent segments
                    if (TryGetSegmentsIntersection(
                            vertices[i], vertices[i + 1],
                            vertices[j], vertices[j + 1],
                            out PointD pt))
                        result.Add(pt);

            return result;
        }

        /// <summary>
        /// Finds all points where the in-progress polyline crosses any existing shape.
        ///
        /// PERFORMANCE:
        ///   - Caps at 100 nearest shapes (sorted by distance to polyline centre)
        ///   - The caller already passes in spatially-filtered shapes via QueryShapesInBound
        /// </summary>
        public List<SnapPoint> GetPolylineShapeIntersections(
            List<PointD> vertices,
            List<ISnapProvider> nearbyShapes)
        {
            var result = new List<SnapPoint>();
            if (vertices.Count < 2) return result;

            // Cap at 100 nearest shapes
            if (nearbyShapes.Count > 100)
            {
                double minX = vertices.Min(v => v.X), maxX = vertices.Max(v => v.X);
                double minY = vertices.Min(v => v.Y), maxY = vertices.Max(v => v.Y);
                var center = new PointD((minX + maxX) / 2, (minY + maxY) / 2);

                nearbyShapes = nearbyShapes
                    .OrderBy(s => DistanceToShapeCenter(s, center))
                    .Take(100)
                    .ToList();
            }

            for (int i = 0; i < vertices.Count - 1; i++)
                foreach (var shape in nearbyShapes)
                    result.AddRange(
                        GetSegmentShapeIntersections(vertices[i], vertices[i + 1], shape));

            return result;
        }

        /// <summary>
        /// Returns all intersection snap points between one line segment and
        /// one shape (handles Line, Polyline, Rectangle, Circle).
        /// </summary>
        public List<SnapPoint> GetSegmentShapeIntersections(
            PointD p1,
            PointD p2,
            ISnapProvider shape)
        {
            var result = new List<SnapPoint>();

            switch (shape)
            {
                case LineShape line:
                    if (TryGetSegmentsIntersection(p1, p2, line.Start, line.End, out PointD pt))
                        result.Add(new SnapPoint(SnapType.Intersection, pt, null));
                    break;

                case PolylineShape poly:
                    for (int i = 0; i < poly.Vertices.Count - 1; i++)
                        if (TryGetSegmentsIntersection(
                                p1, p2, poly.Vertices[i], poly.Vertices[i + 1], out PointD inter))
                            result.Add(new SnapPoint(SnapType.Intersection, inter, null));
                    if (poly.IsClosed && poly.Vertices.Count > 2)
                        if (TryGetSegmentsIntersection(
                                p1, p2, poly.Vertices.Last(), poly.Vertices.First(), out PointD closing))
                            result.Add(new SnapPoint(SnapType.Intersection, closing, null));
                    break;

                case RectangleShape rect:
                    var corners = new[]
                    {
                        rect.Start,
                        new PointD(rect.End.X, rect.Start.Y),
                        rect.End,
                        new PointD(rect.Start.X, rect.End.Y)
                    };
                    for (int i = 0; i < 4; i++)
                        if (TryGetSegmentsIntersection(
                                p1, p2, corners[i], corners[(i + 1) % 4], out PointD edge))
                            result.Add(new SnapPoint(SnapType.Intersection, edge, null));
                    break;

                case CircleShape circle:
                    result.AddRange(LineCircleIntersections(p1, p2, circle));
                    break;
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GEOMETRY MATH  –  pure static/private functions, no side effects
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parametric segment-segment intersection test.
        /// Returns true and the world-space intersection point when segments cross.
        /// </summary>
        public static bool TryGetSegmentsIntersection(
            PointD p1, PointD p2,
            PointD q1, PointD q2,
            out PointD intersection)
        {
            intersection = default;
            double s1x = p2.X - p1.X, s1y = p2.Y - p1.Y;
            double s2x = q2.X - q1.X, s2y = q2.Y - q1.Y;
            double denom = -s2x * s1y + s1x * s2y;
            if (denom == 0) return false; // parallel

            double s = (-s1y * (p1.X - q1.X) + s1x * (p1.Y - q1.Y)) / denom;
            double t = (s2x * (p1.Y - q1.Y) - s2y * (p1.X - q1.X)) / denom;

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                intersection = new PointD(p1.X + t * s1x, p1.Y + t * s1y);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns all intersection points between one shape pair.
        /// Dispatches to the correct algorithm based on shape types.
        /// </summary>
        private IEnumerable<SnapPoint> GetShapePairIntersections(
            ISnapProvider shapeA,
            ISnapProvider shapeB)
        {
            var result = new List<SnapPoint>();
            var segmentsA = GetSegments(shapeA);
            var segmentsB = GetSegments(shapeB);

            // Both segmented shapes
            if (segmentsA.Count > 0 && segmentsB.Count > 0)
                foreach (var a in segmentsA)
                    foreach (var b in segmentsB)
                        if (TryGetSegmentsIntersection(a.a, a.b, b.a, b.b, out PointD pt))
                            result.Add(new SnapPoint(SnapType.Intersection, pt, null));

            // Segmented shape vs Circle
            if (segmentsA.Count > 0 && shapeB is CircleShape cB1)
                foreach (var seg in segmentsA)
                    result.AddRange(LineCircleIntersections(seg.a, seg.b, cB1));

            if (segmentsB.Count > 0 && shapeA is CircleShape cA1)
                foreach (var seg in segmentsB)
                    result.AddRange(LineCircleIntersections(seg.a, seg.b, cA1));

            // Circle vs Circle
            if (shapeA is CircleShape cA2 && shapeB is CircleShape cB2)
                result.AddRange(CircleCircleIntersections(cA2, cB2));

            return result;
        }

        /// <summary>
        /// Extracts all line segments from a shape.
        /// Returns empty list for Circles (handled by separate algorithms).
        /// </summary>
        private static List<(PointD a, PointD b)> GetSegments(ISnapProvider shape)
        {
            var segs = new List<(PointD, PointD)>();
            switch (shape)
            {
                case LineShape line:
                    segs.Add((line.Start, line.End));
                    break;
                case PolylineShape poly:
                    for (int i = 0; i < poly.Vertices.Count - 1; i++)
                        segs.Add((poly.Vertices[i], poly.Vertices[i + 1]));
                    if (poly.IsClosed && poly.Vertices.Count > 2)
                        segs.Add((poly.Vertices.Last(), poly.Vertices.First()));
                    break;
                case RectangleShape rect:
                    var c0 = rect.Start;
                    var c1 = new PointD(rect.End.X, rect.Start.Y);
                    var c2 = rect.End;
                    var c3 = new PointD(rect.Start.X, rect.End.Y);
                    segs.Add((c0, c1)); segs.Add((c1, c2));
                    segs.Add((c2, c3)); segs.Add((c3, c0));
                    break;
            }
            return segs;
        }

        /// <summary>
        /// Line segment – Circle intersection (quadratic formula).
        /// Returns 0, 1, or 2 snap points.
        /// </summary>
        private static IEnumerable<SnapPoint> LineCircleIntersections(
            PointD p1, PointD p2, CircleShape circle)
        {
            var result = new List<SnapPoint>();
            double cx = circle.Center.X, cy = circle.Center.Y, r = circle.GetRadius();
            double x1 = p1.X - cx, y1 = p1.Y - cy;
            double dx = p2.X - p1.X, dy = p2.Y - p1.Y;
            double a = dx * dx + dy * dy;
            double b = 2 * (x1 * dx + y1 * dy);
            double c = x1 * x1 + y1 * y1 - r * r;
            double disc = b * b - 4 * a * c;
            if (disc < 0) return result;
            double sq = System.Math.Sqrt(disc);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                double t = (-b + sign * sq) / (2 * a);
                if (t >= 0 && t <= 1)
                    result.Add(new SnapPoint(SnapType.Intersection,
                        new PointD(x1 + t * dx + cx, y1 + t * dy + cy), null));
            }
            return result;
        }

        /// <summary>
        /// Circle – Circle intersection (geometric radical-line approach).
        /// Returns 0 or 2 snap points.
        /// </summary>
        private static IEnumerable<SnapPoint> CircleCircleIntersections(
            CircleShape c1, CircleShape c2)
        {
            var result = new List<SnapPoint>();
            double x0 = c1.Center.X, y0 = c1.Center.Y, r0 = c1.GetRadius();
            double x1 = c2.Center.X, y1 = c2.Center.Y, r1 = c2.GetRadius();
            double dx = x1 - x0, dy = y1 - y0;
            double d = System.Math.Sqrt(dx * dx + dy * dy);
            if (d > r0 + r1 || d < System.Math.Abs(r0 - r1) || d == 0)
                return result;
            double a = (r0 * r0 - r1 * r1 + d * d) / (2 * d);
            double h = System.Math.Sqrt(r0 * r0 - a * a);
            double px = x0 + a * dx / d, py = y0 + a * dy / d;
            double ox = h * dy / d, oy = h * dx / d;
            result.Add(new SnapPoint(SnapType.Intersection, new PointD(px - ox, py + oy), null));
            result.Add(new SnapPoint(SnapType.Intersection, new PointD(px + ox, py - oy), null));
            return result;
        }

        /// <summary>
        /// Straight-line distance from a point to the centre of a shape's
        /// bounding box. Used to rank nearby shapes when capping for performance.
        /// </summary>
        private static double DistanceToShapeCenter(ISnapProvider shape, PointD point)
        {
            if (shape is IShape s)
            {
                var bb = s.GetBoundingBox();
                double dx = point.X - (bb.X + bb.Width / 2);
                double dy = point.Y - (bb.Y + bb.Height / 2);
                return System.Math.Sqrt(dx * dx + dy * dy);
            }
            return double.MaxValue;
        }
    }
}