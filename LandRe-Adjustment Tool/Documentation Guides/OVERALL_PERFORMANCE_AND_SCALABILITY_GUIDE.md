# Overall Performance and Scalability Guide

## Purpose

This guide defines the performance-first architecture direction for RePlot.

This rule applies to the whole application:

- project load and save
- import workflows
- owner and parcel data management
- raster and vector map rendering
- topology and geometry operations
- reporting
- validation
- future replotting workspaces

The application is intended to behave like serious spatial desktop software. Smoothness, correctness, and robustness are product requirements.

---

## 1. Standing Product Rule

For RePlot:

- performance is a first-class architectural requirement
- smooth interaction is more important than visual decoration
- correctness comes before clever optimization
- after correctness, performance takes priority over cosmetic richness

This does not mean low-quality visuals. It means visuals must be designed within a performance-aware architecture.

---

## 2. What High Performance Means in RePlot

High performance for this application means:

- fast project open and close
- responsive forms even with large projects
- smooth zoom, pan, select, and redraw on the map canvas
- predictable performance with many parcels, layers, and canvas objects
- background execution for heavy imports and geometry processing
- no UI freezes during file, database, or large geometry operations
- controlled memory use
- stable behavior without flicker, stalls, or data corruption

---

## 3. Architecture Doctrine

The right approach is not "optimize later".

The correct approach is:

- separate read models from write workflows where helpful
- keep the UI thread thin
- move heavy work into services
- use caching intentionally
- query only the visible or requested data
- render only what is visible
- measure hotspots before deep optimization

### 3.1 Shared orchestration, specialized engines

RePlot should use one coordinated architecture with specialized components:

- shared layer management
- shared project/session management
- shared command/transaction boundaries
- raster-specific processing services
- vector-specific processing services
- topology-specific analysis services
- rendering services that only do rendering

Do not create one giant service or one giant control that does everything.

### 3.2 Read path and write path should be different in spirit

The application writes data relatively infrequently compared with how often it reads and redraws.

That means:

- editing and saving can be more expensive, as long as they remain reliable
- viewing, panning, hit-testing, filtering, and reviewing must be very fast

This is especially important for the map canvas.

---

## 4. Rendering Performance Doctrine

## 4.1 Main rule

The map canvas should render from layer-ordered, already-prepared display data.

It should not:

- open large source files during paint
- run heavy geometry repair during paint
- query unnecessary database data during paint
- allocate large temporary objects per frame

## 4.2 Renderer pipeline

Recommended renderer flow:

1. determine visible extent
2. determine visible layers in display order
3. query only the visible features or tiles for those layers
4. use in-memory caches for decoded or mapped display objects
5. draw raster background layers first
6. draw vector geometry layers next
7. draw labels, selection, and temporary overlays last

## 4.3 Paint event rules

In the paint path:

- avoid file I/O
- avoid large LINQ materialization
- avoid expensive spatial recomputation
- avoid repeated object creation if reusable caches can be used
- avoid rendering invisible layers
- clip work to the visible area

## 4.4 Windows Forms rendering guidance

Official WinForms guidance supports:

- double buffering to reduce flicker
- custom painting through the control paint pipeline

Practical RePlot rule:

- the canvas should be a real custom control with deliberate paint handling
- the control should be double-buffered
- invalidation should be intentional, not constant full-control refresh unless necessary

## 4.5 Image drawing guidance

Microsoft documentation notes that GDI+ may automatically scale images during `DrawImage`, which can hurt performance if you are not explicit about destination sizing.

Practical RePlot rule:

- draw raster tiles with explicit destination rectangles
- avoid hidden scaling behavior where possible
- prefer pre-sized/prepared display tiles over arbitrary giant image draws

---

## 5. Raster Performance Doctrine

For raster support:

- original raster source should remain in project storage
- raster metadata should be persisted in SQLite through EF Core
- display tiles should be cached internally
- only visible tiles should be queried and drawn
- decoded tile images should be reused from memory cache when possible

Do not:

- store one massive raster blob and render it directly every time
- reopen GeoTIFF or large raster sources on every pan
- force raster import logic into the UI

GDAL documentation supports:

- direct raster reads
- geotransform and CRS handling
- overview-aware access
- MBTiles raster support

That makes GDAL the correct raster-processing engine for RePlot.

---

## 6. Vector and Spatial Analysis Performance Doctrine

For vector and topology-heavy work:

- keep authoritative geometry in structured domain entities
- use spatial indexing for viewport and proximity queries
- precompute or cache envelopes when repeatedly accessed
- use prepared geometries for repeated predicate checks

NetTopologySuite provides key building blocks:

- `STRtree` for query-oriented spatial indexing
- prepared geometry support for repeated geometric predicates

Practical RePlot rule:

- use database and/or in-memory spatial filtering to reduce candidate sets first
- only then run heavier exact geometry operations

Never run expensive geometry predicates against the whole project set if a cheaper bounding-box or indexed prefilter can reduce the candidate list first.

---

## 7. Database Performance Doctrine

## 7.1 EF Core

Official EF Core guidance strongly supports:

- indexing frequently filtered columns
- projecting only the fields needed
- limiting result sizes
- avoiding unnecessary tracking
- being careful with related-entity loading

Practical RePlot rules:

- use `AsNoTracking()` for read-only map and list queries
- do not load entire entity graphs for simple left-panel lists
- project only the columns needed for UI summaries
- keep write transactions short and explicit
- review every frequently used query for index support

## 7.2 SQLite

SQLite is a good fit for a portable project database, but architecture must respect its behavior.

Important operational rules:

- SQLite allows many readers but only one writer at a time
- write-heavy workflows should be serialized intentionally
- long-running read transactions should be avoided because they interfere with checkpointing in WAL mode

## 7.3 WAL mode

SQLite official documentation explains that WAL improves concurrency and can make writes fast, but read performance can degrade if the WAL file grows too large, so checkpointing strategy matters.

Practical RePlot rule:

- if WAL mode is used, keep read transactions short
- use deliberate checkpoint strategy for large import sessions
- avoid leaving idle long-lived reads open

Important date-sensitive note:

- SQLite documented a WAL reset bug fixed on March 13, 2026 in SQLite `3.51.3`, with backports in `3.44.6` and `3.50.7`

Before relying on multi-connection WAL-heavy workflows, verify the runtime SQLite version used by the application.

---

## 8. Async and Background Work Doctrine

Official .NET guidance distinguishes CPU-bound and I/O-bound work. RePlot should do the same.

Use background execution for:

- large imports
- tile cache generation
- report exports
- geometry validation batches
- contribution/replot recalculation
- backups

Rules:

- UI event handlers may start async work, but business services should expose real async APIs when doing async I/O
- do not hide `Task.Run` inside libraries as fake async
- CPU-heavy geometry work may use controlled background execution
- always support progress and cancellation for long operations where practical

---

## 9. Caching Doctrine

Caching must be layered, intentional, and disposable.

Recommended cache layers:

- database indexes for lookup speed
- tile cache in SQLite for raster display
- `IMemoryCache` for decoded display objects and repeated short-lived data
- optional in-memory viewport indexes for active sessions

Rules:

- cache derived data, not authoritative business truth
- cache invalidation must be tied to edits/import rebuilds
- do not let caches become hidden second databases

---

## 10. Diagnostics and Measurement Doctrine

Do not guess.

Measure.

Official .NET diagnostics guidance supports:

- `EventCounters`
- `dotnet-counters`
- EventPipe-based tracing tools

Practical RePlot rule:

- add structured logging for long operations
- log timings for imports, large queries, redraws, and major geometry operations
- introduce counters/metrics for high-value operations
- benchmark critical hotspots before major rewrites

Examples of things worth measuring:

- project load time
- visible-layer query time
- raster tile decode time
- paint time
- selection/hit-test time
- import throughput
- save transaction time

---

## 11. Reliability and Performance Together

Performance architecture must never weaken project safety.

So:

- use transactions around multi-entity writes
- use clear error handling and logging
- avoid silent failures
- dispose unmanaged or image resources carefully
- keep long-running operations cancellable where possible

Fast but corrupt is failure.

Smooth but wrong is failure.

The target is:

- correct
- stable
- measurable
- fast enough to feel professional

---

## 12. Practical Technology Direction for RePlot

Recommended technology direction at this stage:

- WinForms custom control for canvas
- EF Core + SQLite for project persistence
- GDAL for raster processing and reprojection
- NetTopologySuite for vector geometry operations and spatial indexing patterns
- `IMemoryCache` for short-lived in-process caches
- `ILogger`-based structured logging
- async application services for I/O-heavy workflows

Potential future upgrade path:

- SkiaSharp if rendering volume eventually exceeds what the current GDI+ path can smoothly support

Do not jump to a renderer rewrite too early. First build the correct layer-driven, cache-aware, measured architecture.

---

## 13. Standing Instruction for Codex

For future prompts, Codex should assume:

- the application is performance-sensitive by design
- performance and smoothness are top-level concerns across the whole system
- map rendering, database access, imports, and geometry operations must all be evaluated with scalability in mind
- architecture decisions should prefer fewer redraws, fewer allocations, fewer unnecessary queries, and shorter blocking operations
- explanations to the user should include what is being done for correctness, what is being done for performance, and why

---

## 14. Official References

- EF Core performance overview: [Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/performance)
- EF Core efficient querying: [Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying)
- EF Core performance diagnosis: [Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/performance/performance-diagnosis)
- WinForms double buffering: [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-reduce-graphics-flicker-with-double-buffering-for-forms-and-controls)
- WinForms custom painting: [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/custom-painting-drawing)
- GDI+ automatic image scaling note: [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-improve-performance-by-avoiding-automatic-scaling)
- .NET EventCounters: [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/event-counters)
- `dotnet-counters`: [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
- SQLite WAL: [SQLite.org](https://www.sqlite.org/wal.html)
- GDAL C# bindings: [GDAL](https://gdal.org/en/stable/api/csharp/index.html)
- GDAL raster C# interface: [GDAL](https://gdal.org/en/stable/api/csharp/csharp_raster.html)
- GDAL raster data model: [GDAL](https://gdal.org/en/stable/user/raster_data_model.html)
- GDAL MBTiles raster driver: [GDAL](https://gdal.org/en/latest/drivers/raster/mbtiles.html)
- NetTopologySuite prepared geometries: [NTS](https://nettopologysuite.github.io/NetTopologySuite/api/NetTopologySuite.Geometries.Prepared.html)
- NetTopologySuite STRtree: [NTS](https://nettopologysuite.github.io/NetTopologySuite/api/NetTopologySuite.Index.Strtree.STRtree-1.html)
