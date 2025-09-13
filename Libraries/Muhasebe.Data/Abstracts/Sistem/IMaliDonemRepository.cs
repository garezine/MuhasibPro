using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Domain.Entities.SistemEntity;

namespace Muhasebe.Data.Abstracts.Sistem
{
    public interface IMaliDonemRepository : IGenericRepository<MaliDonem>
    {
        Task<bool> GetIsCalismaDonem(long firmaId);
        Task<MaliDonem> GetByCalismaDonemWithFirmaId(long firmaId);
        Task<MaliDonem> GetByCalismaDonemId(long firmaId, long donemId);
        Task<IList<Firma>> GetByListCalismaDonemlerWithFirmaId(long firmaId);
    }
}
