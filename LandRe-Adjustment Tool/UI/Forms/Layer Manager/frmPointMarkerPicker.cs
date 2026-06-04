using System.ComponentModel;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmPointMarkerPicker : Form
    {
        private readonly Color _markerColor;

        public string SelectedMarkerKey { get; private set; }

        public frmPointMarkerPicker(string? selectedMarkerKey, Color markerColor)
        {
            _markerColor = markerColor;
            SelectedMarkerKey = PointMarkerRenderer.Normalize(selectedMarkerKey);

            InitializeComponent();
            BuildMarkerButtons();
        }

        private void BuildMarkerButtons()
        {
            foreach (PointMarkerDefinition marker in PointMarkerRenderer.GetMarkers())
            {
                MarkerOptionControl option = new(marker, _markerColor)
                {
                    IsSelected = string.Equals(
                        marker.Key,
                        SelectedMarkerKey,
                        StringComparison.OrdinalIgnoreCase)
                };

                option.Click += (_, _) => SelectMarker(option);
                option.DoubleClick += (_, _) =>
                {
                    SelectMarker(option);
                    DialogResult = DialogResult.OK;
                    Close();
                };

                _markerLayout.Controls.Add(option);
            }
        }

        private void SelectMarker(MarkerOptionControl selectedOption)
        {
            SelectedMarkerKey = selectedOption.Marker.Key;
            foreach (Control control in _markerLayout.Controls)
            {
                if (control is MarkerOptionControl option)
                {
                    option.IsSelected = ReferenceEquals(option, selectedOption);
                }
            }
        }

        private sealed class MarkerOptionControl : Control
        {
            private readonly Color _markerColor;
            private bool _isSelected;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public PointMarkerDefinition Marker { get; }

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

            public MarkerOptionControl(PointMarkerDefinition marker, Color markerColor)
            {
                Marker = marker;
                _markerColor = markerColor;
                Width = 116;
                Height = 92;
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

                RectangleF previewRect = new(Width / 2f - 18f, 12f, 36f, 36f);
                PointMarkerRenderer.Draw(
                    e.Graphics,
                    previewRect,
                    Marker.Key,
                    _markerColor,
                    1.8f);

                Rectangle textRect = new(8, 58, Width - 16, 24);
                TextRenderer.DrawText(
                    e.Graphics,
                    Marker.Name,
                    Font,
                    textRect,
                    Color.FromArgb(31, 41, 55),
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis);

                using Pen borderPen = new(borderColor, IsSelected ? 2f : 1f);
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }
        }
    }
}
