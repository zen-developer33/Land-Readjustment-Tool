using Land_Readjustment_Tool.Core.Entities.Policy;
using Land_Readjustment_Tool.Services.Policy;
using Microsoft.VisualBasic;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Code-behind for the policy editor dashboard.
    /// Static layout/sizes/colors live in <see cref="frmPolicyManagerDashboard.Designer.cs"/>.
    /// This file only holds event wiring and runtime behavior.
    /// </summary>
    public sealed partial class frmPolicyManagerDashboard : Form
    {
        private readonly PolicyManagerService _service;
        private bool _readOnlyMode;
        private List<PolicySet> _policySummaries = [];
        private PolicySet? _currentPolicy;
        private PolicySectionDefinition? _currentSection;
        private PolicyClause? _currentClause;
        private bool _isReloadingSections;
        private int? _selectedParameterIdBeforeRefresh;

        public event EventHandler<string>? StatusChanged;

        public frmPolicyManagerDashboard(PolicyManagerService service, bool readOnlyMode)
        {
            _service = service;
            _readOnlyMode = readOnlyMode;
            InitializeComponent();
            RecordFormTheme.Apply(this);
            _service.PolicyChanged += PolicyManagerService_PolicyChanged;
        }

        public void SetReadOnlyMode(bool readOnlyMode)
        {
            _readOnlyMode = readOnlyMode;
            UpdateEditState();
        }

        public async Task RefreshCurrentPolicyAsync()
        {
            if (_currentPolicy == null)
                return;

            await ReloadPoliciesAsync(_currentPolicy.Id, _currentClause?.Id, SelectedParameterId(), _currentSection?.Id);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Service events
        // ──────────────────────────────────────────────────────────────────────

        private void PolicyManagerService_PolicyChanged(object? sender, int policySetId)
        {
            if (_currentPolicy == null || _currentPolicy.Id != policySetId || IsDisposed || !IsHandleCreated)
                return;

            BeginInvoke(new Action(async () =>
            {
                if (!IsDisposed)
                    await RefreshCurrentPolicyAsync();
            }));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Form load
        // ──────────────────────────────────────────────────────────────────────

        private async void frmPolicyManagerDashboard_Load(object? sender, EventArgs e)
        {
            Notify("Loading policies...");
            lstValidation.Items.Clear();
            lstValidation.Items.Add("Loading policy standards...");
            BeginInvoke(new Action(async () => await ReloadPoliciesSafelyAsync()));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Header toolbar
        // ──────────────────────────────────────────────────────────────────────

        private async void btnManagePolicies_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Opening policy selector...", ManagePoliciesAsync);

        private async void btnSaveDraft_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Saving draft...", SaveCurrentEditorsAsync);

        private async void btnApprove_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Approving policy...", ApproveAsync);

        private async void btnLockUnlock_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Changing edit lock...", ToggleLockAsync);

        private async void btnExport_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Exporting policy...", ExportAsync);

        // ──────────────────────────────────────────────────────────────────────
        // Sections grid
        // ──────────────────────────────────────────────────────────────────────

        private void dgvSections_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isReloadingSections)
                return;

            _currentSection = dgvSections.SelectedRows.Count > 0
                ? dgvSections.SelectedRows[0].Tag as PolicySectionDefinition
                : null;

            LoadClauseGrid();
            SelectFirstClause();
            UpdateEditState();
        }

        private async void btnAddSection_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Adding section...", AddSectionAsync);

        private async void btnRenameSection_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Renaming section...", RenameSectionAsync);

        private async void btnDeleteSection_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Deleting section...", DeleteSectionAsync);

        private async Task AddSectionAsync()
        {
            if (_currentPolicy == null || !IsCurrentEditable())
                return;

            string heading = Interaction.InputBox(
                "Enter the new section's heading:",
                "Add Section",
                "");
            if (string.IsNullOrWhiteSpace(heading))
                return;

            PolicySectionDefinition added = await RunServiceAsync(
                () => _service.AddSectionAsync(_currentPolicy.Id, heading));
            await ReloadPoliciesAsync(_currentPolicy.Id, _currentClause?.Id, SelectedParameterId(), added.Id);
            Notify("Section added");
        }

        private async Task RenameSectionAsync()
        {
            if (_currentPolicy == null || _currentSection == null || !IsCurrentEditable())
                return;

            string heading = Interaction.InputBox(
                "Edit the section's heading:",
                "Rename Section",
                _currentSection.Heading);
            if (string.IsNullOrWhiteSpace(heading) ||
                string.Equals(heading, _currentSection.Heading, StringComparison.Ordinal))
                return;

            await RunServiceAsync(() => _service.RenameSectionAsync(_currentSection.Id, heading));
            await ReloadPoliciesAsync(_currentPolicy.Id, _currentClause?.Id, SelectedParameterId(), _currentSection.Id);
            Notify("Section renamed");
        }

        private async Task DeleteSectionAsync()
        {
            if (_currentPolicy == null || _currentSection == null || !IsCurrentEditable())
                return;

            if (MessageBox.Show(
                    $"Delete section '{_currentSection.SectionCode} - {_currentSection.Heading}'?\n\n" +
                    "A section that still has clauses cannot be deleted; reassign or delete its clauses first.",
                    "Policy Manager",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            await RunServiceAsync(() => _service.DeleteSectionAsync(_currentSection.Id));
            await ReloadPoliciesAsync(_currentPolicy.Id);
            Notify("Section deleted");
        }

        // ──────────────────────────────────────────────────────────────────────
        // Clause grid
        // ──────────────────────────────────────────────────────────────────────

        private void dgvClauses_SelectionChanged(object? sender, EventArgs e) => SelectClauseFromGrid();

        private void dgvClauses_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e) =>
            SelectClauseRowForContextMenu(e);

        private async void btnAddClause_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Adding clause...", AddSiblingClauseAsync);

        private async void btnAddSubClause_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Adding sub-clause...", () => AddClauseAsync(_currentClause?.Id));

        private async void btnDuplicateClause_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Duplicating clause...", DuplicateClauseAsync);

        private async void btnDeleteClause_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Deleting clause...", DeleteClauseAsync);

        private async void btnMoveUp_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Moving clause...", () => MoveClauseAsync(-1));

        private async void btnMoveDown_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Moving clause...", () => MoveClauseAsync(1));

        private void btnOpenDiagram_Click(object? sender, EventArgs e) => OpenDiagram();

        // Right-click context menu mirrors the toolbar buttons.
        private async void menuAddClause_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Adding clause...", AddSiblingClauseAsync);

        private async void menuAddSubClause_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Adding sub-clause...", () => AddClauseAsync(_currentClause?.Id));

        private async void menuDuplicateClause_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Duplicating clause...", DuplicateClauseAsync);

        private async void menuDeleteClause_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Deleting clause...", DeleteClauseAsync);

        // ──────────────────────────────────────────────────────────────────────
        // Parameter grid
        // ──────────────────────────────────────────────────────────────────────

        private async void btnAddParameter_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Adding parameter...", AddParameterAsync);

        private async void btnDeleteParameter_Click(object? sender, EventArgs e) =>
            await RunPolicyUiOperationAsync("Deleting parameter...", DeleteParameterAsync);

        private async void dgvParameters_CellEndEdit(object? sender, DataGridViewCellEventArgs e) =>
            await RunPolicyUiOperationAsync("Saving parameter...", () => SaveParameterRowAsync(e.RowIndex));

        private void dgvParameters_SelectionChanged(object? sender, EventArgs e) => UpdateEditState();

        // ──────────────────────────────────────────────────────────────────────
        // Data loading
        // ──────────────────────────────────────────────────────────────────────

        private async Task ReloadPoliciesAsync(
            int? selectPolicyId = null,
            int? selectClauseId = null,
            int? selectParameterId = null,
            int? selectSectionId = null)
        {
            _policySummaries = await RunServiceAsync(() => _service.GetPolicySummariesAsync());
            if (_policySummaries.Count == 0)
            {
                _currentPolicy = null;
                ClearPolicyUi();
                return;
            }

            int policyId = selectPolicyId ?? _currentPolicy?.Id ?? _policySummaries[0].Id;
            if (!_policySummaries.Any(p => p.Id == policyId))
                policyId = _policySummaries[0].Id;

            await LoadPolicyByIdAsync(policyId, selectClauseId, selectParameterId, selectSectionId);
        }

        private async Task LoadPolicyByIdAsync(
            int policyId,
            int? selectClauseId,
            int? selectParameterId,
            int? selectSectionId)
        {
            Notify("Loading selected policy...");
            _currentPolicy = await RunServiceAsync(() => _service.GetPolicyDashboardAsync(policyId));
            _selectedParameterIdBeforeRefresh = selectParameterId;
            LoadPolicyToUi(selectClauseId, selectSectionId);
            ShowDashboardValidationPlaceholder();
            Notify("Policy loaded");
        }

        private async Task ReloadPoliciesSafelyAsync(int? selectPolicyId = null)
        {
            await RunPolicyUiOperationAsync(
                "Loading policies...",
                () => ReloadPoliciesAsync(selectPolicyId));
        }

        // ──────────────────────────────────────────────────────────────────────
        // UI population
        // ──────────────────────────────────────────────────────────────────────

        private void ClearPolicyUi()
        {
            lblCurrentPolicy.Text = "No policy selected";
            txtPolicyCode.Clear();
            txtPolicyName.Clear();
            lblStatus.Text = "No policy";
            dgvSections.Rows.Clear();
            dgvClauses.Rows.Clear();
            dgvParameters.Rows.Clear();
            SelectClause(null);
            UpdateEditState();
        }

        private void LoadPolicyToUi(int? selectClauseId, int? selectSectionId)
        {
            if (_currentPolicy == null)
            {
                ClearPolicyUi();
                return;
            }

            lblCurrentPolicy.Text = _currentPolicy.PolicyName;
            txtPolicyCode.Text = _currentPolicy.PolicyCode;
            txtPolicyName.Text = _currentPolicy.PolicyName;
            lblStatus.Text = $"{_currentPolicy.Status} v{_currentPolicy.VersionNo}";
            btnLockUnlock.Text = _currentPolicy.IsLocked ? "Unlock Editing" : "Lock Editing";

            LoadSections(selectSectionId, selectClauseId);
            LoadClauseGrid();
            SelectClauseByIdOrFirst(selectClauseId ?? _currentClause?.Id);
            UpdateEditState();
        }

        private void LoadSections(int? selectSectionId, int? selectClauseId)
        {
            _isReloadingSections = true;
            try
            {
                dgvSections.Rows.Clear();
                if (_currentPolicy == null)
                {
                    _currentSection = null;
                    return;
                }

                List<PolicySectionDefinition> sections = _currentPolicy.Sections
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.SectionCode)
                    .ToList();

                int? targetId = selectSectionId
                    ?? _currentSection?.Id
                    ?? FindSectionIdForClause(sections, selectClauseId)
                    ?? sections.FirstOrDefault()?.Id;

                int rowToSelect = 0;
                for (int i = 0; i < sections.Count; i++)
                {
                    PolicySectionDefinition section = sections[i];
                    int rowIndex = dgvSections.Rows.Add(section.SectionCode, section.Heading);
                    dgvSections.Rows[rowIndex].Tag = section;
                    if (targetId.HasValue && section.Id == targetId.Value)
                        rowToSelect = rowIndex;
                }

                if (dgvSections.Rows.Count > 0)
                {
                    dgvSections.ClearSelection();
                    dgvSections.Rows[rowToSelect].Selected = true;
                    dgvSections.CurrentCell = dgvSections.Rows[rowToSelect].Cells[0];
                    _currentSection = dgvSections.Rows[rowToSelect].Tag as PolicySectionDefinition;
                }
                else
                {
                    _currentSection = null;
                }
            }
            finally
            {
                _isReloadingSections = false;
            }
        }

        private int? FindSectionIdForClause(List<PolicySectionDefinition> sections, int? clauseId)
        {
            if (!clauseId.HasValue || _currentPolicy == null)
                return null;
            PolicyClause? clause = _currentPolicy.Clauses.FirstOrDefault(c => c.Id == clauseId.Value);
            if (clause == null)
                return null;
            PolicySectionDefinition? match = sections.FirstOrDefault(
                s => string.Equals(s.Heading, clause.PolicySection, StringComparison.OrdinalIgnoreCase));
            return match?.Id;
        }

        private void LoadClauseGrid()
        {
            dgvClauses.Rows.Clear();
            if (_currentPolicy == null)
                return;

            string? sectionHeading = _currentSection?.Heading;
            foreach ((PolicyClause clause, int level) in FlattenClauses()
                .Where(item => string.IsNullOrWhiteSpace(sectionHeading) ||
                               string.Equals(item.Clause.PolicySection, sectionHeading, StringComparison.OrdinalIgnoreCase)))
            {
                int rowIndex = dgvClauses.Rows.Add(
                    clause.ClauseCode ?? "",
                    $"{new string(' ', level * 4)}{clause.Heading}");
                dgvClauses.Rows[rowIndex].Tag = clause;
            }
        }

        private IEnumerable<(PolicyClause Clause, int Level)> FlattenClauses()
        {
            if (_currentPolicy == null)
                yield break;

            Dictionary<int, List<PolicyClause>> byParent = _currentPolicy.Clauses
                .OrderBy(c => c.DisplayOrder)
                .GroupBy(c => c.ParentClauseId ?? 0)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (PolicyClause root in byParent.GetValueOrDefault(0, []))
                foreach ((PolicyClause Clause, int Level) item in FlattenClause(root, 0, byParent))
                    yield return item;
        }

        private static IEnumerable<(PolicyClause Clause, int Level)> FlattenClause(
            PolicyClause clause,
            int level,
            Dictionary<int, List<PolicyClause>> byParent)
        {
            yield return (clause, level);
            foreach (PolicyClause child in byParent.GetValueOrDefault(clause.Id, []))
                foreach ((PolicyClause Clause, int Level) item in FlattenClause(child, level + 1, byParent))
                    yield return item;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Clause selection
        // ──────────────────────────────────────────────────────────────────────

        private void SelectFirstClause() => SelectClauseByIdOrFirst(null);

        private void SelectClauseByIdOrFirst(int? clauseId)
        {
            if (dgvClauses.Rows.Count == 0)
            {
                SelectClause(null);
                return;
            }

            DataGridViewRow? row = clauseId.HasValue
                ? dgvClauses.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(r => r.Tag is PolicyClause c && c.Id == clauseId.Value)
                : null;

            row ??= dgvClauses.Rows[0];
            dgvClauses.ClearSelection();
            row.Selected = true;
            dgvClauses.CurrentCell = row.Cells[0];
            SelectClause(row.Tag as PolicyClause);
        }

        private void SelectClauseFromGrid()
        {
            if (dgvClauses.SelectedRows.Count == 0)
                return;

            SelectClause(dgvClauses.SelectedRows[0].Tag as PolicyClause);
        }

        private void SelectClause(PolicyClause? clause)
        {
            _currentClause = clause;
            txtClauseCode.Text = clause?.ClauseCode ?? "";
            txtClauseHeading.Text = clause?.Heading ?? "";
            txtClauseSection.Text = clause?.PolicySection ?? "";
            txtClauseDescription.Text = clause?.Description ?? "";
            LoadClauseParameterGrid();
            UpdateEditState();
        }

        private void SelectClauseRowForContextMenu(DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.RowIndex >= dgvClauses.Rows.Count)
                return;

            dgvClauses.ClearSelection();
            dgvClauses.Rows[e.RowIndex].Selected = true;
            dgvClauses.CurrentCell = dgvClauses.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
            SelectClauseFromGrid();
            UpdateEditState();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Parameter grid population
        // ──────────────────────────────────────────────────────────────────────

        private void LoadClauseParameterGrid()
        {
            dgvParameters.Rows.Clear();
            if (_currentPolicy == null || _currentClause == null)
                return;

            foreach (PolicyParameter parameter in _currentPolicy.Parameters
                .Where(p => p.PolicyClauseId == _currentClause.Id)
                .OrderBy(p => p.DisplayOrder))
            {
                int rowIndex = dgvParameters.Rows.Add(
                    parameter.Label,
                    parameter.ValueText ?? "",
                    parameter.Unit ?? "",
                    parameter.Description ?? "");
                dgvParameters.Rows[rowIndex].Tag = parameter;
            }

            SelectParameterByIdOrFirst(_selectedParameterIdBeforeRefresh);
            _selectedParameterIdBeforeRefresh = null;
        }

        private void SelectParameterByIdOrFirst(int? parameterId)
        {
            if (dgvParameters.Rows.Count == 0)
                return;

            DataGridViewRow? row = parameterId.HasValue
                ? dgvParameters.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(r => r.Tag is PolicyParameter p && p.Id == parameterId.Value)
                : null;

            row ??= dgvParameters.Rows[0];
            dgvParameters.ClearSelection();
            row.Selected = true;
            dgvParameters.CurrentCell = row.Cells[0];
        }

        // ──────────────────────────────────────────────────────────────────────
        // Policy operations
        // ──────────────────────────────────────────────────────────────────────

        private async Task ManagePoliciesAsync()
        {
            using frmPolicyListManager manager = new(_service, _readOnlyMode, _currentPolicy?.Id);
            if (manager.ShowDialog(this) == DialogResult.OK && manager.SelectedPolicyId.HasValue)
                await ReloadPoliciesAsync(manager.SelectedPolicyId.Value);
        }

        private async Task SaveCurrentEditorsAsync()
        {
            // Header fields and clause text fields are read-only in this view; Save Draft is
            // kept for downstream "saved" audit + refresh of any structural edits queued by
            // section/clause/parameter buttons that already persisted on their own.
            if (_currentPolicy == null || !IsCurrentEditable())
                return;

            await ReloadPoliciesAsync(_currentPolicy.Id, _currentClause?.Id, SelectedParameterId(), _currentSection?.Id);
            Notify("Draft refreshed");
        }

        private async Task AddSiblingClauseAsync() => await AddClauseAsync(_currentClause?.ParentClauseId);

        private async Task AddClauseAsync(int? parentClauseId)
        {
            if (_currentPolicy == null || !IsCurrentEditable())
                return;

            string section = _currentSection?.Heading
                ?? _currentClause?.PolicySection
                ?? "General";

            PolicyClause clause = new()
            {
                PolicySetId = _currentPolicy.Id,
                ParentClauseId = parentClauseId,
                ClauseCode = null,
                Heading = parentClauseId.HasValue ? "New Sub-Clause" : "New Clause",
                Description = "",
                PolicySection = section,
                DisplayOrder = 0
            };
            PolicyClause created = await RunServiceAsync(() => _service.SaveClauseAsync(clause));
            await ReloadPoliciesAsync(_currentPolicy.Id, created.Id, null, _currentSection?.Id);
            Notify("Clause added");
        }

        private async Task DuplicateClauseAsync()
        {
            if (_currentClause == null || !IsCurrentEditable())
                return;

            PolicyClause? duplicate = await RunServiceAsync(() => _service.DuplicateClauseAsync(_currentClause.Id));
            await ReloadPoliciesAsync(_currentPolicy!.Id, duplicate?.Id ?? _currentClause.Id, null, _currentSection?.Id);
            Notify("Clause duplicated");
        }

        private async Task DeleteClauseAsync()
        {
            if (_currentClause == null || !IsCurrentEditable())
                return;

            if (MessageBox.Show("Delete selected clause and its sub-clauses?", "Policy Manager",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            int? parentClauseId = _currentClause.ParentClauseId;
            await RunServiceAsync(() => _service.DeleteClauseAsync(_currentClause.Id));
            await ReloadPoliciesAsync(_currentPolicy!.Id, parentClauseId, null, _currentSection?.Id);
            Notify("Clause deleted");
        }

        private async Task MoveClauseAsync(int direction)
        {
            if (_currentClause == null || !IsCurrentEditable())
                return;

            await RunServiceAsync(() => _service.MoveClauseAsync(_currentClause.Id, direction));
            await ReloadPoliciesAsync(_currentPolicy!.Id, _currentClause.Id, null, _currentSection?.Id);
            Notify("Clause reordered");
        }

        private async Task AddParameterAsync()
        {
            if (_currentPolicy == null || _currentClause == null || !IsCurrentEditable())
                return;

            PolicyParameter parameter = new()
            {
                PolicySetId = _currentPolicy.Id,
                PolicyClauseId = _currentClause.Id,
                ParameterKey = $"clauseParameter{DateTime.Now:HHmmssfff}",
                Label = "New Parameter",
                ValueType = "Text",
                ValueText = "",
                Unit = "",
                Description = "",
                DisplayOrder = _currentPolicy.Parameters.Count + 1
            };
            PolicyParameter created = await RunServiceAsync(() => _service.SaveParameterAsync(parameter));
            await ReloadPoliciesAsync(_currentPolicy.Id, _currentClause.Id, created.Id, _currentSection?.Id);
            Notify("Parameter added");
        }

        private async Task DeleteParameterAsync()
        {
            if (!IsCurrentEditable() || dgvParameters.SelectedRows.Count == 0)
                return;

            if (dgvParameters.SelectedRows[0].Tag is not PolicyParameter parameter)
                return;

            await RunServiceAsync(() => _service.DeleteParameterAsync(parameter.Id));
            await ReloadPoliciesAsync(_currentPolicy!.Id, _currentClause?.Id, null, _currentSection?.Id);
            Notify("Parameter deleted");
        }

        private async Task SaveParameterRowAsync(int rowIndex)
        {
            // The parameter grid is read-only in this view; this handler still exists for
            // future inline editing — when ReadOnly is flipped off it will persist edits.
            if (!IsCurrentEditable() || rowIndex < 0 || rowIndex >= dgvParameters.Rows.Count)
                return;

            DataGridViewRow row = dgvParameters.Rows[rowIndex];
            if (row.Tag is not PolicyParameter parameter)
                return;

            parameter.PolicyClauseId = _currentClause?.Id;
            parameter.Label = string.IsNullOrWhiteSpace(Value(row, 0)) ? "Parameter" : Value(row, 0);
            parameter.ValueText = Value(row, 1);
            parameter.Unit = NullIfBlank(Value(row, 2));
            parameter.Description = Value(row, 3);
            _ = await RunServiceAsync(() => _service.SaveParameterAsync(parameter));
            Notify("Parameter saved");
        }

        private async Task ApproveAsync()
        {
            if (_currentPolicy == null || _readOnlyMode)
                return;

            try
            {
                await RunServiceAsync(() => _service.ApprovePolicyAsync(_currentPolicy.Id));
                await ReloadPoliciesAsync(_currentPolicy.Id, _currentClause?.Id, SelectedParameterId(), _currentSection?.Id);
                Notify("Policy approved");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Approval Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                await RefreshValidationAsync(true);
            }
        }

        private async Task ToggleLockAsync()
        {
            if (_currentPolicy == null || _readOnlyMode)
                return;

            if (string.Equals(_currentPolicy.Status, PolicyStatuses.Approved, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Approved policies cannot be unlocked directly. Create a new draft version from the policy selector.",
                    "Policy Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            bool targetLockState = !_currentPolicy.IsLocked;
            await RunServiceAsync(() => _service.SetPolicyLockAsync(_currentPolicy.Id, targetLockState));
            await ReloadPoliciesAsync(_currentPolicy.Id, _currentClause?.Id, SelectedParameterId(), _currentSection?.Id);
            Notify(targetLockState ? "Draft locked" : "Draft unlocked");
        }

        private async Task ExportAsync()
        {
            if (_currentPolicy == null)
                return;

            using SaveFileDialog dialog = new()
            {
                Filter = "RePlot Policy Package (*.rpolicy)|*.rpolicy",
                FileName = $"{_currentPolicy.PolicyCode}-v{_currentPolicy.VersionNo}.rpolicy"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            await RunServiceAsync(() => _service.ExportAsync(_currentPolicy.Id, dialog.FileName));
            await ReloadPoliciesAsync(_currentPolicy.Id, _currentClause?.Id, SelectedParameterId(), _currentSection?.Id);
            Notify("Policy exported");
        }

        private void OpenDiagram()
        {
            if (_currentPolicy == null || _currentClause == null)
                return;

            using frmPolicyClauseDiagram diagram = new(
                _service,
                _currentPolicy.Id,
                _currentClause.Id,
                $"{_currentClause.ClauseCode} - {_currentClause.Heading}",
                IsCurrentEditable());
            diagram.ShowDialog(this);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Validation
        // ──────────────────────────────────────────────────────────────────────

        private async Task RefreshValidationAsync(bool approvalMode)
        {
            if (_currentPolicy == null)
                return;

            PolicySet? policy = await RunServiceAsync(() => _service.GetPolicyAsync(_currentPolicy.Id));
            if (policy != null)
                _currentPolicy = policy;

            RefreshValidationFromLoadedPolicy(approvalMode);
        }

        private void RefreshValidationFromLoadedPolicy(bool approvalMode)
        {
            lstValidation.Items.Clear();
            if (_currentPolicy == null)
                return;

            List<string> issues = _service.ValidateLoadedPolicy(_currentPolicy, approvalMode);
            if (issues.Count == 0)
            {
                lstValidation.Items.Add(approvalMode ? "Policy is ready for approval." : "No draft warnings.");
                return;
            }

            foreach (string issue in issues)
                lstValidation.Items.Add(issue);
        }

        private void ShowDashboardValidationPlaceholder()
        {
            lstValidation.Items.Clear();
            if (_currentPolicy == null)
                return;

            lstValidation.Items.Add("Policy editor loaded.");
            lstValidation.Items.Add("Full validation runs during approval.");
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit state
        // ──────────────────────────────────────────────────────────────────────

        private void UpdateEditState()
        {
            bool editable = IsCurrentEditable();
            bool hasPolicy = _currentPolicy != null;
            bool hasClause = _currentClause != null;
            bool hasSection = _currentSection != null;

            btnSaveDraft.Enabled = editable;
            btnApprove.Enabled = !_readOnlyMode && hasPolicy && editable;
            btnLockUnlock.Enabled = !_readOnlyMode && hasPolicy;
            btnExport.Enabled = hasPolicy;

            btnAddSection.Enabled = editable;
            btnRenameSection.Enabled = editable && hasSection;
            btnDeleteSection.Enabled = editable && hasSection;

            btnAddClause.Enabled = editable && hasSection;
            btnAddSubClause.Enabled = editable && hasClause;
            btnDuplicateClause.Enabled = editable && hasClause;
            btnDeleteClause.Enabled = editable && hasClause;
            btnMoveUp.Enabled = editable && hasClause;
            btnMoveDown.Enabled = editable && hasClause;
            btnOpenDiagram.Enabled = hasClause;
            btnAddParameter.Enabled = editable && hasClause;
            btnDeleteParameter.Enabled = editable && dgvParameters.SelectedRows.Count > 0;

            menuAddClause.Enabled = editable && hasSection;
            menuAddSubClause.Enabled = editable && hasClause;
            menuDuplicateClause.Enabled = editable && hasClause;
            menuDeleteClause.Enabled = editable && hasClause;
        }

        private bool IsCurrentEditable() =>
            !_readOnlyMode &&
            _currentPolicy != null &&
            PolicyValidationService.IsEditable(_currentPolicy);

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        private async Task RunPolicyUiOperationAsync(string status, Func<Task> operation)
        {
            try
            {
                UseWaitCursor = true;
                Notify(status);
                await operation();
            }
            catch (Exception ex)
            {
                Notify("Policy Manager operation failed");
                lstValidation.Items.Clear();
                lstValidation.Items.Add(ex.Message);
                MessageBox.Show(ex.Message, "Policy Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task<T> RunServiceAsync<T>(Func<Task<T>> operation) =>
            await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));

        private async Task RunServiceAsync(Func<Task> operation) =>
            await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));

        private static string Value(DataGridViewRow row, int index) =>
            Convert.ToString(row.Cells[index].Value)?.Trim() ?? "";

        private static string? NullIfBlank(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private int? SelectedParameterId()
        {
            if (dgvParameters.SelectedRows.Count == 0 ||
                dgvParameters.SelectedRows[0].Tag is not PolicyParameter parameter)
                return null;

            return parameter.Id;
        }

        private void Notify(string message) => StatusChanged?.Invoke(this, message);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _service.PolicyChanged -= PolicyManagerService_PolicyChanged;
            base.OnFormClosed(e);
        }
    }
}
