namespace Land_Readjustment_Tool.UI.Forms.Project
{
    partial class frmProjectSettings
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
            lblFormSubtitle = new Label();
            tabSettings = new TabControl();
            tabCoordinates = new TabPage();
            grpCRS = new GroupBox();
            lblCRS = new Label();
            cmbCRS = new ComboBox();
            btnManageCRS = new Button();
            lblCrsInfo = new Label();
            grpDatumTransformation = new GroupBox();
            lblDatum = new Label();
            cmbDatumTransformation = new ComboBox();
            btnManageDatum = new Button();
            lblDatumNote = new Label();
            tabArea = new TabPage();
            grpAreaUnit = new GroupBox();
            lblTraditionalUnit = new Label();
            cmbTraditionalUnit = new ComboBox();
            lblAreaNote = new Label();
            tabCanvas = new TabPage();
            grpCanvas = new GroupBox();
            lblBgColor = new Label();
            pnlBgColor = new Panel();
            btnPickColor = new Button();
            lblBgColorHex = new Label();
            chkGridVisible = new CheckBox();
            chkSnapEnabled = new CheckBox();
            lblSnapTolerance = new Label();
            nudSnapTolerance = new NumericUpDown();
            lblSnapUnit = new Label();
            tabParcel = new TabPage();
            grpParcelNum = new GroupBox();
            lblParcelFormat = new Label();
            cmbParcelFormat = new ComboBox();
            lblParcelPrefix = new Label();
            txtParcelPrefix = new TextBox();
            lblPadding = new Label();
            nudPadding = new NumericUpDown();
            grpReplotRules = new GroupBox();
            lblMinPlot = new Label();
            nudMinPlot = new NumericUpDown();
            lblSqm = new Label();
            tabDocument = new TabPage();
            grpDocument = new GroupBox();
            lblLanguage = new Label();
            cmbLanguage = new ComboBox();
            lblDateFormat = new Label();
            cmbDateFormat = new ComboBox();
            tabPrint = new TabPage();
            grpPrint = new GroupBox();
            lblPaperSize = new Label();
            cmbPaperSize = new ComboBox();
            lblScale = new Label();
            lblScalePrefix = new Label();
            nudPrintScale = new NumericUpDown();
            pnlContent = new Panel();
            pnlFooter = new Panel();
            btnOK = new Button();
            btnCancel = new Button();
            lblStatus = new Label();
            btnRestoreDefaults = new Button();
            pnlHeader.SuspendLayout();
            tabSettings.SuspendLayout();
            tabCoordinates.SuspendLayout();
            grpCRS.SuspendLayout();
            grpDatumTransformation.SuspendLayout();
            tabArea.SuspendLayout();
            grpAreaUnit.SuspendLayout();
            tabCanvas.SuspendLayout();
            grpCanvas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSnapTolerance).BeginInit();
            tabParcel.SuspendLayout();
            grpParcelNum.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPadding).BeginInit();
            grpReplotRules.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPlot).BeginInit();
            tabDocument.SuspendLayout();
            grpDocument.SuspendLayout();
            tabPrint.SuspendLayout();
            grpPrint.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPrintScale).BeginInit();
            pnlContent.SuspendLayout();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(28, 36, 54);
            pnlHeader.Controls.Add(lblFormTitle);
            pnlHeader.Controls.Add(lblFormSubtitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(582, 68);
            pnlHeader.TabIndex = 2;
            pnlHeader.Paint += pnlHeader_Paint;
            // 
            // lblFormTitle
            // 
            lblFormTitle.AutoSize = true;
            lblFormTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblFormTitle.ForeColor = Color.White;
            lblFormTitle.Location = new Point(18, 10);
            lblFormTitle.Name = "lblFormTitle";
            lblFormTitle.Size = new Size(178, 30);
            lblFormTitle.TabIndex = 0;
            lblFormTitle.Text = "Project Settings";
            // 
            // lblFormSubtitle
            // 
            lblFormSubtitle.AutoSize = true;
            lblFormSubtitle.Font = new Font("Segoe UI", 8.5F);
            lblFormSubtitle.ForeColor = Color.FromArgb(170, 185, 210);
            lblFormSubtitle.Location = new Point(19, 38);
            lblFormSubtitle.Name = "lblFormSubtitle";
            lblFormSubtitle.Size = new Size(419, 20);
            lblFormSubtitle.TabIndex = 1;
            lblFormSubtitle.Text = "Configure coordinate system, display units and output options";
            // 
            // tabSettings
            // 
            tabSettings.Controls.Add(tabCoordinates);
            tabSettings.Controls.Add(tabArea);
            tabSettings.Controls.Add(tabCanvas);
            tabSettings.Controls.Add(tabParcel);
            tabSettings.Controls.Add(tabDocument);
            tabSettings.Controls.Add(tabPrint);
            tabSettings.Dock = DockStyle.Fill;
            tabSettings.Font = new Font("Segoe UI", 9F);
            tabSettings.Location = new Point(10, 8);
            tabSettings.Name = "tabSettings";
            tabSettings.Padding = new Point(10, 5);
            tabSettings.SelectedIndex = 0;
            tabSettings.Size = new Size(562, 499);
            tabSettings.TabIndex = 0;
            // 
            // tabCoordinates
            // 
            tabCoordinates.BackColor = Color.FromArgb(248, 249, 252);
            tabCoordinates.Controls.Add(grpCRS);
            tabCoordinates.Controls.Add(grpDatumTransformation);
            tabCoordinates.Location = new Point(4, 33);
            tabCoordinates.Name = "tabCoordinates";
            tabCoordinates.Padding = new Padding(10);
            tabCoordinates.Size = new Size(554, 462);
            tabCoordinates.TabIndex = 0;
            tabCoordinates.Text = "📍 Coordinate System";
            // 
            // grpCRS
            // 
            grpCRS.Controls.Add(lblCRS);
            grpCRS.Controls.Add(cmbCRS);
            grpCRS.Controls.Add(btnManageCRS);
            grpCRS.Controls.Add(lblCrsInfo);
            grpCRS.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpCRS.ForeColor = Color.FromArgb(35, 45, 65);
            grpCRS.Location = new Point(10, 10);
            grpCRS.Name = "grpCRS";
            grpCRS.Size = new Size(545, 124);
            grpCRS.TabIndex = 0;
            grpCRS.TabStop = false;
            grpCRS.Text = "Coordinate Reference System";
            // 
            // lblCRS
            // 
            lblCRS.Font = new Font("Segoe UI", 9F);
            lblCRS.ForeColor = Color.FromArgb(55, 65, 85);
            lblCRS.Location = new Point(14, 30);
            lblCRS.Name = "lblCRS";
            lblCRS.Size = new Size(80, 20);
            lblCRS.TabIndex = 0;
            lblCRS.Text = "Select CRS:";
            // 
            // cmbCRS
            // 
            cmbCRS.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCRS.Font = new Font("Segoe UI", 9F);
            cmbCRS.Location = new Point(14, 52);
            cmbCRS.Name = "cmbCRS";
            cmbCRS.Size = new Size(400, 28);
            cmbCRS.TabIndex = 1;
            cmbCRS.SelectedIndexChanged += cmbCRS_SelectedIndexChanged;
            // 
            // btnManageCRS
            // 
            btnManageCRS.BackColor = Color.White;
            btnManageCRS.Cursor = Cursors.Hand;
            btnManageCRS.FlatAppearance.BorderColor = Color.FromArgb(28, 36, 54);
            btnManageCRS.FlatStyle = FlatStyle.Flat;
            btnManageCRS.Font = new Font("Segoe UI", 9F);
            btnManageCRS.ForeColor = Color.FromArgb(28, 36, 54);
            btnManageCRS.Location = new Point(421, 50);
            btnManageCRS.Name = "btnManageCRS";
            btnManageCRS.Size = new Size(110, 31);
            btnManageCRS.TabIndex = 2;
            btnManageCRS.Text = "⚙ Manage";
            btnManageCRS.UseVisualStyleBackColor = false;
            btnManageCRS.Click += btnManageCRS_Click;
            // 
            // lblCrsInfo
            // 
            lblCrsInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblCrsInfo.ForeColor = Color.FromArgb(90, 110, 150);
            lblCrsInfo.Location = new Point(14, 86);
            lblCrsInfo.Name = "lblCrsInfo";
            lblCrsInfo.Size = new Size(516, 20);
            lblCrsInfo.TabIndex = 3;
            // 
            // grpDatumTransformation
            // 
            grpDatumTransformation.Controls.Add(lblDatum);
            grpDatumTransformation.Controls.Add(cmbDatumTransformation);
            grpDatumTransformation.Controls.Add(btnManageDatum);
            grpDatumTransformation.Controls.Add(lblDatumNote);
            grpDatumTransformation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpDatumTransformation.ForeColor = Color.FromArgb(35, 45, 65);
            grpDatumTransformation.Location = new Point(10, 140);
            grpDatumTransformation.Name = "grpDatumTransformation";
            grpDatumTransformation.Size = new Size(545, 130);
            grpDatumTransformation.TabIndex = 1;
            grpDatumTransformation.TabStop = false;
            grpDatumTransformation.Text = "Datum Transformation (Required for MUTM zones)";
            // 
            // lblDatum
            // 
            lblDatum.Font = new Font("Segoe UI", 9F);
            lblDatum.ForeColor = Color.FromArgb(55, 65, 85);
            lblDatum.Location = new Point(14, 30);
            lblDatum.Name = "lblDatum";
            lblDatum.Size = new Size(150, 20);
            lblDatum.TabIndex = 0;
            lblDatum.Text = "Select Transformation:";
            // 
            // cmbDatumTransformation
            // 
            cmbDatumTransformation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDatumTransformation.Font = new Font("Segoe UI", 9F);
            cmbDatumTransformation.Location = new Point(14, 52);
            cmbDatumTransformation.Name = "cmbDatumTransformation";
            cmbDatumTransformation.Size = new Size(400, 28);
            cmbDatumTransformation.TabIndex = 1;
            // 
            // btnManageDatum
            // 
            btnManageDatum.BackColor = Color.White;
            btnManageDatum.Cursor = Cursors.Hand;
            btnManageDatum.FlatAppearance.BorderColor = Color.FromArgb(28, 36, 54);
            btnManageDatum.FlatStyle = FlatStyle.Flat;
            btnManageDatum.Font = new Font("Segoe UI", 9F);
            btnManageDatum.ForeColor = Color.FromArgb(28, 36, 54);
            btnManageDatum.Location = new Point(418, 49);
            btnManageDatum.Name = "btnManageDatum";
            btnManageDatum.Size = new Size(110, 33);
            btnManageDatum.TabIndex = 2;
            btnManageDatum.Text = "⚙ Manage";
            btnManageDatum.UseVisualStyleBackColor = false;
            btnManageDatum.Click += btnManageDatum_Click;
            // 
            // lblDatumNote
            // 
            lblDatumNote.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblDatumNote.ForeColor = Color.FromArgb(90, 110, 150);
            lblDatumNote.Location = new Point(14, 88);
            lblDatumNote.Name = "lblDatumNote";
            lblDatumNote.Size = new Size(516, 20);
            lblDatumNote.TabIndex = 3;
            lblDatumNote.Text = "ℹ  Used for converting between local datum (Everest) and WGS84.";
            // 
            // tabArea
            // 
            tabArea.BackColor = Color.FromArgb(248, 249, 252);
            tabArea.Controls.Add(grpAreaUnit);
            tabArea.Location = new Point(4, 33);
            tabArea.Name = "tabArea";
            tabArea.Padding = new Padding(10);
            tabArea.Size = new Size(554, 462);
            tabArea.TabIndex = 1;
            tabArea.Text = "📐 Area Units";
            // 
            // grpAreaUnit
            // 
            grpAreaUnit.Controls.Add(lblTraditionalUnit);
            grpAreaUnit.Controls.Add(cmbTraditionalUnit);
            grpAreaUnit.Controls.Add(lblAreaNote);
            grpAreaUnit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpAreaUnit.ForeColor = Color.FromArgb(35, 45, 65);
            grpAreaUnit.Location = new Point(10, 10);
            grpAreaUnit.Name = "grpAreaUnit";
            grpAreaUnit.Size = new Size(545, 120);
            grpAreaUnit.TabIndex = 0;
            grpAreaUnit.TabStop = false;
            grpAreaUnit.Text = "Traditional Area Display Unit";
            // 
            // lblTraditionalUnit
            // 
            lblTraditionalUnit.Font = new Font("Segoe UI", 9F);
            lblTraditionalUnit.ForeColor = Color.FromArgb(55, 65, 85);
            lblTraditionalUnit.Location = new Point(14, 32);
            lblTraditionalUnit.Name = "lblTraditionalUnit";
            lblTraditionalUnit.Size = new Size(220, 20);
            lblTraditionalUnit.TabIndex = 0;
            lblTraditionalUnit.Text = "Unit for reports and documents:";
            // 
            // cmbTraditionalUnit
            // 
            cmbTraditionalUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTraditionalUnit.Font = new Font("Segoe UI", 9F);
            cmbTraditionalUnit.Items.AddRange(new object[] { "RAPD — Ropani-Aana-Paisa-Daam (Hilly)", "BKD  — Bigha-Kattha-Dhur (Terai)" });
            cmbTraditionalUnit.Location = new Point(14, 54);
            cmbTraditionalUnit.Name = "cmbTraditionalUnit";
            cmbTraditionalUnit.Size = new Size(320, 28);
            cmbTraditionalUnit.TabIndex = 1;
            // 
            // lblAreaNote
            // 
            lblAreaNote.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblAreaNote.ForeColor = Color.FromArgb(90, 110, 150);
            lblAreaNote.Location = new Point(14, 88);
            lblAreaNote.Name = "lblAreaNote";
            lblAreaNote.Size = new Size(516, 20);
            lblAreaNote.TabIndex = 2;
            lblAreaNote.Text = "ℹ  All internal calculations always use Square Meters.";
            // 
            // tabCanvas
            // 
            tabCanvas.BackColor = Color.FromArgb(248, 249, 252);
            tabCanvas.Controls.Add(grpCanvas);
            tabCanvas.Location = new Point(4, 33);
            tabCanvas.Name = "tabCanvas";
            tabCanvas.Padding = new Padding(10);
            tabCanvas.Size = new Size(554, 462);
            tabCanvas.TabIndex = 2;
            tabCanvas.Text = "🖥 Canvas";
            // 
            // grpCanvas
            // 
            grpCanvas.Controls.Add(lblBgColor);
            grpCanvas.Controls.Add(pnlBgColor);
            grpCanvas.Controls.Add(btnPickColor);
            grpCanvas.Controls.Add(lblBgColorHex);
            grpCanvas.Controls.Add(chkGridVisible);
            grpCanvas.Controls.Add(chkSnapEnabled);
            grpCanvas.Controls.Add(lblSnapTolerance);
            grpCanvas.Controls.Add(nudSnapTolerance);
            grpCanvas.Controls.Add(lblSnapUnit);
            grpCanvas.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpCanvas.ForeColor = Color.FromArgb(35, 45, 65);
            grpCanvas.Location = new Point(10, 10);
            grpCanvas.Name = "grpCanvas";
            grpCanvas.Size = new Size(545, 220);
            grpCanvas.TabIndex = 0;
            grpCanvas.TabStop = false;
            grpCanvas.Text = "Canvas Display Options";
            // 
            // lblBgColor
            // 
            lblBgColor.Font = new Font("Segoe UI", 9F);
            lblBgColor.ForeColor = Color.FromArgb(55, 65, 85);
            lblBgColor.Location = new Point(14, 32);
            lblBgColor.Name = "lblBgColor";
            lblBgColor.Size = new Size(130, 20);
            lblBgColor.TabIndex = 0;
            lblBgColor.Text = "Background Color:";
            // 
            // pnlBgColor
            // 
            pnlBgColor.BackColor = Color.FromArgb(30, 41, 51);
            pnlBgColor.BorderStyle = BorderStyle.FixedSingle;
            pnlBgColor.Cursor = Cursors.Hand;
            pnlBgColor.Location = new Point(14, 56);
            pnlBgColor.Name = "pnlBgColor";
            pnlBgColor.Size = new Size(50, 26);
            pnlBgColor.TabIndex = 1;
            pnlBgColor.Click += btnPickColor_Click;
            // 
            // btnPickColor
            // 
            btnPickColor.Cursor = Cursors.Hand;
            btnPickColor.FlatAppearance.BorderColor = Color.FromArgb(180, 185, 200);
            btnPickColor.FlatStyle = FlatStyle.Flat;
            btnPickColor.Font = new Font("Segoe UI", 9F);
            btnPickColor.Location = new Point(70, 56);
            btnPickColor.Name = "btnPickColor";
            btnPickColor.Size = new Size(80, 26);
            btnPickColor.TabIndex = 2;
            btnPickColor.Text = "Choose...";
            btnPickColor.Click += btnPickColor_Click;
            // 
            // lblBgColorHex
            // 
            lblBgColorHex.AutoSize = true;
            lblBgColorHex.Font = new Font("Consolas", 9F);
            lblBgColorHex.ForeColor = Color.FromArgb(100, 110, 130);
            lblBgColorHex.Location = new Point(158, 60);
            lblBgColorHex.Name = "lblBgColorHex";
            lblBgColorHex.Size = new Size(0, 18);
            lblBgColorHex.TabIndex = 3;
            // 
            // chkGridVisible
            // 
            chkGridVisible.Font = new Font("Segoe UI", 9F);
            chkGridVisible.ForeColor = Color.FromArgb(50, 60, 80);
            chkGridVisible.Location = new Point(14, 96);
            chkGridVisible.Name = "chkGridVisible";
            chkGridVisible.Size = new Size(160, 22);
            chkGridVisible.TabIndex = 4;
            chkGridVisible.Text = "Show Grid Lines";
            // 
            // chkSnapEnabled
            // 
            chkSnapEnabled.Font = new Font("Segoe UI", 9F);
            chkSnapEnabled.ForeColor = Color.FromArgb(50, 60, 80);
            chkSnapEnabled.Location = new Point(14, 124);
            chkSnapEnabled.Name = "chkSnapEnabled";
            chkSnapEnabled.Size = new Size(160, 22);
            chkSnapEnabled.TabIndex = 5;
            chkSnapEnabled.Text = "Enable Snap";
            // 
            // lblSnapTolerance
            // 
            lblSnapTolerance.Font = new Font("Segoe UI", 9F);
            lblSnapTolerance.ForeColor = Color.FromArgb(55, 65, 85);
            lblSnapTolerance.Location = new Point(14, 160);
            lblSnapTolerance.Name = "lblSnapTolerance";
            lblSnapTolerance.Size = new Size(110, 20);
            lblSnapTolerance.TabIndex = 6;
            lblSnapTolerance.Text = "Snap Tolerance:";
            // 
            // nudSnapTolerance
            // 
            nudSnapTolerance.DecimalPlaces = 1;
            nudSnapTolerance.Font = new Font("Segoe UI", 9F);
            nudSnapTolerance.Location = new Point(130, 158);
            nudSnapTolerance.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudSnapTolerance.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSnapTolerance.Name = "nudSnapTolerance";
            nudSnapTolerance.Size = new Size(70, 27);
            nudSnapTolerance.TabIndex = 7;
            nudSnapTolerance.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblSnapUnit
            // 
            lblSnapUnit.AutoSize = true;
            lblSnapUnit.Font = new Font("Segoe UI", 9F);
            lblSnapUnit.ForeColor = Color.FromArgb(100, 110, 130);
            lblSnapUnit.Location = new Point(206, 160);
            lblSnapUnit.Name = "lblSnapUnit";
            lblSnapUnit.Size = new Size(25, 20);
            lblSnapUnit.TabIndex = 8;
            lblSnapUnit.Text = "px";
            // 
            // tabParcel
            // 
            tabParcel.BackColor = Color.FromArgb(248, 249, 252);
            tabParcel.Controls.Add(grpParcelNum);
            tabParcel.Controls.Add(grpReplotRules);
            tabParcel.Location = new Point(4, 33);
            tabParcel.Name = "tabParcel";
            tabParcel.Padding = new Padding(10);
            tabParcel.Size = new Size(554, 462);
            tabParcel.TabIndex = 3;
            tabParcel.Text = "🔢 Parcels";
            // 
            // grpParcelNum
            // 
            grpParcelNum.Controls.Add(lblParcelFormat);
            grpParcelNum.Controls.Add(cmbParcelFormat);
            grpParcelNum.Controls.Add(lblParcelPrefix);
            grpParcelNum.Controls.Add(txtParcelPrefix);
            grpParcelNum.Controls.Add(lblPadding);
            grpParcelNum.Controls.Add(nudPadding);
            grpParcelNum.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpParcelNum.ForeColor = Color.FromArgb(35, 45, 65);
            grpParcelNum.Location = new Point(10, 10);
            grpParcelNum.Name = "grpParcelNum";
            grpParcelNum.Size = new Size(545, 160);
            grpParcelNum.TabIndex = 0;
            grpParcelNum.TabStop = false;
            grpParcelNum.Text = "Parcel Numbering";
            // 
            // lblParcelFormat
            // 
            lblParcelFormat.Font = new Font("Segoe UI", 9F);
            lblParcelFormat.Location = new Point(14, 32);
            lblParcelFormat.Name = "lblParcelFormat";
            lblParcelFormat.Size = new Size(60, 20);
            lblParcelFormat.TabIndex = 0;
            lblParcelFormat.Text = "Format:";
            // 
            // cmbParcelFormat
            // 
            cmbParcelFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbParcelFormat.Font = new Font("Segoe UI", 9F);
            cmbParcelFormat.Items.AddRange(new object[] { "Sequential", "BlockBased", "Custom" });
            cmbParcelFormat.Location = new Point(80, 30);
            cmbParcelFormat.Name = "cmbParcelFormat";
            cmbParcelFormat.Size = new Size(160, 28);
            cmbParcelFormat.TabIndex = 1;
            // 
            // lblParcelPrefix
            // 
            lblParcelPrefix.Font = new Font("Segoe UI", 9F);
            lblParcelPrefix.Location = new Point(14, 70);
            lblParcelPrefix.Name = "lblParcelPrefix";
            lblParcelPrefix.Size = new Size(60, 20);
            lblParcelPrefix.TabIndex = 2;
            lblParcelPrefix.Text = "Prefix:";
            // 
            // txtParcelPrefix
            // 
            txtParcelPrefix.Font = new Font("Segoe UI", 9F);
            txtParcelPrefix.Location = new Point(80, 68);
            txtParcelPrefix.Name = "txtParcelPrefix";
            txtParcelPrefix.PlaceholderText = "e.g. RP-";
            txtParcelPrefix.Size = new Size(100, 27);
            txtParcelPrefix.TabIndex = 3;
            // 
            // lblPadding
            // 
            lblPadding.Font = new Font("Segoe UI", 9F);
            lblPadding.Location = new Point(14, 110);
            lblPadding.Name = "lblPadding";
            lblPadding.Size = new Size(100, 20);
            lblPadding.TabIndex = 4;
            lblPadding.Text = "Digit Padding:";
            // 
            // nudPadding
            // 
            nudPadding.Font = new Font("Segoe UI", 9F);
            nudPadding.Location = new Point(120, 108);
            nudPadding.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            nudPadding.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudPadding.Name = "nudPadding";
            nudPadding.Size = new Size(60, 27);
            nudPadding.TabIndex = 5;
            nudPadding.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // grpReplotRules
            // 
            grpReplotRules.Controls.Add(lblMinPlot);
            grpReplotRules.Controls.Add(nudMinPlot);
            grpReplotRules.Controls.Add(lblSqm);
            grpReplotRules.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpReplotRules.ForeColor = Color.FromArgb(35, 45, 65);
            grpReplotRules.Location = new Point(10, 180);
            grpReplotRules.Name = "grpReplotRules";
            grpReplotRules.Size = new Size(545, 80);
            grpReplotRules.TabIndex = 1;
            grpReplotRules.TabStop = false;
            grpReplotRules.Text = "Replotting Rules";
            // 
            // lblMinPlot
            // 
            lblMinPlot.Font = new Font("Segoe UI", 9F);
            lblMinPlot.Location = new Point(14, 32);
            lblMinPlot.Name = "lblMinPlot";
            lblMinPlot.Size = new Size(130, 20);
            lblMinPlot.TabIndex = 0;
            lblMinPlot.Text = "Minimum Plot Area:";
            // 
            // nudMinPlot
            // 
            nudMinPlot.DecimalPlaces = 2;
            nudMinPlot.Font = new Font("Segoe UI", 9F);
            nudMinPlot.Location = new Point(150, 30);
            nudMinPlot.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudMinPlot.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudMinPlot.Name = "nudMinPlot";
            nudMinPlot.Size = new Size(90, 27);
            nudMinPlot.TabIndex = 1;
            nudMinPlot.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblSqm
            // 
            lblSqm.AutoSize = true;
            lblSqm.Font = new Font("Segoe UI", 9F);
            lblSqm.ForeColor = Color.FromArgb(100, 110, 130);
            lblSqm.Location = new Point(246, 32);
            lblSqm.Name = "lblSqm";
            lblSqm.Size = new Size(42, 20);
            lblSqm.TabIndex = 2;
            lblSqm.Text = "Sq.m";
            // 
            // tabDocument
            // 
            tabDocument.BackColor = Color.FromArgb(248, 249, 252);
            tabDocument.Controls.Add(grpDocument);
            tabDocument.Location = new Point(4, 33);
            tabDocument.Name = "tabDocument";
            tabDocument.Padding = new Padding(10);
            tabDocument.Size = new Size(554, 462);
            tabDocument.TabIndex = 4;
            tabDocument.Text = "📄 Documents";
            // 
            // grpDocument
            // 
            grpDocument.Controls.Add(lblLanguage);
            grpDocument.Controls.Add(cmbLanguage);
            grpDocument.Controls.Add(lblDateFormat);
            grpDocument.Controls.Add(cmbDateFormat);
            grpDocument.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpDocument.ForeColor = Color.FromArgb(35, 45, 65);
            grpDocument.Location = new Point(10, 10);
            grpDocument.Name = "grpDocument";
            grpDocument.Size = new Size(545, 150);
            grpDocument.TabIndex = 0;
            grpDocument.TabStop = false;
            grpDocument.Text = "Document Output";
            // 
            // lblLanguage
            // 
            lblLanguage.Font = new Font("Segoe UI", 9F);
            lblLanguage.Location = new Point(14, 32);
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Size = new Size(80, 20);
            lblLanguage.TabIndex = 0;
            lblLanguage.Text = "Language:";
            // 
            // cmbLanguage
            // 
            cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguage.Font = new Font("Segoe UI", 9F);
            cmbLanguage.Items.AddRange(new object[] { "English", "Nepali", "Both" });
            cmbLanguage.Location = new Point(100, 30);
            cmbLanguage.Name = "cmbLanguage";
            cmbLanguage.Size = new Size(180, 28);
            cmbLanguage.TabIndex = 1;
            // 
            // lblDateFormat
            // 
            lblDateFormat.Font = new Font("Segoe UI", 9F);
            lblDateFormat.Location = new Point(14, 78);
            lblDateFormat.Name = "lblDateFormat";
            lblDateFormat.Size = new Size(90, 20);
            lblDateFormat.TabIndex = 2;
            lblDateFormat.Text = "Date Format:";
            // 
            // cmbDateFormat
            // 
            cmbDateFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDateFormat.Font = new Font("Segoe UI", 9F);
            cmbDateFormat.Items.AddRange(new object[] { "AD", "BS", "Both" });
            cmbDateFormat.Location = new Point(110, 76);
            cmbDateFormat.Name = "cmbDateFormat";
            cmbDateFormat.Size = new Size(180, 28);
            cmbDateFormat.TabIndex = 3;
            // 
            // tabPrint
            // 
            tabPrint.BackColor = Color.FromArgb(248, 249, 252);
            tabPrint.Controls.Add(grpPrint);
            tabPrint.Location = new Point(4, 33);
            tabPrint.Name = "tabPrint";
            tabPrint.Padding = new Padding(10);
            tabPrint.Size = new Size(554, 462);
            tabPrint.TabIndex = 5;
            tabPrint.Text = "🖨 Print";
            // 
            // grpPrint
            // 
            grpPrint.Controls.Add(lblPaperSize);
            grpPrint.Controls.Add(cmbPaperSize);
            grpPrint.Controls.Add(lblScale);
            grpPrint.Controls.Add(lblScalePrefix);
            grpPrint.Controls.Add(nudPrintScale);
            grpPrint.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpPrint.ForeColor = Color.FromArgb(35, 45, 65);
            grpPrint.Location = new Point(10, 10);
            grpPrint.Name = "grpPrint";
            grpPrint.Size = new Size(545, 150);
            grpPrint.TabIndex = 0;
            grpPrint.TabStop = false;
            grpPrint.Text = "Print & Export";
            // 
            // lblPaperSize
            // 
            lblPaperSize.Font = new Font("Segoe UI", 9F);
            lblPaperSize.Location = new Point(14, 32);
            lblPaperSize.Name = "lblPaperSize";
            lblPaperSize.Size = new Size(80, 20);
            lblPaperSize.TabIndex = 0;
            lblPaperSize.Text = "Paper Size:";
            // 
            // cmbPaperSize
            // 
            cmbPaperSize.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaperSize.Font = new Font("Segoe UI", 9F);
            cmbPaperSize.Items.AddRange(new object[] { "A4", "A3", "A2", "A1" });
            cmbPaperSize.Location = new Point(100, 30);
            cmbPaperSize.Name = "cmbPaperSize";
            cmbPaperSize.Size = new Size(120, 28);
            cmbPaperSize.TabIndex = 1;
            // 
            // lblScale
            // 
            lblScale.Font = new Font("Segoe UI", 9F);
            lblScale.Location = new Point(14, 78);
            lblScale.Name = "lblScale";
            lblScale.Size = new Size(80, 20);
            lblScale.TabIndex = 2;
            lblScale.Text = "Print Scale:";
            // 
            // lblScalePrefix
            // 
            lblScalePrefix.AutoSize = true;
            lblScalePrefix.Font = new Font("Segoe UI", 9F);
            lblScalePrefix.Location = new Point(100, 78);
            lblScalePrefix.Name = "lblScalePrefix";
            lblScalePrefix.Size = new Size(24, 20);
            lblScalePrefix.TabIndex = 3;
            lblScalePrefix.Text = "1 :";
            // 
            // nudPrintScale
            // 
            nudPrintScale.Font = new Font("Segoe UI", 9F);
            nudPrintScale.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            nudPrintScale.Location = new Point(130, 76);
            nudPrintScale.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudPrintScale.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            nudPrintScale.Name = "nudPrintScale";
            nudPrintScale.Size = new Size(100, 27);
            nudPrintScale.TabIndex = 4;
            nudPrintScale.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(245, 246, 250);
            pnlContent.Controls.Add(tabSettings);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 68);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10, 8, 10, 8);
            pnlContent.Size = new Size(582, 515);
            pnlContent.TabIndex = 0;
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.FromArgb(232, 234, 240);
            pnlFooter.Controls.Add(btnRestoreDefaults);
            pnlFooter.Controls.Add(btnOK);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(lblStatus);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 583);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(582, 50);
            pnlFooter.TabIndex = 1;
            // 
            // btnOK
            // 
            btnOK.BackColor = Color.FromArgb(28, 36, 54);
            btnOK.Cursor = Cursors.Hand;
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(356, 9);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(110, 32);
            btnOK.TabIndex = 0;
            btnOK.Text = "Save Settings";
            btnOK.UseVisualStyleBackColor = false;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.White;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 215);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.FromArgb(60, 65, 80);
            btnCancel.Location = new Point(472, 9);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 32);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblStatus.ForeColor = Color.FromArgb(100, 110, 130);
            lblStatus.Location = new Point(14, 16);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(48, 20);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Ready";
            // 
            // btnRestoreDefaults
            // 
            btnRestoreDefaults.BackColor = Color.White;
            btnRestoreDefaults.Cursor = Cursors.Hand;
            btnRestoreDefaults.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 215);
            btnRestoreDefaults.FlatStyle = FlatStyle.Flat;
            btnRestoreDefaults.ForeColor = Color.FromArgb(60, 65, 80);
            btnRestoreDefaults.Location = new Point(208, 9);
            btnRestoreDefaults.Name = "btnRestoreDefaults";
            btnRestoreDefaults.Size = new Size(133, 32);
            btnRestoreDefaults.TabIndex = 4;
            btnRestoreDefaults.Text = "Restore Defaults";
            btnRestoreDefaults.UseVisualStyleBackColor = false;
            // 
            // frmProjectSettings
            // 
            BackColor = Color.FromArgb(245, 246, 250);
            ClientSize = new Size(582, 633);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MaximumSize = new Size(600, 680);
            MinimizeBox = false;
            MinimumSize = new Size(600, 680);
            Name = "frmProjectSettings";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Project Settings";
            Load += frmProjectSettings_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            tabSettings.ResumeLayout(false);
            tabCoordinates.ResumeLayout(false);
            grpCRS.ResumeLayout(false);
            grpDatumTransformation.ResumeLayout(false);
            tabArea.ResumeLayout(false);
            grpAreaUnit.ResumeLayout(false);
            tabCanvas.ResumeLayout(false);
            grpCanvas.ResumeLayout(false);
            grpCanvas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudSnapTolerance).EndInit();
            tabParcel.ResumeLayout(false);
            grpParcelNum.ResumeLayout(false);
            grpParcelNum.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudPadding).EndInit();
            grpReplotRules.ResumeLayout(false);
            grpReplotRules.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPlot).EndInit();
            tabDocument.ResumeLayout(false);
            grpDocument.ResumeLayout(false);
            tabPrint.ResumeLayout(false);
            grpPrint.ResumeLayout(false);
            grpPrint.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudPrintScale).EndInit();
            pnlContent.ResumeLayout(false);
            pnlFooter.ResumeLayout(false);
            pnlFooter.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // Controls
        private Panel           pnlHeader;
        private Label           lblFormTitle;
        private Label           lblFormSubtitle;
        private TabControl      tabSettings;
        private TabPage         tabCoordinates;
        private TabPage         tabArea;
        private TabPage         tabCanvas;
        private TabPage         tabParcel;
        private TabPage         tabDocument;
        private TabPage         tabPrint;
        private Panel           pnlContent;
        private Panel           pnlFooter;
        private Button          btnOK;
        private Button          btnCancel;
        private Label           lblStatus;
        private GroupBox        grpCRS;
        private Label           lblCRS;
        private ComboBox        cmbCRS;
        private Button          btnManageCRS;
        private Label           lblCrsInfo;
        private GroupBox        grpDatumTransformation;
        private Label           lblDatum;
        private ComboBox        cmbDatumTransformation;
        private Button          btnManageDatum;
        private Label           lblDatumNote;
        private GroupBox        grpAreaUnit;
        private Label           lblTraditionalUnit;
        private ComboBox        cmbTraditionalUnit;
        private Label           lblAreaNote;
        private GroupBox        grpCanvas;
        private Label           lblBgColor;
        private Panel           pnlBgColor;
        private Button          btnPickColor;
        private Label           lblBgColorHex;
        private CheckBox        chkGridVisible;
        private CheckBox        chkSnapEnabled;
        private Label           lblSnapTolerance;
        private NumericUpDown   nudSnapTolerance;
        private Label           lblSnapUnit;
        private GroupBox        grpParcelNum;
        private Label           lblParcelFormat;
        private ComboBox        cmbParcelFormat;
        private Label           lblParcelPrefix;
        private TextBox         txtParcelPrefix;
        private Label           lblPadding;
        private NumericUpDown   nudPadding;
        private GroupBox        grpReplotRules;
        private Label           lblMinPlot;
        private NumericUpDown   nudMinPlot;
        private Label           lblSqm;
        private GroupBox        grpDocument;
        private Label           lblLanguage;
        private ComboBox        cmbLanguage;
        private Label           lblDateFormat;
        private ComboBox        cmbDateFormat;
        private GroupBox        grpPrint;
        private Label           lblPaperSize;
        private ComboBox        cmbPaperSize;
        private Label           lblScale;
        private Label           lblScalePrefix;
        private NumericUpDown   nudPrintScale;
        private Button btnRestoreDefaults;
    }
}
