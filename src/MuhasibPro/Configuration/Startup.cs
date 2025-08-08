using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Data.Sqlite;
using Muhasebe.Business.Services.Abstract.App;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Core.Infrastructure.Common;
using MuhasibPro.Core.Services.Common;
using MuhasibPro.Services.Common;
using MuhasibPro.ViewModels.ViewModel;
using MuhasibPro.ViewModels.ViewModel.Dashboard;
using MuhasibPro.ViewModels.ViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModel.Login;
using MuhasibPro.ViewModels.ViewModel.Shell;
using MuhasibPro.Views.Dashboard;
using MuhasibPro.Views.Firma;
using MuhasibPro.Views.Firmalar;
using MuhasibPro.Views.Login;
using MuhasibPro.Views.Settings;
using MuhasibPro.Views.Shell;
using Windows.Storage;


namespace MuhasibPro.Configuration
{
    public static class Startup
    {
        public static async Task ConfigureAsync()
        {
            ConfigureNavigation();
            await EnsureSistemDbAsync();
        }

        private static void ConfigureNavigation()
        {
            NavigationService.Register<ShellViewModel, ShellView>();
            NavigationService.Register<MainShellViewModel, MainShellView>();
            NavigationService.Register<LoginViewModel, LoginView>();

            NavigationService.Register<DashboardViewModel, DashboardView>();
            NavigationService.Register<SettingsViewModel, SettingsView>();

            NavigationService.Register<FirmaDetailsViewModel, FirmaView>();
            NavigationService.Register<FirmalarViewModel, FirmalarView>();
        }

        public static IFirmaService FirmaService = Ioc.Default.GetService<IFirmaService>();

        public static async Task FirmaEkleAsync()
        {
            var navigation = Ioc.Default.GetService<INavigationService>();
            if (!await FirmaService.IsFirma())
            {
                await navigation.CreateNewViewAsync<FirmaDetailsViewModel>(new FirmaDetailsArgs(), customTitle: "Yeni Firma");
            }
        }

        public static async Task EnsureSistemDbAsync()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var sistemDbFolder = await localFolder.CreateFolderAsync(
                AppMessage.DatabaseName.DbFolder,
                CreationCollisionOption.OpenIfExists);
            if (await sistemDbFolder.TryGetItemAsync(AppMessage.DatabaseName.SistemDbName) == null)
            {
                var sourceSistemDbFile = await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///Databases/Sistem.db"));
                var targetSistemDbFile = await sistemDbFolder.CreateFileAsync(
                    AppMessage.DatabaseName.SistemDbName,
                    CreationCollisionOption.ReplaceExisting);
                await sourceSistemDbFile.CopyAndReplaceAsync(targetSistemDbFile);
            }
            // Veritabanı bağlantısını kontrol et
            bool isDbConnected = CheckDatabaseConnection();

            if (!isDbConnected)
            {
                StatusMessage.Message.Add("Veritabanına bağlanılamadı!");
                throw new Exception("Veritabanına bağlanılamadı!");
            }
        }

        public static bool CheckDatabaseConnection()
        {
            var dbPath = Path.Combine(
                ApplicationData.Current.LocalFolder.Path,
                AppMessage.DatabaseName.DbFolder,
                AppMessage.DatabaseName.SistemDbName);
            var statusBar = StatusBarManager.Instance;

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' LIMIT 1";
                    command.ExecuteScalar();
                    
                    statusBar.SetDatabaseStatus(true);
                    return true;
                }
            }
            catch
            {
                statusBar.SetDatabaseStatus(false);
                return false;
            }
        }

    }
}
