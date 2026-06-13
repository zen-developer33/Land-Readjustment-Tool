# Project Agent Rules - Land Re-Adjustment Tool

These rules are binding for all automated/assistant work in this repository.
Read this file before starting any implementation task.

## 1. Designer-only UI rule (mandatory)

All WinForms static visual layout must live in `*.Designer.cs` files, authored
as the Visual Studio WinForms designer would author it:

- `frmMain` menus, toolstrips, context menus, field declarations, and static layout
  are declared and built in `frmMain.Designer.cs`.
- Every form's controls, panels, sizes, fonts, colors, columns, tabs, and layout are
  created in that form's `*.Designer.cs` inside `InitializeComponent`.
- New forms get both files: `<Form>.Designer.cs` for full static UI and a minimal
  `<Form>.cs` shell with the partial form class and constructor.

## 2. Code-behind is behavior only

`*.cs` files may contain only event wiring, data loading, selection/query logic,
validation, and runtime behavior. Do not create static form layout in code-behind.

## 3. Two-phase delivery

Features are delivered in two ordered phases:

1. UI/UX phase: produce static Designer files and minimal form shells.
2. Wiring and implementation phase: attach handlers and implement behavior in `.cs`.

## 4. Build discipline

Run `dotnet build` on `LandRe-Adjustment Tool.sln` after edits and report the result
before declaring a task done.

## 5. Theming

Reuse `Land_Readjustment_Tool.UI.Forms.RecordFormTheme.Apply(this)` as the last line
of `InitializeComponent` so new forms match the existing record-form look.
