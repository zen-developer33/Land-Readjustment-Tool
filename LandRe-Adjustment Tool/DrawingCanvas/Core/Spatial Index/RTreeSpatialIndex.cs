using System.Drawing;
using Land_Readjustment_Tool.DrawingCanvas.Models.Shapes;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace Land_Readjustment_Tool.DrawingCanvas.Core.SpatialIndex
{
    /// <summary>
    /// R-tree spatial index using NetTopologySuite for efficient viewport culling and spatial queries.
    /// Replaces QuadTree with STRtree for robust, scalable spatial indexing.
    /// </summary>
    public class RTreeSpatialIndex
    {
        private STRtree<IShape> _index;
        private Envelope _bounds;
        private List<IShape> _allShapes;

        public RTreeSpatialIndex(RectangleD bounds)
        {
            _bounds = new Envelope(bounds.X, bounds.X + bounds.Width, bounds.Y, bounds.Y + bounds.Height);
            _index = new STRtree<IShape>();
            _allShapes = new List<IShape>();
        }

        public void Clear()
        {
            _allShapes.Clear();
            Rebuild();
        }

        public void Insert(IShape shape)
        {
            _allShapes.Add(shape);
            Rebuild();
        }

        public bool Remove(IShape shape)
        {
            _allShapes.Remove(shape);
            Rebuild();
            return true;
        }

        public void Rebuild()
        {
            _index = new STRtree<IShape>();
            foreach (var shape in _allShapes)
            {
                var env = ToEnvelope(shape.GetBoundingBox());
                _index.Insert(env, shape);
            }
        }

        public List<IShape> Query(RectangleD area)
        {
            var env = ToEnvelope(area);
            var found = _index.Query(env);
            // Filter for intersection
            var result = new List<IShape>();
            foreach (var shape in found)
            {
                if (shape.GetBoundingBox().IntersectsWith(area))
                    result.Add(shape);
            }
            return result;
        }

        public int GetTotalObjects()
        {
            return _allShapes.Count;
        }

        private Envelope ToEnvelope(RectangleD rect)
        {
            return new Envelope(rect.X, rect.X + rect.Width, rect.Y, rect.Y + rect.Height);
        }
    }
}