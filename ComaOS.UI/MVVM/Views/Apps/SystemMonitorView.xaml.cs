using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ComaOS.Core.Kernel;
using ComaOS.UI.MVVM.ViewModels;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// System Monitor application showing CPU, RAM, and process information.
/// </summary>
public partial class SystemMonitorView : UserControl
{
    private readonly ObservableCollection<ProcessInfo> _processes = new();
    private readonly DispatcherTimer _updateTimer;
    private MainViewModel? _mainViewModel;

    public SystemMonitorView()
    {
        InitializeComponent();
        ProcessGrid.ItemsSource = _processes;
        
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _updateTimer.Tick += UpdateTimer_Tick;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow?.DataContext is MainViewModel vm)
        {
            _mainViewModel = vm;
        }
        
        _updateTimer.Start();
        UpdateStats();
        UpdateProcessList();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        _updateTimer.Stop();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        UpdateStats();
        UpdateProcessList();
    }

    private void UpdateStats()
    {
        if (_mainViewModel == null) return;

        // Update CPU
        var cpuUsage = _mainViewModel.CpuUsage;
        CpuBar.Value = cpuUsage;
        CpuText.Text = $"{cpuUsage:F0}%";
        CpuInfo.Text = $"{_mainViewModel.Processes.Count} active processes";

        // Update RAM
        var ramUsage = _mainViewModel.RamUsage;
        RamBar.Value = ramUsage;
        RamText.Text = $"{ramUsage:F0}%";
        RamInfo.Text = $"{_mainViewModel.RamUsed} MB / {_mainViewModel.RamTotal} MB";
    }

    private void UpdateProcessList()
    {
        if (_mainViewModel == null) return;

        var currentProcesses = _mainViewModel.Processes.ToList();

        // Update existing processes
        foreach (var proc in currentProcesses)
        {
            var existing = _processes.FirstOrDefault(p => p.ProcessId == proc.ProcessId);
            if (existing != null)
            {
                existing.State = proc.State;
                existing.Progress = proc.Progress;
            }
            else
            {
                _processes.Add(new ProcessInfo
                {
                    ProcessId = proc.ProcessId,
                    Name = proc.Name,
                    State = proc.State,
                    Priority = proc.Priority,
                    Memory = proc.RamUsage,
                    Progress = proc.Progress
                });
            }
        }

        // Remove terminated processes
        var toRemove = _processes.Where(p => !currentProcesses.Any(c => c.ProcessId == p.ProcessId)).ToList();
        foreach (var p in toRemove)
        {
            _processes.Remove(p);
        }
    }

    private void EndTask_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessGrid.SelectedItem is ProcessInfo process)
        {
            // Find the corresponding window and close it
            var window = _mainViewModel?.OpenWindows.FirstOrDefault(w => w.ProcessId == process.ProcessId);
            if (window != null)
            {
                var result = MessageBox.Show($"End task '{process.Name}' (PID: {process.ProcessId})?", 
                    "End Task", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _mainViewModel?.CloseWindowCommand?.Execute(window);
                }
            }
            else
            {
                MessageBox.Show("Cannot end system process!", "System Monitor", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            MessageBox.Show("Select a process to end!", "System Monitor", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

public class ProcessInfo : ComaOS.UI.MVVM.ViewModels.BaseViewModel
{
    private ProcessState _state;
    private int _progress;

    public int ProcessId { get; set; }
    public string Name { get; set; } = "";
    public int Priority { get; set; }
    public long Memory { get; set; }

    public ProcessState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StateText));
            }
        }
    }

    public int Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
            }
        }
    }

    public string StateText => State switch
    {
        ProcessState.Running => "🟢 Running",
        ProcessState.Ready => "🟡 Ready",
        ProcessState.Blocked => "🔴 Blocked",
        ProcessState.Terminated => "⚫ Ended",
        _ => "⚪ Unknown"
    };

    public string PriorityText => Priority switch
    {
        <= 10 => "🔴 High",
        <= 20 => "🟡 Normal",
        _ => "🟢 Low"
    };

    public string MemoryText => $"{Memory} MB";
    public string ProgressText => $"{Progress}%";
}
