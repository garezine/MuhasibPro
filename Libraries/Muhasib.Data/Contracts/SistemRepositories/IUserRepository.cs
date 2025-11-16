using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Contracts.SistemRepositories
{
    public interface IUserRepository : IRepository<Kullanici>
    {
        Task<Kullanici?> GetByEmailAsync(string email);
        Task<Kullanici?> GetByUsernameAsync(string userName);
    }
}
