namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed class frmOperationProgress : Form
    {
        private readonly Label _titleLabel;
        private readonly Label _statusLabel;
        private readonly ProgressBar _progressBar;

        public frmOperationProgress()
        {
            Text = "Working";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(420, 112);
            Padding = new Padding(18);

            _titleLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font(Font, FontStyle.Bold),
                Text = "Preparing",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _statusLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Please wait...",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 18,
                Minimum = 0,
                Maximum = 100,
                Style = ProgressBarStyle.Continuous
            };

            Controls.Add(_progressBar);
            Controls.Add(_statusLabel);
            Controls.Add(_titleLabel);
        }

        public void UpdateProgress(
            string title,
            string status,
            int percent)
        {
            int clampedPercent = Math.Clamp(percent, 0, 100);

            Text = title;
            _titleLabel.Text = title;
            _statusLabel.Text = status;
            _progressBar.Value = clampedPercent;
            Refresh();
        }
    }
}
