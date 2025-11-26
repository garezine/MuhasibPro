using Muhasib.Business.Models.SistemModel;
using Muhasib.Data.Common;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Business.Services.Contracts.SistemServices
{
    public interface IFirmaService
    {
        Task<string> GetYeniFirmaKodu();
        Task<bool> IsFirma();

        Task<ApiDataResponse<FirmaModel>> GetByFirmaIdAsync(long id);
        Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarAsync(int skip, int take, DataRequest<Firma> request);
        Task<int> GetFirmalarCountAsync(DataRequest<Firma> request);
        Task<ApiDataResponse<int>> UpdateFirmaAsync(FirmaModel model);
        Task<ApiDataResponse<int>> DeleteFirmaAsync(FirmaModel model);
        Task<ApiDataResponse<int>> DeleteFirmaRangeAsync(int index, int length, DataRequest<Firma> request);


    }
}
