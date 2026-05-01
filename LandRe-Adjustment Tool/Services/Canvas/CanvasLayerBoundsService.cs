using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.Services.Canvas
{
    /// <summary>
    /// Calculates map bounds for layer commands such as Zoom To Layer.
    /// </summary>
    public sealed class CanvasLayerBoundsService
    {
        private readonly IProjectScopedFactory _projectScopedFactory;

        /// <summary>
        /// Creates a bounds service using project-scoped object repositories.
        /// </summary>
        public CanvasLayerBoundsService(IProjectScopedFactory projectScopedFactory)
        {
            _projectScopedFactory = projectScopedFactory
                ?? throw new ArgumentNullException(nameof(projectScopedFactory));
        }

        /// <summary>
        /// Gets the drawable world bounds for raster or database-backed object layers.
        /// </summary>
        public async Task<RectangleD?> GetWorldBoundsAsync(
            ProjectSession? session,
            CanvasLayer layer,
            string? projectFolderPath,
            CancellationToken ct = default)
        {
            if (IsRasterLayer(layer))
            {
                return GetRasterBounds(layer, projectFolderPath);
            }

            if (session == null)
            {
                return null;
            }

            var repository = _projectScopedFactory.CreateCanvasObjectRepository(session);
            List<CanvasObject> objects = await repository.GetByLayerIdAsync(layer.Id, ct);
            return GetObjectBounds(objects);
        }

        /// <summary>
        /// Returns whether a layer is stored as raster imagery.
        /// </summary>
        public static bool IsRasterLayer(CanvasLayer? layer)
        {
            return layer != null &&
                   string.Equals(
                       layer.LayerType,
                       CanvasLayerTreeService.RasterLayerType,
                       StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets raster world bounds from the raster render layer metadata.
        /// </summary>
        private static RectangleD? GetRasterBounds(
            CanvasLayer layer,
            string? projectFolderPath)
        {
            if (string.IsNullOrWhiteSpace(layer.SourceFile))
            {
                return null;
            }

            using IRasterRenderLayer rasterLayer =
                RasterRenderLayerFactory.FromCanvasLayer(layer, projectFolderPath);

            RectangleD bounds = rasterLayer.WorldBounds;
            if (!IsFinite(bounds.Left) ||
                !IsFinite(bounds.Top) ||
                !IsFinite(bounds.Width) ||
                !IsFinite(bounds.Height))
            {
                return null;
            }

            double minX = Math.Min(bounds.Left, bounds.Right);
            double minY = Math.Min(bounds.Top, bounds.Bottom);
            double width = Math.Abs(bounds.Width);
            double height = Math.Abs(bounds.Height);

            const double minimumExtent = 1.0;
            if (width <= 0)
            {
                minX -= minimumExtent / 2.0;
                width = minimumExtent;
            }

            if (height <= 0)
            {
                minY -= minimumExtent / 2.0;
                height = minimumExtent;
            }

            return new RectangleD(minX, minY, width, height);
        }

        /// <summary>
        /// Combines all object geometry envelopes into one layer bounds rectangle.
        /// </summary>
        private static RectangleD? GetObjectBounds(IEnumerable<CanvasObject> objects)
        {
            bool hasBounds = false;
            double minX = 0;
            double maxX = 0;
            double minY = 0;
            double maxY = 0;

            foreach (CanvasObject canvasObject in objects)
            {
                NetTopologySuite.Geometries.Envelope? envelope =
                    canvasObject.Shape?.EnvelopeInternal;

                if (envelope == null || envelope.IsNull)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    minX = envelope.MinX;
                    maxX = envelope.MaxX;
                    minY = envelope.MinY;
                    maxY = envelope.MaxY;
                    hasBounds = true;
                    continue;
                }

                minX = Math.Min(minX, envelope.MinX);
                maxX = Math.Max(maxX, envelope.MaxX);
                minY = Math.Min(minY, envelope.MinY);
                maxY = Math.Max(maxY, envelope.MaxY);
            }

            if (!hasBounds)
            {
                return null;
            }

            double width = maxX - minX;
            double height = maxY - minY;
            const double minimumLayerExtent = 1.0;

            if (width <= 0)
            {
                minX -= minimumLayerExtent / 2.0;
                width = minimumLayerExtent;
            }

            if (height <= 0)
            {
                minY -= minimumLayerExtent / 2.0;
                height = minimumLayerExtent;
            }

            return new RectangleD(minX, minY, width, height);
        }

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
