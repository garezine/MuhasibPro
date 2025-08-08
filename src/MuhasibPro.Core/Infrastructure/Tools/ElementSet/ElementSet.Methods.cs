using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Composition;

namespace MuhasibPro.Core.Infrastructure.Tools.ElementSet;

partial class ElementSet<T>
{
    public ElementSet<T> SetIsTranslationEnabled(bool value) => ForEach(e => ElementCompositionPreview.SetIsTranslationEnabled(e, value));
    public ElementSet<T> SetImplicitShowAnimation(ICompositionAnimationBase animation) => ForEach(e => ElementCompositionPreview.SetImplicitShowAnimation(e, animation));
    public ElementSet<T> SetImplicitHideAnimation(ICompositionAnimationBase animation) => ForEach(e => ElementCompositionPreview.SetImplicitHideAnimation(e, animation));

    public ElementSet<T> Show() => ForEach(e => e.Visibility = Visibility.Visible);
    public ElementSet<T> Hide() => ForEach(e => e.Visibility = Visibility.Collapsed);

    public void Focus(FocusState value) => FirstOrDefault<Control>()?.Focus(value);
}
