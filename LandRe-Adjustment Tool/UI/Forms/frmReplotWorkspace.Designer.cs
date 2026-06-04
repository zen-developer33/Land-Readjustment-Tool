using Land_Readjustment_Tool.UI.CustomControls;

namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmReplotWorkspace
    {
        private System.ComponentModel.IContainer components = null;
        private MapCanvasControl mapCanvasControl;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            mapCanvasControl = new MapCanvasControl();
            SuspendLayout();
            // 
            // mapCanvasControl
            // 
            mapCanvasControl.Dock = DockStyle.Fill;
            mapCanvasControl.Location = new Point(0, 0);
            mapCanvasControl.Name = "mapCanvasControl";
            mapCanvasControl.Size = new Size(900, 600);
            mapCanvasControl.TabIndex = 0;
            // 
            // frmReplotWorkspace
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 600);
            Controls.Add(mapCanvasControl);
            MinimumSize = new Size(900, 600);
            Name = "frmReplotWorkspace";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Replot Workspace";
            WindowState = FormWindowState.Maximized;
            ResumeLayout(false);
        }
    }
}
