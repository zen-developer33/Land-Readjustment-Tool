# Application Menu, Toolstrip, Workspace, and Layer Design Guide

## Purpose

This document defines a professional, application-specific menu, toolstrip, shortcut, workspace, and layer-management design for `RePlot`.

It is based on:

- the current `frmMain` menu and toolbar structure
- the current layer-management UI direction
- the current map-canvas and entity-model architecture
- the intended product direction of a land readjustment platform

This document is intentionally opinionated. The goal is not to mirror generic GIS or CAD software. The goal is to give `RePlot` its own strong and practical interaction model.

---

## 1. Core Product UI Principles

The application should follow these interaction principles:

1. The main application window is a project-control and map-review surface, not the primary parcel-editing surface.
2. Parcel editing, replotting, block design, and geometry-changing work should happen in a dedicated workspace with stronger guardrails.
3. Menus should follow real project tasks, not technical implementation modules.
4. Frequently used commands should exist in both menu and toolstrip form.
5. Shortcuts should be memorable, consistent, and safe.
6. Layers should be structured, system-aware, and scenario-aware, not just a flat list of colors.
7. The UI should clearly separate:
   - authoritative project data
   - imported references
   - design proposals
   - temporary work
   - markup and notes

---

## 2. Current Main Window Assessment

The current main menu has a workable foundation:

- `File`
- `Project`
- `Data`
- `Contribution`
- `Replotting`
- `Validation`
- `Output`
- `Tools`
- `Help`

The current top toolstrip also includes:

- project open/save/backup actions
- project information/settings
- undo/redo
- pan/zoom tools

These are useful, but the current structure still has some problems:

1. `Data` is overloaded and mixes import, records, map sources, and ownership access.
2. The current top-level menus do not yet reflect the application's actual operational flow.
3. The main map window still feels too close to an editing surface.
4. Layer handling is present but not yet positioned as a system-wide map organization concept.
5. There is no strong separation between:
   - project overview
   - data management
   - map review
   - block-level replanning

The next iteration should make the application feel like a professional planning workstation.

---

## 3. Recommended High-Level Application Structure

The application should be organized around four major user contexts:

### 3.1 Project Shell

For:

- project creation
- project opening
- project settings
- backup/restore
- import pipeline
- navigation to major work areas

### 3.2 Main Map Workspace

For:

- viewing the full project area
- reviewing original parcels
- selecting parcels, roads, blocks, and layers
- simple read-only inspection
- basic markup or auxiliary drawing

This workspace should **not** be used for serious parcel editing.

### 3.3 Block Replot Workspace

For:

- opening one selected block in a dedicated editing studio
- creating and editing internal roads and replotted parcels
- splitting, reshaping, and allocating parcels
- scenario comparison and validation

This should be the main geometry-editing workspace.

### 3.4 Review and Output Workspace

For:

- issue review
- topology validation
- contribution checking
- report generation
- export and print

---

## 4. Recommended Main Menu Bar

The menu bar should be renamed and regrouped as follows:

1. `File`
2. `Project`
3. `Data`
4. `Map`
5. `Review`
6. `Replot`
7. `Reports`
8. `Tools`
9. `Window`
10. `Help`

This order matches the actual work lifecycle better than the current layout.

---

## 5. Detailed Menu Design

## 5.1 `File`

Purpose:

- document lifecycle
- session lifecycle
- quick entry and exit

Recommended items:

### Group: Project File

- `New Project...` — `Ctrl+N`
- `Open Project...` — `Ctrl+O`
- `Open Recent` — no shortcut
- `Close Project` — `Ctrl+W`

### Group: Save

- `Save` — `Ctrl+S`
- `Save As...` — `Ctrl+Shift+S`
- `Create Snapshot Backup...` — `Ctrl+Alt+S`
- `Restore from Backup...` — no shortcut

### Group: Exchange

- `Import Package...`
- `Export Package...`

### Group: Exit

- `Exit RePlot` — `Alt+F4`

Notes:

- Rename current `Save Project` to `Save`.
- Rename current `Save As Project` to `Save As...`.
- Rename current `Backup Project` to `Create Snapshot Backup...`.

---

## 5.2 `Project`

Purpose:

- project metadata
- settings
- environment configuration

Recommended items:

### Group: Project Definition

- `Project Information...` — `Alt+Enter`
- `Project Settings...` — `Ctrl+,`
- `Workspace Folders...`
- `Coordinate System and Units...`

### Group: Standards

- `Layer Standards...`
- `Parcel Numbering Rules...`
- `Contribution Rules...`
- `Replot Defaults...`

### Group: Administration

- `Project Health Check`
- `Project Log`

Notes:

- Rename current `Project Setting` to `Project Settings...`.
- `Coordinate System and Units...` deserves first-class visibility for this application.

---

## 5.3 `Data`

Purpose:

- all record data and imports
- original data management
- map source import

Recommended items:

### Group: Import Records

- `Import Parcel and Owner Records...` — `Ctrl+Shift+I`
- `Import Validation Review...`
- `Import Session History...`

### Group: Import Spatial Sources

- `Import Cadastral Map...`
- `Import Project Boundary...`
- `Import Block Layout...`
- `Import Roads...`
- `Import Basemap Raster...`
- `Connect Online Basemap...`

### Group: Manage Records

- `Original Parcels...` — `Ctrl+1`
- `Land Owners...` — `Ctrl+2`
- `Malpot References...` — `Ctrl+3`
- `Blocks...` — `Ctrl+4`
- `Roads...` — `Ctrl+5`

### Group: Data Quality

- `Owner Deduplication Review...`
- `Parcel-Link Matching Review...`
- `Missing Geometry Review...`

Notes:

- Rename current `Parcel Ownership Records (Excel/CSV)` to `Import Parcel and Owner Records...`.
- Rename current `View/Edit Record` to `Original Parcels...`.
- Rename current `Land Owner Data` to `Land Owners...`.
- Add `Import Block Layout...` because imported block plans will become important in your workflow.

---

## 5.4 `Map`

Purpose:

- everything related to map viewing, display, selection, layers, and non-destructive inspection

Recommended items:

### Group: Navigation

- `Pan` — `H`
- `Zoom In`
- `Zoom Out`
- `Zoom Window` — `Z`
- `Zoom to Project` — `E`
- `Previous View` — `Alt+Left`
- `Next View` — `Alt+Right`

### Group: Selection and Inquiry

- `Select` — `S`
- `Identify` — `I`
- `Measure Distance` — `D`
- `Measure Area` — `A`
- `Find Parcel...` — `Ctrl+F`
- `Find Owner...` — `Ctrl+Shift+F`

### Group: Display

- `Layer Manager...` — `Ctrl+L`
- `Layer Presets`
- `Labels`
- `Basemap`
- `Transparency Controls`
- `Bookmark Current View`

### Group: Markup

- `Add Note`
- `Add Callout`
- `Draw Reference Line`
- `Draw Reference Polygon`
- `Clear Temporary Markup`

Important rule:

The `Map` menu should not contain parcel-changing tools. It is for navigation, viewing, inspection, and simple annotation.

---

## 5.5 `Review`

Purpose:

- validation
- issue review
- reconciliation

Recommended items:

### Group: Data Review

- `Validation Dashboard...` — `F8`
- `Parcel Record Issues...`
- `Ownership Issues...`
- `Import Exceptions...`

### Group: Spatial Review

- `Topology Check...` — `Ctrl+Shift+T`
- `Missing Links Review...`
- `Overlap and Gap Review...`
- `Block Access Review...`

### Group: Reconciliation

- `Area Reconciliation...`
- `Contribution Review...`
- `Allocation Difference Review...`

Notes:

- Rename current `Validation` top-level concept to `Review`.
- Validation is too narrow; review better matches how users actually work.

---

## 5.6 `Replot`

Purpose:

- launching and managing the dedicated design workspace
- controlling scenario-based replotting

Recommended items:

### Group: Workspace

- `Open Block Replot Workspace...` — `Ctrl+R`
- `Open Last Replot Workspace`
- `Close Current Replot Workspace`

### Group: Scenario

- `New Replot Scenario...`
- `Duplicate Scenario...`
- `Scenario Manager...`
- `Compare Scenarios...`

### Group: Operations

- `Generate Candidate Plots...`
- `Contribution and Returnable Area...`
- `Owner Allocation...`
- `Finalize Block Design...`

### Group: Replot Review

- `Block Replot Issues...`
- `Scenario Metrics...`

Notes:

- Rename current `Start Replot Workspace` to `Open Block Replot Workspace...`.
- `Replot` should be one of the most important menus in the whole system.

---

## 5.7 `Reports`

Purpose:

- outputs, exports, and print products

Recommended items:

### Group: Standard Reports

- `Parcel Register`
- `Owner Register`
- `Contribution Summary`
- `Block Summary`
- `Allocation Summary`

### Group: Map Outputs

- `Project Overview Map`
- `Block Replot Map`
- `Issue Map`
- `Contribution Heatmap`

### Group: Export

- `Export to Excel...`
- `Export to PDF...`
- `Export GIS Data...`
- `Export CAD Data...`

Notes:

- Rename current `Output` to `Reports`.
- `Reports` is more professional and more discoverable.

---

## 5.8 `Tools`

Purpose:

- supporting utilities
- admin tools
- advanced diagnostics

Recommended items:

### Group: Utilities

- `Area Converter` — `Ctrl+Shift+U`
- `Coordinate Converter`
- `Bearing and Distance Calculator`

### Group: Advanced

- `Command Palette...` — `Ctrl+Shift+P`
- `Performance Diagnostics`
- `System Logs`

### Group: Customization

- `Keyboard Shortcuts...`
- `Toolbar Customization...`
- `Display Preferences...`

---

## 5.9 `Window`

Purpose:

- workspace layout and dock panel control

Recommended items:

- `Project Explorer`
- `Layers`
- `Properties`
- `Selection Results`
- `Review Panel`
- `Reset Window Layout`
- `Switch Workspace`

This menu will become more useful once the application grows into multiple workspaces.

---

## 5.10 `Help`

Recommended items:

- `User Guide`
- `Keyboard Shortcuts`
- `Sample Projects`
- `About RePlot`

---

## 6. Recommended Top Toolstrips

The application should not rely on one overloaded toolbar. Use several compact toolstrip groups with clear roles.

## 6.1 Quick Access Toolstrip

Always visible.

Items:

- New
- Open
- Save
- Undo
- Redo
- Command Palette

## 6.2 Project Context Toolstrip

Visible in main shell.

Items:

- Project Information
- Project Settings
- Backup
- Restore
- Current Scenario selector

## 6.3 Map Navigation Toolstrip

Visible in main map workspace.

Items:

- Select
- Pan
- Zoom In
- Zoom Out
- Zoom Window
- Zoom to Project
- Previous View
- Next View
- Find Parcel

## 6.4 Review and Layers Toolstrip

Visible in main map workspace.

Items:

- Layer Manager
- Layer Preset selector
- Labels toggle
- Basemap selector
- Identify
- Measure
- Validation Dashboard

## 6.5 Replot Launch Toolstrip

Visible in main shell when a project is open.

Items:

- Open Block Replot Workspace
- Block selector
- Open Scenario
- Compare Scenarios

## 6.6 Workspace-Specific Toolstrips

The Block Replot Workspace should use its own toolstrips. Do not overload the main window toolbar with parcel-editing tools that belong only inside the replot workspace.

---

## 7. Recommended Keyboard Shortcuts

Shortcuts should be divided into:

- global shortcuts
- main-map shortcuts
- block-replot shortcuts

## 7.1 Global Shortcuts

- `Ctrl+N` — New Project
- `Ctrl+O` — Open Project
- `Ctrl+S` — Save
- `Ctrl+Shift+S` — Save As
- `Ctrl+W` — Close Project
- `Ctrl+Z` — Undo
- `Ctrl+Y` — Redo
- `Ctrl+Shift+P` — Command Palette
- `Ctrl+L` — Layer Manager
- `Ctrl+F` — Find Parcel
- `Ctrl+Shift+F` — Find Owner
- `F8` — Validation Dashboard

## 7.2 Main Map Shortcuts

- `S` — Select
- `H` — Pan
- `Z` — Zoom Window
- `E` — Zoom to Project
- `I` — Identify
- `D` — Measure Distance
- `A` — Measure Area
- `Esc` — Clear selection / cancel active command
- `F7` — Toggle left panel
- `F9` — Toggle right panel

## 7.3 Block Replot Workspace Shortcuts

- `P` — Create Parcel
- `R` — Reshape Parcel
- `X` — Split Parcel
- `M` — Merge Parcels
- `O` — Offset Edge
- `G` — Generate Candidate Plots
- `B` — Switch to Block Boundary tools
- `V` — Validate Current Block
- `Ctrl+Enter` — Commit current operation
- `Backspace` — Remove last vertex
- `Space` — temporary pan while drawing
- `Esc` — cancel current editing command

Rule:

Single-letter shortcuts for parcel editing should only be active in the dedicated replot workspace, not globally.

---

## 8. Recommended Main Map Workspace

This workspace should be the project overview and inspection canvas.

## 8.1 What the main map workspace should allow

- view all project layers
- turn layers on and off
- inspect parcel, owner, block, road, and issue information
- select a block and launch replot workspace
- add temporary markup or reference drawing
- bookmark views
- measure distance and area

## 8.2 What it should not allow

- serious parcel splitting
- block-internal replotted parcel design
- ownership allocation editing
- geometry-finalizing operations
- block-level editing commands that can create accidental project-wide changes

## 8.3 Why this is important

At full project scale, users need confidence and clarity. The main map should stay stable and readable. Serious editing should happen in a more focused environment with:

- smaller spatial scope
- stronger validation
- contextual toolsets
- reduced accidental edits

---

## 9. Recommended Block Replot Workspace

This is the key idea that should define the future of the application.

## 9.1 Concept

When the user selects a block from the main map or block register, they open a dedicated `Block Replot Workspace`.

This workspace is like a design studio for one block and its immediate surroundings.

## 9.2 Purpose

It should support:

- block-focused editing
- road-edge and access planning
- plot generation
- parcel split/reshape/merge
- ownership allocation
- validation and approval for that block

## 9.3 Recommended layout

### Left panel

- block navigator
- scenario selector
- block-specific layers
- operation checklist

### Center canvas

- zoomed block editing canvas
- editable replotted features
- surrounding context shown in subdued style

### Right panel

- selected feature properties
- parcel metrics
- allocation panel
- split parameters
- validation messages

### Bottom panel

- operation log
- issue list
- scenario metrics
- warnings and suggestions

## 9.4 Recommended tool groups inside Block Replot Workspace

### Block Setup

- set active frontage edge
- mark access roads
- set internal constraints

### Geometry Editing

- create parcel
- reshape boundary
- split parcel
- merge parcel
- offset edge

### Plot Generation

- frontage-depth plot creation
- equal width generation
- target area generation
- auto-fill remaining parcel

### Allocation

- assign owner
- split ownership
- compare assigned vs returnable area

### Validation

- topology validation
- area reconciliation
- road access check
- minimum area check

### Finalization

- approve block
- lock design
- publish to main project map

## 9.5 Important behavior

Changes in Block Replot Workspace should not directly become final project geometry without:

- validation
- save/commit
- scenario versioning

This is where scenario-based design becomes very valuable.

---

## 10. Recommended Additional Workspaces

Besides the main map and block replot workspace, the application should eventually support these:

## 10.1 Data Review Workspace

For:

- import review
- deduplication review
- missing-link review
- record correction

## 10.2 Validation Workspace

For:

- topology issues
- contribution anomalies
- allocation mismatches
- unresolved warnings

## 10.3 Print and Output Workspace

For:

- map compositions
- report export setup
- legends, title blocks, and print layout

---

## 11. Recommended Layer Management Strategy

Layers in `RePlot` should not be a flat visual stack only. They should be a structured project organization system.

## 11.1 Layer design principles

1. Some layers are system-controlled and should not be deleted casually.
2. Some layers are imported references and should be treated differently from editable design layers.
3. Layer groups should reflect project meaning, not only geometry type.
4. Visibility presets are as important as individual layer toggles.
5. Main-map and replot-workspace layer behavior should be different.

---

## 12. Recommended Layer Groups

The following group structure is recommended.

## 12.1 System Group: Project Framework

- Project Boundary
- Municipal Boundary
- Ward Boundary
- Control Points
- Grid / Reference

## 12.2 Original Land Data Group

- Original Parcels
- Original Parcel Labels
- Original Parcel Numbers
- Original Owners
- Malpot References
- Original Parcel Issues

## 12.3 Existing Context Group

- Existing Roads
- Existing Buildings
- Existing Utilities
- Water Bodies
- Topography
- Trees / Landmarks

## 12.4 Imported Design Group

- Imported Block Layouts
- Imported Road Layouts
- Imported Survey Drafting
- Imported Reference CAD Layers

## 12.5 Replot Design Group

- Replot Blocks
- Proposed Internal Roads
- Candidate Replotted Parcels
- Approved Replotted Parcels
- Replot Parcel Labels
- Allocation Labels

## 12.6 Review and Analysis Group

- Topology Issues
- Missing Geometry
- Ownership Conflicts
- Contribution Heatmap
- Access Warnings
- Validation Notes

## 12.7 Annotation and Markup Group

- User Notes
- Callouts
- Review Markup
- Meeting Comments

## 12.8 Temporary Work Group

- Temporary Split Lines
- Construction Guides
- Draft Shapes
- Selection Highlights

Important rule:

Temporary layers should be auto-cleanable and visually distinct from authoritative data.

---

## 13. Layer Rules by Workspace

## 13.1 Main Map Workspace Layer Behavior

Allowed:

- visibility toggle
- label toggle
- reorder within safe limits
- transparency adjustments
- simple user markup

Not allowed:

- editing authoritative parcel geometry
- deleting system layers
- accidental movement of core cadastral layers

The main map should emphasize readability and trust.

## 13.2 Block Replot Workspace Layer Behavior

Allowed:

- editing of replot candidate layers
- temporary guide layers
- scenario layers
- block-specific hidden support layers

Special behavior:

- surrounding original parcels should remain visible but dimmed
- outside-block content should be locked by default
- block boundary should always remain visible

---

## 14. Layer Presets

Layer presets are very important for productivity. Instead of manually toggling many layers every time, the system should offer named presets.

Recommended presets:

- `Project Overview`
- `Original Cadastral Review`
- `Owner Review`
- `Block Planning`
- `Road Planning`
- `Replot Design`
- `Allocation Review`
- `Validation Review`
- `Print Preparation`

These presets should become first-class UI features in both menu and toolbar.

---

## 15. Layer Naming and Ownership Rules

Recommended conventions:

- system layers use stable names and codes
- user-created layers live under `User / Markup / Reference`
- imported CAD layers retain source name but are wrapped into a controlled group
- scenario layers include scenario code

Examples:

- `SYS_ProjectBoundary`
- `ORG_Parcels`
- `ORG_ParcelLabels`
- `IMP_BlockLayout_2026A`
- `RPT_ScenarioA_CandidatePlots`
- `REV_TopologyIssues`
- `USR_Markup`

This will make project organization much easier over time.

---

## 16. Recommended Existing Menu Renames

Map current items to these improved names:

- `Project Setting` -> `Project Settings...`
- `Import Data` -> `Import`
- `View/Edit Record` -> `Original Parcels...`
- `Land Owner Data` -> `Land Owners...`
- `Output` -> `Reports`
- `Start Replot Workspace` -> `Open Block Replot Workspace...`
- `Raster Base Map` -> `Basemap Raster`
- `Cadastral Map (DXF/DWG/Shapefile)` -> `Import Cadastral Map...`
- `Project Boundary (DXF/DWG)` -> `Import Project Boundary...`
- `Topographical Map (DXF/DWG)` -> `Import Topographic Reference...`

These names are clearer, more professional, and more consistent with industry desktop software.

---

## 17. Recommended Context Menus

Context menus should reduce top-menu hunting.

## 17.1 Main Map Canvas Right-Click

- Zoom Here
- Identify
- Find Linked Parcel Record
- Find Owner
- Add Note
- Open Block Replot Workspace

## 17.2 Layer Tree Right-Click

- Turn On Only This Group
- Hide Group
- Lock Group
- Set as Active Markup Layer
- Save as Preset
- Layer Properties

## 17.3 Parcel Right-Click in Main Map

- View Parcel Details
- View Owner Details
- Highlight Related Parcels
- Open in Block Replot Workspace

## 17.4 Parcel Right-Click in Block Replot Workspace

- Split Parcel
- Merge Parcel
- Reshape Boundary
- Assign Owner
- Validate Parcel
- View Revision History

---

## 18. Creative Product Ideas Worth Adding

Because this application is unusual, the UI should support a few original ideas that are genuinely useful.

## 18.1 Command Palette

Like professional modern tools, `Ctrl+Shift+P` should open a searchable command palette.

Why this matters:

- users will forget where rare commands live
- the application will grow
- it reduces menu overload

## 18.2 Block Readiness Score

Each block can show a readiness state:

- Data Incomplete
- Review Required
- Ready for Replot
- In Replot
- Validation Failed
- Approved

This makes the product feel more workflow-aware and less like a loose collection of forms.

## 18.3 Scenario Compare Overlay

When viewing a block, users should be able to compare:

- Scenario A
- Scenario B
- Difference layer

This is highly valuable in a land readjustment context and not common in standard GIS tools.

## 18.4 Guided Replot Checklist

Inside Block Replot Workspace, display a checklist:

1. Confirm block boundary
2. Confirm access roads
3. Validate original parcels
4. Calculate contribution
5. Generate candidate plots
6. Allocate owners
7. Validate design
8. Approve block

This can make the application much easier for new users and field teams.

---

## 19. Implementation Recommendations for UI Architecture

To support this design cleanly, the UI should move toward command-driven architecture.

Recommended concepts:

- `CommandDefinition`
- `MenuModelBuilder`
- `WorkspaceContext`
- `LayerPresetService`
- `ShortcutRegistry`
- `FeatureSelectionContext`

Important rule:

Menu enable/disable state should be driven by context such as:

- no project open
- project open
- main map active
- block replot workspace active
- parcel selected
- block selected

This will prevent invalid operations and make the UI feel intentional.

---

## 20. Final Recommended Direction

The strongest direction for `RePlot` is:

1. Keep the main window as the stable project shell and project overview map.
2. Make the main canvas mostly read-only, inspectable, and layer-driven.
3. Move serious geometry editing into a block-focused `Block Replot Workspace`.
4. Use structured layer groups and presets to control complexity.
5. Organize menus around real land readjustment tasks, not just data types.
6. Add workflow-aware features like scenario management, readiness state, validation dashboards, and command search.

If you build the product this way, it will feel less like a generic CAD/GIS hybrid and more like a true land readjustment platform.

