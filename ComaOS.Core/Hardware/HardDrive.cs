namespace ComaOS.Core.Hardware;

/// <summary>
/// Represents the Hard Disk Drive of the simulated operating system.
/// Manages disk storage using a block-based allocation system.
/// </summary>
public class HardDrive
{
    private readonly long _totalSizeGB;
    private readonly List<DiskBlock> _allocatedBlocks;
    private readonly object _lockObject = new();

    /// <summary>
    /// Gets the total hard drive size in gigabytes.
    /// </summary>
    public long TotalSizeGB => _totalSizeGB;

    /// <summary>
    /// Gets the currently used disk space in gigabytes.
    /// </summary>
    public long UsedSizeGB
    {
        get
        {
            lock (_lockObject)
            {
                return _allocatedBlocks.Sum(b => b.SizeGB);
            }
        }
    }

    /// <summary>
    /// Gets the available free disk space in gigabytes.
    /// </summary>
    public long FreeSizeGB => _totalSizeGB - UsedSizeGB;

    /// <summary>
    /// Gets the disk usage percentage.
    /// </summary>
    public double UsagePercentage => (_totalSizeGB > 0) ? (UsedSizeGB / (double)_totalSizeGB) * 100.0 : 0.0;

    /// <summary>
    /// Initializes a new instance of HardDrive with the specified size.
    /// </summary>
    /// <param name="sizeGB">Total disk size in gigabytes (minimum 10 GB, maximum 10240 GB).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when sizeGB is invalid.</exception>
    public HardDrive(long sizeGB)
    {
        if (sizeGB < 10 || sizeGB > 10240)
            throw new ArgumentOutOfRangeException(nameof(sizeGB), "Hard drive size must be between 10 GB and 10240 GB.");

        _totalSizeGB = sizeGB;
        _allocatedBlocks = new List<DiskBlock>();
    }

    /// <summary>
    /// Attempts to allocate disk space for a file or data.
    /// </summary>
    /// <param name="fileName">Name of the file/data block.</param>
    /// <param name="sizeGB">Amount of disk space to allocate in gigabytes.</param>
    /// <param name="blockId">Output parameter containing the allocated block ID.</param>
    /// <returns>True if allocation succeeded; false if insufficient space.</returns>
    public bool AllocateDiskSpace(string fileName, long sizeGB, out int blockId)
    {
        blockId = -1;

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));

        if (sizeGB <= 0)
            throw new ArgumentException("Size must be positive.", nameof(sizeGB));

        lock (_lockObject)
        {
            if (FreeSizeGB < sizeGB)
                return false; // Insufficient disk space

            blockId = _allocatedBlocks.Count > 0 ? _allocatedBlocks.Max(b => b.BlockId) + 1 : 1;
            var block = new DiskBlock(blockId, fileName, sizeGB);
            _allocatedBlocks.Add(block);
            return true;
        }
    }

    /// <summary>
    /// Deallocates disk space by block ID.
    /// </summary>
    /// <param name="blockId">The block ID to deallocate.</param>
    /// <returns>Amount of disk space freed in gigabytes.</returns>
    public long DeallocateDiskSpace(int blockId)
    {
        lock (_lockObject)
        {
            var block = _allocatedBlocks.FirstOrDefault(b => b.BlockId == blockId);
            if (block == null)
                return 0;

            _allocatedBlocks.Remove(block);
            return block.SizeGB;
        }
    }

    /// <summary>
    /// Deallocates disk space by file name.
    /// </summary>
    /// <param name="fileName">The file name to deallocate.</param>
    /// <returns>Amount of disk space freed in gigabytes.</returns>
    public long DeallocateDiskSpace(string fileName)
    {
        lock (_lockObject)
        {
            var blocksToRemove = _allocatedBlocks.Where(b => b.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)).ToList();
            long freedSpace = blocksToRemove.Sum(b => b.SizeGB);

            foreach (var block in blocksToRemove)
            {
                _allocatedBlocks.Remove(block);
            }

            return freedSpace;
        }
    }

    /// <summary>
    /// Checks if a file exists on the disk.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file exists; otherwise false.</returns>
    public bool FileExists(string fileName)
    {
        lock (_lockObject)
        {
            return _allocatedBlocks.Any(b => b.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Gets all allocated disk blocks.
    /// </summary>
    public IReadOnlyList<DiskBlock> GetAllocatedBlocks()
    {
        lock (_lockObject)
        {
            return _allocatedBlocks.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Forcefully clears all disk allocations. Only available in Kernel Mode.
    /// </summary>
    public void ClearAll()
    {
        lock (_lockObject)
        {
            _allocatedBlocks.Clear();
        }
    }

    /// <summary>
    /// Gets detailed disk status information.
    /// </summary>
    public string GetDiskStatus()
    {
        lock (_lockObject)
        {
            return $"HDD: {UsedSizeGB} GB / {_totalSizeGB} GB ({UsagePercentage:F1}% used)\n" +
                   $"Free: {FreeSizeGB} GB\n" +
                   $"Allocated Blocks: {_allocatedBlocks.Count}";
        }
    }
}

/// <summary>
/// Represents a block of allocated disk space.
/// </summary>
public record DiskBlock(int BlockId, string FileName, long SizeGB)
{
    /// <summary>
    /// Gets the timestamp when this block was allocated.
    /// </summary>
    public DateTime AllocatedAt { get; } = DateTime.Now;
}
