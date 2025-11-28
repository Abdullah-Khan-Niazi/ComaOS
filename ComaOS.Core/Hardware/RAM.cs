namespace ComaOS.Core.Hardware;

/// <summary>
/// Represents the Random Access Memory (RAM) of the simulated operating system.
/// Manages memory allocation and deallocation using a block-based system.
/// </summary>
public class RAM
{
    private readonly long _totalSizeMB;
    private readonly List<MemoryBlock> _allocatedBlocks;
    private readonly object _lockObject = new();

    /// <summary>
    /// Gets the total RAM size in megabytes.
    /// </summary>
    public long TotalSizeMB => _totalSizeMB;

    /// <summary>
    /// Gets the currently used RAM in megabytes.
    /// </summary>
    public long UsedSizeMB
    {
        get
        {
            lock (_lockObject)
            {
                return _allocatedBlocks.Sum(b => b.SizeMB);
            }
        }
    }

    /// <summary>
    /// Gets the available free RAM in megabytes.
    /// </summary>
    public long FreeSizeMB => _totalSizeMB - UsedSizeMB;

    /// <summary>
    /// Gets the RAM usage percentage.
    /// </summary>
    public double UsagePercentage => (_totalSizeMB > 0) ? (UsedSizeMB / (double)_totalSizeMB) * 100.0 : 0.0;

    /// <summary>
    /// Initializes a new instance of RAM with the specified size.
    /// </summary>
    /// <param name="sizeMB">Total RAM size in megabytes (minimum 512 MB, maximum 65536 MB).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when sizeMB is invalid.</exception>
    public RAM(long sizeMB)
    {
        if (sizeMB < 512 || sizeMB > 65536)
            throw new ArgumentOutOfRangeException(nameof(sizeMB), "RAM size must be between 512 MB and 65536 MB.");

        _totalSizeMB = sizeMB;
        _allocatedBlocks = new List<MemoryBlock>();
    }

    /// <summary>
    /// Attempts to allocate a block of memory for a process.
    /// </summary>
    /// <param name="processId">The Process ID requesting memory.</param>
    /// <param name="sizeMB">Amount of memory to allocate in megabytes.</param>
    /// <returns>True if allocation succeeded; false if insufficient memory.</returns>
    public bool Allocate(int processId, long sizeMB)
    {
        if (sizeMB <= 0)
            throw new ArgumentException("Size must be positive.", nameof(sizeMB));

        lock (_lockObject)
        {
            if (FreeSizeMB < sizeMB)
                return false; // Insufficient memory

            var block = new MemoryBlock(processId, sizeMB);
            _allocatedBlocks.Add(block);
            return true;
        }
    }

    /// <summary>
    /// Deallocates all memory blocks associated with a specific process.
    /// </summary>
    /// <param name="processId">The Process ID whose memory should be freed.</param>
    /// <returns>Amount of memory freed in megabytes.</returns>
    public long Deallocate(int processId)
    {
        lock (_lockObject)
        {
            var blocksToRemove = _allocatedBlocks.Where(b => b.ProcessId == processId).ToList();
            long freedMemory = blocksToRemove.Sum(b => b.SizeMB);

            foreach (var block in blocksToRemove)
            {
                _allocatedBlocks.Remove(block);
            }

            return freedMemory;
        }
    }

    /// <summary>
    /// Gets the total memory allocated to a specific process.
    /// </summary>
    /// <param name="processId">The Process ID to query.</param>
    /// <returns>Memory allocated in megabytes.</returns>
    public long GetProcessMemory(int processId)
    {
        lock (_lockObject)
        {
            return _allocatedBlocks.Where(b => b.ProcessId == processId).Sum(b => b.SizeMB);
        }
    }

    /// <summary>
    /// Forcefully clears all memory allocations. Only available in Kernel Mode.
    /// </summary>
    public void ClearAll()
    {
        lock (_lockObject)
        {
            _allocatedBlocks.Clear();
        }
    }

    /// <summary>
    /// Gets detailed memory status information.
    /// </summary>
    public string GetMemoryStatus()
    {
        lock (_lockObject)
        {
            return $"RAM: {UsedSizeMB} MB / {_totalSizeMB} MB ({UsagePercentage:F1}% used)\n" +
                   $"Free: {FreeSizeMB} MB\n" +
                   $"Allocated Blocks: {_allocatedBlocks.Count}";
        }
    }

    /// <summary>
    /// Gets a list of all currently allocated memory blocks.
    /// </summary>
    public IReadOnlyList<MemoryBlock> GetAllocatedBlocks()
    {
        lock (_lockObject)
        {
            return _allocatedBlocks.ToList().AsReadOnly();
        }
    }
}

/// <summary>
/// Represents a block of allocated memory.
/// </summary>
public record MemoryBlock(int ProcessId, long SizeMB)
{
    /// <summary>
    /// Gets the timestamp when this block was allocated.
    /// </summary>
    public DateTime AllocatedAt { get; } = DateTime.Now;
}
