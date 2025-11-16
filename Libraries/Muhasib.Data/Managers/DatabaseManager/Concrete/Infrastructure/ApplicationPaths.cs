using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure
{
    public class ApplicationPaths : IApplicationPaths
    {
        private readonly IEnvironmentDetector _environmentDetector;
        private readonly string _applicationName;

        public ApplicationPaths(IEnvironmentDetector environmentDetector, string applicationName = "MuhasibPro")
        {
            _environmentDetector = environmentDetector;
            _applicationName = applicationName;
        }

        public string GetDatabasePath()
        {
            var basePath = _environmentDetector.IsDevelopment()
                ? GetDevelopmentProjectPath()
                : GetAppDataPath();

            var dbPath = Path.Combine(basePath, "Databases");
            Directory.CreateDirectory(dbPath);
            return dbPath;
        }

        public string GetAppDataPath()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                _applicationName
            );
            Directory.CreateDirectory(path);
            return path;
        }

        public string GetBackupPath()
        {
            var basePath = GetAppDataPath();
            var backupPath = Path.Combine(basePath, "Backups");
            Directory.CreateDirectory(backupPath);
            return backupPath;
        }

        public string GetTempPath()
        {
            var basePath = GetAppDataPath();
            var tempPath = Path.Combine(basePath, "Temp");
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        public string GetSistemDbPath()
        {
            var dbPath = GetDatabasePath();
            return Path.Combine(dbPath, "Sistem.db");
        }

        private string GetDevelopmentProjectPath()
        {
            var currentDirectory = AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(currentDirectory);

            while (directoryInfo != null)
            {
                var projectFiles = directoryInfo.GetFiles("*.csproj");
                if (projectFiles.Length > 0)
                {
                    return directoryInfo.FullName;
                }
                directoryInfo = directoryInfo.Parent;
            }

            return GetAppDataPath();
        }
    }
}
