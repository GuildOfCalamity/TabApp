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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TabApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Tab2Page : Page
    {
        public TabViewModel ViewModel { get; private set; }

        public Tab2Page()
        {
            Debug.WriteLine($"[DEBUG] {MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

            this.InitializeComponent();

            // Ensure that the Page is only created once, and cached during navigation.
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            ViewModel = App.GetService<TabViewModel>();
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
    }
}
