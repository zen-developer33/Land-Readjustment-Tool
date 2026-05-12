using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services.Assignment;

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
            btnPrevious.Click += (_, _) => MoveSelection(-1);
            btnNext.Click += (_, _) => MoveSelection(1);
            btnAssign.Click += btnAssign_Click;
            btnAutoAssign.Click += btnAutoAssign_Click;
            FormClosed += (_, _) => SelectedCanvasObjectChanged?.Invoke(null, false);
        }

        private async void frmCadastralRecordAssignment_Load(object? sender, EventArgs e)
        {
            ConfigureObjectFilter();
            await LoadReferenceDataAsync();
            ConfigureLayerMappingGrid();
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
                HeaderText = "Record MapSheetNo",
                Width = 170,
                FlatStyle = FlatStyle.Flat
            };
            mapSheetColumn.Items.Add(string.Empty);
            foreach (object item in cboMapSheet.Items)
                mapSheetColumn.Items.Add(item.ToString() ?? string.Empty);

            dgvLayerMapSheets.Columns.Add(mapSheetColumn);
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
            foreach (LayerMappingRow layer in _allCandidates
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
            IReadOnlyList<CadastralLayerMapSheetMapping> mappings = GetLayerMappings();
            if (mappings.Count == 0)
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
                SetBusy(true, "Auto assigning from layer map and parcel text locations...");

                CadastralAssignmentResult result =
                    await _assignmentService.AutoAssignAsync(
                        _session,
                        chkReplaceExisting.Checked,
                        mappings);

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

        private static void SelectComboValue(ComboBox comboBox, string value)
        {
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
            cboObjectFilter.Enabled = !busy;
            dgvLayerMapSheets.Enabled = !busy && dgvLayerMapSheets.Rows.Count > 0 && cboMapSheet.Items.Count > 0;
            cboMapSheet.Enabled = !busy;
            cboParcel.Enabled = !busy;
            chkReplaceExisting.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                lblStatus.Text = status;
        }

        private static string BuildLayerMappingKey(string layerName, string? sourceLayer)
        {
            return $"{layerName.Trim()}::{(sourceLayer ?? string.Empty).Trim()}";
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
