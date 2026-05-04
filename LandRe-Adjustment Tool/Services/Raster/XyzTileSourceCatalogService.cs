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

            // Built-in sources are always derived from code — never persisted to disk.
            List<XyzTileSourceCatalogItem> cleanedSources = sources
                .Where(s => !s.IsBuiltIn)
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

        /// <summary>
        /// Returns the hard-coded list of popular, read-only built-in tile sources.
        /// These are always available regardless of whether a catalog file exists.
        /// </summary>
        private static List<XyzTileSourceCatalogItem> GetDefaultSources()
        {
            return
            [
                // ── OpenStreetMap ──────────────────────────────────────────────
                new("OpenStreetMap Standard",
                    "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                new("OpenStreetMap Humanitarian (HOT)",
                    "https://tile.openstreetmap.fr/hot/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),
                // ── Esri / ArcGIS Online ───────────────────────────────────────
                new("Esri World Imagery",
                    "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
                    0, 25, "jpg", IsBuiltIn: false),

                new("Esri World Street Map",
                    "https://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}",
                    0, 25, "jpg", IsBuiltIn: false),
                new("Esri World Topo Map",
                    "https://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{z}/{y}/{x}",
                    0, 25, "jpg", IsBuiltIn: false),

                new("Esri World Light Gray Canvas",
                    "https://services.arcgisonline.com/ArcGIS/rest/services/Canvas/World_Light_Gray_Base/MapServer/tile/{z}/{y}/{x}",
                    0, 22, "jpg", IsBuiltIn: false),

                new("Esri World Dark Gray Canvas",
                    "https://services.arcgisonline.com/ArcGIS/rest/services/Canvas/World_Dark_Gray_Base/MapServer/tile/{z}/{y}/{x}",
                    0, 22, "jpg", IsBuiltIn: false),

                new("Esri World Shaded Relief",
                    "https://services.arcgisonline.com/ArcGIS/rest/services/World_Shaded_Relief/MapServer/tile/{z}/{y}/{x}",
                    0, 22, "jpg", IsBuiltIn: false),

                new("Esri World Physical Map",
                    "https://services.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{z}/{y}/{x}",
                    0, 16, "jpg", IsBuiltIn: false),

                // ── Google (unofficial, no API key — use in compliance with terms) ──
                new("Google Satellite",
                    "https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}",
                    0, 25, "jpg", IsBuiltIn: false),

                new("Google Hybrid (Satellite + Labels)",
                    "https://mt1.google.com/vt/lyrs=y&x={x}&y={y}&z={z}",
                    0, 25, "jpg", IsBuiltIn: false),
                new("Google Streets",
                    "https://mt1.google.com/vt/lyrs=m&x={x}&y={y}&z={z}",
                    0, 25, "png", IsBuiltIn: false),

                new("Google Terrain",
                    "https://mt1.google.com/vt/lyrs=p&x={x}&y={y}&z={z}",
                    0, 25, "png", IsBuiltIn: false),
                // ── CartoDB / CARTO ────────────────────────────────────────────
                new("CartoDB Positron (Light)",
                    "https://cartodb-basemaps-a.global.ssl.fastly.net/light_all/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                new("CartoDB Dark Matter",
                    "https://cartodb-basemaps-a.global.ssl.fastly.net/dark_all/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                new("CartoDB Positron (No Labels)",
                    "https://cartodb-basemaps-a.global.ssl.fastly.net/light_nolabels/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                // ── Stadia Maps (Stamen successor) ─────────────────────────────
                new("Stadia Stamen Terrain",
                    "https://tiles.stadiamaps.com/tiles/stamen_terrain/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                new("Stadia Stamen Toner",
                    "https://tiles.stadiamaps.com/tiles/stamen_toner/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                new("Stadia Stamen Toner Lite",
                    "https://tiles.stadiamaps.com/tiles/stamen_toner_lite/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                new("Stadia Alidade Smooth",
                    "https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                new("Stadia Alidade Smooth Dark",
                    "https://tiles.stadiamaps.com/tiles/alidade_smooth_dark/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                new("Stadia OSM Bright",
                    "https://tiles.stadiamaps.com/tiles/osm_bright/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),
                // ── OpenTopoMap ────────────────────────────────────────────────
                new("OpenTopoMap",
                    "https://opentopomap.org/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),

                // ── USGS National Map (US-centric, useful for reference) ───────
                new("USGS Topo",
                    "https://basemap.nationalmap.gov/arcgis/rest/services/USGSTopo/MapServer/tile/{z}/{y}/{x}",
                    0, 25, "jpg", IsBuiltIn: false),

                new("USGS Imagery",
                    "https://basemap.nationalmap.gov/arcgis/rest/services/USGSImageryOnly/MapServer/tile/{z}/{y}/{x}",
                    0, 22, "jpg", IsBuiltIn: false),

                // ── Wikimedia Maps ─────────────────────────────────────────────
                new("Wikimedia Maps",
                    "https://maps.wikimedia.org/osm-intl/{z}/{x}/{y}.png",
                    0, 25, "png", IsBuiltIn: false),
            ];
        }

        private static List<XyzTileSourceCatalogItem> MergeWithDefaultSources(
            IEnumerable<XyzTileSourceCatalogItem>? savedSources)
        {
            List<XyzTileSourceCatalogItem> builtInSources = GetDefaultSources();
            HashSet<string> builtInNames = new(
                builtInSources.Select(s => s.Name),
                StringComparer.OrdinalIgnoreCase);

            // Only keep user-added sources; strip any stale saved copy of a built-in name.
            List<XyzTileSourceCatalogItem> userSources = (savedSources ?? [])
                .Where(s => !s.IsBuiltIn && !builtInNames.Contains(s.Name))
                .Where(IsValidSource)
                .GroupBy(source => source.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            // Built-in sources always come first, then user sources sorted alphabetically.
            return builtInSources
                .Concat(userSources.OrderBy(s => s.Name))
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
        string ImageExtension,
        bool IsBuiltIn = false);
}
