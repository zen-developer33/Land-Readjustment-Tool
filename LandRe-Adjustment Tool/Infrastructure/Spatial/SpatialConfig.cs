using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Infrastructure.Spatial
{
    public static class SpatialConfig
    {
        public const int SRID = 32644;

        public static readonly GeometryFactory Factory =
            NtsGeometryServices.Instance.CreateGeometryFactory(srid: SRID);
    }
}
