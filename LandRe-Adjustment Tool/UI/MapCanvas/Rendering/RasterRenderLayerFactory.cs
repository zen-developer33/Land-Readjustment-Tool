using Land_Readjustment_Tool.Core.Entities.Canvas;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    internal static class RasterRenderLayerFactory
    {
        public static IRasterRenderLayer FromCanvasLayer(
            CanvasLayer layer,
            string? projectFolderPath)
        {
            ArgumentNullException.ThrowIfNull(layer);

            if (string.IsNullOrWhiteSpace(layer.SourceFile))
            {
                throw new InvalidOperationException(
                    $"Raster layer '{layer.Name}' does not have a source file.");
            }

            string filePath = ResolveLayerFilePath(
                layer.SourceFile,
                projectFolderPath);

            if (MbTilesRenderLayer.IsMbTilesPath(filePath))
            {
                try
                {
                    return MbTilesRenderLayer.FromCanvasLayer(layer, filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Direct MBTiles renderer failed for '{layer.Name}'. Falling back to GDAL raster renderer. {ex.Message}");
                }
            }

            return RasterRenderLayer.FromCanvasLayer(layer, projectFolderPath);
        }

        internal static string ResolveLayerFilePath(
            string storedPath,
            string? projectFolderPath)
        {
            if (Path.IsPathRooted(storedPath))
            {
                return Path.GetFullPath(storedPath);
            }

            if (string.IsNullOrWhiteSpace(projectFolderPath))
            {
                return Path.GetFullPath(storedPath);
            }

            return Path.GetFullPath(Path.Combine(projectFolderPath, storedPath));
        }
    }
}
