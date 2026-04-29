namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Creates local GDAL source definitions for web XYZ tile imports.
    /// </summary>
    public interface IXyzTileSourceService
    {
        /// <summary>
        /// Creates a local GDAL WMS/TMS XML file for a web XYZ tile URL template.
        /// </summary>
        XyzTileSourceDefinition CreateSourceDefinition(
            string projectFolderPath,
            XyzTileSourceImportRequest request);
    }
}
