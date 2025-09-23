using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using MuhasibPro.ViewModels.Contracts.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels.Common;
using System.Collections.Concurrent;
using WinRT.Interop;

namespace MuhasibPro.Helpers
{
    public static class WindowHelper
    {
        private static readonly object _lock = new object();
        private static readonly ConcurrentDictionary<string, WindowInfo> _windows = new();
        private static readonly ConcurrentDictionary<Window, string> _windowIds = new();
        private static bool _isShowingCloseDialog = false;
        public static Window MainWindow { get; private set; }
        public static Window CurrentWindow => GetActiveWindow();
        public static XamlRoot CurrentXamlRoot => GetCurrnetXamlRoot();
        private static Window GetActiveWindow()
        {
            lock (_lock)
            {
                var activeWindow = _windows.Values
                    .Where(w => w.Window != null)
                    .OrderByDescending(w => w.LastActivated)
                    .FirstOrDefault()?.Window;
                return activeWindow ?? MainWindow;
            }
        }
        private static XamlRoot GetCurrnetXamlRoot()
        {
            var activeWindow = GetActiveWindow();
            if (activeWindow?.Content is FrameworkElement content && content.XamlRoot != null)
            {
                return content.XamlRoot;
            }

            throw new InvalidOperationException("Active window not found or XamlRoot is not accessible.");
        }
        public static void SetMainWindow(Window window)
        {
            lock (_lock)
            {
                MainWindow = window;
                if (window != null)
                {
                    RegisterWindow(window, "MainWindow", null);
                    window.AppWindow.Closing += OnMainWindowClosing;
                }
            }
        }
        private static async void OnMainWindowClosing(Microsoft.UI.Windowing.AppWindow sender,
            Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            if (_isShowingCloseDialog) return;
            _isShowingCloseDialog = true;

            try
            {
                var dialog = Ioc.Default.GetService<IDialogService>();
                bool shouldClose = await dialog.ShowConfirmationAsync(
                    "Uygulamayı Kapat",
                    "Uygulamadan çıkmak istediğinize emin misiniz?",
                    "Evet", "İptal");

                if (shouldClose)
                {
                    MainWindow.AppWindow.Closing -= OnMainWindowClosing;

                    // Tüm window'ları kapat
                    lock (_lock)
                    {
                        var allWindows = _windows.Values.ToList();
                        foreach (var windowInfo in allWindows)
                        {
                            if (windowInfo.Window != MainWindow)
                            {
                                windowInfo.Window.Close();
                            }
                        }
                    }

                    Application.Current.Exit();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Window closing error: {ex.Message}");
                MainWindow.AppWindow.Closing -= OnMainWindowClosing;
                Application.Current.Exit();
            }
            finally
            {
                _isShowingCloseDialog = false;
            }
        }
        public static void RegisterWindow(Window window, string windowId, Type viewModelType)
        {
            if (window == null) return;

            var windowInfo = new WindowInfo
            {
                Window = window,
                WindowId = windowId,
                ViewModelType = viewModelType,
            };

            lock (_lock)
            {
                _windows[windowId] = windowInfo;
                _windowIds[window] = windowId;
            }
            // Setup event handlers


            window.Closed += (s, e) =>
            {
                UnregisterWindow(windowId);

                // Eğer kapatılan pencere aktifse, MainWindow'u aktif yap
                if (window == CurrentWindow)
                {
                    MainWindow?.Activate();
                }
            };
        }
        private static void UnregisterWindow(string windowId)
        {
            lock (_lock)
            {
                if (_windows.TryRemove(windowId, out var windowInfo))
                {
                    _windowIds.TryRemove(windowInfo.Window, out _);
                }
            }
        }
        public static Window GetWindowForElement(FrameworkElement element)
        {
            if (element?.XamlRoot == null) return GetActiveWindow();

            // Element'in XamlRoot'una göre window bul
            var window = GetAllWindows()
                .FirstOrDefault(w => w.Content is FrameworkElement content &&
                               content.XamlRoot == element.XamlRoot);

            return window ?? GetActiveWindow();
        }
        public static List<Window> GetAllWindows()
        {
            lock (_lock)
            {
                return _windows.Values.Select(w => w.Window).Where(w => w != null).ToList();
            }
        }
        public static string GetWindowId(Window window)
        {
            if (window == null) return null;

            lock (_lock)
            {
                return _windowIds.TryGetValue(window, out var windowId) ? windowId : null;
            }
        }
        public static Window GetWindowByViewModel(Type viewModelType)
        {
            lock (_lock)
            {
                return _windows.Values
                    .FirstOrDefault(w => w.ViewModelType == viewModelType)?.Window;
            }
        }
        public static Window GetWindowById(string windowId)
        {
            if (string.IsNullOrEmpty(windowId)) return null;

            lock (_lock)
            {
                return _windows.TryGetValue(windowId, out var windowInfo) ? windowInfo.Window : null;
            }
        }
        public static void ActivateWindow(string windowId)
        {
            var window = GetWindowById(windowId);
            window?.Activate();
        }
        public static bool TryActivateExistingWindow(Type viewModelType)
        {
            if (!IsViewModelSingle(viewModelType)) return false;

            var existingWindow = GetWindowByViewModel(viewModelType);
            if (existingWindow == null) return false;

            var windowId = GetWindowId(existingWindow);
            if (windowId != null)
            {
                ActivateWindow(windowId);
                return true;
            }

            return false;
        }
        private static bool IsViewModelSingle(Type viewModelType)
        {
            return SingleInstanceViewModels._singleViewModel.Contains(viewModelType.Name);
        }
        public static string GenerateWindowId(Type viewModelType = null)
        {
            if (viewModelType != null)
            {
                return $"{viewModelType.Name}_{Guid.NewGuid():N}"[..24];
            }
            return $"Window_{Guid.NewGuid():N}";
        }
        public static void CenterWindow(Window window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                var displayArea = DisplayArea.Primary;
                var centerX = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
                var centerY = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;

                appWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
            }
        }
        /// <summary>
        /// Pencereyi ana pencereye göre konumlandır
        /// </summary>
        public static void PositionRelativeToParent(Window window, Window parentWindow)
        {
            if (parentWindow == null) return;

            var hWnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            var parentHWnd = WindowNative.GetWindowHandle(parentWindow);
            var parentWindowId = Win32Interop.GetWindowIdFromWindow(parentHWnd);
            var parentAppWindow = AppWindow.GetFromWindowId(parentWindowId);
            if (appWindow != null && parentAppWindow != null)
            {
                // Ana pencerenin merkezine konumlandır
                var offsetX = (parentAppWindow.Size.Width - appWindow.Size.Width) / 2;
                var offsetY = (parentAppWindow.Size.Height - appWindow.Size.Height) / 2;

                var newX = parentAppWindow.Position.X + offsetX;
                var newY = parentAppWindow.Position.Y + offsetY;

                appWindow.Move(new Windows.Graphics.PointInt32(newX, newY));

            }
        }
        public class WindowInfo
        {
            public Window Window { get; set; }
            public string WindowId { get; set; }
            public Type ViewModelType { get; set; }
            public DateTime LastActivated { get; set; } = DateTime.Now;
        }
    }
}


