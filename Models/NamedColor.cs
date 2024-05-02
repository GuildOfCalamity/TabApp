using System;
using Windows.UI;

namespace TabApp.Models;

public class NamedColor : ICloneable
{
    /// <summary>
    /// The time in which the event was created.
    /// </summary>
    public string? Time { get; set; } 
    
    /// <summary>
    /// CPU use percentage.
    /// </summary>
    public string? Amount { get; set; }
    
    /// <summary>
    /// Histogram size.
    /// </summary>
    public double Width { get; set; }
    
    /// <summary>
    /// The color associated with the usage.
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Support for deep-copy routines.
    /// </summary>
    public object Clone()
    {
        return new NamedColor
        {
            Time = this.Time,
            Amount = this.Amount,
            Width = this.Width,
            Color = this.Color,
        };
    }

    public override string ToString()
    {
        string format = "Time:{0,-20} Amount:{1,-20} Color:{2,-10} Width:{3,-10}";
        return String.Format(format, $"{Time}", $"{Amount}", $"{Color}", $"{Width}");
    }
}
