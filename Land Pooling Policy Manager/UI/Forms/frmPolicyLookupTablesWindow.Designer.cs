namespace Land_Pooling_Policy_Manager.UI.Forms
{
    partial class frmPolicyLookupTablesWindow
    {
        private System.ComponentModel.IContainer components = null;
        private Panel commandPanel;
        private Label lblPolicy;
        private ComboBox cboPolicies;
        private Label lblTable;
        private ComboBox cboTables;
        private Button btnAddRow;
        private Button btnDeleteRow;
        private Button btnRefresh;
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
            commandPanel = new Panel();
            lblPolicy = new Label();
            cboPolicies = new ComboBox();
            lblTable = new Label();
            cboTables = new ComboBox();
            btnAddRow = new Button();
            btnDeleteRow = new Button();
            btnRefresh = new Button();
            headerPanel = new Panel();
            lblClause = new Label();
            cboClauses = new ComboBox();
            lblDescription = new Label();
            dgvLookup = new DataGridView();
            commandPanel.SuspendLayout();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLookup).BeginInit();
            SuspendLayout();
            //
            // commandPanel
            //
            commandPanel.BorderStyle = BorderStyle.FixedSingle;
            commandPanel.Controls.Add(lblPolicy);
            commandPanel.Controls.Add(cboPolicies);
            commandPanel.Controls.Add(lblTable);
            commandPanel.Controls.Add(cboTables);
            commandPanel.Controls.Add(btnAddRow);
            commandPanel.Controls.Add(btnDeleteRow);
            commandPanel.Controls.Add(btnRefresh);
            commandPanel.Dock = DockStyle.Top;
            commandPanel.Height = 40;
            commandPanel.Name = "commandPanel";
            commandPanel.Padding = new Padding(8, 6, 8, 6);
            commandPanel.TabIndex = 0;
            //
            // lblPolicy
            //
            lblPolicy.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPolicy.Location = new Point(8, 8);
            lblPolicy.Name = "lblPolicy";
            lblPolicy.Size = new Size(54, 24);
            lblPolicy.TabIndex = 0;
            lblPolicy.Text = "Policy:";
            lblPolicy.TextAlign = ContentAlignment.MiddleLeft;
            //
            // cboPolicies
            //
            cboPolicies.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPolicies.Location = new Point(66, 6);
            cboPolicies.Name = "cboPolicies";
            cboPolicies.Size = new Size(330, 28);
            cboPolicies.TabIndex = 1;
            cboPolicies.SelectedIndexChanged += cboPolicies_SelectedIndexChanged;
            //
            // lblTable
            //
            lblTable.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTable.Location = new Point(408, 8);
            lblTable.Name = "lblTable";
            lblTable.Size = new Size(52, 24);
            lblTable.TabIndex = 2;
            lblTable.Text = "Table:";
            lblTable.TextAlign = ContentAlignment.MiddleLeft;
            //
            // cboTables
            //
            cboTables.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTables.Location = new Point(464, 6);
            cboTables.Name = "cboTables";
            cboTables.Size = new Size(300, 28);
            cboTables.TabIndex = 3;
            cboTables.SelectedIndexChanged += cboTables_SelectedIndexChanged;
            //
            // btnAddRow
            //
            btnAddRow.Location = new Point(776, 6);
            btnAddRow.Name = "btnAddRow";
            btnAddRow.Size = new Size(82, 28);
            btnAddRow.TabIndex = 4;
            btnAddRow.Text = "Add Row";
            btnAddRow.UseVisualStyleBackColor = true;
            btnAddRow.Click += btnAddRow_Click;
            //
            // btnDeleteRow
            //
            btnDeleteRow.Location = new Point(864, 6);
            btnDeleteRow.Name = "btnDeleteRow";
            btnDeleteRow.Size = new Size(94, 28);
            btnDeleteRow.TabIndex = 5;
            btnDeleteRow.Text = "Delete Row";
            btnDeleteRow.UseVisualStyleBackColor = true;
            btnDeleteRow.Click += btnDeleteRow_Click;
            //
            // btnRefresh
            //
            btnRefresh.Location = new Point(964, 6);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(76, 28);
            btnRefresh.TabIndex = 6;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            //
            // headerPanel
            //
            headerPanel.Controls.Add(lblDescription);
            headerPanel.Controls.Add(cboClauses);
            headerPanel.Controls.Add(lblClause);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 40);
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
            dgvLookup.Location = new Point(0, 112);
            dgvLookup.MultiSelect = false;
            dgvLookup.Name = "dgvLookup";
            dgvLookup.RowHeadersWidth = 35;
            dgvLookup.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLookup.Size = new Size(1120, 528);
            dgvLookup.TabIndex = 2;
            dgvLookup.CellEndEdit += dgvLookup_CellEndEdit;
            //
            // frmPolicyLookupTablesWindow
            //
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1120, 640);
            Controls.Add(dgvLookup);
            Controls.Add(headerPanel);
            Controls.Add(commandPanel);
            MinimumSize = new Size(0, 0);
            Name = "frmPolicyLookupTablesWindow";
            Text = "Policy Lookup Tables";
            Load += frmPolicyLookupTablesWindow_Load;
            commandPanel.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLookup).EndInit();
            ResumeLayout(false);
        }
    }
}
