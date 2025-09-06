namespace Muhasebe.Data.Database.Interfaces.Operations
{
    // Veritabanı bakım işlemleri (indeksleme, bütünlük kontrolü vb.)
    public interface IDatabaseMaintenanceOperations
    {
        Task CheckIntegrityAsync(string connectionString, string dbName, string dbPath);
        Task ReindexTablesAsync(string connectionString, string dbName, string dbPath);
        Task ShrinkDatabaseAsync(string connectionString, string dbName, string dbPath);
    }
}
