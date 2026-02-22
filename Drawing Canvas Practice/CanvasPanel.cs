using System.Windows.Forms;

namespace Drawing_Canvas_Practice
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
