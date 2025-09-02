using Muhasebe.Data.Abstract.Common;
using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Data.Abstract.Sistem.Authentication
{
    public interface IUserRepository : IGenericRepository<Kullanici>
    {
        Task<Kullanici> GetByUsernameAsync(string userName);
        Task<Kullanici> GetByEmailAsync(string email);
    }
}
