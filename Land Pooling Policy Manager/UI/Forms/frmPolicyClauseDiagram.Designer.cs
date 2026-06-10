using Land_Pooling_Policy_Manager.UI.Controls;

namespace Land_Pooling_Policy_Manager.UI.Forms
{
    partial class frmPolicyClauseDiagram
    {
        private System.ComponentModel.IContainer components = null;
        private Panel commandPanel;
        private Button btnAttachImage;
        private Label lblClause;
        private ZoomPanPanel imageViewer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            commandPanel = new Panel();
            btnAttachImage = new Button();
            lblClause = new Label();
            imageViewer = new ZoomPanPanel();
            commandPanel.SuspendLayout();
            SuspendLayout();
            // 
            // commandPanel
            // 
            commandPanel.BorderStyle = BorderStyle.FixedSingle;
            commandPanel.Controls.Add(btnAttachImage);
            commandPanel.Controls.Add(lblClause);
            commandPanel.Dock = DockStyle.Top;
            commandPanel.Location = new Point(0, 0);
            commandPanel.Name = "commandPanel";
            commandPanel.Padding = new Padding(8, 6, 8, 6);
            commandPanel.Size = new Size(900, 40);
            commandPanel.TabIndex = 0;
            // 
            // btnAttachImage
            // 
            btnAttachImage.Location = new Point(8, 6);
            btnAttachImage.Name = "btnAttachImage";
            btnAttachImage.Size = new Size(169, 28);
            btnAttachImage.TabIndex = 0;
            btnAttachImage.Text = "Attach / Replace";
            btnAttachImage.UseVisualStyleBackColor = true;
            btnAttachImage.Click += btnAttachImage_Click;
            // 
            // lblClause
            // 
            lblClause.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblClause.BorderStyle = BorderStyle.FixedSingle;
            lblClause.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClause.Location = new Point(183, 6);
            lblClause.Name = "lblClause";
            lblClause.Size = new Size(704, 28);
            lblClause.TabIndex = 1;
            lblClause.Text = "Clause diagram";
            lblClause.TextAlign = ContentAlignment.MiddleLeft;
            //
            // imageViewer
            //
            imageViewer.BackColor = Color.Gainsboro;
            imageViewer.BorderStyle = BorderStyle.FixedSingle;
            imageViewer.Dock = DockStyle.Fill;
            imageViewer.Location = new Point(0, 40);
            imageViewer.Name = "imageViewer";
            imageViewer.Size = new Size(900, 560);
            imageViewer.TabIndex = 1;
            imageViewer.TabStop = true;
            //
            // frmPolicyClauseDiagram
            //
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(900, 600);
            Controls.Add(imageViewer);
            Controls.Add(commandPanel);
            MinimizeBox = false;
            MinimumSize = new Size(700, 450);
            Name = "frmPolicyClauseDiagram";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Clause Diagram";
            Load += frmPolicyClauseDiagram_Load;
            commandPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
