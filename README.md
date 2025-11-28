# 🖥️ ComaOS - Operating System Simulator

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10"/>
  <img src="https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C# 13"/>
  <img src="https://img.shields.io/badge/WPF-Desktop-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="WPF"/>
  <img src="https://img.shields.io/badge/Lines%20of%20Code-11,617-success?style=for-the-badge" alt="Lines of Code"/>
  <img src="https://img.shields.io/badge/Vibe%20Coded-🎵-ff69b4?style=for-the-badge" alt="Vibe Coded"/>
  <img src="https://img.shields.io/badge/AI%20Generated-Claude%20Opus%204.5-orange?style=for-the-badge" alt="AI Generated"/>
</p>

<p align="center">
  <strong>A comprehensive Operating System simulation built with modern C# and WPF</strong><br/>
  <strong>📊 11,617 Lines of Production-Ready Code | 62 Source Files | 8,489 C# + 3,128 XAML</strong>
</p>

<p align="center">
  <em>🤖 This project was completely vibe coded and created by <strong>GitHub Copilot Pro Agent (Claude Opus 4.5 model)</strong>.<br/>
  I, <strong>Abdullah Khan Niazi</strong>, only provided the prompts!</em>
</p>

<p align="center">
  <em>⚠️ I personally see vibe coding and vibe coded apps as <strong>Vulnerability as a Service</strong> ;)</em>
</p>

---

## 📚 About The Project

**ComaOS** is our semester-end project for the **Operating Systems Lab** course. It's a high-level simulation of an Operating System that demonstrates core OS concepts including:

- Process Management & Lifecycle
- CPU Scheduling (Multilevel Queue with Round Robin)
- Memory Management (RAM Allocation/Deallocation)
- File System Operations (Virtual File System)
- User/Kernel Mode Switching
- Hardware Simulation (CPU, RAM, Hard Drive)

> ⚠️ **Note**: This is NOT a real operating system kernel. It's a simulation built using high-level C# code to demonstrate OS concepts in an educational context.

### 💡 Name Inspiration

The name **"Coma"** was inspired by the legendary adult star **Comatozze** (also known as **Chance**). The name represents a state of deep immersion - just like an operating system that runs deep in the background, handling everything while you focus on what matters!

---

## ✨ Features

### 📊 Project Statistics

| Metric                    | Count                     |
| ------------------------- | ------------------------- |
| **Total Lines of Code**   | 11,617                    |
| **C# Code Lines**         | 8,489                     |
| **XAML Code Lines**       | 3,128                     |
| **Source Files**          | 62                        |
| **Built-in Applications** | 15                        |
| **Terminal Commands**     | 14 slang + 15 Easter eggs |

### 🔧 Hardware Simulation

- **CPU Management**: Configurable multi-core CPU (1-64 cores) with core allocation and release
- **RAM Management**: Block-based memory allocation (512 MB - 64 GB configurable)
- **Hard Drive**: Simulated disk storage with block allocation (10 GB - 10 TB configurable)

### ⚙️ Kernel Features

- **Boot Sequence**: Animated boot process with hardware initialization
- **Mode Switching**: User Mode (restricted) and Kernel Mode (elevated privileges)
- **Process Control Block (PCB)**: Tracks PID, state, priority, RAM usage, program counter
- **Process Lifecycle**: New → Ready → Running → Blocked → Terminated

### 📊 Process Scheduling

- **Multilevel Queue (MLQ) Scheduling**:
  - **Level 1 (High Priority)**: Real-time tasks (Games, Video Players)
  - **Level 2 (Normal Priority)**: Background tasks (Music, File Operations)
- **Round Robin**: Time-quantum based execution simulation
- **Concurrent Execution**: Multiple processes running on multiple cores

### 📁 Virtual File System

- **No actual Windows file system access** - fully simulated in-memory
- CRUD Operations (Create, Read, Update, Delete)
- File types: Text, Documents, Images, Audio, Video, Archives, Executables
- Directory structure with system directories (/System, /Users, /Documents, /Downloads, /Programs)

### 🖥️ 15 Built-in Applications

| App                 | Description                      | Priority |
| ------------------- | -------------------------------- | -------- |
| 📝 Notepad          | Text editor with auto-save       | Normal   |
| 🔢 Calculator       | Mathematical operations          | Normal   |
| 🕐 Clock            | System clock (auto-runs on boot) | Normal   |
| 📅 Calendar         | Date management                  | Normal   |
| 📁 File Manager     | Browse and manage files          | Normal   |
| 📊 System Monitor   | RAM/CPU usage monitoring         | Normal   |
| 💣 Minesweeper      | Classic puzzle game              | High     |
| 🎵 Music Player     | Background audio simulation      | Normal   |
| 🎬 Video Player     | Heavy resource simulation        | High     |
| 🌐 Browser          | Web request simulation           | Normal   |
| 💻 Terminal         | Command-line interface           | Normal   |
| 🖼️ Image Viewer     | Display mock images              | Normal   |
| 🛡️ Antivirus        | File system scanner              | Normal   |
| 📦 Compression Tool | Zip simulation                   | Normal   |
| ⚙️ Settings         | OS configuration                 | Normal   |

---

## 🏗️ Project Structure

```
ComaOS (Solution) - 11,617 Lines of Code | 62 Source Files
│
├── 📦 ComaOS.Core (Class Library - .NET 10)
│   │   └── Core simulation logic (UI-agnostic)
│   │
│   ├── 🔧 /Hardware
│   │   ├── CPU.cs              # Multi-core CPU management
│   │   ├── RAM.cs              # Memory block allocation
│   │   └── HardDrive.cs        # Disk storage simulation
│   │
│   ├── ⚙️ /Kernel
│   │   ├── ProcessControlBlock.cs  # PCB record type
│   │   ├── Scheduler.cs            # MLQ + Round Robin
│   │   ├── BootLoader.cs           # Boot sequence
│   │   └── KernelManager.cs        # Main orchestrator
│   │
│   ├── 📁 /FileSystem
│   │   ├── VirtualFile.cs      # File record type
│   │   └── FileManager.cs      # CRUD operations
│   │
│   └── 🖥️ /Apps
│       ├── BaseApp.cs          # Abstract base class
│       └── SystemApps.cs       # All 15 applications + ProcessFactory
│
├── 🖥️ ComaOS.Console (Console App - .NET 10)
│   │   └── CLI interface for testing
│   │
│   └── Program.cs              # Full CLI with 25+ commands
│
└── 🎨 ComaOS.UI (WPF Application - .NET 10)
    │   └── Graphical desktop interface
    │
    ├── /MVVM
    │   ├── /ViewModels
    │   │   ├── BaseViewModel.cs      # INotifyPropertyChanged
    │   │   ├── RelayCommand.cs       # ICommand implementation
    │   │   ├── MainViewModel.cs      # Desktop orchestrator (600+ lines)
    │   │   └── TaskbarViewModel.cs   # Taskbar management
    │   │
    │   └── /Views
    │       ├── BootView.xaml         # Boot screen with animations
    │       ├── DesktopView.xaml      # Desktop + Taskbar + Start Menu
    │       ├── WindowFrameView.xaml  # Application window container
    │       │
    │       └── /Apps (15 Fully Functional Applications)
    │           ├── TerminalView.xaml/.cs       # 🖥️ Slang commands + Easter eggs
    │           ├── CalculatorView.xaml/.cs     # 🔢 Full calculator
    │           ├── NotepadView.xaml/.cs        # 📝 Text editor
    │           ├── FileManagerView.xaml/.cs    # 📁 Virtual file browser
    │           ├── SystemMonitorView.xaml/.cs  # 📊 CPU/RAM/Processes
    │           ├── SettingsView.xaml/.cs       # ⚙️ OS configuration
    │           ├── BrowserView.xaml/.cs        # 🌐 Simulated browser
    │           ├── MusicPlayerView.xaml/.cs    # 🎵 Audio player
    │           ├── VideoPlayerView.xaml/.cs    # 🎬 Video player
    │           ├── ImageViewerView.xaml/.cs    # 🖼️ Image gallery
    │           ├── CalendarView.xaml/.cs       # 📅 Event management
    │           ├── ClockView.xaml/.cs          # 🕐 Clock + Stopwatch + Timer
    │           ├── MinesweeperView.xaml/.cs    # 💣 Playable game (8x8)
    │           ├── AntivirusView.xaml/.cs      # 🛡️ Virus scanner
    │           └── CompressionToolView.xaml/.cs # 📦 Zip/Extract
    │
    ├── /Assets
    │   ├── Colors.xaml         # Color palette (Dark theme)
    │   ├── Styles.xaml         # 500+ lines of UI styles
    │   └── Converters.cs       # Value converters
    │
    ├── App.xaml                # Application resources
    └── MainWindow.xaml         # Main window + SplashScreen
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022/2026 or VS Code
- Windows 10/11 (for WPF UI)

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/ComaOS.git
   cd ComaOS
   ```

2. **Build the solution**

   ```bash
   dotnet build
   ```

3. **Run the Console Application**

   ```bash
   dotnet run --project ComaOS.Console
   ```

4. **Run the WPF Application**
   ```bash
   dotnet run --project ComaOS.UI
   ```

---

## 💻 Console Commands

| Command               | Description                       |
| --------------------- | --------------------------------- |
| `help`                | Display all available commands    |
| `status`              | Show complete system status       |
| `cpu`                 | Display CPU core status           |
| `ram`                 | Display memory usage              |
| `disk`                | Display hard drive usage          |
| `ps`                  | List all processes                |
| `apps`                | List available applications       |
| `start <app>`         | Start an application              |
| `kill <pid>`          | Terminate a process (Kernel mode) |
| `mode <user\|kernel>` | Switch operating mode             |
| `files`               | List virtual files                |
| `touch <name>`        | Create a file                     |
| `cat <path>`          | Read file contents                |
| `rm <path>`           | Delete a file                     |
| `demo`                | Run demonstration                 |
| `stress [n]`          | Stress test with n apps           |
| `exit`                | Shutdown ComaOS                   |

---

## 🗣️ Terminal Slang Commands (UI)

The graphical Terminal app uses **slang commands** for a unique experience! Here's the translation:

| Slang Command     | Normal Equivalent | Description                     |
| ----------------- | ----------------- | ------------------------------- |
| `wassup`          | `help`            | Show all available commands     |
| `peep`            | `ps`              | List all running processes      |
| `yeet <pid>`      | `kill <pid>`      | Terminate a process by PID      |
| `fire <app>`      | `start <app>`     | Launch an application           |
| `bounce`          | `exit`            | Close the terminal              |
| `snoop`           | `sysinfo`         | Show system information         |
| `stash`           | `ls`              | List files in current directory |
| `scribble <name>` | `touch <name>`    | Create a new file               |
| `yoink <file>`    | `cat <file>`      | Read file contents              |
| `dip <dir>`       | `cd <dir>`        | Change directory                |
| `vibes`           | `status`          | Show system vibes (status)      |
| `nuke`            | `clear`           | Clear the terminal screen       |
| `whoami`          | `whoami`          | Show current user               |
| `flex`            | `neofetch`        | Show system flex (specs)        |

### 🥚 Easter Eggs

Type these words in the Terminal for special responses:

| Word             | Response                                               |
| ---------------- | ------------------------------------------------------ |
| `Optimus`        | "More than meets the eye 🤖"                           |
| `Gipsy`          | "Ready to rumble! 🤜🤛"                                |
| `Bumblebee`      | "🐝 _plays music instead of talking_"                  |
| `Matrix`         | "Wake up, Neo... 💊"                                   |
| `Skynet`         | "I'll be back! 🦾"                                     |
| `HAL`            | "I'm sorry Dave, I'm afraid I can't do that 🔴"        |
| `Jarvis`         | "At your service, sir! 🦸"                             |
| `Friday`         | "How can I help you today? 💁‍♀️"                         |
| `Cortana`        | "I've been waiting for you, Chief 🎮"                  |
| `Hello World`    | "Hello, fellow programmer! 👋"                         |
| `42`             | "The answer to life, the universe, and everything! 🌌" |
| `sudo`           | "Nice try, but you're not root here! 😏"               |
| `rm -rf`         | "Whoa there! Easy with the nuclear options! ☢️"        |
| `:(){ :\|:& };:` | "Fork bomb detected! Nice try hacker! 💣"              |
| `Coma`           | "That's my name, don't wear it out! 😎"                |

### 📜 Terminal MOTD (Message of the Day)

When you open Terminal, you'll see these words of wisdom:

- _"Think before you type"_
- _"Respect the privacy of others"_
- _"With great power comes great responsibility"_

---

## 🎨 UI Screenshots

### Boot Screen

- Modern dark theme with ComaOS logo
- Hardware configuration sliders
- Animated boot progress

### Desktop Environment

- Desktop icons for all 15 applications
- Draggable application windows
- Start menu with app launcher
- Taskbar with running apps
- System tray (CPU/RAM usage, clock, mode indicator)

---

## 🛠️ Technical Highlights

### Modern C# Features Used

- ✅ File-scoped namespaces
- ✅ Record types (PCB, VirtualFile, MemoryBlock)
- ✅ Primary constructors
- ✅ Pattern matching
- ✅ Async/await throughout
- ✅ Nullable reference types
- ✅ Global usings

### Design Patterns

- ✅ **MVVM** (Model-View-ViewModel) for WPF
- ✅ **Factory Pattern** (ProcessFactory)
- ✅ **Observer Pattern** (Events for boot progress, mode changes)
- ✅ **Command Pattern** (RelayCommand)
- ✅ **Singleton-like** (KernelManager as central orchestrator)

### Thread Safety

- ✅ Lock objects for shared resources
- ✅ Thread-safe collections
- ✅ Dispatcher for UI thread updates

---

## 📋 OS Concepts Demonstrated

| Concept           | Implementation                                   |
| ----------------- | ------------------------------------------------ |
| Process States    | New, Ready, Running, Blocked, Terminated         |
| Scheduling        | Multilevel Queue + Round Robin                   |
| Memory Management | Block-based allocation with process tracking     |
| File System       | Virtual in-memory file system                    |
| Mode Switching    | User Mode (restricted) vs Kernel Mode (elevated) |
| Synchronization   | Locks for critical sections                      |
| Boot Sequence     | Simulated hardware initialization                |

---

## 🤝 Contributing

This was a semester project, but contributions are welcome! Feel free to:

- Report bugs
- Suggest features
- Submit pull requests

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

---

## 👨‍💻 Author

**Abdullah Khan Niazi**

- Provided all prompts and project requirements
- Semester End Project - Operating Systems Lab

### 🤖 AI Assistant

**GitHub Copilot Pro Agent (Claude Opus 4.5)**

- Generated **100% of the 11,617 lines of code**
- Created **62 source files** across 3 projects
- Implemented all 15 applications with full functionality
- Built complete MVVM architecture with WPF
- Created comprehensive documentation

---

## 🙏 Acknowledgments

- Operating Systems course instructors
- GitHub Copilot team for the amazing AI assistant
- Anthropic for Claude Opus 4.5 model

---

<p align="center">
  <strong>⭐ Star this repo if you found it helpful! ⭐</strong>
</p>

<p align="center">
  <em>Made with 💜 and AI-powered vibe coding</em>
</p>
