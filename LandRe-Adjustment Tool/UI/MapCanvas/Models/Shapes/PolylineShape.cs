using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    public class PolylineShape : Shape, ISnapProvider
    {
        private const double MaxScreenCoordinate = 1_000_000.0;
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

        public List<PointD> Vertices { get; set; }
        public List<PolylineSegment> Segments { get; set; }
        public bool IsClosed { get; set; }

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

        public override RectangleD GetBoundingBox()
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

        public GraphicsPath CreateScreenPath(Func<PointD, PointD> worldToScreen)
        {
            GraphicsPath path = new();

            if (Vertices.Count == 0)
            {
                return path;
            }

            if (Segments.Count == 0)
            {
                if (Vertices.Count >= 2)
                {
                    PointF[] points = Vertices
                        .Select(point => ToRoundedScreenPoint(worldToScreen(point)))
                        .ToArray();
                    if (points.All(IsSafePoint))
                    {
                        path.AddLines(points);
                        if (IsClosed && points.Length > 2)
                        {
                            path.AddLine(points[^1], points[0]);
                        }
                    }
                }

                return path;
            }

            foreach (PolylineSegment segment in Segments)
            {
                if (segment.Kind == PolylineSegmentKind.Line || segment.Arc == null)
                {
                    PointF start = ToRoundedScreenPoint(worldToScreen(segment.Start));
                    PointF end = ToRoundedScreenPoint(worldToScreen(segment.End));
                    if (IsSafePoint(start) && IsSafePoint(end))
                    {
                        path.AddLine(start, end);
                    }

                    continue;
                }

                ArcShape arc = segment.Arc;
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
                if (!IsSafeRect(bounds))
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
                PointF last = ToRoundedScreenPoint(worldToScreen(lastEndWorld));
                PointF first = ToRoundedScreenPoint(worldToScreen(Vertices[0]));
                if (IsSafePoint(last) && IsSafePoint(first))
                {
                    path.AddLine(last, first);
                }
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

            foreach (PointD vertex in Vertices)
            {
                yield return new SnapPoint(SnapType.Endpoint, vertex, this);
            }

            foreach (PolylineSegment segment in EnumerateEffectiveSegments())
            {
                if (segment.Kind == PolylineSegmentKind.Arc && segment.Arc != null)
                {
                    yield return new SnapPoint(SnapType.Midpoint, segment.Arc.MidPoint, this);
                    yield return new SnapPoint(SnapType.Center, segment.Arc.Center, this);
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

        private static bool IsSafePoint(PointF point)
        {
            return IsValidPoint(point) &&
                   Math.Abs(point.X) <= MaxScreenCoordinate &&
                   Math.Abs(point.Y) <= MaxScreenCoordinate;
        }

        private static bool IsSafeRect(RectangleF rect)
        {
            return float.IsFinite(rect.Left) &&
                   float.IsFinite(rect.Top) &&
                   float.IsFinite(rect.Right) &&
                   float.IsFinite(rect.Bottom) &&
                   Math.Abs(rect.Left) <= MaxScreenCoordinate &&
                   Math.Abs(rect.Top) <= MaxScreenCoordinate &&
                   Math.Abs(rect.Right) <= MaxScreenCoordinate &&
                   Math.Abs(rect.Bottom) <= MaxScreenCoordinate;
        }

        private static double Distance(PointD first, PointD second)
        {
            double dx = second.X - first.X;
            double dy = second.Y - first.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
