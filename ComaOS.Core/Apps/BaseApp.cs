namespace ComaOS.Core.Apps;

/// <summary>
/// Abstract base class for all applications in ComaOS.
/// All tasks/apps must inherit from this class.
/// </summary>
public abstract class BaseApp
{
    /// <summary>
    /// Gets the application name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the application description.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the priority level (1 = High, 2 = Normal/Background).
    /// </summary>
    public abstract int Priority { get; }

    /// <summary>
    /// Gets the RAM requirement in megabytes.
    /// </summary>
    public abstract long RamRequirementMB { get; }

    /// <summary>
    /// Gets the estimated execution time in milliseconds.
    /// </summary>
    public abstract int ExecutionTimeMs { get; }

    /// <summary>
    /// Gets whether this is a background application.
    /// </summary>
    public virtual bool IsBackgroundApp => Priority == ProcessPriority.Normal;

    /// <summary>
    /// Gets the application type.
    /// </summary>
    public abstract ApplicationType AppType { get; }

    /// <summary>
    /// Executes the application logic.
    /// This method simulates the work performed by the application.
    /// </summary>
    public abstract Task ExecuteAsync();

    /// <summary>
    /// Gets the application icon (unicode character or emoji).
    /// </summary>
    public virtual string Icon => "??";

    /// <summary>
    /// Gets whether the application can be closed by the user.
    /// </summary>
    public virtual bool CanClose => true;
}

/// <summary>
/// Defines priority constants for applications.
/// </summary>
public static class ProcessPriority
{
    public const int High = 1;
    public const int Normal = 2;
}

/// <summary>
/// Enumeration of all available application types in ComaOS.
/// </summary>
public enum ApplicationType
{
    Notepad,
    Calculator,
    Clock,
    Calendar,
    FileManager,
    SystemMonitor,
    Minesweeper,
    MusicPlayer,
    VideoPlayer,
    Browser,
    Terminal,
    ImageViewer,
    Antivirus,
    CompressionTool,
    Settings
}
