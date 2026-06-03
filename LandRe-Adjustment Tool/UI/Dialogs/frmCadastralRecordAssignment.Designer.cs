namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmCadastralRecordAssignment
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private GroupBox modeGroup;
        private FlowLayoutPanel modePanel;
        private RadioButton rdoAutoAssign;
        private RadioButton rdoManualAssign;
        private Button btnOpenAutoAssignment;
        private Label lblFilterCaption;
        private ComboBox cboObjectFilter;
        private DataGridView dgvObjects;
        private FlowLayoutPanel navPanel;
        private Button btnPrevious;
        private Button btnNext;
        private CheckBox chkZoomToSelected;
        private Label lblSelectionInfo;
        private GroupBox manualGroup;
        private TableLayoutPanel manualLayout;
        private Label lblSelectedRecordCaption;
        private TextBox txtSelectedRecord;
        private CheckBox chkReplaceExisting;
        private FlowLayoutPanel manualActionPanel;
        private Button btnAssignParcel;
        private Button btnRemoveAssignment;
        private Button btnClearAssignments;
        private FlowLayoutPanel actionPanel;
        private Button btnClose;
        private Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            mainLayout = new TableLayoutPanel();
            modeGroup = new GroupBox();
            modePanel = new FlowLayoutPanel();
            rdoAutoAssign = new RadioButton();
            rdoManualAssign = new RadioButton();
            btnOpenAutoAssignment = new Button();
            lblFilterCaption = new Label();
            cboObjectFilter = new ComboBox();
            dgvObjects = new DataGridView();
            navPanel = new FlowLayoutPanel();
            btnPrevious = new Button();
            btnNext = new Button();
            chkZoomToSelected = new CheckBox();
            lblSelectionInfo = new Label();
            manualGroup = new GroupBox();
            manualLayout = new TableLayoutPanel();
            lblSelectedRecordCaption = new Label();
            txtSelectedRecord = new TextBox();
            chkReplaceExisting = new CheckBox();
            manualActionPanel = new FlowLayoutPanel();
            btnAssignParcel = new Button();
            btnRemoveAssignment = new Button();
            btnClearAssignments = new Button();
            lblStatus = new Label();
            actionPanel = new FlowLayoutPanel();
            btnClose = new Button();
            mainLayout.SuspendLayout();
            modeGroup.SuspendLayout();
            modePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvObjects).BeginInit();
            navPanel.SuspendLayout();
            manualGroup.SuspendLayout();
            manualLayout.SuspendLayout();
            manualActionPanel.SuspendLayout();
            actionPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(modeGroup, 0, 0);
            mainLayout.Controls.Add(lblFilterCaption, 0, 1);
            mainLayout.Controls.Add(cboObjectFilter, 1, 1);
            mainLayout.Controls.Add(dgvObjects, 0, 2);
            mainLayout.Controls.Add(navPanel, 1, 3);
            mainLayout.Controls.Add(lblSelectionInfo, 1, 4);
            mainLayout.Controls.Add(manualGroup, 0, 5);
            mainLayout.Controls.Add(lblStatus, 0, 6);
            mainLayout.Controls.Add(actionPanel, 1, 7);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(10);
            mainLayout.RowCount = 8;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 134F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainLayout.Size = new Size(640, 560);
            mainLayout.TabIndex = 0;
            mainLayout.Paint += mainLayout_Paint;
            // 
            // modeGroup
            // 
            mainLayout.SetColumnSpan(modeGroup, 2);
            modeGroup.Controls.Add(modePanel);
            modeGroup.Dock = DockStyle.Fill;
            modeGroup.Location = new Point(13, 13);
            modeGroup.Name = "modeGroup";
            modeGroup.Size = new Size(614, 70);
            modeGroup.TabIndex = 0;
            modeGroup.TabStop = false;
            modeGroup.Text = "Assignment mode";
            // 
            // modePanel
            // 
            modePanel.Controls.Add(rdoAutoAssign);
            modePanel.Controls.Add(rdoManualAssign);
            modePanel.Controls.Add(btnOpenAutoAssignment);
            modePanel.Dock = DockStyle.Fill;
            modePanel.Location = new Point(3, 23);
            modePanel.Name = "modePanel";
            modePanel.Padding = new Padding(8, 4, 0, 0);
            modePanel.Size = new Size(608, 44);
            modePanel.TabIndex = 0;
            // 
            // rdoAutoAssign
            // 
            rdoAutoAssign.Checked = true;
            rdoAutoAssign.Location = new Point(11, 7);
            rdoAutoAssign.Name = "rdoAutoAssign";
            rdoAutoAssign.Size = new Size(130, 28);
            rdoAutoAssign.TabIndex = 0;
            rdoAutoAssign.TabStop = true;
            rdoAutoAssign.Text = "Auto assign";
            rdoAutoAssign.UseVisualStyleBackColor = true;
            // 
            // rdoManualAssign
            // 
            rdoManualAssign.Location = new Point(147, 7);
            rdoManualAssign.Name = "rdoManualAssign";
            rdoManualAssign.Size = new Size(130, 28);
            rdoManualAssign.TabIndex = 1;
            rdoManualAssign.Text = "Manual assign";
            rdoManualAssign.UseVisualStyleBackColor = true;
            // 
            // btnOpenAutoAssignment
            // 
            btnOpenAutoAssignment.Location = new Point(283, 7);
            btnOpenAutoAssignment.Name = "btnOpenAutoAssignment";
            btnOpenAutoAssignment.Size = new Size(170, 30);
            btnOpenAutoAssignment.TabIndex = 2;
            btnOpenAutoAssignment.Text = "Auto Assignment...";
            btnOpenAutoAssignment.UseVisualStyleBackColor = true;
            // 
            // lblFilterCaption
            // 
            lblFilterCaption.Dock = DockStyle.Fill;
            lblFilterCaption.Location = new Point(13, 86);
            lblFilterCaption.Name = "lblFilterCaption";
            lblFilterCaption.Size = new Size(84, 38);
            lblFilterCaption.TabIndex = 1;
            lblFilterCaption.Text = "Show";
            lblFilterCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboObjectFilter
            // 
            cboObjectFilter.Dock = DockStyle.Fill;
            cboObjectFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboObjectFilter.FormattingEnabled = true;
            cboObjectFilter.Location = new Point(103, 89);
            cboObjectFilter.Name = "cboObjectFilter";
            cboObjectFilter.Size = new Size(524, 28);
            cboObjectFilter.TabIndex = 1;
            // 
            // dgvObjects
            // 
            dgvObjects.AllowUserToAddRows = false;
            dgvObjects.AllowUserToDeleteRows = false;
            dgvObjects.AllowUserToResizeRows = false;
            dgvObjects.BackgroundColor = SystemColors.Window;
            dgvObjects.BorderStyle = BorderStyle.Fixed3D;
            dgvObjects.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            mainLayout.SetColumnSpan(dgvObjects, 2);
            dgvObjects.Dock = DockStyle.Fill;
            dgvObjects.Location = new Point(13, 127);
            dgvObjects.MultiSelect = false;
            dgvObjects.Name = "dgvObjects";
            dgvObjects.ReadOnly = true;
            dgvObjects.RowHeadersVisible = false;
            dgvObjects.RowHeadersWidth = 51;
            dgvObjects.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvObjects.Size = new Size(614, 156);
            dgvObjects.TabIndex = 2;
            // 
            // navPanel
            // 
            navPanel.Controls.Add(btnPrevious);
            navPanel.Controls.Add(btnNext);
            navPanel.Controls.Add(chkZoomToSelected);
            navPanel.Dock = DockStyle.Fill;
            navPanel.Location = new Point(103, 289);
            navPanel.Name = "navPanel";
            navPanel.Size = new Size(524, 30);
            navPanel.TabIndex = 3;
            // 
            // btnPrevious
            // 
            btnPrevious.Location = new Point(3, 3);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new Size(90, 30);
            btnPrevious.TabIndex = 0;
            btnPrevious.Text = "Previous";
            btnPrevious.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            btnNext.Location = new Point(99, 3);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(90, 30);
            btnNext.TabIndex = 1;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // chkZoomToSelected
            // 
            chkZoomToSelected.Checked = true;
            chkZoomToSelected.CheckState = CheckState.Checked;
            chkZoomToSelected.Location = new Point(195, 3);
            chkZoomToSelected.Name = "chkZoomToSelected";
            chkZoomToSelected.Size = new Size(150, 28);
            chkZoomToSelected.TabIndex = 2;
            chkZoomToSelected.Text = "Zoom to selected";
            chkZoomToSelected.UseVisualStyleBackColor = true;
            // 
            // lblSelectionInfo
            // 
            lblSelectionInfo.Dock = DockStyle.Fill;
            lblSelectionInfo.ForeColor = Color.DimGray;
            lblSelectionInfo.Location = new Point(103, 322);
            lblSelectionInfo.Name = "lblSelectionInfo";
            lblSelectionInfo.Size = new Size(524, 24);
            lblSelectionInfo.TabIndex = 4;
            lblSelectionInfo.Text = "Select an imported parcel object.";
            lblSelectionInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // manualGroup
            // 
            mainLayout.SetColumnSpan(manualGroup, 2);
            manualGroup.Controls.Add(manualLayout);
            manualGroup.Dock = DockStyle.Fill;
            manualGroup.Location = new Point(13, 349);
            manualGroup.Name = "manualGroup";
            manualGroup.Size = new Size(614, 128);
            manualGroup.TabIndex = 4;
            manualGroup.TabStop = false;
            manualGroup.Text = "Manual assignment";
            // 
            // manualLayout
            // 
            manualLayout.ColumnCount = 2;
            manualLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            manualLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            manualLayout.Controls.Add(lblSelectedRecordCaption, 0, 0);
            manualLayout.Controls.Add(txtSelectedRecord, 1, 0);
            manualLayout.Controls.Add(chkReplaceExisting, 1, 1);
            manualLayout.Controls.Add(manualActionPanel, 1, 2);
            manualLayout.Dock = DockStyle.Fill;
            manualLayout.Location = new Point(3, 23);
            manualLayout.Name = "manualLayout";
            manualLayout.Padding = new Padding(8, 4, 8, 8);
            manualLayout.RowCount = 3;
            manualLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            manualLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            manualLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            manualLayout.Size = new Size(608, 102);
            manualLayout.TabIndex = 0;
            // 
            // lblSelectedRecordCaption
            // 
            lblSelectedRecordCaption.Dock = DockStyle.Fill;
            lblSelectedRecordCaption.Location = new Point(11, 4);
            lblSelectedRecordCaption.Name = "lblSelectedRecordCaption";
            lblSelectedRecordCaption.Size = new Size(114, 37);
            lblSelectedRecordCaption.TabIndex = 0;
            lblSelectedRecordCaption.Text = "Assigned record";
            lblSelectedRecordCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSelectedRecord
            // 
            txtSelectedRecord.Dock = DockStyle.Fill;
            txtSelectedRecord.Location = new Point(131, 7);
            txtSelectedRecord.Name = "txtSelectedRecord";
            txtSelectedRecord.ReadOnly = true;
            txtSelectedRecord.Size = new Size(466, 27);
            txtSelectedRecord.TabIndex = 0;
            // 
            // chkReplaceExisting
            // 
            chkReplaceExisting.Dock = DockStyle.Fill;
            chkReplaceExisting.Location = new Point(131, 44);
            chkReplaceExisting.Name = "chkReplaceExisting";
            chkReplaceExisting.Size = new Size(466, 28);
            chkReplaceExisting.TabIndex = 1;
            chkReplaceExisting.Text = "Replace existing record-to-map assignments";
            chkReplaceExisting.UseVisualStyleBackColor = true;
            // 
            // manualActionPanel
            // 
            manualActionPanel.Controls.Add(btnAssignParcel);
            manualActionPanel.Controls.Add(btnRemoveAssignment);
            manualActionPanel.Controls.Add(btnClearAssignments);
            manualActionPanel.Dock = DockStyle.Fill;
            manualActionPanel.Location = new Point(131, 78);
            manualActionPanel.Name = "manualActionPanel";
            manualActionPanel.Size = new Size(466, 25);
            manualActionPanel.TabIndex = 2;
            // 
            // btnAssignParcel
            // 
            btnAssignParcel.Location = new Point(3, 3);
            btnAssignParcel.Name = "btnAssignParcel";
            btnAssignParcel.Size = new Size(130, 30);
            btnAssignParcel.TabIndex = 0;
            btnAssignParcel.Text = "Assign Parcel...";
            btnAssignParcel.UseVisualStyleBackColor = true;
            // 
            // btnRemoveAssignment
            // 
            btnRemoveAssignment.Location = new Point(139, 3);
            btnRemoveAssignment.Name = "btnRemoveAssignment";
            btnRemoveAssignment.Size = new Size(150, 30);
            btnRemoveAssignment.TabIndex = 1;
            btnRemoveAssignment.Text = "Remove Selected";
            btnRemoveAssignment.UseVisualStyleBackColor = true;
            // 
            // btnClearAssignments
            // 
            btnClearAssignments.Location = new Point(295, 3);
            btnClearAssignments.Name = "btnClearAssignments";
            btnClearAssignments.Size = new Size(130, 30);
            btnClearAssignments.TabIndex = 2;
            btnClearAssignments.Text = "Remove All";
            btnClearAssignments.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            mainLayout.SetColumnSpan(lblStatus, 2);
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Location = new Point(13, 480);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(614, 30);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // actionPanel
            // 
            actionPanel.Controls.Add(btnClose);
            actionPanel.Dock = DockStyle.Fill;
            actionPanel.FlowDirection = FlowDirection.RightToLeft;
            actionPanel.Location = new Point(103, 513);
            actionPanel.Name = "actionPanel";
            actionPanel.Size = new Size(524, 34);
            actionPanel.TabIndex = 5;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(431, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 32);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmCadastralRecordAssignment
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(640, 560);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCadastralRecordAssignment";
            StartPosition = FormStartPosition.Manual;
            Text = "Assign Cadastral Records";
            mainLayout.ResumeLayout(false);
            modeGroup.ResumeLayout(false);
            modePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvObjects).EndInit();
            navPanel.ResumeLayout(false);
            manualGroup.ResumeLayout(false);
            manualLayout.ResumeLayout(false);
            manualLayout.PerformLayout();
            manualActionPanel.ResumeLayout(false);
            actionPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
