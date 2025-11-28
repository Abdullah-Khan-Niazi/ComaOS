using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ComaOS.UI.MVVM.Views.Apps;

/// <summary>
/// Interactive Calendar with event management.
/// </summary>
public partial class CalendarView : UserControl
{
    private DateTime _currentMonth;
    private DateTime _selectedDate;
    private readonly Dictionary<DateTime, List<CalendarEvent>> _events;

    public CalendarView()
    {
        InitializeComponent();
        
        _events = new Dictionary<DateTime, List<CalendarEvent>>();
        _currentMonth = DateTime.Today;
        _selectedDate = DateTime.Today;
        
        // Add some sample events
        AddSampleEvents();
        
        RenderCalendar();
        UpdateEventsDisplay();
    }

    private void AddSampleEvents()
    {
        var today = DateTime.Today;
        
        AddEvent(today, "Team Standup", "09:00");
        AddEvent(today, "Project Review", "14:00");
        AddEvent(today.AddDays(1), "Doctor Appointment", "10:30");
        AddEvent(today.AddDays(2), "Birthday Party 🎂", "18:00");
        AddEvent(today.AddDays(5), "Dentist", "11:00");
        AddEvent(today.AddDays(7), "Meeting with Client", "15:00");
        AddEvent(new DateTime(today.Year, today.Month, 25), "Christmas 🎄", "All Day");
        AddEvent(new DateTime(today.Year, today.Month, 1), "Month Start Review", "09:00");
    }

    private void AddEvent(DateTime date, string title, string time)
    {
        var dateOnly = date.Date;
        if (!_events.ContainsKey(dateOnly))
            _events[dateOnly] = new List<CalendarEvent>();
        
        _events[dateOnly].Add(new CalendarEvent(title, time));
    }

    private void RenderCalendar()
    {
        CalendarGrid.Children.Clear();
        
        MonthYearText.Text = _currentMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        
        var firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
        var startDayOfWeek = (int)firstDay.DayOfWeek;
        
        // Previous month's trailing days
        var prevMonth = firstDay.AddMonths(-1);
        var daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        
        for (int i = startDayOfWeek - 1; i >= 0; i--)
        {
            var day = daysInPrevMonth - i;
            var btn = CreateDayButton(day, true, prevMonth.Year, prevMonth.Month);
            CalendarGrid.Children.Add(btn);
        }
        
        // Current month days
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_currentMonth.Year, _currentMonth.Month, day);
            var btn = CreateDayButton(day, false, _currentMonth.Year, _currentMonth.Month);
            
            // Highlight today
            if (date.Date == DateTime.Today)
            {
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60a5fa")!);
                btn.Foreground = Brushes.White;
            }
            
            // Highlight selected date
            if (date.Date == _selectedDate.Date && date.Date != DateTime.Today)
            {
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a3a4a")!);
            }
            
            // Mark days with events
            if (_events.ContainsKey(date.Date))
            {
                btn.Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = day.ToString(), HorizontalAlignment = HorizontalAlignment.Center },
                        new Ellipse { Width = 4, Height = 4, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!), HorizontalAlignment = HorizontalAlignment.Center }
                    }
                };
            }
            
            // Color Sunday red, Saturday blue
            if (date.DayOfWeek == DayOfWeek.Sunday && date.Date != DateTime.Today)
                btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f87171")!);
            else if (date.DayOfWeek == DayOfWeek.Saturday && date.Date != DateTime.Today)
                btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60a5fa")!);
            
            CalendarGrid.Children.Add(btn);
        }
        
        // Next month's leading days
        var totalCells = startDayOfWeek + daysInMonth;
        var remainingCells = (totalCells <= 35) ? 35 - totalCells : 42 - totalCells;
        
        for (int day = 1; day <= remainingCells; day++)
        {
            var nextMonth = firstDay.AddMonths(1);
            var btn = CreateDayButton(day, true, nextMonth.Year, nextMonth.Month);
            CalendarGrid.Children.Add(btn);
        }
    }

    private Button CreateDayButton(int day, bool isOtherMonth, int year, int month)
    {
        var btn = new Button
        {
            Content = day.ToString(),
            Background = Brushes.Transparent,
            Foreground = isOtherMonth 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444")!) 
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ccc")!),
            BorderThickness = new Thickness(0),
            FontSize = 13,
            Padding = new Thickness(0, 8, 0, 8),
            Cursor = System.Windows.Input.Cursors.Hand,
            Tag = new DateTime(year, month, day)
        };
        
        btn.Click += DayButton_Click;
        return btn;
    }

    private void DayButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is DateTime date)
        {
            _selectedDate = date;
            
            // If clicked on different month, navigate to it
            if (date.Month != _currentMonth.Month || date.Year != _currentMonth.Year)
            {
                _currentMonth = new DateTime(date.Year, date.Month, 1);
            }
            
            RenderCalendar();
            UpdateEventsDisplay();
        }
    }

    private void UpdateEventsDisplay()
    {
        EventsList.Children.Clear();
        
        var dateStr = _selectedDate.Date == DateTime.Today 
            ? "Today" 
            : _selectedDate.ToString("MMM d, yyyy");
        SelectedDateText.Text = $"📅 Events for {dateStr}";
        
        if (_events.TryGetValue(_selectedDate.Date, out var events))
        {
            foreach (var evt in events)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a1a2a")!),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                
                var titleText = new TextBlock
                {
                    Text = evt.Title,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ccc")!),
                    FontSize = 12
                };
                
                var timeText = new TextBlock
                {
                    Text = evt.Time,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60a5fa")!),
                    FontSize = 11
                };
                Grid.SetColumn(timeText, 1);
                
                grid.Children.Add(titleText);
                grid.Children.Add(timeText);
                border.Child = grid;
                
                EventsList.Children.Add(border);
            }
        }
        else
        {
            EventsList.Children.Add(new TextBlock
            {
                Text = "No events scheduled",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555")!),
                FontSize = 12,
                FontStyle = FontStyles.Italic
            });
        }
    }

    private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(-1);
        RenderCalendar();
    }

    private void NextMonthButton_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(1);
        RenderCalendar();
    }

    private void TodayButton_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = DateTime.Today;
        _selectedDate = DateTime.Today;
        RenderCalendar();
        UpdateEventsDisplay();
    }

    private void AddEventButton_Click(object sender, RoutedEventArgs e)
    {
        EventTitleInput.Text = "";
        EventTimeInput.Text = "09:00";
        AddEventPopup.IsOpen = true;
    }

    private void CancelEvent_Click(object sender, RoutedEventArgs e)
    {
        AddEventPopup.IsOpen = false;
    }

    private void SaveEvent_Click(object sender, RoutedEventArgs e)
    {
        var title = EventTitleInput.Text.Trim();
        var time = EventTimeInput.Text.Trim();
        
        if (string.IsNullOrEmpty(title))
        {
            MessageBox.Show("Please enter an event title.", "Calendar", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        AddEvent(_selectedDate, title, time);
        AddEventPopup.IsOpen = false;
        RenderCalendar();
        UpdateEventsDisplay();
    }

    private class CalendarEvent
    {
        public string Title { get; }
        public string Time { get; }

        public CalendarEvent(string title, string time)
        {
            Title = title;
            Time = time;
        }
    }
}
