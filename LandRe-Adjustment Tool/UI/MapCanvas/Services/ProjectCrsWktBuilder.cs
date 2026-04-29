using Land_Readjustment_Tool.Core.Entities.Spatial;
using OSGeo.OSR;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    internal static class ProjectCrsWktBuilder
    {
        public static string BuildTargetSrsDefinition(
            CoordinateSystem coordinateSystem,
            DatumTransformation? datumTransformation)
        {
            ArgumentNullException.ThrowIfNull(coordinateSystem);

            GdalConfiguration.ConfigureGdal();
            if (!GdalConfiguration.Usable)
            {
                throw new InvalidOperationException(
                    "GDAL/OSR native libraries are not configured correctly. Raster CRS transformation cannot continue.");
            }

            if (coordinateSystem.EpsgCode.HasValue)
                return $"EPSG:{coordinateSystem.EpsgCode.Value}";

            ProjectionParameters parameters = coordinateSystem.ProjectionParameters
                ?? throw new InvalidOperationException(
                    $"Projection parameters are missing for {coordinateSystem.Code}.");

            if (!string.IsNullOrWhiteSpace(parameters.WktDefinition))
                return parameters.WktDefinition;

            SpatialReference spatialReference = new(string.Empty);
            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);

            CheckOsr(
                spatialReference.SetProjCS(coordinateSystem.Name),
                "set projected CRS name");

            string ellipsoidName = string.IsNullOrWhiteSpace(parameters.Ellipsoid)
                ? "WGS 84"
                : parameters.Ellipsoid;

            double semiMajorAxis = parameters.SemiMajorAxis ?? 6378137.0;
            double inverseFlattening = parameters.InverseFlattening ?? 298.257223563;
            string datumName = ellipsoidName.Contains(
                "Everest",
                StringComparison.OrdinalIgnoreCase)
                    ? "Everest_1830"
                    : "WGS_1984";

            CheckOsr(
                spatialReference.SetGeogCS(
                    $"{datumName} geographic",
                    datumName,
                    ellipsoidName,
                    semiMajorAxis,
                    inverseFlattening,
                    "Greenwich",
                    0.0,
                    "degree",
                    Math.PI / 180.0),
                "set geographic CRS");

            if (datumTransformation != null)
            {
                CheckOsr(
                    spatialReference.SetTOWGS84(
                        datumTransformation.DeltaX,
                        datumTransformation.DeltaY,
                        datumTransformation.DeltaZ,
                        datumTransformation.RotationX,
                        datumTransformation.RotationY,
                        datumTransformation.RotationZ,
                        datumTransformation.ScalePpm),
                    "set datum transformation");
            }

            CheckOsr(
                spatialReference.SetTM(
                    parameters.LatitudeOfOrigin ?? 0.0,
                    parameters.CentralMeridian
                        ?? throw new InvalidOperationException(
                            $"Central meridian is missing for {coordinateSystem.Code}."),
                    parameters.ScaleFactor ?? 1.0,
                    parameters.FalseEasting ?? 0.0,
                    parameters.FalseNorthing ?? 0.0),
                "set Transverse Mercator projection");

            CheckOsr(
                spatialReference.SetLinearUnits("metre", 1.0),
                "set linear units");

            CheckOsr(
                spatialReference.ExportToWkt(out string wkt, []),
                "export CRS WKT");

            return wkt;
        }

        private static void CheckOsr(int errorCode, string operation)
        {
            if (errorCode != 0)
                throw new InvalidOperationException(
                    $"GDAL OSR failed to {operation}. Error code: {errorCode}.");
        }
    }
}
