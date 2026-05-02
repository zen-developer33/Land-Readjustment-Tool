using System.ComponentModel;
using Land_Readjustment_Tool.Services.Raster;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Collects the settings needed to import an online XYZ tile source.
    /// </summary>
    public sealed partial class frmXyzTileImportOptions : Form
    {
        private const decimal NepalDefaultMinLongitude = 80.058622m;
        private const decimal NepalDefaultMinLatitude = 26.347000m;
        private const decimal NepalDefaultMaxLongitude = 88.201525m;
        private const decimal NepalDefaultMaxLatitude = 30.447020m;

        private readonly string _projectFolderPath;
        private readonly XyzTileImportOptionsState? _initialState;
        private readonly XyzTilePreDownloadService _tilePreDownloadService = new();
        private List<XyzTileSourceCatalogItem> _tileSources = [];
        private XyzTileSourceImportRequest? _downloadedRequest;
        private XyzTileSourceImportRequest? _lastSuccessfulDownloadRequest;
        private CancellationTokenSource? _downloadCancellation;
        private bool _isImportInProgress;

        public frmXyzTileImportOptions()
            : this(string.Empty)
        {
        }

        public frmXyzTileImportOptions(
            string projectFolderPath,
            XyzTileImportOptionsState? initialState = null)
        {
            _projectFolderPath = projectFolderPath;
            _initialState = initialState;
            InitializeComponent();
            WireRuntimeEvents();
            ConfigureRuntimeFormBehavior();
            ApplyNepalDefaultBounds();

            if (!IsDesignMode())
            {
                LoadTileSources();
                ApplyInitialState();
            }
        }

        public event EventHandler<XyzTileImportRequestedEventArgs>? ImportRequested;

        public event EventHandler<XyzTileImportOptionsStateChangedEventArgs>? OptionsStateChanged;

        public XyzTileSourceImportRequest ImportRequest =>
            new(
                txtLayerName.Text.Trim(),
                SelectedTileSource.UrlTemplate,
                decimal.ToDouble(numMinLongitude.Value),
                decimal.ToDouble(numMinLatitude.Value),
                decimal.ToDouble(numMaxLongitude.Value),
                decimal.ToDouble(numMaxLatitude.Value),
                decimal.ToInt32(numZoomLevel.Value),
                SelectedTileSource.ImageExtension);

        private XyzTileSourceCatalogItem SelectedTileSource =>
            cmbTileSource.SelectedItem as XyzTileSourceCatalogItem ??
            _tileSources.FirstOrDefault() ??
            new XyzTileSourceCatalogItem(
                "OpenStreetMap Standard",
                "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                0,
                19,
                "png");

        private void btnImport_Click(object? sender, EventArgs e)
        {
            if (_isImportInProgress)
                return;

            if (string.IsNullOrWhiteSpace(txtLayerName.Text))
            {
                ShowValidationMessage("Please enter a layer name.");
                return;
            }

            if (_downloadedRequest == null)
            {
                ShowValidationMessage(
                    "Please download tiles at least once before importing.");
                return;
            }

            XyzTileSourceImportRequest request =
                CreateImportRequestFromDownloadedTiles(
                    txtLayerName.Text.Trim(),
                    _downloadedRequest);

            ImportRequested?.Invoke(this, new XyzTileImportRequestedEventArgs(request));
        }

        private async void btnDownloadTiles_Click(object? sender, EventArgs e)
        {
            if (_isImportInProgress)
            {
                ShowValidationMessage("Please wait for the current import to finish.");
                return;
            }

            if (!TryBuildValidatedRequest(out XyzTileSourceImportRequest request))
                return;

            if (_downloadCancellation != null)
                return;

            _downloadCancellation = new CancellationTokenSource();
            SetDownloadUiState(isDownloading: true);
            lblDownloadStatus.Text = "Starting tile download...";
            progressTileDownload.Value = 0;

            try
            {
                Progress<XyzTileDownloadProgress> progress = new(update =>
                {
                    progressTileDownload.Value = Math.Clamp(update.Percent, 0, 100);
                    lblDownloadStatus.Text = FormatDownloadProgressStatus(update);
                });

                XyzTileDownloadResult result =
                    await _tilePreDownloadService.DownloadTilesAsync(
                        _projectFolderPath,
                        request,
                        progress,
                        _downloadCancellation.Token);

                _downloadedRequest = request;
                _lastSuccessfulDownloadRequest = request;
                btnImport.Enabled = true;
                progressTileDownload.Value = 100;
                lblDownloadStatus.Text =
                    "Download complete. Click Import to add to the project.";
                RaiseOptionsStateChanged();
            }
            catch (OperationCanceledException)
            {
                lblDownloadStatus.Text = "Tile download canceled.";
                btnImport.Enabled = false;
            }
            catch (Exception ex)
            {
                lblDownloadStatus.Text = "Tile download failed.";
                btnImport.Enabled = false;
                ShowValidationMessage($"Failed to download tiles: {ex.Message}");
            }
            finally
            {
                _downloadCancellation.Dispose();
                _downloadCancellation = null;
                SetDownloadUiState(isDownloading: false);
            }
        }

        private void ShowValidationMessage(string message)
        {
            MessageBox.Show(
                this,
                message,
                "Import XYZ Tiles",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }

        private static bool ContainsTileToken(string urlTemplate, string token)
        {
            return urlTemplate.Contains($"{{{token}}}", StringComparison.OrdinalIgnoreCase) ||
                   urlTemplate.Contains($"${{{token}}}", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadTileSources()
        {
            _tileSources = XyzTileSourceCatalogService.Load(_projectFolderPath);
            cmbTileSource.Items.Clear();

            foreach (XyzTileSourceCatalogItem source in _tileSources)
            {
                cmbTileSource.Items.Add(source);
            }

            if (cmbTileSource.Items.Count > 0)
            {
                cmbTileSource.SelectedIndex = 0;
            }

            RefreshSelectedSourceDetails();
            ResetDownloadState("Download tiles to enable Import.");
        }

        public XyzTileImportOptionsState GetCurrentOptionsState()
        {
            XyzTileSourceImportRequest? lastDownload =
                _lastSuccessfulDownloadRequest ??
                CreateDownloadRequestFromInitialState();

            return new XyzTileImportOptionsState(
                txtLayerName.Text.Trim(),
                SelectedTileSource.UrlTemplate,
                decimal.ToDouble(numMinLongitude.Value),
                decimal.ToDouble(numMinLatitude.Value),
                decimal.ToDouble(numMaxLongitude.Value),
                decimal.ToDouble(numMaxLatitude.Value),
                decimal.ToInt32(numZoomLevel.Value),
                SelectedTileSource.ImageExtension,
                lastDownload?.MinLongitude,
                lastDownload?.MinLatitude,
                lastDownload?.MaxLongitude,
                lastDownload?.MaxLatitude);
        }

        private void ApplyInitialState()
        {
            if (_initialState == null)
                return;

            if (!string.IsNullOrWhiteSpace(_initialState.LayerName))
                txtLayerName.Text = _initialState.LayerName;

            if (!string.IsNullOrWhiteSpace(_initialState.UrlTemplate))
                SelectTileSourceByUrl(_initialState.UrlTemplate);

            ApplyBounds(
                _initialState.MinLongitude,
                _initialState.MinLatitude,
                _initialState.MaxLongitude,
                _initialState.MaxLatitude);

            if (_initialState.ZoomLevel.HasValue)
            {
                numZoomLevel.Value = ClampNumericValue(
                    numZoomLevel,
                    (decimal)_initialState.ZoomLevel.Value);
            }

            _lastSuccessfulDownloadRequest =
                CreateDownloadRequestFromInitialState();
            _downloadedRequest = _lastSuccessfulDownloadRequest;

            if (_downloadedRequest != null)
            {
                btnImport.Enabled = true;
                progressTileDownload.Value = 100;
                lblDownloadStatus.Text =
                    "Previously downloaded tiles are ready. Click Import to add to the project.";
            }
            else
            {
                ResetDownloadState("Download tiles to enable Import.");
            }
        }

        private void ApplyBounds(
            double? minLongitude,
            double? minLatitude,
            double? maxLongitude,
            double? maxLatitude)
        {
            ApplyNullableDouble(numMinLongitude, minLongitude);
            ApplyNullableDouble(numMinLatitude, minLatitude);
            ApplyNullableDouble(numMaxLongitude, maxLongitude);
            ApplyNullableDouble(numMaxLatitude, maxLatitude);
        }

        private void SelectTileSourceByUrl(string urlTemplate)
        {
            for (int i = 0; i < cmbTileSource.Items.Count; i++)
            {
                if (cmbTileSource.Items[i] is XyzTileSourceCatalogItem item &&
                    string.Equals(
                        item.UrlTemplate,
                        urlTemplate,
                        StringComparison.OrdinalIgnoreCase))
                {
                    cmbTileSource.SelectedIndex = i;
                    return;
                }
            }
        }

        private XyzTileSourceImportRequest? CreateDownloadRequestFromInitialState()
        {
            if (_initialState == null ||
                !_initialState.LastDownloadMinLongitude.HasValue ||
                !_initialState.LastDownloadMinLatitude.HasValue ||
                !_initialState.LastDownloadMaxLongitude.HasValue ||
                !_initialState.LastDownloadMaxLatitude.HasValue)
            {
                return null;
            }

            return new XyzTileSourceImportRequest(
                _initialState.LayerName ?? string.Empty,
                _initialState.UrlTemplate ?? SelectedTileSource.UrlTemplate,
                _initialState.LastDownloadMinLongitude.Value,
                _initialState.LastDownloadMinLatitude.Value,
                _initialState.LastDownloadMaxLongitude.Value,
                _initialState.LastDownloadMaxLatitude.Value,
                _initialState.ZoomLevel ?? decimal.ToInt32(numZoomLevel.Value),
                _initialState.ImageExtension ?? SelectedTileSource.ImageExtension);
        }

        private void cmbTileSource_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RefreshSelectedSourceDetails();
        }

        private void btnManageSources_Click(object? sender, EventArgs e)
        {
            using frmXyzTileSourceManager manager = new(_projectFolderPath);

            if (manager.ShowDialog(this) == DialogResult.OK)
            {
                LoadTileSources();
            }
        }

        private void RefreshSelectedSourceDetails()
        {
            if (cmbTileSource.SelectedItem is not XyzTileSourceCatalogItem source)
            {
                txtUrlTemplate.Text = string.Empty;
                ResetDownloadState("Download tiles to enable Import.");
                return;
            }

            txtUrlTemplate.Text = NormalizeUrlForTextbox(source.UrlTemplate);
            decimal minZoom = source.MinZoom;
            decimal maxZoom = source.MaxZoom;
            decimal targetZoom = Math.Clamp(numZoomLevel.Value, minZoom, maxZoom);

            numZoomLevel.Minimum = 0;
            numZoomLevel.Maximum = 25;
            numZoomLevel.Value = targetZoom;
            numZoomLevel.Minimum = minZoom;
            numZoomLevel.Maximum = maxZoom;

            InvalidateDownloadedRequest();
        }

        private bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   DesignMode ||
                   string.IsNullOrWhiteSpace(_projectFolderPath);
        }

        private void WireRuntimeEvents()
        {
            cmbTileSource.SelectedIndexChanged += cmbTileSource_SelectedIndexChanged;
            btnManageSources.Click += btnManageSources_Click;
            btnImport.Click += btnImport_Click;
            btnDownloadTiles.Click += btnDownloadTiles_Click;
            btnCancel.Click += btnCancel_Click;
            FormClosing += frmXyzTileImportOptions_FormClosing;

            txtLayerName.TextChanged += (_, _) => InvalidateDownloadedRequest();
            numMinLongitude.ValueChanged += (_, _) => InvalidateDownloadedRequest();
            numMinLatitude.ValueChanged += (_, _) => InvalidateDownloadedRequest();
            numMaxLongitude.ValueChanged += (_, _) => InvalidateDownloadedRequest();
            numMaxLatitude.ValueChanged += (_, _) => InvalidateDownloadedRequest();
            numZoomLevel.ValueChanged += (_, _) => InvalidateDownloadedRequest();
        }

        private void ConfigureRuntimeFormBehavior()
        {
            // Runtime-only behavior so designer loading is never required.
            btnImport.DialogResult = DialogResult.None;
            btnCancel.DialogResult = DialogResult.None;
            AcceptButton = btnImport;
            CancelButton = btnCancel;
            MinimizeBox = true;
            ShowInTaskbar = true;
        }

        private void ApplyNepalDefaultBounds()
        {
            numMinLongitude.Value = ClampNumericValue(
                numMinLongitude,
                NepalDefaultMinLongitude);
            numMinLatitude.Value = ClampNumericValue(
                numMinLatitude,
                NepalDefaultMinLatitude);
            numMaxLongitude.Value = ClampNumericValue(
                numMaxLongitude,
                NepalDefaultMaxLongitude);
            numMaxLatitude.Value = ClampNumericValue(
                numMaxLatitude,
                NepalDefaultMaxLatitude);
        }

        private static decimal ClampNumericValue(
            NumericUpDown input,
            decimal value)
        {
            return Math.Min(input.Maximum, Math.Max(input.Minimum, value));
        }

        private static void ApplyNullableDouble(
            NumericUpDown input,
            double? value)
        {
            if (!value.HasValue || !double.IsFinite(value.Value))
                return;

            input.Value = ClampNumericValue(input, (decimal)value.Value);
        }

        private void frmXyzTileImportOptions_Load(object sender, EventArgs e)
        {

        }

        private bool TryBuildValidatedRequest(out XyzTileSourceImportRequest request)
        {
            request = null!;

            if (string.IsNullOrWhiteSpace(txtLayerName.Text))
            {
                ShowValidationMessage("Please enter a layer name.");
                return false;
            }

            if (_tileSources.Count == 0 || cmbTileSource.SelectedItem == null)
            {
                ShowValidationMessage("Please add or select an XYZ tile source.");
                return false;
            }

            XyzTileSourceCatalogItem selectedSource = SelectedTileSource;
            if (!ContainsTileToken(selectedSource.UrlTemplate, "z") ||
                !ContainsTileToken(selectedSource.UrlTemplate, "x") ||
                !ContainsTileToken(selectedSource.UrlTemplate, "y"))
            {
                ShowValidationMessage(
                    "The selected tile source URL must include {z}, {x}, and {y} tokens.");
                return false;
            }

            int zoom = decimal.ToInt32(numZoomLevel.Value);
            if (zoom < selectedSource.MinZoom || zoom > selectedSource.MaxZoom)
            {
                ShowValidationMessage(
                    $"Zoom level must be between {selectedSource.MinZoom} and {selectedSource.MaxZoom} for this source.");
                return false;
            }

            if (numMinLongitude.Value >= numMaxLongitude.Value)
            {
                ShowValidationMessage("Minimum longitude must be less than maximum longitude.");
                return false;
            }

            if (numMinLatitude.Value >= numMaxLatitude.Value)
            {
                ShowValidationMessage("Minimum latitude must be less than maximum latitude.");
                return false;
            }

            request = new XyzTileSourceImportRequest(
                txtLayerName.Text.Trim(),
                SelectedTileSource.UrlTemplate,
                decimal.ToDouble(numMinLongitude.Value),
                decimal.ToDouble(numMinLatitude.Value),
                decimal.ToDouble(numMaxLongitude.Value),
                decimal.ToDouble(numMaxLatitude.Value),
                decimal.ToInt32(numZoomLevel.Value),
                SelectedTileSource.ImageExtension);
            return true;
        }

        private void InvalidateDownloadedRequest()
        {
            if (_downloadCancellation != null)
            {
                return;
            }

            btnImport.Enabled = _downloadedRequest != null;

            if (_downloadedRequest == null)
            {
                progressTileDownload.Value = 0;
                lblDownloadStatus.Text = "Download tiles to enable Import.";
                return;
            }

            lblDownloadStatus.Text =
                "Settings changed. You can import the last downloaded tiles or download again to refresh.";
        }

        private static XyzTileSourceImportRequest CreateImportRequestFromDownloadedTiles(
            string layerName,
            XyzTileSourceImportRequest downloadedRequest)
        {
            return new XyzTileSourceImportRequest(
                layerName,
                downloadedRequest.UrlTemplate,
                downloadedRequest.MinLongitude,
                downloadedRequest.MinLatitude,
                downloadedRequest.MaxLongitude,
                downloadedRequest.MaxLatitude,
                downloadedRequest.ZoomLevel,
                downloadedRequest.ImageExtension);
        }

        private void ResetDownloadState(string statusText)
        {
            _downloadedRequest = null;
            btnImport.Enabled = false;
            progressTileDownload.Value = 0;
            lblDownloadStatus.Text = statusText;
        }

        private void SetDownloadUiState(bool isDownloading)
        {
            btnDownloadTiles.Enabled = !isDownloading;
            btnManageSources.Enabled = !isDownloading;
            cmbTileSource.Enabled = !isDownloading;
            txtLayerName.Enabled = !isDownloading;
            numMinLongitude.Enabled = !isDownloading;
            numMinLatitude.Enabled = !isDownloading;
            numMaxLongitude.Enabled = !isDownloading;
            numMaxLatitude.Enabled = !isDownloading;
            numZoomLevel.Enabled = !isDownloading;
            btnCancel.Enabled = true;
        }

        public void BeginImportExecution(string statusText = "Importing tiles to map...")
        {
            if (IsDisposed)
                return;

            _isImportInProgress = true;
            btnImport.Enabled = false;
            btnDownloadTiles.Enabled = false;
            btnManageSources.Enabled = false;
            lblDownloadStatus.Text = statusText;
        }

        public void CompleteImportExecution(string statusText)
        {
            if (IsDisposed)
                return;

            _isImportInProgress = false;
            btnManageSources.Enabled = true;
            btnDownloadTiles.Enabled = _downloadCancellation == null;
            btnImport.Enabled = _downloadedRequest != null;
            lblDownloadStatus.Text = statusText;
        }

        public void FailImportExecution(string statusText)
        {
            if (IsDisposed)
                return;

            _isImportInProgress = false;
            btnManageSources.Enabled = true;
            btnDownloadTiles.Enabled = _downloadCancellation == null;
            btnImport.Enabled = _downloadedRequest != null;
            lblDownloadStatus.Text = statusText;
        }

        private static bool AreEquivalentRequests(
            XyzTileSourceImportRequest left,
            XyzTileSourceImportRequest right)
        {
            return string.Equals(left.LayerName, right.LayerName, StringComparison.Ordinal) &&
                   string.Equals(left.UrlTemplate, right.UrlTemplate, StringComparison.Ordinal) &&
                   left.MinLongitude.Equals(right.MinLongitude) &&
                   left.MinLatitude.Equals(right.MinLatitude) &&
                   left.MaxLongitude.Equals(right.MaxLongitude) &&
                   left.MaxLatitude.Equals(right.MaxLatitude) &&
                   left.ZoomLevel == right.ZoomLevel &&
                   string.Equals(left.ImageExtension, right.ImageExtension, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeUrlForTextbox(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            return url
                .Replace("\r\n", string.Empty, StringComparison.Ordinal)
                .Replace("\n", string.Empty, StringComparison.Ordinal)
                .Replace("\r", string.Empty, StringComparison.Ordinal)
                .Trim();
        }

        private static string FormatDownloadProgressStatus(
            XyzTileDownloadProgress progress)
        {
            return
                $"{progress.Percent}% ({progress.CompletedTiles:N0}/{progress.TotalTiles:N0} tiles) - {progress.Status}";
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            if (_downloadCancellation != null)
            {
                PromptToCancelActiveDownload();
                return;
            }

            if (_isImportInProgress)
            {
                ShowValidationMessage("Import is in progress. Please wait until it completes.");
                return;
            }

            Close();
        }

        private void frmXyzTileImportOptions_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_downloadCancellation == null)
            {
                RaiseOptionsStateChanged();
                return;
            }

            DialogResult result = MessageBox.Show(
                this,
                "Tile download is in progress. Stop downloading tiles?",
                "Import XYZ Tiles",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                _downloadCancellation.Cancel();
            }

            e.Cancel = true;
        }

        private void RaiseOptionsStateChanged()
        {
            OptionsStateChanged?.Invoke(
                this,
                new XyzTileImportOptionsStateChangedEventArgs(
                    GetCurrentOptionsState()));
        }

        private void PromptToCancelActiveDownload()
        {
            DialogResult result = MessageBox.Show(
                this,
                "Tile download is in progress. Stop downloading tiles?",
                "Import XYZ Tiles",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                _downloadCancellation?.Cancel();
            }
        }

        private void lblHint_Click(object sender, EventArgs e)
        {

        }

        private void layout_Paint(object sender, PaintEventArgs e)
        {

        }

        private void frmXyzTileImportOptions_Load_1(object sender, EventArgs e)
        {

        }
    }

    public sealed class XyzTileImportRequestedEventArgs : EventArgs
    {
        public XyzTileImportRequestedEventArgs(XyzTileSourceImportRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public XyzTileSourceImportRequest Request { get; }
    }

    public sealed class XyzTileImportOptionsStateChangedEventArgs : EventArgs
    {
        public XyzTileImportOptionsStateChangedEventArgs(
            XyzTileImportOptionsState state)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public XyzTileImportOptionsState State { get; }
    }
}
