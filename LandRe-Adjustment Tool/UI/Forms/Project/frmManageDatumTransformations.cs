using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Manage datum transformations dialog.
    /// Default entries are read-only.
    /// User can add, copy, edit or delete custom entries.
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
            dgvDatum.Rows.Clear();

            foreach (var d in _items)
            {
                int rowIdx = dgvDatum.Rows.Add(
                    d.Code,
                    d.Name,
                    d.SourceDatum,
                    d.TargetDatum,
                    d.ApplicableCrsCodes ?? "All",
                    d.IsSystemDefault ? "🔒 Default" : "✏ Custom");

                if (d.IsSystemDefault)
                {
                    dgvDatum.Rows[rowIdx].DefaultCellStyle
                        .ForeColor = Color.FromArgb(100, 110, 130);
                    dgvDatum.Rows[rowIdx].DefaultCellStyle
                        .BackColor = Color.FromArgb(245, 247, 252);
                }

                dgvDatum.Rows[rowIdx].Tag = d;
            }

            UpdateButtons();
        }

        private void dgvDatum_SelectionChanged(
            object? sender, EventArgs e)
        {
            UpdateButtons();
            ShowDetails();
        }

        private void UpdateButtons()
        {
            var selected = GetSelected();
            bool hasSelection = selected != null;
            bool isDefault = selected?.IsSystemDefault ?? true;

            btnEdit.Enabled   = hasSelection && !isDefault;
            btnDelete.Enabled = hasSelection && !isDefault;
            btnCopyNew.Enabled = hasSelection;
        }

        private void ShowDetails()
        {
            var d = GetSelected();
            if (d == null)
            {
                txtDetails.Text = "";
                return;
            }

            txtDetails.Text =
                $"Code      : {d.Code}\n" +
                $"Name      : {d.Name}\n" +
                $"Source    : {d.SourceDatum}\n" +
                $"Target    : {d.TargetDatum}\n" +
                $"Applies to: {d.ApplicableCrsCodes ?? "All"}\n\n" +
                $"── Helmert Parameters ──\n" +
                $"ΔX = {d.DeltaX:F4} m\n" +
                $"ΔY = {d.DeltaY:F4} m\n" +
                $"ΔZ = {d.DeltaZ:F4} m\n" +
                $"rX = {d.RotationX:F6} \"\n" +
                $"rY = {d.RotationY:F6} \"\n" +
                $"rZ = {d.RotationZ:F6} \"\n" +
                $"Sc = {d.ScalePpm:F4} ppm\n\n" +
                $"Source    : {d.Source ?? "—"}\n" +
                $"Region    : {d.Region ?? "—"}\n\n" +
                $"{d.Description ?? ""}";
        }

        private DatumTransformation? GetSelected()
        {
            if (dgvDatum.SelectedRows.Count == 0)
                return null;
            return dgvDatum.SelectedRows[0].Tag
                as DatumTransformation;
        }

        // ── BUTTONS ──────────────────────────────────

        private async void btnAdd_Click(
            object? sender, EventArgs e)
        {
            using var frm =
                new frmAddEditDatumTransformation(
                    null, _repo);
            if (frm.ShowDialog() == DialogResult.OK)
                await LoadAsync();
        }

        private async void btnCopyNew_Click(
            object? sender, EventArgs e)
        {
            var source = GetSelected();
            if (source == null) return;

            var copy = new DatumTransformation
            {
                Code                = source.Code + "_COPY",
                Name                = source.Name + " (Copy)",
                SourceDatum         = source.SourceDatum,
                TargetDatum         = source.TargetDatum,
                DeltaX              = source.DeltaX,
                DeltaY              = source.DeltaY,
                DeltaZ              = source.DeltaZ,
                RotationX           = source.RotationX,
                RotationY           = source.RotationY,
                RotationZ           = source.RotationZ,
                ScalePpm            = source.ScalePpm,
                ApplicableCrsCodes  = source.ApplicableCrsCodes,
                Source              = source.Source,
                Region              = source.Region,
                Description         = source.Description,
                IsSystemDefault     = false,
                IsActive            = true,
                DisplayOrder        = _items.Count + 1
            };

            using var frm =
                new frmAddEditDatumTransformation(
                    copy, _repo);
            if (frm.ShowDialog() == DialogResult.OK)
                await LoadAsync();
        }

        private async void btnEdit_Click(
            object? sender, EventArgs e)
        {
            var d = GetSelected();
            if (d == null || d.IsSystemDefault) return;

            using var frm =
                new frmAddEditDatumTransformation(
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
            this.Close();
        }
    }
}
