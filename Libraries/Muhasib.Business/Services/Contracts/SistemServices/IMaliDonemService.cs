using Muhasib.Business.Models.SistemModel;
using Muhasib.Data.Common;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Business.Services.Contracts.SistemServices
{
    public interface IMaliDonemService
    {

        Task<ApiDataResponse<MaliDonemModel>> GetByMaliDonemIdAsync(long id);       
        Task<ApiDataResponse<IList<MaliDonemModel>>> GetMaliDonemlerAsync(int skip, int take, DataRequest<MaliDonem> request);
        Task<ApiDataResponse<IList<MaliDonemModel>>> GetMaliDonemlerAsync(DataRequest<MaliDonem> request);
        Task<ApiDataResponse<int>> UpdateMaliDonemAsync(MaliDonemModel model);
        Task<ApiDataResponse<int>> DeleteMaliDonemAsync(MaliDonemModel model);
        Task<ApiDataResponse<int>> DeleteMaliDonemRangeAsync(int index, int length, DataRequest<MaliDonem> request);
        Task<int> GetMaliDonemlerCountAsync(DataRequest<MaliDonem> request);
        Task<bool> IsMaliDonem(long firmaId,int maliYil);
        Task<ApiDataResponse<MaliDonemModel>> CreateNewMaliDonemAsync(long firmaId);
       

    }
}
