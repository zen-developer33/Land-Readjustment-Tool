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
            tabMapCanvas = new TabPage();
            grpOther = new GroupBox();
            chkOriginAxisMarkerVisible = new CheckBox();
            nudSnapTolerance = new NumericUpDown();
            lblSnapTolerance = new Label();
            chkGridVisible = new CheckBox();
            chkSnapEnabled = new CheckBox();
            lblSnapUnit = new Label();
            grpCanvasTheme = new GroupBox();
            lblCanvasTheme = new Label();
            cmbCanvasTheme = new ComboBox();
            lblBgColor = new Label();
            pnlBgColor = new Panel();
            btnPickColor = new Button();
            lblBgColorHex = new Label();
            lblGridColor = new Label();
            pnlGridColor = new Panel();
            btnPickGridColor = new Button();
            lblGridColorHex = new Label();
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
            pnlFooter = new Panel();
            btnRestoreDefaults = new Button();
            btnApply = new Button();
            btnCancel = new Button();
            tabSettings.SuspendLayout();
            tabCoordinates.SuspendLayout();
            grpCRS.SuspendLayout();
            grpDatumTransformation.SuspendLayout();
            tabArea.SuspendLayout();
            grpAreaUnit.SuspendLayout();
            tabMapCanvas.SuspendLayout();
            grpOther.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSnapTolerance).BeginInit();
            grpCanvasTheme.SuspendLayout();
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
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // tabSettings
            // 
            tabSettings.Controls.Add(tabCoordinates);
            tabSettings.Controls.Add(tabArea);
            tabSettings.Controls.Add(tabMapCanvas);
            tabSettings.Controls.Add(tabParcel);
            tabSettings.Controls.Add(tabDocument);
            tabSettings.Controls.Add(tabPrint);
            tabSettings.Dock = DockStyle.Fill;
            tabSettings.Font = new Font("Segoe UI", 9F);
            tabSettings.HotTrack = true;
            tabSettings.Location = new Point(0, 0);
            tabSettings.Margin = new Padding(3, 2, 3, 2);
            tabSettings.Name = "tabSettings";
            tabSettings.Padding = new Point(8, 4);
            tabSettings.SelectedIndex = 0;
            tabSettings.Size = new Size(493, 324);
            tabSettings.TabIndex = 0;
            // 
            // tabCoordinates
            // 
            tabCoordinates.Controls.Add(grpCRS);
            tabCoordinates.Controls.Add(grpDatumTransformation);
            tabCoordinates.Location = new Point(4, 26);
            tabCoordinates.Margin = new Padding(3, 2, 3, 2);
            tabCoordinates.Name = "tabCoordinates";
            tabCoordinates.Padding = new Padding(7, 6, 7, 6);
            tabCoordinates.Size = new Size(485, 294);
            tabCoordinates.TabIndex = 0;
            tabCoordinates.Text = "Coordinate System";
            tabCoordinates.UseVisualStyleBackColor = true;
            // 
            // grpCRS
            // 
            grpCRS.Controls.Add(lblCRS);
            grpCRS.Controls.Add(cmbCRS);
            grpCRS.Controls.Add(btnManageCRS);
            grpCRS.Controls.Add(lblCrsInfo);
            grpCRS.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpCRS.Location = new Point(7, 6);
            grpCRS.Margin = new Padding(3, 2, 3, 2);
            grpCRS.Name = "grpCRS";
            grpCRS.Padding = new Padding(3, 2, 3, 2);
            grpCRS.Size = new Size(469, 83);
            grpCRS.TabIndex = 0;
            grpCRS.TabStop = false;
            grpCRS.Text = "Coordinate Reference System";
            // 
            // lblCRS
            // 
            lblCRS.Font = new Font("Segoe UI", 9F);
            lblCRS.Location = new Point(10, 20);
            lblCRS.Name = "lblCRS";
            lblCRS.Size = new Size(70, 15);
            lblCRS.TabIndex = 0;
            lblCRS.Text = "Select CRS:";
            // 
            // cmbCRS
            // 
            cmbCRS.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCRS.Font = new Font("Segoe UI", 9F);
            cmbCRS.Location = new Point(10, 36);
            cmbCRS.Margin = new Padding(3, 2, 3, 2);
            cmbCRS.Name = "cmbCRS";
            cmbCRS.Size = new Size(316, 23);
            cmbCRS.TabIndex = 0;
            cmbCRS.SelectedIndexChanged += cmbCRS_SelectedIndexChanged;
            // 
            // btnManageCRS
            // 
            btnManageCRS.Location = new Point(331, 34);
            btnManageCRS.Margin = new Padding(3, 2, 3, 2);
            btnManageCRS.Name = "btnManageCRS";
            btnManageCRS.Size = new Size(96, 25);
            btnManageCRS.TabIndex = 1;
            btnManageCRS.Text = "Manage...";
            btnManageCRS.Click += btnManageCRS_Click;
            // 
            // lblCrsInfo
            // 
            lblCrsInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblCrsInfo.ForeColor = SystemColors.GrayText;
            lblCrsInfo.Location = new Point(10, 58);
            lblCrsInfo.Name = "lblCrsInfo";
            lblCrsInfo.Size = new Size(416, 22);
            lblCrsInfo.TabIndex = 2;
            // 
            // grpDatumTransformation
            // 
            grpDatumTransformation.Controls.Add(lblDatum);
            grpDatumTransformation.Controls.Add(cmbDatumTransformation);
            grpDatumTransformation.Controls.Add(btnManageDatum);
            grpDatumTransformation.Controls.Add(lblDatumNote);
            grpDatumTransformation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpDatumTransformation.Location = new Point(7, 94);
            grpDatumTransformation.Margin = new Padding(3, 2, 3, 2);
            grpDatumTransformation.Name = "grpDatumTransformation";
            grpDatumTransformation.Padding = new Padding(3, 2, 3, 2);
            grpDatumTransformation.Size = new Size(469, 89);
            grpDatumTransformation.TabIndex = 1;
            grpDatumTransformation.TabStop = false;
            grpDatumTransformation.Text = "Datum Transformation (MUTM zones only)";
            // 
            // lblDatum
            // 
            lblDatum.Font = new Font("Segoe UI", 9F);
            lblDatum.Location = new Point(10, 20);
            lblDatum.Name = "lblDatum";
            lblDatum.Size = new Size(96, 15);
            lblDatum.TabIndex = 0;
            lblDatum.Text = "Transformation:";
            // 
            // cmbDatumTransformation
            // 
            cmbDatumTransformation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDatumTransformation.Font = new Font("Segoe UI", 9F);
            cmbDatumTransformation.Location = new Point(10, 36);
            cmbDatumTransformation.Margin = new Padding(3, 2, 3, 2);
            cmbDatumTransformation.Name = "cmbDatumTransformation";
            cmbDatumTransformation.Size = new Size(316, 23);
            cmbDatumTransformation.TabIndex = 2;
            // 
            // btnManageDatum
            // 
            btnManageDatum.Location = new Point(331, 34);
            btnManageDatum.Margin = new Padding(3, 2, 3, 2);
            btnManageDatum.Name = "btnManageDatum";
            btnManageDatum.Size = new Size(96, 25);
            btnManageDatum.TabIndex = 3;
            btnManageDatum.Text = "Manage...";
            btnManageDatum.Click += btnManageDatum_Click;
            // 
            // lblDatumNote
            // 
            lblDatumNote.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblDatumNote.ForeColor = SystemColors.GrayText;
            lblDatumNote.Location = new Point(10, 62);
            lblDatumNote.Name = "lblDatumNote";
            lblDatumNote.Size = new Size(416, 21);
            lblDatumNote.TabIndex = 4;
            lblDatumNote.Text = "Required for converting MUTM coordinates to/from WGS84.";
            // 
            // tabArea
            // 
            tabArea.Controls.Add(grpAreaUnit);
            tabArea.Location = new Point(4, 26);
            tabArea.Margin = new Padding(3, 2, 3, 2);
            tabArea.Name = "tabArea";
            tabArea.Padding = new Padding(7, 6, 7, 6);
            tabArea.Size = new Size(485, 304);
            tabArea.TabIndex = 1;
            tabArea.Text = "Area Units";
            tabArea.UseVisualStyleBackColor = true;
            // 
            // grpAreaUnit
            // 
            grpAreaUnit.Controls.Add(lblTraditionalUnit);
            grpAreaUnit.Controls.Add(cmbTraditionalUnit);
            grpAreaUnit.Controls.Add(lblAreaNote);
            grpAreaUnit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpAreaUnit.Location = new Point(7, 6);
            grpAreaUnit.Margin = new Padding(3, 2, 3, 2);
            grpAreaUnit.Name = "grpAreaUnit";
            grpAreaUnit.Padding = new Padding(3, 2, 3, 2);
            grpAreaUnit.Size = new Size(469, 82);
            grpAreaUnit.TabIndex = 0;
            grpAreaUnit.TabStop = false;
            grpAreaUnit.Text = "Traditional Area Display Unit";
            // 
            // lblTraditionalUnit
            // 
            lblTraditionalUnit.Font = new Font("Segoe UI", 9F);
            lblTraditionalUnit.Location = new Point(10, 20);
            lblTraditionalUnit.Name = "lblTraditionalUnit";
            lblTraditionalUnit.Size = new Size(245, 15);
            lblTraditionalUnit.TabIndex = 0;
            lblTraditionalUnit.Text = "Display unit for reports and documents:";
            // 
            // cmbTraditionalUnit
            // 
            cmbTraditionalUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTraditionalUnit.Font = new Font("Segoe UI", 9F);
            cmbTraditionalUnit.Items.AddRange(new object[] { "RAPD — Ropani-Aana-Paisa-Daam (Hilly)", "BKD  — Bigha-Kattha-Dhur (Terai)" });
            cmbTraditionalUnit.Location = new Point(10, 34);
            cmbTraditionalUnit.Margin = new Padding(3, 2, 3, 2);
            cmbTraditionalUnit.Name = "cmbTraditionalUnit";
            cmbTraditionalUnit.Size = new Size(316, 23);
            cmbTraditionalUnit.TabIndex = 0;
            // 
            // lblAreaNote
            // 
            lblAreaNote.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblAreaNote.ForeColor = SystemColors.GrayText;
            lblAreaNote.Location = new Point(10, 58);
            lblAreaNote.Name = "lblAreaNote";
            lblAreaNote.Size = new Size(416, 22);
            lblAreaNote.TabIndex = 1;
            lblAreaNote.Text = "All internal calculations always use Square Meters.";
            // 
            // tabMapCanvas
            // 
            tabMapCanvas.Controls.Add(grpOther);
            tabMapCanvas.Controls.Add(grpCanvasTheme);
            tabMapCanvas.Location = new Point(4, 26);
            tabMapCanvas.Margin = new Padding(3, 2, 3, 2);
            tabMapCanvas.Name = "tabMapCanvas";
            tabMapCanvas.Padding = new Padding(7, 6, 7, 6);
            tabMapCanvas.Size = new Size(485, 304);
            tabMapCanvas.TabIndex = 2;
            tabMapCanvas.Text = "Map Canvas";
            tabMapCanvas.UseVisualStyleBackColor = true;
            // 
            // grpOther
            // 
            grpOther.Controls.Add(chkOriginAxisMarkerVisible);
            grpOther.Controls.Add(nudSnapTolerance);
            grpOther.Controls.Add(lblSnapTolerance);
            grpOther.Controls.Add(chkGridVisible);
            grpOther.Controls.Add(chkSnapEnabled);
            grpOther.Controls.Add(lblSnapUnit);
            grpOther.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpOther.Location = new Point(7, 131);
            grpOther.Margin = new Padding(3, 2, 3, 2);
            grpOther.Name = "grpOther";
            grpOther.Padding = new Padding(3, 2, 3, 2);
            grpOther.Size = new Size(469, 92);
            grpOther.TabIndex = 9;
            grpOther.TabStop = false;
            grpOther.Text = "Other";
            // 
            // chkOriginAxisMarkerVisible
            // 
            chkOriginAxisMarkerVisible.Font = new Font("Segoe UI", 9F);
            chkOriginAxisMarkerVisible.Location = new Point(9, 41);
            chkOriginAxisMarkerVisible.Margin = new Padding(3, 2, 3, 2);
            chkOriginAxisMarkerVisible.Name = "chkOriginAxisMarkerVisible";
            chkOriginAxisMarkerVisible.Size = new Size(131, 20);
            chkOriginAxisMarkerVisible.TabIndex = 9;
            chkOriginAxisMarkerVisible.Text = "Show Origin/Axis Marker";
            // 
            // nudSnapTolerance
            // 
            nudSnapTolerance.DecimalPlaces = 1;
            nudSnapTolerance.Location = new Point(242, 62);
            nudSnapTolerance.Margin = new Padding(3, 2, 3, 2);
            nudSnapTolerance.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudSnapTolerance.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSnapTolerance.Name = "nudSnapTolerance";
            nudSnapTolerance.Size = new Size(61, 23);
            nudSnapTolerance.TabIndex = 5;
            nudSnapTolerance.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblSnapTolerance
            // 
            lblSnapTolerance.Font = new Font("Segoe UI", 9F);
            lblSnapTolerance.Location = new Point(125, 64);
            lblSnapTolerance.Name = "lblSnapTolerance";
            lblSnapTolerance.Size = new Size(112, 16);
            lblSnapTolerance.TabIndex = 7;
            lblSnapTolerance.Text = "Snap Tolerance:";
            // 
            // chkGridVisible
            // 
            chkGridVisible.Font = new Font("Segoe UI", 9F);
            chkGridVisible.Location = new Point(9, 20);
            chkGridVisible.Margin = new Padding(3, 2, 3, 2);
            chkGridVisible.Name = "chkGridVisible";
            chkGridVisible.Size = new Size(131, 20);
            chkGridVisible.TabIndex = 3;
            chkGridVisible.Text = "Show Grid Lines";
            // 
            // chkSnapEnabled
            // 
            chkSnapEnabled.Font = new Font("Segoe UI", 9F);
            chkSnapEnabled.Location = new Point(9, 63);
            chkSnapEnabled.Margin = new Padding(3, 2, 3, 2);
            chkSnapEnabled.Name = "chkSnapEnabled";
            chkSnapEnabled.Size = new Size(131, 20);
            chkSnapEnabled.TabIndex = 4;
            chkSnapEnabled.Text = "Enable Snap";
            // 
            // lblSnapUnit
            // 
            lblSnapUnit.AutoSize = true;
            lblSnapUnit.Font = new Font("Segoe UI", 9F);
            lblSnapUnit.ForeColor = SystemColors.GrayText;
            lblSnapUnit.Location = new Point(309, 64);
            lblSnapUnit.Name = "lblSnapUnit";
            lblSnapUnit.Size = new Size(37, 15);
            lblSnapUnit.TabIndex = 8;
            lblSnapUnit.Text = "pixels";
            // 
            // grpCanvasTheme
            // 
            grpCanvasTheme.Controls.Add(lblCanvasTheme);
            grpCanvasTheme.Controls.Add(cmbCanvasTheme);
            grpCanvasTheme.Controls.Add(lblBgColor);
            grpCanvasTheme.Controls.Add(pnlBgColor);
            grpCanvasTheme.Controls.Add(btnPickColor);
            grpCanvasTheme.Controls.Add(lblBgColorHex);
            grpCanvasTheme.Controls.Add(lblGridColor);
            grpCanvasTheme.Controls.Add(pnlGridColor);
            grpCanvasTheme.Controls.Add(btnPickGridColor);
            grpCanvasTheme.Controls.Add(lblGridColorHex);
            grpCanvasTheme.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpCanvasTheme.Location = new Point(7, 6);
            grpCanvasTheme.Margin = new Padding(3, 2, 3, 2);
            grpCanvasTheme.Name = "grpCanvasTheme";
            grpCanvasTheme.Padding = new Padding(3, 2, 3, 2);
            grpCanvasTheme.Size = new Size(469, 121);
            grpCanvasTheme.TabIndex = 0;
            grpCanvasTheme.TabStop = false;
            grpCanvasTheme.Text = "Canvas Theme";
            grpCanvasTheme.Enter += grpCanvasTheme_Enter;
            // 
            // lblCanvasTheme
            // 
            lblCanvasTheme.Font = new Font("Segoe UI", 9F);
            lblCanvasTheme.Location = new Point(10, 20);
            lblCanvasTheme.Name = "lblCanvasTheme";
            lblCanvasTheme.Size = new Size(96, 15);
            lblCanvasTheme.TabIndex = 0;
            lblCanvasTheme.Text = "Theme Preset:";
            // 
            // cmbCanvasTheme
            // 
            cmbCanvasTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCanvasTheme.Font = new Font("Segoe UI", 9F);
            cmbCanvasTheme.Items.AddRange(new object[] { "Light", "Dark", "Custom" });
            cmbCanvasTheme.Location = new Point(110, 18);
            cmbCanvasTheme.Margin = new Padding(3, 2, 3, 2);
            cmbCanvasTheme.Name = "cmbCanvasTheme";
            cmbCanvasTheme.Size = new Size(127, 23);
            cmbCanvasTheme.TabIndex = 0;
            cmbCanvasTheme.SelectedIndexChanged += cmbCanvasTheme_SelectedIndexChanged;
            // 
            // lblBgColor
            // 
            lblBgColor.Font = new Font("Segoe UI", 9F);
            lblBgColor.Location = new Point(10, 41);
            lblBgColor.Name = "lblBgColor";
            lblBgColor.Size = new Size(114, 15);
            lblBgColor.TabIndex = 1;
            lblBgColor.Text = "Background Color:";
            // 
            // pnlBgColor
            // 
            pnlBgColor.BackColor = Color.FromArgb(30, 41, 51);
            pnlBgColor.BorderStyle = BorderStyle.FixedSingle;
            pnlBgColor.Cursor = Cursors.Hand;
            pnlBgColor.Location = new Point(10, 58);
            pnlBgColor.Margin = new Padding(3, 2, 3, 2);
            pnlBgColor.Name = "pnlBgColor";
            pnlBgColor.Size = new Size(42, 18);
            pnlBgColor.TabIndex = 2;
            // 
            // btnPickColor
            // 
            btnPickColor.Location = new Point(58, 57);
            btnPickColor.Margin = new Padding(3, 2, 3, 2);
            btnPickColor.Name = "btnPickColor";
            btnPickColor.Size = new Size(70, 20);
            btnPickColor.TabIndex = 1;
            btnPickColor.Text = "Choose...";
            btnPickColor.Click += btnPickColor_Click;
            // 
            // lblBgColorHex
            // 
            lblBgColorHex.AutoSize = true;
            lblBgColorHex.Font = new Font("Consolas", 9F);
            lblBgColorHex.ForeColor = SystemColors.GrayText;
            lblBgColorHex.Location = new Point(133, 61);
            lblBgColorHex.Name = "lblBgColorHex";
            lblBgColorHex.Size = new Size(0, 14);
            lblBgColorHex.TabIndex = 3;
            // 
            // lblGridColor
            // 
            lblGridColor.Font = new Font("Segoe UI", 9F);
            lblGridColor.Location = new Point(9, 80);
            lblGridColor.Name = "lblGridColor";
            lblGridColor.Size = new Size(114, 15);
            lblGridColor.TabIndex = 4;
            lblGridColor.Text = "Grid Color:";
            // 
            // pnlGridColor
            // 
            pnlGridColor.BackColor = Color.FromArgb(74, 85, 98);
            pnlGridColor.BorderStyle = BorderStyle.FixedSingle;
            pnlGridColor.Cursor = Cursors.Hand;
            pnlGridColor.Location = new Point(9, 96);
            pnlGridColor.Margin = new Padding(3, 2, 3, 2);
            pnlGridColor.Name = "pnlGridColor";
            pnlGridColor.Size = new Size(42, 18);
            pnlGridColor.TabIndex = 5;
            // 
            // btnPickGridColor
            // 
            btnPickGridColor.Location = new Point(56, 95);
            btnPickGridColor.Margin = new Padding(3, 2, 3, 2);
            btnPickGridColor.Name = "btnPickGridColor";
            btnPickGridColor.Size = new Size(70, 20);
            btnPickGridColor.TabIndex = 2;
            btnPickGridColor.Text = "Choose...";
            btnPickGridColor.Click += btnPickGridColor_Click;
            // 
            // lblGridColorHex
            // 
            lblGridColorHex.AutoSize = true;
            lblGridColorHex.Font = new Font("Consolas", 9F);
            lblGridColorHex.ForeColor = SystemColors.GrayText;
            lblGridColorHex.Location = new Point(131, 99);
            lblGridColorHex.Name = "lblGridColorHex";
            lblGridColorHex.Size = new Size(0, 14);
            lblGridColorHex.TabIndex = 6;
            // 
            // tabParcel
            // 
            tabParcel.Controls.Add(grpParcelNum);
            tabParcel.Controls.Add(grpReplotRules);
            tabParcel.Location = new Point(4, 26);
            tabParcel.Margin = new Padding(3, 2, 3, 2);
            tabParcel.Name = "tabParcel";
            tabParcel.Padding = new Padding(7, 6, 7, 6);
            tabParcel.Size = new Size(485, 304);
            tabParcel.TabIndex = 3;
            tabParcel.Text = "Parcels";
            tabParcel.UseVisualStyleBackColor = true;
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
            grpParcelNum.Location = new Point(7, 6);
            grpParcelNum.Margin = new Padding(3, 2, 3, 2);
            grpParcelNum.Name = "grpParcelNum";
            grpParcelNum.Padding = new Padding(3, 2, 3, 2);
            grpParcelNum.Size = new Size(439, 111);
            grpParcelNum.TabIndex = 0;
            grpParcelNum.TabStop = false;
            grpParcelNum.Text = "Parcel Numbering";
            // 
            // lblParcelFormat
            // 
            lblParcelFormat.Font = new Font("Segoe UI", 9F);
            lblParcelFormat.Location = new Point(10, 20);
            lblParcelFormat.Name = "lblParcelFormat";
            lblParcelFormat.Size = new Size(70, 15);
            lblParcelFormat.TabIndex = 0;
            lblParcelFormat.Text = "Format:";
            // 
            // cmbParcelFormat
            // 
            cmbParcelFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbParcelFormat.Font = new Font("Segoe UI", 9F);
            cmbParcelFormat.Items.AddRange(new object[] { "Sequential", "BlockBased", "Custom" });
            cmbParcelFormat.Location = new Point(88, 18);
            cmbParcelFormat.Margin = new Padding(3, 2, 3, 2);
            cmbParcelFormat.Name = "cmbParcelFormat";
            cmbParcelFormat.Size = new Size(140, 23);
            cmbParcelFormat.TabIndex = 0;
            // 
            // lblParcelPrefix
            // 
            lblParcelPrefix.Font = new Font("Segoe UI", 9F);
            lblParcelPrefix.Location = new Point(10, 46);
            lblParcelPrefix.Name = "lblParcelPrefix";
            lblParcelPrefix.Size = new Size(70, 15);
            lblParcelPrefix.TabIndex = 1;
            lblParcelPrefix.Text = "Prefix:";
            // 
            // txtParcelPrefix
            // 
            txtParcelPrefix.Font = new Font("Segoe UI", 9F);
            txtParcelPrefix.Location = new Point(88, 45);
            txtParcelPrefix.Margin = new Padding(3, 2, 3, 2);
            txtParcelPrefix.Name = "txtParcelPrefix";
            txtParcelPrefix.PlaceholderText = "e.g. RP-";
            txtParcelPrefix.Size = new Size(106, 23);
            txtParcelPrefix.TabIndex = 1;
            // 
            // lblPadding
            // 
            lblPadding.Font = new Font("Segoe UI", 9F);
            lblPadding.Location = new Point(10, 75);
            lblPadding.Name = "lblPadding";
            lblPadding.Size = new Size(88, 15);
            lblPadding.TabIndex = 2;
            lblPadding.Text = "Digit Padding:";
            // 
            // nudPadding
            // 
            nudPadding.Location = new Point(102, 74);
            nudPadding.Margin = new Padding(3, 2, 3, 2);
            nudPadding.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            nudPadding.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudPadding.Name = "nudPadding";
            nudPadding.Size = new Size(52, 23);
            nudPadding.TabIndex = 2;
            nudPadding.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // grpReplotRules
            // 
            grpReplotRules.Controls.Add(lblMinPlot);
            grpReplotRules.Controls.Add(nudMinPlot);
            grpReplotRules.Controls.Add(lblSqm);
            grpReplotRules.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpReplotRules.Location = new Point(7, 123);
            grpReplotRules.Margin = new Padding(3, 2, 3, 2);
            grpReplotRules.Name = "grpReplotRules";
            grpReplotRules.Padding = new Padding(3, 2, 3, 2);
            grpReplotRules.Size = new Size(439, 48);
            grpReplotRules.TabIndex = 1;
            grpReplotRules.TabStop = false;
            grpReplotRules.Text = "Replotting Rules";
            // 
            // lblMinPlot
            // 
            lblMinPlot.Font = new Font("Segoe UI", 9F);
            lblMinPlot.Location = new Point(10, 20);
            lblMinPlot.Name = "lblMinPlot";
            lblMinPlot.Size = new Size(114, 15);
            lblMinPlot.TabIndex = 0;
            lblMinPlot.Text = "Minimum Plot Area:";
            // 
            // nudMinPlot
            // 
            nudMinPlot.DecimalPlaces = 2;
            nudMinPlot.Location = new Point(128, 18);
            nudMinPlot.Margin = new Padding(3, 2, 3, 2);
            nudMinPlot.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudMinPlot.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudMinPlot.Name = "nudMinPlot";
            nudMinPlot.Size = new Size(79, 23);
            nudMinPlot.TabIndex = 3;
            nudMinPlot.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblSqm
            // 
            lblSqm.AutoSize = true;
            lblSqm.Font = new Font("Segoe UI", 9F);
            lblSqm.ForeColor = SystemColors.GrayText;
            lblSqm.Location = new Point(212, 20);
            lblSqm.Name = "lblSqm";
            lblSqm.Size = new Size(34, 15);
            lblSqm.TabIndex = 4;
            lblSqm.Text = "Sq.m";
            // 
            // tabDocument
            // 
            tabDocument.Controls.Add(grpDocument);
            tabDocument.Location = new Point(4, 26);
            tabDocument.Margin = new Padding(3, 2, 3, 2);
            tabDocument.Name = "tabDocument";
            tabDocument.Padding = new Padding(7, 6, 7, 6);
            tabDocument.Size = new Size(485, 304);
            tabDocument.TabIndex = 4;
            tabDocument.Text = "Documents";
            tabDocument.UseVisualStyleBackColor = true;
            // 
            // grpDocument
            // 
            grpDocument.Controls.Add(lblLanguage);
            grpDocument.Controls.Add(cmbLanguage);
            grpDocument.Controls.Add(lblDateFormat);
            grpDocument.Controls.Add(cmbDateFormat);
            grpDocument.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpDocument.Location = new Point(7, 6);
            grpDocument.Margin = new Padding(3, 2, 3, 2);
            grpDocument.Name = "grpDocument";
            grpDocument.Padding = new Padding(3, 2, 3, 2);
            grpDocument.Size = new Size(439, 98);
            grpDocument.TabIndex = 0;
            grpDocument.TabStop = false;
            grpDocument.Text = "Document Output";
            // 
            // lblLanguage
            // 
            lblLanguage.Font = new Font("Segoe UI", 9F);
            lblLanguage.Location = new Point(10, 20);
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Size = new Size(70, 15);
            lblLanguage.TabIndex = 0;
            lblLanguage.Text = "Language:";
            // 
            // cmbLanguage
            // 
            cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguage.Font = new Font("Segoe UI", 9F);
            cmbLanguage.Items.AddRange(new object[] { "English", "Nepali", "Both" });
            cmbLanguage.Location = new Point(88, 18);
            cmbLanguage.Margin = new Padding(3, 2, 3, 2);
            cmbLanguage.Name = "cmbLanguage";
            cmbLanguage.Size = new Size(158, 23);
            cmbLanguage.TabIndex = 0;
            // 
            // lblDateFormat
            // 
            lblDateFormat.Font = new Font("Segoe UI", 9F);
            lblDateFormat.Location = new Point(10, 48);
            lblDateFormat.Name = "lblDateFormat";
            lblDateFormat.Size = new Size(70, 15);
            lblDateFormat.TabIndex = 1;
            lblDateFormat.Text = "Date Format:";
            // 
            // cmbDateFormat
            // 
            cmbDateFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDateFormat.Font = new Font("Segoe UI", 9F);
            cmbDateFormat.Items.AddRange(new object[] { "AD", "BS", "Both" });
            cmbDateFormat.Location = new Point(88, 46);
            cmbDateFormat.Margin = new Padding(3, 2, 3, 2);
            cmbDateFormat.Name = "cmbDateFormat";
            cmbDateFormat.Size = new Size(158, 23);
            cmbDateFormat.TabIndex = 1;
            // 
            // tabPrint
            // 
            tabPrint.Controls.Add(grpPrint);
            tabPrint.Location = new Point(4, 26);
            tabPrint.Margin = new Padding(3, 2, 3, 2);
            tabPrint.Name = "tabPrint";
            tabPrint.Padding = new Padding(7, 6, 7, 6);
            tabPrint.Size = new Size(485, 304);
            tabPrint.TabIndex = 5;
            tabPrint.Text = "Print";
            tabPrint.UseVisualStyleBackColor = true;
            // 
            // grpPrint
            // 
            grpPrint.Controls.Add(lblPaperSize);
            grpPrint.Controls.Add(cmbPaperSize);
            grpPrint.Controls.Add(lblScale);
            grpPrint.Controls.Add(lblScalePrefix);
            grpPrint.Controls.Add(nudPrintScale);
            grpPrint.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpPrint.Location = new Point(7, 6);
            grpPrint.Margin = new Padding(3, 2, 3, 2);
            grpPrint.Name = "grpPrint";
            grpPrint.Padding = new Padding(3, 2, 3, 2);
            grpPrint.Size = new Size(439, 98);
            grpPrint.TabIndex = 0;
            grpPrint.TabStop = false;
            grpPrint.Text = "Print & Export";
            // 
            // lblPaperSize
            // 
            lblPaperSize.Font = new Font("Segoe UI", 9F);
            lblPaperSize.Location = new Point(10, 20);
            lblPaperSize.Name = "lblPaperSize";
            lblPaperSize.Size = new Size(70, 15);
            lblPaperSize.TabIndex = 0;
            lblPaperSize.Text = "Paper Size:";
            // 
            // cmbPaperSize
            // 
            cmbPaperSize.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaperSize.Font = new Font("Segoe UI", 9F);
            cmbPaperSize.Items.AddRange(new object[] { "A4", "A3", "A2", "A1" });
            cmbPaperSize.Location = new Point(88, 18);
            cmbPaperSize.Margin = new Padding(3, 2, 3, 2);
            cmbPaperSize.Name = "cmbPaperSize";
            cmbPaperSize.Size = new Size(106, 23);
            cmbPaperSize.TabIndex = 0;
            // 
            // lblScale
            // 
            lblScale.Font = new Font("Segoe UI", 9F);
            lblScale.Location = new Point(10, 48);
            lblScale.Name = "lblScale";
            lblScale.Size = new Size(70, 15);
            lblScale.TabIndex = 1;
            lblScale.Text = "Print Scale:";
            // 
            // lblScalePrefix
            // 
            lblScalePrefix.AutoSize = true;
            lblScalePrefix.Font = new Font("Segoe UI", 9F);
            lblScalePrefix.Location = new Point(88, 48);
            lblScalePrefix.Name = "lblScalePrefix";
            lblScalePrefix.Size = new Size(19, 15);
            lblScalePrefix.TabIndex = 2;
            lblScalePrefix.Text = "1 :";
            // 
            // nudPrintScale
            // 
            nudPrintScale.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            nudPrintScale.Location = new Point(110, 46);
            nudPrintScale.Margin = new Padding(3, 2, 3, 2);
            nudPrintScale.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudPrintScale.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            nudPrintScale.Name = "nudPrintScale";
            nudPrintScale.Size = new Size(88, 23);
            nudPrintScale.TabIndex = 1;
            nudPrintScale.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(btnRestoreDefaults);
            pnlFooter.Controls.Add(btnApply);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 324);
            pnlFooter.Margin = new Padding(3, 2, 3, 2);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(7, 6, 7, 6);
            pnlFooter.Size = new Size(493, 41);
            pnlFooter.TabIndex = 1;
            // 
            // btnRestoreDefaults
            // 
            btnRestoreDefaults.Location = new Point(209, 4);
            btnRestoreDefaults.Margin = new Padding(3, 2, 3, 2);
            btnRestoreDefaults.Name = "btnRestoreDefaults";
            btnRestoreDefaults.Size = new Size(123, 26);
            btnRestoreDefaults.TabIndex = 2;
            btnRestoreDefaults.Text = "Restore Defaults";
            btnRestoreDefaults.Click += btnRestoreDefaults_Click;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(338, 4);
            btnApply.Margin = new Padding(3, 2, 3, 2);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(70, 26);
            btnApply.TabIndex = 0;
            btnApply.Text = "Apply";
            btnApply.Click += btnApply_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(413, 4);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(70, 26);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "OK";
            btnCancel.Click += btnCancel_Click;
            // 
            // frmProjectSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(493, 365);
            Controls.Add(tabSettings);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmProjectSettings";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Project Settings";
            Load += frmProjectSettings_Load;
            tabSettings.ResumeLayout(false);
            tabCoordinates.ResumeLayout(false);
            grpCRS.ResumeLayout(false);
            grpDatumTransformation.ResumeLayout(false);
            tabArea.ResumeLayout(false);
            grpAreaUnit.ResumeLayout(false);
            tabMapCanvas.ResumeLayout(false);
            grpOther.ResumeLayout(false);
            grpOther.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudSnapTolerance).EndInit();
            grpCanvasTheme.ResumeLayout(false);
            grpCanvasTheme.PerformLayout();
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
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // Controls
        private TabControl tabSettings;
        private TabPage tabCoordinates;
        private TabPage tabArea;
        private TabPage tabMapCanvas;
        private TabPage tabParcel;
        private TabPage tabDocument;
        private TabPage tabPrint;
        private Panel pnlFooter;
        private Button btnRestoreDefaults;
        private Button btnApply;
        private Button btnCancel;
        private GroupBox grpCRS;
        private Label lblCRS;
        private ComboBox cmbCRS;
        private Button btnManageCRS;
        private Label lblCrsInfo;
        private GroupBox grpDatumTransformation;
        private Label lblDatum;
        private ComboBox cmbDatumTransformation;
        private Button btnManageDatum;
        private Label lblDatumNote;
        private GroupBox grpAreaUnit;
        private Label lblTraditionalUnit;
        private ComboBox cmbTraditionalUnit;
        private Label lblAreaNote;
        private GroupBox grpCanvasTheme;
        private Label lblCanvasTheme;
        private ComboBox cmbCanvasTheme;
        private Label lblBgColor;
        private Panel pnlBgColor;
        private Button btnPickColor;
        private Label lblBgColorHex;
        private Label lblGridColor;
        private Panel pnlGridColor;
        private Button btnPickGridColor;
        private Label lblGridColorHex;
        private CheckBox chkGridVisible;
        private CheckBox chkSnapEnabled;
        private Label lblSnapTolerance;
        private NumericUpDown nudSnapTolerance;
        private Label lblSnapUnit;
        private GroupBox grpParcelNum;
        private Label lblParcelFormat;
        private ComboBox cmbParcelFormat;
        private Label lblParcelPrefix;
        private TextBox txtParcelPrefix;
        private Label lblPadding;
        private NumericUpDown nudPadding;
        private GroupBox grpReplotRules;
        private Label lblMinPlot;
        private NumericUpDown nudMinPlot;
        private Label lblSqm;
        private GroupBox grpDocument;
        private Label lblLanguage;
        private ComboBox cmbLanguage;
        private Label lblDateFormat;
        private ComboBox cmbDateFormat;
        private GroupBox grpPrint;
        private Label lblPaperSize;
        private ComboBox cmbPaperSize;
        private Label lblScale;
        private Label lblScalePrefix;
        private NumericUpDown nudPrintScale;
        private GroupBox grpOther;
        private CheckBox chkOriginAxisMarkerVisible;
    }
}
