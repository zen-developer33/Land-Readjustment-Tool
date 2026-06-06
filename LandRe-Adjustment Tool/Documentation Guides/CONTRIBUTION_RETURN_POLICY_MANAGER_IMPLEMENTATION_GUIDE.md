# Contribution / Return Policy Manager Implementation Guide

## Purpose And Scope

The Policy Manager is a separate MDI window for managing land contribution and land return policy standards. It stores policy clauses, sub-clauses, descriptions, parameters, lookup tables, implementation diagrams, approval status, lock state, import packages, and export packages.

This manager does not collect parcel inputs, does not calculate parcel contribution, does not manage owner ledgers, and does not create replot allocation results. Those workflows may later read an approved policy, but they are outside this implementation.

## Database Standard

The policy data is stored in normalized project database tables so it remains queryable and editable:

- `tblPolicySets`: one policy version, including policy group key, code, name, type, version, status, lock state, source metadata, and dates.
- `tblPolicyClauses`: clause and sub-clause hierarchy with code, heading, section, description, parent clause, and display order.
- `tblPolicyParameters`: policy parameters linked to the policy and optionally to a clause.
- `tblPolicyLookupTables`, `tblPolicyLookupColumns`, `tblPolicyLookupRows`, `tblPolicyLookupCells`: editable tables for road contribution, special facilities, project corner-type definitions, and corner-plot rates. Each lookup table may be linked to the policy clause it supports.
- `tblPolicyAttachments`: implementation/reference images linked to a policy or clause.
- `tblPolicyAuditEntries`: draft saves, imports, exports, approvals, locks, and version creation history.

An EF migration creates these tables for new databases. `ProjectDatabaseCompatibility.EnsureAsync` also creates the policy tables when opening older `.lpp` project files with incomplete migration history.

## Baireni Draft Template

On first Policy Manager open, the application seeds one editable draft policy:

**Baireni Land Pooling Contribution and Return Policy v1**

Seeded content:

- contribution clauses `2.2.1` through `2.2.11`;
- return clauses `2.3.1` through `2.3.16`;
- fixed parameters such as open area `5.04%`, infrastructure `4.79%`, road corner `1.00%`, and fixed common total `10.83%`;
- return policy parameters such as `79.5 sqm` absolute minimum plot area, `130 sqm` preferred minimum, `6 m` frontage, depth `13-20 m`, block depth `26-40 m`, `1 m` setbacks, `200 sqm` consolidation limit, and editable project land rate;
- project corner-type definitions generated from the two adjacent frontage roads, using the primary frontage road name/width and secondary frontage road name/width;
- Schedule 1 Table 1 road contribution values;
- Schedule 1 Table 2(a) special facility rates;
- Schedule 1 Table 2(b) corner plot rates.

The template is intentionally a draft. The user may edit text, codes, parameters, and lookup values before approval.

Existing editable Baireni draft policies are upgraded when the Policy Manager opens. The upgrade links lookup tables to clauses, adds the project corner-type definition table if missing, and changes contribution ratios from hidden decimal values to user-facing percent values with `%` as the unit.

## UI Workflow

The main RePlot form is not converted to MDI. Existing Contribution menu items open a separate `frmPolicyManagerMdiHost`, which is a top-level MDI container.

The default child window is `frmPolicyManagerDashboard`:

- top toolbar: policy selector, new policy, save draft, lock/unlock, new draft from approved, approve, import, export;
- left grid: clauses and sub-clauses with fixed-width columns;
- center editor: policy code/name and selected clause code, section, heading, description;
- right picture box: implementation diagram/reference image;
- the dashboard is kept focused on readable policy text and implementation figures.

Additional MDI child windows keep the workflow uncluttered:

- `frmPolicyParametersWindow`: all policy parameters in a DataGridView, with clause code, key, label, editable value, unit, value type, and description.
- `frmPolicyLookupTablesWindow`: road contribution, special-facility, and corner-rate lookup tables. The selected table shows its linked clause above the grid.
- `frmPolicyLookupTablesWindow` in corner-type mode: project-specific corner definitions based on the primary frontage road and secondary frontage road. Each road reference is selected by road name and width from the project road definitions. The grid auto-generates all unordered frontage combinations, for example `9.14 x 9.14`, `9.14 x 8`, `9.14 x 7`, `8 x 8`, `8 x 7`, and so on.
- In the corner-type grid, only `Primary Frontage Road` and `Secondary Frontage Road` are editable. `Corner Code`, `Display Name`, default rate, and description are regenerated from the two selected roads.

DataGridViews use explicit practical column widths rather than fill-to-width sizing. Horizontal scrolling remains available for wider policy tables. Editable cells use a beige background so users can immediately identify where drafting changes are allowed. Unit values are stored and displayed in their own columns, for example `%`, `sqm`, and `m`.

## Drafting And Approval Rules

- Draft policies are editable in place and can be saved repeatedly.
- Drafts can be locked to make them read-only for review, then unlocked again before approval.
- Drafts may contain incomplete clause codes or parameter keys while the user is still drafting.
- Approval runs strict validation:
  - policy code required;
  - all clauses need codes;
  - all parameters need keys;
  - duplicate clause codes and duplicate parameter keys are rejected;
  - numeric parameters are checked against bounds when bounds exist.
- Approved policies are locked and read-only.
- To edit an approved policy, the user creates a new draft version from the approved policy.
- If the global application edit lock is enabled, the Policy Manager opens in read-only mode.

## Import And Export

Policy packages use `.rpolicy` JSON files.

Package contents:

- schema version and export timestamp;
- policy metadata;
- clauses and parent-child hierarchy;
- parameters and values;
- lookup table columns, rows, and cells;
- attachments as base64 image data.

Import creates a new draft copy. Approved policies are not overwritten through import.

## Manual Test Checklist

- Open a project and launch the Policy Manager from the Contribution menu.
- Confirm the Baireni draft policy is seeded on first open.
- Add a new clause and sub-clause.
- Edit clause heading, section, description, and save draft.
- Open the Parameters MDI child and add, edit, and delete a parameter.
- Confirm editable parameter cells are highlighted beige and unit values are separate from values.
- Open the Corner Types MDI child and confirm all road name/width combinations are generated.
- Edit only the primary and secondary frontage road cells and confirm the corner code and display name are regenerated.
- Open the Lookup Tables MDI child and edit road contribution, special-facility, and corner contribution lookup cells.
- Confirm each lookup table shows its associated clause.
- Attach an implementation image to a clause.
- Lock a draft and confirm the dashboard and grids become read-only, then unlock it.
- Export a `.rpolicy` file and import it as a draft copy.
- Approve a policy and confirm fields/grids become read-only.
- Create a new draft from the approved policy and confirm editing is available again.
