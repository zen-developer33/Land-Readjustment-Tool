# Raster Rendering Fix Guide — RePlot Map Canvas

> **Scope.** This guide explains *why* your XYZ live-tile rendering produces white patches and feels slow on pan/zoom, and *how* to fix it without rewriting the renderer. Each fix is independent — apply them in order, test after each one.
>
> **Files touched.** Almost all changes are in two files:
> - `LandRe-Adjustment Tool/UI/MapCanvas/Rendering/XyzLiveTileRenderLayer.cs`
> - `LandRe-Adjustment Tool/UI/CustomControls/MapCanvasControl.cs`
>
> **Audience.** You. The author understands the codebase. This guide explains the *intent* behind each change and gives you the *shape* of the code — you write the actual lines.

---

## Table of Contents

1. [Mental Model — How the Render Pipeline Works Today](#1-mental-model)
2. [The Three Visible Symptoms and Their Root Causes](#2-symptoms-and-causes)
3. [Fix 1 — Clip Tile Requests to a Valid Project Extent](#3-fix-1)
4. [Fix 2 — Parent-Tile Placeholder ("Blurry Cached Version")](#4-fix-2)
5. [Fix 3 — Skip Re-Projection When Viewport Hasn't Changed](#5-fix-3)
6. [Fix 4 — Branch the DrawImage Path on Project CRS](#6-fix-4)
7. [Fix 5 — Async Pan-End Refresh](#7-fix-5)
8. [Fix 6 — Pre-warm the Project Tile Bounds Cache](#8-fix-6)
9. [Testing Checklist](#9-testing-checklist)
10. [Performance Targets and How to Measure](#10-performance-targets)
11. [What Not To Change](#11-what-not-to-change)
12. [Reference — Key Methods Touched](#12-reference)

---

## 1. Mental Model

Before changing anything, get this picture in your head.

### 1.1 The render call chain

```
User scrolls wheel / drags mouse
        ↓
MapCanvasControl.canvasSurface_MouseWheel  /  MouseMove
        ↓
MapCanvasEngine.ZoomAtPoint  /  PanByScreenDelta     ← updates viewport math only
        ↓
canvasSurface.Invalidate()    (RequestRender)
        ↓
canvasSurface_Paint
        ↓
MapCanvasRenderer.Render
        ↓
RenderRasterContent
        ├─ if frame from RasterDeferredRenderer → blit cached bitmap (fast)
        └─ else → IRasterRenderLayer.RenderVisible (does work)
```

### 1.2 The two cache layers you already have

| Layer | What it caches | When it's used |
|---|---|---|
| `RasterDeferredRenderer._rasterCache` | Full canvas-size bitmap of all raster layers composited at *current* viewport | Steady state — when not panning/zooming |
| `RasterDeferredRenderer._panBuffer` | Snapshot of `_rasterCache` taken when pan starts | During mouse-down pan drag, blitted with screen-pixel offset |
| `RasterDeferredRenderer._zoomStartView` | Viewport snapshot (origin + zoom scale) at zoom start | During wheel zoom — used to scale-up `_rasterCache` for preview |
| `XyzLiveTileRenderLayer._tileCache` | Per-tile decoded `Bitmap` keyed by `TileKey(z,x,y)`, LRU 512 entries | When `RasterDeferredRenderer` re-composites |

### 1.3 The interactive vs steady distinction

Look at `MapCanvasControl.ShouldDeferDirectRasterRendering`:

```csharp
private bool IsInteractiveNavigation =>
    _isPanning || _isZooming || _isSelectingZoomWindow;

private bool ShouldDeferDirectRasterRendering =>
    IsInteractiveNavigation || _rasterCacheRefreshPending;
```

When `true`, the renderer **must not** call `IRasterRenderLayer.RenderVisible` directly. It must use a cached frame from `RasterDeferredRenderer`. This is the part that already works correctly.

The pain is on the **non-interactive paint** — the one that fires after `_zoomingStatusTimer` ticks, after `MouseUp`, after `Resize`. That paint takes the slow path through `RenderVisible`, and that's where the issues live.

---

## 2. Symptoms and Causes

### 2.1 Symptom A — White fan-shaped gaps on the world map

**What you see in the screenshot.** A curved fan-shaped imagery covering Africa/Asia/Australia, with thin white slivers between tiles especially toward the edges.

**Root cause.** Your project CRS is metric local (UTM-style — coords like `E: -226440  N: 3698524` in the status bar). Esri tiles arrive in EPSG:3857 (Web Mercator). In `XyzLiveTileRenderLayer.GetProjectTileBounds()`, every Web Mercator tile square gets reprojected into your project CRS using `CoordinateTransformation.TransformPoint` on **only its 4 corners**.

Two things go wrong:

1. **No clipping to a sensible project zone.** The code clips visible bounds to `±WebMercatorExtent` (the global Web Mercator limit, ±20,037,508 m). It never asks "is this tile even reasonable to project into UTM zone 45N?". So tiles from Greenland, Antarctica, the Pacific are all reprojected — and at the projection's mathematical edges, those reprojections produce degenerate or extremely curved shapes.

2. **4-corner reprojection of curved areas.** UTM is conformal only locally. Reproject a tile near the projection's edge of validity using only 4 corners → the tile's true reprojected boundary is curved, but you draw a 4-corner parallelogram. Adjacent tiles' parallelograms don't touch along curved edges → white gaps appear.

**Fix.** Clip to a project-specific valid extent before reprojection. See [Fix 1](#3-fix-1).

### 2.2 Symptom B — Slow pan and zoom

**What you feel.** Wheel scroll is laggy. Pan stutters. Releasing pan freezes the UI for a beat.

**Root causes** (multiple — they compound):

1. **Every paint re-runs the full reprojection pipeline.** `RenderVisible` always computes tile range, projects bounds, cancels previous CTS, creates new CTS — even when the viewport didn't change.

2. **`DrawImage` parallelogram form is the slowest GDI+ path.** `DrawBitmapRegion` uses the 3-point overload, which forces software-path warping. The integer-rectangle overload is 5–10× faster.

3. **`MouseUp` on pan-end calls `RefreshRasterCacheForCurrentViewImmediately` synchronously on the UI thread.** Re-stitching 50+ tiles freezes the UI for ~100ms.

**Fix.** [Fix 3](#5-fix-3), [Fix 4](#6-fix-4), [Fix 5](#7-fix-5).

### 2.3 Symptom C — White areas on zoom-in while tiles load

**What you see.** Zoom in deep → white squares where tiles haven't downloaded yet → tiles pop in one by one.

**Root cause.** `XyzLiveTileRenderLayer.DrawVisibleTiles` does:

```csharp
if (!_tileCache.TryGetValue(key, out Bitmap? bitmap))
    continue;   // ← just skips it. Nothing drawn.
```

There's no fallback to a parent tile (lower zoom level, blurry but covering the same area).

**Fix.** [Fix 2](#4-fix-2).

---

## 3. Fix 1

### Clip Tile Requests to a Valid Project Extent

**Goal.** Stop trying to render tiles in geographic regions that don't reproject cleanly into your project CRS.

**File.** `XyzLiveTileRenderLayer.cs`

### 3.1 Concept

Every projected CRS has a **valid use zone**. UTM zone 45N is valid roughly from 84°E to 90°E longitude, latitudes -80° to 84°. Use it outside that and reprojection produces garbage shapes.

You need a `RectangleD _validProjectExtent` field, computed once at construction, that represents this zone *in your project CRS coordinates*. Then in `RenderVisible` and `OnDebounceElapsed`, intersect the visible world bounds with this extent **before** any other math. Tiles outside the intersection are dropped.

### 3.2 Where the field comes from

You have three options, in order of preference:

1. **Read it from the project CRS metadata.** GDAL's `OSRGetAreaOfUse` (P/Invoke or recent OSGeo.OSR binding) returns the official "area of use" rectangle in WGS84 lat/lon. Reproject those four corners into your project CRS → you have your valid extent.

2. **Use the project's data extent.** If you already know the project covers a specific municipality/region, use that bounding box plus a 10–20% padding. This is what AutoCAD-style apps do — they trust the user's data.

3. **Hard-code per known CRS.** For Nepal/UTM 45N, use roughly `(-200000, 2900000)` to `(800000, 4500000)`. Acceptable as a stopgap, not as a long-term solution.

For RePlot, **option 2 is the right answer.** Your raster layer already exposes `WorldBounds`. Use that, expanded by 50% in each direction so the user can pan to context around their project.

### 3.3 Code shape

In the constructor of `XyzLiveTileRenderLayer`:

```csharp
// Pseudocode — you write the actual implementation
private readonly RectangleD _validProjectExtent;

// In constructor, after worldBounds is computed:
_validProjectExtent = ExpandExtent(worldBounds, paddingFactor: 0.5);
//   ↑ method that returns a rectangle 50% larger in each direction,
//     centered on the original.
```

In `RenderVisible`, **before** calling `TryTransformBounds`:

```csharp
// Step 1: Clip the visible world bounds to the valid project zone.
// If the visible area is entirely outside the project zone, return false.
if (!TryGetIntersection(visibleWorldBounds, _validProjectExtent,
                        out RectangleD clippedVisibleWorldBounds))
{
    return false;   // nothing to render
}

// Step 2: Now transform the *clipped* bounds to Web Mercator,
// not the original visible bounds.
if (!TryTransformBounds(_projectToWebMercator, clippedVisibleWorldBounds,
                        out RectangleD visibleWebMercatorBounds))
{
    return false;
}
```

Add `TryGetIntersection` as a private static helper if it's not already there (it's a standard rectangle intersection — `Math.Max(left)`, `Math.Min(right)`, etc., return `false` if degenerate).

### 3.4 Result

After this fix:
- The fan-shaped artifact disappears. You'll see imagery only over your project zone (and reasonable surrounding context).
- White slivers between projected tiles are gone, because every remaining tile is in the well-behaved part of the projection.
- Tile-fetch volume drops dramatically when the user is zoomed out — no more downloading tiles for Greenland.

### 3.5 Caveat — this is *not* the same as `TryClipWebMercatorBounds`

That method only clips to ±20M meters in Web Mercator space. It's still required (it prevents projection math from going wild at the antimeridian) — keep it. The new clipping happens **earlier**, in *project CRS space*, and is much tighter.

---

## 4. Fix 2

### Parent-Tile Placeholder ("Blurry Cached Version")

**Goal.** When a tile at the requested zoom isn't loaded yet, draw a scaled-up sub-region of a parent tile (lower zoom level) so the user always sees *something*.

**File.** `XyzLiveTileRenderLayer.cs`

### 4.1 Concept

Tile pyramids have a parent-child relationship. A tile at `(z, x, y)` covers the same world area as four tiles at `(z+1, 2x, 2y)`, `(z+1, 2x+1, 2y)`, `(z+1, 2x, 2y+1)`, `(z+1, 2x+1, 2y+1)`. Going *up* the pyramid from a child:

- Parent at `z-1`: tile `(x/2, y/2)`. The child occupies one quadrant of the parent — a 128×128 pixel region of the 256×256 parent (assuming standard tile size).
- Grandparent at `z-2`: tile `(x/4, y/4)`. The child occupies a 64×64 region of the 256×256 grandparent.

So if a tile `(z, x, y)` is not in `_tileCache`, walk up until you find one that *is*, and draw the appropriate sub-region scaled up to the child's full destination rectangle.

### 4.2 Where to add it

Inside `DrawVisibleTiles`, replace the `continue` with a fallback lookup:

```csharp
// Pseudocode for the new fallback path:
if (!_tileCache.TryGetValue(key, out Bitmap? bitmap))
{
    // Not loaded. Walk up the parent chain.
    if (TryGetParentPlaceholder(key, maxLevelsUp: 4,
                                 out Bitmap parentBitmap,
                                 out RectangleF parentSourceRect))
    {
        // Draw the parent sub-region stretched into the child's dest rect.
        DrawBitmapRegion(graphics, parentBitmap, destination, parentSourceRect);
        // NOTE: we don't TouchTile() the parent — it's a fallback, not a hit.
    }
    // If no parent found either, leave the area transparent.
    // The pan buffer below it (from RasterDeferredRenderer) often still covers it.
    continue;   // skip to next tile
}
```

### 4.3 The placeholder lookup method

Pseudocode:

```csharp
private bool TryGetParentPlaceholder(
    TileKey child,
    int maxLevelsUp,
    out Bitmap bitmap,
    out RectangleF sourceRect)
{
    bitmap = null!;
    sourceRect = default;

    for (int levels = 1; levels <= maxLevelsUp; levels++)
    {
        int parentZ = child.Z - levels;
        if (parentZ < 0) return false;

        int factor = 1 << levels;          // 2, 4, 8, 16
        int parentX = child.X / factor;
        int parentY = child.Y / factor;

        TileKey parentKey = new TileKey(parentZ, parentX, parentY);
        if (!_tileCache.TryGetValue(parentKey, out Bitmap? cached))
            continue;

        // Compute which sub-region of the parent maps to the child.
        int subSize = TilePixelSize / factor;     // 128, 64, 32, 16
        int offsetX = (child.X % factor) * subSize;
        int offsetY = (child.Y % factor) * subSize;

        bitmap = cached;
        sourceRect = new RectangleF(offsetX, offsetY, subSize, subSize);
        return true;
    }

    return false;
}
```

### 4.4 Drawing-mode hint

When the source rect is small (say `subSize <= 32`), the upscaling is blurry by definition. **Don't fight it** — switching to `InterpolationMode.HighQualityBicubic` for placeholder tiles makes the blur look intentional and smooth, instead of pixelated. Real tiles still use `NearestNeighbor` for crispness.

You can pass an extra `bool isPlaceholder` parameter to `DrawBitmapRegion` and toggle interpolation mode there.

### 4.5 Result

- During fast zoom-in, areas show the parent's blurry version *immediately*, sharpening to the real tile when the HTTP fetch lands.
- Pan into a new area: the previous viewport's tiles act as parents for the new viewport's children. No white squares.
- Combined with the `RasterDeferredRenderer` pan buffer, the user always sees *something* during navigation.

---

## 5. Fix 3

### Skip Re-Projection When Viewport Hasn't Changed

**Goal.** Stop doing reprojection work on paints where the viewport is identical to the previous paint.

**File.** `XyzLiveTileRenderLayer.cs`

### 5.1 Concept

`RenderVisible` runs reprojection, tile-range computation, CTS cancellation+recreation, and debounce-timer arming on **every** call. But `MapCanvasRenderer.Render` is called for any reason that triggers `Invalidate`:

- Status bar update
- Layer panel selection change
- Window focus change
- Tooltip render
- Any internal repaint Windows decides it needs

In all those cases the viewport is unchanged. The work is pure waste.

### 5.2 The check

You already have `_lastWebMercatorBounds` and `_lastZoom` fields. They're set inside the lock at the end of the bounds-computation phase. Add a *fast comparison at the top* of `RenderVisible`:

```csharp
public bool RenderVisible(...)
{
    // Snapshot the inputs needed for an early-out check.
    if (!IsVisible || !TryIntersects(WorldBounds, visibleWorldBounds))
        return false;

    // ── Fast path: viewport unchanged → skip projection work ────────────
    bool viewportUnchanged;
    int cachedZoom;
    RectangleD cachedBounds;
    lock (_renderSync)
    {
        cachedBounds = _lastWebMercatorBounds;
        cachedZoom = _lastZoom;
    }

    // Compute the proposed new bounds without committing them.
    if (!TryComputeWebMercatorRequest(visibleWorldBounds, engine,
                                      out RectangleD proposedBounds,
                                      out int proposedZoom))
        return false;

    viewportUnchanged =
        proposedZoom == cachedZoom &&
        BoundsAreEqual(proposedBounds, cachedBounds, epsilon: 0.5);

    if (viewportUnchanged)
    {
        // Just draw what we have. No reprojection, no CTS churn, no debounce reset.
        return DrawVisibleTilesUsingCachedRange(graphics, engine,
                                                visibleWorldBounds, cancellationToken);
    }

    // ── Slow path: viewport changed → full work ─────────────────────────
    // (existing RenderVisible body, unchanged)
}
```

### 5.3 Two new helpers

`TryComputeWebMercatorRequest` is a refactor — extract the current reprojection + zoom-selection logic from `RenderVisible` into a pure function that returns the *proposed* bounds and zoom **without** mutating any fields. The slow path then uses the same function and *commits* its results.

`BoundsAreEqual` is float comparison with epsilon — `Math.Abs(a.Left - b.Left) < epsilon` etc.

`DrawVisibleTilesUsingCachedRange` recomputes the `TileRange` from `_lastZoom` + `_lastWebMercatorBounds` (cheap — just integer math) and calls the existing `DrawVisibleTiles`.

### 5.4 What this saves

On a paint where the viewport is unchanged:
- **No** GDAL `CoordinateTransformation.TransformPoint` calls (slow — these are P/Invoke into native code)
- **No** `CancellationTokenSource` allocation/cancellation/disposal
- **No** debounce timer rearm (so the in-flight tile fetch isn't disturbed)
- Just the tile-cache lookups and `DrawImage` calls — which is what you actually want.

### 5.5 Result

CPU usage during static viewing drops to near-zero. Pan and zoom feel snappier because the *meaningful* work happens once per real change, not once per paint.

---

## 6. Fix 4

### Branch the DrawImage Path on Project CRS

**Goal.** Use the fast `DrawImage(bitmap, Rectangle, ...)` overload when the project is Web Mercator (no warping needed). Use the slow parallelogram form only when reprojection genuinely produces non-axis-aligned tile bounds.

**File.** `XyzLiveTileRenderLayer.cs`, method `DrawBitmapRegion`.

### 6.1 The two GDI+ overloads

| Overload | Speed | When to use |
|---|---|---|
| `DrawImage(bitmap, Rectangle dest, int sx, int sy, int sw, int sh, GraphicsUnit, ImageAttributes)` | **Fast.** Hardware-accelerated blit on most GPUs. | Destination is axis-aligned and integer-pixel. |
| `DrawImage(bitmap, PointF[3] dest, RectangleF source, GraphicsUnit, ImageAttributes)` | **Slow.** Software path with affine transform. | Destination is a parallelogram (rotated/skewed). |

You're using the slow one in all cases. That's wrong when the project is Web Mercator — there's no rotation, just translation and uniform scale.

### 6.2 The branch

In `DrawBitmapRegion`:

```csharp
private void DrawBitmapRegion(
    Graphics graphics,
    Bitmap bitmap,
    RectangleF destination,
    RectangleF source)
{
    // Web Mercator project → tile bounds are axis-aligned in screen space.
    // Use the fast integer-rectangle path.
    if (_projectIsWebMercator)
    {
        Rectangle dest = CreateIntegerDestinationRectangle(destination);
        graphics.DrawImage(bitmap, dest,
            source.X, source.Y, source.Width, source.Height,
            GraphicsUnit.Pixel,
            _opacityImageAttributes);
        return;
    }

    // Non-mercator project → tiles may be skewed/rotated after reprojection.
    // Fall through to the parallelogram form.
    // (existing 3-point DrawImage code)
}
```

You're partially doing this already at the bottom of the method — but the condition doesn't gate on `_projectIsWebMercator`. Make that the first branch.

### 6.3 Why this matters

In your specific case with imagery from Esri (Web Mercator) into a UTM project:
- The tile bounds **are** non-axis-aligned after reprojection. So you DO need the parallelogram form for some tiles.
- BUT — once Fix 1 is in place, you've clipped to the project's valid zone, where reprojection is *nearly* affine. You can take this further: compute the deviation between the 4 corners' average rotation and skip the parallelogram path if it's below a threshold.

Pseudocode for the deviation check:

```csharp
private static bool IsNearlyAxisAligned(PointD[] reprojectedCorners,
                                         double tolerancePixels)
{
    // Check if the parallelogram differs from a rectangle by less than tolerance.
    // Compare top edge length to bottom edge length, left to right, etc.
    double topWidth = Distance(reprojectedCorners[0], reprojectedCorners[1]);
    double bottomWidth = Distance(reprojectedCorners[3], reprojectedCorners[2]);
    return Math.Abs(topWidth - bottomWidth) < tolerancePixels;
}
```

If the tile is nearly axis-aligned, draw with the integer rectangle overload using the bounding box of the 4 corners. If it's significantly skewed, use the parallelogram overload.

### 6.4 Result

For Web Mercator projects: **5–10× faster tile draw.** For UTM-style projects: still faster on tiles in the central part of the zone (which is most tiles after Fix 1).

---

## 7. Fix 5

### Async Pan-End Refresh

**Goal.** Stop freezing the UI thread when the user releases the pan button.

**File.** `MapCanvasControl.cs`, method `canvasSurface_MouseUp`.

### 7.1 The current code

```csharp
if (_isPanning)
{
    _isPanning = false;
    canvasSurface.Capture = false;
    _currentMouseWorld = _engine.ScreenToWorld(e.Location);
    _panStartWorld = null;
    _totalPanDelta = PointF.Empty;
    RefreshRasterCacheForCurrentViewImmediately();   // ← UI thread, synchronous
    UpdateCanvasCursor();
    RequestRender();
}
```

`RefreshRasterCacheForCurrentViewImmediately` calls `_rasterDeferredRenderer.RenderNow(...)` which:
1. Allocates a new bitmap (canvas size, 32bpp).
2. For each raster layer, calls `RenderVisible` into that bitmap.
3. Swaps the cache.

For a screen with 50+ visible tiles, that's 50+ `DrawImage` calls plus the reprojection overhead. On the UI thread. The user feels it as a jank on every pan-release.

### 7.2 The fix

Replace it with the async version:

```csharp
if (_isPanning)
{
    _isPanning = false;
    canvasSurface.Capture = false;
    _currentMouseWorld = _engine.ScreenToWorld(e.Location);
    _panStartWorld = null;
    _totalPanDelta = PointF.Empty;
    RefreshRasterCacheForCurrentViewAsync();   // ← background thread
    UpdateCanvasCursor();
    RequestRender();
}
```

### 7.3 Why this is safe

During the async refresh, `_rasterCacheRefreshPending == true`, so `ShouldDeferDirectRasterRendering` is `true`, so `GetRasterRenderFrame()` returns the existing pan-buffer or zoom-frame. The user sees the *correctly translated* old bitmap until the new one arrives. There is no visual gap.

When the async render completes, `RequestRender` is called from `BeginInvoke`, the new cache becomes the source frame, and the screen updates seamlessly.

### 7.4 Caveat — keep `RefreshRasterCacheForCurrentViewImmediately` for one case

It's still useful when the canvas size changes (resize). In that case, the pan buffer is the wrong size and you need a synchronous refresh before the next paint, or the user sees garbage briefly. Keep the method, but only call it from `canvasSurface_Resize`.

### 7.5 Result

Pan release feels weightless. No frame drops.

---

## 8. Fix 6

### Pre-warm the Project Tile Bounds Cache

**Goal.** Eliminate the cost of `_projectTileBoundsCache` misses for low zoom levels (which are visited every time the user zooms out to overview).

**File.** `XyzLiveTileRenderLayer.cs`

### 8.1 Concept

`GetProjectTileBounds(TileKey)` looks up `_projectTileBoundsCache`. On miss, it does a GDAL `TransformBounds` (slow). For zooms 0 through 6, the total tile count is:

| Zoom | Tile count | Cumulative |
|---|---|---|
| 0 | 1 | 1 |
| 1 | 4 | 5 |
| 2 | 16 | 21 |
| 3 | 64 | 85 |
| 4 | 256 | 341 |
| 5 | 1024 | 1365 |
| 6 | 4096 | 5461 |

5,461 entries × ~80 bytes per `RectangleD` = **~430 KB.** Trivial.

### 8.2 The pre-warm

In the constructor, after the coordinate transformations are set up, run a one-time loop:

```csharp
// Pseudocode
for (int z = 0; z <= 6; z++)
{
    int matrixSize = 1 << z;
    for (int x = 0; x < matrixSize; x++)
    {
        for (int y = 0; y < matrixSize; y++)
        {
            TileKey key = new TileKey(z, x, y);
            // Compute and cache. Don't trim during pre-warm.
            _ = ComputeProjectTileBoundsUncached(key);
        }
    }
}
```

`ComputeProjectTileBoundsUncached` is the body of the existing `GetProjectTileBounds` minus the dictionary lookup and trim — just the math.

### 8.3 Adjust the trim policy

Right now `TrimProjectBoundsCache` evicts to keep the cache size bounded. That's correct for high zoom levels (where tile count explodes), but the pre-warmed low-zoom entries should be **pinned** — never evicted.

Two options:

1. Use two dictionaries: `_pinnedBoundsCache` (zooms 0–6, never trimmed) and `_lazyBoundsCache` (zooms 7+, LRU-trimmed). Lookup checks pinned first.

2. Single dictionary with a separate `HashSet<TileKey> _pinnedKeys`. `TrimProjectBoundsCache` skips keys in the pinned set when choosing eviction candidates.

Option 1 is cleaner.

### 8.4 Result

Zooming out to overview is instant. No more "GDAL transform thinking" pause when the user hits zoom-extents.

### 8.5 Memory cost

~430 KB per layer. If the user has 5 XYZ layers active, ~2 MB total. Acceptable for a desktop application.

---

## 9. Testing Checklist

Apply fixes in order. After each one, run through this checklist before moving on.

### After Fix 1 (clip to valid extent)

- [ ] Open a project with UTM CRS and load Esri World Imagery.
- [ ] Confirm: imagery covers only the project area + reasonable surrounding context. No fan shape extending across continents.
- [ ] Confirm: no white slivers between adjacent tiles in the rendered area.
- [ ] Pan to the edge of your project zone — imagery should stop cleanly at the clip boundary.
- [ ] Pan beyond the edge — should see the canvas background, not garbled imagery.

### After Fix 2 (parent placeholder)

- [ ] Load a fresh project (no disk cache for the tile region).
- [ ] Zoom in 5 levels rapidly. Confirm: never see white squares — always blurry-then-sharp transitions.
- [ ] Pan into an unloaded area at high zoom. Confirm: blurry parent visible, sharpens as tiles load.

### After Fix 3 (viewport-unchanged fast path)

- [ ] Open the project, leave it idle for 30 seconds with the Performance Profiler attached.
- [ ] Confirm: CPU usage is near zero. No GDAL transform calls happening.
- [ ] Click another window and back — no spike from refocus-triggered paint.

### After Fix 4 (DrawImage branch)

- [ ] Profile a pan in Web Mercator project mode. Compare DrawImage time vs before.
- [ ] Profile a pan in UTM project mode. Confirm: still works correctly (parallelogram path engaged for skewed tiles).
- [ ] Visual smoke test: no missing tiles, no flicker, no tearing.

### After Fix 5 (async pan-end)

- [ ] Pan a viewport with 50+ visible tiles. Release.
- [ ] Confirm: zero perceptible UI freeze on release. The pan-buffer image stays visible while the refresh runs.
- [ ] Pan-release-pan-release rapidly. Confirm: no race-condition crashes, no stale frames.

### After Fix 6 (pre-warm)

- [ ] Open project. Wait 1 second for pre-warm to finish (or move to constructor body if blocking is unacceptable, otherwise spawn a background thread).
- [ ] Hit zoom-to-extents. Confirm: instant render with no stutter.
- [ ] Memory check: layer construction adds ~500 KB to working set. Acceptable.

---

## 10. Performance Targets

After all six fixes, measure against these targets on a typical project (10–50 raster layers, modest density):

| Operation | Target | How to measure |
|---|---|---|
| Idle (no pan/zoom) CPU usage | < 0.5% | Task Manager, 5-second average |
| Pan drag latency (mouse move → screen update) | < 16 ms | Stopwatch in `OnMouseMove`, log to Debug.WriteLine |
| Wheel zoom step time | < 50 ms | Stopwatch around `ZoomAtPoint` to first paint |
| Pan release → cache refresh complete | Async — visible jank zero, refresh complete < 250 ms | Stopwatch around `RefreshRasterCacheForCurrentViewAsync` |
| Tile cache hit rate during steady viewing | > 95% | Add counter in `_tileCache.TryGetValue` |
| Memory growth on 10-minute pan/zoom session | < 50 MB | Working set in Task Manager |

### 10.1 If you don't hit the targets

Don't escalate to SkiaSharp yet. Instead:

1. Profile with **dotTrace** or **Visual Studio Performance Profiler** — find the actual hot spot.
2. Check whether you're allocating bitmaps with `Format32bppArgb` instead of `Format32bppPArgb` anywhere. PArgb is 25–35% faster in `DrawImage`. Search the codebase for `new Bitmap(...` and verify every one passes `PixelFormat.Format32bppPArgb`.
3. Confirm `OptimizedDoubleBuffer | AllPaintingInWmPaint | UserPaint | ResizeRedraw` are set on `canvasSurface`.

---

## 11. What Not To Change

These are deliberate parts of your architecture. Don't touch them while doing the fixes above — they'll cause confusion if you mix concerns.

### 11.1 Don't change the threading model

The current model (UI thread for paint, thread pool for HTTP fetches and decode, `BeginInvoke` to post tile-ready callbacks) is correct. Don't introduce additional threads or change how cancellation flows.

### 11.2 Don't change the cache eviction strategy mid-fix

`MaxCachedTiles = 512` and the LRU policy work for typical usage. Tune *after* the fixes are in and you're profiling, not during.

### 11.3 Don't add a third bitmap cache layer

You already have:
- `_tileCache` (decoded per-tile bitmaps)
- `_rasterCache` (full canvas composite)
- `_panBuffer` (snapshot for pan)

Adding more will eat RAM without speed gain.

### 11.4 Don't migrate to SkiaSharp during these fixes

The roadmap mentions SkiaSharp eventually. It's a larger project — different paint pipeline, different bitmap types, different threading. Do that as a separate phase. The fixes in this guide get GDI+ to acceptable performance for RePlot's needs.

### 11.5 Don't change the project CRS handling at the canvas level

The canvas takes the project CRS as given and reprojects on the fly. This is the right separation. CRS conversion is a *project setup* concern, not a *render* concern.

---

## 12. Reference

### 12.1 Files modified

| File | Changes |
|---|---|
| `XyzLiveTileRenderLayer.cs` | Fixes 1, 2, 3, 4, 6 |
| `MapCanvasControl.cs` | Fix 5 |

### 12.2 Methods added

These are new methods you'll write while applying the fixes. Names are suggestions — match your codebase style.

| Method | Fix | Purpose |
|---|---|---|
| `ExpandExtent` | 1 | Returns a rectangle larger by a factor in each direction. |
| `TryGetIntersection` | 1 | Standard rectangle intersection helper. |
| `TryGetParentPlaceholder` | 2 | Walks up the tile pyramid to find a cached ancestor. |
| `TryComputeWebMercatorRequest` | 3 | Pure function — proposes new bounds and zoom without mutating state. |
| `BoundsAreEqual` | 3 | Float comparison with epsilon. |
| `DrawVisibleTilesUsingCachedRange` | 3 | Paints using last-known tile range, no recomputation. |
| `IsNearlyAxisAligned` | 4 | Checks if reprojected corners deviate from a rectangle. |
| `ComputeProjectTileBoundsUncached` | 6 | Pure projection math; no cache lookup. |

### 12.3 Methods modified

| Method | Fix | What changes |
|---|---|---|
| `RenderVisible` | 1, 3 | Adds early clip to valid project extent. Adds fast path for unchanged viewport. |
| `OnDebounceElapsed` | 1 | Same clipping as `RenderVisible`. Don't fetch tiles outside the project zone. |
| `DrawVisibleTiles` | 2 | Falls back to `TryGetParentPlaceholder` instead of skipping. |
| `DrawBitmapRegion` | 4 | Branches on `_projectIsWebMercator` to choose fast vs slow `DrawImage` overload. |
| `canvasSurface_MouseUp` (in `MapCanvasControl`) | 5 | Replaces immediate refresh with async refresh. |
| Constructor | 1, 6 | Computes `_validProjectExtent`, pre-warms low-zoom bounds cache. |

### 12.4 Existing methods you should re-read before starting

These are the parts of the codebase you'll be touching. Read them once end-to-end before making any change so you have full context:

1. `XyzLiveTileRenderLayer.RenderVisible` — the top of the slow path.
2. `XyzLiveTileRenderLayer.DrawVisibleTiles` — where placeholder fallback goes.
3. `XyzLiveTileRenderLayer.GetProjectTileBounds` — what you're caching.
4. `XyzLiveTileRenderLayer.TryClipWebMercatorBounds` — keep this; it's the global bound clip.
5. `RasterDeferredRenderer.RenderNow` and `BeginPan` — to understand what runs synchronously vs async.
6. `MapCanvasControl.GetRasterRenderFrame` — to understand frame priority during interaction.
7. `MapCanvasControl.ShouldDeferDirectRasterRendering` — the gate that prevents `RenderVisible` during interactive nav.

---

## Closing Notes

These six fixes address the symptoms in your screenshot completely. They preserve your existing architecture — the layered renderer, the deferred composite, the cache hierarchy — and improve it surgically.

If you want to go further after this:
- Look at adding `WorldBounds`-based culling at the `RasterDeferredRenderer` level so it skips compositing layers that are entirely off-screen.
- Consider a per-tile **decoded-bitmap pool** (reuse buffers instead of `new Bitmap` on every decode) — gives another 10–15% on tile-heavy panning.
- Eventually move to SkiaSharp for hardware-accelerated compositing; only worth it when you have 100+ raster layers active simultaneously.

Apply Fixes 1, 2, 3 first — they cover 80% of the perceived improvement. Then 4, 5, 6 for the polish.
