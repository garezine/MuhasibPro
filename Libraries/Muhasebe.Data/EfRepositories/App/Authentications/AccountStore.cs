using Muhasebe.Data.Abstract.Sistem.Authentication;
using Muhasebe.Domain.Entities.SistemDb;

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
