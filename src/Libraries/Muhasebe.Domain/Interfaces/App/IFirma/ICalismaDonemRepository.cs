using Muhasebe.Domain.Entities.Sistem;
using Muhasebe.Domain.Interfaces.Database;

namespace Muhasebe.Domain.Interfaces.App.IFirma
{
    public interface ICalismaDonemRepository : IGenericRepository<CalismaDonem>
    {
        Task<bool> GetIsCalismaDonem(long firmaId);
        Task<CalismaDonem> GetByCalismaDonemWithFirmaId(long firmaId);
        Task<CalismaDonem> GetByCalismaDonemId(long firmaId, long donemId);
        Task<IList<Entities.Sistem.Firma>> GetByListCalismaDonemlerWithFirmaId(long firmaId);
    }
}
