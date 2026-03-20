
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Repositories.Spatial;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.Forms.Project;

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
                // TODO: implement save
                return true;
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
            using SaveFileDialog sfd = new()
            {
                Filter =
                    "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Create New Project"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

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
                OpenProjectSettings();

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
            frm.ShowDialog();

            // Refresh title after user edits
            UpdateWindowTitle();
        }


        // ── CLOSE PROJECT ────────────────────────────

        private void tsmCloseProject_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;

            if (!HandleUnsavedChangesOnClose()) return;

            // Unsubscribe from state changes
            AppServices.Context.StateChanged -= UpdateWindowTitle;

            // Clear context — disposes DB connection
            AppServices.ClearContext();

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

            var frm = new frm_ProjectDetails(service);
            frm.ShowDialog();

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

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: implement save
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: implement save as
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

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {

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

