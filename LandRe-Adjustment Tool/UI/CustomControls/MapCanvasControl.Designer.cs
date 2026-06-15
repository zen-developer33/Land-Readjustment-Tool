namespace Land_Readjustment_Tool.UI.CustomControls
{
    partial class MapCanvasControl
    {
        private System.ComponentModel.IContainer components = null;
        private Land_Readjustment_Tool.UI.MapCanvas.CanvasPanel canvasSurface;
        private ContextMenuStrip _objectSelectionContextMenu;
        private ContextMenuStrip _drawingOptionsContextMenu;
        private ContextMenuStrip _selectionOptionsContextMenu;
        private ToolStripMenuItem _mnuEditText;
        private ToolStripMenuItem _mnuClearSelection;
        private ToolStripMenuItem _mnuMoveSelectedObjects;
        private ToolStripMenuItem _mnuCopySelectedObjects;
        private ToolStripMenuItem _mnuViewEditData;
        private ToolStripMenuItem _mnuAssignData;
        private ToolStripMenuItem _mnuCreateFeaturesFromSelection;
        private ToolStripMenuItem _mnuDeleteSelectedObjects;
        private ToolStripSeparator _mnuObjectSelectionSeparator1;
        private ToolStripSeparator _mnuObjectSelectionSeparator2;

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
                _selectionCursor?.Dispose();
                _zoomInCursor?.Dispose();
                _zoomOutCursor?.Dispose();
                _zoomWindowCursor?.Dispose();
                _debugOverlayFont?.Dispose();
                _compositePanBitmap?.Dispose();
                _refreshHoldFrame?.Dispose();
                _movePreviewBitmap?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            canvasSurface = new Land_Readjustment_Tool.UI.MapCanvas.CanvasPanel();
            _objectSelectionContextMenu = new ContextMenuStrip(components);
            _mnuEditText = new ToolStripMenuItem();
            _mnuObjectSelectionSeparator1 = new ToolStripSeparator();
            _mnuClearSelection = new ToolStripMenuItem();
            _mnuMoveSelectedObjects = new ToolStripMenuItem();
            _mnuCopySelectedObjects = new ToolStripMenuItem();
            _mnuViewEditData = new ToolStripMenuItem();
            _mnuAssignData = new ToolStripMenuItem();
            _mnuCreateFeaturesFromSelection = new ToolStripMenuItem();
            _mnuObjectSelectionSeparator2 = new ToolStripSeparator();
            _mnuDeleteSelectedObjects = new ToolStripMenuItem();
            _drawingOptionsContextMenu = new ContextMenuStrip(components);
            _selectionOptionsContextMenu = new ContextMenuStrip(components);
            _objectSelectionContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // _objectSelectionContextMenu
            // 
            _objectSelectionContextMenu.ImageScalingSize = new Size(20, 20);
            _objectSelectionContextMenu.Items.AddRange(new ToolStripItem[] { _mnuEditText, _mnuObjectSelectionSeparator1, _mnuClearSelection, _mnuMoveSelectedObjects, _mnuCopySelectedObjects, _mnuViewEditData, _mnuAssignData, _mnuCreateFeaturesFromSelection, _mnuObjectSelectionSeparator2, _mnuDeleteSelectedObjects });
            _objectSelectionContextMenu.Name = "_objectSelectionContextMenu";
            _objectSelectionContextMenu.Size = new Size(222, 188);
            // 
            // _mnuEditText
            // 
            _mnuEditText.Name = "_mnuEditText";
            _mnuEditText.Size = new Size(221, 24);
            _mnuEditText.Text = "Edit Text";
            // 
            // _mnuObjectSelectionSeparator1
            // 
            _mnuObjectSelectionSeparator1.Name = "_mnuObjectSelectionSeparator1";
            _mnuObjectSelectionSeparator1.Size = new Size(218, 6);
            // 
            // _mnuClearSelection
            // 
            _mnuClearSelection.Name = "_mnuClearSelection";
            _mnuClearSelection.Size = new Size(221, 24);
            _mnuClearSelection.Text = "Clear Selection";
            // 
            // _mnuMoveSelectedObjects
            // 
            _mnuMoveSelectedObjects.Name = "_mnuMoveSelectedObjects";
            _mnuMoveSelectedObjects.Size = new Size(221, 24);
            _mnuMoveSelectedObjects.Text = "Move object(s)";
            // 
            // _mnuCopySelectedObjects
            // 
            _mnuCopySelectedObjects.Name = "_mnuCopySelectedObjects";
            _mnuCopySelectedObjects.Size = new Size(221, 24);
            _mnuCopySelectedObjects.Text = "Copy Object(s)";
            // 
            // _mnuViewEditData
            // 
            _mnuViewEditData.Name = "_mnuViewEditData";
            _mnuViewEditData.Size = new Size(221, 24);
            _mnuViewEditData.Text = "View/Edit Data";
            // 
            // _mnuAssignData
            // 
            _mnuAssignData.Name = "_mnuAssignData";
            _mnuAssignData.Size = new Size(221, 24);
            _mnuAssignData.Text = "Assign Data";
            // 
            // _mnuCreateFeaturesFromSelection
            // 
            _mnuCreateFeaturesFromSelection.Name = "_mnuCreateFeaturesFromSelection";
            _mnuCreateFeaturesFromSelection.Size = new Size(221, 24);
            _mnuCreateFeaturesFromSelection.Text = "Create Features...";
            // 
            // _mnuObjectSelectionSeparator2
            // 
            _mnuObjectSelectionSeparator2.Name = "_mnuObjectSelectionSeparator2";
            _mnuObjectSelectionSeparator2.Size = new Size(218, 6);
            // 
            // _mnuDeleteSelectedObjects
            // 
            _mnuDeleteSelectedObjects.Name = "_mnuDeleteSelectedObjects";
            _mnuDeleteSelectedObjects.Size = new Size(221, 24);
            _mnuDeleteSelectedObjects.Text = "Delete Selected Object(s)";
            // 
            // _drawingOptionsContextMenu
            // 
            _drawingOptionsContextMenu.ImageScalingSize = new Size(20, 20);
            _drawingOptionsContextMenu.Name = "_drawingOptionsContextMenu";
            _drawingOptionsContextMenu.Size = new Size(61, 4);
            // 
            // _selectionOptionsContextMenu
            // 
            _selectionOptionsContextMenu.ImageScalingSize = new Size(20, 20);
            _selectionOptionsContextMenu.Name = "_selectionOptionsContextMenu";
            _selectionOptionsContextMenu.Size = new Size(61, 4);
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
            _objectSelectionContextMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
