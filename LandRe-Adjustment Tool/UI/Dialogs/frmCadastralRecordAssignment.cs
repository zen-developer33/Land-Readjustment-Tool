using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services.Assignment;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmCadastralRecordAssignment : Form
    {
        private readonly ProjectSession _session;
        private readonly ICadastralRecordAssignmentService _assignmentService;
        private List<CadastralAssignmentCandidate> _candidates = [];
        private List<CadastralParcelRecordChoice> _parcelChoices = [];

        public event Action<Guid?, bool>? SelectedCanvasObjectChanged;

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
            cboMapSheet.SelectedIndexChanged += async (_, _) => await LoadParcelsForSelectedMapSheetAsync();
            btnPrevious.Click += (_, _) => MoveSelection(-1);
            btnNext.Click += (_, _) => MoveSelection(1);
            btnAssign.Click += btnAssign_Click;
            btnAutoAssign.Click += btnAutoAssign_Click;
            FormClosed += (_, _) => SelectedCanvasObjectChanged?.Invoke(null, false);
        }

        private async void frmCadastralRecordAssignment_Load(object? sender, EventArgs e)
        {
            await LoadReferenceDataAsync();
            await LoadCandidatesAsync();
        }

        private async Task LoadReferenceDataAsync()
        {
            cboMapSheet.Items.Clear();
            foreach (string mapSheet in await _assignmentService.GetMapSheetNumbersAsync(_session))
                cboMapSheet.Items.Add(mapSheet);

            if (cboMapSheet.Items.Count > 0)
                cboMapSheet.SelectedIndex = 0;
        }

        private async Task LoadCandidatesAsync()
        {
            _candidates = (await _assignmentService.GetCandidatesAsync(_session)).ToList();
            lstObjects.Items.Clear();
            foreach (CadastralAssignmentCandidate candidate in _candidates)
                lstObjects.Items.Add(new CandidateItem(candidate));

            lblStatus.Text = _candidates.Count == 0
                ? "No imported cadastral parcel shapes found."
                : $"{_candidates.Count} imported parcel shapes loaded.";
            btnAssign.Enabled = _candidates.Count > 0 && cboMapSheet.Items.Count > 0;
            btnAutoAssign.Enabled = _candidates.Count > 0 && cboMapSheet.Items.Count > 0;

            if (lstObjects.Items.Count > 0)
                lstObjects.SelectedIndex = 0;
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
                SelectedCanvasObjectChanged?.Invoke(null, false);
                return;
            }

            lblSelectionInfo.Text =
                $"{candidate.LayerName} | source: {candidate.SourceLayer ?? "-"} | " +
                $"sheet: {candidate.MapSheetNo ?? "-"} | parcel: {candidate.ParcelNo ?? "-"} | " +
                $"status: {candidate.AssignmentStatus} | area: {candidate.CalculatedAreaSqm:0.##}";
            SelectedCanvasObjectChanged?.Invoke(candidate.CanvasObjectId, chkZoomToSelected.Checked);

            if (!string.IsNullOrWhiteSpace(candidate.MapSheetNo))
                SelectComboValue(cboMapSheet, candidate.MapSheetNo);
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
                lblStatus.Text = $"Assigned parcel {parcelItem.Parcel.ParcelNo}.";
                await LoadCandidatesAsync();
                SelectCandidate(candidate.CanvasObjectId);
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
            try
            {
                SetBusy(true, "Auto assigning cadastral records...");

                CadastralAssignmentResult result =
                    await _assignmentService.AutoAssignAsync(
                        _session,
                        chkReplaceExisting.Checked);

                AssignmentChanged = result.AssignedCount > 0;
                lblStatus.Text =
                    $"Assigned {result.AssignedCount}. Missing keys: {result.MissingKeyCount}. No record match: {result.NoRecordMatchCount}.";
                await LoadCandidatesAsync();
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

        private CadastralAssignmentCandidate? GetSelectedCandidate()
        {
            return lstObjects.SelectedItem is CandidateItem item
                ? item.Candidate
                : null;
        }

        private void SelectCandidate(Guid canvasObjectId)
        {
            for (int index = 0; index < lstObjects.Items.Count; index++)
            {
                if (lstObjects.Items[index] is CandidateItem item &&
                    item.Candidate.CanvasObjectId == canvasObjectId)
                {
                    lstObjects.SelectedIndex = index;
                    return;
                }
            }
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

        private void SetBusy(bool busy, string? status = null)
        {
            btnAssign.Enabled = !busy;
            btnAutoAssign.Enabled = !busy;
            btnClose.Enabled = !busy;
            btnPrevious.Enabled = !busy;
            btnNext.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                lblStatus.Text = status;
        }

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
