namespace Land_Readjustment_Tool.UI.Helpers
{
    /// <summary>
    /// Makes NumericUpDown entry behave like a direct numeric field: clicking
    /// into it selects the existing value so the next typed digit replaces it.
    /// </summary>
    public static class NumericUpDownSelectAllBehavior
    {
        public static void AttachTo(Control root)
        {
            foreach (NumericUpDown input in EnumerateNumericInputs(root))
            {
                input.Enter -= NumericInput_SelectAll;
                input.Enter += NumericInput_SelectAll;
            }
        }

        private static IEnumerable<NumericUpDown> EnumerateNumericInputs(Control root)
        {
            foreach (Control child in root.Controls)
            {
                if (child is NumericUpDown input)
                {
                    yield return input;
                }

                foreach (NumericUpDown nested in EnumerateNumericInputs(child))
                {
                    yield return nested;
                }
            }
        }

        private static void NumericInput_SelectAll(object? sender, EventArgs e)
        {
            if (sender is not NumericUpDown input || !input.Enabled)
            {
                return;
            }

            input.BeginInvoke((MethodInvoker)(() =>
            {
                input.Focus();
                input.Select(0, input.Text.Length);
            }));
        }
    }
}
