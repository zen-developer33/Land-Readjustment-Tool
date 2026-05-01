using System.ComponentModel;
using Land_Readjustment_Tool.Services.Raster;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Lets users manage reusable XYZ tile source templates for the project.
    /// </summary>
    public sealed partial class frmXyzTileSourceManager : Form
    {
        private readonly string _projectFolderPath;
        private readonly BindingList<XyzTileSourceRow> _sources = [];

        public frmXyzTileSourceManager()
            : this(string.Empty)
        {
        }

        public frmXyzTileSourceManager(string projectFolderPath)
        {
            _projectFolderPath = projectFolderPath;
            InitializeComponent();
            dgvSources.DataSource = _sources;

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
            {
                return;
            }

            _sources.Remove(row);
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            dgvSources.EndEdit();

            if (!TryBuildCatalog(out List<XyzTileSourceCatalogItem> catalog))
            {
                return;
            }

            XyzTileSourceCatalogService.Save(_projectFolderPath, catalog);
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool TryBuildCatalog(out List<XyzTileSourceCatalogItem> catalog)
        {
            catalog = [];
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            foreach (XyzTileSourceRow row in _sources)
            {
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

                if (!ContainsTileToken(url, "z") ||
                    !ContainsTileToken(url, "x") ||
                    !ContainsTileToken(url, "y"))
                {
                    ShowValidationMessage(
                        $"Tile source '{name}' must include {{z}}, {{x}}, and {{y}} in the URL.");
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
                    name,
                    url,
                    row.MinZoom,
                    row.MaxZoom,
                    imageExtension));
            }

            return true;
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

        private bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   DesignMode ||
                   string.IsNullOrWhiteSpace(_projectFolderPath);
        }

        private sealed class XyzTileSourceRow
        {
            public string Name { get; set; } = string.Empty;
            public string UrlTemplate { get; set; } = string.Empty;
            public int MinZoom { get; set; }
            public int MaxZoom { get; set; }
            public string ImageExtension { get; set; } = "png";

            public static XyzTileSourceRow FromCatalogItem(
                XyzTileSourceCatalogItem item)
            {
                return new XyzTileSourceRow
                {
                    Name = item.Name,
                    UrlTemplate = item.UrlTemplate,
                    MinZoom = item.MinZoom,
                    MaxZoom = item.MaxZoom,
                    ImageExtension = item.ImageExtension
                };
            }
        }
    }
}
