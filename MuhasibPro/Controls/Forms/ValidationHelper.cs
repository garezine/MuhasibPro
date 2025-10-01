namespace MuhasibPro.Controls;

public static class ValidationHelper
{
    #region PropertyName Attached Property

    public static string GetPropertyName(DependencyObject obj)
    {
        return (string)obj.GetValue(PropertyNameProperty);
    }

    public static void SetPropertyName(DependencyObject obj, string value)
    {
        obj.SetValue(PropertyNameProperty, value);
    }

    public static readonly DependencyProperty PropertyNameProperty =
        DependencyProperty.RegisterAttached(
            "PropertyName",
            typeof(string),
            typeof(ValidationHelper),
            new PropertyMetadata(null));

    #endregion

    #region ValidationSource Attached Property (ViewModel referansı)

    public static object GetValidationSource(DependencyObject obj)
    {
        return obj.GetValue(ValidationSourceProperty);
    }

    public static void SetValidationSource(DependencyObject obj, object value)
    {
        obj.SetValue(ValidationSourceProperty, value);
    }

    public static readonly DependencyProperty ValidationSourceProperty =
        DependencyProperty.RegisterAttached(
            "ValidationSource",
            typeof(object),
            typeof(ValidationHelper),
            new PropertyMetadata(null));

    #endregion
}