using Muhasebe.Business.Models.DbModel.AppModel;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Business.Services.Abstract.Common
{
    public interface IAuthenticationService
    {
        KullaniciModel CurrentAccount { get; }
        bool IsLoggedIn { get; }
        Task<RegistrationResult> Register(string email, string username, string password, string confirmPassword);
        Task Login(string username, string password);
        void Logout();
        string CurrentUsername { get; }
        long CurrentUserId { get; }
    }
}
