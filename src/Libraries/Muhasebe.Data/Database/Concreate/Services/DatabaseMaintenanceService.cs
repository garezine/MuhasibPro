using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Configurations;
using Muhasebe.Data.Database.Interfaces.Operations;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Concreate.Services
{
    public class DatabaseMaintenanceService : IDatabaseMaintenanceService
    {
        private readonly AppSistemDbContext _sistemContext;
        private readonly IDatabaseConfiguration _dbConfig;
        private readonly Func<DatabaseType, IDatabaseMaintenanceOperations> _maintenanceOperationsFactory; // Factory
        private readonly ILogger<DatabaseMaintenanceService> _logger;

        // Eksik: IDatabaseMaintenanceOperations Interface ve Implementasyonları
        // public interface IDatabaseMaintenanceOperations {
        //     Task CheckIntegrityAsync(string connectionString, string dbName, string dbPath);
        //     Task ReindexTablesAsync(string connectionString, string dbName, string dbPath);
        //     // Task ShrinkDatabaseAsync(string connectionString, string dbName, string dbPath); // vb.
        // }
        // public class SqlServerMaintenanceOperations : IDatabaseMaintenanceOperations { /* ... DBCC CHECKDB, ALTER INDEX ... */ }
        // public class SQLiteMaintenanceOperations : IDatabaseMaintenanceOperations { /* ... PRAGMA integrity_check, REINDEX, VACUUM ... */ }

        public DatabaseMaintenanceService(
            AppSistemDbContext sistemContext,
            IDatabaseConfiguration dbConfig,
            Func<DatabaseType, IDatabaseMaintenanceOperations> maintenanceOperationsFactory, // DI ile factory
            ILogger<DatabaseMaintenanceService> logger)
        {
            _sistemContext = sistemContext;
            _dbConfig = dbConfig;
            _maintenanceOperationsFactory = maintenanceOperationsFactory;
            _logger = logger;
        }

        public async Task CheckDatabaseIntegrityAsync(long firmaId, long donemId)
        {
            await ExecuteMaintenanceOperation(firmaId, donemId, "Integrity Check",
                (ops, connStr, dbName, dbPath) => ops.CheckIntegrityAsync(connStr, dbName, dbPath))
                .ConfigureAwait(false);
        }

        public async Task ReindexDatabaseAsync(long firmaId, long donemId)
        {
            await ExecuteMaintenanceOperation(firmaId, donemId, "Reindexing",
               (ops, connStr, dbName, dbPath) => ops.ReindexTablesAsync(connStr, dbName, dbPath))
               .ConfigureAwait(false);
        }

        // Shrink gibi ek operasyonlar için de benzer metotlar eklenebilir
        // public async Task ShrinkDatabaseAsync(long firmaId, long donemId)
        // {
        //     await ExecuteMaintenanceOperation(firmaId, donemId, "Shrinking",
        //        (ops, connStr, dbName, dbPath) => ops.ShrinkDatabaseAsync(connStr, dbName, dbPath))
        //        .ConfigureAwait(false);
        // }

        // Tekrarlanan kodu azaltmak için yardımcı metot
        private async Task ExecuteMaintenanceOperation(long firmaId, long donemId, string operationName,
            Func<IDatabaseMaintenanceOperations, string, string, string, Task> maintenanceAction)
        {
            _logger.LogInformation("Starting {OperationName} for FirmaId: {FirmaId}, DonemId: {DonemId}", operationName, firmaId, donemId);
            DonemDBSec donemDb = null;
            DatabaseType dbType;
            IDatabaseMaintenanceOperations maintenanceOps = null;

            try
            {
                // 1. İlgili veritabanı kaydını bul ve tipini öğren
                donemDb = await _sistemContext.DonemDBSecim
                    .AsNoTracking()
                    .FirstOrDefaultAsync(db => db.FirmaId == firmaId && db.MaliDonemId == donemId)
                    .ConfigureAwait(false) ??
                    throw new InvalidOperationException($"Maintenance için veritabanı kaydı bulunamadı: FirmaId={firmaId}, DonemId={donemId}");

                dbType = donemDb.DatabaseType; // CalismaDonemDb'den tipi al
                maintenanceOps = _maintenanceOperationsFactory(dbType); // Doğru operasyon implementasyonunu al

                // 2. Tenant veritabanı bağlantısını yapılandır ve al
                // InitializeAsync artık dbType parametresi almıyor.
                await _dbConfig.InitializeAsync(firmaId, donemId).ConfigureAwait(false);
                var connectionString = _dbConfig.GetConnectionString();
                var databaseName = donemDb.DBName;
                var databasePath = donemDb.DBPath; // DB Path de gönderelim, SQLite için gerekebilir

                _logger.LogDebug("Executing {OperationName} on database '{DatabaseName}' (Type: {DbType})...", operationName, databaseName, dbType);

                // 3. İlgili bakım operasyonunu çağır
                await maintenanceAction(maintenanceOps, connectionString, databaseName, databasePath).ConfigureAwait(false);

                _logger.LogInformation("{OperationName} completed successfully for database '{DatabaseName}'.", operationName, databaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during {OperationName} for FirmaId: {FirmaId}, DonemId: {DonemId}", operationName, firmaId, donemId);
                throw new Exception($"{operationName} işlemi sırasında hata oluştu: {ex.Message}", ex); // Hata yukarı fırlatılmalı
            }
            finally
            {
                // İşlem bittikten sonra connection string state'ini temizle
                _dbConfig.ResetState();
                _logger.LogDebug("Database configuration state reset after {OperationName}.", operationName);
            }
        }
    }
}
