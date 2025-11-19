using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.BaseServices;
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

        public AuthenticationService(IAuthenticator authenticator, IBitmapToolsService bitmapTools)
        {
            _authenticator = authenticator;
            _bitmapTools = bitmapTools;
        }

        public KullaniciModel CurrentAccount
        {
            get { return CreateKullaniciModel(_authenticator.CurrentAccount, true); }
            private set
            {
                var currentAccut = CreateKullaniciModel(_authenticator.CurrentAccount, true);
                currentAccut = value;
                StateChanged?.Invoke();
            }
        }

        public event Action StateChanged;

        public bool IsLoggedIn 
        { 
            get => CurrentAccount != null;
            set
            {
               
            }
        } 

        public string CurrentUsername => CurrentAccount?.KullaniciAdi ?? "App";

        public long CurrentUserId => CurrentAccount?.Id ?? -1;

        public async Task Login(string username, string password)
        {
            CurrentAccount = CreateKullaniciModel(await _authenticator.Login(username, password).ConfigureAwait(false), includeAllFields: true);
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
                    AktifMi = source.AktifMi
                };
                if (includeAllFields)
                {
                    model.Resim = source.Resim;
                    model.ResimSource = _bitmapTools.CreateLazyImageLoader(model.Resim);
                }
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı oluşturulurken hata oluştu", ex);
            }
        }
    }
}
