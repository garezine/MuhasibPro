namespace Muhasebe.Data.Database.Helpers
{
    public class DeletionResult
    {
        public bool Success { get; }
        public string Message { get; }
        public string DeletedDatabaseName { get; }
        public string DeletedDirectoryPath { get; }

        public DeletionResult(
            bool success,
            string message,
            string dbName = null,
            string directoryPath = null)
        {
            Success = success;
            Message = message;
            DeletedDatabaseName = dbName;
            DeletedDirectoryPath = directoryPath;
        }
    }
}
