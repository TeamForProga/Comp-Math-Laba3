using System;
using System.Collections.Generic;
using System.Linq;

namespace laba3.Models;

public class ApproxNewtonFunc : IApproximateFunc
{
    public List<Coord> Points { get; set; } = [];

    public string Name { get; set; } = "Newton";
    
    // кэш разделённых разностей
    private readonly double[] diffCache; 

    // Хеш-функция
    // Каждому i и j подбирает уникальное значение
    private int Index(int i, int j) => i * n - i * (i + 1) / 2 + j; 

    // Число точек
    private readonly int n;

    public ApproxNewtonFunc(List<Coord> points)
    {
        if (points.Count < 1)
            throw new System.ArgumentException("Incorrect amount of points");

        Points = [.. points ];

        n = Points.Count;
        
        int diffCacheSize = n * (n + 1) / 2; 
        diffCache = new double[diffCacheSize];

        // Заполняем кеш
        // Его можно представить как треугольную матрицу
        // *
        // * *
        // * * *
        // * * * *
        // Сейчас заполняется главная диагональ
        // Чем ближе к углу, тем выше порядок
        for (int i = 0; i < n; ++i)
            diffCache[Index(i,i)] = Points[i].Y;

        for (int len = 2; len <= n; len++)
        {
            for (int i = 0; i <= n - len; i++)
            {
                int j = i + len - 1;
                double left  = diffCache[Index(i, j - 1)];
                double right = diffCache[Index(i + 1, j)];
                double denom = Points[i].X - Points[j].X;
                diffCache[Index(i, j)] = (left - right) / denom;
            }
        }
    }

    // Прямой доступ к разделённой разности f[x_i,...,x_j]
    private double GetSepDiff(int i, int j) => diffCache[Index(i, j)];

    // Вычисление полинома Ньютона (схема Горнера)
    public double Func(double x)
    {
        double result = 0;
        double product = 1;
        for (int i = 0; i < n; i++)
        {
            result += GetSepDiff(0, i) * product;
            product *= x - Points[i].X;
        }
        return result;
    }
}