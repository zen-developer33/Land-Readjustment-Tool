namespace Land_Pooling_Policy_Manager.UI.Forms
{
    partial class frmPolicyListManager
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layout;
        private ListBox lstPolicies;
        private TextBox txtPolicyName;
        private FlowLayoutPanel buttonPanel;
        private Button btnSelect;
        private Button btnNew;
        private Button btnCopy;
        private Button btnDraftFromApproved;
        private Button btnRename;
        private Button btnDelete;
        private Button btnImport;
        private Button btnExport;
        private Button btnClose;
        private Label lblName;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            layout = new TableLayoutPanel();
            lstPolicies = new ListBox();
            lblName = new Label();
            txtPolicyName = new TextBox();
            buttonPanel = new FlowLayoutPanel();
            btnSelect = new Button();
            btnNew = new Button();
            btnCopy = new Button();
            btnDraftFromApproved = new Button();
            btnRename = new Button();
            btnDelete = new Button();
            btnImport = new Button();
            btnExport = new Button();
            btnClose = new Button();
            layout.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 148F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Controls.Add(lstPolicies, 0, 0);
            layout.Controls.Add(lblName, 0, 1);
            layout.Controls.Add(txtPolicyName, 1, 1);
            layout.Controls.Add(buttonPanel, 0, 2);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Name = "layout";
            layout.Padding = new Padding(10);
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            layout.Size = new Size(862, 441);
            layout.TabIndex = 0;
            // 
            // lstPolicies
            // 
            layout.SetColumnSpan(lstPolicies, 2);
            lstPolicies.Dock = DockStyle.Fill;
            lstPolicies.Font = new Font("Segoe UI", 9F);
            lstPolicies.IntegralHeight = false;
            lstPolicies.Location = new Point(13, 13);
            lstPolicies.Name = "lstPolicies";
            lstPolicies.Size = new Size(750, 332);
            lstPolicies.TabIndex = 0;
            lstPolicies.SelectedIndexChanged += lstPolicies_SelectedIndexChanged;
            lstPolicies.DoubleClick += lstPolicies_DoubleClick;
            // 
            // lblName
            // 
            lblName.Dock = DockStyle.Fill;
            lblName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblName.Location = new Point(13, 348);
            lblName.Name = "lblName";
            lblName.Size = new Size(142, 31);
            lblName.TabIndex = 1;
            lblName.Text = "Policy Name:";
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtPolicyName
            // 
            txtPolicyName.Dock = DockStyle.Fill;
            txtPolicyName.Location = new Point(161, 351);
            txtPolicyName.Name = "txtPolicyName";
            txtPolicyName.Size = new Size(602, 27);
            txtPolicyName.TabIndex = 2;
            // 
            // buttonPanel
            // 
            layout.SetColumnSpan(buttonPanel, 2);
            buttonPanel.Controls.Add(btnSelect);
            buttonPanel.Controls.Add(btnNew);
            buttonPanel.Controls.Add(btnCopy);
            buttonPanel.Controls.Add(btnDraftFromApproved);
            buttonPanel.Controls.Add(btnRename);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnExport);
            buttonPanel.Controls.Add(btnClose);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Location = new Point(13, 382);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(750, 46);
            buttonPanel.TabIndex = 3;
            buttonPanel.WrapContents = false;
            // 
            // btnSelect
            // 
            btnSelect.AutoSize = true;
            btnSelect.Location = new Point(3, 3);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new Size(75, 39);
            btnSelect.TabIndex = 0;
            btnSelect.Text = "Select";
            btnSelect.Click += btnSelect_Click;
            // 
            // btnNew
            // 
            btnNew.AutoSize = true;
            btnNew.Location = new Point(84, 3);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(75, 39);
            btnNew.TabIndex = 1;
            btnNew.Text = "New";
            btnNew.Click += btnNew_Click;
            // 
            // btnCopy
            // 
            btnCopy.AutoSize = true;
            btnCopy.Location = new Point(165, 3);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(75, 39);
            btnCopy.TabIndex = 2;
            btnCopy.Text = "Copy";
            btnCopy.Click += btnCopy_Click;
            // 
            // btnDraftFromApproved
            // 
            btnDraftFromApproved.AutoSize = true;
            btnDraftFromApproved.Location = new Point(246, 3);
            btnDraftFromApproved.Name = "btnDraftFromApproved";
            btnDraftFromApproved.Size = new Size(161, 39);
            btnDraftFromApproved.TabIndex = 3;
            btnDraftFromApproved.Text = "Draft From Approved";
            btnDraftFromApproved.Click += btnDraftFromApproved_Click;
            // 
            // btnRename
            // 
            btnRename.AutoSize = true;
            btnRename.Location = new Point(413, 3);
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(75, 39);
            btnRename.TabIndex = 4;
            btnRename.Text = "Rename";
            btnRename.Click += btnRename_Click;
            // 
            // btnDelete
            // 
            btnDelete.AutoSize = true;
            btnDelete.Location = new Point(494, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 39);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "Delete";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnImport
            // 
            btnImport.AutoSize = true;
            btnImport.Location = new Point(575, 3);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(75, 39);
            btnImport.TabIndex = 6;
            btnImport.Text = "Import";
            btnImport.Click += btnImport_Click;
            //
            // btnExport
            //
            btnExport.AutoSize = true;
            btnExport.Location = new Point(656, 3);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(75, 39);
            btnExport.TabIndex = 7;
            btnExport.Text = "Export";
            btnExport.Click += btnExport_Click;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.Location = new Point(737, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 39);
            btnClose.TabIndex = 8;
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;
            // 
            // frmPolicyListManager
            // 
            AcceptButton = btnSelect;
            AutoScaleMode = AutoScaleMode.None;
            CancelButton = btnClose;
            ClientSize = new Size(862, 441);
            Controls.Add(layout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmPolicyListManager";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select / Manage Policies";
            Load += frmPolicyListManager_Load;
            layout.ResumeLayout(false);
            layout.PerformLayout();
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
