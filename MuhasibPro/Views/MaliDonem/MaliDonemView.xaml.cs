// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;

namespace MuhasibPro.Views.MaliDonem
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MaliDonemView : Page
    {
        public MaliDonemView()
        {
            InitializeComponent();
            ViewModel = ServiceLocator.Current.GetService<MaliDonemDetailsViewModel>();
            NavigationService = ServiceLocator.Current.GetService<INavigationService>();
        }
        public MaliDonemDetailsViewModel ViewModel { get; }
        public INavigationService NavigationService { get; }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Subscribe();
            await ViewModel.LoadAsync(e.Parameter as MaliDonemDetailsArgs);
            if (ViewModel.IsEditMode)
            {
                await Task.Delay(100);
                details.SetFocus();
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Unload();
            ViewModel.Unsubscribe();
        }
    }
}
