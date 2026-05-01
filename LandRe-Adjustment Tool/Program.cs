

using Land_Readjustment_Tool.Forms;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Canvas;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;
using Land_Readjustment_Tool.Core.Interfaces;


namespace Land_Readjustment_Tool
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //if (Environment.OSVersion.Version.Major >= 6)
                //SetProcessDPIAware();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ConfigureNativeLibrarySearchPath();
            SQLitePCL.Batteries_V2.Init();
            Application.EnableVisualStyles();           
            Application.SetCompatibleTextRenderingDefault(false);
            ShowSplashScreen();

            // args[0] is the .lpp file path when opened from Explorer
            // args is empty when launched normally from the Start Menu / taskbar
            string? startupFile = args.Length > 0 ? args[0] : null;

            var services = new ServiceCollection();
            ConfigureServices(services);

            using var serviceProvider = services.BuildServiceProvider();
            var mainForm = string.IsNullOrWhiteSpace(startupFile)
                ? serviceProvider.GetRequiredService<frmMain>()
                : ActivatorUtilities.CreateInstance<frmMain>(serviceProvider, startupFile);

            Application.Run(mainForm);

            //Application.Run(new frmLandownersRecord());
        }

        private static void ShowSplashScreen()
        {
            using var splashForm = new frmSplash();
            Application.Run(splashForm);
        }

        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //private static extern bool SetProcessDPIAware();

        private static void ConfigureNativeLibrarySearchPath()
        {
            string baseDirectory = AppContext.BaseDirectory;
            string runtimeFolder = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "win-x86",
                Architecture.Arm => "win-arm",
                Architecture.Arm64 => "win-arm64",
                _ => "win-x64"
            };

            string runtimeNativePath = Path.Combine(
                baseDirectory,
                "runtimes",
                runtimeFolder,
                "native");

            PrependPathIfExists(runtimeNativePath);

            // GDAL sets restricted DLL search flags later. Registering these paths
            // up front keeps SQLite/SpatiaLite native modules resolvable even after
            // the default DLL search order is tightened.
            TryEnableSafeDllSearchWithUserDirectories();
            AddNativeDllDirectoryIfExists(runtimeNativePath);
            AddNativeDllDirectoryIfExists(baseDirectory);
        }

        private static void PrependPathIfExists(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] entries = path.Split(
                Path.PathSeparator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (entries.Any(entry => string.Equals(
                    entry,
                    directory,
                    StringComparison.OrdinalIgnoreCase)))
                return;

            Environment.SetEnvironmentVariable(
                "PATH",
                directory + Path.PathSeparator + path);
        }

        private static void TryEnableSafeDllSearchWithUserDirectories()
        {
            const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
            try
            {
                SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
            }
            catch
            {
                // Best effort only.
            }
        }

        private static void AddNativeDllDirectoryIfExists(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            try
            {
                AddDllDirectory(directory);
            }
            catch
            {
                // Best effort only.
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDefaultDllDirectories(uint directoryFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr AddDllDirectory(string lpPathName);



        // Call this once from Program.cs or frmMain_Load
        public static void RegisterFileAssociation()
        {
            try
            {
                string exePath = Application.ExecutablePath;

                // Register .lpp extension
                // HKEY_CURRENT_USER does not need admin rights
                using var ext = Registry.CurrentUser
                    .CreateSubKey(@"Software\Classes\.lpp");
                ext.SetValue("", "RePlot.ProjectFile");

                // Register the file type
                using var fileType = Registry.CurrentUser
                    .CreateSubKey(@"Software\Classes\RePlot.ProjectFile");
                fileType.SetValue("", "RePlot Land Pooling Project");

                // Register the open command
                using var openCmd = Registry.CurrentUser.CreateSubKey(
                    @"Software\Classes\RePlot.ProjectFile\shell\open\command");
                openCmd.SetValue("", $"\"{exePath}\" \"%1\"");

                // Register icon (optional — uses the exe icon)
                using var icon = Registry.CurrentUser.CreateSubKey(
                    @"Software\Classes\RePlot.ProjectFile\DefaultIcon");
                icon.SetValue("", $"\"{exePath}\",0");

                // Tell Windows Explorer to refresh icons
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }
            catch
            {
                // Silently ignore — not critical if this fails
            }
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern void SHChangeNotify(
            int wEventId, int uFlags,
            IntPtr dwItem1, IntPtr dwItem2);

        private static void ConfigureServices(IServiceCollection services)
        {
            // Core application services
            services.AddSingleton<ProjectBackupService>();
            services.AddSingleton<ProjectSessionFactory>();
            services.AddSingleton<IProjectScopedFactory, ProjectScopedFactory>();
            services.AddSingleton<CanvasLayerCommandService>();
            services.AddSingleton<CanvasLayerBoundsService>();
            services.AddSingleton<IProjectRasterCrsResolver, ProjectRasterCrsResolver>();
            services.AddSingleton<IRasterDatasetImporter, GdalRasterDatasetImporter>();
            services.AddSingleton<IRasterLayerImportService, RasterLayerImportService>();
            services.AddSingleton<RasterImportFileManagementService>();
            services.AddSingleton<IXyzTileSourceService, XyzTileSourceService>();
            services.AddSingleton<RasterLayerProjectionService>();
            services.AddSingleton<ProjectOpenService>();
            services.AddSingleton<ProjectSaveAsService>();
            services.AddTransient<ProjectService>();

            // Forms
            services.AddTransient<frmMain>();
        }
    }
}
