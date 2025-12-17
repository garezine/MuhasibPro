using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Business.Services.Contracts.UtilityServices;
using Muhasib.Data.Contracts.SistemRepositories.Authentication;
using Muhasib.Domain.Entities.SistemEntity;
using Muhasib.Domain.Models;

namespace Muhasib.Business.Services.Concrete.BaseServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticator _authenticator;
        private readonly IBitmapToolsService _bitmapTools;
        private readonly IMessageService _messageService;
        private KullaniciModel _currentAccount;

        public AuthenticationService(
            IAuthenticator authenticator,
            IBitmapToolsService bitmapTools,
            IMessageService messageService)
        {
            _authenticator = authenticator;
            _bitmapTools = bitmapTools;
            _messageService = messageService;
        }

        public KullaniciModel CurrentAccount
        {
            get => _currentAccount;
            private set
            {
                if(_currentAccount != value)
                {
                    _currentAccount = value;
                    StateChanged?.Invoke();
                    _messageService.Send(this, "AuthenticationChanged", IsAuthenticated);
                }
            }
        }

        public event Action StateChanged;

        public bool IsAuthenticated => CurrentAccount != null;


        public string CurrentUsername => CurrentAccount?.KullaniciAdi ?? "App";

        public long CurrentUserId => CurrentAccount?.Id ?? -1;

        public async Task Login(string username, string password)
        {
            var kullanici = await _authenticator.Login(username, password);
            CurrentAccount = CreateKullaniciModel(kullanici, includeAllFields: true);
        }

        public void Logout()
        {
            _authenticator.Logout();
            CurrentAccount = null;
        }

        public async Task<RegistrationResult> Register(
            string email,
            string username,
            string password,
            string confirmPassword)
        {
            try
            {
                return await _authenticator.Register(email, username, password, confirmPassword).ConfigureAwait(false);
            } catch(Exception ex)
            {
                throw new Exception("Kullanıcı kaydedilmedi", ex);
            }
        }

        private KullaniciModel CreateKullaniciModel(Kullanici source, bool includeAllFields)
        {
            try
            {
                var model = new KullaniciModel()
                {
                    Id = source.Id,
                    Adi = source.Adi,
                    Soyadi = source.Soyadi,
                    Eposta = source.Eposta,
                    KullaniciAdi = source.KullaniciAdi,
                    KayitTarihi = source.KayitTarihi,
                    GuncellemeTarihi = source.GuncellemeTarihi,
                    RolId = source.RolId,
                    GuncelleyenId = source.GuncelleyenId,
                    Telefon = source.Telefon,
                    ResimOnizleme = source.Resim,
                    ResimOnizlemeSource = _bitmapTools.CreateLazyImageLoader(source.ResimOnizleme), //await _bitmapTools.LoadBitmapAsync(source.ResimOnizleme),
                    Resim = source.Resim,
                    KaydedenId = source.KaydedenId,
                    ResimSource = source.Resim,
                    AktifMi = source.AktifMi,
                    Rol = CreateKullaniciRol(source.Rol),
                };
                if(includeAllFields)
                {                    
                    model.Resim = source.Resim;
                    model.ResimSource = _bitmapTools.CreateLazyImageLoader(model.Resim);
                }
                return model;
            } catch(Exception ex)
            {
                throw new Exception("Kullanıcı oluşturulurken hata oluştu", ex);
            }
        }

        private KullaniciRolModel CreateKullaniciRol(KullaniciRol source)
        {
            try
            {
                var model = new KullaniciRolModel
                {
                    Aciklama = source.Aciklama,
                    RolAdi = source.RolAdi,
                    //Base Entity
                    Id = source.Id,
                    AktifMi = source.AktifMi,
                    GuncellemeTarihi = source.GuncellemeTarihi,
                    GuncelleyenId = source.GuncelleyenId,
                    KaydedenId = source.KaydedenId,
                    KayitTarihi = source.KayitTarihi,
                };
                return model;
            } catch(Exception)
            {
                throw;
            }
        }
    }
}
