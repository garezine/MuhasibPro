using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Data.Sqlite;
using Muhasebe.Business.Services.Abstract.App;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Domain.Utilities;
using MuhasibPro.Services.Common;
using MuhasibPro.Views.Dashboard;
using MuhasibPro.Views.Firma;
using MuhasibPro.Views.Firmalar;
using MuhasibPro.Views.Login;
using MuhasibPro.Views.Settings;
using MuhasibPro.Views.Shell;
using MuhasibPro.Views.Splash;
using MuhasibPro.Infrastructure.Infrastructure.Common;
using MuhasibPro.Infrastructure.Services.Abstract.Common;
using MuhasibPro.ViewModels.ViewModel.Dashboard;
using MuhasibPro.ViewModels.ViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModel.Login;
using MuhasibPro.ViewModels.ViewModel.Settings;
using MuhasibPro.ViewModels.ViewModel.Shell;
using Windows.Storage;


namespace MuhasibPro.Configuration
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

        public IFirmaService FirmaService = Ioc.Default.GetService<IFirmaService>();

        public async Task FirmaEkleAsync()
        {
            var navigation = Ioc.Default.GetService<INavigationService>();
            if (!await FirmaService.IsFirma())
            {
                await navigation.CreateNewViewAsync<FirmaDetailsViewModel>(
                    new FirmaDetailsArgs(),
                    customTitle: "Yeni Firma");
            }
        }
        public async Task InitializeSistemDatabaseAsync()
        {
            var databaseManager = Ioc.Default.GetService<IDatabaseRestoreService>();
            await databaseManager.CreateOrUpdateSistemDatabaseAsync();
        }
        public async Task EnsureSistemDbAsync()
        {
            var localFolder = ApplicationData.Current.SharedLocalFolder;
            if (localFolder == null) { return; }
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
                ExtendedSplash.StatusMessages.Enqueue("Veritabanına bağlanılamadı!");
                throw new Exception("Veritabanına bağlanılamadı!");
            }
        }

        public bool CheckDatabaseConnection()
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
