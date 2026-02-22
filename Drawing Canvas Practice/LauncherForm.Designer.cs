namespace Land_Readjustment_Tool.DrawingCanvas
{
    partial class LauncherForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOldVersion = new System.Windows.Forms.Button();
            this.btnNewVersion = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOldVersion
            // 
            this.btnOldVersion.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnOldVersion.Location = new System.Drawing.Point(30, 80);
            this.btnOldVersion.Name = "btnOldVersion";
            this.btnOldVersion.Size = new System.Drawing.Size(240, 60);
            this.btnOldVersion.TabIndex = 0;
            this.btnOldVersion.Text = "Old Version\r\n(Original Code)";
            this.btnOldVersion.UseVisualStyleBackColor = true;
            this.btnOldVersion.Click += new System.EventHandler(this.btnOldVersion_Click);
            // 
            // btnNewVersion
            // 
            this.btnNewVersion.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnNewVersion.Location = new System.Drawing.Point(30, 160);
            this.btnNewVersion.Name = "btnNewVersion";
            this.btnNewVersion.Size = new System.Drawing.Size(240, 60);
            this.btnNewVersion.TabIndex = 1;
            this.btnNewVersion.Text = "New Version ⚡\r\n(Refactored - High Performance)";
            this.btnNewVersion.UseVisualStyleBackColor = true;
            this.btnNewVersion.Click += new System.EventHandler(this.btnNewVersion_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(30, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 40);
            this.label1.TabIndex = 2;
            this.label1.Text = "Choose Version to Launch";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LauncherForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnNewVersion);
            this.Controls.Add(this.btnOldVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "LauncherForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Replot Workspace - Launcher";
            this.Load += new System.EventHandler(this.LauncherForm_Load);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnOldVersion;
        private System.Windows.Forms.Button btnNewVersion;
        private System.Windows.Forms.Label label1;
    }
}