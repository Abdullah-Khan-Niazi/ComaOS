using System.Windows;
using System.Windows.Controls;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Notepad application with full text editing capabilities.
/// </summary>
public partial class NotepadView : UserControl
{
    private string _currentFileName = "Untitled.txt";
    private string _savedContent = "";
    private bool _isModified = false;
    private readonly Dictionary<string, string> _virtualFiles = new();

    public NotepadView()
    {
        InitializeComponent();
        
        // Initialize with some sample files
        _virtualFiles["readme.txt"] = "Welcome to ComaOS Notepad!\n\nThis is a fully functional text editor.\n\nFeatures:\n- Undo/Redo support\n- Cut/Copy/Paste\n- Character and line count\n- Multiple virtual files";
        _virtualFiles["notes.txt"] = "My Notes\n========\n\n- Remember to vibe code responsibly\n- ComaOS is 100% AI generated\n- Claude Opus 4.5 is the GOAT";
        _virtualFiles["todo.txt"] = "TODO List\n=========\n\n[ ] Finish the OS project\n[ ] Submit before deadline\n[x] Make it look cool\n[x] Add Easter eggs";
        _virtualFiles["secrets.txt"] = "🔐 CLASSIFIED 🔐\n\nJust kidding, there's nothing here.\n\nOr is there? 👀\n\n...\n\nNo, there really isn't.";
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        if (_isModified)
        {
            var result = MessageBox.Show("Do you want to save changes?", "ComaOS Notepad", 
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                SaveCurrentFile();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        Editor.Clear();
        _currentFileName = "Untitled.txt";
        _savedContent = "";
        _isModified = false;
        UpdateFileNameDisplay();
        UpdateModifiedIndicator();
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        // Show a simple file picker dialog
        var fileList = string.Join("\n", _virtualFiles.Keys.Select(f => $"  📄 {f}"));
        var message = $"Available files:\n{fileList}\n\nEnter filename to open:";
        
        var inputDialog = new InputDialog("Open File", message);
        if (inputDialog.ShowDialog() == true)
        {
            var filename = inputDialog.ResponseText.Trim();
            if (_virtualFiles.TryGetValue(filename, out var content))
            {
                Editor.Text = content;
                _currentFileName = filename;
                _savedContent = content;
                _isModified = false;
                UpdateFileNameDisplay();
                UpdateModifiedIndicator();
            }
            else
            {
                MessageBox.Show($"File '{filename}' not found!", "ComaOS Notepad", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFileName == "Untitled.txt")
        {
            // Save As
            var inputDialog = new InputDialog("Save As", "Enter filename:");
            if (inputDialog.ShowDialog() == true)
            {
                var filename = inputDialog.ResponseText.Trim();
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    if (!filename.Contains('.'))
                    {
                        filename += ".txt";
                    }
                    _currentFileName = filename;
                }
            }
            else
            {
                return;
            }
        }

        SaveCurrentFile();
    }

    private void SaveCurrentFile()
    {
        _virtualFiles[_currentFileName] = Editor.Text;
        _savedContent = Editor.Text;
        _isModified = false;
        UpdateFileNameDisplay();
        UpdateModifiedIndicator();
        
        MessageBox.Show($"File '{_currentFileName}' saved!", "ComaOS Notepad", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (Editor.CanUndo)
        {
            Editor.Undo();
        }
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        if (Editor.CanRedo)
        {
            Editor.Redo();
        }
    }

    private void Cut_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(Editor.SelectedText))
        {
            Clipboard.SetText(Editor.SelectedText);
            var start = Editor.SelectionStart;
            Editor.Text = Editor.Text.Remove(start, Editor.SelectionLength);
            Editor.SelectionStart = start;
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(Editor.SelectedText))
        {
            Clipboard.SetText(Editor.SelectedText);
        }
    }

    private void Paste_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsText())
        {
            var start = Editor.SelectionStart;
            var text = Clipboard.GetText();
            
            if (Editor.SelectionLength > 0)
            {
                Editor.Text = Editor.Text.Remove(start, Editor.SelectionLength);
            }
            
            Editor.Text = Editor.Text.Insert(start, text);
            Editor.SelectionStart = start + text.Length;
        }
    }

    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Update character and line count
        var text = Editor.Text;
        CharCount.Text = $"Characters: {text.Length}";
        LineCount.Text = $"Lines: {text.Split('\n').Length}";

        // Check if modified
        _isModified = text != _savedContent;
        UpdateModifiedIndicator();
    }

    private void UpdateFileNameDisplay()
    {
        FileNameDisplay.Text = $"📝 {_currentFileName}";
    }

    private void UpdateModifiedIndicator()
    {
        ModifiedIndicator.Text = _isModified ? "● Modified" : "";
    }
}

/// <summary>
/// Simple input dialog for getting text from user.
/// </summary>
public class InputDialog : Window
{
    private TextBox _textBox;
    
    public string ResponseText => _textBox.Text;

    public InputDialog(string title, string message)
    {
        Title = title;
        Width = 400;
        Height = 180;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 46));
        
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var label = new TextBlock
        {
            Text = message,
            Foreground = System.Windows.Media.Brushes.White,
            Margin = new Thickness(15),
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        _textBox = new TextBox
        {
            Margin = new Thickness(15, 5, 15, 15),
            Padding = new Thickness(8),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 42)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 58, 74)),
            CaretBrush = System.Windows.Media.Brushes.White
        };
        Grid.SetRow(_textBox, 1);
        grid.Children.Add(_textBox);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(15, 0, 15, 15)
        };

        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
            Padding = new Thickness(10, 5, 10, 5),
            Margin = new Thickness(0, 0, 10, 0),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(96, 165, 250)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderThickness = new Thickness(0)
        };
        okButton.Click += (s, e) => { DialogResult = true; };

        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 80,
            Padding = new Thickness(10, 5, 10, 5),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 58, 74)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderThickness = new Thickness(0)
        };
        cancelButton.Click += (s, e) => { DialogResult = false; };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        Grid.SetRow(buttonPanel, 2);
        grid.Children.Add(buttonPanel);

        Content = grid;
    }
}
