using System.ComponentModel;

namespace Land_Pooling_Policy_Manager.UI.Controls
{
    /// <summary>
    /// A scrollable Panel that displays an Image with mouse-wheel zoom and
    /// left-button drag pan. Wrap a clause diagram (or any image) by setting
    /// <see cref="Image"/> — the panel manages its own child PictureBox sized
    /// to <c>image.Size × zoom</c>, with the scrollbars kicking in once the
    /// rendered image grows past the viewport.
    /// </summary>
    public sealed class ZoomPanPanel : Panel
    {
        private const float MinZoom = 0.1f;
        private const float MaxZoom = 10.0f;
        private const float WheelStep = 1.15f;

        private readonly PictureBox _picture;
        private Image? _image;
        private float _zoom = 1.0f;
        private Point _panStartCursor;
        private Point _panStartScroll;
        private bool _isPanning;

        public ZoomPanPanel()
        {
            AutoScroll = true;
            DoubleBuffered = true;
            BackColor = Color.Gainsboro;

            _picture = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                TabStop = false,
            };
            Controls.Add(_picture);

            // Forward the wheel/mouse events from the inner PictureBox up to the
            // panel — without this the wheel would only zoom while the cursor
            // was on the panel margin, not over the image itself.
            _picture.MouseWheel += (s, e) => OnMouseWheel(e);
            _picture.MouseDown += (s, e) => OnMouseDown(TranslateFromPicture(e));
            _picture.MouseMove += (s, e) => OnMouseMove(TranslateFromPicture(e));
            _picture.MouseUp += (s, e) => OnMouseUp(TranslateFromPicture(e));
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image? Image
        {
            get => _image;
            set
            {
                _image?.Dispose();
                _image = value;
                _picture.Image = value;
                _zoom = 1.0f;
                FitToViewport();
            }
        }

        public void ResetZoom()
        {
            _zoom = 1.0f;
            ApplyLayout(focusViewportCenter: true);
        }

        /// <summary>
        /// Scales <see cref="_zoom"/> so the image fits the viewport exactly,
        /// preserving aspect ratio. Called when a new image is loaded; the user
        /// can then wheel-zoom in/out from this baseline.
        /// </summary>
        public void FitToViewport()
        {
            if (_image == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                ApplyLayout(focusViewportCenter: true);
                return;
            }

            float zoomX = (float)ClientSize.Width / _image.Width;
            float zoomY = (float)ClientSize.Height / _image.Height;
            _zoom = Math.Clamp(Math.Min(zoomX, zoomY), MinZoom, MaxZoom);
            ApplyLayout(focusViewportCenter: true);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            // When the user resizes the form, keep the image centered without
            // touching the user-controlled zoom — only re-anchor.
            ApplyLayout(focusViewportCenter: false);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            // Grab focus so MouseWheel routes here. Panel by default isn't
            // selectable; we toggle that on demand without subclass plumbing.
            if (CanFocus)
                Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_image == null)
                return;

            // Zoom keeping the pixel under the cursor stationary in the viewport.
            float oldZoom = _zoom;
            float newZoom = e.Delta > 0 ? _zoom * WheelStep : _zoom / WheelStep;
            newZoom = Math.Clamp(newZoom, MinZoom, MaxZoom);
            if (Math.Abs(newZoom - oldZoom) < 0.0001f)
                return;

            // Cursor position translated into the panel's content (logical)
            // coordinate space. AutoScrollPosition.X/Y are negative offsets.
            Point cursorClient = PointToClient(Cursor.Position);
            int contentX = cursorClient.X - AutoScrollPosition.X;
            int contentY = cursorClient.Y - AutoScrollPosition.Y;

            _zoom = newZoom;
            ApplyLayout(focusViewportCenter: false);

            // After resizing the PictureBox, scroll so the same image pixel is
            // back under the cursor.
            float ratio = newZoom / oldZoom;
            int targetContentX = (int)(contentX * ratio);
            int targetContentY = (int)(contentY * ratio);
            AutoScrollPosition = new Point(
                Math.Max(0, targetContentX - cursorClient.X),
                Math.Max(0, targetContentY - cursorClient.Y));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || _image == null)
                return;

            _isPanning = true;
            _panStartCursor = Cursor.Position;
            _panStartScroll = AutoScrollPosition;
            _picture.Cursor = Cursors.Hand;
            Capture = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_isPanning)
                return;

            Point current = Cursor.Position;
            int dx = current.X - _panStartCursor.X;
            int dy = current.Y - _panStartCursor.Y;
            // AutoScrollPosition reports negative values but is assigned with
            // positive values; multiplying by -1 keeps the drag-feels-natural
            // direction (image follows the cursor).
            AutoScrollPosition = new Point(
                -_panStartScroll.X - dx,
                -_panStartScroll.Y - dy);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_isPanning)
                return;

            _isPanning = false;
            _picture.Cursor = Cursors.Default;
            Capture = false;
        }

        private void ApplyLayout(bool focusViewportCenter)
        {
            if (_image == null)
            {
                _picture.Size = ClientSize;
                _picture.Location = Point.Empty;
                return;
            }

            int w = Math.Max(1, (int)(_image.Width * _zoom));
            int h = Math.Max(1, (int)(_image.Height * _zoom));
            _picture.Size = new Size(w, h);
            AutoScrollMinSize = new Size(w, h);

            // Center the image inside the viewport when it's smaller than the
            // panel, otherwise let AutoScroll handle it.
            int x = w < ClientSize.Width ? (ClientSize.Width - w) / 2 : 0;
            int y = h < ClientSize.Height ? (ClientSize.Height - h) / 2 : 0;
            _picture.Location = new Point(x + AutoScrollPosition.X, y + AutoScrollPosition.Y);

            if (focusViewportCenter)
            {
                AutoScrollPosition = new Point(
                    Math.Max(0, (w - ClientSize.Width) / 2),
                    Math.Max(0, (h - ClientSize.Height) / 2));
            }
        }

        // Translate a MouseEventArgs received on the child PictureBox into the
        // ZoomPanPanel's own coordinate space, so OnMouse* helpers see consistent
        // values regardless of whether the cursor entered via the panel margin
        // or the image itself.
        private MouseEventArgs TranslateFromPicture(MouseEventArgs e)
        {
            return new MouseEventArgs(
                e.Button,
                e.Clicks,
                e.X + _picture.Left,
                e.Y + _picture.Top,
                e.Delta);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            // Panel must own the keyboard to receive MouseWheel reliably.
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image?.Dispose();
                _image = null;
            }
            base.Dispose(disposing);
        }
    }
}
