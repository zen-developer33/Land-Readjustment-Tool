namespace Land_Readjustment_Tool.UI.Forms.Project
{
    partial class frmAddEditDatumTransformation
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
            lblSourceDatum = new Label();
            txtSourceDatum = new TextBox();
            lblTargetDatum = new Label();
            cmbTargetDatum = new ComboBox();
            grpHelmert = new GroupBox();
            lblDeltaX = new Label();
            nudDeltaX = new NumericUpDown();
            lblDeltaY = new Label();
            nudDeltaY = new NumericUpDown();
            lblDeltaZ = new Label();
            nudDeltaZ = new NumericUpDown();
            lblRx = new Label();
            nudRx = new NumericUpDown();
            lblRy = new Label();
            nudRy = new NumericUpDown();
            lblRz = new Label();
            nudRz = new NumericUpDown();
            lblScale = new Label();
            nudScale = new NumericUpDown();
            lblAppliesTo = new Label();
            txtAppliesTo = new TextBox();
            lblNote = new Label();
            grpMetadata = new GroupBox();
            lblDataSource = new Label();
            txtDataSource = new TextBox();
            lblRegion = new Label();
            txtRegion = new TextBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            pnlHeader.SuspendLayout();
            pnlFooter.SuspendLayout();
            pnlContent.SuspendLayout();
            grpIdentity.SuspendLayout();
            grpHelmert.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudDeltaX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDeltaY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDeltaZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRx).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRy).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRz).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudScale).BeginInit();
            grpMetadata.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(40, 60, 95);
            pnlHeader.Controls.Add(lblFormTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(517, 50);
            pnlHeader.TabIndex = 2;
            // 
            // lblFormTitle
            // 
            lblFormTitle.AutoSize = true;
            lblFormTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblFormTitle.ForeColor = Color.White;
            lblFormTitle.Location = new Point(15, 12);
            lblFormTitle.Name = "lblFormTitle";
            lblFormTitle.Size = new Size(271, 28);
            lblFormTitle.TabIndex = 0;
            lblFormTitle.Text = "Add Datum Transformation";
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.FromArgb(240, 240, 240);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 690);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(517, 55);
            pnlFooter.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(40, 60, 95);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(315, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 30);
            btnSave.TabIndex = 0;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.White;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9F);
            btnCancel.Location = new Point(415, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // pnlContent
            // 
            pnlContent.AutoScroll = true;
            pnlContent.BackColor = Color.FromArgb(235, 235, 235);
            pnlContent.Controls.Add(grpIdentity);
            pnlContent.Controls.Add(grpHelmert);
            pnlContent.Controls.Add(grpMetadata);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 50);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10);
            pnlContent.Size = new Size(517, 640);
            pnlContent.TabIndex = 0;
            // 
            // grpIdentity
            // 
            grpIdentity.BackColor = Color.White;
            grpIdentity.Controls.Add(lblCode);
            grpIdentity.Controls.Add(txtCode);
            grpIdentity.Controls.Add(lblName);
            grpIdentity.Controls.Add(txtName);
            grpIdentity.Controls.Add(lblSourceDatum);
            grpIdentity.Controls.Add(txtSourceDatum);
            grpIdentity.Controls.Add(lblTargetDatum);
            grpIdentity.Controls.Add(cmbTargetDatum);
            grpIdentity.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpIdentity.ForeColor = Color.FromArgb(40, 60, 95);
            grpIdentity.Location = new Point(10, 10);
            grpIdentity.Name = "grpIdentity";
            grpIdentity.Padding = new Padding(10);
            grpIdentity.Size = new Size(500, 165);
            grpIdentity.TabIndex = 0;
            grpIdentity.TabStop = false;
            grpIdentity.Text = "Identity";
            // 
            // lblCode
            // 
            lblCode.Font = new Font("Segoe UI", 9F);
            lblCode.ForeColor = Color.Black;
            lblCode.Location = new Point(15, 30);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(101, 23);
            lblCode.TabIndex = 0;
            lblCode.Text = "Code:";
            lblCode.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtCode
            // 
            txtCode.Font = new Font("Segoe UI", 9F);
            txtCode.Location = new Point(131, 28);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(349, 27);
            txtCode.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.ForeColor = Color.Black;
            lblName.Location = new Point(15, 63);
            lblName.Name = "lblName";
            lblName.Size = new Size(101, 23);
            lblName.TabIndex = 2;
            lblName.Text = "Name:";
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtName
            // 
            txtName.Font = new Font("Segoe UI", 9F);
            txtName.Location = new Point(131, 61);
            txtName.Name = "txtName";
            txtName.Size = new Size(349, 27);
            txtName.TabIndex = 3;
            // 
            // lblSourceDatum
            // 
            lblSourceDatum.Font = new Font("Segoe UI", 9F);
            lblSourceDatum.ForeColor = Color.Black;
            lblSourceDatum.Location = new Point(15, 96);
            lblSourceDatum.Name = "lblSourceDatum";
            lblSourceDatum.Size = new Size(124, 23);
            lblSourceDatum.TabIndex = 4;
            lblSourceDatum.Text = "Source Datum:";
            lblSourceDatum.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSourceDatum
            // 
            txtSourceDatum.Font = new Font("Segoe UI", 9F);
            txtSourceDatum.Location = new Point(131, 94);
            txtSourceDatum.Name = "txtSourceDatum";
            txtSourceDatum.Size = new Size(349, 27);
            txtSourceDatum.TabIndex = 5;
            // 
            // lblTargetDatum
            // 
            lblTargetDatum.Font = new Font("Segoe UI", 9F);
            lblTargetDatum.ForeColor = Color.Black;
            lblTargetDatum.Location = new Point(15, 129);
            lblTargetDatum.Name = "lblTargetDatum";
            lblTargetDatum.Size = new Size(110, 23);
            lblTargetDatum.TabIndex = 6;
            lblTargetDatum.Text = "Target Datum:";
            lblTargetDatum.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbTargetDatum
            // 
            cmbTargetDatum.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTargetDatum.Font = new Font("Segoe UI", 9F);
            cmbTargetDatum.Items.AddRange(new object[] { "WGS84", "GRS80", "NAD83", "ED50" });
            cmbTargetDatum.Location = new Point(131, 127);
            cmbTargetDatum.Name = "cmbTargetDatum";
            cmbTargetDatum.Size = new Size(349, 28);
            cmbTargetDatum.TabIndex = 7;
            // 
            // grpHelmert
            // 
            grpHelmert.BackColor = Color.White;
            grpHelmert.Controls.Add(lblDeltaX);
            grpHelmert.Controls.Add(nudDeltaX);
            grpHelmert.Controls.Add(lblDeltaY);
            grpHelmert.Controls.Add(nudDeltaY);
            grpHelmert.Controls.Add(lblDeltaZ);
            grpHelmert.Controls.Add(nudDeltaZ);
            grpHelmert.Controls.Add(lblRx);
            grpHelmert.Controls.Add(nudRx);
            grpHelmert.Controls.Add(lblRy);
            grpHelmert.Controls.Add(nudRy);
            grpHelmert.Controls.Add(lblRz);
            grpHelmert.Controls.Add(nudRz);
            grpHelmert.Controls.Add(lblScale);
            grpHelmert.Controls.Add(nudScale);
            grpHelmert.Controls.Add(lblAppliesTo);
            grpHelmert.Controls.Add(txtAppliesTo);
            grpHelmert.Controls.Add(lblNote);
            grpHelmert.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpHelmert.ForeColor = Color.FromArgb(40, 60, 95);
            grpHelmert.Location = new Point(10, 185);
            grpHelmert.Name = "grpHelmert";
            grpHelmert.Padding = new Padding(10);
            grpHelmert.Size = new Size(500, 250);
            grpHelmert.TabIndex = 1;
            grpHelmert.TabStop = false;
            grpHelmert.Text = "Helmert 7-Parameter Transform";
            // 
            // lblDeltaX
            // 
            lblDeltaX.Font = new Font("Segoe UI", 9F);
            lblDeltaX.ForeColor = Color.Black;
            lblDeltaX.Location = new Point(15, 33);
            lblDeltaX.Name = "lblDeltaX";
            lblDeltaX.Size = new Size(75, 23);
            lblDeltaX.TabIndex = 0;
            lblDeltaX.Text = "ΔX (m):";
            lblDeltaX.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudDeltaX
            // 
            nudDeltaX.DecimalPlaces = 4;
            nudDeltaX.Font = new Font("Segoe UI", 9F);
            nudDeltaX.Location = new Point(95, 31);
            nudDeltaX.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudDeltaX.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudDeltaX.Name = "nudDeltaX";
            nudDeltaX.Size = new Size(120, 27);
            nudDeltaX.TabIndex = 1;
            // 
            // lblDeltaY
            // 
            lblDeltaY.Font = new Font("Segoe UI", 9F);
            lblDeltaY.ForeColor = Color.Black;
            lblDeltaY.Location = new Point(15, 65);
            lblDeltaY.Name = "lblDeltaY";
            lblDeltaY.Size = new Size(75, 23);
            lblDeltaY.TabIndex = 2;
            lblDeltaY.Text = "ΔY (m):";
            lblDeltaY.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudDeltaY
            // 
            nudDeltaY.DecimalPlaces = 4;
            nudDeltaY.Font = new Font("Segoe UI", 9F);
            nudDeltaY.Location = new Point(95, 63);
            nudDeltaY.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudDeltaY.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudDeltaY.Name = "nudDeltaY";
            nudDeltaY.Size = new Size(120, 27);
            nudDeltaY.TabIndex = 3;
            // 
            // lblDeltaZ
            // 
            lblDeltaZ.Font = new Font("Segoe UI", 9F);
            lblDeltaZ.ForeColor = Color.Black;
            lblDeltaZ.Location = new Point(15, 97);
            lblDeltaZ.Name = "lblDeltaZ";
            lblDeltaZ.Size = new Size(75, 23);
            lblDeltaZ.TabIndex = 4;
            lblDeltaZ.Text = "ΔZ (m):";
            lblDeltaZ.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudDeltaZ
            // 
            nudDeltaZ.DecimalPlaces = 4;
            nudDeltaZ.Font = new Font("Segoe UI", 9F);
            nudDeltaZ.Location = new Point(95, 95);
            nudDeltaZ.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudDeltaZ.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudDeltaZ.Name = "nudDeltaZ";
            nudDeltaZ.Size = new Size(120, 27);
            nudDeltaZ.TabIndex = 5;
            // 
            // lblRx
            // 
            lblRx.Font = new Font("Segoe UI", 9F);
            lblRx.ForeColor = Color.Black;
            lblRx.Location = new Point(245, 33);
            lblRx.Name = "lblRx";
            lblRx.Size = new Size(95, 23);
            lblRx.TabIndex = 6;
            lblRx.Text = "Rx (sec):";
            lblRx.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudRx
            // 
            nudRx.DecimalPlaces = 3;
            nudRx.Font = new Font("Segoe UI", 9F);
            nudRx.Location = new Point(345, 31);
            nudRx.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudRx.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudRx.Name = "nudRx";
            nudRx.Size = new Size(140, 27);
            nudRx.TabIndex = 7;
            // 
            // lblRy
            // 
            lblRy.Font = new Font("Segoe UI", 9F);
            lblRy.ForeColor = Color.Black;
            lblRy.Location = new Point(245, 65);
            lblRy.Name = "lblRy";
            lblRy.Size = new Size(95, 23);
            lblRy.TabIndex = 8;
            lblRy.Text = "Ry (sec):";
            lblRy.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudRy
            // 
            nudRy.DecimalPlaces = 3;
            nudRy.Font = new Font("Segoe UI", 9F);
            nudRy.Location = new Point(345, 63);
            nudRy.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudRy.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudRy.Name = "nudRy";
            nudRy.Size = new Size(140, 27);
            nudRy.TabIndex = 9;
            // 
            // lblRz
            // 
            lblRz.Font = new Font("Segoe UI", 9F);
            lblRz.ForeColor = Color.Black;
            lblRz.Location = new Point(245, 97);
            lblRz.Name = "lblRz";
            lblRz.Size = new Size(95, 23);
            lblRz.TabIndex = 10;
            lblRz.Text = "Rz (sec):";
            lblRz.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudRz
            // 
            nudRz.DecimalPlaces = 3;
            nudRz.Font = new Font("Segoe UI", 9F);
            nudRz.Location = new Point(345, 95);
            nudRz.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudRz.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudRz.Name = "nudRz";
            nudRz.Size = new Size(140, 27);
            nudRz.TabIndex = 11;
            // 
            // lblScale
            // 
            lblScale.Font = new Font("Segoe UI", 9F);
            lblScale.ForeColor = Color.Black;
            lblScale.Location = new Point(229, 130);
            lblScale.Name = "lblScale";
            lblScale.Size = new Size(111, 23);
            lblScale.TabIndex = 12;
            lblScale.Text = "Scale (ppm):";
            lblScale.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudScale
            // 
            nudScale.DecimalPlaces = 4;
            nudScale.Font = new Font("Segoe UI", 9F);
            nudScale.Location = new Point(345, 128);
            nudScale.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudScale.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudScale.Name = "nudScale";
            nudScale.Size = new Size(140, 27);
            nudScale.TabIndex = 13;
            // 
            // lblAppliesTo
            // 
            lblAppliesTo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAppliesTo.ForeColor = Color.Black;
            lblAppliesTo.Location = new Point(15, 168);
            lblAppliesTo.Name = "lblAppliesTo";
            lblAppliesTo.Size = new Size(101, 23);
            lblAppliesTo.TabIndex = 14;
            lblAppliesTo.Text = "Applies to:";
            lblAppliesTo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtAppliesTo
            // 
            txtAppliesTo.Font = new Font("Segoe UI", 9F);
            txtAppliesTo.ForeColor = Color.Gray;
            txtAppliesTo.Location = new Point(131, 166);
            txtAppliesTo.Name = "txtAppliesTo";
            txtAppliesTo.Size = new Size(354, 27);
            txtAppliesTo.TabIndex = 15;
            txtAppliesTo.Text = "e.g. MUTM81, MUTM82, MUTM83";
            // 
            // lblNote
            // 
            lblNote.Font = new Font("Segoe UI", 8.5F);
            lblNote.ForeColor = Color.FromArgb(80, 80, 80);
            lblNote.Location = new Point(15, 205);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(470, 20);
            lblNote.TabIndex = 16;
            lblNote.Text = "Note: Translation in meters, Rotation in arcesconds, Scale in ppm.";
            // 
            // grpMetadata
            // 
            grpMetadata.BackColor = Color.White;
            grpMetadata.Controls.Add(lblDataSource);
            grpMetadata.Controls.Add(txtDataSource);
            grpMetadata.Controls.Add(lblRegion);
            grpMetadata.Controls.Add(txtRegion);
            grpMetadata.Controls.Add(lblDescription);
            grpMetadata.Controls.Add(txtDescription);
            grpMetadata.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpMetadata.ForeColor = Color.FromArgb(40, 60, 95);
            grpMetadata.Location = new Point(10, 441);
            grpMetadata.Name = "grpMetadata";
            grpMetadata.Padding = new Padding(10);
            grpMetadata.Size = new Size(500, 194);
            grpMetadata.TabIndex = 2;
            grpMetadata.TabStop = false;
            grpMetadata.Text = "Metadata";
            // 
            // lblDataSource
            // 
            lblDataSource.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDataSource.ForeColor = Color.Black;
            lblDataSource.Location = new Point(15, 30);
            lblDataSource.Name = "lblDataSource";
            lblDataSource.Size = new Size(110, 23);
            lblDataSource.TabIndex = 0;
            lblDataSource.Text = "Data Source:";
            lblDataSource.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtDataSource
            // 
            txtDataSource.Font = new Font("Segoe UI", 9F);
            txtDataSource.ForeColor = Color.Gray;
            txtDataSource.Location = new Point(135, 28);
            txtDataSource.Name = "txtDataSource";
            txtDataSource.Size = new Size(345, 27);
            txtDataSource.TabIndex = 1;
            txtDataSource.Text = "e.g. Survey Department Nepal";
            // 
            // lblRegion
            // 
            lblRegion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRegion.ForeColor = Color.Black;
            lblRegion.Location = new Point(15, 63);
            lblRegion.Name = "lblRegion";
            lblRegion.Size = new Size(110, 23);
            lblRegion.TabIndex = 2;
            lblRegion.Text = "Region:";
            lblRegion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtRegion
            // 
            txtRegion.Font = new Font("Segoe UI", 9F);
            txtRegion.ForeColor = Color.Gray;
            txtRegion.Location = new Point(135, 61);
            txtRegion.Name = "txtRegion";
            txtRegion.Size = new Size(345, 27);
            txtRegion.TabIndex = 3;
            txtRegion.Text = "e.g. Nepal";
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDescription.ForeColor = Color.Black;
            lblDescription.Location = new Point(15, 96);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(110, 23);
            lblDescription.TabIndex = 4;
            lblDescription.Text = "Description:";
            lblDescription.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtDescription
            // 
            txtDescription.Font = new Font("Segoe UI", 9F);
            txtDescription.Location = new Point(135, 94);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(345, 74);
            txtDescription.TabIndex = 5;
            // 
            // frmAddEditDatumTransformation
            // 
            ClientSize = new Size(517, 745);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "frmAddEditDatumTransformation";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add Datum Transformation";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlFooter.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            grpIdentity.ResumeLayout(false);
            grpIdentity.PerformLayout();
            grpHelmert.ResumeLayout(false);
            grpHelmert.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudDeltaX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDeltaY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDeltaZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRx).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRy).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRz).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudScale).EndInit();
            grpMetadata.ResumeLayout(false);
            grpMetadata.PerformLayout();
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

        // Identity group
        private GroupBox grpIdentity;
        private Label lblCode;
        private TextBox txtCode;
        private Label lblName;
        private TextBox txtName;
        private Label lblSourceDatum;
        private TextBox txtSourceDatum;
        private Label lblTargetDatum;
        private ComboBox cmbTargetDatum;

        // Helmert group
        private GroupBox grpHelmert;
        private Label lblDeltaX;
        private NumericUpDown nudDeltaX;
        private Label lblDeltaY;
        private NumericUpDown nudDeltaY;
        private Label lblDeltaZ;
        private NumericUpDown nudDeltaZ;
        private Label lblRx;
        private NumericUpDown nudRx;
        private Label lblRy;
        private NumericUpDown nudRy;
        private Label lblRz;
        private NumericUpDown nudRz;
        private Label lblScale;
        private NumericUpDown nudScale;
        private Label lblAppliesTo;
        private TextBox txtAppliesTo;
        private Label lblNote;

        // Metadata group
        private GroupBox grpMetadata;
        private Label lblDataSource;
        private TextBox txtDataSource;
        private Label lblRegion;
        private TextBox txtRegion;
        private Label lblDescription;
        private TextBox txtDescription;
    }
}