using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.SistemDatabase
{
    public class SistemMigrationManager : ISistemMigrationManager
    {
        private readonly SistemDbContext _dbContext;
        private readonly ILogger<SistemMigrationManager> _logger;
        private readonly ISistemBackupManager _backupManager;
        
        private const string databaseName = DatabaseConstants.SISTEM_DB_NAME;

        public SistemMigrationManager(
            SistemDbContext dbContext,
            ILogger<SistemMigrationManager> logger,
            ISistemBackupManager backupManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _backupManager = backupManager;            
        }

        public async Task<List<string>> GetPendingMigrationsAsync()
        {
            try
            {
                var migrationList = await _dbContext.Database.GetPendingMigrationsAsync();

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

        public async Task<bool> InitializeDatabaseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await RunMigrationsAsync(cancellationToken);
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Initialize database hatası: {DatabaseName}", databaseName);
                return false;
            }
        }

        public async Task<bool> RunMigrationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {                
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                if(!canConnect)
                {
                    _logger.LogWarning("Cannot connect to database: {DatabaseName}", databaseName);
                    return false;
                }

                var pendingMigrations = await GetPendingMigrationsAsync();

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
                var tableExists = await TableExistsAsync(_dbContext, "AppDbVersiyonlar");

                if(tableExists)
                    await _backupManager.CreateBackupAsync();

                // ✅ Migration timeout'u artır (varsayılan 30sn yetersiz olabilir)
                _dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

                await _dbContext.Database.MigrateAsync(cancellationToken);

                var migrationVersion = GetLatestMigrationVersion(await _dbContext.Database.GetAppliedMigrationsAsync());
                await UpdateSistemDatabaseVersionAsync(migrationVersion);

                _logger.LogInformation("Database initialized successfully: {DatabaseName}", databaseName);
                return true;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Migration çalıştırma hatası: {databaseName}", databaseName);
                return false;
            }
        }

        private async Task<bool> TableExistsAsync(SistemDbContext context, string tableName)
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

        private async Task<bool> UpdateSistemDatabaseVersionAsync(string newVersion)
        {
            try
            {
                var currentVersion = await _dbContext.AppDbVersiyonlar
                    .Where(v => v.DatabaseName == databaseName)
                    .FirstOrDefaultAsync();

                if(currentVersion == null)
                {
                    // İlk kurulum
                    var initialVersion = new AppDbVersion
                    {
                        DatabaseName = databaseName,
                        CurrentDatabaseVersion = newVersion,
                        CurrentAppVersionLastUpdate = DateTime.Now,
                        PreviousDatabaseVersion = null
                    };
                    _dbContext.AppDbVersiyonlar.Add(initialVersion);
                } else
                {
                    // Güncelleme
                    currentVersion.PreviousDatabaseVersion = currentVersion.CurrentDatabaseVersion;
                    currentVersion.CurrentDatabaseVersion = newVersion;
                    currentVersion.CurrentDatabaseLastUpdate = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation(
                    "Sistem database version updated to {Version} for {DatabaseName}",
                    newVersion,
                    databaseName);
                return true;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to update sistem database version for {DatabaseName}", databaseName);
                return false;
            }
        }
    }
}
