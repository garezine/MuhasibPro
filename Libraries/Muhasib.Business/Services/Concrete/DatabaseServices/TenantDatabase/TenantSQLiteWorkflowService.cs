using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Models.TenantModel;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Business.Services.Contracts.SistemServices;
using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantSQLiteWorkflowService : ITenantSQLiteWorkflowService
    {
        private readonly ITenantSQLiteDatabaseLifecycleService _lifecycleService;
        private readonly ITenantSQLiteDatabaseOperationService _operationService;
        private readonly ITenantSQLiteInfoService _selectionService;
        private readonly ITenantSQLiteConnectionService _connectionService;
        private readonly IFirmaService _firmaService;
        private readonly IMaliDonemService _maliDonemService;
        private readonly IApplicationPaths _applicationPaths;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly ILogService _logService;
        private readonly ILogger<TenantSQLiteWorkflowService> _logger;

        public TenantSQLiteWorkflowService(
            ITenantSQLiteDatabaseLifecycleService lifecycleService,
            ITenantSQLiteDatabaseOperationService operationService,
            ITenantSQLiteInfoService selectionService,
            ITenantSQLiteConnectionService connectionService,
            IFirmaService firmaService,
            IMaliDonemService maliDonemService,
            IApplicationPaths applicationPaths,
            IUnitOfWork<SistemDbContext> unitOfWork,
            ILogService logService,
            ILogger<TenantSQLiteWorkflowService> logger)
        {
            _lifecycleService = lifecycleService;
            _operationService = operationService;
            _selectionService = selectionService;
            _firmaService = firmaService;
            _maliDonemService = maliDonemService;
            _applicationPaths = applicationPaths;
            _unitOfWork = unitOfWork;
            _logService = logService;
            _logger = logger;
            _connectionService = connectionService;
        }

        public async Task<ApiDataResponse<TenantCreationResult>> CreateNewTenantAsync(TenantCreationRequest request)
        {
            _logger.LogInformation(
                "Tenant oluşturma başlatıldı - FirmaId: {FirmaId}, MaliYil: {MaliYil}",
                request.FirmaId,
                request.MaliYil);

            var result = new TenantCreationResult();
            var saga = new TenantOperationSaga(_logger);

            try
            {
                // ============================================
                // STEP 1: Firma Kontrolü
                // ============================================
                _logger.LogInformation("Step 1/7: Firma kontrol ediliyor...");

                var firmaResponse = await _firmaService.GetByFirmaIdAsync(request.FirmaId);
                if (!firmaResponse.Success || firmaResponse.Data == null)
                {
                    _logger.LogWarning("Firma bulunamadı: {FirmaId}", request.FirmaId);
                    return new ErrorApiDataResponse<TenantCreationResult>(
                        null,
                        firmaResponse.Message ?? $"Firma bulunamadı (FirmaId: {request.FirmaId})",
                        false,
                        ResultCodes.HATA_Bulunamadi);
                }
                var firma = firmaResponse.Data;
                _logger.LogInformation("Firma bulundu: {FirmaKodu}", firma.FirmaKodu);

                // ============================================
                // STEP 2: Duplicate Mali Dönem Kontrolü
                // ============================================
                _logger.LogInformation("Step 2/7: Mali dönem kontrolü yapılıyor...");

                var existingDonem = await _maliDonemService.IsMaliDonem(request.FirmaId, request.MaliYil);
                if (existingDonem)
                {
                    _logger.LogWarning("Mali dönem zaten mevcut: {FirmaId}-{MaliYil}", request.FirmaId, request.MaliYil);
                    return new ErrorApiDataResponse<TenantCreationResult>(
                        null,
                        $"Bu firma için {request.MaliYil} mali dönemi zaten mevcut",
                        false,
                        ResultCodes.HATA_ZatenVar);
                }
                _logger.LogInformation("Mali dönem mevcut değil, devam ediliyor");

                // ============================================
                // STEP 3: Database Adı Oluştur
                // ============================================
                _logger.LogInformation(" Step 3/7: Database adı oluşturuluyor...");

                var dbNameResponse = _lifecycleService.GenerateDatabaseName(firma.FirmaKodu, request.MaliYil);
                if (!dbNameResponse.Success || string.IsNullOrEmpty(dbNameResponse.Data))
                {
                    _logger.LogError("Database adı oluşturulamadı");
                    return new ErrorApiDataResponse<TenantCreationResult>(
                        null,
                        dbNameResponse.Message ?? "Database adı oluşturulamadı",
                        false,
                        ResultCodes.HATA_Olusturulamadi);
                }

                result.DatabaseName = dbNameResponse.Data;
                var databasePath = _applicationPaths.GetTenantDatabaseFilePath(result.DatabaseName);

                _logger.LogInformation("Database adı oluşturuldu: {DatabaseName}", result.DatabaseName);

                // ============================================
                // STEP 4: MaliDonem Kaydı Oluştur
                // ============================================
                _logger.LogInformation("Step 4/7: MaliDonem kaydı oluşturuluyor...");

                var maliDonem = new MaliDonemModel
                {
                    FirmaId = request.FirmaId,
                    MaliYil = request.MaliYil,
                    DBName = result.DatabaseName,
                    Directory = _applicationPaths.GetTenantDatabasesFolderPath(),
                    DBPath = databasePath,
                    DatabaseType = DatabaseType.SQLite,
                    AktifMi = true
                };

                // ✅ DÜZELTİLDİ: Saga step içinde transaction kullan
                await saga.ExecuteStepAsync(
                    stepName: "CreateMaliDonem",
                    action: async () =>
                    {
                        using (var transaction = await _unitOfWork.BeginTransactionAsync())
                        {
                            try
                            {
                                await _maliDonemService.UpdateMaliDonemAsync(maliDonem);
                                await _unitOfWork.SaveChangesAsync();
                                await transaction.CommitAsync();

                                result.MaliDonemId = maliDonem.Id;
                                _logger.LogInformation("MaliDonem kaydı oluşturuldu: {MaliDonemId}", maliDonem.Id);
                                return maliDonem.Id;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "MaliDonem oluşturma hatası");
                                await transaction.RollbackAsync();
                                throw new InvalidOperationException($"MaliDonem oluşturulamadı: {ex.Message}", ex);
                            }
                        }
                    },
                    compensate: async (maliDonemId) =>
                    {
                        _logger.LogWarning("Rollback: MaliDonem kaydı siliniyor: {MaliDonemId}", maliDonemId);
                        try
                        {
                            using (var transaction = await _unitOfWork.BeginTransactionAsync())
                            {
                                var deleteMaliDonem = new MaliDonemModel { Id = maliDonemId };
                                await _maliDonemService.DeleteMaliDonemAsync(deleteMaliDonem);
                                await _unitOfWork.SaveChangesAsync();
                                await transaction.CommitAsync();
                                _logger.LogInformation("MaliDonem silindi: {MaliDonemId}", maliDonemId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "MaliDonem silme hatası: {MaliDonemId}", maliDonemId);
                            // Compensate hatası fırlatmıyoruz, sadece logluyoruz
                        }
                    }
                );

                // ============================================
                // STEP 5: SQL Server Database Oluştur
                // ============================================
                if (request.AutoCreateDatabase)
                {
                    _logger.LogInformation("Step 5/7: Database oluşturuluyor: {DatabaseName}", result.DatabaseName);

                    await saga.ExecuteStepAsync(
                        stepName: "CreateDatabase",
                        action: async () =>
                        {
                            var dbCreateResponse = await _lifecycleService.CreateDatabaseAsync(result.DatabaseName);
                            if (!dbCreateResponse.Success)
                            {
                                throw new InvalidOperationException(
                                    $"Database oluşturulamadı: {dbCreateResponse.Message}");
                            }
                            result.DatabaseCreated = true;
                            _logger.LogInformation("Database oluşturuldu: {DatabaseName}", result.DatabaseName);
                            return result.DatabaseName;
                        },
                        compensate: async (dbName) =>
                        {
                            _logger.LogWarning("Rollback: Database siliniyor: {DatabaseName}", dbName);
                            try
                            {
                                var deleteResponse = await _lifecycleService.DeleteDatabaseAsync(dbName);
                                if (deleteResponse.Success)
                                {
                                    _logger.LogInformation("Database silindi: {DatabaseName}", dbName);
                                }
                                else
                                {
                                    _logger.LogWarning("Database silinemedi: {DatabaseName} - {Message}", dbName, deleteResponse.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Database silme hatası: {DatabaseName}", dbName);
                            }
                        }
                    );
                }
                else
                {
                    _logger.LogInformation("Step 5/7: Database oluşturma atlandı (AutoCreateDatabase=false)");
                }                

                // ============================================
                // BAŞARILI SONUÇ
                // ============================================
                result.Message = "Tenant başarıyla oluşturuldu";

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantSQLiteWorkflowService),
                    nameof(CreateNewTenantAsync),
                    $"Yeni tenant oluşturuldu. Firma: {firma.FirmaKodu}, Yıl: {request.MaliYil}, DB: {result.DatabaseName}, MaliDonemId: {result.MaliDonemId}",
                    string.Empty);

                _logger.LogInformation(
                    "Tenant başarıyla oluşturuldu - DatabaseName: {DatabaseName}, MaliDonemId: {MaliDonemId}",
                    result.DatabaseName,
                    result.MaliDonemId);

                return new SuccessApiDataResponse<TenantCreationResult>(
                    result,
                    "Tenant başarıyla oluşturuldu",
                    true,
                    ResultCodes.BASARILI_Olusturuldu,
                    1);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Tenant oluşturma BAŞARISIZ - FirmaId: {FirmaId}, MaliYil: {MaliYil}",
                    request.FirmaId,
                    request.MaliYil);

                // ============================================
                // SAGA ROLLBACK - Tüm İşlemleri Geri Al
                // ============================================
                _logger.LogWarning("Saga rollback başlatılıyor...");
                try
                {
                    await saga.CompensateAllAsync();
                    _logger.LogInformation("Saga rollback tamamlandı");
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Saga rollback sırasında hata oluştu! Manuel müdahale gerekebilir!");
                }

                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantSQLiteWorkflowService),
                    nameof(CreateNewTenantAsync),
                    ex);

                return new ErrorApiDataResponse<TenantCreationResult>(
                    result,
                    $"Tenant oluşturma hatası: {ex.Message}",
                    false,
                    ResultCodes.HATA_Olusturulamadi);
            }
        }

        public async Task<ApiDataResponse<TenantDeletingResult>> DeleteTenantCompleteAsync(TenantDeletingRequest request)
        {
            _logger.LogInformation("Tenant silme başlatıldı - MaliDonemId: {MaliDonemId}", request.MaliDonemId);

            var result = new TenantDeletingResult();
            var saga = new TenantOperationSaga(_logger);

            try
            {
                // Step 1: Tenant Veritabanı (Mali dönem) kontrolü
                var tenantResponse = await _selectionService.GetTenantDetailsAsync(request.MaliDonemId);
                if (!tenantResponse.Success || tenantResponse.Data == null)
                {
                    return new ErrorApiDataResponse<TenantDeletingResult>(
                        null,
                        $"Mali Dönem bulunamadı (MaliDonemId: {request.MaliDonemId})",
                        false,
                        ResultCodes.HATA_Bulunamadi);
                }

                var tenantDetails = tenantResponse.Data;
                result.DatabaseName = tenantDetails.DatabaseName;
                result.MaliDonemId = tenantDetails.MaliDonemId;
                result.DatabaseFilePath = tenantResponse.Data.DatabasePath;

                // ✅ Aktif tenant kontrolü
                bool isCurrentTenantDeleting = false;

                if (_connectionService.IsConnected)
                {
                    var currentTenantResponse = _connectionService.GetCurrentTenant();
                    if (currentTenantResponse.Success && currentTenantResponse.Data != null)
                    {
                        isCurrentTenantDeleting = currentTenantResponse.Data.DatabaseName == tenantDetails.DatabaseName;

                        _logger.LogInformation(
                            "Aktif tenant kontrolü: Silinen={TenantToDelete}, Aktif={CurrentTenant}, Sonuç={IsCurrent}",
                            tenantDetails.DatabaseName,
                            currentTenantResponse.Data.DatabaseName,
                            isCurrentTenantDeleting);
                    }
                }
                else
                {
                    _logger.LogInformation("Aktif tenant bağlantısı yok, normal silme işlemi devam ediyor");
                }

                // Step 2: Veritabanı silme (önce)
                if (request.IsDeleteDatabase)
                {
                    await saga.ExecuteStepAsync(
                        stepName: "DatabaseSil",
                        action: async () =>
                        {
                            // ✅ Aktif tenant siliniyorsa backup al
                            if (isCurrentTenantDeleting)
                            {
                                var sourceDbPath = _applicationPaths.GetTenantDatabaseFilePath(tenantDetails.DatabaseName);
                                if (File.Exists(sourceDbPath))
                                {
                                    var backupPath = _applicationPaths.GetTenantBackupFolderPath();
                                    var backupFileName = $"safety_{tenantDetails.DatabaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                                    var backupFilePath = Path.Combine(backupPath, backupFileName);

                                    SqliteConnection.ClearAllPools();
                                    await Task.Delay(50);

                                    await SafeFileCopyAsync(sourceDbPath, backupFilePath);
                                    request.BackupFilePath = backupFilePath;

                                    _logger.LogInformation("Aktif tenant silinmeden önce backup alındı: {BackupPath}", backupFilePath);
                                }
                            }

                            var dbDeleteResponse = await _lifecycleService.DeleteDatabaseAsync(tenantDetails.DatabaseName);
                            if (!dbDeleteResponse.Success)
                            {
                                throw new InvalidOperationException($"Database silinemedi: {dbDeleteResponse.Message}");
                            }

                            result.DatabaseDeleted = true;
                            _logger.LogInformation("Veritabanı silindi: {DatabaseName}", tenantDetails.DatabaseName);

                            return tenantDetails.DatabaseName;
                        },
                        compensate: async (dbName) =>
                        {
                            if (!string.IsNullOrEmpty(request.BackupFilePath) && File.Exists(request.BackupFilePath))
                            {
                                _logger.LogWarning("Rollback: Database geri yükleniyor: {DatabaseName}", dbName);
                                try
                                {
                                    var restoreResponse = await _operationService.RestoreBackupAsync(
                                        tenantDetails.DatabaseName,
                                        request.BackupFilePath);

                                    if (restoreResponse.Success)
                                    {
                                        _logger.LogInformation("Database geri yüklendi: {DatabaseName}", dbName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Database restore hatası: {DatabaseName}", dbName);
                                }
                            }
                        }
                    );
                }

                // Step 3: MaliDonem kaydını sil (sonra)
                await saga.ExecuteStepAsync(
                    stepName: "MaliDonemSil",
                    action: async () =>
                    {
                        using (var transaction = await _unitOfWork.BeginTransactionAsync())
                        {
                            try
                            {
                                var deleteMaliDonem = new MaliDonemModel { Id = tenantDetails.MaliDonemId };
                                await _maliDonemService.DeleteMaliDonemAsync(deleteMaliDonem);
                                await _unitOfWork.SaveChangesAsync();
                                await transaction.CommitAsync();

                                _logger.LogInformation("MaliDonem kaydı silindi: {MaliDonemId}", tenantDetails.MaliDonemId);
                                return tenantDetails.MaliDonemId;
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                throw new InvalidOperationException($"MaliDonem silinemedi: {ex.Message}", ex);
                            }
                        }
                    },
                    compensate: async (maliDonemId) =>
                    {
                        _logger.LogWarning("Rollback: MaliDonem kaydı yeniden oluşturuluyor: {MaliDonemId}", maliDonemId);
                        try
                        {
                            using (var transaction = await _unitOfWork.BeginTransactionAsync())
                            {
                                var maliDonem = new MaliDonemModel
                                {
                                    FirmaId = tenantDetails.FirmaId,
                                    MaliYil = tenantDetails.MaliYil,
                                    DBName = tenantDetails.DatabaseName,
                                    Directory = tenantDetails.Directory,
                                    DBPath = tenantDetails.DatabasePath,
                                    DatabaseType = DatabaseType.SQLite,
                                    AktifMi = true
                                };
                                await _maliDonemService.UpdateMaliDonemAsync(maliDonem);
                                await _unitOfWork.SaveChangesAsync();
                                await transaction.CommitAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "MaliDonem restore hatası: {MaliDonemId}", maliDonemId);
                        }
                    }
                );

                // ✅ KRİTİK: Aktif tenant silindiyse bağlantıyı kes
                if (isCurrentTenantDeleting)
                {
                    _connectionService.ClearCurrentTenant();
                    _logger.LogWarning("Aktif tenant silindi, bağlantı kesildi: {DatabaseName}", tenantDetails.DatabaseName);
                }

                result.Message = "Tenant başarıyla silindi";

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantSQLiteWorkflowService),
                    nameof(DeleteTenantCompleteAsync),
                    $"Tenant silindi. DB: {tenantDetails.DatabaseName}, MaliDonemId: {tenantDetails.MaliDonemId}",
                    string.Empty);
                if (!string.IsNullOrEmpty(request.BackupFilePath))
                {
                    await saga.ExecuteStepAsync(
                        stepName: "BackupTemizle",
                        action: async () =>
                        {
                            await CleanupBackupFileAsync(request.BackupFilePath);
                            return request.BackupFilePath;
                        },
                        compensate: null // Geri alınacak bir şey yok
                    );
                }
                return new SuccessApiDataResponse<TenantDeletingResult>(
                    result,
                    "Tenant başarıyla silindi",
                    true,
                    ResultCodes.BASARILI_Silindi,
                    1);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tenant silme başarısız - MaliDonemId: {MaliDonemId}", request.MaliDonemId);

                await saga.CompensateAllAsync();

                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantSQLiteWorkflowService),
                    nameof(DeleteTenantCompleteAsync),
                    ex);

                return new ErrorApiDataResponse<TenantDeletingResult>(
                    result,
                    $"Tenant silme hatası: {ex.Message}",
                    false,
                    ResultCodes.HATA_Silinemedi);
            }
        }

     
        private async Task SafeFileCopyAsync(string source, string dest)
        {
            SqliteConnection.ClearAllPools();
            await Task.Delay(100);

            using (var sourceStream = new FileStream(source, FileMode.Open,
                   FileAccess.Read, FileShare.ReadWrite))
            using (var destStream = new FileStream(dest, FileMode.Create))
            {
                await sourceStream.CopyToAsync(destStream);
            }
        }
        private async Task CleanupBackupFileAsync(string backupFilePath)
        {
            if (string.IsNullOrEmpty(backupFilePath) || !File.Exists(backupFilePath))
            {
                return;
            }

            try
            {
                // 3 defa deneyelim (file lock olabilir)
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        File.Delete(backupFilePath);
                        _logger.LogInformation("Backup dosyası silindi: {BackupPath}", backupFilePath);
                        return;
                    }
                    catch (IOException ioEx) when (i < 2) // Son deneme değilse
                    {
                        _logger.LogDebug("Backup dosyası silinemedi, tekrar denenecek ({Attempt}/3): {Error}",
                            i + 1, ioEx.Message);
                        await Task.Delay(100 * (i + 1)); // Artan gecikme
                    }
                }

                _logger.LogWarning("Backup dosyası silinemedi (son deneme): {BackupPath}", backupFilePath);
            }
            catch (Exception ex)
            {
                // Kritik olmayan hata, sadece logla
                _logger.LogWarning(ex, "Backup dosyası temizleme hatası: {BackupPath}", backupFilePath);
            }
        }
    }

}
