using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Land_Readjustment_Tool.Services.Raster;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class XyzTileBingSupportTests : IDisposable
{
    private readonly string _projectFolder;

    public XyzTileBingSupportTests()
    {
        _projectFolder = Path.Combine(
            Path.GetTempPath(),
            "landreadjustment-bing-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_projectFolder);
    }

    [Theory]
    [InlineData(0, 0, 1, "0")]
    [InlineData(1, 0, 1, "1")]
    [InlineData(0, 1, 1, "2")]
    [InlineData(1, 1, 1, "3")]
    [InlineData(3, 5, 3, "213")]
    public void TileXYToQuadkey_ConvertsXyzToExpectedQuadkey(
        int tileX,
        int tileY,
        int zoomLevel,
        string expectedQuadkey)
    {
        string quadkey = QuadkeyConverter.TileXYToQuadkey(
            tileX,
            tileY,
            zoomLevel);

        Assert.Equal(expectedQuadkey, quadkey);
        Assert.Equal((tileX, tileY, zoomLevel), QuadkeyConverter.QuadkeyToTileXY(quadkey));
    }

    [Fact]
    public void CreateSourceDefinition_BingLiveTiles_UsesVirtualEarthQuadkeyXml()
    {
        XyzTileSourceService service = new();
        XyzTileSourceImportRequest request = new(
            "Bing Aerial",
            "http://ecn.t3.tiles.virtualearth.net/tiles/a{quadkey}.jpeg?g=1",
            -180,
            -85.05112878,
            180,
            85.05112878,
            20,
            "jpg",
            IsLiveTiles: true);

        XyzTileSourceDefinition definition =
            service.CreateSourceDefinition(_projectFolder, request);

        string xml = File.ReadAllText(definition.DefinitionPath);
        Assert.Contains("<Service name=\"VirtualEarth\">", xml);
        Assert.Contains("a${quadkey}.jpeg?g=1", xml);
        Assert.Contains("<Projection>EPSG:3857</Projection>", xml);
        Assert.Contains("<TileLevel>20</TileLevel>", xml);
        Assert.Equal("EPSG:3857", definition.SourceExtent.SrsDefinition);
    }

    [Fact]
    public void BuildTileUrl_BingTemplate_ReplacesQuadkey()
    {
        MethodInfo method = typeof(XyzTilePreDownloadService).GetMethod(
            "BuildTileUrl",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("BuildTileUrl method not found.");

        string url = (string)method.Invoke(
            null,
            ["http://ecn.t3.tiles.virtualearth.net/tiles/a{quadkey}.jpeg?g=1", 3, 3, 5])!;

        Assert.Equal(
            "http://ecn.t3.tiles.virtualearth.net/tiles/a213.jpeg?g=1",
            url);
    }

    [Fact]
    public void LiveTileRendererBuildTileUrl_BingTemplate_ReplacesQuadkey()
    {
        Type renderLayerType = typeof(GdalRasterDatasetImporter).Assembly.GetType(
            "Land_Readjustment_Tool.UI.MapCanvas.Rendering.XyzLiveTileRenderLayer")
            ?? throw new InvalidOperationException("XyzLiveTileRenderLayer type not found.");
        MethodInfo method = renderLayerType.GetMethod(
            "BuildTileUrl",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            [typeof(string), typeof(int), typeof(int), typeof(int)],
            modifiers: null)
            ?? throw new InvalidOperationException("BuildTileUrl method not found.");

        string url = (string)method.Invoke(
            null,
            ["http://ecn.t3.tiles.virtualearth.net/tiles/a${quadkey}.jpeg?g=1", 3, 3, 5])!;

        Assert.Equal(
            "http://ecn.t3.tiles.virtualearth.net/tiles/a213.jpeg?g=1",
            url);
    }

    [Fact]
    public void LiveTileRendererBuildDiskCacheRoot_BingTemplate_AvoidsLegacyUnexpandedQuadkeyCache()
    {
        Type renderLayerType = typeof(GdalRasterDatasetImporter).Assembly.GetType(
            "Land_Readjustment_Tool.UI.MapCanvas.Rendering.XyzLiveTileRenderLayer")
            ?? throw new InvalidOperationException("XyzLiveTileRenderLayer type not found.");
        MethodInfo method = renderLayerType.GetMethod(
            "BuildDiskCacheRoot",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("BuildDiskCacheRoot method not found.");

        const string urlTemplate =
            "http://ecn.t3.tiles.virtualearth.net/tiles/a${quadkey}.jpeg?g=1";
        string root = (string)method.Invoke(null, [urlTemplate])!;
        string legacyRoot = BuildLegacyLiveCacheRoot(urlTemplate);

        Assert.NotEqual(legacyRoot, root);
    }

    [Fact]
    public void LiveTileRendererBuildDiskCacheRoot_StandardXyzTemplate_AvoidsLegacyMixedZoomCache()
    {
        Type renderLayerType = typeof(GdalRasterDatasetImporter).Assembly.GetType(
            "Land_Readjustment_Tool.UI.MapCanvas.Rendering.XyzLiveTileRenderLayer")
            ?? throw new InvalidOperationException("XyzLiveTileRenderLayer type not found.");
        MethodInfo method = renderLayerType.GetMethod(
            "BuildDiskCacheRoot",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("BuildDiskCacheRoot method not found.");

        const string urlTemplate =
            "https://tile.openstreetmap.org/${z}/${x}/${y}.png";
        string root = (string)method.Invoke(null, [urlTemplate])!;

        Assert.NotEqual(BuildLegacyLiveCacheRoot(urlTemplate), root);
    }

    [Theory]
    [InlineData("http://ecn.t3.tiles.virtualearth.net/tiles/a${quadkey}.jpeg?g=1", true)]
    [InlineData("https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/${z}/${y}/${x}", true)]
    [InlineData("https://tile.openstreetmap.org/${z}/${x}/${y}.png", true)]
    public void LiveTileRendererShouldAllowParentPlaceholders_AllowsCompleteCachedParentFallback(
        string urlTemplate,
        bool expected)
    {
        Type renderLayerType = typeof(GdalRasterDatasetImporter).Assembly.GetType(
            "Land_Readjustment_Tool.UI.MapCanvas.Rendering.XyzLiveTileRenderLayer")
            ?? throw new InvalidOperationException("XyzLiveTileRenderLayer type not found.");
        MethodInfo method = renderLayerType.GetMethod(
            "ShouldAllowParentPlaceholders",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ShouldAllowParentPlaceholders method not found.");

        bool actual = (bool)method.Invoke(null, [urlTemplate])!;

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("http://ecn.t3.tiles.virtualearth.net/tiles/a${quadkey}.jpeg?g=1", 20, 14)]
    [InlineData("http://ecn.t3.tiles.virtualearth.net/tiles/a${quadkey}.jpeg?g=1", 12, 12)]
    [InlineData("https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/${z}/${y}/${x}", 19, 19)]
    public void LiveTileRendererGetEffectiveMaxSourceZoom_CapsBingLiveFetchesOnly(
        string urlTemplate,
        int requestedMaxZoom,
        int expectedMaxZoom)
    {
        Type renderLayerType = typeof(GdalRasterDatasetImporter).Assembly.GetType(
            "Land_Readjustment_Tool.UI.MapCanvas.Rendering.XyzLiveTileRenderLayer")
            ?? throw new InvalidOperationException("XyzLiveTileRenderLayer type not found.");
        MethodInfo method = renderLayerType.GetMethod(
            "GetEffectiveMaxSourceZoom",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("GetEffectiveMaxSourceZoom method not found.");

        int actual = (int)method.Invoke(null, [urlTemplate, requestedMaxZoom])!;

        Assert.Equal(expectedMaxZoom, actual);
    }

    private static string BuildLegacyLiveCacheRoot(string urlTemplate)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(urlTemplate));
        string prefix = Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
        return Path.Combine(Path.GetTempPath(), "replot-live-tiles", prefix);
    }

    public void Dispose()
    {
        if (Directory.Exists(_projectFolder))
            Directory.Delete(_projectFolder, recursive: true);
    }
}
