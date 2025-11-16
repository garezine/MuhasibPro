namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices
{
    public interface INotificationService
    {
        void ShowError(string title, string message, int autoCloseDuration = 5000);
        void ShowWarning(string title, string message, int autoCloseDuration = 5000);
        void ShowSuccess(string title, string message, int autoCloseDuration = 3000);
        void ShowInfo(string title, string message, int autoCloseDuration = 4000);
        void Close();
    }
}
