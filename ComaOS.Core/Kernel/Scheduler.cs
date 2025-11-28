using ComaOS.Core.Hardware;

namespace ComaOS.Core.Kernel;

/// <summary>
/// Implements Multilevel Queue (MLQ) Scheduling with Round Robin.
/// Level 1: High Priority (Real-time tasks)
/// Level 2: Background tasks
/// </summary>
public class Scheduler
{
    private readonly CPU _cpu;
    private readonly RAM _ram;
    private readonly Queue<ProcessControlBlock> _highPriorityQueue;
    private readonly Queue<ProcessControlBlock> _normalPriorityQueue;
    private readonly List<ProcessControlBlock> _runningProcesses;
    private readonly List<ProcessControlBlock> _blockedProcesses;
    private readonly List<ProcessControlBlock> _terminatedProcesses;
    private readonly object _lockObject = new();
    private readonly CancellationTokenSource _schedulerCts;
    private Task? _schedulerTask;
    private readonly int _timeQuantumMs = 100; // Time slice for Round Robin

    /// <summary>
    /// Gets all processes currently in the ready queues.
    /// </summary>
    public IReadOnlyList<ProcessControlBlock> ReadyProcesses
    {
        get
        {
            lock (_lockObject)
            {
                return _highPriorityQueue.Concat(_normalPriorityQueue).ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets all processes currently running on CPU cores.
    /// </summary>
    public IReadOnlyList<ProcessControlBlock> RunningProcesses
    {
        get
        {
            lock (_lockObject)
            {
                return _runningProcesses.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets all blocked processes waiting for I/O.
    /// </summary>
    public IReadOnlyList<ProcessControlBlock> BlockedProcesses
    {
        get
        {
            lock (_lockObject)
            {
                return _blockedProcesses.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets all terminated processes.
    /// </summary>
    public IReadOnlyList<ProcessControlBlock> TerminatedProcesses
    {
        get
        {
            lock (_lockObject)
            {
                return _terminatedProcesses.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets all active processes (New, Ready, Running, Blocked states).
    /// </summary>
    public IReadOnlyList<ProcessControlBlock> AllActiveProcesses
    {
        get
        {
            lock (_lockObject)
            {
                return _highPriorityQueue
                    .Concat(_normalPriorityQueue)
                    .Concat(_runningProcesses)
                    .Concat(_blockedProcesses)
                    .ToList()
                    .AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the Scheduler.
    /// </summary>
    /// <param name="cpu">The CPU to manage.</param>
    /// <param name="ram">The RAM to check for availability.</param>
    public Scheduler(CPU cpu, RAM ram)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _ram = ram ?? throw new ArgumentNullException(nameof(ram));
        _highPriorityQueue = new Queue<ProcessControlBlock>();
        _normalPriorityQueue = new Queue<ProcessControlBlock>();
        _runningProcesses = new List<ProcessControlBlock>();
        _blockedProcesses = new List<ProcessControlBlock>();
        _terminatedProcesses = new List<ProcessControlBlock>();
        _schedulerCts = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts the scheduler loop that manages process execution.
    /// </summary>
    public void Start()
    {
        if (_schedulerTask != null && !_schedulerTask.IsCompleted)
            return; // Already running

        _schedulerTask = Task.Run(async () => await SchedulerLoop(_schedulerCts.Token));
    }

    /// <summary>
    /// Stops the scheduler gracefully.
    /// </summary>
    public async Task StopAsync()
    {
        _schedulerCts.Cancel();
        if (_schedulerTask != null)
        {
            await _schedulerTask;
        }
    }

    /// <summary>
    /// Adds a process to the appropriate ready queue based on priority.
    /// </summary>
    /// <param name="pcb">The Process Control Block to add.</param>
    public void AddProcess(ProcessControlBlock pcb)
    {
        lock (_lockObject)
        {
            pcb.State = ProcessState.Ready;

            if (pcb.Priority == ProcessPriority.High)
            {
                _highPriorityQueue.Enqueue(pcb);
            }
            else
            {
                _normalPriorityQueue.Enqueue(pcb);
            }
        }
    }

    /// <summary>
    /// Terminates a process by its ID. Only available in Kernel Mode.
    /// </summary>
    /// <param name="processId">The Process ID to terminate.</param>
    /// <param name="isKernelMode">Whether the operation is performed in Kernel Mode.</param>
    /// <returns>True if process was terminated; false if not found or permission denied.</returns>
    public bool TerminateProcess(int processId, bool isKernelMode)
    {
        if (!isKernelMode)
            return false; // Only kernel mode can kill processes

        lock (_lockObject)
        {
            // Find the process in any state
            ProcessControlBlock? pcb = null;

            pcb = _runningProcesses.FirstOrDefault(p => p.ProcessId == processId);
            if (pcb != null)
            {
                _runningProcesses.Remove(pcb);
                if (pcb.AssignedCoreId.HasValue)
                {
                    _cpu.ReleaseCore(pcb.AssignedCoreId.Value);
                }
            }
            else
            {
                // Try to remove from high priority queue
                pcb = RemoveFromQueue(_highPriorityQueue, processId);
                
                if (pcb == null)
                {
                    // Try to remove from normal priority queue
                    pcb = RemoveFromQueue(_normalPriorityQueue, processId);
                }
                
                if (pcb == null)
                {
                    pcb = _blockedProcesses.FirstOrDefault(p => p.ProcessId == processId);
                    if (pcb != null)
                    {
                        _blockedProcesses.Remove(pcb);
                    }
                }
            }

            if (pcb == null)
                return false; // Process not found

            pcb.State = ProcessState.Terminated;
            pcb.TerminatedAt = DateTime.Now;
            _terminatedProcesses.Add(pcb);

            // Deallocate RAM
            _ram.Deallocate(pcb.ProcessId);

            return true;
        }
    }

    /// <summary>
    /// Helper method to remove a process from a queue by ID.
    /// </summary>
    private ProcessControlBlock? RemoveFromQueue(Queue<ProcessControlBlock> queue, int processId)
    {
        var tempList = new List<ProcessControlBlock>();
        ProcessControlBlock? found = null;

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            if (item.ProcessId == processId)
            {
                found = item;
            }
            else
            {
                tempList.Add(item);
            }
        }

        // Re-enqueue remaining items
        foreach (var item in tempList)
        {
            queue.Enqueue(item);
        }

        return found;
    }

    /// <summary>
    /// Gets a process by its ID from all queues.
    /// </summary>
    public ProcessControlBlock? GetProcessById(int processId)
    {
        lock (_lockObject)
        {
            return AllActiveProcesses.FirstOrDefault(p => p.ProcessId == processId)
                ?? _terminatedProcesses.FirstOrDefault(p => p.ProcessId == processId);
        }
    }

    /// <summary>
    /// The main scheduling loop implementing MLQ with Round Robin.
    /// </summary>
    private async Task SchedulerLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_timeQuantumMs, cancellationToken);

                lock (_lockObject)
                {
                    // Check for processes that have completed execution
                    var completedProcesses = _runningProcesses.Where(p => p.ProgramCounter >= 100).ToList();
                    foreach (var process in completedProcesses)
                    {
                        process.State = ProcessState.Terminated;
                        process.TerminatedAt = DateTime.Now;
                        _runningProcesses.Remove(process);
                        _terminatedProcesses.Add(process);

                        if (process.AssignedCoreId.HasValue)
                        {
                            _cpu.ReleaseCore(process.AssignedCoreId.Value);
                        }

                        _ram.Deallocate(process.ProcessId);
                    }

                    // Attempt to schedule processes from high priority queue first
                    ScheduleFromQueue(_highPriorityQueue);

                    // Then schedule from normal priority queue
                    ScheduleFromQueue(_normalPriorityQueue);

                    // Simulate process execution (increment program counter)
                    foreach (var process in _runningProcesses)
                    {
                        // Simulate work: increment by a percentage based on time quantum
                        int increment = (int)((double)_timeQuantumMs / process.ExecutionTimeMs * 100);
                        process.ProgramCounter = Math.Min(100, process.ProgramCounter + increment);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log exception in production
                Console.WriteLine($"Scheduler error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Attempts to schedule processes from a given queue to available CPU cores.
    /// </summary>
    private void ScheduleFromQueue(Queue<ProcessControlBlock> queue)
    {
        while (queue.Count > 0)
        {
            var core = _cpu.AllocateCore();
            if (core == null)
                break; // No available cores

            var pcb = queue.Dequeue();
            pcb.State = ProcessState.Running;
            pcb.StartedAt ??= DateTime.Now;
            pcb.AssignedCoreId = core.CoreId;
            core.AssignProcess(pcb.ProcessId);
            _runningProcesses.Add(pcb);
        }
    }

    /// <summary>
    /// Gets statistics about the scheduler state.
    /// </summary>
    public string GetSchedulerStatus()
    {
        lock (_lockObject)
        {
            return $"Scheduler Status:\n" +
                   $"  High Priority Queue: {_highPriorityQueue.Count}\n" +
                   $"  Normal Priority Queue: {_normalPriorityQueue.Count}\n" +
                   $"  Running: {_runningProcesses.Count}\n" +
                   $"  Blocked: {_blockedProcesses.Count}\n" +
                   $"  Terminated: {_terminatedProcesses.Count}";
        }
    }
}
