using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.HostBuilders;
using MuhasibPro.Contracts.BaseAppServices;
using MuhasibPro.Contracts.SistemServices.DatabaseServices;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using System.Diagnostics;
using Velopack;
using WinUIEx;

namespace MuhasibPro
{
    public partial class App : Application
    {
        private readonly IHost _host;
        public App()
        {
            this.InitializeComponent();
            var culture = new System.Globalization.CultureInfo("tr-TR");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            _host = CreateHostBuilder().Build();
            Ioc.Default.ConfigureServices(_host.Services);

            VelopackApp.Build()
                .OnFirstRun(v =>
                {
                    // Show welcome message or perform first-run actions
                })
                .OnRestarted(v =>
                {

                })
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args = null)
        {
            return Host.CreateDefaultBuilder(args)
                .UseContentRoot(AppContext.BaseDirectory)
                .AddConfiguration()
                .AddDatabaseManager()
                .AddRepositories()
                .AddSystemLog()
                .AddBaseServices()
                .AddServices()
                .AddAppViewModel()
                .AddAppView();

        }


        public static UIElement? AppTitlebar { get; set; }
        public static WindowEx? MainWindow { get; } = new MainWindow();
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                await InitializeSystemDatabaseAsync();
                var sistemService = Ioc.Default.GetRequiredService<ISistemDatabaseService>();
                var systemStatus = await sistemService.GetSystemStatusAsync();
                Debug.WriteLine($"System Status: {systemStatus}");
                var themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
                if (MainWindow.Content is FrameworkElement rootelement)
                {
                    rootelement.RequestedTheme = themeSelectorService.Theme;
                }
                await ActivateAsync(args);
            }
            catch (Exception ex)
            {
                ShowStartupError(ex);
                Exit();
            }
        }
        private async Task InitializeSystemDatabaseAsync()
        {
            var systemService = _host.Services.GetRequiredService<ISistemDatabaseService>();
            var success = await systemService.ApplyDatabaseUpdatesAsync();
            
            if (!success) throw new Exception("Sistem DB başlatılamadı!");
        }
        private async Task ActivateAsync(LaunchActivatedEventArgs args)
        {
            WindowHelper.SetMainWindow(MainWindow);

            var activationService = Ioc.Default.GetService<IActivationService>();
            await activationService.ActivateAsync(args);
        }
        private void ShowStartupError(Exception ex)
        {
            // ContentDialog ile hata göster
            var errorMessage = $"Uygulama başlatılamadı:\n{ex.Message}";
            // UI thread'de göster
            System.Diagnostics.Debug.WriteLine(errorMessage);
        }
    }

}
