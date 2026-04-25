# Codex Working Guide for RePlot

## Purpose

This folder is the working guide that Codex should consult before making future implementation decisions for RePlot.

It exists because RePlot is not a normal CRUD application. It is a professional land readjustment and replotting tool where records, geometry, ownership, contribution, block planning, validation, reporting, and auditability must all stay connected.

The goal is to keep every future change aligned with:

- the existing project documentation
- the user's product ideas from this chat
- professional C# and .NET design principles
- EF Core and SQLite project-file architecture
- clean architecture and dependency management
- practical explanations for a non-expert software developer

## How Codex Should Use This Folder

Codex does not need to reread every document on every prompt.

Use this folder as a context anchor when:

- the conversation context has been compacted or reset
- a new major implementation area begins
- there is confusion about product direction
- an architectural decision needs to be checked
- the user explicitly asks to reconnect with the documentation

Before major implementation work after a context reset, Codex should review:

1. `01_CONTEXT_AND_PRODUCT_VISION.md`
2. `02_ENGINEERING_STANDARDS.md`
3. `03_STEPWISE_IMPLEMENTATION_PROCESS.md`
4. `04_USER_SUGGESTIONS_AND_DECISIONS.md`
5. `05_CHANGE_TRACKER_CHECKLIST.md`

This should happen especially before touching:

- `frmMain`
- project/session/database infrastructure
- import workflow
- land owner and parcel records
- map canvas
- layer management
- contribution engine
- replotting workspace
- validation and reporting

## Source Documents Considered

This guide was created from the current `Documentation Guides` folder, including:

- `01_PROJECT_OVERVIEW.md`
- `01_PROJECT_OVERVIEW (1).md`
- `02_IMPLEMENTATION_STATUS.md`
- `03_ROADMAP_CHECKLIST.md`
- `MAP_CANVAS_LAND_READJUSTMENT_IMPLEMENTATION_GUIDE.md`
- `APPLICATION_MENU_WORKSPACE_AND_LAYER_DESIGN_GUIDE.md`
- NetTopologySuite guide files
- replot implementation and dependency injection guide files
- undo/redo guide files
- concept Word documents, images, and UI mockups

## Working Agreement

For future prompts:

- explain the next step in simple language before large changes
- implement in small, reviewable phases
- prefer service-layer and domain-layer changes over putting business logic directly in forms
- preserve existing user work and do not revert unrelated changes
- run focused validation after implementation
- update checklists or notes when architectural decisions change

## Dynamic Documentation Rule

This folder is a living guide.

When the user gives a new suggestion, product decision, workflow rule, or engineering preference during brainstorming or implementation, Codex should update the relevant documentation file immediately.

Common destinations:

- product idea or workflow decision: `04_USER_SUGGESTIONS_AND_DECISIONS.md`
- engineering standard: `02_ENGINEERING_STANDARDS.md`
- implementation process rule: `03_STEPWISE_IMPLEMENTATION_PROCESS.md`
- tracking item or reminder: `05_CHANGE_TRACKER_CHECKLIST.md`
