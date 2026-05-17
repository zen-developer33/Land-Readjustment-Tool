namespace Land_Readjustment_Tool.UI.CustomControls
{
    partial class MapCanvasControl
    {
        private System.ComponentModel.IContainer components = null;
        private Land_Readjustment_Tool.UI.MapCanvas.CanvasPanel canvasSurface;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                StopAndDisposeZoomingStatusTimer();
                DisposeRasterRenderLayers();
                _rasterDeferredRenderer?.Dispose();
                _renderer?.Dispose();
                _activeTextEditor?.Dispose();
                _panCursor?.Dispose();
                _debugOverlayFont?.Dispose();
                _compositePanBitmap?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            canvasSurface = new Land_Readjustment_Tool.UI.MapCanvas.CanvasPanel();
            SuspendLayout();
            // 
            // canvasSurface
            // 
            canvasSurface.BackColor = Color.White;
            canvasSurface.Dock = DockStyle.Fill;
            canvasSurface.Location = new Point(0, 0);
            canvasSurface.Name = "canvasSurface";
            canvasSurface.Size = new Size(800, 500);
            canvasSurface.TabIndex = 0;
            canvasSurface.Paint += canvasSurface_Paint;
            canvasSurface.Resize += canvasSurface_Resize;
            // 
            // MapCanvasControl
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            Controls.Add(canvasSurface);
            Name = "MapCanvasControl";
            Size = new Size(800, 500);
            ResumeLayout(false);
        }

        #endregion
    }
}
