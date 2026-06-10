namespace Land_Pooling_Policy_Manager
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Standalone mode: open the per-user policy library at
            // %AppData%\RePlot\PolicyManager\policies.db. When launched from the
            // RePlot main app, the caller hits PolicyManagerLauncher.ShowDialog
            // directly with the project's .lpp path, so this entry point is
            // only used for the .exe path.
            string dbPath = PolicyManagerLauncher.GetDefaultStandaloneDbPath();
            PolicyManagerLauncher.ShowDialog(owner: null, sqliteDbPath: dbPath, readOnly: false);
        }
    }
}
