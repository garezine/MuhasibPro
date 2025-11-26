using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;
using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantManagers
{
    public class AppSqlDatabaseManager : IAppSqlDatabaseManager
    {
        private readonly ILogger<AppSqlDatabaseManager> _logger;
        private readonly IApplicationPaths _applicationPaths;
        private readonly ISqlConnectionStringFactory _connectionFactory;
        private readonly string _backupBasePath;

        public AppSqlDatabaseManager(
            ILogger<AppSqlDatabaseManager> logger,
            IApplicationPaths applicationPaths,
            ISqlConnectionStringFactory connectionFactory)
        {
            _logger = logger;
            _applicationPaths = applicationPaths;
            _connectionFactory = connectionFactory;
            _backupBasePath = Path.Combine(_applicationPaths.GetDatabasePath(), "Backups", "Muhasebe");
            Directory.CreateDirectory(_backupBasePath);
        }

        public async Task<bool> InitializeDatabaseAsync(string databaseName)
        {
            try
            {
                using var context = CreateAppDbContext(databaseName);

                _logger.LogInformation("Initializing accounting database: {databaseName}", databaseName);

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                if(pendingMigrations.Any())
                {
                    _logger.LogInformation(
                        "Found {Count} pending migrations for {databaseName}",
                        pendingMigrations.Count(),
                        databaseName);

                    // Migration öncesi backup
                    await CreateSafetyBackupAsync(databaseName);

                    try
                    {
                        // Migration'ları uygula
                        await context.Database.MigrateAsync();

                        // Migration başarılı - versiyon bilgisini güncelle
                        var migrationVersion = GetLatestMigrationVersion(
                            await context.Database.GetAppliedMigrationsAsync());
                        await UpdateMuhasebeVersionAsync(databaseName, migrationVersion);
                    } catch(Exception ex)
                    {
                        _logger.LogError(ex, "Migration failed for {databaseName}", databaseName);
                        throw;
                    }
                }

                var canConnect = await context.Database.CanConnectAsync();
                _logger.LogInformation("Accounting database initialization completed: {Success}", canConnect);

                return canConnect;
            } catch(Exception ex)
            {
                // ✅ DOĞRU - 1 placeholder, 1 parametre
                _logger.LogError(ex, "Accounting database initialization failed for {databaseName}", databaseName);
                return false;
            }
        }

        public async Task<bool> CreateNewDatabaseAsync(string databaseName)
        {
            try
            {
                var masterConnectionString = _connectionFactory.GetMasterConnectionString();

                _logger.LogInformation("Creating new accounting database: {DatabaseName}", databaseName);

                using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync();

                var createDbSql = $@"
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
                    BEGIN
                        CREATE DATABASE [{databaseName}]
                    END";

                using var command = new SqlCommand(createDbSql, connection);
                await command.ExecuteNonQueryAsync();

                // Yeni oluşturulan database'i initialize et
                return await InitializeDatabaseAsync(databaseName);
            } catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed to create accounting database for {databaseName}");
                return false;
            }
        }

        public async Task<bool> CreateManualBackupAsync(string databaseName)
        {
            try
            {
                var backupDir = Path.Combine(_backupBasePath, $"{databaseName}");
                Directory.CreateDirectory(backupDir);

                var backupFileName = $"manual_{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var backupPath = Path.Combine(backupDir, backupFileName);

                using var context = CreateAppDbContext(databaseName);

                var sql = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = N'{backupPath.Replace("'", "''")}' 
                    WITH FORMAT, INIT, COMPRESSION";

                await context.Database.ExecuteSqlRawAsync(sql);

                _logger.LogInformation("Manual accounting backup created: {BackupPath}", backupPath);
                return File.Exists(backupPath);
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Manual accounting backup failed for {FirmaKodu}_{MaliYil}", databaseName);
                return false;
            }
        }

        public async Task<DatabaseHealthInfo> GetHealthInfoAsync(string databaseName)
        {
            try
            {
                using var context = CreateAppDbContext(databaseName);

                var canConnect = await context.Database.CanConnectAsync();
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

                var backupDir = Path.Combine(_backupBasePath, $"{databaseName}");
                var backupFiles = Directory.Exists(backupDir) ? Directory.GetFiles(backupDir, "*.bak").Length : 0;

                return new DatabaseHealthInfo
                {
                    CanConnect = canConnect,
                    PendingMigrationsCount = pendingMigrations.Count(),
                    AppliedMigrationsCount = appliedMigrations.Count(),
                    BackupFilesCount = backupFiles,
                    LastBackupDate = GetLastBackupDate(databaseName)
                };
            } catch(Exception ex)
            {
                return new DatabaseHealthInfo { HasError = true, ErrorMessage = ex.Message };
            }
        }

        public Task<List<BackupFileInfo>> GetBackupHistoryAsync(string databaseName)
        {
            try
            {
                var backupDir = Path.Combine(_backupBasePath, $"{databaseName}");

                if(!Directory.Exists(backupDir))
                    return Task.FromResult(new List<BackupFileInfo>());

                var result = Directory.GetFiles(backupDir, "*.bak")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(20)
                    .Select(
                        f => new BackupFileInfo
                        {
                            FileName = f.Name,
                            FilePath = f.FullName,
                            CreatedDate = f.CreationTime,
                            SizeBytes = f.Length,
                            SizeFormatted = FormatFileSize(f.Length)
                        })
                    .ToList();
                return Task.FromResult(result);
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to get backup history for {databaseName}", databaseName);
                return Task.FromResult(new List<BackupFileInfo>());
            }
        }

        private AppDbContext CreateAppDbContext(string databaseName)
        {
            var connectionString = _connectionFactory.GetMasterConnectionString();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AppDbContext(options);
        }

        

        

        private async Task CreateSafetyBackupAsync(string databaseName)
        {
            var backupDir = Path.Combine(_backupBasePath, $"{databaseName}");
            Directory.CreateDirectory(backupDir);

            var backupFileName = $"safety_{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var backupPath = Path.Combine(backupDir, backupFileName);

            try
            {
                using var context = CreateAppDbContext(databaseName);

                var sql = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = N'{backupPath.Replace("'", "''")}' 
                    WITH FORMAT, INIT, COMPRESSION";

                await context.Database.ExecuteSqlRawAsync(sql);
                _logger.LogInformation("Safety backup created: {BackupPath}", backupPath);
            } catch(Exception ex)
            {
                _logger.LogWarning(ex, "Safety backup failed: {BackupPath}", backupPath);
            }
        }

        private DateTime? GetLastBackupDate(string databaseName)
        {
            try
            {
                var backupDir = Path.Combine(_backupBasePath, $"{databaseName}");

                if(!Directory.Exists(backupDir))
                    return null;

                var lastBackup = Directory.GetFiles(backupDir, "*.bak")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .FirstOrDefault();

                return lastBackup?.CreationTime;
            } catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteDatabaseAsync(string databaseName)
        {
            try
            {
                var masterConnectionString = _connectionFactory.GetMasterConnectionString();

                _logger.LogInformation("Deleting accounting database: {DatabaseName}", databaseName);

                using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync();

                // Önce database'i SINGLE_USER mode'a al ve aktif connection'ları kes
                var setSingleUserSql = $@"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
            BEGIN
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            END";

                using var singleUserCommand = new SqlCommand(setSingleUserSql, connection);
                await singleUserCommand.ExecuteNonQueryAsync();

                // Sonra database'i sil
                var dropDbSql = $@"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
            BEGIN
                DROP DATABASE [{databaseName}];
            END";

                using var dropCommand = new SqlCommand(dropDbSql, connection);
                await dropCommand.ExecuteNonQueryAsync();

                _logger.LogInformation("Accounting database deleted successfully: {DatabaseName}", databaseName);
                return true;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to delete accounting database for {FirmaKodu}_{MaliYil}", databaseName);
                return false;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;

            while(Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1}{suffixes[counter]}";
        }

        public async Task<MuhasebeVersiyon> GetCurrentMuhasebeVersionAsync(string databaseName)
        {
            try
            {
                using var context = CreateAppDbContext(databaseName);
                return await context.MuhasebeVersiyonlar
                    .Where(v => v.DatabaseName == databaseName)
                    .OrderByDescending(v => v.MuhasebeDBSonGuncellemeTarihi)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to get muhasebe version for {FirmaKodu}_{MaliYil}", databaseName);
                return null;
            }
        }

        public async Task<bool> UpdateMuhasebeVersionAsync(string databaseName, string newVersion)
        {
            try
            {
                using var context = CreateAppDbContext(databaseName);

                var currentVersion = await context.MuhasebeVersiyonlar
                    .Where(v => v.DatabaseName == databaseName)
                    .FirstOrDefaultAsync();

                if(currentVersion == null)
                {
                    // İlk kurulum
                    var initialVersion = new MuhasebeVersiyon
                    {
                        DatabaseName = databaseName,
                        MuhasebeDBVersiyon = newVersion,
                        MuhasebeDBSonGuncellemeTarihi = DateTime.Now,
                        OncekiMuhasebeDbVersiyon = null
                    };
                    context.MuhasebeVersiyonlar.Add(initialVersion);
                } else
                {
                    // Güncelleme
                    currentVersion.OncekiMuhasebeDbVersiyon = currentVersion.MuhasebeDBVersiyon;
                    currentVersion.MuhasebeDBVersiyon = newVersion;
                    currentVersion.MuhasebeDBSonGuncellemeTarihi = DateTime.Now;
                }

                await context.SaveChangesAsync();
                _logger.LogInformation(
                    "Muhasebe version updated to {Version} for {FirmaKodu}_{MaliYil}",
                    newVersion,
                    databaseName);
                return true;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to update muhasebe version for {FirmaKodu}_{MaliYil}", databaseName);
                return false;
            }
        }

        private string GetLatestMigrationVersion(IEnumerable<string> appliedMigrations)
        {
            var latest = appliedMigrations.LastOrDefault();
            if(latest != null && latest.Length > 8) // Migration timestamp kısmını al
            {
                return latest.Substring(0, 14); // YYYYMMDDHHMMSS formatı
            }
            return "1.0.0";
        }

        public async Task<bool> RestoreDatabaseAsync(string databaseName, string backupFilePath)
        {
            // Master/Sistem veritabanı bağlantı dizesini al.
            // RESTORE komutu Master DB üzerinden çalıştırılmalıdır.            
            var masterConnectionString = _connectionFactory.GetMasterConnectionString();

            try
            {
                // 1. Veritabanına olan tüm bağlantıları kes (SINGLE_USER moduna al)
                var disconnectSql = $@"
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                ";

                // 2. Geri Yükleme komutu
                var restoreSql = $@"
                    RESTORE DATABASE [{databaseName}] 
                    FROM DISK = '{backupFilePath}'
                    WITH REPLACE;
                ";

                // 3. Normal Kullanıcı moduna geri dön
                var multiUserSql = $@"
                    ALTER DATABASE [{databaseName}] SET MULTI_USER;
                ";

                using(var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();

                    // Bağlantıları Kesme
                    using(var command = new SqlCommand(disconnectSql, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Database {Name} set to SINGLE_USER.", databaseName);
                    }

                    // Geri Yükleme
                    using(var command = new SqlCommand(restoreSql, connection))
                    {
                        command.CommandTimeout = 300; // Uzun sürebilir, timeout artırıldı
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation(
                            "Database {Name} restored successfully from {Path}.",
                            databaseName,
                            backupFilePath);
                    }

                    // MULTI_USER moduna geri alma
                    using(var command = new SqlCommand(multiUserSql, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Database {Name} set to MULTI_USER.", databaseName);
                    }
                }

                return true;
            } catch(SqlException ex)
            {
                _logger.LogError(ex, "SQL Server Restore failed for database {Name}.", databaseName);

                // Hata oluşsa bile MULTI_USER moduna geri almaya çalış.
                try
                {
                    var connectionString = _connectionFactory.GetMasterConnectionString();
                    using(var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        using(var command = new SqlCommand(
                            $"ALTER DATABASE [{databaseName}] SET MULTI_USER;",
                            connection))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                } catch(Exception cleanupEx)
                {
                    _logger.LogError(
                        cleanupEx,
                        "Failed to reset database {Name} to MULTI_USER after restore error.",
                        databaseName);
                }

                return false;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Restore operation failed for database {Name}.", databaseName);
                return false;
            }
        }
    }
}

