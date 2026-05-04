using Microsoft.Data.Sqlite;
using System.Net.Http;
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

            TileRange tileRange = BuildTileRange(request);
            long totalTiles = tileRange.TotalTiles;
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
                    response.EnsureSuccessStatusCode();

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
            string signature = $"{safeLayerName}_z{request.ZoomLevel}_{safeToken}";

            return Path.Combine(
                projectFolderPath,
                DownloadCacheFolderName,
                signature);
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
            normalized = ReplaceToken(normalized, "z", zoomLevel.ToString());
            normalized = ReplaceToken(normalized, "x", tileX.ToString());
            normalized = ReplaceToken(normalized, "y", tileY.ToString());
            return normalized;
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
            double latitudeRadians = latitude * Math.PI / 180d;
            int tileCount = 1 << zoomLevel;
            int tileY = (int)Math.Floor(
                (1d - Math.Log(
                    Math.Tan(latitudeRadians) + 1d / Math.Cos(latitudeRadians)) / Math.PI) /
                2d * tileCount);
            return Math.Clamp(tileY, 0, tileCount - 1);
        }

        private readonly record struct TileRange(
            int MinTileX,
            int MaxTileX,
            int MinTileY,
            int MaxTileY,
            long TotalTiles);
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
