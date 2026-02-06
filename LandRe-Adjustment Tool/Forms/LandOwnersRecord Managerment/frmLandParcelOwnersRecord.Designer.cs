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
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
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
            grpFilterByMapSheet.ForeColor = Color.FromArgb(45, 65, 95);
            grpFilterByMapSheet.Location = new Point(9, 47);
            grpFilterByMapSheet.Margin = new Padding(3, 4, 3, 4);
            grpFilterByMapSheet.Name = "grpFilterByMapSheet";
            grpFilterByMapSheet.Padding = new Padding(3, 4, 3, 4);
            grpFilterByMapSheet.Size = new Size(518, 89);
            grpFilterByMapSheet.TabIndex = 1;
            grpFilterByMapSheet.TabStop = false;
            grpFilterByMapSheet.Text = "Filter By Location";
            // 
            // cbWardNo
            // 
            cbWardNo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbWardNo.Font = new Font("Segoe UI", 9F);
            cbWardNo.Location = new Point(438, 54);
            cbWardNo.Margin = new Padding(3, 4, 3, 4);
            cbWardNo.Name = "cbWardNo";
            cbWardNo.Size = new Size(71, 28);
            cbWardNo.TabIndex = 8;
            cbWardNo.SelectedIndexChanged += comboBox3_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(438, 29);
            label3.Name = "label3";
            label3.Size = new Size(71, 20);
            label3.TabIndex = 9;
            label3.Text = "Ward No:";
            // 
            // cbMunicipalityVillage
            // 
            cbMunicipalityVillage.DropDownStyle = ComboBoxStyle.DropDownList;
            cbMunicipalityVillage.Font = new Font("Segoe UI", 9F);
            cbMunicipalityVillage.Location = new Point(280, 54);
            cbMunicipalityVillage.Margin = new Padding(3, 4, 3, 4);
            cbMunicipalityVillage.Name = "cbMunicipalityVillage";
            cbMunicipalityVillage.Size = new Size(148, 28);
            cbMunicipalityVillage.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(280, 29);
            label2.Name = "label2";
            label2.Size = new Size(145, 20);
            label2.TabIndex = 7;
            label2.Text = "Municipality/Village:";
            label2.Click += label2_Click;
            // 
            // cbDistrict
            // 
            cbDistrict.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDistrict.Font = new Font("Segoe UI", 9F);
            cbDistrict.Location = new Point(139, 54);
            cbDistrict.Margin = new Padding(3, 4, 3, 4);
            cbDistrict.Name = "cbDistrict";
            cbDistrict.Size = new Size(135, 28);
            cbDistrict.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(139, 29);
            label1.Name = "label1";
            label1.Size = new Size(59, 20);
            label1.TabIndex = 5;
            label1.Text = "District:";
            // 
            // cbProvince
            // 
            cbProvince.DropDownStyle = ComboBoxStyle.DropDownList;
            cbProvince.Font = new Font("Segoe UI", 9F);
            cbProvince.Location = new Point(19, 54);
            cbProvince.Margin = new Padding(3, 4, 3, 4);
            cbProvince.Name = "cbProvince";
            cbProvince.Size = new Size(112, 28);
            cbProvince.TabIndex = 1;
            // 
            // lblMapSheet
            // 
            lblMapSheet.AutoSize = true;
            lblMapSheet.Font = new Font("Segoe UI", 9F);
            lblMapSheet.ForeColor = Color.Black;
            lblMapSheet.Location = new Point(19, 29);
            lblMapSheet.Name = "lblMapSheet";
            lblMapSheet.Size = new Size(68, 20);
            lblMapSheet.TabIndex = 2;
            lblMapSheet.Text = "Province:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(cbMapSheet);
            groupBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox1.ForeColor = Color.FromArgb(45, 65, 95);
            groupBox1.Location = new Point(533, 47);
            groupBox1.Margin = new Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(161, 89);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Filter By Map Sheet";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(18, 29);
            label4.Name = "label4";
            label4.Size = new Size(101, 20);
            label4.TabIndex = 11;
            label4.Text = "Mapsheet No:";
            label4.Click += label4_Click;
            // 
            // cbMapSheet
            // 
            cbMapSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            cbMapSheet.Font = new Font("Segoe UI", 9F);
            cbMapSheet.Location = new Point(18, 54);
            cbMapSheet.Margin = new Padding(3, 4, 3, 4);
            cbMapSheet.Name = "cbMapSheet";
            cbMapSheet.Size = new Size(132, 28);
            cbMapSheet.TabIndex = 10;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.MidnightBlue;
            label5.Location = new Point(12, 9);
            label5.Name = "label5";
            label5.Size = new Size(284, 28);
            label5.TabIndex = 11;
            label5.Text = "Original Land Parcel Records";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtToArea);
            groupBox3.Controls.Add(txtFromArea);
            groupBox3.Controls.Add(rbRopanee);
            groupBox3.Controls.Add(rbSqm);
            groupBox3.Controls.Add(label8);
            groupBox3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox3.ForeColor = Color.FromArgb(45, 65, 95);
            groupBox3.Location = new Point(881, 47);
            groupBox3.Margin = new Padding(3, 4, 3, 4);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(3, 4, 3, 4);
            groupBox3.Size = new Size(195, 89);
            groupBox3.TabIndex = 13;
            groupBox3.TabStop = false;
            groupBox3.Text = "Filter by Area Range";
            // 
            // txtToArea
            // 
            txtToArea.BorderStyle = BorderStyle.FixedSingle;
            txtToArea.Font = new Font("Microsoft Sans Serif", 9F);
            txtToArea.Location = new Point(120, 55);
            txtToArea.Name = "txtToArea";
            txtToArea.PlaceholderText = "sq.m.";
            txtToArea.Size = new Size(67, 24);
            txtToArea.TabIndex = 16;
            txtToArea.TextAlign = HorizontalAlignment.Center;
            // 
            // txtFromArea
            // 
            txtFromArea.BorderStyle = BorderStyle.FixedSingle;
            txtFromArea.Font = new Font("Microsoft Sans Serif", 9F);
            txtFromArea.Location = new Point(16, 55);
            txtFromArea.Name = "txtFromArea";
            txtFromArea.PlaceholderText = "sq.m.";
            txtFromArea.Size = new Size(67, 24);
            txtFromArea.TabIndex = 15;
            txtFromArea.TextAlign = HorizontalAlignment.Center;
            // 
            // rbRopanee
            // 
            rbRopanee.AutoSize = true;
            rbRopanee.Font = new Font("Microsoft Sans Serif", 9F);
            rbRopanee.ForeColor = SystemColors.ControlText;
            rbRopanee.Location = new Point(89, 27);
            rbRopanee.Name = "rbRopanee";
            rbRopanee.Size = new Size(89, 22);
            rbRopanee.TabIndex = 16;
            rbRopanee.Text = "Ropanee";
            rbRopanee.UseVisualStyleBackColor = true;
            rbRopanee.CheckedChanged += rbRopanee_CheckedChanged;
            // 
            // rbSqm
            // 
            rbSqm.AutoSize = true;
            rbSqm.Checked = true;
            rbSqm.Font = new Font("Microsoft Sans Serif", 9F);
            rbSqm.ForeColor = SystemColors.ControlText;
            rbSqm.Location = new Point(17, 27);
            rbSqm.Name = "rbSqm";
            rbSqm.Size = new Size(66, 22);
            rbSqm.TabIndex = 15;
            rbSqm.TabStop = true;
            rbSqm.Text = "sq.m.";
            rbSqm.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 9F);
            label8.ForeColor = Color.Black;
            label8.Location = new Point(89, 57);
            label8.Name = "label8";
            label8.Size = new Size(23, 20);
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
            groupBox4.ForeColor = Color.FromArgb(45, 65, 95);
            groupBox4.Location = new Point(12, 144);
            groupBox4.Margin = new Padding(3, 4, 3, 4);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(3, 4, 3, 4);
            groupBox4.Size = new Size(682, 88);
            groupBox4.TabIndex = 10;
            groupBox4.TabStop = false;
            groupBox4.Text = "Search by";
            // 
            // txtLandOwner
            // 
            txtLandOwner.BorderStyle = BorderStyle.FixedSingle;
            txtLandOwner.Font = new Font("Microsoft Sans Serif", 9F);
            txtLandOwner.Location = new Point(188, 54);
            txtLandOwner.Name = "txtLandOwner";
            txtLandOwner.PlaceholderText = "Search by Land Owner";
            txtLandOwner.Size = new Size(483, 24);
            txtLandOwner.TabIndex = 14;
            // 
            // txtParcelNo
            // 
            txtParcelNo.BorderStyle = BorderStyle.FixedSingle;
            txtParcelNo.Font = new Font("Microsoft Sans Serif", 9F);
            txtParcelNo.Location = new Point(16, 54);
            txtParcelNo.Name = "txtParcelNo";
            txtParcelNo.PlaceholderText = "Search by Parcel No.";
            txtParcelNo.Size = new Size(151, 24);
            txtParcelNo.TabIndex = 12;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 9F);
            label10.ForeColor = Color.Black;
            label10.Location = new Point(188, 31);
            label10.Name = "label10";
            label10.Size = new Size(144, 20);
            label10.TabIndex = 5;
            label10.Text = "Land Owner's Name:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 9F);
            label11.ForeColor = Color.Black;
            label11.Location = new Point(12, 31);
            label11.Name = "label11";
            label11.Size = new Size(75, 20);
            label11.TabIndex = 2;
            label11.Text = "Parcel No.";
            // 
            // btnApplyFilter
            // 
            btnApplyFilter.Cursor = Cursors.Hand;
            btnApplyFilter.Enabled = false;
            btnApplyFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnApplyFilter.Image = Properties.Resources.business_1565329211;
            btnApplyFilter.Location = new Point(1082, 88);
            btnApplyFilter.Name = "btnApplyFilter";
            btnApplyFilter.Size = new Size(140, 44);
            btnApplyFilter.TabIndex = 20;
            btnApplyFilter.Text = "Apply Filter";
            btnApplyFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnApplyFilter.UseVisualStyleBackColor = false;
            // 
            // btnClearFilter
            // 
            btnClearFilter.Cursor = Cursors.Hand;
            btnClearFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClearFilter.Location = new Point(1228, 88);
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new Size(73, 44);
            btnClearFilter.TabIndex = 21;
            btnClearFilter.Text = "Clear";
            btnClearFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            // 
            // chkToggleQuickFilter
            // 
            chkToggleQuickFilter.AutoSize = true;
            chkToggleQuickFilter.Checked = true;
            chkToggleQuickFilter.CheckState = CheckState.Checked;
            chkToggleQuickFilter.Location = new Point(1082, 60);
            chkToggleQuickFilter.Name = "chkToggleQuickFilter";
            chkToggleQuickFilter.Size = new Size(155, 24);
            chkToggleQuickFilter.TabIndex = 22;
            chkToggleQuickFilter.Text = "Toggle Quick Filter";
            chkToggleQuickFilter.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ControlLightLight;
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
            panel1.Name = "panel1";
            panel1.Size = new Size(1312, 249);
            panel1.TabIndex = 2;
            // 
            // chkToggleQuickSearch
            // 
            chkToggleQuickSearch.AutoSize = true;
            chkToggleQuickSearch.Checked = true;
            chkToggleQuickSearch.CheckState = CheckState.Checked;
            chkToggleQuickSearch.Location = new Point(700, 160);
            chkToggleQuickSearch.Name = "chkToggleQuickSearch";
            chkToggleQuickSearch.Size = new Size(166, 24);
            chkToggleQuickSearch.TabIndex = 25;
            chkToggleQuickSearch.Text = "Toggle Quick Search";
            chkToggleQuickSearch.UseVisualStyleBackColor = true;
            // 
            // btnClearSearch
            // 
            btnClearSearch.Cursor = Cursors.Hand;
            btnClearSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClearSearch.Location = new Point(846, 188);
            btnClearSearch.Name = "btnClearSearch";
            btnClearSearch.Size = new Size(67, 44);
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
            btnApplySearch.Location = new Point(700, 188);
            btnApplySearch.Name = "btnApplySearch";
            btnApplySearch.Size = new Size(140, 44);
            btnApplySearch.TabIndex = 23;
            btnApplySearch.Text = "Apply Search";
            btnApplySearch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnApplySearch.UseVisualStyleBackColor = false;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(cbLandOwnership);
            groupBox2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox2.ForeColor = Color.FromArgb(45, 65, 95);
            groupBox2.Location = new Point(700, 47);
            groupBox2.Margin = new Padding(3, 4, 3, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 4, 3, 4);
            groupBox2.Size = new Size(175, 89);
            groupBox2.TabIndex = 12;
            groupBox2.TabStop = false;
            groupBox2.Text = "Filter By Ownership";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F);
            label6.ForeColor = Color.Black;
            label6.Location = new Point(18, 29);
            label6.Name = "label6";
            label6.Size = new Size(118, 20);
            label6.TabIndex = 11;
            label6.Text = "Land Ownership:";
            // 
            // cbLandOwnership
            // 
            cbLandOwnership.DropDownStyle = ComboBoxStyle.DropDownList;
            cbLandOwnership.Font = new Font("Segoe UI", 9F);
            cbLandOwnership.Location = new Point(18, 54);
            cbLandOwnership.Margin = new Padding(3, 4, 3, 4);
            cbLandOwnership.Name = "cbLandOwnership";
            cbLandOwnership.Size = new Size(148, 28);
            cbLandOwnership.TabIndex = 10;
            // 
            // lblSelectedRecords
            // 
            lblSelectedRecords.AutoSize = true;
            lblSelectedRecords.Font = new Font("Segoe UI", 9F);
            lblSelectedRecords.ForeColor = Color.FromArgb(0, 123, 255);
            lblSelectedRecords.Location = new Point(9, 44);
            lblSelectedRecords.Name = "lblSelectedRecords";
            lblSelectedRecords.Size = new Size(81, 20);
            lblSelectedRecords.TabIndex = 21;
            lblSelectedRecords.Text = "Selected: 0";
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel3.Controls.Add(lblTotalRecords);
            panel3.Controls.Add(lblFilteredRecords);
            panel3.Controls.Add(lblSelectedRecords);
            panel3.Location = new Point(0, 825);
            panel3.Name = "panel3";
            panel3.Size = new Size(1312, 71);
            panel3.TabIndex = 20;
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.AutoSize = true;
            lblTotalRecords.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalRecords.ForeColor = Color.FromArgb(45, 65, 95);
            lblTotalRecords.Location = new Point(9, 4);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(121, 20);
            lblTotalRecords.TabIndex = 19;
            lblTotalRecords.Text = "Total Records: 0";
            // 
            // lblFilteredRecords
            // 
            lblFilteredRecords.AutoSize = true;
            lblFilteredRecords.Font = new Font("Segoe UI", 9F);
            lblFilteredRecords.ForeColor = Color.Black;
            lblFilteredRecords.Location = new Point(9, 24);
            lblFilteredRecords.Name = "lblFilteredRecords";
            lblFilteredRecords.Size = new Size(131, 20);
            lblFilteredRecords.TabIndex = 20;
            lblFilteredRecords.Text = "Filtered Records: 0";
            // 
            // dgvRecords
            // 
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToOrderColumns = true;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(248, 249, 250);
            dgvRecords.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            dgvRecords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvRecords.BackgroundColor = Color.White;
            dgvRecords.BorderStyle = BorderStyle.None;
            dgvRecords.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRecords.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(45, 65, 95);
            dataGridViewCellStyle5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle5.ForeColor = Color.White;
            dataGridViewCellStyle5.SelectionBackColor = Color.FromArgb(45, 65, 95);
            dataGridViewCellStyle5.SelectionForeColor = Color.White;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.False;
            dgvRecords.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            dgvRecords.ColumnHeadersHeight = 36;
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = Color.White;
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle6.ForeColor = Color.FromArgb(33, 37, 41);
            dataGridViewCellStyle6.SelectionBackColor = Color.FromArgb(0, 123, 255);
            dataGridViewCellStyle6.SelectionForeColor = Color.White;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
            dgvRecords.DefaultCellStyle = dataGridViewCellStyle6;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.GridColor = Color.FromArgb(222, 226, 230);
            dgvRecords.Location = new Point(9, 68);
            dgvRecords.Margin = new Padding(3, 4, 3, 4);
            dgvRecords.Name = "dgvRecords";
            dgvRecords.ReadOnly = true;
            dgvRecords.RowHeadersWidth = 50;
            dgvRecords.RowTemplate.Height = 28;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.Size = new Size(1303, 495);
            dgvRecords.TabIndex = 19;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.Controls.Add(toolStrip1);
            panel2.Controls.Add(dgvRecords);
            panel2.Location = new Point(0, 252);
            panel2.Name = "panel2";
            panel2.Size = new Size(1312, 567);
            panel2.TabIndex = 18;
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.ImageScalingSize = new Size(48, 48);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnAdd, btnEdit, btnDelete, toolStripSeparator2, toolStripDropDownButton1, saveToolStripButton, toolStripSeparator1, toolStripButton1 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1312, 64);
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
            btnAdd.Size = new Size(41, 61);
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
            btnEdit.Size = new Size(39, 61);
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
            btnDelete.Size = new Size(57, 61);
            btnDelete.Text = "Delete";
            btnDelete.TextDirection = ToolStripTextDirection.Horizontal;
            btnDelete.TextImageRelation = TextImageRelation.ImageAboveText;
            btnDelete.ToolTipText = "Delete Record";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 64);
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.Image = Properties.Resources.icons8_refresh_25;
            toolStripDropDownButton1.ImageScaling = ToolStripItemImageScaling.None;
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(97, 61);
            toolStripDropDownButton1.Text = "Refresh";
            toolStripDropDownButton1.ToolTipText = "Refresh Records";
            // 
            // saveToolStripButton
            // 
            saveToolStripButton.Image = Properties.Resources.diskette4;
            saveToolStripButton.ImageScaling = ToolStripItemImageScaling.None;
            saveToolStripButton.ImageTransparentColor = Color.Magenta;
            saveToolStripButton.Name = "saveToolStripButton";
            saveToolStripButton.Size = new Size(74, 61);
            saveToolStripButton.Text = "Save";
            saveToolStripButton.ToolTipText = "Save to Database ";
            saveToolStripButton.Click += saveToolStripButton_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 64);
            // 
            // toolStripButton1
            // 
            toolStripButton1.BackgroundImageLayout = ImageLayout.None;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(214, 61);
            toolStripButton1.Text = "View Land Owners Details";
            toolStripButton1.TextDirection = ToolStripTextDirection.Horizontal;
            toolStripButton1.ToolTipText = "Add Record";
            // 
            // frmLandParcelOwnersRecord
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            ClientSize = new Size(1312, 899);
            Controls.Add(panel3);
            Controls.Add(panel1);
            Controls.Add(panel2);
            MinimumSize = new Size(1330, 0);
            Name = "frmLandParcelOwnersRecord";
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
        private ToolStripButton toolStripButton2;
    }
}