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
        private Panel pnlTop;
        private Panel pnlActions;
        private Panel pnlDetails;
        private PictureBox picOwner;
        private Label lblPreviewTitle;
        private Label lblPrimaryCaption;
        private Label lblPrimaryValue;
        private Label lblFatherCaption;
        private Label lblFatherValue;
        private Label lblCitizenshipCaption;
        private Label lblCitizenshipValue;
        private Label lblAddressCaption;
        private Label lblAddressValue;
        private Label lblCoOwnerCount;

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
            pnlTop = new Panel();
            pnlActions = new Panel();
            pnlDetails = new Panel();
            lblCoOwnerCount = new Label();
            lblAddressValue = new Label();
            lblAddressCaption = new Label();
            lblCitizenshipValue = new Label();
            lblCitizenshipCaption = new Label();
            lblFatherValue = new Label();
            lblFatherCaption = new Label();
            lblPrimaryValue = new Label();
            lblPrimaryCaption = new Label();
            lblPreviewTitle = new Label();
            picOwner = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)dgvOwners).BeginInit();
            pnlTop.SuspendLayout();
            pnlActions.SuspendLayout();
            pnlDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picOwner).BeginInit();
            SuspendLayout();
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(14, 17);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(56, 20);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Location = new Point(78, 13);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Name, father/spouse, citizenship, address";
            txtSearch.Size = new Size(582, 27);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            // 
            // dgvOwners
            // 
            dgvOwners.AllowUserToAddRows = false;
            dgvOwners.AllowUserToDeleteRows = false;
            dgvOwners.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvOwners.AutoGenerateColumns = false;
            dgvOwners.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOwners.Columns.AddRange(
                new DataGridViewCheckBoxColumn { Name = "IsCoOwner", HeaderText = "", DataPropertyName = "IsCoOwner", Width = 34 },
                new DataGridViewTextBoxColumn { Name = "LandOwnerId", HeaderText = "ID", DataPropertyName = "LandOwnerId", Width = 55, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "LandOwnersName", HeaderText = "Owner Name", DataPropertyName = "LandOwnersName", Width = 190, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "FatherSpouse", HeaderText = "Father/Spouse", DataPropertyName = "FatherSpouse", Width = 145, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "CitizenshipNumber", HeaderText = "Citizenship No", DataPropertyName = "CitizenshipNumber", Width = 125, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "PermanentAddress", HeaderText = "Address", DataPropertyName = "PermanentAddress", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvOwners.Location = new Point(14, 60);
            dgvOwners.MultiSelect = false;
            dgvOwners.Name = "dgvOwners";
            dgvOwners.RowHeadersVisible = false;
            dgvOwners.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOwners.Size = new Size(646, 438);
            dgvOwners.TabIndex = 2;
            dgvOwners.CellDoubleClick += DgvOwners_CellDoubleClick;
            dgvOwners.CellValueChanged += DgvOwners_CellValueChanged;
            dgvOwners.CurrentCellDirtyStateChanged += DgvOwners_CurrentCellDirtyStateChanged;
            dgvOwners.SelectionChanged += DgvOwners_SelectionChanged;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoad.Enabled = false;
            btnLoad.Location = new Point(734, 13);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(122, 34);
            btnLoad.TabIndex = 3;
            btnLoad.Text = "Load Selection";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += BtnLoad_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(866, 13);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(94, 34);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblSearch);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(984, 54);
            pnlTop.TabIndex = 5;
            // 
            // pnlActions
            // 
            pnlActions.Controls.Add(btnLoad);
            pnlActions.Controls.Add(btnCancel);
            pnlActions.Dock = DockStyle.Bottom;
            pnlActions.Location = new Point(0, 564);
            pnlActions.Name = "pnlActions";
            pnlActions.Size = new Size(984, 60);
            pnlActions.TabIndex = 6;
            // 
            // pnlDetails
            // 
            pnlDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            pnlDetails.BorderStyle = BorderStyle.FixedSingle;
            pnlDetails.Controls.Add(lblCoOwnerCount);
            pnlDetails.Controls.Add(lblAddressValue);
            pnlDetails.Controls.Add(lblAddressCaption);
            pnlDetails.Controls.Add(lblCitizenshipValue);
            pnlDetails.Controls.Add(lblCitizenshipCaption);
            pnlDetails.Controls.Add(lblFatherValue);
            pnlDetails.Controls.Add(lblFatherCaption);
            pnlDetails.Controls.Add(lblPrimaryValue);
            pnlDetails.Controls.Add(lblPrimaryCaption);
            pnlDetails.Controls.Add(lblPreviewTitle);
            pnlDetails.Controls.Add(picOwner);
            pnlDetails.Location = new Point(674, 60);
            pnlDetails.Name = "pnlDetails";
            pnlDetails.Size = new Size(296, 438);
            pnlDetails.TabIndex = 7;
            // 
            // lblCoOwnerCount
            // 
            lblCoOwnerCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblCoOwnerCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCoOwnerCount.Location = new Point(14, 398);
            lblCoOwnerCount.Name = "lblCoOwnerCount";
            lblCoOwnerCount.Size = new Size(266, 23);
            lblCoOwnerCount.TabIndex = 10;
            lblCoOwnerCount.Text = "0 co-owner(s) checked";
            // 
            // lblAddressValue
            // 
            lblAddressValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblAddressValue.Location = new Point(14, 341);
            lblAddressValue.Name = "lblAddressValue";
            lblAddressValue.Size = new Size(266, 50);
            lblAddressValue.TabIndex = 9;
            lblAddressValue.Text = "-";
            // 
            // lblAddressCaption
            // 
            lblAddressCaption.AutoSize = true;
            lblAddressCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAddressCaption.Location = new Point(14, 318);
            lblAddressCaption.Name = "lblAddressCaption";
            lblAddressCaption.Size = new Size(69, 20);
            lblAddressCaption.TabIndex = 8;
            lblAddressCaption.Text = "Address";
            // 
            // lblCitizenshipValue
            // 
            lblCitizenshipValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCitizenshipValue.Location = new Point(14, 286);
            lblCitizenshipValue.Name = "lblCitizenshipValue";
            lblCitizenshipValue.Size = new Size(266, 23);
            lblCitizenshipValue.TabIndex = 7;
            lblCitizenshipValue.Text = "-";
            // 
            // lblCitizenshipCaption
            // 
            lblCitizenshipCaption.AutoSize = true;
            lblCitizenshipCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCitizenshipCaption.Location = new Point(14, 263);
            lblCitizenshipCaption.Name = "lblCitizenshipCaption";
            lblCitizenshipCaption.Size = new Size(86, 20);
            lblCitizenshipCaption.TabIndex = 6;
            lblCitizenshipCaption.Text = "Citizenship";
            // 
            // lblFatherValue
            // 
            lblFatherValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFatherValue.Location = new Point(14, 231);
            lblFatherValue.Name = "lblFatherValue";
            lblFatherValue.Size = new Size(266, 23);
            lblFatherValue.TabIndex = 5;
            lblFatherValue.Text = "-";
            // 
            // lblFatherCaption
            // 
            lblFatherCaption.AutoSize = true;
            lblFatherCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFatherCaption.Location = new Point(14, 208);
            lblFatherCaption.Name = "lblFatherCaption";
            lblFatherCaption.Size = new Size(114, 20);
            lblFatherCaption.TabIndex = 4;
            lblFatherCaption.Text = "Father/Spouse";
            // 
            // lblPrimaryValue
            // 
            lblPrimaryValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPrimaryValue.Location = new Point(14, 176);
            lblPrimaryValue.Name = "lblPrimaryValue";
            lblPrimaryValue.Size = new Size(266, 23);
            lblPrimaryValue.TabIndex = 3;
            lblPrimaryValue.Text = "-";
            // 
            // lblPrimaryCaption
            // 
            lblPrimaryCaption.AutoSize = true;
            lblPrimaryCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPrimaryCaption.Location = new Point(14, 153);
            lblPrimaryCaption.Name = "lblPrimaryCaption";
            lblPrimaryCaption.Size = new Size(113, 20);
            lblPrimaryCaption.TabIndex = 2;
            lblPrimaryCaption.Text = "Primary Owner";
            // 
            // lblPreviewTitle
            // 
            lblPreviewTitle.AutoSize = true;
            lblPreviewTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPreviewTitle.Location = new Point(14, 12);
            lblPreviewTitle.Name = "lblPreviewTitle";
            lblPreviewTitle.Size = new Size(108, 23);
            lblPreviewTitle.TabIndex = 1;
            lblPreviewTitle.Text = "Owner Card";
            // 
            // picOwner
            // 
            picOwner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            picOwner.BackColor = Color.WhiteSmoke;
            picOwner.BackgroundImage = Properties.Resources.Portrait_Placeholder1;
            picOwner.BackgroundImageLayout = ImageLayout.Zoom;
            picOwner.BorderStyle = BorderStyle.FixedSingle;
            picOwner.Location = new Point(91, 45);
            picOwner.Name = "picOwner";
            picOwner.Size = new Size(112, 96);
            picOwner.SizeMode = PictureBoxSizeMode.Zoom;
            picOwner.TabIndex = 0;
            picOwner.TabStop = false;
            // 
            // frmOwnerLookup
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 624);
            Controls.Add(pnlDetails);
            Controls.Add(dgvOwners);
            Controls.Add(pnlActions);
            Controls.Add(pnlTop);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmOwnerLookup";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select Primary Owner and Co-Owners";
            ((System.ComponentModel.ISupportInitialize)dgvOwners).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlActions.ResumeLayout(false);
            pnlDetails.ResumeLayout(false);
            pnlDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picOwner).EndInit();
            ResumeLayout(false);
        }
    }
}
