using Land_Readjustment_Tool.Core.Entities.Import;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Entities.Replotting;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Constants;
using Land_Readjustment_Tool.Models;
using Microsoft.EntityFrameworkCore;
using LegacyLandOwner = Land_Readjustment_Tool.Models.LandOwner;
using LegacyParcel = Land_Readjustment_Tool.Models.OriginalLandParcel;
using CoreLandOwner = Land_Readjustment_Tool.Core.Entities.LandData.LandOwner;

namespace Land_Readjustment_Tool.Services.LandData
{
    /// <summary>
    /// EF Core backed service for land owner and baseline parcel views/edits.
    /// Keeps legacy form models while persisting to new normalized EF entities.
    /// </summary>
    public sealed class LandRecordsService
    {
        private readonly AppDbContext _context;
        private readonly string _projectFilePath;

        public LandRecordsService(ProjectSession session, string projectFilePath)
        {
            _context = session.GetDbContext();
            _projectFilePath = projectFilePath;
        }

        public sealed class OwnerParcelStats
        {
            public int ParcelCount { get; init; }
            public double TotalAreaSqm { get; init; }
        }

        public List<LegacyLandOwner> GetAllOwners()
        {
            return _context.LandOwners
                .AsNoTracking()
                .OrderBy(o => o.FullName)
                .Select(MapOwner)
                .ToList();
        }

        public Dictionary<int, OwnerParcelStats> GetOwnerParcelStats()
        {
            return _context.BaselineParcels
                .AsNoTracking()
                .GroupBy(p => p.LandOwnerId)
                .Select(group => new
                {
                    OwnerId = group.Key,
                    ParcelCount = group.Count(),
                    TotalAreaSqm = group.Sum(p => (double?)p.OriginalAreaSqm) ?? 0
                })
                .ToDictionary(
                    x => x.OwnerId,
                    x => new OwnerParcelStats
                    {
                        ParcelCount = x.ParcelCount,
                        TotalAreaSqm = x.TotalAreaSqm
                    });
        }

        public LegacyLandOwner? GetOwnerById(int ownerId)
        {
            var owner = _context.LandOwners
                .AsNoTracking()
                .FirstOrDefault(o => o.Id == ownerId);
            return owner == null ? null : MapOwner(owner);
        }

        public string GetTraditionalAreaUnit()
        {
            var unit = _context.ProjectSettings
                .AsNoTracking()
                .Select(s => s.TraditionalAreaUnit)
                .FirstOrDefault();

            return string.Equals(unit, "BKD", StringComparison.OrdinalIgnoreCase)
                ? "BKD"
                : "RAPD";
        }

        public (int SqmPrecision, int TraditionalPrecision) GetAreaPrecisionSettings()
        {
            var s = _context.ProjectSettings
                .AsNoTracking()
                .Select(ps => new { ps.AreaSqmDecimalPlaces, ps.TraditionalAreaLowestUnitDecimalPlaces })
                .FirstOrDefault();
            return s == null ? (3, 2) : (s.AreaSqmDecimalPlaces, s.TraditionalAreaLowestUnitDecimalPlaces);
        }

        public List<LegacyParcel> GetAllParcelsWithOwners()
        {
            var parcels = _context.BaselineParcels
                .AsNoTracking()
                .Include(p => p.LandOwner)
                .Include(p => p.CoOwners)
                    .ThenInclude(c => c.LandOwner)
                .Include(p => p.MalpotReference)
                .OrderBy(p => p.MapSheetNo)
                .ThenBy(p => p.ParcelNo)
                .ToList();

            var (sqmPrec, tradPrec) = GetAreaPrecisionSettings();
            return parcels.Select(p => MapParcel(p, sqmPrec, tradPrec)).ToList();
        }

        public List<LegacyParcel> GetParcelsByOwnerId(int ownerId)
        {
            var parcels = _context.BaselineParcels
                .AsNoTracking()
                .Include(p => p.LandOwner)
                .Include(p => p.CoOwners)
                    .ThenInclude(c => c.LandOwner)
                .Include(p => p.MalpotReference)
                .Where(p => p.LandOwnerId == ownerId)
                .OrderBy(p => p.MapSheetNo)
                .ThenBy(p => p.ParcelNo)
                .ToList();

            var (sqmPrec, tradPrec) = GetAreaPrecisionSettings();
            return parcels.Select(p => MapParcel(p, sqmPrec, tradPrec)).ToList();
        }

        public double GetTotalAreaByOwnerId(int ownerId)
        {
            return _context.BaselineParcels
                .AsNoTracking()
                .Where(p => p.LandOwnerId == ownerId)
                .Select(p => (double?)p.OriginalAreaSqm)
                .Sum() ?? 0;
        }

        public int GetDocumentCountByOwnerId(int ownerId)
        {
            var owner = _context.LandOwners
                .AsNoTracking()
                .FirstOrDefault(o => o.Id == ownerId);

            if (owner == null || string.IsNullOrWhiteSpace(owner.DocumentsFolderPath))
                return 0;

            var projectDir = Path.GetDirectoryName(_projectFilePath) ?? string.Empty;
            var docsFolder = Path.IsPathRooted(owner.DocumentsFolderPath)
                ? owner.DocumentsFolderPath
                : Path.Combine(projectDir, owner.DocumentsFolderPath);
            return Directory.Exists(docsFolder) ? Directory.GetFiles(docsFolder).Length : 0;
        }

        public bool OwnerExists(string? name, string? fatherSpouse, string? citizenshipNumber, int? excludeOwnerId = null)
        {
            var normalizedName = NormalizeName(name);
            if (string.IsNullOrWhiteSpace(normalizedName))
                return false;

            var isInstitution = IsInstitutionOwnerName(normalizedName);
            var normalizedFather = NormalizeName(fatherSpouse);
            var normalizedCitizenship = NormalizeCitizenship(citizenshipNumber);

            var query = _context.LandOwners.AsNoTracking().AsQueryable();
            if (excludeOwnerId.HasValue)
                query = query.Where(o => o.Id != excludeOwnerId.Value);

            var owners = query
                .Select(o => new
                {
                    o.FullName,
                    o.FatherOrSpouseName,
                    o.CitizenshipNumber
                })
                .ToList();

            if (isInstitution)
            {
                return owners.Any(o =>
                    IsInstitutionOwnerName(o.FullName) &&
                    NormalizeName(o.FullName) == normalizedName);
            }

            if (!string.IsNullOrWhiteSpace(normalizedCitizenship))
            {
                return owners.Any(o =>
                    !IsInstitutionOwnerName(o.FullName) &&
                    NormalizeCitizenship(o.CitizenshipNumber) == normalizedCitizenship);
            }

            return owners.Any(o =>
                !IsInstitutionOwnerName(o.FullName) &&
                NormalizeName(o.FullName) == normalizedName &&
                NormalizeName(o.FatherOrSpouseName ?? string.Empty) == normalizedFather);
        }

        public int CreateOwner(LegacyLandOwner owner)
        {
            var fullName = Normalize(owner.LandOwnersName) ?? "Unknown Owner";
            var isInstitution = IsInstitutionOwnerName(fullName);

            var entity = new CoreLandOwner
            {
                FullName = fullName,
                FatherOrSpouseName = isInstitution ? null : Normalize(owner.FatherSpouse),
                Gender = isInstitution ? null : Normalize(owner.Gender),
                CitizenshipNumber = isInstitution ? null : Normalize(owner.CitizenshipNumber),
                CitizenshipIssueDistrict = isInstitution ? null : Normalize(owner.CitizenshipIssuedDistrict),
                CitizenshipIssueDate = isInstitution ? null : Normalize(owner.CitizenshipIssuedDate),
                PermanentAddress = isInstitution ? null : Normalize(owner.PermanentAddress),
                TemporaryAddress = isInstitution ? null : Normalize(owner.TemporaryAddress),
                ContactNumber = isInstitution ? null : Normalize(owner.ContactNumber),
                Email = isInstitution ? null : Normalize(owner.EmailID),
                PhotoPath = Normalize(owner.PhotoPath),
                DocumentsFolderPath = Normalize(owner.DocumentsFolderPath),
                IdentificationMethod = isInstitution
                    ? "InstitutionName"
                    : string.IsNullOrWhiteSpace(owner.CitizenshipNumber)
                        ? "NameFatherFuzzy"
                        : "CitizenshipNumber",
                MatchConfidenceScore = 1.0,
                NeedsManualReview = false,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.LandOwners.Add(entity);
            _context.SaveChanges();
            MarkModified();
            return entity.Id;
        }

        public bool UpdateOwner(LegacyLandOwner owner)
        {
            var entity = _context.LandOwners.FirstOrDefault(o => o.Id == owner.LandOwnerId);
            if (entity == null)
                return false;

            var fullName = Normalize(owner.LandOwnersName) ?? entity.FullName;
            var isInstitution = IsInstitutionOwnerName(fullName);

            entity.FullName = fullName;
            entity.FatherOrSpouseName = isInstitution ? null : Normalize(owner.FatherSpouse);
            entity.Gender = isInstitution ? null : Normalize(owner.Gender);
            entity.CitizenshipNumber = isInstitution ? null : Normalize(owner.CitizenshipNumber);
            entity.CitizenshipIssueDistrict = isInstitution ? null : Normalize(owner.CitizenshipIssuedDistrict);
            entity.CitizenshipIssueDate = isInstitution ? null : Normalize(owner.CitizenshipIssuedDate);
            entity.PermanentAddress = isInstitution ? null : Normalize(owner.PermanentAddress);
            entity.TemporaryAddress = isInstitution ? null : Normalize(owner.TemporaryAddress);
            entity.ContactNumber = isInstitution ? null : Normalize(owner.ContactNumber);
            entity.Email = isInstitution ? null : Normalize(owner.EmailID);
            entity.IdentificationMethod = isInstitution
                ? "InstitutionName"
                : string.IsNullOrWhiteSpace(owner.CitizenshipNumber)
                    ? "NameFatherFuzzy"
                    : "CitizenshipNumber";
            entity.PhotoPath = Normalize(owner.PhotoPath);
            entity.DocumentsFolderPath = Normalize(owner.DocumentsFolderPath);
            entity.LastModifiedDate = DateTime.UtcNow;

            _context.SaveChanges();
            MarkModified();
            return true;
        }

        public bool UpdateOwnerPhotoPath(int ownerId, string relativePhotoPath)
        {
            var owner = _context.LandOwners.FirstOrDefault(o => o.Id == ownerId);
            if (owner == null)
                return false;

            owner.PhotoPath = Normalize(relativePhotoPath);
            owner.LastModifiedDate = DateTime.UtcNow;
            _context.SaveChanges();
            MarkModified();
            return true;
        }

        public bool UpdateOwnerDocumentsFolder(int ownerId, string relativeDocumentsPath)
        {
            var owner = _context.LandOwners.FirstOrDefault(o => o.Id == ownerId);
            if (owner == null)
                return false;

            owner.DocumentsFolderPath = Normalize(relativeDocumentsPath);
            owner.LastModifiedDate = DateTime.UtcNow;
            _context.SaveChanges();
            MarkModified();
            return true;
        }

        public bool DeleteOwnerWithParcels(int ownerId)
        {
            var owner = _context.LandOwners.FirstOrDefault(o => o.Id == ownerId);
            if (owner == null)
                return false;

            var parcelIds = _context.BaselineParcels
                .Where(p => p.LandOwnerId == ownerId)
                .Select(p => p.Id)
                .ToList();

            if (parcelIds.Count > 0)
            {
                var hasReplotMaps = _context.OriginalToReplottedMaps.Any(m => parcelIds.Contains(m.BaselineParcelId));
                if (hasReplotMaps)
                {
                    throw new InvalidOperationException(
                        "Cannot delete this owner because one or more parcels are linked to replotting records.");
                }

                _context.ParcelContributions.RemoveRange(
                    _context.ParcelContributions.Where(c => parcelIds.Contains(c.BaselineParcelId)));
                _context.ParcelContributionSummaries.RemoveRange(
                    _context.ParcelContributionSummaries.Where(s => parcelIds.Contains(s.BaselineParcelId)));
                _context.ParcelFrontages.RemoveRange(
                    _context.ParcelFrontages.Where(f => f.BaselineParcelId.HasValue && parcelIds.Contains(f.BaselineParcelId.Value)));
                _context.BaselineParcels.RemoveRange(
                    _context.BaselineParcels.Where(p => parcelIds.Contains(p.Id)));
            }

            _context.MalpotReferences.RemoveRange(
                _context.MalpotReferences.Where(m => m.LandOwnerId == ownerId));
            _context.LandOwners.Remove(owner);

            _context.SaveChanges();
            MarkModified();
            return true;
        }

        public List<string> GetUniqueMapSheets()
        {
            return _context.BaselineParcels
                .AsNoTracking()
                .Select(p => p.MapSheetNo)
                .Where(v => v != null && v.Trim() != string.Empty)
                .Distinct()
                .OrderBy(v => v)
                .ToList();
        }

        public bool ParcelExists(string? parcelNo, string? mapSheetNo, int? excludeParcelId = null)
        {
            if (string.IsNullOrWhiteSpace(parcelNo) || string.IsNullOrWhiteSpace(mapSheetNo))
                return false;

            var fullCode = BuildParcelCode(mapSheetNo, parcelNo);
            var query = _context.BaselineParcels
                .AsNoTracking()
                .Where(p => p.FullUniqueParcelCode == fullCode);

            if (excludeParcelId.HasValue)
                query = query.Where(p => p.Id != excludeParcelId.Value);

            return query.Any();
        }

        public int DeleteParcel(int parcelId)
        {
            var parcel = _context.BaselineParcels.FirstOrDefault(p => p.Id == parcelId);
            if (parcel == null)
                return 0;

            var hasReplotMap = _context.OriginalToReplottedMaps.Any(m => m.BaselineParcelId == parcelId);
            if (hasReplotMap)
            {
                throw new InvalidOperationException(
                    "Cannot delete this parcel because it is already linked to replotting records.");
            }

            _context.ParcelContributions.RemoveRange(
                _context.ParcelContributions.Where(c => c.BaselineParcelId == parcelId));
            _context.ParcelContributionSummaries.RemoveRange(
                _context.ParcelContributionSummaries.Where(s => s.BaselineParcelId == parcelId));
            _context.ParcelFrontages.RemoveRange(
                _context.ParcelFrontages.Where(f => f.BaselineParcelId == parcelId));
            _context.BaselineParcels.Remove(parcel);

            _context.SaveChanges();
            MarkModified();
            return 1;
        }

        public bool UpdateParcel(LegacyParcel parcel)
        {
            var entity = _context.BaselineParcels
                .Include(p => p.MalpotReference)
                .Include(p => p.CoOwners)
                .FirstOrDefault(p => p.Id == parcel.ParcelId);
            if (entity == null)
                return false;

            if (ParcelExists(parcel.ParcelNo, parcel.MapSheetNo, parcel.ParcelId))
                throw new InvalidOperationException("Another parcel with the same Parcel No and Map Sheet already exists.");

            var oldMalpotId = entity.MalpotReferenceId;
            entity.LandOwnerId = parcel.LandOwnerId;
            entity.ParcelNo = Normalize(parcel.ParcelNo) ?? string.Empty;
            entity.MapSheetNo = Normalize(parcel.MapSheetNo) ?? string.Empty;
            entity.FullUniqueParcelCode = BuildParcelCode(entity.MapSheetNo, entity.ParcelNo);
            entity.Province = Normalize(parcel.Province);
            entity.District = Normalize(parcel.District);
            entity.Municipality = Normalize(parcel.MunicipalityVillage);
            entity.WardNo = Normalize(parcel.WardNo);
            entity.OriginalAreaSqm = parcel.AreaInSqm ?? 0;
            entity.FieldMeasuredAreaSqm = parcel.FieldMeasuredAreaSqm;
            entity.LandUse = Normalize(parcel.LandUse);
            entity.LandOwnershipType = Normalize(parcel.LandOwnershipType);
            entity.TenantName = NormalizeTenantName(parcel.TenantName, parcel.IsTenant);
            entity.HasTenant = ParseTenant(parcel.IsTenant) || !string.IsNullOrWhiteSpace(entity.TenantName);
            entity.Remarks = Normalize(parcel.Remarks);
            entity.LastModifiedDate = DateTime.UtcNow;

            var malpot = ResolveOrCreateMalpot(entity.LandOwnerId, parcel.MothNo, parcel.PaanaNo);
            if (malpot == null)
            {
                entity.MalpotReferenceId = null;
                entity.MalpotReference = null;
            }
            else if (malpot.Id > 0)
            {
                entity.MalpotReferenceId = malpot.Id;
                entity.MalpotReference = null;
            }
            else
            {
                entity.MalpotReference = malpot;
                entity.MalpotReferenceId = null;
            }

            ReplaceParcelCoOwners(entity, parcel.JointCoOwners);

            _context.SaveChanges();
            CleanupOrphanMalpot(oldMalpotId);
            MarkModified();
            return true;
        }

        public Dictionary<int, int> SaveUniqueOwnersFromDeduplication(OwnerDeduplicationService.DeduplicationResult deduplicationResult)
        {
            var parcelToOwnerMap = new Dictionary<int, int>();
            if (deduplicationResult?.UniqueOwners == null || deduplicationResult.UniqueOwners.Count == 0)
                return parcelToOwnerMap;

            using var tx = _context.Database.BeginTransaction();
            try
            {
                var ownerCache = _context.LandOwners.ToList();
                foreach (var unique in deduplicationResult.UniqueOwners)
                {
                    var owner = new LegacyLandOwner
                    {
                        LandOwnersName = unique.LandOwnersName,
                        FatherSpouse = unique.FatherSpouse,
                        Gender = unique.Gender,
                        CitizenshipNumber = unique.CitizenshipNumber,
                        CitizenshipIssuedDistrict = unique.CitizenshipIssuedDistrict,
                        CitizenshipIssuedDate = unique.CitizenshipIssuedDate,
                        PermanentAddress = unique.PermanentAddress,
                        TemporaryAddress = unique.TemporaryAddress,
                        ContactNumber = unique.ContactNumber,
                        EmailID = unique.EmailID,
                        IsAnonymous = unique.IsAnonymous,
                        CreatedDate = DateTime.UtcNow
                    };

                    var ownerId = SaveOrGetOwnerId(owner, ownerCache);
                    foreach (var index in unique.ParcelIndices)
                    {
                        parcelToOwnerMap[index] = ownerId;
                    }
                }

                tx.Commit();
                if (parcelToOwnerMap.Count > 0)
                    MarkModified();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            return parcelToOwnerMap;
        }

        public int SaveParcelsWithDeduplication(List<BaselineLandParcelRecord> records, Dictionary<int, int> parcelToOwnerMap)
        {
            if (records == null || records.Count == 0 || parcelToOwnerMap.Count == 0)
                return 0;

            using var tx = _context.Database.BeginTransaction();
            try
            {
                var importSession = CreateManualImportSession("Saved from parcel record form.");
                var existingCodes = new HashSet<string>(
                    _context.BaselineParcels.Select(p => p.FullUniqueParcelCode).ToList(),
                    StringComparer.OrdinalIgnoreCase);

                var savedCount = 0;
                for (int i = 0; i < records.Count; i++)
                {
                    if (!parcelToOwnerMap.TryGetValue(i, out var ownerId))
                        continue;

                    var record = records[i];
                    var mapSheetNo = Normalize(record.MapSheetNo) ?? string.Empty;
                    var parcelNo = Normalize(record.ParcelNo) ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(mapSheetNo) || string.IsNullOrWhiteSpace(parcelNo))
                        continue;

                    var fullCode = BuildParcelCode(mapSheetNo, parcelNo);
                    if (existingCodes.Contains(fullCode))
                        continue;

                    var malpot = ResolveOrCreateMalpot(ownerId, record.MothNo, record.PaanaNo);
                    var parcel = new BaselineParcel
                    {
                        ImportSessionId = importSession.Id,
                        LandOwnerId = ownerId,
                        MapSheetNo = mapSheetNo,
                        ParcelNo = parcelNo,
                        FullUniqueParcelCode = fullCode,
                        Province = Normalize(record.Province),
                        District = Normalize(record.District),
                        Municipality = Normalize(record.MunicipalityVillage),
                        WardNo = Normalize(record.WardNo),
                        OriginalAreaSqm = record.AreaInSqm ?? 0,
                        FieldMeasuredAreaSqm = record.FieldMeasuredAreaSqm,
                        LandUse = Normalize(record.LandUse),
                        LandOwnershipType = Normalize(record.LandOwnershipType),
                        TenantName = NormalizeTenantName(record.TenantName, record.Tenant),
                        HasTenant = ParseTenant(record.Tenant) || !string.IsNullOrWhiteSpace(NormalizeTenantName(record.TenantName, record.Tenant)),
                        Remarks = Normalize(record.Remarks),
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    };

                    if (malpot != null)
                    {
                        if (malpot.Id > 0)
                            parcel.MalpotReferenceId = malpot.Id;
                        else
                            parcel.MalpotReference = malpot;
                    }

                    _context.BaselineParcels.Add(parcel);
                    SaveCoOwnersForParcel(parcel, record.JointCoOwners);
                    existingCodes.Add(fullCode);
                    savedCount++;
                }

                _context.SaveChanges();
                tx.Commit();

                if (savedCount > 0)
                    MarkModified();

                return savedCount;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private int SaveOrGetOwnerId(LegacyLandOwner owner, List<CoreLandOwner> ownerCache)
        {
            var normalizedName = Normalize(owner.LandOwnersName) ?? "Unknown Owner";
            var isInstitution = IsInstitutionOwnerName(normalizedName);
            var normalizedCitizenship = NormalizeCitizenship(owner.CitizenshipNumber);
            var normalizedFather = Normalize(owner.FatherSpouse) ?? string.Empty;
            CoreLandOwner? existing = null;

            if (isInstitution)
            {
                existing = ownerCache.FirstOrDefault(o =>
                    IsInstitutionOwnerName(o.FullName) &&
                    NormalizeName(o.FullName) == NormalizeName(normalizedName));
            }
            else if (!string.IsNullOrWhiteSpace(normalizedCitizenship))
            {
                existing = ownerCache.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.CitizenshipNumber) &&
                    NormalizeCitizenship(o.CitizenshipNumber) == normalizedCitizenship);
            }

            if (!isInstitution && existing == null)
            {
                existing = ownerCache.FirstOrDefault(o =>
                    NormalizeName(o.FullName) == NormalizeName(normalizedName) &&
                    NormalizeName(o.FatherOrSpouseName ?? string.Empty) == NormalizeName(normalizedFather));
            }

            if (existing != null)
            {
                var changed = false;
                if (isInstitution)
                {
                    changed |= ClearIfNotEmpty(existing.FatherOrSpouseName, v => existing.FatherOrSpouseName = v);
                    changed |= ClearIfNotEmpty(existing.Gender, v => existing.Gender = v);
                    changed |= ClearIfNotEmpty(existing.CitizenshipNumber, v => existing.CitizenshipNumber = v);
                    changed |= ClearIfNotEmpty(existing.CitizenshipIssueDistrict, v => existing.CitizenshipIssueDistrict = v);
                    changed |= ClearIfNotEmpty(existing.CitizenshipIssueDate, v => existing.CitizenshipIssueDate = v);
                    changed |= ClearIfNotEmpty(existing.PermanentAddress, v => existing.PermanentAddress = v);
                    changed |= ClearIfNotEmpty(existing.TemporaryAddress, v => existing.TemporaryAddress = v);
                    changed |= ClearIfNotEmpty(existing.ContactNumber, v => existing.ContactNumber = v);
                    changed |= ClearIfNotEmpty(existing.Email, v => existing.Email = v);
                    changed |= UpdateIfDifferent(existing.IdentificationMethod, "InstitutionName", v => existing.IdentificationMethod = v);
                }
                else
                {
                    changed |= MergeIfEmpty(existing.FatherOrSpouseName, Normalize(owner.FatherSpouse), v => existing.FatherOrSpouseName = v);
                    changed |= MergeIfEmpty(existing.Gender, Normalize(owner.Gender), v => existing.Gender = v);
                    changed |= MergeIfEmpty(existing.CitizenshipNumber, Normalize(owner.CitizenshipNumber), v => existing.CitizenshipNumber = v);
                    changed |= MergeIfEmpty(existing.CitizenshipIssueDistrict, Normalize(owner.CitizenshipIssuedDistrict), v => existing.CitizenshipIssueDistrict = v);
                    changed |= MergeIfEmpty(existing.CitizenshipIssueDate, Normalize(owner.CitizenshipIssuedDate), v => existing.CitizenshipIssueDate = v);
                    changed |= MergeIfEmpty(existing.PermanentAddress, Normalize(owner.PermanentAddress), v => existing.PermanentAddress = v);
                    changed |= MergeIfEmpty(existing.TemporaryAddress, Normalize(owner.TemporaryAddress), v => existing.TemporaryAddress = v);
                    changed |= MergeIfEmpty(existing.ContactNumber, Normalize(owner.ContactNumber), v => existing.ContactNumber = v);
                    changed |= MergeIfEmpty(existing.Email, Normalize(owner.EmailID), v => existing.Email = v);
                }

                if (changed)
                {
                    existing.LastModifiedDate = DateTime.UtcNow;
                    _context.SaveChanges();
                }

                return existing.Id;
            }

            if (isInstitution)
            {
                owner.FatherSpouse = null;
                owner.Gender = null;
                owner.CitizenshipNumber = null;
                owner.CitizenshipIssuedDistrict = null;
                owner.CitizenshipIssuedDate = null;
                owner.PermanentAddress = null;
                owner.TemporaryAddress = null;
                owner.ContactNumber = null;
                owner.EmailID = null;
            }

            var createdId = CreateOwner(owner);
            var created = _context.LandOwners.FirstOrDefault(o => o.Id == createdId);
            if (created != null)
            {
                ownerCache.Add(created);
            }

            return createdId;
        }

        private void ReplaceParcelCoOwners(BaselineParcel parcel, IEnumerable<CoOwnerRecord>? coOwners)
        {
            var targetCoOwners = BuildCoOwnerEntries(parcel, coOwners).ToList();
            var targetOwnerIds = targetCoOwners.Select(c => c.OwnerId).ToHashSet();

            foreach (var existing in parcel.CoOwners.ToList())
            {
                if (!targetOwnerIds.Contains(existing.LandOwnerId))
                {
                    _context.BaselineParcelCoOwners.Remove(existing);
                    parcel.CoOwners.Remove(existing);
                    continue;
                }

                var target = targetCoOwners.First(c => c.OwnerId == existing.LandOwnerId);
                existing.OwnershipSharePercent = target.SharePercent;
            }

            var existingOwnerIds = parcel.CoOwners.Select(c => c.LandOwnerId).ToHashSet();
            foreach (var target in targetCoOwners.Where(c => !existingOwnerIds.Contains(c.OwnerId)))
            {
                parcel.CoOwners.Add(new BaselineParcelCoOwner
                {
                    BaselineParcel = parcel,
                    BaselineParcelId = parcel.Id,
                    LandOwnerId = target.OwnerId,
                    OwnershipSharePercent = target.SharePercent,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        private void SaveCoOwnersForParcel(BaselineParcel parcel, IEnumerable<CoOwnerRecord>? coOwners)
        {
            foreach (var target in BuildCoOwnerEntries(parcel, coOwners))
            {
                parcel.CoOwners.Add(new BaselineParcelCoOwner
                {
                    BaselineParcel = parcel,
                    BaselineParcelId = parcel.Id,
                    LandOwnerId = target.OwnerId,
                    OwnershipSharePercent = target.SharePercent,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        private IEnumerable<(int OwnerId, double? SharePercent)> BuildCoOwnerEntries(
            BaselineParcel parcel,
            IEnumerable<CoOwnerRecord>? coOwners)
        {
            if (coOwners == null)
            {
                yield break;
            }

            var ownerCache = _context.LandOwners.ToList();
            var addedOwnerIds = new HashSet<int>();

            foreach (var coOwner in coOwners)
            {
                if (string.IsNullOrWhiteSpace(coOwner.OwnerName))
                {
                    continue;
                }

                var owner = new LegacyLandOwner
                {
                    LandOwnersName = coOwner.OwnerName.Trim(),
                    FatherSpouse = coOwner.FatherSpouse,
                    Gender = coOwner.Gender,
                    CitizenshipNumber = coOwner.CitizenshipNumber,
                    CitizenshipIssuedDistrict = coOwner.CitizenshipIssuedDistrict,
                    CitizenshipIssuedDate = coOwner.CitizenshipIssuedDate,
                    PermanentAddress = coOwner.PermanentAddress,
                    TemporaryAddress = coOwner.TemporaryAddress,
                    ContactNumber = coOwner.ContactNumber,
                    EmailID = coOwner.EmailID,
                    CreatedDate = DateTime.UtcNow
                };

                var ownerId = SaveOrGetOwnerId(owner, ownerCache);
                if (ownerId == parcel.LandOwnerId || !addedOwnerIds.Add(ownerId))
                {
                    continue;
                }

                yield return (ownerId, coOwner.OwnershipSharePercent);
            }
        }

        private ImportSession CreateManualImportSession(string notes)
        {
            var session = new ImportSession
            {
                SourceFileName = "Manual Entry",
                SourceFilePath = null,
                ImportDate = DateTime.UtcNow,
                TotalRowsInFile = 0,
                TotalRowsImported = 0,
                TotalRowsInvalid = 0,
                Notes = notes
            };

            _context.ImportSessions.Add(session);
            _context.SaveChanges();
            return session;
        }

        private MalpotReference? ResolveOrCreateMalpot(int ownerId, string? mothNo, string? paanaNo)
        {
            var normalizedMoth = Normalize(mothNo);
            if (string.IsNullOrWhiteSpace(normalizedMoth))
                return null;

            var normalizedMothLower = normalizedMoth.ToLowerInvariant();
            var existing = _context.MalpotReferences.FirstOrDefault(m =>
                m.LandOwnerId == ownerId &&
                m.MothNo != null &&
                m.MothNo.ToLower() == normalizedMothLower);

            if (existing != null)
            {
                var normalizedPaana = Normalize(paanaNo);
                if (!string.IsNullOrWhiteSpace(normalizedPaana) &&
                    !string.Equals(existing.PaanaNo, normalizedPaana, StringComparison.OrdinalIgnoreCase))
                {
                    existing.PaanaNo = normalizedPaana;
                }

                return existing;
            }

            var created = new MalpotReference
            {
                LandOwnerId = ownerId,
                MothNo = normalizedMoth,
                PaanaNo = Normalize(paanaNo) ?? string.Empty
            };

            _context.MalpotReferences.Add(created);
            return created;
        }

        private void CleanupOrphanMalpot(int? oldMalpotReferenceId)
        {
            if (!oldMalpotReferenceId.HasValue)
                return;

            var stillUsed = _context.BaselineParcels.Any(p => p.MalpotReferenceId == oldMalpotReferenceId.Value);
            if (stillUsed)
                return;

            var old = _context.MalpotReferences.FirstOrDefault(m => m.Id == oldMalpotReferenceId.Value);
            if (old == null)
                return;

            _context.MalpotReferences.Remove(old);
            _context.SaveChanges();
        }

        private static string BuildParcelCode(string? mapSheetNo, string? parcelNo)
        {
            var map = Normalize(mapSheetNo) ?? string.Empty;
            var parcel = Normalize(parcelNo) ?? string.Empty;
            return $"{map.ToUpperInvariant()}::{parcel.ToUpperInvariant()}";
        }

        private static bool ParseTenant(string? tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant))
                return false;

            var normalized = tenant.Trim().ToLowerInvariant();
            return normalized is not ("no" or "n" or "false" or "0" or "none" or "छैन");
        }

        private static string? NormalizeTenantName(string? tenantName, string? tenant)
        {
            var value = Normalize(tenantName);
            if (!string.IsNullOrWhiteSpace(value))
                return value;

            value = Normalize(tenant);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var normalized = value.ToLowerInvariant();
            return normalized is "yes" or "y" or "true" or "1" or "mohi" or "no" or "n" or "false" or "0" or "none" or "छैन"
                ? null
                : value;
        }

        private static bool MergeIfEmpty(string? target, string? source, Action<string?> assign)
        {
            if (string.IsNullOrWhiteSpace(target) && !string.IsNullOrWhiteSpace(source))
            {
                assign(source);
                return true;
            }

            return false;
        }

        private static bool ClearIfNotEmpty(string? target, Action<string?> assign)
        {
            if (!string.IsNullOrWhiteSpace(target))
            {
                assign(null);
                return true;
            }

            return false;
        }

        private static bool UpdateIfDifferent(string? current, string next, Action<string> assign)
        {
            if (!string.Equals(current, next, StringComparison.Ordinal))
            {
                assign(next);
                return true;
            }

            return false;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string NormalizeName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return OwnerDeduplicationService.NormalizeString(value);
        }

        private static bool IsInstitutionOwnerName(string? ownerName)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
                return false;

            var normalized = NormalizeName(ownerName);
            return NepalDomainConstants.InstitutionKeywords.Any(keyword =>
                normalized.Contains(NormalizeName(keyword), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeCitizenship(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var converted = new System.Text.StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (c >= '\u0966' && c <= '\u096F')
                    converted.Append((char)('0' + (c - '\u0966')));
                else
                    converted.Append(c);
            }

            return new string(converted.ToString().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }

        private LegacyParcel MapParcel(BaselineParcel parcel, int sqmPrecision = 3, int traditionalPrecision = 2)
        {
            var areaSqm = parcel.OriginalAreaSqm;
            var owner = MapOwner(parcel.LandOwner);
            return new LegacyParcel
            {
                ParcelId = parcel.Id,
                LandOwnerId = parcel.LandOwnerId,
                ParcelNo = parcel.ParcelNo,
                Province = parcel.Province,
                District = parcel.District,
                MunicipalityVillage = parcel.Municipality,
                WardNo = parcel.WardNo,
                ParcelLocation = null,
                MapSheetNo = parcel.MapSheetNo,
                IsTenant = parcel.HasTenant ? "Yes" : "No",
                TenantName = parcel.TenantName,
                LandUse = parcel.LandUse,
                LandOwnershipType = parcel.LandOwnershipType,
                AreaInSqm = areaSqm,
                FieldMeasuredAreaSqm = parcel.FieldMeasuredAreaSqm,
                AreaInRAPD = AreaConverterService.SqmToRAPDString(areaSqm, traditionalPrecision),
                AreaInBKD = AreaConverterService.SqmToBKDString(areaSqm, traditionalPrecision),
                MothNo = parcel.MalpotReference?.MothNo,
                PaanaNo = parcel.MalpotReference?.PaanaNo,
                Remarks = parcel.Remarks,
                ImportedDate = parcel.CreatedDate,
                ModifiedDate = parcel.LastModifiedDate,
                IsValid = true,
                ValidationErrors = null,
                JointCoOwners = parcel.CoOwners
                    .OrderBy(c => c.Id)
                    .Select(MapCoOwner)
                    .ToList(),
                Owner = owner
            };
        }

        private static CoOwnerRecord MapCoOwner(BaselineParcelCoOwner coOwner)
        {
            return new CoOwnerRecord
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
            };
        }

        private static LegacyLandOwner MapOwner(CoreLandOwner owner)
        {
            var model = new LegacyLandOwner
            {
                LandOwnerId = owner.Id,
                LandOwnersName = owner.FullName,
                FatherSpouse = owner.FatherOrSpouseName,
                Gender = owner.Gender,
                CitizenshipNumber = owner.CitizenshipNumber,
                CitizenshipIssuedDistrict = owner.CitizenshipIssueDistrict,
                CitizenshipIssuedDate = owner.CitizenshipIssueDate,
                PermanentAddress = owner.PermanentAddress,
                TemporaryAddress = owner.TemporaryAddress,
                ContactNumber = owner.ContactNumber,
                EmailID = owner.Email,
                PhotoPath = owner.PhotoPath,
                DocumentsFolderPath = owner.DocumentsFolderPath,
                IsAnonymous = false,
                CreatedDate = owner.CreatedDate,
                ModifiedDate = owner.LastModifiedDate
            };

            if (IsInstitutionOwnerName(model.LandOwnersName))
            {
                model.FatherSpouse = null;
                model.Gender = null;
                model.CitizenshipNumber = null;
                model.CitizenshipIssuedDistrict = null;
                model.CitizenshipIssuedDate = null;
                model.PermanentAddress = null;
                model.TemporaryAddress = null;
                model.ContactNumber = null;
                model.EmailID = null;
            }

            return model;
        }

        private static void MarkModified()
        {
            if (AppServices.HasContext)
                AppServices.Context.MarkAsModified();
        }
    }
}
