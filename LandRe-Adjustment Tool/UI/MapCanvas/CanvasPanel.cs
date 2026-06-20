using System.Windows.Forms;
using SkiaSharp.Views.Desktop;

namespace Land_Readjustment_Tool.UI.MapCanvas
{
    public class CanvasPanel : SKGLControl
    {
        public CanvasPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw,
                true);

            ResizeRedraw = true;
            UpdateStyles();
        }

        public event EventHandler<SKPaintGLSurfaceEventArgs>? GpuPaintSurface;

        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            GpuPaintSurface?.Invoke(this, e);
        }
    }
}
