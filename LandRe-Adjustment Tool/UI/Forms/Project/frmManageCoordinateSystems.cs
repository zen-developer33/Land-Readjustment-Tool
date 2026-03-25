using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Manage coordinate systems dialog.
    /// Default entries are read-only.
    /// User can add new, copy from default,
    /// edit or delete custom entries.
    ///
    /// STAGING PATTERN:
    /// Add / Edit / Delete stage changes in EF Core.
    /// Changes are NOT committed to disk here.
    /// LoadAsync merges committed DB records with
    /// EF Core local cache so new staged records
    /// appear in the list immediately.
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
        /// Loads CRS list by merging:
        ///   1. Committed records from DB (AsNoTracking)
        ///   2. Staged records from EF Core Local cache
        ///
        /// This ensures newly added/edited records appear
        /// in the list even before the user presses Ctrl+S.
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
                // 1 — Committed records from disk
                var fromDb = await _repo.GetAllActiveAsync();

                // 2 — Staged records from EF Core Local cache
                //     These exist in memory but not yet on disk
                var local = AppServices.Context.Session
                    .GetContext()
                    .Set<CoordinateSystem>()
                    .Local
                    .Where(c => c.IsActive)
                    .ToList();

                // 3 — Merge:
                //     Keep DB records whose Id is not in local
                //     (local version takes precedence for same Id)
                //     Append new staged records (Id = 0)
                _items = fromDb
                    .Where(d => !local.Any(
                        l => l.Id == d.Id && l.Id != 0))
                    .Concat(local)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToList();

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
                $"Delete '{crs.Name}'?\n\n" +
                "This cannot be undone.",
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

        private void btnViewParams_Click(
            object? sender, EventArgs e)
        {
            // Details shown on selection in txtDetails
        }

        private void btnClose_Click(
            object? sender, EventArgs e)
        {
            Close();
        }
    }
}