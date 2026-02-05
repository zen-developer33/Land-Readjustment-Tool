using System.Data.SQLite;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Database schema manager for all application tables
    /// Creates and manages tables: ProjectInfo, tblLandOwner, tblOriginalLandParcels
    /// </summary>
    public class DatabaseSchema
    {
        /// <summary>
        /// Creates all required database tables
        /// </summary>
        public static void CreateSchema(SQLiteConnection connection)
        {
            CreateProjectInfoTable(connection);
            CreateLandOwnerTable(connection);
            CreateOriginalLandParcelsTable(connection);
        }

        /// <summary>
        /// Creates ProjectInfo table - stores project metadata
        /// </summary>
        private static void CreateProjectInfoTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS ProjectInfo (
                    GUID TEXT PRIMARY KEY,
                    ProjectName TEXT,
                    ProjectPath TEXT,
                    CreatedDate TEXT,
                    ApprovalDate TEXT,
                    Province TEXT,
                    District TEXT,
                    Municipality TEXT,
                    WardNo TEXT,
                    ProjectSite TEXT,
                    ImplementingAgency TEXT,
                    ConsultingAgency TEXT,
                    ProjectNotes TEXT
                );";

            using (var cmd = new SQLiteCommand(sql, connection))
            {
                _ = cmd.ExecuteNonQuery();
            }
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
                    ContactInfo TEXT,
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
                    AreaInRAPD TEXT N0T NULL,
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

            // Create indexes for efficient search and filter
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
                WHERE type='table' AND name IN ('ProjectInfo', 'tblLandOwner', 'tblOriginalLandParcels');";

            using var cmd = new SQLiteCommand(sql, connection);
            long count = (long)cmd.ExecuteScalar();
            return count == 3;
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
        /// Drops all application tables
        /// </summary>
        public static void DropTables(SQLiteConnection connection)
        {
            string[] dropStatements = new[]
            {
                "DROP TABLE IF EXISTS tblOriginalLandParcels;",
                "DROP TABLE IF EXISTS tblLandOwner;",
                "DROP TABLE IF EXISTS ProjectInfo;"
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
    }
}
