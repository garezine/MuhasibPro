using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase;
using Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Data.DataContext;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Concrete.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Concrete.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Concrete.TenantSqliteManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Managers.UpdataManager;

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
                        (provider, options) =>
                        {
                            // ⭐ DOĞRUSU: IApplicationPaths'den METOD çağır
                            var appPaths = provider.GetRequiredService<IApplicationPaths>();
                            var sistemDbPath = appPaths.GetSistemDatabaseFilePath();

                            var connectionString = $"Data Source={sistemDbPath};Mode=ReadWriteCreate;";


                            options.UseSqlite(
                                connectionString,
                                sqliteOptions =>
                                {
                                    sqliteOptions.CommandTimeout(30);
                                    sqliteOptions.MigrationsAssembly("Muhasib.Data");
                                });

#if DEBUG
                        options.EnableSensitiveDataLogging();
                            options.EnableDetailedErrors();

#endif
                    });


                    services.AddScoped<AppDbContext>(
                        provider =>
                        {
                            var factory = provider.GetRequiredService<IAppDbContextFactory>();
                            var tenantManager = provider.GetService<ITenantSQLiteSelectionManager>();

                            // Eğer aktif tenant varsa onun context'ini ver
                            if(tenantManager?.IsTenantLoaded == true)
                            {
                                var currentTenant = tenantManager.GetCurrentTenant();
                                return factory.CreateContext(currentTenant.DatabaseName);
                            }

                            // Yoksa sistem context'i
                            return factory.CreateContext("Sistem");
                        });

                    //Sistem Database Managers
                    services.AddSingleton<ISistemDatabaseManager, SistemDatabaseManager>();
                    services.AddSingleton<ISistemBackupManager, SistemBackupManager>();
                    services.AddSingleton<ISistemMigrationManager, SistemMigrationManager>();


                    services.AddSingleton<ISistemDatabaseService, SistemDatabaseService>();
                    services.AddSingleton<ISistemDatabaseOperationService, SistemDatabaseOperationService>();


                    services.AddScoped<ILocalUpdateManager, LocalUpdateManager>();


                    // Managers

                    services.AddSingleton<IAppDbContextFactory, AppDbContextFactory>();
                    services.AddSingleton<ITenantSQLiteConnectionStringFactory, TenantSQLiteConnectionStringFactory>();
                    services.AddSingleton<ITenantSQLiteDatabaseManager, TenantSQLiteDatabaseManager>();
                    services.AddSingleton<ITenantSQLiteBackupManager, TenantSQLiteBackupManager>();
                    
                    services.AddSingleton<ITenantSQLiteMigrationManager, TenantSQLiteMigrationManager>();
                    services.AddSingleton<ITenantSQLiteSelectionManager, TenantSQLiteSelectionManager>();


                    //Tenant Database Services                    
                    
                    services.AddSingleton<ITenantSQLiteInfoService, TenantSQLiteInfoService>();
                    services.AddSingleton<ITenantSQLiteSelectionService, TenantSQLiteSelectionService>();
                    services.AddSingleton<ITenantSQLiteWorkflowService, TenantSQLiteWorkflowService>();
                    services.AddSingleton<ITenantSQLiteDatabaseOperationService, TenantSQLiteDatabaseOperationService>();
                    services.AddSingleton<ITenantSQLiteDatabaseLifecycleService, TenantSQLiteDatabaseLifecycleService>();


                    //Database Infrastructure Services
                    services.AddSingleton<IEnvironmentDetector, EnvironmentDetector>();
                    services.AddSingleton<IApplicationPaths, ApplicationPaths>();
                    services.AddSingleton<IDatabaseNamingService, DatabaseNamingService>();
                });
            return host;
        }
    }
}
