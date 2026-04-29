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
            grpSourceProjection = new GroupBox();
            projectionLayout = new TableLayoutPanel();
            rdoDetectedCrs = new RadioButton();
            lblDefineSourceCrs = new Label();
            cmbSourceCrs = new ComboBox();
            txtCustomCrs = new TextBox();
            lblProjectionHint = new Label();
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
            previewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            buttonPanel.SuspendLayout();
            grpSourceProjection.SuspendLayout();
            projectionLayout.SuspendLayout();
            grpRasterDetails.SuspendLayout();
            detailsLayout.SuspendLayout();
            mainLayout.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(15, 42);
            label1.Name = "label1";
            label1.Size = new Size(100, 24);
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
            previewPanel.Location = new Point(15, 69);
            previewPanel.Name = "previewPanel";
            mainLayout.SetRowSpan(previewPanel, 4);
            previewPanel.Size = new Size(272, 247);
            previewPanel.TabIndex = 0;
            // 
            // picPreview
            // 
            picPreview.BackColor = Color.WhiteSmoke;
            picPreview.Dock = DockStyle.Fill;
            picPreview.Location = new Point(0, 0);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(270, 245);
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
            lblPreviewFallback.Size = new Size(270, 245);
            lblPreviewFallback.TabIndex = 1;
            lblPreviewFallback.Text = "Low-quality raster preview";
            lblPreviewFallback.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // buttonPanel
            // 
            mainLayout.SetColumnSpan(buttonPanel, 2);
            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(293, 492);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(419, 55);
            buttonPanel.TabIndex = 6;
            // 
            // btnImport
            // 
            btnImport.DialogResult = DialogResult.OK;
            btnImport.Location = new Point(341, 3);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(75, 28);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(260, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 28);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // grpSourceProjection
            // 
            mainLayout.SetColumnSpan(grpSourceProjection, 2);
            grpSourceProjection.Controls.Add(projectionLayout);
            grpSourceProjection.Dock = DockStyle.Fill;
            grpSourceProjection.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpSourceProjection.Location = new Point(293, 247);
            grpSourceProjection.Name = "grpSourceProjection";
            grpSourceProjection.Size = new Size(419, 239);
            grpSourceProjection.TabIndex = 5;
            grpSourceProjection.TabStop = false;
            grpSourceProjection.Text = "Source CRS and projection";
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
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            projectionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            projectionLayout.Size = new Size(413, 217);
            projectionLayout.TabIndex = 0;
            // 
            // rdoDetectedCrs
            // 
            rdoDetectedCrs.AutoSize = true;
            rdoDetectedCrs.Dock = DockStyle.Fill;
            rdoDetectedCrs.Font = new Font("Segoe UI", 9F);
            rdoDetectedCrs.Location = new Point(11, 7);
            rdoDetectedCrs.Name = "rdoDetectedCrs";
            rdoDetectedCrs.Size = new Size(391, 18);
            rdoDetectedCrs.TabIndex = 0;
            rdoDetectedCrs.TabStop = true;
            rdoDetectedCrs.Text = "Use detected raster CRS";
            rdoDetectedCrs.UseVisualStyleBackColor = true;
            // 
            // lblDefineSourceCrs
            // 
            lblDefineSourceCrs.Dock = DockStyle.Fill;
            lblDefineSourceCrs.Font = new Font("Segoe UI", 9F);
            lblDefineSourceCrs.ForeColor = Color.DimGray;
            lblDefineSourceCrs.Location = new Point(11, 28);
            lblDefineSourceCrs.Name = "lblDefineSourceCrs";
            lblDefineSourceCrs.Size = new Size(391, 22);
            lblDefineSourceCrs.TabIndex = 1;
            lblDefineSourceCrs.Text = "Define the source CRS of not already defined:";
            lblDefineSourceCrs.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbSourceCrs
            // 
            cmbSourceCrs.Dock = DockStyle.Fill;
            cmbSourceCrs.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSourceCrs.Font = new Font("Segoe UI", 9F);
            cmbSourceCrs.FormattingEnabled = true;
            cmbSourceCrs.Location = new Point(11, 53);
            cmbSourceCrs.Name = "cmbSourceCrs";
            cmbSourceCrs.Size = new Size(391, 23);
            cmbSourceCrs.TabIndex = 2;
            // 
            // txtCustomCrs
            // 
            txtCustomCrs.Dock = DockStyle.Fill;
            txtCustomCrs.Font = new Font("Segoe UI", 9F);
            txtCustomCrs.Location = new Point(11, 81);
            txtCustomCrs.Multiline = true;
            txtCustomCrs.Name = "txtCustomCrs";
            txtCustomCrs.PlaceholderText = "Custom EPSG code or WKT text";
            txtCustomCrs.Size = new Size(391, 84);
            txtCustomCrs.TabIndex = 3;
            // 
            // lblProjectionHint
            // 
            lblProjectionHint.Dock = DockStyle.Fill;
            lblProjectionHint.ForeColor = Color.DimGray;
            lblProjectionHint.Location = new Point(11, 168);
            lblProjectionHint.Name = "lblProjectionHint";
            lblProjectionHint.Size = new Size(391, 43);
            lblProjectionHint.TabIndex = 4;
            // 
            // grpRasterDetails
            // 
            mainLayout.SetColumnSpan(grpRasterDetails, 2);
            grpRasterDetails.Controls.Add(detailsLayout);
            grpRasterDetails.Dock = DockStyle.Fill;
            grpRasterDetails.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpRasterDetails.Location = new Point(293, 69);
            grpRasterDetails.Name = "grpRasterDetails";
            grpRasterDetails.Size = new Size(419, 172);
            grpRasterDetails.TabIndex = 4;
            grpRasterDetails.TabStop = false;
            grpRasterDetails.Text = "Raster details";
            // 
            // detailsLayout
            // 
            detailsLayout.ColumnCount = 2;
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 121F));
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
            detailsLayout.Size = new Size(413, 150);
            detailsLayout.TabIndex = 0;
            // 
            // lblSourceCaption
            // 
            lblSourceCaption.Location = new Point(9, 4);
            lblSourceCaption.Name = "lblSourceCaption";
            lblSourceCaption.Size = new Size(100, 19);
            lblSourceCaption.TabIndex = 0;
            // 
            // lblSourceValue
            // 
            lblSourceValue.Location = new Point(130, 4);
            lblSourceValue.Name = "lblSourceValue";
            lblSourceValue.Size = new Size(100, 19);
            lblSourceValue.TabIndex = 1;
            // 
            // lblSizeCaption
            // 
            lblSizeCaption.Location = new Point(9, 27);
            lblSizeCaption.Name = "lblSizeCaption";
            lblSizeCaption.Size = new Size(100, 19);
            lblSizeCaption.TabIndex = 2;
            // 
            // lblSizeValue
            // 
            lblSizeValue.Location = new Point(130, 27);
            lblSizeValue.Name = "lblSizeValue";
            lblSizeValue.Size = new Size(100, 19);
            lblSizeValue.TabIndex = 3;
            // 
            // lblDriverCaption
            // 
            lblDriverCaption.Location = new Point(9, 50);
            lblDriverCaption.Name = "lblDriverCaption";
            lblDriverCaption.Size = new Size(100, 19);
            lblDriverCaption.TabIndex = 4;
            // 
            // lblDriverValue
            // 
            lblDriverValue.Location = new Point(130, 50);
            lblDriverValue.Name = "lblDriverValue";
            lblDriverValue.Size = new Size(100, 19);
            lblDriverValue.TabIndex = 5;
            // 
            // lblGeoCaption
            // 
            lblGeoCaption.Location = new Point(9, 73);
            lblGeoCaption.Name = "lblGeoCaption";
            lblGeoCaption.Size = new Size(100, 19);
            lblGeoCaption.TabIndex = 6;
            // 
            // lblGeoValue
            // 
            lblGeoValue.Location = new Point(130, 73);
            lblGeoValue.Name = "lblGeoValue";
            lblGeoValue.Size = new Size(100, 19);
            lblGeoValue.TabIndex = 7;
            // 
            // lblRasterCrsCaption
            // 
            lblRasterCrsCaption.Location = new Point(9, 96);
            lblRasterCrsCaption.Name = "lblRasterCrsCaption";
            lblRasterCrsCaption.Size = new Size(100, 19);
            lblRasterCrsCaption.TabIndex = 8;
            // 
            // lblRasterCrsValue
            // 
            lblRasterCrsValue.Location = new Point(130, 96);
            lblRasterCrsValue.Name = "lblRasterCrsValue";
            lblRasterCrsValue.Size = new Size(100, 19);
            lblRasterCrsValue.TabIndex = 9;
            // 
            // lblProjectCrsCaption
            // 
            lblProjectCrsCaption.Location = new Point(9, 119);
            lblProjectCrsCaption.Name = "lblProjectCrsCaption";
            lblProjectCrsCaption.Size = new Size(100, 21);
            lblProjectCrsCaption.TabIndex = 10;
            // 
            // lblProjectCrsValue
            // 
            lblProjectCrsValue.Location = new Point(130, 119);
            lblProjectCrsValue.Name = "lblProjectCrsValue";
            lblProjectCrsValue.Size = new Size(100, 21);
            lblProjectCrsValue.TabIndex = 11;
            // 
            // txtLayerName
            // 
            txtLayerName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtLayerName.Location = new Point(405, 45);
            txtLayerName.Multiline = true;
            txtLayerName.Name = "txtLayerName";
            txtLayerName.Size = new Size(307, 18);
            txtLayerName.TabIndex = 3;
            // 
            // lblLayerName
            // 
            lblLayerName.AutoSize = true;
            lblLayerName.Dock = DockStyle.Fill;
            lblLayerName.Location = new Point(293, 42);
            lblLayerName.Name = "lblLayerName";
            lblLayerName.Size = new Size(106, 24);
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
            lblTitle.Location = new Point(293, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(419, 30);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Review Raster Import and Projection";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 3;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 278F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
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
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(12);
            mainLayout.RowCount = 5;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 178F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 61F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            mainLayout.Size = new Size(727, 582);
            mainLayout.TabIndex = 0;
            mainLayout.Paint += mainLayout_Paint;
            // 
            // frmRasterImportReview
            // 
            AcceptButton = btnImport;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(727, 582);
            Controls.Add(mainLayout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
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
            grpSourceProjection.ResumeLayout(false);
            projectionLayout.ResumeLayout(false);
            projectionLayout.PerformLayout();
            grpRasterDetails.ResumeLayout(false);
            detailsLayout.ResumeLayout(false);
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
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
    }
}
