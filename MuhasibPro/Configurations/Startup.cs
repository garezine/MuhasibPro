using Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.Infrastructure.CommonServices;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.Login;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Loggings.SistemLogs;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;
using MuhasibPro.Views.Dashboard;
using MuhasibPro.Views.Firma;
using MuhasibPro.Views.Firmalar;
using MuhasibPro.Views.Loggings;
using MuhasibPro.Views.Login;
using MuhasibPro.Views.MaliDonem;
using MuhasibPro.Views.Settings;
using MuhasibPro.Views.Shell;

namespace MuhasibPro.Configurations
{
    public class Startup
    {
        private static readonly Lazy<Startup> _instance = new Lazy<Startup>(() => new Startup());
        public static Startup Instance => _instance.Value;

        private Startup()
        {
        }

        public async Task ConfigureAsync()
        {
            ConfigureNavigation();
            await SistemDatabaseConnectionTest();

            await Task.CompletedTask;
        }

        private void ConfigureNavigation()
        {
            NavigationService.Register<ShellViewModel, ShellView>();
            NavigationService.Register<MainShellViewModel, MainShellView>();
            NavigationService.Register<LoginViewModel, LoginView>();

            NavigationService.Register<DashboardViewModel, DashboardView>();
            NavigationService.Register<SettingsViewModel, SettingsView>();
            NavigationService.Register<UpdateViewModel, UpdateView>();
            NavigationService.Register<SistemLogsViewModel, SistemLogsView>();

            NavigationService.Register<FirmaDetailsViewModel, FirmaView>();
            NavigationService.Register<FirmalarViewModel, FirmalarView>();
            
            NavigationService.Register<MaliDonemDetailsViewModel, MaliDonemView>();
        }

        
        public async Task<bool> SistemDatabaseConnectionTest()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("1. ServiceLocator çağrılıyor...");

                var sistemDbService = ServiceLocator.Current.GetService<ISistemDatabaseService>();
                var statusBarService = ServiceLocator.Current.GetService<IStatusBarService>();

                System.Diagnostics.Debug.WriteLine($"2. sistemDbService: {sistemDbService != null}");
                System.Diagnostics.Debug.WriteLine($"3. statusBarService: {statusBarService != null}");

                if (sistemDbService == null)
                    return false;

                System.Diagnostics.Debug.WriteLine("4. IsSystemHealthyAsync çağrılıyor...");
                var connection = await sistemDbService.IsSystemHealthyAsync();

                System.Diagnostics.Debug.WriteLine($"5. Connection sonucu: {connection}");

                if (connection && statusBarService != null)
                {
                    System.Diagnostics.Debug.WriteLine("6. Status bar güncelleniyor...");
                    statusBarService.SetDatabaseStatus(connection, "Sistem Db");
                }

                System.Diagnostics.Debug.WriteLine("7. Test tamamlandı");
                return connection;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HATA: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
