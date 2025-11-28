using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Compression Tool for creating and extracting archives.
/// </summary>
public partial class CompressionToolView : UserControl
{
    private readonly List<VirtualFile> _filesToCompress;
    private readonly DispatcherTimer _progressTimer;
    private readonly Random _random;
    private int _currentFileIndex;
    private bool _isCompressing;
    private string? _selectedArchive;

    private readonly VirtualFile[] _sampleFiles = new[]
    {
        new VirtualFile("📄 document.pdf", 2450),
        new VirtualFile("🖼 photo.jpg", 3200),
        new VirtualFile("📊 spreadsheet.xlsx", 890),
        new VirtualFile("📝 notes.txt", 45),
        new VirtualFile("🎵 music.mp3", 4500),
        new VirtualFile("📁 project/", 0),
        new VirtualFile("  📄 readme.md", 12),
        new VirtualFile("  📄 main.py", 156),
        new VirtualFile("  📄 config.json", 8),
    };

    private readonly string[] _archiveFiles = new[]
    {
        "backup_2025.zip",
        "photos.tar.gz",
        "documents.7z",
        "project_archive.zip",
        "music_collection.rar",
    };

    public CompressionToolView()
    {
        InitializeComponent();
        
        _filesToCompress = new List<VirtualFile>();
        _random = new Random();
        
        _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _progressTimer.Tick += ProgressTimer_Tick;
        
        UpdateFilesList();
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        var isCompress = CompressTab.IsChecked == true;
        CompressPanel.Visibility = isCompress ? Visibility.Visible : Visibility.Collapsed;
        ExtractPanel.Visibility = isCompress ? Visibility.Collapsed : Visibility.Visible;
        ActionButton.Content = isCompress ? "📦 Compress" : "📂 Extract";
        
        ProgressPanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Collapsed;
    }

    private void UpdateFilesList()
    {
        FilesToCompress.Items.Clear();
        
        if (_filesToCompress.Count == 0)
        {
            NoFilesText.Visibility = Visibility.Visible;
        }
        else
        {
            NoFilesText.Visibility = Visibility.Collapsed;
            foreach (var file in _filesToCompress)
            {
                FilesToCompress.Items.Add($"{file.Name} ({FormatSize(file.SizeKB)})");
            }
        }
    }

    private static string FormatSize(long kb)
    {
        if (kb < 1024) return $"{kb} KB";
        return $"{kb / 1024.0:F1} MB";
    }

    private void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        // Simulate file picker by adding random sample files
        var filesToAdd = _random.Next(2, 5);
        for (int i = 0; i < filesToAdd; i++)
        {
            var file = _sampleFiles[_random.Next(_sampleFiles.Length)];
            if (!_filesToCompress.Exists(f => f.Name == file.Name))
            {
                _filesToCompress.Add(new VirtualFile(file.Name, file.SizeKB + _random.Next(-100, 500)));
            }
        }
        
        UpdateFilesList();
    }

    private void ClearFiles_Click(object sender, RoutedEventArgs e)
    {
        _filesToCompress.Clear();
        UpdateFilesList();
        ProgressPanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Collapsed;
    }

    private void SelectArchive_Click(object sender, RoutedEventArgs e)
    {
        // Simulate archive selection
        _selectedArchive = _archiveFiles[_random.Next(_archiveFiles.Length)];
        
        NoArchivePanel.Visibility = Visibility.Collapsed;
        ArchiveInfoPanel.Visibility = Visibility.Visible;
        
        var size = _random.Next(5, 50);
        var files = _random.Next(10, 100);
        
        ArchiveNameText.Text = $"📦 {_selectedArchive}";
        ArchiveSizeText.Text = $"Size: {size} MB";
        ArchiveContentsText.Text = $"Contains {files} files";
        
        ProgressPanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Collapsed;
    }

    private void Action_Click(object sender, RoutedEventArgs e)
    {
        if (CompressTab.IsChecked == true)
        {
            StartCompression();
        }
        else
        {
            StartExtraction();
        }
    }

    private void StartCompression()
    {
        if (_filesToCompress.Count == 0)
        {
            MessageBox.Show("Please add files to compress.", "Compression Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var archiveName = ArchiveNameInput.Text.Trim();
        if (string.IsNullOrEmpty(archiveName))
        {
            MessageBox.Show("Please enter an archive name.", "Compression Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        _isCompressing = true;
        _currentFileIndex = 0;
        
        ProgressPanel.Visibility = Visibility.Visible;
        ResultsPanel.Visibility = Visibility.Collapsed;
        ProgressTitle.Text = "Compressing...";
        ProgressBar.Value = 0;
        ProgressPercent.Text = "0%";
        
        ActionButton.IsEnabled = false;
        
        _progressTimer.Start();
    }

    private void StartExtraction()
    {
        if (string.IsNullOrEmpty(_selectedArchive))
        {
            MessageBox.Show("Please select an archive to extract.", "Compression Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        _isCompressing = false;
        _currentFileIndex = 0;
        
        ProgressPanel.Visibility = Visibility.Visible;
        ResultsPanel.Visibility = Visibility.Collapsed;
        ProgressTitle.Text = "Extracting...";
        ProgressBar.Value = 0;
        ProgressPercent.Text = "0%";
        
        ActionButton.IsEnabled = false;
        
        _progressTimer.Start();
    }

    private void ProgressTimer_Tick(object? sender, EventArgs e)
    {
        var increment = _random.Next(3, 8);
        var newValue = Math.Min(ProgressBar.Value + increment, 100);
        ProgressBar.Value = newValue;
        ProgressPercent.Text = $"{(int)newValue}%";
        
        if (_isCompressing)
        {
            if (_filesToCompress.Count > 0)
            {
                var fileIndex = (int)(newValue / 100 * _filesToCompress.Count);
                fileIndex = Math.Min(fileIndex, _filesToCompress.Count - 1);
                ProgressFile.Text = $"Compressing: {_filesToCompress[fileIndex].Name}";
            }
        }
        else
        {
            var fakeFiles = new[] { "extracting data...", "decompressing...", "writing files...", "verifying integrity..." };
            ProgressFile.Text = fakeFiles[_random.Next(fakeFiles.Length)];
        }
        
        if (newValue >= 100)
        {
            CompleteOperation();
        }
    }

    private void CompleteOperation()
    {
        _progressTimer.Stop();
        
        ProgressPanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Visible;
        ActionButton.IsEnabled = true;
        
        if (_isCompressing)
        {
            var format = (FormatCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? ".zip";
            var archiveName = ArchiveNameInput.Text + format;
            
            // Calculate "compressed" size (simulate compression ratio)
            long totalSize = 0;
            foreach (var file in _filesToCompress)
            {
                totalSize += file.SizeKB;
            }
            
            var compressionLevel = LevelCombo.SelectedIndex;
            var ratio = compressionLevel switch
            {
                0 => 0.8, // Fast
                1 => 0.6, // Normal
                2 => 0.45, // Maximum
                3 => 0.35, // Ultra
                _ => 0.6
            };
            
            var compressedSize = (long)(totalSize * ratio);
            var savedPercent = (int)((1 - ratio) * 100);
            
            ResultIcon.Text = "✅";
            ResultTitle.Text = "Compression Complete!";
            ResultTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
            ResultDetails.Text = $"Created: {archiveName}\n" +
                                 $"Original: {FormatSize(totalSize)} → Compressed: {FormatSize(compressedSize)}\n" +
                                 $"Space saved: {savedPercent}%";
        }
        else
        {
            var extractPath = ExtractPathInput.Text;
            var fileCount = _random.Next(10, 100);
            
            ResultIcon.Text = "📂";
            ResultTitle.Text = "Extraction Complete!";
            ResultTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
            ResultDetails.Text = $"Extracted {fileCount} files\nLocation: {extractPath}";
        }
    }

    private class VirtualFile
    {
        public string Name { get; }
        public long SizeKB { get; }

        public VirtualFile(string name, long sizeKb)
        {
            Name = name;
            SizeKB = sizeKb;
        }
    }
}
