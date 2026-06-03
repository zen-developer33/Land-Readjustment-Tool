using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services.Assignment;
using System.Drawing;
using System.Text.Json;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmCadastralAutoAssignment : Form
    {
        private readonly ProjectSession _session;
        private readonly ICadastralRecordAssignmentService _assignmentService;
        private List<CadastralAssignmentCandidate> _allCandidates = [];
        private IReadOnlyList<string>? _mapSheets;
        private bool _suppressAttributeMappingEvents;

        public bool AssignmentChanged { get; private set; }

        public frmCadastralAutoAssignment(
            ProjectSession session,
            ICadastralRecordAssignmentService assignmentService)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));

            InitializeComponent();
            Load += frmCadastralAutoAssignment_Load;
            _cboSourceMapSheetField.SelectedIndexChanged += async (_, _) =>
            {
                if (!_suppressAttributeMappingEvents)
                    await PopulateAttributeMapSheetMappingGridAsync();
            };
            _dgvLayerMapSheets.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_dgvLayerMapSheets.IsCurrentCellDirty)
                    _dgvLayerMapSheets.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _dgvAttributeMapSheetMappings.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_dgvAttributeMapSheetMappings.IsCurrentCellDirty)
                    _dgvAttributeMapSheetMappings.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _btnRun.Click += btnRun_Click;
            _btnClose.Click += (_, _) => Close();
        }

        private async void frmCadastralAutoAssignment_Load(object? sender, EventArgs e)
        {
            SetBusy(true, "Loading imported cadastral objects...");
            await Task.Yield();
            try
            {
                _allCandidates = (await _assignmentService.GetCandidatesAsync(_session)).ToList();
                _mapSheets = await _assignmentService.GetMapSheetNumbersAsync(_session);
                await PopulateMappingControlsAsync();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnRun_Click(object? sender, EventArgs e)
        {
            bool useAttributeMapping = UsesAttributeBasedMapping();
            IReadOnlyList<CadastralLayerMapSheetMapping> layerMappings = useAttributeMapping ? [] : GetLayerMappings();
            if (!useAttributeMapping && layerMappings.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "Map at least one imported parcel layer to an Original Parcel Record MapSheetNo before auto assignment.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (useAttributeMapping &&
                (string.IsNullOrWhiteSpace(GetSelectedComboText(_cboSourceMapSheetField)) ||
                 string.IsNullOrWhiteSpace(GetSelectedComboText(_cboSourceParcelField)) ||
                 GetAttributeMapSheetValueMappings().Count == 0))
            {
                MessageBox.Show(
                    this,
                    "Select the source map-sheet field, map at least one source value to a target MapSheetNo, and select the parcel field.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                SetBusy(true, useAttributeMapping
                    ? "Assigning from saved source attributes..."
                    : "Assigning from layer map and parcel text locations...");

                CadastralAssignmentResult result = useAttributeMapping
                    ? await _assignmentService.AutoAssignFromAttributesAsync(
                        _session,
                        _chkReplaceExisting.Checked,
                        GetAttributeFieldMapping())
                    : await _assignmentService.AutoAssignAsync(
                        _session,
                        _chkReplaceExisting.Checked,
                        layerMappings);

                if (!result.Success)
                {
                    MessageBox.Show(
                        this,
                        result.ErrorMessage ?? "Cadastral assignment failed.",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    _lblStatus.Text = result.ErrorMessage ?? "Assignment failed.";
                    return;
                }

                AssignmentChanged |= result.AssignedCount > 0;
                _lblStatus.Text =
                    $"Assigned {result.AssignedCount:N0}. Missing key: {result.MissingKeyCount:N0}. No record/conflict: {result.NoRecordMatchCount:N0}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Cadastral assignment failed: {ex.Message}",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _lblStatus.Text = "Assignment failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task PopulateMappingControlsAsync()
        {
            IReadOnlyList<string> mapSheets = await GetMapSheetsAsync();
            bool useAttributeMapping = UsesAttributeBasedMapping();
            _lblMappingCaption.Text = useAttributeMapping ? "Attribute mapping" : "Layer mapping";
            _dgvLayerMapSheets.Visible = !useAttributeMapping;
            _attributeLayout.Visible = useAttributeMapping;

            if (useAttributeMapping)
            {
                PopulateAttributeFieldSelectors();
                await PopulateAttributeMapSheetMappingGridAsync();
                _lblStatus.Text = GetAttributeFieldNames().Count == 0
                    ? "No saved attribute table was found for these imported objects."
                    : $"{_allCandidates.Count:N0} imported cadastral object(s) ready for attribute assignment.";
                _btnRun.Enabled = _allCandidates.Count > 0 && GetAttributeFieldNames().Count > 0;
                return;
            }

            ConfigureLayerMappingGrid();
            DataGridViewComboBoxColumn? mapSheetColumn =
                _dgvLayerMapSheets.Columns["MapSheet"] as DataGridViewComboBoxColumn;
            if (mapSheetColumn != null)
            {
                mapSheetColumn.Items.Clear();
                mapSheetColumn.Items.Add(string.Empty);
                foreach (string mapSheet in mapSheets)
                    mapSheetColumn.Items.Add(mapSheet);
            }

            List<DataGridViewRow> rows = [];
            foreach (LayerMappingRow layer in _allCandidates
                .Where(candidate => !IsAttributeMappedSource(candidate))
                .GroupBy(candidate => BuildLayerMappingKey(candidate.LayerName, candidate.SourceLayer), StringComparer.OrdinalIgnoreCase)
                .Select(group => new LayerMappingRow(
                    group.First().LayerName,
                    group.First().SourceLayer,
                    group.Select(item => item.MapSheetNo).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))))
                .OrderBy(item => item.LayerName)
                .ThenBy(item => item.SourceLayer))
            {
                DataGridViewRow row = new();
                row.CreateCells(
                    _dgvLayerMapSheets,
                    layer.LayerName,
                    layer.SourceLayer ?? "-",
                    ResolveDefaultMapSheet(layer, mapSheets));
                row.Tag = layer;
                rows.Add(row);
            }

            ReplaceGridRows(_dgvLayerMapSheets, rows);

            _lblStatus.Text = $"{_allCandidates.Count:N0} imported cadastral object(s) ready for auto assignment.";
            _btnRun.Enabled = _allCandidates.Count > 0 && mapSheets.Count > 0;
        }

        private IReadOnlyList<CadastralLayerMapSheetMapping> GetLayerMappings()
        {
            List<CadastralLayerMapSheetMapping> mappings = [];
            foreach (DataGridViewRow row in _dgvLayerMapSheets.Rows)
            {
                if (row.Tag is not LayerMappingRow layer)
                    continue;

                string? mapSheet = Convert.ToString(row.Cells["MapSheet"].Value)?.Trim();
                if (string.IsNullOrWhiteSpace(mapSheet))
                    continue;

                mappings.Add(new CadastralLayerMapSheetMapping(layer.LayerName, layer.SourceLayer, mapSheet));
            }

            return mappings;
        }

        private string ResolveDefaultMapSheet(LayerMappingRow layer, IReadOnlyList<string> mapSheets)
        {
            if (!string.IsNullOrWhiteSpace(layer.MapSheetNo) &&
                mapSheets.Contains(layer.MapSheetNo, StringComparer.OrdinalIgnoreCase))
            {
                return layer.MapSheetNo;
            }

            foreach (string mapSheet in mapSheets)
            {
                if (string.Equals(mapSheet, layer.SourceLayer, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(mapSheet, layer.LayerName, StringComparison.OrdinalIgnoreCase))
                {
                    return mapSheet;
                }
            }

            return string.Empty;
        }

        private void PopulateAttributeFieldSelectors()
        {
            List<string> fields = GetAttributeFieldNames();
            _suppressAttributeMappingEvents = true;
            try
            {
                _cboSourceMapSheetField.Items.Clear();
                _cboSourceParcelField.Items.Clear();
                _cboSourceMapSheetField.Items.Add(string.Empty);
                _cboSourceParcelField.Items.Add(string.Empty);
                foreach (string field in fields)
                {
                    _cboSourceMapSheetField.Items.Add(field);
                    _cboSourceParcelField.Items.Add(field);
                }

                SelectComboValue(
                    _cboSourceMapSheetField,
                    FindBestAttributeField(
                        fields,
                        ["mapsheet", "map_sheet", "map sheet", "mapsheetno", "map_sheet_no", "sheet", "sheetno", "sheet_no"]));
                SelectComboValue(
                    _cboSourceParcelField,
                    FindBestAttributeField(
                        fields,
                        ["parcel", "parcelno", "parcel_no", "parcel number", "parcelnumber", "kitta", "kitta_no", "kittano", "plot", "plotno", "plot_no"]));
            }
            finally
            {
                _suppressAttributeMappingEvents = false;
            }
        }

        private async Task PopulateAttributeMapSheetMappingGridAsync()
        {
            IReadOnlyList<string> mapSheets = await GetMapSheetsAsync();
            if (_dgvAttributeMapSheetMappings.Columns["TargetMapSheet"] is DataGridViewComboBoxColumn targetColumn)
            {
                targetColumn.Items.Clear();
                targetColumn.Items.Add(string.Empty);
                foreach (string mapSheet in mapSheets)
                    targetColumn.Items.Add(mapSheet);
            }

            string? mapSheetField = GetSelectedComboText(_cboSourceMapSheetField);
            List<DataGridViewRow> rows = [];
            foreach (string sourceValue in GetAttributeUniqueValues(mapSheetField))
            {
                DataGridViewRow row = new();
                row.CreateCells(
                    _dgvAttributeMapSheetMappings,
                    sourceValue,
                    ResolveLikelyMapSheet(sourceValue, mapSheets));
                row.Tag = sourceValue;
                rows.Add(row);
            }

            ReplaceGridRows(_dgvAttributeMapSheetMappings, rows);
        }

        private CadastralAttributeFieldMapping GetAttributeFieldMapping()
        {
            return new CadastralAttributeFieldMapping(
                GetSelectedComboText(_cboSourceMapSheetField),
                GetSelectedComboText(_cboSourceParcelField),
                GetAttributeMapSheetValueMappings());
        }

        private Dictionary<string, string> GetAttributeMapSheetValueMappings()
        {
            Dictionary<string, string> mappings = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in _dgvAttributeMapSheetMappings.Rows)
            {
                string? sourceValue = row.Tag as string;
                string? targetMapSheet = Convert.ToString(row.Cells["TargetMapSheet"].Value)?.Trim();
                if (string.IsNullOrWhiteSpace(sourceValue) || string.IsNullOrWhiteSpace(targetMapSheet))
                    continue;

                mappings[sourceValue] = targetMapSheet;
                string normalized = NormalizeMapSheetValue(sourceValue);
                if (!mappings.ContainsKey(normalized))
                    mappings[normalized] = targetMapSheet;
            }

            return mappings;
        }

        private List<string> GetAttributeFieldNames()
        {
            return _allCandidates
                .Where(IsAttributeMappedSource)
                .SelectMany(candidate => ReadAttributeDictionary(candidate.AttributesJson).Keys)
                .Where(field => !string.IsNullOrWhiteSpace(field))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(field => field, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private List<string> GetAttributeUniqueValues(string? fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return [];

            return _allCandidates
                .Where(IsAttributeMappedSource)
                .Select(candidate => ReadAttributeDictionary(candidate.AttributesJson))
                .Select(attributes => attributes.TryGetValue(fieldName, out string? value) ? value?.Trim() : null)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string ResolveLikelyMapSheet(string sourceValue, IReadOnlyList<string> mapSheets)
        {
            foreach (string mapSheet in mapSheets)
            {
                if (string.Equals(
                        NormalizeMapSheetValue(sourceValue),
                        NormalizeMapSheetValue(mapSheet),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return mapSheet;
                }
            }

            return string.Empty;
        }

        private void SetBusy(bool busy, string? status = null)
        {
            _btnRun.Enabled = !busy;
            _btnClose.Enabled = !busy;
            _dgvLayerMapSheets.Enabled = !busy;
            _attributeLayout.Enabled = !busy;
            _chkReplaceExisting.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                _lblStatus.Text = status;
        }

        private async Task<IReadOnlyList<string>> GetMapSheetsAsync()
        {
            _mapSheets ??= await _assignmentService.GetMapSheetNumbersAsync(_session);
            return _mapSheets;
        }

        private bool UsesAttributeBasedMapping()
        {
            return _allCandidates.Count > 0 &&
                   _allCandidates.All(IsAttributeMappedSource);
        }

        private static bool IsAttributeMappedSource(CadastralAssignmentCandidate candidate)
        {
            return string.Equals(candidate.SourceFormat, "SHP", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(candidate.SourceFormat, "KML", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(candidate.SourceFormat, "KMZ", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string?> ReadAttributeDictionary(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            try
            {
                Dictionary<string, string?>? attributes =
                    JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
                return attributes == null
                    ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string?>(attributes, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static string BuildLayerMappingKey(string layerName, string? sourceLayer)
        {
            return $"{layerName.Trim()}::{(sourceLayer ?? string.Empty).Trim()}";
        }

        private static string? GetSelectedComboText(ComboBox comboBox)
        {
            string? value = Convert.ToString(comboBox.SelectedItem)?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static string? FindBestAttributeField(IEnumerable<string> fields, IReadOnlyList<string> names)
        {
            foreach (string name in names)
            {
                string? exact = fields.FirstOrDefault(field =>
                    string.Equals(NormalizeMapSheetValue(field), NormalizeMapSheetValue(name), StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(exact))
                    return exact;
            }

            foreach (string name in names)
            {
                string normalizedName = NormalizeMapSheetValue(name);
                string? contains = fields.FirstOrDefault(field =>
                    NormalizeMapSheetValue(field).Contains(normalizedName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(contains))
                    return contains;
            }

            return null;
        }

        private static void SelectComboValue(ComboBox comboBox, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            for (int index = 0; index < comboBox.Items.Count; index++)
            {
                if (string.Equals(comboBox.Items[index]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedIndex = index;
                    return;
                }
            }
        }

        private static string NormalizeMapSheetValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value
                    .Where(ch => !char.IsWhiteSpace(ch))
                    .Select(char.ToUpperInvariant)
                    .ToArray());
        }

        private static void ApplyQuietGridStyle(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(214, 219, 226);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 239, 255);
            grid.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 253, 255);
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 239, 255);
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
        }

        private static void ReplaceGridRows(DataGridView grid, IReadOnlyList<DataGridViewRow> rows)
        {
            grid.SuspendLayout();
            try
            {
                grid.Rows.Clear();
                if (rows.Count > 0)
                    grid.Rows.AddRange(rows.ToArray());
            }
            finally
            {
                grid.ResumeLayout();
            }
        }

        private sealed record LayerMappingRow(
            string LayerName,
            string? SourceLayer,
            string? MapSheetNo);
    }
}
