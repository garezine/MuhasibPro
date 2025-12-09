using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.Globalization;
using Muhasib.Business.HostBuilders;
using Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using MuhasibPro.Contracts.CoreServices;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using System.Diagnostics;
using Velopack;

namespace MuhasibPro;
public partial class App : Application
{
    private readonly IHost _host;
   
    private DispatcherQueue _dispatcherQueue;
    public App()
    {
        this.InitializeComponent();
        // Kültür ayarı
        var culture = new System.Globalization.CultureInfo("tr-TR");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        ApplicationLanguages.PrimaryLanguageOverride = "tr-TR";
        // Host builder
        _host = CreateHostBuilder().Build();
        ServiceLocator.Configure(_host.Services);
        // Unhandled exception handler - UYGULAMAYI KAPATMAYI ENGELLE
        this.UnhandledException += OnUnhandledException;
        // Velopack
        
        VelopackApp.Build()
            .OnFirstRun(
                v =>
                {
                    // Show welcome message or perform first-run actions
                })
            .OnRestarted(
                v =>
                {
                    // Restart actions
                })
            .Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args = null)
    {
        return Host.CreateDefaultBuilder(args)
            .UseContentRoot(AppContext.BaseDirectory)
            .AddConfiguration()
            .AddDatabaseManagement()
            .AddRepositories()
            .AddCommonServices()
            .AddBusinessServices()
            .AddAppViewModel()
            .AddAppView();
            
    }
    public static UIElement? AppTitleBar { get; set; }
    public static Window? MainWindow { get; } = new MainWindow();
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            // DispatcherQueue'yu al
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            // 1. ÖNCE ARAYÜZÜ HEMEN GÖSTER
            await ActivateAsync(args);          
        }
        catch (Exception)
        {            
            
            await ActivateAsync(args);
        }
    }    
    
    public ILogService LogService => _host.Services.GetRequiredService<ILogService>();    
    
    private async Task SetThemeAsync()
    {
        try
        {
            // DispatcherQueue ile UI thread'de çalış
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(
                    () =>
                    {
                        var themeSelectorService = ServiceLocator.Current.GetService<IThemeSelectorService>();
                        if (MainWindow?.Content is FrameworkElement rootElement && themeSelectorService != null)
                        {
                            rootElement.RequestedTheme = themeSelectorService.Theme;
                        }
                    });
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Theme setting failed: {ex.Message}");
        }
    }
    private async Task ActivateAsync(LaunchActivatedEventArgs args)
    {
        try
        {
            await SetThemeAsync();
            CustomWindowHelper.SetMainWindow(MainWindow);
            var activationService = _host.Services.GetRequiredService<IActivationService>();
            if (activationService != null)
            {
                await activationService.ActivateAsync(args);
            }
            else
            {
                // Activation service yoksa bile pencereyi göster
                MainWindow?.Activate();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Activation failed: {ex.Message}");
            // Activation başarısız olsa bile pencereyi göster
            MainWindow?.Activate();
            _dispatcherQueue.TryEnqueue(
                async () =>
                {
                    await ShowNotificationAsync(
                        "Sistem başlatma hatası",
                        "Sistem Hatası");
                });
        }
    }
    private async Task ShowNotificationAsync(string message, string title = "Bilgi")
    {
        try
        {
            // UI thread'de çalıştır
            if (_dispatcherQueue != null)
            {
                var taskCompletionSource = new TaskCompletionSource<bool>();
                _dispatcherQueue.TryEnqueue(
                    async () =>
                    {
                        try
                        {
                            if (MainWindow?.Content?.XamlRoot != null)
                            {
                                var dialog = new ContentDialog
                                {
                                    Title = title,
                                    Content = message,
                                    PrimaryButtonText = "Tamam",
                                    XamlRoot = MainWindow.Content.XamlRoot
                                };
                                await dialog.ShowAsync();
                            }
                            taskCompletionSource.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Notification failed: {ex.Message}");
                            taskCompletionSource.SetResult(false);
                        }
                    });
                await taskCompletionSource.Task;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Notification failed: {ex.Message}");
        }
    }
    
    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // KRİTİK: Exception'ı handled olarak işaretle - uygulama çökmeyecek
        e.Handled = true;
        Debug.WriteLine($"Unhandled exception (handled): {e.Exception}");
        LogService.SistemLogService.WriteAsync(Muhasib.Domain.Enum.LogType.Hata, this.ToString(), e.Message, e.Exception);
        // UI thread'de hata göster
        if (_dispatcherQueue != null)
        {
            _dispatcherQueue.TryEnqueue(
                async () =>
                {
                    await ShowNotificationAsync(
                        "Beklenmedik bir hata oluştu, ancak uygulama çalışmaya devam ediyor.",
                        "Sistem Hatası");
                });
        }
    }
}

