
using CommunityToolkit.Mvvm.DependencyInjection;
using Muhasebe.Business.Common;
using MuhasibPro.Infrastructure.Services;
using MuhasibPro.Infrastructure.Services.Abstract.Common;
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
    private async void StartSplashScreenAsync()
    {
        var frame = App.MainWindow.Content as Frame;

        if (frame != null)
        {
            IMessageService messageService = Ioc.Default.GetService<IMessageService>();

            // 1. Önce ExtendedSplash'i oluştur
            _extendedSplash = new ExtendedSplash();
            _extendedSplash.RequestedTheme = _themeSelectorService.Theme;

            // 2. Window'a set et
            App.MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.Content = _extendedSplash;
            App.MainWindow.Activate();

            // 3. UI'ın yüklenmesini bekle
            await Task.Delay(100); // UI'ın render olması için kısa bekle

            ExtendedSplash.StatusMessages.Enqueue("Uygulama başlatılıyor...");
        }
    }
}
