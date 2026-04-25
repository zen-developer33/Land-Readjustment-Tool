# RePlot — Implementation Status
## What Is Done, What Is Missing, What Needs Fixing

---

## ✅ COMPLETED — Solid Foundation

### Project Management
- [x] New project creation with folder structure
- [x] Open project (`.lpp` file association)
- [x] Save (WAL checkpoint + backup rotation)
- [x] Save As (copy folder, switch session)
- [x] Close project with unsaved-changes prompt
- [x] Recent projects menu
- [x] Restore from backup (`frmBackupManager`)
- [x] Project info form (`frm_ProjectDetails`)
- [x] Project settings form (`frmProjectSettings`)

### Database & Infrastructure
- [x] EF Core code-first with SQLite
- [x] `ProjectSession` — dependency injection for DB context + logger
- [x] `ProjectSessionFactory` — composition root
- [x] `AppServices` — static service locator (acceptable for WinForms)
- [x] `BaseRepository<T>` — generic CRUD with async
- [x] `ProjectInfoRepository`, `ProjectSettingsRepository`
- [x] `DatumTransformationRepository`, `CoordinateSystemRepository`
- [x] Seed data: coordinate systems, datum transformations, plot types
- [x] `IAppLogger` interface with `FileLogger`, `DebugLogger`, `CompositeLogger`
- [x] WAL checkpoint on Save and Close
- [x] Rollback to backup on "discard changes" close

### Import Pipeline
- [x] `ExcelImportService` — reads `.xls`/`.xlsx` into DataSet
- [x] `frmImportParcelOwnershipRecords` — 4-step import wizard
- [x] Field mapping (auto-map with Nepali/English keyword matching)
- [x] `DataTransformationService` — Excel rows → `BaselineLandParceRecord`
- [x] Validation with error list and fix-in-place (`frmValidationErrors`)
- [x] `OwnerDeduplicationService` — sophisticated fuzzy deduplication
  - Levenshtein distance
  - Citizenship number matching (with Devanagari digit conversion)
  - Institution vs. person categorization
  - Auto-merge (high confidence) + manual review (medium confidence)
- [x] `frmReviewDuplicates` — side-by-side comparison, merge/keep decisions, undo
- [x] `frmUniqueOwnersPreview` — preview final unique owners before save
- [x] `ImportManagerService` — staging raw records (audit trail)
- [x] `ImportPersistenceService` — save to EF Core entities
- [x] Post-import processing: auto-calculate RAPD/BKD, auto-detect ownership type
- [x] Replace vs. append existing data handling

### Land Records Management
- [x] `frmLandOwnersRecord` — owner list with search
- [x] `frmLandOwnerDetails` — owner detail/edit with photo and documents
- [x] `frmOwnerDocuments` — attach, view, delete documents per owner
- [x] `frmOwnerParcels` — view all parcels owned
- [x] `frmLandParcelOwnersRecord` — parcel list with multi-layer filtering
  - Province/District/Municipality/Ward filter
  - Map sheet filter
  - Ownership type filter
  - Area range filter (sqm, Ropani, Aana)
  - Parcel No and owner name search
  - Quick filter toggle
- [x] `frmAddEditRecord` — add/edit parcel with duplicate check
- [x] `frmOwnerLookup` — owner search and select
- [x] `LandRecordsService` — all DB operations bridging legacy↔EF models
- [x] `AreaConverterService` — sqm ↔ RAPD ↔ BKD conversions
- [x] `frmAreaConverter` — standalone area conversion tool
- [x] `FuzzyMatchingService` — general-purpose string similarity

### Layer Manager
- [x] `frmLayerManager` — full layer property editing
  - Visibility, lock, printable toggles
  - Color swatches (inline grid + right panel)
  - Line style, weight, fill, hatch, labels
  - Move up/down, show/hide all, lock all
  - Search/filter

### Spatial Configuration
- [x] Coordinate system master data (UTM44N, UTM45N, MUTM81/84/87)
- [x] Projection parameters for MUTM zones (Everest 1830 ellipsoid)
- [x] Datum transformation parameters (Nagarkot, Kalianpur, Survey Dept official)

---

## ❌ NOT STARTED — Critical Missing Pieces

### 2D Drawing Canvas (MOST CRITICAL)
- [ ] `DrawingCanvasControl` — only a shell exists, NO implementation
- [ ] Hardware-accelerated rendering (GDI+ or SkiaSharp)
- [ ] Pan and zoom with no flicker
- [ ] Coordinate system (world ↔ screen transformation)
- [ ] Layer rendering pipeline
- [ ] Parcel polygon display
- [ ] Selection (click, box select, crossing select)
- [ ] Snap system (endpoint, midpoint, intersection, perpendicular)
- [ ] DXF/DWG import → canvas objects
- [ ] Geometry editing (move, rotate, scale, trim, extend)
- [ ] Parcel splitting and merging
- [ ] Road centerline and ROW drawing
- [ ] Block boundary drawing
- [ ] Annotation tools (text, dimension, leader)

### Contribution Calculation
- [ ] `ContributionCalculationService`
- [ ] General contribution (% of effective area)
- [ ] Road contribution formula
- [ ] Corner plot formula
- [ ] Specific contribution (manual allocation)
- [ ] Effective area calculation (account for slope, existing roads)
- [ ] `frmContributionSetup` — configure categories and rates
- [ ] `frmContributionReview` — review per-parcel calculations
- [ ] Manual override with reason logging

### Replotting Workspace
- [ ] Block creation and management
- [ ] Replotted parcel creation (draw on canvas)
- [ ] Parcel number assignment (Sequential / BlockBased / Custom)
- [ ] Original → Replotted mapping
- [ ] Ownership share calculation
- [ ] Minimum plot area validation (79.49 sqm default)
- [ ] `frmReplotWorkspace` — main replotting interface

### Reports & Output
- [ ] Land owner register (Excel/PDF)
- [ ] Parcel contribution statement
- [ ] Replotted parcel schedule
- [ ] Comparison report (original vs. replotted)
- [ ] KML export for Google Earth
- [ ] DXF export of replotted layout
- [ ] Print layout with scale bar and north arrow

### Validation Engine
- [ ] Area balance check (total original = total replotted + public land)
- [ ] Owner completeness check
- [ ] Geometry topology check (no overlaps, no gaps)
- [ ] Contribution percentage compliance check

---

## ⚠️ PARTIALLY DONE — Needs Completion or Refactoring

### frmMain Canvas Area
- Canvas placeholder (`mainSplitContainer`) is wired but `DrawingCanvasControl` has no rendering
- Toolbar buttons (Pan, Zoom In/Out, Zoom Extent, Undo/Redo) are UI-only, no handlers

### AppServices (Service Locator)
- Works but is a static service locator — acceptable for WinForms but limits testability
- Needs: null-safety checks are present, good
- Missing: no way to inject mock services for testing

### Error Handling
- Most services use try/catch with MessageBox — acceptable for desktop
- Missing: centralized error handling / unhandled exception handler in `Program.cs`

### Geometry Storage
- `tblCanvasObjects` has `Geometry Shape` (NetTopologySuite) column defined
- But NO canvas objects are ever created or read — completely unused

### Migration Strategy
- `AppDbContext` has full EF Core setup with `MigrateAsync()` called on open
- Seed data is in `OnModelCreating` — correct
- BUT: no migration files shown — need to verify these exist

---

## 🔴 CODE QUALITY ISSUES TO FIX

### 1. Commented-Out Code (Dead Code)
Files with entire implementations commented out:
- `Land Owers Record.cs` — 300+ lines commented
- `Land Owers Record.Designer.cs` — entire designer commented
- `OriginalLandParcelsWithLandOwnersRepository.cs` — entire file commented
- `Land Owners Record.Designer.cs` — entire file commented

**Action:** Delete these files. They are replaced by newer implementations.

### 2. Naming Inconsistencies
- `BaselineLandParceRecord` — typo (`Parce` should be `Parcel`)
- `TempoaryAddress` — typo (`Tempoary` should be `Temporary`)
- `citizenshipIssuedDate` — lowercase first letter breaks convention
- `frmLandownersRecord` vs `frmLandOwnersRecord` — two different forms
- `Land Owers Record` — file named with typo

### 3. Duplicate `LandOwner` Class
- `Land_Readjustment_Tool.Models.LandOwner` (Models/LandOwnerModels.cs)
- `Land_Readjustment_Tool.Core.Entities.LandData.LandOwner` (Core/Entities)
- This causes `using` aliases in `LandRecordsService.cs`:
  ```csharp
  using LegacyLandOwner = Land_Readjustment_Tool.Models.LandOwner;
  using CoreLandOwner = Land_Readjustment_Tool.Core.Entities.LandData.LandOwner;
  ```
- **Action:** Eliminate the Models.LandOwner once all forms use LandRecordsService

### 4. Missing Null-Safety
- `record.TempoaryAddress` (typo) used in multiple places
- Many `?.ToString()` calls could use pattern matching

### 5. InstitutionKeywords Duplication
- Defined separately in: `OwnerDeduplicationService`, `ImportPersistenceService`, `LandRecordsService`
- **Action:** Move to a shared `DomainConstants.cs` or `NepalDomainData.cs`

### 6. `frmLandOwnerDetails` Has Dead Event Handler
- `btnEdit_Click` and `chkEdit_CheckedChanged` both try to enable editing — confusing
- `private Button btnEdit` declared in designer but `chkEdit` (CheckBox styled as Button) is used instead

### 7. `DataTransformationService` Not Shown
- Referenced throughout but source not provided — likely has its own issues
