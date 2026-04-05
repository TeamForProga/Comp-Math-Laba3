using System.Collections.Generic;
using laba3.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq;
using System;

public class LeastSquares : IApproximateFunc
{
    public Vector<double> Coefficients;
    public List<Coord> Points { get; set; }
    
    // Степень многочлена
    private int k;
    // Кол-во точек
    private  int n;
    private  Vector<double> X;
    private  Vector<double> Y;

    public string Name { get; set; } = "LeastSquares";

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

    // Фунция получения коэффициентов правой части
    private double GetD(int j)
    {
        double sum = 0;
        for (int i = 0; i < k; ++i)
        {
            sum += Y[i] * Math.Pow(X[i], j);
        }

        return sum;
    }

    // Функция получения коэффициентов левой части
    private double GetC(int m)
    {
        double sum = 0;
        for (int i = 0; i < k; ++i)
        {
            sum += Math.Pow(X[i], m);
        }

        return sum;
    }

    public LeastSquares(List<Coord> points, int power = 4)
    {
        if (power > points.Count) throw new System.ArgumentException("Power must be bigger then points count");

        Points = [.. points];

        k = power + 1;
        n = points.Count;

        X = Vector<double>.Build.DenseOfArray(Points.Select(p => p.X).ToArray());
        Y = Vector<double>.Build.DenseOfArray(Points.Select(p => p.Y).ToArray());
        
        // коэффициенты правой части
        var D = Vector<double>.Build.Dense(n);
        for (int i = 0; i < n; ++i) {
            D[i] = GetD(i);
        }    

        // Коэффициенты левой части
        var M = Matrix<double>.Build.Dense(n, k);
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < k; ++j)
            {
                M[i, j] = GetC(i + j);
            }
        }

        // Можно было решить так: A = (M * M^T)^-1 * M^T * D
        // Или так:
        //if (k == n)
        //{
        //    A = M.Inverse() * D;
        //}
        //else
        //{
        //    A = M.PseudoInverse() * D;
        //}
        
        // Значения коэффициентов аппроксимирующего многочлена Ai являются решением полученной системы 
        Coefficients = M.Solve(D);
    }
}