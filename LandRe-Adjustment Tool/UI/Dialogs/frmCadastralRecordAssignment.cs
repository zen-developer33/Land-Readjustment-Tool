using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services.Assignment;
using System.Drawing;
using System.Text.Json;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmCadastralRecordAssignment : Form
    {
        private const string FilterAll = "All objects";
        private const string FilterNotAssigned = "Not assigned";
        private const string FilterAssigned = "Assigned";
        private const string FilterAutoAssigned = "Auto assigned";
        private const string FilterManualAssigned = "Manual assigned";

        private readonly ProjectSession _session;
        private readonly ICadastralRecordAssignmentService _assignmentService;
        private List<CadastralAssignmentCandidate> _allCandidates = [];
        private List<CadastralAssignmentCandidate> _candidates = [];
        private List<CadastralParcelRecordChoice> _parcelChoices = [];
        private bool _suppressCanvasPreviewEvent;
        private bool _suppressAttributeMappingEvents;

        public event Action<Guid?, bool>? SelectedCanvasObjectChanged;
        public event Action? AssignmentCommitted;

        public bool AssignmentChanged { get; private set; }

        public frmCadastralRecordAssignment(
            ProjectSession session,
            ICadastralRecordAssignmentService assignmentService)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));

            InitializeComponent();
            Load += frmCadastralRecordAssignment_Load;
            lstObjects.SelectedIndexChanged += (_, _) => ShowSelectedCandidate();
            cboObjectFilter.SelectedIndexChanged += (_, _) => ApplyCandidateFilter();
            cboMapSheet.SelectedIndexChanged += async (_, _) => await LoadParcelsForSelectedMapSheetAsync();
            dgvLayerMapSheets.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (dgvLayerMapSheets.IsCurrentCellDirty)
                    dgvLayerMapSheets.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvAttributeMapSheetMappings.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (dgvAttributeMapSheetMappings.IsCurrentCellDirty)
                    dgvAttributeMapSheetMappings.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            cboSourceMapSheetField.SelectedIndexChanged += (_, _) =>
            {
                if (!_suppressAttributeMappingEvents)
                    PopulateAttributeMapSheetMappingGrid();
            };
            btnPrevious.Click += (_, _) => MoveSelection(-1);
            btnNext.Click += (_, _) => MoveSelection(1);
            btnAssign.Click += btnAssign_Click;
            btnAutoAssign.Click += btnAutoAssign_Click;
            btnClearAssignments.Click += btnClearAssignments_Click;
            FormClosed += (_, _) => SelectedCanvasObjectChanged?.Invoke(null, false);
        }

        private async void frmCadastralRecordAssignment_Load(object? sender, EventArgs e)
        {
            ConfigureObjectFilter();
            await LoadReferenceDataAsync();
            ConfigureLayerMappingGrid();
            ConfigureAttributeMappingGrid();
            await LoadCandidatesAsync();
        }

        private void ConfigureObjectFilter()
        {
            cboObjectFilter.Items.Clear();
            cboObjectFilter.Items.Add(FilterAll);
            cboObjectFilter.Items.Add(FilterNotAssigned);
            cboObjectFilter.Items.Add(FilterAssigned);
            cboObjectFilter.Items.Add(FilterAutoAssigned);
            cboObjectFilter.Items.Add(FilterManualAssigned);
            cboObjectFilter.SelectedIndex = 0;
        }

        private async Task LoadReferenceDataAsync()
        {
            cboMapSheet.Items.Clear();
            foreach (string mapSheet in await _assignmentService.GetMapSheetNumbersAsync(_session))
                cboMapSheet.Items.Add(mapSheet);

            if (cboMapSheet.Items.Count > 0)
                cboMapSheet.SelectedIndex = 0;
        }

        private void ConfigureLayerMappingGrid()
        {
            dgvLayerMapSheets.Columns.Clear();
            dgvLayerMapSheets.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Layer",
                HeaderText = "Imported parcel layer",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dgvLayerMapSheets.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SourceLayer",
                HeaderText = "Drawing source layer",
                ReadOnly = true,
                Width = 180
            });

            DataGridViewComboBoxColumn mapSheetColumn = new()
            {
                Name = "MapSheet",
                HeaderText = "Target MapSheetNo",
                Width = 170,
                FlatStyle = FlatStyle.Flat
            };
            mapSheetColumn.Items.Add(string.Empty);
            foreach (object item in cboMapSheet.Items)
                mapSheetColumn.Items.Add(item.ToString() ?? string.Empty);

            dgvLayerMapSheets.Columns.Add(mapSheetColumn);
            ApplyQuietGridStyle(dgvLayerMapSheets, enabled: true);
        }

        private async Task LoadCandidatesAsync(Guid? preferredSelection = null)
        {
            Guid? selectedBeforeReload = preferredSelection ?? GetSelectedCandidate()?.CanvasObjectId;
            _allCandidates = (await _assignmentService.GetCandidatesAsync(_session)).ToList();
            PopulateLayerMappingGrid();
            ApplyCandidateFilter(selectedBeforeReload);
        }

        private void PopulateLayerMappingGrid()
        {
            bool useAttributeMapping = UsesAttributeBasedMapping();
            lblLayerMappingCaption.Text = useAttributeMapping
                ? "Attribute mapping"
                : "Source to target MapSheet";

            dgvLayerMapSheets.Visible = !useAttributeMapping;
            attributeMappingLayout.Visible = useAttributeMapping;

            Dictionary<string, string?> existingSelections = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in dgvLayerMapSheets.Rows)
            {
                if (row.Tag is LayerMappingRow layer &&
                    row.Cells["MapSheet"].Value is string mapSheet)
                {
                    existingSelections[BuildLayerMappingKey(layer.LayerName, layer.SourceLayer)] = mapSheet;
                }
            }

            dgvLayerMapSheets.Rows.Clear();
            if (useAttributeMapping)
            {
                PopulateAttributeFieldSelectors();
                PopulateAttributeMapSheetMappingGrid();
                ApplyAttributeMappingGridState(GetAttributeFieldNames().Count > 0);
                return;
            }

            ConfigureLayerMappingGrid();
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
                string key = BuildLayerMappingKey(layer.LayerName, layer.SourceLayer);
                string defaultMapSheet = existingSelections.TryGetValue(key, out string? selected)
                    ? selected ?? string.Empty
                    : ResolveDefaultMapSheet(layer);
                int rowIndex = dgvLayerMapSheets.Rows.Add(layer.LayerName, layer.SourceLayer ?? "-", defaultMapSheet);
                dgvLayerMapSheets.Rows[rowIndex].Tag = layer;
            }

            dgvLayerMapSheets.Enabled = cboMapSheet.Items.Count > 0 && dgvLayerMapSheets.Rows.Count > 0;
            dgvLayerMapSheets.ClearSelection();
            dgvLayerMapSheets.CurrentCell = null;
        }

        private string ResolveDefaultMapSheet(LayerMappingRow layer)
        {
            if (!string.IsNullOrWhiteSpace(layer.MapSheetNo) &&
                ComboContains(cboMapSheet, layer.MapSheetNo))
            {
                return layer.MapSheetNo;
            }

            foreach (object item in cboMapSheet.Items)
            {
                string mapSheet = item.ToString() ?? string.Empty;
                if (string.Equals(mapSheet, layer.SourceLayer, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(mapSheet, layer.LayerName, StringComparison.OrdinalIgnoreCase))
                {
                    return mapSheet;
                }
            }

            return string.Empty;
        }

        private void ApplyCandidateFilter(Guid? preferredSelection = null)
        {
            string filter = cboObjectFilter.SelectedItem?.ToString() ?? FilterAll;
            _candidates = _allCandidates
                .Where(candidate => MatchesFilter(candidate, filter))
                .ToList();

            lstObjects.Items.Clear();
            foreach (CadastralAssignmentCandidate candidate in _candidates)
                lstObjects.Items.Add(new CandidateItem(candidate));

            lblStatus.Text = _allCandidates.Count == 0
                ? "No imported cadastral parcel shapes found."
                : $"{_candidates.Count} of {_allCandidates.Count} imported parcel shapes shown.";
            btnAssign.Enabled = _candidates.Count > 0 && cboMapSheet.Items.Count > 0;
            btnAutoAssign.Enabled = _allCandidates.Count > 0 && cboMapSheet.Items.Count > 0;
            if (UsesAttributeBasedMapping())
            {
                btnAutoAssign.Text = "Remap / Auto Assign";
                lblStatus.Text += GetAttributeFieldNames().Count == 0
                    ? " No saved attribute table was found for these imported objects."
                    : " Choose saved source fields below to remap from the imported attribute table.";
            }
            else
            {
                btnAutoAssign.Text = "Auto Assign";
            }

            if (preferredSelection.HasValue && SelectCandidate(preferredSelection.Value))
                return;

            if (lstObjects.Items.Count > 0)
                lstObjects.SelectedIndex = 0;
            else
                ShowSelectedCandidate();
        }

        private static bool MatchesFilter(CadastralAssignmentCandidate candidate, string filter)
        {
            bool isAssigned = candidate.BaselineParcelId.HasValue ||
                              string.Equals(candidate.AssignmentStatus, "AutoAssigned", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(candidate.AssignmentStatus, "ManualAssigned", StringComparison.OrdinalIgnoreCase);

            return filter switch
            {
                FilterNotAssigned => !isAssigned,
                FilterAssigned => isAssigned,
                FilterAutoAssigned => string.Equals(candidate.AssignmentStatus, "AutoAssigned", StringComparison.OrdinalIgnoreCase),
                FilterManualAssigned => string.Equals(candidate.AssignmentStatus, "ManualAssigned", StringComparison.OrdinalIgnoreCase),
                _ => true
            };
        }

        private async Task LoadParcelsForSelectedMapSheetAsync()
        {
            cboParcel.Items.Clear();
            string? mapSheet = cboMapSheet.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(mapSheet))
                return;

            _parcelChoices = (await _assignmentService.GetParcelsByMapSheetAsync(_session, mapSheet)).ToList();
            foreach (CadastralParcelRecordChoice parcel in _parcelChoices)
                cboParcel.Items.Add(new ParcelItem(parcel));

            if (cboParcel.Items.Count > 0)
                cboParcel.SelectedIndex = 0;
        }

        private void ShowSelectedCandidate()
        {
            CadastralAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null)
            {
                lblSelectionInfo.Text = "Select an imported parcel shape.";
                if (!_suppressCanvasPreviewEvent)
                    SelectedCanvasObjectChanged?.Invoke(null, false);
                return;
            }

            lblSelectionInfo.Text =
                $"{candidate.LayerName} | source: {candidate.SourceLayer ?? "-"} | " +
                $"sheet: {candidate.MapSheetNo ?? "-"} | parcel: {candidate.ParcelNo ?? "-"} | " +
                $"status: {candidate.AssignmentStatus} | area: {candidate.CalculatedAreaSqm:0.##}";
            if (!_suppressCanvasPreviewEvent)
                SelectedCanvasObjectChanged?.Invoke(candidate.CanvasObjectId, chkZoomToSelected.Checked);

            if (!string.IsNullOrWhiteSpace(candidate.MapSheetNo))
                SelectComboValue(cboMapSheet, candidate.MapSheetNo);
        }

        public bool SelectCanvasObjectFromCanvas(Guid canvasObjectId)
        {
            if (TrySelectCandidateFromVisibleList(canvasObjectId))
                return true;

            if (_allCandidates.Any(candidate => candidate.CanvasObjectId == canvasObjectId))
            {
                SelectComboValue(cboObjectFilter, FilterAll);
                ApplyCandidateFilter(canvasObjectId);
                return true;
            }

            return false;
        }

        private bool TrySelectCandidateFromVisibleList(Guid canvasObjectId)
        {
            _suppressCanvasPreviewEvent = true;
            try
            {
                return SelectCandidate(canvasObjectId);
            }
            finally
            {
                _suppressCanvasPreviewEvent = false;
            }
        }

        private void MoveSelection(int delta)
        {
            if (lstObjects.Items.Count == 0)
                return;

            int index = lstObjects.SelectedIndex < 0 ? 0 : lstObjects.SelectedIndex + delta;
            index = Math.Clamp(index, 0, lstObjects.Items.Count - 1);
            lstObjects.SelectedIndex = index;
        }

        private async void btnAssign_Click(object? sender, EventArgs e)
        {
            CadastralAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null || cboParcel.SelectedItem is not ParcelItem parcelItem)
                return;

            try
            {
                SetBusy(true, "Assigning selected parcel...");
                await _assignmentService.AssignManualAsync(
                    _session,
                    candidate.CanvasObjectId,
                    parcelItem.Parcel.Id,
                    chkReplaceExisting.Checked);

                AssignmentChanged = true;
                AssignmentCommitted?.Invoke();
                lblStatus.Text = $"Assigned parcel {parcelItem.Parcel.ParcelNo}.";
                await LoadCandidatesAsync(candidate.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "Assign Cadastral Records",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = "Assignment failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnAutoAssign_Click(object? sender, EventArgs e)
        {
            bool useAttributeMapping = UsesAttributeBasedMapping();
            IReadOnlyList<CadastralLayerMapSheetMapping> mappings = useAttributeMapping ? [] : GetLayerMappings();
            if (!useAttributeMapping && mappings.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "Map at least one imported parcel layer to an Original Parcel Record MapSheetNo before auto assignment.",
                    "Assign Cadastral Records",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                SetBusy(true, useAttributeMapping
                    ? "Remapping from saved source attributes..."
                    : "Auto assigning from layer map and parcel text locations...");

                CadastralAssignmentResult result = useAttributeMapping
                    ? await _assignmentService.AutoAssignFromAttributesAsync(
                        _session,
                        chkReplaceExisting.Checked,
                        GetAttributeFieldMapping())
                    : await _assignmentService.AutoAssignAsync(
                        _session,
                        chkReplaceExisting.Checked,
                        mappings);
                if (!result.Success)
                {
                    MessageBox.Show(
                        this,
                        result.ErrorMessage ?? "Cadastral assignment failed.",
                        "Assign Cadastral Records",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    lblStatus.Text = result.ErrorMessage ?? "Assignment failed.";
                    return;
                }

                AssignmentChanged = result.AssignedCount > 0;
                if (result.AssignedCount > 0)
                    AssignmentCommitted?.Invoke();

                lblStatus.Text =
                    $"Assigned {result.AssignedCount}. Missing layer/text: {result.MissingKeyCount}. No record/conflict: {result.NoRecordMatchCount}.";
                await LoadCandidatesAsync(GetSelectedCandidate()?.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Cadastral assignment failed: {ex.Message}",
                    "Assign Cadastral Records",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = "Assignment failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnClearAssignments_Click(object? sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                this,
                "Remove all record-to-map assignments from imported cadastral parcel objects? Imported source attributes and geometry will be kept.",
                "Assign Cadastral Records",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.OK)
                return;

            try
            {
                SetBusy(true, "Removing parcel assignments...");
                int cleared = await _assignmentService.ClearAssignmentsAsync(_session);
                AssignmentChanged = cleared > 0;
                if (cleared > 0)
                    AssignmentCommitted?.Invoke();

                lblStatus.Text = $"Removed {cleared:N0} assignment(s).";
                await LoadCandidatesAsync(GetSelectedCandidate()?.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Could not remove assignments: {ex.Message}",
                    "Assign Cadastral Records",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = "Remove assignments failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private IReadOnlyList<CadastralLayerMapSheetMapping> GetLayerMappings()
        {
            List<CadastralLayerMapSheetMapping> mappings = [];
            foreach (DataGridViewRow row in dgvLayerMapSheets.Rows)
            {
                if (row.Tag is not LayerMappingRow layer)
                    continue;

                string? mapSheet = Convert.ToString(row.Cells["MapSheet"].Value)?.Trim();
                if (string.IsNullOrWhiteSpace(mapSheet))
                    continue;

                mappings.Add(new CadastralLayerMapSheetMapping(
                    layer.LayerName,
                    layer.SourceLayer,
                    mapSheet));
            }

            return mappings;
        }

        private void ConfigureAttributeMappingGrid()
        {
            dgvAttributeMapSheetMappings.Columns.Clear();
            dgvAttributeMapSheetMappings.EnableHeadersVisualStyles = false;
            dgvAttributeMapSheetMappings.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SourceMapSheet",
                HeaderText = "Source map-sheet value",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            DataGridViewComboBoxColumn targetMapSheetColumn = new()
            {
                Name = "TargetMapSheet",
                HeaderText = "Target MapSheetNo",
                Width = 210,
                FlatStyle = FlatStyle.Flat
            };
            dgvAttributeMapSheetMappings.Columns.Add(targetMapSheetColumn);
            ApplyQuietGridStyle(dgvAttributeMapSheetMappings, enabled: true);
        }

        private void PopulateAttributeFieldSelectors()
        {
            List<string> fields = GetAttributeFieldNames();
            string? previousMapSheet = GetSelectedComboText(cboSourceMapSheetField);
            string? previousParcel = GetSelectedComboText(cboSourceParcelField);

            _suppressAttributeMappingEvents = true;
            try
            {
                cboSourceMapSheetField.Items.Clear();
                cboSourceParcelField.Items.Clear();
                cboSourceMapSheetField.Items.Add(string.Empty);
                cboSourceParcelField.Items.Add(string.Empty);
                foreach (string field in fields)
                {
                    cboSourceMapSheetField.Items.Add(field);
                    cboSourceParcelField.Items.Add(field);
                }

                SelectComboValue(
                    cboSourceMapSheetField,
                    previousMapSheet ?? FindBestAttributeField(
                        fields,
                        ["mapsheet", "map_sheet", "map sheet", "mapsheetno", "map_sheet_no", "sheet", "sheetno", "sheet_no"]));
                SelectComboValue(
                    cboSourceParcelField,
                    previousParcel ?? FindBestAttributeField(
                        fields,
                        ["parcel", "parcelno", "parcel_no", "parcel number", "parcelnumber", "kitta", "kitta_no", "kittano", "plot", "plotno", "plot_no"]));
            }
            finally
            {
                _suppressAttributeMappingEvents = false;
            }
        }

        private void PopulateAttributeMapSheetMappingGrid()
        {
            Dictionary<string, string?> existingSelections = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in dgvAttributeMapSheetMappings.Rows)
            {
                if (row.Tag is string sourceValue)
                    existingSelections[sourceValue] = Convert.ToString(row.Cells["TargetMapSheet"].Value)?.Trim();
            }

            dgvAttributeMapSheetMappings.Rows.Clear();
            DataGridViewComboBoxColumn? targetMapSheetColumn =
                dgvAttributeMapSheetMappings.Columns["TargetMapSheet"] as DataGridViewComboBoxColumn;
            if (targetMapSheetColumn != null)
            {
                targetMapSheetColumn.Items.Clear();
                targetMapSheetColumn.Items.Add(string.Empty);
                foreach (object item in cboMapSheet.Items)
                    targetMapSheetColumn.Items.Add(item.ToString() ?? string.Empty);
            }

            string? mapSheetField = GetSelectedComboText(cboSourceMapSheetField);
            foreach (string sourceValue in GetAttributeUniqueValues(mapSheetField))
            {
                string target = existingSelections.TryGetValue(sourceValue, out string? selected)
                    ? selected ?? string.Empty
                    : ResolveLikelyMapSheet(sourceValue);
                int rowIndex = dgvAttributeMapSheetMappings.Rows.Add(sourceValue, target);
                dgvAttributeMapSheetMappings.Rows[rowIndex].Tag = sourceValue;
            }

            dgvAttributeMapSheetMappings.ClearSelection();
            dgvAttributeMapSheetMappings.CurrentCell = null;
        }

        private CadastralAttributeFieldMapping GetAttributeFieldMapping()
        {
            return new CadastralAttributeFieldMapping(
                GetSelectedComboText(cboSourceMapSheetField),
                GetSelectedComboText(cboSourceParcelField),
                GetAttributeMapSheetValueMappings());
        }

        private Dictionary<string, string> GetAttributeMapSheetValueMappings()
        {
            Dictionary<string, string> mappings = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in dgvAttributeMapSheetMappings.Rows)
            {
                string? sourceValue = row.Tag as string;
                string? targetMapSheet = Convert.ToString(row.Cells["TargetMapSheet"].Value)?.Trim();
                if (string.IsNullOrWhiteSpace(sourceValue) ||
                    string.IsNullOrWhiteSpace(targetMapSheet))
                {
                    continue;
                }

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

        private string ResolveLikelyMapSheet(string sourceValue)
        {
            foreach (object item in cboMapSheet.Items)
            {
                string mapSheet = item.ToString() ?? string.Empty;
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

        private CadastralAssignmentCandidate? GetSelectedCandidate()
        {
            return lstObjects.SelectedItem is CandidateItem item
                ? item.Candidate
                : null;
        }

        private bool SelectCandidate(Guid canvasObjectId)
        {
            for (int index = 0; index < lstObjects.Items.Count; index++)
            {
                if (lstObjects.Items[index] is CandidateItem item &&
                    item.Candidate.CanvasObjectId == canvasObjectId)
                {
                    lstObjects.SelectedIndex = index;
                    return true;
                }
            }

            return false;
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

        private static bool ComboContains(ComboBox comboBox, string value)
        {
            return comboBox.Items
                .Cast<object>()
                .Any(item => string.Equals(item.ToString(), value, StringComparison.OrdinalIgnoreCase));
        }

        private void SetBusy(bool busy, string? status = null)
        {
            btnAssign.Enabled = !busy && _candidates.Count > 0 && cboMapSheet.Items.Count > 0;
            btnAutoAssign.Enabled = !busy && _allCandidates.Count > 0 && cboMapSheet.Items.Count > 0;
            btnClose.Enabled = !busy;
            btnPrevious.Enabled = !busy;
            btnNext.Enabled = !busy;
            btnClearAssignments.Enabled = !busy && _allCandidates.Count > 0;
            cboObjectFilter.Enabled = !busy;
            dgvLayerMapSheets.Enabled = !busy &&
                                        !UsesAttributeBasedMapping() &&
                                        dgvLayerMapSheets.Rows.Count > 0 &&
                                        cboMapSheet.Items.Count > 0;
            ApplyAttributeMappingGridState(!busy && UsesAttributeBasedMapping() && GetAttributeFieldNames().Count > 0);
            cboMapSheet.Enabled = !busy;
            cboParcel.Enabled = !busy;
            chkReplaceExisting.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                lblStatus.Text = status;
        }

        private void ApplyAttributeMappingGridState(bool enabled)
        {
            attributeMappingLayout.Enabled = enabled;
            cboSourceMapSheetField.Enabled = enabled;
            cboSourceParcelField.Enabled = enabled;
            dgvAttributeMapSheetMappings.Enabled = enabled;
            dgvAttributeMapSheetMappings.ReadOnly = !enabled;
            dgvAttributeMapSheetMappings.TabStop = enabled;
            ApplyQuietGridStyle(dgvAttributeMapSheetMappings, enabled);

            foreach (DataGridViewColumn column in dgvAttributeMapSheetMappings.Columns)
                column.ReadOnly = !enabled || column.Name == "SourceMapSheet";

            if (!enabled)
            {
                dgvAttributeMapSheetMappings.ClearSelection();
                dgvAttributeMapSheetMappings.CurrentCell = null;
            }
        }

        private static string BuildLayerMappingKey(string layerName, string? sourceLayer)
        {
            return $"{layerName.Trim()}::{(sourceLayer ?? string.Empty).Trim()}";
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

        private static string? GetSelectedComboText(ComboBox comboBox)
        {
            string? value = Convert.ToString(comboBox.SelectedItem)?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static string? FindBestAttributeField(
            IEnumerable<string> fields,
            IReadOnlyList<string> names)
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

        private static string NormalizeMapSheetValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value
                    .Where(ch => !char.IsWhiteSpace(ch))
                    .Select(char.ToUpperInvariant)
                    .ToArray());
        }

        private static void ApplyQuietGridStyle(DataGridView grid, bool enabled)
        {
            Color backColor = enabled ? SystemColors.Window : SystemColors.Control;
            Color foreColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            Color headerBackColor = enabled ? Color.FromArgb(248, 250, 252) : SystemColors.Control;

            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = backColor;
            grid.GridColor = Color.FromArgb(214, 219, 226);
            grid.DefaultCellStyle.BackColor = backColor;
            grid.DefaultCellStyle.ForeColor = foreColor;
            grid.DefaultCellStyle.SelectionBackColor = backColor;
            grid.DefaultCellStyle.SelectionForeColor = foreColor;
            grid.AlternatingRowsDefaultCellStyle.BackColor = enabled
                ? Color.FromArgb(252, 253, 255)
                : backColor;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = foreColor;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = grid.AlternatingRowsDefaultCellStyle.BackColor;
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = foreColor;
            grid.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = foreColor;
        }

        private sealed record LayerMappingRow(
            string LayerName,
            string? SourceLayer,
            string? MapSheetNo);

        private sealed class CandidateItem(CadastralAssignmentCandidate candidate)
        {
            public CadastralAssignmentCandidate Candidate { get; } = candidate;

            public override string ToString()
            {
                string key = string.IsNullOrWhiteSpace(Candidate.ParcelNo)
                    ? "unassigned"
                    : $"{Candidate.MapSheetNo}-{Candidate.ParcelNo}";
                return $"{Candidate.LayerName} | {key} | {Candidate.AssignmentStatus}";
            }
        }

        private sealed class ParcelItem(CadastralParcelRecordChoice parcel)
        {
            public CadastralParcelRecordChoice Parcel { get; } = parcel;

            public override string ToString() => Parcel.DisplayText;
        }
    }
}
