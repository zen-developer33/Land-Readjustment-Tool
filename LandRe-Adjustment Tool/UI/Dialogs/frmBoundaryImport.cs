using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Import;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmBoundaryImport : Form
    {
        private readonly ProjectSession _session;
        private readonly string _filePath;
        private readonly IBoundaryReaderFactory _readerFactory;
        private readonly IBoundaryImportService _importService;
        private readonly IProjectScopedFactory _projectScopedFactory;
        private readonly IProjectRasterCrsResolver _projectCrsResolver;

        private VectorFileInfo? _fileInfo;
        private ProjectRasterCrsContext? _projectCrs;

        public BoundaryImportResult? ImportResult { get; private set; }

        public frmBoundaryImport(
            ProjectSession session,
            string filePath,
            IBoundaryReaderFactory readerFactory,
            IBoundaryImportService importService,
            IProjectScopedFactory projectScopedFactory,
            IProjectRasterCrsResolver projectCrsResolver)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _readerFactory = readerFactory ?? throw new ArgumentNullException(nameof(readerFactory));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _projectScopedFactory = projectScopedFactory ?? throw new ArgumentNullException(nameof(projectScopedFactory));
            _projectCrsResolver = projectCrsResolver ?? throw new ArgumentNullException(nameof(projectCrsResolver));

            InitializeComponent();
            Load += frmBoundaryImport_Load;
            btnImport.Click += import_Click;
            lstLayers.SelectedIndexChanged += (_, _) => UpdateImportButtonState();
        }

        private async void frmBoundaryImport_Load(object? sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Inspecting file...";
                _projectCrs = await _projectCrsResolver.ResolveAsync(_session);
                _fileInfo = _readerFactory.GetReader(_filePath).Inspect(_filePath);

                lblFileValue.Text = Path.GetFileName(_filePath);
                lblFormatValue.Text = _fileInfo.FileFormat;
                lblProjectCrsValue.Text =
                    $"{_projectCrs.CoordinateSystem.Code} - {_projectCrs.CoordinateSystem.Name}";

                lstLayers.Items.Clear();
                foreach (VectorLayerInfo layer in _fileInfo.Layers)
                    lstLayers.Items.Add(new LayerChoice(layer));

                if (lstLayers.Items.Count > 0)
                    lstLayers.SelectedIndex = 0;

                if (_fileInfo.RequiresCrsFromUser)
                    await LoadSourceCrsChoicesAsync();

                ApplyCrsState();

                lblStatus.Text = _fileInfo.Layers.Count == 0
                    ? GetEmptyLayerMessage(_fileInfo.FileFormat)
                    : "Ready.";
                UpdateImportButtonState();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Could not inspect file.";
                btnImport.Enabled = false;
                MessageBox.Show(
                    this,
                    $"Could not inspect boundary file: {ex.Message}",
                    "Boundary Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DialogResult = DialogResult.Cancel;
                BeginInvoke(new Action(Close));
            }
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

            cmbSourceCrs.Items.Add(new CrsChoice("WGS 84 (EPSG:4326)", "EPSG:4326"));
            cmbSourceCrs.Items.Add(new CrsChoice("WGS 84 / UTM 44N (EPSG:32644)", "EPSG:32644"));
            cmbSourceCrs.Items.Add(new CrsChoice("WGS 84 / UTM 45N (EPSG:32645)", "EPSG:32645"));

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
                if (string.IsNullOrWhiteSpace(definition))
                    continue;

                cmbSourceCrs.Items.Add(new CrsChoice(
                    $"{crs.Code} - {crs.Name}",
                    definition));
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

        private void UpdateImportButtonState()
        {
            btnImport.Enabled =
                _fileInfo != null &&
                lstLayers.SelectedItem is LayerChoice selectedLayer &&
                selectedLayer.Layer.HasClosedPolygons;
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

        private async void import_Click(object? sender, EventArgs e)
        {
            if (_fileInfo == null)
                return;

            if (lstLayers.SelectedItem is not LayerChoice selectedLayer)
            {
                MessageBox.Show(
                    this,
                    "Select a layer or group to import.",
                    "Boundary Import",
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
                    "Boundary Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnImport.Enabled = false;
                btnCancel.Enabled = false;
                lblStatus.Text = "Transforming and saving boundary...";

                ImportResult = await _importService.ImportProjectBoundaryAsync(
                    _session,
                    _filePath,
                    new BoundaryImportOptions(selectedLayer.Layer.Name, sourceCrs));

                if (!ImportResult.Success)
                {
                    MessageBox.Show(
                        this,
                        ImportResult.ErrorMessage ?? "Boundary import failed.",
                        "Boundary Import",
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
                    $"Boundary import failed: {ex.Message}",
                    "Boundary Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                btnImport.Enabled = true;
                btnCancel.Enabled = true;
                lblStatus.Text = "Import failed.";
            }
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

        private sealed class LayerChoice
        {
            public LayerChoice(VectorLayerInfo layer)
            {
                Layer = layer;
            }

            public VectorLayerInfo Layer { get; }

            public override string ToString()
            {
                return Layer.FeatureCount == 1
                    ? $"{Layer.Name} (1 boundary)"
                    : $"{Layer.Name} ({Layer.FeatureCount} boundaries)";
            }
        }

        private static string GetEmptyLayerMessage(string fileFormat)
        {
            return string.Equals(fileFormat, "DXF", StringComparison.OrdinalIgnoreCase)
                ? "No closed polyline layers found."
                : "No polygon layers found.";
        }

        private sealed record CrsChoice(string Label, string Definition)
        {
            public override string ToString() => Label;
        }
    }
}
