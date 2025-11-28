namespace ComaOS.Core.FileSystem;

/// <summary>
/// Represents a virtual file in the simulated file system.
/// This does not use the actual Windows file system.
/// </summary>
public record VirtualFile
{
    /// <summary>
    /// Gets the unique file ID.
    /// </summary>
    public int FileId { get; init; }

    /// <summary>
    /// Gets the file name (including extension).
    /// </summary>
    public string FileName { get; init; }

    /// <summary>
    /// Gets the file path (directory structure).
    /// </summary>
    public string FilePath { get; init; }

    /// <summary>
    /// Gets the full path (FilePath + FileName).
    /// </summary>
    public string FullPath => System.IO.Path.Combine(FilePath, FileName);

    /// <summary>
    /// Gets the file content (stored as string for simplicity).
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long SizeBytes => System.Text.Encoding.UTF8.GetByteCount(Content);

    /// <summary>
    /// Gets the file size in kilobytes.
    /// </summary>
    public double SizeKB => SizeBytes / 1024.0;

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    public string Extension => System.IO.Path.GetExtension(FileName);

    /// <summary>
    /// Gets the file type based on extension.
    /// </summary>
    public FileType Type => DetermineFileType(Extension);

    /// <summary>
    /// Gets the timestamp when the file was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the file was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Gets whether this file is read-only.
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// Gets whether this file is hidden.
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// Initializes a new VirtualFile.
    /// </summary>
    public VirtualFile(int fileId, string fileName, string filePath, string content, bool isReadOnly = false, bool isHidden = false)
    {
        FileId = fileId;
        FileName = fileName;
        FilePath = filePath;
        Content = content;
        CreatedAt = DateTime.Now;
        ModifiedAt = DateTime.Now;
        IsReadOnly = isReadOnly;
        IsHidden = isHidden;
    }

    /// <summary>
    /// Determines the file type based on extension.
    /// </summary>
    private static FileType DetermineFileType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => FileType.Text,
            ".doc" or ".docx" => FileType.Document,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => FileType.Image,
            ".mp3" or ".wav" or ".flac" => FileType.Audio,
            ".mp4" or ".avi" or ".mkv" or ".mov" => FileType.Video,
            ".zip" or ".rar" or ".7z" => FileType.Archive,
            ".exe" or ".dll" => FileType.Executable,
            _ => FileType.Unknown
        };
    }

    /// <summary>
    /// Creates a copy of this file with a new ID and name.
    /// </summary>
    public VirtualFile Copy(int newFileId, string newFileName)
    {
        return new VirtualFile(newFileId, newFileName, FilePath, Content, IsReadOnly, IsHidden)
        {
            ModifiedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Gets a string representation of the file.
    /// </summary>
    public override string ToString()
    {
        return $"{FullPath} ({SizeKB:F2} KB) - {Type}";
    }
}

/// <summary>
/// Defines the types of files in the virtual file system.
/// </summary>
public enum FileType
{
    Unknown,
    Text,
    Document,
    Image,
    Audio,
    Video,
    Archive,
    Executable
}
