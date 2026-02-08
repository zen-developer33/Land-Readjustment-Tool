using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;
using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmStartup : Form
    {
        private const string RecentProjectsFileName = "RecentProjects.txt";

        public frmStartup()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Wire card paint events
            pnlNewProject.Paint += PnlNewProject_Paint;
            pnlOpenProject.Paint += PnlOpenProject_Paint;
            pnlFooter.Paint += PnlFooter_Paint;

            // Wire card click events
            pnlNewProject.Click += (s, e) => NewProject_Click();
            pnlOpenProject.Click += (s, e) => OpenProject_Click();

            SetupFooterEvents();
            LoadRecentProjects();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                LoadRecentProjects();
        }

        // ==================== GRADIENT CARD PAINTING ====================

        private void PnlNewProject_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, pnlNewProject.Width - 1, pnlNewProject.Height - 1);
            using var path = GetRoundedRectPath(rect, 14);
            using var gradientBrush = new LinearGradientBrush(
                rect, Color.FromArgb(25, 55, 109), Color.FromArgb(55, 95, 160),
                LinearGradientMode.Horizontal);
            g.FillPath(gradientBrush, path);

            // Icon placeholder area
            DrawDocumentIcon(g, new Rectangle(25, 25, 55, 65));

            // Title
            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            g.DrawString("New Project", titleFont, Brushes.White, 100, 28);

            // Description
            using var descFont = new Font("Segoe UI", 10F);
            using var descBrush = new SolidBrush(Color.FromArgb(200, 215, 235));
            g.DrawString("Create a new land re adjustment project", descFont, descBrush, 100, 64);
        }

        private void PnlOpenProject_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, pnlOpenProject.Width - 1, pnlOpenProject.Height - 1);
            using var path = GetRoundedRectPath(rect, 14);
            using var gradientBrush = new LinearGradientBrush(
                rect, Color.FromArgb(55, 105, 65), Color.FromArgb(125, 165, 80),
                LinearGradientMode.Horizontal);
            g.FillPath(gradientBrush, path);

            // Icon placeholder area
            DrawFolderIcon(g, new Rectangle(25, 25, 55, 65));

            // Title
            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            g.DrawString("Open Project", titleFont, Brushes.White, 100, 28);

            // Description
            using var descFont = new Font("Segoe UI", 10F);
            using var descBrush = new SolidBrush(Color.FromArgb(200, 230, 205));
            g.DrawString("Open an existing project from your files", descFont, descBrush, 100, 64);
        }

        private static void DrawDocumentIcon(Graphics g, Rectangle area)
        {
            int cx = area.X + area.Width / 2;
            int cy = area.Y + area.Height / 2;
            using var pen = new Pen(Color.FromArgb(160, 190, 225), 1.8f);

            // Document body
            int w = 24, h = 32;
            int fold = 8;
            var pts = new[]
            {
                new Point(cx - w / 2, cy - h / 2),
                new Point(cx + w / 2 - fold, cy - h / 2),
                new Point(cx + w / 2, cy - h / 2 + fold),
                new Point(cx + w / 2, cy + h / 2),
                new Point(cx - w / 2, cy + h / 2),
            };
            g.DrawPolygon(pen, pts);
            g.DrawLine(pen, pts[1], new Point(pts[1].X, pts[1].Y + fold));
            g.DrawLine(pen, new Point(pts[1].X, pts[1].Y + fold), pts[2]);

            // Text lines
            g.DrawLine(pen, cx - 7, cy - 3, cx + 7, cy - 3);
            g.DrawLine(pen, cx - 7, cy + 5, cx + 7, cy + 5);
            g.DrawLine(pen, cx - 7, cy + 13, cx + 2, cy + 13);
        }

        private static void DrawFolderIcon(Graphics g, Rectangle area)
        {
            int cx = area.X + area.Width / 2;
            int cy = area.Y + area.Height / 2;
            using var pen = new Pen(Color.FromArgb(160, 215, 175), 1.8f);
            using var fillBrush = new SolidBrush(Color.FromArgb(30, 255, 255, 255));

            // Folder tab
            var tabPts = new[]
            {
                new Point(cx - 16, cy - 8),
                new Point(cx - 16, cy - 14),
                new Point(cx - 3, cy - 14),
                new Point(cx + 1, cy - 8),
            };
            g.DrawLines(pen, tabPts);

            // Folder body
            var folderRect = new Rectangle(cx - 16, cy - 8, 32, 22);
            g.FillRectangle(fillBrush, folderRect);
            g.DrawRectangle(pen, folderRect);
        }

        private void PnlFooter_Paint(object? sender, PaintEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(218, 220, 224));
            e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ==================== CARD ACTIONS ====================

        private void NewProject_Click()
        {
            using SaveFileDialog saveFileDialog = new()
            {
                Filter = "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Create New Project"
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                string filePathFromDialog = saveFileDialog.FileName;
                string projectFileName = Path.GetFileNameWithoutExtension(filePathFromDialog);
                string projectFolder = Path.Combine(
                    Path.GetDirectoryName(filePathFromDialog)!, projectFileName);
                string projectFilePath = Path.Combine(projectFolder, Path.GetFileName(filePathFromDialog));
                Directory.CreateDirectory(projectFolder);

                frmMain.ProjectFolderCreator.CreateFolders(projectFolder);

                DatabaseHelper dbHelper = new(projectFilePath);
                dbHelper.InitializeDatabase();

                ProjectInfo projectInfo = new()
                {
                    ProjectName = projectFileName,
                    ProjectPath = projectFilePath,
                    CreatedDate = DateTime.Now
                };
                CurrentProject.Info = projectInfo;

                ProjectInfoRepository repo = new(dbHelper.GetConnection());
                repo.SaveProjectInfo(projectInfo);
                CurrentProject.MarkAsSaved();

                AddToRecentProjects(projectInfo.ProjectName, projectFilePath);

                frm_ProjectDetails frm = new();
                frm.ShowDialog();

                OpenMainForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create project: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenProject_Click()
        {
            using OpenFileDialog ofd = new()
            {
                Filter = "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Open Project"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            OpenProjectFile(ofd.FileName);
        }

        private void OpenProjectFile(string filePath)
        {
            if (!ProjectFileValidator.IsValidProjectFile(filePath))
            {
                MessageBox.Show("Invalid or corrupted project file.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                DatabaseHelper db = new(filePath);
                db.InitializeDatabase();

                ProjectInfoRepository repo = new(db.GetConnection());
                CurrentProject.Info = repo.GetProjectInfo();

                if (CurrentProject.Info != null)
                {
                    CurrentProject.Info.ProjectPath = filePath;
                    AddToRecentProjects(CurrentProject.Info.ProjectName, filePath);
                }

                OpenMainForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open project: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                CurrentProject.Info = null;
            }
        }

        private void OpenMainForm()
        {
            var mainForm = new frmMain();
            mainForm.Show();
            this.Hide();
            mainForm.FormClosed += (s, e) =>
            {
                if (CurrentProject.IsOpen)
                    CurrentProject.Close();
                this.Show();
                LoadRecentProjects();
            };
        }

        // ==================== RECENT PROJECTS ====================

        private static string RecentProjectsFilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RePlot", RecentProjectsFileName);

        private void LoadRecentProjects()
        {
            pnlRecentList.Controls.Clear();

            var recentProjects = GetRecentProjects();

            if (recentProjects.Count == 0)
            {
                var noProjectsLabel = new Label
                {
                    Text = "No recent projects",
                    ForeColor = Color.FromArgb(170, 170, 170),
                    Font = new Font("Segoe UI", 11f),
                    Dock = DockStyle.Top,
                    Height = 60,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlRecentList.Controls.Add(noProjectsLabel);
                return;
            }

            // Add in reverse order so DockStyle.Top stacks them correctly
            for (int i = recentProjects.Count - 1; i >= 0; i--)
            {
                var item = CreateRecentProjectItem(recentProjects[i].name, recentProjects[i].path);
                pnlRecentList.Controls.Add(item);
            }
        }

        private Panel CreateRecentProjectItem(string name, string path)
        {
            var itemPanel = new Panel
            {
                Height = 68,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            var separator = new Panel
            {
                Height = 1,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(232, 234, 237)
            };

            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 11.5f),
                ForeColor = Color.FromArgb(35, 35, 35),
                Location = new Point(55, 12),
                AutoSize = true,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            var lblPath = new Label
            {
                Text = path,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(140, 140, 140),
                Location = new Point(55, 36),
                AutoSize = true,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            itemPanel.Controls.AddRange([separator, lblName, lblPath]);

            // Paint document icon on the left and open-arrow on the right
            itemPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(180, 185, 195), 1.3f);

                // Small document icon
                g.DrawRectangle(pen, 17, 16, 20, 28);
                g.DrawLine(pen, 31, 16, 37, 16);
                g.DrawLine(pen, 37, 16, 37, 22);

                // Open-arrow icon on the right
                int rx = itemPanel.Width - 38;
                using var arrowPen = new Pen(Color.FromArgb(175, 180, 190), 1.2f);
                g.DrawRectangle(arrowPen, rx, 22, 14, 18);
                g.DrawLine(arrowPen, rx + 3, 19, rx + 3, 22);
                g.DrawLine(arrowPen, rx + 3, 19, rx + 11, 19);
                g.DrawLine(arrowPen, rx + 11, 19, rx + 11, 22);
            };

            // Click handlers
            void openHandler(object? s, EventArgs e) => OpenRecentProject(path);
            itemPanel.Click += openHandler;
            lblName.Click += openHandler;
            lblPath.Click += openHandler;

            // Hover effects
            var hoverColor = Color.FromArgb(240, 244, 249);
            void mouseEnter(object? s, EventArgs e)
            {
                itemPanel.BackColor = hoverColor;
                lblName.BackColor = hoverColor;
                lblPath.BackColor = hoverColor;
            }
            void mouseLeave(object? s, EventArgs e)
            {
                itemPanel.BackColor = Color.White;
                lblName.BackColor = Color.White;
                lblPath.BackColor = Color.White;
            }

            itemPanel.MouseEnter += mouseEnter;
            itemPanel.MouseLeave += mouseLeave;
            lblName.MouseEnter += mouseEnter;
            lblName.MouseLeave += mouseLeave;
            lblPath.MouseEnter += mouseEnter;
            lblPath.MouseLeave += mouseLeave;

            return itemPanel;
        }

        private void OpenRecentProject(string path)
        {
            if (!File.Exists(path))
            {
                var result = MessageBox.Show(
                    $"The project file was not found:\n{path}\n\nRemove from recent list?",
                    "File Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    RemoveFromRecentProjects(path);
                    LoadRecentProjects();
                }
                return;
            }

            OpenProjectFile(path);
        }

        private static List<(string name, string path)> GetRecentProjects()
        {
            var list = new List<(string name, string path)>();
            string filePath = RecentProjectsFilePath;

            if (!File.Exists(filePath))
                return list;

            try
            {
                foreach (var line in File.ReadAllLines(filePath))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                        list.Add((parts[0], parts[1]));
                }
            }
            catch
            {
                // Ignore corrupt recent file
            }

            return list;
        }

        internal static void AddToRecentProjects(string name, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RecentProjectsFilePath)!);

            var projects = GetRecentProjects();
            projects.RemoveAll(p => p.path.Equals(path, StringComparison.OrdinalIgnoreCase));
            projects.Insert(0, (name, path));

            if (projects.Count > 10)
                projects = projects.Take(10).ToList();

            File.WriteAllLines(RecentProjectsFilePath,
                projects.Select(p => $"{p.name}|{p.path}"));
        }

        private static void RemoveFromRecentProjects(string path)
        {
            var projects = GetRecentProjects();
            projects.RemoveAll(p => p.path.Equals(path, StringComparison.OrdinalIgnoreCase));

            Directory.CreateDirectory(Path.GetDirectoryName(RecentProjectsFilePath)!);
            File.WriteAllLines(RecentProjectsFilePath,
                projects.Select(p => $"{p.name}|{p.path}"));
        }

        // ==================== FOOTER EVENTS ====================

        private void SetupFooterEvents()
        {
            lnkProjectBackup.Click += (s, e) =>
            {
                if (!CurrentProject.IsOpen)
                {
                    MessageBox.Show("Please open a project first.", "No Project",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using var backupMgr = new frmBackupManager(CurrentProject.Info!.ProjectPath);
                backupMgr.ShowDialog();
            };

            lnkSystemSettings.Click += (s, e) =>
            {
                MessageBox.Show("System Settings will be available in a future update.",
                    "System Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            lnkHelpDocs.Click += (s, e) =>
            {
                MessageBox.Show("Help & Documentation will be available in a future update.",
                    "Help & Documentation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Hover effect for footer links
            foreach (var lnk in new[] { lnkProjectBackup, lnkSystemSettings, lnkHelpDocs })
            {
                lnk.MouseEnter += (s, e) => ((Label)s!).ForeColor = Color.FromArgb(30, 30, 30);
                lnk.MouseLeave += (s, e) => ((Label)s!).ForeColor = Color.FromArgb(80, 80, 80);
            }
        }
    }
}
