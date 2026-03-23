using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Manage coordinate systems dialog.
    /// Default entries are read-only — cannot be edited or deleted.
    /// User can add new, copy from default, edit/delete custom entries.
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

        private async Task LoadAsync()
        {
            try
            {
                _items = await _repo.GetAllActiveAsync();

                System.Diagnostics.Debug.WriteLine(
                    $"[CRS] Loaded {_items.Count} items");

                RefreshGrid();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[CRS] Load error: {ex.Message}");

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

                // Subtle gray for default rows
                if (crs.IsSystemDefault)
                {
                    dgvCRS.Rows[i].DefaultCellStyle
                        .ForeColor = SystemColors.GrayText;
                    dgvCRS.Rows[i].DefaultCellStyle
                        .BackColor = Color.FromArgb(248, 248, 250);
                }

                dgvCRS.Rows[i].Tag = crs;
            }

            if (dgvCRS.Rows.Count > 0)
            {
                dgvCRS.ClearSelection();
                dgvCRS.Rows[0].Selected = true;
            }

            dgvCRS.ResumeLayout();
            UpdateButtons();
            ShowDetails();
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
            // Double-click a custom row to edit it
            var crs = GetSelectedCRS();
            if (crs == null || crs.IsSystemDefault) return;
            btnEdit.PerformClick();
        }

        private void UpdateButtons()
        {
            var crs = GetSelectedCRS();
            bool has = crs != null;
            bool isDefault = crs?.IsSystemDefault ?? true;

            btnCopyNew.Enabled = has;
            btnEdit.Enabled = has && !isDefault;
            btnDelete.Enabled = has && !isDefault;

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
            sb.AppendLine();
            sb.AppendLine(crs.Description ?? "");

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

            // Pre-fill form with copy of selected
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
            ShowDetails();
        }

        private void btnClose_Click(
            object? sender, EventArgs e)
        {
            Close();
        }
    }
}