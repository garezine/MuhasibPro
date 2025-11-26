using CommunityToolkit.WinUI;
using MuhasibPro.Contracts.CoreServices;
using MuhasibPro.Extensions.ExtensionService;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.Views.Shell;
using MuhasibPro.Views.ViewWindow;

namespace MuhasibPro.Helpers
{
    public class NavigationHelper
    {
        private static readonly Lazy<NavigationHelper> _instance = new(() => new NavigationHelper());

        public static NavigationHelper Instance => _instance.Value;

        private async Task SetupWindowContentAsync(Window window, Type viewModelType, object parameter)
        {
            await window.DispatcherQueue
                .EnqueueAsync(
                    () =>
                    {
                        var frame = new Frame();
                        var args = new ShellArgs { ViewModel = viewModelType, Parameter = parameter };
                        frame.Navigate(typeof(ShellView), args);
                        window.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
                        window.Content = frame;
                    });
        }

        public async Task CloseWindowAsync(int windowId)
        {
            var window = CustomWindowHelper.GetWindowById(windowId);
            if(window == null || window == App.MainWindow)
                return;

            try
            {
                if(window.Content is Frame frame && frame.Content is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                await window.DispatcherQueue.EnqueueAsync(() => window.Close());
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Window close error: {ex.Message}");
            }
        }

        private DetailsWindow DetailsWindow { get; set; }

        public async Task<int> CreateWindowAsync(Type viewModelType, object parameter, string customTitle)
        {
            try
            {
                if(CustomWindowHelper.TryActivateExistingWindow(viewModelType))
                {
                    var existingWindow = CustomWindowHelper.GetWindowByViewModel(viewModelType);
                    return CustomWindowHelper.GetWindowId(existingWindow);
                }
                var _themeSelectorService = ServiceLocator.Current.GetService<IThemeSelectorService>();
                DetailsWindow = new DetailsWindow();

                int viewId = (int)DetailsWindow.AppWindow.Id.Value;
                DetailsWindow.Title = customTitle;
                CustomWindowHelper.PositionRelativeToParent(DetailsWindow, App.MainWindow);
                CustomWindowHelper.RegisterWindow(DetailsWindow, viewId, viewModelType);
                await SetupWindowContentAsync(DetailsWindow, viewModelType, parameter);
                _themeSelectorService.ApplyThemeToWindow(DetailsWindow);
                _themeSelectorService.SubscribeToThemeChanges(DetailsWindow);
                return viewId;

            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateWindowAsync failed: {ex.Message}");
                return -1;
            }
        }
    }
}
