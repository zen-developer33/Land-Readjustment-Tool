namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmPolicyParametersWindow
    {
        private System.ComponentModel.IContainer components = null;
        private ToolStrip toolStrip;
        private ToolStripLabel lblPolicy;
        private ToolStripComboBox cboPolicies;
        private ToolStripButton btnAddParameter;
        private ToolStripButton btnDeleteParameter;
        private ToolStripButton btnRefresh;
        private ToolStripLabel lblEditHint;
        private DataGridView dgvParameters;
        private DataGridViewTextBoxColumn colClause;
        private DataGridViewTextBoxColumn colKey;
        private DataGridViewTextBoxColumn colLabel;
        private DataGridViewTextBoxColumn colValue;
        private DataGridViewTextBoxColumn colUnit;
        private DataGridViewTextBoxColumn colType;
        private DataGridViewTextBoxColumn colDescription;

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
            btnAddParameter = new ToolStripButton();
            btnDeleteParameter = new ToolStripButton();
            btnRefresh = new ToolStripButton();
            lblEditHint = new ToolStripLabel();
            dgvParameters = new DataGridView();
            colClause = new DataGridViewTextBoxColumn();
            colKey = new DataGridViewTextBoxColumn();
            colLabel = new DataGridViewTextBoxColumn();
            colValue = new DataGridViewTextBoxColumn();
            colUnit = new DataGridViewTextBoxColumn();
            colType = new DataGridViewTextBoxColumn();
            colDescription = new DataGridViewTextBoxColumn();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParameters).BeginInit();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.Items.AddRange(new ToolStripItem[] { lblPolicy, cboPolicies, new ToolStripSeparator(), btnAddParameter, btnDeleteParameter, btnRefresh, new ToolStripSeparator(), lblEditHint });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(1100, 28);
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
            cboPolicies.Size = new Size(360, 28);
            cboPolicies.SelectedIndexChanged += cboPolicies_SelectedIndexChanged;
            // 
            // btnAddParameter
            // 
            btnAddParameter.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnAddParameter.Name = "btnAddParameter";
            btnAddParameter.Size = new Size(111, 25);
            btnAddParameter.Text = "Add Parameter";
            btnAddParameter.Click += btnAddParameter_Click;
            // 
            // btnDeleteParameter
            // 
            btnDeleteParameter.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDeleteParameter.Name = "btnDeleteParameter";
            btnDeleteParameter.Size = new Size(126, 25);
            btnDeleteParameter.Text = "Delete Parameter";
            btnDeleteParameter.Click += btnDeleteParameter_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(64, 25);
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // lblEditHint
            // 
            lblEditHint.Name = "lblEditHint";
            lblEditHint.Size = new Size(227, 25);
            lblEditHint.Text = "Beige cells are editable draft values";
            // 
            // dgvParameters
            // 
            dgvParameters.AllowUserToAddRows = false;
            dgvParameters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvParameters.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParameters.Columns.AddRange(new DataGridViewColumn[] { colClause, colKey, colLabel, colValue, colUnit, colType, colDescription });
            dgvParameters.Dock = DockStyle.Fill;
            dgvParameters.Location = new Point(0, 28);
            dgvParameters.MultiSelect = false;
            dgvParameters.Name = "dgvParameters";
            dgvParameters.RowHeadersWidth = 35;
            dgvParameters.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParameters.Size = new Size(1100, 592);
            dgvParameters.TabIndex = 1;
            dgvParameters.CellEndEdit += dgvParameters_CellEndEdit;
            // 
            // columns
            // 
            colClause.HeaderText = "Clause";
            colClause.Name = "colClause";
            colClause.ReadOnly = true;
            colClause.SortMode = DataGridViewColumnSortMode.NotSortable;
            colClause.Width = 90;
            colKey.HeaderText = "Key";
            colKey.Name = "colKey";
            colKey.SortMode = DataGridViewColumnSortMode.NotSortable;
            colKey.Width = 170;
            colLabel.HeaderText = "Label";
            colLabel.Name = "colLabel";
            colLabel.SortMode = DataGridViewColumnSortMode.NotSortable;
            colLabel.Width = 220;
            colValue.HeaderText = "Value";
            colValue.Name = "colValue";
            colValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            colValue.Width = 110;
            colUnit.HeaderText = "Unit";
            colUnit.Name = "colUnit";
            colUnit.SortMode = DataGridViewColumnSortMode.NotSortable;
            colUnit.Width = 80;
            colType.HeaderText = "Type";
            colType.Name = "colType";
            colType.SortMode = DataGridViewColumnSortMode.NotSortable;
            colType.Width = 95;
            colDescription.HeaderText = "Description";
            colDescription.Name = "colDescription";
            colDescription.SortMode = DataGridViewColumnSortMode.NotSortable;
            colDescription.Width = 430;
            // 
            // frmPolicyParametersWindow
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1100, 620);
            Controls.Add(dgvParameters);
            Controls.Add(toolStrip);
            MinimumSize = new Size(900, 520);
            Name = "frmPolicyParametersWindow";
            Text = "Policy Parameters";
            Load += frmPolicyParametersWindow_Load;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParameters).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
