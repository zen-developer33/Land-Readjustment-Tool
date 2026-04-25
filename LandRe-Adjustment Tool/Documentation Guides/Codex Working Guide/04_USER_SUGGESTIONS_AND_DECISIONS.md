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
