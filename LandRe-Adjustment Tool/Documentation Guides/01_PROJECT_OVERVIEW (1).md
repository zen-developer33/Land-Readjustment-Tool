# RePlot — Land Readjustment & Replotting Tool
## Project Overview & Architecture Document

---

## 1. What Is This Application?

**RePlot** is a specialized desktop application for **Land Readjustment (Land Pooling)** projects in Nepal. It is NOT a generic CAD or GIS tool — it is purpose-built for the legal, administrative, and spatial workflow of reorganizing land parcels within a project boundary.

### Core Domain Problem

Land readjustment is a process where:
1. Original landowners contribute a portion of their land to the project
2. Roads, open spaces, and infrastructure are created from contributions
3. Each owner receives a smaller but better-located "replotted" parcel
4. The entire process must be documented, calculated, and legally traceable

### Who Uses It

- Town Planning Offices (Nepal government)
- Private consulting firms doing land pooling projects
- Survey engineers managing land parcel data

---

## 2. Project Architecture — High Level

```
RePlot/
├── Core/                        ← Domain entities, interfaces (EF Core)
│   ├── Entities/
│   │   ├── Project/             ← ProjectInfo, ProjectSettings
│   │   ├── Import/              ← ImportSession, ImportedRawRecord, ValidationError
│   │   ├── LandData/            ← LandOwner, BaselineParcel, MalpotReference, ParcelFrontage
│   │   ├── Contribution/        ← ContributionCategory, ParcelContribution, Summary
│   │   ├── Replotting/          ← ReplottedParcel, Block, PlotType, OriginalToReplottedMap
│   │   ├── Canvas/              ← CanvasLayer, CanvasObject
│   │   ├── Layout/              ← Road, Block
│   │   └── Spatial/             ← CoordinateSystem, ProjectionParameters, DatumTransformation
│   └── Interfaces/              ← IRepository<T>, IProjectInfoService, etc.
│
├── Data/                        ← EF Core DbContext, session management
│   ├── AppDbContext.cs
│   ├── ProjectSession.cs
│   ├── ProjectSessionFactory.cs
│   ├── ProjectContext.cs
│   └── AppServices.cs           ← Static service locator
│
├── Repositories/                ← EF Core repository implementations
│   ├── Base/BaseRepository.cs
│   ├── Project/
│   └── Spatial/
│
├── Services/                    ← Business logic
│   ├── Import/
│   │   ├── ImportManagerService.cs
│   │   └── ImportPersistenceService.cs
│   ├── LandData/
│   │   └── LandRecordsService.cs
│   ├── Project/
│   ├── OwnerDeduplicationService.cs
│   ├── FuzzyMatchingService.cs
│   ├── DataTransformationService.cs
│   ├── AreaConverterService.cs
│   └── ExcelImportService.cs
│
├── Models/                      ← Legacy in-memory models (NOT EF entities)
│   ├── BaselineLandParceRecord.cs
│   ├── LandOwnerModels.cs
│   └── ProjectInfo.cs
│
├── UI/
│   ├── Forms/
│   │   ├── frmMain.cs/.Designer.cs
│   │   ├── Import Management/
│   │   ├── Land Owners Record/
│   │   └── Original Land Parcel Records/
│   └── CustomControls/
│       └── DrawingCanvasControl.cs   ← 2D Canvas (CRITICAL — needs rebuild)
│
└── Infrastructure/
    └── Logging/
        ├── IAppLogger.cs
        ├── FileLogger.cs
        ├── DebugLogger.cs
        └── CompositeLogger.cs
```

---

## 3. Technology Stack

| Component | Technology | Notes |
|---|---|---|
| Language | C# 12 / .NET 8 | Windows-only |
| UI Framework | Windows Forms | Not WPF — intentional for control |
| ORM | Entity Framework Core 8 | Code-first migrations |
| Database | SQLite (per-project file) | One `.lpp` file = one project |
| Spatial | NetTopologySuite | Geometry storage |
| Excel Import | ExcelDataReader | .xls and .xlsx |
| Logging | Custom (File + Debug) | IAppLogger interface |
| Coordinate Math | ProjNET | CRS transformations |

---

## 4. Project File Model (.lpp)

Each project is a **single SQLite database file** with `.lpp` extension.

- File path: `C:\Projects\Ward5\Ward5.lpp`
- Project folder: `C:\Projects\Ward5\`
- Contains: all project data, settings, canvas objects, land records
- Backup: `.lpp.bak` rotated on every Save
- WAL mode: enabled for performance, checkpointed on Save

**Folder structure created on new project:**
```
Ward5/
├── Ward5.lpp              ← Main database
├── Ward5.lpp.bak          ← Latest backup
├── Maps/
├── GIS/
├── Documents/
├── Reports/
├── Exports/Excel/
├── Images/LandOwners Certificate/
├── Images/Cadastral Sheets/
├── Images/Land Owners Photos/
├── Logs/
└── Temp/
```

---

## 5. Data Flow — Import to Save

```
Excel File
    │
    ▼
ExcelImportService.ReadExcelFileAsDataSet()
    │  (DataSet with all sheets)
    ▼
frmImportParcelOwnershipRecords
    │  Step 1: Browse & select sheet
    │  Step 2: Map fields (Auto-Map + manual)
    │  Step 3: Review & edit records
    │  Step 4: Validate → Deduplicate → Save
    ▼
DataTransformationService.TransformDataToEntities()
    │  (BaselineLandParceRecord list)
    ▼
OwnerDeduplicationService.ExtractUniqueOwners()
    │  High confidence → auto-merge
    │  Medium confidence → frmReviewDuplicates
    ▼
ImportPersistenceService.PersistImportAsync()
    │  Stage to tblImportedRawRecords (audit trail)
    │  Upsert owners → tblLandOwners
    │  Insert parcels → tblBaselineParcels
    ▼
SQLite Database (.lpp file)
```

---

## 6. Database Schema — Key Tables

```sql
tblProjectInfo          -- One row: project name, location, dates
tblProjectSettings      -- One row: CRS, area units, canvas settings

tblImportSessions       -- Each import run
tblImportedRawRecords   -- Raw rows as imported (audit trail)
tblValidationErrors     -- Errors per import session

tblLandOwners           -- Unique landowners (deduped)
tblMalpotReferences     -- Land registry references (Moth/Paana)
tblBaselineParcels      -- Original land parcels with geometry link
tblParcelFrontages      -- Which road each parcel faces

tblContributionCategories  -- Types of contributions (road, open space)
tblParcelContributions     -- Calculated contributions per parcel
tblParcelContributionSummaries  -- Summary per parcel

tblReplottedParcels     -- New parcels after replotting
tblReplottedParcelOwners -- Who owns replotted parcels (with % share)
tblOriginalToReplottedMaps -- Links original → replotted

tblCanvasLayers         -- Layer definitions
tblCanvasObjects        -- Geometry objects on canvas

tblRoads                -- Road entities
tblBlocks               -- Block (group of plots) entities
tblPlotTypes            -- Private, Sales, Government, etc.

tblCoordinateSystems    -- CRS master data (seeded)
tblProjectionParameters -- MUTM parameters
tblDatumTransformations -- Datum shift parameters (seeded)
```

---

## 7. Current Dual-Model Problem (IMPORTANT)

There are currently **two parallel model systems** that need to be unified:

### Legacy Models (Models/ folder)
- `LandOwner` — in-memory, not EF mapped (`[NotMapped]`)
- `OriginalLandParcel` — in-memory
- `BaselineLandParceRecord` — import/display model

### EF Core Entities (Core/Entities/ folder)
- `LandOwner` (Core.Entities.LandData) — EF mapped, `tblLandOwners`
- `BaselineParcel` — EF mapped, `tblBaselineParcels`

**LandRecordsService** bridges these — it reads EF entities and maps them to legacy models for UI consumption. This is the correct pattern (anti-corruption layer) but must be maintained carefully.

**Action Required:** Gradually eliminate legacy models. Forms should eventually work with DTOs/ViewModels rather than touching EF entities directly.
