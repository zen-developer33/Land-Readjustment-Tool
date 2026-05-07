using System.Globalization;
using System.Security;
using System.Text;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Builds GDAL WMS/TMS source definitions for common web XYZ tile servers.
    /// </summary>
    public sealed class XyzTileSourceService : IXyzTileSourceService
    {
        private const string XyzSourceFolderName = "XyzTileSources";
        private const string XyzCacheFolderName = "XyzTileCache";
        private const int TileSize = 256;
        private const long MaximumTileRequestCount = 2_000_000;
        private const double WebMercatorMaximumLatitude = 85.05112878;
        private const double WebMercatorOriginShift = 20037508.342789244;
        private const string DefaultUserAgent =
            "RePlot Land Readjustment Tool/1.0 (XYZ raster import)";
        private static readonly Encoding Utf8NoBom =
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        /// <inheritdoc />
        public XyzTileSourceDefinition CreateSourceDefinition(
            string projectFolderPath,
            XyzTileSourceImportRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(projectFolderPath) ||
                !Directory.Exists(projectFolderPath))
            {
                throw new DirectoryNotFoundException(
                    "Project folder was not found.");
            }

            ValidateRequest(request);

            string sourceFolder = Path.Combine(projectFolderPath, XyzSourceFolderName);
            string cacheFolder = Path.Combine(projectFolderPath, XyzCacheFolderName);
            Directory.CreateDirectory(sourceFolder);
            Directory.CreateDirectory(cacheFolder);

            string normalizedUrl = NormalizeUrlTemplate(request.UrlTemplate);
            int tileLevel = request.IsLiveTiles
                ? ResolvePracticalLiveTileMaxZoom(normalizedUrl, request.ZoomLevel)
                : request.ZoomLevel;
            RasterSourceExtent sourceExtent = request.IsLiveTiles
                ? BuildGlobalSourceExtent()
                : BuildSourceExtent(request);
            long tileCount = request.IsLiveTiles ? 0L : CalculateTileCount(request);
            string xmlPath = GetUniquePath(
                Path.Combine(
                    sourceFolder,
                    $"{SanitizeFileName(request.LayerName)}.gdal-wms.xml"));

            File.WriteAllText(
                xmlPath,
                request.IsLiveTiles
                    ? BuildGdalWmsXmlLive(normalizedUrl, cacheFolder, tileLevel, request.ImageExtension)
                    : BuildGdalWmsXml(normalizedUrl, cacheFolder, tileLevel, request.ImageExtension),
                Utf8NoBom);

            return new XyzTileSourceDefinition(
                xmlPath,
                sourceExtent,
                tileCount);
        }

        /// <summary>
        /// Validates user-entered web tile settings before a GDAL definition is written.
        /// Live-tile requests skip bounds and tile-count checks — GDAL fetches on demand.
        /// </summary>
        private static void ValidateRequest(XyzTileSourceImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LayerName))
                throw new ArgumentException("Layer name is required.");

            if (string.IsNullOrWhiteSpace(request.UrlTemplate))
                throw new ArgumentException("XYZ URL template is required.");

            if (request.UrlTemplate.Contains("virtualearth.net", StringComparison.OrdinalIgnoreCase) &&
                !HasTileToken(request.UrlTemplate, "quadkey"))
            {
                throw new ArgumentException(
                    "Bing Maps tile URLs must include the {quadkey} tile variable.");
            }

            if (!HasUsableTileTokens(request.UrlTemplate))
            {
                throw new ArgumentException(
                    "XYZ URL template must include {z}, {x}, and {y} tile variables, or a {quadkey} tile variable for Bing Maps.");
            }

            if (request.ZoomLevel < 0 || request.ZoomLevel > 25)
                throw new ArgumentException("Zoom level must be between 0 and 25.");

            // Live-tile sources use a global extent — no per-request bounds to validate.
            if (request.IsLiveTiles)
                return;

            if (request.MinLongitude < -180 ||
                request.MaxLongitude > 180 ||
                request.MinLongitude >= request.MaxLongitude)
            {
                throw new ArgumentException(
                    "Longitude bounds must be between -180 and 180, with minimum less than maximum.");
            }

            if (request.MinLatitude < -WebMercatorMaximumLatitude ||
                request.MaxLatitude > WebMercatorMaximumLatitude ||
                request.MinLatitude >= request.MaxLatitude)
            {
                throw new ArgumentException(
                    "Latitude bounds must be within Web Mercator limits and minimum must be less than maximum.");
            }

            long tileCount = CalculateTileCount(request);
            if (tileCount > MaximumTileRequestCount)
            {
                throw new ArgumentException(
                    $"The selected bounds and zoom cover {tileCount:N0} tiles. " +
                    $"This is beyond the supported import safety limit of {MaximumTileRequestCount:N0} tiles. " +
                    "Reduce the bounds or lower the zoom level.");
            }
        }

        /// <summary>
        /// Converts the lon/lat import window to the EPSG:3857 source extent used by GDAL.
        /// </summary>
        private static RasterSourceExtent BuildSourceExtent(
            XyzTileSourceImportRequest request)
        {
            (double minX, double minY) = LonLatToWebMercator(
                request.MinLongitude,
                request.MinLatitude);
            (double maxX, double maxY) = LonLatToWebMercator(
                request.MaxLongitude,
                request.MaxLatitude);

            return new RasterSourceExtent(
                "EPSG:3857",
                minX,
                minY,
                maxX,
                maxY);
        }

        /// <summary>
        /// Returns a global Web Mercator extent used by live-tile sources.
        /// </summary>
        private static RasterSourceExtent BuildGlobalSourceExtent()
        {
            return new RasterSourceExtent(
                "EPSG:3857",
                -WebMercatorOriginShift,
                -WebMercatorOriginShift,
                WebMercatorOriginShift,
                WebMercatorOriginShift);
        }

        /// <summary>
        /// Builds a GDAL WMS XML for live (on-demand internet) tile access.
        /// Uses a higher connection count and no pre-download extent restriction.
        /// </summary>
        private static string BuildGdalWmsXmlLive(
            string normalizedUrl,
            string cacheFolder,
            int maximumZoomLevel,
            string imageExtension)
        {
            string escapedUrl = SecurityElement.Escape(normalizedUrl) ?? normalizedUrl;
            string escapedCachePath = SecurityElement.Escape(cacheFolder) ?? cacheFolder;
            string extension = NormalizeImageExtension(imageExtension);
            string format = extension.TrimStart('.');
            int bandsCount = format.Equals("png", StringComparison.OrdinalIgnoreCase) ? 4 : 3;

            if (IsBingTileUrl(normalizedUrl))
            {
                return BuildGdalWmsXmlForBing(
                    normalizedUrl,
                    cacheFolder,
                    maximumZoomLevel,
                    extension,
                    bandsCount);
            }

            return
                "<GDAL_WMS>\r\n" +
                "  <Service name=\"TMS\">\r\n" +
                $"    <ServerUrl>{escapedUrl}</ServerUrl>\r\n" +
                $"    <Format>{format}</Format>\r\n" +
                "  </Service>\r\n" +
                "  <DataWindow>\r\n" +
                $"    <UpperLeftX>{FormatDouble(-WebMercatorOriginShift)}</UpperLeftX>\r\n" +
                $"    <UpperLeftY>{FormatDouble(WebMercatorOriginShift)}</UpperLeftY>\r\n" +
                $"    <LowerRightX>{FormatDouble(WebMercatorOriginShift)}</LowerRightX>\r\n" +
                $"    <LowerRightY>{FormatDouble(-WebMercatorOriginShift)}</LowerRightY>\r\n" +
                $"    <TileLevel>{maximumZoomLevel}</TileLevel>\r\n" +
                "    <TileCountX>1</TileCountX>\r\n" +
                "    <TileCountY>1</TileCountY>\r\n" +
                "    <YOrigin>top</YOrigin>\r\n" +
                "  </DataWindow>\r\n" +
                "  <Projection>EPSG:3857</Projection>\r\n" +
                $"  <BlockSizeX>{TileSize}</BlockSizeX>\r\n" +
                $"  <BlockSizeY>{TileSize}</BlockSizeY>\r\n" +
                $"  <BandsCount>{bandsCount}</BandsCount>\r\n" +
                "  <Cache>\r\n" +
                $"    <Path>{escapedCachePath}</Path>\r\n" +
                $"    <Extension>{extension}</Extension>\r\n" +
                "    <Type>file</Type>\r\n" +
                "    <Unique>True</Unique>\r\n" +
                "  </Cache>\r\n" +
                "  <MaxConnections>6</MaxConnections>\r\n" +
                "  <Timeout>20</Timeout>\r\n" +
                $"  <UserAgent>{DefaultUserAgent}</UserAgent>\r\n" +
                "  <ZeroBlockHttpCodes>204,404</ZeroBlockHttpCodes>\r\n" +
                "  <ZeroBlockOnServerException>true</ZeroBlockOnServerException>\r\n" +
                "</GDAL_WMS>\r\n";
        }

        /// <summary>
        /// Builds GDAL WMS XML for Bing/VirtualEarth quadkey tile sources.
        /// </summary>
        private static string BuildGdalWmsXmlForBing(
            string normalizedUrl,
            string cacheFolder,
            int maximumZoomLevel,
            string extension,
            int bandsCount)
        {
            string escapedUrl = SecurityElement.Escape(normalizedUrl) ?? normalizedUrl;
            string escapedCachePath = SecurityElement.Escape(cacheFolder) ?? cacheFolder;

            return
                "<GDAL_WMS>\r\n" +
                "  <Service name=\"VirtualEarth\">\r\n" +
                $"    <ServerUrl>{escapedUrl}</ServerUrl>\r\n" +
                "  </Service>\r\n" +
                "  <DataWindow>\r\n" +
                $"    <UpperLeftX>{FormatDouble(-WebMercatorOriginShift)}</UpperLeftX>\r\n" +
                $"    <UpperLeftY>{FormatDouble(WebMercatorOriginShift)}</UpperLeftY>\r\n" +
                $"    <LowerRightX>{FormatDouble(WebMercatorOriginShift)}</LowerRightX>\r\n" +
                $"    <LowerRightY>{FormatDouble(-WebMercatorOriginShift)}</LowerRightY>\r\n" +
                $"    <TileLevel>{maximumZoomLevel}</TileLevel>\r\n" +
                "    <TileCountX>1</TileCountX>\r\n" +
                "    <TileCountY>1</TileCountY>\r\n" +
                "    <YOrigin>top</YOrigin>\r\n" +
                "  </DataWindow>\r\n" +
                "  <Projection>EPSG:3857</Projection>\r\n" +
                $"  <BlockSizeX>{TileSize}</BlockSizeX>\r\n" +
                $"  <BlockSizeY>{TileSize}</BlockSizeY>\r\n" +
                $"  <BandsCount>{bandsCount}</BandsCount>\r\n" +
                "  <Cache>\r\n" +
                $"    <Path>{escapedCachePath}</Path>\r\n" +
                $"    <Extension>{extension}</Extension>\r\n" +
                "    <Type>file</Type>\r\n" +
                "    <Unique>True</Unique>\r\n" +
                "  </Cache>\r\n" +
                "  <MaxConnections>6</MaxConnections>\r\n" +
                "  <Timeout>20</Timeout>\r\n" +
                $"  <UserAgent>{DefaultUserAgent}</UserAgent>\r\n" +
                "  <ZeroBlockHttpCodes>204,404</ZeroBlockHttpCodes>\r\n" +
                "  <ZeroBlockOnServerException>true</ZeroBlockOnServerException>\r\n" +
                "</GDAL_WMS>\r\n";
        }

        /// <summary>
        /// Builds the local GDAL WMS XML expected by the TMS mini-driver.
        /// </summary>
        private static string BuildGdalWmsXml(
            string normalizedUrl,
            string cacheFolder,
            int maximumZoomLevel,
            string imageExtension)
        {
            string escapedUrl = SecurityElement.Escape(normalizedUrl) ?? normalizedUrl;
            string escapedCachePath = SecurityElement.Escape(cacheFolder) ?? cacheFolder;
            string extension = NormalizeImageExtension(imageExtension);
            string format = extension.TrimStart('.');
            int bandsCount = format.Equals("png", StringComparison.OrdinalIgnoreCase)
                ? 4
                : 3;

            if (IsBingTileUrl(normalizedUrl))
            {
                return BuildGdalWmsXmlForBing(
                    normalizedUrl,
                    cacheFolder,
                    maximumZoomLevel,
                    extension,
                    bandsCount);
            }

            return
                "<GDAL_WMS>\r\n" +
                "  <Service name=\"TMS\">\r\n" +
                $"    <ServerUrl>{escapedUrl}</ServerUrl>\r\n" +
                $"    <Format>{format}</Format>\r\n" +
                "  </Service>\r\n" +
                "  <DataWindow>\r\n" +
                $"    <UpperLeftX>{FormatDouble(-WebMercatorOriginShift)}</UpperLeftX>\r\n" +
                $"    <UpperLeftY>{FormatDouble(WebMercatorOriginShift)}</UpperLeftY>\r\n" +
                $"    <LowerRightX>{FormatDouble(WebMercatorOriginShift)}</LowerRightX>\r\n" +
                $"    <LowerRightY>{FormatDouble(-WebMercatorOriginShift)}</LowerRightY>\r\n" +
                $"    <TileLevel>{maximumZoomLevel}</TileLevel>\r\n" +
                "    <TileCountX>1</TileCountX>\r\n" +
                "    <TileCountY>1</TileCountY>\r\n" +
                "    <YOrigin>top</YOrigin>\r\n" +
                "  </DataWindow>\r\n" +
                "  <Projection>EPSG:3857</Projection>\r\n" +
                $"  <BlockSizeX>{TileSize}</BlockSizeX>\r\n" +
                $"  <BlockSizeY>{TileSize}</BlockSizeY>\r\n" +
                $"  <BandsCount>{bandsCount}</BandsCount>\r\n" +
                "  <Cache>\r\n" +
                $"    <Path>{escapedCachePath}</Path>\r\n" +
                $"    <Extension>{extension}</Extension>\r\n" +
                "    <Type>file</Type>\r\n" +
                "    <Unique>True</Unique>\r\n" +
                "  </Cache>\r\n" +
                "  <MaxConnections>2</MaxConnections>\r\n" +
                "  <Timeout>30</Timeout>\r\n" +
                $"  <UserAgent>{DefaultUserAgent}</UserAgent>\r\n" +
                "</GDAL_WMS>\r\n";
        }

        /// <summary>
        /// Converts common XYZ token syntax into the syntax GDAL's TMS driver expects.
        /// </summary>
        private static string NormalizeUrlTemplate(string urlTemplate)
        {
            string normalized = urlTemplate.Trim();
            normalized = NormalizeToken(normalized, "x");
            normalized = NormalizeToken(normalized, "y");
            normalized = NormalizeToken(normalized, "z");
            normalized = NormalizeToken(normalized, "quadkey");
            normalized = NormalizeToken(normalized, "format");
            return normalized;
        }

        /// <summary>
        /// Converts one token from common XYZ syntax to GDAL syntax when needed.
        /// </summary>
        private static string NormalizeToken(string value, string token)
        {
            return value.Contains($"${{{token}}}", StringComparison.OrdinalIgnoreCase)
                ? value
                : value.Replace(
                    $"{{{token}}}",
                    $"${{{token}}}",
                    StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks whether the URL contains either common or GDAL tile token syntax.
        /// </summary>
        private static bool HasTileToken(string urlTemplate, string token)
        {
            return urlTemplate.Contains($"{{{token}}}", StringComparison.OrdinalIgnoreCase) ||
                   urlTemplate.Contains($"${{{token}}}", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasUsableTileTokens(string urlTemplate)
        {
            return HasTileToken(urlTemplate, "quadkey") ||
                   (HasTileToken(urlTemplate, "z") &&
                    HasTileToken(urlTemplate, "x") &&
                    HasTileToken(urlTemplate, "y"));
        }

        private static bool IsBingTileUrl(string normalizedUrl)
        {
            return normalizedUrl.Contains("${quadkey}", StringComparison.OrdinalIgnoreCase) ||
                   normalizedUrl.Contains("virtualearth.net", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Counts the approximate number of tiles covered by the requested lon/lat bounds.
        /// </summary>
        private static long CalculateTileCount(XyzTileSourceImportRequest request)
        {
            int minTileX = LongitudeToTileX(request.MinLongitude, request.ZoomLevel);
            int maxTileX = LongitudeToTileX(request.MaxLongitude, request.ZoomLevel);
            int minTileY = LatitudeToTileY(request.MaxLatitude, request.ZoomLevel);
            int maxTileY = LatitudeToTileY(request.MinLatitude, request.ZoomLevel);

            long width = (long)maxTileX - minTileX + 1L;
            long height = (long)maxTileY - minTileY + 1L;
            return width * height;
        }

        /// <summary>
        /// Converts longitude to an XYZ tile column.
        /// </summary>
        private static int LongitudeToTileX(double longitude, int zoomLevel)
        {
            int tileCount = 1 << zoomLevel;
            int tileX = (int)Math.Floor((longitude + 180d) / 360d * tileCount);
            return Math.Clamp(tileX, 0, tileCount - 1);
        }

        /// <summary>
        /// Converts latitude to an XYZ tile row with top-origin numbering.
        /// </summary>
        private static int LatitudeToTileY(double latitude, int zoomLevel)
        {
            double latitudeRadians = latitude * Math.PI / 180d;
            int tileCount = 1 << zoomLevel;
            int tileY = (int)Math.Floor(
                (1d - Math.Log(
                    Math.Tan(latitudeRadians) +
                    1d / Math.Cos(latitudeRadians)) / Math.PI) /
                2d * tileCount);
            return Math.Clamp(tileY, 0, tileCount - 1);
        }

        /// <summary>
        /// Converts WGS84 longitude and latitude into Web Mercator meters.
        /// </summary>
        private static (double X, double Y) LonLatToWebMercator(
            double longitude,
            double latitude)
        {
            double clampedLatitude = Math.Clamp(
                latitude,
                -WebMercatorMaximumLatitude,
                WebMercatorMaximumLatitude);
            double x = longitude * WebMercatorOriginShift / 180d;
            double y = Math.Log(
                Math.Tan((90d + clampedLatitude) * Math.PI / 360d)) /
                (Math.PI / 180d);
            y = y * WebMercatorOriginShift / 180d;

            return (x, y);
        }

        /// <summary>
        /// Normalizes image extension text for GDAL cache and format settings.
        /// </summary>
        private static string NormalizeImageExtension(string imageExtension)
        {
            string extension = string.IsNullOrWhiteSpace(imageExtension)
                ? ".png"
                : imageExtension.Trim().ToLowerInvariant();

            if (!extension.StartsWith('.'))
                extension = $".{extension}";

            return extension is ".jpg" or ".jpeg" or ".png"
                ? extension
                : ".png";
        }

        private static int ResolvePracticalLiveTileMaxZoom(
            string normalizedUrl,
            int requestedZoomLevel)
        {
            int maxZoom = Math.Clamp(requestedZoomLevel, 0, 25);
            string url = normalizedUrl.ToLowerInvariant();

            if (url.Contains("world_physical_map", StringComparison.Ordinal))
                return Math.Min(maxZoom, 16);

            if (url.Contains("opentopomap", StringComparison.Ordinal))
                return Math.Min(maxZoom, 17);

            if (url.Contains("tile.openstreetmap.", StringComparison.Ordinal) ||
                url.Contains("openstreetmap.org", StringComparison.Ordinal) ||
                url.Contains("wikimedia.org", StringComparison.Ordinal))
            {
                return Math.Min(maxZoom, 19);
            }

            if (url.Contains("arcgisonline.com", StringComparison.Ordinal) ||
                url.Contains("basemap.nationalmap.gov", StringComparison.Ordinal))
            {
                return Math.Min(maxZoom, 19);
            }

            if (url.Contains("cartodb-basemaps", StringComparison.Ordinal) ||
                url.Contains("tiles.stadiamaps.com", StringComparison.Ordinal))
            {
                return Math.Min(maxZoom, 20);
            }

            if (url.Contains("google.com", StringComparison.Ordinal) ||
                url.Contains("googleapis.com", StringComparison.Ordinal))
            {
                return Math.Min(maxZoom, 22);
            }

            if (url.Contains("virtualearth.net", StringComparison.Ordinal) ||
                url.Contains("${quadkey}", StringComparison.Ordinal))
            {
                return Math.Min(maxZoom, 21);
            }

            return maxZoom;
        }

        /// <summary>
        /// Sanitizes layer text for use in a local XML file name.
        /// </summary>
        private static string SanitizeFileName(string name)
        {
            string sanitized = name.Trim();
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                sanitized = sanitized.Replace(invalidChar, '_');

            return string.IsNullOrWhiteSpace(sanitized)
                ? "XYZ_Tiles"
                : sanitized;
        }

        /// <summary>
        /// Returns a unique path without replacing an existing GDAL XML definition.
        /// </summary>
        private static string GetUniquePath(string desiredPath)
        {
            if (!File.Exists(desiredPath))
                return desiredPath;

            string directory = Path.GetDirectoryName(desiredPath)
                ?? throw new InvalidOperationException("Invalid XYZ source path.");
            string name = Path.GetFileNameWithoutExtension(desiredPath);
            string extension = Path.GetExtension(desiredPath);

            for (int counter = 1; counter < 10000; counter++)
            {
                string candidate = Path.Combine(
                    directory,
                    $"{name}_{counter}{extension}");

                if (!File.Exists(candidate))
                    return candidate;
            }

            throw new IOException("Could not create a unique XYZ source path.");
        }

        /// <summary>
        /// Formats numeric values for GDAL XML.
        /// </summary>
        private static string FormatDouble(double value)
        {
            return value.ToString("0.########", CultureInfo.InvariantCulture);
        }
    }
}
