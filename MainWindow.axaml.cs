using Ambulance.Views;
using Avalonia.Controls;
using System;

namespace Ambulance
{
    public partial class MainWindow : Window
    {
        private MapWindow? _mapWindow;
        
        public MainWindow()
        {
            InitializeComponent();
            Opened += MainWindow_Opened;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Opened(object? sender, EventArgs e)
        {
            if (_mapWindow is { IsVisible: true })
            {
                return;
            }

            _mapWindow = new MapWindow();
            _mapWindow.Show();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _mapWindow?.Close();
            _mapWindow = null;
        }

        public void SetAddressFromMap(string address)
        {
            if (MainContent.Content is not RegistrationView registrationView)
            {
                Registration_Click(this, new Avalonia.Interactivity.RoutedEventArgs());
                registrationView = MainContent.Content as RegistrationView;
            }

            registrationView?.SetAddress(address);
        }

        private void Registration_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Сбрасываем стили всех кнопок
            ResetButtonStyles();

            // Устанавливаем стиль для активной кнопки
            var registrationButton = this.FindControl<Button>("RegistrationButton");
            registrationButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E53935"));
            registrationButton.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("White"));

            // Меняем контент на представление регистрации
            MainContent.Content = new RegistrationView();
        }

        private void Requests_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Сбрасываем стили всех кнопок
            ResetButtonStyles();

            // Устанавливаем стиль для активной кнопки
            var requestsButton = this.FindControl<Button>("RequestsButton");
            requestsButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E53935"));
            requestsButton.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("White"));

            // Меняем контент на представление всех заявок
            MainContent.Content = new AllRequestsView();
        }

        private void Stats_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Сбрасываем стили всех кнопок
            ResetButtonStyles();

            // Устанавливаем стиль для активной кнопки
            var statsButton = this.FindControl<Button>("StatsButton");
            statsButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E53935"));
            statsButton.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("White"));

            // Меняем контент на представление статистики
            MainContent.Content = new StatisticsView();
        }

        private void Test_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Сбрасываем стили всех кнопок
            ResetButtonStyles();

            // Устанавливаем стиль для активной кнопки
            var testButton = this.FindControl<Button>("TestButton");
            testButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E53935"));
            testButton.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("White"));

            // Меняем контент на представление статистики
            MainContent.Content = new TestView();
        }

        private void Test_Click2(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Сбрасываем стили всех кнопок
            ResetButtonStyles();

            // Устанавливаем стиль для активной кнопки
            var testButton = this.FindControl<Button>("TestButton2");
            testButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E53935"));
            testButton.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("White"));

            // Меняем контент на представление статистики
            //MainContent.Content = new TestViewModel();
        }

        private void ResetButtonStyles()
        {
            var registrationButton = this.FindControl<Button>("RegistrationButton");
            var requestsButton = this.FindControl<Button>("RequestsButton");
            var statsButton = this.FindControl<Button>("StatsButton");

            registrationButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("Transparent"));
            registrationButton.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("White"));

            requestsButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("Transparent"));
            requestsButton.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("White"));

            statsButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("Transparent"));
            statsButton.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("White"));
        }
        

    }
}