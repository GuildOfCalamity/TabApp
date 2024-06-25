using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace TabApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        _ = App.GetStopWatch(true);

        Debug.WriteLine($"[DEBUG] {MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        this.InitializeComponent();

        this.Title = $"{App.GetCurrentNamespace()?.SeparateCamelCase()} Demo";
        // https://learn.microsoft.com/en-us/windows/apps/develop/title-bar?tabs=wasdk#full-customization
        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(CustomTitleBar);

        //if (GeneralExtensions.IsWindows11OrGreater())
        //    this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
        //else
        //    this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();

        this.SizeChanged += (sender, args) =>
        {
            if (!App.IsClosing)
            {
                Debug.WriteLine($"[INFO] MainWindow size changed to '{args.Size}'");
                //App.RootEventBus?.Publish("SizeChangedEvent", args.Size);
            }
        };

        this.VisibilityChanged += (sender, args) =>
        {
            if (!App.IsClosing)
            {
                Debug.WriteLine($"[INFO] MainWindow visibility changed to '{args.Visible}'");
                App.RootEventBus?.Publish("WindowVisibilityEvent", args.Visible);
            }
        };

        App.MainDispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        // We could fire an event when the display area changes using the watcher.
        // This could be used to update settings in the config our reposition the window.
        App.MainDispatcher?.CallOnUIThread(() =>
        {
            // Enumerates display areas and raises events when the collection of
            // display areas or the configuration of an individual DisplayArea changes.
            var dw = Microsoft.UI.Windowing.DisplayArea.CreateWatcher();
            // https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.displayareawatcher?view=windows-app-sdk-1.5#events
            dw.EnumerationCompleted += (s, e) => { Debug.WriteLine($"[INFO] DisplayWatcher ⇨ {s.Status}"); };
            dw.Updated += (s, e) =>
            {
                Debug.WriteLine($"[INFO] DisplayWatcher.IsPrimary ⇨ {e.IsPrimary}");
                Debug.WriteLine($"[INFO] DisplayWatcher.WorkArea.Width ⇨ {e.WorkArea.Width}");
                Debug.WriteLine($"[INFO] DisplayWatcher.WorkArea.Height ⇨ {e.WorkArea.Height}");
            };
            if (dw.Status != DisplayAreaWatcherStatus.Started) { dw.Start(); }
        });



        Debug.WriteLine($"[INFO] {nameof(MainWindow)} took {App.GetStopWatch().TotalMilliseconds:N1} milliseconds");
    }
}
