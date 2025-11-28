// ComaOS Console Application
// A CLI interface for testing ComaOS kernel operations

using ComaOS.Core.Kernel;
using ComaOS.Core.Apps;

namespace ComaOS.Console;

/// <summary>
/// Main entry point for the ComaOS Console application.
/// Provides a CLI interface for interacting with the OS kernel.
/// </summary>
internal class Program
{
    private static KernelManager? _kernel;
    private static bool _isRunning = true;

    /// <summary>
    /// Application entry point.
    /// </summary>
    private static async Task Main(string[] args)
    {
        System.Console.Title = "ComaOS Console";
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        DisplayWelcome();

        // Initialize and boot the OS
        if (!await InitializeKernelAsync())
        {
            System.Console.WriteLine("Failed to initialize ComaOS. Press any key to exit.");
            System.Console.ReadKey();
            return;
        }

        // Main command loop
        await RunCommandLoopAsync();

        // Shutdown
        await ShutdownAsync();
    }

    /// <summary>
    /// Displays the welcome message and banner.
    /// </summary>
    private static void DisplayWelcome()
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║       ██████╗ ██████╗ ███╗   ███╗ █████╗  ██████╗ ███████╗        ║
║      ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██╔═══██╗██╔════╝        ║
║      ██║     ██║   ██║██╔████╔██║███████║██║   ██║███████╗        ║
║      ██║     ██║   ██║██║╚██╔╝██║██╔══██║██║   ██║╚════██║        ║
║      ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║  ██║╚██████╔╝███████║        ║
║       ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝        ║
║                                                                   ║
║                 Operating System Simulator v1.0                   ║
║                         Console Edition                           ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
");
        System.Console.ResetColor();
    }

    /// <summary>
    /// Initializes the kernel with user-specified hardware configuration.
    /// </summary>
    private static async Task<bool> InitializeKernelAsync()
    {
        System.Console.WriteLine("\n[SYSTEM CONFIGURATION]");
        System.Console.WriteLine("Configure your simulated hardware:\n");

        // Get RAM size
        long ramSize = GetValidatedInput<long>(
            "Enter RAM size (MB) [512-65536, default: 4096]: ",
            512, 65536, 4096);

        // Get HDD size
        long hddSize = GetValidatedInput<long>(
            "Enter Hard Drive size (GB) [10-10240, default: 500]: ",
            10, 10240, 500);

        // Get Core count
        int coreCount = GetValidatedInput<int>(
            "Enter CPU Core count [1-64, default: 4]: ",
            1, 64, 4);

        System.Console.WriteLine("\n[INITIALIZING HARDWARE]");
        System.Console.WriteLine($"  • RAM: {ramSize} MB");
        System.Console.WriteLine($"  • HDD: {hddSize} GB");
        System.Console.WriteLine($"  • CPU: {coreCount} Core(s)");
        System.Console.WriteLine();

        try
        {
            _kernel = new KernelManager(ramSize, hddSize, coreCount);

            // Subscribe to boot progress events
            _kernel.GetBootLoader().BootProgress += (sender, e) =>
            {
                DisplayBootProgress(e.Percentage, e.Message);
            };

            System.Console.WriteLine("[BOOTING COMAOS]");
            var bootResult = await _kernel.BootAsync();

            if (bootResult.Success)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"\n✓ {bootResult.Message}\n");
                System.Console.ResetColor();
                return true;
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"\n✗ {bootResult.Message}\n");
                System.Console.ResetColor();
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"\n✗ Initialization failed: {ex.Message}\n");
            System.Console.ResetColor();
            return false;
        }
    }

    /// <summary>
    /// Displays boot progress with a progress bar.
    /// </summary>
    private static void DisplayBootProgress(int percentage, string message)
    {
        int barWidth = 40;
        int filled = (int)((percentage / 100.0) * barWidth);

        System.Console.Write("\r[");
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.Write(new string('█', filled));
        System.Console.ResetColor();
        System.Console.Write(new string('░', barWidth - filled));
        System.Console.Write($"] {percentage,3}% - {message,-40}");

        if (percentage == 100)
            System.Console.WriteLine();
    }

    /// <summary>
    /// Gets validated numerical input from the user.
    /// </summary>
    private static T GetValidatedInput<T>(string prompt, T min, T max, T defaultValue) where T : IComparable<T>
    {
        while (true)
        {
            try
            {
                System.Console.Write(prompt);
                string? input = System.Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return defaultValue;

                T value = (T)Convert.ChangeType(input, typeof(T));

                if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine($"  ⚠ Value must be between {min} and {max}. Using default: {defaultValue}");
                    System.Console.ResetColor();
                    return defaultValue;
                }

                return value;
            }
            catch (FormatException)
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine($"  ⚠ Invalid input. Using default: {defaultValue}");
                System.Console.ResetColor();
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Main command processing loop.
    /// </summary>
    private static async Task RunCommandLoopAsync()
    {
        DisplayHelp();

        while (_isRunning && _kernel != null)
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.Write($"\nComaOS [{(_kernel.CurrentMode == OperatingMode.Kernel ? "KERNEL" : "USER")}]> ");
            System.Console.ResetColor();

            string? input = System.Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            await ProcessCommandAsync(input);
        }
    }

    /// <summary>
    /// Processes a single command input.
    /// </summary>
    private static async Task ProcessCommandAsync(string input)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string command = parts[0].ToLowerInvariant();
        string[] args = parts.Skip(1).ToArray();

        try
        {
            switch (command)
            {
                case "help":
                case "?":
                    DisplayHelp();
                    break;

                case "status":
                case "stat":
                    DisplaySystemStatus();
                    break;

                case "cpu":
                    DisplayCpuStatus();
                    break;

                case "ram":
                case "memory":
                    DisplayRamStatus();
                    break;

                case "disk":
                case "hdd":
                    DisplayDiskStatus();
                    break;

                case "ps":
                case "processes":
                    DisplayProcesses();
                    break;

                case "start":
                case "run":
                    StartApplication(args);
                    break;

                case "kill":
                case "terminate":
                    KillProcess(args);
                    break;

                case "apps":
                case "list":
                    ListAvailableApps();
                    break;

                case "mode":
                    SwitchMode(args);
                    break;

                case "files":
                case "ls":
                    ListFiles(args);
                    break;

                case "touch":
                case "create":
                    CreateFile(args);
                    break;

                case "cat":
                case "read":
                    ReadFile(args);
                    break;

                case "rm":
                case "delete":
                    DeleteFile(args);
                    break;

                case "write":
                    WriteFile(args);
                    break;

                case "scheduler":
                case "sched":
                    DisplaySchedulerStatus();
                    break;

                case "clear":
                case "cls":
                    System.Console.Clear();
                    DisplayWelcome();
                    break;

                case "clearmem":
                    ClearAllMemory();
                    break;

                case "cleardisk":
                    ClearAllDisk();
                    break;

                case "demo":
                    await RunDemoAsync();
                    break;

                case "stress":
                    await RunStressTestAsync(args);
                    break;

                case "exit":
                case "quit":
                case "shutdown":
                    _isRunning = false;
                    break;

                default:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine($"Unknown command: '{command}'. Type 'help' for available commands.");
                    System.Console.ResetColor();
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error executing command: {ex.Message}");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Displays the help menu.
    /// </summary>
    private static void DisplayHelp()
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        System.Console.WriteLine("                     COMAOS COMMANDS                            ");
        System.Console.WriteLine("═══════════════════════════════════════════════════════════════");
        System.Console.ResetColor();

        System.Console.WriteLine("\n[SYSTEM]");
        System.Console.WriteLine("  help, ?           - Display this help menu");
        System.Console.WriteLine("  status, stat      - Show complete system status");
        System.Console.WriteLine("  mode <user|kernel>- Switch operating mode");
        System.Console.WriteLine("  clear, cls        - Clear the console");
        System.Console.WriteLine("  exit, quit        - Shutdown ComaOS");

        System.Console.WriteLine("\n[HARDWARE]");
        System.Console.WriteLine("  cpu               - Show CPU status and core usage");
        System.Console.WriteLine("  ram, memory       - Show RAM usage details");
        System.Console.WriteLine("  disk, hdd         - Show hard drive usage");

        System.Console.WriteLine("\n[PROCESSES]");
        System.Console.WriteLine("  ps, processes     - List all processes");
        System.Console.WriteLine("  apps, list        - List available applications");
        System.Console.WriteLine("  start <app>       - Start an application by name or number");
        System.Console.WriteLine("  kill <pid>        - Terminate a process (Kernel mode only)");
        System.Console.WriteLine("  scheduler, sched  - Show scheduler queue status");

        System.Console.WriteLine("\n[FILE SYSTEM]");
        System.Console.WriteLine("  files, ls [path]  - List files in directory");
        System.Console.WriteLine("  touch <name> [path] - Create a new file");
        System.Console.WriteLine("  cat <path>        - Read file contents");
        System.Console.WriteLine("  write <path> <text> - Write to a file");
        System.Console.WriteLine("  rm <path>         - Delete a file");

        System.Console.WriteLine("\n[ADMINISTRATION] (Kernel Mode)");
        System.Console.WriteLine("  clearmem          - Clear all RAM allocations");
        System.Console.WriteLine("  cleardisk         - Clear all disk allocations");

        System.Console.WriteLine("\n[TESTING]");
        System.Console.WriteLine("  demo              - Run a demonstration sequence");
        System.Console.WriteLine("  stress [count]    - Run stress test with multiple apps");

        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("\n═══════════════════════════════════════════════════════════════\n");
        System.Console.ResetColor();
    }

    /// <summary>
    /// Displays complete system status.
    /// </summary>
    private static void DisplaySystemStatus()
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("\n" + _kernel.GetSystemStatus());
        System.Console.ResetColor();
    }

    /// <summary>
    /// Displays CPU status.
    /// </summary>
    private static void DisplayCpuStatus()
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.Magenta;
        System.Console.WriteLine("\n[CPU STATUS]");
        System.Console.ResetColor();
        System.Console.WriteLine(_kernel.CPU.GetCoreStatus());
        System.Console.WriteLine($"Overall CPU Usage: {_kernel.CPU.GetCpuUsage():F1}%");
    }

    /// <summary>
    /// Displays RAM status.
    /// </summary>
    private static void DisplayRamStatus()
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.Blue;
        System.Console.WriteLine("\n[MEMORY STATUS]");
        System.Console.ResetColor();
        System.Console.WriteLine(_kernel.RAM.GetMemoryStatus());

        System.Console.WriteLine("\nAllocated Blocks:");
        var blocks = _kernel.RAM.GetAllocatedBlocks();
        if (blocks.Count == 0)
        {
            System.Console.WriteLine("  (No allocations)");
        }
        else
        {
            foreach (var block in blocks)
            {
                System.Console.WriteLine($"  PID {block.ProcessId}: {block.SizeMB} MB (allocated {block.AllocatedAt:HH:mm:ss})");
            }
        }
    }

    /// <summary>
    /// Displays disk status.
    /// </summary>
    private static void DisplayDiskStatus()
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
        System.Console.WriteLine("\n[DISK STATUS]");
        System.Console.ResetColor();
        System.Console.WriteLine(_kernel.HardDrive.GetDiskStatus());
    }

    /// <summary>
    /// Displays all processes.
    /// </summary>
    private static void DisplayProcesses()
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("\n[PROCESS LIST]");
        System.Console.ResetColor();

        var activeProcesses = _kernel.Scheduler.AllActiveProcesses;
        var terminatedProcesses = _kernel.Scheduler.TerminatedProcesses;

        if (!activeProcesses.Any() && !terminatedProcesses.Any())
        {
            System.Console.WriteLine("No processes found.");
            return;
        }

        System.Console.WriteLine("\n{0,-6} {1,-20} {2,-12} {3,-8} {4,-8} {5,-8}",
            "PID", "NAME", "STATE", "PRIORITY", "RAM(MB)", "PC(%)");
        System.Console.WriteLine(new string('-', 70));

        foreach (var p in activeProcesses)
        {
            ConsoleColor color = p.State switch
            {
                ProcessState.Running => ConsoleColor.Green,
                ProcessState.Ready => ConsoleColor.Yellow,
                ProcessState.Blocked => ConsoleColor.DarkYellow,
                _ => ConsoleColor.White
            };
            System.Console.ForegroundColor = color;
            System.Console.WriteLine("{0,-6} {1,-20} {2,-12} {3,-8} {4,-8} {5,-8}",
                p.ProcessId, p.ProcessName, p.State, p.Priority == 1 ? "HIGH" : "NORMAL", p.RamUsageMB, p.ProgramCounter);
        }

        if (terminatedProcesses.Any())
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.WriteLine("\n[TERMINATED]");
            foreach (var p in terminatedProcesses.TakeLast(5))
            {
                System.Console.WriteLine("{0,-6} {1,-20} {2,-12}",
                    p.ProcessId, p.ProcessName, p.State);
            }
        }

        System.Console.ResetColor();
    }

    /// <summary>
    /// Lists available applications.
    /// </summary>
    private static void ListAvailableApps()
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("\n[AVAILABLE APPLICATIONS]");
        System.Console.ResetColor();

        var apps = Enum.GetValues<ApplicationType>();
        int index = 1;

        System.Console.WriteLine("\n{0,-4} {1,-20} {2,-8} {3,-8}",
            "#", "NAME", "PRIORITY", "RAM(MB)");
        System.Console.WriteLine(new string('-', 45));

        foreach (var app in apps)
        {
            var (priority, ram) = GetAppInfo(app);
            System.Console.WriteLine("{0,-4} {1,-20} {2,-8} {3,-8}",
                index++, app.ToString(), priority == 1 ? "HIGH" : "NORMAL", ram);
        }

        System.Console.WriteLine("\nUsage: start <name|number>  (e.g., 'start notepad' or 'start 1')");
    }

    /// <summary>
    /// Gets application info (priority and RAM requirement).
    /// </summary>
    private static (int priority, long ram) GetAppInfo(ApplicationType appType)
    {
        return appType switch
        {
            ApplicationType.Notepad => (2, 64),
            ApplicationType.Calculator => (2, 32),
            ApplicationType.Clock => (2, 16),
            ApplicationType.Calendar => (2, 48),
            ApplicationType.FileManager => (2, 128),
            ApplicationType.SystemMonitor => (2, 96),
            ApplicationType.Minesweeper => (1, 128),
            ApplicationType.MusicPlayer => (2, 192),
            ApplicationType.VideoPlayer => (1, 512),
            ApplicationType.Browser => (2, 256),
            ApplicationType.Terminal => (2, 64),
            ApplicationType.ImageViewer => (2, 128),
            ApplicationType.Antivirus => (2, 256),
            ApplicationType.CompressionTool => (2, 192),
            ApplicationType.Settings => (2, 96),
            _ => (2, 64)
        };
    }

    /// <summary>
    /// Starts an application by name or index.
    /// </summary>
    private static void StartApplication(string[] args)
    {
        if (_kernel == null) return;

        if (args.Length == 0)
        {
            System.Console.WriteLine("Usage: start <app_name|number>");
            System.Console.WriteLine("Example: start notepad  OR  start 1");
            return;
        }

        ApplicationType? appType = null;

        // Try parsing as number first
        if (int.TryParse(args[0], out int index))
        {
            var apps = Enum.GetValues<ApplicationType>();
            if (index >= 1 && index <= apps.Length)
            {
                appType = apps[index - 1];
            }
        }
        else
        {
            // Try parsing as name
            if (Enum.TryParse<ApplicationType>(args[0], ignoreCase: true, out var parsed))
            {
                appType = parsed;
            }
        }

        if (appType == null)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Unknown application: '{args[0]}'");
            System.Console.WriteLine("Use 'apps' command to see available applications.");
            System.Console.ResetColor();
            return;
        }

        System.Console.Write($"Starting {appType}... ");

        int pid = _kernel.StartApplication(appType.Value);

        if (pid > 0)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"✓ Started with PID {pid}");
            System.Console.ResetColor();
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("✗ Failed to start (insufficient RAM?)");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Kills a process by PID.
    /// </summary>
    private static void KillProcess(string[] args)
    {
        if (_kernel == null) return;

        if (args.Length == 0)
        {
            System.Console.WriteLine("Usage: kill <pid>");
            return;
        }

        if (!int.TryParse(args[0], out int pid))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Invalid PID. Must be a number.");
            System.Console.ResetColor();
            return;
        }

        if (_kernel.CurrentMode != OperatingMode.Kernel)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("⚠ Warning: Kill operation requires Kernel Mode.");
            System.Console.WriteLine("Use 'mode kernel' to switch to Kernel Mode first.");
            System.Console.ResetColor();
            return;
        }

        bool success = _kernel.TerminateProcess(pid);

        if (success)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"✓ Process {pid} terminated.");
            System.Console.ResetColor();
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Failed to terminate process {pid}.");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Switches between User and Kernel mode.
    /// </summary>
    private static void SwitchMode(string[] args)
    {
        if (_kernel == null) return;

        if (args.Length == 0)
        {
            System.Console.WriteLine($"Current mode: {_kernel.CurrentMode}");
            System.Console.WriteLine("Usage: mode <user|kernel>");
            return;
        }

        string targetMode = args[0].ToLowerInvariant();

        OperatingMode newMode = targetMode switch
        {
            "user" => OperatingMode.User,
            "kernel" => OperatingMode.Kernel,
            _ => _kernel.CurrentMode
        };

        if (newMode == _kernel.CurrentMode)
        {
            System.Console.WriteLine($"Already in {_kernel.CurrentMode} mode.");
            return;
        }

        _kernel.SwitchMode(newMode);

        System.Console.ForegroundColor = newMode == OperatingMode.Kernel ? ConsoleColor.Red : ConsoleColor.Green;
        System.Console.WriteLine($"✓ Switched to {newMode} Mode");
        System.Console.ResetColor();

        if (newMode == OperatingMode.Kernel)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("⚠ Kernel Mode: You now have elevated privileges.");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Lists files in the virtual file system.
    /// </summary>
    private static void ListFiles(string[] args)
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
        System.Console.WriteLine("\n[FILE SYSTEM]");
        System.Console.ResetColor();

        string path = args.Length > 0 ? args[0] : "/";
        var files = _kernel.FileManager.GetFilesInDirectory(path);

        if (!files.Any())
        {
            System.Console.WriteLine($"No files in '{path}' (or directory doesn't exist).");
            System.Console.WriteLine("\nAll files:");
        }

        var allFiles = _kernel.FileManager.GetAllFiles().Where(f => !f.IsHidden);

        System.Console.WriteLine("\n{0,-30} {1,-10} {2,-15} {3,-20}",
            "PATH", "SIZE", "TYPE", "MODIFIED");
        System.Console.WriteLine(new string('-', 80));

        foreach (var file in allFiles)
        {
            System.Console.WriteLine("{0,-30} {1,-10} {2,-15} {3,-20}",
                file.FullPath, $"{file.SizeKB:F2} KB", file.Type, file.ModifiedAt.ToString("yyyy-MM-dd HH:mm"));
        }

        System.Console.WriteLine(_kernel.FileManager.GetFileSystemStatus());
    }

    /// <summary>
    /// Creates a new file.
    /// </summary>
    private static void CreateFile(string[] args)
    {
        if (_kernel == null) return;

        if (args.Length == 0)
        {
            System.Console.WriteLine("Usage: touch <filename> [directory]");
            System.Console.WriteLine("Example: touch myfile.txt /Documents");
            return;
        }

        string fileName = args[0];
        string directory = args.Length > 1 ? args[1] : "/Documents";

        var file = _kernel.FileManager.CreateFile(fileName, directory);

        if (file != null)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"✓ Created: {file.FullPath}");
            System.Console.ResetColor();
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Failed to create file (exists or disk full?)");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Reads file contents.
    /// </summary>
    private static void ReadFile(string[] args)
    {
        if (_kernel == null) return;

        if (args.Length == 0)
        {
            System.Console.WriteLine("Usage: cat <full_path>");
            System.Console.WriteLine("Example: cat /Documents/myfile.txt");
            return;
        }

        string content = _kernel.FileManager.ReadFile(args[0]) ?? "[File not found]";

        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.WriteLine($"\n--- {args[0]} ---");
        System.Console.WriteLine(string.IsNullOrEmpty(content) ? "(empty file)" : content);
        System.Console.WriteLine("--- EOF ---");
        System.Console.ResetColor();
    }

    /// <summary>
    /// Writes content to a file.
    /// </summary>
    private static void WriteFile(string[] args)
    {
        if (_kernel == null) return;

        if (args.Length < 2)
        {
            System.Console.WriteLine("Usage: write <full_path> <content>");
            System.Console.WriteLine("Example: write /Documents/myfile.txt Hello World");
            return;
        }

        string path = args[0];
        string content = string.Join(" ", args.Skip(1));

        bool success = _kernel.FileManager.UpdateFile(path, content);

        if (success)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"✓ Written {content.Length} characters to {path}");
            System.Console.ResetColor();
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Failed to write (file not found or read-only?)");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Deletes a file.
    /// </summary>
    private static void DeleteFile(string[] args)
    {
        if (_kernel == null) return;

        if (args.Length == 0)
        {
            System.Console.WriteLine("Usage: rm <full_path>");
            return;
        }

        bool success = _kernel.FileManager.DeleteFile(args[0]);

        if (success)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"✓ Deleted: {args[0]}");
            System.Console.ResetColor();
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Failed to delete (not found or read-only?)");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Displays scheduler status.
    /// </summary>
    private static void DisplaySchedulerStatus()
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.Magenta;
        System.Console.WriteLine("\n[SCHEDULER STATUS]");
        System.Console.ResetColor();
        System.Console.WriteLine(_kernel.Scheduler.GetSchedulerStatus());

        System.Console.WriteLine("\nReady Queue (High Priority):");
        var ready = _kernel.Scheduler.ReadyProcesses.Where(p => p.Priority == 1);
        foreach (var p in ready)
        {
            System.Console.WriteLine($"  → {p}");
        }

        System.Console.WriteLine("\nReady Queue (Normal Priority):");
        ready = _kernel.Scheduler.ReadyProcesses.Where(p => p.Priority == 2);
        foreach (var p in ready)
        {
            System.Console.WriteLine($"  → {p}");
        }

        System.Console.WriteLine("\nRunning:");
        foreach (var p in _kernel.Scheduler.RunningProcesses)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"  ▶ {p} on Core {p.AssignedCoreId}");
        }
        System.Console.ResetColor();
    }

    /// <summary>
    /// Clears all memory (Kernel mode required).
    /// </summary>
    private static void ClearAllMemory()
    {
        if (_kernel == null) return;

        if (_kernel.CurrentMode != OperatingMode.Kernel)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("⚠ This operation requires Kernel Mode.");
            System.Console.ResetColor();
            return;
        }

        System.Console.Write("Are you sure you want to clear ALL memory? (y/N): ");
        var response = System.Console.ReadLine();

        if (response?.ToLowerInvariant() == "y")
        {
            _kernel.ClearAllMemory();
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("✓ All memory cleared.");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Clears all disk (Kernel mode required).
    /// </summary>
    private static void ClearAllDisk()
    {
        if (_kernel == null) return;

        if (_kernel.CurrentMode != OperatingMode.Kernel)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("⚠ This operation requires Kernel Mode.");
            System.Console.ResetColor();
            return;
        }

        System.Console.Write("Are you sure you want to clear ALL disk data? (y/N): ");
        var response = System.Console.ReadLine();

        if (response?.ToLowerInvariant() == "y")
        {
            _kernel.ClearAllDisk();
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("✓ All disk data cleared.");
            System.Console.ResetColor();
        }
    }

    /// <summary>
    /// Runs a demo sequence showcasing OS capabilities.
    /// </summary>
    private static async Task RunDemoAsync()
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("\n[RUNNING DEMO SEQUENCE]");
        System.Console.ResetColor();

        System.Console.WriteLine("\n1. Creating sample files...");
        _kernel.FileManager.CreateFile("readme.txt", "/Documents", "Welcome to ComaOS!");
        _kernel.FileManager.CreateFile("notes.txt", "/Documents", "This is a sample note.");
        _kernel.FileManager.CreateFile("data.txt", "/Downloads", "Downloaded data content.");
        System.Console.WriteLine("   ✓ Created 3 files");

        System.Console.WriteLine("\n2. Starting applications...");
        var apps = new[] { ApplicationType.Calculator, ApplicationType.Notepad, ApplicationType.Browser };
        foreach (var app in apps)
        {
            int pid = _kernel.StartApplication(app);
            System.Console.WriteLine($"   ✓ Started {app} (PID: {pid})");
            await Task.Delay(500);
        }

        System.Console.WriteLine("\n3. Displaying system status...");
        await Task.Delay(2000);
        DisplaySystemStatus();

        System.Console.WriteLine("\n4. Monitoring process execution (5 seconds)...");
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(1000);
            System.Console.Write(".");
        }
        System.Console.WriteLine(" Done!");

        System.Console.WriteLine("\n5. Final process status:");
        DisplayProcesses();

        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("\n[DEMO COMPLETE]");
        System.Console.ResetColor();
    }

    /// <summary>
    /// Runs a stress test by starting multiple applications.
    /// </summary>
    private static async Task RunStressTestAsync(string[] args)
    {
        if (_kernel == null) return;

        int count = 5;
        if (args.Length > 0 && int.TryParse(args[0], out int parsed))
        {
            count = Math.Min(parsed, 15); // Cap at 15
        }

        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"\n[STRESS TEST - Starting {count} random applications]");
        System.Console.ResetColor();

        var random = new Random();
        var apps = Enum.GetValues<ApplicationType>();
        int started = 0;
        int failed = 0;

        for (int i = 0; i < count; i++)
        {
            var appType = apps[random.Next(apps.Length)];
            int pid = _kernel.StartApplication(appType);

            if (pid > 0)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"  [{i + 1}/{count}] ✓ Started {appType} (PID: {pid})");
                started++;
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"  [{i + 1}/{count}] ✗ Failed to start {appType} (RAM full?)");
                failed++;
            }
            System.Console.ResetColor();

            await Task.Delay(300);
        }

        System.Console.WriteLine($"\n[STRESS TEST RESULTS]");
        System.Console.WriteLine($"  Started: {started}");
        System.Console.WriteLine($"  Failed:  {failed}");
        System.Console.WriteLine();

        DisplayRamStatus();
        DisplayCpuStatus();
    }

    /// <summary>
    /// Graceful shutdown of ComaOS.
    /// </summary>
    private static async Task ShutdownAsync()
    {
        if (_kernel == null) return;

        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("\n[SHUTTING DOWN COMAOS]");
        System.Console.ResetColor();

        System.Console.Write("Terminating processes");
        for (int i = 0; i < 3; i++)
        {
            await Task.Delay(300);
            System.Console.Write(".");
        }

        await _kernel.ShutdownAsync();

        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("\n\n✓ ComaOS has been shut down safely.");
        System.Console.WriteLine("Thank you for using ComaOS!\n");
        System.Console.ResetColor();
    }
}

