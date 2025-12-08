using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure
{
    public static class DatabaseConstants
    {
        public const string DATABASE_FOLDER = "Databases";
        public const string SISTEM_DB_NAME = "Sistem.db";
        public const string TENANT_DATABASES_FOLDER = "TenantDatabases";
        public const string BACKUP_FOLDER = "Backups";
        public const string TENANT_BACKUPS_FOLDER = "TenantBackups";
        public const string TEMP_FOLDER = "Temp";
    }

    public class ApplicationPaths : IApplicationPaths
    {
        private readonly IEnvironmentDetector _environmentDetector;
        private readonly string _applicationName;

        // Simple cache - thread-safe için Lazy<T>
        private static readonly Lazy<string> _cachedDevProjectPath = new Lazy<string>(
            () =>
            {
                var currentDir = AppContext.BaseDirectory;
                var dirInfo = new DirectoryInfo(currentDir);

                // Max 6 levels up (8 fazlaydı)
                for (int i = 0; i < 6 && dirInfo?.Parent != null; i++)
                {
                    if (dirInfo.GetFiles("*.csproj", SearchOption.TopDirectoryOnly).Length > 0 ||
                        dirInfo.GetFiles("*.sln", SearchOption.TopDirectoryOnly).Length > 0)
                        return dirInfo.FullName;

                    dirInfo = dirInfo.Parent;
                }

                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MuhasibPro");
            });

        public ApplicationPaths(IEnvironmentDetector environmentDetector, string applicationName = "MuhasibPro")
        {
            _environmentDetector = environmentDetector;
            _applicationName = applicationName ?? "MuhasibPro";
        }

        #region Base Paths
        public string GetAppDataFolderPath()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                _applicationName);

            // Directory.CreateDirectory zaten thread-safe ve idempotent
            Directory.CreateDirectory(path);
            return path;
        }

        private string GetDevelopmentProjectFolderPath() => _cachedDevProjectPath.Value;

        private string GetRootDataPath()
        { return _environmentDetector.IsDevelopment() ? GetDevelopmentProjectFolderPath() : GetAppDataFolderPath(); }
        #endregion

        #region Databases Structure
        // [ROOT]/Databases/
        public string GetDatabasesFolderPath()
        {
            var path = Path.Combine(GetRootDataPath(), DatabaseConstants.DATABASE_FOLDER);
            Directory.CreateDirectory(path);
            return path;
        }

        // [ROOT]/Databases/Tenant/
        public string GetTenantDatabasesFolderPath()
        {
            var path = Path.Combine(GetDatabasesFolderPath(), DatabaseConstants.TENANT_DATABASES_FOLDER);
            Directory.CreateDirectory(path);
            return path;
        }

        // [ROOT]/Databases/sistem.db
        public string GetSistemDatabaseFilePath()
        { return Path.Combine(GetDatabasesFolderPath(), DatabaseConstants.SISTEM_DB_NAME); }

        // [ROOT]/Databases/Tenant/{databaseName}.db
        public string GetTenantDatabaseFilePath(string databaseName)
        {
            var sanitizedName = SanitizeDatabaseName(databaseName);
            var tenantPath = GetTenantDatabasesFolderPath();

            var fileName = sanitizedName.EndsWith(".db", StringComparison.OrdinalIgnoreCase)
                ? sanitizedName
                : $"{sanitizedName}.db";

            return Path.Combine(tenantPath, fileName);
        }

        // Basit ve güvenli sanitize
        public string SanitizeDatabaseName(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database adı boş olamaz");

            if (databaseName.Length > 100)
                throw new ArgumentException("Database adı çok uzun");

            // Sadece güvenli olmayan karakterleri temizle
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(
                databaseName
                .Where(c => !invalidChars.Contains(c))
                    .ToArray())
                .Trim();

            // Basit rezerve isim kontrolü
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL" };
            if (reservedNames.Contains(sanitized.ToUpperInvariant()))
                throw new ArgumentException($"'{sanitized}' rezerve bir dosya adıdır");

            return string.IsNullOrEmpty(sanitized) ? throw new ArgumentException("Database adı geçersiz") : sanitized;
        }
        #endregion

        #region Backup Structure
        // [ROOT]/Databases/Backup/
        public string GetBackupFolderPath()
        {
            var path = Path.Combine(GetDatabasesFolderPath(), DatabaseConstants.BACKUP_FOLDER);
            Directory.CreateDirectory(path);
            return path;
        }

        // [ROOT]/Databases/Backup/Tenant/
        public string GetTenantBackupFolderPath()
        {
            var path = Path.Combine(GetBackupFolderPath(), DatabaseConstants.TENANT_BACKUPS_FOLDER);
            Directory.CreateDirectory(path);
            return path;
        }

        #endregion

        #region Temp Structure
        // [ROOT]/Temp/
        public string GetTempFolderPath()
        {
            var path = Path.Combine(GetRootDataPath(), DatabaseConstants.TEMP_FOLDER);
            Directory.CreateDirectory(path);
            return path;
        }
        #endregion

        #region DatabaseExists

        public long GetSistemDatabaseSize()
        {
            try
            {
                var dbFilePath = GetSistemDatabaseFilePath();
                if (!File.Exists(dbFilePath))
                    return 0L;

                var fileInfo = new FileInfo(dbFilePath);
                return fileInfo.Length;
            }
            catch
            {
                return 0L;
            }
        }
        public bool IsSistemDatabaseSizeValid()
        {
            try
            {
                var dbPath = GetSistemDatabaseFilePath();

                if (!File.Exists(dbPath))
                    return false;

                var fileInfo = new FileInfo(dbPath);
                return fileInfo.Length > 0;
            }
            catch
            {
                return false;
            }
        }
        public bool SistemDatabaseFileExists()
        {
            try
            {
                var filePath = GetSistemDatabaseFilePath();
                return File.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }

        //Tenant Database
        public bool IsTenantDatabaseSizeValid(string databaseName)
        {
            try
            {
                var dbPath = GetTenantDatabaseFilePath(databaseName);

                if (!File.Exists(dbPath))
                    return false;

                var fileInfo = new FileInfo(dbPath);
                return fileInfo.Length > 0;
            }
            catch
            {
                return false;
            }
        }
        public bool TenantDatabaseFileExists(string databaseName)
        {
            try
            {
                var filePath = GetTenantDatabaseFilePath(databaseName);
                return File.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }
        public long GetTenantDatabaseSize(string databaseName)
        {
            try
            {
                var dbFilePath = GetTenantDatabaseFilePath(databaseName);
                if (!File.Exists(dbFilePath))
                    return 0L;

                var fileInfo = new FileInfo(dbFilePath);
                return fileInfo.Length;
            }
            catch
            {
                return 0L;
            }
        }

        #endregion

        #region Helper Methods


        public string GenerateUniqueTempFilePath(string extension = ".tmp")
        {
            var tempDir = GetTempFolderPath();
            var fileName = $"temp_{Guid.NewGuid():N}{extension}";
            return Path.Combine(tempDir, fileName);
        }

        public void CleanupTempFiles(TimeSpan olderThan)
        {
            var tempDir = GetTempFolderPath();
            if (!Directory.Exists(tempDir))
                return;

            var cutoff = DateTime.UtcNow - olderThan;

            foreach (var file in Directory.GetFiles(tempDir))
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(file) < cutoff)
                        File.Delete(file);
                }
                catch
                {
                    // Temp dosya silinemezse problem değil
                }
            }
        }

        public void CleanupSqliteWalFiles(string databaseName)
        {
            try
            {
                var dbPath = GetTenantDatabaseFilePath(databaseName);

                // WAL ve SHM dosyalarını sil
                var walPath = dbPath + "-wal";
                var shmPath = dbPath + "-shm";

                if (File.Exists(walPath))
                {
                    File.Delete(walPath);

                }

                if (File.Exists(shmPath))
                {
                    File.Delete(shmPath);

                }
            }
            catch
            {

            }
        }
        #endregion
    }
}