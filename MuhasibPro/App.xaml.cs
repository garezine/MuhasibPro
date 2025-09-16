using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.SistemDatabase;
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
            await InitializeDatabasesAsync();
           
            //MainWindow = new MainWindow();            

            MainWindow.Activate();
        }
        private async Task InitializeDatabasesAsync()
        {
            try
            {
                using var scope = _host.Services.CreateScope();
                var startup = scope.ServiceProvider.GetRequiredService<StartupSistemDatabase>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();

                logger.LogInformation("Starting application database initialization...");

                var result = await startup.InitializeSistemDatabaseAsync();

                if (!result.Success)
                {
                    logger.LogError("Database initialization failed: {Error}", result.ErrorMessage);

                    // Kullanıcıya hata mesajı göster (ContentDialog vs.)
                    throw new InvalidOperationException($"Veritabanı başlatılamadı: {result.ErrorMessage}");
                }

                // Başarılı initialization log'u
                logger.LogInformation("Database initialization successful. Action: {Action}, Schema: {Schema}",
                    result.Action, result.CurrentSchemaVersion);

                // Eğer migration uygulandıysa kullanıcıya bilgi ver
                if (result.MigrationStatus == MigrationStatus.PendingMigrations)
                {
                    logger.LogInformation("Database updated with {Count} migrations", result.PendingMigrations.Count);
                }
            }
            catch (Exception ex)
            {
                var logger = _host.Services.GetService<ILogger<App>>();
                logger?.LogError(ex, "Critical database initialization error");
                throw;
            }
        }
    }

}
