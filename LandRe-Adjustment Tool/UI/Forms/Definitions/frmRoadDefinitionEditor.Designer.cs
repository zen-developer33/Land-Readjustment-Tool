namespace Land_Readjustment_Tool.UI.Forms.Definitions
{
    partial class frmRoadDefinitionEditor
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
            grpSpecs = new GroupBox();
            lblRowWidth = new Label();
            nudRowWidth = new NumericUpDown();
            lblRowWidthUnit = new Label();
            lblSurface = new Label();
            cboSurface = new ComboBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            grpIdentification = new GroupBox();
            lblCode = new Label();
            txtCode = new TextBox();
            lblName = new Label();
            txtName = new TextBox();
            lblType = new Label();
            cboType = new ComboBox();
            pnlFooter = new Panel();
            btnSave = new Button();
            btnCancel = new Button();
            pnlContent.SuspendLayout();
            grpSpecs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudRowWidth).BeginInit();
            grpIdentification.SuspendLayout();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.AutoScroll = true;
            pnlContent.Controls.Add(grpSpecs);
            pnlContent.Controls.Add(grpIdentification);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10, 8, 10, 4);
            pnlContent.Size = new Size(504, 349);
            pnlContent.TabIndex = 0;
            // 
            // grpSpecs
            // 
            grpSpecs.Controls.Add(lblRowWidth);
            grpSpecs.Controls.Add(nudRowWidth);
            grpSpecs.Controls.Add(lblRowWidthUnit);
            grpSpecs.Controls.Add(lblSurface);
            grpSpecs.Controls.Add(cboSurface);
            grpSpecs.Controls.Add(lblDescription);
            grpSpecs.Controls.Add(txtDescription);
            grpSpecs.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpSpecs.Location = new Point(10, 144);
            grpSpecs.Name = "grpSpecs";
            grpSpecs.Padding = new Padding(8);
            grpSpecs.Size = new Size(484, 224);
            grpSpecs.TabIndex = 1;
            grpSpecs.TabStop = false;
            grpSpecs.Text = "Specifications";
            // 
            // lblRowWidth
            // 
            lblRowWidth.Font = new Font("Segoe UI", 9F);
            lblRowWidth.Location = new Point(14, 30);
            lblRowWidth.Name = "lblRowWidth";
            lblRowWidth.Size = new Size(110, 20);
            lblRowWidth.TabIndex = 0;
            lblRowWidth.Text = "ROW Width:";
            // 
            // nudRowWidth
            // 
            nudRowWidth.DecimalPlaces = 2;
            nudRowWidth.Font = new Font("Segoe UI", 9F);
            nudRowWidth.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            nudRowWidth.Location = new Point(130, 27);
            nudRowWidth.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            nudRowWidth.Name = "nudRowWidth";
            nudRowWidth.Size = new Size(120, 27);
            nudRowWidth.TabIndex = 1;
            // 
            // lblRowWidthUnit
            // 
            lblRowWidthUnit.Font = new Font("Segoe UI", 9F);
            lblRowWidthUnit.ForeColor = SystemColors.GrayText;
            lblRowWidthUnit.Location = new Point(254, 30);
            lblRowWidthUnit.Name = "lblRowWidthUnit";
            lblRowWidthUnit.Size = new Size(80, 20);
            lblRowWidthUnit.TabIndex = 2;
            lblRowWidthUnit.Text = "metres";
            // 
            // lblSurface
            // 
            lblSurface.Font = new Font("Segoe UI", 9F);
            lblSurface.Location = new Point(14, 62);
            lblSurface.Name = "lblSurface";
            lblSurface.Size = new Size(110, 20);
            lblSurface.TabIndex = 3;
            lblSurface.Text = "Surface:";
            // 
            // cboSurface
            // 
            cboSurface.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSurface.Font = new Font("Segoe UI", 9F);
            cboSurface.Items.AddRange(new object[] { "Earthen", "Gravelled", "Blacktopped", "Concrete", "Other" });
            cboSurface.Location = new Point(130, 59);
            cboSurface.Name = "cboSurface";
            cboSurface.Size = new Size(200, 28);
            cboSurface.TabIndex = 4;
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("Segoe UI", 9F);
            lblDescription.Location = new Point(14, 96);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(110, 20);
            lblDescription.TabIndex = 5;
            lblDescription.Text = "Description:";
            // 
            // txtDescription
            // 
            txtDescription.Font = new Font("Segoe UI", 9F);
            txtDescription.Location = new Point(130, 93);
            txtDescription.MaxLength = 500;
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(336, 80);
            txtDescription.TabIndex = 6;
            // 
            // grpIdentification
            // 
            grpIdentification.Controls.Add(lblCode);
            grpIdentification.Controls.Add(txtCode);
            grpIdentification.Controls.Add(lblName);
            grpIdentification.Controls.Add(txtName);
            grpIdentification.Controls.Add(lblType);
            grpIdentification.Controls.Add(cboType);
            grpIdentification.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpIdentification.Location = new Point(10, 8);
            grpIdentification.Name = "grpIdentification";
            grpIdentification.Padding = new Padding(8);
            grpIdentification.Size = new Size(484, 130);
            grpIdentification.TabIndex = 0;
            grpIdentification.TabStop = false;
            grpIdentification.Text = "Identification";
            // 
            // lblCode
            // 
            lblCode.Font = new Font("Segoe UI", 9F);
            lblCode.Location = new Point(14, 30);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(110, 20);
            lblCode.TabIndex = 0;
            lblCode.Text = "Road Code:";
            // 
            // txtCode
            // 
            txtCode.Font = new Font("Segoe UI", 9F);
            txtCode.Location = new Point(130, 27);
            txtCode.MaxLength = 40;
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(336, 27);
            txtCode.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.Location = new Point(14, 62);
            lblName.Name = "lblName";
            lblName.Size = new Size(110, 20);
            lblName.TabIndex = 2;
            lblName.Text = "Road Name:";
            // 
            // txtName
            // 
            txtName.Font = new Font("Segoe UI", 9F);
            txtName.Location = new Point(130, 59);
            txtName.MaxLength = 120;
            txtName.Name = "txtName";
            txtName.Size = new Size(336, 27);
            txtName.TabIndex = 3;
            // 
            // lblType
            // 
            lblType.Font = new Font("Segoe UI", 9F);
            lblType.Location = new Point(14, 94);
            lblType.Name = "lblType";
            lblType.Size = new Size(110, 20);
            lblType.TabIndex = 4;
            lblType.Text = "Road Type:";
            // 
            // cboType
            // 
            cboType.Font = new Font("Segoe UI", 9F);
            cboType.Items.AddRange(new object[] { "Local", "Collector", "Arterial", "Service", "Lane", "Other" });
            cboType.Location = new Point(130, 91);
            cboType.Name = "cboType";
            cboType.Size = new Size(200, 28);
            cboType.TabIndex = 5;
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 349);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(6);
            pnlFooter.Size = new Size(504, 42);
            pnlFooter.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Location = new Point(328, 7);
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
            btnCancel.Location = new Point(414, 7);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 28);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmRoadDefinitionEditor
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(504, 391);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmRoadDefinitionEditor";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Road Definition";
            pnlContent.ResumeLayout(false);
            grpSpecs.ResumeLayout(false);
            grpSpecs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudRowWidth).EndInit();
            grpIdentification.ResumeLayout(false);
            grpIdentification.PerformLayout();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContent;
        private GroupBox grpIdentification;
        private Label lblCode;
        private TextBox txtCode;
        private Label lblName;
        private TextBox txtName;
        private Label lblType;
        private ComboBox cboType;
        private GroupBox grpSpecs;
        private Label lblRowWidth;
        private NumericUpDown nudRowWidth;
        private Label lblRowWidthUnit;
        private Label lblSurface;
        private ComboBox cboSurface;
        private Label lblDescription;
        private TextBox txtDescription;
        private Panel pnlFooter;
        private Button btnSave;
        private Button btnCancel;
    }
}
