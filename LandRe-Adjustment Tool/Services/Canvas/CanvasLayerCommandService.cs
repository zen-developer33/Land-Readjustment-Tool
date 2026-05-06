using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;

namespace Land_Readjustment_Tool.Services.Canvas
{
    /// <summary>
    /// Performs layer edit commands so WinForms code only coordinates UI state.
    /// </summary>
    public sealed class CanvasLayerCommandService
    {
        private readonly IProjectScopedFactory _projectScopedFactory;

        /// <summary>
        /// Creates a layer command service using project-scoped repositories.
        /// </summary>
        public CanvasLayerCommandService(IProjectScopedFactory projectScopedFactory)
        {
            _projectScopedFactory = projectScopedFactory
                ?? throw new ArgumentNullException(nameof(projectScopedFactory));
        }

        /// <summary>
        /// Creates an editable copy for UI forms without mutating the original layer.
        /// </summary>
        public CanvasLayer CreateEditableCopy(CanvasLayer layer)
        {
            return CloneLayer(layer);
        }

        /// <summary>
        /// Renames a layer and persists the change when a project session is open.
        /// </summary>
        public async Task<CanvasLayer?> RenameAsync(
            ProjectSession? session,
            CanvasLayer layer,
            string newName,
            CancellationToken ct = default)
        {
            await EnsureLayerEditableAsync(session, layer, "renamed", ct);

            if (string.IsNullOrWhiteSpace(newName) ||
                string.Equals(layer.Name, newName, StringComparison.Ordinal))
            {
                return null;
            }

            CanvasLayer updatedLayer = CloneLayer(layer);
            updatedLayer.Name = newName.Trim();
            updatedLayer.LastModifiedDate = DateTime.Now;

            await SaveLayerAsync(session, updatedLayer, ct);
            return updatedLayer;
        }

        /// <summary>
        /// Changes layer visibility and persists the change when a project session is open.
        /// </summary>
        public async Task<CanvasLayer?> SetVisibilityAsync(
            ProjectSession? session,
            CanvasLayer layer,
            bool isVisible,
            CancellationToken ct = default)
        {
            if (layer.IsVisible == isVisible)
            {
                return null;
            }

            CanvasLayer updatedLayer = CloneLayer(layer);
            updatedLayer.IsVisible = isVisible;
            updatedLayer.LastModifiedDate = DateTime.Now;

            await SaveLayerAsync(session, updatedLayer, ct);
            return updatedLayer;
        }

        /// <summary>
        /// Toggles layer lock state and persists the change when a project session is open.
        /// </summary>
        public async Task<CanvasLayer?> ToggleLockAsync(
            ProjectSession? session,
            CanvasLayer layer,
            CancellationToken ct = default)
        {
            CanvasLayer updatedLayer = CloneLayer(layer);
            updatedLayer.IsLocked = !updatedLayer.IsLocked;
            updatedLayer.LastModifiedDate = DateTime.Now;

            await SaveLayerAsync(session, updatedLayer, ct);
            return updatedLayer;
        }

        /// <summary>
        /// Saves layer property changes and persists them when a project session is open.
        /// </summary>
        public async Task<CanvasLayer> UpdatePropertiesAsync(
            ProjectSession? session,
            CanvasLayer layer,
            CancellationToken ct = default)
        {
            await EnsureLayerPropertiesEditableAsync(session, layer, ct);

            CanvasLayer updatedLayer = CloneLayer(layer);
            updatedLayer.LastModifiedDate = DateTime.Now;

            await SaveLayerAsync(session, updatedLayer, ct);
            return updatedLayer;
        }

        /// <summary>
        /// Deletes a persisted layer; unsaved default layers are removed only from UI by the caller.
        /// </summary>
        public async Task DeleteAsync(
            ProjectSession? session,
            CanvasLayer layer,
            CancellationToken ct = default)
        {
            await EnsureLayerEditableAsync(session, layer, "deleted", ct);

            if (session == null || layer.Id <= 0)
            {
                return;
            }

            var repository = _projectScopedFactory.CreateCanvasLayerRepository(session);
            await repository.DeleteAsync(layer.Id, ct);
        }

        /// <summary>
        /// Persists a layer update when a session exists; otherwise keeps the operation in memory.
        /// </summary>
        private async Task SaveLayerAsync(
            ProjectSession? session,
            CanvasLayer layer,
            CancellationToken ct)
        {
            if (session == null || layer.Id <= 0)
            {
                return;
            }

            var repository = _projectScopedFactory.CreateCanvasLayerRepository(session);
            await repository.UpdateAsync(layer, ct);
        }

        /// <summary>
        /// Stops destructive or editing commands for locked layers while still allowing lock toggles.
        /// </summary>
        private async Task EnsureLayerEditableAsync(
            ProjectSession? session,
            CanvasLayer layer,
            string action,
            CancellationToken ct)
        {
            if (session != null && layer.Id > 0)
            {
                var repository = _projectScopedFactory.CreateCanvasLayerRepository(session);
                CanvasLayer? currentLayer = await repository.GetByIDAsync(layer.Id, ct);

                if (currentLayer?.IsLocked == true)
                    throw new InvalidOperationException(
                        $"Layer '{currentLayer.Name}' is locked and cannot be {action}.");

                return;
            }

            if (layer.IsLocked)
                throw new InvalidOperationException(
                    $"Layer '{layer.Name}' is locked and cannot be {action}.");
        }

        /// <summary>
        /// Allows the properties dialog to unlock a locked layer, while blocking
        /// normal property edits until that lock checkbox is cleared.
        /// </summary>
        private async Task EnsureLayerPropertiesEditableAsync(
            ProjectSession? session,
            CanvasLayer layer,
            CancellationToken ct)
        {
            if (session != null && layer.Id > 0)
            {
                var repository = _projectScopedFactory.CreateCanvasLayerRepository(session);
                CanvasLayer? currentLayer = await repository.GetByIDAsync(layer.Id, ct);

                if (currentLayer?.IsLocked == true && layer.IsLocked)
                    throw new InvalidOperationException(
                        $"Layer '{currentLayer.Name}' is locked and cannot be edited.");

                return;
            }

            if (layer.IsLocked)
                throw new InvalidOperationException(
                    $"Layer '{layer.Name}' is locked and cannot be edited.");
        }

        /// <summary>
        /// Copies a layer so UI-held instances are not mutated before persistence succeeds.
        /// </summary>
        private static CanvasLayer CloneLayer(CanvasLayer source)
        {
            return new CanvasLayer
            {
                Id = source.Id,
                Name = source.Name,
                LayerType = source.LayerType,
                IsVisible = source.IsVisible,
                IsLocked = source.IsLocked,
                IsSelectable = source.IsSelectable,
                IsPrintable = source.IsPrintable,
                DisplayOrder = source.DisplayOrder,
                BorderColor = source.BorderColor,
                LineWeight = source.LineWeight,
                LineStyle = source.LineStyle,
                LineTypeScale = source.LineTypeScale,
                FillColor = source.FillColor,
                FillTransparency = source.FillTransparency,
                FillStyle = source.FillStyle,
                HatchPattern = source.HatchPattern,
                ShowLabels = source.ShowLabels,
                LabelFontName = source.LabelFontName,
                LabelFontSize = source.LabelFontSize,
                LabelColor = source.LabelColor,
                LabelField = source.LabelField,
                PointSymbol = source.PointSymbol,
                PointSize = source.PointSize,
                SourceFile = source.SourceFile,
                ImportedDate = source.ImportedDate,
                CreatedDate = source.CreatedDate,
                LastModifiedDate = source.LastModifiedDate,
                Description = source.Description
            };
        }
    }
}
