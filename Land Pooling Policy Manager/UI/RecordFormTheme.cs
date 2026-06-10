using System;
using System.Drawing;
using System.Windows.Forms;

namespace Land_Pooling_Policy_Manager.UI.Forms
{
    internal static class RecordFormTheme
    {
        private static readonly Color Surface = Color.FromArgb(243, 244, 246);
        private static readonly Color PanelSurface = Color.FromArgb(248, 250, 252);
        private static readonly Color ControlSurface = Color.White;
        private static readonly Color Border = Color.FromArgb(203, 213, 225);
        private static readonly Color GridLine = Color.FromArgb(214, 222, 232);
        private static readonly Color HeaderBack = Color.FromArgb(238, 242, 247);
        private static readonly Color Text = Color.FromArgb(11, 31, 54);
        private static readonly Color MutedText = Color.FromArgb(71, 85, 105);
        private static readonly Color SelectionBack = Color.FromArgb(215, 235, 255);
        private static readonly Color SelectionText = Color.FromArgb(6, 45, 88);

        public static void Apply(Form form)
        {
            ArgumentNullException.ThrowIfNull(form);

            form.BackColor = Surface;
            form.ForeColor = Text;
            form.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            ApplyControls(form.Controls);
        }

        private static void ApplyControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyControl(control);

                if (control is SplitContainer splitContainer)
                {
                    ApplyControls(splitContainer.Panel1.Controls);
                    ApplyControls(splitContainer.Panel2.Controls);
                    continue;
                }

                if (control.HasChildren)
                {
                    ApplyControls(control.Controls);
                }
            }
        }

        private static void ApplyControl(Control control)
        {
            switch (control)
            {
                case DataGridView grid:
                    ApplyGrid(grid);
                    break;

                case Button button:
                    ApplyButton(button);
                    break;

                case TextBox textBox:
                    textBox.BackColor = ControlSurface;
                    textBox.ForeColor = Text;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ComboBox comboBox:
                    comboBox.BackColor = ControlSurface;
                    comboBox.ForeColor = Text;
                    comboBox.FlatStyle = FlatStyle.System;
                    break;

                case CheckBox checkBox:
                    checkBox.ForeColor = Text;
                    checkBox.BackColor = Color.Transparent;
                    break;

                case RadioButton radioButton:
                    radioButton.ForeColor = Text;
                    radioButton.BackColor = Color.Transparent;
                    break;

                case GroupBox groupBox:
                    groupBox.ForeColor = Text;
                    groupBox.BackColor = Surface;
                    break;

                case TabPage tabPage:
                    tabPage.BackColor = PanelSurface;
                    tabPage.ForeColor = Text;
                    break;

                case TabControl tabControl:
                    tabControl.BackColor = Surface;
                    tabControl.ForeColor = Text;
                    break;

                case ToolStrip toolStrip:
                    ApplyToolStrip(toolStrip);
                    break;

                case Label label:
                    ApplyLabel(label);
                    break;

                case Panel or TableLayoutPanel or FlowLayoutPanel:
                    control.BackColor = Surface;
                    control.ForeColor = Text;
                    break;
            }
        }

        private static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = ControlSurface;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = GridLine;
            grid.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderBack;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBack;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 3, 4, 3);

            grid.DefaultCellStyle.BackColor = ControlSurface;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = SelectionBack;
            grid.DefaultCellStyle.SelectionForeColor = SelectionText;
            grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Text;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = SelectionBack;
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = SelectionText;

            grid.RowHeadersDefaultCellStyle.BackColor = HeaderBack;
            grid.RowHeadersDefaultCellStyle.ForeColor = MutedText;
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = HeaderBack;
            grid.RowHeadersDefaultCellStyle.SelectionForeColor = Text;
            grid.RowTemplate.Height = Math.Max(grid.RowTemplate.Height, 28);
        }

        private static void ApplyButton(Button button)
        {
            button.BackColor = ControlSurface;
            button.ForeColor = Text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(219, 234, 254);
            button.UseVisualStyleBackColor = false;
        }

        private static void ApplyToolStrip(ToolStrip toolStrip)
        {
            toolStrip.BackColor = PanelSurface;
            toolStrip.ForeColor = Text;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;

            foreach (ToolStripItem item in toolStrip.Items)
            {
                item.ForeColor = Text;
                item.BackColor = PanelSurface;

                if (item is ToolStripTextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = ControlSurface;
                    textBox.ForeColor = Text;
                }
            }
        }

        private static void ApplyLabel(Label label)
        {
            label.ForeColor = label.Font.Bold
                ? Text
                : MutedText;
            label.BackColor = Color.Transparent;
        }
    }
}
