using System.Windows.Forms;
using SkiaSharp.Views.Desktop;

namespace Land_Readjustment_Tool.UI.MapCanvas
{
    public class SkiaCpuCanvasPanel : SKControl
    {
        public SkiaCpuCanvasPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw,
                true);

            TabStop = true;
            ResizeRedraw = true;
            UpdateStyles();
        }
    }
}
