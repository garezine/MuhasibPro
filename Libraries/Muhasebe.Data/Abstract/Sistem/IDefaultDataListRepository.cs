using Muhasebe.Domain.Entities.Uygulama;

namespace Muhasebe.Domain.Interfaces.App
{
    public interface IDefaultDataListRepository
    {
        Task<IList<Iller>> GetIllerAsync();
    }
}
