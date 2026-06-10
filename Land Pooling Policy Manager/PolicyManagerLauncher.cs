using Land_Pooling_Policy_Manager.Data;
using Land_Pooling_Policy_Manager.Services;
using Land_Pooling_Policy_Manager.UI.Forms;

namespace Land_Pooling_Policy_Manager
{
    /// <summary>
    /// Public entry the main RePlot solution calls to open the policy manager
    /// against a project's .lpp SQLite file. This is the only surface the main
    /// project needs to take a project reference on.
    /// </summary>
    public static class PolicyManagerLauncher
    {
        /// <summary>
        /// Show the policy manager modally over the supplied owner, reading and
        /// writing the policy tables in <paramref name="sqliteDbPath"/>.
        /// </summary>
        /// <param name="roadReferenceOptionsProvider">Optional callback the main
        /// host uses to feed the project's actual roads into the corner-type
        /// editor. If null, the standalone fallback list is used.</param>
        public static void ShowDialog(
            Form? owner,
            string sqliteDbPath,
            bool readOnly,
            Func<CancellationToken, Task<List<string>>>? roadReferenceOptionsProvider = null,
            bool valueOnlyEditMode = false)
        {
            if (string.IsNullOrWhiteSpace(sqliteDbPath))
                throw new ArgumentException("Database path is required.", nameof(sqliteDbPath));

            // Schema bootstrap — safe to call against an existing .lpp or a fresh DB.
            using (PolicyDbContext bootstrap = new(sqliteDbPath))
            {
                bootstrap.EnsureSchemaAsync().GetAwaiter().GetResult();
            }

            // The MDI host owns a DbContext for the lifetime of the window.
            PolicyDbContext context = new(sqliteDbPath);
            PolicyValidationService validation = new();
            PolicyPackageService package = new();
            PolicyTemplateSeeder seeder = new(context);
            PolicyManagerService service = new(context, validation, package, seeder)
            {
                RoadReferenceOptionsProvider = roadReferenceOptionsProvider
            };

            using frmPolicyManagerMdiHost shell = new(service, readOnly, valueOnlyEditMode);
            shell.FormClosed += (_, _) => context.Dispose();
            if (owner == null)
                shell.ShowDialog();
            else
                shell.ShowDialog(owner);
        }

        /// <summary>
        /// The default DB path used when the manager is run standalone — single
        /// per-user policy library under %AppData%\RePlot\PolicyManager.
        /// </summary>
        public static string GetDefaultStandaloneDbPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "RePlot", "PolicyManager");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "policies.db");
        }
    }
}
