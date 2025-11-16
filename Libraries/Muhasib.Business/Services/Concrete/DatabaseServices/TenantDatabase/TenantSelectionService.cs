using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Models.TenantModel;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantSelectionService : ITenantSelectionService
    {
        private readonly IMaliDonemRepository _maliDonemRepo;
        private readonly IMaliDonemDbRepository _maliDonemDbRepo;
        private readonly IFirmaRepository _firmaRepo;
        private readonly ILogService _logService;
        private readonly ILogger<TenantSelectionService> _logger;

        public TenantSelectionService(
            IMaliDonemRepository maliDonemRepo,
            IMaliDonemDbRepository maliDonemDbRepo,
            IFirmaRepository firmaRepo,
            ILogService logService,
            ILogger<TenantSelectionService> logger)
        {
            _maliDonemRepo = maliDonemRepo;
            _maliDonemDbRepo = maliDonemDbRepo;
            _firmaRepo = firmaRepo;
            _logService = logService;
            _logger = logger;
        }

        public async Task<ApiDataResponse<List<TenantSelectionModel>>> GetTenantsForSelectionAsync(long? firmaId = null)
        {
            try
            {
                var query = await _maliDonemRepo.GetAllAsync();

                // Firma filtresi
                if (firmaId.HasValue)
                {
                    query = query.Where(md => md.FirmaId == firmaId.Value).ToList();
                }

                var models = new List<TenantSelectionModel>();

                foreach (var maliDonem in query)
                {
                    var firma = await _firmaRepo.GetByFirmaId(maliDonem.FirmaId);
                    var maliDonemDb = await _maliDonemDbRepo.GetByMaliDonemDbIdAsync(maliDonem.Id);

                    models.Add(new TenantSelectionModel
                    {
                        MaliDonemId = maliDonem.Id,
                        FirmaId = maliDonem.FirmaId,
                        FirmaKodu = firma?.FirmaKodu ?? string.Empty,
                        FirmaKisaUnvani = firma?.KisaUnvani ?? string.Empty,
                        MaliYil = maliDonem.MaliYil,
                        DatabaseName = maliDonemDb?.DBName ?? string.Empty,
                        DbOlusturulduMu = maliDonem.DbOlusturulduMu,
                        AktifMi = maliDonem.AktifMi
                    });
                }

                return new SuccessApiDataResponse<List<TenantSelectionModel>>(
                    models.OrderByDescending(m => m.MaliYil).ToList(),
                    "Tenant listesi başarıyla alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get tenants for selection failed");
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantSelectionService),
                    nameof(GetTenantsForSelectionAsync),
                    ex);

                return new ErrorApiDataResponse<List<TenantSelectionModel>>(
                    null,
                    $"Tenant listesi alınamadı: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<List<TenantSelectionModel>>> SearchTenantsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetTenantsForSelectionAsync();
                }

                var allTenants = await GetTenantsForSelectionAsync();
                if (!allTenants.Success)
                {
                    return allTenants;
                }

                var filtered = allTenants.Data
                    .Where(t =>
                        t.FirmaKodu.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.FirmaKisaUnvani.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.MaliYil.ToString().Contains(searchTerm))
                    .ToList();

                return new SuccessApiDataResponse<List<TenantSelectionModel>>(
                    filtered,
                    $"{filtered.Count} adet sonuç bulundu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search tenants failed: {SearchTerm}", searchTerm);
                return new ErrorApiDataResponse<List<TenantSelectionModel>>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<TenantDetailsModel>> GetTenantDetailsAsync(long maliDonemId)
        {
            try
            {
                var maliDonem = await _maliDonemRepo.GetByMaliDonemId(maliDonemId);
                if (maliDonem == null)
                {
                    return new ErrorApiDataResponse<TenantDetailsModel>(
                        null,
                        "Mali dönem bulunamadı");
                }

                var firma = await _firmaRepo.GetByFirmaId(maliDonem.FirmaId);
                var maliDonemDb = await _maliDonemDbRepo.GetByMaliDonemDbIdAsync(maliDonemId);

                var details = new TenantDetailsModel
                {
                    MaliDonemId = maliDonem.Id,
                    FirmaId = maliDonem.FirmaId,
                    FirmaKodu = firma?.FirmaKodu ?? string.Empty,
                    FirmaUnvani = firma?.TamUnvani ?? firma?.KisaUnvani ?? string.Empty,
                    MaliYil = maliDonem.MaliYil,
                    DatabaseName = maliDonemDb?.DBName ?? string.Empty,
                    DatabasePath = maliDonemDb?.DBPath ?? string.Empty,
                    DbOlusturulduMu = maliDonem.DbOlusturulduMu
                };

                return new SuccessApiDataResponse<TenantDetailsModel>(
                    details,
                    "Tenant detayları başarıyla alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get tenant details failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                return new ErrorApiDataResponse<TenantDetailsModel>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<List<TenantSelectionModel>>> GetMaliDonemlerByFirmaAsync(long firmaId)
        {
            try
            {
                return await GetTenantsForSelectionAsync(firmaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get mali donemler by firma failed for FirmaId: {FirmaId}", firmaId);
                return new ErrorApiDataResponse<List<TenantSelectionModel>>(null, ex.Message);
            }
        }
    }
}
