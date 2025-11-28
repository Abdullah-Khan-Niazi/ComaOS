using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ComaOS.UI.MVVM.ViewModels;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Settings application for configuring ComaOS.
/// </summary>
public partial class SettingsView : UserControl
{
    private MainViewModel? _mainViewModel;

    public SettingsView()
    {
        InitializeComponent();
        VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        DarkModeToggle.Checked += DarkModeToggle_Changed;
        DarkModeToggle.Unchecked += DarkModeToggle_Changed;
    }

    private void DarkModeToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow == null) return;

        bool isDarkMode = DarkModeToggle.IsChecked == true;

        // Update the main window and all child elements
        if (isDarkMode)
        {
            // Dark mode colors
            Application.Current.MainWindow.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(26, 26, 42)); // #1a1a2a
        }
        else
        {
            // Light mode colors
            Application.Current.MainWindow.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(240, 240, 245)); // Light gray
        }

        // Store preference in MainViewModel if available
        if (_mainViewModel != null)
        {
            _mainViewModel.IsDarkMode = isDarkMode;
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow?.DataContext is MainViewModel vm)
        {
            _mainViewModel = vm;
            UpdateSystemInfo();
        }
    }

    private void UpdateSystemInfo()
    {
        if (_mainViewModel == null) return;

        ModeText.Text = _mainViewModel.CurrentMode.ToString();
        RamDisplay.Text = $"{_mainViewModel.RamTotal} MB";
        HddDisplay.Text = $"{_mainViewModel.HddSizeGB} GB";
        CpuDisplay.Text = $"{_mainViewModel.CoreCount} Cores";
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (VolumeText != null)
        {
            VolumeText.Text = $"{(int)e.NewValue}%";
        }
    }

    private void ClearMemory_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "This will terminate ALL running processes and clear memory.\n\nAre you sure?",
            "Clear Memory",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            // Close all windows except system processes
            if (_mainViewModel != null)
            {
                var windows = _mainViewModel.OpenWindows.ToList();
                foreach (var window in windows)
                {
                    _mainViewModel.CloseWindowCommand?.Execute(window);
                }
            }

            MessageBox.Show("Memory cleared successfully!", "Settings", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ClearFiles_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "This will delete ALL files in the virtual file system.\n\nAre you sure?",
            "Clear Files",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            MessageBox.Show("Files cleared successfully!\n\n(Note: This is simulated - files will reappear on next File Manager launch)", 
                "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Restart_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "This will restart the ComaOS system.\n\nAll unsaved work will be lost. Continue?",
            "Restart System",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            // Shutdown and request restart
            _mainViewModel?.ShutdownCommand?.Execute(null);
            
            MessageBox.Show("System shutdown complete.\n\nClick 'Boot System' to restart.", 
                "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
