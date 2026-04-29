using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using OSGeo.GDAL;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class RasterRenderLayer : IDisposable
    {
        private const int MaxCachePixels = 12_000_000;
        private const int MaxInteractiveCachePixels = 1_500_000;
        private const int InteractiveTileSize = 256;

        private readonly Dataset _dataset;
        private readonly double[] _geoTransform;
        private readonly double[] _inverseGeoTransform;
        private Bitmap? _visibleBitmapCache;
        private RasterViewCacheKey? _visibleBitmapCacheKey;

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
            Transparency = Math.Clamp(transparency, 0, 100);
            IsVisible = true;
        }

        public int LayerId { get; }
        public string Name { get; }
        public string FilePath { get; }
        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public RectangleD WorldBounds { get; }
        public int Transparency { get; private set; }
        public bool IsVisible { get; set; }

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
            bool interactive)
        {
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
            RasterViewCacheKey cacheKey = new(
                readWindow.X,
                readWindow.Y,
                readWindow.Width,
                readWindow.Height,
                targetSize.Width,
                targetSize.Height);

            Bitmap bitmap = GetOrCreateVisibleBitmap(readWindow, targetSize, cacheKey);

            GraphicsState state = graphics.Save();
            try
            {
                graphics.InterpolationMode = interactive
                    ? InterpolationMode.NearestNeighbor
                    : InterpolationMode.HighQualityBilinear;
                graphics.PixelOffsetMode = interactive
                    ? PixelOffsetMode.HighSpeed
                    : PixelOffsetMode.Half;
                graphics.CompositingMode = CompositingMode.SourceOver;
                DrawBitmap(graphics, bitmap, destination);
            }
            finally
            {
                graphics.Restore(state);
            }

            return true;
        }

        /// <summary>
        /// Updates lightweight raster render flags without rebuilding the GDAL dataset or bitmap cache.
        /// </summary>
        public void UpdateRenderState(bool isVisible, int transparency)
        {
            IsVisible = isVisible;
            Transparency = Math.Clamp(transparency, 0, 100);
        }

        public void InvalidateCache()
        {
            _visibleBitmapCache?.Dispose();
            _visibleBitmapCache = null;
            _visibleBitmapCacheKey = null;
        }

        public void Dispose()
        {
            InvalidateCache();
            _dataset.Dispose();
        }

        private Bitmap GetOrCreateVisibleBitmap(
            RasterReadWindow readWindow,
            Size targetSize,
            RasterViewCacheKey cacheKey)
        {
            if (_visibleBitmapCache != null &&
                _visibleBitmapCacheKey == cacheKey)
            {
                return _visibleBitmapCache;
            }

            _visibleBitmapCache?.Dispose();
            _visibleBitmapCache = CreateBitmap(readWindow, targetSize);
            _visibleBitmapCacheKey = cacheKey;
            return _visibleBitmapCache;
        }

        private Bitmap CreateBitmap(
            RasterReadWindow readWindow,
            Size targetSize)
        {
            byte[] red;
            byte[] green;
            byte[] blue;
            byte[] alpha = Enumerable.Repeat(
                (byte)255,
                targetSize.Width * targetSize.Height).ToArray();

            int? paletteBandIndex = FindBand(ColorInterp.GCI_PaletteIndex);
            if (paletteBandIndex.HasValue)
            {
                using Band paletteBand = _dataset.GetRasterBand(paletteBandIndex.Value);
                (red, green, blue, alpha) = ReadPaletteBand(
                    paletteBand,
                    readWindow,
                    targetSize);
            }
            else if (_dataset.RasterCount >= 3)
            {
                int redBand = FindBand(ColorInterp.GCI_RedBand) ?? 1;
                int greenBand = FindBand(ColorInterp.GCI_GreenBand) ?? 2;
                int blueBand = FindBand(ColorInterp.GCI_BlueBand) ?? 3;

                using Band redRasterBand = _dataset.GetRasterBand(redBand);
                using Band greenRasterBand = _dataset.GetRasterBand(greenBand);
                using Band blueRasterBand = _dataset.GetRasterBand(blueBand);

                red = ReadByteBand(redRasterBand, readWindow, targetSize);
                green = ReadByteBand(greenRasterBand, readWindow, targetSize);
                blue = ReadByteBand(blueRasterBand, readWindow, targetSize);

                int? alphaBand = FindBand(ColorInterp.GCI_AlphaBand);
                if (alphaBand.HasValue)
                {
                    using Band alphaRasterBand = _dataset.GetRasterBand(alphaBand.Value);
                    alpha = ReadByteBand(alphaRasterBand, readWindow, targetSize);
                }
            }
            else
            {
                using Band grayRasterBand = _dataset.GetRasterBand(1);
                red = ReadByteBand(grayRasterBand, readWindow, targetSize);
                green = red;
                blue = red;
            }

            return CreateArgbBitmap(red, green, blue, alpha, targetSize.Width, targetSize.Height);
        }

        private void DrawBitmap(
            Graphics graphics,
            Bitmap bitmap,
            RectangleF destination)
        {
            double opacityFactor = (100 - Math.Clamp(Transparency, 0, 100)) / 100d;

            if (opacityFactor >= 1d)
            {
                graphics.DrawImage(bitmap, destination);
                return;
            }

            using ImageAttributes imageAttributes = new();
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

            Rectangle destinationRectangle = Rectangle.Round(destination);
            graphics.DrawImage(
                bitmap,
                destinationRectangle,
                0,
                0,
                bitmap.Width,
                bitmap.Height,
                GraphicsUnit.Pixel,
                imageAttributes);
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

        private int? FindBand(ColorInterp colorInterp)
        {
            for (int bandIndex = 1; bandIndex <= _dataset.RasterCount; bandIndex++)
            {
                using Band band = _dataset.GetRasterBand(bandIndex);
                if (band.GetRasterColorInterpretation() == colorInterp)
                    return bandIndex;
            }

            return null;
        }

        private static byte[] ReadByteBand(
            Band band,
            RasterReadWindow readWindow,
            Size targetSize)
        {
            byte[] buffer = new byte[targetSize.Width * targetSize.Height];
            CPLErr result = band.ReadRaster(
                readWindow.X,
                readWindow.Y,
                readWindow.Width,
                readWindow.Height,
                buffer,
                targetSize.Width,
                targetSize.Height,
                0,
                0);

            if (result != CPLErr.CE_None)
                throw new InvalidOperationException(
                    $"GDAL failed to read raster band. Error: {result}.");

            return buffer;
        }

        private static (byte[] Red, byte[] Green, byte[] Blue, byte[] Alpha)
            ReadPaletteBand(
                Band band,
                RasterReadWindow readWindow,
                Size targetSize)
        {
            byte[] indexes = ReadByteBand(
                band,
                readWindow,
                targetSize);

            byte[] red = new byte[indexes.Length];
            byte[] green = new byte[indexes.Length];
            byte[] blue = new byte[indexes.Length];
            byte[] alpha = new byte[indexes.Length];
            ColorTable colorTable = band.GetRasterColorTable();

            for (int i = 0; i < indexes.Length; i++)
            {
                ColorEntry entry = new();
                if (colorTable != null &&
                    colorTable.GetColorEntryAsRGB(indexes[i], entry) != 0)
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
                    alpha[i] = 255;
                }
            }

            return (red, green, blue, alpha);
        }

        private static Bitmap CreateArgbBitmap(
            byte[] red,
            byte[] green,
            byte[] blue,
            byte[] alpha,
            int width,
            int height)
        {
            Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

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

                        pixels[pixelIndex] = blue[sourceIndex];
                        pixels[pixelIndex + 1] = green[sourceIndex];
                        pixels[pixelIndex + 2] = red[sourceIndex];
                        pixels[pixelIndex + 3] = alpha[sourceIndex];
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

        private readonly record struct RasterReadWindow(
            int X,
            int Y,
            int Width,
            int Height);

        private readonly record struct RasterViewCacheKey(
            int SourceX,
            int SourceY,
            int SourceWidth,
            int SourceHeight,
            int TargetWidth,
            int TargetHeight);
    }
}
