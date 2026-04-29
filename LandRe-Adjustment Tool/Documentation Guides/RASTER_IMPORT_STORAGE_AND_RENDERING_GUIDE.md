# Raster Import, Storage, and Rendering Guide

## Purpose

This guide defines the recommended architecture for importing, storing, caching, and rendering raster map sources in RePlot.

Supported examples:

- GeoTIFF
- TIFF
- JPEG / PNG
- MBTiles

The goal is to fit raster support into RePlot's existing project-file, EF Core, SQLite, and map-canvas architecture without turning the canvas into a slow image viewer.

---

## 1. Core Design Decision

For RePlot, the best design is:

- store raster layer metadata in the project database
- keep the original raster source file in the project workspace when practical
- build an internal tiled raster cache for display
- render only the visible tiles in the current map extent

Do not treat a large raster like one big image blob that is loaded and drawn every frame.

---

## 2. Recommended Raster Types

## 2.1 Geo-referenced raster

Examples:

- GeoTIFF
- TIFF with georeferencing
- MBTiles

These have enough information to be placed automatically, or nearly automatically, on the map.

## 2.2 Non-georeferenced raster

Examples:

- JPG
- PNG
- ordinary TIFF

These should be imported as images and then georeferenced by the user if they are to behave as map layers.

---

## 3. Best Storage Strategy for RePlot

## 3.1 Hybrid storage is recommended

Use a hybrid model:

- original file stays in the project folder
- project database stores metadata
- project database stores display cache tiles

Why this is better:

- original file remains available for reprocessing
- the database stays organized
- rendering is fast because cached tiles are already prepared
- reimport and rebuild are possible without losing the source

## 3.2 Do not explode everything blindly

Not all raster types should be handled the same way:

- GeoTIFF/TIFF: import source, read metadata, generate internal tiles
- MBTiles: often keep as source and optionally cache or proxy tiles
- small JPG/PNG: allow direct import, then optional tile-cache generation after georeferencing

---

## 4. Important RePlot-Specific Decision

RePlot is not a web map. It is a project-centered land readjustment application using project CRSs such as UTM or MUTM.

That means raster handling should be project-aware.

Recommended rule:

- on import, interpret the source CRS
- compare it with the project CRS
- if needed, normalize display tiles into the project display CRS

This matters especially for MBTiles, because MBTiles presentation is tied to EPSG:3857.

If your project is working in MUTM or another cadastral/project CRS, you should not rely forever on raw MBTiles display assumptions. For serious usage, build internal cache tiles aligned to the project display system.

---

## 5. Recommended Entity Model

## 5.1 RasterLayer

`RasterLayer` should represent the layer source and metadata.

Suggested fields:

- `Id`
- `Name`
- `LayerType`
- `SourceType` such as GeoTiff, MbTiles, ImageFile
- `SourceFilePath`
- `StorageMode` such as ExternalFile, CachedTiles
- `OriginalWidthPx`
- `OriginalHeightPx`
- `ProjectCoordinateSystemId`
- `SourceCrsWkt`
- `DisplayCrsWkt`
- `MinX`
- `MinY`
- `MaxX`
- `MaxY`
- `PixelSizeX`
- `PixelSizeY`
- `HasGeoreferencing`
- `GeoreferenceMethod`
- `MinZoomLevel`
- `MaxZoomLevel`
- `TileSize`
- `Opacity`
- `IsVisible`
- `IsLocked`
- `CreatedDate`
- `LastModifiedDate`

## 5.2 RasterTile

`RasterTile` should represent cached display tiles.

Suggested fields:

- `Id`
- `RasterLayerId`
- `ZoomLevel`
- `TileX`
- `TileY`
- `MinX`
- `MinY`
- `MaxX`
- `MaxY`
- `ImageFormat`
- `ImageBytes`
- `CreatedDate`

Add a unique index on:

- `RasterLayerId`
- `ZoomLevel`
- `TileX`
- `TileY`

## 5.3 Why separate metadata and tile cache

This separation gives you:

- clean EF Core entities
- faster visible-tile lookup
- easier cache rebuild
- support for both source-file and cached-tile workflows

---

## 6. Recommended Services

## 6.1 RasterImportService

Responsibilities:

- detect source type
- validate file
- open raster source
- extract metadata
- create `RasterLayer`
- trigger cache build if needed

## 6.2 RasterMetadataReader

Responsibilities:

- width and height
- band information
- source CRS
- geotransform
- extent
- pixel size
- alpha/no-data presence

## 6.3 RasterGeoreferencingService

Responsibilities:

- user georeferencing for plain images
- world-file support
- transform metadata into layer extents

## 6.4 RasterTileCacheBuilder

Responsibilities:

- read source pixels
- reproject if needed
- build pyramid levels
- generate 256x256 or 512x512 cache tiles
- save tiles in batches

## 6.5 RasterTileRepository

Responsibilities:

- save and load raster metadata
- fetch visible tiles
- rebuild cache
- clear cache for one layer

## 6.6 RasterRenderService

Responsibilities:

- determine visible zoom level
- query visible tiles
- use memory cache for decoded tiles
- pass decoded images to canvas renderer

---

## 7. Best .NET Library Choices

## 7.1 GeoTIFF / TIFF / raster metadata and reprojection

Best choice: GDAL with C# bindings.

Why:

- industry-standard raster support
- reads GeoTIFF and many raster formats
- supports geotransform, CRS, reprojection, resampling, and MBTiles driver support

Use GDAL for:

- metadata reading
- geotransform and CRS inspection
- reprojection into project CRS when needed
- overview-aware raster reads
- MBTiles raster access when MBTiles is used as a source

## 7.2 Database persistence

Best choice: EF Core with SQLite.

Use EF Core + SQLite for:

- raster layer metadata
- cached tile index and tile blobs
- transaction-safe batch writes
- future project-level filtering and layer management

Important note:

- use EF Core for your application model and tile-cache persistence
- do not force EF Core to become the raster-processing engine
- heavy raster pixel work should stay in GDAL-oriented services

## 7.3 In-memory tile cache

Best choice: `IMemoryCache` from `Microsoft.Extensions.Caching.Memory`.

Use it for:

- decoded tile image reuse during pan and zoom
- short-lived display caching
- avoiding repeated byte-to-image decoding on every paint

## 7.4 Rendering engine

Short-term best choice for RePlot now:

- keep using WinForms + GDI+ because your canvas is already built this way

Future upgrade path if raster + vector load grows significantly:

- evaluate `SkiaSharp` for faster and more scalable rendering

Do not switch rendering engines before raster architecture is stable. First get the storage, CRS, cache, and visible-tile workflow correct.

## 7.5 Recommended package set

Recommended packages for the first implementation:

- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.Options`
- `Microsoft.Extensions.Caching.Memory`
- GDAL C# bindings package or a controlled local GDAL runtime strategy

Optional later:

- `SkiaSharp`
- `SkiaSharp.Views.WindowsForms`

---

## 8. Recommended Solution Architecture

Use clear separation between domain, application, infrastructure, and UI responsibilities.

## 8.1 Domain layer

Keep simple entities here:

- `RasterLayer`
- `RasterTile`
- `RasterImportJob` if import tracking is added later

This layer should not know about GDAL or WinForms.

## 8.2 Application layer

Application services coordinate the use case:

- import raster
- georeference plain image
- build cache
- refresh layer display

Recommended interfaces:

- `IRasterImportService`
- `IRasterMetadataReader`
- `IRasterGeoreferencingService`
- `IRasterTileCacheBuilder`
- `IRasterTileQueryService`
- `IRasterRenderService`

## 8.3 Infrastructure layer

This layer contains:

- GDAL-based readers and processors
- EF Core repositories
- file-system project storage helpers
- tile encoding helpers

## 8.4 UI layer

This layer contains:

- import dialog
- georeferencing dialog
- layer manager integration
- map canvas renderer calls

The UI should ask services to do work. The UI should not read GeoTIFF pixels itself.

---

## 9. Dependency Injection Pattern

Register raster services through DI just like other application services.

Suggested composition:

- `IRasterImportService -> RasterImportService`
- `IRasterMetadataReader -> GdalRasterMetadataReader`
- `IRasterGeoreferencingService -> RasterGeoreferencingService`
- `IRasterTileCacheBuilder -> GdalRasterTileCacheBuilder`
- `IRasterTileQueryService -> RasterTileQueryService`
- `IRasterRenderService -> RasterRenderService`

Also inject:

- `AppDbContext` or a factory
- `ILogger<T>`
- `IMemoryCache`
- project path/configuration service

Important rule:

- use `IDbContextFactory<AppDbContext>` or short-lived contexts for long-running raster import jobs
- do not keep one UI-owned DbContext alive for the whole session

---

## 10. Recommended Import Workflow

## 10.1 Step 1: Create layer record

Create a `RasterLayer` record first with status such as:

- `Pending`
- `Importing`
- `Ready`
- `Error`

## 10.2 Step 2: Read source metadata

Read:

- width and height
- band count
- CRS
- geotransform
- extent
- nodata
- overview information if available

## 10.3 Step 3: Decide georeferencing path

If source has georeferencing:

- validate CRS against project CRS
- choose direct display or reprojection-to-cache flow

If source has no georeferencing:

- treat it as an image layer
- allow user to place by extent, world file, or control points

## 10.4 Step 4: Build internal display cache

Generate cache tiles in the project display CRS.

Recommended behavior:

- build base zoom tiles
- optionally build overview pyramid levels
- store each tile as PNG or JPEG depending on transparency needs

## 10.5 Step 5: Persist and finalize

Save:

- layer metadata
- tile metadata and bytes
- import timestamps
- failure reason if import fails

## 10.6 Step 6: Refresh map display

After import completes:

- add layer to layer manager
- trigger redraw
- allow zoom to extent

---

## 11. Rendering Workflow for Map Canvas

The canvas should follow this order:

1. determine current world extent
2. choose display zoom level
3. query intersecting raster tiles only
4. load decoded images from memory cache if present
5. decode missing tiles from database bytes
6. draw tiles in world-aligned screen rectangles
7. draw vector overlays above raster

Recommended rule:

- rasters draw first
- vector parcel boundaries, labels, selection, and markup draw on top

---

## 12. Performance Rules

These rules matter a lot for your application:

- never reopen and resample the original raster file on every paint
- never decode every tile on every paint
- never load the whole raster image into memory if only a small extent is visible
- batch database writes during cache generation
- use async import workflows so the UI stays responsive
- keep rendering read-only and fast on the main canvas
- move heavier editing and analysis work to dedicated services/workspaces

Recommended practical choices:

- tile size: start with `512 x 512` for desktop testing
- fall back to `256 x 256` if memory or overlap behavior is better in practice
- use `AsNoTracking()` for visible-tile queries
- keep an LRU-style memory cache for decoded images
- dispose `Image`, `Bitmap`, GDAL `Dataset`, and unmanaged wrappers carefully

---

## 13. RePlot-Specific Recommendation

For RePlot, the main canvas should treat raster layers as contextual background and reference material.

Use raster layers for:

- cadastral scan reference
- orthophoto context
- block planning reference
- alignment and review

Do not use raster layers as the place where parcel topology truth lives.

Your parcel truth should remain in structured geometry/domain objects. Raster layers support understanding, tracing, and review.

---

## 14. Layer Manager Integration Direction

Raster support should not bypass the layer system.

Recommended rule:

- raster must participate in the same layer-management architecture as vector data
- visibility, ordering, locking, selectability policy, naming, and tree placement should be layer-driven
- the left panel layer tree should show raster in a dedicated `Raster` group or branch

Recommended practical model:

- one top-level raster group in the tree
- one child layer entry per imported raster source

This is better than placing all imported rasters into one undifferentiated flat layer because users will need:

- per-raster visibility
- per-raster opacity
- per-raster draw order
- per-raster extent zoom
- per-raster source metadata
- future replace/rebuild/remove behavior

So the best architecture is:

- shared layer system for all map content
- raster-specific data model and renderer under that shared system
- tree grouping by content family, not a completely separate UI path for raster
- band reading
- raster reprojection
- cache generation

## 7.2 MBTiles

Best practical choice for RePlot:

- use GDAL MBTiles driver for unified raster handling
- or read raw SQLite MBTiles directly only if you want a lightweight MBTiles-only path

For RePlot, unified GDAL-based handling is cleaner because the app will already need GDAL for GeoTIFF/TIFF.

Important note:

MBTiles is a tiled format, not raw raster storage. It is presentation-oriented and tied to Web Mercator for display.

## 7.3 EF Core and SQLite

Best choice:

- `Microsoft.EntityFrameworkCore.Sqlite`

Use EF Core for:

- metadata entities
- layer configuration
- normal tile cache queries

For very large tile-ingest batches, consider using:

- EF Core transaction boundary
- batched inserts
- or direct `Microsoft.Data.Sqlite` commands inside a controlled repository for the heavy write path

This is because inserting huge numbers of tiles one entity at a time through change tracking can become slow.

## 7.4 Canvas rendering library

For RePlot specifically, recommended path is phased:

- short term: use the current WinForms/GDI-based canvas architecture and integrate raster tile rendering into it
- medium term: move the canvas renderer to SkiaSharp if vector + raster rendering becomes heavy

Why this is the best fit:

- it avoids rewriting your canvas immediately
- it lets you add raster support sooner
- it still leaves a path to higher-performance rendering later

## 7.5 Image decoding

For decoded cached tile display on the current WinForms canvas:

- `System.Drawing` is acceptable because the application is Windows-only

If later moving to SkiaSharp:

- decode and draw with SkiaSharp to reduce conversions

---

## 8. Recommended Workflow

## 8.1 Import workflow

1. User chooses `Import Raster Map`.
2. System detects source type.
3. Metadata is read.
4. If no georeferencing exists:
   - ask user to georeference
5. If source CRS differs from project CRS:
   - prompt whether to build project-aligned cache
6. Save `RasterLayer` metadata.
7. Build internal tile pyramid cache.
8. Add raster layer to canvas layer manager.
9. Render visible tiles only.

## 8.2 Non-georeferenced image workflow

1. User imports JPG/PNG/TIFF.
2. System detects no CRS/geotransform.
3. User enters:
   - project CRS
   - top-left / bottom-right coordinates
   - or two/four control points
4. System stores georeference metadata.
5. Cache tiles are built.

## 8.3 MBTiles workflow

1. User imports MBTiles.
2. System reads metadata.
3. System recognizes Web Mercator display assumptions.
4. If project CRS is not compatible for direct display:
   - build project-aligned cache tiles
5. Save metadata and layer config.

---

## 9. Rendering Workflow

## 9.1 Rendering rule

At paint time:

1. determine current visible world extent
2. determine appropriate zoom level
3. query only intersecting raster tiles
4. decode from memory cache if available
5. otherwise decode from stored tile bytes
6. draw in world-to-screen transformed rectangles

## 9.2 Important performance rule

Do not:

- load all tiles for a raster layer
- decode all tile images every frame
- load the original GeoTIFF on every paint

Do:

- query visible tiles only
- use in-memory LRU cache for decoded images
- invalidate only affected screen regions when possible

---

## 10. Performance Strategy

## 10.1 Tile size

Recommended tile size:

- `256x256` for compatibility
- `512x512` if you want fewer tile rows and less draw-call overhead

For desktop GIS-like usage, `512x512` is often a good default if memory handling is controlled.

## 10.2 Pyramid levels

Build zoom levels ahead of time.

This avoids expensive resampling during every paint event.

## 10.3 Memory cache

Use an in-memory cache for decoded visible tiles.

Good candidates:

- `MemoryCache`
- custom LRU dictionary

Cache key example:

- `RasterLayerId + ZoomLevel + TileX + TileY`

## 10.4 Background import

Tile generation should run asynchronously with progress reporting.

Do not freeze the UI while generating many tiles.

## 10.5 Batched persistence

For tile cache writes:

- write in batches
- wrap in transaction
- avoid one `SaveChanges()` per tile

---

## 11. Recommended RePlot Integration

Raster layers should integrate into the existing map layer system.

Recommended additions:

- new `CanvasLayer.LayerType` values such as `RasterBasemap`, `RasterReference`, `Orthophoto`
- raster layers should be visible in layer manager
- raster layers should support opacity and visibility
- raster layers should be non-selectable by default
- raster layers should be rendered below parcels, roads, labels, and editing overlays

Recommended draw order:

1. raster basemap
2. reference raster
3. project boundary and reference vectors
4. original parcels
5. roads and blocks
6. replotted parcels
7. labels
8. selection and review overlays

---

## 12. Final Recommendation

For RePlot, the best industry-style implementation is:

- GDAL for raster ingestion and reprojection
- EF Core + SQLite for raster metadata and internal cached tiles
- project-folder source-file retention
- project-CRS-aware cache generation
- visible-tile-only rendering on the existing WinForms canvas
- optional future SkiaSharp renderer upgrade if canvas load becomes heavy

The beginner-friendly mental model is:

source file -> metadata -> optional georeferencing -> tile cache -> visible tile query -> draw on canvas

That is the right long-term shape for a professional GIS-like land readjustment application.

---

## 13. Current Implementation Scope

The current implementation phase is raster-only.

Do now:

- import GDAL-readable raster sources such as GeoTIFF, TIFF, VRT, IMG, JPG, PNG, BMP, and MBTiles
- keep the import workflow behind services, interfaces, dependency injection, logging, and error handling
- show a minimal raster review form after file selection
- show source metadata, project CRS, and source CRS definition before import
- default missing source CRS to WGS 1984 (`EPSG:4326`)
- transform georeferenced rasters into the project CRS when a source CRS is available or defined

Do later:

- AutoCAD, DXF, DWG, and vector external-layer projection workflows
- full control-point georeferencing for plain images with no map coordinates
- internal raster tile pyramid caching and visible-tile database rendering
