using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using System.Data;
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
                    cmd.Parameters.AddWithValue("@LandOwnerId", ownerId);
                    cmd.Parameters.AddWithValue("@ParcelNo", record.ParcelNo ?? "");
                    cmd.Parameters.AddWithValue("@Province", record.Province ?? "");
                    cmd.Parameters.AddWithValue("@District", record.District ?? "");
                    cmd.Parameters.AddWithValue("@MunicipalityVillage", record.MunicipalityVillage ?? "");
                    cmd.Parameters.AddWithValue("@WardNo", record.WardNo ?? "");
                    cmd.Parameters.AddWithValue("@ParcelLocation", record.ParcelLocation ?? "");
                    cmd.Parameters.AddWithValue("@MapSheetNo", record.MapSheetNo ?? "");
                    cmd.Parameters.AddWithValue("@Tenant", record.Tenant ?? "");
                    cmd.Parameters.AddWithValue("@LandUse", record.LandUse ?? "");
                    cmd.Parameters.AddWithValue("@LandOwnershipType", record.LandOwnershipType ?? "");
                    cmd.Parameters.AddWithValue("@AreaInSqm", record.AreaInSqm ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AreaInRAPD", record.AreaInRAPD ?? "");
                    cmd.Parameters.AddWithValue("@AreaInBKD", record.AreaInBKD ?? "");
                    cmd.Parameters.AddWithValue("@MothNo", record.MothNo ?? "");
                    cmd.Parameters.AddWithValue("@PaanaNo", record.PaanaNo ?? "");
                    cmd.Parameters.AddWithValue("@Remarks", record.Remarks ?? "");
                    cmd.Parameters.AddWithValue("@IsValid", 1);

                    cmd.ExecuteNonQuery();
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
            cmd.Parameters.AddWithValue("@ParcelNo", parcelNo);
            cmd.Parameters.AddWithValue("@MapSheetNo", mapSheetNo);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
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
            cmd.Parameters.AddWithValue("@ParcelNo", parcelNo);
            cmd.Parameters.AddWithValue("@MapSheetNo", mapSheetNo);
            if (excludeParcelId.HasValue)
            {
                cmd.Parameters.AddWithValue("@ParcelId", excludeParcelId.Value);
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
                cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
                cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
                cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");

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
                cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
                cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
                cmd.Parameters.AddWithValue("@Gender", owner.Gender ?? "");
                cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");
                cmd.Parameters.AddWithValue("@CitizenshipIssuedDistrict", owner.CitizenshipIssuedDistrict ?? "");
                cmd.Parameters.AddWithValue("@CitizenshipIssuedDate", owner.CitizenshipIssuedDate ?? "");
                cmd.Parameters.AddWithValue("@PermanentAddress", owner.PermanentAddress ?? "");
                cmd.Parameters.AddWithValue("@TemporaryAddress", owner.TemporaryAddress ?? "");
                cmd.Parameters.AddWithValue("@ContactNumber", owner.ContactNumber ?? "");
                cmd.Parameters.AddWithValue("@EmailID", owner.EmailID ?? "");
                cmd.Parameters.AddWithValue("@IsAnonymous", owner.IsAnonymous ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreatedDate", owner.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));

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

            using var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var record in records)
                {
                    string ownerKey = GetOwnerKey(record);
                    if (!ownerMap.ContainsKey(ownerKey)) continue;

                    int ownerId = ownerMap[ownerKey];

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
                    cmd.Parameters.AddWithValue("@LandOwnerId", ownerId);
                    cmd.Parameters.AddWithValue("@ParcelNo", record.ParcelNo ?? "");
                    cmd.Parameters.AddWithValue("@Province", record.Province ?? "");
                    cmd.Parameters.AddWithValue("@District", record.District ?? "");
                    cmd.Parameters.AddWithValue("@MunicipalityVillage", record.MunicipalityVillage ?? "");
                    cmd.Parameters.AddWithValue("@WardNo", record.WardNo ?? "");
                    cmd.Parameters.AddWithValue("@ParcelLocation", record.ParcelLocation ?? "");
                    cmd.Parameters.AddWithValue("@MapSheetNo", record.MapSheetNo ?? "");
                    cmd.Parameters.AddWithValue("@Tenant", record.Tenant ?? "");
                    cmd.Parameters.AddWithValue("@LandUse", record.LandUse ?? "");
                    cmd.Parameters.AddWithValue("@LandOwnershipType", record.LandOwnershipType ?? "");
                    cmd.Parameters.AddWithValue("@AreaInSqm", record.AreaInSqm ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AreaInRAPD", record.AreaInRAPD ?? "");
                    cmd.Parameters.AddWithValue("@AreaInBKD", record.AreaInBKD ?? "");
                    cmd.Parameters.AddWithValue("@MothNo", record.MothNo ?? "");
                    cmd.Parameters.AddWithValue("@PaanaNo", record.PaanaNo ?? "");
                    cmd.Parameters.AddWithValue("@Remarks", record.Remarks ?? "");
                    cmd.Parameters.AddWithValue("@IsValid", 1);

                    cmd.ExecuteNonQuery();
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
                cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
                cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
                cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");

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
                cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
                cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
                cmd.Parameters.AddWithValue("@Gender", owner.Gender ?? "");
                cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");
                cmd.Parameters.AddWithValue("@CitizenshipIssuedDistrict", owner.CitizenshipIssuedDistrict ?? "");
                cmd.Parameters.AddWithValue("@CitizenshipIssuedDate", owner.CitizenshipIssuedDate ?? "");
                cmd.Parameters.AddWithValue("@PermanentAddress", owner.PermanentAddress ?? "");
                cmd.Parameters.AddWithValue("@TemporaryAddress", owner.TemporaryAddress ?? "");
                cmd.Parameters.AddWithValue("@ContactNumber", owner.ContactNumber ?? "");
                cmd.Parameters.AddWithValue("@EmailID", owner.EmailID ?? "");
                cmd.Parameters.AddWithValue("@IsAnonymous", owner.IsAnonymous ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreatedDate", owner.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));

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
            cmd.Parameters.AddWithValue("@PhotoPath", photoPath);
            cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);
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
            cmd.Parameters.AddWithValue("@FolderPath", folderPath);
            cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);
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
            cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
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
            cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
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
                    cmd.ExecuteNonQuery();
                }

                // Then delete owners
                string deleteOwners = "DELETE FROM tblLandOwner";
                using (var cmd = new SQLiteCommand(deleteOwners, _connection, transaction))
                {
                    cmd.ExecuteNonQuery();
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
            cmd.Parameters.AddWithValue("@ParcelId", parcelId);
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
                    cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
                    cmd.ExecuteNonQuery();
                }

                // Delete the owner
                string deleteOwner = "DELETE FROM tblLandOwner WHERE LandOwnerId = @LandOwnerId";
                using (var cmd = new SQLiteCommand(deleteOwner, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@LandOwnerId", landOwnerId);
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
            cmd.Parameters.AddWithValue("@ParcelId", parcel.ParcelId);
            cmd.Parameters.AddWithValue("@ParcelNo", parcel.ParcelNo);
            cmd.Parameters.AddWithValue("@Province", parcel.Province ?? "");
            cmd.Parameters.AddWithValue("@District", parcel.District ?? "");
            cmd.Parameters.AddWithValue("@MunicipalityVillage", parcel.MunicipalityVillage ?? "");
            cmd.Parameters.AddWithValue("@WardNo", parcel.WardNo ?? "");
            cmd.Parameters.AddWithValue("@ParcelLocation", parcel.ParcelLocation ?? "");
            cmd.Parameters.AddWithValue("@MapSheetNo", parcel.MapSheetNo);
            cmd.Parameters.AddWithValue("@Tenant", parcel.IsTenant ?? "");
            cmd.Parameters.AddWithValue("@LandUse", parcel.LandUse ?? "");
            cmd.Parameters.AddWithValue("@LandOwnershipType", parcel.LandOwnershipType ?? "");
            cmd.Parameters.AddWithValue("@AreaInSqm", parcel.AreaInSqm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AreaInRAPD", parcel.AreaInRAPD ?? "");
            cmd.Parameters.AddWithValue("@AreaInBKD", parcel.AreaInBKD ?? "");
            cmd.Parameters.AddWithValue("@MothNo", parcel.MothNo ?? "");
            cmd.Parameters.AddWithValue("@PaanaNo", parcel.PaanaNo ?? "");
            cmd.Parameters.AddWithValue("@Remarks", parcel.Remarks ?? "");
            cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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
                    ModifiedDate = @ModifiedDate
                WHERE LandOwnerId = @LandOwnerId";

            using var cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@LandOwnerId", owner.LandOwnerId);
            cmd.Parameters.AddWithValue("@LandOwnersName", owner.LandOwnersName);
            cmd.Parameters.AddWithValue("@FatherSpouse", owner.FatherSpouse ?? "");
            cmd.Parameters.AddWithValue("@Gender", owner.Gender ?? "");
            cmd.Parameters.AddWithValue("@CitizenshipNumber", owner.CitizenshipNumber ?? "");
            cmd.Parameters.AddWithValue("@CitizenshipIssuedDistrict", owner.CitizenshipIssuedDistrict ?? "");
            cmd.Parameters.AddWithValue("@CitizenshipIssuedDate", owner.CitizenshipIssuedDate ?? "");
            cmd.Parameters.AddWithValue("@PermanentAddress", owner.PermanentAddress ?? "");
            cmd.Parameters.AddWithValue("@TemporaryAddress", owner.TemporaryAddress ?? "");
            cmd.Parameters.AddWithValue("@ContactNumber", owner.ContactNumber ?? "");
            cmd.Parameters.AddWithValue("@EmailID", owner.EmailID ?? "");
            cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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
            cmd.Parameters.AddWithValue("@ParcelId", parcelId);
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
    }
}
