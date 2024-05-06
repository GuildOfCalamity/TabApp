using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TabApp.Helpers;
using TabApp.Models;


namespace TabApp.ViewModels;

public class TabViewModel : ObservableRecipient
{
    #region [Props]
    bool _option1 = false;
    bool _option2 = true;
    bool _option3 = true;
    bool _canThrowError = false;
    int _maxCPU = 100;
    int _currentCPU = 0;
    int _counter = 0;
    bool _isBusy = false;
    string _popupText = "...";
    SystemStates _systemState = SystemStates.None;
    DataItem? _selectedItem;
    
    DateTime _lastMove = DateTime.Now;
    static DispatcherTimer? _timer;
    static bool _verbose = false;
    SolidColorBrush _needleColor = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
    // Only possible due to our System.Diagnostics.PerformanceCounter NuGet (sadly .NET Core does not offer the PerformanceCounter)
    System.Diagnostics.PerformanceCounter? _perfCPU;
    SolidColorBrush _level1;
    SolidColorBrush _level2;
    SolidColorBrush _level3;
    SolidColorBrush _level4;
    SolidColorBrush _level5;
    SolidColorBrush _level6;
    NamedColor? _selectedColor = new NamedColor { Color = Colors.DodgerBlue, Time = DateTime.Now.ToString(), Amount = "0%", Width = 1d };

    public SolidColorBrush NeedleColor
    {
        get => _needleColor;
        set => SetProperty(ref _needleColor, value);
    }

    public SystemStates SystemState
    {
        get => _systemState;
        set => SetProperty(ref _systemState, value);
    }

    public bool Option1
    {
        get => _option1;
        set => SetProperty(ref _option1, value);
    }

    public bool Option2
    {
        get => _option2;
        set => SetProperty(ref _option2, value);
    }

    public bool Option3
    {
        get => _option3;
        set => SetProperty(ref _option3, value);
    }

    public int MaxCPU
    {
        get => _maxCPU;
        set => SetProperty(ref _maxCPU, value);
    }

    public int CurrentCPU
    {
        get => _currentCPU;
        set => SetProperty(ref _currentCPU, value);
    }

    public int Counter
    {
        get => _counter;
        set => SetProperty(ref _counter, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (value)
                SystemState = SystemStates.Processing;
            else
                SystemState = SystemStates.Ready;

            App.RootEventBus?.Publish("BusyEvent", value);
            SetProperty(ref _isBusy, value);
        }
    }

    public string PopupText
    {
        get => _popupText;
        set => SetProperty(ref _popupText, value);
    }

    public ObservableCollection<DataItem> DataItems = new(); //public ObservableCollection<DataItem> DataItems = new();
    public DataItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value); 
    }

    public ObservableCollection<NamedColor> NamedColors = new();
    public NamedColor? SelectedColor
    {
        get => _selectedColor;
        set 
        {
            App.RootEventBus?.Publish("CPUEvent", value); // Fire event after user selects list item.
            SetProperty(ref _selectedColor, value); 
        }
    }

    public bool CanThrowError
    {
        get => _canThrowError;
        set
        {
            SetProperty(ref _canThrowError, value);
            ThrowExCommand?.NotifyCanExecuteChanged();
        }
    }

    private DataItem? _scrollToItem;
    public DataItem? ScrollToItem
    {
        get => _scrollToItem;
        set => SetProperty(ref _scrollToItem, value);
    }


    public ICommand SampleCommand { get; }
    public RelayCommand? ThrowExCommand { get; }

    static readonly List<string> _emojis = new List<string> 
    { 
        "✔️","👍","👌","🧹","🧯","🛒","💼","🎛️","📂","🗂️",
        "🔨","⛏️","⚒️","🛠️","🗡️","⚔️","💣","🏹","🛡️","🔧",
        "🔩","⚙️","🗜️","⚖️","🔗","💰","🧰","🧲","📌","🧪",
        "🧫","🔬","🔭","📡","🔈","🔉","🔊","📢","📣","📯",
        "🔔","🔕","🎼","🎵","🎶","🎙️","🎚️","🎛️","🎤","🎧",
        "📻","🎷","🎸","🗝️","🎺","🎻","⏰","📚","🖨️","📦"
    };
    #endregion

    public TabViewModel()
    {
        _ = App.GetStopWatch(true);

        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        SystemState = SystemStates.Init;

        if (!App.RootEventBus.IsSubscribed("WindowVisibilityEvent"))
            App.RootEventBus.Subscribe("WindowVisibilityEvent", EventBusMessageHandler);

        // Populate the observable list with data.
        DataItems = GenerateDefaultItems();

        Debug.WriteLine($"[INFO] DispatcherShutdownMode is {(Application.Current as App)?.DispatcherShutdownMode}");

        // Sample command for flyouts, et al.
        SampleCommand = new RelayCommand<object>(async (obj) =>
        {
            IsBusy = true;

            if (DataItems.Count > 100) { DataItems.Clear(); }

            if (obj is MenuFlyoutItem mfi)
            {
                if (mfi != null)
                    AddDataItem($"INFO 📢", $"Got MenuFlyoutItem \"{mfi.Text}\"");
            }
            else if (obj is AppBarButton abb)
            {
                if (abb != null)
                    AddDataItem($"INFO 📢", $"Got AppBarButton \"{abb.Label}\"");
            }
            else if (obj is Button btn)
            {
                if (btn != null)
                    AddDataItem($"INFO 📢", $"Got Button \"{btn.Content}\"");

                var installRoot = App.ContentRootPath;
                var files = new List<FileInfo>();
                var dir = new DirectoryInfo(installRoot);
                if (dir.Exists)
                {
                    var name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                    var results = dir.GetFiles($"{name}*.*", SearchOption.TopDirectoryOnly);
                    foreach (var file in results)
                    {
                        AddDataItem($"INFO 📢", $"{file.Name}  v{file.FullName.GetFileVersion()}");
                    }

                    files.AddRange(dir.GetFiles($"{name}.exe", SearchOption.AllDirectories));
                    files.AddRange(dir.GetFiles($"{name}.dll", SearchOption.AllDirectories));
                    files = files.Where(o => !o.FullName.Contains(Path.Combine(dir.FullName, ".backup"))).ToList();
                    foreach (var file in files)
                    {
                        AddDataItem($"INFO 📢", $"{file.Name}  v{file.FullName.GetFileVersion()}");
                    }
                }

                //App.MessageProcessor.Enqueue(new MessageEntry { Level = "INFO", Message = $"Got Button \"{btn.Content}\"" });
            }
            else if (obj is Page pg)
            {
                if (pg != null)
                    AddDataItem($"INFO 📢", $"Got Page (using font {pg.FontFamily.Source})");
            }
            else if (obj is String str)
            {
                if (!string.IsNullOrEmpty(str))
                    AddDataItem($"INFO 📢", $"Got Behavior event \"{str}\"");
            }
            else if (obj is Microsoft.Xaml.Interactions.Core.InvokeCommandAction cmd)
            {
                if (cmd != null)
                    AddDataItem($"INFO 📢", $"Got Behavior InvokeCommandAction");
                
            }
            else if (obj is DataItem di)
            {
                if (di != null)
                    AddDataItem($"INFO 📢", $"Got DataItem created {di.Created}");
            }
            else
            {
                AddDataItem($"WARNING 📢", $"No action defined for type '{obj?.GetType()}'");
            }

            await Task.Delay(3000);

            IsBusy = false;
        });

        // Throw command for exceptions.
        ThrowExCommand = new RelayCommand(async () => await ThrowErrorAsync(), () => CanThrowError);

        TaskTimer.Start(async () =>
        {
            // Instantiating a PerformanceCounter can take up to 20+ seconds, so we'll queue this on another thread.
            await Task.Run(() =>
            {
                if (_perfCPU == null)
                    _perfCPU = new System.Diagnostics.PerformanceCounter("Processor Information", "% Processor Time", "_Total", true);
            });

        }).ContinueWith((t) =>
        {
            //AddDataItem("DEBUG", $"⏱️ PerformanceCounter took {t.Result.Duration.TotalSeconds:N1} seconds to initialize");
            Debug.WriteLine($"[INFO] ⏱️ PerformanceCounter took {t.Result.Duration.TotalSeconds:N1} seconds to initialize.");
        });

        // CPU usage with RadialGauge.
        _level6 = new SolidColorBrush(Microsoft.UI.Colors.OrangeRed)   { Opacity = 0.7 };
        _level5 = new SolidColorBrush(Microsoft.UI.Colors.Orange)      { Opacity = 0.7 };
        _level4 = new SolidColorBrush(Microsoft.UI.Colors.Yellow)      { Opacity = 0.6 };
        _level3 = new SolidColorBrush(Microsoft.UI.Colors.YellowGreen) { Opacity = 0.7 };
        _level2 = new SolidColorBrush(Microsoft.UI.Colors.SpringGreen) { Opacity = 0.7 };
        _level1 = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue)   { Opacity = 0.7 };
        if (_timer == null)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(2000);
            _timer.Tick += (_, _) =>
            {
                if (!App.IsClosing)
                    CurrentCPU = (int)GetCPU();
                else
                    _timer?.Stop();
            };
            _timer?.Start();
        }

        #region [Heartbeat Simulation]
        //SystemState = SystemStates.Ready;
        ThreadPool.QueueUserWorkItem((object? state) =>
        {
            while (!App.IsClosing)
            {
                Thread.Sleep(2900);

                if (!IsBusy && !App.IsClosing)
                {
                    App.MainDispatcher?.CallOnUIThread(() =>
                    {
                        SystemState = SystemStates.Processing;
                        CanThrowError = false;
                    });
                }
                
                Thread.Sleep(400);

                if (!IsBusy && !App.IsClosing)
                {
                    App.MainDispatcher?.CallOnUIThread(() =>
                    {
                        SystemState = SystemStates.Ready;
                        CanThrowError = true;
                    });
                }
            }
            Debug.WriteLine($"[INFO] Heartbeat thread loop exit.");
        });
        #endregion

        Debug.WriteLine($"[INFO] {nameof(TabViewModel)} took {App.GetStopWatch().TotalMilliseconds:N1} milliseconds");
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
        else if (e.Payload?.GetType() == typeof(Boolean)) // IsVisible Property
        {
            // We'll only run CPU timer when not minimized.
            if (!(bool)e.Payload)
                _timer?.Stop();
            else
                _timer?.Start();
        }
    }

    public void SetState(SystemStates state)
    {
        if (!App.IsClosing)
            App.MainDispatcher?.CallOnUIThread(() => { SystemState = state; });
    }

    /// <summary>
    /// We're using this to send a signal to the UI that an item has 
    /// been added so common routines like auto-scroll can be picked up.
    /// </summary>
    void AddDataItem(string title, string text)
    {
        var di = new DataItem { Title = title, Data = $"{text}", Created = DateTime.Now.AddSeconds(-1), Updated = DateTime.Now };

        #region [Auto-scroll attempt]
        ScrollToItem = di;
        //App.RootEventBus.Publish("ItemAddedEvent", di);
        #endregion

        App.MainDispatcher?.CallOnUIThread(() => { DataItems.Add(di); });
    }

    /// <summary>
    /// Creates a new <see cref="List{T}"/> object with example data.
    /// </summary>
    /// <returns><see cref="List{T}"/></returns>
    ObservableCollection<DataItem> GenerateDefaultItems()
    {
        return new ObservableCollection<DataItem>
        {
            new DataItem { Title = "Title #1", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-4), Updated = DateTime.Now.AddDays(-2), },
            new DataItem { Title = "Title #2", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-30), Updated = DateTime.Now.AddDays(-4), },
            new DataItem { Title = "Title #3", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-60), Updated = DateTime.Now.AddDays(-6), },
            new DataItem { Title = "Title #4", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-90), Updated = DateTime.Now.AddDays(-8), },
            //new DataItem { Title = "Title #5", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-10), },
            //new DataItem { Title = "Title #6", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-12), },
            //new DataItem { Title = "Title #7", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-14), },
            //new DataItem { Title = "Title #8", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-16), },
            //new DataItem { Title = "Title #9", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-18), },
            //new DataItem { Title = "Title #10", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-20), },
            //new DataItem { Title = "Title #11", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-22), },
            //new DataItem { Title = "Title #12", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-24), },
            //new DataItem { Title = "Title #13", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-26), },
            //new DataItem { Title = "Title #14", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-28), },
            //new DataItem { Title = "Title #15", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-31), Updated = DateTime.Now.AddDays(-30), },
            //new DataItem { Title = "Title #16", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-60), Updated = DateTime.Now.AddDays(-32), },
            //new DataItem { Title = "Title #17", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-80), Updated = DateTime.Now.AddDays(-34), },
            //new DataItem { Title = "Title #18", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-100), Updated = DateTime.Now.AddDays(-36), },
            //new DataItem { Title = "Title #19", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-150), Updated = DateTime.Now.AddDays(-40), },
            //new DataItem { Title = "Title #20", Data = $"{_emojis[Random.Shared.Next(_emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-365), Updated = DateTime.Now.AddDays(-48), },
        };
    }

    #region [PerfCounter]
    int _startupTest = 100;
    int _lastValue = -1;
    bool _useLogarithm = true;
    float GetCPU(int maxWidth = 200)
    {
        if (_perfCPU == null)
            return 0;

        float newValue = _perfCPU.NextValue();

        // For testing each color range.
        if (_startupTest > 0)
        {
            newValue = _startupTest;
            _startupTest -= 20;
        }

        switch (newValue)
        {
            case float f when f > 80: NeedleColor = _level6;
                break;
            case float f when f > 70: NeedleColor = _level5;
                break;
            case float f when f > 40: NeedleColor = _level4;
                break;
            case float f when f > 20: NeedleColor = _level3;
                break;
            case float f when f > 10: NeedleColor = _level2;
                break;
            default: NeedleColor = _level1;
                break;
        }

        float width = 0;

        // Simple duplicate checking.
        if ((int)newValue != _lastValue)
        {
            _lastValue = (int)newValue;

            // Auto-size rectangle graphic.
            if (_useLogarithm)
            {
                //width = ScaleValueLog(newValue);
                width = ScaleValueLog10(newValue);
            }
            else
            {
                width = AmplifyLinear(newValue);
            }

            if (_verbose)
            {
                string format = "[INFO] {0,-15} {1,-15}"; //negative left-justifies, positive right-justifies
                Debug.WriteLine(String.Format(format, $"Input: {newValue:N1}", $"Output: {width:N1}"));
            }

            // Opacity is not carried over from the SolidColorBrush color property accessor.
            var clr = NeedleColor.Color; clr.A = 210;
            // Add entry for histogram.
            NamedColors.Insert(0, new NamedColor { Width = (double)width, Amount = $"{(int)newValue}%", Time = $"{DateTime.Now.ToString("h:mm:ss tt")}", Color = clr });

            // Monitor memory consumption.
            if (NamedColors.Count > 300)
                NamedColors.RemoveAt(NamedColors.Count - 1);
        }

        return newValue;
    }

    #region [Scaling and Clamping]
    /// <summary>
    /// Values of one or below will result in zero.
    /// </summary>
    /// <param name="value">0 to 100</param>
    /// <returns>scaled amount</returns>
    float ScaleValueLog(float value) => MathF.Log(value < 1.1f ? 1.1f : value, 10f) * 100f;

    float ScaleValueLog10(float value)
    {
        // Clamp value between 1 and 100
        value = Math.Clamp(value, 1f, 100f);

        // Scale the value logarithmically to a range between 1 and 200
        float scaledValue = (float)(1 + (199 * Math.Log10(1 + (10 * value / 100)) / Math.Log10(11)));

        return scaledValue;
    }

    /// <summary>
    /// Smaller values will be harder to see on the graph, so we'll scale them up to be more visible.
    /// </summary>
    float AmplifyLinear(float number, int maxClamp = 200)
    {
        if (number < 10)
            return ((number + 1f) * 6f).Clamp(1, maxClamp);
        else if (number < 20)
            return ((number + 1f) * 5f).Clamp(1, maxClamp);
        else if (number < 40)
            return ((number + 1f) * 4f).Clamp(1, maxClamp);
        else if (number < 60)
            return ((number + 1f) * 3f).Clamp(1, maxClamp);
        else if (number < 80)
            return ((number + 1f) * 2.5f).Clamp(1, maxClamp);
        else
            return ((number + 1f) * 2f).Clamp(1, maxClamp);
    }
    float ScaleValue(float value, float multiply = 250F) => (value / 100F) * multiply;
    float ScaleValue(float value, float min, float max) => (value - min) / (max - min) * (max - min) + min;
    float AmplifyUsingLog(float number) => MathF.Exp(MathF.Log(number) * MathF.E / 2f);
    float ScaleValueVector(float begin, float end, int divy = 100)
    {
        var result = System.Numerics.Vector3.Dot(new System.Numerics.Vector3(begin, begin, 0), new System.Numerics.Vector3(end, end, 0));
        return result > 0 ? result / divy : result;
    }
    #endregion

    #endregion

    #region [Miscellaneous]
    /// <summary>
    /// Command testing method.
    /// </summary>
    async Task ThrowError()
    {
        IsBusy = true;
        try
        {
            int test = Random.Shared.Next(1, 11);
            if (test < 8)
                throw new Exception("I don't like this number.");
            else
                Debug.WriteLine($"[INFO] Passed: {test}");
        }
        catch (Exception)
        {
            SetState(SystemStates.Warning);
        }
        await Task.Delay(1000);
        IsBusy = false;
    }

    /// <summary>
    /// Command testing method.
    /// </summary>
    async Task ThrowErrorAsync()
    {
        IsBusy = true;
        Func<int> testFunc = () =>
        {
            int test = Random.Shared.Next(1, 11);
            if (test < 8)
                throw new Exception("I don't like this number.");
            return test;
        };
        try
        {
            int result = await testFunc.RetryAsync(3);
            Debug.WriteLine($"Passed: {result}");
        }
        catch (Exception)
        {
            SetState(SystemStates.Warning);
        }
        await Task.Delay(1000);
        IsBusy = false;
    }

    public void TestTimerScheduler()
    {
        int counter = 0;

        var schedules = new[]
        {
             new TimerSchedule(TimeSpan.FromSeconds(2), 0, "First (2 second)"),
             new TimerSchedule(TimeSpan.FromSeconds(3), 1, "Second (3 second)"),
             new TimerSchedule(TimeSpan.FromSeconds(4), 1, "Third (4 second)"),
             new TimerSchedule(TimeSpan.FromSeconds(5), 1, "Fourth (5 second)"),
        };

        TimerScheduler timer = new TimerScheduler(schedules);
        DateTime previousTime = DateTime.MinValue;

        timer.Elapsed += (object? sender, TimerElapsedEventArgs e) =>
        {
            counter++;

            Debug.WriteLine($"[INFO] Elapsed event id {e.SignalId}: {counter} at {DateTime.Now.ToString("h:mm:ss.fff tt")}");

            // If first round then we won't have a previous time to compare so just return.
            if (counter == 1)
            {
                previousTime = e.SignalTime;
                return;
            }

            TimeSpan interval = e.SignalTime - previousTime;
            Debug.WriteLine($"[INFO] Interval was {interval.TotalSeconds:N1} seconds");
            previousTime = e.SignalTime;
        };

        timer.Start();
        //await Task.Delay(5000);
        //timer.Dispose();
    }
    
    #endregion
}
