using Land_Readjustment_Tool.Core.Interfaces.Import;
using Land_Readjustment_Tool.Services.Import.Readers;

namespace Land_Readjustment_Tool.Services.Import
{
    public interface IBoundaryReaderFactory
    {
        IBoundaryFileReader GetReader(string filePath);
    }

    public sealed class BoundaryReaderFactory : IBoundaryReaderFactory
    {
        private readonly DxfBoundaryReader _dxf;
        private readonly ShpBoundaryReader _shp;
        private readonly KmlBoundaryReader _kml;

        public BoundaryReaderFactory(
            DxfBoundaryReader dxf,
            ShpBoundaryReader shp,
            KmlBoundaryReader kml)
        {
            _dxf = dxf;
            _shp = shp;
            _kml = kml;
        }

        public IBoundaryFileReader GetReader(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".dxf" => _dxf,
                ".shp" => _shp,
                ".kml" or ".kmz" => _kml,
                _ => throw new NotSupportedException(
                    $"Boundary format not supported: {Path.GetExtension(filePath)}")
            };
        }
    }
}
