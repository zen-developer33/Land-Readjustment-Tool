using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    public sealed class DonutPolygonShape : Shape
    {
        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 0);

        public DonutPolygonShape(
            IEnumerable<PointD> exteriorRing,
            IEnumerable<IEnumerable<PointD>> interiorRings)
        {
            ExteriorRing = exteriorRing.ToList();
            InteriorRings = interiorRings
                .Select(ring => ring.ToList())
                .Where(ring => ring.Count >= 3)
                .ToList();
        }

        private DonutPolygonShape(DonutPolygonShape source)
            : base(source)
        {
            ExteriorRing = source.ExteriorRing.ToList();
            InteriorRings = source.InteriorRings
                .Select(ring => ring.ToList())
                .ToList();
        }

        public List<PointD> ExteriorRing { get; }

        public List<List<PointD>> InteriorRings { get; }

        protected override RectangleD ComputeBoundingBox()
        {
            if (ExteriorRing.Count == 0)
            {
                return new RectangleD(0, 0, 0, 0);
            }

            double minX = ExteriorRing.Min(point => point.X);
            double minY = ExteriorRing.Min(point => point.Y);
            double maxX = ExteriorRing.Max(point => point.X);
            double maxY = ExteriorRing.Max(point => point.Y);
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public GraphicsPath CreateScreenPath(
            Func<PointD, PointD> worldToScreen,
            RectangleD? clipWorldBounds = null)
        {
            GraphicsPath path = new() { FillMode = System.Drawing.Drawing2D.FillMode.Alternate };
            AddRing(path, ExteriorRing, worldToScreen, clipWorldBounds);

            foreach (List<PointD> ring in InteriorRings)
            {
                AddRing(path, ring, worldToScreen, clipWorldBounds);
            }

            return path;
        }

        public Polygon ToGeometry()
        {
            LinearRing shell = GeometryFactory.CreateLinearRing(ToClosedCoordinates(ExteriorRing));
            LinearRing[] holes = InteriorRings
                .Select(ring => GeometryFactory.CreateLinearRing(ToClosedCoordinates(ring)))
                .ToArray();
            return GeometryFactory.CreatePolygon(shell, holes);
        }

        public override void Draw(Graphics graphics, Func<PointD, PointD> worldToScreen, bool isPreview = false)
        {
            if (ExteriorRing.Count < 3)
            {
                return;
            }

            using GraphicsPath path = CreateScreenPath(worldToScreen);
            if (path.PointCount == 0)
            {
                return;
            }

            using SolidBrush fill = new(isPreview ? Color.FromArgb(32, FillColor) : FillColor);
            using Pen pen = new(
                isPreview ? Color.LightGray : IsSelected ? Color.Yellow : BorderColor,
                IsSelected ? 2.0f : 0.75f)
            {
                DashStyle = isPreview ? DashStyle.Dash : DashStyle.Solid
            };

            graphics.FillPath(fill, path);
            graphics.DrawPath(pen, path);
        }

        public override IShape Clone() => new DonutPolygonShape(this);

        /// <summary>
        /// Moves the exterior and interior polygon rings by the supplied world-coordinate delta.
        /// </summary>
        /// <param name="delta">The distance to add to every ring coordinate.</param>
        public override void Translate(PointD delta)
        {
            TranslateRing(ExteriorRing, delta);
            foreach (List<PointD> ring in InteriorRings)
            {
                TranslateRing(ring, delta);
            }

            InvalidateBounds();
        }

        public override bool ContainsPoint(PointD worldPoint, float tolerance)
        {
            Geometry geometry = ToGeometry();
            NetTopologySuite.Geometries.Point point = GeometryFactory.CreatePoint(
                new Coordinate(worldPoint.X, worldPoint.Y));

            if (geometry.Contains(point))
            {
                return true;
            }

            return tolerance > 0 &&
                   geometry.Boundary.Distance(point) <= tolerance;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            foreach (PointD point in ExteriorRing)
            {
                yield return new SnapPoint(SnapType.Endpoint, point, this);
            }

            foreach (List<PointD> ring in InteriorRings)
            {
                foreach (PointD point in ring)
                {
                    yield return new SnapPoint(SnapType.Endpoint, point, this);
                }
            }
        }

        private static void AddRing(
            GraphicsPath path,
            IReadOnlyList<PointD> ring,
            Func<PointD, PointD> worldToScreen,
            RectangleD? clipWorldBounds)
        {
            if (ring.Count < 3)
            {
                return;
            }

            IReadOnlyList<PointD> worldPoints = clipWorldBounds.HasValue
                ? ViewportClip.ClipPolygon(ring, clipWorldBounds.Value)
                : ring;
            if (worldPoints.Count < 3)
            {
                return;
            }

            PointF[] points = worldPoints
                .Select(worldToScreen)
                .Select(point => new PointF((float)Math.Round(point.X), (float)Math.Round(point.Y)))
                .Where(point => float.IsFinite(point.X) && float.IsFinite(point.Y))
                .ToArray();

            if (points.Length >= 3)
            {
                path.AddPolygon(points);
            }
        }

        private static void TranslateRing(IList<PointD> ring, PointD delta)
        {
            for (int i = 0; i < ring.Count; i++)
            {
                PointD point = ring[i];
                ring[i] = new PointD(point.X + delta.X, point.Y + delta.Y);
            }
        }

        private static Coordinate[] ToClosedCoordinates(IReadOnlyList<PointD> ring)
        {
            if (ring.Count == 0)
            {
                return [];
            }

            List<Coordinate> coordinates = ring
                .Select(point => new Coordinate(point.X, point.Y))
                .ToList();

            Coordinate first = coordinates[0];
            Coordinate last = coordinates[^1];
            if (!first.Equals2D(last))
            {
                coordinates.Add(new Coordinate(first.X, first.Y));
            }

            return coordinates.ToArray();
        }
    }
}
