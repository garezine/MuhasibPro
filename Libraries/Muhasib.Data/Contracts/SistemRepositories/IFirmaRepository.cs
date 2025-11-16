using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Contracts.SistemRepositories
{
    public interface IFirmaRepository : IRepository<Firma>
    {
        Task<Firma> GetByFirmaId(long id);
        Task<IList<Firma>> GetFirmaKeysAsync(int skip, int take, DataRequest<Firma> request);
        Task<IList<Firma>> GetFirmalarAsync(int skip, int take, DataRequest<Firma> request);

        Task<int> GetFirmalarCountAsync(DataRequest<Firma> request);
        Task DeleteFirmalarAsync(params Firma[] firmalar);
        Task UpdateFirmaAsync(Firma firma);
        Task<bool> IsFirma();
        Task<string> GetYeniFirmaKodu(string customCode = null);
    }
}
