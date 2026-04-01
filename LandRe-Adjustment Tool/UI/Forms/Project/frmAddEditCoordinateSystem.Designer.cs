namespace Land_Readjustment_Tool.UI.Forms.Project
{
    partial class frmAddEditCoordinateSystem
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlFooter = new Panel();
            btnSave = new Button();
            btnCancel = new Button();
            pnlContent = new Panel();
            grpIdentity = new GroupBox();
            lblCode = new Label();
            txtCode = new TextBox();
            lblName = new Label();
            txtName = new TextBox();
            lblEpsg = new Label();
            txtEpsg = new TextBox();
            lblEpsgHint = new Label();
            lblProjectionType = new Label();
            cmbProjectionType = new ComboBox();
            lblRegion = new Label();
            txtRegion = new TextBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            grpProjectionParams = new GroupBox();
            txtWkt = new TextBox();
            lblCentralMeridian = new Label();
            nudCentralMeridian = new NumericUpDown();
            lblLatOrigin = new Label();
            nudLatOrigin = new NumericUpDown();
            lblScaleFactor = new Label();
            nudScaleFactor = new NumericUpDown();
            lblFalseEasting = new Label();
            nudFalseEasting = new NumericUpDown();
            lblFalseNorthing = new Label();
            nudFalseNorthing = new NumericUpDown();
            lblEllipsoid = new Label();
            txtEllipsoid = new TextBox();
            lblSemiMajor = new Label();
            nudSemiMajor = new NumericUpDown();
            lblInvFlat = new Label();
            nudInvFlat = new NumericUpDown();
            lblWkt = new Label();
            pnlFooter.SuspendLayout();
            pnlContent.SuspendLayout();
            grpIdentity.SuspendLayout();
            grpProjectionParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudCentralMeridian).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLatOrigin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudScaleFactor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudFalseEasting).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudFalseNorthing).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSemiMajor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudInvFlat).BeginInit();
            SuspendLayout();
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 633);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(6);
            pnlFooter.Size = new Size(502, 40);
            pnlFooter.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(329, 6);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 28);
            btnSave.TabIndex = 90;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(413, 6);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 28);
            btnCancel.TabIndex = 91;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            // 
            // pnlContent
            // 
            pnlContent.AutoScroll = true;
            pnlContent.Controls.Add(grpIdentity);
            pnlContent.Controls.Add(grpProjectionParams);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10, 8, 10, 4);
            pnlContent.Size = new Size(502, 633);
            pnlContent.TabIndex = 0;
            // 
            // grpIdentity
            // 
            grpIdentity.Controls.Add(lblCode);
            grpIdentity.Controls.Add(txtCode);
            grpIdentity.Controls.Add(lblName);
            grpIdentity.Controls.Add(txtName);
            grpIdentity.Controls.Add(lblEpsg);
            grpIdentity.Controls.Add(txtEpsg);
            grpIdentity.Controls.Add(lblEpsgHint);
            grpIdentity.Controls.Add(lblProjectionType);
            grpIdentity.Controls.Add(cmbProjectionType);
            grpIdentity.Controls.Add(lblRegion);
            grpIdentity.Controls.Add(txtRegion);
            grpIdentity.Controls.Add(lblDescription);
            grpIdentity.Controls.Add(txtDescription);
            grpIdentity.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpIdentity.Location = new Point(10, 8);
            grpIdentity.Name = "grpIdentity";
            grpIdentity.Size = new Size(490, 220);
            grpIdentity.TabIndex = 0;
            grpIdentity.TabStop = false;
            grpIdentity.Text = "Identity";
            // 
            // lblCode
            // 
            lblCode.Font = new Font("Segoe UI", 9F);
            lblCode.Location = new Point(12, 29);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(130, 20);
            lblCode.TabIndex = 0;
            lblCode.Text = "Code:";
            // 
            // txtCode
            // 
            txtCode.Font = new Font("Segoe UI", 9F);
            txtCode.Location = new Point(144, 26);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(330, 27);
            txtCode.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.Location = new Point(12, 61);
            lblName.Name = "lblName";
            lblName.Size = new Size(130, 20);
            lblName.TabIndex = 2;
            lblName.Text = "Name:";
            // 
            // txtName
            // 
            txtName.Font = new Font("Segoe UI", 9F);
            txtName.Location = new Point(144, 58);
            txtName.Name = "txtName";
            txtName.Size = new Size(330, 27);
            txtName.TabIndex = 3;
            // 
            // lblEpsg
            // 
            lblEpsg.Font = new Font("Segoe UI", 9F);
            lblEpsg.Location = new Point(12, 93);
            lblEpsg.Name = "lblEpsg";
            lblEpsg.Size = new Size(130, 20);
            lblEpsg.TabIndex = 4;
            lblEpsg.Text = "EPSG Code:";
            // 
            // txtEpsg
            // 
            txtEpsg.Font = new Font("Segoe UI", 9F);
            txtEpsg.Location = new Point(144, 90);
            txtEpsg.Name = "txtEpsg";
            txtEpsg.Size = new Size(120, 27);
            txtEpsg.TabIndex = 5;
            txtEpsg.TextChanged += txtEpsg_TextChanged;
            // 
            // lblEpsgHint
            // 
            lblEpsgHint.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblEpsgHint.ForeColor = SystemColors.GrayText;
            lblEpsgHint.Location = new Point(270, 93);
            lblEpsgHint.Name = "lblEpsgHint";
            lblEpsgHint.Size = new Size(206, 18);
            lblEpsgHint.TabIndex = 6;
            lblEpsgHint.Text = "Leave blank for MUTM (no EPSG code)";
            // 
            // lblProjectionType
            // 
            lblProjectionType.Font = new Font("Segoe UI", 9F);
            lblProjectionType.Location = new Point(12, 125);
            lblProjectionType.Name = "lblProjectionType";
            lblProjectionType.Size = new Size(130, 20);
            lblProjectionType.TabIndex = 7;
            lblProjectionType.Text = "Projection Type:";
            // 
            // cmbProjectionType
            // 
            cmbProjectionType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjectionType.Font = new Font("Segoe UI", 9F);
            cmbProjectionType.Items.AddRange(new object[] { "TransverseMercator", "Geographic", "LambertConformalConic" });
            cmbProjectionType.Location = new Point(144, 122);
            cmbProjectionType.Name = "cmbProjectionType";
            cmbProjectionType.Size = new Size(240, 28);
            cmbProjectionType.TabIndex = 8;
            cmbProjectionType.SelectedIndexChanged += cmbProjectionType_SelectedIndexChanged;
            // 
            // lblRegion
            // 
            lblRegion.Font = new Font("Segoe UI", 9F);
            lblRegion.Location = new Point(12, 157);
            lblRegion.Name = "lblRegion";
            lblRegion.Size = new Size(130, 20);
            lblRegion.TabIndex = 9;
            lblRegion.Text = "Region:";
            // 
            // txtRegion
            // 
            txtRegion.Font = new Font("Segoe UI", 9F);
            txtRegion.Location = new Point(144, 154);
            txtRegion.Name = "txtRegion";
            txtRegion.Size = new Size(160, 27);
            txtRegion.TabIndex = 10;
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("Segoe UI", 9F);
            lblDescription.Location = new Point(12, 189);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(130, 20);
            lblDescription.TabIndex = 11;
            lblDescription.Text = "Description:";
            // 
            // txtDescription
            // 
            txtDescription.Font = new Font("Segoe UI", 9F);
            txtDescription.Location = new Point(144, 186);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(330, 27);
            txtDescription.TabIndex = 12;
            // 
            // grpProjectionParams
            // 
            grpProjectionParams.Controls.Add(txtWkt);
            grpProjectionParams.Controls.Add(lblCentralMeridian);
            grpProjectionParams.Controls.Add(nudCentralMeridian);
            grpProjectionParams.Controls.Add(lblLatOrigin);
            grpProjectionParams.Controls.Add(nudLatOrigin);
            grpProjectionParams.Controls.Add(lblScaleFactor);
            grpProjectionParams.Controls.Add(nudScaleFactor);
            grpProjectionParams.Controls.Add(lblFalseEasting);
            grpProjectionParams.Controls.Add(nudFalseEasting);
            grpProjectionParams.Controls.Add(lblFalseNorthing);
            grpProjectionParams.Controls.Add(nudFalseNorthing);
            grpProjectionParams.Controls.Add(lblEllipsoid);
            grpProjectionParams.Controls.Add(txtEllipsoid);
            grpProjectionParams.Controls.Add(lblSemiMajor);
            grpProjectionParams.Controls.Add(nudSemiMajor);
            grpProjectionParams.Controls.Add(lblInvFlat);
            grpProjectionParams.Controls.Add(nudInvFlat);
            grpProjectionParams.Controls.Add(lblWkt);
            grpProjectionParams.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpProjectionParams.Location = new Point(10, 234);
            grpProjectionParams.Name = "grpProjectionParams";
            grpProjectionParams.Size = new Size(490, 392);
            grpProjectionParams.TabIndex = 1;
            grpProjectionParams.TabStop = false;
            grpProjectionParams.Text = "Projection Parameters  (Custom CRS only)";
            // 
            // txtWkt
            // 
            txtWkt.Font = new Font("Consolas", 8.5F);
            txtWkt.Location = new Point(10, 311);
            txtWkt.Multiline = true;
            txtWkt.Name = "txtWkt";
            txtWkt.PlaceholderText = "Optional. Full WKT overrides all individual parameters.";
            txtWkt.ScrollBars = ScrollBars.Vertical;
            txtWkt.Size = new Size(464, 69);
            txtWkt.TabIndex = 17;
            // 
            // lblCentralMeridian
            // 
            lblCentralMeridian.Font = new Font("Segoe UI", 9F);
            lblCentralMeridian.Location = new Point(12, 28);
            lblCentralMeridian.Name = "lblCentralMeridian";
            lblCentralMeridian.Size = new Size(150, 20);
            lblCentralMeridian.TabIndex = 0;
            lblCentralMeridian.Text = "Central Meridian (°):";
            // 
            // nudCentralMeridian
            // 
            nudCentralMeridian.DecimalPlaces = 6;
            nudCentralMeridian.Font = new Font("Segoe UI", 9F);
            nudCentralMeridian.Location = new Point(166, 26);
            nudCentralMeridian.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudCentralMeridian.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudCentralMeridian.Name = "nudCentralMeridian";
            nudCentralMeridian.Size = new Size(140, 27);
            nudCentralMeridian.TabIndex = 1;
            // 
            // lblLatOrigin
            // 
            lblLatOrigin.Font = new Font("Segoe UI", 9F);
            lblLatOrigin.Location = new Point(12, 61);
            lblLatOrigin.Name = "lblLatOrigin";
            lblLatOrigin.Size = new Size(150, 20);
            lblLatOrigin.TabIndex = 2;
            lblLatOrigin.Text = "Latitude of Origin (°):";
            // 
            // nudLatOrigin
            // 
            nudLatOrigin.DecimalPlaces = 6;
            nudLatOrigin.Font = new Font("Segoe UI", 9F);
            nudLatOrigin.Location = new Point(166, 58);
            nudLatOrigin.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudLatOrigin.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudLatOrigin.Name = "nudLatOrigin";
            nudLatOrigin.Size = new Size(140, 27);
            nudLatOrigin.TabIndex = 3;
            // 
            // lblScaleFactor
            // 
            lblScaleFactor.Font = new Font("Segoe UI", 9F);
            lblScaleFactor.Location = new Point(12, 93);
            lblScaleFactor.Name = "lblScaleFactor";
            lblScaleFactor.Size = new Size(150, 20);
            lblScaleFactor.TabIndex = 4;
            lblScaleFactor.Text = "Scale Factor:";
            // 
            // nudScaleFactor
            // 
            nudScaleFactor.DecimalPlaces = 6;
            nudScaleFactor.Font = new Font("Segoe UI", 9F);
            nudScaleFactor.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            nudScaleFactor.Location = new Point(166, 90);
            nudScaleFactor.Maximum = new decimal(new int[] { 2, 0, 0, 0 });
            nudScaleFactor.Name = "nudScaleFactor";
            nudScaleFactor.Size = new Size(140, 27);
            nudScaleFactor.TabIndex = 5;
            nudScaleFactor.Value = new decimal(new int[] { 9999, 0, 0, 262144 });
            // 
            // lblFalseEasting
            // 
            lblFalseEasting.Font = new Font("Segoe UI", 9F);
            lblFalseEasting.Location = new Point(12, 125);
            lblFalseEasting.Name = "lblFalseEasting";
            lblFalseEasting.Size = new Size(150, 20);
            lblFalseEasting.TabIndex = 6;
            lblFalseEasting.Text = "False Easting (m):";
            // 
            // nudFalseEasting
            // 
            nudFalseEasting.DecimalPlaces = 3;
            nudFalseEasting.Font = new Font("Segoe UI", 9F);
            nudFalseEasting.Location = new Point(166, 122);
            nudFalseEasting.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudFalseEasting.Name = "nudFalseEasting";
            nudFalseEasting.Size = new Size(140, 27);
            nudFalseEasting.TabIndex = 7;
            nudFalseEasting.Value = new decimal(new int[] { 500000, 0, 0, 0 });
            // 
            // lblFalseNorthing
            // 
            lblFalseNorthing.Font = new Font("Segoe UI", 9F);
            lblFalseNorthing.Location = new Point(12, 157);
            lblFalseNorthing.Name = "lblFalseNorthing";
            lblFalseNorthing.Size = new Size(150, 20);
            lblFalseNorthing.TabIndex = 8;
            lblFalseNorthing.Text = "False Northing (m):";
            // 
            // nudFalseNorthing
            // 
            nudFalseNorthing.DecimalPlaces = 3;
            nudFalseNorthing.Font = new Font("Segoe UI", 9F);
            nudFalseNorthing.Location = new Point(166, 154);
            nudFalseNorthing.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            nudFalseNorthing.Name = "nudFalseNorthing";
            nudFalseNorthing.Size = new Size(140, 27);
            nudFalseNorthing.TabIndex = 9;
            // 
            // lblEllipsoid
            // 
            lblEllipsoid.Font = new Font("Segoe UI", 9F);
            lblEllipsoid.Location = new Point(12, 188);
            lblEllipsoid.Name = "lblEllipsoid";
            lblEllipsoid.Size = new Size(150, 20);
            lblEllipsoid.TabIndex = 10;
            lblEllipsoid.Text = "Ellipsoid:";
            // 
            // txtEllipsoid
            // 
            txtEllipsoid.Font = new Font("Segoe UI", 9F);
            txtEllipsoid.Location = new Point(166, 186);
            txtEllipsoid.Name = "txtEllipsoid";
            txtEllipsoid.PlaceholderText = "e.g. Everest1830";
            txtEllipsoid.Size = new Size(180, 27);
            txtEllipsoid.TabIndex = 11;
            // 
            // lblSemiMajor
            // 
            lblSemiMajor.Font = new Font("Segoe UI", 9F);
            lblSemiMajor.Location = new Point(12, 219);
            lblSemiMajor.Name = "lblSemiMajor";
            lblSemiMajor.Size = new Size(150, 20);
            lblSemiMajor.TabIndex = 12;
            lblSemiMajor.Text = "Semi-Major Axis (m):";
            // 
            // nudSemiMajor
            // 
            nudSemiMajor.DecimalPlaces = 3;
            nudSemiMajor.Font = new Font("Segoe UI", 9F);
            nudSemiMajor.Location = new Point(166, 216);
            nudSemiMajor.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            nudSemiMajor.Name = "nudSemiMajor";
            nudSemiMajor.Size = new Size(140, 27);
            nudSemiMajor.TabIndex = 13;
            // 
            // lblInvFlat
            // 
            lblInvFlat.Font = new Font("Segoe UI", 9F);
            lblInvFlat.Location = new Point(12, 251);
            lblInvFlat.Name = "lblInvFlat";
            lblInvFlat.Size = new Size(150, 20);
            lblInvFlat.TabIndex = 14;
            lblInvFlat.Text = "Inverse Flattening:";
            // 
            // nudInvFlat
            // 
            nudInvFlat.DecimalPlaces = 6;
            nudInvFlat.Font = new Font("Segoe UI", 9F);
            nudInvFlat.Location = new Point(166, 248);
            nudInvFlat.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudInvFlat.Name = "nudInvFlat";
            nudInvFlat.Size = new Size(140, 27);
            nudInvFlat.TabIndex = 15;
            // 
            // lblWkt
            // 
            lblWkt.Font = new Font("Segoe UI", 9F);
            lblWkt.Location = new Point(12, 288);
            lblWkt.Name = "lblWkt";
            lblWkt.Size = new Size(250, 20);
            lblWkt.TabIndex = 16;
            lblWkt.Text = "WKT Definition (overrides above):";
            // 
            // frmAddEditCoordinateSystem
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(502, 673);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MaximumSize = new Size(520, 720);
            MinimizeBox = false;
            MinimumSize = new Size(520, 600);
            Name = "frmAddEditCoordinateSystem";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Coordinate System";
            Load += frmAddEditCoordinateSystem_Load;
            pnlFooter.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            grpIdentity.ResumeLayout(false);
            grpIdentity.PerformLayout();
            grpProjectionParams.ResumeLayout(false);
            grpProjectionParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudCentralMeridian).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLatOrigin).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudScaleFactor).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudFalseEasting).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudFalseNorthing).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSemiMajor).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudInvFlat).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlFooter;
        private Button btnSave;
        private Button btnCancel;
        private Panel pnlContent;
        private GroupBox grpIdentity;
        private Label lblCode;
        private TextBox txtCode;
        private Label lblName;
        private TextBox txtName;
        private Label lblEpsg;
        private TextBox txtEpsg;
        private Label lblEpsgHint;
        private Label lblProjectionType;
        private ComboBox cmbProjectionType;
        private Label lblRegion;
        private TextBox txtRegion;
        private Label lblDescription;
        private TextBox txtDescription;
        private GroupBox grpProjectionParams;
        private Label lblCentralMeridian;
        private NumericUpDown nudCentralMeridian;
        private Label lblLatOrigin;
        private NumericUpDown nudLatOrigin;
        private Label lblScaleFactor;
        private NumericUpDown nudScaleFactor;
        private Label lblFalseEasting;
        private NumericUpDown nudFalseEasting;
        private Label lblFalseNorthing;
        private NumericUpDown nudFalseNorthing;
        private Label lblEllipsoid;
        private TextBox txtEllipsoid;
        private Label lblSemiMajor;
        private NumericUpDown nudSemiMajor;
        private Label lblInvFlat;
        private NumericUpDown nudInvFlat;
        private Label lblWkt;
        private TextBox txtWkt;
    }
}