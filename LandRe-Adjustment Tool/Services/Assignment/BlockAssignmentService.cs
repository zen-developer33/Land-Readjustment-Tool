using System.Text.Json;
using System.Text.RegularExpressions;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Layout;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Services.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Services.Assignment
{
    public interface IBlockAssignmentService
    {
        Task<IReadOnlyList<BlockAssignmentCandidate>> GetCandidatesAsync(
            ProjectSession session,
            CancellationToken ct = default);

        Task<IReadOnlyList<BlockRecordChoice>> GetBlocksAsync(
            ProjectSession session,
            CancellationToken ct = default);

        Task<IReadOnlyList<BlockLabelSourceChoice>> GetLabelSourcesAsync(
            ProjectSession session,
            CancellationToken ct = default);

        Task<int> AssignBySourceLayerAsync(
            ProjectSession session,
            IReadOnlyDictionary<string, int> sourceLayerBlockIds,
            bool replaceExistingAssignments,
            CancellationToken ct = default);

        Task<BlockAutoAssignmentResult> AutoAssignFromLabelsAsync(
            ProjectSession session,
            string? labelSourceLayer,
            bool createMissingBlockDefinitions,
            bool replaceExistingAssignments,
            CancellationToken ct = default);

        Task AssignManualAsync(
            ProjectSession session,
            Guid canvasObjectId,
            int blockId,
            CancellationToken ct = default);

        Task<bool> ClearAssignmentAsync(
            ProjectSession session,
            Guid canvasObjectId,
            CancellationToken ct = default);

        Task<int> ClearAllAssignmentsAsync(
            ProjectSession session,
            CancellationToken ct = default);
    }

    public sealed class BlockAssignmentService : IBlockAssignmentService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<IReadOnlyList<BlockAssignmentCandidate>> GetCandidatesAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            List<CanvasObject> blockObjects = await LoadBlockObjects(context)
                .AsNoTracking()
                .OrderBy(item => item.CanvasLayer.Name)
                .ThenBy(item => item.CreatedDate)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            Dictionary<int, Block> blocksById = await LoadAssignedBlocksAsync(context, blockObjects, ct)
                .ConfigureAwait(false);
            List<CanvasObject> labelObjects = await LoadLabelObjects(context)
                .AsNoTracking()
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return blockObjects
                .Select(item =>
                {
                    Block? block = item.BlockId.HasValue && blocksById.TryGetValue(item.BlockId.Value, out Block? linkedBlock)
                        ? linkedBlock
                        : null;

                    return new BlockAssignmentCandidate(
                        item.Id,
                        item.CanvasLayer?.Name ?? string.Empty,
                        item.ObjectType,
                        ReadSourceLayer(item),
                        item.BlockId,
                        block?.BlockName,
                        block?.BlockCode,
                        block?.BlockLandUse,
                        block?.BlockDepth,
                        item.Shape?.Area ?? 0.0,
                        CanvasGeometryMetricsService.GetBlockDepthFromGeometry(item),
                        FindContainedLabel(item, labelObjects));
                })
                .ToList();
        }

        public async Task<IReadOnlyList<BlockRecordChoice>> GetBlocksAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            return await session.GetDbContext()
                .Blocks
                .AsNoTracking()
                .OrderBy(block => block.BlockCode)
                .ThenBy(block => block.BlockName)
                .Select(block => new BlockRecordChoice(
                    block.Id,
                    block.BlockName,
                    block.BlockCode,
                    block.BlockLandUse,
                    block.BlockDepth,
                    block.BlockLength,
                    block.BlockArea))
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<BlockLabelSourceChoice>> GetLabelSourcesAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            List<CanvasObject> labels = await LoadLabelObjects(session.GetDbContext())
                .AsNoTracking()
                .ToListAsync(ct)
                .ConfigureAwait(false);
            List<string> sourceLayers = labels
                .Select(ReadSourceLayer)
                .Where(layer => !string.IsNullOrWhiteSpace(layer))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(layer => layer, StringComparer.OrdinalIgnoreCase)
                .ToList();

            List<BlockLabelSourceChoice> choices =
            [
                new(null, "All text label layers")
            ];
            choices.AddRange(sourceLayers.Select(layer => new BlockLabelSourceChoice(layer, layer)));
            return choices;
        }

        public async Task<int> AssignBySourceLayerAsync(
            ProjectSession session,
            IReadOnlyDictionary<string, int> sourceLayerBlockIds,
            bool replaceExistingAssignments,
            CancellationToken ct = default)
        {
            if (sourceLayerBlockIds.Count == 0)
                return 0;

            var context = session.GetDbContext();
            List<CanvasObject> objects = await LoadBlockObjects(context)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            HashSet<int> validBlockIds = (await context.Blocks
                    .Where(block => sourceLayerBlockIds.Values.Contains(block.Id))
                    .Select(block => block.Id)
                    .ToListAsync(ct)
                    .ConfigureAwait(false))
                .ToHashSet();

            int assigned = 0;
            foreach (CanvasObject canvasObject in objects)
            {
                if (!replaceExistingAssignments && canvasObject.BlockId.HasValue)
                    continue;

                string sourceLayer = ReadSourceLayer(canvasObject);
                if (!sourceLayerBlockIds.TryGetValue(sourceLayer, out int blockId) ||
                    !validBlockIds.Contains(blockId))
                {
                    continue;
                }

                ApplyBlockAssignment(canvasObject, blockId);
                assigned++;
            }

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return assigned;
        }

        public async Task<BlockAutoAssignmentResult> AutoAssignFromLabelsAsync(
            ProjectSession session,
            string? labelSourceLayer,
            bool createMissingBlockDefinitions,
            bool replaceExistingAssignments,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            List<CanvasObject> blockObjects = await LoadBlockObjects(context)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            List<CanvasObject> labelObjects = await LoadLabelObjects(context)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(labelSourceLayer))
            {
                labelObjects = labelObjects
                    .Where(item => string.Equals(ReadSourceLayer(item), labelSourceLayer, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            Dictionary<string, Block> blocksByCode = (await context.Blocks
                    .ToListAsync(ct)
                    .ConfigureAwait(false))
                .Where(block => !string.IsNullOrWhiteSpace(block.BlockCode ?? block.BlockName))
                .GroupBy(block => block.BlockCode ?? block.BlockName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            DateTime now = DateTime.Now;
            int defined = 0;
            int assigned = 0;
            int missingDefinitions = 0;
            List<(CanvasObject CanvasObject, Block Block)> pendingAssignments = [];

            foreach (CanvasObject blockObject in blockObjects)
            {
                if (!replaceExistingAssignments && blockObject.BlockId.HasValue)
                    continue;

                string? label = FindContainedLabel(blockObject, labelObjects);
                if (string.IsNullOrWhiteSpace(label))
                    continue;

                string code = BuildDefinitionCode(label, "Block");
                if (!blocksByCode.TryGetValue(code, out Block? block))
                {
                    if (!createMissingBlockDefinitions)
                    {
                        missingDefinitions++;
                        continue;
                    }

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
                        Description = "Auto-defined from contained block label text."
                    };
                    context.Blocks.Add(block);
                    blocksByCode[code] = block;
                    defined++;
                }

                pendingAssignments.Add((blockObject, block));
            }

            if (defined > 0)
                await context.SaveChangesAsync(ct).ConfigureAwait(false);

            foreach ((CanvasObject canvasObject, Block block) in pendingAssignments)
            {
                if (block.Id == 0)
                    continue;

                ApplyBlockAssignment(canvasObject, block.Id);
                assigned++;
            }

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return new BlockAutoAssignmentResult(defined, assigned, missingDefinitions);
        }

        public async Task AssignManualAsync(
            ProjectSession session,
            Guid canvasObjectId,
            int blockId,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            CanvasObject canvasObject = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .FirstAsync(item => item.Id == canvasObjectId, ct)
                .ConfigureAwait(false);
            if (!IsBlockObject(canvasObject))
                throw new InvalidOperationException("The selected object is not a block layout object.");

            bool blockExists = await context.Blocks.AnyAsync(block => block.Id == blockId, ct).ConfigureAwait(false);
            if (!blockExists)
                throw new InvalidOperationException("The selected block definition no longer exists.");

            ApplyBlockAssignment(canvasObject, blockId);
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
            if (canvasObject == null || !canvasObject.BlockId.HasValue)
                return false;

            canvasObject.BlockId = null;
            canvasObject.LastModifiedDate = DateTime.Now;
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return true;
        }

        public async Task<int> ClearAllAssignmentsAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            List<CanvasObject> assignedObjects = await LoadBlockObjects(context)
                .Where(item => item.BlockId.HasValue)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            if (assignedObjects.Count == 0)
                return 0;

            DateTime modifiedAt = DateTime.Now;
            foreach (CanvasObject canvasObject in assignedObjects)
            {
                canvasObject.BlockId = null;
                canvasObject.LastModifiedDate = modifiedAt;
            }

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return assignedObjects.Count;
        }

        private static IQueryable<CanvasObject> LoadBlockObjects(DbContext context)
        {
            return context.Set<CanvasObject>()
                .Include(item => item.CanvasLayer)
                .Where(item => item.CanvasLayer != null)
                .Where(item => item.CanvasLayer != null && item.CanvasLayer.LayerType == "Block");
        }

        private static IQueryable<CanvasObject> LoadLabelObjects(DbContext context)
        {
            return context.Set<CanvasObject>()
                .Include(item => item.CanvasLayer)
                .Where(item =>
                    item.ObjectType == "Text" &&
                    item.LabelText != null &&
                    item.LabelText != string.Empty);
        }

        private static async Task<Dictionary<int, Block>> LoadAssignedBlocksAsync(
            DbContext context,
            IEnumerable<CanvasObject> blockObjects,
            CancellationToken ct)
        {
            List<int> assignedBlockIds = blockObjects
                .Select(item => item.BlockId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
            if (assignedBlockIds.Count == 0)
                return [];

            return await context.Set<Block>()
                .AsNoTracking()
                .Where(block => assignedBlockIds.Contains(block.Id))
                .ToDictionaryAsync(block => block.Id, ct)
                .ConfigureAwait(false);
        }

        private static bool IsBlockObject(CanvasObject canvasObject)
        {
            return canvasObject.CanvasLayer != null &&
                   CanvasLayerTreeService.IsBlockLayoutLayer(canvasObject.CanvasLayer);
        }

        private static string? FindContainedLabel(CanvasObject blockObject, IReadOnlyList<CanvasObject> labelObjects)
        {
            if (blockObject.Shape == null || labelObjects.Count == 0)
                return null;

            return labelObjects
                .Where(label => label.Shape != null && blockObject.Shape.Covers(label.Shape))
                .OrderBy(label => label.LabelText)
                .Select(label => label.LabelText?.Trim())
                .FirstOrDefault(label => !string.IsNullOrWhiteSpace(label));
        }

        private static void ApplyBlockAssignment(CanvasObject canvasObject, int blockId)
        {
            canvasObject.BlockId = blockId;
            canvasObject.LastModifiedDate = DateTime.Now;
        }

        private static string ReadSourceLayer(CanvasObject canvasObject)
        {
            return ReadSourceLayerFromMetadata(canvasObject.GeometryMetadataJson)
                   ?? canvasObject.CanvasLayer?.Name
                   ?? "Unknown";
        }

        private static string? ReadSourceLayerFromMetadata(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
                return null;

            try
            {
                SourceMetadata? metadata = JsonSerializer.Deserialize<SourceMetadata>(
                    metadataJson,
                    JsonOptions);
                return string.IsNullOrWhiteSpace(metadata?.SourceLayer)
                    ? null
                    : metadata.SourceLayer.Trim();
            }
            catch
            {
                return null;
            }
        }

        private static string BuildDefinitionCode(string value, string fallbackPrefix)
        {
            string text = Regex.Replace(value.Trim(), @"\s+", " ");
            text = Regex.Replace(text, @"[^\p{L}\p{Nd}\._\-\s]+", "");
            return string.IsNullOrWhiteSpace(text)
                ? $"{fallbackPrefix}-{DateTime.Now:HHmmss}"
                : text;
        }

        private sealed class SourceMetadata
        {
            public string? SourceLayer { get; set; }
        }
    }

    public sealed record BlockAssignmentCandidate(
        Guid CanvasObjectId,
        string LayerName,
        string ObjectType,
        string SourceLayer,
        int? BlockId,
        string? BlockName,
        string? BlockCode,
        string? BlockLandUse,
        float? BlockDepth,
        double GeometryArea,
        double? GeometryBlockDepth,
        string? DetectedLabel);

    public sealed record BlockRecordChoice(
        int Id,
        string BlockName,
        string? BlockCode,
        string? BlockLandUse,
        float BlockDepth,
        float BlockLength,
        double BlockArea)
    {
        public string DisplayText
        {
            get
            {
                string code = string.IsNullOrWhiteSpace(BlockCode) ? string.Empty : BlockCode.Trim();
                string name = string.IsNullOrWhiteSpace(BlockName) ? string.Empty : BlockName.Trim();
                string label = !string.IsNullOrWhiteSpace(code) && !string.Equals(code, name, StringComparison.OrdinalIgnoreCase)
                    ? string.IsNullOrWhiteSpace(name) ? code : $"{code} - {name}"
                    : !string.IsNullOrWhiteSpace(name) ? name : $"Block #{Id}";
                string landUse = string.IsNullOrWhiteSpace(BlockLandUse) ? string.Empty : $" | {BlockLandUse}";
                return $"{label}{landUse}";
            }
        }
    }

    public sealed record BlockLabelSourceChoice(string? SourceLayer, string DisplayText)
    {
        public override string ToString() => DisplayText;
    }

    public sealed record BlockAutoAssignmentResult(
        int BlocksDefined,
        int BlocksAssigned,
        int MissingDefinitions);
}
