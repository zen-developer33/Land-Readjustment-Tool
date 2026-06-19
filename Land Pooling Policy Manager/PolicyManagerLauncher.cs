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
        private static readonly Dictionary<string, frmPolicyManagerMdiHost> OpenShells = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Show the policy manager as an independent modeless top-level window.
        /// This is the preferred entry point when RePlot launches the manager
        /// from an open project, because minimizing or maximizing the manager
        /// must not change the main RePlot window state.
        /// </summary>
        /// <param name="roadReferenceOptionsProvider">Optional callback the main
        /// host uses to feed the project's actual roads into policy lookup
        /// editors. If null, the standalone fallback list is used.</param>
        public static void Show(
            Form? owner,
            string sqliteDbPath,
            bool readOnly,
            Func<CancellationToken, Task<List<string>>>? roadReferenceOptionsProvider = null,
            bool valueOnlyEditMode = false)
        {
            if (string.IsNullOrWhiteSpace(sqliteDbPath))
                throw new ArgumentException("Database path is required.", nameof(sqliteDbPath));

            string normalizedPath = NormalizeDbPath(sqliteDbPath);
            if (OpenShells.TryGetValue(normalizedPath, out frmPolicyManagerMdiHost? existing) &&
                !existing.IsDisposed)
            {
                if (existing.ValueOnlyEditMode != valueOnlyEditMode)
                {
                    existing.Close();
                }
                else
                {
                    existing.SetReadOnlyMode(readOnly);
                    if (existing.WindowState == FormWindowState.Minimized)
                        existing.WindowState = FormWindowState.Normal;

                    existing.Show();
                    existing.Activate();
                    return;
                }
            }

            using (PolicyDbContext bootstrap = new(normalizedPath))
            {
                bootstrap.EnsureSchemaAsync().GetAwaiter().GetResult();
            }

            PolicyDbContext context = new(normalizedPath);
            PolicyValidationService validation = new();
            PolicyPackageService package = new();
            PolicyTemplateSeeder seeder = new(context);
            PolicyManagerService service = new(context, validation, package, seeder)
            {
                RoadReferenceOptionsProvider = roadReferenceOptionsProvider
            };

            frmPolicyManagerMdiHost shell = new(service, readOnly, valueOnlyEditMode)
            {
                Owner = null,
                ShowInTaskbar = true,
                StartPosition = FormStartPosition.CenterScreen
            };

            OpenShells[normalizedPath] = shell;
            shell.FormClosed += (_, _) =>
            {
                if (OpenShells.TryGetValue(normalizedPath, out frmPolicyManagerMdiHost? current) &&
                    ReferenceEquals(current, shell))
                {
                    OpenShells.Remove(normalizedPath);
                }

                context.Dispose();
            };

            shell.Show();
            shell.Activate();
        }

        public static void SetReadOnlyMode(string sqliteDbPath, bool readOnly)
        {
            if (string.IsNullOrWhiteSpace(sqliteDbPath))
                return;

            string normalizedPath = NormalizeDbPath(sqliteDbPath);
            if (!OpenShells.TryGetValue(normalizedPath, out frmPolicyManagerMdiHost? shell) ||
                shell.IsDisposed)
                return;

            shell.SetReadOnlyMode(readOnly);
        }

        public static void Close(string sqliteDbPath)
        {
            if (string.IsNullOrWhiteSpace(sqliteDbPath))
                return;

            string normalizedPath = NormalizeDbPath(sqliteDbPath);
            if (!OpenShells.TryGetValue(normalizedPath, out frmPolicyManagerMdiHost? shell) ||
                shell.IsDisposed)
                return;

            shell.Close();
        }

        /// <summary>
        /// Show the policy manager modally. This is retained for standalone
        /// execution and any caller that explicitly needs modal behavior.
        /// </summary>
        public static void ShowDialog(
            Form? owner,
            string sqliteDbPath,
            bool readOnly,
            Func<CancellationToken, Task<List<string>>>? roadReferenceOptionsProvider = null,
            bool valueOnlyEditMode = false)
        {
            if (string.IsNullOrWhiteSpace(sqliteDbPath))
                throw new ArgumentException("Database path is required.", nameof(sqliteDbPath));

            string normalizedPath = NormalizeDbPath(sqliteDbPath);
            using (PolicyDbContext bootstrap = new(normalizedPath))
            {
                bootstrap.EnsureSchemaAsync().GetAwaiter().GetResult();
            }

            PolicyDbContext context = new(normalizedPath);
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
        /// The default DB path used when the manager is run standalone: single
        /// per-user policy library under %AppData%\RePlot\PolicyManager.
        /// </summary>
        public static string GetDefaultStandaloneDbPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "RePlot", "PolicyManager");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "policies.db");
        }

        private static string NormalizeDbPath(string sqliteDbPath)
        {
            return Path.GetFullPath(sqliteDbPath);
        }
    }
}
