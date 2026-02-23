using System.Data.SQLite;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Extended database schema for full Land Pooling System
    /// This version DOES NOT modify existing tables.
    /// It only adds new required tables.
    /// </summary>
    public class _DatabaseSchema
    {
        public static void CreateSchema(SQLiteConnection connection)
        {
            CreateProjectInfoTable(connection);
            CreateLandOwnerTable(connection);
            CreateOriginalLandParcelsTable(connection);

            // NEW TABLES (safe additions)
            CreateLayersTable(connection);
            CreateParcelGeometryTable(connection);
            CreateRoadCenterlineTable(connection);
            CreateRoadSurfaceTable(connection);
            CreateBlocksTable(connection);
            CreateOwnerPoolingSummaryTable(connection);
            CreateReplotAllocationTable(connection);
            CreateParcelRelationsTable(connection);
        }

        #region EXISTING TABLES (UNCHANGED)

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

            Execute(connection, sql);
        }

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
                    TemporaryAddress TEXT,
                    ContactNumber TEXT,
                    EmailID TEXT,
                    PhotoPath TEXT,
                    DocumentsFolderPath TEXT,
                    IsAnonymous INTEGER DEFAULT 0,
                    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                    ModifiedDate TEXT,
                    UNIQUE(LandOwnersName, FatherSpouse, CitizenshipNumber)
                );";

            Execute(connection, sql);
        }

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
                    Tenant TEXT,
                    LandUse TEXT,
                    LandOwnershipType TEXT,
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

            Execute(connection, sql);
        }

        #endregion

        #region NEW TABLES FOR LAND POOLING

        private static void CreateLayersTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblLayers (
                    LayerId INTEGER PRIMARY KEY AUTOINCREMENT,
                    LayerName TEXT NOT NULL,
                    LayerType TEXT,
                    IsVisible INTEGER DEFAULT 1,
                    IsLocked INTEGER DEFAULT 0,
                    ZOrder INTEGER DEFAULT 0,
                    StrokeColor INTEGER,
                    FillColor INTEGER,
                    StrokeWidth REAL DEFAULT 1.0,
                    Opacity REAL DEFAULT 1.0
                );";

            Execute(connection, sql);
        }

        private static void CreateParcelGeometryTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblParcelGeometry (
                    GeometryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ParcelId INTEGER,
                    LayerId INTEGER,
                    GeometryWKT TEXT NOT NULL,
                    BBoxMinX REAL,
                    BBoxMinY REAL,
                    BBoxMaxX REAL,
                    BBoxMaxY REAL,
                    AreaSqm REAL,
                    FOREIGN KEY(ParcelId) REFERENCES tblOriginalLandParcels(ParcelId) ON DELETE CASCADE
                );";

            Execute(connection, sql);
        }

        private static void CreateRoadCenterlineTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblRoadCenterlines (
                    RoadId INTEGER PRIMARY KEY AUTOINCREMENT,
                    RoadName TEXT,
                    Width REAL NOT NULL,
                    CornerRadius REAL DEFAULT 0,
                    GeometryWKT TEXT NOT NULL,
                    BBoxMinX REAL,
                    BBoxMinY REAL,
                    BBoxMaxX REAL,
                    BBoxMaxY REAL,
                    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP
                );";

            Execute(connection, sql);
        }

        private static void CreateRoadSurfaceTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblRoadSurface (
                    SurfaceId INTEGER PRIMARY KEY AUTOINCREMENT,
                    GeometryWKT TEXT NOT NULL,
                    BBoxMinX REAL,
                    BBoxMinY REAL,
                    BBoxMaxX REAL,
                    BBoxMaxY REAL,
                    AreaSqm REAL,
                    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP
                );";

            Execute(connection, sql);
        }

        private static void CreateBlocksTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblBlocks (
                    BlockId INTEGER PRIMARY KEY AUTOINCREMENT,
                    BlockNumber TEXT,
                    GeometryWKT TEXT NOT NULL,
                    BBoxMinX REAL,
                    BBoxMinY REAL,
                    BBoxMaxX REAL,
                    BBoxMaxY REAL,
                    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP
                );";

            Execute(connection, sql);
        }

        private static void CreateOwnerPoolingSummaryTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblOwnerPoolingSummary (
                    SummaryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    LandOwnerId INTEGER,
                    OriginalArea REAL,
                    DeductedArea REAL,
                    NetArea REAL,
                    DeductionPercentage REAL,
                    CalculatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY(LandOwnerId) REFERENCES tblLandOwner(LandOwnerId) ON DELETE CASCADE
                );";

            Execute(connection, sql);
        }

        private static void CreateReplotAllocationTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblReplotAllocation (
                    AllocationId INTEGER PRIMARY KEY AUTOINCREMENT,
                    LandOwnerId INTEGER,
                    ReplotParcelId INTEGER,
                    AllocatedArea REAL,
                    Status TEXT,
                    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY(LandOwnerId) REFERENCES tblLandOwner(LandOwnerId)
                );";

            Execute(connection, sql);
        }

        private static void CreateParcelRelationsTable(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS tblParcelRelations (
                    RelationId INTEGER PRIMARY KEY AUTOINCREMENT,
                    FromParcelId INTEGER,
                    ToParcelId INTEGER,
                    RelationType TEXT,
                    AreaOverlap REAL
                );";

            Execute(connection, sql);
        }

        #endregion

        private static void Execute(SQLiteConnection connection, string sql)
        {
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }
    }
}