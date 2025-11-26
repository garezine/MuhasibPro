using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;

namespace Muhasib.Data.DataContext.Factories
{
    /// <summary>
    /// AppDbContext instance'ları oluşturmak için factory sınıfı. Connection string'e göre otomatik provider seçimi
    /// yapar (SQLite/SQLServer). Dependency Injection ile kullanım için tasarlanmıştır.
    /// </summary>
    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly ISqlConnectionStringFactory _connectionStringFactory;
        private readonly IMaliDonemRepository _maliDonemRepo;

        public AppDbContextFactory(
            ISqlConnectionStringFactory connectionStringFactory,
            IMaliDonemRepository maliDonemDbRepo)
        {
            _connectionStringFactory = connectionStringFactory;
            _maliDonemRepo = maliDonemDbRepo;
        }

        public AppDbContext CreateForTenant(long maliDonemId)
        {
            var maliDonemDb = _maliDonemRepo.GetByMaliDonemId(maliDonemId);

            if(maliDonemDb == null)
                throw new InvalidOperationException($"MaliDonemDb bulunamadı: {maliDonemId}");


            return CreateForDatabase(maliDonemDb.DBName);
        }

        public async Task<AppDbContext> CreateForTenantAsync(long maliDonemId)
        {
            var maliDonemDb = await _maliDonemRepo.GetByMaliDonemIdAsync(maliDonemId);

            if(maliDonemDb == null)
                throw new InvalidOperationException($"MaliDonemDb bulunamadı: {maliDonemId}");

            return CreateForDatabase(maliDonemDb.DBName);
        }

        public async Task<AppDbContext> CreateForDatabaseAsync(string databaseName)
        { return await Task.FromResult(CreateForDatabase(databaseName)); }

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
