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

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            dgvUniqueOwners = new DataGridView();
            btnOK = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvUniqueOwners).BeginInit();
            SuspendLayout();
            // 
            // dgvUniqueOwners
            // 
            dgvUniqueOwners.AllowUserToAddRows = false;
            dgvUniqueOwners.AllowUserToDeleteRows = false;
            dgvUniqueOwners.DoubleBuffered(true);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dgvUniqueOwners.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);
            dgvUniqueOwners.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvUniqueOwners.BackgroundColor = SystemColors.Control;
            dgvUniqueOwners.ColumnHeadersHeight = 29;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvUniqueOwners.DefaultCellStyle = dataGridViewCellStyle2;
            dgvUniqueOwners.Location = new Point(12, 12);
            dgvUniqueOwners.Name = "dgvUniqueOwners";
            dgvUniqueOwners.ReadOnly = true;
            dgvUniqueOwners.RowHeadersVisible = true;
            dgvUniqueOwners.RowHeadersWidth = 50;
            dgvUniqueOwners.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUniqueOwners.Size = new Size(694, 389);
            dgvUniqueOwners.TabIndex = 0;

            // Headers styled (not bold)
            dgvUniqueOwners.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvUniqueOwners.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 65, 95);
            dgvUniqueOwners.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUniqueOwners.ColumnHeadersHeight = 34;
            dgvUniqueOwners.EnableHeadersVisualStyles = false;
            dgvUniqueOwners.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvUniqueOwners.RowTemplate.Height = 28;
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
            Text = "Unique Owners Preview";
            Load += frmUniqueOwnersPreview_Load;
            ((System.ComponentModel.ISupportInitialize)dgvUniqueOwners).EndInit();
            ResumeLayout(false);
        }

        private Button btnOK;
        private DataGridView dgvUniqueOwners;
    }
}
