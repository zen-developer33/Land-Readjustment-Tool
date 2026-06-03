using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Services.Assignment;
using System.Drawing;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmBlockAssignment : Form
    {
        private const string SourceLayerColumn = "colSourceLayer";
        private const string BlockColumn = "colBlock";
        private const string ObjectBlockColumn = "colObjectBlock";

        private readonly ProjectSession _session;
        private readonly IBlockAssignmentService _assignmentService;
        private readonly Guid? _preferredCanvasObjectId;
        private List<BlockAssignmentCandidate> _candidates = [];
        private List<BlockRecordChoice> _blocks = [];
        private List<BlockLabelSourceChoice> _labelSources = [];
        private bool _loading;

        public event Action<Guid?, bool>? SelectedCanvasObjectChanged;
        public event Action? AssignmentCommitted;

        public bool AssignmentChanged { get; private set; }

        public frmBlockAssignment(
            ProjectSession session,
            IBlockAssignmentService assignmentService,
            Guid? preferredCanvasObjectId = null)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _preferredCanvasObjectId = preferredCanvasObjectId;

            InitializeComponent();
            ApplyQuietGridStyle(_dgvLayerMappings);
            ApplyQuietGridStyle(_dgvObjects);

            Load += frmBlockAssignment_Load;
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
            _rdoAutoLabels.CheckedChanged += (_, _) => ApplyAssignmentMode();
            _btnApplyMappings.Click += btnApplyMappings_Click;
            _btnAutoAssign.Click += btnAutoAssign_Click;
            _btnAssignSelected.Click += btnAssignSelected_Click;
            _btnRemoveSelected.Click += btnRemoveSelected_Click;
            _btnRemoveAll.Click += btnRemoveAll_Click;
            _btnClose.Click += (_, _) => Close();

            if (preferredCanvasObjectId.HasValue)
                _rdoObject.Checked = true;

            ApplyAssignmentMode();
        }

        private async void frmBlockAssignment_Load(object? sender, EventArgs e)
        {
            SetBusy(true, "Loading block objects...");
            await Task.Yield();
            await ReloadAsync(_preferredCanvasObjectId);
        }

        private async Task ReloadAsync(Guid? preferredObjectId = null)
        {
            _loading = true;
            SetBusy(true, "Loading block objects...");
            await Task.Yield();
            try
            {
                _blocks = (await _assignmentService.GetBlocksAsync(_session)).ToList();
                _labelSources = (await _assignmentService.GetLabelSourcesAsync(_session)).ToList();
                _candidates = (await _assignmentService.GetCandidatesAsync(_session)).ToList();
                _dgvLayerMappings.Rows.Clear();
                _dgvObjects.Rows.Clear();
                PopulateBlockComboColumns();
                PopulateLabelLayerChoices();
                PopulateLayerMappings();
                PopulateObjects(preferredObjectId);
                if (preferredObjectId.HasValue)
                    _rdoObject.Checked = true;

                _lblStatus.Text = $"{_candidates.Count:N0} block object(s), {_blocks.Count:N0} block definition(s).";
                ApplyAssignmentMode();
            }
            finally
            {
                _loading = false;
                SetBusy(false);
            }

            ShowSelectedObjectOnCanvas();
        }

        private void PopulateBlockComboColumns()
        {
            PopulateBlockComboColumn(colBlock);
            PopulateBlockComboColumn(colObjectBlock);
        }

        private void PopulateBlockComboColumn(DataGridViewComboBoxColumn? column)
        {
            if (column == null)
                return;

            column.DataSource = null;
            column.DisplayMember = nameof(BlockRecordChoice.DisplayText);
            column.ValueMember = nameof(BlockRecordChoice.Id);
            column.ValueType = typeof(int);
            column.DefaultCellStyle.NullValue = string.Empty;
            column.DefaultCellStyle.DataSourceNullValue = null;
            column.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            column.DropDownWidth = Math.Max(column.Width, 260);
            column.DataSource = _blocks.ToList();
        }

        private void PopulateLabelLayerChoices()
        {
            _cboLabelLayer.DataSource = null;
            _cboLabelLayer.DisplayMember = nameof(BlockLabelSourceChoice.DisplayText);
            _cboLabelLayer.ValueMember = nameof(BlockLabelSourceChoice.SourceLayer);
            _cboLabelLayer.DataSource = _labelSources.ToList();
        }

        private void PopulateLayerMappings()
        {
            List<DataGridViewRow> rows = [];
            foreach (IGrouping<string, BlockAssignmentCandidate> group in _candidates
                .GroupBy(candidate => candidate.SourceLayer, StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                int? selectedBlockId = ResolveMostCommonBlockId(group);
                DataGridViewRow row = new();
                row.CreateCells(
                    _dgvLayerMappings,
                    group.Key,
                    group.Count(),
                    selectedBlockId.HasValue ? selectedBlockId.Value : null!);
                row.Tag = group.Key;
                rows.Add(row);
            }

            ReplaceGridRows(_dgvLayerMappings, rows);
        }

        private void PopulateObjects(Guid? preferredObjectId)
        {
            List<DataGridViewRow> rows = new(_candidates.Count);
            int preferredRowIndex = -1;
            foreach (BlockAssignmentCandidate candidate in _candidates
                .OrderBy(candidate => candidate.SourceLayer, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.BlockCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.GeometryArea))
            {
                DataGridViewRow row = new();
                row.CreateCells(
                    _dgvObjects,
                    candidate.SourceLayer,
                    candidate.GeometryArea.ToString("0.##"),
                    candidate.DetectedLabel ?? string.Empty,
                    candidate.BlockId.HasValue ? candidate.BlockId.Value : null!);
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

        private static int? ResolveMostCommonBlockId(IEnumerable<BlockAssignmentCandidate> candidates)
        {
            return candidates
                .Where(candidate => candidate.BlockId.HasValue)
                .GroupBy(candidate => candidate.BlockId!.Value)
                .OrderByDescending(group => group.Count())
                .Select(group => (int?)group.Key)
                .FirstOrDefault();
        }

        private async void btnApplyMappings_Click(object? sender, EventArgs e)
        {
            Dictionary<string, int> mappings = GetSourceLayerMappings();
            if (mappings.Count == 0)
            {
                MessageBox.Show(this, "Map at least one source layer to a defined block.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                SetBusy(true, "Assigning blocks from source layer mappings...");
                int assigned = await _assignmentService.AssignBySourceLayerAsync(
                    _session,
                    mappings,
                    _chkReplaceExisting.Checked);
                AssignmentChanged |= assigned > 0;
                if (assigned > 0)
                    AssignmentCommitted?.Invoke();

                _lblStatus.Text = $"Assigned {assigned:N0} block object(s).";
                await ReloadAsync(GetSelectedCandidate()?.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Block assignment failed: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "Block assignment failed.";
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
                SetBusy(true, "Assigning blocks from contained label text...");
                BlockAutoAssignmentResult result = await _assignmentService.AutoAssignFromLabelsAsync(
                    _session,
                    GetSelectedLabelSource()?.SourceLayer,
                    _chkCreateMissingBlocks.Checked,
                    _chkReplaceExisting.Checked);
                AssignmentChanged |= result.BlocksAssigned > 0 || result.BlocksDefined > 0;
                if (result.BlocksAssigned > 0 || result.BlocksDefined > 0)
                    AssignmentCommitted?.Invoke();

                _lblStatus.Text = $"Defined {result.BlocksDefined:N0}, assigned {result.BlocksAssigned:N0}, missing data {result.MissingDefinitions:N0}.";
                await ReloadAsync(GetSelectedCandidate()?.CanvasObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Auto block assignment failed: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "Auto block assignment failed.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnAssignSelected_Click(object? sender, EventArgs e)
        {
            BlockAssignmentCandidate? candidate = GetSelectedCandidate();
            BlockRecordChoice? block = GetSelectedObjectBlock();
            if (candidate == null || block == null)
                return;

            try
            {
                SetBusy(true, "Assigning selected block...");
                await _assignmentService.AssignManualAsync(_session, candidate.CanvasObjectId, block.Id);
                AssignmentChanged = true;
                AssignmentCommitted?.Invoke();
                _lblStatus.Text = $"Assigned selected object to {block.BlockName}.";
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
            BlockAssignmentCandidate? candidate = GetSelectedCandidate();
            if (candidate == null)
                return;

            try
            {
                SetBusy(true, "Removing selected block assignment...");
                bool removed = await _assignmentService.ClearAssignmentAsync(_session, candidate.CanvasObjectId);
                AssignmentChanged |= removed;
                if (removed)
                    AssignmentCommitted?.Invoke();

                _lblStatus.Text = removed ? "Removed selected block assignment." : "Selected object has no block assignment.";
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
                "Remove all block assignments?",
                Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            Guid? selectedObjectId = GetSelectedCandidate()?.CanvasObjectId;
            try
            {
                SetBusy(true, "Removing all block assignments...");
                int removed = await _assignmentService.ClearAllAssignmentsAsync(_session);
                AssignmentChanged |= removed > 0;
                if (removed > 0)
                    AssignmentCommitted?.Invoke();

                _lblStatus.Text = removed > 0
                    ? $"Removed {removed:N0} block assignment(s)."
                    : "There are no block assignments to remove.";
                await ReloadAsync(selectedObjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not remove block assignments: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                int? blockId = GetBlockIdFromCellValue(row.Cells[BlockColumn].Value);
                if (string.IsNullOrWhiteSpace(sourceLayer) || !blockId.HasValue)
                    continue;

                mappings[sourceLayer] = blockId.Value;
            }

            return mappings;
        }

        private BlockAssignmentCandidate? GetSelectedCandidate()
        {
            return _dgvObjects.CurrentRow?.Tag as BlockAssignmentCandidate
                ?? _dgvObjects.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.Tag as BlockAssignmentCandidate;
        }

        private BlockRecordChoice? GetSelectedObjectBlock()
        {
            DataGridViewRow? row = _dgvObjects.CurrentRow
                ?? _dgvObjects.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            int? blockId = GetBlockIdFromCellValue(row?.Cells[ObjectBlockColumn].Value);
            return !blockId.HasValue
                ? null
                : _blocks.FirstOrDefault(block => block.Id == blockId.Value);
        }

        private BlockLabelSourceChoice? GetSelectedLabelSource()
        {
            return _cboLabelLayer.SelectedItem as BlockLabelSourceChoice;
        }

        private static int? GetBlockIdFromCellValue(object? value)
        {
            return value switch
            {
                int id => id,
                BlockRecordChoice item => item.Id,
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

            BlockAssignmentCandidate? candidate = GetSelectedCandidate();
            SelectedCanvasObjectChanged?.Invoke(candidate?.CanvasObjectId, candidate != null && _chkZoomToSelected.Checked);
        }

        public bool SelectCanvasObjectFromCanvas(
            Guid canvasObjectId,
            bool previewOnCanvas = false)
        {
            foreach (DataGridViewRow row in _dgvObjects.Rows)
            {
                if (row.Tag is BlockAssignmentCandidate candidate &&
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
            _chkCreateMissingBlocks.Enabled = !busy;
            _chkZoomToSelected.Enabled = !busy;
            _cboLabelLayer.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                _lblStatus.Text = status;

            if (!busy)
                ApplyAssignmentMode();
            else
            {
                _btnApplyMappings.Enabled = false;
                _btnAutoAssign.Enabled = false;
                _btnAssignSelected.Enabled = false;
                _btnRemoveSelected.Enabled = false;
                _btnRemoveAll.Enabled = false;
                _rdoSourceLayer.Enabled = false;
                _rdoObject.Enabled = false;
                _rdoAutoLabels.Enabled = false;
            }
        }

        private void ApplyAssignmentMode()
        {
            bool bySourceLayer = _rdoSourceLayer.Checked;
            bool manual = _rdoObject.Checked;
            bool autoLabels = _rdoAutoLabels.Checked;
            bool hasBlocks = _blocks.Count > 0;
            bool hasObjects = _dgvObjects.Rows.Count > 0;

            _rdoSourceLayer.Enabled = true;
            _rdoObject.Enabled = true;
            _rdoAutoLabels.Enabled = true;
            _dgvLayerMappings.Visible = bySourceLayer;
            _dgvObjects.Visible = !bySourceLayer;
            _chkZoomToSelected.Visible = manual;
            _chkReplaceExisting.Visible = bySourceLayer || autoLabels;
            _chkCreateMissingBlocks.Visible = autoLabels;
            _lblLabelLayer.Visible = autoLabels;
            _cboLabelLayer.Visible = autoLabels;
            _btnApplyMappings.Visible = bySourceLayer;
            _btnAutoAssign.Visible = autoLabels;
            _btnAssignSelected.Visible = manual;
            _btnRemoveSelected.Visible = manual;
            _btnRemoveAll.Visible = true;
            _btnApplyMappings.Enabled = bySourceLayer && hasBlocks && _candidates.Count > 0;
            _btnAutoAssign.Enabled = autoLabels && hasObjects;
            _btnAssignSelected.Enabled = manual && hasBlocks && hasObjects;
            _btnRemoveSelected.Enabled = manual && hasObjects;
            _btnRemoveAll.Enabled = hasObjects;
            AcceptButton = bySourceLayer
                ? _btnApplyMappings
                : autoLabels
                    ? _btnAutoAssign
                    : _btnAssignSelected;
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
    }
}
