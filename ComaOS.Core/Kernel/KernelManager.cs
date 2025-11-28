using ComaOS.Core.Hardware;
using ComaOS.Core.FileSystem;
using ComaOS.Core.Apps;

namespace ComaOS.Core.Kernel;

/// <summary>
/// The main kernel manager that orchestrates all OS components and handles mode switching.
/// This is the primary entry point for the entire operating system logic.
/// </summary>
public class KernelManager
{
    private readonly CPU _cpu;
    private readonly RAM _ram;
    private readonly HardDrive _hardDrive;
    private readonly Scheduler _scheduler;
    private readonly FileManager _fileManager;
    private readonly BootLoader _bootLoader;
    private readonly ProcessFactory _processFactory;
    private int _nextProcessId = 1;
    private bool _isRunning;

    /// <summary>
    /// Gets the current operating mode (User or Kernel).
    /// </summary>
    public OperatingMode CurrentMode { get; private set; }

    /// <summary>
    /// Gets the CPU hardware component.
    /// </summary>
    public CPU CPU => _cpu;

    /// <summary>
    /// Gets the RAM hardware component.
    /// </summary>
    public RAM RAM => _ram;

    /// <summary>
    /// Gets the Hard Drive hardware component.
    /// </summary>
    public HardDrive HardDrive => _hardDrive;

    /// <summary>
    /// Gets the process scheduler.
    /// </summary>
    public Scheduler Scheduler => _scheduler;

    /// <summary>
    /// Gets the file system manager.
    /// </summary>
    public FileManager FileManager => _fileManager;

    /// <summary>
    /// Gets whether the kernel is currently running.
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Event raised when the operating mode changes.
    /// </summary>
    public event EventHandler<ModeChangedEventArgs>? ModeChanged;

    /// <summary>
    /// Initializes a new instance of the KernelManager.
    /// This constructor performs the hardware initialization.
    /// </summary>
    /// <param name="ramSizeMB">Total RAM size in megabytes.</param>
    /// <param name="hddSizeGB">Total hard drive size in gigabytes.</param>
    /// <param name="coreCount">Number of CPU cores.</param>
    public KernelManager(long ramSizeMB, long hddSizeGB, int coreCount)
    {
        try
        {
            // Initialize hardware components
            _cpu = new CPU(coreCount);
            _ram = new RAM(ramSizeMB);
            _hardDrive = new HardDrive(hddSizeGB);

            // Initialize kernel components
            _scheduler = new Scheduler(_cpu, _ram);
            _fileManager = new FileManager(_hardDrive);
            _bootLoader = new BootLoader();
            _processFactory = new ProcessFactory(_fileManager);

            // Start in kernel mode
            CurrentMode = OperatingMode.Kernel;
            _isRunning = false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize Kernel Manager: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Boots the operating system asynchronously.
    /// </summary>
    /// <returns>A task representing the boot operation result.</returns>
    public async Task<BootResult> BootAsync()
    {
        if (_isRunning)
            return new BootResult(false, "System is already running.");

        var bootResult = await _bootLoader.BootAsync(_ram.TotalSizeMB, _hardDrive.TotalSizeGB, _cpu.CoreCount);

        if (bootResult.Success)
        {
            _isRunning = true;
            _scheduler.Start();

            // Auto-start system applications
            await StartSystemAppsAsync();
        }

        return bootResult;
    }

    /// <summary>
    /// Boots the operating system synchronously.
    /// </summary>
    public BootResult Boot()
    {
        return BootAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Shuts down the operating system gracefully.
    /// </summary>
    public async Task ShutdownAsync()
    {
        if (!_isRunning)
            return;

        // Terminate all processes
        var activeProcesses = _scheduler.AllActiveProcesses.ToList();
        foreach (var process in activeProcesses)
        {
            _scheduler.TerminateProcess(process.ProcessId, isKernelMode: true);
        }

        // Stop the scheduler
        await _scheduler.StopAsync();

        _isRunning = false;
    }

    /// <summary>
    /// Switches the operating mode between User and Kernel.
    /// </summary>
    /// <param name="newMode">The mode to switch to.</param>
    public void SwitchMode(OperatingMode newMode)
    {
        if (CurrentMode == newMode)
            return;

        var oldMode = CurrentMode;
        CurrentMode = newMode;

        ModeChanged?.Invoke(this, new ModeChangedEventArgs(oldMode, newMode));
    }

    /// <summary>
    /// Starts a new application/process.
    /// </summary>
    /// <param name="appType">The type of application to start.</param>
    /// <returns>The Process ID if successful; -1 if failed.</returns>
    public int StartApplication(ApplicationType appType)
    {
        try
        {
            var app = _processFactory.CreateApplication(appType);
            var pcb = new ProcessControlBlock(
                processId: _nextProcessId++,
                processName: app.Name,
                priority: app.Priority,
                ramUsageMB: app.RamRequirementMB,
                executionTimeMs: app.ExecutionTimeMs,
                isBackgroundProcess: app.IsBackgroundApp
            );

            // Check if sufficient RAM is available
            if (!_ram.Allocate(pcb.ProcessId, pcb.RamUsageMB))
            {
                return -1; // Insufficient RAM
            }

            // Add to scheduler
            _scheduler.AddProcess(pcb);

            return pcb.ProcessId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start application: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Terminates a process. Requires Kernel Mode.
    /// </summary>
    /// <param name="processId">The Process ID to terminate.</param>
    /// <returns>True if successful; false otherwise.</returns>
    public bool TerminateProcess(int processId)
    {
        return _scheduler.TerminateProcess(processId, CurrentMode == OperatingMode.Kernel);
    }

    /// <summary>
    /// Forcefully clears all memory. Requires Kernel Mode.
    /// </summary>
    /// <returns>True if successful; false if not in Kernel Mode.</returns>
    public bool ClearAllMemory()
    {
        if (CurrentMode != OperatingMode.Kernel)
            return false;

        _ram.ClearAll();
        return true;
    }

    /// <summary>
    /// Forcefully clears all disk space. Requires Kernel Mode.
    /// </summary>
    /// <returns>True if successful; false if not in Kernel Mode.</returns>
    public bool ClearAllDisk()
    {
        if (CurrentMode != OperatingMode.Kernel)
            return false;

        _hardDrive.ClearAll();
        _fileManager.ClearAllFiles();
        return true;
    }

    /// <summary>
    /// Gets comprehensive system status information.
    /// </summary>
    public string GetSystemStatus()
    {
        var status = new System.Text.StringBuilder();
        status.AppendLine("=== ComaOS System Status ===");
        status.AppendLine($"Mode: {CurrentMode}");
        status.AppendLine($"Running: {_isRunning}");
        status.AppendLine();
        status.AppendLine(_cpu.GetCoreStatus());
        status.AppendLine();
        status.AppendLine(_ram.GetMemoryStatus());
        status.AppendLine();
        status.AppendLine(_hardDrive.GetDiskStatus());
        status.AppendLine();
        status.AppendLine(_scheduler.GetSchedulerStatus());
        status.AppendLine();
        status.AppendLine($"Files: {_fileManager.GetAllFiles().Count}");
        return status.ToString();
    }

    /// <summary>
    /// Starts system applications that run automatically on boot.
    /// </summary>
    private async Task StartSystemAppsAsync()
    {
        await Task.Run(() =>
        {
            // Auto-start clock (always running)
            StartApplication(ApplicationType.Clock);
        });
    }

    /// <summary>
    /// Gets the boot loader for splash screen display.
    /// </summary>
    public BootLoader GetBootLoader() => _bootLoader;
}

/// <summary>
/// Defines the operating modes for ComaOS.
/// </summary>
public enum OperatingMode
{
    /// <summary>
    /// User mode with restricted permissions.
    /// </summary>
    User,

    /// <summary>
    /// Kernel mode with full system access (can kill processes, clear memory).
    /// </summary>
    Kernel
}

/// <summary>
/// Event arguments for mode change events.
/// </summary>
public class ModeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous operating mode.
    /// </summary>
    public OperatingMode OldMode { get; }

    /// <summary>
    /// Gets the new operating mode.
    /// </summary>
    public OperatingMode NewMode { get; }

    /// <summary>
    /// Initializes a new instance of ModeChangedEventArgs.
    /// </summary>
    public ModeChangedEventArgs(OperatingMode oldMode, OperatingMode newMode)
    {
        OldMode = oldMode;
        NewMode = newMode;
    }
}
