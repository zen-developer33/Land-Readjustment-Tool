# GIS Map Canvas — Complete Implementation Guide
### Rendering Architecture for Windows Forms / C# .NET

> **Scope:** This document covers every layer of a production-grade GIS map canvas: WinForms control setup, viewport mathematics, spatial tile queries, raster sources (MBTiles, GeoTIFF, WMS, HTTP), tile caching (L1/L2), zoom-timer debounce, pan bitmap caching, deferred composite rendering, dynamic image quality, vector spatial culling, and threading architecture. Each section includes complete, ready-to-use C# code.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Project Setup & Dependencies](#2-project-setup--dependencies)
3. [Core Data Types](#3-core-data-types)
4. [WinForms Canvas Control](#4-winforms-canvas-control)
5. [Viewport Transform](#5-viewport-transform)
6. [Tile XYZ Spatial Query](#6-tile-xyz-spatial-query)
7. [Tile Cache — L1 Memory (LRU)](#7-tile-cache--l1-memory-lru)
8. [Tile Cache — L2 Disk](#8-tile-cache--l2-disk)
9. [Zoom Timer & Debounce](#9-zoom-timer--debounce)
10. [Pan Bitmap Cache & Dirty Regions](#10-pan-bitmap-cache--dirty-regions)
11. [Dynamic Image Quality by Zoom Level](#11-dynamic-image-quality-by-zoom-level)
12. [Deferred Composite Off-Screen Rendering](#12-deferred-composite-off-screen-rendering)
13. [Raster Layer — Tile Stitching & Placeholders](#13-raster-layer--tile-stitching--placeholders)
14. [MBTiles Raster Source (SQLite)](#14-mbtiles-raster-source-sqlite)
15. [GeoTIFF Raster Source & Overview Pyramids](#15-geotiff-raster-source--overview-pyramids)
16. [WMS / WMTS Raster Source](#16-wms--wmts-raster-source)
17. [HTTP Tile Server Source](#17-http-tile-server-source)
18. [Async Tile Loading Pipeline](#18-async-tile-loading-pipeline)
19. [Vector Layer & Spatial Culling](#19-vector-layer--spatial-culling)
20. [R-Tree Spatial Index](#20-r-tree-spatial-index)
21. [Label & Annotation Rendering](#21-label--annotation-rendering)
22. [Overlay Objects (Markers, Shapes, Rulers)](#22-overlay-objects-markers-shapes-rulers)
23. [Threading Model & Safety](#23-threading-model--safety)
24. [Memory Management & Bitmap Disposal](#24-memory-management--bitmap-disposal)
25. [Layer Manager & Draw Order](#25-layer-manager--draw-order)
26. [Scale Bar & Map Decorations](#26-scale-bar--map-decorations)
27. [Hit Testing & Mouse Interaction](#27-hit-testing--mouse-interaction)
28. [Coordinate Systems & Projections](#28-coordinate-systems--projections)
29. [Performance Benchmarks & Tuning](#29-performance-benchmarks--tuning)
30. [Complete Wiring — MapCanvas Integration](#30-complete-wiring--mapcanvas-integration)

---

## 1. Architecture Overview

The rendering pipeline flows in a strict order. Every stage feeds the next and can be cancelled independently when the viewport changes.

```
User Input (mouse/keyboard)
        │
        ▼
Viewport Transform          ← world ↔ screen coordinate math
        │
   ┌────┴────┐
   ▼         ▼
Zoom Timer  Spatial Query   ← debounce zoom; compute visible tile IDs
   └────┬────┘
        ▼
Tile Request Queue          ← prioritised by distance to screen centre
        │
   ┌────┴──────────┐
   ▼               ▼
L1 Memory Cache  L2 Disk Cache  ← LRU bitmap dict; keyed PNG files
        │               │
        └───────┬────────┘
                ▼
         Raster Source          ← MBTiles / GeoTIFF / WMS / HTTP
                │
                ▼
   Deferred Compositor          ← off-screen Bitmap, PArgb, async Task
        │
   ┌────┴──────────────┐
   ▼                   ▼
Pan Bitmap Cache    Vector Layer   ← translate existing buffer; spatial cull
        │                   │
        └────────┬────────── ┘
                 ▼
     WinForms Panel OnPaint        ← DrawImage from back buffer, DoubleBuffer
```

**Key design principles:**

- The `OnPaint` handler does **one thing only**: `DrawImage` from the pre-composed back buffer. It never fetches data or does layout work.
- Every long-running operation (tile decode, file I/O, HTTP) runs on `Task.Run`. Results post back to the UI via `BeginInvoke(() => Invalidate())`.
- Cancellation tokens flow through every async path so stale renders are aborted immediately when the viewport changes.
- `PixelFormat.Format32bppPArgb` is used on every `Bitmap` — it is the single biggest free performance gain in GDI+.

---

## 2. Project Setup & Dependencies

### NuGet Packages

```xml
<!-- .csproj -->
<PackageReference Include="System.Data.SQLite" Version="1.0.118" />
<PackageReference Include="BitMiracle.LibTiff.NET" Version="2.4.649" />
<PackageReference Include="NetTopologySuite" Version="2.5.0" />
<!-- Optional: faster HTTP tile loading -->
<PackageReference Include="System.Net.Http" Version="4.3.4" />
```

### Project Properties

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>   <!-- needed for LockBits fast pixel ops -->
    <Optimize>true</Optimize>
  </PropertyGroup>
</Project>
```

### Namespace Layout

```
MapCanvas/
├── Core/
│   ├── TileId.cs
│   ├── Viewport.cs
│   ├── RectangleD.cs
│   └── PointD.cs
├── Cache/
│   ├── TileCache.cs          ← L1 + L2 combined
│   └── PanCache.cs
├── Sources/
│   ├── IRasterTileSource.cs
│   ├── MbTilesSource.cs
│   ├── GeoTiffSource.cs
│   ├── WmsSource.cs
│   └── HttpTileSource.cs
├── Layers/
│   ├── IMapLayer.cs
│   ├── RasterLayer.cs
│   ├── VectorLayer.cs
│   └── OverlayLayer.cs
├── Rendering/
│   ├── LayerCompositor.cs
│   ├── RenderQuality.cs
│   └── ScaleBar.cs
├── Interaction/
│   ├── TileLoader.cs
│   └── HitTester.cs
└── MapCanvas.cs              ← WinForms Panel subclass
```

---

## 3. Core Data Types

These structs are used everywhere and should be defined first.

```csharp
// Core/PointD.cs
public readonly struct PointD
{
    public double X { get; }
    public double Y { get; }
    public PointD(double x, double y) { X = x; Y = y; }
    public static PointD operator +(PointD a, PointD b) => new(a.X + b.X, a.Y + b.Y);
    public static PointD operator -(PointD a, PointD b) => new(a.X - b.X, a.Y - b.Y);
    public override string ToString() => $"({X:F6}, {Y:F6})";
}

// Core/RectangleD.cs
public readonly struct RectangleD
{
    public double X      { get; }
    public double Y      { get; }
    public double Width  { get; }
    public double Height { get; }

    public double Left   => X;
    public double Right  => X + Width;
    public double Top    => Y + Height;   // geographic: Y increases northward
    public double Bottom => Y;

    public RectangleD(double x, double y, double width, double height)
    { X = x; Y = y; Width = width; Height = height; }

    public bool IntersectsWith(RectangleD other) =>
        Left < other.Right && Right > other.Left &&
        Bottom < other.Top && Top > other.Bottom;

    public bool Contains(PointD p) =>
        p.X >= Left && p.X <= Right && p.Y >= Bottom && p.Y <= Top;
}

// Core/TileId.cs
public readonly struct TileId : IEquatable<TileId>
{
    public int Z { get; }
    public int X { get; }
    public int Y { get; }

    public TileId(int z, int x, int y) { Z = z; X = x; Y = y; }

    public bool Equals(TileId other) => Z == other.Z && X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is TileId t && Equals(t);
    public override int GetHashCode() => HashCode.Combine(Z, X, Y);
    public override string ToString() => $"{Z}/{X}/{Y}";

    public TileId Parent   => new(Z - 1, X / 2, Y / 2);
    public TileId[] Children => new[]
    {
        new TileId(Z + 1, X * 2,     Y * 2),
        new TileId(Z + 1, X * 2 + 1, Y * 2),
        new TileId(Z + 1, X * 2,     Y * 2 + 1),
        new TileId(Z + 1, X * 2 + 1, Y * 2 + 1)
    };
}
```

---

## 4. WinForms Canvas Control

The canvas is a `Panel` subclass. The critical settings are the `ControlStyles` flags — without `AllPaintingInWmPaint` and `OptimizedDoubleBuffer` the control will flicker on every repaint.

```csharp
// MapCanvas.cs  (partial — input handlers in Section 27)
public sealed partial class MapCanvas : Panel
{
    // ── Public surface ───────────────────────────────────────────────────
    public Viewport          Viewport    { get; private set; }
    public LayerManager      Layers      { get; }          = new();
    public TileCache         Cache       { get; }
    public event EventHandler? ViewportChanged;

    // ── Private state ────────────────────────────────────────────────────
    private readonly LayerCompositor _compositor;
    private readonly TileLoader      _loader;
    private readonly PanCache        _panCache    = new();

    private bool   _isPanning;
    private bool   _isZooming;
    private PointF _panStart;
    private PointF _panLastScreen;

    public MapCanvas(string diskCachePath = "tile_cache")
    {
        // ── CRITICAL: these three flags eliminate all flicker ────────────
        SetStyle(
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint  |
            ControlStyles.UserPaint             |
            ControlStyles.ResizeRedraw, true);
        UpdateStyles();

        DoubleBuffered = true;    // belt-and-suspenders
        BackColor      = Color.FromArgb(24, 24, 28);
        Cursor         = Cursors.Cross;

        Cache        = new TileCache(diskCachePath);
        Viewport     = new Viewport { ScreenSize = ClientSize };
        _compositor  = new LayerCompositor(this, Layers, Cache);
        _loader      = new TileLoader(Cache);
        _loader.TileReady += () => BeginInvoke(Invalidate);
    }

    // ── Paint ────────────────────────────────────────────────────────────
    protected override void OnPaint(PaintEventArgs e)
    {
        // OnPaint does exactly ONE thing: draw the pre-composed back buffer.
        // All render work happens on background threads.
        e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
        _compositor.DrawTo(e.Graphics);
        DrawDecorations(e.Graphics);   // scale bar, attribution — always synchronous
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width < 1 || Height < 1) return;
        Viewport = Viewport.WithScreenSize(ClientSize);
        RequestFullRender();
    }

    // ── Trigger a fresh composite render ─────────────────────────────────
    public void RequestFullRender()
    {
        _loader.LoadTilesForViewport(Viewport);
        _compositor.RequestRender(Viewport);
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    // ── Navigate ─────────────────────────────────────────────────────────
    public void ZoomTo(double zoom, PointD? centre = null)
    {
        Viewport = Viewport
            .WithZoom(Math.Clamp(zoom, 0, 22))
            .WithCenter(centre ?? Viewport.Center);
        RequestFullRender();
    }

    public void PanTo(PointD worldCenter)
    {
        Viewport = Viewport.WithCenter(worldCenter);
        RequestFullRender();
    }

    public void ZoomToExtent(RectangleD worldExtent)
    {
        double zoomX = Math.Log2(ClientSize.Width  / (worldExtent.Width  / 156543.034));
        double zoomY = Math.Log2(ClientSize.Height / (worldExtent.Height / 156543.034));
        double zoom  = Math.Min(zoomX, zoomY) - 0.5;
        PanTo(new PointD(worldExtent.X + worldExtent.Width  / 2,
                         worldExtent.Y + worldExtent.Height / 2));
        ZoomTo(zoom);
    }
}
```

---

## 5. Viewport Transform

The `Viewport` is an **immutable record** — changing zoom or centre produces a new instance. This makes it safe to pass to background render tasks without synchronisation, because the task holds a snapshot that cannot change under it.

```csharp
// Core/Viewport.cs
public sealed class Viewport
{
    // ── State ─────────────────────────────────────────────────────────────
    public PointD Center     { get; init; } = new(0, 0);
    public double ZoomLevel  { get; init; } = 2.0;
    public Size   ScreenSize { get; init; } = new(800, 600);

    // ── Derived ───────────────────────────────────────────────────────────
    /// <summary>Metres per pixel in Web Mercator at the current zoom.</summary>
    public double Resolution => 156543.03392 / Math.Pow(2, ZoomLevel);

    /// <summary>Integer zoom for tile selection.</summary>
    public int TileZoom => (int)Math.Round(ZoomLevel);

    /// <summary>Coarser tile zoom used during animated zoom for fewer requests.</summary>
    public int CoarseTileZoom => Math.Max(0, (int)Math.Floor(ZoomLevel) - 1);

    // ── Coordinate conversions ────────────────────────────────────────────
    public PointF WorldToScreen(PointD world)
    {
        double dx = (world.X - Center.X) / Resolution;
        double dy = (Center.Y - world.Y) / Resolution;
        return new PointF(
            (float)(ScreenSize.Width  / 2.0 + dx),
            (float)(ScreenSize.Height / 2.0 + dy));
    }

    public PointD ScreenToWorld(PointF screen)
    {
        double dx = (screen.X - ScreenSize.Width  / 2.0) * Resolution;
        double dy = (screen.Y - ScreenSize.Height / 2.0) * Resolution;
        return new PointD(Center.X + dx, Center.Y - dy);
    }

    /// <summary>Visible world extent in WGS-84 degrees (approximate for Mercator).</summary>
    public RectangleD VisibleExtent()
    {
        var tl = ScreenToWorld(new PointF(0, 0));
        var br = ScreenToWorld(new PointF(ScreenSize.Width, ScreenSize.Height));
        return new RectangleD(tl.X, br.Y, br.X - tl.X, tl.Y - br.Y);
    }

    /// <summary>Screen rectangle for a world-space bounding box.</summary>
    public RectangleF WorldToScreenRect(RectangleD world)
    {
        var tl = WorldToScreen(new PointD(world.Left,  world.Top));
        var br = WorldToScreen(new PointD(world.Right, world.Bottom));
        return RectangleF.FromLTRB(tl.X, tl.Y, br.X, br.Y);
    }

    // ── Immutable mutators ────────────────────────────────────────────────
    public Viewport WithCenter(PointD c)     => new() { Center = c,     ZoomLevel = ZoomLevel, ScreenSize = ScreenSize };
    public Viewport WithZoom(double z)       => new() { Center = Center, ZoomLevel = z,         ScreenSize = ScreenSize };
    public Viewport WithScreenSize(Size s)   => new() { Center = Center, ZoomLevel = ZoomLevel, ScreenSize = s };

    // ── Scale helpers ─────────────────────────────────────────────────────
    /// <summary>Ground distance per pixel in metres, accounting for latitude distortion.</summary>
    public double GroundResolutionAtLatitude(double latDegrees)
    {
        double lat = latDegrees * Math.PI / 180.0;
        return Math.Cos(lat) * 2 * Math.PI * 6378137.0 / (256 * Math.Pow(2, ZoomLevel));
    }

    /// <summary>Approximate map scale denominator (1 : N).</summary>
    public double ScaleDenominator(double latDegrees, double screenDpi = 96)
    {
        double mPerPixel = GroundResolutionAtLatitude(latDegrees);
        double mPerDot   = 0.0254 / screenDpi;
        return mPerPixel / mPerDot;
    }
}
```

### Coordinate System Notes

| System | Units | Origin | Used for |
|--------|-------|--------|----------|
| WGS-84 geographic | degrees lon/lat | Prime meridian / equator | Data storage, GPS |
| Web Mercator (EPSG:3857) | metres | Null island | Tile math, rendering |
| Screen pixels | px | Top-left of control | Drawing |
| Tile XYZ | integer | Top-left at zoom 0 | Cache keys, tile requests |

The `Viewport` works in Web Mercator internally. Input data in WGS-84 is projected before passing to `WorldToScreen`.

```csharp
// ProjectionUtils.cs
public static class ProjectionUtils
{
    public static PointD Wgs84ToMercator(double lon, double lat)
    {
        double x = lon * Math.PI / 180.0 * 6378137.0;
        double y = Math.Log(Math.Tan(Math.PI / 4.0 + lat * Math.PI / 360.0)) * 6378137.0;
        return new PointD(x, y);
    }

    public static (double lon, double lat) MercatorToWgs84(double x, double y)
    {
        double lon = x / 6378137.0 * 180.0 / Math.PI;
        double lat = (2 * Math.Atan(Math.Exp(y / 6378137.0)) - Math.PI / 2) * 180.0 / Math.PI;
        return (lon, lat);
    }
}
```

---

## 6. Tile XYZ Spatial Query

Given a visible extent and a zoom level, this section computes exactly which tiles to request. Over-requesting wastes bandwidth and memory; under-requesting leaves gaps on screen.

```csharp
// Core/TileUtils.cs
public static class TileUtils
{
    // ── Tile coordinate math ──────────────────────────────────────────────
    public static int LonToTileX(double lonDeg, int zoom) =>
        (int)Math.Floor((lonDeg + 180.0) / 360.0 * (1 << zoom));

    public static int LatToTileY(double latDeg, int zoom)
    {
        double rad = latDeg * Math.PI / 180.0;
        return (int)Math.Floor(
            (1.0 - Math.Log(Math.Tan(rad) + 1.0 / Math.Cos(rad)) / Math.PI)
            / 2.0 * (1 << zoom));
    }

    // ── Visible tile set ─────────────────────────────────────────────────
    /// <summary>All tile IDs that overlap the given WGS-84 extent at the given zoom.</summary>
    public static IEnumerable<TileId> GetVisibleTiles(RectangleD extentWgs84, int zoom)
    {
        zoom = Math.Clamp(zoom, 0, 22);
        int xMin = Math.Max(0, LonToTileX(extentWgs84.Left,  zoom));
        int xMax = Math.Min((1 << zoom) - 1, LonToTileX(extentWgs84.Right, zoom));
        int yMin = Math.Max(0, LatToTileY(extentWgs84.Top,   zoom));
        int yMax = Math.Min((1 << zoom) - 1, LatToTileY(extentWgs84.Bottom, zoom));

        for (int x = xMin; x <= xMax; x++)
        for (int y = yMin; y <= yMax; y++)
            yield return new TileId(zoom, x, y);
    }

    // ── Tile world bounds ─────────────────────────────────────────────────
    /// <summary>WGS-84 bounding box for a tile.</summary>
    public static RectangleD TileToBoundsWgs84(TileId t)
    {
        int n = 1 << t.Z;
        double west  =  t.X       / (double)n * 360.0 - 180.0;
        double east  = (t.X + 1)  / (double)n * 360.0 - 180.0;
        double north = TileYToLat(t.Y,     t.Z);
        double south = TileYToLat(t.Y + 1, t.Z);
        return new RectangleD(west, south, east - west, north - south);
    }

    private static double TileYToLat(int y, int z)
    {
        double n = Math.PI - 2.0 * Math.PI * y / (1 << z);
        return 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
    }

    // ── Priority sort ─────────────────────────────────────────────────────
    /// <summary>
    /// Orders tiles by Manhattan distance to the screen-centre tile.
    /// Centre tiles load first for a visually pleasing progressive fill.
    /// </summary>
    public static List<TileId> PrioritisedTiles(IEnumerable<TileId> tiles, TileId centre)
        => tiles
            .OrderBy(t => Math.Abs(t.X - centre.X) + Math.Abs(t.Y - centre.Y))
            .ToList();

    /// <summary>Centre tile ID for the current viewport.</summary>
    public static TileId CentreTile(Viewport vp)
    {
        int z = vp.TileZoom;
        return new TileId(z,
            LonToTileX(vp.Center.X, z),
            LatToTileY(vp.Center.Y, z));
    }

    // ── TMS ↔ XYZ ─────────────────────────────────────────────────────────
    /// <summary>MBTiles uses TMS (Y flipped). Convert before querying SQLite.</summary>
    public static int XyzToTmsY(int y, int zoom) => (1 << zoom) - 1 - y;
}
```

---

## 7. Tile Cache — L1 Memory (LRU)

The L1 cache holds decoded `Bitmap` objects in memory in an LRU (least-recently-used) linked list. When the cache reaches its capacity limit, the oldest entry is evicted and its bitmap disposed. The dictionary provides O(1) lookup; the linked list provides O(1) eviction.

```csharp
// Cache/MemoryTileCache.cs
public sealed class MemoryTileCache : IDisposable
{
    private readonly int _capacity;
    private readonly Dictionary<TileId, (Bitmap Bmp, LinkedListNode<TileId> Node)> _map;
    private readonly LinkedList<TileId> _lru = new();
    private readonly object _lock = new();

    public int Count { get { lock (_lock) return _map.Count; } }

    public MemoryTileCache(int capacity = 300)
    {
        _capacity = capacity;
        _map      = new Dictionary<TileId, (Bitmap, LinkedListNode<TileId>)>(capacity + 8);
    }

    // ── Get ───────────────────────────────────────────────────────────────
    public Bitmap? Get(TileId id)
    {
        lock (_lock)
        {
            if (!_map.TryGetValue(id, out var entry)) return null;
            // Move to front (most recently used)
            _lru.Remove(entry.Node);
            _lru.AddFirst(entry.Node);
            return entry.Bmp;
        }
    }

    // ── Put ───────────────────────────────────────────────────────────────
    public void Put(TileId id, Bitmap bmp)
    {
        lock (_lock)
        {
            if (_map.ContainsKey(id)) return; // already cached

            while (_map.Count >= _capacity && _lru.Last != null)
            {
                var evict = _lru.Last.Value;
                _lru.RemoveLast();
                if (_map.Remove(evict, out var old))
                    old.Bmp.Dispose();
            }

            var node = _lru.AddFirst(id);
            _map[id] = (bmp, node);
        }
    }

    // ── Remove ────────────────────────────────────────────────────────────
    public bool Remove(TileId id)
    {
        lock (_lock)
        {
            if (!_map.Remove(id, out var entry)) return false;
            _lru.Remove(entry.Node);
            entry.Bmp.Dispose();
            return true;
        }
    }

    // ── Invalidate zoom level ─────────────────────────────────────────────
    public void InvalidateZoom(int zoom)
    {
        lock (_lock)
        {
            var toRemove = _map.Keys.Where(k => k.Z == zoom).ToList();
            foreach (var id in toRemove) Remove(id);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            foreach (var entry in _map.Values) entry.Bmp.Dispose();
            _map.Clear();
            _lru.Clear();
        }
    }

    public void Dispose() => Clear();
}
```

**Capacity sizing guide:**

| Use case | Recommended L1 capacity | Approx. RAM (256×256 RGBA) |
|----------|------------------------|---------------------------|
| Desktop single monitor | 300 tiles | ~75 MB |
| Large dual monitor | 600 tiles | ~150 MB |
| Memory-constrained | 100 tiles | ~25 MB |
| Unlimited (use with caution) | int.MaxValue | Unbounded |

---

## 8. Tile Cache — L2 Disk

The L2 disk cache persists decoded tiles as PNG files keyed by `z_x_y.png`. It acts as a fallback when L1 is cold (app restart, cache eviction). Writes are always async to avoid blocking the render thread.

```csharp
// Cache/DiskTileCache.cs
public sealed class DiskTileCache
{
    private readonly string _root;
    private readonly long   _maxBytes;
    private long            _usedBytes;

    public DiskTileCache(string rootPath, long maxBytes = 512 * 1024 * 1024) // 512 MB default
    {
        _root     = rootPath;
        _maxBytes = maxBytes;
        Directory.CreateDirectory(_root);
        _usedBytes = Directory.EnumerateFiles(_root, "*.png", SearchOption.AllDirectories)
                              .Sum(f => new FileInfo(f).Length);
    }

    public string KeyPath(TileId id) =>
        Path.Combine(_root, id.Z.ToString(), $"{id.Z}_{id.X}_{id.Y}.png");

    // ── Get ───────────────────────────────────────────────────────────────
    public Bitmap? Get(TileId id)
    {
        string path = KeyPath(id);
        if (!File.Exists(path)) return null;
        try
        {
            // Copy bytes before constructing Bitmap so the file handle is released
            var bytes = File.ReadAllBytes(path);
            using var ms = new MemoryStream(bytes);
            return new Bitmap(ms);
        }
        catch { return null; }
    }

    // ── Put (async) ───────────────────────────────────────────────────────
    public Task PutAsync(TileId id, Bitmap bmp)
    {
        return Task.Run(() =>
        {
            string path = KeyPath(id);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            if (File.Exists(path)) return; // already on disk

            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] bytes = ms.ToArray();

            File.WriteAllBytes(path, bytes);
            Interlocked.Add(ref _usedBytes, bytes.Length);

            if (_usedBytes > _maxBytes)
                Task.Run(EvictOldest); // background eviction
        });
    }

    // ── Evict oldest files until under quota ─────────────────────────────
    private void EvictOldest()
    {
        var files = Directory.EnumerateFiles(_root, "*.png", SearchOption.AllDirectories)
            .Select(f => new FileInfo(f))
            .OrderBy(f => f.LastAccessTimeUtc)
            .ToList();

        foreach (var fi in files)
        {
            if (_usedBytes <= _maxBytes * 0.8) break; // trim to 80%
            try
            {
                long len = fi.Length;
                fi.Delete();
                Interlocked.Add(ref _usedBytes, -len);
            }
            catch { /* ignore locked files */ }
        }
    }

    // ── Check existence without decoding ─────────────────────────────────
    public bool Contains(TileId id) => File.Exists(KeyPath(id));

    public void Clear()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
        Directory.CreateDirectory(_root);
        _usedBytes = 0;
    }
}
```

### Combined TileCache Facade

```csharp
// Cache/TileCache.cs
public sealed class TileCache : IDisposable
{
    public  MemoryTileCache L1   { get; }
    private DiskTileCache   _l2;

    public TileCache(string diskPath, int l1Capacity = 300, long l2MaxBytes = 512L * 1024 * 1024)
    {
        L1  = new MemoryTileCache(l1Capacity);
        _l2 = new DiskTileCache(diskPath, l2MaxBytes);
    }

    public Bitmap? Get(TileId id)
    {
        var bmp = L1.Get(id);
        if (bmp != null) return bmp;

        bmp = _l2.Get(id);
        if (bmp != null) { L1.Put(id, bmp); } // promote to L1
        return bmp;
    }

    public void Put(TileId id, Bitmap bmp)
    {
        L1.Put(id, bmp);
        _ = _l2.PutAsync(id, bmp); // fire-and-forget disk write
    }

    public bool Contains(TileId id) => L1.Get(id) != null || _l2.Contains(id);

    public void Dispose() => L1.Dispose();
}
```

---

## 9. Zoom Timer & Debounce

When the user scrolls quickly, the mouse wheel fires dozens of events per second. Re-fetching and re-compositing tiles on every event causes severe jank. The zoom timer **debounces** this: it resets on every wheel event and fires the full quality rebuild only after 200 ms of inactivity.

During the debounce window, the existing back buffer is scaled in-place as a **preview**. This is cheap (`Graphics.ScaleTransform`) and keeps the map visually responsive at the cost of temporary pixel blur.

```csharp
// MapCanvas.cs  (zoom input section)
public sealed partial class MapCanvas : Panel
{
    private readonly System.Windows.Forms.Timer _zoomTimer;
    private double _pendingZoom;
    private PointF _zoomOriginScreen; // point under cursor
    private Bitmap? _zoomPreview;

    private void InitZoomTimer()
    {
        _zoomTimer = new System.Windows.Forms.Timer { Interval = 200 };
        _zoomTimer.Tick += OnZoomSettled;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        double delta = e.Delta > 0 ? 0.75 : -0.75;  // zoom step per wheel notch
        _pendingZoom      = Math.Clamp(Viewport.ZoomLevel + delta, 0, 22);
        _zoomOriginScreen = e.Location;
        _isZooming        = true;

        // Build a cheap scaled preview from the current back buffer
        _zoomPreview?.Dispose();
        _zoomPreview = BuildZoomPreview(_pendingZoom, e.Location);
        Invalidate();

        // Reset debounce window
        _zoomTimer.Stop();
        _zoomTimer.Start();
    }

    private void OnZoomSettled(object? sender, EventArgs e)
    {
        _zoomTimer.Stop();
        _isZooming = false;

        // Adjust centre so the point under cursor stays fixed
        var worldUnderCursor = Viewport.ScreenToWorld(_zoomOriginScreen);
        Viewport = Viewport.WithZoom(_pendingZoom);
        var newScreenPt = Viewport.WorldToScreen(worldUnderCursor);
        var offset = new PointD(
            (_zoomOriginScreen.X - newScreenPt.X) * Viewport.Resolution,
            (_zoomOriginScreen.Y - newScreenPt.Y) * -Viewport.Resolution);
        Viewport = Viewport.WithCenter(new PointD(
            Viewport.Center.X + offset.X,
            Viewport.Center.Y + offset.Y));

        // Full quality re-render
        RequestFullRender();
    }

    private Bitmap BuildZoomPreview(double targetZoom, PointF originPx)
    {
        var src    = _compositor.CurrentBuffer;
        var result = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        double scale = Math.Pow(2, targetZoom - Viewport.ZoomLevel);
        using var g = Graphics.FromImage(result);
        g.Clear(BackColor);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        // Scale around the cursor point
        g.TranslateTransform(originPx.X, originPx.Y);
        g.ScaleTransform((float)scale, (float)scale);
        g.TranslateTransform(-originPx.X, -originPx.Y);
        if (src != null) g.DrawImage(src, 0, 0);
        return result;
    }
}
```

### Zoom Preview in OnPaint

```csharp
protected override void OnPaint(PaintEventArgs e)
{
    if (_isZooming && _zoomPreview != null)
    {
        // Low-quality fast path during gesture
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        e.Graphics.DrawImage(_zoomPreview, 0, 0);
        DrawDecorations(e.Graphics);
        return;
    }
    e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
    _compositor.DrawTo(e.Graphics);
    DrawDecorations(e.Graphics);
}
```

---

## 10. Pan Bitmap Cache & Dirty Regions

Panning is the most frequent operation. The key insight is that during a pan, most of the screen content is still valid — it has simply moved. Translating the existing back buffer by the pan delta and re-filling only the newly exposed strip is far cheaper than a full re-render.

```csharp
// Cache/PanCache.cs
public sealed class PanCache
{
    public List<Rectangle> DirtyRegions { get; } = new();

    /// <summary>
    /// Translates the source bitmap by (dx, dy) pixels into a new bitmap
    /// and records which screen regions are now uncovered (dirty).
    /// </summary>
    public Bitmap Translate(Bitmap source, int dx, int dy, Size screenSize)
    {
        DirtyRegions.Clear();

        var result = new Bitmap(
            screenSize.Width, screenSize.Height,
            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

        using var g = Graphics.FromImage(result);
        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

        // Draw existing content at offset
        g.DrawImage(source, dx, dy);

        // Record dirty strips
        if (dx > 0)
            DirtyRegions.Add(new Rectangle(0, 0, dx, screenSize.Height));
        else if (dx < 0)
            DirtyRegions.Add(new Rectangle(screenSize.Width + dx, 0, -dx, screenSize.Height));

        if (dy > 0)
            DirtyRegions.Add(new Rectangle(0, 0, screenSize.Width, dy));
        else if (dy < 0)
            DirtyRegions.Add(new Rectangle(0, screenSize.Height + dy, screenSize.Width, -dy));

        return result;
    }
}
```

### Using PanCache in MapCanvas

```csharp
// MapCanvas.cs (pan input section)
protected override void OnMouseDown(MouseEventArgs e)
{
    if (e.Button != MouseButtons.Left) return;
    _isPanning    = true;
    _panStart     = e.Location;
    _panLastScreen = e.Location;
    Cursor        = Cursors.SizeAll;
}

protected override void OnMouseMove(MouseEventArgs e)
{
    if (!_isPanning) return;

    int dx = (int)(e.X - _panLastScreen.X);
    int dy = (int)(e.Y - _panLastScreen.Y);
    _panLastScreen = e.Location;

    // Translate existing buffer immediately — no background work needed
    var translated = _panCache.Translate(_compositor.CurrentBuffer!, dx, dy, ClientSize);
    _compositor.SetBuffer(translated);
    Invalidate();

    // Adjust viewport centre
    var worldDelta = new PointD(
        -dx * Viewport.Resolution,
         dy * Viewport.Resolution);
    Viewport = Viewport.WithCenter(new PointD(
        Viewport.Center.X + worldDelta.X,
        Viewport.Center.Y + worldDelta.Y));

    // Schedule tile fill for dirty regions (debounced 50 ms)
    _panDirtyTimer.Stop();
    _panDirtyTimer.Start();
}

protected override void OnMouseUp(MouseEventArgs e)
{
    if (!_isPanning) return;
    _isPanning = false;
    Cursor     = Cursors.Cross;
    RequestFullRender(); // full quality re-render when pan ends
}
```

---

## 11. Dynamic Image Quality by Zoom Level

GDI+ offers multiple interpolation modes with very different performance/quality trade-offs. The correct mode depends on:

1. Whether the user is actively gesturing (zoom/pan)
2. The current zoom level (close-up vs overview)
3. Whether the source tile is being scaled up or down

```csharp
// Rendering/RenderQuality.cs
public static class RenderQuality
{
    public enum RenderState { Idle, Panning, Zooming }

    public readonly struct QualitySettings
    {
        public System.Drawing.Drawing2D.InterpolationMode Interpolation { get; init; }
        public System.Drawing.Drawing2D.SmoothingMode     Smoothing     { get; init; }
        public System.Drawing.Drawing2D.PixelOffsetMode   PixelOffset   { get; init; }
        public System.Drawing.Drawing2D.CompositingQuality Compositing  { get; init; }
    }

    // Fastest — used during zoom/pan gestures
    private static readonly QualitySettings _gesture = new()
    {
        Interpolation = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor,
        Smoothing     = System.Drawing.Drawing2D.SmoothingMode.None,
        PixelOffset   = System.Drawing.Drawing2D.PixelOffsetMode.None,
        Compositing   = System.Drawing.Drawing2D.CompositingQuality.HighSpeed
    };

    // Balanced — used while panning after settling
    private static readonly QualitySettings _pan = new()
    {
        Interpolation = System.Drawing.Drawing2D.InterpolationMode.Bilinear,
        Smoothing     = System.Drawing.Drawing2D.SmoothingMode.None,
        PixelOffset   = System.Drawing.Drawing2D.PixelOffsetMode.Half,
        Compositing   = System.Drawing.Drawing2D.CompositingQuality.Default
    };

    // High quality — overview zoom (zoom < 10)
    private static readonly QualitySettings _overview = new()
    {
        Interpolation = System.Drawing.Drawing2D.InterpolationMode.Bilinear,
        Smoothing     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias,
        PixelOffset   = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality,
        Compositing   = System.Drawing.Drawing2D.CompositingQuality.HighQuality
    };

    // Maximum quality — street zoom (zoom >= 14)
    private static readonly QualitySettings _street = new()
    {
        Interpolation = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic,
        Smoothing     = System.Drawing.Drawing2D.SmoothingMode.HighQuality,
        PixelOffset   = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality,
        Compositing   = System.Drawing.Drawing2D.CompositingQuality.HighQuality
    };

    public static QualitySettings For(double zoom, RenderState state)
    {
        if (state == RenderState.Zooming) return _gesture;
        if (state == RenderState.Panning) return _pan;
        if (zoom >= 14)  return _street;
        if (zoom >= 10)  return _overview;
        return _overview; // overview default for idle
    }

    public static void Apply(Graphics g, double zoom, RenderState state)
    {
        var q = For(zoom, state);
        g.InterpolationMode      = q.Interpolation;
        g.SmoothingMode          = q.Smoothing;
        g.PixelOffsetMode        = q.PixelOffset;
        g.CompositingQuality     = q.Compositing;
    }
}
```

### Zoom-Level Tile Selection Quality

Beyond interpolation mode, quality is also controlled by which zoom level is used to request tiles:

```csharp
// During smooth zoom animation, request one zoom level coarser.
// The tile covers 4× the area but loads 4× faster.
// Swap to correct zoom once zoom timer fires.

public static int EffectiveTileZoom(double viewportZoom, bool isAnimating)
{
    int baseZoom = (int)Math.Round(viewportZoom);
    if (isAnimating) return Math.Max(0, baseZoom - 1);
    return baseZoom;
}

// Source tile scale factor relative to viewport — drives interpolation choice
public static float TileScaleFactor(int tileZoom, double viewportZoom)
    => (float)Math.Pow(2, viewportZoom - tileZoom);
// > 1.0  →  tile is being scaled up (zoomed in past tile boundary)  →  use bicubic
// < 1.0  →  tile is being scaled down (zoomed out past tile boundary) → use bilinear (sharper)
// = 1.0  →  exact match → NearestNeighbor is fine (no resampling needed)
```

---

## 12. Deferred Composite Off-Screen Rendering

The compositor is the heart of the rendering system. It runs entirely on a background thread, composites all visible layers into a single `Bitmap`, then hands it off to the UI thread for a single fast `DrawImage`.

```csharp
// Rendering/LayerCompositor.cs
public sealed class LayerCompositor : IDisposable
{
    private readonly Control        _host;
    private readonly LayerManager   _layers;
    private readonly TileCache      _cache;

    private Bitmap? _frontBuffer;     // read by UI thread
    private Bitmap? _backBuffer;      // written by render thread
    private readonly object _bufLock = new();

    private CancellationTokenSource _cts = new();
    private int _renderGen = 0;       // monotonic counter for stale-render detection

    public Bitmap? CurrentBuffer { get { lock (_bufLock) return _frontBuffer; } }

    public LayerCompositor(Control host, LayerManager layers, TileCache cache)
    {
        _host   = host;
        _layers = layers;
        _cache  = cache;
    }

    // ── Request a new render ─────────────────────────────────────────────
    public void RequestRender(Viewport vp)
    {
        int gen = Interlocked.Increment(ref _renderGen);
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(() => RenderAsync(vp, gen, token), token);
    }

    // ── Background render task ───────────────────────────────────────────
    private void RenderAsync(Viewport vp, int gen, CancellationToken token)
    {
        int w = vp.ScreenSize.Width;
        int h = vp.ScreenSize.Height;
        if (w < 1 || h < 1) return;

        // Format32bppPArgb is ~30% faster for GDI+ DrawImage than Format32bppArgb
        var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

        try
        {
            using var g = Graphics.FromImage(bmp);
            RenderQuality.Apply(g, vp.ZoomLevel, RenderQuality.RenderState.Idle);
            g.Clear(Color.FromArgb(24, 24, 28));

            foreach (var layer in _layers.VisibleLayers)
            {
                if (token.IsCancellationRequested) { bmp.Dispose(); return; }
                if (!layer.IsVisible) continue;
                layer.Render(g, vp, _cache, token);
            }
        }
        catch (OperationCanceledException)
        {
            bmp.Dispose();
            return;
        }

        // Only publish if this is still the most recent render generation
        if (gen != _renderGen) { bmp.Dispose(); return; }

        Bitmap? old;
        lock (_bufLock)
        {
            old          = _frontBuffer;
            _frontBuffer = bmp;
        }
        old?.Dispose();

        // Post invalidate to UI thread
        if (!_host.IsDisposed)
            _host.BeginInvoke(() => _host.Invalidate());
    }

    // ── Replace buffer directly (used by pan cache) ──────────────────────
    public void SetBuffer(Bitmap bmp)
    {
        Bitmap? old;
        lock (_bufLock)
        {
            old          = _frontBuffer;
            _frontBuffer = bmp;
        }
        old?.Dispose();
    }

    // ── Draw to screen ───────────────────────────────────────────────────
    public void DrawTo(Graphics g)
    {
        Bitmap? buf;
        lock (_bufLock) buf = _frontBuffer;
        if (buf != null) g.DrawImage(buf, 0, 0);
    }

    public void Dispose()
    {
        _cts.Cancel();
        lock (_bufLock)
        {
            _frontBuffer?.Dispose();
            _backBuffer?.Dispose();
        }
    }
}
```

---

## 13. Raster Layer — Tile Stitching & Placeholders

The raster layer's `Render` method takes tiles from the cache and stitches them onto the `Graphics` surface at exact screen positions. When a tile is not yet loaded, it falls back to a **scaled parent tile** to avoid blank gaps.

```csharp
// Layers/RasterLayer.cs
public sealed class RasterLayer : IMapLayer
{
    public string         Name       { get; set; } = "Raster";
    public bool           IsVisible  { get; set; } = true;
    public float          Opacity    { get; set; } = 1.0f;
    public IRasterTileSource Source  { get; set; }

    public RasterLayer(IRasterTileSource source) => Source = source;

    public void Render(Graphics g, Viewport vp, TileCache cache, CancellationToken token)
    {
        int zoom     = vp.TileZoom;
        var tiles    = TileUtils.GetVisibleTiles(vp.VisibleExtent(), zoom).ToList();
        var ia       = BuildImageAttributes(Opacity);

        foreach (var id in tiles)
        {
            if (token.IsCancellationRequested) return;

            var bmp = cache.Get(id);

            // Fallback: scale up a parent tile as a placeholder
            if (bmp == null)
            {
                bmp = GetParentPlaceholder(id, cache, out var srcRect, out var isTile);
                if (bmp != null && isTile)
                {
                    DrawTileWithSrcRect(g, bmp, srcRect, id, vp, ia);
                    continue;
                }
            }

            if (bmp == null) continue;
            DrawTile(g, bmp, id, vp, ia);
        }
    }

    // ── Draw a full tile ─────────────────────────────────────────────────
    private static void DrawTile(
        Graphics g, Bitmap bmp, TileId id, Viewport vp,
        System.Drawing.Imaging.ImageAttributes ia)
    {
        var destRect = TileScreenRect(id, vp);
        if (destRect.Width < 1 || destRect.Height < 1) return;
        g.DrawImage(bmp,
            new Rectangle((int)destRect.X, (int)destRect.Y,
                           (int)destRect.Width, (int)destRect.Height),
            0, 0, bmp.Width, bmp.Height,
            GraphicsUnit.Pixel, ia);
    }

    // ── Draw a cropped portion of a parent tile ──────────────────────────
    private static void DrawTileWithSrcRect(
        Graphics g, Bitmap bmp, Rectangle srcRect, TileId id, Viewport vp,
        System.Drawing.Imaging.ImageAttributes ia)
    {
        var destRect = TileScreenRect(id, vp);
        if (destRect.Width < 1 || destRect.Height < 1) return;
        g.DrawImage(bmp,
            new Rectangle((int)destRect.X, (int)destRect.Y,
                           (int)destRect.Width, (int)destRect.Height),
            srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
            GraphicsUnit.Pixel, ia);
    }

    // ── Compute screen dest rect for a tile ──────────────────────────────
    private static RectangleF TileScreenRect(TileId id, Viewport vp)
    {
        var bounds = TileUtils.TileToBoundsWgs84(id);
        var tl     = vp.WorldToScreen(new PointD(bounds.Left, bounds.Top));
        var br     = vp.WorldToScreen(new PointD(bounds.Right, bounds.Bottom));
        return new RectangleF(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);
    }

    // ── Parent tile placeholder ──────────────────────────────────────────
    private static Bitmap? GetParentPlaceholder(
        TileId id, TileCache cache, out Rectangle srcRect, out bool found)
    {
        srcRect = Rectangle.Empty;
        found   = false;

        // Walk up to 3 zoom levels to find a cached ancestor
        var current = id;
        int steps   = 0;
        while (current.Z > 0 && steps < 3)
        {
            current = current.Parent;
            steps++;
            var parentBmp = cache.Get(current);
            if (parentBmp == null) continue;

            // Determine which quadrant of the parent covers the requested tile
            int factor    = 1 << steps;         // 2, 4, or 8
            int tileSize  = parentBmp.Width / factor;
            int qx        = (id.X % factor) * tileSize;
            int qy        = (id.Y % factor) * tileSize;
            srcRect       = new Rectangle(qx, qy, tileSize, tileSize);
            found         = true;
            return parentBmp;
        }
        return null;
    }

    // ── Opacity image attributes ─────────────────────────────────────────
    private static System.Drawing.Imaging.ImageAttributes BuildImageAttributes(float opacity)
    {
        var cm = new System.Drawing.Imaging.ColorMatrix
        { Matrix33 = Math.Clamp(opacity, 0f, 1f) };
        var ia = new System.Drawing.Imaging.ImageAttributes();
        ia.SetColorMatrix(cm,
            System.Drawing.Imaging.ColorMatrixFlag.Default,
            System.Drawing.Imaging.ColorAdjustType.Bitmap);
        return ia;
    }
}
```

---

## 14. MBTiles Raster Source (SQLite)

MBTiles is a SQLite database where each tile blob is stored as PNG or JPEG. The Y axis uses TMS convention (flipped relative to XYZ) so conversion is required.

```csharp
// Sources/MbTilesSource.cs
public sealed class MbTilesSource : IRasterTileSource, IDisposable
{
    private readonly string _path;
    // One connection per thread — SQLite is not thread-safe across connections
    private readonly System.Threading.ThreadLocal<System.Data.SQLite.SQLiteConnection> _conn;

    public string Name  { get; }
    public int    MinZoom { get; private set; }
    public int    MaxZoom { get; private set; }
    public string Format  { get; private set; } = "png";

    public MbTilesSource(string path)
    {
        _path = path;
        Name  = Path.GetFileNameWithoutExtension(path);
        _conn = new System.Threading.ThreadLocal<System.Data.SQLite.SQLiteConnection>(() =>
        {
            var c = new System.Data.SQLite.SQLiteConnection(
                $"Data Source={path};Version=3;Read Only=True;Pooling=True;");
            c.Open();
            return c;
        });
        LoadMetadata();
    }

    // ── Load metadata (zoom range, format) ───────────────────────────────
    private void LoadMetadata()
    {
        using var cmd = _conn.Value!.CreateCommand();
        cmd.CommandText = "SELECT name, value FROM metadata";
        using var r = cmd.ExecuteReader();
        var meta = new Dictionary<string, string>();
        while (r.Read()) meta[r.GetString(0)] = r.GetString(1);

        if (meta.TryGetValue("minzoom", out var minZ)) MinZoom = int.Parse(minZ);
        if (meta.TryGetValue("maxzoom", out var maxZ)) MaxZoom = int.Parse(maxZ);
        if (meta.TryGetValue("format",  out var fmt))  Format  = fmt;
    }

    // ── Single tile ───────────────────────────────────────────────────────
    public Bitmap? GetTile(TileId id)
    {
        if (id.Z < MinZoom || id.Z > MaxZoom) return null;
        int tmsY = TileUtils.XyzToTmsY(id.Y, id.Z);

        using var cmd = _conn.Value!.CreateCommand();
        cmd.CommandText =
            "SELECT tile_data FROM tiles " +
            "WHERE zoom_level=@z AND tile_column=@x AND tile_row=@y";
        cmd.Parameters.AddWithValue("@z", id.Z);
        cmd.Parameters.AddWithValue("@x", id.X);
        cmd.Parameters.AddWithValue("@y", tmsY);

        var raw = cmd.ExecuteScalar() as byte[];
        if (raw == null) return null;
        return DecodeTile(raw);
    }

    // ── Batch tile fetch (much faster than N individual calls) ───────────
    public Dictionary<TileId, Bitmap> GetTiles(IEnumerable<TileId> ids)
    {
        var list   = ids.ToList();
        var result = new Dictionary<TileId, Bitmap>(list.Count);
        if (list.Count == 0) return result;

        // Build parameterised IN clause
        var paramList = list.Select((t, i) => $"(@z{i},@x{i},@y{i})");
        using var cmd  = _conn.Value!.CreateCommand();
        cmd.CommandText =
            $"SELECT zoom_level, tile_column, tile_row, tile_data FROM tiles " +
            $"WHERE (zoom_level, tile_column, tile_row) IN ({string.Join(",", paramList)})";

        for (int i = 0; i < list.Count; i++)
        {
            int tmsY = TileUtils.XyzToTmsY(list[i].Y, list[i].Z);
            cmd.Parameters.AddWithValue($"@z{i}", list[i].Z);
            cmd.Parameters.AddWithValue($"@x{i}", list[i].X);
            cmd.Parameters.AddWithValue($"@y{i}", tmsY);
        }

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int z = reader.GetInt32(0), x = reader.GetInt32(1);
            int tmsYr = reader.GetInt32(2);
            int xyzY  = TileUtils.XyzToTmsY(tmsYr, z); // flip back
            var raw   = (byte[])reader[3];
            var bmp   = DecodeTile(raw);
            if (bmp != null)
                result[new TileId(z, x, xyzY)] = bmp;
        }
        return result;
    }

    // ── Decode PNG or JPEG blob ───────────────────────────────────────────
    private static Bitmap? DecodeTile(byte[] raw)
    {
        try
        {
            using var ms = new MemoryStream(raw);
            // Clone to detach from MemoryStream before it disposes
            return (Bitmap)Image.FromStream(ms).Clone();
        }
        catch { return null; }
    }

    public void Dispose()
    {
        if (_conn.IsValueCreated) _conn.Value?.Dispose();
        _conn.Dispose();
    }
}
```

---

## 15. GeoTIFF Raster Source & Overview Pyramids

GeoTIFF files frequently embed multiple **overview levels** (internal reduced-resolution copies) using the TIFF directory structure. Selecting the right overview at query time avoids loading full-resolution data when the viewport only needs an overview.

```csharp
// Sources/GeoTiffSource.cs  (requires BitMiracle.LibTiff.NET)
public sealed class GeoTiffSource : IRasterTileSource, IDisposable
{
    private readonly string _path;
    private readonly object _lock = new();

    // ── Geotransform from TIFF tags ───────────────────────────────────────
    // [0]=originX  [1]=pixelW  [2]=rotX  [3]=originY  [4]=rotY  [5]=pixelH(neg)
    private double[] _gt = new double[6];
    private int      _fullW, _fullH;
    private int      _overviewCount;

    public string Name { get; }

    public GeoTiffSource(string path)
    {
        _path = path;
        Name  = Path.GetFileNameWithoutExtension(path);
        ReadMetadata();
    }

    private void ReadMetadata()
    {
        using var tiff = BitMiracle.LibTiff.Classic.Tiff.Open(_path, "r");
        if (tiff == null) throw new InvalidOperationException($"Cannot open {_path}");

        _fullW         = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH)[0].ToInt();
        _fullH         = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH)[0].ToInt();
        _overviewCount = tiff.NumberOfDirectories() - 1;

        var modelPixelScale    = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.GEOTIFF_MODELPIXELSCALETAG);
        var modelTiepointTag   = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.GEOTIFF_MODELTIEPOINTTAG);
        if (modelPixelScale != null && modelTiepointTag != null)
        {
            double[] scale    = (double[])modelPixelScale[1].Value;
            double[] tiepoint = (double[])modelTiepointTag[1].Value;
            _gt[0] = tiepoint[3]; // originX
            _gt[1] = scale[0];    // pixelWidth
            _gt[3] = tiepoint[4]; // originY
            _gt[5] = -scale[1];   // pixelHeight (negative = top-down)
        }
    }

    // ── Select best overview for current viewport resolution ─────────────
    public int SelectOverviewLevel(double viewportResMetresPerPixel)
    {
        double fullRes = Math.Abs(_gt[1]); // metres/pixel at full resolution
        for (int ov = 0; ov <= _overviewCount; ov++)
        {
            double ovRes = fullRes * Math.Pow(2, ov);
            // Use the overview that is slightly finer than what we need
            if (ovRes >= viewportResMetresPerPixel * 0.9)
                return Math.Max(0, ov - 1);
        }
        return _overviewCount; // coarsest level
    }

    // ── Read a pixel window from the selected overview ───────────────────
    public Bitmap? ReadRegion(RectangleD worldExtentMercator, int overviewLevel, Size outputSizePx)
    {
        lock (_lock)
        {
            using var tiff = BitMiracle.LibTiff.Classic.Tiff.Open(_path, "r");
            if (tiff == null) return null;
            tiff.SetDirectory((short)overviewLevel);

            int ovW = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH)[0].ToInt();
            int ovH = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH)[0].ToInt();

            // Convert world extent to pixel window in this overview
            double scale = Math.Pow(2, overviewLevel);
            int px0 = (int)((worldExtentMercator.Left   - _gt[0]) / (_gt[1] * scale));
            int py0 = (int)((worldExtentMercator.Top    - _gt[3]) / (_gt[5] * scale));
            int px1 = (int)((worldExtentMercator.Right  - _gt[0]) / (_gt[1] * scale));
            int py1 = (int)((worldExtentMercator.Bottom - _gt[3]) / (_gt[5] * scale));

            px0 = Math.Clamp(px0, 0, ovW - 1);
            py0 = Math.Clamp(py0, 0, ovH - 1);
            px1 = Math.Clamp(px1, px0 + 1, ovW);
            py1 = Math.Clamp(py1, py0 + 1, ovH);

            int readW = px1 - px0;
            int readH = py1 - py0;
            if (readW < 1 || readH < 1) return null;

            // Read raw RGBA scanlines
            int scanSize = tiff.ScanlineSize();
            var raw      = new byte[readH * scanSize];
            for (int row = 0; row < readH; row++)
            {
                byte[] buf = new byte[scanSize];
                tiff.ReadScanline(buf, py0 + row);
                Buffer.BlockCopy(buf, px0 * 4, raw, row * readW * 4, readW * 4);
            }

            // Build bitmap from RGBA bytes then resize to outputSizePx
            var src = new Bitmap(readW, readH, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bmpData = src.LockBits(
                new Rectangle(0, 0, readW, readH),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(raw, 0, bmpData.Scan0, raw.Length);
            src.UnlockBits(bmpData);

            var result = new Bitmap(outputSizePx.Width, outputSizePx.Height,
                                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, 0, 0, outputSizePx.Width, outputSizePx.Height);
            src.Dispose();
            return result;
        }
    }

    public Bitmap? GetTile(TileId id) => null; // GeoTIFF uses region reads, not tile reads
    public void Dispose() { }
}
```

---

## 16. WMS / WMTS Raster Source

WMS (Web Map Service) returns map images for arbitrary bounding boxes. WMTS returns pre-tiled images. Both are fetched over HTTP.

```csharp
// Sources/WmsSource.cs
public sealed class WmsSource : IRasterTileSource
{
    private readonly HttpClient _http;
    private readonly string     _baseUrl;
    private readonly string     _layers;
    private readonly string     _srs;
    public  string Name { get; }

    public WmsSource(string baseUrl, string layers, string srs = "EPSG:4326",
                     string name = "WMS")
    {
        _baseUrl = baseUrl.TrimEnd('?');
        _layers  = layers;
        _srs     = srs;
        Name     = name;
        _http    = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    public Bitmap? GetTile(TileId id)
    {
        var bounds = TileUtils.TileToBoundsWgs84(id);
        string bbox = _srs == "EPSG:4326"
            ? $"{bounds.Left},{bounds.Bottom},{bounds.Right},{bounds.Top}"
            : $"{bounds.Bottom},{bounds.Left},{bounds.Top},{bounds.Right}"; // axis flip for EPSG:4326 in WMS 1.3

        string url = $"{_baseUrl}?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap" +
                     $"&LAYERS={Uri.EscapeDataString(_layers)}" +
                     $"&CRS={_srs}&BBOX={bbox}" +
                     $"&WIDTH=256&HEIGHT=256&FORMAT=image/png&TRANSPARENT=TRUE";
        try
        {
            byte[] bytes = _http.GetByteArrayAsync(url).GetAwaiter().GetResult();
            using var ms = new MemoryStream(bytes);
            return (Bitmap)Image.FromStream(ms).Clone();
        }
        catch { return null; }
    }
}
```

---

## 17. HTTP Tile Server Source

Standard slippy-map HTTP tile servers (OpenStreetMap, Mapbox, ESRI, custom tile servers). URL template uses `{z}/{x}/{y}`.

```csharp
// Sources/HttpTileSource.cs
public sealed class HttpTileSource : IRasterTileSource, IDisposable
{
    private readonly HttpClient _http;
    private readonly string     _urlTemplate; // e.g. "https://tile.openstreetmap.org/{z}/{x}/{y}.png"
    public  string Name { get; }

    public HttpTileSource(string urlTemplate, string name = "HTTP",
                          string? userAgent = null)
    {
        Name          = name;
        _urlTemplate  = urlTemplate;
        _http         = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            userAgent ?? "MapCanvas/1.0 (C# WinForms GIS)");
        _http.Timeout = TimeSpan.FromSeconds(10);
    }

    // ── Async ─────────────────────────────────────────────────────────────
    public async Task<Bitmap?> GetTileAsync(TileId id, CancellationToken token = default)
    {
        string url = _urlTemplate
            .Replace("{z}", id.Z.ToString())
            .Replace("{x}", id.X.ToString())
            .Replace("{y}", id.Y.ToString());
        try
        {
            var bytes = await _http.GetByteArrayAsync(url, token);
            using var ms = new MemoryStream(bytes);
            return (Bitmap)Image.FromStream(ms).Clone();
        }
        catch (OperationCanceledException) { return null; }
        catch { return null; }
    }

    // Synchronous wrapper (used when called from TileLoader threadpool)
    public Bitmap? GetTile(TileId id) =>
        GetTileAsync(id).GetAwaiter().GetResult();

    public void Dispose() => _http.Dispose();
}
```

### Multiple Tile Server Subdomains (Load Balancing)

```csharp
// Spread requests across a, b, c subdomains for parallel loading
private static readonly string[] _subdomains = { "a", "b", "c" };

private string BuildUrl(TileId id)
{
    string sub = _subdomains[(id.X + id.Y) % _subdomains.Length];
    return _urlTemplate
        .Replace("{s}", sub)
        .Replace("{z}", id.Z.ToString())
        .Replace("{x}", id.X.ToString())
        .Replace("{y}", id.Y.ToString());
}
```

---

## 18. Async Tile Loading Pipeline

The tile loader coordinates between the spatial query, the cache, and the tile sources. It uses a bounded semaphore to limit concurrent I/O and cancels all in-flight requests when the viewport changes.

```csharp
// Interaction/TileLoader.cs
public sealed class TileLoader
{
    public event Action? TileReady;

    private readonly TileCache             _cache;
    private readonly List<IRasterTileSource> _sources;
    private readonly SemaphoreSlim         _sem = new(4); // max 4 concurrent loads
    private CancellationTokenSource        _cts = new();

    public TileLoader(TileCache cache, IEnumerable<IRasterTileSource>? sources = null)
    {
        _cache   = cache;
        _sources = sources?.ToList() ?? new List<IRasterTileSource>();
    }

    public void AddSource(IRasterTileSource source) => _sources.Add(source);

    // ── Call when viewport changes ───────────────────────────────────────
    public void LoadTilesForViewport(Viewport vp)
    {
        // Cancel all pending loads from previous viewport
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        int zoom      = vp.TileZoom;
        var extent    = vp.VisibleExtent();
        var visible   = TileUtils.GetVisibleTiles(extent, zoom).ToList();
        var centre    = TileUtils.CentreTile(vp);
        var sorted    = TileUtils.PrioritisedTiles(visible, centre);

        // Also pre-fetch one zoom level coarser (parent tiles) as placeholders
        var coarseZoom  = Math.Max(0, zoom - 2);
        var coarse      = TileUtils.GetVisibleTiles(extent, coarseZoom).ToList();

        var toLoad = coarse.Concat(sorted)
            .Where(id => !_cache.Contains(id))
            .Distinct()
            .ToList();

        foreach (var id in toLoad)
            _ = LoadOneAsync(id, token);
    }

    // ── Load a single tile ───────────────────────────────────────────────
    private async Task LoadOneAsync(TileId id, CancellationToken token)
    {
        await _sem.WaitAsync(token).ConfigureAwait(false);
        try
        {
            if (token.IsCancellationRequested) return;
            if (_cache.Contains(id)) return; // another task beat us to it

            Bitmap? bmp = null;
            foreach (var src in _sources)
            {
                bmp = src is HttpTileSource h
                    ? await h.GetTileAsync(id, token)
                    : await Task.Run(() => src.GetTile(id), token);
                if (bmp != null) break;
            }

            if (bmp != null && !token.IsCancellationRequested)
            {
                _cache.Put(id, bmp);
                TileReady?.Invoke();
            }
        }
        catch (OperationCanceledException) { }
        finally { _sem.Release(); }
    }

    // ── Pre-warm cache for an extent (e.g. offline mode setup) ──────────
    public async Task PreWarmAsync(RectangleD extent, int minZoom, int maxZoom,
                                    IProgress<(int done, int total)>? progress = null,
                                    CancellationToken token = default)
    {
        var tiles = Enumerable.Range(minZoom, maxZoom - minZoom + 1)
            .SelectMany(z => TileUtils.GetVisibleTiles(extent, z))
            .Where(id => !_cache.Contains(id))
            .ToList();

        int done = 0;
        var tasks = tiles.Select(async id =>
        {
            await LoadOneAsync(id, token);
            progress?.Report((Interlocked.Increment(ref done), tiles.Count));
        });
        await Task.WhenAll(tasks);
    }
}
```

---

## 19. Vector Layer & Spatial Culling

Vector layers draw points, lines, and polygons on top of rasters. The critical optimisation is the **spatial cull** — features whose bounding box does not intersect the current viewport extent are skipped entirely before any drawing code runs.

```csharp
// Layers/VectorLayer.cs
public sealed class VectorLayer : IMapLayer
{
    public string  Name      { get; set; } = "Vector";
    public bool    IsVisible { get; set; } = true;

    private readonly List<IFeature> _features = new();
    private ISpatialIndex? _index;

    public void AddFeature(IFeature f)
    {
        _features.Add(f);
        _index = null; // invalidate index
    }

    public void BuildIndex() => _index = new SimpleGridIndex(_features);

    // ── Render ───────────────────────────────────────────────────────────
    public void Render(Graphics g, Viewport vp, TileCache cache, CancellationToken token)
    {
        var extent   = vp.VisibleExtent();
        var features = _index != null
            ? _index.Query(extent)
            : _features.Where(f => f.Envelope.IntersectsWith(extent));

        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        foreach (var feat in features)
        {
            if (token.IsCancellationRequested) return;

            switch (feat.GeometryType)
            {
                case GeometryType.Point:
                    DrawPoint(g, feat, vp);
                    break;
                case GeometryType.LineString:
                    DrawLine(g, feat, vp);
                    break;
                case GeometryType.Polygon:
                    DrawPolygon(g, feat, vp);
                    break;
                case GeometryType.MultiPolygon:
                    foreach (var ring in feat.Rings)
                        DrawRing(g, ring, feat.Style, vp);
                    break;
            }
        }
    }

    private static void DrawPoint(Graphics g, IFeature f, Viewport vp)
    {
        var pt   = vp.WorldToScreen(f.Points[0]);
        float r  = f.Style.PointRadius;
        using var brush = new SolidBrush(f.Style.FillColor);
        using var pen   = new Pen(f.Style.StrokeColor, f.Style.StrokeWidth);
        g.FillEllipse(brush, pt.X - r, pt.Y - r, r * 2, r * 2);
        g.DrawEllipse(pen,   pt.X - r, pt.Y - r, r * 2, r * 2);
    }

    private static void DrawLine(Graphics g, IFeature f, Viewport vp)
    {
        if (f.Points.Count < 2) return;
        var pts = f.Points.Select(p => vp.WorldToScreen(p)).ToArray();
        using var pen = new Pen(f.Style.StrokeColor, f.Style.StrokeWidth)
        {
            LineJoin  = System.Drawing.Drawing2D.LineJoin.Round,
            StartCap  = System.Drawing.Drawing2D.LineCap.Round,
            EndCap    = System.Drawing.Drawing2D.LineCap.Round
        };
        g.DrawLines(pen, pts);
    }

    private static void DrawPolygon(Graphics g, IFeature f, Viewport vp)
    {
        if (f.Points.Count < 3) return;
        var pts = f.Points.Select(p => vp.WorldToScreen(p)).ToArray();
        using var brush = new SolidBrush(f.Style.FillColor);
        using var pen   = new Pen(f.Style.StrokeColor, f.Style.StrokeWidth);
        g.FillPolygon(brush, pts);
        g.DrawPolygon(pen,   pts);
    }

    private static void DrawRing(Graphics g, IReadOnlyList<PointD> ring,
                                  FeatureStyle style, Viewport vp)
    {
        var pts = ring.Select(p => vp.WorldToScreen(p)).ToArray();
        using var brush = new SolidBrush(style.FillColor);
        using var pen   = new Pen(style.StrokeColor, style.StrokeWidth);
        g.FillPolygon(brush, pts);
        g.DrawPolygon(pen, pts);
    }
}

// ── Feature model ────────────────────────────────────────────────────────────
public enum GeometryType { Point, LineString, Polygon, MultiPolygon }

public interface IFeature
{
    GeometryType              GeometryType { get; }
    RectangleD                Envelope     { get; }
    IReadOnlyList<PointD>     Points       { get; }
    IReadOnlyList<IReadOnlyList<PointD>> Rings { get; }
    FeatureStyle              Style        { get; }
    string?                   Label        { get; }
    object?                   Tag          { get; }
}

public sealed class FeatureStyle
{
    public Color FillColor   { get; set; } = Color.FromArgb(80, 0, 120, 200);
    public Color StrokeColor { get; set; } = Color.FromArgb(0, 80, 180);
    public float StrokeWidth { get; set; } = 1.5f;
    public float PointRadius { get; set; } = 5f;
    public Font? LabelFont   { get; set; }
    public Color LabelColor  { get; set; } = Color.White;
}
```

---

## 20. R-Tree Spatial Index

For layers with more than ~5,000 features, a spatial index is essential. This is a simplified grid-based index; for production use `NetTopologySuite.Index.Strtree.STRtree<T>`.

```csharp
// Using NetTopologySuite STRtree (NuGet: NetTopologySuite)
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Geometries;

public sealed class NtsRTreeIndex : ISpatialIndex
{
    private readonly STRtree<IFeature> _tree = new();
    private bool _built = false;

    public void Insert(IFeature f)
    {
        var env = new Envelope(
            f.Envelope.Left, f.Envelope.Right,
            f.Envelope.Bottom, f.Envelope.Top);
        _tree.Insert(env, f);
    }

    public IEnumerable<IFeature> Query(RectangleD extent)
    {
        if (!_built) { _tree.Build(); _built = true; }
        var env = new Envelope(extent.Left, extent.Right, extent.Bottom, extent.Top);
        return _tree.Query(env).Cast<IFeature>();
    }
}
```

---

## 21. Label & Annotation Rendering

Labels require collision detection — drawing every label causes unreadable overlap. A simple approach is a list of already-placed screen rectangles; a new label is only drawn if its bounding box does not overlap any previously placed label.

```csharp
// Rendering/LabelRenderer.cs
public sealed class LabelRenderer
{
    private readonly List<RectangleF> _placed = new();
    private readonly Font  _font;
    private readonly Brush _fill;
    private readonly Pen   _halo;

    public LabelRenderer(Font font, Color textColor, Color haloColor)
    {
        _font = font;
        _fill = new SolidBrush(textColor);
        _halo = new Pen(haloColor, 3f) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round };
    }

    public void Reset() => _placed.Clear();

    public bool TryDrawLabel(Graphics g, string text, PointF screenPt,
                              ContentAlignment anchor = ContentAlignment.MiddleCenter)
    {
        SizeF sz      = g.MeasureString(text, _font);
        RectangleF r  = AnchoredRect(screenPt, sz, anchor);
        r.Inflate(2, 2); // padding for collision

        // Collision check
        if (_placed.Any(p => p.IntersectsWith(r))) return false;
        _placed.Add(r);

        // Draw halo then text for readability on any background
        var gp = new System.Drawing.Drawing2D.GraphicsPath();
        gp.AddString(text, _font.FontFamily, (int)_font.Style, g.DpiY * _font.Size / 72,
                     r.Location, StringFormat.GenericDefault);
        g.DrawPath(_halo, gp);
        g.FillPath(_fill, gp);
        return true;
    }

    private static RectangleF AnchoredRect(PointF pt, SizeF sz, ContentAlignment anchor)
    {
        float ox = anchor switch {
            ContentAlignment.MiddleCenter or ContentAlignment.TopCenter
                or ContentAlignment.BottomCenter => -sz.Width / 2,
            ContentAlignment.MiddleRight or ContentAlignment.TopRight
                or ContentAlignment.BottomRight  => -sz.Width,
            _ => 0 };
        float oy = anchor switch {
            ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter
                or ContentAlignment.MiddleRight => -sz.Height / 2,
            ContentAlignment.BottomLeft or ContentAlignment.BottomCenter
                or ContentAlignment.BottomRight  => -sz.Height,
            _ => 0 };
        return new RectangleF(pt.X + ox, pt.Y + oy, sz.Width, sz.Height);
    }
}
```

---

## 22. Overlay Objects (Markers, Shapes, Rulers)

Overlay objects are drawn above all raster and vector layers. They are not stored in the tile system — they are re-drawn on every composite.

```csharp
// Layers/OverlayLayer.cs
public sealed class OverlayLayer : IMapLayer
{
    public string Name      { get; set; } = "Overlay";
    public bool   IsVisible { get; set; } = true;

    private readonly List<IOverlayObject> _objects = new();
    public IReadOnlyList<IOverlayObject>  Objects  => _objects;

    public void Add(IOverlayObject obj) => _objects.Add(obj);
    public void Remove(IOverlayObject obj) => _objects.Remove(obj);
    public void Clear() => _objects.Clear();

    public void Render(Graphics g, Viewport vp, TileCache cache, CancellationToken token)
    {
        foreach (var obj in _objects)
        {
            if (token.IsCancellationRequested) return;
            obj.Render(g, vp);
        }
    }
}

public interface IOverlayObject { void Render(Graphics g, Viewport vp); }

// ── Pin marker ───────────────────────────────────────────────────────────────
public sealed class PinMarker : IOverlayObject
{
    public PointD  Position { get; set; }
    public string? Label    { get; set; }
    public Color   Color    { get; set; } = Color.OrangeRed;
    public int     Size     { get; set; } = 24;

    public void Render(Graphics g, Viewport vp)
    {
        var pt = vp.WorldToScreen(Position);
        int r  = Size / 2;
        // Drop shadow
        using var shadow = new SolidBrush(Color.FromArgb(60, 0, 0, 0));
        g.FillEllipse(shadow, pt.X - r + 2, pt.Y - r + 2, Size, Size);
        // Pin circle
        using var fill = new SolidBrush(Color);
        using var pen  = new Pen(Color.White, 2f);
        g.FillEllipse(fill, pt.X - r, pt.Y - r, Size, Size);
        g.DrawEllipse(pen, pt.X - r, pt.Y - r, Size, Size);
        // Centre dot
        using var dot = new SolidBrush(Color.White);
        g.FillEllipse(dot, pt.X - 3, pt.Y - 3, 6, 6);
    }
}

// ── Measurement ruler ─────────────────────────────────────────────────────────
public sealed class RulerOverlay : IOverlayObject
{
    public List<PointD> Points { get; } = new();

    public double TotalDistanceMetres()
    {
        if (Points.Count < 2) return 0;
        double total = 0;
        for (int i = 1; i < Points.Count; i++)
            total += HaversineMetres(Points[i - 1], Points[i]);
        return total;
    }

    public void Render(Graphics g, Viewport vp)
    {
        if (Points.Count < 2) return;
        var screenPts = Points.Select(p => vp.WorldToScreen(p)).ToArray();
        using var pen  = new Pen(Color.Yellow, 2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
        using var font = new Font("Segoe UI", 9f);
        using var bg   = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
        using var fg   = new SolidBrush(Color.Yellow);
        g.DrawLines(pen, screenPts);

        double total = TotalDistanceMetres();
        string label = total > 1000
            ? $"{total / 1000.0:F2} km"
            : $"{total:F0} m";
        var mid = screenPts[screenPts.Length / 2];
        var sz  = g.MeasureString(label, font);
        g.FillRectangle(bg, mid.X + 4, mid.Y - sz.Height / 2, sz.Width + 4, sz.Height);
        g.DrawString(label, font, fg, mid.X + 6, mid.Y - sz.Height / 2);
    }

    private static double HaversineMetres(PointD a, PointD b)
    {
        const double R = 6371000;
        double dLat = (b.Y - a.Y) * Math.PI / 180.0;
        double dLon = (b.X - a.X) * Math.PI / 180.0;
        double h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(a.Y * Math.PI / 180.0)
                 * Math.Cos(b.Y * Math.PI / 180.0)
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
    }
}
```

---

## 23. Threading Model & Safety

The threading rules are strict and must be followed consistently to avoid deadlocks, corrupted bitmaps, and cross-thread UI exceptions.

```
Thread          Allowed operations
──────────────  ─────────────────────────────────────────────────────────────
UI thread       OnPaint, Invalidate, UpdateStyles, all Control property writes
                Reading _frontBuffer inside a lock
                Starting/stopping Timer
Render thread   New Bitmap, Graphics.DrawImage, layer.Render()
                Writing _frontBuffer inside a lock
                _compositor.SetBuffer()
I/O thread      File.ReadAllBytes, SQLite queries, HTTP GetByteArrayAsync
                Cache.Put() (lock-protected)
Any thread      Cache.Get() (lock-protected)
                TileId computations (pure math, no state)
```

### Cross-Thread Invalidate Pattern

```csharp
// CORRECT: post to UI thread
_host.BeginInvoke(() => _host.Invalidate());

// WRONG: calling Invalidate() from a background thread crashes
_host.Invalidate(); // throws InvalidOperationException
```

### Bitmap Cross-Thread Safety

`Bitmap` is **not thread-safe**. Never read and write the same `Bitmap` from different threads simultaneously.

```csharp
// CORRECT: swap under lock; old bitmap disposed after lock is released
Bitmap? old;
lock (_bufLock)
{
    old          = _frontBuffer;
    _frontBuffer = newBitmap;
}
old?.Dispose(); // dispose outside lock

// WRONG: reading front buffer without lock while render thread may be writing
g.DrawImage(_frontBuffer, 0, 0); // race condition
```

### Cancellation Token Propagation

Every long-running operation accepts a `CancellationToken`. When the viewport changes, the previous token is cancelled before a new render is started.

```csharp
// Pattern used throughout the codebase
_cts.Cancel();
_cts = new CancellationTokenSource();
var token = _cts.Token;
Task.Run(() => DoWork(token), token);
```

---

## 24. Memory Management & Bitmap Disposal

GDI+ `Bitmap` objects wrap native GDI handles. Failing to dispose them causes handle leaks and `OutOfMemoryException` (which in GDI+ often means "out of GDI handles" not RAM). Every `Bitmap` must have a clear owner and a clear disposal path.

```csharp
// Ownership rules:
// 1. TileCache owns all cached Bitmaps. Consumers must not Dispose them.
// 2. LayerCompositor owns front/back buffers. Dispose old buffer after swap.
// 3. PanCache produces new Bitmaps — caller owns and must swap into compositor.
// 4. ZoomPreview is owned by MapCanvas and disposed before next preview is built.

// ── Safe bitmap clone before storing ────────────────────────────────────────
// When decoding from MemoryStream, Image.FromStream keeps the stream open.
// Always clone to detach:
using var ms = new MemoryStream(rawBytes);
var bmp = (Bitmap)Image.FromStream(ms).Clone(); // Clone creates independent copy
// ms can now safely dispose

// ── Pixel format conversion (for DrawImage performance) ──────────────────────
public static Bitmap ConvertToPArgb(Bitmap src)
{
    var dst = new Bitmap(src.Width, src.Height,
                         System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
    using var g = Graphics.FromImage(dst);
    g.DrawImage(src, 0, 0, src.Width, src.Height);
    return dst;
}

// ── Fast pixel copy using LockBits (unsafe, ~10x faster than GetPixel) ───────
public static unsafe void CopyRegionFast(Bitmap src, Bitmap dst, Point offset)
{
    var srcData = src.LockBits(
        new Rectangle(0, 0, src.Width, src.Height),
        System.Drawing.Imaging.ImageLockMode.ReadOnly,
        System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
    var dstData = dst.LockBits(
        new Rectangle(offset.X, offset.Y,
                      Math.Min(src.Width,  dst.Width  - offset.X),
                      Math.Min(src.Height, dst.Height - offset.Y)),
        System.Drawing.Imaging.ImageLockMode.WriteOnly,
        System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
    try
    {
        int copyW = Math.Min(src.Width,  dst.Width  - offset.X);
        int copyH = Math.Min(src.Height, dst.Height - offset.Y);
        for (int row = 0; row < copyH; row++)
        {
            Buffer.MemoryCopy(
                (void*)(srcData.Scan0 + row * srcData.Stride),
                (void*)(dstData.Scan0 + row * dstData.Stride),
                copyW * 4, copyW * 4);
        }
    }
    finally
    {
        src.UnlockBits(srcData);
        dst.UnlockBits(dstData);
    }
}
```

---

## 25. Layer Manager & Draw Order

```csharp
// Layers/LayerManager.cs
public sealed class LayerManager
{
    private readonly List<IMapLayer> _layers = new();
    private readonly object _lock = new();

    public IReadOnlyList<IMapLayer>       All           { get { lock (_lock) return _layers.ToList(); } }
    public IEnumerable<IMapLayer>         VisibleLayers => All.Where(l => l.IsVisible);

    public void Add(IMapLayer layer)      { lock (_lock) _layers.Add(layer); }
    public void Remove(IMapLayer layer)   { lock (_lock) _layers.Remove(layer); }
    public void MoveUp(IMapLayer layer)   { lock (_lock) Swap(layer, 1);  }
    public void MoveDown(IMapLayer layer) { lock (_lock) Swap(layer, -1); }

    private void Swap(IMapLayer layer, int delta)
    {
        int i = _layers.IndexOf(layer);
        int j = i + delta;
        if (i < 0 || j < 0 || j >= _layers.Count) return;
        (_layers[i], _layers[j]) = (_layers[j], _layers[i]);
    }

    // Recommended draw order (bottom to top)
    // [0] Base raster (satellite / OSM)
    // [1] Overlay rasters (hillshade, NDVI, etc.)
    // [2] Vector polygons (parcels, zones)
    // [3] Vector lines (roads, contours)
    // [4] Vector points (POIs, GPS tracks)
    // [5] Labels / annotations
    // [6] Overlay objects (pins, rulers, selection highlights)
}
```

---

## 26. Scale Bar & Map Decorations

Scale bar, north arrow, and attribution are drawn synchronously on every `OnPaint` — they are cheap and do not require background threads.

```csharp
// Rendering/ScaleBar.cs
public static class ScaleBar
{
    public static void Draw(Graphics g, Viewport vp, Rectangle screenArea, float dpi = 96)
    {
        // Choose a round-number ground distance for the bar
        double resM    = vp.GroundResolutionAtLatitude(vp.Center.Y);
        double targetW = screenArea.Width * 0.25; // 25% of canvas width
        double rawM    = targetW * resM;

        double barMetres = RoundToNice(rawM);
        float  barPx     = (float)(barMetres / resM);

        int x  = screenArea.Left + 12;
        int y  = screenArea.Bottom - 28;
        int h  = 6;

        // Background pill
        using var bgBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
        g.FillRectangle(bgBrush, x - 4, y - 14, barPx + 8, 24);

        // Bar segments (alternating black/white)
        using var black = new SolidBrush(Color.White);
        using var white = new SolidBrush(Color.FromArgb(180, 180, 180));
        g.FillRectangle(black, x,             y, barPx / 2, h);
        g.FillRectangle(white, x + barPx / 2, y, barPx / 2, h);
        using var pen = new Pen(Color.White, 1f);
        g.DrawRectangle(pen, x, y, barPx, h);

        // Label
        string label = barMetres >= 1000
            ? $"{barMetres / 1000.0:G3} km"
            : $"{barMetres:G3} m";
        using var font = new Font("Segoe UI", 8f);
        using var fg   = new SolidBrush(Color.White);
        g.DrawString(label, font, fg, x + barPx / 2, y - 13,
                     new StringFormat { Alignment = StringAlignment.Center });
    }

    private static double RoundToNice(double metres)
    {
        double[] niceValues = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000,
                                 5000, 10000, 20000, 50000, 100000, 500000, 1000000 };
        return niceValues.OrderBy(v => Math.Abs(v - metres)).First();
    }
}
```

---

## 27. Hit Testing & Mouse Interaction

```csharp
// Interaction/HitTester.cs
public sealed class HitTester
{
    private readonly LayerManager _layers;

    public HitTester(LayerManager layers) => _layers = layers;

    /// <summary>Returns all features whose envelope contains the screen point.</summary>
    public IEnumerable<IFeature> HitTest(PointF screenPt, Viewport vp, float tolerancePx = 6)
    {
        var worldPt = vp.ScreenToWorld(screenPt);
        double tolW = tolerancePx * vp.Resolution;

        var testRect = new RectangleD(
            worldPt.X - tolW, worldPt.Y - tolW, tolW * 2, tolW * 2);

        return _layers.VisibleLayers
            .OfType<VectorLayer>()
            .SelectMany(layer => layer.FeaturesInExtent(testRect))
            .Where(f => ContainsPoint(f, worldPt, tolW));
    }

    private static bool ContainsPoint(IFeature f, PointD pt, double tol)
    {
        return f.GeometryType switch
        {
            GeometryType.Point      => Distance(f.Points[0], pt) <= tol,
            GeometryType.LineString => PointNearPolyline(pt, f.Points, tol),
            GeometryType.Polygon    => PointInPolygon(pt, f.Points),
            _ => false
        };
    }

    private static double Distance(PointD a, PointD b) =>
        Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    private static bool PointNearPolyline(PointD pt, IReadOnlyList<PointD> pts, double tol)
    {
        for (int i = 1; i < pts.Count; i++)
            if (PointToSegmentDistance(pt, pts[i - 1], pts[i]) <= tol) return true;
        return false;
    }

    private static double PointToSegmentDistance(PointD p, PointD a, PointD b)
    {
        double dx = b.X - a.X, dy = b.Y - a.Y;
        double lenSq = dx * dx + dy * dy;
        if (lenSq == 0) return Distance(p, a);
        double t = Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq, 0, 1);
        return Distance(p, new PointD(a.X + t * dx, a.Y + t * dy));
    }

    // Winding number algorithm — handles concave and self-intersecting polygons
    private static bool PointInPolygon(PointD p, IReadOnlyList<PointD> poly)
    {
        int winding = 0;
        int n = poly.Count;
        for (int i = 0; i < n; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % n];
            if (a.Y <= p.Y)
            {
                if (b.Y > p.Y && Cross(a, b, p) > 0) winding++;
            }
            else
            {
                if (b.Y <= p.Y && Cross(a, b, p) < 0) winding--;
            }
        }
        return winding != 0;
    }

    private static double Cross(PointD a, PointD b, PointD p) =>
        (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
}
```

---

## 28. Coordinate Systems & Projections

```csharp
// Core/CoordinateSystems.cs
public static class CoordinateSystems
{
    // ── WGS-84 ↔ Web Mercator (EPSG 4326 ↔ 3857) ─────────────────────────
    public static PointD Wgs84ToMercator(double lonDeg, double latDeg)
    {
        const double R = 6378137.0;
        double x = lonDeg * Math.PI / 180.0 * R;
        double y = Math.Log(Math.Tan(Math.PI / 4.0 + latDeg * Math.PI / 360.0)) * R;
        return new PointD(x, y);
    }

    public static (double lon, double lat) MercatorToWgs84(double x, double y)
    {
        const double R = 6378137.0;
        double lon = x / R * 180.0 / Math.PI;
        double lat = (2 * Math.Atan(Math.Exp(y / R)) - Math.PI / 2) * 180.0 / Math.PI;
        return (lon, lat);
    }

    // ── DMS ↔ Decimal degrees ─────────────────────────────────────────────
    public static double DmsToDeg(int degrees, int minutes, double seconds, bool negative)
    {
        double d = degrees + minutes / 60.0 + seconds / 3600.0;
        return negative ? -d : d;
    }

    public static (int d, int m, double s) DegToDms(double degrees)
    {
        int d   = (int)degrees;
        double rem = (degrees - d) * 60.0;
        int m   = (int)rem;
        double s = (rem - m) * 60.0;
        return (d, m, s);
    }

    // ── Haversine distance ────────────────────────────────────────────────
    public static double DistanceMetres(PointD wgs84A, PointD wgs84B)
    {
        const double R = 6371000;
        double dLat = (wgs84B.Y - wgs84A.Y) * Math.PI / 180.0;
        double dLon = (wgs84B.X - wgs84A.X) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(wgs84A.Y * Math.PI / 180.0)
                 * Math.Cos(wgs84B.Y * Math.PI / 180.0)
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    // ── Bearing ────────────────────────────────────────────────────────────
    public static double BearingDegrees(PointD from, PointD to)
    {
        double dLon = (to.X - from.X) * Math.PI / 180.0;
        double lat1 = from.Y * Math.PI / 180.0;
        double lat2 = to.Y   * Math.PI / 180.0;
        double y = Math.Sin(dLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2)
                 - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
        return (Math.Atan2(y, x) * 180.0 / Math.PI + 360) % 360;
    }
}
```

---

## 29. Performance Benchmarks & Tuning

### Impact Table

| Optimisation | Typical gain | Effort |
|---|---|---|
| `PixelFormat.Format32bppPArgb` | 25–35% faster DrawImage | Low — change 1 line per Bitmap |
| Deferred off-screen compositor | Eliminates UI-thread render work | Medium |
| Zoom timer 200 ms debounce | Prevents repeated tile floods | Low |
| L1 LRU memory cache | Eliminates I/O on revisit | Medium |
| Pan bitmap translate | Near-zero cost pan latency | Medium |
| Prioritised tile loading | Visible area renders first | Low |
| Coarse tile zoom during animation | 4× fewer tile requests | Low |
| Parent tile placeholder | No blank gaps on zoom | Medium |
| Cancellation tokens | Aborts stale renders | Low |
| `SemaphoreSlim(4)` I/O limit | Prevents HTTP/disk flooding | Low |
| `CompositingMode.SourceCopy` in OnPaint | Skips alpha blend during DrawImage | Low |
| NTS STRtree spatial index | O(log n) instead of O(n) feature scan | Medium |
| Batch SQLite tile fetch | 1 query vs N queries for N tiles | Medium |

### Profiling Checklist

1. Is `OnPaint` doing any work other than `DrawImage`? If yes, move it to the compositor.
2. Are bitmaps being allocated in `Format32bppArgb` instead of `PArgb`? Check every `new Bitmap(...)`.
3. Is `Invalidate()` being called from a background thread without `BeginInvoke`?
4. Are disposable GDI objects (`Pen`, `Brush`, `Font`) being created inside render loops? Cache them as fields.
5. Are vector features being re-projected on every frame? Cache screen-space point arrays and invalidate only on viewport change.

### Recommended GDI Object Caching

```csharp
// Render-loop GDI object cache — create once, reuse, dispose on shutdown
public sealed class RenderResources : IDisposable
{
    public Pen   RoadPen      { get; } = new(Color.White, 1.5f);
    public Pen   BuildingPen  { get; } = new(Color.FromArgb(180, 220, 220, 220), 1f);
    public Brush WaterBrush   { get; } = new SolidBrush(Color.FromArgb(180, 100, 160, 210));
    public Font  LabelFont    { get; } = new("Segoe UI", 9f);

    public void Dispose()
    {
        RoadPen.Dispose(); BuildingPen.Dispose();
        WaterBrush.Dispose(); LabelFont.Dispose();
    }
}
```

---

## 30. Complete Wiring — MapCanvas Integration

This section shows how all components connect inside `MapCanvas`.

```csharp
// MapCanvas.cs — complete class (abbreviated for clarity)
public sealed class MapCanvas : Panel, IDisposable
{
    // ── Component fields ─────────────────────────────────────────────────
    public  Viewport          Viewport    { get; private set; }
    public  LayerManager      Layers      { get; } = new();
    public  TileCache         Cache       { get; }
    public  HitTester         HitTester   { get; }

    private readonly LayerCompositor _compositor;
    private readonly TileLoader      _loader;
    private readonly PanCache        _panCache = new();
    private readonly RenderResources _res      = new();

    private readonly System.Windows.Forms.Timer _zoomTimer;
    private readonly System.Windows.Forms.Timer _panDirtyTimer;

    private bool   _isPanning, _isZooming;
    private PointF _panLastScreen;
    private double _pendingZoom;
    private PointF _zoomOriginScreen;
    private Bitmap? _zoomPreview;

    public event EventHandler<IFeature>? FeatureClicked;
    public event EventHandler?           ViewportChanged;

    // ── Construction ─────────────────────────────────────────────────────
    public MapCanvas(string diskCachePath = "tile_cache", int l1Size = 300)
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint  |
                 ControlStyles.UserPaint             |
                 ControlStyles.ResizeRedraw, true);
        UpdateStyles();
        BackColor = Color.FromArgb(24, 24, 28);

        Cache       = new TileCache(diskCachePath, l1Size);
        Viewport    = new Viewport { ScreenSize = new Size(800, 600) };
        _compositor = new LayerCompositor(this, Layers, Cache);
        _loader     = new TileLoader(Cache);
        HitTester   = new HitTester(Layers);

        _loader.TileReady += () => BeginInvoke(Invalidate);

        _zoomTimer = new System.Windows.Forms.Timer { Interval = 200 };
        _zoomTimer.Tick += OnZoomSettled;

        _panDirtyTimer = new System.Windows.Forms.Timer { Interval = 50 };
        _panDirtyTimer.Tick += (_, _) => { _panDirtyTimer.Stop(); RequestFullRender(); };
    }

    // ── Add a raster source ───────────────────────────────────────────────
    public RasterLayer AddRasterSource(IRasterTileSource source)
    {
        var layer = new RasterLayer(source);
        Layers.Add(layer);
        _loader.AddSource(source);
        return layer;
    }

    // ── Add a vector layer ────────────────────────────────────────────────
    public VectorLayer AddVectorLayer(string name = "Vector")
    {
        var layer = new VectorLayer { Name = name };
        Layers.Add(layer);
        return layer;
    }

    // ── Navigate ──────────────────────────────────────────────────────────
    public void NavigateTo(double lon, double lat, double zoom)
    {
        Viewport = Viewport
            .WithCenter(new PointD(lon, lat))
            .WithZoom(zoom);
        RequestFullRender();
    }

    // ── Render pipeline entry point ───────────────────────────────────────
    public void RequestFullRender()
    {
        _loader.LoadTilesForViewport(Viewport);
        _compositor.RequestRender(Viewport);
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    // ── Paint ─────────────────────────────────────────────────────────────
    protected override void OnPaint(PaintEventArgs e)
    {
        if (_isZooming && _zoomPreview != null)
        {
            e.Graphics.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(_zoomPreview, 0, 0);
        }
        else
        {
            e.Graphics.CompositingMode =
                System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            _compositor.DrawTo(e.Graphics);
        }
        // Decorations always on top
        ScaleBar.Draw(e.Graphics, Viewport, ClientRectangle);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width < 1 || Height < 1) return;
        Viewport = Viewport.WithScreenSize(ClientSize);
        RequestFullRender();
    }

    // ── Mouse input ───────────────────────────────────────────────────────
    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        { _isPanning = true; _panLastScreen = e.Location; Cursor = Cursors.SizeAll; }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isPanning) return;
        int dx = (int)(e.X - _panLastScreen.X);
        int dy = (int)(e.Y - _panLastScreen.Y);
        _panLastScreen = e.Location;
        var translated = _panCache.Translate(_compositor.CurrentBuffer!, dx, dy, ClientSize);
        _compositor.SetBuffer(translated);
        Viewport = Viewport.WithCenter(new PointD(
            Viewport.Center.X - dx * Viewport.Resolution,
            Viewport.Center.Y + dy * Viewport.Resolution));
        Invalidate();
        _panDirtyTimer.Stop();
        _panDirtyTimer.Start();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _isPanning)
        {
            _isPanning = false;
            Cursor     = Cursors.Cross;
            RequestFullRender();
            // Hit test on click (small movement threshold)
            if (Math.Abs(e.X - _panLastScreen.X) < 4 &&
                Math.Abs(e.Y - _panLastScreen.Y) < 4)
            {
                var hit = HitTester.HitTest(e.Location, Viewport).FirstOrDefault();
                if (hit != null) FeatureClicked?.Invoke(this, hit);
            }
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        double delta     = e.Delta > 0 ? 0.75 : -0.75;
        _pendingZoom     = Math.Clamp(Viewport.ZoomLevel + delta, 0, 22);
        _zoomOriginScreen = e.Location;
        _isZooming       = true;
        _zoomPreview?.Dispose();
        _zoomPreview = BuildZoomPreview(_pendingZoom, e.Location);
        Invalidate();
        _zoomTimer.Stop();
        _zoomTimer.Start();
    }

    private void OnZoomSettled(object? s, EventArgs e)
    {
        _zoomTimer.Stop();
        _isZooming = false;
        var worldAtCursor = Viewport.ScreenToWorld(_zoomOriginScreen);
        Viewport = Viewport.WithZoom(_pendingZoom);
        var newScreenPt = Viewport.WorldToScreen(worldAtCursor);
        double offX = (_zoomOriginScreen.X - newScreenPt.X) * Viewport.Resolution;
        double offY = (_zoomOriginScreen.Y - newScreenPt.Y) * -Viewport.Resolution;
        Viewport = Viewport.WithCenter(new PointD(
            Viewport.Center.X + offX, Viewport.Center.Y + offY));
        RequestFullRender();
    }

    private Bitmap BuildZoomPreview(double targetZoom, PointF origin)
    {
        var src    = _compositor.CurrentBuffer;
        var result = new Bitmap(Width, Height,
                                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        double scale = Math.Pow(2, targetZoom - Viewport.ZoomLevel);
        using var g  = Graphics.FromImage(result);
        g.Clear(BackColor);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.TranslateTransform(origin.X, origin.Y);
        g.ScaleTransform((float)scale, (float)scale);
        g.TranslateTransform(-origin.X, -origin.Y);
        if (src != null) g.DrawImage(src, 0, 0);
        return result;
    }

    // ── Cleanup ───────────────────────────────────────────────────────────
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _compositor.Dispose();
            Cache.Dispose();
            _zoomTimer.Dispose();
            _panDirtyTimer.Dispose();
            _zoomPreview?.Dispose();
            _res.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

### Typical Usage (Form1.cs)

```csharp
public partial class Form1 : Form
{
    private MapCanvas _map;

    public Form1()
    {
        InitializeComponent();

        _map = new MapCanvas("C:/tile_cache", l1Size: 400)
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(_map);

        // Add an MBTiles base layer
        var mbSource = new MbTilesSource("C:/data/world.mbtiles");
        _map.AddRasterSource(mbSource);

        // Add an HTTP tile layer on top
        var osmSource = new HttpTileSource("https://tile.openstreetmap.org/{z}/{x}/{y}.png", "OSM");
        _map.AddRasterSource(osmSource);

        // Add a vector layer
        var roads = _map.AddVectorLayer("Roads");
        roads.AddFeature(/* load from GeoJSON, Shapefile, database, etc. */);
        roads.BuildIndex();

        // React to feature clicks
        _map.FeatureClicked += (_, feature) =>
            MessageBox.Show($"Clicked: {feature.Label}");

        // Start at London
        _map.NavigateTo(-0.1276, 51.5074, zoom: 12);
    }
}
```

---

## Interface Contracts

```csharp
// Layers/IMapLayer.cs
public interface IMapLayer
{
    string Name      { get; set; }
    bool   IsVisible { get; set; }
    void   Render(Graphics g, Viewport vp, TileCache cache, CancellationToken token);
}

// Sources/IRasterTileSource.cs
public interface IRasterTileSource
{
    string  Name    { get; }
    Bitmap? GetTile(TileId id);
}

// Index interface
public interface ISpatialIndex
{
    IEnumerable<IFeature> Query(RectangleD extent);
}
```

---

*End of GIS Map Canvas Implementation Guide*
*All code targets .NET 8 / C# 12. WinForms, System.Data.SQLite, BitMiracle.LibTiff.NET, NetTopologySuite.*
