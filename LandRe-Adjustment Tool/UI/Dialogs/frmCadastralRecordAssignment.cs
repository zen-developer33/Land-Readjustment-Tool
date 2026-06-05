using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services.Assignment;
using System.Drawing;

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
        private readonly bool _openAutoAssignmentOnLoad;
        private readonly Guid? _preferredCanvasObjectId;
        private readonly bool _readOnlyMode;
        private List<CadastralAssignmentCandidate> _allCandidates = [];
        private List<CadastralAssignmentCandidate> _visibleCandidates = [];
        private bool _suppressCanvasPreviewEvent;
        private bool _loadingCandidates;

        public event Action<Guid?, bool>? SelectedCanvasObjectChanged;
        public event Action? AssignmentCommitted;

        public bool AssignmentChanged { get; private set; }

        public frmCadastralRecordAssignment(
            ProjectSession session,
            ICadastralRecordAssignmentService assignmentService,
            bool openAutoAssignmentOnLoad = false,
            Guid? preferredCanvasObjectId = null,
            bool readOnlyMode = false)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _readOnlyMode = readOnlyMode;
            _openAutoAssignmentOnLoad = openAutoAssignmentOnLoad && !readOnlyMode;
            _preferredCanvasObjectId = preferredCanvasObjectId;

            InitializeComponent();
            Load += frmCadastralRecordAssignment_Load;
            FormClosed += (_, _) => SelectedCanvasObjectChanged?.Invoke(null, false);
            cboObjectFilter.SelectedIndexChanged += (_, _) => ApplyCandidateFilter();
            dgvObjects.SelectionChanged += (_, _) => ShowSelectedCandidate();
            dgvObjects.CellDoubleClick += (_, e) =>
            {
                if (!_readOnlyMode && e.RowIndex >= 0 && rdoManualAssign.Checked)
                    OpenParcelPickerForSelectedCandidate();
            };
            rdoAutoAssign.CheckedChanged += (_, _) => ApplyModeState();
            rdoManualAssign.CheckedChanged += (_, _) => ApplyModeState();
            btnOpenAutoAssignment.Click += (_, _) => OpenAutoAssignmentDialog();
            btnPrevious.Click += (_, _) => MoveSelection(-1);
            btnNext.Click += (_, _) => MoveSelection(1);
            btnAssignParcel.Click += (_, _) => OpenParcelPickerForSelectedCandidate();
            btnRemoveAssignment.Click += btnRemoveAssignment_Click;
            btnClearAssignments.Click += btnClearAssignments_Click;
            btnClose.Click += (_, _) => Close();

            if (_preferredCanvasObjectId.HasValue && !_openAutoAssignmentOnLoad)
                rdoManualAssign.Checked = true;

            if (_readOnlyMode)
            {
                Text = "Assign Cadastral Records (Read Only)";
                chkReplaceExisting.Enabled = false;
            }
        }

        private async void frmCadastralRecordAssignment_Load(object? sender, EventArgs e)
        {
            ConfigureObjectFilter();
            ConfigureObjectGrid();
            lblStatus.Text = "Loading cadastral assignment objects...";
            await Task.Yield();
            await LoadCandidatesAsync(_preferredCanvasObjectId);
            ApplyModeState();

            if (_openAutoAssignmentOnLoad)
                BeginInvoke(OpenAutoAssignmentDialog);
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

        private void ConfigureObjectGrid()
        {
            dgvObjects.Columns.Clear();
            dgvObjects.AutoGenerateColumns = false;
            dgvObjects.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvObjects.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "SourceLayer", HeaderText = "Source layer", Width = 115 },
                new DataGridViewTextBoxColumn { Name = "Sheet", HeaderText = "Sheet", Width = 95 },
                new DataGridViewTextBoxColumn { Name = "Parcel", HeaderText = "Parcel", Width = 75 },
                new DataGridViewTextBoxColumn { Name = "Record", HeaderText = "Assigned record", Width = 130 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 110 },
                new DataGridViewTextBoxColumn { Name = "Area", HeaderText = "Area sq.m", Width = 80 }
            });
            ApplyQuietGridStyle(dgvObjects);
        }

        private async Task LoadCandidatesAsync(Guid? preferredSelection = null)
        {
            Guid? selectedBeforeReload = preferredSelection ?? GetSelectedCandidate()?.CanvasObjectId;
            _loadingCandidates = true;
            SetBusy(true, "Loading cadastral assignment objects...");
            await Task.Yield();
            try
            {
                _allCandidates = (await _assignmentService.GetCandidatesAsync(_session)).ToList();
                ApplyCandidateFilter(selectedBeforeReload);
            }
            finally
            {
                _loadingCandidates = false;
                SetBusy(false);
            }
        }

        private void ApplyCandidateFilter(Guid? preferredSelection = null)
        {
            string filter = cboObjectFilter.SelectedItem?.ToString() ?? FilterAll;
            _visibleCandidates = _allCandidates
                .Where(candidate => MatchesFilter(candidate, filter))
                .ToList();

            List<DataGridViewRow> rows = new(_visibleCandidates.Count);
            foreach (CadastralAssignmentCandidate candidate in _visibleCandidates)
            {
                string assignedRecord = candidate.BaselineParcelId.HasValue
                    ? $"{candidate.MapSheetNo ?? "-"} / {candidate.ParcelNo ?? "-"}"
                    : "-";
                DataGridViewRow row = new();
                row.CreateCells(
                    dgvObjects,
                    candidate.SourceLayer ?? "-",
                    candidate.MapSheetNo ?? "-",
                    candidate.ParcelNo ?? "-",
                    assignedRecord,
                    ToFriendlyStatus(candidate.AssignmentStatus),
                    candidate.CalculatedAreaSqm.ToString("0.##"));
                row.Tag = candidate;
                rows.Add(row);
            }

            ReplaceGridRows(dgvObjects, rows);

            lblStatus.Text = _allCandidates.Count == 0
                ? "No imported cadastral parcel objects found."
                : $"{_visibleCandidates.Count:N0} of {_allCandidates.Count:N0} cadastral parcel object(s) shown.";

            if (preferredSelection.HasValue && SelectCandidate(preferredSelection.Value))
            {
                ApplyModeState();
                return;
            }

            DataGridViewRow? defaultRow = dgvObjects.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(row =>
                    row.Tag is CadastralAssignmentCandidate candidate &&
                    !IsAssigned(candidate))
                ?? dgvObjects.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault();
            if (defaultRow != null)
            {
                defaultRow.Selected = true;
                dgvObjects.CurrentCell = defaultRow.Cells[0];
            }
            else
            {
                ShowSelectedCandidate();
            }

            ApplyModeState();
        }

        private static bool MatchesFilter(CadastralAssignmentCandidate candidate, string filter)
        {
            bool isAssigned = IsAssigned(candidate);
            return filter switch
            {
                FilterNotAssigned => !isAssigned,
                FilterAssigned => isAssigned,
                FilterAutoAssigned => string.Equals(candidate.AssignmentStatus, "AutoAssigned", StringComparison.OrdinalIgnoreCase),
                FilterManualAssigned => string.Equals(candidate.AssignmentStatus, "ManualAssigned", StringComparison.OrdinalIgnoreCase),
                _ => true
            };
        }

        private static bool IsAssigned(CadastralAssignmentCandidate candidate)
        {
            return candidate.BaselineParcelId.HasValue ||
                   string.Equals(candidate.AssignmentStatus, "AutoAssigned", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(candidate.AssignmentStatus, "ManualAssigned", StringComparison.OrdinalIgnoreCase);
        }

        private void ShowSelectedCandidate()
        {
            CadastralAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null)
            {
                lblSelectionInfo.Text = "Select an imported parcel object.";
                txtSelectedRecord.Text = string.Empty;
                if (!_suppressCanvasPreviewEvent)
                    SelectedCanvasObjectChanged?.Invoke(null, false);
                ApplyModeState();
                return;
            }

            lblSelectionInfo.Text =
                $"{candidate.LayerName} | source: {candidate.SourceLayer ?? "-"} | " +
                $"sheet: {candidate.MapSheetNo ?? "-"} | parcel: {candidate.ParcelNo ?? "-"} | " +
                $"status: {ToFriendlyStatus(candidate.AssignmentStatus)} | area: {candidate.CalculatedAreaSqm:0.##}";
            txtSelectedRecord.Text = candidate.BaselineParcelId.HasValue
                ? $"{candidate.MapSheetNo ?? "-"} / {candidate.ParcelNo ?? "-"}"
                : "No parcel record assigned";

            if (!_suppressCanvasPreviewEvent)
                SelectedCanvasObjectChanged?.Invoke(candidate.CanvasObjectId, chkZoomToSelected.Checked);

            ApplyModeState();
        }

        public bool SelectCanvasObjectFromCanvas(
            Guid canvasObjectId,
            bool previewOnCanvas = false)
        {
            if (TrySelectCandidateFromVisibleList(canvasObjectId, previewOnCanvas))
                return true;

            if (_allCandidates.Any(candidate => candidate.CanvasObjectId == canvasObjectId))
            {
                SelectComboValue(cboObjectFilter, FilterAll);
                ApplyCandidateFilter(canvasObjectId);
                if (previewOnCanvas)
                    ShowSelectedCandidate();
                return true;
            }

            return false;
        }

        public void OpenManualAssignmentMode()
        {
            rdoManualAssign.Checked = true;
            ApplyModeState();
        }

        private bool TrySelectCandidateFromVisibleList(
            Guid canvasObjectId,
            bool previewOnCanvas)
        {
            if (previewOnCanvas)
            {
                bool selected = SelectCandidate(canvasObjectId);
                if (selected)
                    ShowSelectedCandidate();
                return selected;
            }

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

        private bool SelectCandidate(Guid canvasObjectId)
        {
            foreach (DataGridViewRow row in dgvObjects.Rows)
            {
                if (row.Tag is CadastralAssignmentCandidate candidate &&
                    candidate.CanvasObjectId == canvasObjectId)
                {
                    dgvObjects.ClearSelection();
                    row.Selected = true;
                    dgvObjects.CurrentCell = row.Cells[0];
                    try
                    {
                        dgvObjects.FirstDisplayedScrollingRowIndex = Math.Max(0, row.Index);
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    return true;
                }
            }

            return false;
        }

        private void MoveSelection(int delta)
        {
            if (dgvObjects.Rows.Count == 0)
                return;

            int currentIndex = dgvObjects.CurrentRow?.Index ?? 0;
            int nextIndex = Math.Clamp(currentIndex + delta, 0, dgvObjects.Rows.Count - 1);
            dgvObjects.Rows[nextIndex].Selected = true;
            dgvObjects.CurrentCell = dgvObjects.Rows[nextIndex].Cells[0];
        }

        private async void OpenParcelPickerForSelectedCandidate()
        {
            if (_readOnlyMode)
                return;

            CadastralAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null)
                return;

            using frmCadastralParcelPicker picker = new(
                _session,
                _assignmentService,
                candidate.MapSheetNo,
                candidate.ParcelNo);
            PositionChildAssignmentForm(picker);
            if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedParcel == null)
                return;

            try
            {
                SetBusy(true, "Assigning selected parcel record...");
                await _assignmentService.AssignManualAsync(
                    _session,
                    candidate.CanvasObjectId,
                    picker.SelectedParcel.Id,
                    chkReplaceExisting.Checked);

                AssignmentChanged = true;
                AssignmentCommitted?.Invoke();
                lblStatus.Text = $"Assigned parcel {picker.SelectedParcel.ParcelNo}.";
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

        private async void btnRemoveAssignment_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
                return;

            CadastralAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null || !IsAssigned(candidate))
                return;

            try
            {
                SetBusy(true, "Removing selected assignment...");
                bool removed = await _assignmentService.ClearAssignmentAsync(_session, candidate.CanvasObjectId);
                AssignmentChanged |= removed;
                if (removed)
                    AssignmentCommitted?.Invoke();

                lblStatus.Text = removed ? "Removed selected assignment." : "Selected object has no assignment.";
                await LoadCandidatesAsync(candidate.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Could not remove the selected assignment: {ex.Message}",
                    "Assign Cadastral Records",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = "Remove selected assignment failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnClearAssignments_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
                return;

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
                SetBusy(true, "Removing all parcel assignments...");
                int cleared = await _assignmentService.ClearAssignmentsAsync(_session);
                AssignmentChanged |= cleared > 0;
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

        public async void OpenAutoAssignmentDialog()
        {
            if (_readOnlyMode)
                return;

            using frmCadastralAutoAssignment form = new(_session, _assignmentService);
            PositionChildAssignmentForm(form);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            AssignmentChanged |= form.AssignmentChanged;
            if (form.AssignmentChanged)
                AssignmentCommitted?.Invoke();

            await LoadCandidatesAsync(GetSelectedCandidate()?.CanvasObjectId);
        }

        private void PositionChildAssignmentForm(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = Location;
        }

        private CadastralAssignmentCandidate? GetSelectedCandidate()
        {
            return dgvObjects.CurrentRow?.Tag as CadastralAssignmentCandidate
                ?? dgvObjects.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.Tag as CadastralAssignmentCandidate;
        }

        private void ApplyModeState()
        {
            bool manualMode = rdoManualAssign.Checked;
            bool canEdit = !_readOnlyMode && !_loadingCandidates;
            manualGroup.Enabled = manualMode && canEdit;
            btnOpenAutoAssignment.Enabled = canEdit && rdoAutoAssign.Checked && _allCandidates.Count > 0;

            CadastralAssignmentCandidate? candidate = GetSelectedCandidate();
            btnAssignParcel.Enabled = canEdit && manualMode && candidate != null;
            btnRemoveAssignment.Enabled = canEdit && manualMode && candidate != null && IsAssigned(candidate);
            btnClearAssignments.Enabled = canEdit && manualMode && _allCandidates.Any(IsAssigned);
            chkReplaceExisting.Enabled = canEdit && manualMode;
        }

        private void SetBusy(bool busy, string? status = null)
        {
            rdoAutoAssign.Enabled = !busy && !_readOnlyMode;
            rdoManualAssign.Enabled = !busy;
            btnOpenAutoAssignment.Enabled = !busy && !_readOnlyMode && rdoAutoAssign.Checked && _allCandidates.Count > 0;
            btnClose.Enabled = !busy;
            btnPrevious.Enabled = !busy;
            btnNext.Enabled = !busy;
            cboObjectFilter.Enabled = !busy;
            dgvObjects.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                lblStatus.Text = status;

            if (!busy)
                ApplyModeState();
            else
                manualGroup.Enabled = false;
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

        private static string ToFriendlyStatus(string? status)
        {
            return status switch
            {
                "AutoAssigned" => "Auto assigned",
                "ManualAssigned" => "Manual assigned",
                "Unassigned" or null or "" => "Not assigned",
                _ => status
            };
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

        private void mainLayout_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
