using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.Data.Sqlite;
using OSGeo.GDAL;
using OSGeo.OSR;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Downloads selected XYZ tiles to a local cache and reports progress.
    /// </summary>
    public sealed class XyzTilePreDownloadService
    {
        private const string DownloadCacheFolderName = "XyzTilePreDownloadCache";
        private const int MaxConcurrentDownloads = 8;
        private const int TilePixelSize = 256;
        private const int DefaultMaxTileCount = 5000;
        private const double WebMercatorOriginShift = 20037508.342789244;
        private const double InitialResolution = 156543.03392804097;
        private const string GoogleOfflineDownloadNotSupportedMessage =
            "Google imagery cannot be pre-downloaded or packaged into MBTiles/GeoTIFF by this tool. Google Maps Platform policies restrict pre-fetching, storage, caching, and offline uses except under limited agreement terms. Use live Google tiles with proper credentials, or choose a tile source whose terms allow offline export.";
        private static readonly HttpClient HttpClient = new();

        public async Task<XyzTileDownloadResult> DownloadTilesAsync(
            string projectFolderPath,
            XyzTileSourceImportRequest request,
            IProgress<XyzTileDownloadProgress>? progress = null,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(projectFolderPath) ||
                !Directory.Exists(projectFolderPath))
            {
                throw new DirectoryNotFoundException("Project folder was not found.");
            }

            if (IsGoogleTileSource(request.UrlTemplate))
            {
                throw new InvalidOperationException(GoogleOfflineDownloadNotSupportedMessage);
            }

            TileRange tileRange = BuildTileRange(request);
            long totalTiles = tileRange.TotalTiles;
            if (totalTiles > DefaultMaxTileCount)
            {
                throw new InvalidOperationException(
                    $"The selected area and zoom level require {totalTiles:N0} tiles. " +
                    "Please reduce the area or choose a lower zoom level.");
            }
            string cacheFolder = BuildCacheFolderPath(projectFolderPath, request);
            Directory.CreateDirectory(cacheFolder);

            long completed = 0;
            progress?.Report(CreateProgress(
                completed,
                totalTiles,
                "Preparing tile download"));

            using SemaphoreSlim throttler = new(MaxConcurrentDownloads);
            List<Task> inFlight = [];

            for (int tileY = tileRange.MinTileY; tileY <= tileRange.MaxTileY; tileY++)
            {
                for (int tileX = tileRange.MinTileX; tileX <= tileRange.MaxTileX; tileX++)
                {
                    ct.ThrowIfCancellationRequested();
                    await throttler.WaitAsync(ct);

                    int scheduledX = tileX;
                    int scheduledY = tileY;
                    Task task = DownloadOneTileAsync(
                        request,
                        cacheFolder,
                        scheduledX,
                        scheduledY,
                        throttler,
                        () =>
                        {
                            long current = Interlocked.Increment(ref completed);
                            progress?.Report(CreateProgress(
                                current,
                                totalTiles,
                                $"Downloading XYZ tiles ({current:N0}/{totalTiles:N0})"));
                        },
                        ct);

                    inFlight.Add(task);
                    if (inFlight.Count >= MaxConcurrentDownloads * 6)
                    {
                        Task finished = await Task.WhenAny(inFlight);
                        inFlight.Remove(finished);
                        await finished;
                    }
                }
            }

            await Task.WhenAll(inFlight);
            progress?.Report(CreateProgress(totalTiles, totalTiles, "Tile download complete"));

            return new XyzTileDownloadResult(totalTiles, totalTiles, cacheFolder);
        }

        /// <summary>
        /// Assembles downloaded XYZ tile files from the cache folder into a
        /// single MBTiles SQLite file. Tiles are stored using TMS row convention
        /// (Y flipped) which is what MbTilesRenderLayer expects by default.
        /// </summary>
        public static string AssembleDownloadedTilesIntoMbTiles(
            XyzTileDownloadResult downloadResult,
            XyzTileSourceImportRequest request,
            string outputMbTilesPath,
            IProgress<XyzTileDownloadProgress>? progress = null)
        {
            string cacheFolder = downloadResult.CacheFolderPath;
            int zoom = request.ZoomLevel;
            string extension = NormalizeImageExtension(request.ImageExtension);
            string format = extension.TrimStart('.');
            int tileCount = 1 << zoom;

            SqliteConnectionStringBuilder builder = new()
            {
                DataSource = outputMbTilesPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            using SqliteConnection connection = new(builder.ToString());
            connection.Open();

            // Create MBTiles schema
            using (SqliteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS metadata (name TEXT, value TEXT);
            CREATE TABLE IF NOT EXISTS tiles (
                zoom_level  INTEGER NOT NULL,
                tile_column INTEGER NOT NULL,
                tile_row    INTEGER NOT NULL,
                tile_data   BLOB    NOT NULL,
                PRIMARY KEY (zoom_level, tile_column, tile_row));
            """;
                cmd.ExecuteNonQuery();
            }

            // Write metadata
            using (SqliteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = """
            INSERT OR REPLACE INTO metadata (name, value) VALUES
                ('name',    $name),
                ('format',  $format),
                ('minzoom', $zoom),
                ('maxzoom', $zoom),
                ('scheme',  'tms'),
                ('bounds',  $bounds);
            """;
                cmd.Parameters.AddWithValue("$name", request.LayerName);
                cmd.Parameters.AddWithValue("$format", format);
                cmd.Parameters.AddWithValue("$zoom", zoom);
                cmd.Parameters.AddWithValue("$bounds",
                    $"{request.MinLongitude},{request.MinLatitude},{request.MaxLongitude},{request.MaxLatitude}");
                cmd.ExecuteNonQuery();
            }

            // Insert tiles inside a transaction for performance
            using SqliteTransaction transaction = connection.BeginTransaction();
            using SqliteCommand insertCmd = connection.CreateCommand();
            insertCmd.Transaction = transaction;
            insertCmd.CommandText = """
        INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data)
        VALUES ($zoom, $x, $row, $data)
        """;
            insertCmd.Parameters.Add("$zoom", SqliteType.Integer);
            insertCmd.Parameters.Add("$x", SqliteType.Integer);
            insertCmd.Parameters.Add("$row", SqliteType.Integer);
            insertCmd.Parameters.Add("$data", SqliteType.Blob);

            long inserted = 0;
            long total = downloadResult.TotalTiles;

            // Walk the cache folder: zoom/x/y.ext
            string zoomFolder = Path.Combine(cacheFolder, zoom.ToString());
            if (Directory.Exists(zoomFolder))
            {
                foreach (string xFolder in Directory.EnumerateDirectories(zoomFolder))
                {
                    if (!int.TryParse(Path.GetFileName(xFolder), out int tileX))
                        continue;

                    foreach (string tileFile in Directory.EnumerateFiles(xFolder, $"*{extension}"))
                    {
                        string yStr = Path.GetFileNameWithoutExtension(tileFile);
                        if (!int.TryParse(yStr, out int tileY))
                            continue;

                        byte[] tileData = File.ReadAllBytes(tileFile);
                        if (tileData.Length == 0)
                            continue;

                        // Convert XYZ Y to TMS row (flip Y axis)
                        int tmsRow = tileCount - 1 - tileY;

                        insertCmd.Parameters["$zoom"].Value = zoom;
                        insertCmd.Parameters["$x"].Value = tileX;
                        insertCmd.Parameters["$row"].Value = tmsRow;
                        insertCmd.Parameters["$data"].Value = tileData;
                        insertCmd.ExecuteNonQuery();

                        inserted++;
                        if (inserted % 100 == 0)
                        {
                            progress?.Report(new XyzTileDownloadProgress(
                                inserted, total,
                                (int)(inserted * 100 / Math.Max(1, total)),
                                $"Packaging tiles ({inserted:N0}/{total:N0})"));
                        }
                    }
                }
            }

            transaction.Commit();

            return outputMbTilesPath;
        }

        public static string AssembleDownloadedTilesIntoGeoTiff(
            XyzTileDownloadResult downloadResult,
            XyzTileSourceImportRequest request,
            string outputTiffPath,
            IProgress<XyzTileDownloadProgress>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(outputTiffPath))
            {
                throw new ArgumentException("Output GeoTIFF path is required.", nameof(outputTiffPath));
            }

            TileRange tileRange = BuildTileRange(request);
            long totalTiles = tileRange.TotalTiles;
            if (totalTiles > DefaultMaxTileCount)
            {
                throw new InvalidOperationException(
                    $"The selected area and zoom level require {totalTiles:N0} tiles. " +
                    "Please reduce the area or choose a lower zoom level.");
            }

            int tileCountX = tileRange.MaxTileX - tileRange.MinTileX + 1;
            int tileCountY = tileRange.MaxTileY - tileRange.MinTileY + 1;
            int mosaicWidth = tileCountX * TilePixelSize;
            int mosaicHeight = tileCountY * TilePixelSize;

            if (mosaicWidth <= 0 || mosaicHeight <= 0)
            {
                throw new InvalidOperationException("Invalid tile range for GeoTIFF assembly.");
            }

            string cacheFolder = downloadResult.CacheFolderPath;
            string extension = NormalizeImageExtension(request.ImageExtension);
            using Bitmap mosaic = new(mosaicWidth, mosaicHeight, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(mosaic))
            {
                graphics.Clear(Color.Transparent);

                long inserted = 0;
                for (int tileY = tileRange.MinTileY; tileY <= tileRange.MaxTileY; tileY++)
                {
                    for (int tileX = tileRange.MinTileX; tileX <= tileRange.MaxTileX; tileX++)
                    {
                        string tilePath = Path.Combine(
                            cacheFolder,
                            request.ZoomLevel.ToString(),
                            tileX.ToString(),
                            $"{tileY}{extension}");

                        if (!File.Exists(tilePath))
                        {
                            continue;
                        }

                        using Image tile = Image.FromFile(tilePath);
                        int pixelX = (tileX - tileRange.MinTileX) * TilePixelSize;
                        int pixelY = (tileY - tileRange.MinTileY) * TilePixelSize;
                        graphics.DrawImage(tile, pixelX, pixelY, TilePixelSize, TilePixelSize);

                        inserted++;
                        if (inserted % 100 == 0)
                        {
                            progress?.Report(new XyzTileDownloadProgress(
                                inserted,
                                totalTiles,
                                (int)(inserted * 100 / Math.Max(1, totalTiles)),
                                $"Stitching tiles ({inserted:N0}/{totalTiles:N0})"));
                        }
                    }
                }
            }

            WebMercatorBounds bounds = TileRangeToWebMercatorBounds(tileRange, request.ZoomLevel);
            WriteGeoTiff(outputTiffPath, mosaic, bounds);
            return outputTiffPath;
        }

        private static XyzTileDownloadProgress CreateProgress(
            long completed,
            long total,
            string status)
        {
            int percent = total <= 0
                ? 0
                : (int)Math.Clamp(
                    Math.Round(completed * 100.0 / total, MidpointRounding.AwayFromZero),
                    0,
                    100);

            return new XyzTileDownloadProgress(completed, total, percent, status);
        }

        private static async Task DownloadOneTileAsync(
            XyzTileSourceImportRequest request,
            string cacheFolder,
            int tileX,
            int tileY,
            SemaphoreSlim throttler,
            Action reportCompleted,
            CancellationToken ct)
        {
            try
            {
                string extension = NormalizeImageExtension(request.ImageExtension);
                string tileDirectory = Path.Combine(
                    cacheFolder,
                    request.ZoomLevel.ToString(),
                    tileX.ToString());
                Directory.CreateDirectory(tileDirectory);

                string tilePath = Path.Combine(tileDirectory, $"{tileY}{extension}");
                if (!File.Exists(tilePath))
                {
                    string url = BuildTileUrl(
                        request.UrlTemplate,
                        request.ZoomLevel,
                        tileX,
                        tileY);

                    using HttpResponseMessage response = await HttpClient.GetAsync(
                        url,
                        HttpCompletionOption.ResponseHeadersRead,
                        ct);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            BuildTileHttpFailureMessage(response.StatusCode, tileX, tileY, request.ZoomLevel),
                            inner: null,
                            response.StatusCode);
                    }

                    await using Stream responseStream =
                        await response.Content.ReadAsStreamAsync(ct);
                    await using FileStream fileStream = new(
                        tilePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        useAsync: true);
                    await responseStream.CopyToAsync(fileStream, ct);
                }

                reportCompleted();
            }
            finally
            {
                throttler.Release();
            }
        }

        private static string BuildCacheFolderPath(
            string projectFolderPath,
            XyzTileSourceImportRequest request)
        {
            string boundsToken =
                $"{request.MinLongitude:F6}_{request.MinLatitude:F6}_{request.MaxLongitude:F6}_{request.MaxLatitude:F6}";
            string safeToken = SanitizeFileName(boundsToken);
            string safeLayerName = SanitizeFileName(request.LayerName);
            string sourceToken = BuildSourceCacheToken(request.UrlTemplate);
            string signature =
                $"{safeLayerName}_z{request.ZoomLevel}_{sourceToken}_{safeToken}";

            return Path.Combine(
                projectFolderPath,
                DownloadCacheFolderName,
                signature);
        }

        private static string BuildSourceCacheToken(string urlTemplate)
        {
            string normalized = urlTemplate.Trim();
            if (ContainsTileToken(normalized, "quadkey"))
            {
                normalized += "|quadkey-expanded-cache-v2";
            }

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToHexString(hash, 0, 6).ToLowerInvariant();
        }

        private static TileRange BuildTileRange(XyzTileSourceImportRequest request)
        {
            int minTileX = LongitudeToTileX(request.MinLongitude, request.ZoomLevel);
            int maxTileX = LongitudeToTileX(request.MaxLongitude, request.ZoomLevel);
            int minTileY = LatitudeToTileY(request.MaxLatitude, request.ZoomLevel);
            int maxTileY = LatitudeToTileY(request.MinLatitude, request.ZoomLevel);

            long width = (long)maxTileX - minTileX + 1L;
            long height = (long)maxTileY - minTileY + 1L;
            long total = width * height;

            return new TileRange(minTileX, maxTileX, minTileY, maxTileY, total);
        }

        private static string BuildTileUrl(
            string urlTemplate,
            int zoomLevel,
            int tileX,
            int tileY)
        {
            string normalized = urlTemplate.Trim();
            if (ContainsTileToken(normalized, "quadkey"))
            {
                string quadkey = QuadkeyConverter.TileXYToQuadkey(
                    tileX,
                    tileY,
                    zoomLevel);
                normalized = ReplaceToken(normalized, "quadkey", quadkey);
            }

            normalized = ReplaceToken(normalized, "z", zoomLevel.ToString());
            normalized = ReplaceToken(normalized, "x", tileX.ToString());
            normalized = ReplaceToken(normalized, "y", tileY.ToString());
            return normalized;
        }

        private static bool ContainsTileToken(string urlTemplate, string token)
        {
            return urlTemplate.Contains($"{{{token}}}", StringComparison.OrdinalIgnoreCase) ||
                   urlTemplate.Contains($"${{{token}}}", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsGoogleTileSource(string urlTemplate)
        {
            return urlTemplate.Contains("google.com", StringComparison.OrdinalIgnoreCase) ||
                   urlTemplate.Contains("googleapis.com", StringComparison.OrdinalIgnoreCase) ||
                   urlTemplate.Contains("gstatic.com", StringComparison.OrdinalIgnoreCase);
        }

        private static string ReplaceToken(
            string value,
            string token,
            string replacement)
        {
            return value
                .Replace($"{{{token}}}", replacement, StringComparison.OrdinalIgnoreCase)
                .Replace($"${{{token}}}", replacement, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildTileHttpFailureMessage(
            HttpStatusCode statusCode,
            int tileX,
            int tileY,
            int zoomLevel)
        {
            int statusCodeNumber = (int)statusCode;
            string message =
                $"Response status code does not indicate success: {statusCodeNumber} ({statusCode}). " +
                $"Tile z={zoomLevel}, x={tileX}, y={tileY} could not be downloaded.";

            return XyzTileErrorMessageBuilder.AddUserGuidance(message);
        }

        private static string NormalizeImageExtension(string imageExtension)
        {
            string extension = string.IsNullOrWhiteSpace(imageExtension)
                ? ".png"
                : imageExtension.Trim().ToLowerInvariant();

            if (!extension.StartsWith('.'))
                extension = $".{extension}";

            return extension is ".png" or ".jpg" or ".jpeg"
                ? extension
                : ".png";
        }

        private static string SanitizeFileName(string input)
        {
            StringBuilder builder = new(input.Length);
            HashSet<char> invalidChars = Path.GetInvalidFileNameChars().ToHashSet();

            foreach (char character in input)
            {
                builder.Append(invalidChars.Contains(character) ? '_' : character);
            }

            return builder.ToString();
        }

        private static int LongitudeToTileX(double longitude, int zoomLevel)
        {
            int tileCount = 1 << zoomLevel;
            int tileX = (int)Math.Floor((longitude + 180d) / 360d * tileCount);
            return Math.Clamp(tileX, 0, tileCount - 1);
        }

        private static int LatitudeToTileY(double latitude, int zoomLevel)
        {
            latitude = Math.Clamp(latitude, -85.05112878, 85.05112878);
            double latitudeRadians = latitude * Math.PI / 180d;
            int tileCount = 1 << zoomLevel;
            int tileY = (int)Math.Floor(
                (1d - Math.Log(
                    Math.Tan(latitudeRadians) + 1d / Math.Cos(latitudeRadians)) / Math.PI) /
                2d * tileCount);
            return Math.Clamp(tileY, 0, tileCount - 1);
        }

        private static double GetResolution(int zoom) =>
            InitialResolution / Math.Pow(2.0, zoom);

        private static WebMercatorBounds TileRangeToWebMercatorBounds(
            TileRange tileRange,
            int zoom)
        {
            double resolution = GetResolution(zoom);
            double minX = tileRange.MinTileX * TilePixelSize * resolution - WebMercatorOriginShift;
            double maxX = (tileRange.MaxTileX + 1) * TilePixelSize * resolution - WebMercatorOriginShift;
            double maxY = WebMercatorOriginShift - tileRange.MinTileY * TilePixelSize * resolution;
            double minY = WebMercatorOriginShift - (tileRange.MaxTileY + 1) * TilePixelSize * resolution;
            return new WebMercatorBounds(minX, minY, maxX, maxY, resolution);
        }

        private static void WriteGeoTiff(
            string outputPath,
            Bitmap bitmap,
            WebMercatorBounds bounds)
        {
            GdalConfiguration.ConfigureGdal();
            if (!GdalConfiguration.Usable)
            {
                throw new InvalidOperationException(
                    "GDAL is not configured correctly. GeoTIFF export cannot continue.");
            }

            Driver? driver = Gdal.GetDriverByName("GTiff");
            if (driver == null)
            {
                throw new InvalidOperationException("GDAL GTiff driver is not available.");
            }

            string[] options =
            [
                "TILED=YES",
                "BIGTIFF=IF_SAFER",
                "COMPRESS=DEFLATE"
            ];

            using Dataset dataset = driver.Create(
                outputPath,
                bitmap.Width,
                bitmap.Height,
                4,
                DataType.GDT_Byte,
                options);

            double[] geoTransform =
            [
                bounds.MinX,
                bounds.Resolution,
                0,
                bounds.MaxY,
                0,
                -bounds.Resolution
            ];
            dataset.SetGeoTransform(geoTransform);

            using SpatialReference srs = new(string.Empty);
            srs.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            srs.ImportFromEPSG(3857);
            srs.ExportToWkt(out string? wkt, null);
            if (!string.IsNullOrWhiteSpace(wkt))
            {
                dataset.SetProjection(wkt);
            }

            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int bytes = Math.Abs(data.Stride) * data.Height;
                byte[] buffer = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, bytes);

                byte[] red = new byte[bitmap.Width * bitmap.Height];
                byte[] green = new byte[bitmap.Width * bitmap.Height];
                byte[] blue = new byte[bitmap.Width * bitmap.Height];
                byte[] alpha = new byte[bitmap.Width * bitmap.Height];

                int index = 0;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int rowOffset = y * data.Stride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int offset = rowOffset + x * 4;
                        blue[index] = buffer[offset];
                        green[index] = buffer[offset + 1];
                        red[index] = buffer[offset + 2];
                        alpha[index] = buffer[offset + 3];
                        index++;
                    }
                }

                dataset.GetRasterBand(1).WriteRaster(0, 0, bitmap.Width, bitmap.Height, red, bitmap.Width, bitmap.Height, 0, 0);
                dataset.GetRasterBand(2).WriteRaster(0, 0, bitmap.Width, bitmap.Height, green, bitmap.Width, bitmap.Height, 0, 0);
                dataset.GetRasterBand(3).WriteRaster(0, 0, bitmap.Width, bitmap.Height, blue, bitmap.Width, bitmap.Height, 0, 0);
                dataset.GetRasterBand(4).WriteRaster(0, 0, bitmap.Width, bitmap.Height, alpha, bitmap.Width, bitmap.Height, 0, 0);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            dataset.FlushCache();
        }

        private readonly record struct TileRange(
            int MinTileX,
            int MaxTileX,
            int MinTileY,
            int MaxTileY,
            long TotalTiles);

        private readonly record struct WebMercatorBounds(
            double MinX,
            double MinY,
            double MaxX,
            double MaxY,
            double Resolution);
    }

    public sealed record XyzTileDownloadProgress(
        long CompletedTiles,
        long TotalTiles,
        int Percent,
        string Status);

    public sealed record XyzTileDownloadResult(
        long DownloadedTiles,
        long TotalTiles,
        string CacheFolderPath);
}
