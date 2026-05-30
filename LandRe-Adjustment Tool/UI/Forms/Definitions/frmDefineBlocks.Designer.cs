namespace Land_Readjustment_Tool.UI.Forms.Definitions
{
    partial class frmDefineBlocks
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
            btnDuplicate = new Button();
            btnDetails = new Button();
            btnDelete = new Button();
            btnRefresh = new Button();
            lblSearch = new Label();
            txtSearch = new TextBox();
            dgvBlocks = new DataGridView();
            colCode = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colType = new DataGridViewComboBoxColumn();
            colDepth = new DataGridViewTextBoxColumn();
            colLength = new DataGridViewTextBoxColumn();
            pnlFooter = new Panel();
            lblStatus = new Label();
            btnClose = new Button();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBlocks).BeginInit();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlToolbar
            // 
            pnlToolbar.Controls.Add(btnAdd);
            pnlToolbar.Controls.Add(btnDuplicate);
            pnlToolbar.Controls.Add(btnDetails);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(lblSearch);
            pnlToolbar.Controls.Add(txtSearch);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(6);
            pnlToolbar.Size = new Size(732, 42);
            pnlToolbar.TabIndex = 0;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(0, 6);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(70, 29);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDuplicate
            // 
            btnDuplicate.Location = new Point(76, 6);
            btnDuplicate.Name = "btnDuplicate";
            btnDuplicate.Size = new Size(88, 29);
            btnDuplicate.TabIndex = 1;
            btnDuplicate.Text = "Duplicate";
            btnDuplicate.UseVisualStyleBackColor = true;
            btnDuplicate.Click += btnDuplicate_Click;
            // 
            // btnDetails
            // 
            btnDetails.Location = new Point(170, 6);
            btnDetails.Name = "btnDetails";
            btnDetails.Size = new Size(75, 29);
            btnDetails.TabIndex = 2;
            btnDetails.Text = "Details";
            btnDetails.UseVisualStyleBackColor = true;
            btnDetails.Click += btnDetails_Click;
            // 
            // btnDelete
            // 
            btnDelete.ForeColor = Color.DarkRed;
            btnDelete.Location = new Point(251, 6);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 29);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(332, 6);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 29);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // lblSearch
            // 
            lblSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(470, 12);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(56, 20);
            lblSearch.TabIndex = 6;
            lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.Location = new Point(532, 9);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(194, 27);
            txtSearch.TabIndex = 7;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // dgvBlocks
            // 
            dgvBlocks.AllowUserToAddRows = false;
            dgvBlocks.AllowUserToDeleteRows = false;
            dgvBlocks.AllowUserToResizeRows = false;
            dgvBlocks.AutoGenerateColumns = false;
            dgvBlocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvBlocks.BackgroundColor = SystemColors.Window;
            dgvBlocks.BorderStyle = BorderStyle.None;
            dgvBlocks.ColumnHeadersHeight = 29;
            dgvBlocks.Columns.AddRange(new DataGridViewColumn[] { colCode, colName, colType, colDepth, colLength });
            dgvBlocks.Dock = DockStyle.Fill;
            dgvBlocks.Font = new Font("Segoe UI", 9F);
            dgvBlocks.GridColor = SystemColors.ControlLight;
            dgvBlocks.Location = new Point(0, 42);
            dgvBlocks.MultiSelect = false;
            dgvBlocks.Name = "dgvBlocks";
            dgvBlocks.RowHeadersVisible = false;
            dgvBlocks.RowHeadersWidth = 51;
            dgvBlocks.RowTemplate.Height = 26;
            dgvBlocks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBlocks.Size = new Size(732, 363);
            dgvBlocks.TabIndex = 1;
            //
            // colCode
            //
            colCode.DataPropertyName = "Code";
            colCode.HeaderText = "Block Code";
            colCode.MinimumWidth = 6;
            colCode.Name = "Code";
            colCode.Width = 110;
            //
            // colName
            //
            colName.DataPropertyName = "Name";
            colName.HeaderText = "Block Name";
            colName.MinimumWidth = 6;
            colName.Name = "Name";
            colName.Width = 240;
            //
            // colType
            //
            colType.DataPropertyName = "Type";
            colType.FlatStyle = FlatStyle.Flat;
            colType.HeaderText = "Block Type";
            colType.Items.AddRange(new object[] { "Residential", "Commercial", "Mixed Use", "Open Space", "Institutional", "Utility", "Other" });
            colType.MinimumWidth = 6;
            colType.Name = "Type";
            colType.Width = 150;
            //
            // colDepth
            //
            colDepth.DataPropertyName = "Depth";
            colDepth.HeaderText = "Depth (m)";
            colDepth.MinimumWidth = 6;
            colDepth.Name = "Depth";
            colDepth.Width = 110;
            //
            // colLength
            //
            colLength.DataPropertyName = "BlockLength";
            colLength.HeaderText = "Length (m)";
            colLength.MinimumWidth = 6;
            colLength.Name = "BlockLength";
            colLength.Width = 110;
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(lblStatus);
            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 405);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(6);
            pnlFooter.Size = new Size(732, 48);
            pnlFooter.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = SystemColors.GrayText;
            lblStatus.Location = new Point(12, 11);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 20);
            lblStatus.TabIndex = 0;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Location = new Point(651, 6);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 33);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmDefineBlocks
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(732, 453);
            Controls.Add(dgvBlocks);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(720, 420);
            Name = "frmDefineBlocks";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Define Blocks";
            Load += frmDefineBlocks_Load;
            pnlToolbar.ResumeLayout(false);
            pnlToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBlocks).EndInit();
            pnlFooter.ResumeLayout(false);
            pnlFooter.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlToolbar;
        private Button btnAdd;
        private Button btnDuplicate;
        private Button btnDetails;
        private Button btnDelete;
        private Button btnRefresh;
        private Label lblSearch;
        private TextBox txtSearch;
        private DataGridView dgvBlocks;
        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewComboBoxColumn colType;
        private DataGridViewTextBoxColumn colDepth;
        private DataGridViewTextBoxColumn colLength;
        private Panel pnlFooter;
        private Label lblStatus;
        private Button btnClose;
    }
}
