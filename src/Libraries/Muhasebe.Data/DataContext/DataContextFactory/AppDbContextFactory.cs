using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.Database.Interfaces.Configurations;

namespace Muhasebe.Data.DataContext.DataContextFactory
{
    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly IDatabaseConfiguration _dbConfig;

        public AppDbContextFactory(IDatabaseConfiguration dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public AppDbContext CreateDbContext()
        {
            var provider = _dbConfig.GetCurrentProvider();
            var options = new DbContextOptionsBuilder<AppDbContext>();
            provider.ConfigureContext(options, _dbConfig.GetConnectionString());
            return new AppDbContext(options.Options);
        }
    }
}
