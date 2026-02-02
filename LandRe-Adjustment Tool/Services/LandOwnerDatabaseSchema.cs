using System.Data.SQLite;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Database schema manager for landowner records
    /// Creates and manages tables: tblLandOwner, tblOriginalLandParcels
    /// </summary>
    public class LandOwnerDatabaseSchema
    {
        /// <summary>
        /// Creates all required tables for landowner management
        /// </summary>
        public static void CreateSchema(SQLiteConnection connection)
        {
            CreateLandOwnerTable(connection);
            CreateOriginalLandParcelsTable(connection);
            MigrateAddressToParcelLocation(connection);
        }

        /// <summary>
        /// Creates tblLandOwner - stores unique landowners
        /// Contains only owner-specific information (personal details, identification)
        /// </summary>
        private static void CreateLandOwnerTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblLandOwner (
                    LandOwnerId INTEGER PRIMARY KEY AUTOINCREMENT,
                    LandOwnersName TEXT NOT NULL,
                    FatherSpouse TEXT,
                    Gender TEXT,
                    CitizenshipNumber TEXT,
                    CitizenshipIssuedDistrict TEXT,
                    CitizenshipIssuedDate TEXT,
                    PermanentAddress TEXT,
                    ContactNumber TEXT,
                    PhotoPath TEXT,
                    DocumentsFolderPath TEXT,
                    IsAnonymous INTEGER DEFAULT 0,
                    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                    ModifiedDate TEXT,
                    UNIQUE(LandOwnersName, FatherSpouse, CitizenshipNumber)
                );";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();

            // Create indexes for better performance
            CreateIndex(connection, "idx_landowner_name", "tblLandOwner", "LandOwnersName");
            CreateIndex(connection, "idx_landowner_citizenship", "tblLandOwner", "CitizenshipNumber");
        }

        /// <summary>
        /// Creates tblOriginalLandParcels - stores parcel data with FK to owner
        /// Contains all parcel-specific information including location details
        /// </summary>
        private static void CreateOriginalLandParcelsTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblOriginalLandParcels (
                    ParcelId INTEGER PRIMARY KEY AUTOINCREMENT,
                    LandOwnerId INTEGER NOT NULL,
                    ParcelNo TEXT NOT NULL,
                    Province TEXT,
                    District TEXT,
                    MunicipalityVillage TEXT,
                    WardNo TEXT,
                    ParcelLocation TEXT,
                    MapSheetNo TEXT NOT NULL,
                    IsTenant TEXT,
                    LandUse TEXT,
                    AreaInSqm REAL,
                    AreaInRAPD TEXT,
                    AreaInBKD TEXT,
                    MothNo TEXT,
                    PaanaNo TEXT,
                    Remarks TEXT,
                    ImportedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                    ModifiedDate TEXT,
                    IsValid INTEGER DEFAULT 1,
                    ValidationErrors TEXT,
                    FOREIGN KEY (LandOwnerId) REFERENCES tblLandOwner(LandOwnerId) ON DELETE CASCADE,
                    UNIQUE(ParcelNo, MapSheetNo)
                );";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();

            // Create indexes
            CreateIndex(connection, "idx_parcel_no", "tblOriginalLandParcels", "ParcelNo");
            CreateIndex(connection, "idx_mapsheet_no", "tblOriginalLandParcels", "MapSheetNo");
            CreateIndex(connection, "idx_landowner_fk", "tblOriginalLandParcels", "LandOwnerId");
        }

        /// <summary>
        /// Helper method to create indexes
        /// </summary>
        private static void CreateIndex(SQLiteConnection connection, string indexName, string tableName, string columnName)
        {
            string sql = $"CREATE INDEX IF NOT EXISTS {indexName} ON {tableName}({columnName});";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Checks if schema exists and is up to date
        /// </summary>
        public static bool SchemaExists(SQLiteConnection connection)
        {
            string sql = @"
                SELECT COUNT(*) FROM sqlite_master 
                WHERE type='table' AND name IN ('tblLandOwner', 'tblOriginalLandParcels');";

            using var cmd = new SQLiteCommand(sql, connection);
            long count = (long)cmd.ExecuteScalar();
            return count == 2;
        }

        /// <summary>
        /// Drops and recreates all tables - USE WITH CAUTION (data will be lost)
        /// </summary>
        public static void RecreateSchema(SQLiteConnection connection)
        {
            DropTables(connection);
            CreateSchema(connection);
        }

        /// <summary>
        /// Drops all landowner-related tables
        /// </summary>
        public static void DropTables(SQLiteConnection connection)
        {
            string[] dropStatements = new[]
            {
                "DROP TABLE IF EXISTS tblOriginalLandParcels;",
                "DROP TABLE IF EXISTS tblLandOwner;"
            };

            foreach (var sql in dropStatements)
            {
                using var cmd = new SQLiteCommand(sql, connection);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Checks if the LandOwnersName column exists in tblLandOwner
        /// </summary>
        public static bool HasCorrectSchema(SQLiteConnection connection)
        {
            try
            {
                string sql = "PRAGMA table_info(tblLandOwner);";
                using var cmd = new SQLiteCommand(sql, connection);
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    string columnName = reader.GetString(1);
                    if (columnName == "LandOwnersName")
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Migrates schema to move ParcelLocation from owner to parcel table
        /// and adds WardNo column to parcel table
        /// </summary>
        private static void MigrateAddressToParcelLocation(SQLiteConnection connection)
        {
            try
            {
                // Check if tblOriginalLandParcels exists
                string checkTableSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='tblOriginalLandParcels';";
                using var checkCmd = new SQLiteCommand(checkTableSql, connection);
                var tableExists = checkCmd.ExecuteScalar();
                
                if (tableExists == null)
                    return; // Table doesn't exist yet, no migration needed
                
                // Check current column names in parcels table
                string pragmaSql = "PRAGMA table_info(tblOriginalLandParcels);";
                bool hasWardNo = false;
                bool hasParcelLocation = false;
                
                using (var cmd = new SQLiteCommand(pragmaSql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader.GetString(1);
                        if (columnName == "WardNo")
                            hasWardNo = true;
                        if (columnName == "ParcelLocation")
                            hasParcelLocation = true;
                    }
                }
                
                // Add WardNo column if missing
                if (!hasWardNo)
                {
                    string addWardNoSql = "ALTER TABLE tblOriginalLandParcels ADD COLUMN WardNo TEXT;";
                    using var addCmd = new SQLiteCommand(addWardNoSql, connection);
                    addCmd.ExecuteNonQuery();
                }
                
                // Add ParcelLocation column if missing
                if (!hasParcelLocation)
                {
                    string addParcelLocationSql = "ALTER TABLE tblOriginalLandParcels ADD COLUMN ParcelLocation TEXT;";
                    using var addCmd = new SQLiteCommand(addParcelLocationSql, connection);
                    addCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log or handle migration error - but don't crash the app
                System.Diagnostics.Debug.WriteLine($"Migration warning: {ex.Message}");
            }
        }
    }
}
