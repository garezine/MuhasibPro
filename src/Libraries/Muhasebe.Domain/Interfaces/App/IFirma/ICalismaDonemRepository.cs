using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Interfaces.Database;

namespace Muhasebe.Domain.Interfaces.App.IFirma
{
    public interface ICalismaDonemRepository : IGenericRepository<MaliDonem>
    {
        Task<bool> GetIsCalismaDonem(long firmaId);
        Task<MaliDonem> GetByCalismaDonemWithFirmaId(long firmaId);
        Task<MaliDonem> GetByCalismaDonemId(long firmaId, long donemId);
        Task<IList<Firma>> GetByListCalismaDonemlerWithFirmaId(long firmaId);
    }
}
