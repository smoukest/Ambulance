using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Ambulance
{
    public partial class App : Application
    {
        private DatabaseService _dt;
        private ClientSocket _clientSocket;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new Authorization();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}