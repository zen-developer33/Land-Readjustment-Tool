using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using System.Diagnostics;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Document management form for a specific landowner.
    /// Displays documents in a DataGridView with attach, open, and delete functionality.
    /// </summary>
    public partial class frmOwnerDocuments : Form
    {
        private readonly string _projectPath;
        private readonly LandOwner _owner;
        private readonly LandOwnerRepository _repository;

        public frmOwnerDocuments(string projectPath, LandOwner owner, LandOwnerRepository repository)
        {
            InitializeComponent();
            _projectPath = projectPath;
            _owner = owner;
            _repository = repository;

            Text = $"Documents - {_owner.LandOwnersName}";
            LoadDocuments();
        }

        private void LoadDocuments()
        {
            dgvDocuments.Rows.Clear();

            if (string.IsNullOrWhiteSpace(_owner.DocumentsFolderPath))
            {
                lblDocCount.Text = "Documents: 0";
                return;
            }

            string docsFolder = Path.Combine(
                Path.GetDirectoryName(_projectPath)!,
                _owner.DocumentsFolderPath
            );

            if (!Directory.Exists(docsFolder))
            {
                lblDocCount.Text = "Documents: 0";
                return;
            }

            try
            {
                var files = Directory.GetFiles(docsFolder);
                foreach (var file in files)
                {
                    var info = new FileInfo(file);
                    string size = info.Length < 1024
                        ? $"{info.Length} B"
                        : info.Length < 1024 * 1024
                            ? $"{info.Length / 1024.0:F1} KB"
                            : $"{info.Length / (1024.0 * 1024.0):F1} MB";

                    dgvDocuments.Rows.Add(info.Name, info.Extension.TrimStart('.').ToUpper(), size);
                }

                lblDocCount.Text = $"Documents: {files.Length}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load documents: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetDocumentsFolder()
        {
            string docsFolder = Path.Combine(
                Path.GetDirectoryName(_projectPath)!,
                "Documents",
                $"LandOwner_{_owner.LandOwnerId}"
            );

            Directory.CreateDirectory(docsFolder);

            // Ensure database has the folder path
            string relativePath = Path.Combine("Documents", $"LandOwner_{_owner.LandOwnerId}");
            if (_owner.DocumentsFolderPath != relativePath)
            {
                _repository.UpdateOwnerDocumentsFolder(_owner.LandOwnerId, relativePath);
                _owner.DocumentsFolderPath = relativePath;
            }

            return docsFolder;
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

                foreach (string filePath in ofd.FileNames)
                {
                    string fileName = Path.GetFileName(filePath);
                    string destPath = Path.Combine(docsFolder, fileName);

                    int counter = 1;
                    while (File.Exists(destPath))
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        string extension = Path.GetExtension(fileName);
                        destPath = Path.Combine(docsFolder, $"{nameWithoutExt}_{counter}{extension}");
                        counter++;
                    }

                    File.Copy(filePath, destPath);
                }

                LoadDocuments();

                MessageBox.Show($"{ofd.FileNames.Length} document(s) attached successfully!", "Success",
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
                string docsFolder = Path.Combine(
                    Path.GetDirectoryName(_projectPath)!,
                    _owner.DocumentsFolderPath!
                );

                string filePath = Path.Combine(docsFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LoadDocuments();
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
                string docsFolder = Path.Combine(
                    Path.GetDirectoryName(_projectPath)!,
                    _owner.DocumentsFolderPath
                );

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
    }
}
