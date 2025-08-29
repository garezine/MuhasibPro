namespace Muhasebe.Data.Database.Interfaces.Configurations
{
    public interface IDatabaseDirectoryManager
    {
        string EnsureDirectoryExists(string baseDir);
        void CleanupDirectory(string directoryPath);
    }
}
