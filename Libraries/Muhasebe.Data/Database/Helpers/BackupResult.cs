namespace Muhasebe.Data.Database.Helpers
{
    public class BackupResult
    {
        public bool Success { get; }
        public string Message { get; }
        public string BackupFilePath { get; }

        public BackupResult(bool success, string message, string backupFilePath = null)
        {
            Success = success;
            Message = message;
            BackupFilePath = backupFilePath;
        }
    }
}
