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
            grpSnap = new GroupBox();
            chkSnapEnabled = new CheckBox();
            chkOrthoEnabled = new CheckBox();
            lblSnapTolerance = new Label();
            nudSnapTolerance = new NumericUpDown();
            lblSnapUnit = new Label();
            lblSnapGlyphSize = new Label();
            nudSnapGlyphSize = new NumericUpDown();
            lblSnapGlyphUnit = new Label();
            grpOther = new GroupBox();
            chkOriginAxisMarkerVisible = new CheckBox();
            chkGridVisible = new CheckBox();
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
            chkNorthMarker = new CheckBox();
            grpGraphics = new GroupBox();
            chkAntiAliasing = new CheckBox();
            tabSettings.SuspendLayout();
            tabCoordinates.SuspendLayout();
            grpCRS.SuspendLayout();
            grpDatumTransformation.SuspendLayout();
            tabArea.SuspendLayout();
            grpAreaUnit.SuspendLayout();
            tabMapCanvas.SuspendLayout();
            grpSnap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSnapTolerance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSnapGlyphSize).BeginInit();
            grpOther.SuspendLayout();
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
            grpGraphics.SuspendLayout();
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
            tabSettings.Name = "tabSettings";
            tabSettings.Padding = new Point(8, 4);
            tabSettings.SelectedIndex = 0;
            tabSettings.Size = new Size(563, 432);
            tabSettings.TabIndex = 0;
            // 
            // tabCoordinates
            // 
            tabCoordinates.Controls.Add(grpCRS);
            tabCoordinates.Controls.Add(grpDatumTransformation);
            tabCoordinates.Location = new Point(4, 31);
            tabCoordinates.Name = "tabCoordinates";
            tabCoordinates.Padding = new Padding(8);
            tabCoordinates.Size = new Size(555, 397);
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
            grpCRS.Location = new Point(8, 8);
            grpCRS.Name = "grpCRS";
            grpCRS.Size = new Size(536, 128);
            grpCRS.TabIndex = 0;
            grpCRS.TabStop = false;
            grpCRS.Text = "Coordinate Reference System";
            // 
            // lblCRS
            // 
            lblCRS.Font = new Font("Segoe UI", 9F);
            lblCRS.Location = new Point(11, 27);
            lblCRS.Name = "lblCRS";
            lblCRS.Size = new Size(80, 20);
            lblCRS.TabIndex = 0;
            lblCRS.Text = "Select CRS:";
            // 
            // cmbCRS
            // 
            cmbCRS.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCRS.Font = new Font("Segoe UI", 9F);
            cmbCRS.Location = new Point(11, 48);
            cmbCRS.Name = "cmbCRS";
            cmbCRS.Size = new Size(361, 28);
            cmbCRS.TabIndex = 0;
            cmbCRS.SelectedIndexChanged += cmbCRS_SelectedIndexChanged;
            // 
            // btnManageCRS
            // 
            btnManageCRS.Location = new Point(378, 45);
            btnManageCRS.Name = "btnManageCRS";
            btnManageCRS.Size = new Size(110, 33);
            btnManageCRS.TabIndex = 1;
            btnManageCRS.Text = "Manage...";
            btnManageCRS.Click += btnManageCRS_Click;
            // 
            // lblCrsInfo
            // 
            lblCrsInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblCrsInfo.ForeColor = SystemColors.GrayText;
            lblCrsInfo.Location = new Point(13, 81);
            lblCrsInfo.Name = "lblCrsInfo";
            lblCrsInfo.Size = new Size(475, 33);
            lblCrsInfo.TabIndex = 2;
            // 
            // grpDatumTransformation
            // 
            grpDatumTransformation.Controls.Add(lblDatum);
            grpDatumTransformation.Controls.Add(cmbDatumTransformation);
            grpDatumTransformation.Controls.Add(btnManageDatum);
            grpDatumTransformation.Controls.Add(lblDatumNote);
            grpDatumTransformation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpDatumTransformation.Location = new Point(8, 155);
            grpDatumTransformation.Name = "grpDatumTransformation";
            grpDatumTransformation.Size = new Size(536, 119);
            grpDatumTransformation.TabIndex = 1;
            grpDatumTransformation.TabStop = false;
            grpDatumTransformation.Text = "Datum Transformation (MUTM zones only)";
            // 
            // lblDatum
            // 
            lblDatum.Font = new Font("Segoe UI", 9F);
            lblDatum.Location = new Point(11, 27);
            lblDatum.Name = "lblDatum";
            lblDatum.Size = new Size(110, 20);
            lblDatum.TabIndex = 0;
            lblDatum.Text = "Transformation:";
            // 
            // cmbDatumTransformation
            // 
            cmbDatumTransformation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDatumTransformation.Font = new Font("Segoe UI", 9F);
            cmbDatumTransformation.Location = new Point(11, 48);
            cmbDatumTransformation.Name = "cmbDatumTransformation";
            cmbDatumTransformation.Size = new Size(361, 28);
            cmbDatumTransformation.TabIndex = 2;
            // 
            // btnManageDatum
            // 
            btnManageDatum.Location = new Point(378, 45);
            btnManageDatum.Name = "btnManageDatum";
            btnManageDatum.Size = new Size(110, 33);
            btnManageDatum.TabIndex = 3;
            btnManageDatum.Text = "Manage...";
            btnManageDatum.Click += btnManageDatum_Click;
            // 
            // lblDatumNote
            // 
            lblDatumNote.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblDatumNote.ForeColor = SystemColors.GrayText;
            lblDatumNote.Location = new Point(11, 92);
            lblDatumNote.Name = "lblDatumNote";
            lblDatumNote.Size = new Size(475, 28);
            lblDatumNote.TabIndex = 4;
            lblDatumNote.Text = "Required for converting MUTM coordinates to/from WGS84.";
            // 
            // tabArea
            // 
            tabArea.Controls.Add(grpAreaUnit);
            tabArea.Location = new Point(4, 31);
            tabArea.Name = "tabArea";
            tabArea.Padding = new Padding(8);
            tabArea.Size = new Size(555, 397);
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
            grpAreaUnit.Location = new Point(8, 8);
            grpAreaUnit.Name = "grpAreaUnit";
            grpAreaUnit.Size = new Size(536, 136);
            grpAreaUnit.TabIndex = 0;
            grpAreaUnit.TabStop = false;
            grpAreaUnit.Text = "Traditional Area Display Unit";
            // 
            // lblTraditionalUnit
            // 
            lblTraditionalUnit.Font = new Font("Segoe UI", 9F);
            lblTraditionalUnit.Location = new Point(11, 27);
            lblTraditionalUnit.Name = "lblTraditionalUnit";
            lblTraditionalUnit.Size = new Size(280, 20);
            lblTraditionalUnit.TabIndex = 0;
            lblTraditionalUnit.Text = "Display unit for reports and documents:";
            // 
            // cmbTraditionalUnit
            // 
            cmbTraditionalUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTraditionalUnit.Font = new Font("Segoe UI", 9F);
            cmbTraditionalUnit.Items.AddRange(new object[] { "RAPD — Ropani-Aana-Paisa-Daam (Hilly)", "BKD  — Bigha-Kattha-Dhur (Terai)" });
            cmbTraditionalUnit.Location = new Point(11, 52);
            cmbTraditionalUnit.Name = "cmbTraditionalUnit";
            cmbTraditionalUnit.Size = new Size(361, 28);
            cmbTraditionalUnit.TabIndex = 0;
            // 
            // lblAreaNote
            // 
            lblAreaNote.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblAreaNote.ForeColor = SystemColors.GrayText;
            lblAreaNote.Location = new Point(11, 90);
            lblAreaNote.Name = "lblAreaNote";
            lblAreaNote.Size = new Size(475, 29);
            lblAreaNote.TabIndex = 1;
            lblAreaNote.Text = "All internal calculations always use Square Meters.";
            // 
            // tabMapCanvas
            // 
            tabMapCanvas.Controls.Add(grpGraphics);
            tabMapCanvas.Controls.Add(grpSnap);
            tabMapCanvas.Controls.Add(grpOther);
            tabMapCanvas.Controls.Add(grpCanvasTheme);
            tabMapCanvas.Location = new Point(4, 31);
            tabMapCanvas.Name = "tabMapCanvas";
            tabMapCanvas.Padding = new Padding(8);
            tabMapCanvas.Size = new Size(555, 397);
            tabMapCanvas.TabIndex = 2;
            tabMapCanvas.Text = "Map Canvas";
            tabMapCanvas.UseVisualStyleBackColor = true;
            // 
            // grpSnap
            // 
            grpSnap.Controls.Add(chkSnapEnabled);
            grpSnap.Controls.Add(chkOrthoEnabled);
            grpSnap.Controls.Add(lblSnapTolerance);
            grpSnap.Controls.Add(nudSnapTolerance);
            grpSnap.Controls.Add(lblSnapUnit);
            grpSnap.Controls.Add(lblSnapGlyphSize);
            grpSnap.Controls.Add(nudSnapGlyphSize);
            grpSnap.Controls.Add(lblSnapGlyphUnit);
            grpSnap.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpSnap.Location = new Point(8, 190);
            grpSnap.Name = "grpSnap";
            grpSnap.Size = new Size(267, 182);
            grpSnap.TabIndex = 10;
            grpSnap.TabStop = false;
            grpSnap.Text = "Snap and Ortho";
            // 
            // chkSnapEnabled
            // 
            chkSnapEnabled.Font = new Font("Segoe UI", 9F);
            chkSnapEnabled.Location = new Point(10, 26);
            chkSnapEnabled.Name = "chkSnapEnabled";
            chkSnapEnabled.Size = new Size(119, 27);
            chkSnapEnabled.TabIndex = 4;
            chkSnapEnabled.Text = "Enable Snap";
            // 
            // chkOrthoEnabled
            // 
            chkOrthoEnabled.Font = new Font("Segoe UI", 9F);
            chkOrthoEnabled.Location = new Point(10, 137);
            chkOrthoEnabled.Name = "chkOrthoEnabled";
            chkOrthoEnabled.Size = new Size(150, 27);
            chkOrthoEnabled.TabIndex = 13;
            chkOrthoEnabled.Text = "Enable Ortho";
            // 
            // lblSnapTolerance
            // 
            lblSnapTolerance.Font = new Font("Segoe UI", 9F);
            lblSnapTolerance.Location = new Point(30, 57);
            lblSnapTolerance.Name = "lblSnapTolerance";
            lblSnapTolerance.Size = new Size(79, 21);
            lblSnapTolerance.TabIndex = 7;
            lblSnapTolerance.Text = "Tolerance";
            // 
            // nudSnapTolerance
            // 
            nudSnapTolerance.DecimalPlaces = 1;
            nudSnapTolerance.Font = new Font("Segoe UI", 9F);
            nudSnapTolerance.Location = new Point(115, 54);
            nudSnapTolerance.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudSnapTolerance.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSnapTolerance.Name = "nudSnapTolerance";
            nudSnapTolerance.Size = new Size(70, 27);
            nudSnapTolerance.TabIndex = 5;
            nudSnapTolerance.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // lblSnapUnit
            // 
            lblSnapUnit.AutoSize = true;
            lblSnapUnit.Font = new Font("Segoe UI", 9F);
            lblSnapUnit.ForeColor = SystemColors.GrayText;
            lblSnapUnit.Location = new Point(191, 57);
            lblSnapUnit.Name = "lblSnapUnit";
            lblSnapUnit.Size = new Size(25, 20);
            lblSnapUnit.TabIndex = 8;
            lblSnapUnit.Text = "px";
            // 
            // lblSnapGlyphSize
            // 
            lblSnapGlyphSize.Font = new Font("Segoe UI", 9F);
            lblSnapGlyphSize.Location = new Point(30, 89);
            lblSnapGlyphSize.Name = "lblSnapGlyphSize";
            lblSnapGlyphSize.Size = new Size(79, 21);
            lblSnapGlyphSize.TabIndex = 10;
            lblSnapGlyphSize.Text = "Glyph Size";
            // 
            // nudSnapGlyphSize
            // 
            nudSnapGlyphSize.DecimalPlaces = 1;
            nudSnapGlyphSize.Font = new Font("Segoe UI", 9F);
            nudSnapGlyphSize.Location = new Point(115, 86);
            nudSnapGlyphSize.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            nudSnapGlyphSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            nudSnapGlyphSize.Name = "nudSnapGlyphSize";
            nudSnapGlyphSize.Size = new Size(70, 27);
            nudSnapGlyphSize.TabIndex = 11;
            nudSnapGlyphSize.Value = new decimal(new int[] { 14, 0, 0, 0 });
            // 
            // lblSnapGlyphUnit
            // 
            lblSnapGlyphUnit.AutoSize = true;
            lblSnapGlyphUnit.Font = new Font("Segoe UI", 9F);
            lblSnapGlyphUnit.ForeColor = SystemColors.GrayText;
            lblSnapGlyphUnit.Location = new Point(191, 89);
            lblSnapGlyphUnit.Name = "lblSnapGlyphUnit";
            lblSnapGlyphUnit.Size = new Size(25, 20);
            lblSnapGlyphUnit.TabIndex = 12;
            lblSnapGlyphUnit.Text = "px";
            // 
            // grpOther
            // 
            grpOther.Controls.Add(chkNorthMarker);
            grpOther.Controls.Add(chkOriginAxisMarkerVisible);
            grpOther.Controls.Add(chkGridVisible);
            grpOther.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpOther.Location = new Point(281, 11);
            grpOther.Name = "grpOther";
            grpOther.Size = new Size(263, 173);
            grpOther.TabIndex = 9;
            grpOther.TabStop = false;
            grpOther.Text = "UI Layers";
            // 
            // chkOriginAxisMarkerVisible
            // 
            chkOriginAxisMarkerVisible.Font = new Font("Segoe UI", 9F);
            chkOriginAxisMarkerVisible.Location = new Point(10, 55);
            chkOriginAxisMarkerVisible.Name = "chkOriginAxisMarkerVisible";
            chkOriginAxisMarkerVisible.Size = new Size(150, 27);
            chkOriginAxisMarkerVisible.TabIndex = 9;
            chkOriginAxisMarkerVisible.Text = "Show Origin/Axis Marker";
            // 
            // chkGridVisible
            // 
            chkGridVisible.Font = new Font("Segoe UI", 9F);
            chkGridVisible.Location = new Point(10, 27);
            chkGridVisible.Name = "chkGridVisible";
            chkGridVisible.Size = new Size(150, 27);
            chkGridVisible.TabIndex = 3;
            chkGridVisible.Text = "Show Grid Lines";
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
            grpCanvasTheme.Location = new Point(8, 8);
            grpCanvasTheme.Name = "grpCanvasTheme";
            grpCanvasTheme.Size = new Size(267, 176);
            grpCanvasTheme.TabIndex = 0;
            grpCanvasTheme.TabStop = false;
            grpCanvasTheme.Text = "Canvas Theme";
            grpCanvasTheme.Enter += grpCanvasTheme_Enter;
            // 
            // lblCanvasTheme
            // 
            lblCanvasTheme.Font = new Font("Segoe UI", 9F);
            lblCanvasTheme.Location = new Point(11, 27);
            lblCanvasTheme.Name = "lblCanvasTheme";
            lblCanvasTheme.Size = new Size(110, 20);
            lblCanvasTheme.TabIndex = 0;
            lblCanvasTheme.Text = "Theme Preset:";
            // 
            // cmbCanvasTheme
            // 
            cmbCanvasTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCanvasTheme.Font = new Font("Segoe UI", 9F);
            cmbCanvasTheme.Items.AddRange(new object[] { "Light", "Dark", "Custom" });
            cmbCanvasTheme.Location = new Point(126, 24);
            cmbCanvasTheme.Name = "cmbCanvasTheme";
            cmbCanvasTheme.Size = new Size(135, 28);
            cmbCanvasTheme.TabIndex = 0;
            cmbCanvasTheme.SelectedIndexChanged += cmbCanvasTheme_SelectedIndexChanged;
            // 
            // lblBgColor
            // 
            lblBgColor.Font = new Font("Segoe UI", 9F);
            lblBgColor.Location = new Point(11, 55);
            lblBgColor.Name = "lblBgColor";
            lblBgColor.Size = new Size(130, 20);
            lblBgColor.TabIndex = 1;
            lblBgColor.Text = "Background Color:";
            // 
            // pnlBgColor
            // 
            pnlBgColor.BackColor = Color.FromArgb(30, 41, 51);
            pnlBgColor.BorderStyle = BorderStyle.FixedSingle;
            pnlBgColor.Cursor = Cursors.Hand;
            pnlBgColor.Location = new Point(24, 81);
            pnlBgColor.Name = "pnlBgColor";
            pnlBgColor.Size = new Size(34, 26);
            pnlBgColor.TabIndex = 2;
            // 
            // btnPickColor
            // 
            btnPickColor.Location = new Point(62, 76);
            btnPickColor.Name = "btnPickColor";
            btnPickColor.Size = new Size(80, 36);
            btnPickColor.TabIndex = 1;
            btnPickColor.Text = "Choose...";
            btnPickColor.Click += btnPickColor_Click;
            // 
            // lblBgColorHex
            // 
            lblBgColorHex.AutoSize = true;
            lblBgColorHex.Font = new Font("Consolas", 9F);
            lblBgColorHex.ForeColor = SystemColors.GrayText;
            lblBgColorHex.Location = new Point(152, 81);
            lblBgColorHex.Name = "lblBgColorHex";
            lblBgColorHex.Size = new Size(0, 18);
            lblBgColorHex.TabIndex = 3;
            // 
            // lblGridColor
            // 
            lblGridColor.Font = new Font("Segoe UI", 9F);
            lblGridColor.Location = new Point(10, 115);
            lblGridColor.Name = "lblGridColor";
            lblGridColor.Size = new Size(130, 20);
            lblGridColor.TabIndex = 4;
            lblGridColor.Text = "Grid Color:";
            // 
            // pnlGridColor
            // 
            pnlGridColor.BackColor = Color.FromArgb(74, 85, 98);
            pnlGridColor.BorderStyle = BorderStyle.FixedSingle;
            pnlGridColor.Cursor = Cursors.Hand;
            pnlGridColor.Location = new Point(24, 140);
            pnlGridColor.Name = "pnlGridColor";
            pnlGridColor.Size = new Size(34, 26);
            pnlGridColor.TabIndex = 5;
            // 
            // btnPickGridColor
            // 
            btnPickGridColor.Location = new Point(62, 135);
            btnPickGridColor.Name = "btnPickGridColor";
            btnPickGridColor.Size = new Size(80, 36);
            btnPickGridColor.TabIndex = 2;
            btnPickGridColor.Text = "Choose...";
            btnPickGridColor.Click += btnPickGridColor_Click;
            // 
            // lblGridColorHex
            // 
            lblGridColorHex.AutoSize = true;
            lblGridColorHex.Font = new Font("Consolas", 9F);
            lblGridColorHex.ForeColor = SystemColors.GrayText;
            lblGridColorHex.Location = new Point(150, 140);
            lblGridColorHex.Name = "lblGridColorHex";
            lblGridColorHex.Size = new Size(0, 18);
            lblGridColorHex.TabIndex = 6;
            // 
            // tabParcel
            // 
            tabParcel.Controls.Add(grpParcelNum);
            tabParcel.Controls.Add(grpReplotRules);
            tabParcel.Location = new Point(4, 31);
            tabParcel.Name = "tabParcel";
            tabParcel.Padding = new Padding(8);
            tabParcel.Size = new Size(555, 397);
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
            grpParcelNum.Location = new Point(8, 8);
            grpParcelNum.Name = "grpParcelNum";
            grpParcelNum.Size = new Size(502, 148);
            grpParcelNum.TabIndex = 0;
            grpParcelNum.TabStop = false;
            grpParcelNum.Text = "Parcel Numbering";
            // 
            // lblParcelFormat
            // 
            lblParcelFormat.Font = new Font("Segoe UI", 9F);
            lblParcelFormat.Location = new Point(11, 27);
            lblParcelFormat.Name = "lblParcelFormat";
            lblParcelFormat.Size = new Size(80, 20);
            lblParcelFormat.TabIndex = 0;
            lblParcelFormat.Text = "Format:";
            // 
            // cmbParcelFormat
            // 
            cmbParcelFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbParcelFormat.Font = new Font("Segoe UI", 9F);
            cmbParcelFormat.Items.AddRange(new object[] { "Sequential", "BlockBased", "Custom" });
            cmbParcelFormat.Location = new Point(101, 24);
            cmbParcelFormat.Name = "cmbParcelFormat";
            cmbParcelFormat.Size = new Size(159, 28);
            cmbParcelFormat.TabIndex = 0;
            // 
            // lblParcelPrefix
            // 
            lblParcelPrefix.Font = new Font("Segoe UI", 9F);
            lblParcelPrefix.Location = new Point(11, 61);
            lblParcelPrefix.Name = "lblParcelPrefix";
            lblParcelPrefix.Size = new Size(80, 20);
            lblParcelPrefix.TabIndex = 1;
            lblParcelPrefix.Text = "Prefix:";
            // 
            // txtParcelPrefix
            // 
            txtParcelPrefix.Font = new Font("Segoe UI", 9F);
            txtParcelPrefix.Location = new Point(101, 60);
            txtParcelPrefix.Name = "txtParcelPrefix";
            txtParcelPrefix.PlaceholderText = "e.g. RP-";
            txtParcelPrefix.Size = new Size(121, 27);
            txtParcelPrefix.TabIndex = 1;
            // 
            // lblPadding
            // 
            lblPadding.Font = new Font("Segoe UI", 9F);
            lblPadding.Location = new Point(11, 100);
            lblPadding.Name = "lblPadding";
            lblPadding.Size = new Size(101, 20);
            lblPadding.TabIndex = 2;
            lblPadding.Text = "Digit Padding:";
            // 
            // nudPadding
            // 
            nudPadding.Location = new Point(117, 99);
            nudPadding.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            nudPadding.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudPadding.Name = "nudPadding";
            nudPadding.Size = new Size(59, 27);
            nudPadding.TabIndex = 2;
            nudPadding.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // grpReplotRules
            // 
            grpReplotRules.Controls.Add(lblMinPlot);
            grpReplotRules.Controls.Add(nudMinPlot);
            grpReplotRules.Controls.Add(lblSqm);
            grpReplotRules.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpReplotRules.Location = new Point(8, 164);
            grpReplotRules.Name = "grpReplotRules";
            grpReplotRules.Size = new Size(502, 64);
            grpReplotRules.TabIndex = 1;
            grpReplotRules.TabStop = false;
            grpReplotRules.Text = "Replotting Rules";
            // 
            // lblMinPlot
            // 
            lblMinPlot.Font = new Font("Segoe UI", 9F);
            lblMinPlot.Location = new Point(11, 27);
            lblMinPlot.Name = "lblMinPlot";
            lblMinPlot.Size = new Size(130, 20);
            lblMinPlot.TabIndex = 0;
            lblMinPlot.Text = "Minimum Plot Area:";
            // 
            // nudMinPlot
            // 
            nudMinPlot.DecimalPlaces = 2;
            nudMinPlot.Location = new Point(146, 24);
            nudMinPlot.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudMinPlot.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudMinPlot.Name = "nudMinPlot";
            nudMinPlot.Size = new Size(90, 27);
            nudMinPlot.TabIndex = 3;
            nudMinPlot.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblSqm
            // 
            lblSqm.AutoSize = true;
            lblSqm.Font = new Font("Segoe UI", 9F);
            lblSqm.ForeColor = SystemColors.GrayText;
            lblSqm.Location = new Point(242, 27);
            lblSqm.Name = "lblSqm";
            lblSqm.Size = new Size(42, 20);
            lblSqm.TabIndex = 4;
            lblSqm.Text = "Sq.m";
            // 
            // tabDocument
            // 
            tabDocument.Controls.Add(grpDocument);
            tabDocument.Location = new Point(4, 31);
            tabDocument.Name = "tabDocument";
            tabDocument.Padding = new Padding(8);
            tabDocument.Size = new Size(555, 397);
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
            grpDocument.Location = new Point(8, 8);
            grpDocument.Name = "grpDocument";
            grpDocument.Size = new Size(502, 131);
            grpDocument.TabIndex = 0;
            grpDocument.TabStop = false;
            grpDocument.Text = "Document Output";
            // 
            // lblLanguage
            // 
            lblLanguage.Font = new Font("Segoe UI", 9F);
            lblLanguage.Location = new Point(11, 27);
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
            cmbLanguage.Location = new Point(101, 24);
            cmbLanguage.Name = "cmbLanguage";
            cmbLanguage.Size = new Size(180, 28);
            cmbLanguage.TabIndex = 0;
            // 
            // lblDateFormat
            // 
            lblDateFormat.Font = new Font("Segoe UI", 9F);
            lblDateFormat.Location = new Point(11, 64);
            lblDateFormat.Name = "lblDateFormat";
            lblDateFormat.Size = new Size(80, 20);
            lblDateFormat.TabIndex = 1;
            lblDateFormat.Text = "Date Format:";
            // 
            // cmbDateFormat
            // 
            cmbDateFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDateFormat.Font = new Font("Segoe UI", 9F);
            cmbDateFormat.Items.AddRange(new object[] { "AD", "BS", "Both" });
            cmbDateFormat.Location = new Point(101, 61);
            cmbDateFormat.Name = "cmbDateFormat";
            cmbDateFormat.Size = new Size(180, 28);
            cmbDateFormat.TabIndex = 1;
            // 
            // tabPrint
            // 
            tabPrint.Controls.Add(grpPrint);
            tabPrint.Location = new Point(4, 31);
            tabPrint.Name = "tabPrint";
            tabPrint.Padding = new Padding(8);
            tabPrint.Size = new Size(555, 397);
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
            grpPrint.Location = new Point(8, 8);
            grpPrint.Name = "grpPrint";
            grpPrint.Size = new Size(502, 131);
            grpPrint.TabIndex = 0;
            grpPrint.TabStop = false;
            grpPrint.Text = "Print & Export";
            // 
            // lblPaperSize
            // 
            lblPaperSize.Font = new Font("Segoe UI", 9F);
            lblPaperSize.Location = new Point(11, 27);
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
            cmbPaperSize.Location = new Point(101, 24);
            cmbPaperSize.Name = "cmbPaperSize";
            cmbPaperSize.Size = new Size(121, 28);
            cmbPaperSize.TabIndex = 0;
            // 
            // lblScale
            // 
            lblScale.Font = new Font("Segoe UI", 9F);
            lblScale.Location = new Point(11, 64);
            lblScale.Name = "lblScale";
            lblScale.Size = new Size(80, 20);
            lblScale.TabIndex = 1;
            lblScale.Text = "Print Scale:";
            // 
            // lblScalePrefix
            // 
            lblScalePrefix.AutoSize = true;
            lblScalePrefix.Font = new Font("Segoe UI", 9F);
            lblScalePrefix.Location = new Point(101, 64);
            lblScalePrefix.Name = "lblScalePrefix";
            lblScalePrefix.Size = new Size(24, 20);
            lblScalePrefix.TabIndex = 2;
            lblScalePrefix.Text = "1 :";
            // 
            // nudPrintScale
            // 
            nudPrintScale.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            nudPrintScale.Location = new Point(126, 61);
            nudPrintScale.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudPrintScale.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            nudPrintScale.Name = "nudPrintScale";
            nudPrintScale.Size = new Size(101, 27);
            nudPrintScale.TabIndex = 1;
            nudPrintScale.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(btnRestoreDefaults);
            pnlFooter.Controls.Add(btnApply);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 432);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(8);
            pnlFooter.Size = new Size(563, 55);
            pnlFooter.TabIndex = 1;
            // 
            // btnRestoreDefaults
            // 
            btnRestoreDefaults.Location = new Point(239, 5);
            btnRestoreDefaults.Name = "btnRestoreDefaults";
            btnRestoreDefaults.Size = new Size(141, 35);
            btnRestoreDefaults.TabIndex = 2;
            btnRestoreDefaults.Text = "Restore Defaults";
            btnRestoreDefaults.Click += btnRestoreDefaults_Click;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(386, 5);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(80, 35);
            btnApply.TabIndex = 0;
            btnApply.Text = "Apply";
            btnApply.Click += btnApply_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(472, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 35);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "OK";
            btnCancel.Click += btnCancel_Click;
            // 
            // chkNorthMarker
            // 
            chkNorthMarker.Font = new Font("Segoe UI", 9F);
            chkNorthMarker.Location = new Point(10, 82);
            chkNorthMarker.Name = "chkNorthMarker";
            chkNorthMarker.Size = new Size(197, 27);
            chkNorthMarker.TabIndex = 10;
            chkNorthMarker.Text = "Show North Marker";
            // 
            // grpGraphics
            // 
            grpGraphics.Controls.Add(chkAntiAliasing);
            grpGraphics.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpGraphics.Location = new Point(281, 190);
            grpGraphics.Name = "grpGraphics";
            grpGraphics.Size = new Size(263, 182);
            grpGraphics.TabIndex = 14;
            grpGraphics.TabStop = false;
            grpGraphics.Text = "Graphics";
            // 
            // chkAntiAliasing
            // 
            chkAntiAliasing.Font = new Font("Segoe UI", 9F);
            chkAntiAliasing.Location = new Point(10, 26);
            chkAntiAliasing.Name = "chkAntiAliasing";
            chkAntiAliasing.Size = new Size(119, 27);
            chkAntiAliasing.TabIndex = 14;
            chkAntiAliasing.Text = "Anti-Aliasing";
            // 
            // frmProjectSettings
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(563, 487);
            Controls.Add(tabSettings);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
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
            grpSnap.ResumeLayout(false);
            grpSnap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudSnapTolerance).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSnapGlyphSize).EndInit();
            grpOther.ResumeLayout(false);
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
            grpGraphics.ResumeLayout(false);
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
        private GroupBox grpSnap;
        private CheckBox chkGridVisible;
        private CheckBox chkSnapEnabled;
        private CheckBox chkOrthoEnabled;
        private Label lblSnapTolerance;
        private NumericUpDown nudSnapTolerance;
        private Label lblSnapUnit;
        private Label lblSnapGlyphSize;
        private NumericUpDown nudSnapGlyphSize;
        private Label lblSnapGlyphUnit;
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
        private CheckBox chkNorthMarker;
        private GroupBox grpGraphics;
        private CheckBox chkAntiAliasing;
    }
}
