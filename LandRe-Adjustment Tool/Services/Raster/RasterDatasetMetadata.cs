namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Lightweight raster metadata used by import workflow, logging, and layer descriptions.
    /// </summary>
    public sealed class RasterDatasetMetadata
    {
        public RasterDatasetMetadata(
            string sourcePath,
            long fileSizeBytes,
            string driverShortName,
            string driverLongName,
            int width,
            int height,
            int bandCount,
            bool hasGeoTransform,
            double[] geoTransform,
            int groundControlPointCount,
            string projectionWkt,
            string projectionSource,
            bool hasCoordinateSystem,
            string coordinateSystemType,
            string coordinateSystemName,
            string coordinateSystemAuthority)
        {
            SourcePath = sourcePath;
            FileSizeBytes = fileSizeBytes;
            DriverShortName = driverShortName;
            DriverLongName = driverLongName;
            Width = width;
            Height = height;
            BandCount = bandCount;
            HasGeoTransform = hasGeoTransform;
            GeoTransform = [.. geoTransform];
            GroundControlPointCount = groundControlPointCount;
            ProjectionWkt = projectionWkt;
            ProjectionSource = projectionSource;
            HasCoordinateSystem = hasCoordinateSystem;
            CoordinateSystemType = coordinateSystemType;
            CoordinateSystemName = coordinateSystemName;
            CoordinateSystemAuthority = coordinateSystemAuthority;
        }

        public string SourcePath { get; }
        public long FileSizeBytes { get; }
        public string DriverShortName { get; }
        public string DriverLongName { get; }
        public int Width { get; }
        public int Height { get; }
        public int BandCount { get; }
        public bool HasGeoTransform { get; }
        public double[] GeoTransform { get; }
        public int GroundControlPointCount { get; }
        public string ProjectionWkt { get; }
        public string ProjectionSource { get; }
        public bool HasCoordinateSystem { get; }
        public string CoordinateSystemType { get; }
        public string CoordinateSystemName { get; }
        public string CoordinateSystemAuthority { get; }
        public bool HasProjection => !string.IsNullOrWhiteSpace(ProjectionWkt);
        public bool HasGeoreferencing => HasGeoTransform || GroundControlPointCount > 0;
    }
}
