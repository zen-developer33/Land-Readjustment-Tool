using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Repositories.Spatial;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.UI.Helpers;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Project settings dialog.
    /// User configures CRS, datum transformation,
    /// canvas, parcel numbering, document and print options.
    /// </summary>
    public partial class frmProjectSettings : Form
    {
        private enum CanvasThemeMode
        {
            Dark = 0,
            Light = 1,
            Custom = 2
        }

        // AutoCAD-like dark defaults (from current old renderer feel)
        private static readonly Color DarkThemeBackground = Color.FromArgb(34, 41, 51);
        private static readonly Color DarkThemeGrid = Color.FromArgb(42, 58, 71);

        // ArcGIS-like light defaults (soft paper tint, visible but subtle grid)
        private static readonly Color LightThemeBackground = Color.White;
        private static readonly Color LightThemeGrid = Color.FromArgb(236, 236, 236);

        private readonly IProjectSettingsService _service;  //These are initialized through the frmProjectSettings contructor only. They cannot be changed.
        private readonly ICoordinateSystemRepository _crsRepo;
        private readonly IDatumTransformationRepository _datumRepo;

        private ProjectSettings? _settings;
        private List<CoordinateSystem> _crsList = [];
        private List<DatumTransformation> _datumList = [];

        // Guard flag: prevents cmbCRS_SelectedIndexChanged from firing
        // while we are programmatically rebinding the dropdown.
        private bool _bindingCrs = false;
        private bool _bindingDatum = false;
        private bool _suppressCanvasThemeEvents = false;

        public frmProjectSettings(
            IProjectSettingsService service,
            ICoordinateSystemRepository crsRepo,
            IDatumTransformationRepository datumRepo)
        {
            InitializeComponent();
            _service = service;
            _crsRepo = crsRepo;
            _datumRepo = datumRepo;
            pnlBgColor.Click += (_, _) => btnPickColor_Click(btnPickColor, EventArgs.Empty);
            pnlGridColor.Click += (_, _) => btnPickGridColor_Click(btnPickGridColor, EventArgs.Empty);

            // BUG FIX 1: Do NOT use the Format event for DataSource-bound
            // ComboBoxes.  When DataSource is set, WinForms evaluates
            // DisplayMember via reflection.  The Format event fires for
            // items that are already strings (after DisplayMember resolved
            // them), so e.ListItem is a string — never a CoordinateSystem —
            // and the guard `e.ListItem is CoordinateSystem` silently does
            // nothing, leaving the raw ToString() of the object as the
            // display text.  Set DisplayMember correctly instead (below).
        }

        // ── LOAD ─────────────────────────────────────────────────────────────

        private async void frmProjectSettings_Load(object? sender, EventArgs e)
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                SetFormEnabled(false);


                var settingsTask = _service.GetAsync();
                var crsTask = _crsRepo.GetAllActiveAsync();
                var datumTask = _datumRepo.GetAllActiveAsync();

                await Task.WhenAll(settingsTask, crsTask, datumTask);  ///It defines a task that is completed only when all the tasks in the argument is completed.

                _settings = settingsTask.Result;
                _crsList = crsTask.Result;
                _datumList = datumTask.Result;

                if (_settings == null)
                {
                    MessageBox.Show(
                        "Could not load project settings.",
                        "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Close();
                    return;
                }

                // Bind dropdowns BEFORE populating the form so that
                // SelectedValue assignments in PopulateForm succeed.
                BindCrsDropdown(_crsList);
                BindDatumDropdown(_datumList);
                PopulateForm(_settings);
                if (!_settings.IsConfigured)
                    grpDatumTransformation.Visible = false;
                // Trigger initial filter of datum dropdown based on CRS

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
            }
            finally
            {
                SetFormEnabled(true);
            }
        }

        // ── DROPDOWN BINDING ─────────────────────────────────────────────────

        /// <summary>
        /// Rebinds the CRS ComboBox and restores the previously selected item.
        ///
        /// BUG FIX 2: The original code set DataSource = null then DataSource =
        /// items.  Whenever DataSource changes, WinForms fires
        /// SelectedIndexChanged, which called cmbCRS_SelectedIndexChanged, which
        /// called BindDatumDropdown again — causing a cascading re-bind loop and
        /// losing the selected value.  The _bindingCrs guard prevents this.
        ///
        /// BUG FIX 3 (THE MAIN DISPLAY BUG): The original set DisplayMember
        /// AFTER DataSource.  WinForms caches display strings at the moment
        /// DataSource is assigned.  Setting DisplayMember afterwards is too late
        /// — the items have already been rendered using ToString(), which returns
        /// the full qualified type name.  Always set DisplayMember and
        /// ValueMember BEFORE DataSource.
        /// </summary>
        private void BindCrsDropdown(
            List<CoordinateSystem> items,
            int? restoreId = null)
        {
            _bindingCrs = true;
            try
            {
                // CRITICAL: set members BEFORE DataSource
                cmbCRS.DisplayMember = nameof(CoordinateSystem.Name);
                cmbCRS.ValueMember = nameof(CoordinateSystem.Id);
                cmbCRS.DataSource = null;
                cmbCRS.DataSource = items;

                // Restore selection after rebind
                if (restoreId.HasValue)
                    cmbCRS.SelectedValue = restoreId.Value;
            }
            finally
            {
                _bindingCrs = false;
            }
        }

        /// <summary>
        /// Rebinds the Datum ComboBox and restores the previously selected item.
        /// Same ordering fix as BindCrsDropdown.
        /// </summary>
        private void BindDatumDropdown(
            List<DatumTransformation> items,
            int? restoreId = null)
        {
            _bindingDatum = true;
            try
            {
                // CRITICAL: set members BEFORE DataSource
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

        // ── POPULATE FORM ────────────────────────────────────────────────────

        private void PopulateForm(ProjectSettings s)
        {
            // ── COORDINATE SYSTEM ────────────────────
            cmbCRS.SelectedValue = s.CoordinateSystemId!.Value;

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
                ParseColorOrDefault(s.CanvasBackgroundColor, DarkThemeBackground);
            pnlGridColor.BackColor =
                ParseColorOrDefault(s.CanvasGridColor, DarkThemeGrid);
            UpdateCanvasColorLabels();

            _suppressCanvasThemeEvents = true;
            cmbCanvasTheme.SelectedIndex = (int)GetThemeModeFromColors(
                pnlBgColor.BackColor, pnlGridColor.BackColor);
            _suppressCanvasThemeEvents = false;

            chkGridVisible.Checked = s.CanvasGridVisible;
            chkSnapEnabled.Checked = s.SnapEnabled;
            nudSnapTolerance.Value = (decimal)s.SnapTolerancePx;

            // ── PARCEL NUMBERING ─────────────────────
            cmbParcelFormat.SelectedItem = s.ParcelNumberFormat;
            txtParcelPrefix.Text = s.ParcelNumberPrefix ?? "";
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

        // ── COLLECT FORM DATA ────────────────────────────────────────────────

        private void CollectFormData(ProjectSettings s)
        {
            s.CoordinateSystemId =
                cmbCRS.SelectedValue is int crsId ? crsId : null;

            s.DatumTransformationId =
                cmbDatumTransformation.SelectedValue is int datumId
                ? datumId : null;

            s.TraditionalAreaUnit =
                cmbTraditionalUnit.SelectedIndex == 1 ? "BKD" : "RAPD";

            s.CanvasBackgroundColor =
                ColorTranslator.ToHtml(pnlBgColor.BackColor);
            s.CanvasGridColor =
                ColorTranslator.ToHtml(pnlGridColor.BackColor);
            s.CanvasGridVisible = chkGridVisible.Checked;
            s.SnapEnabled = chkSnapEnabled.Checked;
            s.SnapTolerancePx = (double)nudSnapTolerance.Value;

            s.ParcelNumberFormat =
                cmbParcelFormat.SelectedItem?.ToString() ?? "Sequential";
            s.ParcelNumberPrefix =
                string.IsNullOrWhiteSpace(txtParcelPrefix.Text)
                ? null : txtParcelPrefix.Text;
            s.ParcelNumberPadding = (int)nudPadding.Value;

            s.MinPlotAreaSqm = (double)nudMinPlot.Value;

            s.DocumentLanguage =
                cmbLanguage.SelectedItem?.ToString() ?? "English";
            s.DateFormat =
                cmbDateFormat.SelectedItem?.ToString() ?? "AD";

            s.DefaultPaperSize =
                cmbPaperSize.SelectedItem?.ToString() ?? "A3";
            s.DefaultPrintScale = (int)nudPrintScale.Value;

            s.IsConfigured = true;
        }

        // ── CRS CHANGED — FILTER DATUM ───────────────────────────────────────

        private void cmbCRS_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // BUG FIX 4: Skip while we are programmatically rebinding to
            // avoid the re-bind loop described in BindCrsDropdown.
            if (_bindingCrs) return;

            if (cmbCRS.SelectedItem is not CoordinateSystem crs)
            {
                BindDatumDropdown(_datumList);
                return;
            }

            // Filter datum list to those that declare applicability to this
            // CRS code, or those with no filter (applicable to all).
            var filtered = _datumList
                .Where(d =>
                    string.IsNullOrWhiteSpace(d.ApplicableCrsCodes) ||
                    d.ApplicableCrsCodes
                        .Split(',', StringSplitOptions.TrimEntries)
                        .Contains(crs.Code,
                            StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Preserve existing datum selection if it is still in the
            // filtered list, otherwise clear.
            int? currentDatumId =
                cmbDatumTransformation.SelectedValue is int id ? id : null;

            bool keepSelection = currentDatumId.HasValue &&
                filtered.Any(d => d.Id == currentDatumId.Value);

            BindDatumDropdown(
                filtered,
                keepSelection ? currentDatumId : null);

            // MUTM codes require a datum transformation; standard UTM/WGS84
            // codes (which are already referenced to WGS84) do not.
            bool needsDatum = crs.Code.StartsWith(
                "MUTM", StringComparison.OrdinalIgnoreCase);

            grpDatumTransformation.Visible = needsDatum;

            if (!needsDatum)
            {
                // Auto-select WGS84 identity transform for non-MUTM CRS
                var identity = _datumList.FirstOrDefault(
                    d => d.Code.Equals(
                        "WGS84_IDENTITY",
                        StringComparison.OrdinalIgnoreCase));

                if (identity != null)
                    cmbDatumTransformation.SelectedValue = identity.Id;
            }

            UpdateCrsInfoLabel(crs);
        }

        private void UpdateCrsInfoLabel(CoordinateSystem crs)
        {
            lblCrsInfo.Text = crs.EpsgCode.HasValue
                ? $"EPSG: {crs.EpsgCode} — {crs.Description}"
                : $"Custom CRS — {crs.Description}";
        }

        // ── MANAGE BUTTONS ───────────────────────────────────────────────────

        private async void btnManageCRS_Click(
            object? sender, EventArgs e)
        {
            // Remember current selection before opening the child form
            int? selectedId =
                cmbCRS.SelectedValue is int id ? id : null;

            var session = AppServices.Context.Session;
            var repo = new CoordinateSystemRepository(session);
            var projRepo = new ProjectionParametersRepository(session);

            using var frm = new frmManageCoordinateSystems(repo, projRepo);
            frm.ShowDialog();

            // BUG FIX 5: After the child form closes, re-fetch from the
            // repository (items may have been added/deleted/renamed) and
            // rebind.  Pass restoreId so the user's previous selection is
            // restored if it still exists.
            _crsList = await _crsRepo.GetAllActiveAsync();
            BindCrsDropdown(_crsList, selectedId);

            // If the previously selected CRS was deleted, clear info label
            if (cmbCRS.SelectedItem is CoordinateSystem selected)
                UpdateCrsInfoLabel(selected);
            else
                lblCrsInfo.Text = string.Empty;
        }

        private async void btnManageDatum_Click(
            object? sender, EventArgs e)
        {
            // BUG FIX 6: The original had a duplicate local variable
            // declaration for selectedId which would not compile:
            //   var selectedId = ...          ← first declaration
            //   int? selectedId = ...         ← duplicate, compiler error
            // Fixed by using a single declaration.
            int? selectedId =
                cmbDatumTransformation.SelectedValue is int id ? id : null;

            var session = AppServices.Context.Session;
            var repo = new DatumTransformationRepository(session);

            using var frm = new frmManageDatumTransformations(repo);
            frm.ShowDialog();

            _datumList = await _datumRepo.GetAllActiveAsync();

            // Re-apply the CRS filter if a CRS is selected, otherwise
            // show the full list.
            if (cmbCRS.SelectedItem is CoordinateSystem crs)
            {
                var filtered = _datumList
                    .Where(d =>
                        string.IsNullOrWhiteSpace(d.ApplicableCrsCodes) ||
                        d.ApplicableCrsCodes
                            .Split(',', StringSplitOptions.TrimEntries)
                            .Contains(crs.Code,
                                StringComparer.OrdinalIgnoreCase))
                    .ToList();
                BindDatumDropdown(filtered, selectedId);
            }
            else
            {
                BindDatumDropdown(_datumList, selectedId);
            }
        }

        // ── COLOR PICKER ─────────────────────────────────────────────────────

        private void btnPickColor_Click(object? sender, EventArgs e)
        {
            var selected = PickColor(pnlBgColor.BackColor);
            if (!selected.HasValue) return;

            pnlBgColor.BackColor = selected.Value;
            SetCustomCanvasThemeSelection();
            UpdateCanvasColorLabels();
        }

        private void btnPickGridColor_Click(object? sender, EventArgs e)
        {
            var selected = PickColor(pnlGridColor.BackColor);
            if (!selected.HasValue) return;

            pnlGridColor.BackColor = selected.Value;
            SetCustomCanvasThemeSelection();
            UpdateCanvasColorLabels();
        }

        private void cmbCanvasTheme_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressCanvasThemeEvents) return;
            if (cmbCanvasTheme.SelectedIndex == (int)CanvasThemeMode.Custom) return;

            ApplyThemePreset((CanvasThemeMode)cmbCanvasTheme.SelectedIndex);
            UpdateCanvasColorLabels();
        }

        // ── SAVE ─────────────────────────────────────────────────────────────

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            if (_settings == null) return;

            try
            {
                SetFormEnabled(false);


                CollectFormData(_settings);
                await _service.SaveAsync(_settings);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message,
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to save settings.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetFormEnabled(true);

            }
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnRestoreDefaults_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to restore all settings to default values?\n\nUnsaved changes in this window will be lost.",
                "Restore Defaults",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            ApplyDefaultValuesToForm();

        }

        // ── HELPERS ──────────────────────────────────────────────────────────

        private void SetFormEnabled(bool enabled)
        {
            btnRestoreDefaults.Enabled = enabled;
            btnOK.Enabled = enabled;
            tabSettings.Enabled = enabled;
        }

        private void ApplyDefaultValuesToForm()
        {
            // Coordinate settings
            _bindingCrs = true;
            cmbCRS.SelectedIndex = -1;
            _bindingCrs = false;
            lblCrsInfo.Text = string.Empty;
            BindDatumDropdown(_datumList);
            cmbDatumTransformation.SelectedIndex = -1;
            grpDatumTransformation.Visible = false;

            // Area settings
            cmbTraditionalUnit.SelectedIndex = 0; // RAPD

            // Canvas settings
            _suppressCanvasThemeEvents = true;
            cmbCanvasTheme.SelectedIndex = (int)CanvasThemeMode.Dark;
            _suppressCanvasThemeEvents = false;
            ApplyThemePreset(CanvasThemeMode.Dark);
            UpdateCanvasColorLabels();
            chkGridVisible.Checked = true;
            chkSnapEnabled.Checked = true;
            nudSnapTolerance.Value = ClampToRange(nudSnapTolerance, 8m);

            // Parcel settings
            cmbParcelFormat.SelectedItem = "Sequential";
            txtParcelPrefix.Text = string.Empty;
            nudPadding.Value = ClampToRange(nudPadding, 3m);
            nudMinPlot.Value = ClampToRange(nudMinPlot, 79.49m);

            // Document settings
            cmbLanguage.SelectedItem = "English";
            cmbDateFormat.SelectedItem = "AD";

            // Print settings
            cmbPaperSize.SelectedItem = "A3";
            nudPrintScale.Value = ClampToRange(nudPrintScale, 500m);
        }

        private static CanvasThemeMode GetThemeModeFromColors(
            Color background,
            Color grid)
        {
            if (background.ToArgb() == DarkThemeBackground.ToArgb() &&
                grid.ToArgb() == DarkThemeGrid.ToArgb())
            {
                return CanvasThemeMode.Dark;
            }

            if (background.ToArgb() == LightThemeBackground.ToArgb() &&
                grid.ToArgb() == LightThemeGrid.ToArgb())
            {
                return CanvasThemeMode.Light;
            }

            return CanvasThemeMode.Custom;
        }

        private void ApplyThemePreset(CanvasThemeMode mode)
        {
            switch (mode)
            {
                case CanvasThemeMode.Dark:
                    pnlBgColor.BackColor = DarkThemeBackground;
                    pnlGridColor.BackColor = DarkThemeGrid;
                    break;
                case CanvasThemeMode.Light:
                    pnlBgColor.BackColor = LightThemeBackground;
                    pnlGridColor.BackColor = LightThemeGrid;
                    break;
                default:
                    break;
            }
        }

        private void SetCustomCanvasThemeSelection()
        {
            _suppressCanvasThemeEvents = true;
            cmbCanvasTheme.SelectedIndex = (int)CanvasThemeMode.Custom;
            _suppressCanvasThemeEvents = false;
        }

        private static Color ParseColorOrDefault(string? colorText, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(colorText))
                return fallback;

            try
            {
                return ColorTranslator.FromHtml(colorText);
            }
            catch
            {
                return fallback;
            }
        }

        private static Color? PickColor(Color current)
        {
            using ColorDialog cd = new()
            {
                Color = current,
                FullOpen = true
            };

            ColorDialogCustomColorsStore.LoadInto(cd);
            var result = cd.ShowDialog();
            ColorDialogCustomColorsStore.SaveFrom(cd);

            return result == DialogResult.OK ? cd.Color : null;
        }

        private void UpdateCanvasColorLabels()
        {
            lblBgColorHex.Text = ColorTranslator.ToHtml(pnlBgColor.BackColor);
            lblGridColorHex.Text = ColorTranslator.ToHtml(pnlGridColor.BackColor);
        }

        private static decimal ClampToRange(NumericUpDown input, decimal value)
        {
            if (value < input.Minimum) return input.Minimum;
            if (value > input.Maximum) return input.Maximum;
            return value;
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        {

        }

        private void grpCanvasTheme_Enter(object sender, EventArgs e)
        {

        }
    }
}
