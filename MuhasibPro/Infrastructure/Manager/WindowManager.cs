using CommunityToolkit.Mvvm.DependencyInjection;
using MuhasibPro.Contracts.BaseAppServices;
using MuhasibPro.Extensions;
using MuhasibPro.Views.Shell;
using MuhasibPro.Views.ViewWindow;
using WinUIEx;
using MuhasibPro.Helpers;
using MuhasibPro.ViewModels.Shell;

namespace MuhasibPro.Infrastructure.Manager
{
    public static class WindowManager
    {
        private static async Task SetupWindowContentAsync(Window window, Type viewModelType, object parameter)
        {
            await window.DispatcherQueue
                .EnqueueAsync(
                    () =>
                    {
                        var themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
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

        public static bool DetermineWindowType(Type viewModelType)
        {
            // ViewModel adına göre fallback
            string viewModelName = viewModelType.Name;
            if (viewModelName.Contains("List", StringComparison.OrdinalIgnoreCase))
                return true;

            if (viewModelName.Contains("Detail", StringComparison.OrdinalIgnoreCase) ||
                viewModelName.Contains("Edit", StringComparison.OrdinalIgnoreCase) ||
                viewModelName.Contains("Create", StringComparison.OrdinalIgnoreCase))
                return false;

            return true; // Default
        }

        public static async Task CloseWindowAsync(string windowId)
        {
            var window = WindowHelper.GetWindowById(windowId);
            if (window == null || window == App.MainWindow)
                return;

            try
            {
                if (window.Content is Frame frame && frame.Content is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                await window.DispatcherQueue.EnqueueAsync(() => window.Close());
                var isMain = window.GetType().Name == "DetailWindow";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Window close error: {ex.Message}");
            }
        }

        private static Window CurrentWindow { get; set; } = new Window();

        public static async Task<Window> CreateWindowAsync<TViewModel>(object parameter, string customTitle)
        {
            var viewModelType = typeof(TViewModel);
            // Check for existing single instance
            if (WindowHelper.TryActivateExistingWindow(viewModelType))
            {
                return WindowHelper.GetWindowByViewModel(viewModelType);
            }
            CurrentWindow = new DetailsWindow();

            CurrentWindow.Title = customTitle;
            var windowId = WindowHelper.GenerateWindowId(viewModelType);
            WindowHelper.RegisterWindow(CurrentWindow, windowId, viewModelType);
            await SetupWindowContentAsync(CurrentWindow, viewModelType, parameter);
            WindowHelper.PositionRelativeToParent(CurrentWindow, App.MainWindow);

            CurrentWindow.Activate();

            return CurrentWindow;
        }

    }
}
