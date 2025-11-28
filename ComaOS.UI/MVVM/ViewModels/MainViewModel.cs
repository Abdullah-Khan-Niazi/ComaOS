using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ComaOS.Core.Apps;
using ComaOS.Core.Kernel;

namespace ComaOS.UI.MVVM.ViewModels;

/// <summary>
/// Main ViewModel for the ComaOS desktop interface.
/// Manages the kernel, running applications, and system state.
/// </summary>
public class MainViewModel : BaseViewModel
{
    private KernelManager? _kernel;
    private readonly DispatcherTimer _updateTimer;
    private bool _isBooted;
    private bool _isBooting;
    private int _bootProgress;
    private string _bootMessage = string.Empty;
    private string _currentTime = string.Empty;
    private string _currentDate = string.Empty;
    private OperatingMode _currentMode;
    private double _cpuUsage;
    private double _ramUsage;
    private long _ramUsed;
    private long _ramTotal;

    // Boot configuration
    private long _ramSizeMB = 4096;
    private long _hddSizeGB = 500;
    private int _coreCount = 4;

    /// <summary>
    /// Gets or sets the RAM size for boot configuration.
    /// </summary>
    public long RamSizeMB
    {
        get => _ramSizeMB;
        set => SetProperty(ref _ramSizeMB, Math.Clamp(value, 512, 65536));
    }

    /// <summary>
    /// Gets or sets the HDD size for boot configuration.
    /// </summary>
    public long HddSizeGB
    {
        get => _hddSizeGB;
        set => SetProperty(ref _hddSizeGB, Math.Clamp(value, 10, 10240));
    }

    /// <summary>
    /// Gets or sets the core count for boot configuration.
    /// </summary>
    public int CoreCount
    {
        get => _coreCount;
        set => SetProperty(ref _coreCount, Math.Clamp(value, 1, 64));
    }

    /// <summary>
    /// Gets whether the OS has booted.
    /// </summary>
    public bool IsBooted
    {
        get => _isBooted;
        private set => SetProperty(ref _isBooted, value);
    }

    /// <summary>
    /// Gets whether the OS is currently booting.
    /// </summary>
    public bool IsBooting
    {
        get => _isBooting;
        private set => SetProperty(ref _isBooting, value);
    }

    /// <summary>
    /// Gets the boot progress percentage.
    /// </summary>
    public int BootProgress
    {
        get => _bootProgress;
        private set => SetProperty(ref _bootProgress, value);
    }

    /// <summary>
    /// Gets the current boot message.
    /// </summary>
    public string BootMessage
    {
        get => _bootMessage;
        private set => SetProperty(ref _bootMessage, value);
    }

    /// <summary>
    /// Gets the current time display.
    /// </summary>
    public string CurrentTime
    {
        get => _currentTime;
        private set => SetProperty(ref _currentTime, value);
    }

    /// <summary>
    /// Gets the current date display.
    /// </summary>
    public string CurrentDate
    {
        get => _currentDate;
        private set => SetProperty(ref _currentDate, value);
    }

    /// <summary>
    /// Gets the current operating mode.
    /// </summary>
    public OperatingMode CurrentMode
    {
        get => _currentMode;
        private set => SetProperty(ref _currentMode, value);
    }

    /// <summary>
    /// Gets the CPU usage percentage.
    /// </summary>
    public double CpuUsage
    {
        get => _cpuUsage;
        private set => SetProperty(ref _cpuUsage, value);
    }

    /// <summary>
    /// Gets the RAM usage percentage.
    /// </summary>
    public double RamUsage
    {
        get => _ramUsage;
        private set => SetProperty(ref _ramUsage, value);
    }

    /// <summary>
    /// Gets the used RAM in MB.
    /// </summary>
    public long RamUsed
    {
        get => _ramUsed;
        private set => SetProperty(ref _ramUsed, value);
    }

    /// <summary>
    /// Gets the total RAM in MB.
    /// </summary>
    public long RamTotal
    {
        get => _ramTotal;
        private set => SetProperty(ref _ramTotal, value);
    }

    /// <summary>
    /// Gets the collection of desktop applications.
    /// </summary>
    public ObservableCollection<DesktopAppViewModel> DesktopApps { get; } = new();

    /// <summary>
    /// Gets the collection of running windows.
    /// </summary>
    public ObservableCollection<WindowViewModel> OpenWindows { get; } = new();

    /// <summary>
    /// Gets the collection of taskbar items.
    /// </summary>
    public ObservableCollection<TaskbarItemViewModel> TaskbarItems { get; } = new();

    /// <summary>
    /// Gets the collection of running processes for display.
    /// </summary>
    public ObservableCollection<ProcessViewModel> Processes { get; } = new();

    // Commands
    public ICommand BootCommand { get; }
    public ICommand ShutdownCommand { get; }
    public ICommand LaunchAppCommand { get; }
    public ICommand CloseWindowCommand { get; }
    public ICommand MinimizeWindowCommand { get; }
    public ICommand MaximizeWindowCommand { get; }
    public ICommand ToggleModeCommand { get; }
    public ICommand OpenStartMenuCommand { get; }

    private bool _isStartMenuOpen;
    public bool IsStartMenuOpen
    {
        get => _isStartMenuOpen;
        set => SetProperty(ref _isStartMenuOpen, value);
    }

    private bool _isDarkMode = true;
    /// <summary>
    /// Gets or sets whether dark mode is enabled.
    /// </summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set => SetProperty(ref _isDarkMode, value);
    }

    /// <summary>
    /// Initializes a new instance of MainViewModel.
    /// </summary>
    public MainViewModel()
    {
        BootCommand = new RelayCommand(_ => Boot(), _ => !IsBooted && !IsBooting);
        ShutdownCommand = new RelayCommand(_ => Shutdown(), _ => IsBooted);
        LaunchAppCommand = new RelayCommand<ApplicationType?>(LaunchApp, _ => IsBooted);
        CloseWindowCommand = new RelayCommand<WindowViewModel>(CloseWindow);
        MinimizeWindowCommand = new RelayCommand<WindowViewModel>(MinimizeWindow);
        MaximizeWindowCommand = new RelayCommand<WindowViewModel>(MaximizeWindow);
        ToggleModeCommand = new RelayCommand(_ => ToggleMode(), _ => IsBooted);
        OpenStartMenuCommand = new RelayCommand(_ => IsStartMenuOpen = !IsStartMenuOpen);

        // Setup update timer
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _updateTimer.Tick += UpdateTimer_Tick;

        // Initialize time
        UpdateTime();

        // Initialize desktop apps
        InitializeDesktopApps();
    }

    /// <summary>
    /// Initializes the desktop application icons.
    /// </summary>
    private void InitializeDesktopApps()
    {
        DesktopApps.Clear();

        var apps = new (ApplicationType Type, string Icon, string Name)[]
        {
            (ApplicationType.Notepad, "📝", "Notepad"),
            (ApplicationType.Calculator, "🔢", "Calculator"),
            (ApplicationType.FileManager, "📁", "Files"),
            (ApplicationType.Browser, "🌐", "Browser"),
            (ApplicationType.Terminal, "💻", "Terminal"),
            (ApplicationType.Settings, "⚙️", "Settings"),
            (ApplicationType.SystemMonitor, "📊", "System Monitor"),
            (ApplicationType.MusicPlayer, "🎵", "Music"),
            (ApplicationType.VideoPlayer, "🎬", "Video"),
            (ApplicationType.ImageViewer, "🖼️", "Images"),
            (ApplicationType.Minesweeper, "💣", "Minesweeper"),
            (ApplicationType.Calendar, "📅", "Calendar"),
            (ApplicationType.Antivirus, "🛡️", "Antivirus"),
            (ApplicationType.CompressionTool, "📦", "Compress"),
            (ApplicationType.Clock, "🕐", "Clock")
        };

        foreach (var (type, icon, name) in apps)
        {
            DesktopApps.Add(new DesktopAppViewModel
            {
                AppType = type,
                Icon = icon,
                Name = name,
                LaunchCommand = new RelayCommand(_ => LaunchApp(type), _ => IsBooted)
            });
        }
    }

    /// <summary>
    /// Boots the ComaOS kernel.
    /// </summary>
    private async void Boot()
    {
        if (IsBooting || IsBooted) return;

        IsBooting = true;
        BootProgress = 0;
        BootMessage = "Initializing...";

        try
        {
            _kernel = new KernelManager(RamSizeMB, HddSizeGB, CoreCount);

            _kernel.GetBootLoader().BootProgress += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BootProgress = e.Percentage;
                    BootMessage = e.Message;
                });
            };

            var result = await _kernel.BootAsync();

            if (result.Success)
            {
                IsBooted = true;
                CurrentMode = _kernel.CurrentMode;
                RamTotal = _kernel.RAM.TotalSizeMB;
                _updateTimer.Start();
            }
            else
            {
                MessageBox.Show($"Boot failed: {result.Message}", "ComaOS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Boot error: {ex.Message}", "ComaOS", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBooting = false;
        }
    }

    /// <summary>
    /// Shuts down the ComaOS kernel.
    /// </summary>
    private async void Shutdown()
    {
        if (_kernel == null || !IsBooted) return;

        _updateTimer.Stop();

        // Close all windows
        OpenWindows.Clear();
        TaskbarItems.Clear();
        Processes.Clear();

        await _kernel.ShutdownAsync();

        IsBooted = false;
        _kernel = null;
        CpuUsage = 0;
        RamUsage = 0;
        RamUsed = 0;
    }

    /// <summary>
    /// Launches an application.
    /// </summary>
    private void LaunchApp(ApplicationType? appType)
    {
        if (_kernel == null || !IsBooted || !appType.HasValue) return;

        IsStartMenuOpen = false;

        int pid = _kernel.StartApplication(appType.Value);

        if (pid > 0)
        {
            var process = _kernel.Scheduler.GetProcessById(pid);
            if (process != null)
            {
                var window = new WindowViewModel
                {
                    ProcessId = pid,
                    Title = process.ProcessName,
                    AppName = process.ProcessName,
                    AppType = appType.Value,
                    Icon = GetAppIcon(appType.Value),
                    IsMinimized = false,
                    IsMaximized = false,
                    Width = 600,
                    Height = 400,
                    Left = 50 + (OpenWindows.Count * 30) % 200,
                    Top = 50 + (OpenWindows.Count * 30) % 150
                };

                OpenWindows.Add(window);

                TaskbarItems.Add(new TaskbarItemViewModel
                {
                    ProcessId = pid,
                    Title = process.ProcessName,
                    Icon = window.Icon,
                    Window = window,
                    ActivateCommand = new RelayCommand(_ => ActivateWindow(window))
                });
            }
        }
        else
        {
            MessageBox.Show($"Failed to start {appType.Value}. Insufficient RAM?", "ComaOS", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Closes a window and terminates its process.
    /// </summary>
    private void CloseWindow(WindowViewModel? window)
    {
        if (window == null || _kernel == null) return;

        // Remove from taskbar
        var taskbarItem = TaskbarItems.FirstOrDefault(t => t.ProcessId == window.ProcessId);
        if (taskbarItem != null)
            TaskbarItems.Remove(taskbarItem);

        // Remove window
        OpenWindows.Remove(window);

        // Terminate process (switch to kernel mode temporarily)
        var previousMode = _kernel.CurrentMode;
        _kernel.SwitchMode(OperatingMode.Kernel);
        _kernel.TerminateProcess(window.ProcessId);
        _kernel.SwitchMode(previousMode);
    }

    /// <summary>
    /// Minimizes a window.
    /// </summary>
    private void MinimizeWindow(WindowViewModel? window)
    {
        if (window == null) return;
        window.IsMinimized = true;
    }

    /// <summary>
    /// Maximizes or restores a window.
    /// </summary>
    private void MaximizeWindow(WindowViewModel? window)
    {
        if (window == null) return;
        window.IsMaximized = !window.IsMaximized;
    }

    /// <summary>
    /// Activates (focuses) a window.
    /// </summary>
    private void ActivateWindow(WindowViewModel window)
    {
        window.IsMinimized = false;
        // Bring to front by moving to end of collection
        if (OpenWindows.Contains(window))
        {
            OpenWindows.Remove(window);
            OpenWindows.Add(window);
        }
    }

    /// <summary>
    /// Toggles between User and Kernel mode.
    /// </summary>
    private void ToggleMode()
    {
        if (_kernel == null) return;

        var newMode = CurrentMode == OperatingMode.User ? OperatingMode.Kernel : OperatingMode.User;
        _kernel.SwitchMode(newMode);
        CurrentMode = newMode;
    }

    /// <summary>
    /// Timer tick handler for updating system status.
    /// </summary>
    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();
        UpdateSystemStatus();
        UpdateProcessList();
    }

    /// <summary>
    /// Updates the time display.
    /// </summary>
    private void UpdateTime()
    {
        var now = DateTime.Now;
        CurrentTime = now.ToString("HH:mm");
        CurrentDate = now.ToString("ddd, MMM d");
    }

    /// <summary>
    /// Updates system status (CPU, RAM).
    /// </summary>
    private void UpdateSystemStatus()
    {
        if (_kernel == null) return;

        CpuUsage = _kernel.CPU.GetCpuUsage();
        RamUsage = _kernel.RAM.UsagePercentage;
        RamUsed = _kernel.RAM.UsedSizeMB;
    }

    /// <summary>
    /// Updates the process list.
    /// </summary>
    private void UpdateProcessList()
    {
        if (_kernel == null) return;

        var activeProcesses = _kernel.Scheduler.AllActiveProcesses;

        // Update existing or add new
        foreach (var pcb in activeProcesses)
        {
            var existing = Processes.FirstOrDefault(p => p.ProcessId == pcb.ProcessId);
            if (existing != null)
            {
                existing.State = pcb.State;
                existing.Progress = pcb.ProgramCounter;
            }
            else
            {
                Processes.Add(new ProcessViewModel
                {
                    ProcessId = pcb.ProcessId,
                    Name = pcb.ProcessName,
                    State = pcb.State,
                    Priority = pcb.Priority,
                    RamUsage = pcb.RamUsageMB,
                    Progress = pcb.ProgramCounter
                });
            }
        }

        // Remove terminated processes from active view
        var toRemove = Processes.Where(p => !activeProcesses.Any(a => a.ProcessId == p.ProcessId)).ToList();
        foreach (var p in toRemove)
        {
            Processes.Remove(p);

            // Also close any associated windows
            var window = OpenWindows.FirstOrDefault(w => w.ProcessId == p.ProcessId);
            if (window != null)
            {
                CloseWindow(window);
            }
        }
    }

    /// <summary>
    /// Gets the icon for an application type.
    /// </summary>
    private static string GetAppIcon(ApplicationType appType)
    {
        return appType switch
        {
            ApplicationType.Notepad => "📝",
            ApplicationType.Calculator => "🔢",
            ApplicationType.Clock => "🕐",
            ApplicationType.Calendar => "📅",
            ApplicationType.FileManager => "📁",
            ApplicationType.SystemMonitor => "📊",
            ApplicationType.Minesweeper => "💣",
            ApplicationType.MusicPlayer => "🎵",
            ApplicationType.VideoPlayer => "🎬",
            ApplicationType.Browser => "🌐",
            ApplicationType.Terminal => "💻",
            ApplicationType.ImageViewer => "🖼️",
            ApplicationType.Antivirus => "🛡️",
            ApplicationType.CompressionTool => "📦",
            ApplicationType.Settings => "⚙️",
            _ => "📄"
        };
    }
}

/// <summary>
/// ViewModel for desktop application icons.
/// </summary>
public class DesktopAppViewModel : BaseViewModel
{
    public ApplicationType AppType { get; set; }
    public string Icon { get; set; } = "📄";
    public string Name { get; set; } = string.Empty;
    public ICommand? LaunchCommand { get; set; }
}

/// <summary>
/// ViewModel for open windows.
/// </summary>
public class WindowViewModel : BaseViewModel
{
    private bool _isMinimized;
    private bool _isMaximized;
    private double _left;
    private double _top;
    private double _width;
    private double _height;
    private string _title = string.Empty;

    public int ProcessId { get; set; }
    public ApplicationType AppType { get; set; }
    public string Icon { get; set; } = "📄";
    public string AppName { get; set; } = string.Empty;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public bool IsMinimized
    {
        get => _isMinimized;
        set => SetProperty(ref _isMinimized, value);
    }

    public bool IsMaximized
    {
        get => _isMaximized;
        set => SetProperty(ref _isMaximized, value);
    }

    public double Left
    {
        get => _left;
        set => SetProperty(ref _left, value);
    }

    public double Top
    {
        get => _top;
        set => SetProperty(ref _top, value);
    }

    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }
}

/// <summary>
/// ViewModel for taskbar items.
/// </summary>
public class TaskbarItemViewModel : BaseViewModel
{
    public int ProcessId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "📄";
    public WindowViewModel? Window { get; set; }
    public ICommand? ActivateCommand { get; set; }
}

/// <summary>
/// ViewModel for process display.
/// </summary>
public class ProcessViewModel : BaseViewModel
{
    private ProcessState _state;
    private int _progress;

    public int ProcessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
    public long RamUsage { get; set; }

    public ProcessState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }
}
