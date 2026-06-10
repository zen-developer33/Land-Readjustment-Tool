using Land_Pooling_Policy_Manager.UI;
using Land_Pooling_Policy_Manager.Entities.Policy;
using Land_Pooling_Policy_Manager.Services;

namespace Land_Pooling_Policy_Manager.UI.Forms
{
    public sealed partial class frmPolicyLookupTablesWindow : Form
    {
        private static readonly Color EditableCellBackColor = Color.FromArgb(255, 244, 214);
        private readonly PolicyManagerService _service;
        private readonly bool _valueOnlyEditMode;
        private bool _readOnlyMode;
        private readonly bool _cornerTypesOnly;
        private List<PolicySet> _policySummaries = [];
        private PolicySet? _currentPolicy;
        private List<PolicyLookupTable> _tables = [];
        private List<string> _roadReferenceOptions = [];
        private bool _loading;
        private bool _loadingClauses;

        public frmPolicyLookupTablesWindow(
            PolicyManagerService service,
            bool readOnlyMode,
            bool cornerTypesOnly = false,
            bool valueOnlyEditMode = false)
        {
            _service = service;
            _readOnlyMode = readOnlyMode;
            _cornerTypesOnly = cornerTypesOnly;
            _valueOnlyEditMode = valueOnlyEditMode;
            InitializeComponent();
            Text = cornerTypesOnly ? "Project Corner Type Definitions" : "Policy Lookup Tables";
            lblTable.Text = cornerTypesOnly ? "Corner Types:" : "Table:";
            btnAddRow.Enabled = !cornerTypesOnly;
            btnDeleteRow.Enabled = !cornerTypesOnly;
            btnRefresh.Text = "Refresh";
            RecordFormTheme.Apply(this);
        }

        private async void frmPolicyLookupTablesWindow_Load(object? sender, EventArgs e) => await ReloadPoliciesAsync();
        private async void cboPolicies_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_loading)
                await LoadSelectedPolicyAsync();
        }

        private void cboTables_SelectedIndexChanged(object? sender, EventArgs e) => LoadSelectedTable();
        private async void cboClauses_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_loadingClauses)
                await SaveTableClauseAsync();
        }

        private async void btnRefresh_Click(object? sender, EventArgs e) => await LoadSelectedPolicyAsync();
        private async void btnAddRow_Click(object? sender, EventArgs e) => await AddRowAsync();
        private async void btnDeleteRow_Click(object? sender, EventArgs e) => await DeleteRowAsync();
        private async void dgvLookup_CellEndEdit(object? sender, DataGridViewCellEventArgs e) => await SaveLookupCellAsync(e.RowIndex, e.ColumnIndex);

        public void SetReadOnlyMode(bool readOnlyMode)
        {
            _readOnlyMode = readOnlyMode;
            LoadSelectedTable();
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

        private async Task LoadSelectedPolicyAsync()
        {
            if (cboPolicies.SelectedIndex < 0 || cboPolicies.SelectedIndex >= _policySummaries.Count)
                return;

            UseWaitCursor = true;
            try
            {
                int policyId = _policySummaries[cboPolicies.SelectedIndex].Id;
                (PolicySet? policy, List<string> roadReferences) = await RunServiceAsync(async () =>
                {
                    await _service.EnsureCornerTypeDefinitionsAsync(policyId);

                    PolicySet? selectedPolicy = await _service.GetPolicyLookupTablesAsync(policyId);
                    List<string> roadOptions = await _service.GetProjectRoadReferenceOptionsAsync();
                    return (selectedPolicy, roadOptions);
                });
                _currentPolicy = policy;
                _roadReferenceOptions = roadReferences;
                LoadTableSelector();
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void LoadTableSelector()
        {
            cboTables.Items.Clear();
            dgvLookup.Columns.Clear();
            dgvLookup.Rows.Clear();
            if (_currentPolicy == null)
                return;

            _tables = _currentPolicy.LookupTables
                .Where(t => !_cornerTypesOnly || string.Equals(t.TableKey, "cornerTypeDefinitions", StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.DisplayOrder)
                .ToList();

            foreach (PolicyLookupTable table in _tables)
                cboTables.Items.Add(table.Title);

            if (cboTables.Items.Count > 0)
                cboTables.SelectedIndex = 0;
        }

        private void LoadSelectedTable()
        {
            dgvLookup.Columns.Clear();
            dgvLookup.Rows.Clear();
            PolicyLookupTable? table = SelectedTable();
            if (table == null)
                return;

            lblDescription.Text = table.Description ?? "";
            LoadClauseSelector(table);
            bool cornerDefinitionTable = IsCornerTypeDefinitionTable(table);
            // Add/Delete change the table inventory (structure), so they're
            // disabled in value-only mode — that mode only edits cell values.
            bool canEditStructure = IsEditable() && !cornerDefinitionTable && !_cornerTypesOnly && !_valueOnlyEditMode;
            btnAddRow.Enabled = canEditStructure;
            btnDeleteRow.Enabled = canEditStructure;
            cboClauses.Enabled = IsEditable() && !_valueOnlyEditMode;

            List<PolicyLookupColumn> columns = table.Columns.OrderBy(c => c.DisplayOrder).ToList();
            foreach (PolicyLookupColumn column in columns)
            {
                DataGridViewColumn gridColumn = CreateGridColumn(column, table);
                dgvLookup.Columns.Add(gridColumn);
            }

            foreach (PolicyLookupRow row in table.Rows.OrderBy(r => r.DisplayOrder))
            {
                int rowIndex = dgvLookup.Rows.Add();
                DataGridViewRow gridRow = dgvLookup.Rows[rowIndex];
                gridRow.Tag = row;
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    PolicyLookupCell? cell = row.Cells.FirstOrDefault(c => c.PolicyLookupColumnId == columns[columnIndex].Id);
                    gridRow.Cells[columnIndex].Value = cell?.ValueText ?? "";
                    gridRow.Cells[columnIndex].Tag = cell?.Id;
                    bool cellEditable = IsCellEditable(columns[columnIndex]);
                    gridRow.Cells[columnIndex].Style.BackColor = cellEditable ? EditableCellBackColor : dgvLookup.DefaultCellStyle.BackColor;
                    gridRow.Cells[columnIndex].ReadOnly = !cellEditable;
                }
            }

            dgvLookup.ReadOnly = !IsEditable();
        }

        private DataGridViewColumn CreateGridColumn(PolicyLookupColumn column, PolicyLookupTable table)
        {
            if (IsCornerTypeDefinitionTable(table) && IsRoadReferenceColumn(column))
            {
                DataGridViewComboBoxColumn comboColumn = new()
                {
                    HeaderText = Header(column),
                    Name = column.ColumnKey,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Width = ColumnWidth(column),
                    Tag = column,
                    FlatStyle = FlatStyle.Flat,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
                };

                foreach (string option in RoadReferenceOptionsForColumn(table, column))
                    comboColumn.Items.Add(option);

                return comboColumn;
            }

            return new DataGridViewTextBoxColumn
            {
                HeaderText = Header(column),
                Name = column.ColumnKey,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = ColumnWidth(column),
                Tag = column
            };
        }

        private List<string> RoadReferenceOptionsForColumn(PolicyLookupTable table, PolicyLookupColumn column)
        {
            List<string> options = _roadReferenceOptions.ToList();
            foreach (PolicyLookupRow row in table.Rows)
            {
                string? value = row.Cells
                    .FirstOrDefault(c => c.PolicyLookupColumnId == column.Id)
                    ?.ValueText;
                if (!string.IsNullOrWhiteSpace(value) &&
                    !options.Contains(value, StringComparer.OrdinalIgnoreCase))
                {
                    options.Add(value);
                }
            }

            return options
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(option => option, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void LoadClauseSelector(PolicyLookupTable table)
        {
            _loadingClauses = true;
            try
            {
                cboClauses.Items.Clear();
                cboClauses.Items.Add("(No clause)");
                if (_currentPolicy != null)
                {
                    foreach (PolicyClause clause in _currentPolicy.Clauses.OrderBy(c => c.DisplayOrder))
                        cboClauses.Items.Add($"{clause.ClauseCode} - {clause.Heading}");
                }

                int selectedIndex = 0;
                if (table.PolicyClauseId.HasValue && _currentPolicy != null)
                {
                    int clauseIndex = _currentPolicy.Clauses
                        .OrderBy(c => c.DisplayOrder)
                        .ToList()
                        .FindIndex(c => c.Id == table.PolicyClauseId.Value);
                    if (clauseIndex >= 0)
                        selectedIndex = clauseIndex + 1;
                }

                cboClauses.SelectedIndex = selectedIndex;
                cboClauses.Enabled = IsEditable() && !_valueOnlyEditMode;
            }
            finally
            {
                _loadingClauses = false;
            }
        }

        private async Task SaveTableClauseAsync()
        {
            PolicyLookupTable? table = SelectedTable();
            if (table == null || !IsEditable() || _cornerTypesOnly)
                return;

            int? clauseId = null;
            if (cboClauses.SelectedIndex > 0 && _currentPolicy != null)
            {
                clauseId = _currentPolicy.Clauses
                    .OrderBy(c => c.DisplayOrder)
                    .ElementAt(cboClauses.SelectedIndex - 1)
                    .Id;
            }

            await RunServiceAsync(() => _service.SaveLookupTableClauseAsync(table.Id, clauseId));
            await LoadSelectedPolicyAsync();
        }

        private async Task AddRowAsync()
        {
            PolicyLookupTable? table = SelectedTable();
            if (table == null || !IsEditable() || IsCornerTypeDefinitionTable(table))
                return;

            await RunServiceAsync(() => _service.AddLookupRowAsync(table.Id));
            await LoadSelectedPolicyAsync();
        }

        private async Task DeleteRowAsync()
        {
            if (!IsEditable() || dgvLookup.SelectedRows.Count == 0)
                return;

            if (SelectedTable() is { } table && IsCornerTypeDefinitionTable(table))
                return;

            if (dgvLookup.SelectedRows[0].Tag is not PolicyLookupRow row)
                return;

            await RunServiceAsync(() => _service.DeleteLookupRowAsync(row.Id));
            await LoadSelectedPolicyAsync();
        }

        private async Task SaveLookupCellAsync(int rowIndex, int columnIndex)
        {
            if (!IsEditable() ||
                rowIndex < 0 ||
                columnIndex < 0 ||
                rowIndex >= dgvLookup.Rows.Count ||
                columnIndex >= dgvLookup.Columns.Count)
            {
                return;
            }

            if (dgvLookup.Rows[rowIndex].Cells[columnIndex].Tag is not int cellId)
                return;

            if (dgvLookup.Columns[columnIndex].Tag is PolicyLookupColumn column && !IsCellEditable(column))
                return;

            string? value = Convert.ToString(dgvLookup.Rows[rowIndex].Cells[columnIndex].Value);
            if (SelectedTable() is { } table && IsCornerTypeDefinitionTable(table))
            {
                await RunServiceAsync(() => _service.SaveCornerRoadReferenceCellAsync(cellId, value));
                await LoadSelectedPolicyAsync();
                return;
            }

            await RunServiceAsync(() => _service.SaveLookupCellAsync(cellId, value));
        }

        private PolicyLookupTable? SelectedTable()
        {
            if (cboTables.SelectedIndex < 0 || cboTables.SelectedIndex >= _tables.Count)
                return null;

            return _tables[cboTables.SelectedIndex];
        }

        private bool IsEditable()
        {
            return !_readOnlyMode &&
                   _currentPolicy != null &&
                   PolicyValidationService.IsEditable(_currentPolicy);
        }

        private bool IsCellEditable(PolicyLookupColumn column)
        {
            if (!IsEditable())
                return false;

            PolicyLookupTable? table = SelectedTable();
            bool baseEditable = table == null
                || !IsCornerTypeDefinitionTable(table)
                || IsRoadReferenceColumn(column);
            if (!baseEditable)
                return false;

            // In value-only mode (hosted by the main app) only numeric value
            // columns are editable — Text/RoadReference identifier columns lock
            // so users can tweak the contribution percentages without renaming
            // codes or pointing rows at different roads.
            if (_valueOnlyEditMode)
                return IsNumericValueColumn(column);

            return true;
        }

        private static bool IsNumericValueColumn(PolicyLookupColumn column)
        {
            return column.ValueType switch
            {
                "Percent" => true,
                "Decimal" => true,
                "Integer" => true,
                "Number" => true,
                "Currency" => true,
                "Bool" => true,
                _ => false,
            };
        }

        private static string Header(PolicyLookupColumn column)
        {
            return string.IsNullOrWhiteSpace(column.Unit)
                ? column.HeaderText
                : $"{column.HeaderText} ({column.Unit})";
        }

        private static int ColumnWidth(PolicyLookupColumn column)
        {
            if (IsRoadReferenceColumn(column))
                return 240;
            if (column.ValueType == "Text")
                return column.HeaderText.Length > 16 ? 260 : 170;
            if (column.ValueType == "Percent")
                return 120;
            return 110;
        }

        private static bool IsRoadReferenceColumn(PolicyLookupColumn column)
        {
            return string.Equals(column.ColumnKey, "primaryFrontageRoad", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.ColumnKey, "secondaryFrontageRoad", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.ValueType, "RoadReference", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCornerTypeDefinitionTable(PolicyLookupTable table)
        {
            return string.Equals(table.TableKey, "cornerTypeDefinitions", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<T> RunServiceAsync<T>(Func<Task<T>> operation)
        {
            return await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));
        }

        private async Task RunServiceAsync(Func<Task> operation)
        {
            await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));
        }
    }
}
