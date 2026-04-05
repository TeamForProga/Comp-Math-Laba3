namespace laba3.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot.Avalonia;

using laba3.ViewModels;
using laba3.Models;


public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<IAproximateFunc> AproximateFuncs { get; } = [];
    public ObservableCollection<Coord> Coords { get; } = [];

    [NotifyCanExecuteChangedFor(nameof(DeleteAprFuncCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReplaceAprFuncCommand))]
    [ObservableProperty] private IAproximateFunc? _selectedAprFunc;
    
    public ObservableCollection<string> CoordInput { get; } = ["", ""];
    public ObservableCollection<string> PolynomiaInput { get; } = ["", "","", "", ""];

    [NotifyCanExecuteChangedFor(nameof(ReplaceCoordCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCoordCommand))]
    [ObservableProperty] private Coord? _selectedCoord; 
    
    AvaPlot? APlot = null;

    [RelayCommand]
    void LagrangeAproximate()
    {
        if (Coords.Count != 5)
        {
            WindowService?.NotificationManager?.Show($"For Lagrange's method you must specify 5 points", NotificationType.Warning, TimeSpan.FromSeconds(3));
            return;
        }

        AproximateFuncs.Add(new AproxLagrangeFunc([.. Coords]));
        
        UpdatePlot();
    }

    [RelayCommand]
    void NewtonAproximate()
    {
        if (Coords.Count != 5)
        {
            WindowService?.NotificationManager?.Show($"For Newtom's metod you must specify 5 points", NotificationType.Warning, TimeSpan.FromSeconds(3));
            return;
        }

        AproximateFuncs.Add(new AproxNewtonFunc([.. Coords]));

        UpdatePlot();
    }

    [RelayCommand]
    void SquresSmoothing() 
    {

    }

    private bool CanEditCoord() => SelectedCoord != null;

    private Coord? TryParseCoordInput()
    {
        List<double> data = [];

        foreach (var elem in CoordInput)
        {
            if (double.TryParse(elem, out double result))
            {
                data.Add(result);
            }
            else 
            {
                WindowService?.NotificationManager?.Show(data.Count == 0 ? "X must be number" : "F(x) must be number", NotificationType.Warning, TimeSpan.FromSeconds(3));
                return null;
            }
        }

        return new Coord{X = data[0], Y = data[1]};
    }

    [RelayCommand]
    private void AddCoord()
    {
        var res = TryParseCoordInput();
        if (res is not null) 
            Coords.Add(res);
    }

    [RelayCommand(CanExecute = nameof(CanEditCoord))]
    private void DeleteCoord()
    {
        if (SelectedCoord is null) return;

        Coords.Remove(SelectedCoord);

        SelectedCoord = null;
    }

    [RelayCommand(CanExecute = nameof(CanEditCoord))]
    private void ReplaceCoord()
    {
        if (SelectedCoord is null) return;

        var res = TryParseCoordInput();
        if (res is not null)
        {
            Coords[Coords.IndexOf(SelectedCoord)] = res;

            SelectedCoord = null;
        }
    }

    private bool CanEditAprFunc() => SelectedAprFunc != null;

    private Polynomia? TryParsePolynomiaInput()
    {
        List<double> data = [];
        
        foreach (var elem in PolynomiaInput)
        {
            if (double.TryParse(elem, out double result))
            {
                data.Add(result);
            }
            else 
            {
                WindowService?.NotificationManager?.Show($"a{data.Count} must be number", NotificationType.Warning, TimeSpan.FromSeconds(3));
                return null;
            }
        }

        return new Polynomia(data);
    }

    [RelayCommand]
    private void AddPolynomia()
    {        
        var res = TryParsePolynomiaInput(); 
        if (res is not null) {
            res.Points = [.. Coords];
            AproximateFuncs.Add(res);
            
            UpdatePlot();
        }

    }

    [RelayCommand(CanExecute = nameof(CanEditAprFunc))]
    private void ReplaceAprFunc()
    {
        if (SelectedAprFunc is null) return;

        var res = TryParsePolynomiaInput();
        if (res is not null)
        {
            res.Points = [.. Coords];
            AproximateFuncs[AproximateFuncs.IndexOf(SelectedAprFunc)] = res;
            SelectedAprFunc = null;

            UpdatePlot();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditAprFunc))]
    private void DeleteAprFunc()
    {
        if (SelectedAprFunc is null) return;

        AproximateFuncs.Remove(SelectedAprFunc);

        SelectedAprFunc = null;

        UpdatePlot();
    }
    
    public void AfterWindowLoaded(AvaPlot? avaPlot)
    {
        APlot = avaPlot;
    }

    public void UpdatePlot()
    {
        if (APlot is null)
            return;
        
        double[][] dataX = new double[AproximateFuncs.Count][];
        double[][] dataY = new double[AproximateFuncs.Count][];

        int dataIndex = 0;
        foreach(var item in AproximateFuncs)
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < item.Points.Count; ++i)
            {
                if (item.Points[i].X > max) max = item.Points[i].X;
                if (item.Points[i].X < min) min = item.Points[i].X;
            }

            const int pointCount = 1000;

            double step = (max - min) / pointCount;
            
            dataX[dataIndex] = new double[pointCount];
            dataY[dataIndex] = new double[pointCount];

            for (int i = 0; i < pointCount; i++) {
                double tempX = step * i + min;
                dataX[dataIndex][i] = tempX;
                dataY[dataIndex][i] = item.Func(tempX);
            }

            ++dataIndex;
        }

        APlot.Plot.Clear();

        for (int i = 0; i < dataX.Length; ++i)
        {
            APlot.Plot.Add.Scatter(dataX[i], dataY[i]);
            Console.WriteLine($"{dataX[i].Length}");

            for (int j = 0; j < dataX[i].Length; ++j)
                Console.WriteLine($"{dataX[i][j]} , {dataY[i][j]}");
        }

        APlot.Refresh();
    }
}
