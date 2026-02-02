namespace Land_Readjustment_Tool.Forms
{
    partial class frmImportManager
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
            lblProgress = new Label();
            lblStatusBar = new Label();
            progressBar = new ProgressBar();
            grpStep2 = new GroupBox();
            label2 = new Label();
            btnClearMapping = new Button();
            btnAutoMap = new Button();
            btnApplyMapping = new Button();
            dgvMapping = new DataGridView();
            grpStep1 = new GroupBox();
            lblStatusBar1 = new Label();
            btnImportData = new Button();
            cbSelectSheet = new ComboBox();
            label1 = new Label();
            btnLoadFile = new Button();
            cmbFileType = new ComboBox();
            lblFileType = new Label();
            btnBrowse = new Button();
            txtFilePath = new TextBox();
            lblSelectFile = new Label();
            grpStep4 = new GroupBox();
            btnCancel = new Button();
            btnResolveOwnerDuplication = new Button();
            btnSaveToDatabase = new Button();
            btnValidate = new Button();
            lblTotalRecordsLabel = new Label();
            lblTotalRecords = new Label();
            lblRecordsReadyLabel = new Label();
            lblRecordsReady = new Label();
            lblValidationLabel = new Label();
            lblValidationStatus = new Label();
            grpStep3 = new GroupBox();
            btnReMapFields = new Button();
            btnRemoveSelected = new Button();
            dgvRecords = new DataGridView();
            btnFixErrors = new Button();
            btnEditRecord = new Button();
            SCImportManager = new SplitContainer();
            pnlStepStatus = new Panel();
            lblStep1Status = new Label();
            lblStep2Status = new Label();
            lblStep3Status = new Label();
            lblStep4Status = new Label();
            grpStep2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMapping).BeginInit();
            grpStep1.SuspendLayout();
            grpStep4.SuspendLayout();
            grpStep3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SCImportManager).BeginInit();
            SCImportManager.Panel1.SuspendLayout();
            SCImportManager.Panel2.SuspendLayout();
            SCImportManager.SuspendLayout();
            pnlStepStatus.SuspendLayout();
            SuspendLayout();
            // 
            // lblProgress
            // 
            lblProgress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(12, 530);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(117, 20);
            lblProgress.TabIndex = 8;
            lblProgress.Text = "Import Progress:";
            // 
            // lblStatusBar
            // 
            lblStatusBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatusBar.AutoSize = true;
            lblStatusBar.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblStatusBar.Location = new Point(12, 563);
            lblStatusBar.Name = "lblStatusBar";
            lblStatusBar.Size = new Size(269, 20);
            lblStatusBar.TabIndex = 9;
            lblStatusBar.Text = "Status: Ready to import land owner data.";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(135, 527);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(1371, 25);
            progressBar.TabIndex = 7;
            // 
            // grpStep2
            // 
            grpStep2.Controls.Add(label2);
            grpStep2.Controls.Add(btnClearMapping);
            grpStep2.Controls.Add(btnAutoMap);
            grpStep2.Controls.Add(btnApplyMapping);
            grpStep2.Controls.Add(dgvMapping);
            grpStep2.Dock = DockStyle.Fill;
            grpStep2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpStep2.Location = new Point(0, 0);
            grpStep2.Name = "grpStep2";
            grpStep2.Size = new Size(432, 521);
            grpStep2.TabIndex = 2;
            grpStep2.TabStop = false;
            grpStep2.Text = "Step 2: Map Fields";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(6, 33);
            label2.Name = "label2";
            label2.Size = new Size(299, 20);
            label2.TabIndex = 43;
            label2.Text = "*Select the appropriate source field to map.";
            // 
            // btnClearMapping
            // 
            btnClearMapping.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClearMapping.Font = new Font("Segoe UI", 9F);
            btnClearMapping.Location = new Point(218, 474);
            btnClearMapping.Name = "btnClearMapping";
            btnClearMapping.Size = new Size(79, 35);
            btnClearMapping.TabIndex = 43;
            btnClearMapping.Text = "Clear";
            btnClearMapping.UseVisualStyleBackColor = true;
            btnClearMapping.Click += btnClearMapping_Click;
            // 
            // btnAutoMap
            // 
            btnAutoMap.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAutoMap.Font = new Font("Segoe UI", 9F);
            btnAutoMap.Location = new Point(121, 474);
            btnAutoMap.Name = "btnAutoMap";
            btnAutoMap.Size = new Size(91, 35);
            btnAutoMap.TabIndex = 42;
            btnAutoMap.Text = "Auto Map";
            btnAutoMap.UseVisualStyleBackColor = true;
            btnAutoMap.Click += btnAutoMap_Click;
            // 
            // btnApplyMapping
            // 
            btnApplyMapping.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnApplyMapping.BackColor = SystemColors.ControlLightLight;
            btnApplyMapping.Font = new Font("Segoe UI", 9F);
            btnApplyMapping.Location = new Point(303, 474);
            btnApplyMapping.Name = "btnApplyMapping";
            btnApplyMapping.Size = new Size(122, 35);
            btnApplyMapping.TabIndex = 41;
            btnApplyMapping.Text = "Apply Mapping";
            btnApplyMapping.UseVisualStyleBackColor = false;
            btnApplyMapping.Click += btnApplyMapping_Click;
            // 
            // dgvMapping
            // 
            dgvMapping.AllowUserToResizeColumns = false;
            dgvMapping.AllowUserToResizeRows = false;
            dgvMapping.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMapping.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvMapping.BackgroundColor = SystemColors.ControlLight;
            dgvMapping.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dgvMapping.DefaultCellStyle = dataGridViewCellStyle4;
            dgvMapping.Location = new Point(3, 63);
            dgvMapping.Name = "dgvMapping";
            dgvMapping.RowHeadersWidth = 51;
            dataGridViewCellStyle5.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dgvMapping.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvMapping.Size = new Size(425, 405);
            dgvMapping.TabIndex = 40;
            // 
            // grpStep1
            // 
            grpStep1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpStep1.Controls.Add(lblStatusBar1);
            grpStep1.Controls.Add(btnImportData);
            grpStep1.Controls.Add(cbSelectSheet);
            grpStep1.Controls.Add(label1);
            grpStep1.Controls.Add(btnLoadFile);
            grpStep1.Controls.Add(cmbFileType);
            grpStep1.Controls.Add(lblFileType);
            grpStep1.Controls.Add(btnBrowse);
            grpStep1.Controls.Add(txtFilePath);
            grpStep1.Controls.Add(lblSelectFile);
            grpStep1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpStep1.Location = new Point(3, 3);
            grpStep1.Name = "grpStep1";
            grpStep1.Size = new Size(570, 172);
            grpStep1.TabIndex = 1;
            grpStep1.TabStop = false;
            grpStep1.Text = "Step 1: Load Data File";
            // 
            // lblStatusBar1
            // 
            lblStatusBar1.AutoSize = true;
            lblStatusBar1.Font = new Font("Segoe UI", 9F);
            lblStatusBar1.Location = new Point(19, 142);
            lblStatusBar1.Name = "lblStatusBar1";
            lblStatusBar1.Size = new Size(89, 20);
            lblStatusBar1.TabIndex = 42;
            lblStatusBar1.Text = "<statusbar>";
            // 
            // btnImportData
            // 
            btnImportData.Font = new Font("Segoe UI", 9F);
            btnImportData.Location = new Point(366, 101);
            btnImportData.Name = "btnImportData";
            btnImportData.Size = new Size(108, 30);
            btnImportData.TabIndex = 41;
            btnImportData.Text = "Import Data";
            btnImportData.UseVisualStyleBackColor = true;
            btnImportData.Click += btnImportData_Click;
            // 
            // cbSelectSheet
            // 
            cbSelectSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            cbSelectSheet.Font = new Font("Segoe UI", 9F);
            cbSelectSheet.FormattingEnabled = true;
            cbSelectSheet.Location = new Point(120, 103);
            cbSelectSheet.Name = "cbSelectSheet";
            cbSelectSheet.Size = new Size(240, 28);
            cbSelectSheet.TabIndex = 40;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(15, 106);
            label1.Name = "label1";
            label1.Size = new Size(93, 20);
            label1.TabIndex = 7;
            label1.Text = "Select Sheet:";
            // 
            // btnLoadFile
            // 
            btnLoadFile.Font = new Font("Segoe UI", 9F);
            btnLoadFile.Location = new Point(462, 35);
            btnLoadFile.Name = "btnLoadFile";
            btnLoadFile.Size = new Size(90, 30);
            btnLoadFile.TabIndex = 5;
            btnLoadFile.Text = "Load File";
            btnLoadFile.UseVisualStyleBackColor = true;
            btnLoadFile.Click += btnLoadFile_Click;
            // 
            // cmbFileType
            // 
            cmbFileType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFileType.Enabled = false;
            cmbFileType.Font = new Font("Segoe UI", 9F);
            cmbFileType.FormattingEnabled = true;
            cmbFileType.Items.AddRange(new object[] { "Excel (.xlsx)", "CSV (.csv)" });
            cmbFileType.Location = new Point(120, 69);
            cmbFileType.Name = "cmbFileType";
            cmbFileType.Size = new Size(240, 28);
            cmbFileType.TabIndex = 4;
            // 
            // lblFileType
            // 
            lblFileType.AutoSize = true;
            lblFileType.Font = new Font("Segoe UI", 9F);
            lblFileType.Location = new Point(15, 72);
            lblFileType.Name = "lblFileType";
            lblFileType.Size = new Size(70, 20);
            lblFileType.TabIndex = 3;
            lblFileType.Text = "File Type:";
            // 
            // btnBrowse
            // 
            btnBrowse.Font = new Font("Segoe UI", 9F);
            btnBrowse.Location = new Point(366, 35);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(90, 30);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtFilePath
            // 
            txtFilePath.Font = new Font("Segoe UI", 9F);
            txtFilePath.Location = new Point(120, 36);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.ReadOnly = true;
            txtFilePath.Size = new Size(240, 27);
            txtFilePath.TabIndex = 1;
            // 
            // lblSelectFile
            // 
            lblSelectFile.AutoSize = true;
            lblSelectFile.Font = new Font("Segoe UI", 9F);
            lblSelectFile.Location = new Point(15, 38);
            lblSelectFile.Name = "lblSelectFile";
            lblSelectFile.Size = new Size(79, 20);
            lblSelectFile.TabIndex = 0;
            lblSelectFile.Text = "Select File:";
            // 
            // grpStep4
            // 
            grpStep4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpStep4.Controls.Add(btnCancel);
            grpStep4.Controls.Add(btnResolveOwnerDuplication);
            grpStep4.Controls.Add(btnSaveToDatabase);
            grpStep4.Controls.Add(btnValidate);
            grpStep4.Controls.Add(lblTotalRecordsLabel);
            grpStep4.Controls.Add(lblTotalRecords);
            grpStep4.Controls.Add(lblRecordsReadyLabel);
            grpStep4.Controls.Add(lblRecordsReady);
            grpStep4.Controls.Add(lblValidationLabel);
            grpStep4.Controls.Add(lblValidationStatus);
            grpStep4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpStep4.Location = new Point(579, 3);
            grpStep4.Name = "grpStep4";
            grpStep4.Size = new Size(494, 172);
            grpStep4.TabIndex = 4;
            grpStep4.TabStop = false;
            grpStep4.Text = "\\";
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 9F);
            btnCancel.Location = new Point(400, 136);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(87, 33);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnResolveOwnerDuplication
            // 
            btnResolveOwnerDuplication.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnResolveOwnerDuplication.BackColor = SystemColors.ControlLightLight;
            btnResolveOwnerDuplication.Font = new Font("Segoe UI", 9F);
            btnResolveOwnerDuplication.Location = new Point(114, 136);
            btnResolveOwnerDuplication.Name = "btnResolveOwnerDuplication";
            btnResolveOwnerDuplication.Size = new Size(198, 33);
            btnResolveOwnerDuplication.TabIndex = 47;
            btnResolveOwnerDuplication.Text = "Resolve Owner Duplication";
            btnResolveOwnerDuplication.UseVisualStyleBackColor = false;
            btnResolveOwnerDuplication.Click += btnResolveOwnerDuplication_Click;
            // 
            // btnSaveToDatabase
            // 
            btnSaveToDatabase.BackColor = SystemColors.ControlLightLight;
            btnSaveToDatabase.Enabled = false;
            btnSaveToDatabase.Font = new Font("Segoe UI", 9F);
            btnSaveToDatabase.Location = new Point(318, 136);
            btnSaveToDatabase.Name = "btnSaveToDatabase";
            btnSaveToDatabase.Size = new Size(76, 33);
            btnSaveToDatabase.TabIndex = 10;
            btnSaveToDatabase.Text = "Save";
            btnSaveToDatabase.UseVisualStyleBackColor = false;
            btnSaveToDatabase.Click += btnSaveToDatabase_Click;
            // 
            // btnValidate
            // 
            btnValidate.Font = new Font("Segoe UI", 9F);
            btnValidate.Location = new Point(6, 136);
            btnValidate.Name = "btnValidate";
            btnValidate.Size = new Size(102, 33);
            btnValidate.TabIndex = 9;
            btnValidate.Text = "Re-Validate";
            btnValidate.UseVisualStyleBackColor = true;
            btnValidate.Click += btnValidate_Click;
            // 
            // lblTotalRecordsLabel
            // 
            lblTotalRecordsLabel.AutoSize = true;
            lblTotalRecordsLabel.Font = new Font("Segoe UI", 9F);
            lblTotalRecordsLabel.Location = new Point(15, 90);
            lblTotalRecordsLabel.Name = "lblTotalRecordsLabel";
            lblTotalRecordsLabel.Size = new Size(102, 20);
            lblTotalRecordsLabel.TabIndex = 7;
            lblTotalRecordsLabel.Text = "Total Records:";
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.AutoSize = true;
            lblTotalRecords.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalRecords.Location = new Point(200, 90);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(18, 20);
            lblTotalRecords.TabIndex = 6;
            lblTotalRecords.Text = "0";
            // 
            // lblRecordsReadyLabel
            // 
            lblRecordsReadyLabel.AutoSize = true;
            lblRecordsReadyLabel.Font = new Font("Segoe UI", 9F);
            lblRecordsReadyLabel.Location = new Point(15, 60);
            lblRecordsReadyLabel.Name = "lblRecordsReadyLabel";
            lblRecordsReadyLabel.Size = new Size(163, 20);
            lblRecordsReadyLabel.TabIndex = 5;
            lblRecordsReadyLabel.Text = "Records Ready to Save:";
            // 
            // lblRecordsReady
            // 
            lblRecordsReady.AutoSize = true;
            lblRecordsReady.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRecordsReady.Location = new Point(200, 60);
            lblRecordsReady.Name = "lblRecordsReady";
            lblRecordsReady.Size = new Size(18, 20);
            lblRecordsReady.TabIndex = 4;
            lblRecordsReady.Text = "0";
            // 
            // lblValidationLabel
            // 
            lblValidationLabel.AutoSize = true;
            lblValidationLabel.Font = new Font("Segoe UI", 9F);
            lblValidationLabel.Location = new Point(15, 30);
            lblValidationLabel.Name = "lblValidationLabel";
            lblValidationLabel.Size = new Size(79, 20);
            lblValidationLabel.TabIndex = 3;
            lblValidationLabel.Text = "Validation:";
            // 
            // lblValidationStatus
            // 
            lblValidationStatus.AutoSize = true;
            lblValidationStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblValidationStatus.ForeColor = Color.Gray;
            lblValidationStatus.Location = new Point(200, 30);
            lblValidationStatus.Name = "lblValidationStatus";
            lblValidationStatus.Size = new Size(105, 20);
            lblValidationStatus.TabIndex = 2;
            lblValidationStatus.Text = "Not Validated";
            // 
            // grpStep3
            // 
            grpStep3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpStep3.Controls.Add(btnReMapFields);
            grpStep3.Controls.Add(btnRemoveSelected);
            grpStep3.Controls.Add(dgvRecords);
            grpStep3.Controls.Add(btnFixErrors);
            grpStep3.Controls.Add(btnEditRecord);
            grpStep3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpStep3.Location = new Point(0, 181);
            grpStep3.Name = "grpStep3";
            grpStep3.Size = new Size(1073, 334);
            grpStep3.TabIndex = 5;
            grpStep3.TabStop = false;
            grpStep3.Text = "Step 3: Review && Edit Records";
            // 
            // btnReMapFields
            // 
            btnReMapFields.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReMapFields.Font = new Font("Segoe UI", 9F);
            btnReMapFields.Location = new Point(930, 293);
            btnReMapFields.Name = "btnReMapFields";
            btnReMapFields.Size = new Size(137, 35);
            btnReMapFields.TabIndex = 48;
            btnReMapFields.Text = "Re-Map Fields";
            btnReMapFields.UseVisualStyleBackColor = true;
            btnReMapFields.Click += btnReMapFields_Click;
            // 
            // btnRemoveSelected
            // 
            btnRemoveSelected.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRemoveSelected.Font = new Font("Segoe UI", 9F);
            btnRemoveSelected.Location = new Point(135, 293);
            btnRemoveSelected.Name = "btnRemoveSelected";
            btnRemoveSelected.Size = new Size(146, 35);
            btnRemoveSelected.TabIndex = 46;
            btnRemoveSelected.Text = "Delete Selected";
            btnRemoveSelected.UseVisualStyleBackColor = true;
            btnRemoveSelected.Click += btnRemoveSelected_Click;
            // 
            // dgvRecords
            // 
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvRecords.BackgroundColor = SystemColors.ControlLight;
            dgvRecords.ColumnHeadersHeight = 29;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = SystemColors.Window;
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle6.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
            dgvRecords.DefaultCellStyle = dataGridViewCellStyle6;
            dgvRecords.Location = new Point(12, 26);
            dgvRecords.Name = "dgvRecords";
            dgvRecords.ReadOnly = true;
            dgvRecords.RowHeadersWidth = 51;
            dgvRecords.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.Size = new Size(1055, 261);
            dgvRecords.TabIndex = 0;
            // 
            // btnFixErrors
            // 
            btnFixErrors.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnFixErrors.BackColor = SystemColors.ControlLightLight;
            btnFixErrors.Font = new Font("Segoe UI", 9F);
            btnFixErrors.Location = new Point(287, 293);
            btnFixErrors.Name = "btnFixErrors";
            btnFixErrors.Size = new Size(216, 35);
            btnFixErrors.TabIndex = 44;
            btnFixErrors.Text = "Show/Fix validation Errors";
            btnFixErrors.UseVisualStyleBackColor = false;
            btnFixErrors.Click += btnFixErrors_Click;
            // 
            // btnEditRecord
            // 
            btnEditRecord.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEditRecord.Font = new Font("Segoe UI", 9F);
            btnEditRecord.Location = new Point(11, 293);
            btnEditRecord.Name = "btnEditRecord";
            btnEditRecord.Size = new Size(118, 35);
            btnEditRecord.TabIndex = 45;
            btnEditRecord.Text = "Edit selected";
            btnEditRecord.UseVisualStyleBackColor = true;
            btnEditRecord.Click += btnEditRecord_Click;
            // 
            // SCImportManager
            // 
            SCImportManager.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SCImportManager.Location = new Point(0, 0);
            SCImportManager.Name = "SCImportManager";
            // 
            // SCImportManager.Panel1
            // 
            SCImportManager.Panel1.Controls.Add(grpStep3);
            SCImportManager.Panel1.Controls.Add(grpStep4);
            SCImportManager.Panel1.Controls.Add(grpStep1);
            SCImportManager.Panel1MinSize = 895;
            // 
            // SCImportManager.Panel2
            // 
            SCImportManager.Panel2.Controls.Add(grpStep2);
            SCImportManager.Panel2MinSize = 300;
            SCImportManager.Size = new Size(1512, 521);
            SCImportManager.SplitterDistance = 1076;
            SCImportManager.TabIndex = 0;
            // 
            // pnlStepStatus
            // 
            pnlStepStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlStepStatus.BackColor = SystemColors.ControlLight;
            pnlStepStatus.BorderStyle = BorderStyle.FixedSingle;
            pnlStepStatus.Controls.Add(lblStep1Status);
            pnlStepStatus.Controls.Add(lblStep2Status);
            pnlStepStatus.Controls.Add(lblStep3Status);
            pnlStepStatus.Controls.Add(lblStep4Status);
            pnlStepStatus.Location = new Point(12, 590);
            pnlStepStatus.Name = "pnlStepStatus";
            pnlStepStatus.Size = new Size(1488, 35);
            pnlStepStatus.TabIndex = 10;
            // 
            // lblStep1Status
            // 
            lblStep1Status.AutoSize = true;
            lblStep1Status.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStep1Status.ForeColor = Color.Gray;
            lblStep1Status.Location = new Point(10, 7);
            lblStep1Status.Name = "lblStep1Status";
            lblStep1Status.Size = new Size(144, 20);
            lblStep1Status.TabIndex = 0;
            lblStep1Status.Text = "Step 1: ⏳ Pending";
            // 
            // lblStep2Status
            // 
            lblStep2Status.AutoSize = true;
            lblStep2Status.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStep2Status.ForeColor = Color.Gray;
            lblStep2Status.Location = new Point(307, 7);
            lblStep2Status.Name = "lblStep2Status";
            lblStep2Status.Size = new Size(144, 20);
            lblStep2Status.TabIndex = 1;
            lblStep2Status.Text = "Step 2: ⏳ Pending";
            // 
            // lblStep3Status
            // 
            lblStep3Status.AutoSize = true;
            lblStep3Status.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStep3Status.ForeColor = Color.Gray;
            lblStep3Status.Location = new Point(604, 7);
            lblStep3Status.Name = "lblStep3Status";
            lblStep3Status.Size = new Size(144, 20);
            lblStep3Status.TabIndex = 2;
            lblStep3Status.Text = "Step 3: ⏳ Pending";
            // 
            // lblStep4Status
            // 
            lblStep4Status.AutoSize = true;
            lblStep4Status.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStep4Status.ForeColor = Color.Gray;
            lblStep4Status.Location = new Point(901, 7);
            lblStep4Status.Name = "lblStep4Status";
            lblStep4Status.Size = new Size(144, 20);
            lblStep4Status.TabIndex = 3;
            lblStep4Status.Text = "Step 4: ⏳ Pending";
            // 
            // frmImportManager
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1512, 635);
            Controls.Add(pnlStepStatus);
            Controls.Add(lblStatusBar);
            Controls.Add(lblProgress);
            Controls.Add(progressBar);
            Controls.Add(SCImportManager);
            Name = "frmImportManager";
            Text = "Import Manager";
            Load += frmImportManager_Load;
            grpStep2.ResumeLayout(false);
            grpStep2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMapping).EndInit();
            grpStep1.ResumeLayout(false);
            grpStep1.PerformLayout();
            grpStep4.ResumeLayout(false);
            grpStep4.PerformLayout();
            grpStep3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            SCImportManager.Panel1.ResumeLayout(false);
            SCImportManager.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SCImportManager).EndInit();
            SCImportManager.ResumeLayout(false);
            pnlStepStatus.ResumeLayout(false);
            pnlStepStatus.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        //private Button btnClearMapping;
        //private Button btnAutoMap;
        //private Button btnApplyMapping;
        private Label lblProgress;
        private Label lblStatusBar;
        private ProgressBar progressBar;
        private GroupBox grpStep2;
        private Button btnAutoMap;
        private Button btnApplyMapping;
        private DataGridView dgvMapping;
        private GroupBox grpStep1;
        private Button btnImportData;
        private ComboBox cbSelectSheet;
        private Label label1;
        private Label lblStatusBar1;
        private Button btnLoadFile;
        private ComboBox cmbFileType;
        private Label lblFileType;
        private Button btnBrowse;
        private TextBox txtFilePath;
        private Label lblSelectFile;
        private GroupBox grpStep4;
        private Button btnCancel;
        private Button btnSaveToDatabase;
        private Button btnValidate;
        private Label lblTotalRecordsLabel;
        private Label lblTotalRecords;
        private Label lblRecordsReadyLabel;
        private Label lblRecordsReady;
        private Label lblValidationLabel;
        private Label lblValidationStatus;
        private GroupBox grpStep3;
        private Button btnRemoveSelected;
        private DataGridView dgvRecords;
        private Button btnFixErrors;
        private Button btnEditRecord;
        private SplitContainer SCImportManager;
        private Label label2;
        private Button btnResolveOwnerDuplication;
        private Button btnReMapFields;
        private Button btnClearMapping;
        private Panel pnlStepStatus;
        private Label lblStep1Status;
        private Label lblStep2Status;
        private Label lblStep3Status;
        private Label lblStep4Status;
    }
}