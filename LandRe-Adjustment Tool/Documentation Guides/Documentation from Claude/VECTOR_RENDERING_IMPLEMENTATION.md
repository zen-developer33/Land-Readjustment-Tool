# RePlot — Vector Rendering & Layer Manager Implementation Guide

**Document Purpose:** Complete step-by-step implementation guide for the new vector rendering system  
**Based On:** Old implementation in `UI/MapCanvas/` (DrawingEngine, OptimizedDeferredRenderer, CanvasRenderer, ShapeManager, shapes folder)  
**Target:** New clean implementation — new files, new classes, same proven concepts, all issues fixed  
**Audience:** Developer (you) learning by doing — every decision is explained

---

## Table of Contents

1. [What We Are Building and Why](#1-what-we-are-building-and-why)
2. [Old Implementation — What Works, What Doesn't](#2-old-implementation--what-works-what-doesnt)
3. [New Architecture Overview](#3-new-architecture-overview)
4. [Folder Structure](#4-folder-structure)
5. [Phase 1 — Coordinate Types and Math](#5-phase-1--coordinate-types-and-math)
6. [Phase 2 — Shape Model (IVectorShape, base classes, concrete shapes)](#6-phase-2--shape-model)
7. [Phase 3 — RenderContext and PenCache](#7-phase-3--rendercontext-and-pencache)
8. [Phase 4 — Viewport Engine (MapCanvasEngine)](#8-phase-4--viewport-engine)
9. [Phase 5 — Spatial Index (FeatureSpatialIndex)](#9-phase-5--spatial-index)
10. [Phase 6 — Feature Store (FeatureStore)](#10-phase-6--feature-store)
11. [Phase 7 — Runtime Layer Manager](#11-phase-7--runtime-layer-manager)
12. [Phase 8 — Deferred Renderer (FrameRenderer)](#12-phase-8--deferred-renderer)
13. [Phase 9 — Grid and Overlay Renderer](#13-phase-9--grid-and-overlay-renderer)
14. [Phase 10 — MapCanvasControl (the UserControl)](#14-phase-10--mapcanvascontrol)
15. [Phase 11 — Wiring Layer Manager to Render Pipeline](#15-phase-11--wiring-layer-manager-to-render-pipeline)
16. [Phase 12 — Persistence Bridge (CanvasObject → Shape)](#16-phase-12--persistence-bridge)
17. [Complete File Map](#17-complete-file-map)
18. [What NOT to Do (Lessons from Old Code)](#18-what-not-to-do)
19. [Testing Checklist](#19-testing-checklist)

---

## 1. What We Are Building and Why

### The Goal

A 2D workspace canvas for land readjustment and replotting. Not a generic CAD clone. Specifically:

- Drawing, moving, dividing, and creating land parcels as polygons
- Snapping to existing geometry edges and vertices (like AutoCAD object snap)
- Smooth pan and zoom from entire city extent down to individual parcel vertex level
- Layer system to separate: Existing Cadastre, Replotted Parcels, Roads, Boundary, Overlays
- All geometry connected to the database via `CanvasObject` entities

### Why We Are Writing New Files (Not Editing Old)

The old `DrawingCanvasControl` path has architectural problems that are too entangled to fix by editing:

- `DrawingEngine` is used everywhere but has unused dead fields (`viewoffsetX`, `viewoffsetY`)
- `CanvasRenderer.Render()` has a direct shape loop that duplicates what `OptimizedDeferredRenderer` does
- `RTreeSpatialIndex` still rebuilds on every single `Insert()` call (confirmed in the actual source)
- Every shape creates `new Pen()` inside its `Draw()` — no pooling
- No coordinate clamping before GDI+ calls (causes artifacts at Nepal TM zoom levels)
- The Layer Manager (`frmLayerManager`) is fully built but has zero connection to the render pipeline
- `ShapeRepository` is a TODO stub — shapes are not persisted

Writing new files gives you a clean starting point without breaking what currently works in the old path. Once the new path is stable, you migrate.

---

## 2. Old Implementation — What Works, What Doesn't

### What Works (Keep These Concepts)

| Old Class | Concept to Keep | Why It Works |
|---|---|---|
| `DrawingEngine` | Y-axis flip for screen coordinates | GDI+ Y increases downward, world coords Y increases upward. Flip is correct. |
| `DrawingEngine.ZoomAtPoint()` | Zoom toward cursor | Preserves world point under cursor. Industry standard. |
| `OptimizedDeferredRenderer` | Three-tier rendering strategy | < 1K shapes = cache, 1K–5K = lower quality cache, >5K = direct draw with LOD |
| `OptimizedDeferredRenderer.BeginPan()` | Snapshot bitmap, shift during drag | Zero re-render cost during pan. AutoCAD does the same. |
| `CanvasRenderer` | Pre-allocated pens in `InitializeResources()` | Prevents GC pressure. Correct pattern. |
| `CanvasRenderer.RenderGrid()` | Adaptive grid with hysteresis | Min/max pixel bands prevent grid jumping. Good UX. |
| `ShapeManager.BulkAddShapes()` | Bulk insert then single rebuild | Correct for imports. |
| `UndoRedoManager` | Command pattern, not state snapshots | O(k) memory instead of O(n×m). |
| `PolylineShape.Draw()` | Validate in double, then convert to float | Prevents NaN from crashing the render loop. |

### What Doesn't Work (Problems to Fix in New Code)

| Old Problem | File | Root Cause | Fix in New Code |
|---|---|---|---|
| R-Tree rebuilds on every `Insert()` | `RTreeSpatialIndex.cs` | `Rebuild()` called inside `Insert()` — O(n²) | Dirty flag: only rebuild before a Query |
| `new Pen()` inside every `Draw()` call | All shape files | No shared resource pool | `RenderContext` carries pre-built `PenCache` |
| No coordinate clamping | `PolylineShape.cs`, others | GDI+ silently corrupts at values > 16M pixels | `ClampToGdi()` applied before every `PointF` cast |
| Fixed `0.25f` pen width at all zoom levels | All shapes | Never adapts to zoom | Zoom-adaptive pen width in `RenderContext` |
| `CanvasRenderer.Render()` has its own shape loop | `CanvasRenderer.cs` | Two separate render paths diverge | `CanvasRenderer` draws NO shapes — only grid, axes, overlays |
| Pan buffer no padding — tearing at edges | `OptimizedDeferredRenderer.cs` | Cache bitmap is exactly viewport size | 100px padding on all 4 sides |
| `frmLayerManager` result never reaches renderer | `DrawingCanvasControl.cs` | No event/bridge between manager and renderer | `RuntimeLayerManager` fires event, renderer subscribes |
| `ShapeManager` has no layer filtering | `ShapeManager.cs` | `QueryShapesInBound()` ignores `LayerName` visibility | `FeatureStore.Query(viewport, layerIds[])` filters by layer |
| Two viewport engines (`DrawingEngine` + `MapCanvasEngine`) | Multiple files | Parallel implementations, will diverge | New path uses `MapCanvasEngine` only. `DrawingEngine` frozen. |
| `viewoffsetX` / `viewoffsetY` dead properties | `DrawingEngine.cs` | Never used, confusing | Not carried forward |
| `ShapeRepository` is a TODO stub | `ShapeRepository.cs` | Never implemented | `CanvasObjectRepository` is fully implemented |

---

## 3. New Architecture Overview

### How Data Flows (Read This Before Anything Else)

```
DATABASE (SQLite)
    tblCanvasLayers  ──→  RuntimeLayerManager  ──→  Ordered list of visible layers
    tblCanvasObjects ──→  CanvasObjectRepository ──→  FeatureStore (in-memory shapes)
                                                           │
                                                           ▼
                                                    FeatureSpatialIndex (R-Tree)
                                                           │
                              MapCanvasEngine ────────────→│ Query(viewport, layerIds[])
                              (zoom/pan math)              │
                                                           ▼
                                                     FrameRenderer
                                                     (bitmap cache + pan buffer)
                                                           │
                              RenderContext ──────────────→│ (pens, brushes, zoom)
                                                           │
                                                           ▼
                                                    GDI+ DrawLines/DrawPolygon
                                                           │
                              GridOverlayRenderer ─────────│ (grid, axis, snap glyph, HUD)
                                                           │
                                                           ▼
                                                  MapCanvasControl (UserControl)
                                                  (event handling only — no math, no drawing)
```

### Separation of Responsibilities (Hard Rules)

| Class | Does | Does NOT do |
|---|---|---|
| `MapCanvasEngine` | Coordinate math (world↔screen, zoom, pan) | Rendering, GDI+, shapes |
| `FrameRenderer` | Bitmap cache, pan buffer, LOD | Grid, overlays, snap glyphs |
| `GridOverlayRenderer` | Grid lines, axis markers, HUD, snap glyph | Shapes, pan buffer |
| `FeatureStore` | Stores shapes, delegates spatial queries | Rendering, coordinate math |
| `FeatureSpatialIndex` | R-Tree spatial queries | Shape logic, rendering |
| `RuntimeLayerManager` | Layer visibility/order/style state | Rendering, persistence |
| `MapCanvasControl` | Mouse events, keyboard, wiring | Any math, any rendering |
| `RenderContext` | Carries pooled pens/brushes for current render | Nothing — it's a data carrier |

---

## 4. Folder Structure

Create these folders. Every new file goes here. Do not put new files in the old `DrawingCanvas` or `UI/MapCanvas/` folders.

```
UI/
  MapCanvas2/                         ← New folder. All new code lives here.
    Core/
      MapCanvasEngine.cs              ← Viewport math (zoom, pan, world↔screen)
      FeatureStore.cs                 ← Shape storage + layer-aware queries
    SpatialIndex/
      FeatureSpatialIndex.cs          ← R-Tree wrapper with dirty flag
    Models/
      Shapes/
        IVectorShape.cs               ← Interface for all shapes
        VectorShape.cs                ← Abstract base class
        PolylineFeature.cs            ← Closed/open polyline (parcels, roads)
        LineFeature.cs                ← Single segment line
        PointFeature.cs               ← Point marker
        TextFeature.cs                ← Label/annotation
      Snapping/
        (keep existing snap classes — they are fine)
    Rendering/
      RenderContext.cs                ← Pooled pens/brushes, zoom-adaptive width
      PenCache.cs                     ← Dictionary<(Color,float), Pen> pool
      BrushCache.cs                   ← Dictionary<Color, SolidBrush> pool
      FrameRenderer.cs                ← Bitmap cache + pan buffer (replaces OptimizedDeferredRenderer)
      GridOverlayRenderer.cs          ← Grid + axis + snap glyph (replaces CanvasRenderer grid code)
    Layers/
      RuntimeLayer.cs                 ← In-memory layer state (from CanvasLayer entity)
      RuntimeLayerManager.cs          ← Manages ordered visible layers, fires events
      LayerStyleResolver.cs           ← Merges layer defaults + object overrides into final style
    Services/
      GeometryShapeMapper.cs          ← NTS Geometry ↔ IVectorShape (persistence bridge)
    Controls/
      MapCanvasControl.cs             ← UserControl: events only, no math, no drawing
```

---

## 5. Phase 1 — Coordinate Types and Math

### What to Know First

Your existing `PointD`, `RectangleD`, and `DrawingEngine` coordinate system are correct. Keep the same math. The Y-axis flip is the critical insight: in world coordinates (Nepal TM), Y increases going north (up). In screen coordinates (GDI+), Y increases going down. The transform is:

```
screenY = canvasHeight - ((worldY - viewOffsetY) * zoomScale)
```

The `viewOffset` is the **world coordinate** of screen point (0, 0). This is the correct mental model.

### Why You Keep `PointD` Instead of `PointF`

You work in Nepal TM coordinates — values like `X = 383421.5`, `Y = 3023187.4`. A `float` has only 7 significant digits. That gives you precision down to ~0.01m at these coordinates, which starts to matter at high zoom. Keep `double` for all world-space operations. Only convert to `float` at the very last moment before passing to GDI+.

### `MapCanvasEngine.cs` — What Changes from `DrawingEngine`

The math stays identical. What changes:

1. Remove the dead `viewoffsetX` / `viewoffsetY` public float properties — they were never used
2. Add `GetViewBounds()` returning `RectangleD` (same as old `GetViewportBounds()` — rename only)
3. The engine does NOT call `Invalidate()` — it only changes state. The caller decides when to repaint.

**Key methods (same logic as old, just cleaner):**

```csharp
PointD WorldToScreen(PointD worldPt)
PointD ScreenToWorld(PointD screenPt)
void ZoomAtScreenPoint(Point screenPt, double factor)
void PanBy(double deltaX, double deltaY)        // in world units
RectangleD GetViewBounds()
```

**What you learn here:** The engine is a pure math object. It holds `_zoomScale` and `_viewOffset`. Every other class is a consumer of these two values. When you understand that, the whole architecture makes sense.

---

## 6. Phase 2 — Shape Model

### Why New Shape Classes (Not Editing Old Ones)

The old shapes have `Draw(Graphics g, Func<PointD, PointD> worldToScreen)`. This forces each shape to create its own GDI+ pen. The new signature is:

```csharp
void Draw(Graphics g, Func<PointD, PointD> worldToScreen, RenderContext ctx)
```

The `RenderContext` carries the pen. The shape never allocates GDI+ resources. This is the single biggest performance fix.

### `IVectorShape.cs`

```csharp
public interface IVectorShape
{
    Guid Id { get; }
    string LayerName { get; set; }
    bool IsVisible { get; set; }
    bool IsSelected { get; set; }
    bool IsLocked { get; set; }

    // Geometry
    RectangleD GetBoundingBox();
    bool ContainsPoint(PointD worldPoint, double tolerance);

    // Rendering — RenderContext carries all GDI+ resources
    void Draw(Graphics g, Func<PointD, PointD> worldToScreen, RenderContext ctx);

    // NTS bridge — for persistence and spatial operations
    NetTopologySuite.Geometries.Geometry ToNtsGeometry();

    // Clone — for undo/redo
    IVectorShape Clone();
}
```

**What you learn here:** The `RenderContext` parameter is the key change from old code. The shape receives resources — it doesn't create them. This is Dependency Inversion applied to rendering.

### `VectorShape.cs` — Abstract Base

Contains all shared state: `Id`, `LayerName`, `IsVisible`, `IsSelected`, `IsLocked`.

Also contains the shared utility method that every concrete shape uses:

```csharp
// Called inside every Draw() before any PointF cast
protected static float ClampToGdi(double v)
{
    const double MAX = 1_000_000.0;
    return v > MAX ? (float)MAX : v < -MAX ? (float)-MAX : (float)v;
}

// Called to convert a world point to a safe GDI+ PointF
protected static PointF ToScreenPointF(PointD screenPt)
{
    return new PointF(
        ClampToGdi(Math.Round(screenPt.X)),
        ClampToGdi(Math.Round(screenPt.Y))
    );
}
```

**Why `Math.Round` before clamping?** At integer pixel boundaries, GDI+ renders thin lines cleanly. Sub-pixel positions cause the line to bleed across two pixels, making it look thinner or blurry. Rounding gives you crisp pixel-aligned edges.

**Why clamping?** Nepal TM easting ~383,000. At MAX_ZOOM 50,000, that's `383000 × 50000 = 19.15 billion`. GDI+ max safe coordinate is ~16 million. Without clamping, GDI+ receives an out-of-range float and silently produces garbage output — shapes appear in wrong positions or disappear.

### `PolylineFeature.cs` — The Most Important Shape

This is what a parcel is. A closed `PolylineFeature` with `IsClosed = true`.

The `Draw()` method structure:

```
1. If fewer than 2 vertices → return immediately
2. Convert all world vertices to screen PointF using ToScreenPointF()
3. Validate: if any screen point is still NaN/Infinity → return (should not happen after clamping, but defensive)
4. Get pen from ctx.GetPen(resolvedColor, ctx.AdaptiveLineWidth)
5. g.DrawLines(pen, points)
6. If IsClosed → g.DrawLine(pen, points[last], points[0])
7. If IsClosed and HasFill → g.FillPolygon(ctx.GetBrush(fillColor, transparency), points)
```

**What you learn here:** The shape calls `ctx.GetPen(...)` — it does not create a pen. The `RenderContext` either returns a cached pen or creates and caches it. This is the pooling pattern.

### `LineFeature.cs` and `PointFeature.cs`

Same structure as `PolylineFeature` but simpler. `LineFeature` has two points (Start, End). `PointFeature` draws a small cross or circle marker.

### `TextFeature.cs`

For parcel labels, dimension annotations. Has `Position`, `Text`, `FontSize`, `FontName`. Uses `ctx.GetFont(name, size)` — same pooling pattern. Important: font rendering at high zoom needs `g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit`.

---

## 7. Phase 3 — RenderContext and PenCache

### `PenCache.cs`

A simple dictionary that pools `Pen` objects by `(Color, float width)` key.

```
Key: (Color borderColor, float lineWidth)
Value: Pen (created once, reused every frame)
```

**Lifecycle:** Created once when `FrameRenderer` is constructed. Disposed when `FrameRenderer` is disposed. The cache lives as long as the canvas lives.

**What you learn:** This eliminates the ~300,000 kernel calls per second from the old code. GDI+ `Pen` creation calls into the OS. Caching reduces this to near zero after the first render frame.

### `BrushCache.cs`

Same pattern. Key is `(Color, int transparency)`. Returns a `SolidBrush`.

### `RenderContext.cs`

The object passed into every `Draw()` call. It is a data carrier — not a manager, not a service.

```csharp
public sealed class RenderContext
{
    // Caches (owned by FrameRenderer, passed by reference)
    private readonly PenCache _penCache;
    private readonly BrushCache _brushCache;

    // Current frame state
    public double ZoomScale { get; }

    // Zoom-adaptive line width — shapes use this, not a hardcoded constant
    public float AdaptiveLineWidth =>
        ZoomScale > 5000 ? 1.5f :
        ZoomScale > 500  ? 1.0f :
        ZoomScale > 50   ? 0.5f : 0.25f;

    // GDI+ resource accessors
    public Pen GetPen(Color color, float width) => _penCache.Get(color, width);
    public SolidBrush GetBrush(Color color, int transparency = 255) => _brushCache.Get(color, transparency);
}
```

**Why `AdaptiveLineWidth` lives in `RenderContext` and not in the shape:** Because it depends on `ZoomScale`, which is a rendering concern, not a shape concern. The shape describes what it is. The context describes how it should look at the current zoom level. Separation of concerns.

**What you learn:** Every shape gets the same `RenderContext` per frame. The zoom-adaptive width is computed once per frame, not once per shape vertex.

---

## 8. Phase 4 — Viewport Engine

### `MapCanvasEngine.cs`

Clean rewrite of `DrawingEngine`. Same math, zero dead code.

**Fields (only these, nothing else):**

```csharp
private double _zoomScale = 1.0;
private PointD _viewOffset;   // world coordinate of screen point (0,0)
private Size _canvasSize;
```

**Public API:**

```csharp
// Coordinate transforms
PointD WorldToScreen(PointD world)
PointD ScreenToWorld(PointD screen)
PointD ScreenToWorld(Point screen)     // convenience overload for MouseEventArgs

// Zoom
void ZoomAtScreenPoint(Point screenPt, double factor)
double ZoomScale { get; }

// Pan
void PanBy(double worldDeltaX, double worldDeltaY)

// Viewport
RectangleD GetViewBounds()
void Resize(Size newSize)

// Zoom bounds
const double MIN_ZOOM = 0.0001;
const double MAX_ZOOM = 50000.0;
const double ZOOM_STEP = 1.4;

// Fit all features
void FitBounds(RectangleD worldBounds, double paddingPercent = 0.1)
```

**What you learn about `FitBounds`:** This is the "zoom to extents" operation used when a project loads. You calculate the bounding box of all loaded features, then set zoom and offset so the entire extent fills the canvas with 10% padding. AutoCAD calls this "ZOOM EXTENTS." It is critical for usability.

**The Y-axis flip — understand it clearly:**

```csharp
public PointD WorldToScreen(PointD world)
{
    double relX = world.X - _viewOffset.X;
    double relY = world.Y - _viewOffset.Y;
    double screenX = relX * _zoomScale;
    // Y is flipped: world Y increases up, screen Y increases down
    double screenY = _canvasSize.Height - (relY * _zoomScale);
    return new PointD(screenX, screenY);
}
```

This is the same formula as the old code. It is correct. Do not change it.

---

## 9. Phase 5 — Spatial Index

### `FeatureSpatialIndex.cs`

Complete rewrite of `RTreeSpatialIndex`. The fix for the O(n²) bug is the only meaningful change.

**The bug in the old code (confirmed from source):**

```csharp
// OLD — BROKEN
public void Insert(IShape shape)
{
    _allShapes.Add(shape);
    Rebuild();  // Full tree rebuild on EVERY single insert
}
```

For 5,000 shapes imported from DXF: 5,000 × 5,000 = 25 million operations. Takes minutes.

**The fix — dirty flag pattern:**

```csharp
// NEW — CORRECT
private bool _dirty = true;

public void Insert(IVectorShape shape)
{
    _allShapes.Add(shape);
    _dirty = true;  // Just mark dirty. Do NOT rebuild yet.
}

public List<IVectorShape> Query(RectangleD area)
{
    if (_dirty)
    {
        Rebuild();
        _dirty = false;
    }
    // ... query logic
}
```

**What you learn:** NTS `STRtree` is a bulk-loaded, static structure. Once built, you cannot insert into it — you can only query it. The old code worked around this limitation by rebuilding after every insert. The correct approach is to accumulate inserts (just update `_allShapes`), mark dirty, and let the first query after a batch of inserts trigger exactly one rebuild.

**Layer-aware query — this is new:**

```csharp
public List<IVectorShape> Query(RectangleD viewport, IEnumerable<string> visibleLayerNames)
{
    if (_dirty) { Rebuild(); _dirty = false; }

    var layerSet = new HashSet<string>(visibleLayerNames);
    var env = ToEnvelope(viewport);
    var candidates = _index.Query(env);

    var result = new List<IVectorShape>();
    foreach (var shape in candidates)
    {
        if (!layerSet.Contains(shape.LayerName)) continue;
        if (!shape.IsVisible) continue;
        if (shape.GetBoundingBox().IntersectsWith(viewport))
            result.Add(shape);
    }
    return result;
}
```

**What you learn:** By accepting `visibleLayerNames`, the spatial index query itself filters out hidden layers. The renderer never receives shapes from invisible layers — it never has to check. This is the correct place for this filter.

---

## 10. Phase 6 — Feature Store

### `FeatureStore.cs`

Replaces `ShapeManager`. The key difference is that `FeatureStore` is **layer-aware** from the start.

**Responsibilities:**

- Hold all in-memory `IVectorShape` objects
- Delegate spatial queries to `FeatureSpatialIndex`
- Support add/remove/bulk operations
- Expose `RebuildSpatialIndex()` for explicit bulk-load finalization

**API:**

```csharp
public class FeatureStore
{
    // Add
    void Add(IVectorShape shape)
    void AddRange(IEnumerable<IVectorShape> shapes)    // marks dirty once at end
    void BulkLoad(IEnumerable<IVectorShape> shapes)    // pre-allocates, single rebuild

    // Remove
    bool Remove(IVectorShape shape)
    void RemoveRange(IEnumerable<IVectorShape> shapes)
    void Clear()

    // Query
    List<IVectorShape> QueryViewport(RectangleD viewport, IEnumerable<string> visibleLayerNames)
    List<IVectorShape> QueryAtPoint(PointD worldPt, double tolerance)
    IReadOnlyList<IVectorShape> GetAll()
    int Count { get; }

    // Index management
    void RebuildSpatialIndex()

    // Events
    event EventHandler<FeatureStoreChangedEventArgs> Changed
}
```

**Why `BulkLoad` is separate from `AddRange`:** `BulkLoad` is called on project open when you load thousands of features from the database. It pre-allocates list capacity (`_shapes.Capacity = count`) before adding, and calls `RebuildSpatialIndex()` exactly once at the end. `AddRange` is for smaller batches during editing. Both result in a single index rebuild — the difference is the pre-allocation.

---

## 11. Phase 7 — Runtime Layer Manager

### Three Classes to Build

These three classes together replace what `frmLayerManager` tried to do but couldn't connect to the renderer.

---

### `RuntimeLayer.cs`

An in-memory representation of one layer, built from a `CanvasLayer` entity. This is NOT an entity — it is a runtime object.

```csharp
public sealed class RuntimeLayer
{
    public int Id { get; }
    public string Name { get; set; }
    public string LayerType { get; set; }
    public int DisplayOrder { get; set; }

    // Visibility / editing state
    public bool IsVisible { get; set; }
    public bool IsLocked { get; set; }
    public bool IsSelectable { get; set; }
    public bool IsPrintable { get; set; }

    // Default style (used when CanvasObject has no override)
    public Color DefaultBorderColor { get; set; }
    public Color DefaultFillColor { get; set; }
    public float DefaultLineWeight { get; set; }
    public string DefaultLineStyle { get; set; }
    public int DefaultFillTransparency { get; set; }

    // Label settings
    public bool ShowLabels { get; set; }
    public string LabelFontName { get; set; }
    public float LabelFontSize { get; set; }
    public Color LabelColor { get; set; }

    // Factory
    public static RuntimeLayer FromEntity(CanvasLayer entity) { ... }
}
```

**What you learn:** `RuntimeLayer` is a pure in-memory object. It can be mutated without touching EF Core tracking. When the user changes visibility in the Layer Manager panel, you update `RuntimeLayer.IsVisible` immediately (instant visual response), and persist the change to the database asynchronously in the background.

---

### `LayerStyleResolver.cs`

This is the most important class to understand conceptually.

**The problem it solves:** A `CanvasObject` (a parcel's geometry record) can have `BorderColorOverride`, `FillColorOverride`, and `FillTransparencyOverride` fields. When these are null, use the layer's default style. When they are set, use the override.

```csharp
public static class LayerStyleResolver
{
    public static ResolvedStyle Resolve(RuntimeLayer layer, IVectorShape shape)
    {
        return new ResolvedStyle
        {
            BorderColor = shape.BorderColorOverride ?? layer.DefaultBorderColor,
            FillColor   = shape.FillColorOverride   ?? layer.DefaultFillColor,
            LineWeight  = shape.LineWeightOverride   ?? layer.DefaultLineWeight,
            Transparency = shape.FillTransparencyOverride ?? layer.DefaultFillTransparency,
            ShowLabel   = layer.ShowLabels,
            FontName    = layer.LabelFontName,
            FontSize    = layer.LabelFontSize,
            LabelColor  = layer.LabelColor
        };
    }
}
```

**What you learn:** This is exactly how AutoCAD's layer/object style system works. Layer provides defaults. Object can override any individual property. The resolver merges them. Shapes themselves store neither color nor line weight directly — they reference a layer and optionally carry overrides. This means you can change an entire layer's color in one operation and all 10,000 parcels update instantly.

---

### `RuntimeLayerManager.cs`

Owns the in-memory list of `RuntimeLayer` objects and exposes the event that wires everything together.

```csharp
public sealed class RuntimeLayerManager
{
    private List<RuntimeLayer> _layers = new();

    // The critical event — renderer subscribes to this
    public event EventHandler LayersChanged;

    // Load from database on project open
    public async Task LoadAsync(ICanvasLayerRepository repo, CancellationToken ct)
    {
        var entities = await repo.GetAllOrderedAsync(ct);
        _layers = entities.Select(RuntimeLayer.FromEntity).ToList();
        LayersChanged?.Invoke(this, EventArgs.Empty);
    }

    // Called by frmLayerManager on OK
    public async Task ApplyChangesAsync(
        IEnumerable<RuntimeLayer> updatedLayers,
        ICanvasLayerRepository repo,
        CancellationToken ct)
    {
        // 1. Update in-memory state
        _layers = updatedLayers.OrderBy(l => l.DisplayOrder).ToList();
        // 2. Persist to DB
        await repo.SaveLayerOrderAndStyleAsync(_layers, ct);
        // 3. Fire event — renderer will invalidate cache
        LayersChanged?.Invoke(this, EventArgs.Empty);
    }

    // Called by toolbar toggle (immediate)
    public void SetLayerVisible(string layerName, bool visible)
    {
        var layer = _layers.FirstOrDefault(l => l.Name == layerName);
        if (layer == null) return;
        layer.IsVisible = visible;
        LayersChanged?.Invoke(this, EventArgs.Empty);
    }

    // Used by renderer and spatial index
    public IReadOnlyList<RuntimeLayer> GetVisibleLayers()
        => _layers.Where(l => l.IsVisible).ToList().AsReadOnly();

    public IReadOnlyList<string> GetVisibleLayerNames()
        => _layers.Where(l => l.IsVisible).Select(l => l.Name).ToList().AsReadOnly();

    public RuntimeLayer? GetLayer(string name)
        => _layers.FirstOrDefault(l => l.Name == name);
}
```

**What you learn — the event is the bridge:** When `LayersChanged` fires, `FrameRenderer` subscribed to it calls `InvalidateCache()`. The next `panelCanvas.Invalidate()` triggers a full re-render that respects the new layer visibility. This is how the Layer Manager "connects" to the renderer — through an event, not through direct coupling. Neither class needs a reference to the other.

---

## 12. Phase 8 — Deferred Renderer

### `FrameRenderer.cs`

Replaces `OptimizedDeferredRenderer`. Same strategy, all issues fixed.

### Three-Tier Strategy (Same as Old, Still Correct)

| Shape Count | Strategy | GDI+ Mode | Cache? |
|---|---|---|---|
| < 1,000 | Full quality | `AntiAlias` + `HighQuality` | Yes — bitmap cache |
| 1,000–5,000 | Balanced | `HighSpeed` | Yes — bitmap cache |
| > 5,000 | LOD | Skip shapes < 1 screen pixel | No cache — direct draw |

### The Padding Fix (Critical — Not in Old Code)

```csharp
// OLD — no padding, tears at edges during pan
_shapesCache = new Bitmap(_canvasSize.Width, _canvasSize.Height);
_panBuffer   = new Bitmap(_canvasSize.Width, _canvasSize.Height);

// NEW — 100px padding on all 4 sides
private const int CACHE_PADDING = 100;

private void AllocateBitmaps()
{
    int w = _canvasSize.Width  + CACHE_PADDING * 2;
    int h = _canvasSize.Height + CACHE_PADDING * 2;
    _shapesCache?.Dispose();
    _panBuffer?.Dispose();
    _shapesCache = new Bitmap(Math.Max(1, w), Math.Max(1, h));
    _panBuffer   = new Bitmap(Math.Max(1, w), Math.Max(1, h));
}
```

When rendering into the cache, offset all drawing by `(CACHE_PADDING, CACHE_PADDING)`. When blitting the cache to the screen, offset by `(-CACHE_PADDING, -CACHE_PADDING)` plus the pan delta. During pan, the 100px buffer on each side means the background only shows through after more than 100px of cursor movement — enough for all normal pan gestures.

**What you learn:** The old code was already correct conceptually (blit-during-pan). The only missing piece was this padding. A Bitmap with 100px extra costs 100 × canvasHeight × 4 bytes per side ≈ 400KB extra for a 1080p canvas — completely negligible.

### How `RenderNow()` Works

```
1. Get visible layer names from RuntimeLayerManager
2. Query FeatureStore.QueryViewport(viewBounds, visibleLayerNames)
3. Choose rendering tier based on shape count
4. Clear cache bitmap
5. For each shape (in layer DisplayOrder):
     a. Resolve style via LayerStyleResolver
     b. Call shape.Draw(g, engine.WorldToScreen, renderContext)
6. Set _cacheValid = true
```

**Why layer order inside `RenderNow`?** The shapes come back from the spatial index without a guaranteed order. Before drawing, sort them by their layer's `DisplayOrder`. This ensures roads always render above parcels, and overlays render above roads — regardless of which shape was added first.

### `BeginPan()` / `DrawDuringPan()` / `EndPan()`

Same logic as old code, with padding adjustment:

```csharp
public void BeginPan()
{
    // Snapshot current cache into pan buffer
    using var g = Graphics.FromImage(_panBuffer);
    g.Clear(Color.Transparent);
    if (_cacheValid)
        g.DrawImage(_shapesCache, 0, 0);
    else
    {
        // Slow path: render directly (happens when > 5000 shapes, cache was skipped)
        var shapes = _featureStore.QueryViewport(...);
        // ... direct draw
    }
}

public void DrawDuringPan(Graphics g, double totalDeltaX, double totalDeltaY)
{
    g.InterpolationMode = InterpolationMode.NearestNeighbor;
    // Offset accounts for padding
    float x = (float)totalDeltaX - CACHE_PADDING;
    float y = (float)totalDeltaY - CACHE_PADDING;
    g.DrawImage(_panBuffer, x, y);
}

public void EndPan()
{
    RenderNow();  // Full re-render at new viewport position
}
```

### High-Zoom GDI+ Mode

```csharp
private void SetGraphicsQuality(Graphics g, double zoomScale, int shapeCount)
{
    if (shapeCount > 5000 || zoomScale < 1.0)
    {
        g.SmoothingMode = SmoothingMode.HighSpeed;
        g.PixelOffsetMode = PixelOffsetMode.None;
    }
    else if (zoomScale > 500)
    {
        // High zoom: half-pixel offset prevents thin line disappearance
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;
    }
    else
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    }
}
```

**What you learn about `PixelOffsetMode.Half`:** At high zoom, GDI+ `AntiAlias` mode with `HighQuality` pixel offset can make a 1-pixel line appear as 0.5px — visually faint. `PixelOffsetMode.Half` shifts the rendering grid by half a pixel, which keeps thin lines crisp. This is the standard fix for hairline visibility at high zoom.

---

## 13. Phase 9 — Grid and Overlay Renderer

### `GridOverlayRenderer.cs`

Replaces the grid/overlay portions of `CanvasRenderer`. It has **exactly one job**: draw non-geometry visual elements.

**What it renders (in order):**

1. Background fill (solid color)
2. Grid lines (minor, then major)
3. X and Y axis markers
4. Snap glyph (when snapping is active)
5. HUD overlay (zoom level, world coordinates, shape count)
6. Snap-disabled warning (when too many shapes)

**What it does NOT render:** Shapes, previews, parcels, roads — nothing from `FeatureStore`.

### Adaptive Grid — Same Logic as Old, One Fix Added

The old grid algorithm (adaptive minor/major with hysteresis bands) is correct. Add one missing guard:

```csharp
// Add after calculating grid bounds, before drawing:
int estimatedLines = (int)((worldRight - worldLeft)  / minorWorldSize)
                   + (int)((worldTop   - worldBottom) / minorWorldSize);

if (estimatedLines > 2000)
    return; // Grid would be too dense — skip it entirely
```

**Why?** At very low zoom (zoomed out to see an entire district), the minor grid could require thousands of lines per frame. This cap prevents a frame rate stall. The user wouldn't see individual grid cells at that zoom anyway.

---

## 14. Phase 10 — MapCanvasControl

### `MapCanvasControl.cs`

A WinForms `UserControl`. This is the topmost orchestrator. Its only job is:

1. Handle user input (mouse, keyboard, scroll wheel)
2. Route events to the correct subsystem
3. Call `Invalidate()` when a repaint is needed
4. Coordinate the paint event (call renderers in correct order)

**Hard rule: zero math, zero GDI+ drawing in this class.** Everything delegates.

### Constructor — What It Creates and Why

```csharp
public MapCanvasControl()
{
    InitializeComponent();

    // Double buffering — critical to prevent flicker
    SetStyle(
        ControlStyles.AllPaintingInWmPaint |
        ControlStyles.UserPaint |
        ControlStyles.OptimizedDoubleBuffer,
        true);
    UpdateStyles();

    // Create subsystems
    _engine          = new MapCanvasEngine(this.Size);
    _featureStore    = new FeatureStore();
    _layerManager    = new RuntimeLayerManager();
    _frameRenderer   = new FrameRenderer(_engine, _featureStore, _layerManager);
    _gridRenderer    = new GridOverlayRenderer();
    _undoManager     = new UndoRedoManager();
    _snapManager     = new SnapManager();

    // Wire the layer-changed event
    _layerManager.LayersChanged += OnLayersChanged;
}
```

**Why `SetStyle` with those three flags:** `AllPaintingInWmPaint` prevents erasing before painting (no white flash). `UserPaint` means WinForms won't auto-paint the control. `OptimizedDoubleBuffer` enables OS-level back buffer. Together these eliminate all flicker. The old `DrawingCanvasControl` does this correctly — keep it.

### The Paint Event (Order Matters)

```csharp
private void OnPaint(object sender, PaintEventArgs e)
{
    var g = e.Graphics;

    // Layer 1: Background
    _gridRenderer.RenderBackground(g, ClientSize, _engine);

    // Layer 2: Grid
    _gridRenderer.RenderGrid(g, _engine);

    // Layer 3: Axis markers
    _gridRenderer.RenderAxisMarkers(g, _engine);

    // Layer 4: Shapes (from FrameRenderer cache or direct draw)
    if (_isPanning)
        _frameRenderer.DrawDuringPan(g, _totalPanDeltaX, _totalPanDeltaY);
    else
        _frameRenderer.DrawFromCache(g);

    // Layer 5: Preview shape (while drawing, before click)
    if (_previewShape != null)
        _previewShape.Draw(g, _engine.WorldToScreen, _renderContext);

    // Layer 6: Polyline in-progress preview
    DrawPolylinePreview(g);

    // Layer 7: Snap glyph
    if (_currentSnapPoint.HasValue)
        _gridRenderer.RenderSnapGlyph(g, _engine.WorldToScreen(_currentSnapPoint.Value.Position), _currentSnapPoint.Value.Type);

    // Layer 8: HUD overlay
    _gridRenderer.RenderHUD(g, _engine, _featureStore.Count, _currentMouseWorldPos);
}
```

**What you learn:** The order is not arbitrary. Background must be first. Shapes must be before preview (the preview appears on top). Snap glyph must be last so it's always visible. If you render in wrong order, shapes appear on top of UI elements or snap glyphs are invisible behind shapes.

### Mouse Events

```csharp
// MouseDown
private void OnMouseDown(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Middle || (_panToolActive && e.Button == MouseButtons.Left))
    {
        _isPanning = true;
        _panStartScreen = e.Location;
        _totalPanDeltaX = 0;
        _totalPanDeltaY = 0;
        _frameRenderer.BeginPan();
        Cursor = Cursors.SizeAll;
    }
    else if (e.Button == MouseButtons.Left && _currentTool != MapTool.Pan)
    {
        HandleDrawingMouseDown(e);
    }
}

// MouseMove
private void OnMouseMove(object sender, MouseEventArgs e)
{
    _currentMouseWorldPos = _engine.ScreenToWorld(e.Location);

    if (_isPanning)
    {
        _totalPanDeltaX = e.X - _panStartScreen.X;
        _totalPanDeltaY = e.Y - _panStartScreen.Y;
        Invalidate(); // Fast — just blits shifted pan buffer
        return;
    }

    UpdateSnap(e.Location);
    Invalidate();
}

// MouseUp
private void OnMouseUp(object sender, MouseEventArgs e)
{
    if (_isPanning && e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
    {
        // Convert total screen pan delta to world delta and apply to engine
        double worldDeltaX = -_totalPanDeltaX / _engine.ZoomScale;
        double worldDeltaY =  _totalPanDeltaY / _engine.ZoomScale;  // note: Y flipped
        _engine.PanBy(worldDeltaX, worldDeltaY);
        _frameRenderer.EndPan();  // triggers RenderNow() at new position
        _isPanning = false;
        Cursor = _panToolActive ? Cursors.SizeAll : Cursors.Cross;
        Invalidate();
    }
}

// MouseWheel
private void OnMouseWheel(object sender, MouseEventArgs e)
{
    double factor = e.Delta > 0 ? MapCanvasEngine.ZOOM_STEP : 1.0 / MapCanvasEngine.ZOOM_STEP;
    _engine.ZoomAtScreenPoint(e.Location, factor);
    _frameRenderer.RenderNow();  // must re-render after zoom
    Invalidate();
}
```

**What you learn about the pan calculation:** During pan, you only call `Invalidate()` — which just blits the shifted buffer. Fast. On `MouseUp`, you apply the accumulated delta to the engine as a world-space translation, then call `EndPan()` which triggers a full `RenderNow()`. This is why pan feels instant — you never re-render during drag, only after release.

---

## 15. Phase 11 — Wiring Layer Manager to Render Pipeline

### The Complete Connection

Here is exactly how `frmLayerManager` connects to the renderer — step by step:

```
1. User opens Layer Manager
   MapCanvasControl calls:
       var frm = new frmLayerManager(_layerManager.GetAllLayers());
       frm.ShowDialog();

2. User toggles layer visibility, changes colors, clicks OK
   frmLayerManager calls back:
       if (result == DialogResult.OK)
           await _layerManager.ApplyChangesAsync(frm.ResultLayers, _repo, ct);

3. RuntimeLayerManager.ApplyChangesAsync() runs:
       _layers = updatedLayers.OrderBy(l => l.DisplayOrder).ToList();
       await _repo.SaveLayerOrderAndStyleAsync(_layers, ct);
       LayersChanged?.Invoke(this, EventArgs.Empty);  // ← fires event

4. MapCanvasControl subscribed to LayersChanged:
       private void OnLayersChanged(object sender, EventArgs e)
       {
           _frameRenderer.InvalidateCache();  // marks cache as stale
           Invalidate();                      // triggers repaint
       }

5. Next Paint event runs:
       _frameRenderer.DrawFromCache(g)
       → cache is invalid → calls RenderNow()
       → RenderNow() queries FeatureStore with NEW visible layer names
       → shapes on hidden layers are not returned by FeatureSpatialIndex
       → those shapes are not drawn
       → canvas shows updated visibility
```

**What you learn:** This is the Observer pattern. `RuntimeLayerManager` is the subject. `MapCanvasControl` is the observer. Neither holds a direct reference to the other's internal state. You can swap implementations of either without touching the other. This is the correct architecture for this kind of cross-component communication.

---

## 16. Phase 12 — Persistence Bridge

### What Needs to Be Built

The `CanvasObject` entity in the database holds NTS geometry. The canvas renders `IVectorShape` objects. A bridge is needed to convert between them on project open and after edits.

### `GeometryShapeMapper.cs`

Converts `NTS Geometry ↔ IVectorShape`. Lives in `UI/MapCanvas2/Services/`.

**DB → Shape (loading):**

```csharp
public IVectorShape MapToShape(CanvasObject obj, RuntimeLayer layer)
{
    if (obj.Shape == null) return null;

    var shape = obj.Shape switch
    {
        Polygon polygon       => MapPolygon(polygon, obj),
        LineString lineString => MapLineString(lineString, obj),
        Point point           => MapPoint(point, obj),
        _ => null
    };

    if (shape == null) return null;

    shape.LayerName = layer.Name;
    shape.IsVisible = obj.IsVisible;
    shape.IsLocked  = obj.IsLocked;

    // Store the DB id on the shape so edits can be written back
    shape.CanvasObjectId = obj.Id;

    // Store style overrides if present
    shape.BorderColorOverride = ParseHex(obj.BorderColorOverride);
    shape.FillColorOverride   = ParseHex(obj.FillColorOverride);

    return shape;
}

private PolylineFeature MapPolygon(Polygon polygon, CanvasObject obj)
{
    var vertices = polygon.ExteriorRing.Coordinates
        .Select(c => new PointD(c.X, c.Y))
        .ToList();

    return new PolylineFeature(vertices, isClosed: true)
    {
        LabelText = obj.LabelText
    };
}
```

**Shape → DB (saving after edit):**

```csharp
public CanvasObject MapToEntity(IVectorShape shape, CanvasLayer layer)
{
    var geometry = shape.ToNtsGeometry();

    return new CanvasObject
    {
        Id            = shape.CanvasObjectId != Guid.Empty ? shape.CanvasObjectId : Guid.NewGuid(),
        CanvasLayerId = layer.Id,
        Shape         = geometry,
        IsVisible     = shape.IsVisible,
        IsLocked      = shape.IsLocked,
        LabelText     = (shape as PolylineFeature)?.LabelText,
        LastModifiedDate = DateTime.UtcNow
    };
}
```

### Loading on Project Open

```csharp
// In MapCanvasControl.LoadProjectAsync():
var layers  = await _layerManager.LoadAsync(_layerRepo, ct);
var objects = await _canvasObjectRepo.GetAllAsync(ct);

var shapes = objects
    .Select(obj => {
        var layer = _layerManager.GetLayer(obj.CanvasLayer.Name);
        return layer != null ? _mapper.MapToShape(obj, layer) : null;
    })
    .Where(s => s != null)
    .ToList();

_featureStore.BulkLoad(shapes);
_engine.FitBounds(_featureStore.GetBounds());
_frameRenderer.RenderNow();
Invalidate();
```

---

## 17. Complete File Map

```
UI/MapCanvas2/
├── Core/
│   ├── MapCanvasEngine.cs           NEW  (replaces DrawingEngine — same math, no dead code)
│   └── FeatureStore.cs              NEW  (replaces ShapeManager — layer-aware)
├── SpatialIndex/
│   └── FeatureSpatialIndex.cs       NEW  (replaces RTreeSpatialIndex — dirty flag fixed)
├── Models/
│   └── Shapes/
│       ├── IVectorShape.cs          NEW  (replaces IShape — adds RenderContext param, NTS method)
│       ├── VectorShape.cs           NEW  (replaces Shape base — adds ClampToGdi, ToScreenPointF)
│       ├── PolylineFeature.cs       NEW  (replaces PolylineShape — IsClosed for parcels)
│       ├── LineFeature.cs           NEW  (replaces LineShape)
│       ├── PointFeature.cs          NEW  (replaces CircleShape used as marker)
│       └── TextFeature.cs           NEW  (replaces TextShape)
├── Rendering/
│   ├── RenderContext.cs             NEW  (carries PenCache, BrushCache, zoom-adaptive width)
│   ├── PenCache.cs                  NEW  (Dictionary<(Color,float), Pen> pool)
│   ├── BrushCache.cs                NEW  (Dictionary<(Color,int), SolidBrush> pool)
│   ├── FrameRenderer.cs             NEW  (replaces OptimizedDeferredRenderer — padding fixed)
│   └── GridOverlayRenderer.cs       NEW  (replaces CanvasRenderer grid/overlay portions)
├── Layers/
│   ├── RuntimeLayer.cs              NEW  (in-memory layer from CanvasLayer entity)
│   ├── RuntimeLayerManager.cs       NEW  (ordered layers, LayersChanged event)
│   └── LayerStyleResolver.cs        NEW  (merges layer defaults + object overrides)
├── Services/
│   └── GeometryShapeMapper.cs       NEW  (NTS Geometry ↔ IVectorShape bridge)
└── Controls/
    └── MapCanvasControl.cs          NEW  (replaces DrawingCanvasControl — events only)
```

**Old files — do not delete, do not edit:**

```
UI/MapCanvas/               ← Freeze. Zero new features here.
UI/CustomControls/DrawingCanvasControl.cs  ← Keep working as-is until new control is stable
```

---

## 18. What NOT to Do

These are specific mistakes to avoid. Each one caused a real problem in the old code.

**Do not call `Rebuild()` inside `Insert()`.**  
This is the O(n²) bug. Set `_dirty = true` and let the next `Query()` rebuild.

**Do not create `new Pen()` or `new SolidBrush()` inside a `Draw()` method.**  
Use `ctx.GetPen()` and `ctx.GetBrush()`. One allocation per unique style, reused every frame.

**Do not pass screen coordinates to NTS operations.**  
NTS works in world space (meters). Screen coordinates (pixels) fed into NTS `Intersects()`, `Union()`, `Buffer()` will produce meaningless results. Convert world→screen only at the final `PointF` cast inside `Draw()`.

**Do not cast world coordinates directly to `float` without clamping first.**  
Nepal TM coordinates at high zoom overflow GDI+ float range. Always `ClampToGdi()` before `(float)`.

**Do not let `CanvasRenderer` / `GridOverlayRenderer` iterate shapes.**  
Grid renderer handles grid and overlays only. Shapes always go through `FrameRenderer`. Two separate loops means two different rendering paths that can diverge.

**Do not call `Invalidate()` from inside a `Draw()` call or a renderer.**  
`Invalidate()` is a UI concern. Only `MapCanvasControl` calls it, in response to state changes.

**Do not persist `CanvasObject` changes directly from shape event handlers.**  
All persistence goes through the `CanvasObjectRepository` via an explicit save command. Direct DB writes from UI events bypass undo/redo and transaction handling.

**Do not add new features to `DrawingCanvasControl` / `DrawingEngine`.**  
The old path is frozen. Every new feature goes into the new `MapCanvasControl` path.

---

## 19. Testing Checklist

Before calling any phase complete, verify these specific behaviors:

### Phase 5 (Spatial Index)
- [ ] Import 5,000 shapes from DXF. Time it. Should complete in < 2 seconds (dirty flag fix).
- [ ] Query returns shapes that intersect the viewport, not shapes outside it.
- [ ] After removing a shape, it does not appear in subsequent queries.

### Phase 7 (Layer Manager)
- [ ] Toggle a layer invisible. Shapes on that layer disappear from canvas immediately.
- [ ] Toggle it back visible. Shapes reappear.
- [ ] Change a layer's border color. All shapes on that layer render in the new color.
- [ ] Lock a layer. Shapes on that layer cannot be selected (hit test returns nothing for locked layers).

### Phase 8 (Frame Renderer)
- [ ] Pan fast across the canvas. No visible tears at canvas edges (padding fix).
- [ ] Zoom to level 50,000 on a Nepal TM coordinate area. Lines are still visible and not corrupted.
- [ ] Zoom to level 0.001 (far out). Grid disappears when too dense (grid cap).
- [ ] Add 6,000 shapes. Verify LOD path is taken (shapes < 1 screen pixel are skipped).

### Phase 10 (Canvas Control)
- [ ] Pan: drag with middle mouse. Shape bitmap shifts instantly. On release, full re-render at new position.
- [ ] Zoom: scroll wheel. Canvas zooms toward cursor, not toward center.
- [ ] At all zoom levels, the world coordinate in the HUD matches the expected Nepal TM coordinate.

### Phase 11 (Layer Wiring)
- [ ] Open Layer Manager, hide "Existing Cadastre" layer, click OK. Parcels disappear.
- [ ] Reopen Layer Manager, show it again. Parcels reappear.
- [ ] Database `tblCanvasLayers.IsVisible` column updated after closing Layer Manager.

### Phase 12 (Persistence Bridge)
- [ ] Save project, close, reopen. All drawn shapes appear in the same positions.
- [ ] Shape on a named layer survives save/load and appears on the correct layer.

---

## Summary — Build Order

```
Phase  1 → MapCanvasEngine.cs              (math only, 1 hour)
Phase  2 → IVectorShape, VectorShape,
            PolylineFeature, LineFeature   (shapes, 2 hours)
Phase  3 → RenderContext, PenCache,
            BrushCache                     (resource pooling, 1 hour)
Phase  4 → FeatureSpatialIndex             (dirty flag fix, 30 min)
Phase  5 → FeatureStore                    (layer-aware storage, 1 hour)
Phase  6 → RuntimeLayer,
            RuntimeLayerManager,
            LayerStyleResolver             (layer runtime, 2 hours)
Phase  7 → FrameRenderer                   (deferred render, 3 hours)
Phase  8 → GridOverlayRenderer             (grid + HUD, 1 hour)
Phase  9 → MapCanvasControl                (wiring, 2 hours)
Phase 10 → GeometryShapeMapper             (persistence bridge, 2 hours)
```

**Estimated total: ~16 hours of focused implementation.**

Each phase compiles and can be tested independently. After Phase 9, you have a working canvas that renders shapes, layers, snapping, and zoom/pan correctly — even before Phase 10 connects it to the database.

---

*Document prepared for: RePlot Land Readjustment Tool — C# .NET 10 WinForms*  
*Based on analysis of: `UI/MapCanvas/` existing implementation + architecture documentation*
