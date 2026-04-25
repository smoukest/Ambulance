using Avalonia;
using System;
using System.IO;

namespace Ambulance
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                // Логируем ошибку в файл перед завершением
                string errorLogPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Ambulance",
                    "error.log"
                );

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(errorLogPath) ?? "");
                    File.AppendAllText(
                        errorLogPath,
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {ex}\n"
                    );
                }
                catch { }

                // Пробуем показать ошибку в консоли/отладчике
                System.Diagnostics.Debug.WriteLine($"Fatal Error: {ex}");
                Console.WriteLine($"Fatal Error: {ex}");
                throw;
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
