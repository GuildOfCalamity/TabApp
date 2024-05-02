using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using TabApp;

namespace TabApp;

/// <summary>
/// To be used with an <see cref="Microsoft.UI.Xaml.Controls.Image"/> control.
/// </summary>
/// <example>
/// {Image Width="28" Height="28" Source="{x:Bind ViewModel.SystemState, Mode=OneWay, Converter={StaticResource StateToImage}}"/}
/// </example>
public class StateToImageConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        ImageSource result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_Off.png", UriKind.Absolute));

        if (value == null)
            return result;

        switch ((SystemStates)value)
        {
            case SystemStates state when state == SystemStates.None:
                result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_Off.png", UriKind.Absolute));
                break;
            case SystemStates state when state == SystemStates.Init:
                result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_On.png", UriKind.Absolute));
                break;
            case SystemStates state when state == SystemStates.Processing:
                result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_On.png", UriKind.Absolute));
                break;
            case SystemStates state when state == SystemStates.Ready:
                result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_Off.png", UriKind.Absolute));
                break;
            case SystemStates state when state == SystemStates.Warning:
                result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_Warning.png", UriKind.Absolute));
                break;
            case SystemStates state when state == SystemStates.Shutdown:
                result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_Idle.png", UriKind.Absolute));
                break;
            case SystemStates state when state == SystemStates.Faulted:
                result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_Error.png", UriKind.Absolute));
                break;
            default: // ???
                result = new BitmapImage(new Uri($"ms-appx:///{App.AssetFolder}/LED_Off.png", UriKind.Absolute));
                break;
        }

        return result;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
