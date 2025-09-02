using System;
using System.Linq;

namespace Muhasebe.Data.Database.Helpers
{
    public class RestoreResult
    {
        public bool Success { get; }
        public string Message { get; }
        public string BackupFilePath { get; }
        public string TargetDatabaseName { get; }
        public string TargetDbDirectory { get; }
        public string TargetDbPath { get; }
        public RestoreResult(bool success, string message = null, string backupFilePath = null, string targetDatabaseName = null, string targetDbDirectory = null, string targetDbPath = null)
        {
            Success = success;
            Message = message;
            BackupFilePath = backupFilePath;
            TargetDatabaseName = targetDatabaseName;
            TargetDbDirectory = targetDbDirectory;
            TargetDbPath = targetDbPath;
        }
    }
}
