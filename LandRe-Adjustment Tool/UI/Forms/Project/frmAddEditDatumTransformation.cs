using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Add or edit a Helmert 7-parameter datum transformation.
    ///
    /// GEODETIC BACKGROUND
    /// ───────────────────
    /// A Helmert (Bursa-Wolf) 7-parameter transformation converts
    /// geocentric Cartesian coordinates (X,Y,Z) between two datums:
    ///
    ///   X_t = ΔX + (1 + s·1e-6)·( X_s − Rz·Y_s + Ry·Z_s)
    ///   Y_t = ΔY + (1 + s·1e-6)·( Rz·X_s + Y_s − Rx·Z_s)
    ///   Z_t = ΔZ + (1 + s·1e-6)·(−Ry·X_s + Rx·Y_s + Z_s)
    ///
    /// Parameters
    ///   ΔX ΔY ΔZ   translations in METRES          typical range: ±1000 m
    ///   Rx Ry Rz   rotations in ARC-SECONDS         typical range: ±5 arcsec
    ///   Scale (s)  scale difference in PPM           typical range: ±30 ppm
    ///
    /// Identity transform (source ≡ target): all 7 parameters = 0.
    ///
    /// Convention: Position-Vector (ISO 19111 / EPSG method 1033) — the
    /// European standard.  The US Coordinate-Frame convention (EPSG 1032)
    /// uses opposite signs on all three rotation parameters.
    ///
    /// Pass null to add new; pass existing entity to edit.
    /// </summary>
    public partial class frmAddEditDatumTransformation : Form
    {
        private readonly IDatumTransformationRepository _repo;
        private readonly DatumTransformation? _existing;
        private readonly bool _isNew;

        // ── Geodetic sanity bounds ────────────────────────────────────────────
        // Wider than any published real-world transform but tight enough
        // to catch obvious data-entry mistakes.
        private const double MaxTranslationM = 5_000.0;  // metres
        private const double MaxRotationArcsec = 60.0;  // arcseconds
        private const double MaxScalePpm = 150.0;  // ppm

        private const string AppliesToPlaceholder = "e.g. MUTM81, MUTM82, MUTM83";
        private const string DataSourcePlaceholder = "e.g. Survey Department Nepal";
        private const string RegionPlaceholder = "e.g. Nepal";

        public frmAddEditDatumTransformation(
            DatumTransformation? existing,
            IDatumTransformationRepository repo)
        {
            InitializeComponent();
            _repo = repo;
            _existing = existing;
            _isNew = existing?.Id == 0 || existing == null;
        }

        // ── LOAD ─────────────────────────────────────────────────────────────

        private void frmAddEditDatumTransformation_Load(
            object? sender, EventArgs e)
        {
            Text = _isNew
                ? "Add Datum Transformation"
                : "Edit Datum Transformation";


            ApplyNudBounds();

            if (_existing != null)
                PopulateForm(_existing);
            // else: all NUDs already default to 0 — correct for
            // the identity transform (source datum ≡ target datum).
        }

        /// <summary>
        /// Tightens NumericUpDown bounds and precision to geodetically
        /// correct ranges.  The designer uses ±1 000 000 for all of them,
        /// which would silently accept physically impossible values.
        /// </summary>
        private void ApplyNudBounds()
        {
            // Translations — metres, 4 decimal places (0.1 mm resolution)
            foreach (var nud in new[] { nudDeltaX, nudDeltaY, nudDeltaZ })
            {
                nud.Minimum = (decimal)-MaxTranslationM;
                nud.Maximum = (decimal)MaxTranslationM;
                nud.DecimalPlaces = 4;
                nud.Increment = 0.0001m;
            }

            // Rotations — arc-seconds, 6 decimal places (micro-arcsecond)
            // EPSG dataset publishes rotations to 6 decimal places.
            foreach (var nud in new[] { nudRx, nudRy, nudRz })
            {
                nud.Minimum = (decimal)-MaxRotationArcsec;
                nud.Maximum = (decimal)MaxRotationArcsec;
                nud.DecimalPlaces = 6;
                nud.Increment = 0.000001m;
            }

            // Scale — ppm, 4 decimal places
            nudScale.Minimum = (decimal)-MaxScalePpm;
            nudScale.Maximum = (decimal)MaxScalePpm;
            nudScale.DecimalPlaces = 4;
            nudScale.Increment = 0.0001m;
        }

        private void PopulateForm(DatumTransformation d)
        {
            txtCode.Text = d.Code;
            txtName.Text = d.Name;
            txtSourceDatum.Text = d.SourceDatum;

            // cmbTargetDatum — select matching item or fall back to free text
            int idx = cmbTargetDatum.FindStringExact(d.TargetDatum);
            if (idx >= 0)
                cmbTargetDatum.SelectedIndex = idx;
            else
                cmbTargetDatum.Text = d.TargetDatum;

            // Clamp to UI bounds before assigning — prevents a silent NUD
            // overflow exception on bad legacy data.
            nudDeltaX.Value = Clamp(nudDeltaX, (decimal)d.DeltaX);
            nudDeltaY.Value = Clamp(nudDeltaY, (decimal)d.DeltaY);
            nudDeltaZ.Value = Clamp(nudDeltaZ, (decimal)d.DeltaZ);
            nudRx.Value = Clamp(nudRx, (decimal)d.RotationX);
            nudRy.Value = Clamp(nudRy, (decimal)d.RotationY);
            nudRz.Value = Clamp(nudRz, (decimal)d.RotationZ);
            nudScale.Value = Clamp(nudScale, (decimal)d.ScalePpm);

            txtAppliesTo.Text = d.ApplicableCrsCodes ?? "";
            txtDataSource.Text = d.Source ?? "";
            txtRegion.Text = d.Region ?? "";
            txtDescription.Text = d.Description ?? "";
        }

        private static decimal Clamp(NumericUpDown nud, decimal v)
            => Math.Max(nud.Minimum, Math.Min(nud.Maximum, v));

        // ── COLLECT ──────────────────────────────────────────────────────────

        private DatumTransformation CollectFormData()
        {
            var d = _existing ?? new DatumTransformation();

            d.Code = txtCode.Text.Trim();
            d.Name = txtName.Text.Trim();
            d.SourceDatum = txtSourceDatum.Text.Trim();
            d.TargetDatum = cmbTargetDatum.Text.Trim();

            // 7 Helmert parameters
            d.DeltaX = (double)nudDeltaX.Value;
            d.DeltaY = (double)nudDeltaY.Value;
            d.DeltaZ = (double)nudDeltaZ.Value;
            d.RotationX = (double)nudRx.Value;
            d.RotationY = (double)nudRy.Value;
            d.RotationZ = (double)nudRz.Value;
            d.ScalePpm = (double)nudScale.Value;

            // Optional metadata — null rather than empty string
            d.ApplicableCrsCodes = NullIfBlank(txtAppliesTo.Text, AppliesToPlaceholder);
            d.Source = NullIfBlank(txtDataSource.Text, DataSourcePlaceholder);
            d.Region = NullIfBlank(txtRegion.Text, RegionPlaceholder);
            d.Description = NullIfBlank(txtDescription.Text);

            d.IsSystemDefault = false;
            d.IsActive = true;

            return d;
        }

        private static string? NullIfBlank(string s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string? NullIfBlank(string s, string placeholder)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            string v = s.Trim();
            return v.Equals(placeholder, StringComparison.OrdinalIgnoreCase)
                ? null
                : v;
        }

        // ── VALIDATE ─────────────────────────────────────────────────────────

        private bool ValidateInput()
        {
            // Required fields
            if (string.IsNullOrWhiteSpace(txtCode.Text))
                return Warn("Code is required.", txtCode);

            if (string.IsNullOrWhiteSpace(txtName.Text))
                return Warn("Name is required.", txtName);

            if (string.IsNullOrWhiteSpace(txtSourceDatum.Text))
                return Warn("Source Datum is required.", txtSourceDatum);

            if (string.IsNullOrWhiteSpace(cmbTargetDatum.Text))
                return Warn("Target Datum is required.", cmbTargetDatum);

            // ── Geodetic sanity warnings ─────────────────────────────────────
            //
            // Translations: published transforms (e.g. local → WGS84) are
            // almost always within ±1000 m.  Values up to ±5000 m are
            // technically accepted but almost certainly a unit error
            // (the user may have entered millimetres instead of metres).
            double dx = (double)nudDeltaX.Value;
            double dy = (double)nudDeltaY.Value;
            double dz = (double)nudDeltaZ.Value;
            if (Math.Abs(dx) > 1000 || Math.Abs(dy) > 1000 || Math.Abs(dz) > 1000)
            {
                if (!ConfirmUnusual(
                    "One or more translation values (ΔX / ΔY / ΔZ) exceed ±1000 m.\n\n" +
                    "Real-world datum transformations are almost always under ±1000 m. " +
                    "Check that you have entered values in METRES (not millimetres or km)."))
                {
                    nudDeltaX.Focus();
                    return false;
                }
            }

            // Rotations: sub-arcsecond is normal; more than ±5 arcsec
            // strongly suggests the user entered degrees instead of arcseconds
            // (1 degree = 3600 arcseconds).
            double rx = (double)nudRx.Value;
            double ry = (double)nudRy.Value;
            double rz = (double)nudRz.Value;
            if (Math.Abs(rx) > 5 || Math.Abs(ry) > 5 || Math.Abs(rz) > 5)
            {
                if (!ConfirmUnusual(
                    "One or more rotation values (Rx / Ry / Rz) exceed ±5 arc-seconds.\n\n" +
                    "Rotations must be entered in ARC-SECONDS (not degrees or radians).\n" +
                    "1 degree = 3600 arcseconds.  Most published transforms are sub-arcsecond."))
                {
                    nudRx.Focus();
                    return false;
                }
            }

            // Scale: most datum transforms are within ±30 ppm.
            // Values above ±30 ppm are geophysically unusual.
            double scale = (double)nudScale.Value;
            if (Math.Abs(scale) > 30)
            {
                if (!ConfirmUnusual(
                    $"Scale difference of {scale:+0.0000;-0.0000} ppm is unusually large.\n\n" +
                    "Most published datum transformations are within ±30 ppm. " +
                    "Verify the value is in PARTS-PER-MILLION (ppm), not in metres/metre."))
                {
                    nudScale.Focus();
                    return false;
                }
            }

            return true;
        }

        private static bool Warn(string message, Control focusControl)
        {
            MessageBox.Show(message, "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focusControl.Focus();
            return false;
        }

        private static bool ConfirmUnusual(string message)
            => MessageBox.Show(
                message + "\n\nContinue anyway?",
                "Unusual Value",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes;

        // ── SAVE ─────────────────────────────────────────────────────────────

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                btnSave.Enabled = false;
                var d = CollectFormData();

                if (_isNew)
                    await _repo.AddAsync(d);
                else
                    await _repo.UpdateAsync(d);

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