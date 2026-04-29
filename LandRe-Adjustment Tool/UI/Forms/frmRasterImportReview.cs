using Land_Readjustment_Tool.Services.Raster;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Lets the user review raster metadata and define a source CRS before importing.
    /// </summary>
    public partial class frmRasterImportReview : Form
    {
        private const string DefaultMissingSourceCrs = "EPSG:4326";
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
            rdoWgs84Crs.CheckedChanged += ProjectionChoiceChanged;
            rdoCustomCrs.CheckedChanged += ProjectionChoiceChanged;
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

                if (rdoWgs84Crs.Checked)
                    return DefaultMissingSourceCrs;

                return txtCustomCrs.Text.Trim();
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
            rdoWgs84Crs.Enabled = !metadata.HasProjection;
            rdoWgs84Crs.Checked = !metadata.HasProjection;
            rdoCustomCrs.Enabled = !metadata.HasProjection;
            txtCustomCrs.Text = DefaultMissingSourceCrs;
            ProjectionChoiceChanged(this, EventArgs.Empty);
            TryLoadImagePreview(metadata.SourcePath);
        }

        /// <summary>
        /// Updates custom CRS text box state and short user guidance.
        /// </summary>
        private void ProjectionChoiceChanged(object? sender, EventArgs e)
        {
            txtCustomCrs.Enabled = rdoCustomCrs.Checked;

            if (!_preview.Metadata.HasGeoreferencing)
            {
                lblProjectionHint.Text =
                    "No map coordinates were found; CRS is kept for reference until georeferencing is added.";
                return;
            }

            lblProjectionHint.Text = _preview.Metadata.HasProjection
                ? "The detected source CRS will be transformed into the project CRS."
                : "Missing source CRS defaults to WGS 1984 unless you define another source CRS.";
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

            if (rdoCustomCrs.Checked &&
                string.IsNullOrWhiteSpace(txtCustomCrs.Text))
            {
                MessageBox.Show(
                    this,
                    "Please enter a source CRS such as EPSG:4326.",
                    "Raster Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
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
