using System.Windows;
using System.Windows.Controls;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Fully functional calculator application.
/// </summary>
public partial class CalculatorView : UserControl
{
    private double _currentValue = 0;
    private double _previousValue = 0;
    private string _currentOperator = "";
    private bool _isNewEntry = true;
    private bool _hasDecimal = false;
    private string _expression = "";

    public CalculatorView()
    {
        InitializeComponent();
    }

    private void Number_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var number = button.Content.ToString();
            
            if (_isNewEntry)
            {
                Display.Text = number == "0" ? "0" : number;
                _isNewEntry = false;
                _hasDecimal = false;
            }
            else
            {
                if (Display.Text == "0" && number != "0")
                {
                    Display.Text = number;
                }
                else if (Display.Text != "0" || number != "0")
                {
                    // Limit display length
                    if (Display.Text.Length < 15)
                    {
                        Display.Text += number;
                    }
                }
            }
        }
    }

    private void Decimal_Click(object sender, RoutedEventArgs e)
    {
        if (_isNewEntry)
        {
            Display.Text = "0.";
            _isNewEntry = false;
            _hasDecimal = true;
        }
        else if (!_hasDecimal)
        {
            Display.Text += ".";
            _hasDecimal = true;
        }
    }

    private void Operator_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string op)
        {
            if (!_isNewEntry && !string.IsNullOrEmpty(_currentOperator))
            {
                // Calculate previous operation first
                Calculate();
            }

            _previousValue = double.Parse(Display.Text);
            _currentOperator = op;
            _isNewEntry = true;
            
            // Update expression display
            var displayOp = op switch
            {
                "*" => "×",
                "/" => "÷",
                "-" => "−",
                _ => op
            };
            _expression = $"{FormatNumber(_previousValue)} {displayOp}";
            ExpressionDisplay.Text = _expression;
        }
    }

    private void Equals_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentOperator))
        {
            var displayOp = _currentOperator switch
            {
                "*" => "×",
                "/" => "÷",
                "-" => "−",
                _ => _currentOperator
            };
            _expression = $"{FormatNumber(_previousValue)} {displayOp} {Display.Text} =";
            ExpressionDisplay.Text = _expression;
            
            Calculate();
            _currentOperator = "";
        }
    }

    private void Calculate()
    {
        _currentValue = double.Parse(Display.Text);

        double result = _currentOperator switch
        {
            "+" => _previousValue + _currentValue,
            "-" => _previousValue - _currentValue,
            "*" => _previousValue * _currentValue,
            "/" => _currentValue != 0 ? _previousValue / _currentValue : double.NaN,
            _ => _currentValue
        };

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            Display.Text = "Error";
            _isNewEntry = true;
            _currentOperator = "";
            _expression = "";
            ExpressionDisplay.Text = "";
        }
        else
        {
            Display.Text = FormatNumber(result);
            _previousValue = result;
            _isNewEntry = true;
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        Display.Text = "0";
        ExpressionDisplay.Text = "";
        _currentValue = 0;
        _previousValue = 0;
        _currentOperator = "";
        _isNewEntry = true;
        _hasDecimal = false;
        _expression = "";
    }

    private void PlusMinus_Click(object sender, RoutedEventArgs e)
    {
        if (Display.Text != "0" && Display.Text != "Error")
        {
            if (Display.Text.StartsWith("-"))
            {
                Display.Text = Display.Text.Substring(1);
            }
            else
            {
                Display.Text = "-" + Display.Text;
            }
        }
    }

    private void Percent_Click(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(Display.Text, out double value))
        {
            value /= 100;
            Display.Text = FormatNumber(value);
            _isNewEntry = true;
        }
    }

    private static string FormatNumber(double value)
    {
        // Handle very large or very small numbers
        if (Math.Abs(value) >= 1e10 || (Math.Abs(value) < 1e-6 && value != 0))
        {
            return value.ToString("E4");
        }
        
        // Remove trailing zeros after decimal point
        var result = value.ToString("G10");
        
        // Limit length
        if (result.Length > 12)
        {
            result = value.ToString("G8");
        }
        
        return result;
    }
}
