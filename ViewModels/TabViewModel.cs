using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TabApp;
using TabApp.Models;
using Windows.Foundation.Collections;

namespace TabApp.ViewModels;

public class TabViewModel : ObservableRecipient
{
    #region [Props]
    bool _option1 = false;
    bool _option2 = true;
    bool _option3 = true;
    int _maxCPU = 100;
    int _currentCPU = 0;
    int _counter = 0;
    bool _isBusy = false;
    string _popupText = "...";
    SystemStates _systemState = SystemStates.None;
    DataItem? _selectedItem;
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

    public DataItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value); 
    }

    public ObservableCollection<DataItem> DataItems = new(); //public ObservableCollection<DataItem> DataItems = new();

    public NamedColor? SelectedColor
    {
        get => _selectedColor;
        set 
        {
            // Fire event after user selects list item.
            App.RootEventBus?.Publish("CPUEvent", value);
            SetProperty(ref _selectedColor, value); 
        }
    }

    public ObservableCollection<NamedColor> NamedColors = new();

    public ICommand SampleCommand { get; }

    static readonly List<string> emojis = new List<string> 
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

        App.OnWindowClosing += App_OnWindowClosing;

        if (!App.RootEventBus.IsSubscribed("WindowVisibilityEvent"))
            App.RootEventBus.Subscribe("WindowVisibilityEvent", EventBusMessageHandler);

        SystemState = SystemStates.Init;

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
                    AddDataItem($"📢 INFO", $"Got MenuFlyoutItem \"{mfi.Text}\"");
            }
            else if (obj is AppBarButton abb)
            {
                if (abb != null)
                    AddDataItem($"📢 INFO", $"Got AppBarButton \"{abb.Label}\"");
            }
            else if (obj is Button btn)
            {
                if (btn != null)
                    AddDataItem($"📢 INFO", $"Got Button \"{btn.Content}\"");

                var installRoot = Directory.GetCurrentDirectory();
                var files = new List<FileInfo>();
                var dir = new DirectoryInfo(installRoot);
                if (dir.Exists)
                {
                    var results = dir.GetFiles($"Tab*.*", SearchOption.TopDirectoryOnly);
                    foreach (var file in results)
                    {
                        AddDataItem($"📢 INFO", $"{file.Name}  v{file.FullName.GetFileVersion()}");
                    }

                    files.AddRange(dir.GetFiles($"TabApp.exe", SearchOption.AllDirectories));
                    files.AddRange(dir.GetFiles($"TabApp.dll", SearchOption.AllDirectories));
                    files = files.Where(o => !o.FullName.Contains(Path.Combine(dir.FullName, ".backup"))).ToList();
                    foreach (var file in files)
                    {
                        AddDataItem($"📢 INFO", $"{file.Name}  v{file.FullName.GetFileVersion()}");
                    }
                }
            }
            else if (obj is Page pg)
            {
                if (pg != null)
                    AddDataItem($"📢 INFO", $"Got Page (using font {pg.FontFamily.Source})");
            }
            else if (obj is String str)
            {
                if (!string.IsNullOrEmpty(str))
                    AddDataItem($"📢 INFO", $"Got Behavior event \"{str}\"");
            }
            else if (obj is Microsoft.Xaml.Interactions.Core.InvokeCommandAction cmd)
            {
                if (cmd != null)
                    AddDataItem($"📢 INFO", $"Got Behavior InvokeCommandAction");
                
            }
            else
            {
                AddDataItem($"📢 WARNING", $"No action defined for type '{obj?.GetType()}'");
            }

            await Task.Delay(3000);

            IsBusy = false;
        });

        // Instantiating a PerformanceCounter can take a few seconds, so we'll queue this on another thread.
        Task.Run(() =>
        {
            if (_perfCPU == null)
            {
                _perfCPU = new System.Diagnostics.PerformanceCounter("Processor Information", "% Processor Time", "_Total", true);
            }
        });

        // CPU usage with RadialGauge.
        _level6 = new SolidColorBrush(Microsoft.UI.Colors.OrangeRed); _level6.Opacity = 0.7;
        _level5 = new SolidColorBrush(Microsoft.UI.Colors.Orange); _level5.Opacity = 0.7;
        _level4 = new SolidColorBrush(Microsoft.UI.Colors.Yellow); _level4.Opacity = 0.7;
        _level3 = new SolidColorBrush(Microsoft.UI.Colors.YellowGreen); _level3.Opacity = 0.7;
        _level2 = new SolidColorBrush(Microsoft.UI.Colors.SpringGreen); _level2.Opacity = 0.7;
        _level1 = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue); _level1.Opacity = 0.7;
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

        SystemState = SystemStates.Ready;

        #region [Heartbeat Simulation]
        //ThreadPool.QueueUserWorkItem((object? state) =>
        //{
        //    while (!App.IsClosing)
        //    {
        //        App.MainDispatcher?.CallOnUIThread(() => { SystemState = SystemStates.Processing; });
        //        Thread.Sleep(1000);
        //        App.MainDispatcher?.CallOnUIThread(() => { SystemState = SystemStates.None; });
        //        Thread.Sleep(3000);
        //    }
        //    Debug.WriteLine($"[INFO] Heartbeat thread loop exit.");
        //});
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
        else if (e.Payload?.GetType() == typeof(Boolean)) // IsVisible
        {
            // We'll only run CPU timer when not minimized.
            if (!(bool)e.Payload)
                _timer?.Stop();
            else
                _timer?.Start();
        }
    }

    void App_OnWindowClosing(string obj)
    {
        App.MainDispatcher?.CallOnUIThread(() => { SystemState = SystemStates.Shutdown; });
    }

    /// <summary>
    /// We're using this to send a signal to the UI that an item has 
    /// been added so common routines like auto-scroll can be picked up.
    /// </summary>
    void AddDataItem(string title, string text)
    {
        var di = new DataItem { Title = title, Data = $"{text}", Created = DateTime.Now, Updated = DateTime.Now };
        App.RootEventBus.Publish("ItemAddedEvent", di);
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
            new DataItem { Title = "Title #1", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-2),  Updated = DateTime.Now.AddDays(-1), },
            new DataItem { Title = "Title #2", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-3),  Updated = DateTime.Now.AddDays(-2), },
            new DataItem { Title = "Title #3", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-4),  Updated = DateTime.Now.AddDays(-3), },
            new DataItem { Title = "Title #4", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-5),  Updated = DateTime.Now.AddDays(-4), },
            new DataItem { Title = "Title #5", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-6),  Updated = DateTime.Now.AddDays(-5), },
            new DataItem { Title = "Title #6", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-7),  Updated = DateTime.Now.AddDays(-6), },
            new DataItem { Title = "Title #7", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-8),  Updated = DateTime.Now.AddDays(-7), },
            new DataItem { Title = "Title #8", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-9),  Updated = DateTime.Now.AddDays(-8), },
            new DataItem { Title = "Title #9", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created  = DateTime.Now.AddDays(-10), Updated = DateTime.Now.AddDays(-9), },
            new DataItem { Title = "Title #10", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-11), Updated = DateTime.Now.AddDays(-10), },
            new DataItem { Title = "Title #11", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-12), Updated = DateTime.Now.AddDays(-11), },
            new DataItem { Title = "Title #12", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-13), Updated = DateTime.Now.AddDays(-12), },
            new DataItem { Title = "Title #13", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-14), Updated = DateTime.Now.AddDays(-13), },
            new DataItem { Title = "Title #14", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-15), Updated = DateTime.Now.AddDays(-14), },
            new DataItem { Title = "Title #15", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-16), Updated = DateTime.Now.AddDays(-15), },
            new DataItem { Title = "Title #16", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-17), Updated = DateTime.Now.AddDays(-16), },
            new DataItem { Title = "Title #17", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-18), Updated = DateTime.Now.AddDays(-17), },
            new DataItem { Title = "Title #18", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-19), Updated = DateTime.Now.AddDays(-18), },
            new DataItem { Title = "Title #19", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-20), Updated = DateTime.Now.AddDays(-19), },
            new DataItem { Title = "Title #20", Data = $"{emojis[Random.Shared.Next(emojis.Count)]} Here is a sample note with data.", Created = DateTime.Now.AddDays(-21), Updated = DateTime.Now.AddDays(-20), },
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
                width = ScaleValueLog2(newValue);

                // This will give us a range scaled around 1 to 100, we'll then multiply by 2 to fit the width.
                //if (newValue < 1.1)
                //    width = MathF.Log(newValue + 2, 1.05f) * (maxWidth / 100f);
                //else
                //    width = MathF.Log(newValue, 1.05f) * (maxWidth / 100f);
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

            // Add entry for histogram.
            NamedColors.Insert(0, new NamedColor { Width = (double)width, Amount = $"{(int)newValue}%", Time = $"{DateTime.Now.ToString("h:mm:ss tt")}", Color = NeedleColor.Color });

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

    float ScaleValueLog2(float value)
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
}
