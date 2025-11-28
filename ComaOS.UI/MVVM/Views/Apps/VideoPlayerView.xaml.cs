using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Video Player with simulated video playback.
/// </summary>
public partial class VideoPlayerView : UserControl
{
    private readonly List<VideoItem> _videos;
    private VideoItem? _currentVideo;
    private bool _isPlaying;
    private double _currentPosition;
    private readonly DispatcherTimer _playbackTimer;
    private int _volume = 75;
    private bool _isUserSeeking;

    public VideoPlayerView()
    {
        InitializeComponent();
        
        _videos = new List<VideoItem>
        {
            new("🎬 Big Buck Bunny", "Animation", 596),
            new("🎥 Sintel", "Fantasy Short", 888),
            new("📽 Tears of Steel", "Sci-Fi", 734),
            new("🎞 Elephant's Dream", "Animation", 654),
            new("🎦 Cosmos Laundromat", "Fantasy", 720),
            new("📹 Spring", "Animation", 468),
            new("🎬 Agent 327", "Action", 232),
            new("🎥 Glass Half", "Comedy", 180),
        };
        
        VideoList.ItemsSource = _videos;
        
        _playbackTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _playbackTimer.Tick += PlaybackTimer_Tick;
    }

    private void PlaybackTimer_Tick(object? sender, EventArgs e)
    {
        if (_currentVideo == null || !_isPlaying) return;
        
        _currentPosition += 0.1;
        if (_currentPosition >= _currentVideo.DurationSeconds)
        {
            _currentPosition = 0;
            _isPlaying = false;
            _playbackTimer.Stop();
            PlayPauseButton.Content = "▶";
            PlayOverlay.Visibility = Visibility.Collapsed;
            
            // Auto-play next
            var currentIndex = _videos.IndexOf(_currentVideo);
            if (currentIndex < _videos.Count - 1)
            {
                LoadVideo(_videos[currentIndex + 1]);
                Play();
            }
        }
        
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (_currentVideo == null || _isUserSeeking) return;
        
        var progress = (_currentPosition / _currentVideo.DurationSeconds) * 100;
        ProgressSlider.Value = progress;
        CurrentTime.Text = FormatTime(_currentPosition);
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return ts.Hours > 0 
            ? $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}" 
            : $"{ts.Minutes}:{ts.Seconds:D2}";
    }

    private void LoadVideo(VideoItem video)
    {
        _currentVideo = video;
        _currentPosition = 0;
        VideoTitle.Text = video.Title;
        VideoInfo.Text = $"{video.Genre} • {FormatTime(video.DurationSeconds)}";
        TotalTime.Text = FormatTime(video.DurationSeconds);
        CurrentTime.Text = "0:00";
        ProgressSlider.Value = 0;
        VideoIcon.Text = video.Title.Split(' ')[0]; // Use emoji from title
    }

    private void Play()
    {
        if (_currentVideo == null) return;
        
        _isPlaying = true;
        PlayPauseButton.Content = "⏸";
        PlayOverlay.Visibility = Visibility.Visible;
        _playbackTimer.Start();
    }

    private void Pause()
    {
        _isPlaying = false;
        PlayPauseButton.Content = "▶";
        PlayOverlay.Visibility = Visibility.Collapsed;
        _playbackTimer.Stop();
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentVideo == null)
        {
            if (_videos.Count > 0)
            {
                LoadVideo(_videos[0]);
            }
            else return;
        }
        
        if (_isPlaying) Pause();
        else Play();
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        LibraryPopup.IsOpen = true;
    }

    private void CloseLibrary_Click(object sender, RoutedEventArgs e)
    {
        LibraryPopup.IsOpen = false;
    }

    private void VideoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (VideoList.SelectedItem is VideoItem video)
        {
            LoadVideo(video);
            LibraryPopup.IsOpen = false;
            Play();
        }
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentVideo == null) return;
        
        var currentIndex = _videos.IndexOf(_currentVideo);
        if (currentIndex > 0)
        {
            LoadVideo(_videos[currentIndex - 1]);
            if (_isPlaying) Play();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentVideo == null) return;
        
        var currentIndex = _videos.IndexOf(_currentVideo);
        if (currentIndex < _videos.Count - 1)
        {
            LoadVideo(_videos[currentIndex + 1]);
            if (_isPlaying) Play();
        }
    }

    private void RewindButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentVideo == null) return;
        _currentPosition = Math.Max(0, _currentPosition - 10);
        UpdateProgress();
    }

    private void FastForwardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentVideo == null) return;
        _currentPosition = Math.Min(_currentVideo.DurationSeconds, _currentPosition + 10);
        UpdateProgress();
    }

    private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_currentVideo == null || !_isUserSeeking) return;
        _currentPosition = (e.NewValue / 100) * _currentVideo.DurationSeconds;
        CurrentTime.Text = FormatTime(_currentPosition);
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _volume = (int)e.NewValue;
    }

    private void FullscreenButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Fullscreen mode is not available in simulated environment.", 
            "Video Player", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private class VideoItem
    {
        public string Title { get; }
        public string Genre { get; }
        public double DurationSeconds { get; }

        public VideoItem(string title, string genre, double duration)
        {
            Title = title;
            Genre = genre;
            DurationSeconds = duration;
        }

        public override string ToString() => $"{Title} ({Genre})";
    }
}
