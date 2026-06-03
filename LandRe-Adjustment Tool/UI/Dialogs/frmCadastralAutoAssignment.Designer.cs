namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmCadastralAutoAssignment
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label _lblMappingCaption;
        private DataGridView _dgvLayerMapSheets;
        private TableLayoutPanel _attributeLayout;
        private Label lblMapSheetField;
        private ComboBox _cboSourceMapSheetField;
        private DataGridView _dgvAttributeMapSheetMappings;
        private Label lblParcelField;
        private ComboBox _cboSourceParcelField;
        private CheckBox _chkReplaceExisting;
        private Label _lblStatus;
        private FlowLayoutPanel buttonPanel;
        private Button _btnRun;
        private Button _btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            mainLayout = new TableLayoutPanel();
            _lblMappingCaption = new Label();
            _dgvLayerMapSheets = new DataGridView();
            _attributeLayout = new TableLayoutPanel();
            lblMapSheetField = new Label();
            _cboSourceMapSheetField = new ComboBox();
            _dgvAttributeMapSheetMappings = new DataGridView();
            lblParcelField = new Label();
            _cboSourceParcelField = new ComboBox();
            _chkReplaceExisting = new CheckBox();
            _lblStatus = new Label();
            buttonPanel = new FlowLayoutPanel();
            _btnClose = new Button();
            _btnRun = new Button();
            mainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvLayerMapSheets).BeginInit();
            _attributeLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvAttributeMapSheetMappings).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(_lblMappingCaption, 0, 0);
            mainLayout.Controls.Add(_dgvLayerMapSheets, 1, 0);
            mainLayout.Controls.Add(_attributeLayout, 1, 0);
            mainLayout.Controls.Add(_chkReplaceExisting, 1, 2);
            mainLayout.Controls.Add(_lblStatus, 0, 3);
            mainLayout.Controls.Add(buttonPanel, 1, 4);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(10);
            mainLayout.RowCount = 5;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            mainLayout.SetRowSpan(_dgvLayerMapSheets, 2);
            mainLayout.SetRowSpan(_attributeLayout, 2);
            mainLayout.SetColumnSpan(_lblStatus, 2);
            mainLayout.Size = new Size(640, 430);
            mainLayout.TabIndex = 0;
            // 
            // _lblMappingCaption
            // 
            _lblMappingCaption.Dock = DockStyle.Fill;
            _lblMappingCaption.Location = new Point(17, 14);
            _lblMappingCaption.Name = "_lblMappingCaption";
            _lblMappingCaption.Size = new Size(144, 34);
            _lblMappingCaption.TabIndex = 0;
            _lblMappingCaption.Text = "Mapping";
            _lblMappingCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _dgvLayerMapSheets
            // 
            ConfigureLayerMappingGrid();
            _dgvLayerMapSheets.Location = new Point(167, 17);
            _dgvLayerMapSheets.Name = "_dgvLayerMapSheets";
            _dgvLayerMapSheets.Size = new Size(576, 379);
            _dgvLayerMapSheets.TabIndex = 0;
            // 
            // _attributeLayout
            // 
            _attributeLayout.ColumnCount = 2;
            _attributeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            _attributeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _attributeLayout.Controls.Add(lblMapSheetField, 0, 0);
            _attributeLayout.Controls.Add(_cboSourceMapSheetField, 1, 0);
            _attributeLayout.Controls.Add(_dgvAttributeMapSheetMappings, 0, 1);
            _attributeLayout.Controls.Add(lblParcelField, 0, 2);
            _attributeLayout.Controls.Add(_cboSourceParcelField, 1, 2);
            _attributeLayout.Dock = DockStyle.Fill;
            _attributeLayout.Location = new Point(167, 17);
            _attributeLayout.Name = "_attributeLayout";
            _attributeLayout.RowCount = 3;
            _attributeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _attributeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _attributeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _attributeLayout.SetColumnSpan(_dgvAttributeMapSheetMappings, 2);
            _attributeLayout.Size = new Size(576, 379);
            _attributeLayout.TabIndex = 0;
            _attributeLayout.Visible = false;
            // 
            // lblMapSheetField
            // 
            lblMapSheetField.Dock = DockStyle.Fill;
            lblMapSheetField.Location = new Point(3, 0);
            lblMapSheetField.Name = "lblMapSheetField";
            lblMapSheetField.Size = new Size(144, 34);
            lblMapSheetField.TabIndex = 0;
            lblMapSheetField.Text = "Map sheet field";
            lblMapSheetField.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboSourceMapSheetField
            // 
            _cboSourceMapSheetField.Dock = DockStyle.Fill;
            _cboSourceMapSheetField.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboSourceMapSheetField.FormattingEnabled = true;
            _cboSourceMapSheetField.Location = new Point(153, 3);
            _cboSourceMapSheetField.Name = "_cboSourceMapSheetField";
            _cboSourceMapSheetField.Size = new Size(420, 28);
            _cboSourceMapSheetField.TabIndex = 0;
            // 
            // _dgvAttributeMapSheetMappings
            // 
            ConfigureAttributeMappingGrid();
            _dgvAttributeMapSheetMappings.Location = new Point(3, 37);
            _dgvAttributeMapSheetMappings.Name = "_dgvAttributeMapSheetMappings";
            _dgvAttributeMapSheetMappings.Size = new Size(570, 305);
            _dgvAttributeMapSheetMappings.TabIndex = 1;
            // 
            // lblParcelField
            // 
            lblParcelField.Dock = DockStyle.Fill;
            lblParcelField.Location = new Point(3, 345);
            lblParcelField.Name = "lblParcelField";
            lblParcelField.Size = new Size(144, 34);
            lblParcelField.TabIndex = 2;
            lblParcelField.Text = "Parcel field";
            lblParcelField.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboSourceParcelField
            // 
            _cboSourceParcelField.Dock = DockStyle.Fill;
            _cboSourceParcelField.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboSourceParcelField.FormattingEnabled = true;
            _cboSourceParcelField.Location = new Point(153, 348);
            _cboSourceParcelField.Name = "_cboSourceParcelField";
            _cboSourceParcelField.Size = new Size(420, 28);
            _cboSourceParcelField.TabIndex = 2;
            // 
            // _chkReplaceExisting
            // 
            _chkReplaceExisting.Dock = DockStyle.Fill;
            _chkReplaceExisting.Location = new Point(167, 402);
            _chkReplaceExisting.Name = "_chkReplaceExisting";
            _chkReplaceExisting.Size = new Size(576, 28);
            _chkReplaceExisting.TabIndex = 1;
            _chkReplaceExisting.Text = "Replace existing record-to-map assignments";
            _chkReplaceExisting.UseVisualStyleBackColor = true;
            // 
            // _lblStatus
            // 
            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.ForeColor = Color.DimGray;
            _lblStatus.Location = new Point(17, 433);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Size = new Size(726, 34);
            _lblStatus.TabIndex = 2;
            _lblStatus.Text = "Loading imported cadastral objects...";
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(_btnClose);
            buttonPanel.Controls.Add(_btnRun);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(167, 470);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(576, 40);
            buttonPanel.TabIndex = 2;
            // 
            // _btnClose
            // 
            _btnClose.DialogResult = DialogResult.OK;
            _btnClose.Location = new Point(483, 3);
            _btnClose.Name = "_btnClose";
            _btnClose.Size = new Size(90, 32);
            _btnClose.TabIndex = 1;
            _btnClose.Text = "Close";
            _btnClose.UseVisualStyleBackColor = true;
            // 
            // _btnRun
            // 
            _btnRun.Location = new Point(327, 3);
            _btnRun.Name = "_btnRun";
            _btnRun.Size = new Size(150, 32);
            _btnRun.TabIndex = 0;
            _btnRun.Text = "Run Auto Assign";
            _btnRun.UseVisualStyleBackColor = true;
            // 
            // frmCadastralAutoAssignment
            // 
            AcceptButton = _btnRun;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _btnClose;
            ClientSize = new Size(640, 430);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCadastralAutoAssignment";
            StartPosition = FormStartPosition.Manual;
            Text = "Auto Cadastral Record Assignment";
            mainLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_dgvLayerMapSheets).EndInit();
            _attributeLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_dgvAttributeMapSheetMappings).EndInit();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void ConfigureLayerMappingGrid()
        {
            DataGridViewTextBoxColumn colLayer = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colSourceLayer = new DataGridViewTextBoxColumn();
            DataGridViewComboBoxColumn colMapSheet = new DataGridViewComboBoxColumn();
            _dgvLayerMapSheets.Columns.Clear();
            _dgvLayerMapSheets.AllowUserToAddRows = false;
            _dgvLayerMapSheets.AllowUserToDeleteRows = false;
            _dgvLayerMapSheets.AllowUserToResizeRows = false;
            _dgvLayerMapSheets.BackgroundColor = SystemColors.Window;
            _dgvLayerMapSheets.BorderStyle = BorderStyle.Fixed3D;
            _dgvLayerMapSheets.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvLayerMapSheets.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvLayerMapSheets.Dock = DockStyle.Fill;
            _dgvLayerMapSheets.MultiSelect = false;
            _dgvLayerMapSheets.RowHeadersVisible = false;
            _dgvLayerMapSheets.SelectionMode = DataGridViewSelectionMode.CellSelect;
            colLayer.HeaderText = "Imported layer";
            colLayer.MinimumWidth = 120;
            colLayer.Name = "Layer";
            colLayer.ReadOnly = true;
            colLayer.Width = 130;
            colSourceLayer.HeaderText = "Source layer";
            colSourceLayer.MinimumWidth = 120;
            colSourceLayer.Name = "SourceLayer";
            colSourceLayer.ReadOnly = true;
            colSourceLayer.Width = 130;
            colMapSheet.FlatStyle = FlatStyle.Flat;
            colMapSheet.HeaderText = "Target MapSheetNo";
            colMapSheet.MinimumWidth = 140;
            colMapSheet.Name = "MapSheet";
            colMapSheet.Width = 155;
            _dgvLayerMapSheets.Columns.AddRange(new DataGridViewColumn[] { colLayer, colSourceLayer, colMapSheet });
            ApplyQuietGridStyle(_dgvLayerMapSheets);
        }

        private void ConfigureAttributeMappingGrid()
        {
            DataGridViewTextBoxColumn colSourceMapSheet = new DataGridViewTextBoxColumn();
            DataGridViewComboBoxColumn colTargetMapSheet = new DataGridViewComboBoxColumn();
            _dgvAttributeMapSheetMappings.Columns.Clear();
            _dgvAttributeMapSheetMappings.AllowUserToAddRows = false;
            _dgvAttributeMapSheetMappings.AllowUserToDeleteRows = false;
            _dgvAttributeMapSheetMappings.AllowUserToResizeRows = false;
            _dgvAttributeMapSheetMappings.BackgroundColor = SystemColors.Window;
            _dgvAttributeMapSheetMappings.BorderStyle = BorderStyle.Fixed3D;
            _dgvAttributeMapSheetMappings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvAttributeMapSheetMappings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvAttributeMapSheetMappings.Dock = DockStyle.Fill;
            _dgvAttributeMapSheetMappings.MultiSelect = false;
            _dgvAttributeMapSheetMappings.RowHeadersVisible = false;
            _dgvAttributeMapSheetMappings.SelectionMode = DataGridViewSelectionMode.CellSelect;
            colSourceMapSheet.HeaderText = "Source map-sheet value";
            colSourceMapSheet.MinimumWidth = 150;
            colSourceMapSheet.Name = "SourceMapSheet";
            colSourceMapSheet.ReadOnly = true;
            colSourceMapSheet.Width = 175;
            colTargetMapSheet.FlatStyle = FlatStyle.Flat;
            colTargetMapSheet.HeaderText = "Target MapSheetNo";
            colTargetMapSheet.MinimumWidth = 140;
            colTargetMapSheet.Name = "TargetMapSheet";
            colTargetMapSheet.Width = 155;
            _dgvAttributeMapSheetMappings.Columns.AddRange(new DataGridViewColumn[] { colSourceMapSheet, colTargetMapSheet });
            ApplyQuietGridStyle(_dgvAttributeMapSheetMappings);
        }
    }
}
