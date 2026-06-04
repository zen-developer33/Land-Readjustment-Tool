using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services.Import;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmCadastralImport : Form
    {
        private readonly ProjectSession _session;
        private readonly string _filePath;
        private readonly ICadastralImportService _importService;
        private readonly IProjectScopedFactory _projectScopedFactory;
        private readonly IProjectRasterCrsResolver _projectCrsResolver;

        private CadastralFileInfo? _fileInfo;
        private ProjectRasterCrsContext? _projectCrs;
        private bool _hasOriginalParcelRecords;
        private IReadOnlyList<string> _availableMapSheets = [];

        public event Action<CadastralImportProgress>? ImportProgressChanged;

        public CadastralImportResult? ImportResult { get; private set; }

        public frmCadastralImport(
            ProjectSession session,
            string filePath,
            ICadastralImportService importService,
            IProjectScopedFactory projectScopedFactory,
            IProjectRasterCrsResolver projectCrsResolver)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _projectScopedFactory = projectScopedFactory ?? throw new ArgumentNullException(nameof(projectScopedFactory));
            _projectCrsResolver = projectCrsResolver ?? throw new ArgumentNullException(nameof(projectCrsResolver));

            InitializeComponent();
            Load += frmCadastralImport_Load;
            btnImport.Click += btnImport_Click;
            dgvLayers.CellValueChanged += (_, _) => UpdateImportButtonState();
            dgvMapSheetMappings.CellValueChanged += (_, _) => UpdateImportButtonState();
            dgvAttributeMapSheetMappings.CellValueChanged += (_, _) => UpdateImportButtonState();
            cboSourceMapSheetField.SelectedIndexChanged += (_, _) =>
            {
                PopulateAttributeMapSheetMappingGrid();
                UpdateImportButtonState();
            };
            cboSourceParcelField.SelectedIndexChanged += (_, _) => UpdateImportButtonState();
            dgvLayers.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (dgvLayers.IsCurrentCellDirty)
                    dgvLayers.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvMapSheetMappings.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (dgvMapSheetMappings.IsCurrentCellDirty)
                    dgvMapSheetMappings.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvAttributeMapSheetMappings.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (dgvAttributeMapSheetMappings.IsCurrentCellDirty)
                    dgvAttributeMapSheetMappings.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
        }

        private async void frmCadastralImport_Load(object? sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Inspecting file...";
                _projectCrs = await _projectCrsResolver.ResolveAsync(_session);
                _fileInfo = _importService.Inspect(_filePath);
                _hasOriginalParcelRecords = await _session.GetDbContext()
                    .BaselineParcels
                    .AsNoTracking()
                    .AnyAsync();
                _availableMapSheets = _hasOriginalParcelRecords
                    ? await _session.GetDbContext()
                        .BaselineParcels
                        .AsNoTracking()
                        .Select(parcel => parcel.MapSheetNo)
                        .Where(value => value != "")
                        .Distinct()
                        .OrderBy(value => value)
                        .ToListAsync()
                    : [];

                lblFileValue.Text = Path.GetFileName(_filePath);
                lblFormatValue.Text = _fileInfo.FileFormat;
                lblProjectCrsValue.Text =
                    $"{_projectCrs.CoordinateSystem.Code} - {_projectCrs.CoordinateSystem.Name}";

                ConfigureLayerGrid();
                PopulateLayerGrid();
                ConfigureMapSheetMappingGrid();
                PopulateMapSheetMappingGrid();
                ConfigureAttributeMappingGrid();
                PopulateAttributeFieldSelectors();
                PopulateAttributeMapSheetMappingGrid();
                ApplyAssignmentState();

                if (_fileInfo.RequiresCrsFromUser)
                    await LoadSourceCrsChoicesAsync();

                ApplyCrsState();
                lblStatus.Text = _fileInfo.Layers.Count == 0
                    ? "No importable layers found."
                    : "Ready.";
                UpdateImportButtonState();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Could not inspect file.";
                btnImport.Enabled = false;
                MessageBox.Show(
                    this,
                    $"Could not inspect cadastral map: {ex.Message}",
                    "Cadastral Map Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DialogResult = DialogResult.Cancel;
                BeginInvoke(new Action(Close));
            }
        }

        private void ConfigureLayerGrid()
        {
            dgvLayers.Columns.Clear();
            dgvLayers.EnableHeadersVisualStyles = false;
            dgvLayers.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Include",
                HeaderText = "",
                Width = 42
            });
            dgvLayers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Layer",
                HeaderText = "Source layer",
                ReadOnly = true,
                Width = 210
            });
            dgvLayers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Count",
                HeaderText = "Objects",
                ReadOnly = true,
                Width = 70
            });
            dgvLayers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Types",
                HeaderText = "Object types",
                ReadOnly = true,
                Width = 150
            });
            dgvLayers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CanvasLayer",
                HeaderText = "Target Layer Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            ApplyQuietGridStyle(dgvLayers, enabled: true);
        }

        private void ConfigureMapSheetMappingGrid()
        {
            dgvMapSheetMappings.Columns.Clear();
            dgvMapSheetMappings.EnableHeadersVisualStyles = false;
            dgvMapSheetMappings.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Layer",
                HeaderText = "Drawing source layer",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            DataGridViewComboBoxColumn mapSheetColumn = new()
            {
                Name = "MapSheet",
                HeaderText = "Target MapSheetNo",
                Width = 210,
                FlatStyle = FlatStyle.Flat
            };
            mapSheetColumn.Items.Add(string.Empty);
            foreach (string mapSheet in _availableMapSheets)
                mapSheetColumn.Items.Add(mapSheet);

            dgvMapSheetMappings.Columns.Add(mapSheetColumn);
            ApplyQuietGridStyle(dgvMapSheetMappings, enabled: true);
        }

        private void PopulateLayerGrid()
        {
            dgvLayers.Rows.Clear();
            if (_fileInfo == null)
                return;

            foreach (CadastralLayerInfo layer in _fileInfo.Layers)
            {
                int rowIndex = dgvLayers.Rows.Add(
                    true,
                    layer.Name,
                    layer.ObjectCount,
                    layer.ObjectTypes,
                    layer.Name);
                dgvLayers.Rows[rowIndex].Tag = layer;
            }

            dgvLayers.ClearSelection();
            dgvLayers.CurrentCell = null;
        }

        private void PopulateMapSheetMappingGrid()
        {
            dgvMapSheetMappings.Rows.Clear();
            if (_fileInfo == null)
                return;

            foreach (CadastralLayerInfo layer in _fileInfo.Layers.Where(IsParcelGeometryLayer))
            {
                string defaultMapSheet = _availableMapSheets
                    .FirstOrDefault(item => string.Equals(item, layer.Name, StringComparison.OrdinalIgnoreCase))
                    ?? string.Empty;
                int rowIndex = dgvMapSheetMappings.Rows.Add(layer.Name, defaultMapSheet);
                dgvMapSheetMappings.Rows[rowIndex].Tag = layer;
            }

            dgvMapSheetMappings.ClearSelection();
            dgvMapSheetMappings.CurrentCell = null;
        }

        private void ApplyAssignmentState()
        {
            bool isShapefile = IsShapefileImport();
            bool hasAttributeFields = _fileInfo?.AttributeFields.Count > 0;

            chkAutoAssign.Enabled = _hasOriginalParcelRecords;
            chkAutoAssign.Checked = false;
            chkAutoAssign.Text = isShapefile
                ? "Auto assign parcel records using mapped source attributes"
                : "Auto assign parcel number from DXF text / SHP attributes when available";

            if (isShapefile)
            {
                lblMapSheetMappingCaption.Text = "Attribute";
                lblMapSheetMappingCaption.Enabled = hasAttributeFields;
                dgvMapSheetMappings.Visible = false;
                attributeMappingLayout.Visible = true;
                ApplyAttributeMappingGridState(hasAttributeFields);
                lblAssignmentNote.Text = _hasOriginalParcelRecords
                    ? "Select the source map-sheet field, map each source value to an Original Parcel Record MapSheetNo, then select the source parcel field."
                    : "Map source attribute fields to keep parcel identifiers in imported object metadata. No Original Parcel Records were found for auto assignment.";
                return;
            }

            lblMapSheetMappingCaption.Text = "Map sheet mapping";
            lblMapSheetMappingCaption.Enabled = _hasOriginalParcelRecords;
            dgvMapSheetMappings.Visible = true;
            attributeMappingLayout.Visible = false;
            ApplyMapSheetMappingGridState(_hasOriginalParcelRecords);
            lblAssignmentNote.Text = _hasOriginalParcelRecords
                ? "Map drawing layers to existing Original Parcel Record MapSheetNo values before using auto assignment."
                : "No Original Parcel Records found. Import will store raw layers only; map-sheet assignment is disabled.";
        }

        private static void ApplyQuietGridStyle(DataGridView grid, bool enabled)
        {
            Color backColor = enabled ? SystemColors.Window : SystemColors.Control;
            Color foreColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            Color headerBackColor = enabled ? Color.FromArgb(248, 250, 252) : SystemColors.Control;
            Color gridColor = Color.FromArgb(214, 219, 226);

            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = backColor;
            grid.GridColor = gridColor;
            grid.DefaultCellStyle.BackColor = backColor;
            grid.DefaultCellStyle.ForeColor = foreColor;
            grid.DefaultCellStyle.SelectionBackColor = backColor;
            grid.DefaultCellStyle.SelectionForeColor = foreColor;
            grid.AlternatingRowsDefaultCellStyle.BackColor = enabled
                ? Color.FromArgb(252, 253, 255)
                : backColor;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = foreColor;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = grid.AlternatingRowsDefaultCellStyle.BackColor;
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = foreColor;
            grid.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = foreColor;
            grid.RowHeadersDefaultCellStyle.BackColor = backColor;
            grid.RowHeadersDefaultCellStyle.ForeColor = foreColor;
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = backColor;
            grid.RowHeadersDefaultCellStyle.SelectionForeColor = foreColor;
        }

        private void ApplyMapSheetMappingGridState(bool enabled)
        {
            dgvMapSheetMappings.Enabled = enabled;
            dgvMapSheetMappings.ReadOnly = !enabled;
            dgvMapSheetMappings.TabStop = enabled;

            ApplyQuietGridStyle(dgvMapSheetMappings, enabled);

            foreach (DataGridViewColumn column in dgvMapSheetMappings.Columns)
                column.ReadOnly = !enabled || column.Name == "Layer";

            if (!enabled)
            {
                dgvMapSheetMappings.ClearSelection();
                dgvMapSheetMappings.CurrentCell = null;
            }
        }

        private void ConfigureAttributeMappingGrid()
        {
            dgvAttributeMapSheetMappings.Columns.Clear();
            dgvAttributeMapSheetMappings.EnableHeadersVisualStyles = false;
            dgvAttributeMapSheetMappings.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SourceMapSheet",
                HeaderText = "Source map-sheet value",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            DataGridViewComboBoxColumn targetMapSheetColumn = new()
            {
                Name = "TargetMapSheet",
                HeaderText = "Target MapSheetNo",
                Width = 260,
                FlatStyle = FlatStyle.Flat
            };

            dgvAttributeMapSheetMappings.Columns.Add(targetMapSheetColumn);
            ApplyQuietGridStyle(dgvAttributeMapSheetMappings, enabled: true);
        }

        private void PopulateAttributeFieldSelectors()
        {
            if (_fileInfo == null)
                return;

            cboSourceMapSheetField.Items.Clear();
            cboSourceParcelField.Items.Clear();
            cboSourceMapSheetField.Items.Add(string.Empty);
            cboSourceParcelField.Items.Add(string.Empty);
            foreach (string field in _fileInfo.AttributeFields.OrderBy(item => item))
            {
                cboSourceMapSheetField.Items.Add(field);
                cboSourceParcelField.Items.Add(field);
            }

            SelectComboValue(
                cboSourceMapSheetField,
                FindBestAttributeField(
                    _fileInfo.AttributeFields,
                    ["mapsheet", "map_sheet", "map sheet", "mapsheetno", "map_sheet_no", "sheet", "sheetno", "sheet_no"]));

            SelectComboValue(
                cboSourceParcelField,
                FindBestAttributeField(
                    _fileInfo.AttributeFields,
                    ["parcel", "parcelno", "parcel_no", "parcel number", "parcelnumber", "kitta", "kitta_no", "kittano", "plot", "plotno", "plot_no"]));
        }

        private void PopulateAttributeMapSheetMappingGrid()
        {
            dgvAttributeMapSheetMappings.Rows.Clear();
            if (_fileInfo == null)
                return;

            string? mapSheetField = GetSelectedComboText(cboSourceMapSheetField);
            List<string> sourceValues = GetAttributeUniqueValues(mapSheetField);
            DataGridViewComboBoxColumn? targetMapSheetColumn =
                dgvAttributeMapSheetMappings.Columns["TargetMapSheet"] as DataGridViewComboBoxColumn;
            if (targetMapSheetColumn != null)
            {
                targetMapSheetColumn.Items.Clear();
                targetMapSheetColumn.Items.Add(string.Empty);
                foreach (string targetMapSheet in _availableMapSheets)
                    targetMapSheetColumn.Items.Add(targetMapSheet);
            }

            foreach (string sourceValue in sourceValues)
            {
                string defaultTargetMapSheet = _availableMapSheets
                    .FirstOrDefault(value => IsLikelySameMapSheet(sourceValue, value))
                    ?? string.Empty;
                int rowIndex = dgvAttributeMapSheetMappings.Rows.Add(sourceValue, defaultTargetMapSheet);
                dgvAttributeMapSheetMappings.Rows[rowIndex].Tag = sourceValue;
            }

            dgvAttributeMapSheetMappings.ClearSelection();
            dgvAttributeMapSheetMappings.CurrentCell = null;
        }

        private void ApplyAttributeMappingGridState(bool enabled)
        {
            attributeMappingLayout.Enabled = enabled;
            dgvAttributeMapSheetMappings.Enabled = enabled;
            dgvAttributeMapSheetMappings.ReadOnly = !enabled;
            dgvAttributeMapSheetMappings.TabStop = enabled;
            cboSourceMapSheetField.Enabled = enabled;
            cboSourceParcelField.Enabled = enabled;

            ApplyQuietGridStyle(dgvAttributeMapSheetMappings, enabled);

            foreach (DataGridViewColumn column in dgvAttributeMapSheetMappings.Columns)
                column.ReadOnly = !enabled || column.Name == "SourceMapSheet";

            if (!enabled)
            {
                dgvAttributeMapSheetMappings.ClearSelection();
                dgvAttributeMapSheetMappings.CurrentCell = null;
            }
        }

        private static bool IsParcelGeometryLayer(CadastralLayerInfo layer)
        {
            return layer.PolygonCount > 0 || layer.PolylineCount > 0;
        }

        private async Task LoadSourceCrsChoicesAsync()
        {
            cmbSourceCrs.Items.Clear();
            if (_projectCrs != null)
            {
                cmbSourceCrs.Items.Add(new CrsChoice(
                    $"Project CRS ({_projectCrs.CoordinateSystem.Code})",
                    _projectCrs.TargetSrsDefinition));
            }

            cmbSourceCrs.Items.Add(new CrsChoice("WGS 84 / UTM 44N (EPSG:32644)", "EPSG:32644"));
            cmbSourceCrs.Items.Add(new CrsChoice("WGS 84 / UTM 45N (EPSG:32645)", "EPSG:32645"));
            cmbSourceCrs.Items.Add(new CrsChoice("WGS 84 (EPSG:4326)", "EPSG:4326"));

            var crsRepository = _projectScopedFactory.CreateCoordinateSystemRepository(_session);
            IReadOnlyList<CoordinateSystem> activeCrs =
                await crsRepository.GetAllActiveAsync();

            foreach (CoordinateSystem crs in activeCrs.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Name))
            {
                if (cmbSourceCrs.Items
                    .OfType<CrsChoice>()
                    .Any(item => item.Label.Contains(crs.Code, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                string? definition = await TryBuildCrsDefinitionAsync(crs);
                if (!string.IsNullOrWhiteSpace(definition))
                {
                    cmbSourceCrs.Items.Add(new CrsChoice(
                        $"{crs.Code} - {crs.Name}",
                        definition));
                }
            }

            if (cmbSourceCrs.Items.Count > 0)
                cmbSourceCrs.SelectedIndex = 0;
        }

        private void ApplyCrsState()
        {
            if (_fileInfo == null)
                return;

            if (_fileInfo.RequiresCrsFromUser)
            {
                cmbSourceCrs.Visible = true;
                lblSourceCrsValue.Visible = false;
                return;
            }

            cmbSourceCrs.Visible = false;
            lblSourceCrsValue.Visible = true;
            lblSourceCrsValue.Text = _fileInfo.DetectedCrsCode ?? "Unknown";
        }

        private async Task<string?> TryBuildCrsDefinitionAsync(CoordinateSystem crs)
        {
            if (crs.EpsgCode.HasValue)
                return $"EPSG:{crs.EpsgCode.Value}";

            try
            {
                var crsRepository = _projectScopedFactory.CreateCoordinateSystemRepository(_session);
                CoordinateSystem? withParameters =
                    await crsRepository.GetWithParametersAsync(crs.Id);

                if (withParameters?.ProjectionParameters == null)
                    return null;

                return ProjectCrsWktBuilder.BuildTargetSrsDefinition(
                    withParameters,
                    _projectCrs?.DatumTransformation);
            }
            catch
            {
                return null;
            }
        }

        private void UpdateImportButtonState()
        {
            btnImport.Enabled = _fileInfo != null && GetLayerOptions().Any(option => option.Include);
        }

        private async void btnImport_Click(object? sender, EventArgs e)
        {
            if (_fileInfo == null)
                return;

            List<CadastralLayerImportOption> layerOptions = GetLayerOptions();
            if (!layerOptions.Any(option => option.Include))
            {
                MessageBox.Show(
                    this,
                    "Select at least one source layer to import.",
                    "Cadastral Map Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string sourceCrs = GetSourceCrsDefinition();
            if (string.IsNullOrWhiteSpace(sourceCrs))
            {
                MessageBox.Show(
                    this,
                    "Select the source CRS before importing.",
                    "Cadastral Map Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult duplicatePrompt = MessageBox.Show(
                this,
                "If duplicate objects with the same geometry are found, only the first unique shape will be imported and the rest will be skipped.\n\nContinue importing?",
                "Cadastral Map Import",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            if (duplicatePrompt != DialogResult.OK)
                return;

            string? shapefileParcelField = null;
            string? shapefileMapSheetField = null;
            Dictionary<string, string> attributeMapSheetMappings = new(StringComparer.OrdinalIgnoreCase);
            if (IsShapefileImport())
            {
                shapefileParcelField = GetSelectedComboText(cboSourceParcelField);
                shapefileMapSheetField = GetSelectedComboText(cboSourceMapSheetField);
                attributeMapSheetMappings = GetAttributeMapSheetValueMappings();

                if (chkAutoAssign.Checked &&
                    chkAutoAssign.Enabled &&
                    (string.IsNullOrWhiteSpace(shapefileParcelField) ||
                     string.IsNullOrWhiteSpace(shapefileMapSheetField) ||
                     attributeMapSheetMappings.Count == 0))
                {
                    MessageBox.Show(
                        this,
                        "Select the source map-sheet field, map at least one source map-sheet value to a target MapSheetNo, and select the source parcel field before auto assigning parcel records.",
                        "Cadastral Map Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                btnImport.Enabled = false;
                btnCancel.Enabled = false;
                ReportImportProgress(new CadastralImportProgress(
                    0,
                    "Starting cadastral map import..."));

                CadastralImportOptions importOptions = new(
                        layerOptions,
                        sourceCrs,
                        chkAutoAssign.Checked && chkAutoAssign.Enabled,
                        shapefileParcelField,
                        shapefileMapSheetField,
                        attributeMapSheetMappings,
                        SkipDuplicateGeometries: true);
                Progress<CadastralImportProgress> progress = new(ReportImportProgress);

                ImportResult = await Task.Run(async () =>
                    await _importService.ImportAsync(
                        _session,
                        _filePath,
                        importOptions,
                        progress));

                if (!ImportResult.Success)
                {
                    MessageBox.Show(
                        this,
                        ImportResult.ErrorMessage ?? "Cadastral map import failed.",
                        "Cadastral Map Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    btnImport.Enabled = true;
                    btnCancel.Enabled = true;
                    lblStatus.Text = "Import failed.";
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Cadastral map import failed: {ex.Message}",
                    "Cadastral Map Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                btnImport.Enabled = true;
                btnCancel.Enabled = true;
                lblStatus.Text = "Import failed.";
            }
        }

        private void ReportImportProgress(CadastralImportProgress progress)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ReportImportProgress(progress)));
                return;
            }

            lblStatus.Text = progress.Status;
            ImportProgressChanged?.Invoke(progress);
        }

        private List<CadastralLayerImportOption> GetLayerOptions()
        {
            List<CadastralLayerImportOption> options = [];
            foreach (DataGridViewRow row in dgvLayers.Rows)
            {
                if (row.Tag is not CadastralLayerInfo layer)
                    continue;

                bool include = row.Cells["Include"].Value is bool value && value;
                string? mapSheet = _hasOriginalParcelRecords
                    && !IsShapefileImport()
                    ? GetMappedMapSheet(layer.Name)
                    : null;
                string canvasLayer = Convert.ToString(row.Cells["CanvasLayer"].Value)?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(canvasLayer))
                    canvasLayer = layer.Name;

                options.Add(new CadastralLayerImportOption(
                    layer.Name,
                    include,
                    canvasLayer,
                    mapSheet));
            }

            return options;
        }

        private string? GetMappedMapSheet(string sourceLayer)
        {
            foreach (DataGridViewRow row in dgvMapSheetMappings.Rows)
            {
                if (row.Tag is not CadastralLayerInfo layer ||
                    !string.Equals(layer.Name, sourceLayer, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string? value = Convert.ToString(row.Cells["MapSheet"].Value)?.Trim();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            return null;
        }

        private Dictionary<string, string> GetAttributeMapSheetValueMappings()
        {
            Dictionary<string, string> mappings = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in dgvAttributeMapSheetMappings.Rows)
            {
                string? sourceValue = row.Tag as string;
                string? targetMapSheet = Convert.ToString(row.Cells["TargetMapSheet"].Value)?.Trim();
                if (string.IsNullOrWhiteSpace(targetMapSheet) ||
                    string.IsNullOrWhiteSpace(sourceValue))
                    continue;

                mappings[sourceValue] = targetMapSheet;
                string normalizedSource = NormalizeMapSheetValue(sourceValue);
                if (!mappings.ContainsKey(normalizedSource))
                    mappings[normalizedSource] = targetMapSheet;
            }

            return mappings;
        }

        private List<string> GetAttributeUniqueValues(string? fieldName)
        {
            if (_fileInfo == null || string.IsNullOrWhiteSpace(fieldName))
                return [];

            return _fileInfo.AttributeUniqueValues.TryGetValue(fieldName, out IReadOnlyList<string>? values)
                ? values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList()
                : [];
        }

        private static string? GetSelectedComboText(ComboBox comboBox)
        {
            string? value = Convert.ToString(comboBox.SelectedItem)?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static void SelectComboValue(ComboBox comboBox, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
                return;
            }

            for (int index = 0; index < comboBox.Items.Count; index++)
            {
                if (string.Equals(
                        Convert.ToString(comboBox.Items[index]),
                        value,
                        StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedIndex = index;
                    return;
                }
            }

            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        private static bool IsLikelySameMapSheet(string sourceValue, string targetMapSheet)
        {
            return string.Equals(
                NormalizeMapSheetValue(sourceValue),
                NormalizeMapSheetValue(targetMapSheet),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeMapSheetValue(string value)
        {
            return new string(value
                .Where(ch => !char.IsWhiteSpace(ch))
                .Select(char.ToUpperInvariant)
                .ToArray());
        }

        private string GetSourceCrsDefinition()
        {
            if (_fileInfo == null)
                return string.Empty;

            if (!_fileInfo.RequiresCrsFromUser)
                return _fileInfo.DetectedCrsCode ?? string.Empty;

            return cmbSourceCrs.SelectedItem is CrsChoice crs
                ? crs.Definition
                : string.Empty;
        }

        private bool IsShapefileImport()
        {
            return string.Equals(_fileInfo?.FileFormat, "SHP", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(_fileInfo?.FileFormat, "KML", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(Path.GetExtension(_filePath), ".kml", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(Path.GetExtension(_filePath), ".kmz", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(Path.GetExtension(_filePath), ".shp", StringComparison.OrdinalIgnoreCase);
        }

        private static string FindBestAttributeField(
            IReadOnlyList<string> fields,
            IReadOnlyList<string> candidates)
        {
            foreach (string candidate in candidates)
            {
                string normalizedCandidate = NormalizeFieldName(candidate);
                string? exactMatch = fields.FirstOrDefault(field =>
                    string.Equals(NormalizeFieldName(field), normalizedCandidate, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(exactMatch))
                    return exactMatch;
            }

            foreach (string candidate in candidates)
            {
                string normalizedCandidate = NormalizeFieldName(candidate);
                string? containsMatch = fields.FirstOrDefault(field =>
                    NormalizeFieldName(field).Contains(normalizedCandidate, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(containsMatch))
                    return containsMatch;
            }

            return string.Empty;
        }

        private static string NormalizeFieldName(string value)
        {
            return new string(value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }

        private sealed record CrsChoice(string Label, string Definition)
        {
            public override string ToString() => Label;
        }
    }
}
