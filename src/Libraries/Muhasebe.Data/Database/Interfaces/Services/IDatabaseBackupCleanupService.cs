namespace Muhasebe.Data.Database.Interfaces.Services
{
    public interface IDatabaseBackupCleanupService
    {
        void CleanupOldBackups(string backupBaseDirectory, int maxAgeInDays);
    }
}
