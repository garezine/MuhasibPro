using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Data.Abstract.Sistem.Authentication
{
    public interface IAccountStore
    {
        Kullanici CurrentAccount { get; set; }

        event Action StateChanged;
    }
}
