using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Geometries;
using OSGeo.GDAL;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class RasterRenderLayer : IRasterRenderLayer
    {
        private const int MaxCachePixels = 12_000_000;
        private const int MaxInteractiveCachePixels = 1_500_000;
        private const int InteractiveTileSize = 256;
        private const int RasterTileSize = 256;
        private const int MaxCachedTiles = 384;

        private readonly Dataset _dataset;
        private readonly double[] _geoTransform;
        private readonly double[] _inverseGeoTransform;
        private readonly RasterBandSelection _bandSelection;
        private readonly RasterTileDiskCache _tileDiskCache;
        private readonly Dictionary<RasterTileCacheKey, Bitmap> _tileCache = new Dictionary<RasterTileCacheKey, Bitmap>();
        private readonly LinkedList<RasterTileCacheKey> _tileLru = new LinkedList<RasterTileCacheKey>();
        private readonly Dictionary<RasterTileCacheKey, LinkedListNode<RasterTileCacheKey>> _tileLruNodes = new Dictionary<RasterTileCacheKey, LinkedListNode<RasterTileCacheKey>>();
        private readonly Dictionary<int, RasterTileIndex> _tileIndexes = new Dictionary<int, RasterTileIndex>();
        private readonly Dictionary<int, RasterOverviewInfo> _overviewInfos = new Dictionary<int, RasterOverviewInfo>();
        private readonly object _renderSync = new();
        private ImageAttributes? _opacityImageAttributes;

        private RasterRenderLayer(
            int layerId,
            string name,
            string filePath,
            Dataset dataset,
            RectangleD worldBounds,
            double[] geoTransform,
            double[] inverseGeoTransform,
            int transparency)
        {
            LayerId = layerId;
            Name = name;
            FilePath = filePath;
            _dataset = dataset;
            WorldBounds = worldBounds;
            _geoTransform = [.. geoTransform];
            _inverseGeoTransform = [.. inverseGeoTransform];
            SourceWidth = dataset.RasterXSize;
            SourceHeight = dataset.RasterYSize;
            Transparency = 0;
            IsVisible = true;
            _bandSelection = ResolveBandSelection(dataset);
            _tileDiskCache = RasterTileDiskCache.Create(
                filePath,
                SourceWidth,
                SourceHeight);
            UpdateOpacityAttributes();
        }

        public int LayerId { get; }
        public string Name { get; }
        public string FilePath { get; }
        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public RectangleD WorldBounds { get; }
        public int Transparency { get; private set; }
        public bool IsVisible { get; private set; }
        public bool CanRenderFromMemoryCacheDuringInteraction => false;

        public static RasterRenderLayer FromCanvasLayer(
            CanvasLayer layer,
            string? projectFolderPath)
        {
            ArgumentNullException.ThrowIfNull(layer);

            if (string.IsNullOrWhiteSpace(layer.SourceFile))
                throw new InvalidOperationException(
                    $"Raster layer '{layer.Name}' does not have a source file.");

            string filePath = ResolveLayerFilePath(
                layer.SourceFile,
                projectFolderPath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException(
                    $"Raster file for layer '{layer.Name}' was not found.",
                    filePath);

            GdalConfiguration.ConfigureGdal();
            if (!GdalConfiguration.Usable)
                throw new InvalidOperationException(
                    "GDAL is not configured correctly. Raster rendering cannot continue.");

            Dataset dataset = Gdal.Open(filePath, Access.GA_ReadOnly)
                ?? throw new InvalidOperationException(
                    $"GDAL could not open raster '{filePath}'.");

            try
            {
                double[] transform = new double[6];
                dataset.GetGeoTransform(transform);

                if (!TryInvertGeoTransform(transform, out double[] inverseTransform))
                {
                    throw new InvalidOperationException(
                        $"Raster layer '{layer.Name}' has an invalid geotransform.");
                }

                RectangleD bounds = GetWorldBounds(dataset, transform);
                return new RasterRenderLayer(
                    layer.Id,
                    layer.Name,
                    filePath,
                    dataset,
                    bounds,
                    transform,
                    inverseTransform,
                    layer.FillTransparency)
                {
                    IsVisible = layer.IsVisible
                };
            }
            catch
            {
                dataset.Dispose();
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

                if (!IsVisible)
                    return false;

                if (!TryGetIntersection(WorldBounds, visibleWorldBounds, out RectangleD visibleRasterWorldBounds))
                    return false;

                if (!TryCreateReadWindow(visibleRasterWorldBounds, interactive, out RasterReadWindow readWindow))
                    return false;

                RectangleD readWindowWorldBounds = GetWorldBounds(readWindow);
                RectangleF destination = WorldBoundsToScreenRectangle(
                    engine,
                    readWindowWorldBounds);

                if (destination.Width < 1.0f || destination.Height < 1.0f)
                    return false;

                Size targetSize = GetTargetBitmapSize(destination, interactive);
                RasterReadContext readContext = CreateReadContext(readWindow, targetSize);

                GraphicsState state = graphics.Save();
                try
                {
                    graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    graphics.InterpolationMode = ResolveInterpolationMode(
                        interactive,
                        readContext);
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.CompositingMode = CompositingMode.SourceOver;

                    return DrawVisibleTiles(
                        graphics,
                        engine,
                        readContext,
                        visibleRasterWorldBounds,
                        cancellationToken);
                }
                finally
                {
                    graphics.Restore(state);
                }
            }
        }

        /// <summary>
        /// Updates lightweight raster render flags without rebuilding the GDAL dataset or bitmap cache.
        /// </summary>
        public void UpdateRenderState(bool isVisible, int transparency)
        {
            lock (_renderSync)
            {
                IsVisible = isVisible;
                Transparency = 0;
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
                _opacityImageAttributes?.Dispose();
                _opacityImageAttributes = null;
                _dataset.Dispose();
            }
        }

        private bool DrawVisibleTiles(
            Graphics graphics,
            MapCanvasEngine engine,
            RasterReadContext context,
            RectangleD visibleWorldBounds,
            CancellationToken cancellationToken)
        {
            bool drawnAny = false;
            Envelope envelope = CreateEnvelope(visibleWorldBounds);
            RasterTileIndex tileIndex = GetOrCreateTileIndex(context);

            foreach (RasterTileDescriptor tile in QueryVisibleTilesByPriority(
                tileIndex,
                envelope,
                visibleWorldBounds))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Intersects(context.OverviewWindow, tile.TileWindow))
                {
                    continue;
                }

                Bitmap tileBitmap = GetOrCreateTileBitmap(context, tile.TileWindow);
                RectangleF destination = WorldBoundsToScreenRectangle(engine, tile.WorldBounds);

                if (destination.Width < 0.5f || destination.Height < 0.5f)
                {
                    continue;
                }

                RectangleF alignedDestination = AlignDestinationToPixelGrid(destination);
                DrawBitmap(graphics, tileBitmap, alignedDestination);
                drawnAny = true;
            }

            return drawnAny;
        }

        private static IEnumerable<RasterTileDescriptor> QueryVisibleTilesByPriority(
            RasterTileIndex tileIndex,
            Envelope visibleEnvelope,
            RectangleD visibleWorldBounds)
        {
            double centerX = visibleWorldBounds.X + visibleWorldBounds.Width / 2d;
            double centerY = visibleWorldBounds.Y + visibleWorldBounds.Height / 2d;

            return tileIndex.SpatialIndex
                .Query(visibleEnvelope)
                .OrderBy(tile =>
                {
                    double tileCenterX = tile.WorldBounds.X + tile.WorldBounds.Width / 2d;
                    double tileCenterY = tile.WorldBounds.Y + tile.WorldBounds.Height / 2d;
                    double dx = tileCenterX - centerX;
                    double dy = tileCenterY - centerY;
                    return dx * dx + dy * dy;
                });
        }

        private RasterTileIndex GetOrCreateTileIndex(RasterReadContext context)
        {
            if (_tileIndexes.TryGetValue(context.OverviewIndex, out RasterTileIndex? cachedIndex))
            {
                return cachedIndex;
            }

            RasterOverviewInfo overviewInfo = GetOverviewInfo(context.OverviewIndex);
            STRtree<RasterTileDescriptor> spatialIndex = new();

            for (int tileY = 0; tileY < overviewInfo.Height; tileY += RasterTileSize)
            {
                for (int tileX = 0; tileX < overviewInfo.Width; tileX += RasterTileSize)
                {
                    int width = Math.Min(RasterTileSize, overviewInfo.Width - tileX);
                    int height = Math.Min(RasterTileSize, overviewInfo.Height - tileY);
                    RasterReadWindow tileWindow = new(tileX, tileY, width, height);
                    RasterReadWindow baseTileWindow = ToBaseReadWindow(tileWindow, overviewInfo.Decimation);
                    RectangleD worldBounds = GetWorldBounds(baseTileWindow);
                    RasterTileDescriptor descriptor = new(
                        new RasterTileCacheKey(
                            context.OverviewIndex,
                            tileWindow.X,
                            tileWindow.Y,
                            tileWindow.Width,
                            tileWindow.Height),
                        tileWindow,
                        baseTileWindow,
                        worldBounds);

                    spatialIndex.Insert(CreateEnvelope(worldBounds), descriptor);
                }
            }

            spatialIndex.Build();
            RasterTileIndex tileIndex = new(spatialIndex);
            _tileIndexes[context.OverviewIndex] = tileIndex;
            return tileIndex;
        }

        private RasterOverviewInfo GetOverviewInfo(int overviewIndex)
        {
            if (_overviewInfos.TryGetValue(overviewIndex, out RasterOverviewInfo cachedInfo))
            {
                return cachedInfo;
            }

            using Band referenceBand = _dataset.GetRasterBand(_bandSelection.ReferenceBand);
            int width = referenceBand.XSize;
            int height = referenceBand.YSize;
            double decimation = 1.0;

            if (overviewIndex >= 0)
            {
                using Band overviewBand = referenceBand.GetOverview(overviewIndex);
                width = overviewBand.XSize;
                height = overviewBand.YSize;
                decimation = Math.Max(
                    referenceBand.XSize / (double)Math.Max(1, width),
                    referenceBand.YSize / (double)Math.Max(1, height));
            }

            RasterOverviewInfo info = new(width, height, decimation);
            _overviewInfos[overviewIndex] = info;
            return info;
        }

        private RasterReadContext CreateReadContext(
            RasterReadWindow readWindow,
            Size targetSize)
        {
            using Band referenceBand = _dataset.GetRasterBand(_bandSelection.ReferenceBand);
            int overviewIndex = SelectOverviewIndex(referenceBand, readWindow, targetSize, out double decimation);

            int selectedWidth = referenceBand.XSize;
            int selectedHeight = referenceBand.YSize;
            if (overviewIndex >= 0)
            {
                using Band overviewBand = referenceBand.GetOverview(overviewIndex);
                selectedWidth = overviewBand.XSize;
                selectedHeight = overviewBand.YSize;
            }

            int x = (int)Math.Floor(readWindow.X / decimation);
            int y = (int)Math.Floor(readWindow.Y / decimation);
            int right = (int)Math.Ceiling((readWindow.X + readWindow.Width) / decimation);
            int bottom = (int)Math.Ceiling((readWindow.Y + readWindow.Height) / decimation);

            x = Math.Clamp(x, 0, Math.Max(0, selectedWidth - 1));
            y = Math.Clamp(y, 0, Math.Max(0, selectedHeight - 1));
            right = Math.Clamp(right, x + 1, selectedWidth);
            bottom = Math.Clamp(bottom, y + 1, selectedHeight);

            return new RasterReadContext(
                overviewIndex,
                decimation,
                new RasterReadWindow(x, y, right - x, bottom - y));
        }

        private static int SelectOverviewIndex(
            Band band,
            RasterReadWindow readWindow,
            Size targetSize,
            out double decimation)
        {
            decimation = 1.0;

            if (targetSize.Width <= 0 || targetSize.Height <= 0)
            {
                return -1;
            }

            int overviewCount = band.GetOverviewCount();
            if (overviewCount <= 0)
            {
                return -1;
            }

            double desiredScale = Math.Max(
                readWindow.Width / (double)targetSize.Width,
                readWindow.Height / (double)targetSize.Height);

            if (desiredScale <= 1.0)
            {
                return -1;
            }

            int bestIndex = -1;
            double bestScale = 1.0;

            for (int index = 0; index < overviewCount; index++)
            {
                using Band overview = band.GetOverview(index);
                if (overview == null || overview.XSize <= 0 || overview.YSize <= 0)
                {
                    continue;
                }

                double scale = Math.Max(
                    band.XSize / (double)overview.XSize,
                    band.YSize / (double)overview.YSize);

                if (scale <= desiredScale && scale > bestScale)
                {
                    bestScale = scale;
                    bestIndex = index;
                }
            }

            decimation = bestScale;
            return bestIndex;
        }

        private Bitmap GetOrCreateTileBitmap(
            RasterReadContext context,
            RasterReadWindow tileWindow)
        {
            RasterTileCacheKey cacheKey = new(
                context.OverviewIndex,
                tileWindow.X,
                tileWindow.Y,
                tileWindow.Width,
                tileWindow.Height);

            if (_tileCache.TryGetValue(cacheKey, out Bitmap? cachedBitmap))
            {
                TouchTile(cacheKey);
                return cachedBitmap;
            }

            if (_tileDiskCache.TryRead(cacheKey, out Bitmap? diskBitmap))
            {
                _tileCache[cacheKey] = diskBitmap;
                LinkedListNode<RasterTileCacheKey> diskNode = _tileLru.AddLast(cacheKey);
                _tileLruNodes[cacheKey] = diskNode;
                TrimTileCache();
                return diskBitmap;
            }

            Bitmap bitmap = CreateTileBitmap(context.OverviewIndex, tileWindow);
            _tileCache[cacheKey] = bitmap;
            LinkedListNode<RasterTileCacheKey> node = _tileLru.AddLast(cacheKey);
            _tileLruNodes[cacheKey] = node;
            _tileDiskCache.QueueWrite(cacheKey, bitmap);
            TrimTileCache();
            return bitmap;
        }

        private Bitmap CreateTileBitmap(
            int overviewIndex,
            RasterReadWindow tileWindow)
        {
            int pixelCount = tileWindow.Width * tileWindow.Height;
            byte[] alpha = new byte[pixelCount];
            Array.Fill(alpha, (byte)255);

            if (_bandSelection.Mode == RasterBandMode.Palette)
            {
                using Band paletteBand = _dataset.GetRasterBand(_bandSelection.PaletteBand);
                byte[] indexes = ReadBandWindow(paletteBand, overviewIndex, tileWindow);
                ApplySingleBandNoData(indexes, alpha, TryGetNoDataValue(paletteBand));
                return CreatePaletteBitmap(paletteBand, overviewIndex, indexes, alpha, tileWindow.Width, tileWindow.Height);
            }

            if (_bandSelection.Mode == RasterBandMode.Rgb)
            {
                using Band redBand = _dataset.GetRasterBand(_bandSelection.RedBand);
                using Band greenBand = _dataset.GetRasterBand(_bandSelection.GreenBand);
                using Band blueBand = _dataset.GetRasterBand(_bandSelection.BlueBand);

                byte[] red = ReadBandWindow(redBand, overviewIndex, tileWindow);
                byte[] green = ReadBandWindow(greenBand, overviewIndex, tileWindow);
                byte[] blue = ReadBandWindow(blueBand, overviewIndex, tileWindow);

                if (_bandSelection.AlphaBand.HasValue)
                {

                    using Band alphaBand = _dataset.GetRasterBand(_bandSelection.AlphaBand.Value);
                    alpha = ReadBandWindow(alphaBand, overviewIndex, tileWindow);
                }

                double? redNoData = TryGetNoDataValue(redBand);
                double? greenNoData = TryGetNoDataValue(greenBand);
                double? blueNoData = TryGetNoDataValue(blueBand);

                ApplyRgbNoData(red, green, blue, alpha, redNoData, greenNoData, blueNoData);

                // If no NoData value is declared in file metadata, flood-fill blank
                // border pixels from the edges. Imported/warped rasters often carry
                // black or white fill without declaring NoData.
                if (redNoData == null && greenNoData == null && blueNoData == null)
                {
                    RemoveBlackNoDataCollar(
                        red, green, blue, alpha,
                        tileWindow.Width, tileWindow.Height);
                    RemoveWhiteNoDataCollar(
                        red, green, blue, alpha,
                        tileWindow.Width, tileWindow.Height);
                }

                return CreateArgbBitmap(red, green, blue, alpha, tileWindow.Width, tileWindow.Height);
            }

            using Band grayBand = _dataset.GetRasterBand(_bandSelection.GrayBand);
            byte[] gray = ReadBandWindow(grayBand, overviewIndex, tileWindow);
            ApplySingleBandNoData(gray, alpha, TryGetNoDataValue(grayBand));
            return CreateArgbBitmap(gray, gray, gray, alpha, tileWindow.Width, tileWindow.Height);
        }

        private static byte[] ReadBandWindow(
            Band baseBand,
            int overviewIndex,
            RasterReadWindow readWindow)
        {
            Band selectedBand = baseBand;
            bool disposeSelectedBand = false;
            try
            {
                if (overviewIndex >= 0)
                {
                    selectedBand = baseBand.GetOverview(overviewIndex);
                    disposeSelectedBand = true;
                }

                byte[] buffer = new byte[readWindow.Width * readWindow.Height];
                CPLErr result = selectedBand.ReadRaster(
                    readWindow.X,
                    readWindow.Y,
                    readWindow.Width,
                    readWindow.Height,
                    buffer,
                    readWindow.Width,
                    readWindow.Height,
                    0,
                    0);

                if (result != CPLErr.CE_None)
                {
                    throw new InvalidOperationException(
                        $"GDAL failed to read raster band. Error: {result}.");
                }

                return buffer;
            }
            finally
            {
                if (disposeSelectedBand)
                {
                    selectedBand.Dispose();
                }
            }
        }

        private static Bitmap CreatePaletteBitmap(
            Band paletteBand,
            int overviewIndex,
            byte[] indexes,
            byte[] alpha,
            int width,
            int height)
        {
            ColorTable? colorTable = null;
            Band selectedBand = paletteBand;
            bool disposeSelectedBand = false;

            try
            {
                if (overviewIndex >= 0)
                {
                    selectedBand = paletteBand.GetOverview(overviewIndex);
                    disposeSelectedBand = true;
                }

                colorTable = selectedBand.GetRasterColorTable() ?? paletteBand.GetRasterColorTable();
            }
            finally
            {
                if (disposeSelectedBand)
                {
                    selectedBand.Dispose();
                }
            }

            byte[] red = new byte[indexes.Length];
            byte[] green = new byte[indexes.Length];
            byte[] blue = new byte[indexes.Length];

            for (int i = 0; i < indexes.Length; i++)
            {
                ColorEntry entry = new();
                if (colorTable != null && colorTable.GetColorEntryAsRGB(indexes[i], entry) != 0)
                {
                    red[i] = ClampByte(entry.c1);
                    green[i] = ClampByte(entry.c2);
                    blue[i] = ClampByte(entry.c3);
                    alpha[i] = ClampByte(entry.c4);
                }
                else
                {
                    red[i] = indexes[i];
                    green[i] = indexes[i];
                    blue[i] = indexes[i];
                }
            }

            return CreateArgbBitmap(red, green, blue, alpha, width, height);
        }

        private static RasterBandSelection ResolveBandSelection(Dataset dataset)
        {
            int? paletteBand = null;
            int? redBand = null;
            int? greenBand = null;
            int? blueBand = null;
            int? alphaBand = null;

            for (int bandIndex = 1; bandIndex <= dataset.RasterCount; bandIndex++)
            {
                using Band band = dataset.GetRasterBand(bandIndex);
                ColorInterp interp = band.GetRasterColorInterpretation();

                if (interp == ColorInterp.GCI_PaletteIndex)
                    paletteBand ??= bandIndex;
                else if (interp == ColorInterp.GCI_RedBand)
                    redBand ??= bandIndex;
                else if (interp == ColorInterp.GCI_GreenBand)
                    greenBand ??= bandIndex;
                else if (interp == ColorInterp.GCI_BlueBand)
                    blueBand ??= bandIndex;
                else if (interp == ColorInterp.GCI_AlphaBand)
                    alphaBand ??= bandIndex;
            }

            if (paletteBand.HasValue)
            {
                return new RasterBandSelection(
                    RasterBandMode.Palette,
                    paletteBand.Value,
                    paletteBand.Value,
                    0,
                    0,
                    0,
                    null,
                    0);
            }

            if (dataset.RasterCount >= 3)
            {
                int resolvedRed = redBand ?? 1;
                int resolvedGreen = greenBand ?? 2;
                int resolvedBlue = blueBand ?? 3;
                return new RasterBandSelection(
                    RasterBandMode.Rgb,
                    resolvedRed,
                    0,
                    resolvedRed,
                    resolvedGreen,
                    resolvedBlue,
                    alphaBand,
                    0);
            }

            return new RasterBandSelection(
                RasterBandMode.Gray,
                1,
                0,
                0,
                0,
                0,
                null,
                1);
        }

        private void TouchTile(RasterTileCacheKey key)
        {
            if (!_tileLruNodes.TryGetValue(key, out LinkedListNode<RasterTileCacheKey>? node))
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
                RasterTileCacheKey key = _tileLru.First.Value;
                _tileLru.RemoveFirst();
                _tileLruNodes.Remove(key);

                if (_tileCache.Remove(key, out Bitmap? bitmap))
                {
                    bitmap.Dispose();
                }
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

        private void DrawBitmap(
            Graphics graphics,
            Bitmap bitmap,
            RectangleF destination)
        {
            if (_opacityImageAttributes == null)
            {
                using ImageAttributes imageAttributes = new();
                imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(
                    bitmap,
                    CreateIntegerDestinationRectangle(destination),
                    0,
                    0,
                    bitmap.Width,
                    bitmap.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
                return;
            }

            Rectangle destinationRectangle =
                CreateIntegerDestinationRectangle(destination);
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

        private bool TryCreateReadWindow(
            RectangleD worldBounds,
            bool interactive,
            out RasterReadWindow readWindow)
        {
            PointD[] pixels =
            [
                WorldToPixel(new PointD(MinX(worldBounds), MinY(worldBounds))),
                WorldToPixel(new PointD(MaxX(worldBounds), MinY(worldBounds))),
                WorldToPixel(new PointD(MaxX(worldBounds), MaxY(worldBounds))),
                WorldToPixel(new PointD(MinX(worldBounds), MaxY(worldBounds)))
            ];

            int minPixel = (int)Math.Floor(pixels.Min(point => point.X)) - 1;
            int maxPixel = (int)Math.Ceiling(pixels.Max(point => point.X)) + 1;
            int minLine = (int)Math.Floor(pixels.Min(point => point.Y)) - 1;
            int maxLine = (int)Math.Ceiling(pixels.Max(point => point.Y)) + 1;

            int x = Math.Clamp(minPixel, 0, SourceWidth - 1);
            int y = Math.Clamp(minLine, 0, SourceHeight - 1);
            int right = Math.Clamp(maxPixel, x + 1, SourceWidth);
            int bottom = Math.Clamp(maxLine, y + 1, SourceHeight);

            int width = right - x;
            int height = bottom - y;

            if (width <= 0 || height <= 0)
            {
                readWindow = default;
                return false;
            }

            readWindow = new RasterReadWindow(x, y, width, height);

            if (interactive)
            {
                readWindow = ExpandAndSnapReadWindow(readWindow);
            }

            return true;
        }

        private RasterReadWindow ExpandAndSnapReadWindow(RasterReadWindow readWindow)
        {
            int horizontalMargin = Math.Max(InteractiveTileSize, readWindow.Width / 2);
            int verticalMargin = Math.Max(InteractiveTileSize, readWindow.Height / 2);

            int x = Math.Max(0, SnapDown(readWindow.X - horizontalMargin, InteractiveTileSize));
            int y = Math.Max(0, SnapDown(readWindow.Y - verticalMargin, InteractiveTileSize));
            int right = Math.Min(SourceWidth, SnapUp(readWindow.X + readWindow.Width + horizontalMargin, InteractiveTileSize));
            int bottom = Math.Min(SourceHeight, SnapUp(readWindow.Y + readWindow.Height + verticalMargin, InteractiveTileSize));

            return new RasterReadWindow(
                x,
                y,
                Math.Max(1, right - x),
                Math.Max(1, bottom - y));
        }

        private RectangleD GetWorldBounds(RasterReadWindow readWindow)
        {
            PointD[] corners =
            [
                PixelToWorld(_geoTransform, readWindow.X, readWindow.Y),
                PixelToWorld(_geoTransform, readWindow.X + readWindow.Width, readWindow.Y),
                PixelToWorld(_geoTransform, readWindow.X + readWindow.Width, readWindow.Y + readWindow.Height),
                PixelToWorld(_geoTransform, readWindow.X, readWindow.Y + readWindow.Height)
            ];

            double minX = corners.Min(corner => corner.X);
            double maxX = corners.Max(corner => corner.X);
            double minY = corners.Min(corner => corner.Y);
            double maxY = corners.Max(corner => corner.Y);

            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        private RasterReadWindow ToBaseReadWindow(
            RasterReadWindow overviewWindow,
            double decimation)
        {
            int baseX = (int)Math.Floor(overviewWindow.X * decimation);
            int baseY = (int)Math.Floor(overviewWindow.Y * decimation);
            int baseRight = (int)Math.Ceiling((overviewWindow.X + overviewWindow.Width) * decimation);
            int baseBottom = (int)Math.Ceiling((overviewWindow.Y + overviewWindow.Height) * decimation);

            baseX = Math.Clamp(baseX, 0, SourceWidth - 1);
            baseY = Math.Clamp(baseY, 0, SourceHeight - 1);
            baseRight = Math.Clamp(baseRight, baseX + 1, SourceWidth);
            baseBottom = Math.Clamp(baseBottom, baseY + 1, SourceHeight);

            return new RasterReadWindow(
                baseX,
                baseY,
                Math.Max(1, baseRight - baseX),
                Math.Max(1, baseBottom - baseY));
        }

        private PointD WorldToPixel(PointD worldPoint)
        {
            double deltaX = worldPoint.X - _geoTransform[0];
            double deltaY = worldPoint.Y - _geoTransform[3];

            return new PointD(
                _inverseGeoTransform[0] +
                _inverseGeoTransform[1] * deltaX +
                _inverseGeoTransform[2] * deltaY,
                _inverseGeoTransform[3] +
                _inverseGeoTransform[4] * deltaX +
                _inverseGeoTransform[5] * deltaY);
        }

        private static Bitmap CreateArgbBitmap(
            byte[] red,
            byte[] green,
            byte[] blue,
            byte[] alpha,
            int width,
            int height)
        {
            Bitmap bitmap = new(width, height, PixelFormat.Format32bppPArgb);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);

            try
            {
                int stride = data.Stride;
                byte[] pixels = new byte[stride * height];

                for (int y = 0; y < height; y++)
                {
                    int rowOffset = y * stride;
                    int sourceOffset = y * width;

                    for (int x = 0; x < width; x++)
                    {
                        int sourceIndex = sourceOffset + x;
                        int pixelIndex = rowOffset + x * 4;

                        byte alphaValue = alpha[sourceIndex];
                        pixels[pixelIndex] = Premultiply(blue[sourceIndex], alphaValue);
                        pixels[pixelIndex + 1] = Premultiply(green[sourceIndex], alphaValue);
                        pixels[pixelIndex + 2] = Premultiply(red[sourceIndex], alphaValue);
                        pixels[pixelIndex + 3] = alphaValue;
                    }
                }

                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        private static double? TryGetNoDataValue(Band band)
        {
            band.GetNoDataValue(out double noDataValue, out int hasNoData);
            return hasNoData != 0 &&
                   noDataValue >= byte.MinValue &&
                   noDataValue <= byte.MaxValue &&
                   !double.IsNaN(noDataValue) &&
                   !double.IsInfinity(noDataValue)
                ? noDataValue
                : null;
        }

        private static void ApplySingleBandNoData(
            byte[] values,
            byte[] alpha,
            double? noDataValue)
        {
            if (!noDataValue.HasValue)
            {
                return;
            }

            byte noData = ClampNoDataByte(noDataValue.Value);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == noData)
                {
                    alpha[i] = 0;
                }
            }
        }

        private static void ApplyRgbNoData(
            byte[] red,
            byte[] green,
            byte[] blue,
            byte[] alpha,
            double? redNoData,
            double? greenNoData,
            double? blueNoData)
        {
            bool hasRedNoData = redNoData.HasValue;
            bool hasGreenNoData = greenNoData.HasValue;
            bool hasBlueNoData = blueNoData.HasValue;
            if (!hasRedNoData && !hasGreenNoData && !hasBlueNoData)
            {
                return;
            }

            byte redNoDataByte = hasRedNoData ? ClampNoDataByte(redNoData!.Value) : byte.MinValue;
            byte greenNoDataByte = hasGreenNoData ? ClampNoDataByte(greenNoData!.Value) : byte.MinValue;
            byte blueNoDataByte = hasBlueNoData ? ClampNoDataByte(blueNoData!.Value) : byte.MinValue;

            for (int i = 0; i < red.Length; i++)
            {
                bool isNoData =
                    (!hasRedNoData || red[i] == redNoDataByte) &&
                    (!hasGreenNoData || green[i] == greenNoDataByte) &&
                    (!hasBlueNoData || blue[i] == blueNoDataByte);

                if (isNoData)
                {
                    alpha[i] = 0;
                }
            }
        }

        private static void RemoveBlackNoDataCollar(
    byte[] red, byte[] green, byte[] blue, byte[] alpha,
    int width, int height)
        {
            // Flood-fill from all 4 edges — only removes black pixels
            // reachable from the border, not interior black pixels.
            bool[] visited = new bool[width * height];
            Queue<int> queue = new();

            void TrySeed(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return;
                int idx = y * width + x;
                if (visited[idx]) return;
                if (red[idx] != 0 || green[idx] != 0 || blue[idx] != 0) return;
                visited[idx] = true;
                queue.Enqueue(idx);
            }

            // Seed from all 4 edges
            for (int x = 0; x < width; x++)
            {
                TrySeed(x, 0);
                TrySeed(x, height - 1);
            }
            for (int y = 1; y < height - 1; y++)
            {
                TrySeed(0, y);
                TrySeed(width - 1, y);
            }

            // Flood fill
            while (queue.Count > 0)
            {
                int idx = queue.Dequeue();
                alpha[idx] = 0;  // make transparent
                int x = idx % width;
                int y = idx / width;
                TrySeed(x - 1, y);
                TrySeed(x + 1, y);
                TrySeed(x, y - 1);
                TrySeed(x, y + 1);
            }
        }

        private static void RemoveWhiteNoDataCollar(
            byte[] red,
            byte[] green,
            byte[] blue,
            byte[] alpha,
            int width,
            int height)
        {
            bool[] visited = new bool[width * height];
            Queue<int> queue = new();

            void TrySeed(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return;
                int idx = y * width + x;
                if (visited[idx]) return;
                if (alpha[idx] == 0) return;
                if (red[idx] < 248 || green[idx] < 248 || blue[idx] < 248) return;
                visited[idx] = true;
                queue.Enqueue(idx);
            }

            for (int x = 0; x < width; x++)
            {
                TrySeed(x, 0);
                TrySeed(x, height - 1);
            }
            for (int y = 1; y < height - 1; y++)
            {
                TrySeed(0, y);
                TrySeed(width - 1, y);
            }

            while (queue.Count > 0)
            {
                int idx = queue.Dequeue();
                alpha[idx] = 0;
                int x = idx % width;
                int y = idx / width;
                TrySeed(x - 1, y);
                TrySeed(x + 1, y);
                TrySeed(x, y - 1);
                TrySeed(x, y + 1);
            }
        }

        private static string ResolveLayerFilePath(
            string storedPath,
            string? projectFolderPath)
        {
            if (Path.IsPathRooted(storedPath))
                return Path.GetFullPath(storedPath);

            if (string.IsNullOrWhiteSpace(projectFolderPath))
                return Path.GetFullPath(storedPath);

            return Path.GetFullPath(Path.Combine(projectFolderPath, storedPath));
        }

        private static RectangleD GetWorldBounds(
            Dataset dataset,
            double[] transform)
        {
            PointD[] corners =
            [
                PixelToWorld(transform, 0, 0),
                PixelToWorld(transform, dataset.RasterXSize, 0),
                PixelToWorld(transform, dataset.RasterXSize, dataset.RasterYSize),
                PixelToWorld(transform, 0, dataset.RasterYSize)
            ];

            double minX = corners.Min(corner => corner.X);
            double maxX = corners.Max(corner => corner.X);
            double minY = corners.Min(corner => corner.Y);
            double maxY = corners.Max(corner => corner.Y);

            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        private static PointD PixelToWorld(
            double[] transform,
            double pixel,
            double line)
        {
            return new PointD(
                transform[0] + pixel * transform[1] + line * transform[2],
                transform[3] + pixel * transform[4] + line * transform[5]);
        }

        private static bool TryInvertGeoTransform(
            double[] transform,
            out double[] inverseTransform)
        {
            inverseTransform = new double[6];

            double determinant = transform[1] * transform[5] -
                                 transform[2] * transform[4];

            if (Math.Abs(determinant) < 1e-18)
                return false;

            inverseTransform[0] = 0.0;
            inverseTransform[1] = transform[5] / determinant;
            inverseTransform[2] = -transform[2] / determinant;
            inverseTransform[3] = 0.0;
            inverseTransform[4] = -transform[4] / determinant;
            inverseTransform[5] = transform[1] / determinant;
            return true;
        }

        private static bool TryGetIntersection(
            RectangleD first,
            RectangleD second,
            out RectangleD intersection)
        {
            double left = Math.Max(MinX(first), MinX(second));
            double right = Math.Min(MaxX(first), MaxX(second));
            double bottom = Math.Max(MinY(first), MinY(second));
            double top = Math.Min(MaxY(first), MaxY(second));

            if (right <= left || top <= bottom)
            {
                intersection = default;
                return false;
            }

            intersection = new RectangleD(left, bottom, right - left, top - bottom);
            return true;
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

        private static bool Intersects(
            RasterReadWindow first,
            RasterReadWindow second)
        {
            return first.X < second.X + second.Width &&
                   first.X + first.Width > second.X &&
                   first.Y < second.Y + second.Height &&
                   first.Y + first.Height > second.Y;
        }

        private static Envelope CreateEnvelope(RectangleD bounds)
        {
            return new Envelope(
                MinX(bounds),
                MaxX(bounds),
                MinY(bounds),
                MaxY(bounds));
        }

        private static InterpolationMode ResolveInterpolationMode(
            bool interactive,
            RasterReadContext context)
        {
            return InterpolationMode.NearestNeighbor;
        }

        private static Size GetTargetBitmapSize(RectangleF destination, bool interactive)
        {
            int width = Math.Max(1, (int)Math.Ceiling(destination.Width));
            int height = Math.Max(1, (int)Math.Ceiling(destination.Height));
            double pixelCount = (double)width * height;
            int maxPixels = interactive
                ? MaxInteractiveCachePixels
                : MaxCachePixels;

            if (pixelCount <= maxPixels)
                return new Size(width, height);

            double scale = Math.Sqrt(maxPixels / pixelCount);
            return new Size(
                Math.Max(1, (int)Math.Round(width * scale)),
                Math.Max(1, (int)Math.Round(height * scale)));
        }

        private static int SnapDown(int value, int interval) =>
            (int)Math.Floor(value / (double)interval) * interval;

        private static int SnapUp(int value, int interval) =>
            (int)Math.Ceiling(value / (double)interval) * interval;

        private static double MinX(RectangleD rectangle) =>
            Math.Min(rectangle.Left, rectangle.Right);

        private static double MaxX(RectangleD rectangle) =>
            Math.Max(rectangle.Left, rectangle.Right);

        private static double MinY(RectangleD rectangle) =>
            Math.Min(rectangle.Top, rectangle.Bottom);

        private static double MaxY(RectangleD rectangle) =>
            Math.Max(rectangle.Top, rectangle.Bottom);

        private static byte ClampByte(short value)
        {
            return (byte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, value));
        }

        private static byte ClampNoDataByte(double value)
        {
            return (byte)Math.Clamp(
                (int)Math.Round(value, MidpointRounding.AwayFromZero),
                byte.MinValue,
                byte.MaxValue);
        }

        private static byte Premultiply(byte color, byte alpha)
        {
            return alpha == byte.MaxValue
                ? color
                : (byte)((color * alpha + 127) / 255);
        }

        private sealed class RasterTileDiskCache
        {
            private const string CacheRootFolderName = "RePlotRasterTileCache";
            private const string CacheFormatVersion = "raster-tile-cache-v3";
            private const int MaxPendingWrites = 8;
            private readonly string _cacheFolder;
            private readonly ConcurrentDictionary<RasterTileCacheKey, byte> _pendingWrites = new ConcurrentDictionary<RasterTileCacheKey, byte>();

            private RasterTileDiskCache(string cacheFolder)
            {
                _cacheFolder = cacheFolder;
                Directory.CreateDirectory(_cacheFolder);
            }

            public static RasterTileDiskCache Create(
                string filePath,
                int sourceWidth,
                int sourceHeight)
            {
                string fullPath = Path.GetFullPath(filePath);
                DateTime lastWrite = File.Exists(fullPath)
                    ? File.GetLastWriteTimeUtc(fullPath)
                    : DateTime.MinValue;
                long fileLength = File.Exists(fullPath)
                    ? new FileInfo(fullPath).Length
                    : 0L;
                string signature = string.Join(
                    "|",
                    CacheFormatVersion,
                    fullPath,
                    lastWrite.Ticks,
                    fileLength,
                    sourceWidth,
                    sourceHeight);
                string hash = Convert.ToHexString(
                    SHA256.HashData(Encoding.UTF8.GetBytes(signature)));
                string cacheFolder = Path.Combine(
                    Path.GetTempPath(),
                    CacheRootFolderName,
                    hash);
                return new RasterTileDiskCache(cacheFolder);
            }

            public bool TryRead(
                RasterTileCacheKey key,
                out Bitmap bitmap)
            {
                string path = GetTilePath(key);
                if (!File.Exists(path))
                {
                    bitmap = null!;
                    return false;
                }

                try
                {
                    using Image image = Image.FromFile(path);
                    Bitmap converted = new(
                        image.Width,
                        image.Height,
                        PixelFormat.Format32bppPArgb);
                    using Graphics graphics = Graphics.FromImage(converted);
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImageUnscaled(image, 0, 0);
                    bitmap = converted;
                    return true;
                }
                catch
                {
                    TryDelete(path);
                    bitmap = null!;
                    return false;
                }
            }

            public void QueueWrite(
                RasterTileCacheKey key,
                Bitmap bitmap)
            {
                string path = GetTilePath(key);
                if (File.Exists(path) ||
                    _pendingWrites.Count >= MaxPendingWrites ||
                    !_pendingWrites.TryAdd(key, 0))
                {
                    return;
                }

                Bitmap bitmapCopy;
                try
                {
                    bitmapCopy = bitmap.Clone(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        PixelFormat.Format32bppPArgb);
                }
                catch
                {
                    _pendingWrites.TryRemove(key, out _);
                    return;
                }

                _ = Task.Run(() =>
                {
                    try
                    {
                        TryWrite(key, bitmapCopy);
                    }
                    finally
                    {
                        bitmapCopy.Dispose();
                        _pendingWrites.TryRemove(key, out _);
                    }
                });
            }

            private void TryWrite(
                RasterTileCacheKey key,
                Bitmap bitmap)
            {
                string path = GetTilePath(key);
                string tempPath = $"{path}.{Guid.NewGuid():N}.tmp";

                try
                {
                    Directory.CreateDirectory(_cacheFolder);
                    bitmap.Save(tempPath, ImageFormat.Png);

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    File.Move(tempPath, path);
                }
                catch
                {
                    TryDelete(tempPath);
                }
            }

            private string GetTilePath(RasterTileCacheKey key)
            {
                string fileName = string.Join(
                    "_",
                    key.OverviewIndex,
                    key.SourceX,
                    key.SourceY,
                    key.SourceWidth,
                    key.SourceHeight) + ".png";
                return Path.Combine(_cacheFolder, fileName);
            }

            private static void TryDelete(string path)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch
                {
                    // Cache cleanup is best-effort only.
                }
            }
        }

        private readonly record struct RasterReadContext(
            int OverviewIndex,
            double Decimation,
            RasterReadWindow OverviewWindow);

        private sealed record RasterTileIndex(
            STRtree<RasterTileDescriptor> SpatialIndex);

        private readonly record struct RasterTileDescriptor(
            RasterTileCacheKey CacheKey,
            RasterReadWindow TileWindow,
            RasterReadWindow BaseWindow,
            RectangleD WorldBounds);

        private readonly record struct RasterOverviewInfo(
            int Width,
            int Height,
            double Decimation);

        private readonly record struct RasterTileCacheKey(
            int OverviewIndex,
            int SourceX,
            int SourceY,
            int SourceWidth,
            int SourceHeight);

        private readonly record struct RasterBandSelection(
            RasterBandMode Mode,
            int ReferenceBand,
            int PaletteBand,
            int RedBand,
            int GreenBand,
            int BlueBand,
            int? AlphaBand,
            int GrayBand);

        private enum RasterBandMode
        {
            Palette,
            Rgb,
            Gray
        }

        private readonly record struct RasterReadWindow(
            int X,
            int Y,
            int Width,
            int Height);
    }
}
