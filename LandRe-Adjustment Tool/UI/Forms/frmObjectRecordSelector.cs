using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Forms
{
    public partial class frmObjectRecordSelector : Form
    {
        private const string AllMapSheetsFilterText = "All Map Sheets";
        private readonly Dictionary<ObjectRecordSelectorCategory, List<ObjectRecordSelectorItem>> _recordsByCategory;
        private readonly Dictionary<ObjectRecordSelectorCategory, HashSet<int>> _checkedRecordIds;
        private bool _suppressGridEvents;

        public IReadOnlyList<Guid> SelectedCanvasObjectIds { get; private set; } = [];
        public bool ZoomToSelection => chkZoomToSelection.Checked;

        public frmObjectRecordSelector(
            IEnumerable<ObjectRecordSelectorItem> originalParcelRecords,
            IEnumerable<Guid> selectedCanvasObjectIds)
            : this(originalParcelRecords, [], [], [], selectedCanvasObjectIds)
        {
        }

        public frmObjectRecordSelector(
            IEnumerable<ObjectRecordSelectorItem> originalParcelRecords,
            IEnumerable<ObjectRecordSelectorItem> replottedParcelRecords,
            IEnumerable<ObjectRecordSelectorItem> blockRecords,
            IEnumerable<ObjectRecordSelectorItem> roadRecords,
            IEnumerable<Guid> selectedCanvasObjectIds)
        {
            InitializeComponent();

            _recordsByCategory = new()
            {
                [ObjectRecordSelectorCategory.OriginalParcel] = SortRecords(originalParcelRecords),
                [ObjectRecordSelectorCategory.ReplottedParcel] = SortRecords(replottedParcelRecords),
                [ObjectRecordSelectorCategory.Block] = SortRecords(blockRecords),
                [ObjectRecordSelectorCategory.Road] = SortRecords(roadRecords)
            };
            _checkedRecordIds = _recordsByCategory.Keys.ToDictionary(
                category => category,
                _ => new HashSet<int>());

            HashSet<Guid> selectedObjects = selectedCanvasObjectIds.ToHashSet();
            foreach (KeyValuePair<ObjectRecordSelectorCategory, List<ObjectRecordSelectorItem>> pair in _recordsByCategory)
            {
                foreach (ObjectRecordSelectorItem item in pair.Value)
                {
                    if (item.CanvasObjectIds.Any(selectedObjects.Contains))
                    {
                        _checkedRecordIds[pair.Key].Add(item.RecordId);
                    }
                }
            }

            WireAdditionalTabs();
            PopulateMapSheetFilter();
            ApplyAllFilters();
        }

        private static List<ObjectRecordSelectorItem> SortRecords(IEnumerable<ObjectRecordSelectorItem> records)
        {
            return records
                .OrderBy(item => item.PrimaryText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.SecondaryText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.RecordId)
                .ToList();
        }

        private void WireAdditionalTabs()
        {
            txtSearchReplotted.TextChanged += (_, _) => ApplyFilter(ObjectRecordSelectorCategory.ReplottedParcel);
            txtSearchBlocks.TextChanged += (_, _) => ApplyFilter(ObjectRecordSelectorCategory.Block);
            txtSearchRoads.TextChanged += (_, _) => ApplyFilter(ObjectRecordSelectorCategory.Road);
            btnClearSearchReplotted.Click += (_, _) => ClearSearch(txtSearchReplotted, ObjectRecordSelectorCategory.ReplottedParcel);
            btnClearSearchBlocks.Click += (_, _) => ClearSearch(txtSearchBlocks, ObjectRecordSelectorCategory.Block);
            btnClearSearchRoads.Click += (_, _) => ClearSearch(txtSearchRoads, ObjectRecordSelectorCategory.Road);

            btnSelectAllReplotted.Click += (_, _) => SetVisibleRowsChecked(ObjectRecordSelectorCategory.ReplottedParcel, true);
            btnSelectAllBlocks.Click += (_, _) => SetVisibleRowsChecked(ObjectRecordSelectorCategory.Block, true);
            btnSelectAllRoads.Click += (_, _) => SetVisibleRowsChecked(ObjectRecordSelectorCategory.Road, true);
            btnSelectNoneReplotted.Click += (_, _) => ClearChecked(ObjectRecordSelectorCategory.ReplottedParcel);
            btnSelectNoneBlocks.Click += (_, _) => ClearChecked(ObjectRecordSelectorCategory.Block);
            btnSelectNoneRoads.Click += (_, _) => ClearChecked(ObjectRecordSelectorCategory.Road);
            btnDeselectReplotted.Click += (_, _) => DeselectVisible(ObjectRecordSelectorCategory.ReplottedParcel);
            btnDeselectBlocks.Click += (_, _) => DeselectVisible(ObjectRecordSelectorCategory.Block);
            btnDeselectRoads.Click += (_, _) => DeselectVisible(ObjectRecordSelectorCategory.Road);
            btnInvertReplotted.Click += (_, _) => InvertVisible(ObjectRecordSelectorCategory.ReplottedParcel);
            btnInvertBlocks.Click += (_, _) => InvertVisible(ObjectRecordSelectorCategory.Block);
            btnInvertRoads.Click += (_, _) => InvertVisible(ObjectRecordSelectorCategory.Road);

            WireGrid(dgvReplottedParcels, colReplottedSelected, ObjectRecordSelectorCategory.ReplottedParcel);
            WireGrid(dgvBlocks, colBlockSelected, ObjectRecordSelectorCategory.Block);
            WireGrid(dgvRoads, colRoadSelected, ObjectRecordSelectorCategory.Road);
            tabRecords.SelectedIndexChanged += (_, _) => UpdateStatus();
        }

        private void WireGrid(
            DataGridView grid,
            DataGridViewCheckBoxColumn selectedColumn,
            ObjectRecordSelectorCategory category)
        {
            grid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (grid.IsCurrentCellDirty)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            grid.CellContentClick += (_, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == selectedColumn.Index)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            grid.CellValueChanged += (_, e) => HandleGridCellValueChanged(grid, selectedColumn, category, e);
        }

        private void txtSearch_TextChanged(object? sender, EventArgs e) => ApplyFilter(ObjectRecordSelectorCategory.OriginalParcel);
        private void cboMapSheetFilter_SelectedIndexChanged(object? sender, EventArgs e) => ApplyFilter(ObjectRecordSelectorCategory.OriginalParcel);
        private void txtPlotNumberSearch_TextChanged(object? sender, EventArgs e) => ApplyFilter(ObjectRecordSelectorCategory.OriginalParcel);

        private void txtPlotNumberSearch_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void btnClearSearch_Click(object? sender, EventArgs e)
        {
            txtSearch.Clear();
            txtPlotNumberSearch.Clear();
            if (cboMapSheetFilter.Items.Count > 0)
                cboMapSheetFilter.SelectedIndex = 0;
            txtSearch.Focus();
        }

        private void btnSelectAll_Click(object? sender, EventArgs e) =>
            SetVisibleRowsChecked(ObjectRecordSelectorCategory.OriginalParcel, true);

        private void btnSelectNone_Click(object? sender, EventArgs e) =>
            ClearChecked(ObjectRecordSelectorCategory.OriginalParcel);

        private void btnDeselectVisible_Click(object? sender, EventArgs e) =>
            DeselectVisible(ObjectRecordSelectorCategory.OriginalParcel);

        private void btnInvertSelection_Click(object? sender, EventArgs e) =>
            InvertVisible(ObjectRecordSelectorCategory.OriginalParcel);

        private void dgvOriginalParcels_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvOriginalParcels.IsCurrentCellDirty)
                dgvOriginalParcels.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvOriginalParcels_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == colSelected.Index)
                dgvOriginalParcels.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvOriginalParcels_CellValueChanged(object? sender, DataGridViewCellEventArgs e) =>
            HandleGridCellValueChanged(dgvOriginalParcels, colSelected, ObjectRecordSelectorCategory.OriginalParcel, e);

        private void HandleGridCellValueChanged(
            DataGridView grid,
            DataGridViewCheckBoxColumn selectedColumn,
            ObjectRecordSelectorCategory category,
            DataGridViewCellEventArgs e)
        {
            if (_suppressGridEvents ||
                e.RowIndex < 0 ||
                e.ColumnIndex != selectedColumn.Index ||
                grid.Rows[e.RowIndex].Tag is not ObjectRecordSelectorItem item)
            {
                return;
            }

            bool isChecked = Convert.ToBoolean(grid.Rows[e.RowIndex].Cells[selectedColumn.Index].Value);
            if (!item.CanSelect)
            {
                grid.Rows[e.RowIndex].Cells[selectedColumn.Index].Value = false;
                _checkedRecordIds[category].Remove(item.RecordId);
                return;
            }

            if (isChecked)
                _checkedRecordIds[category].Add(item.RecordId);
            else
                _checkedRecordIds[category].Remove(item.RecordId);

            UpdateStatus();
        }

        private void btnApply_Click(object? sender, EventArgs e)
        {
            SelectedCanvasObjectIds = _recordsByCategory
                .SelectMany(pair => pair.Value.Where(item =>
                    item.CanSelect &&
                    _checkedRecordIds[pair.Key].Contains(item.RecordId)))
                .SelectMany(item => item.CanvasObjectIds)
                .Distinct()
                .ToList();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ApplyAllFilters()
        {
            foreach (ObjectRecordSelectorCategory category in _recordsByCategory.Keys)
                ApplyFilter(category);
        }

        private void ApplyFilter(ObjectRecordSelectorCategory category)
        {
            IEnumerable<ObjectRecordSelectorItem> filtered = _recordsByCategory[category];
            string searchText = GetSearchText(category);
            if (!string.IsNullOrWhiteSpace(searchText))
                filtered = filtered.Where(item => item.Matches(searchText));

            if (category == ObjectRecordSelectorCategory.OriginalParcel)
            {
                string mapSheetFilter = cboMapSheetFilter.SelectedItem?.ToString() ?? AllMapSheetsFilterText;
                string plotNumberSearch = txtPlotNumberSearch.Text.Trim();
                if (!string.Equals(mapSheetFilter, AllMapSheetsFilterText, StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(item =>
                        string.Equals(item.MapSheetNo, mapSheetFilter, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(plotNumberSearch))
                    filtered = filtered.Where(item => item.MatchesPlotNumber(plotNumberSearch));
            }

            PopulateGrid(category, filtered.ToList());
            UpdateStatus();
        }

        private void PopulateGrid(ObjectRecordSelectorCategory category, List<ObjectRecordSelectorItem> filteredItems)
        {
            DataGridView grid = GetGrid(category);
            DataGridViewCheckBoxColumn selectedColumn = GetSelectedColumn(category);

            _suppressGridEvents = true;
            grid.SuspendLayout();
            try
            {
                grid.Rows.Clear();
                foreach (ObjectRecordSelectorItem item in filteredItems)
                {
                    int rowIndex = category switch
                    {
                        ObjectRecordSelectorCategory.OriginalParcel => grid.Rows.Add(
                            IsChecked(category, item),
                            item.ParcelNo,
                            item.MapSheetNo,
                            item.OwnerName,
                            item.DisplayArea,
                            item.LayerName,
                            item.Status),
                        ObjectRecordSelectorCategory.ReplottedParcel => grid.Rows.Add(
                            IsChecked(category, item),
                            item.PrimaryText,
                            item.SecondaryText,
                            item.TertiaryText,
                            item.DisplayArea,
                            item.LayerName,
                            item.Status),
                        ObjectRecordSelectorCategory.Block => grid.Rows.Add(
                            IsChecked(category, item),
                            item.PrimaryText,
                            item.SecondaryText,
                            item.TertiaryText,
                            item.DisplayArea,
                            item.LayerName,
                            item.Status),
                        ObjectRecordSelectorCategory.Road => grid.Rows.Add(
                            IsChecked(category, item),
                            item.PrimaryText,
                            item.SecondaryText,
                            item.TertiaryText,
                            item.DisplayArea,
                            item.LayerName,
                            item.Status),
                        _ => -1
                    };

                    if (rowIndex < 0)
                        continue;

                    DataGridViewRow row = grid.Rows[rowIndex];
                    row.Tag = item;
                    row.Cells[selectedColumn.Index].ReadOnly = !item.CanSelect;
                    if (!item.CanSelect)
                    {
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(135, 145, 158);
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
                    }
                }

                grid.ClearSelection();
            }
            finally
            {
                grid.ResumeLayout();
                _suppressGridEvents = false;
            }
        }

        private bool IsChecked(ObjectRecordSelectorCategory category, ObjectRecordSelectorItem item) =>
            item.CanSelect && _checkedRecordIds[category].Contains(item.RecordId);

        private void PopulateMapSheetFilter()
        {
            cboMapSheetFilter.BeginUpdate();
            try
            {
                cboMapSheetFilter.Items.Clear();
                cboMapSheetFilter.Items.Add(AllMapSheetsFilterText);

                foreach (string mapSheetNo in _recordsByCategory[ObjectRecordSelectorCategory.OriginalParcel]
                             .Select(item => item.MapSheetNo)
                             .Where(value => !string.IsNullOrWhiteSpace(value))
                             .Distinct(StringComparer.OrdinalIgnoreCase)
                             .OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
                {
                    cboMapSheetFilter.Items.Add(mapSheetNo);
                }

                cboMapSheetFilter.SelectedIndex = 0;
            }
            finally
            {
                cboMapSheetFilter.EndUpdate();
            }
        }

        private IEnumerable<ObjectRecordSelectorItem> GetVisibleSelectableItems(ObjectRecordSelectorCategory category)
        {
            return GetGrid(category).Rows
                .Cast<DataGridViewRow>()
                .Select(row => row.Tag as ObjectRecordSelectorItem)
                .Where(item => item?.CanSelect == true)
                .Cast<ObjectRecordSelectorItem>();
        }

        private void SetVisibleRowsChecked(ObjectRecordSelectorCategory category, bool isChecked)
        {
            foreach (ObjectRecordSelectorItem item in GetVisibleSelectableItems(category))
            {
                if (isChecked)
                    _checkedRecordIds[category].Add(item.RecordId);
                else
                    _checkedRecordIds[category].Remove(item.RecordId);
            }

            ApplyFilter(category);
        }

        private void ClearChecked(ObjectRecordSelectorCategory category)
        {
            _checkedRecordIds[category].Clear();
            ApplyFilter(category);
        }

        private void DeselectVisible(ObjectRecordSelectorCategory category)
        {
            foreach (ObjectRecordSelectorItem item in GetVisibleSelectableItems(category))
                _checkedRecordIds[category].Remove(item.RecordId);
            ApplyFilter(category);
        }

        private void InvertVisible(ObjectRecordSelectorCategory category)
        {
            foreach (ObjectRecordSelectorItem item in GetVisibleSelectableItems(category))
            {
                if (!_checkedRecordIds[category].Add(item.RecordId))
                    _checkedRecordIds[category].Remove(item.RecordId);
            }

            ApplyFilter(category);
        }

        private void ClearSearch(TextBox textBox, ObjectRecordSelectorCategory category)
        {
            textBox.Clear();
            textBox.Focus();
            ApplyFilter(category);
        }

        private string GetSearchText(ObjectRecordSelectorCategory category)
        {
            return category switch
            {
                ObjectRecordSelectorCategory.OriginalParcel => txtSearch.Text.Trim(),
                ObjectRecordSelectorCategory.ReplottedParcel => txtSearchReplotted.Text.Trim(),
                ObjectRecordSelectorCategory.Block => txtSearchBlocks.Text.Trim(),
                ObjectRecordSelectorCategory.Road => txtSearchRoads.Text.Trim(),
                _ => string.Empty
            };
        }

        private DataGridView GetGrid(ObjectRecordSelectorCategory category)
        {
            return category switch
            {
                ObjectRecordSelectorCategory.OriginalParcel => dgvOriginalParcels,
                ObjectRecordSelectorCategory.ReplottedParcel => dgvReplottedParcels,
                ObjectRecordSelectorCategory.Block => dgvBlocks,
                ObjectRecordSelectorCategory.Road => dgvRoads,
                _ => dgvOriginalParcels
            };
        }

        private DataGridViewCheckBoxColumn GetSelectedColumn(ObjectRecordSelectorCategory category)
        {
            return category switch
            {
                ObjectRecordSelectorCategory.OriginalParcel => colSelected,
                ObjectRecordSelectorCategory.ReplottedParcel => colReplottedSelected,
                ObjectRecordSelectorCategory.Block => colBlockSelected,
                ObjectRecordSelectorCategory.Road => colRoadSelected,
                _ => colSelected
            };
        }

        private void UpdateStatus()
        {
            int total = _recordsByCategory.Values.Sum(records => records.Count);
            int selectable = _recordsByCategory.Values.Sum(records => records.Count(item => item.CanSelect));
            int selected = _recordsByCategory.Sum(pair =>
                pair.Value.Count(item => item.CanSelect && _checkedRecordIds[pair.Key].Contains(item.RecordId)));
            int visible = GetGrid(GetActiveCategory()).Rows.Count;

            lblStatus.Text = $"{selected:N0} selected | {visible:N0} shown in current tab | {selectable:N0} linked of {total:N0} records";
            btnApply.Text = selected == 0 ? "Clear" : "Select";
        }

        private ObjectRecordSelectorCategory GetActiveCategory()
        {
            if (tabRecords.SelectedTab == tabReplottedParcels)
                return ObjectRecordSelectorCategory.ReplottedParcel;
            if (tabRecords.SelectedTab == tabBlocks)
                return ObjectRecordSelectorCategory.Block;
            if (tabRecords.SelectedTab == tabRoads)
                return ObjectRecordSelectorCategory.Road;
            return ObjectRecordSelectorCategory.OriginalParcel;
        }
    }

    public enum ObjectRecordSelectorCategory
    {
        OriginalParcel,
        ReplottedParcel,
        Block,
        Road
    }

    public sealed class ObjectRecordSelectorItem
    {
        public ObjectRecordSelectorItem(
            int recordId,
            string mapSheetNo,
            string parcelNo,
            string uniqueCode,
            string ownerName,
            double areaSqm,
            Guid? canvasObjectId,
            string? layerName,
            int sqmPrecision = 3)
            : this(
                ObjectRecordSelectorCategory.OriginalParcel,
                recordId,
                parcelNo,
                mapSheetNo,
                ownerName,
                uniqueCode,
                areaSqm,
                canvasObjectId,
                layerName,
                sqmPrecision)
        {
        }

        public ObjectRecordSelectorItem(
            int recordId,
            string mapSheetNo,
            string parcelNo,
            string uniqueCode,
            string ownerName,
            double areaSqm,
            IEnumerable<Guid> canvasObjectIds,
            string? layerName,
            int sqmPrecision = 3)
            : this(
                ObjectRecordSelectorCategory.OriginalParcel,
                recordId,
                parcelNo,
                mapSheetNo,
                ownerName,
                uniqueCode,
                areaSqm,
                canvasObjectIds,
                layerName,
                sqmPrecision)
        {
        }

        public ObjectRecordSelectorItem(
            ObjectRecordSelectorCategory category,
            int recordId,
            string primaryText,
            string secondaryText,
            string tertiaryText,
            string uniqueCode,
            double areaSqm,
            Guid? canvasObjectId,
            string? layerName,
            int sqmPrecision = 3)
            : this(
                category,
                recordId,
                primaryText,
                secondaryText,
                tertiaryText,
                uniqueCode,
                areaSqm,
                canvasObjectId.HasValue ? [canvasObjectId.Value] : [],
                layerName,
                sqmPrecision)
        {
        }

        public ObjectRecordSelectorItem(
            ObjectRecordSelectorCategory category,
            int recordId,
            string primaryText,
            string secondaryText,
            string tertiaryText,
            string uniqueCode,
            double areaSqm,
            IEnumerable<Guid> canvasObjectIds,
            string? layerName,
            int sqmPrecision = 3)
        {
            Category = category;
            RecordId = recordId;
            PrimaryText = string.IsNullOrWhiteSpace(primaryText) ? "--" : primaryText;
            SecondaryText = string.IsNullOrWhiteSpace(secondaryText) ? "--" : secondaryText;
            TertiaryText = string.IsNullOrWhiteSpace(tertiaryText) ? "--" : tertiaryText;
            UniqueCode = uniqueCode ?? string.Empty;
            AreaSqm = areaSqm;
            CanvasObjectIds = canvasObjectIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();
            LayerName = string.IsNullOrWhiteSpace(layerName) ? "--" : layerName;
            SqmPrecision = sqmPrecision;
        }

        public ObjectRecordSelectorCategory Category { get; }
        public int RecordId { get; }
        public string PrimaryText { get; }
        public string SecondaryText { get; }
        public string TertiaryText { get; }
        public string MapSheetNo => SecondaryText;
        public string ParcelNo => PrimaryText;
        public string OwnerName => TertiaryText;
        public string UniqueCode { get; }
        public double AreaSqm { get; }
        public int SqmPrecision { get; }
        public IReadOnlyList<Guid> CanvasObjectIds { get; }
        public Guid? CanvasObjectId => CanvasObjectIds.Count > 0 ? CanvasObjectIds[0] : null;
        public string LayerName { get; }
        public bool CanSelect => CanvasObjectIds.Count > 0;
        public string Status => CanSelect ? "Mapped" : "No map object";

        public string DisplayArea => AreaSqm > 0
            ? $"{AreaSqm.ToString($"F{SqmPrecision}", CultureInfo.InvariantCulture)} sq.m"
            : "--";

        public bool Matches(string searchText)
        {
            return Contains(PrimaryText, searchText) ||
                   Contains(SecondaryText, searchText) ||
                   Contains(TertiaryText, searchText) ||
                   Contains(UniqueCode, searchText) ||
                   Contains(LayerName, searchText) ||
                   Contains(Status, searchText);
        }

        public bool MatchesPlotNumber(string plotNumber) => Contains(ParcelNo, plotNumber);

        private static bool Contains(string? value, string searchText)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}
