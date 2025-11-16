using MuhasibPro.ViewModels.Infrastructure.Common;

namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices;
public interface IDialogService
{
    // Mevcut metotlar
    Task ShowAsync(string title, Exception ex, string ok = "Tamam");
    Task<bool> ShowAsync(string title, string content, string ok = "Tamam", string cancel = null);
    Task ShowAsync(Result result, string ok = "Tamam");

    // Yeni modern dialog metotları
    Task ShowSuccessAsync(string title, string content, string ok = "Tamam");
    Task ShowErrorAsync(string title, string content, string ok = "Tamam");
    Task ShowWarningAsync(string title, string content, string ok = "Tamam");
    Task ShowInfoAsync(string title, string content, string ok = "Tamam");
    Task<bool> ShowConfirmationAsync(string title, string content, string confirmText = "Evet", string cancelText = "Hayır");
    Task<string> ShowInputAsync(string title, string placeholder, string defaultValue = "");

}
