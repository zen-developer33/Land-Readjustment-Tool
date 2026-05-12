using Land_Readjustment_Tool.Core.Models.Import;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OSGeo.OGR;
using OSGeo.OSR;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;

namespace Land_Readjustment_Tool.Services.Import.Readers
{
    public abstract class OgrBoundaryReaderBase
    {
        private readonly string _fileFormat;

        protected OgrBoundaryReaderBase(string fileFormat)
        {
            _fileFormat = fileFormat;
        }

        protected VectorFileInfo InspectWithOgr(
            string filePath,
            bool forceWgs84,
            bool singleLayerForShapefile)
        {
            GdalBootstrapper.ConfigureAll();

            using DataSource dataSource = Ogr.Open(filePath, 0)
                ?? throw new InvalidOperationException($"Could not open vector file: {filePath}");

            List<VectorLayerInfo> layers = [];
            string? detectedCrs = forceWgs84 ? "EPSG:4326" : null;

            int layerCount = dataSource.GetLayerCount();
            for (int i = 0; i < layerCount; i++)
            {
                using Layer layer = dataSource.GetLayerByIndex(i);
                string layerName = singleLayerForShapefile
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : layer.GetName();

                int polygonCount = CountPolygonalFeatures(layer);
                layers.Add(new VectorLayerInfo(layerName, polygonCount, polygonCount > 0));

                if (!forceWgs84 && detectedCrs == null)
                    detectedCrs = GetLayerCrsDefinition(layer);
            }

            bool requiresCrsFromUser = string.IsNullOrWhiteSpace(detectedCrs);
            return new VectorFileInfo(
                filePath,
                _fileFormat,
                layers,
                detectedCrs,
                requiresCrsFromUser);
        }

        protected IReadOnlyList<NtsGeometry> ReadOgrGeometries(
            string filePath,
            BoundaryImportOptions options,
            bool singleLayerForShapefile)
        {
            GdalBootstrapper.ConfigureAll();

            using DataSource dataSource = Ogr.Open(filePath, 0)
                ?? throw new InvalidOperationException($"Could not open vector file: {filePath}");

            WKTReader reader = new();
            List<NtsGeometry> geometries = [];

            int layerCount = dataSource.GetLayerCount();
            for (int i = 0; i < layerCount; i++)
            {
                using Layer layer = dataSource.GetLayerByIndex(i);
                string layerName = singleLayerForShapefile
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : layer.GetName();

                if (!string.IsNullOrWhiteSpace(options.SelectedLayerName) &&
                    !string.Equals(layerName, options.SelectedLayerName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                layer.ResetReading();
                Feature? feature;
                while ((feature = layer.GetNextFeature()) != null)
                {
                    using (feature)
                    {
                        OSGeo.OGR.Geometry ogrGeometry = feature.GetGeometryRef();
                        if (ogrGeometry == null)
                            continue;

                        ogrGeometry.ExportToWkt(out string wkt);
                        NtsGeometry? valid = BoundaryGeometryReaderHelpers
                            .ValidatePolygonalGeometry(reader.Read(wkt));
                        if (valid != null)
                            geometries.Add(valid);
                    }
                }
            }

            return geometries;
        }

        private static int CountPolygonalFeatures(Layer layer)
        {
            WKTReader reader = new();
            int count = 0;
            layer.ResetReading();

            Feature? feature;
            while ((feature = layer.GetNextFeature()) != null)
            {
                using (feature)
                {
                    OSGeo.OGR.Geometry ogrGeometry = feature.GetGeometryRef();
                    if (ogrGeometry == null)
                        continue;

                    ogrGeometry.ExportToWkt(out string wkt);
                    NtsGeometry? valid = BoundaryGeometryReaderHelpers
                        .ValidatePolygonalGeometry(reader.Read(wkt));
                    if (valid != null)
                        count++;
                }
            }

            layer.ResetReading();
            return count;
        }

        private static string? GetLayerCrsDefinition(Layer layer)
        {
            using SpatialReference spatialReference = layer.GetSpatialRef();
            if (spatialReference == null)
                return null;

            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            spatialReference.AutoIdentifyEPSG();

            string? authorityName =
                spatialReference.GetAuthorityName(null) ??
                spatialReference.GetAuthorityName("PROJCS") ??
                spatialReference.GetAuthorityName("GEOGCS");
            string? authorityCode =
                spatialReference.GetAuthorityCode(null) ??
                spatialReference.GetAuthorityCode("PROJCS") ??
                spatialReference.GetAuthorityCode("GEOGCS");

            if (!string.IsNullOrWhiteSpace(authorityName) &&
                !string.IsNullOrWhiteSpace(authorityCode))
            {
                return $"{authorityName}:{authorityCode}";
            }

            spatialReference.ExportToWkt(out string wkt, []);
            return string.IsNullOrWhiteSpace(wkt) ? null : wkt;
        }
    }
}
