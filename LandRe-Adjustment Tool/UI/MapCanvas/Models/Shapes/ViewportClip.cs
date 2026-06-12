namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    internal static class ViewportClip
    {
        public static bool ClipSegment(
            PointD a,
            PointD b,
            RectangleD clip,
            out PointD outA,
            out PointD outB)
        {
            double x0 = a.X;
            double y0 = a.Y;
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double t0 = 0.0;
            double t1 = 1.0;

            double xmin = Math.Min(clip.Left, clip.Right);
            double xmax = Math.Max(clip.Left, clip.Right);
            double ymin = Math.Min(clip.Top, clip.Bottom);
            double ymax = Math.Max(clip.Top, clip.Bottom);

            Span<double> p = stackalloc double[] { -dx, dx, -dy, dy };
            Span<double> q = stackalloc double[] { x0 - xmin, xmax - x0, y0 - ymin, ymax - y0 };

            for (int i = 0; i < 4; i++)
            {
                if (Math.Abs(p[i]) < double.Epsilon)
                {
                    if (q[i] < 0.0)
                    {
                        outA = default;
                        outB = default;
                        return false;
                    }

                    continue;
                }

                double r = q[i] / p[i];
                if (p[i] < 0.0)
                {
                    if (r > t1)
                    {
                        outA = default;
                        outB = default;
                        return false;
                    }

                    if (r > t0)
                    {
                        t0 = r;
                    }
                }
                else
                {
                    if (r < t0)
                    {
                        outA = default;
                        outB = default;
                        return false;
                    }

                    if (r < t1)
                    {
                        t1 = r;
                    }
                }
            }

            outA = new PointD(x0 + t0 * dx, y0 + t0 * dy);
            outB = new PointD(x0 + t1 * dx, y0 + t1 * dy);
            return true;
        }

        public static List<PointD> ClipPolygon(IReadOnlyList<PointD> polygon, RectangleD clip)
        {
            if (polygon.Count == 0)
            {
                return [];
            }

            double xmin = Math.Min(clip.Left, clip.Right);
            double xmax = Math.Max(clip.Left, clip.Right);
            double ymin = Math.Min(clip.Top, clip.Bottom);
            double ymax = Math.Max(clip.Top, clip.Bottom);

            List<PointD> output = new(polygon);
            output = ClipEdge(output, point => point.X >= xmin, (start, end) => IntersectX(start, end, xmin));
            output = ClipEdge(output, point => point.X <= xmax, (start, end) => IntersectX(start, end, xmax));
            output = ClipEdge(output, point => point.Y >= ymin, (start, end) => IntersectY(start, end, ymin));
            output = ClipEdge(output, point => point.Y <= ymax, (start, end) => IntersectY(start, end, ymax));
            return output;
        }

        private static List<PointD> ClipEdge(
            List<PointD> input,
            Func<PointD, bool> inside,
            Func<PointD, PointD, PointD> intersect)
        {
            List<PointD> result = new();
            if (input.Count == 0)
            {
                return result;
            }

            PointD start = input[^1];
            foreach (PointD end in input)
            {
                bool startInside = inside(start);
                bool endInside = inside(end);
                if (endInside)
                {
                    if (!startInside)
                    {
                        result.Add(intersect(start, end));
                    }

                    result.Add(end);
                }
                else if (startInside)
                {
                    result.Add(intersect(start, end));
                }

                start = end;
            }

            return result;
        }

        private static PointD IntersectX(PointD start, PointD end, double x)
        {
            double dx = end.X - start.X;
            if (Math.Abs(dx) < double.Epsilon)
            {
                return new PointD(x, start.Y);
            }

            double t = (x - start.X) / dx;
            return new PointD(x, start.Y + t * (end.Y - start.Y));
        }

        private static PointD IntersectY(PointD start, PointD end, double y)
        {
            double dy = end.Y - start.Y;
            if (Math.Abs(dy) < double.Epsilon)
            {
                return new PointD(start.X, y);
            }

            double t = (y - start.Y) / dy;
            return new PointD(start.X + t * (end.X - start.X), y);
        }
    }
}
