using Muhasib.Data.Contracts.SistemRepositories.Authentication;
using Muhasib.Domain.Entities.SistemEntity;
using Muhasib.Domain.Models;

namespace Muhasib.Data.Repositories.SistemRepositories.Authentications
{
    public class Authenticator : IAuthenticator
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IAccountStore _accountStore;

        public Authenticator(IAuthenticationRepository authenticationRepository, IAccountStore accountStore)
        {
            _authenticationRepository = authenticationRepository;
            _accountStore = accountStore;
        }

        public Kullanici CurrentAccount
        {
            get => _accountStore.CurrentAccount;
            private set
            {
                _accountStore.CurrentAccount = value;
                OnStateChanged();
            }
        }

        public bool IsLoggedIn => CurrentAccount != null;

        public event Action StateChanged;

        public async Task<Kullanici> Login(string username, string password)
        {
            var account = await _authenticationRepository.Login(username, password).ConfigureAwait(false);
            if (account != null)
            {
                CurrentAccount = account;
            }
            return account;
        }

        public void Logout() { CurrentAccount = null; }

        public async Task<RegistrationResult> Register(
            string email,
            string username,
            string password,
            string confirmPassword)
        {
            return await _authenticationRepository.Register(email, username, password, confirmPassword)
                .ConfigureAwait(false);
        }
        protected virtual void OnStateChanged() { Volatile.Read(ref StateChanged)?.Invoke(); }
    }
}
