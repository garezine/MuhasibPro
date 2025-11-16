using Muhasib.Domain.Entities.SistemEntity;
using Muhasib.Domain.Models;

namespace Muhasib.Data.Contracts.SistemRepositories.Authentication
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
