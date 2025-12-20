using Microsoft.UI.Xaml.Navigation;
using Muhasib.Business.Services.Contracts.BaseServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.Login;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.Views.Login;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Firmalar
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirmaWithMaliDonemSelectView : Page
    {
        public FirmaWithMaliDonemSelectViewModel ViewModel { get; }

        public FirmaWithMaliDonemSelectView()
        {
            ViewModel = ServiceLocator.Current.GetService<FirmaWithMaliDonemSelectViewModel>();
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var args = e.Parameter as ShellArgs ?? new ShellArgs();
            await ViewModel.LoadAsync(args);

            ViewModel.BaseSubscribe();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ViewModel.BaseUnsubscribe();
            ViewModel.Unload();
        }
        private async void Logout()
        {
            var dialogService = ViewModel.DialogService;
            ContentDialog dialog = new ContentDialog();
            dialog.Title = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                    {
                        new SymbolIcon(Symbol.Important),
                        new TextBlock { Text = "Uyarý", Margin = new Thickness(20, 0, 0, 0) },
                    }
            };
            dialog.Content = "Çýkmak istedinize emin misiniz!";
            dialog.PrimaryButtonText = "Oturumu Kapat";
            dialog.SecondaryButtonText = "Uygulamadan Çýk";
            dialog.CloseButtonText = "Ýptal";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.RequestedTheme = App.ThemeService.Theme;
            dialog.XamlRoot = this.XamlRoot;
            var logout = await dialog.ShowAsync();
            if (logout == ContentDialogResult.Primary)
            {
                var authentication = ServiceLocator.Current.GetService<IAuthenticationService>();
                authentication.Logout();
                if (Frame.CanGoBack)
                {
                    ViewModel.ViewModelArgs.ViewModel = typeof(LoginViewModel);
                    Frame.Navigate(typeof(LoginView), ViewModel.ViewModelArgs);
                }
            }
            else if (logout == ContentDialogResult.Secondary)
            {
                ViewModel.Unload();
                Application.Current.Exit();
            }
        }

        private void CýkisYap_Button_Click(object sender, RoutedEventArgs e)
        {
            Logout();
        }
    }
}
