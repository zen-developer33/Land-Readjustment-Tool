using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Entities.Replotting;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using BaselineRecord = Land_Readjustment_Tool.Models.BaselineLandParceRecord;

namespace Land_Readjustment_Tool.Services.Import
{
    public sealed class ImportPersistenceService : IImportPersistenceService
    {
        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;
        private readonly IImportManagerService _importManagerService;

        public ImportPersistenceService(ProjectSession session)
        {
            _context = session.GetDbContext();
            _logger = session.Logger;
            _importManagerService = new ImportManagerService(session);
        }

        public async Task<(int Owners, int Parcels)> GetExistingCountsAsync(CancellationToken ct = default)
        {
            var owners = await _context.LandOwners.CountAsync(ct);
            var parcels = await _context.BaselineParcels.CountAsync(ct);
            return (owners, parcels);
        }

        public async Task<ImportPersistenceResult> PersistImportAsync(
            IReadOnlyList<BaselineRecord> records,
            OwnerDeduplicationService.DeduplicationResult? deduplicationResult,
            bool replaceExistingData,
            string sourceFileName,
            string? sourceFilePath,
            string? sheetName,
            CancellationToken ct = default)
        {
            if (records == null || records.Count == 0)
                throw new InvalidOperationException("No records available for persistence.");

            ValidateRecordsBeforeSave(records);

            var initialOwners = await _context.LandOwners.CountAsync(ct);
            var initialParcels = await _context.BaselineParcels.CountAsync(ct);

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var deletedOwners = 0;
                var deletedParcels = 0;

                if (replaceExistingData)
                {
                    await EnsureReplacementIsSafeAsync(ct);
                    (deletedOwners, deletedParcels) = await ClearExistingImportDataAsync(ct);
                }

                var session = await _importManagerService.StageNormalizedRecordsAsync(
                    records,
                    sourceFileName,
                    sourceFilePath,
                    sheetName,
                    ct);

                var ownerSeeds = BuildOwnerSeeds(records, deduplicationResult);
                var ownerUpsert = await UpsertOwnersAsync(ownerSeeds, ct);
                var (savedParcels, skippedDuplicates) = await SaveParcelsAsync(records, session.Id, ownerUpsert.OwnerIdByRow, ct);

                await tx.CommitAsync(ct);

                var result = new ImportPersistenceResult
                {
                    ReplacedExistingData = replaceExistingData,
                    InitialOwners = initialOwners,
                    InitialParcels = initialParcels,
                    DeletedOwners = deletedOwners,
                    DeletedParcels = deletedParcels,
                    SavedOwners = ownerUpsert.OwnerIdByRow.Values.Distinct().Count(),
                    NewOwnersCreated = ownerUpsert.NewOwnersCreated,
                    ExistingOwnersUpdated = ownerUpsert.ExistingOwnersUpdated,
                    SavedParcels = savedParcels,
                    SkippedDuplicateParcels = skippedDuplicates,
                    ImportSessionId = session.Id
                };

                _logger.LogInfo(
                    $"Import persisted. Session={result.ImportSessionId}, Owners={result.SavedOwners}, " +
                    $"Parcels={result.SavedParcels}, Skipped={result.SkippedDuplicateParcels}, " +
                    $"Replace={result.ReplacedExistingData}");

                return result;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError("PersistImportAsync failed.", ex);
                throw;
            }
        }

        private static void ValidateRecordsBeforeSave(IReadOnlyList<BaselineRecord> records)
        {
            var errors = new List<string>();
            var duplicateKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < records.Count; i++)
            {
                var rowNo = i + 1;
                var rowErrors = DataTransformationService.ValidateSingleRecord(records[i], rowNo);
                if (rowErrors.Count > 0)
                {
                    errors.AddRange(rowErrors.Select(err => $"Row {rowNo}: {err}"));
                }

                var mapSheet = records[i].MapSheetNo?.Trim();
                var parcelNo = records[i].ParcelNo?.Trim();
                if (!string.IsNullOrWhiteSpace(mapSheet) && !string.IsNullOrWhiteSpace(parcelNo))
                {
                    var key = $"{mapSheet.ToUpperInvariant()}::{parcelNo.ToUpperInvariant()}";
                    if (!duplicateKeys.Add(key))
                    {
                        errors.Add($"Row {rowNo}: Duplicate ParcelNo + MapSheetNo in import set ({parcelNo}/{mapSheet}).");
                    }
                }
            }

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Take(20)));
        }

        private async Task EnsureReplacementIsSafeAsync(CancellationToken ct)
        {
            var hasReplotData =
                await _context.ReplottedParcels.AnyAsync(ct) ||
                await _context.OriginalToReplottedMaps.AnyAsync(ct) ||
                await _context.ReplottedParcelOwners.AnyAsync(ct);

            if (hasReplotData)
            {
                throw new InvalidOperationException(
                    "Cannot replace imported land data because replotting data already exists. " +
                    "Create a new project or clear replotting workflow first.");
            }
        }

        private async Task<(int DeletedOwners, int DeletedParcels)> ClearExistingImportDataAsync(CancellationToken ct)
        {
            var deletedOwners = await _context.LandOwners.CountAsync(ct);
            var deletedParcels = await _context.BaselineParcels.CountAsync(ct);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblParcelContributions;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblParcelContributionSummaries;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblParcelFrontages WHERE BaselineParcelId IS NOT NULL;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblBaselineParcels;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblMalpotReferences;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblLandOwners;", ct);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblCitizenshipConflictRecords;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblCitizenshipConflicts;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblValidationErrors;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblImportedRawRecords;", ct);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tblImportSessions;", ct);

            return (deletedOwners, deletedParcels);
        }

        private static Dictionary<int, OwnerSeed> BuildOwnerSeeds(
            IReadOnlyList<BaselineRecord> records,
            OwnerDeduplicationService.DeduplicationResult? deduplicationResult)
        {
            var seeds = new Dictionary<int, OwnerSeed>();

            if (deduplicationResult?.UniqueOwners?.Count > 0)
            {
                foreach (var unique in deduplicationResult.UniqueOwners)
                {
                    var seed = new OwnerSeed
                    {
                        FullName = NormalizeText(unique.LandOwnersName) ?? "Unknown Owner",
                        FatherOrSpouseName = NormalizeText(unique.FatherSpouse),
                        Gender = NormalizeText(unique.Gender),
                        CitizenshipNumber = NormalizeText(unique.CitizenshipNumber),
                        CitizenshipIssueDistrict = NormalizeText(unique.CitizenshipIssuedDistrict),
                        CitizenshipIssueDate = NormalizeText(unique.CitizenshipIssuedDate),
                        PermanentAddress = NormalizeText(unique.PermanentAddress),
                        TemporaryAddress = NormalizeText(unique.TemporaryAddress),
                        ContactNumber = NormalizeText(unique.ContactNumber),
                        Email = NormalizeText(unique.EmailID),
                        IdentificationMethod = string.IsNullOrWhiteSpace(unique.CitizenshipNumber) ? "NameFatherFuzzy" : "CitizenshipNumber",
                        MatchConfidenceScore = unique.CompletenessScore / 10.0,
                        NeedsManualReview = false
                    };

                    foreach (var index in unique.ParcelIndices)
                    {
                        if (!seeds.ContainsKey(index))
                            seeds[index] = seed;
                    }
                }
            }

            for (int i = 0; i < records.Count; i++)
            {
                if (seeds.ContainsKey(i))
                    continue;

                var record = records[i];
                seeds[i] = new OwnerSeed
                {
                    FullName = NormalizeText(record.LandOwnersName) ?? "Unknown Owner",
                    FatherOrSpouseName = NormalizeText(record.FatherSpouse),
                    Gender = NormalizeText(record.Gender),
                    CitizenshipNumber = NormalizeText(record.CitizenshipNumber),
                    CitizenshipIssueDistrict = NormalizeText(record.CitizenshipIssuedDistrict),
                    CitizenshipIssueDate = NormalizeText(record.citizenshipIssuedDate),
                    PermanentAddress = NormalizeText(record.PermanentAddress),
                    TemporaryAddress = NormalizeText(record.TempoaryAddress),
                    ContactNumber = NormalizeText(record.ContactNumber),
                    Email = NormalizeText(record.EmailID),
                    IdentificationMethod = string.IsNullOrWhiteSpace(record.CitizenshipNumber) ? "NameFatherFuzzy" : "CitizenshipNumber",
                    MatchConfidenceScore = 0.8,
                    NeedsManualReview = false
                };
            }

            return seeds;
        }

        private async Task<OwnerUpsertResult> UpsertOwnersAsync(
            Dictionary<int, OwnerSeed> ownerSeedsByRow,
            CancellationToken ct)
        {
            var ownerByRow = new Dictionary<int, LandOwner>(ownerSeedsByRow.Count);
            var ownerCache = await _context.LandOwners.ToListAsync(ct);
            var newOwnersCreated = 0;
            var existingOwnersUpdated = 0;

            foreach (var pair in ownerSeedsByRow)
            {
                var rowIndex = pair.Key;
                var seed = pair.Value;

                var matched = ownerCache.FirstOrDefault(o => IsOwnerMatch(o, seed));
                if (matched == null)
                {
                    matched = new LandOwner
                    {
                        FullName = seed.FullName,
                        FatherOrSpouseName = seed.FatherOrSpouseName,
                        Gender = seed.Gender,
                        CitizenshipNumber = seed.CitizenshipNumber,
                        CitizenshipIssueDistrict = seed.CitizenshipIssueDistrict,
                        CitizenshipIssueDate = seed.CitizenshipIssueDate,
                        PermanentAddress = seed.PermanentAddress,
                        TemporaryAddress = seed.TemporaryAddress,
                        ContactNumber = seed.ContactNumber,
                        Email = seed.Email,
                        IdentificationMethod = seed.IdentificationMethod,
                        MatchConfidenceScore = seed.MatchConfidenceScore,
                        NeedsManualReview = seed.NeedsManualReview,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    };

                    _context.LandOwners.Add(matched);
                    ownerCache.Add(matched);
                    newOwnersCreated++;
                }
                else
                {
                    MergeOwnerData(matched, seed);
                    matched.LastModifiedDate = DateTime.UtcNow;
                    existingOwnersUpdated++;
                }

                ownerByRow[rowIndex] = matched;
            }

            await _context.SaveChangesAsync(ct);

            var ownerIdByRow = ownerByRow.ToDictionary(p => p.Key, p => p.Value.Id);
            return new OwnerUpsertResult(ownerIdByRow, newOwnersCreated, existingOwnersUpdated);
        }

        private async Task<(int SavedParcels, int SkippedDuplicates)> SaveParcelsAsync(
            IReadOnlyList<BaselineRecord> records,
            int importSessionId,
            Dictionary<int, int> ownerIdByRow,
            CancellationToken ct)
        {
            var existingCodes = new HashSet<string>(
                await _context.BaselineParcels
                    .Select(p => p.FullUniqueParcelCode)
                    .ToListAsync(ct),
                StringComparer.OrdinalIgnoreCase);

            var savedParcels = 0;
            var skippedDuplicates = 0;
            var parcelsToAdd = new List<BaselineParcel>();

            var ownerIds = ownerIdByRow.Values.Distinct().ToList();
            var malpotRefs = await _context.MalpotReferences
                .Where(m => ownerIds.Contains(m.LandOwnerId))
                .ToListAsync(ct);
            var malpotByOwnerAndMoth = malpotRefs.ToDictionary(
                m => BuildMalpotKey(m.LandOwnerId, m.MothNo),
                m => m,
                StringComparer.OrdinalIgnoreCase);

            foreach (var pair in ownerIdByRow)
            {
                var rowIndex = pair.Key;
                var ownerId = pair.Value;
                var record = records[rowIndex];

                var mapSheetNo = record.MapSheetNo?.Trim() ?? string.Empty;
                var parcelNo = record.ParcelNo?.Trim() ?? string.Empty;
                var fullCode = BuildParcelCode(mapSheetNo, parcelNo);

                if (existingCodes.Contains(fullCode))
                {
                    skippedDuplicates++;
                    continue;
                }

                int? malpotRefId = null;
                MalpotReference? malpotRefEntity = null;
                var mothNo = NormalizeText(record.MothNo);
                if (!string.IsNullOrWhiteSpace(mothNo))
                {
                    var malpotKey = BuildMalpotKey(ownerId, mothNo);
                    var existingMalpot = malpotByOwnerAndMoth.GetValueOrDefault(malpotKey);

                    if (existingMalpot == null)
                    {
                        existingMalpot = new MalpotReference
                        {
                            LandOwnerId = ownerId,
                            MothNo = mothNo,
                            PaanaNo = NormalizeText(record.PaanaNo) ?? string.Empty
                        };
                        _context.MalpotReferences.Add(existingMalpot);
                        malpotRefs.Add(existingMalpot);
                        malpotByOwnerAndMoth[malpotKey] = existingMalpot;
                    }

                    if (existingMalpot.Id > 0)
                    {
                        malpotRefId = existingMalpot.Id;
                    }
                    else
                    {
                        malpotRefEntity = existingMalpot;
                    }
                }

                var parcel = new BaselineParcel
                {
                    ImportSessionId = importSessionId,
                    LandOwnerId = ownerId,
                    MalpotReferenceId = malpotRefId,
                    MalpotReference = malpotRefEntity,
                    MapSheetNo = mapSheetNo,
                    ParcelNo = parcelNo,
                    FullUniqueParcelCode = fullCode,
                    Province = NormalizeText(record.Province),
                    District = NormalizeText(record.District),
                    Municipality = NormalizeText(record.MunicipalityVillage),
                    WardNo = NormalizeText(record.WardNo),
                    OriginalAreaSqm = record.AreaInSqm ?? 0,
                    EffectiveAreaSqm = null,
                    IsEffectiveAreaManual = false,
                    LandUse = NormalizeText(record.LandUse),
                    HasTenant = ParseTenant(record.Tenant),
                    TenantName = null,
                    Remarks = NormalizeText(record.Remarks),
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                parcelsToAdd.Add(parcel);

                existingCodes.Add(fullCode);
                savedParcels++;
            }

            if (parcelsToAdd.Count > 0)
            {
                await _context.BaselineParcels.AddRangeAsync(parcelsToAdd, ct);
                await _context.SaveChangesAsync(ct);
            }

            return (savedParcels, skippedDuplicates);
        }

        private static string BuildParcelCode(string mapSheetNo, string parcelNo)
        {
            return $"{mapSheetNo.ToUpperInvariant()}::{parcelNo.ToUpperInvariant()}";
        }

        private static string BuildMalpotKey(int ownerId, string mothNo)
        {
            return $"{ownerId}::{mothNo.Trim().ToUpperInvariant()}";
        }

        private static bool ParseTenant(string? tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant))
                return false;

            var normalized = tenant.Trim().ToLowerInvariant();
            return normalized is "yes" or "y" or "true" or "1" or "mohi";
        }

        private static string? NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }

        private static bool IsOwnerMatch(LandOwner existing, OwnerSeed incoming)
        {
            if (!string.IsNullOrWhiteSpace(incoming.CitizenshipNumber) &&
                !string.IsNullOrWhiteSpace(existing.CitizenshipNumber))
            {
                return string.Equals(
                    existing.CitizenshipNumber.Trim(),
                    incoming.CitizenshipNumber.Trim(),
                    StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(existing.FullName.Trim(), incoming.FullName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                   string.Equals((existing.FatherOrSpouseName ?? string.Empty).Trim(),
                                 (incoming.FatherOrSpouseName ?? string.Empty).Trim(),
                                 StringComparison.OrdinalIgnoreCase);
        }

        private static void MergeOwnerData(LandOwner target, OwnerSeed source)
        {
            target.FatherOrSpouseName ??= source.FatherOrSpouseName;
            target.Gender ??= source.Gender;
            target.CitizenshipNumber ??= source.CitizenshipNumber;
            target.CitizenshipIssueDistrict ??= source.CitizenshipIssueDistrict;
            target.CitizenshipIssueDate ??= source.CitizenshipIssueDate;
            target.PermanentAddress ??= source.PermanentAddress;
            target.TemporaryAddress ??= source.TemporaryAddress;
            target.ContactNumber ??= source.ContactNumber;
            target.Email ??= source.Email;
            target.MatchConfidenceScore = Math.Max(target.MatchConfidenceScore ?? 0, source.MatchConfidenceScore ?? 0);
            target.NeedsManualReview = target.NeedsManualReview || source.NeedsManualReview;
        }

        private sealed class OwnerSeed
        {
            public string FullName { get; init; } = "Unknown Owner";
            public string? FatherOrSpouseName { get; init; }
            public string? Gender { get; init; }
            public string? CitizenshipNumber { get; init; }
            public string? CitizenshipIssueDistrict { get; init; }
            public string? CitizenshipIssueDate { get; init; }
            public string? PermanentAddress { get; init; }
            public string? TemporaryAddress { get; init; }
            public string? ContactNumber { get; init; }
            public string? Email { get; init; }
            public string IdentificationMethod { get; init; } = "NameFatherFuzzy";
            public double? MatchConfidenceScore { get; init; }
            public bool NeedsManualReview { get; init; }
        }

        private sealed record OwnerUpsertResult(
            Dictionary<int, int> OwnerIdByRow,
            int NewOwnersCreated,
            int ExistingOwnersUpdated);
    }
}
