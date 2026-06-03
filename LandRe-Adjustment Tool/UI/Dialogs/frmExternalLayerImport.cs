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
        private CheckState _includeHeaderState = CheckState.Checked;

        private const string IncludeColumn = "Include";
        private const string LayerColumn = "Layer";
        private const string ObjectsColumn = "Objects";
        private const string TypesColumn = "Types";

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
            _grid.EnableHeadersVisualStyles = false;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _grid.ColumnHeadersHeight = 26;
            _grid.RowTemplate.Height = 24;
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = IncludeColumn,
                HeaderText = "",
                Width = 38
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = LayerColumn,
                HeaderText = "Layer",
                ReadOnly = true,
                Width = 190
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ObjectsColumn,
                HeaderText = "Objects",
                ReadOnly = true,
                Width = 70
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = TypesColumn,
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
            _grid.CellMouseClick += (_, args) =>
            {
                int includeColumnIndex = _grid.Columns[IncludeColumn]?.Index ?? -1;
                if (args.RowIndex == -1 && args.ColumnIndex == includeColumnIndex)
                    SetAllIncluded(_includeHeaderState != CheckState.Checked);
            };
            _grid.CellPainting += Grid_CellPainting;

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

                bool include = row.Cells[IncludeColumn].Value is bool value && value;
                options.Add(new ExternalLayerImportOption(layer.Name, include));
            }

            return options;
        }

        private void SetAllIncluded(bool included)
        {
            foreach (DataGridViewRow row in _grid.Rows)
                row.Cells[IncludeColumn].Value = included;

            UpdateImportState();
        }

        private void UpdateImportState()
        {
            _btnImport.Enabled = GetLayerOptions().Any(option => option.Include);
            UpdateHeaderCheckState();
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs args)
        {
            int includeColumnIndex = _grid.Columns[IncludeColumn]?.Index ?? -1;
            if (args.RowIndex != -1 || args.ColumnIndex != includeColumnIndex || args.Graphics == null)
                return;

            args.Paint(args.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            System.Windows.Forms.VisualStyles.CheckBoxState state = _includeHeaderState switch
            {
                CheckState.Checked => System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal,
                CheckState.Indeterminate => System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal,
                _ => System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal
            };

            Size checkSize = CheckBoxRenderer.GetGlyphSize(args.Graphics, state);
            Point location = new(
                args.CellBounds.Left + (args.CellBounds.Width - checkSize.Width) / 2,
                args.CellBounds.Top + (args.CellBounds.Height - checkSize.Height) / 2);

            CheckBoxRenderer.DrawCheckBox(args.Graphics, location, state);
            args.Handled = true;
        }

        private void UpdateHeaderCheckState()
        {
            int included = _grid.Rows
                .Cast<DataGridViewRow>()
                .Count(row => row.Cells[IncludeColumn].Value is bool value && value);

            _includeHeaderState = included == 0
                ? CheckState.Unchecked
                : included == _grid.Rows.Count
                    ? CheckState.Checked
                    : CheckState.Indeterminate;

            DataGridViewColumn? includeColumn = _grid.Columns[IncludeColumn];
            if (includeColumn != null)
                _grid.InvalidateCell(includeColumn.HeaderCell);
        }
    }
}
