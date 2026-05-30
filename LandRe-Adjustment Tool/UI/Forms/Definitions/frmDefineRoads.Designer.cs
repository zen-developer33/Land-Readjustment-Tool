namespace Land_Readjustment_Tool.UI.Forms.Definitions
{
    partial class frmDefineRoads
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
            dgvRoads = new DataGridView();
            pnlFooter = new Panel();
            lblStatus = new Label();
            btnClose = new Button();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRoads).BeginInit();
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
            btnAdd.Location = new Point(6, 6);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(70, 29);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDuplicate
            // 
            btnDuplicate.Location = new Point(79, 6);
            btnDuplicate.Name = "btnDuplicate";
            btnDuplicate.Size = new Size(90, 29);
            btnDuplicate.TabIndex = 1;
            btnDuplicate.Text = "Duplicate";
            btnDuplicate.UseVisualStyleBackColor = true;
            btnDuplicate.Click += btnDuplicate_Click;
            // 
            // btnDetails
            // 
            btnDetails.Location = new Point(172, 6);
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
            btnDelete.Location = new Point(250, 6);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 29);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(328, 6);
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
            // dgvRoads
            // 
            dgvRoads.AllowUserToAddRows = false;
            dgvRoads.AllowUserToDeleteRows = false;
            dgvRoads.AllowUserToResizeRows = false;
            dgvRoads.BackgroundColor = SystemColors.Window;
            dgvRoads.BorderStyle = BorderStyle.None;
            dgvRoads.ColumnHeadersHeight = 29;
            dgvRoads.Dock = DockStyle.Fill;
            dgvRoads.Font = new Font("Segoe UI", 9F);
            dgvRoads.GridColor = SystemColors.ControlLight;
            dgvRoads.Location = new Point(0, 42);
            dgvRoads.MultiSelect = false;
            dgvRoads.Name = "dgvRoads";
            dgvRoads.RowHeadersVisible = false;
            dgvRoads.RowHeadersWidth = 51;
            dgvRoads.RowTemplate.Height = 26;
            dgvRoads.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoads.Size = new Size(732, 364);
            dgvRoads.TabIndex = 1;
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(lblStatus);
            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 406);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(6);
            pnlFooter.Size = new Size(732, 47);
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
            btnClose.Size = new Size(75, 32);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmDefineRoads
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(732, 453);
            Controls.Add(dgvRoads);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(720, 420);
            Name = "frmDefineRoads";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Define Roads";
            Load += frmDefineRoads_Load;
            pnlToolbar.ResumeLayout(false);
            pnlToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRoads).EndInit();
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
        private DataGridView dgvRoads;
        private Panel pnlFooter;
        private Label lblStatus;
        private Button btnClose;
    }
}
