using Land_Readjustment_Tool.Core.Entities.Layout;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Project;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.UI.Forms.Definitions
{
    public sealed partial class frmDefineBlocks : Form
    {
        private readonly BindingSource _bindingSource = new();
        private List<BlockDefinitionRow> _rows = [];
        private bool _isLoading;
        private bool _isSaving;

        public frmDefineBlocks()
        {
            InitializeComponent();
            ConfigureGridColumns();
            dgvBlocks.DataSource = _bindingSource;
            dgvBlocks.DataError += (_, _) => { };
            dgvBlocks.CellEndEdit += dgvBlocks_CellEndEdit;
            dgvBlocks.CellDoubleClick += dgvBlocks_CellDoubleClick;
            dgvBlocks.CurrentCellDirtyStateChanged += dgvBlocks_CurrentCellDirtyStateChanged;
            RecordFormTheme.Apply(this);
        }

        private void ConfigureGridColumns()
        {
            dgvBlocks.AutoGenerateColumns = false;
            dgvBlocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvBlocks.Columns.Clear();

            dgvBlocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BlockDefinitionRow.Code),
                HeaderText = "Block Code",
                Name = nameof(BlockDefinitionRow.Code),
                Width = 120
            });

            dgvBlocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BlockDefinitionRow.Name),
                HeaderText = "Block Name",
                Name = nameof(BlockDefinitionRow.Name),
                Width = 220
            });

            dgvBlocks.Columns.Add(new DataGridViewComboBoxColumn
            {
                DataPropertyName = nameof(BlockDefinitionRow.Type),
                HeaderText = "Type / Land Use",
                Name = nameof(BlockDefinitionRow.Type),
                FlatStyle = FlatStyle.Flat,
                Width = 170,
                Items =
                {
                    "Residential",
                    "Commercial",
                    "Mixed Use",
                    "Open Space",
                    "Institutional",
                    "Utility",
                    "Other"
                }
            });

            dgvBlocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BlockDefinitionRow.Depth),
                HeaderText = "Depth (m)",
                Name = nameof(BlockDefinitionRow.Depth),
                Width = 110
            });

            dgvBlocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BlockDefinitionRow.BlockLength),
                HeaderText = "Length (m)",
                Name = nameof(BlockDefinitionRow.BlockLength),
                Width = 110
            });
        }

        private async void frmDefineBlocks_Load(object? sender, EventArgs e)
        {
            await LoadAsync();
        }

        private async void btnAdd_Click(object? sender, EventArgs e) =>
            await ExecuteBlockOperationAsync(AddBlockAsync);

        private async void btnDuplicate_Click(object? sender, EventArgs e) =>
            await ExecuteBlockOperationAsync(DuplicateBlockAsync);

        private async void btnDetails_Click(object? sender, EventArgs e) =>
            await ExecuteBlockOperationAsync(EditBlockDetailsAsync);

        private async void btnDelete_Click(object? sender, EventArgs e) =>
            await ExecuteBlockOperationAsync(DeleteBlockAsync);

        private async void btnRefresh_Click(object? sender, EventArgs e) => await LoadAsync();

        private void txtSearch_TextChanged(object? sender, EventArgs e) => ApplyFilter();

        private async Task ExecuteBlockOperationAsync(Func<Task> operation)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not save block definitions. {ex.Message}", "Define Blocks", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void dgvBlocks_CellEndEdit(object? sender, DataGridViewCellEventArgs e) => await SaveCurrentEditsAsync();

        private async void dgvBlocks_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            await ExecuteBlockOperationAsync(EditBlockDetailsAsync);
        }

        private void dgvBlocks_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvBlocks.IsCurrentCellDirty &&
                dgvBlocks.CurrentCell is DataGridViewComboBoxCell)
            {
                dgvBlocks.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private async Task LoadAsync()
        {
            if (!AppServices.HasContext)
            {
                MessageBox.Show(this, "Open a project before defining blocks.", "Define Blocks", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                return;
            }

            _isLoading = true;
            try
            {
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                await ProjectDatabaseCompatibility.EnsureAsync(context);
                List<Block> blocks = await context.Blocks
                    .AsNoTracking()
                    .OrderBy(block => block.BlockCode)
                    .ThenBy(block => block.BlockName)
                    .ToListAsync();

                Dictionary<int, int> assignedCounts = await context.CanvasObjects
                    .AsNoTracking()
                    .Where(item => item.BlockId.HasValue)
                    .GroupBy(item => item.BlockId!.Value)
                    .Select(group => new { Id = group.Key, Count = group.Count() })
                    .ToDictionaryAsync(item => item.Id, item => item.Count);

                _rows = blocks
                    .Select(block => BlockDefinitionRow.FromBlock(
                        block,
                        assignedCounts.GetValueOrDefault(block.Id) + (block.CanvasObjectId.HasValue ? 1 : 0)))
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
            IEnumerable<BlockDefinitionRow> filtered = _rows;

            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(row => row.Matches(search));

            _bindingSource.DataSource = filtered.ToList();
            lblStatus.Text = $"{_bindingSource.Count:N0} shown   |   {_rows.Count:N0} block definition(s)";
        }

        private BlockDefinitionRow? SelectedRow()
        {
            return _bindingSource.Current as BlockDefinitionRow;
        }

        private List<BlockDefinitionRow> SelectedRows()
        {
            List<BlockDefinitionRow> rows = dgvBlocks.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => row.DataBoundItem as BlockDefinitionRow)
                .Where(row => row != null)
                .Cast<BlockDefinitionRow>()
                .Distinct()
                .ToList();

            if (rows.Count == 0 && SelectedRow() is BlockDefinitionRow current)
                rows.Add(current);

            return rows;
        }

        private async Task AddBlockAsync()
        {
            string code = NextBlockCode();
            BlockDefinitionRow row = new()
            {
                Code = code,
                Name = code,
                Type = "Residential",
                Depth = 1
            };

            _rows.Add(row);
            await SaveAllAsync(reloadAfterSave: false);
            ApplyFilter();
            SelectRow(row);
            BeginEdit(nameof(BlockDefinitionRow.Name));
        }

        private async Task DuplicateBlockAsync()
        {
            BlockDefinitionRow? selected = SelectedRow();
            if (selected == null)
                return;

            BlockDefinitionRow row = selected.CopyAsNew();
            row.Code = $"{row.Code}_COPY";
            row.Name = $"{row.Name} (Copy)";
            _rows.Add(row);
            await SaveAllAsync(reloadAfterSave: false);
            ApplyFilter();
            SelectRow(row);
            BeginEdit(nameof(BlockDefinitionRow.Code));
        }

        private async Task EditBlockDetailsAsync()
        {
            BlockDefinitionRow? row = SelectedRow();
            if (row == null)
                return;

            using frmBlockDefinitionEditor editor = new(row.ToBlock());
            if (editor.ShowDialog(this) != DialogResult.OK)
                return;

            row.ApplyBlock(editor.Block);
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
                dgvBlocks.EndEdit();
                _bindingSource.EndEdit();

                string? validationError = ValidateRows();
                if (validationError != null)
                {
                    MessageBox.Show(this, validationError, "Define Blocks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppDbContext context = AppServices.Context.Session.GetDbContext();
                await ProjectDatabaseCompatibility.EnsureAsync(context);
                List<(BlockDefinitionRow Row, Block Block)> insertedBlocks = [];

                foreach (BlockDefinitionRow row in _rows)
                {
                    if (row.Id == 0)
                    {
                        Block block = row.ToBlock();
                        context.Blocks.Add(block);
                        insertedBlocks.Add((row, block));
                        continue;
                    }

                    bool exists = await context.Blocks
                        .AsNoTracking()
                        .AnyAsync(block => block.Id == row.Id);
                    if (!exists)
                    {
                        MessageBox.Show(this, $"Block '{row.Code}' was not found. Refresh definitions and try again.", "Define Blocks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DetachTrackedBlock(context, row.Id);
                    context.Blocks.Update(row.ToBlock());
                }

                await context.SaveChangesAsync();
                foreach ((BlockDefinitionRow row, Block block) in insertedBlocks)
                    row.ApplyBlock(block);

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
            foreach (BlockDefinitionRow row in _rows)
            {
                row.Normalize();
                if (string.IsNullOrWhiteSpace(row.Code))
                    return "Block Code is required.";

                if (string.IsNullOrWhiteSpace(row.Name))
                    return "Block Name is required.";

                if (string.IsNullOrWhiteSpace(row.Type))
                    return $"Block Type is required for '{row.Name}'.";

                if (row.Depth <= 0)
                    return $"Block Depth must be greater than zero for '{row.Name}'.";

                if (!codes.Add(row.Code))
                    return $"Block Code '{row.Code}' is duplicated.";
            }

            return null;
        }

        private async Task DeleteBlockAsync()
        {
            List<BlockDefinitionRow> selectedRows = SelectedRows();
            if (selectedRows.Count == 0)
                return;

            List<BlockDefinitionRow> savedRows = selectedRows
                .Where(row => row.Id != 0)
                .ToList();
            if (savedRows.Count > 0)
            {
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                List<int> savedIds = savedRows.Select(row => row.Id).ToList();
                HashSet<int> blocksWithReplottedParcels = (await context.ReplottedParcels
                        .AsNoTracking()
                        .Where(item => savedIds.Contains(item.BlockId))
                        .Select(item => item.BlockId)
                        .Distinct()
                        .ToListAsync())
                    .ToHashSet();
                HashSet<int> blocksWithAssignments = (await context.CanvasObjects
                        .AsNoTracking()
                        .Where(item => item.BlockId.HasValue && savedIds.Contains(item.BlockId.Value))
                        .Select(item => item.BlockId!.Value)
                        .Distinct()
                        .ToListAsync())
                    .ToHashSet();

                List<BlockDefinitionRow> assignedRows = savedRows
                    .Where(row =>
                        row.AssignedCount > 0 ||
                        blocksWithAssignments.Contains(row.Id) ||
                        blocksWithReplottedParcels.Contains(row.Id))
                    .ToList();
                if (assignedRows.Count > 0)
                {
                    string sample = string.Join(", ", assignedRows
                        .Take(5)
                        .Select(row => string.IsNullOrWhiteSpace(row.Code) ? row.Name : row.Code));
                    string suffix = assignedRows.Count > 5 ? ", ..." : string.Empty;
                    MessageBox.Show(
                        this,
                        $"Cannot delete {assignedRows.Count:N0} selected assigned block definition(s): {sample}{suffix}. Remove assignments before deleting.",
                        "Define Blocks",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string message = selectedRows.Count == 1
                    ? $"Delete block definition '{selectedRows[0].Name}'?"
                    : $"Delete {selectedRows.Count:N0} selected block definition(s)?";
                if (MessageBox.Show(this, message, "Define Blocks", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                List<Block> blocks = await context.Blocks
                    .Where(block => savedIds.Contains(block.Id))
                    .ToListAsync();
                foreach (Block block in blocks)
                {
                    DetachTrackedBlock(context, block.Id);
                    context.Blocks.Remove(block);
                }

                await context.SaveChangesAsync();
                AppServices.Context.MarkAsModified();
            }
            else if (MessageBox.Show(this, $"Delete {selectedRows.Count:N0} selected unsaved block definition(s)?", "Define Blocks", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            foreach (BlockDefinitionRow row in selectedRows)
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
                MessageBox.Show(this, $"Could not save block definitions. {ex.Message}", "Define Blocks", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void DetachTrackedBlock(AppDbContext context, int blockId)
        {
            foreach (var entry in context.ChangeTracker
                         .Entries<Block>()
                         .Where(entry => entry.Entity.Id == blockId)
                         .ToList())
            {
                entry.State = EntityState.Detached;
            }
        }

        private string NextBlockCode()
        {
            int index = _rows.Count + 1;
            string code;
            do
            {
                code = $"B-{index:00}";
                index++;
            } while (_rows.Any(row => string.Equals(row.Code, code, StringComparison.OrdinalIgnoreCase)));

            return code;
        }

        private void SelectRow(BlockDefinitionRow row)
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
            if (dgvBlocks.CurrentRow == null)
                return;

            dgvBlocks.CurrentCell = dgvBlocks.CurrentRow.Cells[columnName];
            dgvBlocks.BeginEdit(true);
        }

        private sealed class BlockDefinitionRow
        {
            public int Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = "Residential";
            public decimal Depth { get; set; }
            public decimal BlockLength { get; set; }
            public double BlockArea { get; set; }
            public string? Description { get; set; }
            public Guid? CanvasObjectId { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime LastModifiedDate { get; set; }
            public int AssignedCount { get; set; }

            public static BlockDefinitionRow FromBlock(Block block, int assignedCount)
            {
                return new BlockDefinitionRow
                {
                    Id = block.Id,
                    Code = block.BlockCode ?? string.Empty,
                    Name = block.BlockName,
                    Type = string.IsNullOrWhiteSpace(block.BlockLandUse) ? "Residential" : block.BlockLandUse,
                    Depth = Convert.ToDecimal(block.BlockDepth),
                    BlockLength = Convert.ToDecimal(block.BlockLength),
                    BlockArea = block.BlockArea,
                    Description = block.Description,
                    CanvasObjectId = block.CanvasObjectId,
                    CreatedDate = block.CreatedDate,
                    LastModifiedDate = block.LastModifiedDate,
                    AssignedCount = assignedCount
                };
            }

            public BlockDefinitionRow CopyAsNew()
            {
                return new BlockDefinitionRow
                {
                    Code = Code,
                    Name = Name,
                    Type = Type,
                    Depth = Depth,
                    BlockLength = BlockLength,
                    Description = Description
                };
            }

            public bool Matches(string search)
            {
                return Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                       Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                       Type.Contains(search, StringComparison.OrdinalIgnoreCase);
            }

            public Block ToBlock()
            {
                Block block = new();
                ApplyTo(block);
                block.Id = Id;
                return block;
            }

            public void ApplyTo(Block block)
            {
                Normalize();
                block.BlockName = Name;
                block.BlockCode = string.IsNullOrWhiteSpace(Code) ? Name : Code;
                block.BlockDepth = Convert.ToSingle(Depth);
                block.BlockLength = Convert.ToSingle(BlockLength);
                block.BlockLandUse = Type;
                block.BlockArea = BlockArea;
                block.CanvasObjectId = CanvasObjectId;
                block.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();

                if (block.CreatedDate == default)
                    block.CreatedDate = CreatedDate == default ? DateTime.Now : CreatedDate;

                block.LastModifiedDate = DateTime.Now;
            }

            public void ApplyBlock(Block block)
            {
                Id = block.Id;
                Code = block.BlockCode ?? string.Empty;
                Name = block.BlockName;
                Type = string.IsNullOrWhiteSpace(block.BlockLandUse) ? "Residential" : block.BlockLandUse;
                Depth = Convert.ToDecimal(block.BlockDepth);
                BlockLength = Convert.ToDecimal(block.BlockLength);
                BlockArea = block.BlockArea;
                Description = block.Description;
                CanvasObjectId = block.CanvasObjectId;
                CreatedDate = block.CreatedDate;
                LastModifiedDate = block.LastModifiedDate;
            }

            public void Normalize()
            {
                Code = Code.Trim();
                Name = Name.Trim();
                Type = string.IsNullOrWhiteSpace(Type) ? "Residential" : Type.Trim();
                if (Depth < 0) Depth = 0;
                if (BlockLength < 0) BlockLength = 0;
            }
        }
    }
}
