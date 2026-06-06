namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmPolicyLookupTablesWindow
    {
        private System.ComponentModel.IContainer components = null;
        private ToolStrip toolStrip;
        private ToolStripLabel lblPolicy;
        private ToolStripComboBox cboPolicies;
        private ToolStripLabel lblTable;
        private ToolStripComboBox cboTables;
        private ToolStripButton btnAddRow;
        private ToolStripButton btnDeleteRow;
        private ToolStripButton btnRefresh;
        private Panel headerPanel;
        private Label lblClause;
        private ComboBox cboClauses;
        private Label lblDescription;
        private DataGridView dgvLookup;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            toolStrip = new ToolStrip();
            lblPolicy = new ToolStripLabel();
            cboPolicies = new ToolStripComboBox();
            lblTable = new ToolStripLabel();
            cboTables = new ToolStripComboBox();
            btnAddRow = new ToolStripButton();
            btnDeleteRow = new ToolStripButton();
            btnRefresh = new ToolStripButton();
            headerPanel = new Panel();
            lblClause = new Label();
            cboClauses = new ComboBox();
            lblDescription = new Label();
            dgvLookup = new DataGridView();
            toolStrip.SuspendLayout();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLookup).BeginInit();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.Items.AddRange(new ToolStripItem[] { lblPolicy, cboPolicies, new ToolStripSeparator(), lblTable, cboTables, new ToolStripSeparator(), btnAddRow, btnDeleteRow, btnRefresh });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(1120, 28);
            toolStrip.TabIndex = 0;
            // 
            // lblPolicy
            // 
            lblPolicy.Name = "lblPolicy";
            lblPolicy.Size = new Size(52, 25);
            lblPolicy.Text = "Policy:";
            // 
            // cboPolicies
            // 
            cboPolicies.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPolicies.Name = "cboPolicies";
            cboPolicies.Size = new Size(330, 28);
            cboPolicies.SelectedIndexChanged += cboPolicies_SelectedIndexChanged;
            // 
            // lblTable
            // 
            lblTable.Name = "lblTable";
            lblTable.Size = new Size(47, 25);
            lblTable.Text = "Table:";
            // 
            // cboTables
            // 
            cboTables.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTables.Name = "cboTables";
            cboTables.Size = new Size(300, 28);
            cboTables.SelectedIndexChanged += cboTables_SelectedIndexChanged;
            // 
            // btnAddRow
            // 
            btnAddRow.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnAddRow.Name = "btnAddRow";
            btnAddRow.Size = new Size(72, 25);
            btnAddRow.Text = "Add Row";
            btnAddRow.Click += btnAddRow_Click;
            // 
            // btnDeleteRow
            // 
            btnDeleteRow.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDeleteRow.Name = "btnDeleteRow";
            btnDeleteRow.Size = new Size(87, 25);
            btnDeleteRow.Text = "Delete Row";
            btnDeleteRow.Click += btnDeleteRow_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(64, 25);
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // headerPanel
            // 
            headerPanel.Controls.Add(lblDescription);
            headerPanel.Controls.Add(cboClauses);
            headerPanel.Controls.Add(lblClause);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 28);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(10, 8, 10, 8);
            headerPanel.Size = new Size(1120, 72);
            headerPanel.TabIndex = 1;
            // 
            // lblClause
            // 
            lblClause.AutoSize = true;
            lblClause.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClause.Location = new Point(12, 13);
            lblClause.Name = "lblClause";
            lblClause.Size = new Size(113, 20);
            lblClause.TabIndex = 0;
            lblClause.Text = "Linked Clause:";
            // 
            // cboClauses
            // 
            cboClauses.DropDownStyle = ComboBoxStyle.DropDownList;
            cboClauses.Location = new Point(132, 10);
            cboClauses.Name = "cboClauses";
            cboClauses.Size = new Size(420, 28);
            cboClauses.TabIndex = 1;
            cboClauses.SelectedIndexChanged += cboClauses_SelectedIndexChanged;
            // 
            // lblDescription
            // 
            lblDescription.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            lblDescription.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblDescription.Location = new Point(12, 43);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(1090, 20);
            lblDescription.TabIndex = 2;
            lblDescription.Text = "Select a lookup table.";
            // 
            // dgvLookup
            // 
            dgvLookup.AllowUserToAddRows = false;
            dgvLookup.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvLookup.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLookup.Dock = DockStyle.Fill;
            dgvLookup.Location = new Point(0, 100);
            dgvLookup.MultiSelect = false;
            dgvLookup.Name = "dgvLookup";
            dgvLookup.RowHeadersWidth = 35;
            dgvLookup.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLookup.Size = new Size(1120, 540);
            dgvLookup.TabIndex = 2;
            dgvLookup.CellEndEdit += dgvLookup_CellEndEdit;
            // 
            // frmPolicyLookupTablesWindow
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1120, 640);
            Controls.Add(dgvLookup);
            Controls.Add(headerPanel);
            Controls.Add(toolStrip);
            MinimumSize = new Size(940, 540);
            Name = "frmPolicyLookupTablesWindow";
            Text = "Policy Lookup Tables";
            Load += frmPolicyLookupTablesWindow_Load;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLookup).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
