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