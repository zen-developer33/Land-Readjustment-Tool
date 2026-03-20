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
            pnlHeader = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            pnlToolbar = new Panel();
            btnAdd = new Button();
            btnCopyNew = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            dgvDatum = new DataGridView();
            colCode = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colSource = new DataGridViewTextBoxColumn();
            colTarget = new DataGridViewTextBoxColumn();
            colApplies = new DataGridViewTextBoxColumn();
            colEntryType = new DataGridViewTextBoxColumn();
            splitter = new SplitContainer();
            pnlLeft = new Panel();
            txtDetails = new TextBox();
            pnlFooter = new Panel();
            btnClose = new Button();
            pnlHeader.SuspendLayout();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatum).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitter).BeginInit();
            splitter.Panel1.SuspendLayout();
            splitter.Panel2.SuspendLayout();
            splitter.SuspendLayout();
            pnlLeft.SuspendLayout();
            pnlFooter.SuspendLayout();
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
            pnlHeader.Size = new Size(963, 60);
            pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(16, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(236, 28);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Datum Transformations";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(170, 185, 210);
            lblSubtitle.Location = new Point(17, 34);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(490, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Helmert 7-parameter transformations between Everest 1830 and WGS84.";
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.FromArgb(238, 240, 248);
            pnlToolbar.Controls.Add(btnAdd);
            pnlToolbar.Controls.Add(btnCopyNew);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(740, 44);
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
            btnDelete.ForeColor = Color.FromArgb(180, 50, 50);
            btnDelete.Location = new Point(216, 8);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(72, 28);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // dgvDatum
            // 
            dgvDatum.AllowUserToAddRows = false;
            dgvDatum.AllowUserToDeleteRows = false;
            dgvDatum.AllowUserToResizeRows = false;
            dgvDatum.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDatum.BackgroundColor = Color.White;
            dgvDatum.BorderStyle = BorderStyle.None;
            dgvDatum.ColumnHeadersHeight = 29;
            dgvDatum.Columns.AddRange(new DataGridViewColumn[] { colCode, colName, colSource, colTarget, colApplies, colEntryType });
            dgvDatum.Dock = DockStyle.Fill;
            dgvDatum.Font = new Font("Segoe UI", 9F);
            dgvDatum.Location = new Point(0, 44);
            dgvDatum.MultiSelect = false;
            dgvDatum.Name = "dgvDatum";
            dgvDatum.ReadOnly = true;
            dgvDatum.RowHeadersVisible = false;
            dgvDatum.RowHeadersWidth = 51;
            dgvDatum.RowTemplate.Height = 30;
            dgvDatum.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDatum.Size = new Size(740, 379);
            dgvDatum.TabIndex = 0;
            dgvDatum.SelectionChanged += dgvDatum_SelectionChanged;
            // 
            // colCode
            // 
            colCode.FillWeight = 18F;
            colCode.HeaderText = "Code";
            colCode.MinimumWidth = 6;
            colCode.Name = "colCode";
            colCode.ReadOnly = true;
            // 
            // colName
            // 
            colName.FillWeight = 32F;
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
            colApplies.FillWeight = 16F;
            colApplies.HeaderText = "Applies To";
            colApplies.MinimumWidth = 6;
            colApplies.Name = "colApplies";
            colApplies.ReadOnly = true;
            // 
            // colEntryType
            // 
            colEntryType.FillWeight = 10F;
            colEntryType.HeaderText = "Type";
            colEntryType.MinimumWidth = 6;
            colEntryType.Name = "colEntryType";
            colEntryType.ReadOnly = true;
            // 
            // splitter
            // 
            splitter.Dock = DockStyle.Fill;
            splitter.Location = new Point(0, 60);
            splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            splitter.Panel1.Controls.Add(pnlLeft);
            splitter.Panel1MinSize = 300;
            // 
            // splitter.Panel2
            // 
            splitter.Panel2.Controls.Add(txtDetails);
            splitter.Panel2MinSize = 200;
            splitter.Size = new Size(963, 423);
            splitter.SplitterDistance = 740;
            splitter.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(dgvDatum);
            pnlLeft.Controls.Add(pnlToolbar);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(740, 423);
            pnlLeft.TabIndex = 0;
            // 
            // txtDetails
            // 
            txtDetails.BackColor = Color.White;
            txtDetails.BorderStyle = BorderStyle.None;
            txtDetails.Dock = DockStyle.Fill;
            txtDetails.Font = new Font("Consolas", 9F);
            txtDetails.Location = new Point(0, 0);
            txtDetails.Multiline = true;
            txtDetails.Name = "txtDetails";
            txtDetails.ReadOnly = true;
            txtDetails.ScrollBars = ScrollBars.Vertical;
            txtDetails.Size = new Size(219, 423);
            txtDetails.TabIndex = 0;
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.FromArgb(232, 234, 240);
            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 483);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(963, 46);
            pnlFooter.TabIndex = 1;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.White;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(180, 185, 200);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Location = new Point(800, 8);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 30);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // frmManageDatumTransformations
            // 
            BackColor = Color.FromArgb(245, 246, 250);
            ClientSize = new Size(963, 529);
            Controls.Add(splitter);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(760, 500);
            Name = "frmManageDatumTransformations";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Manage Datum Transformations";
            Load += frmManageDatumTransformations_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDatum).EndInit();
            splitter.Panel1.ResumeLayout(false);
            splitter.Panel2.ResumeLayout(false);
            splitter.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitter).EndInit();
            splitter.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            pnlFooter.ResumeLayout(false);
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
        private DataGridView                dgvDatum;
        private DataGridViewTextBoxColumn   colCode;
        private DataGridViewTextBoxColumn   colName;
        private DataGridViewTextBoxColumn   colSource;
        private DataGridViewTextBoxColumn   colTarget;
        private DataGridViewTextBoxColumn   colApplies;
        private DataGridViewTextBoxColumn   colEntryType;
        private TextBox                     txtDetails;
        private Panel                       pnlFooter;
        private Button                      btnClose;
        private SplitContainer             splitter;
    }
}
