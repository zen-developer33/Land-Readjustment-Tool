# RePlot — Implementation Roadmap
## Phased Development Plan with Checklists

---

## How to Use This Document

Each phase builds on the previous. Do NOT skip phases — the canvas and contribution engine
depend on solid data being in the database first. Each checklist item that is **bolded** is
a prerequisite for the next phase.

---

## Phase 0 — Code Cleanup (Do This First)
*Estimated: 1–2 days. This unblocks clean work in all future phases.*

### 0.1 Delete Dead Files
- [ ] Delete `LandRe-Adjustment Tool/UI/Forms/Land Owers Record.cs`
- [ ] Delete `LandRe-Adjustment Tool/UI/Forms/Land Owers Record.Designer.cs`
- [ ] Delete `LandRe-Adjustment Tool/Repositories/OriginalLandParcelsWithLandOwnersRepository.cs`
- [ ] Verify no project references break

### 0.2 Fix Typos (Breaking if Left)
- [ ] Rename `BaselineLandParceRecord` → `BaselineLandParcelRecord` (find+replace all usages)
- [ ] Fix `TempoaryAddress` → `TemporaryAddress` in model and all usages
- [ ] Fix `citizenshipIssuedDate` → `CitizenshipIssuedDate` (capital C)
- [ ] Fix file name `Land Owers Record` → `Land Owners Record`

### 0.3 Extract Shared Constants
- [ ] Create `Infrastructure/Constants/NepalDomainConstants.cs`
- [ ] Move `InstitutionKeywords[]` from 3 locations → single source
- [ ] Move `AnonymousKeywords[]` from `OwnerDeduplicationService`

### 0.4 Add Unhandled Exception Handler
- [ ] In `Program.cs`, add:
  ```csharp
  Application.ThreadException += (s, e) => LogAndShow(e.Exception);
  AppDomain.CurrentDomain.UnhandledException += (s, e) => LogAndShow(e.ExceptionObject);
  ```

### 0.5 Verify EF Core Migrations Exist
- [ ] Run `dotnet ef migrations list` — confirm migrations present
- [ ] If not: `dotnet ef migrations add InitialCreate`
- [ ] Test: create new project, open project, confirm schema created

---

## Phase 1 — Canvas Foundation (CRITICAL PATH)
*Estimated: 3–4 weeks. Everything else depends on this.*

> See `04_CANVAS_ARCHITECTURE.md` for full technical design.

### 1.1 Rendering Engine Decision
- [ ] **Choose rendering backend**: GDI+ (built-in, simpler) OR SkiaSharp (faster, better quality)
  - Recommendation: **SkiaSharp** via `SkiaSharp.Views.WindowsForms` NuGet
  - Reason: hardware accelerated, no flicker, excellent text rendering, used in production CAD tools
- [ ] Install NuGet: `SkiaSharp`, `SkiaSharp.Views.WindowsForms`

### 1.2 Coordinate System
- [ ] **World-to-Screen transformation class** (`ViewTransform`)
  - Pan offset (X, Y in world units)
  - Zoom level (pixels per world unit)
  - `WorldToScreen(PointF worldPt) → PointF`
  - `ScreenToWorld(PointF screenPt) → PointF`
  - `WorldToScreen(RectangleF worldRect) → RectangleF`
- [ ] Zoom to extents (fit all geometry in view)
- [ ] Zoom to rectangle (drag box)
- [ ] Zoom in/out at cursor position (not center)
- [ ] Minimum and maximum zoom limits

### 1.3 DrawingCanvasControl (SKControl subclass)
- [ ] Double-buffered paint via SkiaSharp `SKControl.PaintSurface`
- [ ] Mouse wheel → zoom
- [ ] Middle mouse button / Space+drag → pan
- [ ] Cursor changes per active tool
- [ ] Grid rendering (dotted or lines, fades at low zoom)
- [ ] Scale bar rendering
- [ ] Background color from project settings

### 1.4 Layer Rendering Pipeline
- [ ] `CanvasRenderer` class — iterates visible layers in DisplayOrder
- [ ] Per-layer style (color, line weight, fill)
- [ ] Render: polygons, polylines, points, text
- [ ] Selection highlight style (blue outline, semi-transparent fill)
- [ ] Hover highlight style

### 1.5 Geometry Loading
- [ ] Load `tblCanvasObjects` from DB into in-memory list on project open
- [ ] Convert NetTopologySuite geometry → SKPath for rendering
- [ ] Spatial index (R-tree or simple grid) for hit-testing at high zoom

### 1.6 Tool System
- [ ] `ICanvasTool` interface: `OnMouseDown`, `OnMouseMove`, `OnMouseUp`, `OnKeyDown`, `Cancel()`
- [ ] `PanTool` — middle mouse or Space key
- [ ] `SelectTool` — click select, box select, crossing select
- [ ] `ZoomWindowTool` — drag rectangle to zoom

---

## Phase 2 — DXF Import & Baseline Parcels on Canvas
*Estimated: 2–3 weeks. Requires Phase 1.*

### 2.1 DXF Import
- [ ] NuGet: `netDxf` (open source DXF reader for .NET)
- [ ] `DxfImportService` — read DXF entities → `CanvasObject` records
- [ ] Support: LWPOLYLINE, POLYLINE, LINE, ARC, TEXT, MTEXT
- [ ] Layer mapping: DXF layer name → `CanvasLayer`
- [ ] Store geometry in `tblCanvasObjects` (NetTopologySuite polygon)
- [ ] `frmDxfImport` — select file, map DXF layers to canvas layers

### 2.2 Link Parcels to Canvas Objects
- [ ] After DXF import, `frmLinkParcelsToGeometry` — match parcel numbers to polygons
- [ ] Auto-link by label/text entity inside polygon
- [ ] Manual link by selecting polygon → assign parcel
- [ ] Store `BaselineParcel.CanvasObjectId` → `tblCanvasObjects.Id`

### 2.3 Parcel Display
- [ ] Parcels rendered with fill color by land use
- [ ] Parcel number label rendered at polygon centroid
- [ ] Area label option
- [ ] Hover: show tooltip with owner name, area
- [ ] Click: select parcel, show details in right panel (`dgvParcelObjProperty`)

---

## Phase 3 — Contribution Calculation Engine
*Estimated: 2 weeks. Requires Phase 2 (parcel geometry needed for area calc).*

> See `05_CONTRIBUTION_ENGINE.md` for formulas.

### 3.1 Contribution Category Setup
- [ ] `frmContributionSetup` — manage `tblContributionCategories`
- [ ] Define: General % contributions (road, open space, infrastructure)
- [ ] Define: Specific contributions (corner plot, slope deduction)
- [ ] Rate types: Percentage, FixedArea, Formula

### 3.2 Effective Area Calculation
- [ ] `EffectiveAreaService.Calculate(BaselineParcel parcel)`
- [ ] Default: `EffectiveArea = OriginalArea`
- [ ] Deductions: existing road within parcel (from geometry intersection)
- [ ] Manual override flag

### 3.3 General Contribution
- [ ] `GeneralContributionService.Calculate(parcel, categories)`
- [ ] For each GeneralContribution category: `Amount = EffectiveArea × Rate`
- [ ] Cumulative (sum of all general contributions)

### 3.4 Specific Contribution Formulas
- [ ] Corner plot formula: `Amount = min(8m frontage area) × Rate`
- [ ] Slope formula: TBD per project requirements
- [ ] Custom formula plugin point

### 3.5 Net Returnable Area
- [ ] `NetReturn = EffectiveArea - TotalContribution + TotalDeductions`
- [ ] Store in `tblParcelContributionSummaries`

### 3.6 Contribution Review UI
- [ ] `frmContributionReview` — grid showing all parcels with contribution breakdown
- [ ] Color coding: high contribution (red), low (green)
- [ ] Manual override: right-click → override value → enter reason
- [ ] Recalculate all button

---

## Phase 4 — Replotting Workspace
*Estimated: 4–6 weeks. Requires Phase 1, 2, 3.*

### 4.1 Block Management
- [ ] Draw block boundary on canvas
- [ ] `frmBlockManager` — list, create, edit blocks
- [ ] Block depth setting (affects minimum frontage)
- [ ] Land use per block

### 4.2 Road Design
- [ ] Draw road centerline on canvas
- [ ] Set road width (carriageway + ROW)
- [ ] Road type (arterial/collector/local/lane)
- [ ] Roads linked to `tblRoads`
- [ ] ROW polygon auto-generated from centerline + width

### 4.3 Replotted Parcel Creation
- [ ] Draw polygon on canvas → creates `ReplottedParcel`
- [ ] Assign to block
- [ ] Assign plot type (Private/Sales/Government/OpenSpace/Community/Road)
- [ ] Auto-assign number by scheme (Sequential/BlockBased/Custom)
- [ ] Area calculated from drawn geometry

### 4.4 Parcel Splitting Tool
- [ ] Draw split line across parcel
- [ ] Splits geometry into two polygons
- [ ] Each fragment becomes separate `ReplottedParcel`
- [ ] Area recalculated

### 4.5 Original → Replotted Mapping
- [ ] `frmMapOriginalToReplotted` — for each replotted parcel, assign original parcels
- [ ] Calculate contributed area per original parcel
- [ ] Store in `tblOriginalToReplottedMaps`

### 4.6 Ownership Assignment
- [ ] From mapping: calculate `OwnershipSharePercent` per owner
- [ ] Joint ownership: split by area contribution ratio
- [ ] Store in `tblReplottedParcelOwners`

### 4.7 Validation Rules
- [ ] Minimum plot area (79.49 sqm default, configurable)
- [ ] Minimum frontage (per road type)
- [ ] No parcel overlap
- [ ] No unassigned original parcels
- [ ] Total area balance check

---

## Phase 5 — Reports & Export
*Estimated: 2 weeks. Requires Phase 3, 4.*

### 5.1 Land Owner Register
- [ ] Excel export of all owners + their parcels
- [ ] PDF export formatted for official submission

### 5.2 Contribution Statement
- [ ] Per-owner contribution breakdown
- [ ] Signature line for owner acknowledgment

### 5.3 Replotted Parcel Schedule
- [ ] New parcel number, area, owner, location

### 5.4 Comparison Report
- [ ] Side-by-side: original parcels vs. replotted parcels per owner

### 5.5 KML Export
- [ ] Replotted parcels → KML for Google Earth review

### 5.6 DXF Export
- [ ] Replotted layout → DXF for handover to survey office

### 5.7 Print Layout
- [ ] A3/A2 page with map, north arrow, scale bar, title block
- [ ] Print preview form

---

## Phase 6 — Polish & Production Readiness
*Estimated: 1–2 weeks.*

- [ ] Keyboard shortcuts (Del=delete, Escape=cancel, Ctrl+Z=undo)
- [ ] Status bar showing cursor coordinates in world units
- [ ] Properties panel (right panel) showing selected object attributes
- [ ] Undo/Redo system (Command pattern)
- [ ] Settings: auto-backup interval
- [ ] MSI installer
- [ ] `.lpp` file association in Windows registry (code exists in `Program.cs`)
- [ ] Update checker (optional)
