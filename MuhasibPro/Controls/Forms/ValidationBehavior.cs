namespace MuhasibPro.Controls;

public static class ValidationBehavior
{
    #region Errors Attached Property

    public static Dictionary<string, List<string>> GetErrors(DependencyObject obj)
    {
        return (Dictionary<string, List<string>>)obj.GetValue(ErrorsProperty);
    }

    public static void SetErrors(DependencyObject obj, Dictionary<string, List<string>> value)
    {
        obj.SetValue(ErrorsProperty, value);
    }

    public static readonly DependencyProperty ErrorsProperty =
        DependencyProperty.RegisterAttached(
            "Errors",
            typeof(Dictionary<string, List<string>>),
            typeof(ValidationBehavior),
            new PropertyMetadata(null, OnErrorsChanged));

    private static void OnErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Panel panel)
        {
            var errors = e.NewValue as Dictionary<string, List<string>>;

            // Her değişiklikte tüm kontrolleri güncelle
            ApplyValidationErrors(panel, errors ?? new Dictionary<string, List<string>>());
        }
    }

    #endregion

    private static void ApplyValidationErrors(Panel panel, Dictionary<string, List<string>> errors)
    {
        // Önce tüm hataları temizle
        ClearAllErrors(panel);

        if (errors == null || errors.Count == 0)
            return;

        // Tüm FormTextBox'ları bul ve ilgili hataları göster
        var formControls = FindVisualChildren<IFormControl>(panel);

        foreach (var control in formControls)
        {
            if (control is FrameworkElement element)
            {
                var propertyName = ValidationHelper.GetPropertyName(element);

                if (!string.IsNullOrEmpty(propertyName) && errors.ContainsKey(propertyName))
                {
                    var errorMessages = errors[propertyName];
                    if (errorMessages.Count > 0)
                    {
                        // Tüm hataları birleştir
                        var errorMessage = string.Join("\n", errorMessages);
                        control.SetError(errorMessage);
                    }
                }
            }
        }
    }

    private static void ClearAllErrors(Panel panel)
    {
        var formControls = FindVisualChildren<IFormControl>(panel);
        foreach (var control in formControls)
        {
            control.ClearError();
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : class
    {
        if (parent == null)
            yield break;

        var childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childCount; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild)
            {
                yield return typedChild;
            }

            foreach (var descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }
}