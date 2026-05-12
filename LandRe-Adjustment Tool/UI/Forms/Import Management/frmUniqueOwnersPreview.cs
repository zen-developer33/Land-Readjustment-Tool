using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmUniqueOwnersPreview : Form
    {
        private List<OwnerDeduplicationService.UniqueOwner> _uniqueOwners;

        public frmUniqueOwnersPreview(List<OwnerDeduplicationService.UniqueOwner> uniqueOwners)
        {
            InitializeComponent();
            _uniqueOwners = uniqueOwners;
        }

        private void frmUniqueOwnersPreview_Load(object sender, EventArgs e)
        {
            dgvUniqueOwners.AutoGenerateColumns = false;
            dgvUniqueOwners.Columns.Clear();

            dgvUniqueOwners.Columns.Add(new DataGridViewTextBoxColumn { Name = "OwnerName", HeaderText = "Owner Name", DataPropertyName = "LandOwnersName", Width = 200 });
            dgvUniqueOwners.Columns.Add(new DataGridViewTextBoxColumn { Name = "Father", HeaderText = "Father/Spouse", DataPropertyName = "FatherSpouse", Width = 160 });
            dgvUniqueOwners.Columns.Add(new DataGridViewTextBoxColumn { Name = "Citizenship", HeaderText = "Citizenship No.", DataPropertyName = "CitizenshipNumber", Width = 140 });
            dgvUniqueOwners.Columns.Add(new DataGridViewTextBoxColumn { Name = "PermanentAddress", HeaderText = "Permanent Address", DataPropertyName = "PermanentAddress", Width = 220 });

            foreach (var owner in _uniqueOwners)
            {
                int idx = dgvUniqueOwners.Rows.Add();
                var row = dgvUniqueOwners.Rows[idx];
                row.Cells["OwnerName"].Value = owner.LandOwnersName;
                row.Cells["Father"].Value = owner.FatherSpouse;
                row.Cells["Citizenship"].Value = owner.CitizenshipNumber;
                row.Cells["PermanentAddress"].Value = owner.PermanentAddress;
            }

            // Clear selection after loading
            dgvUniqueOwners.ClearSelection();

            // Wire up events to clear selection when focus leaves
            dgvUniqueOwners.Leave += DgvUniqueOwners_Leave;
            dgvUniqueOwners.MouseLeave += DgvUniqueOwners_MouseLeave;
        }

        /// <summary>
        /// Paints row numbers in the row header for each visible row.
        /// </summary>
        private void DgvUniqueOwners_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            string rowNumber = (e.RowIndex + 1).ToString();
            var headerBounds = new Rectangle(
                e.RowBounds.Left, 
                e.RowBounds.Top, 
                dgvUniqueOwners.RowHeadersWidth - 4, 
                e.RowBounds.Height);
            
            TextRenderer.DrawText(
                e.Graphics,
                rowNumber,
                dgvUniqueOwners.RowHeadersDefaultCellStyle.Font ?? dgvUniqueOwners.DefaultCellStyle.Font,
                headerBounds,
                dgvUniqueOwners.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void DgvUniqueOwners_Leave(object? sender, EventArgs e)
        {
            dgvUniqueOwners.ClearSelection();
        }

        private void DgvUniqueOwners_MouseLeave(object? sender, EventArgs e)
        {
            dgvUniqueOwners.ClearSelection();
        }
    }
}
