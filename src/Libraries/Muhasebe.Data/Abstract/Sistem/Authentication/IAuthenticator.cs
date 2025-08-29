using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Abstract.Sistem.Authentication
{
    public interface IAuthenticator
    {
        Kullanici CurrentAccount { get; }
        bool IsLoggedIn { get; }

        event Action StateChanged;    
        void Logout();
        Task<Kullanici> Login(string username, string password);
        Task<RegistrationResult> Register(
            string email,
            string username,
            string password,
            string confirmPassword);
    }
}
