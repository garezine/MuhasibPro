using Muhasebe.Business.Models.DbModel.AppModel;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Utilities.Responses;

namespace Muhasebe.Business.Services.Abstract.App
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
