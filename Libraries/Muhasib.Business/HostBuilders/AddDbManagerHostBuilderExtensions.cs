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
using Muhasib.Data.Managers.DatabaseManager.Concrete.TenantManagers;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;
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
                        options =>
                        {
                            var paths = services.BuildServiceProvider().GetRequiredService<IApplicationPaths>();
                            var dbPath = paths.GetDatabasePath();
                            var connectionString = $"Data Source={Path.Combine(dbPath, "Sistem.db")};Mode=ReadWriteCreate;";
                            options.UseSqlite(
                                connectionString,
                                sqliteOptions =>
                                {
                                    sqliteOptions.CommandTimeout(30);
                                });
                        });

                    services.AddScoped<AppDbContext>(factory =>
                    {
                        var tenantManager = factory.GetRequiredService<ITenantSelectionManager>();
                        var contextFactory = factory.GetRequiredService<IAppDbContextFactory>();

                        var currentTenant = tenantManager.GetCurrentTenant();

                        // 1. Kontrol: Aktif ve Yüklü bir Tenant var mı?
                        if (currentTenant == null || !currentTenant.IsLoaded || currentTenant.MaliDonemId <= 0)
                        {
                            // 2. Aktif Tenant yoksa: Boş bir DbContextOptions ile context oluştur.
                            // Bu, hiçbir veritabanına bağlanmayan, sadece Entity Framework'ün çalışabilmesi için 
                            // gerekli minimum yapılandırmayı içerir (Memory veya boşa ayarlanmış).

                            // Boş bir options objesi oluştur:
                            var emptyOptions = new DbContextOptionsBuilder<AppDbContext>()
                                .UseInMemoryDatabase(databaseName: "Tenant_NOT_SELECTED")
                                .Options;

                            // Loglama yaparak uyarı verebilirsiniz.
                            // factory.GetRequiredService<ILogger<AppDbContext>>().LogWarning("AppDbContext, aktif tenant olmadan oluşturuldu (In-Memory).");

                            return new AppDbContext(emptyOptions);
                        }

                        // 3. Aktif Tenant varsa: Tenant'a özel Context'i oluştur.
                        return contextFactory.CreateForTenant(currentTenant.MaliDonemId);
                    });

                    // Sistem Database Managers
                    services.AddSingleton<ISistemDatabaseManager, SistemDatabaseManager>();
                    services.AddSingleton<ISistemDatabaseService, SistemDatabaseService>();
                    services.AddSingleton<ISistemDatabaseUpdateService, SistemDatabaseUpdateService>();

                    // Sql Database Yöneticisi
                    // Factories
                    services.AddSingleton<IAppDbContextFactory, AppDbContextFactory>();
                    services.AddSingleton<ISqlConnectionStringFactory, SqlConnectionStringFactory>();


                    // Managers
                    services.AddSingleton<ITenantDbContextAccessor, TenantDbContextAccessor>();
                    services.AddSingleton<ITenantConnectionManager, TenantConnectionManager>();
                    services.AddSingleton<ITenantSelectionManager, TenantSelectionManager>();
                    services.AddSingleton<ITenantMigrationManager, TenantMigrationManager>();
                    services.AddSingleton<IAppSqlDatabaseManager, AppSqlDatabaseManager>();

                    services.AddScoped<ILocalUpdateManager, LocalUpdateManager>();

                    //Tenant Database Services                    
                    services.AddSingleton<ITenantDatabaseLifecycleService, TenantDatabaseLifecycleService>();
                    services.AddSingleton<ITenantDatabaseOperationService, TenantDatabaseOperationService>();
                    services.AddSingleton<ITenantConnectionService, TenantConnectionService>();
                    services.AddSingleton<ITenantSelectionService, TenantSelectionService>();
                    services.AddSingleton<ITenantWorkflowService, TenantWorkflowService>();

                    //Database Infrastructure Services
                    services.AddSingleton<IEnvironmentDetector, EnvironmentDetector>();
                    services.AddSingleton<IApplicationPaths, ApplicationPaths>();
                    services.AddSingleton<IDatabaseNamingService, DatabaseNamingService>();


                });
            return host;
        }
    }
}
