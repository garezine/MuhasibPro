using Muhasebe.Domain.Entities.Uygulama;

namespace Muhasebe.Domain.Interfaces.App
{
    public interface IEntityTablesListRepository
    {
        Task<IList<Iller>> GetIllerAsync();
    }
}
