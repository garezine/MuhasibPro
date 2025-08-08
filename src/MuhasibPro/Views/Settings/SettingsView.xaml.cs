using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Core.Infrastructure.Helpers;
using MuhasibPro.Core.Services;
using MuhasibPro.Helpers;
using MuhasibPro.ViewModels.ViewModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        public SettingsView()
        {
            InitializeComponent();
            ThemeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
            ViewModel = Ioc.Default.GetService<SettingsViewModel>();
            ThemeSelectorService.InitializeAsync();
        }
        public string Version
        {
            get
            {
                var version = ProcessInfoHelper.GetVersion();
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public SettingsViewModel ViewModel { get; private set; }
        public IThemeSelectorService ThemeSelectorService { get; private set; }
        
   

        private async void themeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();

            if (selectedTheme != null && Enum.TryParse<ElementTheme>(selectedTheme, out var theme))
            {
                await ThemeSelectorService.SetThemeAsync(theme);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var currentTheme = ThemeSelectorService.Theme;
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
    }
}
