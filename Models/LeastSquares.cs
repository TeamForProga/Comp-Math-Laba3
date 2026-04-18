using System.Collections.Generic;
using laba3.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq;
using System;
using Avalonia.LogicalTree;

public class LeastSquares : IApproximateFunc
{
    public double[] Coefficients;
    public List<Coord> Points { get; set; }
    
    // Степень многочлена
    public int k;
    // Кол-во точек
    public int n;
    public Vector<double> X;
    public Vector<double> Y;

    public string Name { get; set; } = "LeastSquares";

    public double Func(double x)
    {
        double value = 0;
        double xPowerN = 1;
        
        for (int i = 0; i < Coefficients.Length; ++i)
        {
            value += xPowerN * Coefficients[i];
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

        X = Vector<double>.Build.DenseOfArray([.. Points.Select(p => p.X)]);
        Y = Vector<double>.Build.DenseOfArray([.. Points.Select(p => p.Y)]);
        
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

        // A = (M * M^T)^-1 * M^T * D
        if (k != n)
        {
            D = M.Transpose() * D;
            M = M.Transpose() * M;
        }
        PrintMatrix(M.ToArray(), D.ToArray(), k);

        // Значения коэффициентов аппроксимирующего многочлена Ai являются решением полученной системы 
        Coefficients = GaussRowPivot(M.ToArray(), D.ToArray(), k);

        if (Coefficients is null) throw new Exception("error");

        Name += $"^{power}";
    }
/// <summary>
    /// Метод Гаусса с ВЫБОРОМ ГЛАВНОГО ЭЛЕМЕНТА ПО СТРОКЕ.
    /// На каждом шаге k ищем максимальный по модулю элемент в строке k
    /// среди столбцов k..n-1. Меняем столбцы (= переставляем неизвестные).
    /// </summary>
    public static double[] GaussRowPivot(double[,] A, double[] b, int n, bool verbose = true)
    {
        double[,] a = (double[,])A.Clone();
        double[] rhs = (double[])b.Clone();

        // colOrder[j] = исходный индекс переменной, стоящей сейчас в столбце j
        // Изначально переменная x_j находится в столбце j
        int[] colOrder = new int[n];
        for (int j = 0; j < n; j++) colOrder[j] = j;

        if (verbose) Console.WriteLine("  === Прямой ход (выбор по строке) ===");

        for (int k = 0; k < n - 1; k++)
        {
            // ---- ВЫБОР ГЛАВНОГО ЭЛЕМЕНТА В СТРОКЕ k ----
            int maxCol = k;
            double maxVal = Math.Abs(a[k, k]);
            // Ищем максимальный по модулю элемент в строке k, начиная со столбца k
            for (int j = k + 1; j < n; j++)
            {
                if (Math.Abs(a[k, j]) > maxVal)
                {
                    maxVal = Math.Abs(a[k, j]);
                    maxCol = j;
                }
            }

            // ---- ЕСЛИ НАЙДЕН ДРУГОЙ СТОЛБЕЦ, МЕНЯЕМ СТОЛБЦЫ МЕСТАМИ ----
            if (maxCol != k)
            {
                // Меняем столбцы k и maxCol во ВСЕХ строках (т.к. перестановка неизвестных)
                for (int i = 0; i < n; i++)
                {
                    double tmp = a[i, k];
                    a[i, k] = a[i, maxCol];
                    a[i, maxCol] = tmp;
                }
                // Запоминаем перестановку переменных
                int tmpIdx = colOrder[k];
                colOrder[k] = colOrder[maxCol];
                colOrder[maxCol] = tmpIdx;

                if (verbose)
                {
                    int leftVar = colOrder[k] + 1;   // +1 для вывода в 1-индексации
                    int rightVar = colOrder[maxCol] + 1;
                    Console.WriteLine($"  Шаг k={k}: меняем столбцы {k} и {maxCol} (x{leftVar} ↔ x{rightVar})");
                }
            }
            else
            {
                if (verbose)
                    Console.WriteLine($"  Шаг k={k}: перестановка не нужна, ведущий элемент a[{k},{k}]={a[k, k]:F5}");
            }

            // Проверка на вырожденность
            if (Math.Abs(a[k, k]) < 1e-10f)
            {
                Console.WriteLine("  [!] Нулевой ведущий элемент — матрица вырожденная.");
                return null;
            }

            // ---- ИСКЛЮЧЕНИЕ (стандартный прямой ход) ----
            for (int i = k + 1; i < n; i++)
            {
                double m = a[i, k] / a[k, k];
                for (int j = k; j < n; j++)
                    a[i, j] -= m * a[k, j];
                rhs[i] -= m * rhs[k];
            }
        }

        if (verbose)
        {
            Console.WriteLine("\n  Матрица после прямого хода [A|b]:");
            PrintMatrix(a, rhs, n);
        }

        // ---- ОБРАТНЫЙ ХОД В ПЕРЕСТАВЛЕННОМ ПОРЯДКЕ ----
        double[] xPerm = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = rhs[i];
            for (int j = i + 1; j < n; j++)
                sum -= a[i, j] * xPerm[j];
            xPerm[i] = sum / a[i, i];
        }

        // ---- ВОССТАНАВЛИВАЕМ ИСХОДНЫЙ ПОРЯДОК ПЕРЕМЕННЫХ ----
        // xPerm[i] — это значение переменной, которая сейчас в столбце i,
        // а исходный индекс этой переменной — colOrder[i]
        double[] x = new double[n];
        for (int i = 0; i < n; i++)
            x[colOrder[i]] = xPerm[i];

        return x;
    }


    // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

    /// <summary>Вывод расширенной матрицы [A|b] в консоль.</summary>
    public static void PrintMatrix(double[,] A, double[] b, int n)
    {
        for (int i = 0; i < n; i++)
        {
            Console.Write("  |");
            for (int j = 0; j < n; j++)
                Console.Write($" {A[i, j],10:F5}");
            Console.WriteLine($" | {b[i],10:F5} |");
        }
        Console.WriteLine();
    }
}