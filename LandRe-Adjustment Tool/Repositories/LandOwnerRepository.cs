using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using System.Data.SQLite;

namespace Land_Readjustment_Tool.Repositories
{
    /// <summary>
    /// Repository for managing landowners and their parcels
    /// Handles extraction of unique owners and database operations
    /// </summary>
    public class LandOwnerRepository
    {
        private readonly SQLiteConnection _connection;

        public LandOwnerRepository(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Saves unique owners from deduplication result and returns dictionary mapping parcel indices to LandOwnerId
        /// This is the preferred method when deduplication has been performed
        /// </summary>
        public Dictionary<int, int> SaveUniqueOwnersFromDeduplication(OwnerDeduplicationService.DeduplicationResult deduplicationResult)
        {
            var parcelToOwnerMap = new Dictionary<int, int>(); // Maps parcel index to LandOwnerId
            var hasNewOwners = false;

            using var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var uniqueOwner in deduplicationResult.UniqueOwners)
                {
                    // Create LandOwner from UniqueOwner
                    var owner = new LandOwner
                    {
                        LandOwnersName = uniqueOwner.LandOwnersName,
                        FatherSpouse = uniqueOwner.FatherSpouse,
                        Gender = uniqueOwner.Gender,
                        CitizenshipNumber = uniqueOwner.CitizenshipNumber,
                        PermanentAddress = uniqueOwner.PermanentAddress,
                        IsAnonymous = uniqueOwner.IsAnonymous,
                        CreatedDate = DateTime.Now
                    };

                    // Save owner and get ID
                    var ownerResult = SaveOrGetOwnerIdWithTransaction(owner, transaction);
                    int ownerId = ownerResult.OwnerId;
                    hasNewOwners |= ownerResult.WasInserted;

                    // Map all parcel indices belonging to this owner to the same LandOwnerId
                    foreach (int parcelIndex in uniqueOwner.ParcelIndices)
                    {
                        parcelToOwnerMap[parcelIndex] = ownerId;
                    }
                }

                transaction.Commit();
                if (hasNewOwners)
                {
                    CurrentProject.MarkAsModified();
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return parcelToOwnerMap;
        }

        /// <summary>
        /// Saves parcels using the parcel-to-owner map from deduplication
        /// </summary>
        public int SaveParcelsWithDeduplication(
            List<BaselineLandParceRecord> records,
            Dictionary<int, int> parcelToOwnerMap)
        {
            int savedCount = 0;
            int skippedCount = 0;

            using var transaction = _connection.BeginTransaction();
            try
            {
                for (int i = 0; i < records.Count; i++)
                {
                    var record = records[i];

                    // Get owner ID from the map
                    if (!parcelToOwnerMap.TryGetValue(i, out int ownerId))
                    {
                        skippedCount++;
                        continue; // Skip records without mapped owners
                    }

                    // Check if parcel already exists (avoid duplicates)
                    if (ParcelExists(record.ParcelNo, record.MapSheetNo, transaction))
                    {
                        skippedCount++;
                        continue;
                    }

                    string sql = @"
                        INSERT INTO tblOriginalLandParcels (
                            LandOwnerId, ParcelNo, Province, District, MunicipalityVillage,
                            WardNo, ParcelLocation, MapSheetNo, Tenant, LandUse, LandOwnershipType,
                            AreaInSqm, AreaInRAPD, AreaInBKD,
                            MothNo, PaanaNo, Remarks, IsValid
                        ) VALUES (
                            @LandOwnerId, @ParcelNo, @Province, @District, @MunicipalityVillage,
                            @WardNo, @ParcelLocation, @MapSheetNo, @Tenant, @LandUse, @LandOwnershipType,
                            @AreaInSqm, @AreaInRAPD, @AreaInBKD,
                            @MothNo, @PaanaNo, @Remarks, @IsValid
                        )";

                    using var cmd = new SQLiteCommand(sql, _connection, transaction);
                    _ = cmd.Parameters.AddWithValue("@LandOwnerId", ownerId);
                    _ = cmd.Parameters.AddWithValue("@ParcelNo", record.ParcelNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@Province", record.Province ?? "");
                    _ = cmd.Parameters.AddWithValue("@District", record.District ?? "");
                    _ = cmd.Parameters.AddWithValue("@MunicipalityVillage", record.MunicipalityVillage ?? "");
                    _ = cmd.Parameters.AddWithValue("@WardNo", record.WardNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@ParcelLocation", record.ParcelLocation ?? "");
                    _ = cmd.Parameters.AddWithValue("@MapSheetNo", record.MapSheetNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@Tenant", record.Tenant ?? "");
                    _ = cmd.Parameters.AddWithValue("@LandUse", record.LandUse ?? "");
                    _ = cmd.Parameters.AddWithValue("@LandOwnershipType", record.LandOwnershipType ?? "");
                    _ = cmd.Parameters.AddWithValue("@AreaInSqm", record.AreaInSqm ?? (object)DBNull.Value);
                    _ = cmd.Parameters.AddWithValue("@AreaInRAPD", record.AreaInRAPD ?? "");
                    _ = cmd.Parameters.AddWithValue("@AreaInBKD", record.AreaInBKD ?? "");
                    _ = cmd.Parameters.AddWithValue("@MothNo", record.MothNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@PaanaNo", record.PaanaNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@Remarks", record.Remarks ?? "");
                    _ = cmd.Parameters.AddWithValue("@IsValid", 1);

                    _ = cmd.ExecuteNonQuery();
                    savedCount++;
                }

                transaction.Commit();
                if (savedCount > 0)
                {
                    CurrentProject.MarkAsModified();
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return savedCount;
        }

        /// <summary>
        /// Checks if a parcel already exists in the database
        /// </summary>
        private bool ParcelExists(string? parcelNo, string? mapSheetNo, SQLiteTransaction transaction)
        {
            if (string.IsNullOrEmpty(parcelNo) || string.IsNullOrEmpty(mapSheetNo))
                return false;

            string sql = "SELECT COUNT(*) FROM tblOriginalLandParcels WHERE ParcelNo = @ParcelNo AND MapSheetNo = @MapSheetNo";
            using var cmd = new SQLiteCommand(sql, _connection, transaction);
            _ = cmd.Parameters.AddWithValue("@ParcelNo", parcelNo);
            _ = cmd.Parameters.AddWithValue("@MapSheetNo", mapSheetNo);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public int GetNextAnonymousOwnerNumber()
        {
            const string sql = @"
                SELECT LandOwnersName
                FROM tblLandOwner
                WHERE IsAnonymous = 1
                   OR LOWER(LandOwnersName) LIKE 'anonymous owner %'";

            int maxNumber = 0;
            using var cmd = new SQLiteCommand(sql, _connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader["LandOwnersName"]?.ToString();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;

                if (int.TryParse(parts[^1], out int number))
                {
                    maxNumber = Math.Max(maxNumber, number);
                }
            }

            return maxNumber + 1;
        }

        /// <summary>
        /// Checks if an owner with the same name, father/spouse, and citizenship number already exists
        /// </summary>
        /// <param name="name">Owner's name</param>
        /// <param name="fatherSpouse">Father/spouse name</param>
        /// <param name="citizenshipNumber">Citizenship number</param>
        /// <param name="excludeOwnerId">Owner ID to exclude from check (for edit mode)</param>
        /// <returns>True if a duplicate exists</returns>
        public bool OwnerExists(string? name, string? fatherSpouse, string? citizenshipNumber, int? excludeOwnerId = null)
        {
            string sql = @"
                SELECT COUNT(*) FROM tblLandOwner 
                WHERE LOWER(TRIM(LandOwnersName)) = LOWER(TRIM(@LandOwnersName))
                AND LOWER(TRIM(COALESCE(FatherSpouse, ''))) = LOWER(TRIM(@FatherSpouse))
                AND LOWER(TRIM(COALESCE(CitizenshipNumber, ''))) = LOWER(TRIM(@CitizenshipNumber))";

            if (excludeOwnerId.HasValue)
            {
                sql += " AND LandOwnerId != @ExcludeOwnerId";
            }

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@LandOwnersName", name?.Trim() ?? "");
            _ = cmd.Parameters.AddWithValue("@FatherSpouse", fatherSpouse?.Trim() ?? "");
            _ = cmd.Parameters.AddWithValue("@CitizenshipNumber", citizenshipNumber?.Trim() ?? "");
            
            if (excludeOwnerId.HasValue)
            {
                _ = cmd.Parameters.AddWithValue("@ExcludeOwnerId", excludeOwnerId.Value);
            }

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public int CreateOwner(LandOwner owner)
        {
            const string insertSql = @"
                INSERT INTO tblLandOwner (
                    LandOwnersName, FatherSpouse, Gender, CitizenshipNumber,
                    CitizenshipIssuedDistrict, CitizenshipIssuedDate,
                    PermanentAddress, TemporaryAddress, ContactNumber, EmailID,
                    IsAnonymous, CreatedDate
                ) VALUES (
                    @LandOwnersName, @FatherSpouse, @Gender, @CitizenshipNumber,
                    @CitizenshipIssuedDistrict, @CitizenshipIssuedDate,
                    @PermanentAddress, @TemporaryAddress, @ContactNumber, @EmailID,
                    @IsAnonymous, @CreatedDate
                );
                SELECT last_insert_rowid();";

            using var cmd = new SQLiteCommand(insertSql, _connection);
            _ = cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
            _ = cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
            _ = cmd.Parameters.AddWithValue("@Gender", owner.Gender ?? "");
            _ = cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");
            _ = cmd.Parameters.AddWithValue("@CitizenshipIssuedDistrict", owner.CitizenshipIssuedDistrict ?? "");
            _ = cmd.Parameters.AddWithValue("@CitizenshipIssuedDate", owner.CitizenshipIssuedDate ?? "");
            _ = cmd.Parameters.AddWithValue("@PermanentAddress", owner.PermanentAddress ?? "");
            _ = cmd.Parameters.AddWithValue("@TemporaryAddress", owner.TemporaryAddress ?? "");
            _ = cmd.Parameters.AddWithValue("@ContactNumber", owner.ContactNumber ?? "");
            _ = cmd.Parameters.AddWithValue("@EmailID", owner.EmailID ?? "");
            _ = cmd.Parameters.AddWithValue("@IsAnonymous", owner.IsAnonymous ? 1 : 0);
            _ = cmd.Parameters.AddWithValue("@CreatedDate", owner.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));

            int ownerId = Convert.ToInt32(cmd.ExecuteScalar());
            CurrentProject.MarkAsModified();
            return ownerId;
        }

        public int InsertParcel(OriginalLandParcel parcel)
        {
            const string sql = @"
                INSERT INTO tblOriginalLandParcels (
                    LandOwnerId, ParcelNo, Province, District, MunicipalityVillage,
                    WardNo, ParcelLocation, MapSheetNo, Tenant, LandUse, LandOwnershipType,
                    AreaInSqm, AreaInRAPD, AreaInBKD,
                    MothNo, PaanaNo, Remarks, IsValid
                ) VALUES (
                    @LandOwnerId, @ParcelNo, @Province, @District, @MunicipalityVillage,
                    @WardNo, @ParcelLocation, @MapSheetNo, @Tenant, @LandUse, @LandOwnershipType,
                    @AreaInSqm, @AreaInRAPD, @AreaInBKD,
                    @MothNo, @PaanaNo, @Remarks, @IsValid
                );
                SELECT last_insert_rowid();";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@LandOwnerId", parcel.LandOwnerId);
            _ = cmd.Parameters.AddWithValue("@ParcelNo", parcel.ParcelNo ?? "");
            _ = cmd.Parameters.AddWithValue("@Province", parcel.Province ?? "");
            _ = cmd.Parameters.AddWithValue("@District", parcel.District ?? "");
            _ = cmd.Parameters.AddWithValue("@MunicipalityVillage", parcel.MunicipalityVillage ?? "");
            _ = cmd.Parameters.AddWithValue("@WardNo", parcel.WardNo ?? "");
            _ = cmd.Parameters.AddWithValue("@ParcelLocation", parcel.ParcelLocation ?? "");
            _ = cmd.Parameters.AddWithValue("@MapSheetNo", parcel.MapSheetNo ?? "");
            _ = cmd.Parameters.AddWithValue("@Tenant", parcel.IsTenant ?? "");
            _ = cmd.Parameters.AddWithValue("@LandUse", parcel.LandUse ?? "");
            _ = cmd.Parameters.AddWithValue("@LandOwnershipType", parcel.LandOwnershipType ?? "");
            _ = cmd.Parameters.AddWithValue("@AreaInSqm", parcel.AreaInSqm ?? (object)DBNull.Value);
            _ = cmd.Parameters.AddWithValue("@AreaInRAPD", parcel.AreaInRAPD ?? "");
            _ = cmd.Parameters.AddWithValue("@AreaInBKD", parcel.AreaInBKD ?? "");
            _ = cmd.Parameters.AddWithValue("@MothNo", parcel.MothNo ?? "");
            _ = cmd.Parameters.AddWithValue("@PaanaNo", parcel.PaanaNo ?? "");
            _ = cmd.Parameters.AddWithValue("@Remarks", parcel.Remarks ?? "");
            _ = cmd.Parameters.AddWithValue("@IsValid", parcel.IsValid ? 1 : 0);

            int parcelId = Convert.ToInt32(cmd.ExecuteScalar());
            CurrentProject.MarkAsModified();
            return parcelId;
        }

        public bool UpdateParcelOwnerId(int parcelId, int landOwnerId)
        {
            const string sql = @"
                UPDATE tblOriginalLandParcels
                SET LandOwnerId = @LandOwnerId, ModifiedDate = @ModifiedDate
                WHERE ParcelId = @ParcelId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@ParcelId", parcelId);
            _ = cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
            _ = cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var updated = cmd.ExecuteNonQuery() > 0;
            if (updated)
            {
                CurrentProject.MarkAsModified();
            }
            return updated;
        }

        public List<string> GetUniqueMapSheets()
        {
            var mapSheets = new List<string>();
            const string sql = @"
                SELECT DISTINCT MapSheetNo
                FROM tblOriginalLandParcels
                WHERE MapSheetNo IS NOT NULL AND TRIM(MapSheetNo) <> ''
                ORDER BY MapSheetNo";

            using var cmd = new SQLiteCommand(sql, _connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var mapSheet = reader["MapSheetNo"]?.ToString();
                if (!string.IsNullOrWhiteSpace(mapSheet))
                {
                    mapSheets.Add(mapSheet.Trim());
                }
            }

            return mapSheets;
        }

        public bool ParcelExists(string? parcelNo, string? mapSheetNo, int? excludeParcelId = null)
        {
            if (string.IsNullOrWhiteSpace(parcelNo) || string.IsNullOrWhiteSpace(mapSheetNo))
                return false;

            string sql = "SELECT COUNT(*) FROM tblOriginalLandParcels WHERE ParcelNo = @ParcelNo AND MapSheetNo = @MapSheetNo";
            if (excludeParcelId.HasValue)
            {
                sql += " AND ParcelId <> @ParcelId";
            }

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@ParcelNo", parcelNo);
            _ = cmd.Parameters.AddWithValue("@MapSheetNo", mapSheetNo);
            if (excludeParcelId.HasValue)
            {
                _ = cmd.Parameters.AddWithValue("@ParcelId", excludeParcelId.Value);
            }

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Gets or creates owner ID within a transaction
        /// </summary>
        private (int OwnerId, bool WasInserted) SaveOrGetOwnerIdWithTransaction(LandOwner owner, SQLiteTransaction transaction)
        {
            // Check if owner already exists
            string selectSql = @"
                SELECT LandOwnerId FROM tblLandOwner
                WHERE LandOwnersName = @LandOwnersName 
                AND COALESCE(FatherSpouse, '') = COALESCE(@FatherSpouse, '')
                AND COALESCE(CitizenshipNumber, '') = COALESCE(@CitizenshipNumber, '')";

            using (var cmd = new SQLiteCommand(selectSql, _connection, transaction))
            {
                _ = cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
                _ = cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
                _ = cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return (Convert.ToInt32(result), false);
                }
            }

            // Insert new owner
            string insertSql = @"
                INSERT INTO tblLandOwner (
                    LandOwnersName, FatherSpouse, Gender, CitizenshipNumber,
                    CitizenshipIssuedDistrict, CitizenshipIssuedDate,
                    PermanentAddress, TemporaryAddress, ContactNumber, EmailID,
                    IsAnonymous, CreatedDate
                ) VALUES (
                    @LandOwnersName, @FatherSpouse, @Gender, @CitizenshipNumber,
                    @CitizenshipIssuedDistrict, @CitizenshipIssuedDate,
                    @PermanentAddress, @TemporaryAddress, @ContactNumber, @EmailID,
                    @IsAnonymous, @CreatedDate
                );
                SELECT last_insert_rowid();";

            using (var cmd = new SQLiteCommand(insertSql, _connection, transaction))
            {
                _ = cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
                _ = cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
                _ = cmd.Parameters.AddWithValue("@Gender", owner.Gender ?? "");
                _ = cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");
                _ = cmd.Parameters.AddWithValue("@CitizenshipIssuedDistrict", owner.CitizenshipIssuedDistrict ?? "");
                _ = cmd.Parameters.AddWithValue("@CitizenshipIssuedDate", owner.CitizenshipIssuedDate ?? "");
                _ = cmd.Parameters.AddWithValue("@PermanentAddress", owner.PermanentAddress ?? "");
                _ = cmd.Parameters.AddWithValue("@TemporaryAddress", owner.TemporaryAddress ?? "");
                _ = cmd.Parameters.AddWithValue("@ContactNumber", owner.ContactNumber ?? "");
                _ = cmd.Parameters.AddWithValue("@EmailID", owner.EmailID ?? "");
                _ = cmd.Parameters.AddWithValue("@IsAnonymous", owner.IsAnonymous ? 1 : 0);
                _ = cmd.Parameters.AddWithValue("@CreatedDate", owner.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));

                return (Convert.ToInt32(cmd.ExecuteScalar()), true);
            }
        }

        /// <summary>
        /// Extracts unique owners from imported records and saves them
        /// Returns dictionary mapping unique keys to LandOwnerId
        /// </summary>
        public Dictionary<string, int> ExtractAndSaveUniqueOwners(List<BaselineLandParceRecord> records)
        {
            var ownerMap = new Dictionary<string, int>();
            var uniqueOwners = new Dictionary<string, LandOwner>();

            // Extract unique owners
            foreach (var record in records)
            {
                string key = GetOwnerKey(record);

                if (!uniqueOwners.ContainsKey(key))
                {
                    uniqueOwners[key] = new LandOwner
                    {
                        LandOwnersName = record.LandOwnersName ?? "Unknown",
                        FatherSpouse = record.FatherSpouse,
                        Gender = record.Gender,
                        CitizenshipNumber = record.CitizenshipNumber,
                        PermanentAddress = record.PermanentAddress,
                        CreatedDate = DateTime.Now
                    };
                }
            }

            // Save owners and build map
            foreach (var kvp in uniqueOwners)
            {
                int ownerId = SaveOrGetOwnerId(kvp.Value);
                ownerMap[kvp.Key] = ownerId;
            }

            return ownerMap;
        }

        /// <summary>
        /// Saves parcels with references to their owners
        /// </summary>
        public int SaveParcels(List<BaselineLandParceRecord> records, Dictionary<string, int> ownerMap)
        {
            int savedCount = 0;
            int skippedCount = 0;

            using var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var record in records)
                {
                    string ownerKey = GetOwnerKey(record);
                    if (!ownerMap.ContainsKey(ownerKey))
                    {
                        skippedCount++;
                        continue;
                    }

                    int ownerId = ownerMap[ownerKey];

                    // Check if parcel already exists (avoid duplicates)
                    if (ParcelExists(record.ParcelNo, record.MapSheetNo, transaction))
                    {
                        skippedCount++;
                        continue;
                    }

                    string sql = @"
                        INSERT INTO tblOriginalLandParcels (
                            LandOwnerId, ParcelNo, Province, District, MunicipalityVillage,
                            WardNo, ParcelLocation, MapSheetNo, Tenant, LandUse, LandOwnershipType,
                            AreaInSqm, AreaInRAPD, AreaInBKD,
                            MothNo, PaanaNo, Remarks, IsValid
                        ) VALUES (
                            @LandOwnerId, @ParcelNo, @Province, @District, @MunicipalityVillage,
                            @WardNo, @ParcelLocation, @MapSheetNo, @Tenant, @LandUse, @LandOwnershipType,
                            @AreaInSqm, @AreaInRAPD, @AreaInBKD,
                            @MothNo, @PaanaNo, @Remarks, @IsValid
                        )";

                    using var cmd = new SQLiteCommand(sql, _connection, transaction);
                    _ = cmd.Parameters.AddWithValue("@LandOwnerId", ownerId);
                    _ = cmd.Parameters.AddWithValue("@ParcelNo", record.ParcelNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@Province", record.Province ?? "");
                    _ = cmd.Parameters.AddWithValue("@District", record.District ?? "");
                    _ = cmd.Parameters.AddWithValue("@MunicipalityVillage", record.MunicipalityVillage ?? "");
                    _ = cmd.Parameters.AddWithValue("@WardNo", record.WardNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@ParcelLocation", record.ParcelLocation ?? "");
                    _ = cmd.Parameters.AddWithValue("@MapSheetNo", record.MapSheetNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@Tenant", record.Tenant ?? "");
                    _ = cmd.Parameters.AddWithValue("@LandUse", record.LandUse ?? "");
                    _ = cmd.Parameters.AddWithValue("@LandOwnershipType", record.LandOwnershipType ?? "");
                    _ = cmd.Parameters.AddWithValue("@AreaInSqm", record.AreaInSqm ?? (object)DBNull.Value);
                    _ = cmd.Parameters.AddWithValue("@AreaInRAPD", record.AreaInRAPD ?? "");
                    _ = cmd.Parameters.AddWithValue("@AreaInBKD", record.AreaInBKD ?? "");
                    _ = cmd.Parameters.AddWithValue("@MothNo", record.MothNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@PaanaNo", record.PaanaNo ?? "");
                    _ = cmd.Parameters.AddWithValue("@Remarks", record.Remarks ?? "");
                    _ = cmd.Parameters.AddWithValue("@IsValid", 1);

                    _ = cmd.ExecuteNonQuery();
                    savedCount++;
                }

                transaction.Commit();
                if (savedCount > 0)
                {
                    CurrentProject.MarkAsModified();
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return savedCount;
        }

        /// <summary>
        /// Gets or creates owner ID
        /// </summary>
        private int SaveOrGetOwnerId(LandOwner owner)
        {
            // Check if owner already exists
            string selectSql = @"
                SELECT LandOwnerId FROM tblLandOwner
                WHERE LandOwnersName = @LandOwnersName 
                AND COALESCE(FatherSpouse, '') = COALESCE(@FatherSpouse, '')
                AND COALESCE(CitizenshipNumber, '') = COALESCE(@CitizenshipNumber, '')";

            using (var cmd = new SQLiteCommand(selectSql, _connection))
            {
                _ = cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
                _ = cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
                _ = cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");

                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            // Insert new owner
            string insertSql = @"
                INSERT INTO tblLandOwner (
                    LandOwnersName, FatherSpouse, Gender, CitizenshipNumber,
                    CitizenshipIssuedDistrict, CitizenshipIssuedDate,
                    PermanentAddress, TemporaryAddress, ContactNumber, EmailID,
                    IsAnonymous, CreatedDate
                ) VALUES (
                    @LandOwnersName, @FatherSpouse, @Gender, @CitizenshipNumber,
                    @CitizenshipIssuedDistrict, @CitizenshipIssuedDate,
                    @PermanentAddress, @TemporaryAddress, @ContactNumber, @EmailID,
                    @IsAnonymous, @CreatedDate
                );
                SELECT last_insert_rowid();";

            using (var cmd = new SQLiteCommand(insertSql, _connection))
            {
                _ = cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
                _ = cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
                _ = cmd.Parameters.AddWithValue("@Gender", owner.Gender ?? "");
                _ = cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");
                _ = cmd.Parameters.AddWithValue("@CitizenshipIssuedDistrict", owner.CitizenshipIssuedDistrict ?? "");
                _ = cmd.Parameters.AddWithValue("@CitizenshipIssuedDate", owner.CitizenshipIssuedDate ?? "");
                _ = cmd.Parameters.AddWithValue("@PermanentAddress", owner.PermanentAddress ?? "");
                _ = cmd.Parameters.AddWithValue("@TemporaryAddress", owner.TemporaryAddress ?? "");
                _ = cmd.Parameters.AddWithValue("@ContactNumber", owner.ContactNumber ?? "");
                _ = cmd.Parameters.AddWithValue("@EmailID", owner.EmailID ?? "");
                _ = cmd.Parameters.AddWithValue("@IsAnonymous", owner.IsAnonymous ? 1 : 0);
                _ = cmd.Parameters.AddWithValue("@CreatedDate", owner.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));

                var ownerId = Convert.ToInt32(cmd.ExecuteScalar());
                CurrentProject.MarkAsModified();
                return ownerId;
            }
        }

        /// <summary>
        /// Updates owner photo path
        /// </summary>
        public void UpdateOwnerPhoto(int ownerId, string photoPath)
        {
            string sql = "UPDATE tblLandOwner SET PhotoPath = @PhotoPath, ModifiedDate = @ModifiedDate WHERE LandOwnerId = @OwnerId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@PhotoPath", photoPath);
            _ = cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            _ = cmd.Parameters.AddWithValue("@OwnerId", ownerId);
            if (cmd.ExecuteNonQuery() > 0)
            {
                CurrentProject.MarkAsModified();
            }
        }

        /// <summary>
        /// Updates owner documents folder path
        /// </summary>
        public void UpdateOwnerDocumentsFolder(int ownerId, string folderPath)
        {
            string sql = "UPDATE tblLandOwner SET DocumentsFolderPath = @FolderPath, ModifiedDate = @ModifiedDate WHERE LandOwnerId = @OwnerId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@FolderPath", folderPath);
            _ = cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            _ = cmd.Parameters.AddWithValue("@OwnerId", ownerId);
            if (cmd.ExecuteNonQuery() > 0)
            {
                CurrentProject.MarkAsModified();
            }
        }

        /// <summary>
        /// Gets all parcels with owner information (for display grid)
        /// </summary>
        public List<OriginalLandParcel> GetAllParcelsWithOwners()
        {
            var parcels = new List<OriginalLandParcel>();

            string sql = @"
                SELECT 
                    p.ParcelId, p.LandOwnerId, p.ParcelNo, p.Province, p.District, 
                    p.MunicipalityVillage, p.WardNo, p.ParcelLocation, p.MapSheetNo, p.Tenant, p.LandUse, p.LandOwnershipType,
                    p.AreaInSqm, p.AreaInRAPD, p.AreaInBKD, p.MothNo, p.PaanaNo, 
                    p.Remarks, p.IsValid,
                    o.LandOwnersName, o.FatherSpouse, o.Gender, o.CitizenshipNumber, 
                    o.CitizenshipIssuedDistrict, o.CitizenshipIssuedDate, o.PermanentAddress, 
                    o.TemporaryAddress, o.ContactNumber, o.EmailID,
                    o.PhotoPath, o.DocumentsFolderPath, o.IsAnonymous
                FROM tblOriginalLandParcels p
                LEFT JOIN tblLandOwner o ON p.LandOwnerId = o.LandOwnerId
                ORDER BY p.ParcelId";

            using var cmd = new SQLiteCommand(sql, _connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var parcel = new OriginalLandParcel
                {
                    ParcelId = reader.GetInt32(reader.GetOrdinal("ParcelId")),
                    LandOwnerId = reader.GetInt32(reader.GetOrdinal("LandOwnerId")),
                    ParcelNo = reader.GetString(reader.GetOrdinal("ParcelNo")),
                    Province = GetNullableString(reader, "Province"),
                    District = GetNullableString(reader, "District"),
                    MunicipalityVillage = GetNullableString(reader, "MunicipalityVillage"),
                    WardNo = GetNullableString(reader, "WardNo"),
                    ParcelLocation = GetNullableString(reader, "ParcelLocation"),
                    MapSheetNo = reader.GetString(reader.GetOrdinal("MapSheetNo")),
                    IsTenant = GetNullableString(reader, "Tenant"),
                    LandUse = GetNullableString(reader, "LandUse"),
                    LandOwnershipType = GetNullableString(reader, "LandOwnershipType"),
                    AreaInSqm = GetNullableDouble(reader, "AreaInSqm"),
                    AreaInRAPD = GetNullableString(reader, "AreaInRAPD"),
                    AreaInBKD = GetNullableString(reader, "AreaInBKD"),
                    MothNo = GetNullableString(reader, "MothNo"),
                    PaanaNo = GetNullableString(reader, "PaanaNo"),
                    Remarks = GetNullableString(reader, "Remarks"),
                    IsValid = reader.GetInt32(reader.GetOrdinal("IsValid")) == 1,
                    Owner = reader.IsDBNull(reader.GetOrdinal("LandOwnersName")) ? null : new LandOwner
                    {
                        LandOwnerId = reader.GetInt32(reader.GetOrdinal("LandOwnerId")),
                        LandOwnersName = reader.GetString(reader.GetOrdinal("LandOwnersName")),
                        FatherSpouse = GetNullableString(reader, "FatherSpouse"),
                        Gender = GetNullableString(reader, "Gender"),
                        CitizenshipNumber = GetNullableString(reader, "CitizenshipNumber"),
                        CitizenshipIssuedDistrict = GetNullableString(reader, "CitizenshipIssuedDistrict"),
                        CitizenshipIssuedDate = GetNullableString(reader, "CitizenshipIssuedDate"),
                        PermanentAddress = GetNullableString(reader, "PermanentAddress"),
                        TemporaryAddress = GetNullableString(reader, "TemporaryAddress"),
                        ContactNumber = GetNullableString(reader, "ContactNumber"),
                        EmailID = GetNullableString(reader, "EmailID"),
                        PhotoPath = GetNullableString(reader, "PhotoPath"),
                        DocumentsFolderPath = GetNullableString(reader, "DocumentsFolderPath"),
                        IsAnonymous = GetNullableInt(reader, "IsAnonymous") == 1
                    }
                };

                parcels.Add(parcel);
            }

            return parcels;
        }

        // Helper methods for nullable fields
        private static string? GetNullableString(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static double? GetNullableDouble(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDouble(ordinal);
        }

        private static int GetNullableInt(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        /// <summary>
        /// Gets owner key for deduplication
        /// </summary>
        private string GetOwnerKey(BaselineLandParceRecord record)
        {
            return $"{record.LandOwnersName?.Trim().ToUpper()}|{record.FatherSpouse?.Trim().ToUpper()}|{record.CitizenshipNumber?.Trim()}";
        }

        /// <summary>
        /// Gets total record count
        /// </summary>
        public int GetTotalRecordCount()
        {
            string sql = "SELECT COUNT(*) FROM tblOriginalLandParcels";
            using var cmd = new SQLiteCommand(sql, _connection);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Gets all parcels for a specific owner
        /// </summary>
        public List<OriginalLandParcel> GetParcelsByOwnerId(int landOwnerId)
        {
            var parcels = new List<OriginalLandParcel>();

            string sql = @"
                SELECT 
                    p.ParcelId, p.LandOwnerId, p.ParcelNo, p.Province, p.District, 
                    p.MunicipalityVillage, p.WardNo, p.ParcelLocation, p.MapSheetNo, p.Tenant, p.LandUse, 
                    p.AreaInSqm, p.AreaInRAPD, p.AreaInBKD, p.MothNo, p.PaanaNo, 
                    p.Remarks, p.IsValid, p.LandOwnershipType,
                    o.LandOwnersName, o.FatherSpouse, o.Gender, o.CitizenshipNumber, 
                    o.CitizenshipIssuedDistrict, o.CitizenshipIssuedDate, o.PermanentAddress, 
                    o.TemporaryAddress, o.ContactNumber, o.EmailID,
                    o.PhotoPath, o.DocumentsFolderPath, o.IsAnonymous
                FROM tblOriginalLandParcels p
                INNER JOIN tblLandOwner o ON p.LandOwnerId = o.LandOwnerId
                WHERE p.LandOwnerId = @LandOwnerId
                ORDER BY p.MapSheetNo, p.ParcelNo";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var parcel = new OriginalLandParcel
                {
                    ParcelId = reader.GetInt32(reader.GetOrdinal("ParcelId")),
                    LandOwnerId = reader.GetInt32(reader.GetOrdinal("LandOwnerId")),
                    ParcelNo = reader.GetString(reader.GetOrdinal("ParcelNo")),
                    Province = GetNullableString(reader, "Province"),
                    District = GetNullableString(reader, "District"),
                    MunicipalityVillage = GetNullableString(reader, "MunicipalityVillage"),
                    WardNo = GetNullableString(reader, "WardNo"),
                    ParcelLocation = GetNullableString(reader, "ParcelLocation"),
                    MapSheetNo = reader.GetString(reader.GetOrdinal("MapSheetNo")),
                    IsTenant = GetNullableString(reader, "Tenant"),
                    LandUse = GetNullableString(reader, "LandUse"),
                    LandOwnershipType = GetNullableString(reader, "LandOwnershipType"),
                    AreaInSqm = GetNullableDouble(reader, "AreaInSqm"),
                    AreaInRAPD = GetNullableString(reader, "AreaInRAPD"),
                    AreaInBKD = GetNullableString(reader, "AreaInBKD"),
                    MothNo = GetNullableString(reader, "MothNo"),
                    PaanaNo = GetNullableString(reader, "PaanaNo"),
                    Remarks = GetNullableString(reader, "Remarks"),
                    IsValid = reader.GetInt32(reader.GetOrdinal("IsValid")) == 1,
                    Owner = new LandOwner
                    {
                        LandOwnerId = reader.GetInt32(reader.GetOrdinal("LandOwnerId")),
                        LandOwnersName = reader.GetString(reader.GetOrdinal("LandOwnersName")),
                        FatherSpouse = GetNullableString(reader, "FatherSpouse"),
                        Gender = GetNullableString(reader, "Gender"),
                        CitizenshipNumber = GetNullableString(reader, "CitizenshipNumber"),
                        CitizenshipIssuedDistrict = GetNullableString(reader, "CitizenshipIssuedDistrict"),
                        CitizenshipIssuedDate = GetNullableString(reader, "CitizenshipIssuedDate"),
                        PermanentAddress = GetNullableString(reader, "PermanentAddress"),
                        TemporaryAddress = GetNullableString(reader, "TemporaryAddress"),
                        ContactNumber = GetNullableString(reader, "ContactNumber"),
                        EmailID = GetNullableString(reader, "EmailID"),
                        PhotoPath = GetNullableString(reader, "PhotoPath"),
                        DocumentsFolderPath = GetNullableString(reader, "DocumentsFolderPath"),
                        IsAnonymous = GetNullableInt(reader, "IsAnonymous") == 1
                    }
                };

                parcels.Add(parcel);
            }

            return parcels;
        }

        /// <summary>
        /// Gets count of parcels owned by a specific owner
        /// </summary>
        public int GetParcelCountByOwnerId(int landOwnerId)
        {
            string sql = "SELECT COUNT(*) FROM tblOriginalLandParcels WHERE LandOwnerId = @LandOwnerId";
            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Clears all parcel data (for re-import)
        /// </summary>
        public void ClearAllParcels()
        {
            string sql = "DELETE FROM tblOriginalLandParcels";
            using var cmd = new SQLiteCommand(sql, _connection);
            if (cmd.ExecuteNonQuery() > 0)
            {
                CurrentProject.MarkAsModified();
            }
        }

        /// <summary>
        /// Clears all landowner data
        /// </summary>
        public void ClearAllOwners()
        {
            string sql = "DELETE FROM tblLandOwner";
            using var cmd = new SQLiteCommand(sql, _connection);
            if (cmd.ExecuteNonQuery() > 0)
            {
                CurrentProject.MarkAsModified();
            }
        }

        /// <summary>
        /// Clears all parcels and owners (for complete data replacement)
        /// Parcels must be deleted first due to foreign key constraint
        /// </summary>
        public void ClearAllData()
        {
            using var transaction = _connection.BeginTransaction();
            try
            {
                // Delete parcels first (due to FK constraint)
                string deleteParcels = "DELETE FROM tblOriginalLandParcels";
                using (var cmd = new SQLiteCommand(deleteParcels, _connection, transaction))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                // Then delete owners
                string deleteOwners = "DELETE FROM tblLandOwner";
                using (var cmd = new SQLiteCommand(deleteOwners, _connection, transaction))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                CurrentProject.MarkAsModified();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Deletes a parcel by ID
        /// </summary>
        public bool DeleteParcel(int parcelId)
        {
            string sql = "DELETE FROM tblOriginalLandParcels WHERE ParcelId = @ParcelId";
            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@ParcelId", parcelId);
            var deleted = cmd.ExecuteNonQuery() > 0;
            if (deleted)
            {
                CurrentProject.MarkAsModified();
            }
            return deleted;
        }

        /// <summary>
        /// Deletes an owner and all associated parcels
        /// </summary>
        public bool DeleteOwnerWithParcels(int landOwnerId)
        {
            using var transaction = _connection.BeginTransaction();
            try
            {
                // Delete all parcels for this owner first (due to FK constraint)
                string deleteParcels = "DELETE FROM tblOriginalLandParcels WHERE LandOwnerId = @LandOwnerId";
                using (var cmd = new SQLiteCommand(deleteParcels, _connection, transaction))
                {
                    _ = cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
                    _ = cmd.ExecuteNonQuery();
                }

                // Delete the owner
                string deleteOwner = "DELETE FROM tblLandOwner WHERE LandOwnerId = @LandOwnerId";
                using (var cmd = new SQLiteCommand(deleteOwner, _connection, transaction))
                {
                    _ = cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
                    int result = cmd.ExecuteNonQuery();
                    transaction.Commit();
                    if (result > 0)
                    {
                        CurrentProject.MarkAsModified();
                        return true;
                    }

                    return false;
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Updates a parcel record
        /// </summary>
        public bool UpdateParcel(OriginalLandParcel parcel)
        {
            string sql = @"
                UPDATE tblOriginalLandParcels SET
                    ParcelNo = @ParcelNo,
                    Province = @Province,
                    District = @District,
                    MunicipalityVillage = @MunicipalityVillage,
                    WardNo = @WardNo,
                    ParcelLocation = @ParcelLocation,
                    MapSheetNo = @MapSheetNo,
                    Tenant = @Tenant,
                    LandUse = @LandUse,
                    LandOwnershipType = @LandOwnershipType,
                    AreaInSqm = @AreaInSqm,
                    AreaInRAPD = @AreaInRAPD,
                    AreaInBKD = @AreaInBKD,
                    MothNo = @MothNo,
                    PaanaNo = @PaanaNo,
                    Remarks = @Remarks,
                    ModifiedDate = @ModifiedDate
                WHERE ParcelId = @ParcelId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@ParcelId", parcel.ParcelId);
            _ = cmd.Parameters.AddWithValue("@ParcelNo", parcel.ParcelNo);
            _ = cmd.Parameters.AddWithValue("@Province", parcel.Province ?? "");
            _ = cmd.Parameters.AddWithValue("@District", parcel.District ?? "");
            _ = cmd.Parameters.AddWithValue("@MunicipalityVillage", parcel.MunicipalityVillage ?? "");
            _ = cmd.Parameters.AddWithValue("@WardNo", parcel.WardNo ?? "");
            _ = cmd.Parameters.AddWithValue("@ParcelLocation", parcel.ParcelLocation ?? "");
            _ = cmd.Parameters.AddWithValue("@MapSheetNo", parcel.MapSheetNo);
            _ = cmd.Parameters.AddWithValue("@Tenant", parcel.IsTenant ?? "");
            _ = cmd.Parameters.AddWithValue("@LandUse", parcel.LandUse ?? "");
            _ = cmd.Parameters.AddWithValue("@LandOwnershipType", parcel.LandOwnershipType ?? "");
            _ = cmd.Parameters.AddWithValue("@AreaInSqm", parcel.AreaInSqm ?? (object)DBNull.Value);
            _ = cmd.Parameters.AddWithValue("@AreaInRAPD", parcel.AreaInRAPD ?? "");
            _ = cmd.Parameters.AddWithValue("@AreaInBKD", parcel.AreaInBKD ?? "");
            _ = cmd.Parameters.AddWithValue("@MothNo", parcel.MothNo ?? "");
            _ = cmd.Parameters.AddWithValue("@PaanaNo", parcel.PaanaNo ?? "");
            _ = cmd.Parameters.AddWithValue("@Remarks", parcel.Remarks ?? "");
            _ = cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var updated = cmd.ExecuteNonQuery() > 0;
            if (updated)
            {
                CurrentProject.MarkAsModified();
            }
            return updated;
        }

        /// <summary>
        /// Updates an owner record
        /// </summary>
        public bool UpdateOwner(LandOwner owner)
        {
            string sql = @"
                UPDATE tblLandOwner SET
                    LandOwnersName = @LandOwnersName,
                    FatherSpouse = @FatherSpouse,
                    Gender = @Gender,
                    CitizenshipNumber = @CitizenshipNumber,
                    CitizenshipIssuedDistrict = @CitizenshipIssuedDistrict,
                    CitizenshipIssuedDate = @CitizenshipIssuedDate,
                    PermanentAddress = @PermanentAddress,
                    TemporaryAddress = @TemporaryAddress,
                    ContactNumber = @ContactNumber,
                    EmailID = @EmailID,
                    IsAnonymous = @IsAnonymous,
                    ModifiedDate = @ModifiedDate
                WHERE LandOwnerId = @LandOwnerId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@LandOwnerId", owner.LandOwnerId);
            _ = cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
            _ = cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
            _ = cmd.Parameters.AddWithValue("@Gender", owner.Gender ?? "");
            _ = cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");
            _ = cmd.Parameters.AddWithValue("@CitizenshipIssuedDistrict", owner.CitizenshipIssuedDistrict ?? "");
            _ = cmd.Parameters.AddWithValue("@CitizenshipIssuedDate", owner.CitizenshipIssuedDate ?? "");
            _ = cmd.Parameters.AddWithValue("@PermanentAddress", owner.PermanentAddress ?? "");
            _ = cmd.Parameters.AddWithValue("@TemporaryAddress", owner.TemporaryAddress ?? "");
            _ = cmd.Parameters.AddWithValue("@ContactNumber", owner.ContactNumber ?? "");
            _ = cmd.Parameters.AddWithValue("@EmailID", owner.EmailID ?? "");
            _ = cmd.Parameters.AddWithValue("@IsAnonymous", owner.IsAnonymous ? 1 : 0);
            _ = cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var updated = cmd.ExecuteNonQuery() > 0;
            if (updated)
            {
                CurrentProject.MarkAsModified();
            }
            return updated;
        }

        /// <summary>
        /// Gets a single parcel by ID with owner info
        /// </summary>
        public OriginalLandParcel? GetParcelById(int parcelId)
        {
            string sql = @"
                SELECT 
                    p.ParcelId, p.LandOwnerId, p.ParcelNo, p.Province, p.District, 
                    p.MunicipalityVillage, p.WardNo, p.ParcelLocation, p.MapSheetNo, p.Tenant, p.LandUse, p.LandOwnershipType,
                    p.AreaInSqm, p.AreaInRAPD, p.AreaInBKD, p.MothNo, p.PaanaNo, 
                    p.Remarks, p.IsValid,
                    o.LandOwnersName, o.FatherSpouse, o.Gender, o.CitizenshipNumber, 
                    o.CitizenshipIssuedDistrict, o.CitizenshipIssuedDate, o.PermanentAddress, 
                    o.TemporaryAddress, o.ContactNumber, o.EmailID,
                    o.PhotoPath, o.DocumentsFolderPath, o.IsAnonymous
                FROM tblOriginalLandParcels p
                INNER JOIN tblLandOwner o ON p.LandOwnerId = o.LandOwnerId
                WHERE p.ParcelId = @ParcelId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@ParcelId", parcelId);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new OriginalLandParcel
                {
                    ParcelId = reader.GetInt32(reader.GetOrdinal("ParcelId")),
                    LandOwnerId = reader.GetInt32(reader.GetOrdinal("LandOwnerId")),
                    ParcelNo = reader.GetString(reader.GetOrdinal("ParcelNo")),
                    Province = GetNullableString(reader, "Province"),
                    District = GetNullableString(reader, "District"),
                    MunicipalityVillage = GetNullableString(reader, "MunicipalityVillage"),
                    WardNo = GetNullableString(reader, "WardNo"),
                    ParcelLocation = GetNullableString(reader, "ParcelLocation"),
                    MapSheetNo = reader.GetString(reader.GetOrdinal("MapSheetNo")),
                    IsTenant = GetNullableString(reader, "Tenant"),
                    LandUse = GetNullableString(reader, "LandUse"),
                    LandOwnershipType = GetNullableString(reader, "LandOwnershipType"),
                    AreaInSqm = GetNullableDouble(reader, "AreaInSqm"),
                    AreaInRAPD = GetNullableString(reader, "AreaInRAPD"),
                    AreaInBKD = GetNullableString(reader, "AreaInBKD"),
                    MothNo = GetNullableString(reader, "MothNo"),
                    PaanaNo = GetNullableString(reader, "PaanaNo"),
                    Remarks = GetNullableString(reader, "Remarks"),
                    IsValid = reader.GetInt32(reader.GetOrdinal("IsValid")) == 1,
                    Owner = new LandOwner
                    {
                        LandOwnerId = reader.GetInt32(reader.GetOrdinal("LandOwnerId")),
                        LandOwnersName = reader.GetString(reader.GetOrdinal("LandOwnersName")),
                        FatherSpouse = GetNullableString(reader, "FatherSpouse"),
                        Gender = GetNullableString(reader, "Gender"),
                        CitizenshipNumber = GetNullableString(reader, "CitizenshipNumber"),
                        CitizenshipIssuedDistrict = GetNullableString(reader, "CitizenshipIssuedDistrict"),
                        CitizenshipIssuedDate = GetNullableString(reader, "CitizenshipIssuedDate"),
                        PermanentAddress = GetNullableString(reader, "PermanentAddress"),
                        TemporaryAddress = GetNullableString(reader, "TemporaryAddress"),
                        ContactNumber = GetNullableString(reader, "ContactNumber"),
                        EmailID = GetNullableString(reader, "EmailID"),
                        PhotoPath = GetNullableString(reader, "PhotoPath"),
                        DocumentsFolderPath = GetNullableString(reader, "DocumentsFolderPath"),
                        IsAnonymous = GetNullableInt(reader, "IsAnonymous") == 1
                    }
                };
            }

            return null;
        }

        /// <summary>
        /// Gets all unique landowners
        /// </summary>
        public List<LandOwner> GetAllOwners()
        {
            var owners = new List<LandOwner>();

            string sql = @"
                SELECT LandOwnerId, LandOwnersName, FatherSpouse, Gender, CitizenshipNumber,
                       CitizenshipIssuedDistrict, CitizenshipIssuedDate,
                       PermanentAddress, TemporaryAddress, ContactNumber, EmailID,
                       PhotoPath, DocumentsFolderPath, IsAnonymous, CreatedDate
                FROM tblLandOwner
                ORDER BY LandOwnersName";

            using var cmd = new SQLiteCommand(sql, _connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                owners.Add(new LandOwner
                {
                    LandOwnerId = reader.GetInt32(reader.GetOrdinal("LandOwnerId")),
                    LandOwnersName = reader.GetString(reader.GetOrdinal("LandOwnersName")),
                    FatherSpouse = GetNullableString(reader, "FatherSpouse"),
                    Gender = GetNullableString(reader, "Gender"),
                    CitizenshipNumber = GetNullableString(reader, "CitizenshipNumber"),
                    CitizenshipIssuedDistrict = GetNullableString(reader, "CitizenshipIssuedDistrict"),
                    CitizenshipIssuedDate = GetNullableString(reader, "CitizenshipIssuedDate"),
                    PermanentAddress = GetNullableString(reader, "PermanentAddress"),
                    TemporaryAddress = GetNullableString(reader, "TemporaryAddress"),
                    ContactNumber = GetNullableString(reader, "ContactNumber"),
                    EmailID = GetNullableString(reader, "EmailID"),
                    PhotoPath = GetNullableString(reader, "PhotoPath"),
                    DocumentsFolderPath = GetNullableString(reader, "DocumentsFolderPath"),
                    IsAnonymous = GetNullableInt(reader, "IsAnonymous") == 1
                });
            }

            return owners;
        }

        /// <summary>
        /// Gets a single land owner by ID
        /// </summary>
        public LandOwner? GetOwnerById(int landOwnerId)
        {
            string sql = @"
                SELECT LandOwnerId, LandOwnersName, FatherSpouse, Gender, CitizenshipNumber,
                       CitizenshipIssuedDistrict, CitizenshipIssuedDate, PermanentAddress,
                       TemporaryAddress, ContactNumber, EmailID, PhotoPath, DocumentsFolderPath,
                       IsAnonymous, CreatedDate, ModifiedDate
                FROM tblLandOwner
                WHERE LandOwnerId = @LandOwnerId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new LandOwner
                {
                    LandOwnerId = reader.GetInt32(reader.GetOrdinal("LandOwnerId")),
                    LandOwnersName = reader.GetString(reader.GetOrdinal("LandOwnersName")),
                    FatherSpouse = GetNullableString(reader, "FatherSpouse"),
                    Gender = GetNullableString(reader, "Gender"),
                    CitizenshipNumber = GetNullableString(reader, "CitizenshipNumber"),
                    CitizenshipIssuedDistrict = GetNullableString(reader, "CitizenshipIssuedDistrict"),
                    CitizenshipIssuedDate = GetNullableString(reader, "CitizenshipIssuedDate"),
                    PermanentAddress = GetNullableString(reader, "PermanentAddress"),
                    TemporaryAddress = GetNullableString(reader, "TemporaryAddress"),
                    ContactNumber = GetNullableString(reader, "ContactNumber"),
                    EmailID = GetNullableString(reader, "EmailID"),
                    PhotoPath = GetNullableString(reader, "PhotoPath"),
                    DocumentsFolderPath = GetNullableString(reader, "DocumentsFolderPath"),
                    IsAnonymous = GetNullableInt(reader, "IsAnonymous") == 1,
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    ModifiedDate = GetNullableDateTime(reader, "ModifiedDate")
                };
            }

            return null;
        }

        /// <summary>
        /// Gets the total area in square meters for all parcels owned by a specific owner
        /// </summary>
        public double GetTotalAreaByOwnerId(int landOwnerId)
        {
            string sql = @"
                SELECT SUM(AreaInSqm) as TotalArea
                FROM tblOriginalLandParcels
                WHERE LandOwnerId = @LandOwnerId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);

            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? Convert.ToDouble(result) : 0.0;
        }

        /// <summary>
        /// Gets the count of documents for a specific owner
        /// </summary>
        public int GetDocumentCountByOwnerId(int landOwnerId)
        {
            var owner = GetOwnerById(landOwnerId);
            if (owner == null || string.IsNullOrWhiteSpace(owner.DocumentsFolderPath))
                return 0;

            string projectDir = Path.GetDirectoryName(CurrentProject.Info.ProjectPath) ?? "";
            string docsPath = Path.Combine(projectDir, owner.DocumentsFolderPath);

            if (!Directory.Exists(docsPath))
                return 0;

            return Directory.GetFiles(docsPath).Length;
        }

        /// <summary>
        /// Updates the photo path for a specific owner
        /// </summary>
        public void UpdateOwnerPhotoPath(int landOwnerId, string photoPath)
        {
            string sql = @"
                UPDATE tblLandOwner 
                SET PhotoPath = @PhotoPath, ModifiedDate = @ModifiedDate
                WHERE LandOwnerId = @LandOwnerId";

            using var cmd = new SQLiteCommand(sql, _connection);
            _ = cmd.Parameters.AddWithValue("@PhotoPath", photoPath);
            _ = cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
            _ = cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);

            _ = cmd.ExecuteNonQuery();
            CurrentProject.MarkAsModified();
        }

        // Helper method for reading nullable DateTime from SQLiteDataReader
        private static DateTime? GetNullableDateTime(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return null;

            string dateString = reader.GetString(ordinal);
            return DateTime.TryParse(dateString, out DateTime result) ? result : null;
        }
    }
}
