using Land_Readjustment_Tool.Core.Entities.Layout;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Services.Assignment;
using Land_Readjustment_Tool.UI.Forms.Definitions;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmRoadCenterlineAssignment : Form
    {
        private const string SourceLayerColumn = "colSourceLayer";
        private const string RoadColumn = "colRoad";
        private const string ObjectSourceColumn = "colObjectSource";
        private const string ObjectAssignedRoadColumn = "colObjectRoad";
        private static readonly Color EditableComboBackColor = Color.FromArgb(255, 250, 232);

        private readonly ProjectSession _session;
        private readonly IRoadCenterlineAssignmentService _assignmentService;
        private readonly Guid? _preferredCanvasObjectId;
        private List<RoadCenterlineAssignmentCandidate> _candidates = [];
        private List<RoadRecordChoice> _roads = [];
        private readonly Dictionary<string, int> _typedRoadValueOverrides = new(StringComparer.OrdinalIgnoreCase);
        private bool _loading;
        private bool _resolvingComboText;

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
            _dgvObjects.EditingControlShowing += RoadGrid_EditingControlShowing;
            _dgvLayerMappings.EditingControlShowing += RoadGrid_EditingControlShowing;
            _dgvObjects.CellValidating += RoadGrid_CellValidating;
            _dgvLayerMappings.CellValidating += RoadGrid_CellValidating;
            _dgvObjects.CellParsing += RoadGrid_CellParsing;
            _dgvLayerMappings.CellParsing += RoadGrid_CellParsing;
            _dgvObjects.CellDoubleClick += RoadGrid_CellDoubleClick;
            _dgvLayerMappings.CellDoubleClick += RoadGrid_CellDoubleClick;
            _dgvObjects.CellFormatting += RoadGrid_CellFormatting;
            _dgvLayerMappings.CellFormatting += RoadGrid_CellFormatting;
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
            column.DisplayMember = nameof(RoadRecordChoice.RoadName);
            column.ValueMember = nameof(RoadRecordChoice.Id);
            column.ValueType = typeof(int);
            column.DefaultCellStyle.NullValue = string.Empty;
            column.DefaultCellStyle.DataSourceNullValue = null;
            column.DefaultCellStyle.BackColor = EditableComboBackColor;
            column.DefaultCellStyle.ForeColor = SystemColors.ControlText;
            column.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 239, 255);
            column.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
            column.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            column.DropDownWidth = Math.Max(column.Width, 180);
            column.DataSource = BuildRoadChoiceItems();
        }

        private void RoadGrid_EditingControlShowing(
            object? sender,
            DataGridViewEditingControlShowingEventArgs e)
        {
            if (sender is not DataGridView grid ||
                !IsRoadComboColumn(grid, grid.CurrentCell?.ColumnIndex ?? -1) ||
                e.Control is not DataGridViewComboBoxEditingControl combo)
            {
                return;
            }

            combo.DropDownStyle = ComboBoxStyle.DropDown;
            combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            combo.AutoCompleteSource = AutoCompleteSource.ListItems;
            combo.BackColor = EditableComboBackColor;
            combo.ForeColor = SystemColors.ControlText;
        }

        private void RoadGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView grid ||
                e.RowIndex < 0 ||
                !IsRoadComboColumn(grid, e.ColumnIndex))
            {
                return;
            }

            DataGridViewCellStyle style = e.CellStyle;
            style.BackColor = EditableComboBackColor;
            style.ForeColor = SystemColors.ControlText;
            style.SelectionBackColor = Color.FromArgb(226, 239, 255);
            style.SelectionForeColor = SystemColors.ControlText;
        }

        private void RoadGrid_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_loading ||
                _resolvingComboText ||
                sender is not DataGridView grid ||
                e.RowIndex < 0 ||
                !IsRoadComboColumn(grid, e.ColumnIndex))
            {
                return;
            }

            string typedText = e.FormattedValue?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(typedText))
                return;

            RoadRecordChoice? existing = FindRoadChoice(typedText);
            if (existing != null)
            {
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = existing.Id;
                return;
            }

            _resolvingComboText = true;
            try
            {
                RoadRecordChoice? created = CreateOrEditRoadFromTypedName(typedText, grid, e.RowIndex);
                if (created == null)
                {
                    e.Cancel = true;
                    return;
                }

                RefreshRoadChoices();
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = created.Id;
                _typedRoadValueOverrides[NormalizeLookup(typedText)] = created.Id;
                AssignmentChanged = true;
                AssignmentCommitted?.Invoke();
            }
            finally
            {
                _resolvingComboText = false;
            }
        }

        private void RoadGrid_CellParsing(object? sender, DataGridViewCellParsingEventArgs e)
        {
            if (sender is not DataGridView grid ||
                e.RowIndex < 0 ||
                !IsRoadComboColumn(grid, e.ColumnIndex) ||
                e.Value is not string text ||
                string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            string normalized = NormalizeLookup(text);
            if (_typedRoadValueOverrides.TryGetValue(normalized, out int overrideId))
            {
                e.Value = overrideId;
                e.ParsingApplied = true;
                return;
            }

            RoadRecordChoice? existing = FindRoadChoice(text);
            if (existing == null)
                return;

            e.Value = existing.Id;
            e.ParsingApplied = true;
        }

        private void RoadGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView grid ||
                e.RowIndex < 0 ||
                !IsRoadComboColumn(grid, e.ColumnIndex))
            {
                return;
            }

            int? roadId = GetRoadIdFromCellValue(grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            if (!roadId.HasValue)
                return;

            EditRoadDefinition(roadId.Value, grid, e.RowIndex, e.ColumnIndex);
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

        private bool IsRoadComboColumn(DataGridView grid, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= grid.Columns.Count)
                return false;

            string columnName = grid.Columns[columnIndex].Name;
            return string.Equals(columnName, RoadColumn, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(columnName, ObjectAssignedRoadColumn, StringComparison.OrdinalIgnoreCase);
        }

        private RoadRecordChoice? FindRoadChoice(string text)
        {
            string normalized = NormalizeLookup(text);
            return _roads.FirstOrDefault(road =>
                string.Equals(NormalizeLookup(road.DisplayText), normalized, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(NormalizeLookup(road.RoadName), normalized, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(NormalizeLookup(road.RoadCode), normalized, StringComparison.OrdinalIgnoreCase));
        }

        private RoadRecordChoice? CreateOrEditRoadFromTypedName(
            string typedName,
            DataGridView grid,
            int rowIndex)
        {
            Road road = CreateDefaultRoadDefinition(typedName, grid, rowIndex);
            return ShowRoadEditorAndSave(road);
        }

        private void EditRoadDefinition(
            int roadId,
            DataGridView grid,
            int rowIndex,
            int columnIndex)
        {
            Road? road = _session.GetDbContext()
                .Roads
                .AsNoTracking()
                .FirstOrDefault(item => item.Id == roadId);
            if (road == null)
                return;

            RoadRecordChoice? saved = ShowRoadEditorAndSave(road);
            if (saved == null)
                return;

            RefreshRoadChoices();
            grid.Rows[rowIndex].Cells[columnIndex].Value = saved.Id;
            AssignmentChanged = true;
            AssignmentCommitted?.Invoke();
            _lblStatus.Text = $"Updated road data: {saved.RoadName}.";
        }

        private Road CreateDefaultRoadDefinition(
            string typedName,
            DataGridView grid,
            int rowIndex)
        {
            RoadCenterlineAssignmentCandidate? candidate = grid == _dgvObjects &&
                                                           rowIndex >= 0 &&
                                                           rowIndex < grid.Rows.Count
                ? grid.Rows[rowIndex].Tag as RoadCenterlineAssignmentCandidate
                : null;

            double width = Math.Max(1, candidate?.RightOfWayWidth ?? candidate?.RoadWidth ?? 1);

            return new Road
            {
                RoadName = typedName.Trim(),
                RoadCode = typedName.Trim(),
                RoadWidth = width,
                RightOfWayWidth = width,
                RoadType = null,
                RoadStatus = string.Empty,
                SurfaceType = "Earthen",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };
        }

        private RoadRecordChoice? ShowRoadEditorAndSave(Road road)
        {
            using frmRoadDefinitionEditor editor = new(road);
            if (editor.ShowDialog(this) != DialogResult.OK)
                return null;

            Road edited = editor.Road;
            var context = _session.GetDbContext();
            DateTime now = DateTime.Now;
            Road entity;
            if (edited.Id == 0)
            {
                entity = new Road
                {
                    CreatedDate = now
                };
                context.Roads.Add(entity);
            }
            else
            {
                entity = context.Roads.First(item => item.Id == edited.Id);
            }

            entity.RoadName = edited.RoadName;
            entity.RoadCode = edited.RoadCode;
            entity.RoadStatus = edited.RoadStatus;
            entity.SurfaceType = edited.SurfaceType;
            entity.RoadWidth = edited.RoadWidth;
            entity.RightOfWayWidth = edited.RightOfWayWidth;
            entity.RoadType = edited.RoadType;
            entity.CanvasObjectId = edited.CanvasObjectId;
            entity.Description = edited.Description;
            entity.LastModifiedDate = now;

            context.SaveChanges();
            return ToRoadChoice(entity);
        }

        private void RefreshRoadChoices()
        {
            _roads = _session.GetDbContext()
                .Roads
                .AsNoTracking()
                .OrderBy(road => road.RoadCode)
                .ThenBy(road => road.RoadName)
                .Select(road => new RoadRecordChoice(
                    road.Id,
                    road.RoadName,
                    road.RoadCode,
                    road.RoadWidth,
                    road.RightOfWayWidth,
                    road.RoadType,
                    road.SurfaceType))
                .ToList();

            PopulateRoadComboColumns();
            ApplyAssignmentMode();
        }

        private static RoadRecordChoice ToRoadChoice(Road road) =>
            new(
                road.Id,
                road.RoadName,
                road.RoadCode,
                road.RoadWidth,
                road.RightOfWayWidth,
                road.RoadType,
                road.SurfaceType);

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

        private static string NormalizeLookup(string? value) =>
            (value ?? string.Empty).Trim().ToUpperInvariant();

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
            if (bySourceLayer)
                ActivateAssignmentGrid(_dgvLayerMappings, _dgvObjects);
            else
                ActivateAssignmentGrid(_dgvObjects, _dgvLayerMappings);
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

        private void ActivateAssignmentGrid(DataGridView activeGrid, DataGridView inactiveGrid)
        {
            inactiveGrid.Visible = false;
            activeGrid.Visible = true;
            activeGrid.BringToFront();
            RefreshGridScrollbars(activeGrid);

            if (IsHandleCreated && !IsDisposed)
                BeginInvoke(new Action(() => RefreshGridScrollbars(activeGrid)));
        }

        private static void RefreshGridScrollbars(DataGridView grid)
        {
            if (grid.IsDisposed)
                return;

            int firstDisplayedRow = -1;
            try
            {
                if (grid.Rows.Count > 0)
                    firstDisplayedRow = grid.FirstDisplayedScrollingRowIndex;
            }
            catch (InvalidOperationException)
            {
                firstDisplayedRow = -1;
            }

            grid.ScrollBars = ScrollBars.None;
            grid.ScrollBars = ScrollBars.Both;
            grid.PerformLayout();
            grid.Invalidate();

            if (firstDisplayedRow < 0 || firstDisplayedRow >= grid.Rows.Count)
                return;

            try
            {
                grid.FirstDisplayedScrollingRowIndex = firstDisplayedRow;
            }
            catch (InvalidOperationException)
            {
            }
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
