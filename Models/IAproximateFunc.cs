

using System.Collections.Generic;

namespace laba3.Models;

public interface IAproximateFunc
{
    List<Coord> Points { get; set; }

    string Name { get; set; }

    double Func(double x);
}