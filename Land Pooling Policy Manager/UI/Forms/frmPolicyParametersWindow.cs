using Land_Pooling_Policy_Manager.UI;
using Land_Pooling_Policy_Manager.Entities.Policy;
using Land_Pooling_Policy_Manager.Services;

namespace Land_Pooling_Policy_Manager.UI.Forms
{
    public sealed partial class frmPolicyParametersWindow : Form
    {
        private static readonly Color EditableCellBackColor = Color.FromArgb(255, 244, 214);

        // Column layout for this grid: 0 Ref. Clause | 1 Parameter | 2 Value | 3 Unit | 4 Description
        private const int ValueColumnIndex = 2;

        private readonly PolicyManagerService _service;
        private readonly bool _valueOnlyEditMode;
        private bool _readOnlyMode;
        private List<PolicySet> _policySummaries = [];
        private PolicySet? _currentPolicy;
        private bool _loading;

        public frmPolicyParametersWindow(
            PolicyManagerService service,
            bool readOnlyMode,
            bool valueOnlyEditMode = false)
        {
            _service = service;
            _readOnlyMode = readOnlyMode;
            _valueOnlyEditMode = valueOnlyEditMode;
            InitializeComponent();
            RecordFormTheme.Apply(this);
        }

        private async void frmPolicyParametersWindow_Load(object? sender, EventArgs e) => await ReloadPoliciesAsync();
        private async void cboPolicies_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_loading)
                await LoadSelectedPolicyAsync();
        }

        private async void btnRefresh_Click(object? sender, EventArgs e) => await LoadSelectedPolicyAsync(SelectedParameterId());
        private async void btnAddParameter_Click(object? sender, EventArgs e) => await AddParameterAsync();
        private async void btnDeleteParameter_Click(object? sender, EventArgs e) => await DeleteParameterAsync();
        private async void dgvParameters_CellEndEdit(object? sender, DataGridViewCellEventArgs e) => await SaveParameterRowAsync(e.RowIndex);
        private void dgvParameters_SelectionChanged(object? sender, EventArgs e) => UpdateEditState();

        public void SetReadOnlyMode(bool readOnlyMode)
        {
            _readOnlyMode = readOnlyMode;
            UpdateEditState();
            foreach (DataGridViewRow row in dgvParameters.Rows)
                ApplyEditableStyle(row);
        }

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

        private async Task LoadSelectedPolicyAsync(int? selectParameterId = null)
        {
            if (cboPolicies.SelectedIndex < 0 || cboPolicies.SelectedIndex >= _policySummaries.Count)
                return;

            UseWaitCursor = true;
            try
            {
                int policyId = _policySummaries[cboPolicies.SelectedIndex].Id;
                _currentPolicy = await RunServiceAsync(() => _service.GetPolicyParametersAsync(policyId));
                LoadGrid(selectParameterId);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void LoadGrid(int? selectParameterId = null)
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
                    parameter.Label,
                    parameter.ValueText ?? "",
                    parameter.Unit ?? "",
                    parameter.Description ?? "");
                DataGridViewRow row = dgvParameters.Rows[rowIndex];
                row.Tag = parameter;
                ApplyEditableStyle(row);
            }

            SelectParameterByIdOrFirst(selectParameterId);
            dgvParameters.ReadOnly = !IsEditable();
            UpdateEditState();
        }

        private void SelectParameterByIdOrFirst(int? parameterId)
        {
            if (dgvParameters.Rows.Count == 0)
                return;

            DataGridViewRow? rowToSelect = null;
            if (parameterId.HasValue)
            {
                rowToSelect = dgvParameters.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(row => row.Tag is PolicyParameter parameter && parameter.Id == parameterId.Value);
            }

            rowToSelect ??= dgvParameters.Rows[0];
            dgvParameters.ClearSelection();
            rowToSelect.Selected = true;
            dgvParameters.CurrentCell = rowToSelect.Cells[0];
        }

        private void ApplyEditableStyle(DataGridViewRow row)
        {
            // Three modes:
            //   • Not editable at all (read-only mode / approved policy) → every cell locked, no highlight.
            //   • Value-only mode (hosted by the main app)             → only the Value column editable + highlighted.
            //   • Full edit mode (standalone)                          → every column except Ref. Clause editable + highlighted.
            bool editable = IsEditable();
            for (int i = 0; i < row.Cells.Count; i++)
            {
                bool cellEditable = editable
                    && (_valueOnlyEditMode ? i == ValueColumnIndex : i != 0);
                row.Cells[i].ReadOnly = !cellEditable;
                row.Cells[i].Style.BackColor = cellEditable
                    ? EditableCellBackColor
                    : dgvParameters.DefaultCellStyle.BackColor;
            }
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
                ParameterKey = $"newParameter{DateTime.Now:HHmmss}",
                Label = "New Parameter",
                ValueType = "Text",
                ValueText = "",
                Unit = "",
                Description = "",
                DisplayOrder = _currentPolicy.Parameters.Count + 1
            };

            PolicyParameter created = await RunServiceAsync(() => _service.SaveParameterAsync(parameter));
            await LoadSelectedPolicyAsync(created.Id);
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

            parameter.Label = string.IsNullOrWhiteSpace(Value(rowIndex, 1)) ? "Parameter" : Value(rowIndex, 1);
            parameter.ValueText = Value(rowIndex, 2);
            parameter.Unit = NullIfBlank(Value(rowIndex, 3));
            parameter.Description = Value(rowIndex, 4);
            await RunServiceAsync(() => _service.SaveParameterAsync(parameter));
        }

        private bool IsEditable()
        {
            return !_readOnlyMode &&
                   _currentPolicy != null &&
                   PolicyValidationService.IsEditable(_currentPolicy);
        }

        private void UpdateEditState()
        {
            // Add/Delete change the parameter inventory (structure), so they're
            // disabled in value-only mode — that mode is for tuning numbers, not
            // for adding/removing rows.
            bool editable = IsEditable();
            bool canEditStructure = editable && !_valueOnlyEditMode;
            btnAddParameter.Enabled = canEditStructure;
            btnDeleteParameter.Enabled = canEditStructure && dgvParameters.SelectedRows.Count > 0;
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

        private int? SelectedParameterId()
        {
            if (dgvParameters.SelectedRows.Count == 0 ||
                dgvParameters.SelectedRows[0].Tag is not PolicyParameter parameter)
            {
                return null;
            }

            return parameter.Id;
        }

        private static string? NullIfBlank(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
