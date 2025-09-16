using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Helpers.Common;
using Windows.UI.ViewManagement;
using WinUIEx;

namespace MuhasibPro
{
    public sealed partial class MainWindow : WindowEx
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
            this.IsMaximizable = true;
            this.IsMinimizable = true;
            this.IsResizable = true;
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
