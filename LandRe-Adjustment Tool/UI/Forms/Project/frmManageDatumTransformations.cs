using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Manage datum transformations dialog.
    ///
    /// STAGING PATTERN:
    /// Add / Edit / Delete stage in EF Core — not committed here.
    /// GetAllActiveAsync uses tracked query — EF Core Local cache
    /// includes staged records automatically. No manual merge needed.
    /// frmMain commits on Ctrl+S.
    /// </summary>
    public partial class frmManageDatumTransformations : Form
    {
        private readonly IDatumTransformationRepository _repo;
        private List<DatumTransformation> _items = [];

        public frmManageDatumTransformations(
            IDatumTransformationRepository repo)
        {
            InitializeComponent();
            _repo = repo;
        }

        // ── LOAD ─────────────────────────────────────

        private async void frmManageDatumTransformations_Load(
            object? sender, EventArgs e)
        {
            await LoadAsync();
        }

        /// <summary>
        /// Loads datum list via tracked repository query.
        /// GetAllActiveAsync loads into EF Core Local cache.
        /// Staged (Added) records automatically included.
        /// No manual merge needed — EF Core handles it.
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
                // Tracked query — returns DB records + staged records
                // EF Core Identity Map deduplicates automatically
                _items = await _repo.GetAllActiveAsync();
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RefreshGrid()
        {
            dgvDatum.SuspendLayout();
            dgvDatum.Rows.Clear();

            foreach (var d in _items)
            {
                int i = dgvDatum.Rows.Add(
                    d.Code,
                    d.Name,
                    d.SourceDatum,
                    d.TargetDatum,
                    d.ApplicableCrsCodes ?? "All",
                    d.IsSystemDefault
                        ? "🔒 Default"
                        : "✏ Custom");

                if (d.IsSystemDefault)
                {
                    dgvDatum.Rows[i].DefaultCellStyle
                        .ForeColor = SystemColors.GrayText;
                    dgvDatum.Rows[i].DefaultCellStyle
                        .BackColor = Color.FromArgb(248, 248, 250);
                }

                dgvDatum.Rows[i].Tag = d;
            }

            dgvDatum.ResumeLayout();
            UpdateButtons();
        }

        // ── SELECTION ────────────────────────────────

        private void dgvDatum_SelectionChanged(
            object? sender, EventArgs e)
        {
            UpdateButtons();
            ShowDetails();
        }

        private void UpdateButtons()
        {
            var d = GetSelected();
            bool has = d != null;
            bool isDef = d?.IsSystemDefault ?? true;

            btnCopyNew.Enabled = has;
            btnEdit.Enabled = has && !isDef;
            btnDelete.Enabled = has && !isDef;
        }

        private void ShowDetails()
        {
            var d = GetSelected();
            if (d == null)
            {
                txtDetails.Text = string.Empty;
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Code       : {d.Code}");
            sb.AppendLine($"Name       : {d.Name}");
            sb.AppendLine($"Source     : {d.SourceDatum}");
            sb.AppendLine($"Target     : {d.TargetDatum}");
            sb.AppendLine($"Applies To : {d.ApplicableCrsCodes ?? "All"}");
            sb.AppendLine($"Type       : {(d.IsSystemDefault ? "System Default" : "Custom")}");
            sb.AppendLine();
            sb.AppendLine("── Helmert Parameters ──────────");
            sb.AppendLine($"ΔX  = {d.DeltaX,10:F4} m");
            sb.AppendLine($"ΔY  = {d.DeltaY,10:F4} m");
            sb.AppendLine($"ΔZ  = {d.DeltaZ,10:F4} m");
            sb.AppendLine($"rX  = {d.RotationX,10:F6} \"");
            sb.AppendLine($"rY  = {d.RotationY,10:F6} \"");
            sb.AppendLine($"rZ  = {d.RotationZ,10:F6} \"");
            sb.AppendLine($"Sc  = {d.ScalePpm,10:F4} ppm");
            sb.AppendLine();
            sb.AppendLine($"Source Ref : {d.Source ?? "—"}");
            sb.AppendLine($"Region     : {d.Region ?? "—"}");
            if (!string.IsNullOrWhiteSpace(d.Description))
            {
                sb.AppendLine();
                sb.AppendLine(d.Description);
            }

            txtDetails.Text = sb.ToString();
        }

        private DatumTransformation? GetSelected()
        {
            if (dgvDatum.SelectedRows.Count == 0) return null;
            return dgvDatum.SelectedRows[0].Tag as DatumTransformation;
        }

        // ── BUTTON HANDLERS ──────────────────────────

        private async void btnAdd_Click(
            object? sender, EventArgs e)
        {
            using var frm = new frmAddEditDatumTransformation(
                null, _repo);
            if (frm.ShowDialog() == DialogResult.OK)
                await LoadAsync();
        }

        private async void btnCopyNew_Click(
            object? sender, EventArgs e)
        {
            var source = GetSelected();
            if (source == null) return;

            // Pre-fill with copy values — Id=0 means new
            var copy = new DatumTransformation
            {
                Code = source.Code + "_COPY",
                Name = source.Name + " (Copy)",
                SourceDatum = source.SourceDatum,
                TargetDatum = source.TargetDatum,
                DeltaX = source.DeltaX,
                DeltaY = source.DeltaY,
                DeltaZ = source.DeltaZ,
                RotationX = source.RotationX,
                RotationY = source.RotationY,
                RotationZ = source.RotationZ,
                ScalePpm = source.ScalePpm,
                ApplicableCrsCodes = source.ApplicableCrsCodes,
                Source = source.Source,
                Region = source.Region,
                Description = source.Description,
                IsSystemDefault = false,
                IsActive = true,
                DisplayOrder = _items.Count + 1
            };

            // frmAddEditDatumTransformation checks _isNew = (Id==0)
            // CollectFormData creates brand new entity — no double create
            using var frm = new frmAddEditDatumTransformation(
                copy, _repo);
            if (frm.ShowDialog() == DialogResult.OK)
                await LoadAsync();
        }

        private async void btnEdit_Click(
            object? sender, EventArgs e)
        {
            var d = GetSelected();
            if (d == null || d.IsSystemDefault) return;

            using var frm = new frmAddEditDatumTransformation(
                d, _repo);
            if (frm.ShowDialog() == DialogResult.OK)
                await LoadAsync();
        }

        private async void btnDelete_Click(
            object? sender, EventArgs e)
        {
            var d = GetSelected();
            if (d == null || d.IsSystemDefault) return;

            var result = MessageBox.Show(
                $"Delete '{d.Name}'?\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                await _repo.DeleteAsync(d.Id);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Delete failed: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(
            object? sender, EventArgs e)
        {
            Close();
        }
    }
}