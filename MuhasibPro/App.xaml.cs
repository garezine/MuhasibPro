using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.DatabaseManager.SistemDatabase;
using Muhasebe.Data.HostBuilders;
using MuhasibPro.HostBuilders;
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

            VelopackApp.Build().SetAutoApplyOnStartup(true)
                .OnFirstRun(v =>
                {
                    // Show welcome message or perform first-run actions
                })
                .OnRestarted(v =>
                {

                });
        }

        public static IHostBuilder CreateHostBuilder(string[] args = null)
        {
            return Host.CreateDefaultBuilder(args)
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
                MainWindow.Activate();
            }
            catch (Exception ex)
            {
                ShowStartupError(ex);
                Exit();
            }
        }
        private async Task InitializeSystemDatabaseAsync()
        {
            var systemService = _host.Services.GetRequiredService<ISistemDatabaseManager>();
            var success = await systemService.InitializeDatabaseAsync();

            if (!success) throw new Exception("Sistem DB başlatılamadı!");


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
