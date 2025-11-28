using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Fully playable Minesweeper game.
/// </summary>
public partial class MinesweeperView : UserControl
{
    private int _gridSize = 9;
    private int _mineCount = 10;
    private int[,] _board = null!;  // -1 = mine, 0-8 = adjacent mine count
    private bool[,] _revealed = null!;
    private bool[,] _flagged = null!;
    private Button[,] _buttons = null!;
    private bool _gameOver = false;
    private bool _gameStarted = false;
    private int _flagsPlaced = 0;
    private int _cellsRevealed = 0;
    private int _seconds = 0;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    private static readonly SolidColorBrush[] NumberColors = new[]
    {
        Brushes.Transparent,    // 0
        Brushes.DodgerBlue,     // 1
        Brushes.Green,          // 2
        Brushes.Red,            // 3
        Brushes.DarkBlue,       // 4
        Brushes.DarkRed,        // 5
        Brushes.Teal,           // 6
        Brushes.Black,          // 7
        Brushes.Gray            // 8
    };

    public MinesweeperView()
    {
        InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        _timer.Stop();
        _seconds = 0;
        TimerText.Text = "000";
        _gameOver = false;
        _gameStarted = false;
        _flagsPlaced = 0;
        _cellsRevealed = 0;
        FaceButton.Content = "😊";
        MineCount.Text = _mineCount.ToString();
        StatusText.Text = "Click a cell to start!";

        _board = new int[_gridSize, _gridSize];
        _revealed = new bool[_gridSize, _gridSize];
        _flagged = new bool[_gridSize, _gridSize];
        _buttons = new Button[_gridSize, _gridSize];

        GameGrid.Children.Clear();
        GameGrid.Rows = _gridSize;
        GameGrid.Columns = _gridSize;
        GameGrid.Width = _gridSize * 30;
        GameGrid.Height = _gridSize * 30;

        for (int row = 0; row < _gridSize; row++)
        {
            for (int col = 0; col < _gridSize; col++)
            {
                var btn = new Button
                {
                    Width = 30,
                    Height = 30,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Background = new SolidColorBrush(Color.FromRgb(60, 60, 80)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 100)),
                    BorderThickness = new Thickness(1),
                    Tag = (row, col)
                };
                btn.Click += Cell_Click;
                btn.MouseRightButtonDown += Cell_RightClick;
                _buttons[row, col] = btn;
                GameGrid.Children.Add(btn);
            }
        }
    }

    private void PlaceMines(int excludeRow, int excludeCol)
    {
        // Place mines randomly, excluding the first clicked cell and its neighbors
        int minesPlaced = 0;
        while (minesPlaced < _mineCount)
        {
            int row = _random.Next(_gridSize);
            int col = _random.Next(_gridSize);

            // Don't place mine on first click or adjacent cells
            if (Math.Abs(row - excludeRow) <= 1 && Math.Abs(col - excludeCol) <= 1)
                continue;

            if (_board[row, col] != -1)
            {
                _board[row, col] = -1;
                minesPlaced++;
            }
        }

        // Calculate adjacent mine counts
        for (int row = 0; row < _gridSize; row++)
        {
            for (int col = 0; col < _gridSize; col++)
            {
                if (_board[row, col] != -1)
                {
                    _board[row, col] = CountAdjacentMines(row, col);
                }
            }
        }
    }

    private int CountAdjacentMines(int row, int col)
    {
        int count = 0;
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                int nr = row + dr;
                int nc = col + dc;
                if (nr >= 0 && nr < _gridSize && nc >= 0 && nc < _gridSize && _board[nr, nc] == -1)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void Cell_Click(object sender, RoutedEventArgs e)
    {
        if (_gameOver) return;
        if (sender is not Button btn || btn.Tag is not (int row, int col)) return;
        if (_flagged[row, col]) return;

        if (!_gameStarted)
        {
            _gameStarted = true;
            PlaceMines(row, col);
            _timer.Start();
            StatusText.Text = "Good luck!";
        }

        RevealCell(row, col);
    }

    private void Cell_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (_gameOver || !_gameStarted) return;
        if (sender is not Button btn || btn.Tag is not (int row, int col)) return;
        if (_revealed[row, col]) return;

        _flagged[row, col] = !_flagged[row, col];
        
        if (_flagged[row, col])
        {
            _flagsPlaced++;
            btn.Content = "🚩";
            btn.Foreground = Brushes.Red;
        }
        else
        {
            _flagsPlaced--;
            btn.Content = "";
        }

        MineCount.Text = (_mineCount - _flagsPlaced).ToString();
        e.Handled = true;
    }

    private void RevealCell(int row, int col)
    {
        if (row < 0 || row >= _gridSize || col < 0 || col >= _gridSize) return;
        if (_revealed[row, col] || _flagged[row, col]) return;

        _revealed[row, col] = true;
        _cellsRevealed++;
        var btn = _buttons[row, col];
        btn.Background = new SolidColorBrush(Color.FromRgb(40, 40, 55));

        if (_board[row, col] == -1)
        {
            // Hit a mine!
            GameOver(false);
            return;
        }

        int adjacentMines = _board[row, col];
        if (adjacentMines > 0)
        {
            btn.Content = adjacentMines.ToString();
            btn.Foreground = NumberColors[adjacentMines];
        }
        else
        {
            // Reveal adjacent cells for empty cells
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    RevealCell(row + dr, col + dc);
                }
            }
        }

        // Check for win
        if (_cellsRevealed == (_gridSize * _gridSize) - _mineCount)
        {
            GameOver(true);
        }
    }

    private void GameOver(bool won)
    {
        _gameOver = true;
        _timer.Stop();

        if (won)
        {
            FaceButton.Content = "😎";
            StatusText.Text = $"🎉 You won in {_seconds} seconds!";
            
            // Reveal all mines with flags
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    if (_board[row, col] == -1)
                    {
                        _buttons[row, col].Content = "🚩";
                        _buttons[row, col].Foreground = Brushes.Green;
                    }
                }
            }
        }
        else
        {
            FaceButton.Content = "😵";
            StatusText.Text = "💥 Game Over! Click 😵 to restart.";
            
            // Reveal all mines
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    if (_board[row, col] == -1)
                    {
                        _buttons[row, col].Content = "💣";
                        _buttons[row, col].Background = _revealed[row, col] 
                            ? Brushes.Red 
                            : new SolidColorBrush(Color.FromRgb(60, 40, 40));
                    }
                    else if (_flagged[row, col])
                    {
                        // Wrong flag
                        _buttons[row, col].Content = "❌";
                    }
                }
            }
        }
    }

    private void NewGame_Click(object sender, RoutedEventArgs e)
    {
        InitializeGame();
    }

    private void Difficulty_Changed(object sender, SelectionChangedEventArgs e)
    {
        // Don't initialize if controls aren't ready yet (happens during XAML load)
        if (GameGrid == null || !IsLoaded) return;

        if (DifficultyCombo.SelectedIndex == 0) // Easy
        {
            _gridSize = 9;
            _mineCount = 10;
        }
        else if (DifficultyCombo.SelectedIndex == 1) // Medium
        {
            _gridSize = 12;
            _mineCount = 25;
        }
        else // Hard
        {
            _gridSize = 15;
            _mineCount = 50;
        }

        InitializeGame();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _seconds++;
        TimerText.Text = _seconds.ToString("D3");
    }
}
