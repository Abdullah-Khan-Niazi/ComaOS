using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComaOS.UI.MVVM.Views.Apps;

namespace ComaOS.UI.MVVM.Views;

/// <summary>
/// Interaction logic for WindowFrameView.xaml
/// Provides window-like behavior for OS application windows.
/// </summary>
public partial class WindowFrameView : UserControl
{
    private Point _dragStartPoint;
    private bool _isDragging;

    public WindowFrameView()
    {
        InitializeComponent();
        DataContextChanged += WindowFrameView_DataContextChanged;
    }

    /// <summary>
    /// Loads the appropriate app view when DataContext changes.
    /// </summary>
    private void WindowFrameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ViewModels.WindowViewModel vm)
        {
            LoadAppContent(vm.AppName);
        }
    }

    /// <summary>
    /// Loads the appropriate UserControl based on app name.
    /// </summary>
    private void LoadAppContent(string appName)
    {
        UserControl? appView = appName.ToLowerInvariant() switch
        {
            "terminal" => new TerminalView { DataContext = DataContext },
            "calculator" => new CalculatorView(),
            "notepad" => new NotepadView { DataContext = DataContext },
            "file manager" => new FileManagerView { DataContext = DataContext },
            "system monitor" => new SystemMonitorView { DataContext = DataContext },
            "settings" => new SettingsView(),
            "browser" => new BrowserView(),
            "music player" => new MusicPlayerView(),
            "minesweeper" => new MinesweeperView(),
            "video player" => new VideoPlayerView(),
            "image viewer" => new ImageViewerView(),
            "calendar" => new CalendarView(),
            "clock" => new ClockView(),
            "antivirus" => new AntivirusView(),
            "compression tool" => new CompressionToolView(),
            _ => CreateDefaultAppView(appName)
        };

        AppContent.Content = appView;
    }

    /// <summary>
    /// Creates a default view for unknown apps.
    /// </summary>
    private UserControl CreateDefaultAppView(string appName)
    {
        var view = new UserControl
        {
            Content = new TextBlock
            {
                Text = $"🚧 {appName} - Coming Soon!",
                FontSize = 18,
                Foreground = System.Windows.Media.Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        return view;
    }

    /// <summary>
    /// Gets the MainViewModel from the MainWindow.
    /// </summary>
    private ViewModels.MainViewModel? GetMainViewModel()
    {
        var mainWindow = Window.GetWindow(this);
        return mainWindow?.DataContext as ViewModels.MainViewModel;
    }

    /// <summary>
    /// Handles the minimize button click.
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        var mainVM = GetMainViewModel();
        var windowVM = DataContext as ViewModels.WindowViewModel;
        if (mainVM != null && windowVM != null)
        {
            mainVM.MinimizeWindowCommand.Execute(windowVM);
        }
    }

    /// <summary>
    /// Handles the maximize button click.
    /// </summary>
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        var mainVM = GetMainViewModel();
        var windowVM = DataContext as ViewModels.WindowViewModel;
        if (mainVM != null && windowVM != null)
        {
            mainVM.MaximizeWindowCommand.Execute(windowVM);
        }
    }

    /// <summary>
    /// Handles the close button click.
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        var mainVM = GetMainViewModel();
        var windowVM = DataContext as ViewModels.WindowViewModel;
        if (mainVM != null && windowVM != null)
        {
            mainVM.CloseWindowCommand.Execute(windowVM);
        }
    }

    /// <summary>
    /// Handles mouse down on the title bar for window dragging.
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double-click to maximize/restore
            MaximizeButton_Click(sender, e);
            return;
        }

        _isDragging = true;
        _dragStartPoint = e.GetPosition(this);
        ((UIElement)sender).CaptureMouse();
    }

    /// <summary>
    /// Handles mouse move for window dragging.
    /// </summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentPosition = e.GetPosition(this.Parent as UIElement);
            
            if (DataContext is ViewModels.WindowViewModel vm)
            {
                vm.Left = currentPosition.X - _dragStartPoint.X;
                vm.Top = currentPosition.Y - _dragStartPoint.Y;
            }
        }
    }

    /// <summary>
    /// Handles mouse up to stop dragging.
    /// </summary>
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        _isDragging = false;
        ReleaseMouseCapture();
    }

    /// <summary>
    /// Handles click on the window to bring it to front.
    /// </summary>
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Bring window to front - handled by click event
    }
}
