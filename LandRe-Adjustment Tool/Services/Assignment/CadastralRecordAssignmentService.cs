using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Microsoft.EntityFrameworkCore;

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
            var context = session.GetDbContext();

            List<BaselineParcel> records = await context.BaselineParcels.ToListAsync(ct);
            Dictionary<string, BaselineParcel> lookup = records
                .GroupBy(record => BuildParcelCode(record.MapSheetNo, record.ParcelNo), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            List<CanvasObject> candidates = await context.CanvasObjects
                .Where(canvasObject => canvasObject.GeometryMetadataJson != null &&
                                       canvasObject.GeometryMetadataJson.Contains(CadastralCanvasMetadata.MetadataKind))
                .ToListAsync(ct);

            int assigned = 0;
            int missingKey = 0;
            int noMatch = 0;

            foreach (CanvasObject canvasObject in candidates)
            {
                CadastralCanvasMetadata? metadata = ReadMetadata(canvasObject.GeometryMetadataJson);
                if (metadata == null)
                    continue;

                if (string.IsNullOrWhiteSpace(metadata.MapSheetNo) ||
                    string.IsNullOrWhiteSpace(metadata.ParcelNo))
                {
                    missingKey++;
                    continue;
                }

                string key = BuildParcelCode(metadata.MapSheetNo, metadata.ParcelNo);
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

                canvasObject.BaselineParcelId = parcel.Id;
                canvasObject.LabelText = metadata.ParcelNo;
                parcel.CanvasObjectId = canvasObject.Id;
                metadata.BaselineParcelId = parcel.Id;
                metadata.AssignmentStatus = "AutoAssigned";
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
            metadata.MapSheetNo = parcel.MapSheetNo;
            metadata.ParcelNo = parcel.ParcelNo;
            metadata.BaselineParcelId = parcel.Id;
            metadata.AssignmentStatus = "ManualAssigned";
            canvasObject.GeometryMetadataJson = JsonSerializer.Serialize(metadata);
            canvasObject.LastModifiedDate = DateTime.Now;
            parcel.LastModifiedDate = DateTime.Now;

            await context.SaveChangesAsync(ct);
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
    }
}
