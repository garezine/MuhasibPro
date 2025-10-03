using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.Contracts.CommonServices
{
    public interface INotificationService
    {
        
        void Initialize(InfoBar infoBar);
        void ShowError(string title, string message, int autoCloseDuration = 5000);
        void ShowWarning(string title, string message, int autoCloseDuration = 5000);
        void ShowSuccess(string title, string message, int autoCloseDuration = 3000);
        void ShowInfo(string title, string message, int autoCloseDuration = 4000);
        void Close();
    }
}
