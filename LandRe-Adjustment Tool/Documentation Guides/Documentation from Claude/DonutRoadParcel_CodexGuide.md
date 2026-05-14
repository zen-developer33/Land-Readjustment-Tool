# Donut-Shaped Road Parcel — Full Concept & Codex Implementation Guide

> **Purpose of this document:**
> This is a self-contained specification and prompt intended for an AI coding agent
> (Codex / Claude Cowork). Read every section before writing any code. Make
> architectural decisions that are consistent with the concepts below. Fill gaps
> with best-practice C# .NET patterns — do not ask the user for every detail.

---

## 1. PROJECT CONTEXT

This is a **C# .NET Windows Desktop application** for **cadastral / land parcel
management**. The application manages land parcels, road parcels, and their
spatial relationships. It uses:

- **NetTopologySuite (NTS)** — all geometry operations
- **Entity Framework Core** with SQL Server — persistence
- **WinForms + GDI+** — map rendering and UI
- **Target framework:** .NET 8 Windows

The feature being implemented is **Donut-Shaped Road Parcel support** — the
ability to correctly identify, import, store, validate, draw, and query road
parcels that have one or more interior holes (islands, medians, roundabout
centres).

---

## 2. CORE CONCEPT — WHAT IS A DONUT ROAD PARCEL?

### 2.1 The Geometric Definition

A **donut parcel** is a `Polygon` that has:
- One **exterior ring** — the outer boundary of the road
- One or more **interior rings** — holes punched through the road polygon

The hole represents a piece of land **excluded from the road** — typically a
traffic island, roundabout garden, median strip, or a separately titled parcel
sitting inside the road boundary.

```
Normal parcel:             Donut parcel (road with island):
┌────────────┐             ┌──────────────────┐
│            │             │  road surface     │
│  land area │             │   ┌──────────┐   │
│            │             │   │  island  │   │
└────────────┘             │   │  (hole)  │   │
                           │   └──────────┘   │
                           └──────────────────┘
```

### 2.2 Ring Winding Direction (Critical)

NTS and SQL Server both require:
- **Exterior ring → Counter-Clockwise (CCW)**
- **Interior rings (holes) → Clockwise (CW)**

If winding is wrong the polygon will be geometrically invalid. Always check and
fix winding during import.

### 2.3 WKT Representation

```
POLYGON(
  (x1 y1, x2 y2, x3 y3, x4 y4, x1 y1),        ← exterior ring (CCW)
  (hx1 hy1, hx2 hy2, hx3 hy3, hx1 hy1)         ← interior ring/hole (CW)
)
```

Multiple holes:
```
POLYGON(
  (outer ring coords ...),
  (hole1 coords ...),
  (hole2 coords ...)
)
```

### 2.4 Real-World Road Scenarios

| Scenario           | Holes | Hole Shape         | What the Hole Represents         |
|--------------------|-------|--------------------|----------------------------------|
| Roundabout         | 1     | ~circular          | Garden / traffic island          |
| Median road        | 1     | long rectangle     | Median strip (separate parcel)   |
| Cul-de-sac         | 0–1   | oval               | Central planting island          |
| Highway interchange| 2+    | irregular polygons | Multiple green islands           |
| T-junction         | 0–1   | triangle           | Junction directional island      |
| Straight road      | 0     | N/A                | No island — not a donut          |

### 2.5 The Island Parcel Relationship

The land inside the hole is **not part of the road parcel**. It is either:
- A separately registered parcel (linked by parcel number)
- An unregistered open space
- A future parcel

The application must store which hole corresponds to which inner parcel so that
topology can be validated.

---

## 3. DATA MODEL

### 3.1 Enumerations

```csharp
public enum RoadParcelType
{
    StraightRoad    = 0,
    Roundabout      = 1,   // typically 1 circular hole
    MedianRoad      = 2,   // 1 elongated hole
    CulDeSac        = 3,   // 0 or 1 oval hole
    Junction        = 4,   // 1+ irregular holes
    Highway         = 5,   // may have 2+ holes
    Unknown         = 99
}

public enum ImportSource
{
    WKT,
    WKB,
    GeoJSON,
    Shapefile,
    ManualEntry
}

public enum DonutValidationStatus
{
    NotChecked,
    Valid,
    InvalidGeometry,
    WrongWindingDirection,
    HoleOutsideExterior,
    HolesOverlap,
    HoleAreaExceedsParcel
}
```

### 3.2 Core Entities

```csharp
/// <summary>
/// Represents any land parcel in the cadastral system.
/// Road parcels that are donuts are a specialisation of this.
/// </summary>
public class Parcel
{
    public int    Id            { get; set; }
    public string ParcelNumber  { get; set; } = "";
    public string ParcelType    { get; set; } = "GENERAL"; // "ROAD", "ISLAND", "GENERAL"
    public Geometry Shape       { get; set; } = null!;

    // Computed
    public bool IsDonut          => (Shape as Polygon)?.NumInteriorRings > 0;
    public int  InteriorRingCount => (Shape as Polygon)?.NumInteriorRings ?? 0;
}

/// <summary>
/// Road-specific parcel. Extends base Parcel with road metadata.
/// The Shape polygon MAY have interior rings (donut) when the road
/// contains traffic islands or medians.
/// </summary>
public class RoadParcel
{
    public int           Id               { get; set; }
    public string        RoadParcelNumber { get; set; } = "";
    public string        RoadName         { get; set; } = "";
    public RoadParcelType RoadType        { get; set; }
    public Polygon       Shape            { get; set; } = null!;
    public ImportSource  ImportedFrom     { get; set; }
    public DateTime      ImportedAt       { get; set; }
    public DonutValidationStatus ValidationStatus { get; set; }
    public string?       ValidationMessage { get; set; }

    // Navigation
    public List<RoadIsland> Islands { get; set; } = new();

    // Computed helpers (not mapped to DB)
    [NotMapped] public bool IsDonut      => Shape?.NumInteriorRings > 0;
    [NotMapped] public int  IslandCount  => Shape?.NumInteriorRings ?? 0;
    [NotMapped] public double RoadArea   => Shape?.Area ?? 0;
}

/// <summary>
/// Represents one hole (interior ring) in a donut road parcel.
/// May link to a registered island parcel.
/// </summary>
public class RoadIsland
{
    public int     Id                { get; set; }
    public int     RoadParcelId      { get; set; }
    public int     HoleIndex         { get; set; }  // 0-based index into interior rings
    public string? LinkedParcelNumber { get; set; } // null if unregistered
    public Polygon IslandShape       { get; set; } = null!;
    public string? IslandDescription { get; set; }

    public RoadParcel RoadParcel { get; set; } = null!;
}
```

### 3.3 EF Core Configuration

```csharp
public class CadastralDbContext : DbContext
{
    public DbSet<RoadParcel>  RoadParcels  { get; set; }
    public DbSet<RoadIsland>  RoadIslands  { get; set; }
    public DbSet<Parcel>      Parcels      { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder o)
        => o.UseSqlServer(
               connectionString,
               x => x.UseNetTopologySuite());   // ← required for geometry columns

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<RoadParcel>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Shape).HasColumnType("geometry");
            e.HasMany(r => r.Islands)
             .WithOne(i => i.RoadParcel)
             .HasForeignKey(i => i.RoadParcelId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<RoadIsland>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.IslandShape).HasColumnType("geometry");
        });

        mb.Entity<Parcel>(e =>
        {
            e.Property(p => p.Shape).HasColumnType("geometry");
        });
    }
}
```

---

## 4. GEOMETRY FACTORY — SINGLE SHARED INSTANCE

Create one factory for the whole application. Use the SRID that matches your
coordinate system. For Nepal/South Asia: 32644 (UTM zone 44N) or 4326 (WGS84).

```csharp
public static class SpatialConfig
{
    // Change SRID to match your project's coordinate reference system
    public const int SRID = 32644;

    public static readonly GeometryFactory Factory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: SRID);
}
```

---

## 5. RING WINDING UTILITIES

These utilities are used everywhere — in import, validation, and creation.

```csharp
using NetTopologySuite.Algorithm;

public static class RingWindingHelper
{
    /// <summary>Forces exterior ring to CCW and all interior rings to CW.</summary>
    public static Polygon NormaliseWindings(Polygon poly)
    {
        var factory = SpatialConfig.Factory;

        var outer = EnsureCCW(poly.ExteriorRing.Coordinates);
        var holes = new LinearRing[poly.NumInteriorRings];
        for (int i = 0; i < poly.NumInteriorRings; i++)
            holes[i] = factory.CreateLinearRing(
                            EnsureCW(poly.GetInteriorRingN(i).Coordinates));

        return factory.CreatePolygon(factory.CreateLinearRing(outer), holes);
    }

    public static Coordinate[] EnsureCCW(Coordinate[] coords)
        => Orientation.IsCCW(coords) ? coords : coords.Reverse().ToArray();

    public static Coordinate[] EnsureCW(Coordinate[] coords)
        => !Orientation.IsCCW(coords) ? coords : coords.Reverse().ToArray();
}
```

---

## 6. IMPORT SERVICE

The import service must handle every realistic input format. It must detect
donut polygons, fix winding, extract island shapes, and auto-detect road type.

```csharp
using NetTopologySuite.IO;

public class RoadParcelImportService
{
    private readonly GeometryFactory _f = SpatialConfig.Factory;
    private readonly CadastralDbContext _ctx;

    // ── 6.1 From WKT ──────────────────────────────────────────────────────

    public RoadParcel ImportFromWkt(string wkt, string parcelNo, string roadName)
    {
        var reader = new WKTReader(_f);
        var geom   = reader.Read(wkt);
        var poly   = ResolveToPolygon(geom);
        return BuildRoadParcel(poly, parcelNo, roadName, ImportSource.WKT);
    }

    // ── 6.2 From WKB (SQL Server varbinary / byte array) ──────────────────

    public RoadParcel ImportFromWkb(byte[] wkb, string parcelNo, string roadName)
    {
        var reader = new WKBReader(_f);
        var geom   = reader.Read(wkb);
        var poly   = ResolveToPolygon(geom);
        return BuildRoadParcel(poly, parcelNo, roadName, ImportSource.WKB);
    }

    // ── 6.3 From GeoJSON ──────────────────────────────────────────────────

    public RoadParcel ImportFromGeoJson(string geoJson, string parcelNo, string roadName)
    {
        var serializer = GeoJsonSerializer.Create(_f);
        using var sr   = new StringReader(geoJson);
        using var jr   = new Newtonsoft.Json.JsonTextReader(sr);
        var feature    = serializer.Deserialize<Feature>(jr)!;
        var poly       = ResolveToPolygon(feature.Geometry);
        return BuildRoadParcel(poly, parcelNo, roadName, ImportSource.GeoJSON);
    }

    // ── 6.4 Core builder ──────────────────────────────────────────────────

    private RoadParcel BuildRoadParcel(
        Polygon poly, string parcelNo, string roadName, ImportSource source)
    {
        // Step 1: Fix winding
        poly = RingWindingHelper.NormaliseWindings(poly);

        // Step 2: Build entity
        var road = new RoadParcel
        {
            RoadParcelNumber = parcelNo,
            RoadName         = roadName,
            Shape            = poly,
            RoadType         = AutoDetectRoadType(poly),
            ImportedFrom     = source,
            ImportedAt       = DateTime.UtcNow,
            ValidationStatus = DonutValidationStatus.NotChecked
        };

        // Step 3: Extract each hole as a RoadIsland record
        for (int i = 0; i < poly.NumInteriorRings; i++)
        {
            var holeCoords = poly.GetInteriorRingN(i).Coordinates;
            // Island exterior must be CCW (it is a standalone polygon now)
            var islandPoly = _f.CreatePolygon(
                _f.CreateLinearRing(
                    RingWindingHelper.EnsureCCW(holeCoords)));

            road.Islands.Add(new RoadIsland
            {
                HoleIndex    = i,
                IslandShape  = islandPoly,
                IslandDescription = $"Island {i + 1} of {road.RoadName}"
            });
        }

        return road;
    }

    // ── 6.5 MultiPolygon handling ─────────────────────────────────────────
    // Some export tools split a donut into multiple polygons.
    // Union them back into a single polygon with holes.

    private Polygon ResolveToPolygon(Geometry geom)
    {
        if (geom is Polygon p) return p;

        if (geom is MultiPolygon mp)
        {
            Geometry merged = mp.GetGeometryN(0);
            for (int i = 1; i < mp.NumGeometries; i++)
                merged = merged.Union(mp.GetGeometryN(i));

            if (merged is Polygon result) return result;
            throw new InvalidDataException(
                "MultiPolygon parts could not be merged into a single polygon. " +
                "Verify that the parts share edges.");
        }

        throw new InvalidDataException(
            $"Cannot import geometry of type '{geom.GeometryType}' as a road parcel.");
    }

    // ── 6.6 Auto-detect road type from shape ─────────────────────────────

    private RoadParcelType AutoDetectRoadType(Polygon poly)
    {
        if (poly.NumInteriorRings == 0)
            return RoadParcelType.StraightRoad;

        // Isoperimetric quotient — how circular is the outer ring?
        double perimeter    = poly.ExteriorRing.Length;
        double compactness  = (4 * Math.PI * poly.Area) / (perimeter * perimeter);
        if (compactness > 0.55)
            return RoadParcelType.Roundabout;

        // Is the first hole elongated? (median strip)
        var holeBounds = poly.GetInteriorRingN(0).EnvelopeInternal;
        double aspectRatio = holeBounds.Width > holeBounds.Height
            ? holeBounds.Width / holeBounds.Height
            : holeBounds.Height / holeBounds.Width;
        if (aspectRatio > 4.0)
            return RoadParcelType.MedianRoad;

        if (poly.NumInteriorRings == 1)
            return RoadParcelType.CulDeSac;

        if (poly.NumInteriorRings >= 2)
            return RoadParcelType.Junction;

        return RoadParcelType.Unknown;
    }
}
```

---

## 7. VALIDATION SERVICE

Run this after every import and before every save. Log all errors to the entity.

```csharp
public class RoadParcelValidator
{
    private readonly GeometryFactory _f = SpatialConfig.Factory;

    public (DonutValidationStatus status, string message) Validate(RoadParcel road)
    {
        var poly = road.Shape;

        // 7.1 Basic NTS validity
        if (!poly.IsValid)
        {
            var reason = new NetTopologySuite.Operation.Valid.IsValidOp(poly)
                             .ValidationError?.Message ?? "unknown";
            return (DonutValidationStatus.InvalidGeometry,
                    $"Geometry invalid: {reason}");
        }

        // 7.2 Exterior ring winding
        if (!NetTopologySuite.Algorithm.Orientation.IsCCW(
                poly.ExteriorRing.Coordinates))
            return (DonutValidationStatus.WrongWindingDirection,
                    "Exterior ring is not Counter-Clockwise.");

        for (int i = 0; i < poly.NumInteriorRings; i++)
        {
            // 7.3 Interior ring winding
            if (NetTopologySuite.Algorithm.Orientation.IsCCW(
                    poly.GetInteriorRingN(i).Coordinates))
                return (DonutValidationStatus.WrongWindingDirection,
                        $"Interior ring {i} is not Clockwise.");

            var hole = _f.CreatePolygon(
                _f.CreateLinearRing(poly.GetInteriorRingN(i).Coordinates));

            // 7.4 Hole must be inside outer boundary
            var outer = _f.CreatePolygon(
                _f.CreateLinearRing(poly.ExteriorRing.Coordinates));
            if (!outer.Contains(hole))
                return (DonutValidationStatus.HoleOutsideExterior,
                        $"Interior ring {i} lies outside the exterior boundary.");

            // 7.5 Holes must not overlap each other
            for (int j = i + 1; j < poly.NumInteriorRings; j++)
            {
                var hole2 = _f.CreatePolygon(
                    _f.CreateLinearRing(poly.GetInteriorRingN(j).Coordinates));
                if (hole.Intersects(hole2) && !hole.Touches(hole2))
                    return (DonutValidationStatus.HolesOverlap,
                            $"Interior rings {i} and {j} overlap.");
            }

            // 7.6 Total hole area must be less than outer area
            if (hole.Area >= outer.Area)
                return (DonutValidationStatus.HoleAreaExceedsParcel,
                        $"Interior ring {i} area equals or exceeds the outer ring.");
        }

        return (DonutValidationStatus.Valid, "OK");
    }

    public void ValidateAndApply(RoadParcel road)
    {
        var (status, message) = Validate(road);
        road.ValidationStatus  = status;
        road.ValidationMessage = message;
    }
}
```

---

## 8. RENDERING — WinForms GDI+

The rendering engine must correctly draw road parcels that are donuts. The key
technique is `FillMode.Alternate` on `GraphicsPath` — this causes any enclosed
sub-path to become a transparent hole rather than a filled area.

### 8.1 GraphicsPath Builder

```csharp
using System.Drawing.Drawing2D;

public static class ParcelPathBuilder
{
    /// <summary>
    /// Converts an NTS Polygon to a GDI+ GraphicsPath.
    /// Holes are correctly transparent because of FillMode.Alternate.
    /// </summary>
    public static GraphicsPath ToPath(
        Polygon poly,
        Func<Coordinate, PointF> worldToScreen)
    {
        var path = new GraphicsPath { FillMode = FillMode.Alternate };

        // Outer ring
        path.AddPolygon(
            poly.ExteriorRing.Coordinates
                .Select(worldToScreen).ToArray());

        // Each interior ring adds a transparent hole
        for (int i = 0; i < poly.NumInteriorRings; i++)
            path.AddPolygon(
                poly.GetInteriorRingN(i).Coordinates
                    .Select(worldToScreen).ToArray());

        return path;
    }
}
```

### 8.2 Road Parcel Renderer

```csharp
public class RoadParcelRenderer
{
    // Fill colours per road type — semi-transparent so underlying map shows
    private static readonly Dictionary<RoadParcelType, Color> FillColors = new()
    {
        { RoadParcelType.StraightRoad, Color.FromArgb(80,  160, 160, 175) },
        { RoadParcelType.Roundabout,   Color.FromArgb(100, 100, 140, 210) },
        { RoadParcelType.MedianRoad,   Color.FromArgb(80,  120, 130, 195) },
        { RoadParcelType.CulDeSac,     Color.FromArgb(90,  145, 110, 190) },
        { RoadParcelType.Junction,     Color.FromArgb(90,  185, 130, 130) },
        { RoadParcelType.Highway,      Color.FromArgb(80,  200, 160, 100) },
    };

    public void Draw(
        Graphics g,
        RoadParcel road,
        Func<Coordinate, PointF> worldToScreen,
        bool isSelected = false,
        bool showIslandOutlines = true)
    {
        var path = ParcelPathBuilder.ToPath(road.Shape, worldToScreen);

        // ── Fill (holes show through automatically) ──
        var baseColor = FillColors.GetValueOrDefault(
            road.RoadType, Color.FromArgb(80, 160, 160, 175));
        using var fill = new SolidBrush(
            isSelected ? Color.FromArgb(160, 255, 215, 0) : baseColor);
        g.FillPath(fill, path);

        // ── Outer boundary ──
        using var outerPen = new Pen(
            isSelected ? Color.DarkOrange : Color.DimGray,
            isSelected ? 2.5f : 1.5f);
        g.DrawPath(outerPen, path);

        // ── Island hole outlines (dashed green) ──
        if (showIslandOutlines && road.IsDonut)
        {
            using var islandPen = new Pen(Color.ForestGreen, 1.0f)
                { DashStyle = DashStyle.Dash };

            for (int i = 0; i < road.Shape.NumInteriorRings; i++)
            {
                var pts = road.Shape.GetInteriorRingN(i)
                              .Coordinates.Select(worldToScreen).ToArray();
                g.DrawPolygon(islandPen, pts);
            }
        }

        // ── Road name label at centroid ──
        DrawLabel(g, road, worldToScreen);
    }

    private void DrawLabel(
        Graphics g, RoadParcel road,
        Func<Coordinate, PointF> worldToScreen)
    {
        if (string.IsNullOrWhiteSpace(road.RoadName)) return;

        // NTS centroid of a donut polygon correctly falls in the road band
        var centroid = road.Shape.Centroid;
        var pt       = worldToScreen(centroid.Coordinate);

        using var font  = new Font("Segoe UI", 7.5f, FontStyle.Regular);
        using var brush = new SolidBrush(Color.Black);
        var size = g.MeasureString(road.RoadName, font);
        g.DrawString(road.RoadName, font, brush,
            pt.X - size.Width  / 2f,
            pt.Y - size.Height / 2f);
    }
}
```

### 8.3 Re-plotting / Map Refresh

When re-plotting the entire map, iterate all road parcels and call Draw() in
order. Always dispose Graphics objects. Use double-buffering on the map Panel
to prevent flicker:

```csharp
// In your map Panel constructor or Load event:
mapPanel.GetType()
    .GetProperty("DoubleBuffered",
        System.Reflection.BindingFlags.Instance |
        System.Reflection.BindingFlags.NonPublic)!
    .SetValue(mapPanel, true);

// In the Panel's Paint event:
private void mapPanel_Paint(object sender, PaintEventArgs e)
{
    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
    e.Graphics.Clear(Color.WhiteSmoke);

    foreach (var parcel in _generalParcels)
        _parcelRenderer.Draw(e.Graphics, parcel, WorldToScreen);

    foreach (var road in _roadParcels)
        _roadRenderer.Draw(e.Graphics, road, WorldToScreen,
            isSelected: road.Id == _selectedRoadId);
}
```

---

## 9. SPATIAL QUERY SERVICE

```csharp
public class RoadSpatialQueryService
{
    private readonly CadastralDbContext _ctx;
    private readonly GeometryFactory    _f = SpatialConfig.Factory;

    // ── 9.1 Is a point on the road surface (NOT in any island hole)? ──────
    // NTS Contains() respects holes — returns false if pt is in a hole
    public bool IsPointOnRoad(RoadParcel road, Coordinate point)
        => road.Shape.Contains(_f.CreatePoint(point));

    // ── 9.2 Find all road parcels that are donut-shaped ──────────────────
    public async Task<List<RoadParcel>> GetDonutRoadParcelsAsync()
        => await _ctx.RoadParcels
                     .Include(r => r.Islands)
                     .Where(r => r.RoadType != RoadParcelType.StraightRoad)
                     .ToListAsync();

    // ── 9.3 Find road parcel at a given map click ─────────────────────────
    public async Task<RoadParcel?> HitTestAsync(Coordinate clickCoord)
    {
        var pt = _f.CreatePoint(clickCoord);
        return await _ctx.RoadParcels
                         .Where(r => r.Shape.Contains(pt))
                         .FirstOrDefaultAsync();
    }

    // ── 9.4 Find general parcels adjacent to a road ───────────────────────
    public async Task<List<Parcel>> GetAdjacentParcelsAsync(RoadParcel road)
    {
        var boundary = road.Shape.Boundary;
        return await _ctx.Parcels
                         .Where(p => p.Shape.Intersects(boundary))
                         .ToListAsync();
    }

    // ── 9.5 Find the parcel that occupies a road's island hole ───────────
    public async Task<Parcel?> FindIslandParcelAsync(RoadParcel road, int holeIndex)
    {
        if (holeIndex >= road.Shape.NumInteriorRings) return null;

        var holePoly = _f.CreatePolygon(
            _f.CreateLinearRing(
                road.Shape.GetInteriorRingN(holeIndex).Coordinates));

        return await _ctx.Parcels
                         .Where(p => holePoly.Contains(p.Shape)
                                  || p.Shape.Equals(holePoly))
                         .FirstOrDefaultAsync();
    }

    // ── 9.6 Get road parcels within a bounding box (viewport query) ───────
    public async Task<List<RoadParcel>> GetRoadParcelsInViewAsync(Envelope envelope)
    {
        var bbox = _f.ToGeometry(envelope);
        return await _ctx.RoadParcels
                         .Include(r => r.Islands)
                         .Where(r => r.Shape.Intersects(bbox))
                         .ToListAsync();
    }
}
```

---

## 10. ROAD PARCEL CREATION UTILITIES

Sometimes a road parcel must be built programmatically (e.g. from a digitised
centre-line or from an existing parcel minus an island).

```csharp
public class RoadParcelCreationService
{
    private readonly GeometryFactory _f = SpatialConfig.Factory;

    // ── 10.1 Create donut by subtracting island from road boundary ────────
    public Polygon CreateDonutFromRoadAndIsland(
        Polygon roadOuter, Polygon islandInner)
    {
        // Both must be valid
        if (!roadOuter.Contains(islandInner))
            throw new ArgumentException(
                "Island polygon must be fully inside the road outer polygon.");

        var result = roadOuter.Difference(islandInner);

        return result as Polygon
               ?? throw new InvalidOperationException(
                   "Difference produced a MultiPolygon. " +
                   "The island may touch the road boundary — " +
                   "islands must be strictly inside.");
    }

    // ── 10.2 Build road polygon from a centre-line and width ──────────────
    public Polygon BuildRoadFromCentreLine(
        Coordinate[] centreLine, double halfWidthMetres)
    {
        var line = _f.CreateLineString(centreLine);
        return (Polygon)line.Buffer(halfWidthMetres,
            new NetTopologySuite.Operation.Buffer.BufferParameters
            {
                EndCapStyle    = NetTopologySuite.Operation.Buffer
                                     .EndCapStyle.Flat,
                JoinStyle      = NetTopologySuite.Operation.Buffer
                                     .JoinStyle.Mitre,
                MitreLimit     = 5.0
            });
    }

    // ── 10.3 Add a new hole to an existing road polygon ───────────────────
    public Polygon AddIslandHole(Polygon existingRoad, Polygon newIsland)
    {
        var result = existingRoad.Difference(newIsland);
        return result as Polygon
               ?? throw new InvalidOperationException(
                   "Could not add island — result is not a simple polygon.");
    }

    // ── 10.4 Remove (fill) an island hole ────────────────────────────────
    public Polygon RemoveIslandHole(Polygon donutRoad, int holeIndex)
    {
        var holes = Enumerable.Range(0, donutRoad.NumInteriorRings)
                              .Where(i => i != holeIndex)
                              .Select(i => _f.CreateLinearRing(
                                               donutRoad.GetInteriorRingN(i).Coordinates))
                              .ToArray();

        return _f.CreatePolygon(
            _f.CreateLinearRing(donutRoad.ExteriorRing.Coordinates),
            holes);
    }
}
```

---

## 11. SQL SERVER — SPATIAL INDEXES

Add these migrations (or run directly) to ensure performant spatial queries.

```sql
-- Spatial index on road parcel shapes
CREATE SPATIAL INDEX SIX_RoadParcels_Shape
    ON RoadParcels(Shape)
    USING GEOMETRY_AUTO_GRID
    WITH (CELLS_PER_OBJECT = 16);

-- Spatial index on general parcel shapes
CREATE SPATIAL INDEX SIX_Parcels_Shape
    ON Parcels(Shape)
    USING GEOMETRY_AUTO_GRID
    WITH (CELLS_PER_OBJECT = 16);

-- Spatial index on island shapes
CREATE SPATIAL INDEX SIX_RoadIslands_IslandShape
    ON RoadIslands(IslandShape)
    USING GEOMETRY_AUTO_GRID
    WITH (CELLS_PER_OBJECT = 8);
```

---

## 12. WORLD-TO-SCREEN COORDINATE TRANSFORM

The renderer needs a stable transform from real-world coordinates to screen
pixels. Implement a `MapViewTransform` class:

```csharp
public class MapViewTransform
{
    private double _scaleX, _scaleY, _offsetX, _offsetY;

    /// <summary>
    /// Fits the given spatial extent into the given screen rectangle.
    /// Call this when the map panel resizes or the user zooms/pans.
    /// </summary>
    public void FitToExtent(Envelope worldExtent, Rectangle screenRect)
    {
        _scaleX  = screenRect.Width  / worldExtent.Width;
        _scaleY  = screenRect.Height / worldExtent.Height;
        _offsetX = screenRect.Left - worldExtent.MinX * _scaleX;
        _offsetY = screenRect.Top  + worldExtent.MaxY * _scaleY;
    }

    public PointF ToScreen(Coordinate world)
        => new PointF(
               (float)(world.X * _scaleX + _offsetX),
               (float)(-world.Y * _scaleY + _offsetY));  // Y is flipped

    public Coordinate ToWorld(PointF screen)
        => new Coordinate(
               (screen.X - _offsetX) / _scaleX,
               -(screen.Y - _offsetY) / _scaleY);
}
```

---

## 13. REQUIRED NUGET PACKAGES

Add these to your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="NetTopologySuite"
                    Version="2.5.*" />
  <PackageReference Include="NetTopologySuite.IO.GeoJSON"
                    Version="4.*" />
  <PackageReference Include="NetTopologySuite.IO.SqlServerBytes"
                    Version="2.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore"
                    Version="8.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer"
                    Version="8.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite"
                    Version="8.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools"
                    Version="8.*"
                    PrivateAssets="all" />
  <PackageReference Include="Newtonsoft.Json"
                    Version="13.*" />
</ItemGroup>
```

---

## 14. IMPLEMENTATION CHECKLIST FOR CODEX

Work through these tasks in order. Each task should be a separate file or class
unless noted. After completing each task, run the compiler before moving on.

### Phase 1 — Foundation
- [ ] Add all NuGet packages listed in Section 13
- [ ] Create `SpatialConfig.cs` with shared `GeometryFactory`
- [ ] Create all enums (`RoadParcelType`, `ImportSource`, `DonutValidationStatus`)
- [ ] Create entity classes (`RoadParcel`, `RoadIsland`, `Parcel`)
- [ ] Create `CadastralDbContext` with EF Core configuration
- [ ] Run `Add-Migration InitialCreate` and `Update-Database`

### Phase 2 — Geometry Utilities
- [ ] Create `RingWindingHelper.cs`
- [ ] Create `ParcelPathBuilder.cs`
- [ ] Create `MapViewTransform.cs`
- [ ] Write unit tests: create a donut polygon, verify `NumInteriorRings == 1`,
      verify `Contains()` returns false for a point inside the hole

### Phase 3 — Import & Validation
- [ ] Create `RoadParcelImportService.cs` (Section 6)
- [ ] Create `RoadParcelValidator.cs` (Section 7)
- [ ] Wire validator to import service — always validate after building
- [ ] Test with all four WKT types: plain polygon, donut, multi-polygon, invalid

### Phase 4 — Rendering
- [ ] Create `RoadParcelRenderer.cs` (Section 8)
- [ ] Enable double-buffering on map Panel
- [ ] Implement `mapPanel_Paint` handler
- [ ] Verify donut roads render with transparent holes (island shows through)
- [ ] Verify island dashed outline is drawn

### Phase 5 — Spatial Queries
- [ ] Create `RoadSpatialQueryService.cs` (Section 9)
- [ ] Wire hit-test to mouse-click on map panel
- [ ] Add viewport query to only load road parcels in current view

### Phase 6 — Creation Utilities
- [ ] Create `RoadParcelCreationService.cs` (Section 10)
- [ ] Add UI for subtracting an island from a road boundary

### Phase 7 — Database
- [ ] Apply spatial indexes from Section 11

---

## 15. DECISION RULES FOR CODEX — THINK ON YOUR OWN

When you encounter a situation not explicitly covered above, apply these rules:

1. **Ambiguous geometry type from import** → always try to resolve to `Polygon`
   via `Union()`. If it still is not a `Polygon`, throw a descriptive exception.

2. **MultiPolygon with more than 2 parts** → union all parts. If the union does
   not produce a valid `Polygon`, log a warning and import it as-is, marking
   `ValidationStatus = InvalidGeometry`.

3. **Interior ring winding is wrong** → auto-fix silently using
   `RingWindingHelper`. Do not reject the import for winding alone.

4. **Hole extends outside exterior ring** → this is a critical error. Do not
   auto-fix. Set `ValidationStatus = HoleOutsideExterior` and surface the error
   to the user. Do not save.

5. **RoadType detection ambiguous** → default to `RoadParcelType.Unknown`. Let
   the user correct it via a dropdown in the property panel.

6. **Point-in-polygon hit test returns two results** → prefer the road parcel
   over a general parcel (road parcels take topological priority).

7. **Island parcel not found in the database** → leave `LinkedParcelNumber`
   null. Do not fail the import. Show a yellow warning badge in the UI.

8. **Coordinate system mismatch** → if imported WKT has no SRID or a different
   SRID from `SpatialConfig.SRID`, log a warning and re-project if a projector
   service is available, otherwise import as-is and flag for review.

9. **Very large polygons (>500 vertices per ring)** → simplify with
   `NetTopologySuite.Simplify.TopologyPreservingSimplifier` using tolerance
   `0.1` (metres) before storing.

10. **UI re-plot performance** → only redraw parcels whose bounding box
    intersects the current viewport `Envelope`. Use `EnvelopeInternal` for fast
    filtering before calling `worldToScreen`.

---

## 16. SUMMARY REFERENCE CARD

| Task                               | API / Pattern                                    |
|------------------------------------|--------------------------------------------------|
| Detect donut                       | `poly.NumInteriorRings > 0`                      |
| Get hole as polygon                | `poly.GetInteriorRingN(i).Coordinates`           |
| Fix exterior ring CCW              | `RingWindingHelper.EnsureCCW(coords)`            |
| Fix interior ring CW               | `RingWindingHelper.EnsureCW(coords)`             |
| Road area (minus islands)          | `poly.Area`                                      |
| Point on road (not in hole)        | `poly.Contains(point)` — holes respected         |
| Draw with transparent holes        | `GraphicsPath` + `FillMode.Alternate`            |
| Label at road centroid             | `poly.Centroid.Coordinate`                       |
| Subtract island from road          | `roadOuter.Difference(islandInner)`              |
| Merge split MultiPolygon           | `geom1.Union(geom2)` iteratively                 |
| Validate geometry                  | `poly.IsValid` + `IsValidOp`                     |
| Import WKT                         | `new WKTReader(factory).Read(wkt)`               |
| Import WKB                         | `new WKBReader(factory).Read(bytes)`             |
| Store in SQL Server                | `HasColumnType("geometry")` + NTS EF extension  |
| Spatial index (SQL)                | `CREATE SPATIAL INDEX ... USING GEOMETRY_AUTO_GRID` |

---

*End of specification. Codex: begin with Phase 1 of Section 14.*
