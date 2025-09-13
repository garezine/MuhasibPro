using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;

namespace Muhasebe.Data.Abstracts.Sistem
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
