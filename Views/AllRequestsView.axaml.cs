using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ambulance.ViewModels;
using System;



namespace Ambulance.Views
{
    public partial class AllRequestsView : UserControl
    {
        private DateOnly? _selectedStartDate;
        private DateOnly? _selectedEndDate;

        public AllRequestsView()
        {
            InitializeComponent();

            // Инициализация ViewModel
            var viewModel = new AllRequestsViewModel();
            DataContext = viewModel;
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
                _selectedStartDate = null;
                _selectedEndDate = null;
            }
        }

        // Обработчик для выбора дат в календаре
        public void DateRangePickerCalendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            var vm = DataContext as AllRequestsViewModel;

            if (calendar != null && vm != null)
            {
                if (calendar.SelectedDates.Count > 0)
                {
                    // Если это первая дата (начало диапазона)
                    if (_selectedStartDate == null)
                    {
                        _selectedStartDate = DateOnly.FromDateTime(calendar.SelectedDates[0]);
                        vm.FilterDateStart = _selectedStartDate.Value.ToDateTime(TimeOnly.MinValue);
                    }
                    // Если это вторая дата (конец диапазона)
                    else if (_selectedEndDate == null)
                    {
                        var selectedDate = DateOnly.FromDateTime(calendar.SelectedDates[calendar.SelectedDates.Count - 1]);

                        // Убедимся, что дата конца после даты начала
                        if (selectedDate >= _selectedStartDate.Value)
                        {
                            _selectedEndDate = selectedDate;
                            vm.FilterDateEnd = _selectedEndDate.Value.ToDateTime(TimeOnly.MaxValue);
                            vm.FilterDateRangeDisplay = $"{_selectedStartDate:dd.MM.yyyy} - {_selectedEndDate:dd.MM.yyyy}";
                        }
                        else
                        {
                            // Если выбранная дата раньше, то это новое начало
                            _selectedStartDate = selectedDate;
                            _selectedEndDate = null;
                            vm.FilterDateStart = _selectedStartDate.Value.ToDateTime(TimeOnly.MinValue);
                            vm.FilterDateEnd = null;
                            vm.FilterDateRangeDisplay = $"{_selectedStartDate:dd.MM.yyyy} (выберите конец)";
                        }
                    }
                    // Если оба значения выбраны, начинаем заново
                    else
                    {
                        _selectedStartDate = DateOnly.FromDateTime(calendar.SelectedDates[0]);
                        _selectedEndDate = null;
                        vm.FilterDateStart = _selectedStartDate.Value.ToDateTime(TimeOnly.MinValue);
                        vm.FilterDateEnd = null;
                        vm.FilterDateRangeDisplay = $"{_selectedStartDate:dd.MM.yyyy} (выберите конец)";
                    }
                }
            }
        }
    }
}