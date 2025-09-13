using Muhasebe.Domain.Entities.SistemEntity;

namespace Muhasebe.Data.Abstracts.Sistem.Authentication
{
    public interface IAccountStore
    {
        Kullanici CurrentAccount { get; set; }

        event Action StateChanged;
    }
}
