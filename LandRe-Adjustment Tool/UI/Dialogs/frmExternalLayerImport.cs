using Land_Readjustment_Tool.Core.Models.Import;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmExternalLayerImport : Form
    {
        private readonly ExternalLayerFileInfo _fileInfo;
        private readonly string _sourceCrsDefinition;
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

            InitializeComponent();
            _lblSummary.Text =
                $"{Path.GetFileName(_fileInfo.FilePath)}  |  {_fileInfo.FileFormat}  |  {_fileInfo.Layers.Count} layer(s)";
            WireEvents();
            PopulateGrid();
            UpdateImportState();
        }

        public ExternalLayerImportOptions ImportOptions =>
            new(GetLayerOptions(), _sourceCrsDefinition);

        private void WireEvents()
        {
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
