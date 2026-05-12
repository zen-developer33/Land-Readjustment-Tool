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
                HeaderText = "Original record MapSheetNo",
                Width = 210,
                FlatStyle = FlatStyle.Flat
            };
            mapSheetColumn.Items.Add(string.Empty);
            foreach (string mapSheet in _availableMapSheets)
                mapSheetColumn.Items.Add(mapSheet);

            dgvMapSheetMappings.Columns.Add(mapSheetColumn);
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
            chkAutoAssign.Enabled = _hasOriginalParcelRecords;
            chkAutoAssign.Checked = _hasOriginalParcelRecords;
            lblAssignmentNote.Text = _hasOriginalParcelRecords
                ? "Map drawing layers to existing Original Parcel Record MapSheetNo values before using auto assignment."
                : "No Original Parcel Records found. Import will store raw layers only; map-sheet assignment is disabled.";

            lblMapSheetMappingCaption.Enabled = _hasOriginalParcelRecords;
            ApplyMapSheetMappingGridState(_hasOriginalParcelRecords);
        }

        private void ApplyMapSheetMappingGridState(bool enabled)
        {
            dgvMapSheetMappings.Enabled = enabled;
            dgvMapSheetMappings.ReadOnly = !enabled;
            dgvMapSheetMappings.TabStop = enabled;

            Color backColor = enabled ? SystemColors.Window : SystemColors.Control;
            Color foreColor = enabled ? SystemColors.ControlText : SystemColors.GrayText;
            Color selectionBackColor = enabled ? SystemColors.Highlight : SystemColors.Control;
            Color selectionForeColor = enabled ? SystemColors.HighlightText : SystemColors.GrayText;

            dgvMapSheetMappings.BackgroundColor = backColor;
            dgvMapSheetMappings.GridColor = enabled ? SystemColors.ControlDark : SystemColors.ControlDark;
            dgvMapSheetMappings.DefaultCellStyle.BackColor = backColor;
            dgvMapSheetMappings.DefaultCellStyle.ForeColor = foreColor;
            dgvMapSheetMappings.DefaultCellStyle.SelectionBackColor = selectionBackColor;
            dgvMapSheetMappings.DefaultCellStyle.SelectionForeColor = selectionForeColor;
            dgvMapSheetMappings.ColumnHeadersDefaultCellStyle.BackColor = enabled
                ? SystemColors.Window
                : SystemColors.Control;
            dgvMapSheetMappings.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
            dgvMapSheetMappings.RowHeadersDefaultCellStyle.BackColor = backColor;
            dgvMapSheetMappings.RowHeadersDefaultCellStyle.ForeColor = foreColor;

            foreach (DataGridViewColumn column in dgvMapSheetMappings.Columns)
                column.ReadOnly = !enabled || column.Name == "Layer";

            if (!enabled)
            {
                dgvMapSheetMappings.ClearSelection();
                dgvMapSheetMappings.CurrentCell = null;
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

            try
            {
                btnImport.Enabled = false;
                btnCancel.Enabled = false;
                lblStatus.Text = "Transforming and saving cadastral map objects...";

                ImportResult = await _importService.ImportAsync(
                    _session,
                    _filePath,
                    new CadastralImportOptions(
                        layerOptions,
                        sourceCrs,
                        chkAutoAssign.Checked && chkAutoAssign.Enabled,
                        null,
                        null));

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

        private List<CadastralLayerImportOption> GetLayerOptions()
        {
            List<CadastralLayerImportOption> options = [];
            foreach (DataGridViewRow row in dgvLayers.Rows)
            {
                if (row.Tag is not CadastralLayerInfo layer)
                    continue;

                bool include = row.Cells["Include"].Value is bool value && value;
                string? mapSheet = _hasOriginalParcelRecords
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

        private sealed record CrsChoice(string Label, string Definition)
        {
            public override string ToString() => Label;
        }
    }
}
