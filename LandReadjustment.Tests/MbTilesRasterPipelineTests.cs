using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Services.Raster;
using Microsoft.Data.Sqlite;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class MbTilesRasterPipelineTests : IDisposable
{
    private const int TileSize = 256;
    private readonly string _tempDir;

    public MbTilesRasterPipelineTests()
    {
        SQLitePCL.Batteries_V2.Init();
        _tempDir = Path.Combine(Path.GetTempPath(), $"RePlotMbTilesTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void DirectRenderer_RejectsNonWebMercatorProjectTarget()
    {
        string mbTilesPath = CreateTinyMbTiles("reject-non-webmercator.mbtiles", "tms");
        WriteMbTilesSidecar(mbTilesPath, "EPSG:3857", "EPSG:32645");

        CanvasLayer layer = new()
        {
            Id = 1,
            Name = "Test MBTiles",
            LayerType = "RasterLayer",
            SourceFile = mbTilesPath,
            IsVisible = true
        };

        Type renderLayerType = typeof(GdalRasterDatasetImporter).Assembly.GetType(
            "Land_Readjustment_Tool.UI.MapCanvas.Rendering.MbTilesRenderLayer")!;
        MethodInfo fromCanvasLayer = renderLayerType.GetMethod(
            "FromCanvasLayer",
            BindingFlags.Public | BindingFlags.Static)!;

        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
            () => fromCanvasLayer.Invoke(null, [layer, mbTilesPath]));

        Assert.IsType<NotSupportedException>(exception.InnerException);
    }

    [Fact]
    public void ImportToProjectCrs_WebMercatorTarget_PreservesDirectMbTiles()
    {
        string mbTilesPath = CreateTinyMbTiles("direct-webmercator.mbtiles", "tms");
        GdalRasterDatasetImporter importer = new();

        RasterDatasetImportOutput result = importer.ImportToProjectCrs(
            mbTilesPath,
            _tempDir,
            "Direct",
            "EPSG:3857");

        Assert.Equal(RasterDatasetImportMode.MbTilesDirectTileSource, result.ImportMode);
        Assert.Equal(".mbtiles", Path.GetExtension(result.AbsolutePath));
        Assert.True(File.Exists(result.AbsolutePath));
    }

    [Fact]
    public void ImportToProjectCrs_NonWebMercatorTarget_WarpsInsteadOfDirectMbTiles()
    {
        string mbTilesPath = CreateTinyMbTiles("warped-utm.mbtiles", "tms");
        GdalRasterDatasetImporter importer = new();

        RasterDatasetImportOutput result = importer.ImportToProjectCrs(
            mbTilesPath,
            _tempDir,
            "Warped",
            "EPSG:32645");

        Assert.NotEqual(RasterDatasetImportMode.MbTilesDirectTileSource, result.ImportMode);
        Assert.Equal(".tif", Path.GetExtension(result.AbsolutePath));
        Assert.True(File.Exists(result.AbsolutePath));
    }

    [Fact]
    public void ReprojectProjectRasterToProjectCrs_NonWebMercatorMbTiles_CreatesProjectedVrt()
    {
        string mbTilesPath = CreateTinyMbTiles("refresh-crs.mbtiles", "tms");
        WriteMbTilesSidecar(mbTilesPath, "EPSG:3857", "EPSG:3857");
        GdalRasterDatasetImporter importer = new();

        RasterProjectReprojectionResult result =
            importer.TryReprojectProjectRasterToProjectCrs(
                mbTilesPath,
                "EPSG:32645");

        Assert.True(result.Reprojected);
        Assert.NotNull(result.UpdatedRasterPath);
        Assert.Equal(".vrt", Path.GetExtension(result.UpdatedRasterPath));
        Assert.True(File.Exists(result.UpdatedRasterPath));
        Assert.True(File.Exists(mbTilesPath));
    }

    private string CreateTinyMbTiles(string fileName, string scheme)
    {
        string path = Path.Combine(_tempDir, fileName);
        (int x, int y, int z) = LonLatToTile(85.3240, 27.7172, 14);
        int storedY = string.Equals(scheme, "xyz", StringComparison.OrdinalIgnoreCase)
            ? y
            : (1 << z) - 1 - y;

        byte[] tileData = CreateTilePng();
        using SqliteConnection connection = new($"Data Source={path}");
        connection.Open();
        ExecuteSql(connection, "CREATE TABLE metadata (name TEXT, value TEXT)");
        ExecuteSql(connection, "CREATE TABLE tiles (zoom_level INTEGER, tile_column INTEGER, tile_row INTEGER, tile_data BLOB)");
        ExecuteSql(connection, "CREATE UNIQUE INDEX tile_index ON tiles(zoom_level, tile_column, tile_row)");

        InsertMetadata(connection, "name", "Tiny Test");
        InsertMetadata(connection, "type", "baselayer");
        InsertMetadata(connection, "version", "1.1");
        InsertMetadata(connection, "format", "png");
        InsertMetadata(connection, "scheme", scheme);
        InsertMetadata(connection, "bounds", TileBounds(x, y, z));

        using SqliteCommand insertTile = connection.CreateCommand();
        insertTile.CommandText = """
            INSERT INTO tiles (zoom_level, tile_column, tile_row, tile_data)
            VALUES ($z, $x, $y, $data)
            """;
        insertTile.Parameters.AddWithValue("$z", z);
        insertTile.Parameters.AddWithValue("$x", x);
        insertTile.Parameters.AddWithValue("$y", storedY);
        insertTile.Parameters.Add("$data", SqliteType.Blob).Value = tileData;
        insertTile.ExecuteNonQuery();

        return path;
    }

    private static byte[] CreateTilePng()
    {
        using Bitmap bitmap = new(TileSize, TileSize, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.FromArgb(255, 56, 132, 88));
            using Brush brush = new SolidBrush(Color.FromArgb(255, 230, 240, 220));
            graphics.FillRectangle(brush, 32, 32, 192, 192);
        }

        using MemoryStream stream = new();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    private static void InsertMetadata(SqliteConnection connection, string name, string value)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO metadata (name, value) VALUES ($name, $value)";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$value", value);
        command.ExecuteNonQuery();
    }

    private static void ExecuteSql(SqliteConnection connection, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static (int X, int Y, int Z) LonLatToTile(double lon, double lat, int zoom)
    {
        double latRadians = lat * Math.PI / 180.0;
        int scale = 1 << zoom;
        int x = (int)Math.Floor((lon + 180.0) / 360.0 * scale);
        int y = (int)Math.Floor(
            (1.0 - Math.Log(Math.Tan(latRadians) + 1.0 / Math.Cos(latRadians)) / Math.PI) / 2.0 * scale);
        return (x, y, zoom);
    }

    private static string TileBounds(int x, int y, int zoom)
    {
        (double west, double north) = TileToLonLat(x, y, zoom);
        (double east, double south) = TileToLonLat(x + 1, y + 1, zoom);
        return string.Join(
            ',',
            west.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            south.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            east.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            north.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
    }

    private static (double Lon, double Lat) TileToLonLat(int x, int y, int zoom)
    {
        double scale = 1 << zoom;
        double lon = x / scale * 360.0 - 180.0;
        double n = Math.PI - 2.0 * Math.PI * y / scale;
        double lat = 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
        return (lon, lat);
    }

    private static void WriteMbTilesSidecar(
        string mbTilesPath,
        string sourceSrsDefinition,
        string targetSrsDefinition)
    {
        string sidecarPath = $"{mbTilesPath}.replot-mbtiles.json";
        var metadata = new
        {
            Kind = "MBTiles",
            SourceSrsDefinition = sourceSrsDefinition,
            TargetSrsDefinition = targetSrsDefinition,
            OriginalSourcePath = mbTilesPath,
            CreatedUtc = DateTime.UtcNow
        };
        File.WriteAllText(sidecarPath, JsonSerializer.Serialize(metadata));
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { }
    }
}
