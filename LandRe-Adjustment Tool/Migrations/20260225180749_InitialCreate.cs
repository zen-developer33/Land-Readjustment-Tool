using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Land_Readjustment_Tool.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Sqlite:InitSpatialMetaData", true);

            migrationBuilder.CreateTable(
                name: "tblCanvasLayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LayerType = table.Column<string>(type: "TEXT", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSelectable = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPrintable = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    BorderColor = table.Column<string>(type: "TEXT", nullable: false),
                    LineWeight = table.Column<double>(type: "REAL", nullable: false),
                    LineStyle = table.Column<string>(type: "TEXT", nullable: false),
                    FillColor = table.Column<string>(type: "TEXT", nullable: true),
                    FillTransparency = table.Column<int>(type: "INTEGER", nullable: false),
                    FillStyle = table.Column<string>(type: "TEXT", nullable: false),
                    HatchPattern = table.Column<string>(type: "TEXT", nullable: true),
                    ShowLabels = table.Column<bool>(type: "INTEGER", nullable: false),
                    LabelFontName = table.Column<string>(type: "TEXT", nullable: true),
                    LabelFontSize = table.Column<double>(type: "REAL", nullable: false),
                    LabelColor = table.Column<string>(type: "TEXT", nullable: false),
                    LabelField = table.Column<string>(type: "TEXT", nullable: true),
                    PointSymbol = table.Column<string>(type: "TEXT", nullable: false),
                    PointSize = table.Column<double>(type: "REAL", nullable: false),
                    SourceFile = table.Column<string>(type: "TEXT", nullable: true),
                    ImportedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCanvasLayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblContributionCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", nullable: false),
                    ContributionType = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeduction = table.Column<bool>(type: "INTEGER", nullable: false),
                    RateType = table.Column<string>(type: "TEXT", nullable: false),
                    Rate = table.Column<double>(type: "REAL", nullable: true),
                    FormulaType = table.Column<string>(type: "TEXT", nullable: true),
                    ApplicableAreaRule = table.Column<string>(type: "TEXT", nullable: true),
                    ApplicableAreaLimit = table.Column<double>(type: "REAL", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblContributionCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblImportSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceFileName = table.Column<string>(type: "TEXT", nullable: false),
                    SourceFilePath = table.Column<string>(type: "TEXT", nullable: true),
                    ImportDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalRowsInFile = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRowsImported = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRowsInvalid = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReplaced = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReplacedBySessionID = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblImportSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblLandOwners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    FatherOrSpouseName = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    CitizenshipNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CitizenshipIssueDistrict = table.Column<string>(type: "TEXT", nullable: true),
                    CitizenshipIssueDate = table.Column<string>(type: "TEXT", nullable: true),
                    PermanentAddress = table.Column<string>(type: "TEXT", nullable: true),
                    TemporaryAddress = table.Column<string>(type: "TEXT", nullable: true),
                    ContactNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    PhotoPath = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentsFolderPath = table.Column<string>(type: "TEXT", nullable: true),
                    IdentificationMethod = table.Column<string>(type: "TEXT", nullable: false),
                    MatchConfidenceScore = table.Column<double>(type: "REAL", nullable: true),
                    NeedsManualReview = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblLandOwners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblPlotTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TypeName = table.Column<string>(type: "TEXT", nullable: false),
                    TypeCode = table.Column<string>(type: "TEXT", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPlotTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblProjectInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectName = table.Column<string>(type: "TEXT", nullable: false),
                    Province = table.Column<string>(type: "TEXT", nullable: true),
                    District = table.Column<string>(type: "TEXT", nullable: true),
                    Municipality = table.Column<string>(type: "TEXT", nullable: true),
                    WardNo = table.Column<string>(type: "TEXT", nullable: true),
                    ProjectSite = table.Column<string>(type: "TEXT", nullable: true),
                    ImplementingAgency = table.Column<string>(type: "TEXT", nullable: true),
                    ConsultingAgency = table.Column<string>(type: "TEXT", nullable: true),
                    GazetteNotificationNumber = table.Column<string>(type: "TEXT", nullable: true),
                    GazzeteDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProjectStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProjectEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProjectNotes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblProjectInfo", x => x.Id);
                    table.CheckConstraint("CK_ProjectInfo_SingleRow", "Id = 1");
                });

            migrationBuilder.CreateTable(
                name: "tblCanvasObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CanvasLayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ObjectType = table.Column<string>(type: "TEXT", nullable: false),
                    Shape = table.Column<Geometry>(type: "GEOMETRY", nullable: false),
                    BorderColorOverride = table.Column<string>(type: "TEXT", nullable: true),
                    FillColorOverride = table.Column<string>(type: "TEXT", nullable: true),
                    FillTransparencyOverride = table.Column<int>(type: "INTEGER", nullable: true),
                    LineWeightOverride = table.Column<double>(type: "REAL", nullable: true),
                    LineStyleOverride = table.Column<string>(type: "TEXT", nullable: true),
                    LabelText = table.Column<string>(type: "TEXT", nullable: true),
                    ObjectDescription = table.Column<string>(type: "TEXT", nullable: true),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    BaselineParcelId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReplottedParcelId = table.Column<int>(type: "INTEGER", nullable: true),
                    RoadId = table.Column<int>(type: "INTEGER", nullable: true),
                    BlockId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceDxfHandle = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCanvasObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblCanvasObjects_tblCanvasLayers_CanvasLayerId",
                        column: x => x.CanvasLayerId,
                        principalTable: "tblCanvasLayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblCitizenshipConflicts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    CitizenshipNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ConflictType = table.Column<string>(type: "TEXT", nullable: false),
                    Resolution = table.Column<string>(type: "TEXT", nullable: true),
                    IsResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCitizenshipConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblCitizenshipConflicts_tblImportSessions_ImportSessionId",
                        column: x => x.ImportSessionId,
                        principalTable: "tblImportSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblImportedRawRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    MapSheetNo = table.Column<string>(type: "TEXT", nullable: true),
                    ParcelNo = table.Column<string>(type: "TEXT", nullable: true),
                    Province = table.Column<string>(type: "TEXT", nullable: true),
                    District = table.Column<string>(type: "TEXT", nullable: true),
                    Municipality = table.Column<string>(type: "TEXT", nullable: true),
                    WardNo = table.Column<string>(type: "TEXT", nullable: true),
                    MothNo = table.Column<string>(type: "TEXT", nullable: true),
                    PaanaNo = table.Column<string>(type: "TEXT", nullable: true),
                    LandUse = table.Column<string>(type: "TEXT", nullable: true),
                    AreaSqm = table.Column<double>(type: "REAL", nullable: true),
                    AreaRAPD = table.Column<string>(type: "TEXT", nullable: true),
                    AreaBKD = table.Column<string>(type: "TEXT", nullable: true),
                    OwnerName = table.Column<string>(type: "TEXT", nullable: true),
                    FatherSpouseName = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    CitizenshipNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CitizenshipDistrict = table.Column<string>(type: "TEXT", nullable: true),
                    CitizenshipDate = table.Column<string>(type: "TEXT", nullable: true),
                    PermanentAddress = table.Column<string>(type: "TEXT", nullable: true),
                    TemporaryAddress = table.Column<string>(type: "TEXT", nullable: true),
                    ContactNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    IsTenant = table.Column<bool>(type: "INTEGER", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    IsValid = table.Column<bool>(type: "INTEGER", nullable: false),
                    RawRowData = table.Column<string>(type: "TEXT", nullable: true),
                    DeduplicatedToOwnerId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeduplicationMethod = table.Column<string>(type: "TEXT", nullable: true),
                    DeduplicationConfidence = table.Column<double>(type: "REAL", nullable: true),
                    WasManuallyReviewed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ManualReviewDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblImportedRawRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblImportedRawRecords_tblImportSessions_ImportSessionId",
                        column: x => x.ImportSessionId,
                        principalTable: "tblImportSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblMalpotReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LandOwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    MothNo = table.Column<string>(type: "TEXT", nullable: false),
                    PaanaNo = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblMalpotReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblMalpotReferences_tblLandOwners_LandOwnerId",
                        column: x => x.LandOwnerId,
                        principalTable: "tblLandOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockName = table.Column<string>(type: "TEXT", nullable: false),
                    BlockCode = table.Column<string>(type: "TEXT", nullable: true),
                    BlockDepth = table.Column<float>(type: "REAL", nullable: false),
                    BlockLandUse = table.Column<string>(type: "TEXT", nullable: true),
                    BlockArea = table.Column<double>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CanvasObjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblBlocks_tblCanvasObjects_CanvasObjectId",
                        column: x => x.CanvasObjectId,
                        principalTable: "tblCanvasObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tblRoads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoadName = table.Column<string>(type: "TEXT", nullable: false),
                    RoadCode = table.Column<string>(type: "TEXT", nullable: true),
                    RoadStatus = table.Column<string>(type: "TEXT", nullable: false),
                    SurfaceType = table.Column<string>(type: "TEXT", nullable: true),
                    RoadWidth = table.Column<double>(type: "REAL", nullable: false),
                    RightOfWayWidth = table.Column<double>(type: "REAL", nullable: true),
                    RoadType = table.Column<string>(type: "TEXT", nullable: true),
                    CanvasObjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblRoads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblRoads_tblCanvasObjects_CanvasObjectId",
                        column: x => x.CanvasObjectId,
                        principalTable: "tblCanvasObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tblCitizenshipConflictRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CitizenshipConflictId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportedRawRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMarkedCorrect = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCitizenshipConflictRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblCitizenshipConflictRecords_tblCitizenshipConflicts_CitizenshipConflictId",
                        column: x => x.CitizenshipConflictId,
                        principalTable: "tblCitizenshipConflicts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblCitizenshipConflictRecords_tblImportedRawRecords_ImportedRawRecordId",
                        column: x => x.ImportedRawRecordId,
                        principalTable: "tblImportedRawRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblValidationErrors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportedRawRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorType = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: false),
                    IsResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblValidationErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblValidationErrors_tblImportSessions_ImportSessionId",
                        column: x => x.ImportSessionId,
                        principalTable: "tblImportSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblValidationErrors_tblImportedRawRecords_ImportedRawRecordId",
                        column: x => x.ImportedRawRecordId,
                        principalTable: "tblImportedRawRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblBaselineParcels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    LandOwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    MalpotReferenceId = table.Column<int>(type: "INTEGER", nullable: true),
                    MapSheetNo = table.Column<string>(type: "TEXT", nullable: false),
                    ParcelNo = table.Column<string>(type: "TEXT", nullable: false),
                    FullUniqueParcelCode = table.Column<string>(type: "TEXT", nullable: false),
                    Province = table.Column<string>(type: "TEXT", nullable: true),
                    District = table.Column<string>(type: "TEXT", nullable: true),
                    Municipality = table.Column<string>(type: "TEXT", nullable: true),
                    WardNo = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalAreaSqm = table.Column<double>(type: "REAL", nullable: false),
                    EffectiveAreaSqm = table.Column<double>(type: "REAL", nullable: true),
                    IsEffectiveAreaManual = table.Column<bool>(type: "INTEGER", nullable: false),
                    LandUse = table.Column<string>(type: "TEXT", nullable: true),
                    HasTenant = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantName = table.Column<string>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    CanvasObjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBaselineParcels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblBaselineParcels_tblCanvasObjects_CanvasObjectId",
                        column: x => x.CanvasObjectId,
                        principalTable: "tblCanvasObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblBaselineParcels_tblImportSessions_ImportSessionId",
                        column: x => x.ImportSessionId,
                        principalTable: "tblImportSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblBaselineParcels_tblLandOwners_LandOwnerId",
                        column: x => x.LandOwnerId,
                        principalTable: "tblLandOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblBaselineParcels_tblMalpotReferences_MalpotReferenceId",
                        column: x => x.MalpotReferenceId,
                        principalTable: "tblMalpotReferences",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tblReplottedParcels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlotTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    SystemGeneratedNumber = table.Column<string>(type: "TEXT", nullable: true),
                    DerivedNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BlockSequenceNumber = table.Column<string>(type: "TEXT", nullable: true),
                    ActiveNumberType = table.Column<string>(type: "TEXT", nullable: false),
                    PlotAreaSqm = table.Column<double>(type: "REAL", nullable: false),
                    CanvasObjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblReplottedParcels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblReplottedParcels_tblBlocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "tblBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblReplottedParcels_tblCanvasObjects_CanvasObjectId",
                        column: x => x.CanvasObjectId,
                        principalTable: "tblCanvasObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblReplottedParcels_tblPlotTypes_PlotTypeId",
                        column: x => x.PlotTypeId,
                        principalTable: "tblPlotTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblParcelContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaselineParcelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContributionCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicableAreaSqm = table.Column<double>(type: "REAL", nullable: false),
                    RateApplied = table.Column<double>(type: "REAL", nullable: false),
                    ContributionAmountSqm = table.Column<double>(type: "REAL", nullable: false),
                    IsManualOverride = table.Column<bool>(type: "INTEGER", nullable: false),
                    ManualOverrideValueSqm = table.Column<double>(type: "REAL", nullable: true),
                    ManualOverrideReason = table.Column<string>(type: "TEXT", nullable: true),
                    CalculatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblParcelContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblParcelContributions_tblBaselineParcels_BaselineParcelId",
                        column: x => x.BaselineParcelId,
                        principalTable: "tblBaselineParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblParcelContributions_tblContributionCategories_ContributionCategoryId",
                        column: x => x.ContributionCategoryId,
                        principalTable: "tblContributionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblOriginalToReplottedMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaselineParcelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReplottedParcelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContributedAreaSqm = table.Column<double>(type: "REAL", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblOriginalToReplottedMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblOriginalToReplottedMaps_tblBaselineParcels_BaselineParcelId",
                        column: x => x.BaselineParcelId,
                        principalTable: "tblBaselineParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblOriginalToReplottedMaps_tblReplottedParcels_ReplottedParcelId",
                        column: x => x.ReplottedParcelId,
                        principalTable: "tblReplottedParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblParcelContributionSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaselineParcelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReplottedParcelId = table.Column<int>(type: "INTEGER", nullable: true),
                    OriginalAreaSqm = table.Column<double>(type: "REAL", nullable: false),
                    EffectiveAreaSqm = table.Column<double>(type: "REAL", nullable: false),
                    TotalGeneralContributionSqm = table.Column<double>(type: "REAL", nullable: false),
                    TotalSpecificContributionSqm = table.Column<double>(type: "REAL", nullable: false),
                    TotalDeductionSqm = table.Column<double>(type: "REAL", nullable: false),
                    TotalContributionSqm = table.Column<double>(type: "REAL", nullable: false),
                    TotalContributionPercent = table.Column<double>(type: "REAL", nullable: false),
                    NetReturnableAreaSqm = table.Column<double>(type: "REAL", nullable: false),
                    ReplottedAreaAssignedSqm = table.Column<double>(type: "REAL", nullable: true),
                    AreaDifferenceSqm = table.Column<double>(type: "REAL", nullable: true),
                    CashCompensationAmount = table.Column<double>(type: "REAL", nullable: true),
                    IsFinalized = table.Column<bool>(type: "INTEGER", nullable: false),
                    FinalizedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastCalculatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblParcelContributionSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblParcelContributionSummaries_tblBaselineParcels_BaselineParcelId",
                        column: x => x.BaselineParcelId,
                        principalTable: "tblBaselineParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblParcelContributionSummaries_tblReplottedParcels_ReplottedParcelId",
                        column: x => x.ReplottedParcelId,
                        principalTable: "tblReplottedParcels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tblParcelFrontages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaselineParcelId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReplottedParcelId = table.Column<int>(type: "INTEGER", nullable: true),
                    RoadId = table.Column<int>(type: "INTEGER", nullable: false),
                    FacingDirection = table.Column<string>(type: "TEXT", nullable: false),
                    FrontageLength = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblParcelFrontages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblParcelFrontages_tblBaselineParcels_BaselineParcelId",
                        column: x => x.BaselineParcelId,
                        principalTable: "tblBaselineParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblParcelFrontages_tblReplottedParcels_ReplottedParcelId",
                        column: x => x.ReplottedParcelId,
                        principalTable: "tblReplottedParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblParcelFrontages_tblRoads_RoadId",
                        column: x => x.RoadId,
                        principalTable: "tblRoads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblReplottedParcelOwners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReplottedParcelId = table.Column<int>(type: "INTEGER", nullable: false),
                    LandOwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnershipSharePercent = table.Column<double>(type: "REAL", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblReplottedParcelOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblReplottedParcelOwners_tblLandOwners_LandOwnerId",
                        column: x => x.LandOwnerId,
                        principalTable: "tblLandOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblReplottedParcelOwners_tblReplottedParcels_ReplottedParcelId",
                        column: x => x.ReplottedParcelId,
                        principalTable: "tblReplottedParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblBaselineParcels_CanvasObjectId",
                table: "tblBaselineParcels",
                column: "CanvasObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblBaselineParcels_FullUniqueParcelCode",
                table: "tblBaselineParcels",
                column: "FullUniqueParcelCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblBaselineParcels_ImportSessionId",
                table: "tblBaselineParcels",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblBaselineParcels_LandOwnerId",
                table: "tblBaselineParcels",
                column: "LandOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_tblBaselineParcels_MalpotReferenceId",
                table: "tblBaselineParcels",
                column: "MalpotReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_tblBlocks_CanvasObjectId",
                table: "tblBlocks",
                column: "CanvasObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblCanvasObjects_CanvasLayerId",
                table: "tblCanvasObjects",
                column: "CanvasLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_tblCitizenshipConflictRecords_CitizenshipConflictId",
                table: "tblCitizenshipConflictRecords",
                column: "CitizenshipConflictId");

            migrationBuilder.CreateIndex(
                name: "IX_tblCitizenshipConflictRecords_ImportedRawRecordId",
                table: "tblCitizenshipConflictRecords",
                column: "ImportedRawRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_tblCitizenshipConflicts_ImportSessionId",
                table: "tblCitizenshipConflicts",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblImportedRawRecords_ImportSessionId",
                table: "tblImportedRawRecords",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblLandOwners_CitizenshipNumber",
                table: "tblLandOwners",
                column: "CitizenshipNumber",
                unique: true,
                filter: "CitizenshipNumber IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tblMalpotReferences_LandOwnerId_MothNo",
                table: "tblMalpotReferences",
                columns: new[] { "LandOwnerId", "MothNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOriginalToReplottedMaps_BaselineParcelId",
                table: "tblOriginalToReplottedMaps",
                column: "BaselineParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_tblOriginalToReplottedMaps_ReplottedParcelId",
                table: "tblOriginalToReplottedMaps",
                column: "ReplottedParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_tblParcelContributions_BaselineParcelId",
                table: "tblParcelContributions",
                column: "BaselineParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_tblParcelContributions_ContributionCategoryId",
                table: "tblParcelContributions",
                column: "ContributionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tblParcelContributionSummaries_BaselineParcelId",
                table: "tblParcelContributionSummaries",
                column: "BaselineParcelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblParcelContributionSummaries_ReplottedParcelId",
                table: "tblParcelContributionSummaries",
                column: "ReplottedParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_tblParcelFrontages_BaselineParcelId",
                table: "tblParcelFrontages",
                column: "BaselineParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_tblParcelFrontages_ReplottedParcelId",
                table: "tblParcelFrontages",
                column: "ReplottedParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_tblParcelFrontages_RoadId",
                table: "tblParcelFrontages",
                column: "RoadId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlotTypes_TypeCode",
                table: "tblPlotTypes",
                column: "TypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblReplottedParcelOwners_LandOwnerId",
                table: "tblReplottedParcelOwners",
                column: "LandOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_tblReplottedParcelOwners_ReplottedParcelId",
                table: "tblReplottedParcelOwners",
                column: "ReplottedParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_tblReplottedParcels_BlockId",
                table: "tblReplottedParcels",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_tblReplottedParcels_CanvasObjectId",
                table: "tblReplottedParcels",
                column: "CanvasObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblReplottedParcels_PlotTypeId",
                table: "tblReplottedParcels",
                column: "PlotTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_tblRoads_CanvasObjectId",
                table: "tblRoads",
                column: "CanvasObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblValidationErrors_ImportedRawRecordId",
                table: "tblValidationErrors",
                column: "ImportedRawRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_tblValidationErrors_ImportSessionId",
                table: "tblValidationErrors",
                column: "ImportSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblCitizenshipConflictRecords");

            migrationBuilder.DropTable(
                name: "tblOriginalToReplottedMaps");

            migrationBuilder.DropTable(
                name: "tblParcelContributions");

            migrationBuilder.DropTable(
                name: "tblParcelContributionSummaries");

            migrationBuilder.DropTable(
                name: "tblParcelFrontages");

            migrationBuilder.DropTable(
                name: "tblProjectInfo");

            migrationBuilder.DropTable(
                name: "tblReplottedParcelOwners");

            migrationBuilder.DropTable(
                name: "tblValidationErrors");

            migrationBuilder.DropTable(
                name: "tblCitizenshipConflicts");

            migrationBuilder.DropTable(
                name: "tblContributionCategories");

            migrationBuilder.DropTable(
                name: "tblBaselineParcels");

            migrationBuilder.DropTable(
                name: "tblRoads");

            migrationBuilder.DropTable(
                name: "tblReplottedParcels");

            migrationBuilder.DropTable(
                name: "tblImportedRawRecords");

            migrationBuilder.DropTable(
                name: "tblMalpotReferences");

            migrationBuilder.DropTable(
                name: "tblBlocks");

            migrationBuilder.DropTable(
                name: "tblPlotTypes");

            migrationBuilder.DropTable(
                name: "tblImportSessions");

            migrationBuilder.DropTable(
                name: "tblLandOwners");

            migrationBuilder.DropTable(
                name: "tblCanvasObjects");

            migrationBuilder.DropTable(
                name: "tblCanvasLayers");
        }
    }
}
