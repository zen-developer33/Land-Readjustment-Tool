using Land_Readjustment_Tool.Services.Raster;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Collects the settings needed to import an online XYZ tile source.
    /// </summary>
    public sealed class frmXyzTileImportOptions : Form
    {
        private readonly string _projectFolderPath;
        private readonly ComboBox cmbTileSource = new();
        private readonly Button btnManageSources = new();
        private readonly TextBox txtLayerName = new();
        private readonly TextBox txtUrlTemplate = new();
        private readonly NumericUpDown numMinLongitude;
        private readonly NumericUpDown numMinLatitude;
        private readonly NumericUpDown numMaxLongitude;
        private readonly NumericUpDown numMaxLatitude;
        private readonly NumericUpDown numZoomLevel = new();
        private readonly Button btnImport = new();
        private readonly Button btnCancel = new();
        private List<XyzTileSourceCatalogItem> _tileSources = [];

        public frmXyzTileImportOptions(string projectFolderPath)
        {
            _projectFolderPath = projectFolderPath;
            Text = "Import XYZ Tiles";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(620, 430);

            numMinLongitude = CreateCoordinateInput(-180m, 180m, 84.0m);
            numMaxLongitude = CreateCoordinateInput(-180m, 180m, 85.0m);
            numMinLatitude = CreateCoordinateInput(-85.05112878m, 85.05112878m, 27.5m);
            numMaxLatitude = CreateCoordinateInput(-85.05112878m, 85.05112878m, 28.5m);

            ConfigureControls();
            BuildLayout();
            LoadTileSources();
        }

        public XyzTileSourceImportRequest ImportRequest =>
            new(
                txtLayerName.Text.Trim(),
                SelectedTileSource.UrlTemplate,
                decimal.ToDouble(numMinLongitude.Value),
                decimal.ToDouble(numMinLatitude.Value),
                decimal.ToDouble(numMaxLongitude.Value),
                decimal.ToDouble(numMaxLatitude.Value),
                decimal.ToInt32(numZoomLevel.Value),
                SelectedTileSource.ImageExtension);

        private XyzTileSourceCatalogItem SelectedTileSource =>
            cmbTileSource.SelectedItem as XyzTileSourceCatalogItem ??
            _tileSources.FirstOrDefault() ??
            new XyzTileSourceCatalogItem(
                "OpenStreetMap Standard",
                "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                0,
                19,
                "png");

        private void ConfigureControls()
        {
            txtLayerName.Text = "XYZ Basemap";
            txtLayerName.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            cmbTileSource.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTileSource.DisplayMember = nameof(XyzTileSourceCatalogItem.Name);
            cmbTileSource.SelectedIndexChanged += cmbTileSource_SelectedIndexChanged;

            btnManageSources.Text = "Manage...";
            btnManageSources.Click += btnManageSources_Click;

            txtUrlTemplate.ReadOnly = true;
            txtUrlTemplate.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            numZoomLevel.Minimum = 0;
            numZoomLevel.Maximum = 22;
            numZoomLevel.Value = 13;
            numZoomLevel.Anchor = AnchorStyles.Left;

            btnImport.Text = "Import";
            btnImport.DialogResult = DialogResult.OK;
            btnImport.Click += btnImport_Click;

            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;

            AcceptButton = btnImport;
            CancelButton = btnCancel;
        }

        private void BuildLayout()
        {
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 10
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int row = 0; row < 8; row++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            AddRow(layout, 0, "Layer name", txtLayerName);
            AddTileSourceRow(layout, 1);
            AddRow(layout, 2, "Source URL", txtUrlTemplate);
            AddRow(layout, 3, "Min longitude", numMinLongitude);
            AddRow(layout, 4, "Min latitude", numMinLatitude);
            AddRow(layout, 5, "Max longitude", numMaxLongitude);
            AddRow(layout, 6, "Max latitude", numMaxLatitude);
            AddRow(layout, 7, "Zoom level", numZoomLevel);

            Label hint = new()
            {
                Dock = DockStyle.Fill,
                Text =
                    "Use a small lon/lat window. The importer limits the request to 4,096 tiles so the project stays responsive. " +
                    "Manage sources to add service URLs with {z}, {x}, and {y}.",
                ForeColor = SystemColors.GrayText
            };
            layout.Controls.Add(hint, 0, 8);
            layout.SetColumnSpan(hint, 2);

            FlowLayoutPanel buttons = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };
            buttons.Controls.Add(btnImport);
            buttons.Controls.Add(btnCancel);
            layout.Controls.Add(buttons, 0, 9);
            layout.SetColumnSpan(buttons, 2);

            Controls.Add(layout);
        }

        private static void AddRow(
            TableLayoutPanel layout,
            int row,
            string labelText,
            Control control)
        {
            Label label = new()
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            control.Dock = DockStyle.Fill;

            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(control, 1, row);
        }

        private void AddTileSourceRow(TableLayoutPanel layout, int row)
        {
            Label label = new()
            {
                Text = "Tile source",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            TableLayoutPanel sourceLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));

            cmbTileSource.Dock = DockStyle.Fill;
            btnManageSources.Dock = DockStyle.Fill;
            sourceLayout.Controls.Add(cmbTileSource, 0, 0);
            sourceLayout.Controls.Add(btnManageSources, 1, 0);

            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(sourceLayout, 1, row);
        }

        private static NumericUpDown CreateCoordinateInput(
            decimal minimum,
            decimal maximum,
            decimal value)
        {
            return new NumericUpDown
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                DecimalPlaces = 8,
                Increment = 0.0001m,
                ThousandsSeparator = false
            };
        }

        private void btnImport_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLayerName.Text))
            {
                ShowValidationMessage("Please enter a layer name.");
                return;
            }

            if (_tileSources.Count == 0 || cmbTileSource.SelectedItem == null)
            {
                ShowValidationMessage("Please add or select an XYZ tile source.");
                return;
            }

            XyzTileSourceCatalogItem selectedSource = SelectedTileSource;
            if (!ContainsTileToken(selectedSource.UrlTemplate, "z") ||
                !ContainsTileToken(selectedSource.UrlTemplate, "x") ||
                !ContainsTileToken(selectedSource.UrlTemplate, "y"))
            {
                ShowValidationMessage(
                    "The selected tile source URL must include {z}, {x}, and {y} tokens.");
                return;
            }

            int zoom = decimal.ToInt32(numZoomLevel.Value);
            if (zoom < selectedSource.MinZoom || zoom > selectedSource.MaxZoom)
            {
                ShowValidationMessage(
                    $"Zoom level must be between {selectedSource.MinZoom} and {selectedSource.MaxZoom} for this source.");
                return;
            }

            if (numMinLongitude.Value >= numMaxLongitude.Value)
            {
                ShowValidationMessage("Minimum longitude must be less than maximum longitude.");
                return;
            }

            if (numMinLatitude.Value >= numMaxLatitude.Value)
            {
                ShowValidationMessage("Minimum latitude must be less than maximum latitude.");
                return;
            }
        }

        private void ShowValidationMessage(string message)
        {
            MessageBox.Show(
                this,
                message,
                "Import XYZ Tiles",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }

        private static bool ContainsTileToken(string urlTemplate, string token)
        {
            return urlTemplate.Contains($"{{{token}}}", StringComparison.OrdinalIgnoreCase) ||
                   urlTemplate.Contains($"${{{token}}}", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadTileSources()
        {
            _tileSources = XyzTileSourceCatalogService.Load(_projectFolderPath);
            cmbTileSource.Items.Clear();

            foreach (XyzTileSourceCatalogItem source in _tileSources)
            {
                cmbTileSource.Items.Add(source);
            }

            if (cmbTileSource.Items.Count > 0)
            {
                cmbTileSource.SelectedIndex = 0;
            }

            RefreshSelectedSourceDetails();
        }

        private void cmbTileSource_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RefreshSelectedSourceDetails();
        }

        private void btnManageSources_Click(object? sender, EventArgs e)
        {
            using frmXyzTileSourceManager manager = new(_projectFolderPath);

            if (manager.ShowDialog(this) == DialogResult.OK)
            {
                LoadTileSources();
            }
        }

        private void RefreshSelectedSourceDetails()
        {
            if (cmbTileSource.SelectedItem is not XyzTileSourceCatalogItem source)
            {
                txtUrlTemplate.Text = string.Empty;
                return;
            }

            txtUrlTemplate.Text = source.UrlTemplate;
            decimal minZoom = source.MinZoom;
            decimal maxZoom = source.MaxZoom;
            decimal targetZoom = Math.Clamp(numZoomLevel.Value, minZoom, maxZoom);

            numZoomLevel.Minimum = 0;
            numZoomLevel.Maximum = 22;
            numZoomLevel.Value = targetZoom;
            numZoomLevel.Minimum = minZoom;
            numZoomLevel.Maximum = maxZoom;
        }
    }
}
