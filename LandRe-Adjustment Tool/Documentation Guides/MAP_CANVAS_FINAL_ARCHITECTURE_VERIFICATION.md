# Map Canvas Final Architecture Verification

Date: 2026-04-27  
Scope: Verify the proposed "robust final map canvas" architecture against the current RePlot codebase and entity model.

## 1. Verdict

The proposed architecture is directionally correct, but it needs project-specific adjustments before implementation.

- Correct and should be kept:
  - Layered canvas architecture (engine + renderer + layer manager + tools)
  - Spatial indexing for viewport queries
  - Command-based editing operations (undo/redo)
  - Deferred rendering and cache invalidation strategy
- Must be adjusted for this codebase:
  - Do not add a separate geometry persistence model; use existing `CanvasObject` as the authoritative feature store
  - Do not create duplicate coordinate engines; standardize on `MapCanvasEngine` for the new map canvas path
  - Do not build a generic `IShape` database repository first; bridge runtime shapes to EF entities (`CanvasObject`, `CanvasLayer`) first

## 2. What Was Verified in Current Code

Verified entity anchors already present:

- `Core/Entities/Canvas/CanvasLayer.cs`
- `Core/Entities/Canvas/CanvasObject.cs` (`Geometry Shape` via NetTopologySuite)
- `Core/Entities/LandData/BaselineParcel.cs` (`CanvasObjectId`)
- `Core/Entities/Replotting/ReplottedParcel.cs` (`CanvasObjectId`)
- `Core/Entities/Layout/Road.cs` (`CanvasObjectId`)
- `Core/Entities/Layout/Block.cs` (`CanvasObjectId`)
- `Data/AppDbContext.cs` (`UseNetTopologySuite`, one-to-one entity links)

Verified technical gaps:

- `UI/MapCanvas/Data/ShapeRepository.cs` is still TODO placeholder
- Two viewport engines exist in code (`DrawingEngine`, `MapCanvasEngine`)
- Runtime canvas and persisted `CanvasObject` are not yet fully bridged
- Canvas services are not yet wired in `IProjectScopedFactory`

## 3. Entity-Model Fit (Application-Specific)

Use this mapping as the canonical implementation model:

- `CanvasLayer` = persisted styling, ordering, lock/visibility, labeling defaults
- `CanvasObject` = authoritative geometry feature (polygon/polyline/line/text/point)
- `BaselineParcel`, `ReplottedParcel`, `Road`, `Block` = domain records linked to features through `CanvasObjectId`
- `ProjectSettings` = canvas theme/grid/snap behavior defaults

This means the canvas should render projection models derived from `CanvasObject` + `CanvasLayer`, not from ad-hoc in-memory shape lists alone.

## 4. Required Data Additions (Minimal and High-Value)

These additions are recommended before final advanced tools:

1. `CanvasObject` additions:
   - `int? GeometrySrid`
   - `int GeometryRevisionNo`
   - `double? CalculatedAreaSqm`
   - `double? CalculatedPerimeterM`
   - `bool IsTopologyValid`
   - `string? TopologyMessage`
   - `DateTime? LastTopologyCheckUtc`
2. New table: `CanvasObjectRevision` (geometry history for undo audit and recovery)
3. New table: `TopologyIssue` (persisted overlap/gap/invalid geometry findings)
4. Constraint rule:
   - ensure a `CanvasObject` links to at most one domain target (`BaselineParcelId`, `ReplottedParcelId`, `RoadId`, `BlockId`)

## 5. Final Step-by-Step Implementation Plan

## Phase 0 - Consolidation (No New Features)

Goal: remove ambiguity.

- Keep `MapCanvasEngine` as the viewport engine for final map canvas
- Keep `DrawingEngine` only for legacy workspace until migrated
- Define one rendering entrypoint for final map canvas (`MapCanvasControl`)

Acceptance:

- All new map-canvas commands use `MapCanvasEngine` only
- No new feature is added on top of `DrawingEngine`

## Phase 1 - Persistence Bridge (Most Important)

Goal: connect runtime shapes to persisted features.

Create:

- `Core/Interfaces/ICanvasLayerRepository.cs`
- `Core/Interfaces/ICanvasObjectRepository.cs`
- `Repositories/Canvas/CanvasLayerRepository.cs`
- `Repositories/Canvas/CanvasObjectRepository.cs`
- `Services/Canvas/CanvasFeatureService.cs`
- `UI/MapCanvas/Services/GeometryShapeMapper.cs`

Responsibilities:

- Load visible features by viewport/layer
- Convert `CanvasObject.Shape` <-> runtime shape types
- Persist geometry edits transactionally

Acceptance:

- Editing a geometry updates `tblCanvasObjects` and survives reopen

## Phase 2 - Runtime Layer Manager

Goal: make `CanvasLayer` operational in rendering.

Create:

- `UI/MapCanvas/Layers/RuntimeLayer.cs`
- `UI/MapCanvas/Layers/RuntimeLayerManager.cs`
- `UI/MapCanvas/Layers/LayerStyleResolver.cs`

Responsibilities:

- Resolve final style = layer defaults + object overrides
- Order layers by `DisplayOrder`
- Respect `IsVisible`, `IsLocked`, `IsSelectable`, `IsPrintable`

Acceptance:

- Toggling layer visibility/lock affects both render and selection behavior

## Phase 3 - Render Pipeline Hardening

Goal: stable high-performance rendering.

Implement in final map canvas path:

- Grid/background first
- Feature layers next
- Preview/snap/interaction overlay last
- Deferred bitmap cache for geometry layers
- Pan-shift cache + clipped invalidation

Acceptance:

- Smooth pan/zoom with large feature count
- No tile/shape tearing lines

## Phase 4 - Tool and Command System

Goal: safe edits with undo/redo.

Create commands:

- `CreateFeatureCommand`
- `UpdateFeatureGeometryCommand`
- `DeleteFeatureCommand`
- `SplitParcelCommand`
- `MergeParcelsCommand`
- `MoveFeatureToLayerCommand`

Wire through service layer, not direct control-to-db writes.

Acceptance:

- Undo/redo replays both runtime and persisted state consistently

## Phase 5 - Parcel Topology and Validation

Goal: land-readjustment-grade reliability.

Create:

- `Services/Canvas/TopologyValidationService.cs`
- `Services/Canvas/ParcelSplitService.cs`

Rules:

- invalid polygon detection
- overlap/gap check
- area reconciliation tolerance

Acceptance:

- Validation issues are persisted and shown consistently on map

## Phase 6 - Main Form and Project Wiring

Goal: full application integration.

Update:

- `IProjectScopedFactory` and `ProjectScopedFactory` with canvas repos/services
- project-open flow to initialize layer/feature caches
- project-settings apply flow to push theme/grid/snap to canvas

Acceptance:

- Opening a project restores canvas data and settings automatically

## 6. What Not To Do

- Do not build another standalone geometry persistence path outside `CanvasObject`
- Do not keep adding behavior to both `DrawingCanvasControl` and final `MapCanvasControl` for the same feature
- Do not place topology/business logic inside WinForms control event handlers

## 7. Definition of Done for "Robust Final Map Canvas"

All conditions must be true:

1. Features are loaded/saved from `CanvasObject` with geometry integrity
2. `CanvasLayer` fully controls rendering and selection behavior
3. Pan/zoom/render pipeline is stable and cache-optimized
4. Undo/redo is command-based and persistence-consistent
5. Topology validation exists and issues are persisted
6. Parcel split/merge flows are service-driven and testable

---

This document is the verified implementation baseline for final canvas work in this repository.
