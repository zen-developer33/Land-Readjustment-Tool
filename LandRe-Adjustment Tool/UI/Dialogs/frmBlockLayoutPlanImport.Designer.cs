namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmBlockLayoutPlanImport
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel _rootLayout;
        private Label _lblSummary;
        private TableLayoutPanel _crsLayout;
        private Label _lblSourceCrsCaption;
        private ComboBox _cmbSourceCrs;
        private Label _lblSourceCrsValue;
        private Label _lblProjectCrsCaption;
        private Label _lblProjectCrsValue;
        private TableLayoutPanel _labelLayout;
        private Label _lblBlockLabelsCaption;
        private ComboBox _cmbBlockLabelLayer;
        private DataGridView _grid;
        private DataGridViewCheckBoxColumn _colInclude;
        private DataGridViewTextBoxColumn _colLayer;
        private DataGridViewTextBoxColumn _colTypes;
        private DataGridViewComboBoxColumn _colTarget;
        private TableLayoutPanel _bottomLayout;
        private Label _lblSelection;
        private FlowLayoutPanel _buttonPanel;
        private Button _btnImport;
        private Button _btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            _rootLayout = new TableLayoutPanel();
            _lblSummary = new Label();
            _crsLayout = new TableLayoutPanel();
            _lblSourceCrsCaption = new Label();
            _cmbSourceCrs = new ComboBox();
            _lblSourceCrsValue = new Label();
            _lblProjectCrsCaption = new Label();
            _lblProjectCrsValue = new Label();
            _labelLayout = new TableLayoutPanel();
            _lblBlockLabelsCaption = new Label();
            _cmbBlockLabelLayer = new ComboBox();
            _grid = new DataGridView();
            _colInclude = new DataGridViewCheckBoxColumn();
            _colLayer = new DataGridViewTextBoxColumn();
            _colTypes = new DataGridViewTextBoxColumn();
            _colTarget = new DataGridViewComboBoxColumn();
            _bottomLayout = new TableLayoutPanel();
            _lblSelection = new Label();
            _buttonPanel = new FlowLayoutPanel();
            _btnImport = new Button();
            _btnCancel = new Button();
            _rootLayout.SuspendLayout();
            _crsLayout.SuspendLayout();
            _labelLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
            _bottomLayout.SuspendLayout();
            _buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _rootLayout
            // 
            _rootLayout.ColumnCount = 1;
            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _rootLayout.Controls.Add(_lblSummary, 0, 0);
            _rootLayout.Controls.Add(_crsLayout, 0, 1);
            _rootLayout.Controls.Add(_labelLayout, 0, 2);
            _rootLayout.Controls.Add(_grid, 0, 3);
            _rootLayout.Controls.Add(_bottomLayout, 0, 4);
            _rootLayout.Dock = DockStyle.Fill;
            _rootLayout.Location = new Point(0, 0);
            _rootLayout.Name = "_rootLayout";
            _rootLayout.Padding = new Padding(10);
            _rootLayout.RowCount = 5;
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            _rootLayout.Size = new Size(760, 420);
            _rootLayout.TabIndex = 0;
            // 
            // _lblSummary
            // 
            _lblSummary.AutoEllipsis = true;
            _lblSummary.Dock = DockStyle.Fill;
            _lblSummary.ForeColor = SystemColors.GrayText;
            _lblSummary.Location = new Point(10, 10);
            _lblSummary.Margin = new Padding(0);
            _lblSummary.Name = "_lblSummary";
            _lblSummary.Size = new Size(740, 26);
            _lblSummary.TabIndex = 0;
            _lblSummary.Text = "Source file";
            _lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _crsLayout
            // 
            _crsLayout.ColumnCount = 2;
            _crsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
            _crsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _crsLayout.Controls.Add(_lblSourceCrsCaption, 0, 0);
            _crsLayout.Controls.Add(_cmbSourceCrs, 1, 0);
            _crsLayout.Controls.Add(_lblSourceCrsValue, 1, 0);
            _crsLayout.Controls.Add(_lblProjectCrsCaption, 0, 1);
            _crsLayout.Controls.Add(_lblProjectCrsValue, 1, 1);
            _crsLayout.Dock = DockStyle.Fill;
            _crsLayout.Location = new Point(10, 36);
            _crsLayout.Margin = new Padding(0);
            _crsLayout.Name = "_crsLayout";
            _crsLayout.RowCount = 2;
            _crsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _crsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _crsLayout.Size = new Size(740, 58);
            _crsLayout.TabIndex = 1;
            // 
            // _lblSourceCrsCaption
            // 
            _lblSourceCrsCaption.Dock = DockStyle.Fill;
            _lblSourceCrsCaption.Location = new Point(0, 0);
            _lblSourceCrsCaption.Margin = new Padding(0);
            _lblSourceCrsCaption.Name = "_lblSourceCrsCaption";
            _lblSourceCrsCaption.Size = new Size(92, 28);
            _lblSourceCrsCaption.TabIndex = 0;
            _lblSourceCrsCaption.Text = "Source CRS";
            _lblSourceCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cmbSourceCrs
            // 
            _cmbSourceCrs.Dock = DockStyle.Fill;
            _cmbSourceCrs.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbSourceCrs.FormattingEnabled = true;
            _cmbSourceCrs.Location = new Point(92, 1);
            _cmbSourceCrs.Margin = new Padding(0, 1, 0, 1);
            _cmbSourceCrs.Name = "_cmbSourceCrs";
            _cmbSourceCrs.Size = new Size(648, 28);
            _cmbSourceCrs.TabIndex = 1;
            // 
            // _lblSourceCrsValue
            // 
            _lblSourceCrsValue.AutoEllipsis = true;
            _lblSourceCrsValue.Dock = DockStyle.Fill;
            _lblSourceCrsValue.Location = new Point(92, 0);
            _lblSourceCrsValue.Margin = new Padding(0);
            _lblSourceCrsValue.Name = "_lblSourceCrsValue";
            _lblSourceCrsValue.Size = new Size(648, 28);
            _lblSourceCrsValue.TabIndex = 2;
            _lblSourceCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblProjectCrsCaption
            // 
            _lblProjectCrsCaption.Dock = DockStyle.Fill;
            _lblProjectCrsCaption.Location = new Point(0, 28);
            _lblProjectCrsCaption.Margin = new Padding(0);
            _lblProjectCrsCaption.Name = "_lblProjectCrsCaption";
            _lblProjectCrsCaption.Size = new Size(92, 28);
            _lblProjectCrsCaption.TabIndex = 3;
            _lblProjectCrsCaption.Text = "Project CRS";
            _lblProjectCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblProjectCrsValue
            // 
            _lblProjectCrsValue.AutoEllipsis = true;
            _lblProjectCrsValue.Dock = DockStyle.Fill;
            _lblProjectCrsValue.ForeColor = SystemColors.GrayText;
            _lblProjectCrsValue.Location = new Point(92, 28);
            _lblProjectCrsValue.Margin = new Padding(0);
            _lblProjectCrsValue.Name = "_lblProjectCrsValue";
            _lblProjectCrsValue.Size = new Size(648, 28);
            _lblProjectCrsValue.TabIndex = 4;
            _lblProjectCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _labelLayout
            // 
            _labelLayout.ColumnCount = 2;
            _labelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
            _labelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _labelLayout.Controls.Add(_lblBlockLabelsCaption, 0, 0);
            _labelLayout.Controls.Add(_cmbBlockLabelLayer, 1, 0);
            _labelLayout.Dock = DockStyle.Fill;
            _labelLayout.Location = new Point(10, 94);
            _labelLayout.Margin = new Padding(0);
            _labelLayout.Name = "_labelLayout";
            _labelLayout.RowCount = 1;
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _labelLayout.Size = new Size(740, 32);
            _labelLayout.TabIndex = 2;
            // 
            // _lblBlockLabelsCaption
            // 
            _lblBlockLabelsCaption.Dock = DockStyle.Fill;
            _lblBlockLabelsCaption.Location = new Point(0, 0);
            _lblBlockLabelsCaption.Margin = new Padding(0);
            _lblBlockLabelsCaption.Name = "_lblBlockLabelsCaption";
            _lblBlockLabelsCaption.Size = new Size(92, 32);
            _lblBlockLabelsCaption.TabIndex = 0;
            _lblBlockLabelsCaption.Text = "Block labels";
            _lblBlockLabelsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cmbBlockLabelLayer
            // 
            _cmbBlockLabelLayer.Dock = DockStyle.Fill;
            _cmbBlockLabelLayer.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbBlockLabelLayer.FormattingEnabled = true;
            _cmbBlockLabelLayer.Location = new Point(92, 2);
            _cmbBlockLabelLayer.Margin = new Padding(0, 2, 0, 2);
            _cmbBlockLabelLayer.Name = "_cmbBlockLabelLayer";
            _cmbBlockLabelLayer.Size = new Size(648, 28);
            _cmbBlockLabelLayer.TabIndex = 1;
            _cmbBlockLabelLayer.SelectedIndexChanged += cmbBlockLabelLayer_SelectedIndexChanged;
            // 
            // _grid
            // 
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.BorderStyle = BorderStyle.FixedSingle;
            _grid.ColumnHeadersHeight = 28;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _grid.Columns.AddRange(new DataGridViewColumn[] { _colInclude, _colLayer, _colTypes, _colTarget });
            _grid.Dock = DockStyle.Fill;
            _grid.EditMode = DataGridViewEditMode.EditOnEnter;
            _grid.EnableHeadersVisualStyles = false;
            _grid.Location = new Point(10, 132);
            _grid.Margin = new Padding(0, 6, 0, 6);
            _grid.MultiSelect = false;
            _grid.Name = "_grid";
            _grid.RowHeadersVisible = false;
            _grid.RowTemplate.Height = 25;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.Size = new Size(740, 232);
            _grid.TabIndex = 3;
            _grid.CellMouseClick += grid_CellMouseClick;
            _grid.CellPainting += Grid_CellPainting;
            _grid.CellValueChanged += grid_CellValueChanged;
            _grid.CurrentCellDirtyStateChanged += grid_CurrentCellDirtyStateChanged;
            _grid.DataError += grid_DataError;
            // 
            // _colInclude
            // 
            _colInclude.HeaderText = "";
            _colInclude.MinimumWidth = 36;
            _colInclude.Name = IncludeColumn;
            _colInclude.Width = 36;
            // 
            // _colLayer
            // 
            _colLayer.HeaderText = "Source layer";
            _colLayer.MinimumWidth = 160;
            _colLayer.Name = LayerColumn;
            _colLayer.ReadOnly = true;
            _colLayer.Width = 220;
            // 
            // _colTypes
            // 
            _colTypes.HeaderText = "Object types";
            _colTypes.MinimumWidth = 150;
            _colTypes.Name = TypesColumn;
            _colTypes.ReadOnly = true;
            _colTypes.Width = 180;
            // 
            // _colTarget
            // 
            _colTarget.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _colTarget.FlatStyle = FlatStyle.Flat;
            _colTarget.HeaderText = "Target layer";
            _colTarget.MinimumWidth = 180;
            _colTarget.Name = TargetColumn;
            // 
            // _bottomLayout
            // 
            _bottomLayout.ColumnCount = 2;
            _bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 188F));
            _bottomLayout.Controls.Add(_lblSelection, 0, 0);
            _bottomLayout.Controls.Add(_buttonPanel, 1, 0);
            _bottomLayout.Dock = DockStyle.Fill;
            _bottomLayout.Location = new Point(10, 370);
            _bottomLayout.Margin = new Padding(0);
            _bottomLayout.Name = "_bottomLayout";
            _bottomLayout.RowCount = 1;
            _bottomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _bottomLayout.Size = new Size(740, 40);
            _bottomLayout.TabIndex = 4;
            // 
            // _lblSelection
            // 
            _lblSelection.Dock = DockStyle.Fill;
            _lblSelection.ForeColor = SystemColors.GrayText;
            _lblSelection.Location = new Point(0, 0);
            _lblSelection.Margin = new Padding(0);
            _lblSelection.Name = "_lblSelection";
            _lblSelection.Size = new Size(552, 40);
            _lblSelection.TabIndex = 0;
            _lblSelection.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _buttonPanel
            // 
            _buttonPanel.Controls.Add(_btnCancel);
            _buttonPanel.Controls.Add(_btnImport);
            _buttonPanel.Dock = DockStyle.Fill;
            _buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            _buttonPanel.Location = new Point(552, 0);
            _buttonPanel.Margin = new Padding(0);
            _buttonPanel.Name = "_buttonPanel";
            _buttonPanel.Size = new Size(188, 40);
            _buttonPanel.TabIndex = 1;
            _buttonPanel.WrapContents = false;
            // 
            // _btnImport
            // 
            _btnImport.DialogResult = DialogResult.OK;
            _btnImport.Location = new Point(7, 4);
            _btnImport.Margin = new Padding(4);
            _btnImport.Name = "_btnImport";
            _btnImport.Size = new Size(86, 30);
            _btnImport.TabIndex = 1;
            _btnImport.Text = "Import";
            _btnImport.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(99, 4);
            _btnCancel.Margin = new Padding(4);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new Size(86, 30);
            _btnCancel.TabIndex = 0;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmBlockLayoutPlanImport
            // 
            AcceptButton = _btnImport;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _btnCancel;
            ClientSize = new Size(760, 420);
            Controls.Add(_rootLayout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmBlockLayoutPlanImport";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Import Block Layout Plan";
            _rootLayout.ResumeLayout(false);
            _crsLayout.ResumeLayout(false);
            _labelLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            _bottomLayout.ResumeLayout(false);
            _buttonPanel.ResumeLayout(false);
            ResumeLayout(false);

            _lblSummary.Text = string.Format(
                "{0}  |  {1}  |  {2} source layer(s)",
                Path.GetFileName(_fileInfo.FilePath),
                _fileInfo.FileFormat,
                _fileInfo.Layers.Count);
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(32, 41, 57);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            foreach (string target in GetTargetLayerDisplayNames())
                _colTarget.Items.Add(target);
        }

        private void cmbBlockLabelLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnsureSelectedLabelLayerIncluded();
        }

        private void grid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_grid.IsCurrentCellDirty)
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                ApplyRowEnabledStyle(_grid.Rows[e.RowIndex]);

            UpdateImportState();
        }

        private void grid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn includeColumn = _grid.Columns[IncludeColumn];
            int includeColumnIndex = includeColumn == null ? -1 : includeColumn.Index;
            if (e.RowIndex == -1 && e.ColumnIndex == includeColumnIndex)
                SetAllIncluded(_includeHeaderState != CheckState.Checked);
        }

        private void grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }
    }
}
