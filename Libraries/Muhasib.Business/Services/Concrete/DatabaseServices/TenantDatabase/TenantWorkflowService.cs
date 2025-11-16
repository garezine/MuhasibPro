using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Models.TenantModel;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Entities.SistemEntity;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantWorkflowService : ITenantWorkflowService
    {
        private readonly ITenantDatabaseLifecycleService _lifecycleService;
        private readonly ITenantDatabaseOperationService _operationService;
        private readonly IFirmaRepository _firmaRepo;
        private readonly IMaliDonemRepository _maliDonemRepo;
        private readonly IMaliDonemDbRepository _maliDonemDbRepo;
        private readonly IApplicationPaths _applicationPaths;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly ILogService _logService;
        private readonly ILogger<TenantWorkflowService> _logger;

        public TenantWorkflowService(
            ITenantDatabaseLifecycleService lifecycleService,
            ITenantDatabaseOperationService operationService,
            IFirmaRepository firmaRepo,
            IMaliDonemRepository maliDonemRepo,
            IMaliDonemDbRepository maliDonemDbRepo,
            IApplicationPaths applicationPaths,
            IUnitOfWork<SistemDbContext> unitOfWork,
            ILogService logService,
            ILogger<TenantWorkflowService> logger)
        {
            _lifecycleService = lifecycleService;
            _operationService = operationService;
            _firmaRepo = firmaRepo;
            _maliDonemRepo = maliDonemRepo;
            _maliDonemDbRepo = maliDonemDbRepo;
            _applicationPaths = applicationPaths;
            _unitOfWork = unitOfWork;
            _logService = logService;
            _logger = logger;
        }

        public async Task<ApiDataResponse<TenantCreationResult>> CreateNewTenantAsync(TenantCreationRequest request)
        {
            _logger.LogInformation(
                "Starting tenant creation workflow for FirmaId: {FirmaId}, MaliYil: {MaliYil}",
                request.FirmaId,
                request.MaliYil);

            var result = new TenantCreationResult();

            try
            {
                // 1. Firma kontrol
                var firma = await _firmaRepo.GetByFirmaId(request.FirmaId);
                if (firma == null)
                {
                    return new ErrorApiDataResponse<TenantCreationResult>(null, "Firma bulunamadı");
                }

                // 2. Aynı mali dönem var mı kontrol
                var existingDonem = await _maliDonemRepo.FindAsync(
                    md => md.FirmaId == request.FirmaId && md.MaliYil == request.MaliYil);

                if (existingDonem != null)
                {
                    return new ErrorApiDataResponse<TenantCreationResult>(
                        null,
                        $"Bu firma için {request.MaliYil} mali dönemi zaten mevcut");
                }

                // 3. Database adını oluştur
                var dbNameResponse = _lifecycleService.GenerateDatabaseNameAsync(firma.FirmaKodu, request.MaliYil);

                if (!dbNameResponse.Success)
                {
                    return new ErrorApiDataResponse<TenantCreationResult>(null, dbNameResponse.Message);
                }
                result.DatabaseName = dbNameResponse.Data;

                // transaction başlat
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                // 4. MaliDonem kaydı oluştur (SQLite)
                var maliDonem = new MaliDonem
                {
                    FirmaId = request.FirmaId,
                    MaliYil = request.MaliYil,
                    DbOlusturulduMu = false,
                    AktifMi = true
                };

                await _maliDonemRepo.UpdateMaliDonemAsync(maliDonem);

                result.MaliDonemId = maliDonem.Id;

                // 5. SQL Server Database oluştur
                if (request.AutoCreateDatabase)
                {
                    var dbCreateResponse = await _lifecycleService.CreateDatabaseAsync(result.DatabaseName);
                    if (!dbCreateResponse.Success)
                    {
                        // Rollback: MaliDonem kaydını sil
                        await transaction.RollbackAsync();

                        return new ErrorApiDataResponse<TenantCreationResult>(
                            result,
                            $"Veritabanı oluşturulamadı: {dbCreateResponse.Message}");
                    }

                    result.DatabaseCreated = true;
                }

                // 6. MaliDonemDb kaydı oluştur (SQLite)
                var databasePath = Path.Combine(_applicationPaths.GetDatabasePath(), "Muhasebe", result.DatabaseName);

                var maliDonemDb = new MaliDonemDb
                {
                    MaliDonemId = maliDonem.Id,
                    DBName = result.DatabaseName,
                    Directory = Path.GetDirectoryName(databasePath),
                    DBPath = databasePath,
                    DatabaseType = DatabaseType.SqlServer,
                    AktifMi = true
                };

                await _maliDonemDbRepo.UpdateMaliDonemDbAsync(maliDonemDb);

                result.MaliDonemDbId = maliDonemDb.Id;

                // 7. Migration çalıştır
                if (request.RunMigrations && result.DatabaseCreated)
                {
                    var migrationResponse = await _operationService.RunMigrationsAsync(maliDonem.Id);
                    if (!migrationResponse.Success)
                    {
                        // Rollback: MaliDonem ve MaliDonemDb kayıtlarını sil
                        await transaction.RollbackAsync();
                        await _lifecycleService.DeleteDatabaseAsync(result.DatabaseName); // Eğer Migration hata verirse sil.
                        return new ErrorApiDataResponse<TenantCreationResult>(
                            result,
                            $"Migration işlemi başarısız: {migrationResponse.Message}");
                    }
                    result.MigrationsRun = migrationResponse.Success;
                }

                // 8. MaliDonem'i güncelle (DB oluşturuldu flag)
                maliDonem.DbOlusturulduMu = result.DatabaseCreated;
                await _maliDonemRepo.UpdateMaliDonemAsync(maliDonem);
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                // 9. İsteğe bağlı backup
                if (request.CreateInitialBackup && result.MigrationsRun)
                {
                    await _operationService.CreateBackupAsync(maliDonem.Id);
                }

                // transaction işlemi bitti 
                result.Message = "Tenant başarıyla oluşturuldu";

                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(TenantWorkflowService),
                        nameof(CreateNewTenantAsync),
                        $"Yeni tenant oluşturuldu. Firma: {firma.FirmaKodu}, Yıl: {request.MaliYil}, DB: {result.DatabaseName}",
string.Empty);

                return new SuccessApiDataResponse<TenantCreationResult>(result, "Tenant başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Tenant creation workflow failed for FirmaId: {FirmaId}, MaliYil: {MaliYil}",
                    request.FirmaId,
                    request.MaliYil);

                await _logService.SistemLogService
                    .SistemLogException(nameof(TenantWorkflowService), nameof(CreateNewTenantAsync), ex);

                return new ErrorApiDataResponse<TenantCreationResult>(result, $"Tenant oluşturma hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<bool>> DeleteTenantCompleteAsync(long maliDonemId)
        {
            _logger.LogInformation("Starting complete tenant deletion for MaliDonemId: {MaliDonemId}", maliDonemId);

            try
            {
                // 1. MaliDonemDb bilgisini al
                var maliDonemDb = await _maliDonemDbRepo.GetByMaliDonemDbIdAsync(maliDonemId);
                if (maliDonemDb == null)
                {
                    return new ErrorApiDataResponse<bool>(false, "Mali dönem veritabanı kaydı bulunamadı");
                }
                var databaseName = maliDonemDb.DBName;

                // 2. SQL Server database'i sil
                var dbDeleteResponse = await _lifecycleService.DeleteDatabaseAsync(databaseName);
                if (!dbDeleteResponse.Success)
                {
                    _logger.LogWarning("Database deletion failed but continuing: {DatabaseName}", databaseName);
                }
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                // 3. MaliDonemDb kaydını sil (SQLite)
                await _maliDonemDbRepo.DeleteAsync(maliDonemDb);


                // 4. MaliDonem kaydını sil (SQLite) - Cascade delete olmalı
                var maliDonem = await _maliDonemRepo.GetByMaliDonemId(maliDonemId);
                if (maliDonem != null)
                {
                    await _maliDonemRepo.DeleteAsync(maliDonem);
                }

                await _unitOfWork.CommitAsync();
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
