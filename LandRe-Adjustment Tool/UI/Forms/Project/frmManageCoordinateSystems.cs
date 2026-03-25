using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Manage coordinate systems dialog.
    ///
    /// STAGING PATTERN:
    /// Add / Edit / Delete stage in EF Core — not committed here.
    /// GetAllActiveAsync uses tracked query — EF Core handles
    /// deduplication automatically. No manual merge needed.
    /// frmMain commits on Ctrl+S.
    /// </summary>
    public partial class frmManageCoordinateSystems : Form
    {
        private readonly ICoordinateSystemRepository _repo;
        private readonly IProjectionParametersRepository _projRepo;
        private List<CoordinateSystem> _items = [];

        public frmManageCoordinateSystems(
            ICoordinateSystemRepository repo,
            IProjectionParametersRepository projRepo)
        {
            InitializeComponent();
            _repo = repo;
            _projRepo = projRepo;
        }

        // ── LOAD ─────────────────────────────────────

        private async void frmManageCoordinateSystems_Load(
            object? sender, EventArgs e)
        {
            await LoadAsync();
        }

        /// <summary>
        /// Loads CRS list via tracked repository query.
        /// GetAllActiveAsync loads into EF Core Local cache.
        /// Staged (Added) records automatically included.
        /// No manual merge needed — EF Core handles it.
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
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
            dgvCRS.SuspendLayout();
            dgvCRS.Rows.Clear();

            foreach (var crs in _items)
            {
                int i = dgvCRS.Rows.Add(
                    crs.Code,
                    crs.Name,
                    crs.EpsgCode?.ToString() ?? "Custom",
                    crs.Region ?? "",
                    crs.IsSystemDefault
                        ? "🔒 Default"
                        : "✏ Custom");

                if (crs.IsSystemDefault)
                {
                    dgvCRS.Rows[i].DefaultCellStyle
                        .ForeColor = SystemColors.GrayText;
                    dgvCRS.Rows[i].DefaultCellStyle
                        .BackColor = Color.FromArgb(248, 248, 250);
                }

                dgvCRS.Rows[i].Tag = crs;
            }

            dgvCRS.ResumeLayout();
            UpdateButtons();
        }

        // ── SELECTION ────────────────────────────────

        private void dgvCRS_SelectionChanged(
            object? sender, EventArgs e)
        {
            UpdateButtons();
            ShowDetails();
        }

        private void dgvCRS_CellDoubleClick(
            object? sender, DataGridViewCellEventArgs e)
        {
            var crs = GetSelectedCRS();
            if (crs == null || crs.IsSystemDefault) return;
            btnEdit.PerformClick();
        }

        private void UpdateButtons()
        {
            var crs = GetSelectedCRS();
            bool has = crs != null;
            bool isDef = crs?.IsSystemDefault ?? true;

            btnCopyNew.Enabled = has;
            btnEdit.Enabled = has && !isDef;
            btnDelete.Enabled = has && !isDef;
        }

        private void ShowDetails()
        {
            var crs = GetSelectedCRS();
            if (crs == null)
            {
                txtDetails.Text = string.Empty;
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Code       : {crs.Code}");
            sb.AppendLine($"Name       : {crs.Name}");
            sb.AppendLine($"EPSG       : {crs.EpsgCode?.ToString() ?? "None — Custom"}");
            sb.AppendLine($"Projection : {crs.ProjectionType ?? "—"}");
            sb.AppendLine($"Region     : {crs.Region ?? "—"}");
            sb.AppendLine($"Type       : {(crs.IsSystemDefault ? "System Default" : "Custom")}");
            if (!string.IsNullOrWhiteSpace(crs.Description))
            {
                sb.AppendLine();
                sb.AppendLine(crs.Description);
            }

            txtDetails.Text = sb.ToString();
        }

        private CoordinateSystem? GetSelectedCRS()
        {
            if (dgvCRS.SelectedRows.Count == 0) return null;
            return dgvCRS.SelectedRows[0].Tag as CoordinateSystem;
        }

        // ── BUTTON HANDLERS ──────────────────────────

        private async void btnAdd_Click(
            object? sender, EventArgs e)
        {
            using var frm = new frmAddEditCoordinateSystem(
                null, _repo, _projRepo);
            if (frm.ShowDialog() == DialogResult.OK)
                await LoadAsync();
        }

        private async void btnCopyNew_Click(
            object? sender, EventArgs e)
        {
            var source = GetSelectedCRS();
            if (source == null) return;

            // Pre-fill with copy values — Id=0 means new
            var copy = new CoordinateSystem
            {
                Code = source.Code + "_COPY",
                Name = source.Name + " (Copy)",
                EpsgCode = source.EpsgCode,
                ProjectionType = source.ProjectionType,
                Region = source.Region,
                Description = source.Description,
                IsSystemDefault = false,
                IsActive = true,
                DisplayOrder = _items.Count + 1
            };

            // frmAddEditCoordinateSystem checks _isNew = (Id==0)
            // btnSave_Click creates brand new entity — no double create
            using var frm = new frmAddEditCoordinateSystem(
                copy, _repo, _projRepo);
            if (frm.ShowDialog() == DialogResult.OK)
                await LoadAsync();
        }

        private async void btnEdit_Click(
            object? sender, EventArgs e)
        {
            var crs = GetSelectedCRS();
            if (crs == null || crs.IsSystemDefault) return;

            using var frm = new frmAddEditCoordinateSystem(
                crs, _repo, _projRepo);
            if (frm.ShowDialog() == DialogResult.OK)
                await LoadAsync();
        }

        private async void btnDelete_Click(
            object? sender, EventArgs e)
        {
            var crs = GetSelectedCRS();
            if (crs == null || crs.IsSystemDefault) return;

            var result = MessageBox.Show(
                $"Delete '{crs.Name}'?\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                await _repo.DeleteAsync(crs.Id);
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