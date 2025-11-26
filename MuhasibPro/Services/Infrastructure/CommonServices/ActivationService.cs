using MuhasibPro.Contracts.CoreServices;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;
using MuhasibPro.Views.Splash;


namespace MuhasibPro.Services.Infrastructure.CommonServices;

public class ActivationInfo()
{
    public static ActivationInfo CreateDefault() => Create<DashboardViewModel>();
    public static ActivationInfo CreateNewFirma() => Create<FirmaDetailsViewModel>();
    public static ActivationInfo Create<TViewModel>(object entryArgs = null) where TViewModel : class
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
            _extendedSplash = new ExtendedSplash();
            _extendedSplash.RequestedTheme = _themeSelectorService.Theme;
        }
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
    private async void StartSplashScreenAsync()
    {
        var frame = App.MainWindow.Content as Frame;

        if (frame != null)
        {
            // 2. Window'a set et
            App.MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.Content = _extendedSplash;
            App.MainWindow.Activate();

            // 3. UI'ın yüklenmesini bekle
            await Task.Delay(100); // UI'ın render olması için kısa bekle

        }
    }
}
