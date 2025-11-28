namespace ComaOS.Core.Hardware;

/// <summary>
/// Represents the Central Processing Unit of the simulated operating system.
/// Manages multiple cores and their execution states.
/// </summary>
public class CPU
{
    private readonly int _coreCount;
    private readonly List<Core> _cores;
    private readonly object _lockObject = new();

    /// <summary>
    /// Gets the number of CPU cores available in the system.
    /// </summary>
    public int CoreCount => _coreCount;

    /// <summary>
    /// Gets the collection of all CPU cores.
    /// </summary>
    public IReadOnlyList<Core> Cores => _cores.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the CPU with the specified number of cores.
    /// </summary>
    /// <param name="coreCount">Number of cores to create (minimum 1, maximum 64).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coreCount is invalid.</exception>
    public CPU(int coreCount)
    {
        if (coreCount < 1 || coreCount > 64)
            throw new ArgumentOutOfRangeException(nameof(coreCount), "Core count must be between 1 and 64.");

        _coreCount = coreCount;
        _cores = new List<Core>(coreCount);

        for (int i = 0; i < coreCount; i++)
        {
            _cores.Add(new Core(i));
        }
    }

    /// <summary>
    /// Attempts to allocate an idle core for process execution.
    /// </summary>
    /// <returns>An available Core, or null if all cores are busy.</returns>
    public Core? AllocateCore()
    {
        lock (_lockObject)
        {
            return _cores.FirstOrDefault(c => c.IsIdle);
        }
    }

    /// <summary>
    /// Releases a core, marking it as idle and clearing its assigned process.
    /// </summary>
    /// <param name="coreId">The ID of the core to release.</param>
    public void ReleaseCore(int coreId)
    {
        lock (_lockObject)
        {
            var core = _cores.FirstOrDefault(c => c.CoreId == coreId);
            if (core != null)
            {
                core.Release();
            }
        }
    }

    /// <summary>
    /// Gets the current CPU usage percentage across all cores.
    /// </summary>
    /// <returns>CPU usage as a percentage (0-100).</returns>
    public double GetCpuUsage()
    {
        lock (_lockObject)
        {
            int busyCores = _cores.Count(c => !c.IsIdle);
            return (busyCores / (double)_coreCount) * 100.0;
        }
    }

    /// <summary>
    /// Gets detailed information about all cores and their states.
    /// </summary>
    public string GetCoreStatus()
    {
        lock (_lockObject)
        {
            var status = new System.Text.StringBuilder();
            status.AppendLine($"CPU: {_coreCount} Core(s)");
            foreach (var core in _cores)
            {
                status.AppendLine($"  Core {core.CoreId}: {(core.IsIdle ? "IDLE" : $"Running PID {core.CurrentProcessId}")}");
            }
            return status.ToString();
        }
    }
}

/// <summary>
/// Represents a single CPU core that can execute one process at a time.
/// </summary>
public class Core
{
    private int? _currentProcessId;

    /// <summary>
    /// Gets the unique identifier for this core.
    /// </summary>
    public int CoreId { get; }

    /// <summary>
    /// Gets whether this core is currently idle (not executing a process).
    /// </summary>
    public bool IsIdle => _currentProcessId == null;

    /// <summary>
    /// Gets the Process ID currently running on this core, or null if idle.
    /// </summary>
    public int? CurrentProcessId => _currentProcessId;

    /// <summary>
    /// Initializes a new instance of a CPU core.
    /// </summary>
    /// <param name="coreId">Unique identifier for this core.</param>
    public Core(int coreId)
    {
        CoreId = coreId;
        _currentProcessId = null;
    }

    /// <summary>
    /// Assigns a process to this core for execution.
    /// </summary>
    /// <param name="processId">The Process ID to execute.</param>
    /// <exception cref="InvalidOperationException">Thrown if core is already busy.</exception>
    public void AssignProcess(int processId)
    {
        if (!IsIdle)
            throw new InvalidOperationException($"Core {CoreId} is already busy with PID {_currentProcessId}.");

        _currentProcessId = processId;
    }

    /// <summary>
    /// Releases the core, marking it as idle.
    /// </summary>
    public void Release()
    {
        _currentProcessId = null;
    }
}
