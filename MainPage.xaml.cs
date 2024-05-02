using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
using static System.Net.Mime.MediaTypeNames;

namespace TabApp;

public sealed partial class MainPage : Page
{
    public TabViewModel ViewModel { get; private set; }

    public MainPage()
    {
        _ = App.GetStopWatch(true);

        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        this.InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        ViewModel = App.GetService<TabViewModel>();

        // Set starting tab.
        this.TabNavigationView.SelectedItem = this.TabNavigationView.MenuItems.FirstOrDefault(o => o is NavigationViewItem { Content: "Tab1" });

        Debug.WriteLine($"[INFO] {nameof(MainPage)} took {App.GetStopWatch().TotalMilliseconds:N1} milliseconds");
    }

    void TabNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // ** Top display mode **
        //sender.Resources["NavigationViewTopPaneBackground"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 80, 0));
        
        // ** Left display mode **
        //sender.Resources["NavigationViewExpandedPaneBackground"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 80, 0));
        
        // ** LeftCompact/LeftMinimal **
        //sender.Resources["NavigationViewDefaultPaneBackground"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 80, 0));

        // https://learn.microsoft.com/en-us/windows/apps/design/controls/navigationview#pane-backgrounds
        /* [Other NavigationView Resource Keys]
           x:Key="NavigationViewBackButtonBackground"                 
           x:Key="NavigationViewButtonBackgroundDisabled"             
           x:Key="NavigationViewButtonBackgroundPointerOver"          
           x:Key="NavigationViewButtonBackgroundPressed"              
           x:Key="NavigationViewButtonForegroundDisabled"             
           x:Key="NavigationViewButtonForegroundPointerOver"          
           x:Key="NavigationViewButtonForegroundPressed"              
           x:Key="NavigationViewContentBackground"                    
           x:Key="NavigationViewContentGridBorderBrush"               
           x:Key="NavigationViewDefaultPaneBackground"                
           x:Key="NavigationViewExpandedPaneBackground"               
           x:Key="NavigationViewItemBackground"                       
           x:Key="NavigationViewItemBackgroundChecked"                
           x:Key="NavigationViewItemBackgroundCheckedDisabled"        
           x:Key="NavigationViewItemBackgroundCheckedPointerOver"     
           x:Key="NavigationViewItemBackgroundCheckedPressed"         
           x:Key="NavigationViewItemBackgroundDisabled"               
           x:Key="NavigationViewItemBackgroundPointerOver"            
           x:Key="NavigationViewItemBackgroundPressed"                
           x:Key="NavigationViewItemBackgroundSelected"               
           x:Key="NavigationViewItemBackgroundSelectedDisabled"       
           x:Key="NavigationViewItemBackgroundSelectedPointerOver"    
           x:Key="NavigationViewItemBackgroundSelectedPressed"        
           x:Key="NavigationViewItemBorderBrush"                      
           x:Key="NavigationViewItemBorderBrushChecked"               
           x:Key="NavigationViewItemBorderBrushCheckedDisabled"       
           x:Key="NavigationViewItemBorderBrushCheckedPointerOver"    
           x:Key="NavigationViewItemBorderBrushCheckedPressed"        
           x:Key="NavigationViewItemBorderBrushDisabled"              
           x:Key="NavigationViewItemBorderBrushPointerOver"           
           x:Key="NavigationViewItemBorderBrushPressed"               
           x:Key="NavigationViewItemBorderBrushSelected"              
           x:Key="NavigationViewItemBorderBrushSelectedDisabled"      
           x:Key="NavigationViewItemBorderBrushSelectedPointerOver"   
           x:Key="NavigationViewItemBorderBrushSelectedPressed"       
           x:Key="NavigationViewItemForeground"                       
           x:Key="NavigationViewItemForegroundChecked"                
           x:Key="NavigationViewItemForegroundCheckedDisabled"        
           x:Key="NavigationViewItemForegroundCheckedPointerOver"     
           x:Key="NavigationViewItemForegroundCheckedPressed"         
           x:Key="NavigationViewItemForegroundDisabled"               
           x:Key="NavigationViewItemForegroundPointerOver"            
           x:Key="NavigationViewItemForegroundPressed"                
           x:Key="NavigationViewItemForegroundSelected"               
           x:Key="NavigationViewItemForegroundSelectedDisabled"       
           x:Key="NavigationViewItemForegroundSelectedPointerOver"    
           x:Key="NavigationViewItemForegroundSelectedPressed"        
           x:Key="NavigationViewItemHeaderForeground"                 
           x:Key="NavigationViewItemSeparatorForeground"              
           x:Key="NavigationViewSelectionIndicatorForeground"         
           x:Key="NavigationViewTopPaneBackground"                    
           x:Key="TopNavigationViewItemBackgroundPointerOver"         
           x:Key="TopNavigationViewItemBackgroundPressed"             
           x:Key="TopNavigationViewItemBackgroundSelected"            
           x:Key="TopNavigationViewItemBackgroundSelectedPointerOver" 
           x:Key="TopNavigationViewItemBackgroundSelectedPressed"     
           x:Key="TopNavigationViewItemForeground"                    
           x:Key="TopNavigationViewItemForegroundDisabled"            
           x:Key="TopNavigationViewItemForegroundPointerOver"         
           x:Key="TopNavigationViewItemForegroundPressed"             
           x:Key="TopNavigationViewItemForegroundSelected"            
           x:Key="TopNavigationViewItemForegroundSelectedPointerOver" 
           x:Key="TopNavigationViewItemForegroundSelectedPressed"     
           x:Key="TopNavigationViewItemRevealBackgroundFocused"       
           x:Key="TopNavigationViewItemRevealContentForegroundFocused"
           x:Key="TopNavigationViewItemRevealIconForegroundFocused"   
           x:Key="TopNavigationViewItemSeparatorForeground" 
           <x:Double x:Key="PaneToggleButtonSize">40</x:Double>
           <x:Double x:Key="PaneToggleButtonHeight">36</x:Double>
           <x:Double x:Key="PaneToggleButtonWidth">40</x:Double>
           <x:Double x:Key="NavigationViewCompactPaneLength">48</x:Double>
           <x:Double x:Key="NavigationViewTopPaneHeight">48</x:Double>
           <Thickness x:Key="NavigationViewItemInnerHeaderMargin">16,0</Thickness>
           <Thickness x:Key="NavigationViewAutoSuggestBoxMargin">16,0</Thickness>
           <Style x:Key="PaneToggleButtonStyle" TargetType="Button">
           <Setter Target="PaneToggleButtonGrid">
        */

        var current = App.GetCurrentNamespace();
        if (args.IsSettingsSelected is true)
        {
            // Do nothing
        }
        else if (args.SelectedItem is NavigationViewItem item && item.Content is string content && Type.GetType($"{current}.Views.{content}Page") is Type pageType)
        {
            _ = this.ContentFrame.Navigate(pageType);
        }
    }

    void MenuItemTabClick(object sender, RoutedEventArgs e)
    {
        var current = App.GetCurrentNamespace();
        if (sender is MenuFlyoutItem mfi && Type.GetType($"{current}.Views.{mfi.Text}Page") is Type pageType)
        {
            _ = this.ContentFrame.Navigate(pageType);
        }
    }

    void TabNavigationItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var element = sender.PaneCustomContent;
        if (element is not null && sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top && element is TextBlock tb) 
            tb.Text = $"Invoked \"{args.InvokedItem}\" at {DateTime.Now.ToString("hh:mm:ss.fff tt")}";
        else
            Debug.WriteLine($"[INFO] NavigationView does not contain CustomContent.");
    }

    void mfiExitClick(object sender, RoutedEventArgs e)
    {
        App.IsClosing = true;
        App.Current.Exit(); // will skip OnWindowClosing and go to OnWindowDestroying
    }

}
