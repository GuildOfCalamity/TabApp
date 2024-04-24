using System;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using VirtualKey = Windows.System.VirtualKey;
using VirtualKeyModifiers = Windows.System.VirtualKeyModifiers;

namespace TabApp.Controls;

public partial class RadialGauge : RangeBase
{
    void RadialGauge_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        SetGaugeValueFromPoint(e.Position);
    }

    void RadialGauge_Tapped(object sender, TappedRoutedEventArgs e)
    {
        SetGaugeValueFromPoint(e.GetPosition(this));
    }

    void RadialGauge_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (IsInteractive)
        {
            e.Handled = true;
        }
    }

    void SetKeyboardAccelerators()
    {
        // Small step
        AddKeyboardAccelerator(VirtualKeyModifiers.None, VirtualKey.Left, static (_, kaea) =>
        {
            if (kaea.Element is RadialGauge gauge)
            {
                gauge.Value = Math.Max(gauge.Minimum, gauge.Value - Math.Max(gauge.StepSize, gauge.SmallChange));
                kaea.Handled = true;
            }
        });

        AddKeyboardAccelerator(VirtualKeyModifiers.None, VirtualKey.Up, static (_, kaea) =>
        {
            if (kaea.Element is RadialGauge gauge)
            {
                gauge.Value = Math.Min(gauge.Maximum, gauge.Value + Math.Max(gauge.StepSize, gauge.SmallChange));
                kaea.Handled = true;
            }
        });

        AddKeyboardAccelerator(VirtualKeyModifiers.None, VirtualKey.Right, static (_, kaea) =>
        {
            if (kaea.Element is RadialGauge gauge)
            {
                gauge.Value = Math.Min(gauge.Maximum, gauge.Value + Math.Max(gauge.StepSize, gauge.SmallChange));
                kaea.Handled = true;
            }
        });

        AddKeyboardAccelerator(VirtualKeyModifiers.None, VirtualKey.Down, static (_, kaea) =>
        {
            if (kaea.Element is RadialGauge gauge)
            {
                gauge.Value = Math.Max(gauge.Minimum, gauge.Value - Math.Max(gauge.StepSize, gauge.SmallChange));
                kaea.Handled = true;
            }
        });

        // Large step
        AddKeyboardAccelerator(VirtualKeyModifiers.Control, VirtualKey.Left, static (_, kaea) =>
        {
            if (kaea.Element is RadialGauge gauge)
            {
                gauge.Value = Math.Max(gauge.Minimum, gauge.Value - Math.Max(gauge.StepSize, gauge.LargeChange));
                kaea.Handled = true;
            }
        });

        AddKeyboardAccelerator(VirtualKeyModifiers.Control, VirtualKey.Up, static (_, kaea) =>
        {
            if (kaea.Element is RadialGauge gauge)
            {
                gauge.Value = Math.Min(gauge.Maximum, gauge.Value + Math.Max(gauge.StepSize, gauge.LargeChange));
                kaea.Handled = true;
            }
        });

        AddKeyboardAccelerator(VirtualKeyModifiers.Control, VirtualKey.Right, static (_, kaea) =>
        {
            if (kaea.Element is RadialGauge gauge)
            {
                gauge.Value = Math.Min(gauge.Maximum, gauge.Value + Math.Max(gauge.StepSize, gauge.LargeChange));
                kaea.Handled = true;
            }
        });

        AddKeyboardAccelerator(VirtualKeyModifiers.Control, VirtualKey.Down, static (_, kaea) =>
        {
            if (kaea.Element is RadialGauge gauge)
            {
                gauge.Value = Math.Max(gauge.Minimum, gauge.Value - Math.Max(gauge.StepSize, gauge.LargeChange));
                kaea.Handled = true;
            }
        });
    }

    void AddKeyboardAccelerator(VirtualKeyModifiers keyModifiers, VirtualKey key, TypedEventHandler<KeyboardAccelerator, KeyboardAcceleratorInvokedEventArgs> handler)
    {
        var accelerator = new KeyboardAccelerator()
        {
            Modifiers = keyModifiers,
            Key = key
        };
        accelerator.Invoked += handler;
        KeyboardAccelerators.Add(accelerator);
    }
}