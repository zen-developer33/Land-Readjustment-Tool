using System.ComponentModel;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmHatchPatternPicker : Form
    {
        private readonly IHatchPatternService _hatchPatternService;
        private readonly Color _hatchColor;
        private readonly Color _fillColor;
        private readonly int _transparency;
        private readonly double _hatchScale;

        public string SelectedPatternKey { get; private set; }

        public frmHatchPatternPicker(
            IHatchPatternService hatchPatternService,
            string? selectedPatternKey,
            Color hatchColor,
            Color fillColor,
            int transparency,
            double hatchScale)
        {
            _hatchPatternService = hatchPatternService
                ?? throw new ArgumentNullException(nameof(hatchPatternService));
            _hatchColor = hatchColor;
            _fillColor = fillColor;
            _transparency = transparency;
            _hatchScale = hatchScale;
            SelectedPatternKey = _hatchPatternService
                .GetPatternOrDefault(selectedPatternKey)
                .Key;

            InitializeComponent();
            BuildPatternButtons();
        }

        private void BuildPatternButtons()
        {
            foreach (HatchPatternDefinition pattern in _hatchPatternService.GetPatterns())
            {
                HatchPatternOptionControl option = new(
                    _hatchPatternService,
                    pattern,
                    _hatchColor,
                    _fillColor,
                    _transparency,
                    _hatchScale)
                {
                    IsSelected = string.Equals(
                        pattern.Key,
                        SelectedPatternKey,
                        StringComparison.OrdinalIgnoreCase)
                };

                option.Click += (_, _) => SelectPattern(option);
                option.DoubleClick += (_, _) =>
                {
                    SelectPattern(option);
                    DialogResult = DialogResult.OK;
                    Close();
                };

                _patternLayout.Controls.Add(option);
            }
        }

        private void SelectPattern(HatchPatternOptionControl selectedOption)
        {
            SelectedPatternKey = selectedOption.Pattern.Key;
            foreach (Control control in _patternLayout.Controls)
            {
                if (control is HatchPatternOptionControl option)
                {
                    option.IsSelected = ReferenceEquals(option, selectedOption);
                }
            }
        }

        private sealed class HatchPatternOptionControl : Control
        {
            private readonly IHatchPatternService _hatchPatternService;
            private readonly Color _hatchColor;
            private readonly Color _fillColor;
            private readonly int _transparency;
            private readonly double _hatchScale;
            private bool _isSelected;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public HatchPatternDefinition Pattern { get; }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected == value)
                        return;

                    _isSelected = value;
                    Invalidate();
                }
            }

            public HatchPatternOptionControl(
                IHatchPatternService hatchPatternService,
                HatchPatternDefinition pattern,
                Color hatchColor,
                Color fillColor,
                int transparency,
                double hatchScale)
            {
                _hatchPatternService = hatchPatternService;
                Pattern = pattern;
                _hatchColor = hatchColor;
                _fillColor = fillColor;
                _transparency = transparency;
                _hatchScale = hatchScale;

                Width = 164;
                Height = 104;
                Margin = new Padding(6);
                Cursor = Cursors.Hand;
                TabStop = true;
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Rectangle borderRect = new(0, 0, Width - 1, Height - 1);
                Color borderColor = IsSelected
                    ? Color.FromArgb(31, 111, 235)
                    : Color.FromArgb(190, 195, 203);

                using SolidBrush backgroundBrush = new(IsSelected
                    ? Color.FromArgb(235, 244, 255)
                    : Color.White);
                e.Graphics.FillRectangle(backgroundBrush, ClientRectangle);

                Rectangle previewRect = new(12, 10, Width - 24, 46);
                _hatchPatternService.DrawPreview(
                    e.Graphics,
                    previewRect,
                    Pattern.Key,
                    _hatchColor,
                    _fillColor,
                    _transparency,
                    _hatchScale,
                    Color.White);

                using Pen previewBorderPen = new(Color.FromArgb(122, 128, 138));
                e.Graphics.DrawRectangle(previewBorderPen, previewRect);

                Rectangle textRect = new(10, 62, Width - 20, 22);
                TextRenderer.DrawText(
                    e.Graphics,
                    Pattern.Name,
                    Font,
                    textRect,
                    Color.FromArgb(31, 41, 55),
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis);

                Rectangle descriptionRect = new(10, 83, Width - 20, 18);
                TextRenderer.DrawText(
                    e.Graphics,
                    Pattern.Description,
                    Font,
                    descriptionRect,
                    Color.FromArgb(91, 97, 110),
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis);

                using Pen borderPen = new(borderColor, IsSelected ? 2f : 1f);
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }
        }
    }
}
