

using Land_Readjustment_Tool.Forms;
using System.Text;


namespace Land_Readjustment_Tool
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //if (Environment.OSVersion.Version.Major >= 6)
            //    SetProcessDPIAware();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Application.EnableVisualStyles();           
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());

            //Application.Run(new frmLandownersRecord());
        }
        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //private static extern bool SetProcessDPIAware();

    }
}
