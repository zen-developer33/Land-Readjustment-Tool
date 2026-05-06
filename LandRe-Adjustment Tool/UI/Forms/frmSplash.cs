using System.ComponentModel;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace Land_Readjustment_Tool.UI.Forms
{
    internal sealed class frmSplash : Form
    {
        private const int ProgressIntervalMs = 10;
        private const int TotalProgressTicks = 80;
        private const int CloseHoldTicks = 1;
        private static readonly Size SplashSize = new(768, 432);

        private readonly Timer _progressTimer;
        private readonly Panel _canvas;
        private readonly ParcelProgressBar _progressBar;
        private readonly Label _lblLoading;
        private int _progressTick;

        public frmSplash()
        {
            SuspendLayout();

            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(170, 184, 179);
            ClientSize = SplashSize;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(frmSplash);
            Padding = new Padding(1);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;

            _canvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 237, 228),
                BackgroundImage = LoadSplashImage(),
                BackgroundImageLayout = ImageLayout.Stretch
            };

            var accentBar = new Panel
            {
                BackColor = Color.FromArgb(108, 135, 126),
                Location = new Point(407, 102),
                Size = new Size(146, 3)
            };

            var lblTitle = new Label
            {
                BackColor = Color.Transparent,
                Font = new Font("Georgia", 31F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(45, 63, 60),
                Location = new Point(188, 116),
                Size = new Size(584, 64),
                Text = "RePlot",
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblSubtitle = new Label
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(92, 102, 92),
                Location = new Point(192, 184),
                Size = new Size(576, 36),
                Text = "A Professional Land Readjustment WorkSpace",
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblLoading = new Label
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(58, 92, 88),
                Location = new Point(330, 282),
                Size = new Size(300, 28),
                Text = "Starting...",
                TextAlign = ContentAlignment.MiddleCenter
            };

            _progressBar = new ParcelProgressBar
            {
                BackColor = Color.Transparent,
                Location = new Point(270, 326),
                Size = new Size(420, 15)
            };

            _canvas.Controls.Add(accentBar);
            _canvas.Controls.Add(lblTitle);
            _canvas.Controls.Add(lblSubtitle);
            _canvas.Controls.Add(_lblLoading);
            _canvas.Controls.Add(_progressBar);
            Controls.Add(_canvas);

            _progressTimer = new Timer { Interval = ProgressIntervalMs };
            _progressTimer.Tick += ProgressTimer_Tick;

            Shown += SplashForm_Shown;

            ResumeLayout(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shown -= SplashForm_Shown;

                _progressTimer.Tick -= ProgressTimer_Tick;
                _progressTimer.Dispose();

                _canvas.BackgroundImage?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SplashForm_Shown(object? sender, EventArgs e)
        {
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            _progressTick++;

            float progress = Math.Min(1f, _progressTick / (float)TotalProgressTicks);
            _progressBar.Progress = progress;

            if (_progressTick >= TotalProgressTicks + CloseHoldTicks)
            {
                _progressTimer.Stop();
                Close();
            }
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

        private void frmSplash_Load(object sender, EventArgs e)
        {

        }
    }

    internal sealed class ParcelProgressBar : Control
    {
        private static readonly Color[] SegmentColors =
        {
            Color.FromArgb(136, 168, 148),
            Color.FromArgb(202, 166, 142),
            Color.FromArgb(214, 191, 141),
            Color.FromArgb(146, 176, 158)
        };

        private float _progress;

        public ParcelProgressBar()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Progress
        {
            get => _progress;
            set
            {
                float clamped = Clamp01(value);
                if (Math.Abs(_progress - clamped) < 0.001f)
                {
                    return;
                }

                _progress = clamped;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (Width <= 1 || Height <= 1)
            {
                return;
            }

            RectangleF outerBounds = new(0.5f, 0.5f, Width - 1f, Height - 1f);
            float trackInsetX = MathF.Min(18f, MathF.Max(12f, Width * 0.04f));
            float trackInsetY = MathF.Min(8f, MathF.Max(4f, Height * 0.22f));
            RectangleF trackBounds = new(
                trackInsetX,
                trackInsetY,
                Width - (trackInsetX * 2f),
                Height - (trackInsetY * 2f));

            float shellRadius = MathF.Min(18f, outerBounds.Height / 2f);
            using GraphicsPath shellPath = CreateRoundedRectangle(outerBounds, shellRadius);
            using SolidBrush shellBrush = new(Color.FromArgb(122, 248, 244, 236));
            using Pen shellBorder = new(Color.FromArgb(155, 125, 144, 138), 1.4f);
            using GraphicsPath trackPath = CreateRoundedRectangle(trackBounds, trackBounds.Height / 2f);
            using SolidBrush trackBrush = new(Color.FromArgb(80, 255, 252, 246));
            using Pen trackBorder = new(Color.FromArgb(70, 150, 165, 159), 1f);

            e.Graphics.FillPath(shellBrush, shellPath);
            e.Graphics.DrawPath(shellBorder, shellPath);
            e.Graphics.FillPath(trackBrush, trackPath);
            e.Graphics.DrawPath(trackBorder, trackPath);

            float segmentGap = MathF.Min(10f, MathF.Max(5f, trackBounds.Width * 0.02f));
            float segmentWidth = (trackBounds.Width - (segmentGap * (SegmentColors.Length - 1))) / SegmentColors.Length;
            float segmentInsetY = MathF.Max(1.5f, trackBounds.Height * 0.14f);
            float segmentHeight = trackBounds.Height - (segmentInsetY * 2f);
            float segmentY = trackBounds.Y + segmentInsetY;

            for (int i = 0; i < SegmentColors.Length; i++)
            {
                RectangleF segmentBounds = new(
                    trackBounds.X + (i * (segmentWidth + segmentGap)),
                    segmentY,
                    segmentWidth,
                    segmentHeight);

                DrawSegment(e.Graphics, segmentBounds, i, SegmentColors[i]);
            }
        }

        private void DrawSegment(Graphics graphics, RectangleF segmentBounds, int segmentIndex, Color activeColor)
        {
            float segmentRadius = MathF.Min(10f, segmentBounds.Height / 2f);
            using GraphicsPath segmentPath = CreateRoundedRectangle(segmentBounds, segmentRadius);
            using LinearGradientBrush baseBrush = new(
                segmentBounds,
                Color.FromArgb(34, activeColor),
                Color.FromArgb(20, activeColor),
                LinearGradientMode.Vertical);
            using Pen outlinePen = new(Color.FromArgb(82, 133, 149, 142), 1f);

            graphics.FillPath(baseBrush, segmentPath);

            float segmentProgress = Clamp01((Progress * SegmentColors.Length) - segmentIndex);
            float fillWidth = segmentBounds.Width * segmentProgress;
            if (fillWidth > 0.5f)
            {
                GraphicsState state = graphics.Save();
                graphics.SetClip(segmentPath);

                RectangleF fillRect = new(segmentBounds.X, segmentBounds.Y, fillWidth, segmentBounds.Height);
                using LinearGradientBrush fillBrush = new(
                    fillRect,
                    Color.FromArgb(222, activeColor),
                    Color.FromArgb(186, activeColor),
                    LinearGradientMode.Vertical);

                graphics.FillRectangle(fillBrush, fillRect);
                graphics.Restore(state);
            }

            graphics.DrawPath(outlinePen, segmentPath);
        }

        private static GraphicsPath CreateRoundedRectangle(RectangleF bounds, float radius)
        {
            GraphicsPath path = new();
            float diameter = radius * 2f;
            RectangleF arc = new(bounds.X, bounds.Y, diameter, diameter);

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }
    }
}
