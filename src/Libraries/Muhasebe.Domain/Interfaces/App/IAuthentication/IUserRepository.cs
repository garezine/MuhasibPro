using Muhasebe.Domain.Entities.Sistem;
using Muhasebe.Domain.Interfaces.Database;

namespace Muhasebe.Domain.Interfaces.App.IAuthentication
{
    public interface IUserRepository : IGenericRepository<Kullanici>
    {
        Task<Kullanici> GetByUsernameAsync(string userName);
        Task<Kullanici> GetByEmailAsync(string email);
    }
}
