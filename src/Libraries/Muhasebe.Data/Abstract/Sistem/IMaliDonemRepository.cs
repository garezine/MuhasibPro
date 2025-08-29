using Muhasebe.Data.Abstract.Common;
using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Data.Abstract.Sistem
{
    public interface IMaliDonemRepository : IGenericRepository<MaliDonem>
    {
        Task<bool> GetIsCalismaDonem(long firmaId);
        Task<MaliDonem> GetByCalismaDonemWithFirmaId(long firmaId);
        Task<MaliDonem> GetByCalismaDonemId(long firmaId, long donemId);
        Task<IList<Firma>> GetByListCalismaDonemlerWithFirmaId(long firmaId);
    }
}
