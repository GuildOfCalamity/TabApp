using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TabApp.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

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
        this.Loaded += (s, e) => { ShowMessage("Page Loaded", InfoBarSeverity.Informational); };
    }

    /// <summary>
    /// For KeyDownTrigger Behavior demonstration.
    /// </summary>
    public void IncrementCount()
    {
        if (!ViewModel.IsBusy)
            ViewModel.Counter++;
        else
            url.Text = $"Please wait ({DateTime.Now.ToString("hh:mm:ss.fff tt")})";
    }

    public void ShowMessage(string message, InfoBarSeverity severity)
    {
        infoBar.DispatcherQueue?.TryEnqueue(() =>
        {
            infoBar.IsOpen = true;
            infoBar.Severity = severity;
            infoBar.Message = $"{message}";

            // If using the ItemsRepeater in the InfoBar.
            //_msgs.Add(new Message { Content = $"{message}", Severity = severity });
        });
    }

    #region [Overrides]
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Debug.WriteLine($"[INFO] NavigatingTo Source => {e.SourcePageType}");
        base.OnNavigatedTo(e);
        OpacityStoryboard.Begin();
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        Debug.WriteLine($"[INFO] NavigatingFrom Source => {e.SourcePageType}");
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
            _ = App.ShowDialogBox($"Name Guess", $"Is your name is \"{selected.Trim()}\" ?", "Correct", "Incorrect", null, null, new Uri("ms-appx:///Assets/Person.png"));
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
        var suitableItems = new List<string>();
        var splitText = sender.Text.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
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
          "Olivia   ","Liam       ",
          "Emma     ","Noah       ",
          "Amelia   ","Oliver     ",
          "Ava      ","Elijah     ",
          "Sophia   ","Lucas      ",
          "Charlotte","Levi       ",
          "Isabella ","Mason      ",
          "Mia      ","Asher      ",
          "Luna     ","James      ",
          "Harper   ","Ethan      ",
          "Gianna   ","Mateo      ",
          "Evelyn   ","Leo        ",
          "Aria     ","Jack       ",
          "Ella     ","Benjamin   ",
          "Ellie    ","Aiden      ",
          "Mila     ","Logan      ",
          "Layla    ","Grayson    ",
          "Avery    ","Jackson    ",
          "Camila   ","Henry      ",
          "Lily     ","Wyatt      ",
          "Scarlett ","Sebastian  ",
          "Sofia    ","Carter     ",
          "Nova     ","Daniel     ",
          "Aurora   ","William    ",
          "Chloe    ","Alexander  ",
          "Betty    ","Amy        ",
          "Margaret ","Peggy      ",
          "Paula    ","Steve      ",
          "Esteban  ","Stephen    ",
          "Riley    ","Ezra       ",
          "Nora     ","Owen       ",
          "Hazel    ","Michael    ",
          "Abigail  ","Muhammad   ",
          "Rylee    ","Julian     ",
          "Penelope ","Hudson     ",
          "Elena    ","Luke       ",
          "Paul     ","Johan      ",
          "Zoey     ","Samuel     ",
          "Isla     ","Jacob      ",
          "Eleanor  ","Lincoln    ",
          "Elizabeth","Gabriel    ",
          "Madison  ","Jayden     ",
          "Willow   ","Luca       ",
          "Emilia   ","Maverick   ",
          "Violet   ","David      ",
          "Emily    ","Josiah     ",
          "Eliana   ","Elias      ",
          "Stella   ","Jaxon      ",
          "Maya     ","Kai        ",
          "Paisley  ","Anthony    ",
          "Everly   ","Isaiah     ",
          "Addison  ","Eli        ",
          "Ryleigh  ","John       ",
          "Ivy      ","Joseph     ",
          "Grace    ","Matthew    ",
          "Hannah   ","Ezekiel    ",
          "Bella    ","Adam       ",
          "Naomi    ","Caleb      ",
          "Zoe      ","Isaac      ",
          "Aaliyah  ","Theodore   ",
          "Kinsley  ","Nathan     ",
          "Lucy     ","Theo       ",
          "Delilah  ","Thomas     ",
          "Skylar   ","Nolan      ",
          "Leilani  ","Waylon     ",
          "Ayla     ","Ryan       ",
          "Victoria ","Easton     ",
          "Alice    ","Roman      ",
          "Aubrey   ","Adrian     ",
          "Savannah ","Miles      ",
          "Serenity ","Greyson    ",
          "Autumn   ","Cameron    ",
          "Leah     ","Colton     ",
          "Sophie   ","Landon     ",
          "Natalie  ","Santiago   ",
          "Athena   ","Andrew     ",
          "Lillian  ","Hunter     ",
          "Hailey   ","Jameson    ",
          "Audrey   ","Joshua     ",
          "Eva      ","Jace       ",
          "Everleigh","Cooper     ",
          "Kennedy  ","Dylan      ",
          "Maria    ","Jeremiah   ",
          "Natalia  ","Kingston   ",
          "Nevaeh   ","Xavier     ",
          "Brooklyn ","Christian  ",
          "Raelynn  ","Christopher",
          "Arya     ","Kayden     ",
          "Ariana   ","Charlie    ",
          "Madelyn  ","Aaron      ",
          "Claire   ","Jaxson     ",
          "Valentina","Silas      ",
          "Sadie    ","Ryder      ",
          "Gabriella","Austin     ",
          "Ruby     ","Dominic    ",
          "Anna     ","Amir       ",
          "Iris     ","Carson     ",
          "Charlie  ","Jordan     ",
          "Brielle  ","Weston     ",
          "Emery    ","Micah      ",
          "Melody   ","Rowan      ",
          "Amara    ","Beau       ",
          "Piper    ","Declan     ",
          "Quinn    ","Everett    ",
          "Jade     ","Alex       "
    };
    #endregion
}
