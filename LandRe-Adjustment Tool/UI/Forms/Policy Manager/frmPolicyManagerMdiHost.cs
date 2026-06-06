using Land_Readjustment_Tool.Services.Policy;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmPolicyManagerMdiHost : Form
    {
        private readonly PolicyManagerService _policyManagerService;
        private readonly ProjectSession? _ownedSession;
        private readonly bool _readOnlyMode;

        public frmPolicyManagerMdiHost(
            PolicyManagerService policyManagerService,
            bool readOnlyMode,
            ProjectSession? ownedSession = null)
        {
            _policyManagerService = policyManagerService;
            _ownedSession = ownedSession;
            _readOnlyMode = readOnlyMode;
            InitializeComponent();

            Text = readOnlyMode
                ? "Contribution / Return Policy Manager - Read Only"
                : "Contribution / Return Policy Manager";
            RecordFormTheme.Apply(this);
        }

        private void frmPolicyManagerMdiHost_Load(object? sender, EventArgs e)
        {
            lblStatus.Text = "Opening policy dashboard...";
            BeginInvoke(new Action(OpenDashboard));
        }

        private void openDashboardToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            OpenDashboard();
        }

        private void closeToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void tileHorizontalToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void tileVerticalToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void cascadeToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void btnDashboard_Click(object? sender, EventArgs e)
        {
            OpenDashboard();
        }

        private void btnParameters_Click(object? sender, EventArgs e)
        {
            OpenParameters();
        }

        private void btnLookupTables_Click(object? sender, EventArgs e)
        {
            OpenLookupTables();
        }

        private void btnCornerTypes_Click(object? sender, EventArgs e)
        {
            OpenCornerTypes();
        }

        private void OpenDashboard()
        {
            foreach (Form child in MdiChildren)
            {
                if (child is frmPolicyManagerDashboard)
                {
                    child.Activate();
                    return;
                }
            }

            frmPolicyManagerDashboard form = new(_policyManagerService, _readOnlyMode)
            {
                MdiParent = this,
                WindowState = FormWindowState.Maximized
            };
            form.StatusChanged += (_, message) => lblStatus.Text = message;
            form.Show();
        }

        private void OpenParameters()
        {
            OpenOrActivate<frmPolicyParametersWindow>(
                () => new frmPolicyParametersWindow(_policyManagerService, _readOnlyMode)
                {
                    MdiParent = this,
                    WindowState = FormWindowState.Maximized
                });
        }

        private void OpenLookupTables()
        {
            OpenOrActivate<frmPolicyLookupTablesWindow>(
                () => new frmPolicyLookupTablesWindow(_policyManagerService, _readOnlyMode, cornerTypesOnly: false)
                {
                    MdiParent = this,
                    WindowState = FormWindowState.Maximized
                },
                form => form.Text == "Policy Lookup Tables");
        }

        private void OpenCornerTypes()
        {
            OpenOrActivate<frmPolicyLookupTablesWindow>(
                () => new frmPolicyLookupTablesWindow(_policyManagerService, _readOnlyMode, cornerTypesOnly: true)
                {
                    MdiParent = this,
                    WindowState = FormWindowState.Maximized
                },
                form => form.Text == "Project Corner Type Definitions");
        }

        private void OpenOrActivate<TForm>(
            Func<TForm> createForm,
            Func<TForm, bool>? predicate = null)
            where TForm : Form
        {
            foreach (Form child in MdiChildren)
            {
                if (child is TForm typedChild &&
                    (predicate == null || predicate(typedChild)))
                {
                    typedChild.Activate();
                    return;
                }
            }

            createForm().Show();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _ownedSession?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
