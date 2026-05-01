namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmXyzTileImportOptions
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layout;
        private Label lblLayerName;
        private Label lblTileSource;
        private Label lblSourceUrl;
        private Label lblMinLongitude;
        private Label lblMinLatitude;
        private Label lblMaxLongitude;
        private Label lblMaxLatitude;
        private Label lblZoomLevel;
        private TableLayoutPanel sourceLayout;
        private FlowLayoutPanel buttonLayout;
        private ComboBox cmbTileSource;
        private Button btnManageSources;
        private TextBox txtLayerName;
        private TextBox txtUrlTemplate;
        private NumericUpDown numMinLongitude;
        private NumericUpDown numMinLatitude;
        private NumericUpDown numMaxLongitude;
        private NumericUpDown numMaxLatitude;
        private NumericUpDown numZoomLevel;
        private Label lblDownloadStatus;
        private ProgressBar progressTileDownload;

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
            layout = new TableLayoutPanel();
            lblLayerName = new Label();
            txtLayerName = new TextBox();
            lblTileSource = new Label();
            sourceLayout = new TableLayoutPanel();
            cmbTileSource = new ComboBox();
            btnManageSources = new Button();
            lblSourceUrl = new Label();
            txtUrlTemplate = new TextBox();
            lblMinLongitude = new Label();
            numMinLongitude = new NumericUpDown();
            lblMinLatitude = new Label();
            numMinLatitude = new NumericUpDown();
            lblMaxLongitude = new Label();
            numMaxLongitude = new NumericUpDown();
            lblMaxLatitude = new Label();
            numMaxLatitude = new NumericUpDown();
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
            ((System.ComponentModel.ISupportInitialize)numMinLongitude).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinLatitude).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxLongitude).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxLatitude).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numZoomLevel).BeginInit();
            buttonLayout.SuspendLayout();
            SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Controls.Add(lblLayerName, 0, 0);
            layout.Controls.Add(txtLayerName, 1, 0);
            layout.Controls.Add(lblTileSource, 0, 1);
            layout.Controls.Add(sourceLayout, 1, 1);
            layout.Controls.Add(lblSourceUrl, 0, 2);
            layout.Controls.Add(txtUrlTemplate, 1, 2);
            layout.Controls.Add(lblMinLongitude, 0, 3);
            layout.Controls.Add(numMinLongitude, 1, 3);
            layout.Controls.Add(lblMinLatitude, 0, 4);
            layout.Controls.Add(numMinLatitude, 1, 4);
            layout.Controls.Add(lblMaxLongitude, 0, 5);
            layout.Controls.Add(numMaxLongitude, 1, 5);
            layout.Controls.Add(lblMaxLatitude, 0, 6);
            layout.Controls.Add(numMaxLatitude, 1, 6);
            layout.Controls.Add(lblZoomLevel, 0, 7);
            layout.Controls.Add(numZoomLevel, 1, 7);
            layout.Controls.Add(lblDownloadStatus, 0, 8);
            layout.Controls.Add(progressTileDownload, 0, 9);
            layout.Controls.Add(buttonLayout, 0, 10);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Margin = new Padding(3, 4, 3, 4);
            layout.Name = "layout";
            layout.Padding = new Padding(8);
            layout.RowCount = 11;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layout.Size = new Size(585, 491);
            layout.TabIndex = 0;
            layout.Paint += layout_Paint;
            // 
            // lblLayerName
            // 
            lblLayerName.Dock = DockStyle.Fill;
            lblLayerName.Location = new Point(11, 8);
            lblLayerName.Name = "lblLayerName";
            lblLayerName.Size = new Size(154, 45);
            lblLayerName.TabIndex = 0;
            lblLayerName.Text = "Layer name:";
            lblLayerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtLayerName
            // 
            txtLayerName.Dock = DockStyle.Fill;
            txtLayerName.Location = new Point(171, 12);
            txtLayerName.Margin = new Padding(3, 4, 3, 4);
            txtLayerName.Name = "txtLayerName";
            txtLayerName.Size = new Size(403, 27);
            txtLayerName.TabIndex = 1;
            txtLayerName.Text = "XYZ Basemap";
            // 
            // lblTileSource
            // 
            lblTileSource.Dock = DockStyle.Fill;
            lblTileSource.Location = new Point(11, 53);
            lblTileSource.Name = "lblTileSource";
            lblTileSource.Size = new Size(154, 45);
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
            sourceLayout.Location = new Point(171, 57);
            sourceLayout.Margin = new Padding(3, 4, 3, 4);
            sourceLayout.Name = "sourceLayout";
            sourceLayout.RowCount = 1;
            sourceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            sourceLayout.Size = new Size(403, 37);
            sourceLayout.TabIndex = 3;
            // 
            // cmbTileSource
            // 
            cmbTileSource.DisplayMember = "Name";
            cmbTileSource.Dock = DockStyle.Fill;
            cmbTileSource.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTileSource.FormattingEnabled = true;
            cmbTileSource.Location = new Point(3, 4);
            cmbTileSource.Margin = new Padding(3, 4, 3, 4);
            cmbTileSource.Name = "cmbTileSource";
            cmbTileSource.Size = new Size(303, 28);
            cmbTileSource.TabIndex = 0;
            // 
            // btnManageSources
            // 
            btnManageSources.Dock = DockStyle.Fill;
            btnManageSources.Location = new Point(312, 4);
            btnManageSources.Margin = new Padding(3, 4, 3, 4);
            btnManageSources.Name = "btnManageSources";
            btnManageSources.Size = new Size(88, 29);
            btnManageSources.TabIndex = 1;
            btnManageSources.Text = "Manage...";
            btnManageSources.UseVisualStyleBackColor = true;
            // 
            // lblSourceUrl
            // 
            lblSourceUrl.Dock = DockStyle.Fill;
            lblSourceUrl.Location = new Point(11, 98);
            lblSourceUrl.Name = "lblSourceUrl";
            lblSourceUrl.Size = new Size(154, 90);
            lblSourceUrl.TabIndex = 4;
            lblSourceUrl.Text = "Source URL:";
            // 
            // txtUrlTemplate
            // 
            txtUrlTemplate.Dock = DockStyle.Fill;
            txtUrlTemplate.Location = new Point(171, 102);
            txtUrlTemplate.Margin = new Padding(3, 4, 3, 4);
            txtUrlTemplate.Multiline = true;
            txtUrlTemplate.Name = "txtUrlTemplate";
            txtUrlTemplate.ReadOnly = true;
            txtUrlTemplate.ScrollBars = ScrollBars.Both;
            txtUrlTemplate.Size = new Size(403, 82);
            txtUrlTemplate.TabIndex = 5;
            // 
            // lblMinLongitude
            // 
            lblMinLongitude.Dock = DockStyle.Fill;
            lblMinLongitude.Location = new Point(11, 188);
            lblMinLongitude.Name = "lblMinLongitude";
            lblMinLongitude.Size = new Size(154, 34);
            lblMinLongitude.TabIndex = 6;
            lblMinLongitude.Text = "Min longitude:";
            lblMinLongitude.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numMinLongitude
            // 
            numMinLongitude.DecimalPlaces = 8;
            numMinLongitude.Dock = DockStyle.Left;
            numMinLongitude.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numMinLongitude.Location = new Point(171, 192);
            numMinLongitude.Margin = new Padding(3, 4, 3, 4);
            numMinLongitude.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            numMinLongitude.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
            numMinLongitude.Name = "numMinLongitude";
            numMinLongitude.Size = new Size(135, 27);
            numMinLongitude.TabIndex = 7;
            numMinLongitude.Value = new decimal(new int[] { 84, 0, 0, 0 });
            // 
            // lblMinLatitude
            // 
            lblMinLatitude.Dock = DockStyle.Fill;
            lblMinLatitude.Location = new Point(11, 222);
            lblMinLatitude.Name = "lblMinLatitude";
            lblMinLatitude.Size = new Size(154, 37);
            lblMinLatitude.TabIndex = 8;
            lblMinLatitude.Text = "Min latitude:";
            lblMinLatitude.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numMinLatitude
            // 
            numMinLatitude.DecimalPlaces = 8;
            numMinLatitude.Dock = DockStyle.Left;
            numMinLatitude.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numMinLatitude.Location = new Point(171, 226);
            numMinLatitude.Margin = new Padding(3, 4, 3, 4);
            numMinLatitude.Maximum = new decimal(new int[] { -84821714, 1, 0, 524288 });
            numMinLatitude.Minimum = new decimal(new int[] { -84821714, 1, 0, -2146959360 });
            numMinLatitude.Name = "numMinLatitude";
            numMinLatitude.Size = new Size(135, 27);
            numMinLatitude.TabIndex = 9;
            numMinLatitude.Value = new decimal(new int[] { 275, 0, 0, 65536 });
            // 
            // lblMaxLongitude
            // 
            lblMaxLongitude.Dock = DockStyle.Fill;
            lblMaxLongitude.Location = new Point(11, 259);
            lblMaxLongitude.Name = "lblMaxLongitude";
            lblMaxLongitude.Size = new Size(154, 36);
            lblMaxLongitude.TabIndex = 10;
            lblMaxLongitude.Text = "Max longitude:";
            lblMaxLongitude.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numMaxLongitude
            // 
            numMaxLongitude.DecimalPlaces = 8;
            numMaxLongitude.Dock = DockStyle.Left;
            numMaxLongitude.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numMaxLongitude.Location = new Point(171, 263);
            numMaxLongitude.Margin = new Padding(3, 4, 3, 4);
            numMaxLongitude.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            numMaxLongitude.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
            numMaxLongitude.Name = "numMaxLongitude";
            numMaxLongitude.Size = new Size(135, 27);
            numMaxLongitude.TabIndex = 11;
            numMaxLongitude.Value = new decimal(new int[] { 85, 0, 0, 0 });
            // 
            // lblMaxLatitude
            // 
            lblMaxLatitude.Dock = DockStyle.Fill;
            lblMaxLatitude.Location = new Point(11, 295);
            lblMaxLatitude.Name = "lblMaxLatitude";
            lblMaxLatitude.Size = new Size(154, 35);
            lblMaxLatitude.TabIndex = 12;
            lblMaxLatitude.Text = "Max latitude:";
            lblMaxLatitude.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numMaxLatitude
            // 
            numMaxLatitude.DecimalPlaces = 8;
            numMaxLatitude.Dock = DockStyle.Left;
            numMaxLatitude.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            numMaxLatitude.Location = new Point(171, 299);
            numMaxLatitude.Margin = new Padding(3, 4, 3, 4);
            numMaxLatitude.Maximum = new decimal(new int[] { -84821714, 1, 0, 524288 });
            numMaxLatitude.Minimum = new decimal(new int[] { -84821714, 1, 0, -2146959360 });
            numMaxLatitude.Name = "numMaxLatitude";
            numMaxLatitude.Size = new Size(135, 27);
            numMaxLatitude.TabIndex = 13;
            numMaxLatitude.Value = new decimal(new int[] { 285, 0, 0, 65536 });
            // 
            // lblZoomLevel
            // 
            lblZoomLevel.Dock = DockStyle.Fill;
            lblZoomLevel.Location = new Point(11, 330);
            lblZoomLevel.Name = "lblZoomLevel";
            lblZoomLevel.Size = new Size(154, 34);
            lblZoomLevel.TabIndex = 14;
            lblZoomLevel.Text = "Zoom level:";
            lblZoomLevel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numZoomLevel
            // 
            numZoomLevel.Location = new Point(171, 334);
            numZoomLevel.Margin = new Padding(3, 4, 3, 4);
            numZoomLevel.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
            numZoomLevel.Name = "numZoomLevel";
            numZoomLevel.Size = new Size(45, 27);
            numZoomLevel.TabIndex = 15;
            numZoomLevel.Value = new decimal(new int[] { 14, 0, 0, 0 });
            // 
            // lblDownloadStatus
            // 
            layout.SetColumnSpan(lblDownloadStatus, 2);
            lblDownloadStatus.Dock = DockStyle.Fill;
            lblDownloadStatus.ForeColor = SystemColors.GrayText;
            lblDownloadStatus.Location = new Point(11, 364);
            lblDownloadStatus.Name = "lblDownloadStatus";
            lblDownloadStatus.Size = new Size(563, 34);
            lblDownloadStatus.TabIndex = 17;
            lblDownloadStatus.Text = "Download tiles to enable Import.";
            lblDownloadStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressTileDownload
            // 
            layout.SetColumnSpan(progressTileDownload, 2);
            progressTileDownload.Dock = DockStyle.Fill;
            progressTileDownload.Location = new Point(11, 402);
            progressTileDownload.Margin = new Padding(3, 4, 3, 4);
            progressTileDownload.Name = "progressTileDownload";
            progressTileDownload.Size = new Size(563, 22);
            progressTileDownload.TabIndex = 18;
            // 
            // buttonLayout
            // 
            layout.SetColumnSpan(buttonLayout, 2);
            buttonLayout.Controls.Add(btnImport);
            buttonLayout.Controls.Add(btnDownloadTiles);
            buttonLayout.Controls.Add(btnCancel);
            buttonLayout.Dock = DockStyle.Fill;
            buttonLayout.FlowDirection = FlowDirection.RightToLeft;
            buttonLayout.Location = new Point(11, 432);
            buttonLayout.Margin = new Padding(3, 4, 3, 4);
            buttonLayout.Name = "buttonLayout";
            buttonLayout.Size = new Size(563, 47);
            buttonLayout.TabIndex = 19;
            // 
            // btnImport
            // 
            btnImport.DialogResult = DialogResult.OK;
            btnImport.Enabled = false;
            btnImport.Location = new Point(474, 4);
            btnImport.Margin = new Padding(3, 4, 3, 4);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(86, 37);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnDownloadTiles
            // 
            btnDownloadTiles.Location = new Point(382, 4);
            btnDownloadTiles.Margin = new Padding(3, 4, 3, 4);
            btnDownloadTiles.Name = "btnDownloadTiles";
            btnDownloadTiles.Size = new Size(86, 37);
            btnDownloadTiles.TabIndex = 2;
            btnDownloadTiles.Text = "Download";
            btnDownloadTiles.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(290, 4);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(86, 37);
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
            ClientSize = new Size(585, 491);
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
            ((System.ComponentModel.ISupportInitialize)numMinLongitude).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinLatitude).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxLongitude).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxLatitude).EndInit();
            ((System.ComponentModel.ISupportInitialize)numZoomLevel).EndInit();
            buttonLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Button btnImport;
        private Button btnDownloadTiles;
        private Button btnCancel;
    }
}
