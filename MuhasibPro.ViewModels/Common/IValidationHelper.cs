using Microsoft.UI.Xaml;

namespace MuhasibPro.ViewModels.Common
{
    public interface IValidationHelper
    {
        bool AreAllControlsValid();
        bool FocusFirstInvalidControl();
        List<string> GetValidationErrors();
        void RegisterAllControls(DependencyObject parent);
        void ClearRegisteredControls();
        event EventHandler<bool> AllControlsValidationChanged;
    }
}
