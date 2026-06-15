using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    public class PolylineShape : Shape, ISnapProvider
    {
        public sealed class PolylineSegment
        {
            public PolylineSegmentKind Kind { get; set; }
            public PointD Start { get; set; }
            public PointD End { get; set; }
            public ArcShape? Arc { get; set; }

            public PolylineSegment()
            {
            }

            public PolylineSegment(PolylineSegmentKind kind, PointD start, PointD end, ArcShape? arc = null)
            {
                Kind = kind;
                Start = start;
                End = end;
                Arc = arc;
            }
        }

        public enum PolylineSegmentKind
        {
            Line,
            Arc
        }

        private List<PointD> _vertices = new();
        private List<PolylineSegment> _segments = new();
        private bool _isClosed;

        public List<PointD> Vertices
        {
            get => _vertices;
            set
            {
                _vertices = value ?? new List<PointD>();
                InvalidateBounds();
            }
        }

        public List<PolylineSegment> Segments
        {
            get => _segments;
            set
            {
                _segments = value ?? new List<PolylineSegment>();
                InvalidateBounds();
            }
        }

        public bool IsClosed
        {
            get => _isClosed;
            set
            {
                _isClosed = value;
                InvalidateBounds();
            }
        }

        public PolylineShape(IEnumerable<PointD> points, bool isClosed = false)
            : this(points, Array.Empty<PolylineSegment>(), isClosed)
        {
        }

        public PolylineShape(
            IEnumerable<PointD> points,
            IEnumerable<PolylineSegment> segments,
            bool isClosed = false)
        {
            Vertices = new List<PointD>(points);
            Segments = new List<PolylineSegment>(segments);
            IsClosed = isClosed;
        }

        protected override RectangleD ComputeBoundingBox()
        {
            IEnumerable<PointD> points = Segments.Count > 0
                ? GetGeometryPoints(32)
                : Vertices;

            using IEnumerator<PointD> enumerator = points.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return new RectangleD(0, 0, 0, 0);
            }

            double minX = enumerator.Current.X;
            double minY = enumerator.Current.Y;
            double maxX = enumerator.Current.X;
            double maxY = enumerator.Current.Y;

            while (enumerator.MoveNext())
            {
                PointD point = enumerator.Current;
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public GraphicsPath CreateScreenPath(
            Func<PointD, PointD> worldToScreen,
            RectangleD? clipWorldBounds = null)
        {
            GraphicsPath path = new();

            if (Vertices.Count == 0)
            {
                return path;
            }

            if (IsClosed && Vertices.Count >= 3 && clipWorldBounds.HasValue)
            {
                IReadOnlyList<PointD> ring = Segments.Count > 0
                    ? GetGeometryPoints(64).ToList()
                    : Vertices;
                AddClippedPolygon(path, ring, worldToScreen, clipWorldBounds.Value);
                return path;
            }

            if (Segments.Count == 0)
            {
                if (Vertices.Count >= 2)
                {
                    AddClippedPolyline(path, Vertices, IsClosed, worldToScreen, clipWorldBounds);
                }

                return path;
            }

            foreach (PolylineSegment segment in Segments)
            {
                if (segment.Kind == PolylineSegmentKind.Line || segment.Arc == null)
                {
                    AddClippedLine(path, segment.Start, segment.End, worldToScreen, clipWorldBounds);
                    continue;
                }

                ArcShape arc = segment.Arc;
                if (clipWorldBounds.HasValue)
                {
                    AddSampledArc(path, arc, worldToScreen, clipWorldBounds.Value);
                    continue;
                }

                PointD centerScreenD = worldToScreen(arc.Center);
                PointD radiusScreenD = worldToScreen(new PointD(arc.Center.X + arc.Radius, arc.Center.Y));
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
                    continue;
                }

                RectangleF bounds = new(
                    centerX - radius,
                    centerY - radius,
                    radius * 2.0f,
                    radius * 2.0f);
                if (!IsValidRect(bounds))
                {
                    continue;
                }

                float startAngleDegrees = (float)(-arc.StartAngleRadians * 180.0 / Math.PI);
                float sweepAngleDegrees = (float)(-arc.SweepAngleRadians * 180.0 / Math.PI);
                if (float.IsFinite(startAngleDegrees) &&
                    float.IsFinite(sweepAngleDegrees) &&
                    Math.Abs(sweepAngleDegrees) >= 0.001f)
                {
                    path.AddArc(bounds, startAngleDegrees, sweepAngleDegrees);
                }
            }

            if (IsClosed && Vertices.Count > 2)
            {
                // Use the actual end of the last segment (arc endpoint may differ from Vertices[^1])
                PointD lastEndWorld = Segments.Count > 0 ? Segments[^1].End : Vertices[^1];
                AddClippedLine(path, lastEndWorld, Vertices[0], worldToScreen, clipWorldBounds);
            }

            return path;
        }

        public override void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false)
        {
            if (Vertices.Count < 2)
            {
                return;
            }

            using GraphicsPath path = CreateScreenPath(worldToScreen);
            if (path.PointCount == 0)
            {
                return;
            }

            float penWidth = IsSelected ? 2f : 0.25f;
            using Pen pen = new(isPreview ? Color.LightGray : (IsSelected ? Color.Yellow : BorderColor), penWidth)
            {
                DashStyle = isPreview ? DashStyle.Dash : DashStyle.Solid
            };

            g.DrawPath(pen, path);
        }

        public override IShape Clone()
        {
            return new PolylineShape(
                new List<PointD>(Vertices),
                Segments.Select(segment => new PolylineSegment(
                    segment.Kind,
                    segment.Start,
                    segment.End,
                    segment.Arc == null
                        ? null
                        : new ArcShape(segment.Arc.Center, segment.Arc.Radius, segment.Arc.StartAngleRadians, segment.Arc.SweepAngleRadians))).ToList(),
                IsClosed)
            {
                BorderColor = this.BorderColor,
                FillColor = this.FillColor,
                LayerName = this.LayerName
            };
        }

        /// <summary>
        /// Moves every vertex and stored segment point by the supplied world-coordinate delta.
        /// </summary>
        /// <param name="delta">The distance to add to each polyline coordinate.</param>
        public override void Translate(PointD delta)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                PointD point = Vertices[i];
                Vertices[i] = new PointD(point.X + delta.X, point.Y + delta.Y);
            }

            foreach (PolylineSegment segment in Segments)
            {
                segment.Start = new PointD(segment.Start.X + delta.X, segment.Start.Y + delta.Y);
                segment.End = new PointD(segment.End.X + delta.X, segment.End.Y + delta.Y);
                segment.Arc?.Translate(delta);
            }

            InvalidateBounds();
        }

        public override bool ContainsPoint(PointD worldPoint, float tolerance)
        {
            foreach (PolylineSegment segment in EnumerateEffectiveSegments())
            {
                if (segment.Kind == PolylineSegmentKind.Arc && segment.Arc != null)
                {
                    PointD[] samples = segment.Arc.SamplePoints(64).ToArray();
                    for (int i = 0; i < samples.Length - 1; i++)
                    {
                        if (PointToSegmentDistance(worldPoint, samples[i], samples[i + 1]) <= tolerance)
                        {
                            return true;
                        }
                    }
                }
                else if (PointToSegmentDistance(worldPoint, segment.Start, segment.End) <= tolerance)
                {
                    return true;
                }
            }

            return false;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            if (Vertices.Count == 0)
            {
                yield break;
            }

            if (Segments.Count == 0)
            {
                foreach (PointD vertex in Vertices)
                {
                    yield return new SnapPoint(SnapType.Endpoint, vertex, this);
                }
            }
            else
            {
                List<PointD> emittedEndpoints = new();
                foreach (PolylineSegment segment in Segments)
                {
                    foreach (PointD endpoint in new[] { segment.Start, segment.End })
                    {
                        if (emittedEndpoints.Any(existing => Distance(existing, endpoint) <= 1e-9))
                        {
                            continue;
                        }

                        emittedEndpoints.Add(endpoint);
                        yield return new SnapPoint(SnapType.Endpoint, endpoint, this);
                    }
                }
            }

            foreach (PolylineSegment segment in EnumerateEffectiveSegments())
            {
                if (segment.Kind == PolylineSegmentKind.Arc && segment.Arc != null)
                {
                    yield return new SnapPoint(SnapType.Midpoint, segment.Arc.MidPoint, this);
                    continue;
                }

                yield return new SnapPoint(
                    SnapType.Midpoint,
                    new PointD((segment.Start.X + segment.End.X) / 2.0, (segment.Start.Y + segment.End.Y) / 2.0),
                    this);
            }
        }

        public IEnumerable<PointD> GetGeometryPoints(int arcSamples = 24)
        {
            if (Segments.Count == 0)
            {
                foreach (PointD vertex in Vertices)
                {
                    yield return vertex;
                }

                yield break;
            }

            if (Vertices.Count > 0)
            {
                yield return Vertices[0];
            }

            foreach (PolylineSegment segment in Segments)
            {
                if (segment.Kind == PolylineSegmentKind.Arc && segment.Arc != null)
                {
                    foreach (PointD point in segment.Arc.SamplePoints(Math.Max(2, arcSamples)).Skip(1))
                    {
                        yield return point;
                    }
                }
                else
                {
                    yield return segment.End;
                }
            }
        }

        private IEnumerable<PolylineSegment> EnumerateEffectiveSegments()
        {
            if (Segments.Count > 0)
            {
                foreach (PolylineSegment segment in Segments)
                {
                    yield return segment;
                }

                if (IsClosed && Vertices.Count > 2)
                {
                    PointD lastEnd = Segments[^1].End;
                    PointD first = Vertices[0];
                    if (Distance(lastEnd, first) > 1e-9)
                    {
                        yield return new PolylineSegment(PolylineSegmentKind.Line, lastEnd, first);
                    }
                }

                yield break;
            }

            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                yield return new PolylineSegment(PolylineSegmentKind.Line, Vertices[i], Vertices[i + 1]);
            }

            if (IsClosed && Vertices.Count > 2)
            {
                yield return new PolylineSegment(PolylineSegmentKind.Line, Vertices[^1], Vertices[0]);
            }
        }

        private static PointF ToRoundedScreenPoint(PointD point)
        {
            // Do NOT round — integer rounding creates sub-pixel gaps between line-segment
            // endpoints and arc-segment start/end points, causing GDI+ to stitch them with
            // unwanted straight lines that make arcs appear polygonal.
            return new PointF((float)point.X, (float)point.Y);
        }

        private static bool IsValidPoint(PointF point)
        {
            return float.IsFinite(point.X) && float.IsFinite(point.Y);
        }

        private static bool IsValidRect(RectangleF rect)
        {
            return float.IsFinite(rect.Left) &&
                   float.IsFinite(rect.Top) &&
                   float.IsFinite(rect.Right) &&
                   float.IsFinite(rect.Bottom) &&
                   rect.Width > 0.0f &&
                   rect.Height > 0.0f;
        }

        private static void AddClippedPolyline(
            GraphicsPath path,
            IReadOnlyList<PointD> vertices,
            bool close,
            Func<PointD, PointD> worldToScreen,
            RectangleD? clipWorldBounds)
        {
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                AddClippedLine(path, vertices[i], vertices[i + 1], worldToScreen, clipWorldBounds);
            }

            if (close && vertices.Count > 2)
            {
                AddClippedLine(path, vertices[^1], vertices[0], worldToScreen, clipWorldBounds);
            }
        }

        private static void AddClippedLine(
            GraphicsPath path,
            PointD startWorld,
            PointD endWorld,
            Func<PointD, PointD> worldToScreen,
            RectangleD? clipWorldBounds)
        {
            if (clipWorldBounds.HasValue &&
                !ViewportClip.ClipSegment(startWorld, endWorld, clipWorldBounds.Value, out startWorld, out endWorld))
            {
                return;
            }

            PointF start = ToRoundedScreenPoint(worldToScreen(startWorld));
            PointF end = ToRoundedScreenPoint(worldToScreen(endWorld));
            if (!IsValidPoint(start) || !IsValidPoint(end) || Distance(start, end) < 0.25)
            {
                return;
            }

            path.StartFigure();
            path.AddLine(start, end);
        }

        private static void AddClippedPolygon(
            GraphicsPath path,
            IReadOnlyList<PointD> ring,
            Func<PointD, PointD> worldToScreen,
            RectangleD clipWorldBounds)
        {
            List<PointD> clipped = ViewportClip.ClipPolygon(ring, clipWorldBounds);
            if (clipped.Count < 3)
            {
                return;
            }

            PointF[] points = clipped
                .Select(point => ToRoundedScreenPoint(worldToScreen(point)))
                .Where(IsValidPoint)
                .ToArray();
            if (points.Length >= 3)
            {
                path.AddPolygon(points);
            }
        }

        private static void AddSampledArc(
            GraphicsPath path,
            ArcShape arc,
            Func<PointD, PointD> worldToScreen,
            RectangleD clipWorldBounds)
        {
            PointD[] samples = arc.SamplePoints(64).ToArray();
            PointF lastEnd = default;
            bool hasVisibleRun = false;

            for (int i = 0; i < samples.Length - 1; i++)
            {
                PointD startWorld = samples[i];
                PointD endWorld = samples[i + 1];
                if (!ViewportClip.ClipSegment(startWorld, endWorld, clipWorldBounds, out startWorld, out endWorld))
                {
                    hasVisibleRun = false;
                    continue;
                }

                PointF start = ToRoundedScreenPoint(worldToScreen(startWorld));
                PointF end = ToRoundedScreenPoint(worldToScreen(endWorld));
                if (!IsValidPoint(start) || !IsValidPoint(end) || Distance(start, end) < 0.25)
                {
                    continue;
                }

                if (!hasVisibleRun || Distance(lastEnd, start) > 0.5)
                {
                    path.StartFigure();
                }

                path.AddLine(start, end);
                lastEnd = end;
                hasVisibleRun = true;
            }
        }

        private static double Distance(PointF first, PointF second)
        {
            double dx = second.X - first.X;
            double dy = second.Y - first.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double Distance(PointD first, PointD second)
        {
            double dx = second.X - first.X;
            double dy = second.Y - first.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
