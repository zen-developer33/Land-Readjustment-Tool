namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmPolicyManagerDashboard
    {
        private System.ComponentModel.IContainer components = null;

        // ── Header ────────────────────────────────────────────────────────────
        private Panel headerPanel;
        private Button btnManagePolicies;
        private Label lblCurrentPolicy;
        private Button btnSaveDraft;
        private Button btnApprove;
        private Button btnLockUnlock;
        private Button btnExport;
        private Label lblStatus;

        // ── Main split (sections+clauses on left, details on right) ───────────
        private SplitContainer mainSplit;
        private Panel clauseListPanel;
        private SplitContainer sectionClauseSplit;

        // Sections (left of the inner split) ──
        private Panel sectionPanel;
        private Label lblSectionsHeader;
        private DataGridView dgvSections;
        private DataGridViewTextBoxColumn colSectionCode;
        private DataGridViewTextBoxColumn colSectionHeading;
        private FlowLayoutPanel sectionButtonPanel;
        private Button btnAddSection;
        private Button btnRenameSection;
        private Button btnDeleteSection;
        private Button btnSectionMoveUp;
        private Button btnSectionMoveDown;

        // Clauses (right of the inner split) ──
        private Panel clausePanel;
        private Label lblClausesHeader;
        private DataGridView dgvClauses;
        private DataGridViewTextBoxColumn colClauseCode;
        private DataGridViewTextBoxColumn colClauseHeading;

        // ── Right-click context menu ──────────────────────────────────────────
        private ContextMenuStrip clauseContextMenu;
        private ToolStripMenuItem menuAddClause;
        private ToolStripMenuItem menuAddSubClause;
        private ToolStripMenuItem menuDuplicateClause;
        private ToolStripMenuItem menuDeleteClause;

        // ── Right panel labels (column 0 of detailLayout) ────────────────────
        private Label lblPolicyCode;
        private Label lblPolicyName;
        private Label lblClauseCode;
        private Label lblClauseSection;
        private Label lblClauseHeading;
        private Label lblDescription;
        private Label lblParameters;

        // ── Right panel layout + fields ───────────────────────────────────────
        private TableLayoutPanel detailLayout;
        private TextBox txtPolicyCode;
        private TextBox txtPolicyName;
        private TextBox txtClauseCode;
        private ComboBox cboClauseSection;
        private TextBox txtClauseHeading;
        private TextBox txtClauseDescription;
        private DataGridView dgvParameters;
        private DataGridViewTextBoxColumn colParameterName;
        private DataGridViewTextBoxColumn colParameterValue;
        private DataGridViewTextBoxColumn colParameterUnit;
        private DataGridViewTextBoxColumn colParameterDescription;

        // ── Bottom toolbar + validation ───────────────────────────────────────
        private FlowLayoutPanel clauseButtonPanel;
        private Button btnAddClause;
        private Button btnAddSubClause;
        private Button btnDuplicateClause;
        private Button btnDeleteClause;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnOpenDiagram;
        private Button btnAddParameter;
        private Button btnDeleteParameter;
        private ListBox lstValidation;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            headerPanel = new Panel();
            btnManagePolicies = new Button();
            lblCurrentPolicy = new Label();
            btnSaveDraft = new Button();
            btnApprove = new Button();
            btnLockUnlock = new Button();
            btnExport = new Button();
            lblStatus = new Label();
            mainSplit = new SplitContainer();
            clauseListPanel = new Panel();
            sectionClauseSplit = new SplitContainer();
            sectionPanel = new Panel();
            lblSectionsHeader = new Label();
            dgvSections = new DataGridView();
            colSectionCode = new DataGridViewTextBoxColumn();
            colSectionHeading = new DataGridViewTextBoxColumn();
            sectionButtonPanel = new FlowLayoutPanel();
            btnAddSection = new Button();
            btnRenameSection = new Button();
            btnDeleteSection = new Button();
            btnSectionMoveUp = new Button();
            btnSectionMoveDown = new Button();
            clausePanel = new Panel();
            lblClausesHeader = new Label();
            dgvClauses = new DataGridView();
            colClauseCode = new DataGridViewTextBoxColumn();
            colClauseHeading = new DataGridViewTextBoxColumn();
            clauseContextMenu = new ContextMenuStrip(components);
            menuAddClause = new ToolStripMenuItem();
            menuAddSubClause = new ToolStripMenuItem();
            menuDuplicateClause = new ToolStripMenuItem();
            menuDeleteClause = new ToolStripMenuItem();
            detailLayout = new TableLayoutPanel();
            lblPolicyCode = new Label();
            txtPolicyCode = new TextBox();
            lblPolicyName = new Label();
            txtPolicyName = new TextBox();
            lblClauseCode = new Label();
            txtClauseCode = new TextBox();
            lblClauseSection = new Label();
            cboClauseSection = new ComboBox();
            lblClauseHeading = new Label();
            txtClauseHeading = new TextBox();
            lblDescription = new Label();
            txtClauseDescription = new TextBox();
            lblParameters = new Label();
            dgvParameters = new DataGridView();
            colParameterName = new DataGridViewTextBoxColumn();
            colParameterValue = new DataGridViewTextBoxColumn();
            colParameterUnit = new DataGridViewTextBoxColumn();
            colParameterDescription = new DataGridViewTextBoxColumn();
            clauseButtonPanel = new FlowLayoutPanel();
            btnAddClause = new Button();
            btnAddSubClause = new Button();
            btnDuplicateClause = new Button();
            btnDeleteClause = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            btnOpenDiagram = new Button();
            btnAddParameter = new Button();
            btnDeleteParameter = new Button();
            lstValidation = new ListBox();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
            mainSplit.Panel1.SuspendLayout();
            mainSplit.Panel2.SuspendLayout();
            mainSplit.SuspendLayout();
            clauseListPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sectionClauseSplit).BeginInit();
            sectionClauseSplit.Panel1.SuspendLayout();
            sectionClauseSplit.Panel2.SuspendLayout();
            sectionClauseSplit.SuspendLayout();
            sectionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSections).BeginInit();
            sectionButtonPanel.SuspendLayout();
            clausePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClauses).BeginInit();
            clauseContextMenu.SuspendLayout();
            detailLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParameters).BeginInit();
            clauseButtonPanel.SuspendLayout();
            SuspendLayout();
            //
            // headerPanel
            //
            headerPanel.BorderStyle = BorderStyle.FixedSingle;
            headerPanel.Controls.Add(btnManagePolicies);
            headerPanel.Controls.Add(lblCurrentPolicy);
            headerPanel.Controls.Add(btnSaveDraft);
            headerPanel.Controls.Add(btnApprove);
            headerPanel.Controls.Add(btnLockUnlock);
            headerPanel.Controls.Add(btnExport);
            headerPanel.Controls.Add(lblStatus);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(6, 5, 6, 5);
            headerPanel.Size = new Size(1553, 48);
            headerPanel.TabIndex = 0;
            //
            // btnManagePolicies
            //
            btnManagePolicies.Location = new Point(6, 5);
            btnManagePolicies.Name = "btnManagePolicies";
            btnManagePolicies.Size = new Size(213, 33);
            btnManagePolicies.TabIndex = 0;
            btnManagePolicies.Text = "Select/Manage Policy...";
            btnManagePolicies.UseVisualStyleBackColor = true;
            btnManagePolicies.Click += btnManagePolicies_Click;
            //
            // lblCurrentPolicy
            //
            lblCurrentPolicy.BorderStyle = BorderStyle.FixedSingle;
            lblCurrentPolicy.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCurrentPolicy.Location = new Point(225, 5);
            lblCurrentPolicy.Name = "lblCurrentPolicy";
            lblCurrentPolicy.Size = new Size(374, 33);
            lblCurrentPolicy.TabIndex = 1;
            lblCurrentPolicy.Text = "No policy selected";
            lblCurrentPolicy.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnSaveDraft
            //
            btnSaveDraft.Location = new Point(605, 5);
            btnSaveDraft.Name = "btnSaveDraft";
            btnSaveDraft.Size = new Size(111, 33);
            btnSaveDraft.TabIndex = 2;
            btnSaveDraft.Text = "Save Draft";
            btnSaveDraft.UseVisualStyleBackColor = true;
            btnSaveDraft.Click += btnSaveDraft_Click;
            //
            // btnApprove
            //
            btnApprove.Location = new Point(722, 5);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(78, 33);
            btnApprove.TabIndex = 3;
            btnApprove.Text = "Approve";
            btnApprove.UseVisualStyleBackColor = true;
            btnApprove.Click += btnApprove_Click;
            //
            // btnLockUnlock
            //
            btnLockUnlock.Location = new Point(806, 5);
            btnLockUnlock.Name = "btnLockUnlock";
            btnLockUnlock.Size = new Size(104, 33);
            btnLockUnlock.TabIndex = 4;
            btnLockUnlock.Text = "Lock Editing";
            btnLockUnlock.UseVisualStyleBackColor = true;
            btnLockUnlock.Click += btnLockUnlock_Click;
            //
            // btnExport
            //
            btnExport.Location = new Point(916, 5);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(70, 33);
            btnExport.TabIndex = 5;
            btnExport.Text = "Export";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            //
            // lblStatus
            //
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.Location = new Point(996, 5);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(207, 33);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "Draft";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            //
            // mainSplit
            //
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.Location = new Point(0, 48);
            mainSplit.Name = "mainSplit";
            mainSplit.Panel1.Controls.Add(clauseListPanel);
            mainSplit.Panel2.Controls.Add(detailLayout);
            mainSplit.Size = new Size(1553, 652);
            mainSplit.SplitterDistance = 675;
            mainSplit.TabIndex = 1;
            //
            // clauseListPanel
            //
            clauseListPanel.Controls.Add(sectionClauseSplit);
            clauseListPanel.Dock = DockStyle.Fill;
            clauseListPanel.Location = new Point(0, 0);
            clauseListPanel.Name = "clauseListPanel";
            clauseListPanel.Padding = new Padding(6);
            clauseListPanel.Size = new Size(675, 652);
            clauseListPanel.TabIndex = 0;
            //
            // sectionClauseSplit  (sections | clauses)
            //
            sectionClauseSplit.Dock = DockStyle.Fill;
            sectionClauseSplit.FixedPanel = FixedPanel.Panel1;
            sectionClauseSplit.Location = new Point(6, 6);
            sectionClauseSplit.Name = "sectionClauseSplit";
            sectionClauseSplit.Panel1.Controls.Add(sectionPanel);
            sectionClauseSplit.Panel2.Controls.Add(clausePanel);
            sectionClauseSplit.Size = new Size(663, 640);
            sectionClauseSplit.SplitterDistance = 260;
            sectionClauseSplit.TabIndex = 0;
            //
            // sectionPanel
            //
            sectionPanel.Controls.Add(dgvSections);
            sectionPanel.Controls.Add(sectionButtonPanel);
            sectionPanel.Controls.Add(lblSectionsHeader);
            sectionPanel.Dock = DockStyle.Fill;
            sectionPanel.Name = "sectionPanel";
            sectionPanel.TabIndex = 0;
            //
            // lblSectionsHeader
            //
            lblSectionsHeader.Dock = DockStyle.Top;
            lblSectionsHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSectionsHeader.Name = "lblSectionsHeader";
            lblSectionsHeader.Size = new Size(260, 24);
            lblSectionsHeader.TabIndex = 0;
            lblSectionsHeader.Text = "Sections";
            lblSectionsHeader.TextAlign = ContentAlignment.MiddleLeft;
            lblSectionsHeader.Padding = new Padding(4, 0, 0, 0);
            //
            // dgvSections
            //
            dgvSections.AllowUserToAddRows = false;
            dgvSections.AllowUserToResizeRows = false;
            dgvSections.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSections.Columns.AddRange(new DataGridViewColumn[] { colSectionCode, colSectionHeading });
            dgvSections.Dock = DockStyle.Fill;
            dgvSections.Font = new Font("Segoe UI", 9F);
            dgvSections.MultiSelect = false;
            dgvSections.Name = "dgvSections";
            dgvSections.ReadOnly = true;
            dgvSections.RowHeadersVisible = false;
            dgvSections.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSections.TabIndex = 1;
            dgvSections.SelectionChanged += dgvSections_SelectionChanged;
            //
            // colSectionCode
            //
            colSectionCode.HeaderText = "Code";
            colSectionCode.Name = "colSectionCode";
            colSectionCode.ReadOnly = true;
            colSectionCode.SortMode = DataGridViewColumnSortMode.NotSortable;
            colSectionCode.Width = 50;
            //
            // colSectionHeading
            //
            colSectionHeading.HeaderText = "Section Heading";
            colSectionHeading.Name = "colSectionHeading";
            colSectionHeading.ReadOnly = true;
            colSectionHeading.SortMode = DataGridViewColumnSortMode.NotSortable;
            colSectionHeading.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //
            // sectionButtonPanel
            //
            sectionButtonPanel.Controls.Add(btnAddSection);
            sectionButtonPanel.Controls.Add(btnRenameSection);
            sectionButtonPanel.Controls.Add(btnDeleteSection);
            sectionButtonPanel.Controls.Add(btnSectionMoveUp);
            sectionButtonPanel.Controls.Add(btnSectionMoveDown);
            sectionButtonPanel.Dock = DockStyle.Bottom;
            sectionButtonPanel.FlowDirection = FlowDirection.LeftToRight;
            sectionButtonPanel.Name = "sectionButtonPanel";
            sectionButtonPanel.Size = new Size(260, 36);
            sectionButtonPanel.TabIndex = 2;
            sectionButtonPanel.WrapContents = true;
            //
            // btnAddSection
            //
            btnAddSection.AutoSize = true;
            btnAddSection.Name = "btnAddSection";
            btnAddSection.Size = new Size(80, 30);
            btnAddSection.TabIndex = 0;
            btnAddSection.Text = "Add";
            btnAddSection.UseVisualStyleBackColor = true;
            btnAddSection.Click += btnAddSection_Click;
            //
            // btnRenameSection
            //
            btnRenameSection.AutoSize = true;
            btnRenameSection.Name = "btnRenameSection";
            btnRenameSection.Size = new Size(70, 30);
            btnRenameSection.TabIndex = 1;
            btnRenameSection.Text = "Rename";
            btnRenameSection.UseVisualStyleBackColor = true;
            btnRenameSection.Click += btnRenameSection_Click;
            //
            // btnDeleteSection
            //
            btnDeleteSection.AutoSize = true;
            btnDeleteSection.Name = "btnDeleteSection";
            btnDeleteSection.Size = new Size(60, 30);
            btnDeleteSection.TabIndex = 2;
            btnDeleteSection.Text = "Delete";
            btnDeleteSection.UseVisualStyleBackColor = true;
            btnDeleteSection.Click += btnDeleteSection_Click;
            //
            // btnSectionMoveUp
            //
            btnSectionMoveUp.AutoSize = true;
            btnSectionMoveUp.Name = "btnSectionMoveUp";
            btnSectionMoveUp.Size = new Size(45, 30);
            btnSectionMoveUp.TabIndex = 3;
            btnSectionMoveUp.Text = "Up";
            btnSectionMoveUp.UseVisualStyleBackColor = true;
            btnSectionMoveUp.Click += btnSectionMoveUp_Click;
            //
            // btnSectionMoveDown
            //
            btnSectionMoveDown.AutoSize = true;
            btnSectionMoveDown.Name = "btnSectionMoveDown";
            btnSectionMoveDown.Size = new Size(60, 30);
            btnSectionMoveDown.TabIndex = 4;
            btnSectionMoveDown.Text = "Down";
            btnSectionMoveDown.UseVisualStyleBackColor = true;
            btnSectionMoveDown.Click += btnSectionMoveDown_Click;
            //
            // clausePanel
            //
            clausePanel.Controls.Add(dgvClauses);
            clausePanel.Controls.Add(lblClausesHeader);
            clausePanel.Dock = DockStyle.Fill;
            clausePanel.Name = "clausePanel";
            clausePanel.TabIndex = 0;
            //
            // lblClausesHeader
            //
            lblClausesHeader.Dock = DockStyle.Top;
            lblClausesHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClausesHeader.Name = "lblClausesHeader";
            lblClausesHeader.Size = new Size(399, 24);
            lblClausesHeader.TabIndex = 0;
            lblClausesHeader.Text = "Clauses";
            lblClausesHeader.TextAlign = ContentAlignment.MiddleLeft;
            lblClausesHeader.Padding = new Padding(4, 0, 0, 0);
            //
            // dgvClauses
            //
            dgvClauses.AllowUserToAddRows = false;
            dgvClauses.AllowUserToResizeRows = false;
            dgvClauses.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClauses.Columns.AddRange(new DataGridViewColumn[] { colClauseCode, colClauseHeading });
            dgvClauses.ContextMenuStrip = clauseContextMenu;
            dgvClauses.Dock = DockStyle.Fill;
            dgvClauses.Font = new Font("Segoe UI", 9F);
            dgvClauses.MultiSelect = false;
            dgvClauses.Name = "dgvClauses";
            dgvClauses.ReadOnly = true;
            dgvClauses.RowHeadersWidth = 35;
            dgvClauses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClauses.TabIndex = 1;
            dgvClauses.CellMouseDown += dgvClauses_CellMouseDown;
            dgvClauses.SelectionChanged += dgvClauses_SelectionChanged;
            //
            // colClauseCode
            //
            colClauseCode.HeaderText = "Code";
            colClauseCode.MinimumWidth = 6;
            colClauseCode.Name = "colClauseCode";
            colClauseCode.ReadOnly = true;
            colClauseCode.SortMode = DataGridViewColumnSortMode.NotSortable;
            colClauseCode.Width = 80;
            //
            // colClauseHeading
            //
            colClauseHeading.HeaderText = "Clause / Sub-Clause Heading";
            colClauseHeading.MinimumWidth = 6;
            colClauseHeading.Name = "colClauseHeading";
            colClauseHeading.ReadOnly = true;
            colClauseHeading.SortMode = DataGridViewColumnSortMode.NotSortable;
            colClauseHeading.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //
            // clauseContextMenu
            //
            clauseContextMenu.ImageScalingSize = new Size(20, 20);
            clauseContextMenu.Items.AddRange(new ToolStripItem[] { menuAddClause, menuAddSubClause, menuDuplicateClause, menuDeleteClause });
            clauseContextMenu.Name = "clauseContextMenu";
            clauseContextMenu.Size = new Size(252, 100);
            //
            // menuAddClause
            //
            menuAddClause.Name = "menuAddClause";
            menuAddClause.Size = new Size(251, 24);
            menuAddClause.Text = "Add Clause At Same Level";
            menuAddClause.Click += menuAddClause_Click;
            //
            // menuAddSubClause
            //
            menuAddSubClause.Name = "menuAddSubClause";
            menuAddSubClause.Size = new Size(251, 24);
            menuAddSubClause.Text = "Add Sub-Clause";
            menuAddSubClause.Click += menuAddSubClause_Click;
            //
            // menuDuplicateClause
            //
            menuDuplicateClause.Name = "menuDuplicateClause";
            menuDuplicateClause.Size = new Size(251, 24);
            menuDuplicateClause.Text = "Duplicate Clause";
            menuDuplicateClause.Click += menuDuplicateClause_Click;
            //
            // menuDeleteClause
            //
            menuDeleteClause.Name = "menuDeleteClause";
            menuDeleteClause.Size = new Size(251, 24);
            menuDeleteClause.Text = "Delete Clause";
            menuDeleteClause.Click += menuDeleteClause_Click;
            //
            // detailLayout
            //
            detailLayout.ColumnCount = 4;
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            detailLayout.Controls.Add(lblPolicyCode, 0, 0);
            detailLayout.Controls.Add(txtPolicyCode, 1, 0);
            detailLayout.Controls.Add(lblPolicyName, 2, 0);
            detailLayout.Controls.Add(txtPolicyName, 3, 0);
            detailLayout.Controls.Add(lblClauseCode, 0, 1);
            detailLayout.Controls.Add(txtClauseCode, 1, 1);
            detailLayout.Controls.Add(lblClauseSection, 2, 1);
            detailLayout.Controls.Add(cboClauseSection, 3, 1);
            detailLayout.Controls.Add(lblClauseHeading, 0, 2);
            detailLayout.Controls.Add(txtClauseHeading, 1, 2);
            detailLayout.Controls.Add(lblDescription, 0, 3);
            detailLayout.Controls.Add(txtClauseDescription, 1, 3);
            detailLayout.Controls.Add(lblParameters, 0, 4);
            detailLayout.Controls.Add(dgvParameters, 1, 4);
            detailLayout.Controls.Add(clauseButtonPanel, 0, 5);
            detailLayout.Controls.Add(lstValidation, 0, 6);
            detailLayout.Dock = DockStyle.Fill;
            detailLayout.Location = new Point(0, 0);
            detailLayout.Name = "detailLayout";
            detailLayout.Padding = new Padding(8);
            detailLayout.RowCount = 7;
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            detailLayout.Size = new Size(874, 652);
            detailLayout.TabIndex = 0;
            //
            // lblPolicyCode
            //
            lblPolicyCode.Dock = DockStyle.Fill;
            lblPolicyCode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPolicyCode.Name = "lblPolicyCode";
            lblPolicyCode.Padding = new Padding(4, 0, 0, 0);
            lblPolicyCode.TabIndex = 0;
            lblPolicyCode.Text = "Policy Code:";
            lblPolicyCode.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtPolicyCode
            //
            txtPolicyCode.Dock = DockStyle.Fill;
            txtPolicyCode.Font = new Font("Segoe UI", 9F);
            txtPolicyCode.Name = "txtPolicyCode";
            txtPolicyCode.ReadOnly = true;
            txtPolicyCode.TabIndex = 1;
            //
            // lblPolicyName
            //
            lblPolicyName.Dock = DockStyle.Fill;
            lblPolicyName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPolicyName.Name = "lblPolicyName";
            lblPolicyName.Padding = new Padding(4, 0, 0, 0);
            lblPolicyName.TabIndex = 2;
            lblPolicyName.Text = "Policy Name:";
            lblPolicyName.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtPolicyName
            //
            txtPolicyName.Dock = DockStyle.Fill;
            txtPolicyName.Font = new Font("Segoe UI", 9F);
            txtPolicyName.Name = "txtPolicyName";
            txtPolicyName.ReadOnly = true;
            txtPolicyName.TabIndex = 3;
            //
            // lblClauseCode
            //
            lblClauseCode.Dock = DockStyle.Fill;
            lblClauseCode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClauseCode.Name = "lblClauseCode";
            lblClauseCode.Padding = new Padding(4, 0, 0, 0);
            lblClauseCode.TabIndex = 4;
            lblClauseCode.Text = "Clause Code:";
            lblClauseCode.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtClauseCode
            //
            txtClauseCode.Dock = DockStyle.Fill;
            txtClauseCode.Font = new Font("Segoe UI", 9F);
            txtClauseCode.Name = "txtClauseCode";
            txtClauseCode.ReadOnly = true;
            txtClauseCode.TabIndex = 5;
            //
            // lblClauseSection
            //
            lblClauseSection.Dock = DockStyle.Fill;
            lblClauseSection.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClauseSection.Name = "lblClauseSection";
            lblClauseSection.Padding = new Padding(4, 0, 0, 0);
            lblClauseSection.TabIndex = 6;
            lblClauseSection.Text = "Section:";
            lblClauseSection.TextAlign = ContentAlignment.MiddleLeft;
            //
            // cboClauseSection
            //
            cboClauseSection.Dock = DockStyle.Fill;
            cboClauseSection.DropDownStyle = ComboBoxStyle.DropDownList;
            cboClauseSection.Enabled = false;
            cboClauseSection.Font = new Font("Segoe UI", 9F);
            cboClauseSection.Name = "cboClauseSection";
            cboClauseSection.TabIndex = 7;
            //
            // lblClauseHeading
            //
            lblClauseHeading.Dock = DockStyle.Fill;
            lblClauseHeading.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClauseHeading.Name = "lblClauseHeading";
            lblClauseHeading.Padding = new Padding(4, 0, 0, 0);
            lblClauseHeading.TabIndex = 8;
            lblClauseHeading.Text = "Heading:";
            lblClauseHeading.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtClauseHeading
            //
            detailLayout.SetColumnSpan(txtClauseHeading, 3);
            txtClauseHeading.Dock = DockStyle.Fill;
            txtClauseHeading.Font = new Font("Segoe UI", 9F);
            txtClauseHeading.Name = "txtClauseHeading";
            txtClauseHeading.TabIndex = 9;
            //
            // lblDescription
            //
            lblDescription.Dock = DockStyle.Fill;
            lblDescription.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDescription.Name = "lblDescription";
            lblDescription.Padding = new Padding(4, 4, 0, 0);
            lblDescription.TabIndex = 10;
            lblDescription.Text = "Description:";
            //
            // txtClauseDescription
            //
            detailLayout.SetColumnSpan(txtClauseDescription, 3);
            txtClauseDescription.Dock = DockStyle.Fill;
            txtClauseDescription.Font = new Font("Segoe UI", 9F);
            txtClauseDescription.Multiline = true;
            txtClauseDescription.Name = "txtClauseDescription";
            txtClauseDescription.ScrollBars = ScrollBars.Vertical;
            txtClauseDescription.TabIndex = 11;
            //
            // lblParameters
            //
            lblParameters.Dock = DockStyle.Fill;
            lblParameters.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblParameters.Name = "lblParameters";
            lblParameters.Padding = new Padding(4, 4, 0, 0);
            lblParameters.TabIndex = 12;
            lblParameters.Text = "Parameters:";
            //
            // dgvParameters
            //
            dgvParameters.AllowUserToAddRows = false;
            dgvParameters.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParameters.Columns.AddRange(new DataGridViewColumn[] { colParameterName, colParameterValue, colParameterUnit, colParameterDescription });
            detailLayout.SetColumnSpan(dgvParameters, 3);
            dgvParameters.Dock = DockStyle.Fill;
            dgvParameters.Font = new Font("Segoe UI", 9F);
            dgvParameters.MultiSelect = false;
            dgvParameters.Name = "dgvParameters";
            dgvParameters.RowHeadersWidth = 35;
            dgvParameters.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParameters.TabIndex = 13;
            dgvParameters.CellEndEdit += dgvParameters_CellEndEdit;
            dgvParameters.SelectionChanged += dgvParameters_SelectionChanged;
            //
            // colParameterName
            //
            colParameterName.HeaderText = "Parameter";
            colParameterName.MinimumWidth = 6;
            colParameterName.Name = "colParameterName";
            colParameterName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterName.Width = 190;
            //
            // colParameterValue
            //
            colParameterValue.HeaderText = "Value";
            colParameterValue.MinimumWidth = 6;
            colParameterValue.Name = "colParameterValue";
            colParameterValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterValue.Width = 110;
            //
            // colParameterUnit
            //
            colParameterUnit.HeaderText = "Unit";
            colParameterUnit.MinimumWidth = 6;
            colParameterUnit.Name = "colParameterUnit";
            colParameterUnit.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterUnit.Width = 80;
            //
            // colParameterDescription
            //
            colParameterDescription.HeaderText = "Description";
            colParameterDescription.MinimumWidth = 6;
            colParameterDescription.Name = "colParameterDescription";
            colParameterDescription.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterDescription.Width = 360;
            //
            // clauseButtonPanel
            //
            detailLayout.SetColumnSpan(clauseButtonPanel, 4);
            clauseButtonPanel.Controls.Add(btnAddClause);
            clauseButtonPanel.Controls.Add(btnAddSubClause);
            clauseButtonPanel.Controls.Add(btnDuplicateClause);
            clauseButtonPanel.Controls.Add(btnDeleteClause);
            clauseButtonPanel.Controls.Add(btnMoveUp);
            clauseButtonPanel.Controls.Add(btnMoveDown);
            clauseButtonPanel.Controls.Add(btnOpenDiagram);
            clauseButtonPanel.Controls.Add(btnAddParameter);
            clauseButtonPanel.Controls.Add(btnDeleteParameter);
            clauseButtonPanel.Dock = DockStyle.Fill;
            clauseButtonPanel.Name = "clauseButtonPanel";
            clauseButtonPanel.TabIndex = 14;
            clauseButtonPanel.WrapContents = false;
            //
            // btnAddClause
            //
            btnAddClause.AutoSize = true;
            btnAddClause.Name = "btnAddClause";
            btnAddClause.Size = new Size(94, 30);
            btnAddClause.TabIndex = 0;
            btnAddClause.Text = "Add Clause";
            btnAddClause.UseVisualStyleBackColor = true;
            btnAddClause.Click += btnAddClause_Click;
            //
            // btnAddSubClause
            //
            btnAddSubClause.AutoSize = true;
            btnAddSubClause.Name = "btnAddSubClause";
            btnAddSubClause.Size = new Size(125, 30);
            btnAddSubClause.TabIndex = 1;
            btnAddSubClause.Text = "Add Sub-Clause";
            btnAddSubClause.UseVisualStyleBackColor = true;
            btnAddSubClause.Click += btnAddSubClause_Click;
            //
            // btnDuplicateClause
            //
            btnDuplicateClause.AutoSize = true;
            btnDuplicateClause.Name = "btnDuplicateClause";
            btnDuplicateClause.Size = new Size(83, 30);
            btnDuplicateClause.TabIndex = 2;
            btnDuplicateClause.Text = "Duplicate";
            btnDuplicateClause.UseVisualStyleBackColor = true;
            btnDuplicateClause.Click += btnDuplicateClause_Click;
            //
            // btnDeleteClause
            //
            btnDeleteClause.AutoSize = true;
            btnDeleteClause.Name = "btnDeleteClause";
            btnDeleteClause.Size = new Size(75, 30);
            btnDeleteClause.TabIndex = 3;
            btnDeleteClause.Text = "Delete";
            btnDeleteClause.UseVisualStyleBackColor = true;
            btnDeleteClause.Click += btnDeleteClause_Click;
            //
            // btnMoveUp
            //
            btnMoveUp.AutoSize = true;
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(79, 30);
            btnMoveUp.TabIndex = 4;
            btnMoveUp.Text = "Move Up";
            btnMoveUp.UseVisualStyleBackColor = true;
            btnMoveUp.Click += btnMoveUp_Click;
            //
            // btnMoveDown
            //
            btnMoveDown.AutoSize = true;
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(99, 30);
            btnMoveDown.TabIndex = 5;
            btnMoveDown.Text = "Move Down";
            btnMoveDown.UseVisualStyleBackColor = true;
            btnMoveDown.Click += btnMoveDown_Click;
            //
            // btnOpenDiagram
            //
            btnOpenDiagram.AutoSize = true;
            btnOpenDiagram.Name = "btnOpenDiagram";
            btnOpenDiagram.Size = new Size(86, 30);
            btnOpenDiagram.TabIndex = 6;
            btnOpenDiagram.Text = "Diagram...";
            btnOpenDiagram.UseVisualStyleBackColor = true;
            btnOpenDiagram.Click += btnOpenDiagram_Click;
            //
            // btnAddParameter
            //
            btnAddParameter.AutoSize = true;
            btnAddParameter.Name = "btnAddParameter";
            btnAddParameter.Size = new Size(118, 30);
            btnAddParameter.TabIndex = 7;
            btnAddParameter.Text = "Add Parameter";
            btnAddParameter.UseVisualStyleBackColor = true;
            btnAddParameter.Click += btnAddParameter_Click;
            //
            // btnDeleteParameter
            //
            btnDeleteParameter.AutoSize = true;
            btnDeleteParameter.Name = "btnDeleteParameter";
            btnDeleteParameter.Size = new Size(134, 30);
            btnDeleteParameter.TabIndex = 8;
            btnDeleteParameter.Text = "Delete Parameter";
            btnDeleteParameter.UseVisualStyleBackColor = true;
            btnDeleteParameter.Click += btnDeleteParameter_Click;
            //
            // lstValidation
            //
            detailLayout.SetColumnSpan(lstValidation, 4);
            lstValidation.Dock = DockStyle.Fill;
            lstValidation.Font = new Font("Segoe UI", 9F);
            lstValidation.Name = "lstValidation";
            lstValidation.TabIndex = 15;
            //
            // frmPolicyManagerDashboard
            //
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1553, 700);
            Controls.Add(mainSplit);
            Controls.Add(headerPanel);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1050, 680);
            Name = "frmPolicyManagerDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Policy Editor";
            Load += frmPolicyManagerDashboard_Load;
            headerPanel.ResumeLayout(false);
            mainSplit.Panel1.ResumeLayout(false);
            mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
            mainSplit.ResumeLayout(false);
            clauseListPanel.ResumeLayout(false);
            sectionClauseSplit.Panel1.ResumeLayout(false);
            sectionClauseSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)sectionClauseSplit).EndInit();
            sectionClauseSplit.ResumeLayout(false);
            sectionPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSections).EndInit();
            sectionButtonPanel.ResumeLayout(false);
            sectionButtonPanel.PerformLayout();
            clausePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvClauses).EndInit();
            clauseContextMenu.ResumeLayout(false);
            detailLayout.ResumeLayout(false);
            detailLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParameters).EndInit();
            clauseButtonPanel.ResumeLayout(false);
            clauseButtonPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
