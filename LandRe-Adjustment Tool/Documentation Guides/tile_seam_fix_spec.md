# Tile Seam / Gap Fix — Codex Task Specification

## Summary
The map canvas renders visible white grid-line gaps between raster tiles.
All root causes are identified below with exact file, method, line description, and the required change.
Apply every fix. Do not change any other logic.

---

## Fix 1 — `AlignDestinationToPixelGrid` adds a –0.5 / +0.5 gap offset (CRITICAL)

**Files:** `MbTilesRenderLayer.cs` and `RasterRenderLayer.cs`  
**Method:** `AlignDestinationToPixelGrid` (identical copy in both files)

**Current broken code (both files):**
```csharp
private static RectangleF AlignDestinationToPixelGrid(RectangleF destination)
{
    float left   = (float)Math.Floor(destination.Left);
    float top    = (float)Math.Floor(destination.Top);
    float right  = (float)Math.Ceiling(destination.Right);
    float bottom = (float)Math.Ceiling(destination.Bottom);

    return RectangleF.FromLTRB(left - 0.5f, top - 0.5f, right + 0.5f, bottom + 0.5f);
}
```

**Problem:**  
The `–0.5f` on TL and `+0.5f` on BR was intended to close sub-pixel gaps but instead creates a 1-pixel bleed overlap that, combined with `PixelOffsetMode.HighSpeed` and `CompositingMode.SourceOver`, produces a dark/white fringe at every tile seam.  
The `PixelOffsetMode.HighSpeed` (see Fix 2) already shifts pixels by +0.5 internally, so this manual adjustment doubles the offset.

**Required fix (both files — replace method body):**
```csharp
private static RectangleF AlignDestinationToPixelGrid(RectangleF destination)
{
    float left   = (float)Math.Floor(destination.Left);
    float top    = (float)Math.Floor(destination.Top);
    float right  = (float)Math.Ceiling(destination.Right);
    float bottom = (float)Math.Ceiling(destination.Bottom);

    return RectangleF.FromLTRB(left, top, right, bottom);
}
```

---

## Fix 2 — `PixelOffsetMode.HighSpeed` causes systematic 0.5 px tile shift

**Files:** `MbTilesRenderLayer.cs` → `RenderVisible()` and `RasterRenderLayer.cs` → `RenderVisible()`

**Current broken code (both files, inside `GraphicsState` block):**
```csharp
graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
```

**Problem:**  
`HighSpeed` shifts every drawn pixel by +0.5 px in both axes relative to the destination rectangle coordinates. When integer-aligned tiles are drawn at adjacent positions, each tile's actual pixels are shifted by 0.5 px, leaving a visible 1-pixel undrawn strip between neighbours.

**Required fix (both files — change the single line):**
```csharp
graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
```

---

## Fix 3 — `CompositingMode.SourceOver` blends transparent tile edges against background

**Files:** `MbTilesRenderLayer.cs` → `RenderVisible()` and `RasterRenderLayer.cs` → `RenderVisible()`

**Current broken code (both files):**
```csharp
graphics.CompositingMode = CompositingMode.SourceOver;
```

**Problem:**  
Tiles decoded as `Format32bppPArgb` have pre-multiplied alpha. `SourceOver` blends edge pixels against the transparent off-screen buffer, which produces a dark/coloured fringe exactly where tile edges meet.

**Required fix (both files):**
```csharp
graphics.CompositingMode = CompositingMode.SourceCopy;
```

---

## Fix 4 — `RasterDeferredRenderer.ConfigureRasterQuality` uses `PixelOffsetMode.HighSpeed`

**File:** `RasterDeferredRenderer.cs`  
**Method:** `ConfigureRasterQuality` (private static)

**Current broken code:**
```csharp
private static void ConfigureRasterQuality(Graphics graphics)
{
    graphics.SmoothingMode       = SmoothingMode.None;
    graphics.InterpolationMode   = InterpolationMode.NearestNeighbor;
    graphics.PixelOffsetMode     = PixelOffsetMode.HighSpeed;   // ← broken
    graphics.CompositingQuality  = CompositingQuality.HighSpeed;
}
```

**Problem:**  
The deferred renderer composites all raster layers into the off-screen `_rasterCache` bitmap. Using `HighSpeed` here shifts every tile by 0.5 px inside the cache bitmap itself, so the seams are baked into the cached image and persist even when the cache is blitted during pan/zoom.

**Required fix:**
```csharp
private static void ConfigureRasterQuality(Graphics graphics)
{
    graphics.SmoothingMode       = SmoothingMode.None;
    graphics.InterpolationMode   = InterpolationMode.NearestNeighbor;
    graphics.PixelOffsetMode     = PixelOffsetMode.HighQuality;  // ← fixed
    graphics.CompositingQuality  = CompositingQuality.HighSpeed;
    graphics.CompositingMode     = CompositingMode.SourceCopy;   // ← add this line
}
```

---

## Fix 5 — `DrawRasterFrame` in `MapCanvasRenderer` uses `PixelOffsetMode.HighSpeed`

**File:** `MapCanvasRenderer.cs`  
**Method:** `DrawRasterFrame` (private static)

**Current broken code:**
```csharp
graphics.SmoothingMode       = SmoothingMode.None;
graphics.InterpolationMode   = InterpolationMode.NearestNeighbor;
graphics.PixelOffsetMode     = PixelOffsetMode.HighSpeed;        // ← broken
graphics.CompositingQuality  = CompositingQuality.HighSpeed;
```

**Problem:**  
When the cached `RasterRenderFrame` bitmap is blitted to the screen during pan/zoom, `HighSpeed` shifts the whole image by 0.5 px, misaligning it with the grid and other vector layers.

**Required fix (change one line):**
```csharp
graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
```

---

## Fix 6 — `MbTilesRenderLayer.DrawBitmapRegion` uses `Rectangle.Round` instead of integer-snapped rect

**File:** `MbTilesRenderLayer.cs`  
**Method:** `DrawBitmapRegion`

**Current broken code (opacity path):**
```csharp
Rectangle destinationRectangle = Rectangle.Round(destination);
graphics.DrawImage(
    bitmap,
    destinationRectangle,
    source.X, source.Y, source.Width, source.Height,
    GraphicsUnit.Pixel,
    _opacityImageAttributes);
```

**Problem:**  
`Rectangle.Round` uses midpoint rounding which can round TL up and BR down, shrinking the tile by 1 pixel on each side and creating a gap. It must use Floor on TL and Ceiling on BR (same as `AlignDestinationToPixelGrid`).

**Required fix:**
```csharp
Rectangle destinationRectangle = new Rectangle(
    (int)Math.Floor(destination.Left),
    (int)Math.Floor(destination.Top),
    (int)Math.Ceiling(destination.Right  - (float)Math.Floor(destination.Left)),
    (int)Math.Ceiling(destination.Bottom - (float)Math.Floor(destination.Top)));
graphics.DrawImage(
    bitmap,
    destinationRectangle,
    source.X, source.Y, source.Width, source.Height,
    GraphicsUnit.Pixel,
    _opacityImageAttributes);
```

---

## Fix 7 — `RasterRenderLayer.DrawBitmap` opacity path uses `Rectangle.Round`

**File:** `RasterRenderLayer.cs`  
**Method:** `DrawBitmap`

**Current broken code (opacity path):**
```csharp
Rectangle destinationRectangle = Rectangle.Round(destination);
```

**Required fix (same pattern as Fix 6):**
```csharp
Rectangle destinationRectangle = new Rectangle(
    (int)Math.Floor(destination.Left),
    (int)Math.Floor(destination.Top),
    (int)Math.Ceiling(destination.Right  - (float)Math.Floor(destination.Left)),
    (int)Math.Ceiling(destination.Bottom - (float)Math.Floor(destination.Top)));
```

---

## Fix 8 — Add `WrapMode.TileFlipXY` to prevent bicubic edge bleed

**Files:** `MbTilesRenderLayer.cs` and `RasterRenderLayer.cs`

In both files, when `_opacityImageAttributes` is built (inside `UpdateOpacityAttributes`), add wrap mode so bicubic sampling does not read outside the tile bitmap boundary:

```csharp
imageAttributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
```

Add this line immediately after `imageAttributes.SetColorMatrix(...)`.

For the no-opacity `DrawImage` calls (parallelogram overload in `DrawBitmapRegion`), wrap the call with a temporary `ImageAttributes` that only sets `WrapMode`:

```csharp
// In MbTilesRenderLayer.DrawBitmapRegion — no-opacity branch:
using var ia = new ImageAttributes();
ia.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
graphics.DrawImage(
    bitmap,
    new[] {
        new PointF(destination.Left,  destination.Top),
        new PointF(destination.Right, destination.Top),
        new PointF(destination.Left,  destination.Bottom)
    },
    source,
    GraphicsUnit.Pixel,
    ia);
```

```csharp
// In RasterRenderLayer.DrawBitmap — no-opacity branch:
using var ia = new ImageAttributes();
ia.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
graphics.DrawImage(bitmap, destination, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
```

---

## Change Summary Table

| # | File | Method | Change |
|---|------|--------|--------|
| 1 | MbTilesRenderLayer.cs | AlignDestinationToPixelGrid | Remove `–0.5f` / `+0.5f` offsets |
| 1 | RasterRenderLayer.cs | AlignDestinationToPixelGrid | Same |
| 2 | MbTilesRenderLayer.cs | RenderVisible | `HighSpeed` → `HighQuality` PixelOffsetMode |
| 2 | RasterRenderLayer.cs | RenderVisible | Same |
| 3 | MbTilesRenderLayer.cs | RenderVisible | `SourceOver` → `SourceCopy` CompositingMode |
| 3 | RasterRenderLayer.cs | RenderVisible | Same |
| 4 | RasterDeferredRenderer.cs | ConfigureRasterQuality | `HighSpeed` → `HighQuality`; add `SourceCopy` |
| 5 | MapCanvasRenderer.cs | DrawRasterFrame | `HighSpeed` → `HighQuality` PixelOffsetMode |
| 6 | MbTilesRenderLayer.cs | DrawBitmapRegion | `Rectangle.Round` → Floor/Ceiling rect |
| 7 | RasterRenderLayer.cs | DrawBitmap | `Rectangle.Round` → Floor/Ceiling rect |
| 8 | Both render layers | UpdateOpacityAttributes + DrawBitmapRegion/DrawBitmap | Add `WrapMode.TileFlipXY` to all ImageAttributes and no-opacity DrawImage calls |

---

## Do NOT change
- Tile selection logic, zoom level selection, SQLite queries, GDAL read paths, cache eviction, coordinate transforms, or any other rendering logic not listed above.
