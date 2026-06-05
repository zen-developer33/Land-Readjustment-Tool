using Land_Readjustment_Tool.Core.Models.Assignment;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public partial class frmProjectBoundaryAssignment : Form
    {
        private readonly IReadOnlyList<ProjectBoundaryAssignmentCandidate> _allCandidates;
        private readonly bool _readOnlyMode;
        private bool _suppressSelectionEvents;

        public event Action<Guid?, bool>? CandidatePreviewRequested;

        public Guid? SelectedCandidateId { get; private set; }
        public bool DeleteExistingBoundary => true;
        public bool RemoveProjectBoundaryRequested { get; private set; }
        public bool ImportProjectBoundaryRequested { get; private set; }

        public frmProjectBoundaryAssignment(
            IReadOnlyList<ProjectBoundaryAssignmentCandidate> candidates,
            bool readOnlyMode = false)
        {
            InitializeComponent();
            _allCandidates = candidates ?? [];
            _readOnlyMode = readOnlyMode;
            if (_readOnlyMode)
            {
                Text = "Project Boundary Assignment (Read Only)";
                btnAssign.Enabled = false;
                btnImportBoundary.Enabled = false;
                btnRemoveBoundary.Enabled = false;
            }
            LoadLayerFilters();
            RefreshObjectList();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            PreviewSelectedCandidate();
        }

        private void LoadLayerFilters()
        {
            _suppressSelectionEvents = true;
            try
            {
                cmbLayerFilter.Items.Clear();
                cmbLayerFilter.Items.Add(new LayerFilterItem(null, "All Available Source Layers"));
                cmbLayerFilter.Items.Add(new LayerFilterItem(
                    CanvasLayerTreeService.DrawingMarkupGroupKey,
                    "Drawing/Markup Layers"));
                cmbLayerFilter.Items.Add(new LayerFilterItem(
                    CanvasLayerTreeService.ExternalGroupKey,
                    "Other/External Layers"));

                cmbLayerFilter.SelectedIndex = 0;
            }
            finally
            {
                _suppressSelectionEvents = false;
            }
        }

        private void RefreshObjectList()
        {
            _suppressSelectionEvents = true;
            try
            {
                lstObjects.Items.Clear();
                string? selectedGroupKey =
                    (cmbLayerFilter.SelectedItem as LayerFilterItem)?.LayerGroupKey;

                IEnumerable<ProjectBoundaryAssignmentCandidate> candidates = _allCandidates;
                if (!string.IsNullOrWhiteSpace(selectedGroupKey))
                {
                    candidates = candidates.Where(candidate =>
                        string.Equals(candidate.LayerGroupKey, selectedGroupKey, StringComparison.OrdinalIgnoreCase));
                }

                foreach (ProjectBoundaryAssignmentCandidate candidate in candidates)
                    lstObjects.Items.Add(new CandidateListItem(candidate));

                if (lstObjects.Items.Count > 0)
                {
                    lstObjects.SelectedIndex = 0;
                    lblStatus.Text = $"{lstObjects.Items.Count} project boundary source object(s) available.";
                }
                else
                {
                    lblStatus.Text = $"No project boundary source objects found in {GetSelectedFilterText().ToLowerInvariant()}.";
                    CandidatePreviewRequested?.Invoke(null, false);
                }
            }
            finally
            {
                _suppressSelectionEvents = false;
            }

            UpdateCommandState();
            PreviewSelectedCandidate();
        }

        private void UpdateCommandState()
        {
            bool hasSelection = GetSelectedCandidate() != null;
            btnAssign.Enabled = !_readOnlyMode && hasSelection;
            btnImportBoundary.Enabled = !_readOnlyMode;
            btnRemoveBoundary.Enabled = !_readOnlyMode;
            btnPrevious.Enabled = lstObjects.Items.Count > 1;
            btnNext.Enabled = lstObjects.Items.Count > 1;
        }

        private ProjectBoundaryAssignmentCandidate? GetSelectedCandidate()
        {
            return (lstObjects.SelectedItem as CandidateListItem)?.Candidate;
        }

        private void PreviewSelectedCandidate()
        {
            if (_suppressSelectionEvents)
                return;

            ProjectBoundaryAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null)
            {
                CandidatePreviewRequested?.Invoke(null, false);
                return;
            }

            lblStatus.Text =
                $"{candidate.DisplayName}  |  {candidate.LayerGroupName}";
            CandidatePreviewRequested?.Invoke(
                candidate.CanvasObjectId,
                chkZoomOnSelect.Checked);
        }

        private void MoveSelection(int delta)
        {
            if (lstObjects.Items.Count == 0)
                return;

            int nextIndex = lstObjects.SelectedIndex < 0
                ? 0
                : (lstObjects.SelectedIndex + delta + lstObjects.Items.Count) % lstObjects.Items.Count;

            lstObjects.SelectedIndex = nextIndex;
        }

        private void cmbLayerFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressSelectionEvents)
                return;

            RefreshObjectList();
        }

        private void lstObjects_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateCommandState();
            PreviewSelectedCandidate();
        }

        private void chkZoomOnSelect_CheckedChanged(object? sender, EventArgs e)
        {
            PreviewSelectedCandidate();
        }

        private void btnPrevious_Click(object? sender, EventArgs e)
        {
            MoveSelection(-1);
        }

        private void btnNext_Click(object? sender, EventArgs e)
        {
            MoveSelection(1);
        }

        private void btnAssign_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
                return;

            ProjectBoundaryAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null)
                return;

            SelectedCandidateId = candidate.CanvasObjectId;
            RemoveProjectBoundaryRequested = false;
            ImportProjectBoundaryRequested = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnImportBoundary_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
                return;

            SelectedCandidateId = null;
            RemoveProjectBoundaryRequested = false;
            ImportProjectBoundaryRequested = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnRemoveBoundary_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
                return;

            DialogResult result = MessageBox.Show(
                this,
                "Remove all existing Project Boundary objects?",
                "Remove Project Boundary",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            SelectedCandidateId = null;
            RemoveProjectBoundaryRequested = true;
            ImportProjectBoundaryRequested = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void frmProjectBoundaryAssignment_FormClosed(object? sender, FormClosedEventArgs e)
        {
            CandidatePreviewRequested?.Invoke(null, false);
        }

        private string GetSelectedFilterText()
        {
            return (cmbLayerFilter.SelectedItem as LayerFilterItem)?.Text ?? "available source layers";
        }

        private sealed record LayerFilterItem(string? LayerGroupKey, string Text)
        {
            public override string ToString() => Text;
        }

        private sealed record CandidateListItem(ProjectBoundaryAssignmentCandidate Candidate)
        {
            public override string ToString() => Candidate.DisplayName;
        }
    }
}
