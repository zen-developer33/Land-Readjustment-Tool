using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.Data.Sqlite;
using OSGeo.OSR;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    internal sealed class MbTilesRenderLayer : IRasterRenderLayer
    {
        private const int TilePixelSize = 256;
        private const int MaxCachedTiles = 1024;
        private const int MaxMissingTileKeys = 8192;
        private const int MaxTilesPerFrame = 512;
        private const int MaxTileFetchesPerFrame = 64;
        private const int MaxTileFetchBatchSize = 64;
        private const int MaxSupportedZoom = 30;
        private const byte TransparentWhiteThreshold = 248;
        private const byte TransparentWhiteMaxChannelSpread = 12;
        private const double TransparentWhiteMinimumMaskRatio = 0.01;
        private const double TransparentWhiteMinimumEdgeRatio = 0.08;
        private const double WebMercatorExtent = 20037508.342789244;
        private const double WebMercatorWorldSize = WebMercatorExtent * 2.0;
        private const double InitialResolution = WebMercatorWorldSize / TilePixelSize;
        private const string WebMercatorSrsDefinition = "EPSG:3857";

        private bool _collarRemovalEnabled = true;
        private int _tilesDecodedWithoutCollar;
        private const int CollarDisableThreshold = 10;

        private readonly object _renderSync = new();
        private readonly SqliteConnection _connection;
        private readonly Dictionary<MbTilesTileKey, Bitmap> _tileCache = new();
        private readonly LinkedList<MbTilesTileKey> _tileLru = new();
        private readonly Dictionary<MbTilesTileKey, LinkedListNode<MbTilesTileKey>> _tileLruNodes = new();
        private readonly HashSet<MbTilesTileKey> _missingTiles = new();
        private readonly Queue<MbTilesTileKey> _missingTileOrder = new();
        private readonly Dictionary<MbTilesTileKey, RectangleD> _projectTileBoundsCache = new();
        private readonly IReadOnlyList<MbTilesZoomInfo> _zoomInfos;
        private readonly HashSet<int> _availableZooms;
        private readonly SpatialReference _webMercatorSrs;
        private readonly SpatialReference _wgs84Srs;
        private readonly SpatialReference _projectSrs;
        private readonly CoordinateTransformation _webMercatorToProject;
        private readonly CoordinateTransformation _projectToWebMercator;
        private readonly CoordinateTransformation _wgs84ToProject;
        private readonly MbTilesTileRowScheme _tileRowScheme;
        private readonly bool _projectIsWebMercator;
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
            CoordinateTransformation wgs84ToProject,
            MbTilesTileRowScheme tileRowScheme,
            bool projectIsWebMercator)
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
            _tileRowScheme = tileRowScheme;
            _projectIsWebMercator = projectIsWebMercator;
            UpdateOpacityAttributes();
        }

        public int LayerId { get; }
        public string Name { get; }
        public string FilePath { get; }
        public RectangleD WorldBounds { get; }
        public int Transparency { get; private set; }
        public bool IsVisible { get; private set; }
        public bool CanRenderFromMemoryCacheDuringInteraction => true;

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
                ValidateRasterTileFormat(filePath, mbTilesMetadata);
                MbTilesTileRowScheme tileRowScheme = ResolveTileRowScheme(mbTilesMetadata);
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

                if (!IsWebMercatorSpatialReference(webMercatorSrs))
                {
                    throw new NotSupportedException(
                        "Direct MBTiles rendering only supports Web Mercator tile pyramids.");
                }

                bool projectIsWebMercator = IsWebMercatorSpatialReference(projectSrs);
                if (!projectIsWebMercator)
                {
                    throw new NotSupportedException(
                        "Direct MBTiles rendering is only enabled when the project canvas uses Web Mercator. Non-WebMercator projects must render a GDAL-warped raster view.");
                }
                CoordinateTransformation webMercatorToProject = new(webMercatorSrs, projectSrs);
                CoordinateTransformation projectToWebMercator = new(projectSrs, webMercatorSrs);
                CoordinateTransformation wgs84ToProject = new(wgs84Srs, projectSrs);

                RectangleD worldBounds = ResolveWorldBounds(
                    mbTilesMetadata,
                    zoomInfos,
                    tileRowScheme,
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
                    wgs84ToProject,
                    tileRowScheme,
                    projectIsWebMercator);
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
                    graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.CompositingMode = CompositingMode.SourceCopy;

                    return DrawVisibleTiles(
                        graphics,
                        engine,
                        visibleWorldBounds,
                        interactive,
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
            bool interactive,
            MbTilesTileRange tileRange,
            CancellationToken cancellationToken)
        {
            bool drawnAny = false;
            PointD visibleCenter = new(
                MinX(visibleWorldBounds) + Math.Abs(visibleWorldBounds.Width) / 2.0,
                MinY(visibleWorldBounds) + Math.Abs(visibleWorldBounds.Height) / 2.0);

            List<MbTilesVisibleTile> visibleTiles = new List<MbTilesVisibleTile>();
            foreach (MbTilesTileDescriptor descriptor in EnumerateTiles(tileRange))
            {
                cancellationToken.ThrowIfCancellationRequested();

                RectangleD projectBounds = GetProjectTileBounds(descriptor.Key);
                if (!TryIntersects(projectBounds, visibleWorldBounds))
                {
                    continue;
                }

                double centerX = MinX(projectBounds) + Math.Abs(projectBounds.Width) / 2.0;
                double centerY = MinY(projectBounds) + Math.Abs(projectBounds.Height) / 2.0;
                double dx = centerX - visibleCenter.X;
                double dy = centerY - visibleCenter.Y;
                visibleTiles.Add(new MbTilesVisibleTile(
                    descriptor.Key,
                    projectBounds,
                    dx * dx + dy * dy));
            }



            List<MbTilesVisibleTile> orderedTiles = visibleTiles
                .OrderBy(tile => tile.Priority)
                .ToList();

            List<MbTilesTileKey> missedKeys = interactive
                ? []
                : orderedTiles
                    .Where(tile =>
                        !_tileCache.ContainsKey(tile.Key) &&
                        !_missingTiles.Contains(tile.Key))
                    .Select(tile => tile.Key)
                    .Take(MaxTileFetchesPerFrame)
                    .ToList();

            if (missedKeys.Count > 0)
            {
                BatchFetchAndCacheTiles(missedKeys, cancellationToken);
            }

            // Draw — read directly from cache, no per-tile SQLite call needed
            foreach (MbTilesVisibleTile visibleTile in orderedTiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_tileCache.TryGetValue(visibleTile.Key, out Bitmap? bitmap))
                    continue;   // tile simply not in the MBTiles file at this zoom

                TouchTile(visibleTile.Key);
                drawnAny |= DrawTileBitmap(
                    graphics, engine, bitmap,
                    visibleTile.ProjectBounds);
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

            if (zoom < 0 || zoom > MaxSupportedZoom)
            {
                return false;
            }

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

        private bool DrawTileBitmap(
            Graphics graphics,
            MapCanvasEngine engine,
            Bitmap bitmap,
            RectangleD projectBounds)
        {
            RectangleF destination = WorldBoundsToScreenRectangle(engine, projectBounds);
            if (!IsValidDestination(destination))
            {
                return false;
            }

            DrawBitmapRegion(
                graphics,
                bitmap,
                AlignDestinationToPixelGrid(destination),
                new RectangleF(0, 0, bitmap.Width, bitmap.Height));
            return true;
        }

        private void BatchFetchAndCacheTiles(
            List<MbTilesTileKey> keys,
            CancellationToken cancellationToken)
        {
            if (keys.Count == 0)
                return;

            try
            {
                foreach (MbTilesTileKey[] batch in keys.Chunk(MaxTileFetchBatchSize))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    HashSet<MbTilesTileKey> pendingKeys = batch.ToHashSet();
                    using SqliteCommand cmd = _connection.CreateCommand();
                    cmd.CommandText = CreateTileFetchSql(batch.Length);

                    for (int i = 0; i < batch.Length; i++)
                    {
                        cmd.Parameters.AddWithValue($"$z{i}", batch[i].Zoom);
                        cmd.Parameters.AddWithValue($"$c{i}", batch[i].X);
                        cmd.Parameters.AddWithValue($"$r{i}", ToStorageTileRow(batch[i].Zoom, batch[i].Y));
                    }

                    using SqliteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int zoom = reader.GetInt32(0);
                        int col = reader.GetInt32(1);
                        int stoRow = reader.GetInt32(2);
                        byte[] tileData = (byte[])reader.GetValue(3);

                        int xyzY = _tileRowScheme == MbTilesTileRowScheme.Xyz
                            ? stoRow
                            : (int)((1L << zoom) - 1 - stoRow);

                        MbTilesTileKey key = new(zoom, col, xyzY);
                        pendingKeys.Remove(key);

                        if (_tileCache.ContainsKey(key))
                            continue;

                        cancellationToken.ThrowIfCancellationRequested();
                        Bitmap? bitmap = DecodeTileBitmap(tileData);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (bitmap == null)
                        {
                            AddMissingTile(key);
                            continue;
                        }

                        _tileCache[key] = bitmap;
                        LinkedListNode<MbTilesTileKey> node = _tileLru.AddLast(key);
                        _tileLruNodes[key] = node;
                    }

                    foreach (MbTilesTileKey missingKey in pendingKeys)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        AddMissingTile(missingKey);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                TrimTileCache();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MbTilesRenderLayer] Batch fetch failed: {ex.Message}");
                // Fall back silently — tiles simply won't appear this frame
            }
        }

        private static string CreateTileFetchSql(int keyCount)
        {
            System.Text.StringBuilder builder = new();
            builder.Append(
                "SELECT zoom_level, tile_column, tile_row, tile_data FROM tiles WHERE ");

            for (int i = 0; i < keyCount; i++)
            {
                if (i > 0)
                {
                    builder.Append(" OR ");
                }

                builder.Append(
                    "(zoom_level=$z" + i +
                    " AND tile_column=$c" + i +
                    " AND tile_row=$r" + i + ")");
            }

            return builder.ToString();
        }

        private Bitmap? DecodeTileBitmap(byte[] tileData)
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
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImageUnscaled(image, 0, 0);

                if (_collarRemovalEnabled)
                {
                    bool hadCollar = RemoveOpaqueWhiteNoDataCollar(bitmap);
                    if (!hadCollar)
                    {
                        _tilesDecodedWithoutCollar++;
                        if (_tilesDecodedWithoutCollar >= CollarDisableThreshold)
                        {
                            _collarRemovalEnabled = false;
                        }
                    }
                    else
                    {
                        _tilesDecodedWithoutCollar = 0; // reset — this file does have collars
                    }
                }
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static bool RemoveOpaqueWhiteNoDataCollar(Bitmap bitmap)
        {
            if (bitmap.Width <= 0 || bitmap.Height <= 0)
            {
                return false;
            }

            Rectangle bounds = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(
                bounds,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppPArgb);

            try
            {
                int stride = bitmapData.Stride;
                int absoluteStride = Math.Abs(stride);
                int byteCount = absoluteStride * bitmap.Height;
                byte[] pixels = new byte[byteCount];
                Marshal.Copy(bitmapData.Scan0, pixels, 0, byteCount);

                bool[] transparentMask = new bool[bitmap.Width * bitmap.Height];
                Queue<int> queue = new();

                void TrySeed(int x, int y)
                {
                    if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height)
                    {
                        return;
                    }

                    int pixelIndex = (y * bitmap.Width) + x;
                    if (transparentMask[pixelIndex])
                    {
                        return;
                    }

                    int offset = GetPixelOffset(
                        x,
                        y,
                        stride,
                        absoluteStride,
                        bitmap.Height);

                    if (!IsOpaqueWhiteNoDataPixel(pixels, offset))
                    {
                        return;
                    }

                    transparentMask[pixelIndex] = true;
                    queue.Enqueue(pixelIndex);
                }

                for (int x = 0; x < bitmap.Width; x++)
                {
                    TrySeed(x, 0);
                    TrySeed(x, bitmap.Height - 1);
                }

                for (int y = 1; y < bitmap.Height - 1; y++)
                {
                    TrySeed(0, y);
                    TrySeed(bitmap.Width - 1, y);
                }

                while (queue.Count > 0)
                {
                    int pixelIndex = queue.Dequeue();
                    int x = pixelIndex % bitmap.Width;
                    int y = pixelIndex / bitmap.Width;

                    TrySeed(x - 1, y);
                    TrySeed(x + 1, y);
                    TrySeed(x, y - 1);
                    TrySeed(x, y + 1);
                }

                if (!IsLikelyNoDataCollar(
                        transparentMask,
                        bitmap.Width,
                        bitmap.Height))
                {
                    return false;
                }

                bool changed = false;
                for (int pixelIndex = 0; pixelIndex < transparentMask.Length; pixelIndex++)
                {
                    if (!transparentMask[pixelIndex])
                    {
                        continue;
                    }

                    int x = pixelIndex % bitmap.Width;
                    int y = pixelIndex / bitmap.Width;
                    int offset = GetPixelOffset(
                        x,
                        y,
                        stride,
                        absoluteStride,
                        bitmap.Height);

                    pixels[offset] = 0;
                    pixels[offset + 1] = 0;
                    pixels[offset + 2] = 0;
                    pixels[offset + 3] = 0;
                    changed = true;
                }

                if (changed)
                {
                    Marshal.Copy(pixels, 0, bitmapData.Scan0, byteCount);
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
            return true;
        }

        private static bool IsOpaqueWhiteNoDataPixel(byte[] pixels, int offset)
        {
            byte blue = pixels[offset];
            byte green = pixels[offset + 1];
            byte red = pixels[offset + 2];
            byte alpha = pixels[offset + 3];

            if (alpha == 0 ||
                red < TransparentWhiteThreshold ||
                green < TransparentWhiteThreshold ||
                blue < TransparentWhiteThreshold)
            {
                return false;
            }

            int maxChannel = Math.Max(red, Math.Max(green, blue));
            int minChannel = Math.Min(red, Math.Min(green, blue));
            return maxChannel - minChannel <= TransparentWhiteMaxChannelSpread;
        }

        private static bool IsLikelyNoDataCollar(
            bool[] transparentMask,
            int width,
            int height)
        {
            int transparentPixelCount = 0;
            for (int index = 0; index < transparentMask.Length; index++)
            {
                if (transparentMask[index])
                {
                    transparentPixelCount++;
                }
            }

            double maskRatio = transparentPixelCount / (double)transparentMask.Length;
            if (maskRatio < TransparentWhiteMinimumMaskRatio)
            {
                return false;
            }

            int edgePixelCount = Math.Max(1, (width * 2) + (height * 2) - 4);
            int transparentEdgePixelCount = 0;

            for (int x = 0; x < width; x++)
            {
                if (transparentMask[x])
                {
                    transparentEdgePixelCount++;
                }

                int bottomIndex = ((height - 1) * width) + x;
                if (transparentMask[bottomIndex])
                {
                    transparentEdgePixelCount++;
                }
            }

            for (int y = 1; y < height - 1; y++)
            {
                if (transparentMask[y * width])
                {
                    transparentEdgePixelCount++;
                }

                int rightIndex = (y * width) + width - 1;
                if (transparentMask[rightIndex])
                {
                    transparentEdgePixelCount++;
                }
            }

            double edgeRatio = transparentEdgePixelCount / (double)edgePixelCount;
            return edgeRatio >= TransparentWhiteMinimumEdgeRatio;
        }

        private static int GetPixelOffset(
            int x,
            int y,
            int stride,
            int absoluteStride,
            int height)
        {
            int rowOffset = stride >= 0
                ? y * stride
                : (height - 1 - y) * absoluteStride;
            return rowOffset + (x * 4);
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

        private static void ValidateRasterTileFormat(
            string filePath,
            IReadOnlyDictionary<string, string> metadata)
        {
            if (!metadata.TryGetValue("format", out string? formatText) ||
                string.IsNullOrWhiteSpace(formatText))
            {
                return;
            }

            string format = formatText.Trim().TrimStart('.').ToLowerInvariant();
            if (format is "png" or "jpg" or "jpeg")
            {
                return;
            }

            throw new NotSupportedException(
                $"MBTiles file '{filePath}' stores '{formatText}' tiles. The direct renderer supports PNG/JPEG raster tiles.");
        }

        private static MbTilesTileRowScheme ResolveTileRowScheme(
            IReadOnlyDictionary<string, string> metadata)
        {
            return metadata.TryGetValue("scheme", out string? scheme) &&
                   string.Equals(
                       scheme,
                       "xyz",
                       StringComparison.OrdinalIgnoreCase)
                ? MbTilesTileRowScheme.Xyz
                : MbTilesTileRowScheme.Tms;
        }

        private static IReadOnlyList<MbTilesZoomInfo> ReadZoomInfos(
            SqliteConnection connection)
        {
            List<MbTilesZoomInfo> zoomInfos = new List<MbTilesZoomInfo>();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT zoom_level,
                       MIN(tile_column),
                       MAX(tile_column),
                       MIN(tile_row),
                       MAX(tile_row),
                       COUNT(1)
                FROM tiles
                WHERE zoom_level >= 0
                  AND zoom_level <= {MaxSupportedZoom}
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
            MbTilesTileRowScheme tileRowScheme,
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
            RectangleD webMercatorBounds = GetWebMercatorTileCoverageBounds(
                highestZoom,
                tileRowScheme);

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

            minLon = Math.Clamp(minLon, -180.0, 180.0);
            maxLon = Math.Clamp(maxLon, -180.0, 180.0);
            minLat = Math.Clamp(minLat, -85.05112878, 85.05112878);
            maxLat = Math.Clamp(maxLat, -85.05112878, 85.05112878);

            if (maxLon <= minLon || maxLat <= minLat)
            {
                return false;
            }

            bounds = new RectangleD(minLon, minLat, maxLon - minLon, maxLat - minLat);
            return true;
        }

        private static RectangleD GetWebMercatorTileCoverageBounds(
            MbTilesZoomInfo zoomInfo,
            MbTilesTileRowScheme tileRowScheme)
        {
            long matrixSize = 1L << zoomInfo.Zoom;
            int minX = zoomInfo.MinColumn;
            int maxX = zoomInfo.MaxColumn;
            int minY = tileRowScheme == MbTilesTileRowScheme.Xyz
                ? zoomInfo.MinRow
                : (int)(matrixSize - 1 - zoomInfo.MaxRow);
            int maxY = tileRowScheme == MbTilesTileRowScheme.Xyz
                ? zoomInfo.MaxRow
                : (int)(matrixSize - 1 - zoomInfo.MinRow);
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

            List<PointD> transformedPoints = new List<PointD>();
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

        private static bool IsWebMercatorSpatialReference(
            SpatialReference spatialReference)
        {
            try
            {
                spatialReference.AutoIdentifyEPSG();
                string? authorityCode =
                    spatialReference.GetAuthorityCode(null) ??
                    spatialReference.GetAuthorityCode("PROJCS");
                if (authorityCode is "3857" or "900913" or "3785")
                {
                    return true;
                }

                string? projectedName = spatialReference.GetAttrValue("PROJCS", 0);
                return projectedName != null &&
                       (projectedName.Contains(
                            "Pseudo-Mercator",
                            StringComparison.OrdinalIgnoreCase) ||
                        projectedName.Contains(
                            "Web Mercator",
                            StringComparison.OrdinalIgnoreCase) ||
                        projectedName.Contains(
                            "Popular Visualisation",
                            StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
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

        private void DrawBitmapRegion(
            Graphics graphics,
            Bitmap bitmap,
            RectangleF destination,
            RectangleF source)
        {
            if (_opacityImageAttributes == null)
            {
                using ImageAttributes imageAttributes = new();
                imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(
                    bitmap,
                    [
                        new PointF(destination.Left, destination.Top),
                        new PointF(destination.Right, destination.Top),
                        new PointF(destination.Left, destination.Bottom)
                    ],
                    source,
                    GraphicsUnit.Pixel,
                    imageAttributes);
                return;
            }

            Rectangle destinationRectangle =
                CreateIntegerDestinationRectangle(destination);
            graphics.DrawImage(
                bitmap,
                destinationRectangle,
                source.X,
                source.Y,
                source.Width,
                source.Height,
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
            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

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

        private void AddMissingTile(MbTilesTileKey key)
        {
            if (!_missingTiles.Add(key))
            {
                return;
            }

            _missingTileOrder.Enqueue(key);
            while (_missingTiles.Count > MaxMissingTileKeys &&
                   _missingTileOrder.Count > 0)
            {
                _missingTiles.Remove(_missingTileOrder.Dequeue());
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
            _missingTiles.Clear();
            _missingTileOrder.Clear();
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

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        private static Rectangle CreateIntegerDestinationRectangle(
            RectangleF destination)
        {
            int left = (int)Math.Floor(destination.Left);
            int top = (int)Math.Floor(destination.Top);
            int right = (int)Math.Ceiling(destination.Right);
            int bottom = (int)Math.Ceiling(destination.Bottom);

            return Rectangle.FromLTRB(left, top, right, bottom);
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

        private static bool IsValidSource(RectangleF source)
        {
            return IsFinite(source.Left) &&
                   IsFinite(source.Top) &&
                   IsFinite(source.Width) &&
                   IsFinite(source.Height) &&
                   source.Width >= 0.5f &&
                   source.Height >= 0.5f;
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

        private int ToStorageTileRow(int zoom, int xyzY)
        {
            if (_tileRowScheme == MbTilesTileRowScheme.Xyz)
            {
                return xyzY;
            }

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

        private readonly record struct MbTilesVisibleTile(
            MbTilesTileKey Key,
            RectangleD ProjectBounds,
            double Priority);

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

        private enum MbTilesTileRowScheme
        {
            Tms,
            Xyz
        }
    }
}
