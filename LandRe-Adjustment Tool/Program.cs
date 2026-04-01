

using Land_Readjustment_Tool.Forms;
using Microsoft.Win32;
using System.Text;


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
            Application.EnableVisualStyles();           
            Application.SetCompatibleTextRenderingDefault(false);

            // args[0] is the .lpp file path when opened from Explorer
            // args is empty when launched normally from the Start Menu / taskbar
            string? startupFile = args.Length > 0 ? args[0] : null;

            Application.Run(new frmMain(startupFile));

            //Application.Run(new frmLandownersRecord());
        }
        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //private static extern bool SetProcessDPIAware();



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
    }
}
