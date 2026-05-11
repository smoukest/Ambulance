using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;

namespace Ambulance.Views;

public partial class RegistrationView : UserControl
{
    DatabaseService _dt;
    string connectionString = "Server=localhost;Port=5432;Username=postgres;Password=123;Database=amb;";
    private int? _pendingPatientId;
    private string _pendingAppealPurpose = "";
    private string _pendingComplaints = "";

    public RegistrationView()
    {
        InitializeComponent();
        _dt = new DatabaseService(connectionString);
        FillWithRandomData();
    }

    private void FillWithRandomData_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        FillWithRandomData();
    }

    private void FillWithRandomData()
    {
        var random = new Random();

        string[] maleNames = { "Иван", "Александр", "Дмитрий", "Сергей", "Андрей", "Алексей", "Максим", "Владимир", "Евгений", "Михаил" };
        string[] femaleNames = { "Анна", "Елена", "Ольга", "Наталья", "Екатерина", "Мария", "Светлана", "Татьяна", "Юлия", "Ирина" };
        string[] maleSurnames = { "Иванов", "Смирнов", "Кузнецов", "Попов", "Васильев", "Петров", "Соколов", "Михайлов", "Новиков", "Фёдоров" };
        string[] femaleSurnames = { "Иванова", "Смирнова", "Кузнецова", "Попова", "Васильева", "Петрова", "Соколова", "Михайлова", "Новикова", "Фёдорова" };
        string[] malePatronymics = { "Иванович", "Александрович", "Дмитриевич", "Сергеевич", "Андреевич", "Алексеевич", "Максимович", "Владимирович" };
        string[] femalePatronymics = { "Ивановна", "Александровна", "Дмитриевна", "Сергеевна", "Андреевна", "Алексеевна", "Максимовна", "Владимировна" };

        string[] streets = { "Ленина", "Пушкина", "Гагарина", "Кирова", "Советская", "Мира", "Садовая", "Новая", "Строителей", "Молодежная" };
        string[] anamneses = { "Хронических заболеваний нет", "Гипертония", "Сахарный диабет 2 типа", "Аллергия на пенициллин", "Бронхиальная астма", "Операция по удалению аппендицита в 2010", "Без особенностей", "ИБС, стенокардия напряжения" };
        string[] complaintsList = { "Головная боль, головокружение", "Боль в груди, отдающая в левую руку", "Высокая температура (39.5), кашель", "Резкая боль в животе", "Травма ноги, подозрение на перелом", "Острая аллергическая реакция, сыпь", "Удушье, нехватка воздуха", "Потеря сознания" };

        bool isMale = random.Next(2) == 0;

        Name.Text = isMale ? maleNames[random.Next(maleNames.Length)] : femaleNames[random.Next(femaleNames.Length)];
        Surname.Text = isMale ? maleSurnames[random.Next(maleSurnames.Length)] : femaleSurnames[random.Next(femaleSurnames.Length)];
        Patronymic.Text = isMale ? malePatronymics[random.Next(malePatronymics.Length)] : femalePatronymics[random.Next(femalePatronymics.Length)];

        // Генерация случайного номера телефона в формате +7 (9XX) XXX-XX-XX
        PhoneNumber.Text = $"+7 (9{random.Next(10, 100)}) {random.Next(100, 1000)}-{random.Next(10, 100)}-{random.Next(10, 100)}";

        // Генерация email на основе имени и фамилии
        string englishName = Transliterate(Name.Text.ToLower());
        string englishSurname = Transliterate(Surname.Text.ToLower());
        Email.Text = $"{englishName}.{englishSurname}{random.Next(1950, 2010)}@email.com";

        // Генерация случайного адреса
        Address.Text = $"г. Москва, ул. {streets[random.Next(streets.Length)]}, д. {random.Next(1, 150)}, кв. {random.Next(1, 400)}";

        // Генерация случайной даты рождения
        int year = random.Next(1940, 2010);
        int month = random.Next(1, 13);
        int day = random.Next(1, 29); // Simplified days
        BirthDate.Text = $"{day:D2}.{month:D2}.{year}";

        Anamnesis.Text = anamneses[random.Next(anamneses.Length)];
        Complaints.Text = complaintsList[random.Next(complaintsList.Length)];

        // Выбор пола
        Gender.SelectedIndex = isMale ? 0 : 1;

        // Выбор случайной цели обращения
        AppealPurpose.SelectedIndex = random.Next(3);
    }

    private string Transliterate(string text)
    {
        var dict = new Dictionary<char, string>
        {
            {'а',"a"}, {'б',"b"}, {'в',"v"}, {'г',"g"}, {'д',"d"}, {'е',"e"}, {'ё',"yo"}, {'ж',"zh"},
            {'з',"z"}, {'и',"i"}, {'й',"y"}, {'к',"k"}, {'л',"l"}, {'м',"m"}, {'н',"n"}, {'о',"o"},
            {'п',"p"}, {'р',"r"}, {'с',"s"}, {'т',"t"}, {'у',"u"}, {'ф',"f"}, {'х',"x"}, {'ц',"c"},
            {'ч',"ch"}, {'ш',"sh"}, {'щ',"shh"}, {'ъ',"w"}, {'ы',"y"}, {'ь',""}, {'э',"e"}, {'ю',"yu"}, {'я',"ya"}
        };

        string result = "";
        foreach(char c in text)
        {
            if(dict.ContainsKey(c)) result += dict[c];
            else result += c;
        }
        return result;
    }

    public void SetAddress(string address)
    {
        Address.Text = address;
    }

    private void ShowBrigadeAssignmentView()
    {
        BrigadeAssignmentView.IsVisible = true;
    }

    private void HideBrigadeAssignmentView()
    {
        BrigadeAssignmentView.IsVisible = false;
        BrigadeComboBox.SelectedIndex = -1;
        _pendingPatientId = null;
        _pendingAppealPurpose = "";
        _pendingComplaints = "";
    }

    private void Clear_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Name.Text = "";
        Surname.Text = "";
        Patronymic.Text = "";
        PhoneNumber.Text = "";
        Email.Text = "";
        Address.Text = "";
        Anamnesis.Text = "";
        Complaints.Text = "";
        BirthDate.Text = "";
        Gender.SelectedIndex = -1;
        AppealPurpose.SelectedIndex = -1;
        HideBrigadeAssignmentView();
    }

    private void Registration_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            string name = Name.Text;
            string surname = Surname.Text;
            string patronymic = Patronymic.Text;
            string phoneNumber = PhoneNumber.Text;
            string email = Email.Text;
            string address = Address.Text;
            string birthDate = BirthDate.Text;
            string gender = "";
            var sel = Gender.SelectedItem as ComboBoxItem;
            if (sel != null) gender = sel.Content?.ToString() ?? "";
            string anamnesis = Anamnesis.Text;
            string complaints = Complaints.Text;
            if (AppealPurpose.SelectedIndex == -1)
                throw new Exception("Пожалуйста, укажите цель поступившего звонка");
            var selectedAppealPurpose = AppealPurpose.SelectedItem as ComboBoxItem;
            string appealPurpose = selectedAppealPurpose?.Content?.ToString() ?? "";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(patronymic) || string.IsNullOrEmpty(phoneNumber)
                 || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(anamnesis) || string.IsNullOrEmpty(complaints))
                throw new Exception("Пожалуйста, заполните все поля ввода");
            var patientId = _dt.CreatePatient(name, surname, patronymic, phoneNumber, address, email, anamnesis, birthDate, gender);

            if (appealPurpose == "Выезд")
            {
                _pendingPatientId = patientId;
                _pendingAppealPurpose = appealPurpose;
                _pendingComplaints = complaints;
                ShowBrigadeAssignmentView();
            }
            else
            {
                _dt.CreateCall(patientId, appealPurpose, complaints);
                HideBrigadeAssignmentView();
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex);
            return;
        }
    }

    private async void AssignBrigade_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_pendingPatientId == null)
        {
            ShowMessage(new Exception("Сначала зарегистрируйте заявку"));
            return;
        }

        if (BrigadeComboBox.SelectedItem is not ComboBoxItem selectedBrigade)
        {
            ShowMessage(new Exception("Выберите бригаду для выезда"));
            return;
        }

        _dt.CreateCall(_pendingPatientId.Value, _pendingAppealPurpose, _pendingComplaints);

        var messageBox = MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams
            {
                ContentTitle = "Бригада назначена",
                ContentMessage = $"Назначена {selectedBrigade.Content}",
                Icon = MsBox.Avalonia.Enums.Icon.Success,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ButtonDefinitions = new List<ButtonDefinition> {
                    new ButtonDefinition { Name = "Ок"}
                }
            });

        await messageBox.ShowAsync();
        HideBrigadeAssignmentView();
    }

    private void CloseBrigadeView_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HideBrigadeAssignmentView();
    }

    private async void ShowMessage(Exception ex)
    {
        var messageBox = MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = ex.Message, // Изменено: показываем только сообщение исключения
                Icon = MsBox.Avalonia.Enums.Icon.Warning,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ButtonDefinitions = new List<ButtonDefinition> {
                    new ButtonDefinition { Name = "Ок"}
                }
            });
        var result = await messageBox.ShowAsync();
    }
}