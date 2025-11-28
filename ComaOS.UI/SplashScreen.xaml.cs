using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ComaOS.UI;

/// <summary>
/// Splash screen for ComaOS with animated "CHANCE" loading.
/// </summary>
public partial class SplashScreen : Window
{
    private readonly string[] _loadingMessages =
    [
        "Initializing system components...",
        "Loading kernel modules...",
        "Setting up memory management...",
        "Initializing CPU scheduler...",
        "Mounting virtual file system...",
        "Loading device drivers...",
        "Starting system services...",
        "Configuring user interface...",
        "Preparing desktop environment...",
        "Focus on what matters...",
        "Welcome to ComaOS!"
    ];

    private TextBlock[] _letters = null!;

    public SplashScreen()
    {
        InitializeComponent();
        Loaded += SplashScreen_Loaded;
    }

    private async void SplashScreen_Loaded(object sender, RoutedEventArgs e)
    {
        _letters = [Letter_C, Letter_H, Letter_A, Letter_N, Letter_C2, Letter_E];
        await AnimateLoadingAsync();
    }

    /// <summary>
    /// Animates the CHANCE letters, progress bar, and loading messages.
    /// </summary>
    private async Task AnimateLoadingAsync()
    {
        double maxWidth = 490; // Progress bar max width
        int totalDuration = 5500; // Total animation time ~5.5 seconds
        int letterAnimationTime = 2400; // Time to animate all letters
        int letterDelay = letterAnimationTime / _letters.Length; // ~400ms per letter

        // Gradient colors for letters
        var letterColors = new Color[]
        {
            (Color)ColorConverter.ConvertFromString("#4361ee")!,
            (Color)ColorConverter.ConvertFromString("#4895ef")!,
            (Color)ColorConverter.ConvertFromString("#4cc9f0")!,
            (Color)ColorConverter.ConvertFromString("#f72585")!,
            (Color)ColorConverter.ConvertFromString("#b5179e")!,
            (Color)ColorConverter.ConvertFromString("#7209b7")!
        };

        // Start progress bar animation
        var progressAnimation = new DoubleAnimation
        {
            From = 0,
            To = maxWidth,
            Duration = TimeSpan.FromMilliseconds(totalDuration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        ProgressBar.BeginAnimation(WidthProperty, progressAnimation);

        // Start message cycling in background
        _ = CycleMessagesAsync(totalDuration);

        // Animate letters one by one
        for (int i = 0; i < _letters.Length; i++)
        {
            await AnimateLetterAsync(_letters[i], letterColors[i], letterDelay);
        }

        // Show tagline after letters complete
        Tagline.Text = "Focus on what matters.";
        var taglineAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(500)
        };
        Tagline.BeginAnimation(OpacityProperty, taglineAnimation);

        // Wait for remaining time
        await Task.Delay(totalDuration - letterAnimationTime - 500);
    }

    /// <summary>
    /// Animates a single letter with color and scale effect.
    /// </summary>
    private async Task AnimateLetterAsync(TextBlock letter, Color targetColor, int duration)
    {
        // Color animation
        var colorAnimation = new ColorAnimation
        {
            To = targetColor,
            Duration = TimeSpan.FromMilliseconds(duration * 0.6),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333")!);
        letter.Foreground = brush;
        brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

        // Scale transform for pop effect
        var scaleTransform = new ScaleTransform(1, 1);
        letter.RenderTransform = scaleTransform;
        letter.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleUp = new DoubleAnimation
        {
            To = 1.15,
            Duration = TimeSpan.FromMilliseconds(duration * 0.3),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleDown = new DoubleAnimation
        {
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(duration * 0.3),
            BeginTime = TimeSpan.FromMilliseconds(duration * 0.3),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);

        await Task.Delay((int)(duration * 0.3));

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDown);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDown);

        await Task.Delay((int)(duration * 0.7));
    }

    /// <summary>
    /// Cycles through loading messages during the animation.
    /// </summary>
    private async Task CycleMessagesAsync(int totalDuration)
    {
        int messageDelay = totalDuration / _loadingMessages.Length;

        foreach (var message in _loadingMessages)
        {
            LoadingText.Text = message;
            await Task.Delay(messageDelay);
        }
    }
}
