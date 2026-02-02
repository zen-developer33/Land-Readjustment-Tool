PRAGMA foreign_keys = ON;

-- =====================================================
-- PROJECT
-- =====================================================
CREATE TABLE Project (
    ProjectId INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectName TEXT NOT NULL,
    ReferenceCode TEXT,
    Location TEXT,
    Municipality TEXT,
    WardNo TEXT,
    SurveyDate TEXT,
    CRS TEXT DEFAULT 'NEPAL_TM',
    TotalOriginalArea REAL,
    TotalReplotArea REAL,
    CreatedOn TEXT DEFAULT CURRENT_TIMESTAMP,
    LastModifiedOn TEXT,
    Notes TEXT
);

-- =====================================================
-- LAYERS
-- =====================================================
CREATE TABLE Layer (
    LayerId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    LayerType TEXT,
    Color TEXT,
    LineType TEXT,
    IsVisible INTEGER DEFAULT 1,
    IsLocked INTEGER DEFAULT 0
);

-- =====================================================
-- OWNERS
-- =====================================================
CREATE TABLE Owner (
    OwnerId INTEGER PRIMARY KEY AUTOINCREMENT,
    OwnerCode TEXT,
    FullName TEXT NOT NULL,
    CitizenshipNo TEXT,
    Address TEXT,
    ContactNo TEXT,
    Remarks TEXT
);

-- =====================================================
-- ORIGINAL PARCELS
-- =====================================================
CREATE TABLE OriginalParcel (
    ParcelId INTEGER PRIMARY KEY AUTOINCREMENT,
    ParcelNo TEXT,
    OwnerId INTEGER,
    GeometryWKT TEXT NOT NULL,
    Area REAL,
    LandUse TEXT,
    SheetNo TEXT,
    Remarks TEXT,
    IsActive INTEGER DEFAULT 1,
    FOREIGN KEY (OwnerId) REFERENCES Owner(OwnerId)
);

-- =====================================================
-- CONTRIBUTION TYPES (DYNAMIC)
-- =====================================================
CREATE TABLE ContributionType (
    ContributionTypeId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Code TEXT,
    IsPercentage INTEGER DEFAULT 1,
    Description TEXT,
    IsActive INTEGER DEFAULT 1
);

-- =====================================================
-- OWNER CONTRIBUTION (HEADER)
-- =====================================================
CREATE TABLE OwnerContribution (
    OwnerContributionId INTEGER PRIMARY KEY AUTOINCREMENT,
    OwnerId INTEGER NOT NULL,
    TotalOriginalArea REAL NOT NULL,
    TotalNetArea REAL,
    Remarks TEXT,
    FOREIGN KEY (OwnerId) REFERENCES Owner(OwnerId)
);

-- =====================================================
-- CONTRIBUTION DETAILS (DYNAMIC VALUES)
-- =====================================================
CREATE TABLE ContributionDetail (
    ContributionDetailId INTEGER PRIMARY KEY AUTOINCREMENT,
    OwnerContributionId INTEGER NOT NULL,
    ContributionTypeId INTEGER NOT NULL,
    Value REAL NOT NULL,
    CalculatedArea REAL,
    FOREIGN KEY (OwnerContributionId) REFERENCES OwnerContribution(OwnerContributionId),
    FOREIGN KEY (ContributionTypeId) REFERENCES ContributionType(ContributionTypeId)
);

-- =====================================================
-- ROADS
-- =====================================================
CREATE TABLE Road (
    RoadId INTEGER PRIMARY KEY AUTOINCREMENT,
    RoadName TEXT,
    Width REAL,
    GeometryWKT TEXT NOT NULL,
    RoadType TEXT,
    LayerId INTEGER,
    IsActive INTEGER DEFAULT 1,
    FOREIGN KEY (LayerId) REFERENCES Layer(LayerId)
);

-- =====================================================
-- OPEN SPACES / PUBLIC LAND
-- =====================================================
CREATE TABLE OpenSpace (
    OpenSpaceId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT,
    GeometryWKT TEXT NOT NULL,
    Area REAL,
    Purpose TEXT,
    LayerId INTEGER,
    FOREIGN KEY (LayerId) REFERENCES Layer(LayerId)
);

-- =====================================================
-- BLOCKS
-- =====================================================
CREATE TABLE Block (
    BlockId INTEGER PRIMARY KEY AUTOINCREMENT,
    BlockName TEXT,
    GeometryWKT TEXT NOT NULL,
    Area REAL,
    LayerId INTEGER,
    FOREIGN KEY (LayerId) REFERENCES Layer(LayerId)
);

-- =====================================================
-- REPLOTTED PARCELS
-- =====================================================
CREATE TABLE ReplotParcel (
    ReplotParcelId INTEGER PRIMARY KEY AUTOINCREMENT,
    PlotNo TEXT,
    BlockId INTEGER,
    GeometryWKT TEXT NOT NULL,
    Area REAL,
    RoadAccess INTEGER DEFAULT 1,
    IsAssigned INTEGER DEFAULT 0,
    LayerId INTEGER,
    FOREIGN KEY (BlockId) REFERENCES Block(BlockId),
    FOREIGN KEY (LayerId) REFERENCES Layer(LayerId)
);

-- =====================================================
-- PLOT ASSIGNMENT
-- =====================================================
CREATE TABLE PlotAssignment (
    AssignmentId INTEGER PRIMARY KEY AUTOINCREMENT,
    ReplotParcelId INTEGER NOT NULL,
    OwnerId INTEGER NOT NULL,
    AssignedArea REAL,
    FOREIGN KEY (ReplotParcelId) REFERENCES ReplotParcel(ReplotParcelId),
    FOREIGN KEY (OwnerId) REFERENCES Owner(OwnerId)
);

-- =====================================================
-- VALIDATION ISSUES
-- =====================================================
CREATE TABLE ValidationIssue (
    IssueId INTEGER PRIMARY KEY AUTOINCREMENT,
    IssueType TEXT,
    RelatedId INTEGER,
    Message TEXT,
    Severity TEXT,
    IsResolved INTEGER DEFAULT 0
);

-- =====================================================
-- IMPORT LOG
-- =====================================================
CREATE TABLE ImportLog (
    ImportId INTEGER PRIMARY KEY AUTOINCREMENT,
    FileName TEXT,
    FileType TEXT,
    ImportedOn TEXT DEFAULT CURRENT_TIMESTAMP,
    Notes TEXT
);

-- =====================================================
-- PROJECT SETTINGS (KEY-VALUE)
-- =====================================================
CREATE TABLE ProjectSettings (
    Key TEXT PRIMARY KEY,
    Value TEXT
);

-- =====================================================
-- INDEXES (PERFORMANCE)
-- =====================================================
CREATE INDEX idx_owner_name ON Owner(FullName);
CREATE INDEX idx_originalparcel_owner ON OriginalParcel(OwnerId);
CREATE INDEX idx_replot_block ON ReplotParcel(BlockId);
CREATE INDEX idx_assignment_owner ON PlotAssignment(OwnerId);
CREATE INDEX idx_contribution_owner ON OwnerContribution(OwnerId);
CREATE INDEX idx_contribution_detail ON ContributionDetail(ContributionTypeId);
