# Codex Working Guide Change Tracker

## Purpose

This file tracks the documentation organization work done in this folder.

It is also the checklist Codex should use to stay aligned before future implementation work.

## Documentation Guide Creation Checklist

- [x] Located the active documentation folder: `LandRe-Adjustment Tool/Documentation Guides`
- [x] Reviewed markdown guide inventory
- [x] Reviewed project overview guide
- [x] Reviewed implementation status guide
- [x] Reviewed roadmap checklist guide
- [x] Considered previously generated map canvas and land readjustment guide
- [x] Considered previously generated menu, workspace, and layer guide
- [x] Created separate `Codex Working Guide` folder
- [x] Created folder index in `README.md`
- [x] Created context and product vision guide
- [x] Created engineering standards guide
- [x] Created stepwise implementation process guide
- [x] Created user suggestions and decisions guide
- [x] Created this change tracker checklist
- [x] Added code-size and old-implementation preservation guidance
- [x] Added dynamic documentation and context-refresh guidance
- [x] Added raster import, storage, and rendering architecture guide
- [x] Expanded raster guide with package choices, DI structure, workflow, and performance rules
- [x] Added overall performance and scalability architecture guide

## Files Created in This Pass

- [x] `README.md`
- [x] `01_CONTEXT_AND_PRODUCT_VISION.md`
- [x] `02_ENGINEERING_STANDARDS.md`
- [x] `03_STEPWISE_IMPLEMENTATION_PROCESS.md`
- [x] `04_USER_SUGGESTIONS_AND_DECISIONS.md`
- [x] `05_CHANGE_TRACKER_CHECKLIST.md`

## Future Prompt Context Checklist

Before future implementation prompts, Codex should check:

- [ ] What workflow is the user asking about?
- [ ] Is this a small continuation where existing context is enough?
- [ ] Has the context been compacted/reset, requiring guide refresh?
- [ ] Which existing guide is most relevant if refresh is needed?
- [ ] Which current code files implement that workflow?
- [ ] Is this UI-only, service-level, database-level, or all three?
- [ ] Does this need EF Core migration?
- [ ] Does this need logging?
- [ ] Does this need async behavior?
- [ ] Does this need exception handling?
- [ ] Does this need transaction safety?
- [ ] Does this need user-facing explanation?
- [ ] Does this affect main map, layer manager, or block replot workspace?
- [ ] Is the implementation compact enough?
- [ ] Is there dead code in files being replaced?
- [ ] If replacing an old file, was the old implementation archived first?
- [ ] Did the user give a new idea that should be stored in documentation immediately?
- [ ] What verification is appropriate?

## Implementation Quality Checklist

When coding, verify:

- [ ] responsibilities are separated cleanly
- [ ] business logic is not buried in forms
- [ ] code is not unnecessarily large
- [ ] dead code was removed from touched replacement files
- [ ] old replaced implementation was copied to an archive folder when appropriate
- [ ] services are named clearly
- [ ] EF Core queries are efficient
- [ ] large operations are asynchronous where practical
- [ ] errors are logged and explained
- [ ] UI remains understandable
- [ ] unrelated user changes are preserved
- [ ] build or relevant validation has been run

## Notes for the User

This guide folder is not final architecture law. It is a living alignment point.

When your idea changes, this folder should change too. That way the code, documentation, and product direction keep walking together.
