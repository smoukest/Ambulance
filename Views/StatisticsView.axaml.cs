using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ambulance.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System;
using LiveChartsCore.SkiaSharpView;

namespace Ambulance.Views;

public partial class StatisticsView : UserControl
{
    public StatisticsView()
    {
        InitializeComponent();
        DataContext = new StatisticsViewModel();

        LiveChartsCore.LiveCharts.Configure(config =>
            config.AddSkiaSharp()
                  .AddDefaultMappers()
                  .AddLightTheme());

        QuestPDF.Settings.License = LicenseType.Community;
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is StatisticsViewModel vm)
        {
            vm.LoadStatistics();
        }
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is StatisticsViewModel vm)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, $"Statistics_Export_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                        page.Header().Text("Ambulance Statistics Report")
                            .SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);

                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                        {
                            col.Item().Text($"Date generated: {DateTime.Now:dd.MM.yyyy HH:mm}");
                            col.Item().PaddingTop(20);

                            col.Item().Text("General Metrics:").SemiBold().FontSize(18);
                            col.Item().Text($"- Total calls: {vm.TotalCalls}");
                            col.Item().Text($"- Completed today: {vm.CompletedToday}");
                            col.Item().Text($"- In progress: {vm.InProgress}");
                            col.Item().Text($"- Critical calls: {vm.CriticalCalls}");

                            // NOTE: Charts can also be rendered as tables of data in PDF.
                            // Image generation from UI for QuestPDF requires additional tools (e.g. Avalonia RenderTargetBitmap logic)
                            // We are rendering the metrics first to make it a working PDF summary.
                        });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                })
                .GeneratePdf(filePath);
            }
            catch (Exception)
            {
                // Handle PDF writing exception if needed
            }
        }
    }
}