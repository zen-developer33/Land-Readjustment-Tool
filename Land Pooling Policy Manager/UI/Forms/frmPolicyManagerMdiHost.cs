using Land_Pooling_Policy_Manager.UI;
using Land_Pooling_Policy_Manager.Services;

namespace Land_Pooling_Policy_Manager.UI.Forms
{
    public sealed partial class frmPolicyManagerMdiHost : Form
    {
        private readonly PolicyManagerService _policyManagerService;
        private readonly bool _valueOnlyEditMode;
        private bool _readOnlyMode;

        public frmPolicyManagerMdiHost(
            PolicyManagerService policyManagerService,
            bool readOnlyMode,
            bool valueOnlyEditMode = false)
        {
            _policyManagerService = policyManagerService;
            _readOnlyMode = readOnlyMode;
            _valueOnlyEditMode = valueOnlyEditMode;
            InitializeComponent();

            Text = readOnlyMode
                ? "Contribution / Return Policy Manager - Read Only"
                : "Contribution / Return Policy Manager";
            RecordFormTheme.Apply(this);
        }

        public void SetReadOnlyMode(bool readOnlyMode)
        {
            _readOnlyMode = readOnlyMode;
            Text = readOnlyMode
                ? "Contribution / Return Policy Manager - Read Only"
                : "Contribution / Return Policy Manager";

            foreach (Form child in MdiChildren)
            {
                if (child is frmPolicyManagerDashboard dashboard)
                    dashboard.SetReadOnlyMode(readOnlyMode);
                else if (child is frmPolicyParametersWindow parameters)
                    parameters.SetReadOnlyMode(readOnlyMode);
                else if (child is frmPolicyLookupTablesWindow lookupTables)
                    lookupTables.SetReadOnlyMode(readOnlyMode);
            }
        }

        private void frmPolicyManagerMdiHost_Load(object? sender, EventArgs e)
        {
            lblStatus.Text = "Opening policy editor...";
            BeginInvoke(new Action(OpenPolicyEditor));
        }

        private void btnPolicyEditor_Click(object? sender, EventArgs e)
        {
            OpenPolicyEditor();
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void btnTileHorizontal_Click(object? sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void btnTileVertical_Click(object? sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void btnCascade_Click(object? sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void btnParameters_Click(object? sender, EventArgs e)
        {
            OpenParameters();
        }

        private void btnLookupTables_Click(object? sender, EventArgs e)
        {
            OpenLookupTables();
        }

        private void OpenPolicyEditor()
        {
            foreach (Form child in MdiChildren)
            {
                if (child is frmPolicyManagerDashboard)
                {
                    child.Activate();
                    return;
                }
            }

            frmPolicyManagerDashboard form = new(_policyManagerService, _readOnlyMode, _valueOnlyEditMode)
            {
                MdiParent = this,
                WindowState = FormWindowState.Normal,
                Size = new Size(1180, 700),
                Location = new Point(12, 12)
            };
            form.StatusChanged += (_, message) => lblStatus.Text = message;
            form.Show();
        }

        private void OpenParameters()
        {
            OpenOrActivate<frmPolicyParametersWindow>(
                () => new frmPolicyParametersWindow(_policyManagerService, _readOnlyMode, _valueOnlyEditMode)
                {
                    MdiParent = this,
                    WindowState = FormWindowState.Normal,
                    Size = new Size(1100, 620),
                    Location = new Point(40, 40)
                });
        }

        private void OpenLookupTables()
        {
            OpenOrActivate<frmPolicyLookupTablesWindow>(
                () => new frmPolicyLookupTablesWindow(_policyManagerService, _readOnlyMode, cornerTypesOnly: false, _valueOnlyEditMode)
                {
                    MdiParent = this,
                    WindowState = FormWindowState.Normal,
                    Size = new Size(1120, 640),
                    Location = new Point(70, 70)
                },
                form => form.Text == "Policy Lookup Tables");
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
            base.OnFormClosed(e);
        }
    }
}
