namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager
{
    public interface ISqlConnectionStringFactory
    {
        string CreateForDatabase(string databaseName);
        string CreateForMaster();
    }
}
