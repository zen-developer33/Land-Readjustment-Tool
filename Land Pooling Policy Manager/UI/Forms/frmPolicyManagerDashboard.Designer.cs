namespace Land_Pooling_Policy_Manager.UI.Forms
{
    partial class frmPolicyManagerDashboard
    {
        private System.ComponentModel.IContainer components = null;
        private Panel headerPanel;
        private Button btnManagePolicies;
        private ComboBox cboCurrentPolicy;
        private Button btnSaveDraft;
        private Button btnApprove;
        private Button btnLockUnlock;
        private Button btnExport;
        private Label lblStatus;
        private SplitContainer mainSplit;
        private SplitContainer navigationSplit;
        private Panel sectionPanel;
        private Label lblSections;
        private DataGridView dgvSections;
        private DataGridViewTextBoxColumn colSectionNo;
        private DataGridViewTextBoxColumn colSectionTitle;
        private FlowLayoutPanel sectionButtonPanel;
        private Button btnAddSection;
        private Button btnRenameSection;
        private Button btnDeleteSection;
        private Panel clauseListPanel;
        private Label lblClauses;
        private DataGridView dgvClauses;
        private DataGridViewTextBoxColumn colClauseCode;
        private DataGridViewTextBoxColumn colClauseHeading;
        private FlowLayoutPanel clauseListButtonPanel;
        private Button btnAddClause;
        private Button btnAddSubClause;
        private Button btnDuplicateClause;
        private Button btnDeleteClause;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnOpenDiagram;
        private ContextMenuStrip clauseContextMenu;
        private ToolStripMenuItem menuAddClause;
        private ToolStripMenuItem menuAddSubClause;
        private ToolStripMenuItem menuDuplicateClause;
        private ToolStripMenuItem menuDeleteClause;
        private TableLayoutPanel detailLayout;
        private Label lblPolicyCodeCaption;
        private Label lblPolicyNameCaption;
        private Label lblClauseCodeCaption;
        private Label lblClauseSectionCaption;
        private Label lblClauseHeadingCaption;
        private Label lblClauseDescriptionCaption;
        private Label lblParametersCaption;
        private TextBox txtPolicyCode;
        private TextBox txtPolicyName;
        private TextBox txtClauseCode;
        private TextBox txtClauseSection;
        private TextBox txtClauseHeading;
        private TextBox txtClauseDescription;
        private DataGridView dgvParameters;
        private DataGridViewTextBoxColumn colParameterName;
        private DataGridViewTextBoxColumn colParameterValue;
        private DataGridViewTextBoxColumn colParameterUnit;
        private DataGridViewTextBoxColumn colParameterDescription;
        private FlowLayoutPanel parameterButtonPanel;
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
            cboCurrentPolicy = new ComboBox();
            btnSaveDraft = new Button();
            btnApprove = new Button();
            btnLockUnlock = new Button();
            btnExport = new Button();
            lblStatus = new Label();
            mainSplit = new SplitContainer();
            navigationSplit = new SplitContainer();
            sectionPanel = new Panel();
            dgvSections = new DataGridView();
            colSectionNo = new DataGridViewTextBoxColumn();
            colSectionTitle = new DataGridViewTextBoxColumn();
            sectionButtonPanel = new FlowLayoutPanel();
            btnAddSection = new Button();
            btnRenameSection = new Button();
            btnDeleteSection = new Button();
            lblSections = new Label();
            clauseListPanel = new Panel();
            dgvClauses = new DataGridView();
            colClauseCode = new DataGridViewTextBoxColumn();
            colClauseHeading = new DataGridViewTextBoxColumn();
            clauseContextMenu = new ContextMenuStrip(components);
            menuAddClause = new ToolStripMenuItem();
            menuAddSubClause = new ToolStripMenuItem();
            menuDuplicateClause = new ToolStripMenuItem();
            menuDeleteClause = new ToolStripMenuItem();
            clauseListButtonPanel = new FlowLayoutPanel();
            btnAddClause = new Button();
            btnAddSubClause = new Button();
            btnDuplicateClause = new Button();
            btnDeleteClause = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            btnOpenDiagram = new Button();
            lblClauses = new Label();
            detailLayout = new TableLayoutPanel();
            lblPolicyCodeCaption = new Label();
            txtPolicyCode = new TextBox();
            lblPolicyNameCaption = new Label();
            txtPolicyName = new TextBox();
            lblClauseCodeCaption = new Label();
            txtClauseCode = new TextBox();
            lblClauseSectionCaption = new Label();
            txtClauseSection = new TextBox();
            lblClauseHeadingCaption = new Label();
            txtClauseHeading = new TextBox();
            lblClauseDescriptionCaption = new Label();
            txtClauseDescription = new TextBox();
            lblParametersCaption = new Label();
            dgvParameters = new DataGridView();
            colParameterName = new DataGridViewTextBoxColumn();
            colParameterValue = new DataGridViewTextBoxColumn();
            colParameterUnit = new DataGridViewTextBoxColumn();
            colParameterDescription = new DataGridViewTextBoxColumn();
            parameterButtonPanel = new FlowLayoutPanel();
            btnAddParameter = new Button();
            btnDeleteParameter = new Button();
            lstValidation = new ListBox();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
            mainSplit.Panel1.SuspendLayout();
            mainSplit.Panel2.SuspendLayout();
            mainSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)navigationSplit).BeginInit();
            navigationSplit.Panel1.SuspendLayout();
            navigationSplit.Panel2.SuspendLayout();
            navigationSplit.SuspendLayout();
            sectionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSections).BeginInit();
            sectionButtonPanel.SuspendLayout();
            clauseListPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClauses).BeginInit();
            clauseContextMenu.SuspendLayout();
            clauseListButtonPanel.SuspendLayout();
            detailLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParameters).BeginInit();
            parameterButtonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BorderStyle = BorderStyle.FixedSingle;
            headerPanel.Controls.Add(btnManagePolicies);
            headerPanel.Controls.Add(cboCurrentPolicy);
            headerPanel.Controls.Add(btnSaveDraft);
            headerPanel.Controls.Add(btnApprove);
            headerPanel.Controls.Add(btnLockUnlock);
            headerPanel.Controls.Add(btnExport);
            headerPanel.Controls.Add(lblStatus);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(6, 5, 6, 5);
            headerPanel.Size = new Size(1380, 48);
            headerPanel.TabIndex = 0;
            // 
            // btnManagePolicies
            // 
            btnManagePolicies.Location = new Point(6, 6);
            btnManagePolicies.Name = "btnManagePolicies";
            btnManagePolicies.Size = new Size(205, 32);
            btnManagePolicies.TabIndex = 0;
            btnManagePolicies.Text = "Select / Manage Policy...";
            btnManagePolicies.UseVisualStyleBackColor = true;
            btnManagePolicies.Click += btnManagePolicies_Click;
            // 
            // cboCurrentPolicy
            // 
            cboCurrentPolicy.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCurrentPolicy.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cboCurrentPolicy.FormattingEnabled = true;
            cboCurrentPolicy.Location = new Point(217, 6);
            cboCurrentPolicy.Name = "cboCurrentPolicy";
            cboCurrentPolicy.Size = new Size(537, 28);
            cboCurrentPolicy.TabIndex = 1;
            cboCurrentPolicy.SelectedIndexChanged += cboCurrentPolicy_SelectedIndexChanged;
            // 
            // btnSaveDraft
            // 
            btnSaveDraft.Location = new Point(760, 6);
            btnSaveDraft.Name = "btnSaveDraft";
            btnSaveDraft.Size = new Size(96, 32);
            btnSaveDraft.TabIndex = 2;
            btnSaveDraft.Text = "Save Draft";
            btnSaveDraft.UseVisualStyleBackColor = true;
            btnSaveDraft.Click += btnSaveDraft_Click;
            // 
            // btnApprove
            // 
            btnApprove.Location = new Point(862, 6);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(86, 32);
            btnApprove.TabIndex = 3;
            btnApprove.Text = "Approve";
            btnApprove.UseVisualStyleBackColor = true;
            btnApprove.Click += btnApprove_Click;
            // 
            // btnLockUnlock
            // 
            btnLockUnlock.Location = new Point(954, 6);
            btnLockUnlock.Name = "btnLockUnlock";
            btnLockUnlock.Size = new Size(116, 32);
            btnLockUnlock.TabIndex = 4;
            btnLockUnlock.Text = "Lock Editing";
            btnLockUnlock.UseVisualStyleBackColor = true;
            btnLockUnlock.Click += btnLockUnlock_Click;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(1076, 6);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(82, 32);
            btnExport.TabIndex = 5;
            btnExport.Text = "Export";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.Location = new Point(1164, 6);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(208, 32);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "Draft";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // mainSplit
            // 
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.Location = new Point(0, 48);
            mainSplit.Name = "mainSplit";
            // 
            // mainSplit.Panel1
            // 
            mainSplit.Panel1.Controls.Add(navigationSplit);
            // 
            // mainSplit.Panel2
            // 
            mainSplit.Panel2.Controls.Add(detailLayout);
            mainSplit.Size = new Size(1380, 682);
            mainSplit.SplitterDistance = 610;
            mainSplit.TabIndex = 1;
            // 
            // navigationSplit
            // 
            navigationSplit.Dock = DockStyle.Fill;
            navigationSplit.FixedPanel = FixedPanel.Panel1;
            navigationSplit.Location = new Point(0, 0);
            navigationSplit.Name = "navigationSplit";
            navigationSplit.Orientation = Orientation.Horizontal;
            // 
            // navigationSplit.Panel1
            // 
            navigationSplit.Panel1.Controls.Add(sectionPanel);
            navigationSplit.Panel1MinSize = 120;
            // 
            // navigationSplit.Panel2
            // 
            navigationSplit.Panel2.Controls.Add(clauseListPanel);
            navigationSplit.Panel2MinSize = 200;
            navigationSplit.Size = new Size(610, 682);
            navigationSplit.SplitterDistance = 220;
            navigationSplit.SplitterWidth = 6;
            navigationSplit.TabIndex = 0;
            // 
            // sectionPanel
            // 
            sectionPanel.Controls.Add(dgvSections);
            sectionPanel.Controls.Add(sectionButtonPanel);
            sectionPanel.Controls.Add(lblSections);
            sectionPanel.Dock = DockStyle.Fill;
            sectionPanel.Location = new Point(0, 0);
            sectionPanel.Name = "sectionPanel";
            sectionPanel.Padding = new Padding(6);
            sectionPanel.Size = new Size(610, 220);
            sectionPanel.TabIndex = 0;
            // 
            // dgvSections
            // 
            dgvSections.AllowUserToAddRows = false;
            dgvSections.AllowUserToDeleteRows = false;
            dgvSections.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSections.Columns.AddRange(new DataGridViewColumn[] { colSectionNo, colSectionTitle });
            dgvSections.Dock = DockStyle.Fill;
            dgvSections.Location = new Point(6, 34);
            dgvSections.MultiSelect = false;
            dgvSections.Name = "dgvSections";
            dgvSections.ReadOnly = true;
            dgvSections.RowHeadersWidth = 28;
            dgvSections.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSections.Size = new Size(598, 144);
            dgvSections.TabIndex = 1;
            dgvSections.SelectionChanged += dgvSections_SelectionChanged;
            // 
            // colSectionNo
            // 
            colSectionNo.HeaderText = "No.";
            colSectionNo.MinimumWidth = 6;
            colSectionNo.Name = "colSectionNo";
            colSectionNo.ReadOnly = true;
            colSectionNo.SortMode = DataGridViewColumnSortMode.NotSortable;
            colSectionNo.Width = 48;
            // 
            // colSectionTitle
            // 
            colSectionTitle.HeaderText = "Section";
            colSectionTitle.MinimumWidth = 6;
            colSectionTitle.Name = "colSectionTitle";
            colSectionTitle.ReadOnly = true;
            colSectionTitle.SortMode = DataGridViewColumnSortMode.NotSortable;
            colSectionTitle.Width = 155;
            // 
            // sectionButtonPanel
            // 
            sectionButtonPanel.Controls.Add(btnAddSection);
            sectionButtonPanel.Controls.Add(btnRenameSection);
            sectionButtonPanel.Controls.Add(btnDeleteSection);
            sectionButtonPanel.Dock = DockStyle.Bottom;
            sectionButtonPanel.Location = new Point(6, 178);
            sectionButtonPanel.Name = "sectionButtonPanel";
            sectionButtonPanel.Size = new Size(598, 36);
            sectionButtonPanel.TabIndex = 2;
            sectionButtonPanel.WrapContents = false;
            // 
            // btnAddSection
            // 
            btnAddSection.AutoSize = true;
            btnAddSection.Location = new Point(3, 3);
            btnAddSection.Name = "btnAddSection";
            btnAddSection.Size = new Size(48, 30);
            btnAddSection.TabIndex = 0;
            btnAddSection.Text = "Add";
            btnAddSection.Click += btnAddSection_Click;
            // 
            // btnRenameSection
            // 
            btnRenameSection.AutoSize = true;
            btnRenameSection.Location = new Point(57, 3);
            btnRenameSection.Name = "btnRenameSection";
            btnRenameSection.Size = new Size(75, 30);
            btnRenameSection.TabIndex = 1;
            btnRenameSection.Text = "Rename";
            btnRenameSection.Click += btnRenameSection_Click;
            // 
            // btnDeleteSection
            // 
            btnDeleteSection.AutoSize = true;
            btnDeleteSection.Location = new Point(138, 3);
            btnDeleteSection.Name = "btnDeleteSection";
            btnDeleteSection.Size = new Size(64, 30);
            btnDeleteSection.TabIndex = 2;
            btnDeleteSection.Text = "Delete";
            btnDeleteSection.Click += btnDeleteSection_Click;
            // 
            // lblSections
            // 
            lblSections.Dock = DockStyle.Top;
            lblSections.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSections.Location = new Point(6, 6);
            lblSections.Name = "lblSections";
            lblSections.Size = new Size(598, 28);
            lblSections.TabIndex = 0;
            lblSections.Text = "Sections";
            lblSections.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // clauseListPanel
            // 
            clauseListPanel.Controls.Add(dgvClauses);
            clauseListPanel.Controls.Add(clauseListButtonPanel);
            clauseListPanel.Controls.Add(lblClauses);
            clauseListPanel.Dock = DockStyle.Fill;
            clauseListPanel.Location = new Point(0, 0);
            clauseListPanel.Name = "clauseListPanel";
            clauseListPanel.Padding = new Padding(6);
            clauseListPanel.Size = new Size(610, 456);
            clauseListPanel.TabIndex = 0;
            // 
            // dgvClauses
            // 
            dgvClauses.AllowUserToAddRows = false;
            dgvClauses.AllowUserToDeleteRows = false;
            dgvClauses.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClauses.Columns.AddRange(new DataGridViewColumn[] { colClauseCode, colClauseHeading });
            dgvClauses.ContextMenuStrip = clauseContextMenu;
            dgvClauses.Dock = DockStyle.Fill;
            dgvClauses.Location = new Point(6, 34);
            dgvClauses.MultiSelect = false;
            dgvClauses.Name = "dgvClauses";
            dgvClauses.ReadOnly = true;
            dgvClauses.RowHeadersWidth = 30;
            dgvClauses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClauses.Size = new Size(598, 380);
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
            colClauseCode.Width = 82;
            // 
            // colClauseHeading
            // 
            colClauseHeading.HeaderText = "Clause / Sub-Clause";
            colClauseHeading.MinimumWidth = 6;
            colClauseHeading.Name = "colClauseHeading";
            colClauseHeading.ReadOnly = true;
            colClauseHeading.SortMode = DataGridViewColumnSortMode.NotSortable;
            colClauseHeading.Width = 260;
            // 
            // clauseContextMenu
            // 
            clauseContextMenu.ImageScalingSize = new Size(20, 20);
            clauseContextMenu.Items.AddRange(new ToolStripItem[] { menuAddClause, menuAddSubClause, menuDuplicateClause, menuDeleteClause });
            clauseContextMenu.Name = "clauseContextMenu";
            clauseContextMenu.Size = new Size(190, 100);
            // 
            // menuAddClause
            // 
            menuAddClause.Name = "menuAddClause";
            menuAddClause.Size = new Size(189, 24);
            menuAddClause.Text = "Add Clause";
            menuAddClause.Click += menuAddClause_Click;
            // 
            // menuAddSubClause
            // 
            menuAddSubClause.Name = "menuAddSubClause";
            menuAddSubClause.Size = new Size(189, 24);
            menuAddSubClause.Text = "Add Sub-Clause";
            menuAddSubClause.Click += menuAddSubClause_Click;
            // 
            // menuDuplicateClause
            // 
            menuDuplicateClause.Name = "menuDuplicateClause";
            menuDuplicateClause.Size = new Size(189, 24);
            menuDuplicateClause.Text = "Duplicate Clause";
            menuDuplicateClause.Click += menuDuplicateClause_Click;
            // 
            // menuDeleteClause
            // 
            menuDeleteClause.Name = "menuDeleteClause";
            menuDeleteClause.Size = new Size(189, 24);
            menuDeleteClause.Text = "Delete Clause";
            menuDeleteClause.Click += menuDeleteClause_Click;
            // 
            // clauseListButtonPanel
            // 
            clauseListButtonPanel.Controls.Add(btnAddClause);
            clauseListButtonPanel.Controls.Add(btnAddSubClause);
            clauseListButtonPanel.Controls.Add(btnDuplicateClause);
            clauseListButtonPanel.Controls.Add(btnDeleteClause);
            clauseListButtonPanel.Controls.Add(btnMoveUp);
            clauseListButtonPanel.Controls.Add(btnMoveDown);
            clauseListButtonPanel.Controls.Add(btnOpenDiagram);
            clauseListButtonPanel.Dock = DockStyle.Bottom;
            clauseListButtonPanel.Location = new Point(6, 414);
            clauseListButtonPanel.Name = "clauseListButtonPanel";
            clauseListButtonPanel.Size = new Size(598, 36);
            clauseListButtonPanel.TabIndex = 2;
            clauseListButtonPanel.WrapContents = false;
            // 
            // btnAddClause
            // 
            btnAddClause.AutoSize = true;
            btnAddClause.Location = new Point(3, 3);
            btnAddClause.Name = "btnAddClause";
            btnAddClause.Size = new Size(94, 30);
            btnAddClause.TabIndex = 0;
            btnAddClause.Text = "Add Clause";
            btnAddClause.Click += btnAddClause_Click;
            // 
            // btnAddSubClause
            // 
            btnAddSubClause.AutoSize = true;
            btnAddSubClause.Location = new Point(103, 3);
            btnAddSubClause.Name = "btnAddSubClause";
            btnAddSubClause.Size = new Size(125, 30);
            btnAddSubClause.TabIndex = 1;
            btnAddSubClause.Text = "Add Sub-Clause";
            btnAddSubClause.Click += btnAddSubClause_Click;
            // 
            // btnDuplicateClause
            // 
            btnDuplicateClause.AutoSize = true;
            btnDuplicateClause.Location = new Point(234, 3);
            btnDuplicateClause.Name = "btnDuplicateClause";
            btnDuplicateClause.Size = new Size(53, 30);
            btnDuplicateClause.TabIndex = 2;
            btnDuplicateClause.Text = "Copy";
            btnDuplicateClause.Click += btnDuplicateClause_Click;
            // 
            // btnDeleteClause
            // 
            btnDeleteClause.AutoSize = true;
            btnDeleteClause.Location = new Point(293, 3);
            btnDeleteClause.Name = "btnDeleteClause";
            btnDeleteClause.Size = new Size(42, 30);
            btnDeleteClause.TabIndex = 3;
            btnDeleteClause.Text = "Del";
            btnDeleteClause.Click += btnDeleteClause_Click;
            // 
            // btnMoveUp
            // 
            btnMoveUp.AutoSize = true;
            btnMoveUp.Location = new Point(341, 3);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(35, 30);
            btnMoveUp.TabIndex = 4;
            btnMoveUp.Text = "↑";
            btnMoveUp.Click += btnMoveUp_Click;
            // 
            // btnMoveDown
            // 
            btnMoveDown.AutoSize = true;
            btnMoveDown.Location = new Point(382, 3);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(35, 30);
            btnMoveDown.TabIndex = 5;
            btnMoveDown.Text = "↓";
            btnMoveDown.Click += btnMoveDown_Click;
            // 
            // btnOpenDiagram
            // 
            btnOpenDiagram.AutoSize = true;
            btnOpenDiagram.Location = new Point(423, 3);
            btnOpenDiagram.Name = "btnOpenDiagram";
            btnOpenDiagram.Size = new Size(149, 30);
            btnOpenDiagram.TabIndex = 6;
            btnOpenDiagram.Text = "Illustration Image";
            btnOpenDiagram.Click += btnOpenDiagram_Click;
            // 
            // lblClauses
            // 
            lblClauses.Dock = DockStyle.Top;
            lblClauses.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClauses.Location = new Point(6, 6);
            lblClauses.Name = "lblClauses";
            lblClauses.Size = new Size(598, 28);
            lblClauses.TabIndex = 0;
            lblClauses.Text = "Clauses";
            lblClauses.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // detailLayout
            // 
            detailLayout.ColumnCount = 4;
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            detailLayout.Controls.Add(lblPolicyCodeCaption, 0, 0);
            detailLayout.Controls.Add(txtPolicyCode, 1, 0);
            detailLayout.Controls.Add(lblPolicyNameCaption, 2, 0);
            detailLayout.Controls.Add(txtPolicyName, 3, 0);
            detailLayout.Controls.Add(lblClauseCodeCaption, 0, 1);
            detailLayout.Controls.Add(txtClauseCode, 1, 1);
            detailLayout.Controls.Add(lblClauseSectionCaption, 2, 1);
            detailLayout.Controls.Add(txtClauseSection, 3, 1);
            detailLayout.Controls.Add(lblClauseHeadingCaption, 0, 2);
            detailLayout.Controls.Add(txtClauseHeading, 1, 2);
            detailLayout.Controls.Add(lblClauseDescriptionCaption, 0, 3);
            detailLayout.Controls.Add(txtClauseDescription, 1, 3);
            detailLayout.Controls.Add(lblParametersCaption, 0, 4);
            detailLayout.Controls.Add(dgvParameters, 1, 4);
            detailLayout.Controls.Add(parameterButtonPanel, 0, 5);
            detailLayout.Controls.Add(lstValidation, 0, 6);
            detailLayout.Dock = DockStyle.Fill;
            detailLayout.Location = new Point(0, 0);
            detailLayout.Name = "detailLayout";
            detailLayout.Padding = new Padding(8);
            detailLayout.RowCount = 7;
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            detailLayout.Size = new Size(766, 682);
            detailLayout.TabIndex = 0;
            // 
            // lblPolicyCodeCaption
            // 
            lblPolicyCodeCaption.Dock = DockStyle.Fill;
            lblPolicyCodeCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPolicyCodeCaption.Location = new Point(11, 8);
            lblPolicyCodeCaption.Name = "lblPolicyCodeCaption";
            lblPolicyCodeCaption.Size = new Size(139, 34);
            lblPolicyCodeCaption.TabIndex = 0;
            lblPolicyCodeCaption.Text = "Policy Code:";
            lblPolicyCodeCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtPolicyCode
            // 
            txtPolicyCode.BackColor = Color.White;
            txtPolicyCode.Dock = DockStyle.Fill;
            txtPolicyCode.Location = new Point(156, 11);
            txtPolicyCode.Name = "txtPolicyCode";
            txtPolicyCode.ReadOnly = true;
            txtPolicyCode.Size = new Size(231, 27);
            txtPolicyCode.TabIndex = 1;
            // 
            // lblPolicyNameCaption
            // 
            lblPolicyNameCaption.Dock = DockStyle.Fill;
            lblPolicyNameCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPolicyNameCaption.Location = new Point(393, 8);
            lblPolicyNameCaption.Name = "lblPolicyNameCaption";
            lblPolicyNameCaption.Size = new Size(124, 34);
            lblPolicyNameCaption.TabIndex = 2;
            lblPolicyNameCaption.Text = "Policy Name:";
            lblPolicyNameCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtPolicyName
            // 
            txtPolicyName.BackColor = Color.White;
            txtPolicyName.Dock = DockStyle.Fill;
            txtPolicyName.Location = new Point(523, 11);
            txtPolicyName.Name = "txtPolicyName";
            txtPolicyName.ReadOnly = true;
            txtPolicyName.Size = new Size(232, 27);
            txtPolicyName.TabIndex = 3;
            // 
            // lblClauseCodeCaption
            // 
            lblClauseCodeCaption.Dock = DockStyle.Fill;
            lblClauseCodeCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClauseCodeCaption.Location = new Point(11, 42);
            lblClauseCodeCaption.Name = "lblClauseCodeCaption";
            lblClauseCodeCaption.Size = new Size(139, 34);
            lblClauseCodeCaption.TabIndex = 4;
            lblClauseCodeCaption.Text = "Clause Code:";
            lblClauseCodeCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtClauseCode
            // 
            txtClauseCode.BackColor = Color.White;
            txtClauseCode.Dock = DockStyle.Fill;
            txtClauseCode.Location = new Point(156, 45);
            txtClauseCode.Name = "txtClauseCode";
            txtClauseCode.ReadOnly = true;
            txtClauseCode.Size = new Size(231, 27);
            txtClauseCode.TabIndex = 5;
            // 
            // lblClauseSectionCaption
            // 
            lblClauseSectionCaption.Dock = DockStyle.Fill;
            lblClauseSectionCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClauseSectionCaption.Location = new Point(393, 42);
            lblClauseSectionCaption.Name = "lblClauseSectionCaption";
            lblClauseSectionCaption.Size = new Size(124, 34);
            lblClauseSectionCaption.TabIndex = 6;
            lblClauseSectionCaption.Text = "Section:";
            lblClauseSectionCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtClauseSection
            // 
            txtClauseSection.BackColor = Color.White;
            txtClauseSection.Dock = DockStyle.Fill;
            txtClauseSection.Location = new Point(523, 45);
            txtClauseSection.Name = "txtClauseSection";
            txtClauseSection.ReadOnly = true;
            txtClauseSection.Size = new Size(232, 27);
            txtClauseSection.TabIndex = 7;
            // 
            // lblClauseHeadingCaption
            // 
            lblClauseHeadingCaption.Dock = DockStyle.Fill;
            lblClauseHeadingCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClauseHeadingCaption.Location = new Point(11, 76);
            lblClauseHeadingCaption.Name = "lblClauseHeadingCaption";
            lblClauseHeadingCaption.Size = new Size(139, 34);
            lblClauseHeadingCaption.TabIndex = 8;
            lblClauseHeadingCaption.Text = "Heading:";
            lblClauseHeadingCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtClauseHeading
            // 
            txtClauseHeading.BackColor = Color.White;
            detailLayout.SetColumnSpan(txtClauseHeading, 3);
            txtClauseHeading.Dock = DockStyle.Fill;
            txtClauseHeading.Location = new Point(156, 79);
            txtClauseHeading.Name = "txtClauseHeading";
            txtClauseHeading.ReadOnly = true;
            txtClauseHeading.Size = new Size(599, 27);
            txtClauseHeading.TabIndex = 9;
            // 
            // lblClauseDescriptionCaption
            // 
            lblClauseDescriptionCaption.Dock = DockStyle.Fill;
            lblClauseDescriptionCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClauseDescriptionCaption.Location = new Point(11, 110);
            lblClauseDescriptionCaption.Name = "lblClauseDescriptionCaption";
            lblClauseDescriptionCaption.Size = new Size(139, 238);
            lblClauseDescriptionCaption.TabIndex = 10;
            lblClauseDescriptionCaption.Text = "Description:";
            // 
            // txtClauseDescription
            // 
            txtClauseDescription.BackColor = Color.White;
            detailLayout.SetColumnSpan(txtClauseDescription, 3);
            txtClauseDescription.Dock = DockStyle.Fill;
            txtClauseDescription.Location = new Point(156, 113);
            txtClauseDescription.Multiline = true;
            txtClauseDescription.Name = "txtClauseDescription";
            txtClauseDescription.ReadOnly = true;
            txtClauseDescription.ScrollBars = ScrollBars.Vertical;
            txtClauseDescription.Size = new Size(599, 232);
            txtClauseDescription.TabIndex = 11;
            // 
            // lblParametersCaption
            // 
            lblParametersCaption.Dock = DockStyle.Fill;
            lblParametersCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblParametersCaption.Location = new Point(11, 348);
            lblParametersCaption.Name = "lblParametersCaption";
            lblParametersCaption.Size = new Size(139, 219);
            lblParametersCaption.TabIndex = 12;
            lblParametersCaption.Text = "Parameters:";
            // 
            // dgvParameters
            // 
            dgvParameters.AllowUserToAddRows = false;
            dgvParameters.AllowUserToDeleteRows = false;
            dgvParameters.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParameters.Columns.AddRange(new DataGridViewColumn[] { colParameterName, colParameterValue, colParameterUnit, colParameterDescription });
            detailLayout.SetColumnSpan(dgvParameters, 3);
            dgvParameters.Dock = DockStyle.Fill;
            dgvParameters.Location = new Point(156, 351);
            dgvParameters.MultiSelect = false;
            dgvParameters.Name = "dgvParameters";
            dgvParameters.ReadOnly = true;
            dgvParameters.RowHeadersWidth = 35;
            dgvParameters.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParameters.Size = new Size(599, 213);
            dgvParameters.TabIndex = 13;
            dgvParameters.CellEndEdit += dgvParameters_CellEndEdit;
            dgvParameters.SelectionChanged += dgvParameters_SelectionChanged;
            // 
            // colParameterName
            // 
            colParameterName.HeaderText = "Parameter";
            colParameterName.MinimumWidth = 6;
            colParameterName.Name = "colParameterName";
            colParameterName.ReadOnly = true;
            colParameterName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterName.Width = 180;
            // 
            // colParameterValue
            // 
            colParameterValue.HeaderText = "Value";
            colParameterValue.MinimumWidth = 6;
            colParameterValue.Name = "colParameterValue";
            colParameterValue.ReadOnly = true;
            colParameterValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterValue.Width = 125;
            // 
            // colParameterUnit
            // 
            colParameterUnit.HeaderText = "Unit";
            colParameterUnit.MinimumWidth = 6;
            colParameterUnit.Name = "colParameterUnit";
            colParameterUnit.ReadOnly = true;
            colParameterUnit.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterUnit.Width = 80;
            // 
            // colParameterDescription
            // 
            colParameterDescription.HeaderText = "Description";
            colParameterDescription.MinimumWidth = 6;
            colParameterDescription.Name = "colParameterDescription";
            colParameterDescription.ReadOnly = true;
            colParameterDescription.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterDescription.Width = 300;
            // 
            // parameterButtonPanel
            // 
            detailLayout.SetColumnSpan(parameterButtonPanel, 4);
            parameterButtonPanel.Controls.Add(btnAddParameter);
            parameterButtonPanel.Controls.Add(btnDeleteParameter);
            parameterButtonPanel.Dock = DockStyle.Fill;
            parameterButtonPanel.Location = new Point(11, 570);
            parameterButtonPanel.Name = "parameterButtonPanel";
            parameterButtonPanel.Size = new Size(744, 36);
            parameterButtonPanel.TabIndex = 14;
            parameterButtonPanel.WrapContents = false;
            // 
            // btnAddParameter
            // 
            btnAddParameter.AutoSize = true;
            btnAddParameter.Location = new Point(3, 3);
            btnAddParameter.Name = "btnAddParameter";
            btnAddParameter.Size = new Size(118, 30);
            btnAddParameter.TabIndex = 0;
            btnAddParameter.Text = "Add Parameter";
            btnAddParameter.Click += btnAddParameter_Click;
            // 
            // btnDeleteParameter
            // 
            btnDeleteParameter.AutoSize = true;
            btnDeleteParameter.Location = new Point(127, 3);
            btnDeleteParameter.Name = "btnDeleteParameter";
            btnDeleteParameter.Size = new Size(134, 30);
            btnDeleteParameter.TabIndex = 1;
            btnDeleteParameter.Text = "Delete Parameter";
            btnDeleteParameter.Click += btnDeleteParameter_Click;
            // 
            // lstValidation
            // 
            detailLayout.SetColumnSpan(lstValidation, 4);
            lstValidation.Dock = DockStyle.Fill;
            lstValidation.Location = new Point(11, 612);
            lstValidation.Name = "lstValidation";
            lstValidation.Size = new Size(744, 59);
            lstValidation.TabIndex = 15;
            // 
            // frmPolicyManagerDashboard
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1380, 730);
            Controls.Add(mainSplit);
            Controls.Add(headerPanel);
            MinimumSize = new Size(500, 500);
            Name = "frmPolicyManagerDashboard";
            Text = "Policy Editor";
            Load += frmPolicyManagerDashboard_Load;
            headerPanel.ResumeLayout(false);
            mainSplit.Panel1.ResumeLayout(false);
            mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
            mainSplit.ResumeLayout(false);
            navigationSplit.Panel1.ResumeLayout(false);
            navigationSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)navigationSplit).EndInit();
            navigationSplit.ResumeLayout(false);
            sectionPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSections).EndInit();
            sectionButtonPanel.ResumeLayout(false);
            sectionButtonPanel.PerformLayout();
            clauseListPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvClauses).EndInit();
            clauseContextMenu.ResumeLayout(false);
            clauseListButtonPanel.ResumeLayout(false);
            clauseListButtonPanel.PerformLayout();
            detailLayout.ResumeLayout(false);
            detailLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParameters).EndInit();
            parameterButtonPanel.ResumeLayout(false);
            parameterButtonPanel.PerformLayout();
            ResumeLayout(false);
        }

    }
}
