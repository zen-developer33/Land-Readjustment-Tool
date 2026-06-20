# Skia CPU Rendering Implementation Checklist

Branch: `skia-cpu-rendering-implementation`

## Repository setup

- [x] Saved and pushed the previous branch state.
- [x] Created a new implementation branch from that pushed state.
- [x] Verified existing backend abstraction files and current renderer wiring.

## Rendering backend status

- [x] `MapRenderBackend` already supports `GdiPlus`, `SkiaCpu`, and `SkiaGpu`.
- [x] `MapRenderSurfaceFactory` already defaults to GDI+ and can create Skia CPU surfaces on request.
- [x] `MapCanvasRenderer` keeps reusable cached frames for background raster/vector work.
- [x] Skia CPU interactive canvas presentation now uses a dedicated `SKControl` host, not the old app-level WinForms `Graphics` paint/blit bridge.
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
- [x] Added a dedicated `SkiaCpuCanvasPanel` so `SkiaCpu` paints directly through SkiaSharp's CPU WinForms host.
- [x] Kept `GdiPlus` as a selectable backend and default fallback path.
- [x] Routed pan, move preview, zoom, debug overlay, and active-surface invalidation through the direct Skia canvas path.
- [x] Added bounded `SKPaint` caches to `SkiaCanvasMapRenderSurface`, which backs direct Skia CPU/GPU canvas rendering.
- [x] Added direct-canvas paint-cache tests for same-style reuse and quality-separated antialias state.
- [x] Verified `dotnet build "LandRe-Adjustment Tool.sln"` succeeds.
- [x] Verified targeted rendering tests pass.

## Remaining guide phases

- [ ] Finish shape-level legacy `Draw(Graphics, ...)` migration to `IMapRenderSurface` for non-live fallback paths.
- [ ] Replace remaining arc/circle/rectangle/selection `GraphicsPath` helpers in `CanvasVectorRenderer` with direct `IMapPathBuilder` construction.
- [ ] Consider extracting shared Skia surface code only after CPU behavior is stable.
- [ ] Replace remaining GDI-backed cached image sources with native Skia image/cache objects where practical.
- [ ] Run manual GDI+ and Skia CPU visual regression with a real `.lpp` project.
