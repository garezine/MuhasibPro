using Muhasib.Data.Helper;
using MuhasibPro.Contracts.CoreServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Loggings.SistemLogs;
using MuhasibPro.Views.Loggings;

namespace MuhasibPro.Views.Settings
{
    public sealed partial class SettingsView : Page
    {
        private readonly IThemeSelectorService _themeSelectorService;

        public string Version => ProcessInfoHelper.VersionWithPrefix;

        public SettingsView()
        {
            this.InitializeComponent();
            ViewModel = ServiceLocator.Current.GetService<SettingsViewModel>();
            _themeSelectorService = ServiceLocator.Current.GetService<IThemeSelectorService>();

        }
        //TODO: settingsView gözden geçir ekle
        public SettingsViewModel ViewModel { get; }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadThemeSettings();
        }

        #region Navigation

        private void UpdateSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // UpdateView'e navigate et
            //_navigationService.CreateNewViewAsync<UpdateViewModel>(null, "Ayarlar Güncelleme");
            Frame.Navigate(typeof(UpdateView));
        }

        #endregion

        #region Header Actions

        private async void ChangeCompanyButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Firma Değiştir",
                Content = "Firma değiştirme özelliği yakında eklenecek.",
                CloseButtonText = "Tamam",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        #endregion

        #region About Actions

        private async void ViewReleaseNotes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uri = new Uri("https://github.com/garezine/MuhasibPro/releases");
                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Sürüm notları açılamadı: {ex.Message}");
            }
        }

        private async void ViewLicense_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Lisans Bilgileri",
                Content = "MuhasibPro\n© 2025 Tüm hakları saklıdır.\n\nBu yazılım ticari kullanım için lisanslanmıştır.",
                CloseButtonText = "Tamam",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        #endregion

        #region Theme Settings

        private void LoadThemeSettings()
        {
            var currentTheme = _themeSelectorService.Theme;
            switch (currentTheme)
            {
                case ElementTheme.Light:
                    themeMode.SelectedIndex = 0; break;
                case ElementTheme.Dark:
                    themeMode.SelectedIndex = 1; break;
                case ElementTheme.Default:
                    themeMode.SelectedIndex = 2; break;
            }
        }

        private async void themeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();

            if (selectedTheme != null && Enum.TryParse<ElementTheme>(selectedTheme, out var theme))
            {
                await _themeSelectorService.SetThemeAsync(theme);
            }
        }

        #endregion

        #region Helper Methods

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Hata",
                Content = message,
                CloseButtonText = "Tamam",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        #endregion

        private void SistemLogs_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SistemLogsView), new SistemLogListArgs());
        }
        private void AppLogs_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
