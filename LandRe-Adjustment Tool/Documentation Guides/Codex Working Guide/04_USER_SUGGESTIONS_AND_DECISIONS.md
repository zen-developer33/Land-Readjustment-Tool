# User Suggestions and Product Decisions

## Purpose

This file records the user's important product ideas and collaboration preferences so they remain visible during future implementation.

## User's Product Direction

The user wants RePlot to become a professional land readjustment application, not a simple records tool.

Important priorities:

- strong C# and .NET implementation
- proper object-oriented design
- EF Core database architecture
- dependency injection where appropriate
- clean code and maintainable services
- error handling and logging
- asynchronous work for heavy operations
- scalable design for large projects
- clear explanations for a non-expert developer

## Main Map Canvas Decision

The main canvas window should not be the heavy parcel-editing workspace.

It should be used for:

- viewing project layers
- selecting parcels, owners, blocks, and roads
- identifying features
- measuring
- read-only review
- simple extra drawing or markup

It should avoid:

- complex parcel split operations
- detailed block replot editing
- ownership allocation editing
- final geometry-changing operations

## Dedicated Replot Workspace Decision

The user suggested, and the guide supports, a dedicated workspace for editing inside a selected block.

This should become the `Block Replot Workspace`.

It should support:

- editing inside one selected block
- block-specific layers
- parcel creation and split tools
- road/access planning
- contribution and returnable area review
- owner allocation
- validation
- scenario comparison
- finalization and publishing back to the main project map

## Layer Management Ideas

The user specifically mentioned the need to organize:

- boundary layers
- parcels before land readjustment
- block layouts imported from other sources
- block layouts drawn inside the application
- basemaps
- many future supporting layers

Recommended layer groups:

- Project Framework
- Original Land Data
- Existing Context
- Imported Design
- Replot Design
- Review and Analysis
- Annotation and Markup
- Temporary Work

Current implementation-phase decision for the main layer tree:

- first implement only a `Raster` root node
- the `Raster` root should behave as a grouping node, not a renderable layer
- the `Raster` root should not show a checkbox
- imported raster layers should appear one by one as child nodes under `Raster`
- child raster nodes should be checkable and uncheckable to control rendering
- keep the architecture open so future vector, annotation, design, and review groups can be added without rewriting the tree again

## Raster Map Support Direction

The user is now exploring raster map support for:

- GeoTIFF
- TIFF
- MBTiles
- JPG and similar image sources

Standing architectural direction:

- raster import should fit the project/session/database architecture
- source files may be external in the project workspace
- the application should use internal cached raster tiles for display
- the map canvas should render visible tiles only
- non-georeferenced images should support user georeferencing
- project CRS alignment is important for RePlot, especially because MBTiles display is Web Mercator oriented

This should be treated as part of the canvas foundation work, not as a one-off utility feature.

Additional direction from discussion:

- raster must connect to the same layer manager system used by vectors
- raster visibility and rendering should be controlled by layer state
- raster layers should appear in the left layer tree/panel
- for architecture, prefer a top-level raster group in the layer tree with raster items beneath it, rather than an isolated rendering path outside the layer system
- do not code this immediately; treat it as a deliberate architecture decision first

Import-management direction decided during planning:

- use one orchestration service for raster import workflow
- keep format-specific readers/handlers behind that orchestrator instead of one giant all-format class
- examples of specialization include GeoTIFF/TIFF handling, MBTiles handling, and plain-image georeferencing support
- keep import workflow separate from layer-tree UI wiring so the tree can be ready before raster persistence/rendering is completed

## Menu and Toolstrip Direction

The application menu should be reorganized around real user workflows:

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

The main toolbar should stay focused on project actions, navigation, layers, and review. Editing-heavy tools should live inside the block replot workspace.

## Communication Preference

The user wants step-by-step explanation.

Codex should:

- explain the purpose before implementation
- describe what files or systems are being changed
- explain technical concepts simply
- avoid assuming the user knows advanced software terms
- keep the user oriented after each implementation

## Documentation Memory Preference

The user clarified that Codex does not need to read all documentation for every prompt.

Standing rule:

- do not reread every guide on every small request
- use the guide folder when context has reset, compacted, or a major new implementation starts
- before starting after a new context window, reconnect through these documentation files
- when a new suggestion, idea, workflow concept, or engineering preference comes up, immediately add it to the relevant documentation file
- treat the documentation as dynamic and continuously evolving

## Code Cleanup Preference

The user wants implementation to stay compact.

Important guidance:

- do not make the code unnecessarily large
- reduce lines of code where the same behavior can be kept clearly
- clean dead code and unnecessary code in implementations Codex works on
- preserve old implementations when replacing them
- if an old file is replaced, copy it to a separate old/archive folder first
- let older implementation remain available for reference, but keep active files clean

## Standing Decision

For future work, Codex should connect implementation back to these guides and explain how the change fits the larger product.

## Performance Priority Decision

The user clarified a project-wide priority:

- this is intended to become robust, practical spatial software, not a toy canvas
- performance is a topmost priority across the whole application
- smoothness and scalability matter more than visual richness
- this applies to the full product, not only the map canvas
- architecture should be chosen for large real-world workloads
- Codex should think like an experienced .NET backend/spatial software engineer when evaluating designs

## Layer Tree UX Decision

The user requested the layer view to be expanded beyond only raster and kept visually consistent:

- include additional necessary layer groups in the main layer tree, not just raster
- keep root/group nodes non-checkable and child layer nodes checkable for visibility
- keep layer view UI simple and clean
- use the same font family as other UI elements
- ensure text/background presentation in the layer tree matches the layer-view surface (no mismatched patchy background)

Reference style note from the user:

- use a practical engineering-software-style tree pattern (similar to road-design software)
- keep expandable grouped hierarchy with checkable leaf layers
- organize groups for replot domain needs (not road-domain wording), e.g. original data, replot design, review, external, raster
- keep TreeView visual style close to default Windows style (show node connector lines; avoid custom-painted look)
- use built-in TreeView checkboxes (`TreeView.CheckBoxes = true`) instead of custom painted/state-image checkbox logic

## Layer Tree UX Decision (Latest Override)

The user later overrode the above checkbox approach and requested a custom owner-draw layer tree:

- use `TreeViewDrawMode.OwnerDrawAll`
- draw checkboxes manually with `CheckBoxRenderer`
- do not show checkbox for root/group nodes
- show checkbox only for real layer nodes
- draw a small colored symbol rectangle beside each real layer node
- keep expand/collapse, selection highlight, and node tree hierarchy behavior
- toggle visibility only when the checkbox hit area is clicked

## Layer Tree Implementation Direction

Current implementation direction:

- the left layer tree should show all major RePlot root groups, not only raster
- standard project layers should exist as real `CanvasLayer` records when a project is open
- root/group nodes remain organizational and non-renderable
- child nodes represent real layers and can control visibility
- layer grouping is currently derived in the layer-tree service until the database model receives a proper `LayerGroup` or `LayerCode` field later

## Layer Tree UX Decision (Current Override)

The user later requested a simpler first implementation:

- do not custom-draw tree nodes or custom checkbox graphics
- use default WinForms `TreeView` checkbox behavior first
- keep layer grouping and visibility logic, but avoid extra rendering complexity at this stage

Exact root list lock (current requirement):

- `Original Data Layer`
- `Proposed Data Layer`
- `RasterLayer`
- `Other External Layers`

No additional root groups should be added in this phase.

Child node visual requirement added:

- add a few child layers under each root group
- child nodes should show a color swatch box before the label text
- swatch should be square/near-square with a dark border
- keep root list as fixed four groups

## Standing Coding Architecture Preference

The user clarified that future coding work should always protect clean architecture and readability.

Standing rule for Codex:

- do not put business workflows, database operations, layer operations, or file-copy/open/save logic directly inside WinForms forms
- forms should mostly handle UI coordination only: events, dialogs, selected controls, positioning, and refresh calls
- encapsulate workflow logic in small readable services
- use abstraction and dependency injection where it keeps code cleaner
- avoid over-engineering; choose the smallest service or helper that clearly separates responsibilities
- keep behavior exactly the same when refactoring unless the user asks for behavior changes
- keep code minimal and understandable; do not clutter files with complex patterns for simple workflows
- add XML summary comments for every new class and method Codex creates
- prefer readable names and straightforward control flow over clever code
- after implementing, build the project and report whether errors remain

## Map Canvas Rendering Order Preference

The user clarified that map-canvas render order must be intentionally managed and should not be hidden as scattered drawing calls inside forms or large renderer methods.

Standing rule for canvas rendering:

- clear the canvas background first
- render fixed reference visuals first, including grid lines, axis lines, and origin markers
- render raster/map content after the fixed reference visuals
- render temporary interaction feedback last, such as zoom-window rectangles, so the user can always see active UI feedback
- manage render-pass order through a small dedicated rendering-order service or pipeline
- keep forms out of rendering-order decisions; forms should only request redraws or pass UI state

## CRS and Raster Projection Direction

The user clarified that project CRS and datum settings must drive raster rendering:

- all MUTM coordinate systems should expose the full list of available datum transformations
- changing project CRS or datum should update already imported raster layers to the new transformation where possible
- imported raster and vector layers should eventually pass through a dedicated projection service
- layers without CRS or georeferencing should have a simple define-projection workflow after import
- map canvas zooming should allow backing out far enough to view a whole-world extent when needed

Current raster-only implementation scope:

- postpone AutoCAD, DXF, DWG, and external vector-layer projection workflows
- focus the current import architecture on raster sources only
- support GDAL-readable raster sources such as GeoTIFF, TIFF, VRT, IMG, JPG, PNG, BMP, and MBTiles
- route raster import through services and interfaces instead of placing import logic directly in forms
- provide a lightweight raster review form after file selection, with preview, layer name, read-only metadata, project CRS, and source CRS definition
- when imported raster data does not store a source CRS, default the source CRS choice to WGS 1984 (`EPSG:4326`)
- use a combobox for source CRS selection, with common CRS choices plus custom EPSG and custom WKT options, instead of making the user type the default CRS manually
- if a raster has no georeferencing at all, import it only as temporary image coordinates until a later georeferencing workflow is added
- keep visual form layout in `.Designer.cs` files and keep `.cs` files focused on behavior only

## Canvas Status Bar Direction

The user clarified that the map canvas status bar should become a general project and canvas operation surface:

- show canvas command status separately from mouse coordinates
- include an active layer selector so the current working layer is always visible
- use the progress bar for imports and other long-running project or canvas operations
- show operation status text beside the progress bar so users know what stage is running
