namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping
{
    /// <summary>
    /// Interface for shapes that provide snap points for object snapping.
    /// </summary>
    public interface ISnapProvider
    {
        IEnumerable<SnapPoint> GetSnapPoints();
    }
}
