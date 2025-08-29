using System.Globalization;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.HostBuilders;
using MuhasibPro.App.Configuration;
using MuhasibPro.App.HostBuilders;
using MuhasibPro.Core.Infrastructure.Helpers;
using MuhasibPro.Core.Services;
using Velopack;
using WinUIEx;

namespace MuhasibPro.App;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        InitializeComponent();
        // Türkçe kültür ayarı
        var culture = new CultureInfo("tr-TR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        _host = CreateHostBuilder().Build();
        Ioc.Default.ConfigureServices(_host.Services);
        VelopackApp.Build().SetAutoApplyOnStartup(true)
            .OnFirstRun(v=>
            { })
            .OnRestarted(v=>
            { })
            .Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args = null)
    {
        return Host.CreateDefaultBuilder(args)
            .AddConfiguration()
            .AddDatabaseManager()
            .AddRepository()
            .AddServices()
            .AddSystemLog()
            .UseContentRoot(AppContext.BaseDirectory)
            .AddAppServices()
            .AddAppViewModel()
            .AddAppView();
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _ = Startup.Instance.InitializeSistemDatabaseAsync();
        var themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
        if (MainWindow.Content is FrameworkElement rootelement)
        {
            rootelement.RequestedTheme = themeSelectorService.Theme;
        }
        await ActivateAsync(args);
    }

    private async Task ActivateAsync(LaunchActivatedEventArgs args)
    {
        WindowHelper.SetMainWindow(MainWindow);

        var activationService = Ioc.Default.GetService<IActivationService>();
        await activationService.ActivateAsync(args);
    }


}

