using CommunityToolkit.Mvvm.DependencyInjection;
using MuhasibPro.Contracts.SistemServices.DatabaseServices;
using MuhasibPro.Helpers;
using MuhasibPro.Services.CommonServices;
using MuhasibPro.ViewModels.Dashboard;
using MuhasibPro.ViewModels.Firmalar;
using MuhasibPro.ViewModels.Login;
using MuhasibPro.ViewModels.Settings;
using MuhasibPro.ViewModels.Shell;
using MuhasibPro.Views.Dashboard;
using MuhasibPro.Views.Firma;
using MuhasibPro.Views.Firmalar;
using MuhasibPro.Views.Login;
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

            NavigationService.Register<FirmaDetailsViewModel, FirmaView>();
            NavigationService.Register<FirmalarViewModel, FirmalarView>();
        }
        public async Task<bool> SistemDatabaseConnectionTest()
        {
            var IsSistemDbConnection = Ioc.Default.GetService<ISistemDatabaseService>();            
            var connection = await IsSistemDbConnection.IsSystemHealthyAsync();
            if (connection)
            {
                StatusBarHelpers.Instance.DatabaseConnectionMessage = "Sistem Db bağlı";
                StatusBarHelpers.Instance.IsDatabaseConnection = connection;
            }
            return connection;
        }

    }
}
