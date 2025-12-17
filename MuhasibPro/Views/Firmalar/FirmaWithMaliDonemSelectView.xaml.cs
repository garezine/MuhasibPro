using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.Shell;


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

            ViewModel.Subscribe();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ViewModel.Unsubscribe();
            ViewModel.Unload();
        }
    }
}
