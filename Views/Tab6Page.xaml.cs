using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TabApp.Models;
using TabApp.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace TabApp.Views;

public sealed partial class Tab6Page : Page
{
    static DispatcherTimer? _timer;
    static double _maxHeight = 60;
    static double _showCount = 5;

    public TabViewModel ViewModel { get; private set; }

    public Tab6Page()
    {
        Debug.WriteLine($"[DEBUG] {MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        this.InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        ViewModel = App.GetService<TabViewModel>();

        #region [Animated InfoBar]
        ibar.Closing += (s, e) =>
        {
            if (_timer != null && _timer.IsEnabled)
            {
                Debug.WriteLine($"[INFO] InfoBar closing, stopping timer at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
                _timer.Stop();
                _showCount = 5;
            }
        };

        StoryboardInfoBar.Completed += (s, e) =>
        {
            Debug.WriteLine($"[INFO] {nameof(StoryboardInfoBar)} completed at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
        };

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1d);
        _timer.Tick += (_, _) =>
        {
            if (_showCount-- <= 0)
            {
                try
                {
                    ibar.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                    {
                        // TODO: Reentry animation handling.

                        if (App.AnimationsEffectsEnabled && !App.IsClosing)
                        {
                            StoryboardOpacity.Children[0].SetValue(DoubleAnimation.FromProperty, ibar.Opacity);
                            StoryboardOpacity.Children[0].SetValue(DoubleAnimation.ToProperty, 0d);
                            StoryboardOpacity.Begin();

                            StoryboardInfoBar.Children[0].SetValue(DoubleAnimation.FromProperty, TranslationInfoBar.Y);
                            StoryboardInfoBar.Children[0].SetValue(DoubleAnimation.ToProperty, _maxHeight * -1);
                            StoryboardInfoBar.Begin();
                        }
                    });

                    if (_timer != null)
                    {
                        // Wait for animations to finished before hiding the InfoBar.
                        Task.Run(async () =>
                        {
                            await Task.Delay(850);
                            if (!App.IsClosing)
                            {
                                ibar.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                                {
                                    ibar.IsOpen = false;
                                });
                            }
                        });
                        _timer.Stop();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine($"[WARNING] Application may be in the process of closing.");
                }
            }
            else
            {
                Debug.WriteLine($"[INFO] show count ⇨ {_showCount}");
            }
        };

        if (!App.RootEventBus.IsSubscribed("CPUEvent"))
            App.RootEventBus.Subscribe("CPUEvent", EventBusMessageHandler);
        #endregion
    }

    /// <summary>
    /// We'll only trigger the InfoBar when a <see cref="NamedColor"/> is selected in the ListView.
    /// </summary>
    void EventBusMessageHandler(object? sender, ObjectEventArgs e)
    {
        if (e.Payload == null)
        {
            Debug.WriteLine($"[WARNING] Received null event bus object!");
        }
        else if (e.Payload?.GetType() == typeof(NamedColor))
        {
            var msg = e.Payload as NamedColor;
            if (msg != null)
            {
                // TODO: Reentry animation handling.

                if (App.AnimationsEffectsEnabled && !App.IsClosing)
                {
                    ViewModel.PopupText = $"Width ⇨ {(int)msg.Width}";

                    StoryboardOpacity.Children[0].SetValue(DoubleAnimation.FromProperty, 0d);
                    StoryboardOpacity.Children[0].SetValue(DoubleAnimation.ToProperty, 1d);
                    StoryboardOpacity.Begin();

                    StoryboardInfoBar.Children[0].SetValue(DoubleAnimation.FromProperty, _maxHeight /* TranslationInfoBar.Y */);
                    StoryboardInfoBar.Children[0].SetValue(DoubleAnimation.ToProperty, 0d);
                    StoryboardInfoBar.Begin();

                    ibar.IsOpen = true;
                }

                // If started again, but timer is not finished.
                if (_timer != null && _timer.IsEnabled)
                {
                    Debug.WriteLine($"[INFO] Resetting timer at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
                    _timer.Stop();
                    _showCount = 5;
                    _timer.Start();
                }
                else if (_timer != null && !_timer.IsEnabled)
                {
                    Debug.WriteLine($"[INFO] Starting timer at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
                    _showCount = 5;
                    _timer.Start();
                }
            }
            else
            {
                // Already running?
                if (_timer != null && _timer.IsEnabled)
                    return;
                else if (_timer != null)
                    _timer?.Start();
            }
        }
        else
        {
            Debug.WriteLine($"[INFO] Received EventBus Payload of type: {e.Payload?.GetType()}");
        }
    }

    #region [Overrides]
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Debug.WriteLine($"[INFO] NavigatingTo Source ⇨ {e.SourcePageType}");

        if (e.Parameter != null && e.Parameter is SystemStates sys)
            Debug.WriteLine($"[INFO] Received system state '{sys}'");

        if (App.AnimationsEffectsEnabled)
            OpacityStoryboard.Begin();

        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        Debug.WriteLine($"[INFO] NavigatingFrom Source ⇨ {e.SourcePageType}");

        if (App.AnimationsEffectsEnabled)
            OpacityStoryboard.SkipToFill();

        base.OnNavigatingFrom(e);
    }
    #endregion
}