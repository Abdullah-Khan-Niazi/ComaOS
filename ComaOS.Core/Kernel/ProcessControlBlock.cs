namespace ComaOS.Core.Kernel;

/// <summary>
/// Represents the Process Control Block (PCB) that contains all metadata about a process.
/// This is a record type for immutable process state representation.
/// </summary>
public record ProcessControlBlock
{
    /// <summary>
    /// Gets the unique Process ID.
    /// </summary>
    public int ProcessId { get; init; }

    /// <summary>
    /// Gets the name of the process/application.
    /// </summary>
    public string ProcessName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current state of the process.
    /// </summary>
    public ProcessState State { get; set; }

    /// <summary>
    /// Gets the priority level of the process (1 = High Priority, 2 = Normal/Background).
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Gets the amount of RAM allocated to this process in megabytes.
    /// </summary>
    public long RamUsageMB { get; init; }

    /// <summary>
    /// Gets the Program Counter (simulated execution progress, 0-100%).
    /// </summary>
    public int ProgramCounter { get; set; }

    /// <summary>
    /// Gets the timestamp when the process was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the process started running (null if never started).
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets the timestamp when the process terminated (null if still running).
    /// </summary>
    public DateTime? TerminatedAt { get; set; }

    /// <summary>
    /// Gets the Core ID this process is currently assigned to (null if not assigned).
    /// </summary>
    public int? AssignedCoreId { get; set; }

    /// <summary>
    /// Gets the total execution time in milliseconds (for simulation).
    /// </summary>
    public int ExecutionTimeMs { get; init; }

    /// <summary>
    /// Gets whether this process runs in the background.
    /// </summary>
    public bool IsBackgroundProcess { get; init; }

    /// <summary>
    /// Gets any additional metadata for the process.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Creates a new ProcessControlBlock with the specified parameters.
    /// </summary>
    public ProcessControlBlock(
        int processId,
        string processName,
        int priority,
        long ramUsageMB,
        int executionTimeMs,
        bool isBackgroundProcess = false)
    {
        ProcessId = processId;
        ProcessName = processName;
        State = ProcessState.New;
        Priority = priority;
        RamUsageMB = ramUsageMB;
        ProgramCounter = 0;
        CreatedAt = DateTime.Now;
        ExecutionTimeMs = executionTimeMs;
        IsBackgroundProcess = isBackgroundProcess;
    }

    /// <summary>
    /// Gets a string representation of the PCB for debugging.
    /// </summary>
    public override string ToString()
    {
        return $"[PID:{ProcessId}] {ProcessName} | State:{State} | Priority:{Priority} | RAM:{RamUsageMB}MB | PC:{ProgramCounter}%";
    }
}

/// <summary>
/// Defines the possible states of a process in its lifecycle.
/// </summary>
public enum ProcessState
{
    /// <summary>
    /// Process has been created but not yet admitted to the ready queue.
    /// </summary>
    New,

    /// <summary>
    /// Process is waiting in the ready queue to be assigned to a CPU core.
    /// </summary>
    Ready,

    /// <summary>
    /// Process is currently executing on a CPU core.
    /// </summary>
    Running,

    /// <summary>
    /// Process is waiting for an I/O operation or event to complete.
    /// </summary>
    Blocked,

    /// <summary>
    /// Process has finished execution and resources are being cleaned up.
    /// </summary>
    Terminated
}

/// <summary>
/// Defines priority levels for processes.
/// </summary>
public static class ProcessPriority
{
    /// <summary>
    /// High priority for real-time tasks (games, video players, etc.).
    /// </summary>
    public const int High = 1;

    /// <summary>
    /// Normal/Background priority for background tasks (music, file operations, etc.).
    /// </summary>
    public const int Normal = 2;
}
