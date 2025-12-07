using Muhasib.Data.DataContext.Factories;
using Muhasib.Domain.Enum;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager
{
    public interface ISQLiteConnectionStringFactory
    {
        /// <summary>
        /// Database adı ve tip'e göre connection string oluşturur
        /// </summary>
        /// <param name="databaseName">Database adı (örn: FIRMA001_2024)</param>
        /// <param name="dbType">Veritabanı tipi (SQLite, SqlServer)</param>
        /// <returns>Connection string</returns>
        string CreateConnectionString(string databaseName);
        Task<bool> TestConnectionStringAsync(string databaseName, CancellationToken cancellationToken = default);


    }
}
