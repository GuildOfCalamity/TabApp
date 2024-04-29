using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.UI.Xaml;

namespace TabApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        _ = App.GetStopWatch(true);

        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        this.InitializeComponent();

        this.Title = $"{App.GetCurrentNamespace()?.SeparateCamelCase()} Demo";
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
                App.RootEventBus?.Publish("SizeChangedEvent", args.Size);
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

        Debug.WriteLine($"[INFO] {nameof(MainWindow)} took {App.GetStopWatch().TotalMilliseconds:N1} milliseconds");
    }
}
