using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace Land_Readjustment_Tool.UI.Forms
{
    internal sealed class frmSplash : Form
    {
        private const int SplashDurationMs = 2500;
        private static readonly Size SplashSize = new(960, 540);

        private readonly Timer _closeTimer;
        private readonly Panel _canvas;
        private readonly Label _lblLoading;

        public frmSplash()
        {
            SuspendLayout();

            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(232, 236, 232);
            ClientSize = SplashSize;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(frmSplash);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;

            _canvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackgroundImage = LoadSplashImage(),
                BackgroundImageLayout = ImageLayout.Stretch
            };

            var accentBar = new Panel
            {
                BackColor = Color.FromArgb(108, 135, 126),
                Location = new Point(420, 106),
                Size = new Size(120, 3)
            };

            var lblTitle = new Label
            {
                BackColor = Color.Transparent,
                Font = new Font("Georgia", 34F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(45, 63, 60),
                Location = new Point(210, 122),
                Size = new Size(540, 70),
                Text = "RePlot",
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblSubtitle = new Label
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(92, 102, 92),
                Location = new Point(180, 202),
                Size = new Size(600, 42),
                Text = "A Professional Land Readjustment WorkSpace",
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblLoading = new Label
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(58, 92, 88),
                Location = new Point(330, 452),
                Size = new Size(300, 30),
                Text = "Starting.......",
                TextAlign = ContentAlignment.MiddleCenter
            };

            var loadingAccent = new Panel
            {
                BackColor = Color.FromArgb(198, 208, 197),
                Location = new Point(410, 440),
                Size = new Size(140, 2)
            };

            _canvas.Controls.Add(accentBar);
            _canvas.Controls.Add(lblTitle);
            _canvas.Controls.Add(lblSubtitle);
            _canvas.Controls.Add(loadingAccent);
            _canvas.Controls.Add(_lblLoading);
            Controls.Add(_canvas);

            _closeTimer = new Timer { Interval = SplashDurationMs };
            _closeTimer.Tick += CloseTimer_Tick;

            Shown += SplashForm_Shown;

            ResumeLayout(false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using var borderPen = new Pen(Color.FromArgb(170, 184, 179));
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shown -= SplashForm_Shown;

                _closeTimer.Tick -= CloseTimer_Tick;
                _closeTimer.Dispose();

                _canvas.BackgroundImage?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SplashForm_Shown(object? sender, EventArgs e)
        {
            _closeTimer.Start();
        }

        private void CloseTimer_Tick(object? sender, EventArgs e)
        {
            _closeTimer.Stop();
            Close();
        }

        private static Image? LoadSplashImage()
        {
            string imagePath = Path.Combine(AppContext.BaseDirectory, "Resources", "replot-splash-minimal-v2.png");
            if (!File.Exists(imagePath))
            {
                return null;
            }

            using var stream = File.OpenRead(imagePath);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }
    }
}
