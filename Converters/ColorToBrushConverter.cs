using System;
using System.Diagnostics;
using System.Drawing;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace TabApp;

public class ColorToBrushConverter : IValueConverter
{
    /// <summary>
    /// Converts the given <paramref name="value"/> into a <see cref="SolidColorBrush"/>.
    /// Format of the color <paramref name="value"/> should be "#123456" or "#12345678".
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            //Debug.WriteLine($"[INFO] Converting color value \"{value}\" to SolidColorBrush.");
            var scb = new SolidColorBrush((Windows.UI.Color)value);
            return scb;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] ColorToBrushConverter: {ex.Message}");
            return new SolidColorBrush(Microsoft.UI.Colors.Red);
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
