using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Data.Abstracts.Sistem.Authentication;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Business.Services.Concrete.Common
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticator _authenticator;
        private readonly IBitmapTools _bitmapTools;

        public AuthenticationService(IAuthenticator authenticator, IBitmapTools bitmapTools)
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

        public bool IsLoggedIn => CurrentAccount != null;

        public string CurrentUsername => CurrentAccount?.KullaniciAdi;

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
        private Lazy<Task<object>> CreateLazyImageLoader(byte[] imageData)
        {
            return new Lazy<Task<object>>(async () =>
            {
                if (imageData == null || imageData.Length == 0)
                    return null;

                return await _bitmapTools.LoadBitmapAsync(imageData).ConfigureAwait(false);
            });
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
                    ResimOnizlemeSource = CreateLazyImageLoader(source.ResimOnizleme), //await _bitmapTools.LoadBitmapAsync(source.ResimOnizleme),
                    Resim = source.Resim,
                    KaydedenId = source.KaydedenId,
                    ResimSource = source.Resim,
                    AktifMi = source.AktifMi
                };
                if (includeAllFields)
                {
                    model.Resim = source.Resim;
                    model.ResimSource = CreateLazyImageLoader(model.Resim);
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
