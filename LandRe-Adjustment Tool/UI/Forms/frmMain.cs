
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Repositories.Spatial;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.Forms.Project;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool
{

    public partial class frmMain : Form
    {
        //App Title shown in window Title bar
        private readonly string _appTitle = "Replot";

        //Canvas Control for drawing
        private DrawingCanvasControl _canvasControl;
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
                "You have unsaved changes. " +
                "Save before closing?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                return SaveCurrentProject();
            }

            if (result == DialogResult.No)
                return true;

            // User clicked Cancel — do not close
            return false;
        }

        // ── MENU ITEMS ───────────────────────────────

        // Enables menu items when project is open
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
                    await SaveCurrentProjectAsync(showMessage: true);
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
                AppServices.Context
                    .Session
                    .GetContext()
                    .SaveChanges();

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
            var result = MessageBox.Show("Project Created Successfully.\n\n +" +
                " Would you like to configure project setting later now? \n\n" +
                "You can always change settings later fro Project -> Project Settings menu.",
                "Project Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Yes)
            {
                OpenProjectSettings();
            }
            else
            {
                //User chose defaults - mark as configured so this prompt never shows up again.
                var repo = new ProjectSettingsRepository(AppServices.Context.Session);
                await repo.MarkAsConfiguredAsync();
            }
        }

        private void OpenProjectSettings()
        {
            if (!AppServices.HasContext) return;

            var context = AppServices.Context;

            var repo = new ProjectSettingsRepository(
                context.Session);

            var crsRepo = new CoordinateSystemRepository(context.Session);
            var service = new ProjectSettingsService(repo, context.Session.Logger);
            var datumRepo = new DatumTransformationRepository(context.Session);
            using var frm = new frmProjectSettings(service, crsRepo, datumRepo);
            if (frm.ShowDialog() == DialogResult.OK)
                AppServices.Context.MarkAsModified();

            // Refresh title after user edits
            UpdateWindowTitle();
        }


        // ── CLOSE PROJECT ────────────────────────────

        private async Task tsmCloseProject_Click(object sender, EventArgs e)
        {
            await CloseCurrentProjectAsync();
        }


        private async Task CloseCurrentProjectAsync()
        {
            if (!AppServices.HasContext) return;

            if (!HandleUnsavedChangesOnClose()) return;

            // Unsubscribe from state changes
            AppServices.Context.StateChanged -= UpdateWindowTitle;

            // Clear context — disposes DB connection
            AppServices.ClearContext();

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
            _ = SaveCurrentProjectAsync(showMessage: true);
        }

        private async void tsmSaveAs_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;

            var currentPath = AppServices.Context.ProjectFilePath;

            using SaveFileDialog sfd = new()
            {
                Filter = "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Save Project As",
                FileName = Path.GetFileName(currentPath)
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            string selectedFilePath = sfd.FileName;
            string newProjectName =
                Path.GetFileNameWithoutExtension(selectedFilePath);
            string newProjectFolder = Path.Combine(
                Path.GetDirectoryName(selectedFilePath)!,
                newProjectName);
            string newProjectFilePath = Path.Combine(
                newProjectFolder,
                Path.GetFileName(selectedFilePath));

            if (string.Equals(currentPath, newProjectFilePath,
                StringComparison.OrdinalIgnoreCase))
            {
                await SaveCurrentProjectAsync(showMessage: true);
                return;
            }

            try
            {
                await SaveCurrentProjectAsync(showMessage: false);

                if (Directory.Exists(newProjectFolder))
                {
                    var overwrite = MessageBox.Show(
                        "Destination project folder already exists. Overwrite it?",
                        "Save As",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (overwrite != DialogResult.Yes)
                        return;

                    Directory.Delete(newProjectFolder, recursive: true);
                }

                CopyDirectory(
                    AppServices.Context.ProjectFolderPath,
                    newProjectFolder);

                string copiedOriginalFile = Path.Combine(
                    newProjectFolder,
                    Path.GetFileName(currentPath));

                if (!string.Equals(copiedOriginalFile, newProjectFilePath,
                    StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(copiedOriginalFile))
                {
                    File.Move(copiedOriginalFile, newProjectFilePath, overwrite: true);
                }

                await OpenProjectInternalAsync(
                    newProjectFilePath,
                    checkUnsavedChanges: false);

                MessageBox.Show(
                    "Project saved as a new file successfully.",
                    "Save As",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save As failed: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                        await SaveCurrentProjectAsync(showMessage: true);
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

        private async Task SaveCurrentProjectAsync(bool showMessage)
        {
            if (!AppServices.HasContext) return;

            try
            {
                await AppServices.Context
                    .Session
                    .GetContext()
                    .SaveChangesAsync();

                AppServices.Context.MarkAsSaved();

                if (showMessage)
                {
                    MessageBox.Show(
                        "Project saved successfully.",
                        "Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save failed: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void CopyDirectory(
            string sourceDir,
            string destinationDir)
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

