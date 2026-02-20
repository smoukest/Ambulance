using Ambulance.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Ambulance.ViewModels
{
    public class AllRequestsViewModel : ReactiveObject
    {
        private bool _isRefreshing = false;
        private bool _isInitialized = false;

        static string connectionString = "Server=localhost;Port=5432;Username=postgres;Password=123;Database=amb;";
        private DatabaseService _dt = new DatabaseService(connectionString);

        // Реактивные свойства фильтров
        [Reactive]
        public string FilterRequestNumber { get; set; } = "";

        [Reactive]
        public string FilterPatient { get; set; } = "";

        [Reactive]
        public string FilterPhone { get; set; } = "";

        [Reactive]
        public string FilterAddress { get; set; } = "";

        [Reactive]
        public string FilterEmail { get; set; } = "";

        [Reactive]
        public string FilterGender { get; set; } = "";

        [Reactive]
        public string FilterAppealPurpose { get; set; } = "";

        [Reactive]
        public DateTime? FilterDate { get; set; }

        [Reactive]
        public DateTime? FilterDateStart { get; set; }

        [Reactive]
        public DateTime? FilterDateEnd { get; set; }

        [Reactive]
        public string FilterDateRangeDisplay { get; set; } = "Выберите период";

        [Reactive]
        public string FilterDateStartDisplay { get; set; } = "";

        [Reactive]
        public string FilterDateEndDisplay { get; set; } = "";

        [Reactive]
        public string FilterBrigade { get; set; } = "Все";

        // Коллекции для выбранных фильтров
        [Reactive]
        public ObservableCollection<FilterItem> AvailablePriorities { get; set; }

        [Reactive]
        public ObservableCollection<FilterItem> AvailableStatuses { get; set; }

        [Reactive]
        public ObservableCollection<FilterItem> SelectedPriorities { get; set; }

        [Reactive]
        public ObservableCollection<FilterItem> SelectedStatuses { get; set; }

        // Данные таблицы
        [Reactive]
        public ObservableCollection<RequestRow> RequestRows { get; set; }

        [Reactive]
        public bool IsLoading { get; set; } = false;

        public AllRequestsViewModel()
        {
            // Инициализируем коллекции фильтров
            AvailablePriorities = new ObservableCollection<FilterItem>
            {
                new FilterItem { Name = "Экстренный", IsSelected = false },
                new FilterItem { Name = "Неотложный", IsSelected = false },
                new FilterItem { Name = "Ложный", IsSelected = false }
            };

            AvailableStatuses = new ObservableCollection<FilterItem>
            {
                new FilterItem { Name = "В работе", IsSelected = false },
                new FilterItem { Name = "Завершена", IsSelected = false },
                new FilterItem { Name = "Отменена", IsSelected = false }
            };

            SelectedPriorities = new ObservableCollection<FilterItem>();
            SelectedStatuses = new ObservableCollection<FilterItem>();
            RequestRows = new ObservableCollection<RequestRow>();

            // Подписываемся на изменения фильтров для автообновления
            // Используем DistinctUntilChanged для игнорирования одинаковых значений
            this.WhenAnyValue(
                vm => vm.FilterRequestNumber,
                vm => vm.FilterPatient,
                vm => vm.FilterPhone,
                vm => vm.FilterAppealPurpose,
                vm => vm.FilterDateStart,
                vm => vm.FilterDateEnd,
                vm => vm.FilterBrigade
            )
            .DistinctUntilChanged()
            .Skip(1) // Пропускаем первоначальное значение при инициализации
            .Throttle(TimeSpan.FromMilliseconds(500)) // Ждём 500мс после последнего изменения
            .Subscribe(_ => 
            {
                if (_isInitialized)
                {
                    RefreshRequests();
                }
            });

            // Следим за изменениями в выбранных приоритетах
            AvailablePriorities.CollectionChanged += (s, e) => 
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace && _isInitialized)
                {
                    RefreshRequests();
                }
            };

            // Следим за изменениями в выбранных статусах
            AvailableStatuses.CollectionChanged += (s, e) => 
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace && _isInitialized)
                {
                    RefreshRequests();
                }
            };

            // Загружаем первоначальный список
            RefreshRequests();

            // Помечаем, что инициализация завершена
            _isInitialized = true;
        }

        /// <summary>
        /// Обновляет список заявок на основе текущих фильтров
        /// </summary>
        public void RefreshRequests()
        {
            try
            {
                IsLoading = true;

                // Разбираем ФИО
                string name = "";
                string surname = "";
                string patronymic = "";
                string address = "";
                string email = "";

                if (!string.IsNullOrWhiteSpace(FilterPatient))
                {
                    var parts = FilterPatient.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0) surname = parts[0];
                    if (parts.Length > 1) name = parts[1];
                    if (parts.Length > 2) patronymic = parts[2];
                }

                // Получаем все выбранные приоритеты
                var selectedPriorities = AvailablePriorities
                    .Where(p => p.IsSelected)
                    .Select(p => p.Name)
                    .ToList();

                // Получаем все выбранные статусы
                var selectedStatuses = AvailableStatuses
                    .Where(s => s.IsSelected)
                    .Select(s => s.Name)
                    .ToList();

                // Запрашиваем данные из БД с ВСЕ фильтрами
                string[,] patients = _dt.GetAllPatient(
                    name,                              // name
                    surname,                           // surname
                    patronymic,                        // patronymic
                    FilterPhone,                       // phone number
                    FilterAddress,                     // address
                    FilterEmail,                       // email
                    FilterAppealPurpose,               // appeal purpose
                    selectedPriorities,                // priorities (multiple)
                    FilterRequestNumber,               // call id
                    selectedStatuses,                  // selected statuses
                    FilterGender,                      // gender
                    FilterDateStart,                   // date start
                    FilterDateEnd                      // date end
                );

                System.Diagnostics.Debug.WriteLine($"GetAllPatient вернул массив: {patients?.Length ?? 0} элементов");

                // Очищаем текущие строки перед добавлением новых
                RequestRows.Clear();

                // Проверяем, что массив не пуст и имеет элементы
                if (patients == null || patients.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Массив пуст или null");
                    IsLoading = false;
                    return;
                }

                // Добавляем новые строки
                int rowCount = patients.GetLength(0);
                int colCount = patients.GetLength(1);

                System.Diagnostics.Debug.WriteLine($"Размер массива: {rowCount} rows x {colCount} cols");

                // Проходим по каждой строке
                for (int i = 0; i < rowCount; i++)
                {
                    // Проверяем, не пуста ли строка
                    if (string.IsNullOrWhiteSpace(patients[i, 0]))
                        continue;

                    System.Diagnostics.Debug.WriteLine($"Обработка строки {i}: PatientID={patients[i, 0]}");

                    var row = new RequestRow
                    {
                        PatientId = colCount > 0 ? patients[i, 0] : "",
                        Name = colCount > 1 ? patients[i, 1] : "",
                        Surname = colCount > 2 ? patients[i, 2] : "",
                        Patronymic = colCount > 3 ? patients[i, 3] : "",
                        Phone = colCount > 4 ? patients[i, 4] : "",
                        Address = colCount > 5 ? patients[i, 5] : "",
                        Email = colCount > 6 ? patients[i, 6] : "",
                        Anamnesis = colCount > 7 ? patients[i, 7] : "",
                        Complaints = colCount > 8 ? patients[i, 8] : "",
                        AppealPurpose = colCount > 9 ? patients[i, 9] : "",
                        Priority = colCount > 10 ? patients[i, 10] : "",
                        CallId = colCount > 11 ? patients[i, 11] : "",
                        Time = colCount > 12 ? patients[i, 12] : "",
                        Status = colCount > 13 ? patients[i, 13] : "",
                        BirthDate = colCount > 14 ? patients[i, 14] : "",
                        Gender = colCount > 15 ? patients[i, 15] : "",
                        VisitId = ""
                    };

                    RequestRows.Add(row);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении списка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Получает выбранные приоритеты
        /// </summary>
        public List<string> GetSelectedPriorities()
        {
            return AvailablePriorities
                .Where(p => p.IsSelected)
                .Select(p => p.Name)
                .ToList();
        }

        /// <summary>
        /// Получает выбранные статусы
        /// </summary>
        public List<string> GetSelectedStatuses()
        {
            return AvailableStatuses
                .Where(s => s.IsSelected)
                .Select(s => s.Name)
                .ToList();
        }
    }

    /// <summary>
    /// Модель элемента фильтра
    /// </summary>
    public class FilterItem : ReactiveObject
    {
        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Модель строки таблицы заявок
    /// </summary>
    public class RequestRow : ReactiveObject
    {
        [Reactive]
        public string PatientId { get; set; }

        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public string Surname { get; set; }

        [Reactive]
        public string Patronymic { get; set; }

        [Reactive]
        public string Phone { get; set; }

        [Reactive]
        public string Address { get; set; }

        [Reactive]
        public string Email { get; set; }

        [Reactive]
        public string Anamnesis { get; set; }

        [Reactive]
        public string Complaints { get; set; }

        [Reactive]
        public string AppealPurpose { get; set; }

        [Reactive]
        public string Priority { get; set; }

        [Reactive]
        public string CallId { get; set; }

        [Reactive]
        public string Time { get; set; }

        [Reactive]
        public string Status { get; set; }

        [Reactive]
        public string BirthDate { get; set; }

        [Reactive]
        public string Gender { get; set; }

        [Reactive]
        public string VisitId { get; set; }

        public string FullName => $"{Surname} {Name} {Patronymic}".Trim();
    }
}
        