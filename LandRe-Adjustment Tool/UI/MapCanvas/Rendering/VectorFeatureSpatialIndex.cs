using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    internal sealed class VectorFeatureSpatialIndex
    {
        private STRtree<CanvasFeature> _spatialIndex = new();
        private bool _hasEntries;

        public void Rebuild(IEnumerable<CanvasFeature> features)
        {
            STRtree<CanvasFeature> spatialIndex = new();
            bool hasEntries = false;

            foreach (CanvasFeature feature in features)
            {
                if (!TryCreateEnvelope(feature.Shape.GetBoundingBox(), out Envelope envelope))
                {
                    continue;
                }

                spatialIndex.Insert(envelope, feature);
                hasEntries = true;
            }

            if (hasEntries)
            {
                spatialIndex.Build();
            }

            _spatialIndex = spatialIndex;
            _hasEntries = hasEntries;
        }

        public IReadOnlyList<CanvasFeature> Query(RectangleD worldBounds)
        {
            if (!_hasEntries ||
                !TryCreateEnvelope(worldBounds, out Envelope envelope))
            {
                return [];
            }

            return _spatialIndex
                .Query(envelope)
                .Where(feature => feature.Shape.GetBoundingBox().IntersectsWith(worldBounds))
                .ToArray();
        }

        private static bool TryCreateEnvelope(RectangleD bounds, out Envelope envelope)
        {
            envelope = new Envelope();

            double minX = Math.Min(bounds.Left, bounds.Right);
            double maxX = Math.Max(bounds.Left, bounds.Right);
            double minY = Math.Min(bounds.Top, bounds.Bottom);
            double maxY = Math.Max(bounds.Top, bounds.Bottom);

            if (!double.IsFinite(minX) ||
                !double.IsFinite(maxX) ||
                !double.IsFinite(minY) ||
                !double.IsFinite(maxY))
            {
                return false;
            }

            envelope = new Envelope(minX, maxX, minY, maxY);
            return !envelope.IsNull;
        }
    }
}
