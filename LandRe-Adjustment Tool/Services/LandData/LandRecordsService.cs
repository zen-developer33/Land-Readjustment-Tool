using Land_Readjustment_Tool.Core.Entities.Import;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Entities.Replotting;
using Land_Readjustment_Tool.Data;
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

        public List<LegacyLandOwner> GetAllOwners()
        {
            return _context.LandOwners
                .AsNoTracking()
                .OrderBy(o => o.FullName)
                .Select(MapOwner)
                .ToList();
        }

        public LegacyLandOwner? GetOwnerById(int ownerId)
        {
            var owner = _context.LandOwners
                .AsNoTracking()
                .FirstOrDefault(o => o.Id == ownerId);
            return owner == null ? null : MapOwner(owner);
        }

        public List<LegacyParcel> GetAllParcelsWithOwners()
        {
            var parcels = _context.BaselineParcels
                .AsNoTracking()
                .Include(p => p.LandOwner)
                .Include(p => p.MalpotReference)
                .OrderBy(p => p.MapSheetNo)
                .ThenBy(p => p.ParcelNo)
                .ToList();

            return parcels.Select(MapParcel).ToList();
        }

        public List<LegacyParcel> GetParcelsByOwnerId(int ownerId)
        {
            var parcels = _context.BaselineParcels
                .AsNoTracking()
                .Include(p => p.LandOwner)
                .Include(p => p.MalpotReference)
                .Where(p => p.LandOwnerId == ownerId)
                .OrderBy(p => p.MapSheetNo)
                .ThenBy(p => p.ParcelNo)
                .ToList();

            return parcels.Select(MapParcel).ToList();
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
            var docsFolder = Path.Combine(projectDir, owner.DocumentsFolderPath);
            return Directory.Exists(docsFolder) ? Directory.GetFiles(docsFolder).Length : 0;
        }

        public bool OwnerExists(string? name, string? fatherSpouse, string? citizenshipNumber, int? excludeOwnerId = null)
        {
            var normalizedName = Normalize(name);
            if (string.IsNullOrWhiteSpace(normalizedName))
                return false;

            var normalizedFather = Normalize(fatherSpouse) ?? string.Empty;
            var normalizedCitizenship = Normalize(citizenshipNumber) ?? string.Empty;

            var query = _context.LandOwners.AsNoTracking().AsQueryable();
            if (excludeOwnerId.HasValue)
                query = query.Where(o => o.Id != excludeOwnerId.Value);

            return query.Any(o =>
                o.FullName.Trim().ToLower() == normalizedName.ToLower() &&
                (o.FatherOrSpouseName ?? string.Empty).Trim().ToLower() == normalizedFather.ToLower() &&
                (o.CitizenshipNumber ?? string.Empty).Trim().ToLower() == normalizedCitizenship.ToLower());
        }

        public int CreateOwner(LegacyLandOwner owner)
        {
            var entity = new CoreLandOwner
            {
                FullName = Normalize(owner.LandOwnersName) ?? "Unknown Owner",
                FatherOrSpouseName = Normalize(owner.FatherSpouse),
                Gender = Normalize(owner.Gender),
                CitizenshipNumber = Normalize(owner.CitizenshipNumber),
                CitizenshipIssueDistrict = Normalize(owner.CitizenshipIssuedDistrict),
                CitizenshipIssueDate = Normalize(owner.CitizenshipIssuedDate),
                PermanentAddress = Normalize(owner.PermanentAddress),
                TemporaryAddress = Normalize(owner.TemporaryAddress),
                ContactNumber = Normalize(owner.ContactNumber),
                Email = Normalize(owner.EmailID),
                PhotoPath = Normalize(owner.PhotoPath),
                DocumentsFolderPath = Normalize(owner.DocumentsFolderPath),
                IdentificationMethod = string.IsNullOrWhiteSpace(owner.CitizenshipNumber) ? "NameFatherFuzzy" : "CitizenshipNumber",
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

            entity.FullName = Normalize(owner.LandOwnersName) ?? entity.FullName;
            entity.FatherOrSpouseName = Normalize(owner.FatherSpouse);
            entity.Gender = Normalize(owner.Gender);
            entity.CitizenshipNumber = Normalize(owner.CitizenshipNumber);
            entity.CitizenshipIssueDistrict = Normalize(owner.CitizenshipIssuedDistrict);
            entity.CitizenshipIssueDate = Normalize(owner.CitizenshipIssuedDate);
            entity.PermanentAddress = Normalize(owner.PermanentAddress);
            entity.TemporaryAddress = Normalize(owner.TemporaryAddress);
            entity.ContactNumber = Normalize(owner.ContactNumber);
            entity.Email = Normalize(owner.EmailID);
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
            entity.LandUse = Normalize(parcel.LandUse);
            entity.HasTenant = ParseTenant(parcel.IsTenant);
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
                        CreatedDate = DateTime.Now
                    };

                    var ownerId = SaveOrGetOwnerId(owner);
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

        public int SaveParcelsWithDeduplication(List<BaselineLandParceRecord> records, Dictionary<int, int> parcelToOwnerMap)
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
                        LandUse = Normalize(record.LandUse),
                        HasTenant = ParseTenant(record.Tenant),
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

        private int SaveOrGetOwnerId(LegacyLandOwner owner)
        {
            var normalizedCitizenship = Normalize(owner.CitizenshipNumber);
            CoreLandOwner? existing = null;

            if (!string.IsNullOrWhiteSpace(normalizedCitizenship))
            {
                existing = _context.LandOwners.FirstOrDefault(o =>
                    o.CitizenshipNumber != null &&
                    o.CitizenshipNumber.ToLower() == normalizedCitizenship.ToLower());
            }

            if (existing == null)
            {
                var normalizedName = Normalize(owner.LandOwnersName);
                var normalizedFather = Normalize(owner.FatherSpouse) ?? string.Empty;

                existing = _context.LandOwners.FirstOrDefault(o =>
                    o.FullName.ToLower() == (normalizedName ?? string.Empty).ToLower() &&
                    (o.FatherOrSpouseName ?? string.Empty).ToLower() == normalizedFather.ToLower());
            }

            if (existing != null)
            {
                var changed = false;
                changed |= MergeIfEmpty(existing.FatherOrSpouseName, Normalize(owner.FatherSpouse), v => existing.FatherOrSpouseName = v);
                changed |= MergeIfEmpty(existing.Gender, Normalize(owner.Gender), v => existing.Gender = v);
                changed |= MergeIfEmpty(existing.CitizenshipNumber, normalizedCitizenship, v => existing.CitizenshipNumber = v);
                changed |= MergeIfEmpty(existing.CitizenshipIssueDistrict, Normalize(owner.CitizenshipIssuedDistrict), v => existing.CitizenshipIssueDistrict = v);
                changed |= MergeIfEmpty(existing.CitizenshipIssueDate, Normalize(owner.CitizenshipIssuedDate), v => existing.CitizenshipIssueDate = v);
                changed |= MergeIfEmpty(existing.PermanentAddress, Normalize(owner.PermanentAddress), v => existing.PermanentAddress = v);
                changed |= MergeIfEmpty(existing.TemporaryAddress, Normalize(owner.TemporaryAddress), v => existing.TemporaryAddress = v);
                changed |= MergeIfEmpty(existing.ContactNumber, Normalize(owner.ContactNumber), v => existing.ContactNumber = v);
                changed |= MergeIfEmpty(existing.Email, Normalize(owner.EmailID), v => existing.Email = v);

                if (changed)
                {
                    existing.LastModifiedDate = DateTime.UtcNow;
                    _context.SaveChanges();
                }

                return existing.Id;
            }

            return CreateOwner(owner);
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

            var existing = _context.MalpotReferences.FirstOrDefault(m =>
                m.LandOwnerId == ownerId &&
                m.MothNo.ToLower() == normalizedMoth.ToLower());

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
            return normalized is "yes" or "y" or "true" or "1" or "mohi";
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

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private LegacyParcel MapParcel(BaselineParcel parcel)
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
                LandUse = parcel.LandUse,
                LandOwnershipType = null,
                AreaInSqm = areaSqm,
                AreaInRAPD = AreaConverterService.SqmToRAPDString(areaSqm),
                AreaInBKD = AreaConverterService.SqmToBKDString(areaSqm),
                MothNo = parcel.MalpotReference?.MothNo,
                PaanaNo = parcel.MalpotReference?.PaanaNo,
                Remarks = parcel.Remarks,
                ImportedDate = parcel.CreatedDate,
                ModifiedDate = parcel.LastModifiedDate,
                IsValid = true,
                ValidationErrors = null,
                Owner = owner
            };
        }

        private static LegacyLandOwner MapOwner(CoreLandOwner owner)
        {
            return new LegacyLandOwner
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
        }

        private static void MarkModified()
        {
            if (AppServices.HasContext)
                AppServices.Context.MarkAsModified();
        }
    }
}
