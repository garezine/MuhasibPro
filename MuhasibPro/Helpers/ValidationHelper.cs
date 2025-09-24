using Microsoft.UI.Xaml.Media;
using MuhasibPro.Controls;
using MuhasibPro.ViewModels.Common;

namespace MuhasibPro.Helpers;

public class ValidationHelper : IValidationHelper
{ 
    private List<FormTextBox> _registeredControls = new List<FormTextBox>();
    private bool _allControlsValid = true;

    public event EventHandler<bool> AllControlsValidationChanged;

    public ValidationHelper()
    {
        FormTextBox.GlobalValidationChanged += OnControlValidationChanged;
    }   

    /// <summary>
    /// Bir FormTextBox'ı validasyon takibine ekler
    /// </summary>
    public void RegisterControl(FormTextBox control)
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
    public void RegisterAllControls(DependencyObject parent)
    {
        FindAndRegisterControls(parent);
        CheckAllControlsValidation();
    }

    private void FindAndRegisterControls(DependencyObject parent)
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
    public bool AreAllControlsValid()
    {
        return _registeredControls.All(c => c.IsValid);
    }

    /// <summary>
    /// İlk geçersiz kontrole focus yapar
    /// </summary>
    public bool FocusFirstInvalidControl()
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
    public List<FormTextBox> GetInvalidControls()
    {
        return _registeredControls.Where(c => !c.IsValid).ToList();
    }

    /// <summary>
    /// Geçersiz kontrollerin hata mesajlarını döndürür
    /// </summary>
    public List<string> GetValidationErrors()
    {
        return _registeredControls
            .Where(c => !c.IsValid && !string.IsNullOrEmpty(c.ValidationMessage))
            .Select(c => c.ValidationMessage)
            .ToList();
    }

    /// <summary>
    /// Tüm kontrolleri temizler (sayfa değişimi vs. için)
    /// </summary>
    public void ClearRegisteredControls()
    {
        _registeredControls.Clear();
        CheckAllControlsValidation();
    }

    private void OnControlValidationChanged(object sender, bool isValid)
    {
        CheckAllControlsValidation();
    }

    private void CheckAllControlsValidation()
    {
        bool newValidationState = AreAllControlsValid();

        if (_allControlsValid != newValidationState)
        {
            _allControlsValid = newValidationState;
            AllControlsValidationChanged?.Invoke(null, _allControlsValid);
        }
    }
}
