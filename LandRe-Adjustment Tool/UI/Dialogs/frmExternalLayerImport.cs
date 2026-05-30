using Land_Readjustment_Tool.Core.Models.Import;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed class frmExternalLayerImport : Form
    {
        private readonly ExternalLayerFileInfo _fileInfo;
        private readonly string _sourceCrsDefinition;
        private readonly DataGridView _grid = new();
        private readonly Button _btnImport = new();
        private readonly Button _btnCancel = new();
        private readonly Label _lblSummary = new();

        public frmExternalLayerImport(
            ExternalLayerFileInfo fileInfo,
            string sourceCrsDefinition)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _sourceCrsDefinition = sourceCrsDefinition ?? string.Empty;

            InitializeForm();
            PopulateGrid();
            UpdateImportState();
        }

        public ExternalLayerImportOptions ImportOptions =>
            new(GetLayerOptions(), _sourceCrsDefinition);

        private void InitializeForm()
        {
            Text = "Import External Layers";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(560, 360);

            _lblSummary.AutoSize = false;
            _lblSummary.Location = new Point(12, 10);
            _lblSummary.Size = new Size(536, 42);
            _lblSummary.Text =
                $"{Path.GetFileName(_fileInfo.FilePath)}  |  {_fileInfo.FileFormat}  |  {_fileInfo.Layers.Count} layer(s)";
            _lblSummary.TextAlign = ContentAlignment.MiddleLeft;

            _grid.Location = new Point(12, 58);
            _grid.Size = new Size(536, 246);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.BorderStyle = BorderStyle.FixedSingle;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _grid.ColumnHeadersHeight = 26;
            _grid.RowTemplate.Height = 24;
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Include",
                HeaderText = "",
                Width = 38
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Layer",
                HeaderText = "Layer",
                ReadOnly = true,
                Width = 190
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Objects",
                HeaderText = "Objects",
                ReadOnly = true,
                Width = 70
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Types",
                HeaderText = "Object types",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            _grid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_grid.IsCurrentCellDirty)
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _grid.CellValueChanged += (_, _) => UpdateImportState();

            _btnImport.Text = "Import";
            _btnImport.Size = new Size(88, 30);
            _btnImport.Location = new Point(ClientSize.Width - 196, 318);
            _btnImport.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _btnImport.DialogResult = DialogResult.OK;

            _btnCancel.Text = "Cancel";
            _btnCancel.Size = new Size(88, 30);
            _btnCancel.Location = new Point(ClientSize.Width - 100, 318);
            _btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _btnCancel.DialogResult = DialogResult.Cancel;

            Controls.Add(_lblSummary);
            Controls.Add(_grid);
            Controls.Add(_btnImport);
            Controls.Add(_btnCancel);
            AcceptButton = _btnImport;
            CancelButton = _btnCancel;
        }

        private void PopulateGrid()
        {
            foreach (ExternalLayerInfo layer in _fileInfo.Layers)
            {
                int rowIndex = _grid.Rows.Add(
                    true,
                    layer.Name,
                    layer.ObjectCount,
                    layer.ObjectTypes);
                _grid.Rows[rowIndex].Tag = layer;
            }

            _grid.ClearSelection();
            _grid.CurrentCell = null;
        }

        private List<ExternalLayerImportOption> GetLayerOptions()
        {
            List<ExternalLayerImportOption> options = [];
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag is not ExternalLayerInfo layer)
                    continue;

                bool include = row.Cells["Include"].Value is bool value && value;
                options.Add(new ExternalLayerImportOption(layer.Name, include));
            }

            return options;
        }

        private void UpdateImportState()
        {
            _btnImport.Enabled = GetLayerOptions().Any(option => option.Include);
        }
    }
}
