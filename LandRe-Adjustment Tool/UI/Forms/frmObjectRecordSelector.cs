using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Forms
{
    public partial class frmObjectRecordSelector : Form
    {
        private readonly List<ObjectRecordSelectorItem> _originalParcelRecords;
        private readonly HashSet<int> _checkedOriginalParcelIds = new();
        private bool _suppressGridEvents;

        public IReadOnlyList<Guid> SelectedCanvasObjectIds { get; private set; } = [];

        public bool ZoomToSelection => chkZoomToSelection.Checked;

        public frmObjectRecordSelector(
            IEnumerable<ObjectRecordSelectorItem> originalParcelRecords,
            IEnumerable<Guid> selectedCanvasObjectIds)
        {
            InitializeComponent();

            HashSet<Guid> selectedObjects = selectedCanvasObjectIds.ToHashSet();
            _originalParcelRecords = originalParcelRecords
                .OrderBy(item => item.MapSheetNo, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.ParcelNo, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (ObjectRecordSelectorItem item in _originalParcelRecords)
            {
                if (item.CanvasObjectId.HasValue &&
                    selectedObjects.Contains(item.CanvasObjectId.Value))
                {
                    _checkedOriginalParcelIds.Add(item.RecordId);
                }
            }

            AddComingSoonContent(tabReplottedParcels, "Replotted parcel record selection will be added here.");
            AddComingSoonContent(tabBlocks, "Block record selection will be added here.");
            AddComingSoonContent(tabRoads, "Road record selection will be added here.");
            ApplyFilter();
        }

        private static void AddComingSoonContent(TabPage tabPage, string text)
        {
            Label label = new()
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                ForeColor = System.Drawing.Color.FromArgb(89, 99, 110),
                Text = text
            };

            tabPage.Controls.Add(label);
        }

        private void txtSearch_TextChanged(object? sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void btnClearSearch_Click(object? sender, EventArgs e)
        {
            txtSearch.Clear();
            txtSearch.Focus();
        }

        private void btnSelectAll_Click(object? sender, EventArgs e)
        {
            SetVisibleRowsChecked(true);
        }

        private void btnSelectNone_Click(object? sender, EventArgs e)
        {
            _checkedOriginalParcelIds.Clear();
            ApplyFilter();
        }

        private void btnDeselectVisible_Click(object? sender, EventArgs e)
        {
            foreach (ObjectRecordSelectorItem item in GetVisibleSelectableItems())
                _checkedOriginalParcelIds.Remove(item.RecordId);

            ApplyFilter();
        }

        private void btnInvertSelection_Click(object? sender, EventArgs e)
        {
            foreach (ObjectRecordSelectorItem item in GetVisibleSelectableItems())
            {
                if (!_checkedOriginalParcelIds.Add(item.RecordId))
                    _checkedOriginalParcelIds.Remove(item.RecordId);
            }

            ApplyFilter();
        }

        private void dgvOriginalParcels_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvOriginalParcels.IsCurrentCellDirty)
                dgvOriginalParcels.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvOriginalParcels_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colSelected.Index)
                return;

            dgvOriginalParcels.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvOriginalParcels_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_suppressGridEvents ||
                e.RowIndex < 0 ||
                e.ColumnIndex != colSelected.Index ||
                dgvOriginalParcels.Rows[e.RowIndex].Tag is not ObjectRecordSelectorItem item)
            {
                return;
            }

            bool isChecked = Convert.ToBoolean(dgvOriginalParcels.Rows[e.RowIndex].Cells[colSelected.Index].Value);
            if (!item.CanSelect)
            {
                dgvOriginalParcels.Rows[e.RowIndex].Cells[colSelected.Index].Value = false;
                _checkedOriginalParcelIds.Remove(item.RecordId);
                return;
            }

            if (isChecked)
                _checkedOriginalParcelIds.Add(item.RecordId);
            else
                _checkedOriginalParcelIds.Remove(item.RecordId);

            UpdateStatus();
        }

        private void btnApply_Click(object? sender, EventArgs e)
        {
            SelectedCanvasObjectIds = _originalParcelRecords
                .Where(item => item.CanvasObjectId.HasValue && _checkedOriginalParcelIds.Contains(item.RecordId))
                .Select(item => item.CanvasObjectId!.Value)
                .Distinct()
                .ToList();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ApplyFilter()
        {
            string searchText = txtSearch.Text.Trim();
            List<ObjectRecordSelectorItem> filtered = string.IsNullOrWhiteSpace(searchText)
                ? _originalParcelRecords
                : _originalParcelRecords
                    .Where(item => item.Matches(searchText))
                    .ToList();

            _suppressGridEvents = true;
            dgvOriginalParcels.SuspendLayout();
            try
            {
                dgvOriginalParcels.Rows.Clear();
                foreach (ObjectRecordSelectorItem item in filtered)
                {
                    int rowIndex = dgvOriginalParcels.Rows.Add(
                        item.CanSelect && _checkedOriginalParcelIds.Contains(item.RecordId),
                        item.ParcelNo,
                        item.MapSheetNo,
                        item.OwnerName,
                        item.DisplayArea,
                        item.LayerName,
                        item.Status);

                    DataGridViewRow row = dgvOriginalParcels.Rows[rowIndex];
                    row.Tag = item;
                    row.Cells[colSelected.Index].ReadOnly = !item.CanSelect;
                    if (!item.CanSelect)
                    {
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(135, 145, 158);
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
                        row.Cells[colStatus.Index].Style.ForeColor = System.Drawing.Color.FromArgb(176, 71, 61);
                    }
                }

                dgvOriginalParcels.ClearSelection();
            }
            finally
            {
                dgvOriginalParcels.ResumeLayout();
                _suppressGridEvents = false;
            }

            UpdateStatus();
        }

        private IEnumerable<ObjectRecordSelectorItem> GetVisibleSelectableItems()
        {
            return dgvOriginalParcels.Rows
                .Cast<DataGridViewRow>()
                .Select(row => row.Tag as ObjectRecordSelectorItem)
                .Where(item => item?.CanSelect == true)
                .Cast<ObjectRecordSelectorItem>();
        }

        private void SetVisibleRowsChecked(bool isChecked)
        {
            foreach (ObjectRecordSelectorItem item in GetVisibleSelectableItems())
            {
                if (isChecked)
                    _checkedOriginalParcelIds.Add(item.RecordId);
                else
                    _checkedOriginalParcelIds.Remove(item.RecordId);
            }

            ApplyFilter();
        }

        private void UpdateStatus()
        {
            int total = _originalParcelRecords.Count;
            int visible = dgvOriginalParcels.Rows.Count;
            int selectable = _originalParcelRecords.Count(item => item.CanSelect);
            int selected = _originalParcelRecords.Count(item =>
                item.CanSelect && _checkedOriginalParcelIds.Contains(item.RecordId));

            lblStatus.Text = $"{selected:N0} selected | {visible:N0} shown | {selectable:N0} linked of {total:N0} records";
            btnApply.Text = selected == 0 ? "Clear" : "Select";
        }
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
            string? layerName)
        {
            RecordId = recordId;
            MapSheetNo = mapSheetNo;
            ParcelNo = parcelNo;
            UniqueCode = uniqueCode;
            OwnerName = string.IsNullOrWhiteSpace(ownerName) ? "--" : ownerName;
            AreaSqm = areaSqm;
            CanvasObjectId = canvasObjectId;
            LayerName = string.IsNullOrWhiteSpace(layerName) ? "--" : layerName;
        }

        public int RecordId { get; }

        public string MapSheetNo { get; }

        public string ParcelNo { get; }

        public string UniqueCode { get; }

        public string OwnerName { get; }

        public double AreaSqm { get; }

        public Guid? CanvasObjectId { get; }

        public string LayerName { get; }

        public bool CanSelect => CanvasObjectId.HasValue;

        public string Status => CanSelect ? "Mapped" : "No map object";

        public string DisplayArea => AreaSqm > 0
            ? $"{AreaSqm.ToString("N2", CultureInfo.CurrentCulture)} sq.m"
            : "--";

        public bool Matches(string searchText)
        {
            return Contains(ParcelNo, searchText) ||
                   Contains(MapSheetNo, searchText) ||
                   Contains(UniqueCode, searchText) ||
                   Contains(OwnerName, searchText) ||
                   Contains(LayerName, searchText) ||
                   Contains(Status, searchText);
        }

        private static bool Contains(string? value, string searchText)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}
