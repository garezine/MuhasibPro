
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Muhasebe.Business.Common;
using MuhasibPro.Core.Services;
using MuhasibPro.ViewModels.ViewModel.Dashboard;
using MuhasibPro.Views.Splash;


namespace MuhasibPro.Services;

public class ActivationInfo()
{
    public static ActivationInfo CreateDefault() => Create<DashboardViewModel>();
    public static ActivationInfo Create<TViewModel>(object entryArgs = null) where TViewModel : ObservableObject
    {
        return new ActivationInfo
        { EntryViewModel = typeof(TViewModel), EntryArgs = entryArgs };
    }
    public Type EntryViewModel { get; set; }
    public object EntryArgs { get; set; }
    
}
public class ActivationService : IActivationService
{
    private readonly IThemeSelectorService _themeSelectorService;
    private UIElement? _shell = null;
    private ExtendedSplash _extendedSplash;

    public ActivationService(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
    }
    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();
        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            App.MainWindow.Content = _shell ?? new Frame();
        }
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = _themeSelectorService.Theme;
        }
        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }



    private async Task InitializeAsync()
    {
        await _themeSelectorService.InitializeAsync();
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        await _themeSelectorService.SetRequestedThemeAsync();
        StartSplashScreenAsync();
        await Task.CompletedTask;
    }
    private void StartSplashScreenAsync()
    {
        var frame = App.MainWindow.Content as Frame;
        // Eğer içerik yoksa, ExtendedSplash ile başlat
        if (frame != null)
        {
            _extendedSplash = new ExtendedSplash();
            _extendedSplash.RequestedTheme = _themeSelectorService.Theme;
            App.MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.Content = _extendedSplash; // Önce splash ekranını göster
            App.MainWindow.Activate();
        }
    }
}
