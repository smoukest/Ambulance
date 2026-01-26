using Avalonia;
using Avalonia.Controls;
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
    public RegistrationView()
    {
        InitializeComponent();
        _dt = new DatabaseService(connectionString);
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
        Status.SelectedIndex = -1;
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
            string anamnesis = Anamnesis.Text;
            string complaints = Complaints.Text;
            if (Status.SelectedIndex == -1)
                throw new Exception("Пожалуйста, укажите цель поступившего звонка");
            string appealPurpose = AppealPurpose.SelectedItem.ToString();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(patronymic) || string.IsNullOrEmpty(phoneNumber)
                 || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(anamnesis) || string.IsNullOrEmpty(complaints))
                throw new Exception("Пожалуйста, заполните все поля ввода");
            _dt.CreatePatient(name, surname, patronymic, phoneNumber, address, email, anamnesis, complaints, appealPurpose);
        }
        catch (Exception ex)
        {
            ShowMessage(ex);
            return;
        }
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