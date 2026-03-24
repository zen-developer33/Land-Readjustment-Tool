using Land_Readjustment_Tool.Services.Project;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Shows all backup files for the current project.
    /// User selects one and clicks Restore.
    /// Caller gets SelectedBackupPath and restores.
    /// </summary>
    public partial class frmBackupManager : Form
    {
        private readonly string _projectFilePath;
        private readonly List<BackupEntry> _backups;

        /// <summary>
        /// Set when user confirms restore.
        /// Caller reads this after DialogResult.OK.
        /// </summary>
        public string? SelectedBackupPath { get; private set; }

        /// <summary>
        /// Receives project file path and pre-loaded
        /// backup list from ProjectBackupService.
        /// </summary>
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
            PopulateList();
        }

        private void PopulateList()
        {
            lstBackups.Items.Clear();

            if (_backups.Count == 0)
            {
                lblInfo.Text = "No backup files found.";
                btnRestore.Enabled = false;
                return;
            }

            lblInfo.Text = $"Found {_backups.Count} backup(s). " +
                           "Select one to restore:";

            foreach (var backup in _backups)
            {
                var item = new ListViewItem(new[]
                {
                    backup.Label,
                    backup.ModifiedDate.ToString(
                        "yyyy-MM-dd HH:mm:ss"),
                    backup.FormattedSize
                });

                item.Tag = backup;
                lstBackups.Items.Add(item);
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

            var backup = (BackupEntry)
                lstBackups.SelectedItems[0].Tag!;

            var result = MessageBox.Show(
                $"Restore from {backup.Label}?\n\n" +
                $"Date : {backup.ModifiedDate:yyyy-MM-dd HH:mm:ss}\n" +
                $"Size : {backup.FormattedSize}\n\n" +
                "Current unsaved changes will be lost.",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            SelectedBackupPath = backup.FilePath;
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