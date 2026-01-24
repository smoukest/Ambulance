using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Ambulance;

public partial class Authorization : Window
{
    public Authorization()
    {
        InitializeComponent();
    }

    private void OnHeaderPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        this.BeginMoveDrag(e);
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }

    private void OnSubmit(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string login, password;
        try
        {
            login = Login.Text;
            password = Password.Text;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                throw new Exception("Пожалуйста, заполните все поля ввода");
            //throw new Exception("Пожалуйста, проверьте корректность введённых данных");
        }
        catch (Exception ex)
        {
            ShowMessage(ex);
            return;
        }
        var newWindow = new MainWindow();
        newWindow.Show();
        this.Close();
    }
    private async void ShowMessage(Exception ex)
    {
        var messageBox = MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = $"{ex}",
                Icon = MsBox.Avalonia.Enums.Icon.Warning,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ButtonDefinitions = new List<ButtonDefinition> {
                    new ButtonDefinition { Name = "Ок"}
                }
            });
        var result = await messageBox.ShowWindowDialogAsync(this);
    }
}