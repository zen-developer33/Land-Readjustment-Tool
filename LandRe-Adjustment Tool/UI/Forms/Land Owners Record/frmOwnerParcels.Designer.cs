namespace Land_Readjustment_Tool.Forms
{
    partial class frmOwnerParcels
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            pnlTop = new Panel();
            lblParcelCount = new Label();
            dgvParcels = new DataGridView();
            btnClose = new Button();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParcels).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlTop.Controls.Add(lblParcelCount);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Margin = new Padding(4);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(783, 41);
            pnlTop.TabIndex = 0;
            // 
            // lblParcelCount
            // 
            lblParcelCount.AutoSize = true;
            lblParcelCount.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblParcelCount.Location = new Point(12, 9);
            lblParcelCount.Margin = new Padding(4, 0, 4, 0);
            lblParcelCount.Name = "lblParcelCount";
            lblParcelCount.Size = new Size(202, 23);
            lblParcelCount.TabIndex = 0;
            lblParcelCount.Text = "No. of Parcels Owned: 0";
            // 
            // dgvParcels
            // 
            dgvParcels.AllowUserToAddRows = false;
            dgvParcels.AllowUserToDeleteRows = false;
            dgvParcels.AllowUserToOrderColumns = true;
            dgvParcels.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvParcels.BackgroundColor = SystemColors.ControlLight;
            dgvParcels.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = Color.SkyBlue;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvParcels.DefaultCellStyle = dataGridViewCellStyle1;
            dgvParcels.Location = new Point(0, 40);
            dgvParcels.Margin = new Padding(4);
            dgvParcels.Name = "dgvParcels";
            dgvParcels.ReadOnly = true;
            dgvParcels.RowHeadersVisible = false;
            dgvParcels.RowHeadersWidth = 51;
            dgvParcels.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParcels.Size = new Size(783, 392);
            dgvParcels.TabIndex = 1;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(680, 439);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(94, 29);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmOwnerParcels
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            CancelButton = btnClose;
            ClientSize = new Size(783, 475);
            Controls.Add(btnClose);
            Controls.Add(dgvParcels);
            Controls.Add(pnlTop);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            MinimizeBox = false;
            MinimumSize = new Size(746, 426);
            Name = "frmOwnerParcels";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Owner Parcels";
            Load += frmOwnerParcels_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParcels).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblParcelCount;
        private DataGridView dgvParcels;
        private Button button1;
        private Button btnClose;
    }
}
