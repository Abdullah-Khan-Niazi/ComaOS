using ComaOS.Core.Hardware;

namespace ComaOS.Core.FileSystem;

/// <summary>
/// Manages CRUD operations for the virtual file system.
/// Does not use the actual Windows file system - all files are stored in memory.
/// </summary>
public class FileManager
{
    private readonly HardDrive _hardDrive;
    private readonly List<VirtualFile> _files;
    private readonly object _lockObject = new();
    private int _nextFileId = 1;

    /// <summary>
    /// Gets all files in the virtual file system.
    /// </summary>
    public IReadOnlyList<VirtualFile> GetAllFiles()
    {
        lock (_lockObject)
        {
            return _files.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Initializes a new instance of the FileManager.
    /// </summary>
    /// <param name="hardDrive">The hard drive to manage disk space allocation.</param>
    public FileManager(HardDrive hardDrive)
    {
        _hardDrive = hardDrive ?? throw new ArgumentNullException(nameof(hardDrive));
        _files = new List<VirtualFile>();

        // Create default root directory structure
        CreateDefaultDirectories();
    }

    /// <summary>
    /// Creates a new file in the virtual file system.
    /// </summary>
    /// <param name="fileName">The name of the file to create.</param>
    /// <param name="filePath">The directory path where the file will be created.</param>
    /// <param name="content">The initial content of the file.</param>
    /// <returns>The created VirtualFile, or null if creation failed.</returns>
    public VirtualFile? CreateFile(string fileName, string filePath, string content = "")
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        lock (_lockObject)
        {
            // Check if file already exists
            var fullPath = System.IO.Path.Combine(filePath, fileName);
            if (_files.Any(f => f.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase)))
            {
                return null; // File already exists
            }

            // Calculate size and allocate disk space
            long sizeBytes = System.Text.Encoding.UTF8.GetByteCount(content);
            long sizeGB = (long)Math.Ceiling(sizeBytes / (1024.0 * 1024.0 * 1024.0));
            if (sizeGB < 1) sizeGB = 1; // Minimum 1 GB allocation

            if (!_hardDrive.AllocateDiskSpace(fileName, sizeGB, out int blockId))
            {
                return null; // Insufficient disk space
            }

            var file = new VirtualFile(_nextFileId++, fileName, filePath, content);
            _files.Add(file);

            return file;
        }
    }

    /// <summary>
    /// Reads the content of a file.
    /// </summary>
    /// <param name="fullPath">The full path to the file.</param>
    /// <returns>The file content, or null if file not found.</returns>
    public string? ReadFile(string fullPath)
    {
        lock (_lockObject)
        {
            var file = _files.FirstOrDefault(f => f.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
            return file?.Content;
        }
    }

    /// <summary>
    /// Updates the content of an existing file.
    /// </summary>
    /// <param name="fullPath">The full path to the file.</param>
    /// <param name="newContent">The new content to write.</param>
    /// <returns>True if update succeeded; false if file not found or read-only.</returns>
    public bool UpdateFile(string fullPath, string newContent)
    {
        lock (_lockObject)
        {
            var file = _files.FirstOrDefault(f => f.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
            if (file == null || file.IsReadOnly)
                return false;

            file.Content = newContent;
            file.ModifiedAt = DateTime.Now;
            return true;
        }
    }

    /// <summary>
    /// Deletes a file from the virtual file system.
    /// </summary>
    /// <param name="fullPath">The full path to the file to delete.</param>
    /// <returns>True if deletion succeeded; false if file not found or read-only.</returns>
    public bool DeleteFile(string fullPath)
    {
        lock (_lockObject)
        {
            var file = _files.FirstOrDefault(f => f.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
            if (file == null || file.IsReadOnly)
                return false;

            _files.Remove(file);
            _hardDrive.DeallocateDiskSpace(file.FileName);
            return true;
        }
    }

    /// <summary>
    /// Copies a file to a new location.
    /// </summary>
    /// <param name="sourceFullPath">The full path of the source file.</param>
    /// <param name="destinationPath">The destination directory path.</param>
    /// <param name="newFileName">Optional new file name (if null, uses original name).</param>
    /// <returns>The copied VirtualFile, or null if operation failed.</returns>
    public VirtualFile? CopyFile(string sourceFullPath, string destinationPath, string? newFileName = null)
    {
        lock (_lockObject)
        {
            var sourceFile = _files.FirstOrDefault(f => f.FullPath.Equals(sourceFullPath, StringComparison.OrdinalIgnoreCase));
            if (sourceFile == null)
                return null;

            string targetFileName = newFileName ?? sourceFile.FileName;
            return CreateFile(targetFileName, destinationPath, sourceFile.Content);
        }
    }

    /// <summary>
    /// Moves a file to a new location.
    /// </summary>
    /// <param name="sourceFullPath">The full path of the source file.</param>
    /// <param name="destinationPath">The destination directory path.</param>
    /// <returns>True if move succeeded; false otherwise.</returns>
    public bool MoveFile(string sourceFullPath, string destinationPath)
    {
        lock (_lockObject)
        {
            var copiedFile = CopyFile(sourceFullPath, destinationPath);
            if (copiedFile == null)
                return false;

            return DeleteFile(sourceFullPath);
        }
    }

    /// <summary>
    /// Gets all files in a specific directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to search.</param>
    /// <returns>List of files in the directory.</returns>
    public IReadOnlyList<VirtualFile> GetFilesInDirectory(string directoryPath)
    {
        lock (_lockObject)
        {
            return _files
                .Where(f => f.FilePath.Equals(directoryPath, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Searches for files by name pattern.
    /// </summary>
    /// <param name="searchPattern">The search pattern (e.g., "*.txt").</param>
    /// <returns>List of matching files.</returns>
    public IReadOnlyList<VirtualFile> SearchFiles(string searchPattern)
    {
        lock (_lockObject)
        {
            var pattern = searchPattern.Replace("*", ".*").Replace("?", ".");
            var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return _files
                .Where(f => regex.IsMatch(f.FileName))
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Gets a file by its ID.
    /// </summary>
    public VirtualFile? GetFileById(int fileId)
    {
        lock (_lockObject)
        {
            return _files.FirstOrDefault(f => f.FileId == fileId);
        }
    }

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    public bool FileExists(string fullPath)
    {
        lock (_lockObject)
        {
            return _files.Any(f => f.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Clears all files from the file system. Only available in Kernel Mode.
    /// </summary>
    public void ClearAllFiles()
    {
        lock (_lockObject)
        {
            _files.Clear();
            CreateDefaultDirectories();
        }
    }

    /// <summary>
    /// Creates default directory structure (empty marker files).
    /// </summary>
    private void CreateDefaultDirectories()
    {
        // Create system directories by adding placeholder files
        var systemDirs = new[] { "/System", "/Users", "/Documents", "/Downloads", "/Programs" };
        
        foreach (var dir in systemDirs)
        {
            // Create a hidden marker file to establish the directory
            var marker = new VirtualFile(_nextFileId++, ".directory", dir, "", isReadOnly: true, isHidden: true);
            _files.Add(marker);
        }
    }

    /// <summary>
    /// Gets statistics about the file system.
    /// </summary>
    public string GetFileSystemStatus()
    {
        lock (_lockObject)
        {
            var totalFiles = _files.Count(f => !f.IsHidden);
            var totalSize = _files.Sum(f => f.SizeKB);
            var filesByType = _files.GroupBy(f => f.Type).Select(g => $"{g.Key}: {g.Count()}");

            return $"File System Status:\n" +
                   $"  Total Files: {totalFiles}\n" +
                   $"  Total Size: {totalSize:F2} KB\n" +
                   $"  By Type:\n    {string.Join("\n    ", filesByType)}";
        }
    }
}
