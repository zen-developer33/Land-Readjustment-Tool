using Land_Readjustment_Tool.Core.Entities.Policy;
using Land_Readjustment_Tool.Services.Policy;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmPolicyListManager : Form
    {
        private readonly PolicyManagerService _service;
        private readonly bool _readOnlyMode;
        private readonly int? _initialPolicyId;
        private List<PolicySet> _policies = [];
        private bool _loading;

        public int? SelectedPolicyId { get; private set; }

        public frmPolicyListManager(
            PolicyManagerService service,
            bool readOnlyMode,
            int? initialPolicyId)
        {
            _service = service;
            _readOnlyMode = readOnlyMode;
            _initialPolicyId = initialPolicyId;
            InitializeComponent();
            RecordFormTheme.Apply(this);
        }

        private async void frmPolicyListManager_Load(object? sender, EventArgs e)
        {
            await ReloadPoliciesAsync(_initialPolicyId);
            UpdateEditState();
        }

        private void lstPolicies_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loading)
                return;

            PolicySet? policy = SelectedPolicy();
            txtPolicyName.Text = policy?.PolicyName ?? "";
            UpdateEditState();
        }

        private void lstPolicies_DoubleClick(object? sender, EventArgs e) => SelectAndClose();
        private void btnSelect_Click(object? sender, EventArgs e) => SelectAndClose();
        private async void btnNew_Click(object? sender, EventArgs e) => await RunUiOperationAsync(CreatePolicyAsync);
        private async void btnCopy_Click(object? sender, EventArgs e) => await RunUiOperationAsync(CopyPolicyAsync);
        private async void btnDraftFromApproved_Click(object? sender, EventArgs e) => await RunUiOperationAsync(CreateDraftFromApprovedAsync);
        private async void btnRename_Click(object? sender, EventArgs e) => await RunUiOperationAsync(RenamePolicyAsync);
        private async void btnDelete_Click(object? sender, EventArgs e) => await RunUiOperationAsync(DeletePolicyAsync);
        private async void btnImport_Click(object? sender, EventArgs e) => await RunUiOperationAsync(ImportPolicyAsync);
        private void btnClose_Click(object? sender, EventArgs e) => Close();

        private async Task ReloadPoliciesAsync(int? selectPolicyId = null)
        {
            _loading = true;
            try
            {
                _policies = await RunServiceAsync(() => _service.GetPolicySummariesAsync());
                lstPolicies.Items.Clear();
                foreach (PolicySet policy in _policies)
                    lstPolicies.Items.Add(policy.PolicyName);

                if (_policies.Count == 0)
                    return;

                int index = selectPolicyId.HasValue
                    ? _policies.FindIndex(p => p.Id == selectPolicyId.Value)
                    : 0;
                lstPolicies.SelectedIndex = Math.Max(0, index);
                txtPolicyName.Text = _policies[lstPolicies.SelectedIndex].PolicyName;
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task CreatePolicyAsync()
        {
            if (_readOnlyMode)
                return;

            // Always auto-generate a unique default name — ignore the textbox so the user
            // doesn't accidentally overwrite an existing policy's name slot. They can rename
            // afterwards.
            string name = GenerateUniqueName("New Policy");
            PolicySet policy = await RunServiceAsync(() => _service.CreatePolicyAsync(
                name,
                $"POL-{DateTime.Now:yyyyMMddHHmm}"));
            await ReloadPoliciesAsync(policy.Id);
        }

        private async Task CopyPolicyAsync()
        {
            if (_readOnlyMode || SelectedPolicy() == null)
                return;

            PolicySet selected = SelectedPolicy()!;
            // Always append "(copy)" — and disambiguate with "(copy 2)", "(copy 3)", ...
            // when copying the same source multiple times.
            string name = GenerateUniqueName($"{selected.PolicyName} (copy)");
            PolicySet copy = await RunServiceAsync(() => _service.CopyPolicyAsDraftAsync(selected.Id, name));
            await ReloadPoliciesAsync(copy.Id);
        }

        private string GenerateUniqueName(string baseName)
        {
            HashSet<string> existing = new(
                _policies.Select(p => p.PolicyName ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);

            if (!existing.Contains(baseName))
                return baseName;

            for (int i = 2; i < 10_000; i++)
            {
                // For "(copy)" base names, render as "(copy 2)" instead of "(copy) 2".
                string candidate = baseName.EndsWith("(copy)", StringComparison.OrdinalIgnoreCase)
                    ? $"{baseName[..^1]} {i})"
                    : $"{baseName} {i}";
                if (!existing.Contains(candidate))
                    return candidate;
            }
            return $"{baseName} {DateTime.Now:HHmmssfff}";
        }

        private async Task CreateDraftFromApprovedAsync()
        {
            if (_readOnlyMode || SelectedPolicy() == null)
                return;

            PolicySet selected = SelectedPolicy()!;
            PolicySet draft = await RunServiceAsync(() => _service.CreateDraftFromApprovedAsync(selected.Id));
            await ReloadPoliciesAsync(draft.Id);
        }

        private async Task RenamePolicyAsync()
        {
            if (_readOnlyMode || SelectedPolicy() == null)
                return;

            PolicySet selected = SelectedPolicy()!;
            await RunServiceAsync(() => _service.RenamePolicyAsync(selected.Id, txtPolicyName.Text));
            await ReloadPoliciesAsync(selected.Id);
        }

        private async Task DeletePolicyAsync()
        {
            if (_readOnlyMode || SelectedPolicy() == null)
                return;

            PolicySet selected = SelectedPolicy()!;

            // Must always keep at least one policy in the project.
            if (_policies.Count <= 1)
            {
                MessageBox.Show(
                    "At least one policy must remain in the project.\n\nCreate or import another policy before deleting this one.",
                    "Delete Policy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Locked policies (and approved policies, which are always locked) cannot be deleted.
            // The user must unlock the draft, or create a new draft from an approved policy and
            // delete that, before removing the underlying record.
            if (selected.IsLocked)
            {
                MessageBox.Show(
                    $"'{selected.PolicyName}' is locked and cannot be deleted.\n\nUnlock it from the Policy Editor first (approved policies must remain locked).",
                    "Delete Policy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                    $"Delete policy '{selected.PolicyName}'?\n\nThis removes its clauses, parameters, lookup tables, attachments, and audit entries from this project database.",
                    "Delete Policy",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            await RunServiceAsync(() => _service.DeletePolicyAsync(selected.Id));
            await ReloadPoliciesAsync();
        }

        private async Task ImportPolicyAsync()
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
        }

        private void SelectAndClose()
        {
            PolicySet? selected = SelectedPolicy();
            if (selected == null)
                return;

            SelectedPolicyId = selected.Id;
            DialogResult = DialogResult.OK;
            Close();
        }

        private PolicySet? SelectedPolicy()
        {
            if (lstPolicies.SelectedIndex < 0 || lstPolicies.SelectedIndex >= _policies.Count)
                return null;

            return _policies[lstPolicies.SelectedIndex];
        }

        private async Task RunUiOperationAsync(Func<Task> operation)
        {
            try
            {
                UseWaitCursor = true;
                await operation();
                UpdateEditState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Policy Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task<T> RunServiceAsync<T>(Func<Task<T>> operation)
        {
            return await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));
        }

        private async Task RunServiceAsync(Func<Task> operation)
        {
            await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));
        }

        private void UpdateEditState()
        {
            PolicySet? selected = SelectedPolicy();
            bool hasSelection = selected != null;
            bool editable = hasSelection && !_readOnlyMode && PolicyValidationService.IsEditable(selected!);
            bool approved = hasSelection &&
                            string.Equals(selected!.Status, PolicyStatuses.Approved, StringComparison.OrdinalIgnoreCase);

            txtPolicyName.ReadOnly = _readOnlyMode;
            btnSelect.Enabled = hasSelection;
            btnNew.Enabled = !_readOnlyMode;
            btnCopy.Enabled = !_readOnlyMode && hasSelection;
            btnDraftFromApproved.Enabled = !_readOnlyMode && approved;
            btnRename.Enabled = editable;
            btnDelete.Enabled = !_readOnlyMode
                && hasSelection
                && !selected!.IsLocked
                && _policies.Count > 1;
            btnImport.Enabled = !_readOnlyMode;
        }
    }
}
