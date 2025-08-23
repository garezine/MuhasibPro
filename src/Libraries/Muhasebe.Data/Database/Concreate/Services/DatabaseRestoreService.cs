using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Configurations;
using Muhasebe.Data.Database.Interfaces.Operations;
using Muhasebe.Data.Database.Interfaces.Provider;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Data.Database.Utilities;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.DataContext.DataContextFactory;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Utilities.IDGenerator;

namespace Muhasebe.Data.Database.Concreate.Services
{
    // DatabaseRestoreService'in SQLite desteği için güncellenmesi gerekiyor.
    // IDatabaseRestoreOperations'ın hem SQL Server hem de SQLite için implementasyonları olmalı.
    public class DatabaseRestoreService : IDatabaseRestoreService
    {
        private readonly AppSistemDbContext _sistemContext;
        private readonly IAppDbContextFactory _dbContextFactory;
        private readonly Func<DatabaseType, IDatabaseRestoreOperations> _restoreOperationsFactory; // Factory pattern daha uygun olabilir
        private readonly IDatabaseProviderFactory _providerFactory; // DB Path oluşturmak için Provider'a ihtiyaç var
        private readonly IDatabaseDirectoryManager _directoryManager; // Dizin işlemleri için
        private readonly ILogger<DatabaseRestoreService> _logger;
        private readonly IDatabaseConfiguration _dbConfig;
        // private readonly IConfiguration _configuration; // Master connection string artık operation içinde yönetilmeli

        // Eksik: IDatabaseRestoreOperations Interface ve Implementasyonları
        // public interface IDatabaseRestoreOperations { Task<RestoreResult> RestoreDatabaseAsync(string backupFilePath, string targetDatabaseName, string targetDbDirectory, string targetDbPath); }
        // public class SqlServerRestoreOperations : IDatabaseRestoreOperations { /* ... SQL RESTORE logic ... */ }
        // public class SQLiteRestoreOperations : IDatabaseRestoreOperations { /* ... File Copy logic ... */ }

        // Eksik: Helper sınıflar
        // public static class AppPaths { public static string GetDatabaseDirectory(string firmaKisaUnvan, long firmaId) => Path.Combine("C:", "EnvanterProData", $"Firma_{firmaId}_{firmaKisaUnvan}"); }
        // public static class UIDGenerator { public static long Next() => DateTime.UtcNow.Ticks; } // Örnek basit ID
        // public class RestoreResult { public bool Success { get; } public string ErrorMessage { get; } public RestoreResult(bool success, string message = null) { Success = success; ErrorMessage = message; } }

        public DatabaseRestoreService(
            AppSistemDbContext sistemContext,
            Func<DatabaseType, IDatabaseRestoreOperations> restoreOperationsFactory, // DI ile factory inject edilir
            IDatabaseProviderFactory providerFactory,
            IDatabaseDirectoryManager directoryManager,
            ILogger<DatabaseRestoreService> logger,
            IDatabaseConfiguration dbConfig,
            IAppDbContextFactory dbContextFactory)
        {
            _sistemContext = sistemContext;
            _restoreOperationsFactory = restoreOperationsFactory;
            _providerFactory = providerFactory;
            _directoryManager = directoryManager;
            _logger = logger;
            _dbConfig = dbConfig;
            _dbContextFactory = dbContextFactory;
        }

        // Restore edilecek veritabanının TÜRÜNÜ de bilmemiz gerekiyor.
        // Bu bilgi yedek dosyasından (örn. uzantı) çıkarılabilir veya parametre olarak alınabilir.
        public async Task<RestoreResult> RestoreDatabaseAsync(long firmaId, long donemId, string backupFilePath, DatabaseType targetDbType)
        {
            _logger.LogInformation("Starting restore process for FirmaId: {FirmaId}, DonemId: {DonemId}, Type: {DbType} from file: {BackupFilePath}", firmaId, donemId, targetDbType, backupFilePath);

            Firma firma = null;
            MaliDonem donem = null;
            string targetDatabaseName = null;
            string targetDbDirectory = null;
            string targetDbPath = null; // SQLite için tam dosya yolu
            IDatabaseRestoreOperations restoreOperations = null;
            IDatabaseProvider provider = null;

            try
            {
                // 0. Yedek dosyasının varlığını kontrol et
                if (!File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException("Backup file not found.", backupFilePath);
                }

                // 1. Gerekli Firma ve Dönem bilgilerini al
                firma = await _sistemContext.Firmalar.FindAsync(firmaId).ConfigureAwait(false) ??
                        throw new InvalidOperationException($"Restore için Firma bulunamadı: ID={firmaId}");

                donem = await _sistemContext.MaliDonemler
                    .FirstOrDefaultAsync(d => d.Id == donemId && d.FirmaId == firmaId)
                    .ConfigureAwait(false) ??
                        throw new InvalidOperationException($"Restore için Dönem bulunamadı: FirmaId={firmaId}, DonemId={donemId}");

                // 2. Hedef veritabanı adını, dizinini ve yolunu oluştur
                targetDatabaseName = DatabaseNameGenarator.GenerateDatabaseName(donem.Firma.KisaUnvani, donem.MaliDonemler);
                targetDbDirectory = AppPaths.GetDatabaseDirectory(firma.KisaUnvani, firmaId);
                _directoryManager.EnsureDirectoryExists(targetDbDirectory); // Dizin yoksa oluştur

                // DB Path'i provider kullanarak oluştur
                provider = _providerFactory.Create(targetDbType);
                // GenerateConnectionString'i sadece path üretmek için kullanıyoruz, gerçek bağlantı için değil.
                // Provider'ın GenerateDbPath gibi bir metodu olsaydı daha temiz olurdu.
                var tempConnectionString = provider.GenerateConnectionString(targetDatabaseName, targetDbDirectory);
                if (targetDbType == DatabaseType.SQLite)
                {
                    // SQLite için connection string "Data Source=..." şeklindedir.
                    targetDbPath = tempConnectionString.Split('=')[1];
                }
                else // SQL Server vb. için sadece DB adı yeterli olabilir, operasyon kendi bağlantısını kurar.
                {
                    targetDbPath = targetDatabaseName; // Veya null? Operasyona bağlı.
                }


                _logger.LogDebug("Target database name: {TargetDbName}, Target directory: {TargetDbDir}, Target Path: {TargetPath}", targetDatabaseName, targetDbDirectory, targetDbPath);

                // 3. Doğru restore operasyonunu seç
                restoreOperations = _restoreOperationsFactory(targetDbType);

                // 4. Veritabanı restore işlemini çağır
                // Artık master connection string göndermiyoruz. Operasyon kendi gereksinimini halleder.
                var restoreResult = await restoreOperations.RestoreDatabaseAsync(
                    backupFilePath,
                    targetDatabaseName, // SQL Server için gerekli olabilir
                    targetDbDirectory,  // SQL Server MDF/LDF yerini belirtmek için gerekli olabilir
                    targetDbPath)       // SQLite için hedef dosya yolu
                    .ConfigureAwait(false);

                if (!restoreResult.Success)
                {
                    _logger.LogError("Database restore operation failed for {TargetDatabaseName}. Error: {ErrorMessage}", targetDatabaseName, restoreResult.Message);
                    return restoreResult;
                }

                _logger.LogInformation("Database {TargetDatabaseName} restored successfully. Updating system records...", targetDatabaseName);

                // 5. Restore başarılıysa, sistem veritabanına CalismaDonemDb kaydını ekle/güncelle
                using var transaction = await _sistemContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    var existingDonemDb = await _sistemContext.DonemDBSecim
                        .FirstOrDefaultAsync(db => db.FirmaId == firmaId && db.MaliDonemId == donemId)
                        .ConfigureAwait(false);

                    if (existingDonemDb != null)
                    {
                        _logger.LogWarning("Existing CalismaDonemDb record found. Updating record.");
                        existingDonemDb.DBName = targetDatabaseName;
                        existingDonemDb.Directory = targetDbDirectory;
                        existingDonemDb.DBPath = targetDbPath; // DB Path eklendi
                        existingDonemDb.DatabaseType = targetDbType; // DB Type eklendi/güncellendi
                        existingDonemDb.AktifMi = true;
                        existingDonemDb.GuncellemeTarihi = DateTime.UtcNow;
                        // existingDonemDb.SonRestoreTarihi = DateTime.UtcNow; // Opsiyonel alan
                        _sistemContext.DonemDBSecim.Update(existingDonemDb);
                    }
                    else
                    {
                        _logger.LogInformation("Creating new CalismaDonemDb record.");

                        var donemDb = new DonemDBSec
                        {

                            Id = UIDModuleGenerator.GenerateModuleId(UIDModuleType.Veritabani), // UIDGenerator gerekli
                            KaydedenId = firma.KaydedenId,
                            FirmaId = firmaId,
                            MaliDonemId = donemId,
                            DBName = targetDatabaseName,
                            Directory = targetDbDirectory,
                            DBPath = targetDbPath, // DB Path eklendi
                            DatabaseType = targetDbType, // DB Type eklendi
                            KayitTarihi = DateTime.Now,
                            AktifMi = true,
                            // SonRestoreTarihi = DateTime.UtcNow // Opsiyonel alan
                        };
                        await _sistemContext.DonemDBSecim.AddAsync(donemDb).ConfigureAwait(false);
                    }

                    await _sistemContext.SaveChangesAsync().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                    _logger.LogInformation("System records updated successfully after restore.");

                    //Restore sonrası hemen şema güncellemesi yapmak iyi olabilir(opsiyonel)
                    try
                    {
                        _logger.LogInformation("Applying schema after restore...");
                        await _dbConfig.InitializeAsync(firmaId, donemId).ConfigureAwait(false); // State'i ayarla
                        await using var context = _dbContextFactory.CreateDbContext();
                        await provider.ApplySchemaAsync(context).ConfigureAwait(false); // Şemayı uygula/doğrula
                        _logger.LogInformation("Schema applied successfully after restore.");
                    }
                    catch (Exception schemaEx)
                    {
                        _logger.LogError(schemaEx, "Failed to apply schema after restore. Manual check might be needed.");
                        // Bu hatayı ana restore sonucuna ekleyebiliriz veya ayrı loglayabiliriz.
                    }
                    finally { _dbConfig.ResetState(); } // State'i temizle

                    return restoreResult;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating system records after successful restore for {TargetDatabaseName}. Rolling back system DB transaction.", targetDatabaseName);
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    // Restore başarılı oldu ama sistem kaydı başarısız oldu. Manuel müdahale gerekebilir.
                    return new RestoreResult(false, $"Veritabanı geri yüklendi ancak sistem kaydı güncellenirken hata oluştu: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error during restore process for FirmaId: {FirmaId}, DonemId: {DonemId}", firmaId, donemId);
                // Hata durumunda, eğer restore işlemi başladıysa ve kısmen tamamlandıysa,
                // oluşturulan dosyaları/DB'yi temizlemek gerekebilir (CleanupDatabaseAsync).
                // Bu, IDatabaseRestoreOperations içinde veya burada yapılabilir.
                if (restoreOperations != null && targetDbPath != null)
                {
                    try { await provider?.CleanupDatabaseAsync(targetDbPath, targetDbDirectory); } // Provider null kontrolü
                    catch (Exception cleanupEx) { _logger.LogError(cleanupEx, "Failed to cleanup partially restored database artifacts."); }
                }
                if (targetDbDirectory != null)
                {
                    try { _directoryManager.CleanupDirectory(targetDbDirectory); } // Boş dizini temizle? Veya sadece logla?
                    catch (Exception cleanupEx) { _logger.LogError(cleanupEx, "Failed to cleanup target directory."); }
                }

                return new RestoreResult(false, $"Restore işlemi sırasında genel hata: {ex.Message}");
            }
        }

        public void RestoreUpdateSistemDatabase()
        {
            using var context = _sistemContext;
            context.Database.Migrate();
        }
    }
}
