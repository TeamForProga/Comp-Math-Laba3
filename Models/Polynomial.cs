using System.Collections.Generic;

namespace laba3.Models;

public class Polynomia : IAproximateFunc
{
    public List<double> Coefficients = [];
    public List<Coord> Points { get; set; } = [];

    public string Name { get; set; } = "Polynomia";

    public double Func(double x)
    {
        double value = 0;
        double xPowerN = 1;
        
        foreach (var coeff in Coefficients)
        {
            value += xPowerN * coeff;
            xPowerN *= x;
        }

        return value;
    }
    public Polynomia(List<double>? coefficients = null, List<Coord>? points = null)
    {
        if (coefficients is not null)
            Coefficients = [.. coefficients ];

        if (Coefficients.Count < 5)
            for (int i = 5 - Coefficients.Count; i > 0; --i)
                Coefficients.Add(0);

        if (points is not null)
            Points = [.. points];
    }

    public double GetCoefficient(int index)
    {
        if (index >= Coefficients.Count)
            return 0;
        else
            return Coefficients[index];
    }

    public void SetCoefficint(int index, double value)
    {
        if (index >= Coefficients.Count)
            for (int i = 5 - Coefficients.Count; i > 0; --i)
                Coefficients.Add(0);

        Coefficients[index] = value;
    }

    public double this[int index]
    {
        get => GetCoefficient(index);
        set => SetCoefficint(index, value);
    }
}