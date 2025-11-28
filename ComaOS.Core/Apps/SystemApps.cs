using ComaOS.Core.FileSystem;

namespace ComaOS.Core.Apps;

/// <summary>
/// Factory class for creating application instances.
/// </summary>
public class ProcessFactory
{
    private readonly FileManager _fileManager;

    /// <summary>
    /// Initializes a new instance of ProcessFactory.
    /// </summary>
    public ProcessFactory(FileManager fileManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    }

    /// <summary>
    /// Creates an application instance based on the specified type.
    /// </summary>
    /// <param name="appType">The type of application to create.</param>
    /// <returns>An instance of the application.</returns>
    public BaseApp CreateApplication(ApplicationType appType)
    {
        return appType switch
        {
            ApplicationType.Notepad => new NotepadApp(_fileManager),
            ApplicationType.Calculator => new CalculatorApp(),
            ApplicationType.Clock => new ClockApp(),
            ApplicationType.Calendar => new CalendarApp(),
            ApplicationType.FileManager => new FileManagerApp(_fileManager),
            ApplicationType.SystemMonitor => new SystemMonitorApp(),
            ApplicationType.Minesweeper => new MinesweeperApp(),
            ApplicationType.MusicPlayer => new MusicPlayerApp(),
            ApplicationType.VideoPlayer => new VideoPlayerApp(),
            ApplicationType.Browser => new BrowserApp(),
            ApplicationType.Terminal => new TerminalApp(),
            ApplicationType.ImageViewer => new ImageViewerApp(),
            ApplicationType.Antivirus => new AntivirusApp(_fileManager),
            ApplicationType.CompressionTool => new CompressionToolApp(),
            ApplicationType.Settings => new SettingsApp(),
            _ => throw new ArgumentException($"Unknown application type: {appType}", nameof(appType))
        };
    }
}

// ==================== SYSTEM APPLICATIONS ====================

/// <summary>
/// Notepad application for text editing with auto-save functionality.
/// </summary>
public class NotepadApp : BaseApp
{
    private readonly FileManager _fileManager;
    public override string Name => "Notepad";
    public override string Description => "Text editor with auto-save";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 64;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.Notepad;
    public override string Icon => "📝";

    public NotepadApp(FileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public override async Task ExecuteAsync()
    {
        // Interactive app - stays open indefinitely until user closes
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Calculator application for mathematical operations.
/// </summary>
public class CalculatorApp : BaseApp
{
    public override string Name => "Calculator";
    public override string Description => "Perform mathematical calculations";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 32;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.Calculator;
    public override string Icon => "🔢";

    public override async Task ExecuteAsync()
    {
        // Interactive app - stays open indefinitely
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Clock application that runs continuously.
/// </summary>
public class ClockApp : BaseApp
{
    public override string Name => "Clock";
    public override string Description => "System clock (auto-runs on boot)";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 16;
    public override int ExecutionTimeMs => int.MaxValue; // Runs indefinitely
    public override ApplicationType AppType => ApplicationType.Clock;
    public override string Icon => "🕐";
    public override bool CanClose => false; // System app

    public override async Task ExecuteAsync()
    {
        // Simulate continuous clock updates
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Calendar application for date management.
/// </summary>
public class CalendarApp : BaseApp
{
    public override string Name => "Calendar";
    public override string Description => "View and manage dates";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 48;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.Calendar;
    public override string Icon => "📅";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// File Manager application for file operations.
/// </summary>
public class FileManagerApp : BaseApp
{
    private readonly FileManager _fileManager;
    public override string Name => "File Manager";
    public override string Description => "Browse and manage files";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 128;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.FileManager;
    public override string Icon => "📁";

    public FileManagerApp(FileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// System Monitor application for monitoring resources.
/// </summary>
public class SystemMonitorApp : BaseApp
{
    public override string Name => "System Monitor";
    public override string Description => "Monitor RAM/CPU usage";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 96;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.SystemMonitor;
    public override string Icon => "📊";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Minesweeper game application.
/// </summary>
public class MinesweeperApp : BaseApp
{
    public override string Name => "Minesweeper";
    public override string Description => "Classic puzzle game";
    public override int Priority => ProcessPriority.High; // Game = high priority
    public override long RamRequirementMB => 128;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.Minesweeper;
    public override string Icon => "💣";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Music Player application (background task).
/// </summary>
public class MusicPlayerApp : BaseApp
{
    public override string Name => "Music Player";
    public override string Description => "Play music files";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 192;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.MusicPlayer;
    public override string Icon => "🎵";
    public override bool IsBackgroundApp => true;

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Video Player application (heavy resource usage).
/// </summary>
public class VideoPlayerApp : BaseApp
{
    public override string Name => "Video Player";
    public override string Description => "Play video files";
    public override int Priority => ProcessPriority.High; // Real-time
    public override long RamRequirementMB => 512;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.VideoPlayer;
    public override string Icon => "🎬";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Web Browser application.
/// </summary>
public class BrowserApp : BaseApp
{
    public override string Name => "Browser";
    public override string Description => "Simulate web requests";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 256;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.Browser;
    public override string Icon => "🌐";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Terminal application.
/// </summary>
public class TerminalApp : BaseApp
{
    public override string Name => "Terminal";
    public override string Description => "Command-line interface";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 64;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.Terminal;
    public override string Icon => "💻";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Image Viewer application.
/// </summary>
public class ImageViewerApp : BaseApp
{
    public override string Name => "Image Viewer";
    public override string Description => "View image files";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 128;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.ImageViewer;
    public override string Icon => "🖼️";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Antivirus application that scans the file system.
/// </summary>
public class AntivirusApp : BaseApp
{
    private readonly FileManager _fileManager;
    public override string Name => "Antivirus";
    public override string Description => "Scan and protect files";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 256;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.Antivirus;
    public override string Icon => "🛡️";
    public override bool IsBackgroundApp => true;

    public AntivirusApp(FileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Compression Tool application for file compression.
/// </summary>
public class CompressionToolApp : BaseApp
{
    public override string Name => "Compression Tool";
    public override string Description => "Compress and extract files";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 192;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.CompressionTool;
    public override string Icon => "📦";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}

/// <summary>
/// Settings application for OS configuration.
/// </summary>
public class SettingsApp : BaseApp
{
    public override string Name => "Settings";
    public override string Description => "Configure OS settings";
    public override int Priority => ProcessPriority.Normal;
    public override long RamRequirementMB => 96;
    public override int ExecutionTimeMs => int.MaxValue; // Stays open until user closes
    public override ApplicationType AppType => ApplicationType.Settings;
    public override string Icon => "⚙️";

    public override async Task ExecuteAsync()
    {
        await Task.Delay(Timeout.Infinite);
    }
}
