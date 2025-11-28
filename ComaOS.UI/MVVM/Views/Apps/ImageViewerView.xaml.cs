using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Image Viewer with gallery and zoom/rotate functionality.
/// </summary>
public partial class ImageViewerView : UserControl
{
    private readonly List<ImageItem> _images;
    private ImageItem? _currentImage;
    private int _currentIndex = -1;
    private double _zoom = 1.0;
    private double _rotation = 0;

    public ImageViewerView()
    {
        InitializeComponent();
        
        _images = new List<ImageItem>
        {
            new("🌅", "Sunset Beach", "1920x1080", "2.4 MB", "Landscape"),
            new("🏔", "Mountain Peak", "3840x2160", "5.1 MB", "Nature"),
            new("🌸", "Cherry Blossom", "2560x1440", "3.2 MB", "Flowers"),
            new("🌊", "Ocean Waves", "1920x1080", "1.8 MB", "Seascape"),
            new("🌲", "Forest Trail", "2048x1365", "2.9 MB", "Nature"),
            new("🏙", "City Skyline", "3840x2160", "4.7 MB", "Urban"),
            new("🌌", "Starry Night", "2560x1440", "2.1 MB", "Astronomy"),
            new("🦋", "Butterfly Garden", "1600x1200", "1.5 MB", "Macro"),
            new("🐱", "Cute Cat", "1920x1280", "1.2 MB", "Animals"),
            new("🎨", "Abstract Art", "2000x2000", "3.8 MB", "Art"),
            new("🏖", "Tropical Paradise", "3000x2000", "4.2 MB", "Travel"),
            new("🌻", "Sunflower Field", "2560x1600", "2.6 MB", "Flowers"),
        };
        
        ImageList.ItemsSource = _images;
        NoImagePlaceholder.Visibility = Visibility.Visible;
        ImageDisplay.Visibility = Visibility.Collapsed;
    }

    private void LoadImage(ImageItem image, int index)
    {
        _currentImage = image;
        _currentIndex = index;
        _zoom = 1.0;
        _rotation = 0;
        
        NoImagePlaceholder.Visibility = Visibility.Collapsed;
        ImageDisplay.Visibility = Visibility.Visible;
        
        ImageEmoji.Text = image.Emoji;
        ImageTitle.Text = image.Name;
        ImageInfo.Text = $"{image.Name} • {image.Resolution} • {image.Size} • {image.Category}";
        ImageIndex.Text = $"{index + 1} / {_images.Count}";
        
        UpdateTransform();
        UpdateZoomLevel();
    }

    private void UpdateTransform()
    {
        ImageScale.ScaleX = _zoom;
        ImageScale.ScaleY = _zoom;
        ImageRotation.Angle = _rotation;
    }

    private void UpdateZoomLevel()
    {
        ZoomLevel.Text = $"{(int)(_zoom * 100)}%";
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        GalleryPopup.IsOpen = true;
    }

    private void CloseGallery_Click(object sender, RoutedEventArgs e)
    {
        GalleryPopup.IsOpen = false;
    }

    private void ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ImageList.SelectedItem is ImageItem image)
        {
            var index = _images.IndexOf(image);
            LoadImage(image, index);
            GalleryPopup.IsOpen = false;
        }
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        if (_images.Count == 0) return;
        
        if (_currentIndex <= 0)
            _currentIndex = _images.Count - 1;
        else
            _currentIndex--;
        
        LoadImage(_images[_currentIndex], _currentIndex);
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_images.Count == 0) return;
        
        if (_currentIndex < 0 || _currentIndex >= _images.Count - 1)
            _currentIndex = 0;
        else
            _currentIndex++;
        
        LoadImage(_images[_currentIndex], _currentIndex);
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null) return;
        
        _zoom = System.Math.Min(3.0, _zoom + 0.25);
        UpdateTransform();
        UpdateZoomLevel();
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null) return;
        
        _zoom = System.Math.Max(0.25, _zoom - 0.25);
        UpdateTransform();
        UpdateZoomLevel();
    }

    private void FitButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null) return;
        
        _zoom = 1.0;
        UpdateTransform();
        UpdateZoomLevel();
    }

    private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null) return;
        
        _rotation -= 90;
        if (_rotation < 0) _rotation += 360;
        UpdateTransform();
    }

    private void RotateRightButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null) return;
        
        _rotation += 90;
        if (_rotation >= 360) _rotation -= 360;
        UpdateTransform();
    }

    private class ImageItem
    {
        public string Emoji { get; }
        public string Name { get; }
        public string Resolution { get; }
        public string Size { get; }
        public string Category { get; }

        public ImageItem(string emoji, string name, string resolution, string size, string category)
        {
            Emoji = emoji;
            Name = name;
            Resolution = resolution;
            Size = size;
            Category = category;
        }

        public override string ToString() => $"{Emoji} {Name} ({Resolution})";
    }
}
