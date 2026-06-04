namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmBlockAssignment
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel root;
        private FlowLayoutPanel modePanel;
        private Label lblAssignBy;
        private RadioButton _rdoSourceLayer;
        private RadioButton _rdoObject;
        private RadioButton _rdoAutoLabels;
        private FlowLayoutPanel optionsPanel;
        private CheckBox _chkZoomToSelected;
        private CheckBox _chkReplaceExisting;
        private CheckBox _chkCreateMissingBlocks;
        private Label _lblLabelLayer;
        private ComboBox _cboLabelLayer;
        private Panel gridPanel;
        private DataGridView _dgvLayerMappings;
        private DataGridView _dgvObjects;
        private Label _lblStatus;
        private FlowLayoutPanel bottomPanel;
        private Button _btnClose;
        private Button _btnApplyMappings;
        private Button _btnAutoAssign;
        private Button _btnRemoveSelected;
        private Button _btnRemoveAll;
        private Button _btnAssignSelected;
        private DataGridViewTextBoxColumn colSourceLayer;
        private DataGridViewTextBoxColumn colCount;
        private DataGridViewComboBoxColumn colBlock;
        private DataGridViewTextBoxColumn colObjectSource;
        private DataGridViewTextBoxColumn colObjectArea;
        private DataGridViewTextBoxColumn colObjectLabel;
        private DataGridViewComboBoxColumn colObjectBlock;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle objectHeaderStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle objectCellStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle mappingHeaderStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle mappingCellStyle = new DataGridViewCellStyle();
            colSourceLayer = new DataGridViewTextBoxColumn();
            colCount = new DataGridViewTextBoxColumn();
            colBlock = new DataGridViewComboBoxColumn();
            colObjectSource = new DataGridViewTextBoxColumn();
            colObjectArea = new DataGridViewTextBoxColumn();
            colObjectLabel = new DataGridViewTextBoxColumn();
            colObjectBlock = new DataGridViewComboBoxColumn();
            root = new TableLayoutPanel();
            modePanel = new FlowLayoutPanel();
            lblAssignBy = new Label();
            _rdoSourceLayer = new RadioButton();
            _rdoObject = new RadioButton();
            _rdoAutoLabels = new RadioButton();
            optionsPanel = new FlowLayoutPanel();
            _chkZoomToSelected = new CheckBox();
            _chkReplaceExisting = new CheckBox();
            _chkCreateMissingBlocks = new CheckBox();
            _lblLabelLayer = new Label();
            _cboLabelLayer = new ComboBox();
            gridPanel = new Panel();
            _dgvObjects = new DataGridView();
            _dgvLayerMappings = new DataGridView();
            _lblStatus = new Label();
            bottomPanel = new FlowLayoutPanel();
            _btnClose = new Button();
            _btnApplyMappings = new Button();
            _btnAutoAssign = new Button();
            _btnRemoveAll = new Button();
            _btnRemoveSelected = new Button();
            _btnAssignSelected = new Button();
            root.SuspendLayout();
            modePanel.SuspendLayout();
            optionsPanel.SuspendLayout();
            gridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvObjects).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_dgvLayerMappings).BeginInit();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // colSourceLayer
            // 
            colSourceLayer.HeaderText = "AutoCAD/DXF source layer";
            colSourceLayer.MinimumWidth = 145;
            colSourceLayer.Name = "colSourceLayer";
            colSourceLayer.ReadOnly = true;
            colSourceLayer.Width = 168;
            // 
            // colCount
            // 
            colCount.HeaderText = "Objects";
            colCount.MinimumWidth = 65;
            colCount.Name = "colCount";
            colCount.ReadOnly = true;
            colCount.Width = 78;
            // 
            // colBlock
            // 
            colBlock.FlatStyle = FlatStyle.Flat;
            colBlock.HeaderText = "Defined Block";
            colBlock.MinimumWidth = 180;
            colBlock.Name = "colBlock";
            colBlock.Width = 190;
            // 
            // colObjectSource
            // 
            colObjectSource.HeaderText = "Source layer";
            colObjectSource.MinimumWidth = 110;
            colObjectSource.Name = "colObjectSource";
            colObjectSource.ReadOnly = true;
            colObjectSource.Width = 115;
            // 
            // colObjectArea
            // 
            colObjectArea.HeaderText = "Area";
            colObjectArea.MinimumWidth = 70;
            colObjectArea.Name = "colObjectArea";
            colObjectArea.ReadOnly = true;
            colObjectArea.Width = 78;
            // 
            // colObjectLabel
            // 
            colObjectLabel.HeaderText = "Label";
            colObjectLabel.MinimumWidth = 95;
            colObjectLabel.Name = "colObjectLabel";
            colObjectLabel.ReadOnly = true;
            colObjectLabel.Width = 105;
            // 
            // colObjectBlock
            // 
            colObjectBlock.FlatStyle = FlatStyle.Flat;
            colObjectBlock.HeaderText = "Defined Block";
            colObjectBlock.MinimumWidth = 170;
            colObjectBlock.Name = "colObjectBlock";
            colObjectBlock.Width = 170;
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Controls.Add(modePanel, 0, 0);
            root.Controls.Add(optionsPanel, 0, 1);
            root.Controls.Add(gridPanel, 0, 2);
            root.Controls.Add(_lblStatus, 0, 3);
            root.Controls.Add(bottomPanel, 0, 4);
            root.Dock = DockStyle.Fill;
            root.Location = new Point(0, 0);
            root.Name = "root";
            root.Padding = new Padding(10);
            root.RowCount = 5;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            root.Size = new Size(760, 501);
            root.TabIndex = 0;
            // 
            // modePanel
            // 
            modePanel.Controls.Add(lblAssignBy);
            modePanel.Controls.Add(_rdoSourceLayer);
            modePanel.Controls.Add(_rdoObject);
            modePanel.Controls.Add(_rdoAutoLabels);
            modePanel.Dock = DockStyle.Fill;
            modePanel.Location = new Point(13, 13);
            modePanel.Name = "modePanel";
            modePanel.Size = new Size(734, 33);
            modePanel.TabIndex = 0;
            modePanel.WrapContents = false;
            // 
            // lblAssignBy
            // 
            lblAssignBy.Location = new Point(3, 0);
            lblAssignBy.Name = "lblAssignBy";
            lblAssignBy.Size = new Size(86, 30);
            lblAssignBy.TabIndex = 0;
            lblAssignBy.Text = "Assign By :";
            lblAssignBy.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _rdoSourceLayer
            // 
            _rdoSourceLayer.Checked = true;
            _rdoSourceLayer.Location = new Point(95, 3);
            _rdoSourceLayer.Name = "_rdoSourceLayer";
            _rdoSourceLayer.Size = new Size(116, 26);
            _rdoSourceLayer.TabIndex = 0;
            _rdoSourceLayer.TabStop = true;
            _rdoSourceLayer.Text = "Source layer";
            _rdoSourceLayer.UseVisualStyleBackColor = true;
            // 
            // _rdoObject
            // 
            _rdoObject.Location = new Point(217, 3);
            _rdoObject.Name = "_rdoObject";
            _rdoObject.Size = new Size(75, 26);
            _rdoObject.TabIndex = 1;
            _rdoObject.Text = "Object";
            _rdoObject.UseVisualStyleBackColor = true;
            // 
            // _rdoAutoLabels
            // 
            _rdoAutoLabels.Location = new Point(298, 3);
            _rdoAutoLabels.Name = "_rdoAutoLabels";
            _rdoAutoLabels.Size = new Size(110, 26);
            _rdoAutoLabels.TabIndex = 2;
            _rdoAutoLabels.Text = "Auto labels";
            _rdoAutoLabels.UseVisualStyleBackColor = true;
            // 
            // optionsPanel
            // 
            optionsPanel.Controls.Add(_chkZoomToSelected);
            optionsPanel.Controls.Add(_chkReplaceExisting);
            optionsPanel.Controls.Add(_chkCreateMissingBlocks);
            optionsPanel.Controls.Add(_lblLabelLayer);
            optionsPanel.Controls.Add(_cboLabelLayer);
            optionsPanel.Dock = DockStyle.Fill;
            optionsPanel.Location = new Point(13, 52);
            optionsPanel.Name = "optionsPanel";
            optionsPanel.Size = new Size(734, 39);
            optionsPanel.TabIndex = 1;
            optionsPanel.WrapContents = false;
            // 
            // _chkZoomToSelected
            // 
            _chkZoomToSelected.Checked = true;
            _chkZoomToSelected.CheckState = CheckState.Checked;
            _chkZoomToSelected.Location = new Point(3, 3);
            _chkZoomToSelected.Name = "_chkZoomToSelected";
            _chkZoomToSelected.Size = new Size(128, 26);
            _chkZoomToSelected.TabIndex = 0;
            _chkZoomToSelected.Text = "Zoom selected";
            _chkZoomToSelected.UseVisualStyleBackColor = true;
            // 
            // _chkReplaceExisting
            // 
            _chkReplaceExisting.Location = new Point(137, 3);
            _chkReplaceExisting.Name = "_chkReplaceExisting";
            _chkReplaceExisting.Size = new Size(124, 26);
            _chkReplaceExisting.TabIndex = 1;
            _chkReplaceExisting.Text = "Replace existing";
            _chkReplaceExisting.UseVisualStyleBackColor = true;
            // 
            // _chkCreateMissingBlocks
            // 
            _chkCreateMissingBlocks.Checked = true;
            _chkCreateMissingBlocks.CheckState = CheckState.Checked;
            _chkCreateMissingBlocks.Location = new Point(275, 3);
            _chkCreateMissingBlocks.Name = "_chkCreateMissingBlocks";
            _chkCreateMissingBlocks.Size = new Size(112, 26);
            _chkCreateMissingBlocks.TabIndex = 2;
            _chkCreateMissingBlocks.Text = "Create data";
            _chkCreateMissingBlocks.UseVisualStyleBackColor = true;
            // 
            // _lblLabelLayer
            // 
            _lblLabelLayer.Location = new Point(393, 0);
            _lblLabelLayer.Name = "_lblLabelLayer";
            _lblLabelLayer.Size = new Size(65, 30);
            _lblLabelLayer.TabIndex = 3;
            _lblLabelLayer.Text = "Labels:";
            _lblLabelLayer.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboLabelLayer
            // 
            _cboLabelLayer.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboLabelLayer.FormattingEnabled = true;
            _cboLabelLayer.Location = new Point(464, 3);
            _cboLabelLayer.Name = "_cboLabelLayer";
            _cboLabelLayer.Size = new Size(240, 28);
            _cboLabelLayer.TabIndex = 4;
            // 
            // gridPanel
            // 
            gridPanel.Controls.Add(_dgvObjects);
            gridPanel.Controls.Add(_dgvLayerMappings);
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.Location = new Point(13, 97);
            gridPanel.Name = "gridPanel";
            gridPanel.Size = new Size(734, 318);
            gridPanel.TabIndex = 2;
            // 
            // _dgvObjects
            // 
            _dgvObjects.AllowUserToAddRows = false;
            _dgvObjects.AllowUserToDeleteRows = false;
            _dgvObjects.AllowUserToResizeRows = false;
            _dgvObjects.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvObjects.BackgroundColor = SystemColors.Window;
            objectHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            objectHeaderStyle.BackColor = SystemColors.Control;
            objectHeaderStyle.Font = new Font("Segoe UI", 9F);
            objectHeaderStyle.ForeColor = SystemColors.WindowText;
            objectHeaderStyle.SelectionBackColor = SystemColors.Highlight;
            objectHeaderStyle.SelectionForeColor = SystemColors.HighlightText;
            objectHeaderStyle.WrapMode = DataGridViewTriState.True;
            _dgvObjects.ColumnHeadersDefaultCellStyle = objectHeaderStyle;
            _dgvObjects.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvObjects.Columns.AddRange(new DataGridViewColumn[] { colObjectSource, colObjectArea, colObjectLabel, colObjectBlock });
            objectCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            objectCellStyle.BackColor = SystemColors.Window;
            objectCellStyle.Font = new Font("Segoe UI", 9F);
            objectCellStyle.ForeColor = SystemColors.ControlText;
            objectCellStyle.SelectionBackColor = SystemColors.Highlight;
            objectCellStyle.SelectionForeColor = SystemColors.HighlightText;
            objectCellStyle.WrapMode = DataGridViewTriState.False;
            _dgvObjects.DefaultCellStyle = objectCellStyle;
            _dgvObjects.Dock = DockStyle.Fill;
            _dgvObjects.EnableHeadersVisualStyles = false;
            _dgvObjects.GridColor = Color.FromArgb(214, 219, 226);
            _dgvObjects.Location = new Point(0, 0);
            _dgvObjects.MultiSelect = false;
            _dgvObjects.Name = "_dgvObjects";
            _dgvObjects.RowHeadersVisible = false;
            _dgvObjects.RowHeadersWidth = 51;
            _dgvObjects.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvObjects.Size = new Size(734, 318);
            _dgvObjects.TabIndex = 1;
            // 
            // _dgvLayerMappings
            // 
            _dgvLayerMappings.AllowUserToAddRows = false;
            _dgvLayerMappings.AllowUserToDeleteRows = false;
            _dgvLayerMappings.AllowUserToResizeRows = false;
            _dgvLayerMappings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvLayerMappings.BackgroundColor = SystemColors.Window;
            mappingHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            mappingHeaderStyle.BackColor = SystemColors.Control;
            mappingHeaderStyle.Font = new Font("Segoe UI", 9F);
            mappingHeaderStyle.ForeColor = SystemColors.WindowText;
            mappingHeaderStyle.SelectionBackColor = SystemColors.Highlight;
            mappingHeaderStyle.SelectionForeColor = SystemColors.HighlightText;
            mappingHeaderStyle.WrapMode = DataGridViewTriState.True;
            _dgvLayerMappings.ColumnHeadersDefaultCellStyle = mappingHeaderStyle;
            _dgvLayerMappings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvLayerMappings.Columns.AddRange(new DataGridViewColumn[] { colSourceLayer, colCount, colBlock });
            mappingCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            mappingCellStyle.BackColor = SystemColors.Window;
            mappingCellStyle.Font = new Font("Segoe UI", 9F);
            mappingCellStyle.ForeColor = SystemColors.ControlText;
            mappingCellStyle.SelectionBackColor = SystemColors.Highlight;
            mappingCellStyle.SelectionForeColor = SystemColors.HighlightText;
            mappingCellStyle.WrapMode = DataGridViewTriState.False;
            _dgvLayerMappings.DefaultCellStyle = mappingCellStyle;
            _dgvLayerMappings.Dock = DockStyle.Fill;
            _dgvLayerMappings.EnableHeadersVisualStyles = false;
            _dgvLayerMappings.GridColor = Color.FromArgb(214, 219, 226);
            _dgvLayerMappings.Location = new Point(0, 0);
            _dgvLayerMappings.MultiSelect = false;
            _dgvLayerMappings.Name = "_dgvLayerMappings";
            _dgvLayerMappings.RowHeadersVisible = false;
            _dgvLayerMappings.RowHeadersWidth = 51;
            _dgvLayerMappings.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _dgvLayerMappings.Size = new Size(734, 318);
            _dgvLayerMappings.TabIndex = 0;
            // 
            // _lblStatus
            // 
            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.ForeColor = Color.DimGray;
            _lblStatus.Location = new Point(13, 418);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Size = new Size(734, 31);
            _lblStatus.TabIndex = 3;
            _lblStatus.Text = "Loading block objects...";
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(_btnClose);
            bottomPanel.Controls.Add(_btnApplyMappings);
            bottomPanel.Controls.Add(_btnAutoAssign);
            bottomPanel.Controls.Add(_btnRemoveAll);
            bottomPanel.Controls.Add(_btnRemoveSelected);
            bottomPanel.Controls.Add(_btnAssignSelected);
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.FlowDirection = FlowDirection.RightToLeft;
            bottomPanel.Location = new Point(13, 452);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(734, 36);
            bottomPanel.TabIndex = 4;
            bottomPanel.WrapContents = false;
            // 
            // _btnClose
            // 
            _btnClose.DialogResult = DialogResult.OK;
            _btnClose.Location = new Point(641, 3);
            _btnClose.Name = "_btnClose";
            _btnClose.Size = new Size(90, 30);
            _btnClose.TabIndex = 5;
            _btnClose.Text = "Close";
            _btnClose.UseVisualStyleBackColor = true;
            // 
            // _btnApplyMappings
            // 
            _btnApplyMappings.Location = new Point(558, 3);
            _btnApplyMappings.Name = "_btnApplyMappings";
            _btnApplyMappings.Size = new Size(77, 30);
            _btnApplyMappings.TabIndex = 4;
            _btnApplyMappings.Text = "Apply";
            _btnApplyMappings.UseVisualStyleBackColor = true;
            // 
            // _btnAutoAssign
            // 
            _btnAutoAssign.Location = new Point(455, 3);
            _btnAutoAssign.Name = "_btnAutoAssign";
            _btnAutoAssign.Size = new Size(97, 30);
            _btnAutoAssign.TabIndex = 3;
            _btnAutoAssign.Text = "Auto Assign";
            _btnAutoAssign.UseVisualStyleBackColor = true;
            // 
            // _btnRemoveAll
            // 
            _btnRemoveAll.Location = new Point(364, 3);
            _btnRemoveAll.Name = "_btnRemoveAll";
            _btnRemoveAll.Size = new Size(85, 30);
            _btnRemoveAll.TabIndex = 2;
            _btnRemoveAll.Text = "Remove All";
            _btnRemoveAll.UseVisualStyleBackColor = true;
            // 
            // _btnRemoveSelected
            // 
            _btnRemoveSelected.Location = new Point(223, 3);
            _btnRemoveSelected.Name = "_btnRemoveSelected";
            _btnRemoveSelected.Size = new Size(135, 30);
            _btnRemoveSelected.TabIndex = 1;
            _btnRemoveSelected.Text = "Remove Selected";
            _btnRemoveSelected.UseVisualStyleBackColor = true;
            // 
            // _btnAssignSelected
            // 
            _btnAssignSelected.Location = new Point(112, 3);
            _btnAssignSelected.Name = "_btnAssignSelected";
            _btnAssignSelected.Size = new Size(105, 30);
            _btnAssignSelected.TabIndex = 0;
            _btnAssignSelected.Text = "Assign Selected";
            _btnAssignSelected.UseVisualStyleBackColor = true;
            // 
            // frmBlockAssignment
            // 
            AcceptButton = _btnApplyMappings;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _btnClose;
            ClientSize = new Size(760, 501);
            Controls.Add(root);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmBlockAssignment";
            StartPosition = FormStartPosition.Manual;
            Text = "Assign Block Data";
            root.ResumeLayout(false);
            modePanel.ResumeLayout(false);
            optionsPanel.ResumeLayout(false);
            gridPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_dgvObjects).EndInit();
            ((System.ComponentModel.ISupportInitialize)_dgvLayerMappings).EndInit();
            bottomPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
