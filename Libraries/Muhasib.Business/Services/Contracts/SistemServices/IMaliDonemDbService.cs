using Muhasib.Business.Models.SistemModel;
using Muhasib.Data.Common;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Business.Services.Contracts.SistemServices
{
    public interface IMaliDonemDbService
    {
        // CRUD - Sistem.db
        Task<ApiDataResponse<MaliDonemDbModel>> GetByMaliDonemDbIdAsync(long maliDonemId);
        Task<ApiDataResponse<IList<MaliDonemDbModel>>> GetMaliDonemDblerAsync(int skip, int take, DataRequest<MaliDonemDb> request);
        Task<ApiDataResponse<int>> UpdateMaliDonemDbAsync(MaliDonemDbModel model);
        Task<ApiDataResponse<int>> DeleteMaliDonemDbAsync(MaliDonemDbModel model);
        Task<int> GetMaliDonemDblerCountAsync(DataRequest<MaliDonemDb> request);


    }
}
