using System.Collections.ObjectModel;
using System.Windows.Input;
using ComaOS.Core.Apps;

namespace ComaOS.UI.MVVM.ViewModels;

/// <summary>
/// ViewModel for the taskbar component.
/// </summary>
public class TaskbarViewModel : BaseViewModel
{
    private readonly MainViewModel _mainViewModel;
    private string _currentTime = string.Empty;
    private string _currentDate = string.Empty;
    private bool _isStartMenuOpen;

    /// <summary>
    /// Gets the current time display.
    /// </summary>
    public string CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
    }

    /// <summary>
    /// Gets the current date display.
    /// </summary>
    public string CurrentDate
    {
        get => _currentDate;
        set => SetProperty(ref _currentDate, value);
    }

    /// <summary>
    /// Gets or sets whether the start menu is open.
    /// </summary>
    public bool IsStartMenuOpen
    {
        get => _isStartMenuOpen;
        set => SetProperty(ref _isStartMenuOpen, value);
    }

    /// <summary>
    /// Gets the collection of taskbar items.
    /// </summary>
    public ObservableCollection<TaskbarItemViewModel> TaskbarItems => _mainViewModel.TaskbarItems;

    /// <summary>
    /// Gets the collection of start menu apps.
    /// </summary>
    public ObservableCollection<StartMenuAppViewModel> StartMenuApps { get; } = new();

    // Commands
    public ICommand ToggleStartMenuCommand { get; }
    public ICommand LaunchAppCommand { get; }

    /// <summary>
    /// Initializes a new instance of TaskbarViewModel.
    /// </summary>
    public TaskbarViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;

        ToggleStartMenuCommand = new RelayCommand(_ => IsStartMenuOpen = !IsStartMenuOpen);
        LaunchAppCommand = new RelayCommand<ApplicationType?>(type =>
        {
            if (type.HasValue)
            {
                _mainViewModel.LaunchAppCommand.Execute(type.Value);
                IsStartMenuOpen = false;
            }
        });

        InitializeStartMenu();
        UpdateTime();
    }

    /// <summary>
    /// Initializes start menu applications.
    /// </summary>
    private void InitializeStartMenu()
    {
        var apps = new (ApplicationType Type, string Icon, string Name, string Category)[]
        {
            (ApplicationType.Notepad, "📝", "Notepad", "Productivity"),
            (ApplicationType.Calculator, "🔢", "Calculator", "Productivity"),
            (ApplicationType.Calendar, "📅", "Calendar", "Productivity"),
            (ApplicationType.FileManager, "📁", "File Manager", "System"),
            (ApplicationType.Terminal, "💻", "Terminal", "System"),
            (ApplicationType.Settings, "⚙️", "Settings", "System"),
            (ApplicationType.SystemMonitor, "📊", "System Monitor", "System"),
            (ApplicationType.Browser, "🌐", "Browser", "Internet"),
            (ApplicationType.MusicPlayer, "🎵", "Music Player", "Media"),
            (ApplicationType.VideoPlayer, "🎬", "Video Player", "Media"),
            (ApplicationType.ImageViewer, "🖼️", "Image Viewer", "Media"),
            (ApplicationType.Minesweeper, "💣", "Minesweeper", "Games"),
            (ApplicationType.Antivirus, "🛡️", "Antivirus", "Security"),
            (ApplicationType.CompressionTool, "📦", "Compression Tool", "Utilities"),
            (ApplicationType.Clock, "🕐", "Clock", "Utilities")
        };

        foreach (var (type, icon, name, category) in apps)
        {
            StartMenuApps.Add(new StartMenuAppViewModel
            {
                AppType = type,
                Icon = icon,
                Name = name,
                Category = category
            });
        }
    }

    /// <summary>
    /// Updates the time display.
    /// </summary>
    public void UpdateTime()
    {
        var now = DateTime.Now;
        CurrentTime = now.ToString("HH:mm");
        CurrentDate = now.ToString("ddd, MMM d");
    }
}

/// <summary>
/// ViewModel for start menu application items.
/// </summary>
public class StartMenuAppViewModel : BaseViewModel
{
    public ApplicationType AppType { get; set; }
    public string Icon { get; set; } = "📄";
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
