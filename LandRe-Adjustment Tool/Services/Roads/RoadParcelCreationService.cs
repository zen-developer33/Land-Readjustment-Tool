using Land_Readjustment_Tool.Infrastructure.Spatial;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;

namespace Land_Readjustment_Tool.Services.Roads
{
    public class RoadParcelCreationService
    {
        private readonly GeometryFactory _factory = SpatialConfig.Factory;

        public Polygon CreateDonutFromRoadAndIsland(
            Polygon roadOuter,
            Polygon islandInner)
        {
            if (!roadOuter.Contains(islandInner))
            {
                throw new ArgumentException(
                    "Island polygon must be fully inside the road outer polygon.",
                    nameof(islandInner));
            }

            Geometry result = roadOuter.Difference(islandInner);
            return NormalizePolygonResult(
                result,
                "Difference produced a MultiPolygon. The island may touch the road boundary; islands must be strictly inside.");
        }

        public Polygon BuildRoadFromCentreLine(
            Coordinate[] centreLine,
            double halfWidthMetres)
        {
            if (centreLine.Length < 2)
            {
                throw new ArgumentException(
                    "At least two centre-line coordinates are required.",
                    nameof(centreLine));
            }

            LineString line = _factory.CreateLineString(centreLine);
            Geometry buffered = line.Buffer(
                halfWidthMetres,
                new BufferParameters
                {
                    EndCapStyle = EndCapStyle.Flat,
                    JoinStyle = JoinStyle.Mitre,
                    MitreLimit = 5.0
                });

            return NormalizePolygonResult(buffered, "Road buffer did not produce a simple polygon.");
        }

        public Polygon AddIslandHole(Polygon existingRoad, Polygon newIsland)
        {
            if (!existingRoad.Contains(newIsland))
            {
                throw new ArgumentException(
                    "Island polygon must be fully inside the existing road polygon.",
                    nameof(newIsland));
            }

            Geometry result = existingRoad.Difference(newIsland);
            return NormalizePolygonResult(result, "Could not add island; result is not a simple polygon.");
        }

        public Polygon RemoveIslandHole(Polygon donutRoad, int holeIndex)
        {
            if (holeIndex < 0 || holeIndex >= donutRoad.NumInteriorRings)
            {
                throw new ArgumentOutOfRangeException(nameof(holeIndex));
            }

            LinearRing[] holes = Enumerable
                .Range(0, donutRoad.NumInteriorRings)
                .Where(index => index != holeIndex)
                .Select(index => _factory.CreateLinearRing(
                    donutRoad.GetInteriorRingN(index).Coordinates))
                .ToArray();

            Polygon result = _factory.CreatePolygon(
                _factory.CreateLinearRing(donutRoad.ExteriorRing.Coordinates),
                holes);

            return RingWindingHelper.NormaliseWindings(result);
        }

        private static Polygon NormalizePolygonResult(Geometry result, string errorMessage)
        {
            if (result is not Polygon polygon)
            {
                throw new InvalidOperationException(errorMessage);
            }

            return RingWindingHelper.NormaliseWindings(polygon);
        }
    }
}
