# RePlot — GDI+ / Skia-CPU / Skia-GPU Rendering Work Plan

**Audience:** Coding agent (Claude Code / Codex), one phase per session.
**Scope:** Make the existing `IMapRenderSurface` abstraction *actually* backend-neutral, then bring the Skia-CPU surface to production quality. GDI+ stays the default and the safety net. Skia-GPU is frozen as-is — do not touch `SkiaGpuMapRenderSurface` in this plan except where explicitly noted.
**Out of scope:** Replacing GDI+ as default, deleting the GPU surface, changing the public behavior of any shape's visual output. Every phase must be a pure refactor or additive change — pixel output for the GDI+ backend must not change unless a phase says so explicitly.

---

## 0. Ground truth before you start

Read these files in full before writing any code. Do not assume their contents — open them.

```
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Abstractions/IMapRenderSurface.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Abstractions/IMapPath.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Abstractions/IMapPathBuilder.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Backends/IMapRenderSurfaceFactory.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Gdi/GdiMapRenderSurface.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Gdi/GdiMapPathBuilder.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Gdi/GdiMapPath.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Skia/SkiaCpuMapRenderSurface.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Skia/SkiaGpuMapRenderSurface.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Skia/SkiaCanvasMapRenderSurface.cs
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/CanvasVectorRenderer.cs
LandRe-Adjustment Tool/UI/MapCanvas/Models/Shapes/IShape.cs
LandRe-Adjustment Tool/UI/MapCanvas/Models/Shapes/Shape.cs
LandRe-Adjustment Tool/UI/MapCanvas/Models/Shapes/PolylineShape.cs
LandRe-Adjustment Tool/UI/CustomControls/MapCanvasControl.cs
LandRe-Adjustment Tool/UI/CustomControls/MapCanvasControl.Designer.cs
LandReadjustment.Tests/GdiMapRenderSurfaceTests.cs
```

### Confirmed facts this plan is built on (do not re-litigate, just verify they still hold)

1. `IMapRenderSurface` / `IMapPath` / `IMapPathBuilder` / `IMapRenderSurfaceFactory` already exist and are clean, backend-neutral contracts.
2. `GdiMapRenderSurface` implements the contract correctly and is tested **through the interface** (`GdiMapRenderSurfaceTests` constructs `GdiMapRenderSurface` but asserts only via `IMapRenderSurface`/`IMapPath` members). This test style is the standard to copy for Skia.
3. `GdiMapRenderSurface` caches `Pen`/`Brush`/`Font` objects in `_penCache` / `_brushCache` / `_fontCache`. **`SkiaCpuMapRenderSurface` has no equivalent cache** — it allocates a new `SKPaint` on every `CreateStrokePaint`/`CreateFillPaint`/`CreateTextPaint` call.
4. Shape geometry is rebuilt from world coordinates **every frame** (this is correct and required for a pan/zoom CAD canvas — do not try to cache "the path" across frames, you will get stale geometry on pan).
5. World-space viewport clipping already exists per-edge (`ViewportClip.ClipSegment` / `ViewportClip.ClipPolygon`) and is called inside shape path-construction. This is not the bug to fix.
6. `Shape.Draw(Graphics g, Func<PointD,PointD> worldToScreen, bool isPreview)` is the abstract method every `IShape` implements. This signature is **hard-coupled to `System.Drawing.Graphics`**. Some shapes (confirmed: `PolylineShape`) call `g.DrawPath(pen, path)` and `new Pen(...)` directly inside `Draw`, completely bypassing `IMapRenderSurface`. This is the root cause that makes "switch to Skia" not actually switch some shapes.
7. `SkiaCpuMapRenderSurface.AsSkiaPath` / `SkiaGpuMapRenderSurface.AsSkiaPath` / `SkiaCanvasMapRenderSurface.AsSkiaPath` (and their `ConvertGdiPath`, `CreateStrokePaint`, `CreateFillPaint`, `DrawHatchedFill` siblings) are **near-duplicated across all three Skia surface classes**. This is dead-weight duplication, not three separate features.
8. `MapCanvasControl` is a plain WinForms `UserControl` (see `.Designer.cs`) wired with manual `new` construction, not a DI container. Two prior architectural reviews already in the project (`RePlot_Architectural_Analysis_missing and roadmap.pdf`, `RePlot_Implementation_Guide MIddlewareapp configuration dependency injection etc.pdf`) independently confirm this and propose a DI phase plan. **This plan does not implement full DI** — see §1 for the scope decision.
9. Known, already-diagnosed, backend-agnostic bugs exist outside rendering proper: `RTreeSpatialIndex.Insert()` does a full rebuild on every insert (O(n²) on import). Not fixed by this plan, but it will distort your Skia benchmarks if left in place — see §7.

---

## 1. Scope decision: DI is explicitly deferred

Full constructor-injected DI (per the existing roadmap docs, Phases 1–5, ~6 hours) is a separate, valid piece of work. This plan **does not require it** to ship a correct Skia-CPU adapter, because:

- `IMapRenderSurfaceFactory` is already an interface. The factory can be **manually constructed once at the composition root** (`frmMain` or `MapCanvasControl`'s constructor) and passed down — this is "poor man's DI" (manual constructor injection without a container) and satisfies Dependency Inversion even without `Microsoft.Extensions.DependencyInjection`.
- Introducing a full DI container as a side effect of a rendering task would conflate two refactors and make this phase plan unreviewable.

**Action for this plan:** wherever a class currently does `new GdiMapRenderSurface(...)` or similar concrete construction, replace it with a call through an injected/passed-in `IMapRenderSurfaceFactory` instance. Do **not** add a DI container. If you want full DI, treat it as a follow-up plan that consumes this one's output (the factory interface already supports it).

---

## 2. Backend selection model

Confirm (or create, if missing) a single enum and a single place that decides the active backend:

```csharp
public enum MapRenderBackend
{
    GdiPlus,
    SkiaCpu,
    SkiaGpu   // frozen — selectable in code, not exposed in UI/settings yet
}
```

`IMapRenderSurfaceFactory.ResolveBackend(MapRenderSurfaceOptions?)` already exists for this purpose — verify it, don't recreate it. The resolution rule for this plan:

- Default: `GdiPlus`.
- `SkiaCpu`: opt-in via a setting/flag (e.g. `MapRenderSurfaceOptions.PreferredBackend = MapRenderBackend.SkiaCpu`), available to switch at runtime for A/B testing during development.
- `SkiaGpu`: remains reachable only through existing code paths. Do not wire it into any new settings UI in this plan.

**Test gate:** a unit test that asserts `ResolveBackend` falls back to `GdiPlus` when `IsBackendAvailable(SkiaCpu)` returns `false` (e.g. missing native Skia binaries) — the app must never crash from a missing backend, it must degrade to GDI+.

---

## Phase 1 — Close the abstraction leak (shape rendering goes 100% through `IMapRenderSurface`)

**Why first:** every later phase assumes shapes draw exclusively through `IMapRenderSurface`. If even one shape calls `Graphics`/`Pen` directly, switching the active backend silently does nothing for that shape, or crashes if `Graphics` is unavailable under a pure-Skia paint path. This must be fully true before Phase 2 has any value.

### 1.1 — Audit

Produce a checklist (as a markdown table in your PR description, not just in your head) of every class implementing `IShape`, found under:

```
LandRe-Adjustment Tool/UI/MapCanvas/Models/Shapes/
```

For each shape class, record:

| Shape class | `Draw()` calls `Graphics`/`Pen`/`Brush` directly? | Goes through `CanvasVectorRenderer.DrawShape` only? | Action needed |
|---|---|---|---|

Known from this session: `PolylineShape.Draw` is confirmed non-compliant (`g.DrawPath(pen, path)`, `new Pen(...)`). Treat every other shape as unverified until you've actually opened the file — do not assume compliance.

### 1.2 — Fix the abstract contract

The signature `void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false)` on `IShape`/`Shape` is the leak itself — it offers `Graphics` as a parameter, so of course implementations use it. Decide between two valid approaches and pick **one** consistently:

- **Option A (preferred, smaller diff):** Keep `Draw(Graphics g, ...)` as a *legacy/fallback* path used only by code that hasn't migrated to `CanvasVectorRenderer.DrawShape`, but make every shape's actual rendering logic live in a new method that takes `IMapRenderSurface`:
  ```csharp
  void DrawOnSurface(IMapRenderSurface surface, MapCanvasEngine engine, bool isPreview = false);
  ```
  and have the legacy `Draw(Graphics g, ...)` build a `GdiMapRenderSurface` around `g` internally and delegate to `DrawOnSurface`. This guarantees both call paths produce identical pixels by construction (there's only one implementation).

- **Option B (larger diff, cleaner long-term):** Remove `Draw(Graphics g, ...)` from `IShape` entirely, force every call site to go through `CanvasVectorRenderer`, which is already the dominant pattern for circle/ellipse/grips per the code reviewed this session.

**Recommendation: Option A for this phase.** It's the smaller, safer diff and gets every shape onto the surface abstraction without a large blast radius. Option B can be a later cleanup once nothing calls the legacy method anymore (verify via a "find all references" pass — if zero non-test callers remain, delete it then).

### 1.3 — Migrate non-compliant shapes

For `PolylineShape` (and any other shape flagged in 1.1):
- Replace `using GraphicsPath path = CreateScreenPath(worldToScreen); g.DrawPath(pen, path);` with construction through `IMapPathBuilder` (`surface.CreatePath()` → `MoveTo`/`LineTo`/`AddPolygon`/`Build()`) and `surface.DrawPath(path, strokeStyle)`.
- Replace `new Pen(color, width) { DashStyle = ... }` with a `StrokeStyle` record (color, width, dash pattern, cap, join) passed to `surface.DrawPath`. `StrokeStyle` already exists as a type used elsewhere in `CanvasVectorRenderer` — reuse it, don't invent a parallel one.
- Preserve exact current visual behavior: same colors for selected/preview/normal states, same pen widths (`2f` selected, `0.25f` normal — copy values exactly, do not "improve" them in this phase).

### 1.4 — Test gate

- Existing GDI+ rendering tests must pass unchanged (no pixel diff).
- Add a test that renders a `PolylineShape` through `GdiMapRenderSurface` and asserts non-background pixels exist, mirroring the style of `GdiMapRenderSurfaceTests.DrawTextAndImage_RendersVisiblePixels`.
- Manual check: open the app on GDI+ (default, unchanged), confirm polylines render identically to before the change — same color, width, dash behavior.

**Do not proceed to Phase 2 until every shape in the audit table is confirmed compliant.**

---

## Phase 2 — Native path construction per backend (eliminate the GDI→Skia translation step)

**Why:** Today, when the Skia surface is active, geometry is built as a GDI+ `GraphicsPath` first (because shape code calls `surface.CreatePath()` which — once Phase 1 lands — is backend-neutral, but internally some geometry-building helpers, e.g. `PolylineShape.CreateScreenPath`, still hard-build a `System.Drawing.Drawing2D.GraphicsPath`). Then, *only on the Skia surface*, `ConvertGdiPath` walks `PathPoints`/`PathTypes` and rebuilds the same geometry as an `SKPath`. That is two full path constructions per shape per frame on Skia, versus one on GDI+.

**Goal:** every shape's geometry-building code writes into the **active backend's path builder directly**, via `IMapPathBuilder`, with zero `GraphicsPath`-as-intermediate step when Skia is active.

### 2.1 — Confirm/extend `IMapPathBuilder`

Open `IMapPathBuilder` and `GdiMapPathBuilder`. Confirm it already supports (it should, per `GdiMapPathBuilder` reviewed this session): `MoveTo`, `LineTo`, `AddLine`, `AddPolygon`, `AddRectangle`, ellipse/arc equivalents, `Build()`. If arc/bezier support is missing (needed for `ArcShape`, curved polyline segments), add it to the interface first, implement in `GdiMapPathBuilder`, and write a test proving GDI+ output is unchanged before touching Skia.

### 2.2 — Create `SkiaMapPathBuilder`

```
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Skia/SkiaMapPathBuilder.cs
```

Implements `IMapPathBuilder` by writing directly into an `SKPath` (`MoveTo`, `LineTo`, `AddPoly`, `AddRect`, `ArcTo`/`CubicTo` for curves), mirroring `GdiMapPathBuilder` method-for-method. `Build()` returns an `IMapPath` wrapping the `SKPath` (`SkiaMapPath`, which should already exist — verify, don't recreate).

**Critical constraint:** `SkiaMapPathBuilder` must apply the exact same `ViewportClip.ClipSegment`/`ClipPolygon` calls, the exact same `worldToScreen` per-vertex transform, and the exact same point-rounding (`Math.Round`) behavior that the GDI path-building code uses today. This is a backend swap, not a geometry algorithm change — if you change clipping or rounding behavior in this phase, you've scope-crept into a different bug fix. Keep the geometry math identical; only the destination object type changes (`GraphicsPath` → `SKPath`, via the builder interface).

### 2.3 — Migrate shape geometry builders off `GraphicsPath`

For every shape identified in Phase 1's audit (and any geometry-building helper methods they call, e.g. `PolylineShape.CreateScreenPath`, `AddClippedPolygon`, `AddSampledArc`, and the equivalents inside `CanvasVectorRenderer` such as `CreateCircularCurvePath`):

- Change the method signature from returning/mutating a `GraphicsPath` to writing into an `IMapPathBuilder` passed in by the caller (the active `IMapRenderSurface.CreatePath()`).
- The calling code (`CanvasVectorRenderer.DrawShape` and friends) already holds the active `IMapRenderSurface` — pass `surface.CreatePath(fillRule)` in, fill it via the migrated builder method, call `.Build()`, then `surface.DrawPath`/`FillPath`.

This is mechanical but must be done shape-by-shape, file-by-file, with a compile-and-test checkpoint after each shape. **Do not attempt this as one giant diff** — Codex should do one shape class per commit, run the GDI+ pixel tests after each, and only move to the next shape once green.

### 2.4 — Remove `ConvertGdiPath` from the Skia hot path

Once 2.3 is complete for all shapes that reach the canvas in normal operation, `ConvertGdiPath` in `SkiaCpuMapRenderSurface` should have zero callers from the live render loop. **Do not delete it yet** — keep it as a documented fallback for any `IMapPath` instance that arrives as `GdiMapPath` from a code path you haven't migrated (better to have a slow-but-correct fallback than a crash). Add a one-line comment marking it as the fallback path, and a `Debug.WriteLine`/log warning the first time it fires per session, so you can detect any shape that was missed in the audit.

### 2.5 — Test gate

- Pixel-diff test: render a fixed scene (a handful of parcels, a polyline, an arc, text) through `GdiMapRenderSurface` before and after Phase 2 — must be **identical**, since this phase touches geometry-building code shared by both backends.
- New test: render the same fixed scene through `SkiaCpuMapRenderSurface`, decode the resulting `SKBitmap`, and assert shapes appear in the expected pixel regions (mirror the `CountNonBlackPixels`/`CountPixels` style already used in `GdiMapRenderSurfaceTests`).
- Add a debug counter/log for `ConvertGdiPath` fallback hits during a full manual pan/zoom/edit session on a real `.lpp` project file. Target: **zero hits** in normal operation by the end of this phase.

---

## Phase 3 — Paint/pen caching for the Skia-CPU surface

**Why:** `GdiMapRenderSurface` already caches `Pen`/`Brush`/`Font` by style key (`_penCache`, `_brushCache`, `_fontCache`, keyed by `PenKey`/`TextKey`). `SkiaCpuMapRenderSurface` allocates a fresh `SKPaint` on every single `CreateStrokePaint`/`CreateFillPaint`/`CreateTextPaint` call, every shape, every frame. This is the most expensive remaining waste once Phase 2 has removed the double path-build.

### 3.1 — Define style keys

If `PenKey` (used by `GdiMapRenderSurface`) is a private nested type, either make it shared/internal so Skia can reuse it, or define an equivalent `record struct StrokePaintKey` / `record struct FillPaintKey` that captures exactly the fields that affect `SKPaint` construction: color (ARGB), width, cap, join, dash pattern + dash values, antialias flag (derived from current `RenderQuality`).

```csharp
internal readonly record struct StrokePaintKey(
    int ColorArgb, float Width, LineCapKind Cap, LineJoinKind Join,
    DashPatternKind Dash, bool AntiAlias);

internal readonly record struct FillPaintKey(int ColorArgb, bool AntiAlias);
```

`AntiAlias` must be part of the key because `IsAntialiasEnabled` changes with `RenderQuality` (pan/zoom interaction frames vs. settled high-quality frames) — a cached paint built for one quality level must not silently get reused at another.

### 3.2 — Add caches to `SkiaCpuMapRenderSurface`

```csharp
private readonly Dictionary<StrokePaintKey, SKPaint> _strokePaintCache = new();
private readonly Dictionary<FillPaintKey, SKPaint> _fillPaintCache = new();
private readonly Dictionary<TextKey, SKPaint> _textPaintCache = new();   // mirror existing _typefaceCache pattern
```

Rewrite `CreateStrokePaint`/`CreateFillPaint`/`CreateTextPaint` to look up the cache first, construct-and-store on miss, exactly mirroring the lookup pattern already used in `GdiMapRenderSurface.GetPen`/`GetBrush` (open that method and copy the shape of the logic, don't reinvent it).

**Critical:** since cached `SKPaint` objects are now long-lived, every call site that currently does `using SKPaint paint = CreateStrokePaint(...)` must **stop disposing them** — ownership moves to the cache, disposed once in `SkiaCpuMapRenderSurface.Dispose()`. Audit every `using SKPaint` in the file and remove the `using` for paints obtained from the cache. Paints that are genuinely one-off (e.g. constructed with `IsAntialias = false` inline for a special composite operation, as seen in `MapCanvasControl.TryDrawGpuInteractionFrameCache`) can remain locally scoped — only style-driven, repeatable paints belong in the cache.

### 3.3 — Cache bound / eviction policy

Add a sane upper bound (e.g. 512 entries per cache) with a simple "clear and rebuild" eviction if exceeded, logged once. In practice, the number of distinct (color, width, style) combinations in a cadastral drawing is small (tens, not thousands) — this is a safety net, not an expected hot path.

### 3.4 — Test gate

- Unit test: call `CreateStrokePaint` twice with identical `StrokeStyle` values, assert **reference equality** of the returned paint (proves caching, not just correctness).
- Unit test: call with two different `RenderQuality` settings (different `AntiAlias`), assert **different** cached instances.
- Frame-time benchmark (simple stopwatch-based, not a full profiler requirement): render a fixed scene of N shapes (reuse the Phase 2 test scene, scaled up to a few thousand parcels — can synthesize via a grid of polygons if a real large dataset isn't on hand) 100 times through `SkiaCpuMapRenderSurface`, log average frame time before/after Phase 3. Record the numbers in the PR description — this is your evidence the phase delivered something, not just a structural change.

---

## Phase 4 — Collapse the three Skia surfaces into one shared implementation

**Why:** `SkiaCpuMapRenderSurface`, `SkiaGpuMapRenderSurface`, `SkiaCanvasMapRenderSurface` independently duplicate `ConvertGdiPath`, `CreateStrokePaint`/`CreateFillPaint`/`CreateTextPaint`, `DrawHatchedFill`, `CreateSourceToParallelogramMatrix`, `CopyBitmapToSkia`. After Phase 2 (native path building) and Phase 3 (paint caching), this duplicated logic is identical across all three — only how each surface obtains/owns its `SKCanvas` differs.

**Constraint from the user's request: do not modify `SkiaGpuMapRenderSurface`'s behavior or freeze status. This phase only restructures shared code; the GPU surface keeps working exactly as it does today, just sourced from a shared base instead of a private copy.**

### 4.1 — Extract a shared base

Create:
```
LandRe-Adjustment Tool/UI/MapCanvas/Rendering/Skia/SkiaMapRenderSurfaceBase.cs
```

An `abstract class SkiaMapRenderSurfaceBase : IMapRenderSurface` holding:
- The paint caches from Phase 3 (`_strokePaintCache`, `_fillPaintCache`, `_textPaintCache`, `_typefaceCache`).
- `ConvertGdiPath` (kept only as the documented fallback per 2.4).
- `DrawHatchedFill`, `CreateSourceToParallelogramMatrix`, `CopyBitmapToSkia`, `IsAntialiasEnabled`.
- All the `IMapRenderSurface` method implementations (`DrawPath`, `FillPath`, `DrawRectangle`, etc.) that only need `_canvas` (an `SKCanvas`, exposed via a `protected abstract SKCanvas Canvas { get; }` or constructor-injected reference) — i.e. everything that is identical across the three today.

Each concrete class (`SkiaCpuMapRenderSurface`, `SkiaGpuMapRenderSurface`, `SkiaCanvasMapRenderSurface`) shrinks to: constructor logic for obtaining/owning its `SKCanvas`/`SKSurface`/`GRContext`, `Dispose()` for its own native resources (GPU context, presented canvas, etc.), and `PixelSize`. Nothing else should remain — if a method body is identical to another surface's after this extraction, it belongs in the base, not in either subclass.

### 4.2 — Verify GPU surface is untouched behaviorally

After extraction, diff `SkiaGpuMapRenderSurface`'s remaining code against its pre-Phase-4 version. The only acceptable changes are: removed methods that moved to the base (verbatim), updated `base.Member` calls. No logic change. Run whatever GPU-path tests/manual checks exist today (even if minimal) to confirm no regression — you are not validating new GPU behavior, only confirming you didn't break old behavior while moving code around.

### 4.3 — Test gate

- All Phase 2 and Phase 3 tests still pass, now running against the refactored class hierarchy.
- Line-count check: total lines across the three Skia surface files should **decrease** (this is a duplication-removal phase — if it doesn't shrink, something was copied instead of shared).
- New `SkiaCpuMapRenderSurfaceTests.cs` (does not exist yet — create it) mirroring `GdiMapRenderSurfaceTests.cs` test-for-test where applicable: `ClipPath_WithSaveState_RestoresPreviousClip`, `DrawTextAndImage_RendersVisiblePixels`, etc., run against `SkiaCpuMapRenderSurface` through the `IMapRenderSurface` interface. This is the Liskov-substitution proof that was missing before this plan — every test that passes for `GdiMapRenderSurface` must have a Skia-CPU equivalent that also passes.

---

## Phase 5 — Wire `SkiaCpuMapRenderSurface` into `MapCanvasControl` as a selectable backend

**Why:** all prior phases produce a correct, fast, tested Skia-CPU surface — but it isn't reachable from the actual paint loop yet. This phase makes it live, behind a flag, without disturbing the GDI+ default.

### 5.1 — Composition root wiring (manual, not container DI — see §1)

In `MapCanvasControl`'s constructor (or wherever it currently does its setup — read the actual constructor before writing this), introduce:

```csharp
private readonly IMapRenderSurfaceFactory _surfaceFactory;
```

constructed once (e.g. `new DefaultMapRenderSurfaceFactory()` — check if this concrete factory already exists; if not, this phase creates it as the one place that knows about `GdiMapRenderSurface`/`SkiaCpuMapRenderSurface`/`SkiaGpuMapRenderSurface` concrete types). All paint-loop code that currently does `new GdiMapRenderSurface(graphics, ...)` directly must instead call `_surfaceFactory.CreateForGraphics(graphics, pixelSize, options)`.

**Constraint:** the factory's `ResolveBackend` must default to `GdiPlus` when no explicit override is set, so existing behavior is 100% unchanged for anyone who doesn't opt in.

### 5.2 — Backend selection surface (developer-facing only, for this plan)

Add a minimal way to flip the active backend for testing — this can be as simple as an environment variable, a debug menu item, or a constructor parameter on `MapCanvasControl` defaulting to `GdiPlus`. **Do not build end-user-facing settings UI in this plan** — that's a follow-up once Skia-CPU has been field-validated.

### 5.3 — Handle the `SKCanvas` source for `SkiaCpuMapRenderSurface` under WinForms

`MapCanvasControl` paints via the WinForms `Paint` event (`canvasSurface_Paint`), which hands you a GDI+ `Graphics`. For Skia-CPU you need an `SKBitmap`/`SKSurface` rendered off-screen, then blitted into that `Graphics` (`SKBitmap` → `Bitmap` → `Graphics.DrawImage`, or via `SkiaSharp.Views.WindowsForms`'s `SKControl` if you choose to host a dedicated Skia child control instead — **decide and document which approach you took**, both are valid, but the plan must record the choice so it isn't re-litigated later). Given the existing deferred-bitmap architecture already in `MapCanvasControl` (composite pan buffers, etc.), the off-screen-`SKSurface`-then-blit approach is more consistent with the current design than introducing a parallel `SKControl` — prefer it unless you find a concrete reason not to.

**Decision recorded after profiling/debug-log comparison:** `SkiaCpu` interactive presentation now uses a dedicated `SkiaCpuCanvasPanel` (`SKControl`) and renders through `SkiaCanvasMapRenderSurface`. This removes the app-level `Graphics -> SKSurface/Bitmap -> Graphics.DrawImage` bridge from the visible canvas path while keeping `GdiPlus` selectable. Cached raster/vector frames may still carry `GdiMapImage` sources until the image cache layer is migrated, so "without GDI layers" means no GDI paint/blit presentation path for `SkiaCpu`, not zero `System.Drawing` objects anywhere in the renderer.

### 5.4 — Test gate

- Full manual regression pass on a real `.lpp` project: load Baireni or another sample project, pan, zoom in to a high zoom level, zoom out, select/move a parcel, undo/redo — once on `GdiPlus` (must be pixel-identical to pre-plan behavior) and once on `SkiaCpu` (must be visually correct, no missing geometry, no antialiasing artifacts, no crash).
- Frame-time comparison logged for both backends on the same pan/zoom sequence (reuse the Phase 3 benchmark harness against the real control, not just the synthetic scene).

---

## Phase 6 — Cleanup

- Remove any now-dead code: if `Shape.Draw(Graphics g, ...)` (the Option A legacy path from §1.2) has zero remaining non-test callers after Phase 5, mark it `[Obsolete]` for one release, then schedule removal — do not delete it in the same PR that's still validating the migration, in case rollback is needed.
- Confirm `ConvertGdiPath`'s fallback-hit counter (from 2.4) shows zero hits across the Phase 5 regression pass. If it's still firing, you missed a shape in Phase 1 — go back and fix it, don't suppress the warning.
- Re-run the full existing test suite (`LandReadjustment.Tests`) — every test, not just the rendering ones — to confirm nothing outside rendering broke from the `IShape` interface changes in Phase 1.

---

## Explicit non-goals (do not do these as part of this plan)

- Do not implement a full DI container (`Microsoft.Extensions.DependencyInjection` wiring) — that's the separate, already-scoped roadmap in the existing PDFs.
- Do not fix `RTreeSpatialIndex`'s rebuild-on-insert bug as part of this plan — it's real and already diagnosed, but it's an indexing bug, not a rendering bug; fixing it here would conflate two unrelated benchmarks. Flag it in your Phase 3/5 benchmark notes if it visibly distorts results, but fix it as a separate, focused change.
- Do not add Skia-GPU to any user-facing settings or default path.
- Do not change clip tolerance, rounding behavior, or any visual constant (pen widths, colors, dash patterns) "while you're in there." If you spot something that looks wrong, note it for a separate ticket — this plan's diffs must be reviewable as "same behavior, different backend," not "behavior improvements bundled with a refactor."

---

## Summary table

| Phase | Deliverable | Touches GPU surface? | Can ship independently? |
|---|---|---|---|
| 1 | All `IShape.Draw` goes through `IMapRenderSurface` | No | Yes |
| 2 | Native per-backend path construction, no GDI→Skia translation in hot path | No | Yes (depends on 1) |
| 3 | `SKPaint` caching in `SkiaCpuMapRenderSurface` | No | Yes (depends on 2) |
| 4 | Shared base class across all 3 Skia surfaces | Restructure only, no behavior change | Yes (depends on 2, 3) |
| 5 | Skia-CPU wired into `MapCanvasControl` as selectable backend | No | Yes (depends on 1–4) |
| 6 | Dead code removal, final verification | No | Yes (depends on 5) |
