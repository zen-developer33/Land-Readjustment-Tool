using System.ComponentModel;
using Land_Readjustment_Tool.Services.Raster;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Lets users manage reusable XYZ tile source templates for the project.
    /// Built-in sources are displayed as read-only and cannot be edited or deleted.
    /// </summary>
    public sealed partial class frmXyzTileSourceManager : Form
    {
        private readonly string _projectFolderPath;
        private readonly BindingList<XyzTileSourceRow> _sources = [];

        // Visual style for built-in (read-only) rows.
        private static readonly Color BuiltInBackColor = Color.FromArgb(235, 243, 255);
        private static readonly Color BuiltInForeColor = Color.FromArgb(50, 80, 130);

        public frmXyzTileSourceManager()
            : this(string.Empty)
        {
        }

        public frmXyzTileSourceManager(string projectFolderPath)
        {
            _projectFolderPath = projectFolderPath;
            InitializeComponent();
            dgvSources.DataSource = _sources;
            dgvSources.CellBeginEdit += DgvSources_CellBeginEdit;
            dgvSources.SelectionChanged += DgvSources_SelectionChanged;
            dgvSources.RowPrePaint += DgvSources_RowPrePaint;

            if (!IsDesignMode())
                LoadSources();
        }

        private void LoadSources()
        {
            _sources.Clear();
            foreach (XyzTileSourceCatalogItem source in
                     XyzTileSourceCatalogService.Load(_projectFolderPath))
            {
                _sources.Add(XyzTileSourceRow.FromCatalogItem(source));
            }
        }

        // ── Grid interaction ────────────────────────────────────────────────────

        /// <summary>
        /// Blocks editing of any cell that belongs to a built-in row.
        /// </summary>
        private void DgvSources_CellBeginEdit(
            object? sender,
            DataGridViewCellCancelEventArgs e)
        {
            if (dgvSources.Rows[e.RowIndex].DataBoundItem is XyzTileSourceRow { IsBuiltIn: true })
            {
                e.Cancel = true;
                ShowBuiltInProtectionMessage();
            }
        }

        /// <summary>
        /// Enables or disables Delete based on whether the selected row is built-in.
        /// </summary>
        private void DgvSources_SelectionChanged(object? sender, EventArgs e)
        {
            bool isBuiltIn =
                dgvSources.CurrentRow?.DataBoundItem is XyzTileSourceRow { IsBuiltIn: true };
            btnDelete.Enabled = !isBuiltIn;
        }

        /// <summary>
        /// Paints built-in rows with the read-only colour scheme.
        /// </summary>
        private void DgvSources_RowPrePaint(
            object? sender,
            DataGridViewRowPrePaintEventArgs e)
        {
            if (dgvSources.Rows[e.RowIndex].DataBoundItem is XyzTileSourceRow { IsBuiltIn: true } row)
            {
                DataGridViewRow dgvRow = dgvSources.Rows[e.RowIndex];
                dgvRow.DefaultCellStyle.BackColor = BuiltInBackColor;
                dgvRow.DefaultCellStyle.ForeColor = BuiltInForeColor;
                dgvRow.DefaultCellStyle.SelectionBackColor =
                    Color.FromArgb(190, 215, 245);
                dgvRow.DefaultCellStyle.SelectionForeColor = BuiltInForeColor;
            }
        }

        // ── Button handlers ─────────────────────────────────────────────────────

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            _sources.Add(new XyzTileSourceRow
            {
                Name = "New XYZ Source",
                UrlTemplate = "https://example.com/{z}/{x}/{y}.png",
                MinZoom = 0,
                MaxZoom = 19,
                ImageExtension = "png"
            });

            dgvSources.ClearSelection();
            int rowIndex = _sources.Count - 1;
            dgvSources.Rows[rowIndex].Selected = true;
            dgvSources.CurrentCell = dgvSources.Rows[rowIndex].Cells[0];
            dgvSources.BeginEdit(true);
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvSources.CurrentRow?.DataBoundItem is not XyzTileSourceRow row)
                return;

            if (row.IsBuiltIn)
            {
                ShowBuiltInProtectionMessage();
                return;
            }

            _sources.Remove(row);
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            dgvSources.EndEdit();

            if (!TryBuildCatalog(out List<XyzTileSourceCatalogItem> catalog))
                return;

            XyzTileSourceCatalogService.Save(_projectFolderPath, catalog);
            DialogResult = DialogResult.OK;
            Close();
        }

        // ── Validation ──────────────────────────────────────────────────────────

        private bool TryBuildCatalog(out List<XyzTileSourceCatalogItem> catalog)
        {
            catalog = [];
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            foreach (XyzTileSourceRow row in _sources)
            {
                // Built-in sources are never saved — they're always re-injected from code.
                if (row.IsBuiltIn)
                    continue;

                string name = row.Name.Trim();
                string url = row.UrlTemplate.Trim();
                string imageExtension = string.IsNullOrWhiteSpace(row.ImageExtension)
                    ? "png"
                    : row.ImageExtension.Trim().TrimStart('.').ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(url))
                {
                    ShowValidationMessage("Each tile source must have a name and URL.");
                    return false;
                }

                if (!HasUsableTileTokens(url))
                {
                    ShowValidationMessage(
                        $"Tile source '{name}' must include {{z}}, {{x}}, and {{y}} in the URL, or {{quadkey}} for Bing Maps.");
                    return false;
                }

                if (row.MinZoom < 0 || row.MaxZoom > 25 || row.MinZoom > row.MaxZoom)
                {
                    ShowValidationMessage(
                        $"Tile source '{name}' has an invalid zoom range. Use 0 to 25.");
                    return false;
                }

                if (!names.Add(name))
                {
                    ShowValidationMessage($"Tile source name '{name}' is duplicated.");
                    return false;
                }

                catalog.Add(new XyzTileSourceCatalogItem(
                    name, url, row.MinZoom, row.MaxZoom, imageExtension));
            }

            return true;
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private void ShowBuiltInProtectionMessage()
        {
            MessageBox.Show(
                this,
                "Built-in tile sources are read-only and cannot be edited or deleted.\n\n" +
                "Click 'Add' to create your own custom source.",
                "Built-in Source",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ShowValidationMessage(string message)
        {
            MessageBox.Show(
                this,
                message,
                "XYZ Tile Sources",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static bool ContainsTileToken(string urlTemplate, string token)
        {
            return urlTemplate.Contains($"{{{token}}}", StringComparison.OrdinalIgnoreCase) ||
                   urlTemplate.Contains($"${{{token}}}", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasUsableTileTokens(string urlTemplate)
        {
            return ContainsTileToken(urlTemplate, "quadkey") ||
                   (ContainsTileToken(urlTemplate, "z") &&
                    ContainsTileToken(urlTemplate, "x") &&
                    ContainsTileToken(urlTemplate, "y"));
        }

        private bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   DesignMode ||
                   string.IsNullOrWhiteSpace(_projectFolderPath);
        }

        // ── Row model ───────────────────────────────────────────────────────────

        private sealed class XyzTileSourceRow
        {
            public string Name { get; set; } = string.Empty;
            public string UrlTemplate { get; set; } = string.Empty;
            public int MinZoom { get; set; }
            public int MaxZoom { get; set; }
            public string ImageExtension { get; set; } = "png";

            /// <summary>
            /// When <see langword="true"/> this row is a built-in source:
            /// it cannot be edited or deleted in the UI.
            /// </summary>
            public bool IsBuiltIn { get; init; }

            public static XyzTileSourceRow FromCatalogItem(XyzTileSourceCatalogItem item)
            {
                return new XyzTileSourceRow
                {
                    Name = item.Name,
                    UrlTemplate = item.UrlTemplate,
                    MinZoom = item.MinZoom,
                    MaxZoom = item.MaxZoom,
                    ImageExtension = item.ImageExtension,
                    IsBuiltIn = item.IsBuiltIn
                };
            }
        }
    }
}
