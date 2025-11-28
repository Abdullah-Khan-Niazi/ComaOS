using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Simulated music player application.
/// </summary>
public partial class MusicPlayerView : UserControl
{
    private readonly ObservableCollection<TrackInfo> _playlist = new();
    private readonly DispatcherTimer _playbackTimer;
    private readonly DispatcherTimer _visualizerTimer;
    private readonly Random _random = new();
    private bool _isPlaying = false;
    private bool _isShuffle = false;
    private bool _isRepeat = false;
    private int _currentTrackIndex = -1;
    private double _currentPosition = 0;
    private double _trackDuration = 0;

    public MusicPlayerView()
    {
        InitializeComponent();
        
        _playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _playbackTimer.Tick += PlaybackTimer_Tick;
        
        _visualizerTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _visualizerTimer.Tick += VisualizerTimer_Tick;
        
        InitializePlaylist();
        PlaylistBox.ItemsSource = _playlist;
    }

    private void InitializePlaylist()
    {
        _playlist.Add(new TrackInfo("Vibe Code Anthem", "Claude Opus", 234));
        _playlist.Add(new TrackInfo("Digital Dreams", "AI Orchestra", 187));
        _playlist.Add(new TrackInfo("Binary Sunset", "The Compilers", 298));
        _playlist.Add(new TrackInfo("RAM & Roll", "Memory Band", 215));
        _playlist.Add(new TrackInfo("Kernel Panic", "System Error", 162));
        _playlist.Add(new TrackInfo("Infinite Loop", "While True", 245));
        _playlist.Add(new TrackInfo("Stack Overflow", "Exception Handlers", 178));
        _playlist.Add(new TrackInfo("Hello World", "First Program", 156));
        _playlist.Add(new TrackInfo("Async Await", "Promise Keepers", 203));
        _playlist.Add(new TrackInfo("Null Reference", "The Exceptions", 189));
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Auto-select first track
        if (_playlist.Count > 0 && _currentTrackIndex < 0)
        {
            SelectTrack(0);
        }
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        _playbackTimer.Stop();
        _visualizerTimer.Stop();
    }

    private void SelectTrack(int index)
    {
        if (index < 0 || index >= _playlist.Count) return;
        
        _currentTrackIndex = index;
        var track = _playlist[index];
        
        TrackTitle.Text = track.Title;
        TrackArtist.Text = track.Artist;
        _trackDuration = track.DurationSeconds;
        _currentPosition = 0;
        
        TotalTime.Text = FormatTime(_trackDuration);
        CurrentTime.Text = "0:00";
        ProgressSlider.Value = 0;
        ProgressSlider.Maximum = _trackDuration;
        
        PlaylistBox.SelectedIndex = index;
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_currentTrackIndex < 0 && _playlist.Count > 0)
        {
            SelectTrack(0);
        }
        
        _isPlaying = !_isPlaying;
        
        if (_isPlaying)
        {
            PlayButton.Content = "⏸";
            _playbackTimer.Start();
            _visualizerTimer.Start();
            Visualizer.Visibility = Visibility.Visible;
        }
        else
        {
            PlayButton.Content = "▶";
            _playbackTimer.Stop();
            _visualizerTimer.Stop();
            Visualizer.Visibility = Visibility.Collapsed;
        }
    }

    private void Previous_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPosition > 3)
        {
            // Restart current track if more than 3 seconds in
            _currentPosition = 0;
            ProgressSlider.Value = 0;
            CurrentTime.Text = "0:00";
        }
        else
        {
            // Go to previous track
            int newIndex = _currentTrackIndex - 1;
            if (newIndex < 0) newIndex = _playlist.Count - 1;
            SelectTrack(newIndex);
        }
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        PlayNextTrack();
    }

    private void PlayNextTrack()
    {
        int newIndex;
        if (_isShuffle)
        {
            newIndex = _random.Next(_playlist.Count);
        }
        else
        {
            newIndex = _currentTrackIndex + 1;
            if (newIndex >= _playlist.Count)
            {
                newIndex = _isRepeat ? 0 : _playlist.Count - 1;
                if (!_isRepeat)
                {
                    _isPlaying = false;
                    PlayButton.Content = "▶";
                    _playbackTimer.Stop();
                    _visualizerTimer.Stop();
                    Visualizer.Visibility = Visibility.Collapsed;
                }
            }
        }
        SelectTrack(newIndex);
    }

    private void Shuffle_Click(object sender, RoutedEventArgs e)
    {
        _isShuffle = !_isShuffle;
        if (sender is Button btn)
        {
            btn.Opacity = _isShuffle ? 1.0 : 0.5;
        }
    }

    private void Repeat_Click(object sender, RoutedEventArgs e)
    {
        _isRepeat = !_isRepeat;
        if (sender is Button btn)
        {
            btn.Opacity = _isRepeat ? 1.0 : 0.5;
        }
    }

    private void Playlist_Click(object sender, RoutedEventArgs e)
    {
        PlaylistPanel.Visibility = Visibility.Visible;
    }

    private void ClosePlaylist_Click(object sender, RoutedEventArgs e)
    {
        PlaylistPanel.Visibility = Visibility.Collapsed;
    }

    private void PlaylistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlaylistBox.SelectedIndex >= 0 && PlaylistBox.SelectedIndex != _currentTrackIndex)
        {
            SelectTrack(PlaylistBox.SelectedIndex);
            if (_isPlaying)
            {
                _currentPosition = 0;
            }
        }
    }

    private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Only update if user is dragging (not from timer)
        if (Math.Abs(e.NewValue - _currentPosition) > 1)
        {
            _currentPosition = e.NewValue;
            CurrentTime.Text = FormatTime(_currentPosition);
        }
    }

    private void PlaybackTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isPlaying) return;
        
        _currentPosition += 0.1;
        
        if (_currentPosition >= _trackDuration)
        {
            if (_isRepeat && !_isShuffle)
            {
                _currentPosition = 0;
            }
            else
            {
                PlayNextTrack();
                return;
            }
        }
        
        ProgressSlider.Value = _currentPosition;
        CurrentTime.Text = FormatTime(_currentPosition);
    }

    private void VisualizerTimer_Tick(object? sender, EventArgs e)
    {
        // Animate visualizer bars
        Bar1.Height = 15 + _random.Next(30);
        Bar2.Height = 15 + _random.Next(35);
        Bar3.Height = 15 + _random.Next(25);
        Bar4.Height = 15 + _random.Next(40);
        Bar5.Height = 15 + _random.Next(30);
        Bar6.Height = 15 + _random.Next(45);
        Bar7.Height = 15 + _random.Next(28);
        Bar8.Height = 15 + _random.Next(38);
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return ts.Minutes + ":" + ts.Seconds.ToString("D2");
    }
}

public class TrackInfo
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public int DurationSeconds { get; set; }
    public string Duration => TimeSpan.FromSeconds(DurationSeconds).ToString(@"m\:ss");

    public TrackInfo(string title, string artist, int duration)
    {
        Title = title;
        Artist = artist;
        DurationSeconds = duration;
    }
}
