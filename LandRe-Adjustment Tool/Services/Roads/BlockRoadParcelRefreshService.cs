using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Layout;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;

namespace Land_Readjustment_Tool.Services.Roads
{
    public sealed class BlockRoadParcelRefreshService
    {
        private const string GeneratedBy = "BlockRoadParcelRefreshService";
        private const string GeneratedRoadParcelDescription =
            "Auto-generated road parcel from Project Boundary minus layout area layers.";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<BlockRoadParcelRefreshResult> RefreshAsync(
            AppDbContext context,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            CanvasLayer roadParcelLayer = await EnsureRoadParcelLayerAsync(context, ct);
            Polygon? projectBoundary = await LoadProjectBoundaryAsync(context, ct);
            if (projectBoundary == null)
            {
                int removedCount = await RemoveExistingRoadParcelObjectsAsync(
                    context,
                    roadParcelLayer,
                    ct);
                return new BlockRoadParcelRefreshResult(
                    false,
                    0,
                    removedCount,
                    "Project Boundary not found.");
            }

            List<Polygon> exclusionPolygons = await LoadRoadParcelExclusionPolygonsAsync(context, ct);
            if (exclusionPolygons.Count == 0)
            {
                int removedCount = await RemoveExistingRoadParcelObjectsAsync(
                    context,
                    roadParcelLayer,
                    ct);
                return new BlockRoadParcelRefreshResult(
                    false,
                    0,
                    removedCount,
                    "No Blocks, Open Spaces, or replotted area polygons found.");
            }

            List<Polygon> roadParcelPolygons;
            try
            {
                roadParcelPolygons = BuildRoadParcelPolygons(
                    projectBoundary,
                    exclusionPolygons);
            }
            catch (Exception ex)
            {
                return new BlockRoadParcelRefreshResult(
                    false,
                    0,
                    0,
                    $"Could not calculate Project Boundary minus layout area layers. {ex.Message}");
            }

            if (roadParcelPolygons.Count == 0)
            {
                int removedCount = await RemoveExistingRoadParcelObjectsAsync(
                    context,
                    roadParcelLayer,
                    ct);
                return new BlockRoadParcelRefreshResult(
                    false,
                    0,
                    removedCount,
                    "Project Boundary minus layout area layers did not produce road parcel geometry.");
            }

            List<CanvasObject> existingRoadParcelObjects = await context.CanvasObjects
                .Where(item => item.CanvasLayerId == roadParcelLayer.Id)
                .ToListAsync(ct);
            if (existingRoadParcelObjects.Count > 0)
                context.CanvasObjects.RemoveRange(existingRoadParcelObjects);

            DateTime now = DateTime.Now;
            int index = 1;
            foreach (Polygon polygon in roadParcelPolygons)
            {
                NormalizeSridForCanvasDatabase(polygon);
                CanvasObject roadParcelObject = new()
                {
                    CanvasLayerId = roadParcelLayer.Id,
                    CanvasLayer = roadParcelLayer,
                    ObjectType = "Polygon",
                    Shape = polygon,
                    GeometryMetadataJson = JsonSerializer.Serialize(
                        new GeneratedRoadParcelMetadata(
                            GeneratedBy,
                            "BlockLayoutRoadParcel",
                            now,
                            projectBoundary.Area,
                            exclusionPolygons.Count),
                        JsonOptions),
                    LabelText = roadParcelPolygons.Count == 1
                        ? "Road Parcel"
                        : $"Road Parcel {index}",
                    ObjectDescription = GeneratedRoadParcelDescription,
                    IsVisible = true,
                    IsLocked = true,
                    CreatedDate = now,
                    LastModifiedDate = now
                };
                await context.CanvasObjects.AddAsync(roadParcelObject, ct);
                index++;
            }

            roadParcelLayer.LastModifiedDate = now;
            await context.SaveChangesAsync(ct);

            return new BlockRoadParcelRefreshResult(
                true,
                roadParcelPolygons.Count,
                existingRoadParcelObjects.Count,
                null);
        }

        public static bool AffectsGeneratedRoadParcel(CanvasObject canvasObject)
        {
            CanvasLayer? layer = canvasObject.CanvasLayer;
            return canvasObject.BlockId.HasValue ||
                   (layer != null && IsRoadParcelDependencyLayer(layer));
        }

        public static bool IsBlockLayer(CanvasLayer layer) =>
            string.Equals(layer.LayerType, "Block", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(layer.Name, "Blocks", StringComparison.OrdinalIgnoreCase);

        public static bool IsRoadParcelLayer(CanvasLayer layer) =>
            string.Equals(layer.LayerType, "RoadParcel", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(layer.Name, "Road Parcel", StringComparison.OrdinalIgnoreCase);

        public static bool IsRoadParcelDependencyLayer(CanvasLayer layer)
        {
            if (IsRoadParcelLayer(layer))
                return false;

            return CanvasLayerTreeService.IsProjectBoundaryLayer(layer) ||
                   IsBlockLayer(layer) ||
                   IsReplotAreaExclusionLayer(layer);
        }

        private static async Task<int> RemoveExistingRoadParcelObjectsAsync(
            AppDbContext context,
            CanvasLayer roadParcelLayer,
            CancellationToken ct)
        {
            List<CanvasObject> existingRoadParcelObjects = await context.CanvasObjects
                .Where(item => item.CanvasLayerId == roadParcelLayer.Id)
                .ToListAsync(ct);
            if (existingRoadParcelObjects.Count > 0)
            {
                context.CanvasObjects.RemoveRange(existingRoadParcelObjects);
                await context.SaveChangesAsync(ct);
            }

            return existingRoadParcelObjects.Count;
        }

        private static async Task<CanvasLayer> EnsureRoadParcelLayerAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            CanvasLayer? layer = await context.CanvasLayers
                .OrderBy(item => item.DisplayOrder)
                .FirstOrDefaultAsync(item =>
                    item.Name == "Road Parcel" ||
                    item.LayerType == "RoadParcel",
                    ct);
            if (layer != null)
                return layer;

            int nextDisplayOrder = await context.CanvasLayers.AnyAsync(ct)
                ? await context.CanvasLayers.MaxAsync(item => item.DisplayOrder, ct) + 1
                : 0;

            layer = new CanvasLayer
            {
                Name = "Road Parcel",
                LayerType = "RoadParcel",
                IsVisible = true,
                IsLocked = true,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = nextDisplayOrder,
                BorderColor = "#D99A5A",
                FillColor = "#F6C766",
                LineWeight = 1.5,
                LineStyle = "Solid",
                LineTypeScale = 1.0,
                ShowFillTransparency = false,
                FillTransparency = 20,
                FillStyle = "Solid",
                LabelFontName = "Nirmala UI",
                LabelFontSize = 1.0,
                LabelColor = "#000000",
                LabelScaleWithZoom = true,
                TextAlignment = "Center Middle",
                PointSymbol = "Dot",
                PointSize = 5.0,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = "Default layer: Road Parcel"
            };

            await context.CanvasLayers.AddAsync(layer, ct);
            await context.SaveChangesAsync(ct);
            return layer;
        }

        private static async Task<Polygon?> LoadProjectBoundaryAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            List<Geometry> geometries = (await context.CanvasObjects
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Where(item =>
                        item.CanvasLayer != null &&
                        (item.CanvasLayer.Name == "Project Boundary" ||
                         item.CanvasLayer.LayerType == "ProjectBoundary"))
                    .ToListAsync(ct))
                .SelectMany(item => ExtractAreaGeometries(item.Shape))
                .ToList();

            return MergeToLargestPolygon(geometries);
        }

        private static async Task<List<Polygon>> LoadRoadParcelExclusionPolygonsAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            HashSet<Guid> blockLinkedObjectIds = (await context.Set<Block>()
                .AsNoTracking()
                .Where(block => block.CanvasObjectId.HasValue)
                .Select(block => block.CanvasObjectId!.Value)
                .ToListAsync(ct))
                .ToHashSet();

            List<CanvasObject> candidateObjects = await context.CanvasObjects
                .AsNoTracking()
                .Include(item => item.CanvasLayer)
                .Where(item => item.CanvasLayer != null)
                .ToListAsync(ct);

            List<CanvasObject> objects = candidateObjects
                .Where(item =>
                    item.CanvasLayer != null &&
                    IsRoadParcelExclusionLayer(
                        item.CanvasLayer,
                        item.BlockId.HasValue || blockLinkedObjectIds.Contains(item.Id)))
                .ToList();

            return objects
                .SelectMany(item => ExtractPolygons(item.Shape))
                .Where(polygon => polygon.Area > 0.000001)
                .Select(NormalizeCanvasPolygonWindings)
                .ToList();
        }

        private static List<Polygon> BuildRoadParcelPolygons(
            Polygon projectBoundary,
            IReadOnlyList<Polygon> exclusionPolygons)
        {
            List<Geometry> clippedExclusions = exclusionPolygons
                .Select(exclusion => NormalizeAreaGeometry(exclusion.Intersection(projectBoundary)))
                .Where(geometry => !geometry.IsEmpty)
                .ToList();

            Geometry roadGeometry = clippedExclusions.Count == 0
                ? NormalizeAreaGeometry(projectBoundary)
                : NormalizeAreaGeometry(projectBoundary.Difference(UnaryUnionOp.Union(clippedExclusions)));
            return ExtractPolygons(roadGeometry)
                .Where(polygon => polygon.Area > 0.000001)
                .Select(NormalizeCanvasPolygonWindings)
                .OrderByDescending(polygon => polygon.Area)
                .ToList();
        }

        private static bool IsRoadParcelExclusionLayer(CanvasLayer layer, bool isLinkedBlockObject)
        {
            if (IsRoadParcelLayer(layer) || CanvasLayerTreeService.IsProjectBoundaryLayer(layer))
                return false;

            return IsBlockLayer(layer) ||
                   IsReplotAreaExclusionLayer(layer) ||
                   isLinkedBlockObject;
        }

        private static bool IsReplotAreaExclusionLayer(CanvasLayer layer)
        {
            return layer.LayerType switch
            {
                "PrivateReplotParcel" => true,
                "PublicFacility" => true,
                "OpenSpace" => true,
                "ServiceSalesPlot" => true,
                "ReplottedParcel" => true,
                _ => false
            };
        }

        private static Polygon? MergeToLargestPolygon(IReadOnlyList<Geometry> geometries)
        {
            if (geometries.Count == 0)
                return null;

            Geometry merged = NormalizeAreaGeometry(UnaryUnionOp.Union(geometries.ToList()));
            return ExtractPolygons(merged)
                .OrderByDescending(polygon => polygon.Area)
                .FirstOrDefault();
        }

        private static IEnumerable<Geometry> ExtractAreaGeometries(Geometry? geometry)
        {
            if (geometry == null || geometry.IsEmpty)
                yield break;

            foreach (Polygon polygon in ExtractPolygons(geometry))
                yield return polygon;
        }

        private static IEnumerable<Polygon> ExtractPolygons(Geometry? geometry)
        {
            if (geometry == null || geometry.IsEmpty)
                yield break;

            if (geometry is Polygon polygon)
            {
                yield return polygon;
                yield break;
            }

            if (geometry is MultiPolygon multiPolygon)
            {
                for (int i = 0; i < multiPolygon.NumGeometries; i++)
                {
                    if (multiPolygon.GetGeometryN(i) is Polygon part)
                        yield return part;
                }

                yield break;
            }

            if (geometry is GeometryCollection collection)
            {
                for (int i = 0; i < collection.NumGeometries; i++)
                {
                    foreach (var p in ExtractPolygons(collection.GetGeometryN(i)))
                        yield return p;
                }
            }
        }

        private static Geometry NormalizeAreaGeometry(Geometry geometry)
        {
            Geometry normalized = geometry.Copy();
            normalized.SRID = 0;
            if (!normalized.IsValid)
                normalized = normalized.Buffer(0);

            if (normalized is Polygon normalizedPolygon)
                return NormalizeCanvasPolygonWindings(normalizedPolygon);

            return normalized;
        }

        private static Polygon NormalizeCanvasPolygonWindings(Polygon polygon)
        {
            Polygon normalized = RingWindingHelper.NormaliseWindings(polygon);
            NormalizeSridForCanvasDatabase(normalized);
            return normalized;
        }

        private static void NormalizeSridForCanvasDatabase(Geometry geometry)
        {
            geometry.SRID = 0;
            if (geometry is GeometryCollection collection)
            {
                for (int i = 0; i < collection.NumGeometries; i++)
                    NormalizeSridForCanvasDatabase(collection.GetGeometryN(i));
            }
        }

        private sealed record GeneratedRoadParcelMetadata(
            string GeneratedBy,
            string Kind,
            DateTime GeneratedAt,
            double ProjectBoundaryArea,
            int ExclusionPolygonCount);
    }

    public sealed record BlockRoadParcelRefreshResult(
        bool Created,
        int CreatedObjects,
        int RemovedObjects,
        string? SkippedReason);
}
