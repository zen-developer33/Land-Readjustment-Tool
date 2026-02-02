namespace Land_Readjustment_Tool.Forms
{
    partial class frmMapping
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
            dgvMapping = new DataGridView();
            btnConfirmMapping = new Button();
            btnCLear = new Button();
            button2 = new Button();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvMapping).BeginInit();
            SuspendLayout();
            // 
            // dgvMapping
            // 
            dgvMapping.AllowUserToResizeColumns = false;
            dgvMapping.AllowUserToResizeRows = false;
            dgvMapping.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMapping.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvMapping.BackgroundColor = SystemColors.Control;
            dgvMapping.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMapping.Location = new Point(12, 36);
            dgvMapping.Name = "dgvMapping";
            dgvMapping.RowHeadersWidth = 51;
            dgvMapping.Size = new Size(382, 609);
            dgvMapping.TabIndex = 0;
            // 
            // btnConfirmMapping
            // 
            btnConfirmMapping.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnConfirmMapping.AutoSize = true;
            btnConfirmMapping.BackColor = SystemColors.ControlLightLight;
            btnConfirmMapping.Location = new Point(234, 651);
            btnConfirmMapping.Name = "btnConfirmMapping";
            btnConfirmMapping.Size = new Size(160, 37);
            btnConfirmMapping.TabIndex = 1;
            btnConfirmMapping.Text = "Confim Mapping";
            btnConfirmMapping.UseVisualStyleBackColor = false;
            btnConfirmMapping.Click += btnConfirmMapping_Click;
            // 
            // btnCLear
            // 
            btnCLear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCLear.AutoSize = true;
            btnCLear.Location = new Point(143, 651);
            btnCLear.Name = "btnCLear";
            btnCLear.Size = new Size(85, 37);
            btnCLear.TabIndex = 3;
            btnCLear.Text = "Clear";
            btnCLear.UseVisualStyleBackColor = true;
            btnCLear.Click += btnCLear_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.AutoSize = true;
            button2.Location = new Point(46, 651);
            button2.Name = "button2";
            button2.Size = new Size(91, 37);
            button2.TabIndex = 4;
            button2.Text = "Auto-Map";
            button2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.Control;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 13);
            label1.Name = "label1";
            label1.Size = new Size(375, 20);
            label1.TabIndex = 5;
            label1.Text = "Select the correspoding source field to map to target field.";
            label1.Click += label1_Click;
            // 
            // frmMapping
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(403, 695);
            Controls.Add(label1);
            Controls.Add(button2);
            Controls.Add(btnCLear);
            Controls.Add(btnConfirmMapping);
            Controls.Add(dgvMapping);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmMapping";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Data Mapping";
            Load += frmMapping_Load;
            ((System.ComponentModel.ISupportInitialize)dgvMapping).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvMapping;
        private Button btnConfirmMapping;
        private Button btnCLear;
        private Button button2;
        private Label label1;
    }
}