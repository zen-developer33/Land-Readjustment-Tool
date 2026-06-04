namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmOperationProgress
    {
        private System.ComponentModel.IContainer components = null;
        private Label _titleLabel;
        private Label _statusLabel;
        private ProgressBar _progressBar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _titleLabel = new Label();
            _statusLabel = new Label();
            _progressBar = new ProgressBar();
            SuspendLayout();
            // 
            // _titleLabel
            // 
            _titleLabel.AutoSize = false;
            _titleLabel.Dock = DockStyle.Top;
            _titleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _titleLabel.Location = new Point(18, 18);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new Size(384, 24);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "Preparing";
            _titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _statusLabel
            // 
            _statusLabel.AutoSize = false;
            _statusLabel.Dock = DockStyle.Top;
            _statusLabel.Location = new Point(18, 42);
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Size = new Size(384, 28);
            _statusLabel.TabIndex = 1;
            _statusLabel.Text = "Please wait...";
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _progressBar
            // 
            _progressBar.Dock = DockStyle.Top;
            _progressBar.Location = new Point(18, 70);
            _progressBar.Name = "_progressBar";
            _progressBar.Size = new Size(384, 18);
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.TabIndex = 2;
            // 
            // frmOperationProgress
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 112);
            ControlBox = false;
            Controls.Add(_progressBar);
            Controls.Add(_statusLabel);
            Controls.Add(_titleLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmOperationProgress";
            Padding = new Padding(18);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Working";
            ResumeLayout(false);
        }
    }
}
