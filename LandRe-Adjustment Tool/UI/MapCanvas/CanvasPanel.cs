using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.MapCanvas
{
    public class CanvasPanel : Panel
    {
        public CanvasPanel()
        {
            // Enable double buffering
            this.DoubleBuffered = true;

            // Prevent background erase flicker
            this.ResizeRedraw = true;
        }
    }
}
