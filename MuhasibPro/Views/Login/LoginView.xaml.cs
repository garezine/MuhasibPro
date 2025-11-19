using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Extensions;
using MuhasibPro.Extensions.ExtensionService;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Login;
using MuhasibPro.ViewModels.ViewModels.Shell;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Login
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginView : Page
    {
        public LoginView()
        {
            ViewModel = ServiceLocator.Current.GetService<LoginViewModel>();
            InitializeContext();
            InitializeComponent();
        }

        public LoginViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentEffectMode = EffectMode.None;
            await ViewModel.LoadAsync(e.Parameter as ShellArgs);
            
            
            InitializeNavigation();
        }

        private void InitializeNavigation()
        {
            var _navigationService = ServiceLocator.Current.GetService<INavigationService>();

            if (_navigationService != null && Frame != null)
                _navigationService.Initialize(Frame);
        }

        public void InitializeContext()
        {
            var _contextService = ServiceLocator.Current.GetService<IContextService>();
            _contextService.InitializeWithContext(dispatcher: DispatcherQueue, viewElement: this);
        }

        private void OnBackgroundFocus(object sender, RoutedEventArgs e) { DoEffectIn(); }

        private void OnForegroundFocus(object sender, RoutedEventArgs e) { DoEffectOut(); }

        protected override async void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                DoEffectOut();
                await Task.Delay(100);
                ViewModel.Login();
            }
            base.OnKeyDown(e);
        }

        private EffectMode _currentEffectMode = EffectMode.None;

        private void DoEffectIn(double milliseconds = 1000)
        {
            if (_currentEffectMode == EffectMode.Foreground || _currentEffectMode == EffectMode.None)
            {
                _currentEffectMode = EffectMode.Background;
                background.Scale(milliseconds, 1.0, 1.1);
                background.Blur(milliseconds, 6.0, 0.0);
                foreground.Scale(500, 1.0, 0.95);
                foreground.Fade(milliseconds, 1.0, 0.75);
            }
        }

        private void DoEffectOut(double milliseconds = 1000)
        {
            if (_currentEffectMode == EffectMode.Background || _currentEffectMode == EffectMode.None)
            {
                _currentEffectMode = EffectMode.Foreground;
                background.Scale(milliseconds, 1.1, 1.0);
                background.Blur(milliseconds, 0.0, 6.0);
                foreground.Scale(500, 0.95, 1.0);
                foreground.Fade(milliseconds, 0.75, 1.0);
            }
        }

        public enum EffectMode
        {
            None,
            Background,
            Foreground,
            Disabled
        }
    }
}
