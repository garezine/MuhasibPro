using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;

namespace Muhasib.Data.DataContext.DesignTimeFactory
{
    public class DesignTimeSistemDbContextFactory : IDesignTimeDbContextFactory<SistemDbContext>
    {
        public SistemDbContext CreateDbContext(string[] args)
        {
            var dbPath = DesignTimePathResolver.GetDatabasePath();
            var fullDbPath = Path.Combine(dbPath, "Sistem.db");
            var connectionString = $"Data Source={fullDbPath};Mode=ReadWriteCreate;";

            Console.WriteLine($"Design Time - Database Path: {fullDbPath}");

            var optionsBuilder = new DbContextOptionsBuilder<SistemDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new SistemDbContext(optionsBuilder.Options);
        }
    }
    public static class DesignTimePathResolver
    {
        public static string GetDatabasePath()
        {
            var environmentDetector = new EnvironmentDetector();
            var applicationPaths = new ApplicationPaths(environmentDetector);
            return applicationPaths.GetSistemDbPath();
        }
    }
}
