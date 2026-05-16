namespace Land_Readjustment_Tool.Forms
{
    partial class frmLandOwnersRecord
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
            toolStrip1 = new ToolStrip();
            btnAdd = new ToolStripButton();
            btnEdit = new ToolStripButton();
            btnDelete = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripLabel1 = new ToolStripLabel();
            txtSearch = new ToolStripTextBox();
            toolStripSeparator2 = new ToolStripSeparator();
            btnRefresh = new ToolStripDropDownButton();
            saveToolStripButton = new ToolStripButton();
            btnClose = new ToolStripButton();
            panel1 = new Panel();
            lblPaginationInfo = new Label();
            lblTotalRecords = new Label();
            dgvRecords = new DataGridView();
            toolStrip1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.ImageScalingSize = new Size(48, 48);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnAdd, btnEdit, btnDelete, toolStripSeparator1, toolStripLabel1, txtSearch, toolStripSeparator2, btnRefresh, saveToolStripButton, btnClose });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1368, 61);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnAdd
            // 
            btnAdd.BackgroundImageLayout = ImageLayout.None;
            btnAdd.Image = Properties.Resources.icons8_add_25__1_;
            btnAdd.ImageAlign = ContentAlignment.BottomCenter;
            btnAdd.ImageScaling = ToolStripItemImageScaling.None;
            btnAdd.ImageTransparentColor = Color.Magenta;
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(41, 58);
            btnAdd.Text = "Add";
            btnAdd.TextImageRelation = TextImageRelation.ImageAboveText;
            btnAdd.ToolTipText = "Add Record";
            // 
            // btnEdit
            // 
            btnEdit.Image = Properties.Resources.edit_icon;
            btnEdit.ImageAlign = ContentAlignment.BottomCenter;
            btnEdit.ImageScaling = ToolStripItemImageScaling.None;
            btnEdit.ImageTransparentColor = Color.Magenta;
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(39, 58);
            btnEdit.Text = "Edit";
            btnEdit.TextImageRelation = TextImageRelation.ImageAboveText;
            btnEdit.ToolTipText = "Edit Record";
            btnEdit.Click += BtnEdit_Click;
            // 
            // btnDelete
            // 
            btnDelete.Image = Properties.Resources.delete_icon;
            btnDelete.ImageAlign = ContentAlignment.BottomCenter;
            btnDelete.ImageScaling = ToolStripItemImageScaling.None;
            btnDelete.ImageTransparentColor = Color.Magenta;
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(57, 58);
            btnDelete.Text = "Delete";
            btnDelete.TextImageRelation = TextImageRelation.ImageAboveText;
            btnDelete.ToolTipText = "Delete Record";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 61);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.AutoToolTip = true;
            toolStripLabel1.Image = Properties.Resources.find_icon1;
            toolStripLabel1.ImageScaling = ToolStripItemImageScaling.None;
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(142, 58);
            toolStripLabel1.Text = "Search keyword ";
            toolStripLabel1.TextImageRelation = TextImageRelation.TextBeforeImage;
            // 
            // txtSearch
            // 
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(300, 61);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 61);
            // 
            // btnRefresh
            // 
            btnRefresh.Image = Properties.Resources.icons8_refresh_25;
            btnRefresh.ImageScaling = ToolStripItemImageScaling.None;
            btnRefresh.ImageTransparentColor = Color.Magenta;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(97, 58);
            btnRefresh.Text = "Refresh";
            btnRefresh.ToolTipText = "Refresh Records";
            btnRefresh.Click += btnRefresh_Click_1;
            // 
            // saveToolStripButton
            // 
            saveToolStripButton.Image = Properties.Resources.diskette2;
            saveToolStripButton.ImageScaling = ToolStripItemImageScaling.None;
            saveToolStripButton.ImageTransparentColor = Color.Magenta;
            saveToolStripButton.Name = "saveToolStripButton";
            saveToolStripButton.Size = new Size(64, 58);
            saveToolStripButton.Text = "Save";
            saveToolStripButton.ToolTipText = "Save to Database ";
            // 
            // btnClose
            // 
            btnClose.Image = Properties.Resources.delete_icon_25;
            btnClose.ImageScaling = ToolStripItemImageScaling.None;
            btnClose.ImageTransparentColor = Color.Magenta;
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(74, 58);
            btnClose.Text = "Close";
            btnClose.Click += toolStripButton1_Click;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(lblPaginationInfo);
            panel1.Controls.Add(lblTotalRecords);
            panel1.Location = new Point(0, 683);
            panel1.Name = "panel1";
            panel1.Size = new Size(1356, 50);
            panel1.TabIndex = 2;
            // 
            // lblPaginationInfo
            // 
            lblPaginationInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblPaginationInfo.AutoSize = true;
            lblPaginationInfo.Location = new Point(3, 24);
            lblPaginationInfo.Name = "lblPaginationInfo";
            lblPaginationInfo.Size = new Size(191, 20);
            lblPaginationInfo.TabIndex = 3;
            lblPaginationInfo.Text = "Showing 1 to 6 of 6 records";
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblTotalRecords.AutoSize = true;
            lblTotalRecords.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalRecords.Location = new Point(3, 4);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(121, 20);
            lblTotalRecords.TabIndex = 0;
            lblTotalRecords.Text = "Total Records: 6";
            // 
            // dgvRecords
            // 
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToOrderColumns = true;
            dgvRecords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvRecords.BackgroundColor = SystemColors.ControlLight;
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = Color.SkyBlue;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvRecords.DefaultCellStyle = dataGridViewCellStyle1;
            dgvRecords.Location = new Point(0, 64);
            dgvRecords.Name = "dgvRecords";
            dgvRecords.ReadOnly = true;
            dgvRecords.RowHeadersVisible = false;
            dgvRecords.RowHeadersWidth = 51;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.Size = new Size(1356, 613);
            dgvRecords.TabIndex = 1;
            // 
            // frmLandOwnersRecord
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1368, 735);
            Controls.Add(panel1);
            Controls.Add(dgvRecords);
            Controls.Add(toolStrip1);
            MinimumSize = new Size(1200, 700);
            Name = "frmLandOwnersRecord";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Landowner Records Manager";
            Load += frmLandOwnersRecord_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            Land_Readjustment_Tool.UI.Forms.RecordFormTheme.Apply(this);
            ResumeLayout(false);
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripButton btnAdd;
        private ToolStripButton btnEdit;
        private ToolStripButton btnDelete;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel toolStripLabel1;
        private ToolStripTextBox txtSearch;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripDropDownButton btnRefresh;
        private DataGridView dgvRecords;
        private Panel panel1;
        private Label lblTotalRecords;
        private Label lblPaginationInfo;
        private ToolStripButton saveToolStripButton;
        private ToolStripButton btnClose;
    }
}
