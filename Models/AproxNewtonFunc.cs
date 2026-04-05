using System;
using System.Collections.Generic;
using System.Linq;

namespace laba3.Models;

public class AproxNewtonFunc : IAproximateFunc
{
    public List<Coord> Points { get; set; } = [];

    public string Name { get; set; } = "Newton";

    public AproxNewtonFunc() {}

    public AproxNewtonFunc(List<Coord> points)
    {
        if (points.Count != 5)
            throw new System.ArgumentException("Incorrect amount of points");

        Points = [.. points ];
    }


    // Разделённая разность
    private double GetSepDiff(Span<int> indexes) {
        if (indexes.Length <= 1) return Points[indexes[0]].Y;
        
        Console.WriteLine($"{indexes.Length}");

        double value1 = GetSepDiff(indexes.Slice(0, indexes.Length - 1)); 
        double value2 = GetSepDiff(indexes.Slice(1));
        double diff1 = value1 - value2;
        double diff2 = Points[indexes[0]].X - Points[indexes[indexes.Length - 1]].X;

        return  diff1 / diff2;
    }

    private double GetFuncN(double x, int n)
    {
        if (n >= 5) return 0;

        double summ = GetSepDiff(Enumerable.Range(0, n + 1).ToArray());

        double mult = (x - Points[n].X);

        double value = GetFuncN(x, n + 1);

        mult *= value;

        summ += mult;

        return summ;
    }

    public double Func(double x)
    {
        return GetFuncN(x, 0);
    }
}