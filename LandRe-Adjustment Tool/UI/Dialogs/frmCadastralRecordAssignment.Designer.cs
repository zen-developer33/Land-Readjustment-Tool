namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmCadastralRecordAssignment
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label lblObjectCaption;
        private ListBox lstObjects;
        private FlowLayoutPanel navPanel;
        private Button btnPrevious;
        private Button btnNext;
        private CheckBox chkZoomToSelected;
        private Label lblSelectionInfo;
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
            components = new System.ComponentModel.Container();
            mainLayout = new TableLayoutPanel();
            lblObjectCaption = new Label();
            lstObjects = new ListBox();
            navPanel = new FlowLayoutPanel();
            btnPrevious = new Button();
            btnNext = new Button();
            chkZoomToSelected = new CheckBox();
            lblSelectionInfo = new Label();
            lblMapSheetCaption = new Label();
            cboMapSheet = new ComboBox();
            lblParcelCaption = new Label();
            cboParcel = new ComboBox();
            chkReplaceExisting = new CheckBox();
            actionPanel = new FlowLayoutPanel();
            btnAssign = new Button();
            btnAutoAssign = new Button();
            btnClose = new Button();
            lblStatus = new Label();
            mainLayout.SuspendLayout();
            navPanel.SuspendLayout();
            actionPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblObjectCaption, 0, 0);
            mainLayout.Controls.Add(lstObjects, 1, 0);
            mainLayout.Controls.Add(navPanel, 1, 2);
            mainLayout.Controls.Add(lblSelectionInfo, 1, 3);
            mainLayout.Controls.Add(lblMapSheetCaption, 0, 4);
            mainLayout.Controls.Add(cboMapSheet, 1, 4);
            mainLayout.Controls.Add(lblParcelCaption, 0, 5);
            mainLayout.Controls.Add(cboParcel, 1, 5);
            mainLayout.Controls.Add(chkReplaceExisting, 1, 6);
            mainLayout.Controls.Add(lblStatus, 1, 7);
            mainLayout.Controls.Add(actionPanel, 1, 8);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(14);
            mainLayout.RowCount = 9;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            mainLayout.SetRowSpan(lstObjects, 2);
            mainLayout.Size = new Size(680, 520);
            mainLayout.TabIndex = 0;
            // 
            // lblObjectCaption
            // 
            lblObjectCaption.Dock = DockStyle.Fill;
            lblObjectCaption.Text = "Canvas objects";
            lblObjectCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lstObjects
            // 
            lstObjects.Dock = DockStyle.Fill;
            lstObjects.FormattingEnabled = true;
            lstObjects.ItemHeight = 20;
            lstObjects.Location = new Point(167, 17);
            lstObjects.Name = "lstObjects";
            lstObjects.Size = new Size(496, 188);
            lstObjects.TabIndex = 0;
            // 
            // navPanel
            // 
            navPanel.Controls.Add(btnPrevious);
            navPanel.Controls.Add(btnNext);
            navPanel.Controls.Add(chkZoomToSelected);
            navPanel.Dock = DockStyle.Fill;
            navPanel.Location = new Point(167, 211);
            navPanel.Name = "navPanel";
            navPanel.Size = new Size(496, 36);
            navPanel.TabIndex = 1;
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
            chkZoomToSelected.Location = new Point(195, 4);
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
            lblSelectionInfo.Text = "Select an imported parcel shape.";
            lblSelectionInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMapSheetCaption
            // 
            lblMapSheetCaption.Dock = DockStyle.Fill;
            lblMapSheetCaption.Text = "Map sheet";
            lblMapSheetCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboMapSheet
            // 
            cboMapSheet.Dock = DockStyle.Fill;
            cboMapSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMapSheet.FormattingEnabled = true;
            cboMapSheet.Location = new Point(167, 295);
            cboMapSheet.Name = "cboMapSheet";
            cboMapSheet.Size = new Size(496, 28);
            cboMapSheet.TabIndex = 2;
            // 
            // lblParcelCaption
            // 
            lblParcelCaption.Dock = DockStyle.Fill;
            lblParcelCaption.Text = "Parcel";
            lblParcelCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboParcel
            // 
            cboParcel.Dock = DockStyle.Fill;
            cboParcel.DropDownStyle = ComboBoxStyle.DropDownList;
            cboParcel.FormattingEnabled = true;
            cboParcel.Location = new Point(167, 333);
            cboParcel.Name = "cboParcel";
            cboParcel.Size = new Size(496, 28);
            cboParcel.TabIndex = 3;
            // 
            // chkReplaceExisting
            // 
            chkReplaceExisting.Dock = DockStyle.Fill;
            chkReplaceExisting.Text = "Replace existing record-to-map assignments";
            chkReplaceExisting.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = Color.DimGray;
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
            actionPanel.Location = new Point(167, 461);
            actionPanel.Name = "actionPanel";
            actionPanel.Size = new Size(496, 42);
            actionPanel.TabIndex = 4;
            // 
            // btnAssign
            // 
            btnAssign.Location = new Point(170, 3);
            btnAssign.Name = "btnAssign";
            btnAssign.Size = new Size(100, 32);
            btnAssign.TabIndex = 0;
            btnAssign.Text = "Assign";
            btnAssign.UseVisualStyleBackColor = true;
            // 
            // btnAutoAssign
            // 
            btnAutoAssign.Location = new Point(276, 3);
            btnAutoAssign.Name = "btnAutoAssign";
            btnAutoAssign.Size = new Size(120, 32);
            btnAutoAssign.TabIndex = 1;
            btnAutoAssign.Text = "Auto Assign";
            btnAutoAssign.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(402, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 32);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmCadastralRecordAssignment
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(680, 520);
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
            actionPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
