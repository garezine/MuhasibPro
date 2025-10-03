using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;
using Muhasebe.Domain.Helpers.Responses;

namespace MuhasibPro.Contracts.SistemServices
{
    public interface IFirmaService
    {
        Task<bool> IsFirma();
        Task<ApiDataResponse<FirmaModel>> GetFirmaAsync(long id);
        Task<ApiDataResponse<FirmaModel>> GetByFirmaIdAsync(long id);
        Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarAsync(int skip, int take, DataRequest<Firma> request);
        Task<IList<FirmaModel>> GetFirmalarAsync(DataRequest<Firma> request);
        Task<int> GetFirmalarCountAsync(DataRequest<Firma> request);

        Task<ApiDataResponse<int>> UpdateFirmaAsync(FirmaModel model);
        Task<ApiDataResponse<int>> DeleteFirmaAsync(FirmaModel model);
        Task<ApiDataResponse<int>> DeleteFirmaRangeAsync(int index, int length, DataRequest<Firma> request);
    }
}
