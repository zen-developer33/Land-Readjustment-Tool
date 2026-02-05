namespace Land_Readjustment_Tool.Forms
{
    partial class frmLandownerRecordsFilterView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle11 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle12 = new DataGridViewCellStyle();
            pnlHeader = new Panel();
            lblFormTitle = new Label();
            picLogo = new PictureBox();
            pnlFilters = new Panel();
            grpFilterByMapSheet = new GroupBox();
            cboMapSheet = new ComboBox();
            lblMapSheet = new Label();
            chkAllMapSheets = new CheckBox();
            grpSearchFields = new GroupBox();
            txtParcelNo = new TextBox();
            lblParcelNo = new Label();
            txtOwnerName = new TextBox();
            lblOwnerName = new Label();
            txtCitizenshipNo = new TextBox();
            lblCitizenshipNo = new Label();
            grpQuickFilters = new GroupBox();
            cboDistrict = new ComboBox();
            lblDistrict = new Label();
            cboLandUse = new ComboBox();
            lblLandUse = new Label();
            btnApplyFilters = new Button();
            btnClearFilters = new Button();
            btnExportFiltered = new Button();
            pnlActions = new Panel();
            btnAdd = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            btnRefresh = new Button();
            btnViewDetails = new Button();
            pnlStatus = new Panel();
            lblTotalRecords = new Label();
            lblFilteredRecords = new Label();
            lblSelectedRecords = new Label();
            progressBar = new ProgressBar();
            dgvRecords = new DataGridView();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            pnlFilters.SuspendLayout();
            grpFilterByMapSheet.SuspendLayout();
            grpSearchFields.SuspendLayout();
            grpQuickFilters.SuspendLayout();
            pnlActions.SuspendLayout();
            pnlStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(45, 65, 95);
            pnlHeader.Controls.Add(lblFormTitle);
            pnlHeader.Controls.Add(picLogo);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1600, 80);
            pnlHeader.TabIndex = 0;
            // 
            // lblFormTitle
            // 
            lblFormTitle.AutoSize = true;
            lblFormTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblFormTitle.ForeColor = Color.White;
            lblFormTitle.Location = new Point(69, 19);
            lblFormTitle.Name = "lblFormTitle";
            lblFormTitle.Size = new Size(575, 37);
            lblFormTitle.TabIndex = 0;
            lblFormTitle.Text = "Land Owner Records - Advanced Filter View";
            // 
            // picLogo
            // 
            picLogo.Location = new Point(14, 13);
            picLogo.Margin = new Padding(3, 4, 3, 4);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(46, 53);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 1;
            picLogo.TabStop = false;
            // 
            // pnlFilters
            // 
            pnlFilters.BackColor = Color.FromArgb(248, 249, 250);
            pnlFilters.BorderStyle = BorderStyle.FixedSingle;
            pnlFilters.Controls.Add(grpFilterByMapSheet);
            pnlFilters.Controls.Add(grpSearchFields);
            pnlFilters.Controls.Add(grpQuickFilters);
            pnlFilters.Controls.Add(btnApplyFilters);
            pnlFilters.Controls.Add(btnClearFilters);
            pnlFilters.Controls.Add(btnExportFiltered);
            pnlFilters.Dock = DockStyle.Top;
            pnlFilters.Location = new Point(0, 80);
            pnlFilters.Margin = new Padding(3, 4, 3, 4);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Padding = new Padding(11, 13, 11, 13);
            pnlFilters.Size = new Size(1600, 173);
            pnlFilters.TabIndex = 1;
            // 
            // grpFilterByMapSheet
            // 
            grpFilterByMapSheet.Controls.Add(cboMapSheet);
            grpFilterByMapSheet.Controls.Add(lblMapSheet);
            grpFilterByMapSheet.Controls.Add(chkAllMapSheets);
            grpFilterByMapSheet.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpFilterByMapSheet.ForeColor = Color.FromArgb(45, 65, 95);
            grpFilterByMapSheet.Location = new Point(17, 13);
            grpFilterByMapSheet.Margin = new Padding(3, 4, 3, 4);
            grpFilterByMapSheet.Name = "grpFilterByMapSheet";
            grpFilterByMapSheet.Padding = new Padding(3, 4, 3, 4);
            grpFilterByMapSheet.Size = new Size(320, 147);
            grpFilterByMapSheet.TabIndex = 0;
            grpFilterByMapSheet.TabStop = false;
            grpFilterByMapSheet.Text = "Filter by Map Sheet";
            // 
            // cboMapSheet
            // 
            cboMapSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMapSheet.Font = new Font("Segoe UI", 9F);
            cboMapSheet.Location = new Point(11, 64);
            cboMapSheet.Margin = new Padding(3, 4, 3, 4);
            cboMapSheet.Name = "cboMapSheet";
            cboMapSheet.Size = new Size(205, 28);
            cboMapSheet.TabIndex = 1;
            cboMapSheet.SelectedIndexChanged += cboMapSheet_SelectedIndexChanged;
            // 
            // lblMapSheet
            // 
            lblMapSheet.AutoSize = true;
            lblMapSheet.Font = new Font("Segoe UI", 9F);
            lblMapSheet.ForeColor = Color.Black;
            lblMapSheet.Location = new Point(11, 37);
            lblMapSheet.Name = "lblMapSheet";
            lblMapSheet.Size = new Size(83, 20);
            lblMapSheet.TabIndex = 2;
            lblMapSheet.Text = "Map Sheet:";
            // 
            // chkAllMapSheets
            // 
            chkAllMapSheets.AutoSize = true;
            chkAllMapSheets.Checked = true;
            chkAllMapSheets.CheckState = CheckState.Checked;
            chkAllMapSheets.Font = new Font("Segoe UI", 9F);
            chkAllMapSheets.ForeColor = Color.Black;
            chkAllMapSheets.Location = new Point(11, 107);
            chkAllMapSheets.Margin = new Padding(3, 4, 3, 4);
            chkAllMapSheets.Name = "chkAllMapSheets";
            chkAllMapSheets.Size = new Size(136, 24);
            chkAllMapSheets.TabIndex = 3;
            chkAllMapSheets.Text = "Show All Sheets";
            chkAllMapSheets.CheckedChanged += chkAllMapSheets_CheckedChanged_1;
            // 
            // grpSearchFields
            // 
            grpSearchFields.Controls.Add(txtParcelNo);
            grpSearchFields.Controls.Add(lblParcelNo);
            grpSearchFields.Controls.Add(txtOwnerName);
            grpSearchFields.Controls.Add(lblOwnerName);
            grpSearchFields.Controls.Add(txtCitizenshipNo);
            grpSearchFields.Controls.Add(lblCitizenshipNo);
            grpSearchFields.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpSearchFields.ForeColor = Color.FromArgb(45, 65, 95);
            grpSearchFields.Location = new Point(354, 13);
            grpSearchFields.Margin = new Padding(3, 4, 3, 4);
            grpSearchFields.Name = "grpSearchFields";
            grpSearchFields.Padding = new Padding(3, 4, 3, 4);
            grpSearchFields.Size = new Size(594, 147);
            grpSearchFields.TabIndex = 1;
            grpSearchFields.TabStop = false;
            grpSearchFields.Text = "Search Fields";
            grpSearchFields.Enter += grpSearchFields_Enter;
            // 
            // txtParcelNo
            // 
            txtParcelNo.BorderStyle = BorderStyle.FixedSingle;
            txtParcelNo.Font = new Font("Segoe UI", 9F);
            txtParcelNo.Location = new Point(11, 64);
            txtParcelNo.Margin = new Padding(3, 4, 3, 4);
            txtParcelNo.Name = "txtParcelNo";
            txtParcelNo.PlaceholderText = "Enter parcel number...";
            txtParcelNo.Size = new Size(171, 27);
            txtParcelNo.TabIndex = 2;
            txtParcelNo.TextChanged += txtParcelNo_TextChanged;
            // 
            // lblParcelNo
            // 
            lblParcelNo.AutoSize = true;
            lblParcelNo.Font = new Font("Segoe UI", 9F);
            lblParcelNo.ForeColor = Color.Black;
            lblParcelNo.Location = new Point(11, 37);
            lblParcelNo.Name = "lblParcelNo";
            lblParcelNo.Size = new Size(75, 20);
            lblParcelNo.TabIndex = 3;
            lblParcelNo.Text = "Parcel No:";
            // 
            // txtOwnerName
            // 
            txtOwnerName.BorderStyle = BorderStyle.FixedSingle;
            txtOwnerName.Font = new Font("Segoe UI", 9F);
            txtOwnerName.Location = new Point(200, 64);
            txtOwnerName.Margin = new Padding(3, 4, 3, 4);
            txtOwnerName.Name = "txtOwnerName";
            txtOwnerName.PlaceholderText = "Enter owner name...";
            txtOwnerName.Size = new Size(194, 27);
            txtOwnerName.TabIndex = 3;
            // 
            // lblOwnerName
            // 
            lblOwnerName.AutoSize = true;
            lblOwnerName.Font = new Font("Segoe UI", 9F);
            lblOwnerName.ForeColor = Color.Black;
            lblOwnerName.Location = new Point(200, 37);
            lblOwnerName.Name = "lblOwnerName";
            lblOwnerName.Size = new Size(99, 20);
            lblOwnerName.TabIndex = 4;
            lblOwnerName.Text = "Owner Name:";
            // 
            // txtCitizenshipNo
            // 
            txtCitizenshipNo.BorderStyle = BorderStyle.FixedSingle;
            txtCitizenshipNo.Font = new Font("Segoe UI", 9F);
            txtCitizenshipNo.Location = new Point(411, 64);
            txtCitizenshipNo.Margin = new Padding(3, 4, 3, 4);
            txtCitizenshipNo.Name = "txtCitizenshipNo";
            txtCitizenshipNo.PlaceholderText = "Enter citizenship...";
            txtCitizenshipNo.Size = new Size(165, 27);
            txtCitizenshipNo.TabIndex = 4;
            // 
            // lblCitizenshipNo
            // 
            lblCitizenshipNo.AutoSize = true;
            lblCitizenshipNo.Font = new Font("Segoe UI", 9F);
            lblCitizenshipNo.ForeColor = Color.Black;
            lblCitizenshipNo.Location = new Point(411, 37);
            lblCitizenshipNo.Name = "lblCitizenshipNo";
            lblCitizenshipNo.Size = new Size(108, 20);
            lblCitizenshipNo.TabIndex = 5;
            lblCitizenshipNo.Text = "Citizenship No:";
            // 
            // grpQuickFilters
            // 
            grpQuickFilters.Controls.Add(cboDistrict);
            grpQuickFilters.Controls.Add(lblDistrict);
            grpQuickFilters.Controls.Add(cboLandUse);
            grpQuickFilters.Controls.Add(lblLandUse);
            grpQuickFilters.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpQuickFilters.ForeColor = Color.FromArgb(45, 65, 95);
            grpQuickFilters.Location = new Point(966, 13);
            grpQuickFilters.Margin = new Padding(3, 4, 3, 4);
            grpQuickFilters.Name = "grpQuickFilters";
            grpQuickFilters.Padding = new Padding(3, 4, 3, 4);
            grpQuickFilters.Size = new Size(389, 147);
            grpQuickFilters.TabIndex = 2;
            grpQuickFilters.TabStop = false;
            grpQuickFilters.Text = "Quick Filters";
            // 
            // cboDistrict
            // 
            cboDistrict.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDistrict.Font = new Font("Segoe UI", 9F);
            cboDistrict.Location = new Point(11, 64);
            cboDistrict.Margin = new Padding(3, 4, 3, 4);
            cboDistrict.Name = "cboDistrict";
            cboDistrict.Size = new Size(171, 28);
            cboDistrict.TabIndex = 5;
            // 
            // lblDistrict
            // 
            lblDistrict.AutoSize = true;
            lblDistrict.Font = new Font("Segoe UI", 9F);
            lblDistrict.ForeColor = Color.Black;
            lblDistrict.Location = new Point(11, 37);
            lblDistrict.Name = "lblDistrict";
            lblDistrict.Size = new Size(59, 20);
            lblDistrict.TabIndex = 6;
            lblDistrict.Text = "District:";
            // 
            // cboLandUse
            // 
            cboLandUse.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLandUse.Font = new Font("Segoe UI", 9F);
            cboLandUse.Location = new Point(200, 64);
            cboLandUse.Margin = new Padding(3, 4, 3, 4);
            cboLandUse.Name = "cboLandUse";
            cboLandUse.Size = new Size(171, 28);
            cboLandUse.TabIndex = 6;
            // 
            // lblLandUse
            // 
            lblLandUse.AutoSize = true;
            lblLandUse.Font = new Font("Segoe UI", 9F);
            lblLandUse.ForeColor = Color.Black;
            lblLandUse.Location = new Point(200, 37);
            lblLandUse.Name = "lblLandUse";
            lblLandUse.Size = new Size(72, 20);
            lblLandUse.TabIndex = 7;
            lblLandUse.Text = "Land Use:";
            // 
            // btnApplyFilters
            // 
            btnApplyFilters.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnApplyFilters.BackColor = Color.FromArgb(40, 167, 69);
            btnApplyFilters.Cursor = Cursors.Hand;
            btnApplyFilters.FlatAppearance.BorderSize = 0;
            btnApplyFilters.FlatStyle = FlatStyle.Flat;
            btnApplyFilters.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnApplyFilters.ForeColor = Color.White;
            btnApplyFilters.Location = new Point(1371, 33);
            btnApplyFilters.Margin = new Padding(3, 4, 3, 4);
            btnApplyFilters.Name = "btnApplyFilters";
            btnApplyFilters.Size = new Size(114, 43);
            btnApplyFilters.TabIndex = 7;
            btnApplyFilters.Text = "🔍 Apply";
            btnApplyFilters.UseVisualStyleBackColor = false;
            // 
            // btnClearFilters
            // 
            btnClearFilters.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearFilters.BackColor = Color.FromArgb(108, 117, 125);
            btnClearFilters.Cursor = Cursors.Hand;
            btnClearFilters.FlatAppearance.BorderSize = 0;
            btnClearFilters.FlatStyle = FlatStyle.Flat;
            btnClearFilters.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClearFilters.ForeColor = Color.White;
            btnClearFilters.Location = new Point(1371, 83);
            btnClearFilters.Margin = new Padding(3, 4, 3, 4);
            btnClearFilters.Name = "btnClearFilters";
            btnClearFilters.Size = new Size(114, 43);
            btnClearFilters.TabIndex = 8;
            btnClearFilters.Text = "✕ Clear";
            btnClearFilters.UseVisualStyleBackColor = false;
            // 
            // btnExportFiltered
            // 
            btnExportFiltered.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportFiltered.BackColor = Color.FromArgb(0, 123, 255);
            btnExportFiltered.Cursor = Cursors.Hand;
            btnExportFiltered.FlatAppearance.BorderSize = 0;
            btnExportFiltered.FlatStyle = FlatStyle.Flat;
            btnExportFiltered.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportFiltered.ForeColor = Color.White;
            btnExportFiltered.Location = new Point(1497, 33);
            btnExportFiltered.Margin = new Padding(3, 4, 3, 4);
            btnExportFiltered.Name = "btnExportFiltered";
            btnExportFiltered.Size = new Size(86, 92);
            btnExportFiltered.TabIndex = 9;
            btnExportFiltered.Text = "📥\r\nExport";
            btnExportFiltered.UseVisualStyleBackColor = false;
            // 
            // pnlActions
            // 
            pnlActions.BackColor = Color.FromArgb(233, 236, 239);
            pnlActions.BorderStyle = BorderStyle.FixedSingle;
            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnEdit);
            pnlActions.Controls.Add(btnDelete);
            pnlActions.Controls.Add(btnRefresh);
            pnlActions.Controls.Add(btnViewDetails);
            pnlActions.Dock = DockStyle.Top;
            pnlActions.Location = new Point(0, 253);
            pnlActions.Margin = new Padding(3, 4, 3, 4);
            pnlActions.Name = "pnlActions";
            pnlActions.Padding = new Padding(11, 7, 11, 7);
            pnlActions.Size = new Size(1600, 66);
            pnlActions.TabIndex = 2;
            // 
            // btnAdd
            // 
            btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAdd.ForeColor = Color.White;
            btnAdd.Location = new Point(17, 11);
            btnAdd.Margin = new Padding(3, 4, 3, 4);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(103, 43);
            btnAdd.TabIndex = 10;
            btnAdd.Text = "➕ Add";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += btnAdd_Click_1;
            // 
            // btnEdit
            // 
            btnEdit.BackColor = Color.FromArgb(0, 123, 255);
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Enabled = false;
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnEdit.ForeColor = Color.White;
            btnEdit.Location = new Point(131, 11);
            btnEdit.Margin = new Padding(3, 4, 3, 4);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(103, 43);
            btnEdit.TabIndex = 11;
            btnEdit.Text = "✏️ Edit";
            btnEdit.UseVisualStyleBackColor = false;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Enabled = false;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(246, 11);
            btnDelete.Margin = new Padding(3, 4, 3, 4);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(103, 43);
            btnDelete.TabIndex = 12;
            btnDelete.Text = "🗑️ Delete";
            btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.BackColor = Color.FromArgb(108, 117, 125);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Location = new Point(1480, 11);
            btnRefresh.Margin = new Padding(3, 4, 3, 4);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(103, 43);
            btnRefresh.TabIndex = 14;
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            // 
            // btnViewDetails
            // 
            btnViewDetails.BackColor = Color.FromArgb(23, 162, 184);
            btnViewDetails.Cursor = Cursors.Hand;
            btnViewDetails.Enabled = false;
            btnViewDetails.FlatAppearance.BorderSize = 0;
            btnViewDetails.FlatStyle = FlatStyle.Flat;
            btnViewDetails.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnViewDetails.ForeColor = Color.White;
            btnViewDetails.Location = new Point(360, 11);
            btnViewDetails.Margin = new Padding(3, 4, 3, 4);
            btnViewDetails.Name = "btnViewDetails";
            btnViewDetails.Size = new Size(126, 43);
            btnViewDetails.TabIndex = 13;
            btnViewDetails.Text = "👁️ View Details";
            btnViewDetails.UseVisualStyleBackColor = false;
            // 
            // pnlStatus
            // 
            pnlStatus.BackColor = Color.FromArgb(248, 249, 250);
            pnlStatus.BorderStyle = BorderStyle.FixedSingle;
            pnlStatus.Controls.Add(lblTotalRecords);
            pnlStatus.Controls.Add(lblFilteredRecords);
            pnlStatus.Controls.Add(lblSelectedRecords);
            pnlStatus.Controls.Add(progressBar);
            pnlStatus.Dock = DockStyle.Bottom;
            pnlStatus.Location = new Point(0, 818);
            pnlStatus.Margin = new Padding(3, 4, 3, 4);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Size = new Size(1600, 53);
            pnlStatus.TabIndex = 3;
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.AutoSize = true;
            lblTotalRecords.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalRecords.ForeColor = Color.FromArgb(45, 65, 95);
            lblTotalRecords.Location = new Point(17, 13);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(121, 20);
            lblTotalRecords.TabIndex = 0;
            lblTotalRecords.Text = "Total Records: 0";
            // 
            // lblFilteredRecords
            // 
            lblFilteredRecords.AutoSize = true;
            lblFilteredRecords.Font = new Font("Segoe UI", 9F);
            lblFilteredRecords.ForeColor = Color.FromArgb(40, 167, 69);
            lblFilteredRecords.Location = new Point(206, 13);
            lblFilteredRecords.Name = "lblFilteredRecords";
            lblFilteredRecords.Size = new Size(131, 20);
            lblFilteredRecords.TabIndex = 1;
            lblFilteredRecords.Text = "Filtered Records: 0";
            // 
            // lblSelectedRecords
            // 
            lblSelectedRecords.AutoSize = true;
            lblSelectedRecords.Font = new Font("Segoe UI", 9F);
            lblSelectedRecords.ForeColor = Color.FromArgb(0, 123, 255);
            lblSelectedRecords.Location = new Point(400, 13);
            lblSelectedRecords.Name = "lblSelectedRecords";
            lblSelectedRecords.Size = new Size(81, 20);
            lblSelectedRecords.TabIndex = 2;
            lblSelectedRecords.Text = "Selected: 0";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressBar.Location = new Point(1371, 13);
            progressBar.Margin = new Padding(3, 4, 3, 4);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(206, 27);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 16;
            progressBar.Visible = false;
            // 
            // dgvRecords
            // 
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToOrderColumns = true;
            dataGridViewCellStyle10.BackColor = Color.FromArgb(248, 249, 250);
            dgvRecords.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle10;
            dgvRecords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvRecords.BackgroundColor = Color.White;
            dgvRecords.BorderStyle = BorderStyle.None;
            dgvRecords.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRecords.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle11.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = Color.FromArgb(45, 65, 95);
            dataGridViewCellStyle11.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle11.ForeColor = Color.White;
            dataGridViewCellStyle11.SelectionBackColor = Color.FromArgb(45, 65, 95);
            dataGridViewCellStyle11.SelectionForeColor = Color.White;
            dataGridViewCellStyle11.WrapMode = DataGridViewTriState.False;
            dgvRecords.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle11;
            dgvRecords.ColumnHeadersHeight = 36;
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle12.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = Color.White;
            dataGridViewCellStyle12.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle12.ForeColor = Color.FromArgb(33, 37, 41);
            dataGridViewCellStyle12.SelectionBackColor = Color.FromArgb(0, 123, 255);
            dataGridViewCellStyle12.SelectionForeColor = Color.White;
            dataGridViewCellStyle12.WrapMode = DataGridViewTriState.False;
            dgvRecords.DefaultCellStyle = dataGridViewCellStyle12;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.GridColor = Color.FromArgb(222, 226, 230);
            dgvRecords.Location = new Point(0, 320);
            dgvRecords.Margin = new Padding(3, 4, 3, 4);
            dgvRecords.Name = "dgvRecords";
            dgvRecords.ReadOnly = true;
            dgvRecords.RowHeadersWidth = 50;
            dgvRecords.RowTemplate.Height = 28;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.Size = new Size(1600, 493);
            dgvRecords.TabIndex = 15;
            // 
            // frmLandownerRecordsFilterView
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1600, 871);
            Controls.Add(dgvRecords);
            Controls.Add(pnlStatus);
            Controls.Add(pnlActions);
            Controls.Add(pnlFilters);
            Controls.Add(pnlHeader);
            DoubleBuffered = true;
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1369, 918);
            Name = "frmLandownerRecordsFilterView";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Land Owner Records - Advanced Filter View";
            Load += frmLandownerRecordsFilterView_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            pnlFilters.ResumeLayout(false);
            grpFilterByMapSheet.ResumeLayout(false);
            grpFilterByMapSheet.PerformLayout();
            grpSearchFields.ResumeLayout(false);
            grpSearchFields.PerformLayout();
            grpQuickFilters.ResumeLayout(false);
            grpQuickFilters.PerformLayout();
            pnlActions.ResumeLayout(false);
            pnlStatus.ResumeLayout(false);
            pnlStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            ResumeLayout(false);
        }

        #endregion

        // Panels
        private Panel pnlHeader;
        private Panel pnlFilters;
        private Panel pnlActions;
        private Panel pnlStatus;

        // Header
        private Label lblFormTitle;
        private PictureBox picLogo;

        // Filter GroupBoxes
        private GroupBox grpFilterByMapSheet;
        private GroupBox grpSearchFields;
        private GroupBox grpQuickFilters;

        // MapSheet filter controls
        private ComboBox cboMapSheet;
        private Label lblMapSheet;
        private CheckBox chkAllMapSheets;

        // Search fields
        private TextBox txtParcelNo;
        private Label lblParcelNo;
        private TextBox txtOwnerName;
        private Label lblOwnerName;
        private TextBox txtCitizenshipNo;
        private Label lblCitizenshipNo;

        // Quick filters
        private ComboBox cboDistrict;
        private Label lblDistrict;
        private ComboBox cboLandUse;
        private Label lblLandUse;

        // Filter action buttons
        private Button btnApplyFilters;
        private Button btnClearFilters;
        private Button btnExportFiltered;

        // CRUD buttons
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnViewDetails;

        // DataGridView
        private DataGridView dgvRecords;

        // Status bar
        private Label lblTotalRecords;
        private Label lblFilteredRecords;
        private Label lblSelectedRecords;
        private ProgressBar progressBar;
    }
}
