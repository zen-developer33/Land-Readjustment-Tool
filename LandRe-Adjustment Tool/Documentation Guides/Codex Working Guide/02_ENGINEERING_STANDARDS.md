# Engineering Standards for RePlot

## Purpose

This file defines the engineering principles Codex should apply when implementing future RePlot features.

These standards are meant to keep the project professional, maintainable, and understandable for the user.

## C# and Object-Oriented Design

Use object-oriented design to separate responsibilities clearly:

- forms should handle presentation and user interaction
- services should hold business workflow logic
- repositories should handle database access
- entities should represent persisted domain data
- DTOs and view models should protect the UI from direct EF Core coupling
- domain services should hold important rules such as validation, splitting, contribution, and allocation

Avoid putting business rules directly inside button click handlers.

## Clean Architecture Direction

The project does not need a disruptive rewrite, but each new feature should move toward:

- `Core` for entities and domain concepts
- `Services` for application and business logic
- `Repositories` or EF-backed services for persistence
- `UI` for WinForms forms and controls
- `Infrastructure` for logging, constants, file storage, and external integrations

When legacy models must remain, use mapping services or DTOs as an anti-corruption layer.

## Code Size and Cleanup Discipline

Implementation should be as small as practical while keeping the same behavior.

Codex should:

- avoid unnecessarily large code
- reduce line count when clarity is preserved
- remove dead code in files being actively replaced or refactored
- avoid duplicated helper logic when a small shared helper is clearer
- avoid adding abstractions that do not reduce real complexity
- keep the new implementation focused on the requested workflow

Important rule:

When replacing an old implementation file with a new implementation, preserve the old file first by copying it into a clearly named archive folder such as:

`Old Implementations/<feature-name>/`

Do this only for files that are truly being replaced. Do not copy every touched file automatically.

The old implementation should remain available for reference, while the active codebase should stay clean and not carry unused duplicate code.

## Dependency Injection

Prefer constructor injection for new services and new complex forms.

Existing `AppServices` can remain as a bridge for WinForms, but new code should avoid expanding static service-locator usage when a better dependency boundary is practical.

Recommended service lifetime thinking:

- database context/session: per project session
- repositories: scoped to the active project session
- business services: scoped or transient
- pure stateless helpers: singleton only if safe
- forms: transient

## Entity Framework Core

Use EF Core carefully:

- keep database queries async where possible
- avoid loading large graphs unless needed
- use `AsNoTracking()` for read-only lists
- use transactions for multi-entity changes
- add indexes for frequent filters and relationships
- keep migrations intentional and reviewed
- validate required relationships before saving
- do not hide database writes inside UI-only helpers

Important domain operations should be transactional:

- import persistence
- parcel geometry assignment
- parcel split and merge
- contribution recalculation
- replot allocation
- finalization and locking

## Error Handling

Use layered error handling:

- validate input before persistence
- services return useful results or throw meaningful exceptions
- UI catches expected failures and shows friendly messages
- unexpected failures should be logged through `IAppLogger`
- do not swallow exceptions silently

For user-facing messages, explain:

- what failed
- what the user can do
- whether project data was saved or rolled back

## Logging

Use logging for:

- project open/save/close
- imports
- deduplication decisions
- validation runs
- geometry operations
- contribution recalculation
- replot scenario changes
- backups and restores
- unexpected exceptions

Logs should help diagnose issues without forcing the user to understand code.

## Asynchronous Programming

Use async for:

- project open/save
- import and validation
- large database queries
- file copy and document operations
- geometry loading
- report export

Keep UI responsive during long operations by using progress reporting and cancellation where appropriate.

## Events and UI Communication

Use events for UI state changes when appropriate:

- selected parcel changed
- active layer changed
- project context changed
- validation status changed
- map feature selection changed
- workspace mode changed

Avoid direct form-to-form coupling when a service or event can communicate the state more cleanly.

## Scalability and Performance

Design for large projects:

- thousands of owners
- thousands of parcels
- many canvas objects
- multiple imports
- heavy validation
- report exports

Use:

- pagination in grids
- filtering at database level where possible
- spatial indexing for canvas queries
- layer visibility to reduce rendering load
- background work for expensive operations

## User Understanding

The user is not an experienced software developer, so Codex should explain implementation in clear steps:

- what problem we are solving
- what files are involved
- why the design is being chosen
- what changed
- how it was verified
- what remains next

Use plain software explanations, not unexplained jargon.
