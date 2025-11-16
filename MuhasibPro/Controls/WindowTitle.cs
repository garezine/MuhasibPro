using Microsoft.UI.Xaml.Media;
using MuhasibPro.Views.Shell;

namespace MuhasibPro.Controls;

public class WindowTitle : Control
{
    public string Prefix
    {
        get
        {
            return (string)GetValue(PrefixProperty);
        }
        set
        {
            SetValue(PrefixProperty, value);
        }
    }
    public static readonly DependencyProperty PrefixProperty = DependencyProperty.Register(nameof(Prefix), typeof(string), typeof(WindowTitle), new PropertyMetadata(null, TitleChanged));

    public string Title
    {
        get
        {
            return (string)GetValue(TitleProperty);
        }
        set
        {
            SetValue(TitleProperty, value);
        }
    }
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(WindowTitle), new PropertyMetadata(null, TitleChanged));

    private static void TitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowTitle control)
        {
            var page = FindParent<ShellView>(control);
            if (page != null && page.FindName("AppTitleBarText") is TextBlock titleText)
            {
                titleText.Text = $"{control.Prefix} {control.Title}".Trim();
            }
        }

    }
    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is T typedParent)
            {
                return typedParent;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}
