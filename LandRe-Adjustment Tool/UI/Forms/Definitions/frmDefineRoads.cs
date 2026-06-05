using Land_Readjustment_Tool.Core.Entities.Layout;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Project;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.UI.Forms.Definitions
{
    public sealed partial class frmDefineRoads : Form
    {
        private readonly BindingSource _bindingSource = new();
        private List<RoadDefinitionRow> _rows = [];
        private bool _isLoading;
        private bool _isSaving;
        private readonly bool _readOnlyMode;

        public frmDefineRoads(bool readOnlyMode = false)
        {
            _readOnlyMode = readOnlyMode;
            InitializeComponent();
            ConfigureGridColumns();
            dgvRoads.DataSource = _bindingSource;
            dgvRoads.DataError += (_, _) => { };
            dgvRoads.CellEndEdit += dgvRoads_CellEndEdit;
            dgvRoads.CellDoubleClick += dgvRoads_CellDoubleClick;
            dgvRoads.CurrentCellDirtyStateChanged += dgvRoads_CurrentCellDirtyStateChanged;
            RecordFormTheme.Apply(this);
            ApplyReadOnlyMode();
        }

        private void ApplyReadOnlyMode()
        {
            if (!_readOnlyMode)
                return;

            Text = "Define Roads (Read Only)";
            dgvRoads.ReadOnly = true;
            btnAdd.Enabled = false;
            btnDuplicate.Enabled = false;
            btnDetails.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void ConfigureGridColumns()
        {
            dgvRoads.AutoGenerateColumns = false;
            dgvRoads.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvRoads.Columns.Clear();

            dgvRoads.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(RoadDefinitionRow.Code),
                HeaderText = "Road Code",
                Name = nameof(RoadDefinitionRow.Code),
                Width = 120
            });

            dgvRoads.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(RoadDefinitionRow.RowWidth),
                HeaderText = "Road Width (ROW) (m)",
                Name = nameof(RoadDefinitionRow.RowWidth),
                Width = 130
            });

            dgvRoads.Columns.Add(new DataGridViewComboBoxColumn
            {
                DataPropertyName = nameof(RoadDefinitionRow.Surface),
                HeaderText = "Surface",
                Name = nameof(RoadDefinitionRow.Surface),
                FlatStyle = FlatStyle.Flat,
                Width = 150,
                Items = { "Earthen", "Gravelled", "Blacktopped", "Concrete", "Other" }
            });
        }

        private async void frmDefineRoads_Load(object? sender, EventArgs e)
        {
            await LoadAsync();
        }

        private async void btnAdd_Click(object? sender, EventArgs e) =>
            await ExecuteRoadOperationAsync(AddRoadAsync);

        private async void btnDuplicate_Click(object? sender, EventArgs e) =>
            await ExecuteRoadOperationAsync(DuplicateRoadAsync);

        private async void btnDetails_Click(object? sender, EventArgs e) =>
            await ExecuteRoadOperationAsync(EditRoadDetailsAsync);

        private async void btnDelete_Click(object? sender, EventArgs e) =>
            await ExecuteRoadOperationAsync(DeleteRoadAsync);

        private async void btnRefresh_Click(object? sender, EventArgs e) => await LoadAsync();

        private void txtSearch_TextChanged(object? sender, EventArgs e) => ApplyFilter();

        private async Task ExecuteRoadOperationAsync(Func<Task> operation)
        {
            if (_readOnlyMode)
                return;

            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not save road definitions. {ex.Message}", "Define Roads", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void dgvRoads_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (_readOnlyMode)
                return;

            await SaveCurrentEditsAsync();
        }

        private async void dgvRoads_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_readOnlyMode || e.RowIndex < 0)
                return;

            await ExecuteRoadOperationAsync(EditRoadDetailsAsync);
        }

        private void dgvRoads_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (!_readOnlyMode &&
                dgvRoads.IsCurrentCellDirty &&
                dgvRoads.CurrentCell is DataGridViewComboBoxCell)
            {
                dgvRoads.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private async Task LoadAsync()
        {
            if (!AppServices.HasContext)
            {
                MessageBox.Show(this, "Open a project before defining roads.", "Define Roads", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                return;
            }

            _isLoading = true;
            try
            {
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                await ProjectDatabaseCompatibility.EnsureAsync(context);

                List<Road> roads = await context.Roads
                    .AsNoTracking()
                    .OrderBy(road => road.RoadCode)
                    .ThenBy(road => road.RoadName)
                    .ToListAsync();

                Dictionary<int, int> assignedCounts = await context.CanvasObjects
                    .AsNoTracking()
                    .Where(item => item.RoadId.HasValue)
                    .GroupBy(item => item.RoadId!.Value)
                    .Select(group => new { Id = group.Key, Count = group.Count() })
                    .ToDictionaryAsync(item => item.Id, item => item.Count);

                _rows = roads
                    .Select(road => RoadDefinitionRow.FromRoad(
                        road,
                        assignedCounts.GetValueOrDefault(road.Id) + (road.CanvasObjectId.HasValue ? 1 : 0)))
                    .ToList();

                ApplyFilter();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyFilter()
        {
            string search = txtSearch.Text.Trim();
            IEnumerable<RoadDefinitionRow> filtered = _rows;

            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(row => row.Matches(search));

            _bindingSource.DataSource = filtered.ToList();
            lblStatus.Text = $"{_bindingSource.Count:N0} shown   |   {_rows.Count:N0} road definition(s)";
        }

        private RoadDefinitionRow? SelectedRow()
        {
            return _bindingSource.Current as RoadDefinitionRow;
        }

        private List<RoadDefinitionRow> SelectedRows()
        {
            List<RoadDefinitionRow> rows = dgvRoads.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => row.DataBoundItem as RoadDefinitionRow)
                .Where(row => row != null)
                .Cast<RoadDefinitionRow>()
                .Distinct()
                .ToList();

            if (rows.Count == 0 && SelectedRow() is RoadDefinitionRow current)
                rows.Add(current);

            return rows;
        }

        private async Task AddRoadAsync()
        {
            RoadDefinitionRow row = new()
            {
                Code = NextRoadCode(),
                RowWidth = 1,
                Surface = "Earthen"
            };

            _rows.Add(row);
            await SaveAllAsync(reloadAfterSave: false);
            ApplyFilter();
            SelectRow(row);
            BeginEdit(nameof(RoadDefinitionRow.Code));
        }

        private async Task DuplicateRoadAsync()
        {
            RoadDefinitionRow? selected = SelectedRow();
            if (selected == null)
                return;

            RoadDefinitionRow row = selected.CopyAsNew();
            row.Code = $"{row.Code}_COPY";
            row.RoadName = $"{row.RoadName} (Copy)";
            _rows.Add(row);
            await SaveAllAsync(reloadAfterSave: false);
            ApplyFilter();
            SelectRow(row);
            BeginEdit(nameof(RoadDefinitionRow.Code));
        }

        private async Task EditRoadDetailsAsync()
        {
            RoadDefinitionRow? row = SelectedRow();
            if (row == null)
                return;

            using frmRoadDefinitionEditor editor = new(row.ToRoad());
            if (editor.ShowDialog(this) != DialogResult.OK)
                return;

            row.ApplyRoad(editor.Road);
            await SaveAllAsync(reloadAfterSave: false);
            _bindingSource.ResetBindings(false);
        }

        private async Task SaveAllAsync(bool reloadAfterSave = true)
        {
            if (_isLoading || _isSaving)
                return;

            _isSaving = true;
            try
            {
                dgvRoads.EndEdit();
                _bindingSource.EndEdit();

                string? validationError = ValidateRows();
                if (validationError != null)
                {
                    MessageBox.Show(this, validationError, "Define Roads", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppDbContext context = AppServices.Context.Session.GetDbContext();
                await ProjectDatabaseCompatibility.EnsureAsync(context);
                List<(RoadDefinitionRow Row, Road Road)> insertedRoads = [];

                foreach (RoadDefinitionRow row in _rows)
                {
                    if (row.Id == 0)
                    {
                        Road road = row.ToRoad();
                        context.Roads.Add(road);
                        insertedRoads.Add((row, road));
                        continue;
                    }

                    bool exists = await context.Roads
                        .AsNoTracking()
                        .AnyAsync(road => road.Id == row.Id);
                    if (!exists)
                    {
                        MessageBox.Show(this, $"Road '{row.Code}' was not found. Refresh definitions and try again.", "Define Roads", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DetachTrackedRoad(context, row.Id);
                    context.Roads.Update(row.ToRoad());
                }

                await context.SaveChangesAsync();
                foreach ((RoadDefinitionRow row, Road road) in insertedRoads)
                    row.ApplyRoad(road);

                AppServices.Context.MarkAsModified();
                if (reloadAfterSave)
                    await LoadAsync();
                else
                    ApplyFilter();
            }
            finally
            {
                _isSaving = false;
            }
        }

        private string? ValidateRows()
        {
            HashSet<string> codes = new(StringComparer.OrdinalIgnoreCase);
            foreach (RoadDefinitionRow row in _rows)
            {
                row.Normalize();
                if (string.IsNullOrWhiteSpace(row.Code))
                    return "Road Code is required.";

                if (row.RowWidth <= 0)
                    return $"Road Width (ROW) must be greater than zero for '{row.Code}'.";

                if (!codes.Add(row.Code))
                    return $"Road Code '{row.Code}' is duplicated.";
            }

            return null;
        }

        private async Task DeleteRoadAsync()
        {
            List<RoadDefinitionRow> selectedRows = SelectedRows();
            if (selectedRows.Count == 0)
                return;

            List<RoadDefinitionRow> savedRows = selectedRows
                .Where(row => row.Id != 0)
                .ToList();
            if (savedRows.Count > 0)
            {
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                List<int> savedIds = savedRows.Select(row => row.Id).ToList();
                HashSet<int> roadsWithFrontages = (await context.ParcelFrontages
                        .AsNoTracking()
                        .Where(item => savedIds.Contains(item.RoadId))
                        .Select(item => item.RoadId)
                        .Distinct()
                        .ToListAsync())
                    .ToHashSet();
                HashSet<int> roadsWithAssignments = (await context.CanvasObjects
                        .AsNoTracking()
                        .Where(item => item.RoadId.HasValue && savedIds.Contains(item.RoadId.Value))
                        .Select(item => item.RoadId!.Value)
                        .Distinct()
                        .ToListAsync())
                    .ToHashSet();

                List<RoadDefinitionRow> assignedRows = savedRows
                    .Where(row =>
                        row.AssignedCount > 0 ||
                        roadsWithAssignments.Contains(row.Id) ||
                        roadsWithFrontages.Contains(row.Id))
                    .ToList();
                if (assignedRows.Count > 0)
                {
                    string sample = string.Join(", ", assignedRows
                        .Take(5)
                        .Select(row => row.Code));
                    string suffix = assignedRows.Count > 5 ? ", ..." : string.Empty;
                    MessageBox.Show(
                        this,
                        $"Cannot delete {assignedRows.Count:N0} selected assigned road definition(s): {sample}{suffix}. Remove assignments before deleting.",
                        "Define Roads",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string message = selectedRows.Count == 1
                    ? $"Delete road definition '{selectedRows[0].Code}'?"
                    : $"Delete {selectedRows.Count:N0} selected road definition(s)?";
                if (MessageBox.Show(this, message, "Define Roads", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                List<Road> roads = await context.Roads
                    .Where(road => savedIds.Contains(road.Id))
                    .ToListAsync();
                foreach (Road road in roads)
                {
                    DetachTrackedRoad(context, road.Id);
                    context.Roads.Remove(road);
                }

                await context.SaveChangesAsync();
                AppServices.Context.MarkAsModified();
            }
            else if (MessageBox.Show(this, $"Delete {selectedRows.Count:N0} selected unsaved road definition(s)?", "Define Roads", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            foreach (RoadDefinitionRow row in selectedRows)
                _rows.Remove(row);

            ApplyFilter();
        }

        private async Task SaveCurrentEditsAsync()
        {
            if (_isLoading || _isSaving)
                return;

            try
            {
                await SaveAllAsync(reloadAfterSave: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not save road definitions. {ex.Message}", "Define Roads", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void DetachTrackedRoad(AppDbContext context, int roadId)
        {
            foreach (var entry in context.ChangeTracker
                         .Entries<Road>()
                         .Where(entry => entry.Entity.Id == roadId)
                         .ToList())
            {
                entry.State = EntityState.Detached;
            }
        }

        private string NextRoadCode()
        {
            int index = _rows.Count + 1;
            string code;
            do
            {
                code = $"R{index}";
                index++;
            } while (_rows.Any(row => string.Equals(row.Code, code, StringComparison.OrdinalIgnoreCase)));

            return code;
        }

        private void SelectRow(RoadDefinitionRow row)
        {
            for (int i = 0; i < _bindingSource.Count; i++)
            {
                if (ReferenceEquals(_bindingSource[i], row))
                {
                    _bindingSource.Position = i;
                    break;
                }
            }
        }

        private void BeginEdit(string columnName)
        {
            if (dgvRoads.CurrentRow == null)
                return;

            dgvRoads.CurrentCell = dgvRoads.CurrentRow.Cells[columnName];
            dgvRoads.BeginEdit(true);
        }

        private sealed class RoadDefinitionRow
        {
            public int Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public decimal RowWidth { get; set; }
            public string Surface { get; set; } = "Earthen";
            public string RoadName { get; set; } = string.Empty;
            public string? RoadType { get; set; }
            public string? Description { get; set; }
            public Guid? CanvasObjectId { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime LastModifiedDate { get; set; }
            public int AssignedCount { get; set; }

            public static RoadDefinitionRow FromRoad(Road road, int assignedCount)
            {
                return new RoadDefinitionRow
                {
                    Id = road.Id,
                    Code = string.IsNullOrWhiteSpace(road.RoadCode) ? road.RoadName : road.RoadCode!,
                    RowWidth = Convert.ToDecimal(road.RightOfWayWidth ?? road.RoadWidth),
                    Surface = NormalizeSurface(road.SurfaceType),
                    RoadName = road.RoadName,
                    RoadType = road.RoadType,
                    Description = road.Description,
                    CanvasObjectId = road.CanvasObjectId,
                    CreatedDate = road.CreatedDate,
                    LastModifiedDate = road.LastModifiedDate,
                    AssignedCount = assignedCount
                };
            }

            public RoadDefinitionRow CopyAsNew()
            {
                return new RoadDefinitionRow
                {
                    Code = Code,
                    RowWidth = RowWidth,
                    Surface = Surface,
                    RoadName = RoadName,
                    RoadType = RoadType,
                    Description = Description
                };
            }

            public bool Matches(string search)
            {
                return Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                       RoadName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                       Surface.Contains(search, StringComparison.OrdinalIgnoreCase);
            }

            public Road ToRoad()
            {
                Road road = new();
                ApplyTo(road);
                road.Id = Id;
                return road;
            }

            public void ApplyTo(Road road)
            {
                Normalize();
                double rowWidth = Convert.ToDouble(RowWidth);
                road.RoadName = string.IsNullOrWhiteSpace(RoadName) ? Code : RoadName.Trim();
                road.RoadCode = Code;
                road.RoadStatus = string.Empty;
                road.SurfaceType = Surface;
                road.RoadWidth = rowWidth;
                road.RightOfWayWidth = rowWidth;
                road.RoadType = string.IsNullOrWhiteSpace(RoadType) ? null : RoadType.Trim();
                road.CanvasObjectId = CanvasObjectId;
                road.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();

                if (road.CreatedDate == default)
                    road.CreatedDate = CreatedDate == default ? DateTime.Now : CreatedDate;

                road.LastModifiedDate = DateTime.Now;
            }

            public void ApplyRoad(Road road)
            {
                Id = road.Id;
                Code = road.RoadCode ?? road.RoadName;
                RowWidth = Convert.ToDecimal(road.RightOfWayWidth ?? road.RoadWidth);
                Surface = NormalizeSurface(road.SurfaceType);
                RoadName = road.RoadName;
                RoadType = road.RoadType;
                Description = road.Description;
                CanvasObjectId = road.CanvasObjectId;
                CreatedDate = road.CreatedDate;
                LastModifiedDate = road.LastModifiedDate;
            }

            public void Normalize()
            {
                Code = Code.Trim();
                RoadName = RoadName.Trim();
                Surface = NormalizeSurface(Surface);
                if (RowWidth < 0)
                    RowWidth = 0;
            }

            private static string NormalizeSurface(string? surface)
            {
                if (string.IsNullOrWhiteSpace(surface))
                    return "Earthen";

                return surface.Equals("Gravel", StringComparison.OrdinalIgnoreCase)
                    ? "Gravelled"
                    : surface.Trim();
            }
        }
    }
}
