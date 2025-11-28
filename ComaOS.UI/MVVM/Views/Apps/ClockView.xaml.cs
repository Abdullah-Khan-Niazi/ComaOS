using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Clock app with world clocks, stopwatch, and timer functionality.
/// </summary>
public partial class ClockView : UserControl
{
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _stopwatchTimer;
    private readonly DispatcherTimer _timerTimer;
    
    private TimeSpan _stopwatchTime;
    private bool _stopwatchRunning;
    private int _lapCount;
    
    private TimeSpan _timerTime;
    private bool _timerRunning;
    
    private readonly List<WorldClock> _worldClocks;

    public ClockView()
    {
        InitializeComponent();
        
        _worldClocks = new List<WorldClock>
        {
            new("🗽 New York", -5),
            new("🗼 London", 0),
            new("🗼 Paris", 1),
            new("🏯 Tokyo", 9),
            new("🌉 Sydney", 11),
        };
        
        // Clock timer - updates every second
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += ClockTimer_Tick;
        _clockTimer.Start();
        
        // Stopwatch timer - updates every 10ms
        _stopwatchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
        _stopwatchTimer.Tick += StopwatchTimer_Tick;
        
        // Timer timer - updates every second
        _timerTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timerTimer.Tick += TimerTimer_Tick;
        
        UpdateClockDisplay();
        UpdateWorldClocks();
    }

    private void ClockTimer_Tick(object? sender, EventArgs e)
    {
        UpdateClockDisplay();
        UpdateWorldClocks();
    }

    private void UpdateClockDisplay()
    {
        var now = DateTime.Now;
        TimeDisplay.Text = now.ToString("HH:mm:ss");
        DateDisplay.Text = now.ToString("dddd, MMMM d, yyyy");
        
        var offset = TimeZoneInfo.Local.GetUtcOffset(now);
        var sign = offset >= TimeSpan.Zero ? "+" : "";
        TimezoneDisplay.Text = $"Local Time (UTC{sign}{offset.Hours:D2}:{offset.Minutes:D2})";
    }

    private void UpdateWorldClocks()
    {
        WorldClocks.Items.Clear();
        var utcNow = DateTime.UtcNow;
        
        foreach (var wc in _worldClocks)
        {
            var localTime = utcNow.AddHours(wc.UtcOffset);
            var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var nameText = new TextBlock
            {
                Text = wc.Name,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ccc")!),
                FontSize = 12
            };
            
            var timeText = new TextBlock
            {
                Text = localTime.ToString("HH:mm"),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60a5fa")!),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas")
            };
            Grid.SetColumn(timeText, 1);
            
            grid.Children.Add(nameText);
            grid.Children.Add(timeText);
            
            WorldClocks.Items.Add(grid);
        }
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        ClockPanel.Visibility = ClockTab.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        StopwatchPanel.Visibility = StopwatchTab.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        TimerPanel.Visibility = TimerTab.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    #region Stopwatch

    private void StopwatchTimer_Tick(object? sender, EventArgs e)
    {
        _stopwatchTime = _stopwatchTime.Add(TimeSpan.FromMilliseconds(10));
        UpdateStopwatchDisplay();
    }

    private void UpdateStopwatchDisplay()
    {
        StopwatchDisplay.Text = $"{_stopwatchTime.Hours:D2}:{_stopwatchTime.Minutes:D2}:{_stopwatchTime.Seconds:D2}.{_stopwatchTime.Milliseconds / 10:D2}";
    }

    private void StopwatchStart_Click(object sender, RoutedEventArgs e)
    {
        if (_stopwatchRunning)
        {
            // Stop
            _stopwatchTimer.Stop();
            _stopwatchRunning = false;
            StopwatchStartButton.Content = "▶ Start";
            StopwatchStartButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
            StopwatchLapButton.IsEnabled = false;
        }
        else
        {
            // Start
            _stopwatchTimer.Start();
            _stopwatchRunning = true;
            StopwatchStartButton.Content = "⏸ Stop";
            StopwatchStartButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!);
            StopwatchLapButton.IsEnabled = true;
        }
    }

    private void StopwatchLap_Click(object sender, RoutedEventArgs e)
    {
        if (!_stopwatchRunning) return;
        
        _lapCount++;
        var lapText = new TextBlock
        {
            Text = $"Lap {_lapCount}: {_stopwatchTime.Hours:D2}:{_stopwatchTime.Minutes:D2}:{_stopwatchTime.Seconds:D2}.{_stopwatchTime.Milliseconds / 10:D2}",
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ccc")!),
            FontSize = 12,
            FontFamily = new FontFamily("Consolas"),
            Margin = new Thickness(0, 2, 0, 2)
        };
        
        LapsList.Children.Insert(0, lapText);
    }

    private void StopwatchReset_Click(object sender, RoutedEventArgs e)
    {
        _stopwatchTimer.Stop();
        _stopwatchRunning = false;
        _stopwatchTime = TimeSpan.Zero;
        _lapCount = 0;
        
        UpdateStopwatchDisplay();
        LapsList.Children.Clear();
        
        StopwatchStartButton.Content = "▶ Start";
        StopwatchStartButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e")!);
        StopwatchLapButton.IsEnabled = false;
    }

    #endregion

    #region Timer

    private void TimerTimer_Tick(object? sender, EventArgs e)
    {
        _timerTime = _timerTime.Subtract(TimeSpan.FromSeconds(1));
        UpdateTimerDisplay();
        
        if (_timerTime <= TimeSpan.Zero)
        {
            _timerTimer.Stop();
            _timerRunning = false;
            _timerTime = TimeSpan.Zero;
            UpdateTimerDisplay();
            
            TimerStartButton.Content = "▶ Start";
            TimerStartButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60a5fa")!);
            TimerInputPanel.Visibility = Visibility.Visible;
            
            // Alert
            MessageBox.Show("⏰ Timer finished!", "Clock", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void UpdateTimerDisplay()
    {
        TimerDisplay.Text = $"{_timerTime.Hours:D2}:{_timerTime.Minutes:D2}:{_timerTime.Seconds:D2}";
    }

    private void TimerStart_Click(object sender, RoutedEventArgs e)
    {
        if (_timerRunning)
        {
            // Pause
            _timerTimer.Stop();
            _timerRunning = false;
            TimerStartButton.Content = "▶ Resume";
            TimerStartButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60a5fa")!);
        }
        else
        {
            // Start/Resume
            if (_timerTime <= TimeSpan.Zero)
            {
                // Parse input
                if (!int.TryParse(TimerHours.Text, out var hours)) hours = 0;
                if (!int.TryParse(TimerMinutes.Text, out var minutes)) minutes = 0;
                if (!int.TryParse(TimerSeconds.Text, out var seconds)) seconds = 0;
                
                _timerTime = new TimeSpan(hours, minutes, seconds);
                
                if (_timerTime <= TimeSpan.Zero)
                {
                    MessageBox.Show("Please set a valid timer duration.", "Clock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            
            _timerTimer.Start();
            _timerRunning = true;
            TimerStartButton.Content = "⏸ Pause";
            TimerStartButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!);
            TimerInputPanel.Visibility = Visibility.Collapsed;
            UpdateTimerDisplay();
        }
    }

    private void TimerReset_Click(object sender, RoutedEventArgs e)
    {
        _timerTimer.Stop();
        _timerRunning = false;
        _timerTime = TimeSpan.Zero;
        
        TimerHours.Text = "00";
        TimerMinutes.Text = "05";
        TimerSeconds.Text = "00";
        
        UpdateTimerDisplay();
        TimerDisplay.Text = "00:05:00";
        
        TimerStartButton.Content = "▶ Start";
        TimerStartButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60a5fa")!);
        TimerInputPanel.Visibility = Visibility.Visible;
    }

    #endregion

    private class WorldClock
    {
        public string Name { get; }
        public int UtcOffset { get; }

        public WorldClock(string name, int utcOffset)
        {
            Name = name;
            UtcOffset = utcOffset;
        }
    }
}
