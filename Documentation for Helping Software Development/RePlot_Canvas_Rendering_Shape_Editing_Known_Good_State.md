# RePlot Canvas Rendering and Shape Editing - Known Good State

Date recorded: 2026-06-13

This document records the current stable behavior for the RePlot canvas rendering and shape editing pipeline. Use this as the baseline when future canvas, selection, grip, pan, zoom, or move-tool work is changed.

## Locked Implementation Rule

The shape editing/rendering pipeline is locked.

Do not change the implementation or behavior listed in this document unless the user explicitly says to change this locked pipeline.

For any future prompt that touches selection, grips, snapping, moving, context menus, pan/zoom rendering, cache rendering, project-open canvas rendering, or shape edit previews:

- Read this locked-state document first.
- Check whether the requested change affects any locked behavior.
- If it does, stop and ask the user before changing that area.
- Explain exactly which locked item would be affected.
- Continue only after the user explicitly confirms what to change.

## Current Good Behavior

- Left click is the only object selection action.
- Right click does not change selection. It opens the context menu for the existing selected objects.
- Context-menu move and grip-based move follow the same two-phase move pipeline:
  - choose or derive the reference point,
  - choose the destination point,
  - commit one translation batch.
- Reference grips that start whole-object move behave consistently:
  - polygon/rectangle geometric center,
  - line midpoint,
  - circle center,
  - arc center,
  - ellipse center,
  - text position.
- During move, the non-moving canvas content is treated as cached background.
- During move, the moved shape preview is rendered live as vector on top of the cached background.
- During pan or zoom while a move is active, the background cache follows the viewport and the moved preview remains visible.
- Move preview geometry is not used as snap geometry.
- After a move reference point is chosen, the moving originals and preview are excluded from snap candidates.
- Right-click/context-menu move may still use normal snapping for the first reference point before preview phase begins.
- Selection decorations and grips remain visually consistent with the selected/moving state.
- Project open no longer shows the temporary default empty coordinate grid before the real canvas state is ready.
- Project open coalesces initial raster/vector cache refreshes instead of launching several redundant refreshes.

## Rendering Pipeline Baseline

- Normal idle paint uses cached raster/vector frames when available.
- Pan/zoom use bitmap cache frames for responsive viewport interaction.
- Shape edit and move overlays are live-rendered where correctness matters.
- Selection/grip overlays are not baked permanently into the vector cache; they are drawn as interaction overlays or included only in temporary move/cache snapshots when needed.
- Raster/vector async cache refreshes may run after the first visible canvas frame, but they must not block project opening or cause the empty grid to flash.

## Important Constraints

- Do not make right-click hit testing replace the current selection.
- Do not let transient move preview shapes participate in snapping.
- Do not reintroduce per-frame full vector rendering during pan/zoom/move.
- Do not block project open on every initial cache refresh if a safe first canvas frame can be shown.
- Do not remove the cache-refresh deferral used during project open without replacing it with another coalescing mechanism.

## Locked Items

These items are final unless explicitly unlocked by the user:

- Object selection behavior:
  - Left click selects objects.
  - Right click must not select, deselect, replace, or modify object selection.
  - Right click opens the context menu for the currently selected object set.
- Context-menu move behavior:
  - Context-menu move works on the existing selected objects.
  - Context-menu move uses a two-phase reference/destination workflow.
  - The first reference point may use normal snapping before preview phase begins.
- Grip-based move behavior:
  - Whole-object move from grips must match the context-menu move pipeline.
  - Polygon/rectangle geometric center is a move reference grip.
  - Line midpoint is a move reference grip.
  - Circle center is a move reference grip.
  - Arc center is a move reference grip.
  - Ellipse center is a move reference grip.
  - Text position is a move reference grip.
- Move rendering behavior:
  - When move destination phase starts, non-moving canvas content is bitmap/cache background.
  - The moving shape preview is live vector rendering.
  - The moving preview stays visible during pan and zoom.
  - The moving preview must not be replaced by stale captured preview geometry when live vector rendering is required.
- Snap behavior during move:
  - Move preview geometry is never a snap candidate.
  - Moving originals are excluded from snap candidates after the reference point is chosen.
  - Snap must not interact with or derive candidates from transient preview shapes.
- Selection and grip rendering:
  - Selection decoration and grips remain visually consistent during selection and move operations.
  - Selection/grip overlays are not permanently baked into the normal vector cache.
- Pan/zoom rendering:
  - Pan and zoom use bitmap/cache rendering for responsiveness.
  - Full vector rerendering must not happen per frame during pan/zoom/move.
  - Live overlays are allowed only where interaction correctness requires them.
- Project-open canvas behavior:
  - The temporary empty coordinate grid must not flash during project loading.
  - Initial raster/vector refreshes are coalesced during project open.
  - Project opening must not block unnecessarily on every initial cache refresh.
- Build state:
  - The locked baseline is expected to compile with 0 errors.

## Verification Baseline

The current baseline has been verified with:

```powershell
dotnet build "LandRe-Adjustment Tool\Land_Readjustment_Tool.csproj" --no-restore -p:UseAppHost=false
```

Expected status at the time of recording:

- 0 build errors.
- Existing project warnings are present.
