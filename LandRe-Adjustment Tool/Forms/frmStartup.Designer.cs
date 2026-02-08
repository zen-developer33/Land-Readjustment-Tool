namespace Land_Readjustment_Tool.Forms
{
    partial class frmStartup
    {
        private System.ComponentModel.IContainer components = null;

        // Header
        private PictureBox picLogo;
        private Label lblAppTitle;
        private Label lblSubtitle;

        // Action Cards
        private Panel pnlNewProject;
        private Panel pnlOpenProject;

        // Recent Projects
        private Label lblRecentProjects;
        private Panel pnlRecentList;

        // Footer
        private Panel pnlFooter;
        private Label lnkProjectBackup;
        private Label lnkSystemSettings;
        private Label lnkHelpDocs;
        private Label lblVersion;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.picLogo = new PictureBox();
            this.lblAppTitle = new Label();
            this.lblSubtitle = new Label();
            this.pnlNewProject = new Panel();
            this.pnlOpenProject = new Panel();
            this.lblRecentProjects = new Label();
            this.pnlRecentList = new Panel();
            this.pnlFooter = new Panel();
            this.lnkProjectBackup = new Label();
            this.lnkSystemSettings = new Label();
            this.lnkHelpDocs = new Label();
            this.lblVersion = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // 
            // picLogo
            // 
            this.picLogo.Location = new System.Drawing.Point(80, 50);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(120, 115);
            this.picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.TabStop = false;

            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 42F, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = System.Drawing.Color.FromArgb(28, 28, 28);
            this.lblAppTitle.Location = new System.Drawing.Point(200, 52);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Text = "RePlot";

            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblSubtitle.Location = new System.Drawing.Point(208, 142);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "Precision Land Re-Adjustment Software";

            // 
            // pnlNewProject
            // 
            this.pnlNewProject.Location = new System.Drawing.Point(80, 230);
            this.pnlNewProject.Name = "pnlNewProject";
            this.pnlNewProject.Size = new System.Drawing.Size(490, 115);
            this.pnlNewProject.BackColor = System.Drawing.Color.FromArgb(243, 245, 248);
            this.pnlNewProject.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlNewProject.TabIndex = 0;

            // 
            // pnlOpenProject
            // 
            this.pnlOpenProject.Location = new System.Drawing.Point(595, 230);
            this.pnlOpenProject.Name = "pnlOpenProject";
            this.pnlOpenProject.Size = new System.Drawing.Size(465, 115);
            this.pnlOpenProject.BackColor = System.Drawing.Color.FromArgb(243, 245, 248);
            this.pnlOpenProject.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlOpenProject.TabIndex = 1;

            // 
            // lblRecentProjects
            // 
            this.lblRecentProjects.AutoSize = true;
            this.lblRecentProjects.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblRecentProjects.ForeColor = System.Drawing.Color.FromArgb(28, 28, 28);
            this.lblRecentProjects.Location = new System.Drawing.Point(80, 375);
            this.lblRecentProjects.Name = "lblRecentProjects";
            this.lblRecentProjects.Text = "Recent Projects";

            // 
            // pnlRecentList
            // 
            this.pnlRecentList.Anchor = System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;
            this.pnlRecentList.AutoScroll = true;
            this.pnlRecentList.BackColor = System.Drawing.Color.White;
            this.pnlRecentList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.pnlRecentList.Location = new System.Drawing.Point(80, 415);
            this.pnlRecentList.Name = "pnlRecentList";
            this.pnlRecentList.Size = new System.Drawing.Size(985, 270);
            this.pnlRecentList.TabIndex = 2;

            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height = 50;
            this.pnlFooter.Name = "pnlFooter";

            // 
            // lnkProjectBackup
            // 
            this.lnkProjectBackup.AutoSize = true;
            this.lnkProjectBackup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkProjectBackup.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lnkProjectBackup.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lnkProjectBackup.Location = new System.Drawing.Point(80, 15);
            this.lnkProjectBackup.Name = "lnkProjectBackup";
            this.lnkProjectBackup.Text = "??  Project Backup";

            // 
            // lnkSystemSettings
            // 
            this.lnkSystemSettings.AutoSize = true;
            this.lnkSystemSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkSystemSettings.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lnkSystemSettings.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lnkSystemSettings.Location = new System.Drawing.Point(250, 15);
            this.lnkSystemSettings.Name = "lnkSystemSettings";
            this.lnkSystemSettings.Text = "?  System Settings";

            // 
            // lnkHelpDocs
            // 
            this.lnkHelpDocs.AutoSize = true;
            this.lnkHelpDocs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lnkHelpDocs.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lnkHelpDocs.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lnkHelpDocs.Location = new System.Drawing.Point(430, 15);
            this.lnkHelpDocs.Name = "lnkHelpDocs";
            this.lnkHelpDocs.Text = "?  Help & Documentation";

            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            this.lblVersion.Location = new System.Drawing.Point(920, 17);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Text = "Version 1.0 (Build 127)";

            // 
            // pnlFooter - add controls
            // 
            this.pnlFooter.Controls.Add(this.lblVersion);
            this.pnlFooter.Controls.Add(this.lnkHelpDocs);
            this.pnlFooter.Controls.Add(this.lnkSystemSettings);
            this.pnlFooter.Controls.Add(this.lnkProjectBackup);

            // 
            // frmStartup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(243, 245, 248);
            this.ClientSize = new System.Drawing.Size(1140, 760);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "frmStartup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RePlot";

            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.lblAppTitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.pnlNewProject);
            this.Controls.Add(this.pnlOpenProject);
            this.Controls.Add(this.lblRecentProjects);
            this.Controls.Add(this.pnlRecentList);
            this.Controls.Add(this.pnlFooter);

            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
