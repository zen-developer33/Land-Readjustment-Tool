using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using System.Text.RegularExpressions;
using NtsPoint = NetTopologySuite.Geometries.Point;

namespace Land_Readjustment_Tool.Services.Assignment
{
    public interface ICadastralRecordAssignmentService
    {
        Task<IReadOnlyList<CadastralAssignmentCandidate>> GetCandidatesAsync(
            ProjectSession session,
            CancellationToken ct = default);

        Task<IReadOnlyList<string>> GetMapSheetNumbersAsync(
            ProjectSession session,
            CancellationToken ct = default);

        Task<IReadOnlyList<CadastralParcelRecordChoice>> GetParcelsByMapSheetAsync(
            ProjectSession session,
            string mapSheetNo,
            CancellationToken ct = default);

        Task<CadastralAssignmentResult> AutoAssignAsync(
            ProjectSession session,
            bool replaceExistingAssignments,
            IReadOnlyList<CadastralLayerMapSheetMapping> layerMappings,
            CancellationToken ct = default);

        Task<CadastralAssignmentResult> AutoAssignAsync(
            ProjectSession session,
            bool replaceExistingAssignments,
            CancellationToken ct = default);

        Task AssignManualAsync(
            ProjectSession session,
            Guid canvasObjectId,
            int baselineParcelId,
            bool replaceExistingAssignment,
            CancellationToken ct = default);
    }

    public sealed class CadastralRecordAssignmentService : ICadastralRecordAssignmentService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private static readonly Regex ParcelTextRegex = new(@"\b[\p{L}\d\-\/]+\b", RegexOptions.Compiled);

        public async Task<IReadOnlyList<CadastralAssignmentCandidate>> GetCandidatesAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            List<CanvasObject> objects = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .Where(canvasObject => canvasObject.GeometryMetadataJson != null &&
                                       canvasObject.GeometryMetadataJson.Contains(CadastralCanvasMetadata.MetadataKind) &&
                                       canvasObject.ObjectType == "Polygon")
                .OrderBy(item => item.CanvasLayer.Name)
                .ThenBy(item => item.CreatedDate)
                .ToListAsync(ct);

            return objects
                .Select(ToCandidate)
                .ToList();
        }

        public async Task<IReadOnlyList<string>> GetMapSheetNumbersAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            return await session.GetDbContext()
                .BaselineParcels
                .AsNoTracking()
                .Select(parcel => parcel.MapSheetNo)
                .Where(value => value != "")
                .Distinct()
                .OrderBy(value => value)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CadastralParcelRecordChoice>> GetParcelsByMapSheetAsync(
            ProjectSession session,
            string mapSheetNo,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(mapSheetNo))
                return [];

            return await session.GetDbContext()
                .BaselineParcels
                .AsNoTracking()
                .Where(parcel => parcel.MapSheetNo == mapSheetNo)
                .OrderBy(parcel => parcel.ParcelNo)
                .Select(parcel => new CadastralParcelRecordChoice(
                    parcel.Id,
                    parcel.MapSheetNo,
                    parcel.ParcelNo,
                    $"{parcel.ParcelNo}  |  {parcel.OriginalAreaSqm:0.##} sq.m",
                    parcel.OriginalAreaSqm,
                    parcel.CanvasObjectId))
                .ToListAsync(ct);
        }

        public async Task<CadastralAssignmentResult> AutoAssignAsync(
            ProjectSession session,
            bool replaceExistingAssignments,
            CancellationToken ct = default)
        {
            return await AutoAssignAsync(session, replaceExistingAssignments, [], ct);
        }

        public async Task<CadastralAssignmentResult> AutoAssignAsync(
            ProjectSession session,
            bool replaceExistingAssignments,
            IReadOnlyList<CadastralLayerMapSheetMapping> layerMappings,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();

            List<BaselineParcel> records = await context.BaselineParcels
                .Include(parcel => parcel.LandOwner)
                .ToListAsync(ct);
            Dictionary<string, BaselineParcel> lookup = records
                .GroupBy(record => BuildParcelCode(record.MapSheetNo, record.ParcelNo), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            Dictionary<string, string> layerMapSheets = BuildLayerMapSheetLookup(layerMappings);
            List<CanvasObject> candidates = await context.CanvasObjects
                .Include(canvasObject => canvasObject.CanvasLayer)
                .Where(canvasObject => canvasObject.GeometryMetadataJson != null &&
                                       canvasObject.GeometryMetadataJson.Contains(CadastralCanvasMetadata.MetadataKind))
                .ToListAsync(ct);
            List<CanvasObject> parcelObjects = candidates
                .Where(item => string.Equals(item.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase))
                .ToList();
            List<CadastralTextAssignmentFeature> textFeatures = candidates
                .Where(item => string.Equals(item.ObjectType, "Text", StringComparison.OrdinalIgnoreCase))
                .Select(ToTextFeature)
                .Where(item => item != null)
                .Select(item => item!)
                .ToList();
            STRtree<CadastralTextAssignmentFeature> textIndex = BuildTextIndex(textFeatures);

            int assigned = 0;
            int missingKey = 0;
            int noMatch = 0;

            foreach (CanvasObject canvasObject in parcelObjects)
            {
                CadastralCanvasMetadata? metadata = ReadMetadata(canvasObject.GeometryMetadataJson);
                if (metadata == null)
                    continue;

                if (!replaceExistingAssignments && canvasObject.BaselineParcelId.HasValue)
                {
                    noMatch++;
                    continue;
                }

                string? mapSheetNo = ResolveMapSheetNo(canvasObject, metadata, layerMapSheets);
                string? parcelNo = FindParcelTextInsidePolygon(canvasObject.Shape, textIndex)
                                   ?? ExtractParcelText(metadata.ParcelNo);
                if (string.IsNullOrWhiteSpace(mapSheetNo) ||
                    string.IsNullOrWhiteSpace(parcelNo))
                {
                    missingKey++;
                    continue;
                }

                string key = BuildParcelCode(mapSheetNo, parcelNo);
                if (!lookup.TryGetValue(key, out BaselineParcel? parcel))
                {
                    noMatch++;
                    continue;
                }

                if (!replaceExistingAssignments &&
                    parcel.CanvasObjectId.HasValue &&
                    parcel.CanvasObjectId.Value != canvasObject.Id)
                {
                    noMatch++;
                    continue;
                }

                if (replaceExistingAssignments)
                {
                    await ClearPreviousParcelLinkedToObjectAsync(context, canvasObject.Id, ct);
                    await ClearPreviousObjectLinkedToParcelAsync(context, parcel, canvasObject.Id, ct);
                }

                canvasObject.BaselineParcelId = parcel.Id;
                canvasObject.LabelText = parcel.ParcelNo;
                parcel.CanvasObjectId = canvasObject.Id;
                metadata.MapSheetNo = parcel.MapSheetNo;
                metadata.ParcelNo = parcel.ParcelNo;
                metadata.MatchedText = parcelNo;
                ApplyParcelMetadata(metadata, parcel, "AutoAssigned");
                canvasObject.GeometryMetadataJson = JsonSerializer.Serialize(metadata);
                canvasObject.LastModifiedDate = DateTime.Now;
                parcel.LastModifiedDate = DateTime.Now;
                assigned++;
            }

            await context.SaveChangesAsync(ct);
            return new CadastralAssignmentResult(
                true,
                null,
                assigned,
                missingKey,
                noMatch);
        }

        public async Task AssignManualAsync(
            ProjectSession session,
            Guid canvasObjectId,
            int baselineParcelId,
            bool replaceExistingAssignment,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            CanvasObject canvasObject = await context.CanvasObjects
                .FirstAsync(item => item.Id == canvasObjectId, ct);
            BaselineParcel parcel = await context.BaselineParcels
                .Include(item => item.LandOwner)
                .FirstAsync(item => item.Id == baselineParcelId, ct);

            if (!replaceExistingAssignment &&
                parcel.CanvasObjectId.HasValue &&
                parcel.CanvasObjectId.Value != canvasObject.Id)
            {
                throw new InvalidOperationException(
                    "The selected parcel record is already assigned to another canvas object.");
            }

            if (replaceExistingAssignment && parcel.CanvasObjectId.HasValue)
            {
                CanvasObject? previousObject = await context.CanvasObjects
                    .FirstOrDefaultAsync(item => item.Id == parcel.CanvasObjectId.Value, ct);
                if (previousObject != null && previousObject.Id != canvasObject.Id)
                {
                    previousObject.BaselineParcelId = null;
                    previousObject.LabelText = null;
                    CadastralCanvasMetadata? previousMetadata = ReadMetadata(previousObject.GeometryMetadataJson);
                    if (previousMetadata != null)
                    {
                        previousMetadata.BaselineParcelId = null;
                        previousMetadata.AssignmentStatus = "Unassigned";
                        previousObject.GeometryMetadataJson = JsonSerializer.Serialize(previousMetadata);
                    }
                }
            }

            canvasObject.BaselineParcelId = parcel.Id;
            canvasObject.LabelText = parcel.ParcelNo;
            parcel.CanvasObjectId = canvasObject.Id;

            CadastralCanvasMetadata? metadata = ReadMetadata(canvasObject.GeometryMetadataJson) ?? new CadastralCanvasMetadata();
            ApplyParcelMetadata(metadata, parcel, "ManualAssigned");
            canvasObject.GeometryMetadataJson = JsonSerializer.Serialize(metadata);
            canvasObject.LastModifiedDate = DateTime.Now;
            parcel.LastModifiedDate = DateTime.Now;

            await context.SaveChangesAsync(ct);
        }

        private static Dictionary<string, string> BuildLayerMapSheetLookup(
            IReadOnlyList<CadastralLayerMapSheetMapping> layerMappings)
        {
            Dictionary<string, string> lookup = new(StringComparer.OrdinalIgnoreCase);
            foreach (CadastralLayerMapSheetMapping mapping in layerMappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.MapSheetNo))
                    continue;

                if (!string.IsNullOrWhiteSpace(mapping.LayerName))
                    lookup[mapping.LayerName.Trim()] = mapping.MapSheetNo.Trim();

                if (!string.IsNullOrWhiteSpace(mapping.SourceLayer))
                    lookup[mapping.SourceLayer.Trim()] = mapping.MapSheetNo.Trim();
            }

            return lookup;
        }

        private static void ApplyParcelMetadata(
            CadastralCanvasMetadata metadata,
            BaselineParcel parcel,
            string assignmentStatus)
        {
            metadata.MapSheetNo = parcel.MapSheetNo;
            metadata.ParcelNo = parcel.ParcelNo;
            metadata.BaselineParcelId = parcel.Id;
            metadata.FullUniqueParcelCode = parcel.FullUniqueParcelCode;
            metadata.RecordAreaSqm = parcel.OriginalAreaSqm;
            metadata.OwnerName = parcel.LandOwner?.FullName;
            metadata.LandUse = parcel.LandUse;
            metadata.AssignmentStatus = assignmentStatus;
        }

        private static string? ResolveMapSheetNo(
            CanvasObject canvasObject,
            CadastralCanvasMetadata metadata,
            IReadOnlyDictionary<string, string> layerMapSheets)
        {
            if (!string.IsNullOrWhiteSpace(metadata.SourceLayer) &&
                layerMapSheets.TryGetValue(metadata.SourceLayer.Trim(), out string? sourceMapSheet))
            {
                return sourceMapSheet;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.CanvasLayer?.Name) &&
                layerMapSheets.TryGetValue(canvasObject.CanvasLayer.Name.Trim(), out string? layerMapSheet))
            {
                return layerMapSheet;
            }

            return metadata.MapSheetNo;
        }

        private static STRtree<CadastralTextAssignmentFeature> BuildTextIndex(
            IEnumerable<CadastralTextAssignmentFeature> textFeatures)
        {
            STRtree<CadastralTextAssignmentFeature> index = new();
            foreach (CadastralTextAssignmentFeature text in textFeatures)
                index.Insert(text.Point.EnvelopeInternal, text);

            index.Build();
            return index;
        }

        private static CadastralTextAssignmentFeature? ToTextFeature(CanvasObject canvasObject)
        {
            if (canvasObject.Shape == null)
                return null;

            string? text = ExtractParcelText(canvasObject.LabelText);
            if (string.IsNullOrWhiteSpace(text))
            {
                CadastralCanvasMetadata? metadata = ReadMetadata(canvasObject.GeometryMetadataJson);
                text = ExtractParcelText(metadata?.ParcelNo ?? metadata?.MatchedText);
            }

            if (string.IsNullOrWhiteSpace(text))
                return null;

            Coordinate coordinate = canvasObject.Shape switch
            {
                NtsPoint point => point.Coordinate,
                _ => canvasObject.Shape.Centroid.Coordinate
            };

            return new CadastralTextAssignmentFeature(
                GeometryFactory.Default.CreatePoint(coordinate),
                text);
        }

        private static string? FindParcelTextInsidePolygon(
            Geometry? polygon,
            STRtree<CadastralTextAssignmentFeature> textIndex)
        {
            if (polygon == null || polygon.IsEmpty)
                return null;

            List<CadastralTextAssignmentFeature> candidates = textIndex.Query(polygon.EnvelopeInternal)
                .Where(text => polygon.Covers(text.Point) || polygon.Buffer(0.01).Covers(text.Point))
                .OrderBy(text => IsNumericParcelText(text.Text) ? 0 : 1)
                .ThenBy(text => text.Point.Distance(polygon.InteriorPoint))
                .ToList();

            return candidates.FirstOrDefault()?.Text;
        }

        private static bool IsNumericParcelText(string text)
        {
            return text.All(char.IsDigit);
        }

        private static string? ExtractParcelText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value
                .Replace("\\P", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
            Match match = ParcelTextRegex.Match(normalized);
            return match.Success ? match.Value.Trim() : normalized;
        }

        private static async Task ClearPreviousParcelLinkedToObjectAsync(
            DbContext context,
            Guid canvasObjectId,
            CancellationToken ct)
        {
            BaselineParcel? existingParcel = await context.Set<BaselineParcel>()
                .FirstOrDefaultAsync(item => item.CanvasObjectId == canvasObjectId, ct);
            if (existingParcel != null)
            {
                existingParcel.CanvasObjectId = null;
                existingParcel.LastModifiedDate = DateTime.Now;
            }
        }

        private static async Task ClearPreviousObjectLinkedToParcelAsync(
            DbContext context,
            BaselineParcel parcel,
            Guid replacementCanvasObjectId,
            CancellationToken ct)
        {
            if (!parcel.CanvasObjectId.HasValue ||
                parcel.CanvasObjectId.Value == replacementCanvasObjectId)
            {
                return;
            }

            CanvasObject? previousObject = await context.Set<CanvasObject>()
                .FirstOrDefaultAsync(item => item.Id == parcel.CanvasObjectId.Value, ct);
            if (previousObject == null)
                return;

            previousObject.BaselineParcelId = null;
            previousObject.LabelText = null;
            CadastralCanvasMetadata? previousMetadata = ReadMetadata(previousObject.GeometryMetadataJson);
            if (previousMetadata != null)
            {
                previousMetadata.BaselineParcelId = null;
                previousMetadata.AssignmentStatus = "Unassigned";
                previousObject.GeometryMetadataJson = JsonSerializer.Serialize(previousMetadata);
            }

            previousObject.LastModifiedDate = DateTime.Now;
        }

        private static CadastralAssignmentCandidate ToCandidate(CanvasObject canvasObject)
        {
            CadastralCanvasMetadata? metadata = ReadMetadata(canvasObject.GeometryMetadataJson);
            return new CadastralAssignmentCandidate(
                canvasObject.Id,
                canvasObject.CanvasLayer?.Name ?? "Unknown layer",
                canvasObject.ObjectType,
                metadata?.SourceLayer,
                metadata?.MapSheetNo,
                metadata?.ParcelNo,
                canvasObject.BaselineParcelId ?? metadata?.BaselineParcelId,
                metadata?.AssignmentStatus ?? "Unassigned",
                metadata?.CalculatedAreaSqm ?? Math.Abs(canvasObject.Shape?.Area ?? 0.0));
        }

        private static CadastralCanvasMetadata? ReadMetadata(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                CadastralCanvasMetadata? metadata =
                    JsonSerializer.Deserialize<CadastralCanvasMetadata>(json, JsonOptions);
                return string.Equals(metadata?.Kind, CadastralCanvasMetadata.MetadataKind, StringComparison.OrdinalIgnoreCase)
                    ? metadata
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private static string BuildParcelCode(string? mapSheetNo, string? parcelNo)
        {
            return $"{(mapSheetNo ?? string.Empty).Trim().ToUpperInvariant()}::{(parcelNo ?? string.Empty).Trim().ToUpperInvariant()}";
        }

        private sealed record CadastralTextAssignmentFeature(
            NtsPoint Point,
            string Text);
    }
}
