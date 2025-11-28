using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Antivirus app with simulated scanning functionality.
/// </summary>
public partial class AntivirusView : UserControl
{
    private readonly DispatcherTimer _scanTimer;
    private readonly Random _random;
    private int _filesScanned;
    private int _totalFiles;
    private int _threatsFound;
    private DateTime _scanStartTime;
    private string _scanType = "";
    private bool _isScanning;
    private readonly List<ThreatItem> _detectedThreats;

    private readonly string[] _scanPaths = new[]
    {
        "/system/kernel/...",
        "/system/drivers/...",
        "/home/user/documents/...",
        "/home/user/downloads/...",
        "/apps/installed/...",
        "/temp/cache/...",
        "/system/config/...",
        "/var/log/...",
        "/boot/...",
        "/usr/bin/...",
    };

    private readonly string[] _threatNames = new[]
    {
        "Trojan.GenericKD.46845123",
        "PUP.Optional.BundleInstaller",
        "Adware.Eorezo.BG",
        "Worm.Win32.AutoRun",
        "Spyware.Keylogger.XK",
        "Ransomware.Crypto.Gen",
        "Backdoor.Agent.ZX",
    };

    public AntivirusView()
    {
        InitializeComponent();
        
        _random = new Random();
        _detectedThreats = new List<ThreatItem>();
        
        _scanTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _scanTimer.Tick += ScanTimer_Tick;
        
        UpdateLastScanTime();
    }

    private void UpdateLastScanTime()
    {
        // Simulate last scan was some time ago
        LastScanText.Text = "Last scan: Never";
    }

    private void StartScan(string scanType, int fileCount)
    {
        _scanType = scanType;
        _totalFiles = fileCount;
        _filesScanned = 0;
        _threatsFound = 0;
        _scanStartTime = DateTime.Now;
        _isScanning = true;
        _detectedThreats.Clear();
        
        IdlePanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Collapsed;
        ScanningPanel.Visibility = Visibility.Visible;
        
        ScanTypeText.Text = $"{scanType} in Progress...";
        ScanProgress.Value = 0;
        ScanPercentText.Text = "0%";
        FilesScannedText.Text = "0";
        ThreatsFoundText.Text = "0";
        ThreatsFoundText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
        TimeElapsedText.Text = "0:00";
        
        // Disable scan buttons during scan
        QuickScanButton.IsEnabled = false;
        FullScanButton.IsEnabled = false;
        CustomScanButton.IsEnabled = false;
        
        _scanTimer.Start();
    }

    private void ScanTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isScanning) return;
        
        // Increment files scanned
        var increment = _random.Next(5, 25);
        _filesScanned = Math.Min(_filesScanned + increment, _totalFiles);
        
        // Update progress
        var progress = (double)_filesScanned / _totalFiles * 100;
        ScanProgress.Value = progress;
        ScanPercentText.Text = $"{(int)progress}%";
        FilesScannedText.Text = _filesScanned.ToString("N0");
        
        // Update current file
        var pathIndex = _random.Next(_scanPaths.Length);
        CurrentFileText.Text = $"Scanning: {_scanPaths[pathIndex]}";
        
        // Update time elapsed
        var elapsed = DateTime.Now - _scanStartTime;
        TimeElapsedText.Text = $"{(int)elapsed.TotalMinutes}:{elapsed.Seconds:D2}";
        
        // Random chance to find a threat (only for full scan or custom scan)
        if (_scanType != "Quick Scan" && _random.Next(100) < 2 && _threatsFound < 3)
        {
            _threatsFound++;
            ThreatsFoundText.Text = _threatsFound.ToString();
            ThreatsFoundText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!);
            
            _detectedThreats.Add(new ThreatItem(
                _threatNames[_random.Next(_threatNames.Length)],
                _scanPaths[pathIndex].Replace("...", $"infected_file_{_threatsFound}.exe"),
                _random.Next(10, 100) > 50 ? "High" : "Medium"
            ));
        }
        
        // Check if scan is complete
        if (_filesScanned >= _totalFiles)
        {
            CompleteScan();
        }
    }

    private void CompleteScan()
    {
        _scanTimer.Stop();
        _isScanning = false;
        
        ScanningPanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Visible;
        
        // Re-enable scan buttons
        QuickScanButton.IsEnabled = true;
        FullScanButton.IsEnabled = true;
        CustomScanButton.IsEnabled = true;
        
        // Update status
        LastScanText.Text = $"Last scan: Just now";
        
        // Build results
        ThreatsList.Children.Clear();
        
        if (_threatsFound == 0)
        {
            ResultsTitle.Text = "✅ Scan Complete - No Threats Found";
            ResultsTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
            CleanThreatsButton.Visibility = Visibility.Collapsed;
            
            StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a3d1a")!);
            StatusTitle.Text = "System Protected";
            StatusTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
            StatusSubtitle.Text = "No threats detected";
            
            ThreatsList.Children.Add(new TextBlock
            {
                Text = $"✓ Scanned {_filesScanned:N0} files\n✓ No malware detected\n✓ No suspicious activity found",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888")!),
                FontSize = 12,
                LineHeight = 22
            });
        }
        else
        {
            ResultsTitle.Text = $"⚠️ Scan Complete - {_threatsFound} Threat(s) Found";
            ResultsTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!);
            CleanThreatsButton.Visibility = Visibility.Visible;
            
            StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d1a1a")!);
            StatusTitle.Text = "Threats Detected";
            StatusTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!);
            StatusSubtitle.Text = $"{_threatsFound} threat(s) require attention";
            
            foreach (var threat in _detectedThreats)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a1a2a")!),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                
                var stack = new StackPanel();
                
                var header = new Grid();
                header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                
                var nameText = new TextBlock
                {
                    Text = $"🦠 {threat.Name}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                
                var severityText = new TextBlock
                {
                    Text = threat.Severity,
                    Foreground = threat.Severity == "High" 
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!)
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fbbf24")!),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetColumn(severityText, 1);
                
                header.Children.Add(nameText);
                header.Children.Add(severityText);
                
                var pathText = new TextBlock
                {
                    Text = threat.Path,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")!),
                    FontSize = 10,
                    Margin = new Thickness(0, 3, 0, 0)
                };
                
                stack.Children.Add(header);
                stack.Children.Add(pathText);
                border.Child = stack;
                
                ThreatsList.Children.Add(border);
            }
        }
    }

    private void QuickScan_Click(object sender, RoutedEventArgs e)
    {
        StartScan("Quick Scan", 5000);
    }

    private void FullScan_Click(object sender, RoutedEventArgs e)
    {
        StartScan("Full System Scan", 25000);
    }

    private void CustomScan_Click(object sender, RoutedEventArgs e)
    {
        StartScan("Custom Scan", 8000);
    }

    private void CancelScan_Click(object sender, RoutedEventArgs e)
    {
        _scanTimer.Stop();
        _isScanning = false;
        
        ScanningPanel.Visibility = Visibility.Collapsed;
        IdlePanel.Visibility = Visibility.Visible;
        
        QuickScanButton.IsEnabled = true;
        FullScanButton.IsEnabled = true;
        CustomScanButton.IsEnabled = true;
    }

    private void CleanThreats_Click(object sender, RoutedEventArgs e)
    {
        _detectedThreats.Clear();
        _threatsFound = 0;
        
        // Update UI to show cleaned
        ResultsTitle.Text = "✅ All Threats Cleaned";
        ResultsTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
        CleanThreatsButton.Visibility = Visibility.Collapsed;
        
        StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a3d1a")!);
        StatusTitle.Text = "System Protected";
        StatusTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
        StatusSubtitle.Text = "All threats have been removed";
        
        ThreatsList.Children.Clear();
        ThreatsList.Children.Add(new TextBlock
        {
            Text = "🧹 All detected threats have been quarantined and removed.\n✓ Your system is now clean.",
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888")!),
            FontSize = 12,
            LineHeight = 22
        });
    }

    private void NewScan_Click(object sender, RoutedEventArgs e)
    {
        ResultsPanel.Visibility = Visibility.Collapsed;
        IdlePanel.Visibility = Visibility.Visible;
    }

    private class ThreatItem
    {
        public string Name { get; }
        public string Path { get; }
        public string Severity { get; }

        public ThreatItem(string name, string path, string severity)
        {
            Name = name;
            Path = path;
            Severity = severity;
        }
    }
}
