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
            pnlToolbar = new Panel();
            btnAdd = new Button();
            btnCopyNew = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            pnlDetails = new Panel();
            txtDetails = new TextBox();
            lblDetailsTitle = new Label();
            pnlFooter = new Panel();
            lblHint = new Label();
            btnClose = new Button();
            dgvCRS = new DataGridView();
            colCode = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colEpsg = new DataGridViewTextBoxColumn();
            colRegion = new DataGridViewTextBoxColumn();
            colType = new DataGridViewTextBoxColumn();
            pnlToolbar.SuspendLayout();
            pnlDetails.SuspendLayout();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCRS).BeginInit();
            SuspendLayout();
            // 
            // pnlToolbar
            // 
            pnlToolbar.Controls.Add(btnAdd);
            pnlToolbar.Controls.Add(btnCopyNew);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(4);
            pnlToolbar.Size = new Size(602, 41);
            pnlToolbar.TabIndex = 0;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(4, 4);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(60, 31);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "New";
            btnAdd.Click += btnAdd_Click;
            // 
            // btnCopyNew
            // 
            btnCopyNew.Enabled = false;
            btnCopyNew.Location = new Point(68, 4);
            btnCopyNew.Name = "btnCopyNew";
            btnCopyNew.Size = new Size(60, 31);
            btnCopyNew.TabIndex = 1;
            btnCopyNew.Text = "Copy";
            btnCopyNew.Click += btnCopyNew_Click;
            // 
            // btnEdit
            // 
            btnEdit.Enabled = false;
            btnEdit.Location = new Point(132, 4);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(60, 31);
            btnEdit.TabIndex = 2;
            btnEdit.Text = "Edit";
            btnEdit.Click += btnEdit_Click;
            // 
            // btnDelete
            // 
            btnDelete.Enabled = false;
            btnDelete.ForeColor = Color.DarkRed;
            btnDelete.Location = new Point(196, 4);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(74, 31);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.Click += btnDelete_Click;
            // 
            // pnlDetails
            // 
            pnlDetails.Controls.Add(txtDetails);
            pnlDetails.Controls.Add(lblDetailsTitle);
            pnlDetails.Dock = DockStyle.Right;
            pnlDetails.Location = new Point(602, 0);
            pnlDetails.Name = "pnlDetails";
            pnlDetails.Padding = new Padding(4);
            pnlDetails.Size = new Size(300, 435);
            pnlDetails.TabIndex = 2;
            // 
            // txtDetails
            // 
            txtDetails.BackColor = SystemColors.Window;
            txtDetails.BorderStyle = BorderStyle.FixedSingle;
            txtDetails.Dock = DockStyle.Fill;
            txtDetails.Font = new Font("Consolas", 9F);
            txtDetails.Location = new Point(4, 26);
            txtDetails.Multiline = true;
            txtDetails.Name = "txtDetails";
            txtDetails.ReadOnly = true;
            txtDetails.ScrollBars = ScrollBars.Vertical;
            txtDetails.Size = new Size(292, 405);
            txtDetails.TabIndex = 1;
            // 
            // lblDetailsTitle
            // 
            lblDetailsTitle.Dock = DockStyle.Top;
            lblDetailsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDetailsTitle.Location = new Point(4, 4);
            lblDetailsTitle.Name = "lblDetailsTitle";
            lblDetailsTitle.Padding = new Padding(2, 2, 0, 0);
            lblDetailsTitle.Size = new Size(292, 22);
            lblDetailsTitle.TabIndex = 0;
            lblDetailsTitle.Text = "Details";
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(lblHint);
            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 435);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(902, 38);
            pnlFooter.TabIndex = 3;
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(100, 9);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(445, 20);
            lblHint.TabIndex = 0;
            lblHint.Text = "🔒 Default entries are read-only. Use Copy to create a custom entry.";
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(1429, 6);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 26);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;
            // 
            // dgvCRS
            // 
            dgvCRS.AllowUserToAddRows = false;
            dgvCRS.AllowUserToDeleteRows = false;
            dgvCRS.AllowUserToResizeRows = false;
            dgvCRS.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCRS.BackgroundColor = SystemColors.Window;
            dgvCRS.BorderStyle = BorderStyle.None;
            dgvCRS.ColumnHeadersHeight = 29;
            dgvCRS.Columns.AddRange(new DataGridViewColumn[] { colCode, colName, colEpsg, colRegion, colType });
            dgvCRS.Dock = DockStyle.Fill;
            dgvCRS.Font = new Font("Segoe UI", 9F);
            dgvCRS.GridColor = SystemColors.ControlLight;
            dgvCRS.Location = new Point(0, 41);
            dgvCRS.MultiSelect = false;
            dgvCRS.Name = "dgvCRS";
            dgvCRS.ReadOnly = true;
            dgvCRS.RowHeadersVisible = false;
            dgvCRS.RowHeadersWidth = 51;
            dgvCRS.RowTemplate.Height = 26;
            dgvCRS.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCRS.Size = new Size(602, 394);
            dgvCRS.TabIndex = 1;
            dgvCRS.CellDoubleClick += dgvCRS_CellDoubleClick;
            dgvCRS.SelectionChanged += dgvCRS_SelectionChanged;
            // 
            // colCode
            // 
            colCode.FillWeight = 14F;
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
            colRegion.FillWeight = 14F;
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
            // frmManageCoordinateSystems
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(902, 473);
            Controls.Add(dgvCRS);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlDetails);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(700, 420);
            Name = "frmManageCoordinateSystems";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Manage Coordinate Systems";
            Load += frmManageCoordinateSystems_Load;
            pnlToolbar.ResumeLayout(false);
            pnlDetails.ResumeLayout(false);
            pnlDetails.PerformLayout();
            pnlFooter.ResumeLayout(false);
            pnlFooter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCRS).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlToolbar;
        private Button btnAdd;
        private Button btnCopyNew;
        private Button btnEdit;
        private Button btnDelete;
        private Panel pnlDetails;
        private Label lblDetailsTitle;
        private TextBox txtDetails;
        private Panel pnlFooter;
        private Label lblHint;
        private Button btnClose;
        private DataGridView dgvCRS;
        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colEpsg;
        private DataGridViewTextBoxColumn colRegion;
        private DataGridViewTextBoxColumn colType;
    }
}