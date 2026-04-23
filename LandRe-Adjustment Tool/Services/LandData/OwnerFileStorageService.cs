using System.Drawing;

namespace Land_Readjustment_Tool.Services.LandData
{
    /// <summary>
    /// Handles photo/document filesystem operations for land owners.
    /// Keeps UI forms free from direct path manipulation details.
    /// </summary>
    public sealed class OwnerFileStorageService
    {
        private readonly string _projectFilePath;
        private readonly string _projectDirectory;

        public OwnerFileStorageService(string projectFilePath)
        {
            _projectFilePath = projectFilePath ?? throw new ArgumentNullException(nameof(projectFilePath));
            _projectDirectory = Path.GetDirectoryName(_projectFilePath)
                ?? throw new InvalidOperationException("Invalid project file path.");
        }

        public string ProjectDirectory => _projectDirectory;

        public string EnsureOwnerPhotosFolder()
        {
            var folder = Path.Combine(_projectDirectory, "OwnerPhotos");
            Directory.CreateDirectory(folder);
            return folder;
        }

        public string SaveOwnerPhoto(int ownerId, string sourceFilePath)
        {
            if (ownerId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ownerId));

            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
                throw new FileNotFoundException("Source photo not found.", sourceFilePath);

            var photosFolder = EnsureOwnerPhotosFolder();
            var extension = Path.GetExtension(sourceFilePath);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";

            var fileName = $"Owner_{ownerId}{extension}";
            var destinationPath = Path.Combine(photosFolder, fileName);

            File.Copy(sourceFilePath, destinationPath, overwrite: true);

            return Path.Combine("OwnerPhotos", fileName);
        }

        public Image? LoadPhotoFromStoredPath(string? storedPath)
        {
            var fullPath = ResolveStoredPath(storedPath);
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                return null;

            // Clone from stream to avoid locking the file.
            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }

        public (string AbsolutePath, string RelativePath) EnsureOwnerDocumentsFolder(int ownerId)
        {
            if (ownerId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ownerId));

            var relative = Path.Combine("Documents", $"LandOwner_{ownerId}");
            var absolute = Path.Combine(_projectDirectory, relative);
            Directory.CreateDirectory(absolute);
            return (absolute, relative);
        }

        public IReadOnlyList<FileInfo> GetDocuments(string? documentsFolderPath)
        {
            var folderPath = ResolveStoredPath(documentsFolderPath);
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return [];

            return Directory.GetFiles(folderPath)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();
        }

        public int GetDocumentCount(string? documentsFolderPath)
        {
            var folderPath = ResolveStoredPath(documentsFolderPath);
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return 0;

            return Directory.GetFiles(folderPath).Length;
        }

        public int AttachDocuments(string absoluteDocumentsFolder, IEnumerable<string> sourceFilePaths)
        {
            if (string.IsNullOrWhiteSpace(absoluteDocumentsFolder))
                throw new ArgumentException("Document folder path is required.", nameof(absoluteDocumentsFolder));
            if (sourceFilePaths == null)
                throw new ArgumentNullException(nameof(sourceFilePaths));

            Directory.CreateDirectory(absoluteDocumentsFolder);
            var copiedCount = 0;

            foreach (var sourcePath in sourceFilePaths)
            {
                if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                    continue;

                var fileName = Path.GetFileName(sourcePath);
                var destinationPath = Path.Combine(absoluteDocumentsFolder, fileName);

                var counter = 1;
                while (File.Exists(destinationPath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var extension = Path.GetExtension(fileName);
                    destinationPath = Path.Combine(absoluteDocumentsFolder, $"{nameWithoutExt}_{counter}{extension}");
                    counter++;
                }

                File.Copy(sourcePath, destinationPath);
                copiedCount++;
            }

            return copiedCount;
        }

        public bool DeleteDocument(string? documentsFolderPath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var folderPath = ResolveStoredPath(documentsFolderPath);
            if (string.IsNullOrWhiteSpace(folderPath))
                return false;
            if (!Directory.Exists(folderPath))
                return false;

            var safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeFileName))
                return false;

            var targetFile = Path.GetFullPath(Path.Combine(folderPath, safeFileName));
            var fullFolderPath = Path.GetFullPath(folderPath);
            if (!targetFile.StartsWith(fullFolderPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!File.Exists(targetFile))
                return false;

            File.Delete(targetFile);
            return true;
        }

        public string? ResolveStoredPath(string? storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                return null;

            if (Path.IsPathRooted(storedPath))
                return Path.GetFullPath(storedPath);

            return Path.GetFullPath(Path.Combine(_projectDirectory, storedPath));
        }
    }
}
