namespace Land_Readjustment_Tool.Forms
{
    partial class frmOwnerLookup
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblSearch;
        private TextBox txtSearch;
        private DataGridView dgvOwners;
        private Button btnLoad;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblSearch = new Label();
            txtSearch = new TextBox();
            dgvOwners = new DataGridView();
            btnLoad = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvOwners).BeginInit();
            SuspendLayout();
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(12, 15);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(45, 15);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(70, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(300, 23);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            // 
            // dgvOwners
            // 
            dgvOwners.AllowUserToAddRows = false;
            dgvOwners.AllowUserToDeleteRows = false;
            dgvOwners.AutoGenerateColumns = false;
            dgvOwners.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOwners.Columns.AddRange(
                new DataGridViewTextBoxColumn { Name = "LandOwnerId", HeaderText = "ID", DataPropertyName = "LandOwnerId", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "LandOwnersName", HeaderText = "Owner Name", DataPropertyName = "LandOwnersName", Width = 180 },
                new DataGridViewTextBoxColumn { Name = "FatherSpouse", HeaderText = "Father/Spouse", DataPropertyName = "FatherSpouse", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "CitizenshipNumber", HeaderText = "Citizenship No", DataPropertyName = "CitizenshipNumber", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "PermanentAddress", HeaderText = "Address", DataPropertyName = "PermanentAddress", Width = 150 });
            dgvOwners.Location = new Point(12, 45);
            dgvOwners.MultiSelect = false;
            dgvOwners.Name = "dgvOwners";
            dgvOwners.ReadOnly = true;
            dgvOwners.RowHeadersVisible = false;
            dgvOwners.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOwners.Size = new Size(560, 320);
            dgvOwners.TabIndex = 2;
            dgvOwners.CellDoubleClick += DgvOwners_CellDoubleClick;
            dgvOwners.SelectionChanged += DgvOwners_SelectionChanged;
            // 
            // btnLoad
            // 
            btnLoad.Enabled = false;
            btnLoad.Location = new Point(400, 375);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(80, 28);
            btnLoad.TabIndex = 3;
            btnLoad.Text = "Load";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += BtnLoad_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(490, 375);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 28);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // frmOwnerLookup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 450);
            Controls.Add(lblSearch);
            Controls.Add(txtSearch);
            Controls.Add(dgvOwners);
            Controls.Add(btnLoad);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmOwnerLookup";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select Land Owner";
            ((System.ComponentModel.ISupportInitialize)dgvOwners).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
