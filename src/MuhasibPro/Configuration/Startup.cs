using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Muhasebe.Business.Services.Abstract.App;
using MuhasibPro.Core.Services.Abstract.Common;
using MuhasibPro.Services.Common;
using MuhasibPro.ViewModels.ViewModel.Dashboard;
using MuhasibPro.ViewModels.ViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModel.Login;
using MuhasibPro.ViewModels.ViewModel.Settings;
using MuhasibPro.ViewModels.ViewModel.Shell;
using MuhasibPro.Views.Dashboard;
using MuhasibPro.Views.Firma;
using MuhasibPro.Views.Firmalar;
using MuhasibPro.Views.Login;
using MuhasibPro.Views.Settings;
using MuhasibPro.Views.Shell;
using MuhasibPro.Views.Splash;


namespace MuhasibPro.Configuration
{
    public class Startup
    {
        private static readonly Lazy<Startup> _instance =  new Lazy<Startup>(() => new Startup());
        public static Startup Instance => _instance.Value;
        private Startup() { }
        public async Task ConfigureAsync()
        {
            ConfigureNavigation();
            await EnsureSistemDbAsync();            
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
                await navigation.CreateNewViewAsync<FirmaDetailsViewModel>(new FirmaDetailsArgs(), customTitle: "Yeni Firma");
            }
        }

        public async Task EnsureSistemDbAsync()
        {
            
                // Database initialization
                bool success = await DatabaseManager.Instance.InitializeDatabaseAsync();
                if (success)
                {
                    ExtendedSplash.StatusMessages.Enqueue("Veritabanı güncelleniyor");
                }
            
            
        }
        //public static async Task EnsureSistemDbAsync()
        //{
        //    var localFolder = ApplicationData.Current.LocalFolder;
        //    var sistemDbFolder = await localFolder.CreateFolderAsync(
        //        AppMessage.DatabaseName.DbFolder,
        //        CreationCollisionOption.OpenIfExists);
        //    if (await sistemDbFolder.TryGetItemAsync(AppMessage.DatabaseName.SistemDbName) == null)
        //    {
        //        var sourceSistemDbFile = await StorageFile.GetFileFromApplicationUriAsync(
        //            new Uri("ms-appx:///Databases/Sistem.db"));
        //        var targetSistemDbFile = await sistemDbFolder.CreateFileAsync(
        //            AppMessage.DatabaseName.SistemDbName,
        //            CreationCollisionOption.ReplaceExisting);
        //        await sourceSistemDbFile.CopyAndReplaceAsync(targetSistemDbFile);
        //    }
        //    // Veritabanı bağlantısını kontrol et
        //    bool isDbConnected = CheckDatabaseConnection();

        //    if (!isDbConnected)
        //    {
        //        StatusMessage.Message.Add("Veritabanına bağlanılamadı!");
        //        throw new Exception("Veritabanına bağlanılamadı!");
        //    }
        //}

        //public static bool CheckDatabaseConnection()
        //{
        //    var dbPath = Path.Combine(
        //        ApplicationData.Current.LocalFolder.Path,
        //        AppMessage.DatabaseName.DbFolder,
        //        AppMessage.DatabaseName.SistemDbName);
        //    var statusBar = StatusBarManager.Instance;

        //    try
        //    {
        //        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        //        {
        //            connection.Open();
        //            var databaseManager = Ioc.Default.GetService<IDatabaseRestoreService>();
        //            databaseManager.RestoreUpdateSistemDatabase();
        //            var command = connection.CreateCommand();
        //            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' LIMIT 1";
        //            command.ExecuteScalar();
                    
        //            statusBar.SetDatabaseStatus(true);
        //            return true;
        //        }
        //    }
        //    catch
        //    {
        //        statusBar.SetDatabaseStatus(false);
        //        return false;
        //    }
        //}

    }
}
