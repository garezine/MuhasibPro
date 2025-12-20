using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;

namespace Muhasib.Data.DataContext.DesignTimeFactory
{
    public class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var dbPath = DesignTimePathResolver.GetDatabasePath();

            var connectionString = $"Data Source={dbPath};Mode=ReadWriteCreate;";

            Console.WriteLine($"Design Time - Database Path: {dbPath}");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
        public static class DesignTimePathResolver
        {
            public static string GetDatabasePath()
            {
                var environmentDetector = new EnvironmentDetector();
                var applicationPaths = new ApplicationPaths(environmentDetector);
                var databaseName = "TenantTest";
                return applicationPaths.GetTenantDatabaseFilePath(databaseName);
            }
        }
    }
}
