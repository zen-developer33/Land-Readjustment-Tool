namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmRasterImportReview
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Panel previewPanel;
        private PictureBox picPreview;
        private Label lblPreviewFallback;
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
        private GroupBox grpSourceProjection;
        private TableLayoutPanel projectionLayout;
        private RadioButton rdoDetectedCrs;
        private Label lblDefineSourceCrs;
        private ComboBox cmbSourceCrs;
        private TextBox txtCustomCrs;
        private Label lblProjectionHint;
        private FlowLayoutPanel buttonPanel;
        private Button btnImport;
        private Button btnCancel;

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
            components = new System.ComponentModel.Container();
            mainLayout = new TableLayoutPanel();
            previewPanel = new Panel();
            picPreview = new PictureBox();
            lblPreviewFallback = new Label();
            lblTitle = new Label();
            lblLayerName = new Label();
            txtLayerName = new TextBox();
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
            grpSourceProjection = new GroupBox();
            projectionLayout = new TableLayoutPanel();
            rdoDetectedCrs = new RadioButton();
            lblDefineSourceCrs = new Label();
            cmbSourceCrs = new ComboBox();
            txtCustomCrs = new TextBox();
            lblProjectionHint = new Label();
            buttonPanel = new FlowLayoutPanel();
            btnImport = new Button();
            btnCancel = new Button();
            mainLayout.SuspendLayout();
            previewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            grpRasterDetails.SuspendLayout();
            detailsLayout.SuspendLayout();
            grpSourceProjection.SuspendLayout();
            projectionLayout.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 3;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(previewPanel, 0, 0);
            mainLayout.Controls.Add(lblTitle, 1, 0);
            mainLayout.Controls.Add(lblLayerName, 1, 1);
            mainLayout.Controls.Add(txtLayerName, 2, 1);
            mainLayout.Controls.Add(grpRasterDetails, 1, 2);
            mainLayout.Controls.Add(grpSourceProjection, 1, 3);
            mainLayout.Controls.Add(buttonPanel, 1, 4);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(12);
            mainLayout.RowCount = 5;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 178F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            mainLayout.Size = new Size(720, 470);
            mainLayout.TabIndex = 0;
            // 
            // previewPanel
            // 
            previewPanel.BackColor = Color.White;
            previewPanel.BorderStyle = BorderStyle.FixedSingle;
            previewPanel.Controls.Add(picPreview);
            previewPanel.Controls.Add(lblPreviewFallback);
            previewPanel.Dock = DockStyle.Fill;
            previewPanel.Location = new Point(15, 15);
            previewPanel.Name = "previewPanel";
            mainLayout.SetRowSpan(previewPanel, 5);
            previewPanel.Size = new Size(224, 440);
            previewPanel.TabIndex = 0;
            // 
            // picPreview
            // 
            picPreview.BackColor = Color.WhiteSmoke;
            picPreview.Dock = DockStyle.Fill;
            picPreview.Location = new Point(0, 0);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(222, 438);
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 0;
            picPreview.TabStop = false;
            // 
            // lblPreviewFallback
            // 
            lblPreviewFallback.Dock = DockStyle.Fill;
            lblPreviewFallback.ForeColor = Color.DimGray;
            lblPreviewFallback.Location = new Point(0, 0);
            lblPreviewFallback.Name = "lblPreviewFallback";
            lblPreviewFallback.Padding = new Padding(12);
            lblPreviewFallback.Size = new Size(222, 438);
            lblPreviewFallback.TabIndex = 1;
            lblPreviewFallback.Text = "Preview will appear here when the raster format can be shown directly.";
            lblPreviewFallback.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            mainLayout.SetColumnSpan(lblTitle, 2);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitle.Location = new Point(245, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(460, 42);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Review Raster Import";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLayerName
            // 
            lblLayerName.AutoSize = true;
            lblLayerName.Dock = DockStyle.Fill;
            lblLayerName.Location = new Point(245, 54);
            lblLayerName.Name = "lblLayerName";
            lblLayerName.Size = new Size(99, 36);
            lblLayerName.TabIndex = 2;
            lblLayerName.Text = "Layer name";
            lblLayerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtLayerName
            // 
            txtLayerName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtLayerName.Location = new Point(350, 60);
            txtLayerName.Name = "txtLayerName";
            txtLayerName.Size = new Size(355, 23);
            txtLayerName.TabIndex = 3;
            // 
            // grpRasterDetails
            // 
            mainLayout.SetColumnSpan(grpRasterDetails, 2);
            grpRasterDetails.Controls.Add(detailsLayout);
            grpRasterDetails.Dock = DockStyle.Fill;
            grpRasterDetails.Location = new Point(245, 93);
            grpRasterDetails.Name = "grpRasterDetails";
            grpRasterDetails.Size = new Size(460, 172);
            grpRasterDetails.TabIndex = 4;
            grpRasterDetails.TabStop = false;
            grpRasterDetails.Text = "Raster details";
            // 
            // detailsLayout
            // 
            detailsLayout.ColumnCount = 2;
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
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
            detailsLayout.Location = new Point(3, 19);
            detailsLayout.Name = "detailsLayout";
            detailsLayout.Padding = new Padding(6, 4, 6, 6);
            detailsLayout.RowCount = 6;
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            detailsLayout.Size = new Size(454, 150);
            detailsLayout.TabIndex = 0;
            // 
            // detail labels
            // 
            ConfigureCaptionLabel(lblSourceCaption, "Source");
            ConfigureValueLabel(lblSourceValue);
            ConfigureCaptionLabel(lblSizeCaption, "Size");
            ConfigureValueLabel(lblSizeValue);
            ConfigureCaptionLabel(lblDriverCaption, "Driver");
            ConfigureValueLabel(lblDriverValue);
            ConfigureCaptionLabel(lblGeoCaption, "Georeferenced");
            ConfigureValueLabel(lblGeoValue);
            ConfigureCaptionLabel(lblRasterCrsCaption, "Raster CRS");
            ConfigureValueLabel(lblRasterCrsValue);
            ConfigureCaptionLabel(lblProjectCrsCaption, "Project CRS");
            ConfigureValueLabel(lblProjectCrsValue);
            // 
            // grpSourceProjection
            // 
            mainLayout.SetColumnSpan(grpSourceProjection, 2);
            grpSourceProjection.Controls.Add(projectionLayout);
            grpSourceProjection.Dock = DockStyle.Fill;
            grpSourceProjection.Location = new Point(245, 271);
            grpSourceProjection.Name = "grpSourceProjection";
            grpSourceProjection.Size = new Size(460, 142);
            grpSourceProjection.TabIndex = 5;
            grpSourceProjection.TabStop = false;
            grpSourceProjection.Text = "Source projection";
            // 
            // projectionLayout
            // 
            projectionLayout.ColumnCount = 1;
            projectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            projectionLayout.Controls.Add(rdoDetectedCrs, 0, 0);
            projectionLayout.Controls.Add(lblDefineSourceCrs, 0, 1);
            projectionLayout.Controls.Add(cmbSourceCrs, 0, 2);
            projectionLayout.Controls.Add(txtCustomCrs, 0, 3);
            projectionLayout.Controls.Add(lblProjectionHint, 0, 4);
            projectionLayout.Dock = DockStyle.Fill;
            projectionLayout.Location = new Point(3, 19);
            projectionLayout.Name = "projectionLayout";
            projectionLayout.Padding = new Padding(8, 4, 8, 6);
            projectionLayout.RowCount = 5;
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            projectionLayout.Size = new Size(454, 120);
            projectionLayout.TabIndex = 0;
            // 
            // rdoDetectedCrs
            // 
            rdoDetectedCrs.AutoSize = true;
            rdoDetectedCrs.Dock = DockStyle.Fill;
            rdoDetectedCrs.Location = new Point(11, 7);
            rdoDetectedCrs.Name = "rdoDetectedCrs";
            rdoDetectedCrs.Size = new Size(432, 18);
            rdoDetectedCrs.TabIndex = 0;
            rdoDetectedCrs.TabStop = true;
            rdoDetectedCrs.Text = "Use detected raster CRS";
            rdoDetectedCrs.UseVisualStyleBackColor = true;
            // 
            // lblDefineSourceCrs
            // 
            lblDefineSourceCrs.Dock = DockStyle.Fill;
            lblDefineSourceCrs.ForeColor = Color.DimGray;
            lblDefineSourceCrs.Location = new Point(11, 28);
            lblDefineSourceCrs.Name = "lblDefineSourceCrs";
            lblDefineSourceCrs.Size = new Size(432, 22);
            lblDefineSourceCrs.TabIndex = 1;
            lblDefineSourceCrs.Text = "When CRS is missing, define the source CRS";
            lblDefineSourceCrs.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbSourceCrs
            // 
            cmbSourceCrs.Dock = DockStyle.Fill;
            cmbSourceCrs.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSourceCrs.FormattingEnabled = true;
            cmbSourceCrs.Location = new Point(11, 53);
            cmbSourceCrs.Name = "cmbSourceCrs";
            cmbSourceCrs.Size = new Size(432, 23);
            cmbSourceCrs.TabIndex = 2;
            // 
            // txtCustomCrs
            // 
            txtCustomCrs.Dock = DockStyle.Fill;
            txtCustomCrs.Location = new Point(11, 81);
            txtCustomCrs.Name = "txtCustomCrs";
            txtCustomCrs.PlaceholderText = "Custom EPSG code or WKT text";
            txtCustomCrs.Size = new Size(432, 23);
            txtCustomCrs.TabIndex = 3;
            // 
            // lblProjectionHint
            // 
            lblProjectionHint.Dock = DockStyle.Fill;
            lblProjectionHint.ForeColor = Color.DimGray;
            lblProjectionHint.Location = new Point(11, 108);
            lblProjectionHint.Name = "lblProjectionHint";
            lblProjectionHint.Size = new Size(432, 6);
            lblProjectionHint.TabIndex = 4;
            // 
            // buttonPanel
            // 
            mainLayout.SetColumnSpan(buttonPanel, 2);
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(245, 419);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(460, 36);
            buttonPanel.TabIndex = 6;
            // 
            // btnImport
            // 
            btnImport.DialogResult = DialogResult.OK;
            btnImport.Location = new Point(382, 3);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(75, 28);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(301, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 28);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmRasterImportReview
            // 
            AcceptButton = btnImport;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(720, 470);
            Controls.Add(mainLayout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmRasterImportReview";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Raster Import";
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            previewPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            grpRasterDetails.ResumeLayout(false);
            detailsLayout.ResumeLayout(false);
            detailsLayout.PerformLayout();
            grpSourceProjection.ResumeLayout(false);
            projectionLayout.ResumeLayout(false);
            projectionLayout.PerformLayout();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        /// <summary>
        /// Applies common styling to caption labels in the details panel.
        /// </summary>
        private static void ConfigureCaptionLabel(Label label, string text)
        {
            label.AutoSize = true;
            label.Dock = DockStyle.Fill;
            label.ForeColor = Color.DimGray;
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleLeft;
        }

        /// <summary>
        /// Applies common styling to value labels in the details panel.
        /// </summary>
        private static void ConfigureValueLabel(Label label)
        {
            label.AutoEllipsis = true;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
        }
    }
}
