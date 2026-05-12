using Land_Readjustment_Tool.Core.Models.Import;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Core.Interfaces.Import
{
    public interface IBoundaryFileReader
    {
        VectorFileInfo Inspect(string filePath);

        IReadOnlyList<Geometry> ReadGeometries(
            string filePath,
            BoundaryImportOptions options);
    }
}
