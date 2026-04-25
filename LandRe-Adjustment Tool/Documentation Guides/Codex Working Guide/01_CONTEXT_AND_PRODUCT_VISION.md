# RePlot Context and Product Vision

## What RePlot Is

RePlot is a specialized desktop application for land readjustment and land pooling projects in Nepal.

It is not intended to be:

- a generic CAD tool
- a generic GIS viewer
- a simple owner/parcel record manager
- a one-screen drawing utility

It is intended to become a complete project workbench for:

- land owner records
- original parcel records
- cadastral and project map layers
- project boundary and block planning
- contribution calculation
- replotted parcel design
- owner allocation
- legal traceability
- validation, reports, and exports

## Core Domain Workflow

The real-world land readjustment process should guide the software:

1. Create or open a project file.
2. Import parcel and owner records.
3. Deduplicate owners and validate imported data.
4. Link original parcels to map geometry.
5. Review original parcels, roads, boundaries, and ownership.
6. Define blocks and planning layout.
7. Calculate contribution and net returnable area.
8. Open a selected block in a dedicated replot workspace.
9. Create or import replotted parcel layouts.
10. Split, reshape, validate, and allocate replotted parcels.
11. Compare original and replotted ownership outcomes.
12. Generate reports and official outputs.

## Current Architecture Summary

The project already has a strong foundation:

- C# / .NET 8 Windows Forms application
- EF Core with SQLite project files using `.lpp`
- NetTopologySuite geometry support already configured
- project session and project context infrastructure
- project metadata and settings entities
- import session and raw-record audit trail
- land owner and baseline parcel entities
- contribution and replotting entities
- canvas layer and canvas object entities
- custom logging infrastructure
- owner deduplication and fuzzy matching services
- land owner and parcel management forms

## Main Architectural Gap

The biggest missing link is the spatial backend.

The database already has `CanvasLayer` and `CanvasObject`, but the runtime canvas and persisted geometry are not yet fully connected. Future canvas work should make geometry a first-class, validated, persisted, domain-linked asset.

## Main UI Direction

The main map window should be mostly for:

- viewing
- selecting
- identifying
- measuring
- simple markup
- launching the proper detailed workspace

It should not become the main place for complex parcel editing.

The dedicated `Block Replot Workspace` should handle:

- block-level parcel creation
- split/merge/reshape
- road and access planning
- contribution review
- owner allocation
- validation and scenario comparison

## Why This Matters

If editing is mixed into the main overview canvas too early, the application can become confusing and risky. Land readjustment work must be traceable, reviewable, and safe. A dedicated workspace makes the workflow clearer and easier to explain to users.

