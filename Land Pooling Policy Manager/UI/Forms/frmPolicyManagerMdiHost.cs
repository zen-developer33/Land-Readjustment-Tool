using Land_Pooling_Policy_Manager.UI;
using Land_Pooling_Policy_Manager.Services;
using System.Linq;

namespace Land_Pooling_Policy_Manager.UI.Forms
{
    public sealed partial class frmPolicyManagerMdiHost : Form
    {
        private readonly PolicyManagerService _policyManagerService;
        private readonly bool _valueOnlyEditMode;
        private bool _readOnlyMode;
        private PolicyWindowLayout? _lastWindowLayout;

        public bool ValueOnlyEditMode => _valueOnlyEditMode;
        private int _layoutSizeIndex;

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
            BeginInvoke(new Action(() => OpenPolicyEditor()));
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
            ArrangeTwoHorizontal();
        }

        private void btnTileVertical_Click(object? sender, EventArgs e)
        {
            ArrangeTwoVertical();
        }

        private void btnCascade_Click(object? sender, EventArgs e)
        {
            foreach (Form child in GetOrderedVisibleChildren())
                child.WindowState = FormWindowState.Normal;

            LayoutMdi(MdiLayout.Cascade);
            lblStatus.Text = "Windows arranged in cascade layout.";
        }

        private void btnParameters_Click(object? sender, EventArgs e)
        {
            OpenParameters();
        }

        private void btnLookupTables_Click(object? sender, EventArgs e)
        {
            OpenLookupTables();
        }

        private void btnLayout2Vertical_Click(object? sender, EventArgs e)
        {
            ArrangeTwoVertical();
        }

        private void btnLayout2Horizontal_Click(object? sender, EventArgs e)
        {
            ArrangeTwoHorizontal();
        }

        private void btnLayout3EditorLeft_Click(object? sender, EventArgs e)
        {
            ArrangeThreeEditorLeft();
        }

        private void btnLayout3Columns_Click(object? sender, EventArgs e)
        {
            ArrangeThreeColumns();
        }

        private void btnLayout3Rows_Click(object? sender, EventArgs e)
        {
            ArrangeThreeRows();
        }

        private void sizeMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem item ||
                item.Tag is not string tag ||
                !int.TryParse(tag, out int index))
                return;

            _layoutSizeIndex = index;
            sizeFullMenuItem.Checked = index == 0;
            sizeComfortMenuItem.Checked = index == 1;
            sizeCompactMenuItem.Checked = index == 2;

            if (_lastWindowLayout.HasValue)
                ArrangeWindows(_lastWindowLayout.Value);
        }

        private frmPolicyManagerDashboard OpenPolicyEditor()
        {
            foreach (Form child in MdiChildren)
            {
                if (child is frmPolicyManagerDashboard existing)
                {
                    child.Activate();
                    return existing;
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
            return form;
        }

        private frmPolicyParametersWindow OpenParameters()
        {
            return OpenOrActivate(
                () => new frmPolicyParametersWindow(_policyManagerService, _readOnlyMode, _valueOnlyEditMode)
                {
                    MdiParent = this,
                    WindowState = FormWindowState.Normal,
                    Size = new Size(1100, 620),
                    Location = new Point(40, 40)
                });
        }

        private frmPolicyLookupTablesWindow OpenLookupTables()
        {
            return OpenOrActivate(
                () => new frmPolicyLookupTablesWindow(_policyManagerService, _readOnlyMode, cornerTypesOnly: false, _valueOnlyEditMode)
                {
                    MdiParent = this,
                    WindowState = FormWindowState.Normal,
                    Size = new Size(1120, 640),
                    Location = new Point(70, 70)
                },
                form => form.Text == "Policy Lookup Tables");
        }

        private TForm OpenOrActivate<TForm>(
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
                    return typedChild;
                }
            }

            TForm form = createForm();
            form.Show();
            return form;
        }

        private void ArrangeTwoVertical()
        {
            OpenPolicyEditor();
            OpenLookupTables();
            ArrangeWindows(PolicyWindowLayout.TwoVertical);
        }

        private void ArrangeTwoHorizontal()
        {
            OpenPolicyEditor();
            OpenParameters();
            ArrangeWindows(PolicyWindowLayout.TwoHorizontal);
        }

        private void ArrangeThreeEditorLeft()
        {
            OpenPolicyEditor();
            OpenLookupTables();
            OpenParameters();
            ArrangeWindows(PolicyWindowLayout.ThreeEditorLeft);
        }

        private void ArrangeThreeColumns()
        {
            OpenPolicyEditor();
            OpenLookupTables();
            OpenParameters();
            ArrangeWindows(PolicyWindowLayout.ThreeColumns);
        }

        private void ArrangeThreeRows()
        {
            OpenPolicyEditor();
            OpenLookupTables();
            OpenParameters();
            ArrangeWindows(PolicyWindowLayout.ThreeRows);
        }

        private void ArrangeWindows(PolicyWindowLayout layout)
        {
            _lastWindowLayout = layout;

            MdiClient? mdiClient = Controls.OfType<MdiClient>().FirstOrDefault();
            if (mdiClient == null || mdiClient.ClientSize.Width <= 100 || mdiClient.ClientSize.Height <= 100)
                return;

            Rectangle workingArea = GetScaledWorkingArea(mdiClient.ClientRectangle);
            int gap = GetLayoutGap();

            Form[] windows = layout switch
            {
                PolicyWindowLayout.TwoVertical => new Form[] { OpenPolicyEditor(), OpenLookupTables() },
                PolicyWindowLayout.TwoHorizontal => new Form[] { OpenPolicyEditor(), OpenParameters() },
                _ => new Form[] { OpenPolicyEditor(), OpenLookupTables(), OpenParameters() }
            };

            foreach (Form child in windows)
            {
                child.WindowState = FormWindowState.Normal;
                child.Show();
            }

            Rectangle[] rectangles = layout switch
            {
                PolicyWindowLayout.TwoVertical => SplitVertical(workingArea, 2, gap),
                PolicyWindowLayout.TwoHorizontal => SplitHorizontal(workingArea, 2, gap),
                PolicyWindowLayout.ThreeColumns => SplitVertical(workingArea, 3, gap),
                PolicyWindowLayout.ThreeRows => SplitHorizontal(workingArea, 3, gap),
                PolicyWindowLayout.ThreeEditorLeft => SplitEditorLeft(workingArea, gap),
                _ => SplitEditorLeft(workingArea, gap)
            };

            int count = Math.Min(windows.Length, rectangles.Length);
            for (int i = 0; i < count; i++)
                windows[i].Bounds = rectangles[i];

            windows[0].Activate();
            lblStatus.Text = layout switch
            {
                PolicyWindowLayout.TwoVertical => "Two-window vertical layout applied.",
                PolicyWindowLayout.TwoHorizontal => "Two-window horizontal layout applied.",
                PolicyWindowLayout.ThreeColumns => "Three-window columns layout applied.",
                PolicyWindowLayout.ThreeRows => "Three-window rows layout applied.",
                PolicyWindowLayout.ThreeEditorLeft => "Editor plus two supporting windows layout applied.",
                _ => "Window layout applied."
            };
        }

        private Rectangle GetScaledWorkingArea(Rectangle clientArea)
        {
            int margin = GetLayoutMargin();
            Rectangle fullArea = new(
                clientArea.Left + margin,
                clientArea.Top + margin,
                Math.Max(300, clientArea.Width - (margin * 2)),
                Math.Max(220, clientArea.Height - (margin * 2)));

            decimal scale = _layoutSizeIndex switch
            {
                1 => 0.90m,
                2 => 0.75m,
                _ => 1.00m
            };

            if (scale == 1.00m)
                return fullArea;

            int width = Math.Max(300, (int)(fullArea.Width * scale));
            int height = Math.Max(220, (int)(fullArea.Height * scale));
            return new Rectangle(
                fullArea.Left + ((fullArea.Width - width) / 2),
                fullArea.Top + ((fullArea.Height - height) / 2),
                width,
                height);
        }

        private int GetLayoutMargin()
        {
            return _layoutSizeIndex switch
            {
                1 => 16,
                2 => 28,
                _ => 8
            };
        }

        private int GetLayoutGap()
        {
            return _layoutSizeIndex switch
            {
                1 => 12,
                2 => 16,
                _ => 8
            };
        }

        private static Rectangle[] SplitVertical(Rectangle area, int count, int gap)
        {
            Rectangle[] rectangles = new Rectangle[count];
            int totalGap = gap * (count - 1);
            int width = Math.Max(120, (area.Width - totalGap) / count);

            for (int i = 0; i < count; i++)
            {
                int x = area.Left + (i * (width + gap));
                int actualWidth = i == count - 1 ? area.Right - x : width;
                rectangles[i] = new Rectangle(x, area.Top, Math.Max(120, actualWidth), area.Height);
            }

            return rectangles;
        }

        private static Rectangle[] SplitHorizontal(Rectangle area, int count, int gap)
        {
            Rectangle[] rectangles = new Rectangle[count];
            int totalGap = gap * (count - 1);
            int height = Math.Max(120, (area.Height - totalGap) / count);

            for (int i = 0; i < count; i++)
            {
                int y = area.Top + (i * (height + gap));
                int actualHeight = i == count - 1 ? area.Bottom - y : height;
                rectangles[i] = new Rectangle(area.Left, y, area.Width, Math.Max(120, actualHeight));
            }

            return rectangles;
        }

        private static Rectangle[] SplitEditorLeft(Rectangle area, int gap)
        {
            int editorWidth = Math.Max(420, (int)(area.Width * 0.50));
            int rightWidth = Math.Max(320, area.Width - editorWidth - gap);
            int rightX = area.Left + editorWidth + gap;
            int rightHeight = Math.Max(150, (area.Height - gap) / 2);

            return new[]
            {
                new Rectangle(area.Left, area.Top, editorWidth, area.Height),
                new Rectangle(rightX, area.Top, rightWidth, rightHeight),
                new Rectangle(rightX, area.Top + rightHeight + gap, rightWidth, area.Bottom - (area.Top + rightHeight + gap))
            };
        }

        private Form[] GetOrderedVisibleChildren()
        {
            return MdiChildren
                .Where(child => !child.IsDisposed && child.Visible)
                .OrderBy(GetWindowSortKey)
                .ToArray();
        }

        private static int GetWindowSortKey(Form child)
        {
            return child switch
            {
                frmPolicyManagerDashboard => 0,
                frmPolicyLookupTablesWindow => 1,
                frmPolicyParametersWindow => 2,
                _ => 10
            };
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        private enum PolicyWindowLayout
        {
            TwoVertical,
            TwoHorizontal,
            ThreeEditorLeft,
            ThreeColumns,
            ThreeRows
        }
    }
}
