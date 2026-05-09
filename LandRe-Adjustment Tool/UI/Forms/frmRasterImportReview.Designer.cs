namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmRasterImportReview
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Cleans up designer resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes the raster import review form layout.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            previewPanel = new Panel();
            picPreview = new PictureBox();
            lblPreviewFallback = new Label();
            buttonPanel = new FlowLayoutPanel();
            btnImport = new Button();
            btnCancel = new Button();
            grpRasterDetails = new GroupBox();
            detailsLayout = new TableLayoutPanel();
            lblSourceCaption = new Label();
            lblSourceValue = new Label();
            lblSizeCaption = new Label();
            lblSizeValue = new Label();
            lblDriverCaption = new Label();
            lblDriverValue = new Label();
            lblGeoCaption = new Label();
            lblGeoValue = new Label();
            lblRasterCrsCaption = new Label();
            lblRasterCrsValue = new Label();
            lblProjectCrsCaption = new Label();
            lblProjectCrsValue = new Label();
            txtLayerName = new TextBox();
            lblLayerName = new Label();
            lblTitle = new Label();
            mainLayout = new TableLayoutPanel();
            grpSourceProjection = new GroupBox();
            projectionLayout = new TableLayoutPanel();
            rdoDetectedCrs = new RadioButton();
            rdoDefineSourceCrs = new RadioButton();
            lblDefineSourceCrs = new Label();
            cmbSourceCrs = new ComboBox();
            txtCustomCrs = new TextBox();
            lblProjectionHint = new Label();
            previewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            buttonPanel.SuspendLayout();
            grpRasterDetails.SuspendLayout();
            detailsLayout.SuspendLayout();
            mainLayout.SuspendLayout();
            grpSourceProjection.SuspendLayout();
            projectionLayout.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(17, 56);
            label1.Name = "label1";
            label1.Size = new Size(114, 32);
            label1.TabIndex = 7;
            label1.Text = "Raster Preview";
            label1.TextAlign = ContentAlignment.BottomLeft;
            // 
            // previewPanel
            // 
            previewPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            previewPanel.BackColor = Color.White;
            previewPanel.BorderStyle = BorderStyle.FixedSingle;
            previewPanel.Controls.Add(picPreview);
            previewPanel.Controls.Add(lblPreviewFallback);
            previewPanel.Location = new Point(17, 92);
            previewPanel.Margin = new Padding(3, 4, 3, 4);
            previewPanel.Name = "previewPanel";
            mainLayout.SetRowSpan(previewPanel, 3);
            previewPanel.Size = new Size(312, 329);
            previewPanel.TabIndex = 0;
            // 
            // picPreview
            // 
            picPreview.BackColor = Color.WhiteSmoke;
            picPreview.BorderStyle = BorderStyle.FixedSingle;
            picPreview.Dock = DockStyle.Fill;
            picPreview.Location = new Point(0, 0);
            picPreview.Margin = new Padding(3, 4, 3, 4);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(310, 327);
            picPreview.TabIndex = 0;
            picPreview.TabStop = false;
            // 
            // lblPreviewFallback
            // 
            lblPreviewFallback.Dock = DockStyle.Fill;
            lblPreviewFallback.ForeColor = Color.DimGray;
            lblPreviewFallback.Location = new Point(0, 0);
            lblPreviewFallback.Name = "lblPreviewFallback";
            lblPreviewFallback.Padding = new Padding(14, 16, 14, 16);
            lblPreviewFallback.Size = new Size(310, 327);
            lblPreviewFallback.TabIndex = 1;
            lblPreviewFallback.Text = "Low-quality raster preview";
            lblPreviewFallback.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // buttonPanel
            // 
            mainLayout.SetColumnSpan(buttonPanel, 2);
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(335, 568);
            buttonPanel.Margin = new Padding(3, 4, 3, 4);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(479, 45);
            buttonPanel.TabIndex = 6;
            // 
            // btnImport
            // 
            btnImport.DialogResult = DialogResult.OK;
            btnImport.Location = new Point(390, 4);
            btnImport.Margin = new Padding(3, 4, 3, 4);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(86, 37);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(298, 4);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(86, 37);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // grpRasterDetails
            // 
            mainLayout.SetColumnSpan(grpRasterDetails, 2);
            grpRasterDetails.Controls.Add(detailsLayout);
            grpRasterDetails.Dock = DockStyle.Fill;
            grpRasterDetails.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpRasterDetails.Location = new Point(335, 92);
            grpRasterDetails.Margin = new Padding(3, 4, 3, 4);
            grpRasterDetails.Name = "grpRasterDetails";
            grpRasterDetails.Padding = new Padding(3, 4, 3, 4);
            grpRasterDetails.Size = new Size(479, 229);
            grpRasterDetails.TabIndex = 4;
            grpRasterDetails.TabStop = false;
            grpRasterDetails.Text = "Raster details";
            // 
            // detailsLayout
            // 
            detailsLayout.ColumnCount = 2;
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138F));
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            detailsLayout.Controls.Add(lblSourceCaption, 0, 0);
            detailsLayout.Controls.Add(lblSourceValue, 1, 0);
            detailsLayout.Controls.Add(lblSizeCaption, 0, 1);
            detailsLayout.Controls.Add(lblSizeValue, 1, 1);
            detailsLayout.Controls.Add(lblDriverCaption, 0, 2);
            detailsLayout.Controls.Add(lblDriverValue, 1, 2);
            detailsLayout.Controls.Add(lblGeoCaption, 0, 3);
            detailsLayout.Controls.Add(lblGeoValue, 1, 3);
            detailsLayout.Controls.Add(lblRasterCrsCaption, 0, 4);
            detailsLayout.Controls.Add(lblRasterCrsValue, 1, 4);
            detailsLayout.Controls.Add(lblProjectCrsCaption, 0, 5);
            detailsLayout.Controls.Add(lblProjectCrsValue, 1, 5);
            detailsLayout.Dock = DockStyle.Fill;
            detailsLayout.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            detailsLayout.Location = new Point(3, 24);
            detailsLayout.Margin = new Padding(3, 4, 3, 4);
            detailsLayout.Name = "detailsLayout";
            detailsLayout.Padding = new Padding(9, 8, 9, 11);
            detailsLayout.RowCount = 6;
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.Size = new Size(473, 201);
            detailsLayout.TabIndex = 0;
            // 
            // lblSourceCaption
            // 
            lblSourceCaption.Dock = DockStyle.Fill;
            lblSourceCaption.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSourceCaption.ForeColor = Color.Black;
            lblSourceCaption.Location = new Point(12, 8);
            lblSourceCaption.Margin = new Padding(3, 0, 9, 0);
            lblSourceCaption.Name = "lblSourceCaption";
            lblSourceCaption.Size = new Size(126, 30);
            lblSourceCaption.TabIndex = 0;
            lblSourceCaption.Text = "Source:";
            lblSourceCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSourceValue
            // 
            lblSourceValue.AutoEllipsis = true;
            lblSourceValue.Dock = DockStyle.Fill;
            lblSourceValue.Location = new Point(150, 8);
            lblSourceValue.Name = "lblSourceValue";
            lblSourceValue.Size = new Size(311, 30);
            lblSourceValue.TabIndex = 1;
            lblSourceValue.TextAlign = ContentAlignment.MiddleLeft;
            lblSourceValue.UseMnemonic = false;
            // 
            // lblSizeCaption
            // 
            lblSizeCaption.Dock = DockStyle.Fill;
            lblSizeCaption.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSizeCaption.ForeColor = Color.Black;
            lblSizeCaption.Location = new Point(12, 38);
            lblSizeCaption.Margin = new Padding(3, 0, 9, 0);
            lblSizeCaption.Name = "lblSizeCaption";
            lblSizeCaption.Size = new Size(126, 30);
            lblSizeCaption.TabIndex = 2;
            lblSizeCaption.Text = "Size:";
            lblSizeCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSizeValue
            // 
            lblSizeValue.AutoEllipsis = true;
            lblSizeValue.Dock = DockStyle.Fill;
            lblSizeValue.Location = new Point(150, 38);
            lblSizeValue.Name = "lblSizeValue";
            lblSizeValue.Size = new Size(311, 30);
            lblSizeValue.TabIndex = 3;
            lblSizeValue.TextAlign = ContentAlignment.MiddleLeft;
            lblSizeValue.UseMnemonic = false;
            // 
            // lblDriverCaption
            // 
            lblDriverCaption.Dock = DockStyle.Fill;
            lblDriverCaption.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDriverCaption.ForeColor = Color.Black;
            lblDriverCaption.Location = new Point(12, 68);
            lblDriverCaption.Margin = new Padding(3, 0, 9, 0);
            lblDriverCaption.Name = "lblDriverCaption";
            lblDriverCaption.Size = new Size(126, 30);
            lblDriverCaption.TabIndex = 4;
            lblDriverCaption.Text = "Driver:";
            lblDriverCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDriverValue
            // 
            lblDriverValue.AutoEllipsis = true;
            lblDriverValue.Dock = DockStyle.Fill;
            lblDriverValue.Location = new Point(150, 68);
            lblDriverValue.Name = "lblDriverValue";
            lblDriverValue.Size = new Size(311, 30);
            lblDriverValue.TabIndex = 5;
            lblDriverValue.TextAlign = ContentAlignment.MiddleLeft;
            lblDriverValue.UseMnemonic = false;
            // 
            // lblGeoCaption
            // 
            lblGeoCaption.Dock = DockStyle.Fill;
            lblGeoCaption.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblGeoCaption.ForeColor = Color.Black;
            lblGeoCaption.Location = new Point(12, 98);
            lblGeoCaption.Margin = new Padding(3, 0, 9, 0);
            lblGeoCaption.Name = "lblGeoCaption";
            lblGeoCaption.Size = new Size(126, 30);
            lblGeoCaption.TabIndex = 6;
            lblGeoCaption.Text = "Georeferenced:";
            lblGeoCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGeoValue
            // 
            lblGeoValue.AutoEllipsis = true;
            lblGeoValue.Dock = DockStyle.Fill;
            lblGeoValue.Location = new Point(150, 98);
            lblGeoValue.Name = "lblGeoValue";
            lblGeoValue.Size = new Size(311, 30);
            lblGeoValue.TabIndex = 7;
            lblGeoValue.TextAlign = ContentAlignment.MiddleLeft;
            lblGeoValue.UseMnemonic = false;
            // 
            // lblRasterCrsCaption
            // 
            lblRasterCrsCaption.Dock = DockStyle.Fill;
            lblRasterCrsCaption.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblRasterCrsCaption.ForeColor = Color.Black;
            lblRasterCrsCaption.Location = new Point(12, 128);
            lblRasterCrsCaption.Margin = new Padding(3, 0, 9, 0);
            lblRasterCrsCaption.Name = "lblRasterCrsCaption";
            lblRasterCrsCaption.Size = new Size(126, 30);
            lblRasterCrsCaption.TabIndex = 8;
            lblRasterCrsCaption.Text = "Raster CRS:";
            lblRasterCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblRasterCrsValue
            // 
            lblRasterCrsValue.AutoEllipsis = true;
            lblRasterCrsValue.Dock = DockStyle.Fill;
            lblRasterCrsValue.Location = new Point(150, 128);
            lblRasterCrsValue.Name = "lblRasterCrsValue";
            lblRasterCrsValue.Size = new Size(311, 30);
            lblRasterCrsValue.TabIndex = 9;
            lblRasterCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            lblRasterCrsValue.UseMnemonic = false;
            // 
            // lblProjectCrsCaption
            // 
            lblProjectCrsCaption.Dock = DockStyle.Fill;
            lblProjectCrsCaption.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblProjectCrsCaption.ForeColor = Color.Black;
            lblProjectCrsCaption.Location = new Point(12, 158);
            lblProjectCrsCaption.Margin = new Padding(3, 0, 9, 0);
            lblProjectCrsCaption.Name = "lblProjectCrsCaption";
            lblProjectCrsCaption.Size = new Size(126, 32);
            lblProjectCrsCaption.TabIndex = 10;
            lblProjectCrsCaption.Text = "Target CRS:";
            lblProjectCrsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProjectCrsValue
            // 
            lblProjectCrsValue.AutoEllipsis = true;
            lblProjectCrsValue.Dock = DockStyle.Fill;
            lblProjectCrsValue.Location = new Point(150, 158);
            lblProjectCrsValue.Name = "lblProjectCrsValue";
            lblProjectCrsValue.Size = new Size(311, 32);
            lblProjectCrsValue.TabIndex = 11;
            lblProjectCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            lblProjectCrsValue.UseMnemonic = false;
            // 
            // txtLayerName
            // 
            txtLayerName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtLayerName.Location = new Point(463, 60);
            txtLayerName.Margin = new Padding(3, 4, 3, 4);
            txtLayerName.Multiline = true;
            txtLayerName.Name = "txtLayerName";
            txtLayerName.Size = new Size(351, 23);
            txtLayerName.TabIndex = 3;
            // 
            // lblLayerName
            // 
            lblLayerName.AutoSize = true;
            lblLayerName.Dock = DockStyle.Fill;
            lblLayerName.Location = new Point(335, 56);
            lblLayerName.Name = "lblLayerName";
            lblLayerName.Size = new Size(122, 32);
            lblLayerName.TabIndex = 2;
            lblLayerName.Text = "Layer name";
            lblLayerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            mainLayout.SetColumnSpan(lblTitle, 2);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitle.Location = new Point(335, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(479, 40);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Review Raster Import and Projection";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 3;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 318F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblTitle, 1, 0);
            mainLayout.Controls.Add(lblLayerName, 1, 1);
            mainLayout.Controls.Add(txtLayerName, 2, 1);
            mainLayout.Controls.Add(grpRasterDetails, 1, 2);
            mainLayout.Controls.Add(grpSourceProjection, 1, 3);
            mainLayout.Controls.Add(buttonPanel, 1, 4);
            mainLayout.Controls.Add(previewPanel, 0, 2);
            mainLayout.Controls.Add(label1, 0, 1);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Margin = new Padding(3, 4, 3, 4);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(14, 16, 14, 16);
            mainLayout.RowCount = 4;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 237F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 53F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            mainLayout.Size = new Size(831, 633);
            mainLayout.TabIndex = 0;
            mainLayout.Paint += mainLayout_Paint;
            // 
            // grpSourceProjection
            // 
            mainLayout.SetColumnSpan(grpSourceProjection, 2);
            grpSourceProjection.Controls.Add(projectionLayout);
            grpSourceProjection.Dock = DockStyle.Fill;
            grpSourceProjection.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpSourceProjection.Location = new Point(335, 329);
            grpSourceProjection.Margin = new Padding(3, 4, 3, 4);
            grpSourceProjection.Name = "grpSourceProjection";
            grpSourceProjection.Padding = new Padding(3, 4, 3, 4);
            grpSourceProjection.Size = new Size(479, 231);
            grpSourceProjection.TabIndex = 5;
            grpSourceProjection.TabStop = false;
            grpSourceProjection.Text = "Source CRS and projection";
            // 
            // projectionLayout
            // 
            projectionLayout.ColumnCount = 1;
            projectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            projectionLayout.Controls.Add(rdoDetectedCrs, 0, 0);
            projectionLayout.Controls.Add(rdoDefineSourceCrs, 0, 1);
            projectionLayout.Controls.Add(lblDefineSourceCrs, 0, 2);
            projectionLayout.Controls.Add(cmbSourceCrs, 0, 3);
            projectionLayout.Controls.Add(txtCustomCrs, 0, 4);
            projectionLayout.Controls.Add(lblProjectionHint, 0, 5);
            projectionLayout.Dock = DockStyle.Fill;
            projectionLayout.Location = new Point(3, 24);
            projectionLayout.Margin = new Padding(3, 4, 3, 4);
            projectionLayout.Name = "projectionLayout";
            projectionLayout.Padding = new Padding(9, 5, 9, 8);
            projectionLayout.RowCount = 6;
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 91F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            projectionLayout.Size = new Size(473, 203);
            projectionLayout.TabIndex = 0;
            // 
            // rdoDetectedCrs
            // 
            rdoDetectedCrs.AutoSize = true;
            rdoDetectedCrs.Dock = DockStyle.Fill;
            rdoDetectedCrs.Font = new Font("Segoe UI", 9F);
            rdoDetectedCrs.Location = new Point(12, 9);
            rdoDetectedCrs.Margin = new Padding(3, 4, 3, 4);
            rdoDetectedCrs.Name = "rdoDetectedCrs";
            rdoDetectedCrs.Size = new Size(449, 24);
            rdoDetectedCrs.TabIndex = 0;
            rdoDetectedCrs.TabStop = true;
            rdoDetectedCrs.Text = "Use detected raster CRS";
            rdoDetectedCrs.UseVisualStyleBackColor = true;
            // 
            // rdoDefineSourceCrs
            // 
            rdoDefineSourceCrs.AutoSize = true;
            rdoDefineSourceCrs.Dock = DockStyle.Fill;
            rdoDefineSourceCrs.Font = new Font("Segoe UI", 9F);
            rdoDefineSourceCrs.Location = new Point(12, 41);
            rdoDefineSourceCrs.Margin = new Padding(3, 4, 3, 4);
            rdoDefineSourceCrs.Name = "rdoDefineSourceCrs";
            rdoDefineSourceCrs.Size = new Size(449, 24);
            rdoDefineSourceCrs.TabIndex = 1;
            rdoDefineSourceCrs.Text = "Define source CRS";
            rdoDefineSourceCrs.UseVisualStyleBackColor = true;
            // 
            // lblDefineSourceCrs
            // 
            lblDefineSourceCrs.Dock = DockStyle.Fill;
            lblDefineSourceCrs.Font = new Font("Segoe UI", 9F);
            lblDefineSourceCrs.ForeColor = Color.DimGray;
            lblDefineSourceCrs.Location = new Point(12, 69);
            lblDefineSourceCrs.Name = "lblDefineSourceCrs";
            lblDefineSourceCrs.Size = new Size(449, 29);
            lblDefineSourceCrs.TabIndex = 2;
            lblDefineSourceCrs.Text = "Select the source CRS to transform from:";
            lblDefineSourceCrs.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbSourceCrs
            // 
            cmbSourceCrs.Dock = DockStyle.Fill;
            cmbSourceCrs.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSourceCrs.Font = new Font("Segoe UI", 9F);
            cmbSourceCrs.FormattingEnabled = true;
            cmbSourceCrs.Location = new Point(12, 102);
            cmbSourceCrs.Margin = new Padding(3, 4, 3, 4);
            cmbSourceCrs.Name = "cmbSourceCrs";
            cmbSourceCrs.Size = new Size(449, 28);
            cmbSourceCrs.TabIndex = 3;
            // 
            // txtCustomCrs
            // 
            txtCustomCrs.Dock = DockStyle.Fill;
            txtCustomCrs.Font = new Font("Segoe UI", 9F);
            txtCustomCrs.Location = new Point(12, 139);
            txtCustomCrs.Margin = new Padding(3, 4, 3, 4);
            txtCustomCrs.Multiline = true;
            txtCustomCrs.Name = "txtCustomCrs";
            txtCustomCrs.PlaceholderText = "Example: EPSG:4326 or 4326";
            txtCustomCrs.ScrollBars = ScrollBars.Vertical;
            txtCustomCrs.Size = new Size(449, 83);
            txtCustomCrs.TabIndex = 4;
            // 
            // lblProjectionHint
            // 
            lblProjectionHint.Dock = DockStyle.Fill;
            lblProjectionHint.ForeColor = Color.DimGray;
            lblProjectionHint.Location = new Point(12, 226);
            lblProjectionHint.Name = "lblProjectionHint";
            lblProjectionHint.Size = new Size(449, 1);
            lblProjectionHint.TabIndex = 5;
            // 
            // frmRasterImportReview
            // 
            AcceptButton = btnImport;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(831, 633);
            Controls.Add(mainLayout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmRasterImportReview";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Raster Import";
            previewPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            buttonPanel.ResumeLayout(false);
            grpRasterDetails.ResumeLayout(false);
            detailsLayout.ResumeLayout(false);
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            grpSourceProjection.ResumeLayout(false);
            projectionLayout.ResumeLayout(false);
            projectionLayout.PerformLayout();
            ResumeLayout(false);
        }

        /// <summary>
        /// Applies common styling to caption labels in the details panel.
        /// </summary>
        private static void ConfigureCaptionLabel(Label label, string text)
        {
            label.AutoSize = false;
            label.Dock = DockStyle.Fill;
            label.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label.ForeColor = Color.DimGray;
            label.Margin = new Padding(3, 0, 8, 0);
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleLeft;
        }

        /// <summary>
        /// Applies common styling to value labels in the details panel.
        /// </summary>
        private static void ConfigureValueLabel(Label label)
        {
            label.AutoEllipsis = true;
            label.AutoSize = false;
            label.Dock = DockStyle.Fill;
            label.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label.ForeColor = Color.Black;
            label.Margin = new Padding(3, 0, 3, 0);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.UseMnemonic = false;
        }

        private Label label1;
        private Panel previewPanel;
        private PictureBox picPreview;
        private Label lblPreviewFallback;
        private TableLayoutPanel mainLayout;
        private Label lblTitle;
        private Label lblLayerName;
        private TextBox txtLayerName;
        private GroupBox grpRasterDetails;
        private TableLayoutPanel detailsLayout;
        private Label lblSourceCaption;
        private Label lblSourceValue;
        private Label lblSizeCaption;
        private Label lblSizeValue;
        private Label lblDriverCaption;
        private Label lblDriverValue;
        private Label lblGeoCaption;
        private Label lblGeoValue;
        private Label lblRasterCrsCaption;
        private Label lblRasterCrsValue;
        private Label lblProjectCrsCaption;
        private Label lblProjectCrsValue;
        private FlowLayoutPanel buttonPanel;
        private Button btnImport;
        private Button btnCancel;
        private GroupBox grpSourceProjection;
        private TableLayoutPanel projectionLayout;
        private RadioButton rdoDetectedCrs;
        private RadioButton rdoDefineSourceCrs;
        private Label lblDefineSourceCrs;
        private ComboBox cmbSourceCrs;
        private TextBox txtCustomCrs;
        private Label lblProjectionHint;
    }
}
