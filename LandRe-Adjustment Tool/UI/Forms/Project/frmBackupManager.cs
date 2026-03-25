using Land_Readjustment_Tool.Services.Project;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Displays up to 5 previous saved states
    /// and allows the user to restore one.
    ///
    /// PURPOSE:
    ///   → Go back to any of the last 5 Ctrl+S saves
    ///   → Recover from corrupted .lpp file
    ///
    /// NOT FOR:
    ///   → Discarding unsaved changes
    ///     (handled by EF Core ChangeTracker.Clear)
    ///
    /// USAGE FROM frmMain:
    ///   var backups = _backupService.GetBackups(filePath);
    ///   using var frm = new frmBackupManager(
    ///       filePath, backups);
    ///   if (frm.ShowDialog() == DialogResult.OK)
    ///       // restore frm.SelectedBackupPath
    /// </summary>
    public partial class frmBackupManager : Form
    {
        private readonly string _projectFilePath;
        private readonly List<BackupEntry> _backups;

        /// <summary>
        /// Full path of backup file user selected.
        /// Read after ShowDialog() returns OK.
        /// </summary>
        public string? SelectedBackupPath
        { get; private set; }

        public frmBackupManager(
            string projectFilePath,
            List<BackupEntry> backups)
        {
            InitializeComponent();
            _projectFilePath = projectFilePath;
            _backups = backups;
        }

        // ── LOAD ─────────────────────────────────────

        private void frmBackupManager_Load(
            object sender, EventArgs e)
        {
            Text = "Restore Backup — " +
                   Path.GetFileName(_projectFilePath);
            PopulateList();
        }

        private void PopulateList()
        {
            lstBackups.Items.Clear();

            if (_backups.Count == 0)
            {
                lblInfo.Text =
                    "No backups found. " +
                    "Backups are created each time " +
                    "you save the project (Ctrl+S).";
                btnRestore.Enabled = false;
                return;
            }

            lblInfo.Text =
                $"{_backups.Count} backup(s) found. " +
                "Select one to restore:";

            foreach (var b in _backups)
            {
                var item = new ListViewItem(new[]
                {
                    b.Label,
                    b.ModifiedDate.ToString(
                        "yyyy-MM-dd HH:mm:ss"),
                    b.FormattedSize
                });
                item.Tag = b;
                lstBackups.Items.Add(item);
            }

            // Auto-select most recent
            if (lstBackups.Items.Count > 0)
            {
                lstBackups.Items[0].Selected = true;
                lstBackups.Items[0].Focused = true;
            }
        }

        // ── EVENTS ───────────────────────────────────

        private void LstBackups_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            btnRestore.Enabled =
                lstBackups.SelectedItems.Count > 0;
        }

        private void LstBackups_DoubleClick(
            object sender, EventArgs e)
        {
            if (lstBackups.SelectedItems.Count > 0)
                BtnRestore_Click(sender, e);
        }

        private void BtnRestore_Click(
            object sender, EventArgs e)
        {
            if (lstBackups.SelectedItems.Count == 0)
                return;

            var b = (BackupEntry)
                lstBackups.SelectedItems[0].Tag!;

            var confirm = MessageBox.Show(
                $"Restore project to this saved state?\n\n" +
                $"  {b.Label}\n" +
                $"  Saved : " +
                $"{b.ModifiedDate:yyyy-MM-dd HH:mm:ss}\n" +
                $"  Size  : {b.FormattedSize}\n\n" +
                "The current project will be replaced.\n" +
                "Any unsaved changes will be lost.\n\n" +
                "Continue?",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                // Default = No prevents accidental restore
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            SelectedBackupPath = b.FilePath;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(
            object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}