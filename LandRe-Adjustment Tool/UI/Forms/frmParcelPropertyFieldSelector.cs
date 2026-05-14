using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Forms
{
    public partial class frmParcelPropertyFieldSelector : Form
    {
        private readonly List<ParcelPropertyFieldSelectorItem> _allFields;

        public IReadOnlyList<string> SelectedFieldKeys { get; private set; }

        public frmParcelPropertyFieldSelector(
            IEnumerable<ParcelPropertyFieldSelectorItem> allFields,
            IEnumerable<string> selectedFieldKeys)
        {
            InitializeComponent();

            _allFields = allFields.ToList();
            SelectedFieldKeys = selectedFieldKeys.ToList();
            LoadFieldLists(SelectedFieldKeys);
        }

        private void LoadFieldLists(IEnumerable<string> selectedFieldKeys)
        {
            var selected = new HashSet<string>(selectedFieldKeys, StringComparer.OrdinalIgnoreCase);

            lstAvailable.Items.Clear();
            lstSelected.Items.Clear();

            foreach (var field in _allFields)
            {
                if (selected.Contains(field.Key))
                    lstSelected.Items.Add(field);
                else
                    lstAvailable.Items.Add(field);
            }
        }

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            MoveSelectedItems(lstAvailable, lstSelected);
        }

        private void btnRemove_Click(object? sender, EventArgs e)
        {
            MoveSelectedItems(lstSelected, lstAvailable);
        }

        private static void MoveSelectedItems(ListBox source, ListBox target)
        {
            var items = source.SelectedItems.Cast<object>().ToList();
            foreach (var item in items)
            {
                source.Items.Remove(item);
                target.Items.Add(item);
            }
        }

        private void btnOk_Click(object? sender, EventArgs e)
        {
            SelectedFieldKeys = lstSelected.Items
                .Cast<ParcelPropertyFieldSelectorItem>()
                .Select(item => item.Key)
                .ToList();

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public sealed class ParcelPropertyFieldSelectorItem
    {
        public ParcelPropertyFieldSelectorItem(string key, string category, string label)
        {
            Key = key;
            Category = category;
            Label = label;
        }

        public string Key { get; }
        public string Category { get; }
        public string Label { get; }

        public override string ToString()
        {
            return $"{Category} - {Label}";
        }
    }
}
