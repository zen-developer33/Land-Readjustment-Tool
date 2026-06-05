using System.Text.Json;
using System.Text.RegularExpressions;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Layout;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Canvas;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NtsPoint = NetTopologySuite.Geometries.Point;

namespace Land_Readjustment_Tool.Services.Import
{
    public sealed class BlockLayoutPlanAutoDefinitionService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<BlockLayoutPlanAutoDefinitionResult> ApplyAsync(
            ProjectSession session,
            IReadOnlyList<Guid> importedObjectIds,
            string? blockLabelLayerName,
            bool replaceMatchingRoadDefinitions,
            CancellationToken ct = default)
        {
            if (importedObjectIds.Count == 0)
                return new BlockLayoutPlanAutoDefinitionResult(0, 0, 0, 0);

            AppDbContext context = session.GetDbContext();
            await ProjectDatabaseCompatibility.EnsureAsync(context, ct);

            List<CanvasObject> importedObjects = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .Where(item => importedObjectIds.Contains(item.Id))
                .ToListAsync(ct);

            (int roadsDefined, int roadsAssigned) = await DefineAndAssignRoadsAsync(
                context,
                importedObjects,
                replaceMatchingRoadDefinitions,
                ct);

            (int blocksDefined, int blocksAssigned) = await DefineAndAssignBlocksAsync(
                context,
                importedObjects,
                blockLabelLayerName,
                ct);

            await context.SaveChangesAsync(ct);

            return new BlockLayoutPlanAutoDefinitionResult(
                roadsDefined,
                roadsAssigned,
                blocksDefined,
                blocksAssigned);
        }

        private static async Task<(int Defined, int Assigned)> DefineAndAssignRoadsAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> importedObjects,
            bool replaceMatchingRoadDefinitions,
            CancellationToken ct)
        {
            List<IGrouping<string, CanvasObject>> roadGroups = importedObjects
                .Where(IsRoadAssignmentObject)
                .GroupBy(GetSourceLayer, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (roadGroups.Count == 0)
                return (0, 0);

            DbSet<Road> roads = context.Set<Road>();
            Dictionary<string, Road> roadsByCode = (await roads.ToListAsync(ct))
                .Where(road => !string.IsNullOrWhiteSpace(road.RoadCode ?? road.RoadName))
                .GroupBy(road => road.RoadCode ?? road.RoadName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            DateTime now = DateTime.Now;
            int defined = 0;
            List<(CanvasObject CanvasObject, Road Road)> pendingAssignments = [];
            foreach (IGrouping<string, CanvasObject> group in roadGroups)
            {
                string code = BuildDefinitionCode(group.Key, "Road");
                double width = ExtractWidthMetres(group.Key) ?? 0;

                if (!roadsByCode.TryGetValue(code, out Road? road))
                {
                    road = new Road
                    {
                        RoadCode = code,
                        RoadName = code,
                        RoadStatus = "Proposed",
                        RoadType = InferRoadType(width),
                        SurfaceType = null,
                        RoadWidth = width,
                        RightOfWayWidth = width > 0 ? width : null,
                        CreatedDate = now,
                        LastModifiedDate = now,
                        Description = $"Auto-defined from source layer '{group.Key}'."
                    };
                    roads.Add(road);
                    roadsByCode[code] = road;
                    defined++;
                }
                else if (replaceMatchingRoadDefinitions)
                {
                    road.RoadName = code;
                    road.RoadCode = code;
                    road.RoadWidth = width;
                    road.RightOfWayWidth = width > 0 ? width : null;
                    road.RoadType = InferRoadType(width);
                    road.LastModifiedDate = now;
                    road.Description = $"Auto-defined from source layer '{group.Key}'.";
                    defined++;
                }

                foreach (CanvasObject canvasObject in group)
                    pendingAssignments.Add((canvasObject, road));
            }

            if (defined > 0)
                await context.SaveChangesAsync(ct);

            int assigned = 0;
            foreach ((CanvasObject canvasObject, Road road) in pendingAssignments)
            {
                if (road.Id == 0)
                    continue;

                canvasObject.RoadId = road.Id;
                canvasObject.Road = null;
                canvasObject.LastModifiedDate = now;
                assigned++;
            }

            return (defined, assigned);
        }

        private static async Task<(int Defined, int Assigned)> DefineAndAssignBlocksAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> importedObjects,
            string? blockLabelLayerName,
            CancellationToken ct)
        {
            List<CanvasObject> blockObjects = importedObjects
                .Where(IsBlockObject)
                .ToList();
            if (blockObjects.Count == 0)
                return (0, 0);

            List<CanvasObject> labelObjects = string.IsNullOrWhiteSpace(blockLabelLayerName)
                ? []
                : importedObjects
                    .Where(item =>
                        item.Shape is NtsPoint &&
                        string.Equals(item.ObjectType, "Text", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(item.LabelText) &&
                        string.Equals(GetSourceLayer(item), blockLabelLayerName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            DbSet<Block> blocks = context.Set<Block>();
            Dictionary<string, Block> blocksByCode = (await blocks.ToListAsync(ct))
                .Where(block => !string.IsNullOrWhiteSpace(block.BlockCode ?? block.BlockName))
                .GroupBy(block => block.BlockCode ?? block.BlockName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            DateTime now = DateTime.Now;
            int defined = 0;
            List<(CanvasObject CanvasObject, Block Block)> pendingAssignments = [];
            foreach (CanvasObject blockObject in blockObjects)
            {
                string label = FindContainedLabel(blockObject.Shape, labelObjects)
                    ?? BuildDefinitionCode(GetSourceLayer(blockObject), "Block");
                string code = BuildDefinitionCode(label, "Block");

                if (!blocksByCode.TryGetValue(code, out Block? block))
                {
                    block = new Block
                    {
                        BlockCode = code,
                        BlockName = label.Trim(),
                        BlockDepth = Convert.ToSingle(CanvasGeometryMetricsService.GetBlockDepthFromGeometry(blockObject) ?? 0),
                        BlockLength = 0,
                        BlockLandUse = "Residential",
                        BlockArea = blockObject.Shape?.Area ?? 0,
                        CreatedDate = now,
                        LastModifiedDate = now,
                        Description = "Auto-defined from imported block layout plan."
                    };
                    blocks.Add(block);
                    blocksByCode[code] = block;
                    defined++;
                }

                pendingAssignments.Add((blockObject, block));
            }

            if (defined > 0)
                await context.SaveChangesAsync(ct);

            int assigned = 0;
            foreach ((CanvasObject blockObject, Block block) in pendingAssignments)
            {
                if (block.Id == 0)
                    continue;

                blockObject.BlockId = block.Id;
                blockObject.Block = null;
                if (!block.CanvasObjectId.HasValue)
                    block.CanvasObjectId = blockObject.Id;
                blockObject.LastModifiedDate = now;
                block.LastModifiedDate = now;
                assigned++;
            }

            return (defined, assigned);
        }

        private static string? FindContainedLabel(Geometry? blockGeometry, IReadOnlyList<CanvasObject> labelObjects)
        {
            if (blockGeometry == null || labelObjects.Count == 0)
                return null;

            return labelObjects
                .Where(label => label.Shape != null && blockGeometry.Covers(label.Shape))
                .OrderBy(label => label.LabelText)
                .Select(label => label.LabelText?.Trim())
                .FirstOrDefault(label => !string.IsNullOrWhiteSpace(label));
        }

        private static bool IsRoadAssignmentObject(CanvasObject canvasObject)
        {
            string? layerType = canvasObject.CanvasLayer?.LayerType;
            return string.Equals(layerType, CanvasLayerTreeService.RoadCenterlineLayerType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layerType, "RoadParcel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layerType, "ProposedRoad", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layerType, "ExistingRoad", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBlockObject(CanvasObject canvasObject) =>
            canvasObject.CanvasLayer != null &&
            CanvasLayerTreeService.IsBlockLayoutLayer(canvasObject.CanvasLayer);

        private static string GetSourceLayer(CanvasObject canvasObject)
        {
            if (string.IsNullOrWhiteSpace(canvasObject.GeometryMetadataJson))
                return canvasObject.CanvasLayer?.Name ?? "Unknown";

            try
            {
                SourceMetadata? metadata = JsonSerializer.Deserialize<SourceMetadata>(
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

        private static double? ExtractWidthMetres(string sourceLayer)
        {
            Match match = Regex.Match(
                sourceLayer,
                @"(?<!\d)(\d{1,3}(?:[._]\d+)?)\s*m\b",
                RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                match = Regex.Match(
                    sourceLayer,
                    @"\bR[_\-\s]*(\d{1,3}(?:[._]\d+)?)\b",
                    RegexOptions.IgnoreCase);
            }

            if (!match.Success)
                return null;

            string text = match.Groups[1].Value.Replace('_', '.');
            return double.TryParse(text, out double value) ? value : null;
        }

        private static string BuildDefinitionCode(string value, string fallbackPrefix)
        {
            string text = Regex.Replace(value.Trim(), @"\s+", " ");
            text = Regex.Replace(text, @"[^\p{L}\p{Nd}\._\-\s]+", "");
            return string.IsNullOrWhiteSpace(text)
                ? $"{fallbackPrefix}-{DateTime.Now:HHmmss}"
                : text;
        }

        private static string? InferRoadType(double width)
        {
            if (width <= 0)
                return null;
            if (width <= 4)
                return "Local";
            if (width <= 6)
                return "Lane";
            if (width <= 8)
                return "Collector";
            return "Arterial";
        }

        private sealed class SourceMetadata
        {
            public string? SourceLayer { get; set; }
        }
    }

    public sealed record BlockLayoutPlanAutoDefinitionResult(
        int RoadsDefined,
        int RoadsAssigned,
        int BlocksDefined,
        int BlocksAssigned);
}
