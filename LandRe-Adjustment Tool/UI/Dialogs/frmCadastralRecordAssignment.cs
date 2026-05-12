using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services.Assignment;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmCadastralRecordAssignment : Form
    {
        private readonly ProjectSession _session;
        private readonly ICadastralRecordAssignmentService _assignmentService;

        public bool AssignmentChanged { get; private set; }

        public frmCadastralRecordAssignment(
            ProjectSession session,
            ICadastralRecordAssignmentService assignmentService)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));

            InitializeComponent();
            btnAutoAssign.Click += btnAutoAssign_Click;
        }

        private async void btnAutoAssign_Click(object? sender, EventArgs e)
        {
            try
            {
                btnAutoAssign.Enabled = false;
                btnClose.Enabled = false;
                lblStatus.Text = "Assigning cadastral records...";

                CadastralAssignmentResult result =
                    await _assignmentService.AutoAssignAsync(
                        _session,
                        chkReplaceExisting.Checked);

                AssignmentChanged = result.AssignedCount > 0;
                lblStatus.Text =
                    $"Assigned {result.AssignedCount}. Missing keys: {result.MissingKeyCount}. No record match: {result.NoRecordMatchCount}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Cadastral assignment failed: {ex.Message}",
                    "Assign Cadastral Records",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                lblStatus.Text = "Assignment failed.";
            }
            finally
            {
                btnAutoAssign.Enabled = true;
                btnClose.Enabled = true;
            }
        }
    }
}
