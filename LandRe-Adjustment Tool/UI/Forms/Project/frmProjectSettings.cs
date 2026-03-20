using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Repositories.Spatial;
using Land_Readjustment_Tool.Services.Project;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Project settings dialog.
    /// User configures CRS, datum transformation,
    /// canvas, parcel numbering, document and print options.
    /// </summary>
    public partial class frmProjectSettings : Form
    {
        private readonly IProjectSettingsService _service;
        private readonly ICoordinateSystemRepository _crsRepo;
        private readonly IDatumTransformationRepository _datumRepo;

        private ProjectSettings? _settings;
        private List<CoordinateSystem> _crsList = [];
        private List<DatumTransformation> _datumList = [];

        /// <summary>
        /// Creates settings form with required services.
        /// Called from frmMain.OpenProjectSettings().
        /// </summary>
        public frmProjectSettings(
            IProjectSettingsService service,
            ICoordinateSystemRepository crsRepo,
            IDatumTransformationRepository datumRepo)
        {
            InitializeComponent();
            _service = service;
            _crsRepo = crsRepo;
            _datumRepo = datumRepo;

            cmbCRS.Format += cmbCRS_Format;
            cmbDatumTransformation.Format +=
                cmbDatumTransformation_Format;
        }

        // ── LOAD ─────────────────────────────────────

        private async void frmProjectSettings_Load(
            object? sender, EventArgs e)
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                SetFormEnabled(false);
                lblStatus.Text = "Loading settings...";

                // Load settings and reference data in parallel
                var settingsTask = _service.GetAsync();
                var crsTask = _crsRepo.GetAllActiveAsync();
                var datumTask = _datumRepo.GetAllActiveAsync();

                await Task.WhenAll(
                    settingsTask, crsTask, datumTask);

                _settings = settingsTask.Result;
                _crsList = crsTask.Result;
                _datumList = datumTask.Result;

                if (_settings == null)
                {
                    MessageBox.Show(
                        "Could not load project settings.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                PopulateCrsDropdown();
                PopulateDatumDropdown();
                PopulateForm(_settings);

                lblStatus.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load settings: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
            finally
            {
                SetFormEnabled(true);
            }
        }

        // ── DROPDOWNS ────────────────────────────────

        private void PopulateCrsDropdown()
        {
            BindCrsDropdown(_crsList);
        }

        private void BindCrsDropdown(
            List<CoordinateSystem> items)
        {
            cmbCRS.DisplayMember = nameof(CoordinateSystem.Name);
            cmbCRS.ValueMember = nameof(CoordinateSystem.Id);
            cmbCRS.DataSource = null;
            cmbCRS.DataSource = items;
        }

        private void PopulateDatumDropdown()
        {
            BindDatumDropdown(_datumList);
        }

        private void BindDatumDropdown(
            List<DatumTransformation> items)
        {
            cmbDatumTransformation.DisplayMember = "Name";
            cmbDatumTransformation.ValueMember = "Id";
            cmbDatumTransformation.DataSource = null;
            cmbDatumTransformation.DataSource = items;
        }

        private void cmbCRS_Format(
            object? sender,
            ListControlConvertEventArgs e)
        {
            if (e.ListItem is CoordinateSystem crs)
                e.Value = string.IsNullOrWhiteSpace(crs.Name)
                    ? crs.Code
                    : crs.Name;
        }

        private void cmbDatumTransformation_Format(
            object? sender,
            ListControlConvertEventArgs e)
        {
            if (e.ListItem is DatumTransformation datum)
                e.Value = string.IsNullOrWhiteSpace(datum.Name)
                    ? datum.Code
                    : datum.Name;
        }

        // ── POPULATE FORM ────────────────────────────

        private void PopulateForm(ProjectSettings s)
        {
            // ── COORDINATE SYSTEM ────────────────────
            if (s.CoordinateSystemId.HasValue)
                cmbCRS.SelectedValue = s.CoordinateSystemId.Value;
            else
                cmbCRS.SelectedIndex = -1;

            // ── DATUM TRANSFORMATION ─────────────────
            if (s.DatumTransformationId.HasValue)
                cmbDatumTransformation.SelectedValue =
                    s.DatumTransformationId.Value;
            else
                cmbDatumTransformation.SelectedIndex = -1;

            // ── AREA UNIT ────────────────────────────
            cmbTraditionalUnit.SelectedIndex =
                s.TraditionalAreaUnit == "BKD" ? 1 : 0;

            // ── CANVAS ───────────────────────────────
            pnlBgColor.BackColor =
                ColorTranslator.FromHtml(
                    s.CanvasBackgroundColor);
            chkGridVisible.Checked = s.CanvasGridVisible;
            chkSnapEnabled.Checked = s.SnapEnabled;
            nudSnapTolerance.Value =
                (decimal)s.SnapTolerancePx;

            // ── PARCEL NUMBERING ─────────────────────
            cmbParcelFormat.SelectedItem =
                s.ParcelNumberFormat;
            txtParcelPrefix.Text =
                s.ParcelNumberPrefix ?? "";
            nudPadding.Value = s.ParcelNumberPadding;

            // ── REPLOTTING ───────────────────────────
            nudMinPlot.Value = (decimal)s.MinPlotAreaSqm;

            // ── DOCUMENT ─────────────────────────────
            cmbLanguage.SelectedItem = s.DocumentLanguage;
            cmbDateFormat.SelectedItem = s.DateFormat;

            // ── PRINT ────────────────────────────────
            cmbPaperSize.SelectedItem = s.DefaultPaperSize;
            nudPrintScale.Value = s.DefaultPrintScale;
        }

        // ── COLLECT FORM DATA ────────────────────────

        private void CollectFormData(ProjectSettings s)
        {
            // CRS selection
            s.CoordinateSystemId =
                cmbCRS.SelectedValue is int crsId
                ? crsId : null;

            // Datum transformation selection
            s.DatumTransformationId =
                cmbDatumTransformation.SelectedValue is int datumId
                ? datumId : null;

            // Area unit
            s.TraditionalAreaUnit =
                cmbTraditionalUnit.SelectedIndex == 1
                ? "BKD" : "RAPD";

            // Canvas
            s.CanvasBackgroundColor =
                ColorTranslator.ToHtml(pnlBgColor.BackColor);
            s.CanvasGridVisible = chkGridVisible.Checked;
            s.SnapEnabled = chkSnapEnabled.Checked;
            s.SnapTolerancePx = (double)nudSnapTolerance.Value;

            // Parcel numbering
            s.ParcelNumberFormat =
                cmbParcelFormat.SelectedItem?.ToString()
                ?? "Sequential";
            s.ParcelNumberPrefix =
                string.IsNullOrWhiteSpace(txtParcelPrefix.Text)
                ? null : txtParcelPrefix.Text;
            s.ParcelNumberPadding = (int)nudPadding.Value;

            // Replotting
            s.MinPlotAreaSqm = (double)nudMinPlot.Value;

            // Document
            s.DocumentLanguage =
                cmbLanguage.SelectedItem?.ToString()
                ?? "English";
            s.DateFormat =
                cmbDateFormat.SelectedItem?.ToString()
                ?? "AD";

            // Print
            s.DefaultPaperSize =
                cmbPaperSize.SelectedItem?.ToString()
                ?? "A3";
            s.DefaultPrintScale = (int)nudPrintScale.Value;

            // Mark as configured
            s.IsConfigured = true;
        }

        // ── CRS CHANGED — FILTER DATUM ───────────────

        private async void cmbCRS_SelectedIndexChanged(
            object? sender, EventArgs e)
        {
            // When CRS changes, filter datum transformations
            // to only show applicable ones
            if (cmbCRS.SelectedItem is not CoordinateSystem crs)
            {
                BindDatumDropdown(_datumList);
                return;
            }

            // Filter by applicable CRS codes
            var filtered = _datumList
                .Where(d =>
                    d.ApplicableCrsCodes == null ||
                    d.ApplicableCrsCodes.Contains(crs.Code))
                .ToList();

            BindDatumDropdown(filtered);

            // Show/hide datum section based on CRS type
            // MUTM needs datum transformation, UTM/WGS84 don't
            bool needsDatum =
                crs.Code.StartsWith("MUTM");
            grpDatumTransformation.Visible = needsDatum;

            if (!needsDatum)
            {
                // Auto-select WGS84 identity for non-MUTM
                var identity = _datumList
                    .FirstOrDefault(d =>
                        d.Code == "WGS84_IDENTITY");
                if (identity != null)
                    cmbDatumTransformation.SelectedValue =
                        identity.Id;
            }

            // Show info label about selected CRS
            UpdateCrsInfoLabel(crs);

            await Task.CompletedTask;
        }

        private void UpdateCrsInfoLabel(CoordinateSystem crs)
        {
            lblCrsInfo.Text = crs.EpsgCode.HasValue
                ? $"EPSG: {crs.EpsgCode} — {crs.Description}"
                : $"Custom CRS — {crs.Description}";
        }

        // ── MANAGE BUTTONS ───────────────────────────

        private async void btnManageCRS_Click(
            object? sender, EventArgs e)
        {
            var session = AppServices.Context.Session;
            var repo = new CoordinateSystemRepository(session);
            var projRepo = new ProjectionParametersRepository(
                session);

            using var frm = new frmManageCoordinateSystems(
                repo, projRepo);
            frm.ShowDialog();

            // Refresh dropdown after managing
            _crsList = await _crsRepo.GetAllActiveAsync();
            int? selectedId =
                cmbCRS.SelectedValue is int id
                    ? id
                    : (cmbCRS.SelectedItem as CoordinateSystem)?.Id;
            PopulateCrsDropdown();
            if (selectedId.HasValue)
                cmbCRS.SelectedValue = selectedId.Value;
        }

        private async void btnManageDatum_Click(
            object? sender, EventArgs e)
        {
            var session = AppServices.Context.Session;
            var repo = new DatumTransformationRepository(
                session);

            using var frm = new frmManageDatumTransformations(
                repo);
            frm.ShowDialog();

            // Refresh dropdown after managing
            _datumList = await _datumRepo.GetAllActiveAsync();
            int? selectedId =
                cmbDatumTransformation.SelectedValue is int id
                    ? id
                    : (cmbDatumTransformation.SelectedItem
                        as DatumTransformation)?.Id;
            PopulateDatumDropdown();
            if (selectedId.HasValue)
                cmbDatumTransformation.SelectedValue =
                    selectedId.Value;
        }

        // ── COLOR PICKER ─────────────────────────────

        private void btnPickColor_Click(
            object? sender, EventArgs e)
        {
            using ColorDialog cd = new()
            {
                Color = pnlBgColor.BackColor,
                FullOpen = true
            };
            if (cd.ShowDialog() == DialogResult.OK)
                pnlBgColor.BackColor = cd.Color;
        }

        // ── SAVE ─────────────────────────────────────

        private async void btnOK_Click(
            object? sender, EventArgs e)
        {
            if (_settings == null) return;

            try
            {
                SetFormEnabled(false);
                lblStatus.Text = "Saving...";

                CollectFormData(_settings);
                await _service.SaveAsync(_settings);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Failed to save settings.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetFormEnabled(true);
                lblStatus.Text = "Ready";
            }
        }

        private void btnCancel_Click(
            object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ── HELPERS ──────────────────────────────────

        private void SetFormEnabled(bool enabled)
        {
            btnOK.Enabled = enabled;
            tabSettings.Enabled = enabled;
        }
    }
}
