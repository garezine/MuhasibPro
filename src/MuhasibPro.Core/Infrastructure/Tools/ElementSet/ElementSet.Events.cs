using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;

namespace MuhasibPro.Core.Infrastructure.Tools.ElementSet;

partial class ElementSet<T>
{
    public event RoutedEventHandler Click
    {
        add => ForEach<Button>(v => v.Click += value);
        remove => ForEach<Button>(v => v.Click -= value);
    }

    public event TappedEventHandler Tapped
    {
        add => ForEach<FrameworkElement>(v => v.Tapped += value);
        remove => ForEach<FrameworkElement>(v => v.Tapped -= value);
    }

    public event PointerEventHandler PointerEntered
    {
        add => ForEach<FrameworkElement>(v => v.PointerEntered += value);
        remove => ForEach<FrameworkElement>(v => v.PointerEntered -= value);
    }
    public event PointerEventHandler PointerExited
    {
        add => ForEach<FrameworkElement>(v => v.PointerExited += value);
        remove => ForEach<FrameworkElement>(v => v.PointerExited -= value);
    }

    public event RoutedEventHandler GotFocus
    {
        add => ForEach<UIElement>(v => v.GotFocus += value);
        remove => ForEach<UIElement>(v => v.GotFocus -= value);
    }
    public event RoutedEventHandler LostFocus
    {
        add => ForEach<UIElement>(v => v.LostFocus += value);
        remove => ForEach<UIElement>(v => v.LostFocus -= value);
    }
}
