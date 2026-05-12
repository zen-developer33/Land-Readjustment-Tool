using System.Diagnostics;
using System.Runtime.InteropServices;
using OSGeo.GDAL;
using Ogr = OSGeo.OGR.Ogr;

namespace Land_Readjustment_Tool
{
    public static partial class GdalConfiguration
    {
        private static volatile bool _configuredOgr;
        private static volatile bool _configuredGdal;
        private static volatile bool _usable;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDefaultDllDirectories(uint directoryFlags);
        private const uint DllSearchFlags = 0x00001000;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddDllDirectory(string lpPathName);

        static GdalConfiguration()
        {
            string? executingDirectory = null, gdalPath = null, nativePath = null;
            try
            {
                if (!IsWindows)
                {
                    const string notSet = "_Not_set_";
                    string tmp = Gdal.GetConfigOption("GDAL_DATA", notSet);
                    _usable = tmp != notSet;
                    return;
                }

                executingDirectory = AppContext.BaseDirectory;
                if (string.IsNullOrEmpty(executingDirectory))
                    throw new InvalidOperationException("cannot get executing directory");

                SetDefaultDllDirectories(DllSearchFlags);

                gdalPath = Path.Combine(executingDirectory, "gdal");
                nativePath = Path.Combine(gdalPath, GetPlatform());
                if (!Directory.Exists(nativePath))
                    throw new DirectoryNotFoundException($"GDAL native directory not found at '{nativePath}'");
                if (!File.Exists(Path.Combine(nativePath, "gdal_wrap.dll")))
                    throw new FileNotFoundException($"GDAL native wrapper not found at '{Path.Combine(nativePath, "gdal_wrap.dll")}'");

                AddDllDirectory(nativePath);
                AddDllDirectory(Path.Combine(nativePath, "plugins"));

                string gdalData = Path.Combine(gdalPath, "data");
                Environment.SetEnvironmentVariable("GDAL_DATA", gdalData);
                Gdal.SetConfigOption("GDAL_DATA", gdalData);

                string driverPath = Path.Combine(nativePath, "plugins");
                Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", driverPath);
                Gdal.SetConfigOption("GDAL_DRIVER_PATH", driverPath);

                Environment.SetEnvironmentVariable("GEOTIFF_CSV", gdalData);
                Gdal.SetConfigOption("GEOTIFF_CSV", gdalData);

                string projSharePath = Path.Combine(gdalPath, "share");
                Environment.SetEnvironmentVariable("PROJ_LIB", projSharePath);
                Gdal.SetConfigOption("PROJ_LIB", projSharePath);
                OSGeo.OSR.Osr.SetPROJSearchPaths(new[] { projSharePath });

                string certFile = Path.Combine(gdalPath, "curl-ca-bundle.crt");
                Gdal.SetConfigOption("GDAL_CURL_CA_BUNDLE", certFile);

                _usable = true;
            }
            catch (Exception e)
            {
                _usable = false;
                Trace.WriteLine(e, "error");
                Trace.WriteLine($"Executing directory: {executingDirectory}", "error");
                Trace.WriteLine($"gdal directory: {gdalPath}", "error");
                Trace.WriteLine($"native directory: {nativePath}", "error");
            }
        }

        public static bool Usable => _usable;

        public static void ConfigureOgr()
        {
            if (!_usable || _configuredOgr) return;
            Ogr.RegisterAll();
            _configuredOgr = true;
        }

        public static void ConfigureGdal()
        {
            if (!_usable || _configuredGdal) return;
            Gdal.AllRegister();
            _configuredGdal = true;
        }

        private static string GetPlatform() =>
            Environment.Is64BitProcess ? "x64" : "x86";

        private static bool IsWindows =>
            Environment.OSVersion.Platform is not PlatformID.Unix and not PlatformID.MacOSX;
    }

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
