using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Configurations;

namespace Muhasebe.Data.Database.Concreate.Configurations
{
    public class DatabaseDirectoryManager : IDatabaseDirectoryManager
    {
        private readonly ILogger<DatabaseDirectoryManager> _logger;

        public DatabaseDirectoryManager(ILogger<DatabaseDirectoryManager> logger)
        {
            _logger = logger;
        }

        public string EnsureDirectoryExists(string baseDir)
        {
            if (Directory.Exists(baseDir))
            {
                _logger.LogWarning("Directory already exists: {DirectoryPath}", baseDir);
                return baseDir;
            }

            Directory.CreateDirectory(baseDir);
            _logger.LogInformation("Directory created: {DirectoryPath}", baseDir);
            return baseDir;
        }

        public void CleanupDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
                _logger.LogInformation("Directory deleted: {DirectoryPath}", directoryPath);
            }
        }
    }
}
