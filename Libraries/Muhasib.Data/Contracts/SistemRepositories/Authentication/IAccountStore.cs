using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Contracts.SistemRepositories.Authentication
{
    public interface IAccountStore
    {
        Kullanici CurrentAccount { get; set; }

        event Action StateChanged;
    }
}
