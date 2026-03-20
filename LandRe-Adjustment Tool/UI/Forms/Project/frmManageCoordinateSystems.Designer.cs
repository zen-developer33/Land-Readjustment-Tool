namespace Land_Readjustment_Tool.UI.Forms.Project
{
    partial class frmManageCoordinateSystems
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            pnlLeft = new Panel();
            dgvCRS = new DataGridView();
            colCode = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colEpsg = new DataGridViewTextBoxColumn();
            colRegion = new DataGridViewTextBoxColumn();
            colType = new DataGridViewTextBoxColumn();
            pnlToolbar = new Panel();
            btnAdd = new Button();
            btnCopyNew = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            btnViewParams = new Button();
            txtDetails = new TextBox();
            pnlFooter = new Panel();
            btnClose = new Button();
            splitter = new SplitContainer();
            tabControl = new TabControl();
            tabDetails = new TabPage();
            pnlRight = new Panel();
            pnlHeader.SuspendLayout();
            pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCRS).BeginInit();
            pnlToolbar.SuspendLayout();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter).BeginInit();
            splitter.Panel1.SuspendLayout();
            splitter.Panel2.SuspendLayout();
            splitter.SuspendLayout();
            tabControl.SuspendLayout();
            tabDetails.SuspendLayout();
            pnlRight.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(28, 36, 54);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(842, 60);
            pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(16, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 28);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Coordinate Reference Systems";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(170, 185, 210);
            lblSubtitle.Location = new Point(17, 34);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(477, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Default systems are read-only. Copy a default to create a custom entry.";
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(pnlToolbar);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(638, 467);
            pnlLeft.TabIndex = 0;
            // 
            // dgvCRS
            // 
            dgvCRS.AllowUserToAddRows = false;
            dgvCRS.AllowUserToDeleteRows = false;
            dgvCRS.AllowUserToResizeRows = false;
            dgvCRS.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCRS.BackgroundColor = Color.White;
            dgvCRS.BorderStyle = BorderStyle.None;
            dgvCRS.ColumnHeadersHeight = 29;
            dgvCRS.Columns.AddRange(new DataGridViewColumn[] { colCode, colName, colEpsg, colRegion, colType });
            dgvCRS.Dock = DockStyle.Bottom;
            dgvCRS.Font = new Font("Segoe UI", 9F);
            dgvCRS.Location = new Point(0, 42);
            dgvCRS.MultiSelect = false;
            dgvCRS.Name = "dgvCRS";
            dgvCRS.ReadOnly = true;
            dgvCRS.RowHeadersVisible = false;
            dgvCRS.RowHeadersWidth = 51;
            dgvCRS.RowTemplate.Height = 30;
            dgvCRS.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCRS.Size = new Size(638, 425);
            dgvCRS.TabIndex = 0;
            dgvCRS.SelectionChanged += dgvCRS_SelectionChanged;
            // 
            // colCode
            // 
            colCode.FillWeight = 15F;
            colCode.HeaderText = "Code";
            colCode.MinimumWidth = 6;
            colCode.Name = "colCode";
            colCode.ReadOnly = true;
            // 
            // colName
            // 
            colName.FillWeight = 40F;
            colName.HeaderText = "Name";
            colName.MinimumWidth = 6;
            colName.Name = "colName";
            colName.ReadOnly = true;
            // 
            // colEpsg
            // 
            colEpsg.FillWeight = 12F;
            colEpsg.HeaderText = "EPSG";
            colEpsg.MinimumWidth = 6;
            colEpsg.Name = "colEpsg";
            colEpsg.ReadOnly = true;
            // 
            // colRegion
            // 
            colRegion.FillWeight = 13F;
            colRegion.HeaderText = "Region";
            colRegion.MinimumWidth = 6;
            colRegion.Name = "colRegion";
            colRegion.ReadOnly = true;
            // 
            // colType
            // 
            colType.FillWeight = 20F;
            colType.HeaderText = "Type";
            colType.MinimumWidth = 6;
            colType.Name = "colType";
            colType.ReadOnly = true;
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.FromArgb(238, 240, 248);
            pnlToolbar.Controls.Add(btnAdd);
            pnlToolbar.Controls.Add(btnCopyNew);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(btnViewParams);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(8);
            pnlToolbar.Size = new Size(638, 47);
            pnlToolbar.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.BackColor = Color.FromArgb(28, 36, 54);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 9F);
            btnAdd.ForeColor = Color.White;
            btnAdd.Location = new Point(8, 8);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(70, 28);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "+ New";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnCopyNew
            // 
            btnCopyNew.BackColor = Color.White;
            btnCopyNew.Cursor = Cursors.Hand;
            btnCopyNew.Enabled = false;
            btnCopyNew.FlatAppearance.BorderColor = Color.FromArgb(28, 36, 54);
            btnCopyNew.FlatStyle = FlatStyle.Flat;
            btnCopyNew.Font = new Font("Segoe UI", 9F);
            btnCopyNew.ForeColor = Color.FromArgb(28, 36, 54);
            btnCopyNew.Location = new Point(84, 8);
            btnCopyNew.Name = "btnCopyNew";
            btnCopyNew.Size = new Size(60, 28);
            btnCopyNew.TabIndex = 1;
            btnCopyNew.Text = "Copy";
            btnCopyNew.UseVisualStyleBackColor = false;
            btnCopyNew.Click += btnCopyNew_Click;
            // 
            // btnEdit
            // 
            btnEdit.BackColor = Color.White;
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Enabled = false;
            btnEdit.FlatAppearance.BorderColor = Color.FromArgb(180, 185, 200);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Segoe UI", 9F);
            btnEdit.Location = new Point(150, 8);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(60, 28);
            btnEdit.TabIndex = 2;
            btnEdit.Text = "Edit";
            btnEdit.UseVisualStyleBackColor = false;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.White;
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Enabled = false;
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(200, 80, 80);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Segoe UI", 9F);
            btnDelete.ForeColor = Color.FromArgb(180, 50, 50);
            btnDelete.Location = new Point(216, 8);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(68, 28);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnViewParams
            // 
            btnViewParams.BackColor = Color.White;
            btnViewParams.Cursor = Cursors.Hand;
            btnViewParams.Enabled = false;
            btnViewParams.FlatAppearance.BorderColor = Color.FromArgb(180, 185, 200);
            btnViewParams.FlatStyle = FlatStyle.Flat;
            btnViewParams.Font = new Font("Segoe UI", 9F);
            btnViewParams.Location = new Point(290, 8);
            btnViewParams.Name = "btnViewParams";
            btnViewParams.Size = new Size(90, 28);
            btnViewParams.TabIndex = 4;
            btnViewParams.Text = "View Params";
            btnViewParams.UseVisualStyleBackColor = false;
            btnViewParams.Click += btnViewParams_Click;
            // 
            // txtDetails
            // 
            txtDetails.BackColor = Color.White;
            txtDetails.BorderStyle = BorderStyle.None;
            txtDetails.Font = new Font("Consolas", 9F);
            txtDetails.Location = new Point(5, 4);
            txtDetails.Multiline = true;
            txtDetails.Name = "txtDetails";
            txtDetails.ReadOnly = true;
            txtDetails.ScrollBars = ScrollBars.Vertical;
            txtDetails.Size = new Size(176, 413);
            txtDetails.TabIndex = 0;
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.FromArgb(232, 234, 240);
            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 527);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(842, 46);
            pnlFooter.TabIndex = 1;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.White;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(180, 185, 200);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9F);
            btnClose.Location = new Point(760, 8);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 30);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // splitter
            // 
            splitter.Dock = DockStyle.Fill;
            splitter.Location = new Point(0, 60);
            splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            splitter.Panel1.Controls.Add(dgvCRS);
            splitter.Panel1.Controls.Add(pnlLeft);
            splitter.Panel1MinSize = 300;
            // 
            // splitter.Panel2
            // 
            splitter.Panel2.Controls.Add(pnlRight);
            splitter.Panel2MinSize = 200;
            splitter.Size = new Size(842, 467);
            splitter.SplitterDistance = 638;
            splitter.TabIndex = 0;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabDetails);
            tabControl.Font = new Font("Segoe UI", 9F);
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(200, 461);
            tabControl.TabIndex = 0;
            // 
            // tabDetails
            // 
            tabDetails.BackColor = Color.White;
            tabDetails.Controls.Add(txtDetails);
            tabDetails.Location = new Point(4, 29);
            tabDetails.Name = "tabDetails";
            tabDetails.Padding = new Padding(8);
            tabDetails.Size = new Size(192, 428);
            tabDetails.TabIndex = 0;
            tabDetails.Text = "Details";
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(tabControl);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Location = new Point(0, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(200, 467);
            pnlRight.TabIndex = 0;
            // 
            // frmManageCoordinateSystems
            // 
            BackColor = Color.FromArgb(245, 246, 250);
            ClientSize = new Size(842, 573);
            Controls.Add(splitter);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(760, 520);
            Name = "frmManageCoordinateSystems";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Manage Coordinate Systems";
            Load += frmManageCoordinateSystems_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCRS).EndInit();
            pnlToolbar.ResumeLayout(false);
            pnlFooter.ResumeLayout(false);
            splitter.Panel1.ResumeLayout(false);
            splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitter).EndInit();
            splitter.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabDetails.ResumeLayout(false);
            tabDetails.PerformLayout();
            pnlRight.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel                       pnlHeader;
        private Label                       lblTitle;
        private Label                       lblSubtitle;
        private Panel                       pnlLeft;
        private Panel                       pnlToolbar;
        private Button                      btnAdd;
        private Button                      btnCopyNew;
        private Button                      btnEdit;
        private Button                      btnDelete;
        private Button                      btnViewParams;
        private DataGridView                dgvCRS;
        private DataGridViewTextBoxColumn   colCode;
        private DataGridViewTextBoxColumn   colName;
        private DataGridViewTextBoxColumn   colEpsg;
        private DataGridViewTextBoxColumn   colRegion;
        private DataGridViewTextBoxColumn   colType;
        private Panel                       pnlDetails;
        private TextBox                     txtDetails;
        private Panel                       pnlFooter;
        private Button                      btnClose;
        private SplitContainer             splitter;
        private TabControl tabControl;
        private TabPage tabDetails;
        private Panel pnlRight;
    }
}
