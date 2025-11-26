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
    public class TenantWorkflowService : ITenantWorkflowService
    {
        public ITenantDatabaseLifecycleService _lifecycleService { get; }
        private readonly ITenantDatabaseOperationService _operationService;
        private readonly IFirmaService _firmaService;
        private readonly IMaliDonemService _maliDonemService;        
        private readonly IApplicationPaths _applicationPaths;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly ILogService _logService;
        private readonly ILogger<TenantWorkflowService> _logger;

        public TenantWorkflowService(
            ITenantDatabaseLifecycleService lifecycleService,
            ITenantDatabaseOperationService operationService,
            IFirmaService firmaService,
            IMaliDonemService maliDonemService,            
            IApplicationPaths applicationPaths,
            IUnitOfWork<SistemDbContext> unitOfWork,
            ILogService logService,
            ILogger<TenantWorkflowService> logger)
        {
            _lifecycleService = lifecycleService;
            _operationService = operationService;
            _firmaService = firmaService;
            _maliDonemService = maliDonemService;            
            _applicationPaths = applicationPaths;
            _unitOfWork = unitOfWork;
            _logService = logService;
            _logger = logger;
        }

        public async Task<ApiDataResponse<TenantCreationResult>> CreateNewTenantAsync(TenantCreationRequest request)
        {
            _logger.LogInformation(
                "Tenant oluşturma başlatıldı - FirmaId: {FirmaId}, MaliYil: {MaliYil}",
                request.FirmaId,
                request.MaliYil);

            var result = new TenantCreationResult();
            var saga = new TenantCreationSaga(_logger);

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

                var dbNameResponse = _lifecycleService.GenerateDatabaseNameAsync(firma.FirmaKodu, request.MaliYil);
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
                var databasePath = Path.Combine(
                    _applicationPaths.GetDatabasePath(),
                    "Muhasebe",
                    result.DatabaseName);

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
                    Directory = Path.GetDirectoryName(databasePath),
                    DBPath = databasePath,
                    DatabaseType = DatabaseType.SqlServer,
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
                // STEP 6: Migration Çalıştır
                // ============================================
                if (request.RunMigrations && result.DatabaseCreated)
                {
                    _logger.LogInformation("Step 6/7: Migration çalıştırılıyor...");

                    await saga.ExecuteStepAsync(
                        stepName: "RunMigrations",
                        action: async () =>
                        {
                            var migrationResponse = await _operationService.RunMigrationsAsync(maliDonem.Id);
                            if (!migrationResponse.Success)
                            {
                                throw new InvalidOperationException(
                                    $"Migration başarısız: {migrationResponse.Message}");
                            }
                            result.MigrationsRun = true;
                            _logger.LogInformation("Migration tamamlandı");
                            return true;
                        },
                        compensate: null // Database zaten silinecek, ayrı compensate gerekmez
                    );
                }
                else
                {
                    _logger.LogInformation("Step 6/7: Migration atlandı");
                }

                // ============================================
                // STEP 7: MaliDonem Güncelle (DB Flag)
                // ============================================
                _logger.LogInformation("Step 7/7: MaliDonem kaydı güncelleniyor...");

                using (var updateTransaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {                        
                        await _maliDonemService.UpdateMaliDonemAsync(maliDonem);
                        await _unitOfWork.SaveChangesAsync();
                        await updateTransaction.CommitAsync();
                        _logger.LogInformation("MaliDonem güncellendi");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "MaliDonem güncelleme hatası");
                        await updateTransaction.RollbackAsync();
                        throw new InvalidOperationException($"MaliDonem güncellenemedi: {ex.Message}", ex);
                    }
                }

                // ============================================
                // BONUS: İsteğe Bağlı Backup
                // ============================================
                if (request.CreateInitialBackup && result.MigrationsRun)
                {
                    _logger.LogInformation("Initial backup oluşturuluyor...");
                    try
                    {
                        await _operationService.CreateBackupAsync(maliDonem.Id);
                        _logger.LogInformation("Initial backup oluşturuldu");
                    }
                    catch (Exception backupEx)
                    {
                        _logger.LogWarning(backupEx, "Initial backup oluşturulamadı (kritik değil)");
                    }
                }

                // ============================================
                // BAŞARILI SONUÇ
                // ============================================
                result.Message = "Tenant başarıyla oluşturuldu";

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantWorkflowService),
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
                    nameof(TenantWorkflowService),
                    nameof(CreateNewTenantAsync),
                    ex);

                return new ErrorApiDataResponse<TenantCreationResult>(
                    result,
                    $"Tenant oluşturma hatası: {ex.Message}",
                    false,
                    ResultCodes.HATA_Olusturulamadi);
            }
        }



        public async Task<ApiDataResponse<bool>> DeleteTenantCompleteAsync(long maliDonemId)
        {
            _logger.LogInformation("Starting complete tenant deletion for MaliDonemId: {MaliDonemId}", maliDonemId);

            try
            {
                // 1. MaliDonemDb bilgisini al
                var maliDonemDb = await _maliDonemService.GetByMaliDonemIdAsync(maliDonemId);
                if (maliDonemDb == null)
                {
                    return new ErrorApiDataResponse<bool>(false, "Mali dönem veritabanı kaydı bulunamadı");
                }
                var databaseName = maliDonemDb.Data.DBName;

                // 2. SQL Server database'i sil
                var dbDeleteResponse = await _lifecycleService.DeleteDatabaseAsync(databaseName);
                if (!dbDeleteResponse.Success)
                {
                    _logger.LogWarning("Database deletion failed but continuing: {DatabaseName}", databaseName);
                }
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                // 3. MaliDonemDb kaydını sil (SQLite)
                await _maliDonemService.DeleteMaliDonemAsync(maliDonemDb.Data);


                // 4. MaliDonem kaydını sil (SQLite) - Cascade delete olmalı
                var maliDonem = await _maliDonemService.GetByMaliDonemIdAsync(maliDonemId);
                if (maliDonem != null)
                {
                    await _maliDonemService.DeleteMaliDonemAsync(maliDonem.Data);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(TenantWorkflowService),
                        nameof(DeleteTenantCompleteAsync),
                        $"Tenant tamamen silindi. MaliDonemId: {maliDonemId}, Database: {databaseName}",
string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Tenant başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Complete tenant deletion failed for MaliDonemId: {MaliDonemId}", maliDonemId);

                await _logService.SistemLogService
                    .SistemLogException(nameof(TenantWorkflowService), nameof(DeleteTenantCompleteAsync), ex);

                return new ErrorApiDataResponse<bool>(false, $"Tenant silme hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<bool>> PrepareTenantForFirstUseAsync(long maliDonemId)
        {
            try
            {
                _logger.LogInformation("Preparing tenant for first use. MaliDonemId: {MaliDonemId}", maliDonemId);

                // 1. Database hazırla (migration check)
                var prepareResponse = await _operationService.PrepareDatabaseAsync(maliDonemId);
                if (!prepareResponse.Success)
                {
                    return new ErrorApiDataResponse<bool>(false, $"Veritabanı hazırlanamadı: {prepareResponse.Message}");
                }

                // 2. Health check
                var healthResponse = await _operationService.GetHealthStatusAsync(maliDonemId);
                if (!healthResponse.Success || !healthResponse.Data.CanConnect)
                {
                    return new ErrorApiDataResponse<bool>(false, "Veritabanı sağlık kontrolü başarısız");
                }

                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(TenantWorkflowService),
                        nameof(PrepareTenantForFirstUseAsync),
                        $"Tenant ilk kullanıma hazırlandı. MaliDonemId: {maliDonemId}",
string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Tenant başarıyla hazırlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Prepare tenant for first use failed for MaliDonemId: {MaliDonemId}", maliDonemId);

                return new ErrorApiDataResponse<bool>(false, ex.Message);
            }
        }
    }
}
