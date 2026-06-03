namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmBoundaryImport
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label lblFileCaption;
        private Label lblFormatCaption;
        private Label lblLayerCaption;
        private Label lblSourceCrsCaption;
        private Label lblProjectCrsCaption;
        private Label lblFileValue;
        private Label lblFormatValue;
        private ListBox lstLayers;
        private ComboBox cmbSourceCrs;
        private Label lblSourceCrsValue;
        private Label lblProjectCrsValue;
        private Label lblStatus;
        private FlowLayoutPanel buttonPanel;
        private Button btnImport;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            mainLayout = new TableLayoutPanel();
            lblFileCaption = new Label();
            lblFormatCaption = new Label();
            lblLayerCaption = new Label();
            lblSourceCrsCaption = new Label();
            lblProjectCrsCaption = new Label();
            lblFileValue = new Label();
            lblFormatValue = new Label();
            lstLayers = new ListBox();
            cmbSourceCrs = new ComboBox();
            lblSourceCrsValue = new Label();
            lblProjectCrsValue = new Label();
            lblStatus = new Label();
            buttonPanel = new FlowLayoutPanel();
            btnImport = new Button();
            btnCancel = new Button();
            mainLayout.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblFileCaption, 0, 0);
            mainLayout.Controls.Add(lblFormatCaption, 0, 1);
            mainLayout.Controls.Add(lblLayerCaption, 0, 2);
            mainLayout.Controls.Add(lblSourceCrsCaption, 0, 4);
            mainLayout.Controls.Add(lblProjectCrsCaption, 0, 5);
            mainLayout.Controls.Add(lblFileValue, 1, 0);
            mainLayout.Controls.Add(lblFormatValue, 1, 1);
            mainLayout.Controls.Add(lstLayers, 1, 2);
            mainLayout.Controls.Add(cmbSourceCrs, 1, 4);
            mainLayout.Controls.Add(lblSourceCrsValue, 1, 4);
            mainLayout.Controls.Add(lblProjectCrsValue, 1, 5);
            mainLayout.Controls.Add(lblStatus, 1, 6);
            mainLayout.Controls.Add(buttonPanel, 1, 7);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(14);
            mainLayout.RowCount = 8;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            mainLayout.Size = new Size(560, 440);
            mainLayout.TabIndex = 0;
            // 
            // lblFileCaption
            // 
            lblFileCaption.Dock = DockStyle.Fill;
            lblFileCaption.Location = new Point(17, 14);
            lblFileCaption.Name = "lblFileCaption";
            lblFileCaption.Size = new Size(126, 32);
            lblFileCaption.TabIndex = 0;
            lblFileCaption.Text = "File";
            lblFileCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFormatCaption
            // 
            lblFormatCaption.Dock = DockStyle.Fill;
            lblFormatCaption.Location = new Point(17, 46);
            lblFormatCaption.Name = "lblFormatCaption";
            lblFormatCaption.Size = new Size(126, 32);
            lblFormatCaption.TabIndex = 1;
            lblFormatCaption.Text = "Format";
            lblFormatCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLayerCaption
            // 
            lblLayerCaption.Dock = DockStyle.Fill;
            lblLayerCaption.Location = new Point(17, 78);
            lblLayerCaption.Name = "lblLayerCaption";
            lblLayerCaption.Size = new Size(126, 30);
            lblLayerCaption.TabIndex = 2;
            lblLayerCaption.Text = "Layer / group";
            lblLayerCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSourceCrsCaption
            // 
            lblSourceCrsCaption.Dock = DockStyle.Fill;
            lblSourceCrsCaption.Location = new Point(17, 272);
            lblSourceCrsCaption.Name = "lblSourceCrsCaption";
            lblSourceCrsCaption.Size = new Size(126, 36);
            lblSourceCrsCaption.TabIndex = 3;
            lblSourceCrsCaption.Text = "Source CRS";
            lblSourceCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProjectCrsCaption
            // 
            lblProjectCrsCaption.Dock = DockStyle.Fill;
            lblProjectCrsCaption.Location = new Point(149, 308);
            lblProjectCrsCaption.Name = "lblProjectCrsCaption";
            lblProjectCrsCaption.Size = new Size(394, 36);
            lblProjectCrsCaption.TabIndex = 4;
            lblProjectCrsCaption.Text = "Project CRS";
            lblProjectCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFileValue
            // 
            lblFileValue.AutoEllipsis = true;
            lblFileValue.Dock = DockStyle.Fill;
            lblFileValue.Location = new Point(149, 14);
            lblFileValue.Name = "lblFileValue";
            lblFileValue.Size = new Size(394, 32);
            lblFileValue.TabIndex = 5;
            lblFileValue.TextAlign = ContentAlignment.MiddleLeft;
            lblFileValue.UseMnemonic = false;
            // 
            // lblFormatValue
            // 
            lblFormatValue.Dock = DockStyle.Fill;
            lblFormatValue.Location = new Point(149, 46);
            lblFormatValue.Name = "lblFormatValue";
            lblFormatValue.Size = new Size(394, 32);
            lblFormatValue.TabIndex = 6;
            lblFormatValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lstLayers
            // 
            lstLayers.Dock = DockStyle.Fill;
            lstLayers.FormattingEnabled = true;
            lstLayers.Location = new Point(149, 81);
            lstLayers.Name = "lstLayers";
            mainLayout.SetRowSpan(lstLayers, 2);
            lstLayers.Size = new Size(394, 188);
            lstLayers.TabIndex = 7;
            // 
            // cmbSourceCrs
            // 
            cmbSourceCrs.Dock = DockStyle.Fill;
            cmbSourceCrs.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSourceCrs.FormattingEnabled = true;
            cmbSourceCrs.Location = new Point(17, 311);
            cmbSourceCrs.Name = "cmbSourceCrs";
            cmbSourceCrs.Size = new Size(126, 28);
            cmbSourceCrs.TabIndex = 8;
            // 
            // lblSourceCrsValue
            // 
            lblSourceCrsValue.Dock = DockStyle.Fill;
            lblSourceCrsValue.Location = new Point(149, 272);
            lblSourceCrsValue.Name = "lblSourceCrsValue";
            lblSourceCrsValue.Size = new Size(394, 36);
            lblSourceCrsValue.TabIndex = 9;
            lblSourceCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            lblSourceCrsValue.Visible = false;
            // 
            // lblProjectCrsValue
            // 
            lblProjectCrsValue.AutoEllipsis = true;
            lblProjectCrsValue.Dock = DockStyle.Fill;
            lblProjectCrsValue.Location = new Point(17, 344);
            lblProjectCrsValue.Name = "lblProjectCrsValue";
            lblProjectCrsValue.Size = new Size(126, 34);
            lblProjectCrsValue.TabIndex = 10;
            lblProjectCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            lblProjectCrsValue.UseMnemonic = false;
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Location = new Point(149, 344);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(394, 34);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(149, 381);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(394, 42);
            buttonPanel.TabIndex = 12;
            // 
            // btnImport
            // 
            btnImport.Enabled = false;
            btnImport.Location = new Point(301, 3);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(90, 32);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(205, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 32);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmBoundaryImport
            // 
            AcceptButton = btnImport;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(560, 440);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmBoundaryImport";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Import Project Boundary";
            mainLayout.ResumeLayout(false);
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
