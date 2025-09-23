using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.DatabaseManager.Models;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.Helper;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Helpers;

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
                ConfigurationHelper.Instance.GetDatabasePath(),
                "Backups", "Muhasebe");

            Directory.CreateDirectory(_backupBasePath);
        }

        public async Task<bool> InitializeDatabaseAsync(string firmaKodu, int maliDonemYil)
        {
            try
            {
                using var context = CreateAppDbContext(firmaKodu, maliDonemYil);

                _logger.LogInformation("Initializing accounting database: {FirmaKodu}_{MaliYil}", firmaKodu, maliDonemYil);

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Found {Count} pending migrations for {FirmaKodu}_{MaliYil}",
                        pendingMigrations.Count(), firmaKodu, maliDonemYil);

                    // Migration öncesi backup
                    await CreateSafetyBackupAsync(firmaKodu, maliDonemYil);

                    try
                    {
                        // Migration'ları uygula
                        await context.Database.MigrateAsync();

                        // Migration başarılı - versiyon bilgisini güncelle
                        var migrationVersion = GetLatestMigrationVersion(await context.Database.GetAppliedMigrationsAsync());
                        await UpdateMuhasebeVersionAsync(firmaKodu, maliDonemYil, migrationVersion);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Migration failed for {FirmaKodu}_{MaliYil}", firmaKodu, maliDonemYil);
                        throw;
                    }
                }

                var canConnect = await context.Database.CanConnectAsync();
                _logger.LogInformation("Accounting database initialization completed: {Success}", canConnect);

                return canConnect;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Accounting database initialization failed for {FirmaKodu}_{MaliYil}",
                    firmaKodu, maliDonemYil);
                return false;
            }
        }

        public async Task<bool> CreateNewDatabaseAsync(string firmaKodu, int maliDonemYil)
        {
            try
            {
                var databaseName = AppMessage.VeritabaniBilgileri.MuhasebeVeritabaniAdi(firmaKodu, maliDonemYil);
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
                return await InitializeDatabaseAsync(firmaKodu, maliDonemYil);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create accounting database for {FirmaKodu}_{MaliYil}",
                    firmaKodu, maliDonemYil);
                return false;
            }
        }

        public async Task<bool> CreateManualBackupAsync(string firmaKodu, int maliDonemYil)
        {
            try
            {
                var databaseName = AppMessage.VeritabaniBilgileri.MuhasebeVeritabaniAdi(firmaKodu, maliDonemYil);
                var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonemYil}");
                Directory.CreateDirectory(backupDir);

                var backupFileName = $"manual_{firmaKodu}_{maliDonemYil}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var backupPath = Path.Combine(backupDir, backupFileName);

                using var context = CreateAppDbContext(firmaKodu, maliDonemYil);

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
                    firmaKodu, maliDonemYil);
                return false;
            }
        }

        public async Task<DatabaseHealthInfo> GetHealthInfoAsync(string firmaKodu, int maliDonemYil)
        {
            try
            {
                using var context = CreateAppDbContext(firmaKodu, maliDonemYil);

                var canConnect = await context.Database.CanConnectAsync();
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

                var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonemYil}");
                var backupFiles = Directory.Exists(backupDir)
                    ? Directory.GetFiles(backupDir, "*.bak").Length
                    : 0;

                return new DatabaseHealthInfo
                {
                    CanConnect = canConnect,
                    PendingMigrationsCount = pendingMigrations.Count(),
                    AppliedMigrationsCount = appliedMigrations.Count(),
                    BackupFilesCount = backupFiles,
                    LastBackupDate = GetLastBackupDate(firmaKodu, maliDonemYil)
                };
            }
            catch (Exception ex)
            {
                return new DatabaseHealthInfo { HasError = true, ErrorMessage = ex.Message };
            }
        }

        public Task<List<BackupFileInfo>> GetBackupHistoryAsync(string firmaKodu, int maliDonemYil)
        {
            try
            {
                var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonemYil}");

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
                    firmaKodu, maliDonemYil);
                return Task.FromResult(new List<BackupFileInfo>());
            }
        }

        private AppDbContext CreateAppDbContext(string firmaKodu, int maliDonemYil)
        {
            var databaseName = AppMessage.VeritabaniBilgileri.MuhasebeVeritabaniAdi(firmaKodu, maliDonemYil);
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

        private async Task CreateSafetyBackupAsync(string firmaKodu, int maliDonemYil)
        {
            var databaseName = AppMessage.VeritabaniBilgileri.MuhasebeVeritabaniAdi(firmaKodu, maliDonemYil);
            var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonemYil}");
            Directory.CreateDirectory(backupDir);

            var backupFileName = $"safety_{firmaKodu}_{maliDonemYil}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var backupPath = Path.Combine(backupDir, backupFileName);

            try
            {
                using var context = CreateAppDbContext(firmaKodu, maliDonemYil);

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

        private DateTime? GetLastBackupDate(string firmaKodu, int maliDonemYil)
        {
            try
            {
                var backupDir = Path.Combine(_backupBasePath, $"{firmaKodu}_{maliDonemYil}");

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
        public async Task<MuhasebeVersiyon> GetCurrentMuhasebeVersionAsync(string firmaKodu, int maliDonemYil)
        {
            try
            {
                using var context = CreateAppDbContext(firmaKodu, maliDonemYil);
                return await context.MuhasebeVersiyonlar
                    .Where(v => v.FirmaKodu == firmaKodu && v.MaliDonemYil == maliDonemYil)
                    .OrderByDescending(v => v.MuhasebeDBSonGuncellemeTarihi)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get muhasebe version for {FirmaKodu}_{MaliYil}", firmaKodu, maliDonemYil);
                return null;
            }
        }

        public async Task<bool> UpdateMuhasebeVersionAsync(string firmaKodu, int maliDonemYil, string newVersion)
        {
            try
            {
                using var context = CreateAppDbContext(firmaKodu, maliDonemYil);

                var currentVersion = await context.MuhasebeVersiyonlar
                    .Where(v => v.FirmaKodu == firmaKodu && v.MaliDonemYil == maliDonemYil)
                    .FirstOrDefaultAsync();

                if (currentVersion == null)
                {
                    // İlk kurulum
                    var initialVersion = new MuhasebeVersiyon
                    {
                        FirmaKodu = firmaKodu,
                        MaliDonemYil = maliDonemYil,
                        MuhasebeDBVersiyon = newVersion,
                        MuhasebeDBSonGuncellemeTarihi = DateTime.Now,
                        OncekiMuhasebeDbVersiyon = null
                    };
                    context.MuhasebeVersiyonlar.Add(initialVersion);
                }
                else
                {
                    // Güncelleme
                    currentVersion.OncekiMuhasebeDbVersiyon = currentVersion.MuhasebeDBVersiyon;
                    currentVersion.MuhasebeDBVersiyon = newVersion;
                    currentVersion.MuhasebeDBSonGuncellemeTarihi = DateTime.Now;
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Muhasebe version updated to {Version} for {FirmaKodu}_{MaliYil}",
                    newVersion, firmaKodu, maliDonemYil);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update muhasebe version for {FirmaKodu}_{MaliYil}", firmaKodu, maliDonemYil);
                return false;
            }
        }
        private string GetLatestMigrationVersion(IEnumerable<string> appliedMigrations)
        {
            var latest = appliedMigrations.LastOrDefault();
            if (latest != null && latest.Length > 8) // Migration timestamp kısmını al
            {
                return latest.Substring(0, 14); // YYYYMMDDHHMMSS formatı
            }
            return "1.0.0";
        }
    }
}
