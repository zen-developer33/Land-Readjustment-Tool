using System.Collections.Generic;
using System.Drawing;
using Land_Readjustment_Tool.DrawingCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.DrawingCanvas.Models.Shapes
{
    /// <summary>
    /// Base interface for all drawable shapes in the replotting application.
    /// This provides the contract for geometric objects (parcels, boundaries, etc.)
    /// 
    /// WHY: Separating interface from implementation allows for future extensibility
    /// (e.g., adding database persistence, import/export, spatial operations)
    /// </summary>
    public interface IShape
    {
        /// <summary>
        /// Unique identifier for the shape (for database persistence later)
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Layer name (e.g., "Parcels", "Roads", "Boundaries")
        /// CRITICAL for land replotting - different objects on different layers
        /// </summary>
        string LayerName { get; set; }

        /// <summary>
        /// Is this shape currently selected in the UI?
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Is this shape visible (respects layer visibility)?
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Border/stroke color
        /// </summary>
        Color BorderColor { get; set; }

        /// <summary>
        /// Fill color (for polygons/parcels)
        /// </summary>
        Color FillColor { get; set; }

        /// <summary>
        /// Custom properties (Area, Owner, PlotNumber, etc.)
        /// WHY: Parcels need metadata - this allows flexible key-value storage
        /// </summary>
        Dictionary<string, object> Properties { get; }

        /// <summary>
        /// Get the axis-aligned bounding box in world coordinates.
        /// CRITICAL for spatial indexing (R-Tree queries)
        /// </summary>
        RectangleD GetBoundingBox();

        /// <summary>
        /// Draw the shape on the graphics context.
        /// WHY: Delegate is used to avoid coupling shapes to the viewport transform
        /// </summary>
        /// <param name="g">Graphics context</param>
        /// <param name="worldToScreen">Coordinate transformation function</param>
        /// <param name="isPreview">Is this a preview (dashed lines)?</param>
        void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false);

        /// <summary>
        /// Create a deep copy of this shape (for undo/redo)
        /// </summary>
        IShape Clone();

        /// <summary>
        /// Check if a world-space point intersects this shape
        /// WHY: Used for selection, snapping, spatial queries
        /// </summary>
        bool ContainsPoint(PointD worldPoint, float tolerance);

        /// <summary>
        /// Returns all snap points for this shape (for object snap)
        /// </summary>
        IEnumerable<SnapPoint> GetSnapPoints();
    }
}
