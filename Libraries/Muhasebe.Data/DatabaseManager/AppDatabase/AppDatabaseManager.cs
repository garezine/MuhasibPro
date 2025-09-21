using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.DatabaseManager.Models;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.Helper;

namespace Muhasebe.Data.DatabaseManager.AppDatabase
{
    public class AppDatabaseManager : IAppDatabaseManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppDatabaseManager> _logger;
        private readonly string _backupBasePath;

        public AppDatabaseManager(
            IServiceProvider serviceProvider,
            ILogger<AppDatabaseManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _backupBasePath = Path.Combine(
                ConfigurationHelper.Instance.GetProjectPath(),
                "Backups", "Muhasebe");

            Directory.CreateDirectory(_backupBasePath);
        }

        public async Task<bool> InitializeDatabaseAsync(string firmaKodu, string maliDonem)
        {
            try
            {
                using var context = CreateAppDbContext(firmaKodu, maliDonem);

                _logger.LogInformation("Initializing accounting database: {FirmaKodu}_{MaliYil}", firmaKodu, maliDonem);

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Found {Count} pending migrations for {FirmaKodu}_{MaliYil}",
                        pendingMigrations.Count(), firmaKodu, maliDonem);

                    // Migration öncesi backup
                    await CreateSafetyBackupAsync(firmaKodu, maliDonem);

                    // Migration'ları uygula
                    await context.Database.MigrateAsync();
                }

                var canConnect = await context.Database.CanConnectAsync();
                _logger.LogInformation("Accounting database initialization completed: {Success}", canConnect);

                return canConnect;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Accounting database initialization failed for {FirmaKodu}_{MaliYil}",
                    firmaKodu, maliDonem);
                return false;
            }
        }

        public async Task<bool> CreateNewDatabaseAsync(string firmaKodu, string maliDonem)
        {
            try
            {
                var databaseName = $"Muhasebe_{firmaKodu}_{maliDonem}";
                var masterConnectionString = GetMasterConnectionString();

                _logger.LogInformation("Creating new accounting database: {DatabaseName}", databaseName);

                using var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnectionString);
                await connection.OpenAsync();

                var createDbSql = $@"
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
                    BEGIN
                        CREATE DATABASE [{databaseName}]
                    END";

                using var command = new Microsoft.Data.SqlClient.SqlCommand(createDbSql, connection);
                await command.ExecuteNonQueryAsync();

                // Yeni oluşturulan database'i initialize et
                return await InitializeDatabaseAsync(firmaKodu, maliDonem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create accounting database for {FirmaKodu}_{MaliYil}",
                    firmaKodu, maliDonem);
                return false;
            }
        }

        public async Task<bool> CreateManualBackupAsync(string firmaKodu, string maliDonem)
        {
            try
            {
                var databaseName = $"Muhasebe_{firmaKodu}_{maliDonem}";
                var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonem}");
                Directory.CreateDirectory(backupDir);

                var backupFileName = $"manual_{firmaKodu}_{maliDonem}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var backupPath = Path.Combine(backupDir, backupFileName);

                using var context = CreateAppDbContext(firmaKodu, maliDonem);

                var sql = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = N'{backupPath.Replace("'", "''")}' 
                    WITH FORMAT, INIT, COMPRESSION";

                await context.Database.ExecuteSqlRawAsync(sql);

                _logger.LogInformation("Manual accounting backup created: {BackupPath}", backupPath);
                return File.Exists(backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual accounting backup failed for {FirmaKodu}_{MaliYil}",
                    firmaKodu, maliDonem);
                return false;
            }
        }

        public async Task<DatabaseHealthInfo> GetHealthInfoAsync(string firmaKodu, string maliDonem)
        {
            try
            {
                using var context = CreateAppDbContext(firmaKodu, maliDonem);

                var canConnect = await context.Database.CanConnectAsync();
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

                var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonem}");
                var backupFiles = Directory.Exists(backupDir)
                    ? Directory.GetFiles(backupDir, "*.bak").Length
                    : 0;

                return new DatabaseHealthInfo
                {
                    CanConnect = canConnect,
                    PendingMigrationsCount = pendingMigrations.Count(),
                    AppliedMigrationsCount = appliedMigrations.Count(),
                    BackupFilesCount = backupFiles,
                    LastBackupDate = GetLastBackupDate(firmaKodu, maliDonem)
                };
            }
            catch (Exception ex)
            {
                return new DatabaseHealthInfo { HasError = true, ErrorMessage = ex.Message };
            }
        }

        public Task<List<BackupFileInfo>> GetBackupHistoryAsync(string firmaKodu, string maliDonem)
        {
            try
            {
                var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonem}");

                if (!Directory.Exists(backupDir))
                    return Task.FromResult(new List<BackupFileInfo>());

                var result = Directory.GetFiles(backupDir, "*.bak")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(20)
                    .Select(f => new BackupFileInfo
                    {
                        FileName = f.Name,
                        FilePath = f.FullName,
                        CreatedDate = f.CreationTime,
                        SizeBytes = f.Length,
                        SizeFormatted = FormatFileSize(f.Length)
                    })
                    .ToList();
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get backup history for {FirmaKodu}_{MaliYil}",
                    firmaKodu, maliDonem);
                return Task.FromResult(new List<BackupFileInfo>());
            }
        }

        private AppDbContext CreateAppDbContext(string firmaKodu, string maliDonem)
        {
            var databaseName = $"Muhasebe_{firmaKodu}_{maliDonem}";
            var connectionString = $"Data Source=localhost;Integrated Security=true;TrustServerCertificate=true;Initial Catalog={databaseName};";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AppDbContext(options);
        }

        private string GetMasterConnectionString()
        {
            return "Data Source=localhost;Integrated Security=true;TrustServerCertificate=true;Initial Catalog=master;";
        }

        private async Task CreateSafetyBackupAsync(string firmaKodu, string maliDonem)
        {
            var databaseName = $"Muhasebe_{firmaKodu}_{maliDonem}";
            var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonem}");
            Directory.CreateDirectory(backupDir);

            var backupFileName = $"safety_{firmaKodu}_{maliDonem}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var backupPath = Path.Combine(backupDir, backupFileName);

            try
            {
                using var context = CreateAppDbContext(firmaKodu, maliDonem);

                var sql = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = N'{backupPath.Replace("'", "''")}' 
                    WITH FORMAT, INIT, COMPRESSION";

                await context.Database.ExecuteSqlRawAsync(sql);
                _logger.LogInformation("Safety backup created: {BackupPath}", backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Safety backup failed: {BackupPath}", backupPath);
            }
        }

        private DateTime? GetLastBackupDate(string firmaKodu, string maliDonem)
        {
            try
            {
                var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonem}");

                if (!Directory.Exists(backupDir))
                    return null;

                var lastBackup = Directory.GetFiles(backupDir, "*.bak")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .FirstOrDefault();

                return lastBackup?.CreationTime;
            }
            catch
            {
                return null;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1}{suffixes[counter]}";
        }
    }
}
