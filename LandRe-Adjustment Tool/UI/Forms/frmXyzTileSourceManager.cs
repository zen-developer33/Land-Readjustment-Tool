using System.ComponentModel;
using Land_Readjustment_Tool.Services.Raster;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Lets users manage reusable XYZ tile source templates for the project.
    /// </summary>
    public sealed class frmXyzTileSourceManager : Form
    {
        private readonly string _projectFolderPath;
        private readonly BindingList<XyzTileSourceRow> _sources = [];
        private readonly DataGridView dgvSources = new();
        private readonly Button btnAdd = new();
        private readonly Button btnDelete = new();
        private readonly Button btnSave = new();
        private readonly Button btnClose = new();

        public frmXyzTileSourceManager(string projectFolderPath)
        {
            _projectFolderPath = projectFolderPath;
            Text = "XYZ Tile Sources";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(820, 450);

            ConfigureGrid();
            ConfigureButtons();
            BuildLayout();
            LoadSources();
        }

        private void ConfigureGrid()
        {
            dgvSources.Dock = DockStyle.Fill;
            dgvSources.AutoGenerateColumns = false;
            dgvSources.AllowUserToAddRows = false;
            dgvSources.AllowUserToDeleteRows = false;
            dgvSources.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSources.MultiSelect = false;
            dgvSources.RowHeadersVisible = false;
            dgvSources.DataSource = _sources;

            dgvSources.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                DataPropertyName = nameof(XyzTileSourceRow.Name),
                Width = 180
            });

            dgvSources.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "URL",
                DataPropertyName = nameof(XyzTileSourceRow.UrlTemplate),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvSources.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Minimum Zoom",
                DataPropertyName = nameof(XyzTileSourceRow.MinZoom),
                Width = 95
            });

            dgvSources.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Max Zoom",
                DataPropertyName = nameof(XyzTileSourceRow.MaxZoom),
                Width = 90
            });

            dgvSources.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = "Image",
                DataPropertyName = nameof(XyzTileSourceRow.ImageExtension),
                DataSource = new[] { "png", "jpg", "jpeg" },
                Width = 75
            });
        }

        private void ConfigureButtons()
        {
            btnAdd.Text = "Add";
            btnAdd.Click += btnAdd_Click;

            btnDelete.Text = "Delete";
            btnDelete.Click += btnDelete_Click;

            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;

            btnClose.Text = "Close";
            btnClose.DialogResult = DialogResult.Cancel;
        }

        private void BuildLayout()
        {
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            layout.Controls.Add(dgvSources, 0, 0);

            FlowLayoutPanel buttons = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };
            buttons.Controls.Add(btnAdd);
            buttons.Controls.Add(btnDelete);
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnClose);

            layout.Controls.Add(buttons, 0, 1);
            Controls.Add(layout);
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

                if (row.MinZoom < 0 || row.MaxZoom > 22 || row.MinZoom > row.MaxZoom)
                {
                    ShowValidationMessage(
                        $"Tile source '{name}' has an invalid zoom range. Use 0 to 22.");
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
