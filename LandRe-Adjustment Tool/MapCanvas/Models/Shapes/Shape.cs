using System.Collections.Generic;
using System.Drawing;
using Land_Readjustment_Tool.DrawingCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.DrawingCanvas.Models.Shapes
{
    /// <summary>
    /// Abstract base class for all shapes.
    /// Provides common functionality and default implementations.
    /// 
    /// WHY: Reduces code duplication across concrete shape implementations
    /// while enforcing the contract through IShape interface.
    /// </summary>
    public abstract class Shape : IShape
    {
        // Auto-property with backing field for Id
        public Guid Id { get; protected set; }

        public string LayerName { get; set; } = "Default";
        public bool IsSelected { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public Color BorderColor { get; set; } = Color.White;
        public Color FillColor { get; set; } = Color.FromArgb(28, 255, 165, 0);

        /// <summary>
        /// Custom properties for parcel metadata
        /// Examples: Area, Owner, PlotNumber, Zoning, etc.
        /// </summary>
        public Dictionary<string, object> Properties { get; private set; }

        protected Shape()
        {
            Id = Guid.NewGuid();
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Copy constructor for cloning
        /// </summary>
        protected Shape(Shape source)
        {
            Id = Guid.NewGuid(); // New ID for clone
            LayerName = source.LayerName;
            IsSelected = false; // Clones start unselected
            IsVisible = source.IsVisible;
            BorderColor = source.BorderColor;
            FillColor = source.FillColor;
            
            // Deep copy properties dictionary
            Properties = new Dictionary<string, object>(source.Properties);
        }

        // Abstract members - must be implemented by concrete shapes
        public abstract RectangleD GetBoundingBox();
        public abstract void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false);
        public abstract IShape Clone();
        public abstract bool ContainsPoint(PointD worldPoint, float tolerance);
        public virtual IEnumerable<SnapPoint> GetSnapPoints() { yield break; }

        /// <summary>
        /// Helper method: Calculate distance from point to line segment
        /// WHY: Used by Line shape for hit testing
        /// </summary>
        protected static float PointToSegmentDistance(PointD point, PointD lineStart, PointD lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            
            if (dx == 0 && dy == 0)
            {
                // Start and end are the same point
                return Distance(point, lineStart);
            }

            // Calculate parameter t that represents the projection of point onto the line
            float t = (float)(((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / (dx * dx + dy * dy));
            
            // Clamp t to [0, 1] to stay within the segment
            t = Math.Max(0, Math.Min(1, t));
            
            // Find the closest point on the segment
            PointD closest = new PointD(
                lineStart.X + t * dx,
                lineStart.Y + t * dy
            );
            
            return Distance(point, closest);
        }

        protected static float Distance(PointD p1, PointD p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
