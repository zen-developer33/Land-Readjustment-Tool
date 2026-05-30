namespace Land_Readjustment_Tool.UI.Forms.Definitions
{
    partial class frmBlockDefinitionEditor
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
            pnlContent = new Panel();
            grpDetails = new GroupBox();
            lblCode = new Label();
            txtCode = new TextBox();
            lblName = new Label();
            txtName = new TextBox();
            lblType = new Label();
            cboType = new ComboBox();
            lblDepth = new Label();
            nudDepth = new NumericUpDown();
            lblDepthUnit = new Label();
            lblLength = new Label();
            nudLength = new NumericUpDown();
            lblLengthUnit = new Label();
            lblDescription = new Label();
            txtDescription = new TextBox();
            pnlFooter = new Panel();
            btnSave = new Button();
            btnCancel = new Button();
            pnlContent.SuspendLayout();
            grpDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudDepth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLength).BeginInit();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.Controls.Add(grpDetails);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10, 8, 10, 4);
            pnlContent.Size = new Size(484, 287);
            pnlContent.TabIndex = 0;
            // 
            // grpDetails
            // 
            grpDetails.Controls.Add(lblCode);
            grpDetails.Controls.Add(txtCode);
            grpDetails.Controls.Add(lblName);
            grpDetails.Controls.Add(txtName);
            grpDetails.Controls.Add(lblType);
            grpDetails.Controls.Add(cboType);
            grpDetails.Controls.Add(lblDepth);
            grpDetails.Controls.Add(nudDepth);
            grpDetails.Controls.Add(lblDepthUnit);
            grpDetails.Controls.Add(lblLength);
            grpDetails.Controls.Add(nudLength);
            grpDetails.Controls.Add(lblLengthUnit);
            grpDetails.Controls.Add(lblDescription);
            grpDetails.Controls.Add(txtDescription);
            grpDetails.Dock = DockStyle.Fill;
            grpDetails.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpDetails.Location = new Point(10, 8);
            grpDetails.Name = "grpDetails";
            grpDetails.Padding = new Padding(8);
            grpDetails.Size = new Size(464, 275);
            grpDetails.TabIndex = 0;
            grpDetails.TabStop = false;
            grpDetails.Text = "Block Definition";
            // 
            // lblCode
            // 
            lblCode.Font = new Font("Segoe UI", 9F);
            lblCode.Location = new Point(14, 32);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(110, 20);
            lblCode.TabIndex = 0;
            lblCode.Text = "Block Code:";
            // 
            // txtCode
            // 
            txtCode.Font = new Font("Segoe UI", 9F);
            txtCode.Location = new Point(130, 29);
            txtCode.MaxLength = 40;
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(323, 27);
            txtCode.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.Location = new Point(14, 64);
            lblName.Name = "lblName";
            lblName.Size = new Size(110, 20);
            lblName.TabIndex = 2;
            lblName.Text = "Block Name:";
            // 
            // txtName
            // 
            txtName.Font = new Font("Segoe UI", 9F);
            txtName.Location = new Point(130, 61);
            txtName.MaxLength = 120;
            txtName.Name = "txtName";
            txtName.Size = new Size(323, 27);
            txtName.TabIndex = 3;
            // 
            // lblType
            // 
            lblType.Font = new Font("Segoe UI", 9F);
            lblType.Location = new Point(14, 96);
            lblType.Name = "lblType";
            lblType.Size = new Size(110, 20);
            lblType.TabIndex = 4;
            lblType.Text = "Block Type:";
            // 
            // cboType
            // 
            cboType.Font = new Font("Segoe UI", 9F);
            cboType.Items.AddRange(new object[] { "Residential", "Commercial", "Mixed Use", "Open Space", "Institutional", "Utility", "Other" });
            cboType.Location = new Point(130, 93);
            cboType.Name = "cboType";
            cboType.Size = new Size(323, 28);
            cboType.TabIndex = 5;
            // 
            // lblDepth
            // 
            lblDepth.Font = new Font("Segoe UI", 9F);
            lblDepth.Location = new Point(14, 128);
            lblDepth.Name = "lblDepth";
            lblDepth.Size = new Size(110, 20);
            lblDepth.TabIndex = 6;
            lblDepth.Text = "Block Depth:";
            // 
            // nudDepth
            // 
            nudDepth.DecimalPlaces = 2;
            nudDepth.Font = new Font("Segoe UI", 9F);
            nudDepth.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            nudDepth.Location = new Point(130, 125);
            nudDepth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudDepth.Name = "nudDepth";
            nudDepth.Size = new Size(120, 27);
            nudDepth.TabIndex = 7;
            // 
            // lblDepthUnit
            // 
            lblDepthUnit.Font = new Font("Segoe UI", 9F);
            lblDepthUnit.ForeColor = SystemColors.GrayText;
            lblDepthUnit.Location = new Point(254, 128);
            lblDepthUnit.Name = "lblDepthUnit";
            lblDepthUnit.Size = new Size(80, 20);
            lblDepthUnit.TabIndex = 8;
            lblDepthUnit.Text = "metres";
            //
            // lblLength
            //
            lblLength.Font = new Font("Segoe UI", 9F);
            lblLength.Location = new Point(14, 160);
            lblLength.Name = "lblLength";
            lblLength.Size = new Size(110, 20);
            lblLength.TabIndex = 9;
            lblLength.Text = "Block Length:";
            //
            // nudLength
            //
            nudLength.DecimalPlaces = 2;
            nudLength.Font = new Font("Segoe UI", 9F);
            nudLength.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            nudLength.Location = new Point(130, 157);
            nudLength.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            nudLength.Name = "nudLength";
            nudLength.Size = new Size(120, 27);
            nudLength.TabIndex = 10;
            //
            // lblLengthUnit
            //
            lblLengthUnit.Font = new Font("Segoe UI", 9F);
            lblLengthUnit.ForeColor = SystemColors.GrayText;
            lblLengthUnit.Location = new Point(254, 160);
            lblLengthUnit.Name = "lblLengthUnit";
            lblLengthUnit.Size = new Size(80, 20);
            lblLengthUnit.TabIndex = 11;
            lblLengthUnit.Text = "metres";
            //
            // lblDescription
            //
            lblDescription.Font = new Font("Segoe UI", 9F);
            lblDescription.Location = new Point(14, 194);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(110, 20);
            lblDescription.TabIndex = 12;
            lblDescription.Text = "Description:";
            //
            // txtDescription
            //
            txtDescription.Font = new Font("Segoe UI", 9F);
            txtDescription.Location = new Point(130, 191);
            txtDescription.MaxLength = 500;
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(323, 68);
            txtDescription.TabIndex = 13;
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 287);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(6);
            pnlFooter.Size = new Size(484, 42);
            pnlFooter.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Location = new Point(308, 7);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 28);
            btnSave.TabIndex = 0;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(394, 7);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 28);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmBlockDefinitionEditor
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 329);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmBlockDefinitionEditor";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Block Definition";
            pnlContent.ResumeLayout(false);
            grpDetails.ResumeLayout(false);
            grpDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudDepth).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLength).EndInit();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContent;
        private GroupBox grpDetails;
        private Label lblCode;
        private TextBox txtCode;
        private Label lblName;
        private TextBox txtName;
        private Label lblType;
        private ComboBox cboType;
        private Label lblDepth;
        private NumericUpDown nudDepth;
        private Label lblDepthUnit;
        private Label lblLength;
        private NumericUpDown nudLength;
        private Label lblLengthUnit;
        private Label lblDescription;
        private TextBox txtDescription;
        private Panel pnlFooter;
        private Button btnSave;
        private Button btnCancel;
    }
}
