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
    ///
    /// TABS:
    ///   Coordinate System — CRS + Datum Transformation
    ///   Area Units        — Traditional unit selection
    ///   Canvas            — Background color, grid, snap
    ///   Parcels           — Numbering format, replot rules
    ///   Documents         — Language, date format
    ///   Print             — Paper size, default scale
    ///
    /// DATUM VISIBILITY RULE:
    ///   MUTM codes (MUTM81/82/83) → show datum group
    ///   All other CRS (UTM, WGS84) → hide datum group
    ///   UTM zones are already WGS84-referenced.
    ///   Only Modified UTM (Everest ellipsoid) needs
    ///   a datum transformation to WGS84.
    ///
    /// STAGING PATTERN:
    ///   btnSave_Click → CollectFormData → _service.SaveAsync
    ///   → repository UpdateAsync → staged in EF Core
    ///   → NOT committed until frmMain Ctrl+S
    /// </summary>
    public partial class frmProjectSettings : Form
    {
        // ── DEPENDENCIES ─────────────────────────────
        private readonly IProjectSettingsService _service;
        private readonly ICoordinateSystemRepository _crsRepo;
        private readonly IDatumTransformationRepository _datumRepo;

        // ── STATE ────────────────────────────────────
        private ProjectSettings? _settings;
        private List<CoordinateSystem> _crsList = [];
        private List<DatumTransformation> _datumList = [];

        // Guards — prevent cascading SelectedIndexChanged
        // events while we are programmatically binding dropdowns
        private bool _bindingCrs = false;
        private bool _bindingDatum = false;

        // ── CONSTRUCTOR ──────────────────────────────

        public frmProjectSettings(
            IProjectSettingsService service,
            ICoordinateSystemRepository crsRepo,
            IDatumTransformationRepository datumRepo)
        {
            InitializeComponent();

            // Store dependencies — must happen BEFORE Load
            _service = service;
            _crsRepo = crsRepo;
            _datumRepo = datumRepo;
        }

        // ── LOAD ─────────────────────────────────────

        private async void frmProjectSettings_Load(
            object? sender, EventArgs e)
        {
            await LoadAsync();
        }

        /// <summary>
        /// Loads settings, CRS list and datum list in parallel.
        /// Binds dropdowns BEFORE PopulateForm so SelectedValue
        /// assignments succeed.
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
                SetFormEnabled(false);
                lblStatus.Text = "Loading...";

                // Load all three in parallel
                var settingsTask = _service.GetAsync();
                var crsTask = _crsRepo.GetAllActiveAsync();
                var datumTask = _datumRepo.GetAllActiveAsync();
                await Task.WhenAll(settingsTask, crsTask, datumTask);

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
                    Close();
                    return;
                }

                // Bind dropdowns FIRST — then populate
                BindCrsDropdown(_crsList);
                BindDatumDropdown(_datumList);
                PopulateForm(_settings);

                lblStatus.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load settings:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
            }
            finally
            {
                SetFormEnabled(true);
            }
        }

        // ── DROPDOWN BINDING ─────────────────────────

        /// <summary>
        /// Binds the CRS combo box.
        ///
        /// CRITICAL — set DisplayMember and ValueMember
        /// BEFORE DataSource. WinForms caches the display
        /// strings at the moment DataSource is assigned.
        /// Setting DisplayMember afterwards shows the full
        /// type name instead of the property value.
        ///
        /// Guard _bindingCrs prevents cmbCRS_SelectedIndexChanged
        /// from firing and triggering a re-bind loop while we
        /// are already binding.
        /// </summary>
        private void BindCrsDropdown(
            List<CoordinateSystem> items,
            int? restoreId = null)
        {
            _bindingCrs = true;
            try
            {
                cmbCRS.DisplayMember = nameof(CoordinateSystem.Name);
                cmbCRS.ValueMember = nameof(CoordinateSystem.Id);
                cmbCRS.DataSource = null;
                cmbCRS.DataSource = items;

                if (restoreId.HasValue)
                    cmbCRS.SelectedValue = restoreId.Value;
            }
            finally
            {
                _bindingCrs = false;
            }
        }

        /// <summary>
        /// Binds the Datum combo box.
        /// Same DisplayMember-before-DataSource rule applies.
        /// </summary>
        private void BindDatumDropdown(
            List<DatumTransformation> items,
            int? restoreId = null)
        {
            _bindingDatum = true;
            try
            {
                cmbDatumTransformation.DisplayMember =
                    nameof(DatumTransformation.Name);
                cmbDatumTransformation.ValueMember =
                    nameof(DatumTransformation.Id);
                cmbDatumTransformation.DataSource = null;
                cmbDatumTransformation.DataSource = items;

                if (restoreId.HasValue)
                    cmbDatumTransformation.SelectedValue = restoreId.Value;
            }
            finally
            {
                _bindingDatum = false;
            }
        }

        // ── POPULATE FORM ────────────────────────────

        /// <summary>
        /// Fills all form controls from the settings entity.
        /// Must be called AFTER BindCrsDropdown and
        /// BindDatumDropdown so SelectedValue works.
        /// </summary>
        private void PopulateForm(ProjectSettings s)
        {
            // ── COORDINATE SYSTEM ────────────────────
            if (s.CoordinateSystemId.HasValue)
                cmbCRS.SelectedValue = s.CoordinateSystemId.Value;
            else
                cmbCRS.SelectedIndex = -1;

            // Apply datum visibility based on selected CRS
            ApplyDatumVisibility(cmbCRS.SelectedItem
                as CoordinateSystem);

            // ── DATUM TRANSFORMATION ─────────────────
            if (s.DatumTransformationId.HasValue)
                cmbDatumTransformation.SelectedValue =
                    s.DatumTransformationId.Value;
            else
                cmbDatumTransformation.SelectedIndex = -1;

            // ── AREA UNIT ────────────────────────────
            // 0 = ROPANI/AANA/PAISA/DAAM  1 = BIGHA/KATTHA/DHUR
            cmbTraditionalUnit.SelectedIndex =
                s.TraditionalAreaUnit == "BKD" ? 1 : 0;

            // ── CANVAS ───────────────────────────────
            try
            {
                pnlBgColor.BackColor =
                    ColorTranslator.FromHtml(s.CanvasBackgroundColor);
            }
            catch
            {
                pnlBgColor.BackColor = Color.FromArgb(0x1E, 0x29, 0x33);
            }
            lblBgColorHex.Text =
                ColorTranslator.ToHtml(pnlBgColor.BackColor);
            chkGridVisible.Checked = s.CanvasGridVisible;
            chkSnapEnabled.Checked = s.SnapEnabled;
            nudSnapTolerance.Value =
                Math.Max(nudSnapTolerance.Minimum,
                Math.Min(nudSnapTolerance.Maximum,
                    (decimal)s.SnapTolerancePx));

            // ── PARCEL NUMBERING ─────────────────────
            if (!string.IsNullOrEmpty(s.ParcelNumberFormat))
            {
                int idx = cmbParcelFormat.Items.IndexOf(
                    s.ParcelNumberFormat);
                if (idx >= 0) cmbParcelFormat.SelectedIndex = idx;
            }
            txtParcelPrefix.Text = s.ParcelNumberPrefix ?? "";
            nudPadding.Value =
                Math.Max(nudPadding.Minimum,
                Math.Min(nudPadding.Maximum,
                    (decimal)s.ParcelNumberPadding));

            // ── REPLOTTING ───────────────────────────
            nudMinPlot.Value =
                Math.Max(nudMinPlot.Minimum,
                Math.Min(nudMinPlot.Maximum,
                    (decimal)s.MinPlotAreaSqm));

            // ── DOCUMENT ─────────────────────────────
            if (!string.IsNullOrEmpty(s.DocumentLanguage))
            {
                int idx = cmbLanguage.Items.IndexOf(
                    s.DocumentLanguage);
                if (idx >= 0) cmbLanguage.SelectedIndex = idx;
            }
            if (!string.IsNullOrEmpty(s.DateFormat))
            {
                int idx = cmbDateFormat.Items.IndexOf(s.DateFormat);
                if (idx >= 0) cmbDateFormat.SelectedIndex = idx;
            }

            // ── PRINT ────────────────────────────────
            if (!string.IsNullOrEmpty(s.DefaultPaperSize))
            {
                int idx = cmbPaperSize.Items.IndexOf(
                    s.DefaultPaperSize);
                if (idx >= 0) cmbPaperSize.SelectedIndex = idx;
            }
            nudPrintScale.Value =
                Math.Max(nudPrintScale.Minimum,
                Math.Min(nudPrintScale.Maximum,
                    (decimal)s.DefaultPrintScale));
        }

        // ── COLLECT FORM DATA ────────────────────────

        /// <summary>
        /// Reads all form controls back into the settings entity.
        /// Called by btnSave_Click before staging.
        /// </summary>
        private void CollectFormData(ProjectSettings s)
        {
            // ── COORDINATE SYSTEM ────────────────────
            s.CoordinateSystemId =
                cmbCRS.SelectedValue is int crsId
                ? crsId : null;

            // Only store datum if group is visible (MUTM)
            s.DatumTransformationId =
                grpDatumTransformation.Visible &&
                cmbDatumTransformation.SelectedValue is int datumId
                ? datumId : null;

            // ── AREA UNIT ────────────────────────────
            s.TraditionalAreaUnit =
                cmbTraditionalUnit.SelectedIndex == 1
                ? "BKD" : "RAPD";

            // ── CANVAS ───────────────────────────────
            s.CanvasBackgroundColor =
                ColorTranslator.ToHtml(pnlBgColor.BackColor);
            s.CanvasGridVisible = chkGridVisible.Checked;
            s.SnapEnabled = chkSnapEnabled.Checked;
            s.SnapTolerancePx = (double)nudSnapTolerance.Value;

            // ── PARCEL NUMBERING ─────────────────────
            s.ParcelNumberFormat =
                cmbParcelFormat.SelectedItem?.ToString()
                ?? "Sequential";
            s.ParcelNumberPrefix =
                string.IsNullOrWhiteSpace(txtParcelPrefix.Text)
                ? null : txtParcelPrefix.Text.Trim();
            s.ParcelNumberPadding = (int)nudPadding.Value;

            // ── REPLOTTING ───────────────────────────
            s.MinPlotAreaSqm = (double)nudMinPlot.Value;

            // ── DOCUMENT ─────────────────────────────
            s.DocumentLanguage =
                cmbLanguage.SelectedItem?.ToString() ?? "English";
            s.DateFormat =
                cmbDateFormat.SelectedItem?.ToString() ?? "AD";

            // ── PRINT ────────────────────────────────
            s.DefaultPaperSize =
                cmbPaperSize.SelectedItem?.ToString() ?? "A3";
            s.DefaultPrintScale = (int)nudPrintScale.Value;

            s.IsConfigured = true;
        }

        // ── CRS SELECTION CHANGED ────────────────────

        /// <summary>
        /// Fired when user selects a different CRS.
        /// Filters datum list to applicable entries.
        /// Shows/hides datum group based on CRS type.
        ///
        /// MUTM (Modified UTM / Everest ellipsoid):
        ///   → show datum group (needs Everest→WGS84 transform)
        ///
        /// Standard UTM / WGS84:
        ///   → hide datum group (already WGS84-referenced)
        /// </summary>
        private void cmbCRS_SelectedIndexChanged(
            object? sender, EventArgs e)
        {
            // Skip while programmatically binding
            if (_bindingCrs) return;

            var crs = cmbCRS.SelectedItem as CoordinateSystem;

            if (crs == null)
            {
                BindDatumDropdown(_datumList);
                grpDatumTransformation.Visible = false;
                lblCrsInfo.Text = string.Empty;
                return;
            }

            // Filter datum list to entries applicable to this CRS
            int? currentDatumId =
                cmbDatumTransformation.SelectedValue is int id
                ? id : null;

            var filtered = FilterDatumList(crs);

            bool keepSelection = currentDatumId.HasValue &&
                filtered.Any(d => d.Id == currentDatumId.Value);

            BindDatumDropdown(
                filtered,
                keepSelection ? currentDatumId : null);

            // Show/hide datum group
            ApplyDatumVisibility(crs);

            // Update info label
            UpdateCrsInfoLabel(crs);
        }

        /// <summary>
        /// Shows datum group only for MUTM codes.
        /// MUTM81, MUTM82, MUTM83 use Everest ellipsoid
        /// and need a datum transformation to WGS84.
        /// Standard UTM is already WGS84-referenced.
        /// </summary>
        private void ApplyDatumVisibility(CoordinateSystem? crs)
        {
            if (crs == null)
            {
                grpDatumTransformation.Visible = false;
                return;
            }

            bool isMUTM = crs.Code.StartsWith(
                "MUTM", StringComparison.OrdinalIgnoreCase);

            grpDatumTransformation.Visible = isMUTM;

            // If UTM — clear datum selection
            if (!isMUTM)
                cmbDatumTransformation.SelectedIndex = -1;
        }

        private List<DatumTransformation> FilterDatumList(
            CoordinateSystem crs)
        {
            return _datumList
                .Where(d =>
                    string.IsNullOrWhiteSpace(
                        d.ApplicableCrsCodes) ||
                    d.ApplicableCrsCodes
                        .Split(',',
                            StringSplitOptions.TrimEntries |
                            StringSplitOptions.RemoveEmptyEntries)
                        .Contains(crs.Code,
                            StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private void UpdateCrsInfoLabel(CoordinateSystem crs)
        {
            lblCrsInfo.Text = crs.EpsgCode.HasValue
                ? $"EPSG: {crs.EpsgCode} — {crs.Description}"
                : $"Custom CRS — {crs.Description}";
        }

        // ── MANAGE BUTTONS ───────────────────────────

        /// <summary>
        /// Opens manage CRS form.
        /// After close — refreshes CRS list from repo
        /// (merged DB + staged local cache) and rebinds.
        /// </summary>
        private async void btnManageCRS_Click(
            object? sender, EventArgs e)
        {
            int? selectedId =
                cmbCRS.SelectedValue is int id ? id : null;

            var session = AppServices.Context.Session;
            var repo = new CoordinateSystemRepository(session);
            var projRepo = new ProjectionParametersRepository(session);

            using var frm = new frmManageCoordinateSystems(
                repo, projRepo);
            frm.ShowDialog();

            // Refresh list — picks up staged changes via
            // GetAllActiveAsync which merges DB + Local cache
            _crsList = await _crsRepo.GetAllActiveAsync();
            BindCrsDropdown(_crsList, selectedId);

            if (cmbCRS.SelectedItem is CoordinateSystem selected)
                UpdateCrsInfoLabel(selected);
            else
                lblCrsInfo.Text = string.Empty;
        }

        /// <summary>
        /// Opens manage datum form.
        /// After close — refreshes datum list and rebinds
        /// with CRS filter re-applied.
        /// </summary>
        private async void btnManageDatum_Click(
            object? sender, EventArgs e)
        {
            int? selectedId =
                cmbDatumTransformation.SelectedValue is int id
                ? id : null;

            var session = AppServices.Context.Session;
            var repo = new DatumTransformationRepository(session);

            using var frm = new frmManageDatumTransformations(repo);
            frm.ShowDialog();

            // Refresh list — picks up staged changes
            _datumList = await _datumRepo.GetAllActiveAsync();

            // Re-apply CRS filter if a CRS is selected
            if (cmbCRS.SelectedItem is CoordinateSystem crs)
            {
                var filtered = FilterDatumList(crs);
                bool keepSelection = selectedId.HasValue &&
                    filtered.Any(d => d.Id == selectedId.Value);
                BindDatumDropdown(
                    filtered,
                    keepSelection ? selectedId : null);
            }
            else
            {
                BindDatumDropdown(_datumList, selectedId);
            }
        }

        // ── COLOR PICKER ─────────────────────────────

        private void btnPickColor_Click(
            object? sender, EventArgs e)
        {
            using var cd = new ColorDialog
            {
                Color = pnlBgColor.BackColor,
                FullOpen = true
            };

            if (cd.ShowDialog() == DialogResult.OK)
            {
                pnlBgColor.BackColor = cd.Color;
                lblBgColorHex.Text =
                    ColorTranslator.ToHtml(cd.Color);
            }
        }

        // ── SAVE ─────────────────────────────────────

        /// <summary>
        /// Validates, collects form data into settings entity
        /// and stages via service → repository.
        /// Does NOT call SaveChangesAsync — frmMain commits.
        /// </summary>
        private async void btnSave_Click(
            object? sender, EventArgs e)
        {
            if (_settings == null) return;

            try
            {
                SetFormEnabled(false);
                lblStatus.Text = "Saving...";

                // Collect form → entity
                CollectFormData(_settings);

                // Service validates business rules
                // Repository stages (no SaveChangesAsync)
                await _service.SaveAsync(_settings);

                DialogResult = DialogResult.OK;
                Close();
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
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── HELPERS ──────────────────────────────────

        private void SetFormEnabled(bool enabled)
        {
            btnOK.Enabled = enabled;
            tabSettings.Enabled = enabled;
        }
    }
}