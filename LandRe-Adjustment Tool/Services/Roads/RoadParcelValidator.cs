using Land_Readjustment_Tool.Core.Entities.Roads;
using Land_Readjustment_Tool.Infrastructure.Spatial;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Valid;

namespace Land_Readjustment_Tool.Services.Roads
{
    public class RoadParcelValidator
    {
        private readonly GeometryFactory _factory = SpatialConfig.Factory;

        public (DonutValidationStatus status, string message) Validate(RoadParcel road)
        {
            ArgumentNullException.ThrowIfNull(road);

            Polygon? polygon = road.Shape;
            if (polygon == null || polygon.IsEmpty)
            {
                return (DonutValidationStatus.InvalidGeometry, "Geometry is empty.");
            }

            if (!polygon.IsValid)
            {
                string reason = new IsValidOp(polygon).ValidationError?.Message ?? "unknown";
                return (DonutValidationStatus.InvalidGeometry, $"Geometry invalid: {reason}");
            }

            if (!NetTopologySuite.Algorithm.Orientation.IsCCW(polygon.ExteriorRing.Coordinates))
            {
                return (
                    DonutValidationStatus.WrongWindingDirection,
                    "Exterior ring is not Counter-Clockwise.");
            }

            Polygon outer = _factory.CreatePolygon(
                _factory.CreateLinearRing(polygon.ExteriorRing.Coordinates));

            double totalHoleArea = 0.0;
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                LineString ring = polygon.GetInteriorRingN(i);
                if (NetTopologySuite.Algorithm.Orientation.IsCCW(ring.Coordinates))
                {
                    return (
                        DonutValidationStatus.WrongWindingDirection,
                        $"Interior ring {i} is not Clockwise.");
                }

                Polygon hole = _factory.CreatePolygon(
                    _factory.CreateLinearRing(RingWindingHelper.EnsureCCW(ring.Coordinates)));

                if (!outer.Contains(hole))
                {
                    return (
                        DonutValidationStatus.HoleOutsideExterior,
                        $"Interior ring {i} lies outside the exterior boundary.");
                }

                for (int j = i + 1; j < polygon.NumInteriorRings; j++)
                {
                    Polygon hole2 = _factory.CreatePolygon(
                        _factory.CreateLinearRing(RingWindingHelper.EnsureCCW(
                            polygon.GetInteriorRingN(j).Coordinates)));

                    if (hole.Intersects(hole2) && !hole.Touches(hole2))
                    {
                        return (
                            DonutValidationStatus.HolesOverlap,
                            $"Interior rings {i} and {j} overlap.");
                    }
                }

                totalHoleArea += hole.Area;
            }

            if (totalHoleArea >= outer.Area)
            {
                return (
                    DonutValidationStatus.HoleAreaExceedsParcel,
                    "Total island area equals or exceeds the outer road parcel area.");
            }

            return (DonutValidationStatus.Valid, "OK");
        }

        public void ValidateAndApply(RoadParcel road)
        {
            var (status, message) = Validate(road);
            road.ValidationStatus = status;
            road.ValidationMessage = message;
        }
    }
}
