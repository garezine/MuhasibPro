using Muhasebe.Data.Abstracts.Sistem.Authentication;
using Muhasebe.Domain.Entities.SistemEntity;

namespace Muhasebe.Data.Concrete.Authentications
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
