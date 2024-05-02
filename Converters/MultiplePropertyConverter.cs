using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.UI.Xaml.Media;

using TabApp.Redemption;

namespace TabApp;

public class MultiplePropertyConverter : IMultiValueConverter
{
    public object Convert(object[] values, object parameter)
    {
        //foreach (var value in values ?? Enumerable.Empty<object>())
        //{
        //    Debug.WriteLine($"[INFO] Got array object of type {value?.GetType()}");
        //}

        if (values != null && values.Length > 1)
        {
            var created = (DateTime)values[0];
            var updated = (DateTime)values[1];
            var diff = updated - created;
            //Debug.WriteLine($"[INFO] Day difference is {diff.TotalDays}");
            switch (Math.Abs(diff.TotalDays))
            {
                case double t when t > 256d:
                    return new SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                case double t when t > 128d:
                    return new SolidColorBrush(Microsoft.UI.Colors.Red);
                case double t when t > 64d:
                    return new SolidColorBrush(Microsoft.UI.Colors.OrangeRed);
                case double t when t > 32d:
                    return new SolidColorBrush(Microsoft.UI.Colors.Orange);
                case double t when t > 16d:
                    return new SolidColorBrush(Microsoft.UI.Colors.Gold);
                case double t when t > 8d:
                    return new SolidColorBrush(Microsoft.UI.Colors.Yellow);
                case double t when t > 4d:
                    return new SolidColorBrush(Microsoft.UI.Colors.YellowGreen);
                case double t when t > 2d:
                    return new SolidColorBrush(Microsoft.UI.Colors.Green);
                case double t when t > 1d:
                    return new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue);
                default:
                    return new SolidColorBrush(Microsoft.UI.Colors.Gray);
            }
        }
        else
        {
            Debug.WriteLine($"[WARNING] Did not receive multiple objects.");
        }

        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }
}
