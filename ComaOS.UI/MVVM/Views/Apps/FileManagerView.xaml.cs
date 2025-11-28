using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// File Manager application with virtual filesystem navigation.
/// </summary>
public partial class FileManagerView : UserControl
{
    private string _currentPath = "/";
    private readonly Stack<string> _navigationHistory = new();
    private readonly ObservableCollection<FileItem> _items = new();
    
    // Virtual file system
    private readonly Dictionary<string, VirtualFolder> _fileSystem = new();

    public FileManagerView()
    {
        InitializeComponent();
        FileList.ItemsSource = _items;
        InitializeFileSystem();
    }

    private void InitializeFileSystem()
    {
        // Create virtual file system structure
        _fileSystem["/"] = new VirtualFolder("/")
        {
            Folders = { "System", "Users", "Documents", "Downloads", "Programs", "Temp" },
            Files =
            {
                new VirtualFile("readme.txt", 1024, DateTime.Now.AddDays(-5)),
                new VirtualFile("config.sys", 256, DateTime.Now.AddDays(-30))
            }
        };

        _fileSystem["/System"] = new VirtualFolder("/System")
        {
            Folders = { "Kernel", "Drivers", "Config" },
            Files =
            {
                new VirtualFile("kernel.bin", 2048576, DateTime.Now.AddDays(-100)),
                new VirtualFile("boot.cfg", 512, DateTime.Now.AddDays(-100)),
                new VirtualFile("system.log", 8192, DateTime.Now)
            }
        };

        _fileSystem["/System/Kernel"] = new VirtualFolder("/System/Kernel")
        {
            Files =
            {
                new VirtualFile("scheduler.dll", 65536, DateTime.Now.AddDays(-100)),
                new VirtualFile("memory.dll", 32768, DateTime.Now.AddDays(-100)),
                new VirtualFile("process.dll", 49152, DateTime.Now.AddDays(-100))
            }
        };

        _fileSystem["/System/Drivers"] = new VirtualFolder("/System/Drivers")
        {
            Files =
            {
                new VirtualFile("display.drv", 16384, DateTime.Now.AddDays(-50)),
                new VirtualFile("keyboard.drv", 8192, DateTime.Now.AddDays(-50)),
                new VirtualFile("mouse.drv", 4096, DateTime.Now.AddDays(-50)),
                new VirtualFile("storage.drv", 24576, DateTime.Now.AddDays(-50))
            }
        };

        _fileSystem["/System/Config"] = new VirtualFolder("/System/Config")
        {
            Files =
            {
                new VirtualFile("settings.json", 2048, DateTime.Now.AddDays(-1)),
                new VirtualFile("users.db", 4096, DateTime.Now.AddDays(-10))
            }
        };

        _fileSystem["/Users"] = new VirtualFolder("/Users")
        {
            Folders = { "coma", "guest" }
        };

        _fileSystem["/Users/coma"] = new VirtualFolder("/Users/coma")
        {
            Folders = { "Desktop", "Documents", "Pictures", "Music" },
            Files =
            {
                new VirtualFile("profile.png", 32768, DateTime.Now.AddDays(-20)),
                new VirtualFile(".bashrc", 512, DateTime.Now.AddDays(-5))
            }
        };

        _fileSystem["/Users/coma/Desktop"] = new VirtualFolder("/Users/coma/Desktop")
        {
            Files =
            {
                new VirtualFile("todo.txt", 256, DateTime.Now),
                new VirtualFile("notes.txt", 1024, DateTime.Now.AddDays(-2)),
                new VirtualFile("project.zip", 1048576, DateTime.Now.AddDays(-7))
            }
        };

        _fileSystem["/Users/coma/Documents"] = new VirtualFolder("/Users/coma/Documents")
        {
            Files =
            {
                new VirtualFile("report.docx", 65536, DateTime.Now.AddDays(-3)),
                new VirtualFile("presentation.pptx", 2097152, DateTime.Now.AddDays(-14)),
                new VirtualFile("data.xlsx", 131072, DateTime.Now.AddDays(-7))
            }
        };

        _fileSystem["/Users/coma/Pictures"] = new VirtualFolder("/Users/coma/Pictures")
        {
            Files =
            {
                new VirtualFile("vacation.jpg", 3145728, DateTime.Now.AddDays(-60)),
                new VirtualFile("cat.png", 524288, DateTime.Now.AddDays(-30)),
                new VirtualFile("screenshot.png", 262144, DateTime.Now.AddDays(-1))
            }
        };

        _fileSystem["/Users/coma/Music"] = new VirtualFolder("/Users/coma/Music")
        {
            Files =
            {
                new VirtualFile("favorite_song.mp3", 5242880, DateTime.Now.AddDays(-45)),
                new VirtualFile("podcast.mp3", 52428800, DateTime.Now.AddDays(-2))
            }
        };

        _fileSystem["/Users/guest"] = new VirtualFolder("/Users/guest")
        {
            Files =
            {
                new VirtualFile("welcome.txt", 128, DateTime.Now.AddDays(-100))
            }
        };

        _fileSystem["/Documents"] = new VirtualFolder("/Documents")
        {
            Files =
            {
                new VirtualFile("readme.md", 2048, DateTime.Now.AddDays(-10)),
                new VirtualFile("license.txt", 1024, DateTime.Now.AddDays(-100)),
                new VirtualFile("changelog.txt", 4096, DateTime.Now)
            }
        };

        _fileSystem["/Downloads"] = new VirtualFolder("/Downloads")
        {
            Files =
            {
                new VirtualFile("setup.exe", 10485760, DateTime.Now.AddDays(-1)),
                new VirtualFile("movie.mp4", 734003200, DateTime.Now.AddDays(-5)),
                new VirtualFile("archive.zip", 52428800, DateTime.Now.AddDays(-3))
            }
        };

        _fileSystem["/Programs"] = new VirtualFolder("/Programs")
        {
            Folders = { "ComaOS", "Games", "Utilities" },
            Files =
            {
                new VirtualFile("install.log", 8192, DateTime.Now.AddDays(-50))
            }
        };

        _fileSystem["/Programs/ComaOS"] = new VirtualFolder("/Programs/ComaOS")
        {
            Files =
            {
                new VirtualFile("comaos.exe", 8388608, DateTime.Now.AddDays(-1)),
                new VirtualFile("core.dll", 2097152, DateTime.Now.AddDays(-1)),
                new VirtualFile("ui.dll", 4194304, DateTime.Now.AddDays(-1))
            }
        };

        _fileSystem["/Programs/Games"] = new VirtualFolder("/Programs/Games")
        {
            Files =
            {
                new VirtualFile("minesweeper.exe", 524288, DateTime.Now.AddDays(-100)),
                new VirtualFile("solitaire.exe", 786432, DateTime.Now.AddDays(-100))
            }
        };

        _fileSystem["/Programs/Utilities"] = new VirtualFolder("/Programs/Utilities")
        {
            Files =
            {
                new VirtualFile("calculator.exe", 131072, DateTime.Now.AddDays(-100)),
                new VirtualFile("notepad.exe", 262144, DateTime.Now.AddDays(-100)),
                new VirtualFile("terminal.exe", 393216, DateTime.Now.AddDays(-100))
            }
        };

        _fileSystem["/Temp"] = new VirtualFolder("/Temp")
        {
            Files =
            {
                new VirtualFile("~temp001.tmp", 1024, DateTime.Now),
                new VirtualFile("~temp002.tmp", 2048, DateTime.Now),
                new VirtualFile("cache.dat", 16384, DateTime.Now)
            }
        };
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        NavigateTo("/");
    }

    private void NavigateTo(string path)
    {
        if (!_fileSystem.ContainsKey(path))
        {
            StatusText.Text = $"Path not found: {path}";
            return;
        }

        if (_currentPath != path)
        {
            _navigationHistory.Push(_currentPath);
        }

        _currentPath = path;
        PathDisplay.Text = path;
        RefreshList();
    }

    private void RefreshList()
    {
        _items.Clear();

        if (_fileSystem.TryGetValue(_currentPath, out var folder))
        {
            // Add folders first
            foreach (var folderName in folder.Folders.OrderBy(f => f))
            {
                _items.Add(new FileItem
                {
                    Icon = "📁",
                    Name = folderName,
                    Size = "",
                    Modified = "",
                    IsFolder = true,
                    FullPath = CombinePath(_currentPath, folderName)
                });
            }

            // Add files
            foreach (var file in folder.Files.OrderBy(f => f.Name))
            {
                _items.Add(new FileItem
                {
                    Icon = GetFileIcon(file.Name),
                    Name = file.Name,
                    Size = FormatSize(file.Size),
                    Modified = file.Modified.ToString("MMM d, yyyy"),
                    IsFolder = false,
                    FullPath = CombinePath(_currentPath, file.Name)
                });
            }
        }

        ItemCount.Text = $"{_items.Count} items";
        StatusText.Text = "Ready";
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationHistory.Count > 0)
        {
            var previousPath = _navigationHistory.Pop();
            _currentPath = ""; // Reset to force navigation
            _navigationHistory.Pop(); // Remove the one that NavigateTo will add
            NavigateTo(previousPath);
        }
    }

    private void Up_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPath != "/")
        {
            var lastSlash = _currentPath.LastIndexOf('/');
            var parentPath = lastSlash <= 0 ? "/" : _currentPath.Substring(0, lastSlash);
            NavigateTo(parentPath);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        RefreshList();
        StatusText.Text = "Refreshed";
    }

    private void NewFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputDialog("New Folder", "Enter folder name:");
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ResponseText))
        {
            var folderName = dialog.ResponseText.Trim();
            if (_fileSystem.TryGetValue(_currentPath, out var folder))
            {
                if (!folder.Folders.Contains(folderName))
                {
                    folder.Folders.Add(folderName);
                    var newPath = CombinePath(_currentPath, folderName);
                    _fileSystem[newPath] = new VirtualFolder(newPath);
                    RefreshList();
                    StatusText.Text = $"Created folder: {folderName}";
                }
                else
                {
                    MessageBox.Show("A folder with that name already exists!", "ComaOS Files", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    private void NewFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputDialog("New File", "Enter file name:");
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ResponseText))
        {
            var fileName = dialog.ResponseText.Trim();
            if (!fileName.Contains('.'))
            {
                fileName += ".txt";
            }
            
            if (_fileSystem.TryGetValue(_currentPath, out var folder))
            {
                if (!folder.Files.Any(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    folder.Files.Add(new VirtualFile(fileName, 0, DateTime.Now));
                    RefreshList();
                    StatusText.Text = $"Created file: {fileName}";
                }
                else
                {
                    MessageBox.Show("A file with that name already exists!", "ComaOS Files", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (FileList.SelectedItem is FileItem item)
        {
            var result = MessageBox.Show($"Delete '{item.Name}'?", "Confirm Delete", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                if (_fileSystem.TryGetValue(_currentPath, out var folder))
                {
                    if (item.IsFolder)
                    {
                        folder.Folders.Remove(item.Name);
                        // Also remove the folder from filesystem
                        _fileSystem.Remove(item.FullPath);
                    }
                    else
                    {
                        var file = folder.Files.FirstOrDefault(f => f.Name == item.Name);
                        if (file != null)
                        {
                            folder.Files.Remove(file);
                        }
                    }
                    RefreshList();
                    StatusText.Text = $"Deleted: {item.Name}";
                }
            }
        }
        else
        {
            MessageBox.Show("Select an item to delete!", "ComaOS Files", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void FileList_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FileList.SelectedItem is FileItem item)
        {
            if (item.IsFolder)
            {
                NavigateTo(item.FullPath);
            }
            else
            {
                // Open file (show info)
                StatusText.Text = $"Opening {item.Name}...";
                MessageBox.Show($"📄 {item.Name}\n\nSize: {item.Size}\nModified: {item.Modified}\n\n(File preview not available)", 
                    "File Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private static string CombinePath(string basePath, string name)
    {
        if (basePath == "/")
            return "/" + name;
        return basePath + "/" + name;
    }

    private static string FormatSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1048576) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1073741824) return $"{bytes / 1048576.0:F1} MB";
        return $"{bytes / 1073741824.0:F2} GB";
    }

    private static string GetFileIcon(string fileName)
    {
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".txt" or ".md" or ".log" => "📄",
            ".doc" or ".docx" => "📝",
            ".xls" or ".xlsx" => "📊",
            ".ppt" or ".pptx" => "📽️",
            ".pdf" => "📕",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "🖼️",
            ".mp3" or ".wav" or ".flac" => "🎵",
            ".mp4" or ".avi" or ".mkv" or ".mov" => "🎬",
            ".zip" or ".rar" or ".7z" or ".tar" => "📦",
            ".exe" or ".msi" => "⚙️",
            ".dll" or ".so" => "🔧",
            ".json" or ".xml" or ".cfg" or ".ini" => "⚙️",
            ".html" or ".htm" => "🌐",
            ".css" => "🎨",
            ".js" or ".ts" => "📜",
            ".cs" or ".java" or ".py" or ".cpp" or ".c" => "💻",
            ".bin" or ".dat" => "💾",
            ".db" or ".sqlite" => "🗃️",
            ".tmp" => "📎",
            ".drv" => "🔌",
            _ => "📄"
        };
    }
}

public class FileItem
{
    public string Icon { get; set; } = "📄";
    public string Name { get; set; } = "";
    public string Size { get; set; } = "";
    public string Modified { get; set; } = "";
    public bool IsFolder { get; set; }
    public string FullPath { get; set; } = "";
}

public class VirtualFolder
{
    public string Path { get; set; }
    public List<string> Folders { get; } = new();
    public List<VirtualFile> Files { get; } = new();

    public VirtualFolder(string path)
    {
        Path = path;
    }
}

public class VirtualFile
{
    public string Name { get; set; }
    public long Size { get; set; }
    public DateTime Modified { get; set; }

    public VirtualFile(string name, long size, DateTime modified)
    {
        Name = name;
        Size = size;
        Modified = modified;
    }
}
