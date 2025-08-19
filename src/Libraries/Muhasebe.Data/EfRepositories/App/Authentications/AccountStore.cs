using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Interfaces.App.IStore;

namespace Muhasebe.Data.EfRepositories.App.Authentications
{
    public class AccountStore : IAccountStore
    {
        private Kullanici _currentAccount;

        public Kullanici CurrentAccount
        {
            get { return _currentAccount; }
            set
            {
                _currentAccount = value;
                StateChanged?.Invoke();
            }
        }

        public event Action StateChanged;
    }
}
