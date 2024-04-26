using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabApp.Helpers;

/// <summary>
/// Provides attached dependency properties and methods for the <see cref="ScrollViewer"/> control.
/// </summary>
public static partial class ScrollViewerExtensions
{
    private static void OnHorizontalScrollBarMarginPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is FrameworkElement baseElement)
        {
            // If it didn't work it means that we need to wait for the component to be loaded before getting its ScrollViewer
            if (ChangeHorizontalScrollBarMarginProperty(baseElement))
            {
                return;
            }

            // We need to wait for the component to be loaded before getting its ScrollViewer
            baseElement.Loaded -= ChangeHorizontalScrollBarMarginProperty;

            if (HorizontalScrollBarMarginProperty != null)
            {
                baseElement.Loaded += ChangeHorizontalScrollBarMarginProperty;
            }
        }
    }

    private static bool ChangeHorizontalScrollBarMarginProperty(FrameworkElement sender)
    {
        if (sender == null)
        {
            return false;
        }

        var scrollViewer = sender as ScrollViewer ?? sender.FindDescendant<ScrollViewer>();

        // Last scrollbar with "HorizontalScrollBar" as name is our target to set its margin and avoid it overlapping the header
        var scrollBar = scrollViewer?.FindDescendants().OfType<ScrollBar>().LastOrDefault(bar => bar.Name == "HorizontalScrollBar");

        if (scrollBar == null)
        {
            return false;
        }

        var newMargin = GetHorizontalScrollBarMargin(sender);

        scrollBar.Margin = newMargin;

        return true;
    }

    private static void ChangeHorizontalScrollBarMarginProperty(object sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is FrameworkElement baseElement)
        {
            ChangeHorizontalScrollBarMarginProperty(baseElement);

            // Handling Loaded event is only required the first time the property is set, so we can stop handling it now
            baseElement.Loaded -= ChangeHorizontalScrollBarMarginProperty;
        }
    }

    private static void OnVerticalScrollBarMarginPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is FrameworkElement baseElement)
        {
            // We try to update the value, if it works we may exit
            if (ChangeVerticalScrollBarMarginProperty(baseElement))
            {
                return;
            }

            // If it didn't work it means that we need to wait for the component to be loaded before getting its ScrollViewer
            baseElement.Loaded -= ChangeVerticalScrollBarMarginProperty;

            if (VerticalScrollBarMarginProperty != null)
            {
                baseElement.Loaded += ChangeVerticalScrollBarMarginProperty;
            }
        }
    }

    private static bool ChangeVerticalScrollBarMarginProperty(FrameworkElement sender)
    {
        if (sender == null)
        {
            return false;
        }

        var scrollViewer = sender as ScrollViewer ?? sender.FindDescendant<ScrollViewer>();

        // Last scrollbar with "HorizontalScrollBar" as name is our target to set its margin and avoid it overlapping the header
        var scrollBar = scrollViewer?.FindDescendants().OfType<ScrollBar>().LastOrDefault(bar => bar.Name == "VerticalScrollBar");

        if (scrollBar == null)
        {
            return false;
        }

        var newMargin = GetVerticalScrollBarMargin(sender);

        scrollBar.Margin = newMargin;

        return true;
    }

    private static void ChangeVerticalScrollBarMarginProperty(object sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is FrameworkElement baseElement)
        {
            ChangeVerticalScrollBarMarginProperty(baseElement);

            // Handling Loaded event is only required the first time the property is set, so we can stop handling it now
            baseElement.Loaded -= ChangeVerticalScrollBarMarginProperty;
        }
    }
}

/// <summary>
/// Provides attached dependency properties and methods for the <see cref="ScrollViewer"/> control.
/// </summary>
public partial class ScrollViewerExtensions
{
    /// <summary>
    /// Attached <see cref="DependencyProperty"/> for binding a <see cref="Thickness"/> for the horizontal <see cref="ScrollBar"/> of a <see cref="ScrollViewer"/>
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollBarMarginProperty = DependencyProperty.RegisterAttached("HorizontalScrollBarMargin", typeof(Thickness), typeof(ScrollViewerExtensions), new PropertyMetadata(null, OnHorizontalScrollBarMarginPropertyChanged));

    /// <summary>
    /// Attached <see cref="DependencyProperty"/> for binding a <see cref="Thickness"/> for the vertical <see cref="ScrollBar"/> of a <see cref="ScrollViewer"/>
    /// </summary>
    public static readonly DependencyProperty VerticalScrollBarMarginProperty = DependencyProperty.RegisterAttached("VerticalScrollBarMargin", typeof(Thickness), typeof(ScrollViewerExtensions), new PropertyMetadata(null, OnVerticalScrollBarMarginPropertyChanged));

    /// <summary>
    /// Gets the <see cref="Thickness"/> associated with the specified vertical <see cref="ScrollBar"/> of a <see cref="ScrollViewer"/>
    /// </summary>
    /// <param name="obj">The <see cref="FrameworkElement"/> to get the associated <see cref="Thickness"/> from</param>
    /// <returns>The <see cref="Thickness"/> associated with the <see cref="FrameworkElement"/></returns>
    public static Thickness GetVerticalScrollBarMargin(FrameworkElement obj)
    {
        return (Thickness)obj.GetValue(VerticalScrollBarMarginProperty);
    }

    /// <summary>
    /// Sets the <see cref="Thickness"/> associated with the specified vertical <see cref="ScrollBar"/> of a <see cref="ScrollViewer"/>
    /// </summary>
    /// <param name="obj">The <see cref="FrameworkElement"/> to associate the <see cref="Thickness"/> with</param>
    /// <param name="value">The <see cref="Thickness"/> for binding to the <see cref="FrameworkElement"/></param>
    public static void SetVerticalScrollBarMargin(FrameworkElement obj, Thickness value)
    {
        obj.SetValue(VerticalScrollBarMarginProperty, value);
    }

    /// <summary>
    /// Gets the <see cref="Thickness"/> associated with the specified horizontal <see cref="ScrollBar"/> of a <see cref="ScrollViewer"/>
    /// </summary>
    /// <param name="obj">The <see cref="FrameworkElement"/> to get the associated <see cref="Thickness"/> from</param>
    /// <returns>The <see cref="Thickness"/> associated with the <see cref="FrameworkElement"/></returns>
    public static Thickness GetHorizontalScrollBarMargin(FrameworkElement obj)
    {
        return (Thickness)obj.GetValue(HorizontalScrollBarMarginProperty);
    }

    /// <summary>
    /// Sets the <see cref="Thickness"/> associated with the specified horizontal <see cref="ScrollBar"/> of a <see cref="ScrollViewer"/>
    /// </summary>
    /// <param name="obj">The <see cref="FrameworkElement"/> to associate the <see cref="Thickness"/> with</param>
    /// <param name="value">The <see cref="Thickness"/> for binding to the <see cref="FrameworkElement"/></param>
    public static void SetHorizontalScrollBarMargin(FrameworkElement obj, Thickness value)
    {
        obj.SetValue(HorizontalScrollBarMarginProperty, value);
    }
}
