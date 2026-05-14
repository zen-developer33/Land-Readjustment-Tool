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

        Task<CadastralAssignmentResult> AutoAssignFromAttributesAsync(
            ProjectSession session,
            bool replaceExistingAssignments,
            CadastralAttributeFieldMapping attributeMapping,
            CancellationToken ct = default);

        Task AssignManualAsync(
            ProjectSession session,
            Guid canvasObjectId,
            int baselineParcelId,
            bool replaceExistingAssignment,
            CancellationToken ct = default);

        Task<int> ClearAssignmentsAsync(
            ProjectSession session,
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

        public async Task<CadastralAssignmentResult> AutoAssignFromAttributesAsync(
            ProjectSession session,
            bool replaceExistingAssignments,
            CadastralAttributeFieldMapping attributeMapping,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(attributeMapping.ParcelField))
            {
                return new CadastralAssignmentResult(
                    false,
                    "Select the source parcel field before assigning from saved attributes.",
                    0,
                    0,
                    0);
            }

            var context = session.GetDbContext();
            List<BaselineParcel> records = await context.BaselineParcels
                .Include(parcel => parcel.LandOwner)
                .ToListAsync(ct);
            Dictionary<string, BaselineParcel> lookup = BuildParcelLookup(records);
            Dictionary<string, BaselineParcel?> uniqueParcelNumberLookup = BuildUniqueParcelNumberLookup(records);

            List<CanvasObject> parcelObjects = await context.CanvasObjects
                .Include(canvasObject => canvasObject.CanvasLayer)
                .Where(canvasObject => canvasObject.GeometryMetadataJson != null &&
                                       canvasObject.GeometryMetadataJson.Contains(CadastralCanvasMetadata.MetadataKind) &&
                                       canvasObject.ObjectType == "Polygon")
                .ToListAsync(ct);

            int assigned = 0;
            int missingKey = 0;
            int noMatch = 0;

            foreach (CanvasObject canvasObject in parcelObjects)
            {
                CadastralCanvasMetadata? metadata = ReadMetadata(canvasObject.GeometryMetadataJson);
                if (metadata == null || !IsAttributeMappedSource(metadata.SourceFormat))
                    continue;

                if (!replaceExistingAssignments && canvasObject.BaselineParcelId.HasValue)
                    continue;

                IReadOnlyDictionary<string, string?> attributes = ReadAttributeDictionary(metadata.AttributesJson);
                if (attributes.Count == 0)
                {
                    missingKey++;
                    continue;
                }

                string? parcelNo = TryGetAttribute(attributes, attributeMapping.ParcelField);
                string? sourceMapSheet = TryGetAttribute(attributes, attributeMapping.MapSheetField);
                string? mapSheetNo = ResolveMappedMapSheet(sourceMapSheet, attributeMapping.MapSheetValueMappings)
                                     ?? sourceMapSheet;
                if (string.IsNullOrWhiteSpace(parcelNo))
                {
                    missingKey++;
                    continue;
                }

                IReadOnlyList<string> parcelCandidates = [parcelNo];
                bool foundParcel = !string.IsNullOrWhiteSpace(mapSheetNo)
                    ? TryFindParcel(lookup, mapSheetNo, parcelCandidates, out BaselineParcel? parcel, out string matchedParcelNo)
                    : TryFindUniqueParcelByParcelNumber(uniqueParcelNumberLookup, parcelCandidates, out parcel, out matchedParcelNo);
                if (!foundParcel || parcel == null)
                {
                    if (string.IsNullOrWhiteSpace(mapSheetNo))
                        missingKey++;
                    else
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
                metadata.MatchedText = matchedParcelNo;
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
            Dictionary<string, BaselineParcel> lookup = BuildParcelLookup(records);
            Dictionary<string, BaselineParcel?> uniqueParcelNumberLookup = BuildUniqueParcelNumberLookup(records);
            HashSet<string> knownMapSheets = records
                .Select(parcel => parcel.MapSheetNo)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(NormalizeIdentifierForMatching)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

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
                    if (lookup.TryGetValue(canvasObject.BaselineParcelId.Value.ToString(), out BaselineParcel? linkedParcel))
                    {
                        bool changed = false;
                        if (linkedParcel.CanvasObjectId != canvasObject.Id)
                        {
                            linkedParcel.CanvasObjectId = canvasObject.Id;
                            linkedParcel.LastModifiedDate = DateTime.Now;
                            changed = true;
                        }

                        if (!string.Equals(metadata.AssignmentStatus, "AutoAssigned", StringComparison.OrdinalIgnoreCase))
                        {
                            ApplyParcelMetadata(metadata, linkedParcel, "AutoAssigned");
                            canvasObject.GeometryMetadataJson = JsonSerializer.Serialize(metadata);
                            canvasObject.LastModifiedDate = DateTime.Now;
                            changed = true;
                        }

                        if (changed)
                            assigned++;

                        continue;
                    }
                }

                string? mapSheetNo = ResolveMapSheetNo(canvasObject, metadata, layerMapSheets, knownMapSheets);
                string? detectedText = FindParcelTextInsidePolygon(canvasObject.Shape, textIndex);
                IReadOnlyList<string> parcelCandidates = BuildParcelNumberCandidates(metadata, detectedText);
                if (parcelCandidates.Count == 0)
                {
                    missingKey++;
                    continue;
                }

                bool foundParcel = !string.IsNullOrWhiteSpace(mapSheetNo)
                    ? TryFindParcel(lookup, mapSheetNo, parcelCandidates, out BaselineParcel? parcel, out string matchedParcelNo)
                    : TryFindUniqueParcelByParcelNumber(uniqueParcelNumberLookup, parcelCandidates, out parcel, out matchedParcelNo);
                if (!foundParcel)
                {
                    if (string.IsNullOrWhiteSpace(mapSheetNo))
                        missingKey++;
                    else
                        noMatch++;
                    continue;
                }
                if (parcel == null)
                {
                    noMatch++;
                    continue;
                }
                BaselineParcel matchedParcel = parcel;

                if (!replaceExistingAssignments &&
                    matchedParcel.CanvasObjectId.HasValue &&
                    matchedParcel.CanvasObjectId.Value != canvasObject.Id)
                {
                    noMatch++;
                    continue;
                }

                if (replaceExistingAssignments)
                {
                    await ClearPreviousParcelLinkedToObjectAsync(context, canvasObject.Id, ct);
                    await ClearPreviousObjectLinkedToParcelAsync(context, matchedParcel, canvasObject.Id, ct);
                }

                canvasObject.BaselineParcelId = matchedParcel.Id;
                canvasObject.LabelText = matchedParcel.ParcelNo;
                matchedParcel.CanvasObjectId = canvasObject.Id;
                metadata.MapSheetNo = matchedParcel.MapSheetNo;
                metadata.ParcelNo = matchedParcel.ParcelNo;
                metadata.MatchedText = matchedParcelNo;
                ApplyParcelMetadata(metadata, matchedParcel, "AutoAssigned");
                canvasObject.GeometryMetadataJson = JsonSerializer.Serialize(metadata);
                canvasObject.LastModifiedDate = DateTime.Now;
                matchedParcel.LastModifiedDate = DateTime.Now;
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

        private static Dictionary<string, BaselineParcel> BuildParcelLookup(
            IEnumerable<BaselineParcel> records)
        {
            Dictionary<string, BaselineParcel> lookup = new(StringComparer.OrdinalIgnoreCase);
            foreach (BaselineParcel record in records)
            {
                lookup.TryAdd(record.Id.ToString(), record);
                foreach (string key in BuildParcelLookupKeys(record.MapSheetNo, record.ParcelNo))
                    lookup.TryAdd(key, record);
            }

            return lookup;
        }

        private static Dictionary<string, BaselineParcel?> BuildUniqueParcelNumberLookup(
            IEnumerable<BaselineParcel> records)
        {
            Dictionary<string, BaselineParcel?> lookup = new(StringComparer.OrdinalIgnoreCase);
            foreach (BaselineParcel record in records)
            {
                foreach (string key in BuildParcelNumberLookupKeys(record.ParcelNo))
                {
                    if (lookup.TryGetValue(key, out BaselineParcel? existing) &&
                        existing?.Id != record.Id)
                    {
                        lookup[key] = null;
                        continue;
                    }

                    lookup.TryAdd(key, record);
                }
            }

            return lookup;
        }

        private static bool TryFindParcel(
            IReadOnlyDictionary<string, BaselineParcel> lookup,
            string mapSheetNo,
            IReadOnlyList<string> parcelCandidates,
            out BaselineParcel? parcel,
            out string matchedParcelNo)
        {
            foreach (string parcelNo in parcelCandidates)
            {
                foreach (string key in BuildParcelLookupKeys(mapSheetNo, parcelNo))
                {
                    if (lookup.TryGetValue(key, out parcel))
                    {
                        matchedParcelNo = parcelNo;
                        return true;
                    }
                }
            }

            parcel = null;
            matchedParcelNo = string.Empty;
            return false;
        }

        private static bool TryFindUniqueParcelByParcelNumber(
            IReadOnlyDictionary<string, BaselineParcel?> lookup,
            IReadOnlyList<string> parcelCandidates,
            out BaselineParcel? parcel,
            out string matchedParcelNo)
        {
            foreach (string parcelNo in parcelCandidates)
            {
                foreach (string key in BuildParcelNumberLookupKeys(parcelNo))
                {
                    if (lookup.TryGetValue(key, out parcel) && parcel != null)
                    {
                        matchedParcelNo = parcelNo;
                        return true;
                    }
                }
            }

            parcel = null;
            matchedParcelNo = string.Empty;
            return false;
        }

        private static IReadOnlyList<string> BuildParcelNumberCandidates(
            CadastralCanvasMetadata metadata,
            string? detectedText)
        {
            List<string> candidates = [];
            AddCandidate(candidates, ExtractParcelText(metadata.ParcelNo));
            AddCandidate(candidates, ExtractParcelText(metadata.MatchedText));
            AddCandidate(candidates, ExtractParcelText(detectedText));
            return candidates;
        }

        private static void AddCandidate(List<string> candidates, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (!candidates.Contains(value, StringComparer.OrdinalIgnoreCase))
                candidates.Add(value);
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

        public async Task<int> ClearAssignmentsAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            var context = session.GetDbContext();
            List<CanvasObject> parcelObjects = await context.CanvasObjects
                .Where(canvasObject => canvasObject.GeometryMetadataJson != null &&
                                       canvasObject.GeometryMetadataJson.Contains(CadastralCanvasMetadata.MetadataKind) &&
                                       canvasObject.ObjectType == "Polygon")
                .ToListAsync(ct);
            if (parcelObjects.Count == 0)
                return 0;

            HashSet<Guid> objectIds = parcelObjects.Select(item => item.Id).ToHashSet();
            HashSet<int> baselineParcelIds = parcelObjects
                .Select(item => item.BaselineParcelId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            List<BaselineParcel> linkedParcels = await context.BaselineParcels
                .Where(parcel =>
                    (parcel.CanvasObjectId.HasValue && objectIds.Contains(parcel.CanvasObjectId.Value)) ||
                    baselineParcelIds.Contains(parcel.Id))
                .ToListAsync(ct);

            int cleared = 0;
            foreach (CanvasObject canvasObject in parcelObjects)
            {
                CadastralCanvasMetadata? metadata = ReadMetadata(canvasObject.GeometryMetadataJson);
                bool hadAssignment = canvasObject.BaselineParcelId.HasValue ||
                                     metadata?.BaselineParcelId != null ||
                                     string.Equals(metadata?.AssignmentStatus, "AutoAssigned", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(metadata?.AssignmentStatus, "ManualAssigned", StringComparison.OrdinalIgnoreCase);
                if (!hadAssignment)
                    continue;

                canvasObject.BaselineParcelId = null;
                canvasObject.LabelText = metadata?.ParcelNo;
                if (metadata != null)
                {
                    metadata.BaselineParcelId = null;
                    metadata.FullUniqueParcelCode = null;
                    metadata.RecordAreaSqm = null;
                    metadata.OwnerName = null;
                    metadata.LandUse = null;
                    metadata.AssignmentStatus = "Unassigned";
                    canvasObject.GeometryMetadataJson = JsonSerializer.Serialize(metadata);
                }

                canvasObject.LastModifiedDate = DateTime.Now;
                cleared++;
            }

            foreach (BaselineParcel parcel in linkedParcels)
            {
                if (!parcel.CanvasObjectId.HasValue ||
                    !objectIds.Contains(parcel.CanvasObjectId.Value))
                {
                    continue;
                }

                parcel.CanvasObjectId = null;
                parcel.LastModifiedDate = DateTime.Now;
            }

            await context.SaveChangesAsync(ct);
            return cleared;
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
            IReadOnlyDictionary<string, string> layerMapSheets,
            IReadOnlySet<string> knownMapSheets)
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

            if (!string.IsNullOrWhiteSpace(metadata.MapSheetNo))
                return metadata.MapSheetNo;

            if (!string.IsNullOrWhiteSpace(metadata.SourceLayer) &&
                knownMapSheets.Contains(NormalizeIdentifierForMatching(metadata.SourceLayer)))
            {
                return metadata.SourceLayer;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.CanvasLayer?.Name) &&
                knownMapSheets.Contains(NormalizeIdentifierForMatching(canvasObject.CanvasLayer.Name)))
            {
                return canvasObject.CanvasLayer.Name;
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
                metadata?.SourceFormat,
                metadata?.SourceFileName,
                metadata?.AttributesJson,
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

        private static IReadOnlyDictionary<string, string?> ReadAttributeDictionary(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            try
            {
                Dictionary<string, string?>? attributes =
                    JsonSerializer.Deserialize<Dictionary<string, string?>>(json, JsonOptions);
                return attributes == null
                    ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string?>(attributes, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static string? TryGetAttribute(
            IReadOnlyDictionary<string, string?> attributes,
            string? fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return null;

            return attributes.TryGetValue(fieldName, out string? value)
                ? NormalizeAttributeText(value)
                : null;
        }

        private static string? NormalizeAttributeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value.Trim();
            return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
        }

        private static string? ResolveMappedMapSheet(
            string? sourceMapSheet,
            IReadOnlyDictionary<string, string> mappings)
        {
            if (string.IsNullOrWhiteSpace(sourceMapSheet))
                return null;

            string normalized = NormalizeIdentifierForMatching(sourceMapSheet);
            if (mappings.TryGetValue(normalized, out string? target))
                return target;

            return mappings.TryGetValue(sourceMapSheet.Trim(), out target)
                ? target
                : null;
        }

        private static bool IsAttributeMappedSource(string? sourceFormat)
        {
            return string.Equals(sourceFormat, "SHP", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(sourceFormat, "KML", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(sourceFormat, "KMZ", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildParcelCode(string? mapSheetNo, string? parcelNo)
        {
            return $"{(mapSheetNo ?? string.Empty).Trim().ToUpperInvariant()}::{(parcelNo ?? string.Empty).Trim().ToUpperInvariant()}";
        }

        private static IEnumerable<string> BuildParcelLookupKeys(string? mapSheetNo, string? parcelNo)
        {
            string exact = BuildParcelCode(mapSheetNo, parcelNo);
            yield return exact;

            string normalizedMapSheet = NormalizeIdentifierForMatching(mapSheetNo);
            string normalizedParcel = NormalizeIdentifierForMatching(parcelNo);
            string normalized = BuildParcelCode(normalizedMapSheet, normalizedParcel);
            if (!string.Equals(normalized, exact, StringComparison.OrdinalIgnoreCase))
                yield return normalized;

            string numericParcel = NormalizeNumericIdentifier(parcelNo);
            if (!string.Equals(numericParcel, normalizedParcel, StringComparison.OrdinalIgnoreCase))
                yield return BuildParcelCode(normalizedMapSheet, numericParcel);
        }

        private static IEnumerable<string> BuildParcelNumberLookupKeys(string? parcelNo)
        {
            string exact = (parcelNo ?? string.Empty).Trim().ToUpperInvariant();
            yield return exact;

            string normalized = NormalizeIdentifierForMatching(parcelNo);
            if (!string.Equals(normalized, exact, StringComparison.OrdinalIgnoreCase))
                yield return normalized;

            string numeric = NormalizeNumericIdentifier(parcelNo);
            if (!string.Equals(numeric, normalized, StringComparison.OrdinalIgnoreCase))
                yield return numeric;
        }

        private static string NormalizeIdentifierForMatching(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return Regex.Replace(value.Trim(), @"\s+", string.Empty).ToUpperInvariant();
        }

        private static string NormalizeNumericIdentifier(string? value)
        {
            string normalized = NormalizeIdentifierForMatching(value);
            if (!normalized.All(char.IsDigit))
                return normalized;

            string trimmed = normalized.TrimStart('0');
            return trimmed.Length == 0 ? "0" : trimmed;
        }

        private sealed record CadastralTextAssignmentFeature(
            NtsPoint Point,
            string Text);
    }
}
