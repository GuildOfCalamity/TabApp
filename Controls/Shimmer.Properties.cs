using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TabApp.Controls;

/// <summary>
/// TODO: Add color DPs for gradient stops so user can configure from XAML if desired.
/// </summary>
public partial class Shimmer : Control
{
    /// <summary>
    /// Identifies the <see cref="Duration"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
        nameof(Duration),
        typeof(object),
        typeof(Shimmer),
        new PropertyMetadata(defaultValue: TimeSpan.FromMilliseconds(2000), PropertyChanged));

    /// <summary>
    /// Gets or sets the animation duration.
    /// </summary>
    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="IsActive"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive),
        typeof(bool),
        typeof(Shimmer),
        new PropertyMetadata(defaultValue: true, PropertyChanged));

    /// <summary>
    /// Gets or sets if the animation is playing.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="InitialStartPointX"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty InitialStartPointXProperty = DependencyProperty.Register(
        nameof(InitialStartPointX),
        typeof(double),
        typeof(Shimmer),
        new PropertyMetadata(defaultValue: -7.92d, PropertyChanged));

    /// <summary>
    /// Gets or sets the vector starting point.
    /// </summary>
    public double InitialStartPointX
    {
        get => (double)GetValue(InitialStartPointXProperty);
        set => SetValue(InitialStartPointXProperty, value);
    }

    private static void PropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
    {
        var self = (Shimmer)s;
        
        if (self == null)
            return;

        if (self.IsActive)
        {
            self.StopAnimation();
            self.TryStartAnimation();
        }
        else
        {
            self.StopAnimation();
        }
    }
}
