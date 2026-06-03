using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Services.Assignment;
using System.Drawing;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmRoadCenterlineAssignment : Form
    {
        private const string SourceLayerColumn = "colSourceLayer";
        private const string RoadColumn = "colRoad";
        private const string ObjectSourceColumn = "colObjectSource";
        private const string ObjectAssignedRoadColumn = "colObjectRoad";

        private readonly ProjectSession _session;
        private readonly IRoadCenterlineAssignmentService _assignmentService;
        private readonly Guid? _preferredCanvasObjectId;
        private List<RoadCenterlineAssignmentCandidate> _candidates = [];
        private List<RoadRecordChoice> _roads = [];
        private bool _loading;

        public event Action<Guid?, bool>? SelectedCanvasObjectChanged;
        public event Action? AssignmentCommitted;

        public bool AssignmentChanged { get; private set; }

        public frmRoadCenterlineAssignment(
            ProjectSession session,
            IRoadCenterlineAssignmentService assignmentService,
            Guid? preferredCanvasObjectId = null)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _preferredCanvasObjectId = preferredCanvasObjectId;

            InitializeComponent();
            Load += frmRoadCenterlineAssignment_Load;
            FormClosed += (_, _) => SelectedCanvasObjectChanged?.Invoke(null, false);
            _dgvObjects.SelectionChanged += (_, _) => ShowSelectedObjectOnCanvas();
            _dgvObjects.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_dgvObjects.IsCurrentCellDirty)
                    _dgvObjects.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _dgvLayerMappings.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_dgvLayerMappings.IsCurrentCellDirty)
                    _dgvLayerMappings.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _dgvObjects.DataError += Grid_DataError;
            _dgvLayerMappings.DataError += Grid_DataError;
            _rdoSourceLayer.CheckedChanged += (_, _) => ApplyAssignmentMode();
            _rdoObject.CheckedChanged += (_, _) => ApplyAssignmentMode();
            _btnApplyMappings.Click += btnApplyMappings_Click;
            _btnAssignSelected.Click += btnAssignSelected_Click;
            _btnRemoveSelected.Click += btnRemoveSelected_Click;
            _btnRemoveAll.Click += btnRemoveAll_Click;
            _btnClose.Click += (_, _) => Close();

            if (preferredCanvasObjectId.HasValue)
                _rdoObject.Checked = true;

            ApplyAssignmentMode();
        }

        private async void frmRoadCenterlineAssignment_Load(object? sender, EventArgs e)
        {
            SetBusy(true, "Loading road centerline objects...");
            await Task.Yield();
            await ReloadAsync(_preferredCanvasObjectId);
        }

        private async Task ReloadAsync(Guid? preferredObjectId = null)
        {
            _loading = true;
            SetBusy(true, "Loading road centerline objects...");
            await Task.Yield();
            try
            {
                _roads = (await _assignmentService.GetRoadsAsync(_session)).ToList();
                _candidates = (await _assignmentService.GetCandidatesAsync(_session)).ToList();
                _dgvLayerMappings.Rows.Clear();
                _dgvObjects.Rows.Clear();
                PopulateRoadComboColumns();
                PopulateLayerMappings();
                PopulateObjects(preferredObjectId);
                if (preferredObjectId.HasValue)
                    _rdoObject.Checked = true;

                _lblStatus.Text = $"{_candidates.Count:N0} road centerline object(s), {_roads.Count:N0} road definition(s).";
                ApplyAssignmentMode();
            }
            finally
            {
                _loading = false;
                SetBusy(false);
            }

            ShowSelectedObjectOnCanvas();
        }

        private void PopulateRoadComboColumns()
        {
            PopulateRoadComboColumn(colRoad);
            PopulateRoadComboColumn(colObjectRoad);
        }

        private void PopulateRoadComboColumn(DataGridViewComboBoxColumn? column)
        {
            if (column == null)
                return;

            column.DataSource = null;
            column.DisplayMember = nameof(RoadRecordChoice.DisplayText);
            column.ValueMember = nameof(RoadRecordChoice.Id);
            column.ValueType = typeof(int);
            column.DefaultCellStyle.NullValue = string.Empty;
            column.DefaultCellStyle.DataSourceNullValue = null;
            column.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            column.DropDownWidth = Math.Max(column.Width, 260);
            column.DataSource = BuildRoadChoiceItems();
        }

        private List<RoadRecordChoice> BuildRoadChoiceItems()
        {
            return _roads.ToList();
        }

        private void PopulateLayerMappings()
        {
            List<DataGridViewRow> rows = [];
            foreach (IGrouping<string, RoadCenterlineAssignmentCandidate> group in _candidates
                .GroupBy(candidate => candidate.SourceLayer, StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                int? selectedRoadId = ResolveMostCommonRoadId(group);
                DataGridViewRow row = new();
                row.CreateCells(_dgvLayerMappings, group.Key, group.Count(), selectedRoadId);
                row.Tag = group.Key;
                rows.Add(row);
            }

            ReplaceGridRows(_dgvLayerMappings, rows);
        }

        private void PopulateObjects(Guid? preferredObjectId)
        {
            List<DataGridViewRow> rows = new(_candidates.Count);
            int preferredRowIndex = -1;
            foreach (RoadCenterlineAssignmentCandidate candidate in _candidates
                .OrderBy(candidate => candidate.SourceLayer, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.RoadCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.GeometryLength))
            {
                DataGridViewRow row = new();
                row.CreateCells(
                    _dgvObjects,
                    candidate.SourceLayer,
                    candidate.GeometryLength.ToString("0.##"),
                    candidate.RoadId);
                row.Tag = candidate;
                rows.Add(row);

                if (preferredObjectId == candidate.CanvasObjectId)
                    preferredRowIndex = rows.Count - 1;
            }

            ReplaceGridRows(_dgvObjects, rows);

            if (_dgvObjects.Rows.Count == 0)
                return;

            int selectedRowIndex = preferredRowIndex >= 0 ? preferredRowIndex : 0;
            _dgvObjects.ClearSelection();
            _dgvObjects.Rows[selectedRowIndex].Selected = true;
            _dgvObjects.CurrentCell = _dgvObjects.Rows[selectedRowIndex].Cells[0];
            _dgvObjects.FirstDisplayedScrollingRowIndex = Math.Max(0, selectedRowIndex);
        }

        private int? ResolveMostCommonRoadId(IEnumerable<RoadCenterlineAssignmentCandidate> candidates)
        {
            return candidates
                .Where(candidate => candidate.RoadId.HasValue)
                .GroupBy(candidate => candidate.RoadId!.Value)
                .OrderByDescending(group => group.Count())
                .Select(group => (int?)group.Key)
                .FirstOrDefault();
        }

        private async void btnApplyMappings_Click(object? sender, EventArgs e)
        {
            Dictionary<string, int> mappings = GetSourceLayerMappings();
            if (mappings.Count == 0)
            {
                MessageBox.Show(this, "Map at least one source layer to a defined road.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                SetBusy(true, "Assigning road centerlines from source layer mappings...");
                int assigned = await _assignmentService.AssignBySourceLayerAsync(
                    _session,
                    mappings,
                    _chkReplaceExisting.Checked);
                AssignmentChanged |= assigned > 0;
                if (assigned > 0)
                    AssignmentCommitted?.Invoke();

                _lblStatus.Text = $"Assigned {assigned:N0} road centerline object(s).";
                await ReloadAsync(GetSelectedCandidate()?.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Road assignment failed: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "Road assignment failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnAssignSelected_Click(object? sender, EventArgs e)
        {
            RoadCenterlineAssignmentCandidate? candidate = GetSelectedCandidate();
            RoadRecordChoice? road = GetSelectedObjectRoad();
            if (candidate == null || road == null)
                return;

            try
            {
                SetBusy(true, "Assigning selected road centerline...");
                await _assignmentService.AssignManualAsync(_session, candidate.CanvasObjectId, road.Id);
                AssignmentChanged = true;
                AssignmentCommitted?.Invoke();
                _lblStatus.Text = $"Assigned selected object to {road.RoadName}.";
                await ReloadAsync(candidate.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not assign selected object: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "Selected assignment failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnRemoveSelected_Click(object? sender, EventArgs e)
        {
            RoadCenterlineAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null)
                return;

            try
            {
                SetBusy(true, "Removing selected road assignment...");
                bool removed = await _assignmentService.ClearAssignmentAsync(_session, candidate.CanvasObjectId);
                AssignmentChanged |= removed;
                if (removed)
                    AssignmentCommitted?.Invoke();

                _lblStatus.Text = removed ? "Removed selected road assignment." : "Selected object has no road assignment.";
                await ReloadAsync(candidate.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not remove selected assignment: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "Remove selected assignment failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnRemoveAll_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                this,
                "Remove all road assignments?",
                Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            Guid? selectedObjectId = GetSelectedCandidate()?.CanvasObjectId;
            try
            {
                SetBusy(true, "Removing all road assignments...");
                int removed = await _assignmentService.ClearAllAssignmentsAsync(_session);
                AssignmentChanged |= removed > 0;
                if (removed > 0)
                    AssignmentCommitted?.Invoke();

                _lblStatus.Text = removed > 0
                    ? $"Removed {removed:N0} road assignment(s)."
                    : "There are no road assignments to remove.";
                await ReloadAsync(selectedObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not remove road assignments: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "Remove all assignments failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private Dictionary<string, int> GetSourceLayerMappings()
        {
            Dictionary<string, int> mappings = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in _dgvLayerMappings.Rows)
            {
                string? sourceLayer = row.Tag as string;
                int? roadId = GetRoadIdFromCellValue(row.Cells[RoadColumn].Value);
                if (string.IsNullOrWhiteSpace(sourceLayer) || !roadId.HasValue)
                    continue;

                mappings[sourceLayer] = roadId.Value;
            }

            return mappings;
        }

        private RoadCenterlineAssignmentCandidate? GetSelectedCandidate()
        {
            return _dgvObjects.CurrentRow?.Tag as RoadCenterlineAssignmentCandidate
                ?? _dgvObjects.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.Tag as RoadCenterlineAssignmentCandidate;
        }

        private RoadRecordChoice? GetSelectedObjectRoad()
        {
            DataGridViewRow? row = _dgvObjects.CurrentRow
                ?? _dgvObjects.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            int? roadId = GetRoadIdFromCellValue(row?.Cells[ObjectAssignedRoadColumn].Value);
            return !roadId.HasValue
                ? null
                : _roads.FirstOrDefault(road => road.Id == roadId.Value);
        }

        private static int? GetRoadIdFromCellValue(object? value)
        {
            return value switch
            {
                int id => id,
                RoadRecordChoice item => item.Id,
                string text when int.TryParse(text, out int id) => id,
                _ => null
            };
        }

        private static void Grid_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = false;
            if (sender is DataGridView grid &&
                e.RowIndex >= 0 &&
                e.ColumnIndex >= 0)
            {
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = null;
            }
        }

        private void ShowSelectedObjectOnCanvas()
        {
            if (_loading)
                return;

            RoadCenterlineAssignmentCandidate? candidate = GetSelectedCandidate();
            SelectedCanvasObjectChanged?.Invoke(candidate?.CanvasObjectId, candidate != null && _chkZoomToSelected.Checked);
        }

        public bool SelectCanvasObjectFromCanvas(
            Guid canvasObjectId,
            bool previewOnCanvas = false)
        {
            foreach (DataGridViewRow row in _dgvObjects.Rows)
            {
                if (row.Tag is RoadCenterlineAssignmentCandidate candidate &&
                    candidate.CanvasObjectId == canvasObjectId)
                {
                    row.Selected = true;
                    _dgvObjects.CurrentCell = row.Cells[0];
                    _rdoObject.Checked = true;
                    if (previewOnCanvas)
                        ShowSelectedObjectOnCanvas();
                    return true;
                }
            }

            return false;
        }

        private void SetBusy(bool busy, string? status = null)
        {
            _btnClose.Enabled = !busy;
            _dgvLayerMappings.Enabled = !busy;
            _dgvObjects.Enabled = !busy;
            _chkReplaceExisting.Enabled = !busy;
            _chkZoomToSelected.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                _lblStatus.Text = status;

            if (!busy)
                ApplyAssignmentMode();
            else
            {
                _btnApplyMappings.Enabled = false;
                _btnAssignSelected.Enabled = false;
                _btnRemoveSelected.Enabled = false;
                _btnRemoveAll.Enabled = false;
                _rdoSourceLayer.Enabled = false;
                _rdoObject.Enabled = false;
            }
        }

        private void ApplyAssignmentMode()
        {
            bool bySourceLayer = _rdoSourceLayer.Checked;
            bool hasRoads = _roads.Count > 0;
            bool hasObjects = _dgvObjects.Rows.Count > 0;

            _rdoSourceLayer.Enabled = true;
            _rdoObject.Enabled = true;
            _dgvLayerMappings.Visible = bySourceLayer;
            _dgvObjects.Visible = !bySourceLayer;
            _chkReplaceExisting.Visible = bySourceLayer;
            _chkZoomToSelected.Visible = !bySourceLayer;
            _btnApplyMappings.Visible = bySourceLayer;
            _btnAssignSelected.Visible = !bySourceLayer;
            _btnRemoveSelected.Visible = !bySourceLayer;
            _btnRemoveAll.Visible = true;
            _btnApplyMappings.Enabled = bySourceLayer && hasRoads && _candidates.Count > 0;
            _btnAssignSelected.Enabled = !bySourceLayer && hasRoads && hasObjects;
            _btnRemoveSelected.Enabled = !bySourceLayer && hasObjects;
            _btnRemoveAll.Enabled = hasObjects;
            AcceptButton = bySourceLayer ? _btnApplyMappings : _btnAssignSelected;
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

        private void _chkZoomToSelected_CheckedChanged(object sender, EventArgs e)
        {

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
    }
}
