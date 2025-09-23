using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.ViewModels.Contracts.CommonServices;
public interface IDialogService
{
    Task<ContentDialogResult> ShowDialogAsync(string title, string content, string primaryButtonText = "Tamam", string secondaryButtonText = null, string closeButtonText = "İptal");
    Task<ContentDialogResult> ShowCustomDialogAsync(ContentDialog dialog);
    Task ShowMessageAsync(string title, string message);
    Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Evet", string noText = "Hayır");
    Task<string> ShowInputDialogAsync(string title, string placeholder = "", string defaultText = "");
}
