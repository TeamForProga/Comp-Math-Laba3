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
using laba3.Services;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<IApproximateFunc> ApproximateFuncs { get; } = [];
    public ObservableCollection<Coord> Coords { get; } = [];

    [NotifyCanExecuteChangedFor(nameof(DeleteAprFuncCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReplaceAprFuncCommand))]
    [ObservableProperty] private IApproximateFunc? _selectedAprFunc;
    
    public ObservableCollection<string> CoordInput { get; } = ["", ""];
    public ObservableCollection<string> PolynomiaInput { get; } = ["", "","", "", ""];
    public string SquaresPowerInput { get; set; } = "";

    [NotifyCanExecuteChangedFor(nameof(ReplaceCoordCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCoordCommand))]
    [ObservableProperty] private Coord? _selectedCoord; 
    
    AvaPlot? APlot = null;

    [RelayCommand]
    void LagrangeApproximate()
    {
        if (Coords.Count < 1)
        {
            WindowService?.NotificationManager?.Show($"For Lagrange's method you must specify at least one point", NotificationType.Warning, TimeSpan.FromSeconds(3));
            return;
        }

        ApproximateFuncs.Add(new ApproxLagrangeFunc([.. Coords ]));
        
        UpdatePlot();
    }

    [RelayCommand]
    void NewtonApproximate()
    {
        if (Coords.Count < 1)
        {
            WindowService?.NotificationManager?.Show($"For Newtom's metod you must specify at least one point", NotificationType.Warning, TimeSpan.FromSeconds(3));
            return;
        }

        ApproximateFuncs.Add(new ApproxNewtonFunc([.. Coords ]));

        UpdatePlot();
    }

    [RelayCommand]
    void SquaresSmoothing() 
    {
        if (!int.TryParse(SquaresPowerInput, out int power)) {
            WindowService?.NotificationManager?.Show($"Power must be natural number (indcluded zero)", NotificationType.Warning, TimeSpan.FromSeconds(3));
            return;
        }
        else if (power >= Coords.Count) 
        {
            WindowService?.NotificationManager?.Show($"Points count must be greater then power", NotificationType.Warning, TimeSpan.FromSeconds(3));
            return;
        }

        ApproximateFuncs.Add(new LeastSquares([.. Coords ], power));

        UpdatePlot();
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
            //res.Points = [.. Coords];
            ApproximateFuncs.Add(res);
            
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
            //res.Points = [.. Coords];
            ApproximateFuncs[ApproximateFuncs.IndexOf(SelectedAprFunc)] = res;
            SelectedAprFunc = null;

            UpdatePlot();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditAprFunc))]
    private void DeleteAprFunc()
    {
        if (SelectedAprFunc is null) return;

        ApproximateFuncs.Remove(SelectedAprFunc);

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
        
        double[][] dataX = new double[ApproximateFuncs.Count][];
        double[][] dataY = new double[ApproximateFuncs.Count][];

        int dataIndex = 0;
        foreach(var item in ApproximateFuncs)
        {
            

            double min = double.MaxValue;
            double max = double.MinValue;
            
            if (item is Polynomia) {
                min = -1000;
                max = 1000;
            }

            for (int i = 0; i < item.Points.Count; ++i)
            {
                if (item.Points[i].X > max) max = item.Points[i].X;
                if (item.Points[i].X < min) min = item.Points[i].X;
            }

            const int pointCount = 1000;

            double step = (max - min) / pointCount;
            
            dataX[dataIndex] = new double[pointCount + 1];
            dataY[dataIndex] = new double[pointCount + 1];

            for (int i = 0; i <= pointCount; i++) {
                double tempX = step * i + min;
                dataX[dataIndex][i] = tempX;
                dataY[dataIndex][i] = item.Func(tempX);
            }

            ++dataIndex;
        }

        APlot.Plot.Clear();

        for (int i = 0; i < dataX.Length; ++i)
        {
            var scatter = APlot.Plot.Add.Scatter(dataX[i], dataY[i]);
            
            scatter.MarkerSize = 2;

            foreach (var point in ApproximateFuncs[i].Points) {
                APlot.Plot.Add.Marker(point.X, point.Y, ScottPlot.MarkerShape.FilledCircle, 10, scatter.Color);
                var line = APlot.Plot.Add.Line(point.X, 0, point.X, point.Y);
                line.LinePattern = ScottPlot.LinePattern.Dashed;
                line.Color = ScottPlot.Colors.SeaGreen;
            }
        }

        APlot.Refresh();
    }

    public AppState BuildState()
    {
        List<Polynomia> Polynomias = [];
        List<ApproxLagrangeFunc> ApproxLagrangeFuncs = [];
        List<ApproxNewtonFunc> ApproxNewtonFuncs = [];
        List<LeastSquares> LeastSquaress = [];
        
        List<Coord> Coordss = [.. Coords];

        foreach (var func in ApproximateFuncs)
        {
            switch (func)
            {
            case Polynomia polynomia:
                Polynomias.Add(polynomia);
                break; 
            case ApproxLagrangeFunc aLagrange: 
                ApproxLagrangeFuncs.Add(aLagrange);
                break; 
            case ApproxNewtonFunc aNewton: 
                ApproxNewtonFuncs.Add(aNewton);
                break; 
            case LeastSquares Least: 
                LeastSquaress.Add(Least);
                break; 
            }
        }

        AppState appState = new() 
        {
            Polynomia = Polynomias,
            ApproxLagrangeFunc = ApproxLagrangeFuncs,
            ApproxNewtonFunc = ApproxNewtonFuncs,
            LeastSquares = LeastSquaress,
            Coord = Coordss
        };
        
        return appState;
    }

    public async Task LoadAsync()
    {
        var state = await DataStorageService.LoadAsync();

        ApproximateFuncs.Clear();
        Coords.Clear();

        foreach (var func in state.ApproxLagrangeFunc)
            ApproximateFuncs.Add(func);
        foreach (var func in state.ApproxNewtonFunc)
            ApproximateFuncs.Add(func);
        foreach (var func in state.LeastSquares)
            ApproximateFuncs.Add(func);
        foreach (var func in state.Polynomia)
            ApproximateFuncs.Add(func);

        foreach (var coord in state.Coord)
            Coords.Add(coord);
    }
}
