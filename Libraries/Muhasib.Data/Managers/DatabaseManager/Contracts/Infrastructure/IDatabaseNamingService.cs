namespace Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure
{
    public interface IDatabaseNamingService
    {
        string GenerateDatabaseName(string firmaKodu, int maliYil);
        string GenerateBackupFileName(string identifier);
    }
}
