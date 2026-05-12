namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmCadastralRecordAssignment
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label lblFilterCaption;
        private ComboBox cboObjectFilter;
        private Label lblObjectCaption;
        private ListBox lstObjects;
        private FlowLayoutPanel navPanel;
        private Button btnPrevious;
        private Button btnNext;
        private CheckBox chkZoomToSelected;
        private Label lblSelectionInfo;
        private Label lblLayerMappingCaption;
        private DataGridView dgvLayerMapSheets;
        private Label lblMapSheetCaption;
        private ComboBox cboMapSheet;
        private Label lblParcelCaption;
        private ComboBox cboParcel;
        private CheckBox chkReplaceExisting;
        private FlowLayoutPanel actionPanel;
        private Button btnAssign;
        private Button btnAutoAssign;
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
            lblFilterCaption = new Label();
            cboObjectFilter = new ComboBox();
            lblObjectCaption = new Label();
            lstObjects = new ListBox();
            navPanel = new FlowLayoutPanel();
            btnPrevious = new Button();
            btnNext = new Button();
            chkZoomToSelected = new CheckBox();
            lblSelectionInfo = new Label();
            lblLayerMappingCaption = new Label();
            dgvLayerMapSheets = new DataGridView();
            lblMapSheetCaption = new Label();
            cboMapSheet = new ComboBox();
            lblParcelCaption = new Label();
            cboParcel = new ComboBox();
            chkReplaceExisting = new CheckBox();
            lblStatus = new Label();
            actionPanel = new FlowLayoutPanel();
            btnClose = new Button();
            btnAutoAssign = new Button();
            btnAssign = new Button();
            mainLayout.SuspendLayout();
            navPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLayerMapSheets).BeginInit();
            actionPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblFilterCaption, 0, 0);
            mainLayout.Controls.Add(cboObjectFilter, 1, 0);
            mainLayout.Controls.Add(lblObjectCaption, 0, 1);
            mainLayout.Controls.Add(lstObjects, 1, 1);
            mainLayout.Controls.Add(navPanel, 1, 3);
            mainLayout.Controls.Add(lblSelectionInfo, 1, 4);
            mainLayout.Controls.Add(lblLayerMappingCaption, 0, 5);
            mainLayout.Controls.Add(dgvLayerMapSheets, 1, 5);
            mainLayout.Controls.Add(lblMapSheetCaption, 0, 7);
            mainLayout.Controls.Add(cboMapSheet, 1, 7);
            mainLayout.Controls.Add(lblParcelCaption, 0, 8);
            mainLayout.Controls.Add(cboParcel, 1, 8);
            mainLayout.Controls.Add(chkReplaceExisting, 1, 9);
            mainLayout.Controls.Add(lblStatus, 1, 10);
            mainLayout.Controls.Add(actionPanel, 1, 11);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(14);
            mainLayout.RowCount = 12;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            mainLayout.Size = new Size(760, 720);
            mainLayout.TabIndex = 0;
            // 
            // lblFilterCaption
            // 
            lblFilterCaption.Dock = DockStyle.Fill;
            lblFilterCaption.Location = new Point(17, 14);
            lblFilterCaption.Name = "lblFilterCaption";
            lblFilterCaption.Size = new Size(164, 38);
            lblFilterCaption.TabIndex = 0;
            lblFilterCaption.Text = "Show";
            lblFilterCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboObjectFilter
            // 
            cboObjectFilter.Dock = DockStyle.Fill;
            cboObjectFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboObjectFilter.FormattingEnabled = true;
            cboObjectFilter.Location = new Point(187, 17);
            cboObjectFilter.Name = "cboObjectFilter";
            cboObjectFilter.Size = new Size(556, 28);
            cboObjectFilter.TabIndex = 0;
            // 
            // lblObjectCaption
            // 
            lblObjectCaption.Dock = DockStyle.Fill;
            lblObjectCaption.Location = new Point(17, 52);
            lblObjectCaption.Name = "lblObjectCaption";
            lblObjectCaption.Size = new Size(164, 30);
            lblObjectCaption.TabIndex = 1;
            lblObjectCaption.Text = "Canvas objects";
            lblObjectCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lstObjects
            // 
            lstObjects.Dock = DockStyle.Fill;
            lstObjects.FormattingEnabled = true;
            lstObjects.Location = new Point(187, 55);
            lstObjects.Name = "lstObjects";
            mainLayout.SetRowSpan(lstObjects, 2);
            lstObjects.Size = new Size(556, 242);
            lstObjects.TabIndex = 1;
            // 
            // navPanel
            // 
            navPanel.Controls.Add(btnPrevious);
            navPanel.Controls.Add(btnNext);
            navPanel.Controls.Add(chkZoomToSelected);
            navPanel.Dock = DockStyle.Fill;
            navPanel.Location = new Point(187, 303);
            navPanel.Name = "navPanel";
            navPanel.Size = new Size(556, 36);
            navPanel.TabIndex = 2;
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
            lblSelectionInfo.Location = new Point(187, 342);
            lblSelectionInfo.Name = "lblSelectionInfo";
            lblSelectionInfo.Size = new Size(556, 46);
            lblSelectionInfo.TabIndex = 3;
            lblSelectionInfo.Text = "Select an imported parcel shape.";
            lblSelectionInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLayerMappingCaption
            // 
            lblLayerMappingCaption.Dock = DockStyle.Fill;
            lblLayerMappingCaption.Location = new Point(17, 388);
            lblLayerMappingCaption.Name = "lblLayerMappingCaption";
            lblLayerMappingCaption.Size = new Size(164, 30);
            lblLayerMappingCaption.TabIndex = 4;
            lblLayerMappingCaption.Text = "Layer to MapSheet";
            lblLayerMappingCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvLayerMapSheets
            // 
            dgvLayerMapSheets.AllowUserToAddRows = false;
            dgvLayerMapSheets.AllowUserToDeleteRows = false;
            dgvLayerMapSheets.AllowUserToResizeRows = false;
            dgvLayerMapSheets.BackgroundColor = SystemColors.Window;
            dgvLayerMapSheets.BorderStyle = BorderStyle.Fixed3D;
            dgvLayerMapSheets.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLayerMapSheets.Dock = DockStyle.Fill;
            dgvLayerMapSheets.Location = new Point(187, 391);
            dgvLayerMapSheets.MultiSelect = false;
            dgvLayerMapSheets.Name = "dgvLayerMapSheets";
            dgvLayerMapSheets.RowHeadersVisible = false;
            dgvLayerMapSheets.RowHeadersWidth = 51;
            mainLayout.SetRowSpan(dgvLayerMapSheets, 2);
            dgvLayerMapSheets.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLayerMapSheets.Size = new Size(556, 120);
            dgvLayerMapSheets.TabIndex = 3;
            // 
            // lblMapSheetCaption
            // 
            lblMapSheetCaption.Dock = DockStyle.Fill;
            lblMapSheetCaption.Location = new Point(17, 514);
            lblMapSheetCaption.Name = "lblMapSheetCaption";
            lblMapSheetCaption.Size = new Size(164, 38);
            lblMapSheetCaption.TabIndex = 5;
            lblMapSheetCaption.Text = "Manual map sheet";
            lblMapSheetCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboMapSheet
            // 
            cboMapSheet.Dock = DockStyle.Fill;
            cboMapSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMapSheet.FormattingEnabled = true;
            cboMapSheet.Location = new Point(187, 517);
            cboMapSheet.Name = "cboMapSheet";
            cboMapSheet.Size = new Size(556, 28);
            cboMapSheet.TabIndex = 4;
            // 
            // lblParcelCaption
            // 
            lblParcelCaption.Dock = DockStyle.Fill;
            lblParcelCaption.Location = new Point(17, 552);
            lblParcelCaption.Name = "lblParcelCaption";
            lblParcelCaption.Size = new Size(164, 38);
            lblParcelCaption.TabIndex = 6;
            lblParcelCaption.Text = "Parcel";
            lblParcelCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboParcel
            // 
            cboParcel.Dock = DockStyle.Fill;
            cboParcel.DropDownStyle = ComboBoxStyle.DropDownList;
            cboParcel.FlatStyle = FlatStyle.Flat;
            cboParcel.FormattingEnabled = true;
            cboParcel.Location = new Point(187, 555);
            cboParcel.Name = "cboParcel";
            cboParcel.Size = new Size(556, 28);
            cboParcel.TabIndex = 5;
            // 
            // chkReplaceExisting
            // 
            chkReplaceExisting.Dock = DockStyle.Fill;
            chkReplaceExisting.Location = new Point(187, 593);
            chkReplaceExisting.Name = "chkReplaceExisting";
            chkReplaceExisting.Size = new Size(556, 28);
            chkReplaceExisting.TabIndex = 6;
            chkReplaceExisting.Text = "Replace existing record-to-map assignments";
            chkReplaceExisting.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Location = new Point(187, 624);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(556, 34);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // actionPanel
            // 
            actionPanel.Controls.Add(btnClose);
            actionPanel.Controls.Add(btnAutoAssign);
            actionPanel.Controls.Add(btnAssign);
            actionPanel.Dock = DockStyle.Fill;
            actionPanel.FlowDirection = FlowDirection.RightToLeft;
            actionPanel.Location = new Point(187, 661);
            actionPanel.Name = "actionPanel";
            actionPanel.Size = new Size(556, 42);
            actionPanel.TabIndex = 7;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(463, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 32);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnAutoAssign
            // 
            btnAutoAssign.Location = new Point(337, 3);
            btnAutoAssign.Name = "btnAutoAssign";
            btnAutoAssign.Size = new Size(120, 32);
            btnAutoAssign.TabIndex = 1;
            btnAutoAssign.Text = "Auto Assign";
            btnAutoAssign.UseVisualStyleBackColor = true;
            // 
            // btnAssign
            // 
            btnAssign.Location = new Point(231, 3);
            btnAssign.Name = "btnAssign";
            btnAssign.Size = new Size(100, 32);
            btnAssign.TabIndex = 0;
            btnAssign.Text = "Assign";
            btnAssign.UseVisualStyleBackColor = true;
            // 
            // frmCadastralRecordAssignment
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(760, 720);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCadastralRecordAssignment";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Assign Cadastral Records";
            mainLayout.ResumeLayout(false);
            navPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLayerMapSheets).EndInit();
            actionPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
