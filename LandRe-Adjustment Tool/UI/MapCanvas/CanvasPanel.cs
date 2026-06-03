using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.MapCanvas
{
    public class CanvasPanel : Panel
    {
        public CanvasPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            DoubleBuffered = true;
            ResizeRedraw = true;
            UpdateStyles();
        }
    }
}
