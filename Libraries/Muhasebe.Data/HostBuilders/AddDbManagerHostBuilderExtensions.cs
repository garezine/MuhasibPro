using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Data.DatabaseManager.AppDatabase;
using Muhasebe.Data.DatabaseManager.SistemDatabase;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.Helper;

namespace Muhasebe.Data.HostBuilders
{
    public static class AddDbManagerHostBuilderExtensions
    {
        public static IHostBuilder AddDatabaseManager(this IHostBuilder host)
        {
            host.ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppSistemDbContext>(options =>
                {
                    var dbPath = ConfigurationHelper.Instance.GetDatabasePath();
                    var connectionString = $"Data Source={Path.Combine(dbPath, "Sistem.db")};Mode=ReadWriteCreate;";
                    options.UseSqlite(connectionString, sqliteOptions =>
                    {
                        sqliteOptions.CommandTimeout(30);
                    });
                });
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(ConfigurationHelper.Instance.GetConfiguration().GetConnectionString("DefaultConnection"));
                    // veya
                    // options.UseSqlite("Data Source=muhasebe.db");
                });

                services.AddScoped<ISistemDatabaseManager, SistemDatabaseManager>();
                services.AddScoped<IAppDatabaseManager, AppDatabaseManager>();
            });

            return host;
        }
    }
}
