using System;
using System.Diagnostics;
using System.Drawing;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace TabApp;

public class ColorToLighterColorConverter : IValueConverter
{
    /// <summary>
    /// Lightens color by <paramref name="parameter"/>.
    /// If no value is provided then 0.5 will be used.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //Debug.WriteLine($"Converting color value {value} to lighter color.");
        var source = (Windows.UI.Color)value;

        if (parameter != null && float.TryParse($"{parameter}", out float factor))
            return source.LighterBy(factor);
        else
            return source.LighterBy(0.5F);

        var red = (int)((float)source.R * 1.6F);
        var green = (int)((float)source.G * 1.6F);
        var blue = (int)((float)source.B * 1.6F);
        if (red == 0) { red = 0x1F; }
        else if (red > 255) { red = 0xFF; }
        if (green == 0) { green = 0x1F; }
        else if (green > 255) { green = 0xFF; }
        if (blue == 0) { blue = 0x1F; }
        else if (blue > 255) { blue = 0xFF; }
        return Windows.UI.Color.FromArgb((byte)255, (byte)red, (byte)green, (byte)blue);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}


public class ColorToDarkerColorConverter : IValueConverter
{
    /// <summary>
    /// Darkens color by <paramref name="parameter"/>.
    /// If no value is provided then 0.5 will be used.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //Debug.WriteLine($"Converting color value {value} to darker color.");
        var source = (Windows.UI.Color)value;

        if (parameter != null && float.TryParse($"{parameter}", out float factor))
            return source.DarkerBy(factor);
        else
            return source.DarkerBy(0.5F);

        if (source.R == 0) { source.R = 2; }
        if (source.G == 0) { source.G = 2; }
        if (source.B == 0) { source.B = 2; }
        var red = (int)((float)source.R / 1.6F);
        var green = (int)((float)source.G / 1.6F);
        var blue = (int)((float)source.B / 1.6F);
        return Windows.UI.Color.FromArgb((byte)255, (byte)red, (byte)green, (byte)blue);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}

public class ColorToLighterBrushConverter : IValueConverter
{
    /// <summary>
    /// Lightens color by <paramref name="parameter"/>.
    /// If no value is provided then 0.5 will be used.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //Debug.WriteLine($"Converting color value {value} to lighter color.");
        var source = (Windows.UI.Color)value;

        if (parameter != null && float.TryParse($"{parameter}", out float factor))
            return new SolidColorBrush(source.LighterBy(factor));
        else
            return new SolidColorBrush(source.LighterBy(0.5F));

        var red = (int)((float)source.R * 1.6F);
        var green = (int)((float)source.G * 1.6F);
        var blue = (int)((float)source.B * 1.6F);
        if (red == 0) { red = 0x1F; }
        else if (red > 255) { red = 0xFF; }
        if (green == 0) { green = 0x1F; }
        else if (green > 255) { green = 0xFF; }
        if (blue == 0) { blue = 0x1F; }
        else if (blue > 255) { blue = 0xFF; }
        var scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)red, (byte)green, (byte)blue));
        return scb;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}

public class ColorToDarkerBrushConverter : IValueConverter
{
    /// <summary>
    /// Darkens color by <paramref name="parameter"/>.
    /// If no value is provided then 0.5 will be used.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //Debug.WriteLine($"Converting color value {value} to darker color.");
        var source = (Windows.UI.Color)value;

        if (parameter != null && float.TryParse($"{parameter}", out float factor))
            return new SolidColorBrush(source.DarkerBy(factor));
        else
            return new SolidColorBrush(source.DarkerBy(0.5F));

        if (source.R == 0) { source.R = 2; }
        if (source.G == 0) { source.G = 2; }
        if (source.B == 0) { source.B = 2; }
        var red = (int)((float)source.R / 1.6F);
        var green = (int)((float)source.G / 1.6F);
        var blue = (int)((float)source.B / 1.6F);
        var scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)red, (byte)green, (byte)blue));
        return scb;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
