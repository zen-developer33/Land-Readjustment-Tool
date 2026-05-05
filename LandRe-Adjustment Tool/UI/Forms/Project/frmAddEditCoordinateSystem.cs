using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.UI.Helpers;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Add or edit a coordinate reference system (CRS).
    ///
    /// GEODETIC BACKGROUND
    /// ───────────────────
    /// Transverse Mercator (UTM / MUTM) standard defaults
    ///   Central Meridian  : zone-specific (e.g. 84°E for MUTM zone 44)
    ///   Latitude of Origin: 0.0° (equator)
    ///   Scale Factor      : 0.9996  (UTM standard; reduces max distortion to ~1:2500)
    ///   False Easting     : 500 000 m  (keeps all eastings positive)
    ///   False Northing    : 0 m (north hemisphere) / 10 000 000 m (south)
    ///
    /// Reference ellipsoids (semi-major axis / inverse flattening)
    ///   WGS84  : 6 378 137.000 m  /  298.257 223 563  (GPS standard)
    ///   GRS80  : 6 378 137.000 m  /  298.257 222 101  (NAD83, ETRS89)
    ///   Everest: 6 377 276.345 m  /  300.801 700 000  (used in Nepal / India)
    ///
    /// Scale factor range: (0, 1].  Values > 1.0 are physically impossible
    /// for a secant-cylinder projection.  UTM uses 0.9996; never use 1.0
    /// for a named UTM zone (that would be a tangent cylinder).
    ///
    /// Pass null to add new; pass existing entity to edit.
    /// </summary>
    public partial class frmAddEditCoordinateSystem : Form
    {
        private readonly ICoordinateSystemRepository _repo;
        private readonly IProjectionParametersRepository _projRepo;
        private readonly CoordinateSystem? _existing;
        private readonly bool _isNew;

        // WKT placeholder text — must never be saved to the database
        private const string WktPlaceholder =
            "Optional. Overrides all parameters above if provided.";

        // ── Well-known ellipsoid presets ──────────────────────────────────────
        // (semi-major axis metres, inverse flattening)
        private static readonly Dictionary<string, (double A, double InvF)>
            EllipsoidPresets = new(StringComparer.OrdinalIgnoreCase)
            {
                // GPS / modern global standard — WGS84 and GRS80 share the same
                // semi-major axis; their inverse flattenings differ by ~0.000001.
                ["WGS84"] = (6_378_137.000, 298.257_223_563),
                ["GRS80"] = (6_378_137.000, 298.257_222_101),
                // Nepal / Indian subcontinent surveys use Everest 1830
                ["Everest"] = (6_377_276.345, 300.801_700_000),
                // Other common ellipsoids
                ["Bessel"] = (6_377_397.155, 299.152_812_853),
                ["Clarke1866"] = (6_378_206.400, 294.978_698_214),
                ["Clarke1880"] = (6_378_249.145, 293.465_000_000),
                ["Airy1830"] = (6_377_563.396, 299.324_964_600),
                ["Intl1924"] = (6_378_388.000, 297.000_000_000), // Hayford
            };

        public frmAddEditCoordinateSystem(
            CoordinateSystem? existing,
            ICoordinateSystemRepository repo,
            IProjectionParametersRepository projRepo)
        {
            InitializeComponent();
            NumericUpDownSelectAllBehavior.AttachTo(this);
            _repo = repo;
            _projRepo = projRepo;
            _existing = existing;
            _isNew = existing?.Id == 0 || existing == null;
        }

        // ── LOAD ─────────────────────────────────────────────────────────────

        private async void frmAddEditCoordinateSystem_Load(
            object? sender, EventArgs e)
        {
            Text = _isNew
                ? "Add Coordinate System"
                : "Edit Coordinate System";


            ApplyNudBounds();
            SetProjectionDefaults();

            if (_existing != null)
                await PopulateFormAsync(_existing);

            UpdateProjectionVisibility();
        }

        /// <summary>
        /// Sets NumericUpDown bounds to geodetically correct values.
        /// </summary>
        private void ApplyNudBounds()
        {
            // Central Meridian: full longitude range ±180°
            nudCentralMeridian.Minimum = -180m;
            nudCentralMeridian.Maximum = 180m;
            nudCentralMeridian.DecimalPlaces = 6;
            nudCentralMeridian.Increment = 0.000001m;

            // Latitude of Origin: ±90°
            nudLatOrigin.Minimum = -90m;
            nudLatOrigin.Maximum = 90m;
            nudLatOrigin.DecimalPlaces = 6;
            nudLatOrigin.Increment = 0.000001m;

            // Scale Factor: (0, 1].  A secant TM projection must be < 1.
            // We use a practical lower bound of 0.9900 (well below any
            // published national grid).
            nudScaleFactor.Minimum = 0.9900m;
            nudScaleFactor.Maximum = 1.0000m;
            nudScaleFactor.DecimalPlaces = 6;
            nudScaleFactor.Increment = 0.000001m;

            // False Easting / Northing: ±10 000 000 m is generous
            nudFalseEasting.Minimum = -10_000_000m;
            nudFalseEasting.Maximum = 10_000_000m;
            nudFalseEasting.DecimalPlaces = 3;
            nudFalseEasting.Increment = 1m;

            nudFalseNorthing.Minimum = -10_000_000m;
            nudFalseNorthing.Maximum = 10_000_000m;
            nudFalseNorthing.DecimalPlaces = 3;
            nudFalseNorthing.Increment = 1m;

            // Semi-Major Axis: Earth radii range ~6 356 000–6 378 200 m
            nudSemiMajor.Minimum = 6_000_000m;
            nudSemiMajor.Maximum = 6_500_000m;
            nudSemiMajor.DecimalPlaces = 3;
            nudSemiMajor.Increment = 0.001m;

            // Inverse Flattening: all practical ellipsoids are ~290–301
            nudInvFlat.Minimum = 250m;
            nudInvFlat.Maximum = 350m;
            nudInvFlat.DecimalPlaces = 9;   // EPSG publishes 9 dp
            nudInvFlat.Increment = 0.000000001m;
        }

        /// <summary>
        /// Populates projection NUDs with geodetically correct UTM defaults
        /// so a new CRS starts in a sensible state.
        /// </summary>
        private void SetProjectionDefaults()
        {
            // UTM / MUTM standard defaults
            nudCentralMeridian.Value = 0m;        // user must set the zone meridian
            nudLatOrigin.Value = 0m;        // equator
            nudScaleFactor.Value = 0.9996m;   // UTM standard scale factor
            nudFalseEasting.Value = 500_000m;  // keeps eastings positive
            nudFalseNorthing.Value = 0m;        // northern hemisphere

            // WGS84 ellipsoid defaults
            txtEllipsoid.Text = "WGS84";
            nudSemiMajor.Value = 6_378_137m;        // metres
            nudInvFlat.Value = 298.257_223_563m;  // WGS84 inverse flattening
        }

        private async Task PopulateFormAsync(CoordinateSystem crs)
        {
            txtCode.Text = crs.Code;
            txtName.Text = crs.Name;
            txtEpsg.Text = crs.EpsgCode?.ToString() ?? "";
            cmbProjectionType.Text = crs.ProjectionType ?? "";
            txtRegion.Text = crs.Region ?? "";
            txtDescription.Text = crs.Description ?? "";

            if (crs.Id > 0)
            {
                var proj = await _projRepo
                    .GetByCoordinateSystemIdAsync(crs.Id);

                if (proj != null)
                {
                    nudCentralMeridian.Value =
                        Clamp(nudCentralMeridian,
                              (decimal)(proj.CentralMeridian ?? 0));

                    nudLatOrigin.Value =
                        Clamp(nudLatOrigin,
                              (decimal)(proj.LatitudeOfOrigin ?? 0));

                    // Scale factor: stored value should be in (0, 1].
                    // If it was accidentally stored as ppm offset (e.g. -4 instead of
                    // 0.9996), detect and convert automatically.
                    double sf = proj.ScaleFactor ?? 0.9996;
                    if (sf > 1.0 || sf <= 0)
                        sf = 0.9996;  // silently reset nonsense values
                    nudScaleFactor.Value =
                        Clamp(nudScaleFactor, (decimal)sf);

                    nudFalseEasting.Value =
                        Clamp(nudFalseEasting,
                              (decimal)(proj.FalseEasting ?? 500_000));

                    nudFalseNorthing.Value =
                        Clamp(nudFalseNorthing,
                              (decimal)(proj.FalseNorthing ?? 0));

                    txtEllipsoid.Text = proj.Ellipsoid ?? "WGS84";

                    nudSemiMajor.Value =
                        Clamp(nudSemiMajor,
                              (decimal)(proj.SemiMajorAxis ?? 6_378_137.0));

                    nudInvFlat.Value =
                        Clamp(nudInvFlat,
                              (decimal)(proj.InverseFlattening ?? 298.257_223_563));

                    // WKT — only replace placeholder if there is real content
                    if (!string.IsNullOrWhiteSpace(proj.WktDefinition))
                    {
                        txtWkt.Text = proj.WktDefinition;
                        txtWkt.ForeColor = SystemColors.WindowText;
                    }
                }
            }
        }

        private static decimal Clamp(NumericUpDown nud, decimal v)
            => Math.Max(nud.Minimum, Math.Min(nud.Maximum, v));

        // ── ELLIPSOID PRESET LOOKUP ───────────────────────────────────────────

        /// <summary>
        /// When the user types a known ellipsoid name, auto-fills the
        /// semi-major axis and inverse flattening with the official values.
        /// </summary>
        private void txtEllipsoid_Leave(object? sender, EventArgs e)
        {
            string name = txtEllipsoid.Text.Trim();
            if (EllipsoidPresets.TryGetValue(name, out var p))
            {
                nudSemiMajor.Value = Clamp(nudSemiMajor, (decimal)p.A);
                nudInvFlat.Value = Clamp(nudInvFlat, (decimal)p.InvF);
            }
        }

        // ── PROJECTION TYPE CHANGE ───────────────────────────────────────────

        private void cmbProjectionType_SelectedIndexChanged(
            object? sender, EventArgs e)
        {
            UpdateProjectionVisibility();

            // When switching to Geographic, reset scale factor to 1.0
            // (no distortion — Geographic CRS has no projection scale).
            if (cmbProjectionType.Text == "Geographic")
                nudScaleFactor.Value = 1.0m;
            else if (nudScaleFactor.Value == 1.0m)
                nudScaleFactor.Value = 0.9996m; // restore UTM default
        }

        private void UpdateProjectionVisibility()
        {
            // Hide the Projection Parameters group when:
            //   (a) an EPSG code is supplied — parameters are fully defined
            //       by the registry and should not be overridden manually, OR
            //   (b) the CRS type is Geographic — no projection parameters apply.
            bool hasEpsg =
                !string.IsNullOrWhiteSpace(txtEpsg.Text);
            bool isGeographic =
                cmbProjectionType.Text == "Geographic";

            grpProjectionParams.Visible = !hasEpsg && !isGeographic;
        }

        private void txtEpsg_TextChanged(object? sender, EventArgs e)
            => UpdateProjectionVisibility();

        // ── WKT PLACEHOLDER BEHAVIOUR ────────────────────────────────────────

        private void txtWkt_Enter(object? sender, EventArgs e)
        {
            if (txtWkt.Text == WktPlaceholder)
            {
                txtWkt.Text = "";
                txtWkt.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtWkt_Leave(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWkt.Text))
            {
                txtWkt.Text = WktPlaceholder;
                txtWkt.ForeColor = Color.Gray;
            }
        }

        private string? GetWktValue()
        {
            string v = txtWkt.Text.Trim();
            return v == WktPlaceholder || v.Length == 0 ? null : v;
        }

        // ── VALIDATE ─────────────────────────────────────────────────────────

        private bool ValidateInput()
        {
            // Required fields
            if (string.IsNullOrWhiteSpace(txtCode.Text))
                return Warn("Code is required.", txtCode);

            if (string.IsNullOrWhiteSpace(txtName.Text))
                return Warn("Name is required.", txtName);

            // EPSG code — must be a positive integer if supplied
            if (!string.IsNullOrWhiteSpace(txtEpsg.Text) &&
                (!int.TryParse(txtEpsg.Text, out int epsg) || epsg <= 0))
                return Warn(
                    "EPSG Code must be a positive integer (e.g. 32644).",
                    txtEpsg);

            if (!grpProjectionParams.Visible)
                return true;  // no projection params to validate

            // Scale factor: UTM = 0.9996, never > 1.0 for a named zone
            double sf = (double)nudScaleFactor.Value;
            if (sf <= 0 || sf > 1.0)
                return Warn(
                    "Scale Factor must be in the range (0, 1].\n" +
                    "The UTM/MUTM standard is 0.9996.",
                    nudScaleFactor);

            if (sf == 1.0)
            {
                // Warn — tangent cylinder causes more edge distortion
                var r = MessageBox.Show(
                    "Scale Factor of exactly 1.0 creates a tangent-cylinder " +
                    "projection with maximum distortion at the zone boundary.\n\n" +
                    "The UTM / MUTM standard uses 0.9996 to balance distortion " +
                    "across the zone.  Continue with 1.0?",
                    "Scale Factor",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (r == DialogResult.No)
                {
                    nudScaleFactor.Value = 0.9996m;
                    nudScaleFactor.Focus();
                    return false;
                }
            }

            // Semi-major axis: must be within Earth-ellipsoid range
            double a = (double)nudSemiMajor.Value;
            if (a < 6_356_000 || a > 6_400_000)
                return Warn(
                    "Semi-Major Axis is outside the plausible range for an " +
                    "Earth ellipsoid (6 356 000 – 6 400 000 m).\n\n" +
                    "WGS84 / GRS80: 6 378 137.000 m\n" +
                    "Everest 1830:  6 377 276.345 m",
                    nudSemiMajor);

            // Inverse flattening: all real Earth ellipsoids are ~290–302
            double invF = (double)nudInvFlat.Value;
            if (invF < 290 || invF > 302)
                return Warn(
                    "Inverse Flattening is outside the plausible range " +
                    "(290 – 302) for an Earth ellipsoid.\n\n" +
                    "WGS84: 298.257 223 563\n" +
                    "GRS80: 298.257 222 101\n" +
                    "Everest 1830: 300.801 700 000",
                    nudInvFlat);

            return true;
        }

        private static bool Warn(string message, Control focusControl)
        {
            MessageBox.Show(message, "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focusControl.Focus();
            return false;
        }

        // ── COLLECT ──────────────────────────────────────────────────────────

        private static string? NullIfBlank(string s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // ── SAVE ─────────────────────────────────────────────────────────────

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                btnSave.Enabled = false;

                // Build / update CRS entity
                var crs = _existing ?? new CoordinateSystem();
                crs.Code = txtCode.Text.Trim();
                crs.Name = txtName.Text.Trim();
                crs.EpsgCode =
                    int.TryParse(txtEpsg.Text, out int epsg) ? epsg : null;
                crs.ProjectionType = cmbProjectionType.Text;
                crs.Region = NullIfBlank(txtRegion.Text);
                crs.Description = NullIfBlank(txtDescription.Text);
                crs.IsSystemDefault = false;
                crs.IsActive = true;

                if (_isNew)
                    await _repo.AddAsync(crs);
                else
                    await _repo.UpdateAsync(crs);

                // Save projection parameters when the group is visible
                if (grpProjectionParams.Visible)
                {
                    var proj = await _projRepo
                        .GetByCoordinateSystemIdAsync(crs.Id)
                        ?? new ProjectionParameters
                        { CoordinateSystemId = crs.Id };

                    proj.CentralMeridian = (double)nudCentralMeridian.Value;
                    proj.LatitudeOfOrigin = (double)nudLatOrigin.Value;
                    proj.ScaleFactor = (double)nudScaleFactor.Value;
                    proj.FalseEasting = (double)nudFalseEasting.Value;
                    proj.FalseNorthing = (double)nudFalseNorthing.Value;

                    proj.Ellipsoid = NullIfBlank(txtEllipsoid.Text);

                    proj.SemiMajorAxis =
                        nudSemiMajor.Value > 0
                        ? (double)nudSemiMajor.Value : null;

                    proj.InverseFlattening =
                        nudInvFlat.Value > 0
                        ? (double)nudInvFlat.Value : null;

                    // Guard: never persist the placeholder string
                    proj.WktDefinition = GetWktValue();

                    if (proj.Id == 0)
                        await _projRepo.AddAsync(proj);
                    else
                        await _projRepo.UpdateAsync(proj);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save failed: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

    }
}
