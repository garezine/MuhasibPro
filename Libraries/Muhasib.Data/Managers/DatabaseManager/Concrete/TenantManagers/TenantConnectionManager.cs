using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantManagers
{
    public class TenantConnectionManager : ITenantConnectionManager
    {
        private readonly IMaliDonemDbRepository _maliDonemDbRepo;
        private readonly ISqlConnectionStringFactory _connectionStringFactory;
        private readonly IAppDbContextFactory _dbContextFactory;
        private readonly ILogger<TenantConnectionManager> _logger;

        public TenantConnectionManager(
            IMaliDonemDbRepository maliDonemDbRepo,
            ISqlConnectionStringFactory connectionStringFactory,
            IAppDbContextFactory dbContextFactory,
            ILogger<TenantConnectionManager> logger)
        {
            _maliDonemDbRepo = maliDonemDbRepo;
            _connectionStringFactory = connectionStringFactory;
            _dbContextFactory = dbContextFactory;
            _logger = logger;

        }

        public async Task<string> GetConnectionStringAsync(long maliDonemId)
        {
            var maliDonemDb = await _maliDonemDbRepo.GetByIdAsync(maliDonemId);
            return _connectionStringFactory.CreateForDatabase(maliDonemDb.DBName);
        }

        public async Task<ConnectionTestResult> TestConnectionAsync(long maliDonemId)
        {
            try
            {
                // 1. Önce SQL Server'a erişimi test et
                using var masterContext = await _dbContextFactory.CreateForDatabaseAsync("master");
                if (!await masterContext.Database.CanConnectAsync())
                {
                    _logger.LogWarning("SQL Server bağlantısı başarısız");
                    return ConnectionTestResult.SqlServerUnavailable;
                }

                // 2. Sonra tenant veritabanının var olup olmadığını kontrol et
                var tenantContext = await _dbContextFactory.CreateForTenantAsync(maliDonemId);
                var databaseName = tenantContext.Database.GetDbConnection().Database;
                var dbExists = await IsTenantDatabase(databaseName);

                if (!dbExists)
                {
                    _logger.LogWarning("Tenant veritabanı mevcut değil: {DatabaseName}", databaseName);
                    return ConnectionTestResult.DatabaseNotFound;
                }

                // 3. En son tenant veritabanına bağlanmayı test et
                using var tenantDbContext = await _dbContextFactory.CreateForTenantAsync(maliDonemId);
                var canConnect = await tenantDbContext.Database.CanConnectAsync();

                _logger.LogDebug("Tenant connection test: {MaliDonemId} - {Result}", maliDonemId, canConnect);
                return canConnect ? ConnectionTestResult.Success : ConnectionTestResult.ConnectionFailed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test hatası: {MaliDonemId}", maliDonemId);
                return ConnectionTestResult.UnknownError;
            }
        }
        public async Task<(bool IsValid, string Message)> ValidateTenantAsync(long maliDonemId)
        {
            var result = await TestConnectionAsync(maliDonemId);

            return result switch
            {
                ConnectionTestResult.Success => (true, "Tenant bağlantısı başarılı"),
                ConnectionTestResult.SqlServerUnavailable => (false, "SQL Server'a erişilemiyor"),
                ConnectionTestResult.DatabaseNotFound => (false, "Tenant veritabanı bulunamadı"),
                ConnectionTestResult.ConnectionFailed => (false, "Tenant veritabanına bağlanılamıyor"),
                ConnectionTestResult.UnknownError => (false, "Bilinmeyen bağlantı hatası"),
                _ => (false, "Geçersiz bağlantı durumu")
            };
        }
        public async Task<string> GetConnectionStringInfoAsync(long maliDonemId)
        {
            try
            {
                var dbContext = await _dbContextFactory.CreateForTenantAsync(maliDonemId);
                return dbContext.Database.GetDbConnection().ConnectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bağlantı dizesi getirilemedi: {MaliDonemId}", maliDonemId);
                return string.Empty;
            }
        }
        private async Task<bool> IsTenantDatabase(string databaseName)
        {
            try
            {
                using var dbContext = await _dbContextFactory.CreateForDatabaseAsync(databaseName);
                return await dbContext.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        public event Action<TenantContext> TenantChanged;



    }
}
