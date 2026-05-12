using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Assignment;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Import.Readers;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Services.Assignment
{
    public interface IProjectBoundaryAssignmentService
    {
        Task<IReadOnlyList<ProjectBoundaryAssignmentCandidate>> GetCandidatesAsync(
            ProjectSession session,
            CancellationToken ct = default);

        Task<ProjectBoundaryAssignmentResult> AssignProjectBoundaryAsync(
            ProjectSession session,
            Guid sourceObjectId,
            bool deleteExistingBoundaryObjects,
            CancellationToken ct = default);

        Task<ProjectBoundaryAssignmentResult> RemoveProjectBoundaryAsync(
            ProjectSession session,
            CancellationToken ct = default);
    }

    public sealed class ProjectBoundaryAssignmentService : IProjectBoundaryAssignmentService
    {
        private const string ProjectBoundaryLayerName = "Project Boundary";
        private const string ProjectBoundaryLayerType = "ProjectBoundary";

        private readonly IProjectScopedFactory _projectScopedFactory;

        public ProjectBoundaryAssignmentService(IProjectScopedFactory projectScopedFactory)
        {
            _projectScopedFactory = projectScopedFactory
                ?? throw new ArgumentNullException(nameof(projectScopedFactory));
        }

        public async Task<IReadOnlyList<ProjectBoundaryAssignmentCandidate>> GetCandidatesAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(session);

            CanvasLayerTreeService layerTreeService =
                _projectScopedFactory.CreateCanvasLayerTreeService(session);
            IReadOnlyList<CanvasLayerTreeGroup> layerTree =
                await layerTreeService.GetLayerTreeAsync(ct);

            Dictionary<int, CanvasLayerTreeGroup> candidateLayerGroups =
                layerTree
                    .Where(group =>
                        string.Equals(group.Key, CanvasLayerTreeService.DrawingMarkupGroupKey, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(group.Key, CanvasLayerTreeService.ExternalGroupKey, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(group => group.Layers.Select(layer => new { Layer = layer, Group = group }))
                    .Where(item => IsEditableCandidateLayer(item.Layer))
                    .ToDictionary(item => item.Layer.Id, item => item.Group);

            if (candidateLayerGroups.Count == 0)
                return [];

            AppDbContext context = session.GetDbContext();
            List<int> layerIds = candidateLayerGroups.Keys.ToList();
            List<CanvasObject> objects = await context.CanvasObjects
                .AsNoTracking()
                .Include(canvasObject => canvasObject.CanvasLayer)
                .Where(canvasObject => layerIds.Contains(canvasObject.CanvasLayerId))
                .ToListAsync(ct);

            List<ProjectBoundaryAssignmentCandidate> candidates = [];
            int index = 1;
            foreach (CanvasObject canvasObject in objects)
            {
                Geometry? geometry = canvasObject.Shape;
                if (!IsAssignableBoundaryGeometry(geometry))
                    continue;

                CanvasLayerTreeGroup group = candidateLayerGroups[canvasObject.CanvasLayerId];
                Envelope envelope = new(geometry!.EnvelopeInternal);
                string layerName = canvasObject.CanvasLayer?.Name ?? $"Layer {canvasObject.CanvasLayerId}";
                string objectType = string.IsNullOrWhiteSpace(canvasObject.ObjectType)
                    ? geometry.GeometryType
                    : canvasObject.ObjectType;

                candidates.Add(new ProjectBoundaryAssignmentCandidate(
                    canvasObject.Id,
                    canvasObject.CanvasLayerId,
                    layerName,
                    group.Name,
                    objectType,
                    envelope,
                    $"{index++}. {layerName} - {objectType}"));
            }

            return candidates
                .OrderBy(candidate => candidate.LayerGroupName)
                .ThenBy(candidate => candidate.LayerName)
                .ThenBy(candidate => candidate.DisplayName)
                .ToList();
        }

        public async Task<ProjectBoundaryAssignmentResult> AssignProjectBoundaryAsync(
            ProjectSession session,
            Guid sourceObjectId,
            bool deleteExistingBoundaryObjects,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(session);
            AppDbContext context = session.GetDbContext();
            await context.Database.MigrateAsync(ct);

            CanvasObject? sourceObject = await context.CanvasObjects
                .AsNoTracking()
                .Include(canvasObject => canvasObject.CanvasLayer)
                .FirstOrDefaultAsync(canvasObject => canvasObject.Id == sourceObjectId, ct);

            if (sourceObject?.Shape == null)
            {
                return Failed("The selected drawing object was not found.");
            }

            if (!IsAssignableBoundaryGeometry(sourceObject.Shape))
            {
                return Failed("The selected object is not a polygon boundary.");
            }

            Geometry sourceGeometry = sourceObject.Shape.Copy();
            Geometry? validGeometry =
                BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(sourceGeometry);
            if (validGeometry == null)
            {
                return Failed("The selected object could not be converted to a valid boundary polygon.");
            }

            NormalizeGeometryForCanvasDatabase(validGeometry);

            CanvasLayer boundaryLayer = await GetOrCreateProjectBoundaryLayerAsync(context, ct);
            ApplyProjectBoundaryDefaultStyle(boundaryLayer);

            int removedCount = 0;
            if (deleteExistingBoundaryObjects)
            {
                List<CanvasObject> existingBoundaryObjects = await context.CanvasObjects
                    .Where(canvasObject => canvasObject.CanvasLayerId == boundaryLayer.Id)
                    .ToListAsync(ct);

                removedCount = existingBoundaryObjects.Count;
                context.CanvasObjects.RemoveRange(existingBoundaryObjects);
            }

            DateTime now = DateTime.Now;
            CanvasObject boundaryObject = new()
            {
                CanvasLayerId = boundaryLayer.Id,
                ObjectType = "Polygon",
                Shape = validGeometry,
                ObjectDescription = $"Project Boundary assigned from {sourceObject.CanvasLayer?.Name ?? "canvas object"}",
                IsVisible = true,
                IsLocked = false,
                CreatedDate = now,
                LastModifiedDate = now
            };

            await context.CanvasObjects.AddAsync(boundaryObject, ct);
            boundaryLayer.LastModifiedDate = now;

            try
            {
                await context.SaveChangesAsync(ct);
                context.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                session.Logger.LogError("Project boundary assignment save failed.", ex);
                return Failed($"Could not save the project boundary: {BuildExceptionMessage(ex)}");
            }

            return new ProjectBoundaryAssignmentResult(
                true,
                null,
                1,
                removedCount,
                new Envelope(validGeometry.EnvelopeInternal));
        }

        public async Task<ProjectBoundaryAssignmentResult> RemoveProjectBoundaryAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(session);
            AppDbContext context = session.GetDbContext();
            await context.Database.MigrateAsync(ct);

            CanvasLayer? boundaryLayer = await context.CanvasLayers
                .FirstOrDefaultAsync(
                    layer => layer.Name == ProjectBoundaryLayerName ||
                             layer.LayerType == ProjectBoundaryLayerType,
                    ct);

            if (boundaryLayer == null)
            {
                return new ProjectBoundaryAssignmentResult(true, null, 0, 0, null);
            }

            ApplyProjectBoundaryDefaultStyle(boundaryLayer);
            List<CanvasObject> existingBoundaryObjects = await context.CanvasObjects
                .Where(canvasObject => canvasObject.CanvasLayerId == boundaryLayer.Id)
                .ToListAsync(ct);

            context.CanvasObjects.RemoveRange(existingBoundaryObjects);
            boundaryLayer.LastModifiedDate = DateTime.Now;

            try
            {
                await context.SaveChangesAsync(ct);
                context.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                session.Logger.LogError("Project boundary removal failed.", ex);
                return Failed($"Could not remove the project boundary: {BuildExceptionMessage(ex)}");
            }

            return new ProjectBoundaryAssignmentResult(
                true,
                null,
                0,
                existingBoundaryObjects.Count,
                null);
        }

        private static bool IsEditableCandidateLayer(CanvasLayer layer)
        {
            return layer.Id > 0 &&
                   !layer.IsLocked &&
                   layer.IsSelectable &&
                   !string.Equals(layer.LayerType, CanvasLayerTreeService.RasterLayerType, StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(layer.LayerType, ProjectBoundaryLayerType, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAssignableBoundaryGeometry(Geometry? geometry)
        {
            if (geometry == null || geometry.IsEmpty)
                return false;

            return geometry is Polygon or MultiPolygon;
        }

        private static void ApplyProjectBoundaryDefaultStyle(CanvasLayer layer)
        {
            layer.FillStyle = "None";
            layer.FillColor = null;
            layer.FillTransparency = 0;

            if (string.IsNullOrWhiteSpace(layer.BorderColor))
                layer.BorderColor = "#CF7C82";

            if (layer.LineWeight <= 0)
                layer.LineWeight = 2.0;

            if (string.IsNullOrWhiteSpace(layer.LineStyle))
                layer.LineStyle = "Solid";
        }

        private static void NormalizeGeometryForCanvasDatabase(Geometry geometry)
        {
            geometry.SRID = 0;
            for (int index = 0; index < geometry.NumGeometries; index++)
                geometry.GetGeometryN(index).SRID = 0;
        }

        private static async Task<CanvasLayer> GetOrCreateProjectBoundaryLayerAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            CanvasLayer? boundaryLayer = await context.CanvasLayers
                .FirstOrDefaultAsync(
                    layer => layer.Name == ProjectBoundaryLayerName ||
                             layer.LayerType == ProjectBoundaryLayerType,
                    ct);

            if (boundaryLayer != null)
                return boundaryLayer;

            DateTime now = DateTime.Now;
            int displayOrder =
                (await context.CanvasLayers
                    .Select(layer => (int?)layer.DisplayOrder)
                    .MaxAsync(ct) ?? -1) + 1;

            boundaryLayer = new CanvasLayer
            {
                Name = ProjectBoundaryLayerName,
                LayerType = ProjectBoundaryLayerType,
                IsVisible = true,
                IsLocked = false,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = displayOrder,
                BorderColor = "#CF7C82",
                LineWeight = 2.0,
                LineStyle = "Solid",
                LineTypeScale = 1.0,
                FillColor = null,
                FillTransparency = 0,
                FillStyle = "None",
                LabelColor = "#000000",
                PointSymbol = "Dot",
                PointSize = 5.0,
                CreatedDate = now,
                LastModifiedDate = now,
                Description = $"Default layer: {ProjectBoundaryLayerName}"
            };

            await context.CanvasLayers.AddAsync(boundaryLayer, ct);
            return boundaryLayer;
        }

        private static ProjectBoundaryAssignmentResult Failed(string message)
        {
            return new ProjectBoundaryAssignmentResult(false, message, 0, 0, null);
        }

        private static string BuildExceptionMessage(Exception ex)
        {
            List<string> messages = [];
            Exception? current = ex;
            while (current != null)
            {
                if (!string.IsNullOrWhiteSpace(current.Message) &&
                    !messages.Contains(current.Message))
                {
                    messages.Add(current.Message);
                }

                current = current.InnerException;
            }

            return string.Join(" ", messages);
        }
    }
}
