using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Infrastructure.Controls;

namespace MuhasibPro.Infrastructure.Infrastructure.Common;

public static class ValidationHelper
{
    private static List<FormTextBox> _registeredControls = new List<FormTextBox>();
    private static bool _allControlsValid = true;

    public static event EventHandler<bool> AllControlsValidationChanged;

    static ValidationHelper()
    {
        FormTextBox.GlobalValidationChanged += OnControlValidationChanged;
    }

    /// <summary>
    /// Bir FormTextBox'ı validasyon takibine ekler
    /// </summary>
    public static void RegisterControl(FormTextBox control)
    {
        if (!_registeredControls.Contains(control))
        {
            _registeredControls.Add(control);

            // Control kaldırıldığında listeden çıkar
            control.Unloaded += (s, e) =>
            {
                _registeredControls.Remove(control);
                CheckAllControlsValidation();
            };
        }
    }

    /// <summary>
    /// Belirtilen parent içindeki tüm FormTextBox'ları otomatik kaydet
    /// </summary>
    public static void RegisterAllControls(DependencyObject parent)
    {
        FindAndRegisterControls(parent);
        CheckAllControlsValidation();
    }

    private static void FindAndRegisterControls(DependencyObject parent)
    {
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is FormTextBox formTextBox)
            {
                RegisterControl(formTextBox);
            }

            FindAndRegisterControls(child);
        }
    }

    /// <summary>
    /// Tüm kayıtlı kontrollerin validasyon durumunu kontrol eder
    /// </summary>
    public static bool AreAllControlsValid()
    {
        return _registeredControls.All(c => c.IsValid);
    }

    /// <summary>
    /// İlk geçersiz kontrole focus yapar
    /// </summary>
    public static bool FocusFirstInvalidControl()
    {
        var invalidControl = _registeredControls.FirstOrDefault(c => !c.IsValid);
        if (invalidControl != null)
        {
            invalidControl.Focus(FocusState.Programmatic);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Geçersiz kontrollerin listesini döndürür
    /// </summary>
    public static List<FormTextBox> GetInvalidControls()
    {
        return _registeredControls.Where(c => !c.IsValid).ToList();
    }

    /// <summary>
    /// Geçersiz kontrollerin hata mesajlarını döndürür
    /// </summary>
    public static List<string> GetValidationErrors()
    {
        return _registeredControls
            .Where(c => !c.IsValid && !string.IsNullOrEmpty(c.ValidationMessage))
            .Select(c => c.ValidationMessage)
            .ToList();
    }

    /// <summary>
    /// Tüm kontrolleri temizler (sayfa değişimi vs. için)
    /// </summary>
    public static void ClearRegisteredControls()
    {
        _registeredControls.Clear();
        CheckAllControlsValidation();
    }

    private static void OnControlValidationChanged(object sender, bool isValid)
    {
        CheckAllControlsValidation();
    }

    private static void CheckAllControlsValidation()
    {
        bool newValidationState = AreAllControlsValid();

        if (_allControlsValid != newValidationState)
        {
            _allControlsValid = newValidationState;
            AllControlsValidationChanged?.Invoke(null, _allControlsValid);
        }
    }
}
