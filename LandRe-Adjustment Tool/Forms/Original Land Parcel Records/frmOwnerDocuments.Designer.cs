namespace Land_Readjustment_Tool.Forms
{
    partial class frmOwnerDocuments
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            toolStrip1 = new ToolStrip();
            btnAttach = new ToolStripButton();
            btnOpen = new ToolStripButton();
            btnDelete = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            lblDocCount = new ToolStripLabel();
            dgvDocuments = new DataGridView();
            colFileName = new DataGridViewTextBoxColumn();
            colFileType = new DataGridViewTextBoxColumn();
            colFileSize = new DataGridViewTextBoxColumn();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDocuments).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnAttach, btnOpen, btnDelete, toolStripSeparator1, lblDocCount });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(634, 45);
            toolStrip1.TabIndex = 0;
            // 
            // btnAttach
            // 
            btnAttach.Image = Properties.Resources.attach_icon;
            btnAttach.Name = "btnAttach";
            btnAttach.Size = new Size(109, 42);
            btnAttach.Text = "Attach Files";
            btnAttach.Click += btnAttach_Click;
            // 
            // btnOpen
            // 
            btnOpen.Image = Properties.Resources.open_icon;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(69, 42);
            btnOpen.Text = "Open";
            btnOpen.Click += btnOpen_Click;
            // 
            // btnDelete
            // 
            btnDelete.Image = Properties.Resources.delete_icon;
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(77, 42);
            btnDelete.Text = "Delete";
            btnDelete.Click += btnDelete_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 45);
            // 
            // lblDocCount
            // 
            lblDocCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDocCount.Name = "lblDocCount";
            lblDocCount.Size = new Size(174, 42);
            lblDocCount.Text = "Documents Attached: 0";
            // 
            // dgvDocuments
            // 
            dgvDocuments.AllowUserToAddRows = false;
            dgvDocuments.AllowUserToDeleteRows = false;
            dgvDocuments.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDocuments.BackgroundColor = SystemColors.ControlLight;
            dgvDocuments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDocuments.Columns.AddRange(new DataGridViewColumn[] { colFileName, colFileType, colFileSize });
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = Color.SkyBlue;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvDocuments.DefaultCellStyle = dataGridViewCellStyle1;
            dgvDocuments.Location = new Point(0, 49);
            dgvDocuments.Margin = new Padding(4);
            dgvDocuments.Name = "dgvDocuments";
            dgvDocuments.ReadOnly = true;
            dgvDocuments.RowHeadersVisible = false;
            dgvDocuments.RowHeadersWidth = 51;
            dgvDocuments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDocuments.Size = new Size(634, 280);
            dgvDocuments.TabIndex = 1;
            dgvDocuments.CellDoubleClick += dgvDocuments_CellDoubleClick;
            // 
            // colFileName
            // 
            colFileName.HeaderText = "File Name";
            colFileName.MinimumWidth = 6;
            colFileName.Name = "colFileName";
            colFileName.ReadOnly = true;
            colFileName.Width = 380;
            // 
            // colFileType
            // 
            colFileType.HeaderText = "Type";
            colFileType.MinimumWidth = 6;
            colFileType.Name = "colFileType";
            colFileType.ReadOnly = true;
            colFileType.Width = 125;
            // 
            // colFileSize
            // 
            colFileSize.HeaderText = "Size";
            colFileSize.MinimumWidth = 6;
            colFileSize.Name = "colFileSize";
            colFileSize.ReadOnly = true;
            colFileSize.Width = 125;
            // 
            // frmOwnerDocuments
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(634, 333);
            Controls.Add(dgvDocuments);
            Controls.Add(toolStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MinimizeBox = false;
            MinimumSize = new Size(620, 363);
            Name = "frmOwnerDocuments";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Owner Documents";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDocuments).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripButton btnAttach;
        private ToolStripButton btnOpen;
        private ToolStripButton btnDelete;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel lblDocCount;
        private DataGridView dgvDocuments;
        private DataGridViewTextBoxColumn colFileName;
        private DataGridViewTextBoxColumn colFileType;
        private DataGridViewTextBoxColumn colFileSize;
    }
}
