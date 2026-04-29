using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.Data.Sqlite;
using OSGeo.OSR;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    internal sealed class MbTilesRenderLayer : IRasterRenderLayer
    {
        private const int TilePixelSize = 256;
        private const int MaxCachedTiles = 768;
        private const int MaxTilesPerFrame = 512;
        private const double WebMercatorExtent = 20037508.342789244;
        private const double WebMercatorWorldSize = WebMercatorExtent * 2.0;
        private const double InitialResolution = WebMercatorWorldSize / TilePixelSize;
        private const string WebMercatorSrsDefinition = "EPSG:3857";

        private readonly object _renderSync = new();
        private readonly SqliteConnection _connection;
        private readonly Dictionary<MbTilesTileKey, Bitmap> _tileCache = [];
        private readonly LinkedList<MbTilesTileKey> _tileLru = [];
        private readonly Dictionary<MbTilesTileKey, LinkedListNode<MbTilesTileKey>> _tileLruNodes = [];
        private readonly Dictionary<MbTilesTileKey, RectangleD> _projectTileBoundsCache = [];
        private readonly IReadOnlyList<MbTilesZoomInfo> _zoomInfos;
        private readonly HashSet<int> _availableZooms;
        private readonly SpatialReference _webMercatorSrs;
        private readonly SpatialReference _wgs84Srs;
        private readonly SpatialReference _projectSrs;
        private readonly CoordinateTransformation _webMercatorToProject;
        private readonly CoordinateTransformation _projectToWebMercator;
        private readonly CoordinateTransformation _wgs84ToProject;
        private ImageAttributes? _opacityImageAttributes;

        private MbTilesRenderLayer(
            CanvasLayer layer,
            string filePath,
            SqliteConnection connection,
            IReadOnlyList<MbTilesZoomInfo> zoomInfos,
            RectangleD worldBounds,
            SpatialReference webMercatorSrs,
            SpatialReference wgs84Srs,
            SpatialReference projectSrs,
            CoordinateTransformation webMercatorToProject,
            CoordinateTransformation projectToWebMercator,
            CoordinateTransformation wgs84ToProject)
        {
            LayerId = layer.Id;
            Name = layer.Name;
            FilePath = filePath;
            Transparency = Math.Clamp(layer.FillTransparency, 0, 100);
            IsVisible = layer.IsVisible;
            WorldBounds = worldBounds;
            _connection = connection;
            _zoomInfos = zoomInfos;
            _availableZooms = zoomInfos.Select(info => info.Zoom).ToHashSet();
            _webMercatorSrs = webMercatorSrs;
            _wgs84Srs = wgs84Srs;
            _projectSrs = projectSrs;
            _webMercatorToProject = webMercatorToProject;
            _projectToWebMercator = projectToWebMercator;
            _wgs84ToProject = wgs84ToProject;
            UpdateOpacityAttributes();
        }

        public int LayerId { get; }
        public string Name { get; }
        public string FilePath { get; }
        public RectangleD WorldBounds { get; }
        public int Transparency { get; private set; }
        public bool IsVisible { get; private set; }

        public static bool IsMbTilesPath(string filePath) =>
            string.Equals(
                Path.GetExtension(filePath),
                ".mbtiles",
                StringComparison.OrdinalIgnoreCase);

        public static MbTilesRenderLayer FromCanvasLayer(
            CanvasLayer layer,
            string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"MBTiles file for layer '{layer.Name}' was not found.",
                    filePath);
            }

            MbTilesLayerMetadata metadata =
                MbTilesLayerMetadataStore.TryRead(filePath) ??
                MbTilesLayerMetadata.Create(
                    WebMercatorSrsDefinition,
                    WebMercatorSrsDefinition,
                    filePath);

            SqliteConnection connection = OpenConnection(filePath);

            try
            {
                Dictionary<string, string> mbTilesMetadata = ReadMetadata(connection);
                IReadOnlyList<MbTilesZoomInfo> zoomInfos = ReadZoomInfos(connection);
                if (zoomInfos.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"MBTiles file '{filePath}' does not contain any tiles.");
                }

                SpatialReference webMercatorSrs = CreateSpatialReference(
                    metadata.SourceSrsDefinition);
                SpatialReference projectSrs = CreateSpatialReference(
                    metadata.TargetSrsDefinition);
                SpatialReference wgs84Srs = CreateSpatialReference("EPSG:4326");

                CoordinateTransformation webMercatorToProject = new(webMercatorSrs, projectSrs);
                CoordinateTransformation projectToWebMercator = new(projectSrs, webMercatorSrs);
                CoordinateTransformation wgs84ToProject = new(wgs84Srs, projectSrs);

                RectangleD worldBounds = ResolveWorldBounds(
                    mbTilesMetadata,
                    zoomInfos,
                    webMercatorToProject,
                    wgs84ToProject);

                return new MbTilesRenderLayer(
                    layer,
                    filePath,
                    connection,
                    zoomInfos,
                    worldBounds,
                    webMercatorSrs,
                    wgs84Srs,
                    projectSrs,
                    webMercatorToProject,
                    projectToWebMercator,
                    wgs84ToProject);
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public bool RenderVisible(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool interactive,
            CancellationToken cancellationToken = default)
        {
            lock (_renderSync)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsVisible ||
                    !TryIntersects(WorldBounds, visibleWorldBounds))
                {
                    return false;
                }

                if (!TryTransformBounds(
                    _projectToWebMercator,
                    visibleWorldBounds,
                    out RectangleD visibleWebMercatorBounds))
                {
                    return false;
                }

                if (!TryClipWebMercatorBounds(
                    visibleWebMercatorBounds,
                    out RectangleD clippedWebMercatorBounds))
                {
                    return false;
                }

                int zoom = SelectZoom(engine, clippedWebMercatorBounds, interactive);
                if (!TryCreateTileRange(
                    zoom,
                    clippedWebMercatorBounds,
                    out MbTilesTileRange tileRange))
                {
                    return false;
                }

                GraphicsState state = graphics.Save();
                try
                {
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.CompositingMode = CompositingMode.SourceOver;

                    return DrawVisibleTiles(
                        graphics,
                        engine,
                        visibleWorldBounds,
                        tileRange,
                        cancellationToken);
                }
                finally
                {
                    graphics.Restore(state);
                }
            }
        }

        public void UpdateRenderState(bool isVisible, int transparency)
        {
            lock (_renderSync)
            {
                IsVisible = isVisible;
                Transparency = Math.Clamp(transparency, 0, 100);
                UpdateOpacityAttributes();
            }
        }

        public void InvalidateCache()
        {
            lock (_renderSync)
            {
                ClearTileCache();
            }
        }

        public void Dispose()
        {
            lock (_renderSync)
            {
                ClearTileCache();
                _projectTileBoundsCache.Clear();
                _opacityImageAttributes?.Dispose();
                _opacityImageAttributes = null;
                _webMercatorToProject.Dispose();
                _projectToWebMercator.Dispose();
                _wgs84ToProject.Dispose();
                _webMercatorSrs.Dispose();
                _wgs84Srs.Dispose();
                _projectSrs.Dispose();
                _connection.Dispose();
            }
        }

        private bool DrawVisibleTiles(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            MbTilesTileRange tileRange,
            CancellationToken cancellationToken)
        {
            bool drawnAny = false;
            PointD visibleCenter = new(
                MinX(visibleWorldBounds) + Math.Abs(visibleWorldBounds.Width) / 2.0,
                MinY(visibleWorldBounds) + Math.Abs(visibleWorldBounds.Height) / 2.0);

            foreach (MbTilesTileDescriptor descriptor in EnumerateTiles(tileRange)
                .OrderBy(tile =>
                {
                    RectangleD bounds = GetProjectTileBounds(tile.Key);
                    double centerX = MinX(bounds) + Math.Abs(bounds.Width) / 2.0;
                    double centerY = MinY(bounds) + Math.Abs(bounds.Height) / 2.0;
                    double dx = centerX - visibleCenter.X;
                    double dy = centerY - visibleCenter.Y;
                    return dx * dx + dy * dy;
                }))
            {
                cancellationToken.ThrowIfCancellationRequested();

                RectangleD projectBounds = GetProjectTileBounds(descriptor.Key);
                if (!TryIntersects(projectBounds, visibleWorldBounds))
                {
                    continue;
                }

                Bitmap? bitmap = GetOrCreateTileBitmap(descriptor.Key);
                if (bitmap == null)
                {
                    continue;
                }

                RectangleF destination = WorldBoundsToScreenRectangle(engine, projectBounds);
                if (!IsValidDestination(destination))
                {
                    continue;
                }

                DrawBitmap(
                    graphics,
                    bitmap,
                    AlignDestinationToPixelGrid(destination));
                drawnAny = true;
            }

            return drawnAny;
        }

        private int SelectZoom(
            MapCanvasEngine engine,
            RectangleD visibleWebMercatorBounds,
            bool interactive)
        {
            double metersPerPixel = Math.Max(
                Math.Abs(visibleWebMercatorBounds.Width) / Math.Max(1, engine.CanvasSize.Width),
                Math.Abs(visibleWebMercatorBounds.Height) / Math.Max(1, engine.CanvasSize.Height));

            if (metersPerPixel <= 0 ||
                double.IsNaN(metersPerPixel) ||
                double.IsInfinity(metersPerPixel))
            {
                return _zoomInfos[0].Zoom;
            }

            int desiredZoom = (int)Math.Round(
                Math.Log(InitialResolution / metersPerPixel, 2.0),
                MidpointRounding.AwayFromZero);

            if (interactive)
            {
                desiredZoom--;
            }

            int selectedZoom = ChooseAvailableZoom(desiredZoom);
            while (TryCreateTileRange(
                       selectedZoom,
                       visibleWebMercatorBounds,
                       out MbTilesTileRange tileRange) &&
                   tileRange.Count > MaxTilesPerFrame)
            {
                int lowerZoom = ChooseAvailableZoom(selectedZoom - 1);
                if (lowerZoom >= selectedZoom)
                {
                    break;
                }

                selectedZoom = lowerZoom;
            }

            return selectedZoom;
        }

        private int ChooseAvailableZoom(int desiredZoom)
        {
            int minZoom = _zoomInfos[0].Zoom;
            int maxZoom = _zoomInfos[^1].Zoom;
            desiredZoom = Math.Clamp(desiredZoom, minZoom, maxZoom);

            if (_availableZooms.Contains(desiredZoom))
            {
                return desiredZoom;
            }

            int? lower = _zoomInfos
                .Select(info => info.Zoom)
                .Where(zoom => zoom <= desiredZoom)
                .Cast<int?>()
                .LastOrDefault();

            return lower ?? minZoom;
        }

        private bool TryCreateTileRange(
            int zoom,
            RectangleD webMercatorBounds,
            out MbTilesTileRange tileRange)
        {
            tileRange = default;

            long matrixSize = 1L << zoom;
            if (matrixSize <= 0)
            {
                return false;
            }

            double tileWorldSize = WebMercatorWorldSize / matrixSize;
            double left = Math.Clamp(MinX(webMercatorBounds), -WebMercatorExtent, WebMercatorExtent);
            double right = Math.Clamp(MaxX(webMercatorBounds), -WebMercatorExtent, WebMercatorExtent);
            double bottom = Math.Clamp(MinY(webMercatorBounds), -WebMercatorExtent, WebMercatorExtent);
            double top = Math.Clamp(MaxY(webMercatorBounds), -WebMercatorExtent, WebMercatorExtent);

            int minX = ClampTileIndex(
                (int)Math.Floor((left + WebMercatorExtent) / tileWorldSize),
                matrixSize);
            int maxX = ClampTileIndex(
                (int)Math.Floor((right + WebMercatorExtent) / tileWorldSize),
                matrixSize);
            int minY = ClampTileIndex(
                (int)Math.Floor((WebMercatorExtent - top) / tileWorldSize),
                matrixSize);
            int maxY = ClampTileIndex(
                (int)Math.Floor((WebMercatorExtent - bottom) / tileWorldSize),
                matrixSize);

            if (maxX < minX || maxY < minY)
            {
                return false;
            }

            tileRange = new MbTilesTileRange(zoom, minX, maxX, minY, maxY);
            return true;
        }

        private IEnumerable<MbTilesTileDescriptor> EnumerateTiles(
            MbTilesTileRange tileRange)
        {
            for (int y = tileRange.MinY; y <= tileRange.MaxY; y++)
            {
                for (int x = tileRange.MinX; x <= tileRange.MaxX; x++)
                {
                    yield return new MbTilesTileDescriptor(
                        new MbTilesTileKey(tileRange.Zoom, x, y));
                }
            }
        }

        private RectangleD GetProjectTileBounds(MbTilesTileKey key)
        {
            if (_projectTileBoundsCache.TryGetValue(key, out RectangleD cachedBounds))
            {
                return cachedBounds;
            }

            RectangleD webMercatorBounds = GetWebMercatorTileBounds(key);
            if (!TryTransformBounds(
                    _webMercatorToProject,
                    webMercatorBounds,
                    out RectangleD projectBounds))
            {
                projectBounds = webMercatorBounds;
            }

            _projectTileBoundsCache[key] = projectBounds;
            TrimProjectBoundsCache();
            return projectBounds;
        }

        private Bitmap? GetOrCreateTileBitmap(MbTilesTileKey key)
        {
            if (_tileCache.TryGetValue(key, out Bitmap? cachedBitmap))
            {
                TouchTile(key);
                return cachedBitmap;
            }

            byte[]? tileData = ReadTileData(key);
            if (tileData == null || tileData.Length == 0)
            {
                return null;
            }

            Bitmap? bitmap = DecodeTileBitmap(tileData);
            if (bitmap == null)
            {
                return null;
            }

            _tileCache[key] = bitmap;
            LinkedListNode<MbTilesTileKey> node = _tileLru.AddLast(key);
            _tileLruNodes[key] = node;
            TrimTileCache();
            return bitmap;
        }

        private byte[]? ReadTileData(MbTilesTileKey key)
        {
            int tmsRow = ToTmsTileRow(key.Zoom, key.Y);

            using SqliteCommand command = _connection.CreateCommand();
            command.CommandText = """
                SELECT tile_data
                FROM tiles
                WHERE zoom_level = $zoom
                  AND tile_column = $column
                  AND tile_row = $row
                LIMIT 1
                """;
            command.Parameters.AddWithValue("$zoom", key.Zoom);
            command.Parameters.AddWithValue("$column", key.X);
            command.Parameters.AddWithValue("$row", tmsRow);

            object? value = command.ExecuteScalar();
            return value as byte[];
        }

        private static Bitmap? DecodeTileBitmap(byte[] tileData)
        {
            try
            {
                using MemoryStream stream = new(tileData, writable: false);
                using Image image = Image.FromStream(
                    stream,
                    useEmbeddedColorManagement: false,
                    validateImageData: false);

                Bitmap bitmap = new(
                    image.Width,
                    image.Height,
                    PixelFormat.Format32bppPArgb);

                using Graphics graphics = Graphics.FromImage(bitmap);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                graphics.DrawImageUnscaled(image, 0, 0);
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static SqliteConnection OpenConnection(string filePath)
        {
            SqliteConnectionStringBuilder builder = new()
            {
                DataSource = filePath,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared,
                Pooling = true
            };

            SqliteConnection connection = new(builder.ToString());
            connection.Open();

            ExecutePragma(connection, "query_only=ON");
            ExecutePragma(connection, "temp_store=MEMORY");
            ExecutePragma(connection, "cache_size=-65536");
            ExecutePragma(connection, "mmap_size=268435456");
            return connection;
        }

        private static void ExecutePragma(
            SqliteConnection connection,
            string pragma)
        {
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = $"PRAGMA {pragma}";
                command.ExecuteNonQuery();
            }
            catch
            {
                // SQLite pragmas are performance hints; unsupported options are safe to ignore.
            }
        }

        private static Dictionary<string, string> ReadMetadata(
            SqliteConnection connection)
        {
            Dictionary<string, string> metadata = new(StringComparer.OrdinalIgnoreCase);

            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT name, value FROM metadata";

                using SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    string value = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    metadata[name] = value;
                }
            }
            catch
            {
                // Metadata is optional in some tile packages.
            }

            return metadata;
        }

        private static IReadOnlyList<MbTilesZoomInfo> ReadZoomInfos(
            SqliteConnection connection)
        {
            List<MbTilesZoomInfo> zoomInfos = [];
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = """
                SELECT zoom_level,
                       MIN(tile_column),
                       MAX(tile_column),
                       MIN(tile_row),
                       MAX(tile_row),
                       COUNT(1)
                FROM tiles
                GROUP BY zoom_level
                ORDER BY zoom_level
                """;

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                zoomInfos.Add(
                    new MbTilesZoomInfo(
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetInt32(2),
                        reader.GetInt32(3),
                        reader.GetInt32(4),
                        reader.GetInt64(5)));
            }

            return zoomInfos;
        }

        private static RectangleD ResolveWorldBounds(
            IReadOnlyDictionary<string, string> metadata,
            IReadOnlyList<MbTilesZoomInfo> zoomInfos,
            CoordinateTransformation webMercatorToProject,
            CoordinateTransformation wgs84ToProject)
        {
            if (metadata.TryGetValue("bounds", out string? boundsText) &&
                TryParseMetadataBounds(boundsText, out RectangleD lonLatBounds) &&
                TryTransformBounds(wgs84ToProject, lonLatBounds, out RectangleD projectMetadataBounds))
            {
                return projectMetadataBounds;
            }

            MbTilesZoomInfo highestZoom = zoomInfos[^1];
            RectangleD webMercatorBounds = GetWebMercatorTileCoverageBounds(highestZoom);

            return TryTransformBounds(
                webMercatorToProject,
                webMercatorBounds,
                out RectangleD projectBounds)
                ? projectBounds
                : webMercatorBounds;
        }

        private static bool TryParseMetadataBounds(
            string boundsText,
            out RectangleD bounds)
        {
            bounds = default;
            string[] parts = boundsText
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length != 4 ||
                !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double minLon) ||
                !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double minLat) ||
                !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double maxLon) ||
                !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double maxLat))
            {
                return false;
            }

            if (maxLon <= minLon || maxLat <= minLat)
            {
                return false;
            }

            bounds = new RectangleD(minLon, minLat, maxLon - minLon, maxLat - minLat);
            return true;
        }

        private static RectangleD GetWebMercatorTileCoverageBounds(
            MbTilesZoomInfo zoomInfo)
        {
            long matrixSize = 1L << zoomInfo.Zoom;
            int minX = zoomInfo.MinColumn;
            int maxX = zoomInfo.MaxColumn;
            int minY = (int)(matrixSize - 1 - zoomInfo.MaxRow);
            int maxY = (int)(matrixSize - 1 - zoomInfo.MinRow);
            double tileWorldSize = WebMercatorWorldSize / matrixSize;

            double left = -WebMercatorExtent + minX * tileWorldSize;
            double right = -WebMercatorExtent + (maxX + 1) * tileWorldSize;
            double top = WebMercatorExtent - minY * tileWorldSize;
            double bottom = WebMercatorExtent - (maxY + 1) * tileWorldSize;

            return new RectangleD(left, bottom, right - left, top - bottom);
        }

        private static RectangleD GetWebMercatorTileBounds(
            MbTilesTileKey key)
        {
            long matrixSize = 1L << key.Zoom;
            double tileWorldSize = WebMercatorWorldSize / matrixSize;
            double left = -WebMercatorExtent + key.X * tileWorldSize;
            double right = left + tileWorldSize;
            double top = WebMercatorExtent - key.Y * tileWorldSize;
            double bottom = top - tileWorldSize;

            return new RectangleD(left, bottom, right - left, top - bottom);
        }

        private static bool TryTransformBounds(
            CoordinateTransformation transformation,
            RectangleD sourceBounds,
            out RectangleD targetBounds)
        {
            targetBounds = default;
            PointD[] sourcePoints =
            [
                new(MinX(sourceBounds), MinY(sourceBounds)),
                new(MinX(sourceBounds), MaxY(sourceBounds)),
                new(MaxX(sourceBounds), MinY(sourceBounds)),
                new(MaxX(sourceBounds), MaxY(sourceBounds)),
                new((MinX(sourceBounds) + MaxX(sourceBounds)) / 2.0, MinY(sourceBounds)),
                new((MinX(sourceBounds) + MaxX(sourceBounds)) / 2.0, MaxY(sourceBounds)),
                new(MinX(sourceBounds), (MinY(sourceBounds) + MaxY(sourceBounds)) / 2.0),
                new(MaxX(sourceBounds), (MinY(sourceBounds) + MaxY(sourceBounds)) / 2.0),
                new((MinX(sourceBounds) + MaxX(sourceBounds)) / 2.0, (MinY(sourceBounds) + MaxY(sourceBounds)) / 2.0)
            ];

            List<PointD> transformedPoints = [];
            foreach (PointD sourcePoint in sourcePoints)
            {
                if (!TryTransformPoint(
                        transformation,
                        sourcePoint,
                        out PointD transformedPoint))
                {
                    continue;
                }

                transformedPoints.Add(transformedPoint);
            }

            if (transformedPoints.Count == 0)
            {
                return false;
            }

            double minX = transformedPoints.Min(point => point.X);
            double maxX = transformedPoints.Max(point => point.X);
            double minY = transformedPoints.Min(point => point.Y);
            double maxY = transformedPoints.Max(point => point.Y);

            if (maxX <= minX || maxY <= minY)
            {
                return false;
            }

            targetBounds = new RectangleD(minX, minY, maxX - minX, maxY - minY);
            return true;
        }

        private static bool TryTransformPoint(
            CoordinateTransformation transformation,
            PointD sourcePoint,
            out PointD transformedPoint)
        {
            transformedPoint = default;

            try
            {
                double[] point = [sourcePoint.X, sourcePoint.Y, 0.0];
                transformation.TransformPoint(point);
                if (!IsFinite(point[0]) || !IsFinite(point[1]))
                {
                    return false;
                }

                transformedPoint = new PointD(point[0], point[1]);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static SpatialReference CreateSpatialReference(string definition)
        {
            SpatialReference spatialReference = new(string.Empty);
            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);

            if (spatialReference.SetFromUserInput(definition) != 0)
            {
                string wkt = definition;
                if (spatialReference.ImportFromWkt(ref wkt) != 0)
                {
                    spatialReference.Dispose();
                    throw new InvalidOperationException(
                        $"Could not parse raster CRS definition '{definition}'.");
                }
            }

            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            return spatialReference;
        }

        private static bool TryClipWebMercatorBounds(
            RectangleD bounds,
            out RectangleD clippedBounds)
        {
            clippedBounds = default;
            double left = Math.Max(MinX(bounds), -WebMercatorExtent);
            double right = Math.Min(MaxX(bounds), WebMercatorExtent);
            double bottom = Math.Max(MinY(bounds), -WebMercatorExtent);
            double top = Math.Min(MaxY(bounds), WebMercatorExtent);

            if (right <= left || top <= bottom)
            {
                return false;
            }

            clippedBounds = new RectangleD(left, bottom, right - left, top - bottom);
            return true;
        }

        private void DrawBitmap(
            Graphics graphics,
            Bitmap bitmap,
            RectangleF destination)
        {
            if (_opacityImageAttributes == null)
            {
                graphics.DrawImage(bitmap, destination);
                return;
            }

            Rectangle destinationRectangle = Rectangle.Round(destination);
            graphics.DrawImage(
                bitmap,
                destinationRectangle,
                0,
                0,
                bitmap.Width,
                bitmap.Height,
                GraphicsUnit.Pixel,
                _opacityImageAttributes);
        }

        private void UpdateOpacityAttributes()
        {
            _opacityImageAttributes?.Dispose();
            _opacityImageAttributes = null;

            double opacityFactor = (100 - Transparency) / 100d;
            if (opacityFactor >= 1d)
            {
                return;
            }

            ImageAttributes imageAttributes = new();
            ColorMatrix opacityMatrix = new(
                [
                    [1f, 0f, 0f, 0f, 0f],
                    [0f, 1f, 0f, 0f, 0f],
                    [0f, 0f, 1f, 0f, 0f],
                    [0f, 0f, 0f, (float)opacityFactor, 0f],
                    [0f, 0f, 0f, 0f, 1f]
                ]);
            imageAttributes.SetColorMatrix(
                opacityMatrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);

            _opacityImageAttributes = imageAttributes;
        }

        private void TouchTile(MbTilesTileKey key)
        {
            if (!_tileLruNodes.TryGetValue(key, out LinkedListNode<MbTilesTileKey>? node))
            {
                return;
            }

            _tileLru.Remove(node);
            _tileLru.AddLast(node);
        }

        private void TrimTileCache()
        {
            while (_tileCache.Count > MaxCachedTiles && _tileLru.First != null)
            {
                MbTilesTileKey key = _tileLru.First.Value;
                _tileLru.RemoveFirst();
                _tileLruNodes.Remove(key);
                _projectTileBoundsCache.Remove(key);

                if (_tileCache.Remove(key, out Bitmap? bitmap))
                {
                    bitmap.Dispose();
                }
            }
        }

        private void TrimProjectBoundsCache()
        {
            if (_projectTileBoundsCache.Count <= MaxCachedTiles * 2)
            {
                return;
            }

            HashSet<MbTilesTileKey> cachedTileKeys = _tileCache.Keys.ToHashSet();
            foreach (MbTilesTileKey key in _projectTileBoundsCache.Keys
                .Where(key => !cachedTileKeys.Contains(key))
                .Take(_projectTileBoundsCache.Count - MaxCachedTiles)
                .ToArray())
            {
                _projectTileBoundsCache.Remove(key);
            }
        }

        private void ClearTileCache()
        {
            foreach (Bitmap bitmap in _tileCache.Values)
            {
                bitmap.Dispose();
            }

            _tileCache.Clear();
            _tileLru.Clear();
            _tileLruNodes.Clear();
            _projectTileBoundsCache.Clear();
        }

        private static RectangleF WorldBoundsToScreenRectangle(
            MapCanvasEngine engine,
            RectangleD worldBounds)
        {
            PointD topLeft = engine.WorldToScreen(
                new PointD(MinX(worldBounds), MaxY(worldBounds)));
            PointD bottomRight = engine.WorldToScreen(
                new PointD(MaxX(worldBounds), MinY(worldBounds)));

            return RectangleF.FromLTRB(
                (float)Math.Min(topLeft.X, bottomRight.X),
                (float)Math.Min(topLeft.Y, bottomRight.Y),
                (float)Math.Max(topLeft.X, bottomRight.X),
                (float)Math.Max(topLeft.Y, bottomRight.Y));
        }

        private static RectangleF AlignDestinationToPixelGrid(RectangleF destination)
        {
            float left = (float)Math.Floor(destination.Left);
            float top = (float)Math.Floor(destination.Top);
            float right = (float)Math.Ceiling(destination.Right);
            float bottom = (float)Math.Ceiling(destination.Bottom);

            return RectangleF.FromLTRB(left - 0.5f, top - 0.5f, right + 0.5f, bottom + 0.5f);
        }

        private static bool IsValidDestination(RectangleF destination)
        {
            return IsFinite(destination.Left) &&
                   IsFinite(destination.Top) &&
                   IsFinite(destination.Width) &&
                   IsFinite(destination.Height) &&
                   destination.Width >= 0.5f &&
                   destination.Height >= 0.5f;
        }

        private static bool TryIntersects(
            RectangleD first,
            RectangleD second)
        {
            return MinX(first) < MaxX(second) &&
                   MaxX(first) > MinX(second) &&
                   MinY(first) < MaxY(second) &&
                   MaxY(first) > MinY(second);
        }

        private static int ClampTileIndex(int value, long matrixSize)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value >= matrixSize)
            {
                return (int)matrixSize - 1;
            }

            return value;
        }

        private static int ToTmsTileRow(int zoom, int xyzY)
        {
            long matrixSize = 1L << zoom;
            return (int)(matrixSize - 1 - xyzY);
        }

        private static double MinX(RectangleD rectangle) =>
            Math.Min(rectangle.Left, rectangle.Right);

        private static double MaxX(RectangleD rectangle) =>
            Math.Max(rectangle.Left, rectangle.Right);

        private static double MinY(RectangleD rectangle) =>
            Math.Min(rectangle.Top, rectangle.Bottom);

        private static double MaxY(RectangleD rectangle) =>
            Math.Max(rectangle.Top, rectangle.Bottom);

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);

        private readonly record struct MbTilesTileKey(
            int Zoom,
            int X,
            int Y);

        private readonly record struct MbTilesTileDescriptor(
            MbTilesTileKey Key);

        private readonly record struct MbTilesTileRange(
            int Zoom,
            int MinX,
            int MaxX,
            int MinY,
            int MaxY)
        {
            public long Count => (long)Math.Max(0, MaxX - MinX + 1) *
                                 Math.Max(0, MaxY - MinY + 1);
        }

        private readonly record struct MbTilesZoomInfo(
            int Zoom,
            int MinColumn,
            int MaxColumn,
            int MinRow,
            int MaxRow,
            long TileCount);
    }
}
