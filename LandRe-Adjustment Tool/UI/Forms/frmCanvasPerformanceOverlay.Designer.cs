namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmCanvasPerformanceOverlay
    {
        private System.ComponentModel.IContainer components = null;

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
            components = new System.ComponentModel.Container();
            pnlHeader = new Panel();
            btnClose = new Button();
            lblTitle = new Label();
            pnlBody = new TableLayoutPanel();
            lblOverall = new Label();
            lblFrame = new Label();
            lblObjects = new Label();
            lblRendered = new Label();
            lblDatabase = new Label();
            lblCache = new Label();
            lblAdvice = new Label();
            lblUpdated = new Label();
            refreshTimer = new System.Windows.Forms.Timer(components);
            pnlHeader.SuspendLayout();
            pnlBody.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(12, 8, 8, 8);
            pnlHeader.Size = new Size(430, 42);
            pnlHeader.TabIndex = 0;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(387, 8);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(34, 26);
            btnClose.TabIndex = 1;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblTitle.Location = new Point(12, 11);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(139, 19);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Canvas Performance";
            // 
            // pnlBody
            // 
            pnlBody.ColumnCount = 1;
            pnlBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlBody.Controls.Add(lblOverall, 0, 0);
            pnlBody.Controls.Add(lblFrame, 0, 1);
            pnlBody.Controls.Add(lblObjects, 0, 2);
            pnlBody.Controls.Add(lblRendered, 0, 3);
            pnlBody.Controls.Add(lblDatabase, 0, 4);
            pnlBody.Controls.Add(lblCache, 0, 5);
            pnlBody.Controls.Add(lblAdvice, 0, 6);
            pnlBody.Controls.Add(lblUpdated, 0, 7);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(0, 42);
            pnlBody.Name = "pnlBody";
            pnlBody.Padding = new Padding(12, 8, 12, 12);
            pnlBody.RowCount = 8;
            pnlBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            pnlBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            pnlBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            pnlBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            pnlBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            pnlBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            pnlBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
            pnlBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            pnlBody.Size = new Size(430, 388);
            pnlBody.TabIndex = 1;
            // 
            // lblOverall
            // 
            lblOverall.Dock = DockStyle.Fill;
            lblOverall.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblOverall.Location = new Point(15, 8);
            lblOverall.Name = "lblOverall";
            lblOverall.Size = new Size(400, 34);
            lblOverall.TabIndex = 0;
            lblOverall.Text = "Overall: waiting for canvas data";
            lblOverall.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFrame
            // 
            lblFrame.Dock = DockStyle.Fill;
            lblFrame.Location = new Point(15, 42);
            lblFrame.Name = "lblFrame";
            lblFrame.Size = new Size(400, 34);
            lblFrame.TabIndex = 1;
            lblFrame.Text = "Screen speed: waiting";
            lblFrame.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblObjects
            // 
            lblObjects.Dock = DockStyle.Fill;
            lblObjects.Location = new Point(15, 76);
            lblObjects.Name = "lblObjects";
            lblObjects.Size = new Size(400, 42);
            lblObjects.TabIndex = 2;
            lblObjects.Text = "Objects loaded: waiting";
            lblObjects.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblRendered
            // 
            lblRendered.Dock = DockStyle.Fill;
            lblRendered.Location = new Point(15, 118);
            lblRendered.Name = "lblRendered";
            lblRendered.Size = new Size(400, 52);
            lblRendered.TabIndex = 3;
            lblRendered.Text = "Last redraw: waiting";
            lblRendered.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDatabase
            // 
            lblDatabase.Dock = DockStyle.Fill;
            lblDatabase.Location = new Point(15, 170);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new Size(400, 62);
            lblDatabase.TabIndex = 4;
            lblDatabase.Text = "Last database work: no save/load measured yet";
            lblDatabase.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCache
            // 
            lblCache.Dock = DockStyle.Fill;
            lblCache.Location = new Point(15, 232);
            lblCache.Name = "lblCache";
            lblCache.Size = new Size(400, 42);
            lblCache.TabIndex = 5;
            lblCache.Text = "Canvas cache: waiting";
            lblCache.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAdvice
            // 
            lblAdvice.Dock = DockStyle.Fill;
            lblAdvice.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblAdvice.Location = new Point(15, 274);
            lblAdvice.Name = "lblAdvice";
            lblAdvice.Size = new Size(400, 74);
            lblAdvice.TabIndex = 6;
            lblAdvice.Text = "Tip: open a project and draw, move, copy, or delete objects.";
            lblAdvice.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUpdated
            // 
            lblUpdated.Dock = DockStyle.Fill;
            lblUpdated.Location = new Point(15, 348);
            lblUpdated.Name = "lblUpdated";
            lblUpdated.Size = new Size(400, 28);
            lblUpdated.TabIndex = 7;
            lblUpdated.Text = "Updated: --";
            lblUpdated.TextAlign = ContentAlignment.MiddleRight;
            // 
            // refreshTimer
            // 
            refreshTimer.Interval = 750;
            // 
            // frmCanvasPerformanceOverlay
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(430, 430);
            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCanvasPerformanceOverlay";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Canvas Performance";
            TopMost = false;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlBody.ResumeLayout(false);
            ResumeLayout(false);
            Land_Readjustment_Tool.UI.Forms.RecordFormTheme.Apply(this);
        }

        private Panel pnlHeader;
        private Button btnClose;
        private Label lblTitle;
        private TableLayoutPanel pnlBody;
        private Label lblOverall;
        private Label lblFrame;
        private Label lblObjects;
        private Label lblRendered;
        private Label lblDatabase;
        private Label lblCache;
        private Label lblAdvice;
        private Label lblUpdated;
        private System.Windows.Forms.Timer refreshTimer;
    }
}
