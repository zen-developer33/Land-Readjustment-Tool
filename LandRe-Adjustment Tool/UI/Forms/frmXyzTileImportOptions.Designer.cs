namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmXyzTileImportOptions
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top section ──────────────────────────────────────────────────────────
        private TableLayoutPanel layout;
        private Label lblLayerName;
        private TextBox txtLayerName;
        private Label lblTileSource;
        private TableLayoutPanel sourceLayout;
        private ComboBox cmbTileSource;
        private Button btnManageSources;
        private Label lblSourceUrl;
        private TextBox txtUrlTemplate;

        // ── Map-bounds mode selector ─────────────────────────────────────────────
        private FlowLayoutPanel pnlBoundsHeader;
        private Label lblMapBounds;
        private RadioButton rdoCenterRadius;
        private RadioButton rdoBoundingBox;
        private RadioButton rdoLiveTiles;
        private Panel pnlBoundsContainer;

        // ── Center + Radius mode ─────────────────────────────────────────────────
        private Panel pnlCenterRadius;
        private Label lblCenterLat;
        private NumericUpDown numCenterLat;
        private Label lblCenterLon;
        private NumericUpDown numCenterLon;
        private Label lblRadius;
        private NumericUpDown numRadius;
        private Label lblRadiusUnit;
        private Label lblAreaHint;

        // ── Live Tiles info panel ────────────────────────────────────────────────
        private Panel pnlLiveTilesInfo;
        private Label lblLiveTilesInfo;

        // ── Bounding Box mode ────────────────────────────────────────────────────
        private Panel pnlBoundingBox;
        private Label lblNorth;
        private NumericUpDown numNorth;
        private Label lblWest;
        private NumericUpDown numWest;
        private Panel pnlMapRect;
        private Label lblMapRect;
        private Label lblEast;
        private NumericUpDown numEast;
        private Label lblSouth;
        private NumericUpDown numSouth;

        // ── Bottom section ───────────────────────────────────────────────────────
        private Label lblZoomLevel;
        private NumericUpDown numZoomLevel;
        private Label lblDownloadStatus;
        private ProgressBar progressTileDownload;
        private FlowLayoutPanel buttonLayout;
        private Button btnImport;
        private Button btnDownloadTiles;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmXyzTileImportOptions));
            layout = new TableLayoutPanel();
            lblLayerName = new Label();
            txtLayerName = new TextBox();
            lblTileSource = new Label();
            sourceLayout = new TableLayoutPanel();
            cmbTileSource = new ComboBox();
            btnManageSources = new Button();
            lblSourceUrl = new Label();
            txtUrlTemplate = new TextBox();
            pnlBoundsHeader = new FlowLayoutPanel();
            lblMapBounds = new Label();
            rdoCenterRadius = new RadioButton();
            rdoBoundingBox = new RadioButton();
            rdoLiveTiles = new RadioButton();
            pnlBoundsContainer = new Panel();
            pnlLiveTilesInfo = new Panel();
            lblLiveTilesInfo = new Label();
            pnlBoundingBox = new Panel();
            lblNorth = new Label();
            numNorth = new NumericUpDown();
            lblWest = new Label();
            numWest = new NumericUpDown();
            pnlMapRect = new Panel();
            lblMapRect = new Label();
            lblEast = new Label();
            numEast = new NumericUpDown();
            lblSouth = new Label();
            numSouth = new NumericUpDown();
            pnlCenterRadius = new Panel();
            lblCenterLat = new Label();
            numCenterLat = new NumericUpDown();
            lblCenterLon = new Label();
            numCenterLon = new NumericUpDown();
            lblRadius = new Label();
            numRadius = new NumericUpDown();
            lblRadiusUnit = new Label();
            lblAreaHint = new Label();
            lblZoomLevel = new Label();
            numZoomLevel = new NumericUpDown();
            lblDownloadStatus = new Label();
            progressTileDownload = new ProgressBar();
            buttonLayout = new FlowLayoutPanel();
            btnImport = new Button();
            btnDownloadTiles = new Button();
            btnCancel = new Button();
            layout.SuspendLayout();
            sourceLayout.SuspendLayout();
            pnlBoundsHeader.SuspendLayout();
            pnlBoundsContainer.SuspendLayout();
            pnlLiveTilesInfo.SuspendLayout();
            pnlBoundingBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numNorth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWest).BeginInit();
            pnlMapRect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numEast).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSouth).BeginInit();
            pnlCenterRadius.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCenterLat).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCenterLon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRadius).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numZoomLevel).BeginInit();
            buttonLayout.SuspendLayout();
            SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Controls.Add(lblLayerName, 0, 0);
            layout.Controls.Add(txtLayerName, 1, 0);
            layout.Controls.Add(lblTileSource, 0, 1);
            layout.Controls.Add(sourceLayout, 1, 1);
            layout.Controls.Add(lblSourceUrl, 0, 2);
            layout.Controls.Add(txtUrlTemplate, 1, 2);
            layout.Controls.Add(pnlBoundsHeader, 0, 3);
            layout.Controls.Add(pnlBoundsContainer, 0, 4);
            layout.Controls.Add(lblZoomLevel, 0, 5);
            layout.Controls.Add(numZoomLevel, 1, 5);
            layout.Controls.Add(lblDownloadStatus, 0, 6);
            layout.Controls.Add(progressTileDownload, 0, 7);
            layout.Controls.Add(buttonLayout, 0, 8);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Name = "layout";
            layout.Padding = new Padding(10);
            layout.RowCount = 9;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 191F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.Size = new Size(600, 591);
            layout.TabIndex = 0;
            // 
            // lblLayerName
            // 
            lblLayerName.Dock = DockStyle.Fill;
            lblLayerName.Location = new Point(13, 10);
            lblLayerName.Name = "lblLayerName";
            lblLayerName.Size = new Size(104, 38);
            lblLayerName.TabIndex = 0;
            lblLayerName.Text = "Layer name:";
            lblLayerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtLayerName
            // 
            txtLayerName.Dock = DockStyle.Fill;
            txtLayerName.Location = new Point(123, 16);
            txtLayerName.Margin = new Padding(3, 6, 10, 6);
            txtLayerName.Name = "txtLayerName";
            txtLayerName.Size = new Size(457, 27);
            txtLayerName.TabIndex = 1;
            txtLayerName.Text = "XYZ Basemap";
            // 
            // lblTileSource
            // 
            lblTileSource.Dock = DockStyle.Fill;
            lblTileSource.Location = new Point(13, 48);
            lblTileSource.Name = "lblTileSource";
            lblTileSource.Size = new Size(104, 40);
            lblTileSource.TabIndex = 2;
            lblTileSource.Text = "Tile source:";
            lblTileSource.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // sourceLayout
            // 
            sourceLayout.ColumnCount = 2;
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));
            sourceLayout.Controls.Add(cmbTileSource, 0, 0);
            sourceLayout.Controls.Add(btnManageSources, 1, 0);
            sourceLayout.Dock = DockStyle.Fill;
            sourceLayout.Location = new Point(123, 52);
            sourceLayout.Margin = new Padding(3, 4, 10, 4);
            sourceLayout.Name = "sourceLayout";
            sourceLayout.RowCount = 1;
            sourceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            sourceLayout.Size = new Size(457, 32);
            sourceLayout.TabIndex = 3;
            // 
            // cmbTileSource
            // 
            cmbTileSource.DisplayMember = "Name";
            cmbTileSource.Dock = DockStyle.Fill;
            cmbTileSource.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTileSource.FormattingEnabled = true;
            cmbTileSource.Location = new Point(3, 3);
            cmbTileSource.Name = "cmbTileSource";
            cmbTileSource.Size = new Size(357, 28);
            cmbTileSource.TabIndex = 0;
            // 
            // btnManageSources
            // 
            btnManageSources.Dock = DockStyle.Fill;
            btnManageSources.Location = new Point(366, 3);
            btnManageSources.Name = "btnManageSources";
            btnManageSources.Size = new Size(88, 26);
            btnManageSources.TabIndex = 1;
            btnManageSources.Text = "Manage...";
            btnManageSources.UseVisualStyleBackColor = true;
            // 
            // lblSourceUrl
            // 
            lblSourceUrl.Dock = DockStyle.Fill;
            lblSourceUrl.Location = new Point(13, 88);
            lblSourceUrl.Name = "lblSourceUrl";
            lblSourceUrl.Size = new Size(104, 68);
            lblSourceUrl.TabIndex = 4;
            lblSourceUrl.Text = "Source URL:";
            lblSourceUrl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtUrlTemplate
            // 
            txtUrlTemplate.Dock = DockStyle.Fill;
            txtUrlTemplate.Location = new Point(123, 92);
            txtUrlTemplate.Margin = new Padding(3, 4, 10, 4);
            txtUrlTemplate.Multiline = true;
            txtUrlTemplate.Name = "txtUrlTemplate";
            txtUrlTemplate.ReadOnly = true;
            txtUrlTemplate.Size = new Size(457, 60);
            txtUrlTemplate.TabIndex = 5;
            // 
            // pnlBoundsHeader
            // 
            layout.SetColumnSpan(pnlBoundsHeader, 2);
            pnlBoundsHeader.Controls.Add(lblMapBounds);
            pnlBoundsHeader.Controls.Add(rdoCenterRadius);
            pnlBoundsHeader.Controls.Add(rdoBoundingBox);
            pnlBoundsHeader.Controls.Add(rdoLiveTiles);
            pnlBoundsHeader.Dock = DockStyle.Fill;
            pnlBoundsHeader.Location = new Point(13, 156);
            pnlBoundsHeader.Margin = new Padding(3, 0, 3, 0);
            pnlBoundsHeader.Name = "pnlBoundsHeader";
            pnlBoundsHeader.Size = new Size(574, 37);
            pnlBoundsHeader.TabIndex = 6;
            pnlBoundsHeader.WrapContents = false;
            // 
            // lblMapBounds
            // 
            lblMapBounds.Location = new Point(3, 6);
            lblMapBounds.Margin = new Padding(3, 6, 10, 0);
            lblMapBounds.Name = "lblMapBounds";
            lblMapBounds.Size = new Size(82, 20);
            lblMapBounds.TabIndex = 0;
            lblMapBounds.Text = "Map Bounds";
            // 
            // rdoCenterRadius
            // 
            rdoCenterRadius.AutoSize = true;
            rdoCenterRadius.Checked = true;
            rdoCenterRadius.Location = new Point(98, 5);
            rdoCenterRadius.Margin = new Padding(3, 5, 8, 4);
            rdoCenterRadius.Name = "rdoCenterRadius";
            rdoCenterRadius.Size = new Size(135, 24);
            rdoCenterRadius.TabIndex = 1;
            rdoCenterRadius.TabStop = true;
            rdoCenterRadius.Text = "Center + Radius";
            rdoCenterRadius.UseVisualStyleBackColor = true;
            // 
            // rdoBoundingBox
            // 
            rdoBoundingBox.AutoSize = true;
            rdoBoundingBox.Location = new Point(244, 5);
            rdoBoundingBox.Margin = new Padding(3, 5, 3, 4);
            rdoBoundingBox.Name = "rdoBoundingBox";
            rdoBoundingBox.Size = new Size(123, 24);
            rdoBoundingBox.TabIndex = 2;
            rdoBoundingBox.Text = "Bounding Box";
            rdoBoundingBox.UseVisualStyleBackColor = true;
            // 
            // rdoLiveTiles
            // 
            rdoLiveTiles.AutoSize = true;
            rdoLiveTiles.Location = new Point(373, 5);
            rdoLiveTiles.Margin = new Padding(3, 5, 3, 4);
            rdoLiveTiles.Name = "rdoLiveTiles";
            rdoLiveTiles.Size = new Size(90, 24);
            rdoLiveTiles.TabIndex = 3;
            rdoLiveTiles.Text = "Live Tiles";
            rdoLiveTiles.UseVisualStyleBackColor = true;
            rdoLiveTiles.CheckedChanged += rdoLiveTiles_CheckedChanged;
            // 
            // pnlBoundsContainer
            // 
            layout.SetColumnSpan(pnlBoundsContainer, 2);
            pnlBoundsContainer.Controls.Add(pnlLiveTilesInfo);
            pnlBoundsContainer.Controls.Add(pnlBoundingBox);
            pnlBoundsContainer.Controls.Add(pnlCenterRadius);
            pnlBoundsContainer.Dock = DockStyle.Fill;
            pnlBoundsContainer.Location = new Point(13, 193);
            pnlBoundsContainer.Margin = new Padding(3, 0, 3, 0);
            pnlBoundsContainer.Name = "pnlBoundsContainer";
            pnlBoundsContainer.Size = new Size(574, 191);
            pnlBoundsContainer.TabIndex = 7;
            // 
            // pnlLiveTilesInfo
            // 
            pnlLiveTilesInfo.Controls.Add(lblLiveTilesInfo);
            pnlLiveTilesInfo.Dock = DockStyle.Fill;
            pnlLiveTilesInfo.Location = new Point(0, 0);
            pnlLiveTilesInfo.Name = "pnlLiveTilesInfo";
            pnlLiveTilesInfo.Size = new Size(574, 191);
            pnlLiveTilesInfo.TabIndex = 2;
            pnlLiveTilesInfo.Visible = false;
            // 
            // lblLiveTilesInfo
            // 
            lblLiveTilesInfo.Dock = DockStyle.Fill;
            lblLiveTilesInfo.ForeColor = SystemColors.GrayText;
            lblLiveTilesInfo.Location = new Point(0, 0);
            lblLiveTilesInfo.Name = "lblLiveTilesInfo";
            lblLiveTilesInfo.Padding = new Padding(12, 16, 12, 0);
            lblLiveTilesInfo.Size = new Size(574, 191);
            lblLiveTilesInfo.TabIndex = 0;
            lblLiveTilesInfo.Text = resources.GetString("lblLiveTilesInfo.Text");
            // 
            // pnlBoundingBox
            // 
            pnlBoundingBox.Controls.Add(lblNorth);
            pnlBoundingBox.Controls.Add(numNorth);
            pnlBoundingBox.Controls.Add(lblWest);
            pnlBoundingBox.Controls.Add(numWest);
            pnlBoundingBox.Controls.Add(pnlMapRect);
            pnlBoundingBox.Controls.Add(lblEast);
            pnlBoundingBox.Controls.Add(numEast);
            pnlBoundingBox.Controls.Add(lblSouth);
            pnlBoundingBox.Controls.Add(numSouth);
            pnlBoundingBox.Dock = DockStyle.Fill;
            pnlBoundingBox.Location = new Point(0, 0);
            pnlBoundingBox.Name = "pnlBoundingBox";
            pnlBoundingBox.Size = new Size(574, 191);
            pnlBoundingBox.TabIndex = 0;
            pnlBoundingBox.Visible = false;
            // 
            // lblNorth
            // 
            lblNorth.AutoSize = true;
            lblNorth.Location = new Point(260, 8);
            lblNorth.Name = "lblNorth";
            lblNorth.Size = new Size(37, 20);
            lblNorth.TabIndex = 0;
            lblNorth.Text = "N ▲";
            // 
            // numNorth
            // 
            numNorth.DecimalPlaces = 8;
            numNorth.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numNorth.Location = new Point(224, 30);
            numNorth.Margin = new Padding(3, 4, 3, 4);
            numNorth.Maximum = new decimal(new int[] { -84821714, 1, 0, 524288 });
            numNorth.Minimum = new decimal(new int[] { -84821714, 1, 0, -2146959360 });
            numNorth.Name = "numNorth";
            numNorth.Size = new Size(124, 27);
            numNorth.TabIndex = 1;
            numNorth.Value = new decimal(new int[] { 285, 0, 0, 65536 });
            // 
            // lblWest
            // 
            lblWest.AutoSize = true;
            lblWest.Location = new Point(32, 87);
            lblWest.Name = "lblWest";
            lblWest.Size = new Size(40, 20);
            lblWest.TabIndex = 2;
            lblWest.Text = "◀ W";
            // 
            // numWest
            // 
            numWest.DecimalPlaces = 8;
            numWest.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numWest.Location = new Point(76, 84);
            numWest.Margin = new Padding(3, 4, 3, 4);
            numWest.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            numWest.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
            numWest.Name = "numWest";
            numWest.Size = new Size(124, 27);
            numWest.TabIndex = 3;
            numWest.Value = new decimal(new int[] { 840, 0, 0, 65536 });
            // 
            // pnlMapRect
            // 
            pnlMapRect.BackColor = Color.FromArgb(240, 245, 255);
            pnlMapRect.BorderStyle = BorderStyle.FixedSingle;
            pnlMapRect.Controls.Add(lblMapRect);
            pnlMapRect.Location = new Point(206, 64);
            pnlMapRect.Name = "pnlMapRect";
            pnlMapRect.Size = new Size(161, 58);
            pnlMapRect.TabIndex = 4;
            // 
            // lblMapRect
            // 
            lblMapRect.ForeColor = Color.FromArgb(100, 120, 160);
            lblMapRect.Location = new Point(24, 18);
            lblMapRect.Name = "lblMapRect";
            lblMapRect.Size = new Size(100, 23);
            lblMapRect.TabIndex = 0;
            lblMapRect.Text = "XYZ Map";
            lblMapRect.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblEast
            // 
            lblEast.AutoSize = true;
            lblEast.Location = new Point(503, 86);
            lblEast.Name = "lblEast";
            lblEast.Size = new Size(34, 20);
            lblEast.TabIndex = 5;
            lblEast.Text = "E ▶";
            // 
            // numEast
            // 
            numEast.DecimalPlaces = 8;
            numEast.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numEast.Location = new Point(373, 83);
            numEast.Margin = new Padding(3, 4, 3, 4);
            numEast.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            numEast.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
            numEast.Name = "numEast";
            numEast.Size = new Size(124, 27);
            numEast.TabIndex = 6;
            numEast.Value = new decimal(new int[] { 850, 0, 0, 65536 });
            // 
            // lblSouth
            // 
            lblSouth.AutoSize = true;
            lblSouth.Location = new Point(263, 160);
            lblSouth.Name = "lblSouth";
            lblSouth.Size = new Size(34, 20);
            lblSouth.TabIndex = 7;
            lblSouth.Text = "S ▼";
            // 
            // numSouth
            // 
            numSouth.DecimalPlaces = 8;
            numSouth.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numSouth.Location = new Point(224, 129);
            numSouth.Margin = new Padding(3, 4, 3, 4);
            numSouth.Maximum = new decimal(new int[] { -84821714, 1, 0, 524288 });
            numSouth.Minimum = new decimal(new int[] { -84821714, 1, 0, -2146959360 });
            numSouth.Name = "numSouth";
            numSouth.Size = new Size(124, 27);
            numSouth.TabIndex = 8;
            numSouth.Value = new decimal(new int[] { 272, 0, 0, 65536 });
            // 
            // pnlCenterRadius
            // 
            pnlCenterRadius.Controls.Add(lblCenterLat);
            pnlCenterRadius.Controls.Add(numCenterLat);
            pnlCenterRadius.Controls.Add(lblCenterLon);
            pnlCenterRadius.Controls.Add(numCenterLon);
            pnlCenterRadius.Controls.Add(lblRadius);
            pnlCenterRadius.Controls.Add(numRadius);
            pnlCenterRadius.Controls.Add(lblRadiusUnit);
            pnlCenterRadius.Controls.Add(lblAreaHint);
            pnlCenterRadius.Dock = DockStyle.Fill;
            pnlCenterRadius.Location = new Point(0, 0);
            pnlCenterRadius.Name = "pnlCenterRadius";
            pnlCenterRadius.Size = new Size(574, 191);
            pnlCenterRadius.TabIndex = 1;
            // 
            // lblCenterLat
            // 
            lblCenterLat.AutoSize = true;
            lblCenterLat.Location = new Point(12, 13);
            lblCenterLat.Name = "lblCenterLat";
            lblCenterLat.Size = new Size(66, 20);
            lblCenterLat.TabIndex = 0;
            lblCenterLat.Text = "Latitude:";
            // 
            // numCenterLat
            // 
            numCenterLat.DecimalPlaces = 8;
            numCenterLat.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numCenterLat.Location = new Point(88, 9);
            numCenterLat.Margin = new Padding(3, 4, 3, 4);
            numCenterLat.Maximum = new decimal(new int[] { -84821714, 1, 0, 524288 });
            numCenterLat.Minimum = new decimal(new int[] { -84821714, 1, 0, -2146959360 });
            numCenterLat.Name = "numCenterLat";
            numCenterLat.Size = new Size(145, 27);
            numCenterLat.TabIndex = 1;
            numCenterLat.Value = new decimal(new int[] { 277, 0, 0, 65536 });
            // 
            // lblCenterLon
            // 
            lblCenterLon.AutoSize = true;
            lblCenterLon.Location = new Point(252, 13);
            lblCenterLon.Name = "lblCenterLon";
            lblCenterLon.Size = new Size(79, 20);
            lblCenterLon.TabIndex = 2;
            lblCenterLon.Text = "Longitude:";
            // 
            // numCenterLon
            // 
            numCenterLon.DecimalPlaces = 8;
            numCenterLon.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numCenterLon.Location = new Point(338, 9);
            numCenterLon.Margin = new Padding(3, 4, 3, 4);
            numCenterLon.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            numCenterLon.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
            numCenterLon.Name = "numCenterLon";
            numCenterLon.Size = new Size(145, 27);
            numCenterLon.TabIndex = 3;
            numCenterLon.Value = new decimal(new int[] { 853, 0, 0, 65536 });
            // 
            // lblRadius
            // 
            lblRadius.AutoSize = true;
            lblRadius.Location = new Point(12, 55);
            lblRadius.Name = "lblRadius";
            lblRadius.Size = new Size(56, 20);
            lblRadius.TabIndex = 4;
            lblRadius.Text = "Radius:";
            // 
            // numRadius
            // 
            numRadius.DecimalPlaces = 3;
            numRadius.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numRadius.Location = new Point(88, 51);
            numRadius.Margin = new Padding(3, 4, 3, 4);
            numRadius.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numRadius.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numRadius.Name = "numRadius";
            numRadius.Size = new Size(95, 27);
            numRadius.TabIndex = 5;
            numRadius.Value = new decimal(new int[] { 100, 0, 0, 65536 });
            // 
            // lblRadiusUnit
            // 
            lblRadiusUnit.AutoSize = true;
            lblRadiusUnit.Location = new Point(189, 55);
            lblRadiusUnit.Name = "lblRadiusUnit";
            lblRadiusUnit.Size = new Size(29, 20);
            lblRadiusUnit.TabIndex = 6;
            lblRadiusUnit.Text = "km";
            // 
            // lblAreaHint
            // 
            lblAreaHint.AutoSize = true;
            lblAreaHint.ForeColor = SystemColors.GrayText;
            lblAreaHint.Location = new Point(228, 55);
            lblAreaHint.Name = "lblAreaHint";
            lblAreaHint.Size = new Size(152, 20);
            lblAreaHint.TabIndex = 7;
            lblAreaHint.Text = "≈ 20.0 × 18.0 km area";
            // 
            // lblZoomLevel
            // 
            lblZoomLevel.Dock = DockStyle.Fill;
            lblZoomLevel.Location = new Point(13, 384);
            lblZoomLevel.Name = "lblZoomLevel";
            lblZoomLevel.Size = new Size(104, 37);
            lblZoomLevel.TabIndex = 8;
            lblZoomLevel.Text = "Zoom level:";
            lblZoomLevel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numZoomLevel
            // 
            numZoomLevel.Location = new Point(123, 389);
            numZoomLevel.Margin = new Padding(3, 5, 3, 4);
            numZoomLevel.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
            numZoomLevel.Name = "numZoomLevel";
            numZoomLevel.Size = new Size(52, 27);
            numZoomLevel.TabIndex = 9;
            numZoomLevel.Value = new decimal(new int[] { 14, 0, 0, 0 });
            // 
            // lblDownloadStatus
            // 
            layout.SetColumnSpan(lblDownloadStatus, 2);
            lblDownloadStatus.Dock = DockStyle.Fill;
            lblDownloadStatus.ForeColor = SystemColors.GrayText;
            lblDownloadStatus.Location = new Point(13, 421);
            lblDownloadStatus.Name = "lblDownloadStatus";
            lblDownloadStatus.Size = new Size(574, 39);
            lblDownloadStatus.TabIndex = 10;
            lblDownloadStatus.Text = "Download tiles to enable Import.";
            lblDownloadStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressTileDownload
            // 
            layout.SetColumnSpan(progressTileDownload, 2);
            progressTileDownload.Dock = DockStyle.Fill;
            progressTileDownload.Location = new Point(13, 462);
            progressTileDownload.Margin = new Padding(3, 2, 3, 2);
            progressTileDownload.Name = "progressTileDownload";
            progressTileDownload.Size = new Size(574, 22);
            progressTileDownload.TabIndex = 11;
            // 
            // buttonLayout
            // 
            layout.SetColumnSpan(buttonLayout, 2);
            buttonLayout.Controls.Add(btnImport);
            buttonLayout.Controls.Add(btnDownloadTiles);
            buttonLayout.Controls.Add(btnCancel);
            buttonLayout.Dock = DockStyle.Fill;
            buttonLayout.FlowDirection = FlowDirection.RightToLeft;
            buttonLayout.Location = new Point(13, 490);
            buttonLayout.Margin = new Padding(3, 4, 3, 4);
            buttonLayout.Name = "buttonLayout";
            buttonLayout.Size = new Size(574, 87);
            buttonLayout.TabIndex = 12;
            // 
            // btnImport
            // 
            btnImport.DialogResult = DialogResult.OK;
            btnImport.Enabled = false;
            btnImport.Location = new Point(481, 4);
            btnImport.Margin = new Padding(3, 4, 3, 4);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(90, 36);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnDownloadTiles
            // 
            btnDownloadTiles.Location = new Point(385, 4);
            btnDownloadTiles.Margin = new Padding(3, 4, 3, 4);
            btnDownloadTiles.Name = "btnDownloadTiles";
            btnDownloadTiles.Size = new Size(90, 36);
            btnDownloadTiles.TabIndex = 2;
            btnDownloadTiles.Text = "Download";
            btnDownloadTiles.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(289, 4);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 36);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmXyzTileImportOptions
            // 
            AcceptButton = btnImport;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(600, 591);
            Controls.Add(layout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmXyzTileImportOptions";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Import XYZ Tiles";
            layout.ResumeLayout(false);
            layout.PerformLayout();
            sourceLayout.ResumeLayout(false);
            pnlBoundsHeader.ResumeLayout(false);
            pnlBoundsHeader.PerformLayout();
            pnlBoundsContainer.ResumeLayout(false);
            pnlLiveTilesInfo.ResumeLayout(false);
            pnlBoundingBox.ResumeLayout(false);
            pnlBoundingBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numNorth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWest).EndInit();
            pnlMapRect.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numEast).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSouth).EndInit();
            pnlCenterRadius.ResumeLayout(false);
            pnlCenterRadius.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCenterLat).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCenterLon).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRadius).EndInit();
            ((System.ComponentModel.ISupportInitialize)numZoomLevel).EndInit();
            buttonLayout.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
