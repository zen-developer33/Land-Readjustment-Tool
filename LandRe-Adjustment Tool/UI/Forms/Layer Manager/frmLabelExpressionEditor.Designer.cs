namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmLabelExpressionEditor
    {
        private System.ComponentModel.IContainer components = null!;

        // Preset group
        private GroupBox _grpPresets        = null!;
        private Label    _lblPresetTemplate = null!;
        private ComboBox _cboPresets        = null!;
        private Button   _btnApplyPreset    = null!;

        // Fields group
        private GroupBox _grpFields      = null!;
        private ListBox  _lstFields      = null!;
        private Button   _btnInsertField = null!;

        // Expression group
        private GroupBox _grpExpression      = null!;
        private Label    _lblExpressionHint  = null!;
        private TextBox  _txtExpression      = null!;
        private Label    _lblQuickInsert     = null!;
        private Button   _btnInsertNewline   = null!;
        private Button   _btnInsertSpace     = null!;
        private Button   _btnClearExpression = null!;

        // Preview group
        private GroupBox _grpPreview          = null!;
        private Button   _btnRefreshPreview   = null!;
        private Label    _lblAlignmentLabel   = null!;
        private ComboBox _cboAlignment        = null!;
        private Panel    _pnlPreviewOutput    = null!;
        private Label    _lblPreviewOutput    = null!;

        // Footer
        private FlowLayoutPanel _footerPanel = null!;
        private Button          _btnOk       = null!;
        private Button          _btnCancel   = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _grpPresets = new GroupBox();
            _lblPresetTemplate = new Label();
            _cboPresets = new ComboBox();
            _btnApplyPreset = new Button();
            _grpFields = new GroupBox();
            _lstFields = new ListBox();
            _btnInsertField = new Button();
            _grpExpression = new GroupBox();
            _lblExpressionHint = new Label();
            _txtExpression = new TextBox();
            _lblQuickInsert = new Label();
            _btnInsertNewline = new Button();
            _btnInsertSpace = new Button();
            _btnClearExpression = new Button();
            _grpPreview = new GroupBox();
            _btnRefreshPreview = new Button();
            _lblAlignmentLabel = new Label();
            _cboAlignment = new ComboBox();
            _pnlPreviewOutput = new Panel();
            _lblPreviewOutput = new Label();
            _footerPanel = new FlowLayoutPanel();
            _btnOk = new Button();
            _btnCancel = new Button();
            _lblPreviewSampleNote = new Label();
            _grpPresets.SuspendLayout();
            _grpFields.SuspendLayout();
            _grpExpression.SuspendLayout();
            _grpPreview.SuspendLayout();
            _pnlPreviewOutput.SuspendLayout();
            _footerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _grpPresets
            // 
            _grpPresets.Controls.Add(_lblPresetTemplate);
            _grpPresets.Controls.Add(_cboPresets);
            _grpPresets.Controls.Add(_btnApplyPreset);
            _grpPresets.Font = new Font("Segoe UI", 9F);
            _grpPresets.Location = new Point(12, 12);
            _grpPresets.Name = "_grpPresets";
            _grpPresets.Size = new Size(626, 64);
            _grpPresets.TabIndex = 0;
            _grpPresets.TabStop = false;
            _grpPresets.Text = "Preset Templates";
            // 
            // _lblPresetTemplate
            // 
            _lblPresetTemplate.Location = new Point(10, 24);
            _lblPresetTemplate.Name = "_lblPresetTemplate";
            _lblPresetTemplate.Size = new Size(78, 23);
            _lblPresetTemplate.TabIndex = 0;
            _lblPresetTemplate.Text = "Template:";
            _lblPresetTemplate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboPresets
            // 
            _cboPresets.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboPresets.Location = new Point(96, 22);
            _cboPresets.Name = "_cboPresets";
            _cboPresets.Size = new Size(384, 28);
            _cboPresets.TabIndex = 1;
            // 
            // _btnApplyPreset
            // 
            _btnApplyPreset.Location = new Point(490, 21);
            _btnApplyPreset.Name = "_btnApplyPreset";
            _btnApplyPreset.Size = new Size(80, 29);
            _btnApplyPreset.TabIndex = 2;
            _btnApplyPreset.Text = "Apply";
            _btnApplyPreset.UseVisualStyleBackColor = true;
            _btnApplyPreset.Click += btnApplyPreset_Click;
            // 
            // _grpFields
            // 
            _grpFields.Controls.Add(_lstFields);
            _grpFields.Controls.Add(_btnInsertField);
            _grpFields.Font = new Font("Segoe UI", 9F);
            _grpFields.Location = new Point(12, 84);
            _grpFields.Name = "_grpFields";
            _grpFields.Size = new Size(188, 284);
            _grpFields.TabIndex = 1;
            _grpFields.TabStop = false;
            _grpFields.Text = "Available Fields";
            // 
            // _lstFields
            // 
            _lstFields.Location = new Point(8, 24);
            _lstFields.Name = "_lstFields";
            _lstFields.Size = new Size(172, 204);
            _lstFields.TabIndex = 0;
            _lstFields.DoubleClick += lstFields_DoubleClick;
            // 
            // _btnInsertField
            // 
            _btnInsertField.Location = new Point(8, 234);
            _btnInsertField.Name = "_btnInsertField";
            _btnInsertField.Size = new Size(172, 43);
            _btnInsertField.TabIndex = 1;
            _btnInsertField.Text = ">> Insert Field";
            _btnInsertField.UseVisualStyleBackColor = true;
            _btnInsertField.Click += btnInsertField_Click;
            // 
            // _grpExpression
            // 
            _grpExpression.Controls.Add(_lblExpressionHint);
            _grpExpression.Controls.Add(_txtExpression);
            _grpExpression.Controls.Add(_lblQuickInsert);
            _grpExpression.Controls.Add(_btnInsertNewline);
            _grpExpression.Controls.Add(_btnInsertSpace);
            _grpExpression.Controls.Add(_btnClearExpression);
            _grpExpression.Font = new Font("Segoe UI", 9F);
            _grpExpression.Location = new Point(208, 84);
            _grpExpression.Name = "_grpExpression";
            _grpExpression.Size = new Size(430, 284);
            _grpExpression.TabIndex = 2;
            _grpExpression.TabStop = false;
            _grpExpression.Text = "Label Expression";
            // 
            // _lblExpressionHint
            // 
            _lblExpressionHint.ForeColor = Color.FromArgb(80, 80, 80);
            _lblExpressionHint.Location = new Point(10, 24);
            _lblExpressionHint.Name = "_lblExpressionHint";
            _lblExpressionHint.Size = new Size(410, 18);
            _lblExpressionHint.TabIndex = 0;
            _lblExpressionHint.Text = "Use {FieldName} for values and \\n for a new line. Double-click a field to insert.";
            // 
            // _txtExpression
            // 
            _txtExpression.Font = new Font("Consolas", 10F);
            _txtExpression.Location = new Point(10, 46);
            _txtExpression.Multiline = true;
            _txtExpression.Name = "_txtExpression";
            _txtExpression.ScrollBars = ScrollBars.Vertical;
            _txtExpression.Size = new Size(410, 196);
            _txtExpression.TabIndex = 1;
            _txtExpression.TextChanged += txtExpression_TextChanged;
            // 
            // _lblQuickInsert
            // 
            _lblQuickInsert.AutoSize = true;
            _lblQuickInsert.Location = new Point(10, 252);
            _lblQuickInsert.Name = "_lblQuickInsert";
            _lblQuickInsert.Size = new Size(48, 20);
            _lblQuickInsert.TabIndex = 2;
            _lblQuickInsert.Text = "Insert:";
            // 
            // _btnInsertNewline
            // 
            _btnInsertNewline.Location = new Point(64, 248);
            _btnInsertNewline.Name = "_btnInsertNewline";
            _btnInsertNewline.Size = new Size(130, 29);
            _btnInsertNewline.TabIndex = 3;
            _btnInsertNewline.Text = "New Line (\\n)";
            _btnInsertNewline.UseVisualStyleBackColor = true;
            _btnInsertNewline.Click += btnInsertNewline_Click;
            // 
            // _btnInsertSpace
            // 
            _btnInsertSpace.Location = new Point(200, 248);
            _btnInsertSpace.Name = "_btnInsertSpace";
            _btnInsertSpace.Size = new Size(74, 29);
            _btnInsertSpace.TabIndex = 4;
            _btnInsertSpace.Text = "Space";
            _btnInsertSpace.UseVisualStyleBackColor = true;
            _btnInsertSpace.Click += btnInsertSpace_Click;
            // 
            // _btnClearExpression
            // 
            _btnClearExpression.Location = new Point(280, 248);
            _btnClearExpression.Name = "_btnClearExpression";
            _btnClearExpression.Size = new Size(74, 29);
            _btnClearExpression.TabIndex = 5;
            _btnClearExpression.Text = "Clear All";
            _btnClearExpression.UseVisualStyleBackColor = true;
            _btnClearExpression.Click += btnClearExpression_Click;
            // 
            // _grpPreview
            // 
            _grpPreview.Controls.Add(_lblPreviewSampleNote);
            _grpPreview.Controls.Add(_btnRefreshPreview);
            _grpPreview.Controls.Add(_lblAlignmentLabel);
            _grpPreview.Controls.Add(_cboAlignment);
            _grpPreview.Controls.Add(_pnlPreviewOutput);
            _grpPreview.Font = new Font("Segoe UI", 9F);
            _grpPreview.Location = new Point(12, 374);
            _grpPreview.Name = "_grpPreview";
            _grpPreview.Size = new Size(626, 172);
            _grpPreview.TabIndex = 3;
            _grpPreview.TabStop = false;
            _grpPreview.Text = "Preview";
            // 
            // _btnRefreshPreview
            // 
            _btnRefreshPreview.Location = new Point(496, 50);
            _btnRefreshPreview.Name = "_btnRefreshPreview";
            _btnRefreshPreview.Size = new Size(120, 32);
            _btnRefreshPreview.TabIndex = 1;
            _btnRefreshPreview.Text = "Next Record";
            _btnRefreshPreview.UseVisualStyleBackColor = true;
            _btnRefreshPreview.Click += btnRefreshPreview_Click;
            // 
            // _lblAlignmentLabel
            // 
            _lblAlignmentLabel.AutoSize = true;
            _lblAlignmentLabel.Location = new Point(11, 57);
            _lblAlignmentLabel.Name = "_lblAlignmentLabel";
            _lblAlignmentLabel.Size = new Size(121, 20);
            _lblAlignmentLabel.TabIndex = 2;
            _lblAlignmentLabel.Text = "Label Alignment:";
            // 
            // _cboAlignment
            // 
            _cboAlignment.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboAlignment.Items.AddRange(new object[] { "Left Top", "Center Top", "Right Top", "Left Middle", "Center Middle", "Right Middle", "Left Bottom", "Center Bottom", "Right Bottom" });
            _cboAlignment.Location = new Point(155, 54);
            _cboAlignment.Name = "_cboAlignment";
            _cboAlignment.Size = new Size(168, 28);
            _cboAlignment.TabIndex = 3;
            // 
            // _pnlPreviewOutput
            // 
            _pnlPreviewOutput.BackColor = Color.White;
            _pnlPreviewOutput.BorderStyle = BorderStyle.FixedSingle;
            _pnlPreviewOutput.Controls.Add(_lblPreviewOutput);
            _pnlPreviewOutput.Location = new Point(10, 94);
            _pnlPreviewOutput.Name = "_pnlPreviewOutput";
            _pnlPreviewOutput.Size = new Size(606, 68);
            _pnlPreviewOutput.TabIndex = 4;
            // 
            // _lblPreviewOutput
            // 
            _lblPreviewOutput.BackColor = Color.White;
            _lblPreviewOutput.Dock = DockStyle.Fill;
            _lblPreviewOutput.Font = new Font("Segoe UI", 9F);
            _lblPreviewOutput.Location = new Point(0, 0);
            _lblPreviewOutput.Name = "_lblPreviewOutput";
            _lblPreviewOutput.Size = new Size(604, 66);
            _lblPreviewOutput.TabIndex = 0;
            _lblPreviewOutput.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _footerPanel
            // 
            _footerPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _footerPanel.Controls.Add(_btnOk);
            _footerPanel.Controls.Add(_btnCancel);
            _footerPanel.FlowDirection = FlowDirection.RightToLeft;
            _footerPanel.Location = new Point(12, 549);
            _footerPanel.Name = "_footerPanel";
            _footerPanel.Size = new Size(626, 40);
            _footerPanel.TabIndex = 4;
            _footerPanel.WrapContents = false;
            // 
            // _btnOk
            // 
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Location = new Point(538, 4);
            _btnOk.Margin = new Padding(8, 4, 0, 0);
            _btnOk.Name = "_btnOk";
            _btnOk.Size = new Size(88, 30);
            _btnOk.TabIndex = 0;
            _btnOk.Text = "OK";
            _btnOk.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(442, 4);
            _btnCancel.Margin = new Padding(8, 4, 0, 0);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new Size(88, 30);
            _btnCancel.TabIndex = 1;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = true;
            // 
            // _lblPreviewSampleNote
            // 
            _lblPreviewSampleNote.ForeColor = Color.FromArgb(80, 80, 80);
            _lblPreviewSampleNote.Location = new Point(10, 24);
            _lblPreviewSampleNote.Name = "_lblPreviewSampleNote";
            _lblPreviewSampleNote.Size = new Size(476, 18);
            _lblPreviewSampleNote.TabIndex = 0;
            _lblPreviewSampleNote.Text = "No layer data — using hardcoded sample values.";
            // 
            // frmLabelExpressionEditor
            // 
            AcceptButton = _btnOk;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            CancelButton = _btnCancel;
            ClientSize = new Size(650, 601);
            Controls.Add(_grpPresets);
            Controls.Add(_grpFields);
            Controls.Add(_grpExpression);
            Controls.Add(_grpPreview);
            Controls.Add(_footerPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmLabelExpressionEditor";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Label Expression Editor";
            _grpPresets.ResumeLayout(false);
            _grpFields.ResumeLayout(false);
            _grpExpression.ResumeLayout(false);
            _grpExpression.PerformLayout();
            _grpPreview.ResumeLayout(false);
            _grpPreview.PerformLayout();
            _pnlPreviewOutput.ResumeLayout(false);
            _footerPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Label _lblPreviewSampleNote;
    }
}
