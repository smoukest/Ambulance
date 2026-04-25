using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Ambulance
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Подписываемся на необработанные исключения
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new Authorization();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException("UnhandledException", e.ExceptionObject as Exception);
        }

        private void LogException(string source, Exception? ex)
        {
            if (ex == null) return;

            string errorLogPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Ambulance",
                "error.log"
            );

            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(errorLogPath) ?? "");
                System.IO.File.AppendAllText(
                    errorLogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{source}]\n{ex}\n\n"
                );
            }
            catch { }

            System.Diagnostics.Debug.WriteLine($"{source}: {ex}");
        }
    }
}