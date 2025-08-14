using Muhasebe.Domain.Entities.Sistem;

namespace Muhasebe.Domain.Interfaces.App.IStore
{
    public interface IAccountStore
    {
        Kullanici CurrentAccount { get; set; }

        event Action StateChanged;
    }
}
