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

public sealed partial class Tab1Page : Page
{
    static DispatcherTimer? _timer;
    static double _maxHeight = 60;
    static double _showCount = 5;

    public TabViewModel ViewModel { get; private set; }

    public Tab1Page()
    {
        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        this.InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        DataListView.Loaded += DataListView_Loaded;

        ViewModel = App.GetService<TabViewModel>();

        if (!App.RootEventBus.IsSubscribed("BusyEvent"))
            App.RootEventBus.Subscribe("BusyEvent", EventBusMessageHandler);

        if (!App.RootEventBus.IsSubscribed("ItemAddedEvent"))
            App.RootEventBus.Subscribe("ItemAddedEvent", ItemAddedEventBusMessage);

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
                Debug.WriteLine($"[INFO] show count => {_showCount}");
            }
        };
        #endregion
    }

    /// <summary>
    /// A crude auto-scroll technique.
    /// </summary>
    void ItemAddedEventBusMessage(object? sender, ObjectEventArgs e)
    {
        if (e.Payload == null)
        {
            Debug.WriteLine($"[WARNING] Received null event bus object!");
        }
        else if (e.Payload?.GetType() == typeof(DataItem))
        {
            Debug.WriteLine($"[INFO] Scrolling to {((DataItem)e.Payload).Data}");
            //DataListView.ScrollIntoView(DataListView.Items.Count);
            DataListView.DispatcherQueue.TryEnqueue(() =>
            {
                int selectedIndex = DataListView.Items.Count - 1;
                if (selectedIndex < 0) { return; }
                DataListView.SelectedIndex = selectedIndex;
                DataListView.UpdateLayout();
                DataListView.ScrollIntoView((DataItem)DataListView.SelectedItem);
            });
        }
    }

    /// <summary>
    /// For <see cref="EventBus"/> model demonstration.
    /// </summary>
    void EventBusMessageHandler(object? sender, ObjectEventArgs e)
    {
        if (e.Payload == null)
        {
            Debug.WriteLine($"[WARNING] Received null event bus object!");
        }
        else if (e.Payload?.GetType() == typeof(Boolean))
        {
            if ((bool)e.Payload) // IsBusy set to true
            {
                if (App.AnimationsEffectsEnabled && !App.IsClosing)
                {
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
            else // IsBusy set to false
            {
                // Already running?
                if (_timer != null && _timer.IsEnabled)
                    return;
                else if (_timer != null)
                    _timer?.Start();
            }
        }
        else if (e.Payload?.GetType() == typeof(System.String))
        {
            Debug.WriteLine($"[INFO] Received EventBus Payload: {e.Payload}");
        }
        else
        {
            Debug.WriteLine($"[INFO] Received EventBus Payload of type: {e.Payload?.GetType()}");
        }
    }

    #region [Overrides]
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Debug.WriteLine($"[INFO] NavigatingTo Source => {e.SourcePageType}");
        base.OnNavigatedTo(e);
        if (App.AnimationsEffectsEnabled)
            OpacityStoryboard.Begin();
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        Debug.WriteLine($"[INFO] NavigatingFrom Source => {e.SourcePageType}");
        if (App.AnimationsEffectsEnabled)
            OpacityStoryboard.SkipToFill();
        base.OnNavigatingFrom(e);
    }
    #endregion

    void DataListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //VisualStateManager.GoToState(sender as Control, "Selected", false);

        // The IList<T> generic interface is a descendant of the ICollection<T>
        // generic interface and is the base interface of all generic lists.
        var list = e.AddedItems as IList<object>;

        // If there was no data model in the ItemTemplate...
        //foreach (ListViewItem item in TestView.Items) { Debug.WriteLine($"[ItemType] {item.GetType()}"); }

        // There could be multiple items in the IList, e.g. if SelectionMode="Multiple".
        foreach (var item in list)
        {
            if (item is DataItem di)
            {
                // Set the currently selected DataItem.
                ViewModel.SelectedItem = di;

                if (!string.IsNullOrEmpty(di.Data))
                {
                    if (!ViewModel.Option1)
                    {
                        return;
                    }
                    else
                    {
                        _ = App.ShowDialogBox($"{di.Title}", $"{di.Data}{Environment.NewLine}{Environment.NewLine}Created: {di.Created}{Environment.NewLine}Updated: {di.Updated}", "OK", "", null, null, new Uri($"ms-appx:///{App.AssetFolder}/Details.png"));
                        //_ = App.ShowMessageBox($"{di.Title}", $"{di.Data}{Environment.NewLine}Created: {di.Created}{Environment.NewLine}Updated: {di.Updated}", "OK", "", null, null);
                    }
                }
            }
        }

        //scrollViewer.ChangeView(horizontalOffset: 0, verticalOffset: scrollViewer.ScrollableHeight, zoomFactor: 1, disableAnimation: true);
    }

    /// <summary>
    /// Auto-scroll to last item in the list.
    /// </summary>
    void DataListView_Loaded(object sender, RoutedEventArgs e)
    {
        var dlv = (ListView)sender;
        
        if (dlv == null)
            return;

        DataListView.ScrollIntoView(dlv.Items.LastOrDefault());
    }
}
