namespace Land_Readjustment_Tool.UI.MapCanvas.Core
{
    /// <summary>
    /// Serializable map viewport state for reopening a project at the user's last saved view.
    /// </summary>
    public sealed record MapCanvasViewportState(
        double CenterX,
        double CenterY,
        double ZoomScale,
        double VisibleWidth,
        double VisibleHeight);
}
