using Muhasib.Business.Models.SistemModel;
using Muhasib.Domain.Models;

namespace Muhasib.Business.Services.Contracts.BaseServices
{
    public interface IAuthenticationService
    {
        KullaniciModel CurrentAccount { get; }
        bool IsAuthenticated { get; }
        Task<RegistrationResult> Register(string email, string username, string password, string confirmPassword);
        Task Login(string username, string password);
        void Logout();
        string CurrentUsername { get; }
        long CurrentUserId { get; }
        event Action StateChanged;
    }
}
