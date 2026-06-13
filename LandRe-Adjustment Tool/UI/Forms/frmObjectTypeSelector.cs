using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Land_Readjustment_Tool.UI.CustomControls;

namespace Land_Readjustment_Tool.UI.Forms
{
    public partial class frmObjectTypeSelector : Form
    {
        private readonly List<ObjectTypeSelectorItem> _items;

        public event Action<IReadOnlyList<Guid>, CanvasSelectionApplyMode>? SelectionRequested;

        public frmObjectTypeSelector(
            string title,
            IEnumerable<ObjectTypeSelectorItem> items)
        {
            InitializeComponent();

            Text = title;
            _items = items
                .OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            txtFilter.TextChanged += (_, _) => ApplyFilter();
            btnSelect.Click += (_, _) => RequestSelection(CanvasSelectionApplyMode.Create);
            btnDeselect.Click += (_, _) => RequestSelection(CanvasSelectionApplyMode.Remove);
            btnClose.Click += (_, _) => Close();
            lstItems.DoubleClick += (_, _) => RequestSelection(CanvasSelectionApplyMode.Create);

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string filter = txtFilter.Text.Trim();
            IEnumerable<ObjectTypeSelectorItem> visibleItems = _items;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                visibleItems = visibleItems.Where(item =>
                    item.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase));
            }

            lstItems.BeginUpdate();
            try
            {
                lstItems.Items.Clear();
                foreach (ObjectTypeSelectorItem item in visibleItems)
                    lstItems.Items.Add(item);
            }
            finally
            {
                lstItems.EndUpdate();
            }

            lblCount.Text = $"{lstItems.Items.Count:N0} of {_items.Count:N0}";
        }

        private void RequestSelection(CanvasSelectionApplyMode mode)
        {
            Guid[] ids = lstItems.SelectedItems
                .Cast<ObjectTypeSelectorItem>()
                .SelectMany(item => item.CanvasObjectIds)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
            {
                MessageBox.Show(
                    this,
                    "Select one or more mapped items first.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            SelectionRequested?.Invoke(ids, mode);
        }
    }
}
