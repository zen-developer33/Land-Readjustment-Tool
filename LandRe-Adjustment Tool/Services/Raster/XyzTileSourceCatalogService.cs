using System.Text.Json;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Stores reusable XYZ tile source definitions inside the active project folder.
    /// </summary>
    public static class XyzTileSourceCatalogService
    {
        private const string XyzSourceFolderName = "XyzTileSources";
        private const string CatalogFileName = "tile-sources.json";

        public static List<XyzTileSourceCatalogItem> Load(string projectFolderPath)
        {
            string catalogPath = GetCatalogPath(projectFolderPath);
            if (!File.Exists(catalogPath))
            {
                return GetDefaultSources();
            }

            try
            {
                string json = File.ReadAllText(catalogPath);
                List<XyzTileSourceCatalogItem>? sources =
                    JsonSerializer.Deserialize<List<XyzTileSourceCatalogItem>>(json);

                return MergeWithDefaultSources(sources);
            }
            catch
            {
                return GetDefaultSources();
            }
        }

        public static void Save(
            string projectFolderPath,
            IEnumerable<XyzTileSourceCatalogItem> sources)
        {
            string catalogPath = GetCatalogPath(projectFolderPath);
            Directory.CreateDirectory(Path.GetDirectoryName(catalogPath)!);

            List<XyzTileSourceCatalogItem> cleanedSources = sources
                .Where(IsValidSource)
                .GroupBy(source => source.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(source => source.Name)
                .ToList();

            string json = JsonSerializer.Serialize(
                cleanedSources,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(catalogPath, json);
        }

        private static string GetCatalogPath(string projectFolderPath)
        {
            if (string.IsNullOrWhiteSpace(projectFolderPath))
            {
                throw new DirectoryNotFoundException(
                    "Project folder was not found.");
            }

            return Path.Combine(
                projectFolderPath,
                XyzSourceFolderName,
                CatalogFileName);
        }

        private static List<XyzTileSourceCatalogItem> GetDefaultSources()
        {
            return
            [
                new XyzTileSourceCatalogItem(
                    "OpenStreetMap Standard",
                    "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                    0,
                    19,
                    "png"),
                new XyzTileSourceCatalogItem(
                    "Esri World Imagery",
                    "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
                    0,
                    19,
                    "jpg")
            ];
        }

        private static List<XyzTileSourceCatalogItem> MergeWithDefaultSources(
            IEnumerable<XyzTileSourceCatalogItem>? savedSources)
        {
            List<XyzTileSourceCatalogItem> mergedSources = (savedSources ?? [])
                .Where(IsValidSource)
                .GroupBy(source => source.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            foreach (XyzTileSourceCatalogItem defaultSource in GetDefaultSources())
            {
                bool exists = mergedSources.Any(source =>
                    string.Equals(
                        source.Name,
                        defaultSource.Name,
                        StringComparison.OrdinalIgnoreCase));

                if (!exists)
                    mergedSources.Add(defaultSource);
            }

            return mergedSources
                .OrderBy(source => source.Name)
                .ToList();
        }

        private static bool IsValidSource(XyzTileSourceCatalogItem source)
        {
            return !string.IsNullOrWhiteSpace(source.Name) &&
                   !string.IsNullOrWhiteSpace(source.UrlTemplate) &&
                   source.MinZoom >= 0 &&
                   source.MaxZoom >= source.MinZoom &&
                   source.MaxZoom <= 25;
        }
    }

    public sealed record XyzTileSourceCatalogItem(
        string Name,
        string UrlTemplate,
        int MinZoom,
        int MaxZoom,
        string ImageExtension);
}
