using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Models.TenantModel;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Business.Services.Contracts.SistemServices;
using Muhasib.Data.Common;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantSQLiteInfoService : ITenantSQLiteInfoService
    {
        private readonly IMaliDonemService _maliDonemService;
        private readonly ILogService _logService;
        private readonly ILogger<TenantSQLiteInfoService> _logger;

        public TenantSQLiteInfoService(
            IMaliDonemService maliDonemService,
            ILogService logService,
            ILogger<TenantSQLiteInfoService> logger)
        {
            _maliDonemService = maliDonemService;
            _logService = logService;
            _logger = logger;
        }


        public async Task<ApiDataResponse<TenantDetailsModel>> GetTenantDetailsAsync(long maliDonemId)
        {
            try
            {
                var maliDonem = await _maliDonemService.GetByMaliDonemIdAsync(maliDonemId);
                if(maliDonem == null)
                {
                    return new ErrorApiDataResponse<TenantDetailsModel>(null, "Mali dönem bulunamadı");
                }
                var details = new TenantDetailsModel
                {
                    MaliDonemId = maliDonem.Data.Id,
                    FirmaId = maliDonem.Data.FirmaId,
                    FirmaKodu = maliDonem.Data.FirmaModel?.FirmaKodu ?? string.Empty,
                    FirmaUnvani =
                        maliDonem.Data.FirmaModel?.TamUnvani ?? maliDonem.Data.FirmaModel?.KisaUnvani ?? string.Empty,
                    MaliYil = maliDonem.Data.MaliYil,
                    DatabaseName = maliDonem.Data?.DBName ?? string.Empty,
                    DatabasePath = maliDonem.Data?.DBPath ?? string.Empty,
                    Directory = maliDonem.Data?.Directory ?? string.Empty,
                    
                };

                return new SuccessApiDataResponse<TenantDetailsModel>(details, "Tenant detayları başarıyla alındı");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Get tenant details failed for maliDonemId: {maliDonemId}", maliDonemId);
                return new ErrorApiDataResponse<TenantDetailsModel>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<List<TenantSelectionModel>>> GetTenantsForSelectionAsync(
            long? firmaId = null,
            DataRequest<MaliDonem> request = null)
        {
            try
            {
                var allMaliDonem = await _maliDonemService.GetMaliDonemlerAsync(request);
                var query = allMaliDonem.Data.ToList();
                // Firma filtresi
                if(firmaId.HasValue)
                {
                    query = query.Where(md => md.FirmaId == firmaId.Value).ToList();
                }

                var models = new List<TenantSelectionModel>();

                foreach(var maliDonem in query)
                {
                    models.Add(
                        new TenantSelectionModel
                        {
                            MaliDonemId = maliDonem.Id,
                            FirmaId = maliDonem.FirmaId,
                            FirmaKodu = maliDonem.FirmaModel?.FirmaKodu ?? string.Empty,
                            FirmaKisaUnvani = maliDonem.FirmaModel?.KisaUnvani ?? string.Empty,
                            MaliYil = maliDonem.MaliYil,
                            DatabaseName = maliDonem?.DBName ?? string.Empty,
                            AktifMi = maliDonem.AktifMi
                        });
                }

                return new SuccessApiDataResponse<List<TenantSelectionModel>>(
                    models.OrderByDescending(m => m.MaliYil).ToList(),
                    "Tenant listesi başarıyla alındı");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Get tenants for selection failed");
                await _logService.SistemLogService
                    .SistemLogException(nameof(TenantSQLiteInfoService), nameof(GetTenantsForSelectionAsync), ex);

                return new ErrorApiDataResponse<List<TenantSelectionModel>>(
                    null,
                    $"Tenant listesi alınamadı: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<List<TenantSelectionModel>>> SearchTenantsAsync(string searchTerm)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetTenantsForSelectionAsync();
                }

                var allTenants = await GetTenantsForSelectionAsync();
                if(!allTenants.Success)
                {
                    return allTenants;
                }

                var filtered = allTenants.Data
                    .Where(
                        t => t.FirmaKodu.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            t.FirmaKisaUnvani.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            t.MaliYil.ToString().Contains(searchTerm))
                    .ToList();

                return new SuccessApiDataResponse<List<TenantSelectionModel>>(
                    filtered,
                    $"{filtered.Count} adet sonuç bulundu");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Search tenants failed: {SearchTerm}", searchTerm);
                return new ErrorApiDataResponse<List<TenantSelectionModel>>(null, ex.Message);
            }
        }
    }
}
