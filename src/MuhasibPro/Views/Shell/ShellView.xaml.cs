using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Core.Infrastructure.Helpers;
using MuhasibPro.Core.Services.Common;
using MuhasibPro.Services.Common;
using MuhasibPro.ViewModels.ViewModel.Shell;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Shell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellView : Page
    {
        public ShellView()
        {
            ViewModel = Ioc.Default.GetService<ShellViewModel>();
            this.InitializeComponent();
            InitializeNavigation();
            InitializeContext();
        }
        public ShellViewModel ViewModel { get; private set; }
     
        private void InitializeNavigation()
        {
            var navigationService = Ioc.Default.GetService<INavigationService>();
            navigationService.Initialize(frame);
            var window = WindowHelper.CurrentWindow;
            window.Closed += Window_Closed;
        }

        public void InitializeContext()
        {
            var context = Ioc.Default.GetService<IContextService>();
            context.InitializeWithViewType(DispatcherQueue, ViewType.Shell, this, "ShellView");
            InitializeNavigation();
        }


        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (ViewModel != null) 
            {
                ViewModel.Unsubscribe();
                ViewModel = null;
                Bindings.StopTracking();
            }

            ContextService.UnregisterViewContext(this.GetHashCode());
            WindowHelper.CurrentWindow.Activate();

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.LoadAsync(e.Parameter as ShellArgs);
            ViewModel.Subscribe();

        }
        private void OnUnlockClick(object sender, RoutedEventArgs e)
        {
            //InitializeContext();
        }
    }
}
