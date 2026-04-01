namespace Land_Readjustment_Tool.UI.Forms.Project
{
    partial class frmManageDatumTransformations
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
            dgvDatum = new DataGridView();
            colCode = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colSource = new DataGridViewTextBoxColumn();
            colTarget = new DataGridViewTextBoxColumn();
            colApplies = new DataGridViewTextBoxColumn();
            colEntryType = new DataGridViewTextBoxColumn();
            btnClose = new Button();
            lblHint = new Label();
            pnlFooter = new Panel();
            pnlToolbar.SuspendLayout();
            pnlDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatum).BeginInit();
            pnlFooter.SuspendLayout();
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
            pnlToolbar.Size = new Size(591, 43);
            pnlToolbar.TabIndex = 0;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(4, 4);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(60, 32);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "New";
            btnAdd.Click += btnAdd_Click;
            // 
            // btnCopyNew
            // 
            btnCopyNew.Enabled = false;
            btnCopyNew.Location = new Point(68, 4);
            btnCopyNew.Name = "btnCopyNew";
            btnCopyNew.Size = new Size(60, 32);
            btnCopyNew.TabIndex = 1;
            btnCopyNew.Text = "Copy";
            btnCopyNew.Click += btnCopyNew_Click;
            // 
            // btnEdit
            // 
            btnEdit.Enabled = false;
            btnEdit.Location = new Point(132, 4);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(60, 32);
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
            btnDelete.Size = new Size(84, 32);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.Click += btnDelete_Click;
            // 
            // pnlDetails
            // 
            pnlDetails.Controls.Add(txtDetails);
            pnlDetails.Controls.Add(lblDetailsTitle);
            pnlDetails.Dock = DockStyle.Right;
            pnlDetails.Location = new Point(591, 0);
            pnlDetails.Name = "pnlDetails";
            pnlDetails.Padding = new Padding(4);
            pnlDetails.Size = new Size(300, 484);
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
            txtDetails.Size = new Size(292, 454);
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
            // dgvDatum
            // 
            dgvDatum.AllowUserToAddRows = false;
            dgvDatum.AllowUserToDeleteRows = false;
            dgvDatum.AllowUserToResizeRows = false;
            dgvDatum.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDatum.BackgroundColor = SystemColors.Window;
            dgvDatum.BorderStyle = BorderStyle.None;
            dgvDatum.ColumnHeadersHeight = 29;
            dgvDatum.Columns.AddRange(new DataGridViewColumn[] { colCode, colName, colSource, colTarget, colApplies, colEntryType });
            dgvDatum.Dock = DockStyle.Fill;
            dgvDatum.Font = new Font("Segoe UI", 9F);
            dgvDatum.GridColor = SystemColors.ControlLight;
            dgvDatum.Location = new Point(0, 43);
            dgvDatum.MultiSelect = false;
            dgvDatum.Name = "dgvDatum";
            dgvDatum.ReadOnly = true;
            dgvDatum.RowHeadersVisible = false;
            dgvDatum.RowHeadersWidth = 51;
            dgvDatum.RowTemplate.Height = 26;
            dgvDatum.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDatum.Size = new Size(591, 441);
            dgvDatum.TabIndex = 1;
            dgvDatum.SelectionChanged += dgvDatum_SelectionChanged;
            // 
            // colCode
            // 
            colCode.FillWeight = 16F;
            colCode.HeaderText = "Code";
            colCode.MinimumWidth = 6;
            colCode.Name = "colCode";
            colCode.ReadOnly = true;
            // 
            // colName
            // 
            colName.FillWeight = 30F;
            colName.HeaderText = "Name";
            colName.MinimumWidth = 6;
            colName.Name = "colName";
            colName.ReadOnly = true;
            // 
            // colSource
            // 
            colSource.FillWeight = 14F;
            colSource.HeaderText = "Source Datum";
            colSource.MinimumWidth = 6;
            colSource.Name = "colSource";
            colSource.ReadOnly = true;
            // 
            // colTarget
            // 
            colTarget.FillWeight = 10F;
            colTarget.HeaderText = "Target";
            colTarget.MinimumWidth = 6;
            colTarget.Name = "colTarget";
            colTarget.ReadOnly = true;
            // 
            // colApplies
            // 
            colApplies.FillWeight = 18F;
            colApplies.HeaderText = "Applies To";
            colApplies.MinimumWidth = 6;
            colApplies.Name = "colApplies";
            colApplies.ReadOnly = true;
            // 
            // colEntryType
            // 
            colEntryType.FillWeight = 12F;
            colEntryType.HeaderText = "Type";
            colEntryType.MinimumWidth = 6;
            colEntryType.Name = "colEntryType";
            colEntryType.ReadOnly = true;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(1478, 6);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 26);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(12, 9);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(445, 20);
            lblHint.TabIndex = 0;
            lblHint.Text = "🔒 Default entries are read-only. Use Copy to create a custom entry.";
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(lblHint);
            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 484);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(891, 38);
            pnlFooter.TabIndex = 3;
            // 
            // frmManageDatumTransformations
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(891, 522);
            Controls.Add(dgvDatum);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlDetails);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(700, 420);
            Name = "frmManageDatumTransformations";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Manage Datum Transformations";
            Load += frmManageDatumTransformations_Load;
            pnlToolbar.ResumeLayout(false);
            pnlDetails.ResumeLayout(false);
            pnlDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatum).EndInit();
            pnlFooter.ResumeLayout(false);
            pnlFooter.PerformLayout();
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
        private DataGridView dgvDatum;
        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colSource;
        private DataGridViewTextBoxColumn colTarget;
        private DataGridViewTextBoxColumn colApplies;
        private DataGridViewTextBoxColumn colEntryType;
        private Button btnClose;
        private Label lblHint;
        private Panel pnlFooter;
    }
}