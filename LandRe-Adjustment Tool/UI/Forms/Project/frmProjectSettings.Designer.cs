namespace Land_Readjustment_Tool.UI.Forms.Project
{
    partial class frmProjectSettings
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlContent = new Panel();
            tabSettings = new TabControl();
            tabArea = new TabPage();
            grpAreaUnit = new GroupBox();
            lblTraditionalUnit = new Label();
            cmbTraditionalUnit = new ComboBox();
            lblAreaNote = new Label();
            tabCoordinates = new TabPage();
            grpMap = new GroupBox();
            cmbMapUnit = new ComboBox();
            lblMapUnit = new Label();
            grpCoordinates = new GroupBox();
            tabCanvas = new TabPage();
            grpCanvas = new GroupBox();
            lblBgColor = new Label();
            pnlBackgroundColor = new Panel();
            btnPickColor = new Button();
            chkGridVisible = new CheckBox();
            chkSnapEnabled = new CheckBox();
            lblSnapTolerance = new Label();
            nudSnapTolerance = new NumericUpDown();
            lblSnapUnit = new Label();
            tabParcel = new TabPage();
            grpParcelNumbering = new GroupBox();
            lblParcelFormat = new Label();
            cmbParcelFormat = new ComboBox();
            lblParcelPrefix = new Label();
            txtParcelPrefix = new TextBox();
            lblParcelPadding = new Label();
            nudParcelPadding = new NumericUpDown();
            grpReplotting = new GroupBox();
            lblMinPlotArea = new Label();
            nudMinPlotArea = new NumericUpDown();
            lblSqm = new Label();
            tabPrint = new TabPage();
            grpPrint = new GroupBox();
            lblPaperSize = new Label();
            cmbPaperSize = new ComboBox();
            lblPrintScale = new Label();
            lblScalePrefix = new Label();
            nudPrintScale = new NumericUpDown();
            pnlFooter = new Panel();
            btnOK = new Button();
            btnCancel = new Button();
            btnDefaults = new Button();
            lblSubtitle = new Label();
            lblTitle = new Label();
            pnlHeader = new Panel();
            pnlContent.SuspendLayout();
            tabSettings.SuspendLayout();
            tabArea.SuspendLayout();
            grpAreaUnit.SuspendLayout();
            tabCoordinates.SuspendLayout();
            grpMap.SuspendLayout();
            tabCanvas.SuspendLayout();
            grpCanvas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSnapTolerance).BeginInit();
            tabParcel.SuspendLayout();
            grpParcelNumbering.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudParcelPadding).BeginInit();
            grpReplotting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPlotArea).BeginInit();
            tabPrint.SuspendLayout();
            grpPrint.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPrintScale).BeginInit();
            pnlFooter.SuspendLayout();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(245, 245, 248);
            pnlContent.Controls.Add(tabSettings);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 70);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(12, 10, 12, 10);
            pnlContent.Size = new Size(443, 431);
            pnlContent.TabIndex = 0;
            // 
            // tabSettings
            // 
            tabSettings.Controls.Add(tabArea);
            tabSettings.Controls.Add(tabCoordinates);
            tabSettings.Controls.Add(tabCanvas);
            tabSettings.Controls.Add(tabParcel);
            tabSettings.Controls.Add(tabPrint);
            tabSettings.Dock = DockStyle.Fill;
            tabSettings.Font = new Font("Segoe UI", 9F);
            tabSettings.Location = new Point(12, 10);
            tabSettings.Name = "tabSettings";
            tabSettings.Padding = new Point(12, 6);
            tabSettings.SelectedIndex = 0;
            tabSettings.Size = new Size(419, 411);
            tabSettings.TabIndex = 0;
            // 
            // tabArea
            // 
            tabArea.BackColor = Color.FromArgb(250, 250, 252);
            tabArea.Controls.Add(grpAreaUnit);
            tabArea.Location = new Point(4, 35);
            tabArea.Name = "tabArea";
            tabArea.Padding = new Padding(10);
            tabArea.Size = new Size(411, 372);
            tabArea.TabIndex = 0;
            tabArea.Text = "Area Units";
            // 
            // grpAreaUnit
            // 
            grpAreaUnit.Controls.Add(lblTraditionalUnit);
            grpAreaUnit.Controls.Add(cmbTraditionalUnit);
            grpAreaUnit.Controls.Add(lblAreaNote);
            grpAreaUnit.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpAreaUnit.ForeColor = Color.FromArgb(40, 50, 65);
            grpAreaUnit.Location = new Point(7, 13);
            grpAreaUnit.Name = "grpAreaUnit";
            grpAreaUnit.Size = new Size(419, 130);
            grpAreaUnit.TabIndex = 0;
            grpAreaUnit.TabStop = false;
            grpAreaUnit.Text = "Traditional Area Display Unit";
            // 
            // lblTraditionalUnit
            // 
            lblTraditionalUnit.Font = new Font("Segoe UI", 9F);
            lblTraditionalUnit.ForeColor = Color.FromArgb(60, 70, 85);
            lblTraditionalUnit.Location = new Point(16, 34);
            lblTraditionalUnit.Name = "lblTraditionalUnit";
            lblTraditionalUnit.Size = new Size(163, 20);
            lblTraditionalUnit.TabIndex = 0;
            lblTraditionalUnit.Text = "Display Unit:";
            // 
            // cmbTraditionalUnit
            // 
            cmbTraditionalUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTraditionalUnit.Font = new Font("Segoe UI", 9F);
            cmbTraditionalUnit.Location = new Point(16, 56);
            cmbTraditionalUnit.Name = "cmbTraditionalUnit";
            cmbTraditionalUnit.Size = new Size(300, 28);
            cmbTraditionalUnit.TabIndex = 1;
            // 
            // lblAreaNote
            // 
            lblAreaNote.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblAreaNote.ForeColor = Color.FromArgb(100, 120, 150);
            lblAreaNote.Location = new Point(16, 92);
            lblAreaNote.Name = "lblAreaNote";
            lblAreaNote.Size = new Size(460, 20);
            lblAreaNote.TabIndex = 2;
            lblAreaNote.Text = "ℹ  All calculations always use Square Meters internally.";
            // 
            // tabCoordinates
            // 
            tabCoordinates.BackColor = Color.FromArgb(250, 250, 252);
            tabCoordinates.Controls.Add(grpMap);
            tabCoordinates.Controls.Add(grpCoordinates);
            tabCoordinates.Location = new Point(4, 35);
            tabCoordinates.Name = "tabCoordinates";
            tabCoordinates.Padding = new Padding(10);
            tabCoordinates.Size = new Size(411, 372);
            tabCoordinates.TabIndex = 1;
            tabCoordinates.Text = "Coordinates";
            // 
            // grpMap
            // 
            grpMap.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpMap.Controls.Add(cmbMapUnit);
            grpMap.Controls.Add(lblMapUnit);
            grpMap.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpMap.ForeColor = Color.FromArgb(40, 50, 65);
            grpMap.Location = new Point(10, 266);
            grpMap.Name = "grpMap";
            grpMap.Size = new Size(388, 93);
            grpMap.TabIndex = 9;
            grpMap.TabStop = false;
            grpMap.Text = "Map Settings";
            // 
            // cmbMapUnit
            // 
            cmbMapUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMapUnit.Font = new Font("Segoe UI", 9F);
            cmbMapUnit.Location = new Point(17, 52);
            cmbMapUnit.Name = "cmbMapUnit";
            cmbMapUnit.Size = new Size(150, 28);
            cmbMapUnit.TabIndex = 3;
            // 
            // lblMapUnit
            // 
            lblMapUnit.Font = new Font("Segoe UI", 9F);
            lblMapUnit.ForeColor = Color.FromArgb(60, 70, 85);
            lblMapUnit.Location = new Point(17, 30);
            lblMapUnit.Name = "lblMapUnit";
            lblMapUnit.Size = new Size(100, 20);
            lblMapUnit.TabIndex = 2;
            lblMapUnit.Text = "Map Unit:";
            // 
            // grpCoordinates
            // 
            grpCoordinates.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpCoordinates.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpCoordinates.ForeColor = Color.FromArgb(40, 50, 65);
            grpCoordinates.Location = new Point(10, 10);
            grpCoordinates.Name = "grpCoordinates";
            grpCoordinates.Size = new Size(388, 206);
            grpCoordinates.TabIndex = 0;
            grpCoordinates.TabStop = false;
            grpCoordinates.Text = "Coordinate Reference System";
            // 
            // tabCanvas
            // 
            tabCanvas.BackColor = Color.FromArgb(250, 250, 252);
            tabCanvas.Controls.Add(grpCanvas);
            tabCanvas.Location = new Point(4, 35);
            tabCanvas.Name = "tabCanvas";
            tabCanvas.Padding = new Padding(10);
            tabCanvas.Size = new Size(411, 372);
            tabCanvas.TabIndex = 2;
            tabCanvas.Text = "Canvas";
            // 
            // grpCanvas
            // 
            grpCanvas.Controls.Add(lblBgColor);
            grpCanvas.Controls.Add(pnlBackgroundColor);
            grpCanvas.Controls.Add(btnPickColor);
            grpCanvas.Controls.Add(chkGridVisible);
            grpCanvas.Controls.Add(chkSnapEnabled);
            grpCanvas.Controls.Add(lblSnapTolerance);
            grpCanvas.Controls.Add(nudSnapTolerance);
            grpCanvas.Controls.Add(lblSnapUnit);
            grpCanvas.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpCanvas.ForeColor = Color.FromArgb(40, 50, 65);
            grpCanvas.Location = new Point(10, 10);
            grpCanvas.Name = "grpCanvas";
            grpCanvas.Size = new Size(490, 220);
            grpCanvas.TabIndex = 0;
            grpCanvas.TabStop = false;
            grpCanvas.Text = "Canvas Display";
            // 
            // lblBgColor
            // 
            lblBgColor.Font = new Font("Segoe UI", 9F);
            lblBgColor.ForeColor = Color.FromArgb(60, 70, 85);
            lblBgColor.Location = new Point(16, 34);
            lblBgColor.Name = "lblBgColor";
            lblBgColor.Size = new Size(184, 20);
            lblBgColor.TabIndex = 0;
            lblBgColor.Text = "Background Color:";
            // 
            // pnlBackgroundColor
            // 
            pnlBackgroundColor.BackColor = Color.FromArgb(30, 41, 51);
            pnlBackgroundColor.BorderStyle = BorderStyle.FixedSingle;
            pnlBackgroundColor.Cursor = Cursors.Hand;
            pnlBackgroundColor.Location = new Point(20, 57);
            pnlBackgroundColor.Name = "pnlBackgroundColor";
            pnlBackgroundColor.Size = new Size(60, 28);
            pnlBackgroundColor.TabIndex = 1;
            // 
            // btnPickColor
            // 
            btnPickColor.Cursor = Cursors.Hand;
            btnPickColor.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 190);
            btnPickColor.FlatStyle = FlatStyle.Flat;
            btnPickColor.Font = new Font("Segoe UI", 9F);
            btnPickColor.Location = new Point(86, 57);
            btnPickColor.Name = "btnPickColor";
            btnPickColor.Size = new Size(137, 28);
            btnPickColor.TabIndex = 2;
            btnPickColor.Text = "Choose Color...";
            // 
            // chkGridVisible
            // 
            chkGridVisible.Font = new Font("Segoe UI", 9F);
            chkGridVisible.ForeColor = Color.FromArgb(50, 60, 75);
            chkGridVisible.Location = new Point(16, 102);
            chkGridVisible.Name = "chkGridVisible";
            chkGridVisible.Size = new Size(120, 22);
            chkGridVisible.TabIndex = 3;
            chkGridVisible.Text = "Show Grid";
            // 
            // chkSnapEnabled
            // 
            chkSnapEnabled.Font = new Font("Segoe UI", 9F);
            chkSnapEnabled.ForeColor = Color.FromArgb(50, 60, 75);
            chkSnapEnabled.Location = new Point(16, 130);
            chkSnapEnabled.Name = "chkSnapEnabled";
            chkSnapEnabled.Size = new Size(120, 22);
            chkSnapEnabled.TabIndex = 4;
            chkSnapEnabled.Text = "Enable Snap";
            // 
            // lblSnapTolerance
            // 
            lblSnapTolerance.Font = new Font("Segoe UI", 9F);
            lblSnapTolerance.ForeColor = Color.FromArgb(60, 70, 85);
            lblSnapTolerance.Location = new Point(16, 162);
            lblSnapTolerance.Name = "lblSnapTolerance";
            lblSnapTolerance.Size = new Size(120, 20);
            lblSnapTolerance.TabIndex = 5;
            lblSnapTolerance.Text = "Snap Tolerance:";
            // 
            // nudSnapTolerance
            // 
            nudSnapTolerance.DecimalPlaces = 1;
            nudSnapTolerance.Font = new Font("Segoe UI", 9F);
            nudSnapTolerance.Location = new Point(143, 160);
            nudSnapTolerance.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudSnapTolerance.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSnapTolerance.Name = "nudSnapTolerance";
            nudSnapTolerance.Size = new Size(70, 27);
            nudSnapTolerance.TabIndex = 6;
            nudSnapTolerance.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblSnapUnit
            // 
            lblSnapUnit.Font = new Font("Segoe UI", 9F);
            lblSnapUnit.ForeColor = Color.FromArgb(100, 110, 125);
            lblSnapUnit.Location = new Point(216, 162);
            lblSnapUnit.Name = "lblSnapUnit";
            lblSnapUnit.Size = new Size(50, 20);
            lblSnapUnit.TabIndex = 7;
            lblSnapUnit.Text = "pixels";
            // 
            // tabParcel
            // 
            tabParcel.BackColor = Color.FromArgb(250, 250, 252);
            tabParcel.Controls.Add(grpParcelNumbering);
            tabParcel.Controls.Add(grpReplotting);
            tabParcel.Location = new Point(4, 35);
            tabParcel.Name = "tabParcel";
            tabParcel.Padding = new Padding(10);
            tabParcel.Size = new Size(411, 372);
            tabParcel.TabIndex = 3;
            tabParcel.Text = "Parcels";
            // 
            // grpParcelNumbering
            // 
            grpParcelNumbering.Controls.Add(lblParcelFormat);
            grpParcelNumbering.Controls.Add(cmbParcelFormat);
            grpParcelNumbering.Controls.Add(lblParcelPrefix);
            grpParcelNumbering.Controls.Add(txtParcelPrefix);
            grpParcelNumbering.Controls.Add(lblParcelPadding);
            grpParcelNumbering.Controls.Add(nudParcelPadding);
            grpParcelNumbering.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpParcelNumbering.ForeColor = Color.FromArgb(40, 50, 65);
            grpParcelNumbering.Location = new Point(10, 10);
            grpParcelNumbering.Name = "grpParcelNumbering";
            grpParcelNumbering.Size = new Size(490, 160);
            grpParcelNumbering.TabIndex = 0;
            grpParcelNumbering.TabStop = false;
            grpParcelNumbering.Text = "Parcel Numbering";
            // 
            // lblParcelFormat
            // 
            lblParcelFormat.Font = new Font("Segoe UI", 9F);
            lblParcelFormat.ForeColor = Color.FromArgb(60, 70, 85);
            lblParcelFormat.Location = new Point(16, 34);
            lblParcelFormat.Name = "lblParcelFormat";
            lblParcelFormat.Size = new Size(80, 20);
            lblParcelFormat.TabIndex = 0;
            lblParcelFormat.Text = "Format:";
            // 
            // cmbParcelFormat
            // 
            cmbParcelFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbParcelFormat.Font = new Font("Segoe UI", 9F);
            cmbParcelFormat.Location = new Point(100, 32);
            cmbParcelFormat.Name = "cmbParcelFormat";
            cmbParcelFormat.Size = new Size(150, 28);
            cmbParcelFormat.TabIndex = 1;
            // 
            // lblParcelPrefix
            // 
            lblParcelPrefix.Font = new Font("Segoe UI", 9F);
            lblParcelPrefix.ForeColor = Color.FromArgb(60, 70, 85);
            lblParcelPrefix.Location = new Point(16, 74);
            lblParcelPrefix.Name = "lblParcelPrefix";
            lblParcelPrefix.Size = new Size(80, 20);
            lblParcelPrefix.TabIndex = 2;
            lblParcelPrefix.Text = "Prefix:";
            // 
            // txtParcelPrefix
            // 
            txtParcelPrefix.Font = new Font("Segoe UI", 9F);
            txtParcelPrefix.Location = new Point(100, 72);
            txtParcelPrefix.Name = "txtParcelPrefix";
            txtParcelPrefix.PlaceholderText = "e.g. RP-";
            txtParcelPrefix.Size = new Size(100, 27);
            txtParcelPrefix.TabIndex = 3;
            // 
            // lblParcelPadding
            // 
            lblParcelPadding.Font = new Font("Segoe UI", 9F);
            lblParcelPadding.ForeColor = Color.FromArgb(60, 70, 85);
            lblParcelPadding.Location = new Point(16, 114);
            lblParcelPadding.Name = "lblParcelPadding";
            lblParcelPadding.Size = new Size(100, 20);
            lblParcelPadding.TabIndex = 4;
            lblParcelPadding.Text = "Digit Padding:";
            // 
            // nudParcelPadding
            // 
            nudParcelPadding.Font = new Font("Segoe UI", 9F);
            nudParcelPadding.Location = new Point(120, 112);
            nudParcelPadding.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            nudParcelPadding.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudParcelPadding.Name = "nudParcelPadding";
            nudParcelPadding.Size = new Size(60, 27);
            nudParcelPadding.TabIndex = 5;
            nudParcelPadding.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // grpReplotting
            // 
            grpReplotting.Controls.Add(lblMinPlotArea);
            grpReplotting.Controls.Add(nudMinPlotArea);
            grpReplotting.Controls.Add(lblSqm);
            grpReplotting.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpReplotting.ForeColor = Color.FromArgb(40, 50, 65);
            grpReplotting.Location = new Point(10, 180);
            grpReplotting.Name = "grpReplotting";
            grpReplotting.Size = new Size(490, 80);
            grpReplotting.TabIndex = 1;
            grpReplotting.TabStop = false;
            grpReplotting.Text = "Replotting Rules";
            // 
            // lblMinPlotArea
            // 
            lblMinPlotArea.Font = new Font("Segoe UI", 9F);
            lblMinPlotArea.ForeColor = Color.FromArgb(60, 70, 85);
            lblMinPlotArea.Location = new Point(16, 32);
            lblMinPlotArea.Name = "lblMinPlotArea";
            lblMinPlotArea.Size = new Size(130, 20);
            lblMinPlotArea.TabIndex = 0;
            lblMinPlotArea.Text = "Minimum Plot Area:";
            // 
            // nudMinPlotArea
            // 
            nudMinPlotArea.DecimalPlaces = 2;
            nudMinPlotArea.Font = new Font("Segoe UI", 9F);
            nudMinPlotArea.Location = new Point(150, 30);
            nudMinPlotArea.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudMinPlotArea.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudMinPlotArea.Name = "nudMinPlotArea";
            nudMinPlotArea.Size = new Size(90, 27);
            nudMinPlotArea.TabIndex = 1;
            nudMinPlotArea.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblSqm
            // 
            lblSqm.Font = new Font("Segoe UI", 9F);
            lblSqm.ForeColor = Color.FromArgb(100, 110, 125);
            lblSqm.Location = new Point(246, 32);
            lblSqm.Name = "lblSqm";
            lblSqm.Size = new Size(40, 20);
            lblSqm.TabIndex = 2;
            lblSqm.Text = "Sq.m";
            // 
            // tabPrint
            // 
            tabPrint.BackColor = Color.FromArgb(250, 250, 252);
            tabPrint.Controls.Add(grpPrint);
            tabPrint.Location = new Point(4, 35);
            tabPrint.Name = "tabPrint";
            tabPrint.Padding = new Padding(10);
            tabPrint.Size = new Size(411, 372);
            tabPrint.TabIndex = 5;
            tabPrint.Text = "Print";
            // 
            // grpPrint
            // 
            grpPrint.Controls.Add(lblPaperSize);
            grpPrint.Controls.Add(cmbPaperSize);
            grpPrint.Controls.Add(lblPrintScale);
            grpPrint.Controls.Add(lblScalePrefix);
            grpPrint.Controls.Add(nudPrintScale);
            grpPrint.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpPrint.ForeColor = Color.FromArgb(40, 50, 65);
            grpPrint.Location = new Point(10, 10);
            grpPrint.Name = "grpPrint";
            grpPrint.Size = new Size(490, 150);
            grpPrint.TabIndex = 0;
            grpPrint.TabStop = false;
            grpPrint.Text = "Print & Export";
            // 
            // lblPaperSize
            // 
            lblPaperSize.Font = new Font("Segoe UI", 9F);
            lblPaperSize.ForeColor = Color.FromArgb(60, 70, 85);
            lblPaperSize.Location = new Point(16, 34);
            lblPaperSize.Name = "lblPaperSize";
            lblPaperSize.Size = new Size(163, 20);
            lblPaperSize.TabIndex = 0;
            lblPaperSize.Text = "Default Paper Size:";
            // 
            // cmbPaperSize
            // 
            cmbPaperSize.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaperSize.Font = new Font("Segoe UI", 9F);
            cmbPaperSize.Location = new Point(16, 56);
            cmbPaperSize.Name = "cmbPaperSize";
            cmbPaperSize.Size = new Size(134, 28);
            cmbPaperSize.TabIndex = 1;
            // 
            // lblPrintScale
            // 
            lblPrintScale.Font = new Font("Segoe UI", 9F);
            lblPrintScale.ForeColor = Color.FromArgb(60, 70, 85);
            lblPrintScale.Location = new Point(16, 96);
            lblPrintScale.Name = "lblPrintScale";
            lblPrintScale.Size = new Size(130, 20);
            lblPrintScale.TabIndex = 2;
            lblPrintScale.Text = "Default Print Scale:";
            // 
            // lblScalePrefix
            // 
            lblScalePrefix.Font = new Font("Segoe UI", 9F);
            lblScalePrefix.ForeColor = Color.FromArgb(60, 70, 85);
            lblScalePrefix.Location = new Point(16, 120);
            lblScalePrefix.Name = "lblScalePrefix";
            lblScalePrefix.Size = new Size(30, 24);
            lblScalePrefix.TabIndex = 3;
            lblScalePrefix.Text = "1 :";
            // 
            // nudPrintScale
            // 
            nudPrintScale.Font = new Font("Segoe UI", 9F);
            nudPrintScale.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            nudPrintScale.Location = new Point(50, 118);
            nudPrintScale.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudPrintScale.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            nudPrintScale.Name = "nudPrintScale";
            nudPrintScale.Size = new Size(100, 27);
            nudPrintScale.TabIndex = 4;
            nudPrintScale.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.FromArgb(235, 235, 240);
            pnlFooter.Controls.Add(btnOK);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnDefaults);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 501);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(12, 10, 12, 10);
            pnlFooter.Size = new Size(443, 52);
            pnlFooter.TabIndex = 1;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.BackColor = Color.FromArgb(32, 40, 52);
            btnOK.Cursor = Cursors.Hand;
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(235, 10);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(110, 32);
            btnOK.TabIndex = 0;
            btnOK.Text = "Save Settings";
            btnOK.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.BackColor = Color.White;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 210);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9F);
            btnCancel.ForeColor = Color.FromArgb(60, 60, 70);
            btnCancel.Location = new Point(351, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 32);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnDefaults
            // 
            btnDefaults.BackColor = Color.White;
            btnDefaults.Cursor = Cursors.Hand;
            btnDefaults.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 210);
            btnDefaults.FlatStyle = FlatStyle.Flat;
            btnDefaults.Font = new Font("Segoe UI", 9F);
            btnDefaults.ForeColor = Color.FromArgb(80, 80, 90);
            btnDefaults.Location = new Point(12, 10);
            btnDefaults.Name = "btnDefaults";
            btnDefaults.Size = new Size(135, 32);
            btnDefaults.TabIndex = 2;
            btnDefaults.Text = "Restore Defaults";
            btnDefaults.UseVisualStyleBackColor = false;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(180, 190, 200);
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(452, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Configure coordinate system, area units, canvas and output options";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(185, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Project Settings";
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(32, 40, 52);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(20, 12, 20, 12);
            pnlHeader.Size = new Size(443, 70);
            pnlHeader.TabIndex = 2;
            // 
            // frmProjectSettings
            // 
            BackColor = Color.FromArgb(245, 245, 248);
            ClientSize = new Size(443, 553);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MaximumSize = new Size(1000, 1000);
            MinimizeBox = false;
            Name = "frmProjectSettings";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Project Settings";
            pnlContent.ResumeLayout(false);
            tabSettings.ResumeLayout(false);
            tabArea.ResumeLayout(false);
            grpAreaUnit.ResumeLayout(false);
            tabCoordinates.ResumeLayout(false);
            grpMap.ResumeLayout(false);
            tabCanvas.ResumeLayout(false);
            grpCanvas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudSnapTolerance).EndInit();
            tabParcel.ResumeLayout(false);
            grpParcelNumbering.ResumeLayout(false);
            grpParcelNumbering.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudParcelPadding).EndInit();
            grpReplotting.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudMinPlotArea).EndInit();
            tabPrint.ResumeLayout(false);
            grpPrint.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudPrintScale).EndInit();
            pnlFooter.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel pnlContent;
        private Panel pnlFooter;
        private Button btnOK;
        private Button btnCancel;
        private Button btnDefaults;
        private Label lblSubtitle;
        private Label lblTitle;
        private Panel pnlHeader;
        private TabControl tabSettings;
        private TabPage tabArea;
        private GroupBox grpAreaUnit;
        private Label lblTraditionalUnit;
        private ComboBox cmbTraditionalUnit;
        private Label lblAreaNote;
        private TabPage tabCoordinates;
        private GroupBox grpCoordinates;
        private Label lblMapUnit;
        private ComboBox cmbMapUnit;
        private TabPage tabCanvas;
        private GroupBox grpCanvas;
        private Label lblBgColor;
        private Panel pnlBackgroundColor;
        private Button btnPickColor;
        private CheckBox chkGridVisible;
        private CheckBox chkSnapEnabled;
        private Label lblSnapTolerance;
        private NumericUpDown nudSnapTolerance;
        private Label lblSnapUnit;
        private TabPage tabParcel;
        private GroupBox grpParcelNumbering;
        private Label lblParcelFormat;
        private ComboBox cmbParcelFormat;
        private Label lblParcelPrefix;
        private TextBox txtParcelPrefix;
        private Label lblParcelPadding;
        private NumericUpDown nudParcelPadding;
        private GroupBox grpReplotting;
        private Label lblMinPlotArea;
        private NumericUpDown nudMinPlotArea;
        private Label lblSqm;
        private TabPage tabPrint;
        private GroupBox grpPrint;
        private Label lblPaperSize;
        private ComboBox cmbPaperSize;
        private Label lblPrintScale;
        private Label lblScalePrefix;
        private NumericUpDown nudPrintScale;
        private GroupBox grpMap;
    }
}