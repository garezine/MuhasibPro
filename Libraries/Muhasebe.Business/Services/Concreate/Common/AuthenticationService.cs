using AutoMapper;
using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Data.Abstract.Sistem.Authentication;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Business.Services.Concreate.Common
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticator _authenticator;
        private readonly IMapper _mapper;

        public AuthenticationService(IAuthenticator authenticator, IMapper mapper)
        {
            _authenticator = authenticator;
            _mapper = mapper;
        }

        public KullaniciModel CurrentAccount
        {
            get { return _mapper.Map<KullaniciModel>(_authenticator.CurrentAccount); }
            private set
            {
                var currentAccut = _mapper.Map<KullaniciModel>(_authenticator.CurrentAccount);
                currentAccut = value;
                StateChanged?.Invoke();
            }
        }

        public event Action StateChanged;

        public bool IsLoggedIn => CurrentAccount != null;

        public string CurrentUsername => CurrentAccount?.KullaniciAdi;

        public long CurrentUserId => CurrentAccount?.Id ?? -1;

        public async Task Login(string username, string password)
        {
            CurrentAccount = _mapper.Map<KullaniciModel>(await _authenticator.Login(username, password).ConfigureAwait(false));
        }

        public void Logout() => _authenticator.Logout();

        public async Task<RegistrationResult> Register(
            string email,
            string username,
            string password,
            string confirmPassword)
        {
            try
            {
                return await _authenticator.Register(email, username, password, confirmPassword).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı kaydedilmedi", ex);
            }
        }
    }
}
