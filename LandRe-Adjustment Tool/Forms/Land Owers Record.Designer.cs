//namespace Land_Readjustment_Tool
//{
//    partial class frmLandownersRecord
//    {
//        /// <summary>
//        /// Required designer variable.
//        /// </summary>
//        private System.ComponentModel.IContainer components = null;

//        /// <summary>
//        /// Clean up any resources being used.
//        /// </summary>
//        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Windows Form Designer generated code

//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
//            dataLandOwnersRecord = new DataGridView();
//            serialNo = new DataGridViewTextBoxColumn();
//            parcelNo = new DataGridViewTextBoxColumn();
//            district = new DataGridViewTextBoxColumn();
//            gapa_napa = new DataGridViewTextBoxColumn();
//            mapSheetNo = new DataGridViewTextBoxColumn();
//            landOwner = new DataGridViewTextBoxColumn();
//            father_spouse = new DataGridViewTextBoxColumn();
//            citizenshipNo = new DataGridViewTextBoxColumn();
//            tenant = new DataGridViewTextBoxColumn();
//            address = new DataGridViewTextBoxColumn();
//            landUse = new DataGridViewTextBoxColumn();
//            area_sqm = new DataGridViewTextBoxColumn();
//            area_rapd = new DataGridViewTextBoxColumn();
//            area_bkd = new DataGridViewTextBoxColumn();
//            mothNo = new DataGridViewTextBoxColumn();
//            paanaNo = new DataGridViewTextBoxColumn();
//            remarks = new DataGridViewTextBoxColumn();
//            btnImportFromExcel = new Button();
//            btn_Export = new Button();
//            btnSave = new Button();
//            groupBox1 = new GroupBox();
//            btnImport = new Button();
//            btnEditRecord = new Button();
//            btnAddRecord = new Button();
//            btnShowValidationErrors = new Button();
//            btnClose = new Button();
//            btnDelete = new Button();
//            label1 = new Label();
//            txtRecordsCount = new TextBox();
//            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
//            ((System.ComponentModel.ISupportInitialize)dataLandOwnersRecord).BeginInit();
//            groupBox1.SuspendLayout();
//            SuspendLayout();
//            // 
//            // dataLandOwnersRecord
//            // 
//            dataLandOwnersRecord.AllowUserToAddRows = false;
//            dataLandOwnersRecord.AllowUserToDeleteRows = false;
//            dataLandOwnersRecord.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
//            dataLandOwnersRecord.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
//            dataLandOwnersRecord.BackgroundColor = SystemColors.ControlLight;
//            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
//            dataGridViewCellStyle1.BackColor = SystemColors.Control;
//            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
//            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
//            dataGridViewCellStyle1.SelectionBackColor = SystemColors.MenuHighlight;
//            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
//            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
//            dataLandOwnersRecord.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
//            dataLandOwnersRecord.ColumnHeadersHeight = 29;
//            dataLandOwnersRecord.Columns.AddRange(new DataGridViewColumn[] { serialNo, parcelNo, district, gapa_napa, mapSheetNo, landOwner, father_spouse, citizenshipNo, tenant, address, landUse, area_sqm, area_rapd, area_bkd, mothNo, paanaNo, remarks });
//            dataLandOwnersRecord.Location = new Point(8, 98);
//            dataLandOwnersRecord.Margin = new Padding(2);
//            dataLandOwnersRecord.Name = "dataLandOwnersRecord";
//            dataLandOwnersRecord.RowHeadersVisible = false;
//            dataLandOwnersRecord.RowHeadersWidth = 51;
//            dataLandOwnersRecord.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
//            dataLandOwnersRecord.Size = new Size(1296, 609);
//            dataLandOwnersRecord.TabIndex = 0;
//            // 
//            // serialNo
//            // 
//            serialNo.Frozen = true;
//            serialNo.HeaderText = "S.N.";
//            serialNo.MinimumWidth = 6;
//            serialNo.Name = "serialNo";
//            serialNo.Width = 66;
//            // 
//            // parcelNo
//            // 
//            parcelNo.Frozen = true;
//            parcelNo.HeaderText = "Parcel No.";
//            parcelNo.MinimumWidth = 6;
//            parcelNo.Name = "parcelNo";
//            parcelNo.Width = 109;
//            // 
//            // district
//            // 
//            district.HeaderText = "District";
//            district.MinimumWidth = 6;
//            district.Name = "district";
//            district.Width = 89;
//            // 
//            // gapa_napa
//            // 
//            gapa_napa.HeaderText = "Village/Municipality";
//            gapa_napa.MinimumWidth = 6;
//            gapa_napa.Name = "gapa_napa";
//            gapa_napa.Width = 178;
//            // 
//            // mapSheetNo
//            // 
//            mapSheetNo.HeaderText = "Map Sheet No.";
//            mapSheetNo.MinimumWidth = 6;
//            mapSheetNo.Name = "mapSheetNo";
//            mapSheetNo.Width = 141;
//            // 
//            // landOwner
//            // 
//            landOwner.HeaderText = "Land Owner";
//            landOwner.MinimumWidth = 6;
//            landOwner.Name = "landOwner";
//            landOwner.Width = 122;
//            // 
//            // father_spouse
//            // 
//            father_spouse.HeaderText = "Father/Spouse";
//            father_spouse.MinimumWidth = 6;
//            father_spouse.Name = "father_spouse";
//            father_spouse.Width = 140;
//            // 
//            // citizenshipNo
//            // 
//            citizenshipNo.HeaderText = "Citizenship Number";
//            citizenshipNo.MinimumWidth = 6;
//            citizenshipNo.Name = "citizenshipNo";
//            citizenshipNo.Width = 176;
//            // 
//            // tenant
//            // 
//            tenant.HeaderText = "Tenant";
//            tenant.MinimumWidth = 6;
//            tenant.Name = "tenant";
//            tenant.Width = 86;
//            // 
//            // address
//            // 
//            address.HeaderText = "Address";
//            address.MinimumWidth = 6;
//            address.Name = "address";
//            address.Width = 95;
//            // 
//            // landUse
//            // 
//            landUse.HeaderText = "Land Use";
//            landUse.MinimumWidth = 6;
//            landUse.Name = "landUse";
//            landUse.Width = 102;
//            // 
//            // area_sqm
//            // 
//            area_sqm.HeaderText = "Area (sq.m)";
//            area_sqm.MinimumWidth = 6;
//            area_sqm.Name = "area_sqm";
//            area_sqm.Width = 121;
//            // 
//            // area_rapd
//            // 
//            area_rapd.HeaderText = "R-A-P-D";
//            area_rapd.MinimumWidth = 6;
//            area_rapd.Name = "area_rapd";
//            area_rapd.Width = 97;
//            // 
//            // area_bkd
//            // 
//            area_bkd.HeaderText = "B-K-D";
//            area_bkd.MinimumWidth = 6;
//            area_bkd.Name = "area_bkd";
//            area_bkd.Width = 81;
//            // 
//            // mothNo
//            // 
//            mothNo.HeaderText = "Moth No.";
//            mothNo.MinimumWidth = 6;
//            mothNo.Name = "mothNo";
//            mothNo.Width = 105;
//            // 
//            // paanaNo
//            // 
//            paanaNo.HeaderText = "Paana No.";
//            paanaNo.MinimumWidth = 6;
//            paanaNo.Name = "paanaNo";
//            paanaNo.Width = 109;
//            // 
//            // remarks
//            // 
//            remarks.HeaderText = "Remarks";
//            remarks.MinimumWidth = 6;
//            remarks.Name = "remarks";
//            remarks.Width = 99;
//            // 
//            // btnImportFromExcel
//            // 
//            btnImportFromExcel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
//            btnImportFromExcel.Location = new Point(6, 18);
//            btnImportFromExcel.Margin = new Padding(2);
//            btnImportFromExcel.Name = "btnImportFromExcel";
//            btnImportFromExcel.Size = new Size(168, 34);
//            btnImportFromExcel.TabIndex = 1;
//            btnImportFromExcel.Text = "Import from Excel";
//            btnImportFromExcel.UseVisualStyleBackColor = true;
//            // 
//            // btn_Export
//            // 
//            btn_Export.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
//            btn_Export.Location = new Point(249, 711);
//            btn_Export.Margin = new Padding(2);
//            btn_Export.Name = "btn_Export";
//            btn_Export.Size = new Size(88, 28);
//            btn_Export.TabIndex = 2;
//            btn_Export.Text = "Export Record";
//            btn_Export.UseVisualStyleBackColor = true;
//            // 
//            // btnSave
//            // 
//            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
//            btnSave.Location = new Point(178, 18);
//            btnSave.Margin = new Padding(2);
//            btnSave.Name = "btnSave";
//            btnSave.Size = new Size(76, 34);
//            btnSave.TabIndex = 7;
//            btnSave.Text = "Save";
//            btnSave.UseVisualStyleBackColor = true;
//            btnSave.Click += btnSave_Click;
//            // 
//            // groupBox1
//            // 
//            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
//            groupBox1.Controls.Add(btnImport);
//            groupBox1.Controls.Add(btnEditRecord);
//            groupBox1.Controls.Add(btnAddRecord);
//            groupBox1.Controls.Add(btnShowValidationErrors);
//            groupBox1.Controls.Add(btnClose);
//            groupBox1.Controls.Add(btnDelete);
//            groupBox1.Controls.Add(btnSave);
//            groupBox1.Controls.Add(btnImportFromExcel);
//            groupBox1.Location = new Point(8, 12);
//            groupBox1.Margin = new Padding(2);
//            groupBox1.Name = "groupBox1";
//            groupBox1.Padding = new Padding(2);
//            groupBox1.Size = new Size(1295, 61);
//            groupBox1.TabIndex = 8;
//            groupBox1.TabStop = false;
//            // 
//            // btnImport
//            // 
//            btnImport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
//            btnImport.Location = new Point(765, 18);
//            btnImport.Margin = new Padding(2);
//            btnImport.Name = "btnImport";
//            btnImport.Size = new Size(168, 34);
//            btnImport.TabIndex = 14;
//            btnImport.Text = "Import Data";
//            btnImport.UseVisualStyleBackColor = true;
//            btnImport.Click += btnImport_Click;
//            // 
//            // btnEditRecord
//            // 
//            btnEditRecord.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
//            btnEditRecord.Location = new Point(376, 18);
//            btnEditRecord.Margin = new Padding(2);
//            btnEditRecord.Name = "btnEditRecord";
//            btnEditRecord.Size = new Size(114, 34);
//            btnEditRecord.TabIndex = 13;
//            btnEditRecord.Text = "Edit Record";
//            btnEditRecord.UseVisualStyleBackColor = true;
//            btnEditRecord.Click += btnEditRecord_Click;
//            // 
//            // btnAddRecord
//            // 
//            btnAddRecord.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
//            btnAddRecord.Location = new Point(258, 18);
//            btnAddRecord.Margin = new Padding(2);
//            btnAddRecord.Name = "btnAddRecord";
//            btnAddRecord.Size = new Size(114, 34);
//            btnAddRecord.TabIndex = 12;
//            btnAddRecord.Text = "Add Record";
//            btnAddRecord.UseVisualStyleBackColor = true;
//            btnAddRecord.Click += btnAddRecord_Click;
//            // 
//            // btnShowValidationErrors
//            // 
//            btnShowValidationErrors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
//            btnShowValidationErrors.Enabled = false;
//            btnShowValidationErrors.Location = new Point(1100, 18);
//            btnShowValidationErrors.Margin = new Padding(2);
//            btnShowValidationErrors.Name = "btnShowValidationErrors";
//            btnShowValidationErrors.Size = new Size(180, 34);
//            btnShowValidationErrors.TabIndex = 11;
//            btnShowValidationErrors.Text = "Show Validation Errors";
//            btnShowValidationErrors.UseVisualStyleBackColor = true;
//            btnShowValidationErrors.Click += btnShowValidationErrors_Click;
//            // 
//            // btnClose
//            // 
//            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
//            btnClose.Location = new Point(695, 18);
//            btnClose.Margin = new Padding(2);
//            btnClose.Name = "btnClose";
//            btnClose.Size = new Size(66, 34);
//            btnClose.TabIndex = 9;
//            btnClose.Text = "Close";
//            btnClose.UseVisualStyleBackColor = true;
//            btnClose.Click += btnClose_Click;
//            // 
//            // btnDelete
//            // 
//            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
//            btnDelete.Location = new Point(494, 18);
//            btnDelete.Margin = new Padding(2);
//            btnDelete.Name = "btnDelete";
//            btnDelete.Size = new Size(197, 34);
//            btnDelete.TabIndex = 8;
//            btnDelete.Text = "Delete Selected Records";
//            btnDelete.UseVisualStyleBackColor = true;
//            btnDelete.Click += btnDelete_Click;
//            // 
//            // label1
//            // 
//            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
//            label1.AutoSize = true;
//            label1.Location = new Point(8, 714);
//            label1.Margin = new Padding(2, 0, 2, 0);
//            label1.Name = "label1";
//            label1.Size = new Size(73, 20);
//            label1.TabIndex = 9;
//            label1.Text = "Records : ";
//            // 
//            // txtRecordsCount
//            // 
//            txtRecordsCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
//            txtRecordsCount.BorderStyle = BorderStyle.None;
//            txtRecordsCount.Location = new Point(76, 714);
//            txtRecordsCount.Margin = new Padding(2);
//            txtRecordsCount.Name = "txtRecordsCount";
//            txtRecordsCount.ReadOnly = true;
//            txtRecordsCount.Size = new Size(85, 20);
//            txtRecordsCount.TabIndex = 10;
//            txtRecordsCount.Text = "100";
//            // 
//            // frmLandownersRecord
//            // 
//            AutoScaleDimensions = new SizeF(120F, 120F);
//            AutoScaleMode = AutoScaleMode.Dpi;
//            ClientSize = new Size(1314, 740);
//            Controls.Add(txtRecordsCount);
//            Controls.Add(btn_Export);
//            Controls.Add(label1);
//            Controls.Add(dataLandOwnersRecord);
//            Controls.Add(groupBox1);
//            DoubleBuffered = true;
//            Margin = new Padding(2, 4, 2, 4);
//            Name = "frmLandownersRecord";
//            ShowIcon = false;
//            StartPosition = FormStartPosition.CenterScreen;
//            Text = "Land Owners Record";
//            Load += frmLandownersRecord_Load_1;
//            ((System.ComponentModel.ISupportInitialize)dataLandOwnersRecord).EndInit();
//            groupBox1.ResumeLayout(false);
//            ResumeLayout(false);
//            PerformLayout();

//        }

//        #endregion

//        private DataGridView dataLandOwnersRecord;
//        private Button btnImportFromExcel;
//        private Button btn_Export;
//        private Button btnSave;
//        private GroupBox groupBox1;
//        private DataGridViewTextBoxColumn serialNo;
//        private DataGridViewTextBoxColumn parcelNo;
//        private DataGridViewTextBoxColumn district;
//        private DataGridViewTextBoxColumn gapa_napa;
//        private DataGridViewTextBoxColumn mapSheetNo;
//        private DataGridViewTextBoxColumn landOwner;
//        private DataGridViewTextBoxColumn father_spouse;
//        private DataGridViewTextBoxColumn citizenshipNo;
//        private DataGridViewTextBoxColumn tenant;
//        private DataGridViewTextBoxColumn address;
//        private DataGridViewTextBoxColumn landUse;
//        private DataGridViewTextBoxColumn area_sqm;
//        private DataGridViewTextBoxColumn area_rapd;
//        private DataGridViewTextBoxColumn area_bkd;
//        private DataGridViewTextBoxColumn mothNo;
//        private DataGridViewTextBoxColumn paanaNo;
//        private DataGridViewTextBoxColumn remarks;
//        private Label label1;
//        private Button btnClose;
//        private Button btnDelete;
//        private TextBox txtRecordsCount;
//        private Button btnShowValidationErrors;
//        private Button btnAddRecord;
//        private Button btnEditRecord;
//        private System.ComponentModel.BackgroundWorker backgroundWorker1;
//        private Button btnImport;
//    }
//}