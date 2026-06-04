namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmHatchPatternPicker
    {
        private System.ComponentModel.IContainer components = null;
        private FlowLayoutPanel _patternLayout;
        private FlowLayoutPanel buttonPanel;
        private Button _okButton;
        private Button _cancelButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _patternLayout = new FlowLayoutPanel();
            buttonPanel = new FlowLayoutPanel();
            _okButton = new Button();
            _cancelButton = new Button();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _patternLayout
            // 
            _patternLayout.AutoScroll = true;
            _patternLayout.Dock = DockStyle.Fill;
            _patternLayout.FlowDirection = FlowDirection.LeftToRight;
            _patternLayout.Location = new Point(0, 0);
            _patternLayout.Name = "_patternLayout";
            _patternLayout.Padding = new Padding(12);
            _patternLayout.Size = new Size(560, 366);
            _patternLayout.TabIndex = 0;
            _patternLayout.WrapContents = true;
            // 
            // buttonPanel
            // 
            buttonPanel.BackColor = SystemColors.Control;
            buttonPanel.Controls.Add(_okButton);
            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(0, 366);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(12);
            buttonPanel.Size = new Size(560, 54);
            buttonPanel.TabIndex = 1;
            // 
            // _okButton
            // 
            _okButton.DialogResult = DialogResult.OK;
            _okButton.Location = new Point(453, 15);
            _okButton.Name = "_okButton";
            _okButton.Size = new Size(92, 29);
            _okButton.TabIndex = 0;
            _okButton.Text = "OK";
            _okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            _cancelButton.DialogResult = DialogResult.Cancel;
            _cancelButton.Location = new Point(355, 15);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new Size(92, 29);
            _cancelButton.TabIndex = 1;
            _cancelButton.Text = "Cancel";
            _cancelButton.UseVisualStyleBackColor = true;
            // 
            // frmHatchPatternPicker
            // 
            AcceptButton = _okButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _cancelButton;
            ClientSize = new Size(560, 420);
            Controls.Add(_patternLayout);
            Controls.Add(buttonPanel);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmHatchPatternPicker";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Choose Hatch Pattern";
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
