using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace TabApp;

/// <summary>
/// To be used with a <see cref="Microsoft.UI.Xaml.Shapes.Rectangle"/> control.
/// </summary>
/// <example>
/// {Rectangle Width="28" Height="28" Fill="{x:Bind ViewModel.SystemState, Mode=OneWay, Converter={StaticResource StateToGradientBrush}}"/}
/// </example>
public class StateToGradientBrushConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        LinearGradientBrush result = new LinearGradientBrush();

        if (value == null || App.IsClosing)
            return result;

        switch ((SystemStates)value)
        {
            case SystemStates state when state == SystemStates.None:       // off
                result = CreateGradient(Windows.UI.Color.FromArgb(255, 250, 250, 250), Windows.UI.Color.FromArgb(255, 100, 100, 100), Windows.UI.Color.FromArgb(255, 20, 20, 20), Windows.UI.Color.FromArgb(255, 70, 70, 70));
                break;
            case SystemStates state when state == SystemStates.Init:       // blue
                result = CreateGradient(Windows.UI.Color.FromArgb(255, 250, 250, 250), Windows.UI.Color.FromArgb(255, 100, 100, 250), Windows.UI.Color.FromArgb(255, 20, 20, 230), Windows.UI.Color.FromArgb(255, 70, 70, 250));
                break;
            case SystemStates state when state == SystemStates.Processing: // green
                result = CreateGradient(Windows.UI.Color.FromArgb(255, 250, 250, 250), Windows.UI.Color.FromArgb(255, 100, 250, 100), Windows.UI.Color.FromArgb(255, 20, 210, 20), Windows.UI.Color.FromArgb(255, 70, 250, 70));
                break;
            case SystemStates state when state == SystemStates.Ready:      // off
                result = CreateGradient(Windows.UI.Color.FromArgb(255, 250, 250, 250), Windows.UI.Color.FromArgb(255, 100, 100, 100), Windows.UI.Color.FromArgb(255, 20, 20, 20), Windows.UI.Color.FromArgb(255, 70, 70, 70));
                break;
            case SystemStates state when state == SystemStates.Warning:    // orange
                result = CreateGradient(Windows.UI.Color.FromArgb(255, 250, 250, 250), Windows.UI.Color.FromArgb(255, 250, 100, 20), Windows.UI.Color.FromArgb(255, 220, 100, 20), Windows.UI.Color.FromArgb(255, 250, 120, 70));
                break;
            case SystemStates state when state == SystemStates.Shutdown:   // yellow
                result = CreateGradient(Windows.UI.Color.FromArgb(255, 250, 250, 250), Windows.UI.Color.FromArgb(255, 220, 180, 20), Windows.UI.Color.FromArgb(255, 180, 100, 20), Windows.UI.Color.FromArgb(255, 150, 100, 20));
                break;
            case SystemStates state when state == SystemStates.Faulted:    // red
                result = CreateGradient(Windows.UI.Color.FromArgb(255, 250, 250, 250), Windows.UI.Color.FromArgb(255, 250, 100, 100), Windows.UI.Color.FromArgb(255, 230, 20, 20), Windows.UI.Color.FromArgb(255, 250, 70, 70));
                break;
            default: // ???
                result = CreateGradient(Windows.UI.Color.FromArgb(255, 250, 250, 250), Windows.UI.Color.FromArgb(255, 100, 100, 250), Windows.UI.Color.FromArgb(255, 20, 20, 230), Windows.UI.Color.FromArgb(255, 70, 70, 250));
                break;
        }

        return result;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }

    /// <summary>
    /// Creates a <see cref="LinearGradientBrush"/> from four input colors.
    /// </summary>
    /// <param name="c1">offset 0.0 color</param>
    /// <param name="c2">offset 0.3 color</param>
    /// <param name="c3">offset 0.75 color</param>
    /// <param name="c4">offset 1.0 color</param>
    /// <returns><see cref="LinearGradientBrush"/></returns>
    LinearGradientBrush CreateGradient(Windows.UI.Color c1, Windows.UI.Color c2, Windows.UI.Color c3, Windows.UI.Color c4)
    {
        GradientStop? gs1 = new() { Color = c1, Offset = 0.0  };
        GradientStop? gs2 = new() { Color = c2, Offset = 0.3  };
        GradientStop? gs3 = new() { Color = c3, Offset = 0.75 };
        GradientStop? gs4 = new() { Color = c4, Offset = 1.0  };
        var gsc = new GradientStopCollection();
        gsc.Add(gs1); gsc.Add(gs2); gsc.Add(gs3); gsc.Add(gs4);
        var lgb = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(0, 1),
            GradientStops = gsc
        };
        return lgb;
    }
}
