using Land_Readjustment_Tool.Core.Interfaces.Import;
using Land_Readjustment_Tool.Core.Models.Import;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Services.Import.Readers
{
    public sealed class KmlBoundaryReader : OgrBoundaryReaderBase, IBoundaryFileReader
    {
        public KmlBoundaryReader()
            : base("KML")
        {
        }

        public VectorFileInfo Inspect(string filePath)
        {
            return InspectWithOgr(
                filePath,
                forceWgs84: true,
                singleLayerForShapefile: false);
        }

        public IReadOnlyList<Geometry> ReadGeometries(
            string filePath,
            BoundaryImportOptions options)
        {
            return ReadOgrGeometries(
                filePath,
                options,
                singleLayerForShapefile: false);
        }
    }
}
