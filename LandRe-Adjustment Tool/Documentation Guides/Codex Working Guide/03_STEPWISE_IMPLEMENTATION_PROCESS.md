# Stepwise Implementation Process

## Purpose

This process should be followed before and during future coding work in RePlot.

It helps Codex stay connected to the larger product vision while still making practical code changes.

## Step 1: Understand the User Request

Before changing code, identify:

- which workflow the request belongs to
- which user role benefits from it
- whether the change is data, UI, map, replot, validation, or reporting
- whether it affects project files or database schema
- whether it must be backward compatible with existing projects

## Step 2: Reconnect with the Relevant Guide When Needed

Do not reread all documentation on every small prompt.

Use this folder as a fast context refresh when:

- context has been compacted or reset
- the next task starts a new major implementation area
- the user asks to align with documentation
- an architectural or workflow decision is unclear

When needed, review:

- product context: `01_CONTEXT_AND_PRODUCT_VISION.md`
- engineering rules: `02_ENGINEERING_STANDARDS.md`
- user ideas: `04_USER_SUGGESTIONS_AND_DECISIONS.md`
- tracker: `05_CHANGE_TRACKER_CHECKLIST.md`

Then check the source documentation if needed:

- `01_PROJECT_OVERVIEW.md`
- `02_IMPLEMENTATION_STATUS.md`
- `03_ROADMAP_CHECKLIST.md`
- map canvas guide
- menu/workspace/layer guide

## Step 2A: Update Documentation When Ideas Emerge

If the user gives a new suggestion, workflow rule, product concept, or engineering preference, update the relevant documentation file immediately.

This applies during:

- brainstorming
- planning
- coding
- review
- bug fixing

The documentation is dynamic and should evolve with the project.

## Step 3: Locate the Existing Code

Find current implementations before designing a new one.

Common areas:

- `Data/AppDbContext.cs`
- `Data/ProjectSession.cs`
- `Data/AppServices.cs`
- `Core/Entities`
- `Services`
- `Repositories`
- `UI/Forms/frmMain.cs`
- `UI/CustomControls/DrawingCanvasControl.cs`
- `UI/MapCanvas`

## Step 4: Choose the Smallest Useful Change

Prefer phased improvements over huge rewrites.

Good changes:

- add a service that isolates business logic
- add a DTO or view model to simplify UI binding
- wire an existing menu to an existing form
- improve one workflow end to end
- add validation around one risky operation

Avoid:

- rewriting multiple modules at once
- introducing a new architecture style in only one place
- mixing unrelated cleanup with feature work
- making code unnecessarily large
- keeping dead code beside a new implementation

If an old implementation file is being replaced, copy it first into a separate archive folder such as:

`Old Implementations/<feature-name>/`

Then keep the active implementation clean.

## Step 5: Explain the Plan Simply

Before substantial edits, explain:

- what will be changed
- why it matters
- which files are likely involved
- what will be verified

The explanation should be understandable to a non-expert developer.

## Step 6: Implement with Professional .NET Practices

During implementation:

- keep forms thin
- put reusable logic in services
- keep code compact while preserving clarity
- remove dead or unnecessary code in files being replaced
- use async for long operations
- log meaningful operations
- handle exceptions carefully
- avoid direct database work inside UI event handlers where possible
- preserve existing user changes
- use EF Core transactions for multi-step saves

## Step 7: Verify

Verification depends on risk.

For UI wiring:

- build the solution
- check menu enable/disable state
- check event handlers

For service or database work:

- build the solution
- run relevant tests if available
- test with a small project if practical
- verify migrations if schema changes

For documentation-only work:

- verify files were created
- check folder structure
- check git status for expected new files

## Step 8: Explain What Happened

After implementation, explain:

- what changed
- why it changed
- how it was checked
- what remains
- what the next safe step is

Keep the explanation direct and calm.

## Step 9: Update Tracking Notes

When documentation or architecture direction changes, update:

- `04_USER_SUGGESTIONS_AND_DECISIONS.md`
- `05_CHANGE_TRACKER_CHECKLIST.md`

When code implementation starts, consider adding a short implementation note under this folder so future work can reconnect quickly.
