using Avalonia.Controls;
using Avalonia.Interactivity;

using ScottPlot.Avalonia;
using laba3.ViewModels;

namespace laba3.Views;

public partial class MainWindow : Window
{
    AvaPlot? avaPlot = null;

    public MainWindow()
    {
        InitializeComponent();

        double[] dataX = { 1, 2, 3, 4, 5 };
        double[] dataY = { 1, 4, 9, 16, 25 };

        avaPlot = this.Find<AvaPlot>("AvaPlot1");
        avaPlot?.Plot.Add.Scatter(dataX, dataY);
        avaPlot?.Refresh();

        avaPlot?.Plot.Add.Scatter(dataY, dataX);

        this.Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {     
            vm.AfterWindowLoaded(avaPlot);
        }
    }
}