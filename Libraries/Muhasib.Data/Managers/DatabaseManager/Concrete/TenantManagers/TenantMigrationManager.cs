using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantManagers
{
    public class TenantMigrationManager : ITenantMigrationManager
    {
        private readonly IAppDbContextFactory _dbContextFactory;
        private readonly ILogger<TenantMigrationManager> _logger;
        private readonly IAppSqlDatabaseManager _appSqlDatabaseManager;
        private readonly IMaliDonemRepository _maliDonemRepo;
        public TenantMigrationManager(
            IAppDbContextFactory dbContextFactory,
            ILogger<TenantMigrationManager> logger,
            IAppSqlDatabaseManager appSqlDatabaseManager,
            IMaliDonemRepository maliDonemRepo)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _appSqlDatabaseManager = appSqlDatabaseManager;
            _maliDonemRepo = maliDonemRepo;
        }

        public async Task<bool> PrepareDatabaseAsync(long maliDonemId)
        {
            try
            {
                // Database ismini al
                var maliDonemDb = await _maliDonemRepo.GetByMaliDonemIdAsync(maliDonemId);
                if (maliDonemDb == null) return false;

                // AppSqlDatabaseManager'ı kullanarak DB'yi hazırla
                return await _appSqlDatabaseManager.InitializeDatabaseAsync(maliDonemDb.DBName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrepareDatabaseAsync hatası: {MaliDonemId}", maliDonemId);
                return false;
            }
        }

        public async Task<List<string>> GetPendingMigrationsAsync(long maliDonemId)
        {
            try
            {
                using var dbContext = await _dbContextFactory.CreateForTenantAsync(maliDonemId);
                var bekleyenMigrationlar = await dbContext.Database.GetPendingMigrationsAsync();
                return bekleyenMigrationlar.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bekleyen migration'lar getirilemedi: {MaliDonemId}", maliDonemId);
                return new List<string>();
            }
        }

        public async Task<bool> RunMigrationsAsync(long maliDonemId)
        {
            try
            {
                using var dbContext = await _dbContextFactory.CreateForTenantAsync(maliDonemId);
                await dbContext.Database.MigrateAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration çalıştırma hatası: {MaliDonemId}", maliDonemId);
                return false;
            }
        }

        public async Task<bool> DatabaseExistsAsync(string databaseName)
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

        public async Task<bool> TableExistsAsync(long maliDonemId, string tableName)
        {
            try
            {
                using var dbContext = await _dbContextFactory.CreateForTenantAsync(maliDonemId);
                var sql = $"SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}') THEN 1 ELSE 0 END";
                var result = await dbContext.Database.ExecuteSqlRawAsync(sql);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

