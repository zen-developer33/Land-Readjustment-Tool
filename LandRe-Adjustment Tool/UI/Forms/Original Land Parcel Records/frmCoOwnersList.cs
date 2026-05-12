using Land_Readjustment_Tool.Models;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmCoOwnersList : Form
    {
        private readonly List<CoOwnerRecord> _coOwners;

        public frmCoOwnersList(List<CoOwnerRecord> coOwners, string parcelNo)
        {
            _coOwners = coOwners;
            InitializeComponent();
            Text = $"Co-Owners - Parcel {parcelNo}";
            LoadGrid();
        }

        private void LoadGrid()
        {
            dgv.Rows.Clear();
            foreach (var coOwner in _coOwners)
            {
                int rowIndex = dgv.Rows.Add();
                var row = dgv.Rows[rowIndex];
                row.Cells["colName"].Value = coOwner.OwnerName ?? string.Empty;
                row.Cells["colFather"].Value = coOwner.FatherSpouse ?? string.Empty;
                row.Cells["colCitizenship"].Value = coOwner.CitizenshipNumber ?? string.Empty;
                row.Cells["colAddress"].Value = coOwner.PermanentAddress ?? string.Empty;
                row.Cells["colShare"].Value = coOwner.OwnershipSharePercent?.ToString("G") ?? string.Empty;
                row.Tag = coOwner;
            }
        }

        private void Dgv_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _coOwners.Count) return;

            var coOwner = _coOwners[e.RowIndex];
            var columnName = dgv.Columns[e.ColumnIndex].Name;
            var value = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? string.Empty;

            switch (columnName)
            {
                case "colName":
                    coOwner.OwnerName = value;
                    break;
                case "colFather":
                    coOwner.FatherSpouse = value;
                    break;
                case "colCitizenship":
                    coOwner.CitizenshipNumber = value;
                    break;
                case "colAddress":
                    coOwner.PermanentAddress = value;
                    break;
                case "colShare":
                    coOwner.OwnershipSharePercent = double.TryParse(value, out var share) ? share : null;
                    break;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var coOwner = new CoOwnerRecord();
            _coOwners.Add(coOwner);
            int rowIndex = dgv.Rows.Add();
            dgv.Rows[rowIndex].Tag = coOwner;
            dgv.CurrentCell = dgv.Rows[rowIndex].Cells["colName"];
            dgv.BeginEdit(true);
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;

            int rowIndex = dgv.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _coOwners.Count) return;

            _coOwners.RemoveAt(rowIndex);
            dgv.Rows.RemoveAt(rowIndex);
        }
    }
}
