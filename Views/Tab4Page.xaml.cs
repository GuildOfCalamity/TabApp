using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;

using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;

using TabApp.ViewModels;

namespace TabApp.Views;

public sealed partial class Tab4Page : Page
{
    public TabViewModel ViewModel { get; private set; }

    public Tab4Page()
    {
        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        this.InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        ViewModel = App.GetService<TabViewModel>();

        url.Text = "For more WinUI3 examples be sure to visit my github at https://github.com/GuildOfCalamity?tab=repositories";

        if (App.AnimationsEffectsEnabled)
            StoryboardSpinner.Begin();

        this.Tapped += (s, e) => { ShowMessage("Page Tapped", InfoBarSeverity.Success); };
        this.Loaded += (s, e) => 
        { 
            ShowMessage("Page Loaded", InfoBarSeverity.Informational);
            prSpinner.DispatcherQueue.TryEnqueue(async () =>
            {
                prSpinner.Foreground = await CalculateAverageColor(imgSpinner);
            });
        };
    }

    /// <summary>
    /// I have modified this method to be more universal and user-friendly.
    /// The original can be found here: https://github.com/XamlBrewer/XamlBrewer-WinUI3-Beer-Color-Meter/blob/master/XamlBrewer.WinUI3.BeerColorMeter/MainWindow.xaml.cs
    /// </summary>
    /// <param name="image"><see cref="Image"/></param>
    /// <returns><see cref="SolidColorBrush"/></returns>
    async Task<SolidColorBrush> CalculateAverageColor(Image image)
    {
        string assetPath = "";
        BitmapDecoder decoder;
        BitmapTransform transform;

        try
        {
            BitmapImage? himage = (BitmapImage)image.Source;
            if (himage == null)
                return new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            if (App.IsPackaged)
                assetPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, himage.UriSour‌​ce.AbsoluteUri.Replace("ms-appx:///", "")).Replace("/", "\\");
            else
                assetPath = Path.Combine(Directory.GetCurrentDirectory(), himage.UriSour‌​ce.AbsoluteUri.Replace("ms-appx:///", "")).Replace("/", "\\");

            StorageFile? file = await StorageFile.GetFileFromPathAsync(assetPath);
            if (file == null)
                return new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            using Stream? stream = await file.OpenStreamForReadAsync();
            using IRandomAccessStream? ras = stream.AsRandomAccessStream();
            decoder = await BitmapDecoder.CreateAsync(ras);

            transform = new()
            {
                ScaledWidth = (uint)image.ActualWidth,
                ScaledHeight = (uint)image.ActualHeight
            };

            // Get the pixels
            PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Rgba8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            // Calculate average color
            byte[] sourcePixels = pixelData.DetachPixelData();
            var nbrOfPixels = sourcePixels.Length / 4;
            int avgR = 0; int avgG = 0; int avgB = 0;
            for (int i = 0; i < sourcePixels.Length; i += 4)
            {
                avgR += sourcePixels[i];
                avgG += sourcePixels[i + 1];
                avgB += sourcePixels[i + 2];
            }

            var color = Windows.UI.Color.FromArgb(255, (byte)(avgR / nbrOfPixels), (byte)(avgG / nbrOfPixels), (byte)(avgB / nbrOfPixels));

            //return new SolidColorBrush(color.DarkerBy(0.5f));
            return new SolidColorBrush(color);
        }
        catch (Exception)
        {
            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
    }

    /// <summary>
    /// For KeyDownTrigger <see cref="Microsoft.Xaml.Interactivity.Behavior{T}"/> demonstration.
    /// </summary>
    public void IncrementCount()
    {
        if (!ViewModel.IsBusy)
            ViewModel.Counter++;
        else
            url.Text = $"Please wait ({DateTime.Now.ToString("hh:mm:ss.fff tt")})";
    }

    /// <summary>
    /// Thread-safe helper for <see cref="Microsoft.UI.Xaml.Controls.InfoBar"/>.
    /// </summary>
    /// <param name="message">text to show</param>
    /// <param name="severity"><see cref="Microsoft.UI.Xaml.Controls.InfoBarSeverity"/></param>
    public void ShowMessage(string message, InfoBarSeverity severity)
    {
        infoBar.DispatcherQueue?.TryEnqueue(() =>
        {
            infoBar.IsOpen = true;
            infoBar.Severity = severity;
            infoBar.Message = $"{message}";
        });
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

    #region [AutoSuggestBox]
    void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) => DisplaySuggestions(sender);
    void AutoSuggestBox_TypingPaused(object sender, EventArgs e) => DisplaySuggestions(sender as AutoSuggestBox);
    void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        var selected = $"{args.SelectedItem}".Trim();
        try
        {
            if (string.IsNullOrEmpty(selected))
            {
                url.Text = $"Pick a valid name.";
                return;
            }
            _ = App.ShowDialogBox($"Name Guess", $"Is your name is \"{selected.Trim()}\" ?", "Correct", "Incorrect", null, null, new Uri($"ms-appx:///{App.AssetFolder}/Person.png"));
        }
        catch (Exception ex)
        {
            url.Text = ex.Message;
        }
    }

    void DisplaySuggestions(AutoSuggestBox? sender)
    {
        if (sender == null)
            return;

        ViewModel.IsBusy = true;
        List<string>? suitableItems = new();
        string[]? splitText = sender.Text.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
        foreach (var name in NameList)
        {
            // LINQ "splitText.All(Func<string, bool>)"
            var found = splitText.All((key) => { return name.Contains(key, StringComparison.OrdinalIgnoreCase); });
            if (found)
                suitableItems.Add(name.Trim());
        }

        if (suitableItems.Count == 0)
            suitableItems.Add("No results found");

        sender.ItemsSource = suitableItems;
        ViewModel.IsBusy = false;
    }

    public List<string> NameList = new() {
          "Olivia   ","Liam       ","Emma     ","Noah       ",
          "Amelia   ","Oliver     ","Ava      ","Elijah     ",
          "Sophia   ","Lucas      ","Charlotte","Levi       ",
          "Isabella ","Mason      ","Mia      ","Asher      ",
          "Luna     ","James      ","Harper   ","Ethan      ",
          "Gianna   ","Mateo      ","Evelyn   ","Leo        ",
          "Aria     ","Jack       ","Ella     ","Benjamin   ",
          "Ellie    ","Aiden      ","Mila     ","Logan      ",
          "Layla    ","Grayson    ","Avery    ","Jackson    ",
          "Camila   ","Henry      ","Lily     ","Wyatt      ",
          "Scarlett ","Sebastian  ","Sofia    ","Carter     ",
          "Nova     ","Daniel     ","Aurora   ","William    ",
          "Chloe    ","Alexander  ","Betty    ","Amy        ",
          "Margaret ","Peggy      ","Paula    ","Steve      ",
          "Esteban  ","Stephen    ","Riley    ","Ezra       ",
          "Nora     ","Owen       ","Hazel    ","Michael    ",
          "Abigail  ","Muhammad   ","Rylee    ","Julian     ",
          "Penelope ","Hudson     ","Elena    ","Luke       ",
          "Paul     ","Johan      ","Zoey     ","Samuel     ",
          "Isla     ","Jacob      ","Eleanor  ","Lincoln    ",
          "Elizabeth","Gabriel    ","Madison  ","Jayden     ",
          "Willow   ","Luca       ","Emilia   ","Maverick   ",
          "Violet   ","David      ","Emily    ","Josiah     ",
          "Eliana   ","Elias      ","Stella   ","Jaxon      ",
          "Maya     ","Kai        ","Paisley  ","Anthony    ",
          "Everly   ","Isaiah     ","Addison  ","Eli        ",
          "Ryleigh  ","John       ","Ivy      ","Joseph     ",
          "Grace    ","Matthew    ","Hannah   ","Ezekiel    ",
          "Bella    ","Adam       ","Naomi    ","Caleb      ",
          "Zoe      ","Isaac      ","Aaliyah  ","Theodore   ",
          "Kinsley  ","Nathan     ","Lucy     ","Theo       ",
          "Delilah  ","Thomas     ","Skylar   ","Nolan      ",
          "Leilani  ","Waylon     ","Ayla     ","Ryan       ",
          "Victoria ","Easton     ","Alice    ","Roman      ",
          "Aubrey   ","Adrian     ","Savannah ","Miles      ",
          "Serenity ","Greyson    ","Autumn   ","Cameron    ",
          "Leah     ","Colton     ","Sophie   ","Landon     ",
          "Natalie  ","Santiago   ","Athena   ","Andrew     ",
          "Lillian  ","Hunter     ","Hailey   ","Jameson    ",
          "Audrey   ","Joshua     ","Eva      ","Jace       ",
          "Everleigh","Cooper     ","Kennedy  ","Dylan      ",
          "Maria    ","Jeremy     ","Natalia  ","Kingston   ",
          "Nevaeh   ","Xavier     ","Brooklyn ","Christian  ",
          "Raelynn  ","Christopher","Arya     ","Kayden     ",
          "Ariana   ","Charlie    ","Madelyn  ","Aaron      ",
          "Claire   ","Jaxson     ","Valentina","Silas      ",
          "Kris     ","Eion       ","Sadie    ","Ryder      ",
          "Gabriella","Austin     ","Ruby     ","Dominic    ",
          "Anna     ","Amir       ","Iris     ","Carson     ",
          "Charlie  ","Jordan     ","Brielle  ","Weston     ",
          "Emery    ","Micah      ","Melody   ","Rowan      ",
          "Amara    ","Beau       ","Piper    ","Declan     ",
          "Eric     ","Nick       ","Jason    ","Evan       ",
          "Quinn    ","Everett    ","Rebecca  ","Stuart     ",
          "Mark     ","Nathan     ","Gloria   ","Wilma      ",
          "Peter    ","Scott      ","Byron    ","Stephanie  ",
          "Fred     ","Frederick  ","Bill     ","Robert     ",
          "Jade     ","Alex       "
    };
    #endregion
}
