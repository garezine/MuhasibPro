using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase;
using Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase;
using Muhasib.Data.DataContext;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Concrete.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Concrete.TenantSqliteManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Managers.UpdataManager;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.HostBuilders
{
    public static class AddDbManagerHostBuilderExtensions
    {
        public static IHostBuilder AddDatabaseManagement(this IHostBuilder host)
        {
            host.ConfigureServices(
                (context, services) =>
                {
                    services.AddDbContext<SistemDbContext>(
                        options =>
                        {
                            var paths = services.BuildServiceProvider().GetRequiredService<IApplicationPaths>();
                            var dbPath = paths.GetDatabasesFolderPath();
                            var connectionString = $"Data Source={Path.Combine(dbPath, DatabaseConstants.SISTEM_DB_NAME)};Mode=ReadWriteCreate;";
                            options.UseSqlite(
                                connectionString,
                                sqliteOptions =>
                                {
                                    sqliteOptions.CommandTimeout(30);
                                });
                        });

                    services.AddScoped<AppDbContext>(provider =>
                    {
                        var factory = provider.GetRequiredService<IAppDbContextFactory>();
                        var tenantManager = provider.GetService<ITenantSQLiteSelectionManager>();

                        // Eğer aktif tenant varsa onun context'ini ver
                        if (tenantManager?.IsTenantLoaded == true)
                        {
                            var currentTenant = tenantManager.GetCurrentTenant();
                            return factory.CreateContext(
                                currentTenant.DatabaseName);
                        }

                        // Yoksa sistem context'i
                        return factory.CreateContext("Sistem");
                    });


                    //Sistem Database Managers
                    services.AddSingleton<ISistemDatabaseManager, SistemDatabaseManager>();
                    services.AddSingleton<ISistemDatabaseService, SistemDatabaseService>();
                    services.AddSingleton<ISistemDatabaseUpdateService, SistemDatabaseUpdateService>();

                    services.AddScoped<ILocalUpdateManager, LocalUpdateManager>();
                    services.AddSingleton<IAppDbContextFactory, AppDbContextFactory>();
                                    


                    // Managers
                    
                    
                    services.AddSingleton<ISQLiteConnectionStringFactory, SQLiteConnectionStringFactory>();
                    services.AddSingleton<ITenantSQLiteBackupManager, TenantSQLiteBackupManager>();
                    services.AddSingleton<ITenantSQLiteSelectionManager, TenantSQLiteSelectionManager>();
                    
                    services.AddSingleton<ISQLiteDatabaseManager, SQLiteDatabaseManager>();


                    //Tenant Database Services                    
                    

                    //Database Infrastructure Services
                    services.AddSingleton<IEnvironmentDetector, EnvironmentDetector>();
                    services.AddSingleton<IApplicationPaths, ApplicationPaths>();
                    services.AddSingleton<IDatabaseNamingService, DatabaseNamingService>();

                   
                });
            return host;
        }
    }
}
