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
        private Label lblMapSheetMappingCaption;
        private Label lblFileValue;
        private Label lblFormatValue;
        private Label lblProjectCrsValue;
        private ComboBox cmbSourceCrs;
        private Label lblSourceCrsValue;
        private DataGridView dgvLayers;
        private DataGridView dgvMapSheetMappings;
        private TableLayoutPanel attributeMappingLayout;
        private Label lblSourceMapSheetField;
        private ComboBox cboSourceMapSheetField;
        private DataGridView dgvAttributeMapSheetMappings;
        private Label lblSourceParcelField;
        private ComboBox cboSourceParcelField;
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
            mainLayout = new TableLayoutPanel();
            lblFileCaption = new Label();
            lblFormatCaption = new Label();
            lblLayersCaption = new Label();
            lblFileValue = new Label();
            lblFormatValue = new Label();
            dgvLayers = new DataGridView();
            lblMapSheetMappingCaption = new Label();
            dgvMapSheetMappings = new DataGridView();
            attributeMappingLayout = new TableLayoutPanel();
            lblSourceMapSheetField = new Label();
            cboSourceMapSheetField = new ComboBox();
            dgvAttributeMapSheetMappings = new DataGridView();
            lblSourceParcelField = new Label();
            cboSourceParcelField = new ComboBox();
            chkAutoAssign = new CheckBox();
            lblAssignmentNote = new Label();
            lblSourceCrsCaption = new Label();
            cmbSourceCrs = new ComboBox();
            lblSourceCrsValue = new Label();
            lblProjectCrsCaption = new Label();
            lblProjectCrsValue = new Label();
            lblStatus = new Label();
            buttonPanel = new FlowLayoutPanel();
            btnImport = new Button();
            btnCancel = new Button();
            mainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLayers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvMapSheetMappings).BeginInit();
            attributeMappingLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAttributeMapSheetMappings).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 166F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblFileCaption, 0, 0);
            mainLayout.Controls.Add(lblFormatCaption, 0, 1);
            mainLayout.Controls.Add(lblLayersCaption, 0, 2);
            mainLayout.Controls.Add(lblFileValue, 1, 0);
            mainLayout.Controls.Add(lblFormatValue, 1, 1);
            mainLayout.Controls.Add(dgvLayers, 1, 2);
            mainLayout.Controls.Add(lblMapSheetMappingCaption, 0, 4);
            mainLayout.Controls.Add(dgvMapSheetMappings, 1, 4);
            mainLayout.Controls.Add(attributeMappingLayout, 1, 4);
            mainLayout.Controls.Add(chkAutoAssign, 1, 6);
            mainLayout.Controls.Add(lblAssignmentNote, 1, 7);
            mainLayout.Controls.Add(lblSourceCrsCaption, 0, 8);
            mainLayout.Controls.Add(cmbSourceCrs, 1, 8);
            mainLayout.Controls.Add(lblSourceCrsValue, 1, 8);
            mainLayout.Controls.Add(lblProjectCrsCaption, 0, 9);
            mainLayout.Controls.Add(lblProjectCrsValue, 1, 9);
            mainLayout.Controls.Add(lblStatus, 1, 10);
            mainLayout.Controls.Add(buttonPanel, 1, 11);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(14);
            mainLayout.RowCount = 12;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 129F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 209F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
            mainLayout.Size = new Size(786, 700);
            mainLayout.TabIndex = 0;
            // 
            // lblFileCaption
            // 
            lblFileCaption.Dock = DockStyle.Fill;
            lblFileCaption.Location = new Point(17, 14);
            lblFileCaption.Name = "lblFileCaption";
            lblFileCaption.Size = new Size(160, 30);
            lblFileCaption.TabIndex = 0;
            lblFileCaption.Text = "File";
            lblFileCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFormatCaption
            // 
            lblFormatCaption.Dock = DockStyle.Fill;
            lblFormatCaption.Location = new Point(17, 44);
            lblFormatCaption.Name = "lblFormatCaption";
            lblFormatCaption.Size = new Size(160, 30);
            lblFormatCaption.TabIndex = 1;
            lblFormatCaption.Text = "Format";
            lblFormatCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLayersCaption
            // 
            lblLayersCaption.Dock = DockStyle.Fill;
            lblLayersCaption.Location = new Point(17, 74);
            lblLayersCaption.Name = "lblLayersCaption";
            lblLayersCaption.Size = new Size(160, 30);
            lblLayersCaption.TabIndex = 2;
            lblLayersCaption.Text = "Layers";
            lblLayersCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFileValue
            // 
            lblFileValue.AutoEllipsis = true;
            lblFileValue.Dock = DockStyle.Fill;
            lblFileValue.Location = new Point(183, 14);
            lblFileValue.Name = "lblFileValue";
            lblFileValue.Size = new Size(586, 30);
            lblFileValue.TabIndex = 3;
            lblFileValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFormatValue
            // 
            lblFormatValue.Dock = DockStyle.Fill;
            lblFormatValue.Location = new Point(183, 44);
            lblFormatValue.Name = "lblFormatValue";
            lblFormatValue.Size = new Size(586, 30);
            lblFormatValue.TabIndex = 4;
            lblFormatValue.TextAlign = ContentAlignment.MiddleLeft;
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
            dgvLayers.Location = new Point(183, 77);
            dgvLayers.MultiSelect = false;
            dgvLayers.Name = "dgvLayers";
            dgvLayers.RowHeadersVisible = false;
            dgvLayers.RowHeadersWidth = 51;
            mainLayout.SetRowSpan(dgvLayers, 2);
            dgvLayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLayers.Size = new Size(586, 153);
            dgvLayers.TabIndex = 0;
            // 
            // lblMapSheetMappingCaption
            // 
            lblMapSheetMappingCaption.Dock = DockStyle.Fill;
            lblMapSheetMappingCaption.Location = new Point(17, 233);
            lblMapSheetMappingCaption.Name = "lblMapSheetMappingCaption";
            lblMapSheetMappingCaption.Size = new Size(160, 35);
            lblMapSheetMappingCaption.TabIndex = 5;
            lblMapSheetMappingCaption.Text = "Map sheet mapping";
            lblMapSheetMappingCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvMapSheetMappings
            // 
            dgvMapSheetMappings.AllowUserToAddRows = false;
            dgvMapSheetMappings.AllowUserToDeleteRows = false;
            dgvMapSheetMappings.AllowUserToResizeRows = false;
            dgvMapSheetMappings.BackgroundColor = SystemColors.Window;
            dgvMapSheetMappings.BorderStyle = BorderStyle.Fixed3D;
            dgvMapSheetMappings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMapSheetMappings.Dock = DockStyle.Fill;
            dgvMapSheetMappings.Location = new Point(17, 271);
            dgvMapSheetMappings.MultiSelect = false;
            dgvMapSheetMappings.Name = "dgvMapSheetMappings";
            dgvMapSheetMappings.RowHeadersVisible = false;
            dgvMapSheetMappings.RowHeadersWidth = 51;
            mainLayout.SetRowSpan(dgvMapSheetMappings, 2);
            dgvMapSheetMappings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMapSheetMappings.Size = new Size(160, 236);
            dgvMapSheetMappings.TabIndex = 1;
            // 
            // attributeMappingLayout
            // 
            attributeMappingLayout.ColumnCount = 2;
            attributeMappingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155F));
            attributeMappingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            attributeMappingLayout.Controls.Add(lblSourceMapSheetField, 0, 0);
            attributeMappingLayout.Controls.Add(cboSourceMapSheetField, 1, 0);
            attributeMappingLayout.Controls.Add(dgvAttributeMapSheetMappings, 0, 1);
            attributeMappingLayout.Controls.Add(lblSourceParcelField, 0, 2);
            attributeMappingLayout.Controls.Add(cboSourceParcelField, 1, 2);
            attributeMappingLayout.Dock = DockStyle.Fill;
            attributeMappingLayout.Location = new Point(183, 236);
            attributeMappingLayout.Name = "attributeMappingLayout";
            attributeMappingLayout.RowCount = 3;
            mainLayout.SetRowSpan(attributeMappingLayout, 2);
            attributeMappingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            attributeMappingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            attributeMappingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            attributeMappingLayout.Size = new Size(586, 238);
            attributeMappingLayout.TabIndex = 2;
            attributeMappingLayout.Visible = false;
            // 
            // lblSourceMapSheetField
            // 
            lblSourceMapSheetField.Dock = DockStyle.Fill;
            lblSourceMapSheetField.Location = new Point(3, 0);
            lblSourceMapSheetField.Name = "lblSourceMapSheetField";
            lblSourceMapSheetField.Size = new Size(149, 32);
            lblSourceMapSheetField.TabIndex = 0;
            lblSourceMapSheetField.Text = "Map sheet field";
            lblSourceMapSheetField.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboSourceMapSheetField
            // 
            cboSourceMapSheetField.Dock = DockStyle.Fill;
            cboSourceMapSheetField.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSourceMapSheetField.FormattingEnabled = true;
            cboSourceMapSheetField.Location = new Point(158, 3);
            cboSourceMapSheetField.Name = "cboSourceMapSheetField";
            cboSourceMapSheetField.Size = new Size(425, 28);
            cboSourceMapSheetField.TabIndex = 1;
            // 
            // dgvAttributeMapSheetMappings
            // 
            dgvAttributeMapSheetMappings.AllowUserToAddRows = false;
            dgvAttributeMapSheetMappings.AllowUserToDeleteRows = false;
            dgvAttributeMapSheetMappings.AllowUserToResizeRows = false;
            dgvAttributeMapSheetMappings.BackgroundColor = SystemColors.Window;
            dgvAttributeMapSheetMappings.BorderStyle = BorderStyle.Fixed3D;
            dgvAttributeMapSheetMappings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            attributeMappingLayout.SetColumnSpan(dgvAttributeMapSheetMappings, 2);
            dgvAttributeMapSheetMappings.Dock = DockStyle.Fill;
            dgvAttributeMapSheetMappings.Location = new Point(3, 35);
            dgvAttributeMapSheetMappings.MultiSelect = false;
            dgvAttributeMapSheetMappings.Name = "dgvAttributeMapSheetMappings";
            dgvAttributeMapSheetMappings.RowHeadersVisible = false;
            dgvAttributeMapSheetMappings.RowHeadersWidth = 51;
            dgvAttributeMapSheetMappings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAttributeMapSheetMappings.Size = new Size(580, 166);
            dgvAttributeMapSheetMappings.TabIndex = 2;
            // 
            // lblSourceParcelField
            // 
            lblSourceParcelField.Dock = DockStyle.Fill;
            lblSourceParcelField.Location = new Point(3, 204);
            lblSourceParcelField.Name = "lblSourceParcelField";
            lblSourceParcelField.Size = new Size(149, 34);
            lblSourceParcelField.TabIndex = 3;
            lblSourceParcelField.Text = "Parcel field";
            lblSourceParcelField.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboSourceParcelField
            // 
            cboSourceParcelField.Dock = DockStyle.Fill;
            cboSourceParcelField.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSourceParcelField.FormattingEnabled = true;
            cboSourceParcelField.Location = new Point(158, 207);
            cboSourceParcelField.Name = "cboSourceParcelField";
            cboSourceParcelField.Size = new Size(425, 28);
            cboSourceParcelField.TabIndex = 4;
            // 
            // chkAutoAssign
            // 
            chkAutoAssign.Dock = DockStyle.Fill;
            chkAutoAssign.Location = new Point(183, 480);
            chkAutoAssign.Name = "chkAutoAssign";
            chkAutoAssign.Size = new Size(586, 27);
            chkAutoAssign.TabIndex = 6;
            chkAutoAssign.Text = "Auto assign parcel number from DXF text / SHP attributes when available";
            chkAutoAssign.UseVisualStyleBackColor = true;
            // 
            // lblAssignmentNote
            // 
            lblAssignmentNote.Dock = DockStyle.Fill;
            lblAssignmentNote.ForeColor = Color.DimGray;
            lblAssignmentNote.Location = new Point(183, 510);
            lblAssignmentNote.Name = "lblAssignmentNote";
            lblAssignmentNote.Size = new Size(586, 29);
            lblAssignmentNote.TabIndex = 7;
            lblAssignmentNote.Text = "Map sheet assignment uses MapSheetNo + ParcelNo from imported records.";
            lblAssignmentNote.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSourceCrsCaption
            // 
            lblSourceCrsCaption.Dock = DockStyle.Fill;
            lblSourceCrsCaption.Location = new Point(17, 539);
            lblSourceCrsCaption.Name = "lblSourceCrsCaption";
            lblSourceCrsCaption.Size = new Size(160, 37);
            lblSourceCrsCaption.TabIndex = 8;
            lblSourceCrsCaption.Text = "Source CRS";
            lblSourceCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbSourceCrs
            // 
            cmbSourceCrs.Dock = DockStyle.Fill;
            cmbSourceCrs.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSourceCrs.FormattingEnabled = true;
            cmbSourceCrs.Location = new Point(17, 579);
            cmbSourceCrs.Name = "cmbSourceCrs";
            cmbSourceCrs.Size = new Size(160, 28);
            cmbSourceCrs.TabIndex = 9;
            // 
            // lblSourceCrsValue
            // 
            lblSourceCrsValue.Dock = DockStyle.Fill;
            lblSourceCrsValue.Location = new Point(183, 539);
            lblSourceCrsValue.Name = "lblSourceCrsValue";
            lblSourceCrsValue.Size = new Size(586, 37);
            lblSourceCrsValue.TabIndex = 10;
            lblSourceCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            lblSourceCrsValue.Visible = false;
            // 
            // lblProjectCrsCaption
            // 
            lblProjectCrsCaption.Dock = DockStyle.Fill;
            lblProjectCrsCaption.Location = new Point(183, 576);
            lblProjectCrsCaption.Name = "lblProjectCrsCaption";
            lblProjectCrsCaption.Size = new Size(586, 34);
            lblProjectCrsCaption.TabIndex = 11;
            lblProjectCrsCaption.Text = "Project CRS";
            lblProjectCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProjectCrsValue
            // 
            lblProjectCrsValue.AutoEllipsis = true;
            lblProjectCrsValue.Dock = DockStyle.Fill;
            lblProjectCrsValue.Location = new Point(17, 610);
            lblProjectCrsValue.Name = "lblProjectCrsValue";
            lblProjectCrsValue.Size = new Size(160, 33);
            lblProjectCrsValue.TabIndex = 12;
            lblProjectCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Location = new Point(183, 610);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(586, 33);
            lblStatus.TabIndex = 13;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(183, 646);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(586, 37);
            buttonPanel.TabIndex = 1;
            // 
            // btnImport
            // 
            btnImport.Enabled = false;
            btnImport.Location = new Point(493, 3);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(90, 32);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(397, 3);
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
            ClientSize = new Size(786, 700);
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
            ((System.ComponentModel.ISupportInitialize)dgvMapSheetMappings).EndInit();
            attributeMappingLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvAttributeMapSheetMappings).EndInit();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
