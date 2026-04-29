using Land_Readjustment_Tool.Services.Raster;

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
        private readonly RasterLayerImportPreview _preview;

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
                if (rdoDetectedCrs.Checked)
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

            rdoDetectedCrs.Enabled = metadata.HasProjection;
            rdoDetectedCrs.Checked = metadata.HasProjection;
            LoadSourceCrsOptions();
            cmbSourceCrs.Enabled = !metadata.HasProjection;
            cmbSourceCrs.SelectedItem = Wgs84Option;
            txtCustomCrs.Text = string.Empty;
            ProjectionChoiceChanged(this, EventArgs.Empty);
            TryLoadImagePreview(metadata.SourcePath);
        }

        /// <summary>
        /// Loads common source CRS options for rasters that do not store CRS metadata.
        /// </summary>
        private void LoadSourceCrsOptions()
        {
            cmbSourceCrs.Items.Clear();
            cmbSourceCrs.Items.Add(Wgs84Option);
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
            bool customInputRequired = IsCustomSourceCrsSelected();
            txtCustomCrs.Enabled = cmbSourceCrs.Enabled && customInputRequired;
            txtCustomCrs.Visible = cmbSourceCrs.Enabled && customInputRequired;

            if (!_preview.Metadata.HasGeoreferencing)
            {
                lblProjectionHint.Text =
                    "No map coordinates were found; CRS is kept for reference until georeferencing is added.";
                return;
            }

            lblProjectionHint.Text = _preview.Metadata.HasProjection
                ? "The detected source CRS will be transformed into the project CRS."
                : "Missing source CRS defaults to WGS 1984. Choose another CRS only if the source data uses it.";
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

            if (IsCustomSourceCrsSelected() &&
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
        /// Normalizes custom EPSG input into the form expected by GDAL.
        /// </summary>
        private static string NormalizeEpsgText(string value)
        {
            string trimmedValue = value.Trim();
            return trimmedValue.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase)
                ? trimmedValue
                : $"EPSG:{trimmedValue}";
        }

        /// <summary>
        /// Shows a simple preview for image formats that System.Drawing can decode directly.
        /// </summary>
        private void TryLoadImagePreview(string sourcePath)
        {
            try
            {
                using FileStream stream = File.OpenRead(sourcePath);
                using Image image = Image.FromStream(stream);
                picPreview.Image = new Bitmap(image);
                lblPreviewFallback.Visible = false;
            }
            catch
            {
                picPreview.Visible = false;
                lblPreviewFallback.Visible = true;
            }
        }
    }
}
