using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Models;
using Microsoft.EntityFrameworkCore;
using CoreLandOwner = Land_Readjustment_Tool.Core.Entities.LandData.LandOwner;

namespace Land_Readjustment_Tool.Services
{
    public sealed class DataQualityReviewService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ProjectSession _session;

        public DataQualityReviewService(ProjectSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public sealed record OwnerReviewRecordSet(
            BindingList<BaselineLandParcelRecord> Records,
            IReadOnlyDictionary<int, int> ParcelIdByRecordIndex);

        public sealed record OwnerReviewPersistenceResult(
            int ParcelsUpdated,
            int OwnersCreated,
            int OwnersUpdated,
            int OrphanOwnersRemoved);

        public sealed class MissingGeometryIssue
        {
            public int ParcelId { get; init; }
            public string MapSheetNo { get; init; } = string.Empty;
            public string ParcelNo { get; init; } = string.Empty;
            public string OwnerName { get; init; } = string.Empty;
            public double AreaSqm { get; init; }
            public Guid? CanvasObjectId { get; init; }
            public string IssueType { get; init; } = string.Empty;
            public string Detail { get; init; } = string.Empty;
            public bool CanClearParcelLink { get; init; }
        }

        public async Task<OwnerReviewRecordSet> LoadOwnerReviewRecordsAsync(
            CancellationToken ct = default)
        {
            AppDbContext context = _session.GetDbContext();
            List<BaselineParcel> parcels = await context.BaselineParcels
                .AsNoTracking()
                .Include(parcel => parcel.LandOwner)
                .Include(parcel => parcel.CoOwners)
                    .ThenInclude(coOwner => coOwner.LandOwner)
                .Include(parcel => parcel.MalpotReference)
                .OrderBy(parcel => parcel.MapSheetNo)
                .ThenBy(parcel => parcel.ParcelNo)
                .ToListAsync(ct);

            BindingList<BaselineLandParcelRecord> records = new();
            Dictionary<int, int> indexMap = new();

            for (int index = 0; index < parcels.Count; index++)
            {
                BaselineParcel parcel = parcels[index];
                records.Add(ToReviewRecord(parcel));
                indexMap[index] = parcel.Id;
            }

            return new OwnerReviewRecordSet(records, indexMap);
        }

        public async Task<OwnerReviewPersistenceResult> SaveOwnerReviewRecordsAsync(
            OwnerReviewRecordSet recordSet,
            CancellationToken ct = default)
        {
            if (recordSet == null)
                throw new ArgumentNullException(nameof(recordSet));

            AppDbContext context = _session.GetDbContext();
            int parcelsUpdated = 0;
            int ownersCreated = 0;
            int ownersUpdated = 0;

            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            try
            {
                List<CoreLandOwner> ownerCache = await context.LandOwners.ToListAsync(ct);

                foreach (KeyValuePair<int, int> item in recordSet.ParcelIdByRecordIndex)
                {
                    if (item.Key < 0 || item.Key >= recordSet.Records.Count)
                        continue;

                    BaselineLandParcelRecord reviewRecord = recordSet.Records[item.Key];
                    BaselineParcel? parcel = await context.BaselineParcels
                        .Include(p => p.CoOwners)
                        .FirstOrDefaultAsync(p => p.Id == item.Value, ct);
                    if (parcel == null)
                        continue;

                    int originalOwnerId = parcel.LandOwnerId;
                    int ownerId = ResolveOrCreateOwner(
                        context,
                        ownerCache,
                        CreateOwnerSeed(reviewRecord),
                        ref ownersCreated,
                        ref ownersUpdated);

                    bool changed = false;
                    if (parcel.LandOwnerId != ownerId)
                    {
                        parcel.LandOwnerId = ownerId;
                        changed = true;
                    }

                    changed |= ReplaceCoOwners(
                        context,
                        ownerCache,
                        parcel,
                        reviewRecord.JointCoOwners,
                        ref ownersCreated,
                        ref ownersUpdated);

                    if (changed)
                    {
                        parcel.LastModifiedDate = DateTime.UtcNow;
                        parcelsUpdated++;
                    }

                    if (originalOwnerId != ownerId)
                        await MoveOrphanMalpotReferencesAsync(context, originalOwnerId, ownerId, ct);
                }

                await context.SaveChangesAsync(ct);
                int removed = await RemoveOrphanOwnersAsync(context, ct);
                await transaction.CommitAsync(ct);

                return new OwnerReviewPersistenceResult(
                    parcelsUpdated,
                    ownersCreated,
                    ownersUpdated,
                    removed);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<IReadOnlyList<MissingGeometryIssue>> GetMissingGeometryIssuesAsync(
            CancellationToken ct = default)
        {
            AppDbContext context = _session.GetDbContext();
            List<BaselineParcel> parcels = await context.BaselineParcels
                .AsNoTracking()
                .Include(parcel => parcel.LandOwner)
                .OrderBy(parcel => parcel.MapSheetNo)
                .ThenBy(parcel => parcel.ParcelNo)
                .ToListAsync(ct);

            HashSet<Guid> linkedObjectIds = parcels
                .Select(parcel => parcel.CanvasObjectId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            HashSet<int> parcelIds = parcels.Select(parcel => parcel.Id).ToHashSet();

            List<CanvasObject> linkedObjects = await context.CanvasObjects
                .AsNoTracking()
                .Where(obj =>
                    (obj.BaselineParcelId.HasValue && parcelIds.Contains(obj.BaselineParcelId.Value)) ||
                    linkedObjectIds.Contains(obj.Id))
                .ToListAsync(ct);

            Dictionary<Guid, CanvasObject> objectsById = linkedObjects
                .GroupBy(obj => obj.Id)
                .ToDictionary(group => group.Key, group => group.First());
            ILookup<int, CanvasObject> objectsByParcelId = linkedObjects
                .Where(obj => obj.BaselineParcelId.HasValue)
                .ToLookup(obj => obj.BaselineParcelId!.Value);

            List<MissingGeometryIssue> issues = new();
            foreach (BaselineParcel parcel in parcels)
            {
                CanvasObject? objectByStoredLink = parcel.CanvasObjectId.HasValue &&
                                                   objectsById.TryGetValue(parcel.CanvasObjectId.Value, out CanvasObject? stored)
                    ? stored
                    : null;
                List<CanvasObject> backLinkedObjects = objectsByParcelId[parcel.Id].ToList();

                if (!parcel.CanvasObjectId.HasValue && backLinkedObjects.Count == 0)
                {
                    issues.Add(ToIssue(
                        parcel,
                        null,
                        "Missing Geometry",
                        "No cadastral polygon is linked to this original parcel record.",
                        canClearParcelLink: false));
                    continue;
                }

                if (parcel.CanvasObjectId.HasValue && objectByStoredLink == null)
                {
                    issues.Add(ToIssue(
                        parcel,
                        parcel.CanvasObjectId,
                        "Broken Geometry Link",
                        "The parcel points to a canvas object that no longer exists.",
                        canClearParcelLink: true));
                    continue;
                }

                if (objectByStoredLink != null &&
                    objectByStoredLink.BaselineParcelId.HasValue &&
                    objectByStoredLink.BaselineParcelId.Value != parcel.Id)
                {
                    issues.Add(ToIssue(
                        parcel,
                        objectByStoredLink.Id,
                        "Conflicting Geometry Link",
                        $"The linked canvas object points to parcel #{objectByStoredLink.BaselineParcelId.Value}.",
                        canClearParcelLink: true));
                    continue;
                }

                if (objectByStoredLink != null &&
                    (objectByStoredLink.Shape == null || objectByStoredLink.Shape.IsEmpty))
                {
                    issues.Add(ToIssue(
                        parcel,
                        objectByStoredLink.Id,
                        "Empty Geometry",
                        "The linked canvas object exists, but its geometry is empty.",
                        canClearParcelLink: true));
                }

                if (parcel.CanvasObjectId.HasValue && objectByStoredLink != null &&
                    !objectByStoredLink.BaselineParcelId.HasValue)
                {
                    issues.Add(ToIssue(
                        parcel,
                        objectByStoredLink.Id,
                        "Back-Link Missing",
                        "The parcel points to a canvas object, but the object is not linked back to the parcel.",
                        canClearParcelLink: false));
                }

                if (backLinkedObjects.Count > 1)
                {
                    issues.Add(ToIssue(
                        parcel,
                        parcel.CanvasObjectId,
                        "Duplicate Geometry Links",
                        $"{backLinkedObjects.Count} canvas objects are linked to this parcel.",
                        canClearParcelLink: false));
                }
            }

            return issues;
        }

        public async Task<int> ClearBrokenGeometryLinksAsync(
            IEnumerable<int> parcelIds,
            CancellationToken ct = default)
        {
            HashSet<int> targets = parcelIds.ToHashSet();
            if (targets.Count == 0)
                return 0;

            AppDbContext context = _session.GetDbContext();
            List<BaselineParcel> parcels = await context.BaselineParcels
                .Where(parcel => targets.Contains(parcel.Id))
                .ToListAsync(ct);

            int cleared = 0;
            foreach (BaselineParcel parcel in parcels.Where(parcel => parcel.CanvasObjectId.HasValue))
            {
                parcel.CanvasObjectId = null;
                parcel.LastModifiedDate = DateTime.UtcNow;
                cleared++;
            }

            if (cleared > 0)
                await context.SaveChangesAsync(ct);

            return cleared;
        }

        public static string BuildMissingGeometryCsv(IEnumerable<MissingGeometryIssue> issues)
        {
            StringBuilder csv = new();
            csv.AppendLine("IssueType,MapSheetNo,ParcelNo,OwnerName,AreaSqm,CanvasObjectId,Detail");
            foreach (MissingGeometryIssue issue in issues)
            {
                csv.Append(Csv(issue.IssueType)).Append(',');
                csv.Append(Csv(issue.MapSheetNo)).Append(',');
                csv.Append(Csv(issue.ParcelNo)).Append(',');
                csv.Append(Csv(issue.OwnerName)).Append(',');
                csv.Append(issue.AreaSqm.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append(',');
                csv.Append(Csv(issue.CanvasObjectId?.ToString() ?? string.Empty)).Append(',');
                csv.AppendLine(Csv(issue.Detail));
            }

            return csv.ToString();
        }

        private static BaselineLandParcelRecord ToReviewRecord(BaselineParcel parcel)
        {
            return new BaselineLandParcelRecord
            {
                ParcelNo = parcel.ParcelNo,
                MapSheetNo = parcel.MapSheetNo,
                Province = parcel.Province,
                District = parcel.District,
                MunicipalityVillage = parcel.Municipality,
                WardNo = parcel.WardNo,
                LandOwnersName = parcel.LandOwner.FullName,
                FatherSpouse = parcel.LandOwner.FatherOrSpouseName,
                Gender = parcel.LandOwner.Gender,
                CitizenshipNumber = parcel.LandOwner.CitizenshipNumber,
                CitizenshipIssuedDistrict = parcel.LandOwner.CitizenshipIssueDistrict,
                CitizenshipIssuedDate = parcel.LandOwner.CitizenshipIssueDate,
                PermanentAddress = parcel.LandOwner.PermanentAddress,
                TemporaryAddress = parcel.LandOwner.TemporaryAddress,
                ContactNumber = parcel.LandOwner.ContactNumber,
                EmailID = parcel.LandOwner.Email,
                Tenant = parcel.HasTenant ? "Yes" : "No",
                TenantName = parcel.TenantName,
                LandUse = parcel.LandUse,
                LandOwnershipType = parcel.LandOwnershipType,
                AreaInSqm = parcel.OriginalAreaSqm,
                FieldMeasuredAreaSqm = parcel.FieldMeasuredAreaSqm,
                MothNo = parcel.MalpotReference?.MothNo,
                PaanaNo = parcel.MalpotReference?.PaanaNo,
                Remarks = parcel.Remarks,
                JointCoOwners = parcel.CoOwners
                    .OrderBy(coOwner => coOwner.Id)
                    .Select(coOwner => new CoOwnerRecord
                    {
                        OwnerName = coOwner.LandOwner.FullName,
                        FatherSpouse = coOwner.LandOwner.FatherOrSpouseName,
                        Gender = coOwner.LandOwner.Gender,
                        CitizenshipNumber = coOwner.LandOwner.CitizenshipNumber,
                        CitizenshipIssuedDistrict = coOwner.LandOwner.CitizenshipIssueDistrict,
                        CitizenshipIssuedDate = coOwner.LandOwner.CitizenshipIssueDate,
                        PermanentAddress = coOwner.LandOwner.PermanentAddress,
                        TemporaryAddress = coOwner.LandOwner.TemporaryAddress,
                        ContactNumber = coOwner.LandOwner.ContactNumber,
                        EmailID = coOwner.LandOwner.Email,
                        OwnershipSharePercent = coOwner.OwnershipSharePercent
                    })
                    .ToList()
            };
        }

        private static OwnerSeed CreateOwnerSeed(BaselineLandParcelRecord record)
        {
            return new OwnerSeed(
                record.LandOwnersName,
                record.FatherSpouse,
                record.Gender,
                record.CitizenshipNumber,
                record.CitizenshipIssuedDistrict,
                record.CitizenshipIssuedDate,
                record.PermanentAddress,
                record.TemporaryAddress,
                record.ContactNumber,
                record.EmailID);
        }

        private static OwnerSeed CreateOwnerSeed(CoOwnerRecord coOwner)
        {
            return new OwnerSeed(
                coOwner.OwnerName,
                coOwner.FatherSpouse,
                coOwner.Gender,
                coOwner.CitizenshipNumber,
                coOwner.CitizenshipIssuedDistrict,
                coOwner.CitizenshipIssuedDate,
                coOwner.PermanentAddress,
                coOwner.TemporaryAddress,
                coOwner.ContactNumber,
                coOwner.EmailID);
        }

        private static int ResolveOrCreateOwner(
            AppDbContext context,
            List<CoreLandOwner> ownerCache,
            OwnerSeed seed,
            ref int ownersCreated,
            ref int ownersUpdated)
        {
            string name = Clean(seed.Name) ?? "Unknown Owner";
            string normalizedName = NormalizeOwnerText(name);
            string normalizedFather = NormalizeOwnerText(seed.FatherSpouse);
            string normalizedCitizenship = NormalizeCitizenship(seed.CitizenshipNumber);

            CoreLandOwner? owner = null;
            if (!string.IsNullOrWhiteSpace(normalizedCitizenship))
            {
                owner = ownerCache.FirstOrDefault(existing =>
                    NormalizeCitizenship(existing.CitizenshipNumber) == normalizedCitizenship);
            }

            owner ??= ownerCache.FirstOrDefault(existing =>
                NormalizeOwnerText(existing.FullName) == normalizedName &&
                NormalizeOwnerText(existing.FatherOrSpouseName) == normalizedFather);

            if (owner == null)
            {
                owner = new CoreLandOwner
                {
                    FullName = name,
                    FatherOrSpouseName = Clean(seed.FatherSpouse),
                    Gender = Clean(seed.Gender),
                    CitizenshipNumber = Clean(seed.CitizenshipNumber),
                    CitizenshipIssueDistrict = Clean(seed.CitizenshipDistrict),
                    CitizenshipIssueDate = Clean(seed.CitizenshipDate),
                    PermanentAddress = Clean(seed.PermanentAddress),
                    TemporaryAddress = Clean(seed.TemporaryAddress),
                    ContactNumber = Clean(seed.ContactNumber),
                    Email = Clean(seed.Email),
                    IdentificationMethod = string.IsNullOrWhiteSpace(normalizedCitizenship)
                        ? "NameFatherReview"
                        : "CitizenshipNumberReview",
                    MatchConfidenceScore = 1.0,
                    NeedsManualReview = false,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                context.LandOwners.Add(owner);
                context.SaveChanges();
                ownerCache.Add(owner);
                ownersCreated++;
                return owner.Id;
            }

            bool changed = false;
            changed |= MergeIfEmpty(owner.FatherOrSpouseName, Clean(seed.FatherSpouse), value => owner.FatherOrSpouseName = value);
            changed |= MergeIfEmpty(owner.Gender, Clean(seed.Gender), value => owner.Gender = value);
            changed |= MergeIfEmpty(owner.CitizenshipNumber, Clean(seed.CitizenshipNumber), value => owner.CitizenshipNumber = value);
            changed |= MergeIfEmpty(owner.CitizenshipIssueDistrict, Clean(seed.CitizenshipDistrict), value => owner.CitizenshipIssueDistrict = value);
            changed |= MergeIfEmpty(owner.CitizenshipIssueDate, Clean(seed.CitizenshipDate), value => owner.CitizenshipIssueDate = value);
            changed |= MergeIfEmpty(owner.PermanentAddress, Clean(seed.PermanentAddress), value => owner.PermanentAddress = value);
            changed |= MergeIfEmpty(owner.TemporaryAddress, Clean(seed.TemporaryAddress), value => owner.TemporaryAddress = value);
            changed |= MergeIfEmpty(owner.ContactNumber, Clean(seed.ContactNumber), value => owner.ContactNumber = value);
            changed |= MergeIfEmpty(owner.Email, Clean(seed.Email), value => owner.Email = value);

            if (changed)
            {
                owner.LastModifiedDate = DateTime.UtcNow;
                ownersUpdated++;
            }

            return owner.Id;
        }

        private static bool ReplaceCoOwners(
            AppDbContext context,
            List<CoreLandOwner> ownerCache,
            BaselineParcel parcel,
            IEnumerable<CoOwnerRecord> coOwners,
            ref int ownersCreated,
            ref int ownersUpdated)
        {
            List<(int OwnerId, double? SharePercent)> target = new();
            HashSet<int> addedOwnerIds = new();

            foreach (CoOwnerRecord coOwner in coOwners)
            {
                if (string.IsNullOrWhiteSpace(coOwner.OwnerName))
                    continue;

                int ownerId = ResolveOrCreateOwner(
                    context,
                    ownerCache,
                    CreateOwnerSeed(coOwner),
                    ref ownersCreated,
                    ref ownersUpdated);
                if (ownerId == parcel.LandOwnerId || !addedOwnerIds.Add(ownerId))
                    continue;

                target.Add((ownerId, coOwner.OwnershipSharePercent));
            }

            bool changed = false;
            HashSet<int> targetOwnerIds = target.Select(item => item.OwnerId).ToHashSet();
            foreach (BaselineParcelCoOwner existing in parcel.CoOwners.ToList())
            {
                if (!targetOwnerIds.Contains(existing.LandOwnerId))
                {
                    context.BaselineParcelCoOwners.Remove(existing);
                    parcel.CoOwners.Remove(existing);
                    changed = true;
                    continue;
                }

                double? nextShare = target.First(item => item.OwnerId == existing.LandOwnerId).SharePercent;
                if (existing.OwnershipSharePercent != nextShare)
                {
                    existing.OwnershipSharePercent = nextShare;
                    changed = true;
                }
            }

            HashSet<int> existingOwnerIds = parcel.CoOwners.Select(item => item.LandOwnerId).ToHashSet();
            foreach ((int ownerId, double? sharePercent) in target.Where(item => !existingOwnerIds.Contains(item.OwnerId)))
            {
                parcel.CoOwners.Add(new BaselineParcelCoOwner
                {
                    BaselineParcel = parcel,
                    BaselineParcelId = parcel.Id,
                    LandOwnerId = ownerId,
                    OwnershipSharePercent = sharePercent,
                    CreatedDate = DateTime.UtcNow
                });
                changed = true;
            }

            return changed;
        }

        private static async Task MoveOrphanMalpotReferencesAsync(
            AppDbContext context,
            int fromOwnerId,
            int toOwnerId,
            CancellationToken ct)
        {
            bool oldOwnerStillHasParcels = await context.BaselineParcels
                .AnyAsync(parcel => parcel.LandOwnerId == fromOwnerId, ct);
            if (oldOwnerStillHasParcels)
                return;

            List<MalpotReference> oldRefs = await context.MalpotReferences
                .Where(reference => reference.LandOwnerId == fromOwnerId)
                .ToListAsync(ct);
            foreach (MalpotReference reference in oldRefs)
                reference.LandOwnerId = toOwnerId;
        }

        private static async Task<int> RemoveOrphanOwnersAsync(AppDbContext context, CancellationToken ct)
        {
            List<int> usedOwnerIds = await context.BaselineParcels
                .Select(parcel => parcel.LandOwnerId)
                .Concat(context.BaselineParcelCoOwners.Select(coOwner => coOwner.LandOwnerId))
                .Concat(context.ReplottedParcelOwners.Select(owner => owner.LandOwnerId))
                .Distinct()
                .ToListAsync(ct);
            HashSet<int> used = usedOwnerIds.ToHashSet();

            List<CoreLandOwner> orphanOwners = await context.LandOwners
                .Where(owner => !used.Contains(owner.Id))
                .ToListAsync(ct);
            if (orphanOwners.Count == 0)
                return 0;

            HashSet<int> orphanIds = orphanOwners.Select(owner => owner.Id).ToHashSet();
            context.MalpotReferences.RemoveRange(
                await context.MalpotReferences
                    .Where(reference => orphanIds.Contains(reference.LandOwnerId))
                    .ToListAsync(ct));
            context.LandOwners.RemoveRange(orphanOwners);
            await context.SaveChangesAsync(ct);
            return orphanOwners.Count;
        }

        private static MissingGeometryIssue ToIssue(
            BaselineParcel parcel,
            Guid? canvasObjectId,
            string issueType,
            string detail,
            bool canClearParcelLink)
        {
            return new MissingGeometryIssue
            {
                ParcelId = parcel.Id,
                MapSheetNo = parcel.MapSheetNo,
                ParcelNo = parcel.ParcelNo,
                OwnerName = parcel.LandOwner?.FullName ?? string.Empty,
                AreaSqm = parcel.OriginalAreaSqm,
                CanvasObjectId = canvasObjectId,
                IssueType = issueType,
                Detail = detail,
                CanClearParcelLink = canClearParcelLink
            };
        }

        private static CadastralCanvasMetadata? ReadMetadata(string? json)
        {
            if (string.IsNullOrWhiteSpace(json) ||
                !json.Contains(CadastralCanvasMetadata.MetadataKind, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<CadastralCanvasMetadata>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static bool MergeIfEmpty(string? current, string? candidate, Action<string?> assign)
        {
            if (!string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(candidate))
                return false;

            assign(candidate);
            return true;
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string NormalizeOwnerText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : OwnerDeduplicationService.NormalizeString(value);
        }

        private static string NormalizeCitizenship(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            StringBuilder builder = new(value.Length);
            foreach (char c in value)
            {
                if (c >= '\u0966' && c <= '\u096F')
                    builder.Append((char)('0' + (c - '\u0966')));
                else
                    builder.Append(c);
            }

            return new string(builder.ToString().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }

        private static string Csv(string value)
        {
            string escaped = value.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        private sealed record OwnerSeed(
            string? Name,
            string? FatherSpouse,
            string? Gender,
            string? CitizenshipNumber,
            string? CitizenshipDistrict,
            string? CitizenshipDate,
            string? PermanentAddress,
            string? TemporaryAddress,
            string? ContactNumber,
            string? Email);
    }
}
