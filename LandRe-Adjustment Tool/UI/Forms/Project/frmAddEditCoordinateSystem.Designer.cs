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
            pnlHeader = new Panel();
            lblFormTitle = new Label();
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
            lblCentralMeridian = new Label();
            nudCentralMeridian = new NumericUpDown();
            cmbCentralMeridianUnit = new ComboBox();
            lblLatOrigin = new Label();
            nudLatOrigin = new NumericUpDown();
            lblScaleFactor = new Label();
            nudScaleFactor = new NumericUpDown();
            lblFalseEasting = new Label();
            nudFalseEasting = new NumericUpDown();
            lblFalseEastingUnit = new Label();
            lblFalseNorthing = new Label();
            nudFalseNorthing = new NumericUpDown();
            lblFalseNorthingUnit = new Label();
            pnlParamDivider = new Panel();
            lblEllipsoid = new Label();
            txtEllipsoid = new TextBox();
            lblSemiMajor = new Label();
            nudSemiMajor = new NumericUpDown();
            lblSemiMajorUnit = new Label();
            lblInvFlat = new Label();
            nudInvFlat = new NumericUpDown();
            lblInvFlatUnit = new Label();
            lblWkt = new Label();
            txtWkt = new TextBox();
            pnlHeader.SuspendLayout();
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
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(40, 60, 95);
            pnlHeader.Controls.Add(lblFormTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(560, 55);
            pnlHeader.TabIndex = 2;
            // 
            // lblFormTitle
            // 
            lblFormTitle.AutoSize = true;
            lblFormTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblFormTitle.ForeColor = Color.White;
            lblFormTitle.Location = new Point(15, 13);
            lblFormTitle.Name = "lblFormTitle";
            lblFormTitle.Size = new Size(257, 30);
            lblFormTitle.TabIndex = 0;
            lblFormTitle.Text = "Add Coordinate System";
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.FromArgb(235, 235, 235);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 850);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(560, 55);
            pnlFooter.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(40, 60, 95);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(345, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 30);
            btnSave.TabIndex = 0;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.White;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9F);
            btnCancel.Location = new Point(445, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // pnlContent
            // 
            pnlContent.AutoScroll = true;
            pnlContent.BackColor = Color.FromArgb(235, 235, 235);
            pnlContent.Controls.Add(grpIdentity);
            pnlContent.Controls.Add(grpProjectionParams);
            pnlContent.Controls.Add(lblWkt);
            pnlContent.Controls.Add(txtWkt);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 55);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10);
            pnlContent.Size = new Size(560, 795);
            pnlContent.TabIndex = 0;
            // 
            // grpIdentity
            // 
            grpIdentity.BackColor = Color.FromArgb(245, 245, 245);
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
            grpIdentity.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpIdentity.ForeColor = Color.FromArgb(40, 60, 95);
            grpIdentity.Location = new Point(10, 10);
            grpIdentity.Name = "grpIdentity";
            grpIdentity.Padding = new Padding(10);
            grpIdentity.Size = new Size(530, 265);
            grpIdentity.TabIndex = 0;
            grpIdentity.TabStop = false;
            grpIdentity.Text = "CRS Identity";
            // 
            // lblCode
            // 
            lblCode.Font = new Font("Segoe UI", 9F);
            lblCode.ForeColor = Color.Black;
            lblCode.Location = new Point(15, 30);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(130, 23);
            lblCode.TabIndex = 0;
            lblCode.Text = "Code:";
            lblCode.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtCode
            // 
            txtCode.Font = new Font("Segoe UI", 9F);
            txtCode.Location = new Point(160, 28);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(219, 27);
            txtCode.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.ForeColor = Color.Black;
            lblName.Location = new Point(15, 62);
            lblName.Name = "lblName";
            lblName.Size = new Size(130, 23);
            lblName.TabIndex = 2;
            lblName.Text = "Name:";
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtName
            // 
            txtName.Font = new Font("Segoe UI", 9F);
            txtName.Location = new Point(160, 60);
            txtName.Name = "txtName";
            txtName.Size = new Size(355, 27);
            txtName.TabIndex = 3;
            // 
            // lblEpsg
            // 
            lblEpsg.Font = new Font("Segoe UI", 9F);
            lblEpsg.ForeColor = Color.Black;
            lblEpsg.Location = new Point(15, 94);
            lblEpsg.Name = "lblEpsg";
            lblEpsg.Size = new Size(130, 23);
            lblEpsg.TabIndex = 4;
            lblEpsg.Text = "EPSG Code:";
            lblEpsg.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtEpsg
            // 
            txtEpsg.Font = new Font("Segoe UI", 9F);
            txtEpsg.Location = new Point(160, 92);
            txtEpsg.Name = "txtEpsg";
            txtEpsg.Size = new Size(130, 27);
            txtEpsg.TabIndex = 5;
            txtEpsg.TextChanged += txtEpsg_TextChanged;
            // 
            // lblEpsgHint
            // 
            lblEpsgHint.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblEpsgHint.ForeColor = Color.FromArgb(130, 130, 130);
            lblEpsgHint.Location = new Point(298, 94);
            lblEpsgHint.Name = "lblEpsgHint";
            lblEpsgHint.Size = new Size(220, 23);
            lblEpsgHint.TabIndex = 6;
            lblEpsgHint.Text = "Leave empty for custom CRS (MUTM zones).";
            lblEpsgHint.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProjectionType
            // 
            lblProjectionType.Font = new Font("Segoe UI", 9F);
            lblProjectionType.ForeColor = Color.Black;
            lblProjectionType.Location = new Point(15, 126);
            lblProjectionType.Name = "lblProjectionType";
            lblProjectionType.Size = new Size(130, 23);
            lblProjectionType.TabIndex = 7;
            lblProjectionType.Text = "Projection Type:";
            lblProjectionType.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbProjectionType
            // 
            cmbProjectionType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjectionType.Font = new Font("Segoe UI", 9F);
            cmbProjectionType.Items.AddRange(new object[] { "TransverseMercator", "Geographic", "LambertConformalConic" });
            cmbProjectionType.Location = new Point(160, 124);
            cmbProjectionType.Name = "cmbProjectionType";
            cmbProjectionType.Size = new Size(220, 28);
            cmbProjectionType.TabIndex = 8;
            cmbProjectionType.SelectedIndexChanged += cmbProjectionType_SelectedIndexChanged;
            // 
            // lblRegion
            // 
            lblRegion.Font = new Font("Segoe UI", 9F);
            lblRegion.ForeColor = Color.Black;
            lblRegion.Location = new Point(15, 160);
            lblRegion.Name = "lblRegion";
            lblRegion.Size = new Size(130, 23);
            lblRegion.TabIndex = 9;
            lblRegion.Text = "Region:";
            lblRegion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtRegion
            // 
            txtRegion.Font = new Font("Segoe UI", 9F);
            txtRegion.Location = new Point(160, 158);
            txtRegion.Name = "txtRegion";
            txtRegion.Size = new Size(355, 27);
            txtRegion.TabIndex = 10;
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("Segoe UI", 9F);
            lblDescription.ForeColor = Color.Black;
            lblDescription.Location = new Point(15, 192);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(130, 23);
            lblDescription.TabIndex = 11;
            lblDescription.Text = "Description:";
            // 
            // txtDescription
            // 
            txtDescription.Font = new Font("Segoe UI", 9F);
            txtDescription.Location = new Point(160, 192);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(355, 60);
            txtDescription.TabIndex = 12;
            // 
            // grpProjectionParams
            // 
            grpProjectionParams.BackColor = Color.FromArgb(245, 245, 245);
            grpProjectionParams.Controls.Add(lblCentralMeridian);
            grpProjectionParams.Controls.Add(nudCentralMeridian);
            grpProjectionParams.Controls.Add(cmbCentralMeridianUnit);
            grpProjectionParams.Controls.Add(lblLatOrigin);
            grpProjectionParams.Controls.Add(nudLatOrigin);
            grpProjectionParams.Controls.Add(lblScaleFactor);
            grpProjectionParams.Controls.Add(nudScaleFactor);
            grpProjectionParams.Controls.Add(lblFalseEasting);
            grpProjectionParams.Controls.Add(nudFalseEasting);
            grpProjectionParams.Controls.Add(lblFalseEastingUnit);
            grpProjectionParams.Controls.Add(lblFalseNorthing);
            grpProjectionParams.Controls.Add(nudFalseNorthing);
            grpProjectionParams.Controls.Add(lblFalseNorthingUnit);
            grpProjectionParams.Controls.Add(pnlParamDivider);
            grpProjectionParams.Controls.Add(lblEllipsoid);
            grpProjectionParams.Controls.Add(txtEllipsoid);
            grpProjectionParams.Controls.Add(lblSemiMajor);
            grpProjectionParams.Controls.Add(nudSemiMajor);
            grpProjectionParams.Controls.Add(lblSemiMajorUnit);
            grpProjectionParams.Controls.Add(lblInvFlat);
            grpProjectionParams.Controls.Add(nudInvFlat);
            grpProjectionParams.Controls.Add(lblInvFlatUnit);
            grpProjectionParams.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpProjectionParams.ForeColor = Color.FromArgb(40, 60, 95);
            grpProjectionParams.Location = new Point(10, 285);
            grpProjectionParams.Name = "grpProjectionParams";
            grpProjectionParams.Padding = new Padding(10);
            grpProjectionParams.Size = new Size(530, 340);
            grpProjectionParams.TabIndex = 1;
            grpProjectionParams.TabStop = false;
            grpProjectionParams.Text = "Projection Parameters (Custom CRS only)";
            // 
            // lblCentralMeridian
            // 
            lblCentralMeridian.Font = new Font("Segoe UI", 9F);
            lblCentralMeridian.ForeColor = Color.Black;
            lblCentralMeridian.Location = new Point(15, 30);
            lblCentralMeridian.Name = "lblCentralMeridian";
            lblCentralMeridian.Size = new Size(165, 23);
            lblCentralMeridian.TabIndex = 0;
            lblCentralMeridian.Text = "Central Meridian (°):";
            lblCentralMeridian.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudCentralMeridian
            // 
            nudCentralMeridian.DecimalPlaces = 6;
            nudCentralMeridian.Font = new Font("Segoe UI", 9F);
            nudCentralMeridian.Location = new Point(190, 28);
            nudCentralMeridian.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            nudCentralMeridian.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
            nudCentralMeridian.Name = "nudCentralMeridian";
            nudCentralMeridian.Size = new Size(130, 27);
            nudCentralMeridian.TabIndex = 1;
            nudCentralMeridian.TextAlign = HorizontalAlignment.Right;
            // 
            // cmbCentralMeridianUnit
            // 
            cmbCentralMeridianUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCentralMeridianUnit.Font = new Font("Segoe UI", 9F);
            cmbCentralMeridianUnit.Items.AddRange(new object[] { "degrees", "radians" });
            cmbCentralMeridianUnit.Location = new Point(325, 28);
            cmbCentralMeridianUnit.Name = "cmbCentralMeridianUnit";
            cmbCentralMeridianUnit.Size = new Size(90, 28);
            cmbCentralMeridianUnit.TabIndex = 2;
            // 
            // lblLatOrigin
            // 
            lblLatOrigin.Font = new Font("Segoe UI", 9F);
            lblLatOrigin.ForeColor = Color.Black;
            lblLatOrigin.Location = new Point(15, 64);
            lblLatOrigin.Name = "lblLatOrigin";
            lblLatOrigin.Size = new Size(165, 23);
            lblLatOrigin.TabIndex = 3;
            lblLatOrigin.Text = "Latitude of Origin (°):";
            lblLatOrigin.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudLatOrigin
            // 
            nudLatOrigin.DecimalPlaces = 6;
            nudLatOrigin.Font = new Font("Segoe UI", 9F);
            nudLatOrigin.Location = new Point(190, 62);
            nudLatOrigin.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            nudLatOrigin.Minimum = new decimal(new int[] { 90, 0, 0, int.MinValue });
            nudLatOrigin.Name = "nudLatOrigin";
            nudLatOrigin.Size = new Size(130, 27);
            nudLatOrigin.TabIndex = 4;
            nudLatOrigin.TextAlign = HorizontalAlignment.Right;
            // 
            // lblScaleFactor
            // 
            lblScaleFactor.Font = new Font("Segoe UI", 9F);
            lblScaleFactor.ForeColor = Color.Black;
            lblScaleFactor.Location = new Point(15, 98);
            lblScaleFactor.Name = "lblScaleFactor";
            lblScaleFactor.Size = new Size(165, 23);
            lblScaleFactor.TabIndex = 5;
            lblScaleFactor.Text = "Scale Factor:";
            lblScaleFactor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudScaleFactor
            // 
            nudScaleFactor.DecimalPlaces = 6;
            nudScaleFactor.Font = new Font("Segoe UI", 9F);
            nudScaleFactor.Increment = new decimal(new int[] { 1, 0, 0, 393216 });
            nudScaleFactor.Location = new Point(190, 96);
            nudScaleFactor.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            nudScaleFactor.Name = "nudScaleFactor";
            nudScaleFactor.Size = new Size(130, 27);
            nudScaleFactor.TabIndex = 6;
            nudScaleFactor.TextAlign = HorizontalAlignment.Right;
            nudScaleFactor.Value = new decimal(new int[] { 9999, 0, 0, 393216 });
            // 
            // lblFalseEasting
            // 
            lblFalseEasting.Font = new Font("Segoe UI", 9F);
            lblFalseEasting.ForeColor = Color.Black;
            lblFalseEasting.Location = new Point(15, 132);
            lblFalseEasting.Name = "lblFalseEasting";
            lblFalseEasting.Size = new Size(165, 23);
            lblFalseEasting.TabIndex = 7;
            lblFalseEasting.Text = "False Easting (m):";
            lblFalseEasting.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudFalseEasting
            // 
            nudFalseEasting.Font = new Font("Segoe UI", 9F);
            nudFalseEasting.Location = new Point(190, 130);
            nudFalseEasting.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            nudFalseEasting.Minimum = new decimal(new int[] { 10000000, 0, 0, int.MinValue });
            nudFalseEasting.Name = "nudFalseEasting";
            nudFalseEasting.Size = new Size(130, 27);
            nudFalseEasting.TabIndex = 8;
            nudFalseEasting.TextAlign = HorizontalAlignment.Right;
            nudFalseEasting.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // lblFalseEastingUnit
            // 
            lblFalseEastingUnit.AutoSize = true;
            lblFalseEastingUnit.Font = new Font("Segoe UI", 9F);
            lblFalseEastingUnit.ForeColor = Color.Black;
            lblFalseEastingUnit.Location = new Point(325, 132);
            lblFalseEastingUnit.Name = "lblFalseEastingUnit";
            lblFalseEastingUnit.Size = new Size(54, 20);
            lblFalseEastingUnit.TabIndex = 9;
            lblFalseEastingUnit.Text = "meters";
            // 
            // lblFalseNorthing
            // 
            lblFalseNorthing.Font = new Font("Segoe UI", 9F);
            lblFalseNorthing.ForeColor = Color.Black;
            lblFalseNorthing.Location = new Point(15, 166);
            lblFalseNorthing.Name = "lblFalseNorthing";
            lblFalseNorthing.Size = new Size(165, 23);
            lblFalseNorthing.TabIndex = 10;
            lblFalseNorthing.Text = "False Northing (m):";
            lblFalseNorthing.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudFalseNorthing
            // 
            nudFalseNorthing.Font = new Font("Segoe UI", 9F);
            nudFalseNorthing.Location = new Point(190, 164);
            nudFalseNorthing.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            nudFalseNorthing.Minimum = new decimal(new int[] { 10000000, 0, 0, int.MinValue });
            nudFalseNorthing.Name = "nudFalseNorthing";
            nudFalseNorthing.Size = new Size(130, 27);
            nudFalseNorthing.TabIndex = 11;
            nudFalseNorthing.TextAlign = HorizontalAlignment.Right;
            // 
            // lblFalseNorthingUnit
            // 
            lblFalseNorthingUnit.AutoSize = true;
            lblFalseNorthingUnit.Font = new Font("Segoe UI", 9F);
            lblFalseNorthingUnit.ForeColor = Color.Black;
            lblFalseNorthingUnit.Location = new Point(325, 166);
            lblFalseNorthingUnit.Name = "lblFalseNorthingUnit";
            lblFalseNorthingUnit.Size = new Size(54, 20);
            lblFalseNorthingUnit.TabIndex = 12;
            lblFalseNorthingUnit.Text = "meters";
            // 
            // pnlParamDivider
            // 
            pnlParamDivider.BackColor = Color.FromArgb(200, 200, 200);
            pnlParamDivider.Location = new Point(10, 202);
            pnlParamDivider.Name = "pnlParamDivider";
            pnlParamDivider.Size = new Size(510, 1);
            pnlParamDivider.TabIndex = 13;
            // 
            // lblEllipsoid
            // 
            lblEllipsoid.Font = new Font("Segoe UI", 9F);
            lblEllipsoid.ForeColor = Color.Black;
            lblEllipsoid.Location = new Point(15, 212);
            lblEllipsoid.Name = "lblEllipsoid";
            lblEllipsoid.Size = new Size(165, 23);
            lblEllipsoid.TabIndex = 14;
            lblEllipsoid.Text = "Ellipsoid:";
            lblEllipsoid.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtEllipsoid
            // 
            txtEllipsoid.Font = new Font("Segoe UI", 9F);
            txtEllipsoid.Location = new Point(190, 210);
            txtEllipsoid.Name = "txtEllipsoid";
            txtEllipsoid.Size = new Size(225, 27);
            txtEllipsoid.TabIndex = 15;
            txtEllipsoid.Leave += txtEllipsoid_Leave;
            // 
            // lblSemiMajor
            // 
            lblSemiMajor.Font = new Font("Segoe UI", 9F);
            lblSemiMajor.ForeColor = Color.Black;
            lblSemiMajor.Location = new Point(15, 246);
            lblSemiMajor.Name = "lblSemiMajor";
            lblSemiMajor.Size = new Size(165, 23);
            lblSemiMajor.TabIndex = 16;
            lblSemiMajor.Text = "Semi-Major Axis (m):";
            lblSemiMajor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudSemiMajor
            // 
            nudSemiMajor.Font = new Font("Segoe UI", 9F);
            nudSemiMajor.Location = new Point(190, 244);
            nudSemiMajor.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            nudSemiMajor.Name = "nudSemiMajor";
            nudSemiMajor.Size = new Size(130, 27);
            nudSemiMajor.TabIndex = 17;
            nudSemiMajor.TextAlign = HorizontalAlignment.Right;
            // 
            // lblSemiMajorUnit
            // 
            lblSemiMajorUnit.AutoSize = true;
            lblSemiMajorUnit.Font = new Font("Segoe UI", 9F);
            lblSemiMajorUnit.ForeColor = Color.Black;
            lblSemiMajorUnit.Location = new Point(325, 246);
            lblSemiMajorUnit.Name = "lblSemiMajorUnit";
            lblSemiMajorUnit.Size = new Size(54, 20);
            lblSemiMajorUnit.TabIndex = 18;
            lblSemiMajorUnit.Text = "meters";
            // 
            // lblInvFlat
            // 
            lblInvFlat.Font = new Font("Segoe UI", 9F);
            lblInvFlat.ForeColor = Color.Black;
            lblInvFlat.Location = new Point(15, 280);
            lblInvFlat.Name = "lblInvFlat";
            lblInvFlat.Size = new Size(165, 23);
            lblInvFlat.TabIndex = 19;
            lblInvFlat.Text = "Inverse Flattening:";
            lblInvFlat.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // nudInvFlat
            // 
            nudInvFlat.Font = new Font("Segoe UI", 9F);
            nudInvFlat.Location = new Point(190, 278);
            nudInvFlat.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudInvFlat.Name = "nudInvFlat";
            nudInvFlat.Size = new Size(130, 27);
            nudInvFlat.TabIndex = 20;
            nudInvFlat.TextAlign = HorizontalAlignment.Right;
            // 
            // lblInvFlatUnit
            // 
            lblInvFlatUnit.AutoSize = true;
            lblInvFlatUnit.Font = new Font("Segoe UI", 9F);
            lblInvFlatUnit.ForeColor = Color.Black;
            lblInvFlatUnit.Location = new Point(325, 280);
            lblInvFlatUnit.Name = "lblInvFlatUnit";
            lblInvFlatUnit.Size = new Size(54, 20);
            lblInvFlatUnit.TabIndex = 21;
            lblInvFlatUnit.Text = "meters";
            // 
            // lblWkt
            // 
            lblWkt.AutoSize = true;
            lblWkt.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblWkt.ForeColor = Color.Black;
            lblWkt.Location = new Point(10, 635);
            lblWkt.Name = "lblWkt";
            lblWkt.Size = new Size(121, 20);
            lblWkt.TabIndex = 2;
            lblWkt.Text = "WKT Definition:";
            // 
            // txtWkt
            // 
            txtWkt.Font = new Font("Segoe UI", 9F);
            txtWkt.ForeColor = Color.Gray;
            txtWkt.Location = new Point(10, 655);
            txtWkt.Multiline = true;
            txtWkt.Name = "txtWkt";
            txtWkt.ScrollBars = ScrollBars.Vertical;
            txtWkt.Size = new Size(530, 130);
            txtWkt.TabIndex = 3;
            txtWkt.Text = "Optional. Overrides all parameters above if provided.";
            txtWkt.Enter += txtWkt_Enter;
            txtWkt.Leave += txtWkt_Leave;
            // 
            // frmAddEditCoordinateSystem
            // 
            ClientSize = new Size(560, 905);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "frmAddEditCoordinateSystem";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Coordinate System";
            Load += frmAddEditCoordinateSystem_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlFooter.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
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

        // ── Control declarations ──────────────────────────────────────────────

        private Panel pnlHeader;
        private Label lblFormTitle;
        private Panel pnlFooter;
        private Button btnSave;
        private Button btnCancel;
        private Panel pnlContent;

        // CRS Identity group
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

        // Projection Parameters group
        private GroupBox grpProjectionParams;
        private Label lblCentralMeridian;
        private NumericUpDown nudCentralMeridian;
        private ComboBox cmbCentralMeridianUnit;
        private Label lblLatOrigin;
        private NumericUpDown nudLatOrigin;
        private Label lblScaleFactor;
        private NumericUpDown nudScaleFactor;
        private Label lblFalseEasting;
        private NumericUpDown nudFalseEasting;
        private Label lblFalseEastingUnit;
        private Label lblFalseNorthing;
        private NumericUpDown nudFalseNorthing;
        private Label lblFalseNorthingUnit;
        private Panel pnlParamDivider;
        private Label lblEllipsoid;
        private TextBox txtEllipsoid;
        private Label lblSemiMajor;
        private NumericUpDown nudSemiMajor;
        private Label lblSemiMajorUnit;
        private Label lblInvFlat;
        private NumericUpDown nudInvFlat;
        private Label lblInvFlatUnit;

        // WKT
        private Label lblWkt;
        private TextBox txtWkt;
    }
}