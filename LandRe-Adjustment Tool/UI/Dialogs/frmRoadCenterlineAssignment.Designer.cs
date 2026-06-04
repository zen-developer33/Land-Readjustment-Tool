namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmRoadCenterlineAssignment
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel root;
        private FlowLayoutPanel modePanel;
        private Label lblAssignBy;
        private RadioButton _rdoSourceLayer;
        private RadioButton _rdoObject;
        private CheckBox _chkReplaceExisting;
        private CheckBox _chkZoomToSelected;
        private Panel gridPanel;
        private DataGridView _dgvLayerMappings;
        private DataGridView _dgvObjects;
        private Label _lblStatus;
        private FlowLayoutPanel bottomPanel;
        private Button _btnClose;
        private Button _btnApplyMappings;
        private Button _btnRemoveSelected;
        private Button _btnRemoveAll;
        private Button _btnAssignSelected;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            colSourceLayer = new DataGridViewTextBoxColumn();
            colCount = new DataGridViewTextBoxColumn();
            colRoad = new DataGridViewComboBoxColumn();
            colObjectSource = new DataGridViewTextBoxColumn();
            colObjectLength = new DataGridViewTextBoxColumn();
            colObjectRoad = new DataGridViewComboBoxColumn();
            root = new TableLayoutPanel();
            modePanel = new FlowLayoutPanel();
            lblAssignBy = new Label();
            _rdoSourceLayer = new RadioButton();
            _rdoObject = new RadioButton();
            _chkZoomToSelected = new CheckBox();
            _chkReplaceExisting = new CheckBox();
            gridPanel = new Panel();
            _dgvObjects = new DataGridView();
            _dgvLayerMappings = new DataGridView();
            _lblStatus = new Label();
            bottomPanel = new FlowLayoutPanel();
            _btnClose = new Button();
            _btnApplyMappings = new Button();
            _btnRemoveSelected = new Button();
            _btnRemoveAll = new Button();
            _btnAssignSelected = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            root.SuspendLayout();
            modePanel.SuspendLayout();
            gridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvObjects).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_dgvLayerMappings).BeginInit();
            bottomPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
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
            colCount.Width = 88;
            // 
            // colRoad
            // 
            colRoad.FlatStyle = FlatStyle.Flat;
            colRoad.HeaderText = "Defined Road";
            colRoad.MinimumWidth = 170;
            colRoad.Name = "colRoad";
            colRoad.Width = 170;
            // 
            // colObjectSource
            // 
            colObjectSource.HeaderText = "Source layer";
            colObjectSource.MinimumWidth = 120;
            colObjectSource.Name = "colObjectSource";
            colObjectSource.ReadOnly = true;
            colObjectSource.Width = 120;
            // 
            // colObjectLength
            // 
            colObjectLength.HeaderText = "Length";
            colObjectLength.MinimumWidth = 70;
            colObjectLength.Name = "colObjectLength";
            colObjectLength.ReadOnly = true;
            colObjectLength.Width = 83;
            // 
            // colObjectRoad
            // 
            colObjectRoad.FlatStyle = FlatStyle.Flat;
            colObjectRoad.HeaderText = "Defined Road";
            colObjectRoad.MinimumWidth = 170;
            colObjectRoad.Name = "colObjectRoad";
            colObjectRoad.Width = 170;
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Controls.Add(flowLayoutPanel1, 0, 1);
            root.Controls.Add(modePanel, 0, 0);
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
            root.Size = new Size(499, 501);
            root.TabIndex = 0;
            // 
            // modePanel
            // 
            modePanel.Controls.Add(lblAssignBy);
            modePanel.Controls.Add(_rdoSourceLayer);
            modePanel.Controls.Add(_rdoObject);
            modePanel.Dock = DockStyle.Fill;
            modePanel.Location = new Point(13, 13);
            modePanel.Name = "modePanel";
            modePanel.Size = new Size(473, 33);
            modePanel.TabIndex = 0;
            modePanel.WrapContents = false;
            // 
            // lblAssignBy
            // 
            lblAssignBy.Location = new Point(3, 0);
            lblAssignBy.Name = "lblAssignBy";
            lblAssignBy.Size = new Size(90, 30);
            lblAssignBy.TabIndex = 0;
            lblAssignBy.Text = "Assign By :";
            lblAssignBy.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _rdoSourceLayer
            // 
            _rdoSourceLayer.Checked = true;
            _rdoSourceLayer.Location = new Point(99, 3);
            _rdoSourceLayer.Name = "_rdoSourceLayer";
            _rdoSourceLayer.Size = new Size(120, 26);
            _rdoSourceLayer.TabIndex = 0;
            _rdoSourceLayer.TabStop = true;
            _rdoSourceLayer.Text = "Source layer";
            _rdoSourceLayer.UseVisualStyleBackColor = true;
            // 
            // _rdoObject
            // 
            _rdoObject.Location = new Point(225, 3);
            _rdoObject.Name = "_rdoObject";
            _rdoObject.Size = new Size(75, 26);
            _rdoObject.TabIndex = 1;
            _rdoObject.Text = "Object";
            _rdoObject.UseVisualStyleBackColor = true;
            // 
            // _chkZoomToSelected
            // 
            _chkZoomToSelected.Checked = true;
            _chkZoomToSelected.CheckState = CheckState.Checked;
            _chkZoomToSelected.Location = new Point(3, 3);
            _chkZoomToSelected.Name = "_chkZoomToSelected";
            _chkZoomToSelected.Size = new Size(135, 26);
            _chkZoomToSelected.TabIndex = 3;
            _chkZoomToSelected.Text = "Zoom selected";
            _chkZoomToSelected.UseVisualStyleBackColor = true;
            _chkZoomToSelected.CheckedChanged += _chkZoomToSelected_CheckedChanged;
            // 
            // _chkReplaceExisting
            // 
            _chkReplaceExisting.Location = new Point(144, 3);
            _chkReplaceExisting.Name = "_chkReplaceExisting";
            _chkReplaceExisting.Size = new Size(141, 26);
            _chkReplaceExisting.TabIndex = 2;
            _chkReplaceExisting.Text = "Replace existing";
            _chkReplaceExisting.UseVisualStyleBackColor = true;
            // 
            // gridPanel
            // 
            gridPanel.Controls.Add(_dgvObjects);
            gridPanel.Controls.Add(_dgvLayerMappings);
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.Location = new Point(13, 97);
            gridPanel.Name = "gridPanel";
            gridPanel.Size = new Size(473, 318);
            gridPanel.TabIndex = 1;
            // 
            // _dgvObjects
            // 
            _dgvObjects.AllowUserToAddRows = false;
            _dgvObjects.AllowUserToDeleteRows = false;
            _dgvObjects.AllowUserToResizeRows = false;
            _dgvObjects.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvObjects.BackgroundColor = SystemColors.Window;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            _dgvObjects.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            _dgvObjects.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvObjects.Columns.AddRange(new DataGridViewColumn[] { colObjectSource, colObjectLength, colObjectRoad });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            _dgvObjects.DefaultCellStyle = dataGridViewCellStyle2;
            _dgvObjects.Dock = DockStyle.Fill;
            _dgvObjects.EnableHeadersVisualStyles = false;
            _dgvObjects.GridColor = Color.FromArgb(214, 219, 226);
            _dgvObjects.Location = new Point(0, 0);
            _dgvObjects.MultiSelect = false;
            _dgvObjects.Name = "_dgvObjects";
            _dgvObjects.RowHeadersVisible = false;
            _dgvObjects.RowHeadersWidth = 51;
            _dgvObjects.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvObjects.Size = new Size(473, 318);
            _dgvObjects.TabIndex = 1;
            // 
            // _dgvLayerMappings
            // 
            _dgvLayerMappings.AllowUserToAddRows = false;
            _dgvLayerMappings.AllowUserToDeleteRows = false;
            _dgvLayerMappings.AllowUserToResizeRows = false;
            _dgvLayerMappings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvLayerMappings.BackgroundColor = SystemColors.Window;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            _dgvLayerMappings.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            _dgvLayerMappings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvLayerMappings.Columns.AddRange(new DataGridViewColumn[] { colSourceLayer, colCount, colRoad });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            _dgvLayerMappings.DefaultCellStyle = dataGridViewCellStyle4;
            _dgvLayerMappings.Dock = DockStyle.Fill;
            _dgvLayerMappings.EnableHeadersVisualStyles = false;
            _dgvLayerMappings.GridColor = Color.FromArgb(214, 219, 226);
            _dgvLayerMappings.Location = new Point(0, 0);
            _dgvLayerMappings.MultiSelect = false;
            _dgvLayerMappings.Name = "_dgvLayerMappings";
            _dgvLayerMappings.RowHeadersVisible = false;
            _dgvLayerMappings.RowHeadersWidth = 51;
            _dgvLayerMappings.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _dgvLayerMappings.Size = new Size(473, 318);
            _dgvLayerMappings.TabIndex = 0;
            // 
            // _lblStatus
            // 
            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.ForeColor = Color.DimGray;
            _lblStatus.Location = new Point(13, 418);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Size = new Size(473, 31);
            _lblStatus.TabIndex = 2;
            _lblStatus.Text = "Loading road centerline objects...";
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(_btnClose);
            bottomPanel.Controls.Add(_btnApplyMappings);
            bottomPanel.Controls.Add(_btnRemoveAll);
            bottomPanel.Controls.Add(_btnRemoveSelected);
            bottomPanel.Controls.Add(_btnAssignSelected);
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.FlowDirection = FlowDirection.RightToLeft;
            bottomPanel.Location = new Point(13, 452);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(473, 36);
            bottomPanel.TabIndex = 3;
            bottomPanel.WrapContents = false;
            // 
            // _btnClose
            // 
            _btnClose.DialogResult = DialogResult.OK;
            _btnClose.Location = new Point(380, 3);
            _btnClose.Name = "_btnClose";
            _btnClose.Size = new Size(90, 30);
            _btnClose.TabIndex = 3;
            _btnClose.Text = "Close";
            _btnClose.UseVisualStyleBackColor = true;
            // 
            // _btnApplyMappings
            // 
            _btnApplyMappings.Location = new Point(297, 3);
            _btnApplyMappings.Name = "_btnApplyMappings";
            _btnApplyMappings.Size = new Size(77, 30);
            _btnApplyMappings.TabIndex = 2;
            _btnApplyMappings.Text = "Apply";
            _btnApplyMappings.UseVisualStyleBackColor = true;
            // 
            // _btnRemoveSelected
            // 
            _btnRemoveSelected.Location = new Point(156, 3);
            _btnRemoveSelected.Name = "_btnRemoveSelected";
            _btnRemoveSelected.Size = new Size(135, 30);
            _btnRemoveSelected.TabIndex = 1;
            _btnRemoveSelected.Text = "Remove Selected";
            _btnRemoveSelected.UseVisualStyleBackColor = true;
            // 
            // _btnRemoveAll
            // 
            _btnRemoveAll.Location = new Point(206, 3);
            _btnRemoveAll.Name = "_btnRemoveAll";
            _btnRemoveAll.Size = new Size(85, 30);
            _btnRemoveAll.TabIndex = 4;
            _btnRemoveAll.Text = "Remove All";
            _btnRemoveAll.UseVisualStyleBackColor = true;
            // 
            // _btnAssignSelected
            // 
            _btnAssignSelected.Location = new Point(45, 3);
            _btnAssignSelected.Name = "_btnAssignSelected";
            _btnAssignSelected.Size = new Size(105, 30);
            _btnAssignSelected.TabIndex = 0;
            _btnAssignSelected.Text = "Assign Selected";
            _btnAssignSelected.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(_chkZoomToSelected);
            flowLayoutPanel1.Controls.Add(_chkReplaceExisting);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(13, 52);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(473, 39);
            flowLayoutPanel1.TabIndex = 4;
            flowLayoutPanel1.WrapContents = false;
            // 
            // frmRoadCenterlineAssignment
            // 
            AcceptButton = _btnApplyMappings;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _btnClose;
            ClientSize = new Size(499, 501);
            Controls.Add(root);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmRoadCenterlineAssignment";
            StartPosition = FormStartPosition.Manual;
            Text = "Assign Road Data";
            root.ResumeLayout(false);
            modePanel.ResumeLayout(false);
            gridPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_dgvObjects).EndInit();
            ((System.ComponentModel.ISupportInitialize)_dgvLayerMappings).EndInit();
            bottomPanel.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private DataGridViewTextBoxColumn colSourceLayer;
        private DataGridViewTextBoxColumn colCount;
        private DataGridViewComboBoxColumn colRoad;
        private DataGridViewTextBoxColumn colObjectSource;
        private DataGridViewTextBoxColumn colObjectLength;
        private DataGridViewComboBoxColumn colObjectRoad;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
