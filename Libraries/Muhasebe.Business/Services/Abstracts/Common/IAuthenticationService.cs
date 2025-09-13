using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Business.Services.Abstracts.Common
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
