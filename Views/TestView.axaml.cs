using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ambulance.Views;

public partial class TestView : UserControl
{
    public TestView()
    {
        InitializeComponent();
        DataContext = new ViewModels.TestViewModel();
    }
}