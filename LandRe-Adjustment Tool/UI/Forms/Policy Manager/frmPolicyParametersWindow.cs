using Land_Readjustment_Tool.Core.Entities.Policy;
using Land_Readjustment_Tool.Services.Policy;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmPolicyParametersWindow : Form
    {
        private static readonly Color EditableCellBackColor = Color.FromArgb(255, 244, 214);
        private readonly PolicyManagerService _service;
        private readonly bool _readOnlyMode;
        private List<PolicySet> _policySummaries = [];
        private PolicySet? _currentPolicy;
        private bool _loading;

        public frmPolicyParametersWindow(PolicyManagerService service, bool readOnlyMode)
        {
            _service = service;
            _readOnlyMode = readOnlyMode;
            InitializeComponent();
            RecordFormTheme.Apply(this);
        }

        private async void frmPolicyParametersWindow_Load(object? sender, EventArgs e) => await ReloadPoliciesAsync();
        private async void cboPolicies_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_loading)
                await LoadSelectedPolicyAsync();
        }

        private async void btnRefresh_Click(object? sender, EventArgs e) => await LoadSelectedPolicyAsync();
        private async void btnAddParameter_Click(object? sender, EventArgs e) => await AddParameterAsync();
        private async void btnDeleteParameter_Click(object? sender, EventArgs e) => await DeleteParameterAsync();
        private async void dgvParameters_CellEndEdit(object? sender, DataGridViewCellEventArgs e) => await SaveParameterRowAsync(e.RowIndex);

        private async Task ReloadPoliciesAsync()
        {
            _loading = true;
            try
            {
                _policySummaries = await RunServiceAsync(() => _service.GetPolicySummariesAsync());
                cboPolicies.Items.Clear();
                foreach (PolicySet policy in _policySummaries)
                    cboPolicies.Items.Add($"{policy.PolicyName} | v{policy.VersionNo} | {policy.Status}");

                if (cboPolicies.Items.Count > 0)
                    cboPolicies.SelectedIndex = 0;
            }
            finally
            {
                _loading = false;
            }

            await LoadSelectedPolicyAsync();
        }

        private async Task LoadSelectedPolicyAsync()
        {
            if (cboPolicies.SelectedIndex < 0 || cboPolicies.SelectedIndex >= _policySummaries.Count)
                return;

            UseWaitCursor = true;
            try
            {
                int policyId = _policySummaries[cboPolicies.SelectedIndex].Id;
                _currentPolicy = await RunServiceAsync(() => _service.GetPolicyParametersAsync(policyId));
                LoadGrid();
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void LoadGrid()
        {
            dgvParameters.Rows.Clear();
            if (_currentPolicy == null)
                return;

            Dictionary<int, string> clauseCodes = _currentPolicy.Clauses
                .ToDictionary(c => c.Id, c => c.ClauseCode ?? "");

            foreach (PolicyParameter parameter in _currentPolicy.Parameters.OrderBy(p => p.DisplayOrder))
            {
                int rowIndex = dgvParameters.Rows.Add(
                    parameter.PolicyClauseId.HasValue && clauseCodes.TryGetValue(parameter.PolicyClauseId.Value, out string? clauseCode) ? clauseCode ?? "" : "",
                    parameter.ParameterKey ?? "",
                    parameter.Label,
                    parameter.ValueText ?? "",
                    parameter.Unit ?? "",
                    parameter.ValueType,
                    parameter.Description ?? "");
                DataGridViewRow row = dgvParameters.Rows[rowIndex];
                row.Tag = parameter;
                ApplyEditableStyle(row);
            }

            dgvParameters.ReadOnly = !IsEditable();
        }

        private void ApplyEditableStyle(DataGridViewRow row)
        {
            bool editable = IsEditable();
            for (int i = 0; i < row.Cells.Count; i++)
                row.Cells[i].ReadOnly = !editable || i == 0;

            for (int i = 1; i < row.Cells.Count; i++)
                row.Cells[i].Style.BackColor = editable ? EditableCellBackColor : dgvParameters.DefaultCellStyle.BackColor;
        }

        private async Task AddParameterAsync()
        {
            if (_currentPolicy == null || !IsEditable())
                return;

            PolicyClause? selectedClause = _currentPolicy.Clauses.OrderBy(c => c.DisplayOrder).FirstOrDefault();
            PolicyParameter parameter = new()
            {
                PolicySetId = _currentPolicy.Id,
                PolicyClauseId = selectedClause?.Id,
                ParameterKey = "newParameter",
                Label = "New Parameter",
                ValueType = "Text",
                ValueText = "",
                Unit = "",
                Description = "",
                DisplayOrder = _currentPolicy.Parameters.Count + 1
            };

            await RunServiceAsync(() => _service.SaveParameterAsync(parameter));
            await LoadSelectedPolicyAsync();
        }

        private async Task DeleteParameterAsync()
        {
            if (!IsEditable() || dgvParameters.SelectedRows.Count == 0)
                return;

            if (dgvParameters.SelectedRows[0].Tag is not PolicyParameter parameter)
                return;

            await RunServiceAsync(() => _service.DeleteParameterAsync(parameter.Id));
            await LoadSelectedPolicyAsync();
        }

        private async Task SaveParameterRowAsync(int rowIndex)
        {
            if (!IsEditable() || rowIndex < 0 || rowIndex >= dgvParameters.Rows.Count)
                return;

            if (dgvParameters.Rows[rowIndex].Tag is not PolicyParameter parameter)
                return;

            parameter.ParameterKey = NullIfBlank(Value(rowIndex, 1));
            parameter.Label = string.IsNullOrWhiteSpace(Value(rowIndex, 2)) ? "Parameter" : Value(rowIndex, 2);
            parameter.ValueText = Value(rowIndex, 3);
            parameter.Unit = NullIfBlank(Value(rowIndex, 4));
            parameter.ValueType = string.IsNullOrWhiteSpace(Value(rowIndex, 5)) ? "Text" : Value(rowIndex, 5);
            parameter.Description = Value(rowIndex, 6);
            await RunServiceAsync(() => _service.SaveParameterAsync(parameter));
        }

        private bool IsEditable()
        {
            return !_readOnlyMode &&
                   _currentPolicy != null &&
                   PolicyValidationService.IsEditable(_currentPolicy);
        }

        private async Task<T> RunServiceAsync<T>(Func<Task<T>> operation)
        {
            return await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));
        }

        private async Task RunServiceAsync(Func<Task> operation)
        {
            await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));
        }

        private string Value(int rowIndex, int columnIndex)
        {
            return Convert.ToString(dgvParameters.Rows[rowIndex].Cells[columnIndex].Value)?.Trim() ?? "";
        }

        private static string? NullIfBlank(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
