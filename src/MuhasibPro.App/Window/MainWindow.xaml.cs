using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.App.Helpers;
using Windows.UI.ViewManagement;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

        private UISettings settings;

        public MainWindow()
        {
            InitializeComponent();
            this.IsMaximizable = true;
            this.IsMinimizable = true;
            this.IsResizable = true;            
            // Mica desteği kontrolü
            if (MicaController.IsSupported())
            {
                SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base};
            }
            else if (DesktopAcrylicController.IsSupported())
            {
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }
            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
            Content = null;
            this.Title = "MuhasibPro";
            // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
            dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            settings = new UISettings();
            settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        }




        // this handles updating the caption button colors correctly when indows system theme is changed
        // while the app is open
        private void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            // This calls comes off-thread, hence we will need to dispatch it to current app's thread
            dispatcherQueue.TryEnqueue(
                () =>
                {
                    TitleBarHelper.ApplySystemThemeToCaptionButtons();
                });
        }
    }
}
