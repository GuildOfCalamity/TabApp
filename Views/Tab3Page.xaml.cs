using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;

using Windows.Storage;

using TabApp.ViewModels;

namespace TabApp.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Tab3Page : Page
{
    double xSpeed = 3;
    double ySpeed = 3;
    List<Uri> _assets = new();
    DispatcherTimer? timer = null;
    List<Canvas> _canvasList = new();

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

        #region [Animation Setup]
        string path = string.Empty;
        if (!App.IsPackaged)
            path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), App.AssetFolder);
        else
            path = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, App.AssetFolder);

        foreach (var f in Directory.GetFiles(path, "LED*.png", SearchOption.TopDirectoryOnly))
        {
            _assets.Add(new Uri($"ms-appx:///{App.AssetFolder}/{System.IO.Path.GetFileName(f)}"));
        }

        // Animation frame update timer.
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(20); // DispatchTimer does not have this resolution, but we'll ask for it anyway.
        timer.Tick += AnimationTimer_Tick;
        
        this.Loaded += (s, e) => { timer?.Start(); };
        this.Unloaded += (s, e) => { timer?.Stop(); };
        #endregion

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

        if (e.Parameter != null && e.Parameter is SystemStates sys)
            Debug.WriteLine($"[INFO] Received system state '{sys}'");

        if (App.AnimationsEffectsEnabled)
            OpacityStoryboard.Begin();

        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        Debug.WriteLine($"[INFO] NavigatingFrom Source => {e.SourcePageType}");
        if (App.AnimationsEffectsEnabled)
            OpacityStoryboard.SkipToFill();
        base.OnNavigatingFrom(e);
    }
    #endregion

    #region [Animation Support]
    /// <summary>
    /// <para>
    /// The <see cref="DispatcherTimer"/> in WinUI3 has a resolution of approximately 33.34 milliseconds 
    /// per tick, which corresponds to a frame rate of around 30 frames per second. This means that 
    /// the timer ticks at a frequency of approximately 30 times per second, with each tick occurring 
    /// roughly every 33.34 milliseconds.
    /// </para>
    /// <para>
    /// The <see cref="DispatcherTimer"/> is based on the UI thread's message pump, which is why its 
    /// accuracy is bound to the UI thread's message processing rate. In most scenarios, especially for 
    /// UI-related tasks and animations, a frame rate of 60 FPS (16.67 milliseconds per frame) or slightly 
    /// above is generally best for a smooth user experience.
    /// </para>
    /// <para>
    /// If you need a higher update rate or a more accurate timer, you might consider using alternative 
    /// timer mechanisms. One such alternative is the <see cref="CompositionTarget.Rendering"/> event, 
    /// which is triggered each time a new frame is rendered. This event is tightly synchronized with 
    /// the display's refresh rate, providing a more accurate timer for animations (usually 60 FPS).
    /// </para>
    /// </summary>
    void AnimationTimer_Tick(object? sender, object e)
    {
        // Do we have anything to do?
        if (_canvasList.Count == 0)
            return;
    
        // We want to avoid oversubscription. In the event that we're doing too
        // much inside the tick event we'll stop the timer while we operate and
        // then restart it. I believe the DispatchTimer handles this internally
        // for you but it's a good idea to follow best practices.
        timer?.Stop();

        int counter = 0;
        foreach (var cvs in _canvasList)
        {
            counter++;

            // If the user is on another page the canvas size will not be usable.
            if (cvs.ActualWidth == 0 || cvs.ActualWidth == double.NaN ||
                cvs.ActualHeight == 0 || cvs.ActualHeight == double.NaN)
                continue;

            MoveImage(cvs, counter);
        }
    
        timer?.Start();
    }

    /// <summary>
    /// Updates a single <see cref="Image"/>.
    /// </summary>
    void MoveImage(Canvas cvs, int counter)
    {
        if (cvs == null)
            return;

        //cvs.DispatcherQueue.TryEnqueue(() => { 
            double x = 0; double y = 0;

            /** Horizontal **/
            if (cvs.ActualWidth > 0)
            {
                // Testing accessors.
                var dcdi = cvs.DataContext as Models.DataItem;
                var img = cvs.Children.FirstOrDefault() as Image;
                if (img != null)
                {
                    // The value can be erroneous when using from inside a parent element with multiple children.
                    x = Canvas.GetLeft(img); 

                    // Update position.
                    x += xSpeed;

                    // Bounce off the horizontal edge.
                    if (x < 0 || x > cvs.ActualWidth - img.ActualWidth)
                        xSpeed *= -1;

                    // Handle resizing issues.
                    if (x < -1) x = 1;
                    if (x > (cvs.ActualWidth - (img.ActualWidth - 6))) x -= 4;

                    // Update image position.
                    Canvas.SetLeft(img, x);
                }
            }

            /** Vertical **/
            if (cvs.ActualHeight > 0)
            {
                // Testing accessors.
                var dcdi = cvs.DataContext as Models.DataItem;
                var img = cvs.Children.FirstOrDefault() as Image;
                if (img != null)
                {
                    // The value can be erroneous when using from inside a parent element with multiple children.
                    y = Canvas.GetTop(img);

                    // Update position.
                    y += ySpeed;

                    // Bounce off the vertical edge.
                    if (y < 0 || y > cvs.ActualHeight - img.ActualHeight)
                        ySpeed *= -1;

                    // Handle resizing issues.
                    if (y < -1) y = 1;
                    if (y > (cvs.ActualHeight - (img.ActualHeight - 6))) y -= 4;

                    // Update image position.
                    Canvas.SetTop(img, y);
                }
            }
        //});
    }

    /// <summary>
    /// Capture each <see cref="Canvas"/> when the element is loaded.
    /// </summary>
    void canvas_Loaded(object sender, RoutedEventArgs e)
    {
        var cvs = sender as Canvas;
        if (cvs != null)
        {
            Image? image = new();
            image.Source = new BitmapImage(_assets[Random.Shared.Next(0, _assets.Count)]);
            image.Width = 32;
            image.Height = 32;
            image.CenterPoint = new System.Numerics.Vector3(16, 16, 0); // If using rotation animations, be sure to set the center point.
            if (_canvasList.Count == 0)
            {
                _canvasList.Add(cvs);
                cvs.Children.Add(image); // Add image to canvas.
            }
            else if (_canvasList.Count > 0 && !_canvasList.Contains(cvs))
            {
                _canvasList.Add(cvs);
                cvs.Children.Add(image); // Add image to canvas.
            }
            Debug.WriteLine($"[INFO] Canvas list count: {_canvasList.Count}");
        }
    }
    #endregion
}
