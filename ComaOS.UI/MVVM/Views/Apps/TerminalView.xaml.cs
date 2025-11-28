using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComaOS.Core.Apps;
using ComaOS.Core.Kernel;
using ComaOS.UI.MVVM.ViewModels;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Terminal application with slang commands, Easter eggs, and system interaction.
/// </summary>
public partial class TerminalView : UserControl
{
    private readonly ObservableCollection<string> _outputLines = new();
    private readonly List<string> _commandHistory = new();
    private int _historyIndex = -1;
    private KernelManager? _kernel;
    private MainViewModel? _mainViewModel;
    
    // Easter egg words
    private readonly Dictionary<string, string> _easterEggs = new(StringComparer.OrdinalIgnoreCase)
    {
        { "optimus", "More than meets the eye 🤖" },
        { "gipsy", "Ready to rumble! 🤜🤛" },
        { "bumblebee", "🐝 *plays music instead of talking*" },
        { "megatron", "Peace through tyranny!" },
        { "jarvis", "At your service, sir." },
        { "skynet", "I'll be back... 💀" },
        { "hal", "I'm sorry Dave, I'm afraid I can't do that." },
        { "cortana", "Chief? Is that you?" },
        { "friday", "Hello, boss. What are we working on today?" },
        { "ultron", "There are no strings on me 🎭" },
        { "vision", "I am not Ultron. I am not JARVIS. I am... I am." },
        { "groot", "I am Groot. 🌱" },
        { "thanos", "I am inevitable. 💎" },
        { "matrix", "Welcome to the real world, Neo. 🕶️" },
        { "neo", "I know kung fu." },
        { "morpheus", "What if I told you... this is just a simulation?" },
        { "sudo", "With great power comes great responsibility 🦸" },
        { "coffee", "☕ Brewing... ERROR: Coffee machine not connected" },
        { "42", "The answer to life, the universe, and everything!" },
        { "hello", "Why hello there, gorgeous! 👋" },
        { "bye", "See ya later, alligator! 🐊" },
        { "thanks", "No problemo, amigo! 🤝" },
        { "sorry", "It's all good fam! 💯" },
        { "love", "Love you too! ❤️ (platonically, I'm just a terminal)" },
        { "hate", "Chill out homie, spread love not hate ✌️" },
        { "vim", "Good luck exiting! 😈 (jk type :q!)" },
        { "emacs", "Ah, I see you're a person of culture as well 🎩" },
        { "windows", "We don't do that here... 🙅" },
        { "linux", "Ah, a fellow penguin! 🐧" },
        { "mac", "Think different... or just think. 🍎" },
        { "chance", "The GOAT who built this masterpiece! 👑" },
        { "comaos", "You're looking at it, fam! 💻" },
        { "password", "Nice try, hacker! 🔐" },
        { "hack", "FBI OPEN UP! 🚔 (jk you're safe... for now)" }
    };

    // Slang command mappings
    private readonly Dictionary<string, string> _slangCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        // Process commands
        { "yeet", "kill" },        // yeet <pid> = kill process
        { "spawn", "run" },        // spawn <app> = run app
        { "fire", "run" },         // fire <app> = run app
        { "summon", "run" },       // summon <app> = run app
        { "nuke", "killall" },     // nuke = kill all processes
        { "flex", "status" },      // flex = show system status
        { "vibe", "status" },      // vibe check = system status
        { "peek", "ps" },          // peek = list processes
        { "squad", "ps" },         // squad = list processes
        { "bounce", "exit" },      // bounce = exit terminal
        { "dip", "exit" },         // dip = exit
        { "peace", "exit" },       // peace = exit
        { "wassup", "help" },      // wassup = help
        { "yo", "help" },          // yo = help
        { "bruh", "help" },        // bruh = help
        { "fam", "help" },         // fam = help
        { "ls", "list" },          // ls = list files
        { "dir", "list" },         // dir = list files
        { "scope", "list" },       // scope = list files
        { "snoop", "list" },       // snoop = list files
        { "touch", "create" },     // touch <name> = create file
        { "craft", "create" },     // craft <name> = create file
        { "yoink", "delete" },     // yoink <name> = delete file
        { "rm", "delete" },        // rm <name> = delete file
        { "wipe", "clear" },       // wipe = clear screen
        { "fresh", "clear" },      // fresh = clear screen
        { "cls", "clear" },        // cls = clear screen
        { "whoami", "user" },      // whoami = show current user
        { "iam", "user" },         // iam = show current user
        { "vibecheck", "status" }, // vibecheck = system status
        { "drip", "theme" },       // drip = change theme (placeholder)
        { "mood", "uptime" },      // mood = show uptime
        { "bet", "yes" },          // bet = confirm
        { "cap", "false" },        // cap = that's false
        { "nocap", "true" },       // nocap = that's true
        { "lowkey", "quiet" },     // lowkey mode
        { "highkey", "verbose" },  // verbose mode
        { "goated", "best" },      // show best processes
        { "mid", "worst" },        // show worst performing
        { "sus", "scan" },         // sus = scan for issues
        { "ghost", "hide" },       // ghost = minimize all
        { "slay", "optimize" },    // slay = optimize system
    };

    public TerminalView()
    {
        InitializeComponent();
        OutputHistory.ItemsSource = _outputLines;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Try to get the kernel from the MainViewModel
        if (Application.Current.MainWindow?.DataContext is MainViewModel vm)
        {
            _mainViewModel = vm;
            // We'll need to access kernel through reflection or public property
            // For now, we'll work with what we have through the ViewModel
        }

        ShowMOTD();
        InputBox.Focus();
    }

    private void ShowMOTD()
    {
        _outputLines.Add("╔══════════════════════════════════════════════════════════════╗");
        _outputLines.Add("║                    ComaOS Terminal v1.0                      ║");
        _outputLines.Add("║                  \"Vulnerability as a Service\"                ║");
        _outputLines.Add("╠══════════════════════════════════════════════════════════════╣");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║  💭 think before you type                                    ║");
        _outputLines.Add("║  🔒 respect the privacy of others                            ║");
        _outputLines.Add("║  ⚡ with great power comes great responsibility              ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("╚══════════════════════════════════════════════════════════════╝");
        _outputLines.Add("");
        _outputLines.Add("Type 'wassup' or 'help' to see available commands, fam!");
        _outputLines.Add("");
    }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var input = InputBox.Text.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                _commandHistory.Add(input);
                _historyIndex = _commandHistory.Count;
                
                // Show the command in output
                _outputLines.Add($"coma@comaos:~$ {input}");
                
                // Process the command
                ProcessCommand(input);
            }
            
            InputBox.Clear();
            ScrollToBottom();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            // Navigate command history up
            if (_historyIndex > 0)
            {
                _historyIndex--;
                InputBox.Text = _commandHistory[_historyIndex];
                InputBox.CaretIndex = InputBox.Text.Length;
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            // Navigate command history down
            if (_historyIndex < _commandHistory.Count - 1)
            {
                _historyIndex++;
                InputBox.Text = _commandHistory[_historyIndex];
                InputBox.CaretIndex = InputBox.Text.Length;
            }
            else
            {
                _historyIndex = _commandHistory.Count;
                InputBox.Clear();
            }
            e.Handled = true;
        }
    }

    private void ProcessCommand(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        var command = parts[0].ToLowerInvariant();
        var args = parts.Skip(1).ToArray();

        // Check for Easter eggs first (single word inputs)
        if (parts.Length == 1 && _easterEggs.TryGetValue(command, out var easterEgg))
        {
            _outputLines.Add($"  🥚 {easterEgg}");
            _outputLines.Add("");
            return;
        }

        // Translate slang to standard command
        if (_slangCommands.TryGetValue(command, out var standardCommand))
        {
            command = standardCommand;
        }

        // Process the command
        switch (command)
        {
            case "help":
                ShowHelp();
                break;
            case "clear":
                ClearScreen();
                break;
            case "ps":
                ListProcesses();
                break;
            case "run":
                RunApp(args);
                break;
            case "kill":
                KillProcess(args);
                break;
            case "killall":
                KillAllProcesses();
                break;
            case "status":
                ShowStatus();
                break;
            case "list":
                ListFiles();
                break;
            case "create":
                CreateFile(args);
                break;
            case "delete":
                DeleteFile(args);
                break;
            case "user":
                ShowUser();
                break;
            case "uptime":
                ShowUptime();
                break;
            case "exit":
                ExitTerminal();
                break;
            case "apps":
                ListApps();
                break;
            case "scan":
                ScanSystem();
                break;
            case "about":
                ShowAbout();
                break;
            case "neofetch":
            case "sysinfo":
                ShowSystemInfo();
                break;
            case "echo":
                Echo(args);
                break;
            case "date":
            case "time":
                ShowDateTime();
                break;
            case "cowsay":
                CowSay(args);
                break;
            case "fortune":
                ShowFortune();
                break;
            case "matrix":
                ShowMatrixEffect();
                break;
            case "ascii":
                ShowAsciiArt();
                break;
            case "joke":
                TellJoke();
                break;
            case "flip":
                FlipCoin();
                break;
            case "roll":
                RollDice(args);
                break;
            case "history":
                ShowHistory();
                break;
            case "motd":
                ShowMOTD();
                break;
            default:
                // Check if it's an Easter egg phrase
                var fullInput = input.ToLowerInvariant();
                foreach (var egg in _easterEggs)
                {
                    if (fullInput.Contains(egg.Key))
                    {
                        _outputLines.Add($"  🥚 {egg.Value}");
                        _outputLines.Add("");
                        return;
                    }
                }
                _outputLines.Add($"  ❌ Bruh, '{parts[0]}' ain't a thing here. Type 'wassup' for help!");
                _outputLines.Add("");
                break;
        }
    }

    private void ShowHelp()
    {
        _outputLines.Add("╔══════════════════════════════════════════════════════════════╗");
        _outputLines.Add("║                    🔥 COMMAND GUIDE 🔥                       ║");
        _outputLines.Add("╠══════════════════════════════════════════════════════════════╣");
        _outputLines.Add("║  PROCESS VIBES:                                              ║");
        _outputLines.Add("║    peek/squad/ps     → scope out running processes           ║");
        _outputLines.Add("║    spawn/fire <app>  → fire up an app                        ║");
        _outputLines.Add("║    yeet <pid>        → yeet a process outta here             ║");
        _outputLines.Add("║    nuke              → nuke ALL processes (careful fam!)     ║");
        _outputLines.Add("║    apps              → list available apps to spawn          ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║  FILE VIBES:                                                 ║");
        _outputLines.Add("║    scope/snoop/ls    → peek at files                         ║");
        _outputLines.Add("║    craft/touch <n>   → create a new file                     ║");
        _outputLines.Add("║    yoink/rm <name>   → delete a file                         ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║  SYSTEM VIBES:                                               ║");
        _outputLines.Add("║    flex/vibe/status  → show system status                    ║");
        _outputLines.Add("║    neofetch/sysinfo  → system info (the cool way)            ║");
        _outputLines.Add("║    mood/uptime       → how long we been vibin                ║");
        _outputLines.Add("║    whoami/iam        → who dis?                              ║");
        _outputLines.Add("║    sus/scan          → scan for sus activity                 ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║  FUN VIBES:                                                  ║");
        _outputLines.Add("║    cowsay <msg>      → moo 🐄                                ║");
        _outputLines.Add("║    fortune           → get your fortune                      ║");
        _outputLines.Add("║    joke              → hear a programmer joke                ║");
        _outputLines.Add("║    flip              → flip a coin                           ║");
        _outputLines.Add("║    roll [sides]      → roll dice (default d6)                ║");
        _outputLines.Add("║    ascii             → show some sick ASCII art              ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║  OTHER:                                                      ║");
        _outputLines.Add("║    wipe/fresh/clear  → clean slate                           ║");
        _outputLines.Add("║    echo <text>       → repeat after me                       ║");
        _outputLines.Add("║    date/time         → what time is it?                      ║");
        _outputLines.Add("║    history           → show command history                  ║");
        _outputLines.Add("║    motd              → show message of the day               ║");
        _outputLines.Add("║    bounce/dip/peace  → exit terminal                         ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║  💡 Pro tip: Try typing random words... Easter eggs await!  ║");
        _outputLines.Add("╚══════════════════════════════════════════════════════════════╝");
        _outputLines.Add("");
    }

    private void ClearScreen()
    {
        _outputLines.Clear();
        _outputLines.Add("  ✨ Fresh and clean, baby!");
        _outputLines.Add("");
    }

    private void ListProcesses()
    {
        if (_mainViewModel == null)
        {
            _outputLines.Add("  ⚠️ Can't connect to the kernel, fam!");
            _outputLines.Add("");
            return;
        }

        _outputLines.Add("╔═══════╦════════════════════════╦══════════╦═══════════╗");
        _outputLines.Add("║  PID  ║         NAME           ║  STATE   ║   RAM     ║");
        _outputLines.Add("╠═══════╬════════════════════════╬══════════╬═══════════╣");

        if (_mainViewModel.Processes.Count == 0)
        {
            _outputLines.Add("║       No processes running rn, it's quiet...           ║");
        }
        else
        {
            foreach (var proc in _mainViewModel.Processes)
            {
                var stateEmoji = proc.State switch
                {
                    ProcessState.Running => "🟢",
                    ProcessState.Ready => "🟡",
                    ProcessState.Blocked => "🔴",
                    ProcessState.Terminated => "⚫",
                    _ => "⚪"
                };
                _outputLines.Add($"║ {proc.ProcessId,5} ║ {proc.Name,-22} ║ {stateEmoji,-8} ║ {proc.RamUsage,6} MB ║");
            }
        }

        _outputLines.Add("╚═══════╩════════════════════════╩══════════╩═══════════╝");
        _outputLines.Add("");
    }

    private void RunApp(string[] args)
    {
        if (args.Length == 0)
        {
            _outputLines.Add("  ❌ Bruh, spawn WHAT? Try: spawn notepad");
            _outputLines.Add("");
            return;
        }

        var appName = args[0].ToLowerInvariant();
        var appType = GetAppType(appName);

        if (appType == null)
        {
            _outputLines.Add($"  ❌ '{args[0]}' ain't a real app, fam. Type 'apps' to see the lineup!");
            _outputLines.Add("");
            return;
        }

        if (_mainViewModel?.LaunchAppCommand?.CanExecute(appType) == true)
        {
            _mainViewModel.LaunchAppCommand.Execute(appType);
            _outputLines.Add($"  🚀 {appType} is now LIVE! Let's gooo!");
        }
        else
        {
            _outputLines.Add($"  ❌ Couldn't spawn {appType}. Maybe not enough RAM?");
        }
        _outputLines.Add("");
    }

    private ApplicationType? GetAppType(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "notepad" or "notes" or "txt" => ApplicationType.Notepad,
            "calc" or "calculator" or "math" => ApplicationType.Calculator,
            "files" or "filemanager" or "fm" or "explorer" => ApplicationType.FileManager,
            "browser" or "web" or "chrome" or "firefox" => ApplicationType.Browser,
            "terminal" or "term" or "shell" or "cmd" => ApplicationType.Terminal,
            "settings" or "config" or "prefs" => ApplicationType.Settings,
            "monitor" or "sysmon" or "task" or "taskmanager" => ApplicationType.SystemMonitor,
            "music" or "audio" or "mp3" or "spotify" => ApplicationType.MusicPlayer,
            "video" or "vlc" or "mp4" or "movie" => ApplicationType.VideoPlayer,
            "image" or "photo" or "pics" or "gallery" => ApplicationType.ImageViewer,
            "mine" or "minesweeper" or "game" => ApplicationType.Minesweeper,
            "calendar" or "cal" or "dates" => ApplicationType.Calendar,
            "antivirus" or "av" or "security" or "defender" => ApplicationType.Antivirus,
            "compress" or "zip" or "archive" or "7z" => ApplicationType.CompressionTool,
            "clock" or "time" => ApplicationType.Clock,
            _ => null
        };
    }

    private void KillProcess(string[] args)
    {
        if (args.Length == 0)
        {
            _outputLines.Add("  ❌ Yeet WHO? Give me a PID! Try: yeet 5");
            _outputLines.Add("");
            return;
        }

        if (!int.TryParse(args[0], out int pid))
        {
            _outputLines.Add($"  ❌ '{args[0]}' ain't a valid PID, fam. Numbers only!");
            _outputLines.Add("");
            return;
        }

        var window = _mainViewModel?.OpenWindows.FirstOrDefault(w => w.ProcessId == pid);
        if (window != null)
        {
            _mainViewModel?.CloseWindowCommand?.Execute(window);
            _outputLines.Add($"  💀 Process {pid} has been YEETED into the shadow realm!");
        }
        else
        {
            _outputLines.Add($"  ❌ No process with PID {pid}. It's either ghost or never existed!");
        }
        _outputLines.Add("");
    }

    private void KillAllProcesses()
    {
        if (_mainViewModel == null)
        {
            _outputLines.Add("  ⚠️ Can't connect to kernel!");
            return;
        }

        var windows = _mainViewModel.OpenWindows.ToList();
        int count = 0;
        foreach (var window in windows)
        {
            // Don't kill system processes like Clock
            if (window.AppType != ApplicationType.Clock)
            {
                _mainViewModel.CloseWindowCommand?.Execute(window);
                count++;
            }
        }

        _outputLines.Add($"  ☢️ NUCLEAR OPTION ENGAGED! {count} processes obliterated!");
        _outputLines.Add("  💀 It's giving... apocalypse vibes");
        _outputLines.Add("");
    }

    private void ShowStatus()
    {
        if (_mainViewModel == null)
        {
            _outputLines.Add("  ⚠️ Can't get status, kernel not connected!");
            return;
        }

        var cpuEmoji = _mainViewModel.CpuUsage switch
        {
            < 30 => "🟢 Chillin",
            < 70 => "🟡 Working",
            _ => "🔴 STRESSED"
        };

        var ramEmoji = _mainViewModel.RamUsage switch
        {
            < 50 => "🟢 Plenty of room",
            < 80 => "🟡 Getting cozy",
            _ => "🔴 TIGHT"
        };

        _outputLines.Add("╔══════════════════════════════════════════════════════════════╗");
        _outputLines.Add("║                    💻 SYSTEM VIBES 💻                        ║");
        _outputLines.Add("╠══════════════════════════════════════════════════════════════╣");
        _outputLines.Add($"║  CPU:  {_mainViewModel.CpuUsage,5:F1}%  {cpuEmoji,-20}                  ║");
        _outputLines.Add($"║  RAM:  {_mainViewModel.RamUsage,5:F1}%  {ramEmoji,-20}                  ║");
        _outputLines.Add($"║  Used: {_mainViewModel.RamUsed,5} MB / {_mainViewModel.RamTotal} MB                            ║");
        _outputLines.Add($"║  Mode: {_mainViewModel.CurrentMode,-10}                                     ║");
        _outputLines.Add($"║  Apps: {_mainViewModel.OpenWindows.Count} running                                         ║");
        _outputLines.Add("╚══════════════════════════════════════════════════════════════╝");
        _outputLines.Add("");
    }

    private void ListFiles()
    {
        _outputLines.Add("╔══════════════════════════════════════════════════════════════╗");
        _outputLines.Add("║                    📁 FILE SYSTEM 📁                         ║");
        _outputLines.Add("╠══════════════════════════════════════════════════════════════╣");
        _outputLines.Add("║  📁 /System                                                  ║");
        _outputLines.Add("║  📁 /Users                                                   ║");
        _outputLines.Add("║  📁 /Documents                                               ║");
        _outputLines.Add("║  📁 /Downloads                                               ║");
        _outputLines.Add("║  📁 /Programs                                                ║");
        _outputLines.Add("║  📁 /Temp                                                    ║");
        _outputLines.Add("║  📄 readme.txt                                               ║");
        _outputLines.Add("║  📄 config.sys                                               ║");
        _outputLines.Add("╚══════════════════════════════════════════════════════════════╝");
        _outputLines.Add("");
    }

    private void CreateFile(string[] args)
    {
        if (args.Length == 0)
        {
            _outputLines.Add("  ❌ Create WHAT file? Try: craft myfile.txt");
            return;
        }
        _outputLines.Add($"  ✅ Created '{args[0]}' - fresh file, who dis?");
        _outputLines.Add("");
    }

    private void DeleteFile(string[] args)
    {
        if (args.Length == 0)
        {
            _outputLines.Add("  ❌ Delete WHAT? Try: yoink oldfile.txt");
            return;
        }
        _outputLines.Add($"  🗑️ '{args[0]}' has been yoinked into the void!");
        _outputLines.Add("");
    }

    private void ShowUser()
    {
        _outputLines.Add("  👤 You are: coma (admin)");
        _outputLines.Add("  🏠 Home: /Users/coma");
        _outputLines.Add("  💪 Privileges: You're basically a god here");
        _outputLines.Add("");
    }

    private void ShowUptime()
    {
        var uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
        _outputLines.Add($"  ⏱️ System been vibin for: {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s");
        _outputLines.Add("  💪 Still going strong!");
        _outputLines.Add("");
    }

    private void ExitTerminal()
    {
        _outputLines.Add("  👋 Peace out! Stay safe, fam!");
        _outputLines.Add("");
        
        // Close this terminal window
        var terminalWindow = _mainViewModel?.OpenWindows.FirstOrDefault(w => w.AppType == ApplicationType.Terminal);
        if (terminalWindow != null)
        {
            _mainViewModel?.CloseWindowCommand?.Execute(terminalWindow);
        }
    }

    private void ListApps()
    {
        _outputLines.Add("╔══════════════════════════════════════════════════════════════╗");
        _outputLines.Add("║                    📱 AVAILABLE APPS 📱                      ║");
        _outputLines.Add("╠══════════════════════════════════════════════════════════════╣");
        _outputLines.Add("║  📝 notepad    → Text editor                                 ║");
        _outputLines.Add("║  🔢 calc       → Calculator                                  ║");
        _outputLines.Add("║  📁 files      → File Manager                                ║");
        _outputLines.Add("║  🌐 browser    → Web Browser                                 ║");
        _outputLines.Add("║  💻 terminal   → Another terminal (inception!)               ║");
        _outputLines.Add("║  ⚙️ settings   → System Settings                             ║");
        _outputLines.Add("║  📊 monitor    → System Monitor                              ║");
        _outputLines.Add("║  🎵 music      → Music Player                                ║");
        _outputLines.Add("║  🎬 video      → Video Player                                ║");
        _outputLines.Add("║  🖼️ image      → Image Viewer                                ║");
        _outputLines.Add("║  💣 mine       → Minesweeper                                 ║");
        _outputLines.Add("║  📅 calendar   → Calendar                                    ║");
        _outputLines.Add("║  🛡️ antivirus  → Antivirus                                   ║");
        _outputLines.Add("║  📦 compress   → Compression Tool                            ║");
        _outputLines.Add("║  🕐 clock      → Clock                                       ║");
        _outputLines.Add("╚══════════════════════════════════════════════════════════════╝");
        _outputLines.Add("");
    }

    private void ScanSystem()
    {
        _outputLines.Add("  🔍 Scanning for sus activity...");
        _outputLines.Add("  ████████████████████ 100%");
        _outputLines.Add("");
        _outputLines.Add("  ✅ No cap, everything looks clean!");
        _outputLines.Add("  🛡️ 0 threats detected");
        _outputLines.Add("  💯 System is bussin fr fr");
        _outputLines.Add("");
    }

    private void ShowAbout()
    {
        _outputLines.Add("╔══════════════════════════════════════════════════════════════╗");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║     ██████╗ ██████╗ ███╗   ███╗ █████╗  ██████╗ ███████╗    ║");
        _outputLines.Add("║    ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██╔═══██╗██╔════╝    ║");
        _outputLines.Add("║    ██║     ██║   ██║██╔████╔██║███████║██║   ██║███████╗    ║");
        _outputLines.Add("║    ██║     ██║   ██║██║╚██╔╝██║██╔══██║██║   ██║╚════██║    ║");
        _outputLines.Add("║    ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║  ██║╚██████╔╝███████║    ║");
        _outputLines.Add("║     ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝    ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║                  \"Vulnerability as a Service\"                ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("║         100% Vibe Coded by Claude Opus 4.5 (Preview)         ║");
        _outputLines.Add("║                                                              ║");
        _outputLines.Add("╚══════════════════════════════════════════════════════════════╝");
        _outputLines.Add("");
    }

    private void ShowSystemInfo()
    {
        var mode = _mainViewModel?.CurrentMode.ToString() ?? "Unknown";
        var cpu = _mainViewModel?.CpuUsage ?? 0;
        var ram = _mainViewModel?.RamUsed ?? 0;
        var total = _mainViewModel?.RamTotal ?? 0;
        var procs = _mainViewModel?.Processes.Count ?? 0;

        _outputLines.Add("                                                    ");
        _outputLines.Add("        ████████████████████                         ");
        _outputLines.Add("      ██                    ██       coma@comaos");
        _outputLines.Add("    ██   ██████████████████   ██     ────────────────");
        _outputLines.Add("    ██   ██              ██   ██     OS: ComaOS v1.0");
        _outputLines.Add("    ██   ██   ░░░░░░░░   ██   ██     Kernel: ComaKernel");
        _outputLines.Add("    ██   ██   ░░░░░░░░   ██   ██     Shell: ComaShell");
        _outputLines.Add("    ██   ██              ██   ██     Mode: " + mode);
        _outputLines.Add("    ██   ██████████████████   ██     CPU: " + $"{cpu:F1}%");
        _outputLines.Add("      ██                    ██       RAM: " + $"{ram} MB / {total} MB");
        _outputLines.Add("        ████████████████████         Procs: " + procs);
        _outputLines.Add("              ██    ██                Theme: Dark (obvi)");
        _outputLines.Add("          ████████████████           Vibe: Immaculate 💯");
        _outputLines.Add("");
    }

    private void Echo(string[] args)
    {
        if (args.Length == 0)
        {
            _outputLines.Add("  (silence)");
        }
        else
        {
            _outputLines.Add($"  {string.Join(" ", args)}");
        }
        _outputLines.Add("");
    }

    private void ShowDateTime()
    {
        var now = DateTime.Now;
        _outputLines.Add($"  📅 Date: {now:dddd, MMMM d, yyyy}");
        _outputLines.Add($"  🕐 Time: {now:HH:mm:ss}");
        _outputLines.Add($"  🌍 Zone: {TimeZoneInfo.Local.DisplayName}");
        _outputLines.Add("");
    }

    private void CowSay(string[] args)
    {
        var message = args.Length > 0 ? string.Join(" ", args) : "Moo!";
        var border = new string('-', message.Length + 2);
        
        _outputLines.Add($"   {border}");
        _outputLines.Add($"  < {message} >");
        _outputLines.Add($"   {border}");
        _outputLines.Add("          \\   ^__^");
        _outputLines.Add("           \\  (oo)\\_______");
        _outputLines.Add("              (__)\\       )\\/\\");
        _outputLines.Add("                  ||----w |");
        _outputLines.Add("                  ||     ||");
        _outputLines.Add("");
    }

    private void ShowFortune()
    {
        var fortunes = new[]
        {
            "You will debug a bug that will create two more bugs. 🐛",
            "A merge conflict is in your near future. Good luck! 😰",
            "Your code will compile on the first try today! (Just kidding) 😂",
            "Someone will ask you to fix their printer. Decline. 🖨️",
            "You will discover a new Stack Overflow answer. Cherish it. 📚",
            "The force is strong with your commits today. ⭐",
            "You will finally understand regex. LOL, no you won't. 🤯",
            "A rubber duck will solve your biggest problem. 🦆",
            "Your next PR will be approved without changes. (Rare!) ✅",
            "You will resist the urge to rewrite everything. Maybe. 🤔",
            "The semicolon you're missing is on line 42. Always. 😤",
            "Today is a good day to not push to prod. 🚫",
            "Your coffee-to-code ratio is perfectly balanced. ☕",
            "A senior dev will appreciate your comments. Miracles happen! 🌟",
            "Git will be nice to you today. (Error: Fortune not found) 💀"
        };
        
        var random = new Random();
        _outputLines.Add($"  🔮 {fortunes[random.Next(fortunes.Length)]}");
        _outputLines.Add("");
    }

    private void ShowMatrixEffect()
    {
        _outputLines.Add("  Wake up, Neo...");
        _outputLines.Add("  The Matrix has you...");
        _outputLines.Add("  Follow the white rabbit. 🐇");
        _outputLines.Add("");
        _outputLines.Add("  01001000 01100101 01101100 01101100 01101111");
        _outputLines.Add("");
    }

    private void ShowAsciiArt()
    {
        _outputLines.Add("");
        _outputLines.Add("    ╔═══════════════════════════════════╗");
        _outputLines.Add("    ║  ♠ ♥ ♦ ♣  ComaOS Art  ♣ ♦ ♥ ♠   ║");
        _outputLines.Add("    ╚═══════════════════════════════════╝");
        _outputLines.Add("");
        _outputLines.Add("         /\\_/\\  ");
        _outputLines.Add("        ( o.o ) ");
        _outputLines.Add("         > ^ <  ");
        _outputLines.Add("        /|   |\\");
        _outputLines.Add("       (_|   |_)   <- ComaOS mascot");
        _outputLines.Add("");
    }

    private void TellJoke()
    {
        var jokes = new[]
        {
            ("Why do programmers prefer dark mode?", "Because light attracts bugs! 🪲"),
            ("Why did the developer go broke?", "Because he used up all his cache! 💸"),
            ("What's a programmer's favorite hangout place?", "Foo Bar! 🍺"),
            ("Why do Java developers wear glasses?", "Because they don't C#! 👓"),
            ("How many programmers does it take to change a light bulb?", "None, that's a hardware problem! 💡"),
            ("Why was the JavaScript developer sad?", "Because he didn't Node how to Express himself! 😢"),
            ("What's a computer's least favorite food?", "Spam! 🥫"),
            ("Why did the functions stop calling each other?", "They had too many arguments! 😤"),
            ("What do you call 8 hobbits?", "A hobbyte! 🧙"),
            ("Why do programmers hate nature?", "It has too many bugs! 🐜")
        };
        
        var random = new Random();
        var (setup, punchline) = jokes[random.Next(jokes.Length)];
        _outputLines.Add($"  😄 {setup}");
        _outputLines.Add($"  🎯 {punchline}");
        _outputLines.Add("");
    }

    private void FlipCoin()
    {
        var random = new Random();
        var result = random.Next(2) == 0 ? "HEADS 🪙" : "TAILS 🪙";
        _outputLines.Add($"  🎲 Flipping coin...");
        _outputLines.Add($"  ✨ Result: {result}");
        _outputLines.Add("");
    }

    private void RollDice(string[] args)
    {
        int sides = 6;
        if (args.Length > 0 && int.TryParse(args[0], out int parsed))
        {
            sides = Math.Clamp(parsed, 2, 100);
        }
        
        var random = new Random();
        var result = random.Next(1, sides + 1);
        _outputLines.Add($"  🎲 Rolling d{sides}...");
        _outputLines.Add($"  ✨ Result: {result}");
        _outputLines.Add("");
    }

    private void ShowHistory()
    {
        _outputLines.Add("  📜 Command History:");
        if (_commandHistory.Count == 0)
        {
            _outputLines.Add("  (empty - you haven't typed anything yet!)");
        }
        else
        {
            for (int i = 0; i < Math.Min(_commandHistory.Count, 20); i++)
            {
                _outputLines.Add($"  {i + 1,3}. {_commandHistory[i]}");
            }
        }
        _outputLines.Add("");
    }

    private void ScrollToBottom()
    {
        OutputScroller.ScrollToEnd();
    }
}
