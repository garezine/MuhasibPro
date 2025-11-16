using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using MuhasibPro.Helpers;
using System.Numerics;

namespace MuhasibPro.Extensions;

public static class AnimationExtensions
{
    public static void Fade(this UIElement element, double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
    {
        element.StartAnimation(nameof(Visual.Opacity), CreateScalarAnimation(milliseconds, start, end, easingFunction));
    }

    public static void TranslateX(this UIElement element, double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
    {
        ElementCompositionPreview.SetIsTranslationEnabled(element, true);
        element.StartAnimation("Translation.X", CreateScalarAnimation(milliseconds, start, end, easingFunction));
    }

    public static void TranslateY(this UIElement element, double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
    {
        ElementCompositionPreview.SetIsTranslationEnabled(element, true);
        element.StartAnimation("Translation.Y", CreateScalarAnimation(milliseconds, start, end, easingFunction));
    }

    public static void Scale(this FrameworkElement element, double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
    {
        element.SetCenterPoint(element.ActualWidth / 2.0, element.ActualHeight / 2.0);
        var vectorStart = new Vector3((float)start, (float)start, 0);
        var vectorEnd = new Vector3((float)end, (float)end, 0);
        element.StartAnimation(nameof(Visual.Scale), CreateVector3Animation(milliseconds, vectorStart, vectorEnd, easingFunction));
    }

    public static void Blur(this UIElement element, double amount)
    {
        var brush = CreateBlurEffectBrush(amount);
        element.SetBrush(brush);
    }
    public static void Blur(this UIElement element, double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
    {
        var brush = CreateBlurEffectBrush();
        element.SetBrush(brush);
        brush.StartAnimation("Blur.BlurAmount", CreateScalarAnimation(milliseconds, start, end, easingFunction));
    }

    public static void Grayscale(this UIElement element)
    {
        var brush = CreateGrayscaleEffectBrush();
        element.SetBrush(brush);
    }

    public static void SetBrush(this UIElement element, CompositionBrush brush)
    {
        var spriteVisual = CreateSpriteVisual(element);
        spriteVisual.Brush = brush;
        ElementCompositionPreview.SetElementChildVisual(element, spriteVisual);
    }

    public static void ClearEffects(this UIElement element)
    {
        ElementCompositionPreview.SetElementChildVisual(element, null);
    }

    public static SpriteVisual CreateSpriteVisual(UIElement element)
    {
        return CreateSpriteVisual(ElementCompositionPreview.GetElementVisual(element));
    }
    public static SpriteVisual CreateSpriteVisual(Visual elementVisual)
    {
        var compositor = elementVisual.Compositor;
        var spriteVisual = compositor.CreateSpriteVisual();
        var expression = compositor.CreateExpressionAnimation();
        expression.Expression = "visual.Size";
        expression.SetReferenceParameter("visual", elementVisual);
        spriteVisual.StartAnimation(nameof(Visual.Size), expression);
        return spriteVisual;
    }

    public static void SetCenterPoint(this UIElement element, double x, double y)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        visual.CenterPoint = new Vector3((float)x, (float)y, 0);
    }

    public static void StartAnimation(this UIElement element, string propertyName, CompositionAnimation animation)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        visual.StartAnimation(propertyName, animation);
    }

    public static CompositionAnimation CreateScalarAnimation(double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
    {
        var animation = CustomWindowHelper.CurrentWindow.Compositor.CreateScalarKeyFrameAnimation();
        animation.InsertKeyFrame(0.0f, (float)start, easingFunction);
        animation.InsertKeyFrame(1.0f, (float)end, easingFunction);
        animation.Duration = TimeSpan.FromMilliseconds(milliseconds);
        return animation;
    }

    public static CompositionAnimation CreateVector3Animation(double milliseconds, Vector3 start, Vector3 end, CompositionEasingFunction easingFunction = null)
    {
        var animation = CustomWindowHelper.CurrentWindow.Compositor.CreateVector3KeyFrameAnimation();
        animation.InsertKeyFrame(0.0f, start);
        animation.InsertKeyFrame(1.0f, end);
        animation.Duration = TimeSpan.FromMilliseconds(milliseconds);
        return animation;
    }

    public static CompositionEffectBrush CreateBlurEffectBrush(double amount = 0.0)
    {
        var effect = new GaussianBlurEffect
        {
            Name = "Blur",
            BlurAmount = (float)amount,
            Source = new CompositionEffectSourceParameter("source")
        };

        var compositor = CustomWindowHelper.CurrentWindow.Compositor;
        var factory = compositor.CreateEffectFactory(effect, new[] { "Blur.BlurAmount" });
        var brush = factory.CreateBrush();
        brush.SetSourceParameter("source", compositor.CreateBackdropBrush());
        return brush;
    }

    public static CompositionEffectBrush CreateGrayscaleEffectBrush()
    {
        var effect = new GrayscaleEffect
        {
            Name = "Grayscale",
            Source = new CompositionEffectSourceParameter("source")
        };

        var compositor = CustomWindowHelper.CurrentWindow.Compositor;
        var factory = compositor.CreateEffectFactory(effect);
        var brush = factory.CreateBrush();
        brush.SetSourceParameter("source", compositor.CreateBackdropBrush());
        return brush;
    }
}
