using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmBackupManager : Form
    {
        private string _projectFilePath;
        private List<BackupInfo>? _backups= new List<BackupInfo>();
        public string? SelectedBackupPath { get; private set; }

        public frmBackupManager(string projectFilePath)
        {
            InitializeComponent();
            _projectFilePath = projectFilePath;
        }

        private void frmBackupManager_Load(object sender, EventArgs e)
        {
            LoadBackups();
        }

        private void LoadBackups()
        {
            _backups = new List<BackupInfo>();
            lstBackups.Items.Clear();

            // Load all backup files
            for (int i = 1; i <= 5; i++)
            {
                string backupPath = i == 1
                    ? $"{_projectFilePath}.bak"
                    : $"{_projectFilePath}.bak{i}";

                if (File.Exists(backupPath))
                {
                    FileInfo fileInfo = new FileInfo(backupPath);
                    BackupInfo backup = new BackupInfo
                    {
                        Path = backupPath,
                        Number = i,
                        ModifiedDate = fileInfo.LastWriteTime,
                        Size = fileInfo.Length
                    };

                    _backups.Add(backup);

                    // Add to ListView
                    ListViewItem item = new ListViewItem(new[]
                    {
                        $"Backup {i}",
                        backup.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        FormatFileSize(backup.Size)
                    });
                    item.Tag = backup;
                    lstBackups.Items.Add(item);
                }
            }

            if (_backups.Count == 0)
            {
                lblInfo.Text = "No backup files found.";
                btnRestore.Enabled = false;
            }
            else
            {
                lblInfo.Text = $"Found {_backups.Count} backup(s). Select one to restore:";
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void LstBackups_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRestore.Enabled = lstBackups.SelectedItems.Count > 0;
        }

        private void LstBackups_DoubleClick(object sender, EventArgs e)
        {
            if (lstBackups.SelectedItems.Count > 0)
            {
                BtnRestore_Click(sender, e);
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            if (lstBackups.SelectedItems.Count == 0)
                return;

            BackupInfo selectedBackup = (BackupInfo)lstBackups.SelectedItems[0].Tag!;

            var result = MessageBox.Show(
                $"Restore from Backup {selectedBackup!.Number}?\n\n" +
                $"Date: {selectedBackup.ModifiedDate:yyyy-MM-dd HH:mm:ss}\n" +
                $"Size: {FormatFileSize(selectedBackup.Size)}\n\n" +
                "Current unsaved changes will be lost.",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                SelectedBackupPath = selectedBackup.Path;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private class BackupInfo
        {
            public string? Path { get; set; }
            public int Number { get; set; }
            public DateTime ModifiedDate { get; set; }
            public long Size { get; set; }
        }
    }
}