using MuhasibPro.Views.Shell;

namespace MuhasibPro.Extensions.Common
{
    public static class AppTitleBarExtensions
    {
        // Title için Attached Property
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached(
                "Title",
                typeof(string),
                typeof(AppTitleBarExtensions),
                new PropertyMetadata(null, OnTitleChanged));

        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(TitleProperty);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(TitleProperty, value);
        }

        // Prefix için Attached Property (opsiyonel)
        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.RegisterAttached(
                "Prefix",
                typeof(string),
                typeof(AppTitleBarExtensions),
                new PropertyMetadata(null, OnTitleChanged));

        public static string GetPrefix(DependencyObject obj)
        {
            return (string)obj.GetValue(PrefixProperty);
        }

        public static void SetPrefix(DependencyObject obj, string value)
        {
            obj.SetValue(PrefixProperty, value);
        }

        // Property değiştiğinde tetiklenen metod
        //TODO: Ioc.Default.GetService<ShellView>(); kısmını düzelt
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                // MainShellView'e erişim (örneğin: Window.Current.Content üzerinden)
                //var shell = Ioc.Default.GetService<ShellView>();
                //shell.DispatcherQueue.TryEnqueue(() =>
                //{
                //    if (shell != null)
                //    {
                //        string title = GetTitle(element);
                //        string prefix = GetPrefix(element); // Opsiyonel
                //        shell.AppTitleBarText.Text = $"{prefix} {title}".Trim();
                //    }
                //});
            }
        }
    }
}
