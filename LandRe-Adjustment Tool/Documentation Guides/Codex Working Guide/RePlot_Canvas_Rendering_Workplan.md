# RePlot — Canvas Rendering Remediation Workplan

A dependency-ordered plan to fix every culprit found in the GDI+ vector pipeline. Each section explains the *why* before the *how*, names the exact files/methods, gives the signatures Codex needs, and ends with a test gate. Implement in phase order — later phases assume earlier ones are done.

The governing principle for all of it: **the cost of an interactive frame must be proportional to what changed, not to what is on screen.** Today a single selection click pays the cost of re-rasterizing the whole viewport. Every fix below moves you toward "small change → small work."

---

## Diagnosis summary (what we are fixing and why, ranked)

| # | Culprit | Root problem | Phase |
|---|---|---|---|
| 2 | Bounding box recomputed O(n) per feature, multiple times per frame | No cached bounds on `Shape` | 0 (foundation) |
| 1 | Full-viewport re-raster on select / move-commit / draw | Invalidation granularity = whole cache; selection rendering half-migrated to overlay | 1 (structural) |
| 3 | Per-vertex `WorldToScreen` via delegate + double-transformed segment endpoints + per-draw allocations | Point-by-point transform instead of batch/matrix | 2 (hot loop) |
| 4 | Two draw routes for the same polyline + `OLD/LegacyCanvas` duplicate tree + double `Draw` overloads | Dead/duplicate code | 2 (hot loop) |
| 5 | `MeasureString`/`DrawString` per label per rebuild | GDI+ text, re-measured every time | 2 (hot loop) |
| 6 | Fresh multi-key `OrderBy().ToList()` every rebuild | Sort not memoized | 2 (hot loop) |
| 7 | Empty `catch {}` hides GDI faults (the real source of high-zoom glitches) | No diagnostics | 3 (hygiene) |

Two consistency bugs surfaced during analysis, folded into the relevant phases:

- **LOD threshold mismatch.** `VectorDeferredRenderer.LevelOfDetailThreshold = 1_000`, but `VECTOR_RENDERING_GUIDE.md` says 20,000. Code wins; decide the real number and align the doc (Phase 0 note).
- **Selection rendering is half-converted.** `SelectObjectByClick` bakes-and-rebuilds; `RenderSelectionDecoration` + the async-refresh comment overlay-and-keep. Pick one (Phase 1).

---

## Phase 0 — Cached bounding box (do this first; everything depends on it)

### Why
`PolylineShape.GetBoundingBox()` scans every vertex and allocates a `RectangleD` on **every call**. It is called at least three times per feature per frame:

1. inside `VectorFeatureSpatialIndex.Query` — `.Where(f => f.Shape.GetBoundingBox().IntersectsWith(worldBounds))`
2. in `CanvasVectorRenderer.Render` for the LOD check (`feature.Shape.GetBoundingBox()`)
3. on every `VectorFeatureSpatialIndex.Rebuild` (one per moved shape today, plus selection rebuilds)

So for a 200-vertex parcel you re-walk 200 points 3× per frame for zero new information. This cost scales with *both* feature count and vertex count — the worst kind. Your own undo/redo guide already flagged the symptom ("Translate must update the bounding box," Issue 6), which confirms bounds are not cached anywhere.

### How
Cache the box on the base class, invalidate lazily on mutation.

**File:** `Models/Shapes/Shape.cs`

```csharp
public abstract class Shape : IShape
{
    private RectangleD? _cachedBounds;

    // Concrete shapes implement the actual computation here (the old body of GetBoundingBox).
    protected abstract RectangleD ComputeBoundingBox();

    public RectangleD GetBoundingBox()
        => _cachedBounds ??= ComputeBoundingBox();

    // Call this from every geometry mutation: Translate, vertex add/remove/edit, resize.
    protected void InvalidateBounds() => _cachedBounds = null;

    public virtual void Translate(PointD delta)
    {
        // subclass shifts its own fields first, then:
        InvalidateBounds();
    }
}
```

**Per concrete shape (`PolylineShape`, `LineShape`, `RectangleShape`, `CircleShape`, `ArcShape`, `EllipseShape`, `TextShape`, `DonutPolygonShape`, `PointShape`):**
- Rename the existing `GetBoundingBox()` body to `protected override RectangleD ComputeBoundingBox()`.
- In `Translate` and in any setter/method that mutates geometry (e.g. `PolylineShape.Vertices` edits, grip edits), call `InvalidateBounds()`.

**Critical gotcha:** `PolylineShape.Vertices` is currently `public List<PointD> { get; set; }`. A caller mutating the list in place (`Vertices.Add(...)`, `Vertices[i] = ...`) will **not** trip `InvalidateBounds()` and the cache will go stale → shape vanishes from spatial queries. Two acceptable fixes:
- (preferred) make the list private and expose mutation methods that invalidate, or
- expose `IReadOnlyList<PointD>` for reads and a `ReplaceVertices(...)` / `SetVertex(i, p)` API that invalidates.

Audit every site that writes to `Vertices` before shipping this.

### Files touched
`Shape.cs`, every concrete shape, plus any code mutating `Vertices` directly.

### Test gate
`shape.Translate(new PointD(100,100))` then `GetBoundingBox()` returns box shifted by (100,100). Edit a vertex → bounds update. `QueryShapesInBound` still returns the shape after both. Add a temporary counter in `ComputeBoundingBox`; confirm it stops growing once a static scene is just being re-rendered.

### Side note while you're here
Decide the real LOD threshold. 1,000 features is low for "skip sub-pixel shapes"; for a replotting project a few thousand parcels is normal and you do **not** want LOD kicking in during normal work and dropping small parcels. Recommend raising `LevelOfDetailThreshold` to something like 15,000–20,000 and updating the guide to match. LOD should be a safety valve for pathological imports, not a normal-operation behavior.

---

## Phase 1 — Stop re-rasterizing the whole viewport (the structural win)

This is the fix that lets you delete the most scaffolding. The hold-frame machinery (`_holdVectorZoomFrameUntilRefresh`, `EnsureVectorZoomSnapshot`, `CaptureRefreshHoldFrame`, the large frame-source state machine) exists because a full re-raster is slow enough to need an async path, and the async path leaves a blank gap that must be covered. Make the common edits cheap and synchronous, and the gaps — and most of the scaffolding — disappear.

You already have the mechanism: `RenderTransientShape`, `RenderSelectionDecoration`, `_immediateEditedOverlayFeatures`, and `SetVectorRenderExclusions(... invalidateCache:false)`. The `useIncrementalFastPath` in `SetVectorFeatures` proves the pattern works for add/edit. The job is to make it the **default for selection and move**, and to retire the bake-and-rebuild path.

### 1a. Selection: overlay only, never rebuild

**Problem.** `SelectObjectByClick` currently calls `RefreshVectorCacheForCurrentViewAsync()` with the comment "The selection glow is baked into the cached bitmap." That re-rasterizes the entire viewport to change the color of one parcel's outline. Meanwhile `RenderSelectionDecoration` and the async-refresh comment ("scene cache is rendered selection-free") describe the correct design. The bake path is the leftover.

**Target design (commit to this one):**
- The deferred vector cache is **always rendered selection-free.** Selection state never enters `CanvasVectorRenderer.Render`'s styling for the cache. (You partly do this already — finish it: remove any `IsSelected`-dependent styling from the cached-render path, and remove the per-feature "second pass" highlight from the cache build, moving it to the overlay.)
- Selection highlight is drawn **every frame as an overlay** on top of the cache blit, in the `InteractionOverlay` stage, via `RenderSelectionDecoration`. This is cheap: it draws only the selected features, which is a tiny set.

**Change `SelectObjectByClick` (and `ReplaceSelectedObjects`/`AddSelectedObjects`) to:**
```csharp
ApplySelectedShapeFlags();
// No cache rebuild. Selection is an overlay now.
RequestRender();
UpdateStatusBar();
NotifySelectedCanvasObjectsChanged();
```
Delete the `RefreshVectorCacheForCurrentViewAsync()` call from the selection paths.

**In the paint path (`RenderCanvasFrame` → InteractionOverlay stage):** after the cache blit, iterate the selected features and call `RenderSelectionDecoration` for each. Keep the data-layer "highlight on top" second pass here too, not in the cache build.

**Why this is correct and fast.** Selecting/deselecting is now O(selected count) of overlay draws on top of an unchanged cache bitmap. No bitmap allocation, no async, no hold-frame, no flicker. This is exactly how AutoCAD/Esri draw selection — highlight is a transient decoration, never part of the rasterized scene.

**Test gate.** Select/deselect parcels rapidly. The base cache bitmap is never rebuilt (add a temporary counter in `RenderNow`; it stays flat during selection). Highlight appears/clears with zero flicker. Pan/zoom still show the highlight (overlay runs each frame).

### 1b. Move-commit: overlay the moved shapes, bake asynchronously

**Problem.** `CommitMoveOperation` translates the shapes, then `EnsureVectorZoomSnapshot()` + `_holdVectorZoomFrameUntilRefresh = true` + `RefreshVectorCacheForCurrentViewAsync()` — i.e. full re-raster with a held frame to cover the gap. For moving one parcel.

**Target design.** Reuse the `useIncrementalFastPath` you already built for add/edit:
- After translate, keep the existing cache valid but add an **exclusion** for the moved shape IDs (`SetVectorRenderExclusions(movedIds, invalidateCache:false)`) so the stale pre-move position is not double-drawn from the cache, and set `_immediateEditedOverlayFeatures = movedFeatures` so the new position paints as a transient overlay.
- Kick `RefreshVectorCacheForCurrentViewAsync()` to bake the final state off-thread; when it lands it clears the overlay and exclusion. No hold-frame needed because the overlay already shows the correct final position synchronously.

This collapses move-commit to the same cheap path as draw-complete. The held-zoom-frame branch for move can then be deleted.

**Note on the in-drag preview:** your `CaptureMovePreviewBitmap` + offset-blit during the drag is already the right technique — leave it. This change is only about the **commit**, replacing the full rebuild with the overlay path.

**Test gate.** Move a parcel in a scene of several thousand. Commit is instant, no blank flash, final position correct, undo returns it. Cache-rebuild counter increments once (async), not synchronously on the UI thread.

### 1c. Retire the scaffolding the above makes dead

Once 1a+1b are in, these become unreachable for the common edits and should be removed (verify with usage search first — pan/zoom may still legitimately use the *zoom* snapshot, so be surgical):
- `_holdVectorZoomFrameUntilRefresh` branch in `GetVectorRenderFrame` and its `EnsureVectorZoomSnapshot()` callers in `CommitMoveOperation`/`CancelMoveOperation`.
- The `CaptureRefreshHoldFrame` / `DrawRefreshHoldFrame` / `ShouldDrawRefreshHoldFrame` path, **if** no remaining synchronous rebuild needs it. (Deletions/bulk refreshes still rebuild — keep the hold frame only for those, or convert deletions to the exclusion-overlay path too and delete it entirely.)

Do not delete the **pan** and **zoom** interactive caches (`BeginPan`/`BeginZoom`, composite pan bitmap, vector zoom buffer) — those are the legitimate, correct use of deferred bitmaps for continuous-gesture interaction and are not part of this problem.

### Phase 1 net effect
Select, move-commit, and draw-complete all become "mutate flags/overlay + async bake," none of which blocks the UI thread or flashes. The frame-source state machine shrinks from ~7 vector states to roughly: live-cache blit, pan frame, zoom frame.

---

## Phase 2 — Hot-loop and dead-code cleanup

These compound with Phase 1: fewer rebuilds *and* each rebuild is cheaper.

### 2a. Batch the world→screen transform (culprit #3)

**Why.** Your transform is a pure affine map (confirmed from the engine formula):
```
ScreenX = (WorldX - ViewOriginX) * ZoomScale
ScreenY = CanvasHeight - (WorldY - ViewOriginY) * ZoomScale
```
That is exactly a GDI+ `Matrix`:
```
[ ZoomScale,        0,         tx ]
[ 0,        -ZoomScale,        ty ]
  where tx = -ViewOriginX * ZoomScale
        ty =  CanvasHeight + ViewOriginY * ZoomScale
```
Calling `engine.WorldToScreen(point)` per vertex pays a delegate dispatch + struct construction per point. `DrawPolylineSegmentsWithPen` is worse: it transforms **both** endpoints of every segment, so each interior vertex is transformed twice. And `PolylineShape.Draw` does `Vertices.ConvertAll(...).ToArray()` — two allocations per polyline per draw, i.e. GC pressure → stutter.

**Two options, pick per shape:**

- **Option A (cleanest for fills/outlines):** set `graphics.Transform = worldToScreenMatrix` once at the start of `CanvasVectorRenderer.Render`, then draw geometry in **world coordinates** directly — no per-vertex transform at all. GDI+ applies the matrix in hardware-assisted fixed point. Caveat: pen widths and text then scale with zoom, so use `pen.Width` expressed in world units, or reset the transform for text/labels and stroke. This is the biggest win but touches every `Draw*` method, so stage it carefully.

- **Option B (lower risk, do this first):** keep screen-space drawing but transform each shape's vertices **once** into a reusable scratch `PointF[]` buffer owned by the renderer (grown as needed, never re-allocated per shape), and stop the double-transform. Replace `DrawPolylineSegmentsWithPen`'s per-segment transform with a single pass over the vertex array, then one `DrawLines`/`DrawPath`.

Recommend Option B now (safe, removes allocations and double-transform), Option A later once the pipeline is otherwise stable.

**Test gate.** Visual output identical. Allocation profiler: per-frame `PointF[]`/`List` allocations from the draw loop drop to ~zero. High-zoom pan of a dense parcel set is visibly smoother.

### 2b. Collapse duplicate draw routes + delete legacy tree (culprit #4)

**Findings:**
- `CanvasVectorRenderer` draws closed polylines via `graphics.DrawPath(pen, path)` **and** via `DrawPolylineSegmentsWithPen` (segment-by-segment `DrawLine`). Two routes, one shape. The path route is faster and the single source of truth — keep it. Keep segment-by-segment **only** where genuinely required (mixed line/arc polylines via `polyline.Segments`), and make that the explicit, documented exception, not a parallel default.
- `PolylineShape` and `TextShape` each have **two `Draw` overloads** (`Func<PointD,PointD>` and `Func<PointD,PointF>`). Pick one signature (the renderer uses `engine.WorldToScreen` → `PointD`), delete the other.
- `OLD/LegacyCanvas/...` shadows the live `MapCanvas` tree: duplicate `MoveShapeCommand`, `DrawingCanvasControl`, `OptimizedDeferredRenderer`, etc. Confirm nothing in the live build references `OLD/` (search the solution for `LegacyCanvas`), then delete the folder. Also `UI/CustomControls/DrawingCanvasControl.cs` appears to be a second legacy canvas alongside `MapCanvasControl` — verify which is live and remove the other.

**Why.** You asked specifically for dead-code removal. Duplicate draw paths also mean a bug fixed in one route silently persists in the other — a real correctness risk, not just clutter.

**Test gate.** Solution compiles with `OLD/` and the dead overloads removed. All shape types still draw. Closed polylines, mixed arc/line polylines, and donuts all render correctly through the single retained route each.

### 2c. Cache label metrics, prefer `TextRenderer` (culprit #5)

**Why.** `TextShape.Draw` calls `g.MeasureString` then `g.DrawString` on every cache rebuild. `MeasureString` is the expensive part, and GDI+ text is slower than GDI `TextRenderer.DrawText` — which your picker controls (`frmPointMarkerPicker`, `frmHatchPatternPicker`) already use, so the codebase contradicts itself.

**How.**
- Memoize measured size keyed by `(text, font, dpi)`; you already have a `LabelFontCache` in the render loop — extend it (or add a `LabelMetricsCache`) to also cache `SizeF`. Invalidate on text/font change only.
- For axis-aligned labels (the common case), draw with `TextRenderer.DrawText`. Keep `DrawString` only where you need rotation or precise GDI+ string formatting.
- The `try { ... } catch { }` wrapping `TextShape.Draw` must go (see Phase 3) — it currently hides exactly the transform-overflow faults that cause label glitches at high zoom.

**Test gate.** Labels render identically. `MeasureString` call count per rebuild drops to (unique label/font) count, not (visible label) count. Text crisper (GDI vs GDI+).

### 2d. Memoize the feature sort (culprit #6)

**Why.** `CanvasVectorRenderer.Render` runs `queriedFeatures.OrderBy(...).ThenBy(...).ThenBy(...).ToList()` — a 4-to-6 key sort plus list allocation — on every rebuild, even though display order rarely changes between, say, a selection and the next frame.

**How.** Maintain a master feature list pre-sorted by the same keys (`DrawingMarkupRenderPass`, `CadastralParcelRenderPass`, `ProjectBoundaryRenderPass`, `OpenSpaceRenderPass`, `DisplayOrder`, `Id`). Re-sort only in `UpdateVectorFeatures` when the set or display order actually changes. The per-frame path then only needs the spatial-index `Query` result *filtered against* the pre-sorted order — or, simpler, query returns candidates and you iterate the pre-sorted master list skipping non-visible ones (fine when visible candidate count is close to total; otherwise intersect). Measure both; the spatial query is the correctness boundary, the sort is the thing to hoist.

**Test gate.** Draw order unchanged. Sort executes on feature-set change only, not per frame (counter).

---

## Phase 3 — Diagnostics hygiene (culprit #7)

**Why.** Empty `catch {}` in `TextShape.Draw` and around the move-bitmap capture swallow the precise GDI exceptions (`OverflowException`, `ExternalException`, `ArgumentException` from out-of-range coordinates) that are usually the *actual* cause of "graphics issues at high zoom." You cannot fix what you cannot see.

**How.** Replace bare catches with typed catches that log via `IAppLogger` (you already inject it). Keep them narrow — catch the GDI family, not `Exception` — and let truly unexpected exceptions surface in development. The existing `Debug.WriteLine` calls elsewhere should also route through `IAppLogger` for consistency.

```csharp
catch (Exception ex) when (ex is OverflowException or ExternalException or ArgumentException)
{
    _logger.Warn($"Text draw skipped at extreme transform: {ex.Message}");
}
```

**Test gate.** Force a high-zoom case that previously glitched; confirm the log now names the exact failure and coordinate, instead of silently dropping the draw.

---

## Recommended execution order (summary)

1. **Phase 0** — cached bounds + LOD-threshold decision. Foundation; unblocks cheap culling and dirty-rect reasoning.
2. **Phase 1a** — selection becomes overlay-only; delete the bake-and-rebuild path.
3. **Phase 1b** — move-commit reuses the add/edit overlay fast-path.
4. **Phase 1c** — delete the now-dead hold-frame / zoom-snapshot scaffolding (surgically).
5. **Phase 2b** — delete `OLD/` tree and duplicate draw overloads (do early; shrinks surface area for the rest).
6. **Phase 2a (Option B)** — batch vertex transform, kill double-transform and per-draw allocations.
7. **Phase 2d** — memoize the sort.
8. **Phase 2c** — label metric cache + `TextRenderer`.
9. **Phase 3** — replace empty catches with logging.
10. *(later, optional)* **Phase 2a (Option A)** — matrix-based world-space drawing, once stable.

After Phase 1 you should already feel the difference: selection and move stop hitching. Phases 2–3 raise the ceiling and clean the codebase. None of this requires leaving GDI+ — a SkiaSharp migration stays correctly deferred until core features are done.

## What to hand Codex, and how

Per your workflow, give Codex **one phase at a time**, not the whole document. For each, paste the relevant section plus the actual current file, and let it apply its own judgment on the mechanics — the section states the contract and the test gate, which is what it needs. Provide only the changed files back. Verify each phase against its test gate before starting the next; the phases are ordered so a regression is always traceable to the last one applied.
