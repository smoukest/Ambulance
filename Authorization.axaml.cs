using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
        try {
            login = Login.Text;
            password = Password.Text;
        }
        catch {
            MessageBox.Show("ГСЧ поддерживает только натуральные числа.\nПожалуйста, проверьте вводимые данные.",
                    "Некорректный ввод!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

    }
}