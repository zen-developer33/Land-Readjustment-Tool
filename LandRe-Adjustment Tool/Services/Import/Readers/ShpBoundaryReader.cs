using Land_Readjustment_Tool.Core.Interfaces.Import;
using Land_Readjustment_Tool.Core.Models.Import;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Services.Import.Readers
{
    public sealed class ShpBoundaryReader : OgrBoundaryReaderBase, IBoundaryFileReader
    {
        public ShpBoundaryReader()
            : base("SHP")
        {
        }

        public VectorFileInfo Inspect(string filePath)
        {
            return InspectWithOgr(
                filePath,
                forceWgs84: false,
                singleLayerForShapefile: true);
        }

        public IReadOnlyList<Geometry> ReadGeometries(
            string filePath,
            BoundaryImportOptions options)
        {
            return ReadOgrGeometries(
                filePath,
                options,
                singleLayerForShapefile: true);
        }
    }
}
