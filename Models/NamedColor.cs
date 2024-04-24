using System;
using Windows.UI;

namespace TabApp.Models;

public class NamedColor
{
    public string? Time { get; set; }
    public string? Amount { get; set; }
    public double Width { get; set; }
    public Color Color { get; set; }
    public override string ToString() => $"{Time},{Amount},{Color}";
}
