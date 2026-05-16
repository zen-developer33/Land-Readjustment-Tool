namespace Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment
{
    partial class frmLandParcelOwnersRecord
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLandParcelOwnersRecord));
            grpFilterByMapSheet = new GroupBox();
            cbWardNo = new ComboBox();
            label3 = new Label();
            cbMunicipalityVillage = new ComboBox();
            label2 = new Label();
            cbDistrict = new ComboBox();
            label1 = new Label();
            cbProvince = new ComboBox();
            lblMapSheet = new Label();
            groupBox1 = new GroupBox();
            label4 = new Label();
            cbMapSheet = new ComboBox();
            label5 = new Label();
            groupBox3 = new GroupBox();
            rbAana = new RadioButton();
            txtToArea = new TextBox();
            txtFromArea = new TextBox();
            rbRopanee = new RadioButton();
            rbSqm = new RadioButton();
            label8 = new Label();
            groupBox4 = new GroupBox();
            txtLandOwner = new TextBox();
            txtParcelNo = new TextBox();
            label10 = new Label();
            label11 = new Label();
            btnApplyFilter = new Button();
            btnClearFilter = new Button();
            chkToggleQuickFilter = new CheckBox();
            panel1 = new Panel();
            chkToggleQuickSearch = new CheckBox();
            btnClearSearch = new Button();
            btnApplySearch = new Button();
            groupBox2 = new GroupBox();
            label6 = new Label();
            cbLandOwnership = new ComboBox();
            lblSelectedRecords = new Label();
            panel3 = new Panel();
            lblTotalRecords = new Label();
            lblFilteredRecords = new Label();
            dgvRecords = new DataGridView();
            panel2 = new Panel();
            toolStrip1 = new ToolStrip();
            btnAdd = new ToolStripButton();
            btnEdit = new ToolStripButton();
            btnDelete = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            saveToolStripButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripButton1 = new ToolStripButton();
            grpFilterByMapSheet.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            panel2.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // grpFilterByMapSheet
            // 
            grpFilterByMapSheet.Controls.Add(cbWardNo);
            grpFilterByMapSheet.Controls.Add(label3);
            grpFilterByMapSheet.Controls.Add(cbMunicipalityVillage);
            grpFilterByMapSheet.Controls.Add(label2);
            grpFilterByMapSheet.Controls.Add(cbDistrict);
            grpFilterByMapSheet.Controls.Add(label1);
            grpFilterByMapSheet.Controls.Add(cbProvince);
            grpFilterByMapSheet.Controls.Add(lblMapSheet);
            grpFilterByMapSheet.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpFilterByMapSheet.ForeColor = SystemColors.ControlText;
            grpFilterByMapSheet.Location = new Point(8, 35);
            grpFilterByMapSheet.Name = "grpFilterByMapSheet";
            grpFilterByMapSheet.Size = new Size(453, 67);
            grpFilterByMapSheet.TabIndex = 1;
            grpFilterByMapSheet.TabStop = false;
            grpFilterByMapSheet.Text = "Filter By Location";
            // 
            // cbWardNo
            // 
            cbWardNo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbWardNo.FlatStyle = FlatStyle.System;
            cbWardNo.Font = new Font("Segoe UI", 9F);
            cbWardNo.Location = new Point(383, 40);
            cbWardNo.Name = "cbWardNo";
            cbWardNo.Size = new Size(63, 23);
            cbWardNo.TabIndex = 8;
            cbWardNo.SelectedIndexChanged += comboBox3_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F);
            label3.ForeColor = SystemColors.ControlText;
            label3.Location = new Point(383, 22);
            label3.Name = "label3";
            label3.Size = new Size(57, 15);
            label3.TabIndex = 9;
            label3.Text = "Ward No:";
            // 
            // cbMunicipalityVillage
            // 
            cbMunicipalityVillage.DropDownStyle = ComboBoxStyle.DropDownList;
            cbMunicipalityVillage.FlatStyle = FlatStyle.System;
            cbMunicipalityVillage.Font = new Font("Segoe UI", 9F);
            cbMunicipalityVillage.Location = new Point(245, 40);
            cbMunicipalityVillage.Name = "cbMunicipalityVillage";
            cbMunicipalityVillage.Size = new Size(130, 23);
            cbMunicipalityVillage.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.ForeColor = SystemColors.ControlText;
            label2.Location = new Point(245, 22);
            label2.Name = "label2";
            label2.Size = new Size(116, 15);
            label2.TabIndex = 7;
            label2.Text = "Municipality/Village:";
            label2.Click += label2_Click;
            // 
            // cbDistrict
            // 
            cbDistrict.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDistrict.FlatStyle = FlatStyle.System;
            cbDistrict.Font = new Font("Segoe UI", 9F);
            cbDistrict.Location = new Point(133, 40);
            cbDistrict.Name = "cbDistrict";
            cbDistrict.Size = new Size(107, 23);
            cbDistrict.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(133, 22);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 5;
            label1.Text = "District:";
            // 
            // cbProvince
            // 
            cbProvince.DropDownStyle = ComboBoxStyle.DropDownList;
            cbProvince.FlatStyle = FlatStyle.System;
            cbProvince.Font = new Font("Segoe UI", 9F);
            cbProvince.Location = new Point(17, 40);
            cbProvince.Name = "cbProvince";
            cbProvince.Size = new Size(112, 23);
            cbProvince.TabIndex = 1;
            // 
            // lblMapSheet
            // 
            lblMapSheet.AutoSize = true;
            lblMapSheet.Font = new Font("Segoe UI", 9F);
            lblMapSheet.ForeColor = SystemColors.ControlText;
            lblMapSheet.Location = new Point(17, 22);
            lblMapSheet.Name = "lblMapSheet";
            lblMapSheet.Size = new Size(56, 15);
            lblMapSheet.TabIndex = 2;
            lblMapSheet.Text = "Province:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(cbMapSheet);
            groupBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox1.ForeColor = SystemColors.ControlText;
            groupBox1.Location = new Point(466, 35);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(150, 67);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Filter By Map Sheet";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F);
            label4.ForeColor = SystemColors.ControlText;
            label4.Location = new Point(16, 22);
            label4.Name = "label4";
            label4.Size = new Size(81, 15);
            label4.TabIndex = 11;
            label4.Text = "Mapsheet No:";
            label4.Click += label4_Click;
            // 
            // cbMapSheet
            // 
            cbMapSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            cbMapSheet.FlatStyle = FlatStyle.System;
            cbMapSheet.Font = new Font("Segoe UI", 9F);
            cbMapSheet.Location = new Point(16, 40);
            cbMapSheet.Name = "cbMapSheet";
            cbMapSheet.Size = new Size(130, 23);
            cbMapSheet.TabIndex = 10;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.ForeColor = SystemColors.ControlText;
            label5.Location = new Point(10, 7);
            label5.Name = "label5";
            label5.Size = new Size(228, 21);
            label5.TabIndex = 11;
            label5.Text = "Original Land Parcel Records";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(rbAana);
            groupBox3.Controls.Add(txtToArea);
            groupBox3.Controls.Add(txtFromArea);
            groupBox3.Controls.Add(rbRopanee);
            groupBox3.Controls.Add(rbSqm);
            groupBox3.Controls.Add(label8);
            groupBox3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox3.ForeColor = SystemColors.ControlText;
            groupBox3.Location = new Point(771, 35);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(278, 67);
            groupBox3.TabIndex = 13;
            groupBox3.TabStop = false;
            groupBox3.Text = "Filter by Area Range";
            // 
            // rbAana
            // 
            rbAana.AutoSize = true;
            rbAana.Font = new Font("Microsoft Sans Serif", 9F);
            rbAana.ForeColor = SystemColors.ControlText;
            rbAana.Location = new Point(198, 20);
            rbAana.Margin = new Padding(3, 2, 3, 2);
            rbAana.Name = "rbAana";
            rbAana.Size = new Size(56, 19);
            rbAana.TabIndex = 17;
            rbAana.Text = "Aana ";
            rbAana.UseVisualStyleBackColor = true;
            // 
            // txtToArea
            // 
            txtToArea.BorderStyle = BorderStyle.FixedSingle;
            txtToArea.Font = new Font("Microsoft Sans Serif", 9F);
            txtToArea.Location = new Point(162, 40);
            txtToArea.Margin = new Padding(3, 2, 3, 2);
            txtToArea.Name = "txtToArea";
            txtToArea.PlaceholderText = "sq.m.";
            txtToArea.Size = new Size(102, 21);
            txtToArea.TabIndex = 16;
            txtToArea.TextAlign = HorizontalAlignment.Center;
            // 
            // txtFromArea
            // 
            txtFromArea.BorderStyle = BorderStyle.FixedSingle;
            txtFromArea.Font = new Font("Microsoft Sans Serif", 9F);
            txtFromArea.Location = new Point(14, 41);
            txtFromArea.Margin = new Padding(3, 2, 3, 2);
            txtFromArea.Name = "txtFromArea";
            txtFromArea.PlaceholderText = "sq.m.";
            txtFromArea.Size = new Size(103, 21);
            txtFromArea.TabIndex = 15;
            txtFromArea.TextAlign = HorizontalAlignment.Center;
            // 
            // rbRopanee
            // 
            rbRopanee.AutoSize = true;
            rbRopanee.Font = new Font("Microsoft Sans Serif", 9F);
            rbRopanee.ForeColor = SystemColors.ControlText;
            rbRopanee.Location = new Point(107, 20);
            rbRopanee.Margin = new Padding(3, 2, 3, 2);
            rbRopanee.Name = "rbRopanee";
            rbRopanee.Size = new Size(65, 19);
            rbRopanee.TabIndex = 16;
            rbRopanee.Text = "Ropani";
            rbRopanee.UseVisualStyleBackColor = true;
            // 
            // rbSqm
            // 
            rbSqm.AutoSize = true;
            rbSqm.Checked = true;
            rbSqm.Font = new Font("Microsoft Sans Serif", 9F);
            rbSqm.ForeColor = SystemColors.ControlText;
            rbSqm.Location = new Point(15, 20);
            rbSqm.Margin = new Padding(3, 2, 3, 2);
            rbSqm.Name = "rbSqm";
            rbSqm.Size = new Size(55, 19);
            rbSqm.TabIndex = 15;
            rbSqm.TabStop = true;
            rbSqm.Text = "sq.m.";
            rbSqm.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 9F);
            label8.ForeColor = SystemColors.ControlText;
            label8.Location = new Point(129, 43);
            label8.Name = "label8";
            label8.Size = new Size(18, 15);
            label8.TabIndex = 13;
            label8.Text = "to";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(txtLandOwner);
            groupBox4.Controls.Add(txtParcelNo);
            groupBox4.Controls.Add(label10);
            groupBox4.Controls.Add(label11);
            groupBox4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox4.ForeColor = SystemColors.ControlText;
            groupBox4.Location = new Point(10, 108);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(597, 66);
            groupBox4.TabIndex = 10;
            groupBox4.TabStop = false;
            groupBox4.Text = "Search by";
            // 
            // txtLandOwner
            // 
            txtLandOwner.BorderStyle = BorderStyle.FixedSingle;
            txtLandOwner.Font = new Font("Segoe UI", 9F);
            txtLandOwner.Location = new Point(164, 40);
            txtLandOwner.Margin = new Padding(3, 2, 3, 2);
            txtLandOwner.Name = "txtLandOwner";
            txtLandOwner.PlaceholderText = "Search by Land Owner";
            txtLandOwner.Size = new Size(423, 23);
            txtLandOwner.TabIndex = 14;
            // 
            // txtParcelNo
            // 
            txtParcelNo.BorderStyle = BorderStyle.FixedSingle;
            txtParcelNo.Font = new Font("Segoe UI", 9F);
            txtParcelNo.Location = new Point(14, 40);
            txtParcelNo.Margin = new Padding(3, 2, 3, 2);
            txtParcelNo.Name = "txtParcelNo";
            txtParcelNo.PlaceholderText = "Search by Parcel No.";
            txtParcelNo.Size = new Size(132, 23);
            txtParcelNo.TabIndex = 12;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 9F);
            label10.ForeColor = SystemColors.ControlText;
            label10.Location = new Point(164, 23);
            label10.Name = "label10";
            label10.Size = new Size(117, 15);
            label10.TabIndex = 5;
            label10.Text = "Land Owner's Name:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 9F);
            label11.ForeColor = SystemColors.ControlText;
            label11.Location = new Point(10, 23);
            label11.Name = "label11";
            label11.Size = new Size(61, 15);
            label11.TabIndex = 2;
            label11.Text = "Parcel No.";
            // 
            // btnApplyFilter
            // 
            btnApplyFilter.Cursor = Cursors.Hand;
            btnApplyFilter.Enabled = false;
            btnApplyFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnApplyFilter.Image = Properties.Resources.business_1565329211;
            btnApplyFilter.Location = new Point(1054, 69);
            btnApplyFilter.Margin = new Padding(3, 2, 3, 2);
            btnApplyFilter.Name = "btnApplyFilter";
            btnApplyFilter.Size = new Size(122, 33);
            btnApplyFilter.TabIndex = 20;
            btnApplyFilter.Text = "Apply Filter";
            btnApplyFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnApplyFilter.UseVisualStyleBackColor = true;
            // 
            // btnClearFilter
            // 
            btnClearFilter.Cursor = Cursors.Hand;
            btnClearFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClearFilter.Location = new Point(1182, 69);
            btnClearFilter.Margin = new Padding(3, 2, 3, 2);
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new Size(59, 33);
            btnClearFilter.TabIndex = 21;
            btnClearFilter.Text = "Clear";
            btnClearFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            // 
            // chkToggleQuickFilter
            // 
            chkToggleQuickFilter.AutoSize = true;
            chkToggleQuickFilter.Checked = true;
            chkToggleQuickFilter.CheckState = CheckState.Checked;
            chkToggleQuickFilter.Location = new Point(1054, 48);
            chkToggleQuickFilter.Margin = new Padding(3, 2, 3, 2);
            chkToggleQuickFilter.Name = "chkToggleQuickFilter";
            chkToggleQuickFilter.Size = new Size(124, 19);
            chkToggleQuickFilter.TabIndex = 22;
            chkToggleQuickFilter.Text = "Toggle Quick Filter";
            chkToggleQuickFilter.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Control;
            panel1.Controls.Add(chkToggleQuickSearch);
            panel1.Controls.Add(btnClearSearch);
            panel1.Controls.Add(groupBox3);
            panel1.Controls.Add(btnApplySearch);
            panel1.Controls.Add(chkToggleQuickFilter);
            panel1.Controls.Add(btnClearFilter);
            panel1.Controls.Add(btnApplyFilter);
            panel1.Controls.Add(groupBox4);
            panel1.Controls.Add(groupBox2);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(groupBox1);
            panel1.Controls.Add(grpFilterByMapSheet);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(3, 2, 3, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(1377, 187);
            panel1.TabIndex = 2;
            // 
            // chkToggleQuickSearch
            // 
            chkToggleQuickSearch.AutoSize = true;
            chkToggleQuickSearch.Checked = true;
            chkToggleQuickSearch.CheckState = CheckState.Checked;
            chkToggleQuickSearch.Location = new Point(612, 120);
            chkToggleQuickSearch.Margin = new Padding(3, 2, 3, 2);
            chkToggleQuickSearch.Name = "chkToggleQuickSearch";
            chkToggleQuickSearch.Size = new Size(133, 19);
            chkToggleQuickSearch.TabIndex = 25;
            chkToggleQuickSearch.Text = "Toggle Quick Search";
            chkToggleQuickSearch.UseVisualStyleBackColor = true;
            // 
            // btnClearSearch
            // 
            btnClearSearch.Cursor = Cursors.Hand;
            btnClearSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClearSearch.Location = new Point(740, 141);
            btnClearSearch.Margin = new Padding(3, 2, 3, 2);
            btnClearSearch.Name = "btnClearSearch";
            btnClearSearch.Size = new Size(59, 33);
            btnClearSearch.TabIndex = 24;
            btnClearSearch.Text = "Clear";
            btnClearSearch.TextImageRelation = TextImageRelation.ImageBeforeText;
            // 
            // btnApplySearch
            // 
            btnApplySearch.Cursor = Cursors.Hand;
            btnApplySearch.Enabled = false;
            btnApplySearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnApplySearch.Image = Properties.Resources.find_icon1;
            btnApplySearch.Location = new Point(612, 141);
            btnApplySearch.Margin = new Padding(3, 2, 3, 2);
            btnApplySearch.Name = "btnApplySearch";
            btnApplySearch.Size = new Size(122, 33);
            btnApplySearch.TabIndex = 23;
            btnApplySearch.Text = "Apply Search";
            btnApplySearch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnApplySearch.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(cbLandOwnership);
            groupBox2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox2.ForeColor = SystemColors.ControlText;
            groupBox2.Location = new Point(622, 35);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(144, 67);
            groupBox2.TabIndex = 12;
            groupBox2.TabStop = false;
            groupBox2.Text = "Filter By Ownership";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F);
            label6.ForeColor = SystemColors.ControlText;
            label6.Location = new Point(16, 22);
            label6.Name = "label6";
            label6.Size = new Size(96, 15);
            label6.TabIndex = 11;
            label6.Text = "Land Ownership:";
            // 
            // cbLandOwnership
            // 
            cbLandOwnership.DropDownStyle = ComboBoxStyle.DropDownList;
            cbLandOwnership.FlatStyle = FlatStyle.System;
            cbLandOwnership.Font = new Font("Segoe UI", 9F);
            cbLandOwnership.Location = new Point(16, 40);
            cbLandOwnership.Name = "cbLandOwnership";
            cbLandOwnership.Size = new Size(122, 23);
            cbLandOwnership.TabIndex = 10;
            // 
            // lblSelectedRecords
            // 
            lblSelectedRecords.AutoSize = true;
            lblSelectedRecords.Font = new Font("Segoe UI", 9F);
            lblSelectedRecords.ForeColor = SystemColors.ControlText;
            lblSelectedRecords.Location = new Point(8, 33);
            lblSelectedRecords.Name = "lblSelectedRecords";
            lblSelectedRecords.Size = new Size(63, 15);
            lblSelectedRecords.TabIndex = 21;
            lblSelectedRecords.Text = "Selected: 0";
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel3.Controls.Add(lblTotalRecords);
            panel3.Controls.Add(lblFilteredRecords);
            panel3.Controls.Add(lblSelectedRecords);
            panel3.Location = new Point(0, 532);
            panel3.Margin = new Padding(3, 2, 3, 2);
            panel3.Name = "panel3";
            panel3.Size = new Size(1377, 53);
            panel3.TabIndex = 20;
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.AutoSize = true;
            lblTotalRecords.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalRecords.ForeColor = SystemColors.ControlText;
            lblTotalRecords.Location = new Point(8, 3);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(95, 15);
            lblTotalRecords.TabIndex = 19;
            lblTotalRecords.Text = "Total Records: 0";
            // 
            // lblFilteredRecords
            // 
            lblFilteredRecords.AutoSize = true;
            lblFilteredRecords.Font = new Font("Segoe UI", 9F);
            lblFilteredRecords.ForeColor = SystemColors.ControlText;
            lblFilteredRecords.Location = new Point(8, 18);
            lblFilteredRecords.Name = "lblFilteredRecords";
            lblFilteredRecords.Size = new Size(103, 15);
            lblFilteredRecords.TabIndex = 20;
            lblFilteredRecords.Text = "Filtered Records: 0";
            // 
            // dgvRecords
            // 
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToOrderColumns = true;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dgvRecords.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvRecords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvRecords.BackgroundColor = SystemColors.Window;
            dgvRecords.BorderStyle = BorderStyle.None;
            dgvRecords.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRecords.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvRecords.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvRecords.ColumnHeadersHeight = 36;
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvRecords.DefaultCellStyle = dataGridViewCellStyle3;
            dgvRecords.EnableHeadersVisualStyles = true;
            dgvRecords.GridColor = SystemColors.ControlDark;
            dgvRecords.Location = new Point(8, 51);
            dgvRecords.Name = "dgvRecords";
            dgvRecords.ReadOnly = true;
            dgvRecords.RowHeadersWidth = 50;
            dgvRecords.RowTemplate.Height = 28;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.Size = new Size(1369, 284);
            dgvRecords.TabIndex = 19;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.Controls.Add(toolStrip1);
            panel2.Controls.Add(dgvRecords);
            panel2.Location = new Point(0, 189);
            panel2.Margin = new Padding(3, 2, 3, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(1377, 338);
            panel2.TabIndex = 18;
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.ImageScalingSize = new Size(48, 48);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnAdd, btnEdit, btnDelete, toolStripSeparator2, toolStripDropDownButton1, saveToolStripButton, toolStripSeparator1, toolStripButton1 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1377, 48);
            toolStrip1.TabIndex = 23;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnAdd
            // 
            btnAdd.BackgroundImageLayout = ImageLayout.None;
            btnAdd.Image = Properties.Resources.icons8_add_25__1_;
            btnAdd.ImageScaling = ToolStripItemImageScaling.None;
            btnAdd.ImageTransparentColor = Color.Magenta;
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(33, 45);
            btnAdd.Text = "Add";
            btnAdd.TextDirection = ToolStripTextDirection.Horizontal;
            btnAdd.TextImageRelation = TextImageRelation.ImageAboveText;
            btnAdd.ToolTipText = "Add Record";
            // 
            // btnEdit
            // 
            btnEdit.Image = Properties.Resources.edit_icon;
            btnEdit.ImageScaling = ToolStripItemImageScaling.None;
            btnEdit.ImageTransparentColor = Color.Magenta;
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(31, 45);
            btnEdit.Text = "Edit";
            btnEdit.TextDirection = ToolStripTextDirection.Horizontal;
            btnEdit.TextImageRelation = TextImageRelation.ImageAboveText;
            btnEdit.ToolTipText = "Edit Record";
            // 
            // btnDelete
            // 
            btnDelete.Image = Properties.Resources.delete_icon_25;
            btnDelete.ImageScaling = ToolStripItemImageScaling.None;
            btnDelete.ImageTransparentColor = Color.Magenta;
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(44, 45);
            btnDelete.Text = "Delete";
            btnDelete.TextDirection = ToolStripTextDirection.Horizontal;
            btnDelete.TextImageRelation = TextImageRelation.ImageAboveText;
            btnDelete.ToolTipText = "Delete Record";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 48);
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.Image = Properties.Resources.icons8_refresh_25;
            toolStripDropDownButton1.ImageScaling = ToolStripItemImageScaling.None;
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(84, 45);
            toolStripDropDownButton1.Text = "Refresh";
            toolStripDropDownButton1.ToolTipText = "Refresh Records";
            // 
            // saveToolStripButton
            // 
            saveToolStripButton.Image = Properties.Resources.diskette22;
            saveToolStripButton.ImageScaling = ToolStripItemImageScaling.None;
            saveToolStripButton.ImageTransparentColor = Color.Magenta;
            saveToolStripButton.Name = "saveToolStripButton";
            saveToolStripButton.Size = new Size(55, 45);
            saveToolStripButton.Text = "Save";
            saveToolStripButton.ToolTipText = "Save to Database ";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 48);
            // 
            // toolStripButton1
            // 
            toolStripButton1.BackgroundImageLayout = ImageLayout.None;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(176, 45);
            toolStripButton1.Text = "View Land Owners Details";
            toolStripButton1.TextDirection = ToolStripTextDirection.Horizontal;
            toolStripButton1.ToolTipText = "Add Record";
            // 
            // frmLandParcelOwnersRecord
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1377, 587);
            Controls.Add(panel3);
            Controls.Add(panel1);
            Controls.Add(panel2);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(1166, 39);
            Name = "frmLandParcelOwnersRecord";
            RightToLeft = RightToLeft.No;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmLandParcelOwnersRecord";
            Load += frmLandParcelOwnersRecord_Load;
            grpFilterByMapSheet.ResumeLayout(false);
            grpFilterByMapSheet.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            panel2.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            Land_Readjustment_Tool.UI.Forms.RecordFormTheme.Apply(this);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpFilterByMapSheet;
        private ComboBox cbWardNo;
        private Label label3;
        private ComboBox cbMunicipalityVillage;
        private Label label2;
        private ComboBox cbDistrict;
        private Label label1;
        private ComboBox cbProvince;
        private Label lblMapSheet;
        private GroupBox groupBox1;
        private Label label4;
        private ComboBox cbMapSheet;
        private Label label5;
        private GroupBox groupBox3;
        private TextBox txtToArea;
        private TextBox txtFromArea;
        private RadioButton rbRopanee;
        private RadioButton rbSqm;
        private Label label8;
        private GroupBox groupBox4;
        private TextBox txtLandOwner;
        private TextBox txtParcelNo;
        private Label label10;
        private Label label11;
        private Button btnApplyFilter;
        private Button btnClearFilter;
        private CheckBox chkToggleQuickFilter;
        private Panel panel1;
        private Label lblSelectedRecords;
        private Panel panel3;
        private Label lblTotalRecords;
        private Label lblFilteredRecords;
        private DataGridView dgvRecords;
        private Panel panel2;
        private ToolStrip toolStrip1;
        private ToolStripButton btnAdd;
        private ToolStripButton btnEdit;
        private ToolStripButton btnDelete;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripButton saveToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButton1;
        private Button btnClearSearch;
        private Button btnApplySearch;
        private CheckBox chkToggleQuickSearch;
        private GroupBox groupBox2;
        private Label label6;
        private ComboBox cbLandOwnership;
        private RadioButton rbAana;
    }
}
