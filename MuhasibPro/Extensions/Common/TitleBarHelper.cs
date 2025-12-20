using Microsoft.UI;
using System.Runtime.InteropServices;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace MuhasibPro.Extensions.Common;

// Helper class to workaround custom title bar bugs.
// DISCLAIMER: The resource key names and color values used below are subject to change. Do not depend on them.
// https://github.com/microsoft/TemplateStudio/issues/4516
internal class TitleBarHelper
{
    private const int WAINACTIVE = 0x00;
    private const int WAACTIVE = 0x01;
    private const int WMACTIVATE = 0x0006;

    [DllImport("user32.dll")]
    private static extern nint GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern nint SendMessage(nint hWnd, int msg, int wParam, nint lParam);

    public static void UpdateTitleBar(ElementTheme theme)
    {
        if (App.MainWindow == null || !App.MainWindow.ExtendsContentIntoTitleBar)
            return;

        try
        {
            // Tema belirlenmemişse, sistem temasını kullan
            if (theme == ElementTheme.Default)
            {
                var uiSettings = new UISettings();
                var background = uiSettings.GetColorValue(UIColorType.Background);
                theme = background == Colors.White ? ElementTheme.Light : ElementTheme.Dark;
            }

            var titleBar = App.MainWindow.AppWindow.TitleBar;

            // Başlık çubuğu düğmeleri için renkleri ayarla
            if (theme == ElementTheme.Dark)
            {
                // Koyu tema
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(0xFF, 0x99, 0x99, 0x99);
            }
            else
            {
                // Açık tema
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0x00, 0x00, 0x00);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x66, 0x00, 0x00, 0x00);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(0xFF, 0x66, 0x66, 0x66);
            }

            // Şeffaf arka plan
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.InactiveBackgroundColor = Colors.Transparent;

            // Pencereyi yeniden etkinleştir (başlık çubuğunu yenilemek için)
            RefreshWindow();
        }
        catch (Exception ex)
        {
            // Hata durumunda loglama yapabilirsiniz
            System.Diagnostics.Debug.WriteLine($"TitleBar güncellenirken hata: {ex.Message}");
        }
    }

    private static void RefreshWindow()
    {
        try
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            if (hwnd == GetActiveWindow())
            {
                // Pencere aktifse, geçici olarak inaktif yapıp tekrar aktif yap
                SendMessage(hwnd, WMACTIVATE, WAINACTIVE, nint.Zero);
                SendMessage(hwnd, WMACTIVATE, WAACTIVE, nint.Zero);
            }
            else
            {
                // Pencere inaktifse, geçici olarak aktif yapıp tekrar inaktif yap
                SendMessage(hwnd, WMACTIVATE, WAACTIVE, nint.Zero);
                SendMessage(hwnd, WMACTIVATE, WAINACTIVE, nint.Zero);
            }
        }
        catch
        {
            // SendMessage hatalarını görmezden gel
        }
    }

    public static void ApplySystemThemeToCaptionButtons()
    {
        if (App.MainWindow == null) return;

        // Ana pencerenin temasını kullan
        UpdateTitleBar(App.ThemeService.Theme);
    }
}

