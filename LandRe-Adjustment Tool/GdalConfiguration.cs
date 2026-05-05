using OSGeo.GDAL;

namespace Land_Readjustment_Tool
{
    public static class GdalBootstrapper
    {
        public static void ConfigureAll()
        {
            GdalConfiguration.ConfigureGdal();
            GdalConfiguration.ConfigureOgr();
            ApplyNetworkOptions();
        }

        public static void ApplyNetworkOptions()
        {
            Gdal.SetConfigOption("GDAL_HTTP_UNSAFESSL", "YES");
            Gdal.SetConfigOption("GDAL_DISABLE_READDIR_ON_OPEN", "EMPTY_DIR");
            Gdal.SetConfigOption("CPL_VSIL_CURL_CACHE_SIZE", "128000000");
            Gdal.SetConfigOption("GDAL_HTTP_MAX_RETRY", "3");
            Gdal.SetConfigOption("GDAL_HTTP_RETRY_DELAY", "1");
        }
    }
}
