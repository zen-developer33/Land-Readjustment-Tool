using System.Windows.Forms;
using SkiaSharp.Views.Desktop;

namespace Land_Readjustment_Tool.UI.MapCanvas
{
    public class GpuCanvasPanel : SKGLControl
    {
        public GpuCanvasPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw,
                true);

            TabStop = true;
            ResizeRedraw = true;
            TrySetBooleanProperty("VSync", true);
            UpdateStyles();
        }

        public event EventHandler<SKPaintGLSurfaceEventArgs>? GpuPaintSurface;

        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            GpuPaintSurface?.Invoke(this, e);
        }

        private void TrySetBooleanProperty(string propertyName, bool value)
        {
            try
            {
                GetType()
                    .BaseType?
                    .GetProperty(propertyName)?
                    .SetValue(this, value);
            }
            catch
            {
                // Older SkiaSharp/OpenTK builds may not expose this property.
            }
        }
    }
}
