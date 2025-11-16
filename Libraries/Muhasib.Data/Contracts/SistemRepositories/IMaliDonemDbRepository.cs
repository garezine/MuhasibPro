using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Contracts.SistemRepositories
{
    public interface IMaliDonemDbRepository : IRepository<MaliDonemDb>
    {
        Task<MaliDonemDb> GetByMaliDonemDbIdAsync(long maliDonemId);
        Task<IList<MaliDonemDb>> GetMaliDonemDblerAsync(int skip, int take, DataRequest<MaliDonemDb> request);
        Task<IList<MaliDonemDb>> GetMaliDonemDbKeysAsync(int skip, int take, DataRequest<MaliDonemDb> request);
        Task<int> GetMaliDonemDblerCountAsync(DataRequest<MaliDonemDb> request);
        Task UpdateMaliDonemDbAsync(MaliDonemDb maliDonemDb);
        Task DeleteMaliDonemDbAsync(params MaliDonemDb[] maliDonemDbler);
        Task<bool> IsMaliDonemDb();
        MaliDonemDb GetByMaliDonemDbId(long maliDonemId);
    }
}
