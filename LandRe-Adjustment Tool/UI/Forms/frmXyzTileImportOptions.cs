using System.ComponentModel;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.Helpers;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Collects the settings needed to import an online XYZ tile source.
    /// Supports three bounds modes: Center + Radius, Bounding Box, and Live Tiles.
    /// </summary>
    public sealed partial class frmXyzTileImportOptions : Form
    {
        // ── Nepal bbox defaults (for Bounding Box mode) ──────────────────────────
        private const decimal NepalDefaultMinLongitude = 80.058622m;
        private const decimal NepalDefaultMinLatitude = 26.347000m;
        private const decimal NepalDefaultMaxLongitude = 88.201525m;
        private const decimal NepalDefaultMaxLatitude = 30.447020m;

        // ── Nepal center+radius defaults (for Center + Radius mode) ──────────────
        private const decimal NepalDefaultCenterLat = 27.7172m;   // Kathmandu
        private const decimal NepalDefaultCenterLon = 85.3240m;
        private const decimal NepalDefaultRadiusKm = 10.0m;

        // ── Source-name abbreviation table ───────────────────────────────────────
        private static readonly Dictionary<string, string> _sourceAbbreviations =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // OpenStreetMap
                ["OpenStreetMap Standard"] = "OSM",
                ["OpenStreetMap Humanitarian (HOT)"] = "OSM-HOT",
                // Bing
                ["Bing Aerial"] = "Bing-Aerial",
                ["Bing Aerial with Labels"] = "Bing-Hybrid",
                ["Bing Road Map"] = "Bing-Road",
                // Esri
                ["Esri World Imagery"] = "ESRI-Img",
                ["Esri World Street Map"] = "ESRI-Streets",
                ["Esri World Topo Map"] = "ESRI-Topo",
                ["Esri World Light Gray Canvas"] = "ESRI-LGray",
                ["Esri World Dark Gray Canvas"] = "ESRI-DGray",
                ["Esri World Shaded Relief"] = "ESRI-Relief",
                ["Esri World Physical Map"] = "ESRI-Phys",
                // Google
                ["Google Satellite"] = "GSat",
                ["Google Hybrid (Satellite + Labels)"] = "GHyb",
                ["Google Streets"] = "GStreets",
                ["Google Terrain"] = "GTerrain",
                // CartoDB
                ["CartoDB Positron (Light)"] = "Carto-Light",
                ["CartoDB Dark Matter"] = "Carto-Dark",
                ["CartoDB Positron (No Labels)"] = "Carto-NoLbl",
                // Stadia
                ["Stadia Stamen Terrain"] = "Stadia-Terrain",
                ["Stadia Stamen Toner"] = "Stadia-Toner",
                ["Stadia Stamen Toner Lite"] = "Stadia-TonerLite",
                ["Stadia Alidade Smooth"] = "Stadia-Smooth",
                ["Stadia Alidade Smooth Dark"] = "Stadia-SmoothDk",
                ["Stadia OSM Bright"] = "Stadia-Bright",
                // OpenTopo
                ["OpenTopoMap"] = "OTopo",
                // USGS
                ["USGS Topo"] = "USGS-Topo",
                ["USGS Imagery"] = "USGS-Img",
                // Wikimedia
                ["Wikimedia Maps"] = "Wiki",
            };

        private readonly string _projectFolderPath;
        private readonly XyzTileImportOptionsState? _initialState;
        private readonly XyzTilePreDownloadService _tilePreDownloadService = new();
        private List<XyzTileSourceCatalogItem> _tileSources = [];
        private XyzTileSourceImportRequest? _downloadedRequest;
        private XyzTileSourceImportRequest? _lastSuccessfulDownloadRequest;
        private CancellationTokenSource? _downloadCancellation;
        private bool _isImportInProgress;
        private bool _closingAfterImport;

        /// <summary>
        /// Tracks the last auto-injected layer name so a user-typed name is
        /// never overwritten by a subsequent auto-suggestion.
        /// </summary>
        private string _lastAutoSuggestedLayerName = string.Empty;

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
            NumericUpDownSelectAllBehavior.AttachTo(this);
            WireRuntimeEvents();
            ApplyNepalDefaultBounds();

            if (!IsDesignMode())
            {
                LoadTileSources();
                ApplyInitialState();
            }
        }

        public event EventHandler<XyzTileImportRequestedEventArgs>? ImportRequested;
        public event EventHandler<XyzTileImportOptionsStateChangedEventArgs>? OptionsStateChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<(double West, double South, double East, double North)?>?
            CurrentViewportBoundsProvider
        { get; set; }

        // ── Public read properties ───────────────────────────────────────────────

        public XyzTileSourceImportRequest ImportRequest
        {
            get
            {
                var (minLon, minLat, maxLon, maxLat) = GetCurrentBounds();
                return new(
                    txtLayerName.Text.Trim(),
                    SelectedTileSource.UrlTemplate,
                    minLon, minLat, maxLon, maxLat,
                    GetEffectiveZoomLevel(SelectedTileSource),
                    SelectedTileSource.ImageExtension,
                    IsLiveMode);
            }
        }

        private bool IsLiveMode => rdoLiveTiles.Checked;

        private XyzTileSourceCatalogItem SelectedTileSource =>
            cmbTileSource.SelectedItem as XyzTileSourceCatalogItem ??
            _tileSources.FirstOrDefault() ??
            new XyzTileSourceCatalogItem(
                "OpenStreetMap Standard",
                "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                0, 19, "png");

        // ── Button handlers ──────────────────────────────────────────────────────

        private void btnImport_Click(object? sender, EventArgs e)
        {
            if (_isImportInProgress)
                return;

            if (string.IsNullOrWhiteSpace(txtLayerName.Text))
            {
                ShowValidationMessage("Please enter a layer name.");
                return;
            }

            XyzTileSourceImportRequest request;

            if (IsLiveMode)
            {
                // Live mode: build request directly — no prior download needed.
                XyzTileSourceCatalogItem src = SelectedTileSource;
                if (!HasUsableTileTokens(src.UrlTemplate))
                {
                    ShowValidationMessage(
                        "The selected tile source URL must include {z}, {x}, and {y} tokens, or {quadkey} for Bing Maps.");
                    return;
                }

                request = new XyzTileSourceImportRequest(
                    txtLayerName.Text.Trim(),
                    src.UrlTemplate,
                    -180, -85.05112878, 180, 85.05112878,
                    GetEffectiveZoomLevel(src),
                    src.ImageExtension,
                    IsLiveTiles: true);
            }
            else
            {
                if (_downloadedRequest == null)
                {
                    ShowValidationMessage(
                        "Please download tiles at least once before importing.");
                    return;
                }

                request = CreateImportRequestFromDownloadedTiles(
                    txtLayerName.Text.Trim(),
                    _downloadedRequest);
            }

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

                // Always update the layer name to the formatted name once download finishes.
                string downloadedName = BuildDownloadedLayerName(SelectedTileSource.Name, request);
                txtLayerName.Text = downloadedName;
                _lastAutoSuggestedLayerName = downloadedName;

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
                ShowValidationMessage(
                    $"Failed to download tiles: {XyzTileErrorMessageBuilder.AddUserGuidance(ex.Message)}");
            }
            finally
            {
                _downloadCancellation.Dispose();
                _downloadCancellation = null;
                SetDownloadUiState(isDownloading: false);
            }
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            if (_downloadCancellation != null)
                PromptToCancelActiveDownload();
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            // btnClose is disabled while download or import is in progress, so this
            // handler can always close without an additional guard.
            Close();
        }

        // ── Bounds mode toggle ───────────────────────────────────────────────────

        private void rdoCenterRadius_CheckedChanged(object? sender, EventArgs e)
        {
            pnlCenterRadius.Visible = rdoCenterRadius.Checked;
            pnlBoundingBox.Visible = rdoBoundingBox.Checked;
            pnlLiveTilesInfo.Visible = rdoLiveTiles.Checked;

            // Progress bar and status label are irrelevant in Live Tiles mode.
            SetProgressAreaVisible(!rdoLiveTiles.Checked);
            UpdateZoomControlState();

            // When switching INTO bbox mode, auto-fill N/S/E/W from current center+radius.
            if (rdoBoundingBox.Checked)
                SyncBboxFromCenterRadius();

            // In live mode Import is always enabled (no download required);
            // disable Download button since it is irrelevant.
            if (rdoLiveTiles.Checked)
            {
                btnImport.Enabled = true;
                btnDownloadTiles.Enabled = false;
                SuggestLayerName(BuildLiveLayerName(SelectedTileSource.Name, GetEffectiveZoomLevel(SelectedTileSource)));
            }
            else
            {
                btnDownloadTiles.Enabled = _downloadCancellation == null;
                btnImport.Enabled = _downloadedRequest != null;
            }

            InvalidateDownloadedRequest();
        }

        /// <summary>
        /// Populates the Bounding Box fields from the current Center + Radius values
        /// whenever the user switches to Bounding Box mode.
        /// </summary>
        private void SyncBboxFromCenterRadius()
        {
            var (minLon, minLat, maxLon, maxLat) = CenterRadiusToBbox(
                decimal.ToDouble(numCenterLat.Value),
                decimal.ToDouble(numCenterLon.Value),
                decimal.ToDouble(numRadius.Value));

            numWest.Value = ClampNumericValue(numWest, (decimal)minLon);
            numSouth.Value = ClampNumericValue(numSouth, (decimal)minLat);
            numEast.Value = ClampNumericValue(numEast, (decimal)maxLon);
            numNorth.Value = ClampNumericValue(numNorth, (decimal)maxLat);
        }

        // ── Source selection ─────────────────────────────────────────────────────

        private void cmbTileSource_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RefreshSelectedSourceDetails();
        }

        private void btnManageSources_Click(object? sender, EventArgs e)
        {
            using frmXyzTileSourceManager manager = new(_projectFolderPath);

            if (manager.ShowDialog(this) == DialogResult.OK)
                LoadTileSources();
        }

        private void btnGetBoundsFromViewport_Click(object? sender, EventArgs e)
        {
            var bounds = CurrentViewportBoundsProvider?.Invoke();
            if (bounds == null)
            {
                ShowValidationMessage(
                    "Current viewport bounds could not be converted to latitude/longitude.");
                return;
            }

            ApplyBounds(
                bounds.Value.West,
                bounds.Value.South,
                bounds.Value.East,
                bounds.Value.North);
            InvalidateDownloadedRequest();
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
            decimal targetZoom = IsLiveMode
                ? maxZoom
                : Math.Clamp(numZoomLevel.Value, minZoom, maxZoom);

            numZoomLevel.Minimum = 0;
            numZoomLevel.Maximum = 25;
            numZoomLevel.Value = targetZoom;
            numZoomLevel.Minimum = minZoom;
            numZoomLevel.Maximum = maxZoom;
            UpdateZoomControlState();

            // In live mode refresh the layer name suggestion immediately.
            if (IsLiveMode)
                SuggestLayerName(BuildLiveLayerName(source.Name, GetEffectiveZoomLevel(source)));
            else
                SuggestLayerName(source.Name.Trim());

            InvalidateDownloadedRequest();
        }

        // ── Loading ──────────────────────────────────────────────────────────────

        private void LoadTileSources()
        {
            _tileSources = XyzTileSourceCatalogService.Load(_projectFolderPath);
            cmbTileSource.Items.Clear();

            foreach (XyzTileSourceCatalogItem source in _tileSources)
                cmbTileSource.Items.Add(source);

            if (cmbTileSource.Items.Count > 0)
                cmbTileSource.SelectedIndex = 0;

            RefreshSelectedSourceDetails();
            ResetDownloadState("Download tiles to enable Import.");
        }

        // ── State read / write ───────────────────────────────────────────────────

        public XyzTileImportOptionsState GetCurrentOptionsState()
        {
            XyzTileSourceImportRequest? lastDownload =
                _lastSuccessfulDownloadRequest ??
                CreateDownloadRequestFromInitialState();

            var (minLon, minLat, maxLon, maxLat) = GetCurrentBounds();

            return new XyzTileImportOptionsState(
                txtLayerName.Text.Trim(),
                SelectedTileSource.UrlTemplate,
                minLon, minLat, maxLon, maxLat,
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
            {
                txtLayerName.Text = _initialState.LayerName;
                _lastAutoSuggestedLayerName = _initialState.LayerName;
            }

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

            _lastSuccessfulDownloadRequest = CreateDownloadRequestFromInitialState();
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

        /// <summary>
        /// Populates both the Bounding Box controls and derives matching
        /// Center + Radius values from the same bbox.
        /// </summary>
        private void ApplyBounds(
            double? minLongitude,
            double? minLatitude,
            double? maxLongitude,
            double? maxLatitude)
        {
            ApplyNullableDouble(numWest, minLongitude);
            ApplyNullableDouble(numSouth, minLatitude);
            ApplyNullableDouble(numEast, maxLongitude);
            ApplyNullableDouble(numNorth, maxLatitude);

            if (minLongitude.HasValue && minLatitude.HasValue &&
                maxLongitude.HasValue && maxLatitude.HasValue)
            {
                (double cLat, double cLon, double radius) = DeriveCenterRadiusFromBbox(
                    minLongitude.Value, minLatitude.Value,
                    maxLongitude.Value, maxLatitude.Value);

                numCenterLat.Value = ClampNumericValue(numCenterLat, (decimal)cLat);
                numCenterLon.Value = ClampNumericValue(numCenterLon, (decimal)cLon);
                numRadius.Value = ClampNumericValue(numRadius, (decimal)radius);
            }

            UpdateAreaHint();
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

        // ── Validation ───────────────────────────────────────────────────────────

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
            if (!HasUsableTileTokens(selectedSource.UrlTemplate))
            {
                ShowValidationMessage(
                    "The selected tile source URL must include {z}, {x}, and {y} tokens, or {quadkey} for Bing Maps.");
                return false;
            }

            int zoom = GetEffectiveZoomLevel(selectedSource);
            if (zoom < selectedSource.MinZoom || zoom > selectedSource.MaxZoom)
            {
                ShowValidationMessage(
                    $"Zoom level must be between {selectedSource.MinZoom} and " +
                    $"{selectedSource.MaxZoom} for this source.");
                return false;
            }

            var (minLon, minLat, maxLon, maxLat) = GetCurrentBounds();

            if (minLon >= maxLon)
            {
                string msg = rdoCenterRadius.Checked
                    ? "Radius produces an invalid longitude range. Try a smaller radius or adjust the center."
                    : "West longitude must be less than East longitude.";
                ShowValidationMessage(msg);
                return false;
            }

            if (minLat >= maxLat)
            {
                string msg = rdoCenterRadius.Checked
                    ? "Radius produces an invalid latitude range. Try a smaller radius or adjust the center."
                    : "South latitude must be less than North latitude.";
                ShowValidationMessage(msg);
                return false;
            }

            request = new XyzTileSourceImportRequest(
                txtLayerName.Text.Trim(),
                selectedSource.UrlTemplate,
                minLon, minLat, maxLon, maxLat,
                zoom,
                selectedSource.ImageExtension);
            return true;
        }

        // ── Bounds helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns the current import bounds (minLon, minLat, maxLon, maxLat)
        /// from whichever bounds mode is active.
        /// Live mode returns the global Web Mercator extent.
        /// </summary>
        private (double minLon, double minLat, double maxLon, double maxLat) GetCurrentBounds()
        {
            if (rdoLiveTiles.Checked)
                return (-180.0, -85.05112878, 180.0, 85.05112878);

            if (rdoCenterRadius.Checked)
            {
                return CenterRadiusToBbox(
                    decimal.ToDouble(numCenterLat.Value),
                    decimal.ToDouble(numCenterLon.Value),
                    decimal.ToDouble(numRadius.Value));
            }

            return (
                decimal.ToDouble(numWest.Value),
                decimal.ToDouble(numSouth.Value),
                decimal.ToDouble(numEast.Value),
                decimal.ToDouble(numNorth.Value));
        }

        /// <summary>
        /// Converts a centre point and radius (km) to a WGS-84 bounding box.
        /// Δlat ≈ r / 111 km/degree,  Δlon ≈ r / (111 · cos(lat))
        /// </summary>
        private static (double minLon, double minLat, double maxLon, double maxLat)
            CenterRadiusToBbox(double centerLat, double centerLon, double radiusKm)
        {
            double deltaLat = radiusKm / 111.0;
            double cosLat = Math.Cos(centerLat * Math.PI / 180.0);
            double deltaLon = cosLat > 1e-6
                ? radiusKm / (111.0 * cosLat)
                : radiusKm / 111.0;

            return (centerLon - deltaLon, centerLat - deltaLat,
                    centerLon + deltaLon, centerLat + deltaLat);
        }

        /// <summary>
        /// Derives the best-fit Center + Radius values from a bounding box.
        /// Radius is the smaller of the N-S and E-W half-spans in km.
        /// </summary>
        private static (double centerLat, double centerLon, double radiusKm)
            DeriveCenterRadiusFromBbox(
                double minLon, double minLat,
                double maxLon, double maxLat)
        {
            double centerLat = (minLat + maxLat) / 2.0;
            double centerLon = (minLon + maxLon) / 2.0;
            double latRadiusKm = (maxLat - minLat) * 111.0 / 2.0;
            double cosLat = Math.Cos(centerLat * Math.PI / 180.0);
            double lonRadiusKm = cosLat > 1e-6
                ? (maxLon - minLon) * 111.0 * cosLat / 2.0
                : (maxLon - minLon) * 111.0 / 2.0;

            double radius = Math.Max(0.1, Math.Round(Math.Min(latRadiusKm, lonRadiusKm), 1));
            return (centerLat, centerLon, radius);
        }

        // ── Layer-name suggestion ────────────────────────────────────────────────

        /// <summary>
        /// Writes <paramref name="suggested"/> into <see cref="txtLayerName"/> only when
        /// the field is empty or still shows our last auto-suggestion.
        /// A name typed manually by the user is never overwritten.
        /// </summary>
        private void SuggestLayerName(string suggested)
        {
            string current = txtLayerName.Text.Trim();

            bool isEmpty = string.IsNullOrEmpty(current);
            bool isStillOurSuggestion =
                string.Equals(current, _lastAutoSuggestedLayerName, StringComparison.Ordinal);

            if (isEmpty || isStillOurSuggestion)
            {
                txtLayerName.Text = suggested;
                _lastAutoSuggestedLayerName = suggested;
            }
        }

        /// <summary>
        /// Returns a short abbreviation for a tile source name.
        /// Falls back to a PascalCase-initial abbreviation for user-added sources.
        /// Example: "My Custom Layer" → "MyCusLay"
        /// </summary>
        private static string AbbreviateSourceName(string sourceName)
        {
            if (_sourceAbbreviations.TryGetValue(sourceName.Trim(), out string? abbrev))
                return abbrev;

            // Fallback: concatenate up to 3 words, each capitalised, max 10 chars total.
            string[] words = sourceName
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string joined = string.Concat(
                words.Take(3).Select(w => char.ToUpperInvariant(w[0]) + w[1..Math.Min(w.Length, 4)]));

            return joined.Length > 10 ? joined[..10] : joined;
        }

        /// <summary>
        /// Builds the layer name used after a successful tile download.
        /// Format: <c>GSat [84.1E 27.3N] Z16</c>
        /// Center is the midpoint of the downloaded bbox.
        /// </summary>
        private static string BuildDownloadedLayerName(
            string sourceName,
            XyzTileSourceImportRequest request)
        {
            double centerLon = (request.MinLongitude + request.MaxLongitude) / 2.0;
            double centerLat = (request.MinLatitude + request.MaxLatitude) / 2.0;

            string lonPart = centerLon >= 0
                ? $"{centerLon:F1}E"
                : $"{Math.Abs(centerLon):F1}W";
            string latPart = centerLat >= 0
                ? $"{centerLat:F1}N"
                : $"{Math.Abs(centerLat):F1}S";

            string abbrev = AbbreviateSourceName(sourceName);
            return $"{abbrev} [{lonPart} {latPart}] Z{request.ZoomLevel}";
        }

        /// <summary>
        /// Builds the layer name used for a live (on-demand internet) tile layer.
        /// Format: <c>World Imagery (Google Satellite)</c>
        /// </summary>
        private static string BuildLiveLayerName(string sourceName, int zoomLevel)
        {
            return $"World Imagery ({sourceName})";
        }

        // ── Area hint ────────────────────────────────────────────────────────────

        /// <summary>
        /// Refreshes the ≈ W × H hint next to the radius field.
        /// The bbox from radius r is always ≈ 2r × 2r km (by construction).
        /// Displays metres when the diameter is below 1 km.
        /// </summary>
        private void UpdateAreaHint()
        {
            double diameter = decimal.ToDouble(numRadius.Value) * 2.0;
            lblAreaHint.Text = diameter < 1.0
                ? $"≈ {diameter * 1000:F0} × {diameter * 1000:F0} m area"
                : $"≈ {diameter:F3} × {diameter:F3} km area";
        }

        // ── UI state helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Enables or disables the download-status label and progress row without
        /// changing their layout visibility.
        /// </summary>
        private void SetProgressAreaVisible(bool visible)
        {
            lblDownloadStatus.Enabled = visible;
            pnlProgressRow.Enabled = visible;
            progressTileDownload.Enabled = visible;
            btnCancel.Enabled = visible && _downloadCancellation != null;
        }

        private void InvalidateDownloadedRequest()
        {
            if (_downloadCancellation != null)
                return;

            // In live mode Import is always enabled — no download required.
            if (IsLiveMode)
            {
                btnImport.Enabled = true;
                btnDownloadTiles.Enabled = false;
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

        private void ResetDownloadState(string statusText)
        {
            _downloadedRequest = null;
            btnImport.Enabled = IsLiveMode;
            progressTileDownload.Value = 0;
            lblDownloadStatus.Text = statusText;
        }

        private void SetDownloadUiState(bool isDownloading)
        {
            btnDownloadTiles.Enabled = !isDownloading;
            btnManageSources.Enabled = !isDownloading;
            cmbTileSource.Enabled = !isDownloading;
            txtLayerName.Enabled = !isDownloading;
            rdoCenterRadius.Enabled = !isDownloading;
            rdoBoundingBox.Enabled = !isDownloading;
            rdoLiveTiles.Enabled = !isDownloading;
            numCenterLat.Enabled = !isDownloading;
            numCenterLon.Enabled = !isDownloading;
            numRadius.Enabled = !isDownloading;
            numNorth.Enabled = !isDownloading;
            numSouth.Enabled = !isDownloading;
            numEast.Enabled = !isDownloading;
            numWest.Enabled = !isDownloading;
            btnGetBoundsFromViewport.Enabled =
                !isDownloading &&
                CurrentViewportBoundsProvider != null;
            numZoomLevel.Enabled = !isDownloading && !IsLiveMode;
            btnCancel.Enabled = isDownloading && !_isImportInProgress;
            btnClose.Enabled = !isDownloading;
            progressTileDownload.Enabled = true;
            UpdateZoomControlState();
        }

        public void BeginImportExecution(string statusText = "Importing tiles to map...")
        {
            if (IsDisposed) return;

            _isImportInProgress = true;
            btnImport.Enabled = false;
            btnDownloadTiles.Enabled = false;
            btnManageSources.Enabled = false;
            btnClose.Enabled = false;
            lblDownloadStatus.Text = statusText;
            UpdateZoomControlState();
        }

        public void CompleteImportExecution(string statusText)
        {
            if (IsDisposed) return;

            _isImportInProgress = false;
            btnManageSources.Enabled = true;
            btnDownloadTiles.Enabled = _downloadCancellation == null && !IsLiveMode;
            btnImport.Enabled = _downloadedRequest != null || IsLiveMode;
            btnClose.Enabled = true;
            lblDownloadStatus.Text = statusText;
            UpdateZoomControlState();
        }



        public void FailImportExecution(string statusText)
        {
            if (IsDisposed) return;

            _isImportInProgress = false;
            btnManageSources.Enabled = true;
            btnDownloadTiles.Enabled = _downloadCancellation == null && !IsLiveMode;
            btnImport.Enabled = _downloadedRequest != null || IsLiveMode;
            btnClose.Enabled = true;
            lblDownloadStatus.Text = statusText;
            UpdateZoomControlState();
        }

        /// <summary>
        /// Shows the completion message then closes the form.
        /// Called after a successful download-and-import workflow.
        /// </summary>
        public void CompleteImportAndClose(string statusText)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(() => CompleteImportAndClose(statusText));
                return;
            }

            CompleteImportExecution(statusText);

            // Give the user a moment to see the success message before closing.
            Task.Delay(1200).ContinueWith(_ =>
            {
                if (!IsDisposed && IsHandleCreated)
                    BeginInvoke(Close);
            });
        }

        // ── Defaults ─────────────────────────────────────────────────────────────

        private void ApplyNepalDefaultBounds()
        {
            // Center + Radius mode: Kathmandu, 10 km radius
            numCenterLat.Value = ClampNumericValue(numCenterLat, NepalDefaultCenterLat);
            numCenterLon.Value = ClampNumericValue(numCenterLon, NepalDefaultCenterLon);
            numRadius.Value = ClampNumericValue(numRadius, NepalDefaultRadiusKm);

            // Bounding Box mode: all of Nepal
            numWest.Value = ClampNumericValue(numWest, NepalDefaultMinLongitude);
            numSouth.Value = ClampNumericValue(numSouth, NepalDefaultMinLatitude);
            numEast.Value = ClampNumericValue(numEast, NepalDefaultMaxLongitude);
            numNorth.Value = ClampNumericValue(numNorth, NepalDefaultMaxLatitude);

            UpdateAreaHint();
        }

        // ── Wiring ───────────────────────────────────────────────────────────────

        private void WireRuntimeEvents()
        {
            cmbTileSource.SelectedIndexChanged += cmbTileSource_SelectedIndexChanged;
            btnManageSources.Click += btnManageSources_Click;
            btnImport.Click += btnImport_Click;
            btnDownloadTiles.Click += btnDownloadTiles_Click;
            btnCancel.Click += btnCancel_Click;
            btnClose.Click += btnClose_Click;
            btnGetBoundsFromViewport.Click += btnGetBoundsFromViewport_Click;
            FormClosing += frmXyzTileImportOptions_FormClosing;

            // Radio buttons — rdoLiveTiles is wired in Designer; the others are here.
            rdoCenterRadius.CheckedChanged += rdoCenterRadius_CheckedChanged;
            rdoBoundingBox.CheckedChanged += rdoCenterRadius_CheckedChanged;

            txtLayerName.TextChanged += (_, _) => InvalidateDownloadedRequest();

            // Center + Radius controls — also refresh the area hint.
            numCenterLat.ValueChanged += (_, _) =>
            {
                UpdateAreaHint();
                InvalidateDownloadedRequest();
            };
            numCenterLon.ValueChanged += (_, _) =>
            {
                UpdateAreaHint();
                InvalidateDownloadedRequest();
            };
            numRadius.ValueChanged += (_, _) =>
            {
                UpdateAreaHint();
                InvalidateDownloadedRequest();
            };

            // Bounding Box controls
            numNorth.ValueChanged += (_, _) => InvalidateDownloadedRequest();
            numSouth.ValueChanged += (_, _) => InvalidateDownloadedRequest();
            numEast.ValueChanged += (_, _) => InvalidateDownloadedRequest();
            numWest.ValueChanged += (_, _) => InvalidateDownloadedRequest();

            numZoomLevel.ValueChanged += (_, _) =>
            {
                InvalidateDownloadedRequest();
                if (IsLiveMode)
                    SuggestLayerName(BuildLiveLayerName(SelectedTileSource.Name, GetEffectiveZoomLevel(SelectedTileSource)));
            };

            numCenterLat.KeyDown += NumCenterLatLon_KeyDown;
            numCenterLon.KeyDown += NumCenterLatLon_KeyDown;
        }

        // ── Form events ──────────────────────────────────────────────────────────

        private void frmXyzTileImportOptions_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Auto-close triggered by CompleteImportAndClose — state already saved.
            if (_closingAfterImport)
                return;

            // Block accidental close while import is running.
            if (_isImportInProgress)
            {
                ShowValidationMessage("Import is in progress. Please wait until it completes.");
                e.Cancel = true;
                return;
            }

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
                _downloadCancellation.Cancel();

            e.Cancel = true;
        }

        // ── Misc helpers ─────────────────────────────────────────────────────────

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

        private static bool HasUsableTileTokens(string urlTemplate)
        {
            return ContainsTileToken(urlTemplate, "quadkey") ||
                   (ContainsTileToken(urlTemplate, "z") &&
                    ContainsTileToken(urlTemplate, "x") &&
                    ContainsTileToken(urlTemplate, "y"));
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

        private int GetEffectiveZoomLevel(XyzTileSourceCatalogItem source)
        {
            return IsLiveMode
                ? source.MaxZoom
                : decimal.ToInt32(numZoomLevel.Value);
        }

        private void UpdateZoomControlState()
        {
            bool enableZoomPicker = !IsLiveMode && !_isImportInProgress && _downloadCancellation == null;
            numZoomLevel.Enabled = enableZoomPicker;
            lblZoomLevel.Enabled = enableZoomPicker;
            lblZoomLevel.Text = IsLiveMode
                ? $"Zoom range: {SelectedTileSource.MinZoom}-{SelectedTileSource.MaxZoom}"
                : "Zoom level:";
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

        private static string FormatDownloadProgressStatus(XyzTileDownloadProgress progress)
        {
            return
                $"{progress.Percent}% " +
                $"({progress.CompletedTiles:N0}/{progress.TotalTiles:N0} tiles)" +
                $" - {progress.Status}";
        }

        private void RaiseOptionsStateChanged()
        {
            OptionsStateChanged?.Invoke(
                this,
                new XyzTileImportOptionsStateChangedEventArgs(GetCurrentOptionsState()));
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
                _downloadCancellation?.Cancel();
        }

        private bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   DesignMode ||
                   string.IsNullOrWhiteSpace(_projectFolderPath);
        }

        private static decimal ClampNumericValue(NumericUpDown input, decimal value)
        {
            return Math.Min(input.Maximum, Math.Max(input.Minimum, value));
        }

        private static void ApplyNullableDouble(NumericUpDown input, double? value)
        {
            if (!value.HasValue || !double.IsFinite(value.Value))
                return;

            input.Value = ClampNumericValue(input, (decimal)value.Value);
        }

        private void frmXyzTileImportOptions_Load(object sender, EventArgs e)
        {

        }

        // ── Lat/Lon smart paste ──────────────────────────────────────────────────

        /// <summary>
        /// When the user pastes "lat,lon" (e.g. "27.7172, 85.3240") into either
        /// the latitude or longitude NUD, both fields are populated automatically.
        /// A plain number falls through to the default NumericUpDown paste behaviour.
        /// </summary>
        private void NumCenterLatLon_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.V)
                return;

            string clipboard = Clipboard.GetText();
            if (!TrySplitLatLon(clipboard, out decimal lat, out decimal lon))
                return;

            numCenterLat.Value = ClampNumericValue(numCenterLat, lat);
            numCenterLon.Value = ClampNumericValue(numCenterLon, lon);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private static bool TrySplitLatLon(string text, out decimal lat, out decimal lon)
        {
            lat = 0;
            lon = 0;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string[] parts = text.Trim().Split(',');
            if (parts.Length != 2)
                return false;

            return decimal.TryParse(
                       parts[0].Trim(),
                       System.Globalization.NumberStyles.Float,
                       System.Globalization.CultureInfo.InvariantCulture,
                       out lat) &&
                   decimal.TryParse(
                       parts[1].Trim(),
                       System.Globalization.NumberStyles.Float,
                       System.Globalization.CultureInfo.InvariantCulture,
                       out lon);
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
        public XyzTileImportOptionsStateChangedEventArgs(XyzTileImportOptionsState state)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public XyzTileImportOptionsState State { get; }
    }
}
