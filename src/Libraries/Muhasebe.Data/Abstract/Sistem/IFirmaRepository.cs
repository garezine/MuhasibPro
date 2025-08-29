using Muhasebe.Data.Abstract.Common;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Data.Abstract.Sistem
{
    public interface IFirmaRepository : IGenericRepository<Firma>
    {
        Task<Firma> GetByFirmaId(long id);
        Task<IList<Firma>> GetFirmaKeysAsync(int skip, int take, DataRequest<Firma> request);
        Task<IList<Firma>> GetFirmalarAsync(int skip, int take, DataRequest<Firma> request);

        Task<int> GetFirmalarCountAsync(DataRequest<Firma> request);
        Task DeleteFirmalarAsync(params Firma[] firmalar);
        Task UpdateFirmaAsync(Firma firma);
        Task<bool> IsFirma();
    }
}
