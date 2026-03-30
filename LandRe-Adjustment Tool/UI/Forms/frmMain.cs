
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Forms;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Repositories.Spatial;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.Forms.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics.Metrics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Windows.Forms.AxHost;

namespace Land_Readjustment_Tool
{

    public partial class frmMain : Form
    {
        //--- INITIALIZE SERVICES -------
        #region
        private readonly ProjectBackupService _backupService = new();
        #endregion

        //App Title shown in window Title bar
        private readonly string _appTitle = "Replot";

        //Canvas Control for drawing
        private DrawingCanvasControl _drawingCanvas;
        public frmMain()
        {
            InitializeComponent();

            UpdateWindowTitle();
            DisableProjectMenuItems();
            this.FormClosing += frmMain_FormClosing;
            tsmSave.Click += tsmSave_Click;
            tsmSaveAs.Click += tsmSaveAs_Click;
        }


        private void InitializeProjectWorkspace()
        {
            mainSplitContainer.Visible = true;
            EnableProjectMenuItems();
            // Initialize the drawing canvas
        }

        private void UnloadProjectWorkspace()
        {
            mainSplitContainer.Visible = false;
            DisableProjectMenuItems();
            // Clean up the drawing canvas
        }

        private void UpdateWindowTitle()
        {
            if (!AppServices.HasContext || AppServices.Context.Info == null)
            {
                this.Text = _appTitle;
                return;
            }

            var name = AppServices.Context.Info.ProjectName;
            var hasChanges = AppServices.Context.HasUnsavedChanges;

            this.Text = hasChanges ? $"{name}* - {_appTitle}" : $"{name} - {_appTitle}";
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Initialize the drawing canvas
        }

        // ── FORM CLOSING ─────────────────────────────

        // Handles form closing event
        private void frmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!HandleUnsavedChangesOnClose())
                e.Cancel = true;
        }

        // Returns true if safe to close
        // Returns false if user clicked Cancel
        private bool HandleUnsavedChangesOnClose()
        {
            if (!AppServices.HasContext ||
                !AppServices.Context.HasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
                "You have unsaved changes.\n\n" +
                "Save before closing?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                return SaveCurrentProject();

            if (result == DialogResult.No)
            {
                // Discard — restore the .lpp file from the last .bak.
                // Must dispose the session BEFORE touching the file,
                // otherwise SQLite will reject the file copy.
                string filePath = AppServices.Context.ProjectFilePath;

                AppServices.Context.Session.Dispose();
                AppServices.ClearContext();

                // Replace .lpp with the last backup.
                // Returns false only if no .bak exists (should never happen
                // after the initial setup backup is created).
                if (!_backupService.RollbackToLatest(filePath))
                    MessageBox.Show(
                        "Could not restore the project to its last saved state — " +
                        "no backup file was found.",
                        "Restore Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                return true;
            }

            // Cancel — do not close
            return false;
        }
        

        
        private void EnableProjectMenuItems()
        {
            tsmSave.Enabled = true;
            tsmSaveAs.Enabled = true;
            tsmProjectInformation.Enabled = true;
            tsmProjectSetting.Enabled = true;
            tsmCloseProject.Enabled = true;
            tsmBackupProject.Enabled = true;
            ImportParcelOwnerShipRecords.Enabled = true;
            landOwnerDataToolStripMenuItem.Enabled = true;
            startReplotWorkspaceToolStripMenuItem.Enabled = true;
        }

        // Disables menu items when no project is open
        private void DisableProjectMenuItems()
        {
            tsmSave.Enabled = false;
            tsmSaveAs.Enabled = false;
            tsmProjectInformation.Enabled = false;
            tsmProjectSetting.Enabled = false;
            tsmCloseProject.Enabled = false;
            tsmBackupProject.Enabled = false;
            ImportParcelOwnerShipRecords.Enabled = false;
            landOwnerDataToolStripMenuItem.Enabled = false;
            startReplotWorkspaceToolStripMenuItem.Enabled = false;
        }

        // ── NEW PROJECT ──────────────────────────────

        private async void tsmNewProject_Click(object sender, EventArgs e)
        {
            if (AppServices.HasContext && AppServices.Context.HasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Do you want to save and current project before creating new one?",
                    "Save Current Project",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    await SaveCurrentProjectAsync(showMessage: false);
                }
            }

            using SaveFileDialog sfd = new()
            {
                Filter =
                    "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Create New Project"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            await CloseCurrentProjectAsync();
            string filePathFromDialog = sfd.FileName;
            string projectFileName = Path.GetFileNameWithoutExtension(filePathFromDialog);
            string projectFolder = Path.Combine(Path.GetDirectoryName(filePathFromDialog)!, projectFileName);
            string projectFilePath = Path.Combine(projectFolder, Path.GetFileName(filePathFromDialog));

            try
            {
                // Create folder structure
                Directory.CreateDirectory(projectFolder);
                ProjectFolderCreator.CreateFolders(projectFolder);

                // Create session and context
                var session = new ProjectSessionFactory().CreateSession(projectFilePath);
                var context = new ProjectContext(session, projectFilePath);

                // Register in AppServices
                AppServices.SetContext(context);

                // Subscribe to state changes
                context.StateChanged += UpdateWindowTitle;

                // Create project in database
                var projectService = new ProjectService();
                var info = await projectService.CreateNewProjectAsync(projectFilePath, projectFileName);

                // Set info on context
                context.SetInfo(info);

                // Enable menu items
                EnableProjectMenuItems();
                UpdateWindowTitle();

                // Open project details form
                OpenProjectDetails();
                PromptProjectSettings();
                InitializeProjectWorkspace();
                ApplyCanvasSettings(); // ← add here
            }
            catch (Exception ex)
            {
                AppServices.ClearContext();
                MessageBox.Show(
                    $"Failed to create project: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool SaveCurrentProject()
        {
            if (!AppServices.HasContext) return true;

            try
            {
                string filePath = AppServices.Context.ProjectFilePath;
                var dbContext = AppServices.Context.Session.GetContext();

                // Repositories write immediately — no pending changes to flush.
                // Save = WAL checkpoint + backup rotation.

                // 1. WAL checkpoint — merge WAL journal into the .lpp file
                dbContext.Database
                    .ExecuteSqlRaw("PRAGMA wal_checkpoint(FULL);");

                // 2. Rotate backups
                _backupService.CreateBackup(filePath);

                // 3. Mark clean
                AppServices.Context.MarkAsSaved();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save failed: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
        private async void PromptProjectSettings()
        {
            var result = MessageBox.Show(
                "Would you like to configure " +
                "project settings now?\n\n" +
                "You can always change settings later " +
                "from Project → Project Settings.",
                "Project Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                OpenProjectSettings();
            }

            // Mark as configured regardless of choice —
            // prevents this prompt from showing again on next open.
            var repo = new ProjectSettingsRepository(
                AppServices.Context.Session);
            await repo.MarkAsConfiguredAsync();

            // Create the very first project backup.
            // This happens AFTER project info + settings are saved,
            // so the .bak captures the correct initial project state.
            // All future discards will restore to this checkpoint.
            if (AppServices.HasContext)
            {
                var filePath = AppServices.Context.ProjectFilePath;
                var dbContext = AppServices.Context.Session.GetContext();

                // WAL checkpoint — flush all writes into the .lpp file
                await dbContext.Database
                    .ExecuteSqlRawAsync("PRAGMA wal_checkpoint(FULL);");

                // Create .lpp.bak — this IS the initial saved state
                _backupService.CreateBackup(filePath);

                // Mark as saved — no asterisk on a fresh project
                AppServices.Context.MarkAsSaved();
            }
        }

        private void OpenProjectSettings()
        {
            if (!AppServices.HasContext) return;

            var context = AppServices.Context;
            var repo = new ProjectSettingsRepository(
                context.Session);
            var crsRepo = new CoordinateSystemRepository(
                context.Session);
            var datumRepo = new DatumTransformationRepository(
                context.Session);
            var service = new ProjectSettingsService(
                repo, context.Session.Logger);

            using var frm = new frmProjectSettings(
                service, crsRepo, datumRepo);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                AppServices.Context.MarkAsModified();
                // Apply canvas settings immediately
                ApplyCanvasSettings();
            }
        }

        private async void ApplyCanvasSettings()
        {
            if (!AppServices.HasContext) return;
            if (_drawingCanvas == null) return;

            try
            {
                var repo = new ProjectSettingsRepository(
                    AppServices.Context.Session);
                var settings = await repo
                    .GetProjectSettingsAsync();

                if (settings == null) return;

                var bgColor = ColorTranslator.FromHtml(
                    settings.CanvasBackgroundColor);

                _drawingCanvas.ApplyBackgroundColor(bgColor);
                _drawingCanvas.ApplyGridVisible(
                    settings.CanvasGridVisible);
                _drawingCanvas.ApplySnapEnabled(
                    settings.SnapEnabled);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ApplyCanvasSettings failed: {ex.Message}");
            }
        }
        // ── CLOSE PROJECT ────────────────────────────

        private async Task CloseCurrentProjectAsync()
        {
            if (!AppServices.HasContext) return;

            // HandleUnsavedChangesOnClose may already
            // dispose session if user clicked No
            if (!HandleUnsavedChangesOnClose()) return;

            if (AppServices.HasContext)
            {
                AppServices.Context.StateChanged
                    -= UpdateWindowTitle;
                AppServices.ClearContext();
            }

            UnloadProjectWorkspace();
            DisableProjectMenuItems();
            UpdateWindowTitle();
        }



        // ── PROJECT DETAILS ──────────────────────────

        private void tsmProjectInformation_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;
            OpenProjectDetails();
        }

        // Opens project details form
        // Wires service via DI
        private void OpenProjectDetails()
        {
            var context = AppServices.Context;

            var repo = new ProjectInfoRepository(
                context.Session);

            var service = new ProjectInfoService(repo, context.Session.Logger);

            using var frm = new frm_ProjectDetails(service);
            if (frm.ShowDialog() == DialogResult.OK)
                AppServices.Context.MarkAsModified();

            // Refresh title after user edits
            UpdateWindowTitle();
        }

        // ── EXIT ─────────────────────────────────────

        private void ExitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to exit?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
                == DialogResult.No)
                return;

            if (!HandleUnsavedChangesOnClose()) return;

            Close();
        }

        // ── STUB HANDLERS ────────────────────────────
        // Implemented later one by one
        private void tsmSave_Click(object sender, EventArgs e)
        {
            _ = SaveCurrentProjectAsync(showMessage: false);
        }
        // ── SAVE AS ──────────────────────────────────
        // Saves current project to a new location.
        // Copies entire project folder to destination.
        // Switches session to new location.

        //    private async void tsmSaveAs_Click(
        //object sender, EventArgs e)
        //    {
        //        if(!AppServices.HasContext) return;
        //        string currentFilePath = AppServices.Context.ProjectFilePath;
        //        string currentFolderPath = Path.GetFullPath(Path.GetDirectoryName(currentFilePath)!);
        //        var sfd = new SaveFileDialog()
        //        {
        //            Filter =
        //                "Land Pooling Project File (*.lpp)|*.lpp",
        //            Title = "Save Project As",
        //            FileName = Path.GetFileName(currentFilePath)
        //        };

        //        if(sfd.ShowDialog() != DialogResult.OK)
        //            return;

        //        string fullDestFilePath = sfd.FileName;
        //        string fullDestFolderPath = Path.GetFullPath(Path.GetDirectoryName(fullDestFilePath)!);

        //        if(fullDestFolderPath.Equals(Path.GetDirectoryName(currentFolderPath), StringComparison.OrdinalIgnoreCase)|| fullDestFolderPath.Equals()
        //        {
        //            MessageBox.Show(
        //                "Selected file is the same as current project file.",
        //                "Save As",
        //                MessageBoxButtons.OK,
        //                MessageBoxIcon.Warning);
        //            return;
        //        }

        //    }

        private async void tsmSaveAs_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;

            var currentFilePath = AppServices.Context.ProjectFilePath; //C:\Projects\MyProject\Myproject.lpp
            var currentFileName = Path.GetFileNameWithoutExtension(currentFilePath); // MyProject
            var currentFolder = Path.GetFullPath(
                                        Path.GetDirectoryName(currentFilePath)!);
            // currentFolder = C:\Projects\MyProject\

            // ── STEP 1 — Open SaveFileDialog ─────────

            string pickedFilePath = string.Empty;

            string pickedFolder = string.Empty;
            string pickedFileName = string.Empty;
            string destFolder = string.Empty;
            string destFile = string.Empty;
            string destFileName = string.Empty;
            while (true)
            {
                using var sfd = new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(currentFolder),
                    Filter = "Land Pooling Project File (*.lpp)|*.lpp",
                    Title = "Save Project As",
                    FileName = Path.GetFileNameWithoutExtension(currentFilePath)
                };
                if (sfd.ShowDialog() != DialogResult.OK) return;

                pickedFilePath = sfd.FileName; //C:\Projects\MyProject2.lpp
                                               // pickedFilePath = wherever user chose

                pickedFolder = Path.GetFullPath(
                                            Path.GetDirectoryName(pickedFilePath)!); //C:\Projects\
                pickedFileName = Path.GetFileNameWithoutExtension(pickedFilePath); // MyProject2
                destFolder = Path.Combine(pickedFolder, pickedFileName); //C:\Projects\MyProject2
                destFile = Path.Combine(destFolder, Path.GetFileName(pickedFilePath)); //C:\Projects\MyProject2\MyProject2.lpp
                                                                                       // pickedFolder = folder user browsed to
                destFileName = Path.GetFileNameWithoutExtension(destFile); // MyProject
                // ── STEP 2 — Block saving inside current project folder ──
                // currentFolder = C:\Projects\MyProject\
                // If user picked something inside it → block
                if (string.Equals(destFile, currentFilePath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    await SaveCurrentProjectAsync(showMessage: false);
                    return;
                }
                else if (destFile.StartsWith(currentFileName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Cannot save inside the current project folder.\n\n" +
                        "Please choose a different location.",
                        "Invalid Location",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    continue;
                }
                else break;
            }
            // ── STEP 3 — Same parent directory → save normally ───
            // If user is browsing the same parent folder
            // where current project folder lives → just save
            // Example:
            // currentFolder = C:\Projects\MyProject\
            // parent        = C:\Projects\
            // pickedFolder  = C:\Projects\  → save normally


            // ── STEP 4 — Different folder → Save As ──────────────

            try
            {
                Cursor = Cursors.WaitCursor;


                // 4a.1 — Checkpoint WAL into main .lpp file
                // SQLite WAL mode keeps pending writes in a
                // separate -wal file. Checkpoint merges them
                // into the main database file before we copy.
                await AppServices.Context.Session
                    .GetContext()
                    .Database
                    .ExecuteSqlRawAsync("PRAGMA wal_checkpoint(FULL);");

                // 4b — Delete destination if already exists
                if (Directory.Exists(destFolder))
                    Directory.Delete(destFolder, recursive: true);

                // 4c — Copy entire project folder to new location
                CopyProjectFolder(currentFolder, destFolder);

                // 4d — Rename copied .lpp file inside new folder
                //      to match new project name
                string copiedFile = Path.Combine(
                    destFolder,
                    Path.GetFileName(currentFilePath));

                if (File.Exists(copiedFile) &&
                    !string.Equals(copiedFile, destFile,
                        StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(copiedFile, destFile,
                        overwrite: true);
                }

                // 4e — Close current session
                AppServices.Context.Session.Dispose();
                AppServices.ClearContext();

                // 4f — Open new session at new location
                var session = new ProjectSessionFactory()
                    .CreateSession(destFile);
                var context = new ProjectContext(
                    session, destFile);

                AppServices.SetContext(context);
                context.StateChanged += UpdateWindowTitle;

                // 4g — Update ProjectName in new database
                var info = await session.GetContext()
                    .ProjectInfo.FirstOrDefaultAsync();
                if (info != null)
                {
                    info.ProjectName = destFileName;
                    await session.GetContext().SaveChangesAsync();
                    context.SetInfo(info);
                }

                // 4h — WAL checkpoint on new file
                await session.GetContext()
                    .Database
                    .ExecuteSqlRawAsync(
                        "PRAGMA wal_checkpoint(FULL);");

                // 4i — Create fresh backup in new location
                // No old backups copied — clean history
                _backupService.CreateBackup(destFile);

                // 4j — Update UI
                EnableProjectMenuItems();
                UpdateWindowTitle();
                context.MarkAsSaved();

                MessageBox.Show(
                    $"Project saved as:\n{destFile}",
                    "Save As Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save As failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Copies all files from source to dest.
        /// Skips .bak files — new project starts
        /// with clean backup history.
        /// </summary>
        private static void CopyProjectFolder(
            string source, string dest)
        {
            string fullSource = Path.GetFullPath(source);
            string fullDest = Path.GetFullPath(dest);

            Directory.CreateDirectory(fullDest);

            // Copy directory structure first (includes empty folders)
            foreach (string dir in Directory.GetDirectories(
                fullSource, "*", SearchOption.AllDirectories))
            {
                string relativeDir = Path.GetRelativePath(
                    fullSource, dir);
                string destDir = Path.Combine(fullDest, relativeDir);
                Directory.CreateDirectory(destDir);
            }

            foreach (string file in Directory.GetFiles(
                fullSource, "*",
                SearchOption.AllDirectories))
            {
                string fullFile = Path.GetFullPath(file);

                // Skip backup files
                if (fullFile.EndsWith(".bak",
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                string relativePath = Path.GetRelativePath(
                    fullSource, fullFile);
                string destFile = Path.Combine(
                    fullDest, relativePath);

                Directory.CreateDirectory(
                    Path.GetDirectoryName(destFile)!);

                File.Copy(fullFile, destFile, overwrite: true);
            }
        }
        /// <summary>
        /// Copies all files from source to dest.
        /// Skips .bak files — new project starts
        /// with clean backup history.
        /// </summary>
        //private static void CopyProjectFolder(
        //    string source, string dest)
        //{
        //    string fullSource = Path.GetFullPath(source);
        //    string fullDest = Path.GetFullPath(dest);

        //    Directory.CreateDirectory(fullDest);

        //    foreach (string file in Directory.GetFiles(
        //        fullSource, "*",
        //        SearchOption.AllDirectories))
        //    {
        //        string fullFile = Path.GetFullPath(file);

        //        // Skip backup files
        //        if (fullFile.EndsWith(".bak",
        //            StringComparison.OrdinalIgnoreCase))
        //            continue;

        //        string relativePath = Path.GetRelativePath(
        //            fullSource, fullFile);
        //        string destFile = Path.Combine(
        //            fullDest, relativePath);

        //        Directory.CreateDirectory(
        //            Path.GetDirectoryName(destFile)!);

        //        File.Copy(fullFile, destFile, overwrite: true);
        //    }
        //}

        /// <summary>
        /// Copies all files from source to dest.
        /// Skips .bak files — new project starts
        /// with clean backup history.
        /// Also skips files inside dest to prevent
        /// infinite copy if dest is inside source.
        /// </summary>


        /// <summary>
        /// Updates ProjectName in the copied database
        /// to match the new file name chosen by user.
        /// </summary>
        private static async Task UpdateProjectNameAsync(
            ProjectSession session, string newName)
        {
            try
            {
                var info = await session
                    .GetContext()
                    .ProjectInfo
                    .FirstOrDefaultAsync();

                if (info == null) return;

                info.ProjectName = newName;
                await session.GetContext().SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"UpdateProjectNameAsync failed: {ex.Message}");
            }
        }
        private void projectSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;
            OpenProjectSettings();
        }

        private void backupProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: implement backup
        }

        private void ImportParcelOwnerShipRecords_Click(object sender, EventArgs e)
        {
            // TODO: implement import
        }

        private void landOwnerDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: implement land owner data
        }

        private void startReplotWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: implement replot workspace
        }

        private void AreaConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: implement area converter
        }

        private async void tsmOpenProject_Click(object sender, EventArgs e)
        {
            if (AppServices.HasContext && AppServices.Context.HasUnsavedChanges)
            {
                {
                    var result = MessageBox.Show(
                        "Do you want to save and current project before opening another one?",
                        "Save Current Project",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        await SaveCurrentProjectAsync(showMessage: false);
                    }
                }
            }

            using OpenFileDialog ofd = new()
            {
                Filter =
                    "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Open Project"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string projectFilePath = ofd.FileName;

            if (!File.Exists(projectFilePath))
            {
                MessageBox.Show(
                    "Selected project file was not found.",
                    "Open Project",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            await CloseCurrentProjectAsync();

            await OpenProjectInternalAsync(
                projectFilePath,
                checkUnsavedChanges: true);
        }


        private async Task OpenProjectInternalAsync(
            string projectFilePath,
            bool checkUnsavedChanges)
        {
            if (checkUnsavedChanges &&
                AppServices.HasContext &&
                !HandleUnsavedChangesOnClose())
                return;

            ProjectSession? session = null;
            try
            {
                if (AppServices.HasContext)
                    AppServices.Context.StateChanged -= UpdateWindowTitle;

                session = new ProjectSessionFactory()
                    .CreateSession(projectFilePath);

                // Ensure database schema is up to date
                await session.GetContext().Database.MigrateAsync();

                var context = new ProjectContext(
                    session, projectFilePath);

                var repo = new ProjectInfoRepository(session);
                var service = new ProjectInfoService(
                    repo, session.Logger);

                var info = await service.GetAsync();
                if (info == null)
                    throw new InvalidOperationException(
                        "Project file is invalid or missing project information.");

                context.SetInfo(info);
                context.StateChanged += UpdateWindowTitle;
                AppServices.SetContext(context);

                EnableProjectMenuItems();
                UpdateWindowTitle();
                InitializeProjectWorkspace();
            }
            catch (Exception ex)
            {
                session?.Dispose();
                MessageBox.Show(
                    $"Failed to open project: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task<bool> SaveCurrentProjectAsync(bool showMessage)
        {
            if (!AppServices.HasContext) return true;

            if (!AppServices.Context.HasUnsavedChanges)
            {
                if (showMessage)
                    MessageBox.Show(
                        "No changes to save.",
                        "Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                return true;
            }

            try
            {
                string filePath = AppServices.Context.ProjectFilePath;
                var dbContext = AppServices.Context.Session.GetContext();

                // Repositories write to the DB immediately on every edit,
                // so there are no pending EF Core changes to flush here.
                // Save = WAL checkpoint (merge WAL → .lpp) + rotate backups.

                // 1. WAL checkpoint — merge WAL journal into the .lpp file
                await dbContext.Database
                    .ExecuteSqlRawAsync(
                        "PRAGMA wal_checkpoint(FULL);");

                // 2. Rotate backups — .bak = state just before this save
                _backupService.CreateBackup(filePath);

                // 3. Mark clean
                AppServices.Context.MarkAsSaved();

                if (showMessage)
                    MessageBox.Show(
                        "Project saved successfully.",
                        "Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(
                    destinationDir,
                    Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(
                    destinationDir,
                    Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }

        private void tsmProjectSetting_Click(object sender, EventArgs e)
        {
            OpenProjectSettings();
        }

        private void lblTransparency_Click(object sender, EventArgs e)
        {

        }

        private async void tsmBackupProject_Click(
    object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;

            try
            {
                // Save first then backup
                var saved = await SaveCurrentProjectAsync(
                    showMessage: false);
                if (!saved) return;

                MessageBox.Show(
                    "Project backed up successfully.",
                    "Backup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Backup failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void tsmRestoreBackup_Click(
    object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;

            string filePath = AppServices.Context
                .ProjectFilePath;

            var backups = _backupService.GetBackups(filePath);

            if (backups.Count == 0)
            {
                MessageBox.Show(
                    "No backup files found for this project.",
                    "Restore",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var frm = new frmBackupManager(
                filePath, backups);

            if (frm.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Close current session before restore
                AppServices.Context.Session.Dispose();
                AppServices.ClearContext();

                // Restore selected backup
                _backupService.RestoreFromBackup(
                    filePath, frm.SelectedBackupPath!);

                // Reopen project from restored file
                await OpenProjectInternalAsync(
                    filePath, checkUnsavedChanges: false);

                MessageBox.Show(
                    "Project restored successfully.",
                    "Restore Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Restore failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void tsmCloseProject_Click(object sender, EventArgs e)
        {
            await CloseCurrentProjectAsync();
        }
    }
    // ── PROJECT FOLDER CREATOR ───────────────────

    // Creates standard folder structure
    // for a new project
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
                Directory.CreateDirectory(Path.Combine(root, folder));
        }
    }
}

