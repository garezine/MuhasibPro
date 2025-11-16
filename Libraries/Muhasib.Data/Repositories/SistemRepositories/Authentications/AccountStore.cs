using Muhasib.Data.Contracts.SistemRepositories.Authentication;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Repositories.SistemRepositories.Authentications
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
