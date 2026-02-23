using System.Drawing;
using Land_Readjustment_Tool.DrawingCanvas.Models.Shapes;
using Land_Readjustment_Tool.DrawingCanvas.Core.SpatialIndex;
using System.Collections.Generic;
using System.Linq;

namespace Land_Readjustment_Tool.DrawingCanvas.Core
{
    /// <summary>
    /// OPTIMIZED ShapeManager with bulk loading support.
    /// Uses NetTopologySuite STRtree R-tree spatial index for robust spatial queries.
    /// </summary>
    public class ShapeManager
    {
        private List<IShape> _shapes;
        private RTreeSpatialIndex _spatialIndex;
        private RectangleD _worldBounds;

        private bool _indexDirty = false;

        public ShapeManager(RectangleD worldBounds)
        {
            _shapes = new List<IShape>();
            _worldBounds = worldBounds;
            _spatialIndex = new RTreeSpatialIndex(worldBounds);
        }

        public IReadOnlyList<IShape> GetAllShapes() => _shapes.AsReadOnly();
        public int Count => _shapes.Count;

        #region Add/Remove Operations

        public void AddShape(IShape shape)
        {
            if (shape == null) return;
            _shapes.Add(shape);
            _spatialIndex.Insert(shape);
        }

        public void AddShapes(IEnumerable<IShape> shapes)
        {
            foreach (var shape in shapes)
            {
                _shapes.Add(shape);
                _spatialIndex.Insert(shape);
            }
        }

        public void BulkAddShapes(IEnumerable<IShape> shapes)
        {
            var shapeList = shapes as IList<IShape> ?? shapes.ToList();
            if (_shapes.Capacity < _shapes.Count + shapeList.Count)
            {
                _shapes.Capacity = _shapes.Count + shapeList.Count;
            }
            _shapes.AddRange(shapeList);
            RebuildSpatialIndex();
        }

        public bool RemoveShape(IShape shape)
        {
            if (shape == null) return false;
            bool removed = _shapes.Remove(shape);
            if (removed)
            {
                _spatialIndex.Remove(shape);
            }
            return removed;
        }

        public void RemoveShapes(IEnumerable<IShape> shapes)
        {
            foreach (var shape in shapes)
            {
                RemoveShape(shape);
            }
        }

        public void BulkRemoveShapes(IEnumerable<IShape> shapesToRemove)
        {
            var removeSet = new HashSet<IShape>(shapesToRemove);
            _shapes.RemoveAll(s => removeSet.Contains(s));
            RebuildSpatialIndex();
        }

        public void Clear()
        {
            _shapes.Clear();
            _spatialIndex.Clear();
        }

        #endregion

        #region Query Operations

        public List<IShape> QueryShapesInBound(RectangleD viewportBounds)
        {
            var candidates = _spatialIndex.Query(viewportBounds);
            return candidates.Where(s => s.IsVisible && s.GetBoundingBox().IntersectsWith(viewportBounds)).ToList();
        }

        public List<IShape> QueryAtPointAll(PointD worldPoint, float tolerance)
        {
            RectangleD searchArea = new RectangleD(
                worldPoint.X - tolerance,
                worldPoint.Y - tolerance,
                tolerance * 2,
                tolerance * 2
            );
            var candidates = _spatialIndex.Query(searchArea);
            return candidates.Where(s => s.ContainsPoint(worldPoint, tolerance)).ToList();
        }

        public IShape? QueryAtPoint(PointD worldPoint, float tolerance)
        {
            return QueryAtPointAll(worldPoint, tolerance).LastOrDefault(s => s.IsVisible);
        }

        public List<IShape> QueryInRectangle(RectangleD selectionRect)
        {
            var candidates = _spatialIndex.Query(selectionRect);
            return candidates.Where(s => selectionRect.IntersectsWith(s.GetBoundingBox())).ToList();
        }

        #endregion

        #region Bounding Box Operations

        public RectangleD CalculateExtents()
        {
            if (_shapes.Count == 0)
                return new RectangleD(0, 0, 1000, 1000);
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var shape in _shapes)
            {
                var box = shape.GetBoundingBox();
                minX = Math.Min(minX, box.X);
                minY = Math.Min(minY, box.Y);
                maxX = Math.Max(maxX, box.X + box.Width);
                maxY = Math.Max(maxY, box.Y + box.Height);
            }
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public void RebuildSpatialIndex()
        {
            _spatialIndex.Clear();
            foreach (var shape in _shapes)
            {
                _spatialIndex.Insert(shape);
            }
        }

        #endregion

        /// <summary>
        /// Ensures the world bounds cover the given rectangle (expands if needed).
        /// </summary>
        public void EnsureWorldBoundsCovers(RectangleD bounds)
        {
            double minX = Math.Min(_worldBounds.X, bounds.X);
            double minY = Math.Min(_worldBounds.Y, bounds.Y);
            double maxX = Math.Max(_worldBounds.X + _worldBounds.Width, bounds.X + bounds.Width);
            double maxY = Math.Max(_worldBounds.Y + _worldBounds.Height, bounds.Y + bounds.Height);
            _worldBounds = new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Sets the world bounds to the given rectangle.
        /// </summary>
        public void SetWorldBounds(RectangleD bounds)
        {
            _worldBounds = bounds;
        }
    }
}