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
            grpParams = new GroupBox();
            lblM = new Label();
            lblR = new Label();
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
            grpMetadata = new GroupBox();
            lblApplies = new Label();
            txtAppliesTo = new TextBox();
            lblSource = new Label();
            txtDataSource = new TextBox();
            lblRegion = new Label();
            txtRegion = new TextBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            pnlFooter.SuspendLayout();
            pnlContent.SuspendLayout();
            grpIdentity.SuspendLayout();
            grpParams.SuspendLayout();
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
            // pnlFooter
            // 
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 594);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(6);
            pnlFooter.Size = new Size(482, 40);
            pnlFooter.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(328, 6);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 28);
            btnSave.TabIndex = 90;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(412, 6);
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
            pnlContent.Controls.Add(grpParams);
            pnlContent.Controls.Add(grpMetadata);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10, 8, 10, 4);
            pnlContent.Size = new Size(482, 594);
            pnlContent.TabIndex = 0;
            // 
            // grpIdentity
            // 
            grpIdentity.Controls.Add(lblCode);
            grpIdentity.Controls.Add(txtCode);
            grpIdentity.Controls.Add(lblName);
            grpIdentity.Controls.Add(txtName);
            grpIdentity.Controls.Add(lblSourceDatum);
            grpIdentity.Controls.Add(txtSourceDatum);
            grpIdentity.Controls.Add(lblTargetDatum);
            grpIdentity.Controls.Add(cmbTargetDatum);
            grpIdentity.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpIdentity.Location = new Point(10, 8);
            grpIdentity.Name = "grpIdentity";
            grpIdentity.Size = new Size(470, 158);
            grpIdentity.TabIndex = 0;
            grpIdentity.TabStop = false;
            grpIdentity.Text = "Identity";
            // 
            // lblCode
            // 
            lblCode.Font = new Font("Segoe UI", 9F);
            lblCode.Location = new Point(12, 29);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(120, 20);
            lblCode.TabIndex = 0;
            lblCode.Text = "Code:";
            // 
            // txtCode
            // 
            txtCode.Font = new Font("Segoe UI", 9F);
            txtCode.Location = new Point(134, 26);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(320, 27);
            txtCode.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.Location = new Point(12, 61);
            lblName.Name = "lblName";
            lblName.Size = new Size(120, 20);
            lblName.TabIndex = 2;
            lblName.Text = "Name:";
            // 
            // txtName
            // 
            txtName.Font = new Font("Segoe UI", 9F);
            txtName.Location = new Point(134, 58);
            txtName.Name = "txtName";
            txtName.Size = new Size(320, 27);
            txtName.TabIndex = 3;
            // 
            // lblSourceDatum
            // 
            lblSourceDatum.Font = new Font("Segoe UI", 9F);
            lblSourceDatum.Location = new Point(12, 93);
            lblSourceDatum.Name = "lblSourceDatum";
            lblSourceDatum.Size = new Size(120, 20);
            lblSourceDatum.TabIndex = 4;
            lblSourceDatum.Text = "Source Datum:";
            // 
            // txtSourceDatum
            // 
            txtSourceDatum.Font = new Font("Segoe UI", 9F);
            txtSourceDatum.Location = new Point(134, 90);
            txtSourceDatum.Name = "txtSourceDatum";
            txtSourceDatum.Size = new Size(200, 27);
            txtSourceDatum.TabIndex = 5;
            // 
            // lblTargetDatum
            // 
            lblTargetDatum.Font = new Font("Segoe UI", 9F);
            lblTargetDatum.Location = new Point(12, 125);
            lblTargetDatum.Name = "lblTargetDatum";
            lblTargetDatum.Size = new Size(120, 20);
            lblTargetDatum.TabIndex = 6;
            lblTargetDatum.Text = "Target Datum:";
            // 
            // cmbTargetDatum
            // 
            cmbTargetDatum.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTargetDatum.Font = new Font("Segoe UI", 9F);
            cmbTargetDatum.Items.AddRange(new object[] { "WGS84" });
            cmbTargetDatum.Location = new Point(134, 122);
            cmbTargetDatum.Name = "cmbTargetDatum";
            cmbTargetDatum.Size = new Size(200, 28);
            cmbTargetDatum.TabIndex = 7;
            // 
            // grpParams
            // 
            grpParams.Controls.Add(lblM);
            grpParams.Controls.Add(lblR);
            grpParams.Controls.Add(lblDeltaX);
            grpParams.Controls.Add(nudDeltaX);
            grpParams.Controls.Add(lblDeltaY);
            grpParams.Controls.Add(nudDeltaY);
            grpParams.Controls.Add(lblDeltaZ);
            grpParams.Controls.Add(nudDeltaZ);
            grpParams.Controls.Add(lblRx);
            grpParams.Controls.Add(nudRx);
            grpParams.Controls.Add(lblRy);
            grpParams.Controls.Add(nudRy);
            grpParams.Controls.Add(lblRz);
            grpParams.Controls.Add(nudRz);
            grpParams.Controls.Add(lblScale);
            grpParams.Controls.Add(nudScale);
            grpParams.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpParams.Location = new Point(10, 174);
            grpParams.Name = "grpParams";
            grpParams.Size = new Size(470, 220);
            grpParams.TabIndex = 1;
            grpParams.TabStop = false;
            grpParams.Text = "Helmert 7-Parameter Transform";
            // 
            // lblM
            // 
            lblM.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblM.ForeColor = SystemColors.GrayText;
            lblM.Location = new Point(12, 24);
            lblM.Name = "lblM";
            lblM.Size = new Size(180, 18);
            lblM.TabIndex = 0;
            lblM.Text = "Translations (meters)";
            // 
            // lblR
            // 
            lblR.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblR.ForeColor = SystemColors.GrayText;
            lblR.Location = new Point(12, 81);
            lblR.Name = "lblR";
            lblR.Size = new Size(180, 18);
            lblR.TabIndex = 1;
            lblR.Text = "Rotations (arc-seconds)";
            // 
            // lblDeltaX
            // 
            lblDeltaX.Font = new Font("Segoe UI", 9F);
            lblDeltaX.Location = new Point(12, 47);
            lblDeltaX.Name = "lblDeltaX";
            lblDeltaX.Size = new Size(27, 20);
            lblDeltaX.TabIndex = 2;
            lblDeltaX.Text = "dX";
            // 
            // nudDeltaX
            // 
            nudDeltaX.DecimalPlaces = 4;
            nudDeltaX.Font = new Font("Segoe UI", 9F);
            nudDeltaX.Location = new Point(45, 44);
            nudDeltaX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudDeltaX.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            nudDeltaX.Name = "nudDeltaX";
            nudDeltaX.Size = new Size(98, 27);
            nudDeltaX.TabIndex = 3;
            // 
            // lblDeltaY
            // 
            lblDeltaY.Font = new Font("Segoe UI", 9F);
            lblDeltaY.Location = new Point(165, 47);
            lblDeltaY.Name = "lblDeltaY";
            lblDeltaY.Size = new Size(27, 20);
            lblDeltaY.TabIndex = 4;
            lblDeltaY.Text = "dY";
            // 
            // nudDeltaY
            // 
            nudDeltaY.DecimalPlaces = 4;
            nudDeltaY.Font = new Font("Segoe UI", 9F);
            nudDeltaY.Location = new Point(198, 44);
            nudDeltaY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudDeltaY.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            nudDeltaY.Name = "nudDeltaY";
            nudDeltaY.Size = new Size(98, 27);
            nudDeltaY.TabIndex = 5;
            // 
            // lblDeltaZ
            // 
            lblDeltaZ.Font = new Font("Segoe UI", 9F);
            lblDeltaZ.Location = new Point(318, 47);
            lblDeltaZ.Name = "lblDeltaZ";
            lblDeltaZ.Size = new Size(27, 20);
            lblDeltaZ.TabIndex = 6;
            lblDeltaZ.Text = "dZ";
            // 
            // nudDeltaZ
            // 
            nudDeltaZ.DecimalPlaces = 4;
            nudDeltaZ.Font = new Font("Segoe UI", 9F);
            nudDeltaZ.Location = new Point(351, 44);
            nudDeltaZ.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudDeltaZ.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            nudDeltaZ.Name = "nudDeltaZ";
            nudDeltaZ.Size = new Size(98, 27);
            nudDeltaZ.TabIndex = 7;
            // 
            // lblRx
            // 
            lblRx.Font = new Font("Segoe UI", 9F);
            lblRx.Location = new Point(165, 107);
            lblRx.Name = "lblRx";
            lblRx.Size = new Size(27, 20);
            lblRx.TabIndex = 8;
            lblRx.Text = "rX";
            // 
            // nudRx
            // 
            nudRx.DecimalPlaces = 4;
            nudRx.Font = new Font("Segoe UI", 9F);
            nudRx.Location = new Point(198, 105);
            nudRx.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudRx.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            nudRx.Name = "nudRx";
            nudRx.Size = new Size(98, 27);
            nudRx.TabIndex = 9;
            // 
            // lblRy
            // 
            lblRy.Font = new Font("Segoe UI", 9F);
            lblRy.Location = new Point(12, 107);
            lblRy.Name = "lblRy";
            lblRy.Size = new Size(24, 20);
            lblRy.TabIndex = 10;
            lblRy.Text = "rY";
            // 
            // nudRy
            // 
            nudRy.DecimalPlaces = 4;
            nudRy.Font = new Font("Segoe UI", 9F);
            nudRy.Location = new Point(45, 105);
            nudRy.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudRy.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            nudRy.Name = "nudRy";
            nudRy.Size = new Size(98, 27);
            nudRy.TabIndex = 11;
            // 
            // lblRz
            // 
            lblRz.Font = new Font("Segoe UI", 9F);
            lblRz.Location = new Point(321, 107);
            lblRz.Name = "lblRz";
            lblRz.Size = new Size(24, 20);
            lblRz.TabIndex = 12;
            lblRz.Text = "rZ";
            // 
            // nudRz
            // 
            nudRz.DecimalPlaces = 4;
            nudRz.Font = new Font("Segoe UI", 9F);
            nudRz.Location = new Point(351, 105);
            nudRz.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudRz.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            nudRz.Name = "nudRz";
            nudRz.Size = new Size(98, 27);
            nudRz.TabIndex = 13;
            // 
            // lblScale
            // 
            lblScale.Font = new Font("Segoe UI", 9F);
            lblScale.Location = new Point(12, 154);
            lblScale.Name = "lblScale";
            lblScale.Size = new Size(52, 20);
            lblScale.TabIndex = 14;
            lblScale.Text = "Scale (ppm):";
            // 
            // nudScale
            // 
            nudScale.DecimalPlaces = 4;
            nudScale.Font = new Font("Segoe UI", 9F);
            nudScale.Location = new Point(70, 152);
            nudScale.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudScale.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            nudScale.Name = "nudScale";
            nudScale.Size = new Size(73, 27);
            nudScale.TabIndex = 15;
            // 
            // grpMetadata
            // 
            grpMetadata.Controls.Add(lblApplies);
            grpMetadata.Controls.Add(txtAppliesTo);
            grpMetadata.Controls.Add(lblSource);
            grpMetadata.Controls.Add(txtDataSource);
            grpMetadata.Controls.Add(lblRegion);
            grpMetadata.Controls.Add(txtRegion);
            grpMetadata.Controls.Add(lblDescription);
            grpMetadata.Controls.Add(txtDescription);
            grpMetadata.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpMetadata.Location = new Point(10, 402);
            grpMetadata.Name = "grpMetadata";
            grpMetadata.Size = new Size(470, 185);
            grpMetadata.TabIndex = 2;
            grpMetadata.TabStop = false;
            grpMetadata.Text = "Metadata";
            // 
            // lblApplies
            // 
            lblApplies.Font = new Font("Segoe UI", 9F);
            lblApplies.Location = new Point(12, 29);
            lblApplies.Name = "lblApplies";
            lblApplies.Size = new Size(90, 20);
            lblApplies.TabIndex = 0;
            lblApplies.Text = "Applies To:";
            // 
            // txtAppliesTo
            // 
            txtAppliesTo.Font = new Font("Segoe UI", 9F);
            txtAppliesTo.Location = new Point(106, 26);
            txtAppliesTo.Name = "txtAppliesTo";
            txtAppliesTo.PlaceholderText = "e.g. MUTM81,MUTM82,MUTM83";
            txtAppliesTo.Size = new Size(350, 27);
            txtAppliesTo.TabIndex = 1;
            // 
            // lblSource
            // 
            lblSource.Font = new Font("Segoe UI", 9F);
            lblSource.Location = new Point(12, 61);
            lblSource.Name = "lblSource";
            lblSource.Size = new Size(90, 20);
            lblSource.TabIndex = 2;
            lblSource.Text = "Source Ref:";
            // 
            // txtDataSource
            // 
            txtDataSource.Font = new Font("Segoe UI", 9F);
            txtDataSource.Location = new Point(106, 58);
            txtDataSource.Name = "txtDataSource";
            txtDataSource.PlaceholderText = "e.g. Survey Department Nepal";
            txtDataSource.Size = new Size(350, 27);
            txtDataSource.TabIndex = 3;
            // 
            // lblRegion
            // 
            lblRegion.Font = new Font("Segoe UI", 9F);
            lblRegion.Location = new Point(12, 93);
            lblRegion.Name = "lblRegion";
            lblRegion.Size = new Size(90, 20);
            lblRegion.TabIndex = 4;
            lblRegion.Text = "Region:";
            // 
            // txtRegion
            // 
            txtRegion.Font = new Font("Segoe UI", 9F);
            txtRegion.Location = new Point(106, 90);
            txtRegion.Name = "txtRegion";
            txtRegion.PlaceholderText = "e.g. Nepal";
            txtRegion.Size = new Size(180, 27);
            txtRegion.TabIndex = 5;
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("Segoe UI", 9F);
            lblDescription.Location = new Point(12, 124);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(90, 20);
            lblDescription.TabIndex = 6;
            lblDescription.Text = "Description:";
            // 
            // txtDescription
            // 
            txtDescription.Font = new Font("Segoe UI", 9F);
            txtDescription.Location = new Point(106, 122);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(350, 57);
            txtDescription.TabIndex = 7;
            // 
            // frmAddEditDatumTransformation
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(482, 634);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MaximumSize = new Size(500, 700);
            MinimizeBox = false;
            MinimumSize = new Size(500, 580);
            Name = "frmAddEditDatumTransformation";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Datum Transformation";
            Load += frmAddEditDatumTransformation_Load;
            pnlFooter.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            grpIdentity.ResumeLayout(false);
            grpIdentity.PerformLayout();
            grpParams.ResumeLayout(false);
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

        private Panel pnlFooter;
        private Button btnSave;
        private Button btnCancel;
        private Panel pnlContent;
        private GroupBox grpIdentity;
        private Label lblCode;
        private TextBox txtCode;
        private Label lblName;
        private TextBox txtName;
        private Label lblSourceDatum;
        private TextBox txtSourceDatum;
        private Label lblTargetDatum;
        private ComboBox cmbTargetDatum;
        private GroupBox grpParams;
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
        private GroupBox grpMetadata;
        private Label lblApplies;
        private TextBox txtAppliesTo;
        private Label lblSource;
        private TextBox txtDataSource;
        private Label lblRegion;
        private TextBox txtRegion;
        private Label lblDescription;
        private TextBox txtDescription;
        private Label lblM;
        private Label lblR;
    }
}