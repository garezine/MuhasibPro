using CommunityToolkit.WinUI;
using MuhasibPro.Contracts.CoreServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.Views.Shell;
using MuhasibPro.Views.ViewWindow;
using WinUIEx;

namespace MuhasibPro.Helpers
{
    public static class NavigationHelper
    {
        private static async Task SetupWindowContentAsync(Window window, Type viewModelType, object parameter)
        {
            await window.DispatcherQueue
                .EnqueueAsync(
                    () =>
                    {
                        var themeSelectorService = ServiceLocator.Current.GetService<IThemeSelectorService>();
                        var frame = new Frame();
                        var args = new ShellArgs { ViewModel = viewModelType, Parameter = parameter };
                        frame.Navigate(typeof(ShellView), args);
                        window.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
                        window.Content = frame;
                        if (window.Content is FrameworkElement element)
                        {
                            element.RequestedTheme = themeSelectorService.Theme;
                        }
                    });
        }

        public static async Task CloseWindowAsync(int windowId)
        {
            var window = CustomWindowHelper.GetWindowById(windowId);
            if (window == null || window == App.MainWindow)
                return;

            try
            {
                if (window.Content is Frame frame && frame.Content is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                await window.DispatcherQueue.EnqueueAsync(() => window.Close());

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Window close error: {ex.Message}");
            }
        }

        public static async Task<int> CreateWindowAsync(Type viewModelType, object parameter, string customTitle)
        {
            try
            {
                if (CustomWindowHelper.TryActivateExistingWindow(viewModelType))
                {
                    var existingWindow = CustomWindowHelper.GetWindowByViewModel(viewModelType);
                    return CustomWindowHelper.GetWindowId(existingWindow);
                }

                var detailsWindow = new DetailsWindow();
                int viewId = (int)detailsWindow.AppWindow.Id.Value;

                detailsWindow.Title = customTitle;
                CustomWindowHelper.RegisterWindow(detailsWindow, viewId, viewModelType);
                await SetupWindowContentAsync(detailsWindow, viewModelType, parameter);
                CustomWindowHelper.PositionRelativeToParent(detailsWindow, App.MainWindow);

                return viewId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateWindowAsync failed: {ex.Message}");
                return -1;
            }
        }
    }
}
