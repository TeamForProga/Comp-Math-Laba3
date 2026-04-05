using System.Collections.Generic;

namespace laba3.Models;

public class ApproxLagrangeFunc : IApproximateFunc
{
    public List<Coord> Points { get; set; } = [];

    public string Name { get; set; } = "Lagrange";

    public ApproxLagrangeFunc() {}

    public ApproxLagrangeFunc(List<Coord> points)
    {
        if (points.Count < 1)
            throw new System.ArgumentException("Incorrect amount of points");

        Points = [.. points ];
    }

    public double Func(double x)
    {
        double summValue = 0;

        for (int i = 0; i < Points.Count; ++i)
        {
            double multValue = Points[i].Y;
            for (int j = 0; j < Points.Count; ++j)
            {
                if (j == i) continue;
                multValue *= (x - Points[j].X) / (Points[i].X - Points[j].X);
            }

            summValue += multValue;
        }
        
        return summValue;
    }
}