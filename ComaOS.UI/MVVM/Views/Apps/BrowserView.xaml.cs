using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Simulated web browser with virtual websites.
/// </summary>
public partial class BrowserView : UserControl
{
    private readonly Stack<string> _backHistory = new();
    private readonly Stack<string> _forwardHistory = new();
    private string _currentUrl = "https://comaos.local";

    // Virtual websites
    private readonly Dictionary<string, (string Title, string[] Content)> _websites = new(StringComparer.OrdinalIgnoreCase)
    {
        ["comaos.local"] = ("ComaOS Home", new[]
        {
            "# Welcome to ComaOS",
            "",
            "The most advanced simulated operating system.",
            "",
            "## Features",
            "• Multi-level queue scheduling",
            "• Virtual memory management", 
            "• Full application suite",
            "• 100% vibe coded by AI",
            "",
            "## Quick Start",
            "Click on any desktop icon to launch an application.",
            "Use the Terminal for advanced operations."
        }),
        ["mail.comaos.local"] = ("ComaOS Mail", new[]
        {
            "# 📧 ComaOS Mail",
            "",
            "## Inbox (3 unread)",
            "",
            "📬 From: system@comaos.local",
            "   Subject: Welcome to ComaOS!",
            "   Welcome to your new operating system...",
            "",
            "📬 From: newsletter@comaos.local",
            "   Subject: What's new in ComaOS 1.0",
            "   Check out the latest features...",
            "",
            "📬 From: friend@comaos.local",
            "   Subject: Hey!",
            "   Just wanted to say hi..."
        }),
        ["news.comaos.local"] = ("ComaOS News", new[]
        {
            "# 📰 ComaOS News",
            "",
            "## Top Stories",
            "",
            "### AI Writes Entire Operating System",
            "In a groundbreaking achievement, Claude Opus 4.5 has",
            "successfully created a complete OS simulator.",
            "",
            "### \"Vibe Coding\" Declared Future of Development",
            "Experts say letting AI do all the work is the future.",
            "",
            "### Local Process Achieves Enlightenment",
            "A background process reports achieving inner peace",
            "while waiting in the ready queue."
        }),
        ["games.comaos.local"] = ("ComaOS Games", new[]
        {
            "# 🎮 ComaOS Games",
            "",
            "## Available Games",
            "",
            "💣 Minesweeper - Classic puzzle game",
            "🃏 Solitaire - Card game (coming soon)",
            "🐍 Snake - Retro arcade (coming soon)",
            "🧱 Tetris - Block stacking (coming soon)",
            "",
            "## Leaderboard",
            "1. COMA - 9999 pts",
            "2. CLAUDE - 8888 pts",
            "3. AI - 7777 pts"
        }),
        ["video.comaos.local"] = ("ComaOS Video", new[]
        {
            "# 📺 ComaOS Video",
            "",
            "## Trending",
            "",
            "▶️ \"How to Vibe Code\" - 1.2M views",
            "▶️ \"OS Development Speedrun\" - 890K views",
            "▶️ \"AI Takes Over the World\" - 2.1M views",
            "",
            "## Recommended",
            "",
            "▶️ \"Building ComaOS in 24 Hours\"",
            "▶️ \"Why Schedulers are Cool\"",
            "▶️ \"RAM Management for Dummies\""
        }),
        ["shop.comaos.local"] = ("ComaOS Shop", new[]
        {
            "# 🛒 ComaOS Shop",
            "",
            "## Featured Products",
            "",
            "💾 Extra RAM - $49.99",
            "   +1024 MB of virtual memory",
            "",
            "⚡ CPU Upgrade - $99.99",
            "   +2 virtual cores",
            "",
            "🎨 Theme Pack - $9.99",
            "   New color schemes (coming soon)",
            "",
            "🛡️ Premium Antivirus - FREE",
            "   Because we love you"
        }),
        ["search.comaos.local"] = ("ComaOS Search", new[]
        {
            "# 🔍 ComaOS Search",
            "",
            "## Search Results",
            "",
            "Your search returned 42 results.",
            "",
            "Did you mean: the meaning of life?",
            "",
            "## Top Results",
            "• ComaOS Documentation",
            "• Terminal Commands Guide",
            "• How to Exit Vim",
            "• Stack Overflow for AI"
        }),
        ["error"] = ("404 Not Found", new[]
        {
            "# ❌ 404 - Page Not Found",
            "",
            "The page you're looking for doesn't exist.",
            "",
            "Possible reasons:",
            "• The URL is incorrect",
            "• The page has been moved",
            "• This is a simulated browser",
            "",
            "Try going back to the home page."
        })
    };

    public BrowserView()
    {
        InitializeComponent();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (_backHistory.Count > 0)
        {
            _forwardHistory.Push(_currentUrl);
            _currentUrl = _backHistory.Pop();
            UrlBox.Text = $"https://{_currentUrl}";
            ShowPage(_currentUrl, false);
        }
    }

    private void Forward_Click(object sender, RoutedEventArgs e)
    {
        if (_forwardHistory.Count > 0)
        {
            _backHistory.Push(_currentUrl);
            _currentUrl = _forwardHistory.Pop();
            UrlBox.Text = $"https://{_currentUrl}";
            ShowPage(_currentUrl, false);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        ShowPage(_currentUrl, false);
    }

    private void Home_Click(object sender, RoutedEventArgs e)
    {
        Navigate("comaos.local");
    }

    private void Go_Click(object sender, RoutedEventArgs e)
    {
        var url = UrlBox.Text.Trim();
        url = url.Replace("https://", "").Replace("http://", "").TrimEnd('/');
        Navigate(url);
    }

    private void UrlBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Go_Click(sender, e);
        }
    }

    private void QuickLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string url)
        {
            Navigate(url);
        }
    }

    private void Navigate(string url)
    {
        if (url != _currentUrl)
        {
            _backHistory.Push(_currentUrl);
            _forwardHistory.Clear();
        }
        _currentUrl = url;
        UrlBox.Text = $"https://{url}";
        ShowPage(url, true);
    }

    private async void ShowPage(string url, bool animate)
    {
        HomePage.Visibility = Visibility.Collapsed;
        
        if (animate)
        {
            LoadingIndicator.Visibility = Visibility.Visible;
            await Task.Delay(300 + new Random().Next(200)); // Simulate loading
            LoadingIndicator.Visibility = Visibility.Collapsed;
        }

        PageContent.Children.Clear();
        
        // Find the website content
        var (title, content) = _websites.ContainsKey(url) 
            ? _websites[url] 
            : _websites["error"];

        foreach (var line in content)
        {
            var tb = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2)
            };

            if (line.StartsWith("# "))
            {
                tb.Text = line.Substring(2);
                tb.FontSize = 24;
                tb.FontWeight = FontWeights.Bold;
                tb.Foreground = Brushes.White;
                tb.Margin = new Thickness(0, 0, 0, 10);
            }
            else if (line.StartsWith("## "))
            {
                tb.Text = line.Substring(3);
                tb.FontSize = 18;
                tb.FontWeight = FontWeights.SemiBold;
                tb.Foreground = new SolidColorBrush(Color.FromRgb(96, 165, 250));
                tb.Margin = new Thickness(0, 15, 0, 8);
            }
            else if (line.StartsWith("### "))
            {
                tb.Text = line.Substring(4);
                tb.FontSize = 14;
                tb.FontWeight = FontWeights.SemiBold;
                tb.Foreground = new SolidColorBrush(Color.FromRgb(244, 114, 182));
                tb.Margin = new Thickness(0, 10, 0, 5);
            }
            else if (line.StartsWith("• ") || line.StartsWith("- "))
            {
                tb.Text = "  " + line;
                tb.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                tb.Text = " ";
                tb.Margin = new Thickness(0, 5, 0, 5);
            }
            else
            {
                tb.Text = line;
                tb.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            }

            PageContent.Children.Add(tb);
        }

        WebPageContent.Visibility = Visibility.Visible;
    }
}
