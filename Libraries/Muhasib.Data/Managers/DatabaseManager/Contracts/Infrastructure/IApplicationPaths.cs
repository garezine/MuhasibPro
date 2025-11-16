namespace Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure
{
    public interface IApplicationPaths
    {
        string GetDatabasePath();
        string GetAppDataPath();
        string GetBackupPath();
        string GetTempPath();
        string GetSistemDbPath();
    }
}
