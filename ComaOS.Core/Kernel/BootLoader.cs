namespace ComaOS.Core.Kernel;

/// <summary>
/// Represents the boot loader responsible for the startup sequence of ComaOS.
/// Simulates a loading screen and initialization process.
/// </summary>
public class BootLoader
{
    private readonly string _osName = "ComaOS";
    private readonly string _osVersion = "1.0.0";

    /// <summary>
    /// Event raised during boot progress updates.
    /// </summary>
    public event EventHandler<BootProgressEventArgs>? BootProgress;

    /// <summary>
    /// Executes the boot sequence with simulated loading delay.
    /// </summary>
    /// <param name="ramSizeMB">RAM size in megabytes to initialize.</param>
    /// <param name="hddSizeGB">Hard drive size in gigabytes to initialize.</param>
    /// <param name="coreCount">Number of CPU cores to initialize.</param>
    /// <returns>A task representing the asynchronous boot operation.</returns>
    public async Task<BootResult> BootAsync(long ramSizeMB, long hddSizeGB, int coreCount)
    {
        try
        {
            ReportProgress(0, "Initializing Boot Sequence...");
            await Task.Delay(500);

            ReportProgress(10, $"Loading {_osName} v{_osVersion}");
            await Task.Delay(300);

            ReportProgress(25, "Checking Hardware...");
            await Task.Delay(400);

            ReportProgress(40, $"Initializing CPU ({coreCount} core(s))...");
            await Task.Delay(300);

            ReportProgress(55, $"Initializing RAM ({ramSizeMB} MB)...");
            await Task.Delay(400);

            ReportProgress(70, $"Initializing Hard Drive ({hddSizeGB} GB)...");
            await Task.Delay(300);

            ReportProgress(85, "Loading Kernel...");
            await Task.Delay(500);

            ReportProgress(95, "Starting System Services...");
            await Task.Delay(300);

            ReportProgress(100, $"{_osName} Ready!");
            await Task.Delay(200);

            return new BootResult(true, $"{_osName} v{_osVersion} booted successfully!");
        }
        catch (Exception ex)
        {
            return new BootResult(false, $"Boot failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a synchronous boot sequence (blocking).
    /// </summary>
    public BootResult Boot(long ramSizeMB, long hddSizeGB, int coreCount)
    {
        return BootAsync(ramSizeMB, hddSizeGB, coreCount).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Reports boot progress to subscribers.
    /// </summary>
    private void ReportProgress(int percentage, string message)
    {
        BootProgress?.Invoke(this, new BootProgressEventArgs(percentage, message));
    }

    /// <summary>
    /// Displays the boot splash screen information.
    /// </summary>
    public string GetSplashScreen()
    {
        return $@"
?????????????????????????????????????????????????????????
?                                                       ?
?           ??????? ??????? ????   ???? ??????         ?
?          ?????????????????????? ?????????????        ?
?          ???     ???   ??????????????????????        ?
?          ???     ???   ??????????????????????        ?
?          ???????????????????? ??? ??????  ???        ?
?           ??????? ??????? ???     ??????  ???        ?
?                                                       ?
?                Operating System Simulator             ?
?                    Version {_osVersion}                      ?
?                                                       ?
?????????????????????????????????????????????????????????
";
    }
}

/// <summary>
/// Represents the result of a boot operation.
/// </summary>
public record BootResult(bool Success, string Message);

/// <summary>
/// Event arguments for boot progress updates.
/// </summary>
public class BootProgressEventArgs : EventArgs
{
    /// <summary>
    /// Gets the boot progress percentage (0-100).
    /// </summary>
    public int Percentage { get; }

    /// <summary>
    /// Gets the current boot status message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of BootProgressEventArgs.
    /// </summary>
    public BootProgressEventArgs(int percentage, string message)
    {
        Percentage = percentage;
        Message = message;
    }
}
