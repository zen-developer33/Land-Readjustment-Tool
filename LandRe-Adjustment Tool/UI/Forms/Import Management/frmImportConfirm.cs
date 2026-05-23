namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Displays an import summary and — when existing data is already in the database —
    /// lets the user choose between replacing all existing data or merging into it.
    /// Replaces the old chain of MessageBox prompts that appeared when clicking Save.
    /// </summary>
    public sealed partial class frmImportConfirm : Form
    {
        // ── Public result ──────────────────────────────────────────────────────────

        /// <summary>
        /// True when the user chose to replace all existing data.
        /// False (default) means add/merge into existing data.
        /// Only meaningful after DialogResult == OK.
        /// </summary>
        public bool ReplaceExisting => rbReplace.Checked;

        // ── Constructor ────────────────────────────────────────────────────────────

        /// <param name="summaryText">Human-readable import summary shown in the text box.</param>
        /// <param name="existingOwnerCount">Landowners already in the database (0 = no conflict).</param>
        /// <param name="existingParcelCount">Parcels already in the database (0 = no conflict).</param>
        public frmImportConfirm(string summaryText, int existingOwnerCount, int existingParcelCount)
        {
            InitializeComponent();

            // Summary text — BorderStyle was overridden by the theme; restore clean look.
            txtSummary.Text = summaryText;
            txtSummary.BorderStyle = BorderStyle.None;

            bool hasConflict = existingOwnerCount > 0 || existingParcelCount > 0;
            grpConflict.Visible = hasConflict;

            if (hasConflict)
            {
                // Populate conflict statistics.
                lblConflictStats.Text =
                    $"The database already contains:\n" +
                    $"    •  {existingOwnerCount:N0}  Landowner(s)\n" +
                    $"    •  {existingParcelCount:N0}  Land Parcel(s)";

                // Semantic colours applied after the theme has run.
                grpConflict.ForeColor    = Color.FromArgb(180, 83, 9);   // amber — "warning"
                rbReplace.ForeColor      = Color.FromArgb(153, 27, 27);  // dark red  — danger
                lblReplaceHint.ForeColor = Color.FromArgb(153, 27, 27);
                rbAdd.ForeColor          = Color.FromArgb(21, 128, 61);  // dark green — safe
                lblAddHint.ForeColor     = Color.FromArgb(71, 85, 105);  // slate — subtle hint

                // Fit: summary + gap + conflict group + gap + button bar.
                ClientSize = new Size(ClientSize.Width, grpConflict.Bottom + 16 + pnlButtons.Height);
            }
            else
            {
                // Fit: summary + gap + button bar only.
                ClientSize = new Size(ClientSize.Width, grpSummary.Bottom + 16 + pnlButtons.Height);
            }
        }

        // ── Button handlers ────────────────────────────────────────────────────────

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            // Extra inline confirmation when the destructive option is selected.
            if (rbReplace.Checked)
            {
                var confirm = MessageBox.Show(
                    "You are about to permanently DELETE all existing landowners and parcels.\n\n" +
                    "This action cannot be undone. Are you absolutely sure?",
                    "Confirm Data Replacement",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation);

                if (confirm != DialogResult.Yes)
                    return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
