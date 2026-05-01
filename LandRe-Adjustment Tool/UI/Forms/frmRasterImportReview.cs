using Land_Readjustment_Tool.Services.Raster;
using OSGeo.OSR;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Lets the user review raster metadata and define a source CRS before importing.
    /// </summary>
    public partial class frmRasterImportReview : Form
    {
        private const string DefaultMissingSourceCrs = "EPSG:4326";
        private const string Wgs84Option = "WGS 1984 (EPSG:4326)";
        private const string WebMercatorOption = "Web Mercator (EPSG:3857)";
        private const string Utm44NOption = "WGS 1984 / UTM Zone 44N (EPSG:32644)";
        private const string Utm45NOption = "WGS 1984 / UTM Zone 45N (EPSG:32645)";
        private const string CustomEpsgOption = "Custom EPSG code";
        private const string CustomWktOption = "Custom WKT";
        private const string CustomEpsgPlaceholder = "Example: EPSG:4326 or 4326";
        private const string CustomWktPlaceholder = "Example: GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",...]]";
        private readonly RasterLayerImportPreview _preview;
        private string _projectCrsOption = "Project CRS";

        /// <summary>
        /// Creates the raster import review form.
        /// </summary>
        public frmRasterImportReview(RasterLayerImportPreview preview)
        {
            _preview = preview ?? throw new ArgumentNullException(nameof(preview));
            InitializeComponent();
            LoadPreview();
            btnImport.Click += btnImport_Click;
            rdoDetectedCrs.CheckedChanged += ProjectionChoiceChanged;
            rdoDefineSourceCrs.CheckedChanged += ProjectionChoiceChanged;
            cmbSourceCrs.SelectedIndexChanged += ProjectionChoiceChanged;
        }

        /// <summary>
        /// Gets the layer name confirmed by the user.
        /// </summary>
        public string LayerName => txtLayerName.Text.Trim();

        /// <summary>
        /// Gets the optional source CRS definition to use when the raster did not store one.
        /// </summary>
        public string? SourceSrsDefinitionOverride
        {
            get
            {
                if (!rdoDefineSourceCrs.Checked)
                    return null;

                return GetSelectedSourceCrsDefinition();
            }
        }

        /// <summary>
        /// Loads metadata, projection choices, and preview image into the form.
        /// </summary>
        private void LoadPreview()
        {
            RasterDatasetMetadata metadata = _preview.Metadata;
            txtLayerName.Text = _preview.SuggestedLayerName;
            lblSourceValue.Text = metadata.SourcePath;
            lblSizeValue.Text = $"{metadata.Width} x {metadata.Height} pixels, {metadata.BandCount} band(s)";
            lblDriverValue.Text = $"{metadata.DriverShortName} - {metadata.DriverLongName}";
            lblGeoValue.Text = metadata.HasGeoreferencing ? "Yes" : "No";
            lblRasterCrsValue.Text = metadata.HasProjection
                ? metadata.CoordinateSystemName
                : "Not stored in raster";
            lblProjectCrsValue.Text =
                $"{_preview.ProjectCrs.CoordinateSystem.Code} - {_preview.ProjectCrs.CoordinateSystem.Name}";
            _projectCrsOption =
                $"Project CRS ({_preview.ProjectCrs.CoordinateSystem.Code} - {_preview.ProjectCrs.CoordinateSystem.Name})";

            rdoDetectedCrs.Enabled = metadata.HasProjection;
            rdoDetectedCrs.Checked = metadata.HasProjection;
            rdoDefineSourceCrs.Checked = !metadata.HasProjection;
            rdoDefineSourceCrs.Enabled = !metadata.HasProjection;
            LoadSourceCrsOptions();
            cmbSourceCrs.SelectedItem = Wgs84Option;
            txtCustomCrs.Visible = true;
            ProjectionChoiceChanged(this, EventArgs.Empty);
            LoadRasterPreview();
        }

        /// <summary>
        /// Loads common source CRS options for rasters that do not store CRS metadata.
        /// </summary>
        private void LoadSourceCrsOptions()
        {
            cmbSourceCrs.Items.Clear();
            cmbSourceCrs.Items.Add(Wgs84Option);
            cmbSourceCrs.Items.Add(_projectCrsOption);
            cmbSourceCrs.Items.Add(WebMercatorOption);
            cmbSourceCrs.Items.Add(Utm44NOption);
            cmbSourceCrs.Items.Add(Utm45NOption);
            cmbSourceCrs.Items.Add(CustomEpsgOption);
            cmbSourceCrs.Items.Add(CustomWktOption);
        }

        /// <summary>
        /// Updates custom CRS text box state and short user guidance.
        /// </summary>
        private void ProjectionChoiceChanged(object? sender, EventArgs e)
        {
            bool defineSourceCrs = rdoDefineSourceCrs.Checked;
            bool customInputRequired = IsCustomSourceCrsSelected();
            lblDefineSourceCrs.Enabled = defineSourceCrs;
            cmbSourceCrs.Enabled = defineSourceCrs;
            txtCustomCrs.Enabled = defineSourceCrs && customInputRequired;
            txtCustomCrs.ReadOnly = !defineSourceCrs || !customInputRequired;

            if (!customInputRequired)
            {
                txtCustomCrs.PlaceholderText = "Selected CRS definition";
                txtCustomCrs.Text = GetSelectedSourceCrsDefinition();
            }
            else if (!defineSourceCrs || IsPresetSourceCrsDefinition(txtCustomCrs.Text))
            {
                txtCustomCrs.PlaceholderText = GetCustomCrsPlaceholderText();
                txtCustomCrs.Text = string.Empty;
            }
            else
            {
                txtCustomCrs.PlaceholderText = GetCustomCrsPlaceholderText();
            }

            if (!_preview.Metadata.HasGeoreferencing)
            {
                lblProjectionHint.Text =
                    "No map coordinates were found. The selected CRS will be assigned to the copied raster without reprojection.";
                return;
            }

            lblProjectionHint.Text = _preview.Metadata.HasProjection
                ? "This raster already stores a CRS. The stored raster CRS will be used."
                : "Missing source CRS defaults to WGS 1984 and will be transformed to the target CRS.";
        }

        /// <summary>
        /// Validates user choices before the form closes.
        /// </summary>
        private void btnImport_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLayerName.Text))
            {
                MessageBox.Show(
                    this,
                    "Please enter a layer name.",
                    "Raster Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (rdoDefineSourceCrs.Checked &&
                IsCustomSourceCrsSelected() &&
                string.IsNullOrWhiteSpace(txtCustomCrs.Text))
            {
                string expectedText = IsCustomEpsgSelected()
                    ? "Please enter an EPSG code such as 4326 or EPSG:4326."
                    : "Please paste the source CRS WKT text.";

                MessageBox.Show(
                    this,
                    expectedText,
                    "Raster Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (rdoDefineSourceCrs.Checked &&
                !TryValidateSourceCrs(
                    GetSelectedSourceCrsDefinition(),
                    out string validationError))
            {
                MessageBox.Show(
                    this,
                    validationError,
                    "Raster Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }

        /// <summary>
        /// Gets the CRS definition represented by the selected dropdown option.
        /// </summary>
        private string GetSelectedSourceCrsDefinition()
        {
            string selectedOption = cmbSourceCrs.SelectedItem?.ToString() ?? Wgs84Option;

            return selectedOption switch
            {
                Wgs84Option => DefaultMissingSourceCrs,
                string value when value == _projectCrsOption =>
                    _preview.ProjectCrs.TargetSrsDefinition,
                WebMercatorOption => "EPSG:3857",
                Utm44NOption => "EPSG:32644",
                Utm45NOption => "EPSG:32645",
                CustomEpsgOption => NormalizeEpsgText(txtCustomCrs.Text),
                CustomWktOption => txtCustomCrs.Text.Trim(),
                _ => DefaultMissingSourceCrs
            };
        }

        /// <summary>
        /// Determines whether the selected CRS option requires manual text input.
        /// </summary>
        private bool IsCustomSourceCrsSelected()
        {
            string selectedOption = cmbSourceCrs.SelectedItem?.ToString() ?? string.Empty;
            return selectedOption == CustomEpsgOption ||
                   selectedOption == CustomWktOption;
        }

        /// <summary>
        /// Determines whether the selected CRS option is the custom EPSG input.
        /// </summary>
        private bool IsCustomEpsgSelected()
        {
            return cmbSourceCrs.SelectedItem?.ToString() == CustomEpsgOption;
        }

        /// <summary>
        /// Returns example text for the active custom CRS input mode.
        /// </summary>
        private string GetCustomCrsPlaceholderText()
        {
            return IsCustomEpsgSelected()
                ? CustomEpsgPlaceholder
                : CustomWktPlaceholder;
        }

        /// <summary>
        /// Normalizes custom EPSG input into the form expected by GDAL.
        /// </summary>
        private static string NormalizeEpsgText(string value)
        {
            string trimmedValue = value.Trim();
            return trimmedValue.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase)
                ? trimmedValue
                : $"EPSG:{trimmedValue}";
        }

        private bool IsPresetSourceCrsDefinition(string value)
        {
            string text = value.Trim();
            return string.Equals(text, DefaultMissingSourceCrs, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(text, _preview.ProjectCrs.TargetSrsDefinition, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(text, "EPSG:3857", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(text, "EPSG:32644", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(text, "EPSG:32645", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryValidateSourceCrs(
            string sourceCrsDefinition,
            out string validationError)
        {
            validationError = string.Empty;

            if (string.IsNullOrWhiteSpace(sourceCrsDefinition))
            {
                validationError = "Please select or enter a source CRS.";
                return false;
            }

            try
            {
                using SpatialReference spatialReference = new(string.Empty);
                int result = sourceCrsDefinition.StartsWith(
                    "EPSG:",
                    StringComparison.OrdinalIgnoreCase)
                    ? spatialReference.SetFromUserInput(sourceCrsDefinition)
                    : spatialReference.ImportFromWkt(ref sourceCrsDefinition);

                if (result != 0)
                {
                    validationError = "The selected source CRS could not be read. Use EPSG:4326, 4326, or valid WKT text.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                validationError = $"The selected source CRS is invalid. {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Shows the low-quality GDAL-rendered raster preview.
        /// </summary>
        private void LoadRasterPreview()
        {
            if (_preview.PreviewImage == null)
            {
                picPreview.Visible = false;
                lblPreviewFallback.Visible = true;
                return;
            }

            picPreview.Image = new Bitmap(_preview.PreviewImage);
            picPreview.Visible = true;
            lblPreviewFallback.Visible = false;
        }

        private void mainLayout_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
