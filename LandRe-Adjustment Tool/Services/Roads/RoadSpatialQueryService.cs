using Land_Readjustment_Tool.Core.Entities.Roads;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Spatial;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Services.Roads
{
    public class RoadSpatialQueryService
    {
        private readonly AppDbContext _context;
        private readonly GeometryFactory _factory = SpatialConfig.Factory;

        public RoadSpatialQueryService(AppDbContext context)
        {
            _context = context;
        }

        public bool IsPointOnRoad(RoadParcel road, Coordinate point) =>
            road.Shape.Contains(_factory.CreatePoint(point));

        public async Task<List<RoadParcel>> GetDonutRoadParcelsAsync(
            CancellationToken cancellationToken = default)
        {
            List<RoadParcel> roads = await _context.RoadParcels
                .Include(road => road.Islands)
                .ToListAsync(cancellationToken);

            return roads
                .Where(road => road.IsDonut)
                .ToList();
        }

        public async Task<RoadParcel?> HitTestAsync(
            Coordinate clickCoordinate,
            CancellationToken cancellationToken = default)
        {
            NetTopologySuite.Geometries.Point point = _factory.CreatePoint(clickCoordinate);
            List<RoadParcel> roads = await _context.RoadParcels
                .Include(road => road.Islands)
                .ToListAsync(cancellationToken);

            return roads.FirstOrDefault(road => road.Shape.Contains(point));
        }

        public async Task<List<Parcel>> GetAdjacentParcelsAsync(
            RoadParcel road,
            CancellationToken cancellationToken = default)
        {
            Geometry boundary = road.Shape.Boundary;
            List<Parcel> parcels = await _context.Parcels.ToListAsync(cancellationToken);
            return parcels
                .Where(parcel => parcel.Shape.Intersects(boundary))
                .ToList();
        }

        public async Task<Parcel?> FindIslandParcelAsync(
            RoadParcel road,
            int holeIndex,
            CancellationToken cancellationToken = default)
        {
            if (holeIndex < 0 || holeIndex >= road.Shape.NumInteriorRings)
            {
                return null;
            }

            Polygon hole = _factory.CreatePolygon(
                _factory.CreateLinearRing(RingWindingHelper.EnsureCCW(
                    road.Shape.GetInteriorRingN(holeIndex).Coordinates)));

            List<Parcel> parcels = await _context.Parcels.ToListAsync(cancellationToken);
            return parcels.FirstOrDefault(parcel =>
                hole.Contains(parcel.Shape) || parcel.Shape.EqualsTopologically(hole));
        }

        public async Task<List<RoadParcel>> GetRoadParcelsInViewAsync(
            Envelope envelope,
            CancellationToken cancellationToken = default)
        {
            Geometry viewport = _factory.ToGeometry(envelope);
            List<RoadParcel> roads = await _context.RoadParcels
                .Include(road => road.Islands)
                .ToListAsync(cancellationToken);

            return roads
                .Where(road =>
                    road.Shape.EnvelopeInternal.Intersects(envelope) &&
                    road.Shape.Intersects(viewport))
                .ToList();
        }
    }
}
