# RePlotting Workspace UI, UX, and Implementation Guide

## Purpose

The replotting workspace is the primary production surface of RePlot. If the application does only one major job, it should let an operator replot land parcels efficiently, safely, and defensibly inside a selected block.

This workspace should be opened from `frmMain` through:

- `Replotting > Open Replotting Workspace...`
- later rename target: `Replot > Open Block Replot Workspace...`

The current `frmReplotWorkspace` is a maximized form containing only `MapCanvasControl`. This guide defines the target UI, interaction model, and implementation architecture for turning that form into a block-focused replotting studio.

Reference mockup:

- `artifacts/replot-workspace-ui-concept.png`

---

## Product Role

The main map workspace is for project overview, data inspection, layer review, and launching focused workflows.

The replotting workspace is for actual block-level geometry work:

- drawing new replotted parcels
- editing parcel boundaries
- splitting parcels
- joining parcels
- copying and moving parcel geometry
- assigning one or more original parcels to replotted parcels
- generating candidate plots from the finalized contribution table
- validating topology, area balance, road access, and ownership allocation
- approving or committing a block design

The workspace must feel like a precise CAD/GIS editing surface, but with land-readjustment workflow guardrails.

---

## Non-Negotiable UX Rules

1. Only one block is active in the replotting workspace at a time.
2. The canvas extent is intentionally limited to the selected block plus immediate road context.
3. Zoom in must support high precision work down to 4 decimal places in coordinate/scale feedback.
4. Zoom out must be limited so the active block is never displayed smaller than `50 x 50` pixels.
5. Outside-block context must be visible only when it helps replotting. It should be locked and subdued.
6. Road centerlines should render only around intersections related to the selected block.
7. Cadastral parcels should be queried only when they intersect the selected block and the relevant road centerlines.
8. Existing buildings and context layers are reference layers by default, not editable replot layers.
9. All destructive or legally meaningful operations must preview before commit.
10. Automatic replotting must produce editable candidate parcels, not final approved parcels.
11. Finalization requires validation and explicit commit.

---

## Visual Direction

Match the existing application theme:

- classic Windows Forms desktop application
- `Segoe UI` at compact sizes
- light gray form background
- white control surfaces
- pale blue selection highlights
- slate borders
- dark navy text
- dense but readable toolstrips and panels
- `SplitContainer`, `GroupBox`, `TabControl`, `TreeView`, `DataGridView`, `StatusStrip`, `ToolStrip`, `ComboBox`, `NumericUpDown`, and `CheckBox`

Avoid:

- web-app landing page composition
- oversized hero sections
- decorative cards
- glossy/futuristic controls
- dark theme as the default
- large rounded UI blocks
- decorative gradients or blobs

---

## Target Form Structure

Recommended top-level layout for `frmReplotWorkspace`:

```text
frmReplotWorkspace
  MenuStrip
  Quick ToolStrip
  Replot ToolStrip
  Main SplitContainer
    Left Dock Panel
    Center/Right SplitContainer
      Canvas Host
      Right Inspector Panel
  Bottom Panel
  StatusStrip
```

Recommended starting size:

- Maximized by default
- Minimum size: `1280 x 760`
- Left panel: `300 px`
- Right panel: `360 px`
- Bottom panel: `150-180 px`
- Center canvas gets all remaining space

---

## Menu Bar

Use the same high-level menu language as `frmMain`:

- File
- Project
- Data
- Map
- Review
- Replot
- Reports
- Tools
- Window
- Help

The replot workspace can show a reduced set of commands, but menu placement should remain consistent with the main application.

Key `Replot` menu items:

- Select Block...
- Open Scenario...
- Save Scenario
- Generate Candidate Plots...
- Auto Replot Current Block...
- Validate Current Block
- Commit Block Design...
- Lock Approved Design
- Close Replot Workspace

---

## Quick ToolStrip

Always visible:

- New
- Open
- Save
- Undo
- Redo
- Select
- Pan
- Zoom Window
- Zoom Extents
- Active Block selector
- Scenario selector
- Validate Block
- Commit Design

Example controls:

```text
[New] [Open] [Save] | [Undo] [Redo] | [Select] [Pan] [Zoom Window] [Zoom Extents]
Block: [B-07 v] Scenario: [Scenario A - Working v] [Validate Block] [Commit Design]
```

---

## Replot ToolStrip

This is the main editing toolbar. Keep it compact and icon-led.

Tool groups:

### Geometry

- Create Parcel
- Draw Polygon
- Reshape Edge
- Add Vertex
- Delete Vertex
- Move
- Copy
- Rotate, optional later

### Parcel Operations

- Split Parcel
- Merge Parcels
- Offset Edge
- Trim/Extend, optional later
- Assign Original Parcel
- Assign Owner

### Generation

- Generate Plots
- Auto Replot
- Frontage/Depth
- Target Area
- Equal Widths
- Equal Areas

### Editing Aids

- Snap
- Ortho
- Construction Guide
- Measure
- Validate Selected

Single-letter shortcuts should be active only inside this workspace:

| Shortcut | Command |
|---|---|
| P | Create Parcel |
| R | Reshape Parcel |
| X | Split Parcel |
| M | Merge Parcels |
| O | Offset Edge |
| G | Generate Candidate Plots |
| V | Validate Current Block |
| Ctrl+Enter | Commit current operation |
| Backspace | Remove last vertex |
| Space | Temporary pan |
| Esc | Cancel command |

---

## Left Panel

The left panel should help the operator choose context and control visibility.

### Block Navigator

Fields:

- Project
- Active Block
- Block code
- Land use
- Block status
- Readiness status
- Active scenario
- Last validation result

Actions:

- Select Block
- Zoom to Block
- Open Contribution Table
- Refresh Context

### Replot Layers

Use a checkbox `TreeView`.

Recommended layer groups:

- Block Boundary
- Road Centerlines
- Intersecting Cadastral Parcels
- Existing Buildings
- Proposed Internal Roads
- Candidate Replotted Parcels
- Approved Replotted Parcels
- Allocation Labels
- Topology Issues
- Temporary Guides

Layer row behavior:

- checkbox toggles visibility
- lock icon shows whether editable
- gray text means reference-only
- bold text means active editing layer
- right-click menu supports isolate, lock, properties, and save preset

### Workflow Checklist

Checklist:

1. Confirm block boundary
2. Confirm access roads
3. Validate original parcels
4. Load finalized contribution data
5. Generate or draw candidate plots
6. Allocate owners
7. Validate design
8. Approve block

This checklist should reflect actual state, not static instructions.

---

## Center Canvas

The canvas is the working heart of the form.

Visible content:

- active block boundary in strong blue
- related road centerlines clipped to the block and nearby intersections
- intersecting original cadastral parcels in thin gray
- existing buildings in muted reference style
- proposed internal roads in orange
- candidate replotted parcels in clean colored polygons
- approved replotted parcels in stronger final style
- labels for parcel number, area, owner/allocation status
- selected parcel grips, vertex handles, midpoint handles, snap marker, and split preview geometry

Canvas overlays:

- block name
- scenario name
- active tool
- scale
- zoom precision
- snap/ortho state
- warning count

Zoom behavior:

- prevent zoom out when selected block would render below `50 x 50` pixels
- maintain high precision status display during zoom in
- use block-focused extents, not project extents
- support zoom window within active block context
- avoid loading project-wide features into this workspace

Selection behavior:

- selection highlight should be overlay-only
- locked context features can be inspected but not moved
- edit handles appear only for editable candidate/replotted parcels
- multi-select supports move, merge, owner assignment, and validation

---

## Right Panel

Use tabs so the operator can stay in one workspace without opening many dialogs.

Recommended tabs:

- Properties
- Allocation
- Split/Generate
- Validation

### Properties Tab

Selected feature fields:

- Replot parcel number
- Plot type
- Design area
- Calculated area
- Original parcel links
- Owner or ownership group
- Road access
- Frontage
- Allocation status
- Geometry status
- Notes

Actions:

- Assign Original Parcel
- Assign Owner
- Preview Split
- Apply Change
- View Revision History

### Allocation Tab

Show:

- returnable area
- assigned area
- difference
- contribution deductions
- linked original parcel or parcels
- ownership shares
- allocation conflicts

Actions:

- Add Original Parcel Link
- Remove Link
- Auto Match Owner
- Balance Area
- Mark Joint Case

### Split/Generate Tab

Split methods:

- Line Cut
- Corridor Cut
- Target Area
- Frontage/Depth
- Equal Widths
- Equal Areas
- Percentage
- Radial
- Vertex-to-Vertex
- Block Template
- Buffer Deduction

Generation inputs:

- plot type
- target area
- minimum frontage
- depth
- road edge/frontage edge
- corner plot rule
- remaining area handling

Actions:

- Preview
- Apply
- Generate Candidate Set
- Auto Fill Remaining Area

### Validation Tab

Show:

- topology state
- overlap/gap issues
- sliver warnings
- area tolerance
- road access
- minimum frontage
- minimum area
- allocation balance
- unresolved owner links

Actions:

- Validate Selected
- Validate Block
- Show Issue Geometry
- Mark Resolved

---

## Bottom Panel

Use a `TabControl`.

Recommended tabs:

- Operation Log
- Issues
- Scenario Metrics
- Land Contribution Table

### Issues Tab

Use `DataGridView` columns:

- Severity
- Feature
- Message
- Action
- Status

Example issues:

- Gap under tolerance
- Owner allocation pending
- Road frontage confirmed
- Area difference outside tolerance
- Parcel has no original parcel link

### Scenario Metrics Tab

Show:

- block area
- developable area
- road deduction area
- candidate parcel count
- approved parcel count
- unallocated owner count
- total returnable area
- total assigned area
- area difference

---

## Status Strip

Show concise state:

```text
Project: Kamalpokhari LR | Block: B-07 | Scenario: A Working | Tool: Split Parcel Preview |
X: 321456.1234 Y: 3076542.9876 | Scale: 1:500 | Snap: On | Ortho: On | Zoom: 0.0001
```

---

## Data Query Rules

The replot workspace should not load the whole project map.

For active block `B`:

1. Load the block polygon.
2. Load road centerlines that intersect or touch the block, plus only the short nearby segments needed to show intersections.
3. Load cadastral parcels where geometry intersects:
   - active block polygon, or
   - related road centerline buffer, if needed for frontage/access review.
4. Load existing buildings intersecting the active block or immediate context envelope.
5. Load candidate and approved replotted parcels for the active block and active scenario.
6. Load topology issues for visible active block features.

Recommended service:

```csharp
public interface IReplotWorkspaceQueryService
{
    Task<ReplotWorkspaceContext> LoadContextAsync(
        int blockId,
        int scenarioId,
        CancellationToken cancellationToken = default);
}
```

---

## Core Application Services

Recommended services:

```csharp
public interface IReplotWorkspaceService
{
    Task<ReplotWorkspaceContext> OpenBlockAsync(int blockId, int scenarioId, CancellationToken ct = default);
    Task SaveScenarioAsync(ReplotWorkspaceSaveRequest request, CancellationToken ct = default);
    Task<ReplotValidationResult> ValidateBlockAsync(int blockId, int scenarioId, CancellationToken ct = default);
    Task CommitBlockDesignAsync(int blockId, int scenarioId, CancellationToken ct = default);
}

public interface IReplotParcelEditingService
{
    Task<ReplotEditPreview> PreviewSplitAsync(ParcelSplitRequest request, CancellationToken ct = default);
    Task<ReplotEditResult> ExecuteSplitAsync(ParcelSplitRequest request, CancellationToken ct = default);
    Task<ReplotEditResult> MergeParcelsAsync(MergeReplotParcelsRequest request, CancellationToken ct = default);
    Task<ReplotEditResult> UpdateParcelGeometryAsync(UpdateReplotParcelGeometryRequest request, CancellationToken ct = default);
}

public interface IReplotGenerationService
{
    Task<CandidatePlotGenerationPreview> PreviewGenerateAsync(CandidatePlotGenerationRequest request, CancellationToken ct = default);
    Task<CandidatePlotGenerationResult> GenerateAsync(CandidatePlotGenerationRequest request, CancellationToken ct = default);
}

public interface IReplotAllocationService
{
    Task<AllocationPreview> PreviewAssignmentAsync(AllocationRequest request, CancellationToken ct = default);
    Task<AllocationResult> AssignOriginalParcelsAsync(AllocationRequest request, CancellationToken ct = default);
}
```

---

## Canvas Integration

The existing `MapCanvasControl` already supports:

- vector feature rendering
- layer-backed canvas features
- selection
- snapping
- ortho mode
- drawing tools
- move operations
- selection grips
- async vector rendering
- status events

The replot workspace should wrap and specialize this control rather than replacing it.

Recommended approach:

1. Keep `MapCanvasControl` as the base drawing/rendering control.
2. Add replot-specific tools as command modes, not as unrelated UI logic.
3. Add a workspace-level command service that converts tool gestures into domain commands.
4. Persist edits through services that update `CanvasObject`, `ReplottedParcel`, `OriginalToReplottedMap`, and related entities.
5. Keep temporary preview geometry in memory until the user commits.

---

## frmMain Access

Current code already has:

- `_replotWorkspaceForm`
- `startReplotWorkspaceToolStripMenuItem_Click`
- `CloseReplotWorkspace`
- `ReplotWorkspaceForm_FormClosed`

Target behavior:

1. If a block is selected in the main map, pass that block to `frmReplotWorkspace`.
2. If no block is selected, show a block selector dialog.
3. Reuse an existing open replot form if the same block/scenario is open.
4. Ask before switching blocks if the current workspace has unsaved edits.
5. Keep the workspace maximized and focused.

Constructor target:

```csharp
public frmReplotWorkspace(
    IReplotWorkspaceService workspaceService,
    IReplotParcelEditingService parcelEditingService,
    IReplotGenerationService generationService,
    IReplotAllocationService allocationService,
    int? initialBlockId = null,
    int? initialScenarioId = null)
```

Short-term constructor is allowed while DI is being introduced:

```csharp
public frmReplotWorkspace(ProjectSession projectSession, int? initialBlockId = null)
```

---

## Implementation Phases

### Phase 1: Workspace Shell

- Replace the canvas-only designer with the full docked layout.
- Apply `RecordFormTheme`.
- Add menu strip, quick toolstrip, replot toolstrip, left panel, right panel, bottom panel, status strip.
- Keep controls wired with placeholder data.
- Ensure form opens from `frmMain`.

### Phase 2: Block Context Loading

- Add block selector.
- Load only selected block context.
- Build replot-specific layer tree.
- Enforce one active block per workspace.
- Add zoom-out constraint so the active block never renders below `50 x 50` pixels.

### Phase 3: Manual Parcel Editing

- Create parcel from polygon.
- Reshape edge with vertex grips.
- Move/copy parcels.
- Split by line and vertex-to-vertex.
- Merge selected candidate parcels.
- Add operation previews and cancel/commit behavior.

### Phase 4: Contribution-Based Generation

- Read finalized land contribution table.
- Generate candidate parcels by frontage/depth, target area, equal width, equal area, and block template.
- Store generated parcels as candidate scenario geometry.
- Show assigned vs returnable area differences.

### Phase 5: Allocation

- Assign one or many original parcels to one replotted parcel.
- Support many original parcels to many replotted parcels.
- Support joint ownership and sales plot cases.
- Show allocation conflicts and area difference.

### Phase 6: Validation and Commit

- Validate topology.
- Validate parcel area tolerance.
- Validate road access and frontage.
- Validate owner allocation.
- Validate numbering.
- Commit approved geometry to final scenario.
- Lock approved parcels.

---

## Image Generation Prompt Used

Use this prompt to regenerate or iterate the UI mockup:

```text
Use case: ui-mockup
Asset type: high-fidelity Windows Forms classic desktop application UI concept image
Primary request: Generate a realistic, practical UI mockup for a classic WinForms desktop land readjustment application named RePlot, showing the dedicated Block Replot Workspace. This workspace is the heart of the software and is focused on parcel replotting inside one selected block.
Style/medium: crisp desktop software screenshot mockup, classic Windows Forms, professional civil/GIS/CAD planning workstation, dense but organized operational UI, Segoe UI style, no web landing page, no mobile UI.
Composition/framing: 16:9 landscape full application window, maximized desktop app. Include a top menu bar and compact toolstrips, a left dock panel, central canvas, right dock panel, bottom dock panel, and status strip. The center canvas should occupy the largest area.
Visual theme: match a restrained WinForms theme: light gray application background (#f3f4f6), white controls, pale blue selection highlights, slate borders, dark navy text, compact buttons, tabs, splitters, DataGridViews, TreeView layer list, status bar. Avoid glossy modern web style, avoid dark theme, avoid rounded card-heavy design.
Top menu text: File, Project, Data, Map, Review, Replot, Reports, Tools, Window, Help.
Top toolbar: small icon buttons for New, Open, Save, Undo, Redo, Select, Pan, Zoom Window, Zoom Extents. Include a Block selector combo box reading "Block B-07" and a Scenario selector reading "Scenario A - Working". Include a prominent but compact button "Validate Block" and "Commit Design".
Second replot toolstrip: grouped compact CAD/GIS tools with icons and short labels: Create Parcel, Draw Polygon, Reshape Edge, Add Vertex, Delete Vertex, Split, Merge, Offset Edge, Move, Copy, Assign Owner, Generate Plots, Auto Replot, Snap, Ortho. Use classic ToolStrip style, not ribbon.
Left panel: width about 300px. Top area titled "Block Navigator" with fields Project: LR-2026, Active Block: B-07, Status: In Replot, Readiness: Ready. Below it a TreeView titled "Replot Layers" with checkboxes and nested groups: Block Boundary, Road Centerlines, Intersecting Cadastral Parcels, Existing Buildings, Proposed Internal Roads, Candidate Replotted Parcels, Approved Replotted Parcels, Allocation Labels, Topology Issues, Temporary Guides. Include visible/locked icons. Bottom left area titled "Workflow" with a checklist: Boundary Confirmed, Roads Confirmed, Contribution Loaded, Candidate Plots, Owner Allocation, Validation, Approval.
Center canvas: large white/off-white map canvas with fine grid, one block only visible. Show a block polygon boundary in strong blue, surrounding road centerlines clipped only around the block, original cadastral parcels intersecting the block in thin gray lines, existing buildings as small muted rectangles, proposed internal roads in orange, candidate replotted parcels as clean colored polygon lots with parcel IDs such as R-07-001, R-07-002, R-07-003. One parcel selected with blue outline, vertex grips, reshape handles, snap marker, and a split preview line. Show subtle labels for area values like 125.40 sqm. Include a small canvas overlay at top-left: scale 1:500, zoom 4-digit precision, min block display 50x50 px. Include no basemap imagery.
Right panel: width about 360px with tabs Properties, Allocation, Split/Generate, Validation. Active tab Properties. Show selected parcel fields: Replot Parcel No, Plot Type, Design Area, Calculated Area, Original Parcel Links, Owner, Road Access, Frontage, Allocation Status. Include editable text boxes, combo boxes, numeric inputs, checkboxes. Include a section "Contribution Balance" with small progress/balance bars: Returnable 126.00 sqm, Assigned 125.40 sqm, Difference -0.60 sqm. Include action buttons Assign Original Parcel, Preview Split, Apply Change.
Bottom panel: height about 160px with tabs Operation Log, Issues, Scenario Metrics, Land Contribution Table. Active tab Issues with a DataGridView listing rows: Severity, Feature, Message, Action. Include example warnings: "Gap under tolerance", "Owner allocation pending", "Road frontage confirmed". To the side show compact metrics: Block area, Developable area, Road deduction, Plot count, Unallocated owners.
Status strip: show Project: Kamalpokhari LR, Tool: Split Parcel Preview, Coordinates: X/Y, Scale, Snap: On, Ortho: On, Zoom: 0.0001 precision.
UX details: make controls practical and implementable in WinForms. Use split containers, group boxes, tabs, tree view, data grid views, status strip, tool strips, combo boxes, checkboxes, numeric controls. Keep all text short and plausible. Do not create nested cards, web hero sections, purple gradients, decorative blobs, or marketing copy.
Constraints: The UI must communicate that only one block is being edited at once, zoom range is limited, block cannot shrink below 50x50 pixels, road centerlines render only near the block intersections, cadastral parcels are queried only when intersecting the selected block and roads, and replotting supports both automatic batch generation from finalized contribution data and manual geometry/topology editing.
Avoid: smartphone UI, browser web app, SaaS landing page, dark mode, oversized hero text, floating rounded cards, cartoon style, impossible futuristic controls, random decorative imagery, cluttered unreadable text, misspelled app name.
```

---

## Acceptance Checklist

- [ ] Workspace opens from `frmMain` replotting menu.
- [ ] Workspace can be opened for one selected block.
- [ ] Active block is clearly visible and cannot zoom below `50 x 50` pixels.
- [ ] Layer tree contains block-specific replot layers.
- [ ] Cadastral/context features are scoped to selected block and relevant road context.
- [ ] Manual parcel drawing, split, merge, move, copy, and reshape tools are represented in the UI.
- [ ] Automatic generation from finalized contribution data is represented in the UI.
- [ ] Right panel shows parcel properties, allocation, generation, and validation controls.
- [ ] Bottom panel shows issues, operation log, metrics, and contribution table.
- [ ] Validation must run before commit.
- [ ] Commit design is explicit and separate from preview/generation.
