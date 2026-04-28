using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Runtime feature composed from persisted canvas object and mapped shape.
    /// </summary>
    public sealed record CanvasFeature(
        CanvasObject CanvasObject,
        IShape Shape,
        CanvasLayer? Layer);
}
