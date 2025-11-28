using System.Windows;

namespace ComaOS.UI;

/// <summary>
/// Interaction logic for App.xaml
/// Main application entry point for ComaOS UI.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Handles application startup - shows splash screen then main window.
    /// </summary>
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        // Show splash screen
        var splashScreen = new SplashScreen();
        splashScreen.Show();

        // Wait for splash animation to complete (~5.5 seconds)
        await Task.Delay(5800);

        // Create and show main window
        var mainWindow = new MainWindow();
        mainWindow.Show();

        // Close splash screen
        splashScreen.Close();
    }
}
