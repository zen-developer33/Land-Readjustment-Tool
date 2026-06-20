# Skia CPU Rendering Implementation Checklist

Branch: `skia-cpu-rendering-implementation`

## Repository setup

- [x] Saved and pushed the previous branch state.
- [x] Created a new implementation branch from that pushed state.
- [x] Verified existing backend abstraction files and current renderer wiring.

## Rendering backend status

- [x] `MapRenderBackend` already supports `GdiPlus`, `SkiaCpu`, and `SkiaGpu`.
- [x] `MapRenderSurfaceFactory` already defaults to GDI+ and can create Skia CPU surfaces on request.
- [x] `MapCanvasRenderer` already keeps a reusable CPU backing bitmap and creates `SkiaCpuMapRenderSurface` for Skia CPU frames.
- [x] Skia CPU already renders through an off-screen CPU bitmap and blits back into the WinForms `Graphics` target.
- [x] Skia GPU remains isolated from this CPU optimization work.

## Completed in this branch

- [x] Added bounded Skia CPU stroke paint cache.
- [x] Added bounded Skia CPU fill paint cache.
- [x] Added bounded Skia CPU text paint cache.
- [x] Included antialias/render-quality state in paint cache keys.
- [x] Moved cached `SKPaint` disposal to `SkiaCpuMapRenderSurface.Dispose()`.
- [x] Kept one-off image and hatch paints locally scoped.
- [x] Added tests proving repeated stroke styles reuse the same `SKPaint`.
- [x] Added tests proving different render-quality states use different cached paints.
- [x] Migrated live polyline rendering to active-backend `IMapPathBuilder` construction.
- [x] Migrated live donut polygon rendering to active-backend `IMapPathBuilder` construction.
- [x] Added debug-overlay telemetry for `GdiMapPath` fallback conversions on Skia CPU.
- [x] Verified `dotnet build "LandRe-Adjustment Tool.sln"` succeeds.
- [x] Verified targeted `SkiaCpuMapRenderSurfaceTests` pass.

## Remaining guide phases

- [ ] Finish shape-level legacy `Draw(Graphics, ...)` migration to `IMapRenderSurface` for non-live fallback paths.
- [ ] Replace remaining arc/circle/rectangle/selection `GraphicsPath` helpers in `CanvasVectorRenderer` with direct `IMapPathBuilder` construction.
- [ ] Add fallback hit diagnostics for Skia conversion from `GdiMapPath`.
- [ ] Consider extracting shared Skia surface code only after CPU behavior is stable.
- [ ] Run manual GDI+ and Skia CPU visual regression with a real `.lpp` project.
