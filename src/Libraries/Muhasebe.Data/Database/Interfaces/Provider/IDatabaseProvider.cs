using Microsoft.EntityFrameworkCore;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Interfaces.Provider
{
    public interface IDatabaseProvider
    {
        DatabaseType DbType { get; }
        string GenerateConnectionString(string dbName, string baseDir);
        void ConfigureContext(DbContextOptionsBuilder optionsBuilder, string connectionString);
        Task CreateDatabaseAsync(string dbPathOrIdentifier, string baseDir); // dbPathOrIdentifier provider'a özel kullanılır
        Task ApplySchemaAsync(DbContext context);
        Task CleanupDatabaseAsync(string dbPathOrIdentifier, string baseDir); // YENİ: Var olan DB/dosyaları temizler

    }
}
