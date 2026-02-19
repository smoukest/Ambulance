using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Ambulance.ViewModels;
using ReactiveUI;
using System;
using System.Globalization;



namespace Ambulance.Views
{
    public partial class AllRequestsView : UserControl
    {
        private DateOnly? _firstSelectedDate;

        // Встроенный конвертер для приоритета в цвет
        public static readonly PriorityToColorConverter PriorityColorConverter = new();

        public AllRequestsView()
        {
            InitializeComponent();

            // Инициализация ViewModel
            var viewModel = new AllRequestsViewModel();
            DataContext = viewModel;

            this.Loaded += (sender, args) =>
            {
                // Находим календарь и подписываемся на двойной клик
                var calendar = this.FindControl<Avalonia.Controls.Calendar>("DateRangePickerCalendar");
                if (calendar != null)
                {
                    calendar.DoubleTapped += (s, e) =>
                    {
                        HandleDateDoubleClick(calendar, viewModel);
                    };
                }
            };
        }

        private void HandleDateDoubleClick(Avalonia.Controls.Calendar calendar, AllRequestsViewModel vm)
        {
            if (calendar.SelectedDate.HasValue)
            {
                var selectedDate = DateOnly.FromDateTime(calendar.SelectedDate.Value);

                // Если это первая дата (начало диапазона)
                if (_firstSelectedDate == null)
                {
                    _firstSelectedDate = selectedDate;
                    vm.FilterDateStart = _firstSelectedDate.Value.ToDateTime(TimeOnly.MinValue);
                    vm.FilterDateRangeDisplay = $"{_firstSelectedDate:dd.MM.yyyy} (выберите конец периода)";

                    // Обновляем визуальный индикатор первой даты
                    UpdateFirstDateIndicator(_firstSelectedDate.Value);
                }
                // Если это вторая дата (конец диапазона)
                else
                {
                    var secondDate = selectedDate;
                    DateOnly startDate, endDate;

                    // Убедимся, чтобы начало было раньше конца
                    if (secondDate >= _firstSelectedDate.Value)
                    {
                        startDate = _firstSelectedDate.Value;
                        endDate = secondDate;
                    }
                    else
                    {
                        startDate = secondDate;
                        endDate = _firstSelectedDate.Value;
                    }

                    // Устанавливаем даты
                    vm.FilterDateStart = startDate.ToDateTime(TimeOnly.MinValue);
                    vm.FilterDateEnd = endDate.ToDateTime(TimeOnly.MaxValue);
                    vm.FilterDateRangeDisplay = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";

                    // Обновляем визуальные индикаторы
                    UpdateSecondDateIndicator(secondDate);

                    // Сбрасываем временное значение
                    _firstSelectedDate = null;
                    calendar.SelectedDate = null;
                }
            }
        }

        private void UpdateFirstDateIndicator(DateOnly date)
        {
            var indicator = this.FindControl<Border>("FirstDateIndicator");
            var dateText = this.FindControl<TextBlock>("FirstDateText");

            if (indicator != null && dateText != null)
            {
                indicator.IsVisible = true;
                dateText.Text = date.ToString("dd.MM.yyyy");
            }
        }

        private void UpdateSecondDateIndicator(DateOnly date)
        {
            var indicator = this.FindControl<Border>("SecondDateIndicator");
            var dateText = this.FindControl<TextBlock>("SecondDateText");

            if (indicator != null && dateText != null)
            {
                indicator.IsVisible = true;
                dateText.Text = date.ToString("dd.MM.yyyy");
            }
        }

        private void ClearDateIndicators()
        {
            var firstIndicator = this.FindControl<Border>("FirstDateIndicator");
            var secondIndicator = this.FindControl<Border>("SecondDateIndicator");

            if (firstIndicator != null)
                firstIndicator.IsVisible = false;

            if (secondIndicator != null)
                secondIndicator.IsVisible = false;
        }

        // Обработчик для кнопки поиска
        public void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            var vm = DataContext as AllRequestsViewModel;
            if (vm != null)
            {
                // Запустить обновление с текущими фильтрами
                vm.RefreshRequests();
            }
        }

        // Обработчик для кнопки фильтра пациента
        public void TogglePatientFilter_Click(object? sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("PatientFilterPopup");
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        // Обработчик для кнопки фильтра приоритета
        public void TogglePriorityFilter_Click(object? sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("PriorityFilterPopup");
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        // Обработчик для кнопки фильтра статуса
        public void ToggleStatusFilter_Click(object? sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("StatusFilterPopup");
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        // Обработчик для кнопки фильтра даты
        public void ToggleDateRangeFilter_Click(object? sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("DateRangeFilterPopup");
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        // Обработчик для очистки диапазона дат
        public void ClearDateRange_Click(object? sender, RoutedEventArgs e)
        {
            var vm = DataContext as AllRequestsViewModel;
            if (vm != null)
            {
                vm.FilterDateStart = null;
                vm.FilterDateEnd = null;
                vm.FilterDateRangeDisplay = "Выберите период";
                _firstSelectedDate = null;

                // Очищаем визуальные индикаторы
                ClearDateIndicators();

                // Очищаем выбор в календаре
                var calendar = this.FindControl<Avalonia.Controls.Calendar>("DateRangePickerCalendar");
                if (calendar != null)
                {
                    calendar.SelectedDate = null;
                }
            }
        }

        /// <summary>
        /// Получает hex-код цвета для приоритета по схеме:
        /// Экстренный - красный (#F44336)
        /// Неотложный - жёлтый (#FF9800)
        /// Ложный - светло-серый (#BDBDBD)
        /// Другие приоритеты - зелёный (#4CAF50)
        /// </summary>
        private string GetPriorityColor(string priority)
        {
            return priority switch
            {
                "Экстренный" => "#F44336",      // Красный
                "Неотложный" => "#FF9800",      // Жёлтый/оранжевый
                "Ложный" => "#BDBDBD",          // Светло-серый
                _ => "#4CAF50"                  // Зелёный для других
            };
        }
    }

    /// <summary>
    /// Встроенный конвертер для преобразования приоритета в цвет
    /// </summary>
    public class PriorityToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string priority)
            {
                string colorCode = priority switch
                {
                    "Экстренный" => "#F44336",      // Красный
                    "Неотложный" => "#FF9800",      // Жёлтый/оранжевый
                    "Ложный" => "#BDBDBD",          // Светло-серый
                    _ => "#4CAF50"                  // Зелёный для других
                };

                return new SolidColorBrush(Color.Parse(colorCode));
            }

            return new SolidColorBrush(Color.Parse("#4CAF50"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}