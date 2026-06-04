namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmExternalLayerImport
    {
        private System.ComponentModel.IContainer components = null;
        private Label _lblSummary;
        private DataGridView _grid;
        private DataGridViewCheckBoxColumn colInclude;
        private DataGridViewTextBoxColumn colLayer;
        private DataGridViewTextBoxColumn colObjects;
        private DataGridViewTextBoxColumn colTypes;
        private Button _btnImport;
        private Button _btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _lblSummary = new Label();
            _grid = new DataGridView();
            colInclude = new DataGridViewCheckBoxColumn();
            colLayer = new DataGridViewTextBoxColumn();
            colObjects = new DataGridViewTextBoxColumn();
            colTypes = new DataGridViewTextBoxColumn();
            _btnImport = new Button();
            _btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
            SuspendLayout();
            // 
            // _lblSummary
            // 
            _lblSummary.AutoSize = false;
            _lblSummary.Location = new Point(12, 10);
            _lblSummary.Name = "_lblSummary";
            _lblSummary.Size = new Size(536, 42);
            _lblSummary.TabIndex = 0;
            _lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _grid
            // 
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.BorderStyle = BorderStyle.FixedSingle;
            _grid.ColumnHeadersHeight = 26;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _grid.Columns.AddRange(new DataGridViewColumn[] { colInclude, colLayer, colObjects, colTypes });
            _grid.EnableHeadersVisualStyles = false;
            _grid.Location = new Point(12, 58);
            _grid.MultiSelect = false;
            _grid.Name = "_grid";
            _grid.RowHeadersVisible = false;
            _grid.RowHeadersWidth = 51;
            _grid.RowTemplate.Height = 24;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.Size = new Size(536, 246);
            _grid.TabIndex = 1;
            // 
            // colInclude
            // 
            colInclude.HeaderText = "";
            colInclude.MinimumWidth = 6;
            colInclude.Name = IncludeColumn;
            colInclude.Width = 38;
            // 
            // colLayer
            // 
            colLayer.HeaderText = "Layer";
            colLayer.MinimumWidth = 6;
            colLayer.Name = LayerColumn;
            colLayer.ReadOnly = true;
            colLayer.Width = 190;
            // 
            // colObjects
            // 
            colObjects.HeaderText = "Objects";
            colObjects.MinimumWidth = 6;
            colObjects.Name = ObjectsColumn;
            colObjects.ReadOnly = true;
            colObjects.Width = 70;
            // 
            // colTypes
            // 
            colTypes.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colTypes.HeaderText = "Object types";
            colTypes.MinimumWidth = 6;
            colTypes.Name = TypesColumn;
            colTypes.ReadOnly = true;
            // 
            // _btnImport
            // 
            _btnImport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnImport.DialogResult = DialogResult.OK;
            _btnImport.Location = new Point(364, 318);
            _btnImport.Name = "_btnImport";
            _btnImport.Size = new Size(88, 30);
            _btnImport.TabIndex = 2;
            _btnImport.Text = "Import";
            _btnImport.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(460, 318);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new Size(88, 30);
            _btnCancel.TabIndex = 3;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmExternalLayerImport
            // 
            AcceptButton = _btnImport;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _btnCancel;
            ClientSize = new Size(560, 360);
            Controls.Add(_btnCancel);
            Controls.Add(_btnImport);
            Controls.Add(_grid);
            Controls.Add(_lblSummary);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmExternalLayerImport";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Import External Layers";
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            ResumeLayout(false);
        }
    }
}
