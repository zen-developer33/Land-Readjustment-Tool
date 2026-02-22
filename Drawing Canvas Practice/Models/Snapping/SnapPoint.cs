using Drawing_Canvas_Practice.Models.Shapes;
using System;

namespace Drawing_Canvas_Practice.Models.Snapping
{
    public class SnapPoint
    {
        public SnapType Type { get; set; }
        public PointD Position { get; set; }
        public IShape ParentShape { get; set; }

        public SnapPoint(SnapType type, PointD position, IShape parentShape)
        {
            Type = type;
            Position = position;
            ParentShape = parentShape;
        }
    }
}
