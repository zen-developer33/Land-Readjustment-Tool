namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmCadastralRecordAssignment
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label lblInfo;
        private CheckBox chkReplaceExisting;
        private Label lblStatus;
        private FlowLayoutPanel buttonPanel;
        private Button btnAutoAssign;
        private Button btnClose;

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
            lblInfo = new Label();
            chkReplaceExisting = new CheckBox();
            lblStatus = new Label();
            buttonPanel = new FlowLayoutPanel();
            btnAutoAssign = new Button();
            btnClose = new Button();
            mainLayout.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblInfo, 0, 0);
            mainLayout.Controls.Add(chkReplaceExisting, 0, 1);
            mainLayout.Controls.Add(lblStatus, 0, 2);
            mainLayout.Controls.Add(buttonPanel, 0, 3);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(14);
            mainLayout.RowCount = 4;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            mainLayout.Size = new Size(520, 220);
            mainLayout.TabIndex = 0;
            // 
            // lblInfo
            // 
            lblInfo.Dock = DockStyle.Fill;
            lblInfo.Location = new Point(17, 14);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(486, 92);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Assign imported cadastral map objects to Original Parcel Records using MapSheetNo + ParcelNo. Parcel labels are saved on the canvas object, so turning on layer labels shows the assigned parcel number.";
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkReplaceExisting
            // 
            chkReplaceExisting.Dock = DockStyle.Fill;
            chkReplaceExisting.Location = new Point(17, 109);
            chkReplaceExisting.Name = "chkReplaceExisting";
            chkReplaceExisting.Size = new Size(486, 28);
            chkReplaceExisting.TabIndex = 1;
            chkReplaceExisting.Text = "Replace existing record-to-map assignments";
            chkReplaceExisting.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Location = new Point(17, 140);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(486, 34);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(btnClose);
            buttonPanel.Controls.Add(btnAutoAssign);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(17, 177);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(486, 26);
            buttonPanel.TabIndex = 3;
            // 
            // btnAutoAssign
            // 
            btnAutoAssign.Location = new Point(264, 3);
            btnAutoAssign.Name = "btnAutoAssign";
            btnAutoAssign.Size = new Size(120, 32);
            btnAutoAssign.TabIndex = 0;
            btnAutoAssign.Text = "Auto Assign";
            btnAutoAssign.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(390, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 32);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmCadastralRecordAssignment
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(520, 220);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCadastralRecordAssignment";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Assign Cadastral Records";
            mainLayout.ResumeLayout(false);
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
