using Land_Readjustment_Tool.UI.CustomControls;
using System.Drawing;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Forms
{
    public class frmReplotWorkspace : Form
    {
        public MapCanvasControl CanvasControl { get; }

        public frmReplotWorkspace()
        {
            Text = "Replot Workspace";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 600);

            CanvasControl = new MapCanvasControl
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(CanvasControl);
        }
    }
}
