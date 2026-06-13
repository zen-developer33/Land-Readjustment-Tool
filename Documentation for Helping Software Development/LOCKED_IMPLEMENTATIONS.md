# Locked Implementations Register

This file records implementation areas that the user has marked as locked.

Rule for future work:

- Before changing any implementation, check this register.
- If the requested work touches a locked item, do not change it immediately.
- First tell the user which locked item would be affected.
- Ask for explicit permission or updated instructions.
- Change the locked implementation only if the user clearly confirms the change.

## Locked Items

### RePlot Shape Editing and Canvas Rendering Pipeline

Status: Locked

Reference document:

- `Documentation for Helping Software Development/RePlot_Canvas_Rendering_Shape_Editing_Known_Good_State.md`

Locked scope:

- Object selection behavior.
- Right-click context menu behavior.
- Context-menu move workflow.
- Grip-based move workflow.
- Move reference grips.
- Move preview rendering.
- Move snap exclusions.
- Selection and grip rendering.
- Pan/zoom bitmap cache rendering.
- Project-open canvas loading/rendering behavior.
- Initial raster/vector cache refresh coalescing.

User instruction:

- Do not change this pipeline unless the user specifically says to change the locked pipeline.
- If a future prompt may affect this pipeline, read the reference document first and ask the user before editing.

