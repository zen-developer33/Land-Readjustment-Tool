namespace Drawing_Canvas_Practice
{
    partial class frmDrawingCanvas
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }



        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDrawingCanvas));
            toolStripContainer1 = new ToolStripContainer();
            panelCanvas = new CanvasPanel();  // ⭐ FIXED: Changed from Panel to CanvasPanel
            toolStrip1 = new ToolStrip();
            toolLine = new ToolStripButton();
            toolRectangle = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripLabel2 = new ToolStripLabel();
            cbDrawingTool = new ToolStripComboBox();
            btnLoadShapes = new ToolStripButton();
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
            toolStripContainer1.ContentPanel.SuspendLayout();
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
            toolStripContainer1.ContentPanel.Size = new Size(1272, 574);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(1272, 630);
            toolStripContainer1.TabIndex = 1;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip2);
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            // 
            // panelCanvas - ⭐ CRITICAL FIX: Added ALL events
            // 
            panelCanvas.BackColor = Color.FromArgb(34, 41, 51);
            panelCanvas.Cursor = Cursors.Cross;
            panelCanvas.Dock = DockStyle.Fill;
            panelCanvas.Location = new Point(0, 0);
            panelCanvas.Name = "panelCanvas";
            panelCanvas.Size = new Size(1272, 574);
            panelCanvas.TabIndex = 0;
            panelCanvas.Paint += panelCanvas_Paint;              // ⭐ RENDERS EVERYTHING
            panelCanvas.MouseClick += panelCanvas_MouseClick;    // ⭐ CLICK EVENTS
            panelCanvas.MouseDown += panelCanvas_MouseDown;      // ⭐ DRAWING START
            panelCanvas.MouseEnter += panelCanvas_MouseEnter;    // ⭐ FOCUS
            panelCanvas.MouseLeave += panelCanvas_MouseLeave;    // ⭐ LEAVE
            panelCanvas.MouseMove += panelCanvas_MouseMove;      // ⭐ PREVIEW & COORDS
            panelCanvas.MouseUp += panelCanvas_MouseUp;          // ⭐ DRAWING END
            // 
            // toolStrip1
            // 
            toolStrip1.AllowItemReorder = true;
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolLine, toolRectangle, toolStripButton2, toolStripButton3, toolStripSeparator1, toolStripLabel2, cbDrawingTool, btnLoadShapes });
            toolStrip1.Location = new Point(4, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(561, 28);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolLine
            // 
            toolLine.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolLine.ImageTransparentColor = Color.Magenta;
            toolLine.Name = "toolLine";
            toolLine.Size = new Size(40, 25);
            toolLine.Text = "Line";
            toolLine.Click += toolLine_Click;
            // 
            // toolRectangle
            // 
            toolRectangle.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolRectangle.ImageTransparentColor = Color.Magenta;
            toolRectangle.Name = "toolRectangle";
            toolRectangle.Size = new Size(79, 25);
            toolRectangle.Text = "Rectangle";
            toolRectangle.Click += toolRectangle_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(50, 25);
            toolStripButton2.Text = "Circle";
            toolStripButton2.Click += toolCircle_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(56, 25);
            toolStripButton3.Text = "Ellipse";
            toolStripButton3.Click += toolEllipse_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 28);
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(97, 25);
            toolStripLabel2.Text = "DrawingTool:";
            // 
            // cbDrawingTool
            // 
            cbDrawingTool.Items.AddRange(new object[] { "Line", "Rectangle", "Ellipse", "Circle" });
            cbDrawingTool.Name = "cbDrawingTool";
            cbDrawingTool.Size = new Size(121, 28);
            cbDrawingTool.SelectedIndexChanged += cbDrawingTool_SelectedIndexChanged;
            // 
            // btnLoadShapes
            // 
            btnLoadShapes.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLoadShapes.Image = (Image)resources.GetObject("btnLoadShapes.Image");
            btnLoadShapes.ImageTransparentColor = Color.Magenta;
            btnLoadShapes.Name = "btnLoadShapes";
            btnLoadShapes.Size = new Size(97, 25);
            btnLoadShapes.Text = "Load Shapes";
            btnLoadShapes.Click += btnLoadShapes_Click;
            // 
            // toolStrip2
            // 
            toolStrip2.AllowItemReorder = true;
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { btnPan, toolStripButton6, toolStripButton7, toolStripButton8, toolStripSeparator2, btnZoom, btnZoomOut, toolStripButton5, toolStripSeparator3, toolStripLabel1, cbTheme });
            toolStrip2.Location = new Point(4, 28);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(469, 28);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // btnPan
            // 
            btnPan.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnPan.Image = (Image)resources.GetObject("btnPan.Image");
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
            toolStripButton7.Click += btnRedo;
            // 
            // toolStripButton8
            // 
            toolStripButton8.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton8.ImageTransparentColor = Color.Magenta;
            toolStripButton8.Name = "toolStripButton8";
            toolStripButton8.Size = new Size(47, 25);
            toolStripButton8.Text = "Clear";
            toolStripButton8.Click += buttonRefresh_Click;
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
            btnZoom.Click += btnZoomIn;
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
            toolStripButton5.Text = "toolStripButton5";
            toolStripButton5.Click += toolStripButton5_Click;
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
            cbTheme.Size = new Size(121, 28);
            cbTheme.Tag = "1";
            cbTheme.Text = "Dark";
            cbTheme.SelectedIndexChanged += cbTheme_SelectedIndexChanged;
            cbTheme.Click += cbTheme_Click;
            // 
            // frmDrawingCanvas
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1272, 630);
            Controls.Add(toolStripContainer1);
            DoubleBuffered = true;
            Name = "frmDrawingCanvas";
            Text = "Replot WorkSpace";
            Load += frmDrawingCanvas_Load;
            toolStripContainer1.ContentPanel.ResumeLayout(false);
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
        private ToolStripContainer toolStripContainer1;
        private ToolStrip toolStrip1;
        private ToolStripButton toolRectangle;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolLine;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripButton6;
        private ToolStripButton toolStripButton7;
        private ToolStripButton toolStripButton8;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox cbTheme;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel toolStripLabel2;
        private ToolStripComboBox cbDrawingTool;
        private ToolStripButton btnPan;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnZoom;
        private ToolStripButton btnZoomOut;
        private ToolStripButton toolStripButton5;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton btnLoadShapes;
        private CanvasPanel panelCanvas;  // ⭐ FIXED: Changed from Panel
    }
}