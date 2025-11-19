// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Extensions.Common;
using Windows.UI.ViewManagement;


namespace MuhasibPro
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;
        private UISettings settings;
        public MainWindow()
        {
            this.InitializeComponent();
            WindowSettings();
            dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            settings = new UISettings();
            settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
        }
        void WindowSettings()
        {
            if (MicaController.IsSupported())
            {
                SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base };
            }
            else if (DesktopAcrylicController.IsSupported())
            {
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }
            ExtendsContentIntoTitleBar = true;
           
            this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/AppIcon.ico"));
            Content = null;
            this.Title = "MuhasibPro";
        }
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
