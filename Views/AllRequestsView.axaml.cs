using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ambulance.Views;

public partial class AllRequestsView : UserControl
{
    DatabaseService _dt;
    string connectionString = "Server=localhost;Port=5432;Username=postgres;Password=123;Database=amb;";

    // Добавлены поля для хранения фильтров
    private readonly List<CheckBox> _priorityChecks = new();
    private readonly List<CheckBox> _statusChecks = new();
    private Flyout? _priorityFlyout;
    private Flyout? _statusFlyout;

    public AllRequestsView()
    {
        InitializeComponent();
        _dt = new DatabaseService(connectionString);
        InitializeFilters(); // Инициализация фильтров
    }

    private async void Search_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // 1. ОЧИСТИТЬ контейнер перед добавлением новых строк!
        CallsContainer.Children.Clear();

        // 2. Получить значения из фильтров (пустые поля = пустые строки)
        string name = "";
        string surname = "";
        string patronymic = "";

        // Разбор ФИО из одного поля
        if (!string.IsNullOrWhiteSpace(FilterPatient.Text))
        {
            var parts = FilterPatient.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0) surname = parts[0];
            if (parts.Length > 1) name = parts[1];
            if (parts.Length > 2) patronymic = parts[2];
        }

        string phone = FilterPhone.Text;
        string address = ""; // FilterAddress не существует в вашем XAML
        string email = "";
        string appealPurpose = "";
        string priority = GetSelectedPriorities().Any() ? GetSelectedPriorities()[0] : "";

        // 3. Выполнить запрос
        string[,] patients = _dt.GetAllPatient(
            name,
            surname,
            patronymic,
            phone,
            address,
            email,
            appealPurpose,
            priority
        );

        // 4. Добавить строки в контейнер
        for (int i = 0; i < patients.GetLength(1); i++)
        {
            // Проверка на конец данных (предполагаем, что пустой patient_id = конец)
            if (string.IsNullOrWhiteSpace(patients[i, 0]))
                break;

            // ВАЖНО: порядок параметров ДОЛЖЕН соответствовать сигнатуре CreateCallRow!
            var row = CreateCallRow(
                patients[i, 0],  // patientId
                patients[i, 1],  // name
                patients[i, 2],  // surname
                patients[i, 3],  // patronymic
                patients[i, 4],  // phone
                patients[i, 5],  // address
                patients[i, 6],  // email
                patients[i, 7],  // anamnesis
                patients[i, 8],  // complaints
                patients[i, 9],  // appealPurpose
                patients[i, 10], // priority
                patients[i, 11], // callId
                patients[i, 12], // time
                patients[i, 13], // status
                patients[i, 14]  // visitId
            );

            // КРИТИЧЕСКИ ВАЖНО: добавить строку в контейнер!
            CallsContainer.Children.Add(row);
        }
    }

    private Border CreateCallRow(
            string patientId , string name, string surname, string patronymic,
            string phone, string address, string email, string anamnesis,
            string complaints, string appealPurpose, string priority,
            string callId, string time, string status, string visitId)
    {
        // Создание раскрывающегося блока
        var expander = new Expander
        {
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };

        // Header - краткая информация
        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100)));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(150)));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100)));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100)));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100)));
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100)));

        // № Заявки
        var idBlock = new TextBlock
        {
            Text = callId,
            Foreground = new SolidColorBrush(Color.Parse("#FF5252")),
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(10, 8, 10, 8)
        };
        headerGrid.Children.Add(idBlock);

        // Пациент (ФИО + адрес)
        var patientPanel = new StackPanel { Margin = new Thickness(10, 5, 10, 5) };
        patientPanel.Children.Add(new TextBlock
        {
            Text = $"{surname} {name} {patronymic}",
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.Black
        });
        patientPanel.Children.Add(new TextBlock
        {
            Text = address,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#666"))
        });
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn вместо прямого свойства
        Grid.SetColumn(patientPanel, 1);
        headerGrid.Children.Add(patientPanel);

        // Телефон
        var phoneBlock = new TextBlock
        {
            Text = phone,
            Foreground = Brushes.Black,
            Margin = new Thickness(10, 8, 10, 8)
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn вместо прямого свойства
        Grid.SetColumn(phoneBlock, 2);
        headerGrid.Children.Add(phoneBlock);

        // Приоритет (цветная метка)
        var priorityColor = GetPriorityColor(priority);
        var priorityBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse(priorityColor)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(5, 2),
            Margin = new Thickness(10, 5, 10, 5),
            Child = new TextBlock
            {
                Text = priority,
                Foreground = Brushes.White,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                FontSize = 12
            }
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn вместо прямого свойства
        Grid.SetColumn(priorityBorder, 3);
        headerGrid.Children.Add(priorityBorder);

        // Статус (цветная метка)
        var statusColor = GetStatusColor(status);
        var statusBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse(statusColor)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(5, 2),
            Margin = new Thickness(10, 5, 10, 5),
            Child = new TextBlock
            {
                Text = status,
                Foreground = Brushes.White,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                FontSize = 12
            }
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn вместо прямого свойства
        Grid.SetColumn(statusBorder, 4);
        headerGrid.Children.Add(statusBorder);

        // Время
        var timeBlock = new TextBlock
        {
            Text = DateTime.Parse(time).ToString("HH:mm dd.MM"),
            Foreground = Brushes.Black,
            Margin = new Thickness(10, 8, 10, 8)
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn вместо прямого свойства
        Grid.SetColumn(timeBlock, 5);
        headerGrid.Children.Add(timeBlock);

        // Бригада
        var brigadeBlock = new TextBlock
        {
            Text = !string.IsNullOrEmpty(callId) ? $"Бригада №{callId}" : "—",
            Foreground = Brushes.Black,
            Margin = new Thickness(10, 8, 10, 8)
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn вместо прямого свойства
        Grid.SetColumn(brigadeBlock, 6);
        headerGrid.Children.Add(brigadeBlock);

        expander.Header = headerGrid;

        // Content - детальная информация
        var detailsGrid = new Grid();
        // ИСПРАВЛЕНИЕ: Используем GridLength.Auto вместо AutoGridLength
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
        // ИСПРАВЛЕНИЕ: Используем GridLength.Auto вместо AutoGridLength
        detailsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        detailsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        detailsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        detailsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        // Email
        var emailLabel = new TextBlock
        {
            Text = "Email:",
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#333")),
            Margin = new Thickness(0, 0, 10, 0)
        };
        detailsGrid.Children.Add(emailLabel);

        var emailValue = new TextBlock
        {
            Text = email,
            Foreground = new SolidColorBrush(Color.Parse("#555"))
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn вместо прямого свойства
        Grid.SetColumn(emailValue, 1);
        detailsGrid.Children.Add(emailValue);

        // Цель обращения
        var purposeLabel = new TextBlock
        {
            Text = "Цель обращения:",
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#333")),
            Margin = new Thickness(0, 10, 10, 0)
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetRow вместо прямого свойства
        Grid.SetRow(purposeLabel, 1);
        detailsGrid.Children.Add(purposeLabel);

        var purposeValue = new TextBlock
        {
            Text = appealPurpose,
            Foreground = new SolidColorBrush(Color.Parse("#555")),
            Margin = new Thickness(0, 10, 0, 0)
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn и Grid.SetRow вместо прямых свойств
        Grid.SetColumn(purposeValue, 1);
        Grid.SetRow(purposeValue, 1);
        detailsGrid.Children.Add(purposeValue);

        // Анамнез
        var anamnesisLabel = new TextBlock
        {
            Text = "Анамнез:",
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#333")),
            Margin = new Thickness(0, 10, 10, 0)
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetRow вместо прямого свойства
        Grid.SetRow(anamnesisLabel, 2);
        detailsGrid.Children.Add(anamnesisLabel);

        var anamnesisValue = new TextBlock
        {
            Text = anamnesis,
            Foreground = new SolidColorBrush(Color.Parse("#555")),
            Margin = new Thickness(0, 10, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn и Grid.SetRow вместо прямых свойств
        Grid.SetColumn(anamnesisValue, 1);
        Grid.SetRow(anamnesisValue, 2);
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumnSpan вместо прямого свойства
        Grid.SetColumnSpan(anamnesisValue, 3);
        detailsGrid.Children.Add(anamnesisValue);

        // Жалобы
        var complaintsLabel = new TextBlock
        {
            Text = "Жалобы:",
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#333")),
            Margin = new Thickness(0, 10, 10, 0)
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetRow вместо прямого свойства
        Grid.SetRow(complaintsLabel, 3);
        detailsGrid.Children.Add(complaintsLabel);

        var complaintsValue = new TextBlock
        {
            Text = complaints,
            Foreground = new SolidColorBrush(Color.Parse("#555")),
            Margin = new Thickness(0, 10, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumn и Grid.SetRow вместо прямых свойств
        Grid.SetColumn(complaintsValue, 1);
        Grid.SetRow(complaintsValue, 3);
        // ИСПРАВЛЕНИЕ: Используем Grid.SetColumnSpan вместо прямого свойства
        Grid.SetColumnSpan(complaintsValue, 3);
        detailsGrid.Children.Add(complaintsValue);

        var detailsBorder = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(0, 0, 4, 4),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 5, 0, 0),
            Child = detailsGrid
        };

        expander.Content = detailsBorder;

        // Обёртка Border
        return new Border
        {
            Background = new SolidColorBrush(Color.Parse("#ecfff8")),
            Margin = new Thickness(0, 5, 0, 5),
            CornerRadius = new CornerRadius(4),
            Child = expander
        };
    }

    // Получение цвета для приоритета
    private string GetPriorityColor(string priority)
    {
        return priority switch
        {
            "Экстренный" => "#F44336",
            "Неотложный" => "#FF9800",
            "Плановый" => "#4CAF50",
            "Низкий" => "#9E9E9E",
            _ => "#9E9E9E"
        };
    }

    // Получение цвета для статуса
    private string GetStatusColor(string status)
    {
        return status switch
        {
            "Новая" => "#2196F3",
            "В работе" => "#FF9800",
            "Ожидание" => "#9C27B0",
            "Завершена" => "#4CAF50",
            "Отменена" => "#F44336",
            _ => "#9E9E9E"
        };
    }

    // Инициализация фильтров
    private void InitializeFilters()
    {
        // Приоритеты
        var priorityPanel = new StackPanel { Margin = new Thickness(10) };
        var priorities = new[] { "Экстренный", "Неотложный", "Плановый", "Низкий" };

        foreach (var priority in priorities)
        {
            var cb = new CheckBox
            {
                Content = priority,
                Margin = new Thickness(0, 4, 0, 4),
                Tag = priority
            };
            cb.Checked += PriorityFilter_Changed;
            cb.Unchecked += PriorityFilter_Changed;
            priorityPanel.Children.Add(cb);
            _priorityChecks.Add(cb);
        }

        // Кнопка "Применить" для приоритетов
        var applyPriorityBtn = new Button
        {
            Content = "Готово",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0),
            Width = 80
        };
        applyPriorityBtn.Click += (s, e) => _priorityFlyout?.Hide();
        priorityPanel.Children.Add(applyPriorityBtn);

        // Создаем Flyout и привязываем к кнопке
        _priorityFlyout = new Flyout
        {
            Content = priorityPanel,
            Placement = PlacementMode.Bottom,
            ShowMode = FlyoutShowMode.Standard
        };
        // ИСПРАВЛЕНИЕ: Используем свойство Flyout для привязки
        PriorityFilterButton.Flyout = _priorityFlyout;

        // Статусы
        var statusPanel = new StackPanel { Margin = new Thickness(10) };
        var statuses = new[] { "Новая", "В работе", "Ожидание", "Завершена", "Отменена" };

        foreach (var status in statuses)
        {
            var cb = new CheckBox
            {
                Content = status,
                Margin = new Thickness(0, 4, 0, 4),
                Tag = status
            };
            cb.Checked += StatusFilter_Changed;
            cb.Unchecked += StatusFilter_Changed;
            statusPanel.Children.Add(cb);
            _statusChecks.Add(cb);
        }

        // Кнопка "Применить" для статусов
        var applyStatusBtn = new Button
        {
            Content = "Готово",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0),
            Width = 80
        };
        applyStatusBtn.Click += (s, e) => _statusFlyout?.Hide();
        statusPanel.Children.Add(applyStatusBtn);

        _statusFlyout = new Flyout
        {
            Content = statusPanel,
            Placement = PlacementMode.Bottom,
            ShowMode = FlyoutShowMode.Standard
        };
        // ИСПРАВЛЕНИЕ: Используем свойство Flyout для привязки
        StatusFilterButton.Flyout = _statusFlyout;
    }

    // Открытие фильтра при клике на кнопку
    // Исправление для ошибки CS1061: "Flyout не содержит определение для 'Show'"
    private void PriorityFilterButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_priorityFlyout != null)
        {
            _priorityFlyout.SetValue(FlyoutBase.IsOpenProperty, true);
        }
    }

    private void StatusFilterButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_statusFlyout != null)
        {
            _statusFlyout.SetValue(FlyoutBase.IsOpenProperty, true);
        }
    }

    // Обновление заголовка кнопки приоритета
    private void PriorityFilter_Changed(object? sender, RoutedEventArgs e)
    {
        var selected = _priorityChecks.Where(cb => cb.IsChecked == true).Count();
        PriorityFilterButton.Content = selected > 0
            ? $"Приоритет ({selected})"
            : "Приоритет";
    }

    // Обновление заголовка кнопки статуса
    private void StatusFilter_Changed(object? sender, RoutedEventArgs e)
    {
        var selected = _statusChecks.Where(cb => cb.IsChecked == true).Count();
        StatusFilterButton.Content = selected > 0
            ? $"Статус ({selected})"
            : "Статус";
    }

    // Получение выбранных приоритетов
    public List<string> GetSelectedPriorities()
    {
        return _priorityChecks
            .Where(cb => cb.IsChecked == true)
            .Select(cb => cb.Tag?.ToString())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    // Получение выбранных статусов
    public List<string> GetSelectedStatuses()
    {
        return _statusChecks
            .Where(cb => cb.IsChecked == true)
            .Select(cb => cb.Tag?.ToString())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }
}