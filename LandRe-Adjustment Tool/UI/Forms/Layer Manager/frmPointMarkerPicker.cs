using System.ComponentModel;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed class frmPointMarkerPicker : Form
    {
        private readonly Color _markerColor;
        private readonly FlowLayoutPanel _markerLayout = new();
        private readonly Button _okButton = new();
        private readonly Button _cancelButton = new();

        public string SelectedMarkerKey { get; private set; }

        public frmPointMarkerPicker(string? selectedMarkerKey, Color markerColor)
        {
            _markerColor = markerColor;
            SelectedMarkerKey = PointMarkerRenderer.Normalize(selectedMarkerKey);

            InitializePicker();
            BuildMarkerButtons();
        }

        private void InitializePicker()
        {
            Text = "Choose Point Marker";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(520, 360);
            Font = new Font("Segoe UI", 9F);

            _markerLayout.Dock = DockStyle.Fill;
            _markerLayout.AutoScroll = true;
            _markerLayout.Padding = new Padding(10);
            _markerLayout.FlowDirection = FlowDirection.LeftToRight;
            _markerLayout.WrapContents = true;

            FlowLayoutPanel buttonPanel = new()
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(10),
                BackColor = SystemColors.Control
            };

            _okButton.Text = "OK";
            _okButton.DialogResult = DialogResult.OK;
            _okButton.Width = 84;

            _cancelButton.Text = "Cancel";
            _cancelButton.DialogResult = DialogResult.Cancel;
            _cancelButton.Width = 84;

            buttonPanel.Controls.Add(_okButton);
            buttonPanel.Controls.Add(_cancelButton);

            Controls.Add(_markerLayout);
            Controls.Add(buttonPanel);
            AcceptButton = _okButton;
            CancelButton = _cancelButton;
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
