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
using TabApp.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TabApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Tab3Page : Page
    {
        DateTime _lastResize = DateTime.Now;
        public TabViewModel ViewModel { get; private set; }

        public Tab3Page()
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

            this.InitializeComponent();

            // Ensure that the Page is only created once, and cached during navigation.
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            ViewModel = App.GetService<TabViewModel>();

            if (!App.RootEventBus.IsSubscribed("SizeChangedEvent"))
                App.RootEventBus.Subscribe("SizeChangedEvent", EventBusMessageHandler);
        }

        /// <summary>
        /// Demonstrates changing the <see cref="Grid"/>'s <see cref="RowDefinition">s after a resize event.
        /// The issue with this technique is after the main window is loaded this page is not constructed yet,
        /// so the first event will be missed until this page is selected and another resize event occurs.
        /// </summary>
        void EventBusMessageHandler(object? sender, ObjectEventArgs e)
        {
            if (e.Payload == null)
            {
                Debug.WriteLine($"[WARNING] Received null event bus object!");
            }
            else if (e.Payload?.GetType() == typeof(Windows.Foundation.Size))
            {
                var size = (Windows.Foundation.Size)e.Payload;
                // Similar to our debouce helper.
                var update = DateTime.Now - _lastResize;
                if (update.TotalMilliseconds >= 250 && size.Height > 0)
                {
                    _lastResize = DateTime.Now;
                    var newRow = size.Height * 0.74d;
                    Debug.WriteLine($"[INFO] New row size will be {(int)newRow} ({DateTime.Now.ToString("hh:mm:ss.fff tt")})");
                    root.RowDefinitions.Clear();
                    root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });  // Row 0 with 0 height (auto)
                    root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(newRow, GridUnitType.Pixel) }); // Row 1 with n pixel height
                    root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });  // Row 2 with 1* star height
                }
                else
                {
                    Debug.WriteLine($"[WARNING] Skipping SizeChanged");
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
    }
}
