using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ambulance.Views;

public partial class AllRequestsView : UserControl
{
    DatabaseService _dt;
    string connectionString = "Server=localhost;Port=5432;Username=postgres;Password=123;Database=amb;";
    public AllRequestsView()
    {
        InitializeComponent();
        _dt = new DatabaseService(connectionString);
    }
}