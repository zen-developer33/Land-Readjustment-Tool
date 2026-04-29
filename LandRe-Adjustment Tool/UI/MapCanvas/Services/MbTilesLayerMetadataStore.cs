using System.Text.Json;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    internal sealed record MbTilesLayerMetadata(
        string Kind,
        string SourceSrsDefinition,
        string TargetSrsDefinition,
        string? OriginalSourcePath,
        DateTime CreatedUtc)
    {
        public static MbTilesLayerMetadata Create(
            string sourceSrsDefinition,
            string targetSrsDefinition,
            string? originalSourcePath) =>
            new(
                "MBTiles",
                sourceSrsDefinition,
                targetSrsDefinition,
                originalSourcePath,
                DateTime.UtcNow);
    }

    internal static class MbTilesLayerMetadataStore
    {
        private const string SidecarSuffix = ".replot-mbtiles.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static string GetSidecarPath(string mbTilesPath) =>
            $"{mbTilesPath}{SidecarSuffix}";

        public static void Write(
            string mbTilesPath,
            MbTilesLayerMetadata metadata)
        {
            string sidecarPath = GetSidecarPath(mbTilesPath);
            string? directory = Path.GetDirectoryName(sidecarPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                sidecarPath,
                JsonSerializer.Serialize(metadata, JsonOptions));
        }

        public static MbTilesLayerMetadata? TryRead(string mbTilesPath)
        {
            string sidecarPath = GetSidecarPath(mbTilesPath);
            if (!File.Exists(sidecarPath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(sidecarPath);
                MbTilesLayerMetadata? metadata =
                    JsonSerializer.Deserialize<MbTilesLayerMetadata>(json);

                if (metadata == null ||
                    !string.Equals(metadata.Kind, "MBTiles", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(metadata.SourceSrsDefinition) ||
                    string.IsNullOrWhiteSpace(metadata.TargetSrsDefinition))
                {
                    return null;
                }

                return metadata;
            }
            catch
            {
                return null;
            }
        }
    }
}
