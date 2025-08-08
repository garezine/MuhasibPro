using Muhasebe.Data.Database.Interfaces.Provider;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Concreate.Providers
{
    public class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        // Constructor artık IConfiguration yerine IServiceProvider alıyor.
        // IServiceProvider, DI container'a erişim sağlar.
        public DatabaseProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IDatabaseProvider Create(DatabaseType dbType)
        {
            // Artık 'new' ile oluşturmak yerine DI container'dan istiyoruz.
            // DI container, provider'ın kendi bağımlılıklarını (IConfiguration, ILogger<T>)
            // otomatik olarak çözecektir.
            var provider = _serviceProvider.GetService(GetProviderType(dbType));
            if (provider is null)
            {
                throw new ArgumentOutOfRangeException(nameof(dbType), $"Desteklenmeyen veritabanı türü: {dbType}");
            }

            return (IDatabaseProvider)provider;
        }
        private static Type GetProviderType(DatabaseType dbType) => dbType switch
        {
            DatabaseType.SqlServer => typeof(SqlServerProvider),
            DatabaseType.SQLite => typeof(SQLiteProvider),
            _ => throw new ArgumentOutOfRangeException(nameof(dbType), $"Desteklenmeyen veritabanı türü: {dbType}")
        };
    }
}
