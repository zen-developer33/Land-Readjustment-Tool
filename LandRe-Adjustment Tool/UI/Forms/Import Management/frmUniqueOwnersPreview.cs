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

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            dgvUniqueOwners = new DataGridView();
            btnOK = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvUniqueOwners).BeginInit();
            SuspendLayout();
            // 
            // dgvUniqueOwners
            // 
            dgvUniqueOwners.AllowUserToAddRows = false;
            dgvUniqueOwners.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(248, 248, 252);
            dgvUniqueOwners.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvUniqueOwners.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvUniqueOwners.BackgroundColor = SystemColors.Control;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(45, 65, 95);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvUniqueOwners.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvUniqueOwners.ColumnHeadersHeight = 34;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvUniqueOwners.DefaultCellStyle = dataGridViewCellStyle3;
            dgvUniqueOwners.EnableHeadersVisualStyles = false;
            dgvUniqueOwners.Location = new Point(12, 12);
            dgvUniqueOwners.Name = "dgvUniqueOwners";
            dgvUniqueOwners.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = SystemColors.Control;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvUniqueOwners.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvUniqueOwners.RowHeadersWidth = 50;
            dgvUniqueOwners.RowTemplate.Height = 28;
            dgvUniqueOwners.Size = new Size(694, 389);
            dgvUniqueOwners.TabIndex = 0;
            dgvUniqueOwners.RowPostPaint += DgvUniqueOwners_RowPostPaint;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.ImeMode = ImeMode.NoControl;
            btnOK.Location = new Point(606, 408);
            btnOK.Margin = new Padding(3, 4, 3, 4);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(100, 35);
            btnOK.TabIndex = 10;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // frmUniqueOwnersPreview
            // 
            ClientSize = new Size(718, 444);
            Controls.Add(btnOK);
            Controls.Add(dgvUniqueOwners);
            Name = "frmUniqueOwnersPreview";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Unique Owners Preview";
            Load += frmUniqueOwnersPreview_Load;
            ((System.ComponentModel.ISupportInitialize)dgvUniqueOwners).EndInit();
            ResumeLayout(false);
        }

        private Button btnOK;
        private DataGridView dgvUniqueOwners;
    }
}
