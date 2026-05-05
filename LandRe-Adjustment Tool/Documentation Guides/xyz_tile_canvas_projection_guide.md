# RePlot Map Canvas: Correct Online XYZ Tile Loading and Projection Guide

## Purpose

This document explains how online map tiles should be loaded into the RePlot / Land Readjustment Tool map canvas without distortion.

It is intended to be used as a development guide for GitHub Copilot while fixing the current raster/XYZ tile implementation.

The current visual problem is that online satellite/map tiles are downloaded and displayed, but they appear as distorted rectangular blocks. The root cause is almost certainly an incorrect projection/georeferencing workflow for XYZ tiles.

---

## Core Rule

**Online XYZ map tiles are normally in Web Mercator projection, EPSG:3857.**

Even when the user enters bounds using latitude/longitude in WGS84, EPSG:4326, the actual tile image grid is not a simple latitude/longitude grid. It is a Web Mercator tile pyramid.

Therefore:

```text
User input bounds:       EPSG:4326 latitude/longitude
XYZ tile image grid:     EPSG:3857 Web Mercator
Project/canvas CRS:      Project CRS selected in RePlot settings
```

The correct pipeline is:

```text
WGS84 bounds
    -> convert to XYZ tile range
    -> download z/x/y tiles
    -> stitch tile images into a mosaic
    -> assign Web Mercator georeference, EPSG:3857
    -> reproject/warp to project CRS
    -> save imported raster
    -> render using final projected bounds
```

Never treat downloaded XYZ tiles as a normal EPSG:4326 raster.

---

## Symptoms of Wrong Implementation

The canvas may show:

- Satellite imagery split into large square or rectangular patches.
- World imagery stretched horizontally or vertically.
- Tiles that appear in the wrong place.
- Correct-looking local tile images, but incorrect world position.
- Blocks overlapping or leaving gaps.
- Distortion increasing toward north/south latitudes.

These symptoms usually mean that either:

1. Tile row/column calculation is wrong.
2. Tile Y coordinate is handled incorrectly.
3. The tile mosaic is assigned EPSG:4326 instead of EPSG:3857.
4. The mosaic geotransform is incorrect.
5. The canvas draws raster bounds using wrong coordinates.
6. Pixel Y size is positive instead of negative.

---

## Existing RePlot Code Areas to Inspect

### 1. `frmXyzTileImportOptions.cs`

This form collects:

- Layer name
- Tile source URL template
- Bounds
- Zoom level
- Image extension

It already supports:

- Center + radius mode
- Bounding box mode
- Nepal default bounds
- Tile source selection
- Download-before-import workflow

The UI is not the main issue. The important thing is that the values returned by this form are WGS84 geographic bounds:

```csharp
(minLon, minLat, maxLon, maxLat)
```

These should be understood as EPSG:4326 only for tile range calculation. They should not be used as the raster's final image georeference.

---

### 2. `XyzTilePreDownloadService`

This service should:

- Convert WGS84 bounds to tile X/Y index range.
- Download every tile in that range.
- Save them to the project cache/download folder.

Check that this service uses proper Web Mercator XYZ formulas.

It must not divide latitude/longitude linearly into tiles.

---

### 3. `XyzTileSourceService.CreateSourceDefinition(...)`

This service is critical.

It should create a source definition for the downloaded tiles/mosaic. The source extent and source CRS must be based on EPSG:3857.

Expected behavior:

```text
sourceDefinition.SourceExtent.SrsDefinition = "EPSG:3857"
```

The source extent should be in Web Mercator meters, not degrees.

---

### 4. `RasterLayerImportService.ImportAsync(...)`

This service should:

- Read the XYZ source definition or mosaic.
- Understand its source CRS as EPSG:3857.
- Reproject it into the project CRS.
- Save the imported raster layer.
- Persist correct raster bounds.

If the project CRS is WGS84 / UTM / MUTM or any local CRS, GDAL should perform the warp/reprojection.

---

### 5. `MapCanvasControl.cs`

The map canvas should not guess raster coordinates.

It should:

- Read the final imported raster layer's projected bounds.
- Render that raster using `IRasterRenderLayer.WorldBounds`.
- Zoom to raster layer using actual projected bounds.

The canvas itself should not treat XYZ tiles as lat/lon images.

---

## Correct XYZ Tile Math

### Longitude to Tile X

```csharp
private static int LonToTileX(double lon, int zoom)
{
    double n = Math.Pow(2.0, zoom);
    int x = (int)Math.Floor((lon + 180.0) / 360.0 * n);
    return ClampTileIndex(x, zoom);
}
```

### Latitude to Tile Y

```csharp
private static int LatToTileY(double lat, int zoom)
{
    lat = Math.Clamp(lat, -85.05112878, 85.05112878);

    double latRad = lat * Math.PI / 180.0;
    double n = Math.Pow(2.0, zoom);

    int y = (int)Math.Floor(
        (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * n);

    return ClampTileIndex(y, zoom);
}
```

### Clamp Tile Index

```csharp
private static int ClampTileIndex(int value, int zoom)
{
    int max = (1 << zoom) - 1;
    return Math.Clamp(value, 0, max);
}
```

### Bounding Box to Tile Range

```csharp
private static (int xMin, int yMin, int xMax, int yMax) GetTileRange(
    double minLon,
    double minLat,
    double maxLon,
    double maxLat,
    int zoom)
{
    int xMin = LonToTileX(minLon, zoom);
    int xMax = LonToTileX(maxLon, zoom);

    // North latitude gives smaller Y tile number.
    int yMin = LatToTileY(maxLat, zoom);
    int yMax = LatToTileY(minLat, zoom);

    if (xMin > xMax)
        (xMin, xMax) = (xMax, xMin);

    if (yMin > yMax)
        (yMin, yMax) = (yMax, yMin);

    return (xMin, yMin, xMax, yMax);
}
```

---

## Correct Web Mercator Georeferencing

Each tile is 256 x 256 pixels unless the tile source states otherwise.

For normal XYZ tiles:

```csharp
private const double WebMercatorOriginShift = 20037508.342789244;
private const double InitialResolution = 156543.03392804097;
private const int TileSize = 256;
```

### Resolution at Zoom

```csharp
private static double GetResolution(int zoom)
{
    return InitialResolution / Math.Pow(2.0, zoom);
}
```

### Tile Range to Web Mercator Bounds

```csharp
private static WebMercatorBounds TileRangeToWebMercatorBounds(
    int xMin,
    int yMin,
    int xMax,
    int yMax,
    int zoom)
{
    double resolution = GetResolution(zoom);

    double minX = xMin * TileSize * resolution - WebMercatorOriginShift;
    double maxX = (xMax + 1) * TileSize * resolution - WebMercatorOriginShift;

    double maxY = WebMercatorOriginShift - yMin * TileSize * resolution;
    double minY = WebMercatorOriginShift - (yMax + 1) * TileSize * resolution;

    return new WebMercatorBounds(minX, minY, maxX, maxY, resolution);
}

private sealed record WebMercatorBounds(
    double MinX,
    double MinY,
    double MaxX,
    double MaxY,
    double Resolution);
```

---

## Correct GDAL GeoTransform for Stitched XYZ Mosaic

After stitching tiles into a single raster image, assign this geotransform:

```text
GeoTransform[0] = minX
GeoTransform[1] = resolution
GeoTransform[2] = 0
GeoTransform[3] = maxY
GeoTransform[4] = 0
GeoTransform[5] = -resolution
Projection      = EPSG:3857
```

C# representation:

```csharp
var geoTransform = new double[6]
{
    bounds.MinX,
    bounds.Resolution,
    0,
    bounds.MaxY,
    0,
    -bounds.Resolution
};
```

Important:

```text
GeoTransform[5] must be negative.
```

Raster rows increase downward, but map Y increases upward. A positive Y pixel size will flip or distort the raster.

---

## Tile Stitching Logic

The tile mosaic size should be:

```csharp
int tileCountX = xMax - xMin + 1;
int tileCountY = yMax - yMin + 1;

int mosaicWidth = tileCountX * 256;
int mosaicHeight = tileCountY * 256;
```

Each tile should be drawn at:

```csharp
int pixelX = (tileX - xMin) * 256;
int pixelY = (tileY - yMin) * 256;
```

Example:

```csharp
using Bitmap mosaic = new Bitmap(mosaicWidth, mosaicHeight);
using Graphics g = Graphics.FromImage(mosaic);

for (int y = yMin; y <= yMax; y++)
{
    for (int x = xMin; x <= xMax; x++)
    {
        string tilePath = GetTilePath(zoom, x, y);
        if (!File.Exists(tilePath))
            continue;

        using Image tile = Image.FromFile(tilePath);
        int px = (x - xMin) * TileSize;
        int py = (y - yMin) * TileSize;
        g.DrawImage(tile, px, py, TileSize, TileSize);
    }
}
```

---

## Recommended Import Strategy

### Option A: Best Reliable Approach

Create a temporary GeoTIFF from the stitched tile mosaic using EPSG:3857, then pass that GeoTIFF to the existing raster import service.

```text
Download XYZ tiles
    -> stitch PNG/JPG mosaic
    -> write temporary GeoTIFF with EPSG:3857
    -> import GeoTIFF using RasterLayerImportService
    -> GDAL reprojects to project CRS
```

This is the easiest and most reliable approach because your existing raster import pipeline already appears designed to handle raster CRS and project CRS transformation.

---

### Option B: Use GDAL VRT Instead of Big Temporary Image

Create a VRT that references each tile image and assigns each tile its own EPSG:3857 georeference.

This is more memory-efficient but harder to implement.

Use this later if large tile downloads become slow or memory-heavy.

---

### Option C: Dynamic Online Tile Renderer

For true online basemap behavior, do not import the whole world or even large areas as one raster.

Instead:

```text
Canvas viewport
    -> transform viewport corners to EPSG:3857 / EPSG:4326
    -> calculate visible XYZ tiles
    -> download/cache only visible tiles
    -> draw tiles directly on canvas
```

This is the correct architecture for live online basemaps, but it requires a separate renderer.

For now, fix the offline/imported XYZ workflow first.

---

## Do Not Download the Whole World

Do not attempt to load the whole world imagery into the canvas as a single raster.

At high zoom levels this becomes impossible:

```text
Zoom 0:      1 tile
Zoom 5:      1,024 tiles
Zoom 10:     1,048,576 tiles
Zoom 15:     1,073,741,824 tiles
Zoom 19:     enormous / impossible
```

The software should download only:

- Project area
- Municipality area
- User-defined bounding box
- Visible viewport tiles

For Nepal land pooling projects, a radius of 1 km to 10 km is normally more practical than whole-country or whole-world imagery.

---

## Validation Rules to Add

Before downloading tiles, calculate tile count:

```csharp
int tileCount = (xMax - xMin + 1) * (yMax - yMin + 1);
```

Warn or block if tile count is too high:

```csharp
if (tileCount > 5000)
{
    throw new InvalidOperationException(
        $"The selected area and zoom level require {tileCount:N0} tiles. " +
        "Please reduce the area or choose a lower zoom level.");
}
```

Suggested limits:

```text
Preview/download test:      100 - 500 tiles
Normal project import:      up to 5,000 tiles
Advanced/manual override:   up to 20,000 tiles
```

---

## URL Handling Notes

XYZ URL templates should support:

```text
{z}
{x}
{y}
```

Optional future support:

```text
{s}        subdomain, e.g. a/b/c
{-y}       TMS inverted Y
{quadkey}  Bing-style quadkey
```

For normal XYZ sources, do not invert Y.

TMS uses inverted Y:

```csharp
int tmsY = ((1 << zoom) - 1) - xyzY;
```

Only use this if the source explicitly requires TMS.

---

## Correct Source CRS Decision

### For normal raster import

Use the raster's own stored CRS if available.

If missing, let the user define source CRS.

### For XYZ tile import

Do not ask the user to define the tile CRS for normal XYZ sources.

Always use:

```text
EPSG:3857
```

Reason:

```text
XYZ web map tiles are generated in the Web Mercator tile pyramid.
```

The user-entered lat/lon bounds only identify the download area.

---

## Pseudocode for Fixed XYZ Import

```csharp
public async Task<RasterLayerImportResult> ImportXyzTilesAsync(
    XyzTileSourceImportRequest request,
    ProjectContext context,
    IProgress<RasterImportProgressInfo> progress,
    CancellationToken ct)
{
    // 1. User request bounds are WGS84 degrees.
    double minLon = request.MinLongitude;
    double minLat = request.MinLatitude;
    double maxLon = request.MaxLongitude;
    double maxLat = request.MaxLatitude;
    int zoom = request.ZoomLevel;

    // 2. Calculate XYZ tile range.
    var range = GetTileRange(minLon, minLat, maxLon, maxLat, zoom);

    // 3. Validate tile count.
    int tileCount = (range.xMax - range.xMin + 1) *
                    (range.yMax - range.yMin + 1);

    if (tileCount > 5000)
        throw new InvalidOperationException("Too many tiles selected.");

    // 4. Download missing tiles.
    await DownloadTilesAsync(request.UrlTemplate, range, zoom, ct);

    // 5. Stitch tiles into a mosaic image.
    string mosaicImagePath = StitchTilesToImage(range, zoom, request.ImageExtension);

    // 6. Convert tile range to EPSG:3857 bounds.
    WebMercatorBounds bounds = TileRangeToWebMercatorBounds(
        range.xMin,
        range.yMin,
        range.xMax,
        range.yMax,
        zoom);

    // 7. Write temporary GeoTIFF with EPSG:3857 georeference.
    string tempGeoTiff = WriteGeoTiff3857(mosaicImagePath, bounds);

    // 8. Import through existing raster import service.
    return await _rasterLayerImportService.ImportAsync(
        new RasterLayerImportRequest(
            context.Session,
            context.ProjectFolderPath,
            tempGeoTiff,
            request.LayerName,
            "EPSG:3857"),
        progress,
        ct);
}
```

---

## GDAL Writing Checklist

When creating the temporary GeoTIFF:

1. Driver: `GTiff`
2. Bands: normally 3 or 4 depending on source image
3. Projection: EPSG:3857 WKT
4. GeoTransform: Web Mercator bounds and resolution
5. Compression: optional but recommended
6. BigTIFF: enable when large

Suggested GDAL creation options:

```text
COMPRESS=JPEG or COMPRESS=DEFLATE
TILED=YES
BIGTIFF=IF_SAFER
```

For satellite imagery:

```text
COMPRESS=JPEG
JPEG_QUALITY=85
TILED=YES
BIGTIFF=IF_SAFER
```

For transparent PNG-style maps:

```text
COMPRESS=DEFLATE
TILED=YES
BIGTIFF=IF_SAFER
```

---

## Canvas Rendering Checklist

After import, verify that:

- Raster layer bounds are in project CRS.
- `IRasterRenderLayer.WorldBounds` uses projected coordinates.
- `ZoomToRasterLayer()` zooms to the imported raster's true bounds.
- The renderer does not reproject again during drawing unless designed to do so.
- The canvas world coordinate system matches the project CRS.

Do not render EPSG:3857 rasters directly on a project CRS canvas unless the project CRS is also EPSG:3857.

---

## Common Mistakes to Avoid

### Mistake 1: Assigning EPSG:4326 to XYZ mosaic

Wrong:

```text
Downloaded XYZ mosaic CRS = EPSG:4326
```

Correct:

```text
Downloaded XYZ mosaic CRS = EPSG:3857
```

---

### Mistake 2: Linear latitude tile calculation

Wrong:

```csharp
y = (lat + 90) / 180 * tileCount;
```

Correct:

```csharp
y = (1 - log(tan(latRad) + sec(latRad)) / PI) / 2 * tileCount;
```

---

### Mistake 3: Positive Y pixel size

Wrong:

```text
GeoTransform[5] = resolution
```

Correct:

```text
GeoTransform[5] = -resolution
```

---

### Mistake 4: Drawing downloaded tiles by lat/lon rectangle

Wrong:

```text
Draw tile image directly from minLon/minLat/maxLon/maxLat
```

Correct:

```text
Convert tile range to EPSG:3857 meters, then reproject to project CRS
```

---

### Mistake 5: Importing huge areas

Wrong:

```text
Download world imagery at zoom 15
```

Correct:

```text
Download only project area or visible viewport
```

---

## Testing Plan

### Test 1: Kathmandu small area

Use:

```text
Center lat: 27.7172
Center lon: 85.3240
Radius:     2 km
Zoom:       15 or 16
```

Expected:

- Tiles align continuously.
- No rectangular patch distortion.
- Roads/rivers/buildings look natural.
- Raster bounds match Kathmandu location.

---

### Test 2: Nepal bounding box at low zoom

Use:

```text
West:  80.058622
South: 26.347000
East:  88.201525
North: 30.447020
Zoom:  7 or 8
```

Expected:

- Nepal appears in correct location.
- No flipped north/south direction.
- Tile seams are minimal or invisible.

---

### Test 3: Compare with known GIS software

Load the same imagery in:

- QGIS
- ArcGIS
- Google Earth style reference
- Existing working road design software

Expected:

- Same general shape and position.
- Same orientation.
- Similar scale.

---

### Test 4: Project CRS reprojection

If RePlot project CRS is UTM/MUTM:

- Import XYZ tiles.
- Confirm final raster is transformed into project CRS.
- Overlay cadastral/project boundary data.
- Check that overlay is not shifted.

---

## Recommended Copilot Task Prompt

Use this prompt for GitHub Copilot:

```text
Fix the XYZ tile import pipeline so that downloaded online map tiles are correctly georeferenced and rendered in the RePlot map canvas.

Important requirements:
1. Treat user-entered bounds as EPSG:4326 WGS84 latitude/longitude only for selecting the tile range.
2. Convert WGS84 bounds to XYZ tile x/y range using standard Web Mercator formulas.
3. Download tiles using z/x/y URL template.
4. Stitch downloaded tiles into a mosaic image using tile positions.
5. Compute the mosaic extent in EPSG:3857 Web Mercator meters.
6. Assign the stitched mosaic CRS as EPSG:3857, not EPSG:4326.
7. Use correct GDAL geotransform:
   [minX, resolution, 0, maxY, 0, -resolution]
8. Pass the EPSG:3857 source raster into the existing raster import service so it can be reprojected to the active project CRS.
9. Persist and render the final raster using its real projected bounds.
10. Add validation to prevent excessive tile downloads.

Do not draw XYZ tiles directly as latitude/longitude rectangles. Do not assign EPSG:4326 to the downloaded XYZ mosaic. Ensure tile Y calculation uses the Mercator log/tan/sec formula. Ensure GeoTransform pixel height is negative.
```

---

## Final Development Decision

For the current RePlot application, implement this first:

```text
XYZ tiles -> stitched EPSG:3857 GeoTIFF -> existing raster import/reprojection pipeline -> canvas render
```

After that works, implement this later:

```text
Dynamic online basemap renderer with tile cache
```

This separation will make the software more stable:

- Imported XYZ raster = offline project evidence/background map
- Dynamic online tile layer = live basemap preview

---

## Quick Summary

The map is distorted because XYZ tiles are being handled like normal WGS84 images.

The correct fix is:

```text
Use WGS84 only for input bounds.
Use Web Mercator EPSG:3857 for tile image georeference.
Use GDAL to warp EPSG:3857 into the project CRS.
Use final projected raster bounds for canvas rendering.
```

If this rule is followed, online map tiles will load cleanly and correctly in the RePlot map canvas.
