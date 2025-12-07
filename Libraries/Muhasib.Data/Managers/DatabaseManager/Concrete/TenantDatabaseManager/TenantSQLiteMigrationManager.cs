using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantDatabaseManager
{
    public class TenantSQLiteMigrationManager : ITenantSQLiteMigrationManager
    {
        private readonly IAppDbContextFactory _appDbContextFactory;
        private readonly ITenantSQLiteBackupManager _tenantSQLiteBackupManager;
        private readonly ILogger<TenantSQLiteMigrationManager> _logger;

        public TenantSQLiteMigrationManager(
            IAppDbContextFactory appDbContextFactory,            
            ILogger<TenantSQLiteMigrationManager> logger,
            ITenantSQLiteBackupManager tenantSQLiteBackupManager)
        {
            _appDbContextFactory = appDbContextFactory;
            _logger = logger;
            _tenantSQLiteBackupManager = tenantSQLiteBackupManager;
        }

        public async Task<List<string>> GetPendingMigrationsAsync(string databaseName)
        {
            try
            {
                using var dbContext = _appDbContextFactory.CreateContext(databaseName);
                var migrationList = await dbContext.Database.GetPendingMigrationsAsync();

                _logger.LogDebug(
                    "Bekleyen migration sayısı: {Count} - {DatabaseName}",
                    migrationList.Count(),
                    databaseName);

                return migrationList.ToList();
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Bekleyen migration'lar getirilemedi: {DatabaseName}", databaseName);
                return new List<string>();
            }
        }

        public async Task<bool> InitializeDatabaseAsync(string databaseName)
        {
            try
            {
                // ✅ DOĞRU: Validation
                if(string.IsNullOrEmpty(databaseName))
                {
                    _logger.LogWarning("Veritabanı adı boş olamaz");
                    return false;
                }

                // DB var mı kontrol et
                var dbExists = _appDbContextFactory.TenantDatabaseFileExists(databaseName);

                if(!dbExists)
                {
                    _logger.LogInformation("Database yok, oluşturulacak: {DatabaseName}", databaseName);
                }


                return await RunMigrationsAsync(databaseName);
            } catch(Exception ex)
            {
                _logger.LogError(ex, "PrepareDatabaseAsync hatası: {DatabaseName}", databaseName);
                return false;
            }
        }

        public async Task<bool> RunMigrationsAsync(string databaseName)
        {
            try
            {
                using var context = _appDbContextFactory.CreateContext(databaseName);
                var canConnect = await context.Database.CanConnectAsync();
                if(!canConnect)
                {
                    _logger.LogWarning("Cannot connect to database: {DatabaseName}", databaseName);
                    return false;
                }

                var pendingMigrations = await GetPendingMigrationsAsync(databaseName);

                if(!pendingMigrations.Any())
                {
                    _logger.LogInformation("No pending migrations for {DatabaseName}", databaseName);
                    return true; // Zaten güncel
                }

                _logger.LogInformation(
                    "Found {Count} pending migrations for {DatabaseName}",
                    pendingMigrations.Count(),
                    databaseName);

                // ⚠️ Backup yavaşlatıyor - ilk oluşturmada gereksiz
                // Sadece mevcut DB'de migration yapılıyorsa yap
                var tableExists = await TableExistsAsync(context, "TenantDatabaseVersions");

                if(tableExists)
                    await _tenantSQLiteBackupManager.CreateBackupAsync(databaseName);

                // ✅ Migration timeout'u artır (varsayılan 30sn yetersiz olabilir)
                context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

                await context.Database.MigrateAsync();

                var migrationVersion = GetLatestMigrationVersion(await context.Database.GetAppliedMigrationsAsync());
                await UpdateTenantDatabaseVersionAsync(databaseName, migrationVersion);

                _logger.LogInformation("Database initialized successfully: {DatabaseName}", databaseName);
                return true;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Migration çalıştırma hatası: {databaseName}", databaseName);
                return false;
            }
        }

        private async Task<bool> TableExistsAsync(AppDbContext context, string tableName)
        {
            try
            {
                // SQL injection'dan kaçınmak için parametre kullan
                var sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name= {0}";

                var result = await context.Database.SqlQueryRaw<int>(sql, tableName).FirstOrDefaultAsync();

                return result > 0;
            } catch
            {
                return false;
            }
        }

        private string GetLatestMigrationVersion(IEnumerable<string> appliedMigrations)
        {
            try
            {
                var latest = appliedMigrations.LastOrDefault();

                if(string.IsNullOrEmpty(latest))
                    return "1.0.0"; // İlk versiyon

                // Migration formatı: 20240115143000_InitialCreate
                var parts = latest.Split('_', StringSplitOptions.RemoveEmptyEntries);

                if(parts.Length == 0)
                    return "1.0.0";

                // İlk kısmı timestamp olarak kontrol et
                var timestampPart = parts[0];

                // Timestamp formatı: YYYYMMDDHHMMSS (14 karakter)
                if(timestampPart.Length == 14 && long.TryParse(timestampPart, out _))
                {
                    return timestampPart; // "20240115143000"
                }

                // Timestamp değilse, migration adının hash'ini al
                var hash = CalculateSimpleHash(latest);
                return $"1.0.0.{hash}";
            } catch
            {
                // Herhangi bir hatada default versiyon
                return "1.0.0";
            }
        }

        private string CalculateSimpleHash(string input)
        {
            // Basit bir hash (CRC32 veya basit checksum)
            unchecked
            {
                int hash = 17;
                foreach(char c in input)
                {
                    hash = hash * 31 + c;
                }
                return Math.Abs(hash).ToString("X8").Substring(0, 6);
            }
        }

        private async Task<bool> UpdateTenantDatabaseVersionAsync(string databaseName, string newVersion)
        {
            try
            {
                using var context = _appDbContextFactory.CreateContext(databaseName);

                var currentVersion = await context.TenantDatabaseVersions
                    .Where(v => v.DatabaseName == databaseName)
                    .FirstOrDefaultAsync();

                if(currentVersion == null)
                {
                    // İlk kurulum
                    var initialVersion = new TenantDatabaseVersiyon
                    {
                        DatabaseName = databaseName,
                        TenantDbVersion = newVersion,
                        TenantDbLastUpdate = DateTime.Now,
                        PreviousTenantDbVersiyon = null
                    };
                    context.TenantDatabaseVersions.Add(initialVersion);
                } else
                {
                    // Güncelleme
                    currentVersion.PreviousTenantDbVersiyon = currentVersion.TenantDbVersion;
                    currentVersion.TenantDbVersion = newVersion;
                    currentVersion.TenantDbLastUpdate = DateTime.Now;
                }

                await context.SaveChangesAsync();
                _logger.LogInformation(
                    "Muhasebe version updated to {Version} for {DatabaseName}",
                    newVersion,
                    databaseName);
                return true;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to update muhasebe version for {DatabaseName}", databaseName);
                return false;
            }
        }
    }
}
