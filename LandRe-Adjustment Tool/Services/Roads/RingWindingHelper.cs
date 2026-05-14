using Land_Readjustment_Tool.Infrastructure.Spatial;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Services.Roads
{
    public static class RingWindingHelper
    {
        public static Polygon NormaliseWindings(Polygon polygon)
        {
            ArgumentNullException.ThrowIfNull(polygon);

            GeometryFactory factory = SpatialConfig.Factory;
            Coordinate[] outer = EnsureCCW(polygon.ExteriorRing.Coordinates);
            LinearRing[] holes = new LinearRing[polygon.NumInteriorRings];

            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                holes[i] = factory.CreateLinearRing(
                    EnsureCW(polygon.GetInteriorRingN(i).Coordinates));
            }

            Polygon normalized = factory.CreatePolygon(
                factory.CreateLinearRing(outer),
                holes);
            normalized.SRID = SpatialConfig.SRID;
            return normalized;
        }

        public static Coordinate[] EnsureCCW(Coordinate[] coordinates)
        {
            ArgumentNullException.ThrowIfNull(coordinates);
            return NetTopologySuite.Algorithm.Orientation.IsCCW(coordinates)
                ? coordinates.Select(CloneCoordinate).ToArray()
                : coordinates.Reverse().Select(CloneCoordinate).ToArray();
        }

        public static Coordinate[] EnsureCW(Coordinate[] coordinates)
        {
            ArgumentNullException.ThrowIfNull(coordinates);
            return NetTopologySuite.Algorithm.Orientation.IsCCW(coordinates)
                ? coordinates.Reverse().Select(CloneCoordinate).ToArray()
                : coordinates.Select(CloneCoordinate).ToArray();
        }

        private static Coordinate CloneCoordinate(Coordinate coordinate) =>
            new(coordinate.X, coordinate.Y);
    }
}
