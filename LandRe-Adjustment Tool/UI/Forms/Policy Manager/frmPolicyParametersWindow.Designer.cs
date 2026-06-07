namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmPolicyParametersWindow
    {
        private System.ComponentModel.IContainer components = null;
        private Panel headerPanel;
        private Label lblPolicy;
        private ComboBox cboPolicies;
        private Button btnAddParameter;
        private Button btnDeleteParameter;
        private Button btnRefresh;
        private DataGridView dgvParameters;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            headerPanel = new Panel();
            lblPolicy = new Label();
            cboPolicies = new ComboBox();
            btnAddParameter = new Button();
            btnDeleteParameter = new Button();
            btnRefresh = new Button();
            dgvParameters = new DataGridView();
            colClause = new DataGridViewTextBoxColumn();
            colLabel = new DataGridViewTextBoxColumn();
            colValue = new DataGridViewTextBoxColumn();
            colUnit = new DataGridViewTextBoxColumn();
            colDescription = new DataGridViewTextBoxColumn();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParameters).BeginInit();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BorderStyle = BorderStyle.FixedSingle;
            headerPanel.Controls.Add(lblPolicy);
            headerPanel.Controls.Add(cboPolicies);
            headerPanel.Controls.Add(btnAddParameter);
            headerPanel.Controls.Add(btnDeleteParameter);
            headerPanel.Controls.Add(btnRefresh);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(8, 6, 8, 6);
            headerPanel.Size = new Size(1100, 40);
            headerPanel.TabIndex = 0;
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
            cboPolicies.Size = new Size(360, 28);
            cboPolicies.TabIndex = 1;
            cboPolicies.SelectedIndexChanged += cboPolicies_SelectedIndexChanged;
            // 
            // btnAddParameter
            // 
            btnAddParameter.Location = new Point(438, 6);
            btnAddParameter.Name = "btnAddParameter";
            btnAddParameter.Size = new Size(118, 28);
            btnAddParameter.TabIndex = 2;
            btnAddParameter.Text = "Add Parameter";
            btnAddParameter.UseVisualStyleBackColor = true;
            btnAddParameter.Click += btnAddParameter_Click;
            // 
            // btnDeleteParameter
            // 
            btnDeleteParameter.Location = new Point(562, 6);
            btnDeleteParameter.Name = "btnDeleteParameter";
            btnDeleteParameter.Size = new Size(132, 28);
            btnDeleteParameter.TabIndex = 3;
            btnDeleteParameter.Text = "Delete Parameter";
            btnDeleteParameter.UseVisualStyleBackColor = true;
            btnDeleteParameter.Click += btnDeleteParameter_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(700, 6);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(76, 28);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // dgvParameters
            // 
            dgvParameters.AllowUserToAddRows = false;
            dgvParameters.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParameters.Columns.AddRange(new DataGridViewColumn[] { colClause, colLabel, colValue, colUnit, colDescription });
            dgvParameters.Dock = DockStyle.Fill;
            dgvParameters.Location = new Point(0, 40);
            dgvParameters.MultiSelect = false;
            dgvParameters.Name = "dgvParameters";
            dgvParameters.RowHeadersWidth = 35;
            dgvParameters.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParameters.Size = new Size(1100, 580);
            dgvParameters.TabIndex = 1;
            dgvParameters.CellEndEdit += dgvParameters_CellEndEdit;
            dgvParameters.SelectionChanged += dgvParameters_SelectionChanged;
            // 
            // colClause
            // 
            colClause.HeaderText = "Ref. Clause";
            colClause.MinimumWidth = 6;
            colClause.Name = "colClause";
            colClause.ReadOnly = true;
            colClause.SortMode = DataGridViewColumnSortMode.NotSortable;
            colClause.Width = 120;
            // 
            // colLabel
            // 
            colLabel.HeaderText = "Parameter";
            colLabel.MinimumWidth = 6;
            colLabel.Name = "colLabel";
            colLabel.SortMode = DataGridViewColumnSortMode.NotSortable;
            colLabel.Width = 260;
            // 
            // colValue
            // 
            colValue.HeaderText = "Value";
            colValue.MinimumWidth = 6;
            colValue.Name = "colValue";
            colValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            colValue.Width = 130;
            // 
            // colUnit
            // 
            colUnit.HeaderText = "Unit";
            colUnit.MinimumWidth = 6;
            colUnit.Name = "colUnit";
            colUnit.SortMode = DataGridViewColumnSortMode.NotSortable;
            colUnit.Width = 80;
            // 
            // colDescription
            // 
            colDescription.HeaderText = "Description";
            colDescription.MinimumWidth = 6;
            colDescription.Name = "colDescription";
            colDescription.SortMode = DataGridViewColumnSortMode.NotSortable;
            colDescription.Width = 520;
            // 
            // frmPolicyParametersWindow
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1100, 620);
            Controls.Add(dgvParameters);
            Controls.Add(headerPanel);
            MinimumSize = new Size(900, 520);
            Name = "frmPolicyParametersWindow";
            Text = "Policy Parameters";
            Load += frmPolicyParametersWindow_Load;
            headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvParameters).EndInit();
            ResumeLayout(false);
        }

        private DataGridViewTextBoxColumn colClause;
        private DataGridViewTextBoxColumn colLabel;
        private DataGridViewTextBoxColumn colValue;
        private DataGridViewTextBoxColumn colUnit;
        private DataGridViewTextBoxColumn colDescription;
    }
}
