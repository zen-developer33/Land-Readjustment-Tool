using Land_Readjustment_Tool.Core.Entities.Roads;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Spatial;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Land_Readjustment_Tool.Services.Roads
{
    public class RoadParcelImportService
    {
        private readonly GeometryFactory _factory = SpatialConfig.Factory;
        private readonly AppDbContext? _context;
        private readonly RoadParcelValidator _validator;

        public RoadParcelImportService()
            : this(null, new RoadParcelValidator())
        {
        }

        public RoadParcelImportService(AppDbContext? context)
            : this(context, new RoadParcelValidator())
        {
        }

        public RoadParcelImportService(AppDbContext? context, RoadParcelValidator validator)
        {
            _context = context;
            _validator = validator;
        }

        public RoadParcel ImportFromWkt(string wkt, string parcelNumber, string roadName)
        {
            WKTReader reader = new(_factory);
            Geometry geometry = reader.Read(wkt);
            return BuildRoadParcel(
                ResolveToPolygon(geometry),
                parcelNumber,
                roadName,
                ImportSource.WKT);
        }

        public RoadParcel ImportFromWkb(byte[] wkb, string parcelNumber, string roadName)
        {
            WKBReader reader = new(NtsGeometryServices.Instance);
            Geometry geometry = reader.Read(wkb);
            return BuildRoadParcel(
                ResolveToPolygon(geometry),
                parcelNumber,
                roadName,
                ImportSource.WKB);
        }

        public RoadParcel ImportFromGeoJson(string geoJson, string parcelNumber, string roadName)
        {
            Geometry geometry = ReadGeoJsonGeometry(geoJson);
            return BuildRoadParcel(
                ResolveToPolygon(geometry),
                parcelNumber,
                roadName,
                ImportSource.GeoJSON);
        }

        public RoadParcel ImportManual(Polygon polygon, string parcelNumber, string roadName)
        {
            return BuildRoadParcel(
                ResolveToPolygon(polygon),
                parcelNumber,
                roadName,
                ImportSource.ManualEntry);
        }

        public async Task<RoadParcel> SaveAsync(
            RoadParcel road,
            CancellationToken cancellationToken = default)
        {
            if (_context == null)
            {
                throw new InvalidOperationException(
                    "A database context is required to save imported road parcels.");
            }

            _validator.ValidateAndApply(road);
            if (road.ValidationStatus is
                DonutValidationStatus.InvalidGeometry or
                DonutValidationStatus.HoleOutsideExterior or
                DonutValidationStatus.HolesOverlap or
                DonutValidationStatus.HoleAreaExceedsParcel)
            {
                throw new InvalidOperationException(
                    $"Road parcel '{road.RoadParcelNumber}' cannot be saved: {road.ValidationMessage}");
            }

            _context.RoadParcels.Add(road);
            await _context.SaveChangesAsync(cancellationToken);
            return road;
        }

        private RoadParcel BuildRoadParcel(
            Polygon polygon,
            string parcelNumber,
            string roadName,
            ImportSource source)
        {
            polygon = SimplifyLargePolygon(polygon);
            polygon = RingWindingHelper.NormaliseWindings(polygon);

            RoadParcel road = new()
            {
                RoadParcelNumber = parcelNumber,
                RoadName = roadName,
                Shape = polygon,
                RoadType = AutoDetectRoadType(polygon),
                ImportedFrom = source,
                ImportedAt = DateTime.UtcNow,
                ValidationStatus = DonutValidationStatus.NotChecked
            };

            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                Coordinate[] holeCoordinates = polygon.GetInteriorRingN(i).Coordinates;
                Polygon island = _factory.CreatePolygon(
                    _factory.CreateLinearRing(RingWindingHelper.EnsureCCW(holeCoordinates)));

                road.Islands.Add(new RoadIsland
                {
                    HoleIndex = i,
                    IslandShape = island,
                    IslandDescription = $"Island {i + 1} of {road.RoadName}"
                });
            }

            _validator.ValidateAndApply(road);
            return road;
        }

        private Geometry ReadGeoJsonGeometry(string geoJson)
        {
            JObject json = JObject.Parse(geoJson);
            string? type = json.Value<string>("type");
            JsonSerializer serializer = GeoJsonSerializer.Create(_factory);

            using JsonReader reader = json.CreateReader();
            if (string.Equals(type, "Feature", StringComparison.OrdinalIgnoreCase))
            {
                Feature feature = serializer.Deserialize<Feature>(reader)
                    ?? throw new InvalidDataException("GeoJSON feature did not contain geometry.");
                return feature.Geometry;
            }

            if (string.Equals(type, "FeatureCollection", StringComparison.OrdinalIgnoreCase))
            {
                FeatureCollection collection = serializer.Deserialize<FeatureCollection>(reader)
                    ?? throw new InvalidDataException("GeoJSON feature collection could not be read.");
                IFeature? firstFeature = collection.FirstOrDefault();
                return firstFeature?.Geometry
                    ?? throw new InvalidDataException("GeoJSON feature collection contains no geometry.");
            }

            return serializer.Deserialize<Geometry>(reader)
                ?? throw new InvalidDataException("GeoJSON geometry could not be read.");
        }

        private Polygon ResolveToPolygon(Geometry geometry)
        {
            if (geometry is Polygon polygon)
            {
                polygon.SRID = SpatialConfig.SRID;
                return polygon;
            }

            if (geometry is MultiPolygon multiPolygon)
            {
                Geometry merged = multiPolygon.GetGeometryN(0);
                for (int i = 1; i < multiPolygon.NumGeometries; i++)
                {
                    merged = merged.Union(multiPolygon.GetGeometryN(i));
                }

                if (merged is Polygon result)
                {
                    result.SRID = SpatialConfig.SRID;
                    return result;
                }

                throw new InvalidDataException(
                    "MultiPolygon parts could not be merged into a single polygon. " +
                    "Verify that the parts share edges and produce one contiguous road parcel.");
            }

            throw new InvalidDataException(
                $"Cannot import geometry of type '{geometry.GeometryType}' as a road parcel.");
        }

        private static Polygon SimplifyLargePolygon(Polygon polygon)
        {
            if (!HasLargeRing(polygon))
            {
                return polygon;
            }

            Geometry simplified = TopologyPreservingSimplifier.Simplify(polygon, 0.1);
            return simplified is Polygon simplifiedPolygon && simplifiedPolygon.IsValid
                ? simplifiedPolygon
                : polygon;
        }

        private static bool HasLargeRing(Polygon polygon)
        {
            if (polygon.ExteriorRing.NumPoints > 500)
            {
                return true;
            }

            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                if (polygon.GetInteriorRingN(i).NumPoints > 500)
                {
                    return true;
                }
            }

            return false;
        }

        private static RoadParcelType AutoDetectRoadType(Polygon polygon)
        {
            if (polygon.NumInteriorRings == 0)
            {
                return RoadParcelType.StraightRoad;
            }

            double perimeter = polygon.ExteriorRing.Length;
            if (perimeter > 0.0)
            {
                double compactness = (4.0 * Math.PI * polygon.Area) / (perimeter * perimeter);
                if (compactness > 0.55)
                {
                    return RoadParcelType.Roundabout;
                }
            }

            Envelope holeBounds = polygon.GetInteriorRingN(0).EnvelopeInternal;
            double minSide = Math.Min(holeBounds.Width, holeBounds.Height);
            double maxSide = Math.Max(holeBounds.Width, holeBounds.Height);
            if (minSide > 0.0 && maxSide / minSide > 4.0)
            {
                return RoadParcelType.MedianRoad;
            }

            return polygon.NumInteriorRings switch
            {
                1 => RoadParcelType.CulDeSac,
                >= 2 => RoadParcelType.Junction,
                _ => RoadParcelType.Unknown
            };
        }
    }
}
