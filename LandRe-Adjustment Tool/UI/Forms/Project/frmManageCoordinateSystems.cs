using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Manage coordinate systems dialog.
    /// Shows all CRS — default ones are read-only.
    /// User can add new CRS or copy from defaults.
    /// User can edit/delete only their own entries.
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
            dgvCRS.Rows.Clear();

            foreach (var crs in _items)
            {
                int rowIdx = dgvCRS.Rows.Add(
                    crs.Code,
                    crs.Name,
                    crs.EpsgCode?.ToString() ?? "Custom",
                    crs.Region ?? "",
                    crs.IsSystemDefault ? "🔒 Default" : "✏ Custom");

                // Gray out default rows visually
                if (crs.IsSystemDefault)
                {
                    dgvCRS.Rows[rowIdx].DefaultCellStyle
                        .ForeColor = Color.FromArgb(100, 110, 130);
                    dgvCRS.Rows[rowIdx].DefaultCellStyle
                        .BackColor = Color.FromArgb(245, 247, 252);
                }

                dgvCRS.Rows[rowIdx].Tag = crs;
            }

            UpdateButtons();
        }

        // ── SELECTION CHANGED ────────────────────────

        private void dgvCRS_SelectionChanged(
            object? sender, EventArgs e)
        {
            UpdateButtons();
            ShowDetails();
        }

        private void UpdateButtons()
        {
            var selected = GetSelectedCRS();
            bool hasSelection = selected != null;
            bool isDefault = selected?.IsSystemDefault ?? true;

            btnEdit.Enabled   = hasSelection && !isDefault;
            btnDelete.Enabled = hasSelection && !isDefault;
            btnCopyNew.Enabled = hasSelection;
            btnViewParams.Enabled = hasSelection;
        }

        private void ShowDetails()
        {
            var crs = GetSelectedCRS();
            if (crs == null)
            {
                txtDetails.Text = string.Empty;
                return;
            }

            var details =
                $"Code: {crs.Code}\n" +
                $"Name: {crs.Name}\n" +
                $"EPSG: {crs.EpsgCode?.ToString() ?? "None — Custom"}\n" +
                $"Type: {crs.ProjectionType ?? "—"}\n" +
                $"Region: {crs.Region ?? "—"}\n\n" +
                $"{crs.Description ?? ""}";

            txtDetails.Text = details;
        }

        private CoordinateSystem? GetSelectedCRS()
        {
            if (dgvCRS.SelectedRows.Count == 0) return null;
            return dgvCRS.SelectedRows[0].Tag as CoordinateSystem;
        }

        // ── BUTTONS ──────────────────────────────────

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

            // Create a copy with IsSystemDefault = false
            var copy = new CoordinateSystem
            {
                Code            = source.Code + "_COPY",
                Name            = source.Name + " (Copy)",
                EpsgCode        = source.EpsgCode,
                ProjectionType  = source.ProjectionType,
                Region          = source.Region,
                Description     = source.Description,
                IsSystemDefault = false,
                IsActive        = true,
                DisplayOrder    = _items.Count + 1
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
            var crs = GetSelectedCRS();
            if (crs == null) return;

            // Show projection parameters in detail view
            tabControl.SelectedTab = tabDetails;
        }

        private void btnClose_Click(
            object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}
