using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Ambulance.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Linq;

namespace Ambulance.ViewModels;

public class StatisticsViewModel : ReactiveObject
{
    private readonly DatabaseService _dbService;

    // Stat properties
    private int _totalCalls;
    public int TotalCalls 
    { 
        get => _totalCalls; 
        set => this.RaiseAndSetIfChanged(ref _totalCalls, value); 
    }

    private int _completedToday;
    public int CompletedToday 
    { 
        get => _completedToday; 
        set => this.RaiseAndSetIfChanged(ref _completedToday, value); 
    }

    private int _inProgress;
    public int InProgress 
    { 
        get => _inProgress; 
        set => this.RaiseAndSetIfChanged(ref _inProgress, value); 
    }

    private int _criticalCalls;
    public int CriticalCalls 
    { 
        get => _criticalCalls; 
        set => this.RaiseAndSetIfChanged(ref _criticalCalls, value); 
    }

    // Chart properties
    private ObservableCollection<ISeries> _statusSeries;
    public ObservableCollection<ISeries> StatusSeries
    {
        get => _statusSeries;
        set => this.RaiseAndSetIfChanged(ref _statusSeries, value);
    }

    private ObservableCollection<ISeries> _prioritySeries;
    public ObservableCollection<ISeries> PrioritySeries
    {
        get => _prioritySeries;
        set => this.RaiseAndSetIfChanged(ref _prioritySeries, value);
    }

    // Hourly Chart Properties
    private ObservableCollection<ISeries> _hourlySeries;
    public ObservableCollection<ISeries> HourlySeries
    {
        get => _hourlySeries;
        set => this.RaiseAndSetIfChanged(ref _hourlySeries, value);
    }

    private ObservableCollection<LiveChartsCore.SkiaSharpView.Axis> _hourlyXAxes;
    public ObservableCollection<LiveChartsCore.SkiaSharpView.Axis> HourlyXAxes
    {
        get => _hourlyXAxes;
        set => this.RaiseAndSetIfChanged(ref _hourlyXAxes, value);
    }

    // Weekly Chart Properties
    private ObservableCollection<ISeries> _weeklySeries;
    public ObservableCollection<ISeries> WeeklySeries
    {
        get => _weeklySeries;
        set => this.RaiseAndSetIfChanged(ref _weeklySeries, value);
    }

    private ObservableCollection<LiveChartsCore.SkiaSharpView.Axis> _weeklyXAxes;
    public ObservableCollection<LiveChartsCore.SkiaSharpView.Axis> WeeklyXAxes
    {
        get => _weeklyXAxes;
        set => this.RaiseAndSetIfChanged(ref _weeklyXAxes, value);
    }

    private ObservableCollection<string> _hourlyPeriods;
    public ObservableCollection<string> HourlyPeriods
    {
        get => _hourlyPeriods;
        set => this.RaiseAndSetIfChanged(ref _hourlyPeriods, value);
    }

    private string _selectedHourlyPeriod;
    public string SelectedHourlyPeriod
    {
        get => _selectedHourlyPeriod;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedHourlyPeriod, value);
            LoadHourlyChart();
        }
    }

    // Constructor
    public StatisticsViewModel()
    {
        string connectionString = "Server=localhost;Port=5432;Username=postgres;Password=123;Database=amb;";
        _dbService = new DatabaseService(connectionString);

        HourlyPeriods = new ObservableCollection<string> { "День", "Неделя", "Месяц", "Год", "Всё время" };
        _selectedHourlyPeriod = "День";

        LoadStatistics();
    }

    public void LoadStatistics()
    {
        var stats = _dbService.GetStatistics();
        TotalCalls = stats.TotalCalls;
        CompletedToday = stats.CompletedToday;
        InProgress = stats.InProgressCalls;
        CriticalCalls = stats.CriticalCalls;

        var chartsData = _dbService.GetChartsData(SelectedHourlyPeriod);

        // 1. Status Chart (Pie)
        var statusSeriesList = new List<ISeries>();
        foreach(var kvp in chartsData.CallsByStatus)
        {
            SKColor color = SKColors.Gray;
            if (kvp.Key == "Завершена") color = SKColor.Parse("#4CAF50");
            else if (kvp.Key == "В работе") color = SKColor.Parse("#FF9800");
            else if (kvp.Key == "Отменена") color = SKColor.Parse("#BDBDBD");
            else if (kvp.Key == "Новый") color = SKColor.Parse("#2196F3");

            statusSeriesList.Add(new PieSeries<int> 
            { 
                Values = new int[] { kvp.Value }, 
                Name = $"{kvp.Key} ({kvp.Value})",
                Fill = new SolidColorPaint(color)
            });
        }
        StatusSeries = new ObservableCollection<ISeries>(statusSeriesList);

        // 2. Priority Chart (Pie)
        var prioritySeriesList = new List<ISeries>();
        foreach(var kvp in chartsData.CallsByPriority)
        {
            SKColor color = SKColor.Parse("#4CAF50");
            if (kvp.Key == "Экстренный") color = SKColor.Parse("#F44336");
            else if (kvp.Key == "Неотложный") color = SKColor.Parse("#FF9800");
            else if (kvp.Key == "Ложный") color = SKColor.Parse("#BDBDBD");

            prioritySeriesList.Add(new PieSeries<int> 
            { 
                Values = new int[] { kvp.Value }, 
                Name = $"{kvp.Key} ({kvp.Value})",
                Fill = new SolidColorPaint(color)
            });
        }
        PrioritySeries = new ObservableCollection<ISeries>(prioritySeriesList);

        // 3. Hourly Chart (Line)
        var hours = chartsData.CallsByHour.Keys.OrderBy(k => k).ToList();
        var hourlyValues = new List<int>();
        var hourlyLabels = new List<string>();

        foreach (var h in hours)
        {
            hourlyValues.Add(chartsData.CallsByHour[h]);
            hourlyLabels.Add($"{h:00}:00");
        }

        HourlySeries = new ObservableCollection<ISeries>
        {
            new LineSeries<int>
            {
                Values = hourlyValues,
                Name = "Заявки",
                Stroke = new SolidColorPaint(SKColor.Parse("#2196F3")) { StrokeThickness = 3 },
                Fill = null,
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#2196F3")) { StrokeThickness = 2 }
            }
        };

        HourlyXAxes = new ObservableCollection<LiveChartsCore.SkiaSharpView.Axis>
        {
            new LiveChartsCore.SkiaSharpView.Axis
            {
                Labels = hourlyLabels,
                LabelsRotation = 45
            }
        };

        // 4. Weekly Chart (Column/Bar)
        var weekDays = new string[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
        var weeklyTotalValues = new List<int>();
        var weeklyCompletedValues = new List<int>();

        foreach (var day in weekDays)
        {
            weeklyTotalValues.Add(chartsData.CallsByDayOfWeek[day].Total);
            weeklyCompletedValues.Add(chartsData.CallsByDayOfWeek[day].Completed);
        }

        WeeklySeries = new ObservableCollection<ISeries>
        {
            new ColumnSeries<int>
            {
                Values = weeklyTotalValues,
                Name = "Всего заявок",
                Fill = new SolidColorPaint(SKColor.Parse("#FF5252"))
            },
            new ColumnSeries<int>
            {
                Values = weeklyCompletedValues,
                Name = "Завершено",
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50"))
            }
        };

        WeeklyXAxes = new ObservableCollection<LiveChartsCore.SkiaSharpView.Axis>
        {
            new LiveChartsCore.SkiaSharpView.Axis
            {
                Labels = weekDays
            }
        };
    }

    public void LoadHourlyChart()
    {
        var chartsData = _dbService.GetChartsData(SelectedHourlyPeriod);

        var hours = chartsData.CallsByHour.Keys.OrderBy(k => k).ToList();
        var hourlyValues = new List<int>();
        var hourlyLabels = new List<string>();

        foreach (var h in hours)
        {
            hourlyValues.Add(chartsData.CallsByHour[h]);
            hourlyLabels.Add($"{h:00}:00");
        }

        HourlySeries = new ObservableCollection<ISeries>
        {
            new LineSeries<int>
            {
                Values = hourlyValues,
                Name = "Заявки",
                Stroke = new SolidColorPaint(SKColor.Parse("#2196F3")) { StrokeThickness = 3 },
                Fill = null,
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#2196F3")) { StrokeThickness = 2 }
            }
        };

        HourlyXAxes = new ObservableCollection<LiveChartsCore.SkiaSharpView.Axis>
        {
            new LiveChartsCore.SkiaSharpView.Axis
            {
                Labels = hourlyLabels,
                LabelsRotation = 45
            }
        };
    }
}