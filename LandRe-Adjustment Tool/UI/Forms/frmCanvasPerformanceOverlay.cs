using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.Forms
{
    public partial class frmCanvasPerformanceOverlay : Form
    {
        private readonly MapCanvasControl _canvas;

        public frmCanvasPerformanceOverlay(MapCanvasControl canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            InitializeComponent();

            btnClose.Click += btnClose_Click;
            refreshTimer.Tick += refreshTimer_Tick;
            Shown += frmCanvasPerformanceOverlay_Shown;
            FormClosed += frmCanvasPerformanceOverlay_FormClosed;
        }

        public void RefreshPerformanceText()
        {
            CanvasLivePerformanceSnapshot live = _canvas.GetPerformanceSnapshot();
            CanvasOperationPerformanceSnapshot operation =
                CanvasPerformanceTelemetry.GetLastOperation();

            lblOverall.Text = $"Overall: {DescribeOverall(live, operation)}";
            lblFrame.Text =
                $"Screen speed: {DescribeFrame(live)} ({live.FramesPerSecond:0} FPS, average {FormatMs(live.AverageFrameElapsedMs)} per redraw).";
            lblObjects.Text =
                $"Objects loaded: {live.LoadedObjectCount:n0}. Visible now: {live.VisibleObjectCount:n0}. Selected: {live.SelectedObjectCount:n0}.";
            lblRendered.Text =
                $"Last redraw: checked {live.SpatialCandidateCount:n0} nearby objects and drew {live.RenderedObjectCount:n0}. Query {FormatMs(live.VectorQueryElapsedMs)}, drawing {FormatMs(live.VectorRenderElapsedMs)}.";
            lblDatabase.Text = FormatDatabaseText(operation);
            lblCache.Text =
                $"Canvas cache: {(live.VectorCacheValid ? "ready" : "refreshing")}. Last cache rebuild {FormatMs(live.VectorCacheRefreshElapsedMs)}. Raster layers visible: {live.VisibleRasterLayerCount:n0}/{live.RasterLayerCount:n0}.";
            lblAdvice.Text = BuildAdvice(live, operation);
            lblUpdated.Text = $"Updated: {DateTime.Now:HH:mm:ss}";
        }

        private void frmCanvasPerformanceOverlay_Shown(object? sender, EventArgs e)
        {
            RefreshPerformanceText();
            refreshTimer.Start();
        }

        private void frmCanvasPerformanceOverlay_FormClosed(object? sender, FormClosedEventArgs e)
        {
            refreshTimer.Stop();
        }

        private void refreshTimer_Tick(object? sender, EventArgs e)
        {
            RefreshPerformanceText();
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private static string FormatDatabaseText(CanvasOperationPerformanceSnapshot operation)
        {
            if (operation.CapturedAt == DateTime.MinValue)
            {
                return "Last database work: no save/load measured yet. Draw, copy, move, edit, delete, or refresh objects.";
            }

            return
                $"Last database work: {operation.OperationName} - {operation.Detail}. Database {FormatMs(operation.DatabaseElapsedMs)}, canvas update {FormatMs(operation.CanvasElapsedMs)}, total {FormatMs(operation.TotalElapsedMs)}.";
        }

        private static string DescribeOverall(
            CanvasLivePerformanceSnapshot live,
            CanvasOperationPerformanceSnapshot operation)
        {
            if (operation.CapturedAt != DateTime.MinValue && operation.DatabaseElapsedMs >= 1000)
            {
                return "database is the main slowdown";
            }

            if (live.AverageFrameElapsedMs <= 0)
            {
                return "waiting for redraw timing";
            }

            if (live.AverageFrameElapsedMs <= 16)
            {
                return "very smooth";
            }

            if (live.AverageFrameElapsedMs <= 33)
            {
                return "smooth enough";
            }

            if (live.AverageFrameElapsedMs <= 66)
            {
                return "noticeable delay";
            }

            return "slow redraws";
        }

        private static string DescribeFrame(CanvasLivePerformanceSnapshot live)
        {
            if (live.AverageFrameElapsedMs <= 0)
            {
                return "waiting";
            }

            if (live.FramesPerSecond >= 55)
            {
                return "very smooth";
            }

            if (live.FramesPerSecond >= 30)
            {
                return "smooth";
            }

            if (live.FramesPerSecond >= 15)
            {
                return "usable but heavy";
            }

            return "slow";
        }

        private static string BuildAdvice(
            CanvasLivePerformanceSnapshot live,
            CanvasOperationPerformanceSnapshot operation)
        {
            if (operation.CapturedAt != DateTime.MinValue && operation.DatabaseElapsedMs >= 1000)
            {
                return "Tip: the last save/load was database-heavy. Bulk operations should stay batched, and hiding unused layers helps avoid extra refresh work.";
            }

            if (live.AverageFrameElapsedMs > 66 && live.RenderedObjectCount > 10000)
            {
                return "Tip: redraw is heavy because many objects are visible. Zoom in or hide layers you do not need right now.";
            }

            if (live.SpatialCandidateCount > live.RenderedObjectCount * 5 && live.SpatialCandidateCount > 5000)
            {
                return "Tip: the canvas is checking many nearby objects. Spatial filtering is working, but this view is still crowded.";
            }

            if (!live.VectorCacheValid)
            {
                return "Tip: the canvas is rebuilding its drawing cache. Wait for it to finish before measuring the final speed.";
            }

            return "Tip: this looks healthy. Use draw, move, copy, edit, delete, and refresh while this window is open to compare timings.";
        }

        private static string FormatMs(double milliseconds)
        {
            if (milliseconds < 0.05)
            {
                return "0 ms";
            }

            if (milliseconds < 100)
            {
                return $"{milliseconds:0.0} ms";
            }

            return $"{milliseconds:0} ms";
        }
    }
}
