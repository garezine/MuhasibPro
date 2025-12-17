using DevWinUI;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Muhasib.Business.Services.Contracts.CommonServices;
using MuhasibPro.HostBuilders;
using System.Collections.Concurrent;
using WinRT.Interop;

namespace MuhasibPro.Helpers
{

    public static class CustomWindowHelper
    {
        private static readonly object _lock = new object();
        private static readonly ConcurrentDictionary<int, WindowInfo> _windows = new();
        private static readonly ConcurrentDictionary<Window, int> _windowIds = new();
        private static bool _isShowingCloseDialog = false;
        public static Window MainWindow { get; private set; }
        public static Window CurrentWindow => GetActiveWindow();
        public static XamlRoot CurrentXamlRoot => GetCurrentXamlRoot();

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

        public static int GetActiveWindowId()
        {
            return CurrentWindow != null ? GetWindowId(CurrentWindow) : -1;
        }
        public static bool TryActivateWindow(int windowId)
        {
            var window = GetWindowById(windowId);
            if (window != null)
            {
                WindowHelper.SwitchToThisWindow(window);

                window.Activate();

                WindowHelper.SetForegroundWindow(window);
                return true;
            }

            return false;
        }

        private static XamlRoot GetCurrentXamlRoot()
        {
            var activeWindow = GetActiveWindow();
            if (activeWindow?.Content is FrameworkElement content && content.XamlRoot != null)
            {
                return content.XamlRoot;
            }

            // Fallback: MainWindow'u deneyelim
            if (MainWindow?.Content is FrameworkElement mainContent && mainContent.XamlRoot != null)
            {
                return mainContent.XamlRoot;
            }

            throw new InvalidOperationException("No accessible XamlRoot found in any window.");
        }
        public static void SetMainWindow(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            lock (_lock)
            {
                MainWindow = window;
                try
                {
                    int mainWindowId = (int)window.AppWindow.Id.Value;
                    RegisterWindow(window, mainWindowId, null);
                    window.AppWindow.Closing += OnMainWindowClosing;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Main window registration failed: {ex.Message}");
                    throw;
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
                //TODO: DialogService'i düzelt
                var dialogService = ServiceLocator.Current.GetService<IDialogService>();
                bool shouldClose = await dialogService.ShowConfirmationAsync(
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
        public static void RegisterWindow(Window window, int windowId, Type viewModelType)
        {
            if (window == null) return;

            var windowInfo = new WindowInfo
            {
                Window = window,
                WindowId = windowId,
                ViewModelType = viewModelType
            };

            lock (_lock)
            {
                _windows[windowId] = windowInfo;
                _windowIds[window] = windowId;
            }

            // Window aktivasyon olayını dinle
            window.Activated += (s, e) =>
            {
                lock (_lock)
                {
                    if (_windows.TryGetValue(windowId, out var info))
                    {
                        info.LastActivated = DateTime.Now;
                    }
                }
            };

            window.Closed += (s, e) =>
            {
                UnregisterWindow(windowId);

                // Deadlock'u önlemek için doğrudan kontrol
                var activeWindow = GetActiveWindow();
                if (window == activeWindow)
                {
                    MainWindow?.Activate();
                }
            };
        }
        private static void UnregisterWindow(int windowId)
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
        public static Window GetWindowByViewModel(Type viewModelType)
        {
            lock (_lock)
            {
                return _windows.Values
                    .FirstOrDefault(w => w.ViewModelType == viewModelType)?.Window;
            }
        }
        public static bool TryActivateExistingWindow(Type viewModelType)
        {
            var existingWindow = GetWindowByViewModel(viewModelType);
            if (existingWindow == null) return false;

            var windowId = GetWindowId(existingWindow);
            if (windowId > 0)
            {
                ActivateWindow(windowId);
                return true;
            }
            return false;
        }
        public static List<Window> GetAllWindows()
        {
            lock (_lock)
            {
                return _windows.Values.Select(w => w.Window).Where(w => w != null).ToList();
            }
        }
        public static int GetWindowId(Window window)
        {
            if (window == null) return 0;

            lock (_lock)
            {
                return _windowIds.TryGetValue(window, out var windowId) ? windowId : 0;
            }
        }

        public static Window GetWindowById(int windowId)
        {
            if (windowId < -1) return null;

            lock (_lock)
            {
                return _windows.TryGetValue(windowId, out var windowInfo) ? windowInfo.Window : null;
            }
        }
        public static void ActivateWindow(int windowId)
        {
            try
            {
                var window = GetWindowById(windowId);
                window?.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error activating window {windowId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Pencereyi ana pencereye göre konumlandır
        /// </summary>
        public static void PositionRelativeToParent(Window window, Window parentWindow)
        {
            if (parentWindow == null || window == null) return;

            try
            {
                var hWnd = WindowNative.GetWindowHandle(window);
                var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                var parentHWnd = WindowNative.GetWindowHandle(parentWindow);
                var parentWindowId = Win32Interop.GetWindowIdFromWindow(parentHWnd);
                var parentAppWindow = AppWindow.GetFromWindowId(parentWindowId);

                if (appWindow != null && parentAppWindow != null)
                {
                    // Ekran sınırlarını kontrol et
                    var offsetX = (parentAppWindow.Size.Width - appWindow.Size.Width) / 2;
                    var offsetY = (parentAppWindow.Size.Height - appWindow.Size.Height) / 2;

                    var newX = Math.Max(0, parentAppWindow.Position.X + offsetX);
                    var newY = Math.Max(0, parentAppWindow.Position.Y + offsetY);

                    appWindow.Move(new Windows.Graphics.PointInt32(newX, newY));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error positioning window: {ex.Message}");
            }
        }
        public static void Cleanup()
        {
            lock (_lock)
            {
                // Event handler'ları temizle
                foreach (var windowInfo in _windows.Values)
                {
                    if (windowInfo.Window == MainWindow)
                    {
                        try
                        {
                            windowInfo.Window.AppWindow.Closing -= OnMainWindowClosing;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error removing event handler: {ex.Message}");
                        }
                    }
                }

                _windows.Clear();
                _windowIds.Clear();
                MainWindow = null;
            }
        }
        public class WindowInfo
        {
            public Window Window { get; set; }
            public int WindowId { get; set; }
            public Type ViewModelType { get; set; }
            public DateTime LastActivated { get; set; } = DateTime.Now;
        }
    }
}


