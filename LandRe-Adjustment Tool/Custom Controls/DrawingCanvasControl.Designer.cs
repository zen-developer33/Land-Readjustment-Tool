namespace Land_Readjustment_Tool.CustomControls
{
    partial class DrawingCanvasControl
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DrawingCanvasControl));
            toolStripContainer1 = new ToolStripContainer();
            panelCanvas = new Land_Readjustment_Tool.DrawingCanvas.CanvasPanel();
            toolStrip1 = new ToolStrip();
            toolLine = new ToolStripButton();
            toolPolyline = new ToolStripButton();
            toolPolygon = new ToolStripButton();
            toolRectangle = new ToolStripButton();
            toolCircle = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStrip2 = new ToolStrip();
            btnPan = new ToolStripButton();
            toolStripButton6 = new ToolStripButton();
            toolStripButton7 = new ToolStripButton();
            toolStripButton8 = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnZoom = new ToolStripButton();
            btnZoomOut = new ToolStripButton();
            toolStripButton5 = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripLabel1 = new ToolStripLabel();
            cbTheme = new ToolStripComboBox();
            btnLoadShapes = new ToolStripButton();
            btnShowHideGrid = new ToolStripButton();
            toolSnap = new ToolStripButton();
            toolStripButton1 = new ToolStripButton();
            btnShowDebugLog = new ToolStripButton();
            btnCollapseLeftPanel = new ToolStripButton();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.LeftToolStripPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            toolStrip1.SuspendLayout();
            toolStrip2.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripContainer1
            // 
            toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Controls.Add(panelCanvas);
            toolStripContainer1.ContentPanel.Size = new Size(1358, 375);
            toolStripContainer1.Dock = DockStyle.Fill;
            // 
            // toolStripContainer1.LeftToolStripPanel
            // 
            toolStripContainer1.LeftToolStripPanel.Controls.Add(toolStrip1);
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(1388, 403);
            toolStripContainer1.TabIndex = 1;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip2);
            // 
            // panelCanvas
            // 
            panelCanvas.BackColor = Color.FromArgb(34, 41, 51);
            panelCanvas.Cursor = Cursors.Cross;
            panelCanvas.Dock = DockStyle.Fill;
            panelCanvas.Location = new Point(0, 0);
            panelCanvas.Name = "panelCanvas";
            panelCanvas.Size = new Size(1358, 375);
            panelCanvas.TabIndex = 0;
            panelCanvas.Paint += panelCanvas_Paint;
            panelCanvas.MouseDown += PanelCanvas_MouseDown;
            panelCanvas.MouseEnter += PanelCanvas_MouseEnter;
            panelCanvas.MouseMove += PanelCanvas_MouseMove;
            panelCanvas.MouseUp += PanelCanvas_MouseUp;
            // 
            // toolStrip1
            // 
            toolStrip1.AllowItemReorder = true;
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolLine, toolPolyline, toolPolygon, toolRectangle, toolCircle, toolStripSeparator1, toolStripSeparator4 });
            toolStrip1.Location = new Point(0, 4);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(30, 139);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolLine
            // 
            toolLine.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolLine.Image = Properties.Resources.icons8_line_24;
            toolLine.ImageTransparentColor = Color.Magenta;
            toolLine.Name = "toolLine";
            toolLine.Size = new Size(28, 24);
            toolLine.Text = "Line";
            toolLine.Click += toolLine_Click;
            // 
            // toolPolyline
            // 
            toolPolyline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolPolyline.Image = Properties.Resources.icons8_polyline_24;
            toolPolyline.ImageTransparentColor = Color.Magenta;
            toolPolyline.Name = "toolPolyline";
            toolPolyline.Size = new Size(28, 24);
            toolPolyline.Text = "Polyline";
            toolPolyline.Click += toolPolyline_Click;
            // 
            // toolPolygon
            // 
            toolPolygon.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolPolygon.Image = Properties.Resources.icons8_polygon_24__1_;
            toolPolygon.ImageTransparentColor = Color.Magenta;
            toolPolygon.Name = "toolPolygon";
            toolPolygon.Size = new Size(28, 24);
            toolPolygon.Text = "Polygon";
            toolPolygon.Click += toolPolygon_Click;
            // 
            // toolRectangle
            // 
            toolRectangle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolRectangle.ImageTransparentColor = Color.Magenta;
            toolRectangle.Name = "toolRectangle";
            toolRectangle.Size = new Size(28, 4);
            toolRectangle.Text = "Rectangle";
            toolRectangle.Click += toolRectangle_Click;
            // 
            // toolCircle
            // 
            toolCircle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolCircle.Image = Properties.Resources.icons8_radius_24;
            toolCircle.ImageTransparentColor = Color.Magenta;
            toolCircle.Name = "toolCircle";
            toolCircle.Size = new Size(28, 24);
            toolCircle.Text = "Circle";
            toolCircle.Click += toolCircle_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(28, 6);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(28, 6);
            // 
            // toolStrip2
            // 
            toolStrip2.AllowItemReorder = true;
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { btnCollapseLeftPanel, btnPan, toolStripButton6, toolStripButton7, toolStripButton8, toolStripSeparator2, btnZoom, btnZoomOut, toolStripButton5, toolStripSeparator3, toolStripLabel1, cbTheme, btnLoadShapes, btnShowHideGrid, toolSnap, toolStripButton1, btnShowDebugLog });
            toolStrip2.Location = new Point(4, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(1033, 28);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // btnPan
            // 
            btnPan.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnPan.ImageTransparentColor = Color.Magenta;
            btnPan.Name = "btnPan";
            btnPan.Size = new Size(36, 25);
            btnPan.Text = "Pan";
            btnPan.Click += btnPan_Click;
            // 
            // toolStripButton6
            // 
            toolStripButton6.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton6.ImageTransparentColor = Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new Size(49, 25);
            toolStripButton6.Text = "Undo";
            toolStripButton6.Click += btnUndo_Click;
            // 
            // toolStripButton7
            // 
            toolStripButton7.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton7.ImageTransparentColor = Color.Magenta;
            toolStripButton7.Name = "toolStripButton7";
            toolStripButton7.Size = new Size(48, 25);
            toolStripButton7.Text = "Redo";
            toolStripButton7.Click += btnRedo_Click;
            // 
            // toolStripButton8
            // 
            toolStripButton8.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton8.ImageTransparentColor = Color.Magenta;
            toolStripButton8.Name = "toolStripButton8";
            toolStripButton8.Size = new Size(47, 25);
            toolStripButton8.Text = "Clear";
            toolStripButton8.Click += btnClear_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 28);
            // 
            // btnZoom
            // 
            btnZoom.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnZoom.Image = Properties.Resources.icons8_zoom_in_24__1_;
            btnZoom.ImageTransparentColor = Color.Magenta;
            btnZoom.Name = "btnZoom";
            btnZoom.Size = new Size(29, 25);
            btnZoom.Text = "Zoom In";
            btnZoom.Click += btnZoomIn_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnZoomOut.Image = Properties.Resources.icons8_zoom_out_24__1_;
            btnZoomOut.ImageTransparentColor = Color.Magenta;
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(29, 25);
            btnZoomOut.Text = "Zoom Out";
            btnZoomOut.Click += btnZoomOut_Click;
            // 
            // toolStripButton5
            // 
            toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton5.Image = Properties.Resources.icons8_zoom_to_extents_24;
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(29, 25);
            toolStripButton5.Text = "Zoom Extents";
            toolStripButton5.Click += btnZoomExtents_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 28);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(54, 25);
            toolStripLabel1.Text = "Theme";
            // 
            // cbTheme
            // 
            cbTheme.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbTheme.FlatStyle = FlatStyle.Standard;
            cbTheme.Items.AddRange(new object[] { "Dark", "Light" });
            cbTheme.Name = "cbTheme";
            cbTheme.Size = new Size(118, 28);
            cbTheme.Tag = "1";
            cbTheme.Text = "Dark";
            cbTheme.SelectedIndexChanged += cbTheme_SelectedIndexChanged;
            // 
            // btnLoadShapes
            // 
            btnLoadShapes.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLoadShapes.ImageTransparentColor = Color.Magenta;
            btnLoadShapes.Name = "btnLoadShapes";
            btnLoadShapes.Size = new Size(95, 25);
            btnLoadShapes.Text = "Load shapes";
            btnLoadShapes.Click += btnLoadShapes_Click;
            // 
            // btnShowHideGrid
            // 
            btnShowHideGrid.CheckOnClick = true;
            btnShowHideGrid.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnShowHideGrid.ImageTransparentColor = Color.Magenta;
            btnShowHideGrid.Name = "btnShowHideGrid";
            btnShowHideGrid.Size = new Size(119, 25);
            btnShowHideGrid.Text = "Show/Hide Grid";
            btnShowHideGrid.Click += btnShowHideGrid_Click;
            // 
            // toolSnap
            // 
            toolSnap.CheckOnClick = true;
            toolSnap.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolSnap.ImageTransparentColor = Color.Magenta;
            toolSnap.Name = "toolSnap";
            toolSnap.Size = new Size(94, 25);
            toolSnap.Text = "Object Snap";
            toolSnap.Click += toolSnap_Click;
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(58, 25);
            toolStripButton1.Text = "Import";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // btnShowDebugLog
            // 
            btnShowDebugLog.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnShowDebugLog.ImageTransparentColor = Color.Magenta;
            btnShowDebugLog.Name = "btnShowDebugLog";
            btnShowDebugLog.Size = new Size(133, 25);
            btnShowDebugLog.Text = "Show Debug Logs";
            btnShowDebugLog.Click += btnShowDebugLog_Click;
            // 
            // btnCollapseLeftPanel
            // 
            btnCollapseLeftPanel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnCollapseLeftPanel.Image = (Image)resources.GetObject("btnCollapseLeftPanel.Image");
            btnCollapseLeftPanel.ImageTransparentColor = Color.Magenta;
            btnCollapseLeftPanel.Name = "btnCollapseLeftPanel";
            btnCollapseLeftPanel.Size = new Size(29, 25);
            btnCollapseLeftPanel.Text = "toolStripButton2";
            // 
            // DrawingCanvasControl
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(toolStripContainer1);
            DoubleBuffered = true;
            Name = "DrawingCanvasControl";
            Size = new Size(1388, 403);
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.LeftToolStripPanel.ResumeLayout(false);
            toolStripContainer1.LeftToolStripPanel.PerformLayout();
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // ── Control field declarations ────────────────────────────────────────
        private ToolStripContainer toolStripContainer1;
        private Land_Readjustment_Tool.DrawingCanvas.CanvasPanel panelCanvas;
        private ToolStrip toolStrip1;
        private ToolStripButton toolLine;
        private ToolStripButton toolPolyline;
        private ToolStripButton toolPolygon;
        private ToolStripButton toolRectangle;
        private ToolStripButton toolCircle;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStrip toolStrip2;
        private ToolStripButton btnPan;
        private ToolStripButton toolStripButton6;
        private ToolStripButton toolStripButton7;
        private ToolStripButton toolStripButton8;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnZoom;
        private ToolStripButton btnZoomOut;
        private ToolStripButton toolStripButton5;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox cbTheme;
        private ToolStripButton btnLoadShapes;
        private ToolStripButton btnShowHideGrid;
        private ToolStripButton toolSnap;
        private ToolStripButton toolStripButton1;
        private ToolStripButton btnShowDebugLog;
        private ToolStripButton btnCollapseLeftPanel;
    }
}
