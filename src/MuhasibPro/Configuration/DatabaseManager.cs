using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Core.Infrastructure.Common;
using MuhasibPro.Core.Services.Abstract.Common;
using MuhasibPro.Services.Common;
using System.Diagnostics;
using Windows.Storage;

namespace MuhasibPro.Configuration
{
    public class DatabaseManager
    {
        #region Singleton Implementation

        private static readonly Lazy<DatabaseManager> _instance = new Lazy<DatabaseManager>(() => new DatabaseManager());
        public static DatabaseManager Instance => _instance.Value;
        
        private DatabaseManager() { }

        #endregion

        #region Properties

        public string DatabasePath => Path.Combine(
            ApplicationData.Current.LocalFolder.Path,
            AppMessage.DatabaseName.DbFolder,
            AppMessage.DatabaseName.SistemDbName);

        public string ConnectionString => $"Data Source={DatabasePath};";

        public bool IsDatabaseInitialized { get; private set; }
        public bool IsDatabaseConnected { get; private set; }

        #endregion

        #region Public Methods

        public async Task<bool> InitializeDatabaseAsync()
        {
            try
            {             

                // 1. Database dosyasını oluştur/kontrol et
                await EnsureDatabaseFileAsync();

                // 2. Bağlantıyı test et
                IsDatabaseConnected = await TestConnectionAsync();

                if (!IsDatabaseConnected)
                {                   
                    return false;
                }
                // 3. Migration'ları çalıştır
                await RunMigrationsAsync();

                // 4. Database yapısını doğrula
                bool isValid = await VerifyDatabaseStructureAsync();

                if (!isValid)
                {
                    
                    return false;
                }

                IsDatabaseInitialized = true;
                
                return true;
            }
            catch (Exception ex)
            {
                
                Debug.WriteLine($"DatabaseManager InitializeDatabaseAsync error: {ex}");
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (!File.Exists(DatabasePath))
                {
                    Debug.WriteLine("Database file does not exist");
                    return false;
                }

                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT 1";
                    var result = await command.ExecuteScalarAsync();

                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TestConnectionAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(DatabasePath))
                    return false;

                var databaseFile = await StorageFile.GetFileFromPathAsync(DatabasePath);
                var backupFile = await StorageFile.GetFileFromPathAsync(backupPath);

                await databaseFile.CopyAndReplaceAsync(backupFile);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BackupDatabaseAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                var backupFile = await StorageFile.GetFileFromPathAsync(backupPath);
                var databaseFile = await StorageFile.GetFileFromPathAsync(DatabasePath);

                await backupFile.CopyAndReplaceAsync(databaseFile);

                // Restore sonrası connection durumunu güncelle
                IsDatabaseConnected = await TestConnectionAsync();

                return IsDatabaseConnected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RestoreDatabaseAsync error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Private Methods

        private async Task EnsureDatabaseFileAsync()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var sistemDbFolder = await localFolder.CreateFolderAsync(
                    AppMessage.DatabaseName.DbFolder,
                    CreationCollisionOption.OpenIfExists);

                var dbFile = await sistemDbFolder.TryGetItemAsync(AppMessage.DatabaseName.SistemDbName);

                if (dbFile == null)
                {
                   

                    var sourceSistemDbFile = await StorageFile.GetFileFromApplicationUriAsync(
                        new Uri("ms-appx:///Databases/Sistem.db"));

                    var targetSistemDbFile = await sistemDbFolder.CreateFileAsync(
                        AppMessage.DatabaseName.SistemDbName,
                        CreationCollisionOption.ReplaceExisting);

                    await sourceSistemDbFile.CopyAndReplaceAsync(targetSistemDbFile);
                   
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnsureDatabaseFileAsync error: {ex.Message}");
                throw;
            }
        }

        private async Task RunMigrationsAsync()
        {
            try            {
               
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    var optionsBuilder = new DbContextOptionsBuilder<AppSistemDbContext>();
                    optionsBuilder.UseSqlite(connection);

                    using (var context = new AppSistemDbContext(optionsBuilder.Options))
                    {
                        await context.Database.MigrateAsync();
                    }
                }               
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RunMigrationsAsync error: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> VerifyDatabaseStructureAsync()
        {
            try
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT COUNT(*) FROM sqlite_master 
                        WHERE type='table' AND name IN ('Kullanicilar', 'Firmalar')";

                    var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync());

                    return tableCount >= 2; // En az 2 tablo bekleniyor
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VerifyDatabaseStructureAsync error: {ex.Message}");
                return false;
            }
        }

        private async Task<long> GetDatabaseSizeAsync()
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(DatabasePath);
                var properties = await file.GetBasicPropertiesAsync();
                return (long)properties.Size;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Utility Methods

        public async Task<string> GetDatabaseInfoAsync()
        {
            try
            {
                var size = await GetDatabaseSizeAsync();
                var sizeInMB = size / (1024f * 1024f);

                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table'";
                    var tableCount = await command.ExecuteScalarAsync();

                    return $"Boyut: {sizeInMB:F2} MB, Tablo Sayısı: {tableCount}";
                }
            }
            catch
            {
                return "Bilgi alınamadı";
            }
        }

        public async Task<bool> ExecuteQueryAsync(string query)
        {
            try
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    await command.ExecuteNonQueryAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteQueryAsync error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Event Handlers

        public event EventHandler<bool> DatabaseConnectionChanged;

        private void OnDatabaseConnectionChanged(bool isConnected)
        {
            IsDatabaseConnected = isConnected;
            DatabaseConnectionChanged?.Invoke(this, isConnected);

            // StatusBar güncelleme
            var statusBar = StatusBarManager.Instance;
            statusBar.SetDatabaseStatus(isConnected);
        }

        #endregion
    }
}

