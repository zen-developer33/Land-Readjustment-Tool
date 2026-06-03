using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Layout;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Services.Assignment
{
    public interface IRoadCenterlineAssignmentService
    {
        Task<IReadOnlyList<RoadCenterlineAssignmentCandidate>> GetCandidatesAsync(
            ProjectSession session,
            CancellationToken ct = default);

        Task<IReadOnlyList<RoadRecordChoice>> GetRoadsAsync(
            ProjectSession session,
            CancellationToken ct = default);

        Task<int> AssignBySourceLayerAsync(
            ProjectSession session,
            IReadOnlyDictionary<string, int> sourceLayerRoadIds,
            bool replaceExistingAssignments,
            CancellationToken ct = default);

        Task AssignManualAsync(
            ProjectSession session,
            Guid canvasObjectId,
            int roadId,
            CancellationToken ct = default);

        Task<bool> ClearAssignmentAsync(
            ProjectSession session,
            Guid canvasObjectId,
            CancellationToken ct = default);

        Task<int> ClearAllAssignmentsAsync(
            ProjectSession session,
            CancellationToken ct = default);
    }

    public sealed class RoadCenterlineAssignmentService : IRoadCenterlineAssignmentService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<IReadOnlyList<RoadCenterlineAssignmentCandidate>> GetCandidatesAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            List<CanvasObject> objects = await context.CanvasObjects
                .AsNoTracking()
                .Include(item => item.CanvasLayer)
                .Where(item =>
                    item.CanvasLayer != null &&
                    item.CanvasLayer.LayerType == CanvasLayerTreeService.RoadCenterlineLayerType)
                .OrderBy(item => item.CanvasLayer.Name)
                .ThenBy(item => item.CreatedDate)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            List<int> assignedRoadIds = objects
                .Select(item => item.RoadId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
            Dictionary<int, Road> roadsById = await context.Roads
                .AsNoTracking()
                .Where(road => assignedRoadIds.Contains(road.Id))
                .ToDictionaryAsync(road => road.Id, ct)
                .ConfigureAwait(false);

            return objects
                .Select(item =>
                {
                    Road? road = item.RoadId.HasValue && roadsById.TryGetValue(item.RoadId.Value, out Road? linkedRoad)
                        ? linkedRoad
                        : null;

                    return new RoadCenterlineAssignmentCandidate(
                        item.Id,
                        item.CanvasLayer?.Name ?? string.Empty,
                        item.ObjectType,
                        ReadSourceLayer(item),
                        item.RoadId,
                        road?.RoadName,
                        road?.RoadCode,
                        road?.RoadWidth,
                        road?.RightOfWayWidth,
                        item.Shape?.Length ?? 0.0);
                })
                .ToList();
        }

        public async Task<IReadOnlyList<RoadRecordChoice>> GetRoadsAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            return await session.GetDbContext()
                .Roads
                .AsNoTracking()
                .OrderBy(road => road.RoadCode)
                .ThenBy(road => road.RoadName)
                .Select(road => new RoadRecordChoice(
                    road.Id,
                    road.RoadName,
                    road.RoadCode,
                    road.RoadWidth,
                    road.RightOfWayWidth,
                    road.RoadType,
                    road.SurfaceType))
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<int> AssignBySourceLayerAsync(
            ProjectSession session,
            IReadOnlyDictionary<string, int> sourceLayerRoadIds,
            bool replaceExistingAssignments,
            CancellationToken ct = default)
        {
            if (sourceLayerRoadIds.Count == 0)
                return 0;

            var context = session.GetDbContext();
            List<CanvasObject> objects = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .Where(item =>
                    item.CanvasLayer != null &&
                    item.CanvasLayer.LayerType == CanvasLayerTreeService.RoadCenterlineLayerType)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            List<int> validRoadIdRows = await context.Roads
                .Where(road => sourceLayerRoadIds.Values.Contains(road.Id))
                .Select(road => road.Id)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            HashSet<int> validRoadIds = validRoadIdRows.ToHashSet();

            int assigned = 0;
            foreach (CanvasObject canvasObject in objects)
            {
                if (!replaceExistingAssignments && canvasObject.RoadId.HasValue)
                    continue;

                string sourceLayer = ReadSourceLayer(canvasObject);
                if (!sourceLayerRoadIds.TryGetValue(sourceLayer, out int roadId) ||
                    !validRoadIds.Contains(roadId))
                {
                    continue;
                }

                ApplyRoadAssignment(canvasObject, roadId);
                assigned++;
            }

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return assigned;
        }

        public async Task AssignManualAsync(
            ProjectSession session,
            Guid canvasObjectId,
            int roadId,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            CanvasObject canvasObject = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .FirstAsync(item => item.Id == canvasObjectId, ct)
                .ConfigureAwait(false);
            bool isRoadCenterline = canvasObject.CanvasLayer != null &&
                                    string.Equals(
                                        canvasObject.CanvasLayer.LayerType,
                                        CanvasLayerTreeService.RoadCenterlineLayerType,
                                        StringComparison.OrdinalIgnoreCase);
            if (!isRoadCenterline)
                throw new InvalidOperationException("The selected object is not a road centerline object.");

            bool roadExists = await context.Roads.AnyAsync(road => road.Id == roadId, ct).ConfigureAwait(false);
            if (!roadExists)
                throw new InvalidOperationException("The selected road definition no longer exists.");

            ApplyRoadAssignment(canvasObject, roadId);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        public async Task<bool> ClearAssignmentAsync(
            ProjectSession session,
            Guid canvasObjectId,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            CanvasObject? canvasObject = await context.CanvasObjects
                .FirstOrDefaultAsync(item => item.Id == canvasObjectId, ct)
                .ConfigureAwait(false);
            if (canvasObject == null || !canvasObject.RoadId.HasValue)
                return false;

            canvasObject.RoadId = null;
            canvasObject.LabelText = null;
            canvasObject.LastModifiedDate = DateTime.Now;
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return true;
        }

        public async Task<int> ClearAllAssignmentsAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            List<CanvasObject> assignedObjects = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .Where(item =>
                    item.RoadId.HasValue &&
                    item.CanvasLayer != null &&
                    item.CanvasLayer.LayerType == CanvasLayerTreeService.RoadCenterlineLayerType)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            if (assignedObjects.Count == 0)
                return 0;

            DateTime modifiedAt = DateTime.Now;
            foreach (CanvasObject canvasObject in assignedObjects)
            {
                canvasObject.RoadId = null;
                canvasObject.LabelText = null;
                canvasObject.LastModifiedDate = modifiedAt;
            }

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return assignedObjects.Count;
        }

        private static void ApplyRoadAssignment(CanvasObject canvasObject, int roadId)
        {
            canvasObject.RoadId = roadId;
            canvasObject.LastModifiedDate = DateTime.Now;
        }

        private static string ReadSourceLayer(CanvasObject canvasObject)
        {
            if (string.IsNullOrWhiteSpace(canvasObject.GeometryMetadataJson))
                return canvasObject.CanvasLayer?.Name ?? "Unknown";

            try
            {
                ExternalObjectMetadata? metadata =
                    JsonSerializer.Deserialize<ExternalObjectMetadata>(
                        canvasObject.GeometryMetadataJson,
                        JsonOptions);
                return string.IsNullOrWhiteSpace(metadata?.SourceLayer)
                    ? canvasObject.CanvasLayer?.Name ?? "Unknown"
                    : metadata.SourceLayer.Trim();
            }
            catch
            {
                return canvasObject.CanvasLayer?.Name ?? "Unknown";
            }
        }

        private sealed class ExternalObjectMetadata
        {
            public string? SourceLayer { get; set; }
        }
    }

    public sealed record RoadCenterlineAssignmentCandidate(
        Guid CanvasObjectId,
        string LayerName,
        string ObjectType,
        string SourceLayer,
        int? RoadId,
        string? RoadName,
        string? RoadCode,
        double? RoadWidth,
        double? RightOfWayWidth,
        double GeometryLength);

    public sealed record RoadRecordChoice(
        int Id,
        string RoadName,
        string? RoadCode,
        double RoadWidth,
        double? RightOfWayWidth,
        string? RoadType,
        string? SurfaceType)
    {
        public string DisplayText
        {
            get
            {
                string code = string.IsNullOrWhiteSpace(RoadCode) ? string.Empty : RoadCode.Trim();
                string name = string.IsNullOrWhiteSpace(RoadName) ? string.Empty : RoadName.Trim();
                string label = !string.IsNullOrWhiteSpace(code) && !string.Equals(code, name, StringComparison.OrdinalIgnoreCase)
                    ? string.IsNullOrWhiteSpace(name) ? code : $"{code} - {name}"
                    : !string.IsNullOrWhiteSpace(name) ? name : $"Road #{Id}";
                string width = RoadWidth > 0 ? $" | {RoadWidth:0.##} m" : string.Empty;
                return $"{label}{width}";
            }
        }
    }
}
