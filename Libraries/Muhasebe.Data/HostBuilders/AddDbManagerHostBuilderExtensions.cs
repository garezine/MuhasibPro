﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Data.Database.SistemDatabase;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.DataContext.DesignTimeFactory;
using Muhasebe.Data.Helper;

namespace Muhasebe.Data.HostBuilders
{
    public static class AddDbManagerHostBuilderExtensions
    {
        public static IHostBuilder AddDatabaseManager(this IHostBuilder host)
        {
            host.ConfigureServices(
                (context, services) =>
                {
                    // --- Sistem Veritabanı ---
                    services.AddDbContext<AppSistemDbContext>(
                        option =>
                        {
                            var dbPath = ConfigurationHelper.Instance.GetDatabasePath();
                            var fullDbPath = Path.Combine(dbPath, "Sistem.db");
                            var connectionString = $"Data Source={fullDbPath};Mode=ReadWriteCreate;";

                            option.UseSqlite(connectionString);
#if DEBUG
                            option.EnableSensitiveDataLogging();
                            option.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
#endif
                        });

                    services.AddScoped<ISistemDatabaseManager, SistemDatabaseManager>();
                    

                    //    services.AddScoped<AppDbContext>(provider =>
                    //    {
                    //        var factory = provider.GetRequiredService<IAppDbContextFactory>();
                    //        return factory.CreateDbContext();
                    //    });
                    //    // --- Temel Veritabanı Bileşenleri (Scoped) ---
                    //    services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();
                    //    services.AddScoped<IDatabaseConfiguration, DatabaseConfiguration>();
                    //    services.AddScoped<IDatabaseProviderFactory, DatabaseProviderFactory>();
                    //    services.AddScoped<IDatabaseDirectoryManager, DatabaseDirectoryManager>();

                    //    // --- Business Servisleri (Scoped) ---
                    //    // Veritabanı Yönetim Servisleri
                    //    services.AddScoped<IDatabaseCreationService, DatabaseCreationService>();
                    //    services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
                    //    services.AddScoped<IDatabaseRestoreService, DatabaseRestoreService>();
                    //    services.AddScoped<IDatabaseMaintenanceService, DatabaseMaintenanceService>();
                    //    services.AddScoped<IDatabaseDeletionService, DatabaseDeletionService>();
                    //    services.AddScoped<IDatabaseBackupCleanupService, DatabaseBackupCleanupService>(); // Eğer varsa
                    //    services.AddScoped<IDatabaseSelectionService, DatabaseSelectionService>();


                    //    // --- Operasyon Sınıflarının Kaydı (Transient) ---
                    //    // Factory'lerin çözebilmesi için somut sınıflar kaydedilir.
                    //    // Not: Bu sınıfların constructor'ları ILogger<T> gibi bağımlılıklar alıyorsa,
                    //    // DI container bunları otomatik olarak inject edecektir.
                    //    services.AddTransient<SqlServerProvider>(); // Provider'ları da kaydetmek iyi olabilir
                    //    services.AddTransient<SQLiteProvider>();    // Provider'ları da kaydetmek iyi olabilir


                    //    services.AddTransient<SqlServerBackupOperations>();
                    //    services.AddTransient<SQLiteBackupOperations>();
                    //    services.AddTransient<SqlServerRestoreOperations>();
                    //    services.AddTransient<SQLiteRestoreOperations>();
                    //    services.AddTransient<SqlServerMaintenanceOperations>();
                    //    services.AddTransient<SQLiteMaintenanceOperations>();
                    //    services.AddTransient<SqlServerDeletionOperations>(); // Eğer ayrı bir sınıf varsa
                    //    services.AddTransient<SQLiteDeletionOperations>();    // Eğer ayrı bir sınıf varsa
                    //                                                          // Not: Deletion işlemleri IDatabaseProvider.CleanupDatabaseAsync'e taşındıysa
                    //                                                          // bu ayrı DeletionOperations sınıflarına gerek kalmayabilir.
                    //                                                          // Eğer hala kullanılıyorsa kayıtlar kalmalı.

                    //    // --- Operasyon Factory Kayıtları (Transient Func) ---
                    //    // Servislerin DatabaseType'a göre doğru operasyon sınıfını almasını sağlar.
                    //    services.AddTransient<Func<DatabaseType, IDatabaseBackupOperations>>(serviceProvider => key =>
                    //    {
                    //        return key switch
                    //        {
                    //            DatabaseType.SqlServer => serviceProvider.GetRequiredService<SqlServerBackupOperations>(),
                    //            DatabaseType.SQLite => serviceProvider.GetRequiredService<SQLiteBackupOperations>(),
                    //            _ => throw new KeyNotFoundException($"No implementation found for IDatabaseBackupOperations with key {key}")
                    //        };
                    //    });

                    //    services.AddTransient<Func<DatabaseType, IDatabaseRestoreOperations>>(serviceProvider => key =>
                    //    {
                    //        return key switch
                    //        {
                    //            DatabaseType.SqlServer => serviceProvider.GetRequiredService<SqlServerRestoreOperations>(),
                    //            DatabaseType.SQLite => serviceProvider.GetRequiredService<SQLiteRestoreOperations>(),
                    //            _ => throw new KeyNotFoundException($"No implementation found for IDatabaseRestoreOperations with key {key}")
                    //        };
                    //    });

                    //    services.AddTransient<Func<DatabaseType, IDatabaseMaintenanceOperations>>(serviceProvider => key =>
                    //    {
                    //        return key switch
                    //        {
                    //            DatabaseType.SqlServer => serviceProvider.GetRequiredService<SqlServerMaintenanceOperations>(),
                    //            DatabaseType.SQLite => serviceProvider.GetRequiredService<SQLiteMaintenanceOperations>(),
                    //            _ => throw new KeyNotFoundException($"No implementation found for IDatabaseMaintenanceOperations with key {key}")
                    //        };
                    //    });

                    //    // Eğer IDatabaseDeletionOperations hala kullanılıyorsa bu factory kalmalı:
                    //    services.AddTransient<Func<DatabaseType, IDatabaseDeletionOperations>>(serviceProvider => key =>
                    //    {
                    //        return key switch
                    //        {
                    //            // Eğer ayrı DeletionOperations sınıfları varsa:
                    //            DatabaseType.SqlServer => serviceProvider.GetRequiredService<SqlServerDeletionOperations>(),
                    //            DatabaseType.SQLite => serviceProvider.GetRequiredService<SQLiteDeletionOperations>(),
                    //            _ => throw new KeyNotFoundException($"No implementation found for IDatabaseDeletionOperations with key {key}")
                    //        };
                    //    });


                    //    // --- Diğer Uygulama Servisleri/Repositoryler (Gerekliyse) ---
                    //    services.AddScoped<IBackupScheduleRepository, BackupScheduleRepository>(); // Örnek
                    //    services.AddScoped<DatabaseScheduledBackupService>(); // Örnek

                    //    // --- Loglama ---
                    //    services.AddLogging(); // Zaten ekliydi, teyit edildi.
                });

            return host;
        }
    }
}
