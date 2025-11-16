using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;

namespace Muhasib.Data.DataContext.Factories
{
    /// <summary>
    /// AppDbContext instance'ları oluşturmak için factory sınıfı.
    /// Connection string'e göre otomatik provider seçimi yapar (SQLite/SQLServer).
    /// Dependency Injection ile kullanım için tasarlanmıştır.
    /// </summary>
    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly ISqlConnectionStringFactory _connectionStringFactory;
        private readonly IMaliDonemDbRepository _maliDonemDbRepo;

        public AppDbContextFactory(
            ISqlConnectionStringFactory connectionStringFactory,
            IMaliDonemDbRepository maliDonemDbRepo)
        {
            _connectionStringFactory = connectionStringFactory;
            _maliDonemDbRepo = maliDonemDbRepo;
        }

        public AppDbContext CreateForTenant(long maliDonemId)
        {
            var maliDonemDb = _maliDonemDbRepo.GetByMaliDonemDbId(maliDonemId);

            if (maliDonemDb == null)
                throw new InvalidOperationException($"MaliDonemDb bulunamadı: {maliDonemId}");

            return CreateForDatabase(maliDonemDb.DBName);
        }
        public async Task<AppDbContext> CreateForTenantAsync(long maliDonemId)
        {
            var maliDonemDb = await _maliDonemDbRepo.GetByMaliDonemDbIdAsync(maliDonemId);

            if (maliDonemDb == null)
                throw new InvalidOperationException($"MaliDonemDb bulunamadı: {maliDonemId}");

            return CreateForDatabase(maliDonemDb.DBName);
        }

        public async Task<AppDbContext> CreateForDatabaseAsync(string databaseName)
        {
            return await Task.FromResult(CreateForDatabase(databaseName));
        }

        public AppDbContext CreateForDatabase(string databaseName)
        {
            var connectionString = _connectionStringFactory.CreateForDatabase(databaseName);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AppDbContext(options);
        }
    }
}
