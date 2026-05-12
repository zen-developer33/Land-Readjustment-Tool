using Land_Readjustment_Tool.Models;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmJointOwnershipResolver : Form
    {
        private readonly List<BaselineLandParcelRecord> _records;
        private readonly string _parcelKey;

        public int PrimaryIndex { get; private set; }

        public frmJointOwnershipResolver(List<BaselineLandParcelRecord> records, string parcelKey)
        {
            if (records == null || records.Count < 2)
                throw new ArgumentException("At least 2 records required.", nameof(records));

            _records = records;
            _parcelKey = parcelKey;

            InitializeComponent();
            lblParcelInfo.Text = $"Parcel: {_parcelKey.Replace("::", " / Map Sheet: ")}";
            PopulateGrid();
        }

        private void PopulateGrid()
        {
            dgvOwners.Rows.Clear();

            for (int i = 0; i < _records.Count; i++)
            {
                var record = _records[i];
                int rowIndex = dgvOwners.Rows.Add();
                var row = dgvOwners.Rows[rowIndex];

                bool isPrimary = i == PrimaryIndex;
                row.Cells["colRole"].Value = isPrimary ? "Primary" : "Co-Owner";
                row.Cells["colOwnerName"].Value = record.LandOwnersName ?? string.Empty;
                row.Cells["colFather"].Value = record.FatherSpouse ?? string.Empty;
                row.Cells["colCitizenship"].Value = record.CitizenshipNumber ?? string.Empty;
                row.Cells["colShare"].Value = string.Empty;
                row.Tag = i;

                ApplyRowState(row, isPrimary);
            }

            if (dgvOwners.Rows.Count > 0)
                dgvOwners.Rows[PrimaryIndex].Selected = true;
        }

        private void ApplyRowState(DataGridViewRow row, bool isPrimary)
        {
            row.Cells["colRole"].Value = isPrimary ? "Primary" : "Co-Owner";
            row.Cells["colShare"].Value = isPrimary ? string.Empty : row.Cells["colShare"].Value;
            ((DataGridViewTextBoxCell)row.Cells["colShare"]).ReadOnly = isPrimary;
        }

        private void DgvOwners_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvOwners.SelectedRows.Count == 0) return;
            if (dgvOwners.SelectedRows[0].Tag is not int index) return;
            if (index == PrimaryIndex) return;

            PrimaryIndex = index;
            RefreshRoles();
        }

        private void RefreshRoles()
        {
            foreach (DataGridViewRow row in dgvOwners.Rows)
            {
                bool isPrimary = row.Tag is int index && index == PrimaryIndex;
                ApplyRowState(row, isPrimary);
            }
        }

        private void DgvOwners_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (dgvOwners.Columns[e.ColumnIndex].Name != "colShare") return;
            var value = e.FormattedValue?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return;

            if (!double.TryParse(value, out double share) || share < 0 || share > 100)
            {
                e.Cancel = true;
                MessageBox.Show("Ownership share must be a number between 0 and 100.",
                    "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            for (int i = 0; i < dgvOwners.Rows.Count; i++)
            {
                var row = dgvOwners.Rows[i];
                if (row.Tag is not int recordIndex) continue;
                if (recordIndex == PrimaryIndex) continue;

                var shareCell = row.Cells["colShare"].Value?.ToString();
                if (double.TryParse(shareCell, out double share))
                {
                    _records[recordIndex].Remarks =
                        string.IsNullOrWhiteSpace(_records[recordIndex].Remarks)
                            ? $"__SHARE__{share}"
                            : _records[recordIndex].Remarks + $" __SHARE__{share}";
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        public static double? ExtractShare(string? remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks)) return null;
            const string tag = "__SHARE__";
            int position = remarks.LastIndexOf(tag, StringComparison.Ordinal);
            if (position < 0) return null;
            var rest = remarks[(position + tag.Length)..].Trim().Split(' ')[0];
            return double.TryParse(rest, out var value) ? value : null;
        }

        public static string StripShareTag(string? remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks)) return string.Empty;
            const string tag = "__SHARE__";
            int position = remarks.LastIndexOf(tag, StringComparison.Ordinal);
            return position < 0 ? remarks : remarks[..position].TrimEnd();
        }
    }
}
