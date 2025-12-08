using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;

namespace Muhasib.Data.DataContext.Factories
{
    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly ITenantSQLiteConnectionStringFactory _connectionStringFactory;
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger<AppDbContextFactory> _logger;

        public AppDbContextFactory(
            ITenantSQLiteConnectionStringFactory connectionStringFactory,
            IApplicationPaths applicationPaths,
            ILogger<AppDbContextFactory> logger = null)
        {
            _connectionStringFactory = connectionStringFactory;
            _applicationPaths = applicationPaths;
            _logger = logger;
        }

        /// <summary>
        /// Database adına göre AppDbContext oluşturur
        /// </summary>
        public AppDbContext CreateContext(string databaseName)
        {
            if(string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database adı boş olamaz", nameof(databaseName));

            var options = CreateDbContextOptions(databaseName);
            return new AppDbContext(options);
        }

        /// <summary>
        /// DbContextOptions oluşturur
        /// </summary>
        public DbContextOptions<AppDbContext> CreateDbContextOptions(string databaseName)
        {
            var connectionString = _connectionStringFactory.CreateConnectionString(databaseName);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlite(
                connectionString,
                sqliteOptions =>
                {
                    sqliteOptions.CommandTimeout(30);
                });

#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
#endif

            return optionsBuilder.Options;
        }

        /// <summary>
        /// Database bağlantısını test eder (AKILLI VERSİYON)
        /// </summary>
        public async Task<bool> TestConnectionAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            // 1. ÖNCE DOSYA VAR MI KONTROL ET (Hızlı)
            var dbPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);

            if(!File.Exists(dbPath))
            {
                _logger?.LogDebug("Database file not found: {DatabaseName}", databaseName); // Warning -> Debug
                return false;
            }

            // 2. Dosya boyutu 0 mı kontrol et
            var fileInfo = new FileInfo(dbPath);
            if(fileInfo.Length == 0)
            {
                _logger?.LogWarning("Database file is empty: {DatabaseName}", databaseName);
                return false;
            }

            // 3. ÖNCE ConnectionStringFactory testi (DbContext'e gerek yok)
            // ⚠️ DÜZELTİLDİ: ! (NOT) operatörü eklendi
            if(!await _connectionStringFactory.TestConnectionStringAsync(databaseName, cancellationToken))
            {
                _logger?.LogWarning("Connection string test failed: {DatabaseName}", databaseName);
                return false;
            }

            // 4. SONRA BAĞLANTIYI TEST ET (DbContext ile)
            try
            {
                using var context = CreateContext(databaseName);
                var canConnect = await context.Database.CanConnectAsync(cancellationToken);

                if(!canConnect)
                {
                    _logger?.LogWarning("Cannot connect to database: {DatabaseName}", databaseName);
                    return false;
                }

                // ⚠️ ExecuteSqlRawAsync GEREKSİZ - CanConnectAsync yeterli
                // await context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);

                _logger?.LogDebug("Connection test successful: {DatabaseName}", databaseName);
                return true;
            } catch(Exception ex)
            {
                _logger?.LogDebug(ex, "Database connection test failed: {DatabaseName}", databaseName); // Error -> Debug
                return false;
            }
        }

        public bool TenantDatabaseFileExists(string databaseName) => _applicationPaths.TenantDatabaseFileExists(databaseName);


        public string GetTenantDatabaseFilePath(string databaseName) => _applicationPaths.GetTenantDatabaseFilePath(
            databaseName);

        /// <summary>
        /// Database dosyası var mı ve boş değil mi kontrol eder
        /// </summary>
        public bool IsDatabaseSizeValid(string databaseName) => _applicationPaths.IsTenantDatabaseSizeValid(databaseName);


        public long GetDatabaseSize(string databaseName) => _applicationPaths.GetTenantDatabaseSize(databaseName);

        
    }
}