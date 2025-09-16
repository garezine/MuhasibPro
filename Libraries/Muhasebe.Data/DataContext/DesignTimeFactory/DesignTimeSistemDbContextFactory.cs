using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Muhasebe.Data.Helper;

namespace Muhasebe.Data.DataContext.DesignTimeFactory
{
    public class DesignTimeSistemDbContextFactory : IDesignTimeDbContextFactory<AppSistemDbContext>
    {
        public AppSistemDbContext CreateDbContext(string[] args)
        {
            var dbPath = ConfigurationHelper.Instance.GetDatabasePath();
            var fullDbPath = Path.Combine(dbPath, "Sistem.db");
            var connectionString = $"Data Source={fullDbPath};Mode=ReadWriteCreate;";

            Console.WriteLine($"Design Time - Database Path: {fullDbPath}");

            var optionsBuilder = new DbContextOptionsBuilder<AppSistemDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new AppSistemDbContext(optionsBuilder.Options);
        }
    }
}
