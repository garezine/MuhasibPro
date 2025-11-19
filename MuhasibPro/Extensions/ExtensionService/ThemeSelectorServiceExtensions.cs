using Microsoft.UI;
using Microsoft.UI.Windowing;
using MuhasibPro.Contracts.CoreServices;
using Windows.UI;

namespace MuhasibPro.Extensions.ExtensionService
{
    public static class ThemeSelectorServiceExtensions
    {
        public static void ApplyThemeToWindow(this IThemeSelectorService themeSelectorService, Window window)
        {
            if (window?.Content is not FrameworkElement rootElement) return;

            // 1. İlk önce içerik temasını uygula
            rootElement.RequestedTheme = themeSelectorService.Theme switch
            {
                ElementTheme.Dark => ElementTheme.Dark,
                ElementTheme.Light => ElementTheme.Light,
                _ => ElementTheme.Default
            };

            // 2. Title bar renklerini güncelle
            themeSelectorService.UpdateTitleBarColors(window);
        }

        public static void UpdateTitleBarColors(this IThemeSelectorService themeSelectorService, Window window)
        {
            if (window?.AppWindow?.TitleBar == null) return;

            var titleBar = window.AppWindow.TitleBar;
            var isLightTheme = themeSelectorService.Theme == ElementTheme.Light;

            // ÖNEMLİ: ExtendsContentIntoTitleBar kontrolü
            if (!window.ExtendsContentIntoTitleBar)
            {
                window.ExtendsContentIntoTitleBar = true;
            }

            // BUTON RENKLERİ
            if (isLightTheme)
            {
                ApplyLightTheme(titleBar);
            }
            else
            {
                ApplyDarkTheme(titleBar);
            }

            // ORTAK AYARLAR
            ApplyCommonTitleBarSettings(titleBar);
        }

        private static void ApplyLightTheme(AppWindowTitleBar titleBar)
        {
            // AÇIK TEMA - Siyah ikonlar
            titleBar.ButtonForegroundColor = Colors.Black;
            titleBar.ButtonHoverForegroundColor = Colors.Black;
            titleBar.ButtonPressedForegroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Colors.Gray;

            // Hover efektleri
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(25, 0, 0, 0);
            titleBar.ButtonPressedBackgroundColor = Color.FromArgb(50, 0, 0, 0);
        }

        private static void ApplyDarkTheme(AppWindowTitleBar titleBar)
        {
            // KOYU TEMA - Beyaz ikonlar
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonHoverForegroundColor = Colors.White;
            titleBar.ButtonPressedForegroundColor = Colors.Black;
            titleBar.ButtonInactiveForegroundColor = Colors.DarkGray;

            // Hover efektleri
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(25, 255, 255, 255);
            titleBar.ButtonPressedBackgroundColor = Color.FromArgb(50, 255, 255, 255);
        }

        private static void ApplyCommonTitleBarSettings(AppWindowTitleBar titleBar)
        {
            // Background'ları şeffaf yap
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        // EVENT HANDLING EXTENSION'ı
        public static void SubscribeToThemeChanges(this IThemeSelectorService themeSelectorService, Window window)
        {
            if (themeSelectorService == null || window == null) return;

            void ThemeChangedHandler(object sender, ElementTheme theme)
            {
                // UI thread'de çalıştığından emin ol
                _ = window.DispatcherQueue.TryEnqueue(() =>
                {
                    themeSelectorService.ApplyThemeToWindow(window);
                });
            }

            // Event'e subscribe ol
            themeSelectorService.ThemeChanged += ThemeChangedHandler;

            // Window kapandığında unsubscribe ol
            window.Closed += (s, e) =>
            {
                themeSelectorService.ThemeChanged -= ThemeChangedHandler;
            };
        }
    }
}