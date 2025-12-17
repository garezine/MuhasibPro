using Muhasib.Business.Infrastructure.Models;
using Muhasib.Business.Services.Contracts.CommonServices;
using MuhasibPro.Contracts.CoreServices;
using MuhasibPro.Controls;
using MuhasibPro.Helpers;

namespace MuhasibPro.Services.Infrastructure.CommonServices
{
    public class DialogService : IDialogService
    {
        private readonly IThemeSelectorService _themeSelectorService;

        public DialogService(IThemeSelectorService themeSelectorService)
        {
            _themeSelectorService = themeSelectorService;
        }

        private CustomContentDialog CreateDialog(string title, object content, CustomContentDialog.DialogIcon icon)
        {
            var xamlRoot = CustomWindowHelper.CurrentXamlRoot;
            if (xamlRoot == null)
                throw new InvalidOperationException("Aktif pencere bulunamadı.");

            var dialog = new CustomContentDialog
            {
                XamlRoot = xamlRoot,
                RequestedTheme = _themeSelectorService.Theme,
                Content = content,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
            };

            dialog.Title = title;
            dialog.SetIcon(icon);

            return dialog;
        }

        // MEVCUT METOTLAR
        public async Task ShowAsync(string title, Exception ex, string ok = "Ok")
        {
            await ShowErrorAsync(title, ex.Message, ok);
        }

        public async Task ShowAsync(Result result, string ok = "Tamam")
        {
            if (result.IsOk)
                await ShowSuccessAsync(result.Message, result.Description, ok);
            else
                await ShowErrorAsync(result.Message, result.Description, ok);
        }

        public async Task<bool> ShowAsync(string title, string content, string ok = "Tamam", string cancel = null)
        {
            var dialog = CreateDialog(title, content, CustomContentDialog.DialogIcon.Info);
            dialog.PrimaryButtonText = ok;

            if (!string.IsNullOrEmpty(cancel))
            {
                dialog.SecondaryButtonText = cancel;
            }

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        // YENİ MODERN METOTLAR
        public async Task ShowSuccessAsync(string title, string content, string ok = "Tamam")
        {
            var dialog = CreateDialog(title, content, CustomContentDialog.DialogIcon.Success);
            dialog.PrimaryButtonText = ok;
            await dialog.ShowAsync();
        }

        public async Task ShowErrorAsync(string title, string content, string ok = "Tamam")
        {
            var dialog = CreateDialog(title, content, CustomContentDialog.DialogIcon.Error);
            dialog.PrimaryButtonText = ok;
            await dialog.ShowAsync();
        }

        public async Task ShowWarningAsync(string title, string content, string ok = "Tamam")
        {
            var dialog = CreateDialog(title, content, CustomContentDialog.DialogIcon.Warning);
            dialog.PrimaryButtonText = ok;
            await dialog.ShowAsync();
        }

        public async Task ShowInfoAsync(string title, string content, string ok = "Tamam")
        {
            var dialog = CreateDialog(title, content, CustomContentDialog.DialogIcon.Info);
            dialog.PrimaryButtonText = ok;
            await dialog.ShowAsync();
        }

        public async Task<bool> ShowConfirmationAsync(string title, string content, string confirmText = "Evet", string cancelText = "Hayır")
        {
            var dialog = CreateDialog(title, content, CustomContentDialog.DialogIcon.Question);
            dialog.PrimaryButtonText = confirmText;
            dialog.SecondaryButtonText = cancelText;

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        public async Task<string> ShowInputAsync(string title, string placeholder, string defaultValue = "")
        {
            var stackPanel = new StackPanel { Spacing = 8 };

            if (!string.IsNullOrEmpty(placeholder))
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = placeholder,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                });
            }

            var textBox = new TextBox
            {
                Text = defaultValue,
                Height = 40,
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(textBox);

            var dialog = CreateDialog(title, stackPanel, CustomContentDialog.DialogIcon.Edit);
            dialog.PrimaryButtonText = "Tamam";
            dialog.SecondaryButtonText = "İptal";

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary ? textBox.Text : string.Empty;
        }
    }
}