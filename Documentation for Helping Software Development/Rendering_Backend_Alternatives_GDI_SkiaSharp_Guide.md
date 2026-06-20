# Rendering Backend Alternatives: GDI+ and SkiaSharp

## Purpose

This document is the living implementation guide for adding alternate rendering
backends to the RePlot map canvas. It starts from the current GDI+ pipeline and
plans the migration toward a backend-neutral renderer that can support both
GDI+ and SkiaSharp.

The document must be updated during implementation whenever new rendering
decisions, coupling points, performance findings, tests, or temporary bridges
are introduced.

## Non-Negotiable Performance Goal

The renderer must be designed like an industry GIS/CAD canvas, not just a
backend swap.

Primary targets:

- Smooth cached pan and zoom near 60 FPS.
- Cached interaction frame time should stay around or below 16 ms.
- Full vector redraw should be tested against at least 100k features.
- Expensive vector and raster cache rebuilds must not block normal UI
  interaction.
- GDI+ must not regress while SkiaSharp is introduced.
- SkiaSharp must improve scalability and rendering flexibility, not merely add
  a second code path.

Performance rules:

- Keep spatial indexing and viewport culling as first-class render pipeline
  behavior.
- Avoid per-feature backend object allocation in hot loops.
- Use backend-local caches for pens, brushes, paints, fonts, paths, images, and
  hatch patterns.
- Keep interaction overlays separate from heavy vector cache rebuilds.
- Keep pan and zoom interaction using cached frames instead of full geometry
  redraw.
- Use level-of-detail, sub-pixel culling, and label culling for large datasets.
- Avoid repeated sorting, text measurement, path construction, and style
  resolution when inputs have not changed.
- Track direct render time, cache refresh time, pan frame time, zoom frame time,
  feature counts, skipped counts, and memory pressure in debug metrics.

## Current Active Rendering Pipeline

The active renderer is under `LandRe-Adjustment Tool/UI/MapCanvas`.

Main components:

- `MapCanvasRenderer`: orchestrates frame stages, background clear, fixed
  reference layers, raster content, vector content, interaction overlays, and
  cached-frame compositing.
- `CanvasVectorRenderer`: renders vector features, labels, hatches, preview
  shapes, transient shapes, selection decoration, and level-of-detail behavior.
- `VectorRenderContext`: stores vector render state and currently exposes GDI
  resources through `PenCache` and `BrushCache`.
- `VectorDeferredRenderer`: stores vector cache, pan buffer, and zoom buffer as
  GDI `Bitmap` instances.
- `RasterDeferredRenderer`: stores raster composite and layer caches as GDI
  `Bitmap` instances.
- `IRasterRenderLayer`: renders raster layers directly through
  `Graphics.DrawImage`.
- Shape models: store geometry, bounds, hit testing, snapping, cloning, and
  some GDI drawing/path helpers.

Current render stages:

1. Fixed reference content: grid, axis, origin marker, north marker support.
2. Raster content: MBTiles, XYZ tiles, projected raster layers, cached raster
   frames.
3. Vector content: spatial query, layer visibility, LOD, style resolution,
   shape drawing, labels.
4. Interaction overlay: zoom window, previews, selection, snap indicators, grip
   editing overlays, debug metrics.

## Current GDI+ Coupling Points

The current implementation is still deeply coupled to GDI+:

- `CanvasVectorRenderer` receives and uses `Graphics`.
- `MapCanvasRenderer` receives and uses `Graphics`.
- `VectorRenderContext` returns `Pen`, `SolidBrush`, `HatchBrush`, and
  `TextureBrush`.
- `PenCache` and `BrushCache` cache concrete GDI resources.
- Shape models expose `Draw(Graphics, ...)`.
- `PolylineShape.CreateScreenPath(...)` returns `GraphicsPath`.
- `DonutPolygonShape.CreateScreenPath(...)` returns `GraphicsPath`.
- `ParcelPathBuilder.ToPath(...)` returns `GraphicsPath`.
- Text rendering uses `Font`, `StringFormat`, `MeasureString`, and
  `DrawString`.
- Deferred vector and raster frames use `Bitmap`.
- Raster layers draw through `Graphics.DrawImage`, `ImageAttributes`, and
  `Bitmap` caches.
- `MapCanvasControl` uses a WinForms `CanvasPanel` and `PaintEventArgs.Graphics`.

These couplings must be separated in phases. Replacing only
`CanvasVectorRenderer` is not enough because frame composition, deferred cache
frames, overlays, and raster layers also touch GDI directly.

## Target Architecture

Rendering logic should depend on RePlot-owned contracts, not on GDI+ or Skia
types.

Backend selection:

```csharp
public enum MapRenderBackend
{
    GdiPlus,
    SkiaSharp
}
```

Core surface contract:

```csharp
public interface IMapRenderSurface : IDisposable
{
    Size PixelSize { get; }

    void Clear(Color color);
    IDisposable SaveState();
    void SetQuality(RenderQuality quality);
    void ClipPath(IMapPath path);

    IMapPathBuilder CreatePath(FillRule fillRule = FillRule.Winding);

    void DrawLine(PointF a, PointF b, in StrokeStyle stroke);
    void DrawPath(IMapPath path, in StrokeStyle stroke);
    void FillPath(IMapPath path, in FillStyle fill);

    void DrawRectangle(RectangleF rect, in StrokeStyle stroke);
    void FillRectangle(RectangleF rect, in FillStyle fill);

    void DrawEllipse(RectangleF rect, in StrokeStyle stroke);
    void FillEllipse(RectangleF rect, in FillStyle fill);

    void DrawArc(RectangleF rect, float startDeg, float sweepDeg, in StrokeStyle stroke);

    SizeF MeasureText(string text, in TextStyle style);
    void DrawText(string text, RectangleF layout, in TextStyle style);

    void DrawImage(IMapImage image, RectangleF dest, RectangleF? src, in ImageStyle style);
}
```

Path contracts:

```csharp
public interface IMapPath : IDisposable
{
    RectangleF Bounds { get; }
    int PointCount { get; }
}

public interface IMapPathBuilder
{
    void MoveTo(PointF point);
    void LineTo(PointF point);
    void AddLine(PointF start, PointF end);
    void AddPolygon(ReadOnlySpan<PointF> points);
    void AddRectangle(RectangleF rect);
    void AddEllipse(RectangleF rect);
    void AddArc(RectangleF bounds, float startDeg, float sweepDeg);
    void CloseFigure();
    IMapPath Build();
}
```

Style descriptors:

```csharp
public readonly record struct StrokeStyle(
    Color Color,
    float Width,
    DashPatternKind DashPattern,
    float DashScale,
    LineCapKind Cap,
    LineJoinKind Join);

public readonly record struct FillStyle(
    Color Color,
    FillPatternKind Pattern,
    Color PatternColor,
    double PatternScale);

public readonly record struct TextStyle(
    string FontFamily,
    float SizePx,
    Color Color,
    bool Bold,
    TextAlign HorizontalAlign,
    TextAlign VerticalAlign,
    float RotationDegrees);

public readonly record struct ImageStyle(
    float Opacity,
    ImageInterpolation Interpolation);
```

The exact interfaces may evolve during implementation, but the key rule must
remain stable: renderer code passes backend-neutral styles and paths, while each
backend owns its native resources and caches.

## Backend Responsibilities

### GDI+ Backend

The GDI backend should preserve existing behavior first.

Responsibilities:

- Wrap `Graphics`.
- Convert `StrokeStyle` to cached `Pen`.
- Convert `FillStyle` to cached `SolidBrush`, `HatchBrush`, or `TextureBrush`.
- Convert `TextStyle` to cached `Font` and `StringFormat`.
- Convert path builder operations to `GraphicsPath`.
- Convert image drawing to `Graphics.DrawImage`.
- Own all GDI caches.
- Match current anti-aliasing, interpolation, pixel offset, compositing, and
  text rendering behavior.

### SkiaSharp Backend

The Skia backend should be implemented after the GDI backend works through the
same abstraction.

Responsibilities:

- Wrap `SKCanvas`.
- Convert `StrokeStyle` and `FillStyle` to cached `SKPaint`.
- Convert dash styles to `SKPathEffect`.
- Convert path builder operations to `SKPath`.
- Convert image handles to `SKImage` or `SKBitmap`.
- Support clip, save/restore, anti-aliasing, text, hatches, and cached frame
  drawing.
- Render through `SKControl.PaintSurface` for the real WinForms backend.

## Step-By-Step Implementation Plan

Every step must be verified before moving to the next.

### Step 1: Create Living Documentation

Create this guide and keep it updated.

Verification:

```powershell
dotnet build "LandRe-Adjustment Tool.sln"
```

Expected result: build succeeds because this is a documentation-only change.

### Step 2: Add Rendering Abstractions

Add `UI/MapCanvas/Rendering/Abstractions`.

Create:

- `MapRenderBackend`
- `IMapRenderSurface`
- `IMapPath`
- `IMapPathBuilder`
- `IMapImage`
- `StrokeStyle`
- `FillStyle`
- `TextStyle`
- `ImageStyle`
- rendering enums for dash, cap, join, fill rule, quality, interpolation, and
  text alignment

No existing renderer should use these yet.

Verification:

```powershell
dotnet build "LandRe-Adjustment Tool.sln"
```

### Step 3: Implement GDI Surface Wrapper

Add:

- `GdiMapRenderSurface`
- `GdiMapPath`
- `GdiMapPathBuilder`
- `GdiMapImage`

Implement by wrapping current GDI+ types. Move or duplicate cache logic
internally first; do not remove old caches until renderer migration is stable.

Verification:

```powershell
dotnet build "LandRe-Adjustment Tool.sln"
dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj"
```

### Step 4: Add Surface Smoke Tests

Add tests proving `GdiMapRenderSurface` can draw:

- line
- path stroke
- path fill
- rectangle
- ellipse
- arc
- text
- image
- save/restore with clip

Verification: rendered bitmap has non-background pixels for each primitive.

### Step 5: Port `VectorRenderContext`

Change `VectorRenderContext` so it no longer returns GDI objects.

It should retain:

- zoom scale
- anti-aliasing flag
- preview flag
- selection-decoration-only flag
- clip world bounds
- adaptive line width helpers

GDI resource caches move into `GdiMapRenderSurface`.

Verification:

```powershell
dotnet build "LandRe-Adjustment Tool.sln"
```

### Step 6: Port `CanvasVectorRenderer` To Surface

Change vector renderer signatures from `Graphics` to `IMapRenderSurface`.

Port:

- feature rendering
- preview rendering
- transient shapes
- selection decoration
- lines, rectangles, circles, ellipses, arcs
- polylines and donut polygons
- text labels
- hatches
- point markers
- selection glow

At the call boundary, construct `GdiMapRenderSurface` so runtime behavior stays
GDI-backed.

Verification:

```powershell
dotnet build "LandRe-Adjustment Tool.sln"
dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj"
```

### Step 7: Replace Shape `GraphicsPath` Coupling

Add backend-neutral path writing to shapes.

Replace:

```csharp
GraphicsPath CreateScreenPath(...)
```

with:

```csharp
void BuildScreenPath(
    IMapPathBuilder builder,
    Func<PointD, PointD> worldToScreen,
    RectangleD? clipWorldBounds);
```

Update:

- `PolylineShape`
- `DonutPolygonShape`
- `ParcelPathBuilder`
- selection outline path creation

Verification: clipping tests and vector tests pass.

### Step 8: Remove Shape `Draw(Graphics, ...)` Usage

The active rendering path should not call shape-owned GDI draw methods.

Shapes should own:

- geometry
- bounds
- snapping
- hit testing
- cloning
- translation

Renderer owns drawing.

Verification:

- No active renderer calls `shape.Draw(Graphics, ...)`.
- Build succeeds.
- Canvas render tests pass.

### Step 9: Port `MapCanvasRenderer` To Surface

Port fixed and overlay rendering:

- background clear
- grid
- axis lines
- origin marker
- north marker
- zoom window
- cached frame drawing
- interaction overlay

`MapCanvasRenderer.Render(Graphics, ...)` may remain as the public GDI entry
point temporarily, but internally it should use `GdiMapRenderSurface`.

Verification: build and tests pass; GDI visual behavior remains stable.

### Step 10: Generalize Deferred Vector Frames

Introduce backend-neutral frame abstractions:

- `IRenderFrame`
- `IRenderFrameLease`
- `IRenderFrameFactory`

GDI frame implementation wraps `Bitmap`.

Verification:

- vector cache refresh works
- pan cache works
- zoom cache works
- async refresh still works
- build and tests pass

### Step 11: Keep Raster GDI First, Add Image Bridge

Do not fully port raster layers yet.

Add an `IMapImage` bridge around existing GDI `Bitmap` objects so current
raster/cache frames can be drawn through the surface interface.

Verification:

- MBTiles tests pass.
- XYZ tile rendering still works.
- Raster pan/zoom cache still works.

### Step 12: Add Backend Selection

Centralize backend creation in one factory or service.

Default must remain:

```csharp
MapRenderBackend.GdiPlus
```

Verification:

- GDI default behaves as before.
- Skia enum path compiles even before full Skia rendering is complete.

### Step 13: Add SkiaSharp WinForms Package

Add:

```xml
<PackageReference Include="SkiaSharp.Views.WindowsForms" Version="3.119.4" />
```

Align `SkiaSharp` if needed.

Verification:

```powershell
dotnet restore "LandRe-Adjustment Tool.sln"
dotnet build "LandRe-Adjustment Tool.sln"
```

### Step 14: Add Skia Canvas Host

Add a Skia host using `SKControl`.

Create:

- `SkiaCanvasHost`
- `GdiCanvasHost` if useful
- shared host interface for invalidation, sizing, focus, mouse/key event
  forwarding, and paint callbacks

Verification:

- GDI host still works.
- Skia host clears a blank canvas.
- Mouse and keyboard interactions still reach `MapCanvasControl`.

### Step 15: Implement Skia Surface

Add:

- `SkiaMapRenderSurface`
- `SkiaMapPath`
- `SkiaMapPathBuilder`
- `SkiaMapImage`

Implement vector primitives first:

- line
- path stroke and fill
- rectangle
- ellipse
- arc
- text
- image bridge
- clip
- save/restore
- anti-aliasing quality

Verification:

- offscreen `SKSurface` smoke tests pass
- `SKControl` renders a simple frame
- build succeeds

### Step 16: Enable Skia Vector Rendering

Wire Skia into the full canvas frame path for vector and overlays.

Raster may still use bridged GDI bitmaps at this stage.

Verification:

- same project opens with GDI backend
- same project opens with Skia backend
- lines, parcels, labels, hatches, selections, previews, grid, axis, and
  overlays appear
- no crash during pan, zoom, selection, draw preview, or grip edit

### Step 17: Visual Parity Pass

Compare GDI and Skia output for:

- simple lines
- dense parcels
- donut parcels
- road polygons
- labels
- rotated labels
- point markers
- hatch patterns
- selected features
- raster plus vector stack

Fix differences in backend implementations or shared style descriptors.

### Step 18: Performance Pass

Benchmark both backends with:

- 20k vector features
- 50k vector features
- 100k vector features
- pan interaction
- zoom interaction
- selection overlay
- label-heavy scene
- raster-heavy scene

Record:

- direct vector render time
- cache refresh time
- pan frame time
- zoom frame time
- memory usage
- GDI handle count for GDI backend

### Step 19: Update This Guide After Every Step

After each implementation step, update:

- completed work
- files changed
- design decisions
- newly discovered coupling points
- build and test result
- known issues
- next step

## Testing Strategy

Required checks:

```powershell
dotnet build "LandRe-Adjustment Tool.sln"
dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj"
```

Existing tests to preserve:

- `CanvasVectorRendererTests`
- `ViewportClipRenderingTests`
- `HatchPatternServiceTests`
- `MbTilesRasterPipelineTests`

New tests to add:

- fake-surface draw order and style tests
- GDI primitive smoke tests
- GDI visual parity tests
- Skia offscreen primitive smoke tests
- selection glow tests
- hatch tests through both backends
- cached frame pan/zoom tests
- large feature performance tests

## Final Acceptance Criteria

The rendering backend work is complete when:

- GDI+ remains stable and default.
- SkiaSharp is selectable as an alternate backend.
- `CanvasVectorRenderer` does not depend directly on `Graphics`.
- Active shape rendering does not depend on `GraphicsPath`.
- Backend switching is centralized.
- Deferred vector cache still works.
- Raster rendering still works.
- Existing tests pass.
- New GDI and Skia rendering tests pass.
- `dotnet build "LandRe-Adjustment Tool.sln"` succeeds.
- 100k-feature scenes remain usable.
- Cached pan and zoom feel smooth and target about 60 FPS.

## External References

- Microsoft WinForms GDI+ drawing:
  https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/graphics-and-drawing-in-windows-forms
- Microsoft WinForms double buffering:
  https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-reduce-graphics-flicker-with-double-buffering-for-forms-and-controls
- Microsoft `System.Drawing`:
  https://learn.microsoft.com/en-us/dotnet/api/system.drawing
- Microsoft `SKControl`:
  https://learn.microsoft.com/en-us/dotnet/api/skiasharp.views.desktop.skcontrol
- Skia `SkPath` overview:
  https://skia.org/docs/user/api/skpath_overview/
- Skia `SkPaint` overview:
  https://skia.org/docs/user/api/skpaint_overview/
- `SkiaSharp.Views.WindowsForms`:
  https://www.nuget.org/packages/SkiaSharp.Views.WindowsForms/

## Progress Log

### 2026-06-19 - Step 1 Started

- Created the living rendering backend guide.
- Captured the current GDI+ coupling points.
- Captured the step-by-step implementation plan.
- Added the performance-first requirement for industry-style canvas behavior.
- Next verification: run `dotnet build "LandRe-Adjustment Tool.sln"`.

### 2026-06-19 - Step 1 Verified

- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and existing warnings.

### 2026-06-19 - Step 2 Completed

- Added backend-neutral rendering abstractions under
  `LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Abstractions`.
- Added `MapRenderBackend`.
- Added primitive contracts:
  - `IMapRenderSurface`
  - `IMapPath`
  - `IMapPathBuilder`
  - `IMapImage`
- Added backend-neutral style records:
  - `StrokeStyle`
  - `FillStyle`
  - `TextStyle`
  - `ImageStyle`
- Added rendering enums for dash patterns, fill patterns, fill rules, image
  interpolation, line caps, line joins, render quality, and text alignment.
- No active renderer uses the abstractions yet; this keeps Step 2 behavior-free
  and makes Step 3 safer.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and existing warnings.
- Next step: implement the GDI+ surface wrapper against these contracts.

### 2026-06-19 - Step 3 Completed

- Added the first GDI+ backend adapter under
  `LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Gdi`.
- Added:
  - `GdiMapRenderSurface`
  - `GdiMapPath`
  - `GdiMapPathBuilder`
  - `GdiMapImage`
- `GdiMapRenderSurface` wraps `Graphics` and implements:
  - clear
  - save/restore state
  - quality settings
  - clip by path
  - line, path, rectangle, ellipse, arc drawing
  - path, rectangle, and ellipse filling
  - text measure/draw with cached `Font`
  - image drawing with opacity support through `ImageAttributes`
- The GDI adapter owns backend-specific resource caches:
  - cached `Pen` instances keyed by stroke style
  - cached `Font` instances keyed by text style
  - existing `BrushCache` for solid and hatch fills
- No active renderer call sites use the GDI adapter yet; this step is still
  behavior-neutral.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and existing warnings.
- Ran focused rendering tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~HatchPatternServiceTests"`.
- Result: passed, `8/8`.
- Ran the full test project. Result: failed, `69` passed and `7` failed.
- Full-suite failures observed:
  - `CreateFeaturesFromPolylineTests.BlockTarget_PreservesClosedPolylineArcMetadataForRendering`
    failed because expected pretty-printed JSON differed from compact JSON.
  - `MbTilesRasterPipelineTests` failed because GDAL was not configured
    correctly in the test environment.
  - `ProjectServiceTests` failed because SQLite extension loading reported
    "The specified module could not be found."
- These failures are recorded as current test-environment/baseline blockers
  because Step 3 does not change any active renderer call site.
- Next step: add direct smoke tests for `GdiMapRenderSurface`.

### 2026-06-19 - Code Documentation Added

- Added XML summaries and focused explanatory comments to the new rendering
  abstraction code so each interface, enum, style record, property, and method
  explains its role in the backend-neutral rendering contract.
- Added XML summaries and focused explanatory comments to the GDI+ adapter code
  so the purpose of each wrapper class, constructor, public method, and important
  helper is clear before the active renderer is migrated.
- The comments document how high-level map drawing concepts map into backend
  resources such as `Graphics`, `GraphicsPath`, `Pen`, `Brush`, `Font`,
  `Bitmap`, and `ImageAttributes`.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and existing warnings.
- Working tree status: the rendering guide, abstraction folder, and GDI adapter
  folder are new untracked additions awaiting review/commit.

### 2026-06-19 - Step 4 Completed

- Added direct smoke tests for the GDI+ backend adapter in
  `LandReadjustment.Tests/GdiMapRenderSurfaceTests.cs`.
- The new tests verify the adapter before any active map renderer migration:
  - primitive drawing through `DrawLine`, `DrawRectangle`, `FillRectangle`,
    `DrawEllipse`, `FillEllipse`, and `DrawArc`
  - backend-neutral path creation through `CreatePath`, `FillPath`, and
    `DrawPath`
  - scoped clipping through `SaveState` and `ClipPath`
  - text measuring and drawing through `MeasureText` and `DrawText`
  - bitmap drawing through `GdiMapImage` and `DrawImage`
- The test class and helper methods include XML summaries so the intent of each
  verification point remains understandable during future renderer work.
- Ran focused tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~GdiMapRenderSurfaceTests"`.
- Result: passed, `3/3`.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and `4 Warning(s)`.
- Build warnings were existing SQLite package vulnerability warnings for
  `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
- Next step: introduce a small backend factory/options layer so production code
  can ask for the active backend without directly constructing GDI+ or future
  SkiaSharp surfaces.

### 2026-06-19 - Step 5 Completed

- Added the backend selection/factory layer under
  `LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Backends`.
- Added:
  - `IMapRenderSurfaceFactory`
  - `MapRenderSurfaceOptions`
  - `MapRenderSurfaceFactory`
- The factory currently supports GDI+ as the available production backend.
- SkiaSharp is represented in the backend resolver but reports unavailable until
  the Skia adapter is implemented.
- `MapRenderSurfaceOptions` allows callers to:
  - request a backend
  - apply an initial render-quality preset
  - enable or disable fallback to GDI+ when the requested backend is unavailable
- `MapRenderSurfaceFactory.CreateForGraphics` creates a
  `GdiMapRenderSurface` for the current WinForms/GDI+ paint target and applies
  the requested initial quality.
- The factory code includes XML summaries explaining each class, interface,
  property, and method.
- Added focused tests in `LandReadjustment.Tests/MapRenderSurfaceFactoryTests.cs`.
- New tests verify:
  - default options create `GdiMapRenderSurface`
  - GDI+ is available and SkiaSharp is not yet available
  - SkiaSharp requests fall back to GDI+ when fallback is enabled
  - strict SkiaSharp requests throw `NotSupportedException` while unavailable
  - initial quality options are applied to the native GDI+ graphics object
- Ran focused tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapRenderSurfaceFactoryTests"`.
- Result: passed, `4/4`.
- Ran combined backend tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~GdiMapRenderSurfaceTests"`.
- Result: passed, `7/7`.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and `4 Warning(s)`.
- Build warnings were existing SQLite package vulnerability warnings for
  `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
- Next step: begin migrating one low-risk rendering path to use
  `IMapRenderSurface` through the factory while keeping the visible output
  unchanged.

### 2026-06-20 - Step 6 Completed

- Migrated the first production rendering path to the backend abstraction.
- Updated `MapCanvasRenderer` so the zoom-window interaction overlay now draws
  through `IMapRenderSurface` created by `IMapRenderSurfaceFactory`.
- Added an optional `IMapRenderSurfaceFactory` constructor parameter to
  `MapCanvasRenderer` so migrated render paths can be tested without changing
  existing call sites.
- Added `OverlaySurfaceOptions` with `ApplyInitialQuality = false` because the
  surrounding render stage already configures the native `Graphics` quality.
- Added `ToDashPatternKind` to translate existing GDI+ `DashStyle` values into
  the backend-neutral dash model.
- Kept vector content, raster content, grid, axis marker, north marker, and
  preview rendering on their existing GDI+ paths for now.
- Added `LandReadjustment.Tests/MapCanvasRendererBackendIntegrationTests.cs`.
- New integration test verifies:
  - the zoom-window overlay asks the factory for a render surface
  - the overlay still paints its fill and border pixels
  - the migrated path works through the normal `MapCanvasRenderer.Render` entry
    point
- Ran focused migrated-path test:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapCanvasRendererBackendIntegrationTests"`.
- Result: passed, `1/1`.
- Ran combined backend and viewport tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~ViewportClipRenderingTests"`.
- Result: passed, `12/12`.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and `4 Warning(s)`.
- Build warnings were existing SQLite package vulnerability warnings for
  `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
- Next step: migrate another contained screen-space overlay, preferably the
  axis/origin marker or north marker, after adding tests that lock its expected
  visible output.

### 2026-06-20 - Step 7 Completed

- Migrated the north marker overlay to the backend abstraction.
- Updated `MapCanvasRenderer.RenderNorthMarker` so the marker now uses
  `IMapRenderSurface` for:
  - left and right triangle fill paths
  - marker outline path
  - center spine line
  - north label text
- Added `CreatePolygonPath` as a small helper for backend-owned screen-space
  overlay polygons.
- Kept the same light/dark canvas color selection logic from the previous GDI+
  implementation.
- Continued using `OverlaySurfaceOptions` with `ApplyInitialQuality = false`
  because the render stage configures graphics quality before overlay drawing.
- Expanded `MapCanvasRendererBackendIntegrationTests` with a north-marker test.
- New north-marker test verifies:
  - the production renderer asks the backend factory for a surface
  - the marker creates visible non-background pixels
  - the marker creates visibly dark pixels from the outline, right fill, or
    label without depending on exact antialiased edge colors
- Ran migrated overlay integration tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapCanvasRendererBackendIntegrationTests"`.
- Result: passed, `2/2`.
- Ran combined backend and viewport tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~ViewportClipRenderingTests"`.
- Result: passed, `13/13`.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and `4 Warning(s)`.
- Build warnings were existing SQLite package vulnerability warnings for
  `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
- Next step: migrate the axis/origin marker overlay, which is larger and needs
  focused pixel tests for axis lines, origin marker shape, and labels.

### 2026-06-20 - Step 8 Completed

- Completed the GDI backend migration for `MapCanvasRenderer` frame-level
  drawing in one coherent pass.
- `MapCanvasRenderer` no longer directly calls `graphics.Clear`,
  `graphics.Draw*`, or `graphics.Fill*` for its own frame drawing.
- Frame-level drawing now goes through `IMapRenderSurface` for:
  - background clear
  - cached raster/vector/fixed-reference frame image drawing
  - adaptive grid lines
  - grid coordinate labels
  - zoom-window overlay
  - north marker overlay
  - axis lines
  - origin marker square
  - origin marker X/Y arms
  - origin marker X/Y labels
- Extended `IMapRenderSurfaceFactory` with `CreateImage` so cached frame
  bitmaps can be wrapped as backend images without `MapCanvasRenderer`
  directly constructing `GdiMapImage`.
- Updated `MapRenderSurfaceFactory` so the current production backend creates
  `GdiMapImage` wrappers.
- Added frame-renderer helper methods:
  - `CreateFrameSurface`
  - `CreateFrameImage`
  - `DrawPointLabel`
  - `DrawOriginMarker`
  - label-anchor offset helpers
  - image-interpolation conversion helper
- Removed renderer-owned grid/axis `Font` fields because migrated text now uses
  backend-neutral `TextStyle`.
- Expanded `MapCanvasRendererBackendIntegrationTests` to cover:
  - adaptive grid rendering
  - axis/origin marker rendering
  - cached frame image drawing through backend image wrappers
  - zoom-window overlay rendering
  - north-marker overlay rendering
- Ran migrated frame-renderer integration tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapCanvasRendererBackendIntegrationTests"`.
- Result: passed, `5/5`.
- Ran focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~HatchPatternServiceTests"`.
- Result: passed, `20/20`.
- Ran a direct search for remaining direct frame-renderer draw calls in
  `MapCanvasRenderer`.
- Result: no remaining `graphics.Clear`, `graphics.Draw*`, or
  `graphics.Fill*` calls in `MapCanvasRenderer`; remaining matches are
  `surface.*` calls and native quality setup for subrenderers.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and `4 Warning(s)`.
- Build warnings were existing SQLite package vulnerability warnings for
  `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
- Important scope note: `CanvasVectorRenderer` still contains direct GDI+
  drawing because it is the larger shape/feature renderer. Migrating it should
  be the next major backend-abstraction pass and will need dedicated tests for
  feature paths, fills, hatches, labels, selection outlines, previews, and area
  labels.

### 2026-06-20 - Step 9 Completed

- Started the `CanvasVectorRenderer` backend migration by moving the core
  geometric shape stroke/fill paths to `IMapRenderSurface`.
- Added `IMapRenderSurface` to `VectorRenderContext` so vector drawing methods
  can use one backend-neutral surface for the whole render pass.
- Added an injectable `IMapRenderSurfaceFactory` to `CanvasVectorRenderer`.
- Updated `MapCanvasRenderer` so its `CanvasVectorRenderer` uses the same
  backend factory as the frame renderer.
- Added vector style conversion helpers in `CanvasVectorRenderer`:
  - existing line style keys to `DashPatternKind`
  - existing vector stroke settings to `StrokeStyle`
  - existing fill/hatch settings to `FillStyle`
  - existing GDI `GraphicsPath` objects into GDI-backed `IMapPath` wrappers
- Migrated these core vector shape operations to backend-surface calls:
  - `LineShape` line drawing
  - `PolylineShape` fill and stroke
  - `DonutPolygonShape` fill and stroke
  - `RectangleShape` fill and stroke
  - `CircleShape` fill and stroke
  - `ArcShape` stroke and preview arc stroke
  - `EllipseShape` fill and stroke
  - shared closed-path solid and hatch fill path
- Kept remaining specialized GDI code in place for the next vector passes:
  - circle/diameter preview dimension callouts
  - selection glow and cadastral parcel selection highlight
  - point marker symbol renderer
  - annotation/text shape rendering
  - feature label rendering, including rotated linear labels
  - default `shape.Draw(...)` fallback for any shape not handled by the
    renderer switch
- Ran focused vector/backend tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests"`.
- Result: passed, `14/14`.
- Ran broader focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~HatchPatternServiceTests"`.
- Result: passed, `20/20`.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and `4 Warning(s)`.
- Build warnings were existing SQLite package vulnerability warnings for
  `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
- Next step: migrate `CanvasVectorRenderer` text/label rendering or selection
  decoration, with tests around selected feature highlights and rendered label
  bounds before changing those paths.

### 2026-06-20 - Step 10 Completed

- Continued the `CanvasVectorRenderer` migration by moving normal text and
  standard feature labels to `IMapRenderSurface`.
- Migrated `TextShape` rendering to backend text APIs:
  - creates backend-neutral `TextStyle` from the active `Font`
  - measures text with `IMapRenderSurface.MeasureText`
  - builds an anchored layout rectangle from the existing alignment rules
  - draws with `IMapRenderSurface.DrawText`
  - keeps `TextShape.LastRenderedBounds` updated for hit-testing/selection
- Migrated non-rotated feature labels to backend text APIs:
  - keeps existing label text resolution and anchor resolution
  - keeps the wide layout rectangle behavior used for aligned multiline labels
  - measures with the backend surface
  - draws with the backend surface when no linear-label rotation is needed
- Added text helper methods in `CanvasVectorRenderer`:
  - `CreateTextStyle`
  - `FontSizeToPixels`
  - `ToTextAlign`
  - `CreateAnchoredTextLayout`
- Deliberately left rotated linear labels on the GDI path for now because the
  current backend `TextStyle.RotationDegrees` rotates around the layout center,
  while the existing label code rotates around the label anchor point. Migrating
  that safely needs an explicit rotation-origin contract.
- Remaining direct GDI text/drawing spots in `CanvasVectorRenderer` after this
  step:
  - circle radius preview dimension callout
  - diameter preview dimension callout
  - rotated linear labels
  - selection glow/highlight drawing
  - point marker symbols
- Ran focused text/vector tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests"`.
- Result: passed, `14/14`.
- Ran broader focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~HatchPatternServiceTests"`.
- Result: passed, `20/20`.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and `4 Warning(s)`.
- Build warnings were existing SQLite package vulnerability warnings for
  `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
- Next step: either add a rotation-origin text contract for backend labels, or
  migrate selection/point-marker drawing to the backend surface.

### 2026-06-20 - Step 11 Completed

- Added an explicit text rotation-origin contract to the backend-neutral text
  model:
  - `TextStyle.RotationOrigin`
  - default behavior still rotates around the layout rectangle center when no
    origin is provided
- Updated `GdiMapRenderSurface.DrawText` so rotated text can rotate around the
  requested screen-space anchor point.
- Migrated rotated linear feature labels in `CanvasVectorRenderer` to
  `IMapRenderSurface.DrawText`.
  This preserves the existing readable-angle behavior while removing the
  renderer-owned `DrawRotatedLabel` GDI transform method.
- Migrated circle radius and diameter preview helper rendering to the backend
  surface:
  - helper lines now use `IMapRenderSurface.DrawLine`
  - white measurement callout boxes now use `FillRectangle` and `DrawRectangle`
  - preview measurement text now uses `MeasureText` and `DrawText`
- Added small helper methods for the preview measurement style:
  - `CreatePreviewMeasurementStroke`
  - `DrawPreviewMeasurementLabel`
- Result after this step:
  - `CanvasVectorRenderer` no longer has direct `graphics.DrawString` or
    `graphics.MeasureString` calls.
  - `RoadParcelRenderer` still has its own direct GDI road-name label drawing
    and should be migrated in a later step.
  - `MapCanvasRenderer` and `VectorDeferredRenderer` still set native
    `TextRenderingHint` values around GDI-backed rendering stages; those are
    quality-state concerns and can be replaced once those stages are fully
    backend-owned.
- Ran focused text/vector tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests"`.
- Result: passed, `14/14`.
- Ran broader focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~ViewportClipRenderingTests"`.
- Result: passed, `18/18`.
- Ran a direct search for remaining direct map-rendering text calls:
  `rg "graphics\.DrawString|graphics\.MeasureString|DrawRotatedLabel" "LandRe-Adjustment Tool/UI/MapCanvas/Rendering" -n`.
- Result: only `RoadParcelRenderer` still contains direct `DrawString` and
  `MeasureString` in the active map-rendering folder.
- Ran `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: build succeeded with `0 Error(s)` and `4 Warning(s)`.
- Build warnings were existing SQLite package vulnerability warnings for
  `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
- Next step: migrate `RoadParcelRenderer` road-name labels or move selection
  glow/highlight and point-marker symbol drawing behind the backend surface.

### 2026-06-20 - SkiaSharp CPU/GPU Rendering Setting Note

- The SkiaSharp implementation should expose rendering mode as an application
  setting instead of hard-coding one Skia path.
- Recommended setting model:
  - `GdiPlus`
  - `SkiaCpu`
  - `SkiaGpu`
  - optional `Auto` mode that tries GPU first and falls back to CPU or GDI when
    GPU initialization fails
- CPU Skia mode should use raster/image surfaces and can remain closer to the
  existing WinForms paint model.
- GPU Skia mode should use a GPU-backed Skia surface, most likely through a
  WinForms `SKGLControl`-style host or an equivalent OpenGL-backed surface.
- This is possible, but it is not just a boolean inside the renderer:
  - CPU and GPU paths have different control/surface lifetime rules
  - GPU needs context creation and fallback handling
  - map tile decoding, downloading, and cache management are still CPU/I/O work
  - GPU mainly helps with compositing, scaling, transforms, antialiasing, and
    pushing many already-prepared draw commands to the screen
- For industry-style performance, keep the existing backend interfaces and add
  two Skia backend implementations:
  - one CPU raster surface implementation
  - one GPU/OpenGL surface implementation
- The setting should be restart-safe and runtime-safe:
  - allow switching through application settings
  - dispose old backend resources cleanly
  - recreate the map canvas host/control if moving between WinForms GDI,
    Skia CPU, and Skia GPU requires a different control type
  - log and visibly fall back when GPU is unavailable

### 2026-06-20 - Step 12 Completed

- Migrated `RoadParcelRenderer` to draw through `IMapRenderSurface`.
- Added an injectable `IMapRenderSurfaceFactory` to `RoadParcelRenderer` while
  keeping the existing `Draw(Graphics, ...)` caller API intact.
- Added XML summaries for the renderer, constructors, public draw method, and
  helper methods so the road parcel backend flow is easier to follow.
- Replaced direct road parcel GDI drawing:
  - road fill now uses `IMapRenderSurface.FillPath`
  - road outline now uses `IMapRenderSurface.DrawPath`
  - island outlines now use backend-created paths and dashed `StrokeStyle`
  - road-name labels now use `MeasureText` and `DrawText`
- Improved the migration quality by building road parcel polygon paths through
  `IMapPathBuilder` instead of wrapping the old `GraphicsPath`.
  This makes road parcel geometry ready for a future Skia path builder.
- Added `RoadParcelRendererBackendTests`:
  - renders a donut road parcel into a bitmap
  - verifies visible output
  - verifies the interior ring remains unfilled
- Ran the new road parcel renderer test:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~RoadParcelRendererBackendTests"`.
- Result: passed, `1/1`.
- Ran focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~RoadParcelRendererBackendTests"`.
- Result: passed, `19/19`.
- Ran a direct search for remaining active map-rendering GDI path/text calls:
  `rg "graphics\.DrawString|graphics\.MeasureString|DrawRotatedLabel|graphics\.FillPath|graphics\.DrawPath|graphics\.DrawPolygon" "LandRe-Adjustment Tool/UI/MapCanvas/Rendering" -n`.
- Result:
  - no remaining direct `DrawString` / `MeasureString` calls in the active
    map-rendering folder
  - remaining direct path/polygon calls are in `CanvasVectorRenderer`
    selection/highlight paths and `PointMarkerRenderer` polygon marker symbols
- Next step: migrate `PointMarkerRenderer` to draw marker symbols through
  backend paths, then continue with `CanvasVectorRenderer` selection and
  highlight decoration.

### 2026-06-20 - Step 13 Completed

- Migrated `PointMarkerRenderer` to render marker symbols through
  `IMapRenderSurface`.
- Added a backend-surface overload:
  `PointMarkerRenderer.Draw(IMapRenderSurface, RectangleF, string?, Color, float)`.
- Kept the existing `Draw(Graphics, ...)` overload for current UI swatches and
  picker previews, but changed it to wrap the target in `GdiMapRenderSurface`
  and call the backend implementation.
- Migrated marker primitives:
  - circles and selected rings use `DrawEllipse`
  - dot markers use `FillEllipse`
  - plus/cross/X use `DrawLine`
  - square uses `DrawRectangle`
  - diamond, triangle, and star use backend-created paths and `DrawPath`
- Added XML summaries to the marker definition, marker renderer, public
  methods, and geometry helpers.
- Updated `CanvasVectorRenderer` so map point symbols call the backend-surface
  marker overload directly.
- Added `PointMarkerRendererBackendTests` for path-based marker symbols.

### 2026-06-20 - Step 14 Completed

- Migrated `CanvasVectorRenderer` selection decorations to the backend surface.
- Selection outline glow now uses `IMapRenderSurface.DrawPath` with
  backend-neutral `StrokeStyle` values instead of raw `graphics.DrawPath`.
- Cadastral/RePlot parcel selection highlight now uses:
  - `FillPath` for the transparent selected fill
  - `SaveState` and `ClipPath` for the clipped interior border treatment
  - `DrawPath` for the outer and inner selected border bands
- Removed the now-unused GDI hatch helper because hatch fills are handled by
  backend `FillStyle`.
- Added XML summaries to the changed selection and point-marker helpers.
- Ran focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~RoadParcelRendererBackendTests|FullyQualifiedName~PointMarkerRendererBackendTests"`.
- Result: passed, `22/22`.
- Ran a direct search for remaining direct vector primitive calls:
  `rg "graphics\.(DrawString|MeasureString|FillPath|DrawPath|DrawPolygon|DrawEllipse|DrawRectangle|DrawLine)" "LandRe-Adjustment Tool/UI/MapCanvas/Rendering" -n`.
- Result: no remaining matches.
- Ran a broader search for `graphics.(Draw|Fill|Clear|Measure)` in the active
  map rendering folder.
- Result: remaining direct drawing calls are now in raster/tile/cache-oriented
  renderers:
  - `XyzLiveTileRenderLayer`
  - `VectorDeferredRenderer`
  - `MbTilesRenderLayer`
  - `RasterDeferredRenderer`
  - `RasterRenderLayer`
- Next step: choose the next backend pass deliberately:
  - raster/cache/tile drawing through backend image APIs, or
  - reducing the remaining `GdiMapPath` wrappers in `CanvasVectorRenderer` by
    building more shape paths directly with `IMapPathBuilder`.

### 2026-06-20 - Step 15 Completed

- Migrated `VectorDeferredRenderer` internal cache clear/blit operations to
  backend surface calls.
- Replaced direct cache `graphics.Clear(...)` calls with a helper that uses
  `GdiMapRenderSurface.Clear`.
- Replaced direct `graphics.DrawImageUnscaled(...)` cache blits with a helper
  that wraps cached bitmaps in `GdiMapImage` and calls
  `IMapRenderSurface.DrawImage`.
- Kept the existing `Bitmap` cache ownership and `Graphics`-based vector render
  target for now, so this step does not change thread or cache lifetime
  behavior.
- Added helper summaries for:
  - `ClearSurface`
  - `DrawBitmapUnscaled`
  - `CreateSurface`

### 2026-06-20 - Step 16 Completed

- Migrated `RasterDeferredRenderer` internal cache clear/blit operations to
  backend surface calls.
- Replaced direct composite, layer-cache, and pan-buffer clear calls with
  `GdiMapRenderSurface.Clear`.
- Replaced direct unscaled layer/cache image composition with backend
  `DrawImage`.
- Replaced scaled pan preview composition with backend `DrawImage`.
- Kept individual raster layer implementations unchanged for now:
  `MbTilesRenderLayer`, `RasterRenderLayer`, and `XyzLiveTileRenderLayer`
  still own their internal tile/image drawing paths and need separate passes.
- Added helper summaries for:
  - `ClearSurface`
  - `DrawBitmapUnscaled`
  - `DrawBitmap`
  - `CreateSurface`

### 2026-06-20 - Step 17 Completed

- Added `DeferredRendererBackendTests`.
- New test coverage:
  - `VectorDeferredRenderer_BeginPan_ProducesPanFrame`
  - `RasterDeferredRenderer_RenderAndBeginPan_PreservesLayerPixels`
- These tests verify cache frame lifecycle and visible bitmap preservation after
  deferred clear/blit operations moved through the backend image path.
- Ran focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~DeferredRendererBackendTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~ViewportClipRenderingTests|FullyQualifiedName~RoadParcelRendererBackendTests|FullyQualifiedName~PointMarkerRendererBackendTests"`.
- Result: passed, `24/24`.
- Ran a direct search for deferred renderer direct clear/blit calls:
  `rg "graphics\.(Clear|DrawImage|DrawImageUnscaled)" "LandRe-Adjustment Tool/UI/MapCanvas/Rendering/VectorDeferredRenderer.cs" "LandRe-Adjustment Tool/UI/MapCanvas/Rendering/RasterDeferredRenderer.cs" -n`.
- Result: no remaining matches.
- Ran a broader active rendering-folder drawing scan:
  `rg "graphics\.(Draw|Fill|Clear|Measure)" "LandRe-Adjustment Tool/UI/MapCanvas/Rendering" -n`.
- Result: remaining direct drawing calls are now in the raster/tile layer
  implementations:
  - `MbTilesRenderLayer`
  - `RasterRenderLayer`
  - `XyzLiveTileRenderLayer`
- Next step: migrate the raster layer image draw paths one layer at a time,
  starting with the smallest static image path before touching live XYZ tile
  composition.

### 2026-06-20 - Step 18 Completed

- Added the SkiaSharp CPU render-surface adapter:
  `SkiaCpuMapRenderSurface`.
- Added Skia path support:
  - `SkiaMapPath`
  - `SkiaMapPathBuilder`
- The Skia CPU adapter draws into a locked `Format32bppPArgb` bitmap using
  `SKSurface.Create(...)`, then flushes and composites the bitmap back to the
  WinForms `Graphics` target on dispose.
- This is intentionally CPU-only:
  - no GPU context is created
  - no `SKGLControl` host is introduced
  - no Skia GPU adapter details are implemented in this step
- Kept the enum options stable:
  - `GdiPlus`
  - `SkiaCpu`
  - `SkiaGpu`
  - `SkiaSharp` as a compatibility alias for `SkiaCpu`
- `SkiaGpu` remains present in the enum but unavailable in
  `MapRenderSurfaceFactory.IsBackendAvailable(...)`.
- Factory behavior now is:
  - `GdiPlus` creates `GdiMapRenderSurface`
  - `SkiaCpu` creates `SkiaCpuMapRenderSurface`
  - `SkiaGpu` falls back to `GdiPlus` when fallback is enabled
  - `SkiaGpu` throws `NotSupportedException` when fallback is disabled
- Added bridge support inside the Skia CPU surface for current migration needs:
  - drawing `GdiMapPath` by converting `GraphicsPath` data to `SKPath`
  - drawing `GdiMapImage` by copying the source image to an `SKBitmap`
- This bridge keeps partially migrated rendering code usable while we continue
  replacing legacy GDI path/image creation in controlled steps.

### 2026-06-20 - Step 19 Completed

- Added backend selection to `MapCanvasRenderSettings`:
  `RenderBackend`.
- Added a persisted user setting:
  `Canvas_RenderBackend`, defaulting to `GdiPlus`.
- Updated `MapCanvasControl` so render settings load the configured backend
  before constructing/updating `MapCanvasRenderer`.
- Added `MapCanvasControl.ApplyRenderBackend(MapRenderBackend backend)` so the
  future settings UI can switch and persist the selected backend without
  touching renderer internals.
- Updated `MapCanvasRenderer` so every created frame surface receives the
  backend from current render settings.
- Updated `CanvasVectorRenderer` with `UpdateRenderBackend(...)` so feature,
  label, preview, and selection drawing use the selected backend too.
- Added and updated tests:
  - `SkiaCpuMapRenderSurfaceTests`
  - `MapRenderSurfaceFactoryTests`
  - `MapCanvasRendererBackendIntegrationTests`
- New Skia CPU test coverage verifies:
  - primitive drawing
  - text measurement/drawing
  - image drawing from `GdiMapImage`
  - clipping save/restore
  - temporary drawing support for legacy `GdiMapPath`
  - renderer-level Skia CPU backend selection
- Ran focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~SkiaCpuMapRenderSurfaceTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~DeferredRendererBackendTests|FullyQualifiedName~RoadParcelRendererBackendTests|FullyQualifiedName~PointMarkerRendererBackendTests"`.
- Result: passed, `26/26`.
- Important current boundary:
  - backend-neutral vector/grid/overlay/selection/label/cached-frame drawing can
    now run through Skia CPU
  - the application still uses WinForms/GDI+ as the host paint target
  - raster/tile layer internals still use GDI+ for tile mosaic building and
    direct tile image draws
- Next controlled implementation step:
  migrate raster/tile layer on-canvas image draw paths to backend image APIs,
  starting with `RasterRenderLayer`, then `MbTilesRenderLayer`, then
  `XyzLiveTileRenderLayer`.
- Do not implement Skia GPU yet. Keep only the enum/settings option and
  fallback behavior until the GPU host/control design is handled separately.

### 2026-06-20 - Step 20 Completed

- Migrated raster/tile image drawing to backend image APIs.
- Added `RasterImageRenderContext`.
  This creates one backend-neutral image surface per raster render pass so tile
  loops do not allocate one surface per tile.
- Extended `ImageStyle` with `TileFlipXY` for tile seam behavior.
- Extended `IMapRenderSurface` with a parallelogram image draw overload:
  `DrawImage(IMapImage, ReadOnlySpan<PointF>, RectangleF, ImageStyle)`.
- Implemented the new image overload in:
  - `GdiMapRenderSurface`
  - `SkiaCpuMapRenderSurface`
- The Skia CPU implementation maps source rectangles into destination
  parallelograms with an affine `SKMatrix`.
  This is needed by projected XYZ tile mesh rendering.
- Updated `IRasterRenderLayer.RenderVisible(...)` to accept the selected
  `MapRenderBackend`, defaulting to `GdiPlus` for compatibility.
- Passed the selected backend from:
  - `MapCanvasRenderer.RenderRasterLayers(...)`
  - `RasterDeferredRenderer.RenderNow(...)`
  - `RasterDeferredRenderer.RenderAsync(...)`
  - `MapCanvasControl` raster cache refresh calls
- Migrated raster layer draw paths:
  - `RasterRenderLayer` tile bitmap draws
  - `MbTilesRenderLayer` tile region draws
  - `XyzLiveTileRenderLayer` interactive composite draw
  - `XyzLiveTileRenderLayer` settled composite rebuild
  - `XyzLiveTileRenderLayer` offscreen mosaic tile composition
  - `XyzLiveTileRenderLayer` projected tile mesh/quadrilateral draws
  - `XyzLiveTileRenderLayer` parent placeholder/fallback draws
- Replaced decode-time GDI image-copy calls in raster/tile layers with bitmap
  clone conversions to `Format32bppPArgb`.
- Added Skia CPU test coverage for parallelogram image drawing.
- Ran focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~SkiaCpuMapRenderSurfaceTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~DeferredRendererBackendTests|FullyQualifiedName~RoadParcelRendererBackendTests|FullyQualifiedName~PointMarkerRendererBackendTests"`.
- Result: passed, `27/27`.
- Ran active map-rendering image draw scan:
  `rg "graphics\.(Draw|Fill|Clear|Measure)|\.DrawImageUnscaled\(|\.DrawImage\(" "LandRe-Adjustment Tool/UI/MapCanvas/Rendering"`.
- Result:
  - remaining native `Graphics.DrawImage` calls are inside `GdiMapRenderSurface`
    only, which is expected for the GDI backend adapter
  - remaining native `DrawImageUnscaled` is inside `SkiaCpuMapRenderSurface`
    final WinForms bridge flush, which is expected while the host control is
    still WinForms/GDI-based
  - active renderer/layer code now uses backend image APIs
- Skia GPU remains intentionally unimplemented. The enum/settings option still
  exists and resolves through fallback/strict unavailable behavior.

### 2026-06-20 - Step 21 Completed

- Added the render-backend selector to the existing Project Settings form.
- UI layout was added in `frmProjectSettings.Designer.cs`, following the
  project rule that static WinForms layout belongs in Designer files.
- The Map Canvas > Graphics group now exposes:
  - `GDI+ (Stable)`
  - `Skia CPU`
  - `Skia GPU (Future)`
- The Skia GPU option remains visible and selectable so the enum/settings path
  stays future-ready, but no GPU adapter details are implemented yet.
- Behavior was added in `frmProjectSettings.cs`:
  - load the persisted `Canvas_RenderBackend` setting into the combo box
  - save the selected backend when the settings form is applied
  - default the backend to `GdiPlus` when restoring default values
- Added XML summaries to the new settings helper methods so the backend setting
  path is easier to follow during future implementation.
- Verified the rendering migration boundary again with the active rendering
  scan:
  `rg "graphics\.(Draw|Fill|Clear|Measure)|\.DrawImageUnscaled\(|\.DrawImage\(" "LandRe-Adjustment Tool/UI/MapCanvas/Rendering"`.
- Result:
  - native `Graphics.DrawImage` remains inside `GdiMapRenderSurface`, which is
    expected for the GDI adapter
  - native `DrawImageUnscaled` remains inside `SkiaCpuMapRenderSurface`, which
    is the temporary WinForms/GDI host bridge for Skia CPU
  - active renderer/layer code uses backend APIs instead of direct GDI image
    drawing
- Ran focused renderer/backend regression tests:
  `dotnet test "LandReadjustment.Tests/LandReadjustment.Tests.csproj" --filter "FullyQualifiedName~SkiaCpuMapRenderSurfaceTests|FullyQualifiedName~MapRenderSurfaceFactoryTests|FullyQualifiedName~MapCanvasRendererBackendIntegrationTests|FullyQualifiedName~GdiMapRenderSurfaceTests|FullyQualifiedName~CanvasVectorRendererTests|FullyQualifiedName~DeferredRendererBackendTests|FullyQualifiedName~RoadParcelRendererBackendTests|FullyQualifiedName~PointMarkerRendererBackendTests"`.
- Result: passed, `27/27`.
- Ran full solution build:
  `dotnet build "LandRe-Adjustment Tool.sln"`.
- Result: succeeded, `0` errors and `72` warnings.
  The warnings are the existing project baseline warnings, including the
  `SQLitePCLRaw.lib.e_sqlite3` NU1903 package vulnerability warnings plus
  older nullability, obsolete API, and unused-field warnings.

### Current GPU Boundary

- Keep `MapRenderBackend.SkiaGpu` in the enum and settings UI.
- Do not add a GPU adapter implementation until a dedicated step designs the
  host control, Skia GPU context lifetime, fallback strategy, and hardware
  compatibility checks.
- Current production-ready choices are:
  - `GdiPlus`
  - `SkiaCpu`
