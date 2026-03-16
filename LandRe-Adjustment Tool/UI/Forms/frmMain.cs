using Land_Readjustment_Tool.CustomControls;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Forms;
using Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment;
using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;
using System.ComponentModel;

namespace Land_Readjustment_Tool
{
    public partial class frmMain : Form
    {
        private DrawingCanvasControl _drawingCanvas;
        private string AppTitle = "RePlot";
        private DatabaseHelper _dbHelper;
        private AppDbContext? _dbContext;
        private BindingList<BaselineLandParceRecord> _OriginalParcelWithOwnerBindingList;

        private TransformationResult transformResult = new();

        // ==================== CONTEXT MENU SETUP ====================
        private ContextMenuStrip contextMenuGrid;
        public frmMain()
        {
            InitializeComponent();
            UpdateWindowTitle();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeProjectWorkspace();
            FormClosing += frmMain_FormClosing;
            CurrentProject.StateChanged += OnProjectStateChanged;
            // Subscribe to collapse/expand event from DrawingCanvasControl
            drawingCanvasControl1.CollapseLeftPanelClicked += DrawingCanvasControl1_CollapseLeftPanelClicked;
        }

        private bool isLeftPanelCollapsed = false;
        private void DrawingCanvasControl1_CollapseLeftPanelClicked(object sender, EventArgs e)
        {
            if (!isLeftPanelCollapsed)
            {
                splitContainer1.Panel1Collapsed = true;
                // Optionally update the button icon/text here if needed
                isLeftPanelCollapsed = true;
            }
            else
            {
                splitContainer1.Panel1Collapsed = false;
                // Optionally update the button icon/text here if needed
                isLeftPanelCollapsed = false;
            }
        }

        private void OnProjectStateChanged()
        {
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            if (!CurrentProject.IsOpen || CurrentProject.Info == null)
            {
                Text = AppTitle;
                return;
            }

            var baseTitle = $"{CurrentProject.Info.ProjectName} - {AppTitle}";
            Text = CurrentProject.HasUnsavedChanges ? $"{CurrentProject.Info.ProjectName}* - {AppTitle}" : baseTitle;
        }

        private void AreaConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAreaConverter areaConverter = new frmAreaConverter();
            areaConverter.Show();
        }

        private void ProjectInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //DatabaseHelper db = new(CurrentProject.Info.ProjectPath);
            //db.InitializeDatabase();
            //ProjectInfoRepository repo = new(db.GetConnection());
            //CurrentProject.Info = repo.GetProjectInfo();

            //frm_ProjectDetails form = new frm_ProjectDetails();
            //_ = form.ShowDialog();

        }

        //private void LandOwnersRecordToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    frmLandownersRecord landownersRecord = new frmLandownersRecord();

        //    landownersRecord.Show();
        //}

        private void ExitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit the application?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }


            if (!HandleUnsavedChangesOnClose())
            {
                return;
            }

            Close();
        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter =
                "Land Pooling Project File (*.lpp)|*.lpp";
            saveFileDialog.Title = "Create New Project";

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            string filePathFromDialog = saveFileDialog.FileName;
            string projectFileName = Path.GetFileNameWithoutExtension(
                filePathFromDialog);
            string projectFolder = Path.Combine(
                Path.GetDirectoryName(filePathFromDialog)!,
                projectFileName);
            string projectFilePath = Path.Combine(
                projectFolder,
                Path.GetFileName(filePathFromDialog));

            try
            {
                // Create folder structure
                Directory.CreateDirectory(projectFolder);
                ProjectFolderCreator.CreateFolders(projectFolder);

                // Create project using ProjectService
                var service = new ProjectService();
                var projectInfo = await service
                    .CreateNewProjectAsync(
                        projectFilePath,
                        projectFileName);

                // Store path in old CurrentProject
                // keeping both systems during transition
                CurrentProject.Info = new Models.ProjectInfo
                {
                    ProjectName = projectInfo.ProjectName,
                    ProjectPath = projectFilePath
                };
                CurrentProject.MarkAsSaved();

                // Open project details form
                //var frm = new frm_ProjectDetails();
                //frm.ShowDialog();

                UpdateWindowTitle();
                InitializeProjectWorkspace();
            }
            catch (Exception ex)
            {
                // Clean up context if creation failed
                CurrentProjectContext.Close();

                MessageBox.Show(
                    $"Failed to create project: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void InitializeProjectWorkspace()
        {
            splitContainer1.Visible = true;

            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            projectInformationToolStripMenuItem.Enabled = true;
            projectSettingToolStripMenuItem.Enabled = true;
            backupProjectToolStripMenuItem.Enabled = true;
            closeProjectToolStripMenuItem.Enabled = true;
            ImportParcelOwnerShipRecords.Enabled = true;
            landOwnerDataToolStripMenuItem.Enabled = true;
            startReplotWorkspaceToolStripMenuItem.Enabled = true;
        }

        public static class ProjectFolderCreator
        {
            public static void CreateFolders(string root)
            {
                string[] folders =
                {
            "Maps",
            "GIS",
            "Documents",
            "Reports",
            "Exports/Excel",
            "Images/LandOwners Certificate",
            "Images/Cadastral Sheets",
            "Images/Land Owners Photos",
            "Temp",
            "Logs"
        };

                foreach (var folder in folders)
                {
                    _ = Directory.CreateDirectory(Path.Combine(root, folder));
                }
            }
        }


        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new()
            {
                Filter = "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Open Project"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            // Validate the Project File
            if (!ProjectFileValidator.IsValidProjectFile(ofd.FileName))
            {
                _ = MessageBox.Show(
                    "Invalid or corrupted project file.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Set Current Project
                DatabaseHelper db = new(ofd.FileName);
                db.InitializeDatabase();

                ProjectInfoRepository repo = new(db.GetConnection());
                CurrentProject.Info = repo.GetProjectInfo();

                if (CurrentProject.Info != null)
                {
                    CurrentProject.Info.ProjectPath = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(
                    $"Failed to open project: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                CurrentProject.Info = null; // Reset project info on failure


            }
            UpdateWindowTitle();
            this.InitializeProjectWorkspace();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if project is open
            if (!CurrentProject.IsOpen)
            {
                _ = MessageBox.Show("No project is currently open.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Save the project
                SaveCurrentProjectWithBackup();

            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Failed to save project: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //private void SaveCurrentProjectWithBackup()
        //{
        //    if (!CurrentProject.IsOpen) return;

        //    // Open database connection
        //    DatabaseHelper db = new(CurrentProject.Info.ProjectPath);
        //    db.InitializeDatabase();

        //    // Save project info
        //    ProjectInfoRepository repo = new(db.GetConnection());
        //    repo.SaveProjectInfo(CurrentProject.Info);

        //    // TODO: Save other data tables here (land owners, etc.)
        //    // Example:
        //    // LandOwnersRepository landRepo = new(db.GetConnection());
        //    // landRepo.SaveAllLandOwners();
        //}

        private void SaveCurrentProjectWithBackup()
        {
            if (!CurrentProject.IsOpen) return;

            string projectFilePath = CurrentProject.Info!.ProjectPath;

            try
            {
                // Rotate backups before saving
                if (File.Exists(projectFilePath))
                {
                    RotateBackupFiles(projectFilePath, maxBackups: 5);
                }

                // Save new data
                DatabaseHelper db = new(projectFilePath);
                db.InitializeDatabase();
                ProjectInfoRepository repo = new(db.GetConnection());
                repo.SaveProjectInfo(CurrentProject.Info);

                // TODO: Save other tables

                CurrentProject.MarkAsSaved();
            }
            catch (Exception ex)
            {
                // Try to restore from most recent backup
                string backupFilePath = projectFilePath + ".bak";
                if (File.Exists(backupFilePath))
                {
                    File.Copy(backupFilePath, projectFilePath, overwrite: true);
                    _ = MessageBox.Show(
                        "Save failed! Project restored from most recent backup.\n\n" +
                        $"Error: {ex.Message}",
                        "Save Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                throw;
            }
        }

        private void RotateBackupFiles(string projectFilePath, int maxBackups)
        {
            // Delete oldest backup (backup 5)
            string oldestBackup = $"{projectFilePath}.bak{maxBackups}";
            if (File.Exists(oldestBackup))
            {
                File.Delete(oldestBackup);
            }

            // Shift existing backups: .bak4 → .bak5, .bak3 → .bak4, etc.
            for (int i = maxBackups - 1; i >= 1; i--)
            {
                string currentBackup = i == 1
                    ? $"{projectFilePath}.bak"
                    : $"{projectFilePath}.bak{i}";

                string nextBackup = $"{projectFilePath}.bak{i + 1}";

                if (File.Exists(currentBackup))
                {
                    File.Move(currentBackup, nextBackup, overwrite: true);
                }
            }

            // Create new backup from current file
            string latestBackup = $"{projectFilePath}.bak";
            File.Copy(projectFilePath, latestBackup, overwrite: true);
        }

        private void RestoreFromBackup(string projectFilePath)
        {
            // Try to restore from most recent backup
            string backupFilePath = $"{projectFilePath}.bak";

            if (File.Exists(backupFilePath))
            {
                File.Copy(backupFilePath, projectFilePath, overwrite: true);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CurrentProject.IsOpen)
            {
                _ = MessageBox.Show("No project is currently open.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Land Pooling Project File (*.lpp)|*.lpp",
                    Title = "Save Project As",
                    FileName = CurrentProject.Info!.ProjectName + ".lpp"
                };

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                Cursor = Cursors.WaitCursor;

                string newFilePathFromDialog = saveFileDialog.FileName;
                string newProjectFileName = Path.GetFileNameWithoutExtension(newFilePathFromDialog);
                string newProjectFolder = Path.Combine(
                    Path.GetDirectoryName(newFilePathFromDialog)!,
                    newProjectFileName);
                string newProjectFilePath = Path.Combine(newProjectFolder, Path.GetFileName(newFilePathFromDialog));

                // Get old project folder
                string oldProjectFolder = Path.GetDirectoryName(CurrentProject.Info.ProjectPath)!;

                // Copy project folder (excluding backup files)
                CopyProjectFolderWithoutBackups(oldProjectFolder, newProjectFolder);

                // Copy/overwrite the database file
                File.Copy(CurrentProject.Info.ProjectPath, newProjectFilePath, overwrite: true);

                // Update project info with new paths
                CurrentProject.Info.ProjectName = newProjectFileName;
                CurrentProject.Info.ProjectPath = newProjectFilePath;

                // Save updated info to the new database
                DatabaseHelper db = new(newProjectFilePath);
                db.InitializeDatabase();
                ProjectInfoRepository repo = new(db.GetConnection());
                repo.SaveProjectInfo(CurrentProject.Info);

                // Update UI
                this.Text = newProjectFileName + " - Land Readjustment Tool";
                CurrentProject.MarkAsSaved();

                Cursor = Cursors.Default;

                _ = MessageBox.Show(
                    $"Project saved successfully to:\n{newProjectFolder}\n\n" +
                    "Note: Backup files were not copied. New backups will be created when you save.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                _ = MessageBox.Show($"Failed to save project: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CopyProjectFolderWithoutBackups(string sourceFolder, string destFolder)
        {
            // Create destination folder
            _ = Directory.CreateDirectory(destFolder);

            // Copy all files EXCEPT .bak files
            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                // Skip backup files
                string fileName = Path.GetFileName(file);
                if (fileName.EndsWith(".bak") || fileName.Contains(".bak"))
                    continue;

                string destFile = Path.Combine(destFolder, fileName);
                File.Copy(file, destFile, overwrite: true);
            }

            // Copy all subdirectories recursively (also excluding .bak files)
            foreach (string subDir in Directory.GetDirectories(sourceFolder))
            {
                string destSubDir = Path.Combine(destFolder, Path.GetFileName(subDir));
                CopyProjectFolderWithoutBackups(subDir, destSubDir);
            }
        }

        private void restoreFromBackupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CurrentProject.IsOpen)
            {
                _ = MessageBox.Show("No project is currently open.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Show backup manager dialog
                using (frmBackupManager? backupManager = new frmBackupManager(CurrentProject.Info!.ProjectPath))
                {
                    if (backupManager.ShowDialog() == DialogResult.OK)
                    {
                        string backupPath = backupManager.SelectedBackupPath!;
                        string projectPath = CurrentProject.Info.ProjectPath;

                        // Close any open connections
                        // (Important: Make sure database connections are closed)

                        // Restore the backup
                        File.Copy(backupPath, projectPath, overwrite: true);

                        // Reload the project
                        DatabaseHelper db = new(projectPath);
                        db.InitializeDatabase();
                        ProjectInfoRepository repo = new(db.GetConnection());
                        CurrentProject.Info = repo.GetProjectInfo();

                        _ = MessageBox.Show("Project restored successfully from backup.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh UI if needed
                        this.Text = CurrentProject.Info!.ProjectName + " - Land Readjustment Tool";
                    }
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Failed to restore from backup: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            _drawingCanvas = new DrawingCanvasControl();
            _drawingCanvas.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(_drawingCanvas);
            if (CurrentProject.IsOpen)
            {
                UpdateWindowTitle();
                InitializeProjectWorkspace();
            }
        }



        private void ImportParcelOwnershipRecords_Click(object sender, EventArgs e)
        {
            if (!CurrentProject.IsOpen || CurrentProject.Info == null)
            {
                _ = MessageBox.Show("Please open or create a project first.", "No Project",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentProject.Info.ProjectPath))
            {
                _ = MessageBox.Show("Project path is invalid. Please save the project first.", "Invalid Project",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var importForm = new frmImportParcelOwnershipRecords(CurrentProject.Info.ProjectPath))
            {
                if (importForm.ShowDialog() == DialogResult.OK)
                {
                    _ = MessageBox.Show($"Successfully imported {importForm.ImportedCount} records!",
                        "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void viewEditRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new frmLandParcelOwnersRecord();
            form.Show();
            //var filterForm = new frmLandownerRecordsFilterView(CurrentProject.Info.ProjectPath);
            //filterForm.Show();

            //var Form = new frmLandownerRecordsManager(CurrentProject.Info.ProjectPath);
            //Form.Show();
        }

        private void landOwnerDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CurrentProject.IsOpen || CurrentProject.Info == null)
            {
                MessageBox.Show("Please open or create a project first.", "No Project",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form = new frmLandOwnersRecord();
            form.Show();
        }

        private void toolStripSeparator1_Click(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ViewDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void closeProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CurrentProject.IsOpen)
            {
                return;
            }

            if (!HandleUnsavedChangesOnClose())
            {
                return;
            }

            CurrentProject.Close();
            CloseProjectWorkspace();
        }

        private void frmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!HandleUnsavedChangesOnClose())
            {
                e.Cancel = true;
            }
        }

        private bool HandleUnsavedChangesOnClose()
        {
            if (!CurrentProject.IsOpen || !CurrentProject.HasUnsavedChanges)
            {
                return true;
            }

            var result = MessageBox.Show(
                "Do you want to save the project before leaving?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SaveCurrentProjectWithBackup();
                return true;
            }

            if (result == DialogResult.No)
            {
                RestoreFromBackup(CurrentProject.Info!.ProjectPath);
                return true;
            }

            return false;
        }

        private void CloseProjectWorkspace()
        {
            splitContainer1.Visible = false;

            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            projectInformationToolStripMenuItem.Enabled = false;
            projectSettingToolStripMenuItem.Enabled = false;
            closeProjectToolStripMenuItem.Enabled = false;
            backupProjectToolStripMenuItem.Enabled = false;
            restoreFromBackupToolStripMenuItem.Enabled = false;
            ImportParcelOwnerShipRecords.Enabled = false;
            startReplotWorkspaceToolStripMenuItem.Enabled = false;

            UpdateWindowTitle();
        }



        private void projectSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void baseMapsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }




    }
}

