using CommunityToolkit.Mvvm.DependencyInjection;
using MuhasibPro.Infrastructure.Infrastructure.Helpers;
using MuhasibPro.Infrastructure.Services;
using MuhasibPro.Infrastructure.Services.Abstract.Common;

namespace MuhasibPro.Services.Common;

public class DialogService : IDialogService
{
    private static DialogService _instance;
    private static readonly object _lock = new object();

    public static DialogService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new DialogService();
                }
            }
            return _instance;
        }
    }
    public IThemeSelectorService ThemeSelector { get; }
    public DialogService() { ThemeSelector = Ioc.Default.GetService<IThemeSelectorService>(); }

    /// <summary>
    /// Genel amaçlı dialog gösterme metodu
    /// </summary>
    public async Task<ContentDialogResult> ShowDialogAsync(string title, string content,
        string primaryButtonText = "Tamam", string secondaryButtonText = null, string closeButtonText = "İptal")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title boş olamaz", nameof(title));

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText,
            DefaultButton = ContentDialogButton.None // Otomatik focus'u engelle
        };

        // Secondary button varsa ekle
        if (!string.IsNullOrWhiteSpace(secondaryButtonText))
        {
            dialog.SecondaryButtonText = secondaryButtonText;
        }

        // Close button varsa ekle
        if (!string.IsNullOrWhiteSpace(closeButtonText))
        {
            dialog.CloseButtonText = closeButtonText;
        }

        return await ShowCustomDialogAsync(dialog);
    }

    /// <summary>
    /// Özel dialog gösterme metodu
    /// </summary>
    public async Task<ContentDialogResult> ShowCustomDialogAsync(ContentDialog dialog)
    {
        if (dialog == null)
            throw new ArgumentNullException(nameof(dialog));

        try
        {
            // XamlRoot'u kontrol et ve ata
            var xamlRoot = WindowHelper.CurrentXamlRoot;
            if (xamlRoot == null)
            {
                throw new InvalidOperationException("Aktif pencere bulunamadı. Dialog gösterilemez.");
            }

            dialog.RequestedTheme = ThemeSelector.Theme;
            dialog.XamlRoot = xamlRoot;
            // Dialog'u ana thread'de çalıştır
            return await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dialog gösterim hatası: {ex.Message}");

            // Hata durumunda kullanıcıya bilgi ver
            if (ex.Message.Contains("Only one ContentDialog can be open at a time"))
            {
                throw new InvalidOperationException("Zaten açık bir dialog var. Lütfen önce mevcut dialog'u kapatın.", ex);
            }

            throw new InvalidOperationException("Dialog gösterilemedi.", ex);
        }
    }

    /// <summary>
    /// Bilgi mesajı göster
    /// </summary>
    public async Task ShowMessageAsync(string title, string message)
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Bilgi";

        await ShowDialogAsync(title, message, "Tamam", null, null);
    }

    /// <summary>
    /// Hata mesajı göster
    /// </summary>
    public async Task ShowErrorAsync(string title, string message)
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Hata";

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Tamam",
            DefaultButton = ContentDialogButton.Primary
        };

        await ShowCustomDialogAsync(dialog);
    }

    /// <summary>
    /// Uyarı mesajı göster
    /// </summary>
    public async Task ShowWarningAsync(string title, string message)
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Uyarı";

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Tamam",
            DefaultButton = ContentDialogButton.Primary
        };

        await ShowCustomDialogAsync(dialog);
    }

    /// <summary>
    /// Onay dialog'u göster - GÜVENLİ VERSİYON
    /// </summary>
    public async Task<bool> ShowConfirmationAsync(string title, string message,
        string yesText = "Evet", string noText = "Hayır")
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Onay";

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = yesText,        // "Evet" - Primary
            SecondaryButtonText = noText,       // "Hayır" - Secondary
            DefaultButton = ContentDialogButton.Secondary, // Varsayılan "Hayır" seçili
            CloseButtonText = null              // Close butonunu kaldır
        };

        var result = await ShowCustomDialogAsync(dialog);
        return result == ContentDialogResult.Primary; // Sadece "Evet" basılırsa true
    }

    /// <summary>
    /// Tehlikeli işlemler için güvenli onay dialog'u
    /// </summary>
    public async Task<bool> ShowDangerousConfirmationAsync(string title, string message,
        string confirmText = "Sil", string cancelText = "İptal")
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Tehlikeli İşlem";

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = confirmText,
            CloseButtonText = cancelText,
            DefaultButton = ContentDialogButton.Close, // Varsayılan "İptal" seçili
            SecondaryButtonText = null
        };

        var result = await ShowCustomDialogAsync(dialog);
        return result == ContentDialogResult.Primary;
    }

    /// <summary>
    /// Üç seçenekli onay dialog'u
    /// </summary>
    public async Task<ContentDialogResult> ShowThreeOptionDialogAsync(string title, string message,
        string primaryText = "Evet", string secondaryText = "Hayır", string cancelText = "İptal")
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Seçim";

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryText,
            SecondaryButtonText = secondaryText,
            CloseButtonText = cancelText,
            DefaultButton = ContentDialogButton.Close // Varsayılan "İptal" seçili
        };

        return await ShowCustomDialogAsync(dialog);
    }

    /// <summary>
    /// Metin girişi dialog'u
    /// </summary>
    public async Task<string> ShowInputDialogAsync(string title, string placeholder = "", string defaultText = "")
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Giriş";

        var textBox = new TextBox
        {
            PlaceholderText = placeholder,
            Text = defaultText,
            AcceptsReturn = false,
            Margin = new Thickness(0, 10, 0, 0),
            MinWidth = 300
        };

        var dialog = new ContentDialog
        {
            Title = title,
            Content = textBox,
            PrimaryButtonText = "Tamam",
            CloseButtonText = "İptal",
            DefaultButton = ContentDialogButton.None // Otomatik focus'u engelle
        };

        // TextBox'a focus ver ve metni seç
        dialog.Opened += (s, e) =>
        {
            textBox.Focus(FocusState.Programmatic);
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.SelectAll();
            }
        };

        var result = await ShowCustomDialogAsync(dialog);
        return result == ContentDialogResult.Primary ? textBox.Text?.Trim() : null;
    }

    /// <summary>
    /// Çok satırlı metin girişi dialog'u
    /// </summary>
    public async Task<string> ShowMultilineInputDialogAsync(string title, string placeholder = "", string defaultText = "")
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Metin Girişi";

        var textBox = new TextBox
        {
            PlaceholderText = placeholder,
            Text = defaultText,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 150,
            MinWidth = 400,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var dialog = new ContentDialog
        {
            Title = title,
            Content = textBox,
            PrimaryButtonText = "Tamam",
            CloseButtonText = "İptal",
            DefaultButton = ContentDialogButton.None
        };

        dialog.Opened += (s, e) =>
        {
            textBox.Focus(FocusState.Programmatic);
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.SelectAll();
            }
        };

        var result = await ShowCustomDialogAsync(dialog);
        return result == ContentDialogResult.Primary ? textBox.Text?.Trim() : null;
    }

    /// <summary>
    /// Sayı girişi dialog'u
    /// </summary>
    public async Task<double?> ShowNumberInputDialogAsync(string title, string placeholder = "", double? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            title = "Sayı Girişi";

        var numberBox = new NumberBox
        {
            PlaceholderText = placeholder,
            Value = defaultValue ?? double.NaN,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            MinWidth = 200,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var dialog = new ContentDialog
        {
            Title = title,
            Content = numberBox,
            PrimaryButtonText = "Tamam",
            CloseButtonText = "İptal",
            DefaultButton = ContentDialogButton.None
        };

        dialog.Opened += (s, e) => numberBox.Focus(FocusState.Programmatic);

        var result = await ShowCustomDialogAsync(dialog);

        if (result == ContentDialogResult.Primary)
        {
            return double.IsNaN(numberBox.Value) ? null : numberBox.Value;
        }

        return null;
    }

    /// <summary>
    /// Loading dialog'u göster
    /// </summary>
    public async Task<ContentDialog> ShowLoadingDialogAsync(string title = "Yükleniyor...", string message = "Lütfen bekleyiniz...")
    {
        var progressRing = new ProgressRing
        {
            IsActive = true,
            Width = 40,
            Height = 40,
            Margin = new Thickness(0, 10, 0, 10)
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { progressRing, new TextBlock { Text = message, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center } }
        };

        var dialog = new ContentDialog
        {
            Title = title,
            Content = stackPanel,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            DefaultButton = ContentDialogButton.None
        };

        // Dialog'u göster ama sonucunu bekleme
        _ = Task.Run(async () =>
        {
            try
            {
                dialog.XamlRoot = WindowHelper.CurrentXamlRoot;
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loading dialog hatası: {ex.Message}");
            }
        });
        await Task.CompletedTask;
        return dialog;
    }

    /// <summary>
    /// Dialog'un açık olup olmadığını kontrol et
    /// </summary>
    public bool IsDialogOpen()
    {
        // Bu WinUI 3'te doğrudan kontrol edilemez
        // Gerekirse static bir flag kullanabilirsiniz
        return false;
    }
}

// Extension methods
public static class DialogServiceExtensions
{
    public static IDialogService GetDialogService(this Application app)
    {
        return DialogService.Instance;
    }

    public static async Task<bool> ShowDeleteConfirmationAsync(this IDialogService dialogService, string itemName)
    {
        return await DialogService.Instance.ShowDangerousConfirmationAsync(
            "Silme Onayı",
            $"'{itemName}' silinsin mi? Bu işlem geri alınamaz.",
            "Sil",
            "İptal"
        );
    }

    public static async Task<bool> ShowSaveConfirmationAsync(this IDialogService dialogService, string fileName)
    {
        return await DialogService.Instance.ShowConfirmationAsync(
            "Kaydetme Onayı",
            $"'{fileName}' kaydedilsin mi?",
            "Kaydet",
            "İptal"
        );
    }
}
