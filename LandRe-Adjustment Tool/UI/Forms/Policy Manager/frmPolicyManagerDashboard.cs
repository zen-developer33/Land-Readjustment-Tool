using Land_Readjustment_Tool.Core.Entities.Policy;
using Land_Readjustment_Tool.Services.Policy;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmPolicyManagerDashboard : Form
    {
        private readonly PolicyManagerService _service;
        private readonly bool _readOnlyMode;
        private List<PolicySet> _policySummaries = [];
        private PolicySet? _currentPolicy;
        private PolicyClause? _currentClause;
        private bool _isReloadingPolicies;
        private bool _isReloadingLookups;
        private int _attachmentLoadVersion;

        public event EventHandler<string>? StatusChanged;

        public frmPolicyManagerDashboard(
            PolicyManagerService service,
            bool readOnlyMode)
        {
            _service = service;
            _readOnlyMode = readOnlyMode;
            InitializeComponent();
            RecordFormTheme.Apply(this);
        }

        private async void frmPolicyManagerDashboard_Load(object? sender, EventArgs e)
        {
            Notify("Loading policies...");
            lstValidation.Items.Clear();
            lstValidation.Items.Add("Loading policy standards...");
            BeginInvoke(new Action(async () => await ReloadPoliciesSafelyAsync()));
        }

        private async void cboPolicies_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isReloadingPolicies)
                return;

            await LoadSelectedPolicySafelyAsync();
        }
        private async void btnNewPolicy_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Creating policy...", CreatePolicyAsync);
        private async void btnSaveDraft_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Saving draft...", SaveCurrentEditorsAsync);
        private async void btnNewDraft_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Creating draft version...", CreateDraftFromApprovedAsync);
        private async void btnApprove_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Approving policy...", ApproveAsync);
        private async void btnLockUnlock_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Changing edit lock...", ToggleLockAsync);
        private async void btnImport_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Importing policy...", ImportAsync);
        private async void btnExport_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Exporting policy...", ExportAsync);
        private void dgvClauses_SelectionChanged(object? sender, EventArgs e) => SelectClauseFromGrid();
        private async void btnAddClause_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Adding clause...", () => AddClauseAsync(null));
        private async void btnAddSubClause_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Adding sub-clause...", () => AddClauseAsync(_currentClause?.Id));
        private async void btnDuplicateClause_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Duplicating clause...", DuplicateClauseAsync);
        private async void btnDeleteClause_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Deleting clause...", DeleteClauseAsync);
        private async void btnMoveUp_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Moving clause...", () => MoveClauseAsync(-1));
        private async void btnMoveDown_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Moving clause...", () => MoveClauseAsync(1));
        private async void btnAttachImage_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Attaching image...", AttachImageAsync);
        private async void btnAddParameter_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Adding parameter...", AddParameterAsync);
        private async void btnDeleteParameter_Click(object? sender, EventArgs e) => await RunPolicyUiOperationAsync("Deleting parameter...", DeleteParameterAsync);
        private void cboLookupTables_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isReloadingLookups)
                return;

            LoadSelectedLookupTable();
        }
        private async void dgvParameters_CellEndEdit(object? sender, DataGridViewCellEventArgs e) => await RunPolicyUiOperationAsync("Saving parameter...", () => SaveParameterRowAsync(e.RowIndex));
        private async void dgvLookup_CellEndEdit(object? sender, DataGridViewCellEventArgs e) => await RunPolicyUiOperationAsync("Saving lookup value...", () => SaveLookupCellAsync(e.RowIndex, e.ColumnIndex));

        private async Task ReloadPoliciesAsync(int? selectPolicyId = null)
        {
            _isReloadingPolicies = true;
            try
            {
                _policySummaries = await RunServiceAsync(() => _service.GetPolicySummariesAsync());
                cboPolicies.Items.Clear();
                foreach (PolicySet policy in _policySummaries)
                    cboPolicies.Items.Add(FormatPolicy(policy));

                int index = 0;
                if (selectPolicyId.HasValue)
                    index = Math.Max(0, _policySummaries.FindIndex(p => p.Id == selectPolicyId.Value));

                if (cboPolicies.Items.Count > 0)
                    cboPolicies.SelectedIndex = index;
                else
                {
                    _currentPolicy = null;
                    _currentClause = null;
                    ClearPolicyUi();
                }
            }
            finally
            {
                _isReloadingPolicies = false;
            }

            await LoadSelectedPolicyAsync();
        }

        private async Task LoadSelectedPolicyAsync()
        {
            if (cboPolicies.SelectedIndex < 0 || cboPolicies.SelectedIndex >= _policySummaries.Count)
                return;

            int policyId = _policySummaries[cboPolicies.SelectedIndex].Id;
            Notify("Loading selected policy...");
            _currentPolicy = await RunServiceAsync(() => _service.GetPolicyDashboardAsync(policyId));
            _currentClause = null;
            LoadPolicyToUi();
            ShowDashboardValidationPlaceholder();
            Notify("Policy loaded");
        }

        private async Task ReloadPoliciesSafelyAsync(int? selectPolicyId = null)
        {
            await RunPolicyUiOperationAsync(
                "Loading policies...",
                () => ReloadPoliciesAsync(selectPolicyId));
        }

        private async Task LoadSelectedPolicySafelyAsync()
        {
            await RunPolicyUiOperationAsync(
                "Loading selected policy...",
                LoadSelectedPolicyAsync);
        }

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
                Notify("Policy Manager load failed");
                lstValidation.Items.Clear();
                lstValidation.Items.Add(ex.Message);
                MessageBox.Show(
                    ex.Message,
                    "Policy Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task<T> RunServiceAsync<T>(Func<Task<T>> operation)
        {
            return await Task.Run(async () =>
            {
                return await _service.RunExclusiveAsync(operation).ConfigureAwait(false);
            });
        }

        private async Task RunServiceAsync(Func<Task> operation)
        {
            await Task.Run(async () =>
            {
                await _service.RunExclusiveAsync(operation).ConfigureAwait(false);
            });
        }

        private void ClearPolicyUi()
        {
            txtPolicyCode.Clear();
            txtPolicyName.Clear();
            lblStatus.Text = "No policy";
            dgvClauses.Rows.Clear();
            dgvParameters.Rows.Clear();
            dgvLookup.Columns.Clear();
            dgvLookup.Rows.Clear();
            dgvAudit.Rows.Clear();
            cboLookupTables.Items.Clear();
            ClearPictureBoxImage();
            SelectClause(null);
        }

        private void LoadPolicyToUi()
        {
            if (_currentPolicy == null)
                return;

            txtPolicyCode.Text = _currentPolicy.PolicyCode;
            txtPolicyName.Text = _currentPolicy.PolicyName;
            lblStatus.Text = $"{_currentPolicy.Status} v{_currentPolicy.VersionNo}";
            bool editable = IsCurrentEditable();
            txtPolicyCode.ReadOnly = !editable;
            txtPolicyName.ReadOnly = !editable;
            btnLockUnlock.Text = _currentPolicy.IsLocked ? "Unlock Editing" : "Lock Editing";
            btnLockUnlock.Enabled = !_readOnlyMode;

            LoadClauseGrid();
            SelectFirstClause();
        }

        private void LoadClauseGrid()
        {
            dgvClauses.Rows.Clear();
            if (_currentPolicy == null)
                return;

            foreach ((PolicyClause Clause, int Level) item in FlattenClauses())
            {
                int rowIndex = dgvClauses.Rows.Add(
                    item.Clause.ClauseCode ?? "",
                    item.Clause.PolicySection,
                    $"{new string(' ', item.Level * 4)}{item.Clause.Heading}");
                dgvClauses.Rows[rowIndex].Tag = item.Clause;
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
            {
                foreach ((PolicyClause Clause, int Level) item in FlattenClause(root, 0, byParent))
                    yield return item;
            }
        }

        private static IEnumerable<(PolicyClause Clause, int Level)> FlattenClause(
            PolicyClause clause,
            int level,
            Dictionary<int, List<PolicyClause>> byParent)
        {
            yield return (clause, level);
            foreach (PolicyClause child in byParent.GetValueOrDefault(clause.Id, []))
            {
                foreach ((PolicyClause Clause, int Level) item in FlattenClause(child, level + 1, byParent))
                    yield return item;
            }
        }

        private void SelectFirstClause()
        {
            if (dgvClauses.Rows.Count > 0)
                dgvClauses.Rows[0].Selected = true;
            else
                SelectClause(null);
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
            bool editable = IsCurrentEditable() && clause != null;
            txtClauseCode.ReadOnly = !editable;
            txtClauseHeading.ReadOnly = !editable;
            txtClauseSection.ReadOnly = !editable;
            txtClauseDescription.ReadOnly = !editable;
            _ = LoadClauseAttachmentSafelyAsync(_currentPolicy?.Id, clause?.Id);
        }

        private async Task LoadClauseAttachmentSafelyAsync(int? policySetId, int? clauseId)
        {
            int loadVersion = ++_attachmentLoadVersion;
            ClearPictureBoxImage();

            if (!policySetId.HasValue)
                return;

            try
            {
                byte[]? imageData = await RunServiceAsync(() =>
                    _service.GetPolicyAttachmentImageDataAsync(policySetId.Value, clauseId));

                if (loadVersion != _attachmentLoadVersion || imageData is not { Length: > 0 })
                    return;

                Image? image = await Task.Run(() =>
                {
                    using MemoryStream stream = new(imageData);
                    return Image.FromStream(stream);
                });

                if (loadVersion != _attachmentLoadVersion)
                {
                    image.Dispose();
                    return;
                }

                Image? oldImage = pictureBox.Image;
                pictureBox.Image = image;
                oldImage?.Dispose();
            }
            catch (Exception ex)
            {
                if (loadVersion == _attachmentLoadVersion)
                    Notify($"Image load skipped: {ex.Message}");
            }
        }

        private void ClearPictureBoxImage()
        {
            Image? oldImage = pictureBox.Image;
            pictureBox.Image = null;
            oldImage?.Dispose();
        }

        private void LoadParameterGrid()
        {
            dgvParameters.Rows.Clear();
            if (_currentPolicy == null)
                return;

            foreach (PolicyParameter parameter in _currentPolicy.Parameters.OrderBy(p => p.DisplayOrder))
            {
                int rowIndex = dgvParameters.Rows.Add(
                    parameter.ParameterKey ?? "",
                    parameter.Label,
                    parameter.ValueType,
                    parameter.ValueText ?? "",
                    parameter.Unit ?? "",
                    parameter.Description ?? "");
                dgvParameters.Rows[rowIndex].Tag = parameter;
            }
            dgvParameters.ReadOnly = !IsCurrentEditable();
        }

        private void LoadLookupSelector()
        {
            _isReloadingLookups = true;
            cboLookupTables.Items.Clear();
            try
            {
                if (_currentPolicy == null)
                    return;

                foreach (PolicyLookupTable table in _currentPolicy.LookupTables.OrderBy(t => t.DisplayOrder))
                    cboLookupTables.Items.Add(table.Title);

                if (cboLookupTables.Items.Count > 0)
                    cboLookupTables.SelectedIndex = 0;
                else
                    dgvLookup.Rows.Clear();
            }
            finally
            {
                _isReloadingLookups = false;
            }

            if (cboLookupTables.Items.Count > 0)
                LoadSelectedLookupTable();
            else
                dgvLookup.Rows.Clear();
        }

        private void LoadSelectedLookupTable()
        {
            dgvLookup.Columns.Clear();
            dgvLookup.Rows.Clear();

            PolicyLookupTable? table = SelectedLookupTable();
            if (table == null)
                return;

            List<PolicyLookupColumn> columns = table.Columns.OrderBy(c => c.DisplayOrder).ToList();
            foreach (PolicyLookupColumn column in columns)
            {
                DataGridViewTextBoxColumn gridColumn = new()
                {
                    Name = column.ColumnKey,
                    HeaderText = column.HeaderText,
                    Width = LookupColumnWidth(column),
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Tag = column
                };
                dgvLookup.Columns.Add(gridColumn);
            }

            foreach (PolicyLookupRow row in table.Rows.OrderBy(r => r.DisplayOrder))
            {
                int rowIndex = dgvLookup.Rows.Add();
                dgvLookup.Rows[rowIndex].Tag = row;
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    PolicyLookupCell? cell = row.Cells
                        .FirstOrDefault(c => c.PolicyLookupColumnId == columns[columnIndex].Id);
                    dgvLookup.Rows[rowIndex].Cells[columnIndex].Value = cell?.ValueText;
                    dgvLookup.Rows[rowIndex].Cells[columnIndex].Tag = cell?.Id;
                }
            }

            dgvLookup.ReadOnly = !IsCurrentEditable();
        }

        private static int LookupColumnWidth(PolicyLookupColumn column)
        {
            if (column.HeaderText.Length <= 6)
                return 75;
            if (column.ValueType == "Text")
                return 220;
            return 110;
        }

        private PolicyLookupTable? SelectedLookupTable()
        {
            if (_currentPolicy == null ||
                cboLookupTables.SelectedIndex < 0 ||
                cboLookupTables.SelectedIndex >= _currentPolicy.LookupTables.Count)
            {
                return null;
            }

            return _currentPolicy.LookupTables
                .OrderBy(t => t.DisplayOrder)
                .ElementAt(cboLookupTables.SelectedIndex);
        }

        private void LoadAuditGrid()
        {
            dgvAudit.Rows.Clear();
            if (_currentPolicy == null)
                return;

            foreach (PolicyAuditEntry audit in _currentPolicy.AuditEntries.OrderByDescending(a => a.CreatedDate))
            {
                dgvAudit.Rows.Add(
                    audit.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                    audit.Action,
                    audit.Details ?? "");
            }
        }

        private async Task SaveCurrentEditorsAsync()
        {
            if (_currentPolicy == null || !IsCurrentEditable())
                return;

            _currentPolicy.PolicyCode = txtPolicyCode.Text.Trim();
            _currentPolicy.PolicyName = txtPolicyName.Text.Trim();
            await RunServiceAsync(() => _service.SavePolicyMetadataAsync(_currentPolicy));

            if (_currentClause != null)
            {
                _currentClause.ClauseCode = NullIfBlank(txtClauseCode.Text);
                _currentClause.Heading = txtClauseHeading.Text.Trim();
                _currentClause.PolicySection = string.IsNullOrWhiteSpace(txtClauseSection.Text)
                    ? "General"
                    : txtClauseSection.Text.Trim();
                _currentClause.Description = txtClauseDescription.Text;
                _ = await RunServiceAsync(() => _service.SaveClauseAsync(_currentClause));
            }

            await ReloadPoliciesAsync(_currentPolicy.Id);
            Notify("Draft saved");
        }

        private async Task AddClauseAsync(int? parentClauseId)
        {
            if (_currentPolicy == null || !IsCurrentEditable())
                return;

            PolicyClause clause = new()
            {
                PolicySetId = _currentPolicy.Id,
                ParentClauseId = parentClauseId,
                ClauseCode = "",
                Heading = parentClauseId.HasValue ? "New Sub-Clause" : "New Clause",
                Description = "",
                PolicySection = parentClauseId.HasValue && _currentClause != null
                    ? _currentClause.PolicySection
                    : "General",
                DisplayOrder = _currentPolicy.Clauses.Count + 1
            };
            _ = await RunServiceAsync(() => _service.SaveClauseAsync(clause));
            await ReloadPoliciesAsync(_currentPolicy.Id);
            Notify("Clause added");
        }

        private async Task DuplicateClauseAsync()
        {
            if (_currentClause == null || !IsCurrentEditable())
                return;

            await RunServiceAsync(() => _service.DuplicateClauseAsync(_currentClause.Id));
            await ReloadPoliciesAsync(_currentPolicy!.Id);
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

            await RunServiceAsync(() => _service.DeleteClauseAsync(_currentClause.Id));
            await ReloadPoliciesAsync(_currentPolicy!.Id);
            Notify("Clause deleted");
        }

        private async Task MoveClauseAsync(int direction)
        {
            if (_currentClause == null || !IsCurrentEditable())
                return;

            await RunServiceAsync(() => _service.MoveClauseAsync(_currentClause.Id, direction));
            await ReloadPoliciesAsync(_currentPolicy!.Id);
            Notify("Clause reordered");
        }

        private async Task AddParameterAsync()
        {
            if (_currentPolicy == null || !IsCurrentEditable())
                return;

            PolicyParameter parameter = new()
            {
                PolicySetId = _currentPolicy.Id,
                PolicyClauseId = _currentClause?.Id,
                ParameterKey = "",
                Label = "New Parameter",
                ValueType = "Text",
                ValueText = "",
                DisplayOrder = _currentPolicy.Parameters.Count + 1
            };
            _ = await RunServiceAsync(() => _service.SaveParameterAsync(parameter));
            await ReloadPoliciesAsync(_currentPolicy.Id);
            Notify("Parameter added");
        }

        private async Task DeleteParameterAsync()
        {
            if (!IsCurrentEditable() || dgvParameters.SelectedRows.Count == 0)
                return;

            if (dgvParameters.SelectedRows[0].Tag is not PolicyParameter parameter)
                return;

            await RunServiceAsync(() => _service.DeleteParameterAsync(parameter.Id));
            await ReloadPoliciesAsync(_currentPolicy!.Id);
            Notify("Parameter deleted");
        }

        private async Task SaveParameterRowAsync(int rowIndex)
        {
            if (!IsCurrentEditable() || rowIndex < 0 || rowIndex >= dgvParameters.Rows.Count)
                return;

            DataGridViewRow row = dgvParameters.Rows[rowIndex];
            if (row.Tag is not PolicyParameter parameter)
                return;

            parameter.ParameterKey = NullIfBlank(Value(row, 0));
            parameter.Label = string.IsNullOrWhiteSpace(Value(row, 1)) ? "Parameter" : Value(row, 1);
            parameter.ValueType = string.IsNullOrWhiteSpace(Value(row, 2)) ? "Text" : Value(row, 2);
            parameter.ValueText = Value(row, 3);
            parameter.Unit = NullIfBlank(Value(row, 4));
            parameter.Description = Value(row, 5);
            _ = await RunServiceAsync(() => _service.SaveParameterAsync(parameter));
            Notify("Parameter saved");
        }

        private async Task SaveLookupCellAsync(int rowIndex, int columnIndex)
        {
            if (!IsCurrentEditable() ||
                rowIndex < 0 ||
                columnIndex < 0 ||
                rowIndex >= dgvLookup.Rows.Count ||
                columnIndex >= dgvLookup.Columns.Count)
            {
                return;
            }

            object? tag = dgvLookup.Rows[rowIndex].Cells[columnIndex].Tag;
            if (tag is not int cellId)
                return;

            string? value = Convert.ToString(dgvLookup.Rows[rowIndex].Cells[columnIndex].Value);
            await RunServiceAsync(() => _service.SaveLookupCellAsync(cellId, value));
            Notify("Lookup cell saved");
        }

        private async Task CreatePolicyAsync()
        {
            PolicySet policy = await RunServiceAsync(() => _service.CreatePolicyAsync(
                $"New Policy {DateTime.Now:yyyyMMdd-HHmm}",
                $"POL-{DateTime.Now:yyyyMMddHHmm}"));
            await ReloadPoliciesAsync(policy.Id);
            Notify("New policy created");
        }

        private async Task CreateDraftFromApprovedAsync()
        {
            if (_currentPolicy == null)
                return;

            PolicySet draft = await RunServiceAsync(() => _service.CreateDraftFromApprovedAsync(_currentPolicy.Id));
            await ReloadPoliciesAsync(draft.Id);
            Notify("Draft version created");
        }

        private async Task ApproveAsync()
        {
            if (_currentPolicy == null || _readOnlyMode)
                return;

            try
            {
                await RunServiceAsync(() => _service.ApprovePolicyAsync(_currentPolicy.Id));
                await ReloadPoliciesAsync(_currentPolicy.Id);
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
                    "Approved policies cannot be unlocked directly. Create a new draft version to edit.",
                    "Policy Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            bool targetLockState = !_currentPolicy.IsLocked;
            await RunServiceAsync(() => _service.SetPolicyLockAsync(_currentPolicy.Id, targetLockState));
            await ReloadPoliciesAsync(_currentPolicy.Id);
            Notify(targetLockState ? "Draft locked" : "Draft unlocked");
        }

        private async Task ImportAsync()
        {
            if (_readOnlyMode)
                return;

            using OpenFileDialog dialog = new()
            {
                Filter = "RePlot Policy Package (*.rpolicy)|*.rpolicy|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            PolicySet policy = await RunServiceAsync(() => _service.ImportAsync(dialog.FileName));
            await ReloadPoliciesAsync(policy.Id);
            Notify("Policy imported");
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
            await ReloadPoliciesAsync(_currentPolicy.Id);
            Notify("Policy exported");
        }

        private async Task AttachImageAsync()
        {
            if (_currentPolicy == null || !IsCurrentEditable())
                return;

            using OpenFileDialog dialog = new()
            {
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            await RunServiceAsync(() => _service.AddAttachmentAsync(
                _currentPolicy.Id,
                _currentClause?.Id,
                dialog.FileName,
                _currentClause?.Heading));
            await ReloadPoliciesAsync(_currentPolicy.Id);
            Notify("Image attached");
        }

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

            lstValidation.Items.Add("Dashboard loaded with policy clauses only.");
            lstValidation.Items.Add("Full validation runs during approval or from the validation action.");
        }

        private bool IsCurrentEditable()
        {
            return !_readOnlyMode &&
                   _currentPolicy != null &&
                   PolicyValidationService.IsEditable(_currentPolicy);
        }

        private static string FormatPolicy(PolicySet policy)
        {
            return $"{policy.PolicyName}  |  v{policy.VersionNo}  |  {policy.Status}";
        }

        private static string Value(DataGridViewRow row, int index)
        {
            return Convert.ToString(row.Cells[index].Value)?.Trim() ?? "";
        }

        private static string? NullIfBlank(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private void Notify(string message)
        {
            StatusChanged?.Invoke(this, message);
        }
    }
}
