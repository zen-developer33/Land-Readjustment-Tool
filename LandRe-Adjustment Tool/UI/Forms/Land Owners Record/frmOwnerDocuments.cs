using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services.LandData;
using System.Diagnostics;
using System.IO;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Document management form for a specific landowner.
    /// Displays documents in a DataGridView with attach, open, and delete functionality.
    /// </summary>
    public partial class frmOwnerDocuments : Form
    {
        private readonly LandOwner _owner;
        private readonly LandRecordsService _landRecordsService;
        private readonly OwnerFileStorageService _ownerFileStorageService;
        private bool _hasChanges;


        public frmOwnerDocuments(
            LandOwner owner,
            LandRecordsService landRecordsService,
            OwnerFileStorageService ownerFileStorageService,
            bool isReadOnly)
        {
            InitializeComponent();
            _owner = owner;
            _landRecordsService = landRecordsService;
            _ownerFileStorageService = ownerFileStorageService;

            Text = $"Documents - {_owner.LandOwnersName}";
            LoadDocuments();
            btnAttach.Enabled = !isReadOnly;
            btnDelete.Enabled = !isReadOnly;
            btnClose.Click += btnClose_Click;
        }

        private void LoadDocuments()
        {
            dgvDocuments.Rows.Clear();

            try
            {
                var files = _ownerFileStorageService.GetDocuments(_owner.DocumentsFolderPath);
                foreach (var info in files)
                {
                    string size = info.Length < 1024
                        ? $"{info.Length} B"
                        : info.Length < 1024 * 1024
                            ? $"{info.Length / 1024.0:F1} KB"
                            : $"{info.Length / (1024.0 * 1024.0):F1} MB";

                    dgvDocuments.Rows.Add(info.Name, info.Extension.TrimStart('.').ToUpper(), size);
                }

                lblDocCount.Text = $"Documents: {files.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load documents: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetDocumentsFolder()
        {
            var (absolutePath, relativePath) = _ownerFileStorageService.EnsureOwnerDocumentsFolder(_owner.LandOwnerId);

            // Ensure database has the folder path
            if (!string.Equals(_owner.DocumentsFolderPath, relativePath, StringComparison.OrdinalIgnoreCase))
            {
                _landRecordsService.UpdateOwnerDocumentsFolder(_owner.LandOwnerId, relativePath);
                _owner.DocumentsFolderPath = relativePath;
            }

            return absolutePath;
        }

        private void btnAttach_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "All Files|*.*|PDF Files|*.pdf|Image Files|*.jpg;*.jpeg;*.png";
            ofd.Title = "Select Documents to Attach";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                string docsFolder = GetDocumentsFolder();

                var attachedCount = _ownerFileStorageService.AttachDocuments(docsFolder, ofd.FileNames);

                LoadDocuments();
                if (attachedCount > 0)
                {
                    _hasChanges = true;
                }

                MessageBox.Show($"{attachedCount} document(s) attached successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to attach document: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenSelectedDocument();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDocuments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a document to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string fileName = dgvDocuments.SelectedRows[0].Cells[0].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(fileName)) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{fileName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                if (_ownerFileStorageService.DeleteDocument(_owner.DocumentsFolderPath, fileName))
                {
                    LoadDocuments();
                    _hasChanges = true;
                }
                else
                {
                    MessageBox.Show("Document could not be deleted because it no longer exists.", "Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete document: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvDocuments_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            OpenSelectedDocument();
        }

        private void OpenSelectedDocument()
        {
            if (dgvDocuments.SelectedRows.Count == 0) return;
            if (string.IsNullOrWhiteSpace(_owner.DocumentsFolderPath)) return;

            string fileName = dgvDocuments.SelectedRows[0].Cells[0].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(fileName)) return;

            try
            {
                var docsFolder = _ownerFileStorageService.ResolveStoredPath(_owner.DocumentsFolderPath);
                if (string.IsNullOrWhiteSpace(docsFolder))
                    return;

                string filePath = Path.Combine(docsFolder, fileName);

                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open document: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmOwnerDocuments_Load(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_hasChanges && DialogResult == DialogResult.None)
            {
                DialogResult = DialogResult.OK;
            }

            base.OnFormClosing(e);
        }
    }
}
