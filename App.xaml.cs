using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.UI.Popups;
using Windows.UI.ViewManagement;

using TabApp.ViewModels;
using Microsoft.UI.Windowing;
using TabApp.Helpers;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TabApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        #region [Props]
        Window? m_window;
        static UISettings m_UISettings = new UISettings();
        static ValueStopwatch vsw = ValueStopwatch.StartNew();
        public static Version WindowsVersion => GeneralExtensions.GetWindowsVersionUsingAnalyticsInfo();
        public static IntPtr WindowHandle { get; set; }
        public static FrameworkElement? MainRoot { get; set; }
        public static bool IsClosing { get; set; } = false;
        public static string AssetFolder { get; set; } = "Assets";
        public static Microsoft.UI.Dispatching.DispatcherQueue? MainDispatcher { get; set; }
        public static Microsoft.UI.Windowing.AppWindow? MainAppWindow { get; set; }

        public static event Action<Microsoft.UI.Windowing.AppWindow> OnWindowClosing = (appwin) => { };
        public static event Action<Microsoft.UI.Windowing.AppWindow> OnWindowDestroying = (appwin) => { };
        public static event Action<Microsoft.UI.Windowing.OverlappedPresenter> OnWindowMinMax = (presenter) => { };
        public static event Action<Microsoft.UI.Windowing.OverlappedPresenter> OnWindowOrderChanged = (presenter) => { };
        public static event Action<Windows.Graphics.PointInt32> OnWindowMove = (point) => { };
        public static event Action<Windows.Graphics.SizeInt32> OnWindowSizeChanged = (size) => { };

        public static EventBus RootEventBus { get; set; } = new();
        /// <summary>
        /// The .NET Microsoft.Extensions.Hosting.Host provides dependency injection, configuration, logging, and other services.
        /// https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        /// https://docs.microsoft.com/dotnet/core/extensions/configuration
        /// https://docs.microsoft.com/dotnet/core/extensions/generic-host
        /// https://docs.microsoft.com/dotnet/core/extensions/logging
        /// </summary>
        public Microsoft.Extensions.Hosting.IHost? Host { get; }

        // https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/#advantages-and-disadvantages-of-packaging-your-app
#if IS_UNPACKAGED // We're using a custom PropertyGroup Condition we defined in the csproj to help us with the decision.
        public static bool IsPackaged { get => false; }
#else
        public static bool IsPackaged { get => true; }
#endif

        // We won't configure backing fields for these as the user could adjust them during app lifetime.
        public static bool TransparencyEffectsEnabled
        {
            get => m_UISettings.AdvancedEffectsEnabled;
        }
        public static bool AnimationsEffectsEnabled
        {
            get => m_UISettings.AnimationsEnabled;
        }
        public static double TextScaleFactor
        {
            get => m_UISettings.TextScaleFactor;
        }
        public static bool AutoHideScrollbars
        {
            get
            {
                if (WindowsVersion.Major >= 10 && WindowsVersion.Build >= 18362)
                    return m_UISettings.AutoHideScrollBars;
                else
                    return true;
            }
        }
        public static ElementTheme ThemeRequested
        {
            get
            {
                if (App.IsPackaged)
                    return (ElementTheme)Enum.Parse(typeof(ElementTheme), Application.Current.RequestedTheme.ToString());
                else
                    return App.MainRoot?.ActualTheme ?? ElementTheme.Default;
            }
        }
        #endregion

        #region [Config]
        static bool _lastSave = false;
        static DateTime _lastMove = DateTime.Now;
        static Config? _localConfig;
        public static Config? LocalConfig
        {
            get => _localConfig;
            set => _localConfig = value;
        }

        public static Func<Config?, bool> SaveConfigFunc = (cfg) =>
        {
            if (cfg is not null)
            {
                Process proc = Process.GetCurrentProcess();
                cfg.firstRun = false;
                cfg.time = DateTime.Now;
                cfg.state = SystemStates.Shutdown;
                cfg.metrics = $"Process used {proc.PrivateMemorySize64 / 1024 / 1024}MB of memory and {proc.TotalProcessorTime.ToReadableString()} TotalProcessorTime on {Environment.ProcessorCount} possible cores.";
                if (App.MainRoot is not null)
                    cfg.theme = $"{App.MainRoot.ActualTheme}";
                try
                {
                    _ = ConfigHelper.SaveConfig(cfg);
                    return true;
                }
                catch (Exception) { return false; }
            }
            return false;
        };
        #endregion

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

            App.Current.DebugSettings.FailFastOnErrors = false;

            #region [New in SDK v1.5.x]
            // https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/stable-channel#improved-functionality-for-debugging-layout-cycles
            App.Current.DebugSettings.LayoutCycleTracingLevel = LayoutCycleTracingLevel.Low;
            App.Current.DebugSettings.LayoutCycleDebugBreakLevel = LayoutCycleDebugBreakLevel.Low;
            #endregion

            #region [Exception handlers]
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainFirstChanceException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            UnhandledException += ApplicationUnhandledException;
            #endregion

            // May not be needed with newer Microsoft.WindowsAppSDK ?
            WinRT.ComWrappersSupport.InitializeComWrappers();

            #region [Host Builder & Dependency Injection]
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseContentRoot(AppContext.BaseDirectory).ConfigureServices((context, services) =>
            {
                 services.AddSingleton<TabViewModel>();

                 // Dump all configs from Microsoft.Extensions.Configuration.IConfiguration
                 foreach (var cfg in context.Configuration.GetChildren())
                 {
                     Debug.WriteLine($"[INFO] {cfg.Key} ⇨ {cfg.Value}");
                 }
            }).
            Build();
            #endregion

            this.InitializeComponent();

            // https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.focusvisualkind?view=windows-app-sdk-1.3
            this.FocusVisualKind = FocusVisualKind.Reveal;

            if (Debugger.IsAttached)
            {
                this.DebugSettings.BindingFailed += DebugOnBindingFailed;
                this.DebugSettings.XamlResourceReferenceFailed += DebugOnXamlResourceReferenceFailed;
            }

            Debug.WriteLine($"[INFO] App Constructor took {vsw.GetElapsedTime().TotalMilliseconds:N1} milliseconds");
            vsw = ValueStopwatch.StartNew();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();

            var appWin = GetAppWindow(m_window);
            if (appWin != null)
            {
                MainAppWindow = appWin;

                // Gets or sets a value that indicates whether this window will appear in various system representations, such as ALT+TAB and taskbar.
                appWin.IsShownInSwitchers = true;

                // We don't have the Closing event exposed by default, so we'll use the AppWindow to compensate.
                appWin.Closing += (s, e) => 
                { 
                    App.IsClosing = true;
                    Debug.WriteLine($"[INFO] Application closing detected at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
                    OnWindowClosing?.Invoke(s);
                    _lastSave = SaveConfigFunc(LocalConfig);
                };

                // Destroying is always called, but Closing is only called when the application is shutdown normally.
                appWin.Destroying += (s, e) =>
                {
                    Debug.WriteLine($"[INFO] Application destroying detected at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
                    OnWindowDestroying?.Invoke(s);
                    if (!_lastSave) // prevent redundant calls
                        SaveConfigFunc(LocalConfig);
                };

                // The changed event holds a bunch of juicy info that we can extrapolate.
                appWin.Changed += (s, args) =>
                {
                    #region [Signal any listening events]
                    /*
                        DidPositionChange..... happens on a move
                        DidPresenterChange.... happens on a maximize/minimize
                        DidZOrderChange....... happens on a foreground change
                        DidSizeChange......... happens on size change
                        DidVisibilityChange... happens on AppWindow visibility
                    */
                    if (args.DidSizeChange) { OnWindowSizeChanged?.Invoke(s.Size); }
                    if (args.DidPositionChange) 
                    { 
                        OnWindowMove?.Invoke(s.Position);
                        // Add debounce in scenarios where this event could be hammered.
                        var idleTime = DateTime.Now - _lastMove;
                        if (idleTime.TotalSeconds > 2.0d && LocalConfig != null)
                        {
                            Debug.WriteLine($"[INFO] Updating window position to {s.Position.X},{s.Position.Y}");
                            _lastMove = DateTime.Now;
                            if (s.Position.X != 0 && s.Position.Y != 0)
                            {
                                LocalConfig.windowX = s.Position.X;
                                LocalConfig.windowY = s.Position.Y;
                                try
                                {
                                    // Recommended to be called on UI thread, but seems harmless so far.
                                    var da = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(App.WindowHandle), Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
                                    LocalConfig.primaryDisplay = da.IsPrimary;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[WARNING] DisplayArea.GetFromWindowId: {ex.Message}");
                                    LocalConfig.primaryDisplay = true;
                                }
                            }
                        }
                    }
                    // This property is initially null. Once a window has been shown it always has a
                    // presenter applied, either one applied by the platform or applied by the app itself.
                    if (args.DidPresenterChange && s.Presenter is not null) 
                    {
                        if (s.Presenter is Microsoft.UI.Windowing.OverlappedPresenter op)
                        {
                            Debug.WriteLine($"[INFO] OnWindowMinMax: {op.State}");
                            OnWindowMinMax?.Invoke(op);
                        }
                    }
                    if (args.DidZOrderChange && s.Presenter is not null) 
                    {
                        if (s.Presenter is Microsoft.UI.Windowing.OverlappedPresenter op)
                        {
                            Debug.WriteLine($"[INFO] OnWindowOrderChanged: {op.State}");
                            OnWindowOrderChanged?.Invoke(op);
                        }
                    }
                    #endregion
                };

                if (IsPackaged)
                    appWin.SetIcon(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, $"{AssetFolder}/TabIcon.ico"));
                else
                    appWin.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, $"{AssetFolder}/TabIcon.ico"));

                appWin.TitleBar.IconShowOptions = IconShowOptions.ShowIconAndSystemMenu;
            }

            // ShowOnceWithRequestedStartupState is the equivalent of calling ShowWindow(SW_SHOWDEFAULT).
            // It uses the show mode specified in the STARTUPINFO struct, if specified. This applies to
            // the default presenter (OverlappedPresenter). Check the OverlappedPresenter.RequestedStartupState
            // property to determine the presenter state (Maximized, Minimized, or Restored) that will result
            // from calling the ShowOnceWithRequestedStartupState method.
            // https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.appwindow.showoncewithrequestedstartupstate?view=windows-app-sdk-1.5
            //appWin?.ShowOnceWithRequestedStartupState();

            m_window.Activate();

            // Save the FrameworkElement for any future content dialogs.
            MainRoot = m_window.Content as FrameworkElement;

            #region [Load Config]
            Task.Run(async () =>
            {
                if (ConfigHelper.DoesConfigExist())
                {
                    try
                    {
                        LocalConfig = await ConfigHelper.LoadConfig();
                        if (LocalConfig != null)
                        {
                            Debug.WriteLine($"[INFO] Moving window to previous position {LocalConfig.windowX},{LocalConfig.windowY}");
                            //appWin?.Move(new Windows.Graphics.PointInt32(LocalConfig.windowX, LocalConfig.windowY));
                            appWin?.MoveAndResize(new Windows.Graphics.RectInt32(LocalConfig.windowX, LocalConfig.windowY, 1100, 700), Microsoft.UI.Windowing.DisplayArea.Primary);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] {nameof(ConfigHelper.LoadConfig)}: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        LocalConfig = new Config
                        {
                            firstRun = true,
                            theme = $"{App.ThemeRequested}",
                            version = $"{App.GetCurrentAssemblyVersion()}",
                            time = DateTime.Now,
                            state = SystemStates.Init,
                            metrics = "N/A",
                        };
                        await ConfigHelper.SaveConfig(LocalConfig);

                        appWin?.Resize(new Windows.Graphics.SizeInt32(1100, 700));
                        CenterWindow(m_window);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] {nameof(ConfigHelper.SaveConfig)}: {ex.Message}");
                    }
                }
            });
            #endregion
        }

        /// <summary>
        /// Uses the <see cref="Microsoft.Extensions.Hosting.IHost"/>
        /// and <see cref="System.IServiceProvider"/> to return a service
        /// object of type <typeparamref name="T"/> -or- null if there is
        /// no service object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">the service type</typeparam>
        public static T GetService<T>() where T : class
        {
            if (typeof(T).IsGenericType)
                Debug.WriteLine($"[NOTE] '{typeof(T).Name}' is a generic type.");
            else
                Debug.WriteLine($"[NOTE] '{typeof(T).Name}' is not a generic type.");

            if ((App.Current as App)!.Host?.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }

        /// <summary>
        /// Simplified debug logger for app-wide use.
        /// </summary>
        /// <param name="message">the text to append to the file</param>
        public static void DebugLog(string message)
        {
            try
            {
                if (App.IsPackaged)
                    System.IO.File.AppendAllText(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Debug.log"), $"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}] {message}{Environment.NewLine}");
                else
                    System.IO.File.AppendAllText(System.IO.Path.Combine(System.AppContext.BaseDirectory, "Debug.log"), $"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}] {message}{Environment.NewLine}");
            }
            catch (Exception)
            {
                Debug.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}] {message}");
            }
        }

        public static TimeSpan GetStopWatch(bool reset = false)
        {
            var ts = vsw.GetElapsedTime();
            if (reset) { vsw = ValueStopwatch.StartNew(); }
            return ts;
        }

        #region [Window Helpers]
        /// <summary>
        /// This code example demonstrates how to retrieve an AppWindow from a WinUI3 window.
        /// The AppWindow class is available for any top-level HWND in your app.
        /// AppWindow is available only to desktop apps (both packaged and unpackaged), it's not available to UWP apps.
        /// https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/windowing/windowing-overview
        /// https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.appwindow.create?view=windows-app-sdk-1.3
        /// </summary>
        public Microsoft.UI.Windowing.AppWindow? GetAppWindow(object window)
        {
            // Retrieve the window handle (HWND) of the current (XAML) WinUI3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // For other classes to use (mostly P/Invoke).
            App.WindowHandle = hWnd;

            // Retrieve the WindowId that corresponds to hWnd.
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            // Lastly, retrieve the AppWindow for the current (XAML) WinUI3 window.
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            return appWindow;
        }

        /// <summary>
        /// Centers a <see cref="Microsoft.UI.Xaml.Window"/> based on the <see cref="Microsoft.UI.Windowing.DisplayArea"/>.
        /// </summary>
        public void CenterWindow(Window window)
        {
            try
            {
                System.IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                if (Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId) is Microsoft.UI.Windowing.AppWindow appWindow &&
                    Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest) is Microsoft.UI.Windowing.DisplayArea displayArea)
                {
                    Windows.Graphics.PointInt32 CenteredPosition = appWindow.Position;
                    CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
                    CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
                    appWindow.Move(CenteredPosition);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// The <see cref="Microsoft.UI.Windowing.DisplayArea"/> exposes properties such as:
        /// OuterBounds     (Rect32)
        /// WorkArea.Width  (int)
        /// WorkArea.Height (int)
        /// IsPrimary       (bool)
        /// DisplayId.Value (ulong)
        /// </summary>
        /// <param name="window"></param>
        /// <returns><see cref="DisplayArea"/></returns>
        public Microsoft.UI.Windowing.DisplayArea? GetDisplayArea(Window window)
        {
            try
            {
                System.IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var da = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
                return da;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetDisplayArea: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region [Debugger Events]
        void DebugOnXamlResourceReferenceFailed(DebugSettings sender, XamlResourceReferenceFailedEventArgs args)
        {
            Debug.WriteLine($"[WARNING] XamlResourceReferenceFailed: {args.Message}");
            DebugLog($"OnXamlResourceReferenceFailed: {args.Message}");
        }

        void DebugOnBindingFailed(object sender, BindingFailedEventArgs args)
        {
            Debug.WriteLine($"[WARNING] BindingFailed: {args.Message}");
            DebugLog($"OnBindingFailed: {args.Message}");
        }
        #endregion

        #region [Domain Events]
        void ApplicationUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // TODO: Log and handle exceptions as appropriate.
            // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
            Exception? ex = e.Exception;
            Debug.WriteLine($"[UnhandledException]: {ex?.Message}");
            Debug.WriteLine($"Unhandled exception of type {ex?.GetType()}: {ex}");
            DebugLog($"Unhandled Exception StackTrace: {Environment.StackTrace}");
            DebugLog($"{ex?.DumpFrames()}");
            e.Handled = true;
        }

        void CurrentDomainOnProcessExit(object? sender, EventArgs e)
        {
            if (!IsClosing)
                IsClosing = true;

            if (sender is null)
                return;

            if (sender is AppDomain ad)
            {
                Debug.WriteLine($"[OnProcessExit]", $"{nameof(App)}");
                Debug.WriteLine($"DomainID: {ad.Id}", $"{nameof(App)}");
                Debug.WriteLine($"FriendlyName: {ad.FriendlyName}", $"{nameof(App)}");
                Debug.WriteLine($"BaseDirectory: {ad.BaseDirectory}", $"{nameof(App)}");
            }
        }

        void CurrentDomainFirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Debug.WriteLine($"[ERROR] First chance exception from {sender?.GetType()}: {e.Exception.Message}");
            DebugLog($"First chance exception from {sender?.GetType()}: {e.Exception.Message}");
            if (e.Exception.InnerException != null)
                DebugLog($"  ⇨ InnerException: {e.Exception.InnerException.Message}");
            DebugLog($"First Chance Exception StackTrace: {Environment.StackTrace}");
            DebugLog($"{e.Exception.DumpFrames()}");
        }

        void CurrentDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            Exception? ex = e.ExceptionObject as Exception;
            Debug.WriteLine($"[ERROR] Thread exception of type {ex?.GetType()}: {ex}");
            DebugLog($"Thread exception of type {ex?.GetType()}: {ex}");
            DebugLog($"{ex?.DumpFrames()}");
        }

        void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception is AggregateException aex)
            {
                aex?.Flatten().Handle(ex =>
                {
                    Debug.WriteLine($"[ERROR] Unobserved task exception: {ex?.Message}");
                    DebugLog($"Unobserved task exception: {ex?.Message}");
                    DebugLog($"{ex?.DumpFrames()}");
                    return true;
                });
            }
            e.SetObserved(); // suppress and handle manually
        }
        #endregion

        #region [Reflection Helpers]
        /// <summary>
        /// Returns the declaring type's namespace.
        /// </summary>
        public static string? GetCurrentNamespace() => System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace;

        /// <summary>
        /// Returns the declaring type's assembly name.
        /// Similar ⇨ System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly.FullName
        /// </summary>
        public static string? GetCurrentAssemblyName() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// Returns the AssemblyVersion, not the FileVersion.
        /// </summary>
        public static Version GetCurrentAssemblyVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
        #endregion

        #region [Dialog Helpers]
        static SemaphoreSlim semaSlim = new SemaphoreSlim(1, 1);
        /// <summary>
        /// The <see cref="Windows.UI.Popups.MessageDialog"/> does not look as nice as the
        /// <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> and is not part of the native Microsoft.UI.Xaml.Controls.
        /// The <see cref="Windows.UI.Popups.MessageDialog"/> offers the <see cref="Windows.UI.Popups.UICommandInvokedHandler"/> 
        /// callback, but this could be replaced with actions. Both can be shown asynchronously.
        /// </summary>
        /// <remarks>
        /// You'll need to call <see cref="WinRT.Interop.InitializeWithWindow.Initialize"/> when using the <see cref="Windows.UI.Popups.MessageDialog"/>,
        /// because the <see cref="Microsoft.UI.Xaml.XamlRoot"/> does not exist and an owner must be defined.
        /// </remarks>
        public static async Task ShowMessageBox(string title, string message, string yesText, string noText, Action? yesAction, Action? noAction)
        {
            if (App.WindowHandle == IntPtr.Zero) { return; }

            // Create the dialog.
            var messageDialog = new MessageDialog($"{message}");
            messageDialog.Title = title;

            if (!string.IsNullOrEmpty(yesText))
            {
                messageDialog.Commands.Add(new UICommand($"{yesText}", (opt) => { yesAction?.Invoke(); }));
                messageDialog.DefaultCommandIndex = 0;
            }

            if (!string.IsNullOrEmpty(noText))
            {
                messageDialog.Commands.Add(new UICommand($"{noText}", (opt) => { noAction?.Invoke(); }));
                messageDialog.DefaultCommandIndex = 1;
            }

            // We must initialize the dialog with an owner.
            WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, App.WindowHandle);
            // Show the message dialog. Our DialogDismissedHandler will deal with what selection the user wants.
            await messageDialog.ShowAsync();
            // We could force the result in a separate timer...
            //DialogDismissedHandler(new UICommand("time-out"));
        }

        /// <summary>
        /// The <see cref="Windows.UI.Popups.MessageDialog"/> does not look as nice as the
        /// <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> and is not part of the native Microsoft.UI.Xaml.Controls.
        /// The <see cref="Windows.UI.Popups.MessageDialog"/> offers the <see cref="Windows.UI.Popups.UICommandInvokedHandler"/> 
        /// callback, but this could be replaced with actions. Both can be shown asynchronously.
        /// </summary>
        /// <remarks>
        /// You'll need to call <see cref="WinRT.Interop.InitializeWithWindow.Initialize"/> when using the <see cref="Windows.UI.Popups.MessageDialog"/>,
        /// because the <see cref="Microsoft.UI.Xaml.XamlRoot"/> does not exist and an owner must be defined.
        /// </remarks>
        public static async Task ShowMessageBox(string title, string message, string primaryText, string cancelText)
        {
            // Create the dialog.
            var messageDialog = new MessageDialog($"{message}");
            messageDialog.Title = title;

            if (!string.IsNullOrEmpty(primaryText))
            {
                messageDialog.Commands.Add(new UICommand($"{primaryText}", new UICommandInvokedHandler(DialogDismissedHandler)));
                messageDialog.DefaultCommandIndex = 0;
            }

            if (!string.IsNullOrEmpty(cancelText))
            {
                messageDialog.Commands.Add(new UICommand($"{cancelText}", new UICommandInvokedHandler(DialogDismissedHandler)));
                messageDialog.DefaultCommandIndex = 1;
            }
            // We must initialize the dialog with an owner.
            WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, App.WindowHandle);
            // Show the message dialog. Our DialogDismissedHandler will deal with what selection the user wants.
            await messageDialog.ShowAsync();

            // We could force the result in a separate timer...
            //DialogDismissedHandler(new UICommand("time-out"));
        }

        /// <summary>
        /// Callback for the selected option from the user.
        /// </summary>
        static void DialogDismissedHandler(IUICommand command)
        {
            Debug.WriteLine($"UICommand.Label ⇨ {command.Label}");
        }

        /// <summary>
        /// The <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> looks much better than the
        /// <see cref="Windows.UI.Popups.MessageDialog"/> and is part of the native Microsoft.UI.Xaml.Controls.
        /// The <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> does not offer a <see cref="Windows.UI.Popups.UICommandInvokedHandler"/>
        /// callback, but in this example was replaced with actions. Both can be shown asynchronously.
        /// </summary>
        /// <remarks>
        /// There is no need to call <see cref="WinRT.Interop.InitializeWithWindow.Initialize"/> when using the <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/>,
        /// but a <see cref="Microsoft.UI.Xaml.XamlRoot"/> must be defined since it inherits from <see cref="Microsoft.UI.Xaml.Controls.Control"/>.
        /// The <see cref="SemaphoreSlim"/> was added to prevent "COMException: Only one ContentDialog can be opened at a time."
        /// </remarks>
        public static async Task ShowDialogBox(string title, string message, string primaryText, string cancelText, Action? onPrimary, Action? onCancel, Uri? imageUri)
        {
            if (App.MainRoot?.XamlRoot == null) { return; }

            await semaSlim.WaitAsync(); 

            #region [Initialize Assets]
            double fontSize = 16;
            Microsoft.UI.Xaml.Media.FontFamily fontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas");

            if (App.Current.Resources["FontSizeMedium"] is not null)
                fontSize = (double)App.Current.Resources["FontSizeMedium"];

            if (App.Current.Resources["PrimaryFont"] is not null)
                fontFamily = (Microsoft.UI.Xaml.Media.FontFamily)App.Current.Resources["PrimaryFont"];

            StackPanel panel = new StackPanel() 
            { 
                Orientation = Microsoft.UI.Xaml.Controls.Orientation.Vertical, 
                Spacing = 10d 
            };

            if (imageUri is not null)
            {
                panel.Children.Add(new Image
                {
                    Margin = new Thickness(1, -45, 1, 1), // Move the image into the title area.
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                    Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill,
                    Width = 48,
                    Height = 48,
                    Source = new BitmapImage(imageUri)
                });
            }
            
            panel.Children.Add(new TextBlock() 
            { 
                Text = message, 
                FontSize = fontSize,
                FontFamily = fontFamily,
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left, 
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap 
            });

            var tb = new TextBox()
            {
                Text = message,
                FontSize = fontSize,
                FontFamily = fontFamily,
                TextWrapping = TextWrapping.Wrap
            };
            tb.Loaded += (s, e) => { tb.SelectAll(); };
            #endregion

            // NOTE: Content dialogs will automatically darken the background.
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = title,
                PrimaryButtonText = primaryText,
                CloseButtonText = cancelText,
                Content = panel,
                XamlRoot = App.MainRoot?.XamlRoot,
                RequestedTheme = App.MainRoot?.ActualTheme ?? ElementTheme.Default
            };

            try
            {
                ContentDialogResult result = await contentDialog.ShowAsync();

                switch (result)
                {
                    case ContentDialogResult.Primary:
                        onPrimary?.Invoke();
                        break;
                    //case ContentDialogResult.Secondary:
                    //    onSecondary?.Invoke();
                    //    break;
                    case ContentDialogResult.None: // Cancel
                        onCancel?.Invoke();
                        break;
                    default:
                        Debug.WriteLine($"Dialog result not defined.");
                        break;
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Debug.WriteLine($"[ERROR] ShowDialogBox: {ex.Message}");
            }
            finally
            {
                semaSlim.Release();
            }
        }
        #endregion
    }
}
