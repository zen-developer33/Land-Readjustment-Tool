namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmOperationProgress : Form
    {
        public frmOperationProgress()
        {
            InitializeComponent();
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
