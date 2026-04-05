namespace laba3.Models;

public class Coord
{
    public double X { get; set; }
    public double Y { get; set; }

    override public string ToString() => $"x={X}\ny={Y}";
}