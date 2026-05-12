using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Services.Import.Readers
{
    internal static class BoundaryGeometryReaderHelpers
    {
        public static Geometry? ValidatePolygonalGeometry(Geometry? geometry)
        {
            if (geometry == null || geometry.IsEmpty)
                return null;

            if (geometry is not Polygon && geometry is not MultiPolygon)
                return null;

            if (geometry.IsValid)
                return geometry;

            Geometry fixedGeometry = geometry.Buffer(0);
            if (fixedGeometry.IsEmpty || !fixedGeometry.IsValid)
                return null;

            return fixedGeometry is Polygon or MultiPolygon
                ? fixedGeometry
                : null;
        }

        public static Coordinate[] CloseRing(IReadOnlyList<Coordinate> coordinates)
        {
            if (coordinates.Count == 0)
                return [];

            List<Coordinate> ring = coordinates
                .Select(coordinate => new Coordinate(coordinate.X, coordinate.Y))
                .ToList();

            Coordinate first = ring[0];
            Coordinate last = ring[^1];
            if (!first.Equals2D(last))
                ring.Add(new Coordinate(first.X, first.Y));

            return ring.ToArray();
        }
    }
}
