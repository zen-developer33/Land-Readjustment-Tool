namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmCadastralImport
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label lblFileCaption;
        private Label lblFormatCaption;
        private Label lblSourceCrsCaption;
        private Label lblProjectCrsCaption;
        private Label lblLayersCaption;
        private Label lblFileValue;
        private Label lblFormatValue;
        private Label lblProjectCrsValue;
        private ComboBox cmbSourceCrs;
        private Label lblSourceCrsValue;
        private DataGridView dgvLayers;
        private CheckBox chkAutoAssign;
        private Label lblAssignmentNote;
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
            components = new System.ComponentModel.Container();
            mainLayout = new TableLayoutPanel();
            lblFileCaption = new Label();
            lblFormatCaption = new Label();
            lblSourceCrsCaption = new Label();
            lblProjectCrsCaption = new Label();
            lblLayersCaption = new Label();
            lblFileValue = new Label();
            lblFormatValue = new Label();
            lblProjectCrsValue = new Label();
            cmbSourceCrs = new ComboBox();
            lblSourceCrsValue = new Label();
            dgvLayers = new DataGridView();
            chkAutoAssign = new CheckBox();
            lblAssignmentNote = new Label();
            lblStatus = new Label();
            buttonPanel = new FlowLayoutPanel();
            btnImport = new Button();
            btnCancel = new Button();
            mainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLayers).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblFileCaption, 0, 0);
            mainLayout.Controls.Add(lblFormatCaption, 0, 1);
            mainLayout.Controls.Add(lblLayersCaption, 0, 2);
            mainLayout.Controls.Add(lblFileValue, 1, 0);
            mainLayout.Controls.Add(lblFormatValue, 1, 1);
            mainLayout.Controls.Add(dgvLayers, 1, 2);
            mainLayout.Controls.Add(chkAutoAssign, 1, 4);
            mainLayout.Controls.Add(lblAssignmentNote, 1, 5);
            mainLayout.Controls.Add(lblSourceCrsCaption, 0, 6);
            mainLayout.Controls.Add(cmbSourceCrs, 1, 6);
            mainLayout.Controls.Add(lblSourceCrsValue, 1, 6);
            mainLayout.Controls.Add(lblProjectCrsCaption, 0, 7);
            mainLayout.Controls.Add(lblProjectCrsValue, 1, 7);
            mainLayout.Controls.Add(lblStatus, 1, 8);
            mainLayout.Controls.Add(buttonPanel, 1, 9);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(14);
            mainLayout.RowCount = 10;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            mainLayout.SetRowSpan(dgvLayers, 2);
            mainLayout.Size = new Size(680, 520);
            mainLayout.TabIndex = 0;
            // 
            // labels
            // 
            lblFileCaption.Dock = DockStyle.Fill;
            lblFileCaption.Text = "File";
            lblFileCaption.TextAlign = ContentAlignment.MiddleLeft;
            lblFormatCaption.Dock = DockStyle.Fill;
            lblFormatCaption.Text = "Format";
            lblFormatCaption.TextAlign = ContentAlignment.MiddleLeft;
            lblLayersCaption.Dock = DockStyle.Fill;
            lblLayersCaption.Text = "Layers";
            lblLayersCaption.TextAlign = ContentAlignment.MiddleLeft;
            lblSourceCrsCaption.Dock = DockStyle.Fill;
            lblSourceCrsCaption.Text = "Source CRS";
            lblSourceCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            lblProjectCrsCaption.Dock = DockStyle.Fill;
            lblProjectCrsCaption.Text = "Project CRS";
            lblProjectCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            lblFileValue.AutoEllipsis = true;
            lblFileValue.Dock = DockStyle.Fill;
            lblFileValue.TextAlign = ContentAlignment.MiddleLeft;
            lblFormatValue.Dock = DockStyle.Fill;
            lblFormatValue.TextAlign = ContentAlignment.MiddleLeft;
            lblProjectCrsValue.AutoEllipsis = true;
            lblProjectCrsValue.Dock = DockStyle.Fill;
            lblProjectCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            lblSourceCrsValue.Dock = DockStyle.Fill;
            lblSourceCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            lblSourceCrsValue.Visible = false;
            // 
            // dgvLayers
            // 
            dgvLayers.AllowUserToAddRows = false;
            dgvLayers.AllowUserToDeleteRows = false;
            dgvLayers.AllowUserToResizeRows = false;
            dgvLayers.BackgroundColor = SystemColors.Window;
            dgvLayers.BorderStyle = BorderStyle.Fixed3D;
            dgvLayers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLayers.Dock = DockStyle.Fill;
            dgvLayers.Location = new Point(145, 77);
            dgvLayers.MultiSelect = false;
            dgvLayers.Name = "dgvLayers";
            dgvLayers.RowHeadersVisible = false;
            dgvLayers.RowHeadersWidth = 51;
            dgvLayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLayers.Size = new Size(518, 217);
            dgvLayers.TabIndex = 0;
            // 
            // chkAutoAssign
            // 
            chkAutoAssign.Checked = true;
            chkAutoAssign.CheckState = CheckState.Checked;
            chkAutoAssign.Dock = DockStyle.Fill;
            chkAutoAssign.Text = "Auto assign parcel number from DXF text / SHP attributes when available";
            chkAutoAssign.UseVisualStyleBackColor = true;
            // 
            // lblAssignmentNote
            // 
            lblAssignmentNote.Dock = DockStyle.Fill;
            lblAssignmentNote.ForeColor = Color.DimGray;
            lblAssignmentNote.Text = "Map sheet assignment uses MapSheetNo + ParcelNo from imported records.";
            lblAssignmentNote.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbSourceCrs
            // 
            cmbSourceCrs.Dock = DockStyle.Fill;
            cmbSourceCrs.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSourceCrs.FormattingEnabled = true;
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(145, 463);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(518, 40);
            buttonPanel.TabIndex = 1;
            // 
            // btnImport
            // 
            btnImport.Enabled = false;
            btnImport.Location = new Point(425, 3);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(90, 32);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(329, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 32);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmCadastralImport
            // 
            AcceptButton = btnImport;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(680, 520);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCadastralImport";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Import Cadastral Map";
            mainLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLayers).EndInit();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
