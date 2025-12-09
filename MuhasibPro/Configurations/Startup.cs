using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase;
using Muhasib.Domain.Enum;
using MuhasibPro.Helpers;
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
using System.Diagnostics;

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

                if(sistemDbService == null)
                    return false;

                System.Diagnostics.Debug.WriteLine("4. IsSystemHealthyAsync çağrılıyor...");
                var connection = await sistemDbService.SistemDatabaseOperation.GetHealthStatusAsync();

                System.Diagnostics.Debug.WriteLine($"5. Connection sonucu: {connection}");

               
                if(connection.Data.PendingMigrationsCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine("UYARI: Sistem veritabanı güncellemeleri bekliyor!");
                    await sistemDbService.InitializeDatabaseAsync();
                    
                }
                if (connection.Success && statusBarService != null)
                {
                    System.Diagnostics.Debug.WriteLine("6. Status bar güncelleniyor...");
                    statusBarService.SetDatabaseStatus(connection.Success, "Sistem Db");
                }

                System.Diagnostics.Debug.WriteLine("7. Test tamamlandı");
                return connection.Success;
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HATA: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        private async Task ShowNotificationAsync(string title, string message = "Bilgi")
        {
            try
            {
                var dispatcherQueue = App.MainWindow?.DispatcherQueue ??
                    CustomWindowHelper.CurrentWindow?.DispatcherQueue;
                // UI thread'de çalıştır
                if(dispatcherQueue != null)
                {
                    var taskCompletionSource = new TaskCompletionSource<bool>();
                    await dispatcherQueue.EnqueueAsync(
                        async () =>
                        {
                            var dialogService = ServiceLocator.Current.GetService<IDialogService>();
                            if (dialogService != null)
                            {
                                await dialogService.ShowAsync(message, title);
                            }
                        });
                    await taskCompletionSource.Task;
                }
            } catch(Exception ex)
            {
                Debug.WriteLine($"Notification failed: {ex.Message}");
            }
        }
    }
}
